using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>
    /// T6 bom (PP-S10): the «medios frentes» that fitted before a write of the draft, with the tramos they had, and the last valid
    /// recomposition of before it (<see cref="SelectiveDraftEditor.GuardSplits"/>, <see cref="SelectiveDraftEditor.KeepTheSplits"/>).
    /// </summary>
    public sealed class SelectiveSplitGuard
    {
        internal SelectiveSplitGuard(SelectivePalletDesign design, SelectiveRackSystem system,
            IReadOnlyDictionary<(int Fondo, int Front), IReadOnlyList<SelectiveSegment>> fitting)
        {
            Design = design;
            System = system;
            Fitting = fitting;
        }

        internal SelectivePalletDesign Design { get; }

        internal SelectiveRackSystem System { get; }

        internal IReadOnlyDictionary<(int Fondo, int Front), IReadOnlyList<SelectiveSegment>> Fitting { get; }
    }

    /// <summary>
    /// T6 bom (PP-S10, the same defect as dolor 13 by another door): the resolver draws a «medio frente» whose tramos and intermediate posts
    /// no longer fit its front as the WHOLE front, with no notice (<see cref="Systems.Selective.SelectiveMedioFrente"/>). DG-10 refuses such a
    /// split where the tramos are written (<see cref="SetSegments"/>), and the fifth batch (T5 pp) refused the two writes of the peralte of
    /// the rack that broke one. Every other write could still break it: the own peralte of a post (the troquel of the larguero slides with
    /// it), the larguero of a cell (the overhang of its ménsula), the pallet and its count, the tolerance or a manual length (the width of
    /// the front), the post, the fondos and the fronts. This guard is for ALL of them, around the one write path of the editor of the window:
    /// a split that fitted before the write and, with the same tramos, no longer fits after it, refuses the write whole, with the front and
    /// the reason, and the editor goes back to the last valid recomposition of before it. A split that did not fit before (an old drawing,
    /// drawn whole already) is not a reason (PP-S8), nor are new tramos (<see cref="SetSegments"/> checks them itself).
    /// </summary>
    public sealed partial class SelectiveDraftEditor
    {
        /// <summary>How the refusal of <see cref="KeepTheSplits"/> starts, for a caller that adds what it can do about it.</summary>
        public const string BrokenSplitsRefusalStart = "No se aplicó el cambio: ";

        /// <summary>What a write must keep: call it BEFORE the write, with the draft recomposed (its last valid recomposition).</summary>
        public SelectiveSplitGuard GuardSplits()
        {
            var fitting = new Dictionary<(int Fondo, int Front), IReadOnlyList<SelectiveSegment>>();
            if (System != null)
            {
                for (var fondo = 0; fondo < FondoCount; fondo++)
                {
                    for (var front = 0; front < FrontCount(fondo); front++)
                    {
                        var data = Front(fondo, front);
                        if (data == null || !data.IsSplit) continue;
                        if (SplitFit(fondo, front, data.Segments).Fits) fitting[(fondo, front)] = data.Segments;
                    }
                }
            }

            return new SelectiveSplitGuard(Design, System, fitting);
        }

        /// <summary>
        /// After a write that recomposed: true when every split of <paramref name="guard"/> still fits with the same tramos (or is no longer
        /// a split). Otherwise false, with the refusal in <see cref="LastError"/>, and the last valid recomposition is again the one of before
        /// the write, so <see cref="RestoreLastValid"/> puts every input back as it was (the path of a write that cannot be recomposed).
        /// </summary>
        public bool KeepTheSplits(SelectiveSplitGuard guard)
        {
            if (guard == null || guard.Fitting.Count == 0 || System == null) return true;

            var broken = new List<((int Fondo, int Front) Key, SelectiveSegmentFitResult Fit)>();
            foreach (var pair in guard.Fitting.OrderBy(p => p.Key.Fondo).ThenBy(p => p.Key.Front))
            {
                var (fondo, front) = pair.Key;
                if (fondo >= FondoCount || front >= FrontCount(fondo)) continue;
                var data = Front(fondo, front);
                if (data == null || !data.IsSplit || !SameTramos(data.Segments, pair.Value)) continue;
                var fit = SplitFit(fondo, front, data.Segments);
                if (!fit.Fits) broken.Add((pair.Key, fit));
            }

            if (broken.Count == 0) return true;

            Design = guard.Design;
            System = guard.System;
            LastError = BrokenSplitsText(broken.Select(b => FrontName(b.Key.Fondo, b.Key.Front)).ToList(), broken[0].Fit);
            return false;
        }

        /// <summary>«No se aplicó el cambio: el medio frente de F1 dejaría de caber. No cabe: … Ajusta antes sus tramos en «Matriz» (inspector del frente).»</summary>
        private static string BrokenSplitsText(IReadOnlyList<string> names, SelectiveSegmentFitResult first)
            => BrokenSplitsRefusalStart
               + (names.Count == 1
                   ? "el medio frente de " + names[0] + " dejaría de caber. "
                   : "los medios frentes de " + string.Join(", ", names.Take(names.Count - 1)) + " y " + names[names.Count - 1] + " dejarían de caber. En " + names[0] + ": ")
               + SelectiveSegmentFit.RefusalOf(first, severalFondos: false)
               + " Ajusta antes sus tramos en «Matriz» (inspector del frente).";

        private SelectiveSegmentFitResult SplitFit(int fondo, int front, IReadOnlyList<SelectiveSegment> segments)
            => SelectiveSegmentFit.Of(System, Catalog, new[] { fondo }, front, segments.Take(segments.Count - 1).Select(s => s.Length).ToList(), segments.Count);

        private static bool SameTramos(IReadOnlyList<SelectiveSegment> now, IReadOnlyList<SelectiveSegment> before)
            => now.Count == before.Count
               && now.Zip(before, (a, b) => Math.Abs(a.Length - b.Length) <= 1e-9 && a.Loaded == b.Loaded).All(same => same);
    }
}
