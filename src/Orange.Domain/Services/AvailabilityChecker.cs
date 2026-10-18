using Orange.Domain.Common;
using Orange.Domain.Leave;

namespace Orange.Domain.Services;

public sealed class AvailabilityChecker : IAvailabilityChecker
{
    public AvailabilityWarning? Check(
        TeamId teamId,
        DateOnly start,
        DateOnly end,
        IReadOnlyList<LeaveRequest> teamRequests,
        int teamSize,
        int minPresent)
    {
        var overlapping = teamRequests
            .Where(r => r.Status is RequestStatus.Approved
                or RequestStatus.PendingSupervisor
                or RequestStatus.PendingManager)
            .Where(r => r.StartDate <= end && start <= r.EndDate)
            .ToList();

        var peak = 0;
        DateOnly? peakDate = null;
        var allAbsent = new HashSet<EmployeeId>();

        for (var date = start; date <= end; date = date.AddDays(1))
        {
            var absentIds = overlapping
                .Where(r => r.StartDate <= date && date <= r.EndDate)
                .Select(r => r.EmployeeId)
                .Distinct()
                .ToList();

            if (absentIds.Count > peak)
            {
                peak = absentIds.Count;
                peakDate = date;
            }

            foreach (var id in absentIds)
                allAbsent.Add(id);
        }

        var present = teamSize - peak - 1;

        if (present < minPresent && peakDate.HasValue)
        {
            return new AvailabilityWarning(
                peakDate.Value,
                peak,
                minPresent,
                allAbsent.ToList());
        }

        return null;
    }
}
