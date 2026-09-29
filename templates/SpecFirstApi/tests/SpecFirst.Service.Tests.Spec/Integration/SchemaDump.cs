using System.Text;

namespace SpecFirst.Service.Tests.Spec.Integration;

/// <summary>
/// Reduces a pg_dump --schema-only output to its statements: no comments, no session settings, no psql meta-commands,
/// no blank lines, and the lines of one statement joined up to its terminating semicolon. Comparing statements rather
/// than lines keeps a column inside its own CREATE TABLE. Both the live dump and the committed contract go through this.
/// </summary>
public static class SchemaDump
{
    private static readonly string[] Noise = ["--", "\\", "SET ", "SELECT pg_catalog.set_config"];

    public static IReadOnlyList<string> Normalise(string dump)
    {
        var statements = new List<string>();
        var current = new StringBuilder();

        foreach (var raw in dump.Split('\n'))
        {
            var line = raw.TrimEnd();
            if (line.Length == 0 || Noise.Any(noise => line.StartsWith(noise, StringComparison.Ordinal)))
                continue;

            if (current.Length > 0)
                current.Append('\n');
            current.Append(line);

            if (line.EndsWith(';'))
            {
                statements.Add(current.ToString());
                current.Clear();
            }
        }

        if (current.Length > 0)
            statements.Add(current.ToString());

        return statements;
    }
}
