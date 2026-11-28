#nullable enable
using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Systems.Selective;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests;

// I-64 A-14, line D, fourth batch, part «ins»; DG-10 of the prototype «dialogos» (accepted by the Owner on 2026-11-26): the «(resto)» of a
// «medio frente» discounts what each intermediate post takes, and the window does not apply a split that does not fit. The data was
// [PENDIENTE] in the prototype; it is in the catalog and the resolver already uses it (SelectiveMedioFrente): each intermediate post takes
// 2 · (troquelX + inicioX). These tests pin the window to the resolver: no rule of geometry of its own.
public class SelectiveSegmentFitTests
{
    private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

    private static SelectiveDraftEditor Draft()
    {
        var draft = SelectiveDraftEditor.CreateNew(Catalog);
        Assert.True(draft.System != null, draft.LastError);
        return draft;
    }

    private static SelectiveBay Bay(SelectiveDraftEditor draft, int fondo, int front) => SelectiveDepthLayout.BaysOfFondo(draft.System!, fondo)[front];

    // The resolver, as the frontal, the plan and the BOM call it, over a copy of the bay with these tramos (the last one calculated).
    private static System.Collections.Generic.IReadOnlyList<SelectiveMedioFrente.Tramo>? Resolve(SelectiveDraftEditor draft, int front, params double[] lengths)
    {
        var bay = Bay(draft, 0, front);
        var probe = new SelectiveBay { BeamLength = bay.BeamLength };
        foreach (var length in lengths) probe.Segments.Add(new SelectiveSegment { Length = length });
        probe.Segments.Add(new SelectiveSegment());
        var troquelX = SelectiveDepthLayout.MasterGrid(draft.System!, draft.Catalog).TroquelXs[front];
        var inicioX = SelectivePostGeometry.BeamProfileStartX(draft.Catalog, bay, SelectiveRackDefaults.View);
        return SelectiveMedioFrente.Resolve(probe, troquelX, inicioX);
    }

    [Fact]
    public void WhatAnIntermediatePostTakesIsTheGapTheResolverLeavesBetweenTwoTramos()
    {
        var draft = Draft();
        var post = SelectiveSegmentFit.PostOverhead(draft.System!, draft.Catalog, 0, 0);
        Assert.True(post > 0.0, "poste intermedio " + post);

        var tramos = Resolve(draft, 0, 30.0)!;
        Assert.NotNull(tramos);
        Assert.Equal(30.0 + post, tramos[1].StartOffset, 9); // the second tramo starts after the first one and the post
        Assert.Equal(3.5, post, 6); // with the catalog of today: 2 · (troquel of the post + overhang of the ménsula)
    }

    [Fact]
    public void TheRestIsTheLastTramoOfTheResolverWithTwoAndThreeTramos()
    {
        var draft = Draft();
        var width = Bay(draft, 0, 0).BeamLength;
        foreach (var lengths in new[] { new[] { 30.0 }, new[] { 20.0, 25.0 } })
        {
            var fit = SelectiveSegmentFit.Of(draft.System, draft.Catalog, new[] { 0 }, 0, lengths, lengths.Length + 1);
            var tramos = Resolve(draft, 0, lengths)!;
            Assert.True(fit.Fits);
            Assert.Equal(tramos.Last().Length, fit.Remainder, 9);
            Assert.Equal(width - lengths.Sum() - lengths.Length * fit.PostOverhead, fit.Remainder, 9);
            Assert.Equal(lengths.Length, fit.IntermediatePosts);
            Assert.Equal(width, fit.Width);
            Assert.Null(SelectiveSegmentFit.RefusalOf(fit, severalFondos: false));
        }
    }

    [Fact]
    public void ASplitTheClassicDialogAcceptedAndTheResolverDrawsAsTheWholeFrontIsRefused()
    {
        // Dolor 13 of the inventory: the classic check only refuses a sum that reaches the front; the resolver also needs room for the post,
        // and without it draws the whole front with no notice.
        var draft = Draft();
        var width = Bay(draft, 0, 0).BeamLength;
        var post = SelectiveSegmentFit.PostOverhead(draft.System!, draft.Catalog, 0, 0);
        var length = width - post / 2.0;
        Assert.True(length < width); // the classic check («no pueden igualar ni superar el frente completo») accepts it
        Assert.Null(Resolve(draft, 0, length)); // and the drawing would be the whole front

        var fit = SelectiveSegmentFit.Of(draft.System, draft.Catalog, new[] { 0 }, 0, new[] { length }, 2);
        Assert.False(fit.Fits);
        Assert.True(fit.Remainder <= 0.0);
        var text = SelectiveSegmentFit.RefusalOf(fit, severalFondos: false);
        Assert.Equal("No cabe: los tramos suman " + Num(length) + " in y el poste intermedio ocupa ≈ 3.5 in; el frente mide ≈ " + Num(width)
                     + " in y no queda resto para el último tramo.", text);

        // The draft refuses it with that text and writes nothing; with room for the post it writes it.
        var edit = draft.SetSegments(0, new[] { new SelectiveSegment { Length = length, Loaded = true }, new SelectiveSegment() });
        Assert.True(edit.Refused);
        Assert.Equal(text, edit.Error);
        Assert.False(draft.Front(0, 0)!.IsSplit);
        Assert.Equal(1, draft.SetSegments(0, new[] { new SelectiveSegment { Length = width - post - 1.0, Loaded = true }, new SelectiveSegment() }).Written);
        Assert.True(draft.Front(0, 0)!.IsSplit);
    }

