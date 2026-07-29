using System;
using System.Collections.Generic;
using System.Globalization;
using RackCad.Application.Geometry;

namespace RackCad.Application.StructuralSections.Geometry
{
    /// <summary>
    /// The rules the family builders share: how a required dimension is demanded, how a root fillet is
    /// derived, and how a degradation is reported.
    ///
    /// The fillet derivation here is the TABULATED one, <c>r = kdes - t</c>, and it belongs to the families
    /// whose authority is tabulated-constrained. S does not use it: its fillet is a declared convention and
    /// lives in its own builder (ADR-0023).
    ///
    /// It exists so the derivation of <c>r = kdes − t</c> lives in ONE place. Three builders need it, and
    /// three copies of a documented derivation is how a documented derivation stops being one.
    /// </summary>
    internal static class SectionGeometrySupport
    {
        /// <summary>Demands a dimension the family cannot be drawn without. Absence is a defect, not a degradation.</summary>
        public static double Require(double? value, string name, StructuralSectionDefinition section)
        {
            if (!value.HasValue || !GeometryTolerance.IsFinite(value.Value) || value.Value <= 0.0)
            {
                throw new InvalidOperationException(
                    "La seccion '" + section.SectionId + "' no puede dibujarse: falta la dimension obligatoria '" +
                    name + "' o no es positiva.");
            }

            return value.Value;
        }

        /// <summary>
        /// The root fillet radius, derived as <c>kdes − thickness</c>.
        ///
        /// The AISC Readme defines <c>kdes</c> as the distance from the OUTER face of the flange to the toe of
        /// the web fillet. The flange occupies the first <c>tf</c> of that distance, so what remains is the
        /// fillet radius. Nothing else about the fillet is invented.
        ///
        /// Returns 0 and reports a degradation when the value is missing, non-positive, or too large to fit
        /// between the web and the flange tip — a fillet that does not fit would produce a self-crossing
        /// contour, which is worse than a square corner.
        /// </summary>
        public static double DeriveRootFillet(
            double? kDesign,
            double thickness,
            double availableHorizontal,
            double availableVertical,
            ICollection<SectionGeometryDiagnostic> diagnostics,
            out bool derived)
        {
            derived = false;

            if (!kDesign.HasValue || !GeometryTolerance.IsFinite(kDesign.Value))
            {
                diagnostics.Add(new SectionGeometryDiagnostic(
                    SectionDiagnosticSeverity.Degraded,
                    SectionGeometryDiagnostics.FilletNotDerivable,
                    "La fuente no publica 'kdes', asi que el filete de raiz no puede derivarse y las esquinas " +
                    "quedan vivas."));
                return 0.0;
            }

            var radius = kDesign.Value - thickness;

            if (radius <= GeometryTolerance.Length)
            {
                diagnostics.Add(new SectionGeometryDiagnostic(
                    SectionDiagnosticSeverity.Degraded,
                    SectionGeometryDiagnostics.FilletNotDerivable,
                    "El radio derivado 'kdes - espesor' vale " +
                    radius.ToString("0.####", CultureInfo.InvariantCulture) +
                    ", que no es un radio: las esquinas quedan vivas."));
                return 0.0;
            }

            if (radius >= availableHorizontal || radius >= availableVertical)
            {
                diagnostics.Add(new SectionGeometryDiagnostic(
                    SectionDiagnosticSeverity.Degraded,
                    SectionGeometryDiagnostics.FilletDoesNotFit,
                    "El filete de radio " + radius.ToString("0.####", CultureInfo.InvariantCulture) +
                    " no cabe en el espacio disponible: las esquinas quedan vivas."));
                return 0.0;
            }

            derived = true;
            return radius;
        }

        /// <summary>The note every C and L tabulated contour carries: the source does not publish the toe rounding.</summary>
        public static SectionGeometryDiagnostic ToeRoundingNotPublished() =>
            new SectionGeometryDiagnostic(
                SectionDiagnosticSeverity.Info,
                SectionGeometryDiagnostics.ToeRoundingNotPublished,
                "AISC redondea la punta del ala y no publica ese radio, asi que el contorno la deja viva. " +
                "Su area geometrica queda ligeramente por encima de la tabulada; inventar el radio la " +
                "acercaria y seria una exactitud falsa.");

        /// <summary>The note every centred asymmetric contour carries.</summary>
        public static SectionGeometryDiagnostic CentredWithTabulatedCentroid(string variables) =>
            new SectionGeometryDiagnostic(
                SectionDiagnosticSeverity.Info,
                SectionGeometryDiagnostics.CentredWithTabulatedCentroid,
                "La seccion se centro con el valor que TABULA la fuente (" + variables + "), no con el " +
                "centroide calculado del contorno. El residuo entre ambos es el detalle que la fuente no " +
                "publica.");

        /// <summary>Moves a contour so a chosen point lands on the origin.</summary>
        public static ClosedContour2D CentreOn(ClosedContour2D contour, double x, double y) =>
            contour.Transformed(Transform2D.Translation(-x, -y));
    }
}
