#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Views.Policy;
using RackCad.Application.Views.Preparation;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests;

// T6 b1 (I-64, sixth batch): B-1 in Push Back and in Cabecera. B-1 (the smoke of 2026-11-25, Dinámico; RackInsertedViewDesignTests) was the
// new view of «Insertar» with a modified draft placed with the design and the name the drawing had BEFORE the edit, while the same edit
// redrew the views that exist with the draft: the sibling views were left divergent. The queue of ID18 prepares the new view of an existing
// rack with RackProductPreparer.PrepareExisting, which composes its envelope from the identity, the name and the design of its SOURCE, and its
// geometry from the edited system. Dinámico and Cantilever hand it RackInsertedViewSource.Edited(...); RackViewBatchProducts.PushBack and
// .Header handed it the picked view as the drawing held it. Push Back has two drawing halves that reach that factory: RACKEDITAR «Insertar»
// (RackPushBackCommands.EditPushBack) and «Insertar vista…» of the RackCad window (RACKPUSHBACKINSERTAR, RackPushBackWindowDrawing.Apply);
// Cabecera has one, RACKEDITAR «Insertar» (RackCabeceraCommands.EditCabecera). The door of a cabecera of its own only reads and updates.
public class PushBackCabeceraInsertedViewDesignTests
{
    private const string RackId = "5a7e2c19-3d4b-4f60-9e81-0c2b4d6f8a13";

    private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

    // One view as the drawing holds it: written and read back as the block finder reads it.
    private static RackEmbedDocument View(string kind, string id, string name, string design, string view, int section)
    {
        var store = new RackEmbedStore();
        return store.Deserialize(store.Serialize(RackEmbedComposer.Compose(null, kind, id, name, view, section, design)));
    }

    // The evidence of the views BEFORE the edit, as RackUnsupportedSiblingInsert.TryAuthorize gives it to the queue.
    private static RackAuthoredInput Before(string rackId, params (string Key, RackEmbedDocument Envelope)[] views)
    {
        var store = new RackEmbedStore();
        return new RackAuthoredInput(rackId,
            views.Select(v => new RackAuthoredSibling(v.Key, v.Envelope.Kind, store.Serialize(v.Envelope), v.Envelope.Design)).ToList(),
            true, "PushBackCabeceraInsertedViewDesignTests");
    }

    private static RackSiblingDesignsOutcome Siblings(IEnumerable<RackEmbedDocument> views)
        => RackSiblingDesigns.Compare(RackId, views.Select((view, i) => new RackSiblingView("2A" + (i + 1), view)).ToList()).Outcome;

    // ================================================================== Push Back

    private const string PushBackName = "PB 1";
    private const string PushBackRenamed = "PB 1 renombrado";

    // The drawing: a new Push Back of the classic window as written. The draft: the same rack with the rear tope of one cell off, copied as
    // «Actualizar» and «Insertar vista…» send it (SnapshotForWrite).
    private static (PushBackDraftSnapshot Drawn, PushBackDraftSnapshot Edit) PushBackDrafts()
    {
        var draft = PushBackDraftEditor.CreateNew(Catalog);
        Assert.Null(draft.LastError);
        var drawn = draft.SnapshotForWrite();
        Assert.NotNull(drawn);
        draft.SetTope(new[] { (0, 1) }, false);
        Assert.True(draft.Recompose(), draft.LastError);
        var edit = draft.SnapshotForWrite();
        Assert.NotNull(edit);
        Assert.NotEqual(drawn!.DesignJson, edit!.DesignJson);
        return (drawn, edit);
    }

    // The design an existing view receives from the drawing half (BuildPushBackPayload): the edited design with the source metadata of that
    // view, and the envelope of that view with the identity and the name of the edit.
    private static RackEmbedDocument PushBackRedrawn(RackEmbedDocument view, PushBackDesign edited, string name)
    {
        var json = new RackProjectStore().Serialize(RackProject.ForPushBack(edited).WithSourceMetadataFrom(new RackProjectStore().Deserialize(view.Design)));
        return RackEmbedComposer.Compose(view, RackEmbedDocument.KindPushBack, RackId, name, view.View, view.Section, json);
    }

