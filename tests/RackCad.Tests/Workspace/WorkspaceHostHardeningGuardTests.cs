#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F4-T3C1-HOST-HARDENING (INV-F4T3C1-01, 03, 04, 05, 07, 08, 10, 12; section 4 of the package): the source guards of the hardening of the
// host of the pilot. The Core test project cannot reference the Plugin nor load WPF, so the sources are read as text. Each guard is a function of
// a text that returns its violations; it is run on the real source and, by the body of a member, on a synthetic text that it must accept and on
// mutations of it that it must reject. The catch blocks of GetRacksAtRisk and of the two queues are unreachable today (D-T3C1-01, D-T3C1-03), so
// these guards are the whole of their coverage: no seam, no conversion and no reflection reaches them.
public class WorkspaceHostHardeningGuardTests
{
    private const string PortPath = Src.ApplicationWorkspace + "IWorkspacePilotEditPort.cs";
    private const string HostPath = Src.ApplicationWorkspace + "WorkspaceHostCore.cs";
    private const string SupportPath = Src.ApplicationWorkspace + "WorkspacePilotSupport.cs";
    private const string UiWorkspace = "src/RackCad.UI/Workspace/";
    private const string BrowserPath = UiWorkspace + "WorkspaceBrowserViewModel.cs";
    private const string PanelPath = UiWorkspace + "WorkspacePanelViewModel.cs";
    private const string FieldPath = UiWorkspace + "WorkspacePilotFieldViewModel.cs";

    private static string Squash(string text) => Regex.Replace(text, @"\s+", string.Empty);

    private static int Count(string text, string pattern) => Regex.Matches(text, pattern).Count;

    private static int IndexOf(string text, string pattern)
    {
        var match = Regex.Match(text, pattern);
        return match.Success ? match.Index : -1;
    }

    // The text of a member from its signature to the closing brace, or null when the member does not exist.
    private static string? BodyOrNull(string code, string signature) => Regex.IsMatch(code, signature) ? Src.BodyOf(code, signature) : null;

    // ------------------------------------------------------------------ TakeLossNotices: each conversion in its own try

    private const string LossSignature = @"IReadOnlyList<PilotLossView>\s+IWorkspacePilotEditPort\.TakeLossNotices\s*\(\s*\)";

    private static IEnumerable<string> TakeLossViolations(string host)
    {
        var body = BodyOrNull(host, LossSignature);
        if (body == null)
        {
            yield return "TakeLossNotices does not exist";
            yield break;
        }

        if (Count(body, @"_lossNotices\.Dequeue\s*\(\s*\)") != 1) yield return "TakeLossNotices does not dequeue exactly once";
        if (Count(body, @"\btry\b") != 1) yield return "TakeLossNotices does not have exactly one try";
        if (Count(body, @"new\s+PilotLossView\s*\(") != 1) yield return "TakeLossNotices does not convert exactly once";

        var dequeue = IndexOf(body, @"_lossNotices\.Dequeue\s*\(");
        var tryAt = IndexOf(body, @"\btry\b");
        var conversion = IndexOf(body, @"new\s+PilotLossView\s*\(");
        var catchAt = IndexOf(body, @"\bcatch\b");
        // The notice leaves the queue before the try, so a conversion that throws never makes the loop go on forever; the conversion is inside.
        if (!(dequeue >= 0 && tryAt > dequeue)) yield return "the Dequeue is not before the try";
        if (!(conversion > tryAt && catchAt > conversion)) yield return "the conversion of the notice is not inside the try";

        if (!Regex.IsMatch(body, @"catch\s*\(\s*Exception\s+ex\s*\)\s*\{\s*Report\s*\(\s*""WorkspaceHostCore\.TakeLossNotices""\s*,\s*ex\s*\)\s*;\s*\}"))
            yield return "the catch of TakeLossNotices is not exactly Report and go on with the next notice";
        if (Regex.IsMatch(body, @"\bAuthorize\b|_faulted|\bContain\s*\(|\bthrow\b")) yield return "TakeLossNotices authorizes, degrades or rethrows";
    }

    private const string GoodLoss =
        "IReadOnlyList<PilotLossView> IWorkspacePilotEditPort.TakeLossNotices() { var taken = new List<PilotLossView>(); while (_lossNotices.Count > 0) "
        + "{ var notice = _lossNotices.Dequeue(); try { taken.Add(new PilotLossView(notice.Session, notice.LostRackIds, notice.UncertainRackIds, notice.UncertainReasons)); } "
        + "catch (Exception ex) { Report(\"WorkspaceHostCore.TakeLossNotices\", ex); } } return taken; }";

    [Fact]
    public void TakeLossNoticesConvertsEachNoticeInsideItsOwnTry()
    {
        Assert.Empty(TakeLossViolations(Src.Code(HostPath)));
        Assert.Empty(TakeLossViolations(GoodLoss));
    }

    [Theory]
    [InlineData("try { taken.Add(new PilotLossView(notice.Session, notice.LostRackIds, notice.UncertainRackIds, notice.UncertainReasons)); } catch (Exception ex) { Report(\"WorkspaceHostCore.TakeLossNotices\", ex); }",
        "taken.Add(new PilotLossView(notice.Session, notice.LostRackIds, notice.UncertainRackIds, notice.UncertainReasons));")]
    [InlineData("catch (Exception ex) { Report(\"WorkspaceHostCore.TakeLossNotices\", ex); }", "catch (Exception ex) { }")]
    [InlineData("catch (Exception ex) { Report(\"WorkspaceHostCore.TakeLossNotices\", ex); }", "catch (Exception ex) { Report(\"WorkspaceHostCore.TakeLossNotices\", ex); throw; }")]
    [InlineData("var notice = _lossNotices.Dequeue(); try {", "try { var notice = _lossNotices.Dequeue();")]
    [InlineData("var notice = _lossNotices.Dequeue();", "var notice = _lossNotices.Peek();")]
    [InlineData("var taken = new List<PilotLossView>();", "Authorize(null); var taken = new List<PilotLossView>();")]
    public void TheGuardOfTakeLossNoticesTripsOnAConversionOutsideTheTryAMissingReportAnEarlyDequeueOrAnAuthorization(string from, string to)
    {
        Assert.Contains(from, GoodLoss);

        Assert.NotEmpty(TakeLossViolations(GoodLoss.Replace(from, to, StringComparison.Ordinal)));
    }

