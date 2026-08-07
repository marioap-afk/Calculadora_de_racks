using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using RackCad.UI.Controls;
using RackCad.UI.Shell;

namespace RackCad.UI.Tests
{
    /// <summary>
    /// Helpers to drive the REAL editor windows through their WPF event surface (initiative I-24): a genuine
    /// <see cref="ButtonBase.ClickEvent"/> raised on the actual button runs the window's own <c>*_Click</c> handler
    /// (which does validation → recompute/BuildSystem → SetModel → session → typed payload → Close), exactly as a user
    /// click would — instead of calling <c>session.RequestInsert/RequestUpdate</c> directly and eluding all of that.
    /// No production seams: the buttons and handlers already exist; these only locate the button and raise its Click.
    /// </summary>
    internal static class EditorWindowTestSupport
    {
        /// <summary>Raise a real Click on the button registered under <paramref name="name"/> (XAML x:Name).</summary>
        public static void ClickNamed(Window window, string name)
        {
            var button = window.FindName(name) as ButtonBase
                ?? throw new InvalidOperationException($"No named button '{name}' in {window.GetType().Name}.");
            Click(button);
        }

        /// <summary>Raise a real Click on the first button whose Content equals <paramref name="content"/> (for the
        /// few action buttons that carry no x:Name, e.g. the selective "Insertar frontal").</summary>
        public static void ClickByContent(Window window, string content)
        {
            var button = FindByContent(window, content)
                ?? throw new InvalidOperationException($"No button with content '{content}' in {window.GetType().Name}.");
            Click(button);
        }

        /// <summary>
        /// The «En blanco» box of one front's matrix header (I-33). It carries no x:Name — the headers are built in
        /// code, one per front — so it is located by its grid cell: row 0, column frontIndex + 1. Setting IsChecked
        /// runs the window's real Checked/Unchecked handler, exactly as the user's click would.
        /// </summary>
        public static CheckBox BlankFrontBox(Window window, string matrixGridName, int frontIndex)
        {
            var grid = window.FindName(matrixGridName) as Grid
                ?? throw new InvalidOperationException($"No matrix grid '{matrixGridName}' in {window.GetType().Name}.");
            foreach (var element in grid.Children)
            {
                if (element is FrameworkElement child
                    && Grid.GetRow(child) == 0
                    && Grid.GetColumn(child) == frontIndex + 1)
                {
                    foreach (var descendant in Descendants(child))
                    {
                        if (descendant is CheckBox box)
                        {
                            return box;
                        }
                    }
                }
            }

            throw new InvalidOperationException($"No «En blanco» box for front {frontIndex} in {matrixGridName}.");
        }

        /// <summary>Switch one front between Activo and En blanco through its real box.</summary>
        public static void SetFrontBlank(Window window, string matrixGridName, int frontIndex, bool blank)
            => BlankFrontBox(window, matrixGridName, frontIndex).IsChecked = blank;

        private static IEnumerable<FrameworkElement> Descendants(FrameworkElement element)
        {
            yield return element;
            if (element is Panel panel)
            {
                foreach (var child in panel.Children)
                {
                    if (child is FrameworkElement framework)
                    {
                        foreach (var descendant in Descendants(framework))
                        {
                            yield return descendant;
                        }
                    }
                }
            }
        }

        /// <summary>Type <paramref name="text"/> into the named TextBox, as the user would before pressing a draw button.</summary>
        public static void SetText(Window window, string name, string text)
        {
            if (window.FindName(name) is TextBox box)
            {
                box.Text = text;
            }
        }

        /// <summary>
        /// Type a number into the named <see cref="NumericField"/> and COMMIT it the way a user does — by leaving the
        /// control. The editors wire their fields with <c>LostFocus="Input_Changed"</c>, so raising the real routed
        /// LostFocus is what runs the window's ordinary edit path (commit → recompute), instead of a scope button.
        /// </summary>
        public static void SetNumberAndCommit(Window window, string name, double? value)
        {
            var field = window.FindName(name) as NumericField
                ?? throw new InvalidOperationException($"No NumericField '{name}' in {window.GetType().Name}.");
            field.SetNumber(value);
            field.RaiseEvent(new RoutedEventArgs(UIElement.LostFocusEvent, field));
        }

