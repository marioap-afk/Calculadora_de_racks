using System;
using System.Collections.Generic;
using System.Linq;

namespace RackCad.Application.StructuralSections
{
    /// <summary>
    /// The loaded neutral catalog. Immutable once built: consumers may query it freely and a shared instance
    /// is safe to hand around (the provider caches one per directory).
    ///
    /// Two invariants are enforced at CONSTRUCTION, so no lookup can ever be ambiguous by accident:
    /// the section id is unique, and the normalized EDI designation is unique. Anything that would break them
    /// throws here rather than resolving to "the first row", which is the failure mode the tolerant catalogs
    /// still have and I-19 has to report.
    ///
    /// The display-name index is the one place ambiguity is TOLERATED but never resolved: if two sections
    /// could answer to the same typed text, the lookup reports ambiguity instead of picking one.
    /// </summary>
    public sealed class StructuralSectionCatalog
    {
        private static readonly StructuralSectionDefinition[] NoSections = new StructuralSectionDefinition[0];

        private readonly Dictionary<StructuralSectionId, StructuralSectionDefinition> _byId;
        private readonly Dictionary<string, StructuralSectionDefinition> _byNormalizedEdi;
        private readonly Dictionary<string, List<StructuralSectionDefinition>> _byNormalizedDesignation;
        private readonly Dictionary<StructuralSectionFamily, StructuralSectionDefinition[]> _byFamily;
        private readonly Dictionary<string, StructuralSectionSource> _sourcesById;

        private StructuralSectionCatalog(
            StructuralSectionDefinition[] all,
            StructuralSectionSource[] sources,
            Dictionary<StructuralSectionId, StructuralSectionDefinition> byId,
            Dictionary<string, StructuralSectionDefinition> byNormalizedEdi,
            Dictionary<string, List<StructuralSectionDefinition>> byNormalizedDesignation,
            Dictionary<StructuralSectionFamily, StructuralSectionDefinition[]> byFamily,
            Dictionary<string, StructuralSectionSource> sourcesById)
        {
            All = all;
            Sources = sources;
            _byId = byId;
            _byNormalizedEdi = byNormalizedEdi;
            _byNormalizedDesignation = byNormalizedDesignation;
            _byFamily = byFamily;
            _sourcesById = sourcesById;
        }

        /// <summary>Every section, enabled or not, ordered by id (ordinal).</summary>
        public IReadOnlyList<StructuralSectionDefinition> All { get; }

        public IReadOnlyList<StructuralSectionSource> Sources { get; }

        public int Count => All.Count;

        public static StructuralSectionCatalog Empty { get; } =
            Create(NoSections, new StructuralSectionSource[0]);

        public static StructuralSectionCatalog Create(
            IEnumerable<StructuralSectionDefinition> sections,
            IEnumerable<StructuralSectionSource> sources)
        {
            if (sections == null)
            {
                throw new ArgumentNullException(nameof(sections));
            }

            var ordered = sections
                .Where(section => section != null)
                .OrderBy(section => section.SectionId.Value, StringComparer.Ordinal)
                .ToArray();

            var byId = new Dictionary<StructuralSectionId, StructuralSectionDefinition>();
            var byEdi = new Dictionary<string, StructuralSectionDefinition>(StringComparer.Ordinal);
            var byDesignation = new Dictionary<string, List<StructuralSectionDefinition>>(StringComparer.Ordinal);

            foreach (var section in ordered)
            {
                if (section.Identity == null)
                {
                    throw new ArgumentException("Una seccion del catalogo no tiene identidad.", nameof(sections));
                }

                if (section.SectionId.IsEmpty)
                {
                    throw new ArgumentException(
                        "Una seccion del catalogo ('" + section.Identity.ManualLabel + "') tiene el id vacio.",
                        nameof(sections));
                }

                if (byId.ContainsKey(section.SectionId))
                {
                    throw new ArgumentException(
                        "Id de seccion duplicado: '" + section.SectionId + "'.", nameof(sections));
                }

                byId.Add(section.SectionId, section);

                var edi = section.Identity.NormalizedEdiDesignation;

                if (byEdi.ContainsKey(edi))
                {
                    throw new ArgumentException(
                        "Designacion EDI duplicada tras normalizar: '" + edi + "' (ids '" +
                        byEdi[edi].SectionId + "' y '" + section.SectionId + "').",
                        nameof(sections));
                }

                byEdi.Add(edi, section);

                Index(byDesignation, edi, section);
                Index(byDesignation, section.Identity.NormalizedManualLabel, section);
            }

            var byFamily = StructuralSectionFamilies.All.ToDictionary(
                family => family,
                family => ordered.Where(section => section.Family == family).ToArray());

            var sourceList = (sources ?? Enumerable.Empty<StructuralSectionSource>())
                .Where(source => source != null)
                .OrderBy(source => source.SourceId, StringComparer.Ordinal)
                .ToArray();

            var sourcesById = new Dictionary<string, StructuralSectionSource>(StringComparer.Ordinal);

            foreach (var source in sourceList)
            {
                if (sourcesById.ContainsKey(source.SourceId))
                {
                    throw new ArgumentException(
                        "Fuente duplicada: '" + source.SourceId + "'.", nameof(sources));
                }

                sourcesById.Add(source.SourceId, source);
            }

            return new StructuralSectionCatalog(
                ordered, sourceList, byId, byEdi, byDesignation, byFamily, sourcesById);
        }

