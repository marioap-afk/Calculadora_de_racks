#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using RackCad.Application.Workspace;
using RackCad.Application.Workspace.Drafts;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F4-T1-DRAFT-CORE: INV-F4T1-15, 16 and 17 as source and shape guards over the pure core of the drafts.
// WorkspaceModelBoundaryTests keeps guarding the namespace RackCad.Application.Workspace; the checks that look only at the
// EXACT namespace (type names, public members) do not see the subnamespace, so this class applies the same rules to it.
public class WorkspaceDraftBoundaryTests
{
    private const string DraftsFolder = "src/RackCad.Application/Workspace/Drafts/";
    private const string DraftsNamespace = "RackCad.Application.Workspace.Drafts";

    private static readonly string[] ExpectedFiles =
    {
        DraftsFolder + "DraftApplyOutcome.cs",
        DraftsFolder + "DraftClosePolicy.cs",
        DraftsFolder + "DraftRegistry.cs",
        DraftsFolder + "DraftSession.cs",
        DraftsFolder + "DraftStaleBase.cs",
    };

    private static IReadOnlyList<(string Path, string Code)> DraftFiles() => Src.Files(DraftsFolder);

    private static IReadOnlyList<Type> DraftTypes() =>
        typeof(DraftRegistry).Assembly.GetTypes()
            .Where(t => t.Namespace == DraftsNamespace && !t.IsDefined(typeof(CompilerGeneratedAttribute), false))
            .ToList();

    private const BindingFlags AllMembers = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;

    // ------------------------------------------------------------------ the folder and its files

    [Fact]
    public void TheDraftsFolderHoldsExactlyTheFiveCoreFilesAndTheParentGuardsRecurseIntoIt()
    {
        Assert.Equal(ExpectedFiles, DraftFiles().Select(f => f.Path).ToArray());

        // The parent guards walk src/RackCad.Application/Workspace/ recursively, so they apply to these files too.
        var parent = Src.ApplicationFiles().Select(f => f.Path).ToHashSet(StringComparer.Ordinal);
        Assert.All(ExpectedFiles, f => Assert.Contains(f, parent));
    }

    [Fact]
    public void EveryFileStartsWithNullableEnableAndDeclaresOneBlockNamespaceEqualToItsFolder()
    {
        foreach (var path in ExpectedFiles)
        {
            var text = Src.Text(path);
            Assert.StartsWith("#nullable enable", text, StringComparison.Ordinal);
            var declarations = Regex.Matches(Src.CodeOnly(text), @"^\s*namespace\s+([\w\.]+)\s*(\{|;)", RegexOptions.Multiline);
            Assert.Single(declarations);
            Assert.Equal(DraftsNamespace, declarations[0].Groups[1].Value);
            Assert.Equal("{", declarations[0].Groups[2].Value);
        }
    }

    [Fact]
    public void EveryPublicTypeOfTheCoreLivesInTheDraftsNamespaceOfApplication()
    {
        var types = DraftTypes();

        Assert.Contains(types, t => t == typeof(DraftSession<>));
        Assert.Contains(types, t => t == typeof(DraftRegistry));
        Assert.Contains(types, t => t == typeof(DraftClosePolicy));
        Assert.Contains(types, t => t == typeof(DraftStaleBase));
        Assert.Contains(types, t => t == typeof(DraftApplyOutcome<>));
        Assert.All(types, t => Assert.Equal(typeof(DraftRegistry).Assembly, t.Assembly));
    }

    // ------------------------------------------------------------------ no AutoCAD, WPF, Plugin or UI

    private static readonly Regex HostReferences = new(@"\bAutodesk\.|System\.Windows|RackCad\.Plugin|RackCad\.UI|Autodesk\.AutoCAD\.Internal");

    [Fact]
    public void NoFileOfTheCoreReferencesAutoCadWpfThePluginOrTheUi()
    {
        foreach (var (path, code) in DraftFiles())
            Assert.True(!HostReferences.IsMatch(code), path + " uses " + HostReferences.Match(code).Value);
    }

    [Theory]
    [InlineData("using Autodesk.AutoCAD.DatabaseServices;")]
    [InlineData("var w = new System.Windows.Window();")]
    [InlineData("using RackCad.Plugin.Workspace;")]
    [InlineData("using RackCad.UI.Controls;")]
    [InlineData("Autodesk.AutoCAD.Internal.Utils.SetFocusToDwgView();")]
    public void TheHostReferenceGuardDetectsEachFamily(string injected)
    {
        var real = DraftFiles()[0].Code;
        Assert.DoesNotMatch(HostReferences, real);
        Assert.Matches(HostReferences, real + "\n" + injected);
    }

    // ------------------------------------------------------------------ no persistence, no authored writes

