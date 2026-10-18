using Orange.Domain.Calendars;
using Orange.Domain.Employees;
using Orange.Domain.Leave;

namespace Orange.Domain.Services;

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
