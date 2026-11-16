#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.Workspace;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F4-T4-HOST-C1 (D-C1-02, D-C1-09 test 5, INV-C1-02, INV-C1-04, IC-10): the source guard of the bridge for the object events. The Core test project
// cannot reference the Plugin (it needs AutoCAD), so the bridge is read as text, without its comments. The five handlers of the object events are
// exactly one form, and the classifier of the object of the event is only type tests of the argument: it opens nothing and reads nothing. The handlers
// of the other sources are exactly what they were. Every guard is followed by the proof that it trips on the real text with ONE injected change.
public class WorkspaceBridgeObjectSubjectGuardTests
{
    private const string BridgePath = "src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs";

    private static string Bridge() => Src.Code(BridgePath);

    // The five sources of the object events with the type of the handler of each one.
    private static readonly (BridgeSource Source, string HandlerType)[] ObjectBlocks =
    {
        (BridgeSource.ObjectAppended, "ObjectEventHandler"),
        (BridgeSource.ObjectErased, "ObjectErasedEventHandler"),
        (BridgeSource.ObjectModified, "ObjectEventHandler"),
        (BridgeSource.ObjectUnappended, "ObjectEventHandler"),
        (BridgeSource.ObjectReappended, "ObjectEventHandler"),
    };

    private const string ObjectBody = "ContainSession(a, () => _core.OnObjectEvent(a.Session, a.Source, SubjectOf(e.DBObject)));";

    private const string SourceBody = "ContainSession(a, () => _core.OnSourceEvent(a.Session, a.Source));";

    private static string Squash(string text) => Regex.Replace(text, @"\s+", string.Empty);

    private static IReadOnlyDictionary<string, string> CaseBlocks(string bridge)
    {
        var hook = Src.BodyOf(bridge, @"private\s+Action\s+Hook\s*\(");
        var starts = Regex.Matches(hook, @"\bcase\s+BridgeSource\.(\w+)\s*:").Cast<Match>().ToList();
        var defaultAt = hook.IndexOf("default:", StringComparison.Ordinal);
        var blocks = new Dictionary<string, string>(StringComparer.Ordinal);
        for (var i = 0; i < starts.Count; i++)
        {
            var end = i + 1 < starts.Count ? starts[i + 1].Index : (defaultAt > starts[i].Index ? defaultAt : hook.Length);
            blocks[starts[i].Groups[1].Value] = hook.Substring(starts[i].Index, end - starts[i].Index);
        }

        return blocks;
    }

    private static string Block(string bridge, string source)
    {
        var blocks = CaseBlocks(bridge);
        Assert.True(blocks.ContainsKey(source), "no case block for " + source);
        return blocks[source];
    }

    // The four sources whose handlers are exactly the line of OnSourceEvent, and the three that carry more than that line.
    private static readonly (string Source, string HandlerType)[] SourceEventBlocks =
    {
        ("ImpliedSelectionChanged", "EventHandler"),
        ("CommandEnded", "CommandEventHandler"),
        ("CommandCancelled", "CommandEventHandler"),
        ("CommandFailed", "CommandEventHandler"),
    };

