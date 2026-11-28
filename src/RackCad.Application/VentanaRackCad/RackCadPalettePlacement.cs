using System;
using System.Collections.Generic;
using System.Linq;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>T6 rl: a rectangle of the screen in device pixels (the units of the work areas of the monitors), left, top, width and height.</summary>
    public readonly record struct ScreenRect(int Left, int Top, int Width, int Height)
    {
        public int Right => Left + Width;

        public int Bottom => Top + Height;
    }

    /// <summary>
    /// T6 rl (A-14 §3, «la ventana recuerda», approved by the Owner on 2026-11-23; guards re-pointed by the Owner on 2026-11-27): AutoCAD
    /// puts the RackCad palette back where the user left it, also on a monitor that is no longer there or with a size larger than the work
    /// area of now. Smoke-1 stopped at SM-03 on a floating palette off the screen. When the palette is shown floating, this rule says where
    /// it must go so the user can reach it: everything in device pixels, so monitors with different scaling compare in the same units.
    /// </summary>
    public static class RackCadPalettePlacement
    {
        /// <summary>The band at the top of the palette that holds its title bar: the part the user drags.</summary>
        public const int TitleBand = 32;

        /// <summary>How much of that band must be on one work area, in width, for the palette to be reachable.</summary>
        public const int MinimumVisibleWidth = 120;

        /// <summary>The distance to the corner of the work area when the palette is brought back (as the first placement, 80 from the corner).</summary>
        public const int Margin = 80;

        /// <summary>
        /// The floating rectangle the palette must take, or null when it is reachable as it is: its title band lies on one work area (all its
        /// height, and <see cref="MinimumVisibleWidth"/> of its width) and it is no larger than that area. A palette larger than the area that
        /// holds its title is cut to that area and moved inside it; a palette whose title is on no work area goes to the primary one,
        /// <see cref="Margin"/> from its corner (or closer, so it fits), with its size cut to it.
        /// </summary>
        public static ScreenRect? KeepOnScreen(ScreenRect palette, IReadOnlyList<ScreenRect> workAreas, ScreenRect primary)
        {
            var areas = (workAreas ?? Array.Empty<ScreenRect>()).Where(a => a.Width > 0 && a.Height > 0).ToList();
            if (primary.Width > 0 && primary.Height > 0 && !areas.Contains(primary)) areas.Add(primary);
            if (areas.Count == 0) return null; // nothing to compare with: the palette stays where AutoCAD put it

            var width = Math.Max(1, palette.Width);
            var height = Math.Max(1, palette.Height);
            var band = Math.Min(TitleBand, height);
            var need = Math.Min(MinimumVisibleWidth, width);

            ScreenRect? host = null;
            var best = 0L;
            foreach (var area in areas)
            {
                var overlapWidth = Math.Min(palette.Left + width, area.Right) - Math.Max(palette.Left, area.Left);
                var bandInside = palette.Top >= area.Top && palette.Top + band <= area.Bottom;
                if (!bandInside || overlapWidth < need) continue;
                if (overlapWidth > best)
                {
                    best = overlapWidth;
                    host = area;
                }
            }

            if (host is ScreenRect found && width <= found.Width && height <= found.Height) return null;

            var target = host ?? (primary.Width > 0 && primary.Height > 0 ? primary : areas[0]);
            var w = Math.Min(width, target.Width);
            var h = Math.Min(height, target.Height);
            int left, top;
            if (host != null)
            {
                left = Clamp(palette.Left, target.Left, target.Right - w);
                top = Clamp(palette.Top, target.Top, target.Bottom - h);
            }
            else
            {
                left = Clamp(target.Left + Margin, target.Left, target.Right - w);
                top = Clamp(target.Top + Margin, target.Top, target.Bottom - h);
            }

            return new ScreenRect(left, top, w, h);
        }

        private static int Clamp(int value, int min, int max) => max < min ? min : Math.Min(Math.Max(value, min), max);
    }
}
