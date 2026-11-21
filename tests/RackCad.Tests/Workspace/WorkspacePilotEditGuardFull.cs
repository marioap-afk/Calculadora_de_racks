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
public class FullPanelCopyWorkspacePilotEditGuardRedTests
{
    private const string SupportPath = "src/RackCad.Application/Workspace/WorkspacePilotSupport.cs";
    private const string HostPath = "src/RackCad.Application/Workspace/WorkspaceHostCore.cs";

    private static string Support() => Src.Code(SupportPath);

    private static string Host() => Src.Code(HostPath);

    private static string? TryBody(string code, string signature) => Regex.IsMatch(code, signature) ? Src.BodyOf(code, signature) : null;

    // The twelve operations of the table of D-T3-02 and, since F4-T4A1 (D-T4A1-03), the three of the explicit reconciliation: the name and the
    // signature in the interface. The twelve are a lower bound of the port and the fifteen are what this task adds to it.
    private static readonly (string Name, string Interface)[] Operations =
    {
        ("TryBeginEdit", @"EditIntentOutcome\s+TryBeginEdit\s*\(\s*EditIntentRequest\s+\w+\s*\)\s*;"),
        ("TryCheckNow", @"EditCheckOutcome\s+TryCheckNow\s*\(\s*EditIntentRequest\s+\w+\s*\)\s*;"),
        ("TryReload", @"EditIntentOutcome\s+TryReload\s*\(\s*EditIntentRequest\s+\w+\s*\)\s*;"),
        ("TryConfirmBase", @"EditConfirmOutcome\s+TryConfirmBase\s*\(\s*EditIntentRequest\s+\w+\s*\)\s*;"),
        ("TryKeepMine", @"EditIntentOutcome\s+TryKeepMine\s*\(\s*EditIntentRequest\s+\w+\s*\)\s*;"),
        ("TryDiscardAndReload", @"EditIntentOutcome\s+TryDiscardAndReload\s*\(\s*EditIntentRequest\s+\w+\s*\)\s*;"),
        ("GetField", @"PilotFieldView\s+GetField\s*\(\s*WorkspaceRequest\s+\w+\s*,\s*string\s+\w+\s*\)\s*;"),
        ("SetPendingText", @"PilotFieldResult\s+SetPendingText\s*\(\s*WorkspaceRequest\s+\w+\s*,\s*string\s+\w+\s*,\s*string\s+\w+\s*\)\s*;"),
        ("CommitField", @"PilotFieldResult\s+CommitField\s*\(\s*WorkspaceRequest\s+\w+\s*,\s*string\s+\w+\s*\)\s*;"),
        ("CrossBoundary", @"PilotBoundaryResult\s+CrossBoundary\s*\(\s*WorkspaceRequest\s+\w+\s*,\s*string\s+\w+\s*,\s*PilotBoundaryKind\s+\w+\s*\)\s*;"),
        ("DiscardRack", @"EditDiscardOutcome\s+DiscardRack\s*\(\s*WorkspaceRequest\s+\w+\s*,\s*string\s+\w+\s*\)\s*;"),
        ("DiscardDocument", @"EditDiscardOutcome\s+DiscardDocument\s*\(\s*WorkspaceRequest\s+\w+\s*\)\s*;"),
        ("GetRacksAtRisk", @"PilotRiskView\s+GetRacksAtRisk\s*\(\s*WorkspaceRequest\s+\w+\s*\)\s*;"),
        ("TakeLossNotices", @"IReadOnlyList<PilotLossView>\s+TakeLossNotices\s*\(\s*\)\s*;"),
        ("TakeCloseNotices", @"IReadOnlyList<PilotCloseView>\s+TakeCloseNotices\s*\(\s*\)\s*;"),

        // I-64 F4-T4C1-HOST (D-T4C-20, row 5): the two operations of the update, at the end of the port.
        ("RequestUpdate", @"EditUpdateStart\s+RequestUpdate\s*\(\s*EditIntentRequest\s+\w+\s*\)\s*;"),
        ("TakeUpdateProgress", @"EditUpdateProgress\s+TakeUpdateProgress\s*\(\s*WorkspaceRequest\s+\w+\s*,\s*string\s+\w+\s*\)\s*;"),
    };

