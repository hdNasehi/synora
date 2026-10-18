using FluentAssertions;
using Synora.Domain.Calendars;
using Synora.Domain.Common;
using Synora.Domain.Employees;
using Synora.Domain.Leave;
using Synora.Domain.Services;

namespace Synora.Domain.Tests;

public class WorkingDayCalculatorTests
{
    private readonly WorkingDayCalculator _sut = new();

    // -- Shared IDs --
    private static readonly WorkScheduleId SchAId = new(Guid.NewGuid());
    private static readonly WorkScheduleId SchBId = new(Guid.NewGuid());
    private static readonly WorkScheduleId SchPId = new(Guid.NewGuid());
    private static readonly HolidayGroupId NlOffice = new(Guid.NewGuid());
    private static readonly HolidayGroupId SupportDesk = new(Guid.NewGuid());
    private static readonly EmployeeId SanneId = new(Guid.NewGuid());
    private static readonly EmployeeId JorisId = new(Guid.NewGuid());
    private static readonly EmployeeId OmarId = new(Guid.NewGuid());
    private static readonly EmployeeId FemkeId = new(Guid.NewGuid());

    // -- Schedules --
    private static WorkSchedule MakeScheduleA()
    {
        var sch = new WorkSchedule { Code = "A", Name = "Mon-Fri" };
        sch.SetIdForTest(SchAId);
        var day = new WorkDay(DayOfWeek.Monday, new TimeOnly(9, 0), new TimeOnly(17, 30), 8);
        foreach (var dow in new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday })
            sch.AddDay(new WorkDay(dow, new TimeOnly(9, 0), new TimeOnly(17, 30), 8));
        return sch;
    }

    private static WorkSchedule MakeScheduleB()
    {
        var sch = new WorkSchedule { Code = "B", Name = "Sun-Thu" };
        sch.SetIdForTest(SchBId);
        foreach (var dow in new[] { DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday })
            sch.AddDay(new WorkDay(dow, new TimeOnly(8, 0), new TimeOnly(16, 30), 8));
        return sch;
    }

    private static WorkSchedule MakeScheduleP(decimal hours = 8)
    {
        var sch = new WorkSchedule { Code = "P", Name = "Mon-Thu part-time" };
        sch.SetIdForTest(SchPId);
        var end = hours == 9 ? new TimeOnly(18, 30) : new TimeOnly(17, 30);
        foreach (var dow in new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday })
            sch.AddDay(new WorkDay(dow, new TimeOnly(9, 0), end, hours));
        return sch;
    }

    private static Employee MakeEmployee(EmployeeId id, string name, HolidayGroupId groupId)
    {
        var emp = new Employee { FirstName = name, LastName = "", HolidayGroupId = groupId };
        emp.SetIdForTest(id);
        return emp;
    }

    private static ScheduleAssignment Assignment(EmployeeId empId, WorkScheduleId schId, DateOnly from, DateOnly? to = null) =>
        new() { Id = Guid.NewGuid(), EmployeeId = empId, ScheduleId = schId, ValidFrom = from, ValidTo = to };

    // -- Holidays --
    private static readonly List<Holiday> AllHolidays = BuildHolidays();
    private static readonly List<HolidayObservance> AllObservances = BuildObservances();

    private static Holiday H(string name, DateOnly date, string kind = "public")
    {
        var h = new Holiday { Date = date, Name = name, Kind = kind };
        h.SetIdForTest(new HolidayId(Guid.NewGuid()));
        return h;
    }

    private static List<Holiday> BuildHolidays()
    {
        return
        [
            H("Good Friday", new DateOnly(2026, 4, 3)),
            H("Liberation Day", new DateOnly(2026, 5, 5)),
            H("Ascension Day", new DateOnly(2026, 5, 14)),
            H("Bridge day", new DateOnly(2026, 5, 15), "company"),
            H("Noordkade Day", new DateOnly(2026, 11, 6), "company"),
            H("Liberation Day 2027", new DateOnly(2027, 5, 5)),
            H("Ascension Day 2027", new DateOnly(2027, 5, 6)),
        ];
    }

    private static List<HolidayObservance> BuildObservances()
    {
        var holidays = AllHolidays;
        var obs = new List<HolidayObservance>();

        void Add(Holiday h, HolidayGroupId g, bool observed) =>
            obs.Add(new HolidayObservance { HolidayId = h.Id, GroupId = g, Observed = observed });

        // Good Friday: not observed by anyone
        Add(holidays[0], NlOffice, false);
        Add(holidays[0], SupportDesk, false);

        // Liberation Day 2026: not observed (non-lustrum)
        Add(holidays[1], NlOffice, false);
        Add(holidays[1], SupportDesk, false);

        // Ascension Day 2026: observed by both
        Add(holidays[2], NlOffice, true);
        Add(holidays[2], SupportDesk, true);

        // Bridge day: observed by nl-office only
        Add(holidays[3], NlOffice, true);
        Add(holidays[3], SupportDesk, false);

        // Noordkade Day: observed by both
        Add(holidays[4], NlOffice, true);
        Add(holidays[4], SupportDesk, true);

        // Liberation Day 2027: default not observed
        Add(holidays[5], NlOffice, false);
        Add(holidays[5], SupportDesk, false);

        // Ascension Day 2027: observed by both
        Add(holidays[6], NlOffice, true);
        Add(holidays[6], SupportDesk, true);

        return obs;
    }

    // ---- Test cases from docs/03-domain-rules.md ----

    [Fact]
    public void Weekend_excluded_Sanne_Thu_to_Mon()
    {
        // Sanne, Schedule A (Mon–Fri), Thu 12 Nov – Mon 16 Nov 2026 → 3 working days
        var result = _sut.Calculate(
            MakeEmployee(SanneId, "Sanne", NlOffice),
            new DateOnly(2026, 11, 12), new DateOnly(2026, 11, 16), null,
            [Assignment(SanneId, SchAId, new DateOnly(2024, 1, 1))],
            [MakeScheduleA()], AllHolidays, AllObservances);

        result.WorkingDays.Should().Be(3);
        result.ExcludedRestDays.Should().HaveCount(2);
    }

    [Fact]
    public void Company_day_excluded_Joris()
    {
        // Joris, Schedule A, Mon 2 – Fri 6 Nov 2026 → 4 days (Fri 6 = Noordkade Day)
        var result = _sut.Calculate(
            MakeEmployee(JorisId, "Joris", NlOffice),
            new DateOnly(2026, 11, 2), new DateOnly(2026, 11, 6), null,
            [Assignment(JorisId, SchAId, new DateOnly(2024, 1, 1))],
            [MakeScheduleA()], AllHolidays, AllObservances);

        result.WorkingDays.Should().Be(4);
        result.ExcludedHolidays.Should().ContainSingle(h => h.Name == "Noordkade Day");
    }

    [Fact]
    public void Ascension_plus_bridge_day_Sanne()
    {
        // Sanne, Schedule A, Mon 11 – Fri 15 May 2026 → 3 days (Thu 14 = Ascension, Fri 15 = Bridge)
        var result = _sut.Calculate(
            MakeEmployee(SanneId, "Sanne", NlOffice),
            new DateOnly(2026, 5, 11), new DateOnly(2026, 5, 15), null,
            [Assignment(SanneId, SchAId, new DateOnly(2024, 1, 1))],
            [MakeScheduleA()], AllHolidays, AllObservances);

        result.WorkingDays.Should().Be(3);
        result.ExcludedHolidays.Should().HaveCount(2);
    }

    [Fact]
    public void Sun_Thu_schedule_Omar()
    {
        // Omar, Schedule B (Sun–Thu), Thu 5 – Sun 8 Nov 2026 → 2 days (Fri 6 = Noordkade but also rest, Sat 7 = rest)
        var result = _sut.Calculate(
            MakeEmployee(OmarId, "Omar", SupportDesk),
            new DateOnly(2026, 11, 5), new DateOnly(2026, 11, 8), null,
            [Assignment(OmarId, SchBId, new DateOnly(2024, 3, 1))],
            [MakeScheduleB()], AllHolidays, AllObservances);

        result.WorkingDays.Should().Be(2);
    }

    [Fact]
    public void Holiday_not_observed_Good_Friday()
    {
        // Sanne, Schedule A, Good Friday (3 Apr) inside 1–3 Apr 2026 → 3 working days
        var result = _sut.Calculate(
            MakeEmployee(SanneId, "Sanne", NlOffice),
            new DateOnly(2026, 4, 1), new DateOnly(2026, 4, 3), null,
            [Assignment(SanneId, SchAId, new DateOnly(2024, 1, 1))],
            [MakeScheduleA()], AllHolidays, AllObservances);

        result.WorkingDays.Should().Be(3);
        result.IgnoredHolidays.Should().ContainSingle(h => h.Name == "Good Friday" && !h.Observed);
    }

    [Theory]
    [InlineData(false, 4)] // Liberation Day not observed → 4 working days
    [InlineData(true, 3)]  // Liberation Day observed → 3 working days
    public void Liberation_Day_toggled(bool observed, int expectedDays)
    {
        // Sanne, Schedule A, Mon 3 – Fri 7 May 2027
        // Ascension Day 2027 (Thu 6 May) is always excluded.
        // Liberation Day 2027 (Wed 5 May) toggled.
        var liberationDay2027 = AllHolidays[5];
        var observances = AllObservances.ToList();

        if (observed)
        {
            var idx = observances.FindIndex(o => o.HolidayId == liberationDay2027.Id && o.GroupId == NlOffice);
            observances[idx] = new HolidayObservance { HolidayId = liberationDay2027.Id, GroupId = NlOffice, Observed = true };
        }

        var result = _sut.Calculate(
            MakeEmployee(SanneId, "Sanne", NlOffice),
            new DateOnly(2027, 5, 3), new DateOnly(2027, 5, 7), null,
            [Assignment(SanneId, SchAId, new DateOnly(2024, 1, 1))],
            [MakeScheduleA()], AllHolidays, observances);

        result.WorkingDays.Should().Be(expectedDays);
    }

    [Fact]
    public void Partial_day_Sanne()
    {
        // Sanne, Schedule A, Fri 18 Sep 2026 13:30–17:30 → 0.5 day, 4 hours
        var result = _sut.Calculate(
            MakeEmployee(SanneId, "Sanne", NlOffice),
            new DateOnly(2026, 9, 18), new DateOnly(2026, 9, 18),
            new TimeRange(new TimeOnly(13, 30), new TimeOnly(17, 30)),
            [Assignment(SanneId, SchAId, new DateOnly(2024, 1, 1))],
            [MakeScheduleA()], AllHolidays, AllObservances);

        result.WorkingDays.Should().Be(0.5m);
        result.TotalHours.Should().Be(4);
    }

    [Fact]
    public void Part_time_hours_Femke()
    {
        // Femke, Schedule P (Mon–Thu 8h), Mon 7 – Fri 11 Dec 2026 → 32 hours
        var result = _sut.Calculate(
            MakeEmployee(FemkeId, "Femke", NlOffice),
            new DateOnly(2026, 12, 7), new DateOnly(2026, 12, 11), null,
            [Assignment(FemkeId, SchPId, new DateOnly(2023, 1, 1))],
            [MakeScheduleP()], AllHolidays, AllObservances);

        result.TotalHours.Should().Be(32);
        result.WorkingDays.Should().Be(4);
        result.ExcludedRestDays.Should().ContainSingle(); // Friday
    }

    [Fact]
    public void Schedule_change_mid_range_Femke()
    {
        // Femke, P (Mon–Thu 8h) until 30 Nov, then Mon–Thu 9h from 1 Dec.
        // Mon 7 – Fri 11 Dec 2026 → all in new schedule → 4 days × 9h = 36h
        var schP9Id = new WorkScheduleId(Guid.NewGuid());
        var schP9 = new WorkSchedule { Code = "P9", Name = "Mon-Thu 9h" };
        schP9.SetIdForTest(schP9Id);
        foreach (var dow in new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday })
            schP9.AddDay(new WorkDay(dow, new TimeOnly(9, 0), new TimeOnly(18, 30), 9));

        var assignments = new List<ScheduleAssignment>
        {
            Assignment(FemkeId, SchPId, new DateOnly(2023, 1, 1), new DateOnly(2026, 11, 30)),
            Assignment(FemkeId, schP9Id, new DateOnly(2026, 12, 1)),
        };

        var result = _sut.Calculate(
            MakeEmployee(FemkeId, "Femke", NlOffice),
            new DateOnly(2026, 12, 7), new DateOnly(2026, 12, 11), null,
            assignments, [MakeScheduleP(), schP9], AllHolidays, AllObservances);

        result.TotalHours.Should().Be(36);
        result.WorkingDays.Should().Be(4);
    }
}

internal static class EntityTestExtensions
{
    public static void SetIdForTest<TId>(this Entity<TId> entity, TId id) where TId : struct
    {
        typeof(Entity<TId>).GetProperty(nameof(Entity<TId>.Id))!.SetValue(entity, id);
    }
}
