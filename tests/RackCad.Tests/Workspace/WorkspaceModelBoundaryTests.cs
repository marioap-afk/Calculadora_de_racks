using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using RackCad.Application.Workspace;
using Xunit;

namespace RackCad.Tests;

// I-64 F1-T1-MODEL: INV-F1-T1-01, 03, 04, 08, 10 as source and shape guards over the pure model.
public class WorkspaceModelBoundaryTests
{
    private static string WorkspaceDir()
    {
        var dir = AppContext.BaseDirectory;
        while (dir != null && !File.Exists(Path.Combine(dir, "RackCad.sln")))
            dir = Path.GetDirectoryName(dir);
        Assert.NotNull(dir);
        return Path.Combine(dir!, "src", "RackCad.Application", "Workspace");
    }

    private static string CodeOnly(string text) =>
        Regex.Replace(Regex.Replace(text, @"/\*.*?\*/", "", RegexOptions.Singleline), @"//[^\r\n]*", "");

    [Fact]
    public void ModelFilesExistAndHaveNoAutoCadWpfOrPluginReferences()
    {
        var files = Directory.GetFiles(WorkspaceDir(), "*.cs", SearchOption.AllDirectories);
        Assert.NotEmpty(files);
        foreach (var file in files)
        {
            var code = CodeOnly(File.ReadAllText(file));
            Assert.DoesNotMatch(new Regex(@"\bAutodesk\."), code);
            Assert.DoesNotMatch(new Regex(@"System\.Windows|RackCad\.Plugin|RackCad\.UI"), code);
        }
    }

    // Concrete persistence / authored-write authorities and forbidden APIs. The bare token "Registry" is
    // deliberately NOT banned: WorkspaceSessionRegistry is the in-memory session registry.
    private static readonly Regex PersistenceOrWriteAuthorities = new(
        @"System\.IO|\bFile\.|\bDirectory\.|\bStreamWriter\b|Microsoft\.Win32|\bRegistry\.|\bUserSettings\b|\bAtomicFile\b|Transaction|LockDocument|\bDatabase\b|\bCommit\(|\bAutodesk\.");

    [Fact]
    public void ModelDoesNotPersistNorWriteAuthoredData()
    {
        foreach (var file in Directory.GetFiles(WorkspaceDir(), "*.cs", SearchOption.AllDirectories))
            Assert.DoesNotMatch(PersistenceOrWriteAuthorities, CodeOnly(File.ReadAllText(file)));
    }

    [Theory]
    [InlineData("using System.IO;")]
    [InlineData("var t = File.ReadAllText(p);")]
    [InlineData("Directory.CreateDirectory(p);")]
    [InlineData("using var w = new StreamWriter(p);")]
    [InlineData("Microsoft.Win32.Registry.CurrentUser.OpenSubKey(k);")]
    [InlineData("Registry.CurrentUser.SetValue(k, v);")]
    [InlineData("UserSettings.Save();")]
    [InlineData("AtomicFile.Write(p, t);")]
    [InlineData("using var tr = db.TransactionManager;")]
    [InlineData("var l = doc.LockDocument();")]
    [InlineData("Database db = null;")]
    [InlineData("tr.Commit();")]
    [InlineData("using Autodesk.AutoCAD.DatabaseServices;")]
    public void BoundaryGuardDetectsEachForbiddenPersistenceFamilyInRealModelText(string injected)
    {
        var real = CodeOnly(File.ReadAllText(Path.Combine(WorkspaceDir(), "WorkspaceSession.cs")));
        Assert.DoesNotMatch(PersistenceOrWriteAuthorities, real);
        Assert.Matches(PersistenceOrWriteAuthorities, real + "\n" + injected);
    }

    [Fact]
    public void BoundaryGuardDoesNotRejectTheInMemorySessionRegistry()
    {
        var registry = CodeOnly(File.ReadAllText(Path.Combine(WorkspaceDir(), "WorkspaceSessionRegistry.cs")));
        Assert.Contains("WorkspaceSessionRegistry", registry);
        Assert.DoesNotMatch(PersistenceOrWriteAuthorities, registry);
    }

    [Fact]
    public void ModelHasNoMetricsProviderOrProjectSummaryConcepts()
    {
        var banned = new Regex(@"ProjectSummary|Metric|Provider|TotalRacks|Aggregat|Bom\b", RegexOptions.IgnoreCase);
        foreach (var type in typeof(WorkspaceSession).Assembly.GetTypes()
                     .Where(t => t.Namespace == "RackCad.Application.Workspace"))
        {
            Assert.DoesNotMatch(banned, type.Name);
            foreach (var m in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                Assert.DoesNotMatch(banned, m.Name);
        }
    }

    [Fact]
    public void ThereIsNoPerTabStateOrPerTabSubscriptions()
    {
        var types = typeof(WorkspaceSession).Assembly.GetTypes()
            .Where(t => t.Namespace == "RackCad.Application.Workspace").ToList();
        Assert.Contains(types, t => t == typeof(WorkspaceSession));
        Assert.DoesNotContain(types, t => Regex.IsMatch(t.Name, "Tab|Subscription|Editor|Draft", RegexOptions.IgnoreCase));
        foreach (var p in typeof(WorkspaceSession).GetProperties())
            Assert.DoesNotMatch(new Regex("Tab|Subscription|Draft|Handler", RegexOptions.IgnoreCase), p.Name);
    }

    [Fact]
    public void ModelTypesLiveInTheWorkspaceNamespaceOfApplication()
    {
        Assert.Equal("RackCad.Application.Workspace", typeof(WorkspaceSessionRegistry).Namespace);
        Assert.Equal(typeof(WorkspaceSession).Assembly, typeof(SelectionContext).Assembly);
        Assert.Equal("RackCad.Application", typeof(WorkspaceSession).Assembly.GetName().Name);
    }
}
