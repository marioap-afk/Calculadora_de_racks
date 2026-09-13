using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using RackCad.Application.CustomProperties;
using RackCad.Application.Persistence;
using Xunit;
using static RackCad.Tests.CustomPropertiesTestKit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-54 G3 — STORE de propiedades personalizadas: T-STO-01..20 de la Proposal V5 §12.1, bajo ADR-0039
    /// (aceptado) §1, §2, §3, §4 (solo la interpretacion del miembro, no el sobre), §5 (restriccion G) y §10.
    ///
    /// <para>
    /// La regla que atraviesa todo el archivo es <b>UNKNOWN != EMPTY</b>: ausente es una coleccion vacia y
    /// escribible; ilegible, ambiguo, de un major futuro o por encima de la cota de profundidad es SOLO LECTURA, con
    /// los bytes intactos y sin ninguna salida destructiva. Y el store nunca lanza por contenido externo: toda
    /// entrada arbitraria termina en un resultado tipado.
    /// </para>
    /// <para>
    /// Cada caso de clasificacion se lee por los DOS contenedores —el texto del NOD de Proyecto y el miembro ya
    /// parseado del sobre de Rack— salvo cuando el contenido no se puede parsear como elemento, porque entonces
    /// nunca llegaria al store por ese camino.
    /// </para>
    /// </summary>
    public class CustomPropertiesStoreTests
    {
        // ================================================================ asserts compartidos

        private static void AssertAbsentVacio(CustomPropertiesReadResult result)
        {
            Assert.Equal(CustomPropertiesReadOutcome.Absent, result.Outcome);
            Assert.True(result.CanWrite);
            Assert.Null(result.Error);
            Assert.NotNull(result.Document);
            Assert.Equal(CustomPropertiesDocument.CurrentSchemaVersion, result.Document.SchemaVersion);
            Assert.NotNull(result.Document.Entries);
            Assert.Empty(result.Document.Entries);
            Assert.Null(result.Document.ExtensionData);
            Assert.Empty(result.RepeatedNameEntryIds);
            Assert.True(CustomPropertiesWriteGuard.CanOverwrite(result, out var error));
            Assert.Null(error);
        }

        private static void AssertIlegible(CustomPropertiesReadResult result)
            => AssertSoloLectura(result, CustomPropertiesReadOutcome.PresentButUnreadable);

        private static void AssertSoloLectura(CustomPropertiesReadResult result, CustomPropertiesReadOutcome expected)
        {
            Assert.Equal(expected, result.Outcome);
            Assert.False(result.CanWrite);
            Assert.Null(result.Document);
            Assert.Empty(result.RepeatedNameEntryIds);
            Assert.False(string.IsNullOrWhiteSpace(result.Error));
            Assert.False(CustomPropertiesWriteGuard.CanOverwrite(result, out var error));
            Assert.False(string.IsNullOrWhiteSpace(error));
        }

        private static IEnumerable<CustomPropertiesIntent> TodasLasMutaciones()
        {
            yield return CustomPropertiesIntent.Create("Obra", "Nave 3");
            yield return CustomPropertiesIntent.Rename(Id(IdA), "Obra");
            yield return CustomPropertiesIntent.ChangeValue(Id(IdA), "Nave 3");
            yield return CustomPropertiesIntent.Delete(Id(IdA));
        }

        private static void AssertNingunaMutacion(CustomPropertiesReadResult result)
        {
            foreach (var intent in TodasLasMutaciones())
            {
                var mutation = Apply(result, intent);

                Assert.False(mutation.Succeeded);
                Assert.Equal(CustomPropertiesRejection.NotWritable, mutation.Rejection);
                Assert.Null(mutation.Document);
                Assert.False(string.IsNullOrWhiteSpace(mutation.Error));
            }
        }

        private static bool TryParseElement(string json, out JsonElement element)
        {
            try
            {
                element = Element(json);
                return true;
            }
            catch (JsonException)
            {
                element = default;
                return false;
            }
            catch (ArgumentException)
            {
                // Texto con UTF-16 crudo invalido: nunca llegaria al store como miembro del sobre (F-14a).
                element = default;
                return false;
            }
        }

        // ================================================================ T-STO-01 ausente

        [Fact]
        public void TSto01_ProyectoSinEntradaEnElNod_EsAbsent_VacioYEscribible()
        {
            AssertAbsentVacio(new CustomPropertiesStore().Read(CustomPropertiesPayload.Absent()));
        }

        [Fact]
        public void TSto01_RackSinMiembro_EsAbsent_VacioYEscribible()
        {
            AssertAbsentVacio(new CustomPropertiesStore().ReadElement(null));
        }

        /// <summary>«No me lo dijeron» no es evidencia de ausencia: tratarlo como vacio borraria la coleccion al escribir.</summary>
        [Fact]
        public void TSto01_SinPayload_NoSeConfundeConAusente_EsIlegible()
        {
            AssertIlegible(new CustomPropertiesStore().Read(null));
        }

        [Fact]
        public void TSto01_EntradaFisicamenteIlegible_EsPresentButUnreadable_ConSuMotivo()
        {
            var result = new CustomPropertiesStore().Read(CustomPropertiesPayload.Unreadable("La entrada no es un Xrecord."));

            AssertIlegible(result);
            Assert.Contains("La entrada no es un Xrecord.", result.Error, StringComparison.Ordinal);
        }

        // ================================================================ T-STO-02 vacio

        [Fact]
        public void TSto02_DocumentoVacio_EsReadableConCeroEntradas()
        {
            var result = ReadBoth(Doc("1.0", "[]"));

            Assert.Equal(CustomPropertiesReadOutcome.Readable, result.Outcome);
            Assert.True(result.CanWrite);
            Assert.Null(result.Error);
            Assert.Equal("1.0", result.Document.SchemaVersion);
            Assert.Empty(result.Document.Entries);
            Assert.Null(result.Document.ExtensionData);
            Assert.Empty(result.RepeatedNameEntryIds);
        }

        // ================================================================ T-STO-03 version sin la forma exacta

        [Theory]
        [InlineData("{\"Entries\":[]}")]
        [InlineData("{\"SchemaVersion\":\"\",\"Entries\":[]}")]
        [InlineData("{\"SchemaVersion\":\"   \",\"Entries\":[]}")]
        [InlineData("{\"SchemaVersion\":\"1\",\"Entries\":[]}")]
        [InlineData("{\"SchemaVersion\":\"1.0.0\",\"Entries\":[]}")]
        [InlineData("{\"SchemaVersion\":\" 1.0\",\"Entries\":[]}")]
        [InlineData("{\"SchemaVersion\":\"1.0 \",\"Entries\":[]}")]
        [InlineData("{\"SchemaVersion\":\"uno.cero\",\"Entries\":[]}")]
        [InlineData("{\"SchemaVersion\":\"1.x\",\"Entries\":[]}")]
        [InlineData("{\"SchemaVersion\":\"x.0\",\"Entries\":[]}")]
        [InlineData("{\"SchemaVersion\":\"0.9\",\"Entries\":[]}")]
        [InlineData("{\"SchemaVersion\":\"-1.0\",\"Entries\":[]}")]
        [InlineData("{\"SchemaVersion\":\"+1.0\",\"Entries\":[]}")]
        [InlineData("{\"SchemaVersion\":\"1.\",\"Entries\":[]}")]
        [InlineData("{\"SchemaVersion\":\".0\",\"Entries\":[]}")]
        [InlineData("{\"SchemaVersion\":\"1,0\",\"Entries\":[]}")]
        public void TSto03_VersionAusenteEnBlancoONoConLaFormaExacta_EsPresentButUnreadable(string json)
        {
            AssertIlegible(ReadBoth(json));
        }

        // ================================================================ T-STO-04 forma estructural

        [Theory]
        [InlineData("[]")]
        [InlineData("\"texto\"")]
        [InlineData("42")]
        [InlineData("true")]
        [InlineData("{\"SchemaVersion\":\"1.0\"}")]
        [InlineData("{\"SchemaVersion\":\"1.0\",\"Entries\":null}")]
        [InlineData("{\"SchemaVersion\":\"1.0\",\"Entries\":{}}")]
        [InlineData("{\"SchemaVersion\":\"1.0\",\"Entries\":\"[]\"}")]
        [InlineData("{\"SchemaVersion\":\"1.0\",\"Entries\":[1]}")]
        [InlineData("{\"SchemaVersion\":\"1.0\",\"Entries\":[\"x\"]}")]
        [InlineData("{\"SchemaVersion\":\"1.0\",\"Entries\":[null]}")]
        [InlineData("{\"SchemaVersion\":\"1.0\",\"Entries\":[[]]}")]
        [InlineData("{\"SchemaVersion\":1.0,\"Entries\":[]}")]
        [InlineData("{\"SchemaVersion\":true,\"Entries\":[]}")]
        [InlineData("{\"SchemaVersion\":{\"v\":\"1.0\"},\"Entries\":[]}")]
        [InlineData("{\"SchemaVersion\":null,\"Entries\":[]}")]
        public void TSto04_FormaEstructuralInvalida_EsPresentButUnreadable(string json)
        {
            AssertIlegible(ReadBoth(json));
        }

        [Theory]
        [InlineData("sin-id")]
        [InlineData("sin-name")]
        [InlineData("sin-value")]
        [InlineData("id-numerico")]
        [InlineData("name-numerico")]
        [InlineData("value-numerico")]
        [InlineData("value-objeto")]
        [InlineData("name-vacio")]
        [InlineData("name-en-blanco")]
        public void TSto04_EntradaSinLaFormaDeD01_EsPresentButUnreadable(string caso)
        {
            string entry;

            switch (caso)
            {
                case "sin-id": entry = "{\"Name\":\"Cliente\",\"Value\":\"ACME\"}"; break;
                case "sin-name": entry = "{\"Id\":\"" + IdA + "\",\"Value\":\"ACME\"}"; break;
                case "sin-value": entry = "{\"Id\":\"" + IdA + "\",\"Name\":\"Cliente\"}"; break;
                case "id-numerico": entry = "{\"Id\":42,\"Name\":\"Cliente\",\"Value\":\"ACME\"}"; break;
                case "name-numerico": entry = EntryRaw(IdA, "7", "\"ACME\""); break;
                case "value-numerico": entry = EntryRaw(IdA, "\"Cliente\"", "3.5"); break;
                case "value-objeto": entry = EntryRaw(IdA, "\"Cliente\"", "{\"v\":1}"); break;
                case "name-vacio": entry = Entry(IdA, "", "ACME"); break;
                case "name-en-blanco": entry = Entry(IdA, "   ", "ACME"); break;
                default: throw new ArgumentOutOfRangeException(nameof(caso));
            }

            AssertIlegible(ReadBoth(Doc("1.0", Entries(entry))));
        }

        // ================================================================ T-STO-05 major futuro

        [Theory]
        [InlineData("{\"SchemaVersion\":\"2.0\",\"Entries\":[{\"Id\":\"no-es-guid\",\"Name\":7,\"Value\":3.5}]}")]
        [InlineData("{\"SchemaVersion\":\"2.0\",\"Entries\":{\"a\":1},\"Tipos\":[\"Numero\"]}")]
        [InlineData("{\"SchemaVersion\":\"10.3\"}")]
        public void TSto05_MajorFuturo_EsIncompatibleMajor_SinExcepcionYSinEnlazarEntradas(string json)
        {
            AssertSoloLectura(ReadBoth(json), CustomPropertiesReadOutcome.IncompatibleMajor);
        }

        // ================================================================ T-STO-06 minor futuro

        [Fact]
        public void TSto06_MinorFuturoConCamposDesconocidos_EsReadable_YReescribirConserva17()
        {
            var json = Doc(
                "1.7",
                Entries(Entry(IdA, "Cliente", "ACME", ",\"Moneda\":\"MXN\"")),
                ",\"Plantilla\":{\"Id\":\"p1\"}");

            var result = ReadBoth(json);

            Assert.Equal(CustomPropertiesReadOutcome.Readable, result.Outcome);
            Assert.True(result.CanWrite);
            Assert.Equal("1.7", result.Document.SchemaVersion);

            var reread = ReadableResult(Serialize(result.Document));

            Assert.Equal("1.7", reread.Document.SchemaVersion);
        }

        // ================================================================ T-STO-07 campos desconocidos

        [Fact]
        public void TSto07_CamposDesconocidosDeRaizYDeEntrada_SobrevivenExactosAReescribir()
        {
            const string raiz = "{\"a\":[1,2.50,{\"b\":\"c\"}],\"n\":1e3,\"t\":true,\"z\":null}";

            var json = Doc(
                "1.0",
                Entries(Entry(IdA, "Cliente", "ACME", ",\"Extra\":\"texto\",\"Num\":1.50")),
                ",\"Raiz\":" + raiz + ",\"Otro\":-0.0");

            var original = Readable(json);

            Assert.Equal(new[] { "Raiz", "Otro" }, original.ExtensionData.Keys);
            Assert.Equal(new[] { "Extra", "Num" }, original.Entries[0].ExtensionData.Keys);

            var serialized = Serialize(original);
            var reread = Readable(serialized);

            Assert.Contains("\"Raiz\":" + raiz, serialized, StringComparison.Ordinal);
            Assert.Equal(new[] { "Raiz", "Otro" }, reread.ExtensionData.Keys);
            Assert.Equal(raiz, reread.ExtensionData["Raiz"].GetRawText());
            Assert.Equal("-0.0", reread.ExtensionData["Otro"].GetRawText());
            Assert.Equal(new[] { "Extra", "Num" }, reread.Entries[0].ExtensionData.Keys);
            Assert.Equal("\"texto\"", reread.Entries[0].ExtensionData["Extra"].GetRawText());
            Assert.Equal("1.50", reread.Entries[0].ExtensionData["Num"].GetRawText());
        }

        // ================================================================ T-STO-08 contenido externo

        private static readonly IReadOnlyDictionary<string, Func<string>> ExternalCases =
            new Dictionary<string, Func<string>>
            {
                ["malformado"] = () => "{",
                ["truncado"] = () => "{\"SchemaVersion\":\"1.0\",\"Entries\":[{\"Id\":\"" + IdA,
                ["no-json"] = () => "hola",
                ["vacio"] = () => string.Empty,
                ["blancos"] = () => "   ",
                ["comentario"] = () => "{\"SchemaVersion\":\"1.0\",/*x*/\"Entries\":[]}",
                ["coma-final"] = () => "{\"SchemaVersion\":\"1.0\",\"Entries\":[],}",
                ["profundidad-65"] = () => Doc("1.0", "[]", ",\"x\":" + Nested(64)),
                ["surrogate-crudo-en-name"] = () => Doc("1.0", Entries(EntryRaw(IdA, "\"a" + Unit(0xD800) + "\"", "\"v\""))),
                ["surrogate-crudo-en-value"] = () => Doc("1.0", Entries(EntryRaw(IdA, "\"n\"", "\"v" + Unit(0xDC00) + "\""))),
                ["surrogate-escapado-en-nombre-de-miembro"] = () => Doc("1.0", "[]", ",\"X" + JsonEscape(0xD800) + "\":1"),
                ["surrogate-escapado-en-name"] = () => Doc("1.0", Entries(EntryRaw(IdA, "\"a" + JsonEscape(0xD800) + "\"", "\"v\""))),
                ["surrogate-escapado-en-value"] = () => Doc("1.0", Entries(EntryRaw(IdA, "\"n\"", "\"v" + JsonEscape(0xDC00) + "\""))),
                ["surrogate-escapado-anidado-en-extensiondata"] = () => Doc("1.0", "[]", ",\"Raiz\":{\"a\":[\"x" + JsonEscape(0xDBFF) + "\"]}"),
                ["surrogate-escapado-en-extension-de-entrada"] = () => Doc("1.0", Entries(Entry(IdA, "n", "v", ",\"E\":\"" + JsonEscape(0xDFFF) + "\""))),
                ["surrogate-escapado-en-schemaversion"] = () => "{\"SchemaVersion\":\"1.0" + JsonEscape(0xD800) + "\",\"Entries\":[]}",
                ["surrogate-escapado-en-id"] = () => Doc("1.0", Entries("{\"Id\":\"" + IdA + JsonEscape(0xD800) + "\",\"Name\":\"n\",\"Value\":\"v\"}")),
                ["name-con-fffe-escapado"] = () => Doc("1.0", Entries(EntryRaw(IdA, "\"a" + JsonEscape(0xFFFE) + "\"", "\"v\""))),
                ["name-con-fdd0-escapado"] = () => Doc("1.0", Entries(EntryRaw(IdA, "\"a" + JsonEscape(0xFDD0) + "\"", "\"v\""))),
                ["name-con-1fffe-escapado"] = () => Doc("1.0", Entries(EntryRaw(IdA, "\"a" + JsonEscape(0xD83F) + JsonEscape(0xDFFE) + "\"", "\"v\""))),
                ["name-con-10ffff-escapado"] = () => Doc("1.0", Entries(EntryRaw(IdA, "\"a" + JsonEscape(0xDBFF) + JsonEscape(0xDFFF) + "\"", "\"v\""))),
                ["name-con-ffff-crudo"] = () => Doc("1.0", Entries(EntryRaw(IdA, "\"a" + Unit(0xFFFF) + "\"", "\"v\""))),
            };

        [Theory]
        [InlineData("malformado")]
        [InlineData("truncado")]
        [InlineData("no-json")]
        [InlineData("vacio")]
        [InlineData("blancos")]
        [InlineData("comentario")]
        [InlineData("coma-final")]
        [InlineData("profundidad-65")]
        [InlineData("surrogate-crudo-en-name")]
        [InlineData("surrogate-crudo-en-value")]
        [InlineData("surrogate-escapado-en-nombre-de-miembro")]
        [InlineData("surrogate-escapado-en-name")]
        [InlineData("surrogate-escapado-en-value")]
        [InlineData("surrogate-escapado-anidado-en-extensiondata")]
        [InlineData("surrogate-escapado-en-extension-de-entrada")]
        [InlineData("surrogate-escapado-en-schemaversion")]
        [InlineData("surrogate-escapado-en-id")]
        [InlineData("name-con-fffe-escapado")]
        [InlineData("name-con-fdd0-escapado")]
        [InlineData("name-con-1fffe-escapado")]
        [InlineData("name-con-10ffff-escapado")]
        [InlineData("name-con-ffff-crudo")]
        public void TSto08_ContenidoExternoInvalido_EsPresentButUnreadable_NuncaUnaExcepcion(string caso)
        {
            var json = ExternalCases[caso]();

            AssertIlegible(ReadText(json));

            if (TryParseElement(json, out var element))
            {
                AssertIlegible(new CustomPropertiesStore().ReadElement(element));
            }
        }

        /// <summary>Un <c>ArgumentException</c> de <c>Parse</c> solo puede venir del contenido si las opciones son validas y constantes.</summary>
        [Fact]
        public void TSto08_LasOpcionesDeParseSonConstantesYValidas()
        {
            var options = CustomPropertiesStore.ParseOptions;

            Assert.False(options.AllowTrailingCommas);
            Assert.Equal(JsonCommentHandling.Disallow, options.CommentHandling);
            Assert.Equal(64, options.MaxDepth);

            using (var document = JsonDocument.Parse(Doc("1.0", "[]"), options))
            {
                Assert.Equal(JsonValueKind.Object, document.RootElement.ValueKind);
            }
        }

        /// <summary>Guarda secundaria: el nucleo no captura <c>Exception</c>; solo las tres clases que el contenido provoca.</summary>
        [Fact]
        public void TSto08_ElNucleoNoCapturaExceptionGenerica_SoloLasClasesDelContenido()
        {
            var sources = CustomPropertiesSources();
            var allowed = new HashSet<string>(StringComparer.Ordinal)
            {
                "JsonException",
                "ArgumentException",
                "InvalidOperationException",
            };

            Assert.True(sources.Count >= 5, "El barrido apenas encontro archivos de propiedades personalizadas.");

            foreach (var path in sources)
            {
                var code = CodeOnly(path);

                Assert.DoesNotMatch(new Regex(@"catch\s*\{"), code);
                Assert.DoesNotMatch(new Regex(@"catch\s*\(\s*(System\.)?Exception\b"), code);

                foreach (Match match in Regex.Matches(code, @"catch\s*\(\s*([\w\.]+)"))
                {
                    Assert.Contains(match.Groups[1].Value, allowed);
                }
            }
        }

        // ================================================================ T-STO-09 profundidad

        [Fact]
        public void TSto09_Profundidad16_EsReadableYEscribible()
        {
            var json = Doc("1.0", Entries(Entry(IdA, "Cliente", "ACME")), ",\"Profundo\":" + Nested(15));

            Assert.Equal(16, Depth(Element(json)));

            var result = ReadBoth(json);

            Assert.Equal(CustomPropertiesReadOutcome.Readable, result.Outcome);

            var changed = Apply(result, CustomPropertiesIntent.ChangeValue(Id(IdA), "Otro"));

            Assert.True(changed.Succeeded);
            Assert.Equal(CustomPropertiesReadOutcome.Readable, ReadText(Serialize(changed.Document)).Outcome);
        }

        [Theory]
        [InlineData(16)]
        [InlineData(19)]
        [InlineData(39)]
        public void TSto09_MasDe16Niveles_EsDepthLimitExceeded_NoIlegible(int anidados)
        {
            var json = Doc("1.0", Entries(Entry(IdA, "Cliente", "ACME")), ",\"Profundo\":" + Nested(anidados));

            Assert.Equal(anidados + 1, Depth(Element(json)));
            AssertSoloLectura(ReadBoth(json), CustomPropertiesReadOutcome.DepthLimitExceeded);
        }

        [Fact]
        public void TSto09_LaCotaCuentaTambienDentroDeUnaEntrada()
        {
            var dieciseis = Doc("1.0", Entries(Entry(IdA, "Cliente", "ACME", ",\"Extra\":" + Nested(13))));
            var diecisiete = Doc("1.0", Entries(Entry(IdA, "Cliente", "ACME", ",\"Extra\":" + Nested(14))));

            Assert.Equal(16, Depth(Element(dieciseis)));
            Assert.Equal(17, Depth(Element(diecisiete)));
            Assert.Equal(CustomPropertiesReadOutcome.Readable, ReadBoth(dieciseis).Outcome);
            AssertSoloLectura(ReadBoth(diecisiete), CustomPropertiesReadOutcome.DepthLimitExceeded);
        }

        [Fact]
        public void TSto09_DepthLimitExceeded_RechazaCrearRenombrarCambiarEliminarYVaciar()
        {
            var result = ReadText(Doc("1.0", Entries(Entry(IdA, "Cliente", "ACME")), ",\"Profundo\":" + Nested(19)));

            Assert.Equal(CustomPropertiesReadOutcome.DepthLimitExceeded, result.Outcome);

            // La coleccion tiene una sola entrada: eliminarla es vaciar.
            AssertNingunaMutacion(result);
        }

        [Fact]
        public void TSto09_UnMinorFuturoProfundo_SigueSiendoDepthLimitExceeded()
        {
            var json = Doc("1.7", Entries(Entry(IdA, "Cliente", "ACME")), ",\"Profundo\":" + Nested(19));

            AssertSoloLectura(ReadBoth(json), CustomPropertiesReadOutcome.DepthLimitExceeded);
        }

        [Fact]
        public void TSto09_UnMajor2Profundo_EsIncompatibleMajor_PorqueLaCotaEsDelMajor1()
        {
            var json = Doc("2.0", Entries(Entry(IdA, "Cliente", "ACME")), ",\"Profundo\":" + Nested(19));

            AssertSoloLectura(ReadBoth(json), CustomPropertiesReadOutcome.IncompatibleMajor);
        }

        [Fact]
        public void TSto09_UnaEstructuraInvalidaGanaALaProfundidad()
        {
            var json = Doc("1.0", Entries(EntryRaw(IdA, "null", "\"ACME\"")), ",\"Profundo\":" + Nested(19));

            AssertIlegible(ReadBoth(json));
        }

        [Fact]
        public void TSto09_LoQueEscribeV1TieneProfundidad3()
        {
            var created = Apply(AbsentResult(), CustomPropertiesIntent.Create("Cliente", "ACME"));

            Assert.True(created.Succeeded);
            Assert.Equal(3, Depth(Element(Serialize(created.Document))));
        }

        [Fact]
        public void TSto09_ElEscritorNuncaEmiteUnDocumentoDeMasDe16Niveles()
        {
            var permitido = CustomPropertiesDocument.CreateNew();
            permitido.ExtensionData = new Dictionary<string, JsonElement> { ["Profundo"] = Element(Nested(15)) };

            Assert.Equal(16, Depth(Element(Serialize(permitido))));

            var excedido = CustomPropertiesDocument.CreateNew();
            excedido.ExtensionData = new Dictionary<string, JsonElement> { ["Profundo"] = Element(Nested(16)) };

            Assert.Throws<InvalidOperationException>(() => Serialize(excedido));
        }

        // ================================================================ T-STO-10 nulos

        [Fact]
        public void TSto10_MiembroNuloOElementoNull_EsAbsent()
        {
            AssertAbsentVacio(new CustomPropertiesStore().ReadElement(null));
            AssertAbsentVacio(new CustomPropertiesStore().ReadElement(Element("null")));
        }

        [Theory]
        [InlineData("value")]
        [InlineData("name")]
        [InlineData("id")]
        public void TSto10_ValueNameOIdNulos_SonPresentButUnreadable(string campo)
        {
            string entry;

            switch (campo)
            {
                case "value": entry = EntryRaw(IdA, "\"Cliente\"", "null"); break;
                case "name": entry = EntryRaw(IdA, "null", "\"ACME\""); break;
                case "id": entry = "{\"Id\":null,\"Name\":\"Cliente\",\"Value\":\"ACME\"}"; break;
                default: throw new ArgumentOutOfRangeException(nameof(campo));
            }

            AssertIlegible(ReadBoth(Doc("1.0", Entries(entry))));
        }

        /// <summary>En el NOD el texto «null» es una entrada PRESENTE cuyo contenido no es un objeto: no es ausencia.</summary>
        [Fact]
        public void TSto10_ElTextoNullEnElProyecto_EsPresentButUnreadable()
        {
            AssertIlegible(ReadText("null"));
        }

        // ================================================================ T-STO-11 Undefined

        [Fact]
        public void TSto11_ElementoUndefined_EsAbsent_SinExcepcion()
        {
            AssertAbsentVacio(new CustomPropertiesStore().ReadElement(default(JsonElement)));
        }

        // ================================================================ T-STO-12 nombres de miembro repetidos

        private static readonly IReadOnlyDictionary<string, Func<string>> DuplicateMemberCases =
            new Dictionary<string, Func<string>>
            {
                ["raiz-exacto"] = () => "{\"SchemaVersion\":\"1.0\",\"SchemaVersion\":\"1.0\",\"Entries\":[]}",
                ["raiz-capitalizacion"] = () => "{\"SchemaVersion\":\"1.0\",\"Entries\":[],\"entries\":[]}",
                ["raiz-capitalizacion-orden-inverso"] = () => "{\"SchemaVersion\":\"1.0\",\"entries\":[]," +
                    "\"Entries\":" + Entries(Entry(IdA, "Cliente", "ACME")) + "}",
                ["entrada-value-texto-primero"] = () => Doc("1.0", Entries(
                    "{\"Id\":\"" + IdA + "\",\"Name\":\"n\",\"Value\":\"v\",\"Value\":5}")),
                ["entrada-value-numero-primero"] = () => Doc("1.0", Entries(
                    "{\"Id\":\"" + IdA + "\",\"Name\":\"n\",\"Value\":5,\"Value\":\"v\"}")),
                ["entrada-name-capitalizacion"] = () => Doc("1.0", Entries(
                    "{\"Id\":\"" + IdA + "\",\"Name\":\"n\",\"name\":\"m\",\"Value\":\"v\"}")),
                ["extensiondata-anidado"] = () => Doc("1.0", "[]", ",\"Raiz\":{\"a\":1,\"A\":2}"),
                ["objeto-dentro-de-array"] = () => Doc("1.0", "[]", ",\"Raiz\":[{\"k\":1},{\"k\":1,\"k\":2}]"),
                ["extension-de-entrada-anidada"] = () => Doc("1.0", Entries(Entry(IdA, "n", "v", ",\"Extra\":{\"z\":{\"q\":1,\"Q\":1}}"))),
                ["clave-desconocida-de-raiz"] = () => Doc("1.0", "[]", ",\"Otro\":1,\"OTRO\":2"),
                ["major-futuro"] = () => "{\"SchemaVersion\":\"2.0\",\"Raiz\":{\"a\":1,\"a\":2}}",
            };

        [Theory]
        [InlineData("raiz-exacto")]
        [InlineData("raiz-capitalizacion")]
        [InlineData("raiz-capitalizacion-orden-inverso")]
        [InlineData("entrada-value-texto-primero")]
        [InlineData("entrada-value-numero-primero")]
        [InlineData("entrada-name-capitalizacion")]
        [InlineData("extensiondata-anidado")]
        [InlineData("objeto-dentro-de-array")]
        [InlineData("extension-de-entrada-anidada")]
        [InlineData("clave-desconocida-de-raiz")]
        [InlineData("major-futuro")]
        public void TSto12_NombresDeMiembroRepetidosACualquierProfundidad_SonPresentButUnreadable(string caso)
        {
            AssertIlegible(ReadBoth(DuplicateMemberCases[caso]()));
        }

        // ================================================================ T-STO-13 colision con ExtensionData

        [Fact]
        public void TSto13_MiembrosDeclaradosEnCualquierCapitalizacion_NuncaAcabanEnExtensionData()
        {
            var json = "{\"schemaversion\":\"1.0\",\"ENTRIES\":[{\"id\":\"" + IdA +
                       "\",\"NAME\":\"Cliente\",\"value\":\"ACME\"}]}";

            var result = ReadBoth(json);

            Assert.Equal(CustomPropertiesReadOutcome.Readable, result.Outcome);
            Assert.Null(result.Document.ExtensionData);

            var entry = Assert.Single(result.Document.Entries);

            Assert.Null(entry.ExtensionData);
            Assert.Equal("Cliente", entry.Name);
            Assert.Equal("ACME", entry.Value);
            Assert.Equal(
                "{\"SchemaVersion\":\"1.0\",\"Entries\":[{\"Id\":\"" + IdA + "\",\"Name\":\"Cliente\",\"Value\":\"ACME\"}]}",
                Serialize(result.Document));
        }

        [Theory]
        [InlineData("raiz", "entries")]
        [InlineData("raiz", "SCHEMAVERSION")]
        [InlineData("entrada", "VALUE")]
        [InlineData("entrada", "id")]
        [InlineData("entrada", "Name")]
        public void TSto13_ElEscritorRechazaExtensionDataQueColisionaConUnMiembroDeclarado(string nivel, string clave)
        {
            var document = CustomPropertiesDocument.CreateNew();
            document.Entries.Add(new CustomPropertyEntryDocument { Id = IdA, Name = "Cliente", Value = "ACME" });

            var extension = new Dictionary<string, JsonElement> { [clave] = Element("1") };

            if (nivel == "raiz")
            {
                document.ExtensionData = extension;
            }
            else
            {
                document.Entries[0].ExtensionData = extension;
            }

            Assert.Throws<InvalidOperationException>(() => Serialize(document));
        }

        // ================================================================ T-STO-14 id duplicado

        [Fact]
        public void TSto14_ElMismoGuidEnOtraCapitalizacion_EsAmbiguousIdentity_DistintoDeIlegible()
        {
            var json = Doc("1.0", Entries(
                Entry(IdA, "Cliente", "ACME"),
                Entry(IdA.ToUpperInvariant(), "Obra", "Nave 3")));

            var result = ReadBoth(json);

            AssertSoloLectura(result, CustomPropertiesReadOutcome.AmbiguousIdentity);
            AssertNingunaMutacion(result);
        }

        [Fact]
        public void TSto14_ConOtraInvalidezEstructural_GanaPresentButUnreadable()
        {
            var json = Doc("1.0", Entries(
                Entry(IdA, "Cliente", "ACME"),
                Entry(IdA.ToUpperInvariant(), "Obra", "Nave 3"),
                EntryRaw(IdB, "\"Revision\"", "5")));

            AssertIlegible(ReadBoth(json));
        }

        [Fact]
        public void TSto14_ConProfundidadExcesiva_GanaDepthLimitExceeded()
        {
            var json = Doc(
                "1.0",
                Entries(Entry(IdA, "Cliente", "ACME"), Entry(IdA, "Obra", "Nave 3")),
                ",\"Profundo\":" + Nested(19));

            AssertSoloLectura(ReadBoth(json), CustomPropertiesReadOutcome.DepthLimitExceeded);
        }

        [Fact]
        public void TSto14_EnUnMajorFuturo_GanaIncompatibleMajor()
        {
            var json = Doc("2.0", Entries(Entry(IdA, "Cliente", "ACME"), Entry(IdA, "Obra", "Nave 3")));

            AssertSoloLectura(ReadBoth(json), CustomPropertiesReadOutcome.IncompatibleMajor);
        }

        // ================================================================ T-STO-15 GUID en forma D exacta

        [Theory]
        [InlineData("0b8e7f7a-3c1d-4c7e-9a52-6f1a2d7b9c10")]
        [InlineData("0B8E7F7A-3C1D-4C7E-9A52-6F1A2D7B9C10")]
        [InlineData("0b8E7f7A-3c1D-4C7e-9a52-6F1a2d7B9c10")]
        public void TSto15_FormaDEnCualquierCaja_SeAcepta_YSeEscribeEnMinusculas(string text)
        {
            Assert.True(CustomPropertyId.TryParse(text, out var id));
            Assert.False(id.IsEmpty);
            Assert.Equal(IdA, id.ToString());
            Assert.Equal(Id(IdA), id);
        }

        [Theory]
        [InlineData(" 0b8e7f7a-3c1d-4c7e-9a52-6f1a2d7b9c10")]
        [InlineData("0b8e7f7a-3c1d-4c7e-9a52-6f1a2d7b9c10 ")]
        [InlineData("\t0b8e7f7a-3c1d-4c7e-9a52-6f1a2d7b9c10")]
        [InlineData("0b8e7f7a-3c1d-4c7e-9a52-6f1a2d7b9c10\t")]
        [InlineData("{0b8e7f7a-3c1d-4c7e-9a52-6f1a2d7b9c10}")]
        [InlineData("(0b8e7f7a-3c1d-4c7e-9a52-6f1a2d7b9c10)")]
        [InlineData("0b8e7f7a3c1d4c7e9a526f1a2d7b9c10")]
        [InlineData("0b8e7f7a-3c1d-4c7e-9a52-6f1a2d7b9c1")]
        [InlineData("0b8e7f7a-3c1d-4c7e-9a52-6f1a2d7b9c100")]
        [InlineData("00000000-0000-0000-0000-000000000000")]
        [InlineData("0b8e7f7a3-c1d-4c7e-9a52-6f1a2d7b9c10")]
        [InlineData("0b8e7f7g-3c1d-4c7e-9a52-6f1a2d7b9c10")]
        [InlineData("")]
        [InlineData(null)]
        public void TSto15_TextoFueraDeLaFormaDExacta_SeRechaza(string text)
        {
            Assert.False(CustomPropertyId.TryParse(text, out var id));
            Assert.True(id.IsEmpty);
        }

        [Fact]
        public void TSto15_LaIdentidadEsElValorDelGuid_NuncaElTexto()
        {
            var lower = Id(IdA);
            var upper = Id(IdA.ToUpperInvariant());

            Assert.Equal(lower, upper);
            Assert.True(lower == upper);
            Assert.False(lower != upper);
            Assert.Equal(lower.GetHashCode(), upper.GetHashCode());
            Assert.NotEqual(lower, Id(IdB));

            var nuevo = CustomPropertyId.New();

            Assert.False(nuevo.IsEmpty);
            Assert.Equal(nuevo.ToString().ToLowerInvariant(), nuevo.ToString());
            Assert.Equal(36, nuevo.ToString().Length);
            Assert.NotEqual(nuevo, CustomPropertyId.New());
        }

        [Fact]
        public void TSto15_EnElDocumento_UnIdEnMayusculasEsLegible_YSeReescribeEnMinusculas()
        {
            var original = Readable(Doc("1.0", Entries(
                Entry(IdA.ToUpperInvariant(), "Cliente", "ACME"),
                Entry(IdB, "Obra", "Nave 3"))));

            var serialized = Serialize(original);

            Assert.Contains("\"Id\":\"" + IdA + "\"", serialized, StringComparison.Ordinal);
            Assert.DoesNotContain(IdA.ToUpperInvariant(), serialized, StringComparison.Ordinal);
        }

        [Theory]
        [InlineData("espacio")]
        [InlineData("llaves")]
        [InlineData("formato-n")]
        [InlineData("guid-vacio")]
        [InlineData("35-caracteres")]
        public void TSto15_EnElDocumento_UnIdFueraDeLaFormaD_EsPresentButUnreadable(string caso)
        {
            string id;

            switch (caso)
            {
                case "espacio": id = " " + IdA; break;
                case "llaves": id = "{" + IdA + "}"; break;
                case "formato-n": id = IdA.Replace("-", string.Empty); break;
                case "guid-vacio": id = Guid.Empty.ToString("D"); break;
                case "35-caracteres": id = IdA.Substring(1); break;
                default: throw new ArgumentOutOfRangeException(nameof(caso));
            }

            AssertIlegible(ReadBoth(Doc("1.0", Entries(Entry(id, "Cliente", "ACME")))));
        }

        // ================================================================ T-STO-16 nombre Unicode NFC

        [Fact]
        public void TSto16_UnNombreNuevoEnNfd_SeEscribeEnNfcYRecortado()
        {
            var created = Apply(AbsentResult(), CustomPropertiesIntent.Create("  " + AreaNfd + "  ", "Norte"));

            Assert.True(created.Succeeded);
            Assert.Equal(AreaNfc, Assert.Single(created.Document.Entries).Name);
            Assert.Equal(AreaNfc, Assert.Single(Readable(Serialize(created.Document)).Entries).Name);
        }

        [Fact]
        public void TSto16_AreaNfcFrenteANfd_ColisionaEnUnicidad()
        {
            var current = ReadableResult(Doc("1.0", Entries(Entry(IdA, AreaNfd, "Norte"))));

            var created = Apply(current, CustomPropertiesIntent.Create(AreaNfc, "Sur"));

            Assert.False(created.Succeeded);
            Assert.Equal(CustomPropertiesRejection.NameCollision, created.Rejection);
            Assert.Null(created.Document);
        }

        [Fact]
        public void TSto16_AreaFrenteAarea_ColisionaEnUnicidad()
        {
            var current = ReadableResult(Doc("1.0", Entries(Entry(IdA, AreaNfc, "Norte"))));

            var created = Apply(current, CustomPropertiesIntent.Create(Unit(0x00E1) + "rea", "Sur"));

            Assert.False(created.Succeeded);
            Assert.Equal(CustomPropertiesRejection.NameCollision, created.Rejection);
        }

        [Fact]
        public void TSto16_ElDiagnosticoDeRepetidosUsaNfc()
        {
            var result = ReadBoth(Doc("1.0", Entries(
                Entry(IdA, AreaNfc, "Norte"),
                Entry(IdB, AreaNfd, "Sur"),
                Entry(IdC, "Obra", "Nave 3"))));

            Assert.Equal(CustomPropertiesReadOutcome.Readable, result.Outcome);
            Assert.Equal(new[] { Id(IdA), Id(IdB) }, result.RepeatedNameEntryIds);
        }

        [Fact]
        public void TSto16_LosNombresNoTocadosNoSeReescriben()
        {
            var current = ReadableResult(Doc("1.0", Entries(
                Entry(IdA, AreaNfd, "Norte"),
                Entry(IdB, "Obra", "Nave 3"))));

            var changed = Apply(current, CustomPropertiesIntent.ChangeValue(Id(IdB), "Nave 4"));

            Assert.True(changed.Succeeded);

            var reread = Readable(Serialize(changed.Document));

            Assert.Equal(AreaNfd, reread.Entries[0].Name);
            Assert.Equal("Nave 4", reread.Entries[1].Value);
        }

        [Theory]
        [InlineData(0xD800, "")]
        [InlineData(0xDC00, "")]
        [InlineData(0xDBFF, "x")]
        public void TSto16_UnNombreConSurrogateSuelto_SeRechazaAntesDeNfc_SinExcepcion(int surrogate, string sufijo)
        {
            var created = Apply(AbsentResult(), CustomPropertiesIntent.Create("a" + Unit(surrogate) + sufijo, "v"));

            Assert.False(created.Succeeded);
            Assert.Equal(CustomPropertiesRejection.NameMalformedUtf16, created.Rejection);
            Assert.Null(created.Document);
        }

        /// <summary>
        /// Con ICU, <c>Normalize(FormC)</c> lanza con U+FFFE: si el recorrido de no-caracteres no fuera ANTES, este caso
        /// saldria por la defensa de normalizacion y no como no-caracter. Los demas no lanzan en ICU, y aun asi se rechazan.
        /// </summary>
        [Theory]
        [InlineData(0xFFFE)]
        [InlineData(0xFFFF)]
        [InlineData(0xFDD0)]
        [InlineData(0xFDEF)]
        [InlineData(0x1FFFE)]
        [InlineData(0x10FFFF)]
        public void TSto16_UnNombreConNoCaracter_SeRechazaAntesDeNfc_SinExcepcion(int codePoint)
        {
            var created = Apply(AbsentResult(), CustomPropertiesIntent.Create("a" + Scalar(codePoint), "v"));

            Assert.False(created.Succeeded);
            Assert.Equal(CustomPropertiesRejection.NameNoncharacter, created.Rejection);

            var current = ReadableResult(Doc("1.0", Entries(Entry(IdA, "Cliente", "ACME"))));
            var renamed = Apply(current, CustomPropertiesIntent.Rename(Id(IdA), "b" + Scalar(codePoint)));

            Assert.False(renamed.Succeeded);
            Assert.Equal(CustomPropertiesRejection.NameNoncharacter, renamed.Rejection);
        }

        [Theory]
        [InlineData(0xFDCF)]
        [InlineData(0xFDF0)]
        [InlineData(0xFFFD)]
        [InlineData(0x1FFFD)]
        [InlineData(0x10FFFD)]
        public void TSto16_LosVecinosDeLosNoCaracteres_SonNombresValidos(int codePoint)
        {
            var created = Apply(AbsentResult(), CustomPropertiesIntent.Create("a" + Scalar(codePoint), "v"));

            Assert.True(created.Succeeded);
            Assert.Equal(CustomPropertiesReadOutcome.Readable, ReadText(Serialize(created.Document)).Outcome);
        }

        // ================================================================ T-STO-17 nombres repetidos entre entradas

        [Fact]
        public void TSto17_NombresRepetidosEntreEntradas_SonReadableConDiagnostico()
        {
            var result = ReadBoth(Doc("1.0", Entries(
                Entry(IdA, "Cliente", "ACME"),
                Entry(IdB, "cliente", "Otra"),
                Entry(IdC, "Obra", "Nave 3"))));

            Assert.Equal(CustomPropertiesReadOutcome.Readable, result.Outcome);
            Assert.True(result.CanWrite);
            Assert.Equal(new[] { Id(IdA), Id(IdB) }, result.RepeatedNameEntryIds);
        }

        [Fact]
        public void TSto17_SinRepetidos_ElDiagnosticoEstaVacio()
        {
            var result = ReadBoth(Doc("1.0", Entries(Entry(IdA, "Cliente", "ACME"), Entry(IdB, "Obra", "Nave 3"))));

            Assert.Equal(CustomPropertiesReadOutcome.Readable, result.Outcome);
            Assert.Empty(result.RepeatedNameEntryIds);
        }

        /// <summary>Dos ENTRADAS con el mismo nombre se leen; dos MIEMBROS JSON con el mismo nombre no.</summary>
        [Fact]
        public void TSto17_NoSeConfundeConUnMiembroJsonRepetido()
        {
            var dosEntradas = ReadBoth(Doc("1.0", Entries(Entry(IdA, "Cliente", "ACME"), Entry(IdB, "Cliente", "Otra"))));
            var unaEntradaConNameDoble = ReadBoth(Doc("1.0", Entries(
                "{\"Id\":\"" + IdA + "\",\"Name\":\"Cliente\",\"Name\":\"Cliente\",\"Value\":\"ACME\"}")));

            Assert.Equal(CustomPropertiesReadOutcome.Readable, dosEntradas.Outcome);
            AssertIlegible(unaEntradaConNameDoble);
        }

        // ================================================================ T-STO-18 limites y validacion de escritura

        private static CustomPropertiesMutationResult Crear(string name, string value) => Apply(AbsentResult(), CustomPropertiesIntent.Create(name, value));

        private static void AssertRechazo(CustomPropertiesMutationResult result, CustomPropertiesRejection expected)
        {
            Assert.False(result.Succeeded);
            Assert.Equal(expected, result.Rejection);
            Assert.Null(result.Document);
            Assert.False(string.IsNullOrWhiteSpace(result.Error));
        }

        [Fact]
        public void TSto18_NombreDe80TrasNfcYRecorte_SeAcepta_Y81SeRechaza()
        {
            Assert.True(Crear("  " + new string('n', 80) + "  ", "v").Succeeded);
            AssertRechazo(Crear(new string('n', 81), "v"), CustomPropertiesRejection.NameTooLong);
        }

        [Fact]
        public void TSto18_ElLimiteDelNombreSeMideTrasNfc()
        {
            var ochentaEnNfd = string.Concat(Enumerable.Repeat(AreaNfd.Substring(0, 2), 80));

            Assert.Equal(160, ochentaEnNfd.Length);
            Assert.True(Crear(ochentaEnNfd, "v").Succeeded);
            AssertRechazo(Crear(ochentaEnNfd + AreaNfd.Substring(0, 2), "v"), CustomPropertiesRejection.NameTooLong);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("\t\r\n")]
        public void TSto18_NombreVacioTrasRecortar_SeRechaza(string name)
        {
            AssertRechazo(Crear(name, "v"), CustomPropertiesRejection.NameEmpty);
        }

        [Theory]
        [InlineData(0x0000)]
        [InlineData(0x0001)]
        [InlineData(0x0009)]
        [InlineData(0x001F)]
        [InlineData(0x007F)]
        public void TSto18_NombreConCaracterDeControl_SeRechaza(int control)
        {
            AssertRechazo(Crear("a" + Unit(control) + "b", "v"), CustomPropertiesRejection.NameControlCharacter);
        }

        [Fact]
        public void TSto18_Valor1000SeAcepta_Y1001SeRechaza()
        {
            Assert.True(Crear("Notas", new string('v', 1000)).Succeeded);
            AssertRechazo(Crear("Notas", new string('v', 1001)), CustomPropertiesRejection.ValueTooLong);
        }

        [Fact]
        public void TSto18_ValorVacioOConCrLfYTabulador_SeAceptaYSeReleeExacto()
        {
            foreach (var value in new[] { string.Empty, "Linea 1\r\nLinea 2\tfin", "  espacios  " })
            {
                var created = Crear("Notas", value);

                Assert.True(created.Succeeded);
                Assert.Equal(value, Assert.Single(Readable(Serialize(created.Document)).Entries).Value);
            }
        }

        [Theory]
        [InlineData(0x0000)]
        [InlineData(0x0007)]
        [InlineData(0x001B)]
        [InlineData(0x007F)]
        public void TSto18_ValorConOtroCaracterDeControl_SeRechaza(int control)
        {
            AssertRechazo(Crear("Notas", "a" + Unit(control) + "b"), CustomPropertiesRejection.ValueControlCharacter);
        }

        [Fact]
        public void TSto18_Crear_Hasta50Entradas_LaQuincuagesimoPrimeraSeRechaza()
        {
            var current = AbsentResult();

            for (var i = 0; i < 50; i++)
            {
                var created = Apply(current, CustomPropertiesIntent.Create("P" + i, "v"));

                Assert.True(created.Succeeded, "entrada " + i);
                current = ReadableResult(Serialize(created.Document));
            }

            AssertRechazo(Apply(current, CustomPropertiesIntent.Create("P50", "v")), CustomPropertiesRejection.TooManyEntries);
        }

        private static string SesentaEntradas()
            => Entries(Enumerable.Range(0, 60).Select(i => Entry(Guid.NewGuid().ToString("D"), "P" + i, "v")).ToArray());

        [Fact]
        public void TSto18_LeerNuncaAplicaLimites()
        {
            var sesenta = ReadBoth(Doc("1.0", SesentaEntradas()));
            var valorLargo = ReadBoth(Doc("1.0", Entries(Entry(IdA, "Notas", new string('v', 1500)))));
            var nombreLargo = ReadBoth(Doc("1.0", Entries(Entry(IdA, new string('n', 120), "v"))));

            Assert.Equal(CustomPropertiesReadOutcome.Readable, sesenta.Outcome);
            Assert.Equal(60, sesenta.Document.Entries.Count);
            Assert.Equal(CustomPropertiesReadOutcome.Readable, valorLargo.Outcome);
            Assert.Equal(CustomPropertiesReadOutcome.Readable, nombreLargo.Outcome);
        }

        [Fact]
        public void TSto18_RenombrarUnaEntradaConValorDe1500_SePermite()
        {
            var current = ReadableResult(Doc("1.0", Entries(Entry(IdA, "Notas", new string('v', 1500)))));

            var renamed = Apply(current, CustomPropertiesIntent.Rename(Id(IdA), "Observaciones"));

            Assert.True(renamed.Succeeded);
            Assert.Equal(new string('v', 1500), Assert.Single(Readable(Serialize(renamed.Document)).Entries).Value);
        }

        [Fact]
        public void TSto18_CambiarValorEnUnaColeccionDe60_SePermite()
        {
            var current = ReadableResult(Doc("1.0", SesentaEntradas()));
            var target = Id(current.Document.Entries[10].Id);

            var changed = Apply(current, CustomPropertiesIntent.ChangeValue(target, "nuevo"));

            Assert.True(changed.Succeeded);
            Assert.Equal(60, changed.Document.Entries.Count);
        }

        [Fact]
        public void TSto18_EliminarUnaEntradaQueExcedeConteoOLongitud_SePermite()
        {
            var sesenta = ReadableResult(Doc("1.0", SesentaEntradas()));
            var borrada = Apply(sesenta, CustomPropertiesIntent.Delete(Id(sesenta.Document.Entries[0].Id)));

            Assert.True(borrada.Succeeded);
            Assert.Equal(59, borrada.Document.Entries.Count);

            var largas = ReadableResult(Doc("1.0", Entries(
                Entry(IdA, "Notas", new string('v', 1500)),
                Entry(IdB, new string('n', 120), "v"))));

            var sinValorLargo = Apply(largas, CustomPropertiesIntent.Delete(Id(IdA)));
            var sinNombreLargo = Apply(largas, CustomPropertiesIntent.Delete(Id(IdB)));

            Assert.True(sinValorLargo.Succeeded);
            Assert.True(sinNombreLargo.Succeeded);
        }

        [Fact]
        public void TSto18_NombreOValorConUtf16MalFormado_EsIntentInvalido()
        {
            AssertRechazo(Crear("a" + Unit(0xD800), "v"), CustomPropertiesRejection.NameMalformedUtf16);
            AssertRechazo(Crear("Notas", "v" + Unit(0xDC00)), CustomPropertiesRejection.ValueMalformedUtf16);

            var current = ReadableResult(Doc("1.0", Entries(Entry(IdA, "Notas", "v"))));

            AssertRechazo(
                Apply(current, CustomPropertiesIntent.ChangeValue(Id(IdA), Unit(0xD83D) + "x")),
                CustomPropertiesRejection.ValueMalformedUtf16);
        }

        [Fact]
        public void TSto18_UnValorConParDeSurrogatesValido_SeReleeExacto_SinU_FFFD()
        {
            var value = "ok " + Scalar(0x1F600) + " " + Scalar(0x10000);
            var created = Crear("Notas", value);

            Assert.True(created.Succeeded);

            var serialized = Serialize(created.Document);

            Assert.DoesNotContain(JsonEscape(0xFFFD), serialized, StringComparison.OrdinalIgnoreCase);
            Assert.Equal(value, Assert.Single(Readable(serialized).Entries).Value);
        }

        [Theory]
        [InlineData(0xFFFE)]
        [InlineData(0x1FFFE)]
        public void TSto18_NombreNuevoConNoCaracter_EsIntentInvalido_SinExcepcion(int codePoint)
        {
            AssertRechazo(Crear("Nota" + Scalar(codePoint), "v"), CustomPropertiesRejection.NameNoncharacter);
        }

        [Fact]
        public void TSto18_ValorConFFFE_EsValido_YSeReleeExacto()
        {
            var value = "a" + Unit(0xFFFE) + "b";
            var created = Crear("Notas", value);

            Assert.True(created.Succeeded);
            Assert.Equal(value, Assert.Single(Readable(Serialize(created.Document)).Entries).Value);
        }

        [Fact]
        public void TSto18_NombreOValorNulos_SonIntentsInvalidos()
        {
            AssertRechazo(Crear(null, "v"), CustomPropertiesRejection.NameMissing);
            AssertRechazo(Crear("Notas", null), CustomPropertiesRejection.ValueMissing);
        }

        // ================================================================ T-STO-19 guarda

        [Theory]
        [InlineData("ilegible")]
        [InlineData("ambigua")]
        [InlineData("major-futuro")]
        [InlineData("profundidad")]
        public void TSto19_TrasUnEstadoNoEscribible_NiMutacionNiEscritura(string estado)
        {
            string json;
            CustomPropertiesReadOutcome expected;

            switch (estado)
            {
                case "ilegible":
                    json = Doc("1.0", Entries(EntryRaw(IdA, "\"Cliente\"", "5")));
                    expected = CustomPropertiesReadOutcome.PresentButUnreadable;
                    break;
                case "ambigua":
                    json = Doc("1.0", Entries(Entry(IdA, "Cliente", "ACME"), Entry(IdA, "Obra", "Nave 3")));
                    expected = CustomPropertiesReadOutcome.AmbiguousIdentity;
                    break;
                case "major-futuro":
                    json = Doc("2.0", Entries(Entry(IdA, "Cliente", "ACME")));
                    expected = CustomPropertiesReadOutcome.IncompatibleMajor;
                    break;
                case "profundidad":
                    json = Doc("1.0", Entries(Entry(IdA, "Cliente", "ACME")), ",\"Profundo\":" + Nested(19));
                    expected = CustomPropertiesReadOutcome.DepthLimitExceeded;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(estado));
            }

            var result = ReadText(json);

            AssertSoloLectura(result, expected);
            AssertNingunaMutacion(result);
        }

        [Fact]
        public void TSto19_SoloAbsentYReadablePermitenEscribir_YNoHaberLeidoNoEsPermiso()
        {
            Assert.True(CustomPropertiesWriteGuard.CanOverwrite(AbsentResult(), out _));
            Assert.True(CustomPropertiesWriteGuard.CanOverwrite(ReadText(Doc("1.0", "[]")), out _));
            Assert.False(CustomPropertiesWriteGuard.CanOverwrite(null, out var error));
            Assert.False(string.IsNullOrWhiteSpace(error));

            var sinLectura = CustomPropertiesMutations.Apply(null, CustomPropertiesIntent.Create("Obra", "Nave 3"));

            Assert.False(sinLectura.Succeeded);
            Assert.Equal(CustomPropertiesRejection.NotWritable, sinLectura.Rejection);
        }

        /// <summary>V1 no tiene recuperacion destructiva (ADR-0039 §10): ningun miembro publico descarta, repara ni fuerza.</summary>
        [Fact]
        public void TSto19_NoExisteNingunaApiDeDescarteNiDeReparacion()
        {
            var forbidden = new[] { "Discard", "Descart", "Repair", "Repar", "Recover", "Reset", "Clear", "Force", "Purge" };

            var types = typeof(CustomPropertiesStore).Assembly.GetTypes()
                .Where(type => type.IsPublic && type.Name.StartsWith("CustomPropert", StringComparison.Ordinal))
                .ToList();

            Assert.True(types.Count >= 8, "El barrido apenas encontro tipos de propiedades personalizadas.");

            foreach (var type in types)
            {
                var names = type.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                    .Select(member => member.Name);

                foreach (var name in names)
                {
                    foreach (var token in forbidden)
                    {
                        Assert.False(
                            name.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0,
                            type.Name + "." + name + " contiene '" + token + "'");
                    }
                }
            }
        }

        // ================================================================ T-STO-20 ida y vuelta

        [Fact]
        public void TSto20_IdaYVuelta_ConservaOrdenIdsNombresValoresYVersion()
        {
            var nombres = new[] { AreaNfc, "Ñandú " + Scalar(0x1F600), Unit(0x6F22) + Unit(0x5B57) };
            var valores = new[]
            {
                "Linea 1\r\nLinea 2\tfin",
                "comillas \" y barra " + Backslash + " y par " + Scalar(0x1F680),
                "no-caracter en valor " + Unit(0xFFFE),
            };

            var json = Doc(
                "1.3",
                Entries(
                    Entry(IdC.ToUpperInvariant(), nombres[0], valores[0], ",\"E\":[1,{\"k\":\"v\"}]"),
                    Entry(IdA, nombres[1], valores[1]),
                    Entry(IdB, nombres[2], valores[2])),
                ",\"Raiz\":{\"r\":true}");

            var original = Readable(json);
            var serialized = Serialize(original);
            var reread = Readable(serialized);

            Assert.Equal("1.3", reread.SchemaVersion);
            Assert.Equal(new[] { Id(IdC), Id(IdA), Id(IdB) }, reread.Entries.Select(entry => Id(entry.Id)));
            Assert.Equal(nombres, reread.Entries.Select(entry => entry.Name));
            Assert.Equal(valores, reread.Entries.Select(entry => entry.Value));
            Assert.Equal("[1,{\"k\":\"v\"}]", reread.Entries[0].ExtensionData["E"].GetRawText());
            Assert.Equal("{\"r\":true}", reread.ExtensionData["Raiz"].GetRawText());
            Assert.Contains("\"Id\":\"" + IdC + "\"", serialized, StringComparison.Ordinal);
            Assert.Equal(serialized, Serialize(reread));
        }

        [Fact]
        public void TSto20_SiempreEmiteSchemaVersionYEntries()
        {
            Assert.Equal("{\"SchemaVersion\":\"1.0\",\"Entries\":[]}", Serialize(CustomPropertiesDocument.CreateNew()));

            var current = ReadableResult(Doc("1.0", Entries(Entry(IdA, "Cliente", "ACME"))));
            var vacia = Apply(current, CustomPropertiesIntent.Delete(Id(IdA)));

            Assert.True(vacia.Succeeded);
            Assert.Equal("{\"SchemaVersion\":\"1.0\",\"Entries\":[]}", Serialize(vacia.Document));
        }

        [Theory]
        [InlineData("1.0", "1.0")]
        [InlineData("1.3", "1.3")]
        [InlineData("1.10", "1.10")]
        public void TSto20_LaVersionNuncaSeDegrada(string leida, string escrita)
        {
            var current = ReadableResult(Doc(leida, Entries(Entry(IdA, "Cliente", "ACME"))));
            var changed = Apply(current, CustomPropertiesIntent.ChangeValue(Id(IdA), "Otro"));

            Assert.True(changed.Succeeded);
            Assert.Equal(escrita, Readable(Serialize(current.Document)).SchemaVersion);
            Assert.Equal(escrita, Readable(Serialize(changed.Document)).SchemaVersion);
        }

        [Theory]
        [InlineData("major-futuro")]
        [InlineData("version-invalida")]
        [InlineData("entries-nulo")]
        [InlineData("entrada-nula")]
        [InlineData("id-invalido")]
        [InlineData("ids-duplicados")]
        [InlineData("name-nulo")]
        [InlineData("name-vacio")]
        [InlineData("name-no-caracter")]
        [InlineData("name-surrogate-suelto")]
        [InlineData("value-nulo")]
        [InlineData("value-surrogate-suelto")]
        [InlineData("extension-undefined")]
        public void TSto20_ElEscritorRechazaUnDocumentoQueNoPodriaReleer(string caso)
        {
            var document = CustomPropertiesDocument.CreateNew();
            var entry = new CustomPropertyEntryDocument { Id = IdA, Name = "Cliente", Value = "ACME" };
            document.Entries.Add(entry);

            switch (caso)
            {
                case "major-futuro": document.SchemaVersion = "2.0"; break;
                case "version-invalida": document.SchemaVersion = "1.0.0"; break;
                case "entries-nulo": document.Entries = null; break;
                case "entrada-nula": document.Entries.Add(null); break;
                case "id-invalido": entry.Id = " " + IdA; break;
                case "ids-duplicados": document.Entries.Add(new CustomPropertyEntryDocument { Id = IdA.ToUpperInvariant(), Name = "Obra", Value = "v" }); break;
                case "name-nulo": entry.Name = null; break;
                case "name-vacio": entry.Name = "  "; break;
                case "name-no-caracter": entry.Name = "a" + Unit(0xFFFE); break;
                case "name-surrogate-suelto": entry.Name = "a" + Unit(0xD800); break;
                case "value-nulo": entry.Value = null; break;
                case "value-surrogate-suelto": entry.Value = "v" + Unit(0xDC00); break;
                case "extension-undefined": document.ExtensionData = new Dictionary<string, JsonElement> { ["X"] = default(JsonElement) }; break;
                default: throw new ArgumentOutOfRangeException(nameof(caso));
            }

            Assert.Throws<InvalidOperationException>(() => Serialize(document));
        }

        [Fact]
        public void TSto20_NameConFFFEyFDD0SonIlegibles_ValueConFFFEEsReadableExacto()
        {
            AssertIlegible(ReadText(Doc("1.0", Entries(EntryRaw(IdA, "\"a" + JsonEscape(0xFFFE) + "\"", "\"v\"")))));
            AssertIlegible(ReadText(Doc("1.0", Entries(EntryRaw(IdA, "\"a" + JsonEscape(0xFDD0) + "\"", "\"v\"")))));

            var result = ReadBoth(Doc("1.0", Entries(EntryRaw(IdA, "\"Notas\"", "\"a" + JsonEscape(0xFFFE) + "b\""))));

            Assert.Equal(CustomPropertiesReadOutcome.Readable, result.Outcome);
            Assert.Equal("a" + Unit(0xFFFE) + "b", Assert.Single(result.Document.Entries).Value);
            Assert.Equal("a" + Unit(0xFFFE) + "b", Assert.Single(Readable(Serialize(result.Document)).Entries).Value);
        }

        [Theory]
        [InlineData("malformado")]
        [InlineData("truncado")]
        [InlineData("no-json")]
        [InlineData("vacio")]
        [InlineData("blancos")]
        [InlineData("comentario")]
        [InlineData("coma-final")]
        [InlineData("profundidad-65")]
        [InlineData("surrogate-crudo-en-name")]
        [InlineData("surrogate-crudo-en-value")]
        [InlineData("surrogate-escapado-en-nombre-de-miembro")]
        [InlineData("surrogate-escapado-en-name")]
        [InlineData("surrogate-escapado-en-value")]
        [InlineData("surrogate-escapado-anidado-en-extensiondata")]
        [InlineData("name-con-fffe-escapado")]
        [InlineData("name-con-fdd0-escapado")]
        public void TSto20_TablaDeTextosExternos_NuncaLanza(string caso)
        {
            var json = ExternalCases[caso]();

            var result = ReadText(json);

            Assert.True(Enum.IsDefined(typeof(CustomPropertiesReadOutcome), result.Outcome));
            Assert.False(result.CanWrite);
        }

        /// <summary>
        /// Propiedad de «nunca lanza» sobre contenido que nadie diseño: mutaciones deterministas (semilla fija) de un
        /// documento valido, con un alfabeto que incluye estructura JSON, escapes, surrogates sueltos y no-caracteres.
        /// </summary>
        [Fact]
        public void TSto20_MutacionesDeterministasDeUnDocumentoValido_NuncaLanzan()
        {
            var seed = Doc(
                "1.0",
                Entries(Entry(IdA, "Cliente", "ACME", ",\"E\":{\"k\":[1,2]}"), Entry(IdB, AreaNfd, "Linea\r\n2")),
                ",\"Raiz\":{\"r\":true}");

            var alphabet = new[]
            {
                "{", "}", "[", "]", "\"", ":", ",", Backslash, "u", "0", "D", "8", "F", "E", " ", "null", "1.0", "2.0",
                Unit(0xD800), Unit(0xDC00), Unit(0xFFFE), Unit(0xFDD0), JsonEscape(0xD800), JsonEscape(0xFFFE),
                "\"SchemaVersion\"", "\"Entries\"", "\"Id\"", "\"Name\"", "\"Value\"",
            };

            var random = new Random(5424);
            var store = new CustomPropertiesStore();

            for (var i = 0; i < 600; i++)
            {
                var text = new StringBuilder(seed);
                var edits = 1 + random.Next(4);

                for (var e = 0; e < edits; e++)
                {
                    var position = random.Next(text.Length + 1);

                    switch (random.Next(3))
                    {
                        case 0:
                            text.Insert(position, alphabet[random.Next(alphabet.Length)]);
                            break;
                        case 1:
                            if (position < text.Length)
                            {
                                text.Remove(position, 1);
                            }

                            break;
                        default:
                            if (position < text.Length)
                            {
                                text.Remove(position, 1);
                                text.Insert(position, alphabet[random.Next(alphabet.Length)]);
                            }

                            break;
                    }
                }

                var json = text.ToString();
                var result = store.Read(CustomPropertiesPayload.Present(json));

                Assert.True(Enum.IsDefined(typeof(CustomPropertiesReadOutcome), result.Outcome));

                if (result.Outcome == CustomPropertiesReadOutcome.Readable)
                {
                    Assert.Equal(CustomPropertiesReadOutcome.Readable, ReadText(store.Serialize(result.Document)).Outcome);
                }

                if (TryParseElement(json, out var element))
                {
                    Assert.Equal(result.Outcome, store.ReadElement(element).Outcome);
                }
            }
        }
    }
}
