using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.UI.Controls;

namespace RackCad.UI.StructuralSections
{
    /// <summary>
    /// Draws a <see cref="StructuralSectionRepresentationPlan"/> on a canvas, fitted to the space available.
    ///
    /// It is a RENDERER, not a generator: it receives points and roles and turns them into polylines. It never
    /// looks at a section's dimensions, which is the boundary ADR-0022 §7 draws and the reason the preview and
    /// AutoCAD cannot drift apart.
    ///
    /// It reuses <see cref="PreviewCanvas"/> and <see cref="PreviewProjection"/> rather than growing a second
    /// world-to-canvas mapping — the repository already had that divergence once and I-30 had to unify it.
    /// </summary>
    public sealed class StructuralSectionPreview : PreviewCanvas
    {
        public StructuralSectionPreview()
        {
            ContentMargins = new Thickness(16);
            ClipToBounds = true;
        }

        /// <summary>The plan currently drawn. Null clears the surface.</summary>
        public StructuralSectionRepresentationPlan Plan { get; private set; }

        /// <summary>Replaces what is drawn and refits it to the current size.</summary>
        public void Show(StructuralSectionRepresentationPlan plan)
        {
            Plan = plan;
            Redraw();
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo info)
        {
            base.OnRenderSizeChanged(info);
            Redraw();
        }

        /// <summary>
        /// Repaints from scratch. The plan is small — a few thousand points at most — so rebuilding is
        /// simpler and fast enough, and it removes a whole class of "stale shape left behind" defects.
        /// </summary>
        public void Redraw()
        {
            Clear();

            if (Plan == null || ActualWidth <= 0.0 || ActualHeight <= 0.0)
            {
                return;
            }

            var bounds = Plan.Bounds;
            var projection = FitToWorld(
                new Rect(bounds.MinX, bounds.MinY, Math.Max(bounds.Width, 1e-6), Math.Max(bounds.Height, 1e-6)),
                new Size(ActualWidth, ActualHeight));

            if (!projection.IsDrawable)
            {
                return;
            }

            foreach (var curve in Plan.Curves)
            {
                var figure = new Polyline
                {
                    Stroke = StrokeFor(curve.Role),
                    StrokeThickness = ThicknessFor(curve.Role),
                    StrokeLineJoin = PenLineJoin.Round,
                    Points = new PointCollection(
                        curve.Points.Select(p => projection.Project(p.X, p.Y)))
                };

                if (curve.IsClosed && curve.Points.Count > 2)
                {
                    figure.Points.Add(projection.Project(curve.Points[0].X, curve.Points[0].Y));
                }

                var dash = DashFor(curve.Role);

                if (dash != null)
                {
                    figure.StrokeDashArray = dash;
                }

                Children.Add(figure);
            }
        }

        /// <summary>
        /// Colour by ROLE, reusing the shared palette so the inspector looks like every other preview.
        ///
        /// Outside and inside get different colours on purpose: seen from the side, the bore of a tube is two
        /// extra lines a hand's breadth inside the outer ones, and a viewer has to be able to tell at a glance
        /// that they are the wall and not a stray edge.
        /// </summary>
        private static Brush StrokeFor(SectionCurveRole role)
        {
            switch (role)
            {
                case SectionCurveRole.OuterContour:
                case SectionCurveRole.EndProfile:
                    return PreviewPalette.Structure;
                case SectionCurveRole.Hole:
                case SectionCurveRole.EndProfileHole:
                    return PreviewPalette.Beam;
                case SectionCurveRole.Generatrix:
                    return PreviewPalette.Muted;
                case SectionCurveRole.InteriorGeneratrix:
                    return PreviewPalette.Beam;
                case SectionCurveRole.Axis:
                    return PreviewPalette.Accent;
                case SectionCurveRole.Envelope:
                    return PreviewPalette.Guide;
                default:
                    return PreviewPalette.Label;
            }
        }

        private static double ThicknessFor(SectionCurveRole role) =>
            role == SectionCurveRole.OuterContour || role == SectionCurveRole.EndProfile ? 1.8 : 1.0;

        /// <summary>Axis and envelope are annotations, so they are dashed like every other guide in the app.</summary>
        private static DoubleCollection DashFor(SectionCurveRole role) =>
            role == SectionCurveRole.Axis || role == SectionCurveRole.Envelope
                ? new DoubleCollection(new[] { 6.0, 4.0 })
                : null;
    }
}
