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
            // land correctly and the bay between them absorbs both troquel offsets.
            double TroquelXFor(int postIndex)
                => ResolveX(troquel, new Dictionary<string, double> { [SelectiveRackDefaults.PeralteParam] = PostPeralteAt(system, postIndex) });

            var xs = new List<double> { 0.0 };
            var troquelXs = new List<double> { TroquelXFor(0) };
            for (var b = 0; b < system.Bays.Count; b++)
            {
                var bay = system.Bays[b];
                var inicioX = BeamProfileStartX(catalog, bay, view);
                // post b's RIGHT troquel + post b+1's LEFT troquel, each with its own peralte.
                xs.Add(xs[xs.Count - 1] + bay.BeamLength + (TroquelXFor(b) + inicioX) + (TroquelXFor(b + 1) + inicioX));
                troquelXs.Add(TroquelXFor(b + 1));
            }

            return new SelectivePostLayout(xs, troquelXs);
        }

        /// <summary>The effective PERALTE of post <paramref name="postIndex"/>: its resolved per-post value, else the run peralte.</summary>
        public static double PostPeralteAt(SelectiveRackSystem system, int postIndex)
            => system != null && postIndex >= 0 && postIndex < system.PostPeraltes.Count && system.PostPeraltes[postIndex] > 0.0
                ? system.PostPeraltes[postIndex]
                : system?.PostPeralte ?? 0.0;

        /// <summary>Height of post <paramref name="postIndex"/> = the tallest of the (up to two) bays it bounds.</summary>
        public static double PostHeight(SelectiveRackSystem system, int postIndex)
        {
            var bays = system.Bays;
            var h = 0.0;
            if (postIndex - 1 >= 0 && postIndex - 1 < bays.Count && bays[postIndex - 1].Height > h) h = bays[postIndex - 1].Height; // bay to the left
            if (postIndex >= 0 && postIndex < bays.Count && bays[postIndex].Height > h) h = bays[postIndex].Height;                // bay to the right
            return h > 0.0 ? h : system.Height;
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
