using System;
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
    /// I-37D, corrección de columna y base — los dos márgenes retirados.
    ///
    /// `OWNER_REJECTED_I_37D_MANUAL_VALIDATION_ROUND_2_COLUMN_BASE`, motivo 1: `ColumnBottomPlateEndOffset` y
    /// `ColumnTopPunchOffset` eran entradas obligatorias sin utilidad de producto. Lo que limita un agujero no
    /// es un número que alguien escribe: es si el agujero ENTERO cabe.
    ///
    /// <para>Las propiedades no se borran —pertenecen al contrato de I-37A ya integrado en <c>main</c>— pero
    /// quedan legacy: ignoradas al resolver, ausentes de la UI, fuera del JSON nuevo y sin poder influir en una
    /// sola cantidad de agujeros. Esta suite es lo que impide que vuelvan por cualquiera de esas puertas.</para>
    /// </summary>
    public class CantileverRetiredMarginTests
    {
        private static readonly StructuralSectionCatalog Catalog =
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private static CantileverLineEditorAssembler Assembler() => new CantileverLineEditorAssembler(Catalog);

        /// <summary>The reference line, with the two legacy values ABSENT — which is every new line.</summary>
        private static CantileverLineDesign WithoutMargins()
        {
            var design = CantileverRoundTwoCharacterizationTests.Reference();
            var punches = design.StationTopology.ColumnBaseTemplate.Connection.Punches;
            punches.ColumnBottomPlateEndOffset = null;
            punches.ColumnTopPunchOffset = null;
            return design;
        }

        // ---- 1. No influyen en NADA ------------------------------------------------------------------------

        [Fact]
        public void UnaLineaConLosMargenesYOtraSinEllosResuelvenIGUAL()
        {
            // La prueba central de la retirada: el valor legacy no cambia una sola coordenada ni un solo
            // agujero. Si alguien lo volviera a leer, estas dos firmas dejarian de coincidir.
            var conMargenes = Assembler().Build(CantileverRoundTwoCharacterizationTests.Reference());
            var sinMargenes = Assembler().Build(WithoutMargins());

            Assert.Equal(conMargenes.Line.Signature(), sinMargenes.Line.Signature());
        }

        [Fact]
        public void UnValorLEGACYABSURDONoCambiaNiUnAgujero()
        {
            // 40 in de margen habria dejado la columna casi sin troqueles bajo la regla vieja.
            var design = CantileverRoundTwoCharacterizationTests.Reference();
            design.StationTopology.ColumnBaseTemplate.Connection.Punches.ColumnTopPunchOffset = 40.0;
            design.StationTopology.ColumnBaseTemplate.Connection.Punches.ColumnBottomPlateEndOffset = 40.0;

            var raro = Assembler().Build(design);
            var normal = Assembler().Build(WithoutMargins());

            Assert.Equal(normal.Line.Signature(), raro.Line.Signature());
            Assert.Equal(
                normal.Line.Stations.Sum(s => s.Station.Punches.Count),
                raro.Line.Stations.Sum(s => s.Station.Punches.Count));
        }

        [Fact]
        public void UnaLineaSinEllosNoSeBLOQUEA()
        {
            var computation = Assembler().Build(WithoutMargins());

            Assert.True(computation.IsValid);
            Assert.Null(computation.Error);
            Assert.NotNull(computation.Bom);
        }

        // ---- 2. La regla unica vive en la autoridad ---------------------------------------------------------

        [Fact]
        public void LaReticulaConoceElRADIOYSeDetieneSolaEnElUltimoAgujeroEntero()
        {
            var computation = Assembler().Build(WithoutMargins());
            var columnBase = computation.Line.Stations[0].Station.ColumnBase;
            var grid = CantileverColumnRegularPunchGrid.FromPattern(columnBase.Pattern);

            Assert.Equal(grid.Diameter / 2.0, grid.PunchRadius, 12);

            var top = computation.Line.ColumnHeight;
            var elevations = grid.ElevationsUpTo(top);

            Assert.NotEmpty(elevations);
            Assert.True(elevations.Last() + grid.PunchRadius <= top + 1e-9, "El ultimo agujero sobresale.");
            Assert.True(
                elevations.Last() + grid.Pitch + grid.PunchRadius > top + 1e-9,
                "Cabria otro agujero entero y la reticula no lo coloco.");
        }

        [Fact]
        public void UnAgujeroTANGENTEAlBordeCABE()
        {
            var columnBase = Assembler().Build(WithoutMargins()).Line.Stations[0].Station.ColumnBase;
            var grid = CantileverColumnRegularPunchGrid.FromPattern(columnBase.Pattern);

            // Justo la altura en la que el agujero toca el borde: entra.
            var tangente = grid.ElevationAt(3) + grid.PunchRadius;

            Assert.Equal(4, grid.CountUpTo(tangente));
            Assert.True(grid.Contains(3, tangente));
        }

        [Fact]
        public void UnAgujeroQueINVADEElBordePorMasQueLaToleranciaNoCABE()
        {
            var columnBase = Assembler().Build(WithoutMargins()).Line.Stations[0].Station.ColumnBase;
            var grid = CantileverColumnRegularPunchGrid.FromPattern(columnBase.Pattern);

            var justoDebajo = grid.ElevationAt(3) + grid.PunchRadius - 0.001;

            Assert.Equal(3, grid.CountUpTo(justoDebajo));
            Assert.False(grid.Contains(3, justoDebajo));
        }

        [Fact]
        public void UnaColumnaEnLaQueNoCabeNINGUNOEsUnaRespuestaLEGITIMA()
        {
            var columnBase = Assembler().Build(WithoutMargins()).Line.Stations[0].Station.ColumnBase;
            var grid = CantileverColumnRegularPunchGrid.FromPattern(columnBase.Pattern);

            Assert.Empty(grid.ElevationsUpTo(grid.FirstElevation + grid.PunchRadius - 0.001));
            Assert.Single(grid.ElevationsUpTo(grid.FirstElevation + grid.PunchRadius));
        }

        [Fact]
        public void LaMismaEntradaDaLaMismaCANTIDAD()
        {
            var first = Assembler().Build(WithoutMargins());
            var second = Assembler().Build(WithoutMargins());

            Assert.Equal(
                first.Line.Stations.Sum(s => s.Station.Punches.Count),
                second.Line.Stations.Sum(s => s.Station.Punches.Count));
        }

        // ---- 3. Persistencia: no vuelven a escribirse -------------------------------------------------------

        [Fact]
        public void ElJSONNuevoNoESCRIBELosDosMargenes()
        {
            var json = new RackProjectStore().Serialize(
                RackProject.ForCantilever(CantileverRoundTwoCharacterizationTests.Reference()));

            Assert.DoesNotContain("ColumnBottomPlateEndOffset", json, StringComparison.Ordinal);
            Assert.DoesNotContain("ColumnTopPunchOffset", json, StringComparison.Ordinal);

            // Y el resto del bloque de troqueles sigue estando: no se borro de mas.
            Assert.Contains("ColumnBottomPlatePitch", json, StringComparison.Ordinal);
            Assert.Contains("RegularColumnPitch", json, StringComparison.Ordinal);
        }

        [Fact]
        public void UnDocumentoANTIGUOQueSiLosTraeCargaResuelveYSeGuardaSINEllos()
        {
            // El round-trip que el encargo pide: deserializar -> resolver -> serializar, y las claves no
            // reaparecen. Tampoco vuelven por ExtensionData: se descartan en el limite de serializacion.
            var store = new RackProjectStore();
            var limpio = store.Serialize(RackProject.ForCantilever(WithoutMargins()));

            var antiguo = limpio.Replace(
                "\"ColumnBottomPlatePitch\":",
                "\"ColumnBottomPlateEndOffset\": 1.5,\n            \"ColumnTopPunchOffset\": 4.0,\n            \"ColumnBottomPlatePitch\":");

            Assert.Contains("ColumnTopPunchOffset", antiguo, StringComparison.Ordinal);

            var reloaded = store.Deserialize(antiguo);

            Assert.NotNull(reloaded.CantileverLineDesign);

            var punches = reloaded.CantileverLineDesign.StationTopology.ColumnBaseTemplate.Connection.Punches;
            Assert.Null(punches.ColumnBottomPlateEndOffset);
            Assert.Null(punches.ColumnTopPunchOffset);

            // Resuelve igual...
            Assert.Equal(
                Assembler().Build(WithoutMargins()).Line.Signature(),
                Assembler().Build(reloaded.CantileverLineDesign).Line.Signature());

            // ...y al re-guardarlo las claves NO reaparecen.
            var again = store.Serialize(RackProject.ForCantilever(reloaded.CantileverLineDesign));

            Assert.DoesNotContain("ColumnBottomPlateEndOffset", again, StringComparison.Ordinal);
            Assert.DoesNotContain("ColumnTopPunchOffset", again, StringComparison.Ordinal);
        }

        // ---- 4. El BOM no cambia ----------------------------------------------------------------------------

        [Fact]
        public void ElBOMEsIDENTICOConYSinLosMargenes()
        {
            // Un troquel no es una linea de BOM: retirarlos no puede mover una sola cantidad comercial.
            var conMargenes = Assembler().Build(CantileverRoundTwoCharacterizationTests.Reference()).Bom;
            var sinMargenes = Assembler().Build(WithoutMargins()).Bom;

            Assert.Equal(conMargenes.Components.Count, sinMargenes.Components.Count);

            foreach (var componente in conMargenes.Components)
            {
                var par = sinMargenes.Components.Single(
                    c => c.Category == componente.Category && c.ProfileId == componente.ProfileId);

                Assert.Equal(componente.Quantity, par.Quantity);
                Assert.Equal(componente.Length, par.Length, 9);
            }
        }
    }
}
