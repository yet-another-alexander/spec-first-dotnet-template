# Working in this repository

This service is developed spec-first. `docs/workflow.md` is the full picture; this file is the operating manual for an
agent session. Read both before changing anything.

## The one rule

What is correct (requirements, contracts, the tests that prove them) and what conforms to it (implementation) never
travel in the same pull request and never come from the same session. If you are implementing, you do not edit the
spec. If you are specifying, you do not implement.

## Protected paths

Implementation PRs may not change:

- `docs/specs/**`: requirements, contracts, change folders
- `tests/SpecFirst.Service.Tests.Spec/**`: acceptance, integration, invariant and guardrail tests

The one permitted change: delete a line that is exactly `@wip` (or the `[Trait("Category", "wip")]` line of a fact)
once the test passes. CI rejects any other change to these paths unless the PR carries the `spec` label. If a test
looks wrong to you, stop and say so in the PR; do not fix the test to fit the code.

## Before writing code

1. Read `docs/specs/requirements/functional.md` and `technical.md`. Every behaviour you implement has an id there. If
   the behaviour you need has no id, it is not specified: stop and ask, do not invent.
2. Read the change folder `docs/specs/changes/<change>/` you are working from: `spec.md`, `plan.md`, `tasks.md`.
   Implement one task per PR, in order.
3. Read the contracts: `docs/specs/api/openapi.yaml`, `docs/specs/db/schema.sql`, `docs/specs/messages/asyncapi.yaml`.
   Code may expose nothing the contracts do not declare. An extra endpoint, field, table, column, message or message
   property fails CI.
4. Read the failing `@wip` tests for the task. They define done.

## Layout

- `src/SpecFirst.Service.Api/`: the service. `Program.cs` is a table of contents of `Add*`/`Map*` calls; each concern
  has its own `*Setup.cs`. Features are folders (`Orders/`) holding endpoints, entities, EF configuration and pure
  logic. `Persistence/` holds the `DbContext` and the migrations.
- `src/SpecFirst.Service.Models/`: the HTTP contract as code: request and response records (`Requests/`, `Responses/`)
  and wire enums (`Enums/`). BCL only and packable, so a client takes it as a package.
- `src/SpecFirst.Service.Messages/`: the message contract as code: the records the service publishes (`Events/`) or
  consumes (`Commands/`), each with its topic. BCL and Models only, packable.
- `tests/SpecFirst.Service.Tests.Spec/`: protected. `Acceptance/` (Reqnroll, REQ over HTTP), `Integration/` (xUnit,
  TECH and the schema drift check), `Invariants/` (CsCheck property tests), `Guardrails/`.
- `tests/SpecFirst.Service.Tests.Unit/`: yours. No requirement ids, no protection.
- `.claude/hooks/`: two session hooks (`.claude/settings.json`): an edit to a protected path on a non-`spec/` branch is
  blocked unless it only deletes a wip marker; `--no-verify` and commits on the default branch are blocked. They stop
  you early; CI enforces the same rules on the PR (`docs/workflow.md` section 9).
- `.claude/rules/`: the coding rules, loaded by path (`common/` for every file, `csharp/` for `src/` and `tests/`).
  They say how to write code; the requirements say what to write. A rule never adds behaviour.

## Conventions

- EARS for every requirement: one sentence, one `shall`. Ids `REQ-nnn` (client-observable) and `TECH-nnn`
  (system-observable, testable). Decisions without a test are ADRs in `docs/adr/`.
- Time comes from `TimeProvider`; `DateTime.Now` and friends do not compile.
- Status transitions are a Stateless state machine on the entity (`Order.TryFire`); endpoints fire triggers and never
  assign a status. A new `When`/`While` on a status is a `Permit` or `Ignore` line there; a rejected transition is 409.
- Errors are `application/problem+json`; unhandled exceptions never leak their message.
- Messages are published with `ICapPublisher` inside a CAP transaction on the `DbContext`, so the outbox row commits
  with the change; never publish outside one. Message records are BCL-only like the other contracts.
- Dependencies are checked by NsDepCop at compile time: `config.nsdepcop` at the root holds the solution rules, each
  project's own `config.nsdepcop` adds what it may take. Models depends on the BCL only, Messages on the BCL and Models,
  the service on both; features never reference the composition root.
- Schema changes are EF Core delta migrations: `dotnet ef migrations add <Name> --project src/SpecFirst.Service.Api
  --output-dir Persistence/Migrations`. Never edit an applied migration.
- Formatting is CSharpier; `dotnet csharpier format .` before committing (the pre-commit hook does it for staged
  files).
- Commits follow Conventional Commits; `spec(...)` is the type for specification changes.

## Checks to run before opening a PR

```sh
dotnet csharpier check .
dotnet build -c Release
dotnet test --filter "Category!=wip"
```

For contract changes also run Spectral and oasdiff as shown in `docs/workflow.md` section 8.

## Commands

- `/specify <ticket>`: ticket to spec PR material (requirements, contracts, `@wip` tests, change folder)
- `/clarify <change>`: collect `[NEEDS CLARIFICATION]` markers and open questions, ask, resolve
- `/plan <change>`: `spec.md` to `plan.md`
- `/tasks <change>`: `plan.md` to `tasks.md`, one task per PR
- `/implement <change> <task>`: one task in a fresh session, respecting the protected paths
- `spec-reviewer` subagent (`.claude/agents/`): read-only review of a change folder before the spec PR opens; asks
  what the guardrails cannot (observable, precise, complete, single, placed, contracted, proven)
- `requirement-hash` skill (`.claude/skills/`): `hash.py REQ-nnn`, `--all`, `--check`, `--fix`; the hash for a new
  test's marker, or the stale markers rewritten after a rewording you have re-read the tests for
- `implement-change` workflow (`.claude/workflows/`): runs the open tasks of a change in order, a fresh implementer
  per task on its own branch and an independent verifier, one repair round, stops at the first failure. Only runs
  when asked for by name ("use the implement-change workflow for <change>"); costs two agents per task.

## When something is unclear

Do not guess. In a spec session, write `[NEEDS CLARIFICATION: the question]` into the change's `spec.md`; CI fails
while any marker remains, which is the point. In an implementation session, stop and report the gap: a wrong assumption
costs a comment now and a week of rework later.

---

# SpecFirst.Service

> [What this service does, why it exists, and any history that explains its current shape.]

## Service neighbors

```
[direct upstream and downstream dependencies]
```

## Domain terminology deviations

<!-- Only where this service deviates from the shared domain language. -->

## Non-obvious commands

<!-- Commands with special flags or execution order not captured in any config. -->

## Intentional workarounds

<!-- WHAT looks wrong, WHY it is that way, WHEN it can be removed. -->

## External constraints

<!-- Timeouts, rate limits, SLAs, payload limits, with the source of truth for each. -->

## Pitfalls

<!-- Non-obvious things that cause bugs: implicit behaviours, format mismatches, hidden side effects. -->

## Don'ts

<!-- Explicit guardrails, each linked to its ticket or decision. -->

## Current focus

<!-- What the team is working on or migrating toward right now. Update it; stale focus is worse than none. -->
