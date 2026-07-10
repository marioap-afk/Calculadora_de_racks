using System.Collections.Generic;
using RackCad.Application.Catalogs;
using RackCad.Application.Geometry;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// The X positions of a resolved selective run's N+1 posts, shared by the FRONTAL and LATERAL builders so
    /// both views line up on the same run. post[i+1] = post[i] + beamLength + 2*(troquelX + inicioPerfilX),
    /// where troquelX (post's larguero troquel) slides with the post peralte and inicioPerfilX is the ménsula
    /// overhang. Pure; the only catalog reads are the parametric connection-point X values.
    /// </summary>
    public static class SelectivePostGeometry
    {
        public static SelectivePostLayout Compute(SelectiveRackSystem system, RackCatalog catalog)
        {
            var view = SelectiveRackDefaults.View;
            var troquel = catalog?.ConnectionLayout.FindConnectionLayout(system.PostId, SelectiveRackDefaults.PostBeamPoint, view);

            // Each post's larguero troquel X slides with ITS OWN peralte (per-post), so posts of different peralte
            // land correctly and the bay between them absorbs both troquel offsets. Resolved ONCE per post up
            // front: the loop below reads interior posts 3 times, and every resolve allocated a parameter
            // Dictionary. (One Dictionary per post is kept on purpose — reusing a mutated one would rely on
            // ResolveX never retaining the reference.)
            var troquelXs = new List<double>(system.Bays.Count + 1);
            for (var i = 0; i <= system.Bays.Count; i++)
            {
                troquelXs.Add(ResolveX(troquel, new Dictionary<string, double> { [SelectiveRackDefaults.PeralteParam] = PostPeralteAt(system, i) }));
            }

            var xs = new List<double> { 0.0 };
            for (var b = 0; b < system.Bays.Count; b++)
            {
                var bay = system.Bays[b];
                var inicioX = BeamProfileStartX(catalog, bay, view);
                // post b's RIGHT troquel + post b+1's LEFT troquel, each with its own peralte. Same expression
                // SHAPE as before the precompute, so the floating-point sums stay bit-identical.
                xs.Add(xs[xs.Count - 1] + bay.BeamLength + (troquelXs[b] + inicioX) + (troquelXs[b + 1] + inicioX));
            }

            return new SelectivePostLayout(xs, troquelXs);
        }

        /// <summary>The effective PERALTE of post <paramref name="postIndex"/>: its resolved per-post value, else the run peralte.</summary>
        public static double PostPeralteAt(SelectiveRackSystem system, int postIndex)
            => system != null && postIndex >= 0 && postIndex < system.PostPeraltes.Count && system.PostPeraltes[postIndex] > 0.0
                ? system.PostPeraltes[postIndex]
                : system?.PostPeralte ?? 0.0;

        /// <summary>Height of post <paramref name="postIndex"/> in fondo 0 = the tallest of the (up to two) bays it
        /// bounds; empty bays fall back to fondo 0's own tallest bay (not the overall system height, which a taller
        /// OTHER fondo could inflate), so the frontal and fondo 0's lateral agree.</summary>
        public static double PostHeight(SelectiveRackSystem system, int postIndex)
        {
            var fondo0Max = MaxBayHeight(system.Bays);
            return PostHeight(system.Bays, postIndex, fondo0Max > 0.0 ? fondo0Max : system.Height);
        }

        private static double MaxBayHeight(IList<SelectiveBay> bays)
        {
            var h = 0.0;
            foreach (var bay in bays)
            {
                if (bay.Height > h) h = bay.Height;
            }

            return h;
        }

        /// <summary>Height of post <paramref name="postIndex"/> WITHIN a given fondo's bays = the tallest of the (up to
        /// two) bays it bounds there; falls back to <paramref name="fallbackHeight"/> when both are absent/empty (e.g. a
        /// column). Lets each fondo of a doble-profundidad rack carry its own cabecera height.</summary>
        public static double PostHeight(IList<SelectiveBay> bays, int postIndex, double fallbackHeight)
        {
            var h = 0.0;
            if (postIndex - 1 >= 0 && postIndex - 1 < bays.Count && bays[postIndex - 1].Height > h) h = bays[postIndex - 1].Height; // bay to the left
            if (postIndex >= 0 && postIndex < bays.Count && bays[postIndex].Height > h) h = bays[postIndex].Height;                // bay to the right
            return h > 0.0 ? h : fallbackHeight;
        }

        /// <summary>
        /// The bay's INICIO_PERFIL X (ménsula overhang from the hook to the profile start); 0 if unset.
        /// Uses the level whose beam GOVERNED the bay length (the widest) — with mixed beam types per bay,
        /// another level's overhang would misplace the posts for the beam that actually spans them.
        /// </summary>
        private static double BeamProfileStartX(RackCatalog catalog, SelectiveBay bay, string view)
        {
            if (bay.Levels.Count == 0)
            {
                return 0.0;
            }

            var level = bay.Levels[0];
            if (!string.IsNullOrEmpty(bay.GoverningBeamId))
            {
                foreach (var candidate in bay.Levels)
                {
                    if (string.Equals(candidate.BeamId, bay.GoverningBeamId, System.StringComparison.OrdinalIgnoreCase))
                    {
                        level = candidate;
                        break;
                    }
                }
            }

            var entry = catalog?.ConnectionLayout.FindConnectionLayout(level.BeamId, SelectiveRackDefaults.BeamProfileStartPoint, view);
            var beamParams = new Dictionary<string, double> { [SelectiveRackDefaults.PeralteParam] = level.BeamPeralte };
            return ResolveX(entry, beamParams);
        }

        /// <summary>Resolve a connection point to (X,Y) for the given block parameters, applying both slopes.</summary>
        public static Point2D Resolve(ConnectionLayoutEntry entry, IReadOnlyDictionary<string, double> parameters)
        {
            return entry == null
                ? new Point2D(0.0, 0.0)
                : new Point2D(ResolveX(entry, parameters), ResolveY(entry, parameters));
        }

        /// <summary>X of a connection point resolved for the given block parameters (X = localX + slope*paramX).</summary>
        private static double ResolveX(ConnectionLayoutEntry entry, IReadOnlyDictionary<string, double> parameters)
        {
            if (entry == null)
            {
                return 0.0;
            }

            var x = entry.LocalX;
            if (entry.LocalXPorParam != 0.0 && !string.IsNullOrWhiteSpace(entry.ParamX)
                && parameters != null && parameters.TryGetValue(entry.ParamX, out var value))
            {
                x += entry.LocalXPorParam * value;
            }

            return x;
        }

        /// <summary>Y of a connection point resolved for the given block parameters (Y = localY + slope*paramY).</summary>
        private static double ResolveY(ConnectionLayoutEntry entry, IReadOnlyDictionary<string, double> parameters)
        {
            if (entry == null)
            {
                return 0.0;
            }

            var y = entry.LocalY;
            if (entry.LocalYPorParam != 0.0 && !string.IsNullOrWhiteSpace(entry.ParamY)
                && parameters != null && parameters.TryGetValue(entry.ParamY, out var value))
            {
                y += entry.LocalYPorParam * value;
            }

            return y;
        }
    }

    /// <summary>Result of <see cref="SelectivePostGeometry.Compute"/>: the N+1 post Xs and each post's larguero troquel X.</summary>
    public sealed class SelectivePostLayout
    {
        public SelectivePostLayout(IReadOnlyList<double> postXs, IReadOnlyList<double> troquelXs)
        {
            PostXs = postXs;
            TroquelXs = troquelXs;
        }

        public IReadOnlyList<double> PostXs { get; }

        /// <summary>The larguero troquel X of each post (N+1), sliding with that post's own peralte.</summary>
        public IReadOnlyList<double> TroquelXs { get; }
    }
}
