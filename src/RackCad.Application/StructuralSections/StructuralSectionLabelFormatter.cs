using System;
using System.Globalization;

namespace RackCad.Application.StructuralSections
{
    /// <summary>
    /// Renders a section's weight the way the owner fixed it (decision 18, ADR-0021 §13): the NATIVE unit
    /// first — the one the perfil is designated and bought in — and the equivalent in parentheses.
    ///
    ///   imperial source:  <c>W12X28 — 28 lb/ft (41.7 kg/m)</c>
    ///   metric source:    <c>Designacion — 41.7 kg/m (28 lb/ft)</c>
    ///
    /// Pure by construction: no WPF, no AutoCAD, no culture leakage (everything is
    /// <see cref="CultureInfo.InvariantCulture"/>). The number ALWAYS comes from
    /// <see cref="StructuralSectionDefinition.WeightPerLength"/>; extracting the "28" from <c>W12X28</c> would
    /// happen to work for a W and be nonsense for an HSS.
    /// </summary>
    public static class StructuralSectionLabelFormatter
    {
        public const string PoundsPerFootUnit = "lb/ft";
        public const string KilogramsPerMeterUnit = "kg/m";

        /// <summary>The separator between designation and weight.</summary>
        public const string Separator = " — ";

        /// <summary>
        /// Native value: printed as tabulated. <c>28</c> stays <c>28</c>, not <c>28.0</c>, because that number
        /// IS part of the commercial designation the user recognises.
        /// </summary>
        private const string NativeFormat = "0.###";

        /// <summary>Converted value: one decimal, which is the precision the owner's example fixes (41.7).</summary>
        private const string EquivalentFormat = "0.#";

        /// <summary><c>28 lb/ft</c> — the weight in the source's own unit.</summary>
        public static string FormatNativeWeight(StructuralSectionDefinition section)
        {
            Require(section);

            return section.WeightPerLength.ToString(NativeFormat, CultureInfo.InvariantCulture) + " " +
                   NativeUnitLabel(section.NativeUnitSystem);
        }

        /// <summary><c>41.7 kg/m</c> — the weight in the other unit, computed, never tabulated.</summary>
        public static string FormatEquivalentWeight(StructuralSectionDefinition section)
        {
            Require(section);

            var value = section.NativeUnitSystem == StructuralSectionUnitSystem.UsCustomary
                ? StructuralSectionUnits.WeightPerLengthInKilogramsPerMeter(section)
                : StructuralSectionUnits.WeightPerLengthInPoundsPerFoot(section);

            return value.ToString(EquivalentFormat, CultureInfo.InvariantCulture) + " " +
                   EquivalentUnitLabel(section.NativeUnitSystem);
        }

        /// <summary><c>28 lb/ft (41.7 kg/m)</c> — both units, native first.</summary>
        public static string FormatWeight(StructuralSectionDefinition section)
        {
            return FormatNativeWeight(section) + " (" + FormatEquivalentWeight(section) + ")";
        }

        /// <summary><c>W12X28 — 28 lb/ft (41.7 kg/m)</c> — the full label of the owner's example.</summary>
        public static string FormatWeightWithDesignation(StructuralSectionDefinition section)
        {
            Require(section);

            return section.DisplayName + Separator + FormatWeight(section);
        }

        private static string NativeUnitLabel(StructuralSectionUnitSystem system) =>
            system == StructuralSectionUnitSystem.UsCustomary ? PoundsPerFootUnit : KilogramsPerMeterUnit;

        private static string EquivalentUnitLabel(StructuralSectionUnitSystem system) =>
            system == StructuralSectionUnitSystem.UsCustomary ? KilogramsPerMeterUnit : PoundsPerFootUnit;

        private static void Require(StructuralSectionDefinition section)
        {
            if (section == null)
            {
                throw new ArgumentNullException(nameof(section));
            }

            if (section.Identity == null)
            {
                throw new ArgumentException("La seccion no tiene identidad.", nameof(section));
            }
        }
    }
}