    // The preparation of one new view by the queue of ID18, composed as RackViewBatchProducts.PushBack composes it.
    private static RackProductPrepareResult<HeaderRunPlan> PreparePushBack(RackEmbedDocument source, RackAuthoredInput before,
        PushBackSystem system, RackViewAddress address, string rackName)
    {
        var catalog = Catalog;
        var facts = new PushBackViewAvailabilityFacts(
            new PushBackSystemLateralBuilder().Cortes(system, catalog).Select(c => c.PostIndex), system.IsComposite);
        var preparer = new RackProductPreparer<PushBackDesign, PushBackSystem, HeaderRunPlan>(
            RackResolvePorts.PushBack<PushBackDesign, PushBackSystem>(_ => system),
            RackViewPreparationPorts.PushBack<PushBackSystem, HeaderRunPlan>(
                (resolved, target) => new PushBackSystemFrontalBuilder().BuildPlan(resolved, catalog,
                    target.Variant.PushBackEnd == RackPushBackEnd.Posterior ? PushBackFrontalEnd.Posterior : PushBackFrontalEnd.EntradaSalida,
                    target.Variant.PushBackSide == RackPushBackSide.B ? PushBackSide.B : PushBackSide.A),
                _ => true,
                RackBlockRequirementExtractors.HeaderRun),
            (resolved, target) => PushBackViewFrameAdapter.Resolve(resolved, catalog, target),
            (resolved, target) => RackViewProductNames.PushBack(resolved, target, rackName, rackName));
        var accepted = RackProductIntentAcceptance.Accept(new ExistingRackViewIntent<RackAuthoredInput>(
            new ExistingRackContext<RackAuthoredInput>(source, before),
            new RackProductViewRequest(RackViewProductOperation.InsertSibling, RackSystemKind.PushBack, address, facts)));
        Assert.True(accepted.IsAccepted, accepted.CauseCode);
        return preparer.PrepareExisting(accepted.Intent, RackAuthoredComparatorPorts.PushBack());
    }

    private static bool TopeOff(RackEmbedDocument envelope)
        => new RackProjectStore().Deserialize(envelope.Design)!.PushBackDesign.RearTope.OffCells.Any(c => c.Frente == 0 && c.Level == 1);

    [Fact]
    public void ANewPushBackViewInsertedByTheEditThatRedrawsTheOthersCarriesTheEditedDesignAndName()
    {
        // The drawing: «PB 1» with its lateral of P1 and its planta. The draft: the rear tope of F1 N2 off, and the rack renamed.
        var (drawn, edit) = PushBackDrafts();
        var lateral = View(RackEmbedDocument.KindPushBack, RackId, PushBackName, drawn.DesignJson, RackEmbedDocument.ViewLateral, 0);
        var planta = View(RackEmbedDocument.KindPushBack, RackId, PushBackName, drawn.DesignJson, RackEmbedDocument.ViewPlanta, -1);
        var before = Before(RackId, ("2A1", lateral), ("2A2", planta));

        // What both drawing halves hand the factory: RACKEDITAR its project read from the picked view; RACKPUSHBACKINSERTAR the project of
        // PushBackDoorRules.PlanApply and the system named by PushBackDoorRules.Named (the same values).
        var write = new PushBackDoorWrite(1, new RackCad.Application.Workspace.SessionId(1), RackId, PushBackRenamed, edit.Design, edit.System,
            new[] { RackViewAddress.PushBackCut(RackPushBackEnd.Posterior, RackPushBackSide.A) });
        var plan = PushBackDoorRules.PlanApply(lateral.Kind, lateral.Design, write);
        Assert.True(plan.CanWrite, plan.Reason);
        var system = PushBackDoorRules.Named(write.System, write.Name, lateral.Name);
        var name = system.Name;
        Assert.Equal(PushBackRenamed, name);
        var serialized = new RackProjectStore().Serialize(RackProject.ForPushBack(write.Design).WithSourceMetadataFrom(plan.Project));

        // The views that exist are redrawn with the draft; the queue prepares the new one (the frontal posterior of side A).
        var redrawn = new[] { PushBackRedrawn(lateral, write.Design, name), PushBackRedrawn(planta, write.Design, name) };
        var result = PreparePushBack(RackInsertedViewSource.Edited(lateral, RackId, name, serialized), before, system,
            write.InsertViews![0], name);

        Assert.True(result.IsSuccess, result.CauseCode + ": " + result.Diagnostic);
        var inserted = result.Product.Envelope;
        Assert.Equal(RackEmbedDocument.ViewFrontal, inserted.View);
        Assert.Equal(serialized, inserted.Design);
        Assert.All(redrawn, view => Assert.Equal(view.Design, inserted.Design)); // byte for byte, as the views redrawn
        Assert.True(TopeOff(inserted));
        Assert.Equal((RackId, PushBackRenamed), (inserted.Id, inserted.Name));
        Assert.Equal(RackSiblingDesignsOutcome.Same, Siblings(redrawn.Append(inserted)));
    }

