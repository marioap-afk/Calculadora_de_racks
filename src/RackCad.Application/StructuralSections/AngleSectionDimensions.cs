namespace RackCad.Application.StructuralSections
{
    /// <summary>
    /// Single angle (AISC Type <c>L</c>) measurements, in inches.
    ///
    /// The leg naming follows the source's own definitions, verified against the data and not assumed: AISC
    /// puts the SHORTER leg in column <c>d</c> and the LONGER leg in column <c>b</c>. So <c>L8X6X1</c> — whose
    /// manual label names the long leg first — tabulates <c>d = 6</c> and <c>b = 8</c>. Naming the properties
    /// <see cref="ShortLeg"/>/<see cref="LongLeg"/> instead of <c>D</c>/<c>B</c> is what keeps a future
    /// consumer from silently swapping them.
    ///
    /// Double angles (<c>2L</c>) are a different AISC type and are deliberately NOT imported: a 2L row
    /// describes an ASSEMBLY of two angles at a given back-to-back spacing, which is a member decision, not a
    /// cross section (ADR-0020).
    /// </summary>
    public sealed class AngleSectionDimensions : IStructuralSectionDimensions
    {
        public StructuralSectionFamily Family => StructuralSectionFamily.Angle;

        /// <summary>AISC <c>d</c> — width of the SHORTER leg.</summary>
        public double? ShortLeg { get; init; }

        /// <summary>AISC <c>b</c> — width of the LONGER leg.</summary>
        public double? LongLeg { get; init; }

        /// <summary>AISC <c>t</c> — leg thickness (both legs share it).</summary>
        public double? Thickness { get; init; }

        /// <summary>AISC <c>kdes</c> — design value of the fillet distance.</summary>
        public double? KDesign { get; init; }

        /// <summary>AISC <c>kdet</c> — detailing value of the fillet distance.</summary>
        public double? KDetailing { get; init; }

        /// <summary>AISC <c>x</c> — designated edge to the CENTRE OF GRAVITY, horizontal.</summary>
        public double? CentroidX { get; init; }

        /// <summary>AISC <c>y</c> — designated edge to the CENTRE OF GRAVITY, vertical.</summary>
        public double? CentroidY { get; init; }

        /// <summary>AISC <c>xp</c> — designated edge to the PLASTIC NEUTRAL AXIS, horizontal.</summary>
        public double? PlasticNeutralAxisX { get; init; }

        /// <summary>AISC <c>yp</c> — designated edge to the PLASTIC NEUTRAL AXIS, vertical.</summary>
        public double? PlasticNeutralAxisY { get; init; }

        /// <summary>True when both legs are equal (an equal-leg angle).</summary>
        public bool IsEqualLeg =>
            ShortLeg.HasValue && LongLeg.HasValue && ShortLeg.Value == LongLeg.Value;
    }
}
