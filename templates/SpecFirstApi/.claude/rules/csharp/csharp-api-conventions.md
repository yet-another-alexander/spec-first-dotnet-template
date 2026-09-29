---
paths:
  - "src/**/*.{cs,csx,csproj}"
---

# C# API Conventions

> The wire shape is a contract: `docs/specs/api/openapi.yaml` is the source of truth and CI fails when code exposes anything it does not declare. Most conventions below are enforced by the Spectral ruleset in `.spectral.yaml`; this file explains the C# side of each.

## Enums in APIs

* Enums are strings in API payloads, never integers. The wire values are lower-case `snake_case` (`submitted`, `confirmed`), set with `[JsonStringEnumMemberName]` and `JsonStringEnumConverter<T>` on the enum; the architecture guardrail rejects a contract enum without the converter.
* Document every value in the OpenAPI contract; a value in code that the contract lacks is drift and fails CI.
* Reject unknown enum values in requests with 400 (the framework does this); never map them to a silent default.
* For enum design see [`csharp-enum-usage.md`](csharp-enum-usage.md).

## Dates and Times in APIs

* All date/time values are UTC in ISO 8601 (`YYYY-MM-DDTHH:mm:ssZ`); `format: date-time` in the contract.
* Use `DateTimeOffset` in C#; `timestamp with time zone` in PostgreSQL.
* Take the current time from `TimeProvider` (`DateTime.Now` and friends do not compile, see `BannedSymbols.txt`).
* Compute in UTC; convert to local time only for display.
* Malformed or ambiguous values are 400. Document the precision (seconds vs milliseconds) in the contract.
* Durations are ISO 8601 (`P1Y2M10D`) or a numeric field with the unit in its name.

## Decimal Numbers in APIs

* Use `decimal` in C#. Never `float` or `double` for money or precision-critical values. The OpenAPI document renders `decimal` as `number`/`double`; that is the wire format, not the C# type.
* Store as `numeric(P,S)` (`HasPrecision`).
* Round explicitly with `decimal.Round()`; defer rounding as late as possible.
* Money carries a currency code (ISO 4217) next to the amount; see [`csharp-service-patterns.md`](csharp-service-patterns.md).
* Return pre-calculated totals; clients do not do arithmetic.

## Binary Data and File Uploads

* `multipart/form-data` and `IFormFile` for uploads; Base64 only for small binary data inside JSON.
* Set `Content-Type` and `Content-Disposition` for downloads, `Content-Length` for progress.
* Validate type, size and content on the server.

## Error Responses

* Every error is RFC 9457 Problem Details (`application/problem+json`): validation failures through the framework's validation, not-found and conflict through `TypedResults.Problem`, unhandled exceptions through the global `IExceptionHandler`. See [`csharp-exception-handling.md`](csharp-exception-handling.md).
* Declare every status an endpoint can return (`Produces`, `ProducesProblem`) so it appears in the generated document; an undeclared status is drift.

## Idempotency

* Where a requirement makes a state-changing request retry-safe, the convention is an `X-Idempotency-Key` request header: persist the key with the outcome and return the stored result on replay instead of re-executing. Bound the key's length; malformed keys are 400. Declare the header in the contract. The mechanism is in [`csharp-service-patterns.md`](csharp-service-patterns.md), the multi-instance reasoning in [`../common/concurrency-conventions.md`](../common/concurrency-conventions.md).
* Do not add it, or any other behaviour, without a requirement: the drift check rejects an undeclared header.
* Do not add a custom request-id header for tracing; W3C Trace Context (`traceparent`) is carried by OpenTelemetry already.
