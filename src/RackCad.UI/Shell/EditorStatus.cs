using System.Windows.Media;

namespace RackCad.UI.Shell
{
    /// <summary>
    /// The severity of an editor status message. Ordinary status text today is only "error / ok"
    /// (<see cref="UiSupport.SetStatus"/>); the shell foundation adds a four-level scale so a presenter can style a
    /// warning differently from an error. Pure — no WPF dependency.
    /// </summary>
    public enum EditorStatusSeverity
    {
        Info,
        Success,
        Warning,
        Error
    }

    /// <summary>
    /// One editor status message: its text and severity. Pure model (no WPF, no system), so the presenter and the
    /// tests share it. An empty (or null) <see cref="Text"/> is the "nothing to say" state — the presenter collapses.
    /// </summary>
    public sealed class EditorStatusMessage
    {
        public EditorStatusMessage(string text, EditorStatusSeverity severity)
        {
            Text = text ?? string.Empty;
            Severity = severity;
        }

        public string Text { get; }

        public EditorStatusSeverity Severity { get; }

        public bool IsEmpty => string.IsNullOrWhiteSpace(Text);

        public static EditorStatusMessage None { get; } = new EditorStatusMessage(string.Empty, EditorStatusSeverity.Info);

        public static EditorStatusMessage Info(string text) => new EditorStatusMessage(text, EditorStatusSeverity.Info);

        public static EditorStatusMessage Success(string text) => new EditorStatusMessage(text, EditorStatusSeverity.Success);

        public static EditorStatusMessage Warning(string text) => new EditorStatusMessage(text, EditorStatusSeverity.Warning);

        public static EditorStatusMessage Error(string text) => new EditorStatusMessage(text, EditorStatusSeverity.Error);
    }

    /// <summary>
    /// The severity → foreground colors the status presenter applies. These MIRROR the <c>ShellStatus*Brush</c> tokens
    /// in <c>Themes/AppStyles.xaml</c> (so the presenter renders consistently even when instantiated outside a window
    /// that merged the dictionary — e.g. in a test). Colors are derived from vigente values: error #B00020 and success
    /// #2F855A come from <see cref="UiSupport"/>; info #2B6CB0 is the primary-button chrome blue; warning #B7791F is the
    /// single new chrome amber, kept distinct from <see cref="Controls.PreviewPalette"/>'s stroke amber (#E08A2B).
    /// </summary>
    public static class EditorStatusPalette
    {
        public static readonly Brush Info = UiSupport.FrozenBrush(Color.FromRgb(0x2B, 0x6C, 0xB0));
        public static readonly Brush Success = UiSupport.FrozenBrush(Color.FromRgb(0x2F, 0x85, 0x5A));
        public static readonly Brush Warning = UiSupport.FrozenBrush(Color.FromRgb(0xB7, 0x79, 0x1F));
        public static readonly Brush Error = UiSupport.FrozenBrush(Color.FromRgb(0xB0, 0x00, 0x20));

        public static Brush For(EditorStatusSeverity severity)
        {
            switch (severity)
            {
                case EditorStatusSeverity.Success: return Success;
                case EditorStatusSeverity.Warning: return Warning;
                case EditorStatusSeverity.Error: return Error;
                default: return Info;
            }
        }
    }
}
