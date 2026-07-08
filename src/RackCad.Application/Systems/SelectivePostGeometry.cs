using System.Collections.Generic;
using RackCad.Application.Catalogs;
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
            var postParams = new Dictionary<string, double> { [SelectiveRackDefaults.PeralteParam] = system.PostPeralte };
            var troquel = catalog?.ConnectionLayout.FindConnectionLayout(system.PostId, SelectiveRackDefaults.PostBeamPoint, view);
            var troquelX = ResolveX(troquel, postParams);

            var xs = new List<double> { 0.0 };
            foreach (var bay in system.Bays)
            {
                var inicioX = BeamProfileStartX(catalog, bay, view);
                xs.Add(xs[xs.Count - 1] + bay.BeamLength + 2.0 * (troquelX + inicioX));
            }

            return new SelectivePostLayout(xs, troquelX);
        }

        /// <summary>Height of post <paramref name="postIndex"/> = the tallest of the (up to two) bays it bounds.</summary>
        public static double PostHeight(SelectiveRackSystem system, int postIndex)
        {
            var bays = system.Bays;
            var h = 0.0;
            if (postIndex - 1 >= 0 && postIndex - 1 < bays.Count && bays[postIndex - 1].Height > h) h = bays[postIndex - 1].Height; // bay to the left
            if (postIndex >= 0 && postIndex < bays.Count && bays[postIndex].Height > h) h = bays[postIndex].Height;                // bay to the right
            return h > 0.0 ? h : system.Height;
        }

        /// <summary>The bay's INICIO_PERFIL X (ménsula overhang from the hook to the profile start); 0 if unset.</summary>
        private static double BeamProfileStartX(RackCatalog catalog, SelectiveBay bay, string view)
        {
            if (bay.Levels.Count == 0)
            {
                return 0.0;
            }

            var level = bay.Levels[0];
            var entry = catalog?.ConnectionLayout.FindConnectionLayout(level.BeamId, SelectiveRackDefaults.BeamProfileStartPoint, view);
            var beamParams = new Dictionary<string, double> { [SelectiveRackDefaults.PeralteParam] = level.BeamPeralte };
            return ResolveX(entry, beamParams);
        }

        /// <summary>X of a connection point resolved for the given block parameters (X = localX + slope*param).</summary>
        private static double ResolveX(ConnectionLayoutEntry entry, IReadOnlyDictionary<string, double> parameters)
        {
            if (entry == null)
            {
                return 0.0;
            }

            var x = entry.LocalX;
            if (entry.LocalXPorParam != 0.0 && !string.IsNullOrWhiteSpace(entry.Param)
                && parameters != null && parameters.TryGetValue(entry.Param, out var value))
            {
                x += entry.LocalXPorParam * value;
            }

            return x;
        }
    }

    /// <summary>Result of <see cref="SelectivePostGeometry.Compute"/>: the N+1 post Xs and the post's troquel X.</summary>
    public sealed class SelectivePostLayout
    {
        public SelectivePostLayout(IReadOnlyList<double> postXs, double troquelX)
        {
            PostXs = postXs;
            TroquelX = troquelX;
        }

        public IReadOnlyList<double> PostXs { get; }
        public double TroquelX { get; }
    }
}
