using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.StructuralSections;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-37D ronda 2, motivo 3 del rechazo: «los perfiles son difíciles de seleccionar».
    ///
    /// La búsqueda es PURA y vive en Application, así que se prueba aquí sin WPF. Lo que fija esta suite es la
    /// distinción que da sentido al control: <b>buscar no es resolver</b>. El texto sirve para ENCONTRAR una
    /// fila; la fila devuelve un id EXACTO. Nada aquí convierte un fragmento en una sección, que es la
    /// resolución por substring que ADR-0024 D6 prohíbe.
    /// </summary>
    public class StructuralSectionSearchTests
    {
        private static readonly StructuralSectionCatalog Catalog =
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private static System.Collections.Generic.IReadOnlyList<StructuralSectionChoice> All() =>
            StructuralSectionSearch.Choices(Catalog);

        // ---- 1. Buscar por designación -------------------------------------------------------------------

        [Fact]
        public void BuscarPorDesignacionEncuentraLaSeccion()
        {
            var hits = StructuralSectionSearch.Filter(All(), "W10X33");

            Assert.Contains(hits, c => c.Id.Value == "AISC-W-W10X33");
        }

        [Fact]
        public void LaBusquedaNoDistingueMayusculasNiSeparadores()
        {
            // Una designación se escribe W10X33 y un id AISC-W-W10X33; quien teclea «w 10x33» quiere lo mismo.
            foreach (var text in new[] { "w10x33", "W 10 X 33", "w-10x33", "W10x33" })
            {
                Assert.Contains(StructuralSectionSearch.Filter(All(), text), c => c.Id.Value == "AISC-W-W10X33");
            }
        }

        // ---- 2. Buscar por id ----------------------------------------------------------------------------

        [Fact]
        public void BuscarPorIdCompletoEncuentraLaSeccion()
        {
            var hits = StructuralSectionSearch.Filter(All(), "AISC-W-W10X33");

            Assert.Single(hits);
            Assert.Equal("AISC-W-W10X33", hits[0].Id.Value);
        }

        // ---- 3. Búsqueda parcial -------------------------------------------------------------------------

        [Fact]
        public void BuscarUnFragmentoEncuentraTodoLoQueLoContiene()
        {
            var hits = StructuralSectionSearch.Filter(All(), "10X33");

            Assert.Contains(hits, c => c.Id.Value == "AISC-W-W10X33");
            Assert.All(hits, c => Assert.Contains("10X33", c.Designation.ToUpperInvariant().Replace(" ", string.Empty)));
        }

        [Fact]
        public void BuscarC4DevuelveLosCanalesDeCuatroPulgadas()
        {
            var hits = StructuralSectionSearch.Filter(All(), "C4");

            Assert.Contains(hits, c => c.Id.Value == "AISC-C-C4X4_5");
            Assert.All(hits, c => Assert.True(
                c.Designation.ToUpperInvariant().Contains("C4") || c.Id.Value.ToUpperInvariant().Contains("C4")));
        }

        // ---- 4. Filtro de familia ------------------------------------------------------------------------

        [Fact]
        public void ElFiltroDeFamiliaDejaSoloEsaFamilia()
        {
            var hits = StructuralSectionSearch.Filter(All(), string.Empty, StructuralSectionFamily.Channel);

            Assert.NotEmpty(hits);
            Assert.All(hits, c => Assert.Equal(StructuralSectionFamily.Channel, c.Family));
        }

        [Fact]
        public void LasCincoFamiliasTienenEtiquetaYEstanEnElOrdenDelManual()
        {
            Assert.Equal(
                new[] { "W", "S", "C", "L", "HSS rectangular" },
                StructuralSectionSearch.Families.Select(StructuralSectionSearch.Label));
        }

        // ---- 5. Búsqueda + familia -----------------------------------------------------------------------

        [Fact]
        public void BusquedaYFamiliaSeCombinan()
        {
            var soloTexto = StructuralSectionSearch.Filter(All(), "4");
            var conFamilia = StructuralSectionSearch.Filter(All(), "4", StructuralSectionFamily.Channel);

            Assert.True(conFamilia.Count < soloTexto.Count);
            Assert.All(conFamilia, c => Assert.Equal(StructuralSectionFamily.Channel, c.Family));
            Assert.Contains(conFamilia, c => c.Id.Value == "AISC-C-C4X4_5");
        }

        // ---- 6. Selección --------------------------------------------------------------------------------

        [Fact]
        public void UnaFilaDevuelveSuIdEXACTOYNoUnFragmento()
        {
            var choice = StructuralSectionSearch.Filter(All(), "W10X33")
                .Single(c => c.Id.Value == "AISC-W-W10X33");

            Assert.Equal("AISC-W-W10X33", choice.Id.Value);
            Assert.Equal("W10X33", choice.Designation);

            // Y el id sale del catálogo tal cual: se resuelve contra él sin ninguna traducción.
            Assert.True(Catalog.TryGetById(choice.Id, out var section));
            Assert.Equal(choice.Designation, section.DisplayName);
        }

        // ---- 7. Limpiar la búsqueda ----------------------------------------------------------------------

        [Fact]
        public void UnaBusquedaVaciaNoFiltraNada()
        {
            var todas = All();

            Assert.Equal(todas.Count, StructuralSectionSearch.Filter(todas, string.Empty).Count);
            Assert.Equal(todas.Count, StructuralSectionSearch.Filter(todas, null).Count);
            Assert.Equal(todas.Count, StructuralSectionSearch.Filter(todas, "   ").Count);
        }

        // ---- 8. Un texto sin resultados -------------------------------------------------------------------

        [Fact]
        public void UnTextoSinCoincidenciasDevuelveVacioYNoInventaUnaSeleccion()
        {
            var hits = StructuralSectionSearch.Filter(All(), "NO-EXISTE-ESTA-SECCION");

            Assert.Empty(hits);
        }

        // ---- 9. Catálogo vacío -----------------------------------------------------------------------------

        [Fact]
        public void UnCatalogoVacioEsUnEstadoLegitimoYNoUnaExcepcion()
        {
            var choices = StructuralSectionSearch.Choices(StructuralSectionCatalog.Empty);

            Assert.Empty(choices);
            Assert.Empty(StructuralSectionSearch.Filter(choices, "W10X33"));
        }

        [Fact]
        public void SinCatalogoSeRechazaExplicitamente()
        {
            Assert.Throws<ArgumentNullException>(() => StructuralSectionSearch.Choices(null));
            Assert.Throws<ArgumentNullException>(() => StructuralSectionSearch.Filter(null, "W"));
        }

        // ---- 10. Filas deshabilitadas ---------------------------------------------------------------------

        [Fact]
        public void UnaSeccionDeshabilitadaNoSeOfrece()
        {
            var offered = All().Select(c => c.Id.Value).ToHashSet();
            var disabled = Catalog.All.Where(s => !s.IsEnabled).ToList();

            Assert.All(disabled, s => Assert.DoesNotContain(s.SectionId.Value, offered));
            Assert.All(All(), c => Assert.True(c.IsEnabled));
        }

        // ---- 11. Familias elegibles del consumidor --------------------------------------------------------

        [Fact]
        public void LasFamiliasElegiblesLasPoneElConsumidorYNoElControl()
        {
            // Un selector de columna ofrece W; uno de separador, canales. Que familias admite un sistema es
            // decision de ese sistema y nunca de un control.
            var soloW = StructuralSectionSearch.Choices(Catalog, new[] { StructuralSectionFamily.W });
            var canalYAngulo = StructuralSectionSearch.Choices(
                Catalog, new[] { StructuralSectionFamily.Channel, StructuralSectionFamily.Angle });

            Assert.All(soloW, c => Assert.Equal(StructuralSectionFamily.W, c.Family));
            Assert.All(canalYAngulo, c => Assert.True(
                c.Family == StructuralSectionFamily.Channel || c.Family == StructuralSectionFamily.Angle));

            // Sin restriccion, el catalogo entero.
            Assert.True(All().Count > soloW.Count);
        }

        [Fact]
        public void ElOrdenEsDeterministaPorFamiliaYDesignacion()
        {
            var first = All().Select(c => c.Id.Value).ToList();
            var second = All().Select(c => c.Id.Value).ToList();

            Assert.Equal(first, second);
        }
    }
}
