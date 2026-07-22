namespace RackCad.Application.Drawing
{
    /// <summary>
    /// Coarse category of a target drawing's units, as far as RackCad's insertion guardrail needs to distinguish them
    /// (initiative I-05, audit finding D4). RackCad authors ALL geometry in inches; this enum exists ONLY to decide
    /// whether inserting that geometry warrants a non-blocking advisory. It is deliberately NOT a units system: there
    /// is no conversion, scaling or reinterpretation anywhere in RackCad (ADR-0005). The mapping from AutoCAD's units
    /// value lives in the Plugin (the sole owner of the AutoCAD API); this type carries no AutoCAD dependency, so the
    /// decision stays pure and unit-testable without AutoCAD (same layering as the pure <c>KindDispatch</c> of I-10).
    /// </summary>
    public enum DrawingUnits
    {
        /// <summary>The drawing declares inches — RackCad's authoring unit. No advisory.</summary>
        Inches,

        /// <summary>The drawing declares no units at all (unitless / INSUNITS = 0). RackCad still assumes inches, so
        /// this is treated as non-inches and DOES warrant an advisory (a unitless drawing is not an assumption that
        /// it is in inches).</summary>
        Unitless,

        /// <summary>The drawing declares some other unit (millimetres, centimetres, metres, feet, …). Advisory warranted.</summary>
        Other
    }

    /// <summary>
    /// Pure policy for the units guardrail (initiative I-05, audit finding D4). It answers ONE question: does inserting
    /// RackCad geometry (always authored in inches) into a drawing of the given units warrant a non-blocking advisory?
    /// Anything that is not inches — including unitless — warrants one. This performs NO conversion, NO scaling and
    /// stores nothing; the advisory TEXT and the AutoCAD <c>INSUNITS</c> read live in the Plugin
    /// (<c>RackUnitsGuard</c>), the only place allowed to touch the AutoCAD API. See ADR-0005.
    /// </summary>
    public static class DrawingUnitsAdvisory
    {
        /// <summary>
        /// True when a fresh insertion into a drawing of <paramref name="units"/> warrants the units advisory. Only
        /// <see cref="DrawingUnits.Inches"/> is quiet; every other category (including <see cref="DrawingUnits.Unitless"/>)
        /// warrants the advisory.
        /// </summary>
        public static bool RequiresInsertionAdvisory(DrawingUnits units) => units != DrawingUnits.Inches;
    }
}