    [Fact]
    public void WhatB1WasInPushBackThePickedViewAsTheDrawingHeldItGivesTheNewViewTheDesignAndTheNameBeforeTheEdit()
    {
        var (drawn, edit) = PushBackDrafts();
        var lateral = View(RackEmbedDocument.KindPushBack, RackId, PushBackName, drawn.DesignJson, RackEmbedDocument.ViewLateral, 0);
        var planta = View(RackEmbedDocument.KindPushBack, RackId, PushBackName, drawn.DesignJson, RackEmbedDocument.ViewPlanta, -1);
        var system = PushBackDoorRules.Named(edit.System, PushBackRenamed, PushBackName);

        var asRead = PreparePushBack(lateral, Before(RackId, ("2A1", lateral), ("2A2", planta)), system,
            RackViewAddress.PushBackCut(RackPushBackEnd.Posterior, RackPushBackSide.A), PushBackRenamed);

        Assert.True(asRead.IsSuccess, asRead.CauseCode);
        Assert.Equal(drawn.DesignJson, asRead.Product.Envelope.Design);
        Assert.False(TopeOff(asRead.Product.Envelope));
        Assert.Equal(PushBackName, asRead.Product.Envelope.Name);
        var redrawn = new[] { PushBackRedrawn(lateral, edit.Design, PushBackRenamed), PushBackRedrawn(planta, edit.Design, PushBackRenamed) };
        Assert.Equal(RackSiblingDesignsOutcome.Divergent, Siblings(redrawn.Append(asRead.Product.Envelope)));
    }

    // M-04 of the review of B-1 in Dinámico, now in Push Back: RACKEDITAR takes the identity of the window when the picked view has none
    // (EditPushBack: window.RackId when embed.Id is blank). Before, the queue refused it at the acceptance (RACK_ID_INVALID); with the edited
    // source the acceptance takes it and the AUTH-13 gate refuses it, whatever the scan gave as evidence. Refused before any geometry, both.
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ALegacyPushBackWithoutIdentityIsStillRefusedBeforeAnyGeometryNowByTheAuthoredGate(string legacyId)
    {
        const string WindowId = "7c2e9b4d-1a3f-4e5b-9d6c-8f0a1b2c3d4e";
        var (drawn, edit) = PushBackDrafts();
        var lateral = View(RackEmbedDocument.KindPushBack, legacyId, PushBackName, drawn.DesignJson, RackEmbedDocument.ViewLateral, 0);
        var serialized = new RackProjectStore().Serialize(RackProject.ForPushBack(edit.Design).WithSourceMetadataFrom(new RackProjectStore().Deserialize(lateral.Design)));
        var facts = new PushBackViewAvailabilityFacts(
            new PushBackSystemLateralBuilder().Cortes(edit.System, Catalog).Select(c => c.PostIndex), edit.System.IsComposite);
        var request = new RackProductViewRequest(RackViewProductOperation.InsertSibling, RackSystemKind.PushBack,
            RackViewAddress.PushBackCut(RackPushBackEnd.Posterior, RackPushBackSide.A), facts);
        var withPicked = Before(WindowId, ("2A1", lateral));
        var withoutSiblings = new RackAuthoredInput(WindowId, new List<RackAuthoredSibling>(), true, "PushBackCabeceraInsertedViewDesignTests");

        // The picked view as the drawing held it (before T6 b1): refused at the acceptance.
        var asRead = RackProductIntentAcceptance.Accept(new ExistingRackViewIntent<RackAuthoredInput>(
            new ExistingRackContext<RackAuthoredInput>(lateral, withPicked), request));
        Assert.False(asRead.IsAccepted);
        Assert.Equal("RACK_ID_INVALID", asRead.CauseCode);

        // The edited source: accepted, and refused by the AUTH-13 gate with either evidence.
        foreach (var before in new[] { withPicked, withoutSiblings })
        {
            var accepted = RackProductIntentAcceptance.Accept(new ExistingRackViewIntent<RackAuthoredInput>(
                new ExistingRackContext<RackAuthoredInput>(RackInsertedViewSource.Edited(lateral, WindowId, PushBackRenamed, serialized), before), request));
            Assert.True(accepted.IsAccepted, accepted.CauseCode);
            var preparer = new RackProductPreparer<PushBackDesign, PushBackSystem, HeaderRunPlan>(
                RackResolvePorts.PushBack<PushBackDesign, PushBackSystem>(_ => throw new InvalidOperationException("resolved before the gate")),
                RackViewPreparationPorts.PushBack<PushBackSystem, HeaderRunPlan>(
                    (_, __) => throw new InvalidOperationException("prepared before the gate"), _ => true, RackBlockRequirementExtractors.HeaderRun),
                (resolved, target) => PushBackViewFrameAdapter.Resolve(resolved, Catalog, target),
                (_, __) => PushBackRenamed);
            var result = preparer.PrepareExisting(accepted.Intent, RackAuthoredComparatorPorts.PushBack());
            Assert.False(result.IsSuccess);
            Assert.Equal("AUTHORED_UNREADABLE", result.CauseCode);
            Assert.Null(result.Product);
        }
    }

