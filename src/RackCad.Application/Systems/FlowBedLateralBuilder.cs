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
    /// Builds the lateral block instances for one roller bed ("cama de rodamiento"): the rail (LONGITUD =
    /// lane depth), the end stop (tope) whose origin lands on the rail's TROQUEL_TOPE, rollers from the tope
    /// at a pitch (the minimum allowed by the roller diameter unless overridden), and — on a DYNAMIC bed
    /// only — brakes (frenos) about every pallet depth: each brake sits 5 troqueles after the last roller
    /// (its 4.21" bracket needs the room) and the next roller resumes 2 troqueles past the brake. Pushback
    /// beds omit brakes. Pure: returns instances the AutoCAD drawer places (all on the troquel line Y).
    /// </summary>
    public sealed class FlowBedLateralBuilder
    {
        public IReadOnlyList<HeaderBlockInstance> Build(FlowBedConfiguration config, RackCatalog catalog)
        {
            var instances = new List<HeaderBlockInstance>();

            if (config == null || config.LaneDepth <= 0.0)
            {
                return instances;
            }

            var grid = FlowBedDefaults.TroquelPitch > 0.0 ? FlowBedDefaults.TroquelPitch : 1.0;
            var rollerId = string.IsNullOrWhiteSpace(config.RollerId) ? FlowBedDefaults.RollerId : config.RollerId;

            // The rail's first troquel (TROQUEL_TOPE) is the reference: its Y is the line everything sits on,
            // its X is where offsets are measured from. Rollers must stay within the rail (lane depth).
            var tope = Local(catalog, FlowBedDefaults.RailId, FlowBedDefaults.RailTopePoint, FlowBedDefaults.View);
            var lineY = tope.Y;
            var originX = tope.X;
            var maxOffset = config.LaneDepth - originX;

            // Rail at the origin (its length is the LONGITUD parameter).
            var rail = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Rail,
                PieceId = FlowBedDefaults.RailId,
                BlockName = Block(catalog, FlowBedDefaults.RailId, FlowBedDefaults.View),
                View = FlowBedDefaults.View,
                Insertion = new Point2D(0.0, 0.0),
                ConnectionAnchor = new Point2D(0.0, 0.0)
            };
            rail.DynamicParameters[SelectiveRackDefaults.LengthParam] = config.LaneDepth;
            instances.Add(rail);

            // End stop: its own origin lands on the rail's TROQUEL_TOPE.
            instances.Add(Piece(catalog, HeaderBlockRole.Stop, FlowBedDefaults.StopId, new Point2D(originX, lineY)));

            // First roller starts at the next troquel past the tope; further ones step by the pitch.
            var pitch = RollerPitch(config, catalog, rollerId, grid);
            var firstRoller = Ceil(FlowBedDefaults.TopeOccupiedLength, grid);
            if (firstRoller > maxOffset)
            {
                return instances; // rail too short for even one roller
            }

            instances.Add(Roller(catalog, rollerId, originX + firstRoller, lineY));
            var lastRoller = firstRoller;

            var dynamic = config.BedType == FlowBedType.Dynamic && config.PalletDepth > 0.0;
            var palletGap = config.PalletDepth + FlowBedDefaults.BrakeClearanceOverPallet;
            var brakeTarget = FlowBedDefaults.TopeOccupiedLength + palletGap; // first brake >= end of tope + pallet + 1"

            while (true)
            {
                // Time for a brake once a brake placed 5 troqueles after the last roller would reach the target
                // (>= pallet depth + 1" from the tope / previous brake), snapped onto the roller grid.
                if (dynamic && lastRoller + FlowBedDefaults.BrakeAfterLastRoller >= brakeTarget)
                {
                    var brakeOffset = lastRoller + FlowBedDefaults.BrakeAfterLastRoller;
                    var nextRoller = brakeOffset + FlowBedDefaults.RollerAfterBrake;
                    if (nextRoller > maxOffset)
                    {
                        // No room for a brake + its trailing roller before the rail end: fall back to plain
                        // rollers so the loading end stays supported instead of leaving the tail empty.
                        dynamic = false;
                        continue;
                    }

                    instances.Add(Piece(catalog, HeaderBlockRole.Brake, FlowBedDefaults.BrakeId, new Point2D(originX + brakeOffset, lineY)));
                    instances.Add(Roller(catalog, rollerId, originX + nextRoller, lineY));
                    lastRoller = nextRoller;
                    brakeTarget = brakeOffset + palletGap;
                    continue;
                }

                var next = lastRoller + pitch;
                if (next > maxOffset)
                {
                    break;
                }

                instances.Add(Roller(catalog, rollerId, originX + next, lineY));
                lastRoller = next;
            }

            return instances;
        }

        /// <summary>Roller pitch: the override if valid, else the minimum the diameter allows (rounded up to a troquel).
        /// The override is floored at the troquel grid: rollers hook ON troqueles, so a sub-grid pitch is physically
        /// impossible — and letting one through turns the placement loop into hundreds of thousands of instances
        /// (a typo like "0.01" froze the editor and AutoCAD's UI thread).</summary>
        private static double RollerPitch(FlowBedConfiguration config, RackCatalog catalog, string rollerId, double grid)
        {
            if (config.RollerPitchOverride.HasValue && config.RollerPitchOverride.Value > 0.0)
            {
                return Math.Max(grid, config.RollerPitchOverride.Value);
            }

            var diameter = catalog?.FlowBedProfiles
                .FirstOrDefault(c => string.Equals(c.Id, rollerId, StringComparison.OrdinalIgnoreCase))?.Diameter ?? 0.0;

            return Math.Max(grid, Ceil(diameter, grid)); // 1.9" -> 2", 2.5" -> 3"
        }

        private static double Ceil(double value, double grid) => Math.Ceiling(value / grid) * grid;

        private static HeaderBlockInstance Roller(RackCatalog catalog, string rollerId, double x, double y)
            => Piece(catalog, HeaderBlockRole.Roller, rollerId, new Point2D(x, y));

        private static HeaderBlockInstance Piece(RackCatalog catalog, HeaderBlockRole role, string pieceId, Point2D at)
            => new HeaderBlockInstance
            {
                Role = role,
                PieceId = pieceId,
                BlockName = Block(catalog, pieceId, FlowBedDefaults.View),
                View = FlowBedDefaults.View,
                Insertion = at,
                ConnectionAnchor = at
            };

        private static Point2D Local(RackCatalog catalog, string pieceId, string connectionPointId, string view)
            => CatalogLookup.Local(catalog, pieceId, connectionPointId, view);

        private static string Block(RackCatalog catalog, string pieceId, string view)
            => CatalogLookup.Block(catalog, pieceId, view);
    }
}
