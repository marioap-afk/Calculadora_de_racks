using System;
using System.Collections.Generic;
using RackCad.Application.Geometry;

namespace RackCad.Application.StructuralSections.Geometry
{
    /// <summary>
    /// Turns an American Standard channel into a contour.
    ///
    /// CANONICAL ORIENTATION, and it matters because a mirrored channel is a different part: the BACK OF THE
    /// WEB faces −X and the flanges OPEN TOWARDS +X. The web is the vertical band on the left; both flanges
    /// reach to the right. The depth <c>d</c> runs along Y and the flange width <c>bf</c> along X.
    ///
    /// The contour is built with the back of the web on x = 0 and mid-depth on y = 0, and then moved left by
    /// the tabulated <c>x</c>, which the source measures from precisely that face. That is why the reference
    /// point <see cref="SectionReferencePointKind.ChannelWebBack"/> exists: it records where the datum was.
    ///
    /// Two honest limitations, both reported rather than hidden:
    ///
    /// - the real flanges are TAPERED (the standard 1:6 inner slope) and this contour uses the tabulated MEAN
    ///   thickness <c>tf</c>, which preserves the flange AREA exactly but not its shape;
    /// - AISC rounds the flange TOE and does not publish that radius, so the toe stays sharp. Measured over
    ///   the 32 shipped channels the area lands within 5.545 % of the published <c>A</c>, with 3 rows above
    ///   5 %; modelling a plausible toe radius would bring it to 3.895 % and would be inventing a dimension.
    ///
    /// Because of those two, a channel never claims <see cref="SectionFidelity.TabulatedComplete"/>.
    /// </summary>
    internal static class ChannelSectionGeometryBuilder
    {
        public static StructuralSectionGeometry Build(
            StructuralSectionDefinition section, SectionDetailLevel detail)
        {
            var dimensions = (ChannelSectionDimensions)section.Dimensions;
            var diagnostics = new List<SectionGeometryDiagnostic>();

            var depth = SectionGeometrySupport.Require(dimensions.Depth, "d", section);
            var flangeWidth = SectionGeometrySupport.Require(dimensions.FlangeWidth, "bf", section);
            var webThickness = SectionGeometrySupport.Require(dimensions.WebThickness, "tw", section);
            var flangeThickness = SectionGeometrySupport.Require(dimensions.FlangeThickness, "tf", section);

            if (webThickness >= flangeWidth || 2.0 * flangeThickness >= depth)
            {
                throw new InvalidOperationException(
                    "La seccion C '" + section.SectionId + "' tiene dimensiones incoherentes.");
            }

            var halfDepth = depth / 2.0;
            var innerY = halfDepth - flangeThickness;

            var fidelity = detail == SectionDetailLevel.Simplified
                ? SectionFidelity.Simplified
                : SectionFidelity.TabulatedDerived;

            double filletRadius = 0.0;

            if (detail == SectionDetailLevel.Tabulated)
            {
                filletRadius = SectionGeometrySupport.DeriveRootFillet(
                    dimensions.KDesign, flangeThickness, flangeWidth - webThickness, innerY,
                    diagnostics, out var derived);

                if (!derived)
                {
                    fidelity = SectionFidelity.DegradedToSimplified;
                }
                else
                {
                    diagnostics.Add(SectionGeometrySupport.ToeRoundingNotPublished());
                    diagnostics.Add(new SectionGeometryDiagnostic(
                        SectionDiagnosticSeverity.Info,
                        SectionGeometryDiagnostics.ChannelFlangeTaperNotModelled,
                        "Los patines reales de un canal son conicos; este contorno usa el espesor MEDIO que " +
                        "tabula la fuente, lo que conserva exactamente el AREA del patin pero no su forma."));
                }
            }

            var contour = filletRadius > 0.0
                ? BuildWithFillets(depth, flangeWidth, webThickness, halfDepth, innerY, filletRadius)
                : BuildSharp(flangeWidth, webThickness, halfDepth, innerY);

            // The source measures x from the back of the web, which is where this contour has x = 0.
            var centroidX = dimensions.CentroidX;
            var referencePoints = new List<SectionReferencePoint>();

            if (centroidX.HasValue && GeometryTolerance.IsFinite(centroidX.Value))
            {
                contour = SectionGeometrySupport.CentreOn(contour, centroidX.Value, 0.0);
                referencePoints.Add(new SectionReferencePoint(
                    SectionReferencePointKind.ChannelWebBack, new Point2D(-centroidX.Value, 0.0)));

                if (dimensions.ShearCenterX.HasValue && GeometryTolerance.IsFinite(dimensions.ShearCenterX.Value))
                {
                    // eo is measured from the same datum, and the source publishes it towards the OPEN side,
                    // i.e. on the far side of the web from the flanges.
                    referencePoints.Add(new SectionReferencePoint(
                        SectionReferencePointKind.ShearCenter,
                        new Point2D(-centroidX.Value - dimensions.ShearCenterX.Value, 0.0)));
                }

                diagnostics.Add(SectionGeometrySupport.CentredWithTabulatedCentroid("x"));
            }
            else
            {
                // Without x there is no published datum; centring on the computed centroid keeps the origin
                // meaningful and the diagnostic says the value did not come from the source.
                contour = SectionGeometrySupport.CentreOn(contour, contour.Centroid.X, contour.Centroid.Y);
                diagnostics.Add(new SectionGeometryDiagnostic(
                    SectionDiagnosticSeverity.Degraded,
                    SectionGeometryDiagnostics.MissingRequiredDimension,
                    "La fuente no publica 'x' para este canal, asi que se centro con el centroide calculado " +
                    "del contorno y no con el valor tabulado."));
                fidelity = SectionFidelity.DegradedToSimplified;
            }

            referencePoints.Add(new SectionReferencePoint(
                SectionReferencePointKind.Centroid, new Point2D(0.0, 0.0)));

            return StructuralSectionGeometry.Create(
                section.SectionId,
                StructuralSectionFamily.Channel,
                detail,
                fidelity,
                contour,
                holes: null,
                referencePoints: referencePoints,
                diagnostics: diagnostics);
        }

