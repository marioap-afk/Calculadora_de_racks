using System.Windows;
using System.Windows.Controls;

namespace RackCad.UI.Shell
{
    /// <summary>
    /// Presents an <see cref="EditorStatusMessage"/> as a single styled line, colored by severity
    /// (<see cref="EditorStatusPalette"/>). It is a THIN presenter: no system knowledge, no geometry/BOM/persistence,
    /// and it does NOT replace <see cref="UiSupport.SetStatus"/> in the existing windows — it is the common status
    /// surface a migrated editor will drop into the shell's <c>StatusContent</c> slot. An empty or null message
    /// collapses the control (no leftover height), so a shell with nothing to say shows no status band.
    /// </summary>
    public class EditorStatusPresenter : ContentControl
    {
        private readonly TextBlock text;

        public static readonly DependencyProperty MessageProperty = DependencyProperty.Register(
            nameof(Message), typeof(EditorStatusMessage), typeof(EditorStatusPresenter),
            new PropertyMetadata(null, OnMessageChanged));

        public EditorStatusPresenter()
        {
            text = new TextBlock { TextWrapping = TextWrapping.Wrap };
            Content = text;
            Apply(null);
        }

        /// <summary>The message to present. Null or empty text collapses the presenter.</summary>
        public EditorStatusMessage Message
        {
            get => (EditorStatusMessage)GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        /// <summary>The current severity being presented (Info when there is no message) — a test seam.</summary>
        public EditorStatusSeverity CurrentSeverity => Message?.Severity ?? EditorStatusSeverity.Info;

        private static void OnMessageChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
            => ((EditorStatusPresenter)d).Apply(e.NewValue as EditorStatusMessage);

        private void Apply(EditorStatusMessage message)
        {
            if (message == null || message.IsEmpty)
            {
                text.Text = string.Empty;
                Visibility = Visibility.Collapsed; // nothing to say → no band, no leftover height
                return;
            }

            text.Text = message.Text;
            text.Foreground = EditorStatusPalette.For(message.Severity);
            Visibility = Visibility.Visible;
        }
    }
}
