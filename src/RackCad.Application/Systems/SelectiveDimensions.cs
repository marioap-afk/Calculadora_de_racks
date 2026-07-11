using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Builds the linear-dimension instances for a selective rack view, gated by <see cref="SelectiveRackSystem.Dimensions"/>
    /// (None/Minimal/Standard/Detailed). Pure — emits <see cref="HeaderBlockRole.Dimension"/> instances (two measured
    /// points + a signed dimension-line offset) that the AutoCAD drawer materializes as RotatedDimensions on the
    /// dimensions layer. Sizes/offsets scale with the rack's annotation scale, so cotas and numbers stay proportional.
    /// </summary>
    internal static class SelectiveDimensions
    {
        /// <summary>Gap (in, ×scale) from the geometry to the nearest (breakdown) dimension line.</summary>
        private const double ChainGap = 14.0;

        /// <summary>Gap (in, ×scale) between the breakdown chain and the overall dimension line — wide enough that the
        /// overall value (e.g. the total height) doesn't crowd the horizontal breakdown text (the level separations).</summary>
        private const double OverallGap = 22.0;

        /// <summary>Step (in, ×scale) between the stacked per-level elevation lines (Detailed).</summary>
        private const double ElevationStep = 14.0;

        /// <summary>Gap (in, ×scale) above a bay's top larguero where its cut-length cota sits (attached to the larguero).</summary>
        private const double LargueroGap = 8.0;

        /// <summary>
        /// FRONTAL cotas. Minimal = overall height + overall width. Standard adds the per-frente LARGUERO cut lengths
        /// (positioned under each larguero, NOT post-to-post) and the level separations (a chain up the left). Detailed
        /// adds each level's elevation from the floor. Breakdown chains sit near the geometry; the overall dimensions
        /// sit further out so their text clears the breakdown. <paramref name="troquelXs"/> are the per-bay larguero
        /// hook offsets (from the left post), so a cota lands on the actual larguero span.
        /// </summary>
        public static void AddFrontal(
            ICollection<HeaderBlockInstance> instances, SelectiveRackSystem system, string view,
            IReadOnlyList<double> postX, IReadOnlyList<double> troquelXs)
        {
            var detail = system.Dimensions;
            if (detail == DimensionDetail.None || postX == null || postX.Count < 2 || system.Height <= 0.0)
            {
                return;
            }

            var scale = system.AnnotationScale > 0.0 ? system.AnnotationScale : 1.0;
            var h = SelectiveAnnotations.TextHeightFor(scale);
            var near = ChainGap * scale;
            var far = (ChainGap + OverallGap) * scale;
            var style = system.DimensionStyle;

            var leftX = postX[0];
            var rightX = postX[postX.Count - 1];
            var height = system.Height;
            var levelYs = FrontalLevelYs(system);

            if (detail == DimensionDetail.Minimal)
            {
                AddVertical(instances, view, leftX, 0.0, height, -near, h, style);   // alto total (izquierda)
                AddHorizontal(instances, view, leftX, rightX, 0.0, -near, h, style); // ancho total post-a-post (abajo)
                return;
            }

            // Largo de corte del larguero por frente, PEGADO al larguero (a la altura del larguero superior de la
            // bahía, un poco por encima), no en el piso — así se entiende a qué larguero pertenece y el hueco de la
            // ménsula no confunde. El ancho total post-a-post va abajo, más afuera.
            var largueroGap = LargueroGap * scale;
            for (var i = 0; i < system.Bays.Count && i < postX.Count && troquelXs != null && i < troquelXs.Count; i++)
            {
                var bay = system.Bays[i];
                if (bay.BeamLength <= 0.0 || bay.Levels.Count == 0)
                {
                    continue;
                }

                var beamLeft = postX[i] + troquelXs[i];
                var beamY = bay.Levels[bay.Levels.Count - 1].Y; // larguero superior de esta bahía
                AddHorizontal(instances, view, beamLeft, beamLeft + bay.BeamLength, beamY, largueroGap, h, style);
            }

            AddHorizontal(instances, view, leftX, rightX, 0.0, -far, h, style);

            // Separaciones entre niveles (cadena, izquierda): piso → nivel 1 → nivel 2 …
            var previous = 0.0;
            foreach (var y in levelYs)
            {
                AddVertical(instances, view, leftX, previous, y, -near, h, style);
                previous = y;
            }

            AddVertical(instances, view, leftX, 0.0, height, -far, h, style); // alto total, más afuera para no pegarse a los claros

            if (detail == DimensionDetail.Detailed)
            {
                // Elevación de CADA nivel desde el piso. Todas parten del piso, así que cada una va en su PROPIA
                // columna (escalonada hacia afuera) para no encimarse en una sola línea.
                var elevationStep = ElevationStep * scale;
                for (var i = 0; i < levelYs.Count; i++)
                {
                    AddVertical(instances, view, leftX, 0.0, levelYs[i], -(far + (i + 1) * elevationStep), h, style);
                }
            }
        }

        /// <summary>Distinct larguero elevations (Y &gt; 0) across every bay, ascending — the rows a frontal dimensions.</summary>
        private static List<double> FrontalLevelYs(SelectiveRackSystem system)
        {
            var ys = new SortedSet<double>();
            foreach (var bay in system.Bays)
            {
                foreach (var level in bay.Levels)
                {
                    if (level.Y > 1e-6)
                    {
                        ys.Add(Math.Round(level.Y, 4));
                    }
                }
            }

            return ys.ToList();
        }

        private static void AddHorizontal(ICollection<HeaderBlockInstance> instances, string view, double x1, double x2, double y, double offset, double height, string style)
        {
            if (Math.Abs(x2 - x1) < 1e-6)
            {
                return; // skip a zero-length dimension
            }

            instances.Add(Dim(view, new Point2D(x1, y), new Point2D(x2, y), offset, height, style));
        }

        private static void AddVertical(ICollection<HeaderBlockInstance> instances, string view, double x, double y1, double y2, double offset, double height, string style)
        {
            if (Math.Abs(y2 - y1) < 1e-6)
            {
                return;
            }

            instances.Add(Dim(view, new Point2D(x, y1), new Point2D(x, y2), offset, height, style));
        }

        private static HeaderBlockInstance Dim(string view, Point2D p1, Point2D p2, double offset, double height, string style)
            => new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Dimension,
                View = view,
                Insertion = p1,
                ConnectionAnchor = p2,
                DimensionOffset = offset,
                TextHeight = height,
                DimensionStyleName = style
            };
    }
}
