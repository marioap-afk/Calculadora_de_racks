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
    /// Turns a resolved selective run into its LATERAL sections: one cross-section (a cabecera frame) per post, at the
    /// post's resolved height and the run's fondo (<see cref="SelectiveRackSystem.PalletDepth"/>), positioned at the
    /// SAME X as the frontal post (<see cref="SelectivePostGeometry"/>). The AutoCAD side draws each section as its own
    /// block, but every section is a view OF the system (shares the rack id/design), so editing the system redraws them
    /// all. TODO: the section still shows only the cabecera; the largueros (and later safety elements) are pending. Pure.
    /// </summary>
    public sealed class SelectiveLateralBuilder
    {
        /// <summary>View the lateral corte draws in (posts, celosía and largueros share it). See views.csv / blocks.csv.</summary>
        public const string LateralView = "LATERAL";

        private readonly RackFrameConfigurationFactory factory = new RackFrameConfigurationFactory();

        public IReadOnlyList<SelectiveCorte> Cortes(SelectiveRackSystem system, RackCatalog catalog)
        {
            var cortes = new List<SelectiveCorte>();
            if (system == null || system.Bays.Count == 0)
            {
                return cortes;
            }

            var postXs = SelectivePostGeometry.Compute(system, catalog).PostXs;
            var depth = system.PalletDepth > 0.0 ? system.PalletDepth : 48.0;
            var template = RackFrameTemplateCatalog.FindById("STD-3P") ?? RackFrameTemplateCatalog.Default;

            for (var i = 0; i < postXs.Count; i++)
            {
                // If the user customized this post's cabecera, the corte IS that cabecera (its height + celosía +
                // plate), so editing it shows in the lateral. Otherwise build a standard frame at the resolved height.
                // Fondo is shared across the run (enforced when the custom is stored).
                var custom = i < system.PostCabeceras.Count ? system.PostCabeceras[i] : null;
                RackFrameConfiguration cabecera;
                if (custom != null && custom.Height > 0.0)
                {
                    cabecera = custom;
                }
                else
                {
                    var height = SelectivePostGeometry.PostHeight(system, i);
                    if (height <= 0.0)
                    {
                        continue;
                    }

                    cabecera = factory.Build(template, system.PostId, height, depth);
                }

                var largueros = BuildLargueros(system, i, depth, catalog);
                cortes.Add(new SelectiveCorte(i, postXs[i], cabecera, largueros));
            }

            return cortes;
        }

        /// <summary>
        /// The lateral larguero sections that attach at frame <paramref name="postIndex"/>: for each level of the
        /// adjacent bays (deduped by Y), a FRONT section at the front post (X=0) and a BACK section at the back post
        /// (X=fondo), at the SAME Y as the frontal (the beam's lateral block origin is its mate, so no offset). The
        /// beam PERALTE carries over. In a lateral view the beam is seen end-on, so LONGITUD is irrelevant.
        /// </summary>
        private static IReadOnlyList<HeaderBlockInstance> BuildLargueros(SelectiveRackSystem system, int postIndex, double depth, RackCatalog catalog)
        {
            var result = new List<HeaderBlockInstance>();

            // Beams physically attaching at this frame come from the bays on either side (an interior frame joins two).
            var byY = new Dictionary<double, SelectiveLevel>();
            void Collect(int bayIndex)
            {
                if (bayIndex < 0 || bayIndex >= system.Bays.Count)
                {
                    return;
                }

                foreach (var level in system.Bays[bayIndex].Levels)
                {
                    var key = Math.Round(level.Y, 4);
                    if (!byY.ContainsKey(key))
                    {
                        byY[key] = level;
                    }
                }
            }

            Collect(postIndex - 1);
            Collect(postIndex);

            foreach (var level in byY.Values)
            {
                var block = catalog?.Blocks.FindBlock(level.BeamId, LateralView)?.BlockName;
                result.Add(MakeLarguero(level, block, x: 0.0, mirrored: false));      // front post
                result.Add(MakeLarguero(level, block, x: depth, mirrored: true));     // back post
            }

            return result;
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
