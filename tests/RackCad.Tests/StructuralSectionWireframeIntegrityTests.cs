using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using Xunit;
using Xunit.Abstractions;

namespace RackCad.Tests
{
    /// <summary>
    /// Integridad del alambre proyectado: que el dibujo tenga TODA la tinta que debe y NINGUNA repetida.
    ///
    /// Nace de dos defectos que la validacion del dueno encontro y que ninguna suite anterior podia ver,
    /// porque todas miraban limites, areas y firmas —magnitudes que un alambre incompleto o repetido no
    /// altera—:
    ///
    /// 1. las generatrices longitudinales salian SOLO del contorno exterior, asi que un HSS visto de lado
    ///    no mostraba su pared: parecia macizo;
    /// 2. un perfil de extremo proyectado exactamente a lo largo de X o de Y colapsa a una recta, y se
    ///    seguia emitiendo como polilinea CERRADA, que en AutoCAD dibuja cada arista dos veces.
    ///
    /// Las comprobaciones son de COMPORTAMIENTO —donde hay tinta y cuanta—, no de nombres de rol, para que
    /// sigan valiendo aunque el modelo de roles cambie.
    /// </summary>
    public class StructuralSectionWireframeIntegrityTests
    {
        private readonly ITestOutputHelper _output;

        public StructuralSectionWireframeIntegrityTests(ITestOutputHelper output) => _output = output;

        private const double Length = 120.0;

        private static StructuralSectionCatalog Catalog() =>
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private static StructuralSectionGeometryFactory Factory() =>
            new StructuralSectionGeometryFactory(Catalog());

        /// <summary>Las cuatro sentinelas de familia, una por cada forma que el catalogo distingue.</summary>
        public static IEnumerable<object[]> Families => new[]
        {
            new object[] { "AISC-W-W12X26" },
            new object[] { "AISC-HSS-RECT-HSS4X4X_250" },
            new object[] { "AISC-C-C10X15_3" },
            new object[] { "AISC-L-L8X6X1" }
        };

        public static IEnumerable<object[]> FamiliesTimesLongitudinalViews =>
            Families.SelectMany(f => new[]
            {
                new[] { f[0], SectionViewKind.LongitudinalX },
                new[] { f[0], SectionViewKind.LongitudinalY }
            });

        private static StructuralSectionRepresentationPlan Plan(
            string sectionId,
            SectionViewKind view,
            double rotationDegrees = 0.0,
            bool mirrored = false,
            SectionDetailLevel detail = SectionDetailLevel.Tabulated)
        {
            var factory = Factory();
            var section = factory.Catalog.GetById(StructuralSectionId.Parse(sectionId));
            var geometry = factory.Get(section, detail);
            var instance = PrismaticSectionInstance.Create(
                section.SectionId, Length, null, rotationDegrees, mirrored);

            return StructuralSectionPlanBuilder.Build(geometry, instance, new SectionRepresentationOptions
            {
                Viewpoint = SectionViewpoint.Standard(view),
                Detail = detail
            });
        }

        // ==================================================================================================
        // F1 — el hueco del HSS tambien recorre la pieza
        // ==================================================================================================

        /// <summary>
        /// Un tubo visto de lado tiene CUATRO lineas longitudinales, no dos: las dos caras exteriores y las
        /// dos interiores. Sin las interiores el dibujo dice que la pieza es maciza.
        /// </summary>
        [Theory]
        [InlineData(SectionViewKind.LongitudinalX)]
        [InlineData(SectionViewKind.LongitudinalY)]
        public void AnHssSeenSideOnShowsItsBore(SectionViewKind view)
        {
            var plan = Plan("AISC-HSS-RECT-HSS4X4X_250", view);

            // Ht = B = 4 in, tnom = 0.25 in → paredes exteriores en ±2.00 e interiores en ±1.75.
            var offsets = FullLengthLineOffsets(plan, view);

            _output.WriteLine(view + ": lineas longitudinales en " + Describe(offsets));

            // Seis, no ocho: los arcos interior y exterior son CONCENTRICOS, asi que sus puntos de tangencia
            // caen a la misma altura (2 - 0.35 = 1.75 - 0.10 = 1.65) y ahi las dos lineas son la misma tinta.
            Assert.Contains(offsets, o => Math.Abs(Math.Abs(o) - 1.75) < 1e-6);
            Assert.Equal(6, offsets.Count);
        }

