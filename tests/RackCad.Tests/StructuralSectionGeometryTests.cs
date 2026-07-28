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
    /// La geometria de las 983 secciones REALMENTE distribuidas.
    ///
    /// Es la unica suite de I-36B que depende de ids AISC reales, igual que en I-36A: todo lo demas usa
    /// fixtures sinteticos para que una revision futura de la fuente no tumbe la base de pruebas entera.
    /// </summary>
    public class StructuralSectionGeometryTests
    {
        private readonly ITestOutputHelper _output;

        public StructuralSectionGeometryTests(ITestOutputHelper output) => _output = output;

        private static StructuralSectionCatalog Catalog() =>
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private static StructuralSectionGeometryFactory Factory() =>
            new StructuralSectionGeometryFactory(Catalog());

        // ---- Las 983, en los dos niveles de detalle -----------------------------------------------------

        [Theory]
        [InlineData(SectionDetailLevel.Simplified)]
        [InlineData(SectionDetailLevel.Tabulated)]
        public void EverySection_ProducesValidGeometry(SectionDetailLevel detail)
        {
            var factory = Factory();
            var failures = new List<string>();

            foreach (var section in factory.Catalog.All)
            {
                StructuralSectionGeometry geometry;

                try
                {
                    geometry = factory.Get(section, detail);
                }
                catch (Exception ex)
                {
                    failures.Add(section.SectionId + ": lanzo " + ex.GetType().Name + " — " + ex.Message);
                    continue;
                }

                if (geometry.Family != section.Family)
                {
                    failures.Add(section.SectionId + ": familia " + geometry.Family + " != " + section.Family);
                }

                if (geometry.Area <= 0.0 || !GeometryTolerance.IsFinite(geometry.Area))
                {
                    failures.Add(section.SectionId + ": area " + geometry.Area);
                }

                if (!geometry.Bounds.HasArea)
                {
                    failures.Add(section.SectionId + ": limites sin extension");
                }

                if (geometry.OuterContour.Orientation != ContourOrientation.CounterClockwise)
                {
                    failures.Add(section.SectionId + ": el contorno exterior no es antihorario");
                }

                foreach (var hole in geometry.Holes)
                {
                    if (hole.Orientation != ContourOrientation.Clockwise)
                    {
                        failures.Add(section.SectionId + ": un hueco no es horario");
                    }
                }

                foreach (var point in AllPoints(geometry))
                {
                    if (!GeometryTolerance.IsFinite(point.X) || !GeometryTolerance.IsFinite(point.Y))
                    {
                        failures.Add(section.SectionId + ": punto no finito " + point);
                        break;
                    }
                }

                // Ninguna degradacion silenciosa: si el resultado dice que degrado, tiene que decir por que.
                if (geometry.Fidelity == SectionFidelity.DegradedToSimplified &&
                    !geometry.Diagnostics.Any(d => d.Severity == SectionDiagnosticSeverity.Degraded))
                {
                    failures.Add(section.SectionId + ": degradada sin diagnostico");
                }
            }

            Assert.True(failures.Count == 0,
                failures.Count + " secciones fallaron en " + detail + ":\n" + string.Join("\n", failures.Take(20)));
        }

        [Fact]
        public void TheWholeCatalogIsCovered()
        {
            Assert.Equal(983, Catalog().Count);
        }

        [Fact]
        public void ContoursAreContinuousAndClosed()
        {
            // ClosedContour2D ya lo exige al construir; esto lo comprueba sobre las 983 reales, que es donde
            // una regla de familia mal encadenada aparecería.
            var factory = Factory();

            foreach (var section in factory.Catalog.All)
            {
                foreach (var contour in factory.Get(section, SectionDetailLevel.Tabulated).AllContours())
                {
                    for (var i = 0; i < contour.Segments.Count; i++)
                    {
                        var next = contour.Segments[(i + 1) % contour.Segments.Count];
                        Assert.True(
                            contour.Segments[i].End.ApproxEquals(next.Start, GeometryTolerance.Continuity),
                            section.SectionId + " segmento " + i);
                    }
                }
            }
        }

        // ---- Centrado ------------------------------------------------------------------------------------

        [Fact]
        public void SymmetricFamilies_AreCentredExactly()
        {
            // W y HSS son doblemente simetricas: su centroide GEOMETRICO tiene que caer en el origen, no
            // cerca del origen.
            var factory = Factory();
            var symmetric = factory.Catalog.All.Where(s =>
                s.Family == StructuralSectionFamily.W || s.Family == StructuralSectionFamily.HssRectangular);

            foreach (var section in symmetric)
            {
                foreach (var detail in new[] { SectionDetailLevel.Simplified, SectionDetailLevel.Tabulated })
                {
                    var geometry = factory.Get(section, detail);
                    Assert.True(geometry.CentroidOffset < 1e-9,
                        section.SectionId + " " + detail + " centroide en " + geometry.Centroid);
                }
            }
        }

        [Fact]
        public void AsymmetricFamilies_AreCentredWithTheTabulatedValue_AndTheResidualIsMeasured()
        {
            // C y L se centran con el x/y que TABULA la fuente. El residuo entre ese punto y el centroide
            // geometrico del contorno es el detalle que la fuente no publica —la conicidad del patin y el
            // redondeo de la punta—, y se mide en vez de esconderse.
            var factory = Factory();
            var worst = new Dictionary<StructuralSectionFamily, (string Id, double Offset, double Relative)>();

            foreach (var section in factory.Catalog.All.Where(s =>
                s.Family == StructuralSectionFamily.Channel || s.Family == StructuralSectionFamily.Angle))
            {
                var geometry = factory.Get(section, SectionDetailLevel.Tabulated);
                var scale = Math.Max(geometry.Bounds.Width, geometry.Bounds.Height);
                var relative = geometry.CentroidOffset / scale;

                if (!worst.TryGetValue(section.Family, out var current) || relative > current.Relative)
                {
                    worst[section.Family] = (section.SectionId.Value, geometry.CentroidOffset, relative);
                }
            }

            foreach (var entry in worst)
            {
                _output.WriteLine(
                    entry.Key + ": residuo maximo " +
                    entry.Value.Offset.ToString("0.####", CultureInfo.InvariantCulture) + " in (" +
                    (entry.Value.Relative * 100).ToString("0.###", CultureInfo.InvariantCulture) +
                    " % del tamano) en " + entry.Value.Id);

                // El residuo tiene que ser PEQUENO frente al tamano de la seccion; si creciera, querria decir
                // que el traslado se hizo con el eje equivocado.
                Assert.True(entry.Value.Relative < 0.05,
                    entry.Key + " residuo relativo " + entry.Value.Relative + " en " + entry.Value.Id);
            }
        }

        // ---- Area frente a la tabulada -------------------------------------------------------------------

        [Fact]
        public void TabulatedArea_IsMeasuredAndReportedPerFamily()
        {
            // No se manipula la geometria para forzar el area: se mide y se documenta.
            var factory = Factory();
            var byFamily = new Dictionary<StructuralSectionFamily, List<double>>();

            foreach (var section in factory.Catalog.All)
            {
                var published = section.Properties.Area;

                if (!published.HasValue)
                {
                    continue;
                }

                var geometry = factory.Get(section, SectionDetailLevel.Tabulated);
                var error = (geometry.Area - published.Value) / published.Value;

                if (!byFamily.TryGetValue(section.Family, out var list))
                {
                    list = new List<double>();
                    byFamily[section.Family] = list;
                }

                list.Add(error);
            }

            foreach (var family in StructuralSectionFamilies.All)
            {
                var errors = byFamily[family];
                var absolute = errors.Select(Math.Abs).ToArray();

                _output.WriteLine(
                    family + ": n=" + errors.Count +
                    "  max=" + (absolute.Max() * 100).ToString("0.###", CultureInfo.InvariantCulture) +
                    "%  media=" + (absolute.Average() * 100).ToString("0.###", CultureInfo.InvariantCulture) +
                    "%  sobre5%=" + absolute.Count(e => e > 0.05));
            }

            // W: la derivacion r = kdes - tf es exacta salvo el redondeo de la fuente.
            Assert.True(byFamily[StructuralSectionFamily.W].Select(Math.Abs).Max() < 0.01);

            // L: por debajo del 5 % pese a no modelar el redondeo de punta.
            Assert.True(byFamily[StructuralSectionFamily.Angle].Select(Math.Abs).Max() < 0.05);

            // C: por encima del 5 % en unas pocas filas, con causa acreditada (punta no publicada + patin
            // conico). Se fija el limite REAL medido para que un empeoramiento se note.
            var channel = byFamily[StructuralSectionFamily.Channel].Select(Math.Abs).ToArray();
            Assert.True(channel.Max() < 0.06, "C max " + channel.Max());
            Assert.True(channel.Count(e => e > 0.05) <= 3, "C sobre 5%: " + channel.Count(e => e > 0.05));

            // HSS: el contorno usa tnom por decision del dueno y la A publicada se calcula con tdes, asi que
            // la diferencia es de DEFINICION. Se comprueba que el sesgo va en la direccion esperada y con la
            // magnitud que implica la razon entre ambos espesores.
            var hss = byFamily[StructuralSectionFamily.HssRectangular];
            Assert.True(hss.All(e => e > 0.0), "todas las HSS deben quedar por ENCIMA de la A publicada");
            Assert.True(hss.Average() > 0.05 && hss.Average() < 0.12, "sesgo medio HSS " + hss.Average());
        }

        [Fact]
        public void Hss_MatchesTheTabulatedAreaWhenTheSameContourUsesTheDesignThickness()
        {
            // La comprobacion de area VALIDA para HSS: rehacer el mismo contorno con tdes y ver que entonces
            // si cuadra. Demuestra que el sesgo con tnom es la definicion y no un defecto del contorno.
            var factory = Factory();
            var errors = new List<double>();

            foreach (var section in factory.Catalog.ByFamily(StructuralSectionFamily.HssRectangular))
            {
                var dimensions = (HssRectangularSectionDimensions)section.Dimensions;
                var design = dimensions.DesignThickness;
                var published = section.Properties.Area;

                if (!design.HasValue || !published.HasValue)
                {
                    continue;
                }

                var withDesign = new StructuralSectionDefinition
                {
                    Identity = section.Identity,
                    WeightPerLength = section.WeightPerLength,
                    NativeUnitSystem = section.NativeUnitSystem,
                    Properties = section.Properties,
                    Dimensions = new HssRectangularSectionDimensions
                    {
                        OverallDepth = dimensions.OverallDepth,
                        OverallWidth = dimensions.OverallWidth,
                        FlatDepth = dimensions.FlatDepth,
                        FlatWidth = dimensions.FlatWidth,
                        NominalThickness = design,
                        DesignThickness = design
                    }
                };

                var geometry = StructuralSectionGeometryFactory.Build(withDesign, SectionDetailLevel.Tabulated);
                errors.Add(Math.Abs((geometry.Area - published.Value) / published.Value));
            }

            _output.WriteLine(
                "HSS con tdes: n=" + errors.Count +
                "  max=" + (errors.Max() * 100).ToString("0.###", CultureInfo.InvariantCulture) +
                "%  media=" + (errors.Average() * 100).ToString("0.###", CultureInfo.InvariantCulture) + "%");

            Assert.True(errors.Max() < 0.05, "HSS con tdes max " + errors.Max());
            Assert.DoesNotContain(errors, e => e > 0.05);
        }

        // ---- Fidelidad -----------------------------------------------------------------------------------

        [Fact]
        public void FidelityIsDeclaredPerFamily_AndCountsAreReported()
        {
            var factory = Factory();
            var counts = new Dictionary<SectionFidelity, int>();

            foreach (var section in factory.Catalog.All)
            {
                var fidelity = factory.Get(section, SectionDetailLevel.Tabulated).Fidelity;
                counts.TryGetValue(fidelity, out var n);
                counts[fidelity] = n + 1;
            }

            foreach (var entry in counts.OrderBy(e => e.Key))
            {
                _output.WriteLine(entry.Key + ": " + entry.Value);
            }

            // W alcanza fidelidad completa; C, L y HSS declaran derivada; nada degrada sobre AISC v16.0.
            Assert.Equal(289, counts[SectionFidelity.TabulatedComplete]);
            Assert.Equal(694, counts[SectionFidelity.TabulatedDerived]);
            Assert.False(counts.ContainsKey(SectionFidelity.DegradedToSimplified));
        }

        [Fact]
        public void SimplifiedAlwaysReportsSimplifiedFidelity()
        {
            var factory = Factory();

            Assert.All(factory.Catalog.All, section =>
                Assert.Equal(
                    SectionFidelity.Simplified,
                    factory.Get(section, SectionDetailLevel.Simplified).Fidelity));
        }

        // ---- Cache ---------------------------------------------------------------------------------------

        [Fact]
        public void TheCacheIsLazyAndReturnsTheSameInstance()
        {
            var factory = Factory();

            Assert.Equal(0, factory.CachedCount);

            var first = factory.Get(StructuralSectionId.Parse("AISC-W-W12X26"), SectionDetailLevel.Tabulated);
            Assert.Equal(1, factory.CachedCount);

            var again = factory.Get(StructuralSectionId.Parse("AISC-W-W12X26"), SectionDetailLevel.Tabulated);
            Assert.Same(first, again);
            Assert.Equal(1, factory.CachedCount);

            // El nivel de detalle forma parte de la clave.
            factory.Get(StructuralSectionId.Parse("AISC-W-W12X26"), SectionDetailLevel.Simplified);
            Assert.Equal(2, factory.CachedCount);
        }

        [Fact]
        public void AMissingSectionIsAClearError()
        {
            Assert.Throws<StructuralSectionGeometryNotFoundException>(
                () => Factory().Get(StructuralSectionId.Parse("AISC-W-NO-EXISTE"), SectionDetailLevel.Simplified));
        }

        [Fact]
        public void ADisabledSectionStillProducesGeometry()
        {
            // Un diseno guardado que referencia una seccion retirada tiene que seguir dibujandose.
            var section = Catalog().All.First();
            var disabled = new StructuralSectionDefinition
            {
                Identity = section.Identity,
                WeightPerLength = section.WeightPerLength,
                NativeUnitSystem = section.NativeUnitSystem,
                Dimensions = section.Dimensions,
                Properties = section.Properties,
                IsEnabled = false
            };

            Assert.NotNull(StructuralSectionGeometryFactory.Build(disabled, SectionDetailLevel.Tabulated));
        }

        [Theory]
        [InlineData(SectionDetailLevel.Simplified)]
        [InlineData(SectionDetailLevel.Tabulated)]
        public void NoDiagnosticIsSilent(SectionDetailLevel detail)
        {
            // Un diagnostico existe para que alguien lo LEA. Uno sin codigo no se puede filtrar, uno sin
            // mensaje no se puede entender, y uno con un codigo que no esta declarado es una cadena suelta que
            // nadie podra buscar dentro de seis meses.
            var declared = new HashSet<string>(
                typeof(SectionGeometryDiagnostics)
                    .GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                    .Select(field => (string)field.GetRawConstantValue()),
                StringComparer.Ordinal);

            var factory = Factory();
            var failures = new List<string>();
            var seen = new HashSet<string>(StringComparer.Ordinal);

            foreach (var section in factory.Catalog.All)
            {
                foreach (var diagnostic in factory.Get(section, detail).Diagnostics)
                {
                    if (string.IsNullOrWhiteSpace(diagnostic.Code))
                    {
                        failures.Add(section.SectionId + ": diagnostico sin codigo");
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(diagnostic.Message))
                    {
                        failures.Add(section.SectionId + ": " + diagnostic.Code + " sin mensaje");
                    }

                    if (!declared.Contains(diagnostic.Code))
                    {
                        failures.Add(section.SectionId + ": codigo no declarado " + diagnostic.Code);
                    }

                    seen.Add(diagnostic.Code);
                }
            }

            Assert.True(failures.Count == 0, string.Join("\n", failures.Take(20)));

            _output.WriteLine("Codigos emitidos en " + detail + ": " +
                              (seen.Count == 0 ? "(ninguno)" : string.Join(", ", seen.OrderBy(c => c, StringComparer.Ordinal))));
        }

        internal static IEnumerable<Point2D> AllPoints(StructuralSectionGeometry geometry)
        {
            foreach (var contour in geometry.AllContours())
            {
                foreach (var segment in contour.Segments)
                {
                    yield return segment.Start;
                    yield return segment.End;
                }
            }
        }
    }
}
