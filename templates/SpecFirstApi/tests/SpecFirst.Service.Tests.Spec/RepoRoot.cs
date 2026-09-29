using System.Reflection;

namespace SpecFirst.Service.Tests.Spec;

/// <summary>The repository root, injected at build time by the AssemblyMetadata item in the project file.</summary>
public static class RepoRoot
{
    public static string Directory { get; } =
        typeof(RepoRoot)
            .Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
            .Single(a => a.Key == "RepoRoot")
            .Value!;

    public static string Resolve(string relativePath) => Path.Combine(Directory, relativePath);

    public static string Relative(string absolutePath) =>
        Path.GetRelativePath(Directory, absolutePath).Replace('\\', '/');
}
