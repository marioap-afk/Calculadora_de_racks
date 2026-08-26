using System;
using System.Linq;
using System.Text.Json;
using RackCad.Application.Bom;
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
    /// I-42 — AUDITORIA. Estas pruebas no persiguen una funcionalidad concreta: persiguen las CLASES de defecto que
    /// una composicion con dos marcos, dos numeraciones y dos configuraciones invita a cometer — indices locales
    /// mezclados con globales, niveles desplazados, estados imposibles, acumulacion entre resoluciones sucesivas,
    /// identidad duplicada y regresiones del legacy.
    /// </summary>
    public class PushBackCompositeAuditTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackSystem Resolve(PushBackDesign design) => new PushBackResolver(Catalog).Resolve(design);

        private static PushBackDesign Design(
            PushBackCellTopology topology = PushBackCellTopology.Encontradas,
            int slotsA = 2, int slotsB = 2, int deepA = 5, int deepB = 4,
            int levelsA = 3, int levelsB = 2, double gap = 0.0)
        {
            var design = PushBackCompositeStructureTests.Composite(
                slotsA: slotsA, slotsB: slotsB, deepA: deepA, deepB: deepB,
                levelsA: levelsA, levelsB: levelsB, gap: gap);
            design.Composite.DefaultTopology = topology;
            return design;
        }

        // ---- Indices: ranura, local y transversal significan lo mismo en todas partes -----------------------

        [Fact]
        public void TheSlotIndex_MeansTheSameInBothSidesAndInTheComposite()
        {
            var system = Resolve(Design(slotsA: 3, slotsB: 4));

            for (var slot = 0; slot < system.Structure.Fronts.Count; slot++)
            {
                Assert.Equal(slot, system.Composite.SideA.LocalIndexBySlot[slot]);
                Assert.Equal(slot, system.Composite.SideB.LocalIndexBySlot[slot]);
            }

            // Y la retícula transversal es LA MISMA en los dos lados y en la estructura compartida.
            var composite = DynamicFrontGeometry.Compute(system.Structure, Catalog).PostPositions;
            var local = DynamicFrontGeometry.Compute(system.Composite.SideB.Local.Structure, Catalog).PostPositions;
            Assert.Equal(composite.Count, local.Count);
            for (var index = 0; index < composite.Count; index++)
            {
                Assert.Equal(composite[index], local[index], 6);
            }
        }

        [Fact]
        public void ATopeDisabledBySlot_TurnsOffTheBedOfThatSlot_AndNoOther()
        {
            var design = Design(slotsA: 3, slotsB: 3, levelsA: 1, levelsB: 1);
            design.SideB.RearTope.Disable(2, 0);   // solo la ranura 2 del lado B

            var system = Resolve(design);
            var runs = PushBackRuns.Resolve(system);

            bool TopeAt(PushBackRun run)
                => run.Source.RearTope.At(run.SourceFrontIndex, run.SourceLevel - 1);

            Assert.True(TopeAt(runs.Runs.Single(r => r.Slot == 0 && r.LowSide == PushBackSide.B)));
            Assert.True(TopeAt(runs.Runs.Single(r => r.Slot == 1 && r.LowSide == PushBackSide.B)));
            Assert.False(TopeAt(runs.Runs.Single(r => r.Slot == 2 && r.LowSide == PushBackSide.B)));
            Assert.All(
                runs.Runs.Where(r => r.LowSide == PushBackSide.A),
                run => Assert.True(TopeAt(run)));
        }

        [Fact]
        public void LevelsAreOneBased_Everywhere_WithNoOffByOne()
        {
            var system = Resolve(Design(levelsA: 3, levelsB: 2, slotsA: 1, slotsB: 1));

            var levels = system.Composite.Cells.Select(cell => cell.LevelNumber).Distinct().OrderBy(n => n).ToList();
            Assert.Equal(new[] { 1, 2, 3 }, levels);

            var runs = PushBackRuns.Resolve(system).Runs;
            Assert.All(runs, run => Assert.InRange(run.Level, 1, 3));
            Assert.All(runs, run => Assert.Equal(run.Level, run.SourceLevel));
            // El nivel 3 solo existe en A, asi que alli solo hay una cama y es de A.
            Assert.Single(runs, run => run.Level == 3);
            Assert.Equal(PushBackSide.A, runs.Single(run => run.Level == 3).LowSide);
        }

        // ---- Estados limite y datos imposibles ---------------------------------------------------------------

        [Fact]
        public void ASideDeclaredPresentWithNoFronts_IsTreatedAsAbsent_NotAsAnError()
        {
            var design = Design(slotsA: 2, slotsB: 2);
            design.SideB.Fronts.Clear();
            design.SideB.FrontConfigs.Clear();

            var system = Resolve(design);

            Assert.False(system.IsComposite);
            Assert.Equal(2, system.Structure.Fronts.Count);
            Assert.DoesNotContain(PushBackRuns.Resolve(system).Runs, run => run.LowSide == PushBackSide.B);
        }

        [Fact]
        public void AllSlotsAbsentInA_IsRejectedWithAVisibleMessage_NotSilently()
        {
            var design = Design(slotsA: 2, slotsB: 2);
            design.Composite.AbsentSlotsA.Add(0);
            design.Composite.AbsentSlotsA.Add(1);

            var error = Record.Exception(() => Resolve(design));
            Assert.NotNull(error);
            Assert.False(string.IsNullOrWhiteSpace(error.Message));
        }

        [Fact]
        public void MinimumAndMaximumDepths_Resolve()
        {
            foreach (var pair in new[] { (2, 2), (2, 12), (12, 2) })
            {
                var system = Resolve(Design(deepA: pair.Item1, deepB: pair.Item2, levelsA: 1, levelsB: 1));
                Assert.True(system.Structure.TotalLength > 0.0);
                Assert.All(system.Composite.Cells, cell => Assert.True(cell.IsValid, cell.DisabledReason));
            }
        }

        [Fact]
        public void ADecimalGap_IsRespectedExactly()
        {
            var tight = Resolve(Design(gap: 0.0));
            var loose = Resolve(Design(gap: 3.5));

            Assert.Equal(tight.Structure.TotalLength + 3.5, loose.Structure.TotalLength, 6);
        }

        [Fact]
        public void ASingleLevelRack_AndAManyLevelRack_BothResolve()
        {
            foreach (var levels in new[] { 1, 6 })
            {
                var system = Resolve(Design(levelsA: levels, levelsB: levels, slotsA: 1, slotsB: 1));
                Assert.Equal(levels, system.Composite.Cells.Count);
                Assert.Equal(levels * 2, PushBackRuns.Resolve(system).Runs.Count);
            }
        }

        // ---- Repeticion: nada se acumula ni deriva -----------------------------------------------------------

        [Fact]
        public void ResolvingTheSameDesignTwice_ProducesTheSameSystem()
        {
            var design = Design(gap: 6.0);
            var first = Resolve(design);
            var second = Resolve(design);

            Assert.Equal(first.Structure.Modules.Count, second.Structure.Modules.Count);
            Assert.Equal(first.Structure.TotalLength, second.Structure.TotalLength, 9);
            Assert.Equal(first.Composite.Cells.Count, second.Composite.Cells.Count);
            Assert.Equal(
                PushBackRuns.Resolve(first).Runs.Count,
                PushBackRuns.Resolve(second).Runs.Count);
            Assert.Equal(
                first.Structure.Modules.Select(m => m.ModuleId),
                second.Structure.Modules.Select(m => m.ModuleId));
        }

        [Fact]
        public void RepeatedSaveAndLoad_Converges_WithoutDrift()
        {
            var design = Design(gap: 6.0);
            var current = design;
            var lengths = new System.Collections.Generic.List<double>();
            for (var round = 0; round < 4; round++)
            {
                var system = Resolve(current);
                lengths.Add(system.Structure.TotalLength);
                var snapshot = new PushBackResolver(Catalog).Snapshot(system);
                snapshot.SideB = current.SideB;
                snapshot.Composite = current.Composite;
                var json = JsonSerializer.Serialize(PushBackDesignDocument.FromDomain(snapshot));
                current = JsonSerializer.Deserialize<PushBackDesignDocument>(json).ToDomain();
            }

            Assert.All(lengths, length => Assert.Equal(lengths[0], length, 6));
        }

        [Fact]
        public void RepeatedTopologyChanges_DoNotAccumulateStoredCells()
        {
            var state = new PushBackCompositeEditorState();
            state.SideA.SetFrontCount(2);
            state.SetSideBPresent(true);
            state.SideB.SetFrontCount(2);
            state.SetDefaults(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB);
            // I-42: declarar la CAPACIDAD del lado B ya no lo declara PRESENTE en ningun frente.
            // Este fixture quiere el rack compuesto ENTERO, asi que lo declara frente a frente.
            for (var declared = 0; declared < state.SlotCount; declared++)
            {
                state.SetSlotPresent(PushBackSide.B, declared, true);
            }


            for (var round = 0; round < 10; round++)
            {
                state.ApplyTopology(PushBackCellTopology.Corrida, PushBackRunDirection.AToB, DynamicRackCellScope.All);
                state.ApplyTopology(PushBackCellTopology.Encontradas, PushBackRunDirection.AToB, DynamicRackCellScope.All);
            }

            // Escribir el valor por defecto BORRA la entrada: la lista no crece indefinidamente.
            Assert.Empty(state.BuildComposite().Topologies);
        }

        // ---- Identidad fisica --------------------------------------------------------------------------------

        [Fact]
        public void NoModuleIdentityIsDuplicated_InAnyConfiguration()
        {
            foreach (var gap in new[] { 0.0, 12.0 })
            {
                foreach (var separator in new[] { false, true })
                {
                    var design = Design(slotsA: 3, slotsB: 4, gap: gap);
                    design.Composite.CentralSeparator = separator;
                    var ids = Resolve(design).Structure.Modules.Select(module => module.ModuleId).ToList();

                    Assert.Equal(ids.Count, ids.Distinct(StringComparer.Ordinal).Count());
                    Assert.DoesNotContain(ids, id => string.IsNullOrWhiteSpace(id));
                }
            }
        }

        [Fact]
        public void TheCompositeStructure_IsContiguous_AndItsFrontsLieInsideIt()
        {
            var system = Resolve(Design(slotsA: 3, slotsB: 4, gap: 9.0));
            var modules = system.Structure.Modules.ToList();

            Assert.Equal(0.0, modules[0].StartX, 9);
            for (var index = 1; index < modules.Count; index++)
            {
                Assert.Equal(modules[index - 1].EndX, modules[index].StartX, 9);
            }

            Assert.All(system.Structure.Fronts, front =>
            {
                Assert.True(front.StartX >= -1e-6);
                Assert.True(front.EndX <= system.Structure.TotalLength + 1e-6);
                Assert.True(front.EndX > front.StartX);
            });
        }

        [Fact]
        public void EveryBedLiesInsideTheRack_AndOnItsOwnSide()
        {
            var system = Resolve(Design(slotsA: 2, slotsB: 2, gap: 6.0, levelsA: 2, levelsB: 2));
            var total = system.Structure.TotalLength;
            var runs = PushBackRuns.Resolve(system);

            foreach (var run in runs.Runs)
            {
                var axis = PushBackRunGeometry.Axis(run, Catalog, runs.MirrorAxis);
                Assert.True(axis.HasValue);
                Assert.InRange(axis.Value.LowContact.X, -1.0, total + 1.0);
                Assert.InRange(axis.Value.HighContact.X, -1.0, total + 1.0);
                if (run.LowSide == PushBackSide.A)
                {
                    Assert.True(axis.Value.LowContact.X < total / 2.0);
                }
                else
                {
                    Assert.True(axis.Value.LowContact.X > total / 2.0);
                }
            }
        }

        // ---- El BOM no inventa ni pierde piezas ---------------------------------------------------------------

        [Fact]
        public void TheBedCount_AlwaysEqualsThePhysicalRunCount()
        {
            foreach (var topology in new[]
                     {
                         PushBackCellTopology.SoloA, PushBackCellTopology.SoloB,
                         PushBackCellTopology.Encontradas, PushBackCellTopology.Corrida
                     })
            {
                var system = Resolve(Design(topology, slotsA: 2, slotsB: 2, levelsA: 2, levelsB: 2));
                var runs = PushBackRuns.Resolve(system).Runs;
                var beds = PushBackBomBuilder.Build(system, Catalog).Components
                    .Where(component => component.Category == SystemBomBuilder.Cama)
                    .Sum(component => component.Quantity);

                // Una calle por cama fisica (los frentes de esta prueba tienen una sola calle).
                Assert.Equal(runs.Count, beds);
            }
        }

        [Fact]
        public void ThePalletsNeverReachTheBom_InAnyTopology()
        {
            foreach (var topology in new[]
                     {
                         PushBackCellTopology.SoloA, PushBackCellTopology.SoloB,
                         PushBackCellTopology.Encontradas, PushBackCellTopology.Corrida
                     })
            {
                var design = Design(topology, slotsA: 1, slotsB: 1, levelsA: 1, levelsB: 1);
                design.Fronts[0].DrawPallets.Add(true);
                design.SideB.FrontConfigs[0].DrawPallets.Add(true);
                var bom = PushBackBomBuilder.Build(Resolve(design), Catalog);

                Assert.DoesNotContain(bom.Components, component =>
                    (component.ProfileId ?? string.Empty).IndexOf("TARIMA", StringComparison.OrdinalIgnoreCase) >= 0);
            }
        }

        [Fact]
        public void TheIntermediateBomCount_EqualsThePiecesTheDrawingProduces()
        {
            foreach (var topology in new[]
                     {
                         PushBackCellTopology.SoloA, PushBackCellTopology.Encontradas, PushBackCellTopology.Corrida
                     })
            {
                var system = Resolve(Design(topology, slotsA: 2, slotsB: 2, deepA: 8, deepB: 6, levelsA: 2, levelsB: 2));
                var runs = PushBackRuns.Resolve(system);
                var builder = new PushBackIntermediateBeamLateralBuilder();
                var drawn = PushBackCompositeContent.Batches(runs, null)
                    .Where(batch => batch.Front != null)
                    .Sum(batch => builder.BuildFor(batch.Source, Catalog, batch.Front, batch.Levels).Count);

                var counted = PushBackBomBuilder.Build(system, Catalog).Components
                    .Where(component => component.Category == SystemBomBuilder.IntermediateBeam)
                    .Sum(component => component.Quantity);

                Assert.Equal(drawn, counted);
            }
        }

        // ---- Legacy: cero cambios ------------------------------------------------------------------------------

        [Fact]
        public void ALegacyRack_IsUnaffectedByEveryCompositeAuthority()
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
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

            var system = Resolve(design);

            Assert.False(system.IsComposite);
            Assert.Null(system.Composite);
            Assert.False(design.Structure.AllowsNonNestedDepthRanges);
            Assert.Empty(PushBackRuns.Resolve(system).Runs);
            Assert.DoesNotContain(system.Structure.Modules, module => module.Kind == DynamicRackModuleKind.Gap);
            Assert.Equal(6, system.Structure.Modules.Count);

            // Y ninguna vista le anade una etiqueta A/B ni una decoracion por lado.
            var lateral = new PushBackSystemLateralBuilder().Build(system, Catalog).Flatten().Instances;
            var labels = lateral.Where(i => i.Role == HeaderBlockRole.Annotation).Select(i => i.Text).ToList();
            Assert.DoesNotContain("A", labels);
            Assert.DoesNotContain("B", labels);
        }

        [Fact]
        public void TheDynamicDepthContract_StillRequiresNesting_ByDefault()
        {
            var fronts = new[]
            {
                new DynamicRackFrontDesign { PalletsDeep = 4, DepthStartPosition = 1 },
                new DynamicRackFrontDesign { PalletsDeep = 3, DepthStartPosition = 6 }
            };

            // El sistema Dinamico NO se relaja: los rangos no anidados siguen siendo un error alli.
            Assert.Throws<ArgumentException>(() => DynamicDepthGeometry.Resolve(fronts, 4));
            // Y con el modo explicito de I-42 se aceptan.
            var layout = DynamicDepthGeometry.Resolve(fronts, 4, DynamicDepthNesting.NotRequired);
            Assert.Equal(8, layout.TotalPositions);
        }
    }
}
