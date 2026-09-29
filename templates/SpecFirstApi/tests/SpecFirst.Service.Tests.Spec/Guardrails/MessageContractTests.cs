using System.Reflection;
using System.Text.Json;
using NJsonSchema;
using NJsonSchema.Generation;
using SpecFirst.Service.Messages;
using SpecFirst.Service.Messages.Events;
using YamlDotNet.Serialization;

namespace SpecFirst.Service.Tests.Spec.Guardrails;

/// <summary>
/// Contract drift, message side. One-directional: every type in the Messages project that carries [MessageTopic] must be declared in
/// docs/specs/messages/asyncapi.yaml with its topic as a channel address and each of its properties (name, JSON type,
/// required) in the payload schema. Messages in the contract with no type in code yet are pending work, not drift.
/// </summary>
[Collection(GuardrailsCollection.Name)]
public sealed class MessageContractTests
{
    private const string ContractPath = "docs/specs/messages/asyncapi.yaml";

    private static readonly IReadOnlyList<Type> MessageTypes = typeof(OrderConfirmed)
        .Assembly.GetTypes()
        .Where(type => type.GetCustomAttribute<MessageTopicAttribute>() is not null)
        .ToList();

    private static readonly Dictionary<object, object> Contract = new DeserializerBuilder()
        .Build()
        .Deserialize<Dictionary<object, object>>(File.ReadAllText(RepoRoot.Resolve(ContractPath)));

    [Fact]
    public void Every_message_topic_in_code_is_a_channel_in_the_contract()
    {
        var addresses = Section("channels")
            .Values.Select(channel => (string)((Dictionary<object, object>)channel)["address"])
            .ToHashSet();
        var undeclared = MessageTypes
            .Select(type => (type.Name, type.GetCustomAttribute<MessageTopicAttribute>()!.Topic))
            .Where(message => !addresses.Contains(message.Topic))
            .Select(message =>
                $"{message.Name} is published on '{message.Topic}', which is not a channel address in {ContractPath}"
            )
            .ToList();

        undeclared.ShouldBeEmpty(string.Join('\n', undeclared));
    }

    [Fact]
    public void Every_property_of_every_message_type_is_in_the_contract()
    {
        var schemas = (Dictionary<object, object>)Section("components")["schemas"];
        var drift = new List<string>();

        foreach (var type in MessageTypes)
        {
            if (!schemas.TryGetValue(type.Name, out var declaredSchema))
            {
                drift.Add($"{type.Name} has no payload schema components/schemas/{type.Name} in {ContractPath}");
                continue;
            }

            var declared = (Dictionary<object, object>)declaredSchema;
            var declaredProperties = (Dictionary<object, object>)declared["properties"];
            var declaredRequired = declared.TryGetValue("required", out var required)
                ? ((List<object>)required).Cast<string>().ToHashSet()
                : [];
            var generated = JsonSchema.FromType(
                type,
                new SystemTextJsonSchemaGeneratorSettings
                {
                    SerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web),
                }
            );

            foreach (var (name, property) in generated.ActualProperties)
            {
                if (!declaredProperties.TryGetValue(name, out var declaredProperty))
                {
                    drift.Add($"{type.Name}.{name} is not in the {type.Name} payload schema");
                    continue;
                }

                var declaredType = (string)((Dictionary<object, object>)declaredProperty)["type"];
                var generatedType = property.Type.ToString().ToLowerInvariant();
                if (declaredType != generatedType)
                    drift.Add($"{type.Name}.{name} is '{generatedType}' in code and '{declaredType}' in the contract");
                if (property.IsRequired && !declaredRequired.Contains(name))
                    drift.Add($"{type.Name}.{name} is required in code but not in the contract");
            }
        }

        drift.ShouldBeEmpty(
            $"Message types expose what {ContractPath} does not declare. Declare it in a spec PR:\n"
                + string.Join('\n', drift)
        );
    }

    private static Dictionary<object, object> Section(string name) => (Dictionary<object, object>)Contract[name];
}
