using System;
using System.Linq;
using System.Windows.Controls;
using RackCad.Application.Systems.PushBack;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Controls;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-41 (PB-015 / PB-016) — la ventana real de Push Back. Conduce el flujo por la superficie WPF verdadera
    /// (escribir en el campo y salir, marcar la casilla, pulsar los botones de alcance) para fijar:
    /// <list type="bullet">
    /// <item>el fondo propio y la tarima se editan por CELDA y llegan al modelo construido;</item>
    /// <item>los alcances de I-41 escriben UNA sola propiedad y NO arrastran el resto de la celda origen;</item>
    /// <item>una operacion masiva produce UNA sola recomputacion;</item>
    /// <item>restaurar devuelve al default del frente / al legacy false;</item>
    /// <item>un frente en blanco deshabilita los dos controles y conserva su configuracion dormida (I-33);</item>
    /// <item>cancelar no deja rastro y el preview sigue la ultima recomputacion valida (contrato I-39).</item>
    /// </list>
    /// </summary>
    public sealed class PushBackCellConfigurationWindowTests
    {
        private static NumericField Num(RackPushBackSystemWindow w, string name) => (NumericField)w.FindName(name);
        private static CheckBox Check(RackPushBackSystemWindow w, string name) => (CheckBox)w.FindName(name);
        private static ComboBox Combo(RackPushBackSystemWindow w, string name) => (ComboBox)w.FindName(name);

        /// <summary>Indices del desplegable de alcance: Celda, Seleccion, Nivel, Frente, Todo.</summary>
        private const int ScopeCell = 0;
        private const int ScopeSelected = 1;
        private const int ScopeLevel = 2;
        private const int ScopeFront = 3;
        private const int ScopeAll = 4;

        private static PushBackDesign SampleDesign(int fronts = 2, int levels = 3, int palletsDeep = 4)
        {
            var design = new PushBackDesign
            {
                Structure = new DynamicRackDesign
                {
                    Pallet = new PalletSpecification(42.0, 48.0, 60.0, 1000.0, "kg"),
                    PalletsDeep = palletsDeep,
                    LoadLevels = levels,
                    FirstLevelHeight = 6.0,
                    BeamDepth = 4.0
                }
            };
            for (var front = 0; front < fronts; front++)
            {
                design.Structure.Fronts.Add(new DynamicRackFrontDesign
                {
                    PalletCount = 1,
                    LoadLevels = levels,
                    PalletsDeep = palletsDeep,
                    DepthStartPosition = 1
                });
                design.Fronts.Add(new PushBackFrontConfig());
            }

            return design;
        }

        private static RackPushBackSystemWindow Loaded(int fronts = 2, int levels = 3, int palletsDeep = 4)
        {
            var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
            window.LoadExisting(SampleDesign(fronts, levels, palletsDeep), "GUID-PB-I41", "PB I-41");
            return window;
        }

        // ---- Los dos controles existen y reflejan la celda ---------------------------------------------------

        [Fact]
        public void TheCellPanel_OffersItsOwnFondoAndPalletControls()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = Loaded();
                return (
                    fondo: Num(w, "CellFondoOverrideBox") != null,
                    pallet: Check(w, "CellDrawPalletCheck") != null,
                    scope: Combo(w, "CellPropertyScopeBox")?.Items.Count ?? 0,
                    // Vacio de salida: la celda hereda, no impone.
                    fondoValue: Num(w, "CellFondoOverrideBox").Value,
                    palletChecked: Check(w, "CellDrawPalletCheck").IsChecked == true);
            });

            Assert.True(r.fondo);
            Assert.True(r.pallet);
            Assert.Equal(5, r.scope);        // los CINCO alcances existentes, ni uno mas
            Assert.Null(r.fondoValue);
            Assert.False(r.palletChecked);   // default legacy
        }

        [Fact]
        public void EditingTheCellFondo_ReachesTheBuiltDesign_ThroughTheOrdinaryPath()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = Loaded(fronts: 1, levels: 3, palletsDeep: 4);
                w.SelectMatrixCell(0, 1, false);
                EditorWindowTestSupport.SetNumberAndCommit(w, "CellFondoOverrideBox", 6.0);

                var system = w.LastComputation?.System;
                return (
                    override1: w.State.Cell(0, 1).PalletsDeepOverride,
                    effective0: system?.EffectivePalletsDeepAt(0, 0),
                    effective1: system?.EffectivePalletsDeepAt(0, 1),
                    envelope: system?.Structure.Fronts[0].PalletsDeep);
            });

            Assert.Equal(6, r.override1);
            Assert.Equal(4, r.effective0);
            Assert.Equal(6, r.effective1);
            Assert.Equal(6, r.envelope);   // la estructura la dimensiona el nivel mas profundo
        }

        [Fact]
        public void TickingDrawPallet_ReachesTheBuiltSystem_ThroughTheOrdinaryPath()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = Loaded(fronts: 1, levels: 2);
                w.SelectMatrixCell(0, 1, false);
                Check(w, "CellDrawPalletCheck").IsChecked = true;   // dispara el handler real

                var system = w.LastComputation?.System;
                return (cell: w.State.Cell(0, 1).DrawPallet, at0: system?.DrawPalletAt(0, 0), at1: system?.DrawPalletAt(0, 1));
            });

            Assert.True(r.cell);
            Assert.False(r.at0);
            Assert.True(r.at1);
        }

        // ---- Los alcances de I-41 escriben UNA sola propiedad ------------------------------------------------

        [Fact]
        public void ApplyingTheFondoToTheFront_WritesOnlyTheFondo()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = Loaded(fronts: 1, levels: 2, palletsDeep: 4);

                // Dos celdas deliberadamente distintas en todo lo demas.
                w.SelectMatrixCell(0, 1, false);
                EditorWindowTestSupport.SetNumberAndCommit(w, "CellClearBox", 14.0);
                w.SelectMatrixCell(0, 0, false);
                EditorWindowTestSupport.SetNumberAndCommit(w, "CellClearBox", 8.0);

                Combo(w, "CellPropertyScopeBox").SelectedIndex = ScopeFront;
                Num(w, "CellFondoOverrideBox").SetNumber(6.0);
                EditorWindowTestSupport.ClickNamed(w, "ApplyCellFondoButton");

                return (
                    deep0: w.State.Cell(0, 0).PalletsDeepOverride,
                    deep1: w.State.Cell(0, 1).PalletsDeepOverride,
                    clear1: w.State.Structure.Fronts[0].Cells[1].ClearHeight,
                    pallet1: w.State.Cell(0, 1).DrawPallet);
            });

            Assert.Equal(6, r.deep0);
            Assert.Equal(6, r.deep1);
            // El claro de la celda destino NO fue pisado por el de la celda origen.
            Assert.Equal(14.0, r.clear1, 6);
            // Y la tarima tampoco viajo de propina.
            Assert.False(r.pallet1);
        }

        [Fact]
        public void ApplyingThePalletToAll_WritesOnlyThePalletFlag()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = Loaded(fronts: 2, levels: 2, palletsDeep: 4);
                w.SelectMatrixCell(1, 1, false);
                EditorWindowTestSupport.SetNumberAndCommit(w, "CellFondoOverrideBox", 7.0);

                w.SelectMatrixCell(0, 0, false);
                Check(w, "CellDrawPalletCheck").IsChecked = true;
                Combo(w, "CellPropertyScopeBox").SelectedIndex = ScopeAll;
                EditorWindowTestSupport.ClickNamed(w, "ApplyCellPalletButton");

                return (
                    all: Enumerable.Range(0, 2).SelectMany(f => Enumerable.Range(0, 2).Select(l => w.State.Cell(f, l).DrawPallet)).ToList(),
                    // El override de fondo de (1,1) sigue en pie: la aplicacion de tarima no lo toco.
                    deep11: w.State.Cell(1, 1).PalletsDeepOverride);
            });

            Assert.All(r.all, Assert.True);
            Assert.Equal(7, r.deep11);
        }

        [Fact]
        public void ApplyingToTheSelection_UsesTheSameCtrlClickMultiSelection()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = Loaded(fronts: 2, levels: 3, palletsDeep: 4);
                w.SelectMatrixCell(0, 0, false);
                w.SelectMatrixCell(1, 2, true);      // Ctrl + clic

                Combo(w, "CellPropertyScopeBox").SelectedIndex = ScopeSelected;
                Num(w, "CellFondoOverrideBox").SetNumber(5.0);
                EditorWindowTestSupport.ClickNamed(w, "ApplyCellFondoButton");

                return (
                    a: w.State.Cell(0, 0).PalletsDeepOverride,
                    b: w.State.Cell(1, 2).PalletsDeepOverride,
                    c: w.State.Cell(0, 1).PalletsDeepOverride,
                    d: w.State.Cell(1, 0).PalletsDeepOverride);
            });

            Assert.Equal(5, r.a);
            Assert.Equal(5, r.b);
            Assert.Null(r.c);
            Assert.Null(r.d);
        }

        [Fact]
        public void ApplyingToTheLevel_CrossesEveryFront()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = Loaded(fronts: 2, levels: 3, palletsDeep: 4);
                w.SelectMatrixCell(0, 1, false);
                Combo(w, "CellPropertyScopeBox").SelectedIndex = ScopeLevel;
                Check(w, "CellDrawPalletCheck").IsChecked = true;
                EditorWindowTestSupport.ClickNamed(w, "ApplyCellPalletButton");

                return (
                    f0l1: w.State.Cell(0, 1).DrawPallet,
                    f1l1: w.State.Cell(1, 1).DrawPallet,
                    f0l0: w.State.Cell(0, 0).DrawPallet,
                    f1l2: w.State.Cell(1, 2).DrawPallet);
            });

            Assert.True(r.f0l1);
            Assert.True(r.f1l1);
            Assert.False(r.f0l0);
            Assert.False(r.f1l2);
        }

        // ---- Una operacion masiva = UNA recomputacion --------------------------------------------------------

        [Fact]
        public void AMassApplication_TriggersExactlyOneRecompute()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = Loaded(fronts: 3, levels: 3, palletsDeep: 4);
                w.SelectMatrixCell(0, 0, false);

                var before = w.LastComputation;
                var passesBefore = w.RecomputePassesForTest;

                Combo(w, "CellPropertyScopeBox").SelectedIndex = ScopeAll;
                Num(w, "CellFondoOverrideBox").SetNumber(6.0);
                EditorWindowTestSupport.ClickNamed(w, "ApplyCellFondoButton");

                return (
                    recomputes: w.RecomputePassesForTest - passesBefore,
                    changed: !ReferenceEquals(before, w.LastComputation),
                    cells: w.State.Structure.Count * 3);
            });

            Assert.Equal(9, r.cells);          // 3 frentes x 3 niveles reescritos...
            Assert.Equal(1, r.recomputes);     // ...con UNA sola recomputacion
            Assert.True(r.changed);
        }

        // ---- Restauracion -----------------------------------------------------------------------------------

        [Fact]
        public void RestoringTheFondo_ClearsTheOverrideAndTheBox()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = Loaded(fronts: 1, levels: 2, palletsDeep: 4);
                w.SelectMatrixCell(0, 0, false);
                EditorWindowTestSupport.SetNumberAndCommit(w, "CellFondoOverrideBox", 7.0);

                Combo(w, "CellPropertyScopeBox").SelectedIndex = ScopeCell;
                EditorWindowTestSupport.ClickNamed(w, "RestoreCellFondoButton");

                var system = w.LastComputation?.System;
                return (
                    stored: w.State.Cell(0, 0).PalletsDeepOverride,
                    box: Num(w, "CellFondoOverrideBox").Value,
                    effective: system?.EffectivePalletsDeepAt(0, 0),
                    envelope: system?.Structure.Fronts[0].PalletsDeep);
            });

            Assert.Null(r.stored);
            Assert.Null(r.box);
            Assert.Equal(4, r.effective);   // vuelve al default del frente
            Assert.Equal(4, r.envelope);    // y la envolvente baja con el
        }

        [Fact]
        public void RestoringThePallet_ReturnsToTheLegacyFalse()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = Loaded(fronts: 1, levels: 2);
                w.SelectMatrixCell(0, 0, false);
                Check(w, "CellDrawPalletCheck").IsChecked = true;
                Combo(w, "CellPropertyScopeBox").SelectedIndex = ScopeAll;
                EditorWindowTestSupport.ClickNamed(w, "ApplyCellPalletButton");
                var marked = w.State.Cell(0, 1).DrawPallet;

                EditorWindowTestSupport.ClickNamed(w, "RestoreCellPalletButton");

                return (marked, after0: w.State.Cell(0, 0).DrawPallet, after1: w.State.Cell(0, 1).DrawPallet,
                    box: Check(w, "CellDrawPalletCheck").IsChecked == true);
            });

            Assert.True(r.marked);
            Assert.False(r.after0);
            Assert.False(r.after1);
            Assert.False(r.box);
        }

        // ---- El panel refleja la celda seleccionada ---------------------------------------------------------

        [Fact]
        public void SelectingAnotherCell_ReloadsItsOwnFondoAndPallet()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = Loaded(fronts: 1, levels: 2, palletsDeep: 4);
                w.SelectMatrixCell(0, 0, false);
                EditorWindowTestSupport.SetNumberAndCommit(w, "CellFondoOverrideBox", 6.0);
                Check(w, "CellDrawPalletCheck").IsChecked = true;

                w.SelectMatrixCell(0, 1, false);
                var other = (Num(w, "CellFondoOverrideBox").Value, Check(w, "CellDrawPalletCheck").IsChecked == true);

                w.SelectMatrixCell(0, 0, false);
                var back = (Num(w, "CellFondoOverrideBox").Value, Check(w, "CellDrawPalletCheck").IsChecked == true);
                return (other, back);
            });

            Assert.Null(r.other.Item1);     // la otra celda no heredo nada
            Assert.False(r.other.Item2);
            Assert.Equal(6.0, r.back.Item1 ?? -1.0, 6);
            Assert.True(r.back.Item2);
        }

        // ---- Frente en blanco (I-33) ------------------------------------------------------------------------

        [Fact]
        public void ABlankFront_DisablesBothControls_AndKeepsTheConfigurationDormant()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = Loaded(fronts: 2, levels: 2, palletsDeep: 4);
                w.SelectMatrixCell(1, 0, false);
                EditorWindowTestSupport.SetNumberAndCommit(w, "CellFondoOverrideBox", 6.0);
                Check(w, "CellDrawPalletCheck").IsChecked = true;

                EditorWindowTestSupport.BlankFrontBox(w, "PushBackMatrixGrid", 1).IsChecked = true;
                var disabled = (
                    Num(w, "CellFondoOverrideBox").IsEnabled,
                    Check(w, "CellDrawPalletCheck").IsEnabled,
                    Combo(w, "CellPropertyScopeBox").IsEnabled);

                EditorWindowTestSupport.BlankFrontBox(w, "PushBackMatrixGrid", 1).IsChecked = false;
                w.SelectMatrixCell(1, 0, false);
                return (disabled, deep: w.State.Cell(1, 0).PalletsDeepOverride, pallet: w.State.Cell(1, 0).DrawPallet);
            });

            Assert.False(r.disabled.Item1);
            Assert.False(r.disabled.Item2);
            Assert.False(r.disabled.Item3);
            // La configuracion volvio intacta al reactivar.
            Assert.Equal(6, r.deep);
            Assert.True(r.pallet);
        }

        // ---- Contrato I-39: cancelar, dirty y preview -------------------------------------------------------

        [Fact]
        public void ClosingWithoutInserting_LeavesNoResult_AndDoesNotTouchTheDrawing()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = Loaded(fronts: 1, levels: 2, palletsDeep: 4);
                w.SelectMatrixCell(0, 0, false);
                EditorWindowTestSupport.SetNumberAndCommit(w, "CellFondoOverrideBox", 6.0);
                Check(w, "CellDrawPalletCheck").IsChecked = true;

                w.Close();
                return (
                    insertRequested: w.Session.InsertRequested,
                    request: w.Session.InsertionRequest,
                    dialog: w.DialogResult);
            });

            Assert.False(r.insertRequested);   // cancelar no produce peticion de dibujo
            Assert.Null(r.request);
            Assert.True(r.dialog != true);
        }

        [Fact]
        public void ThePreview_FollowsTheLastValidRecompute_AfterAMassApplication()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = Loaded(fronts: 1, levels: 2, palletsDeep: 4);
                w.SelectMatrixCell(0, 0, false);
                Check(w, "CellDrawPalletCheck").IsChecked = true;
                Combo(w, "CellPropertyScopeBox").SelectedIndex = ScopeAll;
                EditorWindowTestSupport.ClickNamed(w, "ApplyCellPalletButton");

                var plan = w.CurrentPreviewPlan;
                return (
                    valid: w.CurrentInputsAreValid,
                    pallets: plan?.Flatten().Instances.Count(i => i.Role == RackCad.Application.Drawing.HeaderBlockRole.Pallet) ?? 0);
            });

            Assert.True(r.valid);
            Assert.True(r.pallets > 0, "el preview debe mostrar las tarimas que la ultima recomputacion valida produjo");
        }

        [Fact]
        public void AnInvalidFondo_BlocksTheRecompute_WithoutTouchingTheModel()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = Loaded(fronts: 1, levels: 2, palletsDeep: 4);
                w.SelectMatrixCell(0, 0, false);
                var before = w.LastComputation;

                // 1 esta por debajo del minimo del control (2): el campo queda en error.
                EditorWindowTestSupport.SetNumberAndCommit(w, "CellFondoOverrideBox", 1.0);

                return (
                    hasError: Num(w, "CellFondoOverrideBox").HasError,
                    valid: w.CurrentInputsAreValid,
                    sameModel: ReferenceEquals(before, w.LastComputation),
                    stored: w.State.Cell(0, 0).PalletsDeepOverride);
            });

            Assert.True(r.hasError);
            Assert.False(r.valid);
            Assert.True(r.sameModel);   // el modelo valido anterior se conserva como referencia
            Assert.Null(r.stored);      // y nada se escribio en el estado
        }

        // ---- La matriz informa del fondo efectivo -----------------------------------------------------------

        [Fact]
        public void TheMatrixCard_ShowsTheCellsEffectiveFondo_AndMarksAnOverride()
        {
            var r = StaTestRunner.Run(() =>
            {
                var w = Loaded(fronts: 1, levels: 2, palletsDeep: 4);
                w.SelectMatrixCell(0, 1, false);
                EditorWindowTestSupport.SetNumberAndCommit(w, "CellFondoOverrideBox", 6.0);
                Check(w, "CellDrawPalletCheck").IsChecked = true;

                return (
                    inherited: PushBackMatrixCardModelProbe.CardText(w, 0, 0),
                    overridden: PushBackMatrixCardModelProbe.CardText(w, 0, 1));
            });

            Assert.Contains("4F ", r.inherited);
            Assert.DoesNotContain("*", r.inherited);
            Assert.Contains("6F*", r.overridden);
            Assert.Contains("tarima", r.overridden);
        }
    }

    /// <summary>Acceso de prueba al texto de una tarjeta, sin exponerlo desde produccion.</summary>
    internal static class PushBackMatrixCardModelProbe
    {
        public static string CardText(RackPushBackSystemWindow window, int frontIndex, int levelIndex)
            => (string)typeof(RackPushBackSystemWindow).Assembly
                .GetType("RackCad.UI.Systems.PushBack.PushBackMatrixCardModel")
                .GetMethod("CardText", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static)
                .Invoke(null, new object[] { window.State, frontIndex, levelIndex });
    }
}
