#nullable enable
using System.Linq;
using RackCad.Application.Workspace;
using Xunit;

namespace RackCad.Tests;

// I-64 F2-T1-NAVIGATION-CORE: D-05 member rule, order and probe-only diagnostics of the single NavigationTargets authority.
public class WorkspaceNavigationTargetsTests
{
    private static readonly SessionId S1 = new(1);
    private static readonly SessionId S2 = new(2);

    private static (NavigationTargets Targets, FakeNavigationFacts Port) Build(params NavigationDefinitionFact[] facts)
    {
        var port = new FakeNavigationFacts().With(S1, facts);
        return (new NavigationTargets(port), port);
    }

    [Fact]
    public void AnInterpretableNonXrefDefinitionWithTheSameIdGivesItsTopLevelModelSpaceReferences()
    {
        var (targets, _) = Build(NavFact.Member(10, "R-1", "frontal", 0, 0x101, 0x102));

        var set = targets.Resolve(S1, "R-1");

        Assert.Equal(new RackKey(S1, "R-1"), set.Key);
        Assert.Equal(new long[] { 0x101, 0x102 }, NavFact.Handles(set));
        Assert.All(set.Targets, t => Assert.Equal(10, t.DefinitionHandle));
        Assert.Empty(set.Diagnostics);
        Assert.False(set.IsEmpty);
    }

    [Theory]
    [InlineData("rack-a", "RACK-A")]
    [InlineData("RACK-A", "rack-a")]
    [InlineData("Rack-A", "rAcK-a")]
    public void TheEnvelopeIdIsComparedWithOrdinalIgnoreCase(string envelopeId, string rackId)
    {
        var (targets, _) = Build(NavFact.Member(10, envelopeId, "lateral", 0, 0x20));

        Assert.Equal(new long[] { 0x20 }, NavFact.Handles(targets.Resolve(S1, rackId)));
    }

    [Fact]
    public void AnXrefDefinitionIsNeverAMember()
    {
        var (targets, _) = Build(
            NavFact.Raw(10, true, true, "R-1", null, "selective", "frontal", 0, 0x30),
            NavFact.Member(11, "R-1", "frontal", 0, 0x31));

        Assert.Equal(new long[] { 0x31 }, NavFact.Handles(targets.Resolve(S1, "R-1")));
    }

