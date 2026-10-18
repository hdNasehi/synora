using Synora.Domain.Common;

namespace Synora.Domain.Leave;

public sealed record LeaveDay(
    DateOnly Date,
    LeaveDayKind Kind,
    decimal Hours,
    decimal Fraction,
    string Label);