    [Fact]
    public void TheGuardOfTakeLossNoticesTripsWhenTheMemberIsMissing()
    {
        Assert.NotEmpty(TakeLossViolations("class C { }"));
    }

    // ------------------------------------------------------------------ TakeCloseNotices: the whole body in one try

    private const string CloseSignature = @"IReadOnlyList<PilotCloseView>\s+IWorkspacePilotEditPort\.TakeCloseNotices\s*\(\s*\)";

    private static IEnumerable<string> TakeCloseViolations(string host)
    {
        var body = BodyOrNull(host, CloseSignature);
        if (body == null)
        {
            yield return "TakeCloseNotices does not exist";
            yield break;
        }

        if (Count(body, @"\btry\b") != 1) yield return "TakeCloseNotices does not have exactly one try";
        if (!Regex.IsMatch(body, @"^IReadOnlyList<PilotCloseView>\s+IWorkspacePilotEditPort\.TakeCloseNotices\s*\(\s*\)\s*\{\s*try\s*\{"))
            yield return "the body of TakeCloseNotices does not start with its try";
        if (!Regex.IsMatch(body, @"catch\s*\(\s*Exception\s+ex\s*\)\s*\{\s*Report\s*\(\s*""WorkspaceHostCore\.TakeCloseNotices""\s*,\s*ex\s*\)\s*;\s*return\s+Array\.Empty<PilotCloseView>\s*\(\s*\)\s*;\s*\}\s*\}\s*$"))
            yield return "the catch of TakeCloseNotices is not exactly Report and an empty list, at the end of the body";

        var catchAt = IndexOf(body, @"\bcatch\b");
        var clear = IndexOf(body, @"_closeNotices\.Clear\s*\(\s*\)");
        var copy = IndexOf(body, @"_closeNotices\.ToArray\s*\(\s*\)");
        // The queue is emptied inside the try, after the copy, and never in the catch: a failure leaves it to be taken again.
        if (!(copy >= 0 && clear > copy && catchAt > clear)) yield return "the copy and the Clear of the queue are not inside the try, in that order";
        if (Count(body, @"_closeNotices\.Clear\s*\(") != 1) yield return "TakeCloseNotices does not empty the queue exactly once";
        if (Regex.IsMatch(body, @"\bAuthorize\b|_faulted|\bContain\s*\(|\bthrow\b")) yield return "TakeCloseNotices authorizes, degrades or rethrows";
    }

    private const string GoodClose =
        "IReadOnlyList<PilotCloseView> IWorkspacePilotEditPort.TakeCloseNotices() { try { var taken = _closeNotices.ToArray(); _closeNotices.Clear(); return taken; } "
        + "catch (Exception ex) { Report(\"WorkspaceHostCore.TakeCloseNotices\", ex); return Array.Empty<PilotCloseView>(); } }";

    [Fact]
    public void TakeCloseNoticesHasItsWholeBodyInOneTry()
    {
        Assert.Empty(TakeCloseViolations(Src.Code(HostPath)));
        Assert.Empty(TakeCloseViolations(GoodClose));
    }

    [Theory]
    [InlineData("{ try { var taken", "{ var taken0 = 1; try { var taken")]
    [InlineData("Report(\"WorkspaceHostCore.TakeCloseNotices\", ex); return Array.Empty<PilotCloseView>();", "return Array.Empty<PilotCloseView>();")]
    [InlineData("return Array.Empty<PilotCloseView>(); } }", "_closeNotices.Clear(); return Array.Empty<PilotCloseView>(); } }")]
    [InlineData("return Array.Empty<PilotCloseView>(); } }", "throw; } }")]
    [InlineData("_closeNotices.Clear(); return taken;", "return taken;")]
    [InlineData("var taken = _closeNotices.ToArray();", "Authorize(null); var taken = _closeNotices.ToArray();")]
    public void TheGuardOfTakeCloseNoticesTripsWithoutTheTryWithoutTheReportOrWithAClearInTheCatch(string from, string to)
    {
        Assert.Contains(from, GoodClose);

        Assert.NotEmpty(TakeCloseViolations(GoodClose.Replace(from, to, StringComparison.Ordinal)));
    }

    [Fact]
    public void TheGuardOfTakeCloseNoticesTripsWhenTheMemberIsMissing()
    {
        Assert.NotEmpty(TakeCloseViolations("class C { }"));
    }

    // ------------------------------------------------------------------ SessionDestroyed: DropCloseNotice inside the first try, after the retirement

    private const string DestroyedSignature = @"void\s+IWorkspaceLifecycleAdvisor\.SessionDestroyed\s*\(";

    private static IEnumerable<string> SessionDestroyedViolations(string host)
    {
        var body = BodyOrNull(host, DestroyedSignature);
        if (body == null)
        {
            yield return "SessionDestroyed does not exist";
            yield break;
        }

        // The order of the two try of the member does not change (the guard of the lifecycle holds it too).
        if (Count(body, @"\btry\b") != 2) yield return "SessionDestroyed does not have exactly two try";
        if (Count(body, @"catch\s*\(\s*Exception\s+ex\s*\)") != 2) yield return "SessionDestroyed does not have exactly two catch (Exception ex)";

        var firstTry = IndexOf(body, @"\btry\b");
        var firstCatch = IndexOf(body, @"\bcatch\b");
        var remove = IndexOf(body, @"_knownClean\.Remove\s*\(\s*session\s*\)");
        if (Count(body, @"_knownClean\.Remove\s*\(") != 1 || remove < 0 || remove > firstTry) yield return "_knownClean.Remove is not before the first try";

        if (Count(body, @"\bDropCloseNotice\s*\(\s*session\s*\)") != 1)
        {
            yield return "SessionDestroyed does not drop the notice of veto exactly once";
            yield break;
        }

        var drop = IndexOf(body, @"\bDropCloseNotice\s*\(");
        var retire = IndexOf(body, @"_drafts\.RetireConservative\s*\(");
        if (!(firstTry >= 0 && retire > firstTry && drop > retire && firstCatch > drop))
            yield return "DropCloseNotice is not inside the first try, after the retirement";
    }

