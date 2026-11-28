#nullable enable
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Systems.Selective;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Selective;
using RackCad.UI.Systems.Selective;
using Xunit;
using static RackCad.UI.Tests.SelectiveWindowFixtures;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// T6 bom (dolor 13 of the inventory of the dialogs, IN-11): the resolver draws a «medio frente» whose tramos and intermediate posts do
    /// not fit the front as the WHOLE front, with no notice (<see cref="SelectiveMedioFrente.Resolve"/>; the domain side is pinned by
    /// <c>SelectiveSegmentFitTests</c>). The RackCad window refuses such a split (DG-10, accepted by the Owner on 2026-11-26); the dialog
    /// «Tramos del frente n» of the classic editor only refused a sum that reached the front, so it accepted one the drawing ignores. It now
    /// checks with the same rule (<see cref="SelectiveSegmentFit"/>), live and at «Aceptar».
    /// </summary>
    public sealed class SelectiveClassicMedioFrenteFitTests
    {
        private static TextBox LengthBox(Window dialog)
            => EditorWindowTestSupport.FindAll<TextBox>(dialog).First(box => box.IsEnabled);

        private static string Hint(Window dialog)
            => EditorWindowTestSupport.FindAll<TextBlock>(dialog)
                .Select(block => block.Text ?? string.Empty)
                .Single(text => text.StartsWith("No cabe", System.StringComparison.Ordinal) || text.StartsWith("Último tramo", System.StringComparison.Ordinal));

        [Fact]
        public void TheClassicDialogRefusesASplitTheDrawingWouldIgnoreAndAcceptsOneThatFits()
        {
            StaTestRunner.Run(() =>
            {
                var window = SelectiveClassicShown.OpenShown(Structure());
                try
                {
                    var catalog = window.Session.Catalog;
                    var system = new SelectiveGeometryResolver().Resolve(window.BuildDesignForTest(out _)!, catalog);
                    var width = SelectiveDepthLayout.BaysOfFondo(system, 0)[0].BeamLength;
                    var post = SelectiveSegmentFit.PostOverhead(system, catalog, 0, 0);
                    Assert.True(post > 0.0);

                    // Half a post too long: below the whole front (the old check accepted it), with no room for the intermediate post.
                    var tooLong = width - post / 2.0;
                    var refusal = SelectiveSegmentFit.RefusalOf(SelectiveSegmentFit.Of(system, catalog, new[] { 0 }, 0, new[] { tooLong }, 2), severalFondos: false);
                    Assert.NotNull(refusal);
                    string? live = null, atOk = null;
                    SelectiveClassicShown.ClickWithDialog(window, "Medio frente…", dialog =>
                    {
                        LengthBox(dialog).Text = tooLong.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
                        live = Hint(dialog);
                        EditorWindowTestSupport.ClickByContent(dialog, "Aceptar");
                        atOk = Hint(dialog);
                        Assert.True(dialog.IsVisible); // refused: the dialog stays open
                    });

                    Assert.Equal(refusal, live);
                    Assert.Equal(refusal, atOk);
                    Assert.Empty(window.BuildDesignForTest(out _)!.Bays[0].Segments); // nothing was written

                    // With room for the post (one inch left for the last tramo) it is accepted and written, and the live line discounts the post.
                    var fits = width - post - 1.0;
                    SelectiveClassicShown.ClickWithDialog(window, "Medio frente…", dialog =>
                    {
                        LengthBox(dialog).Text = fits.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
                        live = Hint(dialog);
                        EditorWindowTestSupport.ClickByContent(dialog, "Aceptar");
                    });

                    Assert.Equal(string.Format(System.Globalization.CultureInfo.InvariantCulture,
                        "Último tramo (calculado) ≈ {0:0.#} in del frente de {1:0.#} in; el poste intermedio ocupa ≈ {2:0.#} in.", 1.0, width, post), live);
                    var segments = window.BuildDesignForTest(out _)!.Bays[0].Segments;
                    Assert.Equal(2, segments.Count);
                    Assert.Equal(fits, segments[0].Length, 3);
                }
                finally { if (window.IsVisible) window.Close(); }
            });
        }

        // ------------------------------------------------------------------ P-2 of the Owner (2026-11-27): «Sí, avisar al abrir»

        // «design» with front «front» of fondo «fondo» split in two tramos and «rest» in left for the last one (a rest ≤ 0 does not fit).
        private static SelectivePalletDesign Split(SelectivePalletDesign design, int fondo, int front, double rest)
        {
            var catalog = UiSupport.LoadCatalogSafe();
            var system = new SelectiveGeometryResolver().Resolve(design, catalog);
            var fit = SelectiveSegmentFit.Of(system, catalog, new[] { fondo }, front, new[] { 10.0 }, 2);
            var bays = fondo == 0 ? design.Bays : design.ExtraFondoBays[fondo - 1];
            bays[front].Segments.Add(new SelectiveSegment { Length = fit.Width - fit.PostOverhead - rest, Loaded = true });
            bays[front].Segments.Add(new SelectiveSegment { Length = 0.0, Loaded = true });
            return design;
        }

        private static string Status(Window window) => ((TextBlock)window.FindName("StatusText")).Text ?? string.Empty;

        [Fact]
        public void TheClassicSaysAtTheOpeningWhichSplitDoesNotFitAndChangesNothing()
        {
            StaTestRunner.Run(() =>
            {
                // An old drawing whose F2 never fitted (half a post too long): the resolver draws it as the whole front.
                var design = Split(Structure(), 0, 1, rest: -1.75);
                var window = SelectiveClassicShown.OpenShown(design);
                try
                {
                    Assert.Contains("El medio frente de F2 no cabe con sus postes intermedios: se dibuja como frente entero.", Status(window));
                    var segments = window.BuildDesignForTest(out _)!.Bays[1].Segments;
                    Assert.Equal(new[] { design.Bays[1].Segments[0].Length, 0.0 }, segments.Select(t => t.Length).ToArray()); // nothing changes
                }
                finally { window.Close(); }

                // With two fondos it names the fondo.
                window = SelectiveClassicShown.OpenShown(Split(TwoFondos(), 1, 1, rest: -0.4));
                try
                {
                    Assert.Contains("El medio frente de F2 del fondo 2 no cabe con sus postes intermedios: se dibuja como frente entero.", Status(window));
                }
                finally { window.Close(); }

                // A split that fits says nothing.
                window = SelectiveClassicShown.OpenShown(Split(Structure(), 0, 1, rest: 0.6));
                try
                {
                    Assert.DoesNotContain("medio frente", Status(window));
                }
                finally { window.Close(); }
            });
        }
    }
}
