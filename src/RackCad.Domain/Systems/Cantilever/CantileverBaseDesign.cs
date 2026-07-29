namespace RackCad.Domain.Systems.Cantilever
{
    /// <summary>
    /// Editable intent of a Cantilever BASE: the catalogued section it is made of, how far it projects, its
    /// two end plates and its gusset.
    ///
    /// <see cref="SectionId"/> is TEXT for the same reason as the column's, and it may be a DIFFERENT
    /// section: the two are resolved independently and only their COMBINATION is validated.
    ///
    /// Neither the rear plate's height nor the gusset's legs live here. Both are DERIVED from where the
    /// connection punches fall, which in turn depends on the height of the base section envelope and on the
    /// transverse extent of the column. Storing them would be storing an answer that the design's own
    /// inputs already determine, and the two would drift apart at the first section change.
    /// </summary>
    public sealed class CantileverBaseDesign
    {
        /// <summary>Catalogued section of the base, as the text of its <c>StructuralSectionId</c>.</summary>
        public string SectionId { get; set; }

        /// <summary>Length of the base piece along its own axis, inches. Strictly positive.</summary>
        public double Length { get; set; }

        /// <summary>The cap at the free end. Carries no punches.</summary>
        public CantileverPlateDesign FrontPlate { get; set; } = new CantileverPlateDesign();

        /// <summary>
        /// The plate that meets the column. Its punch columns are GOVERNED BY THE COLUMN, so this type
        /// carries only its thickness: a rear plate that declared its own hole positions would be a second
        /// authority for the same bolts.
        /// </summary>
        public CantileverPlateDesign RearPlate { get; set; } = new CantileverPlateDesign();

        /// <summary>The triangular stiffener between the rear plate and the top of the base.</summary>
        public CantileverGussetDesign Gusset { get; set; } = new CantileverGussetDesign();

        public CantileverBaseDesign DeepCopy() =>
            new CantileverBaseDesign
            {
                SectionId = SectionId,
                Length = Length,
                FrontPlate = FrontPlate?.DeepCopy() ?? new CantileverPlateDesign(),
                RearPlate = RearPlate?.DeepCopy() ?? new CantileverPlateDesign(),
                Gusset = Gusset?.DeepCopy() ?? new CantileverGussetDesign()
            };
    }
}
