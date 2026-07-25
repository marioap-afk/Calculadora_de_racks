using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// The physical Push Back bed geometry for one level. Unlike the dynamic bed (whose two mates are the IN/OUT beam's
    /// <c>TROQUEL_CAMA</c> at both ends), the Push Back axis runs from the LOW IN/OUT beam's <c>TROQUEL_CAMA</c> to the
    /// HIGH beam's (<c>LARGUERO_ESCALON_TROQUEL_REDONDO</c>) <c>INICIO_IZQUIERDO/DERECHO</c>. It is the SINGLE source for
    /// the bed's rotation/elevation and for the tangency of the intermediate beams — exactly one physical line.
    /// </summary>
    public readonly struct PushBackFlowBedAxis
    {
        public PushBackFlowBedAxis(int levelNumber, Point2D exitMate, Point2D highMate, Point2D railLocalMate)
        {
            LevelNumber = levelNumber;
            ExitMate = exitMate;
            HighMate = highMate;
            RailLocalMate = railLocalMate;
        }

        public int LevelNumber { get; }

        /// <summary>Low-end mate: the IN/OUT beam's TROQUEL_CAMA (same physical point the dynamic bed uses).</summary>
        public Point2D ExitMate { get; }

        /// <summary>High-end mate: the rear TROQUEL_REDONDO beam's INICIO_IZQUIERDO/DERECHO.</summary>
        public Point2D HighMate { get; }

        /// <summary>The rail's TROQUEL_IN local point (where the rail bolts onto the low IN/OUT beam).</summary>
        public Point2D RailLocalMate { get; }

        public double Rise => HighMate.Y - ExitMate.Y;
        public double Run => HighMate.X - ExitMate.X;
        public double Length => Math.Sqrt(Run * Run + Rise * Rise);
        public double AngleRadians => Math.Atan2(Rise, Run);

        /// <summary>World origin of the rail block after TROQUEL_IN is bolted to the low IN/OUT beam.</summary>
        public Point2D RailOrigin
        {
            get
            {
                var cos = Math.Cos(AngleRadians);
                var sin = Math.Sin(AngleRadians);
                return new Point2D(
                    ExitMate.X - RailLocalMate.X * cos + RailLocalMate.Y * sin,
                    ExitMate.Y - RailLocalMate.X * sin - RailLocalMate.Y * cos);
            }
        }

        /// <summary>Height of the rail ORIGIN line at a world X — the line every intermediate support is tangent to.</summary>
        public double RailOriginYAt(double worldX)
            => Math.Abs(Run) < 1e-9
                ? RailOrigin.Y
                : RailOrigin.Y + (worldX - RailOrigin.X) * Rise / Run;
    }

    /// <summary>
    /// Single source of truth for the Push Back bed line. Reuses <see cref="DynamicLoadBeamGeometry.Placements"/> for the
    /// already-snapped end elevations, then mates the LOW IN/OUT beam's <c>TROQUEL_CAMA</c> and the HIGH rear beam's
    /// <c>INICIO_DERECHO</c> (chosen by the same mirror convention the intermediate beams use). Does not touch the dynamic
    /// bed geometry; the divergent high mate lives here.
    /// </summary>
    public static class PushBackFlowBedGeometry
    {
        /// <summary>Push Back commercial bed length = the front's COMPLETE span, no 4" clearance (see <see cref="PushBackFlowBedLateralBuilder.ResolveBedLength"/>).</summary>
        public static double ResolveBedLength(PushBackSystem system, DynamicRackFront front = null)
            => PushBackFlowBedLateralBuilder.ResolveBedLength(system, front);

        public static IReadOnlyList<PushBackFlowBedAxis> Resolve(PushBackSystem system, RackCatalog catalog, DynamicRackFront front = null)
        {
            var result = new List<PushBackFlowBedAxis>();
            var structure = system?.Structure;
            if (structure == null || structure.TotalLength <= 0.0)
            {
                return result;
            }

            var railLocalMate = CatalogLookup.Local(catalog, FlowBedDefaults.RailId, FlowBedDefaults.RailInOutMatePoint, FlowBedDefaults.View);
            var highBeamId = string.IsNullOrWhiteSpace(system.HighEndBeamCatalogId)
                ? PushBackDefaults.HighEndBeamCatalogId
                : system.HighEndBeamCatalogId;
            var highMateLocal = CatalogLookup.Local(catalog, highBeamId, PushBackDefaults.HighEndBeamRightBedMatePoint, PushBackDefaults.HighEndBeamView);

            var placements = DynamicLoadBeamGeometry.Placements(structure, front);
            foreach (var level in placements.Select(p => p.LevelNumber).Distinct())
            {
                var low = placements.FirstOrDefault(p => p.LevelNumber == level && !p.IsEntrance);
                var high = placements.FirstOrDefault(p => p.LevelNumber == level && p.IsEntrance);
                if (low == null || high == null)
                {
                    continue;
                }

                var lowBeamId = string.IsNullOrWhiteSpace(low.BeamCatalogId) ? DynamicRackDefaults.InOutBeamCatalogId : low.BeamCatalogId;
                var lowMateLocal = CatalogLookup.Local(catalog, lowBeamId, DynamicRackDefaults.InOutBeamBedMatePoint, DynamicRackDefaults.InOutBeamView);

                var exitMate = BeamMate(low, lowMateLocal);
                var highMate = BeamMate(high, highMateLocal);
                if (highMate.X - exitMate.X <= 0.0)
                {
                    continue;
                }

                result.Add(new PushBackFlowBedAxis(level, exitMate, highMate, railLocalMate));
            }

            return result;
        }

        private static Point2D BeamMate(DynamicLoadBeamPlacement placement, Point2D localMate)
        {
            var localX = placement.MirroredX ? -localMate.X : localMate.X;
            return new Point2D(placement.X + localX, placement.Y + localMate.Y);
        }
    }
}
