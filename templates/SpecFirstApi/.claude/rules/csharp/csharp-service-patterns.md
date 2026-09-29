---
paths:
  - "src/**/*.{cs,csx,csproj}"
---

# C# Service Patterns

What the service is built from, and what to reach for when a requirement needs more. Every pattern here is either already wired in the template or a one-package addition; do not introduce a parallel mechanism for something listed.

## Already wired

* **Composition**: `Program.cs` is a table of contents of `Add*`/`Map*` calls; each concern has one `*Setup.cs`. New infrastructure gets a new setup file, not lines in `Program.cs`.
* **Validation**: DataAnnotations on request contracts plus the framework's built-in validation (`AddValidation`), which yields RFC 9457 validation problems. Reach for a validator library only when a rule cannot be expressed as an attribute.
* **Time**: `TimeProvider`, injected; the banned-API analyzer rejects `DateTime.Now`, `UtcNow`, `Today`.
* **Errors**: Problem Details everywhere; expected failures are return values, unexpected ones reach the global `IExceptionHandler`.
* **Persistence**: EF Core with delta migrations applied at startup, snake_case naming, `AppDbContext` as the only seam. No repository layer.
* **Messaging**: CAP with the transactional outbox in the service's own PostgreSQL and RabbitMQ as transport. Publish with `ICapPublisher` inside a CAP transaction on the `DbContext` so the outbox row commits with the business change; never publish outside one. Consumers are `ICapSubscribe` classes with `[CapSubscribe(topic)]`; CAP's inbox makes redelivery safe when a handler is idempotent.
* **State machines**: `Stateless`, owned by the entity (`Order.TryFire`); endpoints fire triggers and never assign a status. A rejected transition is a 409.
* **Optimistic concurrency**: entities carry PostgreSQL's `xmin` as a row version (`Property<uint>("xmin").IsRowVersion()` in the EF configuration; no column, no schema change). A `DbUpdateConcurrencyException` means another request wrote first: reload the entity and answer from its current state, as `ConfirmOrder` does; never retry the write blindly.
* **Health**: `AddHealthChecks` with a check per dependency, mapped at `/health`.
* **Telemetry**: OpenTelemetry traces and metrics, exported over OTLP when `OTEL_EXPORTER_OTLP_ENDPOINT` is set.
* **DI**: the built-in container. **Packages**: central package management in `Directory.Packages.props`.

## Contract separation

* HTTP contracts live in the `Models` project (`Requests/`, `Responses/`, `Enums/`), message contracts in the `Messages` project (`Events/`, `Commands/`). Both are BCL-only and packable, so a consumer takes the contract as a package instead of copying it. A message may carry a wire enum from `Models`, never a request or response record: the two evolve on different schedules and different consumers depend on each.

## Idempotency

* Handlers of messages and of state-changing requests are idempotent: processing the same input twice yields the same state and no second side effect (REQ-007 and TECH-006 are the shipped examples). Key on a stable identifier: the entity's status for transitions, an idempotency key for creates, CAP's message id for consumers. See [`../common/concurrency-conventions.md`](../common/concurrency-conventions.md).

## Run-once and multi-instance

* Anything that must happen exactly once across instances (scheduled jobs, one-off migrations) takes a distributed lock or leader election, never "instance 0 does it". EF Core's migration lock covers migrations at startup.

## Money

* An amount is never without its currency. Use a small `Money` value type (`decimal Amount`, `string Currency`) in the domain and a `MoneyDto` record on the wire; arithmetic and comparison across currencies go through a conversion service, equality is always safe. Defer rounding as late as possible.

## Feature flags

* If flags are needed, use the OpenFeature API (`IFeatureClient`) with a provider of your choice, so the code does not depend on a vendor.

## HTTP clients

* Typed clients through `IHttpClientFactory`; `Refit` when an interface-first client is worth it. Every outgoing call carries the cancellation token and is traced by the HTTP instrumentation already registered.
