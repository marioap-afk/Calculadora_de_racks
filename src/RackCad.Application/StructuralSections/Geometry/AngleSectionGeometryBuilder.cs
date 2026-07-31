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
    /// The root fillet is derived as <c>kdes − t</c>, the same documented rule as W and C.
    ///
    /// <para><b>Las PUNTAS también se redondean, desde la ronda 3 de I-37D.</b> Hasta entonces se dejaban a
    /// escuadra con un motivo declarado —AISC las redondea pero no publica el radio— y el resultado era el
    /// perfil rígido que el dueño rechazó al compararlo con el real. La regla que las sustituye no se eligió
    /// por gusto: se MIDIÓ contra el área publicada de las 137 secciones, que es el único juez independiente
    /// que hay. Radio de punta igual a <b>la mitad del filete de raíz</b>, en las <b>dos</b> esquinas de cada
    /// punta, acotado a medio espesor de ala porque una punta no puede ser más redonda que gruesa es el ala.</para>
    ///
    /// <para>Lo que esa regla consigue, sobre las 137:</para>
    /// <list type="table">
    ///   <item><term>a escuadra (antes)</term><description>sesgo +0.800 %, |error| medio 0.814 %, máximo 3.012 %</description></item>
    ///   <item><term>R/2 en las dos esquinas</term><description>sesgo −0.078 %, |error| medio 0.441 %, máximo 2.316 %</description></item>
    /// </list>
    ///
    /// <para>El sesgo cae un factor diez y las otras dos métricas mejoran a la vez, así que el contorno no
    /// sólo se ve mejor: se PARECE más al perfil real. Las alternativas medidas —R/2 sólo en la esquina
    /// exterior, R/3, t/2, R entero— quedaron todas por detrás en las tres columnas.</para>
    ///
    /// <para>El radio sigue siendo DERIVADO y no publicado, así que un ángulo sigue sin poder reclamar
    /// <see cref="SectionFidelity.TabulatedComplete"/>: mejor aproximación no es lo mismo que exactitud.</para>
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
                ? BuildWithFillet(
                    shortLeg, longLeg, thickness, filletRadius, ToeRadius(filletRadius, thickness))
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
                SectionReferencePointKind.TabulatedCentroid, new Point2D(0.0, 0.0)));

            return StructuralSectionGeometry.Create(
                section.SectionId,
                StructuralSectionFamily.Angle,
                detail,
                fidelity,
                contour,
                holes: null,
                originBasis: SectionOriginBasis.TabulatedCentroid,
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

        /// <summary>
        /// El radio de PUNTA que corresponde a un filete de raíz y un espesor de ala.
        ///
        /// Mitad del filete, y nunca más de medio espesor: un ala no puede redondearse más de lo gruesa que
        /// es. Justo en el tope las dos esquinas se tocan y la punta pasa a ser un semicírculo, que es una
        /// forma legítima —una pletina delgada termina así— y la dibuja <see cref="IsFullNose"/>. El tope
        /// muerde de verdad en las secciones delgadas, donde <c>kdes − t</c> supera al propio espesor.
        ///
        /// Se expone para que la prueba que compara con el área publicada mida LA MISMA regla que se dibuja.
        /// </summary>
        internal static double ToeRadius(double filletRadius, double thickness) =>
            Math.Min(filletRadius / 2.0, thickness / 2.0);

        /// <summary>
        /// Si el radio de punta consume el espesor entero y la punta pasa a ser un semicírculo.
        ///
        /// Ocurre en las secciones delgadas, donde <c>kdes − t</c> supera al propio espesor y el tope de
        /// <see cref="ToeRadius"/> muerde. Se pregunta con la misma tolerancia que el resto de la geometría
        /// para que un redondeo de coma flotante no deje una recta de longitud cero, que es geometría inválida
        /// y no una recta muy corta.
        /// </summary>
        private static bool IsFullNose(double toe, double thickness) =>
            thickness - (2.0 * toe) <= GeometryTolerance.Length;

        /// <summary>
        /// La L con su filete de raíz y sus dos puntas redondeadas.
        ///
        /// Recorrido antihorario desde el talón. Cada punta son dos arcos convexos de un cuarto de vuelta
        /// unidos por el tramo recto del canto del ala; el filete de raíz es cóncavo y por eso barre al revés.
        /// </summary>
        private static ClosedContour2D BuildWithFillet(
            double shortLeg, double longLeg, double thickness, double radius, double toe)
        {
            var corner = thickness + radius;
            var quarter = Math.PI / 2.0;

            var segments = new List<PathSegment2D>();

            // ---- ala corta, hacia +X, y su punta -----------------------------------------------------------
            if (toe > 0.0)
            {
                segments.Add(PathSegment2D.Line(new Point2D(0.0, 0.0), new Point2D(shortLeg - toe, 0.0)));

                if (IsFullNose(toe, thickness))
                {
                    // El radio llega a medio espesor: los dos arcos se tocan y el canto recto entre ellos
                    // desaparece. La punta es entonces UN semicírculo, no dos cuartos y una recta de longitud
                    // cero — que además `PathSegment2D` rechaza, con razón.
                    segments.Add(PathSegment2D.Arc(
                        new Point2D(shortLeg - toe, thickness / 2.0), toe, -quarter, Math.PI));
                }
                else
                {
                    segments.Add(PathSegment2D.Arc(new Point2D(shortLeg - toe, toe), toe, -quarter, quarter));
                    segments.Add(PathSegment2D.Line(new Point2D(shortLeg, toe), new Point2D(shortLeg, thickness - toe)));
                    segments.Add(PathSegment2D.Arc(new Point2D(shortLeg - toe, thickness - toe), toe, 0.0, quarter));
                }

                segments.Add(PathSegment2D.Line(new Point2D(shortLeg - toe, thickness), new Point2D(corner, thickness)));
            }
            else
            {
                segments.Add(PathSegment2D.Line(new Point2D(0.0, 0.0), new Point2D(shortLeg, 0.0)));
                segments.Add(PathSegment2D.Line(new Point2D(shortLeg, 0.0), new Point2D(shortLeg, thickness)));
                segments.Add(PathSegment2D.Line(new Point2D(shortLeg, thickness), new Point2D(corner, thickness)));
            }

            // ---- el filete de raíz: cóncavo, barre en sentido contrario al contorno -------------------------
            segments.Add(PathSegment2D.Arc(new Point2D(corner, corner), radius, -quarter, -quarter));

            // ---- ala larga, hacia +Y, y su punta ------------------------------------------------------------
            if (toe > 0.0)
            {
                segments.Add(PathSegment2D.Line(new Point2D(thickness, corner), new Point2D(thickness, longLeg - toe)));

                if (IsFullNose(toe, thickness))
                {
                    segments.Add(PathSegment2D.Arc(
                        new Point2D(thickness / 2.0, longLeg - toe), toe, 0.0, Math.PI));
                }
                else
                {
                    segments.Add(PathSegment2D.Arc(new Point2D(thickness - toe, longLeg - toe), toe, 0.0, quarter));
                    segments.Add(PathSegment2D.Line(new Point2D(thickness - toe, longLeg), new Point2D(toe, longLeg)));
                    segments.Add(PathSegment2D.Arc(new Point2D(toe, longLeg - toe), toe, quarter, quarter));
                }

                segments.Add(PathSegment2D.Line(new Point2D(0.0, longLeg - toe), new Point2D(0.0, 0.0)));
            }
            else
            {
                segments.Add(PathSegment2D.Line(new Point2D(thickness, corner), new Point2D(thickness, longLeg)));
                segments.Add(PathSegment2D.Line(new Point2D(thickness, longLeg), new Point2D(0.0, longLeg)));
                segments.Add(PathSegment2D.Line(new Point2D(0.0, longLeg), new Point2D(0.0, 0.0)));
            }

            return ClosedContour2D.Create(segments);
        }
    }
}