    private static IEnumerable<string> Violations(string bridge)
    {
        var all = Squash(bridge);

        // 1. The five blocks of the object events: exactly one handler, of that form, and nothing of OnSourceEvent.
        foreach (var (source, handlerType) in ObjectBlocks)
        {
            var raw = Block(bridge, source.ToString());
            var block = Squash(raw);
            var expected = Squash(handlerType + " handler = (s, e) => " + ObjectBody);
            if (Regex.Matches(block, Regex.Escape(expected)).Count != 1) yield return source + " does not have its handler exactly as " + handlerType + " handler = (s, e) => " + ObjectBody;
            if (block.Contains("OnSourceEvent", StringComparison.Ordinal)) yield return source + " still names OnSourceEvent";
            if (Regex.Matches(raw, @"\bhandler\s*=\s*\(").Count != 1) yield return source + " does not have exactly one handler";
            if (Regex.Matches(raw, @"\+=").Count != 1 || Regex.Matches(raw, @"-=").Count != 1) yield return source + " does not have exactly one hookup and one undoing";
        }

        // 2. The classifier is only type tests of the argument, in memory, and its name appears in the five handlers and nowhere else.
        if (Regex.Matches(bridge, @"\bSubjectOf\b").Count != 6) yield return "SubjectOf is not named exactly six times (its declaration and the five handlers)";
        if (Regex.Matches(bridge, @"\be\.DBObject\b").Count != 5) yield return "e.DBObject is not named exactly five times, once per handler";
        if (!Regex.IsMatch(bridge, @"\bprivate\s+static\s+ObjectEventSubject\s+SubjectOf\s*\(\s*DBObject\s+(\w+)\s*\)\s*=>\s*\1\s+is\s+AssocAction\s*\|\|\s*\1\s+is\s+AssocDependency\s*\?\s*ObjectEventSubject\.AssociativeBookkeeping\s*:\s*ObjectEventSubject\.Drawing\s*;"))
            yield return "SubjectOf is not exactly private static, with two type tests (AssocAction, AssocDependency) and the two subjects";
        if (Regex.IsMatch(bridge, @"\bObjectEventSubject\s+SubjectOf\s*\("))
        {
            // Whatever the form, the body is read apart: only the two type tests, no opening of the object and no read of anything of it.
            var subject = Src.BodyOf(bridge, @"\bObjectEventSubject\s+SubjectOf\s*\(");
            if (Regex.Matches(subject, @"\bis\s+Assoc(?:Action|Dependency)\b").Count != 2) yield return "SubjectOf does not test exactly AssocAction and AssocDependency";
            foreach (Match match in Regex.Matches(subject, @"\bHandle\b|\bObjectId\b|\.\s*Id\b|\.\s*Database\b|GetObject|OpenMode|\.\s*Read\w*\s*\(|\.\s*GetType\s*\(|\bas\s+\w|\(\s*Assoc\w+\s*\)"))
                yield return "SubjectOf names " + match.Value;
            if (Regex.Matches(subject, @"\.").Count != 2) yield return "SubjectOf reads more than the two subjects of the enumeration";
        }

        // 3. The other handlers are what they were: the line of OnSourceEvent, the two that decide a notice of the command state, and the Idle.
        foreach (var (source, handlerType) in SourceEventBlocks)
        {
            var block = Squash(Block(bridge, source));
            if (Regex.Matches(block, Regex.Escape(Squash(handlerType + " handler = (s, e) => " + SourceBody))).Count != 1) yield return source + " does not have its handler exactly as " + handlerType + " handler = (s, e) => " + SourceBody;
            foreach (var banned in new[] { "OnObjectEvent", "SubjectOf", "DBObject" })
                if (block.Contains(banned, StringComparison.Ordinal)) yield return source + " names " + banned;
        }

        var start = Squash(Block(bridge, "CommandWillStart"));
        if (!start.Contains(Squash("if (_core.OnSourceEvent(a.Session, a.Source)) RaiseCommandStateChanged();"), StringComparison.Ordinal)) yield return "CommandWillStart does not decide with OnSourceEvent as before";
        if (!start.Contains(Squash("_core.NoteForeignCommandStart(a.Session);"), StringComparison.Ordinal)) yield return "CommandWillStart does not count the foreign commands as before";
        var quiescent = Squash(Block(bridge, "EnteringQuiescentState"));
        if (!quiescent.Contains(Squash("ContainSession(a, () => { if (_core.OnSourceEvent(a.Session, a.Source)) RaiseCommandStateChanged(); });"), StringComparison.Ordinal)) yield return "EnteringQuiescentState does not decide with OnSourceEvent as before";
        foreach (var source in new[] { "CommandWillStart", "EnteringQuiescentState", "Idle", "DocumentCreated", "DocumentActivated", "DocumentToBeDestroyed", "BeginDocumentClose" })
            foreach (var banned in new[] { "OnObjectEvent", "SubjectOf", "DBObject" })
                if (Block(bridge, source).Contains(banned, StringComparison.Ordinal)) yield return source + " names " + banned;
        var idle = Squash(Block(bridge, "Idle"));
        if (!idle.Contains(Squash("EventHandler handler = (s, e) => Contain(\"Idle\", OnIdleTick);"), StringComparison.Ordinal)) yield return "the Idle handler is not Contain(\"Idle\", OnIdleTick) as before";

        // The classification adds no hookup, no continuation and no read: the 16 hookups and the 16 undoings are the same, and OnSourceEvent has its six uses.
        if (Regex.Matches(bridge, @"\+=").Count != 16) yield return "the bridge does not have exactly 16 hookups";
        if (Regex.Matches(bridge, @"-=").Count != 16) yield return "the bridge does not have exactly 16 undoings";
        if (Regex.Matches(bridge, @"_core\.OnSourceEvent\s*\(").Count != 6) yield return "the bridge does not call OnSourceEvent exactly six times (the selection, the start, the three ends and the rest)";
        if (Regex.Matches(bridge, @"_core\.OnObjectEvent\s*\(").Count != 5) yield return "the bridge does not call OnObjectEvent exactly five times";
        if (Regex.IsMatch(bridge, @"\bRegen\b|\bRegenerate\b|\.Regen\s*\(")) yield return "the bridge regenerates";

        // The containment is the one of the other handlers: the classifier is evaluated INSIDE ContainSession, whose catch degrades the session.
        var contain = Src.BodyOf(bridge, @"private\s+void\s+ContainSession\s*\(");
        if (!Regex.IsMatch(contain, @"catch\s*\(\s*System\.Exception\s+ex\s*\)[\s\S]*_core\.ReportFault\s*\(\s*attachment\.Session")) yield return "ContainSession no longer reports the fault of the session";
        if (!all.Contains("privatevoidContainSession(SourceAttachmentattachment,Actionbody)", StringComparison.Ordinal)) yield return "ContainSession changed its form";
    }

