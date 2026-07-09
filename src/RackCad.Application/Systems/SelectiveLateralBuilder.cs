using System.Collections.Generic;
using RackCad.Application.Catalogs;
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

                cortes.Add(new SelectiveCorte(i, postXs[i], cabecera));
            }

            return cortes;
        }
    }

    /// <summary>One lateral cross-section of a selective run: the cabecera (frame) at post <see cref="PostIndex"/> and its X.</summary>
    public sealed class SelectiveCorte
    {
        public SelectiveCorte(int postIndex, double x, RackFrameConfiguration cabecera)
        {
            PostIndex = postIndex;
            X = x;
            Cabecera = cabecera;
        }

        public int PostIndex { get; }
        public double X { get; }
        public RackFrameConfiguration Cabecera { get; }
    }
}
