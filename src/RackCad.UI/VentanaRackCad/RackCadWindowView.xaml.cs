using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
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

        // The same for the Selectivo editors (Selectivo E1-B).
        private readonly System.Collections.Generic.Dictionary<SelectiveEditorViewModel, SelectiveEditorView> _selectiveViews = new();

        private void OnViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RackCadWindowViewModel.SideCollapsed)) ApplySideCollapsed();
            if (e.PropertyName == nameof(RackCadWindowViewModel.ActiveDynamicEditor)
                || e.PropertyName == nameof(RackCadWindowViewModel.ShowPanel)
                || e.PropertyName == nameof(RackCadWindowViewModel.DynamicEditors)
                || e.PropertyName == nameof(RackCadWindowViewModel.ActiveSelectiveEditor)
                || e.PropertyName == nameof(RackCadWindowViewModel.SelectiveEditors))
                ShowRackContent();
            if (e.PropertyName == nameof(RackCadWindowViewModel.NewDynamicEditor)) ShowNewContent();
            if (e.PropertyName == nameof(RackCadWindowViewModel.NewSelectiveEditor)) ShowNewSelectiveContent();
        }

        // E2 P3: «Nuevo» hosts the editor of the new Selectivo being made here; the view is the one the page of the rack shows once it is
        // inserted (kept with the views of the Selectivo editors).
        private void ShowNewSelectiveContent()
        {
            var editor = _viewModel.NewSelectiveEditor;
            if (editor == null)
            {
                NewSelectiveRackHost.Content = null;
                return;
            }

            if (!_selectiveViews.TryGetValue(editor, out var view))
            {
                view = new SelectiveEditorView(editor);
                _selectiveViews[editor] = view;
            }

            if (!ReferenceEquals(NewSelectiveRackHost.Content, view)) NewSelectiveRackHost.Content = view;
        }

        private void OnNewSelectiveHereClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.StartNewSelective());

        // T4 new (SD-06): the drafts of «Nuevo». Their buttons carry the draft as their data (the column, the list, the strip).
        private static RackCadDraftItem DraftOf(object sender) => (sender as FrameworkElement)?.DataContext as RackCadDraftItem;

        private void OnDraftItemClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            if (DraftOf(sender) is RackCadDraftItem item) _viewModel.OpenDraft(item);
        });

        private void OnDuplicateDraftClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            if (DraftOf(sender) is RackCadDraftItem item) _viewModel.DuplicateDraft(item);
        });

        private void OnDiscardDraftClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            if (DraftOf(sender) is RackCadDraftItem item) _viewModel.DiscardDraft(item);
        });

        private void OnConfirmDiscardDraftClick(object sender, RoutedEventArgs e) => Contained(() =>
        {
            if (DraftOf(sender) is RackCadDraftItem item) _viewModel.ConfirmDiscardDraft(item);
        });

        private void OnCancelDiscardDraftClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.CancelDiscardDraft(DraftOf(sender)));

        private void OnDraftsHomeClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.ShowPage(RackCadWindowPage.Nuevo));

        // «Nuevo» hosts the editor of the new Dinámico being made here; the view is the one the page of the rack shows once it is inserted.
        private void ShowNewContent()
        {
            var editor = _viewModel.NewDynamicEditor;
            if (editor == null)
            {
                NewRackHost.Content = null;
                return;
            }

            if (!_dynamicViews.TryGetValue(editor, out var view))
            {
                view = new DynamicEditorView(editor);
                _dynamicViews[editor] = view;
            }

            if (!ReferenceEquals(NewRackHost.Content, view)) NewRackHost.Content = view;
        }

        private void OnNewDynamicHereClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.StartNewDynamic());

        // The page of the rack shows the editor of the window of the active rack when it is open (a Dinámico or a Selectivo, and the user
        // did not ask for the panel), and the workspace panel otherwise. The views of editors the window no longer keeps (closed, or of a
        // closed drawing) are forgotten.
        private void ShowRackContent()
        {
            var live = _viewModel.DynamicEditors;
            foreach (var gone in _dynamicViews.Keys.Where(editor => !live.Contains(editor)).ToList()) _dynamicViews.Remove(gone);
            var liveSelective = _viewModel.SelectiveEditors;
            foreach (var gone in _selectiveViews.Keys.Where(editor => !liveSelective.Contains(editor)).ToList()) _selectiveViews.Remove(gone);

            var active = _viewModel.ActiveDynamicEditor;
            var activeSelective = active == null ? _viewModel.ActiveSelectiveEditor : null;
            if ((active == null && activeSelective == null) || _viewModel.ShowPanel)
            {
                if (!ReferenceEquals(RackPageHost.Content, PanelView)) RackPageHost.Content = PanelView;
                return;
            }

            if (activeSelective != null)
            {
                if (!_selectiveViews.TryGetValue(activeSelective, out var selectiveView))
                {
                    selectiveView = new SelectiveEditorView(activeSelective);
                    _selectiveViews[activeSelective] = selectiveView;
                }

                if (!ReferenceEquals(RackPageHost.Content, selectiveView)) RackPageHost.Content = selectiveView;
                return;
            }

            if (!_dynamicViews.TryGetValue(active, out var view))
            {
                view = new DynamicEditorView(active);
                _dynamicViews[active] = view;
            }

            if (!ReferenceEquals(RackPageHost.Content, view)) RackPageHost.Content = view;
        }

        /// <summary>How many editor views the page keeps (one per editor that the window keeps).</summary>
        internal int DynamicViewCount => _dynamicViews.Count;

        /// <summary>How many Selectivo editor views the page keeps (one per Selectivo editor that the window keeps).</summary>
        internal int SelectiveViewCount => _selectiveViews.Count;

        private void OnEditDynamicHereClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.EditDynamicHere());

        private void OnEditSelectiveHereClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.EditSelectiveHere());

        private void OnStopWaitingClick(object sender, RoutedEventArgs e) => Contained(() => _viewModel.StopWaiting());

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

        // T4 ut: «Examinar…» and «Restablecer» of the block library moved with «Configuración» to RackCadSettingsPage.

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
