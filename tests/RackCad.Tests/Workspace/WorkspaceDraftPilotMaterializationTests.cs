#nullable enable
using System;
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
    public void AnInnerDesignThatCannotBeReadOrHasNoPushBackFails()
    {
        var broken = Rewrite(PilotKit.Text("pb-01-base"), embed => embed.Design = "{ this is not json");
        var empty = Rewrite(PilotKit.Text("pb-01-base"), embed => embed.Design = string.Empty);
        var noDesign = Rewrite(PilotKit.Text("pb-01-base"), embed =>
        {
            var inner = JsonNode.Parse(embed.Design)!.AsObject();
            inner["PushBack"] = null;
            embed.Design = inner.ToJsonString();
        });

        foreach (var text in new[] { broken, empty, noDesign })
        {
            var result = PushBackPilotMaterializer.Materialize(text, PilotKit.Catalog);
            Assert.Equal(PushBackPilotMaterializationKind.Failed, result.Kind);
            Assert.False(string.IsNullOrEmpty(result.Reason));
        }
    }

    [Fact]
    public void AMissingCatalogFailsInsteadOfThrowing()
    {
        var result = PushBackPilotMaterializer.Materialize(PilotKit.Text("pb-01-base"), null!);

        Assert.Equal(PushBackPilotMaterializationKind.Failed, result.Kind);
        Assert.Equal(PushBackPilotMaterializer.CatalogRequiredDiagnostic, result.Reason);
    }

    [Fact]
    public void AFailureOfTheResolverBecomesAFailedResultAndNeverAnException()
    {
        var corrupt = Rewrite(PilotKit.Text("pb-01-base"), embed =>
        {
            var inner = JsonNode.Parse(embed.Design)!.AsObject();
            inner["PushBack"]!["Structure"]!["Fronts"] = new JsonArray();
            embed.Design = inner.ToJsonString();
        });

        PushBackPilotMaterialization? result = null;
        var exception = Record.Exception(() => result = PushBackPilotMaterializer.Materialize(corrupt, PilotKit.Catalog));

        Assert.Null(exception);
        Assert.NotNull(result);
        Assert.True(
            result!.Kind == PushBackPilotMaterializationKind.Failed || result.Kind == PushBackPilotMaterializationKind.Admitted,
            "the result is always typed");
    }

    [Fact]
    public void MaterializeReproducesTheApplicationPartOfTheWindowLoadInTheSameOrder()
    {
        var text = PilotKit.Text("pb-02-modulos");
        var design = new RackProjectStore().Deserialize(PilotKit.DesignOf(text)).PushBackDesign;
        var resolver = new PushBackResolver(PilotKit.Catalog);

        // The window, by hand: SetActiveSide(A), SetSideBPresent(false), LoadFromDesign, SetSideBPresent(false),
        // LoadComposite, ClearDormantTail, park the tail ids, AlignSlotGrid.
        var editor = new PushBackCompositeEditorState();
        editor.SetActiveSide(PushBackSide.A);
        editor.SetSideBPresent(false);
        var inputs = editor.SideA.LoadFromDesign(design, resolver);
        editor.SetSideBPresent(false);
        editor.LoadComposite(design.Composite);
        editor.ClearDormantTail();
        editor.AlignSlotGrid();
        var byHand = new PushBackPilotDraftState(editor, inputs, text);

        var materialized = PilotKit.Admitted(text);

        Assert.True(Comparer.Equals(byHand, materialized));
        Assert.Equal(byHand.Editor.SideA.Structure.SelectedFrontIndex, materialized.Editor.SideA.Structure.SelectedFrontIndex);
        Assert.Equal(byHand.Editor.SideA.Structure.SelectedLevelIndex, materialized.Editor.SideA.Structure.SelectedLevelIndex);
        Assert.NotNull(materialized.Editor.SideA.WorkingBaseline);
        Assert.Equal(byHand.Editor.SideA.WorkingBaseline!.Structure.Modules.Count, materialized.Editor.SideA.WorkingBaseline!.Structure.Modules.Count);
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

    [Fact]
    public void KeepMineAfterAnExternalModuleChangeWritesTheNewModulesAndTheScaleOfTheDraftAndReturnsToCleanWhenTheScaleIsRestored()
    {
        var v1 = PilotKit.Text("pb-01-base");
        var v2 = PilotKit.Text("pb-31-v2-modulos");
        var registry = new DraftRegistry();
        var draft = registry.Begin(
            DraftKit.S1, "R1", DraftIntentRead<PushBackPilotDraftState>.Admitted(DraftKit.DefaultBase(), PilotKit.Admitted(v1)),
            Comparer, PushBackPilotStateComparer.Copy).Draft!;

        // 1. V1 is open and the scale is edited.
        draft.CurrentDraftState.Inputs.Annotations.AnnotationScale = 2.25;
        Assert.True(draft.IsDirty);

        // 2. An external V2 changes ONLY the module layer (a custom header), with the same scale as V1.
        var second = PilotKit.Admitted(v2);
        Assert.Equal(1.5, second.Inputs.Annotations.AnnotationScale);
        Assert.True(Comparer.Equals(PilotKit.Admitted(v1), second));
        Assert.NotEqual(PilotKit.DesignOf(v1), PilotKit.DesignOf(v2));

        // 3. The user keeps the draft on the current version: the initial state moves to V2.
        var keep = draft.KeepMineOnCurrentVersion(
            DraftIntentRead<PushBackPilotDraftState>.Admitted(DraftKit.DefaultBase(), second), confirmed: true);
        Assert.Equal(DraftReloadStatus.Reloaded, keep.Status);
        Assert.Equal(v2, draft.InitialDraftState.Origin);
        Assert.True(draft.IsDirty);

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
        ["pb-13-sideb-no-presente"] = new[] { "$.PushBack.SideB: {object} -> <absent>" },
        ["pb-14-peralte-heredado"] = new[]
        {
            "$.PushBack.Fronts[0].HighEndBeamPeraltes: null -> [3 items]", "$.PushBack.Fronts[1].HighEndBeamPeraltes: null -> [3 items]",
            "$.PushBack.Fronts[2].HighEndBeamPeraltes: null -> [3 items]", "$.PushBack.LegacyHighEndBeamPeralte: 5 -> 3.5",
        },
        ["pb-15-sin-composite"] = new[] { "$.PushBack.Composite: <absent> -> {object}" },
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
