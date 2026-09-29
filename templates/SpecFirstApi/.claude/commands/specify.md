---
description: Turn a ticket into specification PR material - EARS requirements with ids, contract changes, work-in-progress tests and a change folder
argument-hint: <ticket id or link> [pasted ticket text]
---

You are writing the specification for a change. You are not implementing it. Input: $ARGUMENTS

Read first: `docs/workflow.md`, `docs/specs/requirements/functional.md`, `docs/specs/requirements/technical.md`,
`docs/specs/api/openapi.yaml`, `docs/specs/db/schema.sql`, `docs/specs/messages/asyncapi.yaml`, and one existing folder
under `docs/specs/changes/` as the example of the shape.

1. **Understand the ticket.** If the ticket lives in a tracker you can reach, pull its description, acceptance
   criteria and comments. Extract the decisions from the discussion, not the whole thread. Acceptance criteria written
   as "situation -> what the system does" translate one line to one requirement.
2. **Write the requirements in EARS.** One sentence, one `shall`, one observable behaviour; an `and` between two actions
   is two requirements. Client-observable behaviour goes to `functional.md` as `REQ-nnn`, system-observable behaviour
   to `technical.md` as `TECH-nnn`, both continuing the sequence. Never reuse an id. For every `When`, ask whether
   there is an `If` for its failure; for every status, a `While`; for every flag or setting, a `Where`; and write at
   least one line about what must never happen. Numbers, not adjectives. Statuses by their real names.
3. **Do not guess.** Where the ticket does not answer a question, write `[NEEDS CLARIFICATION: <question>]` in the
   change's `spec.md`. CI fails while any marker remains; `/clarify` collects them for the product owner.
4. **Create the change folder** `docs/specs/changes/<TICKET>-<slug>/spec.md`: ticket link, problem statement in two
   sentences, the ids added or changed, the contracts touched, what is out of scope and where it went instead.
5. **Update the contracts.** Every new endpoint, parameter, schema, table or column goes into `openapi.yaml` and
   `schema.sql` now; every new message, topic or message property into `messages/asyncapi.yaml`. The contract may be ahead of the code; the code may never be ahead of the contract.
6. **Write the tests that prove each requirement, all failing for now.**
   - Acceptance: a scenario in `tests/SpecFirst.Service.Tests.Spec/Acceptance/*.feature` tagged `@REQ-nnn:<hash>`, with
     `@wip` alone on the next line. Example data comes from the ticket. Step definitions you need are part of the spec.
   - Integration: a fact in `tests/SpecFirst.Service.Tests.Spec/Integration/` with `[Requirement("TECH-nnn", "<hash>")]`
     and `[Trait("Category", "wip")]` each on its own line.
   - Ubiquitous requirements: a CsCheck property test in `tests/SpecFirst.Service.Tests.Spec/Invariants/`.
   - A requirement that no test can verify is tagged `@no-test` on its heading; use it rarely and say why.
7. **Hashes.** Use the `requirement-hash` skill: `python3 .claude/skills/requirement-hash/scripts/hash.py REQ-nnn`
   prints the value for the tag or attribute; `--check` compares every marker with the requirements. The hash is the
   first eight hex characters of SHA-256 over the requirement text, trimmed, whitespace collapsed; the traceability
   guardrail uses the same algorithm.
8. **Verify.** `dotnet test --filter "Category!=wip"` must be green (traceability, architecture, existing behaviour).
   Then `dotnet test --filter "Category=wip"`: the new tests must fail because the behaviour is missing, not because a
   step or a fixture is broken. Lint the contract with Spectral.
9. **Review before opening.** Ask the `spec-reviewer` subagent to review the change folder and address every
   finding; it checks what the guardrails cannot (observable, precise, complete, single, placed, contracted, proven).
   Repeat until it answers "Ready for review".
10. **Open a draft PR labelled `spec`.** Title `spec(<area>): <ticket> <summary>`. Body: why, the ids, the contracts
   touched, the open clarifications. Someone other than the author reviews it. Do not implement anything.
