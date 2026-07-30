using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Geometry;
using RackCad.Domain.Systems.Cantilever;

namespace RackCad.Application.Systems.Cantilever
{
    /// <summary>What one vertical slot of the bracing sequence is.</summary>
    public enum CantileverBracingSlotKind
    {
        /// <summary>Space below the first panel or above the last. Half of the remainder each.</summary>
        ExternalSpace = 0,

        /// <summary>A braced panel: two braces in an X between its two separators.</summary>
        BracedPanel = 1,

        /// <summary>A central empty space between two blocks of panels. Has separators but no braces.</summary>
        CentralEmptySpace = 2
    }

    /// <summary>One slot of the vertical sequence, with the elevations it spans.</summary>
    public readonly struct CantileverBracingSlot
    {
        internal CantileverBracingSlot(CantileverBracingSlotKind kind, double bottomZ, double topZ, int index)
        {
            Kind = kind;
            BottomZ = bottomZ;
            TopZ = topZ;
            Index = index;
        }

        public CantileverBracingSlotKind Kind { get; }

        public double BottomZ { get; }

        public double TopZ { get; }

        /// <summary>Ordinal among slots of the SAME kind, base zero. Panel 0, panel 1, gap 0…</summary>
        public int Index { get; }

        public double Height => TopZ - BottomZ;

        public override string ToString() => string.Format(
            CultureInfo.InvariantCulture, "{0}[{1}] {2:0.##}..{3:0.##}", Kind, Index, BottomZ, TopZ);
    }

    /// <summary>
    /// The vertical layout of one interval's bracing: its slots, its separator elevations and the panels.
    /// </summary>
    public sealed class CantileverBracingLayout
    {
        internal CantileverBracingLayout(
            int bracedPanelCount,
            int centralEmptySpaceCount,
            double bracedPanelHeight,
            double centralEmptySpaceHeight,
            double coreHeight,
            double externalSpaceHeight,
            IReadOnlyList<CantileverBracingSlot> slots,
            IReadOnlyList<double> separatorElevations,
            IReadOnlyList<CantileverBracingSlot> bracedPanels,
            IReadOnlyList<CantileverDiagnostic> diagnostics)
        {
            BracedPanelCount = bracedPanelCount;
            CentralEmptySpaceCount = centralEmptySpaceCount;
            BracedPanelHeight = bracedPanelHeight;
            CentralEmptySpaceHeight = centralEmptySpaceHeight;
            CoreHeight = coreHeight;
            ExternalSpaceHeight = externalSpaceHeight;
            Slots = slots;
            SeparatorElevations = separatorElevations;
            BracedPanels = bracedPanels;
            Diagnostics = diagnostics;
        }

        public int BracedPanelCount { get; }

        public int CentralEmptySpaceCount { get; }

        public double BracedPanelHeight { get; }

        public double CentralEmptySpaceHeight { get; }

        /// <summary>`panels × panelHeight + gaps × gapHeight`. What the bracing occupies, without the extremes.</summary>
        public double CoreHeight { get; }

        /// <summary>Half the remainder. The SAME at the bottom and at the top (ADR-0027, D4).</summary>
        public double ExternalSpaceHeight { get; }

        /// <summary>The whole sequence, bottom to top, external spaces included.</summary>
        public IReadOnlyList<CantileverBracingSlot> Slots { get; }

        /// <summary>
        /// The elevations a separator sits at, ascending: every INTERNAL boundary of the sequence.
        ///
        /// Two adjacent braced panels SHARE the separator between them, and it appears once. That is the whole
        /// content of `SeparatorCountPerInterval = panels + gaps + 1` (ADR-0027, D5).
        /// </summary>
        public IReadOnlyList<double> SeparatorElevations { get; }

        /// <summary>Only the braced panels, in order. Each becomes two braces in an X.</summary>
        public IReadOnlyList<CantileverBracingSlot> BracedPanels { get; }

        public IReadOnlyList<CantileverDiagnostic> Diagnostics { get; }

