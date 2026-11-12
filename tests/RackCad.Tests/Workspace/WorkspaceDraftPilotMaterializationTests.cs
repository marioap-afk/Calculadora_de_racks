#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.PushBack.Pilot;
using RackCad.Application.Workspace.Drafts;
using RackCad.Domain.Systems.PushBack;
using Xunit;

namespace RackCad.Tests;

// I-64 F4-T2A-PILOT-STATE: INV-F4T2A-06 (materialization), 07 (composition to write) and 08 (round trip) of the pilot.
public class WorkspaceDraftPilotMaterializationTests
{
    private static readonly PushBackPilotStateComparer Comparer = new();
    private const string ScalePath = "$.PushBack.Structure.AnnotationScale";

    private static string Rewrite(string text, Action<RackEmbedDocument> edit)
    {
        var store = new RackEmbedStore();
        var embed = store.Deserialize(text)!;
        edit(embed);
        return store.Serialize(embed);
    }

    private static PushBackDesign Assemble(PushBackPilotComposition composition)
    {
        Assert.True(composition.IsComposed, composition.Reason);
        return new PushBackCompositeEditorAssembler(PilotKit.Catalog).BuildDesign(composition.Editor!, composition.Inputs!);
    }

    private static string WriteFrom(PushBackPilotDraftState initial, PushBackPilotDraftState current) =>
        PilotKit.Persist(Assemble(PushBackPilotMaterializer.ComposeForWrite(initial, current, PilotKit.Catalog)));

    /// <summary>
    /// A canonical variant of a fixture: the state it materializes to, edited through the editor API, assembled by the product
    /// and put back in the same envelope. Its text is what the product itself would write for that content.
    /// </summary>
    private static string Variant(string fixture, Action<PushBackPilotDraftState> edit)
    {
        var state = PilotKit.State(fixture);
        edit(state);
        var design = new PushBackCompositeEditorAssembler(PilotKit.Catalog).BuildDesign(state.Editor, state.Inputs);
        return Rewrite(PilotKit.Text(fixture), embed => embed.Design = new RackProjectStore().Serialize(RackProject.ForPushBack(design)));
    }

    /// <summary>A list that throws as soon as anyone looks at it: a catalog that fails in the middle of a load.</summary>
    private sealed class ThrowingList<T> : IReadOnlyList<T>
    {
        public int Count => throw new InvalidOperationException("catalog-boom");

        public T this[int index] => throw new InvalidOperationException("catalog-boom");

        public IEnumerator<T> GetEnumerator() => throw new InvalidOperationException("catalog-boom");

        IEnumerator IEnumerable.GetEnumerator() => throw new InvalidOperationException("catalog-boom");
    }

    private static RackCatalog ThrowingCatalog()
    {
        var real = PilotKit.Catalog;
        return new RackCatalog
        {
            PostProfiles = new ThrowingList<ProfileCatalogEntry>(),
            BeamProfiles = new ThrowingList<BeamProfileCatalogEntry>(),
            SafetyElements = new ThrowingList<SafetyElementCatalogEntry>(),
            TrussProfiles = real.TrussProfiles,
            BasePlates = real.BasePlates,
            FlowBedProfiles = real.FlowBedProfiles,
            Mensulas = real.Mensulas,
            SpacerProfiles = real.SpacerProfiles,
            ConnectionPoints = real.ConnectionPoints,
            ConnectionLayout = real.ConnectionLayout,
            Views = real.Views,
            Blocks = real.Blocks,
            Defaults = real.Defaults,
        };
    }

    // ------------------------------------------------------------------ INV-F4T2A-06: the typed materialization

    [Theory]
    [MemberData(nameof(Corpus))]
    public void ASingleSidedEnvelopeIsAdmittedAndCarriesItsRawTextAsTheOrigin(string fixture)
    {
        var text = PilotKit.Text(fixture);

        var result = PushBackPilotMaterializer.Materialize(text, PilotKit.Catalog);

        Assert.Equal(PushBackPilotMaterializationKind.Admitted, result.Kind);
        Assert.True(result.IsAdmitted);
        Assert.Null(result.Reason);
        Assert.Same(text, result.State!.Origin);
        Assert.False(result.State.Editor.SideBPresent);
        Assert.Equal(PushBackSide.A, result.State.Editor.ActiveSide);
        Assert.Empty(result.State.Editor.DormantCompositeTail);
        Assert.Empty(result.State.Editor.DormantTailLineOverrides);
    }

    public static IEnumerable<object[]> Corpus() =>
        PilotKit.Canonical.Concat(PilotKit.Legacy).Select(name => new object[] { name });

    [Fact]
    public void ATwoSidedRackIsOutOfThePilotWithAReason()
    {
        var result = PushBackPilotMaterializer.Materialize(PilotKit.Text("pb-20-ab"), PilotKit.Catalog);

        Assert.Equal(PushBackPilotMaterializationKind.OutOfPilot, result.Kind);
        Assert.Null(result.State);
        Assert.Equal(PushBackPilotMaterializer.CompositeOutOfPilotReason, result.Reason);
    }

    [Fact]
    public void ADesignThatLeavesADormantTailParkedIsOutOfThePilot()
    {
        var result = PushBackPilotMaterializer.Materialize(PilotKit.Text("pb-21-cola"), PilotKit.Catalog);

        Assert.Equal(PushBackPilotMaterializationKind.OutOfPilot, result.Kind);
        Assert.Null(result.State);
        Assert.Equal(PushBackPilotMaterializer.DormantTailOutOfPilotReason, result.Reason);
    }

