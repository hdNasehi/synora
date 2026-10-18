using Synora.Domain.Balances;
using Synora.Domain.Employees;
using Synora.Domain.Leave;

namespace Synora.Domain.Services;

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
