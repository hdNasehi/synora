using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orange.Domain.Balances;
using Orange.Domain.Calendars;
using Orange.Domain.Common;
using Orange.Domain.Employees;

namespace Orange.Infrastructure.Persistence.Seed;

public static class DevelopmentSeeder
{
    public static async Task SeedAsync(IHost host)
    {
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<OrangeDbContext>();
        await db.Database.MigrateAsync();

        if (await db.Employees.AnyAsync()) return;

        SeedDepartments(db);
        SeedTeams(db);
        SeedHolidayGroups(db);
        SeedSchedules(db);
        SeedHolidays(db);
        SeedHolidayObservances(db);
        SeedPolicies(db);
        SeedEmployees(db);
        SeedEmployeeAssignments(db);
        SeedScheduleAssignments(db);
        SeedPolicyAssignments(db);
        SeedSubstitutes(db);
        SeedAdjustments(db);

        await db.SaveChangesAsync();
    }

    private static readonly Dictionary<string, Guid> Ids = new()
    {
        ["e01"] = Guid.Parse("00000000-0000-0000-0000-000000000001"),
        ["e02"] = Guid.Parse("00000000-0000-0000-0000-000000000002"),
        ["e03"] = Guid.Parse("00000000-0000-0000-0000-000000000003"),
        ["e04"] = Guid.Parse("00000000-0000-0000-0000-000000000004"),
        ["e05"] = Guid.Parse("00000000-0000-0000-0000-000000000005"),
        ["e06"] = Guid.Parse("00000000-0000-0000-0000-000000000006"),
        ["e07"] = Guid.Parse("00000000-0000-0000-0000-000000000007"),
        ["e08"] = Guid.Parse("00000000-0000-0000-0000-000000000008"),
        ["e09"] = Guid.Parse("00000000-0000-0000-0000-000000000009"),
        ["e10"] = Guid.Parse("00000000-0000-0000-0000-00000000000a"),
        ["e11"] = Guid.Parse("00000000-0000-0000-0000-00000000000b"),
        ["e12"] = Guid.Parse("00000000-0000-0000-0000-00000000000c"),
        ["e13"] = Guid.Parse("00000000-0000-0000-0000-00000000000d"),
        ["e14"] = Guid.Parse("00000000-0000-0000-0000-00000000000e"),
        ["e15"] = Guid.Parse("00000000-0000-0000-0000-00000000000f"),
        ["e16"] = Guid.Parse("00000000-0000-0000-0000-000000000010"),
        ["e17"] = Guid.Parse("00000000-0000-0000-0000-000000000011"),
    };

    private static EmployeeId EmpId(string key) => new(Ids[key]);

    private static readonly Guid DeptOperations = Guid.Parse("10000000-0000-0000-0000-000000000001");
    private static readonly Guid DeptFinance = Guid.Parse("10000000-0000-0000-0000-000000000002");
    private static readonly Guid DeptTechnology = Guid.Parse("10000000-0000-0000-0000-000000000003");
    private static readonly Guid DeptPeople = Guid.Parse("10000000-0000-0000-0000-000000000004");
    private static readonly Guid DeptManagement = Guid.Parse("10000000-0000-0000-0000-000000000005");

    private static readonly Dictionary<string, Guid> TeamIds = new()
    {
        ["cs"] = Guid.Parse("20000000-0000-0000-0000-000000000001"),
        ["pl"] = Guid.Parse("20000000-0000-0000-0000-000000000002"),
        ["fin"] = Guid.Parse("20000000-0000-0000-0000-000000000003"),
        ["eng"] = Guid.Parse("20000000-0000-0000-0000-000000000004"),
        ["hr"] = Guid.Parse("20000000-0000-0000-0000-000000000005"),
        ["mgmt"] = Guid.Parse("20000000-0000-0000-0000-000000000006"),
    };

    private static readonly Dictionary<string, Guid> ScheduleIds = new()
    {
        ["sch-a"] = Guid.Parse("30000000-0000-0000-0000-000000000001"),
        ["sch-b"] = Guid.Parse("30000000-0000-0000-0000-000000000002"),
        ["sch-p"] = Guid.Parse("30000000-0000-0000-0000-000000000003"),
    };

    private static readonly Dictionary<string, Guid> HolidayGroupIds = new()
    {
        ["nl-office"] = Guid.Parse("40000000-0000-0000-0000-000000000001"),
        ["support-desk"] = Guid.Parse("40000000-0000-0000-0000-000000000002"),
    };

