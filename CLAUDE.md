# Orange — Leave Management System

Internal employee leave management for a company with offices in the Netherlands (sample company: Noordkade B.V.). Four roles: Employee, Supervisor, Manager, HR admin. UI in English and Persian (RTL), dates in Gregorian or Solar Hijri.

A working, clickable prototype exists: open `reference/prototype/synora-prototype.html` in a browser (add `#fa` for Persian). It is the **source of truth for UX and behaviour**. `reference/prototype/engine.js` is the **source of truth for business rules** — port it, don't reinvent it.

Read these before any non-trivial change:
- `docs/01-product-spec.md` — what the product must do
- `docs/02-architecture.md` — solution structure and patterns (follow strictly)
- `docs/03-domain-rules.md` — leave calculation, approval routing, balances, with test cases
- `docs/04-ui-guidelines.md` — design tokens, MudBlazor mapping, RTL/Persian
- `docs/05-teams-shifts-sync.md` — Microsoft Teams Shifts integration
- `docs/06-roadmap.md` — milestones; work through them in order
- `docs/07-open-questions.md` — unresolved business rules; use the sample answer, mark it `// TODO(rule-N)`

## Stack

- .NET 9, C# 13, nullable enabled, `TreatWarningsAsErrors` on
- Blazor Web App, **Interactive Server** render mode
- MudBlazor (UI), FluentValidation, EF Core 9 + SQL Server
- Microsoft Entra ID for auth (ASP.NET Core Identity in Development)
- xUnit + FluentAssertions; Testcontainers for integration tests
- Serilog; background work via `BackgroundService` + outbox table
- Localisation: `IStringLocalizer` + `.resx` (en, fa-IR)

## Solution layout

```
src/
  Orange.Domain/          pure C#: entities, value objects, domain services. No EF, no ASP.NET references.
  Orange.Application/     use cases by feature folder (command/query + handler + validator), interfaces for infrastructure
  Orange.Infrastructure/  EF Core DbContext + configurations + migrations, Graph (Teams), email, outbox processor
  Orange.Web/             Blazor app, MudBlazor, auth, localisation, DI composition root
tests/
  Orange.Domain.Tests/
  Orange.Application.Tests/
  Orange.IntegrationTests/
```

Dependency direction: Web → Application → Domain; Infrastructure → Application/Domain. Domain depends on nothing.

## Commands

```bash
dotnet build
dotnet test
dotnet run --project src/Orange.Web
dotnet ef migrations add <Name> -p src/Orange.Infrastructure -s src/Orange.Web
dotnet ef database update -p src/Orange.Infrastructure -s src/Orange.Web
```

## Non-negotiable rules

1. **Balances are computed, never stored.** `Available = Entitlement + CarryOver + Adjustments − Approved − Pending`. A pending request is a reservation; approval moves it to consumed. Never deduct twice.
2. **Rest days and observed holidays are excluded** using the schedule effective *on each date* and the employee's *holiday group*. A public holiday is not automatically a rest day.
3. **Request status changes only through domain methods** (`Submit`, `Approve(by, comment)`, `Reject(by, reason)`, `Cancel(by, reason)`). No public setters on `Status`.
4. **Rejection requires a reason** (min 10 chars) — enforced in the domain, not only in the UI.
5. **Snapshot on submit:** store the day-by-day calculation and the approval route on the request. Later config changes never rewrite in-flight or historic requests.
6. **Final manager approval is always the last step.** A final manager can't also be a supervisor in the same route.
7. **Effective dating:** schedule, supervisors/rule/manager and policy assignments are rows with `ValidFrom`/`ValidTo`, never overwritten columns.
8. **Audit everything** (approvals, config changes, balance adjustments) in the same transaction, append-only.
9. **Authorization on the server**, resource-based (e.g. "can act on the active step"). Hiding a menu item is not security.
10. **Sick leave privacy:** colleagues see "Absent"; only the employee, their supervisors, managers and HR see the type. Never store medical details.
11. **Dates:** `DateOnly` for leave days, `DateTimeOffset` (UTC) for timestamps. Solar Hijri / Persian is presentation only (`CultureInfo("fa-IR")`, `PersianCalendar`).
12. **Side effects via outbox** (Teams sync, email). An external outage never blocks an approval.

## Conventions

- Feature folders: `Application/Requests/Submit/SubmitLeaveRequest.cs` (record command), `...Handler.cs`, `...Validator.cs`.
- Return `Result<T>` from handlers for expected failures; throw only for bugs.
- Use `TimeProvider` (inject), never `DateTime.Now` — tests pin "today" to **Monday 2026-10-19** like the prototype.
- Strongly-typed IDs (`EmployeeId`, `LeaveRequestId`) as `readonly record struct`.
- EF configurations in `Infrastructure/Persistence/Configurations`, one per aggregate. Use `rowversion` on `LeaveRequest`.
- UI strings always through `IStringLocalizer<SharedResource>`; English text is the key. Never concatenate translated fragments — use placeholders.
- CSS: logical properties only (`margin-inline-start`, never `margin-left`) so RTL works.
- Every domain rule gets a unit test before UI work. Use the cases in `docs/03-domain-rules.md`.

## Working style (for Claude)

- Work milestone by milestone from `docs/06-roadmap.md`. At the start of a milestone, state the plan briefly; at the end, run `dotnet build` and `dotnet test` and report.
- Small, reviewable commits with clear messages.
- When a business rule is unclear, check `docs/07-open-questions.md`, use the sample answer and leave `// TODO(rule-N)`; don't invent new rules silently.
- When matching UI, open the prototype and copy its labels, states and empty/error/loading cases.
