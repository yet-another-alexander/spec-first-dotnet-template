# Tasks

1. **Persistence and health** - done. TECH-001, TECH-002 covered. Files: `Persistence/*`, `PersistenceSetup.cs`,
   `DatabaseBootstrap.cs`.
2. **Error handling** - done. TECH-003 covered. Files: `ProblemDetailsSetup.cs`, `GlobalExceptionHandler.cs`.
3. **Submit, read and confirm orders** - done. REQ-001 to REQ-007, TECH-005, TECH-006 covered. Files: `Orders/*`,
   the `Models` and `Messages` projects, `MessagingSetup.cs`.
4. **Cancel a submitted order** - open. Takes REQ-008 to REQ-011 from work in progress to covered by deleting the four
   `@wip` lines under the cancellation scenarios in `tests/SpecFirst.Service.Tests.Spec/Acceptance/Orders.feature`.
   Files: `Orders/OrderTrigger.cs` (add `Cancel`), `Orders/Order.cs` (permit `Cancel` from Submitted to Cancelled,
   ignore it on Cancelled; Confirm on Cancelled and Cancel on Confirmed stay unhandled and answer 409),
   `Orders/OrderEndpoints.cs` (map `POST /v1/orders/{orderId}/cancel`, operation id `cancelOrder`, and declare the 409
   on both operations as in `docs/specs/api/openapi.yaml`). Done when `dotnet test` is green with the markers removed.