    private const string GoodDestroyed =
        "void IWorkspaceLifecycleAdvisor.SessionDestroyed(SessionId session, bool closeSequenceIntact) { var marked = _knownClean.Remove(session); DraftLossNotice? notice = null; "
        + "try { notice = marked && closeSequenceIntact ? _drafts.RetireKnownSafe(session) : _drafts.RetireConservative(session); DropCloseNotice(session); } "
        + "catch (Exception ex) { Report(\"a\", ex); } try { if (notice == null || !notice.HasLoss) return; _lossNotices.Enqueue(notice); } catch (Exception ex) { Report(\"b\", ex); } }";

    [Fact]
    public void DropCloseNoticeIsInsideTheFirstTryOfSessionDestroyedAfterTheRetirement()
    {
        Assert.Empty(SessionDestroyedViolations(Src.Code(HostPath)));
        Assert.Empty(SessionDestroyedViolations(GoodDestroyed));
    }

    [Fact]
    public void TheGuardOfSessionDestroyedTripsOnADropOutsideTheFirstTryBeforeTheRetirementRepeatedOrGone()
    {
        var withoutDrop = GoodDestroyed.Replace(" DropCloseNotice(session);", string.Empty, StringComparison.Ordinal);
        Assert.NotEqual(GoodDestroyed, withoutDrop);

        var mutants = new Dictionary<string, string>
        {
            ["before the first try"] = withoutDrop.Replace("DraftLossNotice? notice = null;", "DropCloseNotice(session); DraftLossNotice? notice = null;", StringComparison.Ordinal),
            ["before the retirement, inside the try"] = withoutDrop.Replace("try { notice = marked", "try { DropCloseNotice(session); notice = marked", StringComparison.Ordinal),
            ["after the first catch"] = withoutDrop.Replace("catch (Exception ex) { Report(\"a\", ex); }", "catch (Exception ex) { Report(\"a\", ex); } DropCloseNotice(session);", StringComparison.Ordinal),
            ["inside the first catch"] = withoutDrop.Replace("catch (Exception ex) { Report(\"a\", ex); }", "catch (Exception ex) { Report(\"a\", ex); DropCloseNotice(session); }", StringComparison.Ordinal),
            ["inside the second try"] = withoutDrop.Replace("_lossNotices.Enqueue(notice);", "_lossNotices.Enqueue(notice); DropCloseNotice(session);", StringComparison.Ordinal),
            ["twice"] = GoodDestroyed.Replace("DropCloseNotice(session); }", "DropCloseNotice(session); DropCloseNotice(session); }", StringComparison.Ordinal),
            ["gone"] = withoutDrop,
            ["the known clean mark moved inside the try"] = GoodDestroyed
                .Replace("var marked = _knownClean.Remove(session);", "var marked = true;", StringComparison.Ordinal)
                .Replace("try { notice = marked", "try { _knownClean.Remove(session); notice = marked", StringComparison.Ordinal),
            ["a third try"] = GoodDestroyed.Replace("Report(\"b\", ex); } }", "Report(\"b\", ex); } try { } catch (Exception ex) { Report(\"c\", ex); } }", StringComparison.Ordinal),
        };

        foreach (var (name, mutant) in mutants)
        {
            Assert.NotEqual(GoodDestroyed, mutant);
            Assert.True(SessionDestroyedViolations(mutant).Any(), "the guard does not see: " + name);
        }
    }

    [Fact]
    public void TheGuardOfSessionDestroyedTripsWhenTheMemberIsMissing()
    {
        Assert.NotEmpty(SessionDestroyedViolations("class C { }"));
    }

    // ------------------------------------------------------------------ GetRacksAtRisk: the outer catch is exactly Report and return default

    private const string RisksSignature = @"PilotRiskView\s+IWorkspacePilotEditPort\.GetRacksAtRisk\s*\(";

    private static IEnumerable<string> RisksViolations(string host)
    {
        var body = BodyOrNull(host, RisksSignature);
        if (body == null)
        {
            yield return "GetRacksAtRisk does not exist";
            yield break;
        }

        // The outer catch is the only one with a name for its exception: the inner ones decide draft by draft and say nothing.
        var outer = Regex.Matches(body, @"catch\s*\(\s*Exception\s+ex\s*\)");
        if (outer.Count != 1)
        {
            yield return "GetRacksAtRisk does not have exactly one outer catch (Exception ex)";
            yield break;
        }

        var catchBody = Src.BodyOf(body, @"catch\s*\(\s*Exception\s+ex\s*\)");
        if (Squash(catchBody) != "catch(Exceptionex){Report(\"WorkspaceHostCore.GetRacksAtRisk\",ex);returndefault;}")
            yield return "the outer catch of GetRacksAtRisk is not exactly Report and return default";
        if (Regex.IsMatch(body, @"_faulted|\bContain\s*\(|\bthrow\s*;|\bAuthorize\b")) yield return "GetRacksAtRisk degrades the session, rethrows or authorizes";
    }

    private const string GoodRisks =
        "PilotRiskView IWorkspacePilotEditPort.GetRacksAtRisk(WorkspaceRequest origin) { try { var session = origin.Origin; try { return new PilotRiskView(_drafts.RacksAtRiskOf(session), Array.Empty<string>()); } "
        + "catch (Exception) { } return new PilotRiskView(a, b); } catch (Exception ex) { Report(\"WorkspaceHostCore.GetRacksAtRisk\", ex); return default; } }";

