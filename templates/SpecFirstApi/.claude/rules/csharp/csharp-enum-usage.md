---
paths:
  - "src/**/*.{cs,csx,csproj}"
---

# C# Enum Usage

Enums in this codebase never travel as integers: they are strings on the wire and strings in the database. That removes the whole class of "renumbered enum" bugs, and it is what the contract check assumes.

## On the wire

* Contract enums carry `[JsonConverter(typeof(JsonStringEnumConverter<T>))]` and a `[JsonStringEnumMemberName("...")]` per member with the lower-case `snake_case` wire value. The architecture guardrail fails a contract enum without the converter; Spectral fails a contract enum with integer values.
* Every member is listed in the contract (`docs/specs/api/openapi.yaml`, `docs/specs/messages/asyncapi.yaml`). Adding a member is a spec change first.

```csharp
[JsonConverter(typeof(JsonStringEnumConverter<OrderStatus>))]
public enum OrderStatus
{
    [JsonStringEnumMemberName("submitted")]
    Submitted,

    [JsonStringEnumMemberName("confirmed")]
    Confirmed,
}
```

## In the database

* Store as strings: `.HasConversion<string>().HasMaxLength(n)` in the EF configuration. The column type then shows in `schema.sql`.

## In code

* Do not assign integer values and do not rely on ordinal positions; nothing may depend on them.
* The default (first) member is the natural initial state, so a freshly created entity is valid; if there is no natural initial state, add a `None` member first.
* Switch expressions handle every member and end with a default arm that throws; the compiler's exhaustiveness warning is an error.

```csharp
var label = status switch
{
    OrderStatus.Submitted => "Submitted",
    OrderStatus.Confirmed => "Confirmed",
    _ => throw new InvalidOperationException("Unknown order status."),
};
```

* Validate values that arrive from outside the type system (configuration, raw SQL) with `Enum.IsDefined`.

## When not to use an enum

* Open or fast-changing sets are string constants in a static class, or a lookup table.
* Behaviour attached to the values is an "enumeration class" (a sealed class with static instances), not a `switch` in ten places.
