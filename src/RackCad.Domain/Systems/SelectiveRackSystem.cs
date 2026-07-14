using System.Collections.Generic;
using RackCad.Domain.RackFrames;

namespace RackCad.Domain.Systems
{
    /// <summary>
    /// The RESOLVED geometry of a selective rack run in the FRONTAL view: a sequence of bays bounded by
    /// shared frames (cabeceras). N bays → N+1 cabeceras (each drawn as one post in frontal). Everything here
    /// is already computed (beam lengths, level Ys, per-bay height) — the pallet-driven rules live in the
    /// resolver (<c>SelectiveGeometryResolver</c>); this is just what the builder places. Each bay (and each
    /// level within it) can differ; a post takes the height of the tallest bay it touches.
    /// </summary>
    public sealed class SelectiveRackSystem
    {
        /// <summary>Tallest bay's height (in) = the run's overall height. Individual posts use their neighbours' <see cref="SelectiveBay.Height"/>.</summary>
        public double Height { get; set; }

        /// <summary>Catalog id of the post used for the cabeceras.</summary>
        public string PostId { get; set; }

        /// <summary>Peralte of the post (drives the larguero troquel X via the parametric mate).</summary>
        public double PostPeralte { get; set; }

        /// <summary>Pallet depth / fondo (in): the cabeceras' depth in the LATERAL view.</summary>
        public double PalletDepth { get; set; }

        /// <summary>Number of cabecera-lines in depth (1 = sencillo; 2/3/4 = doble profundidad). Pass-through from the design.</summary>
        public int DepthCount { get; set; } = 1;

        /// <summary>Separations (in) between consecutive fondos (one per gap; short list / missing = fall back to the last value, else the default). Pass-through.</summary>
        public IList<double> SeparatorLengths { get; } = new List<double>();

        /// <summary>Resolved pallet depth (in) of each fondo (one per fondo; index 0 = the primary fondo's <see cref="PalletDepth"/>). Each back-to-back line can have its own fondo.</summary>
        public IList<double> FondoDepths { get; } = new List<double>();

        /// <summary>Resolved CUSTOM cabecera-depth override per fondo (index k; &lt;= 0 = derived by the rule, pallet − allowance). Pass-through.</summary>
        public IList<double> FondoCabeceraOverrides { get; } = new List<double>();

        /// <summary>The bays of fondo 0 (the primary/front fondo), left to right. N bays sit between N+1 cabeceras.</summary>
        public IList<SelectiveBay> Bays { get; } = new List<SelectiveBay>();

        /// <summary>
        /// The resolved bays of EACH fondo (index 0 is the same content as <see cref="Bays"/>, the primary fondo).
        /// Every fondo shares the same per-bay <see cref="SelectiveBay.BeamLength"/> (the horizontal grid from fondo 0,
        /// so the posts align), but carries its OWN levels and per-bay height — doble profundidad with distinct level
        /// configs. A fondo's bay with no levels is an empty frente (e.g. a building column). One entry per fondo
        /// (<see cref="DepthCount"/>); a single fondo leaves this with just fondo 0.
        /// </summary>
        public IList<IList<SelectiveBay>> FondoBays { get; } = new List<IList<SelectiveBay>>();

        /// <summary>Optional per-post cabecera (pass-through from the design), one per post position; null = run default.</summary>
        public IList<RackFrameConfiguration> PostCabeceras { get; } = new List<RackFrameConfiguration>();

        /// <summary>Resolved per-post PERALTE, one per post (N+1). Each is the post's override or the run's <see cref="PostPeralte"/>.</summary>
        public IList<double> PostPeraltes { get; } = new List<double>();

        /// <summary>Draw the base plates (frontal/planta). Default true; false omits the plate blocks.</summary>
        public bool DrawBasePlate { get; set; } = true;

        /// <summary>Draw a number under each frente (bay). Text annotation.</summary>
        public bool NumberFronts { get; set; }

        /// <summary>Draw a number for each load level. Text annotation.</summary>
        public bool NumberLevels { get; set; }

        /// <summary>Draw the rack name as visible text above the frontal.</summary>
        public bool DrawRackName { get; set; }

