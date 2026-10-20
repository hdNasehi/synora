using Microsoft.EntityFrameworkCore;
using Orange.Application.Abstractions;
using Orange.Domain.Services;

namespace Orange.Application.Balances.GetMyBalance;

public sealed class GetMyBalanceHandler
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IBalanceCalculator _calculator;
    private readonly TimeProvider _time;

    public GetMyBalanceHandler(
        IAppDbContext db,
        ICurrentUser currentUser,
        IBalanceCalculator calculator,
        TimeProvider time)
    {
        _db = db;
        _currentUser = currentUser;
        _calculator = calculator;
        _time = time;
    }

    public async Task<GetMyBalanceResponse?> HandleAsync(
        GetMyBalanceQuery query, CancellationToken ct = default)
    {
        var empId = _currentUser.EmployeeId;
        var today = DateOnly.FromDateTime(_time.GetLocalNow().DateTime);

        var employee = await _db.Employees.FirstOrDefaultAsync(e => e.Id == empId, ct);
        if (employee is null) return null;

        var policyAssignment = await _db.PolicyAssignments
            .Where(a => a.EmployeeId == empId
                && a.ValidFrom <= today
                && (a.ValidTo == null || a.ValidTo >= today))
            .OrderByDescending(a => a.ValidFrom)
            .FirstOrDefaultAsync(ct);
        if (policyAssignment is null) return null;

        var policy = await _db.LeavePolicies
            .FirstOrDefaultAsync(p => p.Id == policyAssignment.PolicyId, ct);
        if (policy is null) return null;

        var requests = await _db.LeaveRequests
            .Include(r => r.Steps)
            .Where(r => r.EmployeeId == empId)
            .AsNoTracking()
            .ToListAsync(ct);

        var adjustments = await _db.BalanceAdjustments
            .Where(a => a.EmployeeId == empId)
            .AsNoTracking()
            .ToListAsync(ct);

        var balance = _calculator.Calculate(
            employee, policy, requests, adjustments, query.Year, today);
        return new GetMyBalanceResponse(balance);
    }
}
