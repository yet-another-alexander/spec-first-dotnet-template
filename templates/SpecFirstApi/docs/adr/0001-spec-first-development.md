# ADR-0001: Spec-first development enforced by CI

Status: accepted. Date: 2026-09-21.

## Context

Agents write most of the code. The bottleneck is knowing the code does what was asked, and reviewing many pull
requests a day. Acceptance criteria die on merge, agents fill gaps by inventing, and rules an agent can edit are advice.

## Decision

- Requirements are the source of truth, written in EARS with stable ids in `docs/specs/requirements/`. Behaviour with
  no id is unspecified and not implemented.
- Every testable requirement has a test that references it by id and by a hash of its text. CI fails on a requirement
  without a test, an unknown id, a stale hash, or a spec with open clarifications.
- The outside shape (OpenAPI, database schema) is declared under `docs/specs/` and checked one-directionally: code may
  expose nothing the contract does not declare; the contract may be ahead of the code.
- Specification and implementation travel in different pull requests from different sessions. Implementation PRs may
  not touch `docs/specs/` or the spec tests except to delete `@wip` markers.

## Consequences

Humans review a one-page spec and a handful of tests once, then CI enforces. Every invention is visible and blocked.
For any requirement, the test that proves it and the change that introduced it are one search away. The cost is a fixed
requirement form, tests before code, and two PRs instead of one.
