#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Workspace;
using Xunit;

namespace RackCad.Tests;

// I-64 F1-T3A-HOST-CORE: INV-F1-T3A-07 (Classify / ApplyContext path) and the per-definition cache of INV-F1-T3A-06.
public class WorkspaceSelectionMappingTests
{
    private static WorkspaceSelectionRead Read(int nonRack, params DefinitionFact[] references) => new(references, nonRack);

    [Fact]
    public void NothingSelectedIsTheNoneContext()
    {
        Assert.Equal(SelectionContext.None, WorkspaceSelectionMapper.ToContext(Read(0)));
    }

    [Fact]
    public void OneRackWithAnInterpretableEnvelopeAndANonEmptyIdIsTheOneContext()
    {
        var context = WorkspaceSelectionMapper.ToContext(Read(0, new DefinitionFact(true, "RACK-1")));

        Assert.Equal(SelectionContextKind.One, context.Kind);
        Assert.Equal("RACK-1", context.RackId);
    }

    [Fact]
    public void SeveralViewBlocksOfTheSameRackAreStillTheOneContext()
    {
        var facts = new[] { new DefinitionFact(true, "RACK-1"), new DefinitionFact(true, "RACK-1"), new DefinitionFact(true, "RACK-1") };

        var context = WorkspaceSelectionMapper.ToContext(Read(0, facts));

        Assert.Equal(SelectionContext.One("RACK-1"), context);
    }

    [Fact]
    public void TwoDifferentRacksAreASelectionOfTwoWithNoOthers()
    {
        var context = WorkspaceSelectionMapper.ToContext(Read(0, new DefinitionFact(true, "A"), new DefinitionFact(true, "B")));

        Assert.Equal(SelectionContext.Selection(2, 0), context);
    }

