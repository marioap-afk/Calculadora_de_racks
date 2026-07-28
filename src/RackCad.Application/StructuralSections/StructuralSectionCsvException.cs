using System;

namespace RackCad.Application.StructuralSections
{
    /// <summary>
    /// Raised by the strict reader. Every instance pins WHERE the problem is — file, 1-based line, column and
    /// section id when one is already known — because the reader's whole reason to exist is that a bad cell
    /// must be findable instead of becoming a default value nobody notices.
    /// </summary>
    public sealed class StructuralSectionCsvException : Exception
    {
        public StructuralSectionCsvException(
            string fileName,
            int line,
            string column,
            string sectionId,
            string reason)
            : base(BuildMessage(fileName, line, column, sectionId, reason))
        {
            FileName = fileName ?? string.Empty;
            Line = line;
            Column = column ?? string.Empty;
            SectionId = sectionId ?? string.Empty;
            Reason = reason ?? string.Empty;
        }

        /// <summary>Name of the offending file, without its directory.</summary>
        public string FileName { get; }

        /// <summary>1-based line inside the file. 0 when the problem is the file as a whole.</summary>
        public int Line { get; }

        /// <summary>Header name of the offending column; empty when the problem is not a single cell.</summary>
        public string Column { get; }

        /// <summary>Id of the offending row when it could be read; empty otherwise.</summary>
        public string SectionId { get; }

        /// <summary>What is wrong, in Spanish, without the location prefix.</summary>
        public string Reason { get; }

        private static string BuildMessage(string fileName, int line, string column, string sectionId, string reason)
        {
            var where = fileName ?? "<archivo>";

            if (line > 0)
            {
                where += ":" + line;
            }

            if (!string.IsNullOrEmpty(column))
            {
                where += " columna '" + column + "'";
            }

            if (!string.IsNullOrEmpty(sectionId))
            {
                where += " seccion '" + sectionId + "'";
            }

            return where + ": " + (reason ?? "error no especificado");
        }
    }
}
