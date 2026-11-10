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

// I-64 F3-T2-BROWSER-UI: the pure lazy summary of the browser (INV-F3T2-04, 05, 06, 13). BrowserRackSummaries projects name, Kind
// and views of ONE rack from neutral capture facts, remembers only the racks that were asked for by (session, rack), and is
// never a second navigation authority. The Core test project cannot load the Plugin, so the host order is guarded as text in
// WorkspaceBrowserHostGuardTests and here it is simulated over the real session authority.
public class WorkspaceBrowserSummaryTests
{
    private const string SummariesPath = "src/RackCad.Application/Workspace/BrowserRackSummaries.cs";
    private const string PortPath = "src/RackCad.Application/Workspace/IWorkspaceBrowserSummaryPort.cs";

    private static readonly SessionId S1 = new(1);
    private static readonly SessionId S2 = new(2);

    private sealed class Rig
    {
        public Rig()
        {
            Facts[S1] = new List<BrowserRackFact>
            {
                new(true, "R-1", "Selective", "lateral", "Rack uno"),
                new(true, "R-1", "Selective", "frontal", "Rack uno"),
                new(true, "R-1", "Selective", "frontal", "Rack uno"),
                new(true, "r-1", "Selective", "planta", null),
                new(false, "R-1", "Wrong", "ignored", "Ignored"),
                new(true, "R-2", null, "frontal", " "),
                new(true, "R-2", "Cantilever", "lateral", "Segundo"),
                new(true, null, "Selective", "frontal", "No id"),
                new(false, "R-3", "Selective", "frontal", "Unreadable"),
            };
            Facts[S2] = new List<BrowserRackFact> { new(true, "R-9", "Selective", "frontal", "Rack nueve") };
            Alive.Add(S1);
            Alive.Add(S2);
            Summaries = new BrowserRackSummaries(ReadFacts, id => Alive.Contains(id));
        }

        public Dictionary<SessionId, List<BrowserRackFact>> Facts { get; } = new();

        public HashSet<SessionId> Alive { get; } = new();

        public List<SessionId> Reads { get; } = new();

        public Exception? Fault { get; set; }

        public BrowserRackSummaries Summaries { get; }

        private IReadOnlyList<BrowserRackFact> ReadFacts(SessionId id)
        {
            Reads.Add(id);
            if (Fault != null) throw Fault;
            return Facts.TryGetValue(id, out var facts) ? facts : new List<BrowserRackFact>();
        }
    }

    private static DrainOutcome Drain(SessionId session, bool drained, params DrainOperation[] operations) =>
        new(session, new DrainResult(drained, operations));

    // ------------------------------------------------------------------ INV-F3T2-05: the projection

    [Fact]
    public void ThePlainSummaryProjectsNameKindAndDistinctOrderedViewsOfTheRackIgnoringCaseAndUninterpretableFacts()
    {
        var rig = new Rig();

        var summary = rig.Summaries.Read(S1, "R-1");

        Assert.Equal(BrowserSummaryStatus.Ready, summary.Status);
        Assert.Equal("Rack uno", summary.Name);
        Assert.Equal("Selective", summary.Kind);
        Assert.Equal(new[] { "frontal", "lateral", "planta" }, summary.Views);
        Assert.Null(summary.Reason);
        Assert.Same(summary, rig.Summaries.Read(S1, "r-1"));
    }

    [Fact]
    public void NameAndKindAreTheFirstNonBlankOnesAmongTheInterpretableFactsOfTheRackInReadingOrder()
    {
        var rig = new Rig();

        var summary = rig.Summaries.Read(S1, "R-2");

        Assert.Equal("Segundo", summary.Name);
        Assert.Equal("Cantilever", summary.Kind);
        Assert.Equal(new[] { "frontal", "lateral" }, summary.Views);
    }

    [Theory]
    [InlineData("R-404")]
    [InlineData("R-3")]
    public void ARackWithNoInterpretableFactIsNotFoundAndHasNoNameKindOrViews(string rack)
    {
        var rig = new Rig();

        var summary = rig.Summaries.Read(S1, rack);

        Assert.Equal(BrowserSummaryStatus.NotFound, summary.Status);
        Assert.Null(summary.Name);
        Assert.Null(summary.Kind);
        Assert.Empty(summary.Views);
        Assert.False(string.IsNullOrWhiteSpace(summary.Reason));
    }

