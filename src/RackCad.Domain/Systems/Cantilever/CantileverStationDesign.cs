using System;
using System.Collections.Generic;
using System.Linq;

namespace RackCad.Domain.Systems.Cantilever
{
    /// <summary>
    /// How many faces a station carries loads on.
    ///
    /// Two values and no third: a station is single-faced or double-faced. What a double face is NOT is two
    /// stations — there is exactly ONE column and ONE column bottom plate either way, and the second face
    /// adds a mirrored base and one arm per level (ADR-0026, D1).
    /// </summary>
    public enum CantileverStationFaceMode
    {
        /// <summary>One face. Product label: «góndola sencilla».</summary>
        Single = 0,

        /// <summary>Two faces sharing one column. Product label: «góndola doble».</summary>
        Double = 1
    }

    /// <summary>Where the resolved column height comes from.</summary>
    public enum CantileverStationColumnHeightMode
    {
        /// <summary>The station computes it: the resolved height IS the minimum, with no commercial rounding.</summary>
        Automatic = 0,

        /// <summary>The user supplies it. It may exceed the minimum; below the minimum the design is BLOCKED.</summary>
        Manual = 1
    }

    /// <summary>
    /// Editable intent of the station's column height.
    ///
    /// <see cref="ManualHeight"/> is nullable and REQUIRED under <see cref="CantileverStationColumnHeightMode.Manual"/>.
    /// It is not a number with a default because there is no approved default for it: the station's own
    /// minimum is the only height anybody has authorised, and that one is computed rather than typed.
    /// </summary>
    public sealed class CantileverStationColumnHeightDesign
    {
        public CantileverStationColumnHeightMode Mode { get; set; } = CantileverStationColumnHeightMode.Automatic;

        /// <summary>
        /// The height the user typed, inches. Only read under <c>Manual</c>, where it is mandatory.
        ///
        /// Under <c>Automatic</c> it is DORMANT DATA and deliberately not validated: a value left behind by
        /// an earlier edit must not reject a station that no longer reads it. Same rule the arm's end plate
        /// thickness follows under <c>None</c>.
        /// </summary>
        public double? ManualHeight { get; set; }

        public CantileverStationColumnHeightDesign DeepCopy() =>
            new CantileverStationColumnHeightDesign
            {
                Mode = Mode,
                ManualHeight = ManualHeight
            };
    }

    /// <summary>
    /// The column–base configuration of a station, WITHOUT a height.
    ///
    /// It is a template and not a <see cref="CantileverColumnBaseDesign"/> for one reason: in a station the
    /// height is COMPUTED. Keeping a height field here would be a second authority for one number, and the
    /// unused one is the one somebody eventually edits (ADR-0026, D2). The station computes the height and
    /// then builds the real design explicitly.
    /// </summary>
    public sealed class CantileverStationColumnBaseTemplateDesign
    {
        /// <summary>Catalogued column section, as the text of its <c>StructuralSectionId</c> (ADR-0024, D1).</summary>
        public string ColumnSectionId { get; set; }

        /// <summary>The column's bottom plate. Its own thickness, shared by both faces.</summary>
        public CantileverPlateDesign ColumnBottomPlate { get; set; } = new CantileverPlateDesign();

        /// <summary>
        /// The base of ONE side. In a double station BOTH sides use this same configuration, and the
        /// negative one is derived by mirror — which is what makes "the same base on both sides" true by
        /// construction instead of by two fields kept in step.
        /// </summary>
        public CantileverBaseDesign Base { get; set; } = new CantileverBaseDesign();

        /// <summary>The column–base connection, including the punch parameters the whole station inherits.</summary>
        public CantileverColumnBaseConnectionDesign Connection { get; set; } =
            new CantileverColumnBaseConnectionDesign();

