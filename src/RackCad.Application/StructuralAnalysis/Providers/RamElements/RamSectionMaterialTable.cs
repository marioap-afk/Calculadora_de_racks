using System;
using System.Collections.Generic;
using RackCad.Application.StructuralSections;

namespace RackCad.Application.StructuralAnalysis.Providers.RamElements
{
    /// <summary>
    /// The one provider table (I-65 D-06, D-07): neutral section id to the names of the analysis program. It is the ONLY product
    /// implementation of <see cref="IStructuralMaterialProfile"/> that assigns materials. A table only exists when it is valid.
    /// </summary>
    public sealed class RamSectionMaterialTable : IStructuralMaterialProfile
    {
        internal RamSectionMaterialTable(IReadOnlyList<RamSectionMapRow> rows)
        {
            Rows = rows;
        }

        /// <summary>The rows in ordinal order of section id.</summary>
        public IReadOnlyList<RamSectionMapRow> Rows { get; }

        public bool TryGetRamRow(StructuralSectionId sectionId, out RamSectionMapRow row) => throw new NotImplementedException();

        public bool TryGetRow(StructuralSectionId sectionId, out StructuralMaterialRow row) => throw new NotImplementedException();
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

    /// <summary>The strict reader of <c>rackcad-ram-elements-section-map/v1</c>. It receives TEXT: it does no I/O.</summary>
    public static class RamSectionMapReader
    {
        public const string SchemaId = "rackcad-ram-elements-section-map/v1";

        public static RamSectionMapReadResult Read(string json) => throw new NotImplementedException();
    }
}
