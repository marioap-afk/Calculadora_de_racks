#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using RackCad.Application.Workspace;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F3-T1-BROWSER-CORE: shape and source guards of the pure browser core (INV-F3T1-01, 14, 15, 16).
public class WorkspaceBrowserBoundaryTests
{
    private const string SessionPath = "src/RackCad.Application/Workspace/BrowserSession.cs";
    private const string RegistryPath = "src/RackCad.Application/Workspace/BrowserSessionRegistry.cs";

    private static readonly string[] BrowserFiles = { SessionPath, RegistryPath };

    private static readonly Type[] BrowserTypes =
    {
        typeof(BrowserTabStatus), typeof(BrowserChange), typeof(BrowserTabRecord), typeof(BrowserSession), typeof(BrowserSessionRegistry),
    };

    private static readonly BindingFlags AllDeclared =
        BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

    // ------------------------------------------------------------------ INV-F3T1-14: purity of the source

    [Fact]
    public void TheBrowserFilesExistInTheWorkspaceFolderWithOneBlockNamespaceAndNullableEnabled()
    {
        foreach (var path in BrowserFiles)
        {
            Assert.True(Src.Exists(path), path);
            var text = Src.Text(path);
            Assert.StartsWith("#nullable enable", text.TrimStart('﻿'), StringComparison.Ordinal);
            Assert.Equal(1, Src.Count(Src.CodeOnly(text), @"\bnamespace\s+RackCad\.Application\.Workspace\b"));
            Assert.Equal(1, Src.Count(Src.CodeOnly(text), @"\bnamespace\b"));
        }
    }

    [Fact]
    public void TheBrowserFilesUseNoAutoCadWpfPluginOrUiType()
    {
        foreach (var path in BrowserFiles)
        {
            var code = Src.Code(path);
            Assert.DoesNotMatch(new Regex(@"\bAutodesk\.|System\.Windows|RackCad\.Plugin|RackCad\.UI|\bUserControl\b|\bDispatcher\b|PaletteSet"), code);
        }
    }

    [Fact]
    public void TheBrowserFilesDeclareNoEventDelegateTaskTimerThreadIoOrStaticState()
    {
        var forbidden = new Regex(
            @"\bevent\b|\bdelegate\b|\bAction\b|\bAction<|\bFunc<|\bPredicate<|\bEventHandler\b|\bTask\b|\bTask<|\bValueTask\b|\basync\b|\bawait\b"
            + @"|\bTimer\b|System\.Timers|\bThread\b|ThreadPool|System\.Threading|Parallel\.|System\.IO|\bFile\.|\bDirectory\.|\bStreamWriter\b|Microsoft\.Win32"
            + @"|\bConsole\b|\bDateTime\b|\bStopwatch\b|\bstatic\b|\block\b|\bvolatile\b|Environment\.");

        foreach (var path in BrowserFiles)
        {
            var code = Src.Code(path);
            Assert.True(!forbidden.IsMatch(code), path + " uses " + forbidden.Match(code).Value);
        }
    }

    [Fact]
    public void TheBrowserFilesNameNoNavigationHostOrSessionType()
    {
        var forbidden = new Regex(
            @"\bNavigationTargets\b|\bNavigationTargetSet\b|\bNavigationDefinitionFact\b|\bINavigationFactsPort\b|\bIWorkspaceNavigationPort\b"
            + @"|\bIWorkspaceHostPort\b|\bWorkspaceSession\b|\bWorkspaceSessionRegistry\b");

        foreach (var path in BrowserFiles)
        {
            var code = Src.Code(path);
            Assert.True(!forbidden.IsMatch(code), path + " names " + forbidden.Match(code).Value);
        }
    }

