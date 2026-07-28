using System;
using System.Collections.Generic;
using RackCad.Application.Geometry;

namespace RackCad.Application.StructuralSections.Geometry
{
    /// <summary>
    /// Turns a single angle into a contour.
    ///
    /// CANONICAL ORIENTATION, verified against the published data rather than assumed: the LONG leg runs
    /// vertically along +Y, the SHORT leg horizontally along +X, and the HEEL — the outer corner where they
    /// meet — is the construction origin. The check is arithmetic and reproducible on <c>L8X6X1</c>, whose
    /// columns give <c>d = 6</c> (short leg), <c>b = 8</c> (long leg), <c>t = 1</c>, <c>x = 1.65</c> and
    /// <c>y = 2.65</c>: building the long leg vertical puts the computed centroid at (1.654, 2.654), which is
    /// those two tabulated values. Building it the other way round would put them at (2.654, 1.654) and every
    /// unequal angle would be silently transposed.
    ///
    /// So <c>x</c> is the horizontal distance from the heel and <c>y</c> the vertical one, and the contour is
    /// moved by both to bring the tabulated centroid to the origin.
    ///
    /// The root fillet is derived as <c>kdes − t</c>, the same documented rule as W and C. The leg TIPS are
    /// left sharp: AISC rounds them and does not publish the radius. Over the 137 shipped angles the area
    /// lands within 3.012 % of the published <c>A</c> (0.814 % mean), so an angle never claims
    /// <see cref="SectionFidelity.TabulatedComplete"/>.
    /// </summary>
    internal static class AngleSectionGeometryBuilder
    {
        public static StructuralSectionGeometry Build(
            StructuralSectionDefinition section, SectionDetailLevel detail)
        {
            var dimensions = (AngleSectionDimensions)section.Dimensions;
            var diagnostics = new List<SectionGeometryDiagnostic>();

            var shortLeg = SectionGeometrySupport.Require(dimensions.ShortLeg, "d", section);
            var longLeg = SectionGeometrySupport.Require(dimensions.LongLeg, "b", section);
            var thickness = SectionGeometrySupport.Require(dimensions.Thickness, "t", section);

            if (thickness >= Math.Min(shortLeg, longLeg))
            {
                throw new InvalidOperationException(
                    "La seccion L '" + section.SectionId + "' tiene un espesor mayor o igual que un ala.");
            }

            var fidelity = detail == SectionDetailLevel.Simplified
                ? SectionFidelity.Simplified
                : SectionFidelity.TabulatedDerived;

            double filletRadius = 0.0;

            if (detail == SectionDetailLevel.Tabulated)
            {
                filletRadius = SectionGeometrySupport.DeriveRootFillet(
                    dimensions.KDesign, thickness,
                    shortLeg - thickness, longLeg - thickness,
                    diagnostics, out var derived);

                if (!derived)
                {
                    fidelity = SectionFidelity.DegradedToSimplified;
                }
                else
                {
                    diagnostics.Add(SectionGeometrySupport.ToeRoundingNotPublished());
                }
            }

            // Short leg horizontal (+X), long leg vertical (+Y), heel at the construction origin.
            var contour = filletRadius > 0.0
                ? BuildWithFillet(shortLeg, longLeg, thickness, filletRadius)
                : BuildSharp(shortLeg, longLeg, thickness);

            var referencePoints = new List<SectionReferencePoint>();
            var centroidX = dimensions.CentroidX;
            var centroidY = dimensions.CentroidY;

            if (centroidX.HasValue && centroidY.HasValue &&
                GeometryTolerance.IsFinite(centroidX.Value) && GeometryTolerance.IsFinite(centroidY.Value))
            {
                contour = SectionGeometrySupport.CentreOn(contour, centroidX.Value, centroidY.Value);
                referencePoints.Add(new SectionReferencePoint(
                    SectionReferencePointKind.AngleHeel,
                    new Point2D(-centroidX.Value, -centroidY.Value)));
                diagnostics.Add(SectionGeometrySupport.CentredWithTabulatedCentroid("x e y"));
            }
            else
            {
                contour = SectionGeometrySupport.CentreOn(contour, contour.Centroid.X, contour.Centroid.Y);
                diagnostics.Add(new SectionGeometryDiagnostic(
                    SectionDiagnosticSeverity.Degraded,
                    SectionGeometryDiagnostics.MissingRequiredDimension,
                    "La fuente no publica 'x' o 'y' para este angulo, asi que se centro con el centroide " +
                    "calculado del contorno y no con los valores tabulados."));
                fidelity = SectionFidelity.DegradedToSimplified;
            }

            referencePoints.Add(new SectionReferencePoint(
                SectionReferencePointKind.Centroid, new Point2D(0.0, 0.0)));

            return StructuralSectionGeometry.Create(
                section.SectionId,
                StructuralSectionFamily.Angle,
                detail,
                fidelity,
                contour,
                holes: null,
                referencePoints: referencePoints,
                diagnostics: diagnostics);
        }

        /// <summary>The six-vertex L with square corners, counter-clockwise, heel at the origin.</summary>
        private static ClosedContour2D BuildSharp(double shortLeg, double longLeg, double thickness)
        {
            return ClosedContour2D.FromPolygon(new[]
            {
                new Point2D(0.0, 0.0),
                new Point2D(shortLeg, 0.0),
                new Point2D(shortLeg, thickness),
                new Point2D(thickness, thickness),
                new Point2D(thickness, longLeg),
                new Point2D(0.0, longLeg)
            });
        }

        /// <summary>The same L with the root fillet in the inner corner where the two legs meet.</summary>
        private static ClosedContour2D BuildWithFillet(
            double shortLeg, double longLeg, double thickness, double radius)
        {
            var corner = thickness + radius;

            return ClosedContour2D.Create(new[]
            {
                PathSegment2D.Line(new Point2D(0.0, 0.0), new Point2D(shortLeg, 0.0)),
                PathSegment2D.Line(new Point2D(shortLeg, 0.0), new Point2D(shortLeg, thickness)),
                PathSegment2D.Line(new Point2D(shortLeg, thickness), new Point2D(corner, thickness)),
                // Inner corner fillet: concave, sweeping clockwise while the contour runs counter-clockwise.
                PathSegment2D.Arc(new Point2D(corner, corner), radius, -Math.PI / 2.0, -Math.PI / 2.0),
                PathSegment2D.Line(new Point2D(thickness, corner), new Point2D(thickness, longLeg)),
                PathSegment2D.Line(new Point2D(thickness, longLeg), new Point2D(0.0, longLeg)),
                PathSegment2D.Line(new Point2D(0.0, longLeg), new Point2D(0.0, 0.0))
            });
        }
    }
}
