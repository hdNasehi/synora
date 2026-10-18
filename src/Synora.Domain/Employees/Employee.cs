using Synora.Domain.Common;

namespace Synora.Domain.Employees;

public sealed class Employee : Entity<EmployeeId>
{
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public TeamId TeamId { get; set; }
    public DepartmentId DepartmentId { get; set; }
    public decimal Fte { get; set; } = 1m;
    public HolidayGroupId HolidayGroupId { get; set; }
    public bool IsActive { get; set; } = true;

    public string FullName => $"{FirstName} {LastName}";
}
