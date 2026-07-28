using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using RackCad.Application.Catalogs;

namespace RackCad.Application.StructuralSections
{
    /// <summary>
    /// Loads the neutral catalog from the seven files distributed under <c>catalogs/</c>, STRICTLY.
    ///
    /// It deliberately does NOT reuse <see cref="JsonRackCatalogProvider"/>: that provider's contract is
    /// tolerance (a missing file yields an empty list, a bad cell keeps a default), which is right for the
    /// legacy catalog and wrong here. A structural file that is missing, unreadable or inconsistent must stop
    /// the load with a message naming file, row and column.
    ///
    /// The cache mirrors the shape of the legacy one — per directory, invalidated by a signature over the
    /// structural files' name+size+mtime — so editing the status overlay and re-running a command picks the
    /// change up without a restart. It is a SEPARATE cache: nothing here touches the legacy provider's.
    /// </summary>
    public sealed class CsvStructuralSectionCatalogProvider : IStructuralSectionCatalogProvider
    {
        private static readonly ConcurrentDictionary<string, CacheEntry> Cache =
            new ConcurrentDictionary<string, CacheEntry>(StringComparer.OrdinalIgnoreCase);

        private readonly string _directory;

        public CsvStructuralSectionCatalogProvider(string directory)
        {
            _directory = directory ?? throw new ArgumentNullException(nameof(directory));
        }

        /// <summary>The <c>catalogs</c> folder next to the executing assembly, exactly as the plugin ships it.</summary>
        public static CsvStructuralSectionCatalogProvider FromBaseDirectory() =>
            new CsvStructuralSectionCatalogProvider(CatalogDirectory.Resolve());

        public string Directory => _directory;

        private sealed class CacheEntry
        {
            public string Signature;
            public StructuralSectionCatalog Catalog;
        }

        /// <summary>
        /// The ONE public way to obtain the catalog. It parses strictly, then VALIDATES —semantic invariants,
        /// manifest metadata, the exact file set and the SHA-256 of every immutable file— and only then
        /// caches. Anything wrong throws <see cref="StructuralSectionCatalogException"/>.
        ///
        /// It fails closed on purpose. A folder that merely parses is not a catalog: the files can be replaced
        /// in a deployed installation, and a half-published import leaves new CSVs beside an old manifest.
        /// Handing that to a consumer as if it were valid is the failure mode this method exists to remove, so
        /// there is deliberately no public way to get an unvalidated catalog.
        ///
        /// The status overlay is validated separately and never participates in a hash (ADR-0020): a
        /// legitimate "disable this section" edit must not look like corruption of the imported data.
        /// </summary>
        public StructuralSectionCatalog Load()
        {
            var signature = ComputeSignature();
            var entry = Cache.GetOrAdd(_directory, _ => new CacheEntry());

            lock (entry)
            {
                if (entry.Catalog != null && entry.Signature == signature)
                {
                    return entry.Catalog;
                }

                var catalog = LoadUnvalidated();
                Validate(catalog);

                entry.Signature = signature;
                entry.Catalog = catalog;
                return catalog;
            }
        }

        private void Validate(StructuralSectionCatalog catalog)
        {
            var validator = new StructuralSectionCatalogValidator();
            StructuralSectionsManifest manifest;

            try
            {
                manifest = ReadManifest();
            }
            catch (Exception ex) when (!(ex is StructuralSectionCatalogException))
            {
                throw new StructuralSectionCatalogException(
                    _directory, "no se pudo leer el manifiesto: " + ex.Message, ex);
            }

            var report = validator.Validate(catalog, manifest, ComputeSha256);
            var overlay = validator.ValidateOverlay(catalog, ReadStatusOverrides());

            if (report.HasErrors || overlay.HasErrors)
            {
                throw new StructuralSectionCatalogException(
                    _directory,
                    "el catalogo de secciones estructurales no es valido." + Environment.NewLine +
                    report.Format() + Environment.NewLine + overlay.Format());
            }
        }

