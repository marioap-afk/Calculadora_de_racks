using System;
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

            // Post X positions + each post's larguero troquel X (per-post; slides with that post's peralte).
            var layout = SelectivePostGeometry.Compute(system, catalog);
            var postX = layout.PostXs;

            var postBlock = Block(catalog, system.PostId, view);
            var defaultPlateId = catalog?.Defaults?.BasePlate;

            // Cabeceras (posts) + their base plates. Each post is as tall as the tallest bay it touches.
            for (var i = 0; i < postX.Count; i++)
            {
                var origin = new Point2D(postX[i], 0.0);

                // A per-post cabecera (if the user customized it) owns this post's height + base plate, so the frontal
                // post and the lateral corte agree. Without one, the resolved (matrix-driven) height governs.
                var cabecera = i < system.PostCabeceras.Count ? system.PostCabeceras[i] : null;
                var postHeight = cabecera != null && cabecera.Height > 0.0
                    ? cabecera.Height
                    : SelectivePostGeometry.PostHeight(system, i);

                // This post's peralte (its per-post override, else the run default) drives the PERALTE param and plate.
                var postPeralte = SelectivePostGeometry.PostPeralteAt(system, i);

                var post = new HeaderBlockInstance
                {
                    Role = HeaderBlockRole.Post,
                    PieceId = system.PostId,
                    BlockName = postBlock,
                    View = view,
                    Insertion = origin,
                    ConnectionAnchor = origin
                };
                post.DynamicParameters[SelectiveRackDefaults.LengthParam] = postHeight;
                post.DynamicParameters[SelectiveRackDefaults.PeralteParam] = postPeralte;
                instances.Add(post);

                // Base plate (unless the "Dibujar placa base" toggle is off): from this post's cabecera if any, else
                // the run default. PERALTE = the cabecera's manual override, else derived from the post (StandardPeralte).
                if (system.DrawBasePlate)
                {
                    var plateId = cabecera?.LeftBasePlate?.PlateCatalogId;
                    if (string.IsNullOrWhiteSpace(plateId)) plateId = defaultPlateId;
                    var plateEntry = catalog?.BasePlates.FindBasePlate(plateId);
                    var platePeralte = cabecera?.LeftBasePlate?.PeralteOverride ?? plateEntry?.StandardPeralte(postPeralte) ?? 0.0;
                    var plateMate = Local(catalog, plateId, SelectiveRackDefaults.PlateMatePoint, view);

                    var plate = new HeaderBlockInstance
                    {
                        Role = HeaderBlockRole.BasePlate,
                        PieceId = plateId,
                        BlockName = Block(catalog, plateId, view),
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
            }

            // Largueros: one per resolved level, at the left post's troquel X, at the level's resolved Y.
            for (var i = 0; i < system.Bays.Count; i++)
            {
                var bay = system.Bays[i];
                var beamX = postX[i] + layout.TroquelXs[i]; // hook on THIS post's troquel (per-post peralte)

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

            AddAnnotations(instances, system, view, postX);
            return instances;
        }

        /// <summary>Text labels when the toggles are on: a number centered under each frente (bay), a number to the
        /// left of each level (of the first bay), and the rack name above the frontal.</summary>
        private static void AddAnnotations(ICollection<HeaderBlockInstance> instances, SelectiveRackSystem system, string view, IReadOnlyList<double> postX)
        {
            const double gap = SelectiveAnnotations.TextHeight + SelectiveAnnotations.Margin;

            if (system.NumberFronts)
            {
                for (var i = 0; i < system.Bays.Count; i++)
                {
                    var centerX = (postX[i] + postX[i + 1]) / 2.0;
                    instances.Add(SelectiveAnnotations.Label(SelectiveAnnotations.Num(i + 1), view, new Point2D(centerX, -gap)));
                }
            }

            if (system.NumberLevels && system.Bays.Count > 0)
            {
                var levels = system.Bays[0].Levels;
                for (var j = 0; j < levels.Count; j++)
                {
                    instances.Add(SelectiveAnnotations.Label(SelectiveAnnotations.Num(j + 1), view, new Point2D(postX[0] - gap, levels[j].Y)));
                }
            }

            if (system.DrawRackName && !string.IsNullOrWhiteSpace(system.Name))
            {
                instances.Add(SelectiveAnnotations.Label(system.Name.Trim(), view, new Point2D(postX[0], system.Height + SelectiveAnnotations.TextHeight), SelectiveAnnotations.TextHeight * 1.5));
            }
        }

        private static Point2D Local(RackCatalog catalog, string pieceId, string connectionPointId, string view)
            => CatalogLookup.Local(catalog, pieceId, connectionPointId, view);

        private static string Block(RackCatalog catalog, string pieceId, string view)
            => CatalogLookup.Block(catalog, pieceId, view);
    }
}
