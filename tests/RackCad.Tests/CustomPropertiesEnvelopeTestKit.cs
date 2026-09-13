using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using RackCad.Application.Persistence;
using static RackCad.Tests.CustomPropertiesTestKit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-54 G4 — utilidades de las pruebas del sobre (Proposal V5 §12.2 y §12.5; ADR-0039 §4 y §6).
    ///
    /// <para>
    /// Los escapes que escribe <c>System.Text.Json</c> (la comilla como barra-u-0022, los surrogates escapados) se
    /// construyen por codigo numerico con <see cref="CustomPropertiesTestKit.JsonEscape"/>, igual que en G3: un escape de
    /// C# resuelto de mas cambiaria en silencio los bytes que la caracterizacion congela.
    /// </para>
    /// </summary>
    internal static class CustomPropertiesEnvelopeTestKit
    {
        internal const string Member = "CustomProperties";

        /// <summary>Una comilla dentro de un string, tal como la escribe el codificador por defecto.</summary>
        internal static readonly string Q = JsonEscape(0x0022);

        internal static RackEmbedDocument Read(string json) => new RackEmbedStore().Deserialize(json);

        internal static string Write(RackEmbedDocument envelope) => new RackEmbedStore().Serialize(envelope);

        /// <summary>
        /// Un sobre JSON con los siete miembros declarados de BASE, en su orden, seguido de <paramref name="tail"/> (miembros
        /// adicionales ya escritos, cada uno precedido de coma). <paramref name="version"/> va como literal JSON.
        /// </summary>
        internal static string Envelope(string tail = "", string version = "\"1.0\"", string kind = "selective")
            => "{\"SchemaVersion\":" + version + ",\"Kind\":\"" + kind + "\",\"View\":\"frontal\",\"Section\":-1,\"Id\":\""
               + IdA + "\",\"Name\":\"Rack A\",\"Design\":\"{}\"" + tail + "}";

        /// <summary>El mismo sobre sin <c>SchemaVersion</c>: la forma de un sobre legado.</summary>
        internal static string EnvelopeWithoutVersion(string tail = "")
            => "{\"Kind\":\"cama\",\"View\":null,\"Section\":-1,\"Id\":\"" + IdA + "\",\"Name\":\"Cama 1\",\"Design\":\"{}\"" + tail + "}";

        /// <summary><paramref name="levels"/> arrays anidados y vacios: su profundidad propia es <paramref name="levels"/>.</summary>
        internal static string NestedArrays(int levels) => new string('[', levels) + new string(']', levels);

        /// <summary>Un elemento JSON independiente de su documento (atajo del kit de G3).</summary>
        internal static JsonElement Json(string json) => Element(json);

        // ======================================================================== T-CHR-01: sobres sin el miembro

        /// <summary>Un caso de T-CHR-01: como se construye el sobre y los bytes que BASE escribe para el.</summary>
        internal sealed class EnvelopeCase
        {
            public EnvelopeCase(string kind, bool withExtensionData, Func<RackEmbedDocument> build, string expected)
            {
                Kind = kind;
                WithExtensionData = withExtensionData;
                Build = build;
                Expected = expected;
            }

            public string Kind { get; }

            public bool WithExtensionData { get; }

            public Func<RackEmbedDocument> Build { get; }

            public string Expected { get; }
        }

        /// <summary>
        /// Sobres representativos SIN <c>CustomProperties</c>, de cada kind, con y sin <c>ExtensionData</c>, y los bytes
        /// exactos que escribe el <c>RackEmbedStore</c> de BASE. T-CHR-01 los congela antes de tocar el sobre y T-ENV-01
        /// exige, despues del cambio, los mismos bytes.
        ///
        /// <para>
        /// Los casos sin <c>ExtensionData</c> se construyen en memoria, como hace el compositor. Los que la llevan se leen de
        /// un texto con campos desconocidos en distintas posiciones: BASE los escribe al final, en su orden, con el texto
        /// crudo de los numeros, y reescribe los escapes con su codificador (comillas como barra-u-0022).
        /// </para>
        /// </summary>
        internal static readonly IReadOnlyDictionary<string, EnvelopeCase> EnvelopesWithoutMember = BuildEnvelopeCases();

        private static IReadOnlyDictionary<string, EnvelopeCase> BuildEnvelopeCases()
        {
            var quote = Backslash + "\"";

            return new Dictionary<string, EnvelopeCase>(StringComparer.Ordinal)
            {
                ["selectivo-frontal"] = new EnvelopeCase(
                    RackEmbedDocument.KindSelective,
                    false,
                    () => new RackEmbedDocument
                    {
                        Kind = RackEmbedDocument.KindSelective,
                        View = RackEmbedDocument.ViewFrontal,
                        Section = -1,
                        Id = IdA,
                        Name = "Rack A",
                        Design = "{\"Name\":\"Rack A\"}"
                    },
                    "{\"SchemaVersion\":\"1.0\",\"Kind\":\"selective\",\"View\":\"frontal\",\"Section\":-1,\"Id\":\"" + IdA
                    + "\",\"Name\":\"Rack A\",\"Design\":\"{" + Q + "Name" + Q + ":" + Q + "Rack A" + Q + "}\"}"),

                ["selectivo-planta"] = new EnvelopeCase(
                    RackEmbedDocument.KindSelective,
                    false,
                    () => new RackEmbedDocument
                    {
                        Kind = RackEmbedDocument.KindSelective,
                        View = RackEmbedDocument.ViewPlanta,
                        Section = -1,
                        Id = IdA,
                        Name = "Rack A",
                        Design = "{}"
                    },
                    "{\"SchemaVersion\":\"1.0\",\"Kind\":\"selective\",\"View\":\"planta\",\"Section\":-1,\"Id\":\"" + IdA
                    + "\",\"Name\":\"Rack A\",\"Design\":\"{}\"}"),

                ["dinamico-lateral"] = new EnvelopeCase(
                    RackEmbedDocument.KindDynamic,
                    false,
                    () => new RackEmbedDocument
                    {
                        Kind = RackEmbedDocument.KindDynamic,
                        View = RackEmbedDocument.ViewLateral,
                        Section = 0,
                        Id = IdA,
                        Name = "Dinamico 1",
                        Design = "{\"Levels\":[1,2]}"
                    },
                    "{\"SchemaVersion\":\"1.0\",\"Kind\":\"dynamic\",\"View\":\"lateral\",\"Section\":0,\"Id\":\"" + IdA
                    + "\",\"Name\":\"Dinamico 1\",\"Design\":\"{" + Q + "Levels" + Q + ":[1,2]}\"}"),

                ["cabecera-lateral"] = new EnvelopeCase(
                    RackEmbedDocument.KindCabecera,
                    false,
                    () => new RackEmbedDocument
                    {
                        Kind = RackEmbedDocument.KindCabecera,
                        View = RackEmbedDocument.ViewLateral,
                        Section = -1,
                        Id = IdA,
                        Name = "Cabecera 1",
                        Design = "{}"
                    },
                    "{\"SchemaVersion\":\"1.0\",\"Kind\":\"cabecera\",\"View\":\"lateral\",\"Section\":-1,\"Id\":\"" + IdA
                    + "\",\"Name\":\"Cabecera 1\",\"Design\":\"{}\"}"),

                ["cama-sin-vista"] = new EnvelopeCase(
                    RackEmbedDocument.KindCama,
                    false,
                    () => new RackEmbedDocument
                    {
                        Kind = RackEmbedDocument.KindCama,
                        View = null,
                        Section = -1,
                        Id = IdA,
                        Name = "Cama 1",
                        Design = "{\"Bed\":1.5}"
                    },
                    "{\"SchemaVersion\":\"1.0\",\"Kind\":\"cama\",\"View\":null,\"Section\":-1,\"Id\":\"" + IdA
                    + "\",\"Name\":\"Cama 1\",\"Design\":\"{" + Q + "Bed" + Q + ":1.5}\"}"),

                ["pushback-lateral"] = new EnvelopeCase(
                    RackEmbedDocument.KindPushBack,
                    false,
                    () => new RackEmbedDocument
                    {
                        Kind = RackEmbedDocument.KindPushBack,
                        View = RackEmbedDocument.ViewLateral,
                        Section = 3,
                        Id = IdA,
                        Name = "Push Back 1",
                        Design = "{\"Lines\":2}"
                    },
                    "{\"SchemaVersion\":\"1.0\",\"Kind\":\"pushback\",\"View\":\"lateral\",\"Section\":3,\"Id\":\"" + IdA
                    + "\",\"Name\":\"Push Back 1\",\"Design\":\"{" + Q + "Lines" + Q + ":2}\"}"),

                ["cantilever-frontal"] = new EnvelopeCase(
                    RackEmbedDocument.KindCantilever,
                    false,
                    () => new RackEmbedDocument
                    {
                        Kind = RackEmbedDocument.KindCantilever,
                        View = RackEmbedDocument.ViewFrontal,
                        Section = -1,
                        Id = IdA,
                        Name = "Cantilever 1",
                        Design = "{\"Line\":{\"Id\":\"L1\"}}"
                    },
                    "{\"SchemaVersion\":\"1.0\",\"Kind\":\"cantilever\",\"View\":\"frontal\",\"Section\":-1,\"Id\":\"" + IdA
                    + "\",\"Name\":\"Cantilever 1\",\"Design\":\"{" + Q + "Line" + Q + ":{" + Q + "Id" + Q + ":" + Q + "L1" + Q
                    + "}}\"}"),

                ["selectivo-lateral-con-extension"] = new EnvelopeCase(
                    RackEmbedDocument.KindSelective,
                    true,
                    () => Read(
                        "{\"Futuro\":{\"n\":2.50,\"a\":[true,false,null]},\"SchemaVersion\":\"1.0\",\"Kind\":\"selective\","
                        + "\"View\":\"lateral\",\"Section\":2,\"Id\":\"" + IdA + "\",\"Name\":\"Rack A\",\"Design\":\"{"
                        + quote + "Name" + quote + ":" + quote + "Rack A" + quote + "}\"}"),
                    "{\"SchemaVersion\":\"1.0\",\"Kind\":\"selective\",\"View\":\"lateral\",\"Section\":2,\"Id\":\"" + IdA
                    + "\",\"Name\":\"Rack A\",\"Design\":\"{" + Q + "Name" + Q + ":" + Q + "Rack A" + Q + "}\","
                    + "\"Futuro\":{\"n\":2.50,\"a\":[true,false,null]}}"),

                ["dinamico-lateral-con-extension"] = new EnvelopeCase(
                    RackEmbedDocument.KindDynamic,
                    true,
                    () => Read(
                        "{\"SchemaVersion\":\"1.3\",\"Kind\":\"dynamic\",\"Otro\":\"x\",\"View\":\"lateral\",\"Section\":1,"
                        + "\"Id\":\"" + IdA + "\",\"Name\":\"Dinamico 1\",\"Design\":\"{}\",\"Mas\":[1,{\"q\":\"w\"}]}"),
                    "{\"SchemaVersion\":\"1.3\",\"Kind\":\"dynamic\",\"View\":\"lateral\",\"Section\":1,\"Id\":\"" + IdA
                    + "\",\"Name\":\"Dinamico 1\",\"Design\":\"{}\",\"Otro\":\"x\",\"Mas\":[1,{\"q\":\"w\"}]}"),

                ["cabecera-con-extension"] = new EnvelopeCase(
                    RackEmbedDocument.KindCabecera,
                    true,
                    () => Read(
                        "{\"SchemaVersion\":\"1.0\",\"Kind\":\"cabecera\",\"View\":\"lateral\",\"Section\":-1,\"Id\":\"" + IdA
                        + "\",\"Name\":\"Cabecera 1\",\"Design\":\"{}\",\"Extra\":-0}"),
                    "{\"SchemaVersion\":\"1.0\",\"Kind\":\"cabecera\",\"View\":\"lateral\",\"Section\":-1,\"Id\":\"" + IdA
                    + "\",\"Name\":\"Cabecera 1\",\"Design\":\"{}\",\"Extra\":-0}"),

                ["cama-con-extension"] = new EnvelopeCase(
                    RackEmbedDocument.KindCama,
                    true,
                    () => Read(
                        "{\"Numero\":1e3,\"SchemaVersion\":\"1.0\",\"Kind\":\"cama\",\"View\":null,\"Section\":-1,\"Id\":\"" + IdA
                        + "\",\"Name\":\"Cama 1\",\"Design\":\"{}\"}"),
                    "{\"SchemaVersion\":\"1.0\",\"Kind\":\"cama\",\"View\":null,\"Section\":-1,\"Id\":\"" + IdA
                    + "\",\"Name\":\"Cama 1\",\"Design\":\"{}\",\"Numero\":1e3}"),

                ["pushback-con-extension"] = new EnvelopeCase(
                    RackEmbedDocument.KindPushBack,
                    true,
                    () => Read(
                        "{\"SchemaVersion\":\"1.0\",\"Kind\":\"pushback\",\"View\":\"lateral\",\"Section\":3,\"Id\":\"" + IdA
                        + "\",\"Name\":\"Push Back 1\",\"Design\":\"{}\",\"Anidado\":{\"a\":{\"b\":{\"c\":[1,2,3]}}}}"),
                    "{\"SchemaVersion\":\"1.0\",\"Kind\":\"pushback\",\"View\":\"lateral\",\"Section\":3,\"Id\":\"" + IdA
                    + "\",\"Name\":\"Push Back 1\",\"Design\":\"{}\",\"Anidado\":{\"a\":{\"b\":{\"c\":[1,2,3]}}}}"),

                ["cantilever-con-extension"] = new EnvelopeCase(
                    RackEmbedDocument.KindCantilever,
                    true,
                    () => Read(
                        "{\"SchemaVersion\":\"1.0\",\"Kind\":\"cantilever\",\"View\":\"frontal\",\"Section\":-1,\"Id\":\"" + IdA
                        + "\",\"Name\":\"Cantilever 1\",\"Texto\":\"hola\",\"Design\":\"{}\",\"Nulo\":null}"),
                    "{\"SchemaVersion\":\"1.0\",\"Kind\":\"cantilever\",\"View\":\"frontal\",\"Section\":-1,\"Id\":\"" + IdA
                    + "\",\"Name\":\"Cantilever 1\",\"Design\":\"{}\",\"Texto\":\"hola\",\"Nulo\":null}"),
            };
        }

        // ======================================================================== El build anterior: forma de BASE

        /// <summary>
        /// El sobre con la forma EXACTA de BASE (<c>46fcac2</c>; <c>RackEmbedDocument.cs</c> es identico en <c>f8deb67</c>):
        /// los siete miembros declarados con sus valores por defecto y <c>ExtensionData</c>, sin <c>CustomProperties</c>.
        /// Representa a un build I-11..I-53, que no conoce el miembro (T-CHR-04, T-ENV-06 y T-ENV-10). Una prueba de T-CHR-01
        /// lo ata al sobre de produccion: si alguien lo desviara, dejaria de caracterizar a BASE.
        /// </summary>
        internal sealed class BaseShapedEnvelope
        {
            public string SchemaVersion { get; set; } = "1.0";

            public string Kind { get; set; }

            public string View { get; set; }

            public int Section { get; set; } = -1;

            public string Id { get; set; }

            public string Name { get; set; }

            public string Design { get; set; }

            [JsonExtensionData]
            public Dictionary<string, JsonElement> ExtensionData { get; set; }
        }

        /// <summary>
        /// El <c>RackEmbedStore</c> y el <c>RackEmbedComposer.Compose</c> de BASE sobre <see cref="BaseShapedEnvelope"/>: las
        /// mismas opciones del serializador, la misma captura de <see cref="JsonException"/>, la misma compuerta de version y
        /// un compositor que del origen solo hereda <c>SchemaVersion</c> y <c>ExtensionData</c> (P-04).
        /// </summary>
        internal static class BaseShaped
        {
            private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNameCaseInsensitive = true
            };

            internal static string Serialize(BaseShapedEnvelope envelope) => JsonSerializer.Serialize(envelope, Options);

            internal static BaseShapedEnvelope Deserialize(string json)
            {
                if (string.IsNullOrWhiteSpace(json))
                {
                    return null;
                }

                BaseShapedEnvelope envelope;
                try
                {
                    envelope = JsonSerializer.Deserialize<BaseShapedEnvelope>(json, Options);
                }
                catch (JsonException)
                {
                    return null;
                }

                if (envelope == null)
                {
                    return null;
                }

                return SchemaVersionPolicy.IsReadable(envelope.SchemaVersion, "1.0") ? envelope : null;
            }

            internal static BaseShapedEnvelope Compose(
                BaseShapedEnvelope source, string kind, string id, string name, string view, int section, string design)
                => new BaseShapedEnvelope
                {
                    SchemaVersion = SchemaVersionPolicy.ResolveWriteVersion(source?.SchemaVersion, "1.0"),
                    Kind = kind,
                    Id = id,
                    Name = name,
                    View = view,
                    Section = section,
                    Design = design,
                    ExtensionData = source?.ExtensionData
                };
        }

        /// <summary>Los miembros que serializa un tipo, con su tipo y su valor en una instancia nueva, sin el de extension.</summary>
        internal static IReadOnlyList<string> DeclaredShape(Type type, params string[] excluded)
        {
            var fresh = Activator.CreateInstance(type);

            return type.GetProperties()
                .Where(property => !property.IsDefined(typeof(JsonExtensionDataAttribute), false))
                .Where(property => !excluded.Contains(property.Name))
                .Select(property => property.Name + ":" + property.PropertyType.Name + "=" + (property.GetValue(fresh) ?? "null"))
                .ToList();
        }
    }
}
