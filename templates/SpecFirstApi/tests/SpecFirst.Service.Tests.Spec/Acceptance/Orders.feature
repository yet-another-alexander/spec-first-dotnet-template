Feature: Orders
  Submitting, reading, confirming and cancelling orders over HTTP.
  Requirements: docs/specs/requirements/functional.md. Every scenario is tagged with the id of the requirement it
  proves and the hash of that requirement's text. A scenario whose behaviour is not implemented yet carries a
  work-in-progress tag alone on its own line.

  @REQ-002:9a317f36 @REQ-001:7f6cdb52
  Scenario: Submitting a valid order creates it
    When a client submits an order with lines
      | sku   | quantity | unitPrice |
      | APPLE | 3        | 1.50      |
      | PEAR  | 1        | 2.25      |
    Then the response status is 201
    And the response is an order with status "submitted" and total 6.75
    And the Location header points to the created order

  @REQ-003:6dfdd854
  Scenario: Submitting an order without lines is rejected
    When a client submits an order with no lines
    Then the response status is 400
    And the response is a validation problem

  @REQ-003:6dfdd854
  Scenario Outline: Submitting an order with an invalid line is rejected
    When a client submits an order with lines
      | sku   | quantity   | unitPrice   |
      | <sku> | <quantity> | <unitPrice> |
    Then the response status is 400
    And the response is a validation problem

    Examples:
      | sku   | quantity | unitPrice |
      |       | 1        | 1.00      |
      | APPLE | 0        | 1.00      |
      | APPLE | 1        | -0.01     |

  @REQ-004:f483965c
  Scenario: Reading an existing order
    Given a submitted order with lines
      | sku   | quantity | unitPrice |
      | APPLE | 2        | 1.00      |
    When the client reads the order
    Then the response status is 200
    And the response is an order with status "submitted" and total 2.00

  @REQ-005:e468e85a
  Scenario: Reading an unknown order
    When the client reads order "00000000-0000-0000-0000-000000000001"
    Then the response status is 404
    And the response is a problem

  @REQ-006:4377a453
  Scenario: Confirming a submitted order
    Given a submitted order with lines
      | sku   | quantity | unitPrice |
      | APPLE | 1        | 1.00      |
    When the client confirms the order
    Then the response status is 204
    And reading the order shows status "confirmed"

  @REQ-007:482ad978
  Scenario: Confirming a confirmed order changes nothing
    Given a submitted order with lines
      | sku   | quantity | unitPrice |
      | APPLE | 1        | 1.00      |
    And the order has been confirmed
    When the client confirms the order
    Then the response status is 204
    And reading the order shows status "confirmed"

  @REQ-008:ea5bc75e
  @wip
  Scenario: Cancelling a submitted order
    Given a submitted order with lines
      | sku   | quantity | unitPrice |
      | APPLE | 1        | 1.00      |
    When the client cancels the order
    Then the response status is 204
    And reading the order shows status "cancelled"

  @REQ-009:749795da
  @wip
  Scenario: Cancelling a cancelled order changes nothing
    Given a submitted order with lines
      | sku   | quantity | unitPrice |
      | APPLE | 1        | 1.00      |
    And the order has been cancelled
    When the client cancels the order
    Then the response status is 204
    And reading the order shows status "cancelled"

  @REQ-010:483f1b8e
  @wip
  Scenario: Cancelling a confirmed order is rejected
    Given a submitted order with lines
      | sku   | quantity | unitPrice |
      | APPLE | 1        | 1.00      |
    And the order has been confirmed
    When the client cancels the order
    Then the response status is 409
    And the response is a problem
    And reading the order shows status "confirmed"

  @REQ-011:e2592d4a
  @wip
  Scenario: Confirming a cancelled order is rejected
    Given a submitted order with lines
      | sku   | quantity | unitPrice |
      | APPLE | 1        | 1.00      |
    And the order has been cancelled
    When the client confirms the order
    Then the response status is 409
    And the response is a problem
    And reading the order shows status "cancelled"
