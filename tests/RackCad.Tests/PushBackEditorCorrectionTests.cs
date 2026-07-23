using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems;
using RackCad.Domain.Systems;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-18b increment 2 — corrections: the editor state and design are CANONICAL by themselves (state = design = resolved
    /// system), the low-end/GUIA-free safety authority is shared, a loaded modular structure (custom cabeceras / manual
    /// fondos) survives recompute, the snapshot restores the full selection, and load/reset always land on a deterministic
    /// (0,0) selection. Pure — the real resolve/BOM/plan pipeline runs against the test catalog, no geometry mocks.
    /// </summary>
    public class PushBackEditorCorrectionTests
    {
        private static RackCatalog Catalog => JsonRackCatalogProvider.FromBaseDirectory().Load();

        private static PushBackEditorDesignAssembler Assembler() => new PushBackEditorDesignAssembler(Catalog);

        private static PushBackEditorInputs Inputs(int palletsDeep = 4)
            => new PushBackEditorInputs
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = palletsDeep
            };

        private static DynamicRackDesign SingleFrontStructure(int palletsDeep = 6, int loadLevels = 2)
            => new DynamicRackDesign
            {
                Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                PalletsDeep = palletsDeep,
                LoadLevels = loadLevels,
                FirstLevelHeight = 6.0,
                BeamDepth = 4.0
            };

        private static PushBackEditorState Grid3x3()
        {
            var state = new PushBackEditorState();   // 1 front x 3 levels
            state.SetFrontCount(3);                  // 3 fronts x 3 levels
            return state;
        }

        // ===== 1. Canonicalization: state = design = resolved system ============================================

        [Theory]
        [InlineData(7.0)]   // out of the catalog list
        [InlineData(0.0)]   // unset
        [InlineData(-2.0)]  // negative
        public void Canonicalization_InvalidPeralte_IsThreeFive_InState_Design_AndSystem(double requested)
        {
            var state = new PushBackEditorState();
            state.Cell(0, 0).HighEndBeamPeralte = requested;
            var assembler = Assembler();

            var design = assembler.BuildDesign(state, Inputs());
            var system = assembler.Resolver.Resolve(design);

            Assert.Equal(3.5, state.Cell(0, 0).HighEndBeamPeralte, 4);              // state normalized in place
            Assert.Equal(3.5, design.Fronts[0].HighEndBeamPeraltes[0].Value, 4);    // design already canonical
            Assert.Equal(3.5, system.HighEndBeamPeralteAt(0, 0), 4);                // resolved system agrees
        }

        [Fact]
        public void Canonicalization_ValidPeralte_IsKept_InState_Design_AndSystem()
        {
            var state = new PushBackEditorState();
            state.Cell(0, 0).HighEndBeamPeralte = 5.0;
            var assembler = Assembler();

            var design = assembler.BuildDesign(state, Inputs());
            var system = assembler.Resolver.Resolve(design);

            Assert.Equal(5.0, state.Cell(0, 0).HighEndBeamPeralte, 4);
            Assert.Equal(5.0, design.Fronts[0].HighEndBeamPeraltes[0].Value, 4);
            Assert.Equal(5.0, system.HighEndBeamPeralteAt(0, 0), 4);
        }

        [Fact]
        public void Canonicalization_NonPositiveSaque_BecomesDefault_InStateAndDesign()
        {
            var state = new PushBackEditorState { RearTopeSaque = -5.0 };

            var design = Assembler().BuildDesign(state, Inputs());

            Assert.Equal(PushBackDefaults.RearTopeSaque, state.RearTopeSaque, 4);
            Assert.Equal(PushBackDefaults.RearTopeSaque, design.RearTope.Saque, 4);
        }

        [Fact]
        public void Canonicalization_CatalogWithoutRearBeamRow_ResolvesTo35_Everywhere()
        {
            var noRedondo = CatalogWithoutRearBeam();
            var assembler = new PushBackEditorDesignAssembler(noRedondo);
            var state = new PushBackEditorState();
            state.Cell(0, 0).HighEndBeamPeralte = 5.0;   // would be valid WITH the row

            Assert.Empty(assembler.AllowedHighEndPeraltes());
            var design = assembler.BuildDesign(state, Inputs());
            var system = new PushBackResolver(noRedondo).Resolve(design);

            Assert.Equal(3.5, state.Cell(0, 0).HighEndBeamPeralte, 4);
            Assert.Equal(3.5, design.Fronts[0].HighEndBeamPeraltes[0].Value, 4);
            Assert.Equal(3.5, system.HighEndBeamPeralteAt(0, 0), 4);
        }

        // ===== 2. Shared safety authority: low-end + GUIA-free in Design AND System =============================

        [Fact]
        public void Safety_EntranceBoth_IsLowEndAndGuiaFree_InDesignAndSystem_WithoutMutatingSource()
        {
            var state = new PushBackEditorState();
            var inputs = Inputs();
            var guia = new SelectiveSafetySelection { ElementId = "GUIA_ENTRADA", Quantity = 1, Side = SafetySide.Both };
            var bota = new SelectiveSafetySelection { ElementId = "PROTECTOR_BOTA_H_3_16_18", Quantity = 1, Side = SafetySide.Both };
            inputs.SafetySelections.Add(guia);
            inputs.SafetySelections.Add(bota);

            var comp = Assembler().Build(state, inputs);
            Assert.True(comp.IsValid, comp.Error);

            // Design.Structure.SafetySelections: GUIA-free, Side.Left, independent from the source.
            Assert.DoesNotContain(comp.Design.Structure.SafetySelections, s => s.ElementId == "GUIA_ENTRADA");
            var designBota = comp.Design.Structure.SafetySelections.Single(s => s.ElementId == "PROTECTOR_BOTA_H_3_16_18");
            Assert.Equal(SafetySide.Left, designBota.Side);
            Assert.NotSame(bota, designBota);

            // System.SafetySelections: same guarantees.
            Assert.DoesNotContain(comp.System.SafetySelections, s => s.ElementId == "GUIA_ENTRADA");
            var systemBota = comp.System.SafetySelections.Single(s => s.ElementId == "PROTECTOR_BOTA_H_3_16_18");
            Assert.Equal(SafetySide.Left, systemBota.Side);
            Assert.NotSame(bota, systemBota);

            // The source selection is untouched (deep copies were used).
            Assert.Equal(SafetySide.Both, bota.Side);
            Assert.Equal(2, inputs.SafetySelections.Count);
        }

        // ===== 3. Loaded modular structure (custom cabecera / manual fondo) survives recompute ==================

        [Fact]
        public void CustomHeader_LoadThenBuild_PreservesManualFondo_AsADeepClone()
        {
            var design = CustomFondoDesign(out var fondo);
            var assembler = Assembler();
            var state = new PushBackEditorState();
            var inputs = state.LoadFromDesign(design, assembler.Resolver);

            var comp = assembler.Build(state, inputs);
            Assert.True(comp.IsValid, comp.Error);

            Assert.Contains(comp.Design.Structure.Modules,
                m => m.IsHeader && m.IsManualOverride && Math.Abs(m.Length - fondo) < 1e-6);   // manual fondo preserved
            var sourceHeader = design.Structure.Modules.First(m => m.IsHeader && m.IsManualOverride);
            Assert.DoesNotContain(comp.Design.Structure.Modules, m => ReferenceEquals(m, sourceHeader)); // deep clone
        }

        [Fact]
        public void CustomHeader_MutatingTheResult_DoesNotAlterTheSourceDesign()
        {
            var design = CustomFondoDesign(out var fondo);
            var assembler = Assembler();
            var state = new PushBackEditorState();
            var inputs = state.LoadFromDesign(design, assembler.Resolver);
            var comp = assembler.Build(state, inputs);

            foreach (var module in comp.Design.Structure.Modules)
            {
                module.Length = 999.0;
            }

            var sourceHeader = design.Structure.Modules.First(m => m.IsHeader && m.IsManualOverride);
            Assert.Equal(fondo, sourceHeader.Length, 4);   // source design untouched
        }

        [Fact]
        public void CustomHeader_HeightOnlyChange_UpdatesCalculatedHeaders_AndPreservesManualFondo()
        {
            var design = CustomFondoDesign(out var fondo);
            var assembler = Assembler();
            var state = new PushBackEditorState();
            var inputs = state.LoadFromDesign(design, assembler.Resolver);

            var before = assembler.Build(state, inputs);
            assembler.AcceptComputation(state, before);
            var heightBefore = before.System.Structure.Fronts[0].Height;

            state.AdjustLevels(0, 1);   // height-only: more levels, same pallet and fondos -> no rebuild
            var after = assembler.Build(state, inputs);

            Assert.True(after.IsValid, after.Error);
            Assert.True(after.System.Structure.Fronts[0].Height > heightBefore, "calculated header height must update");
            Assert.Contains(after.Design.Structure.Modules,
                m => m.IsHeader && m.IsManualOverride && Math.Abs(m.Length - fondo) < 1e-6);   // manual fondo preserved
        }

        [Fact]
        public void CustomHeader_StructuralChange_PreservesManualFondosByOrdinal()
        {
            var design = CustomFondoDesign(out var fondo);
            var assembler = Assembler();
            var state = new PushBackEditorState();
            var inputs = state.LoadFromDesign(design, assembler.Resolver);
            var before = assembler.Build(state, inputs);
            assembler.AcceptComputation(state, before);

            inputs.PalletsDeep += 2;   // structural change: forces a rebuild
            var after = assembler.Build(state, inputs);

            Assert.True(after.IsValid, after.Error);
            Assert.Contains(after.Design.Structure.Modules,
                m => m.IsHeader && m.IsManualOverride && Math.Abs(m.Length - fondo) < 1e-6);   // fondo restored by ordinal
        }

        [Fact]
        public void CustomHeader_TwoRecomputesAfterAccept_PreserveTheBaseline()
        {
            var design = CustomFondoDesign(out var fondo);
            var assembler = Assembler();
            var state = new PushBackEditorState();
            var inputs = state.LoadFromDesign(design, assembler.Resolver);

            var first = assembler.Build(state, inputs);
            assembler.AcceptComputation(state, first);
            var second = assembler.Build(state, inputs);
            assembler.AcceptComputation(state, second);
            var third = assembler.Build(state, inputs);

            Assert.True(third.IsValid, third.Error);
            Assert.Contains(third.Design.Structure.Modules,
                m => m.IsHeader && m.IsManualOverride && Math.Abs(m.Length - fondo) < 1e-6);
        }

        [Fact]
        public void FailedComputation_DoesNotReplaceTheBaseline()
        {
            var design = CustomFondoDesign(out _);
            var assembler = Assembler();
            var state = new PushBackEditorState();
            var inputs = state.LoadFromDesign(design, assembler.Resolver);
            var baselineBefore = state.WorkingBaseline;

            var bad = Inputs();
            bad.Pallet = new PalletSpecification(42.0, 0.0, 60.0, 1000.0, "kg");   // zero depth -> structure build throws
            var failed = assembler.Build(state, bad);
            assembler.AcceptComputation(state, failed);

            Assert.False(failed.IsValid);
            Assert.Same(baselineBefore, state.WorkingBaseline);   // a failed compute never replaces the baseline
        }

        // ===== 4. Full selection snapshot ======================================================================

        [Fact]
        public void Snapshot_Restore_RecoversExactPrimaryAndMultiSelection()
        {
            var state = Grid3x3();
            state.ToggleCell(0, 0, false);
            state.ToggleCell(2, 1, true);
            state.ToggleCell(1, 2, true);   // primary = (1,2), set = {(0,0),(2,1),(1,2)}
            Assert.Equal(1, state.Structure.SelectedFrontIndex);
            Assert.Equal(2, state.Structure.SelectedLevelIndex);
            Assert.Equal(3, state.Structure.SelectedCellCount);

            var snapshot = state.Snapshot();
            state.ToggleCell(0, 0, false);   // collapse to a single, different selection
            state.SetFrontCount(1);          // and shrink the structure

            state.Restore(snapshot);

            Assert.Equal(1, state.Structure.SelectedFrontIndex);   // primary recovered exactly
            Assert.Equal(2, state.Structure.SelectedLevelIndex);
            Assert.Equal(3, state.Structure.SelectedCellCount);
            Assert.True(state.Structure.IsSelected(0, 0));
            Assert.True(state.Structure.IsSelected(2, 1));
            Assert.True(state.Structure.IsSelected(1, 2));
        }

        // ===== 5. Reset / load land on a deterministic (0,0) selection =========================================

        [Fact]
        public void LoadNew_AfterMultiSelection_LeavesASingle00Selection_AndStandardBaseline()
        {
            var state = Grid3x3();
            state.ToggleCell(0, 0, false);
            state.ToggleCell(2, 2, true);
            Assert.Equal(2, state.Structure.SelectedCellCount);

            state.LoadNew();

            Assert.Equal(1, state.Structure.Count);
            Assert.Equal(DynamicRackDefaults.DefaultLoadLevels, state.Structure.Fronts[0].LoadLevels);
            Assert.Equal(1, state.Structure.SelectedCellCount);
            Assert.Equal(0, state.Structure.SelectedFrontIndex);
            Assert.Equal(0, state.Structure.SelectedLevelIndex);
            Assert.True(state.Structure.IsSelected(0, 0));
            Assert.All(state.PushFronts[0].Cells, c =>
            {
                Assert.Equal(3.5, c.HighEndBeamPeralte, 4);
                Assert.True(c.RearTopeEnabled);
            });
            Assert.Null(state.WorkingBaseline);
        }

        [Fact]
        public void LoadFromDesign_AfterMultiSelection_SetsDeterministic00Selection()
        {
            var state = Grid3x3();
            state.ToggleCell(0, 0, false);
            state.ToggleCell(2, 2, true);   // stale multi-selection from a previous design

            var design = new PushBackDesign { Structure = SingleFrontStructure(palletsDeep: 6, loadLevels: 2) };
            state.LoadFromDesign(design, new PushBackResolver(Catalog));

            Assert.Equal(1, state.Structure.SelectedCellCount);
            Assert.Equal(0, state.Structure.SelectedFrontIndex);
            Assert.Equal(0, state.Structure.SelectedLevelIndex);
            Assert.True(state.Structure.IsSelected(0, 0));
        }

        // ===== Fixtures ========================================================================================

        /// <summary>A Push Back design whose first (start) header is a MANUAL fondo (a custom cabecera): IsManualOverride with
        /// a distinct Length, so <see cref="DynamicEditorDesignAssembler.SnapshotHeaderFondos"/> captures it by ordinal.</summary>
        private static PushBackDesign CustomFondoDesign(out double manualFondo)
        {
            manualFondo = 55.0;
            var resolver = new PushBackResolver(Catalog);
            var system = resolver.Resolve(new PushBackDesign { Structure = SingleFrontStructure(palletsDeep: 6, loadLevels: 2) });

            var header = system.Structure.Modules.First(module => module.IsHeader);
            header.Length = manualFondo;
            header.IsManualOverride = true;
            header.IsCalculated = false;
            header.UseCalculatedHeaderConfiguration = true;   // custom FONDO, config still regenerated from it

            return resolver.Snapshot(system);
        }

        /// <summary>An independent catalog with the high-end (rear) beam profile removed, so no rear peralte is allowed.</summary>
        private static RackCatalog CatalogWithoutRearBeam()
        {
            var source = Catalog;
            var clone = new RackCatalog();
            foreach (var property in typeof(RackCatalog).GetProperties())
            {
                if (property.CanRead && property.CanWrite)
                {
                    property.SetValue(clone, property.GetValue(source));
                }
            }

            clone.BeamProfiles = source.BeamProfiles
                .Where(beam => !string.Equals(beam?.Id, PushBackDefaults.HighEndBeamCatalogId, StringComparison.OrdinalIgnoreCase))
                .ToList();
            return clone;
        }
    }
}