    [Fact]
    public void TheOuterCatchOfGetRacksAtRiskIsExactlyReportAndReturnDefault()
    {
        Assert.Empty(RisksViolations(Src.Code(HostPath)));
        Assert.Empty(RisksViolations(GoodRisks));
    }

    [Theory]
    [InlineData("return default; }", "return new PilotRiskView(new string[0], new string[0]); }")]
    [InlineData("Report(\"WorkspaceHostCore.GetRacksAtRisk\", ex); return default;", "return default;")]
    [InlineData("Report(\"WorkspaceHostCore.GetRacksAtRisk\", ex); return default;", "Report(\"WorkspaceHostCore.GetRacksAtRisk\", ex); throw;")]
    [InlineData("Report(\"WorkspaceHostCore.GetRacksAtRisk\", ex); return default;", "Report(\"WorkspaceHostCore.GetRacksAtRisk\", ex); _faulted.Add(origin.Origin); return default;")]
    [InlineData("Report(\"WorkspaceHostCore.GetRacksAtRisk\", ex); return default;", "Report(\"WorkspaceHostCore.GetRacksAtRisk\", ex); return default; Cleanup();")]
    [InlineData("catch (Exception ex) { Report(\"WorkspaceHostCore.GetRacksAtRisk\", ex);", "catch (Exception ex) { Report(\"WorkspaceHostCore.Other\", ex);")]
    [InlineData("catch (Exception) { }", "catch (Exception ex) { }")]
    public void TheGuardOfGetRacksAtRiskTripsOnAnOuterCatchThatIsNotExactlyReportAndReturnDefault(string from, string to)
    {
        Assert.Contains(from, GoodRisks);

        Assert.NotEmpty(RisksViolations(GoodRisks.Replace(from, to, StringComparison.Ordinal)));
    }

    [Fact]
    public void TheGuardOfGetRacksAtRiskTripsWhenTheMemberIsMissing()
    {
        Assert.NotEmpty(RisksViolations("class C { }"));
    }

    // ------------------------------------------------------------------ PilotRiskView: _known is assigned only in the constructor

    private static IEnumerable<string> KnownViolations(string port)
    {
        var body = BodyOrNull(port, @"public\s+readonly\s+struct\s+PilotRiskView\b");
        if (body == null)
        {
            yield return "PilotRiskView does not exist";
            yield break;
        }

        if (Count(body, @"private\s+readonly\s+bool\s+_known\s*;") != 1) yield return "_known is not a private readonly bool declared once";
        if (Count(body, @"public\s+bool\s+IsKnown\s*=>\s*_known\s*;") != 1) yield return "IsKnown is not exactly => _known";

        var constructor = BodyOrNull(body, @"public\s+PilotRiskView\s*\(");
        if (constructor == null)
        {
            yield return "PilotRiskView has no constructor";
            yield break;
        }

        if (Count(body, @"public\s+PilotRiskView\s*\(") != 1) yield return "PilotRiskView has more than one constructor";
        var assignments = Count(body, @"\b_known\s*=(?!=|>)");
        if (assignments != 1) yield return "_known is assigned " + assignments + " times; it must be assigned exactly once";
        if (!Regex.IsMatch(constructor, @"\b_known\s*=\s*true\s*;")) yield return "the constructor does not set _known to true";
        if (Regex.IsMatch(body.Replace(constructor, string.Empty, StringComparison.Ordinal), @"\b_known\s*=(?!=|>)"))
            yield return "_known is assigned outside the constructor";
    }

    private const string GoodRisk =
        "public readonly struct PilotRiskView { private readonly bool _known; private readonly IReadOnlyList<string>? _rackIds; "
        + "public PilotRiskView(IReadOnlyList<string> rackIds, IReadOnlyList<string> uncertainRackIds) { _known = true; _rackIds = rackIds; } "
        + "public bool IsKnown => _known; public IReadOnlyList<string> RackIds => _rackIds ?? Array.Empty<string>(); }";

    [Fact]
    public void TheKnownMarkOfThePilotRiskViewIsAssignedOnlyInItsConstructor()
    {
        Assert.Empty(KnownViolations(Src.Code(PortPath)));
        Assert.Empty(KnownViolations(GoodRisk));
    }

    [Theory]
    [InlineData("_known = true;", "_known = false;")]
    [InlineData("_known = true;", "")]
    [InlineData("private readonly bool _known;", "private bool _known;")]
    [InlineData("public bool IsKnown => _known;", "public bool IsKnown => true;")]
    [InlineData("public bool IsKnown => _known;", "public bool IsKnown => _known; public void Forget() { _known = false; }")]
    [InlineData("_known = true; _rackIds = rackIds; }", "_known = true; _rackIds = rackIds; } public PilotRiskView(int other) { _known = true; }")]
    public void TheGuardOfTheKnownMarkTripsOnAnAssignmentOutsideTheConstructorOrAConstantFalse(string from, string to)
    {
        Assert.Contains(from, GoodRisk);

        Assert.NotEmpty(KnownViolations(GoodRisk.Replace(from, to, StringComparison.Ordinal)));
    }

    [Fact]
    public void TheGuardOfTheKnownMarkTripsWhenTheStructIsMissing()
    {
        Assert.NotEmpty(KnownViolations("class C { }"));
    }

    // ------------------------------------------------------------------ the two additions to the port: the optional ErrorReason and IsKnown, nothing else

