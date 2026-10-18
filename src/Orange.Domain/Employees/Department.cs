using Orange.Domain.Common;

namespace Orange.Domain.Employees;

public sealed class Department : Entity<DepartmentId>
{
    public string Name { get; set; } = "";
}