    // The six operations that read the drawing: the three of F4-T3A and the three of the explicit reconciliation of F4-T4A1.
    private static readonly string[] ReadOperations = { "TryBeginEdit", "TryCheckNow", "TryReload", "TryConfirmBase", "TryKeepMine", "TryDiscardAndReload" };

    private static readonly string[] MemoryOperations = { "GetField", "GetRacksAtRisk", "TakeLossNotices", "TakeCloseNotices", "TakeUpdateProgress" };

    private static readonly string[] MutatingOperations = { "SetPendingText", "CommitField", "CrossBoundary", "DiscardRack", "DiscardDocument", "RequestUpdate" };

    // The one operation of the request of the update: the only one, outside the six reads, that may look at the rest, the executor and the
    // gate of the document, and the only one that asks the executor of the host. Its authorization is the one of the gate (PassGate).
    private static readonly string[] UpdateOperations = { "RequestUpdate" };

    // The operations that change a draft, or that discard one, while an update is in flight: after their authorization (Authorize or the
    // gate, PassGate) and, where the body has it, the mark of the reads they answer Busy or Waiting, with no change (D-T4C-08 and D-T4C-10,
    // row 5; M-01). The three reconciliations are looked at in the body that passes the gate, which is the one of the pilot (the wrapper of
    // the port only marks the read), so the check of the flight can never come before the authorization.
    private static readonly (string Name, string Signature, bool MarkInBody)[] FlightBodies =
    {
        ("SetPendingText", HostSignature("SetPendingText"), true),
        ("CommitField", HostSignature("CommitField"), true),
        ("TryKeepPilotEdit", @"public\s+EditIntentOutcome\s+TryKeepPilotEdit\s*\(", false),
        ("TryDiscardAndReloadPilotEdit", @"public\s+EditIntentOutcome\s+TryDiscardAndReloadPilotEdit\s*\(", false),
        ("TryConfirmPilotBase", @"public\s+EditConfirmOutcome\s+TryConfirmPilotBase\s*\(", false),
        ("DiscardRack", HostSignature("DiscardRack"), true),
        ("DiscardDocument", HostSignature("DiscardDocument"), true),
    };

    private static readonly Regex FlightCheck = new(@"\bUpdateInFlightFor(?:Request)?\s*\(|\bExpireStaleUpdate\s*\(");

    // The first place where a body authorizes: its own Authorize or the gate, whose last step is the authorization of the session.
    private static int FirstAuthorization(string body)
    {
        var places = new[] { body.IndexOf("Authorize(", StringComparison.Ordinal), body.IndexOf("PassGate(", StringComparison.Ordinal) }.Where(i => i >= 0).ToArray();
        return places.Length == 0 ? -1 : places.Min();
    }

    private static string HostSignature(string name) => @"\w[\w<>,\s]*?\s+IWorkspacePilotEditPort\." + name + @"\s*\(";

    private const string WriteApis =
        @"OpenMode\.(?:ForWrite|ForNotify)|UpgradeOpen|DowngradeOpen|AppendEntity|\.Erase\s*\(|\.Commit\s*\(|RackBlockData\.Write|SetImpliedSelection|SetCurrentView|"
        + @"StartTransaction|StartOpenCloseTransaction|LockDocument|SetSystemVariable|SendStringToExecute|\bSaveAs\b|\bSave\s*\(";

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
        if (Regex.IsMatch(host, @"private\s+(?:readonly\s+)?PushBackFullFieldAdapter\s+_\w+")) yield return "the adapter of the field is a field of instance";
        if (Src.Count(host, @"private\s+static\s+readonly\s+PushBackFullFieldAdapter\s+\w+") != 1) yield return "the adapter of the field is not the one static field";

