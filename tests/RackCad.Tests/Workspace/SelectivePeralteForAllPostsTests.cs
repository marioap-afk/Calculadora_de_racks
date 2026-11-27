#nullable enable
using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Selective;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests;

// I-64, fifth batch, part «pp» (the request of the Owner of 2026-11-26: «en el selectivo no hay forma de colocar un peralte de poste en
// todos los postes, tienes que ir uno por uno»): «Aplicar a todos» of «Avanzado» › «Postes y cabeceras» makes one peralte the one of the
// rack and takes every own peralte away, in the draft, for the posts of every fondo; the drawing reads the peralte of each post from the
// resolved system (SelectivePostGeometry.PostPeralteAt), so after the recomposition every post has it.
public class SelectivePeralteForAllPostsTests
{
    private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

    // A new Selectivo with own peraltes on P1 (5 in) and on the last post (6 in); the peralte of the rack stays 3 in.
    private static SelectiveDraftEditor Draft()
    {
        var draft = SelectiveDraftEditor.CreateNew(Catalog);
        Assert.True(draft.System != null, draft.LastError);
        Assert.Equal(3.0, draft.PostPeralte);
        Assert.False(draft.SetPostPeralte(0, 5.0).Refused);
        Assert.False(draft.SetPostPeralte(draft.PostCount - 1, 6.0).Refused);
        Assert.True(draft.Recompose(), draft.LastError);
        return draft;
    }

    private static double[] PeraltesOfThePosts(SelectiveDraftEditor draft)
        => Enumerable.Range(0, draft.PostCount).Select(post => SelectivePostGeometry.PostPeralteAt(draft.System!, post)).ToArray();

    [Fact]
    public void OnePeralteBecomesTheOneOfTheRackAndEveryOwnPeralteGoes()
    {
        var draft = Draft();
        var last = draft.PostCount - 1;
        Assert.Equal(new[] { 0, last }, draft.OwnPostPeraltes().Select(o => o.Post).ToArray());
        Assert.Equal(5.0, PeraltesOfThePosts(draft)[0]);

        var result = draft.SetPeralteForAllPosts(4.0);

        Assert.False(result.Refused);
        Assert.True(result.Changed);
        Assert.True(result.RackPeralteChanged);
        Assert.Equal(new[] { new SelectiveOwnPostPeralte(0, 5.0), new SelectiveOwnPostPeralte(last, 6.0) }, result.Removed.ToArray());
        Assert.Equal(4.0, draft.PostPeralte);
        Assert.Empty(draft.OwnPostPeraltes());
        Assert.All(Enumerable.Range(0, draft.PostCount), post => Assert.Null(draft.PostPeralteOverride(post)));

        // The caller recomposes: then every post of the system the drawing reads has it.
        Assert.True(draft.Recompose(), draft.LastError);
        Assert.All(PeraltesOfThePosts(draft), peralte => Assert.Equal(4.0, peralte));
        Assert.Equal(4.0, draft.Design!.PostPeralte);
        Assert.All(draft.Design.PostPeraltes, own => Assert.Equal(0.0, own));

        var p = (last + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);
        Assert.Equal("Peralte de 4 in en todos los postes del borrador: se quitaron 2 peraltes propios (P1: 5 in, P" + p + ": 6 in). "
                     + SelectiveLayoutRestoreResult.DiscardText, result.NoticeFor(newRack: false));
        Assert.EndsWith(SelectiveLayoutRestoreResult.NewRackDiscardText, result.NoticeFor(newRack: true));
    }

    [Fact]
    public void TheSamePeralteOfTheRackStillTakesTheOwnOnesAwayAndWithNothingToChangeItSaysSo()
    {
        var draft = Draft();
        var same = draft.SetPeralteForAllPosts(3.0);
        Assert.False(same.RackPeralteChanged);
        Assert.True(same.Changed);
        Assert.Equal(2, same.Removed.Count);
        Assert.Empty(draft.OwnPostPeraltes());

        var single = SelectiveDraftEditor.CreateNew(Catalog);
        Assert.False(single.SetPostPeralte(1, 2.5).Refused);
        Assert.Equal("Peralte de 3 in en todos los postes del borrador: se quitó 1 peralte propio (P2: 2.5 in). " + SelectiveLayoutRestoreResult.DiscardText,
            single.SetPeralteForAllPosts(3.0).NoticeFor(newRack: false));

        var nothing = draft.SetPeralteForAllPosts(3.0);
        Assert.False(nothing.Changed);
        Assert.Empty(nothing.Removed);
        Assert.Equal("Todos los postes ya tienen 3 in y ninguno tiene peralte propio: el borrador no cambió.", nothing.NoticeFor(newRack: false));

        var rackOnly = draft.SetPeralteForAllPosts(3.5);
        Assert.True(rackOnly.Changed);
        Assert.Equal("Peralte de 3.5 in en todos los postes del borrador: ningún poste tenía peralte propio. " + SelectiveLayoutRestoreResult.DiscardText,
            rackOnly.NoticeFor(newRack: false));
    }

