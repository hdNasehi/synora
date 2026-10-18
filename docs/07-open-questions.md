# Unresolved business rules

These need a decision from HR, Legal or Payroll before the product is built. The prototype uses the sample answer in brackets.

## Entitlement

1. What does **“one month per year”** mean exactly: 20, 22, 25 working days, or a number of hours? Does it differ by contract or CAO? *(25 working days, pro-rated by FTE.)*
2. Should balances be kept in **days or hours** for everyone? Hours are more accurate for part-time and irregular schedules. *(Days for full-time, hours for part-time.)*
3. How is the Dutch split between **statutory** (wettelijke) and **extra-statutory** (bovenwettelijke) days handled? Statutory days usually expire six months after the year ends, extra-statutory days after five years. Should the system track them as separate buckets and consume the expiring bucket first? *(Not modelled; one bucket with a single carry-over limit.)*
4. Is leave **accrued monthly** with advance booking allowed, or granted upfront on 1 January? What happens to an advance balance when someone leaves? *(Monthly accrual, advance allowed.)*
5. How are **joiners, leavers and FTE changes** pro-rated mid-year, and is rounding to half days or whole hours? *(By FTE; no rounding rule.)*
6. Do **long-service days**, birthday days or other extra days exist, and are they automatic or manual adjustments? *(Manual adjustment with a reason.)*

## Sick leave

7. Does sick leave need **any approval**, or only registration? Who may register it on someone's behalf? *(Registration; supervisor notified.)*
8. Is sickness during **approved annual leave** converted back into balance (common in the Netherlands)? What evidence is needed? *(Not modelled.)*
9. Should the system connect to the **occupational health service** (arbodienst) workflow, or stay out of it? Which data may HR store under the GDPR? *(Out of scope; no medical data stored.)*
10. Are **partial sick days** and return-to-work percentages needed? *(Full and partial days only.)*

## Unpaid leave

11. Is there a **maximum** amount of unpaid leave, and does it need HR or Payroll approval in addition to the manager? *(No maximum; supervisor + manager.)*
12. Does unpaid leave affect **accrual** of annual leave or pension? *(Not modelled.)*

## Schedules and holidays

13. Which **Dutch public holidays** are company rest days, and for which employee groups? Is **Liberation Day** observed every year or only in lustrum years? Is **Good Friday** a day off? *(Liberation Day lustrum only; Good Friday not observed.)*
14. What happens when a public holiday falls on someone's **rest day**: nothing, or a compensating day? *(Nothing.)*
15. Does the Sun–Thu support desk observe **Dutch holidays**, other holidays, or both? *(Dutch holidays; the bridge day isn't observed.)*
16. Can a schedule have **different hours per day** (for example a short Friday)? *(Same hours every working day.)*
17. When a schedule or holiday rule changes, should already **approved future leave be recalculated**, and who confirms the new amount? *(Not recalculated.)*

## Language and calendar

31. Should there be an **Iranian holiday calendar** (Nowruz, Tasua and Ashura, and the other official solar and lunar holidays) as a holiday group, and for which employees? *(Not included; Dutch and company holidays only.)*
32. Should the **leave year** run on the Solar calendar (1 Farvardin – 29/30 Esfand) for employees who use it? *(Gregorian January–December for everyone.)*
33. Should the default language and calendar be **saved per user**, or set by the organisation? *(Chosen per session in the top bar.)*

## Approval workflow

18. Under **all supervisors must approve**, does one rejection end the request, or can others still approve? *(One rejection ends it.)*
19. Can a request be **approved partially** (some of the days)? *(No.)*
20. What happens when a supervisor or manager **leaves or is inactive**: automatic reassignment, substitute, or escalation to HR? *(Substitute set by HR.)*
21. Is there an **SLA and escalation**: after how many working days, and to whom? *(3 working days; final manager notified.)*
22. Can approvers **edit** a request, or only approve or reject? *(Approve or reject only.)*
23. Who approves the leave of **managers and the managing director**? *(Their own final manager; the MD has none, so this needs a rule.)*
24. Are requests submitted before an assignment change kept on the **old route**? *(Yes.)*

## Cancellation and corrections

25. Can employees cancel **approved** leave themselves, and up to when? Does cancellation need approval? *(Self-service until the start date, no approval.)*
26. Can leave **in the past** be corrected by the employee, or only by HR through an adjustment? *(HR only.)*
27. Should **minimum notice** and **maximum consecutive days** be hard blocks or warnings that need a justification? *(Hard blocks for notice, 62 calendar days maximum.)*

## Visibility and data

28. Which colleagues may see the **team calendar**: own team, department, or everyone? *(Own team; managers see assigned departments; HR sees all.)*
29. How long are **audit logs and requests retained**, and who can export them? *(Not defined; HR and managers can export.)*
30. Which **systems** must be integrated: payroll (e.g. AFAS, Nmbrs), Microsoft 365 calendars, Entra ID for sign-in and roles? *(None; simulated.)*
