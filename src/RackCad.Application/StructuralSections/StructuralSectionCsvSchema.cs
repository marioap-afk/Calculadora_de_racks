using System;
using System.Collections.Generic;
using System.Linq;

namespace RackCad.Application.StructuralSections
{
    /// <summary>
    /// The ONE authority for the column names and column ORDER of every structural-section file.
    ///
    /// The importer writes with it and the provider reads with it, so a schema change is impossible to apply
    /// to only one side — which is exactly how a generated catalog and its reader drift apart. The order is
    /// part of the contract because the generated files must be byte-identical between runs.
    ///
    /// Naming rule, deliberate: RackCad's own fields use camelCase like every other catalog, and AISC's own
    /// variables keep their PUBLISHED spelling (<c>Ix</c>, <c>bf</c>, <c>kdes</c>, <c>Ht</c> vs <c>h</c>).
    /// Lower-casing them would merge <c>B</c> with <c>b</c> and <c>Ht</c> with <c>ht</c>, which are different
    /// measurements. Header matching is therefore ORDINAL and case sensitive. Only three source names are
    /// rewritten, because a CSV header cannot carry them safely:
    ///   <c>twdet/2</c> → <c>twdet_2</c>, <c>tan(α)</c> → <c>tanAlpha</c>, and the sheet's <c>W</c> (weight,
    ///   which collides with the W family token) → <c>weightPerLength</c>.
    /// </summary>
    public static class StructuralSectionCsvSchema
    {
        // ---- File names -------------------------------------------------------------------------------

        public const string WFile = "structural-sections-w.csv";
        public const string HssRectangularFile = "structural-sections-hss-rect.csv";
        public const string ChannelFile = "structural-sections-c.csv";
        public const string AngleFile = "structural-sections-l.csv";
        public const string SourcesFile = "structural-section-sources.csv";
        public const string StatusFile = "structural-section-status.csv";
        public const string ManifestFile = "structural-sections-manifest.json";

        // ---- Shared identity columns ------------------------------------------------------------------

        public const string SectionId = "sectionId";
        public const string Family = "family";
        public const string EdiDesignation = "ediDesignation";
        public const string ManualLabel = "manualLabel";
        public const string SourceId = "sourceId";
        public const string SourceRevision = "sourceRevision";
        public const string WeightPerLength = "weightPerLength";
        public const string NativeUnitSystem = "nativeUnitSystem";

        // ---- Sources file ------------------------------------------------------------------------------

        public const string SourceRevisionColumn = "revision";
        public const string Publisher = "publisher";
        public const string SourceType = "sourceType";
        public const string Title = "title";
        public const string Url = "url";

        // ---- Status overlay ----------------------------------------------------------------------------

        public const string IsEnabled = "isEnabled";
        public const string Notes = "notes";

        /// <summary>Identity block shared by the four family files, in order.</summary>
        public static readonly string[] IdentityColumns =
        {
            SectionId, Family, EdiDesignation, ManualLabel, SourceId, SourceRevision,
            WeightPerLength, NativeUnitSystem
        };

        /// <summary>Section properties every one of the four families publishes.</summary>
        public static readonly string[] CommonPropertyColumns =
        {
            "A", "Ix", "Zx", "Sx", "rx", "Iy", "Zy", "Sy", "ry", "J"
        };

        /// <summary>W: dimensions, the special-note flag and the properties only W and C tabulate.</summary>
        public static readonly string[] WSpecificColumns =
        {
            "T_F",
            "d", "ddet", "bf", "bfdet", "tw", "twdet", "twdet_2", "tf", "tfdet",
            "kdes", "kdet", "k1", "T", "WGi", "WGo",
            "Cw", "Wno", "Sw1", "Qf", "Qw", "rts", "ho", "PA", "PB", "PC", "PD"
        };

        /// <summary>Rectangular/square HSS: walls, flat walls, the two thicknesses and its torsional constant.</summary>
        public static readonly string[] HssRectangularSpecificColumns =
        {
            "Ht", "B", "h", "b", "tnom", "tdes",
            "C"
        };

        /// <summary>C: like W plus the three eccentricities an asymmetric channel needs.</summary>
        public static readonly string[] ChannelSpecificColumns =
        {
            "d", "ddet", "bf", "bfdet", "tw", "twdet", "twdet_2", "tf", "tfdet",
            "kdes", "kdet", "x", "eo", "xp", "T", "WGi",
            "Cw", "Wno", "Sw1", "Sw2", "Sw3", "Qf", "Qw", "ro", "H", "rts", "ho", "PA", "PB", "PC", "PD"
        };

        /// <summary>L: both legs, the centroid, the principal axes and the point A/B/C geometry.</summary>
        public static readonly string[] AngleSpecificColumns =
        {
            "d", "b", "t", "kdes", "kdet", "x", "y", "xp", "yp",
            "Iz", "rz", "Sz", "Cw", "ro", "H", "tanAlpha", "Iw",
            "zA", "zB", "zC", "wA", "wB", "wC",
            "SwA", "SwB", "SwC", "SzA", "SzB", "SzC",
            "PA", "PA2", "PB"
        };

        public static readonly string[] SourcesColumns =
        {
            SourceId, SourceRevisionColumn, Publisher, SourceType, NativeUnitSystem, Title, Url
        };

        public static readonly string[] StatusColumns = { SectionId, IsEnabled, Notes };

        public static string FileFor(StructuralSectionFamily family)
        {
            switch (family)
            {
                case StructuralSectionFamily.W: return WFile;
                case StructuralSectionFamily.HssRectangular: return HssRectangularFile;
                case StructuralSectionFamily.Channel: return ChannelFile;
                case StructuralSectionFamily.Angle: return AngleFile;
                default:
                    throw new ArgumentOutOfRangeException(nameof(family), family, "Familia desconocida.");
            }
        }

        /// <summary>The complete, ordered header list of a family file: identity, then common, then specific.</summary>
        public static string[] ColumnsFor(StructuralSectionFamily family)
        {
            return IdentityColumns
                .Concat(CommonPropertyColumns)
                .Concat(SpecificColumns(family))
                .ToArray();
        }

        private static IReadOnlyList<string> SpecificColumns(StructuralSectionFamily family)
        {
            switch (family)
            {
                case StructuralSectionFamily.W: return WSpecificColumns;
                case StructuralSectionFamily.HssRectangular: return HssRectangularSpecificColumns;
                case StructuralSectionFamily.Channel: return ChannelSpecificColumns;
                case StructuralSectionFamily.Angle: return AngleSpecificColumns;
                default:
                    throw new ArgumentOutOfRangeException(nameof(family), family, "Familia desconocida.");
            }
        }

        /// <summary>Every file the neutral catalog is distributed as, in a stable order.</summary>
        public static string[] AllFiles()
        {
            return StructuralSectionFamilies.All
                .Select(FileFor)
                .Concat(new[] { SourcesFile, StatusFile })
                .ToArray();
        }
    }
}
