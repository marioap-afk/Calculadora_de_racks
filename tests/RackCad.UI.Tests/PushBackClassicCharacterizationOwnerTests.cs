using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Systems.PushBack;
using RackCad.UI.Views;
using Xunit;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// F4-T1B (I-64) — CARACTERIZACION de los duenos y de la ubicacion de la ventana CLASICA de Push Back
    /// (<see cref="RackPushBackSystemWindow"/>) y de los subdialogos con costura de instancia. CT-12, CT-13 y CT-14
    /// del paquete v5.
    ///
    /// <para><b>Fija lo que HAY, no lo que ADR-0029 D9 querria</b> (D13): hoy la ventana no tiene <c>Owner</c> (los
    /// comandos RACKPUSHBACK y RACKEDITAR la muestran con <c>ShowModalWindow</c> sin dueno WPF) y sus subdialogos
    /// la tienen a ELLA como dueno, tambien el de las botas aunque la ventana de seguridad este abierta (OD-5).</para>
    ///
    /// <para>Las propiedades de la ventana se leen ANTES de mostrarla y los subdialogos se leen AL ENTRAR en su
    /// costura, antes de que nadie pueda reescribirlos. Ninguna prueba muestra un dialogo modal: las costuras
    /// devuelven «cancelar» y ningun <c>Assert</c> va dentro de ellas.</para>
    /// </summary>
    [Collection(EditorDiscardPromptCollection.Name)]
    public sealed class PushBackClassicCharacterizationOwnerTests
    {
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

        private static void Click(ButtonBase button) => button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button));

        // ================================================================================================== CT-12

        [Fact]
        public void TheWindowHasNoOwnerAndCentersOnItsOwnerWithTheStyleSizes()
        {
            StaTestRunner.Run(() =>
            {
                // Todo se lee ANTES de mostrar la ventana y antes de que cualquier prueba reescriba la ubicacion.
                var window = new RackPushBackSystemWindow(canInsertInAutoCad: true);
                try
                {
                    Assert.Equal("Sistema Push Back - editor", window.Title);

                    // Ancla: el Owner no se asigna en ningun sitio de apertura de la ventana (RACKPUSHBACK y RACKEDITAR
                    // usan ShowModalWindow sin dueno WPF).
                    Assert.Null(window.Owner);
                    Assert.Equal(WindowStartupLocation.CenterOwner, window.WindowStartupLocation);
                    Assert.True(window.ShowInTaskbar);
                    Assert.Equal(ResizeMode.CanResize, window.ResizeMode);
                    Assert.Equal(SizeToContent.Manual, window.SizeToContent);
                    Assert.Equal(1280.0, window.Width);
                    Assert.Equal(720.0, window.Height);
                    Assert.Equal(1120.0, window.MinWidth);
                    Assert.Equal(672.0, window.MinHeight);
                }
                finally { window.Close(); }
            });
        }

        // ================================================================================================== CT-13

        [Fact]
        public void TheSafetyDialogIsOwnedByTheWindowAndCentersOnItsOwner()
        {
            StaTestRunner.Run(() =>
            {
                var window = Shown();
                try
                {
                    var calls = 0;
                    Window owner = null;
                    var location = WindowStartupLocation.Manual;
                    window.SafetyWindowDialog = dialog =>
                    {
                        calls++;
                        owner = dialog.Owner;
                        location = dialog.WindowStartupLocation;
                        return false;
                    };

                    Click((Button)window.FindName("SafetyButton"));

                    Assert.Equal(1, calls);
                    Assert.Equal(WindowStartupLocation.CenterOwner, location);
                    // Ancla: el dueno lo asigna el codigo y ningun mutante permitido lo toca.
                    Assert.Same(window, owner);
                }
                finally
                {
                    window.SafetyWindowDialog = null;
                    window.Close();
                }
            });
        }

        [Fact]
        public void ThePerPostBootDialogIsOwnedByTheWindowNotByTheOpenSafetyDialog()
        {
            StaTestRunner.Run(() =>
            {
                var window = Shown();
                try
                {
                    var safetyCalls = 0;
                    var bootCalls = 0;
                    Window safetyDialog = null;
                    Window bootOwner = null;
                    var bootLocation = WindowStartupLocation.Manual;
                    window.SafetyWindowDialog = dialog =>
                    {
                        safetyCalls++;
                        safetyDialog = dialog;
                        window.BootPerPostWindowDialog = bootDialog =>
                        {
                            bootCalls++;
                            bootOwner = bootDialog.Owner;
                            bootLocation = bootDialog.WindowStartupLocation;
                            return false;
                        };
                        Click(window.BootSectionForTest.Button);
                        window.BootPerPostWindowDialog = null;
                        return false;
                    };

                    Click((Button)window.FindName("SafetyButton"));

                    Assert.Equal(1, safetyCalls);
                    Assert.Equal(1, bootCalls);
                    Assert.Equal(WindowStartupLocation.CenterOwner, bootLocation);
                    // INCUMPLIMIENTO CARACTERIZADO (ADR-0029 D9, OD-5): con la ventana de seguridad abierta, el dueno de la
                    // rejilla por poste es la ventana Push Back y no la de seguridad. Ancla: nadie puede cambiar el dueno
                    // con un mutante permitido.
                    Assert.Same(window, bootOwner);
                    Assert.NotSame(safetyDialog, bootOwner);
                }
                finally
                {
                    window.SafetyWindowDialog = null;
                    window.BootPerPostWindowDialog = null;
                    window.Close();
                }
            });
        }

        // ================================================================================================== CT-14

        [Fact]
        public void TheBatchDialogCentersOnItsOwnerWithInsertarColaAsDefaultAndCancelarAsCancel()
        {
            StaTestRunner.Run(() =>
            {
                var presenter = new RackViewBatchDialogPresenter(
                    RackSystemKind.PushBack,
                    new[]
                    {
                        new RackViewBatchOption("Frontal · entrada/salida A", RackViewAddress.PushBackCut(RackPushBackEnd.EntradaSalida, RackPushBackSide.A)),
                        new RackViewBatchOption("Planta", RackViewAddress.Whole(DimensionViewKind.Planta)),
                    });
                var dialog = new RackViewBatchDialog(presenter);
                try
                {
                    Assert.Equal(WindowStartupLocation.CenterOwner, dialog.WindowStartupLocation);
                    Assert.Null(dialog.Owner);

                    var buttons = EditorWindowTestSupport.FindAll<Button>(dialog);
                    Assert.Equal(new[] { "Insertar cola" }, buttons.Where(button => button.IsDefault).Select(button => (string)button.Content).ToArray());
                    Assert.Equal(new[] { "Cancelar" }, buttons.Where(button => button.IsCancel).Select(button => (string)button.Content).ToArray());
                }
                finally { dialog.Close(); }
            });
        }
    }
}
