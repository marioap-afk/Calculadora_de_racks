using System;
using RackCad.Application.Catalogs;
using RackCad.Application.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Builds a <see cref="DynamicRackSystem"/>, creating its header through the existing
    /// <see cref="RackFrameConfigurationFactory"/> with the system-driven depth
    /// (override, or pallet depth + 6"). This is where the dynamic system reuses the header
    /// component instead of duplicating it.
    /// </summary>
    public sealed class DynamicRackSystemFactory
    {
        private readonly RackFrameConfigurationFactory headerFactory;

        public DynamicRackSystemFactory()
            : this(new RackFrameConfigurationFactory())
        {
        }

        public DynamicRackSystemFactory(RackCatalog catalog)
            : this(new RackFrameConfigurationFactory(catalog))
        {
        }

        public DynamicRackSystemFactory(RackFrameConfigurationFactory headerFactory)
        {
            this.headerFactory = headerFactory ?? new RackFrameConfigurationFactory();
        }

        public DynamicRackSystem Create(
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

            // Header depth is fully pallet-driven: pallet depth + 6" (= the first/last module length).
            var headerDepth = pallet.Depth + DynamicRackDefaults.HeaderEndAllowance;

            var header = headerFactory.Build(
                headerTemplate ?? RackFrameTemplateCatalog.Default,
                headerPostCatalogId,
                headerHeight,
                headerDepth);

            return new DynamicRackSystem
            {
                Kind = RackSystemKind.PalletFlow,
                Pallet = pallet,
                PalletsDeep = palletsDeep,
                Header = header
            };
        }
    }
}