    private static IEnumerable<string> PortAdditionViolations(string port)
    {
        if (Count(port, @"string\?\s+errorReason\s*=\s*null\s*\)\s*\{") != 1) yield return "the last parameter of PilotFieldView is not string? errorReason = null";
        if (Count(port, @"public\s+string\?\s+ErrorReason\s*\{\s*get;\s*\}") != 1) yield return "PilotFieldView has no string? ErrorReason";
        if (Count(port, @"\bErrorReason\s*=\s*errorReason\s*;") != 1) yield return "the constructor of PilotFieldView does not keep errorReason";
        if (Count(port, @"public\s+PilotFieldView\s*\(") != 1) yield return "PilotFieldView does not have exactly one constructor";
    }

    private const string GoodField =
        "public readonly struct PilotFieldView { public PilotFieldView(bool exists, string? pendingMessage, string? errorReason = null) { Exists = exists; ErrorReason = errorReason; } "
        + "public string? ErrorReason { get; } }";

    [Fact]
    public void PilotFieldViewGainsOnlyTheLastOptionalErrorReason()
    {
        Assert.Empty(PortAdditionViolations(Src.Code(PortPath)));
        Assert.Empty(PortAdditionViolations(GoodField));
    }

    [Theory]
    [InlineData("string? errorReason = null)", "string? errorReason)")]
    [InlineData("string? errorReason = null)", "string errorReason = null)")]
    [InlineData("public string? ErrorReason { get; }", "public string? ErrorReason { get; set; }")]
    [InlineData("ErrorReason = errorReason;", "ErrorReason = null;")]
    [InlineData("public readonly struct PilotFieldView {", "public readonly struct PilotFieldView { public PilotFieldView(bool a) { }")]
    public void TheGuardOfThePortAdditionTripsOnARequiredParameterAMutableOrUnkeptReasonOrASecondConstructor(string from, string to)
    {
        Assert.Contains(from, GoodField);

        Assert.NotEmpty(PortAdditionViolations(GoodField.Replace(from, to, StringComparison.Ordinal)));
    }

    // ------------------------------------------------------------------ H-07: the 28 literals of the port of the pilot are in Spanish

    // The table of the package (H-07): the literal in English, its Spanish text and how many times it stands in the host and the support.
    private static readonly (string English, string Spanish, int Times)[] Table =
    {
        ("A read of the pilot is in progress.", "Hay una lectura del piloto en curso.", 1),
        ("The request does not belong to the active document.", "La petición no es del documento activo.", 6),
        ("There is no draft for the rack.", "No hay borrador para el rack.", 3),
        ("The preview witness has no key.", "El testigo de la vista previa no tiene clave.", 3),
        ("The drawing is not ready to be read.", "El dibujo no está listo para leerse.", 3),
        ("The read of the edit intention failed.", "Falló la lectura de la intención de edición.", 1),
        ("The read of the edit intention carries no result.", "La lectura de la intención de edición no trajo resultado.", 1),
        ("The session was retired.", "La sesión se retiró.", 1),
        ("A blank rack has no draft.", "Un rack sin identificador no tiene borrador.", 1),
        ("The draft is not unknown after a commit that threw.", "El borrador no quedó desconocido tras una aplicación fallida.", 2),
        ("The reread failed.", "Falló la relectura.", 1),
        ("The reread carries no result.", "La relectura no trajo resultado.", 1),
        ("The draft does not need to be reloaded.", "El borrador no necesita recargarse.", 1),
        ("The read for the reload failed.", "Falló la lectura para la recarga.", 1),
        ("The read for the reload carries no result.", "La lectura para la recarga no trajo resultado.", 1),
        ("The draft is being applied or is no longer registered.", "El borrador se está aplicando o ya no está registrado.", 1),
        ("The draft was not reloaded.", "El borrador no se recargó.", 1),
        ("The state of the field could not be read.", "No se pudo leer el estado del campo.", 1),
        ("The text is required.", "El texto es obligatorio.", 1),
        ("The field cannot be edited now.", "El campo no se puede editar ahora.", 4),
        ("The boundary is not known.", "La frontera no se conoce.", 1),
        ("The rack has no annotation scale.", "El rack no tiene escala de anotación.", 1),
        ("No reader of the edit intention is assigned.", "No hay un lector de la intención de edición asignado.", 1),
        ("There is no active document.", "No hay documento activo.", 1),
        ("A RackCad window is open.", "Hay una ventana de RackCad abierta.", 1),
        ("A command is in progress.", "Hay un comando en curso.", 1),
        ("The value is not a finite number greater than zero.", "El valor no es un número finito mayor que cero.", 1),
        ("A notice of the panel is being delivered.", "Se está entregando un aviso del panel.", 1),
    };

    // The four identifiers that stay (two held by the guards of the port, two by decision) with the Spanish text each one carries.
    private static readonly (string Pattern, string Name)[] Identifiers =
    {
        (@"private\s+const\s+string\s+PilotReadInProgressReason\s*=\s*""Hay una lectura del piloto en curso\.""\s*;", "PilotReadInProgressReason"),
        (@"private\s+const\s+string\s+NotThisDocumentReason\s*=\s*""La petición no es del documento activo\.""\s*;", "NotThisDocumentReason"),
        (@"private\s+const\s+string\s+NoDraftReason\s*=\s*""No hay borrador para el rack\.""\s*;", "NoDraftReason"),
        (@"public\s+const\s+string\s+InsideNoticeReason\s*=\s*""Se está entregando un aviso del panel\.""\s*;", "InsideNoticeReason"),
    };

    // The substrings that the guards of the port forbid in these files (G-04), compared without case: the Spanish texts carry none.
    private static readonly Regex ForbiddenSubstrings = new(
        @"Transaction|Database|Commit\(|RackLog|Provider|Metric|LockDocument|Aggregat|\bBom|TotalRacks|ProjectSummary|ComputedParameter|proveedor|metrica|métrica",
        RegexOptions.IgnoreCase);

