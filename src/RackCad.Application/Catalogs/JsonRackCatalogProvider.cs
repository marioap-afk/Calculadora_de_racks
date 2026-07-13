using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace RackCad.Application.Catalogs
{
    /// <summary>
    /// Loads the rack catalog from a directory of versioned JSON files. Each file
    /// is an array of entries. Missing files yield empty lists rather than throwing,
    /// so a partial catalog folder still produces a usable <see cref="RackCatalog"/>.
    /// </summary>
    public sealed class JsonRackCatalogProvider : IRackCatalogProvider
    {
        /// <summary>Unified structural-profile catalog (posts + celosía + beams in one sheet, split by "rol").</summary>
        public const string SeccionesFile = "secciones.json";

        // Legacy split files, still read as a fallback when secciones.csv/json is absent (older deployed folders).
        public const string PostProfilesFile = "post-profiles.json";
        public const string TrussProfilesFile = "truss-profiles.json";
        public const string BasePlatesFile = "base-plates.json";
        public const string FlowBedProfilesFile = "flow-bed-profiles.json";
        public const string BeamProfilesFile = "beam-profiles.json";
        public const string MensulasFile = "mensulas.json";
        public const string SafetyElementsFile = "seguridad.json";
        public const string ConnectionPointsFile = "connection-points.json";
        public const string ConnectionLayoutFile = "connection-layout.json";
        public const string ViewsFile = "views.json";
        public const string BlocksFile = "blocks.json";
        public const string DefaultsFile = "defaults.json";

        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        private readonly string _directory;

        /// <summary>
        /// Per-directory cache keyed by a signature of every csv/json file (name + size + mtime). Loading the
        /// catalog several times within one command (or across the test suite) stops re-parsing the folder,
        /// while an Excel save changes the signature so the very next command picks the edit up — the
        /// "edit the CSV live, re-run the command" workflow stays intact. Cached instances are SHARED:
        /// consumers must treat a loaded catalog as read-only (they all do — builders only query it).
        /// </summary>
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, CacheEntry> Cache =
            new System.Collections.Concurrent.ConcurrentDictionary<string, CacheEntry>(StringComparer.OrdinalIgnoreCase);

        private sealed class CacheEntry
        {
            public string Signature;
            public RackCatalog Catalog;
        }

        public JsonRackCatalogProvider(string directory)
        {
            _directory = directory ?? throw new ArgumentNullException(nameof(directory));
        }

        /// <summary>
        /// Builds a provider pointing at the <c>catalogs</c> folder next to the executing assembly, which is
        /// how the AutoCAD plugin and the tests ship them. See <see cref="CatalogDirectory"/> for why this is
        /// resolved relative to the assembly rather than <see cref="AppContext.BaseDirectory"/>.
        /// </summary>
        public static JsonRackCatalogProvider FromBaseDirectory()
        {
            return new JsonRackCatalogProvider(CatalogDirectory.Resolve());
        }

        public RackCatalog Load()
        {
            var signature = ComputeSignature(_directory);
            var entry = Cache.GetOrAdd(_directory, _ => new CacheEntry());

            lock (entry)
            {
                if (entry.Catalog != null && entry.Signature == signature)
                {
                    return entry.Catalog;
                }

                var catalog = LoadUncached();
                entry.Signature = signature;
                entry.Catalog = catalog;
                return catalog;
            }
        }

        private RackCatalog LoadUncached()
        {
            // Unified profiles first (secciones.csv: one Excel sheet with a "rol" column); when it is absent,
            // fall back to the legacy split files so older deployed catalog folders keep working unchanged.
            var secciones = ReadArray<SeccionCatalogEntry>(SeccionesFile);
            var posts = new List<ProfileCatalogEntry>();
            var truss = new List<ProfileCatalogEntry>();
            var beams = new List<BeamProfileCatalogEntry>();
            var spacers = new List<ProfileCatalogEntry>();

            if (secciones.Count > 0)
            {
                SplitSecciones(secciones, posts, truss, beams, spacers);
            }
            else
            {
                posts = ReadArray<ProfileCatalogEntry>(PostProfilesFile);
                truss = ReadArray<ProfileCatalogEntry>(TrussProfilesFile);
                beams = ReadArray<BeamProfileCatalogEntry>(BeamProfilesFile);
            }

            return new RackCatalog
            {
                PostProfiles = posts,
                TrussProfiles = truss,
                BasePlates = ReadArray<BasePlateCatalogEntry>(BasePlatesFile),
                FlowBedProfiles = ReadArray<FlowBedComponentCatalogEntry>(FlowBedProfilesFile),
                BeamProfiles = beams,
                SpacerProfiles = spacers,
                Mensulas = ReadArray<MensulaCatalogEntry>(MensulasFile),
                SafetyElements = ReadArray<SafetyElementCatalogEntry>(SafetyElementsFile),
                ConnectionPoints = ReadArray<ConnectionPointCatalogEntry>(ConnectionPointsFile),
                ConnectionLayout = ReadArray<ConnectionLayoutEntry>(ConnectionLayoutFile),
                Views = ReadArray<ViewCatalogEntry>(ViewsFile),
                Blocks = ReadArray<BlockCatalogEntry>(BlocksFile),
                Defaults = ReadObject(DefaultsFile, new RackDefaults())
            };
        }

        /// <summary>Split the unified rows into the legacy typed lists by "rol", so every consumer of
        /// <see cref="RackCatalog"/> keeps its API. Unknown/empty roles are skipped (tolerant, like the rest
        /// of the catalog): a typo in Excel drops that row instead of breaking the load.</summary>
        private static void SplitSecciones(
            List<SeccionCatalogEntry> secciones,
            List<ProfileCatalogEntry> posts,
            List<ProfileCatalogEntry> truss,
            List<BeamProfileCatalogEntry> beams,
            List<ProfileCatalogEntry> spacers)
        {
            foreach (var row in secciones)
            {
                var rol = (row.Rol ?? string.Empty).Trim().ToUpperInvariant();

                switch (rol)
                {
                    case "POSTE":
                        posts.Add(ToProfile(row));
                        break;
                    case "CELOSIA":
                    case "CELOSÍA":
                        truss.Add(ToProfile(row));
                        break;
                    case "LARGUERO":
                        beams.Add(ToBeam(row));
                        break;
                    case "SEPARADOR":
                        spacers.Add(ToProfile(row));
                        break;
                }
            }
        }

        private static ProfileCatalogEntry ToProfile(SeccionCatalogEntry row)
        {
            var entry = new ProfileCatalogEntry
            {
                Family = row.Family,
                Width = row.Width,
                Depth = row.Depth,
                Thickness = row.Thickness,
                Units = row.Units,
                Gauge = row.Gauge,
                WeightPerMeter = row.WeightPerMeter
            };
            CopyBase(row, entry);
            return entry;
        }

        private static BeamProfileCatalogEntry ToBeam(SeccionCatalogEntry row)
        {
            var entry = new BeamProfileCatalogEntry
            {
                Family = row.Family,
                Peraltes = row.Peraltes,
                Width = row.Width,
                Thickness = row.Thickness,
                Units = row.Units,
                Gauge = row.Gauge,
                Mensula = row.Mensula,
                WeightPerMeter = row.WeightPerMeter
            };
            CopyBase(row, entry);
            return entry;
        }

        private static void CopyBase(CatalogEntryBase from, CatalogEntryBase to)
        {
            to.Id = from.Id;
            to.DisplayName = from.DisplayName;
            to.Description = from.Description;
            to.Material = from.Material;
            to.PartNumber = from.PartNumber;
            to.Manufacturer = from.Manufacturer;
            to.Finish = from.Finish;
            to.UnitCost = from.UnitCost;
            to.Currency = from.Currency;
            to.CostUnit = from.CostUnit;
            to.Properties = from.Properties; // extra Excel columns (Ix/Iy/norma/...) travel untouched
        }

        /// <summary>Signature of the catalog folder: every csv/json's name + size + last write time.</summary>
        private static string ComputeSignature(string directory)
        {
            try
            {
                if (!Directory.Exists(directory))
                {
                    return "<missing>";
                }

                var parts = new List<string>();
                foreach (var pattern in new[] { "*.csv", "*.json" })
                {
                    foreach (var file in Directory.EnumerateFiles(directory, pattern))
                    {
                        var info = new FileInfo(file);
                        parts.Add(info.Name + "|" + info.Length + "|" + info.LastWriteTimeUtc.Ticks);
                    }
                }

                parts.Sort(StringComparer.OrdinalIgnoreCase);
                return string.Join(";", parts);
            }
            catch (Exception)
            {
                // Unable to stat the folder: return a unique token so this load is NOT cached.
                return Guid.NewGuid().ToString();
            }
        }

        /// <summary>
        /// Reads a catalog CSV tolerating BOTH encodings Excel produces: UTF-8 (with or without BOM) and the
        /// legacy ANSI (Windows-1252/Latin-1) that plain "CSV" saves use. Reading an ANSI file as UTF-8 turns
        /// á/ñ into U+FFFD (the user sees "escal�n" in dropdowns), so we decode strictly as UTF-8 first and
        /// fall back to Latin-1 — which maps every Spanish accented letter identically to Windows-1252.
        /// </summary>
        private static string ReadCsvText(string path)
        {
            var bytes = File.ReadAllBytes(path);
            var offset = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF ? 3 : 0;

            try
            {
                var strictUtf8 = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
                return strictUtf8.GetString(bytes, offset, bytes.Length - offset);
            }
            catch (System.Text.DecoderFallbackException)
            {
                return System.Text.Encoding.Latin1.GetString(bytes, offset, bytes.Length - offset);
            }
        }

        private T ReadObject<T>(string fileName, T fallback)
        {
            var path = Path.Combine(_directory, fileName);

            if (!File.Exists(path))
            {
                return fallback;
            }

            var json = File.ReadAllText(path);

            if (string.IsNullOrWhiteSpace(json))
            {
                return fallback;
            }

            try
            {
                return JsonSerializer.Deserialize<T>(json, SerializerOptions) ?? fallback;
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    "El catalogo '" + fileName + "' no es JSON valido: " + ex.Message, ex);
            }
        }

        private List<T> ReadArray<T>(string fileName) where T : CatalogEntryBase, new()
        {
            // Excel-first: a sibling .csv (the format users edit in Excel) wins over the .json.
            var csvPath = Path.Combine(_directory, Path.ChangeExtension(fileName, ".csv"));

            if (File.Exists(csvPath))
            {
                var csv = ReadCsvText(csvPath);

                if (!string.IsNullOrWhiteSpace(csv))
                {
                    return CsvCatalogReader.Read<T>(csv);
                }
            }

            var path = Path.Combine(_directory, fileName);

            if (!File.Exists(path))
            {
                return new List<T>();
            }

            var json = File.ReadAllText(path);

            if (string.IsNullOrWhiteSpace(json))
            {
                return new List<T>();
            }

            try
            {
                return JsonSerializer.Deserialize<List<T>>(json, SerializerOptions) ?? new List<T>();
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException(
                    "El catalogo '" + fileName + "' no es JSON valido: " + ex.Message, ex);
            }
        }
    }
}