    private static readonly Dictionary<string, Guid> PolicyIds = new()
    {
        ["pol-std"] = Guid.Parse("50000000-0000-0000-0000-000000000001"),
        ["pol-pt"] = Guid.Parse("50000000-0000-0000-0000-000000000002"),
    };

    private static void SeedDepartments(OrangeDbContext db)
    {
        Dept(db, DeptOperations, "Operations");
        Dept(db, DeptFinance, "Finance");
        Dept(db, DeptTechnology, "Technology");
        Dept(db, DeptPeople, "People");
        Dept(db, DeptManagement, "Management");
    }

    private static void Dept(OrangeDbContext db, Guid id, string name)
    {
        var dept = new Department { Name = name };
        db.Entry(dept).Property(d => d.Id).CurrentValue = new DepartmentId(id);
        db.Departments.Add(dept);
    }

    private static void SeedTeams(OrangeDbContext db)
    {
        Team(db, "cs", "Customer Success", DeptOperations, 5);
        Team(db, "pl", "Planning & Logistics", DeptOperations, 4);
        Team(db, "fin", "Finance", DeptFinance, 1);
        Team(db, "eng", "Engineering", DeptTechnology, 2);
        Team(db, "hr", "People & HR", DeptPeople, 1);
        Team(db, "mgmt", "Management", DeptManagement, 0);
    }

    private static void Team(OrangeDbContext db, string key, string name, Guid deptId, int minPresent)
    {
        var team = new Team { Name = name, DepartmentId = new DepartmentId(deptId), MinPresent = minPresent };
        db.Entry(team).Property(t => t.Id).CurrentValue = new TeamId(TeamIds[key]);
        db.Teams.Add(team);
    }

    private static void SeedHolidayGroups(OrangeDbContext db)
    {
        HGroup(db, "nl-office", "NL office staff");
        HGroup(db, "support-desk", "International support desk");
    }

    private static void HGroup(OrangeDbContext db, string key, string name)
    {
        var g = new HolidayGroup { Name = name };
        db.Entry(g).Property(x => x.Id).CurrentValue = new HolidayGroupId(HolidayGroupIds[key]);
        db.HolidayGroups.Add(g);
    }

    private static void SeedSchedules(OrangeDbContext db)
    {
        var day = new WorkDay(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(17, 30), 8m);

        var schA = new WorkSchedule { Code = "A", Name = "Schedule A — Mon–Fri" };
        db.Entry(schA).Property(s => s.Id).CurrentValue = new WorkScheduleId(ScheduleIds["sch-a"]);
        for (var d = DayOfWeek.Monday; d <= DayOfWeek.Friday; d++)
            schA.AddDay(new WorkDay(d, new TimeOnly(9, 0), new TimeOnly(17, 30), 8m));
        db.WorkSchedules.Add(schA);

        var schB = new WorkSchedule { Code = "B", Name = "Schedule B — Sun–Thu" };
        db.Entry(schB).Property(s => s.Id).CurrentValue = new WorkScheduleId(ScheduleIds["sch-b"]);
        foreach (var d in new[] { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday })
            schB.AddDay(new WorkDay(d, new TimeOnly(8, 0), new TimeOnly(16, 30), 8m));
        db.WorkSchedules.Add(schB);

        var schP = new WorkSchedule { Code = "P", Name = "Schedule P — Mon–Thu (part-time)" };
        db.Entry(schP).Property(s => s.Id).CurrentValue = new WorkScheduleId(ScheduleIds["sch-p"]);
        for (var d = DayOfWeek.Monday; d <= DayOfWeek.Thursday; d++)
            schP.AddDay(new WorkDay(d, new TimeOnly(9, 0), new TimeOnly(17, 30), 8m));
        db.WorkSchedules.Add(schP);
    }

    private static readonly Dictionary<string, Guid> HolidayIds = [];
    private static int _holidaySeq;