        // Only the three reads read the drawing (C03): the other nine operations never name the reader, the probe or the gate of reading.
        // Row 5: the request of the update is the one exception, with its own rules below.
        foreach (var (name, _) in Operations.Where(o => !ReadOperations.Contains(o.Name) && !UpdateOperations.Contains(o.Name)))
        {
            var body = TryBody(host, HostSignature(name));
            if (body == null) continue;
            if (Regex.IsMatch(body, @"_editIntentReader|ReadIntent|_reader\b|PassGate|_probe\b|_bridge\b")) yield return name + " reaches a reader, the probe or the bridge";
        }

        // The request of the update: it passes the gate of the document, the only place of the rest, and it is the one caller of the executor.
        foreach (var name in UpdateOperations)
        {
            var body = TryBody(host, HostSignature(name));
            if (body == null) continue;
            if (!Regex.IsMatch(body, @"\bPassGate\s*\(")) yield return name + " does not pass the gate of the document";
            if (Regex.IsMatch(body, @"_editIntentReader|ReadIntent|_reader\b")) yield return name + " reads the drawing";
            if (Src.Count(body, @"\.TryRequest\s*\(") != 1) yield return name + " does not ask the executor exactly once";
            if (Regex.IsMatch(body, @"TryUpdatePilot|\.Execute\s*\(")) yield return name + " runs the update itself";
        }

        // The lazy bound of the ticket: it only reads the count of foreign commands of the bridge, and it never touches the own mutation,
        // the reader, the probe or the gate (RR N-04). The same for the progress, which is memory only.
        var expire = TryBody(host, @"private\s+UpdateTicket\?\s+ExpireStaleUpdate\s*\(");
        if (expire == null) yield return "ExpireStaleUpdate is missing";
        else
        {
            if (Regex.IsMatch(expire, @"CloseOwnMutation|OpenOwnMutation|_editIntentReader|_reader\b|_probe\b|PassGate")) yield return "ExpireStaleUpdate reaches more than the count of foreign commands";
            if (Regex.Matches(expire, @"_bridge\.(\w+)").Cast<Match>().Any(m => m.Groups[1].Value != "ForeignCommandStartsOf")) yield return "ExpireStaleUpdate names a member of the bridge other than ForeignCommandStartsOf";
            if (!Regex.IsMatch(expire, @"_bridge\.ForeignCommandStartsOf\s*\(")) yield return "ExpireStaleUpdate does not read the count of foreign commands";
        }

        var progress = TryBody(host, HostSignature("TakeUpdateProgress"));
        if (progress != null && Regex.IsMatch(progress, @"OwnMutation|_bridge\b|_probe\b|PassGate|_lastAllowedClose")) yield return "TakeUpdateProgress mutates the bridge or looks at the document";

        // While an update is in flight (D-T4C-08, D-T4C-10; M-01): the check of the flight comes after the authorization (Authorize or
        // PassGate) and after the mark of the reads where the body has it. A body without any authorization has no order to keep: it trips.
        foreach (var (name, signature, markInBody) in FlightBodies)
        {
            var body = TryBody(host, signature);
            if (body == null) { yield return name + " is missing"; continue; }

            var check = FlightCheck.Match(body);
            var mark = body.IndexOf("_pilotReadInProgress", StringComparison.Ordinal);
            var authorize = FirstAuthorization(body);
            if (!check.Success) yield return name + " does not look at the update in flight";
            else if (authorize < 0 || check.Index < authorize) yield return name + " looks at the update in flight before its authorization";
            else if (markInBody && (mark < 0 || check.Index < mark)) yield return name + " looks at the update in flight before the mark of the reads";
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

            // The request of the update authorizes through the gate of the document, which ends with the authorization of the session.
            var authorizes = Regex.IsMatch(body, @"\bAuthorize\s*\(") || (UpdateOperations.Contains(name) && Regex.IsMatch(body, @"\bPassGate\s*\("));
            if (!authorizes) yield return name + " does not authorize";
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
        foreach (var name in new[] { "SetPendingText", "CommitField", "CrossBoundary", "DiscardRack", "DiscardDocument", "GetField", "GetRacksAtRisk", "RequestUpdate", "TakeUpdateProgress" })
        {
            var body = TryBody(host, HostSignature(name));
            if (body == null) continue;
            if (!Regex.IsMatch(body, @"catch\s*\(\s*Exception")) yield return name + " does not contain its exception";
            if (Regex.IsMatch(body, @"_faulted|\bContain\s*\(|\bthrow\s*;")) yield return name + " degrades the session or rethrows";
        }
    }