    [Fact]
    public void ABlankRackIdReadsNothing()
    {
        var rig = new Rig();

        Assert.Equal(BrowserSummaryStatus.NotFound, rig.Summaries.Read(S1, " ").Status);
        Assert.Equal(BrowserSummaryStatus.NotFound, rig.Summaries.Read(S1, string.Empty).Status);
        Assert.Empty(rig.Reads);
    }

    // ------------------------------------------------------------------ INV-F3T2-05 and 06: memory by (session, rack), no inventory

    [Fact]
    public void FactsAreReadOnlyWhenNoSummaryIsRememberedForTheSessionAndTheRack()
    {
        var rig = new Rig();

        rig.Summaries.Read(S1, "R-1");
        rig.Summaries.Read(S1, "R-1");
        rig.Summaries.Read(S1, "R-1");
        Assert.Equal(new[] { S1 }, rig.Reads);

        rig.Summaries.Read(S1, "R-2");
        Assert.Equal(new[] { S1, S1 }, rig.Reads);

        // The same rack id in another session is another key: it is read there and nowhere else.
        Assert.Equal(BrowserSummaryStatus.NotFound, rig.Summaries.Read(S2, "R-1").Status);
        Assert.Equal(new[] { S1, S1, S2 }, rig.Reads);
        Assert.Equal(BrowserSummaryStatus.Ready, rig.Summaries.Read(S2, "R-9").Status);
    }

    [Fact]
    public void ThereIsNoGlobalInventoryOnlyTheSummariesOfTheRacksThatWereAskedFor()
    {
        var rig = new Rig();

        rig.Summaries.Read(S1, "R-1");

        Assert.True(rig.Summaries.IsCached(S1, "R-1"));
        Assert.True(rig.Summaries.IsCached(S1, "r-1"));
        Assert.False(rig.Summaries.IsCached(S1, "R-2"));
        Assert.False(rig.Summaries.IsCached(S2, "R-1"));
        Assert.Equal(1, rig.Summaries.CachedRackCount(S1));
        Assert.Equal(0, rig.Summaries.CachedRackCount(S2));
    }

    [Fact]
    public void AFailingReadRemembersNothingAndTheNextRequestReadsAgain()
    {
        var rig = new Rig { Fault = new InvalidOperationException("boom") };

        var thrown = Assert.Throws<InvalidOperationException>(() => rig.Summaries.Read(S1, "R-1"));
        Assert.Equal("boom", thrown.Message);
        Assert.False(rig.Summaries.IsCached(S1, "R-1"));

        rig.Fault = null;
        Assert.Equal(BrowserSummaryStatus.Ready, rig.Summaries.Read(S1, "R-1").Status);
        Assert.Equal(2, rig.Reads.Count);
    }

    // ------------------------------------------------------------------ INV-F3T2-06: invalidation and purge

    [Theory]
    [InlineData(true, DrainOperation.InvalidateNavigationCaches, true)]
    [InlineData(true, DrainOperation.ReadImplicitSelection, false)]
    [InlineData(true, DrainOperation.RecomputeSelectionContext, false)]
    [InlineData(true, DrainOperation.MarkDraftsPossiblyStale, false)]
    [InlineData(false, DrainOperation.InvalidateNavigationCaches, false)]
    public void ADrainInvalidatesTheSessionMemoryUnderTheSameConditionAsTheNavigationAuthority(bool drained, DrainOperation operation, bool invalidated)
    {
        var rig = new Rig();
        rig.Summaries.Read(S1, "R-1");
        rig.Summaries.Read(S2, "R-9");

        rig.Summaries.OnDrained(Drain(S1, drained, operation));

        Assert.Equal(invalidated, !rig.Summaries.IsCached(S1, "R-1"));
        Assert.True(rig.Summaries.IsCached(S2, "R-9"));

        // After an invalidation the facts are read again; otherwise they are not.
        rig.Summaries.Read(S1, "R-1");
        Assert.Equal(invalidated ? 3 : 2, rig.Reads.Count);
    }

