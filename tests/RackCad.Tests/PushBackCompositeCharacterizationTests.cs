using System.Linq;
using System.Text.Json;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 — CARACTERIZACION previa del Push Back de UN SOLO SENTIDO. Fija, ANTES de tocar ninguna autoridad, el
    /// contrato completo que la iniciativa NO puede romper:
    /// <list type="bullet">
    /// <item>un Push Back tiene UN solo sentido de flujo: el extremo BAJO en el arranque del frente y el ALTO hacia
    /// +X, en TODOS los niveles de TODOS los frentes;</item>
    /// <item>la profundidad fisica es UNA sola secuencia de modulos sin hueco: no hay gap ni separador central;</item>
    /// <item>el BOM cuenta una cama por calle x nivel, un larguero bajo y uno alto por celda y un tope por celda
    /// activa — sin ninguna nocion de lado;</item>
    /// <item>el tope posterior es UNA rejilla (frente, nivel), no dos;</item>
    /// <item>un documento sin campos de I-42 se re-escribe IDENTICO.</item>
    /// </list>
    /// Estas pruebas deben seguir verdes despues de I-42: son el contrato de no-regresion del legacy, no la
    /// funcionalidad nueva.
    /// </summary>
    public class PushBackCompositeCharacterizationTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackDesign Design(int palletsDeep = 5, int loadLevels = 3)
            => new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = palletsDeep,
                    LoadLevels = loadLevels,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            };

        // ---- Un solo sentido de flujo ---------------------------------------------------------------------

        [Fact]
        public void Characterization_EveryBedFlows_FromTheFrontStart_TowardIncreasingX()
        {
            var system = new PushBackResolver(Catalog).Resolve(Design());
            var front = system.Structure.Fronts[0];

            var axes = PushBackFlowBedGeometry.Resolve(system, Catalog, front);

            Assert.Equal(3, axes.Count);
            foreach (var axis in axes)
            {
                // El extremo bajo esta SIEMPRE antes que el alto: no existe hoy una cama que fluya al reves.
                Assert.True(axis.ExitMate.X < axis.HighMate.X);
                Assert.True(axis.HighMate.Y > axis.ExitMate.Y);
                Assert.True(axis.RotationRadians > 0.0);
            }
        }

        [Fact]
        public void Characterization_EveryCell_HasExactlyOneLowBeamAndOneHighBeam()
        {
            var system = new PushBackResolver(Catalog).Resolve(Design());
            var front = system.Structure.Fronts[0];

            var low = PushBackLoadBeamGeometry.LowBeams(system, Catalog, front);
            var high = PushBackLoadBeamGeometry.HighBeams(system, Catalog, 0, front);

            Assert.Equal(3, low.Count);
            Assert.Equal(3, high.Count);
            // El bajo esta en el arranque del frente en TODOS los niveles; el alto, mas alla.
            Assert.All(low, beam => Assert.Equal(front.StartX, beam.Insertion.X, 6));
            Assert.All(high, beam => Assert.True(beam.Insertion.X > front.StartX));
        }

        // ---- Una sola secuencia de modulos, sin hueco ------------------------------------------------------

        [Fact]
        public void Characterization_TheDepthRun_IsContiguous_WithNoGap()
        {
            var system = new PushBackResolver(Catalog).Resolve(Design(palletsDeep: 5));
            var modules = system.Structure.Modules.ToList();

            Assert.NotEmpty(modules);
            for (var index = 1; index < modules.Count; index++)
            {
                // Cada modulo arranca exactamente donde acaba el anterior: no hay hueco fisico en la profundidad.
                Assert.Equal(modules[index - 1].EndX, modules[index].StartX, 9);
            }

            Assert.Equal(0.0, modules[0].StartX, 9);
            Assert.Equal(system.Structure.TotalLength, modules[modules.Count - 1].EndX, 9);
            Assert.Equal(system.Structure.TotalLength, system.Structure.Fronts.Max(front => front.EndX), 9);
        }

        // ---- El BOM no sabe de lados ----------------------------------------------------------------------

        [Fact]
        public void Characterization_TheBom_CountsOneBedPerLaneAndLevel_AndOneBeamPairPerCell()
        {
            var design = Design(palletsDeep: 4, loadLevels: 2);
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 2, LoadLevels = 2, PalletsDeep = 4 });
            var system = new PushBackResolver(Catalog).Resolve(design);

            var bom = PushBackBomBuilder.Build(system, Catalog);

            Assert.Equal(2 * 2, Quantity(bom, SystemBomBuilder.Cama));           // 2 calles x 2 niveles
            Assert.Equal(2, Quantity(bom, SystemBomBuilder.InOutBeam));          // 1 bajo por celda
            Assert.Equal(2, Quantity(bom, PushBackBomBuilder.HighEndBeam));      // 1 alto por celda
            Assert.Equal(2, Quantity(bom, PushBackBomBuilder.RearTope));         // 1 tope por celda activa
        }

        // ---- Un solo tope por celda, en UNA rejilla --------------------------------------------------------

        [Fact]
        public void Characterization_TheRearTope_IsOneGridOfFrontByLevel()
        {
            var design = Design(loadLevels: 2);
            design.RearTope.Disable(0, 1);
            var system = new PushBackResolver(Catalog).Resolve(design);

            Assert.True(system.RearTope.At(0, 0));
            Assert.False(system.RearTope.At(0, 1));
            Assert.Equal(1, Quantity(PushBackBomBuilder.Build(system, Catalog), PushBackBomBuilder.RearTope));
        }

        // ---- Persistencia: un documento sin campos de I-42 se re-escribe identico --------------------------

        [Fact]
        public void Characterization_ALegacyDocument_RoundTripsByteIdentical()
        {
            var design = Design(palletsDeep: 4, loadLevels: 2);
            design.Structure.Fronts.Add(new DynamicRackFrontDesign
            {
                PalletCount = 1, LoadLevels = 2, PalletsDeep = 4, DepthStartPosition = 1
            });
            var first = JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(design));

            var reloaded = JsonSerializer.Deserialize<PushBackDesignDocument>(first);
            var second = JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(reloaded.ToDomain(), reloaded));

            Assert.Equal(first, second);
            Assert.DoesNotContain("\"SideB\"", first);
            Assert.DoesNotContain("\"Composite\"", first);
        }

        private static int Quantity(BillOfMaterials bom, string category)
            => bom.Components.Where(component => component.Category == category).Sum(component => component.Quantity);
    }
}
