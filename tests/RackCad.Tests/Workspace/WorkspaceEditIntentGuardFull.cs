#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F4-T2B2A-INTENT-WIRING (D-T2B2-01, 02, 03, 05, 06, 08): source guards of the wiring of the edit intention. The Core test project
// cannot reference the Plugin (it needs AutoCAD), so the Plugin files are read as text, like the rest of the guards of the workspace.
// These guards are ADDITIVE and live in a new file: no existing guard was changed or weakened for this task. Every guard fails by
// ASSERTION when the text it expects is missing (a missing file or member is an assertion too). Only the guards that are followed by a
// test named "...Detects..." (or by a theory that injects a line into the real text) have such a detection test; the others read the
// real text and nothing more. They are limited to the names of this task and to the bodies of its two operations: what the package
// assigns to F4-T2B2B (the lifecycle) is not prohibited here, and the exposure to the UI exists since F4-T3A and F4-T3B1 (the edit port
// of the pilot and the panel that calls it).
//
// Evidence named by the package: R6 (no write to the drawing: the adapter, the scan, the host and the core), obligation 3 (the
// preview never reaches the intention), obligation 10 (the base of a read only reaches Begin, CheckNow or DiscardAndReload) and
// obligation 13 (the session of A never operates on B: the order of the nested reader, with the authorization before the revalidation).
public class FullPanelCopyWorkspaceEditIntentGuardRedTests
{
    private const string CorePath = "src/RackCad.Application/Workspace/WorkspaceHostCore.cs";
    private const string InterfacePath = "src/RackCad.Application/Workspace/IWorkspaceEditIntentReader.cs";
    private const string IntentPath = "src/RackCad.Application/Workspace/WorkspaceEditIntent.cs";

    private static string Core() => Src.Code(CorePath);

    private static string IntentBody() => Src.BodyOf(Core(), @"public\s+EditIntentOutcome\s+TryBeginPilotEdit\s*\(");

    private static string CheckBody() => Src.BodyOf(Core(), @"public\s+EditCheckOutcome\s+TryCheckPilotEdit\s*\(");

    // I-64 F4-T4A1 (D-T4A1-03): the three bodies of the explicit reconciliation of the host.
    private static string ConfirmBody() => Src.BodyOf(Core(), @"public\s+EditConfirmOutcome\s+TryConfirmPilotBase\s*\(");

    private static string KeepBody() => Src.BodyOf(Core(), @"public\s+EditIntentOutcome\s+TryKeepPilotEdit\s*\(");

    private static string DiscardAndReloadBody() => Src.BodyOf(Core(), @"public\s+EditIntentOutcome\s+TryDiscardAndReloadPilotEdit\s*\(");

    private static int IndexOf(string text, string needle) => text.IndexOf(needle, StringComparison.Ordinal);

    private static void AssertInOrder(string text, params string[] needles)
    {
        var previous = -1;
        foreach (var needle in needles)
        {
            var at = IndexOf(text, needle);
            Assert.True(at >= 0, "missing: " + needle);
            Assert.True(at > previous, "out of order: " + needle);
            previous = at;
        }
    }

    // ------------------------------------------------------------------ obligation 10: the base of a result only reaches Begin, CheckNow or DiscardAndReload

    // The ways to use the base or the read of a result of the pilot: the base, the initial state, the read itself and its entries.
    // M-2 (I-64 F4-T4A2): besides the form without spaces, the read through the name of the pilot is seen with a null-conditional or a null-forgiving
    // operator and with spaces or line breaks (pilot?.Read, pilot!.Read, pilot . Read). The alternative of result is kept as it was.
    private static readonly Regex BaseOrReadUse = new(@"\.StaleBase\b|\.InitialState\b|\b(?:pilot|full|result)\.Read\b|\b(?:pilot|full)\s*[?!]?\s*\.\s*Read\b|\.Entries\b");

    // The consumers that the obligation allows: the registry (Begin), the draft (CheckNow), the reload of F4-T2B2B and, since F4-T4A1
    // (D-T4A1-07), the general check (ConfirmBase) and the atomic keep (KeepMineOnCurrentVersion).
    private static readonly Regex AllowedConsumers = new(@"\.(?:Begin|DiscardAndReload|ConfirmBase|KeepMineOnCurrentVersion)\s*\([^;]*\)\s*;|\.CheckNow\s*\([^;]*\)\s*;");

