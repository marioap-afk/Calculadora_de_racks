using System;
using System.Collections.Generic;
using System.Linq;

namespace RackCad.Domain.Systems.Cantilever
{
    /// <summary>How the braced panel count of an interval is decided.</summary>
    public enum CantileverBracedPanelCountMode
    {
        /// <summary>The product rule decides it from the column height (ADR-0027, D4).</summary>
        Automatic = 0,

        /// <summary>The user supplies it. Must be positive.</summary>
        Manual = 1
    }

    /// <summary>
    /// What a brace physically IS.
    ///
    /// Two products with the same function and different anatomy: a bolted structural profile, and a
    /// cold-rolled rod that cannot be drilled and bolted so it needs an ADAPTER at each end. Modelling only
    /// the first and "approximating" the second would give a BOM nobody can buy (ADR-0027, D7).
    /// </summary>
    public enum CantileverBraceBodyKind
    {
        /// <summary>A catalogued profile, bolted through a hole near each end. Families Channel and Angle.</summary>
        StructuralSection = 0,

        /// <summary>A round cold-rolled rod between two end adapters. The product default.</summary>
        ColdRolledRound = 1
    }

    /// <summary>
    /// Editable intent of a cold-rolled brace: its diameter, and nothing else.
    ///
    /// The cut length is DERIVED from the adapters —the distance between their cold-rolled datums— so it is
    /// not a field. And there are no threads, nuts, tolerances or fabrication extensions here: ADR-0027 D7
    /// keeps all of that out of scope, and inventing one would put a number in the BOM nobody approved.
    /// </summary>
    public sealed class CantileverColdRolledBraceDesign
    {
        /// <summary>Rod diameter, inches. Editable, finite and positive. Default 3/4 in.</summary>
        public double Diameter { get; set; } = CantileverLineDefaults.ColdRolledBraceDiameter;

        public CantileverColdRolledBraceDesign DeepCopy() =>
            new CantileverColdRolledBraceDesign { Diameter = Diameter };
    }

    /// <summary>
    /// Editable intent of the bracing of a whole line: the separator, the vertical distribution of panels and
    /// the brace.
    ///
    /// It belongs to the LINE and not to an interval, because every interval of a line braces the same way.
    /// An interval that could brace differently from its neighbour is a decision nobody has taken.
    /// </summary>
    public sealed class CantileverBracingDesign
    {
        /// <summary>
        /// Catalogued separator section, as the text of its <c>StructuralSectionId</c> (ADR-0024, D1).
        ///
        /// Default <c>AISC-C-C4X4_5</c>: the lightest 4 in channel, and unambiguously so — the catalogue has
        /// exactly four C4 rows and their weights do not tie (4.5 &lt; 5.4 &lt; 6.25 &lt; 7.25). It is an EXACT
        /// id: nothing resolves a separator by searching a designation (decision 12.19).
        /// </summary>
        public string SeparatorSectionId { get; set; } = CantileverLineDefaults.SeparatorSectionId;

        public CantileverBracedPanelCountMode PanelCountMode { get; set; } =
            CantileverBracedPanelCountMode.Automatic;

        /// <summary>
        /// The panel count the user typed. Only read under <c>Manual</c>, where it is mandatory and positive.
        ///
        /// Under <c>Automatic</c> it is DORMANT DATA and deliberately not validated — the same rule the
        /// station's manual height follows.
        /// </summary>
        public int? ManualPanelCount { get; set; }

        /// <summary>Height of one braced panel, inches. Default 40 in.</summary>
        public double BracedPanelHeight { get; set; } = CantileverLineDefaults.BracedPanelHeight;

        /// <summary>Height of one central empty space, inches. Default 40 in.</summary>
        public double CentralEmptySpaceHeight { get; set; } = CantileverLineDefaults.CentralEmptySpaceHeight;

        public CantileverBraceBodyKind BraceKind { get; set; } = CantileverBraceBodyKind.ColdRolledRound;

        /// <summary>
        /// Catalogued brace section, as text. Only read under <see cref="CantileverBraceBodyKind.StructuralSection"/>.
        ///
        /// It has NO default: the owner registered no production id for it, and inventing one would be
        /// indistinguishable from an approved value. A structural brace with no section is rejected.
        /// </summary>
        public string BraceSectionId { get; set; }

