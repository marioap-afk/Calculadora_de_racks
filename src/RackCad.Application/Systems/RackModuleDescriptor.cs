using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// A READ-ONLY projection of one longitudinal module, for an editor that must show and choose modules without
    /// holding the live domain object (I-35).
    /// <para>
    /// The dynamic editor binds <see cref="DynamicRackModule"/> instances straight into its grid and edits them in
    /// place, so every selection change is one keystroke away from mutating the drawn system. A descriptor removes
    /// that coupling: it carries what a UI needs to render a row and identify a module, and nothing a UI could
    /// write through.
    /// </para>
    /// <para>
    /// Deliberately NEUTRAL: it knows nothing about <c>RackSystemKind</c> and contains no branch per system. The
    /// modules of a rack are ONE longitudinal sequence along the depth axis shared by every front and every post
    /// (<see cref="DynamicRackSystem.Modules"/>), so a descriptor identifies a module of the RACK — there is no
    /// per-front or per-post module to describe.
    /// </para>
    /// </summary>
    public sealed class RackModuleDescriptor
    {
        private RackModuleDescriptor(
            string moduleId,
            int index,
            DynamicRackModuleKind kind,
            double length,
            double startX,
            double endX,
            bool isCalculated,
            bool isManualOverride,
            bool usesCalculatedHeaderConfiguration,
            bool hasHeaderConfiguration)
        {
            ModuleId = moduleId;
            Index = index;
            Kind = kind;
            Length = length;
            StartX = startX;
            EndX = endX;
            IsCalculated = isCalculated;
            IsManualOverride = isManualOverride;
            UsesCalculatedHeaderConfiguration = usesCalculatedHeaderConfiguration;
            HasHeaderConfiguration = hasHeaderConfiguration;
        }

        /// <summary>Stable identity of the module inside its rack; the ONLY handle an editor should keep.</summary>
        public string ModuleId { get; }

        /// <summary>Ordinal position in the rack's longitudinal sequence.</summary>
        public int Index { get; }

        public DynamicRackModuleKind Kind { get; }

        /// <summary>Longitudinal length (the header's fondo). Zero for the derived-post entries.</summary>
        public double Length { get; }

        public double StartX { get; }

        public double EndX { get; }

        /// <summary>True while the module still matches the calculated default layout.</summary>
        public bool IsCalculated { get; }

        /// <summary>True when the user overrode the module's length relative to that default.</summary>
        public bool IsManualOverride { get; }

        /// <summary>
        /// Provenance of the cabecera: true while it is still derived from the design inputs, false once the user
        /// customized it. Separators carry the harmless calculated default.
        /// </summary>
        public bool UsesCalculatedHeaderConfiguration { get; }

        public bool HasHeaderConfiguration { get; }

        public bool IsHeader =>
            Kind == DynamicRackModuleKind.HeaderStart
            || Kind == DynamicRackModuleKind.HeaderIntermediate
            || Kind == DynamicRackModuleKind.HeaderEnd;

        /// <summary>True when the module carries longitudinal length; the zero-length derived posts do not.</summary>
        public bool IsLengthBearing => Length > 0.0;

        /// <summary>True when the cabecera of this module is the user's own and a recompute must not regenerate it.</summary>
        public bool HasCustomHeaderConfiguration => IsHeader && HasHeaderConfiguration && !UsesCalculatedHeaderConfiguration;

        /// <summary>Describe one resolved module.</summary>
        public static RackModuleDescriptor From(DynamicRackModule module)
        {
            if (module == null) throw new ArgumentNullException(nameof(module));

            return new RackModuleDescriptor(
                module.ModuleId,
                module.Index,
                module.Kind,
                module.Length,
                module.StartX,
                module.EndX,
                module.IsCalculated,
                module.IsManualOverride,
                module.UseCalculatedHeaderConfiguration,
                module.AssociatedFrameConfiguration != null);
        }

        /// <summary>Describe one editable module intent (no resolved coordinates, so X reads as zero).</summary>
        public static RackModuleDescriptor From(DynamicRackModuleDesign design)
        {
            if (design == null) throw new ArgumentNullException(nameof(design));

            return new RackModuleDescriptor(
                design.ModuleId,
                0,
                design.Kind,
                design.Length,
                0.0,
                0.0,
                design.IsCalculated,
                design.IsManualOverride,
                design.UseCalculatedHeaderConfiguration,
                design.HeaderConfiguration != null);
        }

        /// <summary>Describe every module of a resolved system, in longitudinal order.</summary>
        public static IReadOnlyList<RackModuleDescriptor> Describe(DynamicRackSystem system)
            => (system?.Modules ?? (IList<DynamicRackModule>)Array.Empty<DynamicRackModule>())
                .Where(module => module != null)
                .Select(From)
                .ToList();

        /// <summary>Describe every module intent, in longitudinal order.</summary>
        public static IReadOnlyList<RackModuleDescriptor> Describe(IEnumerable<DynamicRackModuleDesign> designs)
            => (designs ?? Enumerable.Empty<DynamicRackModuleDesign>())
                .Where(design => design != null)
                .Select(From)
                .ToList();
    }
}
