---
paths:
  - "src/**/*.{cs,csx,csproj}"
---

# C# Exception Handling

## Exceptions vs. results

* Exceptions signal **unexpected** failures only. Expected outcomes (not found, not allowed in this status, validation) are return values: a typed HTTP result in an endpoint, a `bool`/enum/result record in domain code (`Order.TryFire` returns `false`, it does not throw).
* Never use exceptions for flow control.

## Throwing

* Messages are short and constant. Error trackers group issues by exception type and message, so an interpolated identifier produces one issue per value and drowns the signal.
* Attach context through `Exception.Data`; trackers and structured loggers record it as properties.

```csharp
// BAD — one issue per payment
throw new PaymentStateException($"Unable to store payment state for {paymentId} in state {state}");

// GOOD
var ex = new PaymentStateException("Unable to store payment state");
ex.Data.Add("PaymentId", paymentId);
ex.Data.Add("State", state);
throw ex;
```

## Catching

* Catch only what you can handle meaningfully, as close to the throw as possible.
* Never `catch (Exception)` in application code. The one exception is the global `IExceptionHandler`, the single fallback for unhandled exceptions.

## Surfacing over HTTP

* Unhandled exceptions become a 500 Problem Details response with a constant title and no exception text (TECH-003). The details go to the log. Never leak a message, a type name or a stack trace to the client.
