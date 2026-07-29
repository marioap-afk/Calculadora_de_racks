namespace RackCad.Domain.Systems.Cantilever
{
    /// <summary>
    /// A flat plate of the column–base sub-assembly, as INTENT: its thickness, and nothing else.
    ///
    /// Its outline is not here on purpose. Every plate of this sub-assembly takes its outline from the
    /// envelope of a resolved section — the base's for the front and rear plates, the column's for the
    /// bottom one — so storing an outline would store a derived value that goes stale the moment the design
    /// changes its section.
    ///
    /// Three separate INSTANCES of this type live in the design (front, rear, column bottom), plus a
    /// <see cref="CantileverGussetDesign"/>. That is four independent thicknesses that share a default
    /// constant and nothing else: changing one never moves another.
    /// </summary>
    public sealed class CantileverPlateDesign
    {
        public CantileverPlateDesign()
        {
        }

        public CantileverPlateDesign(double thickness)
        {
            Thickness = thickness;
        }

        /// <summary>Thickness, inches. Its own value, defaulted from <see cref="CantileverDefaults.PlateThickness"/>.</summary>
        public double Thickness { get; set; } = CantileverDefaults.PlateThickness;

        public CantileverPlateDesign DeepCopy() => new CantileverPlateDesign(Thickness);
    }

    /// <summary>
    /// The gusset, as INTENT: its thickness.
    ///
    /// It is a type of its own and not a third <see cref="CantileverPlateDesign"/> because a gusset is not
    /// a plate: it is triangular, it lives in the central vertical plane rather than facing a section, and
    /// its two legs are DERIVED from where the last connection punch falls — never from a stored size.
    /// </summary>
    public sealed class CantileverGussetDesign
    {
        public CantileverGussetDesign()
        {
        }

        public CantileverGussetDesign(double thickness)
        {
            Thickness = thickness;
        }

        /// <summary>Thickness, inches.</summary>
        public double Thickness { get; set; } = CantileverDefaults.PlateThickness;

        public CantileverGussetDesign DeepCopy() => new CantileverGussetDesign(Thickness);
    }
}