    // The ONE other use of a read that the obligation allows, since F4-T4A1 (M-5): the diagnostic of the admission of a confirmed check, read
    // as pilot.Read.IsAdmitted and pilot.Read.Admission.Diagnostic and nothing else of the read. It is explicit and it is counted per
    // operation (only the bodies of the general check and, since F4-T4C2, of "check now" have it); no alias and no renamed variable stands for it.
    private const string AdmissionDiagnosticPattern = @"\bfull\.Read\.IsAdmitted\s*\?\s*null\s*:\s*full\.Read\.Admission\.Diagnostic\b";

    private static readonly Regex AllowedAdmissionDiagnostic = new(AdmissionDiagnosticPattern);

    private static string WithoutTheAllowedConsumers(string code) =>
        AllowedAdmissionDiagnostic.Replace(AllowedConsumers.Replace(code, string.Empty), string.Empty);

    // The read of a result of the pilot has ONE name in the host core, "pilot", and it is only ever assigned by one statement. So no alias
    // or renamed variable can stand for it and escape the guard of the use of the base (F4-T4A1, INV-F4T4A1-04).
    private static readonly Regex PilotReadAssignment = new(@"var\s+pilot\s*=\s*read\.PilotRead\s*;");

    // I-64 F4B PB-T4b: the full read of a result has ONE name in the host core, "full", assigned by one statement per operation that reads.
    private static readonly Regex FullReadAssignment = new(@"var\s+full\s*=\s*read\.FullRead\s*;");

    // M-3 (I-64 F4-T4A2), changed by F4-T4C2 (D-T4C-12, row 12): the expression of the diagnostic of the admission is in the whole host core exactly
    // twice, once in the body of the general check and once in the body of "check now", so no other operation and no helper can reach the
    // diagnostic of a read.
    private const string AnotherDiagnosticExpression = "\nvar again = full.Read.IsAdmitted ? null : full.Read.Admission.Diagnostic;";

    [Fact]
    public void TheBaseOfAResultOfThePilotOnlyReachesBeginCheckNowOrDiscardAndReloadInTheHostCoreAndTheTwoOperationsCountTheirOwnUses()
    {
        var core = Core();

        // Whatever the number of operations of the host, every use of the base or of the read of a result is an argument of a consumer.
        Assert.DoesNotMatch(BaseOrReadUse, WithoutTheAllowedConsumers(core));
        foreach (var code in new[] { Src.Code(IntentPath), Src.Code(InterfacePath) })
            Assert.DoesNotMatch(BaseOrReadUse, WithoutTheAllowedConsumers(code));

        // Per operation (these counts are about the two operations of this task, not about the whole file): the intention hands the
        // read to Begin and never the base to CheckNow, and "check now" hands the base to CheckNow and never the read to Begin.
        var intent = IntentBody();
        Assert.Equal(1, Src.Count(intent, @"\bfull\.Read\b"));
        Assert.Matches(@"\.Begin\s*\(\s*[^;]*?\bfull\.Read\b[^;]*?\)\s*;", intent);
        Assert.Equal(0, Src.Count(intent, @"\.StaleBase\b|\.CheckNow\s*\("));

        var check = CheckBody();
        Assert.Equal(1, Src.Count(check, @"\.StaleBase\b"));
        Assert.Matches(@"\.CheckNow\s*\(\s*\w+\.StaleBase\s*\)", check);
        // I-64 F4-T4C2 (D-T4C-12, row 12 of D-T4C-20): "check now" carries the diagnostic of the admission of the SAME reread, so the expression is in its
        // body exactly once and the read is named exactly twice, both inside that one expression. The reader is still called once.
        Assert.Equal(1, Src.Count(check, AdmissionDiagnosticPattern));
        Assert.Equal(2, Src.Count(check, @"\bfull\.Read\b"));
        Assert.Equal(0, Src.Count(WithoutTheAllowedConsumers(check), @"\bfull\.Read\b"));
        Assert.Equal(0, Src.Count(check, @"\.Begin\s*\("));
    }