    private static IEnumerable<string> LiteralViolations(string host, string support)
    {
        var all = host + "\n" + support;
        if (Table.Length != 28) yield return "the table does not have its 28 literals";

        foreach (var (english, spanish, times) in Table)
        {
            if (all.Contains("\"" + english + "\"", StringComparison.Ordinal)) yield return "the literal in English stays: " + english;
            var found = Regex.Matches(all, Regex.Escape("\"" + spanish + "\"")).Count;
            if (found != times) yield return "the Spanish text of '" + english + "' stands " + found + " times; it must stand " + times;
            if (ForbiddenSubstrings.IsMatch(spanish)) yield return "the Spanish text carries a forbidden substring: " + spanish;
        }

        foreach (var (pattern, name) in Identifiers)
            if (Count(all, pattern) != 1) yield return "the identifier " + name + " is not preserved with its Spanish text";

        // They stay in English by decision: the text of the record and the one of the differences.
        if (!Regex.IsMatch(host, @"DescribeLoss[\s\S]*?Uncertain racks")) yield return "DescribeLoss changed";
        if (!host.Contains("(variable registry)", StringComparison.Ordinal)) yield return "DescribeDifferences changed";
    }

    [Fact]
    public void NoneOfThe28LiteralsInEnglishStaysInTheHostOrTheSupportAndTheFourIdentifiersAreKept()
    {
        Assert.Equal(28, Table.Length);
        Assert.Equal(28, Table.Select(t => t.English).Distinct().Count());
        Assert.Equal(28, Table.Select(t => t.Spanish).Distinct().Count());

        Assert.Empty(LiteralViolations(Src.Code(HostPath), Src.Code(SupportPath)));
    }

    [Fact]
    public void TheGuardOfTheLiteralsTripsOnAnEnglishLiteralAMissingSpanishTextAndALostIdentifier()
    {
        var host = Src.Code(HostPath);
        var support = Src.Code(SupportPath);

        // An English literal that comes back, in the host and in the support.
        Assert.NotEmpty(LiteralViolations(host + " \"The reread failed.\"", support));
        Assert.NotEmpty(LiteralViolations(host, support + " \"A notice of the panel is being delivered.\""));
        // A Spanish text that goes away (here, replaced by another), and one that is repeated.
        Assert.NotEmpty(LiteralViolations(host.Replace("\"Falló la relectura.\"", "\"Otra cosa.\"", StringComparison.Ordinal), support));
        Assert.NotEmpty(LiteralViolations(host + " \"Falló la relectura.\"", support));
        // A lost identifier.
        Assert.NotEmpty(LiteralViolations(host.Replace("NoDraftReason", "NoBorradorReason", StringComparison.Ordinal), support));
        Assert.NotEmpty(LiteralViolations(host, support.Replace("InsideNoticeReason", "DentroDelAvisoReason", StringComparison.Ordinal)));
        // The text of the record or of the differences changes.
        Assert.NotEmpty(LiteralViolations(host.Replace("(variable registry)", "(registro de variables)", StringComparison.Ordinal), support));
        Assert.NotEmpty(LiteralViolations(host.Replace("Uncertain racks", "Racks inciertos", StringComparison.Ordinal), support));
    }

    [Theory]
    [InlineData("El texto de la métrica.")]
    [InlineData("El proveedor falló.")]
    [InlineData("La Transaction falló.")]
    [InlineData("Falló el Commit(")]
    public void TheForbiddenSubstringsOfThePortTripOnASpanishTextThatCarriesOne(string text)
    {
        Assert.Matches(ForbiddenSubstrings, text);
    }

    // ------------------------------------------------------------------ the UI: consumers of the unknown, M-1, M-2 and M-3

    private static IEnumerable<string> UnknownConsumerViolations(string browser)
    {
        foreach (var member in new[] { "RequestDocumentDiscard", "ConfirmDiscard" })
        {
            var body = BodyOrNull(browser, @"public\s+void\s+" + member + @"\s*\(\s*\)");
            if (body == null)
            {
                yield return member + " does not exist";
                continue;
            }

            var known = IndexOf(body, @"!\s*\w+\.IsKnown\b");
            var noRisk = IndexOf(body, @"\bNoRiskText\b");
            if (known < 0) yield return member + " does not look at IsKnown";
            else if (noRisk >= 0 && known > noRisk) yield return member + " says nothing is at risk before it looks at IsKnown";
            if (Count(body, @"\.IsKnown\b") != 1) yield return member + " does not look at IsKnown exactly once";
        }
    }

    private const string GoodBrowser =
        "public sealed class WorkspaceBrowserViewModel { "
        + "public void RequestDocumentDiscard() { var risks = _p.GetRacksAtRisk(o); if (!risks.IsKnown) { PostNotice(UnknownRiskText); return; } if (risks.RackIds.Count == 0) { PostNotice(NoRiskText); return; } } "
        + "public void ConfirmDiscard() { var again = _p.GetRacksAtRisk(o); if (!again.IsKnown) { RemoveConfirmation(); PostNotice(UnknownRiskAgainText); return; } if (again.RackIds.Count == 0) { PostNotice(NoRiskText); } } "
        + "public void CloseTab(string rackId) { try { var outcome = _p.DiscardRack(o, rackId); } catch (Exception ex) { RemoveConfirmation(); notice = CleanDraftKeptText(ReasonOf(ex)); } CloseNow(rackId, notice); } "
        + "private string ApplySelection(SelectionContext context) { var couldNotCheck = false; var keepActive = isNew && !string.IsNullOrWhiteSpace(rack) && KeepsActive(id, active, out couldNotCheck); return null; } "
        + "private bool KeepsActive(SessionId session, string active, out bool couldNotCheck) { couldNotCheck = false; if (_pilotEdit == null) return false; try { return true; } catch (Exception) { couldNotCheck = true; return true; } } }";

    [Fact]
    public void TheConsumersOfTheRiskLookAtIsKnownBeforeTheySayThatNothingIsAtRisk()
    {
        Assert.Empty(UnknownConsumerViolations(Src.Code(BrowserPath)));
        Assert.Empty(UnknownConsumerViolations(GoodBrowser));
    }