    // RackViewBatchProducts.PushBack gives the queue the edited source, with the same serialized design it gives as the design of a new rack;
    // the two drawing halves that insert (RACKEDITAR and RACKPUSHBACKINSERTAR) hand it the design, the identity and the name with which they
    // redraw the views that exist.
    [Fact]
    public void TheQueueOfPushBackPreparesTheNewViewFromTheEditedSourceOnBothDrawingHalves()
    {
        var products = WorkspaceHostSource.Code("src/RackCad.Plugin/Views/RackViewBatchProducts.cs");
        var pushBack = WorkspaceHostSource.BodyOf(products, @"internal\s+static\s+[\w<>,\s]+\s+PushBack\s*\(");
        Assert.Matches(@"var\s+serialized\s*=\s*Project\(RackProject\.ForPushBack\(design\),\s*innerSource\);", pushBack);
        Assert.Matches(@"RackAuthoredComparatorPorts\.PushBack\(\),\s*comparison,\s*RackInsertedViewSource\.Edited\(source,\s*rackId,\s*rackName,\s*serialized\),\s*design,\s*rackId,\s*rackName,\s*serialized,", pushBack);

        var classic = WorkspaceHostSource.BodyOf(WorkspaceHostSource.Code("src/RackCad.Plugin/RackPushBackCommands.cs"),
            @"internal\s+static\s+void\s+EditPushBack\s*\(");
        Assert.Contains("RackViewBatchProducts.PushBack(document, system, design, id, name, embed, project, authorized.AuthoredInput)", classic, StringComparison.Ordinal);
        Assert.Equal(3, Regex.Matches(classic, @"BuildPushBackPayload\(design,\s*id,\s*name,").Count);

        var window = WorkspaceHostSource.BodyOf(WorkspaceHostSource.Code("src/RackCad.Plugin/RackPushBackWindowDrawing.cs"),
            @"internal\s+static\s+\(int\s+Redrawn,\s*int\s+Erased\)\s+Apply\s*\(");
        Assert.Contains("RackViewBatchProducts.PushBack(document, system, design, id, name, embed, project, authorized.AuthoredInput)", window, StringComparison.Ordinal);
        Assert.Equal(3, Regex.Matches(window, @"RackPushBackCommands\.BuildPushBackPayload\(design,\s*id,\s*name,").Count);

        // RACKPUSHBACKINSERTAR («Insertar vista…» of the window) runs that half with the draft the window sent and the project of the picked view.
        var door = WorkspaceHostSource.BodyOf(WorkspaceHostSource.Code("src/RackCad.Plugin/VentanaRackCad/RackCadPushBackDoorCommands.cs"),
            @"private\s+static\s+void\s+Write\s*\(");
        Assert.Contains("RackPushBackWindowDrawing.Apply(document, blockId, embed, plan.Project, write.Design, system, pending.RackId, write.InsertViews)", door, StringComparison.Ordinal);
    }

