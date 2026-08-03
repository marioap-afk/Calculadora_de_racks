using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.StructuralSections;
using RackCad.Application.StructuralSections.Geometry;
using RackCad.Application.Systems.Cantilever;
using RackCad.Domain.Systems.Cantilever;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-37D ronda 4, frente B — el EDITOR AVANZADO DE PANELES.
    ///
    /// <para>Hasta esta ronda la secuencia vertical de un tramo la decidía entera la regla del producto:
    /// cuántos paneles caben en la altura, en bloques de a dos, con el hueco central entre bloques. Es una
    /// buena regla y sigue siendo la de por omisión; lo que no era, era negociable. El dueño pidió poder
    /// declarar la secuencia tramo a tramo.</para>
    ///
    /// <para><b>La idea que sostiene todo el frente:</b> los dos modos producen la MISMA cosa — una lista de
    /// tramos contigua, de abajo arriba — y el resolver posterior sólo conoce esa lista. Automático no es un
    /// camino distinto hasta el dibujo: es otra forma de escribir la lista.</para>
    ///
    /// <para>Y un vacío es un TRAMO, con los tensores apagados. Nunca la ausencia de uno: un hueco implícito no
    /// se distingue de un tramo que alguien olvidó escribir.</para>
    /// </summary>
    public class CantileverAdvancedPanelLayoutTests
    {
        private static readonly StructuralSectionCatalog Catalog =
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve()).Load();

        private static readonly StructuralSectionGeometryFactory Factory =
            new StructuralSectionGeometryFactory(Catalog);

        private const double ColumnHeight = 240.0;

        private static CantileverBracingDesign Bracing() => new CantileverBracingDesign();

        private static CantileverBracingLayout Layout(
            CantileverBracingDesign bracing, double columnHeight = ColumnHeight) =>
            CantileverBracingLayoutResolver.Resolve(bracing, columnHeight, heightIsManual: true);

        private static CantileverPanelSegmentDesign Seg(
            double a, double b, CantileverPanelBracingMode mode) =>
            new CantileverPanelSegmentDesign
            {
                StartElevation = a,
                EndElevation = b,
                BracingMode = mode
            };

        private static CantileverPanelSegmentDesign Braced(double a, double b) =>
            Seg(a, b, CantileverPanelBracingMode.CrossBraced);

        private static CantileverPanelSegmentDesign Empty(double a, double b) =>
            Seg(a, b, CantileverPanelBracingMode.None);

        private static CantileverBracingDesign Advanced(params CantileverPanelSegmentDesign[] segments)
        {
            var bracing = Bracing();
            bracing.PanelLayoutMode = CantileverPanelLayoutMode.Advanced;
            bracing.AdvancedPanelSegments = segments.ToList();
            return bracing;
        }

        private static bool Has(CantileverBracingLayout layout, string code) =>
            layout.Diagnostics.Any(d => d.Code == code);

        private static string Why(CantileverBracingLayout layout) =>
            string.Join(" | ", layout.Diagnostics.Select(d => d.Code + ": " + d.Message));

        // ---- 1. El automatico reproduce la tabla vigente --------------------------------------------------

        [Fact]
        public void ElAutomaticoSIGUEProduciendoLaSecuenciaDeLaREGLA()
        {
            // La comprobacion de que meter una lista efectiva por en medio no cambio el producto. Los pines de
            // caracterizacion ya lo dicen sobre la linea entera; esto lo dice sobre la secuencia sola, que es
            // donde se veria primero.
            var layout = Layout(Bracing());

            Assert.False(layout.IsBlocked, Why(layout));
            Assert.Equal(CantileverPanelLayoutMode.Automatic, layout.Mode);

            var esperados = CantileverBracingLayoutResolver.StandardBracedPanelCount(ColumnHeight);

            Assert.Equal(esperados, layout.BracedPanelCount);
            Assert.Equal(
                CantileverBracingLayoutResolver.CentralEmptySpaceCount(esperados),
                layout.CentralEmptySpaceCount);

            // paneles + huecos + 1, que es la regla de separadores compartidos escrita como cuenta.
            Assert.Equal(
                layout.BracedPanelCount + layout.CentralEmptySpaceCount + 1,
                layout.SeparatorCount);

            Assert.Equal(2 * layout.BracedPanelCount, layout.BraceCount);
        }

        [Fact]
        public void ElAutomaticoPUBLICASuListaEfectivaYEsContigua()
        {
            // La lista existe tambien en automatico, y es la que se materializa al pasar a avanzado.
            var layout = Layout(Bracing());

            Assert.NotEmpty(layout.EffectiveSegments);

            for (var i = 1; i < layout.EffectiveSegments.Count; i++)
            {
                Assert.Equal(
                    layout.EffectiveSegments[i - 1].EndElevation,
                    layout.EffectiveSegments[i].StartElevation,
                    9);
            }

            // Cubre el NUCLEO y no la columna entera: por debajo y por encima quedan los espacios externos.
            Assert.True(layout.EffectiveSegments[0].StartElevation > 0.0);
            Assert.True(
                layout.EffectiveSegments[layout.EffectiveSegments.Count - 1].EndElevation < ColumnHeight);
        }

        // ---- 2. El avanzado: tramos declarados ------------------------------------------------------------

        [Fact]
        public void UnTramoARRIOSTRADOProduceDosTensoresYUnoVACIONinguno()
        {
            var layout = Layout(Advanced(
                Braced(20.0, 60.0),
                Empty(60.0, 100.0),
                Braced(100.0, 140.0)));

            Assert.False(layout.IsBlocked, Why(layout));
            Assert.Equal(CantileverPanelLayoutMode.Advanced, layout.Mode);

            Assert.Equal(2, layout.BracedPanelCount);
            Assert.Equal(1, layout.CentralEmptySpaceCount);
            Assert.Equal(4, layout.BraceCount);

            // Y los tensores caen SOLO donde corresponde: en los dos tramos arriostrados, no en el vacio.
            var arriostrados = layout.BracedPanels.Select(p => (p.BottomZ, p.TopZ)).ToList();

            Assert.Contains((20.0, 60.0), arriostrados);
            Assert.Contains((100.0, 140.0), arriostrados);
            Assert.DoesNotContain((60.0, 100.0), arriostrados);
        }

        [Fact]
        public void LasFRONTERASCompartidasDanUNSeparadorYNoDOS()
        {
            // Tres tramos que se tocan tienen CUATRO fronteras, no seis: la de en medio de cada par es la
            // misma cota y ahi va un separador, no dos.
            var layout = Layout(Advanced(
                Braced(20.0, 60.0),
                Empty(60.0, 100.0),
                Braced(100.0, 140.0)));

            Assert.Equal(new[] { 20.0, 60.0, 100.0, 140.0 }, layout.SeparatorElevations);
            Assert.Equal(4, layout.SeparatorCount);
            Assert.Equal(layout.SeparatorElevations.Distinct().Count(), layout.SeparatorCount);
        }

        [Fact]
        public void UnaSecuenciaDeUnSoloTramoVACIOEsLegal()
        {
            // No arriostrar es una decision, y se declara: un tramo con los tensores apagados. No es lo mismo
            // que una lista vacia, que es un olvido.
            var layout = Layout(Advanced(Empty(10.0, 200.0)));

            Assert.False(layout.IsBlocked, Why(layout));
            Assert.Equal(0, layout.BracedPanelCount);
            Assert.Equal(0, layout.BraceCount);
            Assert.Equal(2, layout.SeparatorCount);
        }

        // ---- 3. Lo que se RECHAZA ------------------------------------------------------------------------

        [Fact]
        public void UnHUECOImplicitoSeRECHAZA()
        {
            var layout = Layout(Advanced(Braced(20.0, 60.0), Braced(80.0, 120.0)));

            Assert.True(layout.IsBlocked);
            Assert.True(Has(layout, CantileverDiagnostics.BracingAdvancedLayoutHasGap), Why(layout));
        }

        [Fact]
        public void UnSOLAPESeRECHAZA()
        {
            var layout = Layout(Advanced(Braced(20.0, 60.0), Braced(50.0, 100.0)));

            Assert.True(layout.IsBlocked);
            Assert.True(Has(layout, CantileverDiagnostics.BracingAdvancedLayoutOverlaps), Why(layout));
        }

        [Fact]
        public void UnTramoDeAlturaCEROSeRECHAZA()
        {
            var layout = Layout(Advanced(Braced(20.0, 60.0), Braced(60.0, 60.0), Braced(60.0, 100.0)));

            Assert.True(layout.IsBlocked);
            Assert.True(Has(layout, CantileverDiagnostics.BracingAdvancedSegmentNotAscending), Why(layout));
        }

        [Fact]
        public void UnTramoAlREVESSeRECHAZA()
        {
            var layout = Layout(Advanced(Braced(60.0, 20.0)));

            Assert.True(layout.IsBlocked);
            Assert.True(Has(layout, CantileverDiagnostics.BracingAdvancedSegmentNotAscending), Why(layout));
        }

        [Fact]
        public void UnaListaVACIAEnModoAvanzadoSeRECHAZA()
        {
            var layout = Layout(Advanced());

            Assert.True(layout.IsBlocked);
            Assert.True(Has(layout, CantileverDiagnostics.BracingAdvancedLayoutEmpty), Why(layout));
        }

        [Fact]
        public void UnTramoPorDEBAJODelPisoSeRECHAZA()
        {
            var layout = Layout(Advanced(Braced(-5.0, 60.0)));

            Assert.True(layout.IsBlocked);
            Assert.True(Has(layout, CantileverDiagnostics.BracingAdvancedLayoutBelowFloor), Why(layout));
        }

        [Fact]
        public void UnTramoQueSEPASADeLaPuntaDeLaColumnaSeRECHAZA()
        {
            var layout = Layout(Advanced(Braced(20.0, ColumnHeight + 10.0)));

            Assert.True(layout.IsBlocked);
            Assert.True(Has(layout, CantileverDiagnostics.BracingDoesNotFitTheColumn), Why(layout));
        }

        // ---- 4. Los dos cambios de modo ------------------------------------------------------------------

        [Fact]
        public void PasarAAvanzadoMATERIALIZALaListaAutomaticaActual()
        {
            var bracing = Bracing();
            var automatic = Layout(bracing);

            var editor = new CantileverPanelLayoutEditorState(
                CantileverPanelLayoutMode.Automatic, Array.Empty<CantileverPanelSegmentDesign>());

            Assert.True(editor.MaterializeAutomatic(automatic.EffectiveSegments).Applied);
            Assert.True(editor.IsAdvanced);

            // El usuario empieza con la lista que ya estaba viendo, no con una en blanco.
            Assert.Equal(automatic.EffectiveSegments.Count, editor.Segments.Count);

            editor.ApplyTo(bracing);

            var advanced = Layout(bracing);

            Assert.False(advanced.IsBlocked, Why(advanced));
            Assert.Equal(CantileverPanelLayoutMode.Advanced, advanced.Mode);

            // Y materializar NO cambia el dibujo: mismos separadores, mismos paneles.
            Assert.Equal(automatic.SeparatorElevations, advanced.SeparatorElevations);
            Assert.Equal(automatic.BracedPanelCount, advanced.BracedPanelCount);
            Assert.Equal(automatic.BraceCount, advanced.BraceCount);
        }

        [Fact]
        public void VolverAAutomaticoAVISAYDevuelveLaAutoridadALaRegla()
        {
            var editor = new CantileverPanelLayoutEditorState(
                CantileverPanelLayoutMode.Advanced, new[] { Braced(10.0, 50.0), Empty(50.0, 90.0) });

            var result = editor.RestoreAutomatic();

            Assert.True(result.Applied);
            Assert.True(result.ReplacesManualWork, "Volver a automatico tiene que avisar.");
            Assert.NotEmpty(result.Reason);
            Assert.False(editor.IsAdvanced);

            // La lista NO se pierde: se conserva como dato dormido para poder volver.
            Assert.Equal(2, editor.Segments.Count);

            var bracing = Bracing();
            editor.ApplyTo(bracing);

            var layout = Layout(bracing);

            Assert.Equal(CantileverPanelLayoutMode.Automatic, layout.Mode);
            Assert.Equal(
                CantileverBracingLayoutResolver.StandardBracedPanelCount(ColumnHeight),
                layout.BracedPanelCount);
        }

        // ---- 5. Las acciones del editor ------------------------------------------------------------------

        private static CantileverPanelLayoutEditorState Editor() =>
            new CantileverPanelLayoutEditorState(
                CantileverPanelLayoutMode.Advanced,
                new[] { Braced(20.0, 60.0), Empty(60.0, 100.0), Braced(100.0, 140.0) });

        [Fact]
        public void AGREGARPoneUnTramoEncimaDelUltimoYNoDejaHueco()
        {
            var editor = Editor();

            Assert.True(editor.Add(40.0, CantileverPanelBracingMode.CrossBraced).Applied);
            Assert.Equal(4, editor.Segments.Count);
            Assert.Equal(140.0, editor.Segments[3].StartElevation, 9);
            Assert.Equal(180.0, editor.Segments[3].EndElevation, 9);
        }

        [Fact]
        public void ELIMINARCierraElVacioBajandoLoQueHabiaEncima()
        {
            var editor = Editor();

            Assert.True(editor.Remove(1).Applied);
            Assert.Equal(2, editor.Segments.Count);

            // El tramo de arriba baja los 40 in del que se fue, asi que la secuencia sigue contigua.
            Assert.Equal(60.0, editor.Segments[1].StartElevation, 9);
            Assert.Equal(100.0, editor.Segments[1].EndElevation, 9);
        }

        [Fact]
        public void ELIMINARElUltimoTramoQueQuedaSeRECHAZA()
        {
            var editor = new CantileverPanelLayoutEditorState(
                CantileverPanelLayoutMode.Advanced, new[] { Braced(10.0, 50.0) });

            var result = editor.Remove(0);

            Assert.False(result.Applied);
            Assert.NotEmpty(result.Reason);
        }

        [Fact]
        public void MOVERIntercambiaContenidoYNoCotas()
        {
            var editor = new CantileverPanelLayoutEditorState(
                CantileverPanelLayoutMode.Advanced,
                new[] { Braced(0.0, 40.0), Empty(40.0, 100.0) });

            Assert.True(editor.Move(0, 1).Applied);

            // El vacio, que medía 60, pasa abajo; el arriostrado de 40 pasa arriba. El techo del par no se
            // mueve y la secuencia sigue contigua: mover no puede romper la continuidad.
            Assert.Equal(CantileverPanelBracingMode.None, editor.Segments[0].BracingMode);
            Assert.Equal(0.0, editor.Segments[0].StartElevation, 9);
            Assert.Equal(60.0, editor.Segments[0].EndElevation, 9);

            Assert.Equal(CantileverPanelBracingMode.CrossBraced, editor.Segments[1].BracingMode);
            Assert.Equal(60.0, editor.Segments[1].StartElevation, 9);
            Assert.Equal(100.0, editor.Segments[1].EndElevation, 9);
        }

        [Fact]
        public void MOVERFueraDeLaSecuenciaSeRECHAZA()
        {
            var editor = Editor();

            Assert.False(editor.Move(0, -1).Applied);
            Assert.False(editor.Move(2, 1).Applied);
        }

        [Fact]
        public void DIVIDIRParteElTramoPorSuMitadYHeredaElArriostramiento()
        {
            var editor = Editor();

            Assert.True(editor.Split(0).Applied);
            Assert.Equal(4, editor.Segments.Count);

            Assert.Equal(20.0, editor.Segments[0].StartElevation, 9);
            Assert.Equal(40.0, editor.Segments[0].EndElevation, 9);
            Assert.Equal(40.0, editor.Segments[1].StartElevation, 9);
            Assert.Equal(60.0, editor.Segments[1].EndElevation, 9);

            Assert.All(
                editor.Segments.Take(2),
                s => Assert.Equal(CantileverPanelBracingMode.CrossBraced, s.BracingMode));
        }

        [Fact]
        public void UNIRExigeQueLosDosLlevenLoMISMODentro()
        {
            var editor = Editor();

            // Arriostrado con vacio: no. Decidir cual gana es del usuario.
            var rejected = editor.MergeWithNext(0);

            Assert.False(rejected.Applied);
            Assert.NotEmpty(rejected.Reason);

            // Dos iguales y contiguos: si.
            var pair = new CantileverPanelLayoutEditorState(
                CantileverPanelLayoutMode.Advanced,
                new[] { Braced(20.0, 60.0), Braced(60.0, 100.0) });

            Assert.True(pair.MergeWithNext(0).Applied);
            Assert.Single(pair.Segments);
            Assert.Equal(20.0, pair.Segments[0].StartElevation, 9);
            Assert.Equal(100.0, pair.Segments[0].EndElevation, 9);
        }

        [Fact]
        public void ALTERNARTensoresConvierteUnTramoEnHuecoYAlReves()
        {
            var editor = Editor();

            Assert.True(editor.ToggleBracing(0).Applied);
            Assert.Equal(CantileverPanelBracingMode.None, editor.Segments[0].BracingMode);

            Assert.True(editor.ToggleBracing(0).Applied);
            Assert.Equal(CantileverPanelBracingMode.CrossBraced, editor.Segments[0].BracingMode);
        }

        // ---- 6. Persistencia -----------------------------------------------------------------------------

        [Fact]
        public void ElModoYLosTramosSobrevivenAlROUNDTRIP()
        {
            var design = CantileverRoundTwoCharacterizationTests.Reference();

            design.Bracing.PanelLayoutMode = CantileverPanelLayoutMode.Advanced;
            design.Bracing.AdvancedPanelSegments = new List<CantileverPanelSegmentDesign>
            {
                Braced(12.5, 52.5),
                Empty(52.5, 92.5),
                Braced(92.5, 132.5)
            };

            var store = new RackProjectStore();
            var json = store.Serialize(RackProject.ForCantilever(design));
            var back = store.Deserialize(json).CantileverLineDesign;

            Assert.Equal(CantileverPanelLayoutMode.Advanced, back.Bracing.PanelLayoutMode);
            Assert.Equal(3, back.Bracing.AdvancedPanelSegments.Count);

            // EL ORDEN es parte del dato: una secuencia es de abajo arriba.
            for (var i = 0; i < 3; i++)
            {
                Assert.Equal(
                    design.Bracing.AdvancedPanelSegments[i].StartElevation,
                    back.Bracing.AdvancedPanelSegments[i].StartElevation, 9);

                Assert.Equal(
                    design.Bracing.AdvancedPanelSegments[i].EndElevation,
                    back.Bracing.AdvancedPanelSegments[i].EndElevation, 9);

                Assert.Equal(
                    design.Bracing.AdvancedPanelSegments[i].BracingMode,
                    back.Bracing.AdvancedPanelSegments[i].BracingMode);
            }

            // Y es DETERMINISTA: volver a escribirlo da el mismo texto.
            Assert.Equal(json, store.Serialize(RackProject.ForCantilever(back)));
        }

        [Fact]
        public void ElJSONNOGuardaLaAlturaDerivadaDeUnTramo()
        {
            var design = CantileverRoundTwoCharacterizationTests.Reference();

            design.Bracing.PanelLayoutMode = CantileverPanelLayoutMode.Advanced;
            design.Bracing.AdvancedPanelSegments =
                new List<CantileverPanelSegmentDesign> { Braced(20.0, 60.0) };

            var json = new RackProjectStore().Serialize(RackProject.ForCantilever(design));

            Assert.Contains("StartElevation", json, StringComparison.Ordinal);
            Assert.Contains("EndElevation", json, StringComparison.Ordinal);
            Assert.Contains("BracingMode", json, StringComparison.Ordinal);

            // La altura es DERIVADA. Guardarla junto a las cotas seria una tercera autoridad sobre el mismo
            // hecho, y el dia que un archivo trajera una que no cuadra nadie sabria cual manda.
            Assert.DoesNotContain("\"Height\"", json, StringComparison.Ordinal);
        }

        [Fact]
        public void LaListaSeCONSERVAAunkeElModoSeaAutomatico()
        {
            // Dato dormido, igual que el conteo manual bajo conteo automatico: se guarda, no se lee. Es lo que
            // permite volver a avanzado sin rehacer el trabajo.
            var design = CantileverRoundTwoCharacterizationTests.Reference();

            design.Bracing.PanelLayoutMode = CantileverPanelLayoutMode.Automatic;
            design.Bracing.AdvancedPanelSegments =
                new List<CantileverPanelSegmentDesign> { Braced(20.0, 60.0), Empty(60.0, 100.0) };

            var store = new RackProjectStore();
            var back = store.Deserialize(
                store.Serialize(RackProject.ForCantilever(design))).CantileverLineDesign;

            Assert.Equal(2, back.Bracing.AdvancedPanelSegments.Count);

            // Pero NO manda: la secuencia sigue siendo la de la regla.
            var layout = Layout(back.Bracing);

            Assert.Equal(CantileverPanelLayoutMode.Automatic, layout.Mode);
            Assert.Equal(
                CantileverBracingLayoutResolver.StandardBracedPanelCount(ColumnHeight),
                layout.BracedPanelCount);
        }

        // ---- 6b. El BOM sale del layout EFECTIVO ---------------------------------------------------------

        [Fact]
        public void ElBOMCuentaSeparadoresPorFRONTERASYTensoresPorTRAMOSArriostrados()
        {
            // La comprobacion de que el BOM no tiene su propia idea de la secuencia: sale del layout efectivo,
            // asi que apagar los tensores de un tramo tiene que quitar SUS dos tensores y ninguno mas, y no
            // tocar los separadores, que son fronteras y siguen ahi.
            var design = CantileverRoundTwoCharacterizationTests.Reference();
            var automatic = new CantileverLineEditorAssembler(Catalog).Build(design);

            var layout = automatic.Line.Intervals[0].Layout;
            var editor = new CantileverPanelLayoutEditorState(
                CantileverPanelLayoutMode.Automatic, Array.Empty<CantileverPanelSegmentDesign>());

            Assert.True(editor.MaterializeAutomatic(layout.EffectiveSegments).Applied);

            // Apagamos el PRIMER tramo arriostrado y dejamos todo lo demas igual.
            var first = editor.Segments
                .Select((seg, i) => (seg, i))
                .First(x => x.seg.BracingMode == CantileverPanelBracingMode.CrossBraced).i;

            Assert.True(editor.ToggleBracing(first).Applied);
            editor.ApplyTo(design.Bracing);

            var advanced = new CantileverLineEditorAssembler(Catalog).Build(design);

            Assert.False(advanced.Line.IsBlocked);

            var antes = automatic.Line.Intervals.SelectMany(i => i.Braces).Count();
            var ahora = advanced.Line.Intervals.SelectMany(i => i.Braces).Count();

            // Un tramo apagado son DOS tensores menos POR TRAMO de la linea.
            Assert.Equal(antes - (2 * automatic.Line.Intervals.Count), ahora);

            // Y los separadores NO cambian: las fronteras siguen siendo las mismas.
            Assert.Equal(
                automatic.Line.Intervals.SelectMany(i => i.Separators).Count(),
                advanced.Line.Intervals.SelectMany(i => i.Separators).Count());
        }

        // ---- 7. La copia profunda ------------------------------------------------------------------------

        [Fact]
        public void LaCopiaProfundaNOComparteLosTramos()
        {
            var bracing = Advanced(Braced(20.0, 60.0));
            var copy = bracing.DeepCopy();

            copy.AdvancedPanelSegments[0].EndElevation = 999.0;

            Assert.Equal(60.0, bracing.AdvancedPanelSegments[0].EndElevation, 9);
            Assert.Equal(CantileverPanelLayoutMode.Advanced, copy.PanelLayoutMode);
        }
    }
}
