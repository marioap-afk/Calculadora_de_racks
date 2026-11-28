#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Views.Policy;
using RackCad.Application.Views.Preparation;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests;

// M-5 of the review of the entrega 2 of «Selectivo en la ventana RackCad» (I-64 A-14, line D): the guard of B-1 for a Selectivo. B-1 (the
// smoke of 2026-11-25, Dinámico) was the new view of «Insertar vista…» placed with the design the drawing had before the edit, while the
// views that exist were redrawn with the draft (RackInsertedViewDesignTests). In a Selectivo the new view (SelectiveInsertPort.TryPrepare)
// and the views that exist (PrepareMember) are composed with the SAME reconciled authored JSON of the draft (authoredJson, built by
// RackSelectivoCommands.InsertSelectiveViews from window.DesignToInsert), so B-1 does not happen; these tests keep it so.
public class SelectiveInsertedViewDesignTests
{
    private const string RackId = "0b8f6a2e-1c3d-4e5f-8a9b-7c6d5e4f3a21";
    private const string Name = "Selectivo A";

    private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

    // A Selectivo of two fronts of two levels with pallets of that height, recomposed by the draft of the window and copied as
    // «Insertar vista…» sends it (SnapshotForWrite).
    private static SelectiveDraftSnapshot Draft(double alto)
    {
        var design = new SelectivePalletDesign { PostId = "POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA", PostPeralte = 3.0, PalletDepth = 48.0, DepthCount = 1 };
        for (var b = 0; b < 2; b++)
        {
            var bay = new SelectiveBayDesign { FloorBeam = true, FloorBeamRiseOverride = 4.0 };
            for (var l = 0; l < 2; l++)
                bay.Levels.Add(new SelectiveCell { Pallet = new Tarima { Frente = 42.0, Alto = alto }, PalletCount = 2, BeamId = "LARGUERO_ESCALON_CAL14_3_REMACHES", BeamPeralte = 4.0 });
            design.Bays.Add(bay);
        }

        foreach (var peralte in new[] { 0.0, 0.0, 0.0 }) design.PostPeraltes.Add(peralte);
        var draft = new SelectiveDraftEditor(Catalog);
        Assert.True(draft.LoadFrom(design, null, Name), draft.LastError);
        var snapshot = draft.SnapshotForWrite();
        Assert.NotNull(snapshot);
        return snapshot!;
    }

    private static string Authored(SelectivePalletDesign design) => new SelectivePalletDesignStore().Serialize(SelectivePalletDesignDocument.From(design, RackId, Name));

    // One view as the drawing holds it: written and read back as the block finder reads it.
    private static RackEmbedDocument View(string design, string view, int section)
    {
        var store = new RackEmbedStore();
        return store.Deserialize(store.Serialize(RackEmbedComposer.Compose(null, RackEmbedDocument.KindSelective, RackId, Name, view, section, design)));
    }

    private static double Alto(RackEmbedDocument envelope) => new SelectivePalletDesignStore().Deserialize(envelope.Design)!.ToDomain().Bays[0].Levels[0].Pallet.Alto;

