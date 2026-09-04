using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.Settings;
using RackCad.UI.Systems.Selective;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// I-43, gate 8.6B (ARQ-43-04): el ÚNICO sitio de la suite que construye <see cref="RackSelectiveWindow"/>.
    /// <para>
    /// El constructor público resuelve un <c>UserSettingsGateway</c> real, de modo que abrir la ventana LEE
    /// <c>%APPDATA%\RackCad\settings.json</c> y tocar los «Fondos destino» lo ESCRIBE. Centralizar la
    /// construcción aquí, siempre con un gateway en memoria, hace la suite hermética: no pisa las preferencias
    /// de quien la ejecuta y ningún test hereda lo que otro dejó escrito.
    /// </para>
    /// <para>
    /// <see cref="SelectiveWindowConstructionGuardTests"/> vigila que nadie vuelva a construirla por su cuenta.
    /// </para>
    /// </summary>
    internal static class SelectiveWindowTestSupport
    {
        /// <summary>
        /// Sustituto en memoria de <c>%APPDATA%\RackCad\settings.json</c>. Sin persistencia física: lo que se
        /// guarda vive solo en la instancia, así que cada test parte de la preferencia que declara y de ninguna
        /// otra.
        /// </summary>
        internal sealed class FakeSettings : IUserSettingsGateway
        {
            public FakeSettings(string stored = null) => Stored = new UserSettings { SelectiveTargetFondos = stored };

            /// <summary>Lo que hay «en disco» ahora mismo; un test puede inspeccionarlo tras un gesto.</summary>
            public UserSettings Stored { get; private set; }

            /// <summary>Cuántas veces se guardó, para los tests que verifican que una elección se recuerda.</summary>
            public int Saves { get; private set; }

            public UserSettings Load() => Stored;

            public void Save(UserSettings settings)
            {
                Stored = settings;
                Saves++;
            }
        }

        /// <summary>
        /// Abre la ventana con <paramref name="fondos"/> fondos, exactamente como hacían los <c>OpenWith</c> de
        /// cada suite: teclear el número y salir del campo. Sin <paramref name="gateway"/> se usa un
        /// <see cref="FakeSettings"/> vacío, es decir, «esta instalación nunca eligió Fondos destino».
        /// </summary>
        internal static RackSelectiveWindow Open(
            int fondos = 1,
            IUserSettingsGateway gateway = null,
            bool canInsertInAutoCad = false)
        {
            var window = new RackSelectiveWindow(canInsertInAutoCad, gateway ?? new FakeSettings());
            if (fondos > 1)
            {
                EditorWindowTestSupport.SetText(window, "FondosBox", fondos.ToString(CultureInfo.InvariantCulture));
                RaiseLostFocus(window, "FondosBox");
            }

            return window;
        }

        /// <summary>Dispara el <c>LostFocus</c> real de un campo, que es el gesto que compromete lo tecleado.</summary>
        internal static void RaiseLostFocus(Window window, string name)
        {
            var box = (TextBox)window.FindName(name);
            box.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, box));
        }
    }
}
