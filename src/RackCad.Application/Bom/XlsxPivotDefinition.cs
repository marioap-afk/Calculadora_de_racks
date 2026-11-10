using System;
using System.Collections.Generic;

namespace RackCad.Application.Bom
{
    /// <summary>
    /// Declares a native Excel PivotTable over a table of the same workbook. Its sheet holds nothing else: the writer
    /// fills it with the pre-calculated result, so the summary is visible before Excel refreshes it.
    /// </summary>
    public sealed class XlsxPivotDefinition
    {
        public XlsxPivotDefinition(
            string name,
            string sourceTable,
            IReadOnlyList<string> filterFields,
            IReadOnlyList<string> rowFields,
            string valueField,
            string valueCaption,
            string rowHeaderCaption,
            string grandTotalCaption,
            string allItemsCaption,
            IReadOnlyList<XlsxCell> notes)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            SourceTable = sourceTable ?? throw new ArgumentNullException(nameof(sourceTable));
            FilterFields = filterFields ?? Array.Empty<string>();
            RowFields = rowFields ?? throw new ArgumentNullException(nameof(rowFields));
            ValueField = valueField ?? throw new ArgumentNullException(nameof(valueField));
            ValueCaption = valueCaption ?? valueField;
            RowHeaderCaption = rowHeaderCaption ?? string.Empty;
            GrandTotalCaption = grandTotalCaption ?? string.Empty;
            AllItemsCaption = allItemsCaption ?? string.Empty;
            Notes = notes ?? Array.Empty<XlsxCell>();
        }

        public string Name { get; }
        public string SourceTable { get; }
        public IReadOnlyList<string> FilterFields { get; }
        public IReadOnlyList<string> RowFields { get; }
        public string ValueField { get; }
        public string ValueCaption { get; }
        public string RowHeaderCaption { get; }
        public string GrandTotalCaption { get; }

        /// <summary>The text a filter cell shows while nothing is selected.</summary>
        public string AllItemsCaption { get; }

        /// <summary>Lines written one per row in column D, outside the area the PivotTable can occupy.</summary>
        public IReadOnlyList<XlsxCell> Notes { get; }
    }
}
