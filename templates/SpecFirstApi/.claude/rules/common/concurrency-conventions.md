# Concurrency & Multi-Instance Conventions

> **How to use this rule:** Assume the service runs as **two or more instances in every environment** (for availability and zero-downtime deploys). Your code is therefore always executing concurrently with identical copies of itself. Assume this by default when you *write* service code, and flag violations when you *review* it. The remedies below are stack-agnostic principles — reach for your stack's concrete primitives (locks, message dedup, atomic store operations) to implement them.

---

## Assume more than one instance

- There is no "the instance." Any request, message, or scheduled tick may land on any replica, and two replicas may handle related work at the same moment.
- During rolling deploys, the old and new versions of the service run at the same time — your change must behave correctly side-by-side with the previous version.
- Treat single-instance behavior as a bug, not a shortcut — it will break the first time the service scales or a deploy briefly runs two versions together.

## Don't rely on in-process state

- In-memory state is **per-instance and ephemeral**: caches, counters, accumulators, and dictionaries are not shared between instances and vanish on restart or scale-down. Never treat them as the source of truth.
- Keep durable or shared state in an external store (database, Redis, etc.) so every instance sees the same data.
- In-process locks (`lock`, mutexes, semaphores) only coordinate threads **within one instance** — they do not prevent two instances from doing the same thing. Use them only for genuinely in-instance concerns.
- Local caches are fine as a performance optimization, but design for them being cold, stale, or divergent between instances.

## Make handlers idempotent

- Requests and messages can be delivered to any instance and **retried** (at-least-once delivery, client retries, redeploys). Processing the same input twice must not double-charge, double-send, or corrupt state.
- Key writes on a stable identifier (idempotency key, message id, business key) so a replay is a no-op.
- See `../csharp/csharp-service-patterns.md` for the concrete idempotency and outbox mechanisms (CAP's transactional outbox in this template).

## Coordinate run-once work

- Work that must happen **exactly once** across the fleet — scheduled jobs, cron ticks, one-off migrations, batch kickoffs — cannot assume a single instance runs it. By default *every* instance will run it.
- Use a distributed lock or leader election so only one instance performs the work while the rest stand by.
- Never gate this on instance ordinal or hostname ("only instance 0 does it") — ordinals and hostnames are not stable across restarts and scaling.
