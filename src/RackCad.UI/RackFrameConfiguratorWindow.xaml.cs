using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using RackCad.Domain.RackFrames;

namespace RackCad.UI
{
    public partial class RackFrameConfiguratorWindow : Window
    {
        private static readonly Brush StandardStroke = new SolidColorBrush(Color.FromRgb(53, 84, 110));
        private static readonly Brush ModifiedStroke = new SolidColorBrush(Color.FromRgb(183, 121, 31));
        private static readonly Brush HorizontalStroke = new SolidColorBrush(Color.FromRgb(56, 128, 111));
        private static readonly Brush TargetStroke = new SolidColorBrush(Color.FromRgb(176, 0, 32));
        private static readonly Brush PostStroke = new SolidColorBrush(Color.FromRgb(31, 41, 51));
        private static readonly Brush PostFill = new SolidColorBrush(Color.FromRgb(230, 235, 240));
        private static readonly Brush SegmentBoundaryStroke = new SolidColorBrush(Color.FromRgb(187, 197, 209));
        private static readonly Brush ModifiedFill = new SolidColorBrush(Color.FromArgb(48, 255, 196, 0));
        private static readonly Brush SelectedFill = new SolidColorBrush(Color.FromArgb(32, 66, 153, 225));
        private static readonly Brush FrontSideStroke = new SolidColorBrush(Color.FromRgb(43, 108, 176));
        private static readonly Brush RearSideStroke = new SolidColorBrush(Color.FromRgb(197, 48, 48));
        private static readonly Brush BothSideStroke = new SolidColorBrush(Color.FromRgb(128, 90, 213));
        private static readonly Brush NoneSideStroke = new SolidColorBrush(Color.FromRgb(160, 174, 192));
        private static readonly Brush ActiveElementStroke = new SolidColorBrush(Color.FromRgb(49, 130, 206));

        private bool syncingTreeSelection;
        private bool syncingGridSelection;
        private readonly bool canInsertInAutoCad;

        /// <summary>True when the user asked to draw the header in AutoCAD; the host inserts it after this
        /// window closes (the placement jig needs the editor free, so it cannot run while the modal is open).</summary>
        public bool InsertRequested { get; private set; }

        /// <summary>Which view the user asked to insert ("lateral" default, or "planta").</summary>
        public string InsertView { get; private set; } = "lateral";

        /// <summary>True when the user chose "Actualizar" (redraw existing views in place, insert nothing).</summary>
        public bool UpdateOnly { get; private set; }

        /// <summary>Set by the host when the window was opened on an EXISTING cabecera (RACKEDITAR). The planta view
        /// can only be inserted then — it links to that cabecera's lateral; inserting it on a new one would orphan it.</summary>
        public bool IsEditingExisting { get; set; }

        public RackFrameConfiguratorWindow(RackFrameConfiguration configuration)
            : this(configuration, false)
        {
        }

        public RackFrameConfiguratorWindow(RackFrameConfiguration configuration, bool canInsertInAutoCad)
        {
            this.canInsertInAutoCad = canInsertInAutoCad;
            InitializeComponent();
            Dispatcher.UnhandledException += Dispatcher_UnhandledException;
            ViewModel = new RackFrameConfiguratorViewModel(configuration);
            DataContext = ViewModel;

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
            ViewModel.BracingSegments.CollectionChanged += BracingSegments_CollectionChanged;
            ViewModel.Horizontals.CollectionChanged += BracingSegments_CollectionChanged;
            Loaded += RackFrameConfiguratorWindow_Loaded;
        }

        public RackFrameConfiguratorViewModel ViewModel { get; private set; }

        public RackFrameConfiguration Configuration => ViewModel.Configuration;

        private void RackFrameConfiguratorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ApplySavedLayout();
            SyncSelectedSegments();

            // "Actualizar" (redraw existing in place) and the planta view link to an EXISTING cabecera: both are
            // disabled (with the reason on hover) unless the window was opened via RACKEDITAR inside AutoCAD.
            if (!IsEditingExisting || !canInsertInAutoCad)
            {
                var reason = !canInsertInAutoCad
                    ? "Disponible solo cuando el configurador se abre desde AutoCAD."
                    : "Primero inserta la cabecera lateral; luego selecciónala con RACKEDITAR y actualiza o agrega la planta desde ahí.";
                UpdateButton.IsEnabled = false;
                UpdateButton.ToolTip = reason;
                InsertPlantaButton.IsEnabled = false;
                InsertPlantaButton.ToolTip = reason;
            }

            DrawPreview();
        }

        private void ModelTree_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (syncingTreeSelection)
            {
                return;
            }

            if (e.NewValue is not ConfiguratorNavigationItem item)
            {
                return;
            }

            RunUiAction(() =>
            {
                syncingTreeSelection = true;

                try
                {
                    ViewModel.SelectNavigationItem(item);
                }
                finally
                {
                    syncingTreeSelection = false;
                }

                if (item.Segment != null)
                {
                    SelectSingleSegment(item.Segment);
                }
                else if (item.Horizontal != null)
                {
                    ViewModel.SelectedHorizontal = item.Horizontal;
                    SyncHorizontalGridSelectionFromViewModel();
                    DrawPreview();
                }
                else if (ShouldKeepSegmentContext(item))
                {
                    SyncGridSelectionFromViewModel();
                    DrawPreview();
                }
                else
                {
                    ClearGridSelection();
                    DrawPreview();
                }
            });
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ResetLayoutButton_Click(object sender, RoutedEventArgs e)
        {
            RunUiAction(() =>
            {
                ApplyDefaultLayout();
                UpdateLayout();
                SaveCurrentLayout();
                DrawPreview();
            });
        }

        private void RestoreSelectedSegment_Click(object sender, RoutedEventArgs e)
        {
            RunUiAction(() => ViewModel.RestoreSelectedSegment());
        }

        private bool ConfirmDiscard(string action)
        {
            if (ViewModel == null || !ViewModel.HasUnsavedManualEdits)
            {
                return true;
            }

            var result = MessageBox.Show(
                this,
                "Hay cambios manuales o excepciones que se perderán al " + action + ". ¿Deseas continuar?",
                "Confirmar",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            return result == MessageBoxResult.Yes;
        }

        private void RestoreStandardFrameButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ConfirmDiscard("restaurar la cabecera estándar"))
            {
                return;
            }

