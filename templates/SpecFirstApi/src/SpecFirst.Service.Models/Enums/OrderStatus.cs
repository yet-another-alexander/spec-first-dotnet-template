using System.Text.Json.Serialization;

namespace SpecFirst.Service.Models.Enums;

/// <summary>Lifecycle of an order. Serialised as lower-case strings, the wire vocabulary of this API.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<OrderStatus>))]
public enum OrderStatus
{
    [JsonStringEnumMemberName("submitted")]
    Submitted,

    [JsonStringEnumMemberName("confirmed")]
    Confirmed,

    [JsonStringEnumMemberName("cancelled")]
    Cancelled,
}
