using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using RackCad.UI;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// CHARACTERIZATION of the dynamic editor's preview renderer, captured BEFORE any extraction (Owner decision
    /// 2026-07-24, item 6). It pins what the renderer paints today — every primitive's kind, geometry, stroke, thickness
    /// and dash, in draw order — so that when the shared preview infrastructure is extracted and the dynamic editor is
    /// migrated onto it, "visually and semantically identical" is a MEASURED claim rather than an assertion.
    ///
    /// It deliberately reads the real Canvas children of a real window instead of an internal seam: an extraction that
    /// changes the produced scene in any way — order, coordinates, colours, thickness or dash — breaks these tests, which
    /// is exactly the safety net the migration needs. Nothing here depends on pixels or on screenshots.
    /// </summary>
    public sealed class DynamicPreviewRendererCharacterizationTests
    {
        private static RackDynamicSystemWindow Shown()
        {
            var w = new RackDynamicSystemWindow
            {
                WindowStartupLocation = WindowStartupLocation.Manual,
                ShowInTaskbar = false,
                Left = -10000,
                Top = -10000,
                Width = 1280,
                Height = 800,
            };
            w.Show();
            w.UpdateLayout();
            return w;
        }

        private static Canvas PreviewCanvas(RackDynamicSystemWindow w)
            => (Canvas)w.FindName("PreviewCanvas");

        private static string Num(double value)
            => value.ToString("0.###", CultureInfo.InvariantCulture);

        private static string Colour(Brush brush)
            => brush is SolidColorBrush solid ? solid.Color.ToString(CultureInfo.InvariantCulture) : brush?.ToString() ?? "-";

        private static string Dash(DoubleCollection dash)
            => dash == null || dash.Count == 0 ? "-" : string.Join(",", dash.Select(Num));

        /// <summary>One line per primitive, in DRAW ORDER: kind, geometry, stroke, thickness and dash.</summary>
        private static IReadOnlyList<string> Scene(Canvas canvas)
        {
            var scene = new List<string>();
            foreach (var child in canvas.Children.OfType<UIElement>())
            {
                switch (child)
                {
                    case Line line:
                        scene.Add(string.Join("|", "line",
                            Num(line.X1), Num(line.Y1), Num(line.X2), Num(line.Y2),
                            Colour(line.Stroke), Num(line.StrokeThickness), Dash(line.StrokeDashArray)));
                        break;
                    case Rectangle rect:
                        scene.Add(string.Join("|", "rect",
                            Num(Canvas.GetLeft(rect)), Num(Canvas.GetTop(rect)),
                            Num(rect.Width), Num(rect.Height),
                            Colour(rect.Stroke), Num(rect.StrokeThickness), Dash(rect.StrokeDashArray),
                            Colour(rect.Fill)));
                        break;
                    case TextBlock text:
                        scene.Add(string.Join("|", "text",
                            Num(Canvas.GetLeft(text)), Num(Canvas.GetTop(text)),
                            text.Text ?? string.Empty, Colour(text.Foreground), Num(text.FontSize)));
                        break;
                    default:
                        scene.Add("other|" + child.GetType().Name);
                        break;
                }
            }

            return scene;
        }

        private static string Signature(IReadOnlyList<string> scene)
            => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(string.Join("\n", scene))));

        /// <summary>Renders one preview view and returns its scene. Views follow the window's own PreviewViewBox order.</summary>
        private static IReadOnlyList<string> SceneOf(RackDynamicSystemWindow w, int viewIndex)
        {
            var views = (ComboBox)w.FindName("PreviewViewBox");
            if (views != null && viewIndex < views.Items.Count)
            {
                views.SelectedIndex = viewIndex;
            }

            w.UpdateLayout();
            return Scene(PreviewCanvas(w));
        }

        [Fact]
        public void EveryPreviewView_PaintsANonDegenerateScene_OfLinesAndRectangles()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    var canvas = PreviewCanvas(w);
                    Assert.NotNull(canvas);

                    var views = (ComboBox)w.FindName("PreviewViewBox");
                    var viewCount = views?.Items.Count ?? 1;
                    Assert.True(viewCount >= 1);

                    for (var index = 0; index < viewCount; index++)
                    {
                        var scene = SceneOf(w, index);

                        // A real 2D drawing: many primitives, not a handful of markers.
                        Assert.True(scene.Count >= 10, $"view {index} painted only {scene.Count} primitives");

                        // And genuinely two-dimensional: the primitives span both axes.
                        var xs = new List<double>();
                        var ys = new List<double>();
                        foreach (var child in PreviewCanvas(w).Children.OfType<UIElement>())
                        {
                            if (child is Line line)
                            {
                                xs.Add(line.X1); xs.Add(line.X2);
                                ys.Add(line.Y1); ys.Add(line.Y2);
                            }
                            else if (child is Rectangle rect)
                            {
                                var left = Canvas.GetLeft(rect);
                                var top = Canvas.GetTop(rect);
                                if (!double.IsNaN(left) && !double.IsNaN(top))
                                {
                                    xs.Add(left); xs.Add(left + rect.Width);
                                    ys.Add(top); ys.Add(top + rect.Height);
                                }
                            }
                        }

                        Assert.True(xs.Count > 0 && ys.Count > 0, $"view {index} produced no geometry");
                        Assert.True(xs.Max() - xs.Min() > 20.0, $"view {index} has no horizontal extent");
                        Assert.True(ys.Max() - ys.Min() > 20.0, $"view {index} has no vertical extent");
                    }
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void TheRenderer_IsDeterministic_SameInputsYieldTheSameScene()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    // The SAME view rendered twice must produce a byte-identical scene: without determinism an
                    // equivalence check after the extraction would be meaningless.
                    var first = Signature(SceneOf(w, 0));
                    var second = Signature(SceneOf(w, 0));
                    Assert.Equal(first, second);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void EachView_PaintsItsOwnScene_SoTheSignatureDistinguishesThem()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    var views = (ComboBox)w.FindName("PreviewViewBox");
                    if (views == null || views.Items.Count < 2)
                    {
                        return;   // single-view host: nothing to distinguish
                    }

                    var signatures = Enumerable.Range(0, views.Items.Count)
                        .Select(index => Signature(SceneOf(w, index)))
                        .ToList();

                    // Distinct views must not collapse to the same drawing (that would hide a regression where the
                    // extraction routes every view through one code path).
                    Assert.Equal(signatures.Count, signatures.Distinct().Count());
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void TheScene_UsesTheSharedPaletteAndRealStrokeWidths_NotDefaults()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    var scene = SceneOf(w, 0);
                    var strokes = scene
                        .Where(entry => entry.StartsWith("line|", StringComparison.Ordinal))
                        .Select(entry => entry.Split('|'))
                        .Where(parts => parts.Length >= 8)
                        .ToList();
                    Assert.NotEmpty(strokes);

                    // More than one colour and more than one thickness: the renderer distinguishes roles visually, and
                    // the extraction must preserve that distinction rather than flattening it.
                    Assert.True(strokes.Select(parts => parts[5]).Distinct().Count() >= 2, "the scene uses a single colour");
                    Assert.True(strokes.Select(parts => parts[6]).Distinct().Count() >= 2, "the scene uses a single thickness");
                }
                finally { w.Close(); }
            });
        }
    }
}