    // ------------------------------------------------------------------ INV-F4T3A-10: zero authored writes, by body

    private static readonly string[] ReconciliationBodies =
    {
        @"public\s+EditConfirmOutcome\s+TryConfirmPilotBase\s*\(",
        @"public\s+EditIntentOutcome\s+TryKeepPilotEdit\s*\(",
        @"public\s+EditIntentOutcome\s+TryDiscardAndReloadPilotEdit\s*\(",
        // F4-T4B (D-T4B-07): the body of the update of the pilot writes nothing to the drawing by itself: it only goes through the port.
        @"public\s+EditUpdateOutcome\s+TryUpdatePilot\s*\(",
    };

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

        // The three public bodies of the explicit reconciliation (F4-T4A1) write nothing either: with the explicit ones above, this is the
        // oracle of the zero writes of PREPARE (D-T4A1-07).
        foreach (var signature in ReconciliationBodies)
        {
            var body = TryBody(host, signature);
            if (body == null) { yield return "the host lacks " + signature; continue; }
            foreach (Match match in Regex.Matches(body, WriteApis)) yield return "the public body " + signature + " names " + match.Value;
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
    public void TheHostImplementsThePortExplicitlyAndNeverReadsOutsideTheThreeReadsAndChangesOnlyThroughNotePilotEdit()
    {
        Assert.Empty(HostViolations(Host()));
        Assert.True(typeof(IWorkspacePilotEditPort).IsAssignableFrom(typeof(WorkspaceHostCore)));
        foreach (var (name, _) in Operations) Assert.Null(typeof(WorkspaceHostCore).GetMethod(name));
        Assert.Equal(new[] { "IWorkspaceLifecycleAdvisor", "IWorkspacePilotEditPort" }, typeof(WorkspaceHostCore).GetInterfaces().Select(i => i.Name).OrderBy(n => n, StringComparer.Ordinal).ToArray());
    }

    // I-64 F4B PB-T4b (D-S5): this Theory is split by rows by the partition of the RED; here, the rows that fail there, and in the other class of the partition, the rest.
    [Theory]
    [InlineData("private static readonly PushBackFullFieldAdapter PilotFieldAdapter = new();", "private readonly PushBackFullFieldAdapter _adapter = new();")]
    public void TheHostGuardTripsOnAPublicOperationAnOrderOfBasesOrAnAdapterThatIsAFieldOfInstance(string from, string to)
    {
        var host = Host();
        Assert.Contains(from, host);

        Assert.NotEmpty(HostViolations(host.Replace(from, to, StringComparison.Ordinal)));
    }

    // M-01: the same order when the authorization is the gate (PassGate) and not an Authorize of the body itself: the flight before the gate trips.
    [Theory]
    [InlineData("TryKeepPilotEdit")]
    [InlineData("TryDiscardAndReloadPilotEdit")]
    [InlineData("TryConfirmPilotBase")]
    public void TheFlightGuardTripsWhenTheFlightIsCheckedBeforeTheGateThatAuthorizesInTheThreeReconciliations(string operation)
    {
        var host = Host();
        Assert.Empty(HostViolations(host));
        var body = Src.BodyOf(host, FlightBodies.Single(b => b.Name == operation).Signature);
        var statement = Regex.Match(body, @"if\s*\(\s*UpdateInFlightForRequest\s*\(\s*request\s*\)\s*\)\s*return\s+[^;]*;");
        var gate = body.IndexOf("var gate = PassGate(", StringComparison.Ordinal);
        Assert.True(statement.Success, "the body does not check the flight with the request");
        Assert.True(gate >= 0 && gate < statement.Index, "the body does not pass the gate before it checks the flight");

        var moved = body.Remove(statement.Index, statement.Length).Insert(gate, statement.Value + " ");
        var mutated = host.Replace(body, moved, StringComparison.Ordinal);

        Assert.NotEqual(host, mutated);
        Assert.NotEmpty(HostViolations(mutated));
    }

    [Fact]
    public void TheHostGuardAllowsWhatTheNextTaskWouldAddAnUpdateOperationThatWrites()
    {
        var update = "PilotFieldResult IWorkspacePilotEditPort.Update(WorkspaceRequest origin, string rackId)\n{\n    var tr = db.TransactionManager.StartTransaction(); tr.Commit(); btr.AppendEntity(e);\n    return default;\n}\n";
        var host = Host().Replace("private DraftSession<PushBackFullDraftState>? FindPilotDraft", update + "private DraftSession<PushBackFullDraftState>? FindPilotDraft", StringComparison.Ordinal);

        Assert.Contains("IWorkspacePilotEditPort.Update(", host);
        Assert.Empty(HostViolations(host));
        Assert.Empty(WriteViolations(host, Support()));
    }
}

// I-64 F4-T3A-EDIT-PORT (INV-F4T3A-01 to 03, 06, 07, 10, 12 and 14): the source guards of the edit port of the pilot. The Core test project
// cannot reference the Plugin (it needs AutoCAD), so the Plugin files are read as text, like the rest of the guards of the workspace. By the
// rule of I-1 (F4-E32, F4-E33) every guard here is limited to the bodies and the names that THIS task owns, the enums are ordered prefixes
// and the sets are lower bounds: none of them prohibits for ever what belongs to F4-T3B, F4-T3C or F4-T4 (an operation of Update, a new kind
// of boundary, a wrapper of the advisor, a dialog). Each guard is a function of the text, with its self-tests: the real source passes, the
// text that must trip it trips it, and the text that T4 would add does not.
public class FullPanelCopyWorkspacePilotEditGuardGreenTests
{
    private const string PortPath = "src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs";
    private const string SupportPath = "src/RackCad.Application/Workspace/WorkspacePilotSupport.cs";
    private const string HostPath = "src/RackCad.Application/Workspace/WorkspaceHostCore.cs";
    private const string DraftsFolder = "src/RackCad.Application/Workspace/Drafts/";
    private const string BrowserSessionPath = "src/RackCad.Application/Workspace/BrowserSession.cs";

