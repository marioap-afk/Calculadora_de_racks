using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.UI.Systems.Selective;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// Drives the REAL "Fondos destino" dropdown of the selective editor (I-43, gate 8A). The selector is a popup of
    /// check boxes rather than a text field, so tests toggle the same boxes a user does; the list is rebuilt after
    /// every toggle, so each one is looked up again instead of being cached.
    /// </summary>
    internal static class SelectiveTargetsTestSupport
    {
        /// <summary>Set the target fondos to exactly <paramref name="oneBased"/>, as the user would: pick the ones
        /// wanted, then drop the rest. Wanted-first matters — emptying the set would fall back to the visible fondo.</summary>
        public static void SetTargets(RackSelectiveWindow window, params int[] oneBased)
        {
            Toggle(window, "Actual", true); // start from a known single-fondo state
            foreach (var fondo in oneBased) Toggle(window, "Fondo " + fondo, true);

            var count = window.EditorState.FondoCount;
            for (var fondo = 1; fondo <= count; fondo++)
            {
                if (!oneBased.Contains(fondo)) Toggle(window, "Fondo " + fondo, false);
            }
        }

        /// <summary>Pick the "Todos" entry.</summary>
        public static void SetAllTargets(RackSelectiveWindow window) => Toggle(window, "Todos", true);

        /// <summary>Pick the "Actual" entry (the default: only the fondo on screen).</summary>
        public static void SetCurrentTarget(RackSelectiveWindow window) => Toggle(window, "Actual", true);

        /// <summary>The dropdown's closed caption, which is what the user reads without opening it.</summary>
        public static string Caption(RackSelectiveWindow window)
            => ((ToggleButton)window.FindName("TargetFondosButton")).Content as string;

        private static void Toggle(RackSelectiveWindow window, string content, bool wanted)
        {
            var host = (StackPanel)window.FindName("TargetFondosList");
            var box = host.Children.OfType<CheckBox>().FirstOrDefault(c => (c.Content as string) == content);
            if (box == null || (box.IsChecked == true) == wanted) return;
            box.IsChecked = wanted; // raises the real Checked/Unchecked handler
        }
    }
}
