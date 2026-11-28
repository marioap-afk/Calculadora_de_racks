using System;
using System.Collections.Generic;
using System.Windows;
using RackCad.Application.Persistence;
using RackCad.Application.Settings;
using RackCad.Domain.Systems.Shared;
using RackCad.UI.Editor;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// The two flows of the classic menu that the RackCad window reaches through a command (I-64 A-14, phase 1): the editor of a new system
    /// and the design library. They do what the buttons of <see cref="RackMainMenuWindow"/> do, with the same registry, the same messages
    /// and the same result (one typed insertion request, or null); the menu keeps its own copy until it retires, so this class changes
    /// nothing of it. The host command draws the request after every window closed.
    /// </summary>
    public static class RackCadClassicFlows
    {
        /// <summary>Opens the editor of <paramref name="kind"/> for a new design, owned by <paramref name="owner"/>.</summary>
        public static RackInsertionRequest OpenForNew(Window owner, RackSystemKind kind, IEnumerable<string> dimensionStyles)
        {
            var module = EditorModuleRegistry.Default.Get(kind);
            try
            {
                return module.OpenForNew(new RackEditorLaunchContext(owner, true, dimensionStyles));
            }
            catch (Exception ex)
            {
                MessageBox.Show(owner, module.OpenFailureMessage + ex.Message, "RackCad", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
        }

        /// <summary>Opens the design library and, when the user picks a design, its editor, owned by <paramref name="owner"/>.</summary>
        public static RackInsertionRequest OpenFromLibrary(Window owner, IEnumerable<string> dimensionStyles)
        {
            try
            {
                var settings = UserSettingsStore.Load();
                var folder = UserSettingsStore.ResolveDesignLibraryPath(settings);
                var library = new RackDesignLibraryWindow(folder) { Owner = owner };
                if (library.ShowDialog() != true || library.SelectedDesign == null) return null;

                var project = new RackProjectStore().Load(library.SelectedDesign.Path);
                var module = EditorModuleRegistry.Default.ResolveForLibrary(library.SelectedDesign, project);
                if (module == null)
                {
                    MessageBox.Show(owner, "El diseño seleccionado no se pudo interpretar.", "RackCad", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }

                return module.OpenFromLibrary(project, library.SelectedDesign, new RackEditorLaunchContext(owner, true, dimensionStyles));
            }
            catch (Exception ex)
            {
                MessageBox.Show(owner, "No se pudo abrir la biblioteca de diseños: " + ex.Message, "RackCad", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
        }
    }
}
