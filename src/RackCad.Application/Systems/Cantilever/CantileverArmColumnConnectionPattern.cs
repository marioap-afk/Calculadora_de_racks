using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// Which of the column's ALREADY RESOLVED regular punches an arm bolts to, and what the plate they imply
    /// measures.
    ///
    /// The arm does NOT create a grid. It selects a contiguous run of
    /// <c>CantileverColumnBaseAssembly.ColumnRegularPunches</c>, keeps their datums EXACTLY, and OBSERVES
    /// their pitch. No spacing constant lives here: today it is 4 in because the column governs it, and if the
    /// column changes the arm follows without being edited. Two algorithms for one set of bolts is PB-004
    /// (ADR-0025, D5).
    /// </summary>
    public sealed class CantileverArmColumnConnectionPattern
    {
        private CantileverArmColumnConnectionPattern(
            double leftRowX,
            double rightRowX,
            int lowerColumnPunchIndex,
            int verticalPunchCount,
            IReadOnlyList<CantileverPunchDatum> selectedDatums,
            IReadOnlyList<double> elevations,
            double diameter,
            double observedPitch,
            double verticalEndOffset,
            IReadOnlyList<CantileverDiagnostic> diagnostics)
        {
            LeftRowX = leftRowX;
            RightRowX = rightRowX;
            LowerColumnPunchIndex = lowerColumnPunchIndex;
            VerticalPunchCount = verticalPunchCount;
            SelectedDatums = selectedDatums;
            Elevations = elevations;
            Diameter = diameter;
            ObservedPitch = observedPitch;
            VerticalEndOffset = verticalEndOffset;
            Diagnostics = diagnostics;
        }

        /// <summary>Transverse coordinate of the left punch column. GOVERNED BY THE COLUMN.</summary>
        public double LeftRowX { get; }

        /// <summary>Transverse coordinate of the right punch column. GOVERNED BY THE COLUMN.</summary>
        public double RightRowX { get; }

        public IReadOnlyList<double> RowX => new[] { LeftRowX, RightRowX };

        /// <summary>Base-zero index, within the column's regular elevations, of the lowest one selected.</summary>
        public int LowerColumnPunchIndex { get; }

        public int VerticalPunchCount { get; }

        /// <summary>
        /// The datums selected, row by row and ascending in elevation. They are the column's OWN datum values,
        /// not recomputed copies, which is what makes the coincidence exact rather than approximate.
        /// </summary>
        public IReadOnlyList<CantileverPunchDatum> SelectedDatums { get; }

        /// <summary>The selected elevations, ascending.</summary>
        public IReadOnlyList<double> Elevations { get; }

        public double Diameter { get; }

        /// <summary>
        /// The vertical spacing MEASURED between the selected elevations. Not a parameter of the arm and not a
        /// constant: it is what the column happens to use.
        /// </summary>
        public double ObservedPitch { get; }

        /// <summary>The margin from the outermost punches to the plate's edges.</summary>
        public double VerticalEndOffset { get; }

        public IReadOnlyList<CantileverDiagnostic> Diagnostics { get; }

        public double FirstElevation => Elevations[0];

        public double LastElevation => Elevations[Elevations.Count - 1];

        /// <summary>Bottom edge of the mounting plate. The body's lower envelope is anchored to it.</summary>
        public double PlateBottomZ => FirstElevation - VerticalEndOffset;

        /// <summary>
        /// Top edge of the mounting plate. Adding rows raises THIS and never lowers
        /// <see cref="PlateBottomZ"/>: the bottom is anchored to the body (ADR-0025, D6).
        /// </summary>
        public double PlateTopZ => LastElevation + VerticalEndOffset;

        public double PlateHeight => PlateTopZ - PlateBottomZ;

        /// <summary>
        /// Builds the pattern, or returns null with a blocking diagnostic already recorded.
        /// </summary>
        /// <param name="columnRegularPunches">The column's regular punches, exactly as it resolved them.</param>
        public static CantileverArmColumnConnectionPattern Build(
            IReadOnlyList<CantileverPunchPlan> columnRegularPunches,
            int lowerColumnPunchIndex,
            int verticalPunchCount,
            double verticalEndOffset,
            ICollection<CantileverDiagnostic> diagnostics)
        {
            if (columnRegularPunches == null)
            {
                throw new ArgumentNullException(nameof(columnRegularPunches));
            }

            if (diagnostics == null)
            {
                throw new ArgumentNullException(nameof(diagnostics));
            }

            if (verticalPunchCount < 2)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ArmVerticalCountTooSmall,
                    "Un brazo necesita al menos dos filas de troqueles; se pidieron " + verticalPunchCount + "."));
                return null;
            }

            if (lowerColumnPunchIndex < 0)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ArmPunchIndexOutOfRange,
                    "El indice del troquel inferior es base cero y no puede ser negativo; se recibio " +
                    lowerColumnPunchIndex + "."));
                return null;
            }

            // The column's list is flat: every row's elevations, one row after another. What the arm indexes
            // is the ELEVATION, so the distinct ascending elevations are the real axis here.
            var elevations = columnRegularPunches
                .Select(p => p.Datum.V)
                .Distinct()
                .OrderBy(v => v)
                .ToList();

            if (elevations.Count == 0)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ArmNotEnoughColumnPunches,
                    "La columna no resolvio ningun troquel regular al que un brazo pueda atornillarse."));
                return null;
            }

            if (elevations.Count < verticalPunchCount)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ArmNotEnoughColumnPunches,
                    "La columna resolvio " + elevations.Count + " elevaciones regulares y el brazo pide " +
                    verticalPunchCount + "."));
                return null;
            }

            // Written as a SUBTRACTION on purpose. The sum `lowerColumnPunchIndex + verticalPunchCount`
            // overflows for a large index — int.MaxValue wraps negative, the check passes, and the selection
            // comes out empty, which then throws where the pitch is read. The count is already known to be
            // no greater than the list, so `Count - verticalPunchCount` cannot underflow.
            if (lowerColumnPunchIndex > elevations.Count - verticalPunchCount)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ArmPunchIndexOutOfRange,
                    "El brazo pide las elevaciones " + lowerColumnPunchIndex + " a " +
                    (lowerColumnPunchIndex + verticalPunchCount - 1) + " y la columna solo tiene " +
                    elevations.Count + " (indices 0 a " + (elevations.Count - 1) + ")."));
                return null;
            }

            var selectedElevations = elevations
                .Skip(lowerColumnPunchIndex)
                .Take(verticalPunchCount)
                .ToList();

            // The pitch is OBSERVED here and nowhere else. It also doubles as the contiguity check: a run that
            // is not evenly spaced is not a run of one grid.
            var observedPitch = selectedElevations[1] - selectedElevations[0];

            for (var i = 1; i < selectedElevations.Count; i++)
            {
                var step = selectedElevations[i] - selectedElevations[i - 1];

                if (Math.Abs(step - observedPitch) > 1e-6)
                {
                    diagnostics.Add(CantileverDiagnostic.Blocking(
                        CantileverDiagnostics.ArmPunchDatumsNotContiguous,
                        "Las elevaciones seleccionadas no forman una secuencia uniforme: " +
                        Format(observedPitch) + " in y luego " + Format(step) + " in."));
                    return null;
                }
            }

            // Take the column's OWN datums for the selected elevations. Rebuilding them would be the second
            // algorithm this type exists to avoid.
            var selectedSet = new HashSet<double>(selectedElevations);
            var selected = columnRegularPunches
                .Where(p => selectedSet.Contains(p.Datum.V))
                .OrderBy(p => p.Datum.U)
                .ThenBy(p => p.Datum.V)
                .Select(p => p.Datum)
                .ToList();

            var rows = selected.Select(d => d.U).Distinct().OrderBy(x => x).ToList();

            if (rows.Count != 2)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ArmPunchDatumsNotContiguous,
                    "Se esperaban dos columnas transversales de troqueles y la columna aporto " + rows.Count + "."));
                return null;
            }

            var diameter = selected[0].Diameter;

            return new CantileverArmColumnConnectionPattern(
                rows[0], rows[1], lowerColumnPunchIndex, verticalPunchCount,
                selected, selectedElevations, diameter, observedPitch, verticalEndOffset,
                new List<CantileverDiagnostic>());
        }

        /// <summary>Deterministic fingerprint.</summary>
        public string Signature()
        {
            var parts = new List<string>
            {
                "rows=" + Format6(LeftRowX) + "," + Format6(RightRowX),
                "lower=" + LowerColumnPunchIndex.ToString(CultureInfo.InvariantCulture),
                "count=" + VerticalPunchCount.ToString(CultureInfo.InvariantCulture),
                "d=" + Format6(Diameter),
                "pitch=" + Format6(ObservedPitch),
                "plate=" + Format6(PlateBottomZ) + ".." + Format6(PlateTopZ),
                "z=" + string.Join("|", Elevations.Select(Format6))
            };

            return string.Join(";", parts);
        }

        private static string Format(double value) =>
            value.ToString("0.####", CultureInfo.InvariantCulture);

        private static string Format6(double value) =>
            Math.Round(value, 6, MidpointRounding.AwayFromZero).ToString("0.######", CultureInfo.InvariantCulture);
    }
}