        public bool IsBlocked => Diagnostics.Any(d => d.IsBlocking);

        public int SeparatorCount => SeparatorElevations.Count;

        /// <summary>Two braces per braced panel.</summary>
        public int BraceCount => BracedPanels.Count * 2;

        /// <summary>The index, within <see cref="SeparatorElevations"/>, of a panel's lower separator.</summary>
        public int LowerSeparatorIndexOf(int panelOrdinal)
        {
            if (panelOrdinal < 0 || panelOrdinal >= BracedPanels.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(panelOrdinal), panelOrdinal, "Panel fuera de rango.");
            }

            var panel = BracedPanels[panelOrdinal];

            for (var i = 0; i < SeparatorElevations.Count; i++)
            {
                if (Math.Abs(SeparatorElevations[i] - panel.BottomZ) <= CantileverBracingLayoutResolver.FitTolerance)
                {
                    return i;
                }
            }

            throw new InvalidOperationException(
                "El panel " + panelOrdinal + " no tiene separador inferior en la secuencia.");
        }

        public string Signature() => string.Format(
            CultureInfo.InvariantCulture,
            "panels={0};gaps={1};ph={2:0.######};gh={3:0.######};core={4:0.######};ext={5:0.######};z={6}",
            BracedPanelCount, CentralEmptySpaceCount, BracedPanelHeight, CentralEmptySpaceHeight,
            CoreHeight, ExternalSpaceHeight,
            string.Join("|", SeparatorElevations.Select(z => z.ToString("0.######", CultureInfo.InvariantCulture))));

