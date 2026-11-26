using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>
    /// Whether the tramos of a «medio frente» fit their front, in one fondo, as the resolver draws them (DG-10 of the prototype «dialogos»).
    /// <see cref="Width"/> is the larguero length of the whole front; <see cref="PostOverhead"/> what each intermediate post takes of it;
    /// <see cref="Remainder"/> what is left for the last tramo, the calculated one (≤ 0 = it does not fit). Not known (<see cref="IsKnown"/>
    /// false) only when there is no system yet or no fondo of the scope has the front: then nothing is checked, as the classic dialog does
    /// with 0. A front that exists with no width (≤ 0) IS known and does not fit: the resolver would draw it whole (correction 3 of the
    /// review).
    /// </summary>
    public sealed record SelectiveSegmentFitResult(int Fondo, double Width, double PostOverhead, int IntermediatePosts, double Specified, double Remainder)
    {
        /// <summary>A fondo of the scope has the front in the system (its width may still be 0).</summary>
        public bool IsKnown => Fondo >= 0;

        /// <summary>What every intermediate post takes, together.</summary>
        public double PostsTotal => IntermediatePosts * PostOverhead;

        /// <summary>The front has a width to be split (the strip and the «(resto)» can be drawn).</summary>
        public bool HasWidth => IsKnown && Width > 0.0;

        /// <summary>The last tramo is longer than 0 on a front with width: the resolver draws the split, and does not fall back to the whole front.</summary>
        public bool Fits => !IsKnown || IntermediatePosts < 1 || (Width > 0.0 && Remainder > 0.0);
    }

    /// <summary>
    /// What the intermediate posts of a «medio frente» take of the front, and whether a split fits (DG-10, accepted by the Owner on
    /// 2026-11-26: «el «(resto)» descuenta lo que ocupa cada poste intermedio, y el inspector no deja aplicar un reparto que no cabe»).
    /// <para>
    /// The data was [PENDIENTE] in the prototype; it exists in the catalog and the resolver already uses it (<see cref="SelectiveMedioFrente"/>):
    /// a post spans larguero + 2·(troquelX + inicioPerfilX), so each intermediate post takes <c>2·(troquelX + inicioX)</c> of the bay, with
    /// troquelX the larguero troquel of the post (it slides with the peralte of that post) and inicioX the overhang of the ménsula of the
    /// larguero of the bay (INICIO_PERFIL X). The last tramo is <c>BeamLength − Σ tramos − (N − 1)·that</c>, and when it is ≤ 0 the resolver
    /// draws the whole front with no notice (dolor 13 of the inventory). The overhead is read with the same calls the BOM and the drawing make
    /// (<see cref="SelectiveDepthLayout.MasterGrid"/> and <see cref="SelectivePostGeometry.BeamProfileStartX"/>), and whether a split fits is
    /// decided by <see cref="SelectiveMedioFrente.Resolve"/> itself, so the window invents no rule of geometry.
    /// </para>
    /// </summary>
    public static class SelectiveSegmentFit
    {
        /// <summary>What one intermediate post of <paramref name="front"/> takes of the front in <paramref name="fondo"/>; 0 when it is not known.</summary>
        public static double PostOverhead(SelectiveRackSystem system, RackCatalog catalog, int fondo, int front)
        {
            var bay = BayOf(system, fondo, front);
            return bay == null ? 0.0 : 2.0 * (TroquelXOf(system, catalog, front) + InicioXOf(catalog, bay));
        }

        /// <summary>
        /// The split of <paramref name="front"/> into <paramref name="count"/> tramos, the first ones with <paramref name="specified"/> lengths
        /// (the last one is calculated), in every fondo of <paramref name="fondos"/> that has the front: the tightest of them (the least left
        /// for the last tramo, the first fondo when they tie). With no system, or no fondo with that front, it is not known.
        /// </summary>
        public static SelectiveSegmentFitResult Of(SelectiveRackSystem system, RackCatalog catalog, IEnumerable<int> fondos, int front,
            IReadOnlyList<double> specified, int count)
        {
            var lengths = (specified ?? Array.Empty<double>()).ToList();
            var posts = Math.Max(0, count - 1);
            var sum = lengths.Where(l => l > 0.0 && !double.IsInfinity(l)).Sum();
            SelectiveSegmentFitResult tightest = null;
            var troquelX = system == null ? 0.0 : TroquelXOf(system, catalog, front);
            foreach (var fondo in fondos ?? Enumerable.Empty<int>())
            {
                var bay = BayOf(system, fondo, front);
                if (bay == null) continue;
                var inicioX = InicioXOf(catalog, bay);
                var overhead = 2.0 * (troquelX + inicioX);
                // A front with no width (≤ 0) has nothing to split: it is the tightest of all (the resolver draws it whole).
                var remainder = bay.BeamLength > 0.0 ? Remainder(bay, troquelX, inicioX, overhead, lengths, count) : Math.Min(0.0, bay.BeamLength);
                var fit = new SelectiveSegmentFitResult(fondo, bay.BeamLength, overhead, posts, sum, remainder);
                if (tightest == null || !fit.Fits && tightest.Fits || fit.Fits == tightest.Fits && fit.Remainder < tightest.Remainder) tightest = fit;
            }

            return tightest ?? new SelectiveSegmentFitResult(-1, 0.0, 0.0, posts, sum, 0.0);
        }

        /// <summary>
        /// Why the split cannot be applied, or null when it fits (or it is not known). <paramref name="severalFondos"/>: the rack has more
        /// than one fondo, so the text names the one where it does not fit.
        /// </summary>
        public static string RefusalOf(SelectiveSegmentFitResult fit, bool severalFondos)
        {
            if (fit == null || fit.Fits) return null;
            var where = severalFondos && fit.Fondo >= 0 ? " en el fondo " + (fit.Fondo + 1).ToString(CultureInfo.InvariantCulture) : string.Empty;
            if (!(fit.Width > 0.0))
                return "No cabe" + where + ": el frente no tiene ancho (≈ " + Num(Math.Max(0.0, fit.Width)) + " in), así que no se puede partir en tramos.";
            return "No cabe" + where + ": los tramos suman " + Num(fit.Specified) + " in y "
                   + (fit.IntermediatePosts == 1 ? "el poste intermedio ocupa" : "los " + fit.IntermediatePosts.ToString(CultureInfo.InvariantCulture) + " postes intermedios ocupan")
                   + " ≈ " + Num(fit.PostsTotal) + " in; el frente mide ≈ " + Num(fit.Width) + " in y no queda resto para el último tramo.";
        }

        // The last tramo as the resolver computes it. With every length written (> 0), the resolver itself decides: when it draws the split,
        // the remainder is the length it gives to the last tramo; when it does not, its same expression, ≤ 0, says how much is missing. While a
        // length is not written yet (blank or not a number) it counts 0, as the live estimate of the classic dialog did.
        private static double Remainder(SelectiveBay bay, double troquelX, double inicioX, double overhead, IReadOnlyList<double> specified, int count)
        {
            if (count < 2) return bay.BeamLength;
            var lengths = Enumerable.Range(0, count - 1).Select(i => i < specified.Count ? specified[i] : 0.0).ToList();
            var written = lengths.Where(l => l > 0.0 && !double.IsInfinity(l)).Sum();
            var expression = bay.BeamLength - written - (count - 1) * overhead;
            if (lengths.Any(l => !(l > 0.0) || double.IsInfinity(l))) return expression;

            var probe = new SelectiveBay { BeamLength = bay.BeamLength };
            foreach (var length in lengths) probe.Segments.Add(new SelectiveSegment { Length = length });
            probe.Segments.Add(new SelectiveSegment { Length = 0.0 });
            var resolved = SelectiveMedioFrente.Resolve(probe, troquelX, inicioX);
            return resolved != null ? resolved[resolved.Count - 1].Length : Math.Min(0.0, expression);
        }

        // The larguero troquel X of the left post of the front, as the BOM and the plan read it (the master grid; it slides with the peralte
        // of that post), and the overhang of the ménsula of the larguero of the bay, as the frontal and the BOM read it.
        private static double TroquelXOf(SelectiveRackSystem system, RackCatalog catalog, int front)
        {
            var troquelXs = SelectiveDepthLayout.MasterGrid(system, catalog).TroquelXs;
            return front >= 0 && front < troquelXs.Count ? troquelXs[front] : 0.0;
        }

        private static double InicioXOf(RackCatalog catalog, SelectiveBay bay)
            => SelectivePostGeometry.BeamProfileStartX(catalog, bay, SelectiveRackDefaults.View);

        private static SelectiveBay BayOf(SelectiveRackSystem system, int fondo, int front)
        {
            if (system == null || fondo < 0 || front < 0 || fondo >= SelectiveDepthLayout.Count(system)) return null;
            var bays = SelectiveDepthLayout.BaysOfFondo(system, fondo);
            return bays != null && front < bays.Count ? bays[front] : null;
        }

        private static string Num(double value) => value.ToString("0.###", CultureInfo.InvariantCulture);
    }
}