    [Theory]
    [InlineData(0.0)]
    [InlineData(-2.0)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void APeralteThatIsNotANumberGreaterThanZeroWritesNothing(double peralte)
    {
        var draft = Draft();
        var result = draft.SetPeralteForAllPosts(peralte);

        Assert.True(result.Refused);
        Assert.False(result.Changed);
        Assert.Equal(SelectiveDraftEditor.InvalidAllPostsPeralteText, result.Edit.Error);
        Assert.Equal(SelectiveDraftEditor.InvalidAllPostsPeralteText, result.NoticeFor(newRack: false));
        Assert.Equal(3.0, draft.PostPeralte);
        Assert.Equal(2, draft.OwnPostPeraltes().Count);
    }

    // ------------------------------------------------------------------ correction 1 of the review: the «medios frentes» (DG-10)

    // A new Selectivo with F1 split in two tramos that leave «rest» in for the last one with the 3 in of the rack.
    private static SelectiveDraftEditor SplitDraft(double rest)
    {
        var draft = SelectiveDraftEditor.CreateNew(Catalog);
        var probe = SelectiveSegmentFit.Of(draft.System!, draft.Catalog, new[] { 0 }, 0, new[] { 10.0 }, 2);
        var first = probe.Width - probe.PostOverhead - rest;
        Assert.False(draft.SetSegments(0, new[] { new SelectiveSegment { Length = first, Loaded = true }, new SelectiveSegment { Loaded = true } }).Refused);
        Assert.True(draft.Recompose(), draft.LastError);
        Assert.Equal(rest, FitOfF1(draft).Remainder, 6);
        return draft;
    }

    private static SelectiveSegmentFitResult FitOfF1(SelectiveDraftEditor draft)
    {
        var segments = draft.Front(0, 0)!.Segments;
        return SelectiveSegmentFit.Of(draft.System!, draft.Catalog, new[] { 0 }, 0, segments.Take(segments.Count - 1).Select(s => s.Length).ToList(), segments.Count);
    }

    private static readonly SelectivePalletDesignStore Store = new SelectivePalletDesignStore();

    private static SelectivePalletDesign Copy(SelectivePalletDesign design) => Store.Deserialize(Store.Serialize(SelectivePalletDesignDocument.From(design, null, null)))!.ToDomain();

    [Fact]
    public void APeralteThatAMedioFrenteNoLongerFitsIsRefusedWholeAndNamesTheFront()
    {
        // The intermediate post takes more with more peralte (its troquel slides with it): 0.6 in left at 3 in, less than nothing at 4 in.
        var draft = SplitDraft(rest: 0.6);
        Assert.False(draft.SetPostPeralte(1, 5.0).Refused); // an own peralte, to see that nothing of the write stays
        Assert.True(draft.Recompose(), draft.LastError);
        var before = Store.Serialize(SelectivePalletDesignDocument.From(draft.Design!, null, null));

        var all = draft.SetPeralteForAllPosts(4.0);

        Assert.True(all.Refused);
        Assert.StartsWith("No se aplicó el peralte de 4 in: el medio frente de F1 dejaría de caber, porque cada poste intermedio ocupa más con más peralte. No cabe: ",
            all.Edit.Error);
        Assert.EndsWith(" Ajusta antes sus tramos en «Matriz» (inspector del frente).", all.Edit.Error);
        Assert.Equal(3.0, draft.PostPeralte);
        Assert.Equal(5.0, draft.PostPeralteOverride(1));
        Assert.Equal(0.6, FitOfF1(draft).Remainder, 6); // the system is the one of before
        Assert.Equal(before, Store.Serialize(SelectivePalletDesignDocument.From(draft.Design!, null, null)));

        // The box of «Lo esencial» has the same rule.
        var rack = draft.SetRackPostPeralte(4.0);
        Assert.True(rack.Refused);
        Assert.StartsWith("No se aplicó el peralte de 4 in: el medio frente de F1 dejaría de caber", rack.Error);
        Assert.Equal(3.0, draft.PostPeralte);
        Assert.Equal(SelectiveDraftEditor.InvalidAllPostsPeralteText, draft.SetRackPostPeralte(0.0).Error);

        // A peralte the split still fits is applied: 0.1 in left.
        Assert.False(draft.SetPeralteForAllPosts(3.5).Refused);
        Assert.True(draft.Recompose(), draft.LastError);
        Assert.Equal(3.5, draft.PostPeralte);
        Assert.Null(draft.PostPeralteOverride(1));
        Assert.Equal(0.1, FitOfF1(draft).Remainder, 6);
        Assert.False(draft.SetRackPostPeralte(3.25).Refused);
        Assert.Equal(3.25, draft.PostPeralte);
    }

    [Fact]
    public void AMedioFrenteThatDidNotFitBeforeIsNoReasonToRefuse()
    {
        // A drawing with F1 split in tramos that never fitted (the resolver draws the front whole): the peralte is not what breaks it.
        var design = Copy(SplitDraft(rest: 0.6).Design!);
        design.Bays[0].Segments[0].Length += 5.0;
        var draft = new SelectiveDraftEditor(Catalog);
        Assert.True(draft.LoadFrom(design, null, "Selectivo"), draft.LastError);
        Assert.False(FitOfF1(draft).Fits);

        Assert.False(draft.SetPeralteForAllPosts(4.0).Refused);
        Assert.False(draft.SetRackPostPeralte(4.5).Refused);
        Assert.Equal(4.5, draft.PostPeralte);
    }

    // ------------------------------------------------------------------ correction 3 of the review: the base plates with their own peralte

    [Fact]
    public void TheBasePlatesOfACustomCabeceraWithTheirOwnPeralteAreCountedAndSaid()
    {
        var created = SelectiveDraftEditor.CreateNew(Catalog);
        var design = Copy(created.Design!);
        var custom = new RackFrameConfigurationFactory(Catalog).Build(RackFrameTemplateCatalog.FindStandardOrDefault(), created.PostId, 120.0, 42.0);
        custom.LeftBasePlate.PeralteOverride = 6.0;
        design.PostCabeceras.Clear();
        for (var post = 0; post < created.PostCount; post++) design.PostCabeceras.Add(post == 1 ? custom : null!);
        var draft = new SelectiveDraftEditor(Catalog);
        Assert.True(draft.LoadFrom(design, null, "Selectivo"), draft.LastError);
        Assert.True(draft.HasCustomCabecera(0, 1));

        Assert.Equal(new[] { "P2" }, draft.CabecerasWithOwnPlatePeralte().ToArray());
        var result = draft.SetPeralteForAllPosts(4.0);

        Assert.Equal(new[] { "P2" }, result.PlatesKept.ToArray());
        Assert.Equal("Peralte de 4 in en todos los postes del borrador: ningún poste tenía peralte propio. "
                     + "La cabecera personalizada de P2 tiene placas base con peralte propio: no cambian con el poste. " + SelectiveLayoutRestoreResult.DiscardText,
            result.NoticeFor(newRack: false));
        Assert.True(draft.HasCustomCabecera(0, 1)); // PP-S4: the custom cabecera stays custom
        Assert.Equal("Las cabeceras personalizadas de P2 y P3 · Fondo 2 tienen placas base con peralte propio: no cambian con el poste.",
            SelectiveDraftEditor.OwnPlatePeralteText(new[] { "P2", "P3 · Fondo 2" }));
        Assert.Equal(string.Empty, SelectiveDraftEditor.OwnPlatePeralteText(Array.Empty<string>()));
    }

    [Fact]
    public void ItIsAWriteOfTheWholeRackWhateverTheFondosOfTheViewAre()
    {
        var draft = Draft();
        Assert.False(draft.SetFondoCount(2).Refused);
        Assert.True(draft.Recompose(), draft.LastError);
        draft.SetFondoScope(1); // «Fondo 2» over the matrix: the own peralte is of the post in every fondo

        var result = draft.SetPeralteForAllPosts(4.5);

        Assert.Equal(2, result.Removed.Count);
        Assert.Equal(1, draft.FondoScope); // the view is not changed
        Assert.True(draft.Recompose(), draft.LastError);
        Assert.All(PeraltesOfThePosts(draft), peralte => Assert.Equal(4.5, peralte));
        Assert.Equal(2, SelectiveDepthLayout.Count(draft.System!));
    }
}
