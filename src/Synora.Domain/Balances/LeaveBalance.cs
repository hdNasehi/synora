using Synora.Domain.Common;

namespace Synora.Domain.Balances;

public sealed record LeaveBalance(
    BalanceUnit Unit,
    decimal Entitlement,
    decimal Accrued,
    decimal CarryOver,
    decimal Adjustments,
    decimal Approved,
    decimal Taken,
    decimal Scheduled,
    decimal Pending,
    decimal Available);
