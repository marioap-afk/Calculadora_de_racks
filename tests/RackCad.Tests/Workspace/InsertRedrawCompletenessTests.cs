#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.PushBack.Draft;
using RackCad.Application.Systems.Shared;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Views.Batch;
using RackCad.Application.Views.Preparation;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests;

// T6 b1, corrections of the review of dc0ada4c (I-64, sixth batch).
// - I-02 of the review of B-1, scenario A: the drawing half of «Insertar» (RACKEDITAR of Dinámico, Push Back, Cantilever and Cabecera, and
//   «Insertar vista…» of the window) answered the queue of ID18 with `_ => Applied()` whatever happened. ViewBlockDraw.RedrawInPlace turns
//   any exception into a failure and the loops only stop counting that view, so the queue placed the new views (with the draft, since B-1)
//   next to a view that kept the design of before: siblings of different designs. RackInsertRedrawOutcome.Of stops the placement then.
// - Scenario B: when every view that existed is obsolete (survivors == 0) they were neither erased nor redrawn, and the queue placed the new
//   view with the draft next to them (Divergent; the door M-02 refuses the window afterwards). They are now erased after the first new view is
//   placed (RackInsertRedrawOutcome.ErasesStaleAfterPlacement): never before, so a cancelled placement never leaves the rack without views.
// - Point 6: a frontal cut of side B of a Push Back that the edit turns into a one-way rack was redrawn with the geometry of side A under the
//   descriptor of B. It is now obsolete, as a lateral of a post that no longer exists.
public class InsertRedrawCompletenessTests
{
    // ================================================================== the rule

