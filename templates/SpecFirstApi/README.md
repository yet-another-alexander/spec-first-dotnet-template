# SpecFirst.Service

A spec-first ASP.NET Core service. The requirements in `docs/specs/requirements/` are the source of truth; the tests in
`tests/SpecFirst.Service.Tests.Spec/` prove them; CI checks that the code exposes nothing the contracts in `docs/specs/`
do not declare. How this works, and why: `docs/workflow.md`. How to work here as an agent: `CLAUDE.md`; the coding rules
are under `.claude/rules/`.

## Run it

You need the .NET 10 SDK (`global.json` pins the feature band) and a Docker daemon for Postgres, RabbitMQ and the test
containers.

```sh
docker compose up -d
dotnet run --project src/SpecFirst.Service.Api
```

The API migrates the database at startup. In Development it serves the API reference at
<http://localhost:5000/scalar/v1> and the generated OpenAPI document at <http://localhost:5000/openapi/v1.json>.

```sh
curl -s -X POST localhost:5000/v1/orders -H 'content-type: application/json' \
  -d '{"lines":[{"sku":"APPLE","quantity":3,"unitPrice":1.5}]}'
```

## Test it

```sh
dotnet test --filter "Category!=wip"     # what CI runs; Docker required
dotnet test --filter "Category=wip"      # the work in progress, expected to fail until implemented
```

`Tests.Spec` starts a real `postgres:18-alpine` and `rabbitmq:4-alpine` through Testcontainers. `Tests.Unit` needs
nothing.

## The sample

The template ships with an Orders feature as a worked example of the workflow:

- `docs/specs/requirements/functional.md`: REQ-001 to REQ-011 in EARS; `technical.md`: TECH-001 to TECH-006.
- `docs/specs/messages/asyncapi.yaml`: the `OrderConfirmed` message the service publishes through CAP's outbox when an
  order is confirmed (TECH-005, TECH-006).
- `tests/SpecFirst.Service.Tests.Spec/Acceptance/Orders.feature`: one scenario per requirement, tagged with id and
  hash. REQ-008 to REQ-011 (cancellation) are specified, contracted in `openapi.yaml`, and tagged `@wip`: the spec is
  ahead of the code, exactly the state a merged spec PR leaves behind. Implement it as your first exercise
  (`/implement EXAMPLE-1-orders 4`), or delete the feature and start your own change with `/specify`.
- `docs/specs/changes/EXAMPLE-1-orders/`: the change folder with `spec.md`, `plan.md`, `tasks.md`.

## Before you commit

The pre-commit hook scans the staged changes for secrets with gitleaks and formats them with CSharpier; the commit-msg
hook checks Conventional Commits. gitleaks is not a dotnet tool: install it once (`brew install gitleaks`,
`winget install gitleaks`, or a release from <https://github.com/gitleaks/gitleaks/releases>) or the hook fails. What
CI checks, and how to run each check locally, is in `docs/workflow.md` section 8.

## Repository settings to configure once

These cannot ship in a template:

- Branch protection on `main`: require the CI checks, require review, no self-merge for agents.
- Replace `@your-org/spec-owners` and `@your-org/maintainers` in `.github/CODEOWNERS` and require code owner review.
- Create the `spec` label; PRs carrying it may change `docs/specs/` and the spec tests.
- For organisation repositories, add the `GITLEAKS_LICENSE` secret (free) or remove the `secrets` job.
