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
    /// Composes the existing complete flow-bed assembly into a dynamic system. The exit is the low left side and
    /// the entrance is the high right side. The rail's TROQUEL_IN is mated to the exit beam's TROQUEL_CAMA; the
    /// already-resolved opposite beam mate and lane rise determine the rigid rotation. All levels reuse one nested
    /// definition, so the roller/brake recipe remains in <see cref="FlowBedLateralBuilder"/> and AutoCAD evaluates
    /// its dynamic rail length only once.
    /// </summary>
    public sealed class DynamicFlowBedLateralBuilder
    {
        private readonly FlowBedLateralBuilder flowBedBuilder = new FlowBedLateralBuilder();

        public HeaderGroup Build(DynamicRackSystem system, RackCatalog catalog, int levelCount = int.MaxValue)
            => Build(system, catalog, null, levelCount);

        public HeaderGroup Build(
            DynamicRackSystem system,
            RackCatalog catalog,
            DynamicRackFront front,
            int levelCount = int.MaxValue)
        {
            if (system == null || DynamicFrontGeometry.LoadBeamLevels(system, front).Count == 0 || system.TotalLength <= 0.0)
            {
                return null;
            }

            var axes = DynamicFlowBedGeometry.Resolve(system, catalog, front)
                .Where(axis => axis.LevelNumber <= levelCount)
                .ToList();
            if (axes.Count == 0)
            {
                return null;
            }

            var firstAxis = axes[0];
            var railMate = firstAxis.RailLocalMate;
            var laneDepth = DynamicFlowBedGeometry.ResolveBedLength(system, front);
            if (laneDepth <= 0.0)
            {
                return null;
            }

            // LONGITUD is the front's complete longitudinal span minus the fixed 4 in clearance. Catalog mates
            // position and rotate the assembly, but their offsets and the diagonal slope do not alter this cut.
            var localAssembly = flowBedBuilder.Build(new FlowBedConfiguration
            {
                BedType = FlowBedType.Dynamic,
                LaneDepth = laneDepth,
                PalletDepth = system.Pallet?.Depth ?? 0.0,
                RollerId = FlowBedDefaults.RollerId
            }, catalog);
            if (localAssembly.Count == 0)
            {
                return null;
            }

            // Store one level as a nested definition local to the exit elevation. Every further level differs only
            // by its vertical placement, preserving the ARRAY/shared-definition insertion pattern.
            var definitionInstances = localAssembly
                .Select(instance => RigidClone(
                    instance,
                    railMate,
                    firstAxis.ExitMate,
                    firstAxis.AngleRadians,
                    -firstAxis.ExitMate.Y))
                .ToList();
            var levelPlacements = axes
                .Select(axis => new HeaderPlacement(0.0, mirrored: false, insertionY: axis.ExitMate.Y))
                .ToList();

            var suffix = front == null ? string.Empty : " F" + (front.Index + 1);
            return new HeaderGroup("Cama dinamica" + suffix, definitionInstances, levelPlacements);
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
