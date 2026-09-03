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
            Press(window, "Actual"); // start from a known state: FollowCurrent, every fondo box empty
            foreach (var fondo in oneBased) Toggle(window, "Fondo " + fondo, true);

            var count = window.EditorState.FondoCount;
            for (var fondo = 1; fondo <= count; fondo++)
            {
                if (!oneBased.Contains(fondo)) Toggle(window, "Fondo " + fondo, false);
            }
        }

        /// <summary>Press the "Todos" action.</summary>
        public static void SetAllTargets(RackSelectiveWindow window) => Press(window, "Todos");

        /// <summary>Press the "Actual" action (the default: only the fondo on screen, and it follows it).</summary>
        public static void SetCurrentTarget(RackSelectiveWindow window) => Press(window, "Actual");

        /// <summary>Tick or untick one fondo box, as a user does.</summary>
        public static void ToggleFondo(RackSelectiveWindow window, int oneBased, bool wanted)
            => Toggle(window, "Fondo " + oneBased, wanted);

        /// <summary>Press one of the two ACTION entries at the top of the popup ("Actual" / "Todos"); their captions
        /// carry a ✓ when they are the mode in force.</summary>
        private static void Press(RackSelectiveWindow window, string label)
        {
            var host = (StackPanel)window.FindName("TargetFondosList");
            var button = host.Children.OfType<Button>()
                .First(b => (b.Content as string) == label || (b.Content as string) == "✓ " + label);
            button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button));
        }

        /// <summary>The dropdown's closed caption, which is what the user reads without opening it.</summary>
        public static string Caption(RackSelectiveWindow window)
            => ((ToggleButton)window.FindName("TargetFondosButton")).Content as string;

        /// <summary>The fondo numbers whose box reads as ticked — the popup's visible truth.</summary>
        public static int[] CheckedFondos(RackSelectiveWindow window)
        {
            var host = (StackPanel)window.FindName("TargetFondosList");
            return host.Children.OfType<CheckBox>()
                .Where(c => c.IsChecked == true && (c.Content as string ?? string.Empty).StartsWith("Fondo "))
                .Select(c => int.Parse(((string)c.Content).Substring("Fondo ".Length)))
                .OrderBy(n => n)
                .ToArray();
        }

        private static void Toggle(RackSelectiveWindow window, string content, bool wanted)
        {
            var host = (StackPanel)window.FindName("TargetFondosList");
            var box = host.Children.OfType<CheckBox>().FirstOrDefault(c => (c.Content as string) == content);
            if (box == null || (box.IsChecked == true) == wanted) return;
            box.IsChecked = wanted; // raises the real Checked/Unchecked handler
        }
    }
}