    [Fact]
    public void AnUnreadableEnvelopeFails()
    {
        foreach (var text in new[] { "", "   ", "not json", "{", "[]", "null" })
        {
            var result = PushBackPilotMaterializer.Materialize(text, PilotKit.Catalog);
            Assert.Equal(PushBackPilotMaterializationKind.Failed, result.Kind);
            Assert.Null(result.State);
            Assert.False(string.IsNullOrEmpty(result.Reason));
        }

        Assert.Equal(PushBackPilotMaterializationKind.Failed, PushBackPilotMaterializer.Materialize(null!, PilotKit.Catalog).Kind);
    }

    [Fact]
    public void ANewerEnvelopeVersionFailsWithTheSingleUnreadableOrFutureDiagnostic()
    {
        var future = Rewrite(PilotKit.Text("pb-01-base"), embed => embed.SchemaVersion = "9.0");

        var result = PushBackPilotMaterializer.Materialize(future, PilotKit.Catalog);

        Assert.Equal(PushBackPilotMaterializationKind.Failed, result.Kind);
        Assert.Equal(PushBackPilotMaterializer.UnreadableOrFutureDiagnostic, result.Reason);
    }

    [Fact]
    public void AnotherKindFails()
    {
        var other = Rewrite(PilotKit.Text("pb-01-base"), embed => embed.Kind = RackEmbedDocument.KindSelective);

        var result = PushBackPilotMaterializer.Materialize(other, PilotKit.Catalog);

        Assert.Equal(PushBackPilotMaterializationKind.Failed, result.Kind);
        Assert.Equal(PushBackPilotMaterializer.KindDivergentDiagnostic, result.Reason);
    }

    [Fact]
    public void AKindWithDifferentCaseIsAnotherKind()
    {
        var other = Rewrite(PilotKit.Text("pb-01-base"), embed => embed.Kind = "PushBack");

        Assert.Equal(PushBackPilotMaterializationKind.Failed, PushBackPilotMaterializer.Materialize(other, PilotKit.Catalog).Kind);
    }

    [Fact]
    public void ANewerInnerDesignMajorFailsWithTheSingleUnreadableOrFutureDiagnostic()
    {
        var future = Rewrite(PilotKit.Text("pb-01-base"), embed =>
        {
            var inner = JsonNode.Parse(embed.Design)!.AsObject();
            inner["SchemaVersion"] = "9.0";
            embed.Design = inner.ToJsonString();
        });

        var result = PushBackPilotMaterializer.Materialize(future, PilotKit.Catalog);

        Assert.Equal(PushBackPilotMaterializationKind.Failed, result.Kind);
        Assert.Equal(PushBackPilotMaterializer.UnreadableOrFutureDiagnostic, result.Reason);
    }

    [Fact]
    public void AnInnerDesignThatCannotBeReadFailsWithTheSingleUnreadableOrFutureDiagnostic()
    {
        var broken = Rewrite(PilotKit.Text("pb-01-base"), embed => embed.Design = "{ this is not json");
        var empty = Rewrite(PilotKit.Text("pb-01-base"), embed => embed.Design = string.Empty);
        var scalar = Rewrite(PilotKit.Text("pb-01-base"), embed => embed.Design = "42");

        foreach (var text in new[] { broken, empty, scalar })
        {
            var result = PushBackPilotMaterializer.Materialize(text, PilotKit.Catalog);
            Assert.Equal(PushBackPilotMaterializationKind.Failed, result.Kind);
            Assert.Equal(PushBackPilotMaterializer.UnreadableOrFutureDiagnostic, result.Reason);
        }
    }

    [Fact]
    public void AReadableProjectWithoutAPushBackDesignFailsWithTheNoDesignDiagnostic()
    {
        // A readable project of another system under a Push Back envelope: TryDeserialize succeeds and there is no Push Back design.
        var noDesign = Rewrite(PilotKit.Text("pb-01-base"), embed =>
            embed.Design = new RackProjectStore().Serialize(RackProject.ForDynamic(DimensionViewScenarios.DynamicPersistedDesign(RackCad.Domain.Systems.Shared.DimensionDetail.Standard, PilotKit.Catalog))));
        Assert.Null(new RackProjectStore().Deserialize(new RackEmbedStore().Deserialize(noDesign)!.Design).PushBackDesign);

        var result = PushBackPilotMaterializer.Materialize(noDesign, PilotKit.Catalog);

        Assert.Equal(PushBackPilotMaterializationKind.Failed, result.Kind);
        Assert.Equal(PushBackPilotMaterializer.NoDesignDiagnostic, result.Reason);
    }

    [Fact]
    public void AnInnerDesignWithANullPushBackBlockIsUnreadableForTheStoreAndGetsTheSingleDiagnostic()
    {
        var nullBlock = Rewrite(PilotKit.Text("pb-01-base"), embed =>
        {
            var inner = JsonNode.Parse(embed.Design)!.AsObject();
            inner["PushBack"] = null;
            embed.Design = inner.ToJsonString();
        });

        var result = PushBackPilotMaterializer.Materialize(nullBlock, PilotKit.Catalog);

        Assert.Equal(PushBackPilotMaterializationKind.Failed, result.Kind);
        Assert.Equal(PushBackPilotMaterializer.UnreadableOrFutureDiagnostic, result.Reason);
    }

