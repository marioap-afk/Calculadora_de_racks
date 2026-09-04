using System;
using System.Windows;

namespace RackCad.UI.Systems.Selective
{
    /// <summary>
    /// El aviso de altura de una cabecera personalizada, detrás de una costura reemplazable (I-43, gate 8.6E).
    /// <para>
    /// Existe por la misma razón que <see cref="RackCad.UI.Shell.EditorDiscardPrompt"/>: <c>MessageBox.Show</c> no
    /// tiene costura, así que una validación construida sobre él sería imposible de probar — y una validación que no
    /// se puede probar es justo la que acaba dejando pasar una cabecera por debajo del nivel de carga. Deliberadamente
    /// diminuta: dos delegados y un ámbito para sustituirlos. Sin framework, sin contenedor, sin paquete nuevo.
    /// </para>
    /// <para>
    /// Dos formas, porque el contrato distingue dos gravedades: una SEVERA pide confirmación y puede cancelarse; una
    /// INFORMATIVA solo informa y el flujo continúa.
    /// </para>
    /// </summary>
    public static class SelectiveCabeceraHeightPrompt
    {
        private static Func<string, Window, bool> confirmSevere = ShowSevere;
        private static Action<string, Window> showInformative = ShowInformative;

        /// <summary>True cuando el usuario decide aplicar la cabecera de todos modos.</summary>
        public static bool ConfirmSevere(string message, Window owner = null) => confirmSevere(message, owner);

        /// <summary>Informa de diferencias no bloqueantes; el flujo sigue en cualquier caso.</summary>
        public static void Inform(string message, Window owner = null) => showInformative(message, owner);

        /// <summary>Sustituye ambos avisos mientras dure el ámbito devuelto. Solo para pruebas.</summary>
        public static IDisposable Substitute(Func<string, bool> onSevere, Action<string> onInformative = null)
        {
            if (onSevere == null) throw new ArgumentNullException(nameof(onSevere));

            var previousSevere = confirmSevere;
            var previousInformative = showInformative;
            confirmSevere = (message, _) => onSevere(message);
            showInformative = (message, _) => onInformative?.Invoke(message);
            return new Restore(() =>
            {
                confirmSevere = previousSevere;
                showInformative = previousInformative;
            });
        }

        // Mismos parametros que los dos MessageBox que "Personalizar" ya mostraba, para que enrutarlos por esta
        // costura no cambie lo que el usuario ve.
        private static bool ShowSevere(string message, Window owner)
            => (owner == null
                ? MessageBox.Show(message, "Cabecera demasiado baja", MessageBoxButton.YesNo, MessageBoxImage.Warning)
                : MessageBox.Show(owner, message, "Cabecera demasiado baja", MessageBoxButton.YesNo, MessageBoxImage.Warning))
            == MessageBoxResult.Yes;

        private static void ShowInformative(string message, Window owner)
        {
            if (owner == null) MessageBox.Show(message, "Altura de cabecera", MessageBoxButton.OK, MessageBoxImage.Warning);
            else MessageBox.Show(owner, message, "Altura de cabecera", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private sealed class Restore : IDisposable
        {
            private readonly Action undo;

            internal Restore(Action undo) => this.undo = undo;

            public void Dispose() => undo();
        }
    }
}
