using System;
using System.Collections.Generic;
using System.Globalization;
using RackCad.Application.Geometry;

namespace RackCad.Application.StructuralSections.Geometry
{
    /// <summary>
    /// Turns a rectangular or square HSS into an outer contour plus one hole.
    ///
    /// The wall thickness of the CONTOUR is always <c>tnom</c>, never <c>tdes</c> — decision 11 of the owner
    /// in I-36A. That has a consequence worth stating plainly instead of discovering later: the tabulated
    /// <c>A</c> that AISC publishes is computed with the DESIGN thickness, so the geometric area of this
    /// contour cannot match it. Measured over the 525 shipped shapes the gap is +8.34 % on average with
    /// <c>tnom</c> and only 1.07 % when the same contour is built with <c>tdes</c>, which is exactly the
    /// 1/0.93 ratio between the two thicknesses. It is a DEFINITION, not a defect, and the geometry carries a
    /// diagnostic saying so.
    ///
    /// The corner radius is DERIVED from what the source publishes: <c>h</c> and <c>b</c> are the FLAT walls,
    /// so the material lost to each rounded corner is <c>(Ht − h)/2</c> vertically and <c>(B − b)/2</c>
    /// horizontally, and both are the same outer radius. The two derivations agree to within 4.762 % over the
    /// whole family, which is what three-significant-figure rounding produces; the mean is used and a
    /// disagreement beyond tolerance degrades the result instead of picking a favourite.
    ///
    /// A square HSS is not special-cased: it is the shape whose two walls happen to be equal.
    /// </summary>
    internal static class HssRectangularSectionGeometryBuilder
    {
        /// <summary>
        /// How far apart the two flat-wall derivations of the corner radius may be, relative to the larger.
        ///
        /// The published flats carry three significant figures, so on a small tube the last digit is worth a
        /// few percent of the derived radius. 10 % comfortably absorbs that — the measured worst case across
        /// the 525 shapes is 4.762 % — while still catching a genuinely inconsistent row.
        /// </summary>
        public const double CornerRadiusAgreementTolerance = 0.10;

        public static StructuralSectionGeometry Build(
            StructuralSectionDefinition section, SectionDetailLevel detail)
        {
            var dimensions = (HssRectangularSectionDimensions)section.Dimensions;
            var diagnostics = new List<SectionGeometryDiagnostic>();

            var depth = SectionGeometrySupport.Require(dimensions.OverallDepth, "Ht", section);
            var width = SectionGeometrySupport.Require(dimensions.OverallWidth, "B", section);
            var wall = SectionGeometrySupport.Require(dimensions.NominalThickness, "tnom", section);

            if (2.0 * wall >= Math.Min(depth, width))
            {
                throw new InvalidOperationException(
                    "La seccion HSS '" + section.SectionId + "' tiene un espesor que consume toda la pared.");
            }

            diagnostics.Add(new SectionGeometryDiagnostic(
                SectionDiagnosticSeverity.Info,
                SectionGeometryDiagnostics.HssContourUsesNominalThickness,
                "El contorno usa el espesor NOMINAL (tnom = " +
                wall.ToString("0.####", CultureInfo.InvariantCulture) +
                "), como fija la decision del dueno. La 'A' que publica AISC se calcula con el espesor de " +
                "diseno, asi que el area geometrica de este contorno no coincide con ella por definicion."));

            var fidelity = detail == SectionDetailLevel.Simplified
                ? SectionFidelity.Simplified
                : SectionFidelity.TabulatedDerived;

            var outerRadius = 0.0;

            if (detail == SectionDetailLevel.Tabulated)
            {
                outerRadius = DeriveCornerRadius(
                    dimensions, depth, width, wall, diagnostics, out var derived);

                if (!derived)
                {
                    fidelity = SectionFidelity.DegradedToSimplified;
                }
            }

            var innerRadius = outerRadius > 0.0 ? outerRadius - wall : 0.0;

            if (outerRadius > 0.0 && innerRadius <= GeometryTolerance.Length)
            {
                innerRadius = 0.0;
                diagnostics.Add(new SectionGeometryDiagnostic(
                    SectionDiagnosticSeverity.Info,
                    SectionGeometryDiagnostics.InnerCornerSquared,
                    "El radio interior derivado (radio exterior menos tnom) no es positivo, asi que el hueco " +
                    "conserva esquinas vivas."));
            }

            var outer = Rectangle(width, depth, outerRadius);
            var hole = Rectangle(width - (2.0 * wall), depth - (2.0 * wall), innerRadius).Reversed();

            return StructuralSectionGeometry.Create(
                section.SectionId,
                StructuralSectionFamily.HssRectangular,
                detail,
                fidelity,
                outer,
                holes: new[] { hole },
                referencePoints: new[]
                {
                    new SectionReferencePoint(SectionReferencePointKind.Centroid, new Point2D(0.0, 0.0))
                },
                diagnostics: diagnostics);
        }

