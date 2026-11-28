#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.Application.VentanaRackCad;
using RackCad.Domain.Systems.Selective;
using RackCad.UI.VentanaRackCad;
using Xunit;
using static RackCad.UI.Tests.SelectiveWindowFixtures;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-64 A-14, line D, fourth batch, part «ins»: the tramos of «medio frente» in the inspector of the Selectivo editor (prototype
    /// «dialogos», DG-09 to DG-11, accepted by the Owner on 2026-11-26): one explicit gesture, «Aplicar tramos» (DG-09); the «(resto)» that
    /// discounts what each intermediate post takes, and a split that does not fit that cannot be applied (DG-10); the strip of the tramos over
    /// the table, read only (DG-11); and what each gesture says (dolores 12, 15 and 16).
    /// </summary>
    public class SelectiveHalfFrontInsTests
    {
        // F2 of the rack of the fixtures: 100 in; each intermediate post takes 3.5 in with the catalog of today.
        private static SelectiveEditorViewModel F2()
        {
            var editor = SelectiveEditor();
            editor.SelectFront(1);
            Assert.Equal(100.0, editor.Draft.FrontWidth(1));
            Assert.Equal(3.5, SelectiveSegmentFit.PostOverhead(editor.Draft.System!, editor.Draft.Catalog, 0, 1), 6);
            return editor;
        }

        [Fact]
        public void TheRestDiscountsTheIntermediatePostsAndTheStripShowsTheTramos()
        {
            var editor = F2();
            editor.StartSegments();
            Assert.Equal("(resto) ≈ 96.5 in", editor.SegmentRows[1].RestText); // 100 − 3.5
            Assert.Equal("El «(resto)» descuenta lo que ocupa cada poste intermedio: ≈ 3.5 in (el troquel del poste y el arranque de la ménsula del larguero, a cada lado).",
                editor.SegmentPostNote);

            editor.SegmentRows[0].LengthText = "44";
            Assert.Equal("(resto) ≈ 52.5 in", editor.SegmentRows[1].RestText);
            var fit = editor.SegmentFit;
            Assert.True(fit.Fits);
            Assert.Equal(52.5, fit.Remainder, 6);

            // DG-11: the strip, read only: a tramo with largueros, the post, the empty rest; each one its share of the front.
            Assert.True(editor.HasSegmentStrip);
            Assert.False(editor.SegmentStripDoesNotFit);
            var strip = editor.SegmentStrip;
            Assert.Equal(new[] { SelectiveSegmentStripKind.Loaded, SelectiveSegmentStripKind.Post, SelectiveSegmentStripKind.Empty }, strip.Select(s => s.Kind).ToArray());
            Assert.Equal(new[] { "44 in", string.Empty, "≈ 52.5 in" }, strip.Select(s => s.Text).ToArray());
            Assert.Equal(0.44, strip[0].Weight, 6);
            Assert.Equal(0.0, strip[1].Weight);
            Assert.Equal(0.525, strip[2].Weight, 6);
            Assert.Equal("F2: tramo 1 de 44 in, con largueros; poste intermedio; tramo 2 (resto) de ≈ 52.5 in, sin largueros.", editor.SegmentStripText);

            // DG-09: written with «Aplicar tramos», as before.
            Assert.True(editor.CanApplySegments);
            editor.ApplySegments();
            Assert.Equal(string.Empty, editor.SegmentError);
            Assert.True(editor.Draft.Front(0, 1)!.IsSplit);
            Assert.False(editor.CanApplySegments); // nothing pending
        }

        [Fact]
        public void ASplitThatDoesNotFitWithItsPostsIsSaidLiveAndCannotBeApplied()
        {
            // DG-10: 98 in does not reach the front, but with the post (3.5 in) nothing is left: the resolver would draw the whole front.
            var editor = F2();
            editor.StartSegments();
            editor.SegmentRows[0].LengthText = "98";
            Assert.Equal("No cabe: los tramos suman 98 in y el poste intermedio ocupa ≈ 3.5 in; el frente mide ≈ 100 in y no queda resto para el último tramo.",
                editor.SegmentFitText);
            Assert.Equal("(resto) ≈ 0 in", editor.SegmentRows[1].RestText);
            Assert.True(editor.SegmentStripDoesNotFit);
            Assert.False(editor.CanApplySegments);

            // The draft refuses it too (the gesture of the view-model): nothing is written.
            editor.ApplySegments();
            Assert.Equal(editor.SegmentFitText, editor.SegmentError);
            Assert.False(editor.Draft.Front(0, 1)!.IsSplit);
            Assert.True(editor.HasPendingSegments);

            // Shorter, it fits.
            editor.SegmentRows[0].LengthText = "96";
            Assert.Equal(string.Empty, editor.SegmentFitText);
            Assert.True(editor.CanApplySegments);
            Assert.Equal("(resto) ≈ 0.5 in", editor.SegmentRows[1].RestText);
        }

        [Fact]
        public void TheGesturesOfTheTramosSayWhatTheyDid()
        {
            var editor = F2();
            editor.StartSegments();
            Assert.Equal("Dos tramos, como la semilla del diálogo clásico: escribe la longitud del tramo 1 y pulsa «Aplicar tramos».", editor.SegmentHint);
            editor.SegmentRows[0].LengthText = "20";
            Assert.Equal(string.Empty, editor.SegmentHint);

            editor.AddSegment();
            Assert.Equal("El tramo 2 deja de ser el resto: escribe su longitud.", editor.SegmentHint);
            editor.SegmentRows[1].LengthText = "30";

            // Dolor 15: removing the last tramo makes the one before it the rest, and the length it had stops counting.
            editor.RemoveSegment(2);
            Assert.Equal("El tramo 2 pasa a ser el resto: su longitud (30 in) ya no cuenta.", editor.SegmentHint);
            Assert.Equal("(resto) ≈ 76.5 in", editor.SegmentRows[1].RestText);

            // Dolor 16: one tramo is a whole front; «Aplicar tramos» asks for two.
            editor.RemoveSegment(1);
            Assert.Equal("El tramo 1 pasa a ser el resto: su longitud (20 in) ya no cuenta. Con un solo tramo el frente es completo: agrega otro tramo, o usa «Sin medio frente».",
                editor.SegmentHint);
            Assert.Equal("Con un solo tramo, F2 es un frente completo: agrega otro tramo o usa «Sin medio frente».", editor.SegmentNote);
            Assert.False(editor.CanApplySegments);
            Assert.False(editor.HasSegmentStrip);

            editor.RemoveSegment(0);
            Assert.Equal("Sin tramos: para dejar el frente completo, «Sin medio frente»; para volver a lo de antes, «Descartar tramos».", editor.SegmentHint);

            // Another front forgets the hint.
            editor.StartSegments();
            editor.SelectFront(0);
            Assert.Equal(string.Empty, editor.SegmentHint);
        }

        [Fact]
        public void WithTwoFondosTheInspectorSaysWhereTheTramosAreWritten()
        {
            // Dolor 12: the classic dialog wrote in the «Fondos destino» and said it only after.
            var editor = SelectiveEditor(TwoFondos());
            editor.SelectFront(0);
            Assert.Equal("Se escribe en F1 de los fondos 1 y 2: los fondos de la vista que lo tienen.", editor.SegmentWhereText);
            editor.SelectFront(2); // F3 is only in fondo 2
            Assert.Equal("Se escribe en F3 del fondo 2: los fondos de la vista que lo tienen.", editor.SegmentWhereText);
            editor.FondoScopeIndex = 1;
            editor.SelectFront(0);
            Assert.Equal("Se escribe en F1 del fondo 1: los fondos de la vista que lo tienen.", editor.SegmentWhereText);

            // One fondo: nothing to say.
            var one = F2();
            Assert.Equal(string.Empty, one.SegmentWhereText);
        }

        // ------------------------------------------------------------------ the corrections of the review

        // Correction 2 (a regression of the first version): with «Todos los fondos», F2 split [30, resto] in fondo 1 and whole in fondo 2, the
        // rows read from fondo 1 are not pending, but «Aplicar tramos» equals the fondos, as the note says.
        [Fact]
        public void AplicarTramosEqualsTheFondosWhoseTramosOfTheFrontDiffer()
        {
            var design = TwoFondos();
            design.Bays[1].Segments.Add(new SelectiveSegment { Length = 30.0, Loaded = true });
            design.Bays[1].Segments.Add(new SelectiveSegment { Length = 0.0, Loaded = false });
            var editor = SelectiveEditor(design);
            editor.SelectFront(1);
            Assert.True(editor.Draft.Front(0, 1)!.IsSplit);
            Assert.False(editor.Draft.Front(1, 1)!.IsSplit);
            Assert.Equal("Los fondos de la vista no tienen los mismos tramos en F2: aplicar aquí los iguala.", editor.SegmentNote);
            Assert.False(editor.HasPendingSegments);
            Assert.True(editor.SegmentsDifferAcrossFondos);
            Assert.True(editor.CanApplySegments);

            editor.ApplySegments();
            Assert.Equal(string.Empty, editor.SegmentError);
            Assert.True(editor.Draft.Front(1, 1)!.IsSplit);
            Assert.Equal(new[] { 30.0, 0.0 }, editor.Draft.Front(1, 1)!.Segments.Select(s => s.Length).ToArray());
            Assert.False(editor.SegmentsDifferAcrossFondos);
            Assert.False(editor.CanApplySegments);
        }

        // Correction 4: a recomposition that changes the front (here the width of the pallet of the whole rack) keeps the rows written and
        // the «(resto)» follows it.
        [Fact]
        public void TheRestFollowsARecompositionThatChangesTheFront()
        {
            var editor = F2();
            editor.StartSegments();
            editor.SegmentRows[0].LengthText = "44";
            Assert.Equal("(resto) ≈ 52.5 in", editor.SegmentRows[1].RestText);

            Write(Field(editor.Essentials, "rWidth"), "48");
            var fit = editor.SegmentFit;
            Assert.NotEqual(100.0, fit.Width);
            Assert.Equal("44", editor.SegmentRows[0].LengthText);
            Assert.Equal("(resto) ≈ " + fit.Remainder.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture) + " in", editor.SegmentRows[1].RestText);
            Assert.NotEqual("(resto) ≈ 52.5 in", editor.SegmentRows[1].RestText);
        }

        // ------------------------------------------------------------------ the view, with real WPF

        private sealed class BindingErrors : TraceListener
        {
            public List<string> Messages { get; } = new();

            public override void Write(string? message)
            {
            }

            public override void WriteLine(string? message)
            {
                if (message != null) Messages.Add(message);
            }
        }

        private static IEnumerable<T> Descendants<T>(DependencyObject root) where T : DependencyObject
        {
            var count = VisualTreeHelper.GetChildrenCount(root);
            for (var i = 0; i < count; i++)
            {
                var child = VisualTreeHelper.GetChild(root, i);
                if (child is T match) yield return match;
                foreach (var nested in Descendants<T>(child)) yield return nested;
            }
        }

        [Fact]
        public void TheStripIsDrawnInProportionWithThePostsAndTheButtonOfApplyFollowsTheFit()
        {
            StaTestRunner.Run(() =>
            {
                var errors = new BindingErrors();
                PresentationTraceSources.Refresh();
                var source = PresentationTraceSources.DataBindingSource;
                var level = source.Switch.Level;
                source.Listeners.Add(errors);
                source.Switch.Level = SourceLevels.Error;
                try
                {
                    var editor = F2();
                    var view = new SelectiveEditorView(editor);
                    editor.SelectedTab = 1;
                    void Lay()
                    {
                        view.Measure(new Size(1200, 900));
                        view.Arrange(new Rect(0, 0, 1200, 900));
                        view.UpdateLayout();
                    }

                    Lay();
                    Assert.Equal(Visibility.Collapsed, view.SegmentStripBlock.Visibility);
                    Assert.Equal(Visibility.Collapsed, view.PendingSegmentsTag.Visibility);

                    editor.StartSegments();
                    editor.SegmentRows[0].LengthText = "44";
                    Lay();
                    Assert.Equal(Visibility.Visible, view.SegmentStripBlock.Visibility);
                    Assert.Equal(Visibility.Visible, view.PendingSegmentsTag.Visibility);
                    Assert.Equal("F2: tramo 1 de 44 in, con largueros; poste intermedio; tramo 2 (resto) de ≈ 52.5 in, sin largueros.",
                        UIElementAutomationPeer.CreatePeerForElement(view.SegmentStrip).GetName());
                    var panel = Descendants<SegmentStripPanel>(view.SegmentStrip).Single();
                    var pieces = panel.Children.OfType<FrameworkElement>().ToList();
                    Assert.Equal(3, pieces.Count);
                    Assert.Equal(6.0, pieces[1].ActualWidth, 3); // the post, of its own width
                    Assert.Equal(44.0 / 52.5, pieces[0].ActualWidth / pieces[2].ActualWidth, 3); // the tramos, in proportion
                    Assert.Equal(panel.ActualWidth, pieces.Sum(p => p.ActualWidth), 3);
                    Assert.True(view.ApplySegmentsButton.IsEnabled);
                    Assert.Equal(Visibility.Visible, view.SegmentPostNoteText.Visibility);

                    // It does not fit: the red line, the border of an error and «Aplicar tramos» off.
                    editor.SegmentRows[0].LengthText = "98";
                    Lay();
                    Assert.Equal(editor.SegmentFitText, view.SegmentFitText.Text);
                    Assert.Equal(Visibility.Visible, view.SegmentFitText.Visibility);
                    Assert.False(view.ApplySegmentsButton.IsEnabled);
                    Assert.Equal(((SolidColorBrush)view.FindResource("ShellStatusErrorBrush")).Color, ((SolidColorBrush)view.SegmentStripBorder.BorderBrush).Color);
                }
                finally
                {
                    source.Listeners.Remove(errors);
                    source.Switch.Level = level;
                }

                Assert.Empty(errors.Messages);
            });
        }
    }
}
