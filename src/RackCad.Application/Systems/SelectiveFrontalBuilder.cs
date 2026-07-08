using System.Collections.Generic;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Builds the FRONTAL block instances of a RESOLVED selective rack: N+1 cabeceras (posts) with their base
    /// plates, and, per bay, one larguero at every load level. Pure — returns instances the AutoCAD drawer
    /// places. The pallet-driven derivation (beam lengths, level Ys, height) already happened in
    /// <see cref="SelectiveGeometryResolver"/>; this only lays out blocks.
    ///
    /// Geometry: the larguero hooks on the post's TROQUEL_LARGUERO, whose X slides with the post peralte
    /// (the parametric mate: X = localX + slope*peralte). Posts carry troqueles on BOTH sides, so no mirror
    /// is needed. The larguero's LONGITUD is the "A corte" (profile cut length), not the clear span: the
    /// ménsula juts out from each profile end to the hook by INICIO_PERFIL's X. So the far hook lands on the
    /// next post's troquel when post-to-post = larguero length + 2*(troquelX + inicioPerfilX). Beams stretch to
    /// the bay length (LONGITUD) and to each level's peralte (PERALTE); each level sits at its resolved Y.
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

            // Post's larguero troquel: X slides with the post peralte (Y is not needed here — levels are resolved).
            var postParams = new Dictionary<string, double> { [SelectiveRackDefaults.PeralteParam] = system.PostPeralte };
            var troquel = catalog?.ConnectionLayout.FindConnectionLayout(system.PostId, SelectiveRackDefaults.PostBeamPoint, view);
            var troquelX = ResolveX(troquel, postParams);

            var postBlock = Block(catalog, system.PostId, view);
            var plateId = catalog?.Defaults?.BasePlate;
            var plateBlock = Block(catalog, plateId, view);
            var plateMate = Local(catalog, plateId, SelectiveRackDefaults.PlateMatePoint, view);

            // Standard plate peralte, derived from the post peralte (the advanced editor may override later).
            var plateEntry = catalog?.BasePlates.FindBasePlate(plateId);
            var platePeralte = plateEntry?.StandardPeralte(system.PostPeralte) ?? 0.0;

            // Post X positions. The larguero's LONGITUD is the profile "A corte", so the hooks sit an extra
            // ménsula overhang (INICIO_PERFIL's X) beyond each profile end. Post-to-post therefore adds the
            // troquel offset AND that overhang on both sides: post[i+1] = post[i] + length + 2*(troquelX + inicioX).
            var postX = new List<double> { 0.0 };
            foreach (var bay in system.Bays)
            {
                var inicioX = BeamProfileStartX(catalog, bay, view);
                postX.Add(postX[postX.Count - 1] + bay.BeamLength + 2.0 * (troquelX + inicioX));
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

            // Largueros: one per resolved level, at the left post's troquel X, at the level's resolved Y.
            for (var i = 0; i < system.Bays.Count; i++)
            {
                var bay = system.Bays[i];
                var beamX = postX[i] + troquelX;

                foreach (var level in bay.Levels)
                {
                    var at = new Point2D(beamX, level.Y);
                    var beam = new HeaderBlockInstance
                    {
                        Role = HeaderBlockRole.Beam,
                        PieceId = level.BeamId,
                        BlockName = Block(catalog, level.BeamId, view),
                        View = view,
                        Insertion = at,
                        ConnectionAnchor = at
                    };
                    beam.DynamicParameters[SelectiveRackDefaults.LengthParam] = bay.BeamLength;
                    beam.DynamicParameters[SelectiveRackDefaults.PeralteParam] = level.BeamPeralte;
                    instances.Add(beam);
                }
            }

            return instances;
        }

        /// <summary>
        /// The bay's INICIO_PERFIL X (ménsula overhang from the hook to the profile start); 0 if unset. Post
        /// spacing is per bay, so it uses the bay's first level as the representative beam (all largueros of a
        /// bay share one length and connector).
        /// </summary>
        private static double BeamProfileStartX(RackCatalog catalog, SelectiveBay bay, string view)
        {
            if (bay.Levels.Count == 0)
            {
                return 0.0;
            }

            var level = bay.Levels[0];
            var entry = catalog?.ConnectionLayout.FindConnectionLayout(level.BeamId, SelectiveRackDefaults.BeamProfileStartPoint, view);
            var beamParams = new Dictionary<string, double> { [SelectiveRackDefaults.PeralteParam] = level.BeamPeralte };
            return ResolveX(entry, beamParams);
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

        private static Point2D Local(RackCatalog catalog, string pieceId, string connectionPointId, string view)
        {
            var entry = catalog?.ConnectionLayout.FindConnectionLayout(pieceId, connectionPointId, view);
            return entry == null ? new Point2D(0.0, 0.0) : new Point2D(entry.LocalX, entry.LocalY);
        }

        private static string Block(RackCatalog catalog, string pieceId, string view)
            => catalog?.Blocks.FindBlock(pieceId, view)?.BlockName;
    }
}
