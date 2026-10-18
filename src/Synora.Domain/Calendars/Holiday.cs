using Synora.Domain.Common;

namespace Synora.Domain.Calendars;

public sealed class Holiday : Entity<HolidayId>
{
    public DateOnly Date { get; set; }
    public string Name { get; set; } = "";
    public string Kind { get; set; } = "";
}
