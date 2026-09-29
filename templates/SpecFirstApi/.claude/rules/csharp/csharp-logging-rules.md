---
paths:
  - "src/**/*.{cs,csx,csproj}"
---

# C# Logging Rules

## General

* Log through `ILogger<T>` from `Microsoft.Extensions.Logging`, injected with the owning class as `T` so the category is right. Never a non-generic `ILogger`, never a logging library's own abstraction.
* Output is structured JSON on stdout outside Development (`LoggingSetup.cs`); collectors take it from there. Do not add file sinks.
* No `Debug` level in production configuration. Review `Information` logs for usefulness; a log line nobody would query is noise.
* No string interpolation in log calls; use message templates.

## Source-generated logging

* New code defines strongly typed log methods with `[LoggerMessage]`; existing `LogInformation`-style calls stay until the file is substantially rewritten.

```csharp
internal static partial class LogMessages
{
    [LoggerMessage(Level = LogLevel.Information, Message = "Processing order {OrderId}")]
    internal static partial void LogProcessingOrder(this ILogger logger, Guid orderId);
}
```

## Structured logging

* Templates are short and constant; aggregators group by template, so variable data goes in properties, never in the text.
* `ILogger.BeginScope` attaches context shared by several statements instead of repeating it.

```csharp
using (logger.BeginScope(new Dictionary<string, object> { ["OrderId"] = orderId }))
{
    logger.LogInformation("Storing order status {Status}", status);
    logger.LogInformation("Order status stored");
}
```

* One property name, one type, everywhere. Indexers reject conflicting types for the same field and drop the events.

```csharp
// BAD — {Command} is a string here and an object elsewhere
logger.LogInformation("Processing {Command}", commandName);
logger.LogInformation("Processing {@Command}", commandObject);
```

* Check argument order against the template; swapped arguments produce wrong types silently.
* Exception details belong to the exception (`Exception.Data`), see [`csharp-exception-handling.md`](csharp-exception-handling.md); pass the exception as the first argument, do not copy its message into the template.
* Never log secrets, tokens, or personal data.
