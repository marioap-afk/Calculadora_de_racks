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
    ///   The shortest front owns two modules of length depth+6. Every other module is depth.
    ///   A module is a CABECERA when its distance to the nearest end is even, else a SEPARADOR
    ///   (so the pattern reads cabecera-separador-cabecera-...). The shared envelope is extended from that base
    ///   without forcing a longer front's own ends to be headers. Intermediate posts are NOT modules; they are derived
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
            double headerHeight,
            double postPeralte = 0.0)
            => BuildDefault(
                pallet,
                new DynamicDepthLayout(
                    new DynamicDepthRange(1, palletsDeep),
                    palletsDeep,
                    new[] { new DynamicDepthRange(1, palletsDeep) }),
                headerTemplate,
                headerPostCatalogId,
                headerHeight,
                postPeralte);

        public DynamicRackSystem BuildDefault(
            PalletSpecification pallet,
            DynamicDepthLayout depthLayout,
            RackFrameTemplate headerTemplate,
            string headerPostCatalogId,
            double headerHeight,
            double postPeralte = 0.0)
        {
            if (pallet == null)
            {
                throw new ArgumentNullException(nameof(pallet));
            }

            if (pallet.Depth <= 0.0)
            {
                throw new ArgumentOutOfRangeException(nameof(pallet), "El fondo de tarima debe ser mayor que cero.");
            }

            if (depthLayout == null || depthLayout.TotalPositions < 2)
            {
                throw new ArgumentOutOfRangeException(nameof(depthLayout), "Se requieren al menos 2 tarimas de fondo.");
            }

            var depth = pallet.Depth;
            var template = headerTemplate ?? RackFrameTemplateCatalog.Default;

            var system = new DynamicRackSystem
            {
                Pallet = pallet,
                PalletsDeep = depthLayout.TotalPositions,
                BaseDepthStartPosition = depthLayout.BaseRange.StartPosition,
                BasePalletsDeep = depthLayout.BaseRange.PalletsDeep,
                PostPeralte = postPeralte
            };

            for (var position = 1; position <= depthLayout.TotalPositions; position++)
            {
                var isHeader = depthLayout.IsHeaderPosition(position);
                var length = depth + (depthLayout.IsAllowancePosition(position)
                    ? DynamicRackDefaults.HeaderEndAllowance
                    : 0.0);

                if (isHeader)
                {
                    var kind = position == 1
                        ? DynamicRackModuleKind.HeaderStart
                        : position == depthLayout.TotalPositions
                            ? DynamicRackModuleKind.HeaderEnd
                            : DynamicRackModuleKind.HeaderIntermediate;

                    system.Modules.Add(CreateHeader(
                        kind,
                        length,
                        template,
                        headerPostCatalogId,
                        headerHeight,
                        postPeralte));
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

        /// <summary>
        /// Whether the module at <paramref name="position"/> (1-indexed) is a header in the default
        /// layout. N=2 (the minimum) degenerates to two back-to-back end headers with no separator.
        /// Odd N: strict alternating C-S-C-...-C. Even N: alternating with ONE pair of
        /// consecutive separators (the derived poste) placed as close to the center as possible —
        /// exactly centered for N divisible by 4, one pallet off-center otherwise.
        /// </summary>
        /// <summary>Builds a header configuration for a module at the given depth.</summary>
        public RackFrameConfiguration BuildHeaderConfiguration(
            RackFrameTemplate template,
            string postCatalogId,
            double height,
            double depth,
            double postPeralte = 0.0)
        {
            var header = headerFactory.Build(template ?? RackFrameTemplateCatalog.Default, postCatalogId, height, depth);
            if (postPeralte > 0.0)
            {
                header.PostPeralte = postPeralte;
            }
            memberBuilder.RefreshPhysicalModel(header);
            return header;
        }

        /// <summary>Applies the rack-wide post PERALTE without replacing custom header configurations.</summary>
        public void ApplyPostPeralte(DynamicRackSystem system, double postPeralte)
        {
            if (system == null || postPeralte <= 0.0)
            {
                return;
            }

            system.PostPeralte = postPeralte;
            foreach (var configuration in system.Modules
                         .Where(module => module.IsHeader && module.AssociatedFrameConfiguration != null)
                         .Select(module => module.AssociatedFrameConfiguration))
            {
                configuration.PostPeralte = postPeralte;
                memberBuilder.RefreshPhysicalModel(configuration);
            }
        }

        private DynamicRackModule CreateHeader(
            DynamicRackModuleKind kind,
            double length,
            RackFrameTemplate template,
            string postCatalogId,
            double headerHeight,
            double postPeralte)
        {
            return new DynamicRackModule
            {
                Kind = kind,
                Length = length,
                IsCalculated = true,
                UseCalculatedHeaderConfiguration = true,
                AssociatedFrameConfiguration = BuildHeaderConfiguration(
                    template,
                    postCatalogId,
                    headerHeight,
                    length,
                    postPeralte)
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
