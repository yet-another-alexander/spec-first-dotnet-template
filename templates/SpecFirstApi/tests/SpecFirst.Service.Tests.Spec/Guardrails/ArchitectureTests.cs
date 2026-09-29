using System.Reflection;
using System.Text.Json.Serialization;
using SpecFirst.Service.Messages.Events;
using SpecFirst.Service.Models.Enums;

namespace SpecFirst.Service.Tests.Spec.Guardrails;

/// <summary>
/// Shape rules the compile-time analyzers cannot express. Namespace dependencies live in config.nsdepcop and forbidden
/// APIs in BannedSymbols.txt; both fail the build, which is the earlier and better place for a rule.
/// </summary>
[Collection(GuardrailsCollection.Name)]
public sealed class ArchitectureTests
{
    /// <summary>Every public type of the two contract projects: the HTTP models and the messages.</summary>
    private static IEnumerable<Type> ContractTypes =>
        new[] { typeof(OrderStatus).Assembly, typeof(OrderConfirmed).Assembly }
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => type.IsPublic);

    [Fact]
    public void Contract_types_are_sealed()
    {
        var open = ContractTypes.Where(type => type.IsClass && !type.IsSealed).Select(type => type.Name).ToList();

        open.ShouldBeEmpty("Contracts are data, not extension points; seal them: " + string.Join(", ", open));
    }

    [Fact]
    public void Contract_enums_serialise_as_strings()
    {
        var numeric = ContractTypes
            .Where(type => type.IsEnum && type.GetCustomAttribute<JsonConverterAttribute>() is null)
            .Select(type => type.Name)
            .ToList();

        numeric.ShouldBeEmpty(
            "Integer enums leak ordinal positions into the contract; add JsonStringEnumConverter: "
                + string.Join(", ", numeric)
        );
    }
}