    private static void SeedHolidays(OrangeDbContext db)
    {
        H(db, "h26-01", "2026-01-01", "New Year's Day", "public");
        H(db, "h26-02", "2026-04-03", "Good Friday", "public");
        H(db, "h26-03", "2026-04-05", "Easter Sunday", "public");
        H(db, "h26-04", "2026-04-06", "Easter Monday", "public");
        H(db, "h26-05", "2026-04-27", "King's Day", "public");
        H(db, "h26-06", "2026-05-05", "Liberation Day", "public");
        H(db, "h26-07", "2026-05-14", "Ascension Day", "public");
        H(db, "h26-c1", "2026-05-15", "Bridge day", "company");
        H(db, "h26-08", "2026-05-24", "Whit Sunday", "public");
        H(db, "h26-09", "2026-05-25", "Whit Monday", "public");
        H(db, "h26-c2", "2026-11-06", "Noordkade Day", "company");
        H(db, "h26-10", "2026-12-25", "Christmas Day", "public");
        H(db, "h26-11", "2026-12-26", "Boxing Day", "public");
        H(db, "h27-01", "2027-01-01", "New Year's Day", "public");
        H(db, "h27-02", "2027-03-26", "Good Friday", "public");
        H(db, "h27-03", "2027-03-28", "Easter Sunday", "public");
        H(db, "h27-04", "2027-03-29", "Easter Monday", "public");
        H(db, "h27-05", "2027-04-27", "King's Day", "public");
        H(db, "h27-06", "2027-05-05", "Liberation Day", "public");
        H(db, "h27-07", "2027-05-06", "Ascension Day", "public");
        H(db, "h27-08", "2027-05-16", "Whit Sunday", "public");
        H(db, "h27-09", "2027-05-17", "Whit Monday", "public");
        H(db, "h27-10", "2027-12-25", "Christmas Day", "public");
        H(db, "h27-11", "2027-12-26", "Boxing Day", "public");
    }

    private static void H(OrangeDbContext db, string key, string date, string name, string kind)
    {
        var id = Guid.Parse($"60000000-0000-0000-0000-{(++_holidaySeq):D12}");
        HolidayIds[key] = id;
        var h = new Holiday { Date = DateOnly.Parse(date), Name = name, Kind = kind };
        db.Entry(h).Property(x => x.Id).CurrentValue = new HolidayId(id);
        db.Holidays.Add(h);
    }

    private static void SeedHolidayObservances(OrangeDbContext db)
    {
        var data = new (string hKey, bool office, bool desk)[]
        {
            ("h26-01", true, true), ("h26-02", false, false), ("h26-03", true, true),
            ("h26-04", true, true), ("h26-05", true, true), ("h26-06", false, false),
            ("h26-07", true, true), ("h26-c1", true, false), ("h26-08", true, true),
            ("h26-09", true, true), ("h26-c2", true, true), ("h26-10", true, true),
            ("h26-11", true, true),
            ("h27-01", true, true), ("h27-02", false, false), ("h27-03", true, true),
            ("h27-04", true, true), ("h27-05", true, true), ("h27-06", false, false),
            ("h27-07", true, true), ("h27-08", true, true), ("h27-09", true, true),
            ("h27-10", true, true), ("h27-11", true, true),
        };

        foreach (var (hKey, office, desk) in data)
        {
            var hId = new HolidayId(HolidayIds[hKey]);
            db.HolidayObservances.Add(new HolidayObservance
            {
                HolidayId = hId,
                GroupId = new HolidayGroupId(HolidayGroupIds["nl-office"]),
                Observed = office
            });
            db.HolidayObservances.Add(new HolidayObservance
            {
                HolidayId = hId,
                GroupId = new HolidayGroupId(HolidayGroupIds["support-desk"]),
                Observed = desk
            });
        }
    }

    private static void SeedPolicies(OrangeDbContext db)
    {
        var std = new LeavePolicy
        {
            Name = "Standard — full-time", Unit = BalanceUnit.Days,
            Entitlement = 25, CarryOverMax = 5, MinNoticeDays = 2, MaxCalendarDays = 62
        };
        db.Entry(std).Property(p => p.Id).CurrentValue = new LeavePolicyId(PolicyIds["pol-std"]);
        db.LeavePolicies.Add(std);

        var pt = new LeavePolicy
        {
            Name = "Part-time — hours based", Unit = BalanceUnit.Hours,
            Entitlement = 160, CarryOverMax = 40, MinNoticeDays = 2, MaxCalendarDays = 62
        };
        db.Entry(pt).Property(p => p.Id).CurrentValue = new LeavePolicyId(PolicyIds["pol-pt"]);
        db.LeavePolicies.Add(pt);
    }

