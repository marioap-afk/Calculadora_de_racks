using System;
using System.Collections.Generic;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.Headers;
using RackCad.Application.RackFrames;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Turns a resolved selective run into its LATERAL sections: one cross-section per post, positioned at the SAME X
    /// as the frontal post (<see cref="SelectivePostGeometry"/>). For a doble-profundidad rack the corte shows EVERY
    /// fondo along the depth axis (<see cref="SelectiveDepthLayout"/>): each fondo is its own cabecera at ITS OWN height
    /// (a side can be taller or shorter) with ITS OWN front/back largueros (its own levels). fondo 0's cabecera is the
    /// corte's <see cref="SelectiveCorte.Cabecera"/> (drawn by the AutoCAD side); the extra fondos + all largueros are
    /// loose instances so the corte stays a single block. Safety elements (protectores/topes/mallas) are a future
    /// extension that needs their own catalog blocks. Pure.
    /// </summary>
    public sealed class SelectiveLateralBuilder
    {
        /// <summary>View the lateral corte draws in (posts, celosía and largueros share it). See views.csv / blocks.csv.</summary>
        public const string LateralView = "LATERAL";

        /// <summary>Nominal vertical spacing between the doble-profundidad separadores (the dynamic uses 60"; here 100").</summary>
        public const double SeparatorMaxSpacing = 100.0;

        private readonly LateralHeaderLayoutBuilder layoutBuilder = new LateralHeaderLayoutBuilder();

        public IReadOnlyList<SelectiveCorte> Cortes(SelectiveRackSystem system, RackCatalog catalog)
        {
            var cortes = new List<SelectiveCorte>();
            if (system == null || system.Bays.Count == 0)
            {
                return cortes;
            }

            // Built from the catalog the caller already loaded (a field initializer would trigger its own load).
            var factory = new RackFrameConfigurationFactory(catalog);

            var postXs = SelectiveDepthLayout.MasterGrid(system, catalog).PostXs; // longest fondo defines the frente grid
            var offsets = SelectiveDepthLayout.Offsets(system); // one X per fondo (doble profundidad), per-fondo depth
            var template = RackFrameTemplateCatalog.FindStandardOrDefault();

            // Safety elements (lateral blocks) + the default plate whose LATERAL mate places them at the front-post base.
            // A LATERAL (end-of-row guard, LONGITUD = the corte's depth) REPLACES the botas at its frente.
            var botas = SelectiveSafetyPlacement.EnabledOfType(system, catalog, LateralView, SelectiveSafetyPlacement.BotaType);
            var laterales = SelectiveSafetyPlacement.EnabledOfType(system, catalog, LateralView, SelectiveSafetyPlacement.LateralType);
            var defaultPlateId = catalog?.Defaults?.BasePlate;

            // Each fondo has its OWN bays (own levels/heights AND its own frente count). Resolve them + a per-fondo
            // height fallback once.
            var fondoBays = new IList<SelectiveBay>[offsets.Count];
            var fondoFallback = new double[offsets.Count];
            for (var k = 0; k < offsets.Count; k++)
            {
                fondoBays[k] = SelectiveDepthLayout.BaysOfFondo(system, k);
                var m = 0.0;
                foreach (var bay in fondoBays[k]) if (bay.Height > m) m = bay.Height;
                fondoFallback[k] = m > 0.0 ? m : system.Height;
            }

            for (var i = 0; i < postXs.Count; i++)
            {
                // A fondo with C bays has posts 0..C, so it "reaches" post i when i <= its bay count. In a corner layout
                // a shorter fondo drops out of the far cortes. The FIRST reaching fondo anchors this corte's depth (its
                // cabecera is the primary, drawn at the block origin by the AutoCAD side); the rest translate relative to
                // it, so a corte where fondo 0 doesn't reach still has a valid primary.
                var firstReaching = -1;
                for (var k = 0; k < offsets.Count; k++)
                {
                    if (i <= fondoBays[k].Count)
                    {
                        firstReaching = k;
                        break;
                    }
                }

                if (firstReaching < 0)
                {
                    continue; // no fondo reaches this post (shouldn't happen within the master grid)
                }

                var anchorOffset = offsets[firstReaching];

                // Primary cabecera = the first reaching fondo. A per-post custom cabecera wins, but only for fondo 0.
                var custom = firstReaching == 0 && i < system.PostCabeceras.Count ? system.PostCabeceras[i] : null;
                RackFrameConfiguration cabecera;
                if (custom != null && custom.Height > 0.0)
                {
                    cabecera = custom;
                }
                else
                {
                    var heightP = SelectivePostGeometry.PostHeight(fondoBays[firstReaching], i, fondoFallback[firstReaching]);
                    if (heightP <= 0.0)
                    {
                        continue;
                    }

                    cabecera = factory.Build(template, system.PostId, heightP, SelectiveDepthLayout.CabeceraDepthOfFondo(system, firstReaching));
                }

                var extras = new List<HeaderBlockInstance>();

                // The OTHER reaching fondos: each its OWN cabecera at its OWN height AND its own fondo (depth) — a side
                // can be taller/shorter and deeper/shallower — translated to its X offset relative to the anchor.
                for (var k = 0; k < offsets.Count; k++)
                {
                    if (k == firstReaching || i > fondoBays[k].Count)
                    {
                        continue;
                    }

                    var heightK = SelectivePostGeometry.PostHeight(fondoBays[k], i, fondoFallback[k]);
                    if (heightK <= 0.0)
                    {
                        continue;
                    }

                    AddCabeceraAtOffset(extras, factory.Build(template, system.PostId, heightK, SelectiveDepthLayout.CabeceraDepthOfFondo(system, k)), offsets[k] - anchorOffset, catalog);
                }

                // Largueros: every REACHING fondo contributes its OWN levels at its own X offset + depth (anchor-relative).
                for (var k = 0; k < offsets.Count; k++)
                {
                    if (i > fondoBays[k].Count)
                    {
                        continue;
                    }

                    BuildLargueros(extras, fondoBays[k], i, SelectiveDepthLayout.CabeceraDepthOfFondo(system, k), offsets[k] - anchorOffset, catalog);
                }

                // Separadores (doble profundidad): spacer beams stacked vertically every ~100" of height that span the
                // GAP between adjacent reaching fondos — same logic as the dynamic's separadores, but 100" instead of 60".
                AddSeparadores(extras, system, catalog, fondoBays, fondoFallback, offsets, anchorOffset, i);

                // Larguero topes (rear pallet stops): at the central fondo's back post, one per larguero level.
                AddTopes(extras, system, catalog, fondoBays, offsets, anchorOffset, i);

                // Tarimas (visual reference): one per reaching fondo per level, spanning the fondo along the depth axis.
                AddTarimas(extras, system, catalog, fondoBays, offsets, anchorOffset, i);

                // Botas belong to the SYSTEM, not each cabecera: ONE at the corte's frontmost post (anchor-relative
                // X=0) for Left, reflected to the backmost post for Right, about the center of THIS corte's total fondo
                // span (the backmost reaching fondo). Never one per fondo.
                var backmostDepth = 0.0;
                for (var k = 0; k < offsets.Count; k++)
                {
                    if (i > fondoBays[k].Count)
                    {
                        continue;
                    }

                    var back = (offsets[k] - anchorOffset) + SelectiveDepthLayout.CabeceraDepthOfFondo(system, k);
                    if (back > backmostDepth) backmostDepth = back;
                }

                var corteFront = new Point2D(0.0, 0.0);
                if (SelectiveSafetyPlacement.DrawsAt(laterales, i))
                {
                    SelectiveSafetyPlacement.AppendAtPost(extras, catalog, LateralView, laterales, corteFront, defaultPlateId, i, backmostDepth / 2.0, longitud: backmostDepth);
                }
                else
                {
                    SelectiveSafetyPlacement.AppendAtPost(extras, catalog, LateralView, botas, corteFront, defaultPlateId, i, backmostDepth / 2.0);
                }

                // Annotations once, from the primary (anchor) fondo's levels + its height.
                AddCorteAnnotations(extras, system, i, CollectLevels(fondoBays[firstReaching], i), fondoBays[firstReaching], fondoFallback[firstReaching]);

                // Cotas for this corte: the reaching fondos' anchor-relative depths (X = depth axis), the anchor
                // fondo's larguero rows, and the corte's total height (the tallest reaching fondo).
                var reachingFrontXs = new List<double>();
                var reachingDepths = new List<double>();
                var corteHeight = 0.0;
                for (var k = 0; k < offsets.Count; k++)
                {
                    if (i > fondoBays[k].Count)
                    {
                        continue; // this fondo doesn't reach the corte
                    }

                    reachingFrontXs.Add(offsets[k] - anchorOffset);
                    reachingDepths.Add(SelectiveDepthLayout.CabeceraDepthOfFondo(system, k));
                    var heightK = SelectivePostGeometry.PostHeight(fondoBays[k], i, fondoFallback[k]);
                    if (heightK > corteHeight) corteHeight = heightK;
                }

                var corteLevelYs = new List<double>();
                foreach (var level in CollectLevels(fondoBays[firstReaching], i))
                {
                    var y = Math.Round(level.Y, 4);
                    if (y > 1e-6 && !corteLevelYs.Contains(y)) corteLevelYs.Add(y);
                }
                corteLevelYs.Sort();

                SelectiveDimensions.AddLateralCorte(extras, system, LateralView, reachingFrontXs, reachingDepths, corteLevelYs, corteHeight);

                cortes.Add(new SelectiveCorte(i, postXs[i], cabecera, extras));
            }

            return cortes;
        }

        /// <summary>Build one fondo's cabecera geometry and translate it to its X offset (fondo 0 is drawn from its config).</summary>
        private void AddCabeceraAtOffset(ICollection<HeaderBlockInstance> result, RackFrameConfiguration cabecera, double offset, RackCatalog catalog)
        {
            if (cabecera == null || cabecera.Height <= 0.0 || cabecera.Depth <= 0.0)
            {
                return;
            }

            var parameters = LateralHeaderParametersFactory.FromConfiguration(cabecera);
            var layout = layoutBuilder.Build(cabecera, parameters, catalog);
            foreach (var instance in layout.Instances)
            {
                result.Add(Translate(instance, offset));
            }
        }

        /// <summary>
        /// The lateral larguero sections that attach at frame <paramref name="postIndex"/> for ONE fondo (its own
        /// <paramref name="bays"/>): each distinct level of the adjacent bays gets a FRONT section at the fondo's front
        /// post (X=<paramref name="offset"/>) and a BACK section at its back post (X=offset+fondo), at the level's Y.
        /// The beam PERALTE carries over. In a lateral view the beam is seen end-on, so LONGITUD is irrelevant.
        /// </summary>
        private static void BuildLargueros(ICollection<HeaderBlockInstance> result, IList<SelectiveBay> bays, int postIndex, double depth, double offset, RackCatalog catalog)
        {
            foreach (var level in CollectLevels(bays, postIndex))
            {
                var block = catalog?.Blocks.FindBlock(level.BeamId, LateralView)?.BlockName;
                result.Add(MakeLarguero(level, block, x: offset, mirrored: false));          // front post of this fondo
                result.Add(MakeLarguero(level, block, x: offset + depth, mirrored: true));    // back post of this fondo
            }
        }

        /// <summary>
        /// Separadores between adjacent REACHING fondos (doble profundidad) for this corte: spacer beams stacked
        /// vertically (one per ~<see cref="SeparatorMaxSpacing"/>" of height, the dynamic's distribution) that span the
        /// GAP between fondo k's back post and fondo k+1's front post. Each beam is the <c>SEPARADOR</c> block (FRONTAL,
        /// like the dynamic), anchored on the back post's <c>TROQUEL_SEPARADOR</c>, with LONGITUD = the gap. Loose.
        /// </summary>
        private void AddSeparadores(
            ICollection<HeaderBlockInstance> result, SelectiveRackSystem system, RackCatalog catalog,
            IList<SelectiveBay>[] fondoBays, double[] fondoFallback, IReadOnlyList<double> offsets, double anchorOffset, int postIndex)
        {
            // Reaching fondos, in depth order; need at least two to have a gap.
            var reaching = new List<int>();
            for (var k = 0; k < offsets.Count; k++)
            {
                if (postIndex <= fondoBays[k].Count) reaching.Add(k);
            }

            if (reaching.Count < 2 || string.IsNullOrWhiteSpace(system.PostId))
            {
                return;
            }

            var separatorBlock = CatalogLookup.Block(catalog, DynamicRackDefaults.SeparatorCatalogId, DynamicRackDefaults.SeparatorView);
            if (string.IsNullOrWhiteSpace(separatorBlock))
            {
                return; // no separador block defined yet
            }

            var separatorMate = CatalogLookup.Local(catalog, DynamicRackDefaults.SeparatorCatalogId, DynamicRackDefaults.SeparatorMatePoint, DynamicRackDefaults.SeparatorView);
            var troquelSeparador = CatalogLookup.Local(catalog, system.PostId, DynamicRackDefaults.SeparatorPostPoint, LateralView);
            const double paso = 2.0; // the standard troquel pitch (as in the dynamic default)

            for (var r = 0; r + 1 < reaching.Count; r++)
            {
                var k = reaching[r];
                var kNext = reaching[r + 1];

                var backX = (offsets[k] - anchorOffset) + SelectiveDepthLayout.CabeceraDepthOfFondo(system, k); // fondo k's back post
                var frontXNext = offsets[kNext] - anchorOffset;                                                 // fondo k+1's front post
                var gap = frontXNext - backX;
                if (gap <= 0.0)
                {
                    continue;
                }

                // Tie up to where BOTH fondos exist (the shorter one).
                var height = Math.Min(
                    SelectivePostGeometry.PostHeight(fondoBays[k], postIndex, fondoFallback[k]),
                    SelectivePostGeometry.PostHeight(fondoBays[kNext], postIndex, fondoFallback[kNext]));
                if (height <= 0.0)
                {
                    continue;
                }

                var levels = SeparatorLevelCalculator.Levels(height, troquelSeparador.Y, paso, maxSpacing: SeparatorMaxSpacing);
                foreach (var level in levels)
                {
                    // Anchor on the (mirrored) back post's TROQUEL_SEPARADOR, one troquel inside the post; span the gap.
                    var anchor = new Point2D(backX - troquelSeparador.X, level);
                    var separador = new HeaderBlockInstance
                    {
                        Role = HeaderBlockRole.Separator,
                        PieceId = DynamicRackDefaults.SeparatorCatalogId,
                        BlockName = separatorBlock,
                        View = DynamicRackDefaults.SeparatorView,
                        ConnectionAnchor = anchor,
                        Insertion = new Point2D(anchor.X - separatorMate.X, anchor.Y - separatorMate.Y)
                    };
                    separador.DynamicParameters[SelectiveRackDefaults.LengthParam] = gap;
                    result.Add(separador);
                }
            }
        }

        /// <summary>
        /// Larguero topes (rear pallet stops) for this corte: at the back post of each tope fondo (central shared, or
        /// the central pair per-fondo), one per larguero level whose (frente,level) grid cell is on, ~8" above the
        /// larguero, mated on the (mirrored) back post's <c>TROQUEL_SEPARADOR</c>, SAQUE from the config. Loose; the
        /// levels of the two bays a corte bounds are deduped by Y so the same height isn't drawn twice.
        /// </summary>
        private void AddTopes(
            ICollection<HeaderBlockInstance> result, SelectiveRackSystem system, RackCatalog catalog,
            IList<SelectiveBay>[] fondoBays, IReadOnlyList<double> offsets, double anchorOffset, int postIndex)
        {
            var topes = SelectiveSafetyPlacement.EnabledOfType(system, catalog, LateralView, SelectiveSafetyPlacement.TopeType);
            if (topes.Count == 0 || string.IsNullOrWhiteSpace(system.PostId))
            {
                return;
            }

            var troquel = CatalogLookup.Local(catalog, system.PostId, SelectiveSafetyPlacement.TopePostPoint, LateralView);
            const double paso = 2.0; // the tope must land on a tope troquel: mate.Y + a whole number of pasos

            foreach (var tope in topes)
            {
                var selection = tope.Selection;
                var saque = selection.TopeSaque > 0.0 ? selection.TopeSaque : SelectiveSafetyPlacement.DefaultSaque;

                foreach (var spot in SelectiveSafetyPlacement.TopeSpots(selection, offsets.Count))
                {
                    var f = spot.Fondo;
                    if (f < 0 || f >= fondoBays.Length || postIndex > fondoBays[f].Count)
                    {
                        continue; // fondo f doesn't reach this corte
                    }

                    // Both spots of a per-fondo pair flank the CENTRAL GAP: fondo c's back post, and fondo c+1's FRONT post.
                    var frontX = offsets[f] - anchorOffset;
                    var postX = spot.AtFront ? frontX : frontX + SelectiveDepthLayout.CabeceraDepthOfFondo(system, f);
                    var mateX = spot.AtFront ? postX + troquel.X : postX - troquel.X; // the TROQUEL_TOPE, facing the gap

                    // Grid-filtered distinct larguero heights of the (up to two) bays this corte bounds.
                    var ys = new HashSet<double>();
                    var bays = fondoBays[f];
                    for (var b = postIndex - 1; b <= postIndex; b++)
                    {
                        if (b < 0 || b >= bays.Count) continue;
                        for (var lvl = 0; lvl < bays[b].Levels.Count; lvl++)
                        {
                            if (selection.TopeAt(b, lvl)) ys.Add(Math.Round(bays[b].Levels[lvl].Y, 4));
                        }
                    }

                    foreach (var y0 in ys)
                    {
                        // Rise ~8" above the larguero, then snap to the TROQUEL_TOPE grid (a whole number of pasos from the mate).
                        var y = troquel.Y + Math.Round((y0 + SelectiveSafetyPlacement.TopeYOffset - troquel.Y) / paso, MidpointRounding.AwayFromZero) * paso;
                        var at = new Point2D(mateX, y);
                        var instance = new HeaderBlockInstance
                        {
                            Role = HeaderBlockRole.Tope,
                            PieceId = tope.PieceId,
                            BlockName = tope.Block,
                            View = LateralView,
                            MirroredX = spot.Mirror,
                            ConnectionAnchor = at,
                            Insertion = at
                        };
                        instance.DynamicParameters[SelectiveSafetyPlacement.SaqueParam] = saque;
                        result.Add(instance);
                    }
                }
            }
        }

        /// <summary>
        /// Tarimas (pallet visual reference) for this corte when DrawPallets is on: one pallet per REACHING fondo per
        /// level, seen edge-on so it spans the fondo along the DEPTH axis (its LONGITUD param carries the fondo, its
        /// ALTURA the pallet alto). All the pallets of a level across the frente overlap into one in the lateral
        /// projection, so a single pallet per fondo/level is drawn. Centred on the cabecera depth, resting on the same
        /// load surface (INICIO_PERFIL Y) the frontal uses. Plus the floor pallet, if the fondo has one. Never in the BOM.
        /// The "TARIMA_GENERICA" catalog block for the LATERAL view; if that row is missing, nothing is drawn.
        /// </summary>
        private static void AddTarimas(
            ICollection<HeaderBlockInstance> result, SelectiveRackSystem system, RackCatalog catalog,
            IList<SelectiveBay>[] fondoBays, IReadOnlyList<double> offsets, double anchorOffset, int postIndex)
        {
            if (!system.DrawPallets)
            {
                return;
            }

            var block = catalog?.Blocks.FindBlock(SelectiveRackDefaults.PalletPieceId, LateralView)?.BlockName;
            if (string.IsNullOrWhiteSpace(block))
            {
                return; // "TARIMA_GENERICA" not wired for the LATERAL view — nothing to draw
            }

            for (var k = 0; k < offsets.Count; k++)
            {
                if (postIndex > fondoBays[k].Count)
                {
                    continue; // this fondo doesn't reach this corte
                }

                var palletFondo = SelectiveDepthLayout.DepthOfFondo(system, k); // the pallet's depth = LONGITUD in lateral
                if (palletFondo <= 0.0)
                {
                    continue;
                }

                // Centre the pallet on the cabecera span (front post at offsetRel, back post at +cabeceraDepth); a pallet
                // deeper than the frame overhangs the beams front and back, as real pallets do.
                var offsetRel = offsets[k] - anchorOffset;
                var startX = offsetRel + (SelectiveDepthLayout.CabeceraDepthOfFondo(system, k) - palletFondo) / 2.0;

                // Dedup by Y ALONE: CollectLevels keeps distinct beams at the same height (each larguero needs its own
                // end-section), but a tarima is one visual reference per load height — two beams at one Y = one pallet.
                var seenY = new HashSet<double>();
                foreach (var level in CollectLevels(fondoBays[k], postIndex))
                {
                    if (level.PalletAlto <= 0.0 || !seenY.Add(Math.Round(level.Y, 4)))
                    {
                        continue;
                    }

                    // Height (INICIO_PERFIL Y above the troquel) is view-independent, so reuse the frontal load surface.
                    var surfaceY = level.Y + SelectivePostGeometry.BeamProfileStartY(catalog, level.BeamId, level.BeamPeralte, SelectiveRackDefaults.View);
                    result.Add(MakePallet(block, startX, surfaceY, palletFondo, level.PalletAlto));
                }

                var floorAlto = FloorPalletAltoOf(fondoBays[k], postIndex);
                if (floorAlto > 0.0)
                {
                    result.Add(MakePallet(block, startX, 0.0, palletFondo, floorAlto)); // the ground pallet rests on the floor
                }
            }
        }

        /// <summary>The floor pallet ALTURA of the (up to two) bays post <paramref name="postIndex"/> bounds — the ground
        /// pallet that rests on the floor with no larguero — or 0 if neither has one.</summary>
        private static double FloorPalletAltoOf(IList<SelectiveBay> bays, int postIndex)
        {
            for (var b = postIndex - 1; b <= postIndex; b++)
            {
                if (b < 0 || b >= bays.Count)
                {
                    continue;
                }

                if (bays[b].FloorPalletCount > 0 && bays[b].FloorPalletAlto > 0.0)
                {
                    return bays[b].FloorPalletAlto;
                }
            }

            return 0.0;
        }

        private static HeaderBlockInstance MakePallet(string block, double x, double y, double fondo, double alto)
        {
            var at = new Point2D(x, y);
            var pallet = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Pallet,
                PieceId = SelectiveRackDefaults.PalletPieceId,
                BlockName = block,
                View = LateralView,
                Insertion = at,
                ConnectionAnchor = at
            };
            pallet.DynamicParameters[SelectiveRackDefaults.PalletFrenteParam] = fondo; // LONGITUD = fondo in the lateral view
            pallet.DynamicParameters[SelectiveRackDefaults.PalletAltoParam] = alto;
            return pallet;
        }

        /// <summary>The distinct levels attaching at post <paramref name="postIndex"/> from the (up to two) bays it
        /// bounds, deduped by (Y, beam, peralte) — NOT by Y alone: adjacent bays can carry DIFFERENT beams at the same
        /// height, and each distinct one deserves its lateral section.</summary>
        private static IReadOnlyList<SelectiveLevel> CollectLevels(IList<SelectiveBay> bays, int postIndex)
        {
            var byKey = new Dictionary<(double Y, string BeamId, double Peralte), SelectiveLevel>();
            void Collect(int bayIndex)
            {
                if (bayIndex < 0 || bayIndex >= bays.Count)
                {
                    return;
                }

                foreach (var level in bays[bayIndex].Levels)
                {
                    var key = (Math.Round(level.Y, 4), level.BeamId, Math.Round(level.BeamPeralte, 4));
                    if (!byKey.ContainsKey(key))
                    {
                        byKey[key] = level;
                    }
                }
            }

            Collect(postIndex - 1);
            Collect(postIndex);
            return new List<SelectiveLevel>(byKey.Values);
        }

        /// <summary>Clone an instance shifted by <paramref name="dx"/> along X — used to repeat a cabecera at each extra fondo.</summary>
        private static HeaderBlockInstance Translate(HeaderBlockInstance source, double dx)
        {
            var clone = new HeaderBlockInstance
            {
                Role = source.Role,
                PieceId = source.PieceId,
                BlockName = source.BlockName,
                View = source.View,
                RotationRadians = source.RotationRadians,
                MirroredX = source.MirroredX,
                Text = source.Text,
                TextHeight = source.TextHeight,
                ConnectionAnchor = new Point2D(source.ConnectionAnchor.X + dx, source.ConnectionAnchor.Y),
                Insertion = new Point2D(source.Insertion.X + dx, source.Insertion.Y)
            };

            foreach (var pair in source.DynamicParameters)
            {
                clone.DynamicParameters[pair.Key] = pair.Value;
            }

            return clone;
        }

        /// <summary>Lateral-corte text labels (when the toggles are on): a number per level on the left, the post
        /// (frente) number and the rack name at the top of the corte.</summary>
        private static void AddCorteAnnotations(ICollection<HeaderBlockInstance> result, SelectiveRackSystem system, int postIndex, IEnumerable<SelectiveLevel> levels, IList<SelectiveBay> primaryBays, double primaryFallback)
        {
            var h = SelectiveAnnotations.TextHeightFor(system.AnnotationScale);
            var gap = h + SelectiveAnnotations.Margin;

            if (system.NumberLevels)
            {
                var ys = new List<double>();
                foreach (var level in levels)
                {
                    var y = Math.Round(level.Y, 4);
                    if (!ys.Contains(y)) ys.Add(y);
                }
                ys.Sort();

                for (var j = 0; j < ys.Count; j++)
                {
                    result.Add(SelectiveAnnotations.Label(SelectiveAnnotations.Num(j + 1), LateralView, new Point2D(-gap, ys[j]), h));
                }
            }

            var height = SelectivePostGeometry.PostHeight(primaryBays, postIndex, primaryFallback);

            if (system.NumberFronts)
            {
                result.Add(SelectiveAnnotations.Label(SelectiveAnnotations.Num(postIndex + 1), LateralView, new Point2D(0.0, height + gap), h));
            }

            if (system.DrawRackName && !string.IsNullOrWhiteSpace(system.Name))
            {
                result.Add(SelectiveAnnotations.Label(system.Name.Trim(), LateralView, new Point2D(0.0, height + gap + h * 2.0), h * 1.5));
            }
        }

        private static HeaderBlockInstance MakeLarguero(SelectiveLevel level, string block, double x, bool mirrored)
        {
            var at = new Point2D(x, level.Y);
            var beam = new HeaderBlockInstance
            {
                Role = HeaderBlockRole.Beam,
                PieceId = level.BeamId,
                BlockName = block,
                View = LateralView,
                MirroredX = mirrored,
                Insertion = at,
                ConnectionAnchor = at
            };
            beam.DynamicParameters[SelectiveRackDefaults.PeralteParam] = level.BeamPeralte;
            return beam;
        }
    }

    /// <summary>One lateral cross-section of a selective run: the cabecera (frame) at post <see cref="PostIndex"/>, its
    /// X, and the larguero sections that attach there (front + back per level).</summary>
    public sealed class SelectiveCorte
    {
        public SelectiveCorte(int postIndex, double x, RackFrameConfiguration cabecera, IReadOnlyList<HeaderBlockInstance> largueros)
        {
            PostIndex = postIndex;
            X = x;
            Cabecera = cabecera;
            Largueros = largueros ?? new List<HeaderBlockInstance>();
        }

        public int PostIndex { get; }
        public double X { get; }
        public RackFrameConfiguration Cabecera { get; }

        /// <summary>The lateral larguero sections that attach at this frame (front at X=0, back at X=fondo, per level).</summary>
        public IReadOnlyList<HeaderBlockInstance> Largueros { get; }
    }
}
