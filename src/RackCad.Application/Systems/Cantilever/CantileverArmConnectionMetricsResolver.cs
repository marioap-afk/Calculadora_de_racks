using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// What one arm's connection MEASURES, and whether its inputs are usable at all.
    ///
    /// A resolved instance carries the four elevations, the two plate edges and the two body edges in the
    /// connection plane, or a set of blocking diagnostics and nothing else. There is no half-resolved state:
    /// <see cref="IsBlocked"/> and a value are mutually exclusive, which is what stops a caller from reading a
    /// number that was invented to fill a gap.
    /// </summary>
    public sealed class CantileverArmConnectionMetrics
    {
        private CantileverArmConnectionMetrics(
            CantileverArmSide side,
            int lowerPunchIndex,
            int verticalPunchCount,
            double verticalEndOffset,
            double slopeRisePer12,
            double angleRadians,
            double combinedSectionHeight,
            double firstElevation,
            double lastElevation,
            double plateBottomZ,
            double plateTopZ,
            double bodyBottomZ,
            double bodyTopZ,
            IReadOnlyList<CantileverDiagnostic> diagnostics)
        {
            Side = side;
            LowerPunchIndex = lowerPunchIndex;
            VerticalPunchCount = verticalPunchCount;
            VerticalEndOffset = verticalEndOffset;
            SlopeRisePer12 = slopeRisePer12;
            AngleRadians = angleRadians;
            CombinedSectionHeight = combinedSectionHeight;
            FirstElevation = firstElevation;
            LastElevation = lastElevation;
            PlateBottomZ = plateBottomZ;
            PlateTopZ = plateTopZ;
            BodyBottomZ = bodyBottomZ;
            BodyTopZ = bodyTopZ;
            Diagnostics = diagnostics;
        }

        public CantileverArmSide Side { get; }

        public int LowerPunchIndex { get; }

        public int VerticalPunchCount { get; }

        /// <summary>The highest index this arm uses. The next level must start ABOVE it.</summary>
        public int UpperPunchIndex => LowerPunchIndex + VerticalPunchCount - 1;

        public double VerticalEndOffset { get; }

        public double SlopeRisePer12 { get; }

        public double AngleRadians { get; }

        /// <summary>Depth of the body's COMBINED section, from <c>Bounds</c> through the arrangement authority.</summary>
        public double CombinedSectionHeight { get; }

        public double FirstElevation { get; }

        public double LastElevation { get; }

        /// <summary>Bottom edge of the mounting plate, and the anchor of the body's lower envelope.</summary>
        public double PlateBottomZ { get; }

        /// <summary>Top edge of the mounting plate. More rows raise THIS (ADR-0025, D6).</summary>
        public double PlateTopZ { get; }

        /// <summary>Lower edge of the BODY in the connection plane. Equal to <see cref="PlateBottomZ"/>.</summary>
        public double BodyBottomZ { get; }

        /// <summary>
        /// Upper edge of the BODY in the connection plane.
        ///
        /// <c>BodyBottomZ + cos(slope) × CombinedSectionHeight</c>. The cosine is there because the section's
        /// depth axis is tilted, so its APPARENT vertical extent in this plane is foreshortened. It is the same
        /// number I-37B's plate-fit gate uses, and the reason a clear check is not simply "plate ≥ depth".
        /// </summary>
        public double BodyTopZ { get; }

        public IReadOnlyList<CantileverDiagnostic> Diagnostics { get; }

        public bool IsBlocked => Diagnostics.Any(d => d.IsBlocking);

        internal static CantileverArmConnectionMetrics Blocked(
            CantileverArmSide side, IReadOnlyList<CantileverDiagnostic> diagnostics) =>
            new CantileverArmConnectionMetrics(
                side, 0, 0, double.NaN, double.NaN, double.NaN, double.NaN,
                double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, double.NaN, diagnostics);

        internal static CantileverArmConnectionMetrics Create(
            CantileverArmSide side,
            int lowerPunchIndex,
            int verticalPunchCount,
            double verticalEndOffset,
            double slopeRisePer12,
            double angleRadians,
            double combinedSectionHeight,
            double firstElevation,
            double lastElevation,
            IReadOnlyList<CantileverDiagnostic> diagnostics)
        {
            var bottom = firstElevation - verticalEndOffset;

            return new CantileverArmConnectionMetrics(
                side, lowerPunchIndex, verticalPunchCount, verticalEndOffset,
                slopeRisePer12, angleRadians, combinedSectionHeight,
                firstElevation, lastElevation,
                bottom, lastElevation + verticalEndOffset,
                bottom, bottom + (Math.Cos(angleRadians) * combinedSectionHeight),
                diagnostics);
        }

        public string Signature() => IsBlocked
            ? "BLOCKED:" + string.Join(",", Diagnostics.Where(d => d.IsBlocking).Select(d => d.Code))
            : string.Format(
                CultureInfo.InvariantCulture,
                "{0}:idx={1}..{2};z={3:0.######}..{4:0.######};plate={5:0.######}..{6:0.######};" +
                "body={7:0.######}..{8:0.######}",
                Side, LowerPunchIndex, UpperPunchIndex, FirstElevation, LastElevation,
                PlateBottomZ, PlateTopZ, BodyBottomZ, BodyTopZ);

        public override string ToString() => "ArmConnectionMetrics " + Signature();
    }

    /// <summary>
    /// THE single authority that validates an arm's connection inputs and measures what they imply.
    ///
    /// It exists because there were TWO. I-37B computed these four elevations inside its resolver, and I-37C's
    /// level layout computed them again to decide where a level goes — with its own
    /// <c>Math.Max(2, VerticalPunchCount)</c> and its own <c>VerticalEndOffset ?? 0</c>. Those two expressions
    /// are the defect: they turn a row count of zero into two and a MISSING mandatory margin into zero, so a
    /// design I-37B would reject produced a confident layout, and the rejection only surfaced later — against
    /// numbers the layout had already used to size the column.
    ///
    /// Now both callers ask this type. An invalid input is a BLOCKING DIAGNOSTIC before any layout happens,
    /// never a normalisation and never an exception (ADR-0025 and ADR-0026, D5).
    ///
    /// It owns exactly the rules that concern the CONNECTION: the row count, the margin, the slope, the frame,
    /// the arrangement, the orientation and whether the body fits the plate it hangs from. It does not own the
    /// cut length, the plate thicknesses or the end plate — those are the arm's own, and they stay in I-37B.
    /// </summary>
    public static class CantileverArmConnectionMetricsResolver
    {
        private const double FitTolerance = 1e-9;

        /// <summary>
        /// Resolves the metrics of one arm connection.
        /// </summary>
        /// <param name="side">Which side the physical arm is on.</param>
        /// <param name="arrangement">How many profiles the body has and how they are paired.</param>
        /// <param name="orientation">The registered body orientation.</param>
        /// <param name="combinedSectionHeight">
        /// Depth of the COMBINED section, from <c>CantileverArmBodyArrangementResolver.CombinedBounds</c>. It is
        /// passed in rather than looked up so this type needs neither the catalogue nor the section policy.
        /// </param>
        /// <param name="slopeRisePer12">Rise per 12 in. Zero is legal; downhill is not.</param>
        /// <param name="lowerPunchIndex">Base-zero index of the lowest column punch the plate uses.</param>
        /// <param name="verticalPunchCount">How many rows the plate uses. Minimum two, and NOT clamped.</param>
        /// <param name="verticalEndOffset">
        /// The plate margin. Nullable because I-37B approved no default for it: missing is REJECTED here, which
        /// is the whole point of taking it as a nullable rather than as a number somebody already defaulted.
        /// </param>
        /// <param name="grid">The one regular-punch authority.</param>
        /// <param name="diameter">Punch diameter, for the margin-versus-radius rule.</param>
        public static CantileverArmConnectionMetrics Resolve(
            CantileverArmSide side,
            CantileverArmBodyArrangement arrangement,
            CantileverArmBodyOrientation orientation,
            double combinedSectionHeight,
            double slopeRisePer12,
            int lowerPunchIndex,
            int verticalPunchCount,
            double? verticalEndOffset,
            CantileverColumnRegularPunchGrid grid,
            double diameter)
        {
            if (grid == null)
            {
                throw new ArgumentNullException(nameof(grid));
            }

            var diagnostics = new List<CantileverDiagnostic>();

            // ---- the side, the arrangement and the orientation must have rules -----------------------------
            if (!CantileverArmFrameResolver.IsSupported(side))
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ArmSideNotSupported,
                    "El lado de brazo '" + side + "' no tiene regla de marco."));
            }

            if (!CantileverArmBodyArrangementResolver.IsSupported(arrangement))
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ArmArrangementNotSupported,
                    "El arreglo de cuerpo '" + arrangement + "' no tiene regla de colocacion."));
            }

            if (!CantileverArmFrameResolver.IsSupported(orientation))
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.OrientationNotSupported,
                    "La orientacion de cuerpo '" + orientation + "' no tiene regla de marco."));
            }

            // ---- the grid itself --------------------------------------------------------------------------
            if (!grid.IsIncreasing)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.StationGridNotIncreasing,
                    "La reticula de troqueles no crece: su pitch debe ser finito y positivo, y se recibio " +
                    Format(grid.Pitch) + "."));
            }

            // ---- the row count. NOT clamped: two is a rule, not a floor to snap to. ------------------------
            if (verticalPunchCount < 2)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ArmVerticalCountTooSmall,
                    "Un brazo necesita al menos dos filas de troqueles; se pidieron " + verticalPunchCount +
                    ". No se ajusta al minimo: una fila sola es una bisagra, no una conexion."));
            }

            if (lowerPunchIndex < 0)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ArmPunchIndexOutOfRange,
                    "El indice del troquel inferior es base cero y no puede ser negativo; se recibio " +
                    lowerPunchIndex + "."));
            }

            // Written on LONGS. `lowerPunchIndex + verticalPunchCount - 1` overflows int for a large index: it
            // wraps negative, every range check passes, and the selection comes out empty. Widening first is
            // what makes the guard actually guard (I-37B's second defect, in the station's arithmetic too).
            var upper = (long)lowerPunchIndex + verticalPunchCount - 1;

            if (upper > CantileverColumnRegularPunchGrid.MaxDefinedIndex)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.StationPunchIndexDomainOverflow,
                    "El rango de troqueles " + lowerPunchIndex + ".." + upper +
                    " queda fuera del dominio de la reticula (indice maximo " +
                    CantileverColumnRegularPunchGrid.MaxDefinedIndex + ")."));

                // And ALSO the code I-37B promises for an index no column can hold. The two say different
                // things — one is about the grid's arithmetic, the other about a column's range — and for an
                // index this far out both are true. Emitting only the new one would have been a silent change
                // to what I-37B's callers are told, which is a behaviour change nobody asked for.
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ArmPunchIndexOutOfRange,
                    "El brazo pide las elevaciones " + lowerPunchIndex + " a " + upper +
                    ", que ninguna columna puede contener."));
            }

            // ---- the margin. Missing is REJECTED, never filled in. -----------------------------------------
            if (verticalEndOffset == null)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.RequiredParameterMissing,
                    "El diseno debe declarar el margen vertical de la placa de conexion: no existe un valor " +
                    "por omision aprobado."));
            }
            else
            {
                var offset = verticalEndOffset.Value;

                if (!GeometryTolerance.IsFinite(offset) || offset < 0.0)
                {
                    diagnostics.Add(CantileverDiagnostic.Blocking(
                        CantileverDiagnostics.ParameterNotPositive,
                        "El margen vertical de la placa de conexion no puede ser negativo; se recibio " +
                        Format(offset) + "."));
                }
                else if (offset + FitTolerance < diameter / 2.0)
                {
                    // Edge-to-CENTRE, so the hole only fits if the margin is at least its RADIUS. Same rule and
                    // same code as I-37A and I-37B.
                    diagnostics.Add(CantileverDiagnostic.Blocking(
                        CantileverDiagnostics.EdgeOffsetBelowRadius,
                        "El margen vertical de la placa de conexion (" + Format(offset) +
                        " in) es menor que el radio del troquel (" + Format(diameter / 2.0) +
                        " in): el agujero no cabe."));
                }
            }

            // ---- the slope, and whether it leaves a frame at all -------------------------------------------
            var slopeUsable = GeometryTolerance.IsFinite(slopeRisePer12) && slopeRisePer12 >= 0.0;

            if (!slopeUsable)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ArmSlopeInvalid,
                    "La pendiente debe ser un numero finito no negativo; se recibio " +
                    Format(slopeRisePer12) + ". Cero es valido; bajar no."));
            }
            else if (CantileverArmFrameResolver.IsSupported(side) &&
                     !CantileverArmFrameResolver.IsRepresentableSlope(side, slopeRisePer12))
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ArmSlopeFrameUndefined,
                    "La pendiente " + Format(slopeRisePer12) +
                    " deja el brazo practicamente vertical: la proyeccion de +Z sobre su plano transversal se " +
                    "anula y el marco no queda definido."));
            }

            if (!GeometryTolerance.IsFinite(combinedSectionHeight) || combinedSectionHeight <= 0.0)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ParameterNotPositive,
                    "El peralte combinado del cuerpo debe ser un numero positivo; se recibio " +
                    Format(combinedSectionHeight) + "."));
            }

            if (diagnostics.Any(d => d.IsBlocking))
            {
                return CantileverArmConnectionMetrics.Blocked(side, diagnostics);
            }

            // ---- the elevations, from the ONE grid authority -----------------------------------------------
            if (!grid.TryElevationAt(lowerPunchIndex, out var first) ||
                !grid.TryElevationAt(upper, out var last))
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.StationPunchIndexDomainOverflow,
                    "Las elevaciones de los indices " + lowerPunchIndex + ".." + upper +
                    " no estan definidas en la reticula."));
                return CantileverArmConnectionMetrics.Blocked(side, diagnostics);
            }

            var angle = CantileverArmFrameResolver.AngleRadians(slopeRisePer12);

            var metrics = CantileverArmConnectionMetrics.Create(
                side, lowerPunchIndex, verticalPunchCount, verticalEndOffset.Value,
                slopeRisePer12, angle, combinedSectionHeight, first, last, diagnostics);

            // ---- and finally, does the body fit the plate its own rows imply? ------------------------------
            if (metrics.PlateTopZ < metrics.BodyTopZ - FitTolerance)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ArmPlateTooShortForBody,
                    "La placa de conexion llega a z = " + Format(metrics.PlateTopZ) +
                    " in y el cuerpo alcanza z = " + Format(metrics.BodyTopZ) +
                    " in en el plano de conexion. Aumenta VerticalPunchCount: mas troqueles extienden la placa " +
                    "hacia arriba. La placa NO se estira sin mas agujeros."));

                return CantileverArmConnectionMetrics.Blocked(side, diagnostics);
            }

            return metrics;
        }

        private static string Format(double value) =>
            value.ToString("0.####", CultureInfo.InvariantCulture);
    }
}
