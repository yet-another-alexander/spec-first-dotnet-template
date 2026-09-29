---
paths:
  - ".claude/CLAUDE.md"
  - "CLAUDE.md"
---

# CLAUDE.md Structure

> **What this rule does:** Defines the structure of `CLAUDE.md`. `CLAUDE.md` is authored and maintained by humans — Claude MUST NEVER write or modify its content. Claude may advise on structure when asked and must validate any proposed `CLAUDE.md` change against this structure before it is committed.

---

`CLAUDE.md` has two parts. The first is the operating manual of the spec-first workflow and ships with the template; it changes only when the workflow changes. The second describes this particular service and starts empty; fill it in as the service grows, and keep it short.

## Part 1 — the workflow (shipped)

- **The one rule**: spec and implementation never travel in the same PR or come from the same session.
- **Protected paths**: what implementation PRs may not touch, and the one permitted change.
- **Before writing code**: the reading order (requirements, change folder, contracts, `@wip` tests).
- **Layout**: where things are and what each folder is for.
- **Conventions**: EARS, ids, time, errors, messaging, namespaces, migrations, formatting, commits.
- **Checks to run before opening a PR**, **Commands**, **When something is unclear**.

## Part 2 — this service (fill in)

# [SERVICE-NAME]

> [What this service does, why it exists, and any history that explains its current shape — extracted from a monolith, built from scratch, inherited.]

## Service neighbors

```
[Direct upstream and downstream dependencies as a small ASCII diagram]
```

<!-- Direct dependencies only. If a live architecture diagram exists, link to it instead of duplicating it. ASCII diagrams rot fast; fewer boxes = less maintenance. -->

## Domain terminology deviations

<!-- Only where this service deviates from the shared domain language: different naming, different meaning, legacy terms not yet aligned. If a term means what the glossary says, don't list it. -->

## Non-obvious commands

<!-- Commands with special flags, argument combinations or execution order not captured in any config. If `dotnet test` just works, don't document it. -->

## Intentional workarounds

<!-- Things that look like bugs or bad code but are deliberate. Each entry: WHAT looks wrong, WHY it is that way, WHEN (if ever) it can be removed. Without this section Claude will try to "fix" these. -->

## External constraints

<!-- Timeouts, rate limits, SLAs, payload limits or other contracts this service must honor when calling or being called. Name the source of truth for each. -->

## Pitfalls

<!-- Non-obvious things that cause bugs or confusion: implicit behaviours not visible in type signatures, data format mismatches between services, easy-to-forget filters, state transitions with hidden side effects, anything where the obvious approach is wrong. -->

## Don'ts

<!-- Explicit guardrails for Claude. Short entries; link the ticket or decision record when one exists. -->

## Current focus

<!-- What the team is actively working on or migrating toward RIGHT NOW. Steers Claude away from building on things about to change. Update it regularly; stale focus is worse than none. -->
