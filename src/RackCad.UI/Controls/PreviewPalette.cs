using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace RackCad.UI.Controls
{
    /// <summary>
    /// The shared, frozen brush palette for the preview panels. Today each window declares its own
    /// <c>static readonly Brush</c> block and several colors are byte-identical across windows (e.g. the floor
    /// #6A7B8A and label #9AA7B4 lines, and the #3DC986 structure green). Centralizing them here — frozen once,
    /// so they are cheap and thread-safe — lets adopters map their local brush to the same hex and keep the
    /// preview looking identical. Names are roles; the hex values are the ones already on screen.
    /// </summary>
    public static class PreviewPalette
    {
        /// <summary>Structural members (posts/horizontals/rollers), green #3DC986.</summary>
        public static readonly Brush Structure = Frozen(0x3D, 0xC9, 0x86);

        /// <summary>Load beams / largueros, blue #5B8DEF.</summary>
        public static readonly Brush Beam = Frozen(0x5B, 0x8D, 0xEF);

        /// <summary>Floor line, slate #6A7B8A.</summary>
        public static readonly Brush Floor = Frozen(0x6A, 0x7B, 0x8A);

        /// <summary>Labels / dimension guides, muted #9AA7B4.</summary>
        public static readonly Brush Label = Frozen(0x9A, 0xA7, 0xB4);

        /// <summary>Warnings / highlights, red #FF6B6B.</summary>
        public static readonly Brush Warning = Frozen(0xFF, 0x6B, 0x6B);

        /// <summary>Secondary guides / faint fills, pale blue #CFDBE8.</summary>
        public static readonly Brush Guide = Frozen(0xCF, 0xDB, 0xE8);

        /// <summary>Accent (pallets / callouts), amber #E08A2B.</summary>
        public static readonly Brush Accent = Frozen(0xE0, 0x8A, 0x2B);

        /// <summary>Muted outlines, grey #B7C3CF.</summary>
        public static readonly Brush Muted = Frozen(0xB7, 0xC3, 0xCF);

        private static readonly ReadOnlyDictionary<string, Brush> ByName = new ReadOnlyDictionary<string, Brush>(
            new Dictionary<string, Brush>
            {
                ["Structure"] = Structure,
                ["Beam"] = Beam,
                ["Floor"] = Floor,
                ["Label"] = Label,
                ["Warning"] = Warning,
                ["Guide"] = Guide,
                ["Accent"] = Accent,
                ["Muted"] = Muted,
            });

        /// <summary>The palette keyed by role name, for adopters that resolve brushes dynamically.</summary>
        public static IReadOnlyDictionary<string, Brush> Named => ByName;

        private static Brush Frozen(byte r, byte g, byte b) => UiSupport.FrozenBrush(Color.FromRgb(r, g, b));
    }
}
