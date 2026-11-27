#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Selective;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Views.Policy;
using RackCad.Application.Views.Preparation;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests;

// T6 b1 (I-64, sixth batch): does B-1 reach a Selectivo through «Insertar varias…» of the classic editor or through SelectiveNew? B-1 (the
// smoke of 2026-11-25; RackInsertedViewDesignTests) was a new view of an existing rack placed with the design the drawing had before the
// edit, while the same edit redrew the views that exist with the draft. SelectiveInsertedViewDesignTests (M-5 of the review of the entrega 2)
// keeps one view of «Insertar» right. These two keep the other two Selectivo paths that place several views at once:
// - «Insertar varias…» of RACKEDITAR on a rack of the drawing (RackSelectiveWindow.InsertBatch_Click → EditSelective → InsertSelectiveViews):
//   every view requested is prepared by SelectiveInsertPort.TryPrepare from the SAME source composed with the reconciled draft (curedSource),
//   and the views that exist are redrawn with that same authoredJson (PrepareMember). No B-1.
// - SelectiveNew (RackMenuCommands.DispatchInsertion, a NEW rack: RACKSELECTIVO, RACKSELECTIVONUEVO of the window, the library): it has no
//   source and no view to redraw; every view is prepared from the same authored document and its serialization. No B-1.
public class SelectiveBatchInsertedViewDesignTests
{
    private const string RackId = "3e9a1f5c-7b2d-4c8e-a6f0-5d4c3b2a1e09";
    private const string Name = "Selectivo B";

    private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

    // A Selectivo of two fronts of two levels with pallets of that height, recomposed by the draft of the window and copied for the write.
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

    private static SelectiveViewAvailabilityFacts Facts(SelectiveRackSystem system, RackCatalog catalog)
        => new SelectiveViewAvailabilityFacts(SelectiveDepthLayout.Count(system), new SelectiveLateralBuilder().Cortes(system, catalog).Select(c => c.PostIndex));

    // The plan of a view, as RackViewBatchProducts.SelectivePlan and SelectiveInsertPort.BuildPlan build it.
    private static HeaderRunPlan Plan(SelectiveRackSystem system, RackViewAddress address, RackCatalog catalog)
    {
        if (address.Kind == DimensionViewKind.Planta) return new SelectivePlantaBuilder().BuildPlan(system, catalog);
        if (address.Kind == DimensionViewKind.Frontal)
            return new SelectiveFrontalBuilder().BuildPlan(SelectiveDepthLayout.FondoSystemView(system, address.Variant.Index), catalog);
        var cut = new SelectiveLateralBuilder().Cortes(system, catalog).First(c => c.PostIndex == address.Variant.Index);
        var layout = new LateralHeaderLayoutBuilder().Build(cut.Cabecera, LateralHeaderParametersFactory.FromConfiguration(cut.Cabecera), catalog);
        return HeaderInstanceGrouper.Group(layout.Instances.Concat(cut.Largueros).ToList(), Name);
    }

    private static RackProductPreparer<SelectivePalletDesignDocument, SelectiveRackSystem, HeaderRunPlan> Preparer(
        SelectiveRackSystem system, RackCatalog catalog)
        => new RackProductPreparer<SelectivePalletDesignDocument, SelectiveRackSystem, HeaderRunPlan>(
            RackResolvePorts.Selective<SelectivePalletDesignDocument, SelectiveRackSystem>(_ => system),
            RackViewPreparationPorts.Selective<SelectiveRackSystem, HeaderRunPlan>(
                (resolved, target) => Plan(resolved, target, catalog), _ => true, RackBlockRequirementExtractors.HeaderRun),
            (resolved, target) => SelectiveViewFrameAdapter.Resolve(resolved, catalog, target),
            (resolved, target) => RackViewProductNames.Selective(resolved, target, Name, Name));

    // What «Insertar varias…» offers for this rack and the user takes: the planta and the lateral of each post with a cut.
    private static IReadOnlyList<RackViewAddress> Several(SelectiveRackSystem system, RackCatalog catalog)
        => new[] { RackViewAddress.Whole(DimensionViewKind.Planta) }
            .Concat(new SelectiveLateralBuilder().Cortes(system, catalog).Select(c => RackViewAddress.Post(c.PostIndex))).ToList();

