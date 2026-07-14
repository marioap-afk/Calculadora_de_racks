using System;
using System.Collections.Generic;
using RackCad.Application.Catalogs;
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
        /// Index of the MASTER fondo — the one with the most frentes. Each fondo can have its OWN frente count (a
        /// corner layout), and the longest fondo defines the shared horizontal grid: every shorter fondo is a prefix
        /// of it (their overlapping frentes share its widths, resolved in <see cref="SelectiveGeometryResolver"/>).
        /// Fondo 0 when it ties (the common single-count case). Ties resolve to the lowest index.
        /// </summary>
        public static int MasterFondoIndex(SelectiveRackSystem system)
        {
            var best = 0;
            var bestCount = system?.Bays?.Count ?? 0;
            var count = Count(system);
            for (var k = 1; k < count; k++)
            {
                var c = BaysOfFondo(system, k)?.Count ?? 0;
                if (c > bestCount)
                {
                    bestCount = c;
                    best = k;
                }
            }

            return best;
        }

        /// <summary>The MASTER post grid (X positions + troquel Xs) of the longest fondo — every fondo's posts are a
        /// prefix of it. The planta and lateral place each fondo's frames up to ITS OWN post count on this grid.</summary>
        public static SelectivePostLayout MasterGrid(SelectiveRackSystem system, RackCatalog catalog)
            => SelectivePostGeometry.Compute(FondoSystemView(system, MasterFondoIndex(system)), catalog);

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
                DrawPallets = system.DrawPallets,
                AnnotationScale = system.AnnotationScale,
                Dimensions = system.Dimensions,
                DimensionStyle = system.DimensionStyle,
                Name = system.Name
            };

            foreach (var safety in system.SafetySelections)
            {
                var copy = new SelectiveSafetySelection { ElementId = safety.ElementId, Quantity = safety.Quantity, Side = safety.Side, TopeShared = safety.TopeShared, TopeSaque = safety.TopeSaque, TopeFrontal = safety.TopeFrontal };
                foreach (var post in safety.PostSides)
                {
                    if (post != null) copy.PostSides.Add(new SafetyPostSide { PostIndex = post.PostIndex, Side = post.Side });
                }

                foreach (var off in safety.TopeOffCells)
                {
                    if (off != null) copy.TopeOffCells.Add(new SelectiveGridCell { Frente = off.Frente, Level = off.Level });
                }

                view.SafetySelections.Add(copy);
            }

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

        /// <summary>PALLET depth (in) of fondo <paramref name="k"/> ("Fondo de tarima"): its resolved
        /// <see cref="SelectiveRackSystem.FondoDepths"/> entry when positive, else the run
        /// <see cref="SelectiveRackSystem.PalletDepth"/>, else the default. Each back-to-back line carries its own.
        /// The DRAWN frame is <see cref="CabeceraDepthOfFondo"/> (this minus the cabecera allowance).</summary>
        public static double DepthOfFondo(SelectiveRackSystem system, int k)
        {
            if (system != null && k >= 0 && k < system.FondoDepths.Count && system.FondoDepths[k] > 0.0)
            {
                return system.FondoDepths[k];
            }

            var fallback = system?.PalletDepth ?? 0.0;
            return fallback > 0.0 ? fallback : SelectiveRackDefaults.DefaultPalletDepth;
        }

        /// <summary>The CABECERA (frame) depth of fondo <paramref name="k"/> (in): a per-fondo custom override when set
        /// (<see cref="SelectiveRackSystem.FondoCabeceraOverrides"/> &gt; 0); otherwise the rule — pallet depth minus the
        /// cabecera allowance (<see cref="SelectiveRackDefaults.CabeceraFondoAllowance"/>), so a 48" pallet → a 42"
        /// cabecera, falling back to the pallet depth if the reduction would be non-positive. THIS is the depth the
        /// lateral/planta draw and that the fondo offsets step by.</summary>
        public static double CabeceraDepthOfFondo(SelectiveRackSystem system, int k)
        {
            if (system != null && k >= 0 && k < system.FondoCabeceraOverrides.Count && system.FondoCabeceraOverrides[k] > 0.0)
            {
                return system.FondoCabeceraOverrides[k]; // custom "Fondo de cabecera" for this line
            }

            var pallet = DepthOfFondo(system, k);
            var cabecera = pallet - SelectiveRackDefaults.CabeceraFondoAllowance;
            return cabecera > 0.0 ? cabecera : pallet;
        }

        /// <summary>The total fondo (depth) span of the whole system: frontmost post (offsets[0]=0) to the backmost post
        /// across all fondos. Used as a LATERAL guard's LONGITUD (it runs the full depth).</summary>
        public static double TotalFondoDepth(SelectiveRackSystem system)
        {
            var offsets = Offsets(system);
            if (offsets.Count == 0)
            {
                return 0.0;
            }

            var front = offsets[0];
            var back = 0.0;
            for (var k = 0; k < offsets.Count; k++)
            {
                if (offsets[k] < front) front = offsets[k];
                var b = offsets[k] + CabeceraDepthOfFondo(system, k);
                if (b > back) back = b;
            }

            return back - front;
        }

        /// <summary>
        /// Cumulative X offset of each fondo along the depth axis: fondo 0 at 0, fondo k at
        /// offset[k-1] + cabeceraDepth(fondo k-1) + separator(gap k-1). Steps by the FRAME depth (each fondo can have
        /// its own). Length = <see cref="Count"/>.
        /// </summary>
        public static IReadOnlyList<double> Offsets(SelectiveRackSystem system)
        {
            var count = Count(system);
            var offsets = new double[count];
            for (var k = 1; k < count; k++)
            {
                offsets[k] = offsets[k - 1] + CabeceraDepthOfFondo(system, k - 1) + Separator(system?.SeparatorLengths, k - 1);
            }

            return offsets;
        }
    }
}