            RunUiAction(() =>
            {
                ViewModel.RestoreStandardConfiguration();
                SyncGridSelectionFromViewModel();
                SyncHorizontalGridSelectionFromViewModel();
                SyncTreeSelectionFromViewModel();
                DrawPreview();
            });
        }

        private void ApplySimpleHeader_Click(object sender, RoutedEventArgs e)
        {
            if (!ConfirmDiscard("generar la cabecera"))
            {
                return;
            }

            RunUiAction(() =>
            {
                ViewModel.ApplySimpleConfiguration();
                SyncGridSelectionFromViewModel();
                SyncHorizontalGridSelectionFromViewModel();
                SyncTreeSelectionFromViewModel();
                DrawPreview();
            });
        }

        private void ViewBom_Click(object sender, RoutedEventArgs e)
        {
            RunUiAction(() =>
            {
                var window = new RackBomWindow(ViewModel.BuildBom()) { Owner = this };
                window.ShowDialog();
            });
        }

        private void InsertInAutoCad_Click(object sender, RoutedEventArgs e) => RequestDraw("lateral", updateOnly: false);

        /// <summary>"Actualizar": redraw the cabecera's already-drawn views (lateral/planta) in place; insert nothing.</summary>
        private void UpdateExisting_Click(object sender, RoutedEventArgs e) => RequestDraw(view: null, updateOnly: true);

        private void InsertPlanta_Click(object sender, RoutedEventArgs e)
        {
            // The planta is a view OF the cabecera: it must link to an existing lateral. Inserting it on a brand-new
            // cabecera would orphan it, so require inserting the lateral first and adding the planta via RACKEDITAR.
            if (!IsEditingExisting)
            {
                MessageBox.Show(
                    this,
                    "Primero inserta la cabecera lateral. Luego selecciónala con RACKEDITAR y desde ahí agrega la vista "
                        + "planta: así queda ligada a la misma cabecera (si la insertas sola quedaría huérfana).",
                    "Vista planta",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            RequestDraw("planta", updateOnly: false);
        }

        /// <summary>
        /// Close asking AutoCAD to draw. <paramref name="updateOnly"/> = redraw existing views only (Actualizar);
        /// otherwise insert a new linked view-block of <paramref name="view"/> AND refresh the existing ones.
        /// </summary>
        private void RequestDraw(string view, bool updateOnly)
        {
            if (!canInsertInAutoCad)
            {
                MessageBox.Show(
                    this,
                    "El dibujo en AutoCAD solo está disponible cuando el configurador se abre desde AutoCAD.",
                    "Insertar en AutoCAD",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            // Single-click parity with the other editors: in quick-config mode, "Insertar" also generates the
            // cabecera from the current inputs first (advanced mode already keeps the config up to date).
            // Regenerating DISCARDS manual/advanced edits, so ask first when there are any.
            if (ViewModel.IsSimpleEditor)
            {
                if (!ConfirmDiscard("regenerar la cabecera desde la configuración rápida"))
                {
                    return;
                }

                RunUiAction(() =>
                {
                    ViewModel.ApplySimpleConfiguration();
                    SyncGridSelectionFromViewModel();
                    SyncHorizontalGridSelectionFromViewModel();
                    SyncTreeSelectionFromViewModel();
                    DrawPreview();
                });
            }

            if (!ViewModel.IsModelConsistent)
            {
                var proceed = MessageBox.Show(
                    this,
                    "El modelo tiene advertencias de consistencia. ¿Deseas dibujar de todos modos?",
                    "Insertar en AutoCAD",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (proceed != MessageBoxResult.Yes)
                {
                    return;
                }
            }

            // The placement jig needs the editor free, so we only flag the request and close; the host
            // command draws the block and runs the jig once every modal window is gone.
            UpdateOnly = updateOnly;
            InsertView = updateOnly ? null : view;
            InsertRequested = true;
            Close();
        }

        private void SaveProject_Click(object sender, RoutedEventArgs e)
        {
            var libraryFolder = RackCad.Application.Settings.UserSettingsStore.ResolveDesignLibraryPath(RackCad.Application.Settings.UserSettingsStore.Load());
            try { System.IO.Directory.CreateDirectory(libraryFolder); } catch { /* best-effort default folder */ }

            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Proyecto RackCad (*.rackcad.json)|*.rackcad.json|JSON (*.json)|*.json",
                FileName = "cabecera" + RackCad.Application.Persistence.RackFrameProjectStore.FileExtension,
                InitialDirectory = libraryFolder
            };

            if (dialog.ShowDialog(this) == true)
            {
                RunUiAction(() => ViewModel.SaveProjectTo(dialog.FileName));
            }
        }

        private void OpenProject_Click(object sender, RoutedEventArgs e)
        {
            if (!ConfirmDiscard("abrir otro proyecto"))
            {
                return;
            }

            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Proyecto RackCad (*.rackcad.json)|*.rackcad.json|JSON (*.json)|*.json|Todos (*.*)|*.*"
            };

            if (dialog.ShowDialog(this) == true)
            {
                RunUiAction(() =>
                {
                    ViewModel.LoadProjectFrom(dialog.FileName);
                    SyncGridSelectionFromViewModel();
                    SyncHorizontalGridSelectionFromViewModel();
                    SyncTreeSelectionFromViewModel();
                    DrawPreview();
                });
            }
        }

        private void Add44Segment_Click(object sender, RoutedEventArgs e)
        {
            RunUiAction(() =>
            {
                ViewModel.AddCommonSegment(44.0);
                SyncGridSelectionFromViewModel();
                SyncTreeSelectionFromViewModel();
            });
        }

        private void Add70Segment_Click(object sender, RoutedEventArgs e)
        {
            RunUiAction(() =>
            {
                ViewModel.AddCommonSegment(70.0);
                SyncGridSelectionFromViewModel();
                SyncTreeSelectionFromViewModel();
            });
        }

        private void AddHorizontal_Click(object sender, RoutedEventArgs e)
        {
            RunUiAction(() =>
            {
                ViewModel.AddHorizontal();
                SyncHorizontalGridSelectionFromViewModel();
                SyncTreeSelectionFromViewModel();
            });
        }

        private void DeleteSelectedHorizontal_Click(object sender, RoutedEventArgs e)
        {
            RunUiAction(() =>
            {
                ViewModel.DeleteSelectedHorizontal();
                SyncHorizontalGridSelectionFromViewModel();
                SyncTreeSelectionFromViewModel();
                SyncGridSelectionFromViewModel();
            });
        }

        private void DuplicateSelectedHorizontal_Click(object sender, RoutedEventArgs e)
        {
            RunUiAction(() =>
            {
                ViewModel.DuplicateSelectedHorizontal();
                SyncHorizontalGridSelectionFromViewModel();
                SyncTreeSelectionFromViewModel();
            });
        }

        private void SplitSelectedSegment_Click(object sender, RoutedEventArgs e)
        {
            RunUiAction(() =>
            {
                ViewModel.SplitSelectedSegment();
                SyncGridSelectionFromViewModel();
                SyncTreeSelectionFromViewModel();
            });
        }

        private void CombineSelectedSegments_Click(object sender, RoutedEventArgs e)
        {
            RunUiAction(() =>
            {
                ViewModel.CombineSelectedSegments();
                SyncGridSelectionFromViewModel();
                SyncTreeSelectionFromViewModel();
            });
        }

        private void QuickNoBracing_Click(object sender, RoutedEventArgs e)
        {
            RunUiAction(() => ViewModel.ApplyNoBracingToSelection());
        }

        private void QuickDoubleBracing_Click(object sender, RoutedEventArgs e)
        {
            RunUiAction(() => ViewModel.ApplyDoubleBracingToSelection());
        }

        private void ApplyBulkPattern_Click(object sender, RoutedEventArgs e)
        {
            RunUiAction(() => ViewModel.ApplyBulkPattern());
        }

        private void ApplyBulkSide_Click(object sender, RoutedEventArgs e)
        {
            RunUiAction(() => ViewModel.ApplyBulkSide());
        }

        private void ApplyBulkProfile_Click(object sender, RoutedEventArgs e)
        {
            RunUiAction(() => ViewModel.ApplyBulkProfile());
        }

        private void ApplyBulkHorizontalProfile_Click(object sender, RoutedEventArgs e)
        {
            RunUiAction(() => ViewModel.ApplyBulkHorizontalProfile());
        }

        private void ApplyBulkHorizontalFace_Click(object sender, RoutedEventArgs e)
        {
            RunUiAction(() => ViewModel.ApplyBulkHorizontalFace());
        }

        private void ApplyBulkHorizontalQuantity_Click(object sender, RoutedEventArgs e)
        {
            RunUiAction(() => ViewModel.ApplyBulkHorizontalQuantity());
        }

        private void ApplyBulkHorizontalElevationOffset_Click(object sender, RoutedEventArgs e)
        {
            RunUiAction(() => ViewModel.ApplyBulkHorizontalElevationOffset());
        }

        private void BracingSegmentsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (syncingGridSelection)
            {
                return;
            }

            SyncSelectedSegments();

            if (ViewModel.SelectedBracingSegment != null)
            {
                var navigationItem = ViewModel.GetNavigationItemForSegment(ViewModel.SelectedBracingSegment);

                if (navigationItem != null)
                {
                    ViewModel.SelectNavigationItem(navigationItem);
                    SyncTreeSelectionFromViewModel();
                }
                else
                {
                    ViewModel.SelectNavigation("Segments");
                }
            }

            DrawPreview();
        }

        private void HorizontalsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (syncingGridSelection)
            {
                return;
            }

            var selectedHorizontals = HorizontalsGrid.SelectedItems.OfType<HorizontalEditorRow>().ToList();
            var activeHorizontal = HorizontalsGrid.SelectedItem as HorizontalEditorRow;
            if (activeHorizontal == null && selectedHorizontals.Count > 0)
            {
                activeHorizontal = selectedHorizontals[0];
            }

            ViewModel.SelectedHorizontal = activeHorizontal;
            ViewModel.SetSelectedHorizontals(selectedHorizontals);

            if (activeHorizontal != null)
            {
                var navigationItem = ViewModel.GetNavigationItemForHorizontal(activeHorizontal);

                if (navigationItem != null)
                {
                    ViewModel.SelectNavigationItem(navigationItem);
                    SyncTreeSelectionFromViewModel();
                }
                else
                {
                    ViewModel.SelectNavigation("Horizontals");
                }
            }

            DrawPreview();
        }

        private void PreviewCanvas_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            DrawPreview();
        }

        protected override void OnClosed(EventArgs e)
        {
            SaveCurrentLayout();
            Loaded -= RackFrameConfiguratorWindow_Loaded;
            Dispatcher.UnhandledException -= Dispatcher_UnhandledException;

            if (ViewModel != null)
            {
                ViewModel.PropertyChanged -= ViewModel_PropertyChanged;
                ViewModel.BracingSegments.CollectionChanged -= BracingSegments_CollectionChanged;
                ViewModel.Horizontals.CollectionChanged -= BracingSegments_CollectionChanged;
            }

            base.OnClosed(e);
        }

        private void ApplySavedLayout()
        {
            var settings = RackFrameConfiguratorLayoutStore.Load();

            if (settings == null)
            {
                ApplyDefaultLayout();
                return;
            }

            ApplyLayout(settings);
        }

        private void ApplyDefaultLayout()
        {
            LeftColumn.Width = new GridLength(245.0);
            PreviewColumn.Width = new GridLength(318.0);
            CenterColumn.Width = new GridLength(1.0, GridUnitType.Star);

            SetPixelRow(LeftModelRow, 205.0, 120.0);
            SetPixelRow(LeftValidationRow, 122.0, 95.0);
            SetStarRow(LeftExceptionsRow, 1.0, 140.0);
            SetPixelRow(CenterPropertiesRow, 150.0, 115.0);
            SetPixelRow(CenterQuickRow, 90.0, 72.0);
            SetPixelRow(CenterBulkRow, 80.0, 65.0);
            SetStarRow(CenterTablesRow, 1.0, 368.0);
            SetStarRow(HorizontalTableRow, 0.95, 180.0);
            SetStarRow(PanelTableRow, 1.15, 180.0);
        }

        private void ApplyLayout(RackFrameConfiguratorLayoutSettings settings)
        {
            if (settings == null)
            {
                return;
            }

            LeftColumn.Width = new GridLength(ClampLayoutValue(settings.LeftWidth, 220.0, 520.0));
            PreviewColumn.Width = new GridLength(ClampLayoutValue(settings.PreviewWidth, 280.0, 620.0));
            CenterColumn.Width = new GridLength(1.0, GridUnitType.Star);

            SetPixelRow(LeftModelRow, settings.LeftModelHeight, 120.0);
            SetPixelRow(LeftValidationRow, settings.LeftValidationHeight, 95.0);
            SetPixelRow(LeftExceptionsRow, settings.LeftExceptionsHeight, 140.0);
            SetPixelRow(CenterPropertiesRow, settings.CenterPropertiesHeight, 115.0);
            SetPixelRow(CenterQuickRow, settings.CenterQuickHeight, 72.0);
            SetPixelRow(CenterBulkRow, settings.CenterBulkHeight, 65.0);
            SetPixelRow(CenterTablesRow, settings.CenterTablesHeight, 368.0);
            SetPixelRow(HorizontalTableRow, settings.HorizontalTableHeight, 180.0);
            SetPixelRow(PanelTableRow, settings.PanelTableHeight, 180.0);
        }

        private void SaveCurrentLayout()
        {
            try
            {
                RackFrameConfiguratorLayoutStore.Save(new RackFrameConfiguratorLayoutSettings
                {
                    LeftWidth = LeftColumn.ActualWidth,
                    PreviewWidth = PreviewColumn.ActualWidth,
                    LeftModelHeight = LeftModelRow.ActualHeight,
                    LeftValidationHeight = LeftValidationRow.ActualHeight,
                    LeftExceptionsHeight = LeftExceptionsRow.ActualHeight,
                    CenterPropertiesHeight = CenterPropertiesRow.ActualHeight,
                    CenterQuickHeight = CenterQuickRow.ActualHeight,
                    CenterBulkHeight = CenterBulkRow.ActualHeight,
                    CenterTablesHeight = CenterTablesRow.ActualHeight,
                    HorizontalTableHeight = HorizontalTableRow.ActualHeight,
                    PanelTableHeight = PanelTableRow.ActualHeight
                });
            }
            catch
            {
                // Layout persistence should never block closing the AutoCAD modal window.
            }
        }

        private static void SetPixelRow(RowDefinition row, double value, double minimum)
        {
            if (row == null)
            {
                return;
            }

            row.Height = new GridLength(Math.Max(minimum, value <= 0.0 ? minimum : value));
            row.MinHeight = minimum;
        }

        private static void SetStarRow(RowDefinition row, double value, double minimum)
        {
            if (row == null)
            {
                return;
            }

            row.Height = new GridLength(Math.Max(0.01, value), GridUnitType.Star);
            row.MinHeight = minimum;
        }

        private static double ClampLayoutValue(double value, double minimum, double maximum)
        {
            if (value <= 0.0)
            {
                return minimum;
            }

            return Math.Clamp(value, minimum, maximum);
        }

        private void ViewModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!syncingTreeSelection &&
                e.PropertyName == nameof(RackFrameConfiguratorViewModel.SelectedNavigationItem))
            {
                SyncTreeSelectionFromViewModel();
            }

            SchedulePreviewRedraw();
        }

        private void BracingSegments_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            SchedulePreviewRedraw();
        }

        /// <summary>
        /// Coalesces preview redraws: a burst of PropertyChanged notifications (~35 on open/generate/restore)
        /// used to rebuild the whole canvas once PER notification; now the burst draws ONCE when idle.
        /// </summary>
        private void SchedulePreviewRedraw()
        {
            if (previewRedrawQueued)
            {
                return;
            }

            previewRedrawQueued = true;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                previewRedrawQueued = false;
                DrawPreview();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private bool previewRedrawQueued;

        private void Dispatcher_UnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            ViewModel?.ReportNonBlockingError(e.Exception);
            e.Handled = true;
        }

        private void RunUiAction(Action action)
        {
            try
            {
                action();
                DrawPreview();
            }
            catch (Exception ex)
            {
                ViewModel.ReportNonBlockingError(ex);
            }
        }

        private void SyncSelectedSegments()
        {
            if (ViewModel == null || BracingSegmentsGrid == null)
            {
                return;
            }

            var selectedSegments = BracingSegmentsGrid.SelectedItems
                .OfType<BracingSegmentEditorRow>()
                .ToList();
            var activeSegment = BracingSegmentsGrid.SelectedItem as BracingSegmentEditorRow;

            if (activeSegment == null && selectedSegments.Count > 0)
            {
                activeSegment = selectedSegments[0];
            }

            ViewModel.SelectedBracingSegment = activeSegment;
            ViewModel.SetSelectedSegments(selectedSegments);
        }

        private void SelectSingleSegment(BracingSegmentEditorRow segment)
        {
            if (segment == null)
            {
                return;
            }

            ViewModel.SelectedBracingSegment = segment;
            ViewModel.SetSelectedSegments(new[] { segment });
            SyncGridSelectionFromViewModel();
            SyncTreeSelectionFromViewModel();
            DrawPreview();
        }

        private void SyncGridSelectionFromViewModel()
        {
            if (BracingSegmentsGrid == null || ViewModel.SelectedBracingSegment == null)
            {
                return;
            }

            if (!BracingSegmentsGrid.Items.Contains(ViewModel.SelectedBracingSegment))
            {
                return;
            }

            syncingGridSelection = true;

            try
            {
                ClearGridVisualSelection();
                BracingSegmentsGrid.SelectedItem = ViewModel.SelectedBracingSegment;
                BracingSegmentsGrid.ScrollIntoView(ViewModel.SelectedBracingSegment);
            }
            finally
            {
                syncingGridSelection = false;
            }
        }

        private void SyncHorizontalGridSelectionFromViewModel()
        {
            if (HorizontalsGrid == null || ViewModel.SelectedHorizontal == null)
            {
                return;
            }

            if (!HorizontalsGrid.Items.Contains(ViewModel.SelectedHorizontal))
            {
                return;
            }

            syncingGridSelection = true;

            try
            {
                HorizontalsGrid.SelectedItem = ViewModel.SelectedHorizontal;
                HorizontalsGrid.ScrollIntoView(ViewModel.SelectedHorizontal);
            }
            finally
            {
                syncingGridSelection = false;
            }
        }

        private void ClearGridSelection()
        {
            if (BracingSegmentsGrid == null)
            {
                return;
            }

            syncingGridSelection = true;

            try
            {
                ClearGridVisualSelection();
            }
            finally
            {
                syncingGridSelection = false;
            }

            ViewModel.SelectedBracingSegment = null;
            ViewModel.SetSelectedSegments(Array.Empty<BracingSegmentEditorRow>());
        }

        private void ClearGridVisualSelection()
        {
            try
            {
                BracingSegmentsGrid.UnselectAll();
                BracingSegmentsGrid.UnselectAllCells();
            }
            catch (ArgumentNullException ex) when (ex.ParamName == "item")
            {
                // WPF can raise this while clearing a DataGrid selection during a TreeView selection change.
                // The view model is still cleared immediately after this visual cleanup path.
            }
        }

        private static bool ShouldKeepSegmentContext(ConfiguratorNavigationItem item)
        {
            if (item == null)
            {
                return false;
            }

            return item.Key == "Segments" || item.Key == "Horizontals" || item.Key == "Connections";
        }

        private void SyncTreeSelectionFromViewModel()
        {
            if (ModelTree == null)
            {
                return;
            }

            var navigationItem = ViewModel.SelectedNavigationItem;

            if (navigationItem == null)
            {
                return;
            }

            syncingTreeSelection = true;

            try
            {
                var treeViewItem = FindTreeViewItem(ModelTree, navigationItem);

                if (treeViewItem != null)
                {
                    treeViewItem.IsSelected = true;
                    treeViewItem.BringIntoView();
                }
            }
            finally
            {
                syncingTreeSelection = false;
            }
        }

        private static TreeViewItem FindTreeViewItem(ItemsControl parent, object item)
        {
            if (parent == null || item == null)
            {
                return null;
            }

            parent.UpdateLayout();

            var directContainer = parent.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;

            if (directContainer != null)
            {
                return directContainer;
            }

            foreach (var childItem in parent.Items)
            {
                var childContainer = parent.ItemContainerGenerator.ContainerFromItem(childItem) as TreeViewItem;

                if (childContainer == null)
                {
                    continue;
                }

                // Expanding forces the child's containers to generate so we can search under it. Only KEEP the
                // expansion if the target is in this branch; otherwise restore the node's previous state so
                // syncing selection never re-expands branches the user had collapsed.
                var wasExpanded = childContainer.IsExpanded;
                childContainer.IsExpanded = true;
                var result = FindTreeViewItem(childContainer, item);

                if (result != null)
                {
                    return result;
                }

                childContainer.IsExpanded = wasExpanded;
            }

            return null;
        }

        private void DrawPreview()
        {
            if (PreviewCanvas == null || ViewModel == null)
            {
                return;
            }

            PreviewCanvas.Children.Clear();

            var width = PreviewCanvas.ActualWidth;
            var height = PreviewCanvas.ActualHeight;
            var layout = RackFrameEngineeringPreviewLayout.Create(ViewModel, width, height);

            if (!layout.HasValidGeometry)
            {
                return;
            }

            DrawPreviewSummary(layout);
            DrawConfiguredHeightDimension(layout);
            DrawSegmentBackgrounds(layout);
            DrawSegmentBoundaries(layout);
            DrawHorizontals(layout);
            DrawPosts(layout);
            DrawTargetHeight(layout);
            DrawDepthDimension(layout);
            DrawSegmentLabels(layout);
            DrawConnectionPoints(layout, ViewModel.ActiveDetailKey == "Connections");
            DrawActiveElementHighlight(layout);
            DrawSegmentHitAreas(layout);
        }

        private void DrawPreviewSummary(RackFrameEngineeringPreviewLayout layout)
        {
            var brush = new SolidColorBrush(Color.FromRgb(65, 81, 97));
            var title = "Modelo: " + ViewModel.ConfiguredHeightText + " alto / " +
                layout.Depth.ToString("0.##", CultureInfo.InvariantCulture) + " fondo";
            AddText(title, 12.0, 10.0, brush, 10.5, FontWeights.SemiBold, Math.Max(80.0, layout.Width - 24.0));
        }

        private void DrawSegmentBackgrounds(RackFrameEngineeringPreviewLayout layout)
        {
            foreach (var segment in layout.Segments)
            {
                var fill = segment.IsSelected ? SelectedFill : segment.IsModified ? ModifiedFill : null;

                if (fill == null)
                {
                    continue;
                }

                var rectangle = new Rectangle
                {
                    Width = layout.FrameWidth,
                    Height = Math.Max(4.0, segment.Bottom - segment.Top),
                    Fill = fill,
                    StrokeThickness = 0.0
                };

                Canvas.SetLeft(rectangle, layout.LeftX);
                Canvas.SetTop(rectangle, segment.Top);
                PreviewCanvas.Children.Add(rectangle);
            }
        }

        private void DrawSegmentBoundaries(RackFrameEngineeringPreviewLayout layout)
        {
            foreach (var segment in layout.Segments)
            {
                var stroke = GetSideBrush(segment.SideMode);
                var indicatorHeight = Math.Max(4.0, segment.Bottom - segment.Top);

                AddLine(layout.LeftX, segment.StartY, layout.RightX, segment.StartY, SegmentBoundaryStroke, 1.0);
                AddLine(layout.LeftX, segment.EndY, layout.RightX, segment.EndY, SegmentBoundaryStroke, 1.0);
                AddSideIndicator(layout.LeftX - 9.0, segment.Top, indicatorHeight, stroke);
                DrawElevationTick(layout.LeftX, layout.ToY(segment.StartElevation), segment.StartElevation);
                DrawPattern(segment, layout, stroke);
            }

            if (layout.Segments.Count > 0)
            {
                var lastSegment = layout.Segments[layout.Segments.Count - 1];
                DrawElevationTick(layout.LeftX, layout.ToY(lastSegment.EndElevation), lastSegment.EndElevation);
            }
        }

        private void DrawPattern(RackFrameEngineeringPreviewSegment segment, RackFrameEngineeringPreviewLayout layout, Brush stroke)
        {
            if (segment.Members.Count == 0)
            {
                return;
            }

            foreach (var member in segment.Members)
            {
                DrawFrameMember(member, layout);
            }
        }

        private void DrawFrameMember(FrameMember member, RackFrameEngineeringPreviewLayout layout)
        {
            if (member == null || member.Start == null || member.End == null)
            {
                return;
            }

            var offset = GetMountingFaceOffset(member.MountingFace);
            var x1 = GetMemberEndX(layout, member.Start) + offset;
            var y1 = layout.ToMemberY(member.Start.Elevation);
            var x2 = GetMemberEndX(layout, member.End) + offset;
            var y2 = layout.ToMemberY(member.End.Elevation);
            var thickness = IsHorizontalMember(member) ? 3.0 : 2.0;
            var stroke = IsHorizontalMember(member) ? HorizontalStroke : GetSideBrush(member.MountingFace);

            if (IsHorizontalMember(member) && member.Quantity > 1)
            {
                var count = Math.Min(member.Quantity, 4);
                var startOffset = -(count - 1) * 2.0;

                for (var index = 0; index < count; index++)
                {
                    var yOffset = startOffset + index * 4.0;
                    AddLine(x1, y1 + yOffset, x2, y2 + yOffset, stroke, thickness);
                }

                return;
            }

            AddLine(x1, y1, x2, y2, stroke, thickness);
        }

        private void DrawHorizontals(RackFrameEngineeringPreviewLayout layout)
        {
            foreach (var member in layout.HorizontalMembers)
            {
                DrawFrameMember(member, layout);
            }
        }

        private void DrawPosts(RackFrameEngineeringPreviewLayout layout)
        {
            AddPost(layout.LeftPost, "Izq");
            AddPost(layout.RightPost, "Der");

            AddBasePlate(layout.LeftPlate);
            AddBasePlate(layout.RightPlate);
        }

        private void AddPost(RackFrameEngineeringPreviewPost post, string label)
        {
            var rectangle = new Rectangle
            {
                Width = 14.0,
                Height = post.BottomY - post.TopY,
                Fill = PostFill,
                Stroke = PostStroke,
                StrokeThickness = 2.0
            };

            Canvas.SetLeft(rectangle, post.X - 7.0);
            Canvas.SetTop(rectangle, post.TopY);
            PreviewCanvas.Children.Add(rectangle);

            AddLine(post.X - 3.5, post.TopY + 5.0, post.X - 3.5, post.BottomY - 5.0, StandardStroke, 0.8);
            AddLine(post.X + 3.5, post.TopY + 5.0, post.X + 3.5, post.BottomY - 5.0, StandardStroke, 0.8);
            AddLine(post.X - 5.0, post.TopY + 9.0, post.X + 5.0, post.TopY + 9.0, StandardStroke, 0.8);
            AddLine(post.X - 5.0, post.BottomY - 9.0, post.X + 5.0, post.BottomY - 9.0, StandardStroke, 0.8);

            if (post.HasReinforcement)
            {
                // Draw the reinforcement only up to its own height (the reinforced zone), on the inner side.
                var offset = post.ReinforcementOnRightSide ? 13.0 : -13.0;
                var reinforcementTopY = post.ReinforcementTopY > 0.0 && post.ReinforcementTopY < post.BottomY
                    ? post.ReinforcementTopY
                    : post.TopY;
                AddLine(post.X + offset, reinforcementTopY, post.X + offset, post.BottomY, ModifiedStroke, 3.0);

                // A short tick marks where the reinforcement zone ends.
                if (reinforcementTopY > post.TopY)
                {
                    AddLine(post.X + offset - 4.0, reinforcementTopY, post.X + offset + 4.0, reinforcementTopY, ModifiedStroke, 1.5);
                }
            }

            var labelX = post.ReinforcementOnRightSide ? post.X - 24.0 : post.X - 16.0;
            var state = post.HasReinforcement ? "Ref." : "Std.";
            AddText(label + " " + state, labelX, post.TopY - 28.0, PostStroke, 9.0, FontWeights.SemiBold);
            AddText(GetCompactCatalogLabel(post.CatalogId), labelX - 8.0, post.TopY - 16.0, PostStroke, 8.5);
        }

        private void AddBasePlate(RackFrameEngineeringPreviewPlate platePlacement)
        {
            var plate = new Rectangle
            {
                Width = 44.0,
                Height = 8.0,
                Fill = new SolidColorBrush(Color.FromRgb(210, 218, 226)),
                Stroke = PostStroke,
                StrokeThickness = 1.0
            };

            Canvas.SetLeft(plate, platePlacement.X - 22.0);
            Canvas.SetTop(plate, platePlacement.BottomY + 5.0);
            PreviewCanvas.Children.Add(plate);

            AddLine(platePlacement.X - 15.0, platePlacement.BottomY + 9.0, platePlacement.X + 15.0, platePlacement.BottomY + 9.0, StandardStroke, 0.8);
        }

        private void DrawTargetHeight(RackFrameEngineeringPreviewLayout layout)
        {
            // The height is derived from the horizontals (top + remate), so this is just the post-top
            // reference line, not a target to compare against.
            var y = layout.ToY(ViewModel.Height);
            var stroke = new SolidColorBrush(Color.FromRgb(47, 133, 90));

            AddLine(layout.LeftX - 22.0, y, layout.RightX + 22.0, y, stroke, 1.6);
            AddText("Altura", layout.LeftX - 42.0, Math.Max(2.0, y - 18.0), stroke, 10.0);
        }

        private void DrawSegmentLabels(RackFrameEngineeringPreviewLayout layout)
        {
            if (layout.Segments.Count == 0)
            {
                return;
            }

            var availableHeight = Math.Max(1.0, layout.BottomY - layout.TopY);
            var compactLabels = layout.Width < 340.0 ||
                layout.Segments.Count * 36.0 > availableHeight ||
                layout.Segments.Any(segment => segment.Bottom - segment.Top < 42.0);
            var labelBlockHeight = compactLabels ? 16.0 : 36.0;
            var labelSpacing = labelBlockHeight + 3.0;
            var labelX = compactLabels
                ? Math.Min(layout.RightX + 12.0, Math.Max(layout.RightX + 8.0, layout.Width - 86.0))
                : Math.Min(layout.RightX + 16.0, Math.Max(layout.RightX + 10.0, layout.Width - 128.0));
            var labelWidth = Math.Max(54.0, layout.Width - labelX - 6.0);
            var forceDistributedLabels = layout.Segments.Count * labelSpacing > availableHeight;
            var previousY = double.NegativeInfinity;

            for (var index = 0; index < layout.Segments.Count; index++)
            {
                var segment = layout.Segments[index];
                var brush = segment.IsModified ? ModifiedStroke : new SolidColorBrush(Color.FromRgb(65, 81, 97));
                var desiredY = segment.MiddleY - (labelBlockHeight / 2.0);
                var labelY = desiredY;

                if (forceDistributedLabels && layout.Segments.Count > 1)
                {
                    labelY = layout.TopY + index * ((availableHeight - labelBlockHeight) / (layout.Segments.Count - 1));
                }
                else
                {
                    labelY = Math.Max(labelY, previousY + labelSpacing);
                }

                labelY = Math.Clamp(labelY, layout.TopY, Math.Max(layout.TopY, layout.BottomY - labelBlockHeight));
                previousY = labelY;

                if (compactLabels)
                {
                    var compactText = "P" + segment.Index + "  " + FormatPreviewInches(segment.ClearHeight);
                    AddText(compactText, labelX, labelY, brush, 8.5, FontWeights.SemiBold, labelWidth);
                    continue;
                }

                var topLabel = "P" + segment.Index + "  " + FormatPreviewInches(segment.ClearHeight);
                var elevationLabel = FormatPreviewInches(segment.StartElevation) + "-" + FormatPreviewInches(segment.EndElevation);
                AddText(topLabel, labelX, labelY, brush, 10.0, FontWeights.SemiBold, labelWidth);
                AddText(elevationLabel, labelX, labelY + 12.0, new SolidColorBrush(Color.FromRgb(97, 112, 128)), 8.5, null, labelWidth);
                AddText(segment.Pattern + " / " + segment.SideMode, labelX, labelY + 24.0, GetSideBrush(segment.SideMode), 8.5, null, labelWidth);
            }
        }

        private void DrawConnectionPoints(RackFrameEngineeringPreviewLayout layout, bool showAll)
        {
            foreach (var segment in layout.Segments)
            {
                if (!showAll && !segment.IsSelected)
                {
                    continue;
                }

                var fallbackBrush = segment.IsSelected ? ActiveElementStroke : new SolidColorBrush(Color.FromRgb(97, 112, 128));
                var radius = segment.IsSelected ? 4.5 : 3.0;

                if (segment.Members.Count == 0)
                {
                    AddConnectionPoint(layout.LeftX, segment.StartY, radius, fallbackBrush, segment.StartConnectionPointId,
                        segment.Source, "Punto de conexion: " + (segment.StartConnectionPointId ?? "(sin punto)"));
                    AddConnectionPoint(layout.RightX, segment.EndY, radius, fallbackBrush, segment.EndConnectionPointId,
                        segment.Source, "Punto de conexion: " + (segment.EndConnectionPointId ?? "(sin punto)"));
                    continue;
                }

                foreach (var member in segment.Members)
                {
                    var brush = segment.IsSelected ? ActiveElementStroke : GetSideBrush(member.MountingFace);
                    AddConnectionPoint(GetMemberEndX(layout, member.Start) + GetMountingFaceOffset(member.MountingFace), layout.ToMemberY(member.Start.Elevation), radius, brush, member.Start.ConnectionPointId,
                        segment.Source, ConnectionPointTooltip(member.Start, member.MountingFace));
                    AddConnectionPoint(GetMemberEndX(layout, member.End) + GetMountingFaceOffset(member.MountingFace), layout.ToMemberY(member.End.Elevation), radius, brush, member.End.ConnectionPointId,
                        segment.Source, ConnectionPointTooltip(member.End, member.MountingFace));
                }
            }
        }

        private void DrawSegmentHitAreas(RackFrameEngineeringPreviewLayout layout)
        {
            foreach (var segment in layout.Segments)
            {
                var rectangle = new Rectangle
                {
                    Width = layout.FrameWidth + 96.0,
                    Height = Math.Max(4.0, segment.Bottom - segment.Top),
                    Fill = Brushes.Transparent,
                    StrokeThickness = 0.0,
                    Tag = segment.Source
                };

                rectangle.MouseLeftButtonDown += PreviewSegment_MouseLeftButtonDown;
                Canvas.SetLeft(rectangle, layout.LeftX);
                Canvas.SetTop(rectangle, segment.Top);
                PreviewCanvas.Children.Add(rectangle);
            }
        }

        private void DrawActiveElementHighlight(RackFrameEngineeringPreviewLayout layout)
        {
            var activeKey = ViewModel.ActiveDetailKey;

            if (activeKey == "Header" || activeKey == "Other")
            {
                AddRectangleOutline(layout.LeftX - 18.0, layout.TopY - 10.0, layout.FrameWidth + 36.0, layout.BottomY - layout.TopY + 22.0, ActiveElementStroke, 2.0, new DoubleCollection { 6, 4 });
                return;
            }

            if (activeKey == "LeftPost")
            {
                AddRectangleOutline(layout.LeftX - 14.0, layout.TopY - 4.0, 28.0, layout.BottomY - layout.TopY + 8.0, ActiveElementStroke, 2.4);
                return;
            }

            if (activeKey == "RightPost")
            {
                AddRectangleOutline(layout.RightX - 14.0, layout.TopY - 4.0, 28.0, layout.BottomY - layout.TopY + 8.0, ActiveElementStroke, 2.4);
                return;
            }

            if (activeKey == "Segments")
            {
                DrawSelectedSegmentOutline(layout);
                return;
            }

            if (activeKey == "Plates")
            {
                AddRectangleOutline(layout.LeftX - 26.0, layout.BottomY + 2.0, 52.0, 16.0, ActiveElementStroke, 2.2);
                AddRectangleOutline(layout.RightX - 26.0, layout.BottomY + 2.0, 52.0, 16.0, ActiveElementStroke, 2.2);
                AddConnectionPoint(layout.LeftX, layout.BottomY + 9.0, 4.0, ActiveElementStroke, layout.LeftPlate.ConnectionPointId);
                AddConnectionPoint(layout.RightX, layout.BottomY + 9.0, 4.0, ActiveElementStroke, layout.RightPlate.ConnectionPointId);
                return;
            }

            if (activeKey == "Reinforcements")
            {
                AddLine(layout.LeftX + 13.0, layout.TopY, layout.LeftX + 13.0, layout.BottomY, ActiveElementStroke, 4.0, ViewModel.LeftPostHasReinforcement ? null : new DoubleCollection { 6, 5 });
                AddLine(layout.RightX - 13.0, layout.TopY, layout.RightX - 13.0, layout.BottomY, ActiveElementStroke, 4.0, ViewModel.RightPostHasReinforcement ? null : new DoubleCollection { 6, 5 });
                return;
            }

            if (activeKey == "Horizontals")
            {
                DrawHorizontalHighlights(layout);
                return;
            }

            if (activeKey == "Connections")
            {
                DrawConnectionHighlights(layout);
            }
        }

        private void DrawSelectedSegmentOutline(RackFrameEngineeringPreviewLayout layout)
        {
            if (ViewModel.SelectedBracingSegment == null)
            {
                return;
            }

            var segment = layout.Segments.FirstOrDefault(item => item.Source == ViewModel.SelectedBracingSegment);

            if (segment == null)
            {
                return;
            }

            AddRectangleOutline(
                layout.LeftX - 10.0,
                segment.Top - 4.0,
                layout.FrameWidth + 20.0,
                Math.Max(6.0, segment.Bottom - segment.Top) + 8.0,
                ActiveElementStroke,
                2.2,
                new DoubleCollection { 7, 3 });
        }

        private void DrawHorizontalHighlights(RackFrameEngineeringPreviewLayout layout)
        {
            var highlightedAny = false;
            var selectedHorizontalNumber = ViewModel.SelectedHorizontal == null ? (int?)null : ViewModel.SelectedHorizontal.Number;
            var members = selectedHorizontalNumber.HasValue
                ? layout.HorizontalMembers.Where(member => member.SourcePanelIndex == selectedHorizontalNumber.Value)
                : layout.HorizontalMembers;

            foreach (var member in members)
            {
                AddLine(
                    GetMemberEndX(layout, member.Start) - 8.0,
                    layout.ToMemberY(member.Start.Elevation),
                    GetMemberEndX(layout, member.End) + 8.0,
                    layout.ToMemberY(member.End.Elevation),
                    ActiveElementStroke,
                    5.0);
                highlightedAny = true;
            }

            if (!highlightedAny && ViewModel.SelectedBracingSegment != null)
            {
                var segment = layout.Segments.FirstOrDefault(item => item.Source == ViewModel.SelectedBracingSegment);

                if (segment != null)
                {
                    AddLine(layout.LeftX - 8.0, segment.MiddleY, layout.RightX + 8.0, segment.MiddleY, ActiveElementStroke, 4.0, new DoubleCollection { 6, 5 });
                }
            }
        }

        private void DrawConnectionHighlights(RackFrameEngineeringPreviewLayout layout)
        {
            if (ViewModel.SelectedBracingSegment != null)
            {
                var segment = layout.Segments.FirstOrDefault(item => item.Source == ViewModel.SelectedBracingSegment);

                if (segment != null)
                {
                    foreach (var member in segment.Members)
                    {
                        AddConnectionPoint(GetMemberEndX(layout, member.Start) + GetMountingFaceOffset(member.MountingFace), layout.ToMemberY(member.Start.Elevation), 5.0, ActiveElementStroke, member.Start.ConnectionPointId);
                        AddConnectionPoint(GetMemberEndX(layout, member.End) + GetMountingFaceOffset(member.MountingFace), layout.ToMemberY(member.End.Elevation), 5.0, ActiveElementStroke, member.End.ConnectionPointId);
                    }

                    if (segment.Members.Count == 0)
                    {
                        AddConnectionPoint(layout.LeftX, segment.StartY, 5.0, ActiveElementStroke, segment.StartConnectionPointId);
                        AddConnectionPoint(layout.RightX, segment.EndY, 5.0, ActiveElementStroke, segment.EndConnectionPointId);
                    }
                }
            }

            AddConnectionPoint(layout.LeftX, layout.BottomY + 9.0, 5.0, ActiveElementStroke, layout.LeftPlate.ConnectionPointId);
            AddConnectionPoint(layout.RightX, layout.BottomY + 9.0, 5.0, ActiveElementStroke, layout.RightPlate.ConnectionPointId);
        }

        private void AddSideIndicator(double x, double y, double height, Brush brush)
        {
            var rectangle = new Rectangle
            {
                Width = 4.0,
                Height = height,
                Fill = brush,
                StrokeThickness = 0.0
            };

            Canvas.SetLeft(rectangle, x);
            Canvas.SetTop(rectangle, y);
            PreviewCanvas.Children.Add(rectangle);
        }

        private void AddRectangleOutline(double x, double y, double width, double height, Brush stroke, double thickness, DoubleCollection dashArray = null)
        {
            var rectangle = new Rectangle
            {
                Width = Math.Max(1.0, width),
                Height = Math.Max(1.0, height),
                Fill = Brushes.Transparent,
                Stroke = stroke,
                StrokeThickness = thickness
            };

            if (dashArray != null)
            {
                rectangle.StrokeDashArray = dashArray;
            }

            Canvas.SetLeft(rectangle, x);
            Canvas.SetTop(rectangle, y);
            PreviewCanvas.Children.Add(rectangle);
        }

        private void AddCircle(double centerX, double centerY, double radius, Brush stroke, object hitTag = null, string tooltip = null)
        {
            var ellipse = new Ellipse
            {
                Width = radius * 2.0,
                Height = radius * 2.0,
                Fill = Brushes.White,
                Stroke = stroke,
                StrokeThickness = 2.0
            };

            // When given a tag, the point is SELECTABLE: it sits above the segment hit-areas (ZIndex), shows its info
            // on hover and, on click, selects its owning panel (reusing the segment click handler).
            if (hitTag != null)
            {
                ellipse.Tag = hitTag;
                ellipse.Cursor = System.Windows.Input.Cursors.Hand;
                ellipse.MouseLeftButtonDown += PreviewSegment_MouseLeftButtonDown;
                Panel.SetZIndex(ellipse, 10);
            }

            if (!string.IsNullOrWhiteSpace(tooltip))
            {
                ellipse.ToolTip = tooltip;
            }

            Canvas.SetLeft(ellipse, centerX - radius);
            Canvas.SetTop(ellipse, centerY - radius);
            PreviewCanvas.Children.Add(ellipse);
        }

        private void AddConnectionPoint(double centerX, double centerY, double radius, Brush stroke, string label, object hitTag = null, string tooltip = null)
        {
            AddCircle(centerX, centerY, radius, stroke, hitTag, tooltip);

            if (string.IsNullOrWhiteSpace(label))
            {
                return;
            }

            AddText(GetCompactCatalogLabel(label), centerX + radius + 3.0, centerY - 7.0, stroke, 7.5);
        }

        private static string ConnectionPointTooltip(FrameMemberEnd end, FrameSide face)
            => "Punto de conexion: " + (string.IsNullOrWhiteSpace(end?.ConnectionPointId) ? "(sin punto)" : end.ConnectionPointId)
               + "  ·  cara " + face + "  ·  elev " + (end?.Elevation ?? 0.0).ToString("0.##", CultureInfo.InvariantCulture) + " in";

        private void PreviewSegment_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.Tag is BracingSegmentEditorRow segment)
            {
                SelectSingleSegment(segment);
                e.Handled = true;
            }
        }

        private static Brush GetSideBrush(FrameSide sideMode)
        {
            if (sideMode == FrameSide.Front)
            {
                return FrontSideStroke;
            }

            if (sideMode == FrameSide.Back)
            {
                return RearSideStroke;
            }

            if (sideMode == FrameSide.Both)
            {
                return BothSideStroke;
            }

            return BothSideStroke;
        }

        private static bool IsHorizontalMember(FrameMember member)
        {
            return member != null &&
                (member.MemberType == FrameMemberType.LowerHorizontal ||
                 member.MemberType == FrameMemberType.UpperHorizontal ||
                 member.MemberType == FrameMemberType.IntermediateHorizontal ||
                 member.MemberType == FrameMemberType.AdditionalHorizontal);
        }

        private static double GetMountingFaceOffset(FrameSide mountingFace)
        {
            if (mountingFace == FrameSide.Front)
            {
                return -2.0;
            }

            if (mountingFace == FrameSide.Back)
            {
                return 2.0;
            }

            return 0.0;
        }

        private static double GetMemberEndX(RackFrameEngineeringPreviewLayout layout, FrameMemberEnd memberEnd)
        {
            if (memberEnd == null)
            {
                return layout.LeftX;
            }

            return layout.LeftX + (layout.FrameWidth * Math.Clamp(memberEnd.HorizontalPositionRatio, 0.0, 1.0));
        }

        private void DrawConfiguredHeightDimension(RackFrameEngineeringPreviewLayout layout)
        {
            var x = layout.LeftX - 38.0;
            var brush = new SolidColorBrush(Color.FromRgb(97, 112, 128));

            AddLine(x, layout.TopY, x, layout.BottomY, brush, 1.0);
            AddLine(x - 5.0, layout.TopY, x + 5.0, layout.TopY, brush, 1.0);
            AddLine(x - 5.0, layout.BottomY, x + 5.0, layout.BottomY, brush, 1.0);
            AddText(ViewModel.ConfiguredHeightText, Math.Max(2.0, x - 26.0), (layout.TopY + layout.BottomY) / 2.0 - 8.0, brush, 10.0, null, 54.0);
        }

        private void DrawDepthDimension(RackFrameEngineeringPreviewLayout layout)
        {
            var y = layout.BottomY + 28.0;
            var brush = new SolidColorBrush(Color.FromRgb(97, 112, 128));
            var depthText = layout.Depth.ToString("0.##", CultureInfo.InvariantCulture) + " in";

            AddLine(layout.LeftX, y, layout.RightX, y, brush, 1.0);
            AddLine(layout.LeftX, y - 5.0, layout.LeftX, y + 5.0, brush, 1.0);
            AddLine(layout.RightX, y - 5.0, layout.RightX, y + 5.0, brush, 1.0);
            AddText(depthText, (layout.LeftX + layout.RightX) / 2.0 - 18.0, y + 6.0, brush, 10.0);
        }

        private void DrawElevationTick(double leftX, double y, double elevation)
        {
            var brush = new SolidColorBrush(Color.FromRgb(97, 112, 128));
            AddLine(leftX - 20.0, y, leftX - 6.0, y, brush, 0.8);
            AddText(FormatPreviewInches(elevation), Math.Max(2.0, leftX - 66.0), y - 6.0, brush, 7.5, null, 58.0);
        }

        private void AddLine(double x1, double y1, double x2, double y2, Brush stroke, double thickness, DoubleCollection dashArray = null)
        {
            var line = new Line
            {
                X1 = x1,
                Y1 = y1,
                X2 = x2,
                Y2 = y2,
                Stroke = stroke,
                StrokeThickness = thickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };

            if (dashArray != null)
            {
                line.StrokeDashArray = dashArray;
            }

            PreviewCanvas.Children.Add(line);
        }

        private void AddText(string text, double x, double y, Brush brush, double fontSize, FontWeight? fontWeight = null, double maxWidth = 0.0)
        {
            var textBlock = new TextBlock
            {
                Text = text,
                Foreground = brush,
                FontSize = fontSize,
                FontWeight = fontWeight ?? FontWeights.Normal,
                TextTrimming = TextTrimming.CharacterEllipsis
            };

            if (maxWidth > 0.0)
            {
                textBlock.MaxWidth = maxWidth;
            }

            Canvas.SetLeft(textBlock, x);
            Canvas.SetTop(textBlock, y);
            PreviewCanvas.Children.Add(textBlock);
        }

        private static string FormatPreviewInches(double value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture) + " in";
        }

        private string GetCompactCatalogLabel(string value)
        {
            // Use the catalog description instead of stripping id prefixes.
            return ViewModel?.DescribeCatalogId(value) ?? (string.IsNullOrWhiteSpace(value) ? "-" : value);
        }
    }
}
