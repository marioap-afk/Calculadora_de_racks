using System;

namespace RackCad.Application.StructuralSections
{
    /// <summary>
    /// The ONE place where a unit is converted in the neutral catalog (ADR-0021 §10).
    ///
    /// Scope, stated so nobody mistakes it for something bigger: this converts DATA of a catalogued section
    /// between the two systems a source may publish in. It does not convert the drawing, does not read or
    /// write <c>INSUNITS</c> and does not reinterpret geometry — ADR-0005 stands untouched, and the inch
    /// remains RackCad's internal geometric unit.
    ///
    /// Every factor is exact by definition, not a rounded literal: 1 lb = 0.45359237 kg and 1 ft = 0.3048 m,
    /// so lb/ft to kg/m is that quotient; 1 in = 25.4 mm exactly, and 1 in.² = 645.16 mm² exactly.
    /// </summary>
    public static class StructuralSectionUnits
    {
        /// <summary>Exact: 1 in = 25.4 mm.</summary>
        public const double InchesToMillimeters = 25.4;

        /// <summary>Exact: 1 in.² = 645.16 mm².</summary>
        public const double SquareInchesToSquareMillimeters = 645.16;

        /// <summary>Exact quotient 0.45359237 kg / 0.3048 m. Computed, so no digit is lost to a literal.</summary>
        public static readonly double PoundsPerFootToKilogramsPerMeter = 0.45359237d / 0.3048d;

        private const double InchesPerFoot = 12d;
        private const double MillimetersPerMeter = 1000d;

        public static double ToKilogramsPerMeter(double poundsPerFoot) =>
            poundsPerFoot * PoundsPerFootToKilogramsPerMeter;

        public static double ToPoundsPerFoot(double kilogramsPerMeter) =>
            kilogramsPerMeter / PoundsPerFootToKilogramsPerMeter;

        public static double ToMillimeters(double inches) => inches * InchesToMillimeters;

        public static double ToSquareMillimeters(double squareInches) =>
            squareInches * SquareInchesToSquareMillimeters;

        /// <summary>The section's linear weight expressed in lb/ft, whatever its native system is.</summary>
        public static double WeightPerLengthInPoundsPerFoot(StructuralSectionDefinition section)
        {
            Require(section);

            return section.NativeUnitSystem == StructuralSectionUnitSystem.UsCustomary
                ? section.WeightPerLength
                : ToPoundsPerFoot(section.WeightPerLength);
        }

        /// <summary>The section's linear weight expressed in kg/m, whatever its native system is.</summary>
        public static double WeightPerLengthInKilogramsPerMeter(StructuralSectionDefinition section)
        {
            Require(section);

            return section.NativeUnitSystem == StructuralSectionUnitSystem.UsCustomary
                ? ToKilogramsPerMeter(section.WeightPerLength)
                : section.WeightPerLength;
        }

        /// <summary>
        /// Weight of a straight run of <paramref name="lengthInches"/> inches, in KILOGRAMS.
        ///
        /// The length is in inches because that is RackCad's internal geometric unit; the section's linear
        /// weight is whatever its source published. This is a generic weight calculation, not a BOM: it
        /// deliberately knows nothing about members, cuts, waste or fittings.
        /// </summary>
        public static double WeightInKilograms(StructuralSectionDefinition section, double lengthInches)
        {
            RequireLength(lengthInches);

            return WeightPerLengthInKilogramsPerMeter(section) * (ToMillimeters(lengthInches) / MillimetersPerMeter);
        }

        /// <summary>Weight of a straight run of <paramref name="lengthInches"/> inches, in POUNDS.</summary>
        public static double WeightInPounds(StructuralSectionDefinition section, double lengthInches)
        {
            RequireLength(lengthInches);

            return WeightPerLengthInPoundsPerFoot(section) * (lengthInches / InchesPerFoot);
        }

        /// <summary>
        /// Weight of a straight run, in the section's OWN native mass unit (pounds for a US source, kilograms
        /// for a metric one). Callers that need a specific unit should say so with the two methods above.
        /// </summary>
        public static double Weight(StructuralSectionDefinition section, double lengthInches)
        {
            Require(section);

            return section.NativeUnitSystem == StructuralSectionUnitSystem.UsCustomary
                ? WeightInPounds(section, lengthInches)
                : WeightInKilograms(section, lengthInches);
        }

        private static void Require(StructuralSectionDefinition section)
        {
            if (section == null)
            {
                throw new ArgumentNullException(nameof(section));
            }

            if (double.IsNaN(section.WeightPerLength) || double.IsInfinity(section.WeightPerLength))
            {
                throw new ArgumentException(
                    "La seccion '" + section.SectionId + "' tiene un peso lineal no finito.", nameof(section));
            }
        }

        private static void RequireLength(double lengthInches)
        {
            if (double.IsNaN(lengthInches) || double.IsInfinity(lengthInches) || lengthInches < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(lengthInches), lengthInches, "La longitud debe ser un numero finito no negativo.");
            }
        }
    }
}
