using System;
using System.Collections.Generic;
using System.Globalization;
using RackCad.Application.Geometry;

namespace RackCad.Application.StructuralSections.Geometry
{
    /// <summary>
    /// Turns an American Standard beam (AISC Type <c>S</c>, locally IPS) into a contour, under ADR-0023.
    ///
    /// This is the ONLY builder in RackCad whose result is not traceable point-by-point to a published datum,
    /// and it says so: every geometry it produces carries
    /// <see cref="SectionGeometryAuthority.VisualDerived"/> and the mandatory convention diagnostic.
    ///
    /// WHY a convention is needed at all. The AISC Shapes Database v16.0 publishes no flange slope and no
    /// explicit radius, for S or for any other family — measured over the whole workbook, not assumed. But the
    /// sloped inner flange face is precisely what makes an S look like an S: without it the contour is a
    /// parallel-flange I, which is a W. Unlike the channels of I-36B, where the approximation loses detail,
    /// here it would lose the FAMILY. So the slope is declared as RackCad's own and kept out of the catalog.
    ///
    /// THE CONVENTION, constant for all 28 shapes and never tuned per designation:
    ///   s = 1/6, a = (bf − tw)/2
    ///   tRoot = tf + s·a/2      tTip = tf − s·a/2      (so tf is the MEAN thickness of the free overhang)
    ///   delta = kdes − tRoot    r = delta·(sqrt(1+s²) + s)
    /// The outer face of the flange is horizontal, the inner one rises from root to tip with slope exactly
    /// <c>s</c>, <c>kdes</c> fixes the ordinate of the fillet's toe on the web, and <c>r</c> is the arc tangent
    /// to both the web face and the sloped face. The tip stays square.
    ///
    /// <c>r</c> is NOT a free choice: with the toe where <c>kdes</c> puts it, it is the only radius that
    /// satisfies both tangencies. And the whole rule DEGENERATES INTO ADR-0022's: at s = 0 it gives
    /// tRoot = tf and r = kdes − tf, which is literally the W root fillet. S does not fork the geometric
    /// model, it adds the slope term to it.
    ///
    /// The area of the result sits 0.25 %–2.59 % above the tabulated <c>A</c> (mean 1.12 %), always positive
    /// because the contour adds four fillets and omits the tip rounding that would remove material. That
    /// residual is DIAGNOSTIC: nothing here is adjusted to close it, ever.
    /// </summary>
    internal static class SSectionGeometryBuilder
    {
        /// <summary>The declared visual slope of the inner flange face. A RackCad convention, not AISC data.</summary>
        public const double FlangeSlope = 1.0 / 6.0;

        public static StructuralSectionGeometry Build(
            StructuralSectionDefinition section, SectionDetailLevel detail)
        {
            var dimensions = (SSectionDimensions)section.Dimensions;
            var diagnostics = new List<SectionGeometryDiagnostic>
            {
                VisualConvention(),
                TipRoundingNotModelled()
            };

            var depth = SectionGeometrySupport.Require(dimensions.Depth, "d", section);
            var flangeWidth = SectionGeometrySupport.Require(dimensions.FlangeWidth, "bf", section);
            var webThickness = SectionGeometrySupport.Require(dimensions.WebThickness, "tw", section);
            var flangeThickness = SectionGeometrySupport.Require(dimensions.FlangeThickness, "tf", section);

            var halfDepth = depth / 2.0;
            var halfWidth = flangeWidth / 2.0;
            var halfWeb = webThickness / 2.0;
            var overhang = halfWidth - halfWeb;                     // a: the free flange overhang

            if (overhang <= 0.0)
            {
                throw new InvalidOperationException(
                    "La seccion S '" + section.SectionId + "' tiene dimensiones incoherentes: tw debe ser " +
                    "menor que bf para que exista vuelo libre de patin.");
            }

            var half = FlangeSlope * overhang / 2.0;
            var rootThickness = flangeThickness + half;             // tRoot
            var tipThickness = flangeThickness - half;              // tTip

            if (tipThickness <= 0.0 || rootThickness * 2.0 >= depth)
            {
                throw new InvalidOperationException(
                    "La seccion S '" + section.SectionId + "' no admite la conicidad declarada: el espesor de " +
                    "punta resultaria nulo o negativo, o 2·tRaiz no cabe en d.");
            }

            var tipInnerY = halfDepth - tipThickness;               // underside of the flange at the tip
            var rootInnerY = halfDepth - rootThickness;             // underside of the flange at the web face

            var fidelity = detail == SectionDetailLevel.Simplified
                ? SectionFidelity.Simplified
                : SectionFidelity.TabulatedDerived;

            var radius = 0.0;
            var toeY = 0.0;

            if (detail == SectionDetailLevel.Tabulated)
            {
                radius = DeriveVisualFillet(
                    dimensions.KDesign, rootThickness, halfDepth, overhang, diagnostics, out var derived);

                if (derived)
                {
                    toeY = halfDepth - dimensions.KDesign.Value;
                }
                else
                {
                    fidelity = SectionFidelity.DegradedToSimplified;
                    radius = 0.0;
                }
            }

            var contour = radius > 0.0
                ? BuildWithFillets(halfWidth, halfDepth, halfWeb, tipInnerY, toeY, radius)
                : BuildSharp(halfWidth, halfDepth, halfWeb, tipInnerY, rootInnerY);

            return StructuralSectionGeometry.Create(
                section.SectionId,
                StructuralSectionFamily.S,
                detail,
                fidelity,
                contour,
                holes: null,
                originBasis: SectionOriginBasis.Symmetry,
                referencePoints: new[]
                {
                    new SectionReferencePoint(SectionReferencePointKind.TabulatedCentroid, new Point2D(0.0, 0.0))
                },
                diagnostics: diagnostics,
                authority: SectionGeometryAuthority.VisualDerived);
        }

