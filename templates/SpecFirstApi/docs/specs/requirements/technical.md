# Technical requirements

Behaviour observable by the system itself and verifiable by a test: migrations, health, error handling, idempotency
of handlers. Same format as `functional.md`: one `### TECH-nnn` heading, one EARS sentence, optional commentary after a
blank line. Decisions that no test can fail belong in `docs/adr/`, not here.

### TECH-001

The system shall apply all pending database migrations at startup before serving requests.

### TECH-002

While the database is reachable, the system shall respond 200 to GET /health.

### TECH-003

If an unhandled exception occurs while handling a request, then the system shall respond 500 with a problem details
body that does not contain the exception message.

Exception details go to the log, never to the client.

### TECH-004 @no-test

The system shall be deployable as a single container image that runs as a non-root user.

Verified by the Docker smoke job in CI rather than by a test with a requirement id, hence `@no-test`.

### TECH-005

When an order is confirmed, the system shall publish an OrderConfirmed message carrying the order id.

The message is written to the outbox in the same transaction as the status change and relayed to the broker
afterwards; the contract is `docs/specs/messages/asyncapi.yaml`.

### TECH-006

While an order is in status Confirmed, the system shall not publish an OrderConfirmed message on a confirmation
request.

Confirmation is idempotent (REQ-007); a retried confirmation must not fan out a second event.
