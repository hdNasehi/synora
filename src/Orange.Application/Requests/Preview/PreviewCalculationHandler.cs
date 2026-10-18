using Microsoft.EntityFrameworkCore;
using Orange.Application.Abstractions;
using Orange.Domain.Common;
using Orange.Domain.Leave;
using Orange.Domain.Services;

namespace Orange.Application.Requests.Preview;

public sealed class PreviewCalculationHandler
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IWorkingDayCalculator _calculator;

    public PreviewCalculationHandler(
        IAppDbContext db, ICurrentUser currentUser, IWorkingDayCalculator calculator)
    {
        _db = db;
        _currentUser = currentUser;
        _calculator = calculator;
    }

    public async Task<Result<PreviewCalculationResponse>> HandleAsync(
        PreviewCalculationRequest query, CancellationToken ct = default)
    {
        var empId = _currentUser.EmployeeId;

        var employee = await _db.Employees.FindAsync([empId], ct);
        if (employee is null) return new Error("NotFound", "Employee not found.");

        var schedAssignments = await _db.ScheduleAssignments
            .Where(a => a.EmployeeId == empId).ToListAsync(ct);
        var schedIds = schedAssignments.Select(a => a.ScheduleId).Distinct().ToList();
        var schedules = await _db.WorkSchedules.Where(s => schedIds.Contains(s.Id)).ToListAsync(ct);
        var holidays = await _db.Holidays.ToListAsync(ct);
        var observances = await _db.HolidayObservances
            .Where(o => o.GroupId == employee.HolidayGroupId).ToListAsync(ct);

        TimeRange? partial = query.PartialFrom.HasValue && query.PartialTo.HasValue
            ? new TimeRange(query.PartialFrom.Value, query.PartialTo.Value)
            : null;

        var calc = _calculator.Calculate(
            employee, query.StartDate, query.EndDate, partial,
            schedAssignments, schedules, holidays, observances);

        return new PreviewCalculationResponse(calc, calc.WorkingDays, calc.TotalHours);
    }
}
