using System.Collections.Generic;
using System.Linq;
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
        /// <see cref="SelectiveRackDefaults.DefaultSeparator"/>. The same value drives the physical separador blocks in
        /// lateral/planta and their BOM component; the frontal intentionally shows only the gap. Editable per gap.
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
        /// base plate (id + peralte); lateral/planta render the full cabecera. In the frontal a post is this cabecera
        /// seen edge-on.
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

        /// <summary>Number the frentes in the generated annotations.</summary>
        public bool NumberFronts { get; set; }

        /// <summary>Number the load levels in frontal/lateral annotations.</summary>
        public bool NumberLevels { get; set; }

        /// <summary>Draw the rack name as visible text in the generated views.</summary>
        public bool DrawRackName { get; set; }

        /// <summary>Draw the pallets (tarimas) as a VISUAL reference on the load levels (and the floor). Default off;
        /// the block is the catalog "TARIMA" piece and never enters the BOM.</summary>
        public bool DrawPallets { get; set; }

        /// <summary>Multiplier on the annotation text height (1 = default 6"). Scales the frente/level/name labels AND the dimensions.</summary>
        public double AnnotationScale { get; set; } = 1.0;

        /// <summary>How much automatic dimensioning to draw per view (None = off). Scaled by <see cref="AnnotationScale"/>.</summary>
        public DimensionDetail Dimensions { get; set; } = DimensionDetail.None;

        /// <summary>Name of the AutoCAD dimension style to use for the cotas; null/empty = automatic (the drawing's
        /// current style, sized to <see cref="AnnotationScale"/>). A chosen style is respected as-is.</summary>
        public string DimensionStyle { get; set; }

        /// <summary>Safety accessories chosen for this rack. Implemented families drive their view blocks and BOM;
        /// unknown/future families retain a manual BOM quantity until their placement rule exists.</summary>
        public IList<SelectiveSafetySelection> SafetySelections { get; } = new List<SelectiveSafetySelection>();
    }

    /// <summary>Which side(s) of a post a drawable safety accessory (e.g. a bota) sits on. None = not drawn.</summary>
    public enum SafetySide
    {
        None = 0,
        Left = 1,
        Right = 2,
        Both = 3
    }

    /// <summary>One safety accessory chosen for a rack: its catalog id, a manual quantity (BOM fallback for elements
    /// with no drawing rule yet), and — for a DRAWABLE element (bota) — the <see cref="Side"/> it sits on at each post,
    /// with optional <see cref="PostSides"/> exceptions for specific posts.</summary>
    public sealed class SelectiveSafetySelection
    {
        public string ElementId { get; set; }
        public int Quantity { get; set; }

        /// <summary>Default side for a drawable element, applied to every post unless overridden in <see cref="PostSides"/>.</summary>
        public SafetySide Side { get; set; } = SafetySide.Both;

        /// <summary>Per-post overrides (post index → side); a post not listed uses <see cref="Side"/>.</summary>
        public IList<SafetyPostSide> PostSides { get; } = new List<SafetyPostSide>();

        /// <summary>The side for post <paramref name="postIndex"/>: its override if present, else the default <see cref="Side"/>.</summary>
        public SafetySide SideForPost(int postIndex)
        {
            foreach (var over in PostSides)
            {
                if (over != null && over.PostIndex == postIndex) return over.Side;
            }

            return Side;
        }

        // ---- Per-family safety configuration (I-22, E7): each family owns a sealed config with its OWN DeepCopy
        // and persistence mapping; the flat accessors below delegate to it, so existing consumers stay unchanged. ----

        private SelectiveTopeConfig tope = new SelectiveTopeConfig();
        private SelectiveDesviadorConfig desviador = new SelectiveDesviadorConfig();
        private SelectiveDefensaConfig defensa = new SelectiveDefensaConfig();
        private SelectiveGuiaConfig guia = new SelectiveGuiaConfig();
        private SelectiveParrillaConfig parrilla = new SelectiveParrillaConfig();

        /// <summary>TOPE (larguero tope) configuration. Never null.</summary>
        public SelectiveTopeConfig Tope { get => tope; set => tope = value ?? new SelectiveTopeConfig(); }

        /// <summary>DESVIADOR configuration. Never null.</summary>
        public SelectiveDesviadorConfig Desviador { get => desviador; set => desviador = value ?? new SelectiveDesviadorConfig(); }

        /// <summary>DEFENSA configuration. Never null.</summary>
        public SelectiveDefensaConfig Defensa { get => defensa; set => defensa = value ?? new SelectiveDefensaConfig(); }

        /// <summary>GUIA (entrance guide) configuration. Never null.</summary>
        public SelectiveGuiaConfig Guia { get => guia; set => guia = value ?? new SelectiveGuiaConfig(); }

        /// <summary>PARRILLA (deck) configuration. Never null.</summary>
        public SelectiveParrillaConfig Parrilla { get => parrilla; set => parrilla = value ?? new SelectiveParrillaConfig(); }

        // ---- Flat accessors delegating to the per-family configs (compatibility surface for existing consumers) ----

        /// <summary>TOPE: shared central tope vs one per fondo. Delegates to <see cref="Tope"/>.</summary>
        public bool TopeShared { get => Tope.Shared; set => Tope.Shared = value; }

        /// <summary>TOPE: which fondo carries the tope; &lt; 0 = automatic central. Delegates to <see cref="Tope"/>.</summary>
        public int TopeFondo { get => Tope.Fondo; set => Tope.Fondo = value; }

        /// <summary>TOPE: the block SAQUE (stick-out), inches. Delegates to <see cref="Tope"/>.</summary>
        public double TopeSaque { get => Tope.Saque; set => Tope.Saque = value; }

        /// <summary>TOPE: also draw it in the FRONTAL view. Delegates to <see cref="Tope"/>.</summary>
        public bool TopeFrontal { get => Tope.Frontal; set => Tope.Frontal = value; }

        /// <summary>TOPE: the (frente, level) cells with NO tope. Delegates to <see cref="Tope"/>.</summary>
        public IList<SelectiveGridCell> TopeOffCells => Tope.OffCells;

        /// <summary>TOPE: true if a tope is drawn at (frente, level). Delegates to <see cref="Tope"/>.</summary>
        public bool TopeAt(int frente, int level) => Tope.At(frente, level);

        /// <summary>DESVIADOR: dynamic LONGITUD (in). Delegates to <see cref="Desviador"/>.</summary>
        public double DesviadorLongitud { get => Desviador.Longitud; set => Desviador.Longitud = value; }

        /// <summary>DESVIADOR: first load-level height above the first TROQUEL_LARGUERO (in). Delegates to <see cref="Desviador"/>.</summary>
        public double DesviadorPrimerNivelAltura { get => Desviador.PrimerNivelAltura; set => Desviador.PrimerNivelAltura = value; }

        /// <summary>DESVIADOR: disabled (post, load-level) cells. Delegates to <see cref="Desviador"/>.</summary>
        public IList<SelectiveGridCell> DesviadorOffCells => Desviador.OffCells;

        public bool DesviadorAt(int post, int level) => Desviador.At(post, level);

        /// <summary>DEFENSA: explicit per-post lengths. Delegates to <see cref="Defensa"/>.</summary>
        public IList<SafetyPostDefense> DefensaPosts => Defensa.Posts;

        /// <summary>GUIA: zero-based frente/level cells without entrance guides. Delegates to <see cref="Guia"/>.</summary>
        public IList<SelectiveGridCell> GuiaEntradaOffCells => Guia.OffCells;

        public bool GuiaEntradaAt(int frontIndex, int levelIndex) => Guia.At(frontIndex, levelIndex);

        /// <summary>PARRILLA: draw the deck in the FRONTAL view. Delegates to <see cref="Parrilla"/>.</summary>
        public bool ParrillaFrontal { get => Parrilla.Frontal; set => Parrilla.Frontal = value; }

        /// <summary>PARRILLA: draw the deck in the LATERAL view. Delegates to <see cref="Parrilla"/>.</summary>
        public bool ParrillaLateral { get => Parrilla.Lateral; set => Parrilla.Lateral = value; }

        /// <summary>PARRILLA: manual deck width (FRENTE, inches). Delegates to <see cref="Parrilla"/>.</summary>
        public double ParrillaFrente { get => Parrilla.Frente; set => Parrilla.Frente = value; }

        /// <summary>PARRILLA: manual deck count per load row. Delegates to <see cref="Parrilla"/>.</summary>
        public int ParrillaCantidad { get => Parrilla.Cantidad; set => Parrilla.Cantidad = value; }

        /// <summary>PARRILLA: the (frente, level) cells with NO deck. Delegates to <see cref="Parrilla"/>.</summary>
        public IList<SelectiveGridCell> ParrillaOffCells => Parrilla.OffCells;

        /// <summary>PARRILLA: true if a deck sits at (frente, level). Delegates to <see cref="Parrilla"/>.</summary>
        public bool ParrillaAt(int frente, int level) => Parrilla.At(frente, level);

        /// <summary>
        /// Deep working copy used when a selection crosses the design/resolver/view/UI boundaries. Delegates to each
        /// per-family config's own DeepCopy, so a new family adds its config's clone rather than another field here
        /// (I-22, E7). Persistence remains an explicit per-family DTO mapping so legacy fallbacks stay visible and tested.
        /// </summary>
        public SelectiveSafetySelection DeepCopy()
        {
            var copy = new SelectiveSafetySelection
            {
                ElementId = ElementId,
                Quantity = Quantity,
                Side = Side,
                Tope = Tope.DeepCopy(),
                Desviador = Desviador.DeepCopy(),
                Defensa = Defensa.DeepCopy(),
                Guia = Guia.DeepCopy(),
                Parrilla = Parrilla.DeepCopy()
            };

            foreach (var post in PostSides)
            {
                if (post != null)
                {
                    copy.PostSides.Add(new SafetyPostSide { PostIndex = post.PostIndex, Side = post.Side });
                }
            }

            return copy;
        }
    }

    /// <summary>A per-post side override for a safety selection.</summary>
    public sealed class SafetyPostSide
    {
        public int PostIndex { get; set; }
        public SafetySide Side { get; set; }
    }

    /// <summary>An explicit dynamic forklift-defense length at one transverse post; zero means disabled.</summary>
    public sealed class SafetyPostDefense
    {
        public int PostIndex { get; set; }
        public double ExitLength { get; set; }
        public double EntranceLength { get; set; }
    }

    /// <summary>A (frente, level) cell reference — used to mark which larguero cells carry (or skip) a tope.</summary>
    public sealed class SelectiveGridCell
    {
        public int Frente { get; set; }
        public int Level { get; set; }
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