        public CantileverColdRolledBraceDesign ColdRolled { get; set; } = new CantileverColdRolledBraceDesign();

        public CantileverBracingDesign DeepCopy() =>
            new CantileverBracingDesign
            {
                SeparatorSectionId = SeparatorSectionId,
                PanelCountMode = PanelCountMode,
                ManualPanelCount = ManualPanelCount,
                BracedPanelHeight = BracedPanelHeight,
                CentralEmptySpaceHeight = CentralEmptySpaceHeight,
                BraceKind = BraceKind,
                BraceSectionId = BraceSectionId,
                ColdRolled = ColdRolled?.DeepCopy() ?? new CantileverColdRolledBraceDesign()
            };
    }

    /// <summary>
    /// The topology every station of a line shares.
    ///
    /// It exists as its own type because sharing is the point: a line whose stations had different column
    /// heights, level counts or clears would not be a line, it would be a set of stations that happen to be
    /// drawn together (ADR-0027, D1).
    ///
    /// Note what it does NOT carry: a list of levels. It carries a <see cref="LevelCount"/>, and the per-cell
    /// arm overrides live on the LINE, indexed by station as well as by level and side — because the line's
    /// matrix has three dimensions and the station's had two.
    /// </summary>
    public sealed class CantileverLineStationTopologyDesign
    {
        public CantileverStationFaceMode FaceMode { get; set; } = CantileverStationFaceMode.Single;

        /// <summary>Under <c>Single</c>, which side carries the base and every arm. Dormant under <c>Double</c>.</summary>
        public CantileverArmSide SingleSide { get; set; } = CantileverArmSide.PositiveY;

        /// <summary>The column, its bottom plate, the base of one side and the connection. No height.</summary>
        public CantileverStationColumnBaseTemplateDesign ColumnBaseTemplate { get; set; } =
            new CantileverStationColumnBaseTemplateDesign();

        /// <summary>How many levels every station has. At least one.</summary>
        public int LevelCount { get; set; } = 1;

        /// <summary>Base-zero index of the column regular punch the first level bolts its lowest row to.</summary>
        public int FirstLevelPunchIndex { get; set; }

        /// <summary>The vertical clear asked for between levels, inches. Body top to body bottom.</summary>
        public double RequestedClearHeight { get; set; }

        /// <summary>Fraction of the clear the column leaves above the top level. Default 1/3, never less.</summary>
        public double TopClearFactor { get; set; } = CantileverStationDefaults.TopClearFactor;

        /// <summary>Automatic or manual COMMON height. One height for the whole line (ADR-0027, D2).</summary>
        public CantileverStationColumnHeightDesign ColumnHeight { get; set; } =
            new CantileverStationColumnHeightDesign();

        public CantileverLineStationTopologyDesign DeepCopy() =>
            new CantileverLineStationTopologyDesign
            {
                FaceMode = FaceMode,
                SingleSide = SingleSide,
                ColumnBaseTemplate =
                    ColumnBaseTemplate?.DeepCopy() ?? new CantileverStationColumnBaseTemplateDesign(),
                LevelCount = LevelCount,
                FirstLevelPunchIndex = FirstLevelPunchIndex,
                RequestedClearHeight = RequestedClearHeight,
                TopClearFactor = TopClearFactor,
                ColumnHeight = ColumnHeight?.DeepCopy() ?? new CantileverStationColumnHeightDesign()
            };
    }

    /// <summary>
    /// One cell of the line's arm matrix that DIFFERS from the default.
    ///
    /// Persisted as a sparse list and not as a three-dimensional array, because the contract is «only
    /// overrides that differ» (ADR-0026, D3, extended to the third dimension). A dense array would store the
    /// default N×M×2 times and lose the distinction between «pinned on purpose» and «never edited».
    /// </summary>
    public sealed class CantileverArmCellOverride
    {
        public int StationIndex { get; set; }

        public int LevelIndex { get; set; }

        public CantileverArmSide Side { get; set; }

        public CantileverArmTemplateDesign Arm { get; set; }

        public CantileverArmCellOverride DeepCopy() =>
            new CantileverArmCellOverride
            {
                StationIndex = StationIndex,
                LevelIndex = LevelIndex,
                Side = Side,
                Arm = Arm?.DeepCopy()
            };
    }

