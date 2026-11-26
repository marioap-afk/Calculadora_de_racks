using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.Dynamic;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.VentanaRackCad
{
    /// <summary>
    /// One row of the editable «Seguridad por poste» of the RackCad window (I-64, cuarta tanda, part «seg»; SG-01, SG-05 and SG-08 of the
    /// Owner): what the drawing places at the post, counted as the BOM counts it, and what the table needs to edit it: the post of the bota,
    /// the protector lateral and the defense (null for an intermediate post of a medio frente, which only carries desviadores, SG-08), and the
    /// column of the desviador grid the drawing reads for it with its levels.
    /// </summary>
    public sealed record RackCadSafetyRow
    {
        /// <summary>«P1»…, or «F2 · intermedio 1» for an intermediate post of a medio frente (C2-04).</summary>
        public string Post { get; init; } = string.Empty;

        /// <summary>«extremo», «intermedio» or «de medio frente».</summary>
        public string Kind { get; init; } = string.Empty;

        /// <summary>The post (zero-based) of the bota, the protector lateral and the defense; null for an intermediate post.</summary>
        public int? FramePost { get; init; }

        public bool IsEndPost { get; init; }

        public int Boots { get; init; }

        /// <summary>The bota of the near face (Selectivo «Izquierda», Dinámico «Salida»).</summary>
        public bool BootNear { get; init; }

        /// <summary>The bota of the far face (Selectivo «Derecha», Dinámico «Entrada»).</summary>
        public bool BootFar { get; init; }

        public int SideGuards { get; init; }

        /// <summary>The column of the desviador grid the drawing reads at this post; null when the post has no load level.</summary>
        public int? DeflectorKey { get; init; }

        /// <summary>
        /// The column an edit of this post writes: the same as <see cref="DeflectorKey"/>, except in a Dinámico still read the historic
        /// way, whose edit first passes the desviador to the reading by post (<see cref="RackCadSafetyEdit.DesviadorToPosts"/>) and then
        /// writes the post itself.
        /// </summary>
        public int? DeflectorWriteKey { get; init; }

        /// <summary>The load levels of the desviador at this post (N1 = the first troquel row, as «Matriz»).</summary>
        public int DeflectorLevels { get; init; }

        /// <summary>The pieces of desviador the BOM counts at this post (both faces count).</summary>
        public int Deflectors { get; init; }

        public int Separators { get; init; }

        /// <summary>Dinámico: a defense drawn at the exit / the entrance of this post (the planta, as the BOM counts it).</summary>
        public bool DefenseExit { get; init; }

        public bool DefenseEntrance { get; init; }

        public int Guides { get; init; }
    }

    /// <summary>
    /// The rows of the editable table of a Selectivo, on a resolved system of its own (the planta and the cuts it reads impose on the custom
    /// cabeceras, ARQ-43-10): the frame posts of <see cref="SelectivePostSafetySummary"/> (botas, protector lateral, separadores, desviadores,
    /// counted as the BOM) and EVERY post of the desviador plan, also an intermediate post whose levels are all off (the summary only lists
    /// the posts with a piece; the table must let the user turn them on again).
    /// </summary>
    public static class SelectiveSafetyRows
    {
        private const double SamePost = 1e-3;

        /// <param name="summary">The summary already counted on <paramref name="system"/> (null: it is counted here).</param>
        public static IReadOnlyList<RackCadSafetyRow> Build(SelectiveRackSystem system, RackCatalog catalog, IReadOnlyList<SelectivePostSafetyRow> summary = null)
        {
            summary ??= SelectivePostSafetySummary.Build(system, catalog);
            if (summary.Count == 0) return Array.Empty<RackCadSafetyRow>();

            var postXs = SelectiveDepthLayout.MasterGrid(system, catalog).PostXs;
            var plan = SelectiveDesviadorPlan.Build(system, catalog);
            var columns = plan.Spots.GroupBy(s => s.PostIndex)
                .Select(g => (Key: g.Key, X: g.First().RunPostX, Levels: g.Key < plan.LevelCounts.Count ? plan.LevelCounts[g.Key] : 0,
                    Pieces: g.Count(s => s.Enabled)))
                .OrderBy(c => c.X)
                .ToList();

            var rows = new List<RackCadSafetyRow>();
            foreach (var frame in summary.Where(r => !r.IsIntermediate))
            {
                var x = frame.PostIndex < postXs.Count ? postXs[frame.PostIndex] : double.NaN;
                var column = columns.FirstOrDefault(c => Math.Abs(c.X - x) <= SamePost);
                var has = columns.Any(c => Math.Abs(c.X - x) <= SamePost);
                rows.Add(new RackCadSafetyRow
                {
                    Post = "P" + (frame.PostIndex + 1),
                    Kind = frame.IsEndPost ? "extremo" : "intermedio",
                    FramePost = frame.PostIndex,
                    IsEndPost = frame.IsEndPost,
                    Boots = frame.Boots,
                    BootNear = frame.BootNear,
                    BootFar = frame.BootFar,
                    SideGuards = frame.SideGuards,
                    Separators = frame.Separators,
                    DeflectorKey = has ? column.Key : (int?)null,
                    DeflectorWriteKey = has ? column.Key : (int?)null,
                    DeflectorLevels = has ? column.Levels : 0,
                    Deflectors = frame.Deflectors,
                });

                // The intermediate posts of the medio frente of the front that starts at this post, in their order (C2-04).
                var ordinal = 0;
                var next = frame.PostIndex + 1 < postXs.Count ? postXs[frame.PostIndex + 1] : double.PositiveInfinity;
                foreach (var inner in columns.Where(c => c.X > x + SamePost && c.X < next - SamePost && !postXs.Any(px => Math.Abs(px - c.X) <= SamePost)))
                {
                    ordinal++;
                    rows.Add(new RackCadSafetyRow
                    {
                        Post = "F" + (frame.PostIndex + 1) + " · intermedio " + ordinal,
                        Kind = "de medio frente",
                        FramePost = null,
                        DeflectorKey = inner.Key,
                        DeflectorWriteKey = inner.Key,
                        DeflectorLevels = inner.Levels,
                        Deflectors = inner.Pieces,
                    });
                }
            }

            return rows;
        }
    }

    /// <summary>
    /// The rows of the editable table of a Dinámico: <see cref="DynamicPostSafetySummary"/> (counted as the BOM), with the column of the
    /// desviador grid the drawing reads at each post (<see cref="SelectiveDesviadorPlan.CellKey"/>), the column an edit of the post writes
    /// (the post itself: an edit passes a desviador still read the historic way to the reading by post, decision of the Owner of
    /// 2026-11-26), and the two ends of its defense as the planta draws them.
    /// </summary>
    public static class DynamicSafetyRows
    {
        /// <param name="summary">The summary already counted on <paramref name="system"/> (null: it is counted here).</param>
        public static IReadOnlyList<RackCadSafetyRow> Build(DynamicRackSystem system, RackCatalog catalog, IReadOnlyList<DynamicPostSafetyRow> summary = null)
        {
            summary ??= DynamicPostSafetySummary.Build(system, catalog);
            if (summary.Count == 0) return Array.Empty<RackCadSafetyRow>();

            var desviador = SelectiveSafetyFamilies.SelectedOfType(system.SafetySelections, catalog.SafetyElements, SelectiveSafetyDefaults.DesviadorType);
            var defensa = SelectiveSafetyFamilies.SelectedOfType(system.SafetySelections, catalog.SafetyElements, SelectiveSafetyDefaults.DefensaType);
            var posts = summary.Count;
            var keys = Enumerable.Range(0, posts)
                .Select(p => DynamicFrontActivation.BoundaryExists(system, p) ? SelectiveDesviadorPlan.CellKey(desviador, p, system.Fronts.Count) : (int?)null)
                .ToList();

            var rows = new List<RackCadSafetyRow>();
            foreach (var row in summary)
            {
                var post = row.PostIndex;
                var exists = DynamicFrontActivation.BoundaryExists(system, post);
                var setting = DynamicForkliftDefensePlan.ForSelection(defensa, post, posts);
                var key = keys[post];
                rows.Add(new RackCadSafetyRow
                {
                    Post = "P" + (post + 1),
                    Kind = post == 0 || post == posts - 1 ? "extremo" : "intermedio",
                    FramePost = post,
                    IsEndPost = post == 0 || post == posts - 1,
                    Boots = row.Boots,
                    SideGuards = row.SideGuards,
                    DeflectorKey = key,
                    DeflectorWriteKey = key.HasValue ? post : (int?)null,
                    DeflectorLevels = exists ? Math.Min(DynamicFrontGeometry.LoadLevelsAtPost(system, post), system.LoadBeamLevels.Count) : 0,
                    Deflectors = row.DeflectorsAtExit + row.DeflectorsAtEntrance,
                    DefenseExit = defensa != null && exists && setting.DrawsExit && DynamicDefenseFaces.HasFace(system, post, farEnd: false),
                    DefenseEntrance = defensa != null && exists && setting.DrawsEntrance && DynamicDefenseFaces.HasFace(system, post, farEnd: true),
                    Guides = row.EntranceGuides,
                });
            }

            return rows;
        }
    }
}