    [Fact]
    public void ThePerOperationCountsOfTheBaseAndOfTheReadInTheThreeBodiesOfTheReconciliationAreTheirOwnConsumer()
    {
        var confirm = ConfirmBody();
        Assert.Equal(1, Src.Count(confirm, @"\.StaleBase\b"));
        Assert.Matches(@"\.ConfirmBase\s*\(\s*\w+\.StaleBase\s*\)", confirm);
        Assert.Equal(0, Src.Count(confirm, @"\.Begin\s*\(|\.CheckNow\s*\("));
        // The one use of the read in the general check is the diagnostic of the admission of a confirmed check (M-5), and it is counted:
        // the read is named exactly twice in the body (IsAdmitted and Admission.Diagnostic), both inside that one explicit expression.
        Assert.Equal(1, Src.Count(confirm, AdmissionDiagnosticPattern));
        Assert.Equal(2, Src.Count(confirm, @"\bfull\.Read\b"));
        Assert.Equal(0, Src.Count(WithoutTheAllowedConsumers(confirm), @"\bfull\.Read\b"));
        // I-64 F4-T4C2 (row 12, RR N-05): "check now" leaves the loop of zero and has exactly one; the other three bodies stay at zero.
        Assert.Equal(1, Src.Count(CheckBody(), AdmissionDiagnosticPattern));
        foreach (var other in new[] { IntentBody(), KeepBody(), DiscardAndReloadBody() })
            Assert.Equal(0, Src.Count(other, AdmissionDiagnosticPattern));

        var keep = KeepBody();
        Assert.Equal(1, Src.Count(keep, @"\bfull\.Read\b"));
        Assert.Matches(@"\.KeepMineOnCurrentVersion\s*\(\s*full\.Read\s*,\s*PushBackFullComposition\.KeepOn\s*,\s*confirmed\s*:\s*true\s*\)", keep);
        Assert.Equal(0, Src.Count(keep, @"\.StaleBase\b|\.InitialState\b|\.Begin\s*\("));

        var discard = DiscardAndReloadBody();
        Assert.Equal(1, Src.Count(discard, @"\bfull\.Read\b"));
        Assert.Matches(@"\.DiscardAndReload\s*\(\s*full\.Read\s*\)", discard);
        Assert.Equal(0, Src.Count(discard, @"\.StaleBase\b|\.InitialState\b|\.Begin\s*\("));

        // The host never names the initial state of a draft: the keep gets it through the draft and the function of the pilot.
        Assert.DoesNotMatch(new Regex(@"\.InitialState\b|\.InitialDraftState\b"), Core());
    }

    [Fact]
    public void TheResultOfThePilotIsOnlyEverNamedPilotInTheHostCoreSoNoAliasEscapesTheGuardOfTheUseOfTheBase()
    {
        var core = Core();
        var withoutTheAssignments = FullReadAssignment.Replace(PilotReadAssignment.Replace(core, string.Empty), string.Empty);

        // I-64 F4B PB-T4b: the full read is only ever named "full", once per operation that reads (the intention, the check, the reload,
        // the general check, the keep and the discard), and the read of the pilot only for the witness of the preview (four).
        Assert.DoesNotMatch(new Regex(@"\.PilotRead\b"), withoutTheAssignments);
        Assert.DoesNotMatch(new Regex(@"\.FullRead\b"), withoutTheAssignments);
        Assert.Equal(6, FullReadAssignment.Matches(core).Count);
        Assert.Equal(4, PilotReadAssignment.Matches(core).Count);
        Assert.Equal(6, Src.Count(core, @"\.ReadIntent\s*\("));
    }

