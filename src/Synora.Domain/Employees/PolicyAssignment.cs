using Synora.Domain.Common;

namespace Synora.Domain.Employees;

public sealed class PolicyAssignment
{
    public Guid Id { get; set; }
    public EmployeeId EmployeeId { get; set; }
    public LeavePolicyId PolicyId { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
}
