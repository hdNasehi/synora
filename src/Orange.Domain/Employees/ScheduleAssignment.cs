using Orange.Domain.Common;

namespace Orange.Domain.Employees;

public sealed class ScheduleAssignment
{
    public Guid Id { get; set; }
    public EmployeeId EmployeeId { get; set; }
    public WorkScheduleId ScheduleId { get; set; }
    public DateOnly ValidFrom { get; set; }
    public DateOnly? ValidTo { get; set; }
}
