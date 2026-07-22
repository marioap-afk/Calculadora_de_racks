using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

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

        /// <summary>Type <paramref name="text"/> into the named TextBox, as the user would before pressing a draw button.</summary>
        public static void SetText(Window window, string name, string text)
        {
            if (window.FindName(name) is TextBox box)
            {
                box.Text = text;
            }
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

            return null;
        }
    }
}
