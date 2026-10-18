# Design decisions & assumptions

## Key design decisions

1. **Calculation before commitment.** The request form keeps a live summary beside the inputs: the total, a day-by-day list (working day, rest day, holiday), excluded days named explicitly, and *available now → after this request*. Submission requires ticking *I've reviewed the calculation*. This is the moment where most leave disputes start, so the design makes it impossible to miss.
2. **Reservation, not deduction, while pending.** Pending annual leave is shown as a striped *Pending* segment in the balance meter and a *Pending reservations* line in the ledger. Approval moves it to *Approved*; rejection or cancellation releases it. *Available* already subtracts pending, so a person can't double-book, and nothing is subtracted twice.
3. **Approval routes live on the employee, not the leave type.** Each employee has 0–n supervisors, a rule (*any one*, *all*, *sequential*, *none*) and exactly one final manager, with an effective date. Templates in *Approval workflows* define structure and service levels; the person's profile fills in the names. This lets two people on the same team have different routes, as the brief requires.
4. **The final manager is a fixed, labelled step.** It can't be removed from a template, it's always last, and the timeline tags it *Final step*. A final manager can't also be a supervisor in the same route.
5. **Sick leave is registered, not approved.** It goes straight to *Approved* with a *Supervisor notified* step, never touches the annual balance, can only be dated today or earlier, and shows as *Absent* to colleagues. The optional attachment is HR-only.
6. **Unpaid leave uses the normal workflow but no balance.** A reason is required. The form says it doesn't affect annual leave.
7. **Schedules and holidays are resolved per person, per day.** The calculation asks two questions for each date: is it a working day in the schedule that applies *on that date*, and does the person's *holiday group* observe a holiday on it? This supports Sun–Thu weeks, part-time weeks, mid-year schedule changes and groups that work on public holidays.
8. **Two holiday kinds, one rule.** Dutch public holidays and company holidays are listed together but marked **NL** (orange) and **Co** (teal). Neither is automatically a rest day: each has an observed/not-observed switch per employee group. The samples show the realistic cases: Good Friday and Liberation Day (non-lustrum years) are working days; the bridge day after Ascension is a rest day for the office but not the support desk.
9. **Waiting time is visible.** Requests waiting more than 3 working days (sample service level) get a red *n days* badge and a banner in the inbox. The manager overview counts them per stage.
10. **Staffing is a warning, not a blocker.** Each team has a minimum headcount. Approvers see *4 of 7 in* and who else is off; the manager overview lists potential conflicts. The decision stays human.
11. **Role-based navigation.** Each role gets its own sidebar; HR configuration never appears for other roles. Supervisors and managers keep their own *My leave* section because they're employees too.
12. **Restrained visual language.** One brand hue for action, status colours reserved for status, every status carrying an icon and a word, striped patterns for *pending* so the calendar works in greyscale and for colour-blind users.
13. **Language and calendar are independent settings.** Persian (right to left) and Solar Hijri dates are separate choices, because Persian-speaking staff abroad may want Gregorian dates and English speakers in Iran may want Solar dates. Storage stays ISO, so switching is purely presentational and safe.
14. **Mobile covers the daily jobs.** Under 760px: dashboard, request, my requests, calendar and the approval inbox, with a bottom tab bar and full-screen drawers. Configuration stays desktop-only.

## Assumptions

- **“One month per year”** is interpreted as **25 working days** (5 working weeks × 5 days) for full-time staff, pro-rated by FTE, and **160 hours** for the hours-based part-time policy. It's shown as a configurable value with its unit. It's not treated as 30 calendar days.
- Annual leave **accrues monthly** (shown for information) but can be **booked in advance** up to the full-year entitlement.
- **Carry-over** is capped at 5 days / 40 hours and expires on 1 July of the following year (sample values).
- A working day is **8 hours** (09:00–17:30 with a 30-minute unpaid break) unless the schedule says otherwise. Partial days are charged as a fraction of that day's hours.
- **Minimum notice** for annual leave is 2 days; requests are limited to 62 calendar days.
- **Approved future leave can be cancelled** by the employee and the days are returned. Past or started leave needs HR.
- **Rejection by any supervisor rejects the request**, including under *all supervisors must approve*.
- Requests already submitted **keep their original route** when HR changes an employee's supervisors.
- **Substitutes** act on behalf of an absent approver for a date range; their decisions are logged as *on behalf of*.
- The company, people, schedules and policies are **fictional sample data** for illustration.
