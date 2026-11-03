using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RackCad.Application.StructuralSections;

namespace RackCad.Application.StructuralAnalysis.Providers.RamElements
{
    /// <summary>
    /// The one provider table (I-65 D-06, D-07): neutral section id to the names of the analysis program. It is the ONLY product
    /// implementation of <see cref="IStructuralMaterialProfile"/> that assigns materials. A table only exists when it is valid.
    /// </summary>
    public sealed class RamSectionMaterialTable : IStructuralMaterialProfile
    {
        private readonly Dictionary<StructuralSectionId, RamSectionMapRow> _bySection;
        private readonly Dictionary<StructuralSectionId, StructuralMaterialRow> _neutral;

        internal RamSectionMaterialTable(IEnumerable<RamSectionMapRow> rows)
        {
            var ordered = rows.OrderBy(row => row.SectionId.Value, StringComparer.Ordinal).ToList();

            _bySection = ordered.ToDictionary(row => row.SectionId);
            _neutral = ordered.ToDictionary(row => row.SectionId, row => row.ToNeutralRow());
            Rows = new ReadOnlyCollection<RamSectionMapRow>(ordered);
        }

        /// <summary>The rows in ordinal order of section id.</summary>
        public IReadOnlyList<RamSectionMapRow> Rows { get; }

        public bool TryGetRamRow(StructuralSectionId sectionId, out RamSectionMapRow row) =>
            _bySection.TryGetValue(sectionId, out row);

        /// <summary>
        /// The neutral projection of the row of <paramref name="sectionId"/>. A proposed row is projected WITHOUT verifier or date,
        /// so the model builder refuses it as unverified: a proposed row assigns nothing.
        /// </summary>
        public bool TryGetRow(StructuralSectionId sectionId, out StructuralMaterialRow row) =>
            _neutral.TryGetValue(sectionId, out row);
    }

    /// <summary>The result of reading the table: the table when, and only when, the outcome is Valid.</summary>
    public sealed class RamSectionMapReadResult
    {
        internal RamSectionMapReadResult(
            StructuralExportOutcome outcome, IReadOnlyList<StructuralDiagnostic> diagnostics, RamSectionMaterialTable table)
        {
            Outcome = outcome;
            Diagnostics = diagnostics;
            Table = table;
        }

        public StructuralExportOutcome Outcome { get; }

        public IReadOnlyList<StructuralDiagnostic> Diagnostics { get; }

        public RamSectionMaterialTable Table { get; }
    }
}
