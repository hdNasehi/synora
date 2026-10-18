using Synora.Domain.Common;

namespace Synora.Domain.Employees;

public sealed class EmployeeAssignment
{
    public Guid Id { get; set; }
    public EmployeeId EmployeeId { get; set; }
    public IReadOnlyList<EmployeeId> SupervisorIds { get; set; } = [];
    public ApprovalRule Rule { get; set; }
    public EmployeeId ManagerId { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
}
