---
name: requirement-hash
description: Compute the hash a test marker must carry for a requirement id, list every requirement with its hash, check the markers in the spec tests, or rewrite stale ones. Use in a spec session whenever a requirement is added or reworded, or when the traceability guardrail reports a missing or stale hash.
---

# Requirement hash

Every test that proves a requirement carries the requirement's id and the first eight hex characters of SHA-256 over
its normalised text (trimmed, runs of whitespace collapsed to one space); `docs/workflow.md` section 1. The
traceability guardrail fails on a marker whose hash no longer matches. This skill computes and maintains those hashes
with the same algorithm as the guardrail (`Guardrails/RequirementHash.cs`, `Guardrails/RequirementsFiles.cs`).

All commands run from the repository root.

```sh
python3 .claude/skills/requirement-hash/scripts/hash.py REQ-008            # the hash for one or more ids
python3 .claude/skills/requirement-hash/scripts/hash.py --all              # every requirement with its hash
python3 .claude/skills/requirement-hash/scripts/hash.py --check            # markers in tests vs requirements; exit 1 on drift
python3 .claude/skills/requirement-hash/scripts/hash.py --fix              # rewrite stale markers in the spec tests
```

## When rewording a requirement

1. Change the text in `docs/specs/requirements/`; keep the id if the meaning is the same, retire it and take a new
   id if the meaning changed.
2. Read every test that carries the id (`--check` lists them) and confirm it still proves the reworded text. If it
   does not, mark it `@wip` or `[Trait("Category", "wip")]` instead of updating the hash.
3. `--fix` rewrites the hashes of the markers that still prove the requirement; commit the requirement and the tests
   together in the spec PR.

## When adding a requirement

Write the requirement, then `hash.py REQ-nnn` gives the value for the `@REQ-nnn:hash` tag or the
`[Requirement("TECH-nnn", "hash")]` attribute of its new, `@wip` test.

## Rules

- Never update a hash without re-reading the test: the hash records which text the test was approved against.
- `--fix` touches only the marker; it never changes a scenario, a step, or a fact.
- Do not run `--fix` in an implementation session; the spec tests are a protected path there.
