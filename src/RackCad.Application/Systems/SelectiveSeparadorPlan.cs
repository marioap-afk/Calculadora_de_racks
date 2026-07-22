using System.Collections.Generic;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// The physical SEPARADOR topology of a doble-profundidad run (I-22, E6): at a given frente/corte index, which
    /// fondos REACH it, the adjacent pairs, and the gap each separador spans. This is the ONE place that traversal
    /// lives; the lateral projects it as a vertical stack per corte, the planta as one collapsed line per frente, and
    /// the BOM counts the lateral projection. The gap length is view-independent (the corte's anchor offset cancels
    /// out), so both views read the same topology. Pure — no AutoCAD, no per-view geometry.
    /// </summary>
    public static class SelectiveSeparadorPlan
    {
        /// <summary>One gap between two adjacent reaching fondos: the two fondos, the ABSOLUTE depth X of fondo
        /// <see cref="FrontFondo"/>'s back post, and the gap the separador spans to fondo <see cref="BackFondo"/>'s
        /// front post.</summary>
        public readonly struct Gap
        {
            public Gap(int frontFondo, int backFondo, double backOffset, double length)
            {
                FrontFondo = frontFondo;
                BackFondo = backFondo;
                BackOffset = backOffset;
                Length = length;
            }

            /// <summary>Fondo k — its back post bounds the front of the gap.</summary>
            public int FrontFondo { get; }

            /// <summary>Fondo k+1 — its front post bounds the back of the gap.</summary>
            public int BackFondo { get; }

            /// <summary>Absolute depth X of fondo k's back post: <c>offsets[k] + cabeceraDepth(k)</c>. A view subtracts
            /// its own anchor offset before drawing.</summary>
            public double BackOffset { get; }

            /// <summary>The gap the separador spans (its LONGITUD).</summary>
            public double Length { get; }
        }

        /// <summary>The fondos that REACH frente/corte <paramref name="index"/>: fondo k reaches when
        /// <paramref name="index"/> &lt;= its bay count (a shorter fondo drops out of the far frentes in a corner
        /// layout). In depth order.</summary>
        public static IReadOnlyList<int> ReachingFondos(SelectiveRackSystem system, int index)
        {
            var reaching = new List<int>();
            var count = SelectiveDepthLayout.Count(system);
            for (var k = 0; k < count; k++)
            {
                if (index <= (SelectiveDepthLayout.BaysOfFondo(system, k)?.Count ?? 0))
                {
                    reaching.Add(k);
                }
            }

            return reaching;
        }

        /// <summary>The separador gaps at frente/corte <paramref name="index"/>: one per adjacent pair of reaching
        /// fondos whose gap is positive.</summary>
        public static IReadOnlyList<Gap> GapsAt(SelectiveRackSystem system, int index)
        {
            var reaching = ReachingFondos(system, index);
            var gaps = new List<Gap>();
            if (reaching.Count < 2)
            {
                return gaps;
            }

            var offsets = SelectiveDepthLayout.Offsets(system);
            for (var r = 0; r + 1 < reaching.Count; r++)
            {
                var k = reaching[r];
                var kNext = reaching[r + 1];
                var backOffset = offsets[k] + SelectiveDepthLayout.CabeceraDepthOfFondo(system, k); // fondo k's back post
                var length = offsets[kNext] - backOffset;                                           // to fondo k+1's front post
                if (length > 0.0)
                {
                    gaps.Add(new Gap(k, kNext, backOffset, length));
                }
            }

            return gaps;
        }
    }
}
