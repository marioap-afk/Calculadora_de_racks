#nullable enable
using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Selective;
using RackCad.Application.VentanaRackCad;
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
