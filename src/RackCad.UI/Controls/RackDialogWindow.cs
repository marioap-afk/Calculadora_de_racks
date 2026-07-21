using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace RackCad.UI.Controls
{
    /// <summary>The action bar produced by <see cref="RackDialogWindow.CreateActionBar"/>: the panel to place plus the
    /// two standard buttons, exposed so a subclass can validate before accepting or relabel them.</summary>
    public sealed class DialogActionBar
    {
        internal DialogActionBar(Panel panel, Button acceptButton, Button cancelButton)
        {
            Panel = panel;
            AcceptButton = acceptButton;
            CancelButton = cancelButton;
        }

        public Panel Panel { get; }

        public Button AcceptButton { get; }

        public Button CancelButton { get; }
    }

    /// <summary>
    /// The shared base for RackCad dialog windows. It applies the chrome every window repeats by hand today — merge
    /// <c>Themes/AppStyles.xaml</c>, Segoe UI, the shared window background and <see cref="WindowStartupLocation"/> =
    /// CenterOwner — and offers the standard Aceptar/Cancelar action bar and status line. Existing windows are NOT
    /// migrated by I-14 (strangler): this base exists for the editor shell and future dialogs to adopt so the chrome
    /// lives in one place.
    /// </summary>
    public class RackDialogWindow : Window
    {
        public RackDialogWindow()
        {
            FontFamily = new FontFamily("Segoe UI");
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            MergeSharedStyles();

            if (TryFindResource("WindowBackgroundBrush") is Brush background)
            {
                Background = background;
            }
        }

        /// <summary>Forwards to the one status styling (<see cref="UiSupport.SetStatus"/>): red on error, green otherwise.
        /// The subclass owns the <paramref name="target"/> text block and passes it in.</summary>
        protected void SetStatus(TextBlock target, string message, bool isError) => UiSupport.SetStatus(target, message, isError);

        /// <summary>Builds the standard bottom action bar: any <paramref name="leading"/> elements (e.g. Todos/Ninguno)
        /// pinned left, and Aceptar (default) + Cancelar (cancel) right, wired to <see cref="Accept"/>/<see cref="Cancel"/>.</summary>
        protected DialogActionBar CreateActionBar(string acceptText = "Aceptar", string cancelText = "Cancelar", params UIElement[] leading)
        {
            var bar = new DockPanel { LastChildFill = false, Margin = new Thickness(0, 12, 0, 0) };

            if (leading != null)
            {
                foreach (var element in leading)
                {
                    if (element == null)
                    {
                        continue;
                    }

                    DockPanel.SetDock(element, Dock.Left);
                    bar.Children.Add(element);
                }
            }

            var accept = new Button { Content = acceptText, IsDefault = true, MinWidth = 96 };
            ApplyStyle(accept, "PrimaryButtonStyle");
            accept.Click += (_, __) => Accept();

            var cancel = new Button { Content = cancelText, IsCancel = true, MinWidth = 96, Margin = new Thickness(8, 0, 0, 0) };
            ApplyStyle(cancel, "SecondaryButtonStyle");
            cancel.Click += (_, __) => Cancel();

            var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            buttons.Children.Add(accept);
            buttons.Children.Add(cancel);
            DockPanel.SetDock(buttons, Dock.Right);
            bar.Children.Add(buttons);

            return new DialogActionBar(bar, accept, cancel);
        }

        /// <summary>Accepts the dialog. Sets <see cref="Window.DialogResult"/> when shown modally, else just closes.
        /// Override to validate first and call <c>base.Accept()</c> only when valid.</summary>
        protected virtual void Accept()
        {
            try
            {
                DialogResult = true;
            }
            catch (InvalidOperationException)
            {
                Close();
            }
        }

        /// <summary>Cancels the dialog (mirror of <see cref="Accept"/>).</summary>
        protected virtual void Cancel()
        {
            try
            {
                DialogResult = false;
            }
            catch (InvalidOperationException)
            {
                Close();
            }
        }

        private void MergeSharedStyles()
        {
            // AppStyles.xaml is a compiled resource of RackCad.UI, so this URI always resolves at runtime. A failure
            // here is a real packaging/deployment error, not a recoverable condition, and must surface (a swallowed
            // catch would silently drop the shared chrome and hide the cause) rather than be hidden.
            var uri = new Uri("/RackCad.UI;component/Themes/AppStyles.xaml", UriKind.Relative);
            Resources.MergedDictionaries.Add(new ResourceDictionary { Source = uri });
        }

        private void ApplyStyle(FrameworkElement element, string key)
        {
            if (TryFindResource(key) is Style style)
            {
                element.Style = style;
            }
        }
    }
}
