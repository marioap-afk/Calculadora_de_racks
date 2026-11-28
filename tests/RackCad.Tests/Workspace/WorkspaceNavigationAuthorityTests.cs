#nullable enable
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using RackCad.Application.Workspace;
using Xunit;
using Src = RackCad.Tests.WorkspaceHostSource;

namespace RackCad.Tests;

// I-64 F2-T1-NAVIGATION-CORE: INV-38 (one authority for the context, the tabs and the navigator) and INV-03 (no event writes
// the selection or the view).
public class WorkspaceNavigationAuthorityTests
{
    private static readonly SessionId S1 = new(1);

    private static (NavigationTargets Targets, FakeNavigationFacts Port) Build()
    {
        var port = new FakeNavigationFacts().With(
            S1,
            NavFact.Member(10, "R-1", "frontal", 0, 0x101, 0x102),
            NavFact.Member(11, "R-1", "lateral", 0, 0x103),
            NavFact.Raw(12, false, false, null, "R-2", "selective", "frontal", 0, 0x201));
        return (new NavigationTargets(port), port);
    }

    [Fact]
    public void TheContextObtainsItsTargetsFromTheSameAuthorityAsAnyOtherEntry()
    {
        var (targets, port) = Build();

        var fromContext = targets.ResolveForContext(S1, SelectionContext.One("R-1"));
        var fromAnotherEntry = targets.Resolve(S1, "R-1");

        Assert.Same(fromContext, fromAnotherEntry);
        Assert.Equal(new long[] { 0x101, 0x102, 0x103 }, NavFact.Handles(fromContext));
        Assert.Equal(1, port.Reads);
    }

    [Fact]
    public void TheContextAndAFutureEntryGiveTheSameSetEvenWhenAskedInTheOtherOrder()
    {
        var (targets, port) = Build();

        var fromTab = targets.Resolve(S1, "r-1");
        var fromContext = targets.ResolveForContext(S1, SelectionContext.One("R-1"));

        Assert.Equal(NavFact.Handles(fromTab), NavFact.Handles(fromContext));
        Assert.Equal(1, port.Reads);
    }

    [Fact]
    public void AContextThatIsNotOneRackNavigatesNowhereAndReadsNothing()
    {
        var (targets, port) = Build();

        foreach (var context in new[]
                 {
                     SelectionContext.None, SelectionContext.NoIdentity, SelectionContext.Diagnostic,
                     SelectionContext.Selection(2, 1),
                 })
        {
            Assert.True(targets.ResolveForContext(S1, context).IsEmpty);
        }

        Assert.Equal(0, port.Reads);
    }

    [Fact]
    public void AnUnreadableMemberKnownOnlyByTheProbeGivesADiagnosticAndZeroTargetsFromTheContext()
    {
        var (targets, _) = Build();

        var set = targets.ResolveForContext(S1, SelectionContext.One("R-2"));

        Assert.Empty(set.Targets);
        var diagnostic = Assert.Single(set.Diagnostics);
        Assert.Equal(NavigationDiagnosticKind.UnreadableMember, diagnostic.Kind);
    }