        /// <summary>
        /// En isometrica las generatrices del hueco tampoco pueden faltar: se ve por la boca.
        ///
        /// La cuenta NO es "un vertice, una linea". En isometrica la pantalla horizontal vale (y - x)/raiz(2),
        /// asi que los ocho vertices del contorno exterior del HSS4X4X1/4 caen sobre solo CUATRO rectas —los
        /// pares (-1.65,-2) y (2,1.65) comparten recta— y sus tramos se solapan. Fusionarlos es exactamente
        /// lo correcto: dibujan la misma tinta. Lo que el hueco tiene que aportar son rectas NUEVAS.
        /// </summary>
        [Fact]
        public void AnHssInIsometricAlsoRunsItsBoreAlongThePiece()
        {
            var plan = Plan("AISC-HSS-RECT-HSS4X4X_250", SectionViewKind.Isometric);

            var all = DistinctLongitudinalLines(plan, SectionViewKind.Isometric, role: null);
            var outerOnly = DistinctLongitudinalLines(
                plan, SectionViewKind.Isometric, SectionCurveRole.Generatrix);

            _output.WriteLine("rectas longitudinales: " + all + " en total, " + outerOnly + " del exterior");

            Assert.True(all > outerOnly,
                "En isometrica el hueco debe aportar rectas propias; hay " + all +
                " en total y " + outerOnly + " del contorno exterior.");

            Assert.Contains(plan.Curves, c => c.Role == SectionCurveRole.InteriorGeneratrix);
        }

        /// <summary>
        /// El espesor nominal tiene que MEDIRSE en la vista longitudinal: dos lineas paralelas separadas
        /// exactamente tnom. Es la comprobacion que el dueno hace con DIST, y la que fallaba.
        /// </summary>
        [Theory]
        [InlineData(0.0, false)]
        [InlineData(90.0, false)]
        [InlineData(0.0, true)]
        public void TheNominalWallIsMeasurableInALongitudinalView(double rotation, bool mirrored)
        {
            const double Tnom = 0.25;

            var plan = Plan("AISC-HSS-RECT-HSS4X4X_250", SectionViewKind.LongitudinalX, rotation, mirrored);
            var offsets = FullLengthLineOffsets(plan, SectionViewKind.LongitudinalX);

            var pairs = offsets
                .SelectMany(a => offsets.Select(b => Math.Abs(a - b)))
                .Where(gap => Math.Abs(gap - Tnom) < 1e-6)
                .Count();

            _output.WriteLine("rot=" + rotation + " espejo=" + mirrored + " · offsets " + Describe(offsets));

            Assert.True(pairs > 0,
                "Ninguna pareja de lineas longitudinales esta separada tnom = " + Tnom +
                "; el espesor de pared no se puede medir en el dibujo.");
        }

        /// <summary>
        /// La otra cara de la moneda: W, C y L NO tienen hueco, asi que no pueden ganar lineas interiores
        /// inventadas. El numero de generatrices es exactamente el de vertices de silueta.
        /// </summary>
        [Theory]
        [InlineData("AISC-W-W12X26")]
        [InlineData("AISC-C-C10X15_3")]
        [InlineData("AISC-L-L8X6X1")]
        public void ASolidSectionGainsNoInteriorLines(string sectionId)
        {
            foreach (var view in new[]
                     {
                         SectionViewKind.LongitudinalX, SectionViewKind.LongitudinalY, SectionViewKind.Isometric
                     })
            {
                var plan = Plan(sectionId, view);

                Assert.DoesNotContain(plan.Curves, c => c.Role == SectionCurveRole.InteriorGeneratrix);
                Assert.DoesNotContain(plan.Curves, c => c.Role == SectionCurveRole.EndProfileHole);

                // Y no solo por el rol: toda recta longitudinal viene del contorno exterior.
                Assert.Equal(
                    DistinctLongitudinalLines(plan, view, role: null),
                    DistinctLongitudinalLines(plan, view, SectionCurveRole.Generatrix));
            }
        }

        // ==================================================================================================
        // F2 — proyecciones que colapsan
        // ==================================================================================================

