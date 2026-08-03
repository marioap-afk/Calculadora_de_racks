namespace RackCad.Domain.Systems.Cantilever
{
    /// <summary>
    /// How many profiles make up one arm, and how they are paired.
    ///
    /// It is an ENUM on the body and not a type hierarchy: a consumer walks the resolved members without
    /// asking which arrangement produced them, and adding the fourth arrangement is a value plus a branch in
    /// one authority rather than a subclass plus a <c>switch</c> in every consumer (ADR-0025, D1).
    /// </summary>
    public enum CantileverArmBodyArrangement
    {
        /// <summary>One profile. Product label: «perfil sencillo». Family-independent.</summary>
        Single = 0,

        /// <summary>
        /// Two channels with their OPENINGS FACING each other, touching at the arm's central plane.
        /// Product label: «canal doble encontrado».
        /// </summary>
        DoubleChannelFacing = 1,

        /// <summary>
        /// Two channels with the BACKS OF THEIR WEBS in contact and the openings facing outwards.
        /// Product label: «canal doble espalda con espalda».
        /// </summary>
        DoubleChannelBackToBack = 2
    }

    /// <summary>What closes the free end of an arm, if anything.</summary>
    public enum CantileverArmEndPlateMode
    {
        /// <summary>No end plate at all.</summary>
        None = 0,

        /// <summary>A plate that closes the end, sized to the body envelope, with no extension.</summary>
        Cap = 1,

        /// <summary>The same plate, extended UPWARDS to retain the load. Needs a positive extra height.</summary>
        Stop = 2
    }

    /// <summary>
    /// Which side of the column an arm hangs from.
    ///
    /// There is deliberately no <c>Both</c>: a physical piece is on one side. A double-sided station will
    /// have TWO resolved arms, one per side, and building them is the station's job (ADR-0025, D1).
    /// </summary>
    public enum CantileverArmSide
    {
        /// <summary>The arm projects towards +Y, the same side the base projects towards.</summary>
        PositiveY = 0,

        /// <summary>The arm projects towards −Y, off the column's other flange face.</summary>
        NegativeY = 1
    }

    /// <summary>
    /// Editable intent of an arm BODY: which arrangement, which catalogued section, how long it is cut and
    /// how much it slopes.
    ///
    /// <see cref="SectionId"/> is TEXT for the reason ADR-0024 D1 fixed: Domain declares no project
    /// reference and cannot see <c>StructuralSectionId</c>.
    ///
    /// There is NO field for the gap between paired channels. They touch, the gap is zero, and a field whose
    /// only legal value is zero is an invitation to change it (ADR-0025, D2).
    /// </summary>
    public sealed class CantileverArmBodyDesign
    {
        /// <summary>How many profiles and how they are paired.</summary>
        public CantileverArmBodyArrangement Arrangement { get; set; } = CantileverArmBodyArrangement.Single;

        /// <summary>Catalogued section, as the text of its <c>StructuralSectionId</c>. Both members of a pair share it.</summary>
        public string SectionId { get; set; }

        /// <summary>
        /// The length the profile is CUT to, inches, measured along its own axis.
        ///
        /// It does NOT include the mounting plate thickness, the end plate thickness or the stop extension.
        /// Changing a plate thickness therefore MOVES the arm rather than shortening it, and the number the
        /// user captures stays the number they order (ADR-0025, D3).
        /// </summary>
        public double CutLength { get; set; }

        /// <summary>
        /// Rise per 12 inches of run. The ONE authority for the slope: zero and positive values are allowed,
        /// and the angle in degrees is DERIVED, never stored alongside — two magnitudes for one quantity
        /// drift apart on the first edit (ADR-0025, D4).
        /// </summary>
        public double SlopeRisePer12 { get; set; } = CantileverDefaults.ArmSlopeRisePer12;

        public CantileverArmBodyDesign DeepCopy() =>
            new CantileverArmBodyDesign
            {
                Arrangement = Arrangement,
                SectionId = SectionId,
                CutLength = CutLength,
                SlopeRisePer12 = SlopeRisePer12
            };
    }

    /// <summary>
    /// Editable intent of the plate that bolts an arm to the column, and of WHICH of the column's existing
    /// punches it uses.
    ///
    /// The plate does not declare its own hole grid: it names a contiguous run of the column's already
    /// resolved regular punches. That is what keeps one authority for one set of bolts (ADR-0025, D5).
    /// </summary>
    public sealed class CantileverArmMountingPlateDesign
    {
        /// <summary>Thickness, inches. Its own value.</summary>
        public double Thickness { get; set; } = CantileverDefaults.PlateThickness;

        /// <summary>
        /// Index, BASE ZERO, of the lowest column regular punch elevation this arm bolts to.
        /// </summary>
        public int LowerColumnPunchIndex { get; set; }

        /// <summary>
        /// How many column punch elevations the plate uses, counting upwards from
        /// <see cref="LowerColumnPunchIndex"/>. Integer, minimum 2, no fixed maximum.
        /// </summary>
        public int VerticalPunchCount { get; set; } = CantileverDefaults.ArmVerticalPunchCount;

        /// <summary>
        /// Distancia del primer troquel elegido al borde inferior de la placa, y del último a su borde
        /// superior. Dos pulgadas por omisión, aprobadas por el dueño en la ronda 3.
        ///
        /// Sigue siendo NULLABLE, y no por inercia: un diseño leído de un JSON anterior a esa aprobación no
        /// trae el valor, y «ausente» no es «aprobado». El resolutor sigue rechazando el nulo explícito.
        /// </summary>
        public double? VerticalEndOffset { get; set; } = CantileverDefaults.ArmMountingPlateVerticalEndOffset;

        public CantileverArmMountingPlateDesign DeepCopy() =>
            new CantileverArmMountingPlateDesign
            {
                Thickness = Thickness,
                LowerColumnPunchIndex = LowerColumnPunchIndex,
                VerticalPunchCount = VerticalPunchCount,
                VerticalEndOffset = VerticalEndOffset
            };
    }

    /// <summary>
    /// Editable intent of the arm's end plate. Cap and stop are MODES of one plate, because they differ in
    /// two numbers and not in their nature (ADR-0025, D7).
    /// </summary>
    public sealed class CantileverArmEndPlateDesign
    {
        public CantileverArmEndPlateMode Mode { get; set; } = CantileverArmEndPlateMode.None;

        /// <summary>Thickness, inches. Its own value.</summary>
        public double Thickness { get; set; } = CantileverDefaults.PlateThickness;

        /// <summary>
        /// How far the plate reaches ABOVE the body envelope, inches. Required to be positive for
        /// <see cref="CantileverArmEndPlateMode.Stop"/> and must be zero otherwise. It never alters the
        /// profile's cut length.
        /// </summary>
        public double ExtraStopHeight { get; set; }

        public CantileverArmEndPlateDesign DeepCopy() =>
            new CantileverArmEndPlateDesign
            {
                Mode = Mode,
                Thickness = Thickness,
                ExtraStopHeight = ExtraStopHeight
            };
    }

    /// <summary>
    /// The editable intent of ONE Cantilever arm: a side, a body, the plate that bolts it to the column and
    /// an optional end plate.
    ///
    /// It holds no resolved coordinate — no punch elevation, no plate height, no envelope — for the same
    /// reason <c>CantileverColumnBaseDesign</c> does not: every one of those is derived by the Application
    /// resolver from the column assembly and from the section's geometry.
    ///
    /// It carries NO station or level identity. The future station is what will supply a deterministic owner
    /// token; an arm that stored its own position would be an authority with no consumer yet.
    /// </summary>
    public sealed class CantileverArmDesign
    {
        public CantileverArmSide Side { get; set; } = CantileverArmSide.PositiveY;

        public CantileverArmBodyDesign Body { get; set; } = new CantileverArmBodyDesign();

        public CantileverArmMountingPlateDesign MountingPlate { get; set; } =
            new CantileverArmMountingPlateDesign();

        public CantileverArmEndPlateDesign EndPlate { get; set; } = new CantileverArmEndPlateDesign();

        public CantileverArmDesign DeepCopy() =>
            new CantileverArmDesign
            {
                Side = Side,
                Body = Body?.DeepCopy() ?? new CantileverArmBodyDesign(),
                MountingPlate = MountingPlate?.DeepCopy() ?? new CantileverArmMountingPlateDesign(),
                EndPlate = EndPlate?.DeepCopy() ?? new CantileverArmEndPlateDesign()
            };
    }
}
