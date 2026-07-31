using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Geometry;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>
    /// The punch parameters after validation, so that no consumer has to re-check a nullable or re-apply a
    /// default. Every value here is a real number the resolver accepted.
    /// </summary>
    public sealed class CantileverResolvedPunchParameters
    {
        public CantileverResolvedPunchParameters(
            double diameter,
            double horizontalEndOffset,
            double connectionPitch,
            double rearPlateVerticalEndOffset,
            double regularColumnPitch,
            int connectionPunchesAboveBase,
            double columnBottomPlatePitch,
            double legacyBottomPlateEndOffset,
            double legacyColumnTopPunchOffset)
        {
            Diameter = diameter;
            HorizontalEndOffset = horizontalEndOffset;
            ConnectionPitch = connectionPitch;
            RearPlateVerticalEndOffset = rearPlateVerticalEndOffset;
            RegularColumnPitch = regularColumnPitch;
            ConnectionPunchesAboveBase = connectionPunchesAboveBase;
            ColumnBottomPlatePitch = columnBottomPlatePitch;
            ColumnBottomPlateEndOffset = legacyBottomPlateEndOffset;
            ColumnTopPunchOffset = legacyColumnTopPunchOffset;
        }

        public double Diameter { get; }

        public double HorizontalEndOffset { get; }

        public double ConnectionPitch { get; }

        public double RearPlateVerticalEndOffset { get; }

        public double RegularColumnPitch { get; }

        public int ConnectionPunchesAboveBase { get; }

        public double ColumnBottomPlatePitch { get; }

        /// <summary>
        /// LEGACY, always zero, read by nothing.
        ///
        /// It was a margin the design had to supply until the owner rejected it as a parameter without product
        /// utility (I-37D, ronda 2). What limits a hole is the hole itself: the rule is now the radius and it
        /// lives in the pattern and in the grid. The property stays because the type is part of the I-37A API
        /// already integrated in main; it is not deleted, it is emptied.
        /// </summary>
        public double ColumnBottomPlateEndOffset { get; }

        /// <summary>LEGACY, always zero, read by nothing. See <see cref="ColumnBottomPlateEndOffset"/>.</summary>
        public double ColumnTopPunchOffset { get; }
    }

    /// <summary>
    /// THE shared authority of the column–base connection: the two transverse punch columns and the
    /// elevations both sides drill at.
    ///
    /// It is computed ONCE and consumed, unchanged, by the base's rear plate and by the column's connecting
    /// face. There is deliberately no second algorithm: the precedent in this repository is PB-004, where a
    /// vertical magnitude "emerged from two independent snaps plus a jump between two datums" and cost four
    /// rejected owner validations.
    ///
    /// Who governs what:
    /// <list type="bullet">
    ///   <item>the <b>COLUMN</b> governs the two transverse coordinates — they are its envelope inset by the
    ///   horizontal offset, so the base has to ACCEPT them, not define them;</item>
    ///   <item>the <b>BASE</b> governs the vertical origin — every elevation is measured from the bottom
    ///   edge of its section envelope.</item>
    /// </list>
    /// </summary>
    public sealed class CantileverColumnBaseConnectionPattern
    {
        /// <summary>Slack used when deciding whether an elevation still falls inside the base, inches.</summary>
        private const double FitTolerance = 1e-9;

        private CantileverColumnBaseConnectionPattern(
            double leftRowX,
            double rightRowX,
            IReadOnlyList<double> elevations,
            double lastElevationInsideBase,
            int punchesInsideBase,
            double baseBottomZ,
            double baseTopZ,
            double rearPlateTopZ,
            CantileverResolvedPunchParameters parameters,
            IReadOnlyList<CantileverDiagnostic> diagnostics)
        {
            LeftRowX = leftRowX;
            RightRowX = rightRowX;
            Elevations = elevations;
            LastElevationInsideBase = lastElevationInsideBase;
            PunchesInsideBase = punchesInsideBase;
            BaseBottomZ = baseBottomZ;
            BaseTopZ = baseTopZ;
            RearPlateTopZ = rearPlateTopZ;
            Parameters = parameters;
            Diagnostics = diagnostics;
        }

        /// <summary>Transverse coordinate of the left punch column. Derived from the COLUMN envelope.</summary>
        public double LeftRowX { get; }

        /// <summary>Transverse coordinate of the right punch column. Derived from the COLUMN envelope.</summary>
        public double RightRowX { get; }

        /// <summary>The two transverse coordinates, left first.</summary>
        public IReadOnlyList<double> RowX => new[] { LeftRowX, RightRowX };

        /// <summary>Every connection elevation, ascending: those inside the base first, then the three above.</summary>
        public IReadOnlyList<double> Elevations { get; }

        /// <summary>The last elevation that still falls within the base section envelope.</summary>
        public double LastElevationInsideBase { get; }

        /// <summary>How many elevations fall within the base section envelope. At least one.</summary>
        public int PunchesInsideBase { get; }

        /// <summary>The highest connection elevation.</summary>
        public double LastConnectionElevation => Elevations[Elevations.Count - 1];

        public double BaseBottomZ { get; }

        public double BaseTopZ { get; }

        /// <summary>Top edge of the rear plate: the last connection elevation plus the vertical end offset.</summary>
        public double RearPlateTopZ { get; }

        /// <summary>Height of the rear plate: from the bottom of the base section to its top edge.</summary>
        public double RearPlateHeight => RearPlateTopZ - BaseBottomZ;

        /// <summary>How far the rear plate reaches ABOVE the base section. It is the gusset's vertical leg.</summary>
        public double TopExtension => RearPlateTopZ - BaseTopZ;

        public CantileverResolvedPunchParameters Parameters { get; }

        public IReadOnlyList<CantileverDiagnostic> Diagnostics { get; }

        /// <summary>
        /// Builds the pattern, or returns null with a blocking diagnostic already in
        /// <paramref name="diagnostics"/>.
        /// </summary>
        /// <param name="columnMinX">World X of the column envelope's lower edge. Ahora sólo LIMITA.</param>
        /// <param name="columnMaxX">World X of the column envelope's upper edge. Ahora sólo LIMITA.</param>
        /// <param name="rearPlateMinX">World X of the rear plate's lower edge. Es el DATUM horizontal.</param>
        /// <param name="rearPlateMaxX">World X of the rear plate's upper edge. Es el DATUM horizontal.</param>
        /// <param name="baseBottomZ">Bottom of the base section envelope.</param>
        /// <param name="baseTopZ">Top of the base section envelope.</param>
        public static CantileverColumnBaseConnectionPattern Build(
            double columnMinX,
            double columnMaxX,
            double rearPlateMinX,
            double rearPlateMaxX,
            double baseBottomZ,
            double baseTopZ,
            CantileverResolvedPunchParameters parameters,
            ICollection<CantileverDiagnostic> diagnostics)
        {
            if (parameters == null)
            {
                throw new ArgumentNullException(nameof(parameters));
            }

            if (diagnostics == null)
            {
                throw new ArgumentNullException(nameof(diagnostics));
            }

            var local = new List<CantileverDiagnostic>();

            // ---- horizontal: la COLUMNA gobierna, pero la pulgada se mide en la PLACA ---------------------
            // Decisión del dueño (I-37D, corrección de ronda 2): «es desde el exterior de la placa hacia el
            // centro de la columna 1 pulgada». La columna sigue gobernando el patrón —son SUS filas, las
            // mismas que suben por la rejilla regular y a las que un brazo se atornilla— pero el borde desde
            // el que se acota es el de la placa posterior.
            //
            // Y es el datum físicamente correcto. Un troquel de esta pareja atraviesa la placa Y la columna,
            // así que el que decide si el agujero se sale es el miembro MÁS ANGOSTO, y la placa lo es: su
            // ancho es el del patín de la base, no el de la columna. Acotar desde la columna hacía que una
            // columna más ancha que su placa empujara los agujeros fuera de ella —exactamente lo que pasaba
            // con la pareja de referencia al bajar el offset a 1 in—.
            var leftRowX = rearPlateMinX + parameters.HorizontalEndOffset;
            var rightRowX = rearPlateMaxX - parameters.HorizontalEndOffset;

            // The two rows are centre-to-centre, so they only clear each other if they are at least one
            // diameter apart. This subsumes the old "right <= left" check and reports it with its own code:
            // rows that merge into a slot are not the same defect as holes that fall off the PLATE, and
            // reusing that message sent the reader to look at the base when the problem was the column.
            var separation = rightRowX - leftRowX;

            if (separation + FitTolerance < parameters.Diameter)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.PunchRowsOverlap,
                    "Las dos filas de troqueles quedarian a " + Format(separation) +
                    " in una de otra, menos que el diametro (" + Format(parameters.Diameter) +
                    " in): la placa posterior es demasiado estrecha para el offset de " +
                    Format(parameters.HorizontalEndOffset) + " in."));
                return null;
            }

            // El agujero atraviesa la placa Y la columna, asi que tambien tiene que caber en la COLUMNA. Es la
            // misma comprobacion de antes con los dos papeles cambiados: al acotar desde la placa, la pieza
            // que puede quedarse corta es la columna, y una columna mas ANGOSTA que su placa es lo que la
            // rompe. Ensanchar cualquiera de las dos seria inventar una regla que nadie aprobo, asi que una
            // pareja incompatible se rechaza y se dice cual de las dos no da.
            var radius = parameters.Diameter / 2.0;

            if (leftRowX - radius < columnMinX - FitTolerance ||
                rightRowX + radius > columnMaxX + FitTolerance)
            {
                var outside = CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.PunchOutsideRearPlate,
                    "Los troqueles acotados desde la placa posterior (x = " + Format(leftRowX) + " y " +
                    Format(rightRowX) + ", diametro " + Format(parameters.Diameter) +
                    " in) no caben en la columna, que abarca de " +
                    Format(columnMinX) + " a " + Format(columnMaxX) + " in.");
                diagnostics.Add(outside);
                return null;
            }

            // ---- vertical: the BASE governs the origin -----------------------------------------------------
            var firstZ = baseBottomZ + parameters.RearPlateVerticalEndOffset;

            if (firstZ > baseTopZ + FitTolerance)
            {
                var noFit = CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.NoPunchFitsInBase,
                    "Ningun troquel de conexion cabe dentro de la seccion de la base: el primero quedaria en z = " +
                    Format(firstZ) + " in y la seccion termina en z = " + Format(baseTopZ) + " in.");
                diagnostics.Add(noFit);
                return null;
            }

            var lastInsideIndex = (int)Math.Floor(((baseTopZ - firstZ) / parameters.ConnectionPitch) + FitTolerance);
            var lastInsideZ = firstZ + (lastInsideIndex * parameters.ConnectionPitch);

            var elevations = new List<double>();

            for (var i = 0; i <= lastInsideIndex; i++)
            {
                elevations.Add(firstZ + (i * parameters.ConnectionPitch));
            }

            // Exactly N above the base section, by the owner's decision. The count is the INPUT; the plate
            // height is what follows from it — not the other way round.
            for (var k = 1; k <= parameters.ConnectionPunchesAboveBase; k++)
            {
                elevations.Add(lastInsideZ + (k * parameters.ConnectionPitch));
            }

            var rearPlateTopZ = elevations[elevations.Count - 1] + parameters.RearPlateVerticalEndOffset;

            return new CantileverColumnBaseConnectionPattern(
                leftRowX,
                rightRowX,
                elevations,
                lastInsideZ,
                lastInsideIndex + 1,
                baseBottomZ,
                baseTopZ,
                rearPlateTopZ,
                parameters,
                local);
        }

        /// <summary>
        /// The datum of one connection punch. Both consumers call THIS, so the two sides cannot disagree
        /// even in principle: there is one function and it takes no side-specific argument.
        /// </summary>
        public CantileverPunchDatum DatumAt(int rowIndex, int elevationIndex)
        {
            if (rowIndex < 0 || rowIndex > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(rowIndex), "Solo hay dos filas de conexion.");
            }

            if (elevationIndex < 0 || elevationIndex >= Elevations.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(elevationIndex));
            }

            return new CantileverPunchDatum(
                CantileverPunchAxis.AlongY,
                rowIndex == 0 ? LeftRowX : RightRowX,
                Elevations[elevationIndex],
                Parameters.Diameter);
        }

        /// <summary>Every connection datum, row by row, ascending in elevation.</summary>
        public IReadOnlyList<CantileverPunchDatum> AllDatums()
        {
            var all = new List<CantileverPunchDatum>(2 * Elevations.Count);

            for (var row = 0; row < 2; row++)
            {
                for (var i = 0; i < Elevations.Count; i++)
                {
                    all.Add(DatumAt(row, i));
                }
            }

            return all;
        }

        /// <summary>Deterministic fingerprint, rounded like the section plans of I-36B.</summary>
        public string Signature()
        {
            var parts = new List<string>
            {
                "rows=" + Format6(LeftRowX) + "," + Format6(RightRowX),
                "d=" + Format6(Parameters.Diameter),
                "base=" + Format6(BaseBottomZ) + ".." + Format6(BaseTopZ),
                "inside=" + PunchesInsideBase.ToString(CultureInfo.InvariantCulture),
                "above=" + Parameters.ConnectionPunchesAboveBase.ToString(CultureInfo.InvariantCulture),
                "top=" + Format6(RearPlateTopZ),
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