    private static void SeedEmployees(OrangeDbContext db)
    {
        Emp(db, "e01", "Sanne", "de Vries", "sanne.de.vries@noordkade.nl", "cs", 1m, "nl-office");
        Emp(db, "e02", "Daan", "Bakker", "daan.bakker@noordkade.nl", "cs", 1m, "nl-office");
        Emp(db, "e03", "Priya", "Raman", "priya.raman@noordkade.nl", "cs", 1m, "nl-office");
        Emp(db, "e04", "Joris", "Meijer", "joris.meijer@noordkade.nl", "cs", 1m, "nl-office");
        Emp(db, "e05", "Omar", "Haddad", "omar.haddad@noordkade.nl", "cs", 1m, "support-desk");
        Emp(db, "e06", "Mila", "Bos", "mila.bos@noordkade.nl", "cs", 1m, "nl-office");
        Emp(db, "e07", "Lucas", "Peeters", "lucas.peeters@noordkade.nl", "cs", 1m, "nl-office");
        Emp(db, "e08", "Femke", "Smit", "femke.smit@noordkade.nl", "pl", 1m, "nl-office");
        Emp(db, "e09", "Bram", "Willems", "bram.willems@noordkade.nl", "pl", 1m, "nl-office");
        Emp(db, "e10", "Ahmed", "El Amrani", "ahmed.el.amrani@noordkade.nl", "pl", 1m, "nl-office");
        Emp(db, "e11", "Tim", "van Dijk", "tim.van.dijk@noordkade.nl", "pl", 1m, "nl-office");
        Emp(db, "e12", "Noor", "Hendriks", "noor.hendriks@noordkade.nl", "pl", 1m, "nl-office");
        Emp(db, "e13", "Eva", "Jansen", "eva.jansen@noordkade.nl", "mgmt", 1m, "nl-office");
        Emp(db, "e14", "Lotte", "Visser", "lotte.visser@noordkade.nl", "hr", 1m, "nl-office");
        Emp(db, "e15", "Chloé", "Martin", "chloe.martin@noordkade.nl", "eng", 1m, "nl-office");
        Emp(db, "e16", "Kees", "Mulder", "kees.mulder@noordkade.nl", "fin", 1m, "nl-office", false);
        Emp(db, "e17", "Marc", "Dekker", "marc.dekker@noordkade.nl", "mgmt", 1m, "nl-office");
    }

    private static void Emp(OrangeDbContext db, string key, string first, string last,
        string email, string teamKey, decimal fte, string hgKey, bool active = true)
    {
        var emp = new Employee
        {
            FirstName = first, LastName = last, Email = email,
            TeamId = new TeamId(TeamIds[teamKey]),
            DepartmentId = new DepartmentId(teamKey switch
            {
                "cs" or "pl" => DeptOperations,
                "fin" => DeptFinance,
                "eng" => DeptTechnology,
                "hr" => DeptPeople,
                "mgmt" => DeptManagement,
                _ => DeptOperations
            }),
            Fte = fte,
            HolidayGroupId = new HolidayGroupId(HolidayGroupIds[hgKey]),
            IsActive = active
        };
        db.Entry(emp).Property(e => e.Id).CurrentValue = EmpId(key);
        db.Employees.Add(emp);
    }

    private static void SeedEmployeeAssignments(OrangeDbContext db)
    {
        Assign(db, "e01", ["e02", "e03"], ApprovalRule.All, "e13", "2025-04-01");
        Assign(db, "e02", ["e03"], ApprovalRule.Any, "e13", "2020-02-03");
        Assign(db, "e03", [], ApprovalRule.None, "e13", "2019-06-17");
        Assign(db, "e04", ["e02", "e03"], ApprovalRule.Any, "e13", "2024-01-08");
        Assign(db, "e05", ["e02"], ApprovalRule.Any, "e13", "2024-03-01");
        Assign(db, "e06", ["e02"], ApprovalRule.Any, "e13", "2023-05-15");
        Assign(db, "e07", ["e02"], ApprovalRule.Any, "e13", "2025-02-03");
        Assign(db, "e08", ["e09"], ApprovalRule.Any, "e13", "2021-11-01");
        Assign(db, "e09", [], ApprovalRule.None, "e13", "2019-10-01");
        Assign(db, "e10", ["e09", "e03"], ApprovalRule.Sequential, "e13", "2022-11-14");
        Assign(db, "e11", ["e09"], ApprovalRule.Any, "e13", "2023-08-21");
        Assign(db, "e12", ["e09"], ApprovalRule.Any, "e13", "2024-02-12");
        Assign(db, "e13", [], ApprovalRule.None, "e17", "2018-04-03");
        Assign(db, "e14", [], ApprovalRule.None, "e17", "2019-01-07");
        Assign(db, "e15", [], ApprovalRule.None, "e17", "2024-06-03");
        Assign(db, "e16", [], ApprovalRule.None, "e17", "2020-05-04");
        Assign(db, "e17", [], ApprovalRule.None, null, "2015-01-05");
    }

