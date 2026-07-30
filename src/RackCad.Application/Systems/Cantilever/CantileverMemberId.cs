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
        Arm = 2
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
    }
}
