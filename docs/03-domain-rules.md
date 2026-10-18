# 03 — Domain rules (port from `reference/prototype/engine.js`)

## Working-day calculation

Input: employee, start date, end date, optional partial-day times. For **each date** in [start, end]:

1. Find the **schedule effective on that date** (employee's schedule assignments).
2. If the weekday is not a working day in that schedule → `Rest` (excluded).
3. Else if a holiday exists on that date **and** the employee's holiday group observes it → `PublicHoliday` / `CompanyHoliday` (excluded).
4. Else → `Work`: full day = 1 day / schedule hours; partial = hours between from/to (must be within the day's working hours) ÷ day hours.

Output: list of `LeaveDay { Date, Kind, Hours, Fraction, Label }`, total working days, total hours, excluded rest days, excluded holidays, holidays that exist but aren't observed (show as "not a rest day for you").

Amount stored on the request = days for day-based policies, hours for hour-based policies.

Validation:
- end < start → error · span > 62 calendar days → error (sample) · partial must be single date · partial end ≤ start → error · times outside working hours → error · zero working days → error.
- Annual: start in the past → error; less than min notice (2 days sample) → error; amount > available → error; overlaps own approved/pending request → error.
- Sick: start after today → error. Unpaid: reason required.

### Test cases (must pass)

| Case | Employee / schedule | Dates | Expected |
|---|---|---|---|
| Weekend excluded | Sanne, A (Mon–Fri) | Thu 12 Nov – Mon 16 Nov 2026 | 3 days; Sat 14, Sun 15 excluded |
| Company day excluded | Joris, A | Mon 2 – Fri 6 Nov 2026 | 4 days (Fri 6 Nov = Noordkade Day) |
| Ascension + bridge day | Sanne, A | Mon 11 – Fri 15 May 2026 | 3 days |
| Sun–Thu schedule | Omar, B (Sun–Thu) | Thu 5 – Sun 8 Nov 2026 | 2 days (Thu, Sun) |
| Holiday not observed | Sanne, A | Good Friday 3 Apr 2026 inside 1–3 Apr | 3 days (Good Friday counts) |
| Liberation Day toggled | Sanne, A | Mon 3 – Fri 7 May 2027 | 4 days observed=false; 3 days observed=true (Ascension 6 May always excluded) |
| Partial day | Sanne, A | Fri 18 Sep 2026 13:30–17:30 | 0.5 day, 4 h |
| Part-time hours policy | Femke, P (Mon–Thu 8 h) | Mon 7 – Fri 11 Dec 2026 | 32 h (Fri rest) |
| Schedule change mid-range | Femke, P until 30 Nov, then Mon–Thu 9 h from 1 Dec | Mon 7 – Fri 11 Dec 2026 | 36 h |

## Approval routing (built at submit, snapshotted)

From the employee's assignment effective on submit date:
- Rule **any**: one supervisor step with all supervisors; done when any one approves.
- Rule **all**: one step; done when every supervisor has approved (any order).
- Rule **sequential**: one step per supervisor in configured order.
- Rule **none**: no supervisor step.
- Always a final **manager** step last. Sick leave: single "supervisor notified" step, auto-done.

Acting: only approvers of the **active** step who haven't decided yet, or an active **substitute** for one of them (decision stored with `OnBehalfOf`). Nobody approves their own request.
Reject: any approver on the active step; reason ≥ 10 chars; step → Rejected, later steps → Skipped.

### Status (derived)

Draft (not submitted) · Cancelled · Rejected (any step rejected) · Pending Supervisor (active step is supervisor) · Pending Manager (active step is manager) · Approved (all steps done).

Cancellation: employee may cancel while pending, or when approved and start date is in the future (returns days). Past/started approved leave → HR adjustment only.

### Test cases

- Sanne (all: Daan, Priya): Daan approves → still Pending Supervisor (1/2); Priya approves → Pending Manager; Eva approves → Approved.
- Joris (any: Daan, Priya): Daan approves → Pending Manager.
- Ahmed (sequential: Bram, Priya): Priya can't act until Bram approved.
- Daan absent 28 Dec–1 Jan, substitute Priya: Priya can act on Daan's step; decision recorded on behalf of Daan.
- Reject without reason → domain error; with reason → Rejected, manager step Skipped, reservation released.
- Final manager listed as supervisor → validation error on assignment.

## Balance

```
Entitlement   = policy.Entitlement × FTE (pro-rated for joiners/leavers — TODO(rule-5))
CarryOver     = Σ adjustments(kind=CarryOver, year)
Adjustments   = Σ adjustments(kind=Adjustment, year)
Approved      = Σ amount of Approved annual requests in year   (Taken = end < today; Upcoming = rest)
Pending       = Σ amount of PendingSupervisor/PendingManager annual requests in year
Available     = Entitlement + CarryOver + Adjustments − Approved − Pending
```

Sanne 2026 check: 25 + 3 + 1 − 17.5 − 2 = **9.5**. After submitting 12–16 Nov (3 days): pending 5, available **6.5**. Approving it: approved 20.5, pending 2, available still 6.5.

Sick and unpaid never touch the annual balance. Drafts don't reserve.

## Availability (warning only)

For each date of the request, count other active team members with approved or pending leave that day; `present = teamSize − maxAbsent − 1`. Warn if `present < team.MinPresent`. Show the peak day and who else is off.

## SLA

Working days (Mon–Fri) from submission to today > 3 (configurable) → "waiting too long".