    [Fact]
    public void WithSeveralPostsTheTextCountsThemAndNamesTheFondoOfARackOfTwo()
    {
        var draft = Draft();
        var width = Bay(draft, 0, 0).BeamLength;
        var fit = SelectiveSegmentFit.Of(draft.System, draft.Catalog, new[] { 0 }, 0, new[] { width / 2.0, width / 2.0 - 1.0 }, 3);
        Assert.False(fit.Fits);
        Assert.Equal(7.0, fit.PostsTotal, 6);
        Assert.Contains("y los 2 postes intermedios ocupan ≈ 7 in;", SelectiveSegmentFit.RefusalOf(fit, severalFondos: false), StringComparison.Ordinal);
        Assert.StartsWith("No cabe en el fondo 1:", SelectiveSegmentFit.RefusalOf(fit, severalFondos: true), StringComparison.Ordinal);
    }

    [Fact]
    public void ALengthNotWrittenYetCountsZeroAndWithNoSystemNothingIsChecked()
    {
        var draft = Draft();
        var width = Bay(draft, 0, 0).BeamLength;
        var fit = SelectiveSegmentFit.Of(draft.System, draft.Catalog, new[] { 0 }, 0, new[] { 0.0, 20.0 }, 3);
        Assert.Equal(width - 20.0 - 2 * fit.PostOverhead, fit.Remainder, 9);
        Assert.True(fit.Fits);

        // Before the first recomposition there is no width: not known, and the check is skipped, as the classic dialog does with 0.
        var none = SelectiveSegmentFit.Of(null!, draft.Catalog, new[] { 0 }, 0, new[] { 900.0 }, 2);
        Assert.False(none.IsKnown);
        Assert.True(none.Fits);
        Assert.Null(SelectiveSegmentFit.RefusalOf(none, severalFondos: false));
        Assert.Equal(0.0, SelectiveSegmentFit.PostOverhead(null!, draft.Catalog, 0, 0));
        Assert.Equal(0.0, SelectiveSegmentFit.PostOverhead(draft.System!, draft.Catalog, 0, 99)); // a front the fondo does not have
    }

    // Correction 3 of the review: «no system» is not known (nothing is checked); a front that exists with no width is known, and a split
    // of it does not fit (the resolver would draw it whole).
    [Fact]
    public void AFrontThatExistsWithNoWidthIsKnownAndDoesNotFit()
    {
        var draft = Draft();
        Bay(draft, 0, 0).BeamLength = 0.0;
        var fit = SelectiveSegmentFit.Of(draft.System, draft.Catalog, new[] { 0 }, 0, new[] { 10.0 }, 2);
        Assert.True(fit.IsKnown);
        Assert.False(fit.HasWidth);
        Assert.False(fit.Fits);
        var text = SelectiveSegmentFit.RefusalOf(fit, severalFondos: false);
        Assert.Equal("No cabe: el frente no tiene ancho (≈ 0 in), así que no se puede partir en tramos.", text);

        var edit = draft.SetSegments(0, new[] { new SelectiveSegment { Length = 10.0, Loaded = true }, new SelectiveSegment() });
        Assert.True(edit.Refused);
        Assert.Equal(text, edit.Error);
        Assert.False(draft.Front(0, 0)!.IsSplit);

        // One tramo (a whole front) is still no split, and fits.
        Assert.True(SelectiveSegmentFit.Of(draft.System, draft.Catalog, new[] { 0 }, 0, Array.Empty<double>(), 1).Fits);
    }

    // Correction 9 of the review: the rule of the window against the real drawing. The frontal of the fondo plants the intermediate post
    // exactly when SelectiveSegmentFit says the split fits, and draws the whole front when it does not.
    [Fact]
    public void TheFrontalPlantsTheIntermediatePostExactlyWhenTheSplitFits()
    {
        var draft = Draft();
        var bay = Bay(draft, 0, 0);
        var width = bay.BeamLength;
        var post = SelectiveSegmentFit.PostOverhead(draft.System!, draft.Catalog, 0, 0);

        int Posts(double length)
        {
            bay.Segments.Clear();
            if (length > 0.0)
            {
                bay.Segments.Add(new SelectiveSegment { Length = length, Loaded = true });
                bay.Segments.Add(new SelectiveSegment { Loaded = false });
            }

            return new SelectiveFrontalBuilder().Build(SelectiveDepthLayout.FondoSystemView(draft.System!, 0), draft.Catalog)
                .Count(i => i.Role == HeaderBlockRole.Post);
        }

        var whole = Posts(0.0);
        foreach (var (length, fits) in new[] { (30.0, true), (width - post - 0.5, true), (width - post / 2.0, false), (width - 1.0, false) })
        {
            var fit = SelectiveSegmentFit.Of(draft.System, draft.Catalog, new[] { 0 }, 0, new[] { length }, 2);
            Assert.Equal(fits, fit.Fits);
            Assert.Equal(fits ? whole + 1 : whole, Posts(length));
        }
    }

    private static string Num(double value) => value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
}
