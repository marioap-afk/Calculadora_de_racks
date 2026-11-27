using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// T5 ct: the view of the Cantilever editor of the RackCad window. It only passes the gestures of the user to the view-model: a field is
    /// committed with Enter or when it loses the focus, a click on the matrix selects with the modifiers of the keyboard, a segment chooses, a
    /// row of the list of sections chooses the section. The lists of the view-model keep their items, so a commit never replaces the box that
    /// has the focus. Every failure is contained and said in the message of the editor.
    /// </summary>
    public partial class CantileverEditorView : UserControl
    {
        private readonly CantileverEditorViewModel _viewModel;

        public CantileverEditorView(CantileverEditorViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            InitializeComponent();
            DataContext = viewModel;
        }

        public CantileverEditorViewModel ViewModel => _viewModel;

        /// <summary>Below this width of the tab «Brazos» the inspector does not fit beside the matrix and goes below it.</summary>
        internal const double InspectorBesideWidth = 900;

        /// <summary>Whether the inspector is below the matrix (a narrow window).</summary>
        internal bool InspectorBelow { get; private set; }

        private void OnArmsGridSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!e.WidthChanged) return;
            PlaceInspector(e.NewSize.Width < InspectorBesideWidth);
        }

        private void PlaceInspector(bool below)
        {
            if (InspectorBelow == below) return;
            InspectorBelow = below;
            Grid.SetRow(InspectorPanel, below ? 1 : 0);
            Grid.SetColumn(InspectorPanel, below ? 0 : 1);
            Grid.SetColumnSpan(InspectorPanel, below ? 2 : 1);
            InspectorPanel.Width = below ? double.NaN : 420;
            InspectorPanel.Margin = below ? new Thickness(0, 10, 0, 0) : new Thickness(10, 0, 0, 0);
            InspectorRow.Height = below ? new GridLength(1, GridUnitType.Auto) : new GridLength(0);
        }

        private void OnFieldKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key != Key.Enter) return;
            e.Handled = true;
            Commit(sender);
        }

        private void OnFieldLostFocus(object sender, RoutedEventArgs e) => Commit(sender);

        private void Commit(object sender) => Contained(() =>
        {
            if ((sender as FrameworkElement)?.DataContext is DynamicFieldItem field) field.Commit();
        });

        private static T DataOf<T>(object sender) where T : class => (sender as FrameworkElement)?.DataContext as T;

        private void OnChoiceOptionClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            if (DataOf<CantileverChoiceOptionItem>(sender) is CantileverChoiceOptionItem option) option.Owner.Choose(option.Value);
        });

        private void OnPickerToggleClick(object sender, RoutedEventArgs e) => Contained(() => DataOf<CantileverSectionPickerItem>(sender)?.Toggle());

        private void OnSectionChoiceClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            if (DataOf<CantileverSectionChoiceItem>(sender) is CantileverSectionChoiceItem choice) choice.Owner.Choose(choice);
        });

        private void OnCellClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            if (DataOf<CantileverCellItem>(sender) is not CantileverCellItem cell) return;
            var modifiers = Keyboard.Modifiers;
            _viewModel.ClickCell(cell.Cell, modifiers.HasFlag(ModifierKeys.Control), modifiers.HasFlag(ModifierKeys.Shift));
        });

        private void OnStationHeadClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            if ((sender as FrameworkElement)?.DataContext is CantileverColumnHead head) _viewModel.SelectStation(head.StationIndex);
        });

        private void OnLevelHeadClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            if (DataOf<CantileverMatrixRow>(sender) is CantileverMatrixRow row) _viewModel.SelectLevel(row.LevelIndex);
        });

        private void OnSelectAllClick(object sender, RoutedEventArgs e) => Contained(_viewModel.SelectAll);

        private void OnRestoreCellsClick(object sender, RoutedEventArgs e) => Contained(_viewModel.RestoreCells);

        private void OnGoDefaultArmClick(object sender, RoutedEventArgs e) => Contained(_viewModel.GoToDefaultArm);

        private void OnComponentTabClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            if (DataOf<CantileverComponentTab>(sender) is CantileverComponentTab tab) _viewModel.SelectComponent(tab.Key);
        });

        private void OnMissingGoClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            if ((sender as FrameworkElement)?.DataContext is CantileverMissingItem item) _viewModel.GoTo(item);
        });

        // The name just typed goes to the draft before a write, as with «Actualizar».
        private void CommitName() => NameBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();

        private void OnUpdateClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            CommitName();
            _viewModel.RequestWrite();
        });

        private void OnInsertViewClick(object sender, RoutedEventArgs e) => Contained(_viewModel.InsertView);

        private void OnInsertPanelInserting(object sender, EventArgs e) => Contained(CommitName);

        private void OnDiscardClick(object sender, RoutedEventArgs e) => Contained(_viewModel.Discard);

        private void OnRestoreClick(object sender, RoutedEventArgs e) => Contained(_viewModel.OpenRestore);

        private void OnLossToggleClick(object sender, RoutedEventArgs e) => Contained(_viewModel.ToggleLoss);

        private void OnRestoreConfirmClick(object sender, RoutedEventArgs e) => Contained(_viewModel.RestoreMarked);

        private void OnRestoreCancelClick(object sender, RoutedEventArgs e) => Contained(_viewModel.CancelRestore);

        private void OnCloseEditorClick(object sender, RoutedEventArgs e) => Contained(_viewModel.RequestClose);

        private void OnCloseYesClick(object sender, RoutedEventArgs e) => Contained(_viewModel.ConfirmClose);

        private void OnCloseNoClick(object sender, RoutedEventArgs e) => Contained(_viewModel.CancelClose);

        private void OnMoreClick(object sender, RoutedEventArgs e) => Contained(_viewModel.ToggleMore);

        private void OnSaveToLibraryClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            CommitName();
            _viewModel.SaveToLibrary();
        });

        private void OnGoSeparatorClick(object sender, RoutedEventArgs e) => Contained(_viewModel.GoToSeparator);

        private void OnToAutomaticYesClick(object sender, RoutedEventArgs e) => Contained(_viewModel.ConfirmToAutomatic);

        private void OnRecoverClick(object sender, RoutedEventArgs e) => Contained(_viewModel.Recover);

        private void OnStartFromRuleClick(object sender, RoutedEventArgs e) => Contained(_viewModel.StartFromRule);

        private void OnLayoutQuestionNoClick(object sender, RoutedEventArgs e) => Contained(_viewModel.CancelLayoutQuestion);

        private void OnSplitSegmentClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.SplitSegment(DataOf<CantileverSegmentRowItem>(sender)));

        private void OnRemoveSegmentClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.RemoveSegment(DataOf<CantileverSegmentRowItem>(sender)));

        private void OnAddSegmentClick(object sender, RoutedEventArgs e) => Contained(_viewModel.AddSegmentTop);

        private void OnApplySegmentsClick(object sender, RoutedEventArgs e) => Contained(_viewModel.ApplySegments);

        private void OnDiscardSegmentsClick(object sender, RoutedEventArgs e) => Contained(_viewModel.DiscardSegments);

        private void Contained(Action action)
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                _viewModel.Say("El editor tuvo un fallo contenido: " + ex.Message, error: true);
            }
        }
    }
}
