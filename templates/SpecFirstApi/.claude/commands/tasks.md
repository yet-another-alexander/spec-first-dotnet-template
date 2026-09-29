---
description: Split a change's plan into tasks, one task per implementation PR
argument-hint: <change folder name>
---

Change: $ARGUMENTS

Read `docs/specs/changes/$ARGUMENTS/plan.md` and the `@wip` tests of the change. Write
`docs/specs/changes/$ARGUMENTS/tasks.md` as a numbered list. Each task:

- is one pull request an agent can finish in one session;
- names the requirement ids it takes from work in progress to covered, and the exact `@wip` lines it will delete;
- names the files it adds or changes, all outside the protected paths;
- states when it is done: which tests pass, which checks are green;
- leaves CI green on its own, so tasks can be reviewed and merged one at a time.

Order the tasks so each one builds on merged work. Prefer three small tasks over one large one. Do not implement.