        /// <summary>
        /// Whether choosing a column also chooses the base.
        ///
        /// A Cantilever is normally built from ONE profile, so the base follows the column until the user picks
        /// a base by hand; from then on the base is theirs and a column change leaves it alone. It was motivo 4
        /// of the round-1 rejection.
        ///
        /// <para>It is stored INTENT and is never derived from «the two ids happen to be equal». Two sections
        /// can coincide by accident, and a user who deliberately chose the same base would lose that decision
        /// the first time they changed the column. It is PERSISTED for the mirror reason: reopening a line must
        /// not change the rule under it. A JSON with no key leaves it <c>true</c>, which is what a new design
        /// says — and no saved design predates the field, because I-37D has not been integrated.</para>
        /// </summary>
        public bool BaseFollowsColumn { get; set; } = true;

        public CantileverStationColumnBaseTemplateDesign DeepCopy() =>
            new CantileverStationColumnBaseTemplateDesign
            {
                ColumnSectionId = ColumnSectionId,
                ColumnBottomPlate = ColumnBottomPlate?.DeepCopy() ?? new CantileverPlateDesign(),
                Base = Base?.DeepCopy() ?? new CantileverBaseDesign(),
                Connection = Connection?.DeepCopy() ?? new CantileverColumnBaseConnectionDesign(),
                BaseFollowsColumn = BaseFollowsColumn
            };

        /// <summary>
        /// Builds the real I-37A design for a resolved height.
        ///
        /// The ONLY place a <see cref="CantileverColumnBaseDesign"/> is composed from a station. Two sites
        /// composing the same design is how they drift, so this one is it (ADR-0026, D2).
        /// </summary>
        public CantileverColumnBaseDesign ToColumnBaseDesign(double resolvedColumnHeight) =>
            new CantileverColumnBaseDesign
            {
                Column = new CantileverColumnDesign
                {
                    SectionId = ColumnSectionId,
                    Height = resolvedColumnHeight,
                    BottomPlate = ColumnBottomPlate?.DeepCopy() ?? new CantileverPlateDesign()
                },
                Base = Base?.DeepCopy() ?? new CantileverBaseDesign(),
                Connection = Connection?.DeepCopy() ?? new CantileverColumnBaseConnectionDesign()
            };
    }

    /// <summary>
    /// The mounting-plate part of an arm template: everything about the plate EXCEPT which punches it uses.
    ///
    /// <c>LowerColumnPunchIndex</c> is absent on purpose. The station computes it per level from the clear
    /// height, so a template carrying one would carry an index nobody reads (ADR-0026, D2).
    /// </summary>
    public sealed class CantileverArmMountingPlateTemplateDesign
    {
        /// <summary>Thickness, inches.</summary>
        public double Thickness { get; set; } = CantileverDefaults.PlateThickness;

        /// <summary>How many column punch elevations the plate uses, counting upwards. Minimum 2.</summary>
        public int VerticalPunchCount { get; set; } = CantileverDefaults.ArmVerticalPunchCount;

        /// <summary>
        /// Margen de los troqueles extremos a los bordes de la placa, en pulgadas. Dos por omisión, aprobadas
        /// por el dueño en la ronda 3; hasta entonces era obligatorio y sin default a propósito.
        /// </summary>
        public double? VerticalEndOffset { get; set; } = CantileverDefaults.ArmMountingPlateVerticalEndOffset;

        public CantileverArmMountingPlateTemplateDesign DeepCopy() =>
            new CantileverArmMountingPlateTemplateDesign
            {
                Thickness = Thickness,
                VerticalPunchCount = VerticalPunchCount,
                VerticalEndOffset = VerticalEndOffset
            };
    }

    /// <summary>
    /// The editable intent of an arm as a station holds it: body, mounting plate template and end plate.
    ///
    /// Neither the SIDE nor the punch index lives here — the station owns both. That is what lets one
    /// template serve the default of every cell on both faces (ADR-0026, D2 and D3).
    /// </summary>
    public sealed class CantileverArmTemplateDesign
    {
        public CantileverArmBodyDesign Body { get; set; } = new CantileverArmBodyDesign();

