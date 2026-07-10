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

        /// <summary>The bays, left to right. Each carries its own column of level cells (its own count).</summary>
        public IList<SelectiveBayDesign> Bays { get; } = new List<SelectiveBayDesign>();

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

        /// <summary>The level cells of this bay, bottom to top. Each cell can differ (pallet, count, beam).</summary>
        public IList<SelectiveCell> Levels { get; } = new List<SelectiveCell>();
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
