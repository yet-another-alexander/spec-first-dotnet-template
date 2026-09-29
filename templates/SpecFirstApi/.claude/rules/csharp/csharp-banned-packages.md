---
paths:
  - "src/**/*.{cs,csx,csproj}"
---

# C# Banned and Restricted Packages

## Banned — Do Not Use in New Code

* **AutoMapper** — Do not use. Replace with manual mapping using the factory pattern. Use `required` and `init` keywords (C# 11) to guarantee no fields are missed. Latest free version: 14.0.0.
* **MediatR** — Do not use. Replace with custom request handlers for command/query separation. Rewrite pipeline behaviours as custom middleware. Latest free version: 12.5.0.
* **MassTransit** — Do not adopt. Latest open-source version: v8.
* **FluentAssertions** — Do not use. Replace with `Shouldly`. For simple cases, use built-in assertions. Latest free version: 7.2.0.
* **Moq** — Do not use. Replace with `NSubstitute` (MIT license).

## Replaced Libraries

* **Newtonsoft.Json** — Use `System.Text.Json` instead. Existing services that already depend on Newtonsoft.Json may continue using it until migrated.
* **RestEase** — Use `Refit` instead.
* **Autofac** — Use the built-in .NET DI container instead.
* **ServiceStack.Redis** — Use `StackExchange.Redis` instead.
