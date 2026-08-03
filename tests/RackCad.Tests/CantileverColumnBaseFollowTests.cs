using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.StructuralSections;
using RackCad.Application.Systems.Cantilever;
using RackCad.Domain.Systems.Cantilever;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-37D ronda 2, motivo 4 del rechazo: «la base no sigue inicialmente la sección de columna».
    ///
    /// Las siete reglas del encargo, una por prueba, más la decisión de persistencia y las dos regresiones que
    /// las protegen. Lo que se fija por encima de todo es que seguir es INTENCIÓN GUARDADA y nunca una
    /// comparación de ids.
    /// </summary>
    public class CantileverColumnBaseFollowTests
    {
        private const string ColumnW = "AISC-W-W10X33";
        private const string OtherW = "AISC-W-W12X26";
        private const string Channel = "AISC-C-C4X4_5";

        private static readonly StructuralSectionCatalog Catalog =
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private static CantileverColumnBaseEditorState State(
            CantileverStationColumnBaseTemplateDesign template = null) =>
            new CantileverColumnBaseEditorState(template, CantileverCataloguePolicies.ColumnBase(Catalog));

        // ---- Las siete reglas -----------------------------------------------------------------------------

        [Fact]
        public void UnDisenoNuevoSigueALaColumna()
        {
            Assert.True(State().BaseFollowsColumn);
            Assert.True(new CantileverStationColumnBaseTemplateDesign().BaseFollowsColumn);
        }

        [Fact]
        public void SeleccionarColumnaPoneLaBaseIgualCuandoEsElegible()
        {
            var state = State();

            state.SelectColumn(ColumnW);

            Assert.Equal(ColumnW, state.ColumnSectionId);
            Assert.Equal(ColumnW, state.BaseSectionId);
            Assert.Empty(state.Diagnostics);
        }

        [Fact]
        public void CambiarLaBaseAManoApagaElSeguimiento()
        {
            var state = State();
            state.SelectColumn(ColumnW);

            state.SelectBase(OtherW);

            Assert.False(state.BaseFollowsColumn);
            Assert.Equal(OtherW, state.BaseSectionId);
        }

        [Fact]
        public void UsarLaMismaSeccionLoVuelveAEncenderYLaAplicaEnElActo()
        {
            var state = State();
            state.SelectColumn(ColumnW);
            state.SelectBase(OtherW);

            state.UseSameSectionAsColumn();

            Assert.True(state.BaseFollowsColumn);
            Assert.Equal(ColumnW, state.BaseSectionId);
        }

        [Fact]
        public void CambiarLaColumnaConSeguimientoEncendidoArrastraLaBase()
        {
            var state = State();
            state.SelectColumn(ColumnW);

            state.SelectColumn(OtherW);

            Assert.Equal(OtherW, state.BaseSectionId);
        }

        [Fact]
        public void CambiarLaColumnaConSeguimientoApagadoConservaLaBase()
        {
            var state = State();
            state.SelectColumn(ColumnW);
            state.SelectBase(OtherW);

            state.SelectColumn(ColumnW);

            Assert.Equal(OtherW, state.BaseSectionId); // la base del usuario es del usuario
            Assert.False(state.BaseFollowsColumn);
        }

        [Fact]
        public void UnaColumnaNoElegibleComoBaseNoNormalizaYLoDICE()
        {
            // Un canal no es una base admitida por la politica de producto. Ni se copia en silencio —produciria
            // una linea que no resuelve— ni se calla —pareceria que la regla se olvido—.
            var state = State();
            state.SelectColumn(ColumnW);

            state.SelectColumn(Channel);

            Assert.Equal(Channel, state.ColumnSectionId);
            Assert.Equal(ColumnW, state.BaseSectionId); // la base NO se toco
            Assert.Contains(state.Diagnostics, d => d.Code == CantileverDiagnostics.ColumnBaseSectionNotEligible);
            Assert.True(state.BaseFollowsColumn); // el seguimiento sigue encendido: fallo la copia, no la regla
        }

        // ---- Seguir es intencion, no una comparacion -------------------------------------------------------

        [Fact]
        public void ElSeguimientoNOSeDeduceComparandoLosIds()
        {
            // Dos secciones pueden coincidir por casualidad. Un usuario que eligio a mano la MISMA base sigue
            // teniendo el seguimiento apagado, y su decision sobrevive al siguiente cambio de columna.
            var state = State();
            state.SelectColumn(ColumnW);
            state.SelectBase(ColumnW); // a mano, e igual a la columna

            Assert.False(state.BaseFollowsColumn);

            state.SelectColumn(OtherW);

            Assert.Equal(ColumnW, state.BaseSectionId);
        }

        // ---- Persistencia ---------------------------------------------------------------------------------

        [Fact]
        public void ElSeguimientoSePERSISTE_ParaQueReabrirNoCambieLaReglaPorDebajo()
        {
            var design = CantileverRoundTwoCharacterizationTests.Reference();
            design.StationTopology.ColumnBaseTemplate.BaseFollowsColumn = false;

            var store = new RackProjectStore();
            var reloaded = store.Deserialize(store.Serialize(RackProject.ForCantilever(design)));

            Assert.False(reloaded.CantileverLineDesign.StationTopology.ColumnBaseTemplate.BaseFollowsColumn);

            // Y encendido tambien viaja.
            design.StationTopology.ColumnBaseTemplate.BaseFollowsColumn = true;
            var again = store.Deserialize(store.Serialize(RackProject.ForCantilever(design)));

            Assert.True(again.CantileverLineDesign.StationTopology.ColumnBaseTemplate.BaseFollowsColumn);
        }

        [Fact]
        public void UnJsonSinLaClaveDejaElSeguimientoEncendido()
        {
            // Ningun diseno guardado precede al campo —I-37D no esta integrada—, asi que el default del diseno
            // hace de fallback igual que para el resto de sus propiedades.
            var json = new RackProjectStore().Serialize(RackProject.ForCantilever(
                CantileverRoundTwoCharacterizationTests.Reference()));

            Assert.Contains("BaseFollowsColumn", json, System.StringComparison.Ordinal);

            var sinClave = json.Replace("\"BaseFollowsColumn\": true,", string.Empty)
                               .Replace("\"BaseFollowsColumn\":true,", string.Empty);
            var reloaded = new RackProjectStore().Deserialize(sinClave);

            Assert.True(reloaded.CantileverLineDesign.StationTopology.ColumnBaseTemplate.BaseFollowsColumn);
        }

        [Fact]
        public void LaCopiaProfundaLoLleva()
        {
            var template = new CantileverStationColumnBaseTemplateDesign { BaseFollowsColumn = false };

            Assert.False(template.DeepCopy().BaseFollowsColumn);
        }

        // ---- El editor trabaja sobre una COPIA -------------------------------------------------------------

        [Fact]
        public void CancelarNoMutaElDisenoOriginal()
        {
            var original = new CantileverStationColumnBaseTemplateDesign
            {
                ColumnSectionId = ColumnW,
                Base = new CantileverBaseDesign { SectionId = ColumnW, Length = 48.0 }
            };

            var state = State(original);
            state.SelectColumn(OtherW);
            state.SelectBase(Channel);

            // Nada se acepto: el original sigue como estaba.
            Assert.Equal(ColumnW, original.ColumnSectionId);
            Assert.Equal(ColumnW, original.Base.SectionId);
            Assert.True(original.BaseFollowsColumn);
        }

        [Fact]
        public void AceptarDevuelveUnaCopiaYNoLaInstanciaDelEditor()
        {
            var state = State();
            state.SelectColumn(ColumnW);

            var accepted = state.Accept();
            accepted.ColumnSectionId = Channel;

            Assert.Equal(ColumnW, state.ColumnSectionId);
        }

        // ---- El resumen de la tarjeta ---------------------------------------------------------------------

        [Fact]
        public void ElResumenEsCompactoYDeterminista()
        {
            var state = State();
            state.SelectColumn(ColumnW);

            Assert.Equal("W10X33 · base W10X33 · sigue a la columna", state.Summary());

            state.SelectBase(OtherW);
            Assert.Equal("W10X33 · base W12X26 · base manual", state.Summary());

            Assert.Equal(state.Summary(), state.Summary());
        }

        [Fact]
        public void UnaLineaConSeguimientoSigueResolviendoIgual()
        {
            // La regla es del EDITOR: no cambia lo que la linea resuelve.
            var design = CantileverRoundTwoCharacterizationTests.Reference();
            var assembler = new CantileverLineEditorAssembler(Catalog);
            var conSeguimiento = assembler.Build(design);

            design.StationTopology.ColumnBaseTemplate.BaseFollowsColumn = false;
            var sinSeguimiento = assembler.Build(design);

            Assert.Equal(conSeguimiento.Line.Signature(), sinSeguimiento.Line.Signature());
        }
    }
}
