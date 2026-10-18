using Orange.Domain.Common;
using Orange.Domain.Leave;

namespace Orange.Application.Calendar;

public sealed record GetTeamCalendarQuery(
    TeamId TeamId,
    DateOnly From,
    DateOnly To);

public sealed record TeamCalendarEntry(
    EmployeeId EmployeeId,
    string EmployeeName,
    DateOnly StartDate,
    DateOnly EndDate,
    // Sick leave type is hidden from non-privileged viewers (shown as "Absent")
    LeaveType? Type,
    RequestStatus Status);
