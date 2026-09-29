---
paths:
  - "src/**/*.{cs,csx,csproj}"
---

# C# Coding Rules

## General

* Follow SOLID, DRY and KISS; prioritise clarity over cleverness (see [`../common/code-quality.md`](../common/code-quality.md)).
* Nullable reference types are on and warnings are errors; never suppress a nullability warning to make a build pass.
* Throw, catch and word exceptions per [`csharp-exception-handling.md`](csharp-exception-handling.md).
* `ConfigureAwait(false)` in library code; not needed in ASP.NET Core application code.
* Named constants instead of magic numbers; `ApiConstants` holds the cross-cutting ones.

## Documentation

* Contract properties carry `[Description]`: it flows into the generated OpenAPI document and the drift check. XML `<summary>` on public types and members whose purpose is not obvious from the name; do not restate the name.

## Architecture and Design

* Endpoint handlers orchestrate: parse, load, call the feature's logic, persist, map. Domain logic lives in the feature folder as pure functions (`OrderTotal`) or entity methods (`Order.TryFire`); never in the handler body.
* Status transitions go through the entity's Stateless state machine; nothing else assigns a status.
* Prefer composition over inheritance. Keep classes and methods single-responsibility.
* Object mapping is explicit code with `required` and `init` so nothing is missed; no mapping library (see [`csharp-banned-packages.md`](csharp-banned-packages.md)).
* Cross-cutting concerns (logging, validation, error shaping) live in middleware, handlers or setup files, never inside business logic.
* Dependencies flow inward: the contract projects (`Models`, `Messages`) depend on nothing in the solution, features depend on the contracts and persistence, only the composition root (`Program.cs`, `*Setup.cs`) depends on everything. NsDepCop enforces it at compile time: the root `config.nsdepcop` holds the solution rules, each project's `config.nsdepcop` declares the assemblies and namespaces it may take.

## Collections and Method Design

* Inputs: the most restrictive type that works, `IReadOnlyList<T>`, `IReadOnlyCollection<T>` or `IEnumerable<T>`; iterate an `IEnumerable<T>` once.
* Outputs: `IReadOnlyList<T>` unless the caller must mutate.
* Every async method takes a `CancellationToken` and passes it on until cancellation stops being possible.
* Replace `bool` parameters with an enum when the call site would otherwise read `Foo(true)`.
* Name intermediate values instead of nesting expressions; use named arguments where a literal would be unclear.
* Many options become an options record, not a long parameter list.

## Extension Methods

* No business logic in extension methods; use them for framework registration (`Add*`/`Map*`) and for types you do not own.

## Time and Money

* Time comes from `TimeProvider` in UTC as `DateTimeOffset`; `DateOnly`/`TimeOnly` when there is no time or date component.
* Money is an amount with its currency, never an amount alone. See [`csharp-service-patterns.md`](csharp-service-patterns.md) and [`csharp-api-conventions.md`](csharp-api-conventions.md).
