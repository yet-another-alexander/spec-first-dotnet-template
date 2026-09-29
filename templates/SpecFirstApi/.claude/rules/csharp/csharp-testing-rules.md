---
paths:
  - "tests/**/*.{cs,csx,csproj}"
---

# C# Testing Rules

> The test layers, what each proves and where it lives are defined in `docs/workflow.md` section 4. This file is about how to write them well.

## Layers and projects

| Layer | Folder | Proves | Needs |
|---|---|---|---|
| Acceptance | `Tests.Spec/Acceptance` (Reqnroll) | REQ, over HTTP | Postgres and RabbitMQ containers |
| Integration | `Tests.Spec/Integration` (xUnit) | TECH, and the schema contract | the same containers |
| Invariants | `Tests.Spec/Invariants` (CsCheck) | ubiquitous REQ | nothing |
| Guardrails | `Tests.Spec/Guardrails` | spec and code agree: traceability, EARS grammar, architecture, the message contract | nothing |
| Unit | `Tests.Unit` | internals | nothing |

* `Tests.Spec` is a protected path: it changes in spec PRs only, except deleting a wip marker. `Tests.Unit` is the implementer's own.
* Each layer is an xUnit collection: sequential inside, parallel between. A new feature file needs the one-line partial class that puts it in the acceptance collection.

## Naming

* Test names are sentences in the language of the requirement, words separated by underscores: `Health_is_200_while_the_database_is_reachable`, `Confirming_a_confirmed_order_publishes_nothing`. No `Test` prefix, no `Should`, no abbreviations; length is not a concern.
* Test classes end in `Tests`; one class per subject.
* Scenario titles read the same way: `Confirming a confirmed order changes nothing`.

## Traceability

* A test that proves a requirement carries its id and hash: `@REQ-nnn:hash` on a scenario, `[Requirement("TECH-nnn", "hash")]` on a fact. The traceability guardrail fails on an unknown id, a stale hash or a requirement without a test.
* Work in progress is `@wip` alone on its line, or `[Trait("Category", "wip")]` on its own line; never `Skip`, which the banned-API analyzer rejects.

## Structure

* Arrange, act, assert separated by blank lines, without the comments.
* One behaviour per test. Assert the observable outcome (status code, body, a row, a message), not the mechanism.
* No shared mutable state between tests; the fixture is shared, so every test creates its own order and filters by its own id.
* Data-driven cases: a `Scenario Outline` with an `Examples` table in a feature file, `[Theory]` with `[InlineData]` or `[MemberData]` in C#. Example data comes from the ticket; agents do not invent it.
* Assertions with Shouldly. Doubles with NSubstitute when a unit test needs one; the spec tests use none.

## Integration and acceptance tests

* Real dependencies through Testcontainers: PostgreSQL and RabbitMQ, the same images as production. Never an in-memory database; it does not reproduce transactions, locking or constraints.
* The application runs as it is, through `WebApplicationFactory<Program>`, with connection strings taken from the containers' mapped ports in `ConfigureWebHost`.
* Services are replaced only through the fixture's named seams, each with a stated purpose: the broken clock to provoke an unhandled exception, the message sink to observe what the service publishes. Do not add a replacement to make a test pass.
* External HTTP dependencies, when the service gains one, are mocked with WireMock.Net, preferably as a container.
* Waiting for asynchronous effects (a message arriving) polls with a bounded timeout; never a fixed sleep as the only synchronisation.

## Unit tests

* For internals and edge cases the spec tests do not name: pure functions, the state machine, mapping. They protect refactoring; they do not prove requirements and carry no ids.
* Mirror the source folders (`Orders/`).

## Invariants

* Ubiquitous requirements are CsCheck properties over generated inputs: additivity, order independence, identities. Generators produce exact values (whole cents, bounded ranges) so failures are reproducible and not filtered away.

## Libraries

* xUnit, Reqnroll, Shouldly, CsCheck, Testcontainers, NSubstitute when needed, WireMock.Net when needed. Not FluentAssertions, not Moq, not AutoFixture (see [`csharp-banned-packages.md`](csharp-banned-packages.md)).
