using System.Collections.Generic;
using RackCad.Application.Geometry;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>One layout module positioned in model space, with its header component for header modules.</summary>
    public sealed class PlacedModule
    {
        public PlacedModule(RackModule module, Placement2D placement, RackFrameConfiguration header)
        {
            Module = module;
            Placement = placement;
            Header = header;
        }

        public RackModule Module { get; }

        /// <summary>Where the module's local origin sits in model space (offset = module start, elevation 0).</summary>
        public Placement2D Placement { get; }

        /// <summary>The end-frame configuration for header modules; null for separators and posts.</summary>
        public RackFrameConfiguration Header { get; }

        public bool IsHeader => Header != null;
    }

    /// <summary>
    /// A dynamic system composed into positioned modules: the derived layout plus, for each module,
    /// its placement and (for headers) the real header configuration. No AutoCAD, no UI.
    /// </summary>
    public sealed class ComposedDynamicRack
    {
        public ComposedDynamicRack(DynamicRackSystem system, DynamicRackLayout layout, IReadOnlyList<PlacedModule> placedModules)
        {
            System = system;
            Layout = layout;
            PlacedModules = placedModules;
        }

        public DynamicRackSystem System { get; }
        public DynamicRackLayout Layout { get; }
        public IReadOnlyList<PlacedModule> PlacedModules { get; }
    }
}