    [Fact]
    public void InsertarVariasOfTheClassicOnARackOfTheDrawingPlacesEveryNewViewWithTheDraftAsTheViewsThatExist()
    {
        // The drawing: «Selectivo B» with its frontal, pallets of 60 in high. RACKEDITAR: 72, then «Insertar varias…» with several views.
        var catalog = Catalog;
        var drawnJson = Authored(Draft(60.0).Design);
        var edit = Draft(72.0);
        var editJson = Authored(edit.Design);
        var frontal = View(drawnJson, RackEmbedDocument.ViewFrontal, 0);
        var comparison = new SelectiveAuthoredComparisonInput(RackId, new List<ProjectVariableScanEntry> { ProjectVariableScanProjection.Project("2A1", frontal, 1) });
        var system = edit.System;
        var facts = Facts(system, catalog);
        var requested = Several(system, catalog);
        Assert.True(requested.Count >= 3, "the rack must offer several views");
        var preparer = Preparer(system, catalog);

        // As InsertSelectiveViews: ONE port, ONE source composed with authoredJson (TryPrepare's curedSource), one preparation per view.
        var curedSource = RackEmbedComposer.Compose(frontal, RackEmbedDocument.KindSelective, RackId, Name, frontal.View, frontal.Section, editJson);
        var inserted = new List<RackEmbedDocument>();
        foreach (var address in requested)
        {
            var accepted = RackProductIntentAcceptance.Accept(new ExistingRackViewIntent<SelectiveAuthoredComparisonInput>(
                new ExistingRackContext<SelectiveAuthoredComparisonInput>(curedSource, comparison),
                new RackProductViewRequest(RackViewProductOperation.InsertSibling, RackSystemKind.SelectiveRack, address, facts)));
            Assert.True(accepted.IsAccepted, accepted.CauseCode);
            var result = preparer.PrepareExisting(accepted.Intent, RackAuthoredComparatorPorts.Selective());
            Assert.True(result.IsSuccess, result.CauseCode + ": " + result.Diagnostic);
            inserted.Add(result.Product.Envelope);
        }

        Assert.Equal(requested.Count, inserted.Select(view => (view.View, view.Section)).Distinct().Count());
        Assert.All(inserted, view =>
        {
            Assert.Equal(editJson, view.Design);
            Assert.Equal(72.0, Alto(view));
            Assert.Equal((RackId, Name), (view.Id, view.Name));
        });

        // As PrepareMember: the frontal that exists is redrawn with the same authoredJson; the views are siblings of the same design.
        var redrawn = RackEmbedComposer.Compose(frontal, RackEmbedDocument.KindSelective, RackId, Name, frontal.View, frontal.Section, editJson);
        var siblings = inserted.Prepend(redrawn).Select((view, i) => new RackSiblingView("2A" + (i + 1), view)).ToList();
        Assert.Equal(RackSiblingDesignsOutcome.Same, RackSiblingDesigns.Compare(RackId, siblings).Outcome);
    }

    [Fact]
    public void ANewSelectivoOfSeveralViewsHasNoViewToRedrawAndEveryViewCarriesTheSameAuthoredDesign()
    {
        // As SelectiveNew: no source, the authored document of the design and its serialization, the identity of the request for every view.
        var catalog = Catalog;
        var snapshot = Draft(72.0);
        var system = snapshot.System;
        var authored = SelectivePalletDesignDocument.From(snapshot.Design, RackId, Name);
        var serialized = new SelectivePalletDesignStore().Serialize(authored);
        var creation = new NewRackCreationContext<SelectivePalletDesignDocument>(authored, Name, serialized, new FixedId(RackId));
        var facts = Facts(system, catalog);
        var preparer = Preparer(system, catalog);
        var views = new[] { RackViewAddress.Fondo(0) }.Concat(Several(system, catalog)).ToList();

        var placed = new List<RackEmbedDocument>();
        foreach (var address in views)
        {
            var accepted = RackProductIntentAcceptance.Accept(new NewRackViewIntent<SelectivePalletDesignDocument>(creation,
                new RackProductViewRequest(RackViewProductOperation.CreateFirst, RackSystemKind.SelectiveRack, address, facts)));
            Assert.True(accepted.IsAccepted, address + ": " + accepted.CauseCode);
            var result = preparer.Prepare(accepted.Intent);
            Assert.True(result.IsSuccess, result.CauseCode + ": " + result.Diagnostic);
            placed.Add(result.Product.Envelope);
        }

        Assert.All(placed, view =>
        {
            Assert.Equal(serialized, view.Design);
            Assert.Equal(72.0, Alto(view));
            Assert.Equal((RackId, Name), (view.Id, view.Name));
        });
        var siblings = placed.Select((view, i) => new RackSiblingView("2A" + (i + 1), view)).ToList();
        Assert.Equal(RackSiblingDesignsOutcome.Same, RackSiblingDesigns.Compare(RackId, siblings).Outcome);
    }

