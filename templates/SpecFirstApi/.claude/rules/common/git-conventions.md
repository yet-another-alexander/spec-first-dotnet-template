# Branch & Commit Conventions

> **How to use this rule:** Applies to every branch, commit and pull request in this repository. The spec-first workflow in `docs/workflow.md` decides *what* a PR may contain; this file decides how it is named and described. The commit-msg hook (`.husky/csx/commit-lint.csx`) enforces the commit format; read it before writing a commit message. This file takes priority — if the hook disagrees, say so and propose updating the hook.

---

## Branches

`{type}/{TICKET-ID}-{short-desc}` where type is `feature|bugfix|hotfix|release|spec`.

- `spec/` for a specification PR (requirements, contracts, `@wip` tests, change folder).
- `feature/` and `bugfix/` for implementation PRs; one task from `docs/specs/changes/<change>/tasks.md` per branch.
- `hotfix/` and `release/` may omit the ticket: `hotfix/{short-desc}`.

**If no ticket is known in the session, ask for the branch name. Never invent one.**

## Commits (Conventional Commits)

```
{type}({optional scope}): {imperative description}
```

Types: `build|chore|ci|docs|feat|fix|ops|perf|refactor|revert|spec|style|test`. Subject line at most 90 characters, no trailing period. Add the ticket id to the subject if your tracker wants one (`feat(orders): ABC-123 add order cancellation`); the hook does not require it.

- `spec(...)` is the type for specification changes: requirements, contracts, `@wip` tests, change folders.
- Breaking changes: `!` after the scope — `feat(api)!: change the order representation` — and an optional `BREAKING CHANGE:` footer.
- Every commit made by an AI tool includes a `Co-Authored-By:` trailer naming the tool.

## Pull requests

**Two kinds, never mixed** (see `docs/workflow.md` section 3):

- **Specification PR**, label `spec`: what is correct. Reviewed by a spec owner other than its author.
- **Implementation PR**, no label: what conforms. May not touch `docs/specs/` or the spec test project except to delete wip markers. Reviewed mostly by the guardrails.

**Title**: the commit format (`type(scope?): summary`), at most 90 characters, summarising the content. Not a bare ticket key.

**Body**: lead with *why*, then *what changed*, at most 200 words, bullets for multi-change PRs, never a raw file list. The PR template in `.github/` asks for the change folder, the task, and the requirement ids added, changed, or taken from work in progress to covered.

**Ticket link**: if the branch or commits carry a ticket id, end the body with a blank line and the ticket URL, one per line, plain, not a markdown link.

**Draft by default**: create PRs as drafts (`gh pr create --draft --assignee @me`). Mark a PR ready (`gh pr ready`) only when the human confirms the work is complete.

**Hotfix PRs** go straight to the default branch; say in the PR that they need a maintainer's approval.

## Rules

- Imperative mood in descriptions.
- No direct commits to the default branch.
- A ticket id, when present, is upper-case and refers to a real ticket.
