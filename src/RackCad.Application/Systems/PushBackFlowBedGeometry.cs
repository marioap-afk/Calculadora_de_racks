using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// The physical Push Back bed geometry for one level. The axis STARTS at the LOW IN/OUT beam's <c>TROQUEL_CAMA</c>
    /// — the exit larguero's troquel-snapped bed face — and RISES from there by the canonical slope
    /// (<see cref="PushBackBedSlope"/>, 7/16" per commercial foot) up to the rear beam's <c>INICIO_DERECHO</c> column.
    /// It is the SINGLE source for the bed's rotation/elevation and for the tangency of the rear and intermediate
    /// beams — exactly one physical line.
    ///
    /// PB-004 (I-32): the high end used to be READ from the rear beam's own independently snapped elevation plus that
    /// beam's own datum, which made the bed's slope an emergent of two mechanisms instead of a rule.
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

        /// <summary>
        /// High-end point of the bed line. Its X is the rear TROQUEL_REDONDO beam's <c>INICIO_DERECHO</c> column; its Y
        /// is DERIVED from <see cref="ExitMate"/> by the canonical slope (<see cref="PushBackBedSlope"/>), not read
        /// from that beam's own snapped elevation (PB-004, I-32). The rear beam follows this line — the line no longer
        /// follows the beam.
        /// </summary>
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
    /// Single source of truth for the Push Back bed line. It reuses <see cref="DynamicLoadBeamGeometry.Placements"/> for
    /// the LOW end's already-snapped elevation, mates the LOW IN/OUT beam's <c>TROQUEL_CAMA</c>, and DERIVES the high
    /// end from it with <see cref="PushBackBedSlope"/>. The rear beam supplies only the high end's X (its
    /// <c>INICIO_DERECHO</c> column); its own elevation never feeds the slope. Does not touch the dynamic bed geometry.
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

            // The rear beam supplies the high end's COLUMN (X) only. Read it fail-closed: a missing row is a missing
            // physical contract, and falling back to the block origin would produce a plausible but false axis.
            var highColumn = catalog?.ConnectionLayout.FindConnectionLayout(
                highBeamId, PushBackDefaults.HighEndBeamRightBedMatePoint, PushBackDefaults.HighEndBeamView);
            if (highColumn == null)
            {
                return result;
            }

            var highMateLocal = new Point2D(highColumn.LocalX, highColumn.LocalY);

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
                var highX = BeamMate(high, highMateLocal).X;
                var run = highX - exitMate.X;
                if (run <= 0.0)
                {
                    continue;
                }

                // PB-004: the high end is DERIVED — the exit larguero's snapped bed face plus the canonical rise over
                // the run. The rear beam's own snapped elevation and its INICIO datum no longer enter the slope.
                var highMate = new Point2D(highX, exitMate.Y + PushBackBedSlope.Rise(run));

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