    [Fact]
    public void ARackPlusOtherEntitiesIsASelectionWithTheOthersCounted()
    {
        var context = WorkspaceSelectionMapper.ToContext(Read(3, new DefinitionFact(true, "A")));

        Assert.Equal(SelectionContextKind.Selection, context.Kind);
        Assert.Equal(1, context.RackCount);
        Assert.Equal(3, context.OtherCount);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AnInterpretableEnvelopeWithoutAnIdIsTheNoIdentityContext(string? id)
    {
        var context = WorkspaceSelectionMapper.ToContext(Read(0, new DefinitionFact(true, id)));

        Assert.Equal(SelectionContext.NoIdentity, context);
        Assert.Null(context.RackId);
    }

    [Fact]
    public void AnUnreadableOrFutureEnvelopeIsTheDiagnosticContext()
    {
        var context = WorkspaceSelectionMapper.ToContext(Read(0, new DefinitionFact(false, null)));

        Assert.Equal(SelectionContext.Diagnostic, context);
    }

    [Fact]
    public void AProbedIdNeverGivesAnIdentityWhenTheEnvelopeIsNotInterpretable()
    {
        var context = WorkspaceSelectionMapper.ToContext(Read(0, new DefinitionFact(false, null, ProbedId: "RACK-9")));

        Assert.Equal(SelectionContext.Diagnostic, context);
        Assert.Null(context.RackId);
    }

    [Fact]
    public void AProbedIdNeverReplacesAMissingEnvelopeId()
    {
        var context = WorkspaceSelectionMapper.ToContext(Read(0, new DefinitionFact(true, null, ProbedId: "RACK-9")));

        Assert.Equal(SelectionContext.NoIdentity, context);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(40)]
    public void NestedExternalAndPaperSpaceReferencesCountAsNotRackSoOneRackBecomesASelection(int notRack)
    {
        var context = WorkspaceSelectionMapper.ToContext(Read(notRack, new DefinitionFact(true, "RACK-1")));

        Assert.Equal(SelectionContext.Selection(1, notRack), context);
        Assert.Null(context.RackId);
    }

    [Fact]
    public void NoImpliedSelectionBecauseOfPickFirstIsTheNoneContextWhateverTheRestSays()
    {
        var read = new WorkspaceSelectionRead(new[] { new DefinitionFact(true, "RACK-1") }, 2, PickFirstEnabled: false);

        Assert.Equal(SelectionContext.None, WorkspaceSelectionMapper.ToContext(read));
    }

    [Fact]
    public void ARackIdIsNeverInventedForAnyKindButOne()
    {
        var reads = new[]
        {
            Read(0),
            Read(0, new DefinitionFact(true, "A"), new DefinitionFact(true, "B")),
            Read(2, new DefinitionFact(true, "A")),
            Read(0, new DefinitionFact(true, null)),
            Read(0, new DefinitionFact(false, null, "A")),
        };

        foreach (var read in reads)
        {
            var context = WorkspaceSelectionMapper.ToContext(read);
            Assert.NotEqual(SelectionContextKind.One, context.Kind);
            Assert.Null(context.RackId);
        }
    }

    [Fact]
    public void TheSameSelectionAlwaysGivesAnEqualContextSoItNeverNavigatesTwice()
    {
        var read = Read(1, new DefinitionFact(true, "A"), new DefinitionFact(true, "B"));

        Assert.Equal(WorkspaceSelectionMapper.ToContext(read), WorkspaceSelectionMapper.ToContext(read));
    }

    [Fact]
    public void TheMappingIsExactlyTheClassifyOfTheModel()
    {
        var facts = new[] { new DefinitionFact(true, "A"), new DefinitionFact(true, null), new DefinitionFact(false, null) };

        Assert.Equal(SelectionContext.Classify(facts, 4), WorkspaceSelectionMapper.ToContext(Read(4, facts)));
    }

    [Fact]
    public void AFailedReadIsNotAContextAndSaysSo()
    {
        Assert.False(WorkspaceSelectionRead.Failed.Succeeded);
        Assert.Empty(WorkspaceSelectionRead.Failed.References);
    }

    // ------------------------------------------------------------------ the per-definition cache

    private static readonly SessionId A = new(1);
    private static readonly SessionId B = new(2);

    [Fact]
    public void AnEmptyCacheRemembersNothing()
    {
        var cache = new WorkspaceDefinitionCache();

        Assert.Equal(0, cache.Count);
        Assert.False(cache.TryGet(A, 10, out _));
    }

    [Fact]
    public void AStoredDefinitionIsFoundAgainWithoutAnotherRead()
    {
        var cache = new WorkspaceDefinitionCache();
        var entry = new CachedDefinition(true, new DefinitionFact(true, "RACK-1"));

        cache.Store(A, 10, entry);

        Assert.True(cache.TryGet(A, 10, out var found));
        Assert.Equal(entry, found);
        Assert.Equal(1, cache.Count);
    }

    [Fact]
    public void AnEntryThatIsNotARackIsRememberedAsNotARack()
    {
        var cache = new WorkspaceDefinitionCache();
        cache.Store(A, 10, new CachedDefinition(false, default));

        Assert.True(cache.TryGet(A, 10, out var found));
        Assert.False(found.IsRack);
    }

    [Fact]
    public void TheSameDefinitionKeyInAnotherSessionIsADifferentEntry()
    {
        var cache = new WorkspaceDefinitionCache();
        cache.Store(A, 10, new CachedDefinition(true, new DefinitionFact(true, "FROM-A")));

        Assert.False(cache.TryGet(B, 10, out _));

        cache.Store(B, 10, new CachedDefinition(true, new DefinitionFact(true, "FROM-B")));
        Assert.True(cache.TryGet(A, 10, out var a));
        Assert.True(cache.TryGet(B, 10, out var b));
        Assert.Equal("FROM-A", a.Fact.EnvelopeId);
        Assert.Equal("FROM-B", b.Fact.EnvelopeId);
    }

    [Fact]
    public void StoringAgainReplacesTheEntry()
    {
        var cache = new WorkspaceDefinitionCache();
        cache.Store(A, 10, new CachedDefinition(true, new DefinitionFact(true, "OLD")));
        cache.Store(A, 10, new CachedDefinition(true, new DefinitionFact(true, "NEW")));

        Assert.True(cache.TryGet(A, 10, out var found));
        Assert.Equal("NEW", found.Fact.EnvelopeId);
        Assert.Equal(1, cache.Count);
    }

    [Fact]
    public void InvalidatingASessionForgetsOnlyThatSession()
    {
        var cache = new WorkspaceDefinitionCache();
        cache.Store(A, 1, new CachedDefinition(false, default));
        cache.Store(A, 2, new CachedDefinition(false, default));
        cache.Store(B, 1, new CachedDefinition(false, default));

        cache.Invalidate(A);

        Assert.Equal(0, cache.CountFor(A));
        Assert.Equal(1, cache.CountFor(B));
        Assert.False(cache.TryGet(A, 1, out _));
        Assert.True(cache.TryGet(B, 1, out _));
    }

    [Fact]
    public void InvalidatingEverythingEmptiesTheCache()
    {
        var cache = new WorkspaceDefinitionCache();
        cache.Store(A, 1, new CachedDefinition(false, default));
        cache.Store(B, 2, new CachedDefinition(false, default));

        cache.InvalidateAll();

        Assert.Equal(0, cache.Count);
    }

    [Fact]
    public void InvalidatingASessionThatWasNeverStoredChangesNothing()
    {
        var cache = new WorkspaceDefinitionCache();
        cache.Store(A, 1, new CachedDefinition(false, default));

        cache.Invalidate(new SessionId(99));

        Assert.Equal(1, cache.Count);
    }
}
