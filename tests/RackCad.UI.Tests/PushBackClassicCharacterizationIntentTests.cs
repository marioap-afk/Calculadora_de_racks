using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Controls;
using RackCad.UI.Editor;
using RackCad.UI.Systems.PushBack;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// F4-T1B (I-64) — CARACTERIZACION de la INTENCION que la ventana CLASICA de Push Back expone y que otro consume
    /// despues (clase B de INV-F4T1B-16): las salidas de Insertar y Actualizar con su senal exacta en el contrato
    /// publico, las guardas que NO cierran, la habilitacion de la barra de acciones, Seguridad cancelada e «Insertar
    /// varias…» antes de su dialogo. CT-07, CT-08, CT-09, CT-17 y CT-23 del paquete v5.
    ///
    /// <para><b>Fija lo que HAY, no lo que ADR-0029 D6 querria</b> (D13). Las rarezas medidas llevan «INCUMPLIMIENTO
    /// CARACTERIZADO» y NO se corrigen: «Insertar varias…» siempre habilitado y sin mirar <c>canInsertInAutoCad</c>
    /// (OD-1), y la confirmacion del modulo antes de un dialogo que luego se puede cancelar (OD-2).</para>
    ///
    /// <para>Ninguna prueba abre un dialogo real: «Insertar varias…» solo se ejerce con entrada invalida o salida
    /// bloqueada, que vuelven ANTES del dialogo, con la ventana sin mostrar; Seguridad entra por su costura. Ninguna
    /// escribe en disco ni toca el Plugin.</para>
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackClassicCharacterizationIntentTests
    {
        private const string NewRackId = "11111111-1111-1111-1111-111111111111";
        private const string AutoCadOnlyStatus = "El dibujo en AutoCAD solo está disponible cuando la ventana se abre desde AutoCAD.";
        private const string UpdateOnlyStatus = "Solo un sistema abierto con RACKEDITAR puede actualizarse en sitio.";
        private const string InvalidModelStatus = "Corrige los datos: no se puede insertar un modelo inválido.";
        private const string InvalidFieldStatus = "Corrige los campos numéricos marcados: Valor no numérico.";
        private const string BlockedStatusWithAStagedModule =
            "Lado A, frente 1, nivel 1: la cama necesita 596\" y la estructura efectiva solo ofrece 404\". Aumenta la estructura de ese lado, el gap o reduce el fondo de la celda.";

        private static ComboBox Combo(Window window, string name) => (ComboBox)window.FindName(name);
        private static CheckBox Check(Window window, string name) => (CheckBox)window.FindName(name);
        private static NumericField Field(Window window, string name) => (NumericField)window.FindName(name);
        private static Button Btn(Window window, string name) => (Button)window.FindName(name);
        private static string Status(Window window) => ((TextBlock)window.FindName("StatusText")).Text;
        private static void Click(ButtonBase button) => button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button));
        private static void LoseFocus(FrameworkElement element) => element.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, element));

        private static RackPushBackSystemWindow Shown()
        {
            var window = new RackPushBackSystemWindow(canInsertInAutoCad: true)
            {
                WindowStartupLocation = WindowStartupLocation.Manual,
                ShowInTaskbar = false,
                ShowActivated = false,
                Left = -10000,
                Top = -10000,
            };
            window.Show();
            window.UpdateLayout();
            return window;
        }

        /// <summary>Escenifica una edicion de modulo SIN confirmarla (el unico ambito que la ventana declara).</summary>
        private static void StageModuleEdit(RackPushBackSystemWindow window)
        {
            Check(window, "AdvancedModulesToggle").IsChecked = true;
            var module = window.EditorStateForTest.ModuleSession.Modules.First(m => m.IsLengthBearing);
            Combo(window, "ModuleBox").SelectedIndex = module.Index;
            var length = Field(window, "ModuleLengthBox");
            length.SetNumber(module.Length + 8.0);
            length.RaiseEvent(new RoutedEventArgs(FrameworkElement.LostFocusEvent));
        }

        /// <summary>Un campo numerico con texto no numerico, comprometido al salir del campo: las entradas pasan a invalidas.</summary>
        private static void MakeInputsInvalid(RackPushBackSystemWindow window)
        {
            var tolerance = Field(window, "ToleranceBox");
            tolerance.Text = "abc";
            LoseFocus(tolerance);
        }

        /// <summary>Un compuesto valido con la estructura del lado A congelada: el fixture natural de las salidas bloqueadas.</summary>
        private static RackPushBackSystemWindow Composite(double structure = 8.0)
        {
            var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
            Check(window, "SideBPresentCheck").IsChecked = true;
            var matrix = window.CompositeState.Of(PushBackSide.B).Structure;
            for (var front = 0; front < matrix.Count; front++)
            {
                matrix.Fronts[front].IsActive = true;
            }

            var gap = Field(window, "GapBox");
            gap.SetNumber(window.CompositeState.Gap);
            LoseFocus(gap);

            var frozen = Field(window, "StructureOverrideBox");
            frozen.SetNumber(structure);
            Click(Btn(window, "ApplyStructureButton"));
            return window;
        }

        /// <summary>La edicion que BLOQUEA la salida y que no es la del modulo: se teclea y no se sale del campo.</summary>
        private static void StageBlockingEdit(RackPushBackSystemWindow window)
            => Field(window, "FondosBox").SetNumber(12.0);

        // ================================================================================================== CT-07

        [Theory]
        [InlineData("InsertLateralButton", 0, "lateral", 0, DimensionViewKind.Lateral)]
        [InlineData("InsertFrontalEntradaButton", 1, "frontal", 0, DimensionViewKind.Frontal)]
        [InlineData("InsertFrontalPosteriorButton", 2, "frontal", 1, DimensionViewKind.Frontal)]
        [InlineData("InsertPlantaButton", 3, "planta", -1, DimensionViewKind.Planta)]
        public void EachPerViewButtonSelectsItsViewAndExposesTheSingleViewIntent(
            string buttonName, int viewIndex, string view, int section, DimensionViewKind kind)
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackPushBackSystemWindow(canInsertInAutoCad: true, newIdFactory: () => NewRackId);
                var closed = false;
                window.Closed += (s, e) => closed = true;
                try
                {
                    Click(Btn(window, buttonName));

                    Assert.True(closed);
                    Assert.True(window.InsertRequested);
                    Assert.False(window.UpdateOnly);
                    Assert.Equal(view, window.InsertView);
                    Assert.Equal(section, window.InsertSection);
                    Assert.Equal(viewIndex, Combo(window, "ViewBox").SelectedIndex);
                    var request = Assert.IsType<PushBackInsertionRequest>(window.InsertionRequest);
                    Assert.Equal(view, request.View);
                    Assert.Equal(section, request.Section);
                    Assert.Single(request.Views);
                    Assert.Equal(kind, request.Views[0].Kind);
                    // El GUID sale de newIdFactory (determinismo): nunca de Guid.NewGuid.
                    Assert.Equal(NewRackId, window.RackId);
                    Assert.Equal(NewRackId, request.RackId);
                }
                finally { if (!closed) { window.Close(); } }
            });
        }

        [Fact]
        public void TheFrontalCutOfSideBFollowsTheFrontalSideSelectorNotTheEditedSide()
        {
            StaTestRunner.Run(() =>
            {
                foreach (var (buttonName, section, end) in new[]
                         {
                             ("InsertFrontalEntradaButton", 2, RackPushBackEnd.EntradaSalida),
                             ("InsertFrontalPosteriorButton", 3, RackPushBackEnd.Posterior),
                         })
                {
                    var window = new RackPushBackSystemWindow(canInsertInAutoCad: true, newIdFactory: () => NewRackId);
                    var closed = false;
                    window.Closed += (s, e) => closed = true;
                    try
                    {
                        Check(window, "SideBPresentCheck").IsChecked = true;
                        Combo(window, "FrontalSideBox").SelectedIndex = 1;

                        Click(Btn(window, buttonName));

                        Assert.True(closed);
                        Assert.Equal(PushBackSide.B, window.FrontalSideForTest);
                        Assert.Equal("frontal", window.InsertView);
                        Assert.Equal(section, window.InsertSection);
                        var request = Assert.IsType<PushBackInsertionRequest>(window.InsertionRequest);
                        Assert.Single(request.Views);
                        Assert.Equal(end, request.Views[0].Variant.PushBackEnd);
                        Assert.Equal(RackPushBackSide.B, request.Views[0].Variant.PushBackSide);
                    }
                    finally { if (!closed) { window.Close(); } }
                }
            });
        }

        // ================================================================================================== CT-08

        [Fact]
        public void TheFirstTwoGuardsShowTheirTextReturnWithoutConfirmingTheStagedModuleAndDoNotClose()
        {
            StaTestRunner.Run(() =>
            {
                // Guarda 1: la ventana no se abrio desde AutoCAD.
                var outsideAutoCad = new RackPushBackSystemWindow(canInsertInAutoCad: false);
                var closedOutside = false;
                outsideAutoCad.Closed += (s, e) => closedOutside = true;
                try
                {
                    StageModuleEdit(outsideAutoCad);
                    Assert.True(outsideAutoCad.PendingWork().HasPendingWork);

                    Click(Btn(outsideAutoCad, "InsertButton"));

                    Assert.Equal(AutoCadOnlyStatus, Status(outsideAutoCad));
                    Assert.False(closedOutside);
                    Assert.False(outsideAutoCad.InsertRequested);
                    // Vuelve SIN confirmar el modulo escenificado.
                    Assert.True(outsideAutoCad.PendingWork().HasPendingWork);
                }
                finally { if (!closedOutside) { outsideAutoCad.Close(); } }

                // Guarda 2: Actualizar sin un sistema abierto con RACKEDITAR.
                var brandNew = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                var closedNew = false;
                brandNew.Closed += (s, e) => closedNew = true;
                try
                {
                    StageModuleEdit(brandNew);
                    Assert.True(brandNew.PendingWork().HasPendingWork);

                    Click(Btn(brandNew, "UpdateButton"));

                    Assert.Equal(UpdateOnlyStatus, Status(brandNew));
                    Assert.False(closedNew);
                    Assert.False(brandNew.InsertRequested);
                    Assert.True(brandNew.PendingWork().HasPendingWork);
                }
                finally { if (!closedNew) { brandNew.Close(); } }
            });
        }

        [Fact]
        public void TheLastTwoGuardsConfirmTheStagedModuleFirstShowTheirDiagnosticAndDoNotClose()
        {
            StaTestRunner.Run(() =>
            {
                // Guarda 3: entrada invalida. Confirma el modulo escenificado ANTES (decision de I-40, OD-2).
                var invalid = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                var closedInvalid = false;
                invalid.Closed += (s, e) => closedInvalid = true;
                try
                {
                    StageModuleEdit(invalid);
                    MakeInputsInvalid(invalid);
                    Assert.True(invalid.PendingWork().HasPendingWork);

                    Click(Btn(invalid, "InsertButton"));

                    Assert.Equal(InvalidModelStatus, Status(invalid));
                    Assert.False(closedInvalid);
                    Assert.False(invalid.InsertRequested);
                    Assert.False(invalid.PendingWork().HasPendingWork);
                }
                finally { if (!closedInvalid) { invalid.Close(); } }

                // Guarda 4: salida bloqueada, con un bloqueo INDEPENDIENTE de la edicion del modulo. El texto es el
                // diagnostico del escenario concreto, no el literal de reserva.
                var blocked = Composite();
                var closedBlocked = false;
                blocked.Closed += (s, e) => closedBlocked = true;
                try
                {
                    StageModuleEdit(blocked);
                    StageBlockingEdit(blocked);
                    Assert.True(blocked.PendingWork().HasPendingWork);

                    Click(Btn(blocked, "InsertButton"));

                    Assert.Equal(BlockedStatusWithAStagedModule, Status(blocked));
                    Assert.False(closedBlocked);
                    Assert.False(blocked.InsertRequested);
                    Assert.True(blocked.OutputIsBlockedForTest);
                    Assert.False(blocked.PendingWork().HasPendingWork);
                }
                finally { if (!closedBlocked) { blocked.Close(); } }
            });
        }

        // ================================================================================================== CT-09

        private static readonly string[] ActionButtons =
        {
            "RestoreButton", "BomButton", "SaveLibraryButton", "UpdateButton", "InsertLateralButton",
            "InsertFrontalEntradaButton", "InsertFrontalPosteriorButton", "InsertPlantaButton", "InsertBatchButton",
            "InsertButton", "CloseButton",
        };

        /// <summary>«IsEnabled/ToolTip» de cada boton de accion, en el orden de la barra.</summary>
        private static string Snapshot(RackPushBackSystemWindow window)
            => string.Join(" | ", ActionButtons.Select(name =>
                name + "=" + Btn(window, name).IsEnabled + "/" + (Btn(window, name).ToolTip ?? "<null>")));

        private const string RestoreTip = "Vuelve todos los valores al último sistema válido.";
        private const string BomTip = "Lista de materiales del sistema actual.";
        private const string SaveTip = "Guarda este diseño Push Back en la biblioteca.";
        private const string BatchTip = "Elige y ordena varias vistas del mismo rack.";
        private const string InsertTip = "Inserta la vista seleccionada enlazada al sistema.";
        private const string OutsideTip = "Disponible solo cuando la ventana se abre desde AutoCAD.";
        private const string NewOnlyTip = "Disponible solo para un sistema abierto con RACKEDITAR.";
        private const string UpdateTip = "Redibuja en sitio todas las vistas del sistema.";

        private static string Expected(bool enabledInsert, string insertTip, bool enabledUpdate, string updateTip)
            => "RestoreButton=True/" + RestoreTip
               + " | BomButton=True/" + BomTip
               + " | SaveLibraryButton=True/" + SaveTip
               + " | UpdateButton=" + enabledUpdate + "/" + updateTip
               + " | InsertLateralButton=" + enabledInsert + "/" + insertTip
               + " | InsertFrontalEntradaButton=" + enabledInsert + "/" + insertTip
               + " | InsertFrontalPosteriorButton=" + enabledInsert + "/" + insertTip
               + " | InsertPlantaButton=" + enabledInsert + "/" + insertTip
               + " | InsertBatchButton=True/" + BatchTip
               + " | InsertButton=" + enabledInsert + "/" + insertTip
               + " | CloseButton=True/<null>";

        [Fact]
        public void TheActionBarStateIsExactForEveryCombinationOfAutoCadAndNewOrExistingRack()
        {
            StaTestRunner.Run(() =>
            {
                var withAutoCadNew = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                var withAutoCadExisting = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                withAutoCadExisting.LoadExisting(withAutoCadExisting.LastComputation.Design, "id-1", "Nombre");
                var withoutAutoCadNew = new RackPushBackSystemWindow(canInsertInAutoCad: false);
                var withoutAutoCadExisting = new RackPushBackSystemWindow(canInsertInAutoCad: false);
                withoutAutoCadExisting.LoadExisting(withoutAutoCadExisting.LastComputation.Design, "id-1", "Nombre");

                Assert.Equal(Expected(true, InsertTip, false, NewOnlyTip), Snapshot(withAutoCadNew));
                Assert.Equal(Expected(true, InsertTip, true, UpdateTip), Snapshot(withAutoCadExisting));
                Assert.Equal(Expected(false, OutsideTip, false, OutsideTip), Snapshot(withoutAutoCadNew));
                Assert.Equal(Expected(false, OutsideTip, false, OutsideTip), Snapshot(withoutAutoCadExisting));
            });
        }

        [Fact]
        public void InsertBatchIsAlwaysEnabledEvenWithInvalidInputs()
        {
            StaTestRunner.Run(() =>
            {
                var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                Assert.True(Btn(window, "InsertBatchButton").IsEnabled);

                MakeInputsInvalid(window);

                Assert.False(window.CurrentInputsAreValid);
                Assert.False(Btn(window, "InsertButton").IsEnabled);
                Assert.False(Btn(window, "BomButton").IsEnabled);
                Assert.False(Btn(window, "SaveLibraryButton").IsEnabled);
                // INCUMPLIMIENTO CARACTERIZADO (ADR-0029 D6, OD-1): «Insertar varias…» no sigue la habilitacion de las
                // demas salidas: ninguna regla la deshabilita.
                Assert.True(Btn(window, "InsertBatchButton").IsEnabled);
            });
        }

        // ================================================================================================== CT-17

        [Fact]
        public void CancellingSafetyChangesNeitherTheSelectionsNorTheRecomputePasses()
        {
            StaTestRunner.Run(() =>
            {
                var window = Shown();
                try
                {
                    var calls = 0;
                    window.SafetyWindowDialog = dialog => { calls++; return false; };
                    var selections = window.SafetySelections.Select(selection => selection.ElementId).ToList();
                    var passes = window.RecomputePassesForTest;
                    Assert.NotEmpty(selections);

                    Click(Btn(window, "SafetyButton"));

                    Assert.Equal(1, calls);
                    Assert.Equal(selections, window.SafetySelections.Select(selection => selection.ElementId).ToList());
                    Assert.Equal(passes, window.RecomputePassesForTest);
                }
                finally
                {
                    window.SafetyWindowDialog = null;
                    window.Close();
                }
            });
        }

        // ================================================================================================== CT-23

        [Fact]
        public void InsertBatchWithAnInvalidFieldConfirmsTheModuleFirstAndStopsBeforeAnyDialog()
        {
            StaTestRunner.Run(() =>
            {
                // La ventana SIN mostrar y con canInsertInAutoCad = false: un escape hacia el dialogo de lote lanzaria en
                // «Owner = owner» en lugar de quedarse esperando un modal.
                var window = new RackPushBackSystemWindow();
                var closed = false;
                window.Closed += (s, e) => closed = true;
                try
                {
                    StageModuleEdit(window);
                    MakeInputsInvalid(window);
                    Assert.True(window.PendingWork().HasPendingWork);

                    Click(Btn(window, "InsertBatchButton"));

                    // INCUMPLIMIENTO CARACTERIZADO (ADR-0029 D6, OD-2): confirma el modulo antes de un camino que puede
                    // acabar en nada, y nunca mira canInsertInAutoCad (OD-1): no aparece el texto de AutoCAD.
                    Assert.False(window.PendingWork().HasPendingWork);
                    Assert.NotEqual(AutoCadOnlyStatus, Status(window));
                    Assert.Equal(InvalidFieldStatus, Status(window));
                    Assert.False(window.CurrentInputsAreValid);
                    Assert.False(closed);
                    Assert.False(window.InsertRequested);
                    Assert.Null(window.InsertionRequest);
                }
                finally { if (!closed) { window.Close(); } }
            });
        }

        [Fact]
        public void InsertBatchWithABlockedOutputConfirmsTheModuleFirstAndStopsBeforeAnyDialog()
        {
            StaTestRunner.Run(() =>
            {
                var window = Composite();
                var closed = false;
                window.Closed += (s, e) => closed = true;
                try
                {
                    StageModuleEdit(window);
                    StageBlockingEdit(window);
                    Assert.True(window.PendingWork().HasPendingWork);

                    Click(Btn(window, "InsertBatchButton"));

                    Assert.False(window.PendingWork().HasPendingWork);
                    Assert.Equal(BlockedStatusWithAStagedModule, Status(window));
                    Assert.True(window.OutputIsBlockedForTest);
                    Assert.False(closed);
                    Assert.False(window.InsertRequested);
                    Assert.Null(window.InsertionRequest);
                }
                finally { if (!closed) { window.Close(); } }
            });
        }
    }
}