        /// <summary>
        /// The visual fillet radius, <c>r = (kdes − tRoot)·(sqrt(1+s²) + s)</c>.
        ///
        /// Degrades — square root corners, taper kept — when <c>kdes</c> is missing, when the toe would sit
        /// above the root corner (a non-positive <c>delta</c>), when the arc would not fit inside the free
        /// overhang, or when the two fillets of one side would meet (<c>d ≤ 2·kdes</c>). Every one of those is
        /// reported; none is silent.
        /// </summary>
        private static double DeriveVisualFillet(
            double? kDesign,
            double rootThickness,
            double halfDepth,
            double overhang,
            ICollection<SectionGeometryDiagnostic> diagnostics,
            out bool derived)
        {
            derived = false;

            if (!kDesign.HasValue || !GeometryTolerance.IsFinite(kDesign.Value))
            {
                diagnostics.Add(new SectionGeometryDiagnostic(
                    SectionDiagnosticSeverity.Degraded,
                    SectionGeometryDiagnostics.FilletNotDerivable,
                    "La fuente no publica 'kdes', asi que el filete visual no puede situarse y las esquinas " +
                    "de raiz quedan vivas. La conicidad del patin se conserva."));
                return 0.0;
            }

            var delta = kDesign.Value - rootThickness;

            if (delta <= GeometryTolerance.Length)
            {
                diagnostics.Add(new SectionGeometryDiagnostic(
                    SectionDiagnosticSeverity.Degraded,
                    SectionGeometryDiagnostics.FilletNotDerivable,
                    "La distancia 'kdes - tRaiz' vale " +
                    delta.ToString("0.####", CultureInfo.InvariantCulture) +
                    ", asi que el pie del filete no queda por debajo de la raiz: las esquinas quedan vivas."));
                return 0.0;
            }

            var slopeHypotenuse = Math.Sqrt(1.0 + (FlangeSlope * FlangeSlope));
            var radius = delta * (slopeHypotenuse + FlangeSlope);

            // Where the arc meets the sloped face, measured from the web face.
            var tangentOffset = radius * (1.0 - (FlangeSlope / slopeHypotenuse));

            if (tangentOffset >= overhang)
            {
                diagnostics.Add(new SectionGeometryDiagnostic(
                    SectionDiagnosticSeverity.Degraded,
                    SectionGeometryDiagnostics.FilletDoesNotFit,
                    "El filete visual de radio " + radius.ToString("0.####", CultureInfo.InvariantCulture) +
                    " alcanzaria la punta del patin: las esquinas quedan vivas."));
                return 0.0;
            }

            if (kDesign.Value >= halfDepth)
            {
                diagnostics.Add(new SectionGeometryDiagnostic(
                    SectionDiagnosticSeverity.Degraded,
                    SectionGeometryDiagnostics.FilletDoesNotFit,
                    "Con 'kdes' = " + kDesign.Value.ToString("0.####", CultureInfo.InvariantCulture) +
                    " los dos filetes de un mismo lado se encontrarian: las esquinas quedan vivas."));
                return 0.0;
            }

            derived = true;
            return radius;
        }

        /// <summary>The twelve-vertex tapered I with square corners. The taper is present at BOTH detail levels.</summary>
        private static ClosedContour2D BuildSharp(
            double halfWidth, double halfDepth, double halfWeb, double tipInnerY, double rootInnerY)
        {
            return ClosedContour2D.FromPolygon(new[]
            {
                new Point2D(-halfWidth, -halfDepth),
                new Point2D(halfWidth, -halfDepth),
                new Point2D(halfWidth, -tipInnerY),
                new Point2D(halfWeb, -rootInnerY),
                new Point2D(halfWeb, rootInnerY),
                new Point2D(halfWidth, tipInnerY),
                new Point2D(halfWidth, halfDepth),
                new Point2D(-halfWidth, halfDepth),
                new Point2D(-halfWidth, tipInnerY),
                new Point2D(-halfWeb, rootInnerY),
                new Point2D(-halfWeb, -rootInnerY),
                new Point2D(-halfWidth, -tipInnerY)
            });
        }

