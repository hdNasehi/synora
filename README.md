# Orange — Leave Management System

Starter pack for building Orange in .NET with Claude Code.

## What's here

| Path | What it is |
|---|---|
| `CLAUDE.md` | Project brief Claude Code reads automatically: stack, architecture, non-negotiable rules, conventions |
| `docs/01-product-spec.md` | Requirements: roles, leave types, workflows, screens |
| `docs/02-architecture.md` | Modular monolith, project layout, patterns, data model |
| `docs/03-domain-rules.md` | Leave calculation, approval routing, balances — with test cases |
| `docs/04-ui-guidelines.md` | Design tokens, MudBlazor mapping, RTL/Persian |
| `docs/05-teams-shifts-sync.md` | Microsoft Teams Shifts integration |
| `docs/06-roadmap.md` | Milestones M1–M9 with ready-to-use prompts |
| `docs/07-open-questions.md` | Business rules still to confirm |
| `reference/prototype/synora-prototype.html` | The clickable prototype (open in a browser; add `#fa` for Persian) |
| `reference/prototype/engine.js` | Business rules to port (calculation, routing, balances) |
| `reference/prototype/data.js` | Sample data — use as dev seed |
| `reference/prototype/fa.js` | Persian translations (English key → Persian) |
| `reference/design-tokens.json` | Colours, type, spacing, radii (light + dark) |
| `.claude/commands/` | Slash commands: `/m1` … `/m9`, `/next`, `/check-rules` |

## Start

```bash
cd synora
git init && git add . && git commit -m "Starter pack"
claude
```

Then in Claude Code type `/m1` (or `/next`) and work through the milestones. Requirements: .NET 9 SDK, SQL Server (or Docker for Testcontainers), Git.
