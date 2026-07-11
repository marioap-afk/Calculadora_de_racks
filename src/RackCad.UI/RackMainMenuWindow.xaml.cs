using System;
using System.Windows;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.RackFrames;
using RackCad.Application.Settings;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.UI
{
    /// <summary>
    /// Application entry point: the user picks which system to design. Each option opens an
    /// independent module. The header configurator and the dynamic system are separate windows;
    /// the dynamic module reuses the header factory, not this menu's header window.
    /// </summary>
    public partial class RackMainMenuWindow : Window
    {
        private readonly bool canInsertInAutoCad;
        private readonly UserSettings settings = UserSettingsStore.Load();

        /// <summary>Set when the user asked to insert the configured header; the host command draws it after
        /// every modal window (this menu included) has closed, so the placement jig has the editor free.</summary>
        public bool InsertRequested { get; private set; }

        public RackFrameConfiguration ConfigurationToInsert { get; private set; }

        public DynamicRackSystem DynamicSystemToInsert { get; private set; }

        /// <summary>Identity of the dynamic system to insert (for the drawing round-trip embed).</summary>
        public string DynamicRackId { get; private set; }
        public string DynamicRackName { get; private set; }

        public FlowBedConfiguration FlowBedToInsert { get; private set; }

        /// <summary>Identity of the cama to insert (for the drawing round-trip embed).</summary>
        public string FlowBedRackId { get; private set; }
        public string FlowBedRackName { get; private set; }

        public SelectiveRackSystem SelectiveSystemToInsert { get; private set; }

        /// <summary>The selective design + identity that produced <see cref="SelectiveSystemToInsert"/> (for the drawing embed).</summary>
        public SelectivePalletDesign SelectiveDesignToInsert { get; private set; }
        public string SelectiveRackId { get; private set; }
        public string SelectiveRackName { get; private set; }
        public string SelectiveView { get; private set; }

        public RackMainMenuWindow()
            : this(false)
        {
        }

        public RackMainMenuWindow(bool canInsertInAutoCad)
        {
            this.canInsertInAutoCad = canInsertInAutoCad;
            InitializeComponent();
            UpdateLibraryPathDisplay();
        }

        private void UpdateLibraryPathDisplay()
        {
            var overridden = !string.IsNullOrWhiteSpace(settings.BlockLibraryPath);
            LibraryPathBox.Text = BlockLibraryLocator.ResolvePath();
            LibraryPathBox.ToolTip = overridden
                ? "Ruta personalizada (guardada)."
                : "Ruta predeterminada (junto a los catálogos). Usa Examinar… para elegir otra.";
        }

        private void BrowseLibrary_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Selecciona la biblioteca de bloques",
                Filter = "Dibujo de AutoCAD (*.dwg)|*.dwg|Todos (*.*)|*.*",
                CheckFileExists = true
            };

            var current = BlockLibraryLocator.ResolvePath();
            if (!string.IsNullOrWhiteSpace(current))
            {
                try
                {
                    var directory = System.IO.Path.GetDirectoryName(current);
                    if (!string.IsNullOrEmpty(directory) && System.IO.Directory.Exists(directory))
                    {
                        dialog.InitialDirectory = directory;
                    }

                    if (System.IO.File.Exists(current))
                    {
                        dialog.FileName = System.IO.Path.GetFileName(current);
                    }
                }
                catch
                {
                    // ignore an invalid current path; just open the dialog at its default
                }
            }

            if (dialog.ShowDialog(this) == true)
            {
                settings.BlockLibraryPath = dialog.FileName;
                UserSettingsStore.Save(settings);
                UpdateLibraryPathDisplay();
            }
        }

        private void ResetLibrary_Click(object sender, RoutedEventArgs e)
        {
            settings.BlockLibraryPath = null;
            UserSettingsStore.Save(settings);
            UpdateLibraryPathDisplay();
        }

        private void DesignHeader_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var configuration = new HardcodedStandardRackFrameService().CreateDefault();
                var window = new RackFrameConfiguratorWindow(configuration, canInsertInAutoCad) { Owner = this };
                window.ShowDialog();

                if (window.InsertRequested)
                {
                    // Bubble the request up and close so the host command can run the placement jig.
                    InsertRequested = true;
                    ConfigurationToInsert = window.Configuration;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudo abrir el configurador de cabeceras: " + ex.Message,
                    "RackCad", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DesignDynamic_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new RackDynamicSystemWindow(canInsertInAutoCad) { Owner = this };
                window.ShowDialog();

                if (window.InsertRequested)
                {
                    InsertRequested = true;
                    DynamicSystemToInsert = window.SystemToInsert;
                    DynamicRackId = window.RackId;
                    DynamicRackName = window.RackName;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudo abrir el sistema dinámico: " + ex.Message,
                    "RackCad", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void OpenDesignLibrary_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var folder = UserSettingsStore.ResolveDesignLibraryPath(settings);
                var library = new RackDesignLibraryWindow(folder) { Owner = this };
                if (library.ShowDialog() != true || library.SelectedDesign == null)
                {
                    return;
                }

                var project = new RackProjectStore().Load(library.SelectedDesign.Path);

                if (library.SelectedDesign.Kind == RackDesignKind.Dinamico && project.DynamicSystem != null)
                {
                    var editor = new RackDynamicSystemWindow(canInsertInAutoCad) { Owner = this };
                    editor.LoadDesignForNew(project.DynamicSystem, library.SelectedDesign.Name);
                    editor.ShowDialog();

                    if (editor.InsertRequested)
                    {
                        InsertRequested = true;
                        DynamicSystemToInsert = editor.SystemToInsert;
                        DynamicRackId = editor.RackId;
                        DynamicRackName = editor.RackName;
                        Close();
                    }
                }
                else if (library.SelectedDesign.Kind == RackDesignKind.Selectivo && project.SelectiveRack != null)
                {
                    var editor = new RackSelectiveWindow(canInsertInAutoCad) { Owner = this };
                    editor.LoadForNew(project.SelectiveRack);
                    editor.ShowDialog();

                    if (editor.InsertRequested)
                    {
                        InsertRequested = true;
                        SelectiveSystemToInsert = editor.SystemToInsert;
                        SelectiveDesignToInsert = editor.DesignToInsert;
                        SelectiveRackId = editor.RackId;
                        SelectiveRackName = editor.RackName;
                        SelectiveView = editor.InsertView;
                        Close();
                    }
                }
                else if (library.SelectedDesign.Kind == RackDesignKind.Cama && project.FlowBed != null)
                {
                    var editor = new RackFlowBedWindow(canInsertInAutoCad) { Owner = this };
                    editor.LoadForNew(project.FlowBed, library.SelectedDesign.Name);
                    editor.ShowDialog();

                    if (editor.InsertRequested)
                    {
                        InsertRequested = true;
                        FlowBedToInsert = editor.FlowBedToInsert;
                        FlowBedRackId = editor.RackId;
                        FlowBedRackName = editor.RackName;
                        Close();
                    }
                }
                else if (library.SelectedDesign.Kind == RackDesignKind.Larguero && project.Larguero != null)
                {
                    // Larguero is visual-only (no AutoCAD block) — just open its editor pre-loaded.
                    var editor = new RackLargueroWindow { Owner = this };
                    editor.LoadExisting(project.Larguero);
                    editor.ShowDialog();
                }
                else if (project.Header != null)
                {
                    var editor = new RackFrameConfiguratorWindow(project.Header, canInsertInAutoCad) { Owner = this };
                    editor.ShowDialog();

                    if (editor.InsertRequested)
                    {
                        InsertRequested = true;
                        ConfigurationToInsert = editor.Configuration;
                        Close();
                    }
                }
                else
                {
                    MessageBox.Show(this, "El diseño seleccionado no se pudo interpretar.",
                        "RackCad", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudo abrir la biblioteca de diseños: " + ex.Message,
                    "RackCad", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DesignFlowBed_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new RackFlowBedWindow(canInsertInAutoCad) { Owner = this };
                window.ShowDialog();

                if (window.InsertRequested)
                {
                    InsertRequested = true;
                    FlowBedToInsert = window.FlowBedToInsert;
                    FlowBedRackId = window.RackId;
                    FlowBedRackName = window.RackName;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudo abrir la cama de rodamiento: " + ex.Message,
                    "RackCad", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DesignSelective_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var window = new RackSelectiveWindow(canInsertInAutoCad) { Owner = this };
                window.ShowDialog();

                if (window.InsertRequested)
                {
                    InsertRequested = true;
                    SelectiveSystemToInsert = window.SystemToInsert;
                    SelectiveDesignToInsert = window.DesignToInsert;
                    SelectiveRackId = window.RackId;
                    SelectiveRackName = window.RackName;
                    SelectiveView = window.InsertView;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudo abrir el sistema selectivo: " + ex.Message,
                    "RackCad", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void DesignLarguero_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Visual + BOM only (no AutoCAD block yet): opens, previews, and saves to the library. Never inserts.
                new RackLargueroWindow { Owner = this }.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudo abrir el editor de largueros: " + ex.Message,
                    "RackCad", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
