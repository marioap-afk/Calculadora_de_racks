using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.Domain.Systems;

namespace RackCad.UI
{
    /// <summary>
    /// Owner decision (2026-07-24, final) — the rear stop is configured INSIDE the visible "Elementos de seguridad"
    /// experience, as its own section: a heading, the current state and a "Configurar…" button.
    ///
    /// It is NOT a <see cref="SelectiveSafetySelection"/> and never becomes ordinary low-end safety: the section edits a
    /// WORKING COPY of <see cref="PushBackRearTopeConfig"/> (only <c>Saque</c> and <c>OffCells</c>) that the host applies
    /// together with the authorized safety when the main dialog is accepted, and discards when it is cancelled.
    ///
    /// The tope grid opens ONLY from this button — never automatically after pressing Aceptar.
    /// </summary>
    internal sealed class PushBackRearTopeSection
    {
        private readonly TextBlock status;
        private readonly Func<PushBackRearTopeConfig, SafetyTopeGridWindow.TopeResult> openDialog;

        public PushBackRearTopeSection(
            PushBackRearTopeConfig current,
            Func<PushBackRearTopeConfig, SafetyTopeGridWindow.TopeResult> openDialog)
        {
            this.openDialog = openDialog ?? throw new ArgumentNullException(nameof(openDialog));

            // A working COPY: nothing the user does here touches the editor's state until the main dialog is accepted.
            Config = Copy(current);

            var heading = new TextBlock
            {
                Text = HeadingText,
                FontWeight = FontWeights.SemiBold,
                Margin = new Thickness(0, 0, 0, 2),
            };

            status = new TextBlock
            {
                Text = StatusText(Config),
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap,
                Foreground = new SolidColorBrush(Color.FromRgb(0x61, 0x70, 0x80)),
                Margin = new Thickness(0, 0, 0, 4),
            };

            Button = new Button
            {
                Name = ConfigureButtonName,
                Content = ConfigureButtonText,
                HorizontalAlignment = HorizontalAlignment.Left,
                Padding = new Thickness(10, 3, 10, 3),
            };
            Button.Click += (_, __) => Configure();

            var panel = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
            panel.Children.Add(heading);
            panel.Children.Add(status);
            panel.Children.Add(Button);
            View = panel;
        }

        /// <summary>The section heading shown inside the safety dialog.</summary>
        public const string HeadingText = "Topes posteriores";

        public const string ConfigureButtonText = "Configurar…";

        /// <summary>x:Name-equivalent of the button, so a test can find it inside the composed dialog.</summary>
        public const string ConfigureButtonName = "RearTopeConfigureButton";

        /// <summary>The element the host hands to <see cref="SelectiveSafetyWindow"/>.</summary>
        public UIElement View { get; }

        public Button Button { get; }

        /// <summary>The working copy the host applies on accept. Never the caller's instance.</summary>
        public PushBackRearTopeConfig Config { get; private set; }

        /// <summary>True once the user actually edited the stop through the grid dialog.</summary>
        public bool Edited { get; private set; }

        /// <summary>Opens the shared tope grid and folds its result into the working copy. Cancelling changes nothing.</summary>
        public void Configure()
        {
            var result = openDialog(Config);
            if (result == null)
            {
                return;
            }

            PushBackRearTopeDialogAdapter.Apply(result, Config);
            Edited = true;
            status.Text = StatusText(Config);
        }

        /// <summary>The human-readable state: the SAQUE and how many cells are deactivated.</summary>
        public static string StatusText(PushBackRearTopeConfig config)
        {
            var saque = PushBackRearTopeDialogAdapter.Saque(config);
            var off = config?.OffCells?.Count ?? 0;
            return off == 0
                ? string.Format(CultureInfo.CurrentCulture, "Activos en todas las celdas · SAQUE {0:0.##}\"", saque)
                : string.Format(CultureInfo.CurrentCulture, "{0} celda(s) sin tope · SAQUE {1:0.##}\"", off, saque);
        }

        private static PushBackRearTopeConfig Copy(PushBackRearTopeConfig source)
        {
            var copy = new PushBackRearTopeConfig { Saque = PushBackRearTopeDialogAdapter.Saque(source) };
            foreach (var cell in source?.OffCells ?? new List<SelectiveGridCell>())
            {
                copy.OffCells.Add(new SelectiveGridCell { Frente = cell.Frente, Level = cell.Level });
            }

            return copy;
        }
    }
}