        /// <summary>
        /// Mirando exactamente a lo largo de X o de Y, la seccion se ve de canto: su contorno cerrado colapsa
        /// a una recta. Emitirlo como polilinea CERRADA hace que AutoCAD recorra cada arista de ida y de
        /// vuelta — tinta duplicada sobre una figura de area cero.
        /// </summary>
        [Theory]
        [MemberData(nameof(FamiliesTimesLongitudinalViews))]
        public void NoClosedCurveCollapsesToALine(string sectionId, SectionViewKind view)
        {
            var plan = Plan(sectionId, view);

            var degenerate = plan.Curves
                .Where(c => c.IsClosed && Dimensionality(c.Points) < 2)
                .ToArray();

            Assert.Empty(degenerate);
        }

        /// <summary>Ninguna arista se dibuja dos veces, ni repetida ni recorrida al reves.</summary>
        [Theory]
        [MemberData(nameof(FamiliesTimesLongitudinalViews))]
        public void NoEdgeIsDrawnTwice(string sectionId, SectionViewKind view)
        {
            var plan = Plan(sectionId, view);
            var seen = new Dictionary<string, int>(StringComparer.Ordinal);

            foreach (var edge in plan.Curves.SelectMany(Edges))
            {
                var key = EdgeKey(edge.Item1, edge.Item2);
                seen[key] = seen.TryGetValue(key, out var count) ? count + 1 : 1;
            }

            var repeated = seen.Where(pair => pair.Value > 1).Select(pair => pair.Key).ToArray();

            Assert.Empty(repeated);
        }

        /// <summary>Ni un solo paso de longitud cero: una polilinea con vertices repetidos es basura.</summary>
        [Theory]
        [MemberData(nameof(FamiliesTimesLongitudinalViews))]
        public void NoStepHasZeroLength(string sectionId, SectionViewKind view)
        {
            var plan = Plan(sectionId, view);

            Assert.DoesNotContain(
                plan.Curves.SelectMany(Edges),
                edge => edge.Item1.ApproxEquals(edge.Item2, GeometryTolerance.Continuity));
        }

        /// <summary>
        /// El perfil de extremo tiene que seguir siendo LEGIBLE tras colapsar: una sola recta que abarca todo
        /// el canto de la pieza, no un puñado de trozos sueltos.
        /// </summary>
        [Theory]
        [MemberData(nameof(FamiliesTimesLongitudinalViews))]
        public void TheEndProfileStaysReadable(string sectionId, SectionViewKind view)
        {
            var plan = Plan(sectionId, view);

            // Los dos extremos estan en los dos valores extremos de la coordenada longitudinal.
            foreach (var end in new[] { plan.Bounds.MinX, plan.Bounds.MaxX })
            {
                var atEnd = plan.Curves
                    .Where(c => c.Points.All(p => Math.Abs(p.X - end) < 1e-6))
                    .ToArray();

                var curve = Assert.Single(atEnd);
                Assert.False(curve.IsClosed);

                var span = curve.Points.Max(p => p.Y) - curve.Points.Min(p => p.Y);
                var height = plan.Bounds.Height;

                Assert.True(Math.Abs(span - height) < 1e-6,
                    "El perfil de extremo abarca " + span.ToString("0.####", CultureInfo.InvariantCulture) +
                    " y el canto de la pieza es " + height.ToString("0.####", CultureInfo.InvariantCulture) + ".");
            }
        }

        /// <summary>
        /// La envolvente tampoco puede ser un rectangulo de area cero. Se alcanza desde la UI: modo Eje mas
        /// "mostrar envolvente" en una vista longitudinal deja un eje recto cuyo cuadro delimitador es plano.
        /// </summary>
        [Fact]
        public void TheEnvelopeOfAFlatDrawingIsNotAClosedRectangle()
        {
            var factory = Factory();
            var section = factory.Catalog.GetById(StructuralSectionId.Parse("AISC-W-W12X26"));
            var instance = PrismaticSectionInstance.Create(section.SectionId, Length);

            var plan = StructuralSectionPlanBuilder.Build(
                factory.Get(section, SectionDetailLevel.Tabulated), instance,
                new SectionRepresentationOptions
                {
                    Viewpoint = SectionViewpoint.LongitudinalX,
                    Mode = SectionRepresentationMode.Axis,
                    IncludeEnvelope = true
                });

            Assert.DoesNotContain(plan.Curves, c => c.IsClosed && Dimensionality(c.Points) < 2);
        }

