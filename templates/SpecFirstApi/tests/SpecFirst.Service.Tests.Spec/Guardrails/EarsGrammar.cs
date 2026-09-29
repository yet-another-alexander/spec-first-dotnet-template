using System.Text.RegularExpressions;

namespace SpecFirst.Service.Tests.Spec.Guardrails;

/// <summary>
/// The mechanical part of EARS (docs/workflow.md section 1): one sentence, the system as subject, one `shall`, and one
/// of the five patterns combined in the order Where, While, When or If. Whether the response is observable or a number
/// replaced an adjective is judgement, and the spec-reviewer agent's job.
/// </summary>
public static partial class EarsGrammar
{
    // Where X, While Y, then exactly one of: When X, the system shall | If X, then the system shall | The system shall.
    // A clause (X, Y) may not contain ", <keyword> " so patterns cannot appear out of order inside another clause.
    private const string Clause = @"(?:(?!, (?:where|while|when|if) ).)+?";

    [GeneratedRegex(
        @"^(?:where "
            + Clause
            + @", (?=while |when |if |the system shall))?"
            + @"(?:while "
            + Clause
            + @", (?=when |if |the system shall))?"
            + @"(?:when "
            + Clause
            + @", (?=the system shall)|if "
            + Clause
            + @", then (?=the system shall))?"
            + @"the system shall .+\.$",
        RegexOptions.IgnoreCase | RegexOptions.Singleline
    )]
    private static partial Regex Pattern();

    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    /// <summary>Null when the text is a well-formed EARS sentence, otherwise what is wrong with it.</summary>
    public static string? Violation(string text)
    {
        var sentence = Whitespace().Replace(text.Trim(), " ");
        if (sentence.Length == 0)
            return "is empty";
        if (!char.IsUpper(sentence[0]))
            return "must start with a capital letter";
        if (!sentence.EndsWith('.'))
            return "must end with a period";
        if (sentence.Contains(". ", StringComparison.Ordinal))
            return "must be one sentence";
        if (!Pattern().IsMatch(sentence))
            return "must be one of: 'The system shall …', 'When X, the system shall …', 'While X, the system shall …', "
                + "'If X, then the system shall …', 'Where X, the system shall …', combined in the order Where, While, When/If";
        return null;
    }
}
