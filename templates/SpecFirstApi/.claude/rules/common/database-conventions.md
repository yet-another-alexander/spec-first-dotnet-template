# Database & Indexing Conventions

> **How to use this rule:** Applies to all data access. This template ships with **PostgreSQL**; the **MongoDB** section is kept for services that add it, because the two engines differ in a way that is easy to get wrong. The principles below are engine-level — they hold regardless of language or driver. For the .NET way to declare indexes in code, see `../csharp/csharp-database-rules.md`, which builds on this file. Examples here use the MongoDB shell and SQL.

The two engines differ in one critical way — **do not carry MongoDB's "one index per query" assumption over to Postgres** (see the per-engine sections).

## Don't Let Production Queries Fall Back to a Full Scan (both engines)

* Any query, sort, or filter that runs regularly in production against a collection/table that grows must be served by an appropriate index. Treat an unexpected full scan (`COLLSCAN` in MongoDB, `Seq Scan` in PostgreSQL) on a large collection as a defect, not a performance nuance to revisit later.
* A full scan is sometimes the *correct* plan — don't force an index in these cases:
  * **Small, bounded collections** (e.g. static reference data) — the planner will scan them anyway; an index is pure write + RAM overhead.
  * **Low-selectivity queries** that return a large fraction of the data — a sequential scan is genuinely faster than random index lookups plus document/heap fetches, and the planner will (correctly) choose it. Confirm with the planner rather than assuming.
  * **One-off** admin, migration, or batch/analytics queries that intentionally read everything.
* Define indexes in code or migrations — never create them by hand in a database GUI. Index creation must be reproducible and reviewable.
* Unindexed hot-path queries pass tests on small datasets and silently degrade in production. Index the fields such a query filters and sorts on — not just the primary key.

## Prefer Covered / Index-Only Reads; Project Only What You Need (both engines)

* A read is fastest when the index contains every field the query touches — both **filtered** and **returned** — so the engine never reads the documents/table: a **covered query** (MongoDB) / **index-only scan** (PostgreSQL). This is a niche optimization: it pays off only when you return a few fields, and rarely applies when deserializing whole entities.
* Always project to the fields you actually use; returning whole documents/rows prevents index-only reads and wastes bandwidth.
* PostgreSQL: add the extra returned columns with `INCLUDE` to enable index-only scans — e.g. `CREATE INDEX ix ON orders (tenant_id, status) INCLUDE (total)`.

## Verify With the Query Planner — Don't Guess (both engines)

* MongoDB: `explain("executionStats")` — the winning plan must use `IXSCAN`, never `COLLSCAN`, with no blocking `SORT` stage for sorted queries; `totalDocsExamined` should be close to `nReturned`.
* PostgreSQL: `EXPLAIN (ANALYZE, BUFFERS)` — expect `Index Scan` / `Index Only Scan`, not `Seq Scan`; watch for high `Rows Removed by Filter` (index not selective enough) and unexpected `Sort` nodes.
* Cover hot-path queries with a planner assertion in integration tests that run a real database (e.g. Testcontainers), never an in-memory fake.

## Bound Result Sets and Writes (both engines)

* Never issue unbounded reads on growing collections/tables — always paginate. Prefer keyset / `_id` / indexed-column pagination over large `skip` / `OFFSET`, which still scan the skipped rows.
* Index the filter fields of frequent `UpdateMany` / `DeleteMany` / `UPDATE` / `DELETE` operations too. An unindexed write filter scans the whole collection/table and holds locks longer.

## Index Hygiene (both engines)

* Indexes cost write throughput and RAM — the index working set should fit in memory. Don't add an index per field "just in case"; prefer a few well-ordered compound/multicolumn indexes.
* Drop redundant indexes: a single-field index is redundant once a compound/multicolumn index has it as the **leading** key, because prefix queries are served by that index.

---

## MongoDB: One Index Per Query — Design Compound Indexes

This is the most common source of slow MongoDB queries:

* For a given query, MongoDB's planner picks a **single winning plan that uses one index**. It does **not** combine several single-field indexes to satisfy one query.
* Index intersection technically exists, but the planner rarely selects it, it cannot satisfy a sort efficiently, and you must **never rely on it**. Two single-field indexes (`{a:1}` and `{b:1}`) will not be merged to serve `find({a, b})` — MongoDB uses one and scans the rest of the matches.
* Therefore create a **compound index** that covers the whole query shape — all equality fields, the sort, and range fields — in one index.

```js
// BAD — two single-field indexes for a query that filters on both fields.
// MongoDB uses only ONE of them and scans the rest.
db.orders.createIndex({ tenantId: 1 })
db.orders.createIndex({ status: 1 })

// GOOD — one compound index serving the full query
db.orders.createIndex({ tenantId: 1, status: 1, createdAt: 1 })
```

### Order Compound Index Fields by ESR (Equality, Sort, Range)

* Arrange compound index keys as **Equality → Sort → Range**:
  * **Equality** fields first — exact matches (`$eq`, `$in`).
  * **Sort** fields next — lets MongoDB return already-sorted results from the index and avoid an in-memory (blocking) `SORT` stage.
  * **Range** fields last (`$gt`, `$lt`, `$gte`, `$lte`, `$ne`).
* A compound index also serves queries on a **prefix** of its keys — order keys so the most-reused prefixes come first, so one index can replace several.
* When a range predicate is far more selective than the sort, ERS (range before sort) can win — measure both with `explain`.

```
// Query: find({ directors: x, runtime: { $lt: 130 } }).sort({ year: 1 })
// ESR index:           { directors: 1, year: 1, runtime: 1 }
//                        equality      sort      range
```

---

## PostgreSQL: Indexes CAN Combine — but Prefer One Multicolumn Index

PostgreSQL behaves **oppositely** to MongoDB here — do not carry the "one index only" rule over:

* Postgres **can** combine multiple indexes for a single query via **bitmap scans** (`BitmapAnd` / `BitmapOr`). Two single-column indexes on `a` and `b` *can* serve `WHERE a = ? AND b = ?` (or an `OR`).
* **But a single multicolumn index is usually faster** for a frequent query: bitmap combination adds overhead and loses index ordering, forcing a separate `Sort` for `ORDER BY`. So create a **multicolumn index** for hot query paths; lean on combinable single-column indexes only for ad-hoc / less-frequent column combinations.

```sql
-- multicolumn index for the hot query path
CREATE INDEX ix_orders_tenant_status_created ON orders (tenant_id, status, created_at);
```

### Column Order, Partial & Covering Indexes

* Multicolumn B-tree column order mirrors MongoDB's ESR: put **equality** columns first, then the columns matching the query's sort/range. The leading column(s) must appear in the query for the index to be used efficiently.
* Use **partial indexes** for queries that always filter to a subset — smaller and cheaper: `CREATE INDEX ix ON orders (created_at) WHERE status = 'active'`.
* Use **covering indexes** (`INCLUDE`) to get index-only scans, as shown above.
