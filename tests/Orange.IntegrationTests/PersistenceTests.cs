using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Orange.Domain.Balances;
using Orange.Domain.Calendars;
using Orange.Domain.Common;
using Orange.Domain.Employees;
using Orange.Domain.Leave;

namespace Orange.IntegrationTests;

[Collection("Database")]
public sealed class PersistenceTests
{
    private readonly DatabaseFixture _fixture;

    public PersistenceTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Can_persist_and_read_employee()
    {
        var empId = new EmployeeId(Guid.NewGuid());
        var teamId = new TeamId(Guid.NewGuid());
        var deptId = new DepartmentId(Guid.NewGuid());
        var hgId = new HolidayGroupId(Guid.NewGuid());

        await using (var db = _fixture.CreateContext())
        {
            var dept = new Department { Name = "Test Dept" };
            db.Entry(dept).Property(d => d.Id).CurrentValue = deptId;
            db.Departments.Add(dept);

            var team = new Team { Name = "Test Team", DepartmentId = deptId, MinPresent = 1 };
            db.Entry(team).Property(t => t.Id).CurrentValue = teamId;
            db.Teams.Add(team);

            var hg = new HolidayGroup { Name = "Test Group" };
            db.Entry(hg).Property(g => g.Id).CurrentValue = hgId;
            db.HolidayGroups.Add(hg);

            var emp = new Employee
            {
                FirstName = "Test", LastName = "User", Email = $"test-{empId.Value}@example.com",
                TeamId = teamId, DepartmentId = deptId, Fte = 0.8m, HolidayGroupId = hgId
            };
            db.Entry(emp).Property(e => e.Id).CurrentValue = empId;
            db.Employees.Add(emp);

            await db.SaveChangesAsync();
        }

        await using (var db = _fixture.CreateContext())
        {
            var emp = await db.Employees.FindAsync(empId);
            emp.Should().NotBeNull();
            emp!.FirstName.Should().Be("Test");
            emp.Fte.Should().Be(0.8m);
        }
    }

    [Fact]
    public async Task Can_persist_work_schedule_with_days()
    {
        var schedId = new WorkScheduleId(Guid.NewGuid());

        await using (var db = _fixture.CreateContext())
        {
            var sched = new WorkSchedule { Code = $"T{schedId.Value:N}"[..10], Name = "Test Schedule" };
            db.Entry(sched).Property(s => s.Id).CurrentValue = schedId;
            sched.AddDay(new WorkDay(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(17, 30), 8m));
            sched.AddDay(new WorkDay(DayOfWeek.Tuesday, new TimeOnly(9, 0), new TimeOnly(17, 30), 8m));
            db.WorkSchedules.Add(sched);
            await db.SaveChangesAsync();
        }

        await using (var db = _fixture.CreateContext())
        {
            var sched = await db.WorkSchedules
                .Include("_days")
                .FirstAsync(s => s.Id == schedId);
            sched.Days.Should().HaveCount(2);
            sched.Days[0].Hours.Should().Be(8m);
        }
    }

    [Fact]
    public async Task Can_persist_holiday_observances()
    {
        var hId = new HolidayId(Guid.NewGuid());
        var gId = new HolidayGroupId(Guid.NewGuid());

        await using (var db = _fixture.CreateContext())
        {
            var h = new Holiday { Date = new DateOnly(2026, 12, 25), Name = "Christmas", Kind = "public" };
            db.Entry(h).Property(x => x.Id).CurrentValue = hId;
            db.Holidays.Add(h);

            var g = new HolidayGroup { Name = "Test Group 2" };
            db.Entry(g).Property(x => x.Id).CurrentValue = gId;
            db.HolidayGroups.Add(g);

            db.HolidayObservances.Add(new HolidayObservance { HolidayId = hId, GroupId = gId, Observed = true });
            await db.SaveChangesAsync();
        }

        await using (var db = _fixture.CreateContext())
        {
            var obs = await db.HolidayObservances
                .FirstAsync(o => o.HolidayId == hId && o.GroupId == gId);
            obs.Observed.Should().BeTrue();
        }
    }