        private static void Click(ButtonBase button)
            => button.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent, button));

        private static ButtonBase FindByContent(DependencyObject root, string content)
        {
            foreach (var child in LogicalTreeHelper.GetChildren(root))
            {
                if (child is ButtonBase button && (button.Content as string) == content)
                {
                    return button;
                }

                if (child is DependencyObject node)
                {
                    var found = FindByContent(node, content);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }

            // Editors composed over RackEditorVisualShell (I-30/I-31) inject their content into dependency-property SLOTS,
            // which are NOT part of the window's logical tree until the shell's template is applied (i.e. until the window is
            // shown). So when the click helpers run on an unshown, shell-composed editor, also descend into the slot values
            // directly, or a nameless action button (e.g. the selective "Insertar frontal") would be unreachable by content.
            if (root is RackEditorVisualShell shell)
            {
                foreach (var slot in ShellSlots(shell))
                {
                    if (slot is ButtonBase slotButton && (slotButton.Content as string) == content)
                    {
                        return slotButton;
                    }

                    if (slot is DependencyObject slotNode)
                    {
                        var found = FindByContent(slotNode, content);
                        if (found != null)
                        {
                            return found;
                        }
                    }
                }
            }

            return null;
        }

        private static IEnumerable<object> ShellSlots(RackEditorVisualShell shell)
        {
            yield return shell.SidebarHeader;
            yield return shell.SidePanelContent;
            yield return shell.MatrixContent;
            yield return shell.PreviewContent;
            yield return shell.StatusContent;
            yield return shell.LeadingActions;
            yield return shell.SecondaryActions;
            yield return shell.PrimaryActions;
            yield return shell.TrailingActions;
        }

        private static IEnumerable<object> ShellSlots(RackBoundedEditorShell shell)
        {
            yield return shell.Header;
            yield return shell.Parameters;
            yield return shell.SectionPicker;
            yield return shell.Preview;
            yield return shell.Diagnostics;
            yield return shell.BomSummary;
            yield return shell.Actions;
        }

        /// <summary>
        /// Every node under <paramref name="root"/>, descending the logical tree AND the dependency-property slots of
        /// both shells (I-39A, extending what I-31 did for the rich editor shell).
        ///
        /// <para>A shell's slots are not part of the window's logical tree until its template is applied — i.e. until
        /// the window is shown — so a test driving an unshown, shell-composed window would otherwise be unable to
        /// reach any of its content. Walking the slots directly keeps a characterization test blind to WHICH
        /// composition a window uses, which is the whole point when the migration changes exactly that.</para>
        /// </summary>
        public static IEnumerable<DependencyObject> Descendants(DependencyObject root)
        {
            if (root == null)
            {
                yield break;
            }

            yield return root;

            foreach (var child in LogicalTreeHelper.GetChildren(root))
            {
                if (child is DependencyObject node)
                {
                    foreach (var descendant in Descendants(node))
                    {
                        yield return descendant;
                    }
                }
            }

            var slots = root is RackEditorVisualShell rich ? ShellSlots(rich)
                : root is RackBoundedEditorShell bounded ? ShellSlots(bounded)
                : null;

            if (slots == null)
            {
                yield break;
            }

            foreach (var slot in slots)
            {
                if (slot is DependencyObject slotNode)
                {
                    foreach (var descendant in Descendants(slotNode))
                    {
                        yield return descendant;
                    }
                }
            }
        }

        /// <summary>The first descendant of type <typeparamref name="T"/> matching <paramref name="match"/>.</summary>
        public static T Find<T>(DependencyObject root, Func<T, bool> match = null) where T : DependencyObject
            => Descendants(root).OfType<T>().FirstOrDefault(node => match == null || match(node));

        /// <summary>Every descendant of type <typeparamref name="T"/>, in logical order.</summary>
        public static IReadOnlyList<T> FindAll<T>(DependencyObject root) where T : DependencyObject
            => Descendants(root).OfType<T>().ToList();
    }
}
