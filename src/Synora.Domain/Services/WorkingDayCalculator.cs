using Synora.Domain.Calendars;
using Synora.Domain.Common;
using Synora.Domain.Employees;
using Synora.Domain.Leave;

namespace Synora.Domain.Services;

public sealed class WorkingDayCalculator : IWorkingDayCalculator
{
    public LeaveCalculation Calculate(
        Employee employee,
        DateOnly start,
        DateOnly end,
        TimeRange? partialTime,
        IReadOnlyList<ScheduleAssignment> scheduleAssignments,
        IReadOnlyList<WorkSchedule> schedules,
        IReadOnlyList<Holiday> holidays,
        IReadOnlyList<HolidayObservance> observances)
    {
        var days = new List<LeaveDay>();
        decimal working = 0;
        decimal totalHours = 0;
        var excludedRest = new List<DateOnly>();
        var excludedHolidays = new List<HolidayInfo>();
        var ignoredHolidays = new List<HolidayInfo>();

        for (var date = start; date <= end; date = date.AddDays(1))
        {
            var schedule = ScheduleFor(date, scheduleAssignments, schedules);
            var workDay = schedule?.Days.FirstOrDefault(d => d.DayOfWeek == date.DayOfWeek);
            var holiday = HolidayOn(date, employee.HolidayGroupId, holidays, observances);

            if (workDay is null)
            {
                days.Add(new LeaveDay(date, LeaveDayKind.Rest, 0, 0, $"{date.DayOfWeek} — rest day ({schedule?.Code})"));
                excludedRest.Add(date);
                if (holiday is not null)
                    ignoredHolidays.Add(holiday);
                continue;
            }

            if (holiday is { Observed: true })
            {
                var holidayEntity = holidays.First(h => h.Date == date);
                var kind = holidayEntity.Kind == "company"
                    ? LeaveDayKind.CompanyHoliday
                    : LeaveDayKind.PublicHoliday;
                days.Add(new LeaveDay(date, kind, 0, 0, holiday.Name));
                excludedHolidays.Add(holiday);
                continue;
            }

            if (holiday is { Observed: false })
                ignoredHolidays.Add(holiday);

            var hrs = workDay.Hours;
            var frac = 1m;

            if (partialTime is not null)
            {
                var partialMinutes = (partialTime.To.Hour * 60 + partialTime.To.Minute)
                    - (partialTime.From.Hour * 60 + partialTime.From.Minute);
                var h = Math.Max(0, partialMinutes / 60m);
                hrs = Math.Min(h, workDay.Hours);
                frac = Math.Round(hrs / workDay.Hours * 100) / 100;
            }

            var label = partialTime is not null
                ? $"{partialTime.From:HH:mm}–{partialTime.To:HH:mm}"
                : $"{workDay.Start:HH:mm}–{workDay.End:HH:mm}";

            days.Add(new LeaveDay(date, LeaveDayKind.Work, hrs, frac, label));
            working += frac;
            totalHours += hrs;
        }

        working = Math.Round(working * 100) / 100;

        return new LeaveCalculation(days, working, totalHours, excludedRest, excludedHolidays, ignoredHolidays);
    }

    private static WorkSchedule? ScheduleFor(
        DateOnly date,
        IReadOnlyList<ScheduleAssignment> assignments,
        IReadOnlyList<WorkSchedule> schedules)
    {
        var assignment = assignments
            .Where(a => a.ValidFrom <= date && (a.ValidTo is null || date <= a.ValidTo))
            .OrderByDescending(a => a.ValidFrom)
            .FirstOrDefault();

        assignment ??= assignments.FirstOrDefault();

        if (assignment is null)
            return null;

        return schedules.FirstOrDefault(s => s.Id == assignment.ScheduleId);
    }

    private static HolidayInfo? HolidayOn(
        DateOnly date,
        HolidayGroupId groupId,
        IReadOnlyList<Holiday> holidays,
        IReadOnlyList<HolidayObservance> observances)
    {
        var holiday = holidays.FirstOrDefault(h => h.Date == date);
        if (holiday is null)
            return null;

        var observance = observances.FirstOrDefault(o => o.HolidayId == holiday.Id && o.GroupId == groupId);
        var observed = observance?.Observed ?? false;

        return new HolidayInfo(date, holiday.Name, observed);
    }
}
