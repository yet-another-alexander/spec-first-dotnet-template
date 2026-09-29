# Functional requirements

Behaviour observable by a client of the API. One requirement per `### REQ-nnn` heading, written in EARS:
one sentence, one `shall`, one observable behaviour. The paragraph right after the heading is the requirement text
and is what the requirement hash covers. Anything after a blank line is commentary.

Tags go on the heading line. `@no-test` marks a requirement that cannot be verified by a test; use it sparingly.

Ids are stable: rewording keeps the id, changing the meaning retires the id and creates a new one. Removed requirements
are deleted, not marked deprecated; git has the history. See `docs/workflow.md`.

### REQ-001

The system shall compute the total of an order as the sum of quantity multiplied by unit price over all of its lines.

Ubiquitous requirement: an invariant, proven by a property test over random line sets as well as by the acceptance
scenarios that check a concrete total.

### REQ-002

When a client submits an order with at least one valid line, the system shall store the order with status Submitted
and respond 201 Created with the order representation.

The response carries a `Location` header pointing at the new order.

### REQ-003

If a client submits an order with no lines, a blank SKU, a quantity below 1 or a negative unit price, then the system
shall respond 400 with a validation problem details body.

### REQ-004

When a client requests an existing order by id, the system shall respond 200 with the order including its lines,
status and total.

### REQ-005

If a client requests an order id that does not exist, then the system shall respond 404 with a problem details body.

### REQ-006

When a client confirms an order in status Submitted, the system shall change the status to Confirmed and respond 204.

### REQ-007

While an order is in status Confirmed, the system shall respond 204 to a confirmation request without changing the
order.

Confirmation is idempotent from the client's point of view: retries are safe.

### REQ-008

When a client cancels an order in status Submitted, the system shall change the status to Cancelled and respond 204.

Specified ahead of the implementation on purpose: the endpoint is in `docs/specs/api/openapi.yaml` and the scenario is
tagged `@wip`. It shows how a spec PR lands before any implementation PR. REQ-009 to REQ-011 complete the lifecycle:
every `When` has its `If`, every status its `While`.

### REQ-009

While an order is in status Cancelled, the system shall respond 204 to a cancellation request without changing the
order.

Cancellation is idempotent, like confirmation (REQ-007).

### REQ-010

If a client cancels an order in status Confirmed, then the system shall respond 409 with a problem details body.

A confirmed order is committed; cancelling it is a different business process, out of scope here.

### REQ-011

If a client confirms an order in status Cancelled, then the system shall respond 409 with a problem details body.