    private static string Port() => Src.Code(PortPath);

    private static string Support() => Src.Code(SupportPath);

    private static string Host() => Src.Code(HostPath);

    private static string? TryBody(string code, string signature) => Regex.IsMatch(code, signature) ? Src.BodyOf(code, signature) : null;

    // The twelve operations of the table of D-T3-02 and, since F4-T4A1 (D-T4A1-03), the three of the explicit reconciliation: the name and the
    // signature in the interface. The twelve are a lower bound of the port and the fifteen are what this task adds to it.
    private static readonly (string Name, string Interface)[] Operations =
    {
        ("TryBeginEdit", @"EditIntentOutcome\s+TryBeginEdit\s*\(\s*EditIntentRequest\s+\w+\s*\)\s*;"),
        ("TryCheckNow", @"EditCheckOutcome\s+TryCheckNow\s*\(\s*EditIntentRequest\s+\w+\s*\)\s*;"),
        ("TryReload", @"EditIntentOutcome\s+TryReload\s*\(\s*EditIntentRequest\s+\w+\s*\)\s*;"),
        ("TryConfirmBase", @"EditConfirmOutcome\s+TryConfirmBase\s*\(\s*EditIntentRequest\s+\w+\s*\)\s*;"),
        ("TryKeepMine", @"EditIntentOutcome\s+TryKeepMine\s*\(\s*EditIntentRequest\s+\w+\s*\)\s*;"),
        ("TryDiscardAndReload", @"EditIntentOutcome\s+TryDiscardAndReload\s*\(\s*EditIntentRequest\s+\w+\s*\)\s*;"),
        ("GetField", @"PilotFieldView\s+GetField\s*\(\s*WorkspaceRequest\s+\w+\s*,\s*string\s+\w+\s*\)\s*;"),
        ("SetPendingText", @"PilotFieldResult\s+SetPendingText\s*\(\s*WorkspaceRequest\s+\w+\s*,\s*string\s+\w+\s*,\s*string\s+\w+\s*\)\s*;"),
        ("CommitField", @"PilotFieldResult\s+CommitField\s*\(\s*WorkspaceRequest\s+\w+\s*,\s*string\s+\w+\s*\)\s*;"),
        ("CrossBoundary", @"PilotBoundaryResult\s+CrossBoundary\s*\(\s*WorkspaceRequest\s+\w+\s*,\s*string\s+\w+\s*,\s*PilotBoundaryKind\s+\w+\s*\)\s*;"),
        ("DiscardRack", @"EditDiscardOutcome\s+DiscardRack\s*\(\s*WorkspaceRequest\s+\w+\s*,\s*string\s+\w+\s*\)\s*;"),
        ("DiscardDocument", @"EditDiscardOutcome\s+DiscardDocument\s*\(\s*WorkspaceRequest\s+\w+\s*\)\s*;"),
        ("GetRacksAtRisk", @"PilotRiskView\s+GetRacksAtRisk\s*\(\s*WorkspaceRequest\s+\w+\s*\)\s*;"),
        ("TakeLossNotices", @"IReadOnlyList<PilotLossView>\s+TakeLossNotices\s*\(\s*\)\s*;"),
        ("TakeCloseNotices", @"IReadOnlyList<PilotCloseView>\s+TakeCloseNotices\s*\(\s*\)\s*;"),

        // I-64 F4-T4C1-HOST (D-T4C-20, row 5): the two operations of the update, at the end of the port.
        ("RequestUpdate", @"EditUpdateStart\s+RequestUpdate\s*\(\s*EditIntentRequest\s+\w+\s*\)\s*;"),
        ("TakeUpdateProgress", @"EditUpdateProgress\s+TakeUpdateProgress\s*\(\s*WorkspaceRequest\s+\w+\s*,\s*string\s+\w+\s*\)\s*;"),
    };

