using System;
using System.Collections.Generic;
using System.Windows;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.UI.Editor;

namespace RackCad.UI.VentanaRackCad
{
    /// <summary>
    /// A design of the library chosen in the RackCad window, opened in its classic editor (I-64 A-14, fourth batch, part «ut», UT-03: the
    /// systems that are not edited in the window yet). It does what the classic library does after the user picks a design
    /// (<see cref="RackCadClassicFlows.OpenFromLibrary"/>), without its list: the same module of the registry
    /// (<see cref="EditorModuleRegistry.ResolveForLibrary"/>), the same entry (<see cref="RackDesignLibrary.EntryOf"/>) and the same result,
    /// one typed insertion request or null. The editor always opens the design as a NEW rack. The host command draws the request.
    /// </summary>
    public static class RackCadLibraryFlows
    {
        /// <summary>Opens the design of <paramref name="path"/> in its editor, owned by <paramref name="owner"/>.</summary>
        public static RackInsertionRequest OpenDesign(Window owner, string path, IEnumerable<string> dimensionStyles)
        {
            try
            {
                var project = new RackProjectStore().Load(path);
                var entry = RackDesignLibrary.EntryOf(path, project, SystemRegistry.Default);
                var module = entry == null ? null : EditorModuleRegistry.Default.ResolveForLibrary(entry, project);
                if (module == null)
                {
                    MessageBox.Show(owner, "El diseño seleccionado no se pudo interpretar.", "RackCad", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return null;
                }

                return module.OpenFromLibrary(project, entry, new RackEditorLaunchContext(owner, true, dimensionStyles));
            }
            catch (Exception ex)
            {
                MessageBox.Show(owner, "No se pudo abrir el diseño de la biblioteca: " + ex.Message, "RackCad", MessageBoxButton.OK, MessageBoxImage.Warning);
                return null;
            }
        }
    }
}