        private static double DeriveCornerRadius(
            HssRectangularSectionDimensions dimensions,
            double depth,
            double width,
            double wall,
            ICollection<SectionGeometryDiagnostic> diagnostics,
            out bool derived)
        {
            derived = false;

            if (!dimensions.FlatDepth.HasValue || !dimensions.FlatWidth.HasValue)
            {
                diagnostics.Add(new SectionGeometryDiagnostic(
                    SectionDiagnosticSeverity.Degraded,
                    SectionGeometryDiagnostics.CornerRadiusNotDerivable,
                    "La fuente no publica las paredes planas 'h' y 'b', asi que el radio de esquina no puede " +
                    "derivarse y el contorno queda con esquinas vivas."));
                return 0.0;
            }

            var fromDepth = (depth - dimensions.FlatDepth.Value) / 2.0;
            var fromWidth = (width - dimensions.FlatWidth.Value) / 2.0;

            if (fromDepth <= GeometryTolerance.Length || fromWidth <= GeometryTolerance.Length)
            {
                diagnostics.Add(new SectionGeometryDiagnostic(
                    SectionDiagnosticSeverity.Degraded,
                    SectionGeometryDiagnostics.CornerRadiusNotDerivable,
                    "Las paredes planas igualan o superan las dimensiones totales, asi que no queda esquina " +
                    "que redondear."));
                return 0.0;
            }

            var largest = Math.Max(fromDepth, fromWidth);

            if (Math.Abs(fromDepth - fromWidth) / largest > CornerRadiusAgreementTolerance)
            {
                diagnostics.Add(new SectionGeometryDiagnostic(
                    SectionDiagnosticSeverity.Degraded,
                    SectionGeometryDiagnostics.CornerRadiusInconsistent,
                    "Las dos derivaciones del radio de esquina discrepan: (Ht-h)/2 = " +
                    fromDepth.ToString("0.####", CultureInfo.InvariantCulture) + " y (B-b)/2 = " +
                    fromWidth.ToString("0.####", CultureInfo.InvariantCulture) +
                    ". Elegir una seria arbitrario, asi que el contorno queda con esquinas vivas."));
                return 0.0;
            }

            var radius = (fromDepth + fromWidth) / 2.0;

            if (radius >= Math.Min(depth, width) / 2.0 || radius < wall)
            {
                diagnostics.Add(new SectionGeometryDiagnostic(
                    SectionDiagnosticSeverity.Degraded,
                    SectionGeometryDiagnostics.CornerRadiusNotPhysical,
                    "El radio derivado " + radius.ToString("0.####", CultureInfo.InvariantCulture) +
                    " no es fisicamente posible para estas dimensiones: el contorno queda con esquinas vivas."));
                return 0.0;
            }

            derived = true;
            return radius;
        }

        /// <summary>A rectangle centred on the origin, counter-clockwise, square-cornered when radius is zero.</summary>
        internal static ClosedContour2D Rectangle(double width, double height, double radius)
        {
            var hw = width / 2.0;
            var hh = height / 2.0;

            if (radius <= GeometryTolerance.Length)
            {
                return ClosedContour2D.FromPolygon(new[]
                {
                    new Point2D(-hw, -hh), new Point2D(hw, -hh), new Point2D(hw, hh), new Point2D(-hw, hh)
                });
            }

            var ix = hw - radius;
            var iy = hh - radius;

            return ClosedContour2D.Create(new[]
            {
                PathSegment2D.Line(new Point2D(-ix, -hh), new Point2D(ix, -hh)),
                PathSegment2D.QuarterArc(new Point2D(ix, -iy), radius, -Math.PI / 2.0, true),
                PathSegment2D.Line(new Point2D(hw, -iy), new Point2D(hw, iy)),
                PathSegment2D.QuarterArc(new Point2D(ix, iy), radius, 0.0, true),
                PathSegment2D.Line(new Point2D(ix, hh), new Point2D(-ix, hh)),
                PathSegment2D.QuarterArc(new Point2D(-ix, iy), radius, Math.PI / 2.0, true),
                PathSegment2D.Line(new Point2D(-hw, iy), new Point2D(-hw, -iy)),
                PathSegment2D.QuarterArc(new Point2D(-ix, -iy), radius, Math.PI, true)
            });
        }
    }
}
