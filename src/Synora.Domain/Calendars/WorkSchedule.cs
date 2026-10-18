using Synora.Domain.Common;

namespace Synora.Domain.Calendars;

public sealed class WorkSchedule : Entity<WorkScheduleId>
{
    public string Code { get; set; } = "";
    public string Name { get; set; } = "";

    private readonly List<WorkDay> _days = [];
    public IReadOnlyList<WorkDay> Days => _days.AsReadOnly();

    public void AddDay(WorkDay day) => _days.Add(day);
}

public sealed record WorkDay(
    DayOfWeek DayOfWeek,
    TimeOnly Start,
    TimeOnly End,
    decimal Hours);
