using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SpecFirst.Service.Tests.Spec.Guardrails;

/// <summary>
/// The first eight hex characters of SHA-256 over the requirement text, trimmed and with every run of whitespace
/// collapsed to one space. The same value from a shell:
/// printf '%s' "$text" | tr -s '[:space:]' ' ' | sed 's/^ //;s/ $//' | shasum -a 256 | cut -c1-8
/// </summary>
public static partial class RequirementHash
{
    [GeneratedRegex(@"\s+")]
    private static partial Regex Whitespace();

    public static string Of(string text)
    {
        var normalised = Whitespace().Replace(text.Trim(), " ");
        return Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(normalised)))[..8];
    }
}
