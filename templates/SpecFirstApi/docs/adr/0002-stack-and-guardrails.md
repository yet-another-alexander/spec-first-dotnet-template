# ADR-0002: Stack and guardrail tooling

Status: accepted. Date: 2026-09-21.

## Context

A lightweight service needs the guardrails of ADR-0001 without a heavy architecture. Every tool below had to be
verifiable in CI and boring to operate.

## Decision

- **Runtime**: ASP.NET Core minimal API in one project, feature folders, `Program.cs` as a table of contents.
  PostgreSQL through EF Core with delta migrations applied at startup. `TimeProvider` for time. Problem details for
  every error. OpenTelemetry wired from the first commit, exported only when an OTLP endpoint is configured. Messages
  through CAP (DotNetCore.CAP) with the transactional outbox in the same Postgres and RabbitMQ as transport; message
  types are plain records in the `Messages` project, the contract is AsyncAPI. The HTTP request and response records
  live in the `Models` project; both contract projects are BCL-only and packable, so a consumer takes a contract as a
  package instead of copying it, and NsDepCop keeps the service from leaking into either.
- **Tests**: xUnit with Shouldly. Reqnroll for acceptance scenarios over HTTP; CsCheck for invariants; Testcontainers
  for a real Postgres and RabbitMQ, with a test-side CAP subscriber seeing what the service publishes. Two test projects
  only: `Tests.Spec` (protected, carries requirement ids) and `Tests.Unit` (free zone).
- **Traceability**: hashes live in the test markers (`@REQ-nnn:hash`, `[Requirement(id, hash)]`), not in a lock file,
  so each test states which text it was approved against. Work in progress is a `Category=wip` trait excluded by
  filter, so wip tests compile and run on demand but do not fail the build. The traceability check is a test in
  `Tests.Spec/Guardrails`, so it runs locally with everything else.
- **API drift**: `oasdiff breaking <generated> <contract>` with the generated document as the base. Elements in code but
  not in the contract appear as removals and fail; contract ahead of code appears as additions and passes.
  `.oasdiff-severity.txt` corrects the places where "breaking for a client" and "present in code but not in the
  contract" disagree: tightened request constraints are demoted, informational removals are promoted. The image is
  pinned to keep the check ids valid.
- **Database drift**: `pg_dump --schema-only` of the migrated test database, normalised, compared as a set of
  statements with `docs/specs/db/schema.sql`. Regenerated with `UPDATE_SCHEMA=1`.
- **Message drift**: a JSON Schema generated from each message type with NJsonSchema, compared one way with the payload
  schemas in `docs/specs/messages/asyncapi.yaml`; no external diff tool, the check is a guardrail test.
- **Compile-time rules**: `TreatWarningsAsErrors`, Sonar analyzers, BannedApiAnalyzers (`BannedSymbols.txt`), NsDepCop
  for namespace and assembly dependencies (a root `config.nsdepcop` inherited by one per project). Rules the analyzers
  cannot express are reflection tests in `Guardrails/`.
- **Repository**: CSharpier and gitleaks through Husky.Net hooks, Conventional Commits with a `spec` type, CODEOWNERS
  on the protected paths, gitleaks and Dependabot in CI, a line-coverage gate over the merged report, a nightly
  stale-wip report, one container image as the deployable.

## Consequences

Five projects (the service, two contract packages, two test projects), no repositories or mediators, no DDD ceremony.
The template stays small enough to read in one sitting.
Stryker runs weekly over both test projects and fails below its break threshold. Status transitions are a Stateless
state machine owned by the entity (`Order.TryFire`), so a `While` requirement is one `Permit` or `Ignore` line and a
rejected transition is one 409. Not chosen, available when needed: Verify for snapshot tests.
