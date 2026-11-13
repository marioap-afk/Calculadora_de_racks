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

// I-64 F4-T3A-EDIT-PORT (INV-F4T3A-01 to 03, 06, 07, 10, 12 and 14): the source guards of the edit port of the pilot. The Core test project
// cannot reference the Plugin (it needs AutoCAD), so the Plugin files are read as text, like the rest of the guards of the workspace. By the
// rule of I-1 (F4-E32, F4-E33) every guard here is limited to the bodies and the names that THIS task owns, the enums are ordered prefixes
// and the sets are lower bounds: none of them prohibits for ever what belongs to F4-T3B, F4-T3C or F4-T4 (an operation of Update, a new kind
// of boundary, a wrapper of the advisor, a dialog). Each guard is a function of the text, with its self-tests: the real source passes, the
// text that must trip it trips it, and the text that T4 would add does not.
public class WorkspacePilotEditGuardTests
{
    private const string PortPath = "src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs";
    private const string SupportPath = "src/RackCad.Application/Workspace/WorkspacePilotSupport.cs";
    private const string HostPath = "src/RackCad.Application/Workspace/WorkspaceHostCore.cs";
    private const string IntentPath = "src/RackCad.Application/Workspace/WorkspaceEditIntent.cs";
    private const string ControllerPath = "src/RackCad.Plugin/Workspace/WorkspaceController.cs";
    private const string BridgePath = "src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs";
    private const string DraftsFolder = "src/RackCad.Application/Workspace/Drafts/";
    private const string BrowserSessionPath = "src/RackCad.Application/Workspace/BrowserSession.cs";

    private static string Port() => Src.Code(PortPath);

    private static string Support() => Src.Code(SupportPath);

    private static string Host() => Src.Code(HostPath);

    private static string Controller() => Src.Code(ControllerPath);

    private static string Squash(string text) => Regex.Replace(text, @"\s+", string.Empty);

    private static string? TryBody(string code, string signature) => Regex.IsMatch(code, signature) ? Src.BodyOf(code, signature) : null;

    // The twelve operations of the table of D-T3-02: the name, the signature in the interface and the signature of the explicit implementation.
    private static readonly (string Name, string Interface)[] Operations =
    {
        ("TryBeginEdit", @"EditIntentOutcome\s+TryBeginEdit\s*\(\s*EditIntentRequest\s+\w+\s*\)\s*;"),
        ("TryCheckNow", @"EditCheckOutcome\s+TryCheckNow\s*\(\s*EditIntentRequest\s+\w+\s*\)\s*;"),
        ("TryReload", @"EditIntentOutcome\s+TryReload\s*\(\s*EditIntentRequest\s+\w+\s*\)\s*;"),
        ("GetField", @"PilotFieldView\s+GetField\s*\(\s*WorkspaceRequest\s+\w+\s*,\s*string\s+\w+\s*\)\s*;"),
        ("SetPendingText", @"PilotFieldResult\s+SetPendingText\s*\(\s*WorkspaceRequest\s+\w+\s*,\s*string\s+\w+\s*,\s*string\s+\w+\s*\)\s*;"),
        ("CommitField", @"PilotFieldResult\s+CommitField\s*\(\s*WorkspaceRequest\s+\w+\s*,\s*string\s+\w+\s*\)\s*;"),
        ("CrossBoundary", @"PilotBoundaryResult\s+CrossBoundary\s*\(\s*WorkspaceRequest\s+\w+\s*,\s*string\s+\w+\s*,\s*PilotBoundaryKind\s+\w+\s*\)\s*;"),
        ("DiscardRack", @"EditDiscardOutcome\s+DiscardRack\s*\(\s*WorkspaceRequest\s+\w+\s*,\s*string\s+\w+\s*\)\s*;"),
        ("DiscardDocument", @"EditDiscardOutcome\s+DiscardDocument\s*\(\s*WorkspaceRequest\s+\w+\s*\)\s*;"),
        ("GetRacksAtRisk", @"PilotRiskView\s+GetRacksAtRisk\s*\(\s*WorkspaceRequest\s+\w+\s*\)\s*;"),
        ("TakeLossNotices", @"IReadOnlyList<PilotLossView>\s+TakeLossNotices\s*\(\s*\)\s*;"),
        ("TakeCloseNotices", @"IReadOnlyList<PilotCloseView>\s+TakeCloseNotices\s*\(\s*\)\s*;"),
    };

    private static readonly string[] ReadOperations = { "TryBeginEdit", "TryCheckNow", "TryReload" };

    private static readonly string[] MemoryOperations = { "GetField", "GetRacksAtRisk", "TakeLossNotices", "TakeCloseNotices" };

    private static readonly string[] MutatingOperations = { "SetPendingText", "CommitField", "CrossBoundary", "DiscardRack", "DiscardDocument" };

    private static string HostSignature(string name) => @"\w[\w<>,\s]*?\s+IWorkspacePilotEditPort\." + name + @"\s*\(";

    private const string WriteApis =
        @"OpenMode\.(?:ForWrite|ForNotify)|UpgradeOpen|DowngradeOpen|AppendEntity|\.Erase\s*\(|\.Commit\s*\(|RackBlockData\.Write|SetImpliedSelection|SetCurrentView|"
        + @"StartTransaction|StartOpenCloseTransaction|LockDocument|SetSystemVariable|SendStringToExecute|\bSaveAs\b|\bSave\s*\(";

    // ------------------------------------------------------------------ INV-F4T3A-01: the port, its values and its enums

    private static readonly Regex DraftOrPilotNames = new(@"\bDraft\w*|\bPushBack\w*|RackCad\.Application\.Systems|Workspace\.Drafts");

    private static readonly (string Enum, string[] Prefix)[] OrderedEnums =
    {
        ("PilotFieldState", new[] { "NotOpen", "Clean", "Dirty", "Conflict", "Applying", "Unknown", "Error", "Undecidable" }),
        ("PilotBoundaryDecision", new[] { "Proceeded", "Aborted", "Kept", "Busy" }),
        ("PilotBoundaryKind", new[] { "TabChange", "RackChange", "DocumentChange", "PanelHide", "Dematerialize", "DocumentClose" }),
    };

    private static IReadOnlyList<string>? EnumMembers(string code, string name)
    {
        var body = TryBody(code, @"public\s+enum\s+" + name + @"\b");
        if (body == null) return null;
        var inner = body.Substring(body.IndexOf('{') + 1).TrimEnd('}');
        return inner.Split(',').Select(m => m.Trim()).Where(m => m.Length > 0).ToArray();
    }