    [Theory]
    [InlineData("if (!risks.IsKnown) { PostNotice(UnknownRiskText); return; } if (risks.RackIds.Count == 0) { PostNotice(NoRiskText); return; }", "if (risks.RackIds.Count == 0) { PostNotice(NoRiskText); return; }")]
    [InlineData("if (!risks.IsKnown) { PostNotice(UnknownRiskText); return; } if (risks.RackIds.Count == 0) { PostNotice(NoRiskText); return; }", "if (risks.RackIds.Count == 0) { PostNotice(NoRiskText); return; } if (!risks.IsKnown) { PostNotice(UnknownRiskText); return; }")]
    [InlineData("if (!again.IsKnown) { RemoveConfirmation(); PostNotice(UnknownRiskAgainText); return; }", "")]
    [InlineData("if (!again.IsKnown) {", "if (!again.IsKnown) { if (!again.IsKnown) { } ")]
    public void TheGuardOfTheConsumersOfTheRiskTripsWithoutTheLookOrWithTheLookAfterNoRisk(string from, string to)
    {
        Assert.Contains(from, GoodBrowser);

        Assert.NotEmpty(UnknownConsumerViolations(GoodBrowser.Replace(from, to, StringComparison.Ordinal)));
    }

    private static IEnumerable<string> RetentionViolations(string browser)
    {
        var keeps = BodyOrNull(browser, @"private\s+bool\s+KeepsActive\s*\(\s*SessionId\s+\w+\s*,\s*string\s+\w+\s*,\s*out\s+bool\s+couldNotCheck\s*\)");
        if (keeps == null) yield return "KeepsActive does not have its out bool couldNotCheck";
        else
        {
            var assign = IndexOf(keeps, @"\bcouldNotCheck\s*=\s*false\s*;");
            var tryAt = IndexOf(keeps, @"\btry\b");
            if (assign < 0 || tryAt < assign) yield return "KeepsActive does not assign couldNotCheck before anything can end it";
            if (!Regex.IsMatch(keeps, @"catch\s*\(\s*Exception\s*\)\s*\{\s*couldNotCheck\s*=\s*true\s*;\s*return\s+true\s*;\s*\}"))
                yield return "KeepsActive does not set couldNotCheck in the catch that keeps the tab";
        }

        var apply = BodyOrNull(browser, @"private\s+string\s+ApplySelection\s*\(");
        if (apply == null) yield return "ApplySelection does not exist";
        else
        {
            var declared = IndexOf(apply, @"\bvar\s+couldNotCheck\s*=\s*false\s*;");
            var chain = IndexOf(apply, @"\bvar\s+keepActive\b");
            if (declared < 0 || chain < declared) yield return "couldNotCheck is not assigned before the chain of && of ApplySelection";
            if (Count(apply, @"\bKeepsActive\s*\([^)]*\bout\s+couldNotCheck\s*\)") != 1) yield return "ApplySelection does not call KeepsActive with out couldNotCheck exactly once";
        }
    }

    [Fact]
    public void TheRetentionGivesTheFlagBeforeTheChainOfAndAndSetsItOnlyInTheCatch()
    {
        Assert.Empty(RetentionViolations(Src.Code(BrowserPath)));
        Assert.Empty(RetentionViolations(GoodBrowser));
    }

    [Theory]
    [InlineData("var couldNotCheck = false; var keepActive", "var keepActive")]
    [InlineData("couldNotCheck = false; if (_pilotEdit == null) return false; try", "if (_pilotEdit == null) return false; try")]
    [InlineData("catch (Exception) { couldNotCheck = true; return true; }", "catch (Exception) { return true; }")]
    [InlineData("KeepsActive(id, active, out couldNotCheck)", "KeepsActive(id, active)")]
    [InlineData("out bool couldNotCheck) {", "out bool unchecked0) {")]
    public void TheGuardOfTheRetentionTripsWithoutTheFlagAssignedBeforeTheChainOrWithoutSettingItInTheCatch(string from, string to)
    {
        Assert.Contains(from, GoodBrowser);

        Assert.NotEmpty(RetentionViolations(GoodBrowser.Replace(from, to, StringComparison.Ordinal)));
    }

    private static IEnumerable<string> CleanCloseViolations(string browser)
    {
        var body = BodyOrNull(browser, @"public\s+void\s+CloseTab\s*\(\s*string\s+rackId\s*\)");
        if (body == null)
        {
            yield return "CloseTab does not exist";
            yield break;
        }

        if (!Regex.IsMatch(body, @"\.DiscardRack\s*\([\s\S]*?catch\s*\(\s*Exception\s+ex\s*\)\s*\{\s*RemoveConfirmation\s*\(\s*\)\s*;"))
            yield return "the catch of the clean branch of CloseTab does not remove the confirmation first";
    }

    [Fact]
    public void TheCatchOfTheCleanBranchOfCloseTabRemovesAPendingConfirmationBeforeItCloses()
    {
        Assert.Empty(CleanCloseViolations(Src.Code(BrowserPath)));
        Assert.Empty(CleanCloseViolations(GoodBrowser));
        Assert.NotEmpty(CleanCloseViolations(GoodBrowser.Replace("catch (Exception ex) { RemoveConfirmation(); notice", "catch (Exception ex) { notice", StringComparison.Ordinal)));
        Assert.NotEmpty(CleanCloseViolations(GoodBrowser.Replace("catch (Exception ex) { RemoveConfirmation(); notice", "catch (Exception ex) { notice = 1; RemoveConfirmation(); notice", StringComparison.Ordinal)));
        Assert.NotEmpty(CleanCloseViolations("class C { }"));
    }

