using RackCad.Domain.RackFrames;

namespace RackCad.Domain.Systems
{
    /// <summary>
    /// One editable longitudinal module of a dynamic system. The default view is calculated from the
    /// pallet rule, but each module can be edited (kind, length, associated header configuration).
    /// Intermediate posts are zero-length modules (Length = 0); they are selectable but add no length.
    /// </summary>
    public sealed class DynamicRackModule
    {
        public string ModuleId { get; set; }
        public int Index { get; set; }
        public DynamicRackModuleKind Kind { get; set; }

        /// <summary>Calculated X start (laid out from the module lengths).</summary>
        public double StartX { get; set; }

        /// <summary>Calculated X end.</summary>
        public double EndX { get; set; }

        /// <summary>Editable length along the run. Headers = depth+6, separators = depth, posts = 0.</summary>
        public double Length { get; set; }

        /// <summary>True while the module still matches the calculated default.</summary>
        public bool IsCalculated { get; set; } = true;

        /// <summary>True when the user has overridden the module relative to the default layout.</summary>
        public bool IsManualOverride { get; set; }

        /// <summary>Header configuration for header modules; null for separators and posts.</summary>
        public RackFrameConfiguration AssociatedFrameConfiguration { get; set; }

        public string Notes { get; set; }

        public bool IsHeader =>
            Kind == DynamicRackModuleKind.HeaderStart
            || Kind == DynamicRackModuleKind.HeaderIntermediate
            || Kind == DynamicRackModuleKind.HeaderEnd;
    }
}