        public CantileverArmMountingPlateTemplateDesign MountingPlate { get; set; } =
            new CantileverArmMountingPlateTemplateDesign();

        public CantileverArmEndPlateDesign EndPlate { get; set; } = new CantileverArmEndPlateDesign();

        public CantileverArmTemplateDesign DeepCopy() =>
            new CantileverArmTemplateDesign
            {
                Body = Body?.DeepCopy() ?? new CantileverArmBodyDesign(),
                MountingPlate = MountingPlate?.DeepCopy() ?? new CantileverArmMountingPlateTemplateDesign(),
                EndPlate = EndPlate?.DeepCopy() ?? new CantileverArmEndPlateDesign()
            };
    }

    /// <summary>
    /// One level of a station: its per-cell arm overrides, and nothing else.
    ///
    /// It carries NO elevation and NO punch index. Both are computed by the level layout from the requested
    /// clear height, and a level that stored either would store an answer its own inputs determine
    /// (ADR-0026, D2 and D4).
    ///
    /// A level is identified by its POSITION in <see cref="CantileverStationDesign.Levels"/>. I-37C
    /// deliberately introduces no persisted level id: ids are for things that survive reordering, and
    /// nothing in this initiative reorders levels.
    /// </summary>
    public sealed class CantileverStationLevelDesign
    {
        /// <summary>
        /// Override for the arm on the +Y side, or null to use the station default.
        ///
        /// Null means "the default", not "empty": <c>EffectiveArm = CellOverride ?? DefaultArmTemplate</c>.
        /// Only differences are persisted, which is what stops a default change from silently not reaching
        /// the cells that were never edited (ADR-0026, D3).
        /// </summary>
        public CantileverArmTemplateDesign PositiveYOverride { get; set; }

        /// <summary>Override for the arm on the −Y side, or null to use the station default.</summary>
        public CantileverArmTemplateDesign NegativeYOverride { get; set; }

        public CantileverStationLevelDesign DeepCopy() =>
            new CantileverStationLevelDesign
            {
                PositiveYOverride = PositiveYOverride?.DeepCopy(),
                NegativeYOverride = NegativeYOverride?.DeepCopy()
            };

        /// <summary>
        /// The override for one side, or null. Keeps the side→property mapping in ONE place.
        ///
        /// An exhaustive <c>switch</c> with a throwing default. The earlier form was
        /// <c>side == PositiveY ? positive : negative</c>, so a third side value would silently have read the
        /// NEGATIVE override — a wrong answer that looks like a right one.
        /// </summary>
        public CantileverArmTemplateDesign OverrideFor(CantileverArmSide side)
        {
            switch (side)
            {
                case CantileverArmSide.PositiveY:
                    return PositiveYOverride;
                case CantileverArmSide.NegativeY:
                    return NegativeYOverride;
                default:
                    throw Undefined(side);
            }
        }

        /// <summary>
        /// Sets or clears the override for one side. The only writer of the two properties, and exhaustive for
        /// the same reason: an undeclared side must not land in the negative slot.
        /// </summary>
        public void SetOverride(CantileverArmSide side, CantileverArmTemplateDesign value)
        {
            switch (side)
            {
                case CantileverArmSide.PositiveY:
                    PositiveYOverride = value;
                    break;
                case CantileverArmSide.NegativeY:
                    NegativeYOverride = value;
                    break;
                default:
                    throw Undefined(side);
            }
        }

        private static ArgumentOutOfRangeException Undefined(CantileverArmSide side) =>
            new ArgumentOutOfRangeException(
                nameof(side), side,
                "El lado '" + side + "' no tiene celda en un nivel. Anadir un lado exige escribirla aqui.");
    }

