# 05 — Microsoft Teams Shifts sync

**Synora is the source of truth. Shifts is a mirror.**

## Push (required)

On `LeaveRequestApproved` → outbox message → `TimeOffSyncHandler` creates a `timeOff` on the employee's team schedule via Microsoft Graph. Store the returned id on the request (`ShiftsTimeOffId`). On cancel/correction → update or delete it.

```csharp
var created = await graph.Teams[teamsId].Schedule.TimesOff.PostAsync(new TimeOff
{
    UserId = employee.EntraUserId,
    SharedTimeOff = new TimeOffItem
    {
        TimeOffReasonId = reasonMap[request.Type],   // sick leave → generic "Absent" reason
        StartDateTime = request.StartUtc,
        EndDateTime   = request.EndUtc,
        Theme = ScheduleEntityTheme.Blue
    }
});
```

## Pull (optional, later)

Register a `workforceIntegration` so Shifts calls our webhook when a user submits a `timeOffRequest` in Teams. Create the request in Synora, run our workflow, then reply with the decision.

## Mapping (HR settings screen "Microsoft Teams")

| Synora | Shifts |
|---|---|
| Team | Teams team id (one schedule per team) |
| Employee | Entra ID user object id |
| Leave type | timeOffReason id |
| Days | UTC start/end date-time (full days → 00:00 to 00:00 next day in the employee's time zone) |

## Notes

- App registration with application permission `Schedule.ReadWrite.All` (admin consent).
- App-only Shifts calls historically required the `MS-APP-ACTS-AS` header; Microsoft announced its deprecation in June 2025 — check current docs before implementing: https://devblogs.microsoft.com/microsoft365dev/deprecation-of-ms-app-acts-as-header-in-shifts-management-microsoft-graph-apis/
- Never recalculate day counts from Shifts. Retries with exponential backoff; dead-letter after N attempts; show sync status on the request ("Synced to Teams" / "Sync failed — retrying").
- Docs: https://learn.microsoft.com/en-us/graph/api/resources/schedule · https://learn.microsoft.com/en-us/graph/api/resources/timeoffrequest · https://learn.microsoft.com/en-us/graph/api/workforceintegration-post
