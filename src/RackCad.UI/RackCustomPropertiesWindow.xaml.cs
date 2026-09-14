using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using RackCad.Application.CustomProperties;
using RackCad.UI.Controls;
using RackCad.UI.Shell;
using ReadOutcome = RackCad.Application.Persistence.CustomPropertiesReadOutcome;

namespace RackCad.UI
{
    /// <summary>
    /// The editor of RACKPROPIEDADES (I-54 G7; Proposal V5 D-18.3..D-18.6; ADR-0039 §8): the custom properties of a rack or of
    /// the project, in one window whose scope is a fact of the workspace, not a type.
    ///
    /// <para>
    /// It decides nothing. It receives a PURE workspace — rows by id when Application says the collection is editable, per-view
    /// summaries and a reason otherwise — and returns ONE operation addressed by <see cref="CustomPropertyId"/>, or ONE unify
    /// from the view the user chose after seeing every view and ticking the box. Whether either may be written is decided again
    /// on a fresh read of the drawing; this window only asks, and it never changes the model it was given.
    /// </para>
    /// <para>
    /// A read-only collection opens read-only, with its reason, and every write is disabled from the start: nothing is offered
    /// that would only fail on confirmation. There is no discard, reset, overwrite or force in any state, and no message box —
    /// the window explains itself in its own banners. The unify panel exists only when Application offers a safe source, and it
    /// lists exactly the sources Application marked: the window never works out which view could be one.
    /// </para>
    /// <para>
    /// Closing — by the button, Escape, Alt+F4 or the system button — returns nothing: only an explicit action records a request,
    /// and no close route writes (ADR-0029 D7). Archetype C: the shared dialog chrome, the common action factory with its visible
    /// disabled reasons, CenterOwner and a deterministic initial focus that never lands on a destructive or blocked action (D6,
    /// D9, D11).
    /// </para>
    /// </summary>
    public partial class RackCustomPropertiesWindow : Window
    {
        /// <summary>How a view, or a unify source, with no properties reads (C-5A).</summary>
        public const string EmptyCollectionText = "vacío / sin propiedades";

        private readonly CustomPropertiesWorkspace workspace;

        /// <summary>What the command captured of every view before opening; what a unify is confirmed against.</summary>
        private readonly RackCustomPropertiesDisplayedState displayed;

        private readonly Button newButton;

        private readonly Button renameButton;

        private readonly Button changeValueButton;

        private readonly Button deleteButton;

        /// <summary>Null unless Application offers a safe unify.</summary>
        private readonly Button unifyButton;

        private readonly Button closeButton;

        private readonly List<RadioButton> unifySources = new List<RadioButton>();

        public RackCustomPropertiesWindow(
            CustomPropertiesWorkspace workspace, RackCustomPropertiesDisplayedState displayed = null, EditorStatusMessage status = null)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            if (workspace.UnifyAvailable && displayed == null)
            {
                throw new ArgumentException(
                    "Una unificación se confirma sobre lo que se mostró de cada vista, y la ventana no lo recibió.", nameof(displayed));
            }

            InitializeComponent();

            // I-39D: el chrome del arquetipo C sale de su fuente unica; la raiz del XAML no declara ni fondo ni tipografia.
            DialogWindowChrome.Apply(this);

            this.workspace = workspace;
            this.displayed = displayed;

            Title = "Propiedades personalizadas — " + workspace.ScopeLabel;
            ScopeText.Text = workspace.ScopeLabel;

            newButton = AddAction(ActionsPanel, "NewButton", "Nueva", New_Click);
            renameButton = AddAction(ActionsPanel, "RenameButton", "Renombrar", Rename_Click);
            changeValueButton = AddAction(ActionsPanel, "ChangeValueButton", "Cambiar valor", ChangeValue_Click);
            deleteButton = AddAction(ActionsPanel, "DeleteButton", "Eliminar", Delete_Click);

            closeButton = EditorActions.Button(new EditorAction("Cerrar", isCancel: true), Close_Click);
            closeButton.MinWidth = 110;
            closeButton.Margin = new Thickness(0);
            Register(CloseActionsPanel, "CloseButton", closeButton);

            PresentCollection();
            PresentViews();

            if (workspace.UnifyAvailable)
            {
                unifyButton = AddAction(UnifyActionsPanel, "UnifyButton", "Unificar", Unify_Click);
                PresentUnifySources();
                UnifyPanel.Visibility = Visibility.Visible;
            }

            StateBanner.Message = StateMessage();
            StatusPresenter.Message = status;

            UpdateActions();
            FocusManager.SetFocusedElement(this, InitialFocus());
        }

