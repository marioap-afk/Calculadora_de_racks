using System;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.RackFrames;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Builds the default editable dynamic system and keeps it consistent after edits.
    ///
    /// Default layout (alternating headers/separators):
    ///   N modules. The two ends are headers of length depth+6. Every other module is depth.
    ///   A module is a CABECERA when its distance to the nearest end is even, else a SEPARADOR
    ///   (so the pattern reads cabecera-separador-cabecera-...). Total = N x depth + 12" always
    ///   (only the two ends carry the +6). Intermediate posts are NOT modules; they are derived
    ///   where two separators are consecutive (only happens for some even N).
    ///
    /// Header modules get their own header configuration built through the existing
    /// <see cref="RackFrameConfigurationFactory"/> at the module's length (depth+6 for ends, depth
    /// for interior headers). Pure logic, no UI/AutoCAD.
    /// </summary>
    public sealed class DynamicRackSystemBuilder
    {
        private readonly RackFrameConfigurationFactory headerFactory;
        private readonly BracingPanelMemberBuilder memberBuilder = new BracingPanelMemberBuilder();

        public DynamicRackSystemBuilder()
            : this(new RackFrameConfigurationFactory())
        {
        }

        public DynamicRackSystemBuilder(RackCatalog catalog)
            : this(new RackFrameConfigurationFactory(catalog))
        {
        }

        public DynamicRackSystemBuilder(RackFrameConfigurationFactory headerFactory)
        {
            this.headerFactory = headerFactory ?? new RackFrameConfigurationFactory();
        }

        public DynamicRackSystem BuildDefault(
            PalletSpecification pallet,
            int palletsDeep,
            RackFrameTemplate headerTemplate,
            string headerPostCatalogId,
            double headerHeight)
        {
            if (pallet == null)
            {
                throw new ArgumentNullException(nameof(pallet));
            }

            if (pallet.Depth <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(pallet), "El fondo de tarima debe ser mayor que cero.");
            }

            if (palletsDeep < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(palletsDeep), "Se requieren al menos 2 tarimas de fondo.");
            }

            var depth = pallet.Depth;
            var endLength = depth + DynamicRackDefaults.HeaderEndAllowance;
            var template = headerTemplate ?? RackFrameTemplateCatalog.Default;

            var system = new DynamicRackSystem { Pallet = pallet, PalletsDeep = palletsDeep };

            for (var position = 1; position <= palletsDeep; position++)
            {
                var isEnd = position == 1 || position == palletsDeep;
                var distanceToNearestEnd = Math.Min(position - 1, palletsDeep - position);
                var isHeader = distanceToNearestEnd % 2 == 0;
                var length = isEnd ? endLength : depth;

                if (isHeader)
                {
                    var kind = position == 1
                        ? DynamicRackModuleKind.HeaderStart
                        : position == palletsDeep
                            ? DynamicRackModuleKind.HeaderEnd
                            : DynamicRackModuleKind.HeaderIntermediate;

                    system.Modules.Add(CreateHeader(kind, length, template, headerPostCatalogId, headerHeight));
                }
                else
                {
                    system.Modules.Add(CreateSeparator(length));
                }
            }

            system.RecalculatePositions();
            AssignIds(system);
            return system;
        }

        /// <summary>
        /// Re-syncs the system after edits: each header module's configuration depth is set to the
        /// module length, its physical members are regenerated, and positions are recalculated.
        /// </summary>
        public void Refresh(DynamicRackSystem system)
        {
            if (system == null)
            {
                return;
            }

            foreach (var module in system.Modules.Where(m => m.IsHeader && m.AssociatedFrameConfiguration != null))
            {
                module.AssociatedFrameConfiguration.Depth = module.Length;
                memberBuilder.RefreshPhysicalModel(module.AssociatedFrameConfiguration);
            }

            system.RecalculatePositions();
            AssignIds(system);
        }

        /// <summary>Builds a header configuration for a module at the given depth.</summary>
        public RackFrameConfiguration BuildHeaderConfiguration(RackFrameTemplate template, string postCatalogId, double height, double depth)
        {
            var header = headerFactory.Build(template ?? RackFrameTemplateCatalog.Default, postCatalogId, height, depth);
            memberBuilder.RefreshPhysicalModel(header);
            return header;
        }

        private DynamicRackModule CreateHeader(
            DynamicRackModuleKind kind,
            double length,
            RackFrameTemplate template,
            string postCatalogId,
            double headerHeight)
        {
            return new DynamicRackModule
            {
                Kind = kind,
                Length = length,
                IsCalculated = true,
                AssociatedFrameConfiguration = BuildHeaderConfiguration(template, postCatalogId, headerHeight, length)
            };
        }

        private static DynamicRackModule CreateSeparator(double length)
        {
            return new DynamicRackModule
            {
                Kind = DynamicRackModuleKind.Separator,
                Length = length,
                IsCalculated = true
            };
        }

        private static void AssignIds(DynamicRackSystem system)
        {
            for (var i = 0; i < system.Modules.Count; i++)
            {
                var module = system.Modules[i];
                module.Index = i;

                if (string.IsNullOrWhiteSpace(module.ModuleId))
                {
                    module.ModuleId = "M" + (i + 1).ToString(CultureInfo.InvariantCulture);
                }
            }
        }
    }
}
