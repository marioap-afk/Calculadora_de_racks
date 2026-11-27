#nullable enable
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Selective;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests;

// I-64, sixth batch, part «bom» (PP-S10 of the fifth batch, the same defect as dolor 13 of the inventory by another door): the resolver draws a
// «medio frente» whose tramos and intermediate posts no longer fit its front as the WHOLE front, with no notice. DG-10 refuses it where the
// tramos are written and T5 pp for the peralte of the rack; the guard of the draft (GuardSplits before a write, KeepTheSplits after it) refuses
// it for every other write: a split that fitted and, with the same tramos, no longer fits gives the last valid recomposition of before back.
public class SelectiveSplitGuardTests
{
    private const string Tail = " Ajusta antes sus tramos en «Matriz» (inspector del frente).";

    private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

    private static readonly SelectivePalletDesignStore Store = new SelectivePalletDesignStore();

    private static string Json(SelectiveDraftEditor draft) => Store.Serialize(SelectivePalletDesignDocument.From(draft.Design!, null, null));

    // A new Selectivo (two fronts of 96 in, the larguero of three rivets: each intermediate post takes 3.5 in) with F1 split in two tramos and
    // «rest» in left for the last one; with «both», F2 too.
    private static SelectiveDraftEditor SplitDraft(double rest, bool both = false)
    {
        var draft = SelectiveDraftEditor.CreateNew(Catalog);
        foreach (var front in both ? new[] { 0, 1 } : new[] { 0 })
        {
            var probe = SelectiveSegmentFit.Of(draft.System!, draft.Catalog, new[] { 0 }, front, new[] { 10.0 }, 2);
            Assert.False(draft.SetSegments(front, new[] { new SelectiveSegment { Length = probe.Width - probe.PostOverhead - rest, Loaded = true }, new SelectiveSegment { Loaded = true } }).Refused);
            Assert.True(draft.Recompose(), draft.LastError);
        }

        Assert.Equal(rest, Fit(draft, 0).Remainder, 6);
        return draft;
    }

    private static SelectiveSegmentFitResult Fit(SelectiveDraftEditor draft, int front)
    {
        var segments = draft.Front(0, front)!.Segments;
        return SelectiveSegmentFit.Of(draft.System!, draft.Catalog, new[] { 0 }, front, segments.Take(segments.Count - 1).Select(s => s.Length).ToList(), segments.Count);
    }

    private static SelectiveMatrixPosition[] CellsOfF1(SelectiveDraftEditor draft)
        => Enumerable.Range(0, draft.Front(0, 0)!.LevelCount).Select(level => new SelectiveMatrixPosition(0, level)).ToArray();

    [Fact]
    public void TheOwnPeralteOfTheLeftPostOfASplitFrontThatLeavesItWithNoRoomGivesTheRecompositionOfBeforeBack()
    {
        // The troquel of the larguero slides with the peralte of the post: P1 from 3 to 4 in takes 1 in more of F1, and 0.6 in is not enough.
        var draft = SplitDraft(rest: 0.6);
        var json = Json(draft);
        var design = draft.Design;
        var system = draft.System;

        var guard = draft.GuardSplits();
        Assert.False(draft.SetPostPeralte(0, 4.0).Refused);
        Assert.True(draft.Recompose(), draft.LastError);
        var after = Fit(draft, 0);
        Assert.False(after.Fits);

        Assert.False(draft.KeepTheSplits(guard));
        Assert.Equal("No se aplicó el cambio: el medio frente de F1 dejaría de caber. " + SelectiveSegmentFit.RefusalOf(after, severalFondos: false) + Tail,
            draft.LastError);
        Assert.Same(design, draft.Design); // the last valid recomposition is the one of before the write
        Assert.Same(system, draft.System);

        // The path of a write that cannot be recomposed puts every input back.
        Assert.True(draft.RestoreLastValid());
        Assert.Null(draft.PostPeralteOverride(0));
        Assert.Equal(json, Json(draft));
        Assert.True(draft.Recompose(), draft.LastError);
        Assert.Equal(json, Json(draft));
        Assert.Equal(0.6, Fit(draft, 0).Remainder, 6);

        // With room left it keeps the write: 3.5 in leaves 0.1 in.
        guard = draft.GuardSplits();
        Assert.False(draft.SetPostPeralte(0, 3.5).Refused);
        Assert.True(draft.Recompose(), draft.LastError);
        Assert.True(draft.KeepTheSplits(guard), draft.LastError);
        Assert.Equal(3.5, draft.PostPeralteOverride(0));
        Assert.Equal(0.1, Fit(draft, 0).Remainder, 6);
    }