    private static IEnumerable<string> PanelConstructorViolations(string panel)
    {
        var body = BodyOrNull(panel, @"public\s+WorkspacePanelViewModel\s*\(");
        if (body == null)
        {
            yield return "the constructor of the panel does not exist";
            yield break;
        }

        if (Count(panel, @"\bPresent\s*\(\s*InitialSnapshot\s*\(\s*\)\s*,\s*null\s*\)") != 1) yield return "the panel does not present its initial snapshot exactly once";
        if (!Regex.IsMatch(body, @"try\s*\{\s*Present\s*\(\s*InitialSnapshot\s*\(\s*\)\s*,\s*null\s*\)\s*;\s*\}\s*catch\s*\(\s*Exception\s+ex\s*\)\s*\{\s*EnterError\s*\(\s*ex\s*\)\s*;\s*\}"))
            yield return "the first presentation of the panel is not inside a try whose catch is the contained Error";
        if (Regex.IsMatch(panel, @"\bIsSessionAlive\b")) yield return "the panel view-model asks whether a session lives";
    }

    private const string GoodPanel =
        "public sealed class WorkspacePanelViewModel { public WorkspacePanelViewModel(IWorkspaceHostPort port) { Browser = new X(); "
        + "try { Present(InitialSnapshot(), null); } catch (Exception ex) { EnterError(ex); } } }";

    [Fact]
    public void TheConstructorOfThePanelContainsItsFirstPresentationAsTheErrorOfThePanel()
    {
        Assert.Empty(PanelConstructorViolations(Src.Code(PanelPath)));
        Assert.Empty(PanelConstructorViolations(GoodPanel));
    }

    [Theory]
    [InlineData("try { Present(InitialSnapshot(), null); } catch (Exception ex) { EnterError(ex); }", "Present(InitialSnapshot(), null);")]
    [InlineData("catch (Exception ex) { EnterError(ex); }", "catch (Exception ex) { }")]
    [InlineData("catch (Exception ex) { EnterError(ex); }", "catch (Exception ex) { throw; }")]
    [InlineData("Browser = new X();", "Browser = new X(); Present(InitialSnapshot(), null);")]
    [InlineData("Browser = new X();", "Browser = new X(); _port.IsSessionAlive(s);")]
    public void TheGuardOfTheConstructorOfThePanelTripsWithoutTheTryOrWithoutTheContainedError(string from, string to)
    {
        Assert.Contains(from, GoodPanel);

        Assert.NotEmpty(PanelConstructorViolations(GoodPanel.Replace(from, to, StringComparison.Ordinal)));
    }

    // ------------------------------------------------------------------ the limits of the task: no new mechanism, no reflection

    private static readonly Regex NewMechanisms = new(
        @"\+=|-=|\bevent\b|\bdelegate\b|\bDispatcher\w*|\w*Timer\b|\bTask\b|\basync\b|\bawait\b|\bKeyboard\b|\.Focus\s*\(|FocusManager",
        RegexOptions.CultureInvariant);

    private static readonly Regex Reflection = new(
        @"System\.Reflection|\bBindingFlags\b|\bActivator\b|\bMethodInfo\b|\bFieldInfo\b|\bPropertyInfo\b|InternalsVisibleTo",
        RegexOptions.CultureInvariant);

    private static IEnumerable<string> LimitViolations(IEnumerable<(string Path, string Code)> files, bool uiMechanisms)
    {
        foreach (var (path, code) in files)
        {
            var reflection = Reflection.Match(code);
            if (reflection.Success) yield return path + " uses reflection: " + reflection.Value;
            if (!uiMechanisms) continue;
            var mechanism = NewMechanisms.Match(code);
            if (mechanism.Success) yield return path + " adds a mechanism: " + mechanism.Value;
        }
    }

    [Fact]
    public void TheThreeFilesOfTheHostAndTheThreeViewModelsUseNoReflectionAndTheViewModelsAddNoSubscriptionEventTimerTaskOrFocus()
    {
        Assert.Empty(LimitViolations(new[] { PortPath, HostPath, SupportPath }.Select(p => (p, Src.Code(p))), false));
        Assert.Empty(LimitViolations(new[] { BrowserPath, PanelPath, FieldPath }.Select(p => (p, Src.Code(p))), true));
    }

    [Theory]
    [InlineData("var a = b; a += c;", true)]
    [InlineData("x.Changed -= Handler;", true)]
    [InlineData("public event Action Changed;", true)]
    [InlineData("var d = Dispatcher.CurrentDispatcher;", true)]
    [InlineData("var t = new DispatcherTimer();", true)]
    [InlineData("Task.Run(() => 1);", true)]
    [InlineData("public async void Go() { await x; }", true)]
    [InlineData("Keyboard.Focus(box);", true)]
    [InlineData("box.Focus();", true)]
    [InlineData("FocusManager.SetFocusedElement(a, b);", true)]
    [InlineData("var info = typeof(X).GetField(\"a\", BindingFlags.NonPublic);", false)]
    [InlineData("using System.Reflection;", false)]
    [InlineData("var m = (MethodInfo)null;", false)]
    public void TheGuardOfTheLimitsTripsOnASubscriptionAnEventATimerATaskTheFocusAndReflection(string code, bool onlyForTheViewModels)
    {
        // A mechanism of the UI is a violation of the three view-models only; reflection is one of the six files.
        Assert.NotEmpty(LimitViolations(new[] { ("a.cs", code) }, uiMechanisms: true));
        if (onlyForTheViewModels) Assert.Empty(LimitViolations(new[] { ("a.cs", code) }, uiMechanisms: false));
        else Assert.NotEmpty(LimitViolations(new[] { ("a.cs", code) }, uiMechanisms: false));
    }

    [Fact]
    public void TheGuardOfTheLimitsAcceptsTheTextsThatTheTaskWrites()
    {
        Assert.Empty(LimitViolations(new[] { ("a.cs", "text = text + \" \" + other; var s = _port.IsSessionAlive(x);") }, uiMechanisms: true));
        Assert.Empty(LimitViolations(new[] { ("a.cs", "catch (Exception ex) { Report(\"a\", ex); return default; }") }, uiMechanisms: false));
    }
}