    private static string Mutate(string code, string from, string to)
    {
        Assert.Contains(from, code, StringComparison.Ordinal);
        return code.Replace(from, to, StringComparison.Ordinal);
    }

    // The text of the real handler of one source, to change ONE of them.
    private static string ReplaceInBlock(string bridge, string source, string from, string to)
    {
        var block = Block(bridge, source);
        Assert.Contains(from, block, StringComparison.Ordinal);
        return Mutate(bridge, block, block.Replace(from, to, StringComparison.Ordinal));
    }

    [Fact]
    public void TheFiveObjectHandlersAreExactlyOneFormAndTheClassifierIsOnlyTypeTests()
    {
        Assert.Empty(Violations(Bridge()));
    }

    [Theory]
    [InlineData("ObjectAppended")]
    [InlineData("ObjectErased")]
    [InlineData("ObjectModified")]
    [InlineData("ObjectUnappended")]
    [InlineData("ObjectReappended")]
    public void TheGuardTripsOnAnObjectHandlerThatReturnsToOnSourceEventThatDropsTheSubjectOrThatEvaluatesItOutsideTheContainment(string source)
    {
        var real = Bridge();
        Assert.Empty(Violations(real));

        Assert.NotEmpty(Violations(ReplaceInBlock(real, source, "_core.OnObjectEvent(a.Session, a.Source, SubjectOf(e.DBObject))", "_core.OnSourceEvent(a.Session, a.Source)")));
        Assert.NotEmpty(Violations(ReplaceInBlock(real, source, "SubjectOf(e.DBObject)", "ObjectEventSubject.Drawing")));
        Assert.NotEmpty(Violations(ReplaceInBlock(real, source, "ContainSession(a, () => _core.OnObjectEvent(a.Session, a.Source, SubjectOf(e.DBObject)))", "_core.OnObjectEvent(a.Session, a.Source, SubjectOf(e.DBObject))")));
        Assert.NotEmpty(Violations(ReplaceInBlock(real, source, "(s, e) => ContainSession", "(s, e) => Contain(\"" + source + "\", () => { }); ContainSession")));
        Assert.NotEmpty(Violations(ReplaceInBlock(real, source, "SubjectOf(e.DBObject)", "SubjectOf(e.DBObject.Database)")));
    }

    [Theory]
    [InlineData("o.Handle != default ? ObjectEventSubject.Drawing : ObjectEventSubject.Drawing")]
    [InlineData("o.ObjectId.IsNull ? ObjectEventSubject.Drawing : ObjectEventSubject.Drawing")]
    [InlineData("o.Id.IsNull ? ObjectEventSubject.Drawing : ObjectEventSubject.Drawing")]
    [InlineData("o.Database == null ? ObjectEventSubject.Drawing : ObjectEventSubject.Drawing")]
    [InlineData("o.GetObject(OpenMode.ForRead) is AssocAction ? ObjectEventSubject.AssociativeBookkeeping : ObjectEventSubject.Drawing")]
    [InlineData("o.ReadX() is AssocAction ? ObjectEventSubject.AssociativeBookkeeping : ObjectEventSubject.Drawing")]
    [InlineData("o is AssocAction || o is AssocDependency || o is AssocActionBody ? ObjectEventSubject.AssociativeBookkeeping : ObjectEventSubject.Drawing")]
    [InlineData("o is AssocAction ? ObjectEventSubject.AssociativeBookkeeping : ObjectEventSubject.Drawing")]
    [InlineData("o is AssocAction || o is AssocDependency ? ObjectEventSubject.Drawing : ObjectEventSubject.AssociativeBookkeeping")]
    [InlineData("o.GetType() == typeof(AssocAction) ? ObjectEventSubject.AssociativeBookkeeping : ObjectEventSubject.Drawing")]
    [InlineData("(o as AssocAction) != null || o is AssocDependency ? ObjectEventSubject.AssociativeBookkeeping : ObjectEventSubject.Drawing")]
    public void TheGuardTripsOnAClassifierThatOpensTheObjectReadsItOrTestsMoreOrLessThanTheTwoTypes(string expression)
    {
        var real = Bridge();
        var declaration = "private static ObjectEventSubject SubjectOf(DBObject o) => o is AssocAction || o is AssocDependency ? ObjectEventSubject.AssociativeBookkeeping : ObjectEventSubject.Drawing;";
        Assert.Empty(Violations(real));

        Assert.NotEmpty(Violations(Mutate(real, declaration, "private static ObjectEventSubject SubjectOf(DBObject o) => " + expression + ";")));
    }

