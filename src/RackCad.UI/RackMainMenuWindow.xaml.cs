using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Application.Settings;
using RackCad.Domain.Systems;
using RackCad.UI.Editor;

namespace RackCad.UI
{
    /// <summary>
    /// Application entry point: the user picks which system to design. Each option opens an independent module through
    /// the <see cref="EditorModuleRegistry"/> (initiative I-15): the per-system <c>Design*_Click</c> handlers and the
    /// library switch delegate to that registry, and the window carries ONE <see cref="InsertionRequest"/> payload
    /// instead of the ~19 per-system properties it used to expose. The header configurator and the dynamic system are
    /// separate windows; the dynamic module reuses the header factory, not this menu's header window.
    /// </summary>
    public partial class RackMainMenuWindow : Window
    {
        private readonly bool canInsertInAutoCad;
        private readonly IReadOnlyList<string> dimensionStyles;
        private readonly UserSettings settings = UserSettingsStore.Load();
        private readonly EditorModuleRegistry registry = EditorModuleRegistry.Default;

        /// <summary>
        /// The design the user asked to insert, as a single typed payload the host command draws after every modal window
        /// (this menu included) has closed, so the placement jig has the editor free. Null unless an editor closed with an
        /// insert request. Replaces the old per-system <c>*ToInsert</c> property bag (I-15).
        /// </summary>
        public RackInsertionRequest InsertionRequest { get; private set; }

        /// <summary>True when <see cref="InsertionRequest"/> is set (an editor asked to draw its design).</summary>
        public bool InsertRequested => InsertionRequest != null;

        public RackMainMenuWindow()
            : this(false, null)
        {
        }

        public RackMainMenuWindow(bool canInsertInAutoCad)
            : this(canInsertInAutoCad, null)
        {
        }

        public RackMainMenuWindow(bool canInsertInAutoCad, IEnumerable<string> dimensionStyles)
        {
            this.canInsertInAutoCad = canInsertInAutoCad;
            this.dimensionStyles = (dimensionStyles ?? Enumerable.Empty<string>()).ToList();
            InitializeComponent();
            UpdateLibraryPathDisplay();
        }

        private RackEditorLaunchContext LaunchContext()
            => new RackEditorLaunchContext(this, canInsertInAutoCad, dimensionStyles);

        private void UpdateLibraryPathDisplay()
        {
            var overridden = !string.IsNullOrWhiteSpace(settings.BlockLibraryPath);
            var path = BlockLibraryLocator.ResolvePath();
            var exists = !string.IsNullOrWhiteSpace(path) && System.IO.File.Exists(path);
            LibraryPathBox.Text = path ?? string.Empty;
            LibraryPathBox.ToolTip = (overridden
                ? "Ruta personalizada (guardada)."
                : "Ruta predeterminada (junto a los catálogos). Usa Examinar… para elegir otra.")
                + (exists ? string.Empty : " El archivo no existe en esa ubicación.");
            UiSupport.SetStatus(
                LibraryStatusText,
                exists
                    ? "Biblioteca disponible."
                    : "Biblioteca no encontrada: las piezas que no existan en el dibujo activo se omitirán.",
                !exists);
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

        // The six "Diseñar …" buttons: each names its kind and delegates to the shared, registry-driven launcher. No
        // per-system payload copying lives here anymore (I-15) — the module builds the InsertionRequest.
        private void DesignSelective_Click(object sender, RoutedEventArgs e) => LaunchDesignModule(RackSystemKind.SelectiveRack);

        private void DesignDynamic_Click(object sender, RoutedEventArgs e) => LaunchDesignModule(RackSystemKind.PalletFlow);

        private void DesignPushBack_Click(object sender, RoutedEventArgs e) => LaunchDesignModule(RackSystemKind.PushBack);

        private void DesignHeader_Click(object sender, RoutedEventArgs e) => LaunchDesignModule(RackSystemKind.Selective);

        private void DesignFlowBed_Click(object sender, RoutedEventArgs e) => LaunchDesignModule(RackSystemKind.Cama);

        private void DesignLarguero_Click(object sender, RoutedEventArgs e) => LaunchDesignModule(RackSystemKind.Larguero);

        /// <summary>Opens the module for <paramref name="kind"/> for a NEW design; if it returns an insert request, bubbles
        /// it up and closes so the host command can run the placement jig. Otherwise the menu stays open.</summary>
        private void LaunchDesignModule(RackSystemKind kind)
        {
            var module = registry.Get(kind);
            try
            {
                var request = module.OpenForNew(LaunchContext());
                if (request != null)
                {
                    InsertionRequest = request;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, module.OpenFailureMessage + ex.Message,
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

                // Dispatch by the registry (fallback = cabecera, tried last), preserving the old if/else exactly.
                var module = registry.ResolveForLibrary(library.SelectedDesign, project);
                if (module == null)
                {
                    MessageBox.Show(this, "El diseño seleccionado no se pudo interpretar.",
                        "RackCad", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var request = module.OpenFromLibrary(project, library.SelectedDesign, LaunchContext());
                if (request != null)
                {
                    InsertionRequest = request;
                    Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "No se pudo abrir la biblioteca de diseños: " + ex.Message,
                    "RackCad", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
