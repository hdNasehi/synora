# 01 — Product specification

## Purpose

An internal business application for requesting, approving and tracking employee leave, with configurable approval workflows, working schedules and holiday calendars. Not a generic HR dashboard.

## Roles and permissions

| Capability | Employee | Supervisor | Manager | HR admin |
|---|---|---|---|---|
| View own balance and requests | ✓ | ✓ | ✓ | ✓ |
| Submit / cancel own requests | ✓ | ✓ | ✓ | ✓ |
| Team calendar (own team) | ✓ | ✓ | ✓ | ✓ |
| See colleagues' sick-leave type | — | own reports | assigned depts | ✓ |
| Approve / reject — supervisor step | — | ✓ | ✓ | — |
| Final approval | — | — | ✓ | — |
| Reports | — | — | assigned depts | ✓ |
| Manage employees and assignments | — | — | — | ✓ |
| Configure workflows, policies, schedules, holidays | — | — | — | ✓ |
| Balance adjustments (with reason) | — | — | — | ✓ |
| Audit log | — | — | own decisions | ✓ |
| Sick-leave attachments | — | — | — | ✓ |

Supervisors and managers are employees too: they keep a "My leave" section.

## Leave types

| Type | Uses annual balance | Approval | Notes |
|---|---|---|---|
| Annual leave | Yes | Workflow (per employee) | Min notice (sample 2 days), max 62 calendar days per request |
| Sick leave | No | Registered; supervisor notified | Today or earlier only. Optional attachment, HR-only. Shown as "Absent" to colleagues |
| Unpaid leave | No | Workflow | Reason required |

## Entitlement (configurable — sample values)

- Business wording: "one month per year". **Do not assume 30 calendar days.** Sample: 25 working days (full-time), pro-rated by FTE; 160 hours for the hours-based part-time policy.
- Unit per policy: working days or hours. Always shown with the balance.
- Accrual: monthly (info only), advance booking up to full-year entitlement.
- Carry-over: max 5 days / 40 h, expires 1 July next year.
- Balance view shows: entitlement, accrued, carry-over, adjustments, approved (taken / upcoming), pending reservations, available.

## Working schedules

- Weekly patterns with hours per working day, e.g. A: Mon–Fri 09:00–17:30 (8 h), B: Sun–Thu 08:00–16:30, P: Mon–Thu part-time.
- Custom schedules; assigned to employees with effective start/end dates.

## Holidays

- Dutch public holidays + company holidays (e.g. bridge day, company day).
- Each holiday has an observed/not-observed switch **per holiday group** (e.g. NL office staff, international support desk).
- Samples: Good Friday and Liberation Day (non-lustrum years) are working days; bridge day after Ascension is a rest day for the office only.

## Approval workflow

- Per employee: 0–n supervisors, a rule (any one / all / sequential / none) and exactly one final manager, effective-dated.
- Route: submit → supervisor step(s) → final manager → approved and recorded.
- Statuses: Draft, Pending Supervisor Approval, Pending Manager Approval, Approved, Rejected, Cancelled.
- Rejection requires a reason; remaining steps are skipped; reservation released.
- Substitute approvers for date ranges; decisions logged "on behalf of".
- SLA flag: waiting > 3 working days (sample) per step.
- Team availability warning when approving would drop a team below its minimum headcount (warning, not blocker).

## Screens (see prototype)

A. Employee dashboard · B. New leave request (live day-by-day calculation, balance before/after, approval route preview, validation) · C. My requests · D. Supervisor approval inbox · E. Manager dashboard · F. Team calendar (month/week, schedules, holidays, privacy) · G. Employee management · H. Approval workflow configuration · I. Leave policies, schedules and holiday calendar · J. Reports and audit log · Access & roles.

Every list has empty, loading and error states. Mobile layout (< 760 px) for employee requests and approvals.

## Localisation

- English and Persian (RTL, Persian digits, Vazirmatn font).
- Gregorian or Solar Hijri dates, independent of language; Persian defaults to Solar. Week starts Saturday (Solar/Persian) or Monday (Gregorian/English).
- Storage stays ISO; switching never changes a calculation.

## Integrations

- Microsoft Teams Shifts: push approved leave as time off; optionally receive Shifts time-off requests via workforce integration. See `05-teams-shifts-sync.md`.
- Email/Teams notifications (later milestone).

## Prototype personas and sample data

Today = **Mon 19 Oct 2026**. Sanne de Vries (employee, Schedule A, supervisors Daan Bakker + Priya Raman "all required", manager Eva Jansen), Daan Bakker (supervisor), Eva Jansen (manager), Lotte Visser (HR). Full sample data: `reference/prototype/data.js` — use it as the development seed.