    [Fact]
    public void TheThreeBodiesOfTheReconciliationFollowTheStepsOfTheCheckAndTheReloadWithTheApplicabilityBeforeTheOneRead()
    {
        var confirm = ConfirmBody();
        var keep = KeepBody();
        var discard = DiscardAndReloadBody();

        AssertInOrder(confirm, "string.IsNullOrWhiteSpace(request.RackId)", "HasWitnessWithoutKey(request)", "PassGate(", "_drafts.Find<", "NotCheckableReason", "_editIntentReader!.ReadIntent(", ".ConfirmBase(");
        AssertInOrder(keep, "string.IsNullOrWhiteSpace(request.RackId)", "HasWitnessWithoutKey(request)", "PassGate(", "_drafts.Find<", "DraftStatus.Applying", "NotKeepableReason", "_editIntentReader!.ReadIntent(", ".KeepMineOnCurrentVersion(");
        AssertInOrder(discard, "string.IsNullOrWhiteSpace(request.RackId)", "HasWitnessWithoutKey(request)", "PassGate(", "_drafts.Find<", "DraftStatus.Applying", "_editIntentReader!.ReadIntent(", ".DiscardAndReload(");
        foreach (var body in new[] { confirm, keep, discard })
        {
            Assert.Equal(1, Src.Count(body, @"\.ReadIntent\s*\("));
            Assert.Equal(1, Src.Count(body, @"_editIntentReader\s*[.!]"));
            // The mark of known clean is removed in the result that changed the draft, never before the read.
            Assert.True(IndexOf(body, "_knownClean.Remove(") > IndexOf(body, "_editIntentReader!.ReadIntent("));
        }

        Assert.Equal(1, Src.Count(confirm, @"_knownClean\.Remove\s*\("));
        Assert.Equal(1, Src.Count(keep, @"_knownClean\.Remove\s*\("));
        Assert.Equal(1, Src.Count(discard, @"_knownClean\.Remove\s*\("));
        // The keep is called with the function of the full model and the confirmation of the second "Si" with the witness.
        Assert.Contains("PushBackFullComposition.KeepOn", keep);
        Assert.Contains("confirmed: true", keep);
    }

    [Theory]
    [InlineData("draft.DiscardAndReload(full.Read);")]
    [InlineData("draft.DiscardAndReload(reload.Read, full.StaleBase);")]
    [InlineData("var check = draft.CheckNow(full.StaleBase);")]
    [InlineData("var begun = _drafts.Begin(session, rack, full.Read, comparer, copy);")]
    [InlineData("var confirmed = draft.ConfirmBase(full.StaleBase);")]
    [InlineData("var kept = draft.KeepMineOnCurrentVersion(full.Read, PushBackFullComposition.KeepOn, confirmed: true);")]
    [InlineData("return EditConfirmOutcome.Confirmed(full.Read.IsAdmitted ? null : full.Read.Admission.Diagnostic);")]
    public void TheBaseGuardAllowsTheConsumersOfTheObligation(string injected)
    {
        Assert.DoesNotMatch(BaseOrReadUse, WithoutTheAllowedConsumers(Core() + "\n" + injected));
    }

    [Fact]
    public void TheExpressionOfTheDiagnosticOfTheAdmissionIsInTheWholeHostCoreExactlyTwiceOnceInTheGeneralCheckAndOnceInCheckNow()
    {
        var core = Core();

        Assert.Equal(2, Src.Count(core, AdmissionDiagnosticPattern));
        Assert.Equal(1, Src.Count(ConfirmBody(), AdmissionDiagnosticPattern));
        Assert.Equal(1, Src.Count(CheckBody(), AdmissionDiagnosticPattern));
        foreach (var body in new[] { IntentBody(), KeepBody(), DiscardAndReloadBody() })
            Assert.Equal(0, Src.Count(body, AdmissionDiagnosticPattern));
    }

    [Fact]
    public void TheGuardOfTheExpressionOfTheDiagnosticSeesAThirdOneInTheCoreASecondOneInCheckNowAndOneInKeep()
    {
        var core = Core();

        // The third in the whole core.
        Assert.Equal(3, Src.Count(core + AnotherDiagnosticExpression, AdmissionDiagnosticPattern));
        // A second in the body of "check now" and one in the body of the keep: the per-body rules see them.
        Assert.Equal(2, Src.Count(CheckBody() + AnotherDiagnosticExpression, AdmissionDiagnosticPattern));
        Assert.Equal(1, Src.Count(KeepBody() + AnotherDiagnosticExpression, AdmissionDiagnosticPattern));
        // The expression itself is the one of the package: the same text, once more, is not allowed by the consumers.
        Assert.Matches(BaseOrReadUse, WithoutTheAllowedConsumers(core + "\nvar d = full.Read.Admission.Diagnostic;"));
    }
}
