using System;
using System.Linq;
using System.Text.Json;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.PushBack;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (ronda 4) — PRUEBAS VINCULANTES del fondo de la CAMA CORRIDA como AUTORIDAD PROPIA y de la relacion
    /// <c>Required &lt;= Resolved &lt;= Available</c>.
    ///
    /// <para>
    /// Lo que se consagra aqui, y que ninguna prueba posterior puede contradecir:
    /// <list type="number">
    /// <item>La demanda de una corrida es SUYA. No es la suma de los fondos de A y de B, ni el fondo de ninguno de
    /// los dos lados.</item>
    /// <item>Los fondos de A y de B sobreviven DORMANTES a un cambio de topologia: la operacion es reversible.</item>
    /// <item>La cama apoya en un soporte fisico REAL —el primero valido desde su ancla ALTA hacia el bajo—, nunca en
    /// un punto flotante obtenido restando una longitud continua.</item>
    /// </list>
    /// </para>
    /// </summary>
    public class PushBackCompositeCorridaAuthorityTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackDesign Design(int deepA = 5, int deepB = 8, int levels = 1, double gap = 0.0)
        {
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: 1, slotsB: 1, deepA: deepA, deepB: deepB, levelsA: levels, levelsB: levels, gap: gap);
            design.Composite.DefaultTopology = PushBackCellTopology.Corrida;
            design.Composite.DefaultDirection = PushBackRunDirection.AToB;
            return design;
        }

        private static PushBackSystem Resolve(PushBackDesign design) => new PushBackResolver(Catalog).Resolve(design);

        // ---- R1: la demanda de la corrida es su propia autoridad -----------------------------------------------

        /// <summary>
        /// R1 — el caso del Owner: estructura 5 + 8, «Fondo corrida = 10». La demanda es 10. Ni 13 (la suma), ni 5,
        /// ni 8. Y las dos estructuras siguen siendo 5 y 8.
        /// </summary>
        [Fact]
        public void TheCorridaDepth_IsItsOwnAuthority_NotTheSumOfBothSides()
        {
            var design = Design(deepA: 5, deepB: 8);
            design.Composite.SetCorridaDepth(0, 0, 10);
            var system = Resolve(design);

            var bed = system.Composite.Cell(0, 1).Beds.Single();
            Assert.Equal(10, bed.DemandPositions);
            Assert.NotEqual(13, bed.DemandPositions);
            Assert.Equal(5, system.Composite.SideA.EffectiveStructure);
            Assert.Equal(8, system.Composite.SideB.EffectiveStructure);
        }

        /// <summary>
        /// R2 — sin fondo propio, la corrida hereda un default DERIVADO: la capacidad de la estructura, es decir «la
        /// calle atraviesa el rack». Es un default, no una autoridad: en cuanto se escribe un fondo propio, manda ese.
        /// </summary>
        [Fact]
        public void WithoutItsOwnDepth_ACorridaCrossesTheWholeRack()
        {
            var system = Resolve(Design(deepA: 5, deepB: 8));
            var bed = system.Composite.Cell(0, 1).Beds.Single();

            Assert.Equal(13, bed.DemandPositions);
            Assert.Equal(system.Structure.TotalLength, bed.ResolvedBedLength, 6);
        }

        // ---- R3: la configuracion de los lados sobrevive DORMANTE ----------------------------------------------

        /// <summary>
        /// R3 — cambiar la topologia es REVERSIBLE. Poner una celda en corrida (con su fondo propio) y devolverla a
        /// encontradas restituye EXACTAMENTE las dos camas por lado que habia antes.
        /// </summary>
        [Fact]
        public void TurningACellIntoACorridaAndBack_RestoresBothSideBeds()
        {
            var design = Design(deepA: 5, deepB: 8);
            design.Composite.DefaultTopology = PushBackCellTopology.Encontradas;
            var before = Resolve(design).Composite.Cell(0, 1);
            var beforeA = before.BedFrom(PushBackSide.A).RequiredBedLength;
            var beforeB = before.BedFrom(PushBackSide.B).RequiredBedLength;

            design.Composite.SetCell(0, 0, PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            design.Composite.SetCorridaDepth(0, 0, 10);
            var corrida = Resolve(design).Composite.Cell(0, 1);
            Assert.Single(corrida.Beds);
            Assert.Equal(10, corrida.Beds[0].DemandPositions);

            design.Composite.SetCell(0, 0, PushBackCellTopology.Encontradas, PushBackRunDirection.AToB);
            var after = Resolve(design).Composite.Cell(0, 1);

            Assert.Equal(2, after.Beds.Count);
            Assert.Equal(beforeA, after.BedFrom(PushBackSide.A).RequiredBedLength, 6);
            Assert.Equal(beforeB, after.BedFrom(PushBackSide.B).RequiredBedLength, 6);

            // Y el fondo de la corrida sigue guardado: volver a corrida no obliga a re-escribirlo.
            Assert.Equal(10, design.Composite.CorridaDepthAt(0, 0));
        }

        /// <summary>R4 — persistencia ADITIVA: el fondo de la corrida viaja, y un documento sin el se lee igual.</summary>
        [Fact]
        public void TheCorridaDepth_SurvivesASaveAndLoad_AndIsAbsentWhenUnused()
        {
            var design = Design(deepA: 5, deepB: 8);
            design.Composite.SetCorridaDepth(0, 0, 10);

            var json = JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(design));
            var restored = JsonSerializer.Deserialize<PushBackDesignDocument>(json).ToDomain();

            Assert.Equal(10, restored.Composite.CorridaDepthAt(0, 0));
            Assert.Equal(
                Resolve(design).Composite.Cell(0, 1).Beds.Single().ResolvedBedLength,
                Resolve(restored).Composite.Cell(0, 1).Beds.Single().ResolvedBedLength,
                6);

            // Un rack que nunca uso la autoridad no escribe el campo: el archivo legacy no cambia.
            var plain = JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(Design(deepA: 5, deepB: 8)));
            Assert.DoesNotContain("CorridaDepth", plain, StringComparison.OrdinalIgnoreCase);
        }

        // ---- R5: Required <= Resolved <= Available -------------------------------------------------------------

        /// <summary>
        /// R5 — la cama toma el PRIMER apoyo valido desde el extremo BAJO hacia el alto. No mide el minimo teorico si
        /// ese punto no es un apoyo, y no se estira hasta todo lo disponible si con menos basta.
        /// </summary>
        [Theory]
        [InlineData(2)]
        [InlineData(5)]
        [InlineData(10)]
        [InlineData(13)]
        public void TheBed_TakesTheFirstValidSupport_FromLowTowardsHigh(int depth)
        {
            var design = Design(deepA: 5, deepB: 8);
            design.Composite.SetCorridaDepth(0, 0, depth);
            var system = Resolve(design);
            var bed = system.Composite.Cell(0, 1).Beds.Single();

            Assert.True(
                bed.RequiredBedLength <= bed.ResolvedBedLength + PushBackBedSpan.Tolerance,
                "Resolved nunca puede quedarse corto frente a la demanda");
            Assert.True(
                bed.ResolvedBedLength <= bed.AvailableBedSpan + PushBackBedSpan.Tolerance,
                "Resolved nunca puede exceder lo que la estructura ofrece");

            // El extremo ALTO cae sobre una linea de modulo: un apoyo fisico real. El BAJO esta en la orilla.
            var lowX = system.Structure.Modules[0].StartX;
            var highX = lowX + bed.ResolvedBedLength;
            Assert.Contains(system.Structure.Modules, module => Math.Abs(module.EndX - highX) < 1e-6);

            // Y es el PRIMERO que sirve: el apoyo inmediatamente anterior ya no alcanzaria.
            var nearer = system.Structure.Modules
                .Where(module => module.EndX < highX - 1e-6)
                .Select(module => module.EndX - lowX)
                .DefaultIfEmpty(-1.0)
                .Max();
            if (nearer > 0.0)
            {
                Assert.True(
                    nearer < bed.RequiredBedLength - PushBackBedSpan.Tolerance,
                    "si un apoyo mas cercano bastaba, la cama tenia que haberlo tomado");
            }
        }

        // ---- R6: el bug visual — la corrida NO arranca en el segundo fondo -------------------------------------

        /// <summary>
        /// R6 — BUG confirmado por el Owner: la cama corrida arrancaba en el SEGUNDO fondo. La causa era una DOBLE
        /// autoridad de colocacion (primero un rango discreto, despues un <c>StartX = EndX - longitud</c> continuo).
        /// Con una sola autoridad, una corrida que atraviesa el rack arranca en el PRIMER fondo, con y sin hueco.
        /// </summary>
        [Theory]
        [InlineData(0.0)]
        [InlineData(18.0)]
        public void AFullCorrida_StartsAtTheFirstDepthPosition(double gap)
        {
            var system = Resolve(Design(deepA: 5, deepB: 8, gap: gap));
            var axis = PushBackRunGeometry.Axes(PushBackRuns.Resolve(system), Catalog).Single();
            var bed = system.Composite.Cell(0, 1).Beds.Single();
            var first = system.Structure.Modules[0];

            // La cama ocupa la estructura entera: su apoyo bajo es la PRIMERA linea de modulo.
            Assert.Equal(system.Structure.TotalLength, bed.ResolvedBedLength, 6);

            // Y su contacto bajo cae DENTRO del primer fondo, no del segundo. (No coincide con la linea de modulo:
            // el contacto lo fija el punto de acoplamiento del larguero, y ese desfase es el de siempre.)
            Assert.True(
                axis.LowContact.X < first.EndX,
                "la cama corrida debe arrancar en el PRIMER fondo, no en el segundo");
            Assert.True(axis.LowContact.X >= first.StartX - 1e-6);
        }

        /// <summary>
        /// El hueco no mueve el arranque de una corrida completa: sigue apoyando en la primera linea de modulo. Es la
        /// comprobacion directa de que ya no hay una resta continua colandose despues del rango.
        /// </summary>
        [Fact]
        public void TheGap_DoesNotShiftWhereAFullCorridaStarts()
        {
            double LowContact(double gap)
                => PushBackRunGeometry.Axes(PushBackRuns.Resolve(Resolve(Design(deepA: 5, deepB: 8, gap: gap))), Catalog)
                    .Single().LowContact.X;

            Assert.Equal(LowContact(0.0), LowContact(18.0), 6);
        }

        /// <summary>
        /// La otra mitad del mismo bug: una corrida CORTA tampoco flota. Su extremo bajo cae sobre un apoyo, y su
        /// longitud dibujada es exactamente la resuelta.
        /// </summary>
        [Theory]
        [InlineData(PushBackRunDirection.AToB)]
        [InlineData(PushBackRunDirection.BToA)]
        public void AShortCorrida_LandsOnASupport_AndNeverFloats(PushBackRunDirection direction)
        {
            var design = Design(deepA: 5, deepB: 8);
            design.Composite.DefaultDirection = direction;
            design.Composite.SetCorridaDepth(0, 0, 6);
            var system = Resolve(design);

            var bed = system.Composite.Cell(0, 1).Beds.Single();
            var axis = PushBackRunGeometry.Axes(PushBackRuns.Resolve(system), Catalog).Single();
            var total = system.Structure.TotalLength;

            // El apoyo se comprueba en el MARCO DE LA CAMA, que es donde se resolvio: el del rack si va A->B y el
            // espejado si va B->A. Es exactamente el mismo marco que usa el dibujo.
            var frame = direction == PushBackRunDirection.AToB
                ? system.Structure
                : PushBackMirror.Structure(system.Structure);
            var highInFrame = frame.Modules[0].StartX + bed.ResolvedBedLength;
            var support = frame.Modules.Single(module => Math.Abs(module.EndX - highInFrame) < 1e-6);

            // El contacto ALTO cae DENTRO de ese fondo, no flotando en cualquier punto intermedio.
            var highContactInFrame = direction == PushBackRunDirection.AToB
                ? axis.HighContact.X
                : total - axis.HighContact.X;
            Assert.True(highContactInFrame >= support.StartX - 1e-6);
            Assert.True(highContactInFrame <= support.EndX + 1e-6);
            Assert.True(bed.ResolvedBedLength < total - 1.0);

            // Y lo DIBUJADO mide lo resuelto: el BOM y el dibujo no pueden divergir por el sentido.
            Assert.True(Math.Abs(axis.Length - bed.ResolvedBedLength) < 6.0);
        }

        /// <summary>
        /// HALLAZGO de la auditoria de esta ronda: la longitud de la cama se resolvia SIEMPRE en el marco del rack,
        /// mientras que el dibujo la resolvia en el marco de la cama. En sentido B-&gt;A esos dos marcos no coinciden
        /// —la secuencia de fondos no es simetrica— y el BOM cotizaba una longitud distinta de la dibujada.
        /// </summary>
        [Theory]
        [InlineData(PushBackRunDirection.AToB)]
        [InlineData(PushBackRunDirection.BToA)]
        public void TheResolvedLength_MatchesTheDrawnBed_InBothDirections(PushBackRunDirection direction)
        {
            foreach (var depth in new[] { 4, 6, 9, 13 })
            {
                var design = Design(deepA: 5, deepB: 8);
                design.Composite.DefaultDirection = direction;
                design.Composite.SetCorridaDepth(0, 0, depth);
                var system = Resolve(design);

                var bed = system.Composite.Cell(0, 1).Beds.Single();
                var bom = PushBackBomBuilder.Build(system, Catalog).Components
                    .Single(component => component.Category == SystemBomBuilder.Cama);

                // El BOM cotiza EXACTAMENTE la longitud resuelta, sea cual sea el sentido.
                Assert.Equal(bed.ResolvedBedLength, bom.Length, 4);
            }
        }

        // ---- R7: la planta dibuja intermedios -------------------------------------------------------------------

        [Fact]
        public void ThePlanta_OfACorrida_DrawsIntermediateBeams()
        {
            var design = Design(deepA: 5, deepB: 8, levels: 2);
            design.Composite.SetCorridaDepth(0, 0, 10);
            var system = Resolve(design);

            var intermediates = new PushBackSystemPlantaBuilder().Build(system, Catalog)
                .Where(PushBackPlanComposer.IsDynamicIntermediate)
                .ToList();

            Assert.NotEmpty(intermediates);
        }

        // ---- R8: el BOM cotiza la longitud RESUELTA ------------------------------------------------------------

        [Fact]
        public void TheBom_QuotesTheResolvedBedLength()
        {
            foreach (var depth in new[] { 4, 7, 13 })
            {
                var design = Design(deepA: 5, deepB: 8);
                design.Composite.SetCorridaDepth(0, 0, depth);
                var system = Resolve(design);

                var bed = PushBackBomBuilder.Build(system, Catalog).Components
                    .Single(component => component.Category == SystemBomBuilder.Cama);

                Assert.Equal(system.Composite.Cell(0, 1).Beds.Single().ResolvedBedLength, bed.Length, 4);
            }
        }

        // ---- El editor escribe la autoridad correcta -----------------------------------------------------------

        /// <summary>
        /// El campo de fondo del editor, cuando la celda es corrida, escribe la autoridad de la CORRIDA — no los
        /// fondos de A ni los de B — y respeta los CINCO alcances de siempre.
        /// </summary>
        [Fact]
        public void TheEditor_WritesTheCorridaAuthority_AcrossTheFiveScopes()
        {
            var state = new PushBackCompositeEditorState();
            state.SetSideBPresent(true);
            state.SetDefaults(PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            state.SideA.SetFrontCount(2);
            state.SideB.SetFrontCount(2);
            state.SideA.Structure.AdjustLevels(0, 2 - state.SideA.Structure.Fronts[0].LoadLevels);
            state.SideA.Structure.AdjustLevels(1, 2 - state.SideA.Structure.Fronts[1].LoadLevels);
            state.SideA.Structure.ToggleCell(0, 0, extendSelection: false);

            Assert.Equal(1, state.ApplyCorridaDepth(9, DynamicRackCellScope.Cell));
            Assert.Equal(9, state.CorridaDepthAt(0, 0));
            Assert.Null(state.CorridaDepthAt(0, 1));

            var all = state.ApplyCorridaDepth(11, DynamicRackCellScope.All);
            Assert.True(all >= 4);
            Assert.Equal(11, state.CorridaDepthAt(1, 1));

            // Restaurar = quitar la autoridad: la celda vuelve al default derivado.
            state.ApplyCorridaDepth(null, DynamicRackCellScope.All);
            Assert.Null(state.CorridaDepthAt(0, 0));
        }

        /// <summary>Un alcance sin ninguna corrida no escribe nada: el valor no significaria nada ahi.</summary>
        [Fact]
        public void TheEditor_WritesNothing_WhereThereIsNoCorrida()
        {
            var state = new PushBackCompositeEditorState();
            state.SetSideBPresent(true);
            state.SetDefaults(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB);
            state.SideA.SetFrontCount(2);
            state.SideB.SetFrontCount(2);
            state.SideA.Structure.AdjustLevels(0, 2 - state.SideA.Structure.Fronts[0].LoadLevels);
            state.SideA.Structure.AdjustLevels(1, 2 - state.SideA.Structure.Fronts[1].LoadLevels);

            Assert.Equal(0, state.ApplyCorridaDepth(9, DynamicRackCellScope.All));

            // Una sola celda corrida: el alcance MEZCLA, y solo esa se escribe.
            state.SetCell(0, 0, PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            Assert.Equal(1, state.ApplyCorridaDepth(9, DynamicRackCellScope.All));
            Assert.True(state.ScopeMixesCorrida(DynamicRackCellScope.All));
        }
    }
}
