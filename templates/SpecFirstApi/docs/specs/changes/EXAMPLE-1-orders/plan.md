# Plan

- **Orders feature folder** (`src/SpecFirst.Service.Api/Orders/`): `Order` and `OrderLine` entities with EF
  configurations, the order lifecycle as a Stateless state machine in `Order.TryFire` (REQ-006, REQ-007), `OrderTotal`
  as a pure function (REQ-001), `OrderEndpoints` mapping the three implemented operations under `/v1/orders`
  (REQ-002 to REQ-007).
- **Contracts** (`src/SpecFirst.Service.Models/`): request and response records with DataAnnotations; the framework's validation turns
  them into 400 validation problems (REQ-003). `OrderStatus` serialises as lower-case strings.
- **Persistence**: `AppDbContext` applying configurations from the assembly; migration `InitialSchema`; migrations run
  at startup (TECH-001); health check on the context (TECH-002).
- **Messaging** (`MessagingSetup.cs`, `src/SpecFirst.Service.Messages/`): CAP with the outbox in the same Postgres and RabbitMQ
  transport; `OrderConfirmed` published inside the confirm transaction (TECH-005), never on the idempotent path
  (TECH-006).
- **Errors**: problem details everywhere; `GlobalExceptionHandler` logs and answers 500 without details (TECH-003).
- **Sequencing**: persistence and error handling first, then the endpoints, then cancellation in a later change.
