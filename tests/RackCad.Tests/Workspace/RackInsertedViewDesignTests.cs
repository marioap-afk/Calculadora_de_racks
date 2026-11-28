#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Views.Policy;
using RackCad.Application.Views.Preparation;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests;

// B-1 (I-64 A-14, line D; smoke of 2026-11-25): «Insertar vista…» of the RackCad window, with a draft that differs from the drawing,
// redrew the views that exist with the draft and placed the NEW view with the design that the drawing had before. The same drawing half
// is RACKEDITAR «Insertar» with changes in its window (EditDynamic → ApplyDynamicEdit), so the classic editor (and main) had it too.
// The cause: the queue of ID18 prepares an existing rack's new view with RackProductPreparer.PrepareExisting, which composes the envelope
// of the new view from the identity, the name and the design of its SOURCE; RackViewBatchProducts.Dynamic handed it the picked view as
// the drawing held it, so the new view carried the design before the edit while its geometry came from the edited system.
public class RackInsertedViewDesignTests
{
    private const string RackId = "6c1f0a3e-9b2d-4e57-8a41-2f7d3c9b5e10";
    private const string Name = "DIN 1";

    private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

    // A Dinámico of two fronts (3 and 2 levels) with the default safety of a new rack, its pallets of the weight given, recomposed as the
    // window recomposes it and copied as «Insertar vista…» sends it (SnapshotForWrite).
    private static DynamicDraftSnapshot Draft(double weight)
    {
        var design = new DynamicRackDesign
        {
            Pallet = new PalletSpecification(42.0, 48.0, 60.0, weight, "kg"),
            PalletsDeep = 6,
            LoadLevels = 3,
            FirstLevelHeight = 6.0,
            BeamDepth = 4.0,
        };
        design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 2, LoadLevels = 3, PalletsDeep = 6, DepthStartPosition = 1 });
        design.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 4, DepthStartPosition = 3 });
        foreach (var selection in DynamicSafetyDefaults.Build(Catalog)) design.SafetySelections.Add(selection.DeepCopy());

        var draft = new DynamicDraftEditor(Catalog);
        draft.LoadFrom(design, Name);
        Assert.True(draft.Recompose(), draft.LastError);
        var snapshot = draft.SnapshotForWrite();
        Assert.NotNull(snapshot);
        return snapshot!;
    }

    // One view as the drawing holds it: written and read back as the block finder reads it.
    private static RackEmbedDocument View(string design, string view, int section)
    {
        var store = new RackEmbedStore();
        return store.Deserialize(store.Serialize(RackEmbedComposer.Compose(null, RackEmbedDocument.KindDynamic, RackId, Name, view, section, design)));
    }

    // The design an existing view receives from the drawing half (BuildDynamicPayload): the edited design with the source metadata of
    // the view, and the envelope of that view with the identity and the name of the edit.
    private static RackEmbedDocument Redrawn(RackEmbedDocument view, DynamicRackDesign edited)
    {
        var json = new RackProjectStore().Serialize(RackProject.ForDynamic(edited).WithSourceMetadataFrom(new RackProjectStore().Deserialize(view.Design)));
        return RackEmbedComposer.Compose(view, RackEmbedDocument.KindDynamic, RackId, Name, view.View, view.Section, json);
    }

    // The evidence of the views BEFORE the edit, as RackUnsupportedSiblingInsert.TryAuthorize gives it to the queue.
    private static RackAuthoredInput Before(params (string Key, RackEmbedDocument Envelope)[] views)
    {
        var store = new RackEmbedStore();
        return new RackAuthoredInput(RackId,
            views.Select(v => new RackAuthoredSibling(v.Key, v.Envelope.Kind, store.Serialize(v.Envelope), v.Envelope.Design)).ToList(),
            true, "RackInsertedViewDesignTests");
    }

    // The preparation of one new view by the queue of ID18 for a rack of the drawing, composed as RackViewBatchProducts.Dynamic composes
    // it: the edited system resolves every view (the picked design is only compared), the AUTH-13 comparator gates the siblings.
    private static RackProductPrepareResult<HeaderRunPlan> Prepare(RackEmbedDocument source, RackAuthoredInput before, DynamicRackSystem system,
        RackViewAddress address)
    {
        var catalog = Catalog;
        var facts = new DynamicViewAvailabilityFacts(new DynamicSystemLateralBuilder().Cortes(system, catalog).Select(c => c.PostIndex));
        var preparer = new RackProductPreparer<DynamicRackDesign, DynamicRackSystem, HeaderRunPlan>(
            RackResolvePorts.Dynamic<DynamicRackDesign, DynamicRackSystem>(_ => system),
            RackViewPreparationPorts.Dynamic<DynamicRackSystem, HeaderRunPlan>(
                (resolved, target) => new DynamicSystemFrontalBuilder().BuildPlan(resolved, catalog,
                    target.Variant.FlowEnd == RackFlowEnd.Entrance ? DynamicRackEnd.Entrance : DynamicRackEnd.Exit),
                _ => true,
                RackBlockRequirementExtractors.HeaderRun),
            (resolved, target) => DynamicViewFrameAdapter.Resolve(resolved, catalog, target),
            (resolved, target) => RackViewProductNames.Dynamic(resolved, target, Name, Name));
        var accepted = RackProductIntentAcceptance.Accept(new ExistingRackViewIntent<RackAuthoredInput>(
            new ExistingRackContext<RackAuthoredInput>(source, before),
            new RackProductViewRequest(RackViewProductOperation.InsertSibling, RackSystemKind.PalletFlow, address, facts)));
        Assert.True(accepted.IsAccepted, accepted.CauseCode);
        return preparer.PrepareExisting(accepted.Intent, RackAuthoredComparatorPorts.Dynamic());
    }

    private static double Weight(RackEmbedDocument envelope)
        => new RackProjectStore().Deserialize(envelope.Design)!.DynamicDesign.Pallet.Weight;

    [Fact]
    public void ANewViewInsertedByTheEditThatRedrawsTheOthersCarriesTheEditedDesign()
    {
        // The drawing: DIN 1 with its lateral of P1 and its planta, pallets of 1100. The draft of the window: 1200.
        var drawn = Draft(1100.0);
        var lateral = View(drawn.DesignJson, RackEmbedDocument.ViewLateral, 0);
        var planta = View(drawn.DesignJson, RackEmbedDocument.ViewPlanta, -1);
        var edit = Draft(1200.0);
        var project = new RackProjectStore().Deserialize(lateral.Design);
        var editedJson = new RackProjectStore().Serialize(RackProject.ForDynamic(edit.Design).WithSourceMetadataFrom(project));
        var before = Before(("2A1", lateral), ("2A2", planta));

        // «Insertar vista…» → «Frontal - salida»: the views that exist are redrawn with the draft and the queue places the new one.
        var redrawn = new[] { Redrawn(lateral, edit.Design), Redrawn(planta, edit.Design) };
        var result = Prepare(RackInsertedViewSource.Edited(lateral, RackId, Name, editedJson), before, edit.System,
            RackViewAddress.FlowEnd(RackFlowEnd.Exit));

        Assert.True(result.IsSuccess, result.CauseCode + ": " + result.Diagnostic);
        var inserted = result.Product.Envelope;
        Assert.Equal(editedJson, inserted.Design);
        Assert.Equal(1200.0, Weight(inserted));
        Assert.All(redrawn, view => Assert.Equal(1200.0, Weight(view)));
        Assert.Equal(RackEmbedDocument.ViewFrontal, inserted.View);
        Assert.Equal((RackId, Name), (inserted.Id, inserted.Name));

        // The three views are siblings of the same design: the window opens the rack again (D-3 of the smoke).
        var siblings = redrawn.Append(inserted).Select((view, i) => new RackSiblingView("2A" + (i + 1), view)).ToList();
        Assert.Equal(RackSiblingDesignsOutcome.Same, RackSiblingDesigns.Compare(RackId, siblings).Outcome);
    }

    // I-64, cuarta tanda (review of «seg», 2026-11-26): an EXISTING Dinámico (its desviador read the historic way, without the marker) whose
    // desviador is edited in the window passes to the reading by post and gains DesviadorCellsByPost (T4S-13). Its views, redrawn with it,
    // must open again and take a new view: the raw reader of AUTH-13 did not know the member, so the comparator found them unreadable and
    // the queue refused the new view. The draft of the other tests is built with DynamicSafetyDefaults.Build, without the marker.
    [Fact]
    public void AnExistingDinamicoWhoseDesviadorIsEditedInTheWindowGainsTheMarkerAndItsViewsOpenAndTakeANewViewAgain()
    {
        var drawn = Draft(1100.0);
        Assert.False(Marked(drawn.DesignJson));
        var lateral = View(drawn.DesignJson, RackEmbedDocument.ViewLateral, 0);
        var planta = View(drawn.DesignJson, RackEmbedDocument.ViewPlanta, -1);

        // The window: the first level of the desviador of P1 off, as a click on its button in «Seguridad por poste» does.
        var editor = new DynamicDraftEditor(Catalog);
        editor.LoadFrom(new RackProjectStore().Deserialize(lateral.Design)!.DynamicDesign, Name);
        var safety = editor.CopySafety().ToList();
        var desviador = RackCadSafetyEdit.Selected(safety, Catalog, RackCad.Domain.Systems.Selective.SelectiveSafetyDefaults.DesviadorType);
        Assert.True(RackCadSafetyEdit.DesviadorToPosts(desviador, editor.SafetyPostCount));
        RackCadSafetyEdit.SetDesviador(desviador, 0, 0, false);
        editor.ReplaceSafety(safety);
        Assert.True(editor.Recompose(), editor.LastError);
        var edit = editor.SnapshotForWrite();
        Assert.NotNull(edit);
        Assert.True(Marked(edit!.DesignJson));

        // Opened again (the door): the views redrawn with the edit coincide.
        var redrawn = new[] { Redrawn(lateral, edit.Design), Redrawn(planta, edit.Design) };
        Assert.All(redrawn, view => Assert.True(Marked(view.Design)));
        var verdict = RackSiblingDesigns.Compare(RackId, redrawn.Select((view, i) => new RackSiblingView("2A" + (i + 1), view)).ToList());
        Assert.True(verdict.Outcome == RackSiblingDesignsOutcome.Same, verdict.Diagnostic);

        // Inserted again: a new view of the rack as the drawing holds it now, its views with the marker, gated by the AUTH-13 comparator.
        var result = Prepare(RackInsertedViewSource.Edited(redrawn[0], RackId, Name, redrawn[0].Design),
            Before(("2A1", redrawn[0]), ("2A2", redrawn[1])), edit.System, RackViewAddress.FlowEnd(RackFlowEnd.Exit));
        Assert.True(result.IsSuccess, result.CauseCode + ": " + result.Diagnostic);
        Assert.True(Marked(result.Product.Envelope.Design));
        var all = redrawn.Append(result.Product.Envelope).Select((view, i) => new RackSiblingView("2A" + (i + 1), view)).ToList();
        Assert.Equal(RackSiblingDesignsOutcome.Same, RackSiblingDesigns.Compare(RackId, all).Outcome);
    }

    private static bool Marked(string design)
        => new RackProjectStore().Deserialize(design)!.DynamicDesign.SafetySelections.Any(selection => selection.Desviador.CellsByPost);

    [Fact]
    public void WhatB1WasThePickedViewAsTheDrawingHeldItGivesTheNewViewTheDesignBeforeTheEdit()
    {
        // PrepareExisting composes the new view from its source (its contract): so the source must be the edited one.
        var drawn = Draft(1100.0);
        var lateral = View(drawn.DesignJson, RackEmbedDocument.ViewLateral, 0);
        var planta = View(drawn.DesignJson, RackEmbedDocument.ViewPlanta, -1);
        var edit = Draft(1200.0);
        var before = Before(("2A1", lateral), ("2A2", planta));

        var asRead = Prepare(lateral, before, edit.System, RackViewAddress.FlowEnd(RackFlowEnd.Exit));

        Assert.True(asRead.IsSuccess, asRead.CauseCode);
        Assert.Equal(1100.0, Weight(asRead.Product.Envelope));
        var siblings = new[] { Redrawn(lateral, edit.Design), Redrawn(planta, edit.Design), asRead.Product.Envelope }
            .Select((view, i) => new RackSiblingView("2A" + (i + 1), view)).ToList();
        Assert.Equal(RackSiblingDesignsOutcome.Divergent, RackSiblingDesigns.Compare(RackId, siblings).Outcome);
    }

    [Fact]
    public void TheNewViewAlsoTakesTheNameAndTheCuredIdentityOfTheEditAndKeepsWhatThePickedEnvelopeCarries()
    {
        var drawn = Draft(1100.0);
        var picked = View(drawn.DesignJson, RackEmbedDocument.ViewLateral, 2);
        picked.CustomProperties = System.Text.Json.JsonDocument.Parse("{\"SchemaVersion\":\"1.0\",\"Values\":{\"Cliente\":\"X\"}}").RootElement.Clone();

        var edited = RackInsertedViewSource.Edited(picked, "otra-identidad", "DIN 2", "{\"design\":2}");

        Assert.NotSame(picked, edited);
        Assert.Equal(("otra-identidad", "DIN 2", "{\"design\":2}"), (edited.Id, edited.Name, edited.Design));
        Assert.Equal((picked.Kind, picked.View, picked.Section), (edited.Kind, edited.View, edited.Section));
        Assert.Equal(picked.CustomProperties!.Value.GetRawText(), edited.CustomProperties!.Value.GetRawText());
        // The envelope of the drawing is not changed here.
        Assert.Equal((RackId, Name, drawn.DesignJson), (picked.Id, picked.Name, picked.Design));

        // A new rack has no source.
        Assert.Null(RackInsertedViewSource.Edited(null!, RackId, Name, drawn.DesignJson));
    }

    // ------------------------------------------------------------------ the composition of the Plugin

    // RackViewBatchProducts.Dynamic gives the queue the edited source, with the same serialized design it gives as the design of a new
    // rack; ApplyDynamicEdit (RACKEDITAR «Insertar» and «Insertar vista…» of the window) hands it the design, the identity and the name
    // with which it redraws the views that exist.
    [Fact]
    public void TheQueueOfDinamicoPreparesTheNewViewFromTheEditedSource()
    {
        var products = WorkspaceHostSource.Code("src/RackCad.Plugin/Views/RackViewBatchProducts.cs");
        var dynamic = WorkspaceHostSource.BodyOf(products, @"internal\s+static\s+[\w<>,\s]+\s+Dynamic\s*\(");
        Assert.Matches(@"var\s+serialized\s*=\s*Project\(RackProject\.ForDynamic\(design\),\s*innerSource\);", dynamic);
        Assert.Matches(@"RackAuthoredComparatorPorts\.Dynamic\(\),\s*comparison,\s*RackInsertedViewSource\.Edited\(source,\s*rackId,\s*rackName,\s*serialized\),\s*design,\s*rackId,\s*rackName,\s*serialized,", dynamic);

        var commands = WorkspaceHostSource.Code("src/RackCad.Plugin/RackDinamicoCommands.cs");
        var apply = WorkspaceHostSource.BodyOf(commands, @"internal\s+static\s+\(int\s+Redrawn,\s*int\s+Erased\)\s+ApplyDynamicEdit\s*\(");
        Assert.Contains("RackViewBatchProducts.Dynamic(document, system, design, id, name, embed, project, authorized.AuthoredInput)", apply, StringComparison.Ordinal);
        Assert.Equal(3, Regex.Matches(apply, @"BuildDynamicPayload\(design,\s*id,\s*name,").Count);
        var edit = WorkspaceHostSource.BodyOf(commands, @"internal\s+static\s+void\s+EditDynamic\s*\(");
        Assert.Contains("ApplyDynamicEdit(document, blockId, embed, project, window);", edit, StringComparison.Ordinal);
    }
}