        /// <summary>
        /// Parses the files WITHOUT validating them. Internal and diagnostic only: it exists so the validator
        /// and the tests can look at a catalog that is deliberately broken. Production code uses
        /// <see cref="Load"/>.
        /// </summary>
        internal StructuralSectionCatalog LoadUnvalidated()
        {
            var sources = ReadSources();
            var overrides = ReadStatusOverrides();
            var sections = new List<StructuralSectionDefinition>();

            foreach (var family in StructuralSectionFamilies.All)
            {
                var fileName = StructuralSectionCsvSchema.FileFor(family);
                var table = StrictCsvTable.Parse(
                    fileName, ReadText(fileName), StructuralSectionCsvSchema.ColumnsFor(family));

                foreach (var row in table.Rows)
                {
                    sections.Add(StructuralSectionCsvSerializer.FromRow(family, row));
                }
            }

            return StructuralSectionCatalog.Create(ApplyStatus(sections, overrides), sources);
        }

        public IReadOnlyList<StructuralSectionSource> ReadSources()
        {
            var fileName = StructuralSectionCsvSchema.SourcesFile;
            var table = StrictCsvTable.Parse(
                fileName, ReadText(fileName), StructuralSectionCsvSchema.SourcesColumns);

            return table.Rows
                .Select(row => new StructuralSectionSource
                {
                    SourceId = row.RequiredText(StructuralSectionCsvSchema.SourceId),
                    Revision = row.RequiredText(StructuralSectionCsvSchema.SourceRevisionColumn),
                    IdNamespace = RequiredNamespace(row),
                    Publisher = row.RequiredText(StructuralSectionCsvSchema.Publisher),
                    SourceType = row.RequiredText(StructuralSectionCsvSchema.SourceType),
                    NativeUnitSystem = row.RequiredUnitSystem(StructuralSectionCsvSchema.NativeUnitSystem),
                    Title = row.OptionalText(StructuralSectionCsvSchema.Title),
                    Url = row.OptionalText(StructuralSectionCsvSchema.Url)
                })
                .ToArray();
        }

        /// <summary>Reads and validates the id authority of a source row: the shape is part of the schema.</summary>
        private static string RequiredNamespace(StrictCsvTable.Row row)
        {
            var value = row.RequiredText(StructuralSectionCsvSchema.IdNamespace);

            if (!StructuralSectionId.IsValidNamespace(value))
            {
                throw row.Fail(
                    StructuralSectionCsvSchema.IdNamespace,
                    "'" + value + "' no es un namespace de id valido; solo se admiten A-Z y 0-9.");
            }

            return value;
        }

        public IReadOnlyList<StructuralSectionStatusOverride> ReadStatusOverrides()
        {
            var fileName = StructuralSectionCsvSchema.StatusFile;
            var table = StrictCsvTable.Parse(
                fileName, ReadText(fileName), StructuralSectionCsvSchema.StatusColumns);

            return table.Rows
                .Select(row =>
                {
                    var id = row.RequiredSectionId(StructuralSectionCsvSchema.SectionId);
                    row.SectionId = id.Value;

                    return new StructuralSectionStatusOverride
                    {
                        SectionId = id,
                        IsEnabled = row.RequiredBool(StructuralSectionCsvSchema.IsEnabled),
                        Notes = row.OptionalText(StructuralSectionCsvSchema.Notes)
                    };
                })
                .ToArray();
        }

        public StructuralSectionsManifest ReadManifest()
        {
            return StructuralSectionsManifest.FromJson(ReadText(StructuralSectionCsvSchema.ManifestFile));
        }

