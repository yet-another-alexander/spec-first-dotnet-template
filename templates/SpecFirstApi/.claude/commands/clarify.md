---
description: Collect the open questions of a change, ask them where the product owner is, and fold the answers back into the requirements
argument-hint: <change folder name, e.g. ABC-123-order-cancellation>
---

Change: $ARGUMENTS

1. Read `docs/specs/changes/$ARGUMENTS/spec.md` and the requirements it lists. Collect every
   `[NEEDS CLARIFICATION: ...]` marker, and add any requirement that fails the completeness check: a `When` without an
   `If` for its failure, a status without a `While`, a flag without a `Where`, an adjective where a number belongs, a
   response that is not observable.
2. Turn each into one question a product owner can answer with a sentence. Offer the options you see and the default
   you would pick, so an answer can be "the default".
3. If the ticket lives in a tracker you can reach, post the questions as one comment on the ticket, addressed to the
   product owner. Otherwise print them for the human running this session to forward.
4. When the answers arrive: rewrite the affected requirements in EARS, keeping ids whose meaning did not change and
   retiring ids whose meaning did; re-read each test of a reworded requirement and, if it still proves it, update
   its hash with the `requirement-hash` skill (`hash.py --fix`), otherwise mark it wip; remove the markers; record the
   decision in one line of commentary under the requirement. Update `spec.md`.
5. Run `dotnet test --filter "FullyQualifiedName~Guardrails"` until it is green. Do not implement anything.