    [Fact]
    public void ALargueroWithALongerMensulaThatLeavesASplitWithNoRoomIsRefused()
    {
        // The ménsula of the «escalón infinito» starts further out: each intermediate post takes 3.603 in instead of 3.5.
        var draft = SplitDraft(rest: 0.05);
        var json = Json(draft);
        const string Infinito = "LARGUERO_ESCALON_INFINITO";
        Assert.Contains(draft.BeamChoices, beam => beam.Id == Infinito);

        var guard = draft.GuardSplits();
        Assert.False(draft.SetBeam(CellsOfF1(draft), Infinito).Refused);
        Assert.True(draft.Recompose(), draft.LastError);
        Assert.False(draft.KeepTheSplits(guard));
        Assert.StartsWith("No se aplicó el cambio: el medio frente de F1 dejaría de caber. No cabe: ", draft.LastError);
        Assert.Contains("el poste intermedio ocupa ≈ 3.603 in", draft.LastError);

        Assert.True(draft.RestoreLastValid());
        Assert.Equal(json, Json(draft));
        Assert.NotEqual(Infinito, draft.Cell(0, 0, 0)!.BeamId);
    }

    [Fact]
    public void SeveralSplitsBrokenByOneWriteAreNamedAndTheFirstSaysWhy()
    {
        // Less «Holgura horizontal» makes every front narrower.
        var draft = SplitDraft(rest: 0.6, both: true);
        var guard = draft.GuardSplits();
        draft.PalletTolerance -= 1.0;
        Assert.True(draft.Recompose(), draft.LastError);
        var first = Fit(draft, 0);

        Assert.False(draft.KeepTheSplits(guard));
        Assert.Equal("No se aplicó el cambio: los medios frentes de F1 y F2 dejarían de caber. En F1: " + SelectiveSegmentFit.RefusalOf(first, severalFondos: false) + Tail,
            draft.LastError);
    }

    [Fact]
    public void ASplitThatDidNotFitNewTramosAndAFrontThatIsGoneAreNoReason()
    {
        // An old drawing whose F1 never fitted (the resolver draws it whole): the write is not what breaks it (PP-S8).
        var fitting = SplitDraft(rest: 0.6);
        var design = Store.Deserialize(Json(fitting))!.ToDomain();
        design.Bays[0].Segments[0].Length += 5.0;
        var draft = new SelectiveDraftEditor(Catalog);
        Assert.True(draft.LoadFrom(design, null, "Selectivo"), draft.LastError);
        Assert.False(Fit(draft, 0).Fits);
        var guard = draft.GuardSplits();
        Assert.False(draft.SetPostPeralte(0, 4.0).Refused);
        Assert.True(draft.Recompose(), draft.LastError);
        Assert.True(draft.KeepTheSplits(guard), draft.LastError);

        // New tramos are checked where they are written (SetSegments, DG-10), not here; a split taken away is no split.
        draft = SplitDraft(rest: 0.6);
        guard = draft.GuardSplits();
        var probe = SelectiveSegmentFit.Of(draft.System!, draft.Catalog, new[] { 0 }, 0, new[] { 10.0 }, 2);
        Assert.False(draft.SetSegments(0, new[] { new SelectiveSegment { Length = probe.Width - probe.PostOverhead - 2.0, Loaded = true }, new SelectiveSegment() }).Refused);
        Assert.True(draft.Recompose(), draft.LastError);
        Assert.True(draft.KeepTheSplits(guard), draft.LastError);
        guard = draft.GuardSplits();
        Assert.False(draft.SetSegments(0, null).Refused);
        Assert.True(draft.Recompose(), draft.LastError);
        Assert.True(draft.KeepTheSplits(guard), draft.LastError);

        // A front that is gone takes its split with it.
        draft = SplitDraft(rest: 0.6, both: true);
        guard = draft.GuardSplits();
        Assert.False(draft.SetFrontCount(1).Refused);
        Assert.True(draft.Recompose(), draft.LastError);
        Assert.True(draft.KeepTheSplits(guard), draft.LastError);
        Assert.Equal(1, draft.FrontCount(0));

        // With no split, the guard has nothing to keep.
        Assert.True(SelectiveDraftEditor.CreateNew(Catalog).KeepTheSplits(SelectiveDraftEditor.CreateNew(Catalog).GuardSplits()));
    }
}
