# 02 — Architecture

**Modular monolith**: one deployable ASP.NET Core app (Blazor Interactive Server) with strict internal boundaries. No microservices.

## Projects

```
src/Orange.Domain
  Common/            Entity, AggregateRoot, ValueObject, Result, DomainEvent, strongly-typed IDs
  Employees/         Employee, Team, Department, Assignment (supervisors, rule, manager; ValidFrom/ValidTo),
                     ScheduleAssignment, PolicyAssignment, Role
  Leave/             LeaveRequest (aggregate), ApprovalStep, ApprovalDecision, LeaveType, RequestStatus,
                     LeaveCalculation (value object: list of LeaveDay)
  Calendars/         WorkSchedule (+ WorkDay per weekday), Holiday, HolidayGroup, HolidayObservance
  Balances/          LeavePolicy, BalanceAdjustment (kind: CarryOver | Adjustment), LeaveBalance (computed VO)
  Services/          WorkingDayCalculator, ApprovalRouteBuilder, BalanceCalculator, AvailabilityChecker
src/Orange.Application
  Abstractions/      IAppDbContext (or repositories), ICurrentUser, IAuditLog, IOutbox, INotificationSender
  Requests/          Submit, SaveDraft, Cancel, Approve, Reject, GetMyRequests, GetRequestDetails, PreviewCalculation
  Approvals/         GetInbox, GetFinalApprovals
  Employees/         Create, Update, AssignSupervisors, AssignSchedule, List, Get
  Workflows/         Templates, Substitutes
  Policies/          Update policy, Leave types
  Calendars/         Schedules CRUD, Holidays CRUD, SetObservance
  Reports/           Balances, Requests, ByDepartment, AuditLog
  Calendar/          TeamCalendar query (respects sick-leave privacy)
src/Orange.Infrastructure
  Persistence/       OrangeDbContext, Configurations/, Migrations/, Seed/ (from reference/prototype/data.js)
  Audit/             AuditLog writer (same transaction via SaveChanges interceptor or explicit)
  Outbox/            OutboxMessage table + OutboxProcessor (BackgroundService)
  Teams/             GraphShiftsClient, TimeOffSyncHandler
  Notifications/     Email sender
src/Orange.Web
  Components/Layout/ MainLayout (sidebar by role, top bar, locale switch, RTL provider)
  Components/Pages/  one folder per screen (Dashboard, RequestLeave, MyRequests, Inbox, FinalApprovals,
                     ManagerOverview, TeamCalendar, Employees, Workflows, Policies, Reports, Access)
  Components/Shared/ StatusBadge, LeaveTypeTag, BalanceCard, ApprovalTimeline, DateRangePicker (calendar-aware),
                     CalendarGrid, EmptyState, ErrorState, LoadingState
  Localization/      SharedResource.resx, SharedResource.fa-IR.resx
  Auth/              policies, resource handlers
```

## Patterns

- **Use cases** as command/query records + handlers (MediatR or plain handlers registered in DI — pick one and stay consistent). Validators with FluentValidation run in a pipeline behaviour.
- **Result pattern** for expected failures (validation, permission, business rule) → UI shows message; exceptions only for bugs.
- **Aggregates:** `LeaveRequest` owns its `ApprovalStep`s and decisions. Load and save as a unit. Concurrency via `rowversion`.
- **Domain events** (e.g. `LeaveRequestApproved`) are collected on aggregates and turned into outbox messages + audit entries in `SaveChanges`.
- **Read side:** reports and calendar use projection queries (`AsNoTracking`, `Select` into DTOs). Balances are computed on read; add a cached projection only if needed.
- **Authorization:** ASP.NET Core policies per role + resource-based handlers (`CanActOnRequest`, `CanViewSickLeaveType`). Handlers re-check; the UI only hides.
- **Time:** inject `TimeProvider`; Development and tests pin 2026-10-19.

## Data model (core tables)

`Employees`, `Teams`, `Departments`, `EmployeeAssignments` (supervisor ids JSON or join table, Rule, ManagerId, ValidFrom, ValidTo), `ScheduleAssignments`, `PolicyAssignments`, `WorkSchedules`, `WorkScheduleDays`, `Holidays`, `HolidayGroups`, `HolidayObservances` (HolidayId, GroupId, Observed), `LeaveTypes`, `LeavePolicies`, `LeaveRequests` (Status, Amount, Unit, Start, End, Partial times, Reason, RowVersion, CalculationSnapshot JSON), `ApprovalSteps` (Order, Kind, Rule, State, ApproverIds), `ApprovalDecisions`, `BalanceAdjustments`, `Substitutes`, `AuditLog`, `OutboxMessages`, `TeamsMappings`.

## Cross-cutting

- **Localisation:** `RequestLocalizationOptions` with en and fa-IR; culture cookie; `<html dir>` set from culture; MudBlazor `MudRTLProvider RightToLeft="@isRtl"`.
- **Calendar display:** a `DateFormatter` service picks `GregorianCalendar` or `PersianCalendar` (user setting) and formats with Persian digits when culture is fa-IR.
- **Logging:** Serilog → console + file/Seq; OpenTelemetry traces; health checks for DB and Graph.
- **Security:** Entra ID (OIDC), roles from app roles or groups; anti-forgery; no PII in logs.