    [Fact]
    public async Task Can_persist_leave_request_with_calculation_snapshot()
    {
        var empId = new EmployeeId(Guid.NewGuid());
        var teamId = new TeamId(Guid.NewGuid());
        var deptId = new DepartmentId(Guid.NewGuid());
        var hgId = new HolidayGroupId(Guid.NewGuid());

        await using (var db = _fixture.CreateContext())
        {
            var dept = new Department { Name = "LR Dept" };
            db.Entry(dept).Property(d => d.Id).CurrentValue = deptId;
            db.Departments.Add(dept);

            var team = new Team { Name = "LR Team", DepartmentId = deptId };
            db.Entry(team).Property(t => t.Id).CurrentValue = teamId;
            db.Teams.Add(team);

            var hg = new HolidayGroup { Name = "LR Group" };
            db.Entry(hg).Property(g => g.Id).CurrentValue = hgId;
            db.HolidayGroups.Add(hg);

            var emp = new Employee
            {
                FirstName = "Leave", LastName = "Tester", Email = $"lr-{empId.Value}@example.com",
                TeamId = teamId, DepartmentId = deptId, HolidayGroupId = hgId
            };
            db.Entry(emp).Property(e => e.Id).CurrentValue = empId;
            db.Employees.Add(emp);
            await db.SaveChangesAsync();
        }

        var calculation = new LeaveCalculation(
            [new LeaveDay(new DateOnly(2026, 11, 2), LeaveDayKind.Work, 8m, 1m, "Monday")],
            1m, 8m,
            [new DateOnly(2026, 11, 1)],
            [],
            []);

        var steps = new List<ApprovalStep>
        {
            new()
            {
                Order = 1, Kind = StepKind.Manager, Rule = ApprovalRule.None,
                ApproverIds = [empId], State = StepState.Active
            }
        };
        // Set step ID
        var stepId = new ApprovalStepId(Guid.NewGuid());

        var request = LeaveRequest.Submit(
            empId, LeaveType.Annual,
            new DateOnly(2026, 11, 2), new DateOnly(2026, 11, 2),
            null, 1m, BalanceUnit.Days, "Test",
            calculation, steps, DateTimeOffset.UtcNow);
        request.ClearDomainEvents();

        await using (var db = _fixture.CreateContext())
        {
            db.LeaveRequests.Add(request);
            await db.SaveChangesAsync();
        }

        await using (var db = _fixture.CreateContext())
        {
            var loaded = await db.LeaveRequests
                .Include(r => r.Steps)
                .FirstAsync(r => r.Id == request.Id);

            loaded.Calculation.Should().NotBeNull();
            loaded.Calculation.WorkingDays.Should().Be(1m);
            loaded.Calculation.Days.Should().HaveCount(1);
            loaded.Steps.Should().HaveCount(1);
            loaded.RowVersion.Should().NotBeEmpty();
        }
    }

    [Fact]
    public async Task Can_persist_balance_adjustment()
    {
        var empId = new EmployeeId(Guid.NewGuid());

        await using var db = _fixture.CreateContext();

        var dept = new Department { Name = "Adj Dept" };
        var deptId = new DepartmentId(Guid.NewGuid());
        db.Entry(dept).Property(d => d.Id).CurrentValue = deptId;
        db.Departments.Add(dept);

        var teamId = new TeamId(Guid.NewGuid());
        var team = new Team { Name = "Adj Team", DepartmentId = deptId };
        db.Entry(team).Property(t => t.Id).CurrentValue = teamId;
        db.Teams.Add(team);

        var hgId = new HolidayGroupId(Guid.NewGuid());
        var hg = new HolidayGroup { Name = "Adj Group" };
        db.Entry(hg).Property(g => g.Id).CurrentValue = hgId;
        db.HolidayGroups.Add(hg);

        var emp = new Employee
        {
            FirstName = "Adj", LastName = "Tester", Email = $"adj-{empId.Value}@example.com",
            TeamId = teamId, DepartmentId = deptId, HolidayGroupId = hgId
        };
        db.Entry(emp).Property(e => e.Id).CurrentValue = empId;
        db.Employees.Add(emp);

        db.BalanceAdjustments.Add(new BalanceAdjustment
        {
            Id = Guid.NewGuid(), EmployeeId = empId,
            Kind = AdjustmentKind.CarryOver, Amount = 3.5m, Year = 2026,
            Reason = "Carry-over", CreatedAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync();

        var adj = await db.BalanceAdjustments.FirstAsync(a => a.EmployeeId == empId);
        adj.Amount.Should().Be(3.5m);
        adj.Kind.Should().Be(AdjustmentKind.CarryOver);
    }

    [Fact]
    public async Task Can_persist_audit_entry_and_outbox_message()
    {
        await using var db = _fixture.CreateContext();

        var auditId = new AuditEntryId(Guid.NewGuid());
        db.AuditEntries.Add(new AuditEntry
        {
            Id = auditId,
            ActorId = new EmployeeId(Guid.NewGuid()),
            Action = "Approved request",
            EntityType = "LeaveRequest",
            EntityId = "LR-001",
            Details = "Test audit entry",
            Timestamp = DateTimeOffset.UtcNow
        });

        var outboxId = Guid.NewGuid();
        db.OutboxMessages.Add(new OutboxMessage
        {
            Id = outboxId,
            EventType = "LeaveRequestApproved",
            Payload = """{"requestId":"LR-001"}""",
            CreatedAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync();

        var audit = await db.AuditEntries.FindAsync(auditId);
        audit.Should().NotBeNull();
        audit!.Action.Should().Be("Approved request");

        var outbox = await db.OutboxMessages.FindAsync(outboxId);
        outbox.Should().NotBeNull();
        outbox!.ProcessedAt.Should().BeNull();
    }
}
