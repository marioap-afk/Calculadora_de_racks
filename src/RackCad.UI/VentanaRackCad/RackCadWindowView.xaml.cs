using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using RackCad.Application.VentanaRackCad;
using RackCad.Application.Workspace;
using RackCad.UI.Workspace;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// Root control of the RackCad window (I-64 A-14, phases 1 and 2): the one visual the palette host adds. Its data context is the
    /// view-model of the window; the page of the rack is the workspace panel itself, placed in the center, with its strip of tabs hidden
    /// because the list of the side column replaces it. The control never asks the host for anything by itself: only the clicks of the user
    /// reach the view-model, and every failure is contained and said in the message of the window.
    /// </summary>
    public partial class RackCadWindowView : UserControl
    {
        private const double SideWidth = 250;

        private readonly RackCadWindowViewModel _viewModel;
        private double _sideWidth = SideWidth;

        public RackCadWindowView(RackCadWindowViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            InitializeComponent();
            DataContext = viewModel;

            PanelView = new WorkspacePanelView(viewModel.Panel);
            // The list of the side column is the way to move between racks: the strip of tabs of the panel and its label are not shown here.
            PanelView.BrowserTabStrip.Visibility = Visibility.Collapsed;
            PanelView.RootLabelText.Visibility = Visibility.Collapsed;
            // Its text of "no tab" would repeat the one of the window (NoRackText).
            PanelView.BrowserEmptyText.Visibility = Visibility.Collapsed;
            RackPageHost.Content = PanelView;

            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
            SizeChanged += OnSizeChanged;
            ApplySideCollapsed();
        }

        /// <summary>Below this width the two columns do not fit: the side column is hidden on its own until the window is wider again.</summary>
        internal const double NarrowWidth = 520;

        private bool _autoCollapsed;

        // A narrow palette (docked, for example) hides the side column by itself, and gives it back when it is wide again, unless the user
        // chose by hand meanwhile.
        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (!e.WidthChanged) return;
            if (e.NewSize.Width < NarrowWidth && !_viewModel.SideCollapsed)
            {
                _autoCollapsed = true;
                _viewModel.SideCollapsed = true;
            }
            else if (e.NewSize.Width >= NarrowWidth && _autoCollapsed && _viewModel.SideCollapsed)
            {
                _autoCollapsed = false;
                _viewModel.SideCollapsed = false;
            }
        }

        public RackCadWindowViewModel ViewModel => _viewModel;

        /// <summary>The workspace panel shown as the page of the rack.</summary>
        public WorkspacePanelView PanelView { get; }

        // One view per Dinámico editor of the window, kept while its draft lives, so moving between racks keeps what was on screen.
        private readonly System.Collections.Generic.Dictionary<DynamicEditorViewModel, DynamicEditorView> _dynamicViews = new();

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RackCadWindowViewModel.SideCollapsed)) ApplySideCollapsed();
            if (e.PropertyName == nameof(RackCadWindowViewModel.ActiveDynamicEditor) || e.PropertyName == nameof(RackCadWindowViewModel.ShowPanel))
                ShowRackContent();
        }

        // The page of the rack shows the Dinámico editor of the window when it is open (and the user did not ask for the panel), and the
        // workspace panel otherwise.
        private void ShowRackContent()
        {
            var editor = _viewModel.ActiveDynamicEditor;
            if (editor == null || _viewModel.ShowPanel)
            {
                if (!ReferenceEquals(RackPageHost.Content, PanelView)) RackPageHost.Content = PanelView;
                return;
            }

            if (!_dynamicViews.TryGetValue(editor, out var view))
            {
                view = new DynamicEditorView(editor, () => _viewModel.OriginalDesignOf(editor), () => _viewModel.OriginalNameOf(editor));
                _dynamicViews[editor] = view;
            }

            if (!ReferenceEquals(RackPageHost.Content, view)) RackPageHost.Content = view;
        }

        private void OnEditDynamicHereClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.EditDynamicHere());

        private void OnPanelToggleClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.ShowPanel = !_viewModel.ShowPanel);

        // Hiding the side column keeps its width, so showing it again gives it back as the user left it.
        private void ApplySideCollapsed()
        {
            if (_viewModel.SideCollapsed)
            {
                if (SideColumn.Width.IsAbsolute && SideColumn.Width.Value > 0) _sideWidth = SideColumn.Width.Value;
                SideColumn.Width = new GridLength(0);
                SidePanel.Visibility = Visibility.Collapsed;
                SideSplitter.Visibility = Visibility.Collapsed;
            }
            else
            {
                SideColumn.Width = new GridLength(_sideWidth > 0 ? _sideWidth : SideWidth);
                SidePanel.Visibility = Visibility.Visible;
                SideSplitter.Visibility = Visibility.Visible;
            }
        }

        private void OnSideToggleClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            _autoCollapsed = false;
            _viewModel.SideCollapsed = !_viewModel.SideCollapsed;
        });

        private void OnRackItemCloseClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            if ((sender as FrameworkElement)?.DataContext is RackListItem item) _viewModel.RackList.Close(item.RackId);
            e.Handled = true;
        });

        private void OnSectionClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            if ((sender as FrameworkElement)?.DataContext is RackCadWindowSection section) _viewModel.ShowPage(section.Page);
        });

        private void OnRackItemClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            if ((sender as FrameworkElement)?.DataContext is RackListItem item) _viewModel.OpenRack(item.RackId);
        });

        private void OnOpenSelectedClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.RackList.OpenSelected());

        private void OnReloadListClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.RackList.Reload());

        private void OnNewSystemClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            if ((sender as FrameworkElement)?.DataContext is RackCadNewSystemCard card) _viewModel.Run(card.Action);
        });

        // The buttons of the pages name their classic action in their Tag.
        private void OnClassicActionClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            var tag = (sender as FrameworkElement)?.Tag as string;
            if (Enum.TryParse<RackCadClassicAction>(tag, out var action)) _viewModel.Run(action);
        });

        private void OnDismissMessageClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.DismissMessage());

        private void OnBrowseBlockLibraryClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Selecciona la biblioteca de bloques",
                Filter = "Dibujo de AutoCAD (*.dwg)|*.dwg|Todos (*.*)|*.*",
                CheckFileExists = true,
            };

            var current = _viewModel.BlockLibraryPath;
            try
            {
                var directory = string.IsNullOrWhiteSpace(current) ? null : Path.GetDirectoryName(current);
                if (!string.IsNullOrEmpty(directory) && Directory.Exists(directory)) dialog.InitialDirectory = directory;
                if (!string.IsNullOrWhiteSpace(current) && File.Exists(current)) dialog.FileName = Path.GetFileName(current);
            }
            catch (Exception)
            {
                // An invalid current path only means the dialog opens at its default folder.
            }

            // Inside the palette of AutoCAD the control has no WPF window: the dialog then takes the active window as its owner.
            var owner = Window.GetWindow(this);
            var chosen = owner != null ? dialog.ShowDialog(owner) : dialog.ShowDialog();
            if (chosen == true) _viewModel.SetBlockLibraryPath(dialog.FileName);
        });

        private void OnResetBlockLibraryClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.ResetBlockLibraryPath());

        private void Contained(Action interaction)
        {
            try
            {
                interaction();
            }
            catch (Exception ex)
            {
                _viewModel.ShowError(ex);
            }
        }
    }
}