    /// <summary>
    /// The editable intent of a whole Cantilever LINE: the root aggregate of the system.
    ///
    /// This is what gets persisted, listed, edited and drawn. A station is NOT the aggregate: it has no
    /// identity of its own and no document of its own, and its internal identity is derived from
    /// <c>LineId + StationIndex</c> (owner decision `OWNER_DECIDED_CANTILEVER_IDENTITY_PER_LINE`).
    ///
    /// It holds no resolved coordinate — no elevation, no punch index, no cut length, no column height, no
    /// envelope. Every one of those is derived, and a design that stored them would store an answer its own
    /// inputs already determine. Same rule every Cantilever design has followed since I-37A.
    ///
    /// <see cref="LateralStationIndex"/> is deliberately ABSENT: which station the lateral view shows is
    /// view/editor state, it is stamped on the drawing envelope, and it must not change the BOM or the
    /// physical signature of the line.
    /// </summary>
    public sealed class CantileverLineDesign
    {
        /// <summary>
        /// The ONE identity of the line, shared by every view block it materialises.
        ///
        /// One GUID per line and none per station. Duplicating the line mints ONE new GUID for the whole
        /// group of views — never one per block and never one per station.
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Free-text name the user gives the line. Shown in the library list.</summary>
        public string Name { get; set; }

        /// <summary>How many stations. At least two: with one there is no interval, and no interval means no bracing.</summary>
        public int StationCount { get; set; } = CantileverLineDefaults.MinimumStationCount;

        /// <summary>
        /// Centre-to-centre distance between columns, inches. Default 48 in, finite and positive.
        ///
        /// It is NOT the length of a separator: that is derived from the punch datums of the two column
        /// plates, which sit on the INNER faces (ADR-0027, D5).
        /// </summary>
        public double ColumnCentreSpacing { get; set; } = CantileverLineDefaults.ColumnCentreSpacing;

        /// <summary>The topology every station shares.</summary>
        public CantileverLineStationTopologyDesign StationTopology { get; set; } =
            new CantileverLineStationTopologyDesign();

        /// <summary>The arm every cell uses unless it carries an override.</summary>
        public CantileverArmTemplateDesign DefaultArmTemplate { get; set; } = new CantileverArmTemplateDesign();

        /// <summary>Only the cells that differ from the default.</summary>
        public List<CantileverArmCellOverride> ArmCellOverrides { get; set; } =
            new List<CantileverArmCellOverride>();

        public CantileverBracingDesign Bracing { get; set; } = new CantileverBracingDesign();

        /// <summary>How many intervals a line of this length has. Derived, never stored.</summary>
        public int IntervalCount => Math.Max(0, StationCount - 1);

        /// <summary>The longitudinal origin of one station. `i × ColumnCentreSpacing` (ADR-0027, D1).</summary>
        public double StationOriginX(int stationIndex)
        {
            if (stationIndex < 0 || stationIndex >= StationCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(stationIndex), stationIndex,
                    "El indice de estacion esta fuera de la linea (0.." + (StationCount - 1) + ").");
            }

            return stationIndex * ColumnCentreSpacing;
        }

        public CantileverLineDesign DeepCopy() =>
            new CantileverLineDesign
            {
                Id = Id,
                Name = Name,
                StationCount = StationCount,
                ColumnCentreSpacing = ColumnCentreSpacing,
                StationTopology = StationTopology?.DeepCopy() ?? new CantileverLineStationTopologyDesign(),
                DefaultArmTemplate = DefaultArmTemplate?.DeepCopy() ?? new CantileverArmTemplateDesign(),
                ArmCellOverrides = (ArmCellOverrides ?? new List<CantileverArmCellOverride>())
                    .Where(o => o != null)
                    .Select(o => o.DeepCopy())
                    .ToList(),
                Bracing = Bracing?.DeepCopy() ?? new CantileverBracingDesign()
            };

        /// <summary>
        /// A copy with a NEW identity, for duplication.
        ///
        /// One new GUID for the whole line, so its three view blocks stay a group. Minting one per block or
        /// per station is exactly what the owner's identity decision forbids.
        /// </summary>
        public CantileverLineDesign DuplicateWithNewIdentity()
        {
            var copy = DeepCopy();
            copy.Id = Guid.NewGuid();
            return copy;
        }

