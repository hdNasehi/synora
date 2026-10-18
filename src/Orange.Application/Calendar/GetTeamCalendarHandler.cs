using Microsoft.EntityFrameworkCore;
using Orange.Application.Abstractions;
using Orange.Domain.Common;
using Orange.Domain.Leave;

namespace Orange.Application.Calendar;

public sealed class GetTeamCalendarHandler
{
    private readonly IAppDbContext _db;
    private readonly ICurrentUser _currentUser;

    public GetTeamCalendarHandler(IAppDbContext db, ICurrentUser currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<TeamCalendarEntry>> HandleAsync(
        GetTeamCalendarQuery query, CancellationToken ct = default)
    {
        var teamEmployees = await _db.Employees
            .Where(e => e.TeamId == query.TeamId && e.IsActive)
            .AsNoTracking()
            .ToDictionaryAsync(e => e.Id, e => e.FullName, ct);

        var empIds = teamEmployees.Keys.ToList();

        var requests = await _db.LeaveRequests
            .Include(r => r.Steps)
            .Where(r => empIds.Contains(r.EmployeeId) &&
                        r.StartDate <= query.To &&
                        r.EndDate >= query.From)
            .AsNoTracking()
            .ToListAsync(ct);

        var actorId = _currentUser.EmployeeId;
        var canSeeSickDetails = _currentUser.IsInRole(AppRole.HrAdmin) ||
                                _currentUser.IsInRole(AppRole.Manager) ||
                                _currentUser.IsInRole(AppRole.Supervisor);

        return requests
            .Where(r => r.Status is RequestStatus.Approved or RequestStatus.PendingManager or RequestStatus.PendingSupervisor)
            .Select(r =>
            {
                // Sick leave privacy: hide type from colleagues who aren't privileged
                var showType = r.Type != LeaveType.Sick ||
                               r.EmployeeId == actorId ||
                               canSeeSickDetails;

                return new TeamCalendarEntry(
                    r.EmployeeId,
                    teamEmployees.TryGetValue(r.EmployeeId, out var name) ? name : "",
                    r.StartDate, r.EndDate,
                    showType ? r.Type : null,
                    r.Status);
            })
            .OrderBy(e => e.StartDate)
            .ThenBy(e => e.EmployeeName)
            .ToList();
    }
}