    [Fact]
    public void TheGuardTripsOnAClassifierThatIsNotPrivateStaticOrThatIsNamedMoreOftenThanTheFiveHandlers()
    {
        var real = Bridge();
        var declaration = "private static ObjectEventSubject SubjectOf(DBObject o)";
        Assert.Empty(Violations(real));

        Assert.NotEmpty(Violations(Mutate(real, declaration, "public static ObjectEventSubject SubjectOf(DBObject o)")));
        Assert.NotEmpty(Violations(Mutate(real, declaration, "private ObjectEventSubject SubjectOf(DBObject o)")));
        Assert.NotEmpty(Violations(Mutate(real, "private static long NativeId", "private static ObjectEventSubject SubjectOf(DBObject o, int x) => ObjectEventSubject.Drawing;\n        private static long NativeId")));
        Assert.NotEmpty(Violations(Mutate(real, "private static long NativeId", "private static void Spy(DBObject e) { var x = SubjectOf(e); }\n        private static long NativeId")));
    }

    [Fact]
    public void TheGuardTripsWhenTheHandlersOfTheOtherSourcesChangeOrTheClassificationLeaksIntoThem()
    {
        var real = Bridge();
        Assert.Empty(Violations(real));

        // The selection and the three ends of a command: the line of OnSourceEvent, and only that.
        foreach (var (source, _) in SourceEventBlocks)
        {
            Assert.NotEmpty(Violations(ReplaceInBlock(real, source, "_core.OnSourceEvent(a.Session, a.Source)", "_core.OnObjectEvent(a.Session, a.Source, ObjectEventSubject.Drawing)")));
            Assert.NotEmpty(Violations(ReplaceInBlock(real, source, "_core.OnSourceEvent(a.Session, a.Source)", "{ _core.OnSourceEvent(a.Session, a.Source); Contain(\"X\", () => { }); }")));
        }

        // The start and the entry into rest keep deciding with OnSourceEvent, and none of the other blocks names the classification.
        Assert.NotEmpty(Violations(ReplaceInBlock(real, "CommandWillStart", "if (_core.OnSourceEvent(a.Session, a.Source)) RaiseCommandStateChanged();", "_core.OnObjectEvent(a.Session, a.Source, ObjectEventSubject.Drawing);")));
        Assert.NotEmpty(Violations(ReplaceInBlock(real, "CommandWillStart", "_core.NoteForeignCommandStart(a.Session);", "_core.NoteForeignCommandStart(a.Session); var k = SubjectOf(null);")));
        Assert.NotEmpty(Violations(ReplaceInBlock(real, "EnteringQuiescentState", "_core.OnSourceEvent(a.Session, a.Source)", "_core.OnObjectEvent(a.Session, a.Source, ObjectEventSubject.Drawing)")));
        Assert.NotEmpty(Violations(ReplaceInBlock(real, "Idle", "Contain(\"Idle\", OnIdleTick)", "Contain(\"Idle\", OnIdleTick); var z = SubjectOf(null)")));
        Assert.NotEmpty(Violations(ReplaceInBlock(real, "DocumentCreated", "_core.OnDocumentCreated(NativeId(e.Document))", "_core.OnDocumentCreated(NativeId(e.Document)); var z = e.DBObject")));
    }

    [Fact]
    public void TheGuardTripsOnANewHookupARegenerationOrAContainmentThatNoLongerReportsTheFault()
    {
        var real = Bridge();
        Assert.Empty(Violations(real));

        Assert.NotEmpty(Violations(Mutate(real, "document.Database.ObjectAppended += handler;", "document.Database.ObjectAppended += handler; document.Database.ObjectOpenedForModify += handler2;")));
        Assert.NotEmpty(Violations(Mutate(real, "private static long NativeId", "private static void Redraw() { Editor.Regen(); }\n        private static long NativeId")));
        Assert.NotEmpty(Violations(Mutate(real, "_core.ReportFault(attachment.Session,", "_core.Spy(attachment.Session,")));
    }
}
