using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Headers;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-18b increment 2 — the PURE Push Back editor state. It composes the dynamic editor state (the SAME
    /// <see cref="DynamicFrontMatrix"/>, <see cref="DynamicEditorValues"/> and <see cref="DynamicRackCellScopeResolver"/>)
    /// and adds only Push Back's own authority: the rear beam peralte per front x level and the rear-tope activation. These
    /// tests exercise the state with NO WPF/AutoCAD and NO geometry mocks — the assembler runs the real resolve/BOM/plan
    /// pipeline against the test catalog.
    /// </summary>
    public class PushBackEditorStateTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackEditorDesignAssembler Assembler() => new PushBackEditorDesignAssembler(Catalog);

        private static PushBackEditorInputs Inputs(int palletsDeep = 4)
            => new PushBackEditorInputs
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = palletsDeep
            };

        private static PushBackEditorValues Values(
            int positions = 1, int levels = 2, int palletsDeep = 4,
            double peralte = 3.5, bool tope = true, double firstLevel = 6.0, double palletFront = 42.0)
            => new PushBackEditorValues
            {
                Dynamic = new DynamicEditorValues
                {
                    PalletCount = positions,
                    LoadLevels = levels,
                    PalletsDeep = palletsDeep,
                    DepthStartPosition = 1,
                    FirstLevelHeight = firstLevel,
                    PalletFront = palletFront,
                    PalletHeight = 60.0,
                    PalletWeight = 1000.0,
                    ClearHeight = 6.0,
                    InOutBeamCatalogId = DynamicRackDefaults.InOutBeamCatalogId,
                    InOutBeamDepth = 6.0,
                    IntermediateBeamCatalogId = DynamicRackDefaults.IntermediateBeamCatalogId,
                    IntermediateBeamDepth = 3.5
                },
                HighEndBeamPeralte = peralte,
                RearTopeEnabled = tope
            };

        private static void SetCell(PushBackEditorState state, int front, int level, double peralte, bool tope = true)
        {
            var cell = state.Cell(front, level);
            cell.HighEndBeamPeralte = peralte;
            cell.RearTopeEnabled = tope;
        }

        /// <summary>A pure 2 front x 2 level grid: no catalog, no resolve — just editor shape.</summary>
        private static PushBackEditorState Grid2x2()
        {
            var state = new PushBackEditorState();
            state.AdjustLevels(0, -1);   // front 0: 3 -> 2 levels
            state.SetFrontCount(2);      // clone front 0 (2 levels) -> front 1
            state.ToggleCell(0, 0, false);
            return state;
        }

        private static IEnumerable<HeaderBlockInstance> AllInstances(DynamicSystemPlan plan)
            => (plan?.Headers ?? new List<HeaderGroup>()).SelectMany(group => group.Instances)
                .Concat(plan?.LooseInstances ?? new List<HeaderBlockInstance>());

        // ---- 1. New-design defaults -----------------------------------------------------------------------------

        [Fact]
        public void New_OpensWithOneDefaultFront_Peralte35_TopesActive_FirstCellSelected()
        {
            var state = new PushBackEditorState();

            Assert.Equal(1, state.Structure.Count);
            Assert.Equal(DynamicRackDefaults.DefaultLoadLevels, state.Structure.Fronts[0].LoadLevels);
            Assert.Single(state.PushFronts);
            Assert.Equal(DynamicRackDefaults.DefaultLoadLevels, state.PushFronts[0].Cells.Count);
            Assert.All(state.PushFronts[0].Cells, cell =>
            {
                Assert.Equal(3.5, cell.HighEndBeamPeralte, 4);   // explicit 3.5, never allowed[0]
                Assert.True(cell.RearTopeEnabled);
            });
            Assert.Equal(0, state.Structure.SelectedFrontIndex);
            Assert.Equal(0, state.Structure.SelectedLevelIndex);
            Assert.Equal(1, state.Structure.SelectedCellCount);
            Assert.True(state.Structure.IsSelected(0, 0));
        }

        // ---- 2. Selection ---------------------------------------------------------------------------------------

        [Fact]
        public void Selection_ToggleAndNormalize_StayOnTheMatrix()
        {
            var state = Grid2x2();

            state.ToggleCell(1, 1, true);   // extend
            Assert.Equal(2, state.Structure.SelectedCellCount);
            Assert.True(state.Structure.IsSelected(0, 0));
            Assert.True(state.Structure.IsSelected(1, 1));

            state.ToggleCell(1, 0, false);  // replace
            Assert.Equal(1, state.Structure.SelectedCellCount);
            Assert.True(state.Structure.IsSelected(1, 0));

            state.SetFrontCount(1);         // front 1 gone; the selection must be re-seated, not orphaned
            state.NormalizeSelection();
            Assert.Equal(0, state.Structure.SelectedFrontIndex);
            Assert.InRange(state.Structure.SelectedLevelIndex, 0, state.Structure.Fronts[0].LoadLevels - 1);
        }

        // ---- 3. Grow / shrink fronts ----------------------------------------------------------------------------

        [Fact]
        public void SetFrontCount_GrowClonesSelectedFrontConfig_ShrinkDropsAbsentFronts()
        {
            var state = new PushBackEditorState();
            SetCell(state, 0, 0, 5.0);
            SetCell(state, 0, 1, 4.0);
            SetCell(state, 0, 2, 6.0);

            state.SetFrontCount(3);
            Assert.Equal(3, state.Structure.Count);
            Assert.Equal(3, state.PushFronts.Count);
            // Every new front is a clone of the (selected) template front's config.
            for (var front = 0; front < 3; front++)
            {
                Assert.Equal(5.0, state.Cell(front, 0).HighEndBeamPeralte, 4);
                Assert.Equal(4.0, state.Cell(front, 1).HighEndBeamPeralte, 4);
                Assert.Equal(6.0, state.Cell(front, 2).HighEndBeamPeralte, 4);
            }

            state.SetFrontCount(1);
            Assert.Equal(1, state.Structure.Count);
            Assert.Single(state.PushFronts);
            Assert.Equal(5.0, state.Cell(0, 0).HighEndBeamPeralte, 4);   // surviving front intact
        }

        // ---- 4. Grow / shrink levels ----------------------------------------------------------------------------

        [Fact]
        public void AdjustLevels_GrowClonesLastCell_ShrinkDropsTrailing_ConservesIntersection()
        {
            var state = new PushBackEditorState();   // 3 levels
            SetCell(state, 0, 0, 5.0);
            SetCell(state, 0, 1, 4.0);
            SetCell(state, 0, 2, 6.0);

            state.AdjustLevels(0, -1);   // 3 -> 2: drop only the trailing cell (level 3 = 6.0)
            Assert.Equal(2, state.PushFronts[0].Cells.Count);
            Assert.Equal(5.0, state.Cell(0, 0).HighEndBeamPeralte, 4);
            Assert.Equal(4.0, state.Cell(0, 1).HighEndBeamPeralte, 4);

            state.AdjustLevels(0, 1);    // 2 -> 3: grow clones the LAST cell (4.0), intersection conserved
            Assert.Equal(3, state.PushFronts[0].Cells.Count);
            Assert.Equal(5.0, state.Cell(0, 0).HighEndBeamPeralte, 4);
            Assert.Equal(4.0, state.Cell(0, 1).HighEndBeamPeralte, 4);
            Assert.Equal(4.0, state.Cell(0, 2).HighEndBeamPeralte, 4);   // cloned from last, not the dropped 6.0
        }

        // ---- 5. Per-cell peraltes differ by front and by level --------------------------------------------------

        [Fact]
        public void Peralte_IsIndependentPerFrontAndPerLevel()
        {
            var state = Grid2x2();
            SetCell(state, 0, 0, 5.0);
            SetCell(state, 0, 1, 4.0);
            SetCell(state, 1, 0, 6.0);
            SetCell(state, 1, 1, 4.5);

            Assert.Equal(5.0, state.Cell(0, 0).HighEndBeamPeralte, 4);
            Assert.NotEqual(state.Cell(0, 0).HighEndBeamPeralte, state.Cell(0, 1).HighEndBeamPeralte); // by level
            Assert.NotEqual(state.Cell(0, 0).HighEndBeamPeralte, state.Cell(1, 0).HighEndBeamPeralte); // by front
        }

        // ---- 6. Invalid peralte -> explicit 3.5 -----------------------------------------------------------------

        [Theory]
        [InlineData(5.0, 5.0)]
        [InlineData(4.5, 4.5)]
        [InlineData(7.0, 3.5)]   // invalid -> explicit default (not allowed[0] = 3.0)
        [InlineData(0.0, 3.5)]   // unset -> explicit default
        public void NormalizePeralte_HonoursValidElseFallsBackTo35(double requested, double expected)
        {
            var allowed = new PushBackResolver(Catalog).AllowedHighEndPeraltes();
            var cell = new PushBackEditorCell { HighEndBeamPeralte = requested };

            cell.NormalizePeralte(allowed);

            Assert.Equal(expected, cell.HighEndBeamPeralte, 4);
        }

        [Fact]
        public void BuildDesign_InvalidPeralteCell_ResolvesToExplicit35()
        {
            var state = new PushBackEditorState();
            SetCell(state, 0, 0, 7.0);   // not in the catalog list

            var comp = Assembler().Build(state, Inputs());

            Assert.True(comp.IsValid, comp.Error);
            Assert.Equal(3.5, comp.System.HighEndBeamPeralteAt(0, 0), 4);
        }

        // ---- 7-11. ApplyScope by cell / selection / level / front / all -----------------------------------------

        [Fact]
        public void ApplyScope_Cell_WritesOnlyTheSourceCell()
        {
            var state = Grid2x2();
            var written = state.ApplyScope(Values(peralte: 5.0, tope: false), DynamicRackCellScope.Cell);

            Assert.Equal(1, written);
            Assert.Equal(5.0, state.Cell(0, 0).HighEndBeamPeralte, 4);
            Assert.False(state.Cell(0, 0).RearTopeEnabled);
            Assert.Equal(3.5, state.Cell(0, 1).HighEndBeamPeralte, 4);   // untouched
            Assert.Equal(3.5, state.Cell(1, 0).HighEndBeamPeralte, 4);
        }

        [Fact]
        public void ApplyScope_Level_WritesThatLevelAcrossFronts()
        {
            var state = Grid2x2();
            var written = state.ApplyScope(Values(peralte: 5.0), DynamicRackCellScope.Level);

            Assert.Equal(2, written);
            Assert.Equal(5.0, state.Cell(0, 0).HighEndBeamPeralte, 4);
            Assert.Equal(5.0, state.Cell(1, 0).HighEndBeamPeralte, 4);
            Assert.Equal(3.5, state.Cell(0, 1).HighEndBeamPeralte, 4);
            Assert.Equal(3.5, state.Cell(1, 1).HighEndBeamPeralte, 4);
        }

        [Fact]
        public void ApplyScope_Front_WritesEveryLevelOfTheSourceFront()
        {
            var state = Grid2x2();
            var written = state.ApplyScope(Values(peralte: 5.0), DynamicRackCellScope.Front);

            Assert.Equal(2, written);
            Assert.Equal(5.0, state.Cell(0, 0).HighEndBeamPeralte, 4);
            Assert.Equal(5.0, state.Cell(0, 1).HighEndBeamPeralte, 4);
            Assert.Equal(3.5, state.Cell(1, 0).HighEndBeamPeralte, 4);   // other front untouched
        }

        [Fact]
        public void ApplyScope_All_WritesEveryCell()
        {
            var state = Grid2x2();
            var written = state.ApplyScope(Values(peralte: 5.0), DynamicRackCellScope.All);

            Assert.Equal(4, written);
            Assert.All(new[] { (0, 0), (0, 1), (1, 0), (1, 1) },
                cell => Assert.Equal(5.0, state.Cell(cell.Item1, cell.Item2).HighEndBeamPeralte, 4));
        }

        [Fact]
        public void ApplyScope_Selected_WritesOnlyTheSelectedCells()
        {
            var state = Grid2x2();
            state.ToggleCell(1, 1, true);   // selection = {(0,0),(1,1)}

            var written = state.ApplyScope(Values(peralte: 5.0), DynamicRackCellScope.Selected);

            Assert.Equal(2, written);
            Assert.Equal(5.0, state.Cell(0, 0).HighEndBeamPeralte, 4);
            Assert.Equal(5.0, state.Cell(1, 1).HighEndBeamPeralte, 4);
            Assert.Equal(3.5, state.Cell(0, 1).HighEndBeamPeralte, 4);   // not selected
            Assert.Equal(3.5, state.Cell(1, 0).HighEndBeamPeralte, 4);
        }

        // ---- 12. Same targets for the dynamic values and the Push Back values -----------------------------------

        [Fact]
        public void ApplyScope_DynamicAndPushBack_HitTheExactSameCells()
        {
            var state = Grid2x2();
            state.ApplyScope(Values(peralte: 7.5, tope: false, palletFront: 99.0), DynamicRackCellScope.Level);

            // Level 0 of both fronts got BOTH the dynamic value (palletFront 99) AND the Push Back value (peralte 7.5, tope off).
            for (var front = 0; front < 2; front++)
            {
                Assert.Equal(99.0, state.Structure.Fronts[front].Cells[0].PalletFront, 4);
                Assert.Equal(7.5, state.Cell(front, 0).HighEndBeamPeralte, 4);
                Assert.False(state.Cell(front, 0).RearTopeEnabled);
                // Level 1 got neither.
                Assert.NotEqual(99.0, state.Structure.Fronts[front].Cells[1].PalletFront);
                Assert.Equal(3.5, state.Cell(front, 1).HighEndBeamPeralte, 4);
                Assert.True(state.Cell(front, 1).RearTopeEnabled);
            }
        }

        // ---- 13-14. Rear topes: bool <-> OffCells ---------------------------------------------------------------

        [Fact]
        public void RearTope_AllActive_ProducesNoPositiveOffCells()
        {
            var design = Assembler().BuildDesign(new PushBackEditorState(), Inputs());

            Assert.Empty(design.RearTope.OffCells);   // active by default => nothing persisted
        }

        [Fact]
        public void RearTope_DeactivatedCell_BecomesTheOnlyOffCell()
        {
            var state = new PushBackEditorState();
            SetCell(state, 0, 1, 3.5, tope: false);   // deactivate one cell

            var design = Assembler().BuildDesign(state, Inputs());

            var off = Assert.Single(design.RearTope.OffCells);
            Assert.Equal(0, off.Frente);
            Assert.Equal(1, off.Level);
            Assert.False(design.RearTope.At(0, 1));
            Assert.True(design.RearTope.At(0, 0));
        }

        // ---- 15. Shape mutation purges deactivations of cells that no longer exist -------------------------------

        [Fact]
        public void OffCells_AreNotEmittedForCellsRemovedByAShapeChange()
        {
            var state = new PushBackEditorState();   // 3 levels
            SetCell(state, 0, 2, 3.5, tope: false);  // deactivate the top cell

            state.AdjustLevels(0, -1);               // remove level 3 (the deactivated cell)
            var design = Assembler().BuildDesign(state, Inputs());

            Assert.Empty(design.RearTope.OffCells);   // the vanished deactivation is not materialized
        }

        // ---- 16-20. Load from a design with two different fronts -------------------------------------------------

        [Fact]
        public void LoadFromDesign_RebuildsDifferentFondos_DepthStart_Levels_Peraltes_AndTopes()
        {
            var design = TwoFrontDesign();

            var state = new PushBackEditorState();
            state.LoadFromDesign(design, new PushBackResolver(Catalog));

            Assert.Equal(2, state.Structure.Count);
            // Different fondo counts and DepthStartPosition survive.
            Assert.Equal(1, state.Structure.Fronts[0].DepthStartPosition);
            Assert.Equal(4, state.Structure.Fronts[1].DepthStartPosition);
            Assert.NotEqual(state.Structure.Fronts[0].PalletsDeep, state.Structure.Fronts[1].PalletsDeep);
            // Different per-front level counts survive.
            Assert.Equal(3, state.Structure.Fronts[0].LoadLevels);
            Assert.Equal(2, state.Structure.Fronts[1].LoadLevels);
            // Per-cell peraltes are rebuilt from the resolved system.
            Assert.Equal(5.0, state.Cell(0, 0).HighEndBeamPeralte, 4);
            Assert.Equal(4.0, state.Cell(0, 1).HighEndBeamPeralte, 4);
            Assert.Equal(6.0, state.Cell(0, 2).HighEndBeamPeralte, 4);
            Assert.Equal(4.5, state.Cell(1, 0).HighEndBeamPeralte, 4);
            Assert.Equal(5.5, state.Cell(1, 1).HighEndBeamPeralte, 4);
            // The one deactivated rear tope survives; everything else is active.
            Assert.False(state.Cell(0, 1).RearTopeEnabled);
            Assert.True(state.Cell(0, 0).RearTopeEnabled);
            Assert.True(state.Cell(1, 0).RearTopeEnabled);
        }

        [Fact]
        public void LoadFromDesign_RecoversRackWideInputs()
        {
            var design = TwoFrontDesign();

            var state = new PushBackEditorState();
            var inputs = state.LoadFromDesign(design, new PushBackResolver(Catalog));

            Assert.Equal(42.0, inputs.Pallet.Front, 4);
            Assert.Equal(48.0, inputs.Pallet.Depth, 4);
            Assert.True(inputs.PalletsDeep >= 2);
        }

        [Fact]
        public void LoadFromSystem_TakesTheSameDesignPath()
        {
            var resolver = new PushBackResolver(Catalog);
            var system = resolver.Resolve(TwoFrontDesign());

            var state = new PushBackEditorState();
            state.LoadFromSystem(system, resolver);

            Assert.Equal(2, state.Structure.Count);
            Assert.Equal(5.0, state.Cell(0, 0).HighEndBeamPeralte, 4);
            Assert.False(state.Cell(0, 1).RearTopeEnabled);
        }

        // ---- 21-22. Safety: keep the applicable families, never a guide, never mutate the source ----------------

        [Fact]
        public void Build_StripsEntranceGuides_KeepsOtherFamilies_WithoutMutatingInput()
        {
            var state = new PushBackEditorState();
            var inputs = Inputs();
            var guia = new SelectiveSafetySelection { ElementId = "GUIA_ENTRADA", Quantity = 1, Side = SafetySide.Both };
            var bota = new SelectiveSafetySelection { ElementId = "PROTECTOR_BOTA_H_3_16_18", Quantity = 1, Side = SafetySide.Both };
            inputs.SafetySelections.Add(guia);
            inputs.SafetySelections.Add(bota);

            var comp = Assembler().Build(state, inputs);

            Assert.True(comp.IsValid, comp.Error);
            Assert.DoesNotContain(comp.System.SafetySelections, s => s.ElementId == "GUIA_ENTRADA");
            Assert.Contains(comp.System.SafetySelections, s => s.ElementId == "PROTECTOR_BOTA_H_3_16_18");
            // The input list and its selections are untouched (independent copies were used).
            Assert.Equal(2, inputs.SafetySelections.Count);
            Assert.Contains(inputs.SafetySelections, s => s.ElementId == "GUIA_ENTRADA");
            Assert.Equal(SafetySide.Both, bota.Side);
        }

        [Fact]
        public void AuthorizedSafety_ReturnsGuiaFreeDeepCopies_WithoutMutatingInput()
        {
            var guia = new SelectiveSafetySelection { ElementId = "GUIA_ENTRADA", Quantity = 1 };
            var bota = new SelectiveSafetySelection { ElementId = "PROTECTOR_BOTA_H_3_16_18", Quantity = 1 };
            var source = new List<SelectiveSafetySelection> { guia, bota };

            var authorized = Assembler().AuthorizedSafety(source);

            var kept = Assert.Single(authorized);
            Assert.Equal("PROTECTOR_BOTA_H_3_16_18", kept.ElementId);
            Assert.NotSame(bota, kept);                 // deep copy
            Assert.Equal(2, source.Count);              // input not mutated
        }

        // ---- 23. Deep snapshot / rollback -----------------------------------------------------------------------

        [Fact]
        public void Snapshot_Restore_DeepRollsBackBothAuthorities()
        {
            var state = Grid2x2();
            SetCell(state, 0, 0, 5.0);
            SetCell(state, 1, 1, 6.0);

            var snapshot = state.Snapshot();
            // Mutate structure AND push config after snapshotting.
            state.SetFrontCount(3);
            SetCell(state, 0, 0, 4.0);
            state.Structure.Fronts[0].Cells[0].PalletFront = 123.0;

            state.Restore(snapshot);

            Assert.Equal(2, state.Structure.Count);                       // structure rolled back
            Assert.Equal(5.0, state.Cell(0, 0).HighEndBeamPeralte, 4);    // push config rolled back
            Assert.Equal(6.0, state.Cell(1, 1).HighEndBeamPeralte, 4);
            Assert.NotEqual(123.0, state.Structure.Fronts[0].Cells[0].PalletFront);

            // The snapshot is independent: mutating the state again does not corrupt it, so a second restore still works.
            SetCell(state, 0, 0, 2.0);
            state.Restore(snapshot);
            Assert.Equal(5.0, state.Cell(0, 0).HighEndBeamPeralte, 4);
        }

        // ---- 24. Design -> Resolve -> editor -> Design -> Resolve preserves geometry ----------------------------

        [Fact]
        public void RoundTrip_ThroughTheEditor_PreservesGeometryPeraltesAndOverrides()
        {
            var resolver = new PushBackResolver(Catalog);
            var design = TwoFrontDesign(withLengthOverride: true);
            var system1 = resolver.Resolve(design);

            var state = new PushBackEditorState();
            var inputs = state.LoadFromDesign(design, resolver);
            var rebuilt = Assembler().BuildDesign(state, inputs);
            var system2 = resolver.Resolve(rebuilt);

            Assert.Equal(system1.Fronts.Count, system2.Fronts.Count);
            Assert.Equal(system1.TotalLength, system2.TotalLength, 3);
            for (var front = 0; front < system1.Fronts.Count; front++)
            {
                Assert.Equal(system1.Fronts[front].EndX - system1.Fronts[front].StartX,
                             system2.Fronts[front].EndX - system2.Fronts[front].StartX, 3);
                Assert.Equal(system1.Fronts[front].LoadLevels, system2.Fronts[front].LoadLevels);
                for (var level = 0; level < system1.Fronts[front].LoadLevels; level++)
                {
                    Assert.Equal(system1.HighEndBeamPeralteAt(front, level),
                                 system2.HighEndBeamPeralteAt(front, level), 4);
                }
            }

            // The per-cell length override rode through the editor (front 1's beam length is preserved).
            Assert.Equal(PushBackLoadBeamGeometry.CellBeamLength(system1.Structure, system1.Fronts[1], 1),
                         PushBackLoadBeamGeometry.CellBeamLength(system2.Structure, system2.Fronts[1], 1), 3);
        }

        // ---- 25. BOM + plans: Push Back pieces present, no brakes, no guides ------------------------------------

        [Fact]
        public void Build_ProducesBomAndFourPlans_WithPushBackPiecesAndNoBrakesOrGuides()
        {
            var comp = Assembler().Build(new PushBackEditorState(), Inputs());

            Assert.True(comp.IsValid, comp.Error);
            Assert.NotNull(comp.LateralPlan);
            Assert.NotNull(comp.FrontalEntradaSalida);
            Assert.NotNull(comp.FrontalPosterior);
            Assert.NotNull(comp.PlantaPlan);

            // BOM carries the Push Back categories and no dynamic bed brake.
            Assert.Contains(comp.Bom.Components, component => component.Category == PushBackBomBuilder.HighEndBeam);
            Assert.Contains(comp.Bom.Components, component => component.Category == PushBackBomBuilder.RearTope);

            var lateral = AllInstances(comp.LateralPlan).ToList();
            Assert.DoesNotContain(lateral, instance => instance.Role == HeaderBlockRole.Brake);   // pushback bed: no frenos

            var posterior = AllInstances(comp.FrontalPosterior).ToList();
            Assert.Contains(posterior, instance =>
                string.Equals(instance.PieceId, PushBackDefaults.HighEndBeamCatalogId, StringComparison.OrdinalIgnoreCase));
            Assert.Contains(posterior, instance =>
                string.Equals(instance.PieceId, PushBackRearTopeBuilder.TopePieceId, StringComparison.OrdinalIgnoreCase));

            // No entrance guide anywhere in the resolved system.
            Assert.DoesNotContain(comp.System.SafetySelections,
                s => s.ElementId != null && s.ElementId.IndexOf("GUIA", StringComparison.OrdinalIgnoreCase) >= 0);
        }

        // ---- Fixtures -------------------------------------------------------------------------------------------

        /// <summary>Two fronts that differ in fondo count, DepthStartPosition, level count and per-cell peralte, with one
        /// deactivated rear tope (front 0, level 1). Optionally a per-cell length override on front 1, level 1.</summary>
        private static PushBackDesign TwoFrontDesign(bool withLengthOverride = false)
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 6,
                    LoadLevels = 3,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 2, LoadLevels = 3, PalletsDeep = 6, DepthStartPosition = 1 });

            var front1 = new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 3, DepthStartPosition = 4 };
            if (withLengthOverride)
            {
                front1.Levels.Add(new DynamicRackLevelDesign { BeamLengthOverride = 132.0 });
            }
            design.Structure.Fronts.Add(front1);

            var f0 = new PushBackFrontConfig();
            f0.HighEndBeamPeraltes.Add(5.0);
            f0.HighEndBeamPeraltes.Add(4.0);
            f0.HighEndBeamPeraltes.Add(6.0);
            var f1 = new PushBackFrontConfig();
            f1.HighEndBeamPeraltes.Add(4.5);
            f1.HighEndBeamPeraltes.Add(5.5);
            design.Fronts.Add(f0);
            design.Fronts.Add(f1);

            design.RearTope.Disable(0, 1);
            return design;
        }
    }
}
