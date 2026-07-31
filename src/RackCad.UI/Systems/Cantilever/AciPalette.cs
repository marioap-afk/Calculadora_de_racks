using System.Collections.Generic;
using System.Windows.Media;

namespace RackCad.UI.Systems.Cantilever
{
    /// <summary>
    /// Turns an AutoCAD Color Index into the colour a screen shows for it.
    ///
    /// It exists so the preview can honour a palette that Application declares in ACI — the unit a layer's
    /// colour is expressed in — without Application knowing what a WPF <see cref="Color"/> is. Adapter, not
    /// authority: it chooses nothing, it converts.
    ///
    /// It covers the indices the Cantilever roles actually declare, not all 256. A partial table is honest
    /// about what it is; inventing the other 247 from a formula would produce colours that do not match what
    /// AutoCAD shows, which is worse than not having them. A test walks every role and fails if one of them
    /// names an index that is missing here.
    /// </summary>
    internal static class AciPalette
    {
        private static readonly IReadOnlyDictionary<short, Color> Colors = new Dictionary<short, Color>
        {
            [1] = Color.FromRgb(0xFF, 0x00, 0x00),   // rojo
            [2] = Color.FromRgb(0xFF, 0xFF, 0x00),   // amarillo
            [3] = Color.FromRgb(0x00, 0xFF, 0x00),   // verde
            [4] = Color.FromRgb(0x00, 0xFF, 0xFF),   // cian
            [5] = Color.FromRgb(0x00, 0x00, 0xFF),   // azul
            [6] = Color.FromRgb(0xFF, 0x00, 0xFF),   // magenta

            // El 7 es blanco sobre fondo oscuro y negro sobre fondo claro. La previa tiene fondo claro, asi que
            // se muestra oscuro: mostrarlo blanco lo haria invisible justo en el panel donde se revisa.
            [7] = Color.FromRgb(0x20, 0x28, 0x30),

            [30] = Color.FromRgb(0xFF, 0x7F, 0x00),  // naranja
            [252] = Color.FromRgb(0x82, 0x82, 0x82), // gris medio
            [254] = Color.FromRgb(0xBE, 0xBE, 0xBE)  // gris claro
        };

        /// <summary>Whether this index has a declared screen colour.</summary>
        internal static bool Knows(short colorIndex) => Colors.ContainsKey(colorIndex);

        /// <summary>
        /// The colour of an index.
        ///
        /// An index with no entry comes back as a strident magenta rather than as a sensible grey, because an
        /// unmapped colour is a defect and a defect that looks reasonable never gets reported.
        /// </summary>
        internal static Color ColorOf(short colorIndex) =>
            Colors.TryGetValue(colorIndex, out var color) ? color : Color.FromRgb(0xFF, 0x00, 0xFF);
    }
}