    [Fact]
    public void TheNavigationAuthorityAndTheSummaryMemoryAgreeOnTheDrainCondition()
    {
        // The same DrainResult must invalidate both or neither: the summary never keeps what the navigation drops.
        foreach (var (drained, operation) in new[]
                 {
                     (true, DrainOperation.InvalidateNavigationCaches), (true, DrainOperation.ReadImplicitSelection),
                     (false, DrainOperation.InvalidateNavigationCaches),
                 })
        {
            var facts = new FakeNavigationFacts().With(S1, NavFact.Member(10, "R-1", "frontal", 0, 0x101));
            var targets = new NavigationTargets(facts);
            targets.Resolve(S1, "R-1");
            var rig = new Rig();
            rig.Summaries.Read(S1, "R-1");
            var drain = new DrainResult(drained, new[] { operation });

            var navigationDropped = targets.ApplyDrain(S1, drain);
            rig.Summaries.OnDrained(new DrainOutcome(S1, drain));

            Assert.Equal(navigationDropped, !rig.Summaries.IsCached(S1, "R-1"));
        }
    }

    [Fact]
    public void ADrainPurgesTheSessionsThatNoLongerLiveAndAnEndedSessionIsNeverResurrected()
    {
        var rig = new Rig();
        rig.Summaries.Read(S1, "R-1");
        rig.Summaries.Read(S2, "R-9");
        Assert.Equal(2, rig.Summaries.KnownSessionCount);
        rig.Alive.Remove(S2);

        rig.Summaries.OnDrained(Drain(S1, true, DrainOperation.ReadImplicitSelection));

        Assert.True(rig.Summaries.IsSessionEnded(S2));
        Assert.Equal(0, rig.Summaries.CachedRackCount(S2));
        Assert.Equal(1, rig.Summaries.KnownSessionCount);
        Assert.True(rig.Summaries.IsCached(S1, "R-1"));

        // Even if the id is reported alive again, it is not revived and nothing is read for it.
        rig.Alive.Add(S2);
        var reads = rig.Reads.Count;
        Assert.Equal(BrowserSummaryStatus.Rejected, rig.Summaries.Read(S2, "R-9").Status);
        Assert.Equal(reads, rig.Reads.Count);
        rig.Summaries.OnDrained(Drain(S2, true, DrainOperation.InvalidateNavigationCaches));
        Assert.Equal(0, rig.Summaries.CachedRackCount(S2));
    }

    [Fact]
    public void ASessionFoundDeadWhenItIsAskedForIsEndedAndNothingIsRead()
    {
        var rig = new Rig();
        rig.Alive.Remove(S1);

        var summary = rig.Summaries.Read(S1, "R-1");

        Assert.Equal(BrowserSummaryStatus.Rejected, summary.Status);
        Assert.False(string.IsNullOrWhiteSpace(summary.Reason));
        Assert.Empty(rig.Reads);
        Assert.True(rig.Summaries.IsSessionEnded(S1));
    }

    [Fact]
    public void EndingASessionDropsItsMemoryAndItNeverAnswersAgain()
    {
        var rig = new Rig();
        rig.Summaries.Read(S1, "R-1");

        rig.Summaries.EndSession(S1);

        Assert.Equal(0, rig.Summaries.CachedRackCount(S1));
        Assert.True(rig.Summaries.IsSessionEnded(S1));
        Assert.Equal(BrowserSummaryStatus.Rejected, rig.Summaries.Read(S1, "R-1").Status);
        Assert.Single(rig.Reads);
    }

    // ------------------------------------------------------------------ INV-F3T2-13: the session of the request

    // The host answers in this order: authorize the session of the request against the active document, and only then the memory or
    // the read. Here the same order runs over the real session authority, with no AutoCAD.
    private sealed class SimulatedHost
    {
        private readonly WorkspaceSessionRegistry _sessions = new();
        private readonly Rig _rig = new();

        public SimulatedHost()
        {
            A = _sessions.GetOrCreate(1001).Id;
            B = _sessions.GetOrCreate(1002).Id;
            _rig.Facts.Clear();
            _rig.Facts[A] = new List<BrowserRackFact> { new(true, "R-1", "Selective", "frontal", "Rack de A") };
            _rig.Facts[B] = new List<BrowserRackFact> { new(true, "R-9", "Selective", "lateral", "Rack de B") };
            _rig.Alive.Clear();
            _rig.Alive.Add(A);
            _rig.Alive.Add(B);
        }

