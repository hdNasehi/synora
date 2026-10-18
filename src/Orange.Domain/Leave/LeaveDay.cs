using Orange.Domain.Common;

namespace Orange.Domain.Leave;

public sealed record LeaveDay(
    DateOnly Date,
    LeaveDayKind Kind,
    decimal Hours,
    decimal Fraction,
    string Label);
