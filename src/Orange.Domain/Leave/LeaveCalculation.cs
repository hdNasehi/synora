namespace Orange.Domain.Leave;

public sealed record LeaveCalculation(
    IReadOnlyList<LeaveDay> Days,
    decimal WorkingDays,
    decimal TotalHours,
    IReadOnlyList<DateOnly> ExcludedRestDays,
    IReadOnlyList<HolidayInfo> ExcludedHolidays,
    IReadOnlyList<HolidayInfo> IgnoredHolidays);

public sealed record HolidayInfo(DateOnly Date, string Name, bool Observed);
