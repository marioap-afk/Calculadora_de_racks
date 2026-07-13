using System.Collections.Generic;
using RackCad.Domain.RackFrames;

namespace RackCad.Domain.Systems
{
    /// <summary>
    /// The pallet-driven DESIGN of a selective rack: what the advanced editor edits. The user no longer types
    /// beam length / separation / height directly — they describe the pallets (frente, alto, count) per cell of
    /// a bays × levels matrix, and <c>SelectiveGeometryResolver</c> derives the geometry:
    /// <list type="bullet">
    /// <item>larguero LONGITUD = Frente*Count + Tolerance*(Count+1) (the widest level governs the bay),</item>
    /// <item>level separation = roundUpTroquel(roundUpEven(Alto + Clearance) + beam peralte),</item>
    /// <item>post height = roundUpFoot(topLevelY + topPalletAlto/3).</item>
    /// </list>
    /// </summary>
    public sealed class SelectivePalletDesign
    {
        /// <summary>Catalog id of the post used for the cabeceras.</summary>
        public string PostId { get; set; }

        /// <summary>Peralte of the post (drives the larguero troquel X via the parametric mate).</summary>
        public double PostPeralte { get; set; }

        /// <summary>Horizontal tolerance per gap between/around pallets (in). Editable; default 4".</summary>
        public double PalletTolerance { get; set; } = 4.0;

        /// <summary>Vertical clearance ("holgura") above a pallet inside its clear opening (in). Editable; default 6".</summary>
        public double VerticalClearance { get; set; } = 6.0;

        /// <summary>How far a "larguero a piso" sits above the lowest troquel (in), so its ménsula clears the base plate. Editable; default 4".</summary>
        public double FloorBeamRise { get; set; } = 4.0;

        /// <summary>Pallet depth / fondo (in): the depth of the cabeceras in the LATERAL view. Editable.</summary>
        public double PalletDepth { get; set; } = SelectiveRackDefaults.DefaultPalletDepth;

        /// <summary>
        /// Number of cabecera-lines in DEPTH: 1 = single fondo (sencillo); 2/3/4 = doble/triple/cuádruple
        /// profundidad (espalda con espalda). Each extra fondo repeats the whole depth structure (cabecera +
        /// front/back largueros) offset by <see cref="PalletDepth"/> + the gap. Only the LATERAL and PLANTA views
        /// (and the BOM) change; the FRONTAL elevation is identical. Editable; default 1.
        /// </summary>
        public int DepthCount { get; set; } = 1;

        /// <summary>
        /// Separations (in) between consecutive fondos — one value per gap (<see cref="DepthCount"/> - 1 gaps),
        /// front to back. A gap with no value (or a short list) falls back to the last given value, else
        /// <see cref="SelectiveRackDefaults.DefaultSeparator"/>. The physical separador block is not drawn yet;
        /// this is only the empty space left between adjacent cabecera-lines. Editable per gap.
        /// </summary>
        public IList<double> SeparatorLengths { get; } = new List<double>();

        /// <summary>
        /// Per-fondo pallet depth (in) for fondos 1..N-1: entry <c>k-1</c> is fondo <c>k</c>'s own fondo. A value &lt;= 0
        /// (or a short list) means that fondo inherits fondo 0's <see cref="PalletDepth"/>. Lets each back-to-back line
        /// carry its own depth (one side deeper than the other). Fondo 0's depth is <see cref="PalletDepth"/>.
        /// </summary>
        public IList<double> ExtraFondoDepths { get; } = new List<double>();

        /// <summary>
        /// Optional CUSTOM cabecera (frame) depth per fondo (in), index <c>k</c> = fondo <c>k</c> (fondo 0 included). A
        /// value &lt;= 0 (or a short list) leaves that fondo's cabecera depth DERIVED by the rule (pallet depth −
        /// <see cref="SelectiveRackDefaults.CabeceraFondoAllowance"/>). Lets a line override the tarima − 6 rule.
        /// </summary>
        public IList<double> CabeceraFondoOverrides { get; } = new List<double>();