        public SessionId A { get; }

        public SessionId B { get; }

        public List<SessionId> Reads => _rig.Reads;

        public BrowserRackSummaries Summaries => _rig.Summaries;

        public BrowserRackSummary Read(WorkspaceRequest origin, string rackId, long activeNativeId)
        {
            var decision = _sessions.Authorize(origin, activeNativeId);
            if (decision != RequestDecision.Accepted)
                return BrowserRackSummary.Without(BrowserSummaryStatus.Rejected, "The request does not belong to the active document.");
            return _rig.Summaries.Read(origin.Origin, rackId);
        }
    }

    [Fact]
    public void ARequestOfSessionAWhileBIsActiveIsRejectedWithoutAnyReadAndTheTabsOfBothSessionsStayApart()
    {
        var host = new SimulatedHost();

        var rejected = host.Read(new WorkspaceRequest(host.A), "R-1", activeNativeId: 1002);
        Assert.Equal(BrowserSummaryStatus.Rejected, rejected.Status);
        Assert.Empty(host.Reads);
        Assert.Equal(0, host.Summaries.CachedRackCount(host.A));
        Assert.Equal(0, host.Summaries.KnownSessionCount);

        // Each session answers only for its own rack, even if the other one is active at the time it asks.
        var forA = host.Read(new WorkspaceRequest(host.A), "R-1", activeNativeId: 1001);
        var forB = host.Read(new WorkspaceRequest(host.B), "R-9", activeNativeId: 1002);
        Assert.Equal("Rack de A", forA.Name);
        Assert.Equal("Rack de B", forB.Name);
        Assert.Equal(BrowserSummaryStatus.NotFound, host.Read(new WorkspaceRequest(host.B), "R-1", activeNativeId: 1002).Status);
        Assert.Equal(1, host.Summaries.CachedRackCount(host.A));
        Assert.Equal(2, host.Summaries.CachedRackCount(host.B));
    }

    // ------------------------------------------------------------------ INV-F3T2-05 and 19: shape and source guards of the new types

    [Fact]
    public void TheStatusesAndTheSummaryHaveExactlyTheContractedShape()
    {
        Assert.Equal(new[] { "Ready", "NotFound", "Busy", "Rejected", "Failed" }, Enum.GetNames<BrowserSummaryStatus>());

        var properties = typeof(BrowserRackSummary).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(p => p.Name + ":" + p.PropertyType.Name)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(
            new[] { "IsReady:Boolean", "Kind:String", "Name:String", "Reason:String", "Status:BrowserSummaryStatus", "Views:IReadOnlyList`1" },
            properties);
        Assert.True(typeof(BrowserRackSummary).IsSealed);
        Assert.True(typeof(IEquatable<BrowserRackSummary>).IsAssignableFrom(typeof(BrowserRackSummary)));
    }

