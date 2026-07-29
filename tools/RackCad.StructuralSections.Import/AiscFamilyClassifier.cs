using System.Collections.Generic;
using RackCad.Application.StructuralSections;

namespace RackCad.StructuralSections.Import
{
    /// <summary>What the classifier decided about one source row.</summary>
    public enum AiscRowDisposition
    {
        /// <summary>The row belongs to one of the four authorized families and must be imported.</summary>
        Selected,

        /// <summary>The row belongs to a type the MVP deliberately leaves out. Reported, never an error.</summary>
        Excluded
    }

    public readonly struct AiscRowClassification
    {
        public AiscRowClassification(
            AiscRowDisposition disposition,
            StructuralSectionFamily family,
            string exclusionToken)
        {
            Disposition = disposition;
            Family = family;
            ExclusionToken = exclusionToken;
        }

        public AiscRowDisposition Disposition { get; }

        /// <summary>Only meaningful when <see cref="Disposition"/> is <see cref="AiscRowDisposition.Selected"/>.</summary>
        public StructuralSectionFamily Family { get; }

        /// <summary>How the exclusion is counted, e.g. <c>2L</c> or <c>HSS-ROUND</c>.</summary>
        public string ExclusionToken { get; }
    }

    /// <summary>
    /// Decides which of the 2 299 published rows the neutral catalog takes.
    ///
    /// The HSS split is the only non-trivial case and it is made from the SOURCE'S OWN FIELDS, exactly as the
    /// Readme defines them, never from a regular expression over the designation:
    ///   <c>OD</c> — "outside diameter of round HSS or pipe" — present  ⇒ ROUND, excluded;
    ///   <c>Ht</c>/<c>B</c>/<c>h</c>/<c>b</c> — the walls of a square or rectangular HSS — present ⇒ imported.
    /// Over AISC v16.0 the partition is exhaustive and clean (525 + 189 = 714 rows, no overlap and no
    /// leftovers). A row that satisfies neither shape, or both, is AMBIGUOUS and stops the import: guessing
    /// there is how a round tube ends up catalogued as a rectangle.
    /// </summary>
    public static class AiscFamilyClassifier
    {
        public const string RoundHssToken = "HSS-ROUND";
        public const string HssType = "HSS";

        public static AiscRowClassification Classify(
            AiscColumnMap columns,
            IReadOnlyDictionary<int, string> row,
            int rowNumber)
        {
            var type = columns.Text(row, AiscColumnMap.Type);

            if (string.IsNullOrEmpty(type))
            {
                throw new XlsxFormatException("Fila " + rowNumber + ": la columna 'Type' esta vacia.");
            }

            switch (type)
            {
                case "W":
                    return Selected(StructuralSectionFamily.W);
                case "C":
                    return Selected(StructuralSectionFamily.Channel);
                case "L":
                    return Selected(StructuralSectionFamily.Angle);
                case "S":
                    // I-36D. Until then this fell through to the default and was counted as an exclusion; the
                    // 28 rows were never missing, they were declined. Nothing else about the row changes.
                    return Selected(StructuralSectionFamily.S);
                case HssType:
                    return ClassifyHss(columns, row, rowNumber);
                default:
                    return Excluded(type);
            }
        }

        private static AiscRowClassification ClassifyHss(
            AiscColumnMap columns,
            IReadOnlyDictionary<int, string> row,
            int rowNumber)
        {
            var hasDiameter = columns.Text(row, AiscColumnMap.OutsideDiameter) != null;
            var hasWalls =
                columns.Text(row, "Ht") != null &&
                columns.Text(row, "B") != null &&
                columns.Text(row, "h") != null &&
                columns.Text(row, "b") != null;

            if (hasDiameter && !hasWalls)
            {
                return Excluded(RoundHssToken);
            }

            if (hasWalls && !hasDiameter)
            {
                return Selected(StructuralSectionFamily.HssRectangular);
            }

            throw new XlsxFormatException(
                "Fila " + rowNumber + ": HSS ambiguo. Diametro exterior " +
                (hasDiameter ? "presente" : "ausente") + " y paredes Ht/B/h/b " +
                (hasWalls ? "presentes" : "ausentes") +
                ". La clasificacion exige exactamente una de las dos formas.");
        }

        private static AiscRowClassification Selected(StructuralSectionFamily family) =>
            new AiscRowClassification(AiscRowDisposition.Selected, family, null);

        private static AiscRowClassification Excluded(string token) =>
            new AiscRowClassification(AiscRowDisposition.Excluded, default, token);
    }
}
