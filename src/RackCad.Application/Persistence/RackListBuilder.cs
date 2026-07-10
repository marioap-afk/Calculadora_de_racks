using System;
using System.Collections.Generic;
using System.Linq;

namespace RackCad.Application.Persistence
{
    /// <summary>One rack in the drawing-wide listing: its identity plus display-ready labels.</summary>
    public sealed class RackListEntry
    {
        public RackListEntry(string id, string name, string kind, string kindLabel, string viewsLabel, int viewCount)
        {
            Id = id;
            Name = name;
            Kind = kind;
            KindLabel = kindLabel;
            ViewsLabel = viewsLabel;
            ViewCount = viewCount;
        }

        /// <summary>Stable rack identity (GUID from the embed envelope).</summary>
        public string Id { get; }

        /// <summary>Client-facing name; "(sin nombre)" when no view-block carries one.</summary>
        public string Name { get; }

        /// <summary>Raw kind from the envelope (<c>selective</c> / <c>dynamic</c> / <c>cabecera</c> / <c>cama</c>).</summary>
        public string Kind { get; }

        /// <summary>Spanish display label for <see cref="Kind"/>.</summary>
        public string KindLabel { get; }

        /// <summary>Views present, e.g. "frontal, lateral ×3, planta" (selective laterals counted by distinct Section).</summary>
        public string ViewsLabel { get; }

        /// <summary>Total number of view-blocks behind <see cref="ViewsLabel"/>.</summary>
        public int ViewCount { get; }
    }

    /// <summary>
    /// Folds the embed envelopes scanned from a drawing's block definitions into one row per rack
    /// (grouped by GUID), with display-ready labels for the RACKLISTA window. Pure and unit-testable;
    /// no AutoCAD, no WPF — the plugin supplies the envelopes and the copy counts.
    /// </summary>
    public static class RackListBuilder
    {
        public static IReadOnlyList<RackListEntry> Build(IEnumerable<RackEmbedDocument> embeds)
        {
            var entries = new List<RackListEntry>();
            if (embeds == null)
            {
                return entries;
            }

            // A rack = every view-block sharing the same GUID; envelopes without identity or kind are not racks.
            var groups = embeds
                .Where(embed => embed != null && !string.IsNullOrWhiteSpace(embed.Id) && !string.IsNullOrWhiteSpace(embed.Kind))
                .GroupBy(embed => embed.Id, StringComparer.OrdinalIgnoreCase);

            foreach (var group in groups)
            {
                var documents = group.ToList();
                var name = documents.Select(document => document.Name).FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));
                var kind = documents.Select(document => document.Kind).First(value => !string.IsNullOrWhiteSpace(value));
                var (viewsLabel, viewCount) = DescribeViews(documents);

                entries.Add(new RackListEntry(
                    group.Key,
                    string.IsNullOrWhiteSpace(name) ? "(sin nombre)" : name.Trim(),
                    kind,
                    KindLabel(kind),
                    viewsLabel,
                    viewCount));
            }

            return entries
                .OrderBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
                .ThenBy(entry => entry.Id, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        /// <summary>Spanish display label for an envelope kind (the raw kind when unknown).</summary>
        public static string KindLabel(string kind)
        {
            switch (kind?.Trim().ToLowerInvariant())
            {
                case RackEmbedDocument.KindSelective: return "Selectivo";
                case RackEmbedDocument.KindDynamic: return "Sistema dinámico";
                case RackEmbedDocument.KindCabecera: return "Cabecera";
                case RackEmbedDocument.KindCama: return "Cama de rodamiento";
                default: return kind ?? string.Empty;
            }
        }

        /// <summary>
        /// "frontal, lateral ×3, planta" in canonical view order. A null/empty view is the single lateral of a
        /// legacy dynamic/cama block; the selective's lateral cuts (one block per Section) collapse into one
        /// "lateral ×N" counted by distinct Section indices.
        /// </summary>
        private static (string Label, int Count) DescribeViews(IReadOnlyList<RackEmbedDocument> documents)
        {
            var viewGroups = documents
                .GroupBy(document => NormalizeView(document.View), StringComparer.OrdinalIgnoreCase)
                .Select(views => (View: views.Key, Count: views.Select(document => document.Section).Distinct().Count()))
                .OrderBy(views => ViewOrder(views.View))
                .ThenBy(views => views.View, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var label = string.Join(", ", viewGroups.Select(views => views.Count > 1 ? views.View + " ×" + views.Count : views.View));
            return (label, viewGroups.Sum(views => views.Count));
        }

        private static string NormalizeView(string view) =>
            string.IsNullOrWhiteSpace(view) ? RackEmbedDocument.ViewLateral : view.Trim().ToLowerInvariant();

        private static int ViewOrder(string view)
        {
            switch (view)
            {
                case RackEmbedDocument.ViewFrontal: return 0;
                case RackEmbedDocument.ViewLateral: return 1;
                case RackEmbedDocument.ViewPlanta: return 2;
                default: return 3;
            }
        }
    }
}
