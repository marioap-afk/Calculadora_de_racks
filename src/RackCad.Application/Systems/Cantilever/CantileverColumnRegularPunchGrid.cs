using System;
using System.Collections.Generic;
using System.Globalization;
using RackCad.Application.Geometry;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// THE authority for the column's REGULAR punch region: where its elevations are, how far apart, on which
    /// two transverse rows and with what diameter.
    ///
    /// It was extracted from <c>CantileverColumnBaseResolver.BuildRegularPunches</c> (I-37C) for a reason that
    /// is not tidiness. A station has to pick which punches each level bolts to BEFORE it knows the column's
    /// final height — the height depends on where the levels land, and where the levels land depends on the
    /// grid. This type is what breaks that circle: it needs the connection pattern and the pitch, and
    /// **nothing about the height**. Generating the punches for a given height is a separate, later call
    /// (ADR-0026, D5).
    ///
    /// The extraction was MECHANICAL and is pinned by
    /// <c>CantileverRegularPunchGridCharacterizationTests</c>, captured before it happened. I-37A consumes
    /// this same type, so the formula
    /// <c>LastConnectionElevation + index × pitch</c> exists in exactly ONE place. A second copy anywhere is
    /// the PB-004 defect: two algorithms for one set of bolts, agreeing right up to the day one is edited.
    /// </summary>
    public sealed class CantileverColumnRegularPunchGrid
    {
        /// <summary>
        /// The slack the generation comparison allows, and the same value the pre-extraction loop used.
        ///
        /// It is here and not duplicated per call site because it is part of WHERE THE LAST PUNCH FALLS: a
        /// different epsilon is a different punch count on a column whose ceiling lands exactly on a pitch.
        /// </summary>
        public const double FitTolerance = 1e-9;

        /// <summary>
        /// How many accumulation steps the grid is DEFINED for.
        ///
        /// It is a numeric-domain bound and NOT a product limit, and it is derived rather than chosen. The
        /// accumulated value drifts from the ideal by roughly <c>n · eps · z</c> after n steps; that drift
        /// exceeds one whole <c>Pitch</c> when <c>n² · eps &gt; 1</c>, i.e. when <c>n &gt; 1/√eps ≈ 6.7 × 10⁷</c>.
        /// Past that point the accumulated sequence has moved by more than a full pitch and is no longer the
        /// grid it claims to be, so an index beyond it is outside the grid's domain — not "too tall for a
        /// product", which is a decision nobody has taken.
        ///
        /// It also keeps random access CHEAP: without it, <c>ElevationAt(int.MaxValue)</c> would walk two
        /// billion additions. With it, an index that far out is rejected in constant time by the estimate
        /// below.
        ///
        /// For a 4 in pitch this is about 268 million inches of column, so it bounds nothing anybody can build.
        /// </summary>
        public const int MaxDefinedIndex = 1 << 26;

        private CantileverColumnRegularPunchGrid(
            double firstElevation, double pitch, double leftRowX, double rightRowX, double diameter)
        {
            FirstElevation = firstElevation;
            Pitch = pitch;
            LeftRowX = leftRowX;
            RightRowX = rightRowX;
            Diameter = diameter;
        }

        /// <summary>
        /// Elevation of index 0: one REGULAR pitch above the last connection punch.
        ///
        /// One regular pitch and not one connection pitch, and never a duplicate of the last connection
        /// punch: the regular region CONTINUES the connection region rather than restarting from the floor.
        /// </summary>
        public double FirstElevation { get; }

        /// <summary>Centre-to-centre spacing of the regular region.</summary>
        public double Pitch { get; }

        /// <summary>Transverse coordinate of the left punch row. Governed by the COLUMN.</summary>
        public double LeftRowX { get; }

        /// <summary>Transverse coordinate of the right punch row. Governed by the COLUMN.</summary>
        public double RightRowX { get; }

        /// <summary>The two rows, left first. Deterministic order.</summary>
        public IReadOnlyList<double> RowX => new[] { LeftRowX, RightRowX };

        public double Diameter { get; }

        /// <summary>
        /// The grid a resolved connection pattern implies.
        ///
        /// Everything comes from the pattern — the rows, the diameter, the pitch and the last connection
        /// elevation — so the grid cannot disagree with the sub-assembly it belongs to.
        /// </summary>
        public static CantileverColumnRegularPunchGrid FromPattern(
            CantileverColumnBaseConnectionPattern pattern)
        {
            if (pattern == null)
            {
                throw new ArgumentNullException(nameof(pattern));
            }

            var parameters = pattern.Parameters;

            return new CantileverColumnRegularPunchGrid(
                pattern.LastConnectionElevation + parameters.RegularColumnPitch,
                parameters.RegularColumnPitch,
                pattern.LeftRowX,
                pattern.RightRowX,
                parameters.Diameter);
        }

        /// <summary>
        /// Whether this grid is INCREASING and usable at all: a finite first elevation and a finite, strictly
        /// positive pitch.
        ///
        /// A non-positive pitch is not a small grid — it is a sequence that never rises, so no monotonic search
        /// over it terminates and no level can ever be placed above another. It has to be a blocking diagnostic
        /// before any search starts, which is why this is a question the grid answers about itself rather than
        /// something each caller re-checks.
        /// </summary>
        public bool IsIncreasing =>
            GeometryTolerance.IsFinite(FirstElevation) &&
            GeometryTolerance.IsFinite(Pitch) &&
            Pitch > 0.0;

        /// <summary>
        /// The elevation of one index, BASE ZERO.
        ///
        /// It ACCUMULATES rather than computing <c>First + index × Pitch</c>, and that is deliberate. The two
        /// forms agree bit for bit on a dyadic pitch like 4 in and DIVERGE on one like 3.7 — the
        /// characterization caught exactly that: index 2 of a 3.7 pitch is
        /// <c>27.599999999999998</c> accumulated and <c>27.6</c> multiplied.
        ///
        /// Accumulation is what I-37A shipped, so it is what the extraction has to preserve: switching to the
        /// multiplication would MOVE every hole of a non-dyadic-pitch column, which is a behaviour change in
        /// integrated code that nobody authorised. Whether the multiplication is the better rule is a real
        /// question and a separate decision — it is not one this extraction gets to take in passing.
        ///
        /// It also means there is still only ONE definition: <see cref="ElevationsUpTo"/> and this method walk
        /// the same steps, so an index and the generated sequence cannot disagree.
        /// </summary>
        public double ElevationAt(int index)
        {
            if (!TryElevationAt(index, out var z))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index), index,
                    "El indice de troquel regular esta fuera del dominio de la reticula.");
            }

            return z;
        }

        /// <summary>
        /// The elevation of one index, reporting failure instead of throwing.
        ///
        /// It exists because the index can come from USER INPUT — a station's first level names one directly —
        /// and a number somebody typed has to come back as a diagnostic, never as an exception. It is the same
        /// rule I-37B applied to a slope that collapses the frame.
        ///
        /// The closed form is computed FIRST, and only as a bound estimate: it decides in constant time whether
        /// the index is inside <see cref="MaxDefinedIndex"/> before any walking happens. The value returned is
        /// always the ACCUMULATED one, which stays the grid's authority (ADR-0026, D5).
        /// </summary>
        public bool TryElevationAt(long index, out double elevation)
        {
            elevation = double.NaN;

            if (index < 0 || index > MaxDefinedIndex || !IsIncreasing)
            {
                return false;
            }

            // Bound estimate only. It is never returned, and it never decides an elevation — it decides
            // whether the accumulation is worth performing.
            var estimate = FirstElevation + (index * Pitch);

            if (!GeometryTolerance.IsFinite(estimate))
            {
                return false;
            }

            var z = FirstElevation;

            for (long i = 0; i < index; i++)
            {
                z += Pitch;
            }

            elevation = z;
            return GeometryTolerance.IsFinite(z);
        }

        /// <summary>
        /// The FIRST index whose elevation reaches <paramref name="target"/>, or false if none does inside the
        /// grid's domain.
        ///
        /// This is what replaced the candidate-by-candidate walk the level layout used to do, and with it the
        /// arbitrary cap of 250 candidates disappeared: because the elevations are strictly increasing, the
        /// first index that satisfies a lower bound can be found directly instead of searched for.
        ///
        /// The closed form gives a starting guess and the ACCUMULATED elevations then correct it by a step or
        /// two — never more, because the two forms differ by far less than a pitch inside the domain. That
        /// keeps the accumulated sequence as the final authority while making the lookup constant-time.
        /// </summary>
        public bool TryFirstIndexAtOrAbove(double target, out int index)
        {
            index = 0;

            if (!IsIncreasing || !GeometryTolerance.IsFinite(target))
            {
                return false;
            }

            if (target <= FirstElevation)
            {
                return true;
            }

            var guess = Math.Floor((target - FirstElevation) / Pitch);

            if (!GeometryTolerance.IsFinite(guess) || guess > MaxDefinedIndex)
            {
                return false;
            }

            var candidate = (long)Math.Max(0.0, guess);

            // Walk BACKWARDS off the guess while the previous index would also do, then forwards until the
            // accumulated elevation reaches the target. Both loops run a couple of steps at most; they exist
            // because the guess comes from the closed form and the answer must come from the accumulation.
            while (candidate > 0 && TryElevationAt(candidate - 1, out var below) && below >= target)
            {
                candidate--;
            }

            while (candidate <= MaxDefinedIndex)
            {
                if (!TryElevationAt(candidate, out var z))
                {
                    return false;
                }

                if (z >= target)
                {
                    index = (int)candidate;
                    return true;
                }

                candidate++;
            }

            return false;
        }

        /// <summary>The two datums of one index, left row first. The datums the column itself would produce.</summary>
        public IReadOnlyList<CantileverPunchDatum> DatumsAt(int index)
        {
            var z = ElevationAt(index);

            return new[]
            {
                new CantileverPunchDatum(CantileverPunchAxis.AlongY, LeftRowX, z, Diameter),
                new CantileverPunchDatum(CantileverPunchAxis.AlongY, RightRowX, z, Diameter)
            };
        }

        /// <summary>
        /// The shortest column that still contains <paramref name="index"/>, given the top offset.
        ///
        /// It is the inverse of <see cref="CountUpTo"/> and exists so a station can ask "how tall must this
        /// column be for the level I just placed?" without generating anything.
        /// </summary>
        public double MinimumColumnHeightFor(int index, double columnTopPunchOffset) =>
            ElevationAt(index) + columnTopPunchOffset;

        /// <summary>
        /// How many indices fit in a column of this height. Zero is a legal answer.
        ///
        /// The ceiling is <c>height − topOffset</c>, exactly as the pre-extraction loop computed it, and the
        /// comparison keeps its tolerance so a ceiling landing on a pitch still includes that punch.
        /// </summary>
        public int CountUpTo(double columnTopZ, double columnTopPunchOffset) =>
            ElevationsUpTo(columnTopZ, columnTopPunchOffset).Count;

        /// <summary>
        /// The elevations that fit in a column of this height, ascending. Zero of them is a legal answer.
        ///
        /// This IS the pre-extraction loop, moved verbatim: same accumulation, same ceiling, same tolerance.
        /// <see cref="CountUpTo"/> delegates to it rather than re-walking, so the count and the sequence
        /// cannot disagree about where the last punch fell.
        /// </summary>
        public IReadOnlyList<double> ElevationsUpTo(double columnTopZ, double columnTopPunchOffset)
        {
            if (!IsIncreasing)
            {
                return new List<double>();
            }

            var ceiling = columnTopZ - columnTopPunchOffset;
            var elevations = new List<double>();

            for (var z = FirstElevation; z <= ceiling + FitTolerance; z += Pitch)
            {
                elevations.Add(z);
            }

            return elevations;
        }

        /// <summary>
        /// Whether the grid contains an index at all. A column can be too short for even the first one.
        /// </summary>
        public bool Contains(int index, double columnTopZ, double columnTopPunchOffset) =>
            index >= 0 && index < CountUpTo(columnTopZ, columnTopPunchOffset);

        /// <summary>Deterministic fingerprint.</summary>
        public string Signature() => string.Format(
            CultureInfo.InvariantCulture,
            "first={0:0.######};pitch={1:0.######};rows={2:0.######},{3:0.######};d={4:0.######}",
            FirstElevation, Pitch, LeftRowX, RightRowX, Diameter);

        public override string ToString() => "RegularGrid " + Signature();
    }
}
