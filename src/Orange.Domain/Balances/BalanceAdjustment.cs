using Orange.Domain.Common;

namespace Orange.Domain.Balances;

public sealed class BalanceAdjustment
{
    public Guid Id { get; set; }
    public EmployeeId EmployeeId { get; set; }
    public AdjustmentKind Kind { get; set; }
    public decimal Amount { get; set; }
    public int Year { get; set; }
    public string Reason { get; set; } = "";
    public DateTimeOffset CreatedAt { get; set; }
}
