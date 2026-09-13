using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using RackCad.Application.CustomProperties;
using RackCad.Application.Persistence;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-54 G3 — utilidades de las pruebas del documento, del store y de las mutaciones puras de propiedades
    /// personalizadas (Proposal V5 §12.1 y §12.4; ADR-0039).
    ///
    /// <para>
    /// Los caracteres delicados —surrogates sueltos, no-caracteres, marcas combinantes— se construyen por CODIGO
    /// NUMERICO y nunca con escapes de C# de la forma barra-u. La diferencia importa: un surrogate ESCAPADO en el
    /// JSON y uno CRUDO en el texto fallan en sitios distintos del pipeline (al decodificar y al transcodificar), y
    /// un escape resuelto de mas convertiria silenciosamente un caso en el otro.
    /// </para>
    /// </summary>
    internal static class CustomPropertiesTestKit
    {
        internal const string IdA = "0b8e7f7a-3c1d-4c7e-9a52-6f1a2d7b9c10";
        internal const string IdB = "5d1c2a90-7b44-4f0e-8a3b-0c9e6d2f1a77";
        internal const string IdC = "9f4e3b2a-1c0d-4e5f-8a7b-6c5d4e3f2a1b";

        internal static readonly string Backslash = ((char)92).ToString();

        /// <summary>«Área» compuesta (NFC): U+00C1 seguido de «rea».</summary>
        internal static readonly string AreaNfc = Unit(0x00C1) + "rea";

        /// <summary>«Área» descompuesta (NFD): «A», U+0301 combinante y «rea».</summary>
        internal static readonly string AreaNfd = "A" + Unit(0x0301) + "rea";

        /// <summary>El escape JSON de UNA unidad UTF-16, construido sin escapes de C#.</summary>
        internal static string JsonEscape(int codeUnit)
            => Backslash + "u" + codeUnit.ToString("X4", CultureInfo.InvariantCulture);

        /// <summary>Una unidad UTF-16 cruda, que puede ser un surrogate suelto.</summary>
        internal static string Unit(int codeUnit) => ((char)codeUnit).ToString();

        /// <summary>Un valor escalar Unicode completo, con par de surrogates si hace falta.</summary>
        internal static string Scalar(int codePoint) => char.ConvertFromUtf32(codePoint);

        /// <summary>Un string bien formado como literal JSON, escapado por el serializador de la plataforma.</summary>
        internal static string JsonString(string value) => JsonSerializer.Serialize(value);

        /// <summary>Una entrada con nombre y valor ya escritos como literales JSON (para contenido invalido).</summary>
        internal static string EntryRaw(string id, string nameLiteral, string valueLiteral, string extra = "")
            => "{\"Id\":\"" + id + "\",\"Name\":" + nameLiteral + ",\"Value\":" + valueLiteral + extra + "}";

        /// <summary>Una entrada con nombre y valor bien formados.</summary>
        internal static string Entry(string id, string name, string value, string extra = "")
            => EntryRaw(id, JsonString(name), JsonString(value), extra);

        internal static string Entries(params string[] entries) => "[" + string.Join(",", entries) + "]";

        internal static string Doc(string version, string entries, string extra = "")
            => "{\"SchemaVersion\":\"" + version + "\",\"Entries\":" + entries + extra + "}";

        /// <summary>Anida <paramref name="levels"/> objetos alrededor de un numero: su profundidad propia es <paramref name="levels"/>.</summary>
        internal static string Nested(int levels)
        {
            var builder = new StringBuilder();

            for (var i = 0; i < levels; i++)
            {
                builder.Append("{\"x\":");
            }

            builder.Append('1');
            builder.Append('}', levels);
            return builder.ToString();
        }

        /// <summary>Contenedor de Proyecto: el texto que el NOD entregaria.</summary>
        internal static CustomPropertiesReadResult ReadText(string text)
            => new CustomPropertiesStore().Read(CustomPropertiesPayload.Present(text));

        /// <summary>Contenedor de Rack: el mismo JSON como miembro ya parseado del sobre.</summary>
        internal static CustomPropertiesReadResult ReadAsElement(string json)
            => new CustomPropertiesStore().ReadElement(Element(json));

        /// <summary>Lee por los dos contenedores y exige el mismo desenlace: la clasificacion es una sola.</summary>
        internal static CustomPropertiesReadResult ReadBoth(string json)
        {
            var text = ReadText(json);
            var element = ReadAsElement(json);

            Assert.Equal(text.Outcome, element.Outcome);
            return text;
        }

        internal static CustomPropertiesReadResult ReadableResult(string json)
        {
            var result = ReadText(json);

            Assert.Equal(CustomPropertiesReadOutcome.Readable, result.Outcome);
            return result;
        }

        internal static CustomPropertiesDocument Readable(string json) => ReadableResult(json).Document;

        internal static string Serialize(CustomPropertiesDocument document) => new CustomPropertiesStore().Serialize(document);

        internal static CustomPropertiesMutationResult Apply(CustomPropertiesReadResult current, CustomPropertiesIntent intent)
            => CustomPropertiesMutations.Apply(current, intent);

        internal static CustomPropertiesReadResult AbsentResult()
            => new CustomPropertiesStore().Read(CustomPropertiesPayload.Absent());

        internal static CustomPropertyId Id(string text)
        {
            Assert.True(CustomPropertyId.TryParse(text, out var id), text);
            return id;
        }

        /// <summary>Un elemento JSON independiente de su documento de origen.</summary>
        internal static JsonElement Element(string json)
        {
            using (var document = JsonDocument.Parse(json))
            {
                return document.RootElement.Clone();
            }
        }

        /// <summary>Profundidad de contenedores: el objeto o array raiz cuenta 1 y cada contenedor anidado suma 1.</summary>
        internal static int Depth(JsonElement element)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    return 1 + element.EnumerateObject().Select(property => Depth(property.Value)).DefaultIfEmpty(0).Max();

                case JsonValueKind.Array:
                    return 1 + element.EnumerateArray().Select(Depth).DefaultIfEmpty(0).Max();

                default:
                    return 0;
            }
        }

        internal static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);

            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.NotNull(dir);
            return dir;
        }

        /// <summary>Los archivos de produccion de propiedades personalizadas: el store y el nucleo puro.</summary>
        internal static IReadOnlyList<string> CustomPropertiesSources()
        {
            var application = Path.Combine(RepoRoot().FullName, "src", "RackCad.Application");

            return Directory.GetFiles(Path.Combine(application, "Persistence"), "CustomPropert*.cs")
                .Concat(Directory.GetFiles(Path.Combine(application, "CustomProperties"), "*.cs"))
                .OrderBy(path => path, StringComparer.Ordinal)
                .ToList();
        }

        /// <summary>El CODIGO de un archivo, sin las lineas de comentario.</summary>
        internal static string CodeOnly(string path)
            => string.Join(
                "\n",
                File.ReadAllLines(path).Where(line => !line.TrimStart().StartsWith("//", StringComparison.Ordinal)));
    }
}
