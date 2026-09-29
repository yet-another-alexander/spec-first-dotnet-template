# Spec-First .NET Template

A template for an ASP.NET Core service developed spec-first: requirements and contracts are written and tested before
the code, and CI keeps code and specification in agreement.

Stack: .NET 10 minimal API, PostgreSQL with EF Core, CAP with RabbitMQ, xUnit, Reqnroll, CsCheck, Shouldly, Testcontainers.

## Quick start

```sh
dotnet new install ./templates/SpecFirstApi
dotnet new spec-first-api -n Acme.Orders
cd Acme.Orders
dotnet test --filter "Category!=wip" # needs Docker
```

The name you pass replaces `SpecFirst.Service` in projects, namespaces and the solution. Its lower-case dashed form
(`acme-orders`) names the database, containers and image. `dotnet new spec-first-api -h` lists the options.

Uninstall with `dotnet new uninstall ./templates/SpecFirstApi`.

## How it works

- **Requirements are the source of truth.** EARS sentences with stable ids (`REQ-nnn`, `TECH-nnn`) live in
  `docs/specs/requirements/`. Every test that proves one carries its id and a hash of its text.
- **Tests enforce traceability.** An untested requirement, an unknown id, a stale hash or an open
  `[NEEDS CLARIFICATION]` marker fails `dotnet test`.
- **Contracts are checked against code.** CI regenerates the OpenAPI document, database schema and message schemas
  from the code and fails when the code exposes anything `docs/specs/` does not declare.
- **Two kinds of pull request.** A spec PR (label `spec`) adds requirements, contracts and failing `@wip` tests. An
  implementation PR makes them pass and may only delete `@wip` markers in the protected paths.
- **Guardrails from commit zero.** Warnings as errors, analyzers, NsDepCop, CSharpier and gitleaks hooks, Conventional
  Commits, coverage gate, Dependabot, OpenTelemetry, a smoke-tested Docker image, weekly Stryker, nightly stale-wip
  report.
- **Agent support.** `/specify`, `/clarify`, `/plan`, `/tasks` and `/implement` commands, a `CLAUDE.md` operating
  manual, path-scoped coding rules, session hooks that block spec edits from implementation sessions, a
  `spec-reviewer` agent and an `implement-change` workflow.
- **A worked example.** An Orders feature with requirements, Reqnroll scenarios, a property test, an `OrderConfirmed`
  message through CAP's outbox, and cancellation left specified but unimplemented.

## Generated layout

```
CLAUDE.md                  operating manual for agent sessions
docs/workflow.md           the full workflow
docs/specs/                protected: requirements, api, db and message contracts, changes/
src/<Name>.Api/            the service
src/<Name>.Models/         HTTP contract: requests, responses, enums (packable)
src/<Name>.Messages/       message contract: events and commands (packable)
tests/<Name>.Tests.Spec/   protected: acceptance, integration, invariant and guardrail tests
tests/<Name>.Tests.Unit/   free zone
.claude/                   commands, rules, hooks, agents, workflows, skills
.github/workflows/         ci, stale-wip (nightly), mutation (weekly)
```

## Working on the template

`templates/SpecFirstApi/` is a normal solution. Build and test it in place:

```sh
cd templates/SpecFirstApi
dotnet tool restore
dotnet build -c Release
dotnet test --filter "Category!=wip" # needs Docker
npm install && npm run lint:contracts
```

CI packs the template, instantiates it as `Acme.Orders` and runs the generated repository's checks against it.
