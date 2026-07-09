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
        public const string PostProfilesFile = "post-profiles.json";
        public const string TrussProfilesFile = "truss-profiles.json";
        public const string BasePlatesFile = "base-plates.json";
        public const string FlowBedProfilesFile = "flow-bed-profiles.json";
        public const string BeamProfilesFile = "beam-profiles.json";
        public const string MensulasFile = "mensulas.json";
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
            return new RackCatalog
            {
                PostProfiles = ReadArray<ProfileCatalogEntry>(PostProfilesFile),
                TrussProfiles = ReadArray<ProfileCatalogEntry>(TrussProfilesFile),
                BasePlates = ReadArray<BasePlateCatalogEntry>(BasePlatesFile),
                FlowBedProfiles = ReadArray<FlowBedComponentCatalogEntry>(FlowBedProfilesFile),
                BeamProfiles = ReadArray<BeamProfileCatalogEntry>(BeamProfilesFile),
                Mensulas = ReadArray<MensulaCatalogEntry>(MensulasFile),
                ConnectionPoints = ReadArray<ConnectionPointCatalogEntry>(ConnectionPointsFile),
                ConnectionLayout = ReadArray<ConnectionLayoutEntry>(ConnectionLayoutFile),
                Views = ReadArray<ViewCatalogEntry>(ViewsFile),
                Blocks = ReadArray<BlockCatalogEntry>(BlocksFile),
                Defaults = ReadObject(DefaultsFile, new RackDefaults())
            };
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
