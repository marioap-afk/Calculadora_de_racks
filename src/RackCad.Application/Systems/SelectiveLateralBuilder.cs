using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.RackFrames;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Builds the LATERAL block plan of a resolved selective run: each of the N+1 posts is drawn as its full
    /// cabecera (2 posts + celosía, via <see cref="LateralHeaderLayoutBuilder"/>) placed at the SAME X as the
    /// frontal post (<see cref="SelectivePostGeometry"/>), so both views line up. Identical cabeceras share one
    /// block definition (same as the dynamic system). Pure.
    ///
    /// The cabecera is a standard frame at the post's resolved height and the run's fondo (`PalletDepth`). The
    /// per-post custom cabeceras (Fase 4) drive the FRONTAL plate today; honouring their celosía/plate here is a
    /// later refinement.
    /// </summary>
    public sealed class SelectiveLateralBuilder
    {
        private readonly LateralHeaderLayoutBuilder headerBuilder = new LateralHeaderLayoutBuilder();
        private readonly RackFrameConfigurationFactory factory = new RackFrameConfigurationFactory();

        public DynamicSystemPlan Build(SelectiveRackSystem system, RackCatalog catalog)
        {
            if (system == null || system.Bays.Count == 0)
            {
                return new DynamicSystemPlan(new List<HeaderGroup>(), new List<HeaderBlockInstance>());
            }

            var postXs = SelectivePostGeometry.Compute(system, catalog).PostXs;
            var depth = system.PalletDepth > 0.0 ? system.PalletDepth : 48.0;
            var template = RackFrameTemplateCatalog.FindById("STD-3P") ?? RackFrameTemplateCatalog.Default;

            var groups = new Dictionary<string, HeaderGroupBuilder>();
            var order = new List<string>();

            for (var i = 0; i < postXs.Count; i++)
            {
                var height = SelectivePostGeometry.PostHeight(system, i);
                if (height <= 0.0)
                {
                    continue;
                }

                var cabecera = factory.Build(template, system.PostId, height, depth);
                var parameters = LateralHeaderParametersFactory.FromConfiguration(cabecera);
                var layout = headerBuilder.Build(cabecera, parameters, catalog);
                var signature = LayoutSignature(layout.Instances);

                if (!groups.TryGetValue(signature, out var group))
                {
                    group = new HeaderGroupBuilder(HeaderName(cabecera), layout.Instances.ToList());
                    groups[signature] = group;
                    order.Add(signature);
                }

                group.Placements.Add(new HeaderPlacement(postXs[i], mirrored: false));
            }

            var headers = order.Select(signature => groups[signature].ToGroup()).ToList();
            return new DynamicSystemPlan(headers, new List<HeaderBlockInstance>());
        }

        private static string HeaderName(RackFrameConfiguration configuration)
            => string.Format(CultureInfo.InvariantCulture, "Cabecera selectivo F{0:0.##} A{1:0.##}", configuration.Depth, configuration.Height);

        /// <summary>Geometry signature: two cabeceras share a block definition only when their drawing matches.</summary>
        private static string LayoutSignature(IReadOnlyList<HeaderBlockInstance> instances)
        {
            var parts = instances.Select(instance => string.Join("|",
                instance.Role,
                instance.BlockName,
                instance.View,
                instance.Insertion.X.ToString("0.###", CultureInfo.InvariantCulture),
                instance.Insertion.Y.ToString("0.###", CultureInfo.InvariantCulture),
                instance.RotationRadians.ToString("0.#####", CultureInfo.InvariantCulture),
                instance.MirroredX,
                string.Join(",", instance.DynamicParameters
                    .OrderBy(p => p.Key, StringComparer.Ordinal)
                    .Select(p => p.Key + "=" + p.Value.ToString("0.###", CultureInfo.InvariantCulture)))));

            return string.Join(";", parts.OrderBy(part => part, StringComparer.Ordinal));
        }

        private sealed class HeaderGroupBuilder
        {
            public HeaderGroupBuilder(string name, IReadOnlyList<HeaderBlockInstance> instances)
            {
                Name = name;
                Instances = instances;
                Placements = new List<HeaderPlacement>();
            }

            public string Name { get; }
            public IReadOnlyList<HeaderBlockInstance> Instances { get; }
            public List<HeaderPlacement> Placements { get; }

            public HeaderGroup ToGroup() => new HeaderGroup(Name, Instances, Placements);
        }
    }
}