        /// <summary>
        /// La invariante que hace imposible el defecto, en vez de solo comprobarlo.
        ///
        /// El materializador de AutoCAD copia <c>curve.IsClosed</c> tal cual, asi que si el tipo del plan no
        /// deja construir una curva cerrada unidimensional, no hay camino por el que una polilinea cerrada de
        /// area cero llegue al dibujo. La guarda de fuente del plugin fija la otra mitad de la costura.
        /// </summary>
        [Fact]
        public void AOneDimensionalCurveCannotBeBornClosed()
        {
            var online = new[] { new Point2D(0.0, -2.0), new Point2D(0.0, 0.0), new Point2D(0.0, 2.0) };

            Assert.Throws<ArgumentException>(() =>
                new SectionPlanCurve(SectionCurveRole.EndProfile, online, isClosed: true));

            // Abierta si vale: la recta es exactamente lo que se ve de canto.
            Assert.False(new SectionPlanCurve(SectionCurveRole.EndProfile, online, isClosed: false).IsClosed);
        }

        /// <summary>
        /// Y el plan que recibe AutoCAD lo cumple en las 983, no solo en las sentinelas.
        ///
        /// Se recorre el catalogo entero en las cuatro vistas estandar porque el colapso depende de la forma:
        /// una familia nueva, o un angulo cuyas alas queden alineadas con la camara, lo reintroduciria sin
        /// que ninguna sentinela se enterase.
        /// </summary>
        [Theory]
        [InlineData(SectionViewKind.CrossSection)]
        [InlineData(SectionViewKind.LongitudinalX)]
        [InlineData(SectionViewKind.LongitudinalY)]
        [InlineData(SectionViewKind.Isometric)]
        public void TheMaterializerOnlyEverReceivesValidCurves(SectionViewKind view)
        {
            var factory = Factory();
            var viewpoint = SectionViewpoint.Standard(view);
            var failures = new List<string>();

            foreach (var section in factory.Catalog.All)
            {
                var plan = StructuralSectionPlanBuilder.Build(
                    factory.Get(section, SectionDetailLevel.Tabulated),
                    PrismaticSectionInstance.Create(section.SectionId, Length),
                    new SectionRepresentationOptions { Viewpoint = viewpoint });

                foreach (var curve in plan.Curves)
                {
                    if (curve.IsClosed && Dimensionality(curve.Points) < 2)
                    {
                        failures.Add(section.SectionId + ": " + curve.Role + " cerrada y plana");
                    }

                    if (curve.Points.Count < 2)
                    {
                        failures.Add(section.SectionId + ": " + curve.Role + " con menos de dos puntos");
                    }
                }
            }

            Assert.True(failures.Count == 0,
                failures.Count + " curvas invalidas en " + view + ":\n" + string.Join("\n", failures.Take(20)));
        }

        // ==================================================================================================
        // Ayudantes
        // ==================================================================================================

        /// <summary>
        /// Dimension del conjunto proyectado: 0 si todo cae en un punto, 1 si cae en una recta, 2 si no.
        /// La tolerancia es relativa al tamaño para que no dependa de si la pieza mide 4 in o 400.
        /// </summary>
        internal static int Dimensionality(IReadOnlyList<Point2D> points)
        {
            if (points.Count == 0)
            {
                return 0;
            }

            var bounds = Bounds2D.FromPoints(points);
            var extent = Math.Max(bounds.Width, bounds.Height);
            var tolerance = Math.Max(GeometryTolerance.Continuity, extent * 1e-9);

            if (extent <= tolerance)
            {
                return 0;
            }

            var origin = points[0];
            var direction = Vector2D.Zero;

            foreach (var point in points)
            {
                var candidate = Vector2D.Between(origin, point);

                if (candidate.Length > tolerance)
                {
                    direction = candidate.Normalized();
                    break;
                }
            }

            foreach (var point in points)
            {
                if (Math.Abs(direction.Cross(Vector2D.Between(origin, point))) > tolerance)
                {
                    return 2;
                }
            }

            return 1;
        }

        /// <summary>Las aristas de una curva, cerrando el ciclo cuando la curva es cerrada.</summary>
        private static IEnumerable<Tuple<Point2D, Point2D>> Edges(SectionPlanCurve curve)
        {
            for (var i = 1; i < curve.Points.Count; i++)
            {
                yield return Tuple.Create(curve.Points[i - 1], curve.Points[i]);
            }

            if (curve.IsClosed && curve.Points.Count > 2)
            {
                yield return Tuple.Create(curve.Points[curve.Points.Count - 1], curve.Points[0]);
            }
        }

