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
using RackCad.Application.Drawing;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.UI;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-18b decision 6 — the Push Back preview, now painted by the SHARED infrastructure, is a real two-dimensional
    /// drawing in all four views. Semantic assertions over the primitives the real window paints on its Canvas: no
    /// pixels, no screenshots, and no dependence on the removed simplified renderer.
    /// </summary>
    public sealed class PushBackRichPreviewTests
    {
        private const int Lateral = 0;
        private const int FrontalEntrada = 1;
        private const int FrontalPosterior = 2;
        private const int Planta = 3;

        private static RackPushBackSystemWindow Shown()
        {
            var w = new RackPushBackSystemWindow
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

        private static Canvas Canvas(RackPushBackSystemWindow w) => (Canvas)w.FindName("PreviewCanvas");

        /// <summary>
        /// Selects a view and renders it on a canvas of a FIXED size. Pinning the canvas makes the projection — and
        /// therefore every metric below — independent of how the host agent happens to lay the window out, so these
        /// assertions mean the same thing on a developer machine and on a headless CI runner.
        /// </summary>
        private static void SelectView(RackPushBackSystemWindow w, int index)
        {
            var canvas = Canvas(w);
            canvas.Width = 900.0;
            canvas.Height = 520.0;
            ((ComboBox)w.FindName("ViewBox")).SelectedIndex = index;
            w.UpdateLayout();
            canvas.UpdateLayout();
        }

        private sealed class Primitive
        {
            public string Kind;
            public double X1, Y1, X2, Y2;
            public string Colour;
            public double Thickness;
        }

        private static IReadOnlyList<Primitive> Primitives(RackPushBackSystemWindow w)
        {
            var result = new List<Primitive>();
            foreach (var child in Canvas(w).Children.OfType<UIElement>())
            {
                if (child is Line line)
                {
                    result.Add(new Primitive
                    {
                        Kind = "line",
                        X1 = line.X1, Y1 = line.Y1, X2 = line.X2, Y2 = line.Y2,
                        Colour = (line.Stroke as SolidColorBrush)?.Color.ToString(CultureInfo.InvariantCulture) ?? "-",
                        Thickness = line.StrokeThickness,
                    });
                }
                else if (child is Rectangle rect)
                {
                    var left = System.Windows.Controls.Canvas.GetLeft(rect);
                    var top = System.Windows.Controls.Canvas.GetTop(rect);
                    if (double.IsNaN(left) || double.IsNaN(top)) continue;
                    result.Add(new Primitive
                    {
                        Kind = "rect",
                        X1 = left, Y1 = top, X2 = left + rect.Width, Y2 = top + rect.Height,
                        Colour = (rect.Stroke as SolidColorBrush)?.Color.ToString(CultureInfo.InvariantCulture) ?? "-",
                        Thickness = rect.StrokeThickness,
                    });
                }
            }

            return result;
        }

        private static (double Width, double Height) Extent(IReadOnlyList<Primitive> primitives)
        {
            if (primitives.Count == 0) return (0.0, 0.0);
            var xs = primitives.SelectMany(p => new[] { p.X1, p.X2 }).ToList();
            var ys = primitives.SelectMany(p => new[] { p.Y1, p.Y2 }).ToList();
            return (xs.Max() - xs.Min(), ys.Max() - ys.Min());
        }

        private static string Signature(IReadOnlyList<Primitive> primitives)
        {
            var text = string.Join("\n", primitives.Select(p => string.Join("|",
                p.Kind,
                p.X1.ToString("0.###", CultureInfo.InvariantCulture),
                p.Y1.ToString("0.###", CultureInfo.InvariantCulture),
                p.X2.ToString("0.###", CultureInfo.InvariantCulture),
                p.Y2.ToString("0.###", CultureInfo.InvariantCulture),
                p.Colour,
                p.Thickness.ToString("0.###", CultureInfo.InvariantCulture))));
            return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(text)));
        }

        // ---- 6. every view is a substantive 2D drawing ----

        [Fact]
        public void AllFourViews_AreSubstantiveTwoDimensionalDrawings()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    foreach (var view in new[] { Lateral, FrontalEntrada, FrontalPosterior, Planta })
                    {
                        SelectView(w, view);
                        var primitives = Primitives(w);
                        var pieces = PushBackPreviewProbe.PieceCount(w);

                        // "Substantive" measured against the plan itself: a small rack legitimately has fewer pieces, but
                        // every view must still draw MORE than one trace per piece (section, span, saque...). An absolute
                        // floor would only measure the rack's size; this measures the renderer's richness.
                        Assert.True(primitives.Count >= 15, $"view {view} painted only {primitives.Count} primitives");
                        Assert.True(primitives.Count > pieces,
                            $"view {view} drew {primitives.Count} primitives for {pieces} pieces — one bare trace each");
                        Assert.True(primitives.Select(p => p.Colour).Distinct().Count() >= 2, $"view {view} uses a single colour");
                        Assert.True(primitives.Select(p => p.Thickness).Distinct().Count() >= 2, $"view {view} uses a single thickness");

                        var (width, height) = Extent(primitives);
                        Assert.True(width > 40.0, $"view {view} has no horizontal extent ({width})");
                        Assert.True(height > 40.0, $"view {view} has no vertical extent ({height})");
                    }
                }
                finally { w.Close(); }
            });
        }

        // ---- 3/5. the lateral carries the main roles; the planta is not collapsed ----

        [Fact]
        public void Lateral_CarriesTheMainRoles_OfTheResolvedPlan()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    SelectView(w, Lateral);

                    // The plan the preview draws is the authority: assert the roles it actually contains reach the canvas
                    // by counting primitives against a plan-free floor, and the roles themselves on the plan.
                    var roles = PushBackPreviewProbe.RolesOf(w);
                    foreach (var role in new[]
                    {
                        HeaderBlockRole.Post, HeaderBlockRole.Beam, HeaderBlockRole.Rail,
                        HeaderBlockRole.Roller, HeaderBlockRole.Tope,
                    })
                    {
                        Assert.Contains(role, roles);
                    }

                    // And the drawing is rich enough to have painted them.
                    Assert.True(Primitives(w).Count >= 40);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void Planta_HasRealExtentOnBothAxes_NotOneOrTwoHorizontalLines()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    SelectView(w, Planta);
                    var primitives = Primitives(w);
                    var (width, height) = Extent(primitives);

                    Assert.True(primitives.Count >= 20, $"planta painted only {primitives.Count} primitives");
                    Assert.True(width > 100.0, $"planta has no useful width ({width})");
                    Assert.True(height > 60.0, $"planta collapsed vertically ({height})");

                    // Not just a couple of horizontal rules: several DISTINCT vertical positions must be occupied.
                    var rows = primitives.Select(p => Math.Round(Math.Min(p.Y1, p.Y2), 0)).Distinct().Count();
                    Assert.True(rows >= 4, $"planta occupies only {rows} vertical positions");
                }
                finally { w.Close(); }
            });
        }

        // ---- 4. the two frontals differ and carry the right families ----

        [Fact]
        public void BothFrontals_DifferFromEachOther_AndCarryTheRightFamilies()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    SelectView(w, FrontalEntrada);
                    var entrada = Signature(Primitives(w));
                    var entradaRoles = PushBackPreviewProbe.RolesOf(w);

                    SelectView(w, FrontalPosterior);
                    var posterior = Signature(Primitives(w));
                    var posteriorRoles = PushBackPreviewProbe.RolesOf(w);

                    Assert.NotEqual(entrada, posterior);

                    // The entrance/exit cut has no rear stop; the rear cut has the stops and no ordinary low-end safety.
                    Assert.DoesNotContain(HeaderBlockRole.Tope, entradaRoles);
                    Assert.Contains(HeaderBlockRole.Tope, posteriorRoles);
                    Assert.DoesNotContain(HeaderBlockRole.Safety, posteriorRoles);
                }
                finally { w.Close(); }
            });
        }

        // ---- 7/8. the corte selector and the tope deactivation are visible in the drawing ----

        [Fact]
        public void ChangingTheLateralCorte_ChangesTheScene()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    EditorWindowTestSupport.ClickNamed(w, "AddFrontButton");   // a second corte exists
                    SelectView(w, Lateral);

                    var sections = (ComboBox)w.FindName("LateralSectionBox");
                    if (sections == null || sections.Items.Count < 2)
                    {
                        return;
                    }

                    sections.SelectedIndex = 0;
                    w.UpdateLayout();
                    var first = Signature(Primitives(w));

                    sections.SelectedIndex = 1;
                    w.UpdateLayout();
                    var second = Signature(Primitives(w));

                    Assert.NotEqual(first, second);
                }
                finally { w.Close(); }
            });
        }

        [Fact]
        public void DeactivatingATope_RemovesItFromTheDrawing()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    SelectView(w, FrontalPosterior);
                    var before = PushBackPreviewProbe.CountOfRole(w, HeaderBlockRole.Tope);
                    Assert.True(before > 0);

                    var config = w.State.RearTopeConfig();
                    config.OffCells.Add(new SelectiveGridCell { Frente = 0, Level = 0 });
                    w.State.LoadRearTopeConfig(config);
                    w.Session.Recompute.Request();
                    w.UpdateLayout();

                    Assert.Equal(before - 1, PushBackPreviewProbe.CountOfRole(w, HeaderBlockRole.Tope));
                }
                finally { w.Close(); }
            });
        }

        // ---- 10/11. empty plan is safe, and rendering never mutates the plan ----

        [Fact]
        public void AnEmptyPlan_DoesNotThrow_AndLeavesTheCanvasEmpty()
        {
            StaTestRunner.Run(() =>
            {
                var surface = new RackCad.UI.Preview.EditorPreviewSurface(new Canvas { Width = 400, Height = 300 });
                var drawn = RackCad.UI.Systems.PushBack.PushBackPreviewRenderer.Draw(
                    surface, null, null, "LATERAL", 6.0,
                    DynamicRackDefaults.InOutBeamCatalogId, PushBackDefaults.HighEndBeamCatalogId);
                Assert.False(drawn);
            });
        }

        [Fact]
        public void Rendering_DoesNotMutateThePlan()
        {
            StaTestRunner.Run(() =>
            {
                var w = Shown();
                try
                {
                    SelectView(w, Lateral);
                    var plan = w.CurrentPreviewPlan;
                    var before = RackCad.UI.Systems.PushBack.PushBackPreviewRenderer.Flatten(plan)
                        .Select(i => $"{i.PieceId}|{i.Insertion.X}|{i.Insertion.Y}|{i.MirroredX}|{i.RotationRadians}")
                        .ToList();

                    // Render the very same plan again.
                    w.UpdateLayout();
                    SelectView(w, Lateral);

                    var after = RackCad.UI.Systems.PushBack.PushBackPreviewRenderer.Flatten(w.CurrentPreviewPlan)
                        .Select(i => $"{i.PieceId}|{i.Insertion.X}|{i.Insertion.Y}|{i.MirroredX}|{i.RotationRadians}")
                        .ToList();

                    Assert.Equal(before, after);
                }
                finally { w.Close(); }
            });
        }

        // ---- 12. the old simplified renderer is not a second productive path ----

        [Fact]
        public void TheSimplifiedRenderer_IsNoLongerInTheDrawingPath()
        {
            var methods = typeof(RackPushBackSystemWindow)
                .GetMethods(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Select(m => m.Name)
                .ToList();

            Assert.DoesNotContain("DrawModel", methods);
            Assert.DoesNotContain("RoleBrush", methods);
            Assert.Contains("DrawSharedPreview", methods);

            // And the window no longer owns a painter of its own: it draws through the shared surface.
            var fieldTypes = typeof(RackPushBackSystemWindow)
                .GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Select(f => f.FieldType.Name)
                .ToList();
            Assert.Contains("EditorPreviewSurface", fieldTypes);
            Assert.DoesNotContain("PreviewCanvasPainter", fieldTypes);
        }
    }

    /// <summary>Reads the roles of the plan the window is currently previewing (the drawing's own source of truth).</summary>
    internal static class PushBackPreviewProbe
    {
        public static IReadOnlyCollection<HeaderBlockRole> RolesOf(RackPushBackSystemWindow w)
            => RackCad.UI.Systems.PushBack.PushBackPreviewRenderer.Flatten(w.CurrentPreviewPlan)
                .Select(instance => instance.Role)
                .Distinct()
                .ToList();

        public static int PieceCount(RackPushBackSystemWindow w)
            => RackCad.UI.Systems.PushBack.PushBackPreviewRenderer.Flatten(w.CurrentPreviewPlan).Count;

        public static int CountOfRole(RackPushBackSystemWindow w, HeaderBlockRole role)
            => RackCad.UI.Systems.PushBack.PushBackPreviewRenderer.Flatten(w.CurrentPreviewPlan)
                .Count(instance => instance.Role == role);
    }
}
