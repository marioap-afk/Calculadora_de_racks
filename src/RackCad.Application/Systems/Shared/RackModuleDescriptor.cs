using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.Dynamic;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Systems.Shared
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
            bool hasHeaderConfiguration,
            bool isPhysicallyPresent)
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
            IsPhysicallyPresent = isPhysicallyPresent;
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

        /// <summary>
        /// Whether the module has a physical assembly anywhere in the rack. A module is drawn at the POSTS whose depth
        /// range covers its position, and I-33 suppresses the boundary shared by two blank fronts — so a module that
        /// only ever appeared at suppressed posts exists LOGICALLY (index, length, coordinates) but is drawn nowhere,
        /// and an editor must refuse to edit it and say why. Always true when described from an intent, which carries
        /// no resolved geometry.
        /// </summary>
        public bool IsPhysicallyPresent { get; }

        /// <summary>Describe one resolved module, assuming it is drawn somewhere. Prefer
        /// <see cref="Describe(DynamicRackSystem)"/>, which resolves physical presence against the rack.</summary>
        public static RackModuleDescriptor From(DynamicRackModule module) => From(module, true);

        private static RackModuleDescriptor From(DynamicRackModule module, bool isPhysicallyPresent)
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
                module.AssociatedFrameConfiguration != null,
                isPhysicallyPresent);
        }

        /// <summary>Describe one editable module intent, standalone (no position in a sequence, so the ordinal
        /// reads as zero). Prefer <see cref="Describe(IEnumerable{DynamicRackModuleDesign})"/>, which numbers each
        /// intent by its place in the longitudinal sequence.</summary>
        public static RackModuleDescriptor From(DynamicRackModuleDesign design) => From(design, 0);

        /// <summary>
        /// Describe one editable module intent at its ORDINAL in the sequence (no resolved coordinates, so X reads
        /// as zero).
        /// <para>
        /// The ordinal has to be supplied because an intent, unlike a resolved <see cref="DynamicRackModule"/>,
        /// carries no <c>Index</c> of its own. Numbering every intent 1 is not a cosmetic detail: the module
        /// selector and the «Copiar de:» list of Push Back are labelled from it, and a list where every cabecera
        /// reads «1. Cabecera» cannot be used to pick one (I-40).
        /// </para>
        /// </summary>
        public static RackModuleDescriptor From(DynamicRackModuleDesign design, int index)
        {
            if (design == null) throw new ArgumentNullException(nameof(design));

            return new RackModuleDescriptor(
                design.ModuleId,
                index,
                design.Kind,
                design.Length,
                0.0,
                0.0,
                design.IsCalculated,
                design.IsManualOverride,
                design.UseCalculatedHeaderConfiguration,
                design.HeaderConfiguration != null,
                true);
        }

        /// <summary>
        /// Describe every module of a resolved system, in longitudinal order, resolving each module's PHYSICAL
        /// presence against the boundaries I-33 says exist: a module is present when at least one surviving post's
        /// depth range covers it.
        /// </summary>
        public static IReadOnlyList<RackModuleDescriptor> Describe(DynamicRackSystem system)
        {
            if (system == null)
            {
                return Array.Empty<RackModuleDescriptor>();
            }

            var present = PresentModulePositions(system);
            return system.Modules
                .Where(module => module != null)
                .Select(module => From(module, present.Contains(module.Index + 1)))
                .ToList();
        }

        /// <summary>
        /// The longitudinal positions actually covered by a surviving post. Reuses I-33's authority
        /// (<see cref="DynamicFrontActivation.PresentBoundaries"/>) and the depth range of each post, so the editor
        /// and the drawing cannot disagree about what exists.
        /// </summary>
        private static HashSet<int> PresentModulePositions(DynamicRackSystem system)
        {
            var positions = new HashSet<int>();
            foreach (var post in DynamicFrontActivation.PresentBoundaries(system))
            {
                var range = DynamicDepthGeometry.AtPost(system, post);
                foreach (var module in DynamicDepthGeometry.ModulesInRange(system, range))
                {
                    positions.Add(module.Index + 1);
                }
            }

            return positions;
        }

        /// <summary>Describe every module intent, in longitudinal order and NUMBERED by that order.</summary>
        public static IReadOnlyList<RackModuleDescriptor> Describe(IEnumerable<DynamicRackModuleDesign> designs)
            => (designs ?? Enumerable.Empty<DynamicRackModuleDesign>())
                .Where(design => design != null)
                .Select((design, index) => From(design, index))
                .ToList();
    }
}
