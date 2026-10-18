using Orange.Domain.Common;

namespace Orange.Domain.Calendars;

public sealed class HolidayGroup : Entity<HolidayGroupId>
{
    public string Name { get; set; } = "";
}

public sealed class HolidayObservance
{
    public HolidayId HolidayId { get; set; }
    public HolidayGroupId GroupId { get; set; }
    public bool Observed { get; set; }
}
