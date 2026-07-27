using System.Windows.Media;
using RackCad.Domain.Systems.Shared;

namespace RackCad.UI.Preview
{
    /// <summary>
    /// The shared preview palette: one brush per drawing ROLE, extracted verbatim from the dynamic editor's renderer so
    /// that every editor paints the same kind of piece the same colour.
    ///
    /// Neutral by contract (ADR-0019 §3): it knows nothing about <c>RackSystemKind</c> and has no branch per system.
    /// The colours are the dynamic editor's originals — changing one changes every editor at once, which is the point.
    /// Brushes are frozen so they can be shared across threads and reused without cloning.
    /// </summary>
    internal static class EditorPreviewPalette
    {
        private static Brush Rgb(byte r, byte g, byte b) => UiSupport.FrozenBrush(Color.FromRgb(r, g, b));

        private static Brush Argb(byte a, byte r, byte g, byte b) => UiSupport.FrozenBrush(Color.FromArgb(a, r, g, b));

        /// <summary>Frame uprights (the header's vertical members).</summary>
        public static readonly Brush Upright = Rgb(0xCF, 0xDB, 0xE8);

        /// <summary>Horizontal frame members — and the intermediate supports that hang from them.</summary>
        public static readonly Brush Horizontal = Rgb(0x3D, 0xC9, 0x86);

        /// <summary>Diagonal braces.</summary>
        public static readonly Brush Diagonal = Rgb(0x5B, 0x8D, 0xEF);

        /// <summary>Separators between headers.</summary>
        public static readonly Brush Separator = Rgb(0x3A, 0x50, 0x68);

        /// <summary>Posts (uprights of the rack itself).</summary>
        public static readonly Brush Post = Rgb(0xFF, 0x6B, 0x6B);

        /// <summary>The floor line.</summary>
        public static readonly Brush Floor = Rgb(0x6A, 0x7B, 0x8A);

        /// <summary>Labels and dimension text.</summary>
        public static readonly Brush Label = Rgb(0x9A, 0xA7, 0xB4);

        /// <summary>The current selection — and the flow bed's end stop.</summary>
        public static readonly Brush Selection = Rgb(0xFF, 0xD1, 0x66);

        /// <summary>Header body fill (translucent).</summary>
        public static readonly Brush HeaderFill = Argb(0x36, 0x2A, 0x5B, 0x78);

        /// <summary>Separator body fill (translucent).</summary>
        public static readonly Brush SeparatorFill = Argb(0x18, 0x8A, 0xA0, 0xB4);

        /// <summary>Module boundaries.</summary>
        public static readonly Brush ModuleBoundary = Argb(0x95, 0x74, 0x86, 0x99);

        /// <summary>Post body fill.</summary>
        public static readonly Brush PostFill = Rgb(0x20, 0x34, 0x48);

        /// <summary>Base-plate fill.</summary>
        public static readonly Brush PlateFill = Rgb(0xB7, 0xC3, 0xCF);

        /// <summary>Reinforcements — and the flow bed's brakes.</summary>
        public static readonly Brush Reinforcement = Rgb(0xF2, 0xA6, 0x3B);

        /// <summary>Flow-bed rails and rollers.</summary>
        public static readonly Brush FlowBed = Rgb(0x59, 0xC3, 0xE6);

        /// <summary>Safety elements (botas, protectores, desviadores, defensa).</summary>
        public static readonly Brush Safety = Rgb(0xFF, 0xC8, 0x57);

        /// <summary>Load beams (IN/OUT and rear).</summary>
        public static readonly Brush LoadBeam = Rgb(0xE0, 0x8A, 0x2B);

        /// <summary>Pallets drawn for reference (never part of the BOM).</summary>
        public static readonly Brush Pallet = Rgb(0x7A, 0x87, 0x94);
    }
}
