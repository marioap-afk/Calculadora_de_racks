using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RackCad.StructuralSections.Import
{
    /// <summary>What the verification concluded about a workbook.</summary>
    public sealed class AiscWorkbookIdentity
    {
        /// <summary>Revision proven by the workbook's own Readme, e.g. <c>16.0</c>.</summary>
        public string Revision { get; init; }

        /// <summary>Name of the data worksheet that revision requires, e.g. <c>Database v16.0</c>.</summary>
        public string DataWorksheet { get; init; }
    }

    /// <summary>
    /// Proves that a workbook IS the AISC Shapes Database v16.0 before a single row is imported.
    ///
    /// The check the importer used to do was accidental: it read whatever sheet it was told to and, if the
    /// headers happened to fit, stamped the output with "AISC Shapes Database v16.0". Any spreadsheet with
    /// compatible column names — a fork, a hand-edited copy, a different revision, another vendor's export —
    /// would have been catalogued as the official source, and the manifest would have said so.
    ///
    /// Identity is therefore verified by CONTENT AND STRUCTURE, not by a file hash: pinning the current
    /// SHA-256 as the only acceptable workbook would make every legitimate future revision impossible to
    /// import. The hash keeps being RECORDED in the manifest as provenance; it is not the gate.
    ///
    /// Four things must hold, all of them published by the document itself:
    ///   1. a <c>Readme</c> sheet exists and can be read;
    ///   2. it names the product and its revision (<c>AISC Shapes Database v16.0</c>);
    ///   3. it names the manual edition (<c>16th Edition</c>) and the EDI naming convention, which are what
    ///      make it THIS database and not a similarly-shaped table;
    ///   4. the data sheet the revision implies (<c>Database v16.0</c>) exists.
    /// </summary>
    public static class AiscWorkbookVerifier
    {
        public const string ReadmeSheetName = "Readme";

        /// <summary>The only revision this importer knows how to map (owner decision 7).</summary>
        public const string SupportedRevision = "16.0";

        private const string ProductMarker = "AISC SHAPES DATABASE V16.0";
        private const string EditionMarker = "16TH EDITION";
        private const string EdiMarker = "ELECTRONIC DATA INTERCHANGE";

        /// <summary>The data worksheet a given revision must publish.</summary>
        public static string DataWorksheetFor(string revision) => "Database v" + revision;

        public static AiscWorkbookIdentity Verify(XlsxWorkbook workbook)
        {
            if (workbook == null)
            {
                throw new ArgumentNullException(nameof(workbook));
            }

            var sheets = workbook.SheetNames;

            if (!sheets.Contains(ReadmeSheetName, StringComparer.Ordinal))
            {
                throw new XlsxFormatException(
                    "El libro no tiene la hoja '" + ReadmeSheetName +
                    "', asi que no puede acreditar que sea la AISC Shapes Database. Hojas presentes: " +
                    string.Join(", ", sheets) + ".");
            }

            var readme = ReadAllText(workbook, ReadmeSheetName);

            if (readme.Length == 0)
            {
                throw new XlsxFormatException(
                    "La hoja '" + ReadmeSheetName + "' esta vacia: no acredita producto ni revision.");
            }

            RequireMarker(readme, ProductMarker,
                "no declara ser la 'AISC Shapes Database v16.0'");
            RequireMarker(readme, EditionMarker,
                "no menciona la '16th Edition' del AISC Steel Construction Manual");
            RequireMarker(readme, EdiMarker,
                "no menciona la convencion de nomenclatura EDI (Electronic Data Interchange)");

            var dataWorksheet = DataWorksheetFor(SupportedRevision);

            if (!sheets.Contains(dataWorksheet, StringComparer.Ordinal))
            {
                throw new XlsxFormatException(
                    "El Readme acredita la revision " + SupportedRevision + " pero el libro no tiene la hoja " +
                    "de datos '" + dataWorksheet + "'. Hojas presentes: " + string.Join(", ", sheets) + ".");
            }

            return new AiscWorkbookIdentity
            {
                Revision = SupportedRevision,
                DataWorksheet = dataWorksheet
            };
        }

        /// <summary>
        /// Checks that the revision, the data sheet and the metadata about to be written tell the SAME story.
        /// Called after mapping, so a future change that decoupled any of the three fails loudly here instead
        /// of shipping a manifest that contradicts its own data.
        /// </summary>
        public static void RequireCoherentMetadata(
            AiscWorkbookIdentity identity,
            string worksheetUsed,
            string declaredRevision)
        {
            if (!string.Equals(identity.DataWorksheet, worksheetUsed, StringComparison.Ordinal))
            {
                throw new XlsxFormatException(
                    "Incoherencia: se acredito la hoja de datos '" + identity.DataWorksheet +
                    "' y se leyo '" + worksheetUsed + "'.");
            }

            if (!string.Equals(identity.Revision, declaredRevision, StringComparison.Ordinal))
            {
                throw new XlsxFormatException(
                    "Incoherencia: el libro acredita la revision '" + identity.Revision +
                    "' y la metadata generada declara '" + declaredRevision + "'.");
            }

            if (!string.Equals(
                    identity.DataWorksheet,
                    DataWorksheetFor(declaredRevision),
                    StringComparison.Ordinal))
            {
                throw new XlsxFormatException(
                    "Incoherencia: la hoja '" + identity.DataWorksheet +
                    "' no corresponde a la revision declarada '" + declaredRevision + "'.");
            }
        }

        private static void RequireMarker(string readme, string marker, string what)
        {
            if (readme.IndexOf(marker, StringComparison.Ordinal) < 0)
            {
                throw new XlsxFormatException(
                    "La hoja '" + ReadmeSheetName + "' " + what + " (no se encontro '" + marker + "').");
            }
        }

        /// <summary>
        /// The whole Readme as one upper-case string with runs of whitespace collapsed, so a marker is found
        /// regardless of how the text is split across cells, wrapped across lines or spaced.
        /// </summary>
        private static string ReadAllText(XlsxWorkbook workbook, string sheetName)
        {
            var builder = new StringBuilder();

            foreach (var row in workbook.ReadRows(sheetName))
            {
                foreach (var cell in row.Value.OrderBy(entry => entry.Key))
                {
                    builder.Append(cell.Value).Append(' ');
                }
            }

            var collapsed = new StringBuilder(builder.Length);
            var previousWasSpace = false;

            foreach (var ch in builder.ToString())
            {
                if (char.IsWhiteSpace(ch))
                {
                    if (!previousWasSpace)
                    {
                        collapsed.Append(' ');
                        previousWasSpace = true;
                    }

                    continue;
                }

                collapsed.Append(char.ToUpperInvariant(ch));
                previousWasSpace = false;
            }

            return collapsed.ToString().Trim();
        }
    }
}
