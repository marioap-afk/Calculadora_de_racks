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

            var postXs = SelectivePostGeometry.Compute(system, catalog).PostXs;
            var depth = system.PalletDepth > 0.0 ? system.PalletDepth : SelectiveRackDefaults.DefaultPalletDepth;
            var offsets = SelectiveDepthLayout.Offsets(system, depth); // one X per fondo (doble profundidad)
            var template = RackFrameTemplateCatalog.FindStandardOrDefault();

            // Each fondo has its OWN bays (own levels/heights). Resolve them + a per-fondo height fallback once.
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
                // fondo 0's cabecera is drawn from `cabecera` (at X=0) by the AutoCAD side. A per-post custom cabecera
                // wins for fondo 0 (its height + celosía + plate). Otherwise a standard frame at fondo 0's own height.
                var custom = i < system.PostCabeceras.Count ? system.PostCabeceras[i] : null;
                RackFrameConfiguration cabecera;
                if (custom != null && custom.Height > 0.0)
                {
                    cabecera = custom;
                }
                else
                {
                    var height0 = SelectivePostGeometry.PostHeight(fondoBays[0], i, fondoFallback[0]);
                    if (height0 <= 0.0)
                    {
                        continue;
                    }

                    cabecera = factory.Build(template, system.PostId, height0, depth);
                }

                var extras = new List<HeaderBlockInstance>();

                // Extra fondos (doble profundidad): each is its OWN cabecera at ITS OWN height — a side can be taller
                // or shorter — translated to its X offset. Fase 1 uses the standard frame per extra fondo (per-post
                // custom cabeceras stay on fondo 0).
                for (var k = 1; k < offsets.Count; k++)
                {
                    var heightK = SelectivePostGeometry.PostHeight(fondoBays[k], i, fondoFallback[k]);
                    if (heightK <= 0.0)
                    {
                        continue;
                    }

                    AddCabeceraAtOffset(extras, factory.Build(template, system.PostId, heightK, depth), offsets[k], catalog);
                }

                // Largueros: every fondo contributes its OWN levels (its own heights) at its own X offset.
                for (var k = 0; k < offsets.Count; k++)
                {
                    BuildLargueros(extras, fondoBays[k], i, depth, offsets[k], catalog);
                }

                // Annotations once, from fondo 0's levels (the primary face) and fondo 0's height.
                AddCorteAnnotations(extras, system, i, CollectLevels(fondoBays[0], i));
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
        private static void AddCorteAnnotations(ICollection<HeaderBlockInstance> result, SelectiveRackSystem system, int postIndex, IEnumerable<SelectiveLevel> levels)
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

            var height = SelectivePostGeometry.PostHeight(system, postIndex);

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
