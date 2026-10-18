using Orange.Domain.Balances;
using Orange.Domain.Employees;
using Orange.Domain.Leave;

namespace Orange.Domain.Services;

public interface IBalanceCalculator
{
    LeaveBalance Calculate(
        Employee employee,
        LeavePolicy policy,
        IReadOnlyList<LeaveRequest> requests,
        IReadOnlyList<BalanceAdjustment> adjustments,
        int year,
        DateOnly today);
}
