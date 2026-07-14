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
        /// <summary>Nested-definition name prefix for the ARRAY grouping of the frontal (see <see cref="BuildPlan"/>).</summary>
        private const string GroupPrefix = "SEL_FRONTAL";

        /// <summary>
        /// The frontal as a structured plan (the ARRAY pattern): identical postes/largueros/placas collapse into ONE
        /// nested block definition referenced at every position, so AutoCAD sets each distinct piece's dynamic
        /// parameters ONCE instead of per piece — the dominant cost when inserting/redrawing many frentes. Geometry is
        /// identical to <see cref="Build"/> (which the WPF preview still consumes flat); the drawer already knows how to
        /// nest <see cref="DynamicSystemPlan.Headers"/>.
        /// </summary>
        public DynamicSystemPlan BuildPlan(SelectiveRackSystem system, RackCatalog catalog)
            => HeaderInstanceGrouper.Group(Build(system, catalog), GroupPrefix);

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

            // Safety elements at each post, origin coincident with the base plate's origin (the user's rule), on the
            // side that post resolves to. A LATERAL (end-of-row guard, LONGITUD = the total fondo) REPLACES the botas at
            // its frente. In the frontal the guard is seen edge-on (its length goes into the depth).
            var botas = SelectiveSafetyPlacement.EnabledOfType(system, catalog, view, SelectiveSafetyPlacement.BotaType);
            var laterales = SelectiveSafetyPlacement.EnabledOfType(system, catalog, view, SelectiveSafetyPlacement.LateralType);
            var systemDepth = SelectiveDepthLayout.TotalFondoDepth(system);

            // Blocks.FindBlock is a linear scan and the run repeats the same 1-2 beam/plate ids across ~100
            // levels + 21 plates, so memoize the RESULT of the existing lookup per piece id, scoped to this Build
            // (view is constant here; caching the FirstOrDefault result keeps first-match-wins on duplicate rows,
            // and the comparer matches FindBlock's OrdinalIgnoreCase). Null/blank ids bypass the cache: they keep
            // FindBlock's null result and Dictionary would reject the key.
            var blockByPieceId = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string CachedBlock(string pieceId)
            {
                if (string.IsNullOrWhiteSpace(pieceId)) return Block(catalog, pieceId, view);
                if (!blockByPieceId.TryGetValue(pieceId, out var block))
                {
                    block = Block(catalog, pieceId, view);
                    blockByPieceId[pieceId] = block;
                }

                return block;
            }

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

                // Base plate (unless the toggle is off): from this post's cabecera if any, else the run default.
                // PERALTE = the cabecera's manual override, else derived from the post (StandardPeralte).
                var plateId = cabecera?.LeftBasePlate?.PlateCatalogId;
                if (string.IsNullOrWhiteSpace(plateId)) plateId = defaultPlateId;
                var plateEntry = catalog?.BasePlates.FindBasePlate(plateId);
                var platePeralte = cabecera?.LeftBasePlate?.PeralteOverride ?? plateEntry?.StandardPeralte(postPeralte) ?? 0.0;

                AddPostWithPlate(instances, catalog, view, system.PostId, postBlock, CachedBlock, origin, postHeight, postPeralte, plateId, platePeralte, system.DrawBasePlate);

                // A LATERAL at this frente replaces the botas; otherwise the botas draw.
                if (SelectiveSafetyPlacement.DrawsAt(laterales, i))
                {
                    SelectiveSafetyPlacement.AppendAtPost(instances, catalog, view, laterales, origin, plateId, i, mirrorAxisX: null, longitud: systemDepth);
                }
                else
                {
                    SelectiveSafetyPlacement.AppendAtPost(instances, catalog, view, botas, origin, plateId, i);
                }
            }

            // Largueros: one per resolved level, at the left post's troquel X, at the level's resolved Y. A "medio
            // frente" bay is split into tramos: each LOADED tramo gets largueros of its own length (from its left
            // post), and an INTERMEDIATE post is planted at every tramo boundary. The shared end posts stay put.
            for (var i = 0; i < system.Bays.Count; i++)
            {
                var bay = system.Bays[i];
                var postPeralte = SelectivePostGeometry.PostPeralteAt(system, i);
                var troquelX = layout.TroquelXs[i]; // hook on THIS post's troquel (per-post peralte)
                var inicioX = SelectivePostGeometry.BeamProfileStartX(catalog, bay, view);
                var tramos = SelectiveMedioFrente.Resolve(bay, troquelX, inicioX);

                if (tramos == null)
                {
                    // Normal full bay: one larguero per level spanning the whole bay.
                    AddLargueros(instances, bay.Levels, CachedBlock, view, postX[i] + troquelX, bay.BeamLength);
                    continue;
                }

                foreach (var tramo in tramos)
                {
                    if (!tramo.Loaded) continue;
                    AddLargueros(instances, bay.Levels, CachedBlock, view, postX[i] + tramo.StartOffset + troquelX, tramo.Length);
                }

                // Intermediate posts = the left post of every tramo except the first (Tramos[k].StartOffset, k>=1).
                // A level-less bay (a column) can still be split; fall back to the run height like the planta so the
                // intermediate post is never a degenerate zero-height post.
                var intermediateHeight = bay.Height > 0.0 ? bay.Height : system.Height;
                var plateEntry = catalog?.BasePlates.FindBasePlate(defaultPlateId);
                var platePeralte = plateEntry?.StandardPeralte(postPeralte) ?? 0.0;
                for (var k = 1; k < tramos.Count; k++)
                {
                    var origin = new Point2D(postX[i] + tramos[k].StartOffset, 0.0);
                    AddPostWithPlate(instances, catalog, view, system.PostId, postBlock, CachedBlock, origin, intermediateHeight, postPeralte, defaultPlateId, platePeralte, system.DrawBasePlate);
                }
            }

            AddTopes(instances, system, catalog, postX, view);
            AddTarimas(instances, system, catalog, postX, layout, view, CachedBlock);
            AddAnnotations(instances, system, view, postX);

            // Where each bay's larguero PROFILE cut starts (hook troquel + the ménsula overhang INICIO_PERFIL) — the
            // larguero cotas span the true cut from here, not from the troquel (which would end short by the ménsula).
            var beamStartXs = new List<double>(system.Bays.Count);
            for (var i = 0; i < system.Bays.Count; i++)
            {
                var inicioX = SelectivePostGeometry.BeamProfileStartX(catalog, system.Bays[i], view);
                beamStartXs.Add(postX[i] + layout.TroquelXs[i] + inicioX);
            }

            SelectiveDimensions.AddFrontal(instances, system, view, postX, beamStartXs);
            return instances;
        }

        /// <summary>
        /// Larguero topes in the FRONTAL (opt-in via the TopeFrontal toggle): the rear stop seen edge-on, one per
        /// larguero whose (frente,level) grid cell is on, spanning the bay (LONGITUD = larguero + ¼"), ~8" above the
        /// larguero and snapped to the TROQUEL_TOPE grid, at the left post's TROQUEL_TOPE (its X slides with the peralte).
        /// </summary>
        private static void AddTopes(ICollection<HeaderBlockInstance> instances, SelectiveRackSystem system, RackCatalog catalog, IReadOnlyList<double> postX, string view)
        {
            var topes = SelectiveSafetyPlacement.EnabledOfType(system, catalog, view, SelectiveSafetyPlacement.TopeType);
            if (topes.Count == 0)
            {
                return;
            }

            var tope = topes[0];
            var selection = tope.Selection;
            if (!selection.TopeFrontal)
            {
                return; // the frontal is an opt-in toggle (lateral + planta always draw the tope)
            }

            var troquelEntry = catalog?.ConnectionLayout.FindConnectionLayout(system.PostId, SelectiveSafetyPlacement.TopePostPoint, view);
            var saque = selection.TopeSaque > 0.0 ? selection.TopeSaque : SelectiveSafetyPlacement.DefaultSaque;
            const double paso = 2.0;

            for (var i = 0; i < system.Bays.Count && i < postX.Count; i++)
            {
                var bay = system.Bays[i];
                if (bay.BeamLength <= 0.0)
                {
                    continue;
                }

                var postParams = new Dictionary<string, double> { [SelectiveRackDefaults.PeralteParam] = SelectivePostGeometry.PostPeralteAt(system, i) };
                var troquel = SelectivePostGeometry.Resolve(troquelEntry, postParams);
                var xStart = postX[i] + troquel.X;

                for (var lvl = 0; lvl < bay.Levels.Count; lvl++)
                {
                    if (!selection.TopeAt(i, lvl))
                    {
                        continue; // this (frente, level) cell is off
                    }

                    var y = troquel.Y + Math.Round((bay.Levels[lvl].Y + SelectiveSafetyPlacement.TopeYOffset - troquel.Y) / paso, MidpointRounding.AwayFromZero) * paso;
                    var at = new Point2D(xStart, y);
                    var instance = new HeaderBlockInstance
                    {
                        Role = HeaderBlockRole.Tope,
                        PieceId = tope.PieceId,
                        BlockName = tope.Block,
                        View = view,
                        Insertion = at,
                        ConnectionAnchor = at
                    };
                    instance.DynamicParameters[SelectiveRackDefaults.LengthParam] = bay.BeamLength + SelectiveSafetyPlacement.TopeLengthAllowance;
                    instance.DynamicParameters[SelectiveSafetyPlacement.SaqueParam] = saque;
                    instances.Add(instance);
                }
            }
        }

        /// <summary>Text labels when the toggles are on: a number centered under each frente (bay), a number to the
        /// left of each level (of the first bay), and the rack name above the frontal.</summary>
        private static void AddAnnotations(ICollection<HeaderBlockInstance> instances, SelectiveRackSystem system, string view, IReadOnlyList<double> postX)
        {
            var h = SelectiveAnnotations.TextHeightFor(system.AnnotationScale);
            var gap = h + SelectiveAnnotations.Margin;

            if (system.NumberFronts)
            {
                for (var i = 0; i < system.Bays.Count; i++)
                {
                    var centerX = (postX[i] + postX[i + 1]) / 2.0;
                    instances.Add(SelectiveAnnotations.Label(SelectiveAnnotations.Num(i + 1), view, new Point2D(centerX, -gap), h));
                }
            }

            if (system.NumberLevels && system.Bays.Count > 0)
            {
                var levels = system.Bays[0].Levels;
                for (var j = 0; j < levels.Count; j++)
                {
                    instances.Add(SelectiveAnnotations.Label(SelectiveAnnotations.Num(j + 1), view, new Point2D(postX[0] - gap, levels[j].Y), h));
                }
            }

            if (system.DrawRackName && !string.IsNullOrWhiteSpace(system.Name))
            {
                instances.Add(SelectiveAnnotations.Label(system.Name.Trim(), view, new Point2D(postX[0], system.Height + h), h * 1.5));
            }
        }

        /// <summary>
        /// Pallets (tarimas) as a VISUAL reference when DrawPallets is on: one pallet block per load position, resting on
        /// its beam's load surface (INICIO_PERFIL above the troquel) — the SAME X datum the larguero profile uses, so a
        /// pallet sits centred on the beam it rests on. Full bays use the design's per-level (and floor) pallet counts;
        /// a medio-frente bay draws pallets on each LOADED tramo, fitting as many as the tramo length allows (its tramos
        /// are free measures, not pallet-count driven). The "TARIMA" catalog block; if that row is missing in blocks.csv
        /// for this view, nothing is drawn. Never in the BOM (Role.Pallet).
        /// </summary>
        private static void AddTarimas(
            ICollection<HeaderBlockInstance> instances, SelectiveRackSystem system, RackCatalog catalog,
            IReadOnlyList<double> postX, SelectivePostLayout layout, string view, Func<string, string> cachedBlock)
        {
            if (!system.DrawPallets)
            {
                return;
            }

            var palletBlock = cachedBlock(SelectiveRackDefaults.PalletPieceId);
            if (string.IsNullOrWhiteSpace(palletBlock))
            {
                return; // "TARIMA" not wired in blocks.csv for this view — nothing to draw
            }

            for (var i = 0; i < system.Bays.Count && i < postX.Count; i++)
            {
                var bay = system.Bays[i];
                if (bay.BeamLength <= 0.0)
                {
                    continue;
                }

                var troquelX = layout.TroquelXs[i];
                var inicioX = SelectivePostGeometry.BeamProfileStartX(catalog, bay, view);
                // The pallet rests on the larguero PROFILE, which starts at the ménsula overhang (INICIO_PERFIL X) past
                // the troquel — the same datum the larguero cotas use (beamStartXs) and the Y path uses (BeamProfileStartY).
                var profileX = postX[i] + troquelX + inicioX;

                var tramos = SelectiveMedioFrente.Resolve(bay, troquelX, inicioX);
                if (tramos == null)
                {
                    // Normal full bay: the design's counts drive the rows across the whole profile.
                    if (bay.FloorPalletCount > 0 && bay.FloorPalletFrente > 0.0)
                    {
                        PlacePalletRow(instances, palletBlock, view, profileX, bay.BeamLength, 0.0,
                            bay.FloorPalletFrente, bay.FloorPalletAlto, bay.FloorPalletCount);
                    }

                    foreach (var level in bay.Levels)
                    {
                        if (level.PalletCount <= 0 || level.PalletFrente <= 0.0)
                        {
                            continue;
                        }

                        var surfaceY = level.Y + SelectivePostGeometry.BeamProfileStartY(catalog, level.BeamId, level.BeamPeralte, view);
                        PlacePalletRow(instances, palletBlock, view, profileX, bay.BeamLength, surfaceY,
                            level.PalletFrente, level.PalletAlto, level.PalletCount);
                    }

                    continue;
                }

                // Medio frente: pallets follow the largueros — one row per level per LOADED tramo, fitting as many as
                // the tramo length holds (the tramos are free measures, so the count is derived from geometry).
                foreach (var tramo in tramos)
                {
                    if (!tramo.Loaded)
                    {
                        continue;
                    }

                    var tramoX = postX[i] + tramo.StartOffset + troquelX + inicioX;
                    var floorFit = PalletFit(tramo.Length, bay.FloorPalletFrente);
                    if (bay.FloorPalletCount > 0 && floorFit > 0)
                    {
                        PlacePalletRow(instances, palletBlock, view, tramoX, tramo.Length, 0.0,
                            bay.FloorPalletFrente, bay.FloorPalletAlto, floorFit);
                    }

                    foreach (var level in bay.Levels)
                    {
                        var fit = PalletFit(tramo.Length, level.PalletFrente);
                        if (level.PalletCount <= 0 || fit <= 0)
                        {
                            continue;
                        }

                        var surfaceY = level.Y + SelectivePostGeometry.BeamProfileStartY(catalog, level.BeamId, level.BeamPeralte, view);
                        PlacePalletRow(instances, palletBlock, view, tramoX, tramo.Length, surfaceY,
                            level.PalletFrente, level.PalletAlto, fit);
                    }
                }
            }
        }

        /// <summary>How many pallets of width <paramref name="frente"/> fit in <paramref name="span"/> (0 if the frente is
        /// non-positive or wider than the span). Used only for medio-frente tramos, whose free length is not pallet-driven.</summary>
        private static int PalletFit(double span, double frente)
            => frente > 0.0 && frente <= span ? (int)Math.Floor(span / frente) : 0;

        /// <summary>Distribute <paramref name="count"/> pallets of width <paramref name="frente"/> evenly across
        /// [<paramref name="anchorX"/>, +<paramref name="span"/>] resting at <paramref name="bottomY"/> (the pallet's
        /// bottom). The TARIMA block's origin is BOTTOM-CENTRE, so each pallet is inserted centred in X (footprint-left +
        /// frente/2) and at its bottom in Y; the LONGITUD/ALTURA params size it.</summary>
        private static void PlacePalletRow(
            ICollection<HeaderBlockInstance> instances, string block, string view,
            double anchorX, double span, double bottomY, double frente, double alto, int count)
        {
            var gap = Math.Max(0.0, (span - count * frente) / (count + 1));
            for (var k = 0; k < count; k++)
            {
                // Origin at the pallet BOTTOM-CENTRE: footprint left = anchorX + gap*(k+1) + frente*k, bottom = bottomY.
                var at = new Point2D(anchorX + gap * (k + 1) + frente * k + frente / 2.0, bottomY);
                var pallet = new HeaderBlockInstance
                {
                    Role = HeaderBlockRole.Pallet,
                    PieceId = SelectiveRackDefaults.PalletPieceId,
                    BlockName = block,
                    View = view,
                    Insertion = at,
                    ConnectionAnchor = at
                };
                pallet.DynamicParameters[SelectiveRackDefaults.PalletFrenteParam] = frente;
                pallet.DynamicParameters[SelectiveRackDefaults.PalletAltoParam] = alto;
                instances.Add(pallet);
            }
        }

        /// <summary>Adds one larguero per level at <paramref name="beamX"/> (the tramo's left-post troquel X), each
        /// stretched to <paramref name="beamLongitud"/> and to its level's peralte, sitting at the level's resolved Y.</summary>
        private static void AddLargueros(
            ICollection<HeaderBlockInstance> instances, IEnumerable<SelectiveLevel> levels,
            Func<string, string> cachedBlock, string view, double beamX, double beamLongitud)
        {
            foreach (var level in levels)
            {
                var at = new Point2D(beamX, level.Y);
                var beam = new HeaderBlockInstance
                {
                    Role = HeaderBlockRole.Beam,
                    PieceId = level.BeamId,
                    BlockName = cachedBlock(level.BeamId),
                    View = view,
                    Insertion = at,
                    ConnectionAnchor = at
                };
                beam.DynamicParameters[SelectiveRackDefaults.LengthParam] = beamLongitud;
                beam.DynamicParameters[SelectiveRackDefaults.PeralteParam] = level.BeamPeralte;
                instances.Add(beam);
            }
        }

        /// <summary>A frontal post at <paramref name="origin"/> (LONGITUD = height, PERALTE = peralte) plus its base
        /// plate (mated on the post origin) unless drawing plates is off. Shared by the run posts and a medio-frente
        /// intermediate post.</summary>
        private static void AddPostWithPlate(
            ICollection<HeaderBlockInstance> instances, RackCatalog catalog, string view, string postId, string postBlock,
            Func<string, string> cachedBlock, Point2D origin, double postHeight, double postPeralte,
            string plateId, double platePeralte, bool drawPlate)
        {
            var post = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Post,
                PieceId = postId,
                BlockName = postBlock,
                View = view,
                Insertion = origin,
                ConnectionAnchor = origin
            };
            post.DynamicParameters[SelectiveRackDefaults.LengthParam] = postHeight;
            post.DynamicParameters[SelectiveRackDefaults.PeralteParam] = postPeralte;
            instances.Add(post);

            if (!drawPlate || string.IsNullOrWhiteSpace(plateId))
            {
                return;
            }

            var plateMate = Local(catalog, plateId, SelectiveRackDefaults.PlateMatePoint, view);
            var plate = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.BasePlate,
                PieceId = plateId,
                BlockName = cachedBlock(plateId),
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

        private static Point2D Local(RackCatalog catalog, string pieceId, string connectionPointId, string view)
            => CatalogLookup.Local(catalog, pieceId, connectionPointId, view);

        private static string Block(RackCatalog catalog, string pieceId, string view)
            => CatalogLookup.Block(catalog, pieceId, view);
    }
}
