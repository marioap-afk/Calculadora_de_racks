using System;
using System.Collections.Generic;
using System.Globalization;

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
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index), index, "El indice de troquel regular es base cero y no puede ser negativo.");
            }

            var z = FirstElevation;

            for (var i = 0; i < index; i++)
            {
                z += Pitch;
            }

            return z;
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
