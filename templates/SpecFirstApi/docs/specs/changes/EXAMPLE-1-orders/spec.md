# EXAMPLE-1: Orders

Ticket: EXAMPLE-1 (the sample change that ships with the template).

## Problem

Clients need to submit orders, read them back and confirm them. A submitted order must be verifiable against its
lines, and invalid input must be rejected before anything is stored.

## Requirements

Added: REQ-001 to REQ-011, TECH-001 to TECH-006.

REQ-008 to REQ-011 (cancellation and the lifecycle rules around it) are specified and contracted but not implemented;
their scenarios are work in progress. They stay that way in the template on purpose, as the worked example of a spec
that is ahead of the code.

## Contracts

- `api/openapi.yaml`: `POST /v1/orders`, `GET /v1/orders/{orderId}`, `POST /v1/orders/{orderId}/confirm`,
  `POST /v1/orders/{orderId}/cancel` (pending).
- `db/schema.sql`: tables `orders`, `order_lines`, the EF migrations history.
- `messages/asyncapi.yaml`: channel `orders.confirmed`, message `OrderConfirmed`.

## Out of scope

- Authentication and tenancy: none in the template; add them as TECH requirements in your first real change.
- Cancelling a confirmed order: rejected (REQ-010); a return or refund process is a separate change.
- Payment, shipping, stock: not modelled.
