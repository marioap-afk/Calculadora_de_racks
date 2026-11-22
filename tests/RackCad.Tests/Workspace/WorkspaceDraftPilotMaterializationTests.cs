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
    private static string Rewrite(string text, Action<RackEmbedDocument> edit)
    {
        var store = new RackEmbedStore();
        var embed = store.Deserialize(text)!;
        edit(embed);
        return store.Serialize(embed);
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
        // I-64 F4B PB-T4d (owner-pbt4d-substitutes-decision.md): the comparer of the pilot is retired; the two states are compared by
        // the design that the product assembles from them.
        Assert.Equal(
            PilotKit.Persist(new PushBackCompositeEditorAssembler(PilotKit.Catalog).BuildDesign(byHand.Editor, byHand.Inputs)),
            PilotKit.Persist(new PushBackCompositeEditorAssembler(PilotKit.Catalog).BuildDesign(materialized.Editor, materialized.Inputs)));
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

    // ------------------------------------------------------------------ INV-F4T2A-08: the round trip

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

    [Fact]
    public void EveryLegacyFixtureHasItsExpectedDelta()
    {
        Assert.Equal(PilotKit.Legacy.OrderBy(name => name, StringComparer.Ordinal), LegacyDeltas.Keys.OrderBy(name => name, StringComparer.Ordinal));
    }

    // ------------------------------------------------------------------ I-64 F4-T4A1 (D-T4A1-05, D-T4A1-02): the carry-overs of F4-E25

}
