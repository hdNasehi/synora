using Synora.Domain.Common;

namespace Synora.Domain.Employees;

public sealed class Substitute
{
    public Guid Id { get; set; }
    public EmployeeId ForEmployeeId { get; set; }
    public EmployeeId SubstituteEmployeeId { get; set; }
    public DateOnly From { get; set; }
    public DateOnly To { get; set; }
}
