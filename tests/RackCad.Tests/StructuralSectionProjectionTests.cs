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
    /// La instancia prismatica, la proyeccion ortografica y el plan neutral.
    ///
    /// Aqui se fija la frontera que hace que el preview y AutoCAD no puedan divergir: los dos reciben ESTE
    /// plan y ninguno recalcula una dimension de la seccion.
    /// </summary>
    public class StructuralSectionProjectionTests
    {
        private static readonly StructuralSectionGeometryFactory Shared =
            new StructuralSectionGeometryFactory(
                new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load());

        private const string W = "AISC-W-W12X26";
        private const string Hss = "AISC-HSS-RECT-HSS4X4X_250";
        private const string Channel = "AISC-C-C10X15_3";
        private const string UnequalAngle = "AISC-L-L8X6X1";

        private static StructuralSectionGeometry Geometry(string id, SectionDetailLevel detail = SectionDetailLevel.Tabulated) =>
            Shared.Get(StructuralSectionId.Parse(id), detail);

        private static PrismaticSectionInstance Instance(
            string id, double length, double rotation = 0.0, bool mirrored = false) =>
            PrismaticSectionInstance.Create(StructuralSectionId.Parse(id), length, null, rotation, mirrored);

        private static StructuralSectionRepresentationPlan Plan(
            string id, double length, SectionViewpoint view,
            double rotation = 0.0, bool mirrored = false,
            SectionRepresentationMode mode = SectionRepresentationMode.Wireframe,
            SectionDetailLevel detail = SectionDetailLevel.Tabulated,
            bool axis = false, bool envelope = false) =>
            StructuralSectionPlanBuilder.Build(
                Geometry(id, detail),
                Instance(id, length, rotation, mirrored),
                new SectionRepresentationOptions
                {
                    Viewpoint = view, Mode = mode, Detail = detail,
                    IncludeAxis = axis, IncludeEnvelope = envelope
                });

        // ---- Instancia prismatica -------------------------------------------------------------------------

        [Theory]
        [InlineData(0.0)]
        [InlineData(-1.0)]
        [InlineData(double.NaN)]
        [InlineData(double.PositiveInfinity)]
        public void AnInvalidLengthIsRejected(double length)
        {
            Assert.Throws<ArgumentException>(
                () => PrismaticSectionInstance.Create(StructuralSectionId.Parse(W), length));
        }

        [Fact]
        public void TheSectionDefinitionNeverGainsALength()
        {
            // La longitud vive en la instancia, no en el catalogo. Comprobado por reflexion para que anadir
            // una propiedad Length a la definicion rompa esta prueba.
            var names = typeof(StructuralSectionDefinition).GetProperties().Select(p => p.Name).ToArray();

            Assert.DoesNotContain("Length", names);
            Assert.Contains("Length", typeof(PrismaticSectionInstance).GetProperties().Select(p => p.Name));
        }

        [Fact]
        public void WeightScalesLinearlyWithLength_AndReusesTheCatalogAuthority()
        {
            Assert.True(Shared.Catalog.TryGetById(W, out var section));

            var oneFoot = Instance(W, 12.0).Weight(section);
            var tenFeet = Instance(W, 120.0).Weight(section);

            Assert.Equal(section.WeightPerLength, oneFoot, 9);
            Assert.Equal(oneFoot * 10.0, tenFeet, 9);
        }

        [Fact]
        public void RotationAndMirrorChangeNeitherAreaNorWeight()
        {
            Assert.True(Shared.Catalog.TryGetById(UnequalAngle, out var section));
            var geometry = Geometry(UnequalAngle);

            var plain = Instance(UnequalAngle, 60.0);
            var turned = Instance(UnequalAngle, 60.0, rotation: 37.0, mirrored: true);

            Assert.Equal(plain.Weight(section), turned.Weight(section), 12);

            // La transformacion de la seccion conserva el area: mueve material, no lo crea ni lo destruye.
            var moved = geometry.OuterContour.Transformed(turned.SectionTransform());
            Assert.Equal(geometry.OuterContour.Area, moved.Area, 9);
            Assert.Equal(geometry.OuterContour.Orientation, moved.Orientation);
        }

        [Fact]
        public void AWeightAskedWithTheWrongSectionIsRejected()
        {
            Assert.True(Shared.Catalog.TryGetById(Channel, out var other));

            Assert.Throws<ArgumentException>(() => Instance(W, 24.0).Weight(other));
        }

        // ---- Vistas ----------------------------------------------------------------------------------------

        [Fact]
        public void TheCrossSectionViewDoesNotDependOnTheLength()
        {
            var shortRun = Plan(W, 1.0, SectionViewpoint.CrossSection);
            var longRun = Plan(W, 480.0, SectionViewpoint.CrossSection);

            Assert.True(shortRun.Bounds.ApproxEquals(longRun.Bounds, 1e-9));
            Assert.Equal(
                shortRun.Curves.Select(c => c.Role + ":" + c.Points.Count),
                longRun.Curves.Select(c => c.Role + ":" + c.Points.Count));
        }

        [Fact]
        public void TheCrossSectionViewShowsThePublishedEnvelope()
        {
            // d = 12.2, bf = 6.49  ->  la seccion cabe exactamente en su caja publicada.
            var plan = Plan(W, 24.0, SectionViewpoint.CrossSection);

            Assert.Equal(6.49, plan.Bounds.Width, 6);
            Assert.Equal(12.2, plan.Bounds.Height, 6);
            Assert.Contains(plan.Curves, c => c.Role == SectionCurveRole.OuterContour);
            Assert.DoesNotContain(plan.Curves, c => c.Role == SectionCurveRole.Generatrix);
        }

        [Theory]
        [InlineData(1.0)]
        [InlineData(24.0)]
        [InlineData(120.0)]
        [InlineData(1200.0)]
        public void ALongitudinalViewReflectsTheLengthExactly(double length)
        {
            // Mirando por X, el largo corre a lo ancho del dibujo y el peralte queda vertical.
            var plan = Plan(W, length, SectionViewpoint.LongitudinalX);

            Assert.Equal(length, plan.Bounds.Width, 6);
            Assert.Equal(12.2, plan.Bounds.Height, 6);
            Assert.Equal(length, plan.Length, 9);
        }

        [Fact]
        public void TheOtherLongitudinalViewShowsTheOtherDimension()
        {
            // Mirando por Y, lo que queda vertical es el ancho del patin.
            var plan = Plan(W, 36.0, SectionViewpoint.LongitudinalY);

            Assert.Equal(36.0, plan.Bounds.Width, 6);
            Assert.Equal(6.49, plan.Bounds.Height, 6);
        }

        [Fact]
        public void ALongitudinalViewDrawsBothEndsAndTheGeneratrices()
        {
            var plan = Plan(W, 48.0, SectionViewpoint.LongitudinalX);

            // Dos extremos —sin eliminacion de lineas ocultas, se dibujan los dos— y sus generatrices.
            Assert.Equal(2, plan.CurvesOf(SectionCurveRole.EndProfile).Count());
            Assert.NotEmpty(plan.CurvesOf(SectionCurveRole.Generatrix));
            Assert.All(plan.CurvesOf(SectionCurveRole.Generatrix), c => Assert.Equal(2, c.Points.Count));
        }

        [Fact]
        public void TheIsometricViewIsForeshortenedOnEveryAxis()
        {
            var plan = Plan(W, 24.0, SectionViewpoint.Isometric);

            Assert.Equal(SectionViewKind.Isometric, plan.View);
            Assert.True(plan.Bounds.HasArea);
            Assert.Equal(2, plan.CurvesOf(SectionCurveRole.EndProfile).Count());

            // Una isometrica acorta: la caja no puede ser tan ancha como el largo completo.
            Assert.True(plan.Bounds.Width < 24.0);
        }

        [Fact]
        public void ACustomViewpointIsBuiltFromAValidCameraFrame()
        {
            var view = SectionViewpoint.Custom(new Vector3D(1, 2, 3), Vector3D.UnitY);
            var plan = Plan(W, 24.0, view);

            Assert.Equal(SectionViewKind.Custom, plan.View);
            Assert.True(plan.Bounds.HasArea);
        }

        [Fact]
        public void ACustomViewpointWithADegenerateFrameIsRejected()
        {
            Assert.Throws<ArgumentException>(
                () => SectionViewpoint.Custom(Vector3D.UnitZ, Vector3D.UnitZ));
        }

        // ---- Rotacion y espejo ------------------------------------------------------------------------------

        [Fact]
        public void RotatingAQuarterTurnSwapsTheCrossSectionEnvelope()
        {
            var upright = Plan(W, 24.0, SectionViewpoint.CrossSection);
            var turned = Plan(W, 24.0, SectionViewpoint.CrossSection, rotation: 90.0);

            Assert.Equal(upright.Bounds.Height, turned.Bounds.Width, 6);
            Assert.Equal(upright.Bounds.Width, turned.Bounds.Height, 6);
        }

        [Fact]
        public void ANonOrthogonalRotationStillProducesAValidPlan()
        {
            var plan = Plan(UnequalAngle, 24.0, SectionViewpoint.CrossSection, rotation: 37.0);

            Assert.True(plan.Bounds.HasArea);
            Assert.All(plan.Curves, curve => Assert.All(curve.Points, p =>
            {
                Assert.True(GeometryTolerance.IsFinite(p.X));
                Assert.True(GeometryTolerance.IsFinite(p.Y));
            }));
        }

        [Fact]
        public void MirroringFlipsTheAsymmetricSectionAcrossX()
        {
            var plain = Plan(Channel, 24.0, SectionViewpoint.CrossSection);
            var mirrored = Plan(Channel, 24.0, SectionViewpoint.CrossSection, mirrored: true);

            Assert.Equal(plain.Bounds.Width, mirrored.Bounds.Width, 6);
            Assert.Equal(plain.Bounds.Height, mirrored.Bounds.Height, 6);

            // El canal abre hacia +X; espejeado abre hacia -X.
            Assert.True(plain.Bounds.MaxX > Math.Abs(plain.Bounds.MinX));
            Assert.True(Math.Abs(mirrored.Bounds.MinX) > mirrored.Bounds.MaxX);
        }

        // ---- Huecos, modos y opciones -----------------------------------------------------------------------

        [Fact]
        public void AnHssKeepsItsHoleInTheCrossSectionView()
        {
            var plan = Plan(Hss, 24.0, SectionViewpoint.CrossSection);

            Assert.Single(plan.CurvesOf(SectionCurveRole.Hole));
            Assert.Single(plan.CurvesOf(SectionCurveRole.OuterContour));
        }

        [Fact]
        public void AnHssDrawsItsInteriorInALongitudinalViewToo()
        {
            // Los contornos interiores forman parte del wireframe: sin eliminacion de lineas ocultas se ven.
            var plan = Plan(Hss, 48.0, SectionViewpoint.LongitudinalX);

            Assert.Equal(4, plan.CurvesOf(SectionCurveRole.EndProfile).Count());
        }

        [Fact]
        public void EnvelopeModeDrawsOnlyTheBoundingBox()
        {
            var plan = Plan(W, 24.0, SectionViewpoint.LongitudinalX, mode: SectionRepresentationMode.Envelope);

            var envelope = Assert.Single(plan.Curves);
            Assert.Equal(SectionCurveRole.Envelope, envelope.Role);
            Assert.Equal(4, envelope.Points.Count);
            Assert.True(envelope.IsClosed);
            Assert.Equal(24.0, plan.Bounds.Width, 6);
        }

        [Fact]
        public void AxisModeDrawsTheLongitudinalAxis()
        {
            var plan = Plan(W, 24.0, SectionViewpoint.LongitudinalX, mode: SectionRepresentationMode.Axis);

            var axis = Assert.Single(plan.Curves);
            Assert.Equal(SectionCurveRole.Axis, axis.Role);
            Assert.Equal(24.0, Math.Abs(axis.Points[1].X - axis.Points[0].X), 6);
        }

        [Fact]
        public void TheAxisBecomesACrossMarkerWhenItProjectsToAPoint()
        {
            // Mirando por Z el eje se ve de punta; una linea de longitud cero seria invalida.
            var plan = Plan(W, 24.0, SectionViewpoint.CrossSection, mode: SectionRepresentationMode.Axis);

            Assert.Equal(2, plan.CurvesOf(SectionCurveRole.Axis).Count());
            Assert.All(plan.Curves, c => Assert.Equal(2, c.Points.Count));
        }

        [Fact]
        public void AxisAndEnvelopeCanBeAddedToAWireframe()
        {
            var plan = Plan(W, 24.0, SectionViewpoint.LongitudinalX, axis: true, envelope: true);

            Assert.NotEmpty(plan.CurvesOf(SectionCurveRole.EndProfile));
            Assert.NotEmpty(plan.CurvesOf(SectionCurveRole.Axis));
            Assert.Single(plan.CurvesOf(SectionCurveRole.Envelope));
        }

        // ---- Calidad del plan --------------------------------------------------------------------------------

        [Fact]
        public void NoCurveContainsAZeroLengthStep()
        {
            foreach (var view in new[]
            {
                SectionViewpoint.CrossSection, SectionViewpoint.LongitudinalX,
                SectionViewpoint.LongitudinalY, SectionViewpoint.Isometric
            })
            {
                foreach (var id in new[] { W, Hss, Channel, UnequalAngle })
                {
                    var plan = Plan(id, 36.0, view, rotation: 15.0);

                    foreach (var curve in plan.Curves)
                    {
                        for (var i = 1; i < curve.Points.Count; i++)
                        {
                            Assert.False(
                                curve.Points[i].ApproxEquals(curve.Points[i - 1], GeometryTolerance.Length),
                                id + " " + view + " " + curve.Role);
                        }
                    }
                }
            }
        }

        [Fact]
        public void ThePlanSignatureIsDeterministic()
        {
            var first = Plan(UnequalAngle, 96.0, SectionViewpoint.Isometric, rotation: 22.5, mirrored: true);
            var second = Plan(UnequalAngle, 96.0, SectionViewpoint.Isometric, rotation: 22.5, mirrored: true);

            Assert.Equal(first.Signature(), second.Signature());
        }

        [Fact]
        public void ThePlanSignatureChangesWhenAnythingRelevantChanges()
        {
            var baseline = Plan(W, 24.0, SectionViewpoint.LongitudinalX).Signature();

            Assert.NotEqual(baseline, Plan(W, 25.0, SectionViewpoint.LongitudinalX).Signature());
            Assert.NotEqual(baseline, Plan(W, 24.0, SectionViewpoint.LongitudinalY).Signature());
            Assert.NotEqual(baseline, Plan(W, 24.0, SectionViewpoint.LongitudinalX, rotation: 5.0).Signature());
            Assert.NotEqual(
                baseline,
                Plan(W, 24.0, SectionViewpoint.LongitudinalX, detail: SectionDetailLevel.Simplified).Signature());

            // El espejo cambia la firma de una seccion ASIMETRICA, en una vista donde la X del modelo se vea.
            var channel = Plan(Channel, 24.0, SectionViewpoint.CrossSection).Signature();
            Assert.NotEqual(
                channel,
                Plan(Channel, 24.0, SectionViewpoint.CrossSection, mirrored: true).Signature());
        }

        [Fact]
        public void TheSignatureIsAFingerprintOfThePlan_NotAGeometricEquivalenceClass()
        {
            // Precision necesaria sobre que es la firma, porque es facil pedirle de mas.
            //
            // Codifica las curvas TAL COMO SE EMITEN: sus puntos Y su orden de recorrido. Eso es justo lo que
            // hace falta para afirmar "no se movio nada" entre dos versiones, y NO la convierte en una clase
            // de equivalencia geometrica: espejear una W produce la misma FIGURA recorrida al reves —el
            // espejo invierte el sentido y el contorno se revierte para conservar la orientacion— asi que la
            // firma difiere aunque el dibujo sea identico.
            var plain = Plan(W, 24.0, SectionViewpoint.CrossSection);
            var mirrored = Plan(W, 24.0, SectionViewpoint.CrossSection, mirrored: true);

            Assert.NotEqual(plain.Signature(), mirrored.Signature());

            // Lo que SI se conserva, y es lo que "el dibujo es el mismo" significa de verdad: la envolvente
            // y el conjunto de puntos.
            Assert.True(plain.Bounds.ApproxEquals(mirrored.Bounds, 1e-9));
            Assert.Equal(SortedPoints(plain), SortedPoints(mirrored));
        }

        [Fact]
        public void MirroringIsInvisibleInTheViewThatDiscardsItsAxis()
        {
            // Mirando a lo largo de X la proyeccion descarta ENTERA la coordenada X del modelo —dibuja la
            // longitud contra la Y de la seccion—, asi que un espejo alrededor de Y no puede verse ahi ni
            // siquiera en un canal asimetrico. No es un defecto: es lo que significa esa vista.
            var plain = Plan(Channel, 24.0, SectionViewpoint.LongitudinalX);
            var mirrored = Plan(Channel, 24.0, SectionViewpoint.LongitudinalX, mirrored: true);

            Assert.True(plain.Bounds.ApproxEquals(mirrored.Bounds, 1e-9));
            Assert.Equal(SortedPoints(plain), SortedPoints(mirrored));

            // Y en la vista de seccion, donde si se ve, la figura cambia de verdad.
            var front = Plan(Channel, 24.0, SectionViewpoint.CrossSection);
            var flipped = Plan(Channel, 24.0, SectionViewpoint.CrossSection, mirrored: true);

            Assert.NotEqual(SortedPoints(front), SortedPoints(flipped));
        }

        /// <summary>Los puntos del plan en un orden canonico, para comparar FIGURAS y no recorridos.</summary>
        private static string SortedPoints(StructuralSectionRepresentationPlan plan) =>
            string.Join(";", plan.Curves
                .SelectMany(c => c.Points)
                .Select(p => p.X.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture) + "," +
                             p.Y.ToString("0.######", System.Globalization.CultureInfo.InvariantCulture))
                .OrderBy(s => s, StringComparer.Ordinal));

        [Fact]
        public void ATighterToleranceNeverProducesFewerPoints()
        {
            var coarse = StructuralSectionPlanBuilder.Build(
                Geometry(Hss), Instance(Hss, 24.0),
                new SectionRepresentationOptions { ChordTolerance = 0.05 });
            var fine = StructuralSectionPlanBuilder.Build(
                Geometry(Hss), Instance(Hss, 24.0),
                new SectionRepresentationOptions { ChordTolerance = 0.0005 });

            Assert.True(fine.Curves.Sum(c => c.Points.Count) >= coarse.Curves.Sum(c => c.Points.Count));
        }

        [Fact]
        public void ANonPositiveToleranceIsRejected()
        {
            Assert.Throws<ArgumentException>(() => StructuralSectionPlanBuilder.Build(
                Geometry(W), Instance(W, 24.0),
                new SectionRepresentationOptions { ChordTolerance = 0.0 }));
        }

        [Fact]
        public void ThePlanCarriesItsFidelityAndDiagnostics()
        {
            var channel = Plan(Channel, 24.0, SectionViewpoint.CrossSection);

            Assert.Equal(SectionFidelity.TabulatedDerived, channel.Fidelity);
            Assert.Contains(channel.Diagnostics,
                d => d.Code == SectionGeometryDiagnostics.ToeRoundingNotPublished);

            var w = Plan(W, 24.0, SectionViewpoint.CrossSection);
            Assert.Equal(SectionFidelity.TabulatedComplete, w.Fidelity);
        }

        [Fact]
        public void APlanCannotBeBuiltForAMismatchedInstance()
        {
            Assert.Throws<ArgumentException>(() => StructuralSectionPlanBuilder.Build(
                Geometry(W), Instance(Channel, 24.0)));
        }

        [Fact]
        public void EveryFamilyProjectsInEveryStandardView()
        {
            foreach (var id in new[] { W, Hss, Channel, UnequalAngle, "AISC-L-L4X4X1_4", "AISC-C-C15X50" })
            {
                foreach (var kind in new[]
                {
                    SectionViewKind.CrossSection, SectionViewKind.LongitudinalX,
                    SectionViewKind.LongitudinalY, SectionViewKind.Isometric
                })
                {
                    var plan = Plan(id, 72.0, SectionViewpoint.Standard(kind));

                    Assert.True(plan.Bounds.HasArea, id + " " + kind);
                    Assert.NotEmpty(plan.Curves);
                }
            }
        }
    }
}
