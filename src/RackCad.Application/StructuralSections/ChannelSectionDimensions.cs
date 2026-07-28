namespace RackCad.Application.StructuralSections
{
    /// <summary>
    /// American Standard channel (AISC Type <c>C</c>) measurements, in inches.
    ///
    /// A channel is not symmetric about its y axis, so — unlike a W — AISC tabulates where its centre of
    /// gravity and its shear centre actually are. Those three eccentricities are carried here because they are
    /// GEOMETRY (they say where the section sits relative to its designated edge), and I-36B needs them to
    /// place a contour honestly instead of guessing a centre.
    ///
    /// <c>MC</c> channels are a different AISC type and are deliberately NOT imported.
    /// </summary>
    public sealed class ChannelSectionDimensions : IStructuralSectionDimensions
    {
        public StructuralSectionFamily Family => StructuralSectionFamily.Channel;

        /// <summary>AISC <c>d</c> — overall depth.</summary>
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

        /// <summary>AISC <c>twdet/2</c> — detailing value of half the web thickness.</summary>
        public double? HalfDetailingWebThickness { get; init; }

        /// <summary>AISC <c>tf</c> — flange thickness.</summary>
        public double? FlangeThickness { get; init; }

        /// <summary>AISC <c>tfdet</c> — detailing value of the flange thickness.</summary>
        public double? DetailingFlangeThickness { get; init; }

        /// <summary>AISC <c>kdes</c> — outer face of flange to web toe of fillet, design value.</summary>
        public double? KDesign { get; init; }

        /// <summary>AISC <c>kdet</c> — the same distance, detailing value.</summary>
        public double? KDetailing { get; init; }

        /// <summary>AISC <c>x</c> — designated edge to the CENTRE OF GRAVITY, horizontal.</summary>
        public double? CentroidX { get; init; }

        /// <summary>AISC <c>eo</c> — designated edge to the SHEAR CENTRE, horizontal.</summary>
        public double? ShearCenterX { get; init; }

        /// <summary>AISC <c>xp</c> — designated edge to the PLASTIC NEUTRAL AXIS, horizontal.</summary>
        public double? PlasticNeutralAxisX { get; init; }

        /// <summary>AISC <c>T</c> — distance between the web toes of the fillets.</summary>
        public double? DistanceBetweenFilletToes { get; init; }

        /// <summary>AISC <c>WGi</c> — workable gage of the inner fastener holes in the flange. Often absent.</summary>
        public double? WorkableGageInner { get; init; }
    }
}
