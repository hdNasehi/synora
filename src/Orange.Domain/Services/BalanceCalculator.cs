using Orange.Domain.Balances;
using Orange.Domain.Common;
using Orange.Domain.Employees;
using Orange.Domain.Leave;

namespace Orange.Domain.Services;

public sealed class BalanceCalculator : IBalanceCalculator
{
    public LeaveBalance Calculate(
        Employee employee,
        LeavePolicy policy,
        IReadOnlyList<LeaveRequest> requests,
        IReadOnlyList<BalanceAdjustment> adjustments,
        int year,
        DateOnly today)
    {
        var annualRequests = requests
            .Where(r => r.Type == LeaveType.Annual && r.StartDate.Year == year)
            .ToList();

        var approved = annualRequests
            .Where(r => r.Status == RequestStatus.Approved)
            .Sum(r => r.Amount);

        var taken = annualRequests
            .Where(r => r.Status == RequestStatus.Approved && r.EndDate < today)
            .Sum(r => r.Amount);

        var pending = annualRequests
            .Where(r => r.Status is RequestStatus.PendingSupervisor or RequestStatus.PendingManager)
            .Sum(r => r.Amount);

        var yearAdjustments = adjustments
            .Where(a => a.EmployeeId == employee.Id && a.Year == year)
            .ToList();

        var carryOver = yearAdjustments
            .Where(a => a.Kind == AdjustmentKind.CarryOver)
            .Sum(a => a.Amount);

        var adj = yearAdjustments
            .Where(a => a.Kind == AdjustmentKind.Adjustment)
            .Sum(a => a.Amount);

        var entitlement = Math.Round(policy.Entitlement * employee.Fte * 100) / 100;
        // TODO(rule-5): pro-rate for joiners/leavers
        var monthsElapsed = Math.Min(today.Month, 12);
        var accrued = Math.Round(entitlement * monthsElapsed / 12 * 10) / 10;

        var total = entitlement + carryOver + adj;
        var available = Math.Round((total - approved - pending) * 100) / 100;

        return new LeaveBalance(
            policy.Unit,
            entitlement,
            accrued,
            carryOver,
            adj,
            approved,
            taken,
            approved - taken,
            pending,
            available);
    }
}