    private static IEnumerable<string> PortViolations(string port)
    {
        if (!Regex.IsMatch(port, @"\bpublic\s+interface\s+IWorkspacePilotEditPort\b")) yield return "the interface is missing";
        if (Src.Count(port, @"\bnamespace\s+RackCad\.Application\.Workspace\s*\{") != 1) yield return "the namespace is not the one of the workspace";

        // The twelve operations are a lower bound: a later task may add its own to this port.
        foreach (var (name, signature) in Operations)
            if (Src.Count(port, signature) != 1) yield return "the port lacks " + name;

        foreach (Match match in DraftOrPilotNames.Matches(port)) yield return "the port names " + match.Value;

        foreach (var (name, prefix) in OrderedEnums)
        {
            var members = EnumMembers(port, name);
            if (members == null) yield return "the enum " + name + " is missing";
            else if (!members.Take(prefix.Length).SequenceEqual(prefix)) yield return "the enum " + name + " lost its ordered prefix";
        }

        var status = EnumMembers(port, "PilotFieldStatus");
        if (status == null) yield return "the enum PilotFieldStatus is missing";
        else
        {
            foreach (var name in new[] { "Applied", "Rejected", "NotEditable", "Busy", "Invalid" })
                if (!status.Contains(name)) yield return "PilotFieldStatus lacks " + name;
            if (status.Count == 0 || status[0] != "Rejected") yield return "the default of PilotFieldStatus is not the inert Rejected";
        }
    }

    [Fact]
    public void ThePortDeclaresTheTwelveOperationsWithTheEnumsAsOrderedPrefixesAndNamesNoTypeOfTheDraftsNorOfThePilot()
    {
        Assert.Empty(PortViolations(Port()));
        Assert.StartsWith("#nullable enable", Src.Text(PortPath).TrimStart((char)0xFEFF));
    }

    [Theory]
    [InlineData("PilotFieldView GetField(WorkspaceRequest origin, string rackId);", "")]
    [InlineData("IReadOnlyList<PilotCloseView> TakeCloseNotices();", "")]
    [InlineData("PilotFieldView GetField(WorkspaceRequest origin, string rackId);", "PilotFieldView GetField(WorkspaceRequest origin, int rackId);")]
    [InlineData("TabChange,", "RackChange,")]
    [InlineData("Proceeded,", "Aborted,")]
    [InlineData("Undecidable,", "Undecidable, Draft,")]
    [InlineData("EditDiscardOutcome DiscardDocument(WorkspaceRequest origin);", "EditDiscardOutcome DiscardDocument(WorkspaceRequest origin); DraftSession<int> Leak();")]
    [InlineData("EditDiscardOutcome DiscardDocument(WorkspaceRequest origin);", "EditDiscardOutcome DiscardDocument(WorkspaceRequest origin); PushBackPilotDraftState Leak();")]
    public void ThePortGuardTripsOnAMissingOperationAWrongSignatureAHurtPrefixOrATypeOfTheDraftsOrThePilot(string from, string to)
    {
        var port = Port();
        Assert.Contains(from, port);

        Assert.NotEmpty(PortViolations(port.Replace(from, to, StringComparison.Ordinal)));
    }

    [Fact]
    public void ThePortGuardAllowsWhatTheNextTaskWouldAddAnUpdateAtTheEndOfTheKindsAndItsOperation()
    {
        var port = Port()
            .Replace("DocumentClose,", "DocumentClose,\n        Update,", StringComparison.Ordinal)
            .Replace("IReadOnlyList<PilotCloseView> TakeCloseNotices();", "IReadOnlyList<PilotCloseView> TakeCloseNotices();\n        PilotFieldResult Update(WorkspaceRequest origin, string rackId);", StringComparison.Ordinal);

        Assert.Contains("Update,", port);
        Assert.Empty(PortViolations(port));
    }

    private static readonly Type[] PortValueTypes =
    {
        typeof(PilotFieldView), typeof(PilotFieldResult), typeof(PilotBoundaryResult), typeof(PilotRiskView),
        typeof(PilotLossView), typeof(PilotCloseView), typeof(PilotFieldProblem),
    };

    private static IEnumerable<Type> Flatten(Type type)
    {
        yield return type;
        if (type.IsGenericType)
            foreach (var argument in type.GetGenericArguments())
                foreach (var inner in Flatten(argument))
                    yield return inner;
        if (type.HasElementType)
            foreach (var inner in Flatten(type.GetElementType()!))
                yield return inner;
    }

    [Fact]
    public void NoTypeOfTheDraftsNorOfThePilotCrossesThePortAndTheValuesAreImmutableWithoutNestedTypes()
    {
        var port = typeof(IWorkspacePilotEditPort);
        var types = new List<Type>();
        foreach (var method in port.GetMethods())
        {
            types.AddRange(Flatten(method.ReturnType));
            foreach (var parameter in method.GetParameters()) types.AddRange(Flatten(parameter.ParameterType));
        }

        foreach (var value in PortValueTypes)
        {
            foreach (var property in value.GetProperties())
                types.AddRange(Flatten(property.PropertyType));
            foreach (var constructor in value.GetConstructors())
                foreach (var parameter in constructor.GetParameters())
                    types.AddRange(Flatten(parameter.ParameterType));
        }

        Assert.NotEmpty(types);
        foreach (var type in types)
        {
            var ns = type.Namespace ?? string.Empty;
            Assert.False(ns.StartsWith("RackCad.Application.Workspace.Drafts", StringComparison.Ordinal), type.FullName);
            Assert.False(ns.StartsWith("RackCad.Application.Systems", StringComparison.Ordinal), type.FullName);
        }

        // The racks are strings, the session is the one of the workspace.
        Assert.Equal(typeof(string), typeof(PilotRiskView).GetProperty(nameof(PilotRiskView.RackIds))!.PropertyType.GetGenericArguments().Single());
        Assert.Equal(typeof(string), typeof(PilotCloseView).GetProperty(nameof(PilotCloseView.RackIds))!.PropertyType.GetGenericArguments().Single());

        Assert.Empty(port.GetNestedTypes());
        foreach (var value in PortValueTypes)
        {
            Assert.True(value.IsValueType, value.Name);
            Assert.Empty(value.GetNestedTypes(BindingFlags.Public | BindingFlags.NonPublic));
            Assert.All(value.GetProperties(), p => Assert.True(p.SetMethod == null || p.SetMethod.ReturnParameter.GetRequiredCustomModifiers().Length > 0, value.Name + "." + p.Name + " is not read-only"));
            Assert.All(value.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic), f => Assert.True(f.IsInitOnly, value.Name + "." + f.Name + " is mutable"));
        }

