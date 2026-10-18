using Synora.Domain.Common;

namespace Synora.Domain.Employees;

public sealed class Department : Entity<DepartmentId>
{
    public string Name { get; set; } = "";
}