    /// <summary>
    /// The editable intent of a whole Cantilever STATION: one column, one or two bases, and a shared list of
    /// levels each carrying one arm per active face.
    ///
    /// It holds no resolved coordinate — no elevation, no punch index, no plate height, no column height, no
    /// envelope. Every one of those is derived by the Application resolver, and a design that stored them
    /// would be storing an answer its own inputs already determine. That is the same rule
    /// <see cref="CantileverColumnBaseDesign"/> follows.
    ///
    /// What is deliberately NOT here, because I-37C does not implement it: the longitudinal position of the
    /// station, its index inside a run, spacers, braces, neighbouring stations, views, persistence and
    /// AutoCAD. Adding any of them is a later initiative, not a property (ADR-0026, D7).
    /// </summary>
    public sealed class CantileverStationDesign
    {
        /// <summary>One face or two.</summary>
        public CantileverStationFaceMode FaceMode { get; set; } = CantileverStationFaceMode.Single;

        /// <summary>
        /// Under <see cref="CantileverStationFaceMode.Single"/>, WHICH side carries the base and every arm.
        ///
        /// Under <c>Double</c> it is DORMANT DATA: both sides are active, so the value is not read and not
        /// validated. Keeping one field instead of two —"the single side" and "the double sides"— is what
        /// stops the two from disagreeing.
        /// </summary>
        public CantileverArmSide SingleSide { get; set; } = CantileverArmSide.PositiveY;

        /// <summary>The column, its bottom plate, the base of one side and the connection. No height.</summary>
        public CantileverStationColumnBaseTemplateDesign ColumnBaseTemplate { get; set; } =
            new CantileverStationColumnBaseTemplateDesign();

        /// <summary>
        /// Index, BASE ZERO, of the column regular punch elevation the FIRST level bolts its lowest row to.
        ///
        /// The elevation itself is not stored: it is derived from this index through the regular punch grid.
        /// Storing both would let a saved elevation contradict the grid it came from (ADR-0026, D2).
        /// </summary>
        public int FirstLevelPunchIndex { get; set; }

        /// <summary>
        /// The vertical clear the user asks for BETWEEN LEVELS, inches: body top of the lower arm to body
        /// bottom of the upper arm, measured in the connection plane (ADR-0026, D4).
        ///
        /// Global for the station in the MVP. Per-level clears are a later initiative.
        /// </summary>
        public double RequestedClearHeight { get; set; }

        /// <summary>
        /// Fraction of <see cref="RequestedClearHeight"/> the column must leave ABOVE the last thing the top
        /// level occupies. Default one third, and never less (ADR-0026, D6).
        /// </summary>
        public double TopClearFactor { get; set; } = CantileverStationDefaults.TopClearFactor;

        /// <summary>Automatic or manual column height.</summary>
        public CantileverStationColumnHeightDesign ColumnHeight { get; set; } =
            new CantileverStationColumnHeightDesign();

        /// <summary>The arm every cell uses unless it carries an override.</summary>
        public CantileverArmTemplateDesign DefaultArmTemplate { get; set; } = new CantileverArmTemplateDesign();

        /// <summary>
        /// The levels, bottom to top. ONE shared list: in a double station both faces use it, which is what
        /// makes "the two sides share their levels" structural rather than enforced (ADR-0026, D1).
        ///
        /// There is no <c>LevelCount</c> property: the count IS <c>Levels.Count</c>. Two authorities for one
        /// cardinality is how a list ends up disagreeing with its own length.
        /// </summary>
        public List<CantileverStationLevelDesign> Levels { get; set; } =
            new List<CantileverStationLevelDesign> { new CantileverStationLevelDesign() };

        /// <summary>How many levels this station has. Derived, never stored.</summary>
        public int LevelCount => Levels?.Count ?? 0;

