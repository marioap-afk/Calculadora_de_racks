using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// The physical Push Back bed geometry for one level: the line between the TWO REAL contact points of the two end
    /// beams, both of them bolted to a valid troquel.
    ///
    /// PB-004 (I-32, Owner rule after round 1): the 7/16"-per-foot rise is a NOMINAL TARGET, not the literal final
    /// rise. The REAR beam is the anchor — it keeps the troquel elevation the resolver already gave it — and the
    /// ENTRANCE/EXIT beam is derived from it through the nominal slope and then snapped to its own nearest troquel.
    /// The resulting slope is whatever those two real contacts produce, within half a troquel step of the target.
    ///
    /// The previous version did the opposite (anchored the LOW end and pulled the rear beam OFF the grid so its edge
    /// touched a theoretical line); the Owner rejected it because a larguero must always be bolted to a troquel.
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
    /// Single source of truth for the Push Back bed line. The high end is the REAR beam's real contact (its resolved
    /// troquel elevation plus its <c>INICIO_DERECHO</c> datum); the low end is the ENTRANCE/EXIT beam's real contact,
    /// whose elevation <see cref="PushBackLoadBeamGeometry.LowBeamElevations"/> derives from the high one and snaps to
    /// the troquel grid. Both ends are therefore physical. Does not touch the dynamic bed geometry.
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

            var gridBase = PushBackTroquelGrid.Base(structure, catalog);
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

                // PB-004: the REAR contact is the anchor, taken from its own troquel elevation.
                var highMate = BeamMate(high, highMateLocal);
                var lowX = BeamMate(low, lowMateLocal).X;
                if (highMate.X - lowX <= 0.0)
                {
                    continue;
                }

                // ...and the LOW contact belongs to the beam derived from it and snapped to its own troquel.
                var lowElevation = PushBackLoadBeamGeometry.LowBeamElevation(
                    highMate, lowX, lowMateLocal.Y, gridBase);
                var exitMate = new Point2D(lowX, lowElevation + lowMateLocal.Y);

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
