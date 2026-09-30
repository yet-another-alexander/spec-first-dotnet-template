# Working in this repository

Spec-first service; `docs/workflow.md` explains how and why. The hooks, analyzers, guardrail tests and CI enforce the
mechanics. This file covers only what they cannot: how a session behaves.

## The one rule

Spec (requirements, contracts, the tests that prove them) and implementation never come from the same session.
Implementing? Don't touch the spec. Specifying? Don't implement. If a test looks wrong while implementing, say so in
the PR; never bend the code to a test you doubt without saying it.

## Before writing code

Read, in order: `docs/specs/requirements/`, the change folder `docs/specs/changes/<change>/` (`spec.md`, `plan.md`,
`tasks.md`; one task per PR, in order), the contracts under `docs/specs/`, then the failing `@wip` tests for the task.
They define done. Every behaviour has a requirement id; no id means not specified.

## When something is unclear

Don't guess. Specifying: write `[NEEDS CLARIFICATION: question]` into `spec.md`. Implementing: stop and report the
gap.