        /// <summary>Clave de arista independiente del sentido: A→B y B→A dan la misma.</summary>
        private static string EdgeKey(Point2D a, Point2D b)
        {
            var first = Key(a);
            var second = Key(b);

            return string.CompareOrdinal(first, second) <= 0 ? first + "|" + second : second + "|" + first;
        }

        private static string Key(Point2D p) =>
            p.X.ToString("0.000000", CultureInfo.InvariantCulture) + "," +
            p.Y.ToString("0.000000", CultureInfo.InvariantCulture);

        /// <summary>
        /// Las curvas que recorren la pieza a lo largo: rectas de dos puntos paralelas al eje longitudinal
        /// PROYECTADO.
        ///
        /// No se compara contra la longitud real porque en isometrica —y en cualquier vista oblicua— el eje
        /// se escorza: 120 in se dibujan como 97.98. Lo que no cambia es la DIRECCION, asi que esa es la
        /// prueba correcta y vale en las cuatro vistas.
        /// </summary>
        private static IReadOnlyList<SectionPlanCurve> LongitudinalCurves(
            StructuralSectionRepresentationPlan plan, SectionViewKind view)
        {
            var viewpoint = SectionViewpoint.Standard(view);
            var origin = viewpoint.Project(new Point3D(0.0, 0.0, 0.0));
            var axis = Vector2D.Between(origin, viewpoint.Project(new Point3D(0.0, 0.0, 1.0)));

            if (axis.Length <= GeometryTolerance.Continuity)
            {
                return new SectionPlanCurve[0]; // vista de seccion: el eje se ve de punta
            }

            var direction = axis.Normalized();

            return plan.Curves
                .Where(c => !c.IsClosed && c.Points.Count == 2)
                .Where(c =>
                {
                    var span = Vector2D.Between(c.Points[0], c.Points[1]);
                    return span.Length > GeometryTolerance.Continuity &&
                           Math.Abs(direction.Cross(span)) < 1e-6;
                })
                .ToArray();
        }

        /// <summary>
        /// La coordenada transversal de cada linea que recorre la pieza entera, sin repetir.
        ///
        /// En una vista longitudinal la longitud corre horizontal, asi que la posicion de cada cara es su Y.
        /// </summary>
        private static IReadOnlyList<double> FullLengthLineOffsets(
            StructuralSectionRepresentationPlan plan, SectionViewKind view)
        {
            var offsets = new List<double>();

            foreach (var curve in LongitudinalCurves(plan, view))
            {
                var y = curve.Points[0].Y;

                if (!offsets.Any(existing => Math.Abs(existing - y) < 1e-6))
                {
                    offsets.Add(y);
                }
            }

            offsets.Sort();
            return offsets;
        }

        /// <summary>
        /// Cuantas RECTAS distintas recorren la pieza a lo largo, opcionalmente filtrando por rol.
        ///
        /// Se cuentan rectas, no curvas: dos generatrices colineales dibujan una sola linea, y despues de la
        /// canonicalizacion son de hecho una sola curva.
        /// </summary>
        private static int DistinctLongitudinalLines(
            StructuralSectionRepresentationPlan plan, SectionViewKind view, SectionCurveRole? role)
        {
            var viewpoint = SectionViewpoint.Standard(view);
            var origin = viewpoint.Project(new Point3D(0.0, 0.0, 0.0));
            var direction = Vector2D.Between(origin, viewpoint.Project(new Point3D(0.0, 0.0, 1.0))).Normalized();
            var normal = direction.Perpendicular();

            var offsets = new List<double>();

            foreach (var curve in LongitudinalCurves(plan, view))
            {
                if (role.HasValue && curve.Role != role.Value)
                {
                    continue;
                }

                var offset = (curve.Points[0].X * normal.X) + (curve.Points[0].Y * normal.Y);

                if (!offsets.Any(existing => Math.Abs(existing - offset) < 1e-6))
                {
                    offsets.Add(offset);
                }
            }

            return offsets.Count;
        }

        private static string Describe(IEnumerable<double> values) =>
            string.Join(", ", values.Select(v => v.ToString("0.####", CultureInfo.InvariantCulture)));
    }
}