        /// <summary>The sides that carry a base and arms. Delegates to the station rule, which fails closed.</summary>
        public bool TryActiveSides(out IReadOnlyList<CantileverArmSide> sides)
        {
            var topology = StationTopology ?? new CantileverLineStationTopologyDesign();

            return new CantileverStationDesign
            {
                FaceMode = topology.FaceMode,
                SingleSide = topology.SingleSide
            }.TryActiveSides(out sides);
        }

        /// <summary>
        /// The override of one cell, or null.
        ///
        /// Linear over a SPARSE list on purpose: the list holds only what differs, so it is short, and an
        /// index would be a second structure to keep in step with the design.
        /// </summary>
        public CantileverArmTemplateDesign OverrideFor(int stationIndex, int levelIndex, CantileverArmSide side) =>
            (ArmCellOverrides ?? new List<CantileverArmCellOverride>())
                .Where(o => o != null &&
                            o.StationIndex == stationIndex &&
                            o.LevelIndex == levelIndex &&
                            o.Side == side)
                .Select(o => o.Arm)
                .FirstOrDefault();

        /// <summary>
        /// The arm in force in one cell: its override, or the line default.
        ///
        /// THE one place the fallback is expressed, exactly as I-37C did for a single station.
        /// </summary>
        public CantileverArmTemplateDesign EffectiveArm(int stationIndex, int levelIndex, CantileverArmSide side) =>
            OverrideFor(stationIndex, levelIndex, side) ?? DefaultArmTemplate;

        /// <summary>
        /// Builds the I-37C station design for one station.
        ///
        /// The ONE adapter from a line to a station, and the reason the line does not duplicate the station's
        /// resolver: the station is resolved by I-37C, unchanged. The per-cell overrides of THIS station are
        /// projected onto the station's own two-dimensional level list, so the station never learns that a
        /// line exists.
        /// </summary>
        public CantileverStationDesign ToStationDesign(int stationIndex)
        {
            if (stationIndex < 0 || stationIndex >= StationCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(stationIndex), stationIndex, "El indice de estacion esta fuera de la linea.");
            }

            var topology = StationTopology ?? new CantileverLineStationTopologyDesign();
            var levels = new List<CantileverStationLevelDesign>(Math.Max(0, topology.LevelCount));

            for (var level = 0; level < Math.Max(0, topology.LevelCount); level++)
            {
                var design = new CantileverStationLevelDesign();

                foreach (var side in new[] { CantileverArmSide.PositiveY, CantileverArmSide.NegativeY })
                {
                    var over = OverrideFor(stationIndex, level, side);

                    if (over != null)
                    {
                        design.SetOverride(side, over.DeepCopy());
                    }
                }

                levels.Add(design);
            }

