namespace RackCad.Application.StructuralSections
{
    /// <summary>
    /// Wide-flange (AISC Type <c>W</c>) measurements, in inches.
    ///
    /// AISC publishes each thickness twice: a DESIGN value (the rounded number used in calculations, e.g.
    /// <c>tw</c>) and a DETAILING value (the fabrication-friendly fraction, e.g. <c>twdet</c>). They are
    /// different data and both are kept; collapsing them would lose information neither can reconstruct.
    /// I-36B will decide which one a drawn contour uses — I-36A only preserves them.
    /// </summary>
    public sealed class WSectionDimensions : IStructuralSectionDimensions
    {
        public StructuralSectionFamily Family => StructuralSectionFamily.W;

        /// <summary>AISC <c>d</c> — overall depth of the member.</summary>
        public double? Depth { get; init; }

        /// <summary>AISC <c>ddet</c> — detailing value of the depth.</summary>
        public double? DetailingDepth { get; init; }

        /// <summary>AISC <c>bf</c> — flange width.</summary>
        public double? FlangeWidth { get; init; }

        /// <summary>AISC <c>bfdet</c> — detailing value of the flange width.</summary>
        public double? DetailingFlangeWidth { get; init; }

        /// <summary>AISC <c>tw</c> — web thickness.</summary>
        public double? WebThickness { get; init; }

        /// <summary>AISC <c>twdet</c> — detailing value of the web thickness.</summary>
        public double? DetailingWebThickness { get; init; }

        /// <summary>AISC <c>twdet/2</c> — detailing value of half the web thickness (tabulated, not derived here).</summary>
        public double? HalfDetailingWebThickness { get; init; }

        /// <summary>AISC <c>tf</c> — flange thickness.</summary>
        public double? FlangeThickness { get; init; }

        /// <summary>AISC <c>tfdet</c> — detailing value of the flange thickness.</summary>
        public double? DetailingFlangeThickness { get; init; }

        /// <summary>AISC <c>kdes</c> — outer face of flange to web toe of fillet, design value.</summary>
        public double? KDesign { get; init; }

        /// <summary>AISC <c>kdet</c> — the same distance, detailing value.</summary>
        public double? KDetailing { get; init; }

        /// <summary>AISC <c>k1</c> — web centre line to flange toe of fillet, detailing value.</summary>
        public double? K1 { get; init; }

        /// <summary>AISC <c>T</c> — distance between the web toes of the fillets.</summary>
        public double? DistanceBetweenFilletToes { get; init; }

        /// <summary>AISC <c>WGi</c> — workable gage of the inner fastener holes in the flange.</summary>
        public double? WorkableGageInner { get; init; }

        /// <summary>AISC <c>WGo</c> — spacing between inner and outer holes when four holes fit. Often absent.</summary>
        public double? WorkableGageOuter { get; init; }
    }
}