    // The six operations that read the drawing: the three of F4-T3A and the three of the explicit reconciliation of F4-T4A1.
    private static readonly string[] ReadOperations = { "TryBeginEdit", "TryCheckNow", "TryReload", "TryConfirmBase", "TryKeepMine", "TryDiscardAndReload" };

    private static readonly string[] MemoryOperations = { "GetField", "GetRacksAtRisk", "TakeLossNotices", "TakeCloseNotices", "TakeUpdateProgress" };

    private static readonly string[] MutatingOperations = { "SetPendingText", "CommitField", "CrossBoundary", "DiscardRack", "DiscardDocument", "RequestUpdate" };

    // The one operation of the request of the update: the only one, outside the six reads, that may look at the rest, the executor and the
    // gate of the document, and the only one that asks the executor of the host. Its authorization is the one of the gate (PassGate).
    private static readonly string[] UpdateOperations = { "RequestUpdate" };

    // The operations that change a draft, or that discard one, while an update is in flight: after their authorization (Authorize or the
    // gate, PassGate) and, where the body has it, the mark of the reads they answer Busy or Waiting, with no change (D-T4C-08 and D-T4C-10,
    // row 5; M-01). The three reconciliations are looked at in the body that passes the gate, which is the one of the pilot (the wrapper of
    // the port only marks the read), so the check of the flight can never come before the authorization.
    private static readonly (string Name, string Signature, bool MarkInBody)[] FlightBodies =
    {
        ("SetPendingText", HostSignature("SetPendingText"), true),
        ("CommitField", HostSignature("CommitField"), true),
        ("TryKeepPilotEdit", @"public\s+EditIntentOutcome\s+TryKeepPilotEdit\s*\(", false),
        ("TryDiscardAndReloadPilotEdit", @"public\s+EditIntentOutcome\s+TryDiscardAndReloadPilotEdit\s*\(", false),
        ("TryConfirmPilotBase", @"public\s+EditConfirmOutcome\s+TryConfirmPilotBase\s*\(", false),
        ("DiscardRack", HostSignature("DiscardRack"), true),
        ("DiscardDocument", HostSignature("DiscardDocument"), true),
    };

