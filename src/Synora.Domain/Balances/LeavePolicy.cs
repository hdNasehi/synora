using Synora.Domain.Common;

namespace Synora.Domain.Balances;

public sealed class LeavePolicy : Entity<LeavePolicyId>
{
    public string Name { get; set; } = "";
    public BalanceUnit Unit { get; set; }
    public decimal Entitlement { get; set; }
    public decimal CarryOverMax { get; set; }
    public int MinNoticeDays { get; set; }
    public int MaxCalendarDays { get; set; } = 62;
}
