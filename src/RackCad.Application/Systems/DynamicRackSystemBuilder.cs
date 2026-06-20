using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.RackFrames;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Builds the default editable dynamic system and keeps it consistent after edits. The default
    /// layout follows the pallet rule (Total = N x depth + 12"; first/last = depth+6; interior =
    /// depth; even N gets a zero-length center post). Header modules get their own header
    /// configuration built through the existing <see cref="RackFrameConfigurationFactory"/> (reusing
    /// the header logic without coupling to its window). Pure logic, no UI/AutoCAD.
    /// </summary>
    public sealed class DynamicRackSystemBuilder
    {
        private readonly RackFrameConfigurationFactory headerFactory;
        private readonly IIntermediatePostRule postRule;
        private readonly BracingPanelMemberBuilder memberBuilder = new BracingPanelMemberBuilder();

        public DynamicRackSystemBuilder()
            : this(new RackFrameConfigurationFactory(), new CenterWhenEvenRule())
        {
        }

        public DynamicRackSystemBuilder(RackCatalog catalog)
            : this(new RackFrameConfigurationFactory(catalog), new CenterWhenEvenRule())
        {
        }

        public DynamicRackSystemBuilder(RackFrameConfigurationFactory headerFactory, IIntermediatePostRule postRule)
        {
            this.headerFactory = headerFactory ?? new RackFrameConfigurationFactory();
            this.postRule = postRule ?? new CenterWhenEvenRule();
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
            var headerLength = depth + DynamicRackDefaults.HeaderEndAllowance;
            var template = headerTemplate ?? RackFrameTemplateCatalog.Default;

            var system = new DynamicRackSystem { Pallet = pallet, PalletsDeep = palletsDeep };

            system.Modules.Add(CreateHeader(DynamicRackModuleKind.HeaderStart, headerLength, template, headerPostCatalogId, headerHeight));

            for (var i = 0; i < palletsDeep - 2; i++)
            {
                system.Modules.Add(CreateSeparator(depth));
            }

            system.Modules.Add(CreateHeader(DynamicRackModuleKind.HeaderEnd, headerLength, template, headerPostCatalogId, headerHeight));

            system.RecalculatePositions();
            InsertIntermediatePosts(system);
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

        private void InsertIntermediatePosts(DynamicRackSystem system)
        {
            var lengthModules = system.Modules.Where(m => m.Length > 0.0).ToList();
            var offsets = postRule.ResolvePostOffsets(system.PalletsDeep, lengthModules) ?? Array.Empty<double>();

            foreach (var offset in offsets.OrderByDescending(o => o))
            {
                var afterIndex = system.Modules
                    .Select((module, index) => (module, index))
                    .Where(item => item.module.Length > 0.0 && Math.Abs(item.module.EndX - offset) <= 1e-6)
                    .Select(item => (int?)item.index)
                    .FirstOrDefault();

                if (afterIndex.HasValue)
                {
                    system.Modules.Insert(afterIndex.Value + 1, CreatePost());
                }
            }
        }

        private DynamicRackModule CreateHeader(
            DynamicRackModuleKind kind,
            double length,
            RackFrameTemplate template,
            string postCatalogId,
            double headerHeight)
        {
            var header = headerFactory.Build(template, postCatalogId, headerHeight, length);
            memberBuilder.RefreshPhysicalModel(header);

            return new DynamicRackModule
            {
                Kind = kind,
                Length = length,
                IsCalculated = true,
                AssociatedFrameConfiguration = header
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

        private static DynamicRackModule CreatePost()
        {
            return new DynamicRackModule
            {
                Kind = DynamicRackModuleKind.IntermediatePost,
                Length = 0.0,
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