        /// <summary>The bays of fondo 0 (the primary/front fondo), left to right. Each carries its own column of level cells.</summary>
        public IList<SelectiveBayDesign> Bays { get; } = new List<SelectiveBayDesign>();

        /// <summary>
        /// Per-fondo level matrices for fondos 1..N-1 (a doble-profundidad rack where each side faces a different
        /// aisle and can carry its OWN levels/heights). Entry <c>k-1</c> is fondo <c>k</c>'s bays; a missing or empty
        /// entry means that fondo inherits fondo 0's <see cref="Bays"/>. The horizontal grid (bay widths / post
        /// positions) is defined by fondo 0 and shared, so the posts of every fondo align — only the vertical (levels)
        /// varies here. A fondo's bay with no levels is an empty frente (e.g. a building column). Frente count follows
        /// fondo 0. Empty = every fondo shares fondo 0's matrix (the plain doble-profundidad case).
        /// </summary>
        public IList<IList<SelectiveBayDesign>> ExtraFondoBays { get; } = new List<IList<SelectiveBayDesign>>();

        /// <summary>
        /// Optional per-post "cabecera" (frame), one entry per post position (N frentes → N+1 posts). A null
        /// entry (or a short list) means that post uses the run defaults. The frontal draw uses each cabecera's
        /// base plate (id + peralte); the future lateral view renders the full cabecera. In the frontal a post is
        /// this cabecera seen edge-on.
        /// </summary>
        public IList<RackFrameConfiguration> PostCabeceras { get; } = new List<RackFrameConfiguration>();

        /// <summary>
        /// Optional per-post PERALTE override, one entry per post position (N frentes → N+1 posts). An entry
        /// &lt;= 0 (or a short list) means that post inherits <see cref="PostPeralte"/>. Lets each post carry its
        /// own peralte in the frontal/planta; the larguero spacing adapts to each post's troquel.
        /// </summary>
        public IList<double> PostPeraltes { get; } = new List<double>();

        // ---- Annotation / drawing toggles ----

        /// <summary>Draw the base plates. Default true; turning it off omits the plate blocks in frontal/planta.</summary>
        public bool DrawBasePlate { get; set; } = true;

        /// <summary>Number the frentes (posts). Persisted now; the text drawing is a future pipeline.</summary>
        public bool NumberFronts { get; set; }

        /// <summary>Number the load levels. Persisted now; the text drawing is a future pipeline.</summary>
        public bool NumberLevels { get; set; }

        /// <summary>Draw the rack name as visible text. Persisted now; the text drawing is a future pipeline.</summary>
        public bool DrawRackName { get; set; }

        /// <summary>Multiplier on the annotation text height (1 = default 6"). Scales the frente/level/name labels AND the dimensions.</summary>
        public double AnnotationScale { get; set; } = 1.0;

        /// <summary>How much automatic dimensioning to draw per view (None = off). Scaled by <see cref="AnnotationScale"/>.</summary>
        public DimensionDetail Dimensions { get; set; } = DimensionDetail.None;

        /// <summary>Name of the AutoCAD dimension style to use for the cotas; null/empty = automatic (the drawing's
        /// current style, sized to <see cref="AnnotationScale"/>). A chosen style is respected as-is.</summary>
        public string DimensionStyle { get; set; }

        /// <summary>Safety accessories chosen for this rack (catalog id + quantity), for the BOM. Drawing them in the
        /// views is a future phase (needs their AutoCAD blocks); for now they are catalog + selection + BOM.</summary>
        public IList<SelectiveSafetySelection> SafetySelections { get; } = new List<SelectiveSafetySelection>();
    }

    /// <summary>One safety accessory chosen for a rack: its catalog id and how many. Quantity ≤ 0 = not included.</summary>
    public sealed class SelectiveSafetySelection
    {
        public string ElementId { get; set; }
        public int Quantity { get; set; }
    }

