using System;
using System.Windows;
using System.Windows.Controls;

namespace RackCad.UI.Shell
{
    /// <summary>
    /// A pure description of one editor action: a label, whether it is enabled, the reason to show while it is
    /// disabled, and whether it should read as a primary call-to-action. No system knowledge, no command wiring — the
    /// editor supplies the callback when it builds the button (<see cref="EditorActions.Button"/>). This is the only
    /// "action model" the foundation defines; there is deliberately NO per-system action enum: the editor decides what
    /// each action does and in which category it lives.
    /// </summary>
    public sealed class EditorAction
    {
        public EditorAction(string label, bool isEnabled = true, string disabledReason = null, bool isPrimary = false)
        {
            Label = label ?? string.Empty;
            IsEnabled = isEnabled;
            DisabledReason = disabledReason;
            IsPrimary = isPrimary;
        }

        public string Label { get; }

        public bool IsEnabled { get; }

        /// <summary>Shown as a tooltip while the action is disabled (via <see cref="ToolTipService.ShowOnDisabledProperty"/>).</summary>
        public string DisabledReason { get; }

        public bool IsPrimary { get; }
    }

    /// <summary>Builds WPF buttons from <see cref="EditorAction"/> descriptions, applying the shared button styles and
    /// the "tooltip visible while disabled" rule. A helper, not a hardcoded operation list.</summary>
    public static class EditorActions
    {
        /// <summary>A button for <paramref name="action"/> wired to <paramref name="onClick"/>. Primary actions get
        /// <c>PrimaryButtonStyle</c>, the rest <c>SecondaryButtonStyle</c> (both from AppStyles). A disabled action
        /// keeps a visible tooltip carrying its <see cref="EditorAction.DisabledReason"/>.</summary>
        public static Button Button(EditorAction action, RoutedEventHandler onClick = null)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            var button = new Button
            {
                Content = action.Label,
                IsEnabled = action.IsEnabled,
                Margin = new Thickness(ShellResources.Get("ShellActionBarSpacing", 6.0) / 2.0, 0.0, ShellResources.Get("ShellActionBarSpacing", 6.0) / 2.0, 0.0),
                Style = ShellResources.Get<Style>(action.IsPrimary ? "PrimaryButtonStyle" : "SecondaryButtonStyle")
            };

            // The disabled reason must remain readable even while the button is disabled.
            ToolTipService.SetShowOnDisabled(button, true);
            if (!string.IsNullOrWhiteSpace(action.DisabledReason))
            {
                button.ToolTip = action.DisabledReason;
            }

            if (onClick != null)
            {
                button.Click += onClick;
            }

            return button;
        }
    }
}