        private static void Index(
            Dictionary<string, List<StructuralSectionDefinition>> index,
            string key,
            StructuralSectionDefinition section)
        {
            if (!index.TryGetValue(key, out var bucket))
            {
                bucket = new List<StructuralSectionDefinition>(1);
                index.Add(key, bucket);
            }

            if (!bucket.Contains(section))
            {
                bucket.Add(section);
            }
        }

        /// <summary>
        /// Resolves by id INCLUDING disabled sections. This is the round-trip path: a design saved months ago
        /// must keep loading even if its section was later withdrawn from new selections (owner decision 15).
        /// </summary>
        public bool TryGetById(StructuralSectionId id, out StructuralSectionDefinition section) =>
            _byId.TryGetValue(id, out section);

        /// <summary>Same as <see cref="TryGetById(StructuralSectionId, out StructuralSectionDefinition)"/>, from text.</summary>
        public bool TryGetById(string id, out StructuralSectionDefinition section)
        {
            section = null;
            return StructuralSectionId.TryParse(id, out var parsed) && TryGetById(parsed, out section);
        }

        public StructuralSectionDefinition GetById(StructuralSectionId id)
        {
            if (!TryGetById(id, out var section))
            {
                throw new KeyNotFoundException("No existe la seccion estructural con id '" + id + "'.");
            }

            return section;
        }

        /// <summary>
        /// Resolves by the EDI designation, normalized. Unique by construction, so this never has to choose.
        /// Disabled sections resolve here too — this is identity resolution, not selection.
        /// </summary>
        public bool TryGetByEdiDesignation(string ediDesignation, out StructuralSectionDefinition section)
        {
            section = null;

            return StructuralSectionDesignationNormalizer.TryNormalize(ediDesignation, out var normalized)
                && _byNormalizedEdi.TryGetValue(normalized, out section);
        }

        /// <summary>
        /// Resolves by anything a user might type — the manual label or the EDI form. Returns <c>false</c>
        /// when the text matches MORE THAN ONE section: an ambiguous designation is rejected, never guessed.
        /// </summary>
        public bool TryGetByDesignation(string designation, out StructuralSectionDefinition section)
        {
            section = null;

            if (!StructuralSectionDesignationNormalizer.TryNormalize(designation, out var normalized))
            {
                return false;
            }

            if (!_byNormalizedDesignation.TryGetValue(normalized, out var matches) || matches.Count != 1)
            {
                return false;
            }

            section = matches[0];
            return true;
        }

        /// <summary>Every section that answers to a designation; more than one means ambiguous.</summary>
        public IReadOnlyList<StructuralSectionDefinition> FindByDesignation(string designation)
        {
            if (!StructuralSectionDesignationNormalizer.TryNormalize(designation, out var normalized) ||
                !_byNormalizedDesignation.TryGetValue(normalized, out var matches))
            {
                return NoSections;
            }

            return matches;
        }

        /// <summary>
        /// The sections of one family. By default only the ENABLED ones, because this is what feeds a new
        /// selection; pass <paramref name="includeDisabled"/> to see the whole family.
        /// </summary>
        public IReadOnlyList<StructuralSectionDefinition> ByFamily(
            StructuralSectionFamily family,
            bool includeDisabled = false)
        {
            if (!_byFamily.TryGetValue(family, out var sections))
            {
                return NoSections;
            }

            return includeDisabled ? sections : sections.Where(section => section.IsEnabled).ToArray();
        }

        /// <summary>Every section available for a NEW selection.</summary>
        public IReadOnlyList<StructuralSectionDefinition> Enabled =>
            All.Where(section => section.IsEnabled).ToArray();

        /// <summary>
        /// Prefix search over the visible designation, for a "type to find" box. Ordinal and case-insensitive
        /// over the NORMALIZED forms, so <c>hss4x4</c>, <c>HSS 4X4</c> and <c>hss4x4x1/4</c> all behave.
        /// </summary>
        public IReadOnlyList<StructuralSectionDefinition> Search(string text, bool includeDisabled = false)
        {
            if (!StructuralSectionDesignationNormalizer.TryNormalize(text, out var normalized))
            {
                return NoSections;
            }

            return All
                .Where(section => includeDisabled || section.IsEnabled)
                .Where(section =>
                    section.Identity.NormalizedManualLabel.StartsWith(normalized, StringComparison.Ordinal) ||
                    section.Identity.NormalizedEdiDesignation.StartsWith(normalized, StringComparison.Ordinal))
                .ToArray();
        }

        public bool TryGetSource(string sourceId, out StructuralSectionSource source) =>
            _sourcesById.TryGetValue(sourceId ?? string.Empty, out source);

        /// <summary>How many sections each family carries. Used by the manifest check.</summary>
        public IReadOnlyDictionary<StructuralSectionFamily, int> CountsByFamily() =>
            _byFamily.ToDictionary(pair => pair.Key, pair => pair.Value.Length);
    }
}
