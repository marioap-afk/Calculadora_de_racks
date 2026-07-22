using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace RackCad.UI.Editor
{
    /// <summary>
    /// The ambient inputs an <see cref="IRackEditorModule"/> needs to open its window (initiative I-15): the modal
    /// <see cref="Owner"/>, whether the host can actually draw in AutoCAD (<see cref="CanInsertInAutoCad"/>, false when
    /// the UI runs standalone) and the dimension-style names the drawing offers. It replaces threading these three
    /// arguments through each per-system handler of <see cref="RackMainMenuWindow"/>.
    /// </summary>
    public sealed class RackEditorLaunchContext
    {
        public RackEditorLaunchContext(Window owner, bool canInsertInAutoCad, IEnumerable<string> dimensionStyles = null)
        {
            Owner = owner;
            CanInsertInAutoCad = canInsertInAutoCad;
            DimensionStyles = (dimensionStyles ?? Enumerable.Empty<string>()).ToList();
        }

        /// <summary>The window that owns the editor's modal dialog (the menu).</summary>
        public Window Owner { get; }

        /// <summary>True when the host command can draw the design in AutoCAD (enables the "Insertar" CTA).</summary>
        public bool CanInsertInAutoCad { get; }

        /// <summary>The dimension-style names read from the active drawing (empty when standalone).</summary>
        public IReadOnlyList<string> DimensionStyles { get; }
    }
}