        /// <summary>The create, rename, change of value or delete the user asked for, or null.</summary>
        public CustomPropertiesIntent Intent { get; private set; }

        /// <summary>The confirmed unify the user asked for, or null.</summary>
        public RackCustomPropertiesUnifyIntent UnifyIntent { get; private set; }

        private bool IsEditable => workspace.State == CustomPropertiesWorkspaceState.Editable;

        private CustomPropertiesRow Selected => (PropertiesList.SelectedItem as PropertyItem)?.Row;

        private string SelectedUnifySource => unifySources.FirstOrDefault(radio => radio.IsChecked == true)?.Tag as string;

        // ---------------------------------------------------------------- presentacion

        private void PresentCollection()
        {
            PropertiesList.ItemsSource = workspace.Rows.Select(row => new PropertyItem(row)).ToList();
            PropertiesList.IsEnabled = IsEditable;
            NameBox.IsEnabled = IsEditable;
            ValueBox.IsEnabled = IsEditable;

            // D-05.2: un nombre repetido es un aviso, no un bloqueo. La identidad sigue siendo el id.
            var repeated = workspace.Rows
                .Where(row => row.NombreRepetido)
                .Select(row => "«" + row.Name + "»")
                .Distinct(StringComparer.Ordinal)
                .ToList();

            RepeatedNamesPresenter.Message = repeated.Count == 0
                ? null
                : EditorStatusMessage.Warning(
                    "Hay nombres repetidos: " + string.Join(", ", repeated) + ". Cada propiedad se sigue distinguiendo por su "
                    + "identidad, así que se pueden editar; conviene renombrarlas.");
        }