        /// <summary>
        /// Applies the overlay. An unknown id and a duplicated id are BOTH errors: an overlay that silently
        /// referred to a section that no longer exists would look like it worked and disable nothing.
        /// </summary>
        public static IReadOnlyList<StructuralSectionDefinition> ApplyStatus(
            IReadOnlyList<StructuralSectionDefinition> sections,
            IReadOnlyList<StructuralSectionStatusOverride> overrides)
        {
            if (sections == null)
            {
                throw new ArgumentNullException(nameof(sections));
            }

            if (overrides == null || overrides.Count == 0)
            {
                return sections;
            }

            var byId = sections.ToDictionary(section => section.SectionId);
            var seen = new HashSet<StructuralSectionId>();
            var result = new Dictionary<StructuralSectionId, StructuralSectionDefinition>(byId);

            foreach (var entry in overrides)
            {
                if (!seen.Add(entry.SectionId))
                {
                    throw new StructuralSectionCsvException(
                        StructuralSectionCsvSchema.StatusFile, 0, StructuralSectionCsvSchema.SectionId,
                        entry.SectionId.Value, "el id aparece mas de una vez en el overlay de estado.");
                }

                if (!byId.TryGetValue(entry.SectionId, out var section))
                {
                    throw new StructuralSectionCsvException(
                        StructuralSectionCsvSchema.StatusFile, 0, StructuralSectionCsvSchema.SectionId,
                        entry.SectionId.Value, "el overlay de estado referencia un id que no existe en el catalogo.");
                }

                result[entry.SectionId] = new StructuralSectionDefinition
                {
                    Identity = section.Identity,
                    WeightPerLength = section.WeightPerLength,
                    NativeUnitSystem = section.NativeUnitSystem,
                    Dimensions = section.Dimensions,
                    Properties = section.Properties,
                    SourceSpecialNote = section.SourceSpecialNote,
                    MaterialGrade = section.MaterialGrade,
                    IsEnabled = entry.IsEnabled,
                    StatusNotes = entry.Notes
                };
            }

            return result.Values
                .OrderBy(section => section.SectionId.Value, StringComparer.Ordinal)
                .ToArray();
        }

        /// <summary>SHA-256 of a distributed file, upper-case hex — the same form the manifest declares.</summary>
        public string ComputeSha256(string fileName)
        {
            var path = Path.Combine(_directory, fileName);

            using (var sha = SHA256.Create())
            using (var stream = File.OpenRead(path))
            {
                return Convert.ToHexString(sha.ComputeHash(stream));
            }
        }

        private string ReadText(string fileName)
        {
            var path = Path.Combine(_directory, fileName);

            if (!File.Exists(path))
            {
                throw new StructuralSectionCsvException(
                    fileName, 0, null, null,
                    "el archivo del catalogo de secciones estructurales no existe en '" + _directory + "'.");
            }

            return Decode(File.ReadAllBytes(path));
        }

        /// <summary>
        /// UTF-8 first, Windows-1252 as fallback — the same tolerance ADR-0007 records for every catalog of the
        /// repo. The generated files are pure ASCII, so this only ever matters for the hand-edited status
        /// overlay, which someone may well save from Excel as ANSI.
        /// </summary>
        private static string Decode(byte[] bytes)
        {
            var offset = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF ? 3 : 0;

            try
            {
                var strictUtf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
                return strictUtf8.GetString(bytes, offset, bytes.Length - offset);
            }
            catch (DecoderFallbackException)
            {
                return Encoding.Latin1.GetString(bytes, offset, bytes.Length - offset);
            }
        }

        private string ComputeSignature()
        {
            try
            {
                if (!System.IO.Directory.Exists(_directory))
                {
                    return "<missing>";
                }

                var parts = new List<string>();

                foreach (var fileName in StructuralSectionCsvSchema.AllFiles()
                    .Concat(new[] { StructuralSectionCsvSchema.ManifestFile }))
                {
                    var info = new FileInfo(Path.Combine(_directory, fileName));
                    parts.Add(info.Exists
                        ? info.Name + "|" + info.Length + "|" + info.LastWriteTimeUtc.Ticks
                        : info.Name + "|<missing>");
                }

                parts.Sort(StringComparer.Ordinal);
                return string.Join(";", parts);
            }
            catch (Exception)
            {
                // Cannot stat the folder: return a unique token so this load is NOT cached.
                return Guid.NewGuid().ToString();
            }
        }
    }
}