        /// <summary>
        /// The same tapered I with the four visual fillets.
        ///
        /// Every arc sweeps by the SAME angle, <c>-atan(1/s)</c>: negative because a concave fillet on a
        /// counter-clockwise boundary turns clockwise, and identical across the four because the two tangent
        /// lines make the same angle at each root.
        /// </summary>
        private static ClosedContour2D BuildWithFillets(
            double halfWidth, double halfDepth, double halfWeb, double tipInnerY, double toeY, double radius)
        {
            var slopeHypotenuse = Math.Sqrt(1.0 + (FlangeSlope * FlangeSlope));
            var centreX = halfWeb + radius;
            var offsetX = radius * FlangeSlope / slopeHypotenuse;   // u
            var offsetY = radius / slopeHypotenuse;                 // v

            var tangentX = centreX - offsetX;
            var tangentY = toeY + offsetY;
            var sweep = -Math.Atan2(1.0, FlangeSlope);

            var segments = new List<PathSegment2D>
            {
                // Bottom flange, outer face, left to right.
                PathSegment2D.Line(new Point2D(-halfWidth, -halfDepth), new Point2D(halfWidth, -halfDepth)),
                PathSegment2D.Line(new Point2D(halfWidth, -halfDepth), new Point2D(halfWidth, -tipInnerY)),
                // Sloped inner face of the bottom flange, tip inwards.
                PathSegment2D.Line(new Point2D(halfWidth, -tipInnerY), new Point2D(tangentX, -tangentY)),
                // Bottom-right fillet, ending on the web face.
                PathSegment2D.Arc(
                    new Point2D(centreX, -toeY), radius, Math.Atan2(1.0, FlangeSlope) - Math.PI, sweep),
                // Web, right face, going up.
                PathSegment2D.Line(new Point2D(halfWeb, -toeY), new Point2D(halfWeb, toeY)),
                // Top-right fillet, leaving the web face onto the sloped face.
                PathSegment2D.Arc(new Point2D(centreX, toeY), radius, Math.PI, sweep),
                PathSegment2D.Line(new Point2D(tangentX, tangentY), new Point2D(halfWidth, tipInnerY)),
                PathSegment2D.Line(new Point2D(halfWidth, tipInnerY), new Point2D(halfWidth, halfDepth)),
                // Top flange, outer face, right to left.
                PathSegment2D.Line(new Point2D(halfWidth, halfDepth), new Point2D(-halfWidth, halfDepth)),
                PathSegment2D.Line(new Point2D(-halfWidth, halfDepth), new Point2D(-halfWidth, tipInnerY)),
                PathSegment2D.Line(new Point2D(-halfWidth, tipInnerY), new Point2D(-tangentX, tangentY)),
                // Top-left fillet.
                PathSegment2D.Arc(
                    new Point2D(-centreX, toeY), radius, Math.Atan2(1.0, FlangeSlope), sweep),
                // Web, left face, going down.
                PathSegment2D.Line(new Point2D(-halfWeb, toeY), new Point2D(-halfWeb, -toeY)),
                // Bottom-left fillet.
                PathSegment2D.Arc(new Point2D(-centreX, -toeY), radius, 0.0, sweep),
                PathSegment2D.Line(new Point2D(-tangentX, -tangentY), new Point2D(-halfWidth, -tipInnerY)),
                PathSegment2D.Line(new Point2D(-halfWidth, -tipInnerY), new Point2D(-halfWidth, -halfDepth))
            };

            return ClosedContour2D.Create(segments);
        }

        /// <summary>The note EVERY S geometry carries. Its code is what the type checks for and the UI shows.</summary>
        private static SectionGeometryDiagnostic VisualConvention() =>
            new SectionGeometryDiagnostic(
                SectionDiagnosticSeverity.Info,
                SectionGeometryDiagnostics.VisualConventionApplied,
                "Geometria visual derivada: AISC no publica la pendiente del patin ni ningun radio, asi que " +
                "la inclinacion 1:6 y el filete son una convencion de RackCad (ADR-0023). Es aproximada, no " +
                "esta garantizada por ningun fabricante y no es apta para CNC ni fabricacion. Las dimensiones, " +
                "el area, el peso y las propiedades siguen siendo los que tabula AISC.");

        private static SectionGeometryDiagnostic TipRoundingNotModelled() =>
            new SectionGeometryDiagnostic(
                SectionDiagnosticSeverity.Info,
                SectionGeometryDiagnostics.STipRoundingNotModelled,
                "La punta del patin se dibuja viva: la fuente no publica radio ni chaflan de punta y aqui no " +
                "se inventa ninguno.");
    }
}
