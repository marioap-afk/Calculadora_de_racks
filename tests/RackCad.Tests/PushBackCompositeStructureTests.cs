using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-42 (G2) — la ESTRUCTURA FISICA UNICA: estructura propuesta y efectiva por lado, override manual y
    /// restauracion, interfaz central con su gap y su separador, y la ausencia de duplicidad entre los dos lados.
    /// </summary>
    public class PushBackCompositeStructureTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        internal static PushBackDesign Composite(
            int slotsA = 2, int slotsB = 2, int deepA = 5, int deepB = 4, int levelsA = 3, int levelsB = 2,
            double gap = 0.0, bool centralSeparator = false)
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = deepA,
                    LoadLevels = levelsA,
                    FirstLevelHeight = 4.0,
                    BeamDepth = 4.0
                },
                SideB = new PushBackSideDesign { IsPresent = true, LoadLevels = levelsB, FirstLevelHeight = 4.0 },
                Composite = new PushBackCompositeDesign
                {
                    Gap = gap,
                    CentralSeparator = centralSeparator,
                    DefaultTopology = PushBackCellTopology.Encontradas
                }
            };

            var slots = Math.Max(slotsA, slotsB);
            for (var slot = 0; slot < slots; slot++)
            {
                if (slot < slotsA)
                {
                    design.Structure.Fronts.Add(new DynamicRackFrontDesign
                    {
                        PalletCount = 1, LoadLevels = levelsA, PalletsDeep = deepA, DepthStartPosition = 1
                    });
                    design.Fronts.Add(new PushBackFrontConfig { DefaultPalletsDeep = deepA });
                }

                design.SideB.Fronts.Add(slot < slotsB
                    ? new DynamicRackFrontDesign
                    {
                        PalletCount = 1, LoadLevels = levelsB, PalletsDeep = deepB, DepthStartPosition = 1
                    }
                    : null);
                design.SideB.FrontConfigs.Add(slot < slotsB
                    ? new PushBackFrontConfig { DefaultPalletsDeep = deepB }
                    : null);
            }

            return design;
        }

        // ---- Estructura propuesta y efectiva por lado --------------------------------------------------------

        [Fact]
        public void EachSide_DerivesItsOwnProposedStructure_FromItsOwnCellDemand()
        {
            var system = new PushBackResolver(Catalog).Resolve(Composite(deepA: 8, deepB: 5));

            Assert.True(system.IsComposite);
            Assert.Equal(8, system.Composite.SideA.ProposedStructure);
            Assert.Equal(5, system.Composite.SideB.ProposedStructure);
            Assert.Equal(8, system.Composite.SideA.EffectiveStructure);
            Assert.Equal(5, system.Composite.SideB.EffectiveStructure);
            Assert.Null(system.Composite.SideA.StructureOverride);
            Assert.Null(system.Composite.SideB.StructureOverride);
        }

        [Fact]
        public void ACellOverride_RaisesOnlyItsOwnSideProposal()
        {
            var design = Composite(deepA: 8, deepB: 5);
            design.SideB.FrontConfigs[0].PalletsDeepOverrides.Add(7);   // una celda de B pide mas fondo

            var system = new PushBackResolver(Catalog).Resolve(design);

            Assert.Equal(8, system.Composite.SideA.ProposedStructure);
            Assert.Equal(7, system.Composite.SideB.ProposedStructure);
        }

        [Fact]
        public void AManualOverride_ReplacesTheProposal_AndChangesTheAvailableSpan()
        {
            var design = Composite(deepA: 8, deepB: 5);
            var baseline = new PushBackResolver(Catalog).Resolve(design);
            var baselineSpan = baseline.Composite.Cell(0, 1).AvailableBedSpan;

            design.Composite.StructureOverrideB = 8;
            var widened = new PushBackResolver(Catalog).Resolve(design);

            Assert.Equal(8, widened.Composite.SideB.EffectiveStructure);
            Assert.Equal(5, widened.Composite.SideB.ProposedStructure);   // la propuesta no se contamina
            Assert.Equal(8, widened.Composite.SideB.StructureOverride);
            Assert.True(widened.Structure.TotalLength > baseline.Structure.TotalLength);
            Assert.True(widened.Composite.Cell(0, 1).AvailableBedSpan >= baselineSpan);
        }

        [Fact]
        public void RestoringTheStructure_MeansClearingTheOverride()
        {
            var design = Composite(deepA: 8, deepB: 5);
            design.Composite.StructureOverrideB = 9;
            Assert.Equal(9, new PushBackResolver(Catalog).Resolve(design).Composite.SideB.EffectiveStructure);

            design.Composite.StructureOverrideB = null;   // restaurar = borrar el override
            var restored = new PushBackResolver(Catalog).Resolve(design);

            Assert.Equal(5, restored.Composite.SideB.EffectiveStructure);
            Assert.Equal(restored.Composite.SideB.ProposedStructure, restored.Composite.SideB.EffectiveStructure);
        }

        [Fact]
        public void AnInsufficientOverride_IsNotSilentlyCorrected_ButReported()
        {
            var design = Composite(deepA: 8, deepB: 5);
            design.Composite.StructureOverrideA = 3;   // menos de lo que la demanda de A necesita

            var system = new PushBackResolver(Catalog).Resolve(design);

            Assert.Equal(3, system.Composite.SideA.EffectiveStructure);   // se respeta: no se sube en silencio
            Assert.Equal(8, system.Composite.SideA.ProposedStructure);
            var cell = system.Composite.Cell(0, 1);
            Assert.False(cell.IsValid);
            Assert.Contains("estructura efectiva", cell.DisabledReason);
            Assert.True(cell.RequiredBedLength > cell.AvailableBedSpan);
        }

        // ---- La estructura fisica es UNA ---------------------------------------------------------------------

        [Fact]
        public void TheCompositeRun_IsOneModuleSequence_AWithTheGapAndBReversed()
        {
            var system = new PushBackResolver(Catalog).Resolve(Composite(deepA: 5, deepB: 4, gap: 12.0));
            var modules = system.Structure.Modules.ToList();

            // 5 posiciones de A + 1 hueco + 4 de B
            Assert.Equal(10, modules.Count);
            Assert.Equal(PushBackCompositeStructure.GapModuleId, modules[5].ModuleId);
            Assert.Equal(DynamicRackModuleKind.Gap, modules[5].Kind);
            Assert.Equal(12.0, modules[5].Length, 6);
            Assert.All(modules.Skip(6), module =>
                Assert.StartsWith(PushBackCompositeStructure.SideBModulePrefix, module.ModuleId));

            // Contigua: ningun modulo se solapa ni deja hueco no declarado.
            for (var index = 1; index < modules.Count; index++)
            {
                Assert.Equal(modules[index - 1].EndX, modules[index].StartX, 9);
            }
        }

        [Fact]
        public void TheInterface_KeepsTwoDistinctPostLines_EvenWithZeroGap()
        {
            var system = new PushBackResolver(Catalog).Resolve(Composite(deepA: 4, deepB: 4, gap: 0.0));
            var modules = system.Structure.Modules.ToList();

            var gapIndex = modules.FindIndex(module => module.ModuleId == PushBackCompositeStructure.GapModuleId);
            Assert.True(gapIndex > 0);
            Assert.Equal(0.0, modules[gapIndex].Length, 9);
            // A un lado y a otro del hueco hay DOS cabeceras distintas: ninguna se fusiona con la otra.
            Assert.True(modules[gapIndex - 1].IsHeader);
            Assert.True(modules[gapIndex + 1].IsHeader);
            Assert.NotEqual(modules[gapIndex - 1].ModuleId, modules[gapIndex + 1].ModuleId);
        }

        [Fact]
        public void TheGap_IsRealPhysicalLength_NotAVisualOffset()
        {
            var tight = new PushBackResolver(Catalog).Resolve(Composite(deepA: 4, deepB: 4, gap: 0.0));
            var loose = new PushBackResolver(Catalog).Resolve(Composite(deepA: 4, deepB: 4, gap: 10.0));

            Assert.Equal(tight.Structure.TotalLength + 10.0, loose.Structure.TotalLength, 6);
            Assert.Equal(10.0, loose.Composite.GapEndX - loose.Composite.GapStartX, 6);
        }

        [Fact]
        public void TheCentralSeparator_ReusesTheExistingSeparatorModule_AndNeedsRoom()
        {
            var withSeparator = new PushBackResolver(Catalog)
                .Resolve(Composite(deepA: 4, deepB: 4, gap: 10.0, centralSeparator: true));
            var gap = withSeparator.Structure.Modules
                .First(module => module.ModuleId == PushBackCompositeStructure.GapModuleId);

            Assert.Equal(DynamicRackModuleKind.Separator, gap.Kind);
            Assert.True(withSeparator.Composite.CentralSeparator);

            // Sin hueco no hay donde ponerlo: se declara ausente en vez de dibujar una pieza de longitud cero.
            var noRoom = new PushBackResolver(Catalog)
                .Resolve(Composite(deepA: 4, deepB: 4, gap: 0.0, centralSeparator: true));
            Assert.False(noRoom.Composite.CentralSeparator);
            Assert.Equal(
                DynamicRackModuleKind.Gap,
                noRoom.Structure.Modules.First(m => m.ModuleId == PushBackCompositeStructure.GapModuleId).Kind);
        }

        [Fact]
        public void TheTransverseGrid_IsSharedByBothSides()
        {
            var system = new PushBackResolver(Catalog).Resolve(Composite(slotsA: 3, slotsB: 4));

            // Cuatro ranuras fisicas: la mayor demanda gobierna la retícula compartida.
            Assert.Equal(4, system.Structure.Fronts.Count);
            Assert.Equal(4, system.Composite.SideA.Fronts.Count);
            Assert.Equal(4, system.Composite.SideB.Fronts.Count);
            // La cuarta existe SOLO en B.
            Assert.Null(system.Composite.SideA.Front(3));
            Assert.NotNull(system.Composite.SideB.Front(3));
            Assert.False(system.Composite.SideA.Resolved(3).IsPresent);
            Assert.True(system.Composite.SideB.Resolved(3).IsPresent);
        }

        [Fact]
        public void ASlotPresentOnlyInB_LivesOnlyInTheBHalfOfTheRun()
        {
            var system = new PushBackResolver(Catalog).Resolve(Composite(slotsA: 3, slotsB: 4, deepA: 5, deepB: 4));
            var slot = system.Structure.Fronts[3];

            // Empieza despues del hueco y llega al final: no tiene estructura en la mitad de A.
            Assert.Equal(system.Composite.SideB.FirstPosition, slot.DepthStartPosition);
            Assert.Equal(system.Structure.TotalLength, slot.EndX, 6);
            Assert.True(slot.StartX > 0.0);
        }

        [Fact]
        public void ASlotOnlyInA_AndASlotOnlyInB_CoexistInOneStructure()
        {
            // F1 compartida, F2 solo A, F3 solo B, F4 compartida — los cuatro sobre UNA estructura fisica.
            var design = Composite(slotsA: 4, slotsB: 4, deepA: 5, deepB: 4);
            design.SideB.Fronts[1] = null;            // ranura 1: solo A
            design.SideB.FrontConfigs[1] = null;
            design.Composite.AbsentSlotsA.Add(2);     // ranura 2: solo B (su config queda dormante en A)

            var system = new PushBackResolver(Catalog).Resolve(design);
            var composite = system.Composite;

            Assert.Equal(4, system.Structure.Fronts.Count);
            Assert.NotNull(composite.SideA.Front(0));
            Assert.NotNull(composite.SideB.Front(0));
            Assert.NotNull(composite.SideA.Front(1));
            Assert.Null(composite.SideB.Front(1));
            Assert.Null(composite.SideA.Front(2));
            Assert.NotNull(composite.SideB.Front(2));
            Assert.NotNull(composite.SideA.Front(3));
            Assert.NotNull(composite.SideB.Front(3));

            // Los rangos NO anidan y las dos ranuras exclusivas viven cada una en su mitad.
            var total = system.Structure.TotalLength;
            var onlyA = system.Structure.Fronts[1];
            var onlyB = system.Structure.Fronts[2];
            Assert.Equal(1, onlyA.DepthStartPosition);
            Assert.True(onlyA.EndX < total - 1e-6);
            Assert.True(onlyB.DepthStartPosition > 1);
            Assert.Equal(total, onlyB.EndX, 6);
            Assert.True(onlyB.StartX > onlyA.EndX - 1e-6);
        }

        [Fact]
        public void MixedExclusiveSlots_KeepOneStructure_WithUniqueModuleIdentity()
        {
            var design = Composite(slotsA: 4, slotsB: 4, deepA: 5, deepB: 4, gap: 6.0);
            design.SideB.Fronts[1] = null;
            design.SideB.FrontConfigs[1] = null;
            design.Composite.AbsentSlotsA.Add(2);

            var system = new PushBackResolver(Catalog).Resolve(design);
            var ids = system.Structure.Modules.Select(module => module.ModuleId).ToList();

            // UNA sola secuencia contigua de modulos, con identidad unica: no hay dos estructuras.
            Assert.Equal(ids.Count, ids.Distinct(StringComparer.Ordinal).Count());
            for (var index = 1; index < system.Structure.Modules.Count; index++)
            {
                Assert.Equal(system.Structure.Modules[index - 1].EndX, system.Structure.Modules[index].StartX, 9);
            }

            // Y ni una cama de la ranura solo-A en la mitad de B, ni al reves.
            var runs = PushBackRuns.Resolve(system);
            Assert.All(runs.Runs.Where(run => run.Slot == 1), run => Assert.Equal(PushBackSide.A, run.LowSide));
            Assert.All(runs.Runs.Where(run => run.Slot == 2), run => Assert.Equal(PushBackSide.B, run.LowSide));
        }

        // ---- Ni un poste, cabecera o placa duplicados --------------------------------------------------------

        [Fact]
        public void TheCompositeStructure_HasNoDuplicateModuleIdentity()
        {
            var system = new PushBackResolver(Catalog).Resolve(Composite(deepA: 6, deepB: 6, gap: 8.0));
            var ids = system.Structure.Modules.Select(module => module.ModuleId).ToList();

            Assert.Equal(ids.Count, ids.Distinct(StringComparer.Ordinal).Count());
        }

        [Fact]
        public void TheCompositeHeight_CoversTheDemandOfBothSides()
        {
            var system = new PushBackResolver(Catalog).Resolve(Composite(levelsA: 2, levelsB: 5));

            // La ranura fisica se dimensiona con la MAYOR demanda de niveles de los dos lados.
            Assert.Equal(5, system.Structure.Fronts[0].LoadLevels);
            Assert.Equal(2, system.Composite.SideA.Front(0).LoadLevels);
            Assert.Equal(5, system.Composite.SideB.Front(0).LoadLevels);
        }
    }
}
