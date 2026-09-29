# Spec-First .NET Template

A `dotnet new` template for spec-first, agent-driven development of a lightweight ASP.NET Core service. Company
agnostic, no DDD ceremony: one API project, two contract projects (HTTP models and messages), two test projects, and
the machinery that keeps code and specification in agreement.

## What you get

- **Requirements as the source of truth.** EARS sentences with stable ids (`REQ-nnn`, `TECH-nnn`) in
  `docs/specs/requirements/`. Every test that proves a requirement carries its id and a hash of its text.
- **Traceability enforced by tests.** A requirement without a test, a test pointing at an unknown id, a stale hash, a
  requirement with zero or two `shall` or outside the EARS patterns, an open `[NEEDS CLARIFICATION]` marker: each
  fails `dotnet test`. A read-only `spec-reviewer` agent judges what a regex cannot before a spec PR opens.
- **Contracts checked one way.** `docs/specs/api/openapi.yaml`, `docs/specs/db/schema.sql` and
  `docs/specs/messages/asyncapi.yaml` are protected. CI regenerates the OpenAPI document, the database schema and the
  message schemas from code and fails when code exposes anything a contract does not declare. Contract ahead of code
  is pending work and passes, so PR order never matters.
- **Two pull requests.** A spec PR (label `spec`) carries requirements, contracts and failing `@wip` tests; the
  implementation PRs may only delete `@wip` lines (or wip trait lines) in the protected paths. CI checks the paths.
- **Guardrails at commit zero.** Warnings as errors, Sonar and BannedApi analyzers, NsDepCop namespace rules, CSharpier
  and gitleaks through Husky.Net hooks, Conventional Commits with a `spec` type, CODEOWNERS, gitleaks and a coverage
  gate in CI, Dependabot, OpenTelemetry, a Docker image smoke-tested in CI, Stryker weekly over both test projects, a
  nightly report of stale `@wip` markers.
- **Agent commands and rules.** `/specify`, `/clarify`, `/plan`, `/tasks`, `/implement` under `.claude/commands/`, a
  `CLAUDE.md` that states the rules the CI enforces, and path-scoped coding rules under `.claude/rules/` (code
  quality, concurrency, database, Dockerfile, git, and the C# conventions), company-agnostic and editable. Two
  session hooks block an implementation session that edits the spec, and `--no-verify` or commits on the default
  branch; Husky owns the commit, CI owns the truth. A `spec-reviewer` agent judges a change before its spec PR, and
  an `implement-change` workflow runs a change's tasks with a fresh implementer and an independent verifier each.
- **A worked example.** An Orders feature with eleven functional and six technical requirements (one of them
  `@no-test`), Reqnroll scenarios,
  a CsCheck property test, an `OrderConfirmed` message through CAP's transactional outbox to RabbitMQ, and cancellation
  (`REQ-008` to `REQ-011`) deliberately left specified but unimplemented.

Stack: .NET 10 minimal API, PostgreSQL with EF Core, CAP with RabbitMQ, xUnit, Reqnroll, CsCheck, Shouldly,
Testcontainers.

## Install and use

From this repository:

```sh
dotnet new install ./templates/SpecFirstApi
dotnet new spec-first-api -n Acme.Orders
```

Or pack once and install the package:

```sh
dotnet pack -c Release -o artifacts
dotnet new install ./artifacts/SpecFirst.Templates.DotNet.0.1.0.nupkg
```

Then, in the new repository:

```sh
cd Acme.Orders
git init && git add -A && git commit -m "chore: scaffold from spec-first-api"
docker compose up -d
dotnet test --filter "Category!=wip"
```

`dotnet new spec-first-api -h` lists the options. The name you pass replaces `SpecFirst.Service` everywhere
(namespaces, projects, solution) and its lower-case dashed form replaces `specfirst-service` (database, container
and image names).

Uninstall with `dotnet new uninstall ./templates/SpecFirstApi` (or the package id `SpecFirst.Templates.DotNet`).

## Layout of a generated repository

```
CLAUDE.md                      the operating manual for agent sessions
docs/workflow.md               the workflow: EARS, ids, hashes, two PRs, contracts, checks
docs/adr/                      decisions without a test
docs/specs/                    protected: requirements, api, db and messages contracts, changes/
src/<Name>.Api/                the service
src/<Name>.Models/             the HTTP contract: requests, responses, wire enums; packable
src/<Name>.Messages/           the message contract: events and commands with their topics; packable
tests/<Name>.Tests.Spec/       protected: acceptance, integration, invariant and guardrail tests
tests/<Name>.Tests.Unit/       free zone
.claude/commands/              specify, clarify, plan, tasks, implement
.claude/rules/                 coding rules loaded by path: common/ and csharp/
.claude/hooks/                 session hooks: protected paths by branch, no --no-verify, no commits on main
.claude/agents/                spec-reviewer: read-only review of a change before the spec PR
.claude/workflows/             implement-change: fresh implementer per task, independent verifier
.claude/skills/                requirement-hash: compute, check and fix the requirement hashes in test markers
.github/workflows/ci.yml       format, protected paths, contracts, tests, coverage, secrets, container
.github/workflows/stale-wip.yml nightly: wip markers older than the limit
.github/workflows/mutation.yml  weekly: Stryker over both test projects
```

## Working on the template

The template content under `templates/SpecFirstApi/` is a normal solution: build and test it in place.

```sh
cd templates/SpecFirstApi
dotnet tool restore
dotnet build -c Release                     # analyzers; writes spec.json for the contract check
dotnet test --filter "Category!=wip"        # Docker required
npm install && npm run lint:contracts       # Spectral over both contracts, pinned in package.json
docker run --rm -v "$PWD:/w:ro" tufin/oasdiff:v1.32.1 breaking /w/spec.json /w/docs/specs/api/openapi.yaml \
  --fail-on WARN --severity-levels /w/.oasdiff-severity.txt
```

The CI of this repository packs the template, instantiates it under a different name and runs the generated
repository's checks against it.
