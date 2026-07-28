using System;

namespace RackCad.Application.StructuralSections
{
    /// <summary>Which system of units a source publishes its values in (ADR-0021).</summary>
    public enum StructuralSectionUnitSystem
    {
        /// <summary>Inches, square inches and lb/ft. AISC v16.0 publishes this block as its canonical values.</summary>
        UsCustomary = 0,

        /// <summary>Millimetres and kg/m. No source uses it today; the enum exists so the formatter is not imperial-only.</summary>
        Metric = 1
    }

    public static class StructuralSectionUnitSystems
    {
        public const string UsCustomaryToken = "US_CUSTOMARY";
        public const string MetricToken = "METRIC";

        public static string ToToken(StructuralSectionUnitSystem system)
        {
            switch (system)
            {
                case StructuralSectionUnitSystem.UsCustomary: return UsCustomaryToken;
                case StructuralSectionUnitSystem.Metric: return MetricToken;
                default:
                    throw new ArgumentOutOfRangeException(nameof(system), system, "Sistema de unidades desconocido.");
            }
        }

        /// <summary>Ordinal, case sensitive — same reason as <see cref="StructuralSectionFamilies.TryParseToken"/>.</summary>
        public static bool TryParseToken(string token, out StructuralSectionUnitSystem system)
        {
            switch (token)
            {
                case UsCustomaryToken: system = StructuralSectionUnitSystem.UsCustomary; return true;
                case MetricToken: system = StructuralSectionUnitSystem.Metric; return true;
                default: system = default; return false;
            }
        }
    }

    /// <summary>
    /// Where a set of sections came from. Provenance is recorded because a catalogued dimension is only as
    /// trustworthy as the document it was copied from; it is NOT a legal statement about licensing.
    ///
    /// The revision lives HERE and never inside <see cref="StructuralSectionId"/>, so a section that survives
    /// a future revision keeps its identity (ADR-0021).
    /// </summary>
    public sealed class StructuralSectionSource
    {
        /// <summary>The only source that exists today.</summary>
        public const string AiscShapesId = "AISC-SHAPES";

        /// <summary>The id authority AISC declares. Keeping it <c>AISC</c> is what preserves the 983 ids.</summary>
        public const string AiscIdNamespace = "AISC";

        /// <summary>Stable machine token, e.g. <c>AISC-SHAPES</c>.</summary>
        public string SourceId { get; init; }

        /// <summary>
        /// The authority segment this source stamps on the ids it names, e.g. <c>AISC</c>.
        ///
        /// It is separate from <see cref="SourceId"/> because the two answer different questions: the source
        /// id says WHICH document a row came from (and may carry a revision or a product name), while the id
        /// namespace says WHO has the authority to name the section — a stable, short token that must never
        /// change, because it is embedded in every stored design.
        ///
        /// Under the current schema a namespace identifies exactly ONE source: two sources may never share
        /// it, not even two documents of the same publisher, and <see cref="StructuralSectionCatalog.Create"/>
        /// refuses a catalog where they do. Sharing one would let the same designation resolve to a single id
        /// from two different documents — the collision this segment exists to prevent.
        ///
        /// Must be non-empty and upper-case ASCII letters and digits only; the hyphen is excluded because it
        /// is the id's own separator.
        /// </summary>
        public string IdNamespace { get; init; }

        /// <summary>Revision of the source, e.g. <c>16.0</c>. Deliberately outside the id.</summary>
        public string Revision { get; init; }

        public string Publisher { get; init; }

        /// <summary>What kind of document this is, e.g. <c>official technical database</c>.</summary>
        public string SourceType { get; init; }

        /// <summary>The units the source's own values are expressed in; they are kept, never replaced.</summary>
        public StructuralSectionUnitSystem NativeUnitSystem { get; init; }

        public string Title { get; init; }

        /// <summary>Official page the document is published from. Not fetched at runtime, ever.</summary>
        public string Url { get; init; }

        /// <summary>
        /// The data worksheet a given source and revision MUST have been read from — for AISC,
        /// <c>Database v{revision}</c>.
        ///
        /// It lives here so the importer (which verifies the workbook) and the validator (which verifies the
        /// manifest) agree by construction. Returns <c>false</c> for a source whose worksheet naming this
        /// build does not know, in which case the name is only required to be non-empty: claiming to know a
        /// convention that has not been established would be worse than admitting it.
        /// </summary>
        public static bool TryExpectedWorksheet(string sourceId, string revision, out string worksheet)
        {
            worksheet = null;

            if (!string.Equals(sourceId, AiscShapesId, StringComparison.Ordinal) ||
                string.IsNullOrWhiteSpace(revision))
            {
                return false;
            }

            worksheet = "Database v" + revision;
            return true;
        }

        public override string ToString() => SourceId + " " + Revision;
    }
}