    [Fact]
    public void TheNewViewOfInsertarVistaOfASelectivoCarriesTheDraftAsTheViewsThatExist()
    {
        // The drawing: «Selectivo A» with its frontal, pallets of 60 in high. The draft of the window: 72.
        var catalog = Catalog;
        var drawnJson = Authored(Draft(60.0).Design);
        var edit = Draft(72.0);
        var editJson = Authored(edit.Design);
        var frontal = View(drawnJson, RackEmbedDocument.ViewFrontal, 0);
        var comparison = new SelectiveAuthoredComparisonInput(RackId, new List<ProjectVariableScanEntry> { ProjectVariableScanProjection.Project("2A1", frontal, 1) });
        var system = edit.System;
        var availability = new SelectiveViewAvailabilityFacts(SelectiveDepthLayout.Count(system), new SelectiveLateralBuilder().Cortes(system, catalog).Select(c => c.PostIndex));
        var preparer = new RackProductPreparer<SelectivePalletDesignDocument, SelectiveRackSystem, HeaderRunPlan>(
            RackResolvePorts.Selective<SelectivePalletDesignDocument, SelectiveRackSystem>(_ => system),
            RackViewPreparationPorts.Selective<SelectiveRackSystem, HeaderRunPlan>(
                (resolved, target) => new SelectivePlantaBuilder().BuildPlan(resolved, catalog),
                target => target.Kind == DimensionViewKind.Planta,
                RackBlockRequirementExtractors.HeaderRun),
            (resolved, target) => SelectiveViewFrameAdapter.Resolve(resolved, catalog, target),
            (resolved, target) => "Planta");

        // As SelectiveInsertPort.TryPrepare: the source is the picked view composed with authoredJson (the reconciled draft).
        var source = RackEmbedComposer.Compose(frontal, RackEmbedDocument.KindSelective, RackId, Name, frontal.View, frontal.Section, editJson);
        var request = new RackProductViewRequest(RackViewProductOperation.InsertSibling, RackSystemKind.SelectiveRack, RackViewAddress.Whole(DimensionViewKind.Planta), availability);
        var accepted = RackProductIntentAcceptance.Accept(new ExistingRackViewIntent<SelectiveAuthoredComparisonInput>(new ExistingRackContext<SelectiveAuthoredComparisonInput>(source, comparison), request));
        Assert.True(accepted.IsAccepted, accepted.CauseCode);
        var result = preparer.PrepareExisting(accepted.Intent, RackAuthoredComparatorPorts.Selective());

        Assert.True(result.IsSuccess, result.CauseCode + ": " + result.Diagnostic);
        var inserted = result.Product.Envelope;
        Assert.Equal(editJson, inserted.Design);
        Assert.Equal(72.0, Alto(inserted));
        Assert.Equal((RackId, Name), (inserted.Id, inserted.Name));

        // As PrepareMember: the frontal that exists is redrawn with the same authoredJson; the two are siblings of the same design.
        var redrawn = RackEmbedComposer.Compose(frontal, RackEmbedDocument.KindSelective, RackId, Name, frontal.View, frontal.Section, editJson);
        var siblings = new[] { redrawn, inserted }.Select((view, i) => new RackSiblingView("2A" + (i + 1), view)).ToList();
        Assert.Equal(RackSiblingDesignsOutcome.Same, RackSiblingDesigns.Compare(RackId, siblings).Outcome);

        // B-1 was the picked view as the drawing held it: the new view would carry the design before the edit, and the views would diverge.
        var asRead = preparer.PrepareExisting(RackProductIntentAcceptance.Accept(new ExistingRackViewIntent<SelectiveAuthoredComparisonInput>(
            new ExistingRackContext<SelectiveAuthoredComparisonInput>(frontal, comparison), request)).Intent, RackAuthoredComparatorPorts.Selective());
        Assert.True(asRead.IsSuccess, asRead.CauseCode);
        Assert.Equal(60.0, Alto(asRead.Product.Envelope));
        Assert.Equal(RackSiblingDesignsOutcome.Divergent,
            RackSiblingDesigns.Compare(RackId, new[] { redrawn, asRead.Product.Envelope }.Select((view, i) => new RackSiblingView("2A" + (i + 1), view)).ToList()).Outcome);
    }

    // ------------------------------------------------------------------ the composition of the Plugin

    // The port composes the new view (TryPrepare) and the views that exist (PrepareMember) with the same authoredJson; the drawing half of
    // «Insertar» builds it from the design of the editor (window.DesignToInsert, reconciled), and RACKSELECTIVOINSERTAR hands it the design
    // the window sent (write.Design).
    [Fact]
    public void TheNewViewAndTheViewsThatExistOfASelectivoAreComposedWithTheSameAuthoredDraft()
    {
        var port = WorkspaceHostSource.Code("src/RackCad.Plugin/RackSelectivoInsertIntegration.cs");
        var prepare = WorkspaceHostSource.BodyOf(port, @"public\s+bool\s+TryPrepare\s*\(");
        Assert.Matches(@"var\s+curedSource\s*=\s*ComposeEnvelope\(source,\s*rackId,\s*rackName,\s*source\?\.View,\s*source\?\.Section\s*\?\?\s*-1,\s*authoredJson\);", prepare);
        Assert.Matches(@"new\s+ExistingRackContext<SelectiveAuthoredComparisonInput>\(curedSource,\s*comparison\)", prepare);
        var member = WorkspaceHostSource.BodyOf(port, @"private\s+RackSiblingUnitPreparation<SiblingRedrawUnit>\s+PrepareMember\s*\(");
        Assert.Matches(@"var\s+payloadEnvelope\s*=\s*ComposeEnvelope\(envelope,\s*rackId,\s*rackName,\s*envelope\.View,\s*envelope\.Section,\s*authoredJson\);", member);
        Assert.Equal(1, Regex.Matches(port, @"this\.authoredJson\s*=\s*authoredJson;").Count);

        var commands = WorkspaceHostSource.Code("src/RackCad.Plugin/RackSelectivoCommands.cs");
        var half = WorkspaceHostSource.BodyOf(commands, @"internal static int InsertSelectiveViews\(");
        Assert.Contains("var design = window.DesignToInsert;", half, StringComparison.Ordinal);
        Assert.Matches(@"LinkedPropertyReconciler\.Reconcile\(\s*saved,\s*design,", half);
        Assert.Contains("var designJson = SerializeSelectiveAuthored(insertReconciled.Authored);", half, StringComparison.Ordinal);
        Assert.Matches(@"new\s+SelectiveInsertPort\(document,\s*blockId,\s*embed,\s*system,\s*designJson,", half);

        var door = WorkspaceHostSource.Code("src/RackCad.Plugin/VentanaRackCad/RackCadRackDoorCommands.cs");
        var insert = WorkspaceHostSource.BodyOf(door, @"public void InsertSelectiveViews\(\)");
        Assert.Matches(@"new\s+RackCad\.UI\.Systems\.Selective\.SelectiveDraftOutcome\(write\.RackId,\s*write\.Name,\s*write\.Design,\s*write\.System,", insert);
    }
}
