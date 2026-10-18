using Orange.Domain.Common;

namespace Orange.Domain.Employees;

public sealed class Team : Entity<TeamId>
{
    public string Name { get; set; } = "";
    public DepartmentId DepartmentId { get; set; }
    public int MinPresent { get; set; }
}
