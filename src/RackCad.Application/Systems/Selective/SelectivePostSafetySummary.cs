using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Drawing;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.Selective
{
    /// <summary>
    /// What the drawing places at one post of a Selectivo, counted as the BOM counts it (<see cref="SelectivePostSafetySummary"/>). A
    /// post is a frame post of the master run (P1 is 0) or an intermediate post that a medio frente plants inside a front: only the
    /// desviador reaches those (<see cref="SelectiveDesviadorPlan"/> treats them as real columns), so they get their own row and the
    /// column still adds up to the BOM.
    /// </summary>
    public sealed class SelectivePostSafetyRow
    {
        internal SelectivePostSafetyRow(int postIndex, int? intermediateOfFront, int intermediateOrdinal, bool isEndPost)
        {
            PostIndex = postIndex;
            IntermediateOfFront = intermediateOfFront;
            IntermediateOrdinal = intermediateOrdinal;
            IsEndPost = isEndPost;
        }

        /// <summary>The frame post (zero-based); for an intermediate post, the frame post on its left.</summary>
        public int PostIndex { get; }

        /// <summary>The front (zero-based) whose medio frente plants this intermediate post; null for a frame post.</summary>
        public int? IntermediateOfFront { get; }

        /// <summary>The order (from 1) of this intermediate post inside its front; 0 for a frame post.</summary>
        public int IntermediateOrdinal { get; }

        public bool IsIntermediate => IntermediateOfFront.HasValue;

        /// <summary>The first or the last frame post of the master run.</summary>
        public bool IsEndPost { get; }

        /// <summary>Botas the planta places at this post (the BOM counts the botas from the planta).</summary>
        public int Boots { get; internal set; }

        /// <summary>A bota protects the near face (the front of fondo 1: «Izquierda»). False when the post has no bota.</summary>
        public bool BootNear { get; internal set; }

        /// <summary>A bota protects the far face (the back of the last fondo: «Derecha»). False when the post has no bota.</summary>
        public bool BootFar { get; internal set; }

        /// <summary>Protectores laterales the planta places at this post.</summary>
        public int SideGuards { get; internal set; }

        /// <summary>The load levels (zero-based: 0 is N1, the first troquel row) of the desviadores at this post, each once.</summary>
        public IReadOnlyList<int> DeflectorLevels { get; internal set; } = Array.Empty<int>();

        /// <summary>Desviadores at this post (both faces count: a level with a desviador on each face is two pieces).</summary>
        public int Deflectors { get; internal set; }

        /// <summary>Separadores entre fondos the lateral corte of this post stacks (the BOM counts them from the cortes).</summary>
        public int Separators { get; internal set; }
    }

    /// <summary>
    /// «Seguridad por poste» of a Selectivo (I-64 A-14, line D, entrega 2; decisions 6 and 13 of the Owner), read from the SAME projections
    /// the BOM counts (<see cref="SelectiveBomBuilder"/>): the planta for the botas and the protectores laterales
    /// (<see cref="SelectiveBomBuilder.CountingPlanta"/>, where a protector lateral replaces the botas of its post), the desviador plan
    /// for the desviadores (<see cref="SelectiveDesviadorPlan"/>, its enabled spots are its physical quantity) and the lateral cortes for
    /// the separadores entre fondos (<see cref="SelectiveBomBuilder.CountingCortes"/>). So every column adds up to its line of the BOM and
    /// the table says nothing the drawing does not place. A planta piece belongs to the post whose line is the nearest to where it is
    /// inserted, as in <c>DynamicPostSafetySummary</c>.
    /// <para>
    /// The face of a bota is read from the one resolution of faces the planta itself consumes
    /// (<see cref="SelectiveSafetySelection.BootFacesAt"/>, through <see cref="SelectiveSafetyEnds.BootCopiesForPost"/>), and only where the
    /// planta does place botas. Near is the low end of the rack's OWN depth axis (the front post of fondo 1), far the high end: never a
    /// world coordinate, so mirroring the drawing in AutoCAD does not change which face is which.
    /// </para>
    /// </summary>
    public static class SelectivePostSafetySummary
    {
        private const double SamePost = 1e-3;

        public static IReadOnlyList<SelectivePostSafetyRow> Build(SelectiveRackSystem system, RackCatalog catalog)
        {
            if (system == null || catalog == null || system.Bays.Count == 0)
            {
                return Array.Empty<SelectivePostSafetyRow>();
            }

            var postXs = SelectiveDepthLayout.MasterGrid(system, catalog).PostXs;
            if (postXs == null || postXs.Count == 0)
            {
                return Array.Empty<SelectivePostSafetyRow>();
            }

            var frames = Enumerable.Range(0, postXs.Count)
                .Select(post => new SelectivePostSafetyRow(post, null, 0, post == 0 || post == postXs.Count - 1))
                .ToList();

            // Botas and protectores laterales: the planta the BOM counts. The post lines run along Y there.
            foreach (var instance in SelectiveBomBuilder.CountingPlanta(system, catalog))
            {
                if (IsType(instance, catalog, SelectiveSafetyDefaults.BotaType))
                {
                    frames[Nearest(postXs, instance.Insertion.Y)].Boots++;
                }
                else if (IsType(instance, catalog, SelectiveSafetyDefaults.LateralType))
                {
                    frames[Nearest(postXs, instance.Insertion.Y)].SideGuards++;
                }
            }

            var bota = SelectiveSafetyFamilies.SelectedOfType(system.SafetySelections, catalog.SafetyElements, SelectiveSafetyDefaults.BotaType);
            foreach (var row in frames.Where(r => r.Boots > 0 && bota != null))
            {
                var faces = bota.BootFacesAt(row.PostIndex);
                row.BootNear = faces.Near;
                row.BootFar = faces.Far;
            }

            // Separadores entre fondos: the lateral cortes the BOM counts, one corte per frame post.
            foreach (var corte in SelectiveBomBuilder.CountingCortes(system, catalog))
            {
                if (corte.PostIndex < 0 || corte.PostIndex >= frames.Count)
                {
                    continue;
                }

                frames[corte.PostIndex].Separators += corte.Largueros.Count(instance => instance.Role == HeaderBlockRole.Separator);
            }

            // Desviadores: the enabled spots of the plan, which is what the BOM counts. A spot on a frame post goes to its row; a spot on
            // an intermediate post of a medio frente goes to the row of that post.
            var spots = SelectiveDesviadorPlan.Build(system, catalog).Spots.Where(spot => spot.Enabled).ToList();
            var intermediates = new Dictionary<(int Front, double X), List<SelectiveDesviadorPlan.Spot>>();
            var byFrame = new Dictionary<int, List<SelectiveDesviadorPlan.Spot>>();
            foreach (var spot in spots)
            {
                var frame = FramePostAt(postXs, spot.RunPostX);
                if (frame.HasValue)
                {
                    Add(byFrame, frame.Value, spot);
                }
                else
                {
                    Add(intermediates, (FrontAt(postXs, spot.RunPostX), Math.Round(spot.RunPostX, 4)), spot);
                }
            }

            foreach (var pair in byFrame)
            {
                SetDeflectors(frames[pair.Key], pair.Value);
            }

            var rows = new List<SelectivePostSafetyRow>(frames.Count + intermediates.Count);
            for (var post = 0; post < frames.Count; post++)
            {
                rows.Add(frames[post]);
                var ordinal = 0;
                foreach (var pair in intermediates.Where(p => p.Key.Front == post).OrderBy(p => p.Key.X))
                {
                    var row = new SelectivePostSafetyRow(post, post, ++ordinal, false);
                    SetDeflectors(row, pair.Value);
                    rows.Add(row);
                }
            }

            return rows;
        }

        private static void SetDeflectors(SelectivePostSafetyRow row, IReadOnlyCollection<SelectiveDesviadorPlan.Spot> spots)
        {
            row.Deflectors = spots.Count;
            row.DeflectorLevels = spots.Select(spot => spot.Level).Distinct().OrderBy(level => level).ToList();
        }

        private static void Add<TKey>(IDictionary<TKey, List<SelectiveDesviadorPlan.Spot>> target, TKey key, SelectiveDesviadorPlan.Spot spot)
        {
            if (!target.TryGetValue(key, out var list))
            {
                list = new List<SelectiveDesviadorPlan.Spot>();
                target[key] = list;
            }

            list.Add(spot);
        }

        // The frame post at that run X, or null when the X is an intermediate post of a medio frente.
        private static int? FramePostAt(IReadOnlyList<double> postXs, double x)
        {
            var nearest = Nearest(postXs, x);
            return Math.Abs(postXs[nearest] - x) <= SamePost ? nearest : (int?)null;
        }

        // The front an intermediate post stands in: the last frame post on its left.
        private static int FrontAt(IReadOnlyList<double> postXs, double x)
        {
            var front = 0;
            for (var i = 0; i < postXs.Count - 1; i++)
            {
                if (postXs[i] < x) front = i;
            }

            return front;
        }

        private static int Nearest(IReadOnlyList<double> positions, double at)
        {
            var best = 0;
            for (var i = 1; i < positions.Count; i++)
            {
                if (Math.Abs(positions[i] - at) < Math.Abs(positions[best] - at)) best = i;
            }

            return best;
        }

        private static bool IsType(HeaderBlockInstance instance, RackCatalog catalog, string type)
        {
            if (instance == null || instance.Role != HeaderBlockRole.Safety || string.IsNullOrWhiteSpace(instance.PieceId))
            {
                return false;
            }

            var element = catalog.SafetyElements?.FirstOrDefault(entry => string.Equals(entry?.Id, instance.PieceId, StringComparison.OrdinalIgnoreCase));
            return element != null && SelectiveSafetyDefaults.IsType(element.Type, type);
        }
    }
}
