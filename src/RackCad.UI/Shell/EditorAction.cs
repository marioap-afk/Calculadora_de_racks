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
        public EditorAction(
            string label,
            bool isEnabled = true,
            string disabledReason = null,
            bool isPrimary = false,
            bool isDefault = false,
            bool isCancel = false)
        {
            if (isDefault && isCancel)
            {
                throw new ArgumentException(
                    "An action cannot be both the default and the cancel action: Enter and Escape would do the same thing.",
                    nameof(isDefault));
            }

            Label = label ?? string.Empty;
            IsEnabled = isEnabled;
            DisabledReason = disabledReason;
            IsPrimary = isPrimary;
            IsDefault = isDefault;
            IsCancel = isCancel;
        }

        public string Label { get; }

        public bool IsEnabled { get; }

        /// <summary>Shown as a tooltip while the action is disabled (via <see cref="ToolTipService.ShowOnDisabledProperty"/>).</summary>
        public string DisabledReason { get; }

        public bool IsPrimary { get; }

        /// <summary>
        /// Whether <c>Enter</c> activates this action (ADR-0029 D7). I-39C adds it: without it this model could not
        /// describe a window's keyboard contract at all, which is the measured reason no window adopted
        /// <see cref="EditorActions.Button"/> — substituting hand-built buttons would have SILENTLY dropped Enter.
        ///
        /// <para>Visual prominence and keyboard role stay SEPARATE on purpose: an editor may want a primary-looking
        /// action that Enter must not fire — D7 only allows Enter on an action that is safe and contextual — so this
        /// is not derived from <see cref="IsPrimary"/>.</para>
        /// </summary>
        public bool IsDefault { get; }

        /// <summary>Whether <c>Escape</c> activates this action (ADR-0029 D7). D7 also warns that <c>IsCancel</c> is
        /// not added mechanically to a window with pending edits: declaring it here says the window HAS a cancel
        /// action, not that closing is free — whether it may close is the close policy's business.</summary>
        public bool IsCancel { get; }
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
                Style = ShellResources.Get<Style>(action.IsPrimary ? "PrimaryButtonStyle" : "SecondaryButtonStyle"),

                // I-39C: the keyboard contract travels WITH the action, so a window that builds its bar from these
                // descriptions keeps Enter and Escape instead of losing them on the way.
                IsDefault = action.IsDefault,
                IsCancel = action.IsCancel
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
