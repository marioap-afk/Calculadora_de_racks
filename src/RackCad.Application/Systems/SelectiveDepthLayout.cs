using System;
using System.Collections.Generic;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// The DEPTH layout of a selective run: how the fondos (cabecera-lines) stack along the depth axis. A single
    /// fondo (<c>DepthCount = 1</c>) is the classic selective; 2/3/4 fondos are the doble-profundidad / espalda-con-
    /// espalda variant, each fondo repeating the whole depth structure (cabecera + front/back largueros) offset by
    /// the pallet depth plus the per-gap separator. This is the ONE place the offset/separator/count rules live, so
    /// the lateral builder, the planta builder and the BOM all agree. Pure — no AutoCAD.
    /// </summary>
    public static class SelectiveDepthLayout
    {
        /// <summary>Number of fondos, floored at 1 (a null/legacy system is a single fondo).</summary>
        public static int Count(SelectiveRackSystem system) => Math.Max(1, system?.DepthCount ?? 1);

        /// <summary>The resolved bays of fondo <paramref name="k"/> (its own levels/heights); falls back to fondo 0
        /// (<see cref="SelectiveRackSystem.Bays"/>) when that fondo isn't populated — the plain doble-profundidad
        /// case where every fondo shares fondo 0's matrix.</summary>
        public static IList<SelectiveBay> BaysOfFondo(SelectiveRackSystem system, int k)
            => system != null && k >= 0 && k < system.FondoBays.Count && system.FondoBays[k] != null
                ? system.FondoBays[k]
                : system?.Bays;

        /// <summary>
        /// A shallow one-fondo VIEW of the system: its own bays (levels/heights) plus the shared run settings, so the
        /// frontal builder / BOM can lay out that single fondo's face. Custom per-post cabeceras apply to fondo 0 only
        /// (Fase 1); extra fondos use the standard frame. Used by the editor preview (show the fondo being edited) and
        /// the BOM (sum every fondo's real content).
        /// </summary>
        public static SelectiveRackSystem FondoSystemView(SelectiveRackSystem system, int k)
        {
            if (system == null)
            {
                return null;
            }

            var view = new SelectiveRackSystem
            {
                Height = system.Height,
                PostId = system.PostId,
                PostPeralte = system.PostPeralte,
                PalletDepth = system.PalletDepth,
                DrawBasePlate = system.DrawBasePlate,
                NumberFronts = system.NumberFronts,
                NumberLevels = system.NumberLevels,
                DrawRackName = system.DrawRackName,
                AnnotationScale = system.AnnotationScale,
                Name = system.Name
            };

            foreach (var bay in BaysOfFondo(system, k)) view.Bays.Add(bay);
            foreach (var peralte in system.PostPeraltes) view.PostPeraltes.Add(peralte);
            if (k == 0)
            {
                foreach (var cabecera in system.PostCabeceras) view.PostCabeceras.Add(cabecera);
            }

            return view;
        }

        /// <summary>
        /// Separation (in) for gap <paramref name="gapIndex"/> (0-based, between fondo g and g+1): the design's value
        /// at that gap when positive; otherwise the last positive value given (so a single value fills every gap);
        /// otherwise <see cref="SelectiveRackDefaults.DefaultSeparator"/>.
        /// </summary>
        public static double Separator(IList<double> separators, int gapIndex)
        {
            if (separators != null && separators.Count > 0)
            {
                if (gapIndex >= 0 && gapIndex < separators.Count && separators[gapIndex] > 0.0)
                {
                    return separators[gapIndex];
                }

                // Pad: reuse the last positive value at/below this gap (a single "8" fills every gap with 8).
                for (var k = Math.Min(gapIndex, separators.Count - 1); k >= 0; k--)
                {
                    if (separators[k] > 0.0)
                    {
                        return separators[k];
                    }
                }
            }

            return SelectiveRackDefaults.DefaultSeparator;
        }

        /// <summary>
        /// Cumulative X offset of each fondo along the depth axis: fondo 0 at 0, fondo k at
        /// offset[k-1] + <paramref name="depth"/> + separator(gap k-1). Length = <see cref="Count"/>.
        /// </summary>
        public static IReadOnlyList<double> Offsets(SelectiveRackSystem system, double depth)
        {
            var count = Count(system);
            var offsets = new double[count];
            for (var k = 1; k < count; k++)
            {
                offsets[k] = offsets[k - 1] + depth + Separator(system?.SeparatorLengths, k - 1);
            }

            return offsets;
        }
    }
}