        /// <summary>Draw the pallets (tarimas) as a visual reference on the load levels + floor. The "TARIMA" catalog
        /// block; never in the BOM.</summary>
        public bool DrawPallets { get; set; }

        /// <summary>Multiplier on the annotation text height (1 = default 6"). Scales the frente/level/name labels.</summary>
        public double AnnotationScale { get; set; } = 1.0;

        /// <summary>How much automatic dimensioning to draw per view (None = off).</summary>
        public DimensionDetail Dimensions { get; set; } = DimensionDetail.None;

        /// <summary>Name of the AutoCAD dimension style for the cotas; null/empty = automatic (current style, scaled).</summary>
        public string DimensionStyle { get; set; }

        /// <summary>Safety accessories chosen for this rack (catalog id + quantity), carried through to the BOM.</summary>
        public IList<SelectiveSafetySelection> SafetySelections { get; } = new List<SelectiveSafetySelection>();

        /// <summary>Client-facing rack name (used by the DrawRackName annotation); set at draw time.</summary>
        public string Name { get; set; }
    }

    /// <summary>One resolved bay: its beam length (which governs post spacing), its height, and its placed levels.</summary>
    public sealed class SelectiveBay
    {
        /// <summary>
        /// Beam length = LONGITUD (the profile "A corte"), per bay: it fixes the post spacing
        /// (post-to-post = BeamLength + 2*(troquelX + inicioPerfilX)). All levels of the bay share it.
        /// </summary>
        public double BeamLength { get; set; }

        /// <summary>
        /// Beam id of the level that GOVERNED <see cref="BeamLength"/> (the widest one). Post spacing must use
        /// THIS beam's ménsula overhang — with mixed beam types per bay, another level's overhang would misplace
        /// the posts. Null/empty = single-type bay (any level works).
        /// </summary>
        public string GoverningBeamId { get; set; }

        /// <summary>Height this bay requires (in) = roundUpFoot(topBeamY + topAlto/3). A post uses the tallest bay it touches.</summary>
        public double Height { get; set; }

        /// <summary>"Medio frente" generalizado: N tramos with N-1 intermediate posts (of this fondo only). Each tramo
        /// carries a larguero length and a loaded flag; the LAST tramo's length is CALCULATED (the remainder). Fewer
        /// than 2 tramos = a normal full-width bay. Does NOT change the bay's <see cref="BeamLength"/> (post spacing) —
        /// the shared end posts stay aligned across fondos. See <c>SelectiveMedioFrente</c> for the tramo layout.</summary>
        public IList<SelectiveSegment> Segments { get; } = new List<SelectiveSegment>();

        /// <summary>The load levels of this bay, bottom to top, each with its own resolved Y and beam.</summary>
        public IList<SelectiveLevel> Levels { get; } = new List<SelectiveLevel>();

        /// <summary>A pallet that rests directly on the FLOOR (Y=0), with NO larguero — only present when the bottom
        /// pallet is not on a floor beam (so it is not one of <see cref="Levels"/>). 0 count = none. For DrawPallets.</summary>
        public double FloorPalletFrente { get; set; }
        public double FloorPalletAlto { get; set; }
        public int FloorPalletCount { get; set; }
    }

    /// <summary>One resolved load level of a bay: a larguero at a fixed (already snapped) troquel Y.</summary>
    public sealed class SelectiveLevel
    {
        /// <summary>Troquel Y where the larguero sits (already snapped to the grid by the resolver).</summary>
        public double Y { get; set; }

        /// <summary>Catalog id of the beam (larguero) at this level.</summary>
        public string BeamId { get; set; }

        /// <summary>Beam peralte (block parameter) at this level.</summary>
        public double BeamPeralte { get; set; }

        /// <summary>Pallet width (frente), height (alto) and count on this level — carried from the design cell so the
        /// pallet visual reference (DrawPallets) can place them without re-reading the design. 0 = no pallet drawn.</summary>
        public double PalletFrente { get; set; }
        public double PalletAlto { get; set; }
        public int PalletCount { get; set; }
    }
}
