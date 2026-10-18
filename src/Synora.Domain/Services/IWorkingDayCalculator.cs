using Synora.Domain.Calendars;
using Synora.Domain.Employees;
using Synora.Domain.Leave;

namespace Synora.Domain.Services;

public interface IWorkingDayCalculator
{
    LeaveCalculation Calculate(
        Employee employee,
        DateOnly start,
        DateOnly end,
        TimeRange? partialTime,
        IReadOnlyList<ScheduleAssignment> scheduleAssignments,
        IReadOnlyList<WorkSchedule> schedules,
        IReadOnlyList<Holiday> holidays,
        IReadOnlyList<HolidayObservance> observances);
}