    /// <summary>One bay's column in the design matrix: its level cells (its own count), bottom to top.</summary>
    public sealed class SelectiveBayDesign
    {
        /// <summary>
        /// Whether the ground level (level 0) carries a larguero ("larguero a piso"). Default false: the ground
        /// pallet rests on the floor (from Y=0) with no beam, and the first larguero sits above it. When true,
        /// the ground level gets a beam at the lowest troquel and the pallet stacks from there.
        /// </summary>
        public bool FloorBeam { get; set; }

        /// <summary>Manual override for this bay's height (in). Null = auto. A post still takes the tallest of the bays it touches.</summary>
        public double? HeightOverride { get; set; }

        /// <summary>
        /// "Medio frente" generalizado: partition this bay into N tramos with N-1 INTERMEDIATE posts (of this fondo
        /// only, so the fondos stay aligned at the shared end posts). Each tramo has a larguero length and a loaded
        /// flag; the LAST tramo's length is CALCULATED (the remainder of the bay). Fewer than 2 tramos = a normal
        /// full-width bay. Lengths are free measures, NOT tied to a pallet count — a triple/quad frente can store
        /// fewer pallets. Marking which tramos carry largueros lets you tie one side, the other, or both. Per fondo.
        /// </summary>
        public IList<SelectiveSegment> Segments { get; } = new List<SelectiveSegment>();

        /// <summary>The level cells of this bay, bottom to top. Each cell can differ (pallet, count, beam).</summary>
        public IList<SelectiveCell> Levels { get; } = new List<SelectiveCell>();
    }

    /// <summary>
    /// One "tramo" of a split frente ("medio frente" generalizado). A larguero of length <see cref="Length"/> that
    /// either carries largueros (<see cref="Loaded"/>) or stays empty. A bay's tramos are separated by intermediate
    /// posts; the LAST tramo's length is CALCULATED (the remainder), so its <see cref="Length"/> is ignored.
    /// </summary>
    public sealed class SelectiveSegment
    {
        /// <summary>Larguero length (in) of this tramo. Ignored for the last tramo (calculated from the remainder).</summary>
        public double Length { get; set; }

        /// <summary>Whether this tramo carries largueros (a load position) or stays empty. Lets you tie one side, the other, or both.</summary>
        public bool Loaded { get; set; } = true;
    }

    /// <summary>One cell of the matrix (a level of a bay): the pallet stored there and its beam.</summary>
    public sealed class SelectiveCell
    {
        /// <summary>The pallet type at this cell (frente drives the beam length, alto the separation above it).</summary>
        public Tarima Pallet { get; set; } = new Tarima();

        /// <summary>How many pallets sit side by side at this level ("tarimas por nivel").</summary>
        public int PalletCount { get; set; } = 1;

        /// <summary>Catalog id of the beam (larguero) at this level.</summary>
        public string BeamId { get; set; }

        /// <summary>Beam peralte (block parameter) at this level.</summary>
        public double BeamPeralte { get; set; }

        /// <summary>Manual override for the larguero LONGITUD at this level (in). Null = auto (Frente*Count + tolerance). The bay uses the longest level.</summary>
        public double? BeamLengthOverride { get; set; }

        /// <summary>Manual override for the clear/separation BELOW this level's beam (in), snapped up to the troquel grid. Null = auto.</summary>
        public double? ClearOverride { get; set; }
    }

    /// <summary>A pallet ("tarima"). Frontal needs its front and height; depth (fondo) comes with the lateral view.</summary>
    public sealed class Tarima
    {
        /// <summary>Front width of the pallet (in), measured along the beam.</summary>
        public double Frente { get; set; }

        /// <summary>Height of the pallet + load (in); drives the clear opening to the level above.</summary>
        public double Alto { get; set; }
    }
}
