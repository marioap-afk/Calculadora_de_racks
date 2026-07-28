using System.Collections.Generic;
using System.Text;

namespace RackCad.Application.Catalogs
{
    /// <summary>
    /// The repository's single RFC-4180 lexer: text in, rows of raw fields out.
    ///
    /// It was extracted VERBATIM from <see cref="CsvCatalogReader"/> when I-36A needed a second, STRICT reader
    /// for the neutral structural-section catalog. Splitting the tolerance from the tokenizing is the whole
    /// point: the two readers must disagree about what an invalid VALUE means (the tolerant one keeps the
    /// default, the strict one fails with file/row/column), and must never disagree about what a FIELD is.
    ///
    /// Its quirks are behaviour, not accidents, and <c>CsvLexerTests</c> pins them:
    /// - a quote opens a quoted field wherever it appears, and a doubled quote inside one is a literal quote;
    /// - lone CR characters are dropped, so CRLF and LF files parse identically;
    /// - a final row without a trailing newline is still emitted, and a file ending in a newline does not
    ///   produce a phantom empty row.
    /// </summary>
    public static class CsvLexer
    {
        /// <summary>Minimal RFC-4180 parse: quoted fields, doubled quotes, commas and CR/LF.</summary>
        public static List<string[]> ParseRows(string text)
        {
            var rows = new List<string[]>();

            if (string.IsNullOrEmpty(text))
            {
                return rows;
            }

            var record = new List<string>();
            var field = new StringBuilder();
            var inQuotes = false;
            var sawAny = false;

            for (var i = 0; i < text.Length; i++)
            {
                var ch = text[i];

                if (inQuotes)
                {
                    if (ch == '"')
                    {
                        if (i + 1 < text.Length && text[i + 1] == '"')
                        {
                            field.Append('"');
                            i++;
                        }
                        else
                        {
                            inQuotes = false;
                        }
                    }
                    else
                    {
                        field.Append(ch);
                    }

                    continue;
                }

                switch (ch)
                {
                    case '"':
                        inQuotes = true;
                        sawAny = true;
                        break;
                    case ',':
                        record.Add(field.ToString());
                        field.Clear();
                        sawAny = true;
                        break;
                    case '\r':
                        break;
                    case '\n':
                        record.Add(field.ToString());
                        field.Clear();
                        rows.Add(record.ToArray());
                        record = new List<string>();
                        sawAny = false;
                        break;
                    default:
                        field.Append(ch);
                        sawAny = true;
                        break;
                }
            }

            if (sawAny || field.Length > 0 || record.Count > 0)
            {
                record.Add(field.ToString());
                rows.Add(record.ToArray());
            }

            return rows;
        }
    }
}
