using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
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
    /// I-41 (PB-015 / PB-016) — CARACTERIZACION previa. Fija las autoridades que I-41 encuentra al abrirse y que la
    /// iniciativa NO puede cambiar:
    /// <list type="bullet">
    /// <item>el fondo de una celda lo decide HOY el frente entero: un frente con N niveles produce N camas de la
    /// misma longitud y N largueros posteriores en la MISMA X;</item>
    /// <item>Push Back no dibuja ninguna tarima en ninguna de sus cuatro vistas;</item>
    /// <item>un documento legacy — sin ningun campo de I-41 — carga y se comporta exactamente igual;</item>
    /// <item>las autoridades de I-40 (ModuleId, cabeceras por linea, altura del poste derivado por linea) sobreviven
    /// a un recalculo que no mueve la envolvente estructural.</item>
    /// </list>
    /// Estas pruebas siguen verdes DESPUES de I-41: son el contrato de no-regresion, no la funcionalidad nueva (esa
    /// vive en PushBackCellDepthTests y PushBackCellPalletTests).
    /// </summary>
    public class PushBackCellConfigurationCharacterizationTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackDesign Design(int palletsDeep = 4, int loadLevels = 3)
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

        // ---- El fondo es HOY una propiedad del FRENTE, nunca de la celda ------------------------------------

        [Fact]
        public void Characterization_EveryLevelOfAFront_SharesOneBedLength()
        {
            var system = new PushBackResolver(Catalog).Resolve(Design(palletsDeep: 5, loadLevels: 3));
            var front = system.Structure.Fronts[0];

            var axes = PushBackFlowBedGeometry.Resolve(system, Catalog, front);

            Assert.Equal(3, axes.Count);
            var length = PushBackFlowBedLateralBuilder.ResolveBedLength(system, front);
            Assert.True(length > 0.0);
            // La longitud es UNA sola: la del frente completo. Ningun nivel puede pedir otra.
            Assert.Equal(front.EndX - front.StartX, length, 6);
        }

        [Fact]
        public void Characterization_EveryRearBeamOfAFront_SitsOnTheSameX()
        {
            var system = new PushBackResolver(Catalog).Resolve(Design(palletsDeep: 5, loadLevels: 3));
            var front = system.Structure.Fronts[0];

            var rear = PushBackLoadBeamGeometry.HighBeams(system, Catalog, 0, front);

            Assert.Equal(3, rear.Count);
            Assert.Single(rear.Select(beam => System.Math.Round(beam.Insertion.X, 6)).Distinct());
            Assert.Equal(front.EndX, rear[0].Insertion.X, 6);
        }

        [Fact]
        public void Characterization_TheFrontStructuralPalletsDeep_IsWhatTheDesignAsked()
        {
            var design = Design(palletsDeep: 6, loadLevels: 2);
            design.Structure.Fronts.Add(new DynamicRackFrontDesign
            {
                PalletCount = 1,
                LoadLevels = 2,
                PalletsDeep = 6,
                DepthStartPosition = 1
            });

            var system = new PushBackResolver(Catalog).Resolve(design);

            Assert.Equal(6, system.Structure.Fronts[0].PalletsDeep);
        }

        // ---- Push Back no dibuja tarimas hoy ----------------------------------------------------------------

        [Fact]
        public void Characterization_NoPushBackViewDrawsAPallet()
        {
            var system = new PushBackResolver(Catalog).Resolve(Design());
            var catalog = Catalog;

            var lateral = new PushBackSystemLateralBuilder().Build(system, catalog);
            var baja = new PushBackSystemFrontalBuilder().BuildPlan(system, catalog, PushBackFrontalEnd.EntradaSalida);
            var posterior = new PushBackSystemFrontalBuilder().BuildPlan(system, catalog, PushBackFrontalEnd.Posterior);
            var planta = new PushBackSystemPlantaBuilder().BuildPlan(system, catalog);

            foreach (var plan in new[] { lateral, baja, posterior, planta })
            {
                Assert.DoesNotContain(plan.Flatten().Instances, i => i.Role == HeaderBlockRole.Pallet);
            }
        }

        [Fact]
        public void Characterization_TheBom_NeverCarriesAPalletComponent()
        {
            var bom = PushBackBomBuilder.Build(new PushBackResolver(Catalog).Resolve(Design()), Catalog);

            Assert.DoesNotContain(bom.Components, c => c.ProfileId != null && c.ProfileId.Contains("TARIMA"));
        }

        // ---- Legacy: un documento sin ningun campo de I-41 --------------------------------------------------

        [Fact]
        public void Characterization_ALegacyDocument_RoundTripsWithoutAnyCellDepthOrPalletField()
        {
            var design = Design(palletsDeep: 4, loadLevels: 2);
            var document = PushBackDesignDocument.FromDomain(design);
            var json = System.Text.Json.JsonSerializer.Serialize(document);

            Assert.DoesNotContain("PalletsDeepOverrides", json);
            Assert.DoesNotContain("DrawPallets", json);

            var reloaded = System.Text.Json.JsonSerializer
                .Deserialize<PushBackDesignDocument>(json).ToDomain();
            var system = new PushBackResolver(Catalog).Resolve(reloaded);

            Assert.Equal(4, system.Structure.Fronts[0].PalletsDeep);
            Assert.DoesNotContain(
                new PushBackSystemLateralBuilder().Build(system, Catalog).Flatten().Instances,
                i => i.Role == HeaderBlockRole.Pallet);
        }

        // ---- I-40: la estructura modular sobrevive a un recalculo que no mueve la envolvente ----------------

        [Fact]
        public void Characterization_ARecomputeThatKeepsTheEnvelope_PreservesModuleIdsAndLineOverrides()
        {
            var catalog = Catalog;
            var assembler = new PushBackEditorDesignAssembler(catalog);
            var state = new PushBackEditorState();
            var inputs = state.LoadNew();

            var first = assembler.Build(state, inputs);
            Assert.True(first.IsValid, first.Error);
            assembler.AcceptComputation(state, first);
            var moduleIds = first.System.Structure.Modules.Select(m => m.ModuleId).ToList();

            // Un recalculo disparado por algo que NO mueve la envolvente conserva los mismos modulos.
            var second = assembler.Build(state, inputs);
            Assert.True(second.IsValid, second.Error);

            Assert.Equal(moduleIds, second.System.Structure.Modules.Select(m => m.ModuleId).ToList());
        }
    }
}
