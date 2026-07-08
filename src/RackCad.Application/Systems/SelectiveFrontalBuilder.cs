using System;
using System.Collections.Generic;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Builds the FRONTAL block instances of a selective rack: N+1 cabeceras (posts) with their base plates,
    /// and, per bay, one larguero at every load level. Pure — returns instances the AutoCAD drawer places.
    ///
    /// Geometry: the larguero hooks on the post's TROQUEL_LARGUERO, whose X slides with the post peralte
    /// (the parametric mate: X = localX + slope*peralte). The next post sits so its (mirrored) troquel lands
    /// on the larguero's far end — i.e. post-to-post = larguero length + 2*troquelX. Levels snap to the
    /// troquel grid (first troquel + k*paso). Beams stretch to the bay length (LONGITUD) and peralte (PERALTE).
    /// </summary>
    public sealed class SelectiveFrontalBuilder
    {
        public IReadOnlyList<HeaderBlockInstance> Build(SelectiveRackSystem system, RackCatalog catalog)
        {
            var instances = new List<HeaderBlockInstance>();

            if (system == null || system.Height <= 0.0 || system.Bays.Count == 0)
            {
                return instances;
            }

            var view = SelectiveRackDefaults.View;
            var paso = SelectiveRackDefaults.TroquelPaso;

            // Post's larguero troquel: X slides with the post peralte, Y is the first troquel (grid base).
            var postParams = new Dictionary<string, double> { [SelectiveRackDefaults.PeralteParam] = system.PostPeralte };
            var troquel = catalog?.ConnectionLayout.FindConnectionLayout(system.PostId, SelectiveRackDefaults.PostBeamPoint, view);
            var troquelX = ResolveX(troquel, postParams);
            var troquelBaseY = troquel?.LocalY ?? paso;

            var postBlock = Block(catalog, system.PostId, view);
            var plateId = catalog?.Defaults?.BasePlate;
            var plateBlock = Block(catalog, plateId, view);
            var plateMate = Local(catalog, plateId, SelectiveRackDefaults.PlateMatePoint, view);

            // Standard plate peralte, derived from the post peralte (the advanced editor may override later).
            var plateEntry = catalog?.BasePlates.FindBasePlate(plateId);
            var platePeralte = plateEntry?.StandardPeralte(system.PostPeralte) ?? 0.0;

            // Post X positions: post[i+1] = post[i] + larguero length + 2*troquelX.
            var postX = new List<double> { 0.0 };
            foreach (var bay in system.Bays)
            {
                postX.Add(postX[postX.Count - 1] + bay.BeamLength + 2.0 * troquelX);
            }

            // Cabeceras (posts) + their base plates.
            foreach (var x in postX)
            {
                var origin = new Point2D(x, 0.0);

                var post = new HeaderBlockInstance
                {
                    Role = HeaderBlockRole.Post,
                    PieceId = system.PostId,
                    BlockName = postBlock,
                    View = view,
                    Insertion = origin,
                    ConnectionAnchor = origin
                };
                post.DynamicParameters[SelectiveRackDefaults.LengthParam] = system.Height;
                post.DynamicParameters[SelectiveRackDefaults.PeralteParam] = system.PostPeralte;
                instances.Add(post);

                // Base plate: its MONTAJE_POSTE lands on the post origin; PERALTE derived from the post.
                var plate = new HeaderBlockInstance
                {
                    Role = HeaderBlockRole.BasePlate,
                    PieceId = plateId,
                    BlockName = plateBlock,
                    View = view,
                    ConnectionAnchor = origin,
                    Insertion = new Point2D(origin.X - plateMate.X, origin.Y - plateMate.Y)
                };
                if (platePeralte > 0.0)
                {
                    plate.DynamicParameters[SelectiveRackDefaults.PeralteParam] = platePeralte;
                }
                instances.Add(plate);
            }

            // Largueros: one per level, at the left post's troquel X, stepping up the troquel grid.
            for (var i = 0; i < system.Bays.Count; i++)
            {
                var bay = system.Bays[i];
                var beamBlock = Block(catalog, bay.BeamId, view);
                var beamX = postX[i] + troquelX;

                foreach (var y in LevelHeights(bay, troquelBaseY, paso))
                {
                    var at = new Point2D(beamX, y);
                    var beam = new HeaderBlockInstance
                    {
                        Role = HeaderBlockRole.Beam,
                        PieceId = bay.BeamId,
                        BlockName = beamBlock,
                        View = view,
                        Insertion = at,
                        ConnectionAnchor = at
                    };
                    beam.DynamicParameters[SelectiveRackDefaults.LengthParam] = bay.BeamLength;
                    beam.DynamicParameters[SelectiveRackDefaults.PeralteParam] = bay.BeamPeralte;
                    instances.Add(beam);
                }
            }

            return instances;
        }

        /// <summary>Level Ys: the first level snapped to the troquel grid, then a troquel-aligned separation apart.</summary>
        private static IReadOnlyList<double> LevelHeights(SelectiveBay bay, double troquelBaseY, double paso)
        {
            var levels = new List<double>();
            if (bay == null || bay.Levels <= 0)
            {
                return levels;
            }

            var first = Snap(bay.FirstLevel, troquelBaseY, paso);
            var separation = bay.Separation > 0.0
                ? Math.Max(paso, Math.Round(bay.Separation / paso, MidpointRounding.AwayFromZero) * paso)
                : paso;

            for (var j = 0; j < bay.Levels; j++)
            {
                levels.Add(first + j * separation);
            }

            return levels;
        }

        /// <summary>X of a connection point resolved for the given block parameters (X = localX + slope*param).</summary>
        private static double ResolveX(ConnectionLayoutEntry entry, IReadOnlyDictionary<string, double> parameters)
        {
            if (entry == null)
            {
                return 0.0;
            }

            var x = entry.LocalX;
            if (entry.LocalXPorParam != 0.0 && !string.IsNullOrWhiteSpace(entry.Param)
                && parameters != null && parameters.TryGetValue(entry.Param, out var value))
            {
                x += entry.LocalXPorParam * value;
            }

            return x;
        }

        private static double Snap(double value, double baseY, double paso)
            => baseY + Math.Round((value - baseY) / paso, MidpointRounding.AwayFromZero) * paso;

        private static Point2D Local(RackCatalog catalog, string pieceId, string connectionPointId, string view)
        {
            var entry = catalog?.ConnectionLayout.FindConnectionLayout(pieceId, connectionPointId, view);
            return entry == null ? new Point2D(0.0, 0.0) : new Point2D(entry.LocalX, entry.LocalY);
        }

        private static string Block(RackCatalog catalog, string pieceId, string view)
            => catalog?.Blocks.FindBlock(pieceId, view)?.BlockName;
    }
}
