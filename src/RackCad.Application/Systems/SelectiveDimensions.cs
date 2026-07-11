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
        private const double NearOffset = 14.0;

        /// <summary>Extra gap (in, ×scale) each further-out dimension chain steps by (overall, then per-level elevations).</summary>
        private const double Step = 11.0;

        /// <summary>
        /// FRONTAL cotas. Minimal = overall height + overall width. Standard adds the per-frente widths (a chain along
        /// the bottom) and the level separations (a chain up the left). Detailed adds each level's elevation from the
        /// floor. Breakdown chains sit near the geometry; the overall dimensions sit one step further out.
        /// </summary>
        public static void AddFrontal(ICollection<HeaderBlockInstance> instances, SelectiveRackSystem system, string view, IReadOnlyList<double> postX)
        {
            var detail = system.Dimensions;
            if (detail == DimensionDetail.None || postX == null || postX.Count < 2 || system.Height <= 0.0)
            {
                return;
            }

            var scale = system.AnnotationScale > 0.0 ? system.AnnotationScale : 1.0;
            var h = SelectiveAnnotations.TextHeightFor(scale);
            var near = NearOffset * scale;
            var far = (NearOffset + Step) * scale;
            var farther = (NearOffset + 2.0 * Step) * scale;

            var leftX = postX[0];
            var rightX = postX[postX.Count - 1];
            var height = system.Height;
            var levelYs = FrontalLevelYs(system);

            if (detail == DimensionDetail.Minimal)
            {
                AddVertical(instances, view, leftX, 0.0, height, -near, h);   // alto total (izquierda)
                AddHorizontal(instances, view, leftX, rightX, 0.0, -near, h); // ancho total (abajo)
                return;
            }

            // Ancho por frente (cadena, abajo) + ancho total un paso más afuera.
            for (var i = 0; i + 1 < postX.Count; i++)
            {
                AddHorizontal(instances, view, postX[i], postX[i + 1], 0.0, -near, h);
            }

            AddHorizontal(instances, view, leftX, rightX, 0.0, -far, h);

            // Separaciones entre niveles (cadena, izquierda): piso → nivel 1 → nivel 2 …
            var previous = 0.0;
            foreach (var y in levelYs)
            {
                AddVertical(instances, view, leftX, previous, y, -near, h);
                previous = y;
            }

            AddVertical(instances, view, leftX, 0.0, height, -far, h); // alto total, más afuera

            if (detail == DimensionDetail.Detailed)
            {
                // Elevación de CADA nivel desde el piso. Todas parten del piso, así que cada una va en su PROPIA
                // columna (escalonadas hacia afuera) para no encimarse en una sola línea.
                var step = Step * scale;
                for (var i = 0; i < levelYs.Count; i++)
                {
                    AddVertical(instances, view, leftX, 0.0, levelYs[i], -(farther + i * step), h);
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

        private static void AddHorizontal(ICollection<HeaderBlockInstance> instances, string view, double x1, double x2, double y, double offset, double height)
        {
            if (Math.Abs(x2 - x1) < 1e-6)
            {
                return; // skip a zero-length dimension
            }

            instances.Add(Dim(view, new Point2D(x1, y), new Point2D(x2, y), offset, height));
        }

        private static void AddVertical(ICollection<HeaderBlockInstance> instances, string view, double x, double y1, double y2, double offset, double height)
        {
            if (Math.Abs(y2 - y1) < 1e-6)
            {
                return;
            }

            instances.Add(Dim(view, new Point2D(x, y1), new Point2D(x, y2), offset, height));
        }

        private static HeaderBlockInstance Dim(string view, Point2D p1, Point2D p2, double offset, double height)
            => new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Dimension,
                View = view,
                Insertion = p1,
                ConnectionAnchor = p2,
                DimensionOffset = offset,
                TextHeight = height
            };
    }
}