            return new CantileverStationDesign
            {
                FaceMode = topology.FaceMode,
                SingleSide = topology.SingleSide,
                ColumnBaseTemplate =
                    topology.ColumnBaseTemplate?.DeepCopy() ?? new CantileverStationColumnBaseTemplateDesign(),
                FirstLevelPunchIndex = topology.FirstLevelPunchIndex,
                RequestedClearHeight = topology.RequestedClearHeight,
                TopClearFactor = topology.TopClearFactor,
                ColumnHeight = topology.ColumnHeight?.DeepCopy() ?? new CantileverStationColumnHeightDesign(),
                DefaultArmTemplate = DefaultArmTemplate?.DeepCopy() ?? new CantileverArmTemplateDesign(),
                Levels = levels
            };
        }

        /// <summary>
        /// The same station design, forced to a MANUAL height.
        ///
        /// Used by the second pass of the line resolution: once the common height is known, every station is
        /// re-resolved with it. Manual rather than automatic on purpose — an automatic station would compute
        /// its own minimum again and the whole point is that they all get the SAME one (ADR-0027, D2).
        /// </summary>
        public CantileverStationDesign ToStationDesignAtHeight(int stationIndex, double commonHeight)
        {
            var design = ToStationDesign(stationIndex);

            design.ColumnHeight = new CantileverStationColumnHeightDesign
            {
                Mode = CantileverStationColumnHeightMode.Manual,
                ManualHeight = commonHeight
            };

            return design;
        }
    }

    /// <summary>
    /// The line and bracing constants the owner approved (I-37D). Same terms as the two constant classes
    /// before it: every value is reachable through a design property, so changing one is an edit.
    /// </summary>
    public static class CantileverLineDefaults
    {
        /// <summary>A line needs two stations: with one there is no interval, and no interval means no bracing.</summary>
        public const int MinimumStationCount = 2;

        /// <summary>Centre-to-centre spacing between columns, inches.</summary>
        public const double ColumnCentreSpacing = 48.0;

        /// <summary>Height of one braced panel, inches.</summary>
        public const double BracedPanelHeight = 40.0;

        /// <summary>Height of one central empty space, inches.</summary>
        public const double CentralEmptySpaceHeight = 40.0;

        /// <summary>
        /// The height the panel-count rule subtracts before dividing, inches.
        ///
        /// It is not a margin anybody measures: it is the intercept of the product rule
        /// <c>max(1, ceil((H − 72) / 60))</c>, which reproduces the twelve rows of the approved table. Both
        /// numbers live here so the rule reads as one thing.
        /// </summary>
        public const double PanelCountBaseHeight = 72.0;

        /// <summary>The height step of the panel-count rule, inches.</summary>
        public const double PanelCountHeightStep = 60.0;

        /// <summary>
        /// The EXACT id of the default separator section: the lightest 4 in channel.
        ///
        /// Unambiguous by measurement, not by preference: the catalogue holds exactly four C4 rows and their
        /// weights do not tie. It is an id and never a designation — nothing searches for a separator by text
        /// (decision 12.19).
        /// </summary>
        public const string SeparatorSectionId = "AISC-C-C4X4_5";

        // ---- the separator's column plate --------------------------------------------------------------

        /// <summary>Transverse width of the separator's column plate, inches.</summary>
        public const double SeparatorColumnPlateWidth = 3.0;

        /// <summary>Vertical height of the separator's column plate, inches.</summary>
        public const double SeparatorColumnPlateHeight = 3.0;

        /// <summary>Thickness of the separator's column plate, inches. Three eighths.</summary>
        public const double SeparatorColumnPlateThickness = 0.375;

        /// <summary>Diameter of every separator and brace punch, inches. Nine sixteenths.</summary>
        public const double SeparatorPunchDiameter = 0.5625;

        // ---- the separator ----------------------------------------------------------------------------

        /// <summary>Distance from a separator end to its column punch, inches.</summary>
        public const double SeparatorColumnPunchEdgeDistance = 1.25;

        /// <summary>Distance from the column punch to the brace punch of the same end, inches.</summary>
        public const double SeparatorBracePunchOffset = 4.0;

        // ---- the braces -------------------------------------------------------------------------------

        /// <summary>Distance from a structural brace end to its punch, inches.</summary>
        public const double BracePunchEdgeDistance = 1.25;

        /// <summary>Diameter of a cold-rolled rod, inches. Three quarters.</summary>
        public const double ColdRolledBraceDiameter = 0.75;

        // ---- the cold-rolled end adapter ---------------------------------------------------------------

        /// <summary>Leg of the adapter's angle, inches. Two by two.</summary>
        public const double AdapterAngleLeg = 2.0;

        /// <summary>Length the adapter's angle is cut to, inches.</summary>
        public const double AdapterCutLength = 2.0;

        /// <summary>Thickness of the adapter's angle, inches. Three sixteenths.</summary>
        public const double AdapterAngleThickness = 0.1875;

        /// <summary>How many adapters one cold-rolled brace needs. One per end.</summary>
        public const int AdaptersPerColdRolledBrace = 2;

        /// <summary>How many gussets one adapter carries. One at each longitudinal end of the angle.</summary>
        public const int GussetsPerAdapter = 2;

        /// <summary>
        /// The gauge of an adapter gusset, as a NUMBER and not a thickness.
        ///
        /// Ten is the product authority. The repository has no gauge table —`gauge` is a free-text per-row
        /// attribute in the legacy catalogues and nothing converts it— so the number is carried as identity
        /// and description, and no decimal thickness is invented for it (decision 12.30). The 2D view draws
        /// the gusset's outline without needing one.
        /// </summary>
        public const int GussetGaugeNumber = 10;
    }
}
