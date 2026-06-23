using System;
using System.Windows;
using RackCad.Application.Catalogs;
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

        public FlowBedConfiguration FlowBedToInsert { get; private set; }

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
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudo abrir el sistema dinamico: " + ex.Message,
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
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudo abrir la cama de rodamiento: " + ex.Message,
                    "RackCad", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
