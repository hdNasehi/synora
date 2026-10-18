namespace Orange.Domain.Common;

public readonly record struct EmployeeId(Guid Value);
public readonly record struct LeaveRequestId(Guid Value);
public readonly record struct TeamId(Guid Value);
public readonly record struct DepartmentId(Guid Value);
public readonly record struct WorkScheduleId(Guid Value);
public readonly record struct HolidayId(Guid Value);
public readonly record struct HolidayGroupId(Guid Value);
public readonly record struct LeavePolicyId(Guid Value);
public readonly record struct ApprovalStepId(Guid Value);
public readonly record struct AuditEntryId(Guid Value);