    [Fact]
    public void TheBrowserFilesUseNoOtherWorkspaceTypeThanSessionIdAndSelectionContext()
    {
        var allowed = new HashSet<string>(StringComparer.Ordinal) { "SessionId", "SelectionContext" };
        var workspaceNames = typeof(WorkspaceSession).Assembly.GetTypes()
            .Where(t => t.Namespace == "RackCad.Application.Workspace" && !t.Name.StartsWith("Browser", StringComparison.Ordinal) && !t.Name.StartsWith("<", StringComparison.Ordinal))
            .Select(t => t.Name)
            .Where(n => !allowed.Contains(n))
            .Distinct(StringComparer.Ordinal)
            .ToList();
        Assert.NotEmpty(workspaceNames);

        foreach (var path in BrowserFiles)
        {
            var code = Src.Code(path);
            foreach (var name in workspaceNames)
                Assert.True(!Regex.IsMatch(code, @"\b" + Regex.Escape(name) + @"\b"), path + " uses the Workspace type " + name);
        }
    }

    [Fact]
    public void TheBrowserFilesCarryNoMetricsProviderSummaryQueryOrAggregationConcept()
    {
        var concepts = new Regex(@"ComputedParameter|Metric|ProjectSummary|TotalRacks|Aggregat|Provider|\bBom|Transaction|\bDatabase\b|\bCommit\(|Subscription|Draft|\bEditor\b|\bDirty\b", RegexOptions.IgnoreCase);

        foreach (var path in BrowserFiles)
        {
            var code = Src.Code(path);
            Assert.True(!concepts.IsMatch(code), path + " mentions " + concepts.Match(code).Value);
        }
    }

    [Theory]
    [InlineData("public event System.Action Changed;")]
    [InlineData("public delegate void Changed();")]
    [InlineData("private Action _callback;")]
    [InlineData("private Func<int> _factory;")]
    [InlineData("var t = Task.Run(() => 1);")]
    [InlineData("var timer = new Timer(1000);")]
    [InlineData("using System.IO;")]
    [InlineData("private static int _counter;")]
    [InlineData("private IWorkspaceHostPort _port;")]
    [InlineData("private WorkspaceSession _session;")]
    public void TheBrowserGuardDetectsEachForbiddenFamilyInRealCoreText(string injected)
    {
        var forbidden = new Regex(
            @"\bevent\b|\bdelegate\b|\bAction\b|\bAction<|\bFunc<|\bPredicate<|\bEventHandler\b|\bTask\b|\bTask<|\bValueTask\b|\basync\b|\bawait\b"
            + @"|\bTimer\b|System\.Timers|\bThread\b|ThreadPool|System\.Threading|Parallel\.|System\.IO|\bFile\.|\bDirectory\.|\bStreamWriter\b|Microsoft\.Win32"
            + @"|\bConsole\b|\bDateTime\b|\bStopwatch\b|\bstatic\b|\block\b|\bvolatile\b|Environment\."
            + @"|\bIWorkspaceHostPort\b|\bWorkspaceSession\b");
        var real = Src.Code(SessionPath);

        Assert.DoesNotMatch(forbidden, real);
        Assert.Matches(forbidden, real + "\n" + injected);
    }

    // ------------------------------------------------------------------ INV-F3T1-01 and 14: shape of the types

    [Fact]
    public void TheBrowserTypesLiveInTheWorkspaceNamespaceOfApplication()
    {
        foreach (var type in BrowserTypes)
        {
            Assert.Equal("RackCad.Application.Workspace", type.Namespace);
            Assert.Equal(typeof(SessionId).Assembly, type.Assembly);
        }
    }

