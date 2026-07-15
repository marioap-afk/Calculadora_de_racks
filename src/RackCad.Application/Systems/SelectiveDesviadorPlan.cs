using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// Resolves the physical DESVIADOR pieces once for drawing, BOM and UI. One logical grid cell is a resolved
    /// post × load-level; <see cref="SelectiveSafetySelection.Side"/> selects the frontmost aisle face (Left), the
    /// mirrored backmost aisle face (Right), or both. The first load row always sits above the post's first
    /// TROQUEL_LARGUERO, even when the pallet rests on the floor without a beam; every upper row sits 6" below its
    /// beam. Intermediate medio-frente posts are real columns in this grid.
    /// </summary>
    public static class SelectiveDesviadorPlan
    {
        public const double BeamYOffset = 6.0;

        public enum AisleFace
        {
            Front,
            Back
        }

        public sealed class Spot
        {
            public int PostIndex { get; internal set; }
            public int Level { get; internal set; }
            public double RunPostX { get; internal set; }
            public double DepthPostX { get; internal set; }
            public double Y { get; internal set; }
            public double PostPeralte { get; internal set; }
            public AisleFace Face { get; internal set; }
            public bool Mirrored => Face == AisleFace.Back;
            public bool Enabled { get; internal set; }
        }

        public sealed class ClearanceIssue
        {
            public int PostIndex { get; internal set; }
            public int LowerLevel { get; internal set; }
            public int UpperLevel { get; internal set; }
            public double Clear { get; internal set; }
        }

        public sealed class Result
        {
            internal Result(
                IReadOnlyList<Spot> spots,
                IReadOnlyList<int> levelCounts,
                IReadOnlyList<ClearanceIssue> clearanceIssues,
                double longitud,
                double firstLevelHeight)
            {
                Spots = spots;
                LevelCounts = levelCounts;
                ClearanceIssues = clearanceIssues;
                Longitud = longitud;
                FirstLevelHeight = firstLevelHeight;
            }

            public IReadOnlyList<Spot> Spots { get; }
            public IReadOnlyList<int> LevelCounts { get; }
            public IReadOnlyList<ClearanceIssue> ClearanceIssues { get; }
            public double Longitud { get; }
            public double FirstLevelHeight { get; }
            public int PhysicalQuantity => Spots.Count(s => s.Enabled);
        }

        /// <summary>Only even integral inches strictly above 8 are accepted by both configurable dimensions.</summary>
        public static bool IsValidEvenAbove8(double value)
        {
            if (value <= 8.0 || Math.Abs(value - Math.Round(value)) > 1e-6)
            {
                return false;
            }

            return ((long)Math.Round(value)) % 2L == 0L;
        }

        public static Result Build(SelectiveRackSystem system, RackCatalog catalog)
        {
            var selection = SelectiveSafetyFamilies.SelectedOfType(
                system?.SafetySelections,
                catalog?.SafetyElements,
                SelectiveSafetyDefaults.DesviadorType);
            return Build(system, catalog, selection);
        }

        /// <summary>Build with an explicit working selection so the editor can preview dimensions before committing.</summary>
        public static Result Build(SelectiveRackSystem system, RackCatalog catalog, SelectiveSafetySelection selection)
        {
            var longitud = Effective(selection?.DesviadorLongitud, SelectiveSafetyDefaults.DesviadorLongitud);
            var firstHeight = Effective(selection?.DesviadorPrimerNivelAltura, SelectiveSafetyDefaults.DesviadorPrimerNivelAltura);
            if (system == null || catalog == null || selection == null || system.Bays.Count == 0)
            {
                return new Result(Array.Empty<Spot>(), Array.Empty<int>(), Array.Empty<ClearanceIssue>(), longitud, firstHeight);
            }

            var raw = new List<RawPost>();
            var offsets = SelectiveDepthLayout.Offsets(system);
            for (var fondo = 0; fondo < offsets.Count; fondo++)
            {
                if (selection.Side == SafetySide.Left || selection.Side == SafetySide.Both)
                {
                    AddFace(raw, system, catalog, fondo, AisleFace.Front, offsets[fondo], firstHeight);
                }

                if (selection.Side == SafetySide.Right || selection.Side == SafetySide.Both)
                {
                    AddFace(raw, system, catalog, fondo, AisleFace.Back,
                        offsets[fondo] + SelectiveDepthLayout.CabeceraDepthOfFondo(system, fondo), firstHeight);
                }
            }

            // In a corner layout a deeper fondo can stop before the master run. At every run post, retain the
            // frontmost/backmost face that actually REACHES that X instead of assuming fondo 0/last reach everywhere.
            raw = raw.GroupBy(p => (p.Face, X: Round(p.RunPostX)))
                .Select(g => g.Key.Face == AisleFace.Front
                    ? g.OrderBy(p => p.DepthPostX).First()
                    : g.OrderByDescending(p => p.DepthPostX).First())
                .ToList();

            var postXs = raw.Select(p => Round(p.RunPostX)).Distinct().OrderBy(x => x).ToList();
            var postIndex = new Dictionary<double, int>();
            for (var i = 0; i < postXs.Count; i++) postIndex[postXs[i]] = i;

            var off = SelectiveSafetyGrid.OffCellKeys(selection.DesviadorOffCells);
            var spots = new List<Spot>();
            var levelCounts = new int[postXs.Count];
            foreach (var post in raw.OrderBy(p => p.RunPostX).ThenBy(p => p.Face))
            {
                var index = postIndex[Round(post.RunPostX)];
                var ys = new List<double> { post.FirstY };
                ys.AddRange(post.UpperYs.OrderBy(y => y));
                levelCounts[index] = Math.Max(levelCounts[index], ys.Count);
                for (var level = 0; level < ys.Count; level++)
                {
                    spots.Add(new Spot
                    {
                        PostIndex = index,
                        Level = level,
                        RunPostX = post.RunPostX,
                        DepthPostX = post.DepthPostX,
                        Y = ys[level],
                        PostPeralte = post.PostPeralte,
                        Face = post.Face,
                        Enabled = !off.Contains((index, level))
                    });
                }
            }

            var issues = new List<ClearanceIssue>();
            foreach (var group in spots.Where(s => s.Enabled).GroupBy(s => (s.Face, RunX: Round(s.RunPostX))))
            {
                var ordered = group.OrderBy(s => s.Y).ToList();
                for (var i = 1; i < ordered.Count; i++)
                {
                    var clear = ordered[i].Y - ordered[i - 1].Y;
                    if (clear + 1e-6 < longitud)
                    {
                        issues.Add(new ClearanceIssue
                        {
                            PostIndex = ordered[i].PostIndex,
                            LowerLevel = ordered[i - 1].Level,
                            UpperLevel = ordered[i].Level,
                            Clear = clear
                        });
                    }
                }
            }

            return new Result(spots, levelCounts, issues, longitud, firstHeight);
        }

        private static void AddFace(
            ICollection<RawPost> target,
            SelectiveRackSystem system,
            RackCatalog catalog,
            int fondo,
            AisleFace face,
            double depthPostX,
            double firstHeight)
        {
            var bays = SelectiveDepthLayout.BaysOfFondo(system, fondo);
            if (bays == null || bays.Count == 0)
            {
                return;
            }

            var fondoView = SelectiveDepthLayout.FondoSystemView(system, fondo);
            var layout = SelectivePostGeometry.Compute(fondoView, catalog);
            var byX = new Dictionary<double, RawPost>();

            for (var i = 0; i < layout.PostXs.Count; i++)
            {
                var peralte = SelectivePostGeometry.PostPeralteAt(system, i);
                var post = GetPost(byX, layout.PostXs[i], depthPostX, peralte, face, FirstTroquelY(system, catalog, peralte) + firstHeight);
                AddBayRows(post, i > 0 ? bays[i - 1] : null);
                AddBayRows(post, i < bays.Count ? bays[i] : null);
            }

            for (var bayIndex = 0; bayIndex < bays.Count && bayIndex < layout.TroquelXs.Count; bayIndex++)
            {
                var bay = bays[bayIndex];
                var inicioX = SelectivePostGeometry.BeamProfileStartX(catalog, bay, SelectiveRackDefaults.View);
                var tramos = SelectiveMedioFrente.Resolve(bay, layout.TroquelXs[bayIndex], inicioX);
                if (tramos == null) continue;

                var peralte = SelectivePostGeometry.PostPeralteAt(system, bayIndex);
                for (var t = 1; t < tramos.Count; t++)
                {
                    var x = layout.PostXs[bayIndex] + tramos[t].StartOffset;
                    var post = GetPost(byX, x, depthPostX, peralte, face, FirstTroquelY(system, catalog, peralte) + firstHeight);
                    AddBayRows(post, bay);
                }
            }

            foreach (var post in byX.Values)
            {
                if (post.HasLoad) target.Add(post);
            }
        }

        private static RawPost GetPost(
            IDictionary<double, RawPost> posts,
            double runPostX,
            double depthPostX,
            double peralte,
            AisleFace face,
            double firstY)
        {
            var key = Round(runPostX);
            if (!posts.TryGetValue(key, out var post))
            {
                post = new RawPost
                {
                    RunPostX = runPostX,
                    DepthPostX = depthPostX,
                    PostPeralte = peralte,
                    Face = face,
                    FirstY = firstY
                };
                posts[key] = post;
            }

            return post;
        }

        private static void AddBayRows(RawPost post, SelectiveBay bay)
        {
            if (bay == null || (bay.FloorPalletCount <= 0 && bay.Levels.Count == 0))
            {
                return;
            }

            post.HasLoad = true;
            // With a floor pallet, every resolved beam is an upper row. With a floor beam, resolved beam 0 is the
            // first load row and is replaced by the configurable first-troquel position; upper beams start at 1.
            var firstUpper = bay.FloorPalletCount > 0 ? 0 : 1;
            for (var i = firstUpper; i < bay.Levels.Count; i++)
            {
                post.UpperYs.Add(Round(bay.Levels[i].Y - BeamYOffset));
            }
        }

        private static double FirstTroquelY(SelectiveRackSystem system, RackCatalog catalog, double peralte)
        {
            var entry = catalog.ConnectionLayout.FindConnectionLayout(
                system.PostId,
                SelectiveRackDefaults.PostBeamPoint,
                SelectiveRackDefaults.View);
            return SelectivePostGeometry.Resolve(entry, new Dictionary<string, double>
            {
                [SelectiveRackDefaults.PeralteParam] = peralte
            }).Y;
        }

        private static double Effective(double? value, double fallback)
            => value.HasValue && IsValidEvenAbove8(value.Value) ? value.Value : fallback;

        private static double Round(double value) => Math.Round(value, 4);

        private sealed class RawPost
        {
            public double RunPostX;
            public double DepthPostX;
            public double PostPeralte;
            public AisleFace Face;
            public double FirstY;
            public bool HasLoad;
            public HashSet<double> UpperYs { get; } = new HashSet<double>();
        }
    }
}