        public CantileverStationDesign DeepCopy() =>
            new CantileverStationDesign
            {
                FaceMode = FaceMode,
                SingleSide = SingleSide,
                ColumnBaseTemplate =
                    ColumnBaseTemplate?.DeepCopy() ?? new CantileverStationColumnBaseTemplateDesign(),
                FirstLevelPunchIndex = FirstLevelPunchIndex,
                RequestedClearHeight = RequestedClearHeight,
                TopClearFactor = TopClearFactor,
                ColumnHeight = ColumnHeight?.DeepCopy() ?? new CantileverStationColumnHeightDesign(),
                DefaultArmTemplate = DefaultArmTemplate?.DeepCopy() ?? new CantileverArmTemplateDesign(),
                Levels = (Levels ?? new List<CantileverStationLevelDesign>())
                    .Select(l => l?.DeepCopy() ?? new CantileverStationLevelDesign())
                    .ToList()
            };

        /// <summary>
        /// The sides that actually carry a base and arms.
        ///
        /// THE authority for "which cells exist". A single station has one side and the opposite one is not a
        /// cell at all — never an inactive cell, never a false one (ADR-0026, D3).
        ///
        /// It THROWS for an undeclared face mode rather than falling through to the single case. The earlier
        /// form was <c>FaceMode == Double ? both : single</c>, which quietly treated any unknown value as a
        /// single-faced station — the exact failure the rest of Cantilever spent two initiatives closing.
        /// <see cref="TryActiveSides"/> is the reporting form for callers that hold user input.
        /// </summary>
        public IReadOnlyList<CantileverArmSide> ActiveSides()
        {
            if (!TryActiveSides(out var sides))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(FaceMode), FaceMode,
                    "El modo de cara '" + FaceMode +
                    "' no tiene regla de lados activos. Anadir un modo exige escribirla aqui.");
            }

            return sides;
        }

        /// <summary>
        /// The same question, reporting failure instead of throwing.
        ///
        /// It exists because the face mode reaches this type from a persisted document eventually, and a value
        /// somebody stored has to come back as a diagnostic rather than as an exception.
        /// </summary>
        public bool TryActiveSides(out IReadOnlyList<CantileverArmSide> sides)
        {
            switch (FaceMode)
            {
                case CantileverStationFaceMode.Double:
                    sides = new[] { CantileverArmSide.PositiveY, CantileverArmSide.NegativeY };
                    return true;

                case CantileverStationFaceMode.Single:
                    if (SingleSide != CantileverArmSide.PositiveY &&
                        SingleSide != CantileverArmSide.NegativeY)
                    {
                        sides = Array.Empty<CantileverArmSide>();
                        return false;
                    }

                    sides = new[] { SingleSide };
                    return true;

                default:
                    sides = Array.Empty<CantileverArmSide>();
                    return false;
            }
        }

        /// <summary>
        /// The arm template in force for one cell: its override, or the station default.
        ///
        /// The ONE place the fallback is expressed. A second copy of <c>?? Default</c> anywhere else is a
        /// second answer to the same question.
        /// </summary>
        public CantileverArmTemplateDesign EffectiveArm(int levelIndex, CantileverArmSide side)
        {
            var levels = Levels ?? new List<CantileverStationLevelDesign>();

            if (levelIndex < 0 || levelIndex >= levels.Count)
            {
                return null;
            }

            return levels[levelIndex]?.OverrideFor(side) ?? DefaultArmTemplate;
        }
    }

    /// <summary>
    /// The station constants the owner approved (I-37C). Same terms as <see cref="CantileverDefaults"/>:
    /// every value is reachable through a design property, so changing one is an edit and not a rebuild.
    /// </summary>
    public static class CantileverStationDefaults
    {
        /// <summary>
        /// Fraction of the requested clear the column leaves above the last occupied point.
        ///
        /// One third, by the owner's decision, and it is also the FLOOR: a smaller factor is rejected rather
        /// than accepted quietly, because the margin exists so the top load can be placed at all.
        /// </summary>
        public const double TopClearFactor = 1.0 / 3.0;
    }
}
