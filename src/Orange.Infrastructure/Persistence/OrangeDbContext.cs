using Microsoft.EntityFrameworkCore;
using Orange.Application.Abstractions;
using Orange.Domain.Balances;
using Orange.Domain.Calendars;
using Orange.Domain.Common;
using Orange.Domain.Employees;
using Orange.Domain.Leave;

namespace Orange.Infrastructure.Persistence;

public sealed class OrangeDbContext : DbContext, IAppDbContext
{
    public OrangeDbContext(DbContextOptions<OrangeDbContext> options) : base(options) { }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<WorkSchedule> WorkSchedules => Set<WorkSchedule>();
    public DbSet<Holiday> Holidays => Set<Holiday>();
    public DbSet<HolidayGroup> HolidayGroups => Set<HolidayGroup>();
    public DbSet<HolidayObservance> HolidayObservances => Set<HolidayObservance>();
    public DbSet<LeavePolicy> LeavePolicies => Set<LeavePolicy>();
    public DbSet<BalanceAdjustment> BalanceAdjustments => Set<BalanceAdjustment>();
    public DbSet<Substitute> Substitutes => Set<Substitute>();
    public DbSet<Team> Teams => Set<Team>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<EmployeeAssignment> EmployeeAssignments => Set<EmployeeAssignment>();
    public DbSet<ScheduleAssignment> ScheduleAssignments => Set<ScheduleAssignment>();
    public DbSet<PolicyAssignment> PolicyAssignments => Set<PolicyAssignment>();
    public DbSet<AuditEntry> AuditEntries => Set<AuditEntry>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrangeDbContext).Assembly);
    }
}