    [Fact]
    public void TheCaptureFactCarriesNoHandleAndNoReferenceToTheDrawing()
    {
        var properties = typeof(BrowserRackFact).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .Select(p => p.Name + ":" + p.PropertyType.Name)
            .OrderBy(n => n, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(new[] { "EnvelopeId:String", "IsInterpretable:Boolean", "Kind:String", "Name:String", "View:String" }, properties);
        Assert.True(typeof(BrowserRackFact).IsSealed);
        foreach (var type in new[] { typeof(BrowserRackFact), typeof(BrowserRackSummary) })
            foreach (var field in type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
                Assert.DoesNotContain("Int64", field.FieldType.ToString());
    }

    [Fact]
    public void ThePortHasExactlyTheTwoReadOnlyOperationsOfTheContract()
    {
        var methods = typeof(IWorkspaceBrowserSummaryPort).GetMethods().OrderBy(m => m.Name, StringComparer.Ordinal).ToArray();

        Assert.Equal(new[] { "IsSessionAlive", "ReadSummary" }, methods.Select(m => m.Name).ToArray());
        var read = methods.Single(m => m.Name == "ReadSummary");
        Assert.Equal(typeof(BrowserRackSummary), read.ReturnType);
        Assert.Equal(new[] { "origin", "rackId" }, read.GetParameters().Select(p => p.Name).ToArray());
        Assert.Equal(new[] { typeof(WorkspaceRequest), typeof(string) }, read.GetParameters().Select(p => p.ParameterType).ToArray());
        var alive = methods.Single(m => m.Name == "IsSessionAlive");
        Assert.Equal(typeof(bool), alive.ReturnType);
        Assert.Equal(new[] { typeof(SessionId) }, alive.GetParameters().Select(p => p.ParameterType).ToArray());
        Assert.Empty(typeof(IWorkspaceBrowserSummaryPort).GetProperties());
        Assert.Empty(typeof(IWorkspaceBrowserSummaryPort).GetEvents());
    }

    [Fact]
    public void ThePureSummaryNamesNeitherTheNavigationAuthorityNorTheHandlesNorTheHostNorWrites()
    {
        foreach (var path in new[] { SummariesPath, PortPath })
        {
            var text = Src.Text(path);
            Assert.StartsWith("#nullable enable", text.TrimStart('﻿'), StringComparison.Ordinal);
            var code = Src.CodeOnly(text);

            Assert.Equal(1, Src.Count(code, @"\bnamespace\b"));
            Assert.Matches(@"namespace\s+RackCad\.Application\.Workspace\b", code);
            Assert.DoesNotMatch(
                new Regex(@"\bNavigationTargets\b|\bNavigationTargetSet\b|\bNavigationTarget\b|\bNavigationDefinitionFact\b|\bINavigationFactsPort\b|\bIWorkspaceNavigationPort\b|ModelSpaceReferenceHandles|ReadDefinitionFacts|\bNavigationActionPolicy\b|\bWorkspaceNavigationRunner\b"),
                code);
            Assert.DoesNotMatch(new Regex(@"\bAutodesk\.|System\.Windows|RackCad\.Plugin|RackCad\.UI|Transaction|LockDocument|\bDatabase\b|\bCommit\(|System\.IO|\bTask\b|\basync\b|\bawait\b|\bTimer\b|\bThread\b|\bevent\b|\bstatic\s+\w+\s+\w+\s*;"), code);
        }

        // It is projected from its own capture facts and its two functions, nothing more.
        var constructor = Assert.Single(typeof(BrowserRackSummaries).GetConstructors());
        Assert.Equal(
            new[] { typeof(Func<SessionId, IReadOnlyList<BrowserRackFact>>), typeof(Func<SessionId, bool>) },
            constructor.GetParameters().Select(p => p.ParameterType).ToArray());
        Assert.Equal(
            new[] { "CachedRackCount", "EndSession", "IsCached", "IsSessionEnded", "OnDrained", "Read" },
            typeof(BrowserRackSummaries).GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                .Where(m => !m.IsSpecialName).Select(m => m.Name).OrderBy(n => n, StringComparer.Ordinal).ToArray());
    }

    [Fact]
    public void NavigationTargetsIsStillTheOnlyAuthorityOfTargetsAndKnowsNothingOfTheSummary()
    {
        var navigation = Src.Code("src/RackCad.Application/Workspace/NavigationTargets.cs") + Src.Code("src/RackCad.Application/Workspace/NavigationFacts.cs");

        Assert.DoesNotMatch(new Regex(@"Browser|Summary"), navigation);
        Assert.Equal(1, Src.AllProduct().Count(f => Regex.IsMatch(f.Code, @"\bclass\s+NavigationTargets\b")));
        Assert.Equal(1, Src.AllProduct().Count(f => Regex.IsMatch(f.Code, @"\bclass\s+BrowserRackSummaries\b")));
    }

    [Fact]
    public void NoTypeOfTheSummaryCarriesTabSubscriptionEditorOrDraftInItsName()
    {
        var names = new[] { typeof(BrowserSummaryStatus), typeof(BrowserRackSummary), typeof(BrowserRackFact), typeof(BrowserRackSummaries), typeof(IWorkspaceBrowserSummaryPort) }
            .Select(t => t.Name)
            .ToArray();

        foreach (var name in names)
        {
            Assert.DoesNotMatch(new Regex("Tab|Subscription|Editor|Draft", RegexOptions.IgnoreCase), name);
            Assert.DoesNotMatch(new Regex(@"ProjectSummary|Metric|Provider|TotalRacks|Aggregat|Bom\b", RegexOptions.IgnoreCase), name);
        }
    }
}
