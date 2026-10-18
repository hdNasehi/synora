# 04 — UI guidelines (Blazor + MudBlazor)

The prototype (`reference/prototype/synora-prototype.html`) defines layout, copy and states. Tokens: `reference/design-tokens.json`.

## Theme

Build a `MudTheme` from the tokens:

| Token | Light | Dark | Use |
|---|---|---|---|
| primary (Delft blue) | `#1d4f91` | `#86b0ea` | primary actions, links, focus, selection |
| accent (Dutch orange) | `#c2410c` | `#f2925a` | "today" marker, public-holiday markers only |
| success | `#1d7a45` | `#5cc68a` | Approved |
| warning | `#a35d00` | `#f0b44c` | Pending |
| danger | `#b42318` | `#f47c70` | Rejected, errors |
| canvas / surface | `#f4f5f7` / `#ffffff` | `#101317` / `#181c22` | background / cards |
| ink / ink-muted | `#161b22` / `#566170` | `#e9ecf0` / `#a2acb8` | text |
| border / border-control | `#e0e4ea` / `#8a94a3` | `#2a313a` / `#6b7684` | hairlines / inputs |

Typography: IBM Plex Sans (UI), IBM Plex Mono (IDs), **Vazirmatn** for Persian. Scale: page title 24/32 600, section 16/24 600, body 14/20, small 13/18, caption 12/16 500.
Radii: 4 (badges), 6 (controls), 10 (cards/dialogs). Spacing 4-px base. Sidebar 240 px, top bar 56 px, row 52 px.

## Components → MudBlazor

| Prototype component | Build with |
|---|---|
| Sidebar (role-based) | `MudDrawer` + `MudNavMenu`, groups per role |
| TopBar | `MudAppBar` (title, date, locale switch, notifications `MudBadge`, user) |
| StatusBadge | custom `MudChip` wrapper: fixed colour **+ icon + word** per status |
| BalanceCard | `MudPaper` + segmented progress (approved solid, pending striped) + ledger list |
| DataTable | `MudDataGrid` (row click → drawer), with empty/loading/error templates |
| Request drawer | `MudDrawer Anchor="Anchor.End"` (auto flips in RTL), 520 px |
| Dialogs | `MudDialog` — reject dialog requires reason |
| DateRangePicker | custom component over a calendar-aware month grid (Gregorian/Persian); MudBlazor picker only if it renders Solar months correctly |
| ApprovalTimeline | `MudTimeline` with custom dots: done / active / waiting / skipped / rejected; manager step tagged "Final step" |
| Team calendar | custom CSS grid (people × days), rest days hatched, holidays tinted, pending striped |
| Toast | `ISnackbar` |

## Rules

- One primary button per view. Verbs in sentence case. Destructive confirm uses a specific verb ("Reject request").
- Status never by colour alone — always icon + word. Pending bars: striped outline + clock icon.
- Every list: empty, loading (skeleton), error (retry).
- Request form: live summary panel (total, day-by-day list, excluded days, balance before → after, approval route); submit disabled until valid and "I've reviewed" ticked.
- Calendar privacy: colleagues see sick leave as "Absent".
- Mobile (< 760 px): bottom navigation, full-screen drawer, single column.

## RTL and Persian

- Culture fa-IR → `dir="rtl"` on `<html>`, `MudRTLProvider RightToLeft`, Vazirmatn font.
- Use logical CSS properties only. Mirror direction icons (chevrons, arrows).
- Persian digits: format numbers with a helper that maps 0–9 → ۰–۹ and `.` → `٫`.
- Dates: `DateFormatter` service; Solar = `new CultureInfo("fa-IR")` (uses `PersianCalendar`), e.g. `dddd d MMMM yyyy` → پنجشنبه ۲۱ آبان ۱۴۰۵. Week starts Saturday (Solar/Persian) or Monday (Gregorian/English).
- Translations: copy from `reference/prototype/fa.js` (English key → Persian) into `SharedResource.fa-IR.resx`. Keep placeholders as `{0}` or named in .NET format.
- Names, request IDs and user-entered comments stay in their original script; wrap names with `dir="auto"`.
