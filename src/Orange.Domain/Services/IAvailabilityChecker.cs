using Orange.Domain.Common;
using Orange.Domain.Leave;

namespace Orange.Domain.Services;

public sealed record AvailabilityWarning(
    DateOnly PeakDate,
    int AbsentCount,
    int MinPresent,
    IReadOnlyList<EmployeeId> OthersOff);

public interface IAvailabilityChecker
{
    AvailabilityWarning? Check(
        TeamId teamId,
        DateOnly start,
        DateOnly end,
        IReadOnlyList<LeaveRequest> teamRequests,
        int teamSize,
        int minPresent);
}
