using System;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>How much a diagnostic matters. Mirrors the shape of <c>SectionDiagnosticSeverity</c>.</summary>
    public enum CantileverDiagnosticSeverity
    {
        /// <summary>Worth knowing; the result is what it claims to be.</summary>
        Info = 0,

        /// <summary>The result is usable but something about it must be shown to the user.</summary>
        Warning = 1,

        /// <summary>The design cannot be resolved. Nothing is drawn and nothing is counted.</summary>
        Blocking = 2
    }

    /// <summary>
    /// One thing the resolver wants the caller to know. <see cref="Code"/> is the stable token: tests key on
    /// it and never on the message, which is Spanish prose for the user and may be reworded.
    /// </summary>
    public sealed class CantileverDiagnostic
    {
        public CantileverDiagnostic(CantileverDiagnosticSeverity severity, string code, string message)
        {
            if (string.IsNullOrWhiteSpace(code))
            {
                throw new ArgumentException("Un diagnostico necesita su codigo estable.", nameof(code));
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Un diagnostico necesita su mensaje.", nameof(message));
            }

            Severity = severity;
            Code = code;
            Message = message;
        }

        public CantileverDiagnosticSeverity Severity { get; }

        /// <summary>Stable machine token, e.g. <c>CANT_SECTION_UNKNOWN</c>.</summary>
        public string Code { get; }

        /// <summary>Spanish description for the user.</summary>
        public string Message { get; }

        public bool IsBlocking => Severity == CantileverDiagnosticSeverity.Blocking;

        public static CantileverDiagnostic Info(string code, string message) =>
            new CantileverDiagnostic(CantileverDiagnosticSeverity.Info, code, message);

        public static CantileverDiagnostic Warning(string code, string message) =>
            new CantileverDiagnostic(CantileverDiagnosticSeverity.Warning, code, message);

        public static CantileverDiagnostic Blocking(string code, string message) =>
            new CantileverDiagnostic(CantileverDiagnosticSeverity.Blocking, code, message);

        public override string ToString() => "[" + Severity + "] " + Code + " — " + Message;
    }

    /// <summary>
    /// The stable diagnostic tokens of the Cantilever resolver.
    ///
    /// They exist as constants, and not as literals at the throw site, because a test that keys on a string
    /// typed twice is a test that passes while the product is broken.
    /// </summary>
    public static class CantileverDiagnostics
    {
        /// <summary>The design left a section id empty.</summary>
        public const string SectionIdMissing = "CANT_SECTION_ID_MISSING";

        /// <summary>The stored text is not a well-formed section id.</summary>
        public const string SectionIdInvalid = "CANT_SECTION_ID_INVALID";

        /// <summary>The id is well formed but the catalogue does not have it.</summary>
        public const string SectionUnknown = "CANT_SECTION_UNKNOWN";

        /// <summary>
        /// The section resolved but is disabled. WARNING, never blocking: a design saved months ago must
        /// keep opening (owner decision 15 of I-36A), and substituting the section silently would change
        /// its geometry behind the user's back.
        /// </summary>
        public const string SectionDisabled = "CANT_SECTION_DISABLED";

        /// <summary>The section's family is not one this Cantilever design admits.</summary>
        public const string SectionFamilyNotEligible = "CANT_SECTION_FAMILY_NOT_ELIGIBLE";

        /// <summary>Column and base resolve, but the policy has no registered variant for the pair.</summary>
        public const string CombinationNotEligible = "CANT_COMBINATION_NOT_ELIGIBLE";

        /// <summary>The registered variant is not the one the design declares.</summary>
        public const string VariantMismatch = "CANT_VARIANT_MISMATCH";

        /// <summary>
        /// The registered variant declares an orientation that has no frame rule.
        ///
        /// It is reported rather than thrown so that a bad REGISTRATION reaches the user as a diagnostic
        /// like any other. The frame authority still throws if called directly with it, which covers the
        /// programmer path.
        /// </summary>
        public const string OrientationNotSupported = "CANT_ORIENTATION_NOT_SUPPORTED";

        /// <summary>The contour of a section carries a declared RackCad convention (ADR-0023).</summary>
        public const string SectionVisualDerived = "CANT_SECTION_VISUAL_DERIVED";

        /// <summary>A required parameter has no approved default and the design did not supply it.</summary>
        public const string RequiredParameterMissing = "CANT_REQUIRED_PARAMETER_MISSING";

        /// <summary>A length, thickness, pitch or offset is not a usable positive number.</summary>
        public const string ParameterNotPositive = "CANT_PARAMETER_NOT_POSITIVE";

        /// <summary>
        /// An edge offset is smaller than the punch RADIUS, so the hole would spill past the edge it is
        /// measured from.
        ///
        /// Its own code, and not <see cref="ParameterNotPositive"/>: the value IS positive, and a message
        /// saying otherwise would send the reader looking for the wrong thing. The offsets are measured
        /// edge-to-CENTRE, so the physical requirement is half a diameter, not a whole one.
        /// </summary>
        public const string EdgeOffsetBelowRadius = "CANT_EDGE_OFFSET_BELOW_RADIUS";

        /// <summary>
        /// A pitch is smaller than the punch diameter, so consecutive holes of one row would overlap.
        ///
        /// Pitch is centre-to-centre, so here the requirement is a whole diameter — that is exactly why this
        /// is a different code from <see cref="EdgeOffsetBelowRadius"/> and not a shared "too small".
        /// </summary>
        public const string PitchBelowDiameter = "CANT_PITCH_BELOW_DIAMETER";

        /// <summary>
        /// The two transverse punch columns are closer to each other than one diameter, so they would merge
        /// into a slot. Distinct from <see cref="PunchOutsideRearPlate"/>, which is about the PLATE.
        /// </summary>
        public const string PunchRowsOverlap = "CANT_PUNCH_ROWS_OVERLAP";

        /// <summary>Not even the first connection punch fits inside the base section envelope.</summary>
        public const string NoPunchFitsInBase = "CANT_NO_PUNCH_FITS_IN_BASE";

        /// <summary>
        /// The punch columns the COLUMN governs fall outside the rear plate the BASE can offer.
        ///
        /// Blocking on purpose: widening the plate would be inventing a rule nobody approved, and the
        /// combination is simply not a product.
        /// </summary>
        public const string PunchOutsideRearPlate = "CANT_PUNCH_OUTSIDE_REAR_PLATE";

        /// <summary>The column is too short to hold a single regular punch after the connection region.</summary>
        public const string NoRegularPunchFits = "CANT_NO_REGULAR_PUNCH_FITS";

        /// <summary>The column bottom plate is too shallow to hold a symmetric pair at the required offset.</summary>
        public const string NoBottomPlatePunchFits = "CANT_NO_BOTTOM_PLATE_PUNCH_FITS";

        // ---- I-37B: the arm ---------------------------------------------------------------------------

        /// <summary>The body arrangement has no placement rule. Added, never guessed.</summary>
        public const string ArmArrangementNotSupported = "CANT_ARM_ARRANGEMENT_NOT_SUPPORTED";

        /// <summary>The arm side has no frame rule.</summary>
        public const string ArmSideNotSupported = "CANT_ARM_SIDE_NOT_SUPPORTED";

        /// <summary>
        /// A paired arrangement was asked for with a section that is not a channel.
        ///
        /// Its own code because the placements of a paired body are written against the anatomy of a channel
        /// — the back of the web and the flange tips — and mean nothing on a W or an HSS.
        /// </summary>
        public const string ArmArrangementRequiresChannel = "CANT_ARM_ARRANGEMENT_REQUIRES_CHANNEL";

        /// <summary>The slope is negative or not finite. Zero is legal; downhill is not.</summary>
        public const string ArmSlopeInvalid = "CANT_ARM_SLOPE_INVALID";

        /// <summary>Fewer than two punch rows were asked for. One bolt line is a hinge, not a connection.</summary>
        public const string ArmVerticalCountTooSmall = "CANT_ARM_VERTICAL_COUNT_TOO_SMALL";

        /// <summary>The requested run of column punches falls outside the ones the column actually resolved.</summary>
        public const string ArmPunchIndexOutOfRange = "CANT_ARM_PUNCH_INDEX_OUT_OF_RANGE";

        /// <summary>The column resolved fewer regular punch elevations than the arm needs.</summary>
        public const string ArmNotEnoughColumnPunches = "CANT_ARM_NOT_ENOUGH_COLUMN_PUNCHES";

        /// <summary>The selected column punches are not one contiguous, evenly spaced run.</summary>
        public const string ArmPunchDatumsNotContiguous = "CANT_ARM_PUNCH_DATUMS_NOT_CONTIGUOUS";

        /// <summary>
        /// The body is deeper than the plate its selected punches can span.
        ///
        /// BLOCKING, and the message asks to raise the row count: stretching the plate without more holes
        /// would produce a piece that looks resolved and is not (ADR-0025, D6).
        /// </summary>
        public const string ArmPlateTooShortForBody = "CANT_ARM_PLATE_TOO_SHORT_FOR_BODY";

        /// <summary>The two channels of a paired body cannot be brought into contact without overlapping.</summary>
        public const string ArmChannelsCannotTouch = "CANT_ARM_CHANNELS_CANNOT_TOUCH";

        /// <summary><c>Stop</c> was asked for without a positive extra height.</summary>
        public const string ArmStopWithoutHeight = "CANT_ARM_STOP_WITHOUT_HEIGHT";

        /// <summary>An extra stop height was given for a mode that is not <c>Stop</c>, or it is not finite.</summary>
        public const string ArmEndPlateHeightWithoutStop = "CANT_ARM_END_PLATE_HEIGHT_WITHOUT_STOP";

        /// <summary>
        /// The end plate mode is not one this initiative declares.
        ///
        /// Its own code because the previous validation had guarded cases and no default: an undeclared value
        /// fell through without a word and was then materialised as a cap.
        /// </summary>
        public const string ArmEndPlateModeNotSupported = "CANT_ARM_END_PLATE_MODE_NOT_SUPPORTED";

        /// <summary>
        /// The slope is finite and non-negative but so steep that the arm's frame is undefined: the projection
        /// of world +Z onto its transverse plane vanishes.
        ///
        /// BLOCKING and reported, because it is USER INPUT. Letting the projection throw would surface an
        /// <c>InvalidOperationException</c> from a number somebody typed.
        /// </summary>
        public const string ArmSlopeFrameUndefined = "CANT_ARM_SLOPE_FRAME_UNDEFINED";

        /// <summary>The column assembly handed to the arm resolver is itself blocked.</summary>
        public const string ArmColumnAssemblyBlocked = "CANT_ARM_COLUMN_ASSEMBLY_BLOCKED";

        /// <summary>
        /// INFO, and always present on a sloped arm: the profile is cut SQUARE while the mounting plate is a
        /// vertical plane, so the start face is not flush against it.
        ///
        /// It is reported rather than hidden because the fix — a mitred end — is end preparation, which
        /// ADR-0024 and ADR-0025 both keep out of scope. Same spirit as
        /// <c>SG_CHANNEL_FLANGE_TAPER_NOT_MODELLED</c>: state the approximation instead of implying exactness.
        /// </summary>
        public const string ArmSquareCutAtSlopedPlate = "CANT_ARM_SQUARE_CUT_AT_SLOPED_PLATE";

        // ---- I-37C: the station -------------------------------------------------------------------------

        /// <summary>The face mode has no composition rule. Added, never guessed.</summary>
        public const string StationFaceModeNotSupported = "CANT_STATION_FACE_MODE_NOT_SUPPORTED";

        /// <summary>The column height mode has no rule.</summary>
        public const string StationHeightModeNotSupported = "CANT_STATION_HEIGHT_MODE_NOT_SUPPORTED";

        /// <summary>A station was handed no levels. One is the minimum; zero is not a station.</summary>
        public const string StationNoLevels = "CANT_STATION_NO_LEVELS";

        /// <summary>
        /// No candidate index satisfies the requested clear for a level.
        ///
        /// BLOCKING, because every alternative is worse. Shrinking the clear would give the user less space
        /// than they asked for, and dropping the level would give them fewer levels — both in silence.
        /// </summary>
        public const string StationLevelDoesNotFit = "CANT_STATION_LEVEL_DOES_NOT_FIT";

        /// <summary>
        /// <c>TopClearFactor</c> is below the approved floor of one third, or is not finite.
        ///
        /// Its own code because the value IS positive: a message about a non-positive number would send the
        /// reader looking for the wrong thing.
        /// </summary>
        public const string StationTopClearFactorTooSmall = "CANT_STATION_TOP_CLEAR_FACTOR_TOO_SMALL";

        /// <summary>A manual column height was asked for and not supplied.</summary>
        public const string StationManualHeightMissing = "CANT_STATION_MANUAL_HEIGHT_MISSING";

        /// <summary>
        /// The manual column height is below the minimum the levels need.
        ///
        /// BLOCKING, and the message says by how much. It is NOT normalised up to the minimum: the user asked
        /// for a specific column, and quietly building a different one is how a drawing stops matching its own
        /// inputs (ADR-0026, D6).
        /// </summary>
        public const string StationManualHeightBelowMinimum = "CANT_STATION_MANUAL_HEIGHT_BELOW_MINIMUM";

        /// <summary>
        /// The final resolve disagrees with the layout that sized the column.
        ///
        /// It should be unreachable, and it is BLOCKING precisely because of that: the two passes exist so the
        /// prediction can be CHECKED, and silently accepting a different answer is how a model starts lying
        /// (ADR-0026, D5).
        /// </summary>
        public const string StationFinalPassDiffersFromLayout = "CANT_STATION_FINAL_PASS_DIFFERS_FROM_LAYOUT";

        /// <summary>An arm of the station could not be resolved by I-37B. Its own diagnostics travel with it.</summary>
        public const string StationArmBlocked = "CANT_STATION_ARM_BLOCKED";

        /// <summary>The column–base of the station could not be resolved by I-37A.</summary>
        public const string StationColumnBaseBlocked = "CANT_STATION_COLUMN_BASE_BLOCKED";

        /// <summary>The column resolved fewer regular punch elevations than the levels need.</summary>
        public const string StationNotEnoughColumnPunches = "CANT_STATION_NOT_ENOUGH_COLUMN_PUNCHES";

        /// <summary>
        /// The regular grid does not rise: its pitch is not finite and positive.
        ///
        /// BLOCKING before any search starts. A non-rising grid is not a small grid — no monotonic search over
        /// it terminates, and no level could ever sit above another.
        /// </summary>
        public const string StationGridNotIncreasing = "CANT_STATION_GRID_NOT_INCREASING";

        /// <summary>
        /// A punch index, or the row range it implies, falls outside the grid's DOMAIN.
        ///
        /// Its own code because it is not "out of range of this column": the column is not even known yet. It is
        /// the arithmetic domain of the grid, and it is what an <c>int.MaxValue</c> index gets instead of an
        /// overflow, a freeze or an empty selection.
        /// </summary>
        public const string StationPunchIndexDomainOverflow = "CANT_STATION_PUNCH_INDEX_DOMAIN_OVERFLOW";

        // ---- I-37D: la linea, el arriostramiento y los tensores ---------------------------------------

        /// <summary>The panel-count mode has no rule. Added, never guessed.</summary>
        public const string BracingPanelCountModeNotSupported = "CANT_BRACING_PANEL_COUNT_MODE_NOT_SUPPORTED";

        /// <summary>Manual panel count asked for and not supplied.</summary>
        public const string BracingManualPanelCountMissing = "CANT_BRACING_MANUAL_PANEL_COUNT_MISSING";

        /// <summary>A manual panel count of zero or less. NOT clamped to the rule's answer.</summary>
        public const string BracingManualPanelCountNotPositive = "CANT_BRACING_MANUAL_PANEL_COUNT_NOT_POSITIVE";

        /// <summary>
        /// The bracing core is taller than the column.
        ///
        /// BLOCKING under a manual height. The alternatives are all silent lies: compressing the panels,
        /// shrinking the central spaces, or changing the count (ADR-0027, D4).
        /// </summary>
        public const string BracingDoesNotFitTheColumn = "CANT_BRACING_DOES_NOT_FIT_THE_COLUMN";

        /// <summary>A line needs at least two stations: with one there is no interval and no bracing.</summary>
        public const string LineNeedsTwoStations = "CANT_LINE_NEEDS_TWO_STATIONS";

        /// <summary>The centre-to-centre column spacing is not a usable positive number.</summary>
        public const string LineSpacingNotPositive = "CANT_LINE_SPACING_NOT_POSITIVE";

        /// <summary>A station of the line could not be resolved. Its own diagnostics travel with it.</summary>
        public const string LineStationBlocked = "CANT_LINE_STATION_BLOCKED";

        /// <summary>
        /// The common height moved a level index of some station.
        ///
        /// BLOCKING and it should be unreachable: the second pass exists so the prediction can be CHECKED
        /// (ADR-0027, D2). Accepting a different answer in silence is how a model starts lying.
        /// </summary>
        public const string LineCommonHeightMovedALevel = "CANT_LINE_COMMON_HEIGHT_MOVED_A_LEVEL";

        /// <summary>A manual common height below what the stations or the bracing need.</summary>
        public const string LineManualHeightBelowMinimum = "CANT_LINE_MANUAL_HEIGHT_BELOW_MINIMUM";

        /// <summary>The separator does not reach between the two column plates.</summary>
        public const string SeparatorTooShortForItsPunches = "CANT_SEPARATOR_TOO_SHORT_FOR_ITS_PUNCHES";

        /// <summary>The brace body kind has no rule.</summary>
        public const string BraceBodyKindNotSupported = "CANT_BRACE_BODY_KIND_NOT_SUPPORTED";

        /// <summary>A structural brace with no section id. There is no approved default for it.</summary>
        public const string BraceSectionMissing = "CANT_BRACE_SECTION_MISSING";

        /// <summary>The cold-rolled rod diameter is not a usable positive number.</summary>
        public const string ColdRolledDiameterNotPositive = "CANT_COLD_ROLLED_DIAMETER_NOT_POSITIVE";

        /// <summary>A separator or brace punch does not coincide with the datum it must bolt to.</summary>
        public const string BracingDatumMismatch = "CANT_BRACING_DATUM_MISMATCH";
    }
}