    private sealed class FixedId : IRackIdFactory
    {
        private readonly string id;
        internal FixedId(string id) => this.id = id;
        public string CreateRackId() => id;
    }

    // ------------------------------------------------------------------ the composition of the Plugin

    // «Insertar varias…» of the classic editor sends all its views in ONE request; RACKEDITAR hands them to InsertSelectiveViews, which
    // prepares each one with the same port (the source composed with the reconciled draft, SelectiveInsertedViewDesignTests) and places what
    // it prepared. SelectiveNew is reached only by the dispatch of a new rack and gives its session no source, so every view is a view of a
    // new rack composed from the same authored document.
    [Fact]
    public void InsertarVariasGoesThroughTheOnePortOfInsertarAndSelectiveNewOnlyCreatesARack()
    {
        var window = WorkspaceHostSource.Code("src/RackCad.UI/Systems/Selective/RackSelectiveWindow.xaml.cs");
        var batch = WorkspaceHostSource.BodyOf(window, @"private\s+void\s+InsertBatch_Click\s*\(");
        Assert.Matches(@"session\.RequestInsertViews\(views,\s*ctx\s*=>\s*new\s+SelectiveInsertionRequest\(system,\s*design,", batch);

        var commands = WorkspaceHostSource.Code("src/RackCad.Plugin/RackSelectivoCommands.cs");
        var edit = WorkspaceHostSource.BodyOf(commands, @"internal\s+static\s+void\s+EditSelective\s*\(");
        Assert.Contains("InsertSelectiveViews(document, blockId, embed, saved, registry, window, window.InsertionRequest.Views);", edit, StringComparison.Ordinal);
        var half = WorkspaceHostSource.BodyOf(commands, @"internal static int InsertSelectiveViews\(");
        Assert.True(Regex.Matches(half, @"new\s+SelectiveInsertPort\(").Count == 1,
            "«Insertar varias…» prepares every view with ONE port, so all of them share its source composed with the draft. A port per view "
            + "would need each one composed with the same authoredJson; check B-1 before changing this.");
        Assert.Matches(@"foreach\s*\(var\s+address\s+in\s+requested\)", half);
        Assert.Contains("port.TryPrepare(address, membership, out var product, out var diagnostic)", half, StringComparison.Ordinal);
        Assert.Contains(".Success(prepared[item.Address])", half, StringComparison.Ordinal);
        Assert.Contains("RackViewPlacement.PlaceSelective(document, product, regen: false)", half, StringComparison.Ordinal);

        var products = WorkspaceHostSource.Code("src/RackCad.Plugin/Views/RackViewBatchProducts.cs");
        var selectiveNew = WorkspaceHostSource.BodyOf(products, @"internal\s+static\s+[\w<>,\s]+\s+SelectiveNew\s*\(");
        Assert.Contains("var authored = SelectivePalletDesignDocument.From(design, rackId, rackName);", selectiveNew, StringComparison.Ordinal);
        Assert.Matches(@"RackSystemKind\.SelectiveRack,\s*facts,\s*preparer,\s*null,\s*null,\s*null,\s*authored,\s*rackId,\s*rackName,\s*new\s+SelectivePalletDesignStore\(\)\.Serialize\(authored\),", selectiveNew);
        var session = WorkspaceHostSource.Code("src/RackCad.Plugin/Views/RackViewBatchProductSession.cs");
        Assert.Matches(@"if\s*\(source\s*==\s*null\)\s*creation\s*=\s*new\s+NewRackCreationContext<TAuthored>\(\s*authored,\s*rackName,\s*serializedDesign,", session);

        var callers = WorkspaceHostSource.AllProduct().Where(file => file.Code.Contains("RackViewBatchProducts.SelectiveNew(", StringComparison.Ordinal)).ToList();
        Assert.Equal(new[] { "src/RackCad.Plugin/RackMenuCommands.cs" }, callers.Select(file => file.Path).ToArray());
        var menu = callers[0].Code;
        var dispatch = menu.Substring(menu.IndexOf("case SelectiveInsertionRequest selective:", StringComparison.Ordinal));
        Assert.True(dispatch.IndexOf("RackViewBatchProducts.SelectiveNew(", StringComparison.Ordinal)
            < dispatch.IndexOf("break;", StringComparison.Ordinal));
        Assert.Contains("RackProductSourceKind.NewRack", dispatch.Substring(0, dispatch.IndexOf("break;", StringComparison.Ordinal)), StringComparison.Ordinal);
    }
}