    // ================================================================== Cabecera

    private const string CabeceraName = "Cabecera 1";
    private const string CabeceraRenamed = "Cabecera 1 renombrada";

    private static RackFrameConfiguration Cabecera(double depthDelta, string name)
    {
        var configuration = new HardcodedStandardRackFrameService(Catalog).CreateDefault();
        configuration.Depth += depthDelta;
        configuration.Name = name;
        return configuration;
    }

    private static string CabeceraJson(RackFrameConfiguration configuration, RackProject? source = null)
        => new RackProjectStore().Serialize(RackProject.ForSelective(configuration).WithSourceMetadataFrom(source));

    // The design an existing view receives from the drawing half (BuildCabeceraPayload): the edited cabecera with the source metadata of that
    // view, and the envelope of that view with the identity and the name of the edit (a cabecera writes its section as -1).
    private static RackEmbedDocument CabeceraRedrawn(RackEmbedDocument view, RackFrameConfiguration edited, string name)
        => RackEmbedComposer.Compose(view, RackEmbedDocument.KindCabecera, RackId, name, view.View, -1,
            CabeceraJson(edited, new RackProjectStore().Deserialize(view.Design)));

    // The preparation of one new view by the queue of ID18, composed as RackViewBatchProducts.Header composes it.
    private static RackProductPrepareResult<HeaderRunPlan> PrepareCabecera(RackEmbedDocument source, RackAuthoredInput before,
        RackFrameConfiguration configuration, RackViewAddress address, string rackName)
    {
        var catalog = Catalog;
        var facts = new CabeceraViewAvailabilityFacts();
        var preparer = new RackProductPreparer<RackFrameConfiguration, RackFrameConfiguration, HeaderRunPlan>(
            RackResolvePorts.Cabecera<RackFrameConfiguration, RackFrameConfiguration>(_ => configuration),
            RackViewPreparationPorts.Cabecera<RackFrameConfiguration, HeaderRunPlan>(
                (resolved, target) => target.Kind == DimensionViewKind.Planta
                    ? HeaderInstanceGrouper.Group(new PlantaHeaderLayoutBuilder().Build(resolved, catalog), RackViewBaseName.CabeceraPlanta(resolved.Name))
                    : HeaderInstanceGrouper.Group(new LateralHeaderLayoutBuilder().Build(resolved, LateralHeaderParametersFactory.FromConfiguration(resolved), catalog).Instances,
                        RackViewBaseName.CabeceraLateral(catalog, resolved, resolved.Name)),
                _ => true,
                RackBlockRequirementExtractors.HeaderRun),
            CabeceraViewFrameAdapter.Resolve,
            (resolved, target) => RackViewProductNames.Header(catalog, resolved, target, rackName));
        var accepted = RackProductIntentAcceptance.Accept(new ExistingRackViewIntent<RackAuthoredInput>(
            new ExistingRackContext<RackAuthoredInput>(source, before),
            new RackProductViewRequest(RackViewProductOperation.InsertSibling, RackSystemKind.Selective, address, facts)));
        Assert.True(accepted.IsAccepted, accepted.CauseCode);
        return preparer.PrepareExisting(accepted.Intent, RackAuthoredComparatorPorts.Cabecera());
    }

    private static double Depth(RackEmbedDocument envelope) => new RackProjectStore().Deserialize(envelope.Design)!.Header.Depth;

