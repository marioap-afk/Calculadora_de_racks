using System.Globalization;
using System.Linq;
using System.Windows;
using RackCad.Application.ProjectVariables;

namespace RackCad.UI
{
    /// <summary>
    /// The central window for the drawing's project variables (I-47 G16).
    ///
    /// <para>
    /// It decides nothing. It receives a PURE projection of what the drawing holds and returns ONE intent,
    /// addressed by <see cref="VariableId"/>. Who consumes a variable, whether a delete may proceed, what
    /// repairing means and how anything is written all live behind it, already proven; a rule written here
    /// would be a second answer to a question that already has one.
    /// </para>
    /// <para>
    /// It also does not keep a mutable model of its own. Asking for a change produces an intent and leaves
    /// the projection exactly as the drawing described it — the DWG stays the source of truth, and the caller
    /// re-reads it after every operation.
    /// </para>
    /// <para>
    /// The two irreversible actions — unlinking every consumer, and repairing a broken reference with the
    /// stored literal — need an explicit confirmation, checked here AND again in the preflight. The second
    /// check is not redundancy: this one is a courtesy to the user, that one is the invariant.
    /// </para>
    /// </summary>
    public partial class RackProjectVariablesWindow : Window
    {
        private readonly ProjectVariablesWorkspace workspace;

        public RackProjectVariablesWindow(ProjectVariablesWorkspace workspace)
        {
            InitializeComponent();

            this.workspace = workspace;

            VariablesList.ItemsSource = workspace?.Variables;
            BrokenList.ItemsSource = workspace?.BrokenBindings;
            RepairAuthorityText.Text = ProjectVariableRepairText.DescribeUnresolvable(workspace?.UnresolvableRacks);

            var editable = workspace != null && workspace.IsEditable;
            EditorPanel.IsEnabled = editable;

            if (!editable)
            {
                BlockedBanner.Visibility = Visibility.Visible;
                BlockedText.Text = workspace?.Error ?? "No se pudo leer el registro de variables de este dibujo.";
            }

            RepairWarningText.Text = string.Empty;
            UpdateActions();
        }

        /// <summary>What the user asked for, or null when the window was closed without asking anything.</summary>
        public ProjectVariableIntent Intent { get; private set; }

        private ProjectVariableRow Selected => VariablesList.SelectedItem as ProjectVariableRow;

        private BrokenBindingRow SelectedBroken => BrokenList.SelectedItem as BrokenBindingRow;

        private bool Editable => workspace != null && workspace.IsEditable;

        // ---------------------------------------------------------------- selección

        private void VariablesList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var row = Selected;

            if (row != null)
            {
                NameBox.Text = row.Name ?? string.Empty;
                ValueBox.Text = row.LiteralValue.ToString("0.###", CultureInfo.InvariantCulture);
            }

            ConsumersText.Text = DescribeConsumers(row);
            UnlinkConfirmCheck.IsChecked = false;
            UpdateActions();
        }

        private void BrokenList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var broken = SelectedBroken;

            RepairWarningText.Text = ProjectVariableRepairText.Describe(broken);

            RepairConfirmCheck.IsChecked = false;
            UpdateActions();
        }

        /// <summary>The racks in the way, named the way the user can act on them (rack + property).</summary>
        private static string DescribeConsumers(ProjectVariableRow row)
        {
            if (row == null)
            {
                return string.Empty;
            }

            if (!row.HasConsumers)
            {
                return "Ningún rack la usa todavía.";
            }

            return string.Join(
                " · ",
                row.Consumers.Select(consumer =>
                    (string.IsNullOrWhiteSpace(consumer.RackName) ? "(sin nombre)" : consumer.RackName)
                    + " [" + consumer.RackId + "] "
                    + string.Join(", ", consumer.PropertyIds)));
        }

        private void UpdateActions()
        {
            var hasSelection = Editable && Selected != null;

            RenameButton.IsEnabled = hasSelection;
            ChangeValueButton.IsEnabled = hasSelection;
            DeleteButton.IsEnabled = hasSelection;
            UnlinkAllDeleteButton.IsEnabled = hasSelection && UnlinkConfirmCheck.IsChecked == true;

            // La reparación NO depende de que el registro se pueda administrar: cuando el alcance del dibujo
            // es indeterminado es justo cuando hace falta poder quitar el vínculo que lo rompe.
            var repairable = SelectedBroken != null && SelectedBroken.RackCanRepair;

            RepairButton.IsEnabled = repairable && RepairConfirmCheck.IsChecked == true;
        }

        /// <summary>
        /// Checked/Unchecked y no Click: la confirmación es un ESTADO, y el estado también cambia por teclado
        /// o por código. Escuchar solo el clic dejaría el botón contradiciendo a la casilla.
        /// </summary>
        private void Confirm_Changed(object sender, RoutedEventArgs e) => UpdateActions();

        // ---------------------------------------------------------------- intents

        private void New_Click(object sender, RoutedEventArgs e)
        {
            if (!Editable || !TryValue(out var value))
            {
                return;
            }

            Ask(ProjectVariableIntent.Create(NameBox.Text?.Trim(), value));
        }

        private void Rename_Click(object sender, RoutedEventArgs e)
        {
            var row = Selected;

            if (!Editable || row == null)
            {
                return;
            }

            Ask(ProjectVariableIntent.Rename(row.Id, NameBox.Text?.Trim()));
        }

        private void ChangeValue_Click(object sender, RoutedEventArgs e)
        {
            var row = Selected;

            if (!Editable || row == null || !TryValue(out var value))
            {
                return;
            }

            Ask(ProjectVariableIntent.ChangeValue(row.Id, value));
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var row = Selected;

            if (!Editable || row == null)
            {
                return;
            }

            Ask(ProjectVariableIntent.Delete(row.Id));
        }

        private void UnlinkAllDelete_Click(object sender, RoutedEventArgs e)
        {
            var row = Selected;

            if (!Editable || row == null || UnlinkConfirmCheck.IsChecked != true)
            {
                return;
            }

            Ask(ProjectVariableIntent.UnlinkAllAndDelete(row.Id, confirmed: true));
        }

        private void Repair_Click(object sender, RoutedEventArgs e)
        {
            var broken = SelectedBroken;

            // RackCanRepair se comprueba AQUI y no solo al habilitar el botón: la habilitación es una pista
            // visual, y la precondición tiene que sostenerse en el punto donde nace el intent.
            if (broken == null || !broken.RackCanRepair || RepairConfirmCheck.IsChecked != true)
            {
                return;
            }

            Ask(ProjectVariableIntent.RepairBroken(broken.RackId, confirmed: true));
        }

        /// <summary>Records the request and hands control back: executing is not this window's job.</summary>
        private void Ask(ProjectVariableIntent intent)
        {
            Intent = intent;
            Close();
        }

        private bool TryValue(out double value)
        {
            if (UiSupport.TryNum(ValueBox.Text, out value) && value > 0.0)
            {
                return true;
            }

            StatusText.Text = "El valor tiene que ser un número mayor que cero.";
            return false;
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();
    }
}
