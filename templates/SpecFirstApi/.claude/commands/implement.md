---
description: Implement one task of a change in a fresh session, respecting the protected paths
argument-hint: <change folder name> <task number>
---

Input: $ARGUMENTS (change folder, then task number).

This session implements. It does not specify. You may not have taken part in writing the spec or the tests of this
change; if you did, stop and hand the task to a fresh session.

1. Read `CLAUDE.md`, then `docs/specs/changes/<change>/tasks.md` and pick the task. Read the requirements it names,
   the contracts, and the `@wip` tests it must turn green. The tests define done; read them before writing code.
2. Implement in `src/SpecFirst.Service.Api/`, following the layout and conventions in `CLAUDE.md`. Add unit tests in
   `tests/SpecFirst.Service.Tests.Unit/` as you see fit; they are yours.
3. Do not change anything under `docs/specs/` or `tests/SpecFirst.Service.Tests.Spec/` except deleting the `@wip`
   lines the task names, once those tests pass. If a test looks wrong, stop and report it in the PR; do not adapt the
   test to the code.
4. Expose nothing the contracts do not declare. Before opening the PR run:
   `dotnet csharpier format .`, `dotnet build -c Release`, `dotnet test --filter "Category!=wip"`, and for contract
   changes Spectral and oasdiff as in `docs/workflow.md` section 8.
5. Open a draft PR without the `spec` label. Title `feat(<area>): <ticket> <task summary>` (or `fix`, `refactor`).
   Body: the task, the ids now covered, anything you had to leave out and why.