    private static readonly Regex FlightCheck = new(@"\bUpdateInFlightFor(?:Request)?\s*\(|\bExpireStaleUpdate\s*\(");

    // The first place where a body authorizes: its own Authorize or the gate, whose last step is the authorization of the session.
    private static int FirstAuthorization(string body)
    {
        var places = new[] { body.IndexOf("Authorize(", StringComparison.Ordinal), body.IndexOf("PassGate(", StringComparison.Ordinal) }.Where(i => i >= 0).ToArray();
        return places.Length == 0 ? -1 : places.Min();
    }

    private static string HostSignature(string name) => @"\w[\w<>,\s]*?\s+IWorkspacePilotEditPort\." + name + @"\s*\(";

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
        if (Regex.IsMatch(host, @"private\s+(?:readonly\s+)?PushBackFullFieldAdapter\s+_\w+")) yield return "the adapter of the field is a field of instance";
        if (Src.Count(host, @"private\s+static\s+readonly\s+PushBackFullFieldAdapter\s+\w+") != 1) yield return "the adapter of the field is not the one static field";

        // Only the three reads read the drawing (C03): the other nine operations never name the reader, the probe or the gate of reading.
        // Row 5: the request of the update is the one exception, with its own rules below.
        foreach (var (name, _) in Operations.Where(o => !ReadOperations.Contains(o.Name) && !UpdateOperations.Contains(o.Name)))
        {
            var body = TryBody(host, HostSignature(name));
            if (body == null) continue;
            if (Regex.IsMatch(body, @"_editIntentReader|ReadIntent|_reader\b|PassGate|_probe\b|_bridge\b")) yield return name + " reaches a reader, the probe or the bridge";
        }

        // The request of the update: it passes the gate of the document, the only place of the rest, and it is the one caller of the executor.
        foreach (var name in UpdateOperations)
        {
            var body = TryBody(host, HostSignature(name));
            if (body == null) continue;
            if (!Regex.IsMatch(body, @"\bPassGate\s*\(")) yield return name + " does not pass the gate of the document";
            if (Regex.IsMatch(body, @"_editIntentReader|ReadIntent|_reader\b")) yield return name + " reads the drawing";
            if (Src.Count(body, @"\.TryRequest\s*\(") != 1) yield return name + " does not ask the executor exactly once";
            if (Regex.IsMatch(body, @"TryUpdatePilot|\.Execute\s*\(")) yield return name + " runs the update itself";
        }

