#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace RackCad.Tests;

/// <summary>
/// I-64 F1-T3A-HOST-CORE: shared helpers of the source guards. The Core test project cannot reference the Plugin (it
/// needs AutoCAD), so the guards of the adapters read the sources as text from the repository root.
/// </summary>
internal static class WorkspaceHostSource
{
    public const string PluginWorkspace = "src/RackCad.Plugin/Workspace/";
    public const string ApplicationWorkspace = "src/RackCad.Application/Workspace/";

    public static string RepoRoot()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, "RackCad.sln")))
            dir = Path.GetDirectoryName(dir);
        Assert.NotNull(dir);
        return dir!;
    }

    public static string CodeOnly(string text) =>
        Regex.Replace(Regex.Replace(text, @"/\*.*?\*/", "", RegexOptions.Singleline), @"//[^\r\n]*", "");

    public static bool Exists(string relative) => File.Exists(Path.Combine(RepoRoot(), relative.Replace('/', Path.DirectorySeparatorChar)));

    public static string Code(string relative)
    {
        var full = Path.Combine(RepoRoot(), relative.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(full), relative + " does not exist");
        return CodeOnly(File.ReadAllText(full));
    }

    public static string Text(string relative)
    {
        var full = Path.Combine(RepoRoot(), relative.Replace('/', Path.DirectorySeparatorChar));
        Assert.True(File.Exists(full), relative + " does not exist");
        return File.ReadAllText(full);
    }

    public static IReadOnlyList<(string Path, string Code)> Files(string folder)
    {
        var dir = Path.Combine(RepoRoot(), folder.TrimEnd('/').Replace('/', Path.DirectorySeparatorChar));
        if (!Directory.Exists(dir)) return Array.Empty<(string, string)>();
        return Directory.GetFiles(dir, "*.cs", SearchOption.AllDirectories)
            .Where(f => !f.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar)
                        && !f.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar))
            .Select(f => (Path.GetRelativePath(RepoRoot(), f).Replace(Path.DirectorySeparatorChar, '/'), CodeOnly(File.ReadAllText(f))))
            .OrderBy(p => p.Item1, StringComparer.Ordinal)
            .ToList();
    }

    public static IReadOnlyList<(string Path, string Code)> PluginFiles() => Files(PluginWorkspace);

    public static IReadOnlyList<(string Path, string Code)> ApplicationFiles() => Files(ApplicationWorkspace);

    public static IReadOnlyList<(string Path, string Code)> AllProduct() => Files("src");

    /// <summary>The text of a member body, from its signature to the matching closing brace (or to the end of an expression body).</summary>
    public static string BodyOf(string code, string signaturePattern)
    {
        var match = Regex.Match(code, signaturePattern);
        Assert.True(match.Success, "member not found: " + signaturePattern);
        var start = match.Index;
        var open = code.IndexOf('{', match.Index + match.Length - 1);
        var arrow = code.IndexOf("=>", match.Index + match.Length - 1, StringComparison.Ordinal);
        if (arrow >= 0 && (open < 0 || arrow < open))
        {
            var end = code.IndexOf(';', arrow);
            return code.Substring(start, end - start + 1);
        }

        Assert.True(open >= 0, "no body for " + signaturePattern);
        var depth = 0;
        for (var i = open; i < code.Length; i++)
        {
            if (code[i] == '{') depth++;
            else if (code[i] == '}' && --depth == 0) return code.Substring(start, i - start + 1);
        }

        throw new InvalidOperationException("unbalanced braces for " + signaturePattern);
    }

    public static int Count(string code, string pattern) => Regex.Matches(code, pattern).Count;
}
