using Microsoft.EntityFrameworkCore;
using Orange.Domain.Balances;
using Orange.Domain.Calendars;
using Orange.Domain.Common;
using Orange.Domain.Employees;
using Orange.Domain.Leave;

namespace Orange.Application.Abstractions;

public interface IAppDbContext
{
    DbSet<Employee> Employees { get; }
    DbSet<LeaveRequest> LeaveRequests { get; }
    DbSet<WorkSchedule> WorkSchedules { get; }
    DbSet<Holiday> Holidays { get; }
    DbSet<HolidayGroup> HolidayGroups { get; }
    DbSet<HolidayObservance> HolidayObservances { get; }
    DbSet<LeavePolicy> LeavePolicies { get; }
    DbSet<BalanceAdjustment> BalanceAdjustments { get; }
    DbSet<Substitute> Substitutes { get; }
    DbSet<Team> Teams { get; }
    DbSet<Department> Departments { get; }
    DbSet<EmployeeAssignment> EmployeeAssignments { get; }
    DbSet<ScheduleAssignment> ScheduleAssignments { get; }
    DbSet<PolicyAssignment> PolicyAssignments { get; }
    DbSet<AuditEntry> AuditEntries { get; }
    DbSet<OutboxMessage> OutboxMessages { get; }
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
