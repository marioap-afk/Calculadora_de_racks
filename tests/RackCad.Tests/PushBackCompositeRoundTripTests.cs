using System;
using System.Linq;
using System.Text.Json;
using RackCad.Application.Bom;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (G7) — ROUND TRIP y regresion integral: save/load, biblioteca, el descriptor de vista que usan RACKEDITAR y
    /// RACKDUPLICAR, campos desconocidos, BOM y la supervivencia de I-40 e I-41 dentro de un rack compuesto.
    /// </summary>
    public class PushBackCompositeRoundTripTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackDesign Composite(double gap = 8.0, bool centralSeparator = true)
        {
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: 2, slotsB: 2, deepA: 5, deepB: 4, levelsA: 3, levelsB: 2,
                gap: gap, centralSeparator: centralSeparator);
            design.Composite.SetCell(0, 0, PushBackCellTopology.Corrida, PushBackRunDirection.AToB);
            design.Composite.SetCell(1, 1, PushBackCellTopology.SoloB, PushBackRunDirection.AToB);
            design.SideB.FrontConfigs[0].PalletsDeepOverrides.Add(3);
            design.SideB.FrontConfigs[0].DrawPallets.Add(true);
            design.SideB.RearTope.Disable(1, 0);
            design.RearTope.Disable(0, 2);
            return design;
        }

        private static PushBackDesign RoundTrip(PushBackDesign design)
        {
            var json = JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(design));
            return JsonSerializer.Deserialize<PushBackDesignDocument>(json).ToDomain();
        }

        // ---- Save / load: el sistema resuelto es el MISMO ------------------------------------------------------

        [Fact]
        public void ACompositeRack_ResolvesIdentically_AfterASaveAndLoad()
        {
            var design = Composite();
            var before = new PushBackResolver(Catalog).Resolve(design);
            var after = new PushBackResolver(Catalog).Resolve(RoundTrip(design));

            Assert.Equal(before.Structure.TotalLength, after.Structure.TotalLength, 6);
            Assert.Equal(before.Structure.Modules.Count, after.Structure.Modules.Count);
            Assert.Equal(before.Structure.Fronts.Count, after.Structure.Fronts.Count);
            Assert.Equal(before.Composite.Gap, after.Composite.Gap, 6);
            Assert.Equal(before.Composite.CentralSeparator, after.Composite.CentralSeparator);
            Assert.Equal(before.Composite.SideA.EffectiveStructure, after.Composite.SideA.EffectiveStructure);
            Assert.Equal(before.Composite.SideB.EffectiveStructure, after.Composite.SideB.EffectiveStructure);
        }

        [Fact]
        public void TheRuns_SurviveASaveAndLoad_WithTheirTopologyAndDirection()
        {
            var design = Composite();
            var before = PushBackRuns.Resolve(new PushBackResolver(Catalog).Resolve(design));
            var after = PushBackRuns.Resolve(new PushBackResolver(Catalog).Resolve(RoundTrip(design)));

            Assert.Equal(before.Runs.Count, after.Runs.Count);
            foreach (var pair in before.Runs.Zip(after.Runs, (first, second) => (first, second)))
            {
                Assert.Equal(pair.first.Slot, pair.second.Slot);
                Assert.Equal(pair.first.Level, pair.second.Level);
                Assert.Equal(pair.first.Topology, pair.second.Topology);
                Assert.Equal(pair.first.LowSide, pair.second.LowSide);
                Assert.Equal(pair.first.HighSide, pair.second.HighSide);
                Assert.Equal(pair.first.Reflected, pair.second.Reflected);
            }
        }

        [Fact]
        public void TheBom_IsIdentical_AfterASaveAndLoad()
        {
            var design = Composite();
            string Signature(BillOfMaterials bom) => string.Join(
                ";",
                bom.Components
                    .OrderBy(component => component.Category, StringComparer.Ordinal)
                    .ThenBy(component => component.ProfileId, StringComparer.Ordinal)
                    .ThenBy(component => component.Length)
                    .Select(component => component.Category + "|" + component.ProfileId + "|"
                        + Math.Round(component.Length, 4) + "|" + component.Quantity));

            var before = Signature(PushBackBomBuilder.Build(new PushBackResolver(Catalog).Resolve(design), Catalog));
            var after = Signature(PushBackBomBuilder.Build(new PushBackResolver(Catalog).Resolve(RoundTrip(design)), Catalog));

            Assert.Equal(before, after);
        }

        // ---- Biblioteca / proyecto -----------------------------------------------------------------------------

        [Fact]
        public void ACompositeRack_SurvivesTheProjectDocument()
        {
            var design = Composite();
            var project = RackProject.ForPushBack(design);

            var document = new RackProjectDocument { PushBack = PushBackDesignDocument.FromDomain(project.PushBackDesign) };
            var json = JsonSerializer.Serialize(document);
            var restored = JsonSerializer.Deserialize<RackProjectDocument>(json).PushBack.ToDomain();

            Assert.True(restored.IsComposite);
            Assert.Equal(design.Composite.Gap, restored.Composite.Gap, 6);
            Assert.Equal(design.SideB.Fronts.Count, restored.SideB.Fronts.Count);
        }

        [Fact]
        public void UnknownFields_SurviveACompositeSaveAndLoad()
        {
            var design = Composite();
            var json = JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(design));
            var withExtra = json.Insert(1, "\"CampoDeUnaVersionFutura\":[1,2,3],");

            var loaded = JsonSerializer.Deserialize<PushBackDesignDocument>(withExtra);
            var rewritten = JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(loaded.ToDomain(), loaded));

            Assert.Contains("CampoDeUnaVersionFutura", rewritten);
            Assert.True(loaded.ToDomain().IsComposite);
        }

        // ---- El descriptor de vista de RACKEDITAR / RACKDUPLICAR ------------------------------------------------

        [Fact]
        public void TheFrontalSectionDescriptor_IsAdditive_AndLegacyValuesStillPointAtSideA()
        {
            // 0 y 1 son EXACTAMENTE lo que escribieron todas las versiones anteriores.
            Assert.Equal(0, PushBackSystemFrontalBuilder.EncodeSection(PushBackFrontalEnd.EntradaSalida, PushBackSide.A));
            Assert.Equal(1, PushBackSystemFrontalBuilder.EncodeSection(PushBackFrontalEnd.Posterior, PushBackSide.A));
            Assert.Equal(2, PushBackSystemFrontalBuilder.EncodeSection(PushBackFrontalEnd.EntradaSalida, PushBackSide.B));
            Assert.Equal(3, PushBackSystemFrontalBuilder.EncodeSection(PushBackFrontalEnd.Posterior, PushBackSide.B));

            Assert.Equal((PushBackFrontalEnd.EntradaSalida, PushBackSide.A), PushBackSystemFrontalBuilder.DecodeSection(0));
            Assert.Equal((PushBackFrontalEnd.Posterior, PushBackSide.A), PushBackSystemFrontalBuilder.DecodeSection(1));
            Assert.Equal((PushBackFrontalEnd.EntradaSalida, PushBackSide.B), PushBackSystemFrontalBuilder.DecodeSection(2));
            Assert.Equal((PushBackFrontalEnd.Posterior, PushBackSide.B), PushBackSystemFrontalBuilder.DecodeSection(3));

            Assert.True(PushBackSystemFrontalBuilder.IsValidSection(3));
            Assert.False(PushBackSystemFrontalBuilder.IsValidSection(4));
            Assert.False(PushBackSystemFrontalBuilder.IsValidSection(-1));
        }

        [Fact]
        public void TheFourFrontalSections_AllProduceAPlan()
        {
            var system = new PushBackResolver(Catalog).Resolve(Composite());
            var builder = new PushBackSystemFrontalBuilder();

            for (var section = 0; section <= 3; section++)
            {
                var (end, side) = PushBackSystemFrontalBuilder.DecodeSection(section);
                var plan = builder.BuildPlan(system, Catalog, end, side).Flatten().Instances;
                Assert.NotEmpty(plan);
            }
        }

        // ---- I-40 dentro de un rack compuesto -------------------------------------------------------------------

        [Fact]
        public void ModuleIdentity_SurvivesASaveAndLoad_ForBothSides()
        {
            var design = Composite();
            var before = new PushBackResolver(Catalog).Resolve(design);
            var storedIds = before.Structure.Modules.Select(module => module.ModuleId).ToList();

            // El diseno vuelve a persistirse con la secuencia COMPUESTA, y al recargar cada lado recupera la suya.
            var snapshot = new PushBackResolver(Catalog).Snapshot(before);
            snapshot.SideB = design.SideB;
            snapshot.Composite = design.Composite;
            var after = new PushBackResolver(Catalog).Resolve(RoundTrip(snapshot));

            Assert.Equal(storedIds.Count, after.Structure.Modules.Count);
            Assert.Contains(after.Structure.Modules, module => module.ModuleId == PushBackCompositeStructure.GapModuleId);
            Assert.Contains(
                after.Structure.Modules,
                module => module.ModuleId != null
                    && module.ModuleId.StartsWith(PushBackCompositeStructure.SideBModulePrefix, StringComparison.Ordinal));
        }

        [Fact]
        public void ACustomHeaderConfiguration_SurvivesTheComposition()
        {
            var design = Composite(gap: 0.0, centralSeparator: false);
            var first = new PushBackResolver(Catalog).Resolve(design);

            // Se personaliza una cabecera del lado A y se vuelve a resolver desde la secuencia COMPUESTA almacenada.
            var snapshot = new PushBackResolver(Catalog).Snapshot(first);
            var header = snapshot.Structure.Modules.First(module => module.IsHeader);
            header.UseCalculatedHeaderConfiguration = false;
            header.HeaderConfiguration = new RackFrameConfiguration { Height = 123.0 };
            var customId = header.ModuleId;
            snapshot.SideB = design.SideB;
            snapshot.Composite = design.Composite;

            var second = new PushBackResolver(Catalog).Resolve(snapshot);
            var survivor = second.Structure.Modules.FirstOrDefault(module => module.ModuleId == customId);

            Assert.NotNull(survivor);
            Assert.False(survivor.UseCalculatedHeaderConfiguration);
            Assert.Equal(123.0, survivor.AssociatedFrameConfiguration.Height, 6);
        }

        [Fact]
        public void TheDerivedPostAndLineOverrides_ReachTheCompositeStructure()
        {
            var design = Composite();
            design.Structure.DerivedPostHeight = 77.0;
            design.Structure.DerivedPostReinforcementHeight = 33.0;

            var system = new PushBackResolver(Catalog).Resolve(design);

            Assert.Equal(77.0, system.Structure.DerivedPostHeight ?? 0.0, 6);
            Assert.Equal(33.0, system.Structure.DerivedPostReinforcementHeight ?? 0.0, 6);
        }

        // ---- I-41 dentro de un rack compuesto -------------------------------------------------------------------

        [Fact]
        public void PerCellDepthAndPallet_AreIndependentPerSide()
        {
            var design = Composite();
            design.Fronts[0].PalletsDeepOverrides.Add(2);       // celda (0,0) del lado A
            design.Fronts[0].DrawPallets.Add(true);

            var system = new PushBackResolver(Catalog).Resolve(design);

            Assert.Equal(2, system.EffectivePalletsDeepAt(PushBackSide.A, 0, 0));
            Assert.True(system.DrawPalletAt(PushBackSide.A, 0, 0));
            // El lado B tiene SU propio override en la misma celda, y no se contamina.
            Assert.Equal(3, system.EffectivePalletsDeepAt(PushBackSide.B, 0, 0));
            Assert.True(system.DrawPalletAt(PushBackSide.B, 0, 0));
        }

        [Fact]
        public void TheFiveScopes_StillWriteOnlyOneProperty_InACompositeRack()
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.SetFrontCount(2);
            state.SetSideBPresent(true);
            state.SideB.SetFrontCount(2);
            state.SetActiveSide(PushBackSide.B);

            var written = state.SideB.ApplyPalletsDeep(4, DynamicRackCellScope.All);
            Assert.True(written > 0);
            // El lado A no se toco: los alcances trabajan DENTRO del lado activo.
            Assert.Null(state.SideA.Cell(0, 0).PalletsDeepOverride);
            Assert.Equal(4, state.SideB.Cell(0, 0).PalletsDeepOverride);
        }

        // ---- El legacy no cambia --------------------------------------------------------------------------------

        [Fact]
        public void ALegacyRack_ResolvesTheSameSystem_BeforeAndAfterTheInitiative()
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new RackCad.Domain.Systems.Shared.PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 6,
                    LoadLevels = 3,
                    FirstLevelHeight = 4.0,
                    BeamDepth = 4.0
                }
            };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign
            {
                PalletCount = 2, LoadLevels = 3, PalletsDeep = 6, DepthStartPosition = 1
            });
            design.Fronts.Add(new PushBackFrontConfig());

            var system = new PushBackResolver(Catalog).Resolve(design);

            Assert.False(system.IsComposite);
            Assert.Null(system.Composite);
            Assert.Empty(PushBackRuns.Resolve(system).Runs);   // un rack de un sentido no pasa por las camas compuestas
            Assert.Equal(6, system.Structure.Modules.Count);
            Assert.DoesNotContain(system.Structure.Modules, module => module.Kind == DynamicRackModuleKind.Gap);
        }
    }
}
