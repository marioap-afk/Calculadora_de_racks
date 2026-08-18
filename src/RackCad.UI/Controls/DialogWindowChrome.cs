using System.Windows;
using RackCad.UI.Shell;

namespace RackCad.UI.Controls
{
    /// <summary>
    /// The single source of the ARCHETYPE C chrome (ADR-0029): the shared dictionary plus the window style that
    /// carries background and typography. Ten dialogs repeated the same four statements in their constructors; this
    /// replaces them with one call.
    ///
    /// <para>It is a helper and NOT a base class, and that is the measured conclusion of I-39D rather than a
    /// preference. An ancestor fits four of the ten and not the other six — the four grids put Todos/Ninguno where a
    /// standard bar would not, and one dialog has THREE terminations —, and, worse, a base that assigns background
    /// and typography in its constructor sets them as LOCAL VALUES, which in WPF precedence beat a Style setter: any
    /// future size or appearance contract for the archetype would be unable to change them in its own subclasses.
    /// Composition has neither problem and costs the same one line.</para>
    ///
    /// <para>It carries appearance only. <see cref="Window.WindowStartupLocation"/> stays per window because the
    /// evidence differs — five dialogs are opened by an AutoCAD command with no WPF parent, so no `Owner` can exist —
    /// and size stays per window because two of the ten size themselves to their content and four compute their size
    /// from the data. Imposing common minimums here would reproduce exactly the dead-letter anomaly that I-39A
    /// measured in Cantilever and I-39C closed (ADR-0029 D9).</para>
    /// </summary>
    public static class DialogWindowChrome
    {
        /// <summary>The style key of the archetype, resolved from <c>Themes/AppStyles.xaml</c>.</summary>
        public const string StyleKey = "DialogWindowStyle";

        /// <summary>
        /// Merges the shared dictionary into <paramref name="window"/> and applies the archetype's window style.
        ///
        /// <para>The dictionary goes in FIRST on purpose: a keyed style is only resolvable once the dictionary that
        /// declares it is in scope, and a code-built window has none by default.</para>
        /// </summary>
        public static void Apply(Window window)
        {
            if (window == null)
            {
                return;
            }

            window.Resources.MergedDictionaries.Add(ShellResources.Shared);

            if (window.TryFindResource(StyleKey) is Style style)
            {
                window.Style = style;
            }
        }
    }
}
