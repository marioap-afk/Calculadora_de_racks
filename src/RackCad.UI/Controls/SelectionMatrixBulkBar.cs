using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace RackCad.UI.Controls
{
    /// <summary>
    /// The shared "Aplicar a:" row of a safety grid (I-34 / PB-007): an Activar/Desactivar choice plus one button per
    /// scope the dialog DECLARED — Celda, the row axis, the column axis ("Frente" or "Poste"), and Todo.
    /// <para>
    /// It is the only WPF surface of the bulk edit, so the three grids share one look and one behaviour and none of
    /// them branches on a system: everything system-specific arrives through the
    /// <see cref="SelectionMatrixBulkEditor"/> the dialog builds (its labels and its declared scopes). Nothing here
    /// knows what a rack, a frente or a poste is.
    /// </para>
    /// <para>
    /// <see cref="Attach"/> wires the primary cell to the grid: the last cell the user clicked becomes the anchor of
    /// the row/column scopes. A scope that cannot be applied is DISABLED with the reason in its tooltip — never offered
    /// as enabled-but-inert.
    /// </para>
    /// </summary>
    public sealed class SelectionMatrixBulkBar : WrapPanel
    {
        private const string ActivateText = "Activar";
        private const string DeactivateText = "Desactivar";

        private readonly SelectionMatrixBulkEditor editor;
        private readonly RadioButton activate;
        private readonly RadioButton deactivate;
        private readonly Dictionary<SelectionMatrixScope, Button> buttons =
            new Dictionary<SelectionMatrixScope, Button>();
        private readonly TextBlock primaryStatus;

        public SelectionMatrixBulkBar(SelectionMatrixBulkEditor editor)
        {
            this.editor = editor ?? throw new ArgumentNullException(nameof(editor));

            Orientation = Orientation.Horizontal;
            Margin = new Thickness(0, 8, 0, 0);

            Children.Add(Caption("Aplicar a:", new Thickness(0, 0, 8, 0)));

            // The two states are mutually exclusive, so they are radio buttons and never a tri-state "invertir":
            // flipping 40 heterogeneous cells is not an expressible intent and would break idempotence (contract §7.2).
            var group = "SelectionMatrixBulkBar_" + Guid.NewGuid().ToString("N");
            // DESACTIVAR is the default because the grids open all-ON and the motivating case of PB-007 is subtractive:
            // "quitar el desviador del segundo nivel en 100 frentes".
            activate = StateOption(ActivateText, group, isChecked: false);
            deactivate = StateOption(DeactivateText, group, isChecked: true);
            activate.Checked += (_, __) => Refresh();   // the tooltips say "Activa…"/"Desactiva…", so they follow it
            deactivate.Checked += (_, __) => Refresh();
            Children.Add(activate);
            Children.Add(deactivate);

            Children.Add(Caption("|", new Thickness(6, 0, 6, 0)));

            foreach (var scope in editor.Scopes)
            {
                var target = scope;
                var button = new Button
                {
                    Style = TryFindResource("SecondaryButtonStyle") as Style,
                    Content = editor.LabelFor(target),
                    Padding = new Thickness(10, 3, 10, 3),
                    Margin = new Thickness(0, 2, 6, 2),
                    VerticalAlignment = VerticalAlignment.Center
                };
                // I-39D (ADR-0029 D6): el motivo de bloqueo se calcula y se asigna al ToolTip, pero era ILEGIBLE
                // justo cuando importaba, porque WPF no muestra la ayuda de un control deshabilitado si nadie lo
                // pide. Era la unica cobertura de D6 en el arquetipo C y estaba a medias: la prueba aseveraba que el
                // motivo EXISTE, no que se pueda leer.
                ToolTipService.SetShowOnDisabled(button, true);

                button.Click += (_, __) => Apply(target);
                buttons[target] = button;
                Children.Add(button);
            }

            primaryStatus = Caption(string.Empty, new Thickness(4, 0, 0, 0));
            Children.Add(primaryStatus);

            // The dialogs build this row BEFORE putting it in the window, so the app resources are not reachable yet
            // (the same lazy lookup SelectionMatrix.HeaderText does). Re-apply once the row is really in the tree.
            Loaded += (_, __) => ApplyThemeStyles();

            Refresh();
        }

        /// <summary>Raised after an application that actually changed something, with the same aggregated payload the
        /// model published. A no-op raises nothing, exactly like <see cref="SelectionMatrixModel.ScopeApplied"/>.</summary>
        public event EventHandler<SelectionMatrixScopeAppliedEventArgs> Applied;

        public SelectionMatrixBulkEditor Editor => editor;

        /// <summary>Whether the next application ACTIVATES (true) or DEACTIVATES (false) the scope.</summary>
        public bool Activates => activate.IsChecked == true;

        /// <summary>
        /// Follows <paramref name="matrix"/>: every cell the user clicks becomes the primary one, and the buttons
        /// re-evaluate. This is the whole wiring an adopting dialog needs.
        /// </summary>
        public void Attach(SelectionMatrix matrix)
        {
            if (matrix == null)
            {
                return;
            }

            matrix.CellInteracted += (_, e) =>
            {
                editor.TrySetPrimary(e.Cell.Column, e.Cell.Row);
                Refresh();
            };
        }

        /// <summary>Re-evaluates every button's enabled state and tooltip against the editor. Cheap and idempotent;
        /// it never rebuilds the row.</summary>
        public void Refresh()
        {
            foreach (var pair in buttons)
            {
                var reason = editor.DisabledReason(pair.Key);
                pair.Value.IsEnabled = reason == null;
                pair.Value.ToolTip = reason ?? EnabledTooltip(pair.Key);
            }

            primaryStatus.Text = PrimaryText();
        }

        /// <summary>Applies <paramref name="scope"/> with the current Activar/Desactivar state.</summary>
        public IReadOnlyList<SelectionMatrixCell> Apply(SelectionMatrixScope scope)
        {
            var activates = Activates;
            var changed = editor.Apply(scope, activates);
            if (changed.Count > 0)
            {
                Applied?.Invoke(this, new SelectionMatrixScopeAppliedEventArgs(scope, changed, activates));
            }

            Refresh();
            return changed;
        }

        /// <summary>The button of a scope, or null when the dialog did not declare it (test seam).</summary>
        internal Button ButtonFor(SelectionMatrixScope scope)
            => buttons.TryGetValue(scope, out var button) ? button : null;

        internal RadioButton ActivateOption => activate;

        internal RadioButton DeactivateOption => deactivate;

        internal TextBlock PrimaryStatus => primaryStatus;

        /// <summary>Picks up the window's shared styles once this row is in the visual tree.</summary>
        private void ApplyThemeStyles()
        {
            if (TryFindResource("SecondaryButtonStyle") is Style buttonStyle)
            {
                foreach (var button in buttons.Values)
                {
                    button.Style = buttonStyle;
                }
            }

            if (TryFindResource("FieldLabel") is Style labelStyle)
            {
                foreach (var child in Children)
                {
                    if (child is TextBlock block)
                    {
                        block.Style = labelStyle;
                    }
                }
            }
        }

        /// <summary>What the scope will touch, said in the adopter's own words so the button is never ambiguous.</summary>
        private string EnabledTooltip(SelectionMatrixScope scope)
        {
            var verb = Activates ? "Activa" : "Desactiva";
            switch (scope)
            {
                case SelectionMatrixScope.Cell:
                    return verb + " solo la celda seleccionada.";
                case SelectionMatrixScope.Row:
                    return verb + " ese " + editor.Labels.RowAxis.ToLower(CultureInfo.CurrentCulture)
                         + " en todos los " + Plural(editor.Labels.ColumnAxis) + ".";
                case SelectionMatrixScope.Column:
                    return verb + " todos los " + Plural(editor.Labels.RowAxis) + " de ese "
                         + editor.Labels.ColumnAxis.ToLower(CultureInfo.CurrentCulture) + ".";
                default:
                    return verb + " todas las celdas de la rejilla.";
            }
        }

        private string PrimaryText()
        {
            var cell = editor.PrimaryCell;
            if (cell == null)
            {
                return "Ninguna celda seleccionada.";
            }

            return "Celda: " + editor.Labels.ColumnAxis + " "
                 + (cell.Value.Column + 1).ToString(CultureInfo.CurrentCulture) + " · "
                 + editor.Labels.RowAxis + " "
                 + (cell.Value.Row + 1).ToString(CultureInfo.CurrentCulture) + ".";
        }

        private static string Plural(string axis)
            => (axis ?? string.Empty).ToLower(CultureInfo.CurrentCulture) + "s";

        private RadioButton StateOption(string text, string group, bool isChecked)
            => new RadioButton
            {
                Content = text,
                GroupName = group,
                IsChecked = isChecked,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 2, 10, 2),
                ToolTip = text == ActivateText
                    ? "Las celdas del alcance quedan marcadas."
                    : "Las celdas del alcance quedan desmarcadas."
            };

        private TextBlock Caption(string text, Thickness margin)
        {
            var block = new TextBlock
            {
                Text = text,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = margin
            };

            if (TryFindResource("FieldLabel") is Style labelStyle)
            {
                block.Style = labelStyle;
            }

            return block;
        }
    }
}