        // The lazy bound of the ticket: it only reads the count of foreign commands of the bridge, and it never touches the own mutation,
        // the reader, the probe or the gate (RR N-04). The same for the progress, which is memory only.
        var expire = TryBody(host, @"private\s+UpdateTicket\?\s+ExpireStaleUpdate\s*\(");
        if (expire == null) yield return "ExpireStaleUpdate is missing";
        else
        {
            if (Regex.IsMatch(expire, @"CloseOwnMutation|OpenOwnMutation|_editIntentReader|_reader\b|_probe\b|PassGate")) yield return "ExpireStaleUpdate reaches more than the count of foreign commands";
            if (Regex.Matches(expire, @"_bridge\.(\w+)").Cast<Match>().Any(m => m.Groups[1].Value != "ForeignCommandStartsOf")) yield return "ExpireStaleUpdate names a member of the bridge other than ForeignCommandStartsOf";
            if (!Regex.IsMatch(expire, @"_bridge\.ForeignCommandStartsOf\s*\(")) yield return "ExpireStaleUpdate does not read the count of foreign commands";
        }

        var progress = TryBody(host, HostSignature("TakeUpdateProgress"));
        if (progress != null && Regex.IsMatch(progress, @"OwnMutation|_bridge\b|_probe\b|PassGate|_lastAllowedClose")) yield return "TakeUpdateProgress mutates the bridge or looks at the document";

        // While an update is in flight (D-T4C-08, D-T4C-10; M-01): the check of the flight comes after the authorization (Authorize or
        // PassGate) and after the mark of the reads where the body has it. A body without any authorization has no order to keep: it trips.
        foreach (var (name, signature, markInBody) in FlightBodies)
        {
            var body = TryBody(host, signature);
            if (body == null) { yield return name + " is missing"; continue; }

            var check = FlightCheck.Match(body);
            var mark = body.IndexOf("_pilotReadInProgress", StringComparison.Ordinal);
            var authorize = FirstAuthorization(body);
            if (!check.Success) yield return name + " does not look at the update in flight";
            else if (authorize < 0 || check.Index < authorize) yield return name + " looks at the update in flight before its authorization";
            else if (markInBody && (mark < 0 || check.Index < mark)) yield return name + " looks at the update in flight before the mark of the reads";
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

            // The request of the update authorizes through the gate of the document, which ends with the authorization of the session.
            var authorizes = Regex.IsMatch(body, @"\bAuthorize\s*\(") || (UpdateOperations.Contains(name) && Regex.IsMatch(body, @"\bPassGate\s*\("));
            if (!authorizes) yield return name + " does not authorize";
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
        foreach (var name in new[] { "SetPendingText", "CommitField", "CrossBoundary", "DiscardRack", "DiscardDocument", "GetField", "GetRacksAtRisk", "RequestUpdate", "TakeUpdateProgress" })
        {
            var body = TryBody(host, HostSignature(name));
            if (body == null) continue;
            if (!Regex.IsMatch(body, @"catch\s*\(\s*Exception")) yield return name + " does not contain its exception";
            if (Regex.IsMatch(body, @"_faulted|\bContain\s*\(|\bthrow\s*;")) yield return name + " degrades the session or rethrows";
        }
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

    // I-64 F4B PB-T4b (D-S5): this Theory is split by rows by the partition of the RED; here, the rows that pass there, and in the other class of the partition, the rest.
    [Theory]
    [InlineData("public sealed class WorkspaceHostCore : IWorkspaceLifecycleAdvisor, IWorkspacePilotEditPort", "public sealed class WorkspaceHostCore : IWorkspacePilotEditPort, IWorkspaceLifecycleAdvisor")]
    [InlineData("PilotFieldView IWorkspacePilotEditPort.GetField(", "public PilotFieldView GetField(")]
    [InlineData("PilotFieldResult IWorkspacePilotEditPort.CommitField(", "public PilotFieldResult CommitField(")]
    public void TheHostGuardTripsOnAPublicOperationAnOrderOfBasesOrAnAdapterThatIsAFieldOfInstance(string from, string to)
    {
        var host = Host();
        Assert.Contains(from, host);

        Assert.NotEmpty(HostViolations(host.Replace(from, to, StringComparison.Ordinal)));
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
}
