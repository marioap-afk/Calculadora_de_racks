using System;
using System.Linq;
using System.Text.Json;
using RackCad.Application.Persistence;
using Xunit;
using static RackCad.Tests.CustomPropertiesEnvelopeTestKit;
using static RackCad.Tests.CustomPropertiesTestKit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-54 G4A — CARACTERIZACION del sobre de BASE antes de tocarlo (Proposal V5 §12.2, T-CHR-01..04; §14, G4).
    ///
    /// <para>
    /// No describe el comportamiento futuro: congela el de <c>RackEmbedDocument</c>, <c>RackEmbedStore</c> y
    /// <c>RackEmbedComposer</c> tal como estan en <c>BASE_SHA</c> (<c>46fcac2</c>; los tres archivos son identicos en
    /// <c>f8deb67</c> y en esta rama antes de G4B). Por eso estas pruebas nacen en verde, y G4B las tiene que conservar
    /// en verde: todo lo que afirman es lo que el miembro nuevo promete no cambiar.
    /// </para>
    /// <para>
    /// Donde el resultado dependeria de que el DTO conozca o no <c>CustomProperties</c> (en que diccionario acaba la
    /// clave), la afirmacion se hace sobre <see cref="CustomPropertiesEnvelopeTestKit.BaseShapedEnvelope"/>, el DTO local
    /// con la forma de BASE, y no sobre el de produccion.
    /// </para>
    /// </summary>
    public class CustomPropertiesEnvelopeCharacterizationTests
    {
        public static TheoryData<string> EnvelopeCaseNames()
        {
            var data = new TheoryData<string>();

            foreach (var name in EnvelopesWithoutMember.Keys)
            {
                data.Add(name);
            }

            return data;
        }

        // ================================================================ T-CHR-01 bytes de sobres sin el miembro

        [Theory]
        [MemberData(nameof(EnvelopeCaseNames))]
        public void TChr01_SobreSinMiembro_BaseEscribeEstosBytes(string caso)
        {
            var envelope = EnvelopesWithoutMember[caso];

            Assert.Equal(envelope.Expected, Write(envelope.Build()));
        }

        [Theory]
        [MemberData(nameof(EnvelopeCaseNames))]
        public void TChr01_LosBytesCongelados_SeReleenYSeReescribenIguales(string caso)
        {
            var expected = EnvelopesWithoutMember[caso].Expected;

            var reread = Read(expected);

            Assert.NotNull(reread);
            Assert.Equal(expected, Write(reread));
        }

        [Fact]
        public void TChr01_LosCasosCubrenCadaKind_ConYSinExtensionData()
        {
            var kinds = new[]
            {
                RackEmbedDocument.KindSelective,
                RackEmbedDocument.KindDynamic,
                RackEmbedDocument.KindCabecera,
                RackEmbedDocument.KindCama,
                RackEmbedDocument.KindPushBack,
                RackEmbedDocument.KindCantilever,
            };

            foreach (var kind in kinds)
            {
                var cases = EnvelopesWithoutMember.Values.Where(envelope => envelope.Kind == kind).ToList();

                Assert.Contains(cases, envelope => !envelope.WithExtensionData);
                Assert.Contains(cases, envelope => envelope.WithExtensionData);
            }

            foreach (var envelope in EnvelopesWithoutMember.Values)
            {
                var built = envelope.Build();

                Assert.Equal(envelope.Kind, built.Kind);
                Assert.Equal(envelope.WithExtensionData, built.ExtensionData != null && built.ExtensionData.Count > 0);
            }
        }

        /// <summary>
        /// El DTO local con la forma de BASE es fiel al de produccion: mismos miembros, tipos y valores por defecto (sin
        /// contar <c>CustomProperties</c>, que BASE no tiene), y los mismos bytes para todos los casos de T-CHR-01.
        /// </summary>
        [Fact]
        public void TChr01_ElDtoConFormaDeBase_EsFielAlSobreDeProduccion()
        {
            Assert.Equal(
                DeclaredShape(typeof(BaseShapedEnvelope)),
                DeclaredShape(typeof(RackEmbedDocument), Member));

            foreach (var envelope in EnvelopesWithoutMember.Values)
            {
                Assert.Equal(envelope.Expected, BaseShaped.Serialize(BaseShaped.Deserialize(envelope.Expected)));
            }
        }

        // ================================================================ T-CHR-02 Compose de BASE

        [Fact]
        public void TChr02_ConOrigen_HeredaVersionYExtensionData_YTomaLoDemasDelLlamador()
        {
            var source = Read(
                "{\"SchemaVersion\":\"1.7\",\"Kind\":\"selective\",\"View\":\"frontal\",\"Section\":-1,\"Id\":\"" + IdB
                + "\",\"Name\":\"Viejo\",\"Design\":\"{}\",\"Futuro\":{\"k\":[1,{\"q\":\"w\"}]}}");

            var composed = RackEmbedComposer.Compose(
                source, RackEmbedDocument.KindDynamic, IdA, "Nuevo", RackEmbedDocument.ViewLateral, 2, "{\"d\":2}");

            Assert.Equal(
                "{\"SchemaVersion\":\"1.7\",\"Kind\":\"dynamic\",\"View\":\"lateral\",\"Section\":2,\"Id\":\"" + IdA
                + "\",\"Name\":\"Nuevo\",\"Design\":\"{" + Q + "d" + Q + ":2}\",\"Futuro\":{\"k\":[1,{\"q\":\"w\"}]}}",
                Write(composed));
        }

        [Fact]
        public void TChr02_DosVistas_CadaUnaConservaSuPropiaExtensionData()
        {
            var frontal = Read(Envelope(",\"Futuro\":\"de la frontal\""));
            var lateral = Read(Envelope(",\"Futuro\":{\"de\":\"la lateral\"}"));

            var composedFrontal = RackEmbedComposer.Compose(
                frontal, RackEmbedDocument.KindSelective, IdA, "Rack A", RackEmbedDocument.ViewFrontal, -1, "{}");
            var composedLateral = RackEmbedComposer.Compose(
                lateral, RackEmbedDocument.KindSelective, IdA, "Rack A", RackEmbedDocument.ViewLateral, 1, "{}");

            Assert.EndsWith(",\"Futuro\":\"de la frontal\"}", Write(composedFrontal));
            Assert.EndsWith(",\"Futuro\":{\"de\":\"la lateral\"}}", Write(composedLateral));
        }

        /// <summary>La version que escribe <c>Compose</c> para cada version de origen (fila: literal JSON o ausente).</summary>
        [Theory]
        [InlineData("\"1.7\"", "1.7")]
        [InlineData("\"1.0\"", "1.0")]
        [InlineData(null, "1.0")]
        [InlineData("null", "1.0")]
        [InlineData("\"abc\"", "1.0")]
        [InlineData("\"\"", "1.0")]
        [InlineData("\"0.9\"", "1.0")]
        [InlineData("\"-1\"", "1.0")]
        [InlineData("\"1\"", "1.0")]
        [InlineData("\"1.0.5\"", "1.0")]
        [InlineData("\" 1.8 \"", "1.8")]
        [InlineData("\"1.99\"", "1.99")]
        public void TChr02_ConOrigen_LaVersionNoSeDegradaNiSeCopiaBasura(string sourceVersion, string expected)
        {
            var source = Read(sourceVersion == null ? EnvelopeWithoutVersion() : Envelope(version: sourceVersion));
            Assert.NotNull(source);

            var composed = RackEmbedComposer.Compose(source, RackEmbedDocument.KindCama, IdA, "Cama 1", null, -1, "{}");

            Assert.Equal(expected, composed.SchemaVersion);
            Assert.StartsWith("{\"SchemaVersion\":\"" + expected + "\",", Write(composed));
        }

        [Fact]
        public void TChr02_SinOrigen_BytesDeUnSobreNuevo()
        {
            Assert.Equal(
                "{\"SchemaVersion\":\"1.0\",\"Kind\":\"selective\",\"View\":\"frontal\",\"Section\":-1,\"Id\":\"" + IdA
                + "\",\"Name\":\"Rack A\",\"Design\":\"{" + Q + "a" + Q + ":1}\"}",
                Write(RackEmbedComposer.Compose(
                    null, RackEmbedDocument.KindSelective, IdA, "Rack A", RackEmbedDocument.ViewFrontal, -1, "{\"a\":1}")));

            Assert.Equal(
                "{\"SchemaVersion\":\"1.0\",\"Kind\":\"cama\",\"View\":null,\"Section\":-1,\"Id\":\"" + IdA
                + "\",\"Name\":\"Cama 1\",\"Design\":\"{}\"}",
                Write(RackEmbedComposer.Compose(null, RackEmbedDocument.KindCama, IdA, "Cama 1", null, -1, "{}")));

            Assert.Equal(
                "{\"SchemaVersion\":\"1.0\",\"Kind\":\"pushback\",\"View\":\"lateral\",\"Section\":4,\"Id\":\"" + IdA
                + "\",\"Name\":\"Push Back 1\",\"Design\":\"{}\"}",
                Write(RackEmbedComposer.Compose(
                    null, RackEmbedDocument.KindPushBack, IdA, "Push Back 1", RackEmbedDocument.ViewLateral, 4, "{}")));
        }

        [Fact]
        public void TChr02_SinOrigen_VersionActualYSinExtensionData()
        {
            var composed = RackEmbedComposer.Compose(
                null, RackEmbedDocument.KindCantilever, IdA, "Cantilever 1", RackEmbedDocument.ViewFrontal, -1, "{}");

            Assert.Equal(RackEmbedDocument.CurrentSchemaVersion, composed.SchemaVersion);
            Assert.Equal("1.0", RackEmbedDocument.CurrentSchemaVersion);
            Assert.Null(composed.ExtensionData);
        }

        // ================================================================ T-CHR-03 ida y vuelta del store

        /// <summary>Un campo desconocido de cada forma JSON sobrevive con su texto crudo exacto.</summary>
        [Theory]
        [InlineData("{\"a\":{\"b\":[1,{\"c\":null}]}}")]
        [InlineData("[1,\"dos\",true,false,null]")]
        [InlineData("\"texto\"")]
        [InlineData("2.50")]
        [InlineData("1e3")]
        [InlineData("-0")]
        [InlineData("12345678901234567890123")]
        [InlineData("true")]
        [InlineData("false")]
        [InlineData("null")]
        public void TChr03_CampoDesconocido_SobreviveConSuTextoCrudo(string value)
        {
            var json = Envelope(",\"Futuro\":" + value);

            Assert.Equal(json, Write(Read(json)));
        }

        [Fact]
        public void TChr03_CamposDesconocidos_SeEscribenAlFinal_EnSuOrden()
        {
            var json = "{\"Primero\":1,\"SchemaVersion\":\"1.0\",\"Kind\":\"cama\",\"Segundo\":[2],\"View\":null,\"Section\":-1,"
                       + "\"Id\":\"" + IdA + "\",\"Name\":\"Cama 1\",\"Design\":\"{}\",\"Tercero\":{\"t\":3}}";

            Assert.Equal(
                "{\"SchemaVersion\":\"1.0\",\"Kind\":\"cama\",\"View\":null,\"Section\":-1,\"Id\":\"" + IdA
                + "\",\"Name\":\"Cama 1\",\"Design\":\"{}\",\"Primero\":1,\"Segundo\":[2],\"Tercero\":{\"t\":3}}",
                Write(Read(json)));
        }

        [Fact]
        public void TChr03_MinorFuturo_EsLegible_YSeReescribeSinTocarLaVersion()
        {
            var json = Envelope(",\"CampoDel17\":{\"x\":1}", version: "\"1.7\"");

            var envelope = Read(json);

            Assert.NotNull(envelope);
            Assert.Equal("1.7", envelope.SchemaVersion);
            Assert.Equal(json, Write(envelope));
        }

        [Theory]
        [InlineData("\"2.0\"")]
        [InlineData("\"2\"")]
        [InlineData("\" 2.0 \"")]
        [InlineData("\"10.1\"")]
        public void TChr03_MajorFuturo_NoEsLegible(string version)
        {
            Assert.Null(Read(Envelope(version: version)));
            Assert.Null(Read(Envelope(",\"CampoDel2\":{\"x\":1}", version: version)));
        }

        /// <summary>
        /// Mismo major o version no interpretable: legible, y el store escribe la version TAL CUAL la leyo (no la resuelve;
        /// eso es cosa de <c>Compose</c>). Un sobre sin version toma el valor por defecto del DTO.
        /// </summary>
        [Theory]
        [InlineData(null, "\"1.0\"")]
        [InlineData("null", "null")]
        [InlineData("\"1.0\"", "\"1.0\"")]
        [InlineData("\"1\"", "\"1\"")]
        [InlineData("\"1.99\"", "\"1.99\"")]
        [InlineData("\"1.0.5\"", "\"1.0.5\"")]
        [InlineData("\" 1.8 \"", "\" 1.8 \"")]
        [InlineData("\"0.9\"", "\"0.9\"")]
        [InlineData("\"-1\"", "\"-1\"")]
        [InlineData("\"abc\"", "\"abc\"")]
        [InlineData("\"\"", "\"\"")]
        public void TChr03_MajorActual_ReglasDeLectura(string version, string written)
        {
            var json = version == null ? EnvelopeWithoutVersion() : Envelope(version: version);

            var envelope = Read(json);

            Assert.NotNull(envelope);
            Assert.StartsWith("{\"SchemaVersion\":" + written + ",", Write(envelope));
        }

        [Fact]
        public void TChr03_NombresSinDistinguirMayusculas_VanAlMiembroDeclarado_YLosNulosSeEscriben()
        {
            var envelope = Read("{\"kind\":\"cama\",\"ID\":\"" + IdA + "\",\"name\":\"Cama 1\"}");

            Assert.Null(envelope.ExtensionData);
            Assert.Equal(
                "{\"SchemaVersion\":\"1.0\",\"Kind\":\"cama\",\"View\":null,\"Section\":-1,\"Id\":\"" + IdA
                + "\",\"Name\":\"Cama 1\",\"Design\":null}",
                Write(envelope));
        }

        /// <summary>Una clave que solo se parece a un miembro (i sin punto, U+0131) NO es ese miembro: va a ExtensionData.</summary>
        [Fact]
        public void TChr03_ClaveParecidaAUnMiembro_NoEsElMiembro()
        {
            var lookalike = "K" + Unit(0x0131) + "nd";

            var envelope = Read("{\"" + lookalike + "\":\"x\",\"Kind\":\"cama\"}");

            Assert.Equal("cama", envelope.Kind);
            Assert.True(envelope.ExtensionData.ContainsKey(lookalike));
            Assert.EndsWith(",\"K" + JsonEscape(0x0131) + "nd\":\"x\"}", Write(envelope));
        }

        [Theory]
        [InlineData("{\"Kind\":\"cama\",\"Section\":\"2\"}")]
        [InlineData("{\"Kind\":\"cama\",\"Id\":5}")]
        [InlineData("{\"Kind\":{}}")]
        [InlineData("null")]
        [InlineData("[]")]
        [InlineData("\"sobre\"")]
        [InlineData("")]
        [InlineData("   ")]
        public void TChr03_NoEsUnSobre_OTipoIncorrectoEnUnMiembroDeclarado_EsIlegible(string json)
        {
            Assert.Null(Read(json));
        }

        /// <summary>
        /// Una corrupcion sintactica o un truncado, TAMBIEN dentro de un campo que BASE no conoce (con la clave del miembro
        /// futuro o con otra), deja el sobre entero ilegible: no hay aislamiento que afirmar para ese caso (D-08.4 A).
        /// </summary>
        [Theory]
        [InlineData(",\"CustomProperties\":{\"SchemaVersion\":\"1.0\",")]
        [InlineData(",\"CustomProperties\":{\"a\":}")]
        [InlineData(",\"CustomProperties\":{\"a\":\"sin cerrar}")]
        [InlineData(",\"CustomProperties\":[1,2")]
        [InlineData(",\"CustomProperties\":{\"a\":1,}")]
        [InlineData(",\"CustomProperties\":{/* comentario */\"a\":1}")]
        [InlineData(",\"Futuro\":{\"a\":}")]
        [InlineData(",\"Futuro\":tru")]
        public void TChr03_CorrupcionOTruncado_DejaElSobreIlegible(string tail)
        {
            Assert.Null(Read(Envelope(tail)));
            Assert.Null(Read(Envelope(tail).TrimEnd('}')));
        }

        /// <summary>
        /// Residual preexistente F-14a (D-08.4): UTF-16 crudo invalido en el TEXTO del sobre. BASE solo captura
        /// <see cref="JsonException"/>, asi que <c>Deserialize</c> lanza <see cref="System.ArgumentException"/> al leer.
        /// </summary>
        [Theory]
        [InlineData("CustomProperties")]
        [InlineData("Futuro")]
        public void TChr03_Utf16CrudoInvalido_ResidualF14a_LanzaArgumentException(string key)
        {
            var json = Envelope(",\"" + key + "\":{\"x\":\"" + Unit(0xD800) + "\"}");

            Assert.Throws<System.ArgumentException>(() => Read(json));
        }

        /// <summary>
        /// El limite del PARSER del sobre, no el del formato de propiedades: 63 contenedores anidados dentro de un campo se
        /// leen (el sobre suma uno: 64) y 64 dejan el sobre ilegible (D-08.4 B).
        /// </summary>
        [Theory]
        [InlineData("CustomProperties")]
        [InlineData("Futuro")]
        public void TChr03_Profundidad_63NivelesSeLeen_64DejanElSobreIlegible(string key)
        {
            foreach (var nested in new[] { NestedArrays(63), Nested(63) })
            {
                var json = Envelope(",\"" + key + "\":" + nested);

                Assert.Equal(json, Write(Read(json)));
            }

            Assert.Null(Read(Envelope(",\"" + key + "\":" + NestedArrays(64))));
            Assert.Null(Read(Envelope(",\"" + key + "\":" + Nested(64))));
        }

        // ================================================================ T-CHR-04 residual F-14b en BASE

        public static TheoryData<string, string> EscapedLoneSurrogateCases()
            => new TheoryData<string, string>
            {
                {
                    "A-valor-bajo-CustomProperties",
                    "\"CustomProperties\":{\"SchemaVersion\":\"1.0\",\"Entries\":[{\"Id\":\"" + IdA + "\",\"Name\":\""
                    + JsonEscape(0xD800) + "\",\"Value\":\"v\"}]}"
                },
                { "A-nombre-bajo-CustomProperties", "\"CustomProperties\":{\"" + JsonEscape(0xD800) + "\":1}" },
                { "B-valor-en-otro-campo", "\"Futuro\":{\"nota\":\"" + JsonEscape(0xDC00) + "\"}" },
                { "B-nombre-en-otro-campo", "\"Futuro\":{\"" + JsonEscape(0xD800) + "\":1}" },
            };

        /// <summary>
        /// Residual preexistente F-14b (C-F2): con el escape de un surrogate suelto, el sobre se lee; reserializar ESE objeto
        /// y componer con el como origen para serializar lanzan <see cref="JsonException"/>. El restamp no se ejecuta aqui
        /// (el Plugin no carga en las suites): lo cubren T-CPY-01, T-GRD-03 y el residual declarado.
        /// </summary>
        [Theory]
        [MemberData(nameof(EscapedLoneSurrogateCases))]
        public void TChr04_SurrogateSueltoEscapado_SeLee_PeroNoSeReserializa(string caso, string member)
        {
            var envelope = Read(Envelope("," + member));

            Assert.True(envelope != null, caso);
            Assert.Throws<JsonException>(() => Write(envelope));
            Assert.Throws<JsonException>(() => Write(RackEmbedComposer.Compose(
                envelope, envelope.Kind, envelope.Id, envelope.Name, envelope.View, envelope.Section, envelope.Design)));
        }

        /// <summary>El mismo residual con el DTO de BASE, que guarda la clave <c>CustomProperties</c> en ExtensionData.</summary>
        [Theory]
        [MemberData(nameof(EscapedLoneSurrogateCases))]
        public void TChr04_ConLaFormaDeBase_LaClaveQuedaEnExtensionData_YTampocoSeReserializa(string caso, string member)
        {
            var envelope = BaseShaped.Deserialize(Envelope("," + member));

            Assert.True(envelope != null, caso);
            Assert.True(
                envelope.ExtensionData.ContainsKey(caso.StartsWith("A", StringComparison.Ordinal) ? "CustomProperties" : "Futuro"),
                caso);
            Assert.Throws<JsonException>(() => BaseShaped.Serialize(envelope));
            Assert.Throws<JsonException>(() => BaseShaped.Serialize(BaseShaped.Compose(
                envelope, envelope.Kind, envelope.Id, envelope.Name, envelope.View, envelope.Section, envelope.Design)));
        }

        /// <summary>Control: con un par de surrogates VALIDO escapado, las mismas reescrituras funcionan y conservan el texto.</summary>
        [Theory]
        [InlineData("CustomProperties")]
        [InlineData("Futuro")]
        public void TChr04_ParDeSurrogatesValido_SeReserializaSinCambios(string key)
        {
            var json = Envelope(",\"" + key + "\":{\"x\":\"" + JsonEscape(0xD83D) + JsonEscape(0xDE00) + "\"}");

            var envelope = Read(json);

            Assert.Equal(json, Write(envelope));
            Assert.Equal(json, Write(RackEmbedComposer.Compose(
                envelope, envelope.Kind, envelope.Id, envelope.Name, envelope.View, envelope.Section, envelope.Design)));
            Assert.Equal(json, BaseShaped.Serialize(BaseShaped.Deserialize(json)));
        }
    }
}
