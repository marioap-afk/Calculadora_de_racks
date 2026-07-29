namespace RackCad.Application.StructuralSections
{
    /// <summary>
    /// American Standard beam (AISC Type <c>S</c>, locally IPS) measurements, in inches.
    ///
    /// The column set is the same one AISC publishes for <see cref="WSectionDimensions"/>, and this is
    /// deliberately a SEPARATE type rather than an alias, a base class or a reuse of it (I-36D decision 6).
    /// Sharing the storage would make the two families interchangeable at every call site that pattern-matches
    /// on the concrete dimensions — and they are not interchangeable, because an S has SLOPED inner flange
    /// faces and a W does not. The type is where that distinction survives refactors.
    ///
    /// What the source does NOT publish for these 28 shapes, measured and not assumed (see the I-36D
    /// evidence): <c>k1</c> and <c>WGo</c> are empty in all 28 rows, and <c>WGi</c> in the 7 smallest. They
    /// are <c>null</c> here, never zero. Above all, AISC publishes NO flange slope and NO explicit radius:
    /// the taper this family is drawn with is a RackCad convention declared in ADR-0023, and it lives in the
    /// geometry builder, not in this type. Nothing here is derived — these are copied values.
    /// </summary>
    public sealed class SSectionDimensions : IStructuralSectionDimensions
    {
        public StructuralSectionFamily Family => StructuralSectionFamily.S;

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

        /// <summary>
        /// AISC <c>tf</c> — flange thickness.
        ///
        /// The Readme defines it as "thickness of flange" and does NOT say where on a sloped flange it is
        /// measured. That gap is real and it is not resolved here: this property is the published number.
        /// ADR-0023 chooses to READ it as the mean thickness of the free overhang, and that reading is scoped
        /// to the visual representation alone.
        /// </summary>
        public double? FlangeThickness { get; init; }

        /// <summary>AISC <c>tfdet</c> — detailing value of the flange thickness.</summary>
        public double? DetailingFlangeThickness { get; init; }

        /// <summary>AISC <c>kdes</c> — outer face of flange to web toe of fillet, design value.</summary>
        public double? KDesign { get; init; }

        /// <summary>AISC <c>kdet</c> — the same distance, detailing value.</summary>
        public double? KDetailing { get; init; }

        /// <summary>AISC <c>k1</c> — web centre line to flange toe of fillet. EMPTY in all 28 S shapes.</summary>
        public double? K1 { get; init; }

        /// <summary>
        /// AISC <c>T</c> — distance between the web toes of the fillets.
        ///
        /// A DISTANCE, never a radius: the Readme says so literally. Over the 28 S shapes it agrees with
        /// <c>d - 2·kdet</c> in 26 rows and with <c>d - 2·kdes</c> in only 14, which is why the drawing anchors
        /// on <see cref="KDesign"/> and uses this value as a check.
        /// </summary>
        public double? DistanceBetweenFilletToes { get; init; }

        /// <summary>AISC <c>WGi</c> — workable gage of the inner flange holes. Absent in the 7 smallest S.</summary>
        public double? WorkableGageInner { get; init; }

        /// <summary>AISC <c>WGo</c> — outer gage. EMPTY in all 28 S shapes.</summary>
        public double? WorkableGageOuter { get; init; }
    }
}
