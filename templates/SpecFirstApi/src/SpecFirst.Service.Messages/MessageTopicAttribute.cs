namespace SpecFirst.Service.Messages;

/// <summary>The topic a message type is published on; the channel address in docs/specs/messages/asyncapi.yaml.</summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class MessageTopicAttribute(string topic) : Attribute
{
    public string Topic { get; } = topic;
}
