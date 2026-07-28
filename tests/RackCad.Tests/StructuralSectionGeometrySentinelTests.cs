using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// Sentinelas geometricas: dos por familia mas un angulo DESIGUAL.
    ///
    /// Cada valor esperado se deriva a mano de las columnas que I-36A distribuye, asi que una prueba en rojo
    /// dice exactamente que dimension se leyo mal. La sentinela de orientacion —L8X6X1— es la mas importante
    /// de todas: es la unica que detecta una designacion interpretada al reves.
    /// </summary>
    public class StructuralSectionGeometrySentinelTests
    {
        private const double Tol = 1e-6;

        private static readonly StructuralSectionGeometryFactory Shared =
            new StructuralSectionGeometryFactory(
                new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load());

        private static StructuralSectionGeometry Get(string id, SectionDetailLevel detail) =>
            Shared.Get(StructuralSectionId.Parse(id), detail);

        // ---- W --------------------------------------------------------------------------------------------

        [Fact]
        public void W44X408_HasItsPublishedEnvelopeAndDerivedFillet()
        {
            // d = 44.8, bf = 16.1, tw = 1.22, tf = 2.17, kdes = 2.96  ->  r = kdes - tf = 0.79
            var geometry = Get("AISC-W-W44X408", SectionDetailLevel.Tabulated);

            Assert.True(geometry.Bounds.ApproxEquals(new Bounds2D(-8.05, -22.4, 8.05, 22.4), Tol));
            Assert.Equal(SectionFidelity.TabulatedComplete, geometry.Fidelity);
            Assert.Empty(geometry.Holes);
            Assert.True(geometry.CentroidOffset < 1e-9);

            var fillets = geometry.OuterContour.Segments.Where(s => s.IsArc).ToArray();
            Assert.Equal(4, fillets.Length);
            Assert.All(fillets, arc => Assert.Equal(0.79, arc.Radius, 6));
        }

        [Fact]
        public void W12X26_IsSymmetricAndItsSimplifiedFormHasNoArcs()
        {
            // d = 12.2, bf = 6.49, tw = 0.23, tf = 0.38, kdes = 0.68  ->  r = 0.30
            var simplified = Get("AISC-W-W12X26", SectionDetailLevel.Simplified);
            var tabulated = Get("AISC-W-W12X26", SectionDetailLevel.Tabulated);

            Assert.True(simplified.Bounds.ApproxEquals(new Bounds2D(-3.245, -6.1, 3.245, 6.1), Tol));
            Assert.True(tabulated.Bounds.ApproxEquals(simplified.Bounds, Tol));

            Assert.DoesNotContain(simplified.OuterContour.Segments, s => s.IsArc);
            Assert.Equal(12, simplified.OuterContour.Segments.Count);
            Assert.Equal(SectionFidelity.Simplified, simplified.Fidelity);

            Assert.Equal(4, tabulated.OuterContour.Segments.Count(s => s.IsArc));
            Assert.All(tabulated.OuterContour.Segments.Where(s => s.IsArc),
                arc => Assert.Equal(0.30, arc.Radius, 6));

            // El filete ANADE material, asi que el area tabulada supera a la simplificada.
            Assert.True(tabulated.Area > simplified.Area);
        }

        // ---- HSS ------------------------------------------------------------------------------------------

        [Fact]
        public void HSS34X10X1_HasAHoleAndUsesTheNominalWall()
        {
            // Ht = 34, B = 10, h = 31.2, b = 7.21, tnom = 1, tdes = 0.93
            // r_out = ((34-31.2)/2 + (10-7.21)/2)/2 = (1.4 + 1.395)/2 = 1.3975 ; r_in = r_out - tnom
            var geometry = Get("AISC-HSS-RECT-HSS34X10X1", SectionDetailLevel.Tabulated);

            Assert.True(geometry.Bounds.ApproxEquals(new Bounds2D(-5, -17, 5, 17), Tol));
            Assert.Single(geometry.Holes);
            Assert.Equal(ContourOrientation.Clockwise, geometry.Holes[0].Orientation);
            Assert.True(geometry.CentroidOffset < 1e-9);
            Assert.Equal(SectionFidelity.TabulatedDerived, geometry.Fidelity);

            Assert.All(geometry.OuterContour.Segments.Where(s => s.IsArc),
                arc => Assert.Equal(1.3975, arc.Radius, 6));
            Assert.All(geometry.Holes[0].Segments.Where(s => s.IsArc),
                arc => Assert.Equal(0.3975, arc.Radius, 6));

            // El hueco mide (Ht - 2·tnom) x (B - 2·tnom) usando el espesor NOMINAL, nunca el de diseno.
            Assert.True(geometry.Holes[0].Bounds.ApproxEquals(new Bounds2D(-4, -16, 4, 16), Tol));

            Assert.Contains(geometry.Diagnostics,
                d => d.Code == SectionGeometryDiagnostics.HssContourUsesNominalThickness);
        }

        [Fact]
        public void HSS4X4X1_4_IsSquareAndItsHoleShrinksByTheNominalWall()
        {
            // Ht = B = 4, h = b = 3.3, tnom = 0.25  ->  r_out = 0.35, r_in = 0.10
            var geometry = Get("AISC-HSS-RECT-HSS4X4X_250", SectionDetailLevel.Tabulated);

            Assert.True(geometry.Bounds.ApproxEquals(new Bounds2D(-2, -2, 2, 2), Tol));
            Assert.True(geometry.Holes[0].Bounds.ApproxEquals(new Bounds2D(-1.75, -1.75, 1.75, 1.75), Tol));
            Assert.All(geometry.OuterContour.Segments.Where(s => s.IsArc),
                arc => Assert.Equal(0.35, arc.Radius, 6));
            Assert.All(geometry.Holes[0].Segments.Where(s => s.IsArc),
                arc => Assert.Equal(0.10, arc.Radius, 6));
        }

        [Fact]
        public void Hss_SimplifiedKeepsTheHoleAndDropsTheRoundedCorners()
        {
            var geometry = Get("AISC-HSS-RECT-HSS4X4X_250", SectionDetailLevel.Simplified);

            Assert.Single(geometry.Holes);
            Assert.DoesNotContain(geometry.OuterContour.Segments, s => s.IsArc);
            Assert.DoesNotContain(geometry.Holes[0].Segments, s => s.IsArc);
            Assert.True(geometry.Holes[0].Bounds.ApproxEquals(new Bounds2D(-1.75, -1.75, 1.75, 1.75), Tol));
        }

        // ---- C --------------------------------------------------------------------------------------------

        [Fact]
        public void C15X50_OpensTowardsPositiveX_AndIsNotMirrored()
        {
            // d = 15, bf = 3.72, tw = 0.716, tf = 0.65, x = 0.799 (desde la ESPALDA del alma)
            // La espalda queda en x = -0.799 y la punta del patin en x = 3.72 - 0.799 = 2.921.
            var geometry = Get("AISC-C-C15X50", SectionDetailLevel.Tabulated);

            Assert.True(geometry.Bounds.ApproxEquals(new Bounds2D(-0.799, -7.5, 2.921, 7.5), Tol));

            // La prueba de NO espejeado: hay mas seccion a la derecha del centroide que a la izquierda,
            // porque los patines abren hacia +X. Si el canal saliera espejeado, esto se invertiria.
            Assert.True(geometry.Bounds.MaxX > Math.Abs(geometry.Bounds.MinX));

            var webBack = geometry.ReferencePoints.Single(
                p => p.Kind == SectionReferencePointKind.ChannelWebBack);
            Assert.Equal(-0.799, webBack.Location.X, 6);
            Assert.Equal(geometry.Bounds.MinX, webBack.Location.X, 6);

            Assert.Equal(SectionFidelity.TabulatedDerived, geometry.Fidelity);
            Assert.Equal(2, geometry.OuterContour.Segments.Count(s => s.IsArc));
            Assert.All(geometry.OuterContour.Segments.Where(s => s.IsArc),
                arc => Assert.Equal(1.44 - 0.65, arc.Radius, 6));
        }

        [Fact]
        public void C10X15_3_DeclaresWhatItDoesNotModel()
        {
            var geometry = Get("AISC-C-C10X15_3", SectionDetailLevel.Tabulated);

            Assert.True(geometry.Bounds.ApproxEquals(new Bounds2D(-0.634, -5, 1.966, 5), Tol));

            // Un canal nunca dice "completa": no modela ni el redondeo de punta ni la conicidad del patin,
            // y lo declara en vez de insinuar exactitud.
            Assert.Equal(SectionFidelity.TabulatedDerived, geometry.Fidelity);
            Assert.Contains(geometry.Diagnostics, d => d.Code == SectionGeometryDiagnostics.ToeRoundingNotPublished);
            Assert.Contains(geometry.Diagnostics,
                d => d.Code == SectionGeometryDiagnostics.ChannelFlangeTaperNotModelled);
            Assert.Contains(geometry.Diagnostics,
                d => d.Code == SectionGeometryDiagnostics.CentredWithTabulatedCentroid);
            Assert.False(geometry.IsDegraded);
        }

        // ---- L --------------------------------------------------------------------------------------------

        [Fact]
        public void L12X12X1_3_8_IsAnEqualLegAngleCentredByItsTabulatedXAndY()
        {
            // d = b = 12, t = 1.38, kdes = 2.09, x = y = 3.5  ->  r = 0.71
            var geometry = Get("AISC-L-L12X12X1_3_8", SectionDetailLevel.Tabulated);

            Assert.True(geometry.Bounds.ApproxEquals(new Bounds2D(-3.5, -3.5, 8.5, 8.5), Tol));

            var heel = geometry.ReferencePoints.Single(p => p.Kind == SectionReferencePointKind.AngleHeel);
            Assert.Equal(-3.5, heel.Location.X, 6);
            Assert.Equal(-3.5, heel.Location.Y, 6);

            var fillet = Assert.Single(geometry.OuterContour.Segments, s => s.IsArc);
            Assert.Equal(0.71, fillet.Radius, 6);
        }

        [Fact]
        public void L4X4X1_4_HasOneFilletAndSixSegmentsWhenSimplified()
        {
            var simplified = Get("AISC-L-L4X4X1_4", SectionDetailLevel.Simplified);
            var tabulated = Get("AISC-L-L4X4X1_4", SectionDetailLevel.Tabulated);

            Assert.Equal(6, simplified.OuterContour.Segments.Count);
            Assert.DoesNotContain(simplified.OuterContour.Segments, s => s.IsArc);
            Assert.True(simplified.Bounds.ApproxEquals(new Bounds2D(-1.08, -1.08, 2.92, 2.92), Tol));

            var fillet = Assert.Single(tabulated.OuterContour.Segments, s => s.IsArc);
            Assert.Equal(0.625 - 0.25, fillet.Radius, 6);
            Assert.True(tabulated.Area > simplified.Area);
        }

        [Fact]
        public void L8X6X1_PutsTheLongLegVerticalAndTheShortOneHorizontal()
        {
            // LA sentinela de orientacion. Las columnas dan d = 6 (ala CORTA) y b = 8 (ala LARGA), con
            // x = 1.65 (horizontal) e y = 2.65 (vertical). Construir el ala larga en vertical hace que el
            // centroide calculado caiga en (1.654, 2.654), que son esos dos valores. Al reves caeria en
            // (2.654, 1.654) y TODO angulo desigual saldria transpuesto en silencio.
            var geometry = Get("AISC-L-L8X6X1", SectionDetailLevel.Tabulated);

            // Ancho = ala corta = 6 ; alto = ala larga = 8.
            Assert.Equal(6.0, geometry.Bounds.Width, 6);
            Assert.Equal(8.0, geometry.Bounds.Height, 6);
            Assert.True(geometry.Bounds.Height > geometry.Bounds.Width);

            Assert.True(geometry.Bounds.ApproxEquals(new Bounds2D(-1.65, -2.65, 4.35, 5.35), Tol));

            var heel = geometry.ReferencePoints.Single(p => p.Kind == SectionReferencePointKind.AngleHeel);
            Assert.Equal(-1.65, heel.Location.X, 6);
            Assert.Equal(-2.65, heel.Location.Y, 6);

            // Y el residuo entre el centroide tabulado y el geometrico es pequeno: si las columnas se
            // hubieran intercambiado, seria enorme.
            Assert.True(geometry.CentroidOffset < 0.05, "residuo " + geometry.CentroidOffset);
        }

        [Fact]
        public void EveryUnequalAngle_IsTallerThanItIsWide()
        {
            // La generalizacion de la sentinela anterior a las 76 filas desiguales del catalogo.
            var unequal = Shared.Catalog.ByFamily(StructuralSectionFamily.Angle)
                .Where(s => !((AngleSectionDimensions)s.Dimensions).IsEqualLeg)
                .ToArray();

            Assert.NotEmpty(unequal);

            foreach (var section in unequal)
            {
                var dimensions = (AngleSectionDimensions)section.Dimensions;
                var geometry = Shared.Get(section, SectionDetailLevel.Tabulated);

                Assert.Equal(dimensions.ShortLeg.Value, geometry.Bounds.Width, 6);
                Assert.Equal(dimensions.LongLeg.Value, geometry.Bounds.Height, 6);
            }
        }

        [Fact]
        public void EveryChannel_OpensTowardsPositiveX()
        {
            foreach (var section in Shared.Catalog.ByFamily(StructuralSectionFamily.Channel))
            {
                var geometry = Shared.Get(section, SectionDetailLevel.Tabulated);
                var dimensions = (ChannelSectionDimensions)section.Dimensions;

                Assert.Equal(dimensions.FlangeWidth.Value, geometry.Bounds.Width, 6);
                Assert.Equal(dimensions.Depth.Value, geometry.Bounds.Height, 6);
                Assert.True(geometry.Bounds.MaxX > Math.Abs(geometry.Bounds.MinX), section.SectionId.Value);
            }
        }
    }
}
