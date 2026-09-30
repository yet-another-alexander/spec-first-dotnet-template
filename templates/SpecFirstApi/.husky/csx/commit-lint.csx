using System.IO;
using System.Text.RegularExpressions;

var lines = File.ReadAllLines(Args[0]);
var subject = lines.Length > 0 ? lines[0] : string.Empty;

var gitPrefixes = new[] { "Merge", "Revert", "Squash", "fixup!", "squash!" };
if (Array.Exists(gitPrefixes, p => subject.StartsWith(p, StringComparison.Ordinal)))
    return 0;

const string pattern =
    @"^(?=.{1,90}\z)(?:build|chore|ci|docs|feat|fix|ops|perf|refactor|revert|spec|style|test)(?:\([^)]+\))?!?: .{5,}(?<![\.\s])\z";

if (Regex.IsMatch(subject, pattern))
    return 0;

Console.Error.WriteLine("Invalid commit message. The first line must match Conventional Commits:");
Console.Error.WriteLine("  feat(orders): add order cancellation");
Console.Error.WriteLine("  spec(orders): REQ-008 cancel a submitted order");
Console.Error.WriteLine("  fix!: reject negative unit prices");
Console.Error.WriteLine(
    "Types: build chore ci docs feat fix ops perf refactor revert spec style test. Max 90 characters, no trailing period."
);
return 1;
