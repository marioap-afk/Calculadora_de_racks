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
    /// The severity → foreground brushes the status presenter applies, read from the SINGLE normative source: the
    /// <c>ShellStatus*Brush</c> tokens in <c>Themes/AppStyles.xaml</c> (a compiled resource of RackCad.UI, so they
    /// resolve even when the presenter is instantiated outside a window that merged the dictionary — e.g. in a test).
    /// There is NO duplicated palette here: a missing or mistyped token is a real resource failure and
    /// <see cref="ShellResources.Require{T}"/> throws rather than hiding it behind a hardcoded default.
    /// </summary>
    public static class EditorStatusPalette
    {
        public static Brush Info => ShellResources.Require<Brush>("ShellStatusInfoBrush");
        public static Brush Success => ShellResources.Require<Brush>("ShellStatusSuccessBrush");
        public static Brush Warning => ShellResources.Require<Brush>("ShellStatusWarningBrush");
        public static Brush Error => ShellResources.Require<Brush>("ShellStatusErrorBrush");

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
