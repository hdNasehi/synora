# 06 — Roadmap

Work in order. Each milestone ends green: `dotnet build` and `dotnet test` pass. Copy the prompt into Claude Code (or run the matching `/m1` … `/m9` command).

## M1 — Solution skeleton
> Create the solution exactly as in docs/02-architecture.md: four src projects and three test projects, .NET 9, nullable, TreatWarningsAsErrors, Directory.Build.props and Directory.Packages.props (central package management). Add a Blazor Web App (Interactive Server) with MudBlazor configured and an empty MainLayout. Add .editorconfig, .gitignore, and a README with run instructions. Make build and test pass.

## M2 — Domain model + working-day calculator
> Implement the Domain per docs/02 and docs/03: strongly-typed IDs, Employee with effective-dated schedule/assignment/policy, WorkSchedule, Holiday, HolidayGroup with observances, and WorkingDayCalculator ported from reference/prototype/engine.js (`calculate`). Write xUnit tests for every case in the "Working-day calculation" table in docs/03. Inject TimeProvider; tests pin today to 2026-10-19.

## M3 — Leave requests, approval routing, balances
> Implement LeaveRequest aggregate with status state machine, ApprovalRouteBuilder (any/all/sequential/none + final manager, sick = notify), approve/reject/cancel with substitutes, and BalanceCalculator. Port from engine.js (`buildSteps`, `decide`, `statusOf`, `balance`, `availability`). Add all test cases from docs/03 including the Sanne balance check (9.5 → 6.5).

## M4 — Persistence + seed
> Add SynoraDbContext with configurations for all aggregates (rowversion on LeaveRequest, calculation snapshot as JSON), audit log and outbox tables, initial migration, and a development seeder that loads the sample data from reference/prototype/data.js (employees, teams, schedules, holidays 2026–2027, policies, adjustments, requests). Integration tests with Testcontainers for SQL Server.

## M5 — Application use cases + auth
> Implement use cases in Application by feature folder: SubmitLeaveRequest, SaveDraft, PreviewCalculation, CancelRequest, ApproveRequest, RejectRequest, GetMyRequests, GetRequestDetails, GetInbox, GetFinalApprovals, GetTeamCalendar (sick privacy). FluentValidation, Result<T>, audit entries, domain events → outbox. Development auth: a persona switcher (Sanne/Daan/Eva/Lotte) like the prototype; production: Entra ID. Resource-based authorization handlers.

## M6 — Employee & approver UI
> Build the Blazor pages matching the prototype: MainLayout (role sidebar, top bar), Employee Dashboard, Request Leave (live summary, validation, route preview, review checkbox), My Requests (filters, drawer), Approval Inbox, Final Approvals, Manager Overview, request drawer with ApprovalTimeline, approve/reject dialogs. Use docs/04-ui-guidelines.md. Copy labels from the prototype. Include empty/loading/error states.

## M7 — HR administration + reports
> Build Employees (list, details, edit with supervisors/rule/manager/effective date), Approval Workflows (templates, substitutes), Policies & calendars (leave types, entitlement, schedules with assign + impact preview, holiday calendar with per-group observance + preview), Access & roles, Reports & audit (balances, requests, by department, audit log, adjustments dialog), Team Calendar (month/week).

## M8 — Localisation: Persian + Solar Hijri
> Add en and fa-IR resources (import translations from reference/prototype/fa.js), culture switch in the top bar, RTL via MudRTLProvider and dir attribute, Vazirmatn font, Persian digits helper, DateFormatter (Gregorian/PersianCalendar), calendar-aware date picker and team calendar (Saturday-first weeks under Solar). Verify every page in fa-IR.

## M9 — Teams Shifts sync + notifications
> Implement docs/05-teams-shifts-sync.md: Graph client, Teams settings page (team and reason mapping), outbox handler that pushes approved leave as timeOff and updates/deletes on cancel, sync status on the request, retries. Add email notifications for submit/approve/reject.

## Afterwards
- Mobile layout polish, accessibility pass (keyboard, contrast), performance (calendar queries), deployment (Azure App Service + Azure SQL), CI (GitHub Actions: build, test, migrations bundle).