    [Fact]
    public void AnUninterpretableDefinitionIsNeverAMemberEvenWhenItsEnvelopeIdMatches()
    {
        var (targets, _) = Build(NavFact.Raw(10, false, false, "R-1", null, "selective", "frontal", 0, 0x30));

        Assert.Empty(targets.Resolve(S1, "R-1").Targets);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void ADefinitionWithoutAnEnvelopeIdIsNeverAMemberEvenWhenTheProbeSawTheId(string? envelopeId)
    {
        var (targets, _) = Build(NavFact.Raw(10, false, true, envelopeId, "R-1", "selective", "frontal", 0, 0x30));

        var set = targets.Resolve(S1, "R-1");

        Assert.Empty(set.Targets);
        Assert.Empty(set.Diagnostics);
    }

    [Fact]
    public void ADefinitionOfAnotherRackIsNotAMember()
    {
        var (targets, _) = Build(
            NavFact.Member(10, "R-1", "frontal", 0, 0x30),
            NavFact.Member(11, "R-2", "frontal", 0, 0x31));

        Assert.Equal(new long[] { 0x30 }, NavFact.Handles(targets.Resolve(S1, "R-1")));
        Assert.Equal(new long[] { 0x31 }, NavFact.Handles(targets.Resolve(S1, "R-2")));
    }

    [Fact]
    public void AMemberKnownOnlyByTheProbeIsADiagnosticAndNeverATarget()
    {
        var (targets, _) = Build(NavFact.Raw(10, false, false, null, "R-1", "selective", "frontal", 0, 0x30, 0x31));

        var set = targets.Resolve(S1, "R-1");

        Assert.Empty(set.Targets);
        Assert.True(set.IsEmpty);
        Assert.Null(set.Primary);
        var diagnostic = Assert.Single(set.Diagnostics);
        Assert.Equal(NavigationDiagnosticKind.UnreadableMember, diagnostic.Kind);
        Assert.Equal(10, diagnostic.DefinitionHandle);
    }

    [Fact]
    public void AProbedMemberDoesNotAddTargetsToTheInterpretableMembersOfTheSameRack()
    {
        var (targets, _) = Build(
            NavFact.Member(10, "R-1", "frontal", 0, 0x40),
            NavFact.Raw(11, false, false, null, "r-1", "selective", "lateral", 0, 0x41));

        var set = targets.Resolve(S1, "R-1");

        Assert.Equal(new long[] { 0x40 }, NavFact.Handles(set));
        Assert.Equal(11, Assert.Single(set.Diagnostics).DefinitionHandle);
    }

    [Fact]
    public void AProbedXrefOrAProbedIdOfAnotherRackProducesNoDiagnosticForThisRack()
    {
        var (targets, _) = Build(
            NavFact.Raw(10, true, false, null, "R-1", "selective", "frontal", 0, 0x30),
            NavFact.Raw(11, false, false, null, "R-2", "selective", "frontal", 0, 0x31));

        var set = targets.Resolve(S1, "R-1");

        Assert.Empty(set.Targets);
        Assert.Empty(set.Diagnostics);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ABlankRackIdResolvesToNothingWithoutReadingTheDocument(string rackId)
    {
        var (targets, port) = Build(NavFact.Member(10, "R-1", "frontal", 0, 0x30));

        var set = targets.Resolve(S1, rackId);

        Assert.Empty(set.Targets);
        Assert.Equal(0, port.Reads);
    }

    [Fact]
    public void TheOrderIsFrontalLateralPlantaThenTheRestAndWithinEachViewByHandle()
    {
        var (targets, _) = Build(
            NavFact.Raw(1, false, true, "R-1", null, "selective", "planta", -1, 0x0B, 0x0A),
            NavFact.Raw(2, false, true, "R-1", null, "selective", "lateral", 1, 0x1F, 0x09),
            NavFact.Raw(3, false, true, "R-1", null, "selective", "frontal", 0, 0x30, 0x2F),
            NavFact.Raw(4, false, true, "R-1", null, "selective", "frontal", 1, 0x10),
            NavFact.Raw(5, false, true, "R-1", null, "unknown-kind", "zeta", 3, 0x05));

        var set = targets.Resolve(S1, "R-1");

        Assert.Equal(new long[] { 0x10, 0x2F, 0x30, 0x09, 0x1F, 0x0A, 0x0B, 0x05 }, NavFact.Handles(set));
        Assert.Equal(
            new[]
            {
                NavigationViewOrder.Frontal, NavigationViewOrder.Frontal, NavigationViewOrder.Frontal,
                NavigationViewOrder.Lateral, NavigationViewOrder.Lateral,
                NavigationViewOrder.Planta, NavigationViewOrder.Planta,
                NavigationViewOrder.Other,
            },
            set.Targets.Select(t => t.ViewOrder).ToArray());
    }

    [Fact]
    public void HandlesAreOrderedAsNumbersNotAsText()
    {
        var (targets, _) = Build(NavFact.Member(1, "R-1", "frontal", 0, 0x100, 0x9, 0xA0, 0x1));

        Assert.Equal(new long[] { 0x1, 0x9, 0xA0, 0x100 }, NavFact.Handles(targets.Resolve(S1, "R-1")));
    }

    [Fact]
    public void TheOrderDoesNotDependOnTheOrderTheHostReturnsTheFacts()
    {
        var facts = new[]
        {
            NavFact.Raw(1, false, true, "R-1", null, "selective", "planta", -1, 0x0A),
            NavFact.Raw(2, false, true, "R-1", null, "selective", "lateral", 0, 0x09),
            NavFact.Raw(3, false, true, "R-1", null, "selective", "frontal", 0, 0x30, 0x2F),
            NavFact.Raw(5, false, true, "R-1", null, "other-kind", "zeta", 3, 0x05, 0x04),
        };
        var (forward, _) = Build(facts);
        var (backward, _) = Build(facts.Reverse().ToArray());

        var expected = NavFact.Handles(forward.Resolve(S1, "R-1"));

        Assert.Equal(new long[] { 0x2F, 0x30, 0x09, 0x0A, 0x04, 0x05 }, expected);
        Assert.Equal(expected, NavFact.Handles(backward.Resolve(S1, "R-1")));
    }

    [Fact]
    public void TheCodecPlacesCabeceraAndCamaDefinitionsWithoutAViewInTheLateralGroup()
    {
        var (targets, _) = Build(
            NavFact.Raw(1, false, true, "R-1", null, "cabecera", null, -1, 0x50),
            NavFact.Raw(2, false, true, "R-1", null, "cama", null, -1, 0x51),
            NavFact.Raw(3, false, true, "R-1", null, "selective", "frontal", 0, 0x52));

        var set = targets.Resolve(S1, "R-1");

        Assert.Equal(new long[] { 0x52, 0x50, 0x51 }, NavFact.Handles(set));
        Assert.Equal(NavigationViewOrder.Lateral, set.Targets[1].ViewOrder);
    }

    [Fact]
    public void AReferenceListedByTwoMembersAppearsOnce()
    {
        var (targets, _) = Build(
            NavFact.Member(1, "R-1", "frontal", 0, 0x60, 0x61),
            NavFact.Member(2, "R-1", "lateral", 0, 0x61));

        Assert.Equal(new long[] { 0x60, 0x61 }, NavFact.Handles(targets.Resolve(S1, "R-1")));
    }

    [Fact]
    public void ThePrimaryTargetIsTheFirstOfTheOrderAndReferenceHandlesFollowIt()
    {
        var (targets, _) = Build(
            NavFact.Member(1, "R-1", "lateral", 0, 0x70),
            NavFact.Member(2, "R-1", "frontal", 0, 0x72, 0x71));

        var set = targets.Resolve(S1, "R-1");

        Assert.Equal(0x71, set.Primary!.ReferenceHandle);
        Assert.Equal(new long[] { 0x71, 0x72, 0x70 }, set.ReferenceHandles());
    }

    [Fact]
    public void ARackWithoutMembersResolvesToAnEmptySetForTheRequestedKey()
    {
        var (targets, _) = Build(NavFact.Member(1, "R-9", "frontal", 0, 0x70));

        var set = targets.Resolve(S1, "R-1");

        Assert.True(set.IsEmpty);
        Assert.Equal(new RackKey(S1, "R-1"), set.Key);
    }

    [Fact]
    public void TheFactsAreReadForTheRequestedSessionOnly()
    {
        var port = new FakeNavigationFacts()
            .With(S1, NavFact.Member(1, "R-1", "frontal", 0, 0x70))
            .With(S2, NavFact.Member(2, "R-1", "frontal", 0, 0x80));
        var targets = new NavigationTargets(port);

        Assert.Equal(new long[] { 0x70 }, NavFact.Handles(targets.Resolve(S1, "R-1")));
        Assert.Equal(new long[] { 0x80 }, NavFact.Handles(targets.Resolve(S2, "R-1")));
        Assert.Equal(new[] { S1, S2 }, port.ReadSessions);
    }
}
