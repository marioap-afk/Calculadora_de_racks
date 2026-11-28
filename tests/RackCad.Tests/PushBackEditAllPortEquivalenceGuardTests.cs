#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.Workspace;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F4B PB-T4c (owner-pbt4-decisions.md, pregunta 4; D-PBT4-13): the equivalent of WorkspacePilotEditGuardTests:155-163, whose Skip is in the
// closed list of PB-T4c. It is that test with the members of its file that it reaches, literal, and ONE more row in the table of the operations:
// TryEditAll, with its default body, at the end of the port, so the port declares eighteen operations and the reflection sees the eighteen.
public class PushBackEditAllPortEquivalenceGuardTests
{
    private const string PortPath = "src/RackCad.Application/Workspace/IWorkspacePilotEditPort.cs";

    private static string Port() => Src.Code(PortPath);

    private static string? TryBody(string code, string signature) => Regex.IsMatch(code, signature) ? Src.BodyOf(code, signature) : null;

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

        // I-64 F4B PB-T4c (D-PBT4-13; owner-pbt4-decisions.md, pregunta 4): "Editar todo", with its default body, at the end of the port.
        ("TryEditAll", @"EditAllOutcome\s+TryEditAll\s*\(\s*EditIntentRequest\s+\w+\s*\)\s*=>\s*EditAllOutcome\.NotAvailable\s*\(\s*null\s*\)\s*;"),
    };

    private static readonly Regex DraftOrPilotNames = new(@"\bDraft\w*|\bPushBack\w*|RackCad\.Application\.Systems|Workspace\.Drafts");

    private static readonly (string Enum, string[] Prefix)[] OrderedEnums =
    {
        ("PilotFieldState", new[] { "NotOpen", "Clean", "Dirty", "Conflict", "Applying", "Unknown", "Error", "Undecidable" }),
        ("PilotBoundaryDecision", new[] { "Proceeded", "Aborted", "Kept", "Busy" }),
        ("PilotBoundaryKind", new[] { "TabChange", "RackChange", "DocumentChange", "PanelHide", "Dematerialize", "DocumentClose", "Update" }),
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

        // The seventeen operations (the two of the update are the last) are a lower bound: a later task may add its own to this port.
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
    public void ThePortDeclaresTheSeventeenOperationsWithTheEnumsAsOrderedPrefixesAndNamesNoTypeOfTheDraftsNorOfThePilot()
    {
        Assert.Empty(PortViolations(Port()));
        Assert.Equal(
            Operations.Select(o => o.Name).OrderBy(n => n, StringComparer.Ordinal).ToArray(),
            typeof(IWorkspacePilotEditPort).GetMethods().Select(m => m.Name).OrderBy(n => n, StringComparer.Ordinal).ToArray());
        Assert.StartsWith("#nullable enable", Src.Text(PortPath).TrimStart((char)0xFEFF));
    }
}
