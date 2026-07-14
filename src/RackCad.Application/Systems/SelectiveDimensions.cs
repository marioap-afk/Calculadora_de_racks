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

        /// <summary>How far below Y=0 the FRONTAL cotas reach (a positive magnitude), so an annotation (the frente number)
        /// can be placed clear of them; 0 when dimensions are off. Minimal stops at the near chain; Standard/Detailed go
        /// out to the overall (far) line.</summary>
        public static double FrontalBottomReach(SelectiveRackSystem system)
        {
            if (system == null || system.Dimensions == DimensionDetail.None)
            {
                return 0.0;
            }

            var scale = system.AnnotationScale > 0.0 ? system.AnnotationScale : 1.0;
            return system.Dimensions == DimensionDetail.Minimal ? ChainGap * scale : (ChainGap + OverallGap) * scale;
        }

        /// <summary>
        /// FRONTAL cotas. Minimal = overall height + overall width. Standard adds the per-frente LARGUERO cut lengths
        /// (along the bottom, spanning the profile cut — NOT post-to-post) and the level separations (a chain up the
        /// left). Detailed adds each level's elevation from the floor. Breakdown chains sit near the geometry; the
        /// overall dimensions sit further out so their text clears the breakdown. <paramref name="beamStartXs"/> is the
        /// per-bay absolute X where the larguero PROFILE cut starts (hook troquel + the ménsula overhang INICIO_PERFIL),
        /// so the cota spans the true cut, not the hook-to-hook distance.
        /// </summary>
        public static void AddFrontal(
            ICollection<HeaderBlockInstance> instances, SelectiveRackSystem system, string view,
            IReadOnlyList<double> postX, IReadOnlyList<double> beamStartXs)
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
                AddVertical(instances, view, leftX, 0.0, height, -near, h, style); // alto total (izquierda)
                // Largo de CORTE del larguero representativo (frente 0), no poste a poste — igual que la planta.
                if (beamStartXs != null && beamStartXs.Count > 0 && system.Bays.Count > 0 && system.Bays[0].BeamLength > 0.0)
                {
                    AddHorizontal(instances, view, beamStartXs[0], beamStartXs[0] + system.Bays[0].BeamLength, 0.0, -near, h, style);
                }

                return;
            }

            // Largo de CORTE del larguero por frente, abajo (en el piso): la cota abarca el perfil real (desde el
            // inicio del corte, ya con la ménsula descontada), no de troquel a troquel. El ancho total post-a-post va
            // más afuera.
            for (var i = 0; i < system.Bays.Count && beamStartXs != null && i < beamStartXs.Count; i++)
            {
                var beamLength = system.Bays[i].BeamLength;
                if (beamLength <= 0.0)
                {
                    continue;
                }

                var start = beamStartXs[i];
                AddHorizontal(instances, view, start, start + beamLength, 0.0, -near, h, style);
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

        /// <summary>
        /// LATERAL-corte cotas (one corte = one cross-section along the DEPTH axis at a post). X = fondo depth
        /// (anchor-relative: the primary fondo's front is 0), Y = height. Minimal = overall height + overall depth.
        /// Standard adds each fondo's depth (a chain along the bottom) and the level separations (a chain up the left).
        /// Detailed adds each level's elevation from the floor. <paramref name="fondoFrontXs"/>/<paramref name="fondoDepths"/>
        /// are the reaching fondos' anchor-relative front X and depth; <paramref name="levelYs"/> the corte's larguero
        /// rows; <paramref name="height"/> the corte's total height.
        /// </summary>
        public static void AddLateralCorte(
            ICollection<HeaderBlockInstance> instances, SelectiveRackSystem system, string view,
            IReadOnlyList<double> fondoFrontXs, IReadOnlyList<double> fondoDepths, IReadOnlyList<double> levelYs, double height)
        {
            var detail = system.Dimensions;
            if (detail == DimensionDetail.None || fondoFrontXs == null || fondoFrontXs.Count == 0 || fondoDepths == null || height <= 0.0)
            {
                return;
            }

            var scale = system.AnnotationScale > 0.0 ? system.AnnotationScale : 1.0;
            var h = SelectiveAnnotations.TextHeightFor(scale);
            var near = ChainGap * scale;
            var far = (ChainGap + OverallGap) * scale;
            var style = system.DimensionStyle;

            var frontX = fondoFrontXs[0];
            var last = fondoFrontXs.Count - 1;
            var backX = fondoFrontXs[last] + (last < fondoDepths.Count ? fondoDepths[last] : 0.0);

            if (detail == DimensionDetail.Minimal)
            {
                AddVertical(instances, view, frontX, 0.0, height, -near, h, style);     // alto total (izquierda)
                AddHorizontal(instances, view, frontX, backX, 0.0, -near, h, style);    // fondo total (abajo)
                return;
            }

            // Fondo por cabecera (cadena, abajo) + fondo total más afuera.
            for (var k = 0; k < fondoFrontXs.Count && k < fondoDepths.Count; k++)
            {
                if (fondoDepths[k] > 0.0)
                {
                    AddHorizontal(instances, view, fondoFrontXs[k], fondoFrontXs[k] + fondoDepths[k], 0.0, -near, h, style);
                }
            }

            AddHorizontal(instances, view, frontX, backX, 0.0, -far, h, style);

            // Separaciones entre niveles (cadena, izquierda) + alto total más afuera.
            var previous = 0.0;
            foreach (var y in levelYs ?? new List<double>())
            {
                AddVertical(instances, view, frontX, previous, y, -near, h, style);
                previous = y;
            }

            AddVertical(instances, view, frontX, 0.0, height, -far, h, style);

            if (detail == DimensionDetail.Detailed && levelYs != null)
            {
                var elevationStep = ElevationStep * scale;
                for (var i = 0; i < levelYs.Count; i++)
                {
                    AddVertical(instances, view, frontX, 0.0, levelYs[i], -(far + (i + 1) * elevationStep), h, style);
                }
            }
        }

        /// <summary>
        /// PLANTA (top view) cotas. X = fondo depth, Y = frente. The larguero runs along Y, so the Y cotas show the
        /// larguero CUT length (centered in its frente pitch — the fabrication dimension, NOT the post-to-post pitch or
        /// the whole-system run). Minimal = one representative cut (Y) + overall depth (X). Standard adds a cut per
        /// frente and each fondo's depth (a chain along the bottom). <paramref name="beamLengths"/> is the governing
        /// larguero cut length per frente.
        /// </summary>
        public static void AddPlanta(
            ICollection<HeaderBlockInstance> instances, SelectiveRackSystem system, string view,
            IReadOnlyList<double> frenteYs, IReadOnlyList<double> offsets, IReadOnlyList<double> fondoDepths,
            IReadOnlyList<double> beamLengths)
        {
            var detail = system.Dimensions;
            if (detail == DimensionDetail.None || frenteYs == null || frenteYs.Count < 2 || offsets == null || offsets.Count == 0 || fondoDepths == null)
            {
                return;
            }

            var scale = system.AnnotationScale > 0.0 ? system.AnnotationScale : 1.0;
            var h = SelectiveAnnotations.TextHeightFor(scale);
            var near = ChainGap * scale;
            var far = (ChainGap + OverallGap) * scale;
            var style = system.DimensionStyle;

            var firstY = frenteYs[0];
            var frontX = offsets[0];
            var lastFondo = offsets.Count - 1;
            var backX = offsets[lastFondo] + (lastFondo < fondoDepths.Count ? fondoDepths[lastFondo] : 0.0);

            // A frente's larguero CUT cota (Y), centered in its post pitch: the pitch is cut + 2×(troquel+ménsula),
            // symmetric, so the larguero sits centered — no need to resolve the planta connection points.
            void AddCut(int i)
            {
                if (i < 0 || i + 1 >= frenteYs.Count || beamLengths == null || i >= beamLengths.Count || beamLengths[i] <= 0.0)
                {
                    return;
                }

                var pitch = frenteYs[i + 1] - frenteYs[i];
                var start = frenteYs[i] + (pitch - beamLengths[i]) / 2.0;
                AddVertical(instances, view, frontX, start, start + beamLengths[i], -near, h, style);
            }

            if (detail == DimensionDetail.Minimal)
            {
                AddCut(0); // larguero de corte representativo
                AddHorizontal(instances, view, frontX, backX, firstY, -near, h, style); // fondo total
                return;
            }

            // Largo de corte del larguero por frente (Y).
            for (var i = 0; i + 1 < frenteYs.Count; i++)
            {
                AddCut(i);
            }

            // Fondo por fondo (cadena, abajo) + fondo total más afuera.
            for (var k = 0; k < offsets.Count && k < fondoDepths.Count; k++)
            {
                if (fondoDepths[k] > 0.0)
                {
                    AddHorizontal(instances, view, offsets[k], offsets[k] + fondoDepths[k], firstY, -near, h, style);
                }
            }

            AddHorizontal(instances, view, frontX, backX, firstY, -far, h, style);
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