    [Fact]
    public void TheTabRecordHasExactlyTheFivePropertiesOfTheContractAndNoOtherState()
    {
        var properties = typeof(BrowserTabRecord).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(p => p.Name + ":" + p.PropertyType.Name)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            new[] { "IsPreview:Boolean", "Kind:String", "Name:String", "RackId:String", "Status:BrowserTabStatus" },
            properties);
        Assert.True(typeof(BrowserTabRecord).IsSealed);
        Assert.True(typeof(IEquatable<BrowserTabRecord>).IsAssignableFrom(typeof(BrowserTabRecord)));
        var fields = typeof(BrowserTabRecord).GetFields(AllDeclared).Select(f => f.FieldType).ToList();
        Assert.All(fields, t => Assert.True(t == typeof(string) || t == typeof(bool) || t == typeof(BrowserTabStatus), t.FullName));
    }

    [Fact]
    public void TheEnumsHaveExactlyTheContractedValuesInOrder()
    {
        Assert.Equal(new[] { "NotLoaded", "Ready", "Unavailable", "Error" }, Enum.GetNames<BrowserTabStatus>());
        Assert.Equal(new[] { "None", "Activated", "Created", "Replaced", "Pinned", "Closed", "SummaryApplied" }, Enum.GetNames<BrowserChange>());
    }

    [Fact]
    public void TheSessionPublicApiIsExactlyTheContractedOne()
    {
        var members = typeof(BrowserSession).GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Where(m => m is not MethodBase mb || !mb.IsSpecialName)
            .Select(m => m.MemberType + " " + m.Name)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            new[]
            {
                "Method Activate", "Method ApplySelectionContext", "Method ApplySummary", "Method Close", "Method Pin",
                "Property ActiveRackId", "Property Id", "Property IsDestroyed", "Property LiveSurfaceRackId", "Property Tabs",
                "Field MaxLiveSurfaces",
            }.OrderBy(n => n, StringComparer.Ordinal).ToArray(),
            members);

        Assert.True(typeof(BrowserSession).IsSealed);
        Assert.Empty(typeof(BrowserSession).GetConstructors(BindingFlags.Instance | BindingFlags.Public));
        Assert.Equal(1, BrowserSession.MaxLiveSurfaces);
        Assert.True(typeof(BrowserSession).GetField("MaxLiveSurfaces")!.IsLiteral);
    }

    [Fact]
    public void TheRegistryPublicApiIsExactlyTheContractedOne()
    {
        var members = typeof(BrowserSessionRegistry).GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly)
            .Where(m => m is not MethodBase mb || !mb.IsSpecialName)
            .Select(m => m.MemberType + " " + m.Name)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            new[] { "Method Destroy", "Method GetOrCreate", "Method TryGet", "Property KnownSessions", "Property LiveCount" }
                .OrderBy(n => n, StringComparer.Ordinal).ToArray(),
            members);
        Assert.True(typeof(BrowserSessionRegistry).IsSealed);
        Assert.Single(typeof(BrowserSessionRegistry).GetConstructors(BindingFlags.Instance | BindingFlags.Public));
    }

    [Fact]
    public void NoBrowserTypeHasAnEventADelegateATaskOrAHostDependencyInAnyMember()
    {
        foreach (var type in BrowserTypes)
        {
            Assert.Empty(type.GetEvents(AllDeclared));

            var used = new List<Type>();
            foreach (var f in type.GetFields(AllDeclared)) used.Add(f.FieldType);
            foreach (var p in type.GetProperties(AllDeclared)) used.Add(p.PropertyType);
            foreach (var c in type.GetConstructors(AllDeclared)) used.AddRange(c.GetParameters().Select(x => x.ParameterType));
            foreach (var m in type.GetMethods(AllDeclared))
            {
                used.Add(m.ReturnType);
                used.AddRange(m.GetParameters().Select(x => x.ParameterType));
            }

            foreach (var t in used.SelectMany(Flatten))
            {
                Assert.False(typeof(Delegate).IsAssignableFrom(t), type.Name + " uses delegate " + t);
                Assert.False(typeof(System.Threading.Tasks.Task).IsAssignableFrom(t), type.Name + " uses task " + t);
                Assert.False((t.Namespace ?? "").StartsWith("Autodesk", StringComparison.Ordinal), type.Name + " uses " + t);
                Assert.False((t.Namespace ?? "").StartsWith("System.Windows", StringComparison.Ordinal), type.Name + " uses " + t);
                Assert.False(t.Namespace == "RackCad.Application.Workspace" && t.IsInterface, type.Name + " depends on port " + t);
                Assert.False(t == typeof(WorkspaceSession) || t == typeof(WorkspaceSessionRegistry), type.Name + " uses " + t);
            }
        }
    }

    private static IEnumerable<Type> Flatten(Type t)
    {
        var current = t.IsByRef ? t.GetElementType()! : t;
        yield return current;
        if (current.IsGenericType)
            foreach (var arg in current.GetGenericArguments().SelectMany(Flatten)) yield return arg;
        if (current.IsArray)
            foreach (var arg in Flatten(current.GetElementType()!)) yield return arg;
    }

    [Fact]
    public void TheBrowserTypesCarryNoMetricsProviderSummaryQueryOrBomConceptInTheirNames()
    {
        var banned = new Regex(@"ProjectSummary|Metric|Provider|TotalRacks|Aggregat|Bom\b", RegexOptions.IgnoreCase);

        foreach (var type in BrowserTypes)
        {
            Assert.DoesNotMatch(banned, type.Name);
            foreach (var m in type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
                Assert.DoesNotMatch(banned, m.Name);
        }
    }

    // ------------------------------------------------------------------ INV-F3T1-15: the exact delta of the per-tab guard

    [Fact]
    public void TheBrowserTypesCarryNoSubscriptionEditorOrDraftEvenThoughTwoNamesMayCarryTab()
    {
        var tabNames = BrowserTypes.Select(t => t.Name).Where(n => Regex.IsMatch(n, "Tab", RegexOptions.IgnoreCase)).OrderBy(n => n, StringComparer.Ordinal).ToArray();

        Assert.Equal(new[] { "BrowserTabRecord", "BrowserTabStatus" }, tabNames);
        foreach (var type in BrowserTypes)
        {
            Assert.DoesNotMatch(new Regex("Subscription|Editor|Draft", RegexOptions.IgnoreCase), type.Name);
            foreach (var m in type.GetMembers(AllDeclared))
                Assert.DoesNotMatch(new Regex("Subscription|Editor|Draft", RegexOptions.IgnoreCase), m.Name);
        }
    }

    [Fact]
    public void TheWorkspaceNamespaceHoldsNoOtherTypeWithTabInItsNameThanTheTwoExemptOnes()
    {
        var offenders = typeof(WorkspaceSession).Assembly.GetTypes()
            .Where(t => t.Namespace == "RackCad.Application.Workspace")
            .Select(t => t.Name)
            .Where(n => Regex.IsMatch(n, "Tab", RegexOptions.IgnoreCase) && n != "BrowserTabRecord" && n != "BrowserTabStatus")
            .ToList();

        Assert.Empty(offenders);
    }

    [Fact]
    public void TheExistingModelGuardExemptsExactlyTheTwoNamesAndKeepsTheOtherFamiliesForEveryType()
    {
        var text = Src.Text("tests/RackCad.Tests/Workspace/WorkspaceModelBoundaryTests.cs");
        var body = Src.BodyOf(Src.CodeOnly(text), @"public\s+void\s+ThereIsNoPerTabStateOrPerTabSubscriptions\s*\(\s*\)");

        Assert.Contains("\"BrowserTabRecord\"", body);
        Assert.Contains("\"BrowserTabStatus\"", body);
        Assert.Equal(1, Src.Count(body, "\"BrowserTabRecord\""));
        Assert.Equal(1, Src.Count(body, "\"BrowserTabStatus\""));
        Assert.Contains("Subscription|Editor|Draft", body);
        Assert.DoesNotMatch(new Regex(@"StartsWith|EndsWith|Contains\(\s*""Browser|Wildcard|Regex\.IsMatch\([^)]*Browser"), body);
        // The check over the properties of WorkspaceSession is unchanged.
        Assert.Contains("typeof(WorkspaceSession).GetProperties()", body);
        Assert.Contains("Tab|Subscription|Draft|Handler", body);
    }
}