        private void PresentViews()
        {
            foreach (var summary in workspace.ViewSummaries)
            {
                SummariesList.Children.Add(Block(DescribeView(summary), DescribeContent(summary)));
            }

            if (workspace.UninterpretableDefinitions.Count > 0)
            {
                SummariesList.Children.Add(Block(
                    "Definiciones que esta versión no puede interpretar",
                    string.Join("\n", workspace.UninterpretableDefinitions.Select(definition =>
                        "bloque " + definition.BlockName + (definition.IsPlaced ? " (colocado)" : " (sin colocar)")))));
            }

            ViewsPanel.Visibility = SummariesList.Children.Count > 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        /// <summary>One radio per source Application marked as safe (D-09.10). None comes checked.</summary>
        private void PresentUnifySources()
        {
            foreach (var summary in workspace.ViewSummaries.Where(summary => summary.IsUnifySourceAvailable))
            {
                var radio = new RadioButton
                {
                    Tag = summary.Handle,
                    Margin = new Thickness(0, 2, 0, 2),
                    Content = new TextBlock
                    {
                        Text = DescribeView(summary) + ": " + DescribeContent(summary).Replace("\n", "; "),
                        TextWrapping = TextWrapping.Wrap,
                    },
                };

                radio.Checked += UnifySource_Checked;
                unifySources.Add(radio);
                UnifySourcesPanel.Children.Add(radio);
            }
        }

        private static FrameworkElement Block(string title, string content)
        {
            var panel = new StackPanel();
            panel.Children.Add(new TextBlock { Text = title, FontWeight = FontWeights.SemiBold, TextWrapping = TextWrapping.Wrap });
            panel.Children.Add(new TextBlock { Text = content, TextWrapping = TextWrapping.Wrap, Margin = new Thickness(8, 2, 0, 0) });

            var border = new Border
            {
                Child = panel,
                Padding = new Thickness(8, 6, 8, 6),
                Margin = new Thickness(0, 0, 0, 6),
                BorderThickness = new Thickness(1),
            };

            border.SetResourceReference(Border.BorderBrushProperty, "ShellBorderBrush");
            border.SetResourceReference(Border.BackgroundProperty, "ShellSurfaceBrush");
            return border;
        }

        /// <summary>Which view a summary is about: its view, its section when it has one, and its block (D-10.3).</summary>
        internal static string DescribeView(CustomPropertiesViewSummary summary)
        {
            var view = string.IsNullOrWhiteSpace(summary.View) ? "(sin nombre)" : summary.View;
            var section = summary.Section >= 0 ? " · sección " + summary.Section.ToString(CultureInfo.InvariantCulture) : string.Empty;

            return "Vista " + view + section + " · bloque " + summary.BlockName;
        }

        /// <summary>What a view holds, as the user reads it: its entries, «vacío / sin propiedades», or why it cannot be written.</summary>
        internal static string DescribeContent(CustomPropertiesViewSummary summary)
        {
            switch (summary.State)
            {
                case ReadOutcome.Absent:
                    return EmptyCollectionText;

                case ReadOutcome.Readable:
                    return summary.Entries.Count == 0
                        ? EmptyCollectionText
                        : string.Join("\n", summary.Entries.Select(entry => entry.Name + " — " + entry.Value));

                case ReadOutcome.AmbiguousIdentity:
                    return "No se puede escribir: identificadores repetidos." + Detail(summary.Error);

                case ReadOutcome.IncompatibleMajor:
                    return "No se puede escribir: versión de formato no admitida." + Detail(summary.Error);

                case ReadOutcome.DepthLimitExceeded:
                    return "No se puede escribir: profundidad máxima superada." + Detail(summary.Error);

                default:
                    return "No se puede escribir: las propiedades no se pueden leer." + Detail(summary.Error);
            }
        }

        /// <summary>The reason a workspace is read-only, as the user reads it (D-18.5). Presentation only: Application decided.</summary>
        internal static string ReasonText(CustomPropertiesReadOnlyReason? reason)
        {
            switch (reason)
            {
                case CustomPropertiesReadOnlyReason.XrefRejected:
                    return "El bloque elegido pertenece a una referencia externa (xref), y las propiedades de una xref no se escriben desde este dibujo.";
                case CustomPropertiesReadOnlyReason.NoIdentity:
                    return "El rack no tiene identidad, así que no se puede saber qué vistas comparten sus propiedades.";
                case CustomPropertiesReadOnlyReason.IndeterminateMembership:
                    return "Hay definiciones de rack que esta versión no puede interpretar, así que no se sabe con certeza qué vistas forman el rack.";
                case CustomPropertiesReadOnlyReason.MixedKind:
                    return "Las vistas del rack no declaran el mismo tipo de sistema.";
                case CustomPropertiesReadOnlyReason.UnknownKind:
                    return "El tipo de sistema del rack no es uno que esta versión de RackCad conozca.";
                case CustomPropertiesReadOnlyReason.CustomPropertiesReadOnly:
                    return "Las propiedades de alguna vista del rack no se pueden escribir; abajo está el estado de cada vista.";
                case CustomPropertiesReadOnlyReason.PresentButUnreadable:
                    return "Las propiedades guardadas en el dibujo no se pueden leer.";
                case CustomPropertiesReadOnlyReason.AmbiguousIdentity:
                    return "Las propiedades guardadas tienen identificadores repetidos.";
                case CustomPropertiesReadOnlyReason.IncompatibleMajor:
                    return "Las propiedades se guardaron con una versión de formato que esta versión de RackCad no admite.";
                case CustomPropertiesReadOnlyReason.DepthLimitExceeded:
                    return "Las propiedades guardadas superan la profundidad máxima que admite el formato.";
                default:
                    return "Las propiedades no se pueden escribir.";
            }
        }

        private static string Detail(string diagnostic)
            => string.IsNullOrWhiteSpace(diagnostic) ? string.Empty : " Detalle: " + diagnostic;

        private EditorStatusMessage StateMessage()
        {
            switch (workspace.State)
            {
                case CustomPropertiesWorkspaceState.Editable:
                    return null;

                case CustomPropertiesWorkspaceState.Divergent:
                    return EditorStatusMessage.Warning(
                        "Las vistas de este rack tienen propiedades distintas, así que no hay una colección del rack que editar. "
                        + (workspace.UnifyAvailable
                            ? "Revisa el contenido de cada vista y, si quieres, unifícalas desde la que elijas."
                            : "Ninguna vista se puede usar como origen de una unificación segura.")
                        + Detail(workspace.Diagnostic));

                default:
                    return EditorStatusMessage.Warning(
                        "Solo lectura. " + ReasonText(workspace.ReadOnlyReason)
                        + " No se ofrece ninguna escritura, y nada se descarta ni se sobrescribe."
                        + Detail(workspace.Diagnostic));
            }
        }

        // ---------------------------------------------------------------- acciones (D6)

        private Button AddAction(Panel panel, string name, string label, RoutedEventHandler onClick)
        {
            var button = EditorActions.Button(new EditorAction(label), onClick);
            button.MinWidth = 110;
            button.Margin = new Thickness(0, 0, 8, 6);
            Register(panel, name, button);
            return button;
        }

        /// <summary>The buttons are built in code, so they are named in the window's scope like the XAML ones.</summary>
        private void Register(Panel panel, string name, Button button)
        {
            button.Name = name;
            panel.Children.Add(button);
            RegisterName(name, button);
        }

        /// <summary>
        /// Every action with its availability and, when blocked, its visible reason — the contract the common factory builds a
        /// button with, re-declared as the selection and the confirmation change.
        /// </summary>
        private void UpdateActions()
        {
            if (newButton == null)
            {
                return;
            }

            var blocked = WriteBlockedReason();
            var needsRow = blocked ?? (Selected == null ? "Elige una propiedad de la lista." : null);

            Declare(newButton, blocked);
            Declare(renameButton, needsRow);
            Declare(changeValueButton, needsRow);
            Declare(deleteButton, needsRow);

            if (unifyButton != null)
            {
                Declare(
                    unifyButton,
                    SelectedUnifySource == null
                        ? "Elige la vista origen: ninguna viene elegida."
                        : UnifyConfirmCheck.IsChecked != true
                            ? "Marca la casilla para confirmar que revisaste el contenido de cada vista."
                            : null);
            }
        }

        private static void Declare(Button button, string disabledReason)
        {
            button.IsEnabled = disabledReason == null;
            button.ToolTip = disabledReason;
        }

        private string WriteBlockedReason()
        {
            switch (workspace.State)
            {
                case CustomPropertiesWorkspaceState.Editable:
                    return null;

                case CustomPropertiesWorkspaceState.Divergent:
                    return workspace.UnifyAvailable
                        ? "Las vistas de este rack tienen propiedades distintas: unifícalas desde una vista antes de editar."
                        : "Las vistas de este rack tienen propiedades distintas y ninguna vista se puede usar como origen de una unificación segura.";

                default:
                    return "Solo lectura: " + ReasonText(workspace.ReadOnlyReason);
            }
        }

        /// <summary>D9: deterministic, and never on a destructive or blocked action.</summary>
        private IInputElement InitialFocus()
        {
            if (!IsEditable)
            {
                return closeButton;
            }

            return workspace.Rows.Count > 0 ? (IInputElement)PropertiesList : NameBox;
        }

        private void PropertiesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var row = Selected;

            if (row != null)
            {
                NameBox.Text = row.Name ?? string.Empty;
                ValueBox.Text = row.Value ?? string.Empty;
            }

            UpdateActions();
        }

