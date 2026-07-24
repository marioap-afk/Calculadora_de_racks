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
    /// EQUIVALENCE of the dynamic editor's preview BEFORE and AFTER the shared-infrastructure extraction (I-18b,
    /// decision 6). The pin below was captured by running this exact signature over the PRE-extraction renderer, then
    /// re-running it over the migrated one: both produced the same hash over the same 736 primitives.
    ///
    /// The signature covers every primitive in DRAW ORDER with its geometry, colour, thickness and dash, so any drift in
    /// coordinates, ordering, palette, stroke width or dashing breaks this test. That is what makes "the dynamic scene is
    /// preserved identically" a measured claim rather than an assertion.
    /// </summary>
    public sealed class DynamicPreviewExtractionEquivalenceTests
    {
        /// <summary>
        /// SHA-256 of the dynamic lateral preview's scene at 1280x800, captured from the renderer BEFORE the extraction
        /// (commit 8e46cd1) and unchanged by it. Regenerate ONLY for an intended, justified change of the dynamic preview.
        /// </summary>
        private const string PreExtractionLateralSignature =
            "B7C0D1CAA25E77C2F8C63405C40AB771FC347E5D7F8406268BAC49889E97D2E5";

        /// <summary>Primitive count of that same pre-extraction scene.</summary>
        private const int PreExtractionLateralPrimitives = 736;

        private static string Num(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);

        private static string Colour(Brush brush)
            => brush is SolidColorBrush solid ? solid.Color.ToString(CultureInfo.InvariantCulture) : "-";

        private static string Dash(DoubleCollection dash)
            => dash == null || dash.Count == 0 ? "-" : string.Join(",", dash.Select(Num));

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
                            Num(Canvas.GetLeft(rect)), Num(Canvas.GetTop(rect)), Num(rect.Width), Num(rect.Height),
                            Colour(rect.Stroke), Num(rect.StrokeThickness), Dash(rect.StrokeDashArray), Colour(rect.Fill)));
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

        [Fact]
        public void TheDynamicScene_IsIdenticalToThePreExtractionRenderer()
        {
            StaTestRunner.Run(() =>
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
                try
                {
                    w.Show();
                    w.UpdateLayout();

                    var scene = Scene((Canvas)w.FindName("PreviewCanvas"));
                    Assert.Equal(PreExtractionLateralPrimitives, scene.Count);
                    Assert.Equal(PreExtractionLateralSignature, Signature(scene));
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void TheDynamicRenderer_NowDrawsThroughTheSharedSurface_NotItsOwnPainter()
        {
            // The window must no longer own a private painter: the extraction is real, not a parallel pipeline.
            var fields = typeof(RackDynamicSystemWindow)
                .GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Select(field => field.FieldType.Name)
                .ToList();

            Assert.Contains("EditorPreviewSurface", fields);
            Assert.DoesNotContain("PreviewCanvasPainter", fields);
        }
    }
}