    [Fact]
    public void ThereAreExactlyTwoWaysToObtainATargetSetAndBothEndInTheSameDerivation()
    {
        var entries = typeof(NavigationTargets)
            .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
            .Where(m => m.ReturnType == typeof(NavigationTargetSet))
            .Select(m => m.Name)
            .OrderBy(n => n, System.StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(new[] { "Resolve", "ResolveForContext" }, entries);
        Assert.Equal(1, Src.AllProduct().Count(f => Regex.IsMatch(f.Code, @"\bclass\s+NavigationTargets\b")));
    }

    [Fact]
    public void TheFactsPortIsReadFromASingleDerivationInTheWholeProduct()
    {
        var callers = Src.AllProduct()
            .Select(f => (f.Path, Count: Src.Count(f.Code, @"\.ReadDefinitionFacts\(")))
            .Where(x => x.Count > 0)
            .ToArray();

        var only = Assert.Single(callers);
        Assert.Equal("src/RackCad.Application/Workspace/NavigationTargets.cs", only.Path);
        Assert.Equal(1, only.Count);
    }

    [Fact]
    public void OnlyTheAuthorityTraversesTheReferenceHandlesOfTheFacts()
    {
        var users = Src.AllProduct()
            .Where(f => Regex.IsMatch(f.Code, @"\bModelSpaceReferenceHandles\b"))
            .Select(f => f.Path)
            .OrderBy(p => p, System.StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(
            new[]
            {
                "src/RackCad.Application/Workspace/NavigationFacts.cs",
                "src/RackCad.Application/Workspace/NavigationTargets.cs",

                // I-64 F2-T2A-NAVIGATION-HOST (INV-F2T2A-05, 10): the one adapter that fills the facts, by EXACT FILE. It only
                // supplies the handles; it classifies nothing and traverses them for nothing.
                "src/RackCad.Plugin/Workspace/WorkspaceNavigationFacts.cs",
            },
            users);
    }

    [Fact]
    public void NoEventSideFileCanReachTheNavigationEffectsOrThePolicy()
    {
        var eventSide = new Regex(@"\bIHostEventSource\b|\bHintKind\b|\bEnqueueHint\b|\bEventBridgeCore\b|\bIWorkspaceBridgeLink\b");
        var effectSide = new Regex(@"\bINavigationEffects\b|\bNavigationActionPolicy\b|\bNavigationActionPlan\b");

        var offenders = Src.AllProduct()
            .Where(f => effectSide.IsMatch(f.Code) && eventSide.IsMatch(f.Code))
            .Select(f => f.Path)
            .ToArray();

        Assert.Empty(offenders);
        Assert.Contains(Src.AllProduct(), f => f.Path.EndsWith("/NavigationActions.cs") && effectSide.IsMatch(f.Code));

        // I-64 F2-T2A-NAVIGATION-HOST (INV-F2T2A-02, 12): the policy and the effects live in the host, the effects adapter and
        // the pure runner; none of them names an event-side type, and no event-side file of the Plugin names them.
        foreach (var path in new[]
                 {
                     "src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs",
                     "src/RackCad.Plugin/Workspace/WorkspaceNavigationEffects.cs",
                     "src/RackCad.Application/Workspace/IWorkspaceNavigationPort.cs",
                 })
        {
            var code = Src.Code(path);
            Assert.True(effectSide.IsMatch(code), path + " must carry the policy or the effects");
            Assert.DoesNotMatch(eventSide, code);
        }

        foreach (var path in new[]
                 {
                     "src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs",
                     "src/RackCad.Plugin/Workspace/WorkspaceController.cs",
                     "src/RackCad.Plugin/Workspace/WorkspaceSelectionReader.cs",
                     "src/RackCad.Plugin/Workspace/WorkspaceHostPort.cs",
                     "src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs",
                     "src/RackCad.Application/Workspace/WorkspaceHostCore.cs",
                     "src/RackCad.Application/Workspace/NavigationEcho.cs",
                 })
            Assert.DoesNotMatch(effectSide, Src.Code(path));
        foreach (var name in new[] { "EventBridgeCore.cs", "WorkspaceHostCore.cs", "WorkspaceSession.cs", "Hints.cs", "BridgeSource.cs" })
        {
            Assert.DoesNotMatch(new Regex(@"\bNavigation(Targets|Action\w*|Effects)\b|\bINavigationEffects\b"), Src.Code("src/RackCad.Application/Workspace/" + name));
        }
    }

    [Fact]
    public void NavigationNeverWritesTheSelectionOrTheViewByItself()
    {
        var effects = typeof(INavigationEffects).GetMethods().Select(m => m.Name).OrderBy(n => n, System.StringComparer.Ordinal).ToArray();

        Assert.Equal(new[] { "CenterOnReference", "SelectReferences", "ViewReference" }, effects);
        var code = string.Concat(
            new[] { "NavigationTargets.cs", "NavigationTargetSet.cs", "NavigationFacts.cs" }
                .Select(n => Src.Code("src/RackCad.Application/Workspace/" + n)));
        Assert.DoesNotMatch(new Regex(@"INavigationEffects|SelectReferences|ViewReference|CenterOnReference"), code);
    }

    [Fact]
    public void TheNavigationTypesLiveInTheApplicationWorkspaceFolderOnly()
    {
        var folder = "src/RackCad.Application/Workspace/";
        var files = new[] { "NavigationFacts.cs", "NavigationTargetSet.cs", "NavigationTargets.cs", "NavigationActions.cs" };

        foreach (var file in files) Assert.True(Src.Exists(folder + file), file);
        Assert.Equal(typeof(NavigationTargets).Namespace, typeof(INavigationFactsPort).Namespace);
        Assert.All(files, f => Assert.StartsWith("#nullable enable", Src.Text(folder + f).TrimStart('﻿')));
    }
}
