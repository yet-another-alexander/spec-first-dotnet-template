---
paths:
  - "src/**/*.{cs,csx,csproj}"
  - "tests/**/*.{cs,csx,csproj}"
---

# C# Database & Indexing Rules

C#-specific notes on top of [`../common/database-conventions.md`](../common/database-conventions.md) — **read that first** for the engine-level rules (full-scan avoidance, MongoDB one-index-per-query + ESR ordering, Postgres bitmap-combine vs. multicolumn indexes, covered / index-only reads, `explain` / `EXPLAIN` verification, pagination, index hygiene). This file only adds the .NET-specific conventions.

## Declaring Indexes

* MongoDB: register index models at startup via `IMongoCollection<T>.Indexes` — never create indexes by hand in a database GUI.
* PostgreSQL: declare indexes in the EF Core configuration (`HasIndex`) and let `dotnet ef migrations add` produce the migration. The index then appears in `docs/specs/db/schema.sql`, which a spec PR must already declare.
* Index creation must be reproducible and reviewable in source control.

## Verifying Index Usage in Tests

* Assert index usage (`explain` / `EXPLAIN`) for hot-path queries in integration tests. The spec tests already run a real PostgreSQL through Testcontainers (see [`csharp-testing-rules.md`](csharp-testing-rules.md)), which makes this realistic — never use in-memory fakes for this.