    [Fact]
    public void AMissingCatalogFailsInsteadOfThrowing()
    {
        var result = PushBackPilotMaterializer.Materialize(PilotKit.Text("pb-01-base"), null!);

        Assert.Equal(PushBackPilotMaterializationKind.Failed, result.Kind);
        Assert.Equal(PushBackPilotMaterializer.CatalogRequiredDiagnostic, result.Reason);
    }

    [Fact]
    public void AnExceptionInsideMaterializeBecomesAFailedResultWithItsPrefixAndNeverAdmitted()
    {
        PushBackPilotMaterialization? result = null;

        var exception = Record.Exception(() => result = PushBackPilotMaterializer.Materialize(PilotKit.Text("pb-01-base"), ThrowingCatalog()));

        Assert.Null(exception);
        Assert.NotNull(result);
        Assert.Equal(PushBackPilotMaterializationKind.Failed, result!.Kind);
        Assert.Null(result.State);
        Assert.StartsWith(PushBackPilotMaterializer.MaterializeFailurePrefix, result.Reason, StringComparison.Ordinal);
        Assert.Contains("catalog-boom", result.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void AnExceptionInsideTheBaseOfComposeForWriteIsARejectionWithTheExceptionPrefix()
    {
        var initial = PilotKit.State("pb-01-base");
        var current = PilotKit.State("pb-01-base");
        PushBackPilotComposition? composition = null;

        var exception = Record.Exception(() => composition = PushBackPilotMaterializer.ComposeForWrite(initial, current, ThrowingCatalog()));

        Assert.Null(exception);
        Assert.NotNull(composition);
        Assert.False(composition!.IsComposed);
        Assert.Null(composition.Editor);
        Assert.Null(composition.Inputs);
        Assert.StartsWith(PushBackPilotMaterializer.MaterializeFailurePrefix, composition.Reason, StringComparison.Ordinal);
    }

    [Fact]
    public void AnExceptionInsideComposeForWriteItselfIsARejectionWithTheComposePrefix()
    {
        var initial = PilotKit.State("pb-01-base");
        var current = PilotKit.State("pb-01-base");
        current.Editor.SideA.Structure.Fronts[0].Cells[0] = null!;
        PushBackPilotComposition? composition = null;

        var exception = Record.Exception(() => composition = PushBackPilotMaterializer.ComposeForWrite(initial, current, PilotKit.Catalog));

        Assert.Null(exception);
        Assert.NotNull(composition);
        Assert.False(composition!.IsComposed);
        Assert.StartsWith(PushBackPilotMaterializer.ComposeFailurePrefix, composition.Reason, StringComparison.Ordinal);
    }

    /// <summary>The Application part of RackPushBackSystemWindow.LoadExisting, by hand and in its order, tail parking included.</summary>
    private static PushBackPilotDraftState LoadByHand(string text, bool parkTheTail)
    {
        var design = new RackProjectStore().Deserialize(PilotKit.DesignOf(text)).PushBackDesign;
        var resolver = new PushBackResolver(PilotKit.Catalog);

        // SetActiveSide(A), SetSideBPresent(false), LoadFromDesign, SetSideBPresent(false), LoadComposite, ClearDormantTail,
        // ParkDormantTail with the ids of the tail, AlignSlotGrid.
        var editor = new PushBackCompositeEditorState();
        editor.SetActiveSide(PushBackSide.A);
        editor.SetSideBPresent(false);
        var inputs = editor.SideA.LoadFromDesign(design, resolver);
        editor.SetSideBPresent(false);
        editor.LoadComposite(design.Composite);
        editor.ClearDormantTail();
        var structure = design.Structure;
        if (parkTheTail && structure != null)
        {
            editor.ParkDormantTail(
                structure.Modules.Where(module => module != null && PushBackCompositeStructure.IsCompositeTailId(module.ModuleId)),
                structure.HeaderLineOverrides.Where(line => line != null && PushBackCompositeStructure.IsCompositeTailId(line.ModuleId)));
        }

        editor.AlignSlotGrid();
        return new PushBackPilotDraftState(editor, inputs, text);
    }

    [Fact]
    public void MaterializeReproducesTheApplicationPartOfTheWindowLoadInTheSameOrder()
    {
        var text = PilotKit.Text("pb-02-modulos");
        var byHand = LoadByHand(text, parkTheTail: true);

        var materialized = PilotKit.Admitted(text);

        Assert.Empty(byHand.Editor.DormantCompositeTail);
        Assert.Empty(byHand.Editor.DormantTailLineOverrides);
        Assert.True(Comparer.Equals(byHand, materialized));
        Assert.Equal(byHand.Editor.SideA.Structure.SelectedFrontIndex, materialized.Editor.SideA.Structure.SelectedFrontIndex);
        Assert.Equal(byHand.Editor.SideA.Structure.SelectedLevelIndex, materialized.Editor.SideA.Structure.SelectedLevelIndex);
        Assert.NotNull(materialized.Editor.SideA.WorkingBaseline);
        Assert.Equal(byHand.Editor.SideA.WorkingBaseline!.Structure.Modules.Count, materialized.Editor.SideA.WorkingBaseline!.Structure.Modules.Count);
    }

    [Fact]
    public void TheParkingOfTheDormantTailIsAStepOfTheLoadThatDecidesWhetherTheDesignIsInThePilot()
    {
        var text = PilotKit.Text("pb-21-cola");

        var parked = LoadByHand(text, parkTheTail: true);
        var skipped = LoadByHand(text, parkTheTail: false);
        var result = PushBackPilotMaterializer.Materialize(text, PilotKit.Catalog);

        // Without the parking step the window would hold no tail; with it the tail is parked, and that is what takes the design out of the pilot.
        Assert.Empty(skipped.Editor.DormantCompositeTail);
        Assert.Empty(skipped.Editor.DormantTailLineOverrides);
        Assert.True(parked.Editor.DormantCompositeTail.Count > 0 || parked.Editor.DormantTailLineOverrides.Count > 0);
        Assert.Equal(PushBackPilotMaterializationKind.OutOfPilot, result.Kind);
        Assert.Equal(PushBackPilotMaterializer.DormantTailOutOfPilotReason, result.Reason);
    }

    [Fact]
    public void MaterializeKeepsTheAuthoredValueInsteadOfTheValueTheControlsDisplay()
    {
        // The window shows 1.23456 as "1.235" through its "0.###" controls; the pilot keeps what the document says.
        var text = Rewrite(PilotKit.Text("pb-01-base"), embed =>
        {
            var project = new RackProjectStore().Deserialize(embed.Design);
            project.PushBackDesign.Structure.AnnotationScale = 1.23456;
            embed.Design = new RackProjectStore().Serialize(project);
        });

        Assert.Equal(1.23456, PilotKit.Admitted(text).Inputs.Annotations.AnnotationScale);
    }

    [Fact]
    public void MaterializeNeverMutatesTheTextItReceives()
    {
        var text = PilotKit.Text("pb-02-modulos");
        var copy = new string(text.AsSpan());

        var one = PilotKit.Admitted(text);
        var two = PilotKit.Admitted(text);

        Assert.Equal(copy, text);
        Assert.NotSame(one.Editor, two.Editor);
        Assert.NotSame(one.Inputs, two.Inputs);
        Assert.Same(text, one.Origin);
    }

    // ------------------------------------------------------------------ INV-F4T2A-07: the composition to write

    [Fact]
    public void ComposeForWriteIsPureAndMutatesNeitherState()
    {
        var initial = PilotKit.State("pb-02-modulos");
        var current = PilotKit.State("pb-02-modulos");
        current.Inputs.Annotations.AnnotationScale = 2.75;
        var initialCopy = PushBackPilotStateComparer.Copy(initial);
        var currentCopy = PushBackPilotStateComparer.Copy(current);
        var baseline = current.Editor.SideA.WorkingBaseline;
        var session = initial.Editor.SideA.ModuleSession;

        var first = PushBackPilotMaterializer.ComposeForWrite(initial, current, PilotKit.Catalog);
        Assemble(first);
        var second = PushBackPilotMaterializer.ComposeForWrite(initial, current, PilotKit.Catalog);

        Assert.True(Comparer.Equals(initialCopy, initial));
        Assert.True(Comparer.Equals(currentCopy, current));
        Assert.Same(baseline, current.Editor.SideA.WorkingBaseline);
        Assert.Same(session, initial.Editor.SideA.ModuleSession);
        Assert.Equal(PilotKit.Text("pb-02-modulos"), initial.Origin);
        Assert.NotSame(initial.Editor, first.Editor);
        Assert.NotSame(current.Editor, first.Editor);
        Assert.NotSame(current.Inputs, first.Inputs);
        Assert.NotSame(first.Editor, second.Editor);
        Assert.Equal(PilotKit.Persist(Assemble(first)), PilotKit.Persist(Assemble(second)));
    }

    [Fact]
    public void ComposeForWriteIgnoresTheOriginOfTheCurrentState()
    {
        var initial = PilotKit.State("pb-01-base");
        var current = PilotKit.State("pb-01-base");
        current.Inputs.Annotations.AnnotationScale = 2.5;
        var foreign = new PushBackPilotDraftState(current.Editor, current.Inputs, "not an envelope at all");

        var withRealOrigin = WriteFrom(initial, current);
        var withForeignOrigin = WriteFrom(initial, foreign);

        Assert.Equal(withRealOrigin, withForeignOrigin);
    }

    [Fact]
    public void ComposeForWriteIsTotalAndRejectsABaseThatNoLongerMaterializes()
    {
        var good = PilotKit.State("pb-01-base");
        var cases = new List<PushBackPilotComposition>
        {
            PushBackPilotMaterializer.ComposeForWrite(null!, good, PilotKit.Catalog),
            PushBackPilotMaterializer.ComposeForWrite(good, null!, PilotKit.Catalog),
            PushBackPilotMaterializer.ComposeForWrite(good, good, null!),
            PushBackPilotMaterializer.ComposeForWrite(new PushBackPilotDraftState(good.Editor, good.Inputs, "garbage"), good, PilotKit.Catalog),
            PushBackPilotMaterializer.ComposeForWrite(new PushBackPilotDraftState(good.Editor, good.Inputs, PilotKit.Text("pb-20-ab")), good, PilotKit.Catalog),
            PushBackPilotMaterializer.ComposeForWrite(new PushBackPilotDraftState(good.Editor, good.Inputs, PilotKit.Text("pb-21-cola")), good, PilotKit.Catalog),
        };

        Assert.All(cases, composition =>
        {
            Assert.False(composition.IsComposed);
            Assert.Null(composition.Editor);
            Assert.Null(composition.Inputs);
            Assert.False(string.IsNullOrEmpty(composition.Reason));
        });
    }

    [Fact]
    public void ComposeForWriteTakesWhatIsEditedFromTheDraftAndWhatIsNotFromTheCurrentBase()
    {
        // The initial state is the base of V2 (custom header); the current state is V1 with an edited scale.
        var v2 = PilotKit.Text("pb-31-v2-modulos");
        var initial = PilotKit.Admitted(v2);
        var current = PilotKit.State("pb-01-base");
        current.Inputs.Annotations.AnnotationScale = 2.25;

        var written = WriteFrom(initial, current);

        Assert.Equal(new[] { ScalePath + ": 1.5 -> 2.25" }, PilotKit.JsonDiff(PilotKit.DesignOf(v2), written));
        Assert.NotEqual(PilotKit.DesignOf(PilotKit.Text("pb-01-base")), written);
    }

    [Fact]
    public void ACleanDraftComposesExactlyItsBase()
    {
        foreach (var fixture in PilotKit.Canonical)
        {
            var text = PilotKit.Text(fixture);
            var registry = new DraftRegistry();
            var draft = registry.Begin(
                DraftKit.S1, "R1", DraftIntentRead<PushBackPilotDraftState>.Admitted(DraftKit.DefaultBase(), PilotKit.Admitted(text)),
                Comparer, PushBackPilotStateComparer.Copy).Draft!;

            Assert.False(draft.IsDirty);
            Assert.Equal(PilotKit.DesignOf(text), WriteFrom(draft.InitialDraftState, draft.CurrentDraftState));
        }
    }

    private static DraftSession<PushBackPilotDraftState> BeginOn(string text) =>
        new DraftRegistry().Begin(
            DraftKit.S1, "R1", DraftIntentRead<PushBackPilotDraftState>.Admitted(DraftKit.DefaultBase(), PilotKit.Admitted(text)),
            Comparer, PushBackPilotStateComparer.Copy).Draft!;

    [Fact]
    public void ComposeForWriteRejectsAnyDifferenceOtherThanTheScaleAndNeverRevertsOrWritesItInSilence()
    {
        var initial = PilotKit.State("pb-01-base");
        var edits = new Dictionary<string, Action<PushBackPilotDraftState>>
        {
            ["PalletTolerance"] = state => state.Inputs.PalletTolerance += 1.0,
            ["DimensionStyle"] = state => state.Inputs.Annotations.DimensionStyle = "SIMPLEX",
            ["PalletHeightOfACell"] = state => state.Editor.SideA.Structure.Fronts[0].Cells[0].PalletHeight += 2.0,
            ["FrontCount"] = state => state.Editor.SideA.SetFrontCount(2),
            ["RearTopeSaque"] = state => state.Editor.SideA.RearTopeSaque += 1.5,
            ["PostCatalogId"] = state => state.Inputs.PostCatalogId = "POSTE-OTRO",
        };

        foreach (var (name, edit) in edits)
        {
            var current = PilotKit.State("pb-01-base");
            edit(current);
            Assert.False(Comparer.Equals(initial, current), name);

            var composition = PushBackPilotMaterializer.ComposeForWrite(initial, current, PilotKit.Catalog);

            Assert.False(composition.IsComposed, name);
            Assert.Null(composition.Editor);
            Assert.Null(composition.Inputs);
            Assert.Equal(PushBackPilotMaterializer.NonPilotContentReason, composition.Reason);
        }

        // The scale together with something else is still a rejection; the scale alone is not.
        var both = PilotKit.State("pb-01-base");
        both.Inputs.Annotations.AnnotationScale = 3.0;
        both.Inputs.PalletTolerance += 1.0;
        Assert.False(PushBackPilotMaterializer.ComposeForWrite(initial, both, PilotKit.Catalog).IsComposed);
        var onlyScale = PilotKit.State("pb-01-base");
        onlyScale.Inputs.Annotations.AnnotationScale = 3.0;
        Assert.True(PushBackPilotMaterializer.ComposeForWrite(initial, onlyScale, PilotKit.Catalog).IsComposed);
    }

    [Fact]
    public void AnInPlaceEditOfTheCurrentEditorOutsideThePilotMakesComposeForWriteReject()
    {
        var draft = BeginOn(PilotKit.Text("pb-01-base"));
        Assert.True(PushBackPilotMaterializer.ComposeForWrite(draft.InitialDraftState, draft.CurrentDraftState, PilotKit.Catalog).IsComposed);

        draft.CurrentDraftState.Editor.SideA.SetFrontCount(draft.CurrentDraftState.Editor.SideA.Structure.Count + 1);

        Assert.True(draft.IsDirty);
        var composition = PushBackPilotMaterializer.ComposeForWrite(draft.InitialDraftState, draft.CurrentDraftState, PilotKit.Catalog);
        Assert.False(composition.IsComposed);
        Assert.Equal(PushBackPilotMaterializer.NonPilotContentReason, composition.Reason);
    }

    [Fact]
    public void KeepPilotEditsOnIsPureAndGivesTheNewBaseWithTheScaleOfTheDraft()
    {
        var newInitial = PilotKit.State("pb-02-modulos");
        newInitial.Inputs.PalletTolerance = 7.25;
        var current = PilotKit.State("pb-01-base");
        current.Inputs.Annotations.AnnotationScale = 2.25;
        current.Inputs.PalletTolerance = 9.0;
        var newInitialBefore = PushBackPilotStateComparer.Copy(newInitial);
        var currentBefore = PushBackPilotStateComparer.Copy(current);

        var kept = PushBackPilotMaterializer.KeepPilotEditsOn(newInitial, current);

        Assert.NotSame(newInitial, kept);
        Assert.NotSame(current, kept);
        Assert.NotSame(newInitial.Editor, kept.Editor);
        Assert.NotSame(newInitial.Inputs, kept.Inputs);
        Assert.NotSame(current.Inputs, kept.Inputs);
        Assert.Equal(newInitial.Origin, kept.Origin);
        Assert.Equal(2.25, kept.Inputs.Annotations.AnnotationScale);
        Assert.Equal(7.25, kept.Inputs.PalletTolerance);
        Assert.Equal(2.0, newInitial.Inputs.Annotations.AnnotationScale);
        Assert.True(Comparer.Equals(newInitialBefore, newInitial));
        Assert.True(Comparer.Equals(currentBefore, current));
        Assert.Equal(2.25, current.Inputs.Annotations.AnnotationScale);

        var withTheBaseScale = PushBackPilotStateComparer.Copy(newInitial);
        withTheBaseScale.Inputs.Annotations.AnnotationScale = 2.25;
        Assert.True(Comparer.Equals(withTheBaseScale, kept));
        Assert.Throws<ArgumentNullException>(() => PushBackPilotMaterializer.KeepPilotEditsOn(null!, current));
        Assert.Throws<ArgumentNullException>(() => PushBackPilotMaterializer.KeepPilotEditsOn(newInitial, null!));
    }

    [Fact]
    public void KeepMineAfterAnExternalModuleChangeWritesTheNewModulesAndTheScaleOfTheDraftAndReturnsToCleanWhenTheScaleIsRestored()
    {
        var v1 = PilotKit.Text("pb-01-base");
        var v2 = PilotKit.Text("pb-31-v2-modulos");
        var draft = BeginOn(v1);

        // 1. V1 is open and the scale is edited.
        draft.CurrentDraftState.Inputs.Annotations.AnnotationScale = 2.25;
        Assert.True(draft.IsDirty);

        // 2. An external V2 changes ONLY the module layer (a custom header), with the same scale as V1.
        var second = PilotKit.Admitted(v2);
        Assert.Equal(1.5, second.Inputs.Annotations.AnnotationScale);
        Assert.True(Comparer.Equals(PilotKit.Admitted(v1), second));
        Assert.NotEqual(PilotKit.DesignOf(v1), PilotKit.DesignOf(v2));

        // 3. The user keeps the draft on the current version: the initial state moves to V2, and the surface sets as the
        // current state the new base with the scale of the draft.
        var keep = draft.KeepMineOnCurrentVersion(
            DraftIntentRead<PushBackPilotDraftState>.Admitted(DraftKit.DefaultBase(), second), confirmed: true);
        Assert.Equal(DraftReloadStatus.Reloaded, keep.Status);
        Assert.Equal(v2, draft.InitialDraftState.Origin);
        Assert.True(draft.IsDirty);
        Assert.Equal(DraftEditStatus.Updated, draft.SetCurrentDraftState(PushBackPilotMaterializer.KeepPilotEditsOn(second, draft.CurrentDraftState)).Status);
        Assert.True(draft.IsDirty);
        Assert.Equal(2.25, draft.CurrentDraftState.Inputs.Annotations.AnnotationScale);

        // 4. The composition carries the header of V2 and the scale of the draft.
        var composed = WriteFrom(draft.InitialDraftState, draft.CurrentDraftState);
        Assert.Equal(new[] { ScalePath + ": 1.5 -> 2.25" }, PilotKit.JsonDiff(PilotKit.DesignOf(v2), composed));
        Assert.Contains(PilotKit.JsonDiff(PilotKit.DesignOf(v1), composed), difference => difference.StartsWith("$.PushBack.Structure.Modules[2]", StringComparison.Ordinal));

        // 5. Giving the scale back its original value leaves the draft Clean and the composition identical to V2.
        draft.CurrentDraftState.Inputs.Annotations.AnnotationScale = 1.5;
        Assert.False(draft.IsDirty);
        Assert.Equal(DraftStatus.Clean, draft.Status);
        Assert.Equal(PilotKit.DesignOf(v2), WriteFrom(draft.InitialDraftState, draft.CurrentDraftState));
    }

    [Fact]
    public void KeepMineAfterAnExternalChangeOfContentThePilotDoesNotEditNeverRevertsItAndComposesTheNewContentPlusTheScaleOfTheDraft()
    {
        var v1 = PilotKit.Text("pb-01-base");
        var v2 = Variant("pb-01-base", state =>
        {
            state.Inputs.PalletTolerance += 2.0;
            state.Editor.SideA.Structure.Fronts[0].Cells[0].PalletHeight += 3.0;
            state.Inputs.Annotations.DimensionStyle = "SIMPLEX";
        });

        // V2 is a design of its own and changes EDITABLE content the pilot does not edit.
        var changed = PilotKit.JsonDiff(PilotKit.DesignOf(v1), PilotKit.DesignOf(v2));
        Assert.Contains("$.PushBack.Structure.PalletTolerance: 4 -> 6", changed);
        Assert.Contains("$.PushBack.Structure.DimensionStyle: null -> \"SIMPLEX\"", changed);
        Assert.Contains(changed, difference => difference.StartsWith("$.PushBack.Structure.Fronts[0].Levels[0].PalletHeight: ", StringComparison.Ordinal));
        var second = PilotKit.Admitted(v2);
        Assert.False(Comparer.Equals(PilotKit.Admitted(v1), second));
        Assert.Equal(PilotKit.DesignOf(v2), WriteFrom(second, second));

        // 1. V1 is open and the scale is edited; 2. the user keeps the draft on V2, with no rebase of the current state.
        var draft = BeginOn(v1);
        draft.CurrentDraftState.Inputs.Annotations.AnnotationScale = 2.25;
        var keep = draft.KeepMineOnCurrentVersion(
            DraftIntentRead<PushBackPilotDraftState>.Admitted(DraftKit.DefaultBase(), second), confirmed: true);
        Assert.Equal(DraftReloadStatus.Reloaded, keep.Status);

        // 3. Composing now would silently revert V2 to V1: it is a typed rejection.
        var rejected = PushBackPilotMaterializer.ComposeForWrite(draft.InitialDraftState, draft.CurrentDraftState, PilotKit.Catalog);
        Assert.False(rejected.IsComposed);
        Assert.Equal(PushBackPilotMaterializer.NonPilotContentReason, rejected.Reason);

        // 4. The surface rebases the current state with KeepPilotEditsOn: V2 plus the scale of the draft.
        draft.SetCurrentDraftState(PushBackPilotMaterializer.KeepPilotEditsOn(second, draft.CurrentDraftState));
        Assert.True(draft.IsDirty);
        var composed = WriteFrom(draft.InitialDraftState, draft.CurrentDraftState);
        Assert.Equal(new[] { ScalePath + ": 1.5 -> 2.25" }, PilotKit.JsonDiff(PilotKit.DesignOf(v2), composed));

        // 5. Giving the scale back leaves the draft Clean and the composition identical to V2.
        draft.CurrentDraftState.Inputs.Annotations.AnnotationScale = 1.5;
        Assert.False(draft.IsDirty);
        Assert.Equal(DraftStatus.Clean, draft.Status);
        Assert.Equal(PilotKit.DesignOf(v2), WriteFrom(draft.InitialDraftState, draft.CurrentDraftState));
    }

    // ------------------------------------------------------------------ INV-F4T2A-08: the round trip

    private static string RoundTrip(string text, double? editedScale)
    {
        var initial = PilotKit.Admitted(text);
        var current = PushBackPilotStateComparer.Copy(initial);
        if (editedScale.HasValue) current.Inputs.Annotations.AnnotationScale = editedScale.Value;
        return WriteFrom(initial, current);
    }

    [Fact]
    public void TheCorpusHasAtLeastSixCanonicalDesignsThatCoverTheFeaturesOfTheContract()
    {
        Assert.True(PilotKit.Canonical.Length >= 6);
        var all = PilotKit.Canonical.ToDictionary(name => name, name => JsonNode.Parse(PilotKit.DesignOf(PilotKit.Text(name)))!["PushBack"]!);
        Func<string, JsonNode> structure = name => all[name]["Structure"]!;

        Assert.Contains(PilotKit.Canonical, name => structure(name)["Modules"]!.AsArray().Any(module => module!["IsCalculated"]!.GetValue<bool>() == false));
        Assert.Contains(PilotKit.Canonical, name => all[name]["Fronts"]!.AsArray().Any(front => front!["PalletsDeepOverrides"] != null)
            && structure(name)["Fronts"]!.AsArray().Select(front => front!["PalletsDeep"]!.GetValue<int>()).Distinct().Count() > 1);
        Assert.Contains(PilotKit.Canonical, name => structure(name)["Fronts"]!.AsArray().Any(front => front!["IsActive"] != null && front["IsActive"]!.GetValue<bool>() == false));
        Assert.Contains(PilotKit.Canonical, name => all[name]["RearTopeOffCells"] != null);
        Assert.Contains(PilotKit.Canonical, name => structure(name)["SafetySelections"]!.AsArray().Count > 0);
        Assert.Contains(PilotKit.Canonical, name => structure(name)["ManualHeaderHeightOverride"] != null);
        Assert.Contains(PilotKit.Canonical, name => structure(name)["DimensionStyle"] != null);
        Assert.All(PilotKit.Canonical, name => Assert.NotEqual(1.0, structure(name)["AnnotationScale"]!.GetValue<double>()));
    }

    [Theory]
    [MemberData(nameof(CanonicalCorpus))]
    public void ACanonicalDesignComesBackAsTheIdenticalPersistedDocument(string fixture)
    {
        var text = PilotKit.Text(fixture);

        Assert.Equal(PilotKit.DesignOf(text), RoundTrip(text, editedScale: null));
    }

    [Theory]
    [MemberData(nameof(CanonicalCorpus))]
    public void EditingTheScaleOfACanonicalDesignChangesOnlyThatField(string fixture)
    {
        var text = PilotKit.Text(fixture);
        var before = JsonNode.Parse(PilotKit.DesignOf(text))!["PushBack"]!["Structure"]!["AnnotationScale"]!.GetValue<double>();

        var after = RoundTrip(text, editedScale: 4.125);

        Assert.Equal(
            new[] { ScalePath + ": " + before.ToString("R", System.Globalization.CultureInfo.InvariantCulture) + " -> 4.125" },
            PilotKit.JsonDiff(PilotKit.DesignOf(text), after));
    }

    public static IEnumerable<object[]> CanonicalCorpus() => PilotKit.Canonical.Select(name => new object[] { name });

    /// <summary>The exact canonicalization an older drawing gets when the pilot writes it (T2H-07), sorted.</summary>
    public static readonly IReadOnlyDictionary<string, string[]> LegacyDeltas = new Dictionary<string, string[]>
    {
        ["pb-10-sin-datum"] = new[] { "$.PushBack.Structure.FirstLevelDatum: <absent> -> 1" },
        ["pb-11-sin-i41"] = new[]
        {
            "$.PushBack.Fronts[0].DefaultPalletsDeep: <absent> -> 8", "$.PushBack.Fronts[0].DrawPallets: <absent> -> null", "$.PushBack.Fronts[0].PalletsDeepOverrides: <absent> -> null",
            "$.PushBack.Fronts[1].DefaultPalletsDeep: <absent> -> 8", "$.PushBack.Fronts[1].DrawPallets: <absent> -> null", "$.PushBack.Fronts[1].PalletsDeepOverrides: <absent> -> null",
            "$.PushBack.Fronts[2].DefaultPalletsDeep: <absent> -> 8", "$.PushBack.Fronts[2].DrawPallets: <absent> -> null", "$.PushBack.Fronts[2].PalletsDeepOverrides: <absent> -> null",
        },
        ["pb-12-poste-nulo"] = new[] { "$.PushBack.Structure.HeaderPostCatalogId: null -> \"POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA\"" },
        ["pb-13-sideb-no-presente"] = new[] { "$.PushBack.SideB.IsPresent: false -> <absent>" },
        ["pb-14-peralte-heredado"] = new[]
        {
            "$.PushBack.Fronts[0].HighEndBeamPeraltes[0]: null -> 5", "$.PushBack.Fronts[0].HighEndBeamPeraltes[1]: null -> 5", "$.PushBack.Fronts[0].HighEndBeamPeraltes[2]: null -> 5",
            "$.PushBack.Fronts[1].HighEndBeamPeraltes[0]: null -> 5", "$.PushBack.Fronts[1].HighEndBeamPeraltes[1]: null -> 5", "$.PushBack.Fronts[1].HighEndBeamPeraltes[2]: null -> 5",
            "$.PushBack.Fronts[2].HighEndBeamPeraltes[0]: null -> 5", "$.PushBack.Fronts[2].HighEndBeamPeraltes[1]: null -> 5", "$.PushBack.Fronts[2].HighEndBeamPeraltes[2]: null -> 5",
            "$.PushBack.LegacyHighEndBeamPeralte: 5 -> 3.5",
        },
        ["pb-15-sin-composite"] = new[]
        {
            "$.PushBack.Composite.AbsentSlotsA: <absent> -> null", "$.PushBack.Composite.AbsentSlotsB[0]: <absent> -> 0", "$.PushBack.Composite.CentralSeparator: <absent> -> null",
            "$.PushBack.Composite.DefaultDirection: <absent> -> \"AToB\"", "$.PushBack.Composite.DefaultTopology: <absent> -> \"SoloA\"", "$.PushBack.Composite.Gap: <absent> -> null",
            "$.PushBack.Composite.StructureOverrideA: <absent> -> null", "$.PushBack.Composite.StructureOverrideB: <absent> -> null", "$.PushBack.Composite.Topologies: <absent> -> null",
        },
    };

    public static IEnumerable<object[]> LegacyCorpus() => PilotKit.Legacy.Select(name => new object[] { name });

    [Fact]
    public void EveryLegacyFixtureHasItsExpectedDelta()
    {
        Assert.Equal(PilotKit.Legacy.OrderBy(name => name, StringComparer.Ordinal), LegacyDeltas.Keys.OrderBy(name => name, StringComparer.Ordinal));
    }

    [Theory]
    [MemberData(nameof(LegacyCorpus))]
    public void AnOlderDrawingComesBackWithExactlyTheCanonicalizationOfTheApplicationPath(string fixture)
    {
        var text = PilotKit.Text(fixture);

        var delta = PilotKit.JsonDiff(PilotKit.DesignOf(text), RoundTrip(text, editedScale: null));

        Assert.Equal(LegacyDeltas[fixture].OrderBy(line => line, StringComparer.Ordinal), delta.OrderBy(line => line, StringComparer.Ordinal));
    }

    [Theory]
    [MemberData(nameof(LegacyCorpus))]
    public void EditingTheScaleOfAnOlderDrawingAddsOnlyTheEditedFieldToTheCanonicalization(string fixture)
    {
        var text = PilotKit.Text(fixture);

        var delta = PilotKit.JsonDiff(PilotKit.DesignOf(text), RoundTrip(text, editedScale: 4.125));

        Assert.Equal(
            LegacyDeltas[fixture].Concat(new[] { ScalePath + ": 1.5 -> 4.125" }).OrderBy(line => line, StringComparer.Ordinal),
            delta.OrderBy(line => line, StringComparer.Ordinal));
    }
}