        /// <summary>The eight-vertex channel with square corners, counter-clockwise, back of web on x = 0.</summary>
        private static ClosedContour2D BuildSharp(
            double flangeWidth, double webThickness, double halfDepth, double innerY)
        {
            return ClosedContour2D.FromPolygon(new[]
            {
                new Point2D(0.0, -halfDepth),
                new Point2D(flangeWidth, -halfDepth),
                new Point2D(flangeWidth, -innerY),
                new Point2D(webThickness, -innerY),
                new Point2D(webThickness, innerY),
                new Point2D(flangeWidth, innerY),
                new Point2D(flangeWidth, halfDepth),
                new Point2D(0.0, halfDepth)
            });
        }

        /// <summary>The same channel with a root fillet where each flange meets the web.</summary>
        private static ClosedContour2D BuildWithFillets(
            double depth, double flangeWidth, double webThickness, double halfDepth, double innerY, double radius)
        {
            var webEdge = webThickness + radius;
            var flangeEdge = innerY - radius;

            return ClosedContour2D.Create(new[]
            {
                PathSegment2D.Line(new Point2D(0.0, -halfDepth), new Point2D(flangeWidth, -halfDepth)),
                PathSegment2D.Line(new Point2D(flangeWidth, -halfDepth), new Point2D(flangeWidth, -innerY)),
                PathSegment2D.Line(new Point2D(flangeWidth, -innerY), new Point2D(webEdge, -innerY)),
                // Bottom fillet: concave, so it sweeps clockwise while the contour runs counter-clockwise.
                PathSegment2D.Arc(new Point2D(webEdge, -flangeEdge), radius, -Math.PI / 2.0, -Math.PI / 2.0),
                PathSegment2D.Line(new Point2D(webThickness, -flangeEdge), new Point2D(webThickness, flangeEdge)),
                PathSegment2D.Arc(new Point2D(webEdge, flangeEdge), radius, Math.PI, -Math.PI / 2.0),
                PathSegment2D.Line(new Point2D(webEdge, innerY), new Point2D(flangeWidth, innerY)),
                PathSegment2D.Line(new Point2D(flangeWidth, innerY), new Point2D(flangeWidth, halfDepth)),
                PathSegment2D.Line(new Point2D(flangeWidth, halfDepth), new Point2D(0.0, halfDepth)),
                PathSegment2D.Line(new Point2D(0.0, halfDepth), new Point2D(0.0, -halfDepth))
            });
        }
    }
}