        public override string ToString() => "BracingLayout " + Signature();
    }

    /// <summary>
    /// THE authority for how many braced panels a column carries and where they fall.
    ///
    /// The panel count is a RULE and not the twelve-row product table:
    /// <c>max(1, ceil((ColumnHeight − 72 in) / 60 in))</c>. It reproduces all twelve approved rows and keeps
    /// answering for the thirteenth height, which is the difference between a rule and twelve <c>if</c>
    /// (ADR-0027, D4).
    ///
    /// The panels are grouped from the BOTTOM in blocks of at most two, with a central empty space between
    /// blocks and the incomplete block at the TOP; only the remainder is split, equally, between the two
    /// extremes. Distributing uniformly, or pushing the central space into the extremes, produces a drawing
    /// that is not the product.
    /// </summary>
    public static class CantileverBracingLayoutResolver
    {
        internal const double FitTolerance = 1e-9;

        /// <summary>
        /// How many braced panels a column of this height carries, by the product rule.
        ///
        /// Exposed on its own because it is the one number the twelve-row table pins, and a test that checks
        /// the table has to be able to ask for it without building a layout.
        /// </summary>
        public static int StandardBracedPanelCount(double columnHeight)
        {
            if (!GeometryTolerance.IsFinite(columnHeight))
            {
                throw new ArgumentOutOfRangeException(
                    nameof(columnHeight), columnHeight, "La altura de columna debe ser finita.");
            }

            var steps = Math.Ceiling(
                (columnHeight - CantileverLineDefaults.PanelCountBaseHeight) /
                CantileverLineDefaults.PanelCountHeightStep);

            if (!GeometryTolerance.IsFinite(steps) || steps > int.MaxValue)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(columnHeight), columnHeight,
                    "La altura de columna produce una cantidad de paneles fuera del dominio.");
            }

            return Math.Max(1, (int)steps);
        }

        /// <summary>
        /// How many central empty spaces a given panel count implies.
        ///
        /// `floor((panels − 1) / 2)`, which is what «blocks of at most two, incomplete block on top» comes to:
        /// 1→0, 2→0, 3→1, 4→1, 5→2, 6→2.
        /// </summary>
        public static int CentralEmptySpaceCount(int bracedPanelCount)
        {
            if (bracedPanelCount < 1)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(bracedPanelCount), bracedPanelCount, "Una cantidad de paneles debe ser positiva.");
            }

            return (bracedPanelCount - 1) / 2;
        }

        /// <summary>
        /// The shortest column that holds a bracing core, ignoring the external spaces.
        ///
        /// It is what the line's common height has to respect, and it is why a bracing can RAISE a column that
        /// its levels alone would have left shorter (ADR-0027, D2).
        /// </summary>
        public static double MinimumColumnHeightFor(
            int bracedPanelCount, double bracedPanelHeight, double centralEmptySpaceHeight) =>
            (bracedPanelCount * bracedPanelHeight) +
            (CentralEmptySpaceCount(bracedPanelCount) * centralEmptySpaceHeight);

        /// <summary>
        /// Resolves the vertical layout of one interval.
        /// </summary>
        /// <param name="bracing">The line's bracing intent.</param>
        /// <param name="columnHeight">The COMMON column height of the line.</param>
        /// <param name="heightIsManual">
        /// Whether the height came from the user. It decides what a negative remainder means: under an
        /// automatic height the core governs the minimum and the caller will have raised the column, so a
        /// negative remainder here is a programming error; under a manual one it is a BLOCKING diagnostic,
        /// because the alternative is compressing panels behind the user's back (ADR-0027, D4).
        /// </param>
        public static CantileverBracingLayout Resolve(
            CantileverBracingDesign bracing, double columnHeight, bool heightIsManual)
        {
            if (bracing == null)
            {
                throw new ArgumentNullException(nameof(bracing));
            }

            var diagnostics = new List<CantileverDiagnostic>();

            var panelHeight = bracing.BracedPanelHeight;
            var gapHeight = bracing.CentralEmptySpaceHeight;

            RequirePositive(panelHeight, "la altura del panel arriostrado", diagnostics);
            RequirePositive(gapHeight, "la altura del espacio central", diagnostics);

            if (!GeometryTolerance.IsFinite(columnHeight) || columnHeight <= 0.0)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ParameterNotPositive,
                    "La altura de columna para el arriostramiento debe ser positiva; se recibio " +
                    Format(columnHeight) + "."));
            }

            var panels = ResolvePanelCount(bracing, columnHeight, diagnostics);

            if (panels == null || diagnostics.Any(d => d.IsBlocking))
            {
                return Blocked(diagnostics);
            }

            var panelCount = panels.Value;
            var gapCount = CentralEmptySpaceCount(panelCount);
            var core = (panelCount * panelHeight) + (gapCount * gapHeight);
            var remainder = columnHeight - core;

            if (remainder < -FitTolerance)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.BracingDoesNotFitTheColumn,
                    "El arriostramiento necesita " + Format(core) + " in y la columna mide " +
                    Format(columnHeight) + " in. " +
                    (heightIsManual
                        ? "Aumenta la altura manual o baja la cantidad de paneles: los paneles NO se comprimen, " +
                          "los espacios centrales NO se reducen y la cantidad NO se cambia en silencio."
                        : "Con altura automatica esto no deberia ocurrir: el nucleo del arriostramiento gobierna " +
                          "el minimo de la columna.")));
                return Blocked(diagnostics);
            }

            var external = remainder / 2.0;

            // ---- the sequence, bottom to top ---------------------------------------------------------------
            //
            // Blocks of at most TWO panels from the bottom, a central gap between blocks, and the incomplete
            // block LAST. Written as a walk rather than as an index formula so the shape is readable: the
            // shape IS the decision.
            var slots = new List<CantileverBracingSlot>();
            var separators = new List<double>();

            var z = external;
            slots.Add(new CantileverBracingSlot(CantileverBracingSlotKind.ExternalSpace, 0.0, z, 0));

            var placedPanels = 0;
            var placedGaps = 0;

            while (placedPanels < panelCount)
            {
                var inBlock = Math.Min(2, panelCount - placedPanels);

                for (var k = 0; k < inBlock; k++)
                {
                    separators.Add(z);
                    slots.Add(new CantileverBracingSlot(
                        CantileverBracingSlotKind.BracedPanel, z, z + panelHeight, placedPanels));
                    z += panelHeight;
                    placedPanels++;
                }

                if (placedPanels < panelCount)
                {
                    separators.Add(z);
                    slots.Add(new CantileverBracingSlot(
                        CantileverBracingSlotKind.CentralEmptySpace, z, z + gapHeight, placedGaps));
                    z += gapHeight;
                    placedGaps++;
                }
            }

            // The top of the last panel is the last separator. Adding it here — and not inside the loop — is
            // what makes two adjacent panels SHARE the separator between them instead of each declaring one.
            separators.Add(z);
            slots.Add(new CantileverBracingSlot(
                CantileverBracingSlotKind.ExternalSpace, z, z + external, 1));

            if (placedGaps != gapCount)
            {
                throw new InvalidOperationException(
                    "La secuencia coloco " + placedGaps + " espacios centrales y la regla exige " + gapCount + ".");
            }

            if (separators.Count != panelCount + gapCount + 1)
            {
                throw new InvalidOperationException(
                    "La secuencia produjo " + separators.Count + " separadores y la regla exige " +
                    (panelCount + gapCount + 1) + ".");
            }

            return new CantileverBracingLayout(
                panelCount, gapCount, panelHeight, gapHeight, core, external,
                slots,
                separators,
                slots.Where(s => s.Kind == CantileverBracingSlotKind.BracedPanel).ToList(),
                diagnostics);
        }

        /// <summary>
        /// The panel count in force: the rule, or the user's, validated.
        ///
        /// A manual count is NOT clamped to the rule's answer. The user asking for a different number is a
        /// legitimate product decision; asking for zero is not.
        /// </summary>
        private static int? ResolvePanelCount(
            CantileverBracingDesign bracing, double columnHeight, ICollection<CantileverDiagnostic> diagnostics)
        {
            switch (bracing.PanelCountMode)
            {
                case CantileverBracedPanelCountMode.Automatic:
                    if (!GeometryTolerance.IsFinite(columnHeight) || columnHeight <= 0.0)
                    {
                        return null;
                    }

                    return StandardBracedPanelCount(columnHeight);

                case CantileverBracedPanelCountMode.Manual:
                    if (bracing.ManualPanelCount == null)
                    {
                        diagnostics.Add(CantileverDiagnostic.Blocking(
                            CantileverDiagnostics.BracingManualPanelCountMissing,
                            "El modo manual de paneles exige declarar la cantidad."));
                        return null;
                    }

                    if (bracing.ManualPanelCount.Value < 1)
                    {
                        diagnostics.Add(CantileverDiagnostic.Blocking(
                            CantileverDiagnostics.BracingManualPanelCountNotPositive,
                            "La cantidad manual de paneles arriostrados debe ser positiva; se pidieron " +
                            bracing.ManualPanelCount.Value + "."));
                        return null;
                    }

                    return bracing.ManualPanelCount.Value;

                default:
                    diagnostics.Add(CantileverDiagnostic.Blocking(
                        CantileverDiagnostics.BracingPanelCountModeNotSupported,
                        "El modo de cantidad de paneles '" + bracing.PanelCountMode + "' no esta declarado."));
                    return null;
            }
        }

        private static CantileverBracingLayout Blocked(IReadOnlyList<CantileverDiagnostic> diagnostics) =>
            new CantileverBracingLayout(
                0, 0, double.NaN, double.NaN, double.NaN, double.NaN,
                Array.Empty<CantileverBracingSlot>(), Array.Empty<double>(),
                Array.Empty<CantileverBracingSlot>(), diagnostics);

        private static void RequirePositive(
            double value, string what, ICollection<CantileverDiagnostic> diagnostics)
        {
            if (!GeometryTolerance.IsFinite(value) || value <= 0.0)
            {
                diagnostics.Add(CantileverDiagnostic.Blocking(
                    CantileverDiagnostics.ParameterNotPositive,
                    "El valor de " + what + " debe ser un numero positivo; se recibio " + Format(value) + "."));
            }
        }

        private static string Format(double value) =>
            value.ToString("0.####", CultureInfo.InvariantCulture);
    }
}
