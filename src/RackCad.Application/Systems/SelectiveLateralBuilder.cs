using System.Collections.Generic;
using RackCad.Application.Catalogs;
using RackCad.Application.RackFrames;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Turns a resolved selective run into its LATERAL "cortes": one cabecera (cross-section) per post, at the
    /// post's resolved height and the run's fondo (<see cref="SelectiveRackSystem.PalletDepth"/>), positioned at
    /// the SAME X as the frontal post (<see cref="SelectivePostGeometry"/>). Each corte is an INDEPENDENT
    /// cabecera — the AutoCAD side draws each as its own cabecera block, so they can be edited one by one. Pure.
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
                var height = SelectivePostGeometry.PostHeight(system, i);
                if (height <= 0.0)
                {
                    continue;
                }

                var cabecera = factory.Build(template, system.PostId, height, depth);
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
