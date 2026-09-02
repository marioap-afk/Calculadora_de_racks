using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.PushBack;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.PushBack;
using RackCad.Domain.Systems.Selective;
using RackCad.UI.Controls;

namespace RackCad.UI.Systems.PushBack
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

        /// <param name="sideLabel">
        /// I-42 (ronda 7B) — el lado al que pertenece esta seccion, o null en un rack de un solo sentido, que es
        /// como se construia hasta ahora. Un rack COMPUESTO tiene un tope por lado y los dos se editan aqui, uno al
        /// lado del otro: era la unica capacidad que la ventana de seguridad no cubria, y por la que existia una
        /// segunda superficie en la ventana principal.
        /// </param>
        public PushBackRearTopeSection(
            PushBackRearTopeConfig current,
            Func<PushBackRearTopeConfig, SafetyTopeGridWindow.TopeResult> openDialog,
            RackCatalog catalog = null,
            string sideLabel = null)
        {
            this.openDialog = openDialog ?? throw new ArgumentNullException(nameof(openDialog));

            // A working COPY: nothing the user does here touches the editor's state until the main dialog is accepted.
            Config = Copy(current);

            var heading = new TextBlock
            {
                Text = string.IsNullOrEmpty(sideLabel) ? HeadingText : HeadingText + " — lado " + sideLabel,
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

            // PB-005 (I-32): the stop TYPE is picked from the catalog's TOPE family instead of being hard-wired to one
            // piece. The list is whatever the catalog declares, so a new variant needs no code; the selection is written
            // into the working copy as it changes, and the host applies that copy on accept like the SAQUE and the cells.
            var variants = SelectiveSafetyFamilies.VariantsOfType(
                catalog?.SafetyElements, SelectiveSafetyDefaults.TopeType);
            PieceBox = new CatalogCombo
            {
                Name = PieceBoxName,
                Margin = new Thickness(0, 0, 0, 6),
                ToolTip = "Tipo de tope posterior. La lista viene del catálogo de elementos de seguridad.",
            };
            // I-42 (ronda 7C, defecto del dueño) — «NINGUNO» es una opcion EXPLICITA del mismo selector, no la
            // ausencia de eleccion. Es del OBJETIVO de esta seccion: en un compuesto hay una seccion por lado, asi
            // que elegirlo en A no dice nada de B. Viaja en la copia de trabajo como el resto y se persiste como
            // cualquier id, de modo que sobrevive a guardar y a RACKEDITAR. Apagar celda por celda sigue estando:
            // esto no lo sustituye, lo resume — y no borra esa mascara, asi que volver a una pieza la devuelve.
            PieceBox.SetCatalogEntries(
                variants,
                Config.IsNone ? PushBackRearTopeConfig.NonePieceId : PushBackRearTopeBuilder.ResolvePieceId(catalog, Config),
                placeholder: new CatalogOption(PushBackRearTopeConfig.NonePieceId, NoneOptionText));
            PieceBox.SelectionChanged += (_, __) =>
            {
                Config.PieceId = PieceBox.SelectedId;
                status.Text = StatusText(Config);
            };

            var panel = new StackPanel { Margin = new Thickness(0, 0, 0, 12) };
            panel.Children.Add(heading);
            panel.Children.Add(status);
            if (variants.Count > 0 || Config.IsNone)
            {
                panel.Children.Add(new TextBlock
                {
                    Text = PieceLabelText,
                    FontSize = 11,
                    Margin = new Thickness(0, 2, 0, 2),
                });
                panel.Children.Add(PieceBox);
            }

            panel.Children.Add(Button);
            View = panel;
        }

        /// <summary>The section heading shown inside the safety dialog.</summary>
        public const string HeadingText = "Topes posteriores";

        public const string ConfigureButtonText = "Configurar…";

        /// <summary>Label above the stop-type selector.</summary>
        public const string PieceLabelText = "Tipo de tope";

        /// <summary>I-42 (ronda 7C) — el texto de la opcion «sin tope» dentro del selector de tipo.</summary>
        public const string NoneOptionText = "Ninguno";

        /// <summary>Lo que la seccion muestra cuando el objetivo quedo sin tope.</summary>
        public const string NoneStatusText = "Sin tope posterior en este objetivo";

        /// <summary>x:Name-equivalent of the stop-type combo, so a test can find it inside the composed dialog.</summary>
        public const string PieceBoxName = "RearTopePieceBox";

        /// <summary>The stop-type selector (PB-005). Present only when the catalog declares TOPE variants.</summary>
        public CatalogCombo PieceBox { get; }

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
            if (config != null && config.IsNone)
            {
                return NoneStatusText;
            }

            var saque = PushBackRearTopeDialogAdapter.Saque(config);
            var off = config?.OffCells?.Count ?? 0;
            return off == 0
                ? string.Format(CultureInfo.CurrentCulture, "Activos en todas las celdas · SAQUE {0:0.##}\"", saque)
                : string.Format(CultureInfo.CurrentCulture, "{0} celda(s) sin tope · SAQUE {1:0.##}\"", off, saque);
        }

        private static PushBackRearTopeConfig Copy(PushBackRearTopeConfig source)
        {
            // PB-005: the chosen variant travels with the working copy. Without it, opening Seguridad and pressing
            // Aceptar without touching anything would silently reset the stop back to the default piece.
            var copy = new PushBackRearTopeConfig
            {
                Saque = PushBackRearTopeDialogAdapter.Saque(source),
                PieceId = source?.PieceId,
            };
            foreach (var cell in source?.OffCells ?? new List<SelectiveGridCell>())
            {
                copy.OffCells.Add(new SelectiveGridCell { Frente = cell.Frente, Level = cell.Level });
            }

            return copy;
        }
    }
}
