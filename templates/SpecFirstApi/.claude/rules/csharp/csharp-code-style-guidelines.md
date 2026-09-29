---
paths:
  - "src/**/*.{cs,csx,csproj}"
  - "tests/**/*.{cs,csx,csproj}"
---

# C# Code Style Guidelines

> Layout is decided by CSharpier and `.editorconfig`, and enforced by the pre-commit hook and the `format` CI job. Never hand-format against the formatter; run `dotnet csharpier format .`. This file covers what a formatter cannot decide.

## Fields and Properties

* Private and internal fields are `_camelCase`; constants and static fields are `PascalCase` (the naming rules in `.editorconfig`).
* Fields are `readonly` unless mutation is required; `static readonly`, not `readonly static`.
* No public fields; use properties. Properties are `PascalCase`.
* No `this.` unless needed to resolve ambiguity.
* Visibility is always explicit and comes first (`public abstract`, not `abstract public`).
* Declare fields at the top of the type.

## Types

* Private, internal and nested types are `static` or `sealed` unless inheritance is required. Contract types are `sealed` (the architecture guardrail checks it).
* Records for data that is compared by value: contracts, messages, results. Classes for entities with identity.
* Namespaces are file-scoped and match the folder.

## Naming

* Methods and local functions are `PascalCase`; local constants `camelCase`.
* `nameof(...)` instead of string literals for member names.
* Names say what a thing is, not its type: `order`, not `orderEntity`; `clock`, not `timeProvider`.

## Declarations

* `var` is fine when the type is obvious from the right-hand side or does not matter to the reader; write the type when it carries meaning (`decimal total = ...`).
* Target-typed `new()` only when the type is named on the left.
* Language keywords (`int`, `string`) over BCL names (`Int32`, `String`).

## Imports

* Usings at the top of the file, outside the namespace, `System.*` first, then alphabetical (`.editorconfig` orders them; CSharpier keeps them).

## Conditionals

* A single-statement branch goes on its own line, with or without braces; never on the same line as the `if`. The formatter enforces this.
* If any branch of an `if`/`else` chain has braces or spans several lines, every branch has braces.

## Miscellaneous

* Non-ASCII characters in code use `\uXXXX` escapes.
* No more than one consecutive blank line.