    [Fact]
    public void ANewCabeceraViewInsertedByTheEditThatRedrawsTheOthersCarriesTheEditedDesignAndName()
    {
        // The drawing: «Cabecera 1» with its lateral. RACKEDITAR: six inches more of depth and the cabecera renamed, «Insertar» › planta.
        var drawn = Cabecera(0.0, CabeceraName);
        var lateral = View(RackEmbedDocument.KindCabecera, RackId, CabeceraName, CabeceraJson(drawn), RackEmbedDocument.ViewLateral, -1);
        var config = Cabecera(6.0, CabeceraRenamed);
        var name = string.IsNullOrWhiteSpace(config.Name) ? lateral.Name : config.Name; // the name of EditCabecera
        var project = new RackProjectStore().Deserialize(lateral.Design);
        var serialized = CabeceraJson(config, project);

        var redrawn = CabeceraRedrawn(lateral, config, name);
        var result = PrepareCabecera(RackInsertedViewSource.Edited(lateral, RackId, name, serialized), Before(RackId, ("3B1", lateral)), config,
            RackViewAddress.Whole(DimensionViewKind.Planta), name);

        Assert.True(result.IsSuccess, result.CauseCode + ": " + result.Diagnostic);
        var inserted = result.Product.Envelope;
        Assert.Equal(RackEmbedDocument.ViewPlanta, inserted.View);
        Assert.Equal(serialized, inserted.Design);
        Assert.Equal(redrawn.Design, inserted.Design); // byte for byte, as the view redrawn
        Assert.Equal(drawn.Depth + 6.0, Depth(inserted));
        Assert.Equal((RackEmbedDocument.KindCabecera, RackId, CabeceraRenamed), (inserted.Kind, inserted.Id, inserted.Name));
        Assert.Equal(RackSiblingDesignsOutcome.Same, Siblings(new[] { redrawn, inserted }));
    }

    [Fact]
    public void WhatB1WasInCabeceraThePickedViewAsTheDrawingHeldItGivesTheNewViewTheDesignAndTheNameBeforeTheEdit()
    {
        var drawn = Cabecera(0.0, CabeceraName);
        var lateral = View(RackEmbedDocument.KindCabecera, RackId, CabeceraName, CabeceraJson(drawn), RackEmbedDocument.ViewLateral, -1);
        var config = Cabecera(6.0, CabeceraRenamed);

        var asRead = PrepareCabecera(lateral, Before(RackId, ("3B1", lateral)), config, RackViewAddress.Whole(DimensionViewKind.Planta), CabeceraRenamed);

        Assert.True(asRead.IsSuccess, asRead.CauseCode);
        Assert.Equal(lateral.Design, asRead.Product.Envelope.Design);
        Assert.Equal(drawn.Depth, Depth(asRead.Product.Envelope));
        Assert.Equal(CabeceraName, asRead.Product.Envelope.Name);
        Assert.Equal(RackSiblingDesignsOutcome.Divergent, Siblings(new[] { CabeceraRedrawn(lateral, config, CabeceraRenamed), asRead.Product.Envelope }));
    }

    // M-04 of the review of B-1 in Dinámico, now in Cabecera: a legacy view without identity takes the identity of the window for «Insertar»
    // (EditCabecera: window.RackId when embed.Id is blank). Before, the queue refused it at the acceptance (RACK_ID_INVALID); with the edited
    // source the acceptance takes it and the AUTH-13 gate refuses it, whatever the scan gave as evidence. Refused before any geometry, both.
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ALegacyCabeceraWithoutIdentityIsStillRefusedBeforeAnyGeometryNowByTheAuthoredGate(string legacyId)
    {
        const string WindowId = "9d3c1b7a-2e4f-4a6b-8c0d-1e2f3a4b5c6d";
        var drawn = Cabecera(0.0, CabeceraName);
        var lateral = View(RackEmbedDocument.KindCabecera, legacyId, CabeceraName, CabeceraJson(drawn), RackEmbedDocument.ViewLateral, -1);
        var config = Cabecera(6.0, CabeceraRenamed);
        var serialized = CabeceraJson(config, new RackProjectStore().Deserialize(lateral.Design));
        var request = new RackProductViewRequest(RackViewProductOperation.InsertSibling, RackSystemKind.Selective,
            RackViewAddress.Whole(DimensionViewKind.Planta), new CabeceraViewAvailabilityFacts());

        var withPicked = Before(WindowId, ("3B1", lateral));
        var withoutSiblings = new RackAuthoredInput(WindowId, new List<RackAuthoredSibling>(), true, "PushBackCabeceraInsertedViewDesignTests");

        // The picked view as the drawing held it (before T6 b1): refused at the acceptance.
        var asRead = RackProductIntentAcceptance.Accept(new ExistingRackViewIntent<RackAuthoredInput>(
            new ExistingRackContext<RackAuthoredInput>(lateral, withPicked), request));
        Assert.False(asRead.IsAccepted);
        Assert.Equal("RACK_ID_INVALID", asRead.CauseCode);

        // The edited source: accepted, and refused by the AUTH-13 gate with either evidence.
        foreach (var before in new[] { withPicked, withoutSiblings })
        {
            var edited = RackInsertedViewSource.Edited(lateral, WindowId, CabeceraRenamed, serialized);
            var accepted = RackProductIntentAcceptance.Accept(new ExistingRackViewIntent<RackAuthoredInput>(
                new ExistingRackContext<RackAuthoredInput>(edited, before), request));
            Assert.True(accepted.IsAccepted, accepted.CauseCode);
            var preparer = new RackProductPreparer<RackFrameConfiguration, RackFrameConfiguration, HeaderRunPlan>(
                RackResolvePorts.Cabecera<RackFrameConfiguration, RackFrameConfiguration>(_ => throw new InvalidOperationException("resolved before the gate")),
                RackViewPreparationPorts.Cabecera<RackFrameConfiguration, HeaderRunPlan>(
                    (_, __) => throw new InvalidOperationException("prepared before the gate"), _ => true, RackBlockRequirementExtractors.HeaderRun),
                CabeceraViewFrameAdapter.Resolve,
                (_, __) => CabeceraRenamed);
            var result = preparer.PrepareExisting(accepted.Intent, RackAuthoredComparatorPorts.Cabecera());
            Assert.False(result.IsSuccess);
            Assert.Equal("AUTHORED_UNREADABLE", result.CauseCode);
            Assert.Null(result.Product);
        }
    }

