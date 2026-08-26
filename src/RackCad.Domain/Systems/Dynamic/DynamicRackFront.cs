using System.Collections.Generic;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Domain.Systems.Dynamic
{
    /// <summary>
    /// I-42 — un TRAMO de profundidad ocupado por un frente, en posiciones 1-based de la secuencia compartida.
    ///
    /// <para>
    /// Existe porque un frente de un rack COMPUESTO no ocupa un rango continuo: su lado A vive pegado al arranque y
    /// su lado B al final, y entre los dos puede quedar profundidad que ese frente NO usa. Un rack de un solo
    /// sentido tiene siempre un unico tramo, que es su rango de siempre.
    /// </para>
    /// </summary>
    public readonly struct DynamicDepthSegment
    {
        public DynamicDepthSegment(int startPosition, int positions)
        {
            StartPosition = startPosition;
            Positions = positions;
        }

        /// <summary>Primera posicion (1-based) del tramo.</summary>
        public int StartPosition { get; }

        /// <summary>Cuantas posiciones ocupa.</summary>
        public int Positions { get; }

        /// <summary>Ultima posicion (1-based) del tramo.</summary>
        public int EndPosition => StartPosition + Positions - 1;

        public bool Contains(int position) => position >= StartPosition && position <= EndPosition;
    }

    /// <summary>Editable transverse intent for one dynamic rack front.</summary>
    public sealed class DynamicRackFrontDesign
    {
        /// <summary>
        /// False marks a BLANK front (I-33/PB-014): the front keeps its claro and its structure — posts, header
        /// modules, separators and derived posts — and still displaces the fronts behind it, but contributes NO
        /// effective load level and therefore no load component. Every other field stays DORMANT so switching the
        /// front back to active restores exactly the configuration it had; a blank front is never represented by a
        /// fake cell or a zeroed count. Legacy documents have no notion of blank fronts and load as active.
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>Number of pallet-flow lanes placed side by side in this front.</summary>
        public int PalletCount { get; set; } = 1;

        /// <summary>Optional number of load levels in this front; null keeps the design-wide legacy value.</summary>
        public int? LoadLevels { get; set; }

        /// <summary>Optional pallets deep for this front; null keeps the design-wide legacy value.</summary>
        public int? PalletsDeep { get; set; }

        /// <summary>One-based longitudinal position where this front starts in the shared structure.</summary>
        public int? DepthStartPosition { get; set; }

        /// <summary>Optional IN/OUT beam cut length (in). Null uses the pallet-driven standard rule.</summary>
        public double? BeamLengthOverride { get; set; }

        /// <summary>Optional first load-beam elevation for this front; null keeps the rack-wide legacy value.</summary>
        public double? FirstLevelHeight { get; set; }

        /// <summary>Editable intermediate-beam PERALTE by level for this front, level 1 first.</summary>
        public IList<double> IntermediateBeamDepths { get; } = new List<double>();

        /// <summary>Editable cell values, level 1 first. Missing entries inherit the rack-wide legacy fields.</summary>
        public IList<DynamicRackLevelDesign> Levels { get; } = new List<DynamicRackLevelDesign>();

        /// <summary>
        /// I-42 — los TRAMOS de profundidad que este frente ocupa realmente. VACIA es lo normal y significa «un solo
        /// tramo, el de <see cref="DepthStartPosition"/> y <see cref="PalletsDeep"/>»: todo diseño anterior a la
        /// iniciativa y todo rack de un solo sentido se comportan exactamente igual que antes.
        ///
        /// <para>
        /// Es DERIVADA y no se persiste: la construye el compositor del rack compuesto a partir de la demanda de
        /// cada lado y de la topologia de sus celdas. Reconstruirla en cada resolucion es lo que evita que un
        /// documento guardado tenga que entenderla.
        /// </para>
        /// </summary>
        public IList<DynamicDepthSegment> DepthSegments { get; } = new List<DynamicDepthSegment>();
    }

    /// <summary>Resolved transverse width of one front; drawing and BOM consume this same result.</summary>
    public sealed class DynamicRackFront
    {
        public int Index { get; set; }

        /// <summary>
        /// False marks a resolved BLANK front (I-33/PB-014). Every structural field below stays resolved exactly as
        /// if the front were active — that is what keeps its claro, its post height and the displacement it imposes
        /// on the fronts behind it — while <see cref="LoadLevels"/> and <see cref="Levels"/> remain DORMANT. Callers
        /// must never read those two directly to size load work: the one authority is
        /// <c>DynamicFrontActivation.EffectiveLoadLevels</c>, which answers zero here.
        /// </summary>
        public bool IsActive { get; set; } = true;

        public int PalletCount { get; set; }
        public int LoadLevels { get; set; }
        public int PalletsDeep { get; set; }
        public int DepthStartPosition { get; set; } = 1;

        /// <summary>Resolved longitudinal limits in the shared system coordinates.</summary>
        public double StartX { get; set; }
        public double EndX { get; set; }

        /// <summary>Bed-frame width (BFR) of one lane: pallet front + 2 in.</summary>
        public double Bfr { get; set; }

        public double BeamLength { get; set; }
        public double? BeamLengthOverride { get; set; }

        /// <summary>Resolved first load-beam elevation owned by this front.</summary>
        public double FirstLevelHeight { get; set; } = DynamicRackDefaults.DefaultFirstLevelHeight;

        /// <summary>Resolved commercial post height required by this front's own load levels.</summary>
        public double Height { get; set; }

        /// <summary>
        /// I-42 — los TRAMOS de profundidad que este frente ocupa realmente (ver
        /// <see cref="DynamicRackFrontDesign.DepthSegments"/>). Vacia = un solo tramo continuo, que es el caso de
        /// todo rack de un solo sentido.
        /// </summary>
        public IList<DynamicDepthSegment> DepthSegments { get; } = new List<DynamicDepthSegment>();

        /// <summary>Resolved end-beam elevations for this front's own depth and slope.</summary>
        public IList<DynamicLoadBeamLevel> LoadBeamLevels { get; } = new List<DynamicLoadBeamLevel>();

        /// <summary>Resolved catalog-valid intermediate-beam PERALTE by level for this front.</summary>
        public IList<double> IntermediateBeamDepths { get; } = new List<double>();

        /// <summary>Resolved values of each front x level cell, level 1 first.</summary>
        public IList<DynamicRackLevel> Levels { get; } = new List<DynamicRackLevel>();
    }

    /// <summary>Editable intent of one dynamic front x level cell. Nullable fields preserve legacy fallbacks.</summary>
    public sealed class DynamicRackLevelDesign
    {
        public double? PalletFront { get; set; }
        public double? PalletHeight { get; set; }
        public double? PalletWeight { get; set; }
        public double? ClearHeight { get; set; }
        public string InOutBeamCatalogId { get; set; }
        public double? InOutBeamDepth { get; set; }
        public double? BeamLengthOverride { get; set; }
        public string IntermediateBeamCatalogId { get; set; }
        public double? IntermediateBeamDepth { get; set; }
    }

    /// <summary>Resolved physical and commercial values of one dynamic front x level cell.</summary>
    public sealed class DynamicRackLevel
    {
        public int LevelNumber { get; set; }
        public PalletSpecification Pallet { get; set; } = new PalletSpecification();
        public double ClearHeight { get; set; } = DynamicRackDefaults.DefaultClearHeight;
        public string InOutBeamCatalogId { get; set; } = DynamicRackDefaults.InOutBeamCatalogId;
        public double InOutBeamDepth { get; set; } = DynamicRackDefaults.DefaultBeamDepth;
        public double? BeamLengthOverride { get; set; }
        public double Bfr { get; set; }
        public double BeamLength { get; set; }
        public string IntermediateBeamCatalogId { get; set; } = DynamicRackDefaults.IntermediateBeamCatalogId;
        public double IntermediateBeamDepth { get; set; } = DynamicRackDefaults.DefaultIntermediateBeamDepth;
    }

    /// <summary>The two physical end cuts of a pallet-flow lane.</summary>
    public enum DynamicRackEnd
    {
        Exit = 0,
        Entrance = 1
    }
}
