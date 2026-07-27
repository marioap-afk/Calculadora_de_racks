using System;
using System.Linq;
using System.Windows.Controls;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.Domain.Systems.Shared;
using RackCad.UI;
using RackCad.UI.Controls;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// PB-013 (I-32) — "Tarima (datos generales)" offered Frente/Alto/Peso as editable fields that changed nothing,
    /// because the CELL is the authority for those three. Owner decision: <b>Fondo and Unidad stay global and editable;
    /// Frente, Alto and Peso belong to the cell</b> and the general panel only MIRRORS the selected cell.
    ///
    /// The mirror has to follow the ordinary editing path too — typing in the cell fields and leaving the control —
    /// not just a click on an "Aplicar a" scope button, which is what made the panel look stale.
    /// </summary>
    public sealed class PushBackPalletScopeTests
    {
        private static NumericField Num(RackPushBackSystemWindow w, string name) => (NumericField)w.FindName(name);
        private static ComboBox Combo(RackPushBackSystemWindow w, string name) => (ComboBox)w.FindName(name);

        private static PushBackDesign SampleDesign()
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = 4,
                    LoadLevels = 2,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            };
            design.Structure.Fronts.Add(new DynamicRackFrontDesign { PalletCount = 1, LoadLevels = 2, PalletsDeep = 4, DepthStartPosition = 1 });
            design.Fronts.Add(new PushBackFrontConfig());
            return design;
        }

        [Fact]
        public void GeneralPallet_FreezesFrenteAltoPeso_AndKeepsFondoAndUnidadEditable()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                return (
                    Num(w, "FrontBox").IsEnabled,
                    Num(w, "PalletHeightBox").IsEnabled,
                    Num(w, "WeightBox").IsEnabled,
                    Num(w, "DepthBox").IsEnabled,
                    Combo(w, "WeightUnitBox").IsEnabled);
            });

            Assert.False(r.Item1);   // Frente: cell-owned, display only
            Assert.False(r.Item2);   // Alto:   cell-owned, display only
            Assert.False(r.Item3);   // Peso:   cell-owned, display only
            Assert.True(r.Item4);    // Fondo:  global, editable
            Assert.True(r.Item5);    // Unidad: global, editable
        }

        /// <summary>
        /// The panel MIRRORS the selected cell after an ordinary edit (type + LostFocus), with no scope button clicked.
        /// This is the path that used to leave the general panel showing a stale number.
        /// </summary>
        [Fact]
        public void GeneralPallet_MirrorsTheSelectedCell_AfterAnOrdinaryCellEdit()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                w.LoadExisting(SampleDesign(), "GUID-PB", "PB");

                var before = (Num(w, "FrontBox").Value, Num(w, "PalletHeightBox").Value, Num(w, "WeightBox").Value);

                EditorWindowTestSupport.SetNumberAndCommit(w, "CellPalletHeightBox", 80.0);
                EditorWindowTestSupport.SetNumberAndCommit(w, "CellPalletFrontBox", 44.0);
                EditorWindowTestSupport.SetNumberAndCommit(w, "CellPalletWeightBox", 1500.0);

                return (
                    before,
                    after: (Num(w, "FrontBox").Value, Num(w, "PalletHeightBox").Value, Num(w, "WeightBox").Value),
                    cellHeight: w.State.Structure.Fronts[0].Cells[0].PalletHeight,
                    rackPallet: w.LastComputation?.Design?.Structure?.Pallet);
            });

            Assert.Equal(60.0, r.before.Item2 ?? -1.0, 6);
            Assert.Equal(80.0, r.cellHeight, 6);

            // The mirror follows the cell.
            Assert.Equal(44.0, r.after.Item1 ?? -1.0, 6);
            Assert.Equal(80.0, r.after.Item2 ?? -1.0, 6);
            Assert.Equal(1500.0, r.after.Item3 ?? -1.0, 6);

            // ...and the rack-wide pallet is NOT rewritten by the mirror: it stays what the design was loaded with, so
            // editing a cell cannot silently change the drawing through the general panel.
            Assert.NotNull(r.rackPallet);
            Assert.Equal(42.0, r.rackPallet.Front, 6);
            Assert.Equal(60.0, r.rackPallet.Height, 6);
            Assert.Equal(1000.0, r.rackPallet.Weight, 6);
        }

        /// <summary>Fondo and Unidad remain rack-wide: editing them still reaches the built design.</summary>
        [Fact]
        public void FondoAndUnidad_StillReachTheDesign()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                w.LoadExisting(SampleDesign(), "GUID-PB", "PB");

                Combo(w, "WeightUnitBox").SelectedItem = "lb";
                EditorWindowTestSupport.SetNumberAndCommit(w, "DepthBox", 40.0);

                return w.LastComputation?.Design?.Structure?.Pallet;
            });

            Assert.NotNull(r);
            Assert.Equal(40.0, r.Depth, 6);
            Assert.Equal("lb", r.WeightUnit);
        }
    }
}
