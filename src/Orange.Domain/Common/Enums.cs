namespace Orange.Domain.Common;

public enum RequestStatus
{
    Draft,
    PendingSupervisor,
    PendingManager,
    Approved,
    Rejected,
    Cancelled
}

public enum LeaveType { Annual, Sick, Unpaid }

public enum BalanceUnit { Days, Hours }

public enum ApprovalRule { Any, All, Sequential, None }

public enum StepKind { Supervisor, Manager, Notify }

public enum StepState { Active, Waiting, Done, Rejected, Skipped }

public enum DecisionType { Approved, Rejected }

public enum AdjustmentKind { CarryOver, Adjustment }

public enum LeaveDayKind { Work, Rest, PublicHoliday, CompanyHoliday }

public enum AppRole { Employee, Supervisor, Manager, HrAdmin }