    private static void Assign(OrangeDbContext db, string empKey, string[] supervisorKeys,
        ApprovalRule rule, string? managerKey, string from)
    {
        db.EmployeeAssignments.Add(new EmployeeAssignment
        {
            Id = Guid.NewGuid(),
            EmployeeId = EmpId(empKey),
            SupervisorIds = supervisorKeys.Select(EmpId).ToList(),
            Rule = rule,
            ManagerId = managerKey is not null ? EmpId(managerKey) : default,
            ValidFrom = DateOnly.Parse(from)
        });
    }

    private static void SeedScheduleAssignments(OrangeDbContext db)
    {
        foreach (var key in Ids.Keys)
        {
            var schedId = key switch
            {
                "e05" => "sch-b",
                "e08" => "sch-p",
                _ => "sch-a"
            };
            var from = key switch
            {
                "e05" => "2024-03-01",
                "e08" => "2023-01-01",
                _ => "2024-01-01"
            };
            db.ScheduleAssignments.Add(new ScheduleAssignment
            {
                Id = Guid.NewGuid(),
                EmployeeId = EmpId(key),
                ScheduleId = new WorkScheduleId(ScheduleIds[schedId]),
                ValidFrom = DateOnly.Parse(from)
            });
        }
    }

    private static void SeedPolicyAssignments(OrangeDbContext db)
    {
        foreach (var key in Ids.Keys)
        {
            var polId = key == "e08" ? "pol-pt" : "pol-std";
            db.PolicyAssignments.Add(new PolicyAssignment
            {
                Id = Guid.NewGuid(),
                EmployeeId = EmpId(key),
                PolicyId = new LeavePolicyId(PolicyIds[polId]),
                ValidFrom = DateOnly.Parse("2026-01-01")
            });
        }
    }

    private static void SeedSubstitutes(OrangeDbContext db)
    {
        db.Substitutes.Add(new Substitute
        {
            Id = Guid.NewGuid(),
            ForEmployeeId = EmpId("e02"),
            SubstituteEmployeeId = EmpId("e03"),
            From = new DateOnly(2026, 12, 28),
            To = new DateOnly(2027, 1, 1)
        });
        db.Substitutes.Add(new Substitute
        {
            Id = Guid.NewGuid(),
            ForEmployeeId = EmpId("e09"),
            SubstituteEmployeeId = EmpId("e03"),
            From = new DateOnly(2026, 10, 19),
            To = new DateOnly(2026, 10, 23)
        });
    }

    private static void SeedAdjustments(OrangeDbContext db)
    {
        Adj(db, "e01", AdjustmentKind.CarryOver, 3, 2026, "Carry-over from 2025", "2026-01-01T00:05:00Z");
        Adj(db, "e01", AdjustmentKind.Adjustment, 1, 2026, "Long-service day (5+ years incl. traineeship) — policy LS-2", "2026-01-05T09:12:00Z");
        Adj(db, "e04", AdjustmentKind.CarryOver, 2, 2026, "Carry-over from 2025", "2026-01-01T00:05:00Z");
        Adj(db, "e10", AdjustmentKind.Adjustment, -1, 2026, "Correction: 4 May booked as sick in error, re-registered as annual leave", "2026-05-12T15:40:00Z");
        Adj(db, "e05", AdjustmentKind.CarryOver, 4.5m, 2026, "Carry-over from 2025", "2026-01-01T00:05:00Z");
        Adj(db, "e08", AdjustmentKind.CarryOver, 12, 2026, "Carry-over from 2025 (hours)", "2026-01-01T00:05:00Z");
    }

    private static void Adj(OrangeDbContext db, string empKey, AdjustmentKind kind,
        decimal amount, int year, string reason, string at)
    {
        db.BalanceAdjustments.Add(new BalanceAdjustment
        {
            Id = Guid.NewGuid(),
            EmployeeId = EmpId(empKey),
            Kind = kind,
            Amount = amount,
            Year = year,
            Reason = reason,
            CreatedAt = DateTimeOffset.Parse(at)
        });
    }
}
