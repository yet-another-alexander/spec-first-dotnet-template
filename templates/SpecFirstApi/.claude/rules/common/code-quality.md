# Code Quality Standards

> **How to use this rule:** Applies to every code change, in two moments — when you *write* code, hold yourself to this bar; when you *review* a PR, flag violations and lead with structural problems over cosmetic nits. This file defines what good and bad structure look like, not review severity or approval mechanics.

---

## Prefer deleting complexity over rearranging it

Be ambitious about structural simplification. Don't stop at "this could be a little cleaner" — look for the reframing that makes whole branches, helpers, modes, or layers disappear.

- Prefer a change that removes moving parts over one that spreads the same complexity around.
- Look for the "code-judo" move: a reorganization that uses the existing architecture so the change becomes dramatically simpler and feels inevitable in hindsight.
- Don't rubber-stamp working code that leaves the codebase messier. If behavior can stay identical while the structure gets meaningfully simpler, take that path.

**Why:** rearranged complexity still has to be read, tested, and maintained. Deleted complexity is gone for good.

**In review:** scope suggestions to the changed code — propose the simpler structure for *this* change; don't demand unrelated codebase-wide rewrites.

## Keep files within a healthy size

Don't let a change push a file from under ~1000 lines to over it without a strong, stated reason.

- Treat crossing that line as a smell, not a milestone.
- Prefer extracting helpers, submodules, or local abstractions over letting one file sprawl.
- Waive only when there is a compelling structural reason and the result is still clearly organized.

**Why:** large files are hard to scan, hard to review, and tend to accumulate unrelated responsibilities.

**Remedy:** split into smaller, focused modules along responsibility lines before adding more.

## Don't grow spaghetti in existing code

Be suspicious of new ad-hoc conditionals, scattered special cases, and one-off branches inserted into unrelated flows.

- A change that adds "weird `if` statements in random places" is a design problem, not a style nit.
- Push the logic into a dedicated abstraction, helper, policy object, state machine, or separate module instead of tangling an existing path.
- Call out changes that make surrounding code harder to reason about, even when they technically work.

**Why:** every special case bolted onto a shared path multiplies the states the next reader has to hold in their head.

**Remedy:** turn special-case logic into a simpler default flow with fewer exceptions; collapse duplicate branches into one clear path.

## Prefer direct, boring code over magical code

Brittle, ad-hoc, or "magic" behavior is a code-quality problem, not cleverness.

- Be skeptical of generic mechanisms that hide simple data-shape assumptions.
- Flag thin abstractions, identity wrappers, and pass-through helpers that add indirection without buying clarity.

**Why:** indirection that doesn't earn its keep makes the simple case harder to follow and the failure modes harder to predict.

**Remedy:** delete the wrapper and keep the direct flow. An abstraction should remove concepts, not add a layer.

## Keep type and module boundaries explicit

Question unnecessary optionality, `any`, `unknown`, and cast-heavy code where a clearer type boundary could exist.

- Prefer explicit typed models or shared contracts over loosely shaped ad-hoc objects.
- If a branch relies on a silent fallback to paper over an unclear invariant, make the boundary explicit instead.

**Why:** a fuzzy boundary pushes the cost of understanding the invariant onto every caller and hides bugs behind casts.

**Remedy:** make the type boundary explicit so the downstream control flow gets simpler.

## Keep logic in its canonical layer and reuse what exists

Logic should live in the layer, package, or module that already owns the concept.

- Call out feature logic leaking into shared paths, or implementation details leaking through public APIs.
- Prefer existing canonical utilities and helpers over bespoke near-duplicates.

**Why:** duplicated and misplaced logic drifts out of sync and normalizes architectural erosion.

**Remedy:** move the logic to the module that owns it; reuse the canonical helper instead of introducing a near-copy.

## Prefer atomic, naturally parallel orchestration

Treat needless sequential orchestration and non-atomic updates as design smells when the cleaner structure is obvious.

- If independent work is serialized for no reason, consider running it in parallel where that also simplifies the flow.
- If related updates can leave state half-applied, prefer a more atomic structure.
- Don't over-index on micro-optimization — flag avoidable orchestration complexity that makes the implementation brittle, not raw speed.

**Why:** half-applied state and incidental sequencing are hard to reason about and hard to recover from.

**Remedy:** separate orchestration from business logic; restructure related updates so they apply atomically.

## When reviewing

- Lead with structural problems: structural regressions, missed simplifications, spaghetti growth, and boundary or type erosion come before file-size and naming.
- Prefer a few high-conviction comments over a long list of cosmetic nits.
- Be direct and specific about maintainability problems. Don't soften a structural issue into a vague "maybe rename this."
