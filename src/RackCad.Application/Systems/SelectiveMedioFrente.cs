using System.Collections.Generic;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Resolves a bay's "medio frente" tramos into placed geometry. A split bay carries N tramos separated by N-1
    /// INTERMEDIATE posts (of this fondo only, so the shared end posts — and thus every fondo — stay aligned).
    /// Each tramo has a larguero length and a loaded flag; the LAST tramo's length is CALCULATED (the remainder of
    /// the bay). The tramos are FREE measures, not tied to pallet counts.
    ///
    /// Geometry: a post spans post-to-post = larguero + 2*(troquelX + inicioPerfilX), so each intermediate post
    /// "consumes" <c>overhead = 2*(troquelX + inicioPerfilX)</c> of the bay (its ménsula overhang on both sides).
    /// Tramo k's left post sits at StartOffset = Σ_{j&lt;k}(L_j + overhead) from the bay's left shared post; the last
    /// tramo's length is <c>BeamLength − Σ specified − (N−1)*overhead</c>, which lands its far post exactly on the
    /// bay's right shared post. If that remainder (or any specified length) is ≤ 0 the split doesn't fit, and the bay
    /// is drawn as a normal full bay (<see cref="Resolve"/> returns null).
    /// </summary>
    public static class SelectiveMedioFrente
    {
        /// <summary>One placed tramo: its larguero length and left-post offset from the bay's left shared post.</summary>
        public readonly struct Tramo
        {
            public Tramo(double startOffset, double length, bool loaded)
            {
                StartOffset = startOffset;
                Length = length;
                Loaded = loaded;
            }

            /// <summary>Distance (in) from the bay's LEFT shared post to this tramo's left post (0 for the first tramo).</summary>
            public double StartOffset { get; }

            /// <summary>This tramo's larguero length (in). For the last tramo it is the calculated remainder.</summary>
            public double Length { get; }

            /// <summary>Whether this tramo carries largueros (a load position) or stays empty.</summary>
            public bool Loaded { get; }
        }

        /// <summary>
        /// Resolves <paramref name="bay"/>'s tramos, or returns null when the bay is NOT split (fewer than 2 tramos)
        /// or the split does not fit geometrically (draw it as a normal full bay in either case). The intermediate
        /// posts are the far posts of every tramo except the last: their offsets are <c>Tramos[k].StartOffset</c> for
        /// k = 1..N-1.
        /// </summary>
        /// <param name="troquelX">The bay posts' larguero-troquel X (slides with the post peralte).</param>
        /// <param name="inicioX">The bay beam's ménsula overhang (INICIO_PERFIL X).</param>
        public static IReadOnlyList<Tramo> Resolve(SelectiveBay bay, double troquelX, double inicioX)
        {
            var segments = bay?.Segments;
            if (segments == null || segments.Count < 2)
            {
                return null; // not split → normal full bay
            }

            var overhead = 2.0 * (troquelX + inicioX);

            double specified = 0.0;
            for (var k = 0; k < segments.Count - 1; k++)
            {
                if (segments[k].Length <= 0.0)
                {
                    return null; // a 0-length specified tramo is nonsense → full bay
                }

                specified += segments[k].Length;
            }

            var lastLength = bay.BeamLength - specified - (segments.Count - 1) * overhead;
            if (lastLength <= 0.0)
            {
                return null; // the tramos + intermediate posts don't fit the bay → full bay
            }

            var tramos = new List<Tramo>(segments.Count);
            var offset = 0.0;
            for (var k = 0; k < segments.Count; k++)
            {
                var length = k < segments.Count - 1 ? segments[k].Length : lastLength;
                tramos.Add(new Tramo(offset, length, segments[k].Loaded));
                offset += length + overhead;
            }

            return tramos;
        }
    }
}
