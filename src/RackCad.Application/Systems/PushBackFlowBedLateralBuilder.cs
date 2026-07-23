using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Composes the existing roller-bed assembly into a Push Back lane. It REUSES <see cref="FlowBedLateralBuilder"/>
    /// verbatim (the rail/roller recipe is never duplicated) with two Push-Back-specific choices:
    /// <list type="bullet">
    /// <item><see cref="FlowBedType.Pushback"/> — rollers + the bed's own tope, NO brakes (frenos).</item>
    /// <item>the commercial bed length is the front's COMPLETE structural span (no 4" clearance) — unlike the dynamic
    /// bed, which subtracts <see cref="DynamicRackDefaults.FlowBedLengthClearance"/>.</item>
    /// </list>
    /// The dynamic system is NOT touched; the divergent rule (full-span length) lives here, in one place.
    /// </summary>
    public sealed class PushBackFlowBedLateralBuilder
    {
        private readonly FlowBedLateralBuilder flowBedBuilder = new FlowBedLateralBuilder();

        /// <summary>
        /// Push Back commercial bed length = the front's COMPLETE longitudinal span (its resolved cabeceras + separators),
        /// i.e. <c>front.EndX - front.StartX</c> (or the whole system when no front is given). NO 4" clearance is
        /// subtracted — that discount is a dynamic-only rule.
        /// </summary>
        public static double ResolveBedLength(PushBackSystem system, DynamicRackFront front = null)
        {
            if (system == null)
            {
                return 0.0;
            }

            var span = front != null && front.EndX > front.StartX
                ? front.EndX - front.StartX
                : system.TotalLength;

            return span > 0.0 ? span : 0.0;
        }

        /// <summary>
        /// The local roller-bed assembly for one Push Back lane, at the tope's origin (before the lateral builder rotates
        /// it onto the bed axis). Rail LONGITUD = the full structural span; <see cref="FlowBedType.Pushback"/> omits brakes.
        /// </summary>
        public IReadOnlyList<HeaderBlockInstance> BuildLocalAssembly(
            PushBackSystem system,
            RackCatalog catalog,
            DynamicRackFront front = null)
        {
            var laneDepth = ResolveBedLength(system, front);
            if (laneDepth <= 0.0)
            {
                return new List<HeaderBlockInstance>();
            }

            return flowBedBuilder.Build(
                new FlowBedConfiguration
                {
                    BedType = FlowBedType.Pushback,
                    LaneDepth = laneDepth,
                    PalletDepth = system.Structure?.Pallet?.Depth ?? 0.0,
                    RollerId = FlowBedDefaults.RollerId
                },
                catalog);
        }

        /// <summary>
        /// The lateral Push Back bed as a grouped <see cref="HeaderGroup"/>: one shared nested definition (rigid-rotated
        /// onto the Push Back bed axis at the low mate/angle) referenced once per level. Same ARRAY/shared-definition
        /// pattern as the dynamic bed, but the axis is the Push Back one (high mate = rear TROQUEL_REDONDO).
        /// </summary>
        public HeaderGroup BuildLateral(PushBackSystem system, RackCatalog catalog, DynamicRackFront front = null, int levelCount = int.MaxValue)
        {
            var axes = PushBackFlowBedGeometry.Resolve(system, catalog, front)
                .Where(axis => axis.LevelNumber <= levelCount)
                .ToList();
            if (axes.Count == 0)
            {
                return null;
            }

            var localAssembly = BuildLocalAssembly(system, catalog, front);
            if (localAssembly.Count == 0)
            {
                return null;
            }

            var firstAxis = axes[0];
            var definitionInstances = localAssembly
                .Select(instance => RigidClone(instance, firstAxis.RailLocalMate, firstAxis.ExitMate, firstAxis.AngleRadians, -firstAxis.ExitMate.Y))
                .ToList();
            var levelPlacements = axes
                .Select(axis => new HeaderPlacement(0.0, mirrored: false, insertionY: axis.ExitMate.Y))
                .ToList();

            var suffix = front == null ? string.Empty : " F" + (front.Index + 1);
            return new HeaderGroup("Cama push back" + suffix, definitionInstances, levelPlacements);
        }

        private static HeaderBlockInstance RigidClone(
            HeaderBlockInstance source,
            Point2D pivot,
            Point2D target,
            double angle,
            double localYOffset)
        {
            Point2D Transform(Point2D point)
            {
                var x = point.X - pivot.X;
                var y = point.Y - pivot.Y;
                var cos = Math.Cos(angle);
                var sin = Math.Sin(angle);
                return new Point2D(
                    target.X + x * cos - y * sin,
                    target.Y + x * sin + y * cos + localYOffset);
            }

            var clone = new HeaderBlockInstance
            {
                Role = source.Role,
                PieceId = source.PieceId,
                BlockName = source.BlockName,
                View = source.View,
                Insertion = Transform(source.Insertion),
                ConnectionAnchor = Transform(source.ConnectionAnchor),
                RotationRadians = source.RotationRadians + angle,
                MirroredX = source.MirroredX,
                MirroredY = source.MirroredY
            };
            foreach (var pair in source.DynamicParameters)
            {
                clone.DynamicParameters[pair.Key] = pair.Value;
            }

            return clone;
        }
    }
}