    // RackViewBatchProducts.Header gives the queue the edited source; RACKEDITAR «Insertar» on a cabecera (EditCabecera) hands it the cabecera,
    // the identity and the name with which it redraws its lateral and its planta. The door of a cabecera of its own (RACKCABECERAABRIR and
    // RACKCABECERAAPLICAR) inserts no view: it reads, or redraws in place with ApplyCabeceraUpdate, which prepares no queue.
    [Fact]
    public void TheQueueOfCabeceraPreparesTheNewViewFromTheEditedSourceAndTheDoorOfCabecerasInsertsNoView()
    {
        var products = WorkspaceHostSource.Code("src/RackCad.Plugin/Views/RackViewBatchProducts.cs");
        var header = WorkspaceHostSource.BodyOf(products, @"internal\s+static\s+[\w<>,\s]+\s+Header\s*\(");
        Assert.Matches(@"var\s+serialized\s*=\s*Project\(RackProject\.ForSelective\(configuration\),\s*innerSource\);", header);
        Assert.Matches(@"RackAuthoredComparatorPorts\.Cabecera\(\),\s*comparison,\s*RackInsertedViewSource\.Edited\(source,\s*rackId,\s*rackName,\s*serialized\),\s*configuration,\s*rackId,\s*rackName,\s*serialized,", header);

        var commands = WorkspaceHostSource.Code("src/RackCad.Plugin/RackCabeceraCommands.cs");
        var edit = WorkspaceHostSource.BodyOf(commands, @"internal\s+static\s+void\s+EditCabecera\s*\(");
        Assert.Matches(@"RackViewBatchProducts\.Header\(\s*document,\s*config,\s*id,\s*name,\s*embed,\s*project,\s*authorized\.AuthoredInput\)", edit);
        Assert.Equal(2, Regex.Matches(edit, @"BuildCabeceraPayload\(config,\s*id,\s*name,").Count);

        var door = WorkspaceHostSource.Code("src/RackCad.Plugin/VentanaRackCad/RackCadCabeceraDoorCommands.cs");
        Assert.Equal(2, Regex.Matches(door, @"\[CommandMethod\(").Count);
        Assert.Contains("RackCabeceraCommands.ApplyCabeceraUpdate(", door, StringComparison.Ordinal);
        Assert.DoesNotContain("RackViewBatch", door, StringComparison.Ordinal);
        var update = WorkspaceHostSource.Code("src/RackCad.Plugin/RackCabeceraCommands.Door.cs");
        Assert.DoesNotContain("RackViewBatch", update, StringComparison.Ordinal);
        Assert.DoesNotContain("TryAuthorize", update, StringComparison.Ordinal);
    }
}
