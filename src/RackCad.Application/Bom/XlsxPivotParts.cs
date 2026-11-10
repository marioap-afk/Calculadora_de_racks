using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace RackCad.Application.Bom
{
    /// <summary>
    /// The two OOXML parts of a native PivotTable, in MINIMAL form: the cache and the fields list no values, and
    /// <c>refreshOnLoad</c> makes Excel rebuild them from the source table when it opens the file. Listing values is not an
    /// option: Excel refuses a workbook whose cached items break its own rules (duplicates differing only in case, all-integer
    /// numbers without <c>containsInteger</c>), and those rules are Excel's to apply.
    /// </summary>
    internal static class XlsxPivotParts
    {
        private const string Ns = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private const string RelNs = "http://schemas.openxmlformats.org/officeDocument/2006/relationships";
        private const string XmlHeader = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"yes\"?>";

        /// <summary>Fixed so the same BOM always yields the same bytes; Excel replaces it on the first refresh.</summary>
        private const string RefreshedDate = "45000";

        public static IReadOnlyList<string> ColumnsOf(XlsxSheet source)
            => source.Rows.Count == 0 ? Array.Empty<string>() : source.Rows[0].Select(cell => cell.Text ?? string.Empty).ToList();

        public static int IndexOf(IReadOnlyList<string> columns, string field, XlsxPivotDefinition pivot)
        {
            for (var i = 0; i < columns.Count; i++)
            {
                if (string.Equals(columns[i], field, StringComparison.Ordinal))
                {
                    return i;
                }
            }

            throw new InvalidOperationException("La tabla dinamica '" + pivot.Name + "' usa el campo inexistente '" + field + "'.");
        }

        public static string CacheDefinition(XlsxPivotDefinition pivot, IReadOnlyList<string> columns)
        {
            var builder = new StringBuilder();
            builder.Append(XmlHeader);
            builder.Append("<pivotCacheDefinition xmlns=\"").Append(Ns).Append("\" xmlns:r=\"").Append(RelNs)
                .Append("\" saveData=\"0\" refreshOnLoad=\"1\" refreshedBy=\"RackCad\" refreshedDate=\"").Append(RefreshedDate)
                .Append("\" createdVersion=\"6\" refreshedVersion=\"6\" minRefreshableVersion=\"3\" recordCount=\"0\">");
            builder.Append("<cacheSource type=\"worksheet\"><worksheetSource name=\"").Append(XlsxWriter.EscapeAttr(pivot.SourceTable)).Append("\"/></cacheSource>");
            builder.Append("<cacheFields count=\"").Append(Count(columns.Count)).Append("\">");
            foreach (var column in columns)
            {
                builder.Append("<cacheField name=\"").Append(XlsxWriter.EscapeAttr(column)).Append("\" numFmtId=\"0\"><sharedItems/></cacheField>");
            }

            builder.Append("</cacheFields></pivotCacheDefinition>");
            return builder.ToString();
        }

        public static string TableDefinition(XlsxPivotDefinition pivot, IReadOnlyList<string> columns, XlsxPivotLayout layout, int cacheId)
        {
            var filters = pivot.FilterFields.Select(field => IndexOf(columns, field, pivot)).ToList();
            var rows = pivot.RowFields.Select(field => IndexOf(columns, field, pivot)).ToList();
            var value = IndexOf(columns, pivot.ValueField, pivot);

            var builder = new StringBuilder();
            builder.Append(XmlHeader);
            builder.Append("<pivotTableDefinition xmlns=\"").Append(Ns).Append("\" name=\"").Append(XlsxWriter.EscapeAttr(pivot.Name))
                .Append("\" cacheId=\"").Append(Count(cacheId))
                .Append("\" applyNumberFormats=\"0\" applyBorderFormats=\"0\" applyFontFormats=\"0\" applyPatternFormats=\"0\" applyAlignmentFormats=\"0\" applyWidthHeightFormats=\"1\"")
                .Append(" dataCaption=\"Valores\" updatedVersion=\"6\" minRefreshableVersion=\"3\" useAutoFormatting=\"1\" itemPrintTitles=\"1\" createdVersion=\"6\" indent=\"0\"")
                .Append(" outline=\"1\" outlineData=\"1\" multipleFieldFilters=\"0\" rowHeaderCaption=\"").Append(XlsxWriter.EscapeAttr(pivot.RowHeaderCaption))
                .Append("\" grandTotalCaption=\"").Append(XlsxWriter.EscapeAttr(pivot.GrandTotalCaption)).Append("\">");

            builder.Append("<location ref=\"A").Append(Count(layout.HeaderRow)).Append(":B").Append(Count(layout.LastRow))
                .Append("\" firstHeaderRow=\"1\" firstDataRow=\"1\" firstDataCol=\"1\" rowPageCount=\"").Append(Count(filters.Count)).Append("\" colPageCount=\"1\"/>");

            builder.Append("<pivotFields count=\"").Append(Count(columns.Count)).Append("\">");
            for (var i = 0; i < columns.Count; i++)
            {
                builder.Append(PivotField(i, filters, rows, value));
            }

            builder.Append("</pivotFields>");

            builder.Append("<rowFields count=\"").Append(Count(rows.Count)).Append("\">");
            foreach (var row in rows)
            {
                builder.Append("<field x=\"").Append(Count(row)).Append("\"/>");
            }

            builder.Append("</rowFields><rowItems count=\"1\"><i t=\"grand\"><x/></i></rowItems><colItems count=\"1\"><i/></colItems>");

            builder.Append("<pageFields count=\"").Append(Count(filters.Count)).Append("\">");
            foreach (var filter in filters)
            {
                builder.Append("<pageField fld=\"").Append(Count(filter)).Append("\" hier=\"-1\"/>");
            }

            builder.Append("</pageFields>");
            builder.Append("<dataFields count=\"1\"><dataField name=\"").Append(XlsxWriter.EscapeAttr(pivot.ValueCaption))
                .Append("\" fld=\"").Append(Count(value)).Append("\" baseField=\"0\" baseItem=\"0\"/></dataFields>");
            builder.Append("<pivotTableStyleInfo name=\"PivotStyleLight16\" showRowHeaders=\"1\" showColHeaders=\"1\" showRowStripes=\"0\" showColStripes=\"0\" showLastColumn=\"1\"/>");
            builder.Append("</pivotTableDefinition>");
            return builder.ToString();
        }

        private static string PivotField(int index, IReadOnlyList<int> filters, IReadOnlyList<int> rows, int value)
        {
            const string Items = "<items count=\"1\"><item t=\"default\"/></items></pivotField>";
            if (filters.Contains(index))
            {
                return "<pivotField axis=\"axisPage\" showAll=\"0\">" + Items;
            }

            if (rows.Contains(index))
            {
                return "<pivotField axis=\"axisRow\" showAll=\"0\" sortType=\"ascending\">" + Items;
            }

            return index == value ? "<pivotField dataField=\"1\" showAll=\"0\"/>" : "<pivotField showAll=\"0\"/>";
        }

        private static string Count(int value) => value.ToString(CultureInfo.InvariantCulture);
    }
}