    // The same families as WorkspaceModelBoundaryTests.ModelDoesNotPersistNorWriteAuthoredData plus the registry of settings.
    private static readonly Regex PersistenceOrWriteAuthorities = new(
        @"System\.IO|\bFile\.|\bDirectory\.|\bStreamWriter\b|Microsoft\.Win32|\bRegistry\.|\bUserSettings\b|\bAtomicFile\b|Transaction|LockDocument|\bDatabase\b|\bCommit\(|\bAutodesk\.|RackLog|Environment\.");

    [Fact]
    public void NoFileOfTheCorePersistsOrWritesAuthoredData()
    {
        foreach (var (path, code) in DraftFiles())
            Assert.True(!PersistenceOrWriteAuthorities.IsMatch(code), path + " uses " + PersistenceOrWriteAuthorities.Match(code).Value);
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
    [InlineData("RackLog.Exception(ex);")]
    [InlineData("Environment.SetEnvironmentVariable(a, b);")]
    public void ThePersistenceGuardDetectsEachForbiddenFamilyInRealCoreText(string injected)
    {
        var real = DraftFiles().First(f => f.Path.EndsWith("DraftSession.cs", StringComparison.Ordinal)).Code;
        Assert.DoesNotMatch(PersistenceOrWriteAuthorities, real);
        Assert.Matches(PersistenceOrWriteAuthorities, real + "\n" + injected);
    }

    [Fact]
    public void ThePersistenceGuardDoesNotRejectTheInMemoryRegistryOfDrafts()
    {
        var registry = DraftFiles().First(f => f.Path.EndsWith("DraftRegistry.cs", StringComparison.Ordinal)).Code;

        Assert.Contains("DraftRegistry", registry);
        Assert.DoesNotMatch(PersistenceOrWriteAuthorities, registry);
    }

    // ------------------------------------------------------------------ nothing of I-63

    private static readonly Regex I63Concepts = new(
        @"ComputedParameter|Metric|ProjectSummary|TotalRacks|Aggregat|Provider|\bBom", RegexOptions.IgnoreCase);

    [Fact]
    public void NoFileOfTheCoreCarriesAMetricsProviderProjectSummaryOrAggregationConcept()
    {
        foreach (var (path, code) in DraftFiles())
            Assert.True(!I63Concepts.IsMatch(code), path + " mentions " + I63Concepts.Match(code).Value);
    }

    [Theory]
    [InlineData("var x = ComputedParameters.Evaluate();")]
    [InlineData("class DraftMetricsHolder {}")]
    [InlineData("var s = new ProjectSummary();")]
    [InlineData("int TotalRacks;")]
    [InlineData("Aggregate(items);")]
    [InlineData("BomBuilder.Build();")]
    [InlineData("class DraftStateProvider {}")]
    public void TheI63GuardDetectsEachConceptInRealCoreText(string injected)
    {
        var real = DraftFiles()[0].Code;
        Assert.DoesNotMatch(I63Concepts, real);
        Assert.Matches(I63Concepts, real + "\n" + injected);
    }

    [Fact]
    public void TheCoreDefinesNoStateOfAnySystemAndNamesNoEditorOfOne()
    {
        var systems = new Regex(@"PushBack|Selectiv|Cantilever|Cabecera|Dynamic|FlowBed|Cama|Tarima|Fondo|Header", RegexOptions.IgnoreCase);
        foreach (var (path, code) in DraftFiles())
            Assert.True(!systems.IsMatch(code), path + " mentions " + systems.Match(code).Value);

        var session = typeof(DraftSession<>);
        Assert.True(session.IsGenericTypeDefinition);
        Assert.Empty(session.GetGenericArguments()[0].GetGenericParameterConstraints());
    }

    // ------------------------------------------------------------------ names (the subnamespace equivalent of WorkspaceModelBoundaryTests)

    private static readonly Regex ForbiddenTypeNames = new("Subscription|Editor|Tab|Handler", RegexOptions.IgnoreCase);
    private static readonly Regex ForbiddenMemberNames = new("Subscription|Handler|ProjectSummary|Metric|Provider|TotalRacks|Aggregat|Bom\\b", RegexOptions.IgnoreCase);

    [Fact]
    public void NoTypeOfTheCoreIsCalledSubscriptionEditorTabOrHandler()
    {
        Assert.NotEmpty(DraftTypes());
        foreach (var type in DraftTypes())
            Assert.True(!ForbiddenTypeNames.IsMatch(type.Name), type.FullName + " matches " + ForbiddenTypeNames.Match(type.Name).Value);
    }

    [Theory]
    [InlineData("DraftSubscription")]
    [InlineData("DraftEditorState")]
    [InlineData("DraftTabState")]
    [InlineData("DraftHandler")]
    [InlineData("DraftStableState")]
    [InlineData("DraftMutableSet")]
    [InlineData("DraftTable")]
    public void TheTypeNameGuardDetectsEachForbiddenFamilyBySubstringAndIgnoringCase(string injected)
    {
        Assert.Matches(ForbiddenTypeNames, injected);
        Assert.Matches(ForbiddenTypeNames, injected.ToLowerInvariant());
        Assert.All(DraftTypes(), t => Assert.DoesNotMatch(ForbiddenTypeNames, t.Name));
    }

    [Fact]
    public void NoMemberOfTheCoreIsAMetricAProviderASummaryASubscriptionOrAHandler()
    {
        foreach (var type in DraftTypes())
        {
            Assert.DoesNotMatch(ForbiddenMemberNames, type.Name);
            foreach (var member in type.GetMembers(AllMembers))
                Assert.True(!ForbiddenMemberNames.IsMatch(member.Name), type.Name + "." + member.Name);
        }
    }

    [Theory]
    [InlineData("MetricsSnapshot")]
    [InlineData("TotalRacks")]
    [InlineData("ProjectSummary")]
    [InlineData("AggregateAll")]
    [InlineData("DraftBom")]
    [InlineData("StateProvider")]
    [InlineData("Subscribe_Subscription")]
    [InlineData("OnChangedHandler")]
    public void TheMemberNameGuardDetectsEachForbiddenFamily(string injected)
    {
        Assert.Matches(ForbiddenMemberNames, injected);
    }

    // ------------------------------------------------------------------ pureza: no tasks, events, threads or static mutable state

    private static readonly Regex BackgroundWork = new(
        @"\basync\b|\bawait\b|\bTask\b|Task\.|\bThread\b|ThreadPool|\bTimer\b|System\.Timers|\bevent\b|\bDispatcher\b|SynchronizationContext|Parallel\.|BackgroundWorker|\bRegen\b|ConfigureAwait|GetAwaiter|\bLazy<");

    private static readonly Regex MutableStaticField = new(@"\bstatic\s+(?!readonly\b)(?!class\b)(?!extern\b)[\w<>,\.\[\]\? ]+?\s+\w+\s*(?:=(?!>)|;)");

    [Fact]
    public void NoFileOfTheCoreUsesAsyncTasksThreadsTimersEventsOrMutableStaticFields()
    {
        foreach (var (path, code) in DraftFiles())
        {
            Assert.True(!BackgroundWork.IsMatch(code), path + " uses " + BackgroundWork.Match(code).Value);
            Assert.True(!MutableStaticField.IsMatch(code), path + " has a mutable static field: " + MutableStaticField.Match(code).Value);
        }
    }

    [Theory]
    [InlineData("public async System.Threading.Tasks.Task Run() {}", "async")]
    [InlineData("var t = Task.Run(() => 1);", "Task")]
    [InlineData("new Thread(() => {});", "Thread")]
    [InlineData("private Timer _t;", "Timer")]
    [InlineData("public event Action Changed;", "event")]
    [InlineData("Dispatcher.Invoke(a);", "Dispatcher")]
    [InlineData("doc.Editor.Regen();", "Regen")]
    public void TheBackgroundWorkGuardDetectsEachFamilyInRealCoreText(string injected, string expected)
    {
        var real = DraftFiles()[0].Code;
        Assert.DoesNotMatch(BackgroundWork, real);
        Assert.Contains(expected, BackgroundWork.Match(real + "\n" + injected).Value);
    }

    [Theory]
    [InlineData("private static int _counter;", true)]
    [InlineData("internal static List<string> Log = new();", true)]
    [InlineData("static string _name = \"x\";", true)]
    [InlineData("private static readonly int One = 1;", false)]
    [InlineData("public static DraftAdmission Admitted => new(true, null);", false)]
    [InlineData("public static bool IsBlocking(DraftBoundary boundary) => true;", false)]
    [InlineData("public static class DraftBoundaries", false)]
    public void TheStaticStateGuardDetectsOnlyMutableStaticFields(string injected, bool mutable)
    {
        Assert.Equal(mutable, MutableStaticField.IsMatch(injected));
    }

    [Fact]
    public void TheCoreHasNoEventsAndItsStaticStateIsImmutable()
    {
        foreach (var type in DraftTypes())
        {
            Assert.Empty(type.GetEvents(AllMembers));
            foreach (var field in type.GetFields(AllMembers).Where(f => f.IsStatic))
                Assert.True(field.IsInitOnly || field.IsLiteral, type.Name + "." + field.Name);
            foreach (var property in type.GetProperties(AllMembers).Where(p => p.GetAccessors(true).Any(m => m.IsStatic)))
                Assert.True(!property.CanWrite, type.Name + "." + property.Name);
        }
    }

    // ------------------------------------------------------------------ only SessionId of the parent namespace

    [Fact]
    public void TheCoreUsesNoTypeOfTheParentNamespaceExceptSessionId()
    {
        var parentTypes = typeof(SessionId).Assembly.GetTypes()
            .Where(t => t.Namespace == "RackCad.Application.Workspace" && !t.IsDefined(typeof(CompilerGeneratedAttribute), false))
            .Select(t => Regex.Replace(t.Name, @"`\d+$", string.Empty))
            .Where(n => n != nameof(SessionId))
            .Distinct()
            .ToList();
        Assert.Contains("WorkspaceSession", parentTypes);

        foreach (var (path, code) in DraftFiles())
        {
            foreach (var name in parentTypes)
                Assert.True(!Regex.IsMatch(code, @"\b" + Regex.Escape(name) + @"\b"), path + " uses " + name);

            var qualified = Regex.Matches(code, @"\bRackCad\.[\w\.]+").Select(m => m.Value).ToList();
            Assert.Equal(new[] { DraftsNamespace }, qualified.ToArray());
        }

        Assert.Contains(DraftFiles(), f => Regex.IsMatch(f.Code, @"\bSessionId\b"));
    }

    [Fact]
    public void EveryTypeInTheSignaturesOfTheCoreIsFromSystemFromTheCoreItselfOrIsSessionId()
    {
        static IEnumerable<Type> Flatten(Type type)
        {
            if (type.IsByRef || type.IsArray || type.IsPointer) type = type.GetElementType()!;
            if (type.IsGenericParameter) yield break;
            yield return type;
            if (type.IsGenericType)
                foreach (var argument in type.GetGenericArguments().SelectMany(Flatten)) yield return argument;
        }

        var referenced = new HashSet<Type>();
        foreach (var type in DraftTypes())
        {
            foreach (var f in type.GetFields(AllMembers)) foreach (var t in Flatten(f.FieldType)) referenced.Add(t);
            foreach (var p in type.GetProperties(AllMembers)) foreach (var t in Flatten(p.PropertyType)) referenced.Add(t);
            foreach (var m in type.GetMethods(AllMembers))
            {
                foreach (var t in Flatten(m.ReturnType)) referenced.Add(t);
                foreach (var parameter in m.GetParameters()) foreach (var t in Flatten(parameter.ParameterType)) referenced.Add(t);
            }

            foreach (var c in type.GetConstructors(AllMembers))
                foreach (var parameter in c.GetParameters()) foreach (var t in Flatten(parameter.ParameterType)) referenced.Add(t);
        }

        Assert.Contains(typeof(SessionId), referenced);
        foreach (var type in referenced)
        {
            var ns = type.Namespace ?? string.Empty;
            var allowed = ns == DraftsNamespace || ns == "System" || ns == "System.Text" || ns == "System.Collections.Generic" || ns == "System.Collections.ObjectModel" || type == typeof(SessionId);
            Assert.True(allowed, type.FullName);
        }
    }

    // ------------------------------------------------------------------ every operation returns a value (INV-F4T1-15)

    [Fact]
    public void EveryPublicOperationOfTheCoreReturnsAValueThatSaysWhatChanged()
    {
        var operations = new[] { typeof(DraftSession<>), typeof(DraftRegistry), typeof(DraftClosePolicy), typeof(DraftStaleBase), typeof(DraftApplyOutcome<>), typeof(DraftIntentRead<>) }
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
            .ToList();

        Assert.True(operations.Count > 30);
        Assert.All(operations, m => Assert.True(m.ReturnType != typeof(void), m.DeclaringType!.Name + "." + m.Name));
        Assert.Contains(operations, m => m.Name == "TryBeginApply");
        Assert.Contains(operations, m => m.Name == "CrossBoundary");
        Assert.Contains(operations, m => m.Name == "RetireSession");
    }

    // ------------------------------------------------------------------ the product scope of the contract (INV-F4T1-17)

    [Fact]
    public void TheCoreFilesAreTheOnlyDraftFilesAndNoExistingWorkspaceFileMentionsThem()
    {
        var elsewhere = Src.AllProduct()
            .Where(f => !f.Path.StartsWith(DraftsFolder, StringComparison.Ordinal))
            .Where(f => Regex.IsMatch(f.Code, @"RackCad\.Application\.Workspace\.Drafts|\bDraftRegistry\b|\bDraftSession\b|\bDraftClosePolicy\b|\bDraftStaleBase\b"))
            .Select(f => f.Path)
            .ToArray();

        Assert.Empty(elsewhere);
    }
}
