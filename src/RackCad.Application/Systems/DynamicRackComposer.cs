using System;
using System.Collections.Generic;
using RackCad.Application.Geometry;
using RackCad.Application.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Composes a <see cref="DynamicRackSystem"/> into positioned modules: it enforces the
    /// system-driven header depth, generates the header's physical members (reusing the existing
    /// builder), derives the layout, and assigns each module a <see cref="Placement2D"/> at its
    /// start offset. The two header modules share the same header instance (symmetric).
    /// </summary>
    public sealed class DynamicRackComposer
    {
        private readonly DynamicRackLayoutGenerator generator;
        private readonly BracingPanelMemberBuilder memberBuilder = new BracingPanelMemberBuilder();

        public DynamicRackComposer()
            : this(new DynamicRackLayoutGenerator())
        {
        }

        public DynamicRackComposer(DynamicRackLayoutGenerator generator)
        {
            this.generator = generator ?? new DynamicRackLayoutGenerator();
        }

        public ComposedDynamicRack Compose(DynamicRackSystem system)
        {
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            if (system.Header == null)
            {
                throw new InvalidOperationException("El sistema dinamico no tiene cabecera.");
            }

            var effectiveDepth = system.EffectiveHeaderDepth;

            // The system drives the header's depth, then the real members are (re)generated.
            system.Header.Depth = effectiveDepth;
            memberBuilder.RefreshPhysicalModel(system.Header);

            var layout = generator.Generate(system.Pallet, system.PalletsDeep, effectiveDepth);

            var placed = new List<PlacedModule>(layout.Modules.Count);
            foreach (var module in layout.Modules)
            {
                var placement = new Placement2D(module.StartOffset, 0.0);
                var isHeader = module.Kind == RackModuleKind.HeaderStart || module.Kind == RackModuleKind.HeaderEnd;
                placed.Add(new PlacedModule(module, placement, isHeader ? system.Header : null));
            }

            return new ComposedDynamicRack(system, layout, placed);
        }
    }
}
