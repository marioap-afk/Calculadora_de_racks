using System;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// What a resolved structural member IS in the sub-assembly.
    ///
    /// The role is an ENUM inside <see cref="CantileverStructuralMemberPlan"/> and not a type hierarchy:
    /// adding the arm later adds a value here, not a type and not a <c>switch</c> in every consumer
    /// (ADR-0024, D2).
    /// </summary>
    public enum CantileverMemberRole
    {
        /// <summary>The vertical profile.</summary>
        Column = 0,

        /// <summary>The horizontal profile that projects from the column.</summary>
        Base = 1,

        /// <summary>
        /// A load-carrying arm. Added by I-37B, at the END so the two existing values keep their numbers.
        ///
        /// One arm may resolve to TWO members of this role — a paired-channel body — which is exactly why the
        /// role is an enum on a flat member plan and not a type: the cardinality lives in the arrangement,
        /// not in the role (ADR-0025, D1).
        /// </summary>
        Arm = 2,

        /// <summary>
        /// A longitudinal separator between two adjacent stations, added by I-37D at the END.
        ///
        /// It belongs to the interval, not to either station — which is what keeps the first and last station
        /// from being special cases (ADR-0027, D3).
        /// </summary>
        Separator = 3,

        /// <summary>
        /// One diagonal of a braced panel, when its body is a structural section. A cold-rolled rod produces
        /// NO member of this role: it is not a catalogued section, so it carries its own body (ADR-0027, D7).
        /// </summary>
        Brace = 4
    }

    /// <summary>
    /// The deterministic identity of a resolved piece: <c>CANT-&lt;owner&gt;-&lt;token&gt;</c>.
    ///
    /// It is DERIVED from the topology and never persisted. Persisting it would create an authority with no
    /// consumer — I-37A has no per-piece overrides to reconcile — and it would have to be kept in step with
    /// a topology that already determines it. What it IS for is the key a later initiative can hang things
    /// on (a BOM line, a structural check) without inheritance.
    ///
    /// It is a type of its own, and not a <c>string</c>, so it can never be passed where a
    /// <c>StructuralSectionId</c> is expected. They are both text and they are not interchangeable.
    /// </summary>
    public readonly struct CantileverPieceId : IEquatable<CantileverPieceId>
    {
        private const string Prefix = "CANT";

        private readonly string _value;

        private CantileverPieceId(string value)
        {
            _value = value;
        }

        public string Value => _value ?? string.Empty;

        public bool IsEmpty => string.IsNullOrEmpty(_value);

        /// <summary>
        /// Builds an id from the owner and the piece token. Both are normalised to the same alphabet the
        /// rest of the product uses for ids — upper case, digits, hyphen — so an id never carries a
        /// character that a downstream name could not.
        /// </summary>
        public static CantileverPieceId Create(string owner, string token)
        {
            if (string.IsNullOrWhiteSpace(owner))
            {
                throw new ArgumentException("El propietario de la pieza no puede estar vacio.", nameof(owner));
            }

            if (string.IsNullOrWhiteSpace(token))
            {
                throw new ArgumentException("El token de la pieza no puede estar vacio.", nameof(token));
            }

            return new CantileverPieceId(Prefix + "-" + Normalize(owner) + "-" + Normalize(token));
        }

        /// <summary>Adds an ordinal suffix, base 1, without zero padding.</summary>
        public CantileverPieceId At(int index)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            // No zero padding on purpose: a fixed width silently changes every id the day a run outgrows it,
            // and ordering is done on the numeric index, never on this text.
            return new CantileverPieceId(Value + "-" + (index + 1).ToString(System.Globalization.CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// The same piece, keyed to the station it belongs to inside a line: <c>CANT-S&lt;n&gt;-…</c>.
        ///
        /// A line has N stations whose pieces are resolved by the I-37C authority, which numbers them without
        /// knowing about a line. Two stations therefore carry the SAME piece ids, and anything that puts a
        /// whole line in one dictionary would lose N−1 of every piece. Scoping is done here, on the way out,
        /// so the station's own ids stay exactly what I-37C shipped.
        /// </summary>
        public CantileverPieceId WithStationScope(int stationIndex)
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("Una pieza vacia no puede recibir alcance de estacion.");
            }

            if (stationIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stationIndex));
            }

            var scope = CantileverPieceTokens.StationScope +
                stationIndex.ToString(System.Globalization.CultureInfo.InvariantCulture);

            return new CantileverPieceId(
                Prefix + "-" + scope + Value.Substring(Prefix.Length));
        }

        private static string Normalize(string text)
        {
            var builder = new System.Text.StringBuilder(text.Length);

            foreach (var ch in text)
            {
                if ((ch >= 'A' && ch <= 'Z') || (ch >= '0' && ch <= '9') || ch == '-')
                {
                    builder.Append(ch);
                }
                else if (ch >= 'a' && ch <= 'z')
                {
                    builder.Append(char.ToUpperInvariant(ch));
                }
                else
                {
                    builder.Append('-');
                }
            }

            return builder.ToString();
        }

        public bool Equals(CantileverPieceId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);

        public override bool Equals(object obj) => obj is CantileverPieceId other && Equals(other);

        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Value);

        public static bool operator ==(CantileverPieceId left, CantileverPieceId right) => left.Equals(right);

        public static bool operator !=(CantileverPieceId left, CantileverPieceId right) => !left.Equals(right);

        public override string ToString() => Value;
    }

    /// <summary>The piece tokens of the column–base sub-assembly. Stable: they travel inside every id.</summary>
    public static class CantileverPieceTokens
    {
        /// <summary>The owner of every piece of a column–base sub-assembly.</summary>
        public const string ColumnBaseOwner = "CB";

        public const string Column = "COL";
        public const string Base = "BAS";
        public const string BaseFrontPlate = "PFRONT";
        public const string BaseRearPlate = "PREAR";
        public const string ColumnBottomPlate = "PBOT";
        public const string Gusset = "GUS";
        public const string RearPlatePunch = "PCH-REAR";
        public const string ColumnConnectionPunch = "PCH-CONN";
        public const string ColumnRegularPunch = "PCH-REG";
        public const string ColumnBottomPlatePunch = "PCH-BOT";

        // ---- I-37B: the arm. Added, never renumbered; the tokens above travel inside existing ids. -------

        /// <summary>Owner token of a standalone arm, for a caller that has no station to name it from yet.</summary>
        public const string ArmOwner = "ARM";

        public const string ArmBody = "BODY";
        public const string ArmMountingPlate = "PMOUNT";
        public const string ArmEndPlate = "PEND";
        public const string ArmMountingPunch = "PCH-MOUNT";

        // ---- I-37C: the station. Added at the end; every token above keeps its text. --------------------

        /// <summary>Owner token of a station's SHARED pieces — the column and its bottom plate.</summary>
        public const string StationOwner = "STN";

        /// <summary>
        /// Side discriminators, appended to an owner so the two bases of a double station cannot collide.
        ///
        /// They exist because a double station has TWO of several pieces that I-37A only ever built once, and
        /// two pieces sharing one id is a BOM that counts one of them.
        /// </summary>
        public const string StationSidePositive = "PY";

        /// <summary>See <see cref="StationSidePositive"/>.</summary>
        public const string StationSideNegative = "NY";

        /// <summary>Level discriminator of an arm owner, used as <c>L&lt;n&gt;</c> with n base one.</summary>
        public const string StationLevel = "L";

        /// <summary>The owner of one side's base pieces inside a station.</summary>
        public static string StationBaseOwner(CantileverArmSide side) =>
            StationOwner + "-" + SideToken(side);

        /// <summary>The owner of one arm: station, level (base one) and side.</summary>
        public static string StationArmOwner(int levelIndex, CantileverArmSide side) =>
            StationOwner + "-" + StationLevel +
            (levelIndex + 1).ToString(System.Globalization.CultureInfo.InvariantCulture) +
            "-" + SideToken(side);

        /// <summary>
        /// The stable text of a side. An explicit <c>switch</c> with a throwing default: a side added
        /// tomorrow must not silently inherit the positive one's ids.
        /// </summary>
        public static string SideToken(CantileverArmSide side)
        {
            switch (side)
            {
                case CantileverArmSide.PositiveY:
                    return StationSidePositive;
                case CantileverArmSide.NegativeY:
                    return StationSideNegative;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(side), side, "El lado '" + side + "' no tiene token de pieza.");
            }
        }

        // ---- I-37D: the line. Added at the end; every token above keeps its text. -----------------------

        /// <summary>
        /// Owner token of the pieces of one interval, used as <c>INT&lt;i&gt;</c> with i base zero.
        ///
        /// The interval — not either station — owns them. Two intervals meet at an interior station and each
        /// brings its own separators and its own column plates, so an interval-owned id cannot be produced
        /// twice for the same piece, and walking the intervals cannot count a separator twice (ADR-0027, D3).
        /// </summary>
        public const string IntervalOwner = "INT";

        /// <summary>The end of an interval at the LOWER station index.</summary>
        public const string IntervalEndLeft = "EL";

        /// <summary>The end of an interval at the HIGHER station index.</summary>
        public const string IntervalEndRight = "ER";

        /// <summary>A separator body, used as <c>SEP&lt;k&gt;</c> with k the separator index, base zero.</summary>
        public const string Separator = "SEP";

        /// <summary>A separator's end punch, into the column plate.</summary>
        public const string SeparatorEndPunch = "PCH-SEP-END";

        /// <summary>A separator's brace punch.</summary>
        public const string SeparatorBracePunch = "PCH-SEP-BRC";

        /// <summary>A separator's column plate.</summary>
        public const string SeparatorColumnPlate = "PLT-SEP";

        /// <summary>The single centred hole of a separator's column plate.</summary>
        public const string SeparatorColumnPlatePunch = "PCH-PLT-SEP";

        /// <summary>One diagonal of a braced panel.</summary>
        public const string Brace = "BRC";

        /// <summary>An end punch of a structural brace.</summary>
        public const string BracePunch = "PCH-BRC";

        /// <summary>The end adapter of a cold-rolled brace.</summary>
        public const string ColdRolledAdapter = "ADP";

        /// <summary>The separator-facing hole of a cold-rolled brace's adapter.</summary>
        public const string ColdRolledAdapterPunch = "PCH-ADP";

        /// <summary>The owner of one interval's pieces.</summary>
        public static string IntervalOwnerOf(int intervalIndex) =>
            IntervalOwner + intervalIndex.ToString(System.Globalization.CultureInfo.InvariantCulture);

        /// <summary>The owner of the column plates at ONE end of an interval.</summary>
        public static string IntervalEndOwner(int intervalIndex, CantileverIntervalSide side) =>
            IntervalOwnerOf(intervalIndex) + "-" + IntervalEndToken(side);

        /// <summary>
        /// The stable text of an interval end. An explicit <c>switch</c> with a throwing default, for the same
        /// reason <see cref="SideToken"/> has one.
        /// </summary>
        public static string IntervalEndToken(CantileverIntervalSide side)
        {
            switch (side)
            {
                case CantileverIntervalSide.Left:
                    return IntervalEndLeft;
                case CantileverIntervalSide.Right:
                    return IntervalEndRight;
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(side), side, "El extremo de intervalo '" + side + "' no tiene token de pieza.");
            }
        }

        /// <summary>The token of one diagonal: the panel index and which diagonal it is.</summary>
        public static string BraceToken(int panelIndex, char diagonal) =>
            Brace + panelIndex.ToString(System.Globalization.CultureInfo.InvariantCulture) + diagonal;

        /// <summary>The token of one adapter: its brace's token and which end of it, base one.</summary>
        public static string AdapterToken(int panelIndex, char diagonal, int endIndex) =>
            ColdRolledAdapter + panelIndex.ToString(System.Globalization.CultureInfo.InvariantCulture) +
            diagonal + "-" + (endIndex + 1).ToString(System.Globalization.CultureInfo.InvariantCulture);

        /// <summary>
        /// The index token that scopes a station's pieces inside a line, as <c>S&lt;n&gt;</c>, base zero.
        ///
        /// A station resolved on its own keeps the ids I-37C gave it — <c>CANT-STN-…</c> — because renaming
        /// them would rewrite every signature already integrated. What a LINE needs on top is a key that
        /// distinguishes the same piece of two different stations, and that is this scope, applied by
        /// <see cref="CantileverPieceId.WithStationScope"/>.
        /// </summary>
        public const string StationScope = "S";
    }
}