        foreach (var type in new[] { typeof(PilotFieldState), typeof(PilotFieldStatus), typeof(PilotBoundaryDecision), typeof(PilotBoundaryKind) })
        {
            Assert.True(type.IsEnum);
            Assert.Equal("RackCad.Application.Workspace", type.Namespace);
        }
    }

    // ------------------------------------------------------------------ INV-F4T3A-01 to 03: the host core

    private static IEnumerable<string> HostViolations(string host)
    {
        // The interface of the advisor stays the first base (WorkspaceLifecycleGuardTests), and the port is the second.
        if (!Regex.IsMatch(host, @"public\s+sealed\s+class\s+WorkspaceHostCore\s*:\s*IWorkspaceLifecycleAdvisor\s*,\s*IWorkspacePilotEditPort\b"))
            yield return "the host is not : IWorkspaceLifecycleAdvisor, IWorkspacePilotEditPort";

        // Explicit implementations only: the public surface of the host does not grow.
        foreach (var (name, _) in Operations)
        {
            if (Src.Count(host, HostSignature(name)) != 1) yield return "the host does not implement " + name + " explicitly exactly once";
            if (Regex.IsMatch(host, @"\bpublic\s+[\w<>,\s?]+\s+" + name + @"\s*\(")) yield return "the host exposes " + name + " publicly";
        }

        // The adapter of the pilot is static, never a field of instance (H-10).
        if (Regex.IsMatch(host, @"private\s+(?:readonly\s+)?PushBackPilotFieldAdapter\s+_\w+")) yield return "the adapter of the pilot is a field of instance";
        if (Src.Count(host, @"private\s+static\s+readonly\s+PushBackPilotFieldAdapter\s+\w+") != 1) yield return "the adapter of the pilot is not the one static field";

        // Only the three reads read the drawing (C03): the other nine operations never name the reader, the probe or the gate of reading.
        foreach (var (name, _) in Operations.Where(o => !ReadOperations.Contains(o.Name)))
        {
            var body = TryBody(host, HostSignature(name));
            if (body == null) continue;
            if (Regex.IsMatch(body, @"_editIntentReader|ReadIntent|_reader\b|PassGate|_probe\b|_bridge\b")) yield return name + " reaches a reader, the probe or the bridge";
        }

        // Memory only: no authorization at all.
        foreach (var name in MemoryOperations)
        {
            var body = TryBody(host, HostSignature(name));
            if (body != null && Regex.IsMatch(body, @"\bAuthorize\b|_pilotReadInProgress")) yield return name + " authorizes or looks at the mark of the reads";
        }

        // The operations that change something: authorization, then the mark of the reads, then the effect.
        foreach (var name in MutatingOperations)
        {
            var body = TryBody(host, HostSignature(name));
            if (body == null) continue;
            if (!Regex.IsMatch(body, @"\bAuthorize\s*\(")) yield return name + " does not authorize";
            if (!Regex.IsMatch(body, @"if\s*\(\s*_pilotReadInProgress\s*\)")) yield return name + " does not look at the mark of the reads";
        }

        // NotePilotEdit (or the discard of the session) in every operation that changes a draft (D-T3-02).
        foreach (var name in new[] { "SetPendingText", "CommitField", "CrossBoundary", "DiscardRack" })
        {
            var body = TryBody(host, HostSignature(name));
            if (body != null && !Regex.IsMatch(body, @"\bNotePilotEdit\s*\(")) yield return name + " does not call NotePilotEdit";
        }

        var document = TryBody(host, HostSignature("DiscardDocument"));
        if (document != null && !Regex.IsMatch(document, @"\bDiscardPilotEdits\s*\(")) yield return "DiscardDocument does not delegate in DiscardPilotEdits";

        // A non-blocking boundary needs no authorization: it answers Kept before the one authorization of the blocking ones.
        var cross = TryBody(host, HostSignature("CrossBoundary"));
        if (cross != null)
        {
            var kept = cross.IndexOf("PilotBoundaryDecision.Kept", StringComparison.Ordinal);
            var authorize = cross.IndexOf("Authorize(", StringComparison.Ordinal);
            if (kept < 0 || authorize < 0 || kept > authorize) yield return "CrossBoundary does not keep the non-blocking boundaries before it authorizes";
        }

        // The ways the host contains an exception: a value with a reason, never the degradation of the session.
        foreach (var name in new[] { "SetPendingText", "CommitField", "CrossBoundary", "DiscardRack", "DiscardDocument", "GetField", "GetRacksAtRisk" })
        {
            var body = TryBody(host, HostSignature(name));
            if (body == null) continue;
            if (!Regex.IsMatch(body, @"catch\s*\(\s*Exception")) yield return name + " does not contain its exception";
            if (Regex.IsMatch(body, @"_faulted|\bContain\s*\(|\bthrow\s*;")) yield return name + " degrades the session or rethrows";
        }
    }

    [Fact]
    public void TheHostImplementsThePortExplicitlyAndNeverReadsOutsideTheThreeReadsAndChangesOnlyThroughNotePilotEdit()
    {
        Assert.Empty(HostViolations(Host()));
        Assert.True(typeof(IWorkspacePilotEditPort).IsAssignableFrom(typeof(WorkspaceHostCore)));
        foreach (var (name, _) in Operations) Assert.Null(typeof(WorkspaceHostCore).GetMethod(name));
        Assert.Equal(new[] { "IWorkspaceLifecycleAdvisor", "IWorkspacePilotEditPort" }, typeof(WorkspaceHostCore).GetInterfaces().Select(i => i.Name).OrderBy(n => n, StringComparer.Ordinal).ToArray());
    }

    [Theory]
    [InlineData("public sealed class WorkspaceHostCore : IWorkspaceLifecycleAdvisor, IWorkspacePilotEditPort", "public sealed class WorkspaceHostCore : IWorkspacePilotEditPort, IWorkspaceLifecycleAdvisor")]
    [InlineData("PilotFieldView IWorkspacePilotEditPort.GetField(", "public PilotFieldView GetField(")]
    [InlineData("PilotFieldResult IWorkspacePilotEditPort.CommitField(", "public PilotFieldResult CommitField(")]
    [InlineData("private static readonly PushBackPilotFieldAdapter PilotFieldAdapter = new();", "private readonly PushBackPilotFieldAdapter _adapter = new();")]
    public void TheHostGuardTripsOnAPublicOperationAnOrderOfBasesOrAnAdapterThatIsAFieldOfInstance(string from, string to)
    {
        var host = Host();
        Assert.Contains(from, host);

        Assert.NotEmpty(HostViolations(host.Replace(from, to, StringComparison.Ordinal)));
    }

    [Theory]
    [InlineData("TakeLossNotices", "var read = _editIntentReader!.ReadIntent(default, \"x\", null, null);")]
    [InlineData("TakeCloseNotices", "_bridge.RequestReconciliation();")]
    [InlineData("GetField", "var gate = Authorize(origin);")]
    [InlineData("GetRacksAtRisk", "if (_pilotReadInProgress) return default;")]
    [InlineData("DiscardRack", "var read = _reader.Read(origin.Origin, 1);")]
    public void TheHostGuardTripsOnAReadAnAuthorizationOrAMarkWhereTheOperationMustNotHaveThem(string operation, string injected)
    {
        var host = Host();
        var body = Src.BodyOf(host, HostSignature(operation));
        var mutated = host.Replace(body, body.TrimEnd('}') + "\n" + injected + "\n}", StringComparison.Ordinal);

        Assert.NotEqual(host, mutated);
        Assert.NotEmpty(HostViolations(mutated));
    }

    [Theory]
    [InlineData("SetPendingText")]
    [InlineData("CommitField")]
    [InlineData("CrossBoundary")]
    [InlineData("DiscardRack")]
    public void TheHostGuardTripsWhenAnOperationThatChangesADraftLeavesOutNotePilotEdit(string operation)
    {
        var host = Host();
        var body = Src.BodyOf(host, HostSignature(operation));
        var mutated = host.Replace(body, Regex.Replace(body, @"NotePilotEdit\s*\([^;]*;", string.Empty), StringComparison.Ordinal);

        Assert.NotEqual(host, mutated);
        Assert.NotEmpty(HostViolations(mutated));
    }

    [Fact]
    public void TheHostGuardAllowsWhatTheNextTaskWouldAddAnUpdateOperationThatWrites()
    {
        var update = "PilotFieldResult IWorkspacePilotEditPort.Update(WorkspaceRequest origin, string rackId)\n{\n    var tr = db.TransactionManager.StartTransaction(); tr.Commit(); btr.AppendEntity(e);\n    return default;\n}\n";
        var host = Host().Replace("private DraftSession<PushBackPilotDraftState>? FindPilotDraft", update + "private DraftSession<PushBackPilotDraftState>? FindPilotDraft", StringComparison.Ordinal);

        Assert.Contains("IWorkspacePilotEditPort.Update(", host);
        Assert.Empty(HostViolations(host));
        Assert.Empty(WriteViolations(host, Support()));
    }

    // ------------------------------------------------------------------ INV-F4T3A-03: the gate of reentry of the three reads

    private static IEnumerable<string> ReadGateViolations(string host)
    {
        var table = new[] { ("TryBeginEdit", "EditIntentOutcome", "TryBeginPilotEdit"), ("TryCheckNow", "EditCheckOutcome", "TryCheckPilotEdit"), ("TryReload", "EditIntentOutcome", "TryReloadPilotEdit") };
        foreach (var (name, outcome, inner) in table)
        {
            var body = TryBody(host, HostSignature(name));
            if (body == null) { yield return name + " is missing"; continue; }

            var squashed = Squash(body);
            if (!squashed.Contains("if(_pilotReadInProgress)return" + outcome + ".Waiting(", StringComparison.Ordinal)) yield return name + " does not answer Waiting while a read is in progress";
            if (!Regex.IsMatch(squashed, @"_pilotReadInProgress=true;try\{return" + inner + @"\(request\);\}finally\{_pilotReadInProgress=false;\}")) yield return name + " does not wrap the read in try/finally";
            if (Src.Count(body, @"_pilotReadInProgress\s*=\s*true") != 1 || Src.Count(body, @"_pilotReadInProgress\s*=\s*false") != 1) yield return name + " sets the mark more or less than once";
            if (Src.Count(body, @"\b" + inner + @"\s*\(") != 1) yield return name + " does not call " + inner + " exactly once";
        }

        // The mark is a bool in memory and the three reads are its only writers (a lower bound: later tasks may read too).
        if (Src.Count(host, @"private\s+bool\s+_pilotReadInProgress\s*;") != 1) yield return "the mark of the reads is not one private bool";
        if (Src.Count(host, @"_pilotReadInProgress\s*=\s*true") < 3) yield return "the three reads do not set the mark";
    }

    [Fact]
    public void TheMarkOfReadInProgressWrapsEachOfTheThreeReadsWithTryFinallyAndAnsweredWaitingForANestedRead()
    {
        Assert.Empty(ReadGateViolations(Host()));
    }

    [Theory]
    [InlineData("_pilotReadInProgress = false;\r\n            }", "}")]
    [InlineData("if (_pilotReadInProgress) return EditCheckOutcome.Waiting(PilotReadInProgressReason);", "")]
    [InlineData("return TryReloadPilotEdit(request);", "return TryBeginPilotEdit(request);")]
    public void TheReadGuardTripsWithoutTheFinallyWithoutTheWaitingAnswerOrWithAnotherRead(string from, string to)
    {
        var host = Host();
        var normalized = host.Replace("\r\n", "\n");
        var fromNormalized = from.Replace("\r\n", "\n");
        // The first form (the finally) removes only the reset of the first read: the squashed shape of that body breaks.
        var index = normalized.IndexOf(fromNormalized, StringComparison.Ordinal);
        Assert.True(index >= 0, from);
        var mutated = normalized.Remove(index, fromNormalized.Length).Insert(index, to);

        Assert.NotEmpty(ReadGateViolations(mutated));
    }

    [Fact]
    public void TheReadGuardAllowsAnotherReadThatALaterTaskWouldAddWithItsOwnMark()
    {
        var extra = "EditIntentOutcome IWorkspacePilotEditPort.TryReadAgain(EditIntentRequest request)\n{\n    if (_pilotReadInProgress) return EditIntentOutcome.Waiting(\"x\");\n    _pilotReadInProgress = true;\n    try { return TryBeginPilotEdit(request); } finally { _pilotReadInProgress = false; }\n}\n";
        var host = Host().Replace("private DraftSession<PushBackPilotDraftState>? FindPilotDraft", extra + "private DraftSession<PushBackPilotDraftState>? FindPilotDraft", StringComparison.Ordinal);

        Assert.Empty(ReadGateViolations(host));
    }

    // ------------------------------------------------------------------ INV-F4T3A-07: the decorator of the gate of notices

    private static IEnumerable<string> GateViolations(string support)
    {
        var gate = TryBody(support, @"public\s+sealed\s+class\s+WorkspacePilotEditGate\s*:\s*IWorkspacePilotEditPort\b[^{]*");
        if (gate == null) { yield return "the decorator is missing"; yield break; }

        if (Src.Count(support, @"public\s+sealed\s+class\s+WorkspaceNoticeDepth\b") != 1) yield return "the counter of notices is missing";

        foreach (var (name, _) in Operations)
        {
            var body = TryBody(gate, @"public\s+[\w<>,\s?]+?\s+" + name + @"\s*\(");
            if (body == null) { yield return "the decorator lacks " + name; continue; }

            var squashed = Squash(body);
            if (ReadOperations.Contains(name))
            {
                var waiting = squashed.IndexOf("_depth.IsInside?", StringComparison.Ordinal);
                var delegated = squashed.IndexOf(":_inner." + name + "(", StringComparison.Ordinal);
                if (waiting < 0 || delegated < waiting || !squashed.Contains(".Waiting(InsideNoticeReason)", StringComparison.Ordinal))
                    yield return name + " does not wait inside a notice before it delegates";
            }
            else if (!Regex.IsMatch(squashed, @"=>_inner\." + name + @"\([\w,]*\);$"))
            {
                yield return name + " is not a plain delegation";
            }
        }

        foreach (Match match in Regex.Matches(gate, @"Dispatcher|\bTimer\b|\bTask\b|\basync\b|\bevent\b|\+=|\bstatic\b")) yield return "the decorator names " + match.Value;
        if (Regex.IsMatch(gate, @"_inner\.(?:TryBeginEdit|TryCheckNow|TryReload)\s*\([^;]*\)\s*;\s*return")) yield return "a read is delegated before the answer of the gate";
    }

    [Fact]
    public void TheDecoratorAnswersWaitingInsideANoticeForTheThreeReadsAndDelegatesTheRestAsItIs()
    {
        Assert.Empty(GateViolations(Support()));
    }

    [Theory]
    [InlineData("_depth.IsInside ? EditIntentOutcome.Waiting(InsideNoticeReason) : _inner.TryBeginEdit(request)", "_inner.TryBeginEdit(request)")]
    [InlineData("_depth.IsInside ? EditCheckOutcome.Waiting(InsideNoticeReason) : _inner.TryCheckNow(request)", "_inner.TryCheckNow(request)")]
    [InlineData("_depth.IsInside ? EditIntentOutcome.Waiting(InsideNoticeReason) : _inner.TryReload(request)", "_inner.TryReload(request)")]
    [InlineData("=> _inner.DiscardRack(origin, rackId);", "=> _depth.IsInside ? default : _inner.DiscardRack(origin, rackId);")]
    [InlineData("public WorkspacePilotEditGate(", "private static int _x; public WorkspacePilotEditGate(")]
    public void TheGateGuardTripsWhenAReadIsNotGatedOrAnotherOperationIsNotAPlainDelegation(string from, string to)
    {
        var support = Support();
        Assert.Contains(from, support);

        Assert.NotEmpty(GateViolations(support.Replace(from, to, StringComparison.Ordinal)));
    }

    // ------------------------------------------------------------------ INV-F4T3A-06 and 07: the controller

    private static IEnumerable<string> ControllerViolations(string controller)
    {
        var notify = TryBody(controller, @"private\s+void\s+NotifyStateChanged\s*\(\s*\)");
        if (notify == null) yield return "NotifyStateChanged is missing";
        else
        {
            var enter = notify.IndexOf("_noticeDepth.Enter();", StringComparison.Ordinal);
            var tryAt = Regex.Match(notify, @"\btry\b").Index;
            if (enter < 0 || enter > tryAt) yield return "the depth of notices is not entered before the try";
            if (!Regex.IsMatch(Squash(notify), @"finally\{_noticeDepth\.Leave\(\);\}")) yield return "the depth of notices is not left in a finally";
            if (!notify.Contains("StateChanged?.Invoke();", StringComparison.Ordinal)) yield return "the notice is not raised inside";
            if (Regex.IsMatch(notify, @"Dispatcher|\+=|\bTimer\b|\bTask\b|\basync\b")) yield return "NotifyStateChanged schedules or subscribes";
        }

        var getter = TryBody(controller, @"public\s+IWorkspacePilotEditPort\s+PilotEdit\b");
        if (getter == null) yield return "PilotEdit is missing";
        else
        {
            var navigation = getter.IndexOf("Navigation", StringComparison.Ordinal);
            var ret = getter.IndexOf("return", StringComparison.Ordinal);
            if (navigation < 0 || ret < 0 || navigation > ret) yield return "the getter does not reach Navigation before it returns";
            if (!Regex.IsMatch(getter, @"return\s+_pilotEdit\s*;")) yield return "the getter does not return the adapter";
        }

        if (!Regex.IsMatch(controller, @"_pilotEdit\s*=\s*new\s+PilotEditAdapter\s*\(\s*new\s+WorkspacePilotEditGate\s*\(\s*_host\s*,\s*_noticeDepth\s*\)\s*\)\s*;"))
            yield return "the adapter is not built over the decorator of the host";

        var adapter = TryBody(controller, @"private\s+sealed\s+class\s+PilotEditAdapter\s*:\s*IWorkspacePilotEditPort\b[^{]*");
        if (adapter == null) yield return "the nested adapter is missing";
        else
        {
            foreach (var (name, _) in Operations)
            {
                var body = TryBody(adapter, @"public\s+[\w<>,.\s?]+?\s+" + name + @"\s*\(");
                if (body == null) yield return "the adapter lacks " + name;
                else if (!Regex.IsMatch(Squash(body), @"=>_gate\." + name + @"\([\w,]*\);$")) yield return "the adapter does not only delegate " + name;
            }
        }

        // The panel never receives the host core (M-8).
        if (Regex.IsMatch(controller, @"\bpublic\s+(?:readonly\s+)?WorkspaceHostCore\b")) yield return "the controller exposes the host core";
        if (Src.Count(controller, @"\bpublic\s+IWorkspacePilotEditPort\b") != 1) yield return "the controller does not expose exactly one edit port";

        // The limits of the controller (H-12): three +=, no -=, one event, nothing that schedules.
        if (Src.Count(controller, @"\+=") != 3) yield return "the controller has " + Src.Count(controller, @"\+=") + " += (expected 3)";
        if (Src.Count(controller, @"-=") != 0) yield return "the controller has a -=";
        if (Src.Count(controller, @"\bevent\b") != 1) yield return "the controller does not declare exactly one event";
        foreach (Match match in Regex.Matches(controller, @"\bDispatcher\b|\bTimer\b|\bTask\b|\basync\b")) yield return "the controller names " + match.Value;
    }

    [Fact]
    public void TheControllerCountsTheNoticesWithTryFinallyExposesTheEdgePortThroughANestedAdapterAndNeverTheHost()
    {
        Assert.Empty(ControllerViolations(Controller()));
    }

    [Theory]
    [InlineData("_noticeDepth.Enter();", "")]
    [InlineData("_noticeDepth.Leave();", "")]
    [InlineData("_ = Navigation;", "")]
    [InlineData("_pilotEdit = new PilotEditAdapter(new WorkspacePilotEditGate(_host, _noticeDepth));", "_pilotEdit = new PilotEditAdapter(_host);")]
    [InlineData("public EditIntentOutcome TryReload(EditIntentRequest request) => _gate.TryReload(request);", "public EditIntentOutcome TryReload(EditIntentRequest request) => _gate.TryReload(request) with { };")]
    [InlineData("public IWorkspacePilotEditPort PilotEdit", "public WorkspaceHostCore Host => _host;\n        public IWorkspacePilotEditPort PilotEdit")]
    [InlineData("_bridge.CommandStateChanged += OnCommandStateChanged;", "_bridge.CommandStateChanged += OnCommandStateChanged; StateChanged += OnTheSide;")]
    public void TheControllerGuardTripsWithoutTheDepthTheNavigationTheDecoratorOrWhenTheHostIsExposedOrASubscriptionIsAdded(string from, string to)
    {
        var controller = Controller();
        Assert.Contains(from, controller);

        Assert.NotEmpty(ControllerViolations(controller.Replace(from, to, StringComparison.Ordinal)));
    }

    // ------------------------------------------------------------------ INV-F4T3A-06: the modal scope

    private static readonly string[] ModalOwners =
    {
        "src/RackCad.Application/Workspace/EventBridgeCore.cs",
        "src/RackCad.Application/Workspace/WorkspaceHostCore.cs",
        "src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs",
        "src/RackCad.Plugin/Workspace/WorkspaceController.cs",
    };

    private static IEnumerable<string> ModalViolations(string controller, IReadOnlyList<(string Path, string Code)> product)
    {
        var enter = TryBody(controller, @"public\s+IDisposable\s+EnterPanelModal\s*\(\s*\)");
        if (enter == null) yield return "EnterPanelModal is missing";
        else
        {
            if (!Regex.IsMatch(enter, @"new\s+WorkspacePanelModalScope\s*\([^;]*_host\.SetModalRackCadActive\s*\(")) yield return "EnterPanelModal is not over _host.SetModalRackCadActive";
            if (!Regex.IsMatch(enter, @"return\s+_modalScope\.Enter\s*\(\s*\)\s*;")) yield return "EnterPanelModal does not return the scope";
        }

        // The public forward is literal and nobody else calls the host from the controller: the forward and EnterPanelModal, and no more.
        var forward = TryBody(controller, @"public\s+void\s+SetModalRackCadActive\s*\(\s*bool\s+active\s*\)");
        if (forward == null || Squash(forward) != "publicvoidSetModalRackCadActive(boolactive)=>_host.SetModalRackCadActive(active);") yield return "the public forward of the controller is not literal";
        var calls = Src.Count(controller, @"_host\.SetModalRackCadActive\s*\(");
        if (calls != 2) yield return "_host.SetModalRackCadActive is called " + calls + " times in the controller, expected 2 (the forward and EnterPanelModal)";

        // The chain below does not change: the bridge forwards to its core and the host to its bridge link.
        var bridge = product.FirstOrDefault(f => f.Path == BridgePath).Code ?? string.Empty;
        if (Squash(TryBody(bridge, @"public\s+void\s+SetModalRackCadActive\s*\(\s*bool\s+active\s*\)") ?? string.Empty) != "publicvoidSetModalRackCadActive(boolactive)=>_core.SetModalRackCadActive(active);")
            yield return "the bridge no longer forwards the modal flag to its core literally";

        // Nobody else drives the flag: no file outside the four owners calls it, and nobody calls it on the controller.
        foreach (var (path, code) in product)
        {
            if (!ModalOwners.Contains(path) && Regex.IsMatch(code, @"\.SetModalRackCadActive\s*\(")) yield return path + " calls SetModalRackCadActive";
            if (path != ControllerPath && Regex.IsMatch(code, @"(?:GetOrCreate\s*\(\s*\)|\b\w*[Cc]ontroller\w*)\s*\.\s*SetModalRackCadActive")) yield return path + " calls SetModalRackCadActive on the controller";
        }
    }

    private static IReadOnlyList<(string Path, string Code)> Product() => Src.AllProduct();

    [Fact]
    public void TheModalFlagIsDrivenOnlyByTheLiteralForwardAndByEnterPanelModalOverTheScope()
    {
        Assert.Empty(ModalViolations(Controller(), Product()));
        Assert.Matches(@"public\s+sealed\s+class\s+WorkspacePanelModalScope\b", Support());
    }

    [Theory]
    [InlineData("public void SetModalRackCadActive(bool active) => _host.SetModalRackCadActive(active);", "public void SetModalRackCadActive(bool active) { _host.SetModalRackCadActive(active); }")]
    [InlineData("_modalScope ??= new WorkspacePanelModalScope(active => _host.SetModalRackCadActive(active));", "_modalScope ??= new WorkspacePanelModalScope(active => { });")]
    [InlineData("return _modalScope.Enter();", "return null;")]
    public void TheModalGuardTripsOnAChangedForwardOrAnEnterPanelModalNotOverTheHost(string from, string to)
    {
        var controller = Controller();
        Assert.Contains(from, controller);

        Assert.NotEmpty(ModalViolations(controller.Replace(from, to, StringComparison.Ordinal), Product()));
    }

    [Theory]
    [InlineData("src/RackCad.UI/Workspace/WorkspacePaletteHostX.cs", "WorkspaceController.GetOrCreate().SetModalRackCadActive(true);")]
    [InlineData("src/RackCad.UI/Workspace/WorkspacePaletteHostX.cs", "_controller.SetModalRackCadActive(true);")]
    [InlineData("src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs", "_controller.SetModalRackCadActive(false);")]
    [InlineData("src/RackCad.Plugin/Commands/Other.cs", "host.SetModalRackCadActive(true);")]
    public void TheModalGuardTripsWhenSomeoneElseCallsTheFlagOrTheController(string path, string injected)
    {
        var product = Product().ToList();
        product.Add((path, injected));

        Assert.NotEmpty(ModalViolations(Controller(), product));
    }

    private static IEnumerable<(int Start, int End)> ModalScopes(string code)
    {
        foreach (Match scope in Regex.Matches(code, @"\busing\s*\(\s*(?:\w+\s*\.\s*)*EnterPanelModal\s*\(\s*\)\s*\)\s*"))
        {
            var at = scope.Index + scope.Length;
            if (at < code.Length && code[at] == '{')
            {
                var depth = 0;
                for (var i = at; i < code.Length; i++)
                {
                    if (code[i] == '{') depth++;
                    else if (code[i] == '}' && --depth == 0) { yield return (at, i); break; }
                }
            }
            else
            {
                var end = code.IndexOf(';', at);
                yield return (at, end < 0 ? code.Length : end);
            }
        }
    }

    // Every dialog call of the folders of the workspace goes inside the scope of the modal of the panel (D-T3-05). Today there is none.
    private static IEnumerable<string> DialogViolations(string code)
    {
        var scopes = ModalScopes(code).ToList();
        foreach (Match call in Regex.Matches(code, @"\b(?:ShowModalWindow|ShowDialog)\s*\(|\bMessageBox\b"))
            if (!scopes.Any(s => call.Index >= s.Start && call.Index <= s.End))
                yield return "a dialog is opened outside EnterPanelModal: " + call.Value;
    }

    [Fact]
    public void EveryDialogOfTheWorkspaceFoldersIsInsideTheScopeOfTheModalOfThePanelAndTodayThereIsNone()
    {
        var folders = new[] { "src/RackCad.Application/Workspace/", "src/RackCad.Plugin/Workspace/", "src/RackCad.UI/Workspace/" };
        foreach (var folder in folders)
            foreach (var (path, code) in Src.Files(folder))
                Assert.True(!DialogViolations(code).Any(), path + ": " + string.Join("; ", DialogViolations(code)));
    }

    [Theory]
    [InlineData("var w = new Window(); w.ShowDialog();", true)]
    [InlineData("MessageBox.Show(\"x\");", true)]
    [InlineData("host.ShowModalWindow(w);", true)]
    [InlineData("using (_controller.EnterPanelModal()) { w.ShowDialog(); }", false)]
    [InlineData("using (EnterPanelModal()) { MessageBox.Show(\"x\"); w.ShowModalWindow(w); }", false)]
    [InlineData("using (EnterPanelModal()) w.ShowDialog();", false)]
    [InlineData("using (EnterPanelModal()) { var a = 1; } w.ShowDialog();", true)]
    [InlineData("using (SomethingElse()) { w.ShowDialog(); }", true)]
    public void TheDialogGuardSeesADialogOutsideTheScopeAndAcceptsTheOnesInside(string code, bool trips)
    {
        Assert.Equal(trips, DialogViolations(code).Any());
    }

    // ------------------------------------------------------------------ INV-F4T3A-10: zero authored writes, by body

    private static IEnumerable<string> WriteViolations(string host, string support)
    {
        foreach (var (name, _) in Operations)
        {
            var body = TryBody(host, HostSignature(name));
            if (body == null) { yield return "the host lacks " + name; continue; }
            foreach (Match match in Regex.Matches(body, WriteApis)) yield return name + " names " + match.Value;
        }

        var gate = TryBody(support, @"public\s+sealed\s+class\s+WorkspacePilotEditGate\s*:\s*IWorkspacePilotEditPort\b[^{]*");
        if (gate == null) yield return "the decorator is missing";
        else
        {
            // By body, one by one: an operation that a later task adds to the decorator is outside this list.
            foreach (var (name, _) in Operations)
            {
                var body = TryBody(gate, @"public\s+[\w<>,\s?]+?\s+" + name + @"\s*\(");
                if (body == null) { yield return "the decorator lacks " + name; continue; }
                foreach (Match match in Regex.Matches(body, WriteApis)) yield return "the decorator " + name + " names " + match.Value;
            }
        }

        // The helpers that the operations use, and the value helpers of the support file, write nothing either.
        foreach (var helper in new[] { @"private\s+DraftSession<PushBackPilotDraftState>\?\s+FindPilotDraft\b", @"private\s+static\s+PilotFieldView\s+ViewOfPilotDraft\b", @"private\s+bool\s+SessionStillAtRisk\b" })
        {
            var body = TryBody(host, helper);
            if (body == null) continue;
            foreach (Match match in Regex.Matches(body, WriteApis)) yield return "a helper names " + match.Value;
        }

        foreach (Match match in Regex.Matches(support, @"OpenMode\.(?:ForWrite|ForNotify)|UpgradeOpen|AppendEntity|StartTransaction|LockDocument|SetSystemVariable|SendStringToExecute")) yield return "the support file names " + match.Value;
    }

    [Fact]
    public void TheOperationsOfThePortAndTheDecoratorNameNoApiOfWritingTheDrawing()
    {
        Assert.Empty(WriteViolations(Host(), Support()));
    }

    [Theory]
    [InlineData("TryBeginEdit")]
    [InlineData("GetField")]
    [InlineData("SetPendingText")]
    [InlineData("CommitField")]
    [InlineData("CrossBoundary")]
    [InlineData("DiscardRack")]
    [InlineData("DiscardDocument")]
    [InlineData("GetRacksAtRisk")]
    [InlineData("TakeLossNotices")]
    [InlineData("TakeCloseNotices")]
    public void TheWriteGuardTripsOnAWriteInEachOperationOfThePort(string operation)
    {
        var host = Host();
        var body = Src.BodyOf(host, HostSignature(operation));
        var mutated = host.Replace(body, body.TrimEnd('}') + "\n tr.Commit(); btr.AppendEntity(e); \n}", StringComparison.Ordinal);

        Assert.NotEqual(host, mutated);
        Assert.NotEmpty(WriteViolations(mutated, Support()));
    }

    [Fact]
    public void TheWriteGuardTripsOnAWriteInTheDecoratorAndAllowsAnOperationThatTheNextTaskAdds()
    {
        var support = Support();
        var mutated = support.Replace("public PilotFieldView GetField(WorkspaceRequest origin, string rackId) => _inner.GetField(origin, rackId);", "public PilotFieldView GetField(WorkspaceRequest origin, string rackId) { tr.Commit(); return _inner.GetField(origin, rackId); }", StringComparison.Ordinal);

        Assert.NotEqual(support, mutated);
        Assert.NotEmpty(WriteViolations(Host(), mutated));

        // A later operation of Update, with its own write, is outside the list of the bodies of this task.
        var extra = support.Replace("public IReadOnlyList<PilotCloseView> TakeCloseNotices()", "public PilotFieldResult Update(WorkspaceRequest origin, string rackId) { tr.Commit(); return default; }\n        public IReadOnlyList<PilotCloseView> TakeCloseNotices()", StringComparison.Ordinal);
        Assert.NotEqual(support, extra);
        Assert.Empty(WriteViolations(Host(), extra));
    }

    // ------------------------------------------------------------------ INV-F4T3A-12: the traps of the neighbouring guards

    private static IEnumerable<string> DeclaredTypeNames(string code) =>
        Regex.Matches(code, @"\b(?:class|struct|enum|interface|record(?:\s+struct)?)\s+(\w+)").Cast<Match>().Select(m => m.Groups[1].Value);

    private static ISet<string> WordsOf(IEnumerable<string> codes) =>
        codes.SelectMany(c => Regex.Matches(c, @"\b[A-Za-z_]\w*\b").Cast<Match>().Select(m => m.Value)).ToHashSet(StringComparer.Ordinal);

    private static IEnumerable<string> NameViolations(IEnumerable<string> names, ISet<string> reserved)
    {
        foreach (var name in names)
        {
            if (Regex.IsMatch(name, "Draft|Editor|Subscription|Tab", RegexOptions.IgnoreCase)) yield return name + " has a forbidden part";
            if (reserved.Contains(name)) yield return name + " is a word of BrowserSession.cs or of the drafts";
        }
    }

    private static readonly Regex CodeTraps = new(@"Transaction|\bDatabase\b|LockDocument|\bCommit\s*\(|RackLog");

    private static readonly Regex NameTraps = new(@"Provider|Metric|Aggregat|\bBom|TotalRacks|ProjectSummary", RegexOptions.IgnoreCase);

    private static IEnumerable<string> TrapViolations(string port, string support, string host)
    {
        var reserved = WordsOf(Src.Files(DraftsFolder).Select(f => f.Code).Append(Src.Code(BrowserSessionPath)));
        foreach (var violation in NameViolations(DeclaredTypeNames(port).Concat(DeclaredTypeNames(support)), reserved)) yield return violation;

        foreach (var (label, code) in new[] { ("the port", port), ("the support file", support), ("the host core", host) })
        {
            foreach (Match match in CodeTraps.Matches(code)) yield return label + " names " + match.Value;
            foreach (Match match in NameTraps.Matches(code)) yield return label + " names " + match.Value;
        }

        // Only the core, the reader interface and the intent file name the pilot (H-10): the two new files do not.
        foreach (var (label, code) in new[] { ("the port", port), ("the support file", support) })
            if (Regex.IsMatch(code, @"\bPushBackPilot\w*|RackCad\.Application\.Systems\.PushBack\.Pilot")) yield return label + " names the pilot";
    }

    [Fact]
    public void TheNewTypesAndTheNewCodeOfApplicationAvoidTheTrapsOfTheNeighbouringGuards()
    {
        Assert.Empty(TrapViolations(Port(), Support(), Host()));

        // The three files that the older guards read, comments included, avoid the words they forbid (the package asks for it).
        foreach (var path in new[] { PortPath, SupportPath, HostPath, IntentPath })
        {
            var text = Src.Text(path);

            Assert.DoesNotMatch(new Regex(@"Transaction|\bDatabase\b|\bCommit\s*\("), text);
            Assert.DoesNotMatch(new Regex(@"Metric|ProjectSummary|TotalRacks|Aggregat|Provider|\bBom|ComputedParameter", RegexOptions.IgnoreCase), text);
        }

        Assert.DoesNotMatch(new Regex(@"\bAutodesk\.|System\.Windows|RackCad\.Plugin|RackCad\.UI|System\.IO|\bFile\."), Port() + Support());
        Assert.DoesNotMatch(new Regex(@"\b(?:class|struct|interface|record|enum)\s+\w*Log\w*\b"), Port() + Support());
        Assert.StartsWith("#nullable enable", Src.Text(SupportPath).TrimStart((char)0xFEFF));
    }

    [Theory]
    [InlineData("public sealed class WorkspacePilotDraftGate { }")]
    [InlineData("public sealed class WorkspaceStableThing { }")]
    [InlineData("public sealed class WorkspacePilotTableView { }")]
    [InlineData("public sealed class PilotEditorPart { }")]
    [InlineData("public sealed class PilotSubscriptionX { }")]
    [InlineData("public sealed class PilotMetricView { }")]
    [InlineData("public sealed class Status { }")]
    [InlineData("public sealed class BomView { }")]
    [InlineData("var t = db.TransactionManager;")]
    [InlineData("var d = Database.Open();")]
    [InlineData("var l = doc.LockDocument();")]
    [InlineData("tr.Commit();")]
    [InlineData("RackLog.Exception(\"x\", null);")]
    [InlineData("var p = new PushBackPilotFieldAdapter();")]
    public void TheTrapGuardTripsOnEachNameAndEachWordThatTheNeighbouringGuardsForbid(string injected)
    {
        Assert.Empty(TrapViolations(Port(), Support(), Host()));

        Assert.NotEmpty(TrapViolations(Port() + "\n" + injected, Support(), Host()));
        Assert.NotEmpty(TrapViolations(Port(), Support() + "\n" + injected, Host()));
    }

    [Fact]
    public void TheNamesThatTheTrapsForbidAreRealWordsOfBrowserSessionAndOfTheDrafts()
    {
        var reserved = WordsOf(Src.Files(DraftsFolder).Select(f => f.Code).Append(Src.Code(BrowserSessionPath)));

        Assert.Contains("DraftSession", reserved);
        Assert.Contains("BrowserSession", reserved);
        Assert.Contains("DraftBoundaryDecision", reserved);
        Assert.DoesNotContain("PilotBoundaryDecision", reserved);
        Assert.Empty(NameViolations(new[] { "PilotFieldView", "WorkspacePilotEditGate" }, reserved));
        Assert.NotEmpty(NameViolations(new[] { "DraftBoundaryDecision" }, reserved));
    }

    // ------------------------------------------------------------------ INV-F4T3A-11: the declared enum and the folders

    [Fact]
    public void TheDiscardStatusGainsBusyAtTheEndAndTheDocumentationOfTheDiscardIsForOneDraftOrTheWholeSession()
    {
        var names = Enum.GetNames<EditDiscardStatus>();

        Assert.Equal(new[] { "Rejected", "Discarded", "Failed", "Busy" }, names.Take(4).ToArray());
        Assert.Equal(EditDiscardStatus.Rejected, default(EditDiscardOutcome).Status);
        Assert.Equal(EditDiscardStatus.Busy, EditDiscardOutcome.Busy("a read is in progress").Status);
        Assert.Empty(EditDiscardOutcome.Busy("x").RemovedRackIds);

        var text = Src.Text(IntentPath);
        var at = text.IndexOf("public readonly struct EditDiscardOutcome", StringComparison.Ordinal);
        var summary = text.LastIndexOf("<summary>", at, StringComparison.Ordinal);
        var doc = text.Substring(summary, at - summary);
        Assert.Contains("one draft or of every draft", doc);
        var discarded = text.IndexOf("        Discarded,", StringComparison.Ordinal);
        var discardedDoc = text.Substring(text.LastIndexOf("<summary>", discarded, StringComparison.Ordinal), discarded - text.LastIndexOf("<summary>", discarded, StringComparison.Ordinal));
        Assert.Contains("One draft, or every draft", discardedDoc);
    }

    [Fact]
    public void TheDocumentationOfWaitingAndOfNotAdmittedTellsTheReloadCases()
    {
        var text = Src.Text(IntentPath);

        var waiting = text.IndexOf("        Waiting,", StringComparison.Ordinal);
        var waitingStart = text.LastIndexOf("<summary>", waiting, StringComparison.Ordinal);
        var waitingDoc = text.Substring(waitingStart, waiting - waitingStart);
        Assert.Contains("AFTER the read", waitingDoc);
        Assert.Contains("reload", waitingDoc);

        var notAdmitted = text.IndexOf("        NotAdmitted,", StringComparison.Ordinal);
        var notAdmittedStart = text.LastIndexOf("<summary>", notAdmitted, StringComparison.Ordinal);
        var notAdmittedDoc = text.Substring(notAdmittedStart, notAdmitted - notAdmittedStart);
        Assert.Contains("in the reload the draft", notAdmittedDoc);
    }

    [Fact]
    public void ThisTaskAddsNoFileToThePluginWorkspaceFolderNorToTheDraftsAndNoCommandNorSubscription()
    {
        Assert.Equal(9, Src.PluginFiles().Count);
        Assert.Equal(5, Src.Files(DraftsFolder).Count);
        Assert.Equal(16, Src.Count(Src.Code(BridgePath), @"\+="));
        Assert.Equal(16, Src.Count(Src.Code(BridgePath), @"-="));
        Assert.Equal(16, Src.Count(Src.Code(BridgePath), @"\bhandler\s*=\s*\("));
        Assert.Equal(1, Src.Count(Src.Code(BridgePath), @"private\s+void\s+OnIdle\s*\(\s*\)"));
        Assert.DoesNotMatch(new Regex(@"Debug\.Assert"), Src.Code(BridgePath));
        Assert.DoesNotMatch(new Regex(@"\[CommandMethod"), Controller() + Support() + Port());
    }
}
