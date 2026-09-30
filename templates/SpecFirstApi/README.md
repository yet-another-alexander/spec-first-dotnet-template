# SpecFirst.Service

A spec-first ASP.NET Core service. The requirements in `docs/specs/` are the source of truth, the tests in
`tests/SpecFirst.Service.Tests.Spec/` prove them, and CI rejects code that exposes anything the contracts do not
declare. `docs/workflow.md` explains the workflow; `CLAUDE.md` is the manual for agent sessions.

## Run

Needs the .NET 10 SDK and Docker.

```sh
docker compose up -d
dotnet run --project src/SpecFirst.Service.Api
```

The database migrates at startup. The API reference is at <http://localhost:5000/scalar/v1>.

## Test

```sh
dotnet test --filter "Category!=wip"     # what CI runs; Docker required
dotnet test --filter "Category=wip"      # specified but not yet implemented; expected to fail
```

## The sample

An Orders feature shows the workflow end to end: requirements in `docs/specs/requirements/`, contracts in
`docs/specs/api/`, `db/` and `messages/`, one acceptance scenario per requirement in
`tests/SpecFirst.Service.Tests.Spec/Acceptance/Orders.feature`, and the change folder in
`docs/specs/changes/EXAMPLE-1-orders/`.

Order cancellation (REQ-008 to REQ-011) is specified and tagged `@wip` but not implemented, which is the state a merged
spec PR leaves behind. Implement it as a first exercise with `/implement EXAMPLE-1-orders 4`, or delete the feature and
start your own change with `/specify`.

## Before you commit

Git hooks format staged files, scan them for secrets and check the commit message. They need
[gitleaks](https://github.com/gitleaks/gitleaks#installing) on your PATH.

## Repository settings to configure once

- Protect `main`: require the CI checks and a review.
- Replace the `@your-org/*` teams in `.github/CODEOWNERS`.
- Create the `spec` label; only PRs carrying it may change `docs/specs/` and the spec tests.
- Add the `GITLEAKS_LICENSE` secret for organisation repositories, or remove the `secrets` job from CI.
