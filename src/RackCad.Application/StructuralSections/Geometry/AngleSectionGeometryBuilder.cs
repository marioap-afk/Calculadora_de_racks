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
    /// <para><b>TRES esquinas, TRES reglas distintas.</b> No hay un «redondear todas las esquinas»: cada
    /// una de las tres familias de esquina del perfil se trata aparte, porque físicamente son cosas
    /// distintas.</para>
    ///
    /// <list type="table">
    ///   <item>
    ///     <term>Filete de RAÍZ</term>
    ///     <description>La curva cóncava donde se encuentran las dos alas. Se deriva de <c>kdes − t</c>, la
    ///     misma regla documentada que W y C. Es el radio GRANDE del perfil.</description>
    ///   </item>
    ///   <item>
    ///     <term>Radios de PUNTA</term>
    ///     <description>Las dos esquinas del extremo libre de cada ala. Son PEQUEÑOS, del orden del espesor:
    ///     <see cref="ToeRadius"/>. Nunca llegan a medio espesor, así que las dos esquinas de una punta
    ///     jamás se juntan en una nariz semicircular.</description>
    ///   </item>
    ///   <item>
    ///     <term>Talón EXTERIOR</term>
    ///     <description><b>VIVO.</b> La esquina exterior donde se cruzan las caras de fuera de las dos alas no
    ///     se redondea: ver <see cref="HeelOuterRadius"/>.</description>
    ///   </item>
    /// </list>
    ///
    /// <para><b>La regla de punta se MIDIÓ, no se eligió.</b> El área que publica AISC no depende de nuestro
    /// contorno, así que es el único juez independiente que hay. Sobre las 137 secciones L:</para>
    ///
    /// <list type="table">
    ///   <item><term>a escuadra</term><description>sesgo +0.800 %, |error| medio 0.814 %, máximo 3.012 %</description></item>
    ///   <item><term>t/3</term><description>sesgo +0.243 %, |error| medio 0.549 %, máximo 2.703 %</description></item>
    ///   <item><term><b>min(R/2, 0.45·t)</b></term><description>sesgo <b>+0.018 %</b>, |error| medio <b>0.449 %</b>, máximo <b>2.449 %</b></description></item>
    /// </list>
    ///
    /// <para>Gana en las tres columnas y además el tope de 0.45·t garantiza por construcción que la punta
    /// nunca se cierra en nariz. Una versión anterior de esta ronda topaba en <c>t/2</c> y ahí las dos
    /// esquinas SÍ se tocaban: el ala terminaba en semicírculo y el dueño lo rechazó al verlo. El tope no es
    /// cosmético — es lo que separa una punta redondeada de una nariz.</para>
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
        /// La fracción del espesor en la que se topa el radio de punta.
        ///
        /// Estrictamente menor que un medio, y ahí está todo el asunto: en un medio las dos esquinas de la
        /// punta se tocan y el ala termina en NARIZ semicircular. Un ángulo laminado no hace eso, y el dueño
        /// lo rechazó al verlo dibujado. 0.45 deja un canto recto de 0.1·t entre las dos curvas — poco, pero
        /// existe, y es la diferencia entre una punta redondeada y una nariz.
        /// </summary>
        internal const double ToeRadiusThicknessCap = 0.45;

        /// <summary>
        /// El radio de PUNTA: la mitad del filete de raíz, topado en <see cref="ToeRadiusThicknessCap"/>·t.
        ///
        /// Pequeño y del orden del espesor, que es lo que un ángulo laminado enseña. El tope muerde en las
        /// secciones delgadas, donde <c>kdes − t</c> supera al propio espesor.
        ///
        /// Se expone para que la prueba que compara con el área publicada mida LA MISMA regla que se dibuja.
        /// </summary>
        internal static double ToeRadius(double filletRadius, double thickness) =>
            Math.Min(filletRadius / 2.0, ToeRadiusThicknessCap * thickness);

        /// <summary>
        /// El radio del talón EXTERIOR: CERO, y a propósito.
        ///
        /// La esquina de fuera del talón —donde se cruzan las caras exteriores de las dos alas— sale de la
        /// laminación viva, no redondeada. Existe como constante nombrada, y no como una esquina que
        /// simplemente nadie redondeó, para que quede dicho que es una DECISIÓN y no un olvido: si algún día
        /// la fuente publicara ese radio, éste es el sitio donde entra.
        /// </summary>
        internal const double HeelOuterRadius = 0.0;

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
                // El primer tramo arranca en el TALÓN EXTERIOR, que es un vértice vivo: ver `HeelOuterRadius`.
                segments.Add(PathSegment2D.Line(new Point2D(0.0, 0.0), new Point2D(shortLeg - toe, 0.0)));
                segments.Add(PathSegment2D.Arc(new Point2D(shortLeg - toe, toe), toe, -quarter, quarter));
                segments.Add(PathSegment2D.Line(new Point2D(shortLeg, toe), new Point2D(shortLeg, thickness - toe)));
                segments.Add(PathSegment2D.Arc(new Point2D(shortLeg - toe, thickness - toe), toe, 0.0, quarter));
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
                segments.Add(PathSegment2D.Arc(new Point2D(thickness - toe, longLeg - toe), toe, 0.0, quarter));
                segments.Add(PathSegment2D.Line(new Point2D(thickness - toe, longLeg), new Point2D(toe, longLeg)));
                segments.Add(PathSegment2D.Arc(new Point2D(toe, longLeg - toe), toe, quarter, quarter));

                // …y cierra volviendo al talón exterior, otra vez sin redondearlo.
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