    [Theory]
    [InlineData(3, 0, 3, 0, true)]   // every view redrawn
    [InlineData(3, 0, 2, 0, false)]  // scenario A: one redraw failed
    [InlineData(2, 0, 0, 0, false)]  // every redraw failed
    [InlineData(3, 1, 2, 1, true)]   // an obsolete view erased at once (there is a survivor), the others redrawn
    [InlineData(3, 1, 1, 1, false)]  // a survivor not redrawn
    [InlineData(3, 1, 2, 0, false)]  // the obsolete view could not be erased: it keeps the design of before
    [InlineData(1, 1, 0, 0, true)]   // scenario B: the only view is obsolete and waits for the first new view
    [InlineData(2, 2, 0, 0, true)]   // scenario B with two obsolete views
    [InlineData(0, 0, 0, 0, true)]   // nothing to redraw
    public void TheQueuePlacesOnlyWhenEveryViewThatRemainsWasRedrawn(int views, int stale, int redrawn, int erased, bool places)
    {
        var result = RackInsertRedrawOutcome.Of(views, stale, redrawn, erased);

        if (places)
        {
            Assert.Equal(RackViewBatchRedrawResultKind.Applied, result.Kind);
            return;
        }

        Assert.Equal(RackViewBatchRedrawResultKind.RolledBack, result.Kind);
        Assert.StartsWith(RackInsertRedrawOutcome.IncompleteCode + ": ", result.Diagnostic, StringComparison.Ordinal);
        Assert.Contains("no se colocó ninguna vista nueva", result.Diagnostic, StringComparison.Ordinal);
        // What was redrawn is NOT undone: the text says so instead of letting «ROLLED_BACK» of the queue suggest it.
        Assert.Contains("no se deshizo", result.Diagnostic, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(1, 1, true)]
    [InlineData(2, 2, true)]
    [InlineData(3, 1, false)]
    [InlineData(3, 0, false)]
    [InlineData(0, 0, false)]
    public void TheObsoleteViewsWaitForTheFirstNewViewOnlyWhenNoViewSurvives(int views, int stale, bool waits)
        => Assert.Equal(waits, RackInsertRedrawOutcome.ErasesStaleAfterPlacement(views, stale));

    [Fact]
    public void TheTextsSayWhatHappened()
    {
        Assert.Equal("REDRAW_INCOMPLETE: no se pudo redibujar 1 de las 3 vistas que ya había del rack, así que no se colocó ninguna vista nueva "
                     + "(sus hermanas quedarían con diseños distintos). Lo que sí se redibujó con el borrador no se deshizo: «U» deshace la orden "
                     + "entera, y «Actualizar» vuelve a intentarlo.", RackInsertRedrawOutcome.Of(3, 0, 2, 0).Diagnostic);
        Assert.StartsWith("REDRAW_INCOMPLETE: no se pudo redibujar la vista que ya había del rack, así que",
            RackInsertRedrawOutcome.Of(1, 0, 0, 0).Diagnostic, StringComparison.Ordinal);

        Assert.Equal("todas las vistas que había quedaban obsoletas con el cambio; tras colocar la vista nueva se borró 1.",
            RackInsertRedrawOutcome.StaleAfterPlacementText(1, 1, 1));
        Assert.Equal("todas las vistas que había quedaban obsoletas con el cambio; tras colocar la vista nueva se borraron 2.",
            RackInsertRedrawOutcome.StaleAfterPlacementText(2, 2, 3));
        Assert.Equal("todas las vistas que había quedaban obsoletas con el cambio; tras colocar la vista nueva se borró 1 de 2: revisa el dibujo.",
            RackInsertRedrawOutcome.StaleAfterPlacementText(2, 1, 1));
        Assert.Equal("no se colocó ninguna vista nueva, así que las vistas que el cambio deja obsoletas se conservan y el rack no cambió.",
            RackInsertRedrawOutcome.StaleAfterPlacementText(1, 0, 0));
    }

    // ================================================================== through the real queue of ID18

    // A port of the queue as the drawing halves build it (RackViewBatchExecution.Run): every view prepared, the redraw answered by the
    // drawing half, the placement counted.
    private sealed class Port : IRackViewBatchPort<string>
    {
        public Port(RackViewBatchRedrawResult redraw) => Redraw = redraw;

        public RackViewBatchRedrawResult Redraw { get; }

        public List<RackViewAddress> Placed { get; } = new();

        public bool TryAcceptVariant(RackViewBatchItem requested, out RackViewBatchItem accepted, out string diagnostic)
        {
            accepted = requested;
            diagnostic = null!;
            return true;
        }

        public RackViewBatchGateResult CheckSiblingGate(RackViewBatchRequest request) => RackViewBatchGateResult.Accept();

        public RackViewBatchPreparation<string> Prepare(RackViewBatchItem item) => RackViewBatchPreparation<string>.Success(item.Address.ToString());

        public RackViewBatchRedrawResult RedrawExisting(IReadOnlyList<string> prepared) => Redraw;

        public RackViewBatchPlacementResult Place(RackViewBatchItem item, string prepared)
        {
            Placed.Add(item.Address);
            return RackViewBatchPlacementResult.Placed();
        }
    }

    private static RackViewBatchRequest Insert(params RackViewAddress[] views)
        => new RackViewBatchRequest(RackProductSourceKind.ExistingRack, RackSystemKind.PushBack,
            views.Select(view => new RackViewBatchItem("PB-1", view)).ToList());

    [Fact]
    public void ScenarioA_AViewThatWasNotRedrawnStopsEveryPlacementWhileTheAnswerOfBeforePlacedThem()
    {
        var request = Insert(RackViewAddress.Whole(DimensionViewKind.Planta), RackViewAddress.Post(1));

        // Before: the drawing half answered Applied() although one of its three views was not redrawn; the queue placed both new views.
        var before = new Port(RackViewBatchRedrawResult.Applied());
        var placedBefore = RackViewBatchPlan<string>.Execute(request, before);
        Assert.Equal(RackViewBatchOutcome.COMPLETED, placedBefore.Outcome);
        Assert.Equal(2, before.Placed.Count);

        // Now: the answer of the rule stops the queue before the first placement, with the text of what happened.
        var now = new Port(RackInsertRedrawOutcome.Of(3, 0, 2, 0));
        var stopped = RackViewBatchPlan<string>.Execute(request, now);
        Assert.Equal(RackViewBatchOutcome.REDRAW_ROLLED_BACK, stopped.Outcome);
        Assert.Empty(now.Placed);
        Assert.Equal(0, stopped.Report.PlacedCount);
        Assert.StartsWith(RackInsertRedrawOutcome.IncompleteCode, stopped.Report.Diagnostic, StringComparison.Ordinal);
    }

    [Fact]
    public void ScenarioB_EveryViewObsoleteTheNewViewIsPlacedAndThenTheObsoleteOnesAreErased()
    {
        // A Push Back whose only view is the lateral of a post the edit removes; «Insertar» › planta.
        var now = new Port(RackInsertRedrawOutcome.Of(1, 1, 0, 0));
        var plan = RackViewBatchPlan<string>.Execute(Insert(RackViewAddress.Whole(DimensionViewKind.Planta)), now);

        Assert.Equal(RackViewBatchOutcome.COMPLETED, plan.Outcome);
        Assert.Single(now.Placed);
        Assert.True(RackInsertRedrawOutcome.ErasesStaleAfterPlacement(1, 1)); // the drawing half then erases the lateral
    }

    // The window, scenario B: the new views are the rack and carry the draft, so the draft is written. The other answers do not change.
    [Fact]
    public void TheWindowTakesTheDraftAsWrittenWhenTheNewViewsReplacedTheObsoleteOnes()
    {
        var replaced = RackDoorRules.AnswerForInsert(9, "PB-1", redrawn: 0, erased: 1, inserted: 1);
        Assert.True(replaced.Written);
        Assert.Equal("AutoCAD insertó 1 vista nueva con el borrador y borró 1 vista que ya no existe con el cambio.", replaced.Message);

        Assert.False(RackDoorRules.AnswerForInsert(8, "PB-1", redrawn: 0, erased: 0, inserted: 2).Written);
        Assert.Equal(RackDoorRules.AnswerForApply(7, "PB-1", 0, 1), RackDoorRules.AnswerForInsert(7, "PB-1", redrawn: 0, erased: 1, inserted: 0));
        Assert.True(RackDoorRules.AnswerForInsert(6, "PB-1", redrawn: 2, erased: 1, inserted: 1).Written);
    }

    // ================================================================== the five drawing halves (the Plugin needs AutoCAD: read as text)

    public static TheoryData<string, string, string> Halves() => new()
    {
        { "src/RackCad.Plugin/RackPushBackCommands.cs", @"internal\s+static\s+void\s+EditPushBack\s*\(",
            "RackInsertRedrawOutcome.Of(blocks.Count, staleViewBlocks.Count, updatedLateral + updatedFrontal + updatedPlanta, erasedPhantoms)" },
        { "src/RackCad.Plugin/RackPushBackWindowDrawing.cs", @"internal\s+static\s+\(int\s+Redrawn,\s*int\s+Erased\)\s+Apply\s*\(",
            "RackInsertRedrawOutcome.Of(blocks.Count, staleViewBlocks.Count, updatedLateral + updatedFrontal + updatedPlanta, erasedPhantoms)" },
        { "src/RackCad.Plugin/RackDinamicoCommands.cs", @"internal\s+static\s+\(int\s+Redrawn,\s*int\s+Erased\)\s+ApplyDynamicEdit\s*\(",
            "RackInsertRedrawOutcome.Of(blocks.Count, staleViewBlocks.Count, updatedLateral + updatedFrontal + updatedPlanta, erasedPhantoms)" },
        { "src/RackCad.Plugin/RackCantileverCommands.cs", @"internal\s+static\s+\(int\s+Redrawn,\s*int\s+Erased\)\s+ApplyCantileverEdit\s*\(",
            "RackInsertRedrawOutcome.Of(blocks.Count, staleViewBlocks.Count, Redrawn(updated), erasedPhantoms)" },
        // T7 cab (review of 258c16b1, I-1): the «Insertar» of EditCabecera was extracted, as it was, to ApplyCabeceraInsert, the one drawing
        // half of RACKEDITAR «Insertar» and of RACKCABECERAINSERTAR of the window (TheInsertarOfACabeceraIsItsOneDrawingHalf, below).
        { "src/RackCad.Plugin/RackCabeceraCommands.Door.cs", @"internal\s+static\s+\(int\s+Redrawn,\s*int\s+Erased\)\s+ApplyCabeceraInsert\s*\(",
            "RackInsertRedrawOutcome.Of(lateralBlocks.Count + plantaBlocks.Count, 0, updated, 0)" },
    };

    // T7 cab (review of 258c16b1, I-1): RACKEDITAR «Insertar» of a cabecera answers the queue with the rule because it IS ApplyCabeceraInsert:
    // EditCabecera has no queue of its own.
    [Fact]
    public void TheInsertarOfACabeceraIsItsOneDrawingHalf()
    {
        var edit = Flat(WorkspaceHostSource.BodyOf(WorkspaceHostSource.Code("src/RackCad.Plugin/RackCabeceraCommands.cs"), @"internal\s+static\s+void\s+EditCabecera\s*\("));
        Assert.Contains("if (!window.UpdateOnly) { ApplyCabeceraInsert(document, blockId, embed, project, config, id, name, window.InsertViews); return; }", edit, StringComparison.Ordinal);
        Assert.DoesNotContain("RackViewBatchExecution", edit, StringComparison.Ordinal);
        Assert.DoesNotContain("RackInsertRedrawOutcome", edit, StringComparison.Ordinal);
        Assert.DoesNotContain("RackViewBatchRedrawResult", edit, StringComparison.Ordinal);
    }

    private static string Flat(string code) => Regex.Replace(code, @"\s+", " ").Replace("( ", "(").Replace(" )", ")");

    [Theory]
    [MemberData(nameof(Halves))]
    public void EveryDrawingHalfThatInsertsAnswersTheQueueWithTheRule(string file, string signature, string answer)
    {
        var body = Flat(WorkspaceHostSource.BodyOf(WorkspaceHostSource.Code(file), signature));

        Assert.DoesNotContain("RackViewBatchRedrawResult.Applied()", body, StringComparison.Ordinal);
        var run = body.IndexOf("RackViewBatchExecution.Run(document, batchProducts, batchRequest, _ => RackCad.Application.Views.Batch." + answer + ")", StringComparison.Ordinal);
        Assert.True(run >= 0, file + ": the queue must receive " + answer + " (T6 b1, I-02). If the counts of this drawing half change, change them here too.");

        // The answer is computed after the loop and the erase at once: every count it reads is final.
        Assert.True(body.LastIndexOf("RedrawInPlace(", StringComparison.Ordinal) < run || body.LastIndexOf("RedefineBlock(", StringComparison.Ordinal) < run,
            file + ": the queue is run after the redraws");
    }

    // The drawing halves with obsolete views (all but Cabecera) erase them after the first new view when none survives, and tell it.
    [Theory]
    [InlineData("src/RackCad.Plugin/RackPushBackCommands.cs", @"internal\s+static\s+void\s+EditPushBack\s*\(")]
    [InlineData("src/RackCad.Plugin/RackPushBackWindowDrawing.cs", @"internal\s+static\s+\(int\s+Redrawn,\s*int\s+Erased\)\s+Apply\s*\(")]
    [InlineData("src/RackCad.Plugin/RackDinamicoCommands.cs", @"internal\s+static\s+\(int\s+Redrawn,\s*int\s+Erased\)\s+ApplyDynamicEdit\s*\(")]
    [InlineData("src/RackCad.Plugin/RackCantileverCommands.cs", @"internal\s+static\s+\(int\s+Redrawn,\s*int\s+Erased\)\s+ApplyCantileverEdit\s*\(")]
    public void TheObsoleteViewsOfARackWithNoSurvivorAreErasedAfterTheFirstNewView(string file, string signature)
    {
        var body = Flat(WorkspaceHostSource.BodyOf(WorkspaceHostSource.Code(file), signature));
        var run = body.IndexOf("var batch = RackViewBatchExecution.Run(", StringComparison.Ordinal);
        var waits = body.IndexOf("if (RackCad.Application.Views.Batch.RackInsertRedrawOutcome.ErasesStaleAfterPlacement(blocks.Count, staleViewBlocks.Count))", StringComparison.Ordinal);
        var erase = body.IndexOf("erasedPhantoms = batch.Report.PlacedCount > 0 ? RackCommandSupport.EraseViewBlocks(document, staleViewBlocks) : 0;", StringComparison.Ordinal);
        var told = body.IndexOf("RackInsertRedrawOutcome.StaleAfterPlacementText(staleViewBlocks.Count, erasedPhantoms, batch.Report.PlacedCount)", StringComparison.Ordinal);

        Assert.True(run >= 0 && run < waits && waits < erase && erase < told, file + ": the queue, then the wait for the first new view, its erase and its line");
        // The erase at once keeps its rule (some view survives); only «Actualizar» says that the last link was kept.
        Assert.Contains("staleViewBlocks.Count > 0 && survivors > 0", body, StringComparison.Ordinal);
        Assert.Equal(2, Regex.Matches(body, Regex.Escape("RackCommandSupport.EraseViewBlocks(document, staleViewBlocks)")).Count);
    }

    [Theory]
    [InlineData("src/RackCad.Plugin/RackPushBackCommands.cs", @"internal\s+static\s+void\s+EditPushBack\s*\(", "window.UpdateOnly")]
    [InlineData("src/RackCad.Plugin/RackPushBackWindowDrawing.cs", @"internal\s+static\s+\(int\s+Redrawn,\s*int\s+Erased\)\s+Apply\s*\(", "updateOnly")]
    [InlineData("src/RackCad.Plugin/RackCantileverCommands.cs", @"internal\s+static\s+\(int\s+Redrawn,\s*int\s+Erased\)\s+ApplyCantileverEdit\s*\(", "window.UpdateOnly")]
    public void OnlyAnUpdateSaysThatTheLastLinkWasKept(string file, string signature, string updateOnly)
    {
        var body = Flat(WorkspaceHostSource.BodyOf(WorkspaceHostSource.Code(file), signature));
        Assert.Contains("if (staleViewBlocks.Count > 0 && survivors == 0 && " + updateOnly + ")", body, StringComparison.Ordinal);
    }

    // ================================================================== point 6: the frontal of side B of a rack that is no longer two-way

    private static PushBackDesign DesignOf(string fixture)
        => new RackProjectStore().Deserialize(new RackEmbedStore().Deserialize(PilotKit.Text(fixture)).Design).PushBackDesign;

    private static string Signature(HeaderRunPlan plan)
        => string.Join("\n", plan.Flatten().Instances.Select(i => string.Join("|", i.PieceId, i.BlockName,
            i.Insertion.X.ToString("R", CultureInfo.InvariantCulture), i.Insertion.Y.ToString("R", CultureInfo.InvariantCulture),
            i.RotationRadians.ToString("R", CultureInfo.InvariantCulture), i.MirroredX, i.MirroredY, i.Text)));

    [Fact]
    public void AFrontalOfSideBIsNotAvailableOnceTheRackIsOneWayAndRedrawingItWouldDrawSideA()
    {
        var catalog = PilotKit.Catalog;
        var draft = new PushBackDraftEditor(catalog);
        Assert.True(draft.LoadFrom(DesignOf("pb-20-ab"), "PB AB"), draft.LastError);
        Assert.True(draft.IsTwoSided);
        var twoWay = draft.SnapshotForWrite()!;
        draft.SetSideBPresent(false);
        Assert.True(draft.Recompose(), draft.LastError);
        var oneWay = draft.SnapshotForWrite()!;
        Assert.True(twoWay.System.IsComposite);
        Assert.False(oneWay.System.IsComposite);

        // The facts exactly as the drawing half computes them, before and after the edit.
        static PushBackViewAvailabilityFacts Facts(PushBackSystem system, RackCad.Application.Catalogs.RackCatalog catalog)
            => new(new PushBackSystemLateralBuilder().Cortes(system, catalog).Select(c => c.PostIndex), system.IsComposite);
        var syntax = RackViewCodec.Encode(RackSystemKind.PushBack, RackViewAddress.PushBackCut(RackPushBackEnd.EntradaSalida, RackPushBackSide.B));
        var decoded = RackViewCodec.Decode(syntax.Kind, syntax.View, syntax.Section);
        Assert.Equal(RackViewAvailabilityStatus.Available, RackViewAvailability.Evaluate(decoded, Facts(twoWay.System, catalog)).Status);
        Assert.Equal(RackViewAvailabilityStatus.VariantNotPresent, RackViewAvailability.Evaluate(decoded, Facts(oneWay.System, catalog)).Status);

        // What the redraw of that view did before T6 b1: the builder of a one-way rack ignores the side, so the view of B drew the cut of A,
        // and the descriptor of the view (rewritten as it came, I-42) still said B.
        var builder = new PushBackSystemFrontalBuilder();
        var sideA = Signature(builder.BuildPlan(oneWay.System, catalog, PushBackFrontalEnd.EntradaSalida, PushBackSide.A));
        Assert.False(string.IsNullOrEmpty(sideA));
        Assert.Equal(sideA, Signature(builder.BuildPlan(oneWay.System, catalog, PushBackFrontalEnd.EntradaSalida, PushBackSide.B)));

        // The panel of F4B already classified that view so (FrontalBOfOneWay, «usa RACKEDITAR»): RACKEDITAR now treats it as obsolete.
        var envelope = RackEmbedComposer.Compose(null, RackEmbedDocument.KindPushBack, "PB-AB", "PB AB", syntax.View, syntax.Section, twoWay.DesignJson);
        var disposition = PushBackViewDispositions.Of(new[] { new KeyValuePair<string, RackEmbedDocument>("4C1", envelope) }, oneWay.System, catalog);
        Assert.Equal(new[] { "4C1" }, disposition.FrontalBOfOneWay);
    }

    // The frontal branch of both drawing halves of Push Back evaluates the availability of the view before drawing it, as the lateral one.
    [Theory]
    [InlineData("src/RackCad.Plugin/RackPushBackCommands.cs", @"internal\s+static\s+void\s+EditPushBack\s*\(")]
    [InlineData("src/RackCad.Plugin/RackPushBackWindowDrawing.cs", @"internal\s+static\s+\(int\s+Redrawn,\s*int\s+Erased\)\s+Apply\s*\(")]
    public void AFrontalOfAPushBackThatIsNotAvailableIsObsoleteAndNeverRedrawn(string file, string signature)
    {
        var body = Flat(WorkspaceHostSource.BodyOf(WorkspaceHostSource.Code(file), signature));
        var frontal = body.IndexOf("else if (decoded.Address.Kind == DimensionViewKind.Frontal) {", StringComparison.Ordinal);
        var check = body.IndexOf("if (RackViewAvailability.Evaluate(decoded, availabilityFacts).Status != RackViewAvailabilityStatus.Available) { staleViewBlocks.Add(viewBlock.BlockId); continue; }", StringComparison.Ordinal);
        var redraw = body.IndexOf("new PushBackFrontalDrawService().RedrawInPlace(", StringComparison.Ordinal);

        Assert.True(frontal >= 0 && frontal < check && check < redraw, file + ": the frontal branch drops a view that is not available before redrawing it");
        Assert.True(Regex.Matches(body, Regex.Escape("staleViewBlocks.Add(viewBlock.BlockId)")).Count == 2,
            file + ": two kinds of obsolete views (the lateral of a post that is gone and the frontal of side B of a one-way rack); if another "
            + "kind is added, count it here and say why");
    }
}