        /// <summary>Choosing another source withdraws the confirmation: the user confirms what they are about to unify from.</summary>
        private void UnifySource_Checked(object sender, RoutedEventArgs e)
        {
            UnifyConfirmCheck.IsChecked = false;
            UpdateActions();
        }

        private void UnifyConfirm_Changed(object sender, RoutedEventArgs e) => UpdateActions();

        // ---------------------------------------------------------------- intents

        // Each precondition is checked where the request is born and not only when enabling the button: enabling is a visual
        // hint, and a click can still reach a disabled button.

        private void New_Click(object sender, RoutedEventArgs e)
        {
            if (!IsEditable)
            {
                return;
            }

            Ask(CustomPropertiesIntent.Create(NameBox.Text, ValueBox.Text));
        }

        private void Rename_Click(object sender, RoutedEventArgs e)
        {
            var row = Selected;

            if (!IsEditable || row == null)
            {
                return;
            }

            Ask(CustomPropertiesIntent.Rename(row.Id, NameBox.Text));
        }

        private void ChangeValue_Click(object sender, RoutedEventArgs e)
        {
            var row = Selected;

            if (!IsEditable || row == null)
            {
                return;
            }

            Ask(CustomPropertiesIntent.ChangeValue(row.Id, ValueBox.Text));
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var row = Selected;

            if (!IsEditable || row == null)
            {
                return;
            }

            Ask(CustomPropertiesIntent.Delete(row.Id));
        }

        private void Unify_Click(object sender, RoutedEventArgs e)
        {
            var source = SelectedUnifySource;

            if (!workspace.UnifyAvailable || source == null || UnifyConfirmCheck.IsChecked != true)
            {
                return;
            }

            UnifyIntent = RackCustomPropertiesUnifyIntent.Create(displayed, source, confirmed: true);
            Close();
        }

        /// <summary>Records the request and hands control back: executing it is not this window's job.</summary>
        private void Ask(CustomPropertiesIntent intent)
        {
            Intent = intent;
            Close();
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        /// <summary>One row as the list shows it: «Nombre — Valor», with its id kept behind the text and never shown.</summary>
        public sealed class PropertyItem
        {
            internal PropertyItem(CustomPropertiesRow row)
            {
                Row = row;
            }

            public CustomPropertiesRow Row { get; }

            public CustomPropertyId Id => Row.Id;

            public string Name => Row.Name;

            public string Value => Row.Value;

            public bool NombreRepetido => Row.NombreRepetido;

            public string Text => Row.Name + " — " + Row.Value;

            /// <summary>The visible mark of a repeated name (D-05.2): a warning next to the row, never a block.</summary>
            public string Marker => Row.NombreRepetido ? "  · nombre repetido" : string.Empty;

            public override string ToString() => Text;
        }
    }
}
