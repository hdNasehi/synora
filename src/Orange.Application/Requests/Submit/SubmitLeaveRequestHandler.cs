using Microsoft.EntityFrameworkCore;
using Orange.Application.Abstractions;
using Orange.Domain.Balances;
using Orange.Domain.Common;
using Orange.Domain.Leave;
using Orange.Domain.Services;

namespace Orange.Application.Requests.Submit;

public sealed class SubmitLeaveRequestHandler
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IWorkingDayCalculator _calculator;
    private readonly IApprovalRouteBuilder _routeBuilder;
    private readonly IBalanceCalculator _balanceCalculator;
    private readonly IAuditLog _audit;
    private readonly IOutbox _outbox;
    private readonly TimeProvider _time;

    public SubmitLeaveRequestHandler(
        IAppDbContext db, ICurrentUser currentUser,
        IWorkingDayCalculator calculator, IApprovalRouteBuilder routeBuilder,
        IBalanceCalculator balanceCalculator,
        IAuditLog audit, IOutbox outbox, TimeProvider time)
    {
        _db = db;
        _currentUser = currentUser;
        _calculator = calculator;
        _routeBuilder = routeBuilder;
        _balanceCalculator = balanceCalculator;
        _audit = audit;
        _outbox = outbox;
        _time = time;
    }

    public async Task<Result<SubmitLeaveResponse>> HandleAsync(
        SubmitLeaveRequest command, CancellationToken ct = default)
    {
        var empId = _currentUser.EmployeeId;
        var today = DateOnly.FromDateTime(_time.GetUtcNow().DateTime);
        var now = _time.GetUtcNow();

        if (command.EndDate < command.StartDate)
            return DomainErrors.EndBeforeStart;

        if (command.EndDate.DayNumber - command.StartDate.DayNumber + 1 > 62)
            return DomainErrors.ExceedsMaxSpan;

        if (command.Type == LeaveType.Sick && command.StartDate > today)
            return DomainErrors.SickLeaveInFuture;

        var employee = await _db.Employees.FindAsync([empId], ct);
        if (employee is null) return new Error("NotFound", "Employee not found.");

        var overlap = await _db.LeaveRequests
            .Include(r => r.Steps)
            .Where(r => r.EmployeeId == empId &&
                        r.StartDate <= command.EndDate &&
                        r.EndDate >= command.StartDate)
            .ToListAsync(ct);

        if (overlap.Any(r => r.Status is not RequestStatus.Rejected and not RequestStatus.Cancelled))
            return DomainErrors.OverlapExists;

        var schedAssignments = await _db.ScheduleAssignments
            .Where(a => a.EmployeeId == empId).ToListAsync(ct);
        var schedIds = schedAssignments.Select(a => a.ScheduleId).Distinct().ToList();
        var schedules = await _db.WorkSchedules.Where(s => schedIds.Contains(s.Id)).ToListAsync(ct);
        var holidays = await _db.Holidays.ToListAsync(ct);
        var observances = await _db.HolidayObservances
            .Where(o => o.GroupId == employee.HolidayGroupId).ToListAsync(ct);

        TimeRange? partial = command.PartialFrom.HasValue && command.PartialTo.HasValue
            ? new TimeRange(command.PartialFrom.Value, command.PartialTo.Value)
            : null;

        var calculation = _calculator.Calculate(
            employee, command.StartDate, command.EndDate, partial,
            schedAssignments, schedules, holidays, observances);

        if (calculation.WorkingDays == 0)
            return DomainErrors.NoWorkingDays;

        var assignment = await _db.EmployeeAssignments
            .Where(a => a.EmployeeId == empId && a.ValidFrom <= today && (a.ValidTo == null || a.ValidTo >= today))
            .FirstOrDefaultAsync(ct);

        if (assignment is null) return new Error("NoAssignment", "No active assignment found.");

        var stepsResult = _routeBuilder.Build(assignment, command.Type);
        if (!stepsResult.IsSuccess) return stepsResult.Error!;

        var policyAssignment = await _db.PolicyAssignments
            .Where(a => a.EmployeeId == empId && a.ValidFrom <= today && (a.ValidTo == null || a.ValidTo >= today))
            .FirstOrDefaultAsync(ct);

        if (policyAssignment is not null && command.Type == LeaveType.Annual)
        {
            var policy = await _db.LeavePolicies.FindAsync([policyAssignment.PolicyId], ct);
            if (policy is not null)
            {
                if (command.StartDate.DayNumber - today.DayNumber < policy.MinNoticeDays && command.Type != LeaveType.Sick)
                    return DomainErrors.MinimumNoticeViolation;

                var yearRequests = await _db.LeaveRequests
                    .Include(r => r.Steps)
                    .Where(r => r.EmployeeId == empId && r.Type == LeaveType.Annual)
                    .ToListAsync(ct);
                var adjustments = await _db.BalanceAdjustments
                    .Where(a => a.EmployeeId == empId && a.Year == command.StartDate.Year)
                    .ToListAsync(ct);

                var balance = _balanceCalculator.Calculate(
                    employee, policy, yearRequests, adjustments, command.StartDate.Year, today);

                var amount = policy.Unit == BalanceUnit.Hours
                    ? calculation.TotalHours
                    : calculation.WorkingDays;

                if (amount > balance.Available)
                    return DomainErrors.InsufficientBalance;
            }
        }

        var unit = BalanceUnit.Days;
        decimal requestAmount = calculation.WorkingDays;
        if (policyAssignment is not null)
        {
            var pol = await _db.LeavePolicies.FindAsync([policyAssignment.PolicyId], ct);
            if (pol is not null)
            {
                unit = pol.Unit;
                requestAmount = pol.Unit == BalanceUnit.Hours ? calculation.TotalHours : calculation.WorkingDays;
            }
        }

        var request = LeaveRequest.Submit(
            empId, command.Type, command.StartDate, command.EndDate,
            partial, requestAmount, unit, command.Reason,
            calculation, stepsResult.Value!, now);

        foreach (var evt in request.DomainEvents)
            _outbox.Enqueue(evt);
        request.ClearDomainEvents();

        _db.LeaveRequests.Add(request);

        _audit.Record("Submitted leave request",
            new { RequestId = request.Id, command.Type, command.StartDate, command.EndDate },
            empId);

        await _db.SaveChangesAsync(ct);

        return new SubmitLeaveResponse(request.Id, request.Status, requestAmount, unit, calculation);
    }
}
