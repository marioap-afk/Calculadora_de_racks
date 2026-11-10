using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace RackCad.Application.Bom
{
    /// <summary>
    /// The pre-calculated cells of a PivotTable sheet, in compact layout: what the reader sees before Excel refreshes it
    /// (Protected View, or the seconds the refresh takes). Excel replaces them with its own result. Grouping ignores case
    /// and respects accents, as Excel does; the order is culture-independent and may differ from the one Excel applies.
    /// </summary>
    internal sealed class XlsxPivotLayout
    {
        private const int NotesColumn = 3;

        private XlsxPivotLayout(IReadOnlyList<IReadOnlyList<XlsxCell>> rows, int headerRow, int lastRow)
        {
            Rows = rows;
            HeaderRow = headerRow;
            LastRow = lastRow;
        }

        public IReadOnlyList<IReadOnlyList<XlsxCell>> Rows { get; }

        /// <summary>One-based row of the PivotTable's header.</summary>
        public int HeaderRow { get; }

        /// <summary>One-based row of its grand total.</summary>
        public int LastRow { get; }

        public static XlsxPivotLayout Compute(XlsxSheet source, XlsxPivotDefinition pivot)
        {
            var columns = XlsxPivotParts.ColumnsOf(source);
            var rowColumns = pivot.RowFields.Select(field => XlsxPivotParts.IndexOf(columns, field, pivot)).ToList();
            var valueColumn = XlsxPivotParts.IndexOf(columns, pivot.ValueField, pivot);
            var records = source.Rows.Skip(1).ToList();

            var rows = new List<List<XlsxCell>>();
            foreach (var filter in pivot.FilterFields)
            {
                XlsxPivotParts.IndexOf(columns, filter, pivot);
                rows.Add(new List<XlsxCell> { XlsxCell.Str(filter), XlsxCell.Str(pivot.AllItemsCaption) });
            }

            rows.Add(new List<XlsxCell>());
            var headerRow = rows.Count + 1;
            rows.Add(new List<XlsxCell> { XlsxCell.Str(pivot.RowHeaderCaption, bold: true), XlsxCell.Str(pivot.ValueCaption, bold: true) });
            AddLevel(rows, records, rowColumns, 0, valueColumn);
            rows.Add(new List<XlsxCell> { XlsxCell.Str(pivot.GrandTotalCaption, bold: true), XlsxCell.Num(Sum(records, valueColumn), bold: true) });
            var lastRow = rows.Count;

            for (var i = 0; i < pivot.Notes.Count; i++)
            {
                while (rows.Count <= i)
                {
                    rows.Add(new List<XlsxCell>());
                }

                while (rows[i].Count < NotesColumn)
                {
                    rows[i].Add(XlsxCell.Blank);
                }

                rows[i].Add(pivot.Notes[i]);
            }

            return new XlsxPivotLayout(rows, headerRow, lastRow);
        }

        private static void AddLevel(List<List<XlsxCell>> rows, IReadOnlyList<IReadOnlyList<XlsxCell>> records, IReadOnlyList<int> rowColumns, int depth, int valueColumn)
        {
            var isLeaf = depth == rowColumns.Count - 1;
            var column = rowColumns[depth];
            var groups = records
                .GroupBy(record => KeyOf(CellAt(record, column)), StringComparer.OrdinalIgnoreCase)
                .Select(group => (Label: CellAt(group.First(), column), Records: group.ToList()))
                .OrderBy(group => group.Label, LabelComparer.Instance);

            foreach (var group in groups)
            {
                rows.Add(new List<XlsxCell> { Label(group.Label, bold: !isLeaf), XlsxCell.Num(Sum(group.Records, valueColumn), bold: !isLeaf) });
                if (!isLeaf)
                {
                    AddLevel(rows, group.Records, rowColumns, depth + 1, valueColumn);
                }
            }
        }

        private static XlsxCell CellAt(IReadOnlyList<XlsxCell> record, int column)
            => column < record.Count ? record[column] : XlsxCell.Blank;

        /// <summary>Numbers group by the value actually written to the sheet, so the summary matches what Excel reads.</summary>
        private static string KeyOf(XlsxCell cell)
            => cell.Number.HasValue ? "n" + cell.Number.Value.ToString("0.######", CultureInfo.InvariantCulture) : "t" + (cell.Text ?? string.Empty);

        private static XlsxCell Label(XlsxCell cell, bool bold)
            => cell.Number.HasValue ? XlsxCell.Num(cell.Number.Value, bold) : XlsxCell.Str(cell.Text, bold);

        private static double Sum(IEnumerable<IReadOnlyList<XlsxCell>> records, int valueColumn)
            => records.Sum(record => CellAt(record, valueColumn).Number ?? 0.0);

        private sealed class LabelComparer : IComparer<XlsxCell>
        {
            public static readonly LabelComparer Instance = new LabelComparer();

            public int Compare(XlsxCell x, XlsxCell y)
            {
                if (x.Number.HasValue && y.Number.HasValue)
                {
                    return x.Number.Value.CompareTo(y.Number.Value);
                }

                if (x.Number.HasValue != y.Number.HasValue)
                {
                    return x.Number.HasValue ? -1 : 1;
                }

                return StringComparer.OrdinalIgnoreCase.Compare(x.Text ?? string.Empty, y.Text ?? string.Empty);
            }
        }
    }
}
