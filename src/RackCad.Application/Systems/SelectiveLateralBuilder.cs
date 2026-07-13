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
