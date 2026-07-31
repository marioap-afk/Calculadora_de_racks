using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Cantilever;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-37D ronda 3, punto 6 — el separador va al ALMA.
    ///
    /// El dueño reportó tres cosas: no está centrado en planta, su longitud es incorrecta, y «se está tomando
    /// hacia el patín cuando debe de ir al alma». Las tres eran la misma: el arriostramiento se soldaba a la
    /// cara exterior del patín, así que quedaba por fuera de la columna y su claro se medía de patín a patín.
    ///
    /// Atándolo al alma, el separador pasa ENTRE los dos patines y topa contra ella: queda en el plano medio
    /// de la columna —centrado— y su claro pasa a medirse de alma a alma.
    /// </summary>
    public class CantileverSeparatorWebTests
    {
        private static readonly StructuralSectionCatalog Catalog =
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private static readonly StructuralSectionGeometryFactory Factory =
            new StructuralSectionGeometryFactory(Catalog);

        private const string ColumnW = "AISC-W-W10X33";   // d 9.73, bf 7.96, tw 0.29
        private const double Spacing = 96.0;

        private static CantileverLineEditorComputation Build() =>
            new CantileverLineEditorAssembler(Catalog)
                .Build(CantileverRoundTwoCharacterizationTests.Reference());

        private static StructuralSectionGeometry Geometry(string id)
        {
            Assert.True(Catalog.TryGetById(id, out var section), "No esta en el catalogo: " + id);
            return Factory.Get(section.SectionId, SectionDetailLevel.Tabulated);
        }

        private static double WebHalf()
        {
            Assert.True(CantileverBracingPlaneResolver.TryWebHalfThickness(
                Geometry(ColumnW),
                SectionRepresentationOptions.DefaultChordTolerance,
                out var half, out _));

            return half;
        }

        [Fact]
        public void ElALMASeMideSobreElContornoQueSeDibuja()
        {
            // No se lee de una tabla: ADR-0024 D5. Si el separador topara contra un alma tabulada mientras el
            // dibujo pinta otra, la pieza no cerraria con su propio plano.
            // 0.145 = tw/2 del W10X33, cuyo alma tabulada mide 0.290 in. Que lo MEDIDO sobre el contorno
            // coincida con lo tabulado es justamente la comprobacion: el dibujo y la tabla dicen lo mismo, y
            // el separador topa contra el alma que se va a pintar.
            Assert.Equal(0.145, WebHalf(), 6);
        }

        [Fact]
        public void UNTUBONoTieneDondeRecibirElArriostramientoYSeDICE()
        {
            // Falla en cerrado. Devolver un numero cualquiera dibujaria un separador atornillado al aire.
            var hss = Geometry("AISC-HSS-RECT-HSS4X4X_250");

            Assert.False(CantileverBracingPlaneResolver.TryWebHalfThickness(
                hss, SectionRepresentationOptions.DefaultChordTolerance, out _, out var reason));

            Assert.Contains("alma", reason, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void ElCLARODelSeparadorSeMideDeALMAAALMAYNoDePatinAPatin()
        {
            var separator = Build().Line.Intervals[0].Separators[0];

            // 96 - 0.29 = 95.71. De patin a patin habria dado 96 - 7.96 = 88.04, que es lo que salia antes.
            Assert.Equal(Spacing - (2.0 * WebHalf()), separator.CutLength, 6);

            var flangeToFlange = Spacing -
                Geometry(ColumnW).Bounds.Width;

            Assert.True(separator.CutLength > flangeToFlange + 1.0,
                "El separador sigue midiendose de patin a patin.");
        }

        [Fact]
        public void ElSeparadorARRANCAEnLaCaraDelAlmaYNoEnLaDelPatin()
        {
            var separator = Build().Line.Intervals[0].Separators[0];
            var origin = separator.Member.Placement.Frame.Origin;

            Assert.Equal(WebHalf(), origin.X, 6);
        }

        [Fact]
        public void EnPLANTAElSeparadorPasaPORDENTRODeLaColumna()
        {
            // Lo que «centrado» quiere decir aqui, medido: el separador cae dentro del canto de la columna en
            // vez de sobresalir por su cara. Antes se soldaba SOBRE el patin, asi que su sitio empezaba justo
            // donde la columna terminaba.
            var c = Build();
            var planta = CantileverViewPlanBuilder.Build(c.Line, CantileverViewKind.Planta, Factory);

            var separator = planta.Curves
                .Where(x => x.Kind == CantileverViewPieceKind.Separator)
                .SelectMany(x => x.Points)
                .ToList();

            Assert.NotEmpty(separator);

            var column = planta.Curves
                .Where(x => x.Kind == CantileverViewPieceKind.Column)
                .SelectMany(x => x.Points)
                .ToList();

            Assert.True(
                separator.Min(p => p.Y) >= column.Min(p => p.Y) - 1e-6 &&
                separator.Max(p => p.Y) <= column.Max(p => p.Y) + 1e-6,
                "El separador sobresale del canto de la columna en planta: sigue yendo por fuera del patin.");
        }

        [Fact]
        public void LosDOSExtremosLleganIgualDeLejos()
        {
            // Simetria: lo que hace que se vea centrado entre las dos columnas y no corrido hacia una.
            var c = Build();
            var interval = c.Line.Intervals[0];
            var separator = interval.Separators[0];
            var origin = separator.Member.Placement.Frame.Origin;

            var left = origin.X - c.Line.Stations[0].OriginX;
            var right = c.Line.Stations[1].OriginX - (origin.X + separator.CutLength);

            Assert.Equal(left, right, 6);
        }
    }
}
