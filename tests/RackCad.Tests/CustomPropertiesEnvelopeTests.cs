using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems.FlowBed;
using Xunit;
using static RackCad.Tests.CustomPropertiesEnvelopeTestKit;
using static RackCad.Tests.CustomPropertiesTestKit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-54 G4B — contrato del miembro <c>CustomProperties</c> del sobre (Proposal V5 D-08, D-11.5, D-12 y D-21; §12.2
    /// T-ENV-01..13 y §12.5 T-CPY-01; ADR-0039 §4 y §6).
    ///
    /// <para>
    /// El sobre solo TRANSPORTA el miembro: lo guarda como <see cref="JsonElement"/> crudo, lo omite cuando es nulo y el
    /// compositor lo hereda. Interpretarlo es cosa exclusiva de <see cref="CustomPropertiesStore"/> (G3). Donde una prueba
    /// dice «igual que en BASE», lo comprueba contra el DTO con la forma de BASE de T-CHR (build I-11..I-53).
    /// </para>
    /// </summary>
    public class CustomPropertiesEnvelopeTests
    {
        private static readonly CustomPropertiesStore Properties = new CustomPropertiesStore();

        private static string ValidDocument => Doc("1.0", Entries(Entry(IdA, "Cliente", "ACME")));

        private static RackEmbedDocument ComposeFrom(RackEmbedDocument source)
            => RackEmbedComposer.Compose(source, source.Kind, source.Id, source.Name, source.View, source.Section, source.Design);

        /// <summary>Cuantas claves del objeto raiz se llaman como el miembro, sin distinguir mayusculas.</summary>
        private static int MemberKeys(string envelopeJson)
        {
            using (var document = JsonDocument.Parse(envelopeJson))
            {
                return document.RootElement.EnumerateObject()
                    .Count(property => string.Equals(property.Name, Member, StringComparison.OrdinalIgnoreCase));
            }
        }

        // ================================================================ T-ENV-01 nulo omitido

        [Theory]
        [MemberData(nameof(CustomPropertiesEnvelopeCharacterizationTests.EnvelopeCaseNames), MemberType = typeof(CustomPropertiesEnvelopeCharacterizationTests))]
        public void TEnv01_SinMiembro_LosBytesSonLosDeTChr01(string caso)
        {
            var envelope = EnvelopesWithoutMember[caso];

            var built = envelope.Build();
            Assert.False(built.CustomProperties.HasValue);
            Assert.Equal(envelope.Expected, Write(built));

            var reread = Read(envelope.Expected);
            Assert.False(reread.CustomProperties.HasValue);
            Assert.Equal(envelope.Expected, Write(reread));
        }

        /// <summary>Componer con un origen sin el miembro escribe lo mismo que el <c>Compose</c> de BASE.</summary>
        [Theory]
        [MemberData(nameof(CustomPropertiesEnvelopeCharacterizationTests.EnvelopeCaseNames), MemberType = typeof(CustomPropertiesEnvelopeCharacterizationTests))]
        public void TEnv01_SinMiembro_ComposeEscribeLoMismoQueElDeBase(string caso)
        {
            var expected = EnvelopesWithoutMember[caso].Expected;
            var source = Read(expected);
            var older = BaseShaped.Deserialize(expected);

            Assert.Equal(
                BaseShaped.Serialize(BaseShaped.Compose(older, older.Kind, IdB, "Otro", "lateral", 7, "{}")),
                Write(RackEmbedComposer.Compose(source, source.Kind, IdB, "Otro", "lateral", 7, "{}")));

            Assert.Equal(
                BaseShaped.Serialize(BaseShaped.Compose(null, older.Kind, IdB, "Otro", null, -1, "{}")),
                Write(RackEmbedComposer.Compose(null, source.Kind, IdB, "Otro", null, -1, "{}")));
        }

        // ================================================================ T-ENV-02 formas de JsonElement

        public static TheoryData<string, CustomPropertiesReadOutcome> MemberForms()
            => new TheoryData<string, CustomPropertiesReadOutcome>
            {
                { ValidDocument, CustomPropertiesReadOutcome.Readable },
                { "{\"x\":1}", CustomPropertiesReadOutcome.PresentButUnreadable },
                { "\"texto\"", CustomPropertiesReadOutcome.PresentButUnreadable },
                { "2.50", CustomPropertiesReadOutcome.PresentButUnreadable },
                { "-1e3", CustomPropertiesReadOutcome.PresentButUnreadable },
                { "[1,{\"b\":null}]", CustomPropertiesReadOutcome.PresentButUnreadable },
                { "true", CustomPropertiesReadOutcome.PresentButUnreadable },
                { "false", CustomPropertiesReadOutcome.PresentButUnreadable },
            };

        /// <summary>
        /// Ninguna forma JSON del miembro vuelve ilegible un sobre sintacticamente valido: se lee, se reescribe con su texto
        /// crudo exacto, y el store de propiedades clasifica cada forma por su cuenta (D-08.4).
        /// </summary>
        [Theory]
        [MemberData(nameof(MemberForms))]
        public void TEnv02_CualquierFormaDelMiembro_SobreLegible_TextoCrudoExacto_YElStoreLaClasifica(
            string member, CustomPropertiesReadOutcome outcome)
        {
            var json = Envelope("," + "\"CustomProperties\":" + member);

            var envelope = Read(json);

            Assert.NotNull(envelope);
            Assert.True(envelope.CustomProperties.HasValue);
            Assert.Equal(member, envelope.CustomProperties.Value.GetRawText());
            Assert.Null(envelope.ExtensionData);
            Assert.Equal(json, Write(envelope));
            Assert.Equal(outcome, Properties.ReadElement(envelope.CustomProperties).Outcome);
        }

        [Fact]
        public void TEnv02_NullLiteral_EsAusente_YSeEliminaAlReescribir()
        {
            var envelope = Read(Envelope(",\"CustomProperties\":null"));

            Assert.NotNull(envelope);
            Assert.False(envelope.CustomProperties.HasValue);
            Assert.Equal(Envelope(), Write(envelope));
            Assert.Equal(CustomPropertiesReadOutcome.Absent, Properties.ReadElement(envelope.CustomProperties).Outcome);
        }

        // ================================================================ T-ENV-03 corrupcion sintactica

        /// <summary>
        /// La afirmacion de aislamiento es solo para sobres SINTACTICAMENTE validos. Una corrupcion o un truncado dentro del
        /// miembro deja el sobre ilegible, exactamente como en BASE (D-08.4 A): no hay falso aislamiento que prometer.
        /// </summary>
        [Theory]
        [InlineData(",\"CustomProperties\":{\"SchemaVersion\":\"1.0\",")]
        [InlineData(",\"CustomProperties\":{\"a\":}")]
        [InlineData(",\"CustomProperties\":{\"a\":\"sin cerrar}")]
        [InlineData(",\"CustomProperties\":[1,2")]
        [InlineData(",\"CustomProperties\":{\"a\":1,}")]
        [InlineData(",\"CustomProperties\":{/* comentario */\"a\":1}")]
        [InlineData(",\"CustomProperties\":tru")]
        public void TEnv03_CorrupcionDentroDelMiembro_SobreIlegible_IgualQueEnBase(string tail)
        {
            Assert.Null(Read(Envelope(tail)));
            Assert.Null(BaseShaped.Deserialize(Envelope(tail)));
        }

        /// <summary>Residual F-14a (D-08.4 A): UTF-16 crudo invalido dentro del miembro lanza al leer, igual que en BASE.</summary>
        [Fact]
        public void TEnv03_Utf16CrudoInvalidoEnElMiembro_ResidualF14a_LanzaIgualQueEnBase()
        {
            var json = Envelope(",\"CustomProperties\":{\"x\":\"" + Unit(0xD800) + "\"}");

            Assert.Throws<ArgumentException>(() => Read(json));
            Assert.Throws<ArgumentException>(() => BaseShaped.Deserialize(json));
        }

        // ================================================================ T-ENV-04 profundidad

        /// <summary>El limite del PARSER del sobre: 63 niveles dentro del miembro se leen y 64 dejan el sobre ilegible.</summary>
        [Fact]
        public void TEnv04_LimiteDelParserDelSobre_63SeLeen_64Ilegible_ConElMiembroYEnBase()
        {
            var json63 = Envelope(",\"CustomProperties\":" + NestedArrays(63));
            var envelope = Read(json63);

            Assert.NotNull(envelope);
            Assert.Equal(NestedArrays(63), envelope.CustomProperties.Value.GetRawText());
            Assert.Equal(json63, Write(envelope));
            Assert.NotNull(BaseShaped.Deserialize(json63));

            var json64 = Envelope(",\"CustomProperties\":" + NestedArrays(64));

            Assert.Null(Read(json64));
            Assert.Null(BaseShaped.Deserialize(json64));
        }

        /// <summary>
        /// La cota del FORMATO de propiedades (16, D-05.7) es otra cosa: un miembro de 17 o 40 niveles es un sobre
        /// perfectamente legible que el store clasifica <c>DepthLimitExceeded</c>, y el sobre lo conserva intacto.
        /// </summary>
        [Theory]
        [InlineData(15, CustomPropertiesReadOutcome.Readable)]
        [InlineData(16, CustomPropertiesReadOutcome.DepthLimitExceeded)]
        [InlineData(39, CustomPropertiesReadOutcome.DepthLimitExceeded)]
        public void TEnv04_LaCotaDelFormato_NoEsLaDelParserDelSobre(int nested, CustomPropertiesReadOutcome outcome)
        {
            var member = Doc("1.0", Entries(Entry(IdA, "Cliente", "ACME")), ",\"Profundo\":" + Nested(nested));
            var json = Envelope(",\"CustomProperties\":" + member);

            var envelope = Read(json);

            Assert.Equal(nested + 1, Depth(envelope.CustomProperties.Value));
            Assert.Equal(outcome, Properties.ReadElement(envelope.CustomProperties).Outcome);
            Assert.Equal(json, Write(envelope));
        }

        [Fact]
        public void TEnv04_ElEscritorDePropiedadesNuncaEmiteMasDe16Niveles_NiAtravesDelSobre()
        {
            var excedido = CustomPropertiesDocument.CreateNew();
            excedido.ExtensionData = new Dictionary<string, JsonElement> { ["Profundo"] = Element(Nested(16)) };

            Assert.Throws<InvalidOperationException>(() => Properties.Serialize(excedido));

            var permitido = CustomPropertiesDocument.CreateNew();
            permitido.ExtensionData = new Dictionary<string, JsonElement> { ["Profundo"] = Element(Nested(15)) };

            var envelope = Read(Write(RackEmbedComposer.WithCustomProperties(Read(Envelope()), Json(Properties.Serialize(permitido)))));

            Assert.Equal(16, Depth(envelope.CustomProperties.Value));
            Assert.Equal(CustomPropertiesReadOutcome.Readable, Properties.ReadElement(envelope.CustomProperties).Outcome);
        }

        // ================================================================ T-ENV-05 Undefined y Null

        public static TheoryData<string> DegenerateMembers() => new TheoryData<string> { "Undefined", "Null" };

        private static JsonElement Degenerate(string kind) => kind == "Undefined" ? default(JsonElement) : Json("null");

        [Theory]
        [MemberData(nameof(DegenerateMembers))]
        public void TEnv05_Compose_NormalizaUndefinedYNullAAusente(string kind)
        {
            var source = Read(Envelope(",\"Futuro\":1"));
            source.CustomProperties = Degenerate(kind);

            var composed = ComposeFrom(source);

            Assert.False(composed.CustomProperties.HasValue);
            Assert.Equal(Envelope(",\"Futuro\":1"), Write(composed));
        }

        [Theory]
        [MemberData(nameof(DegenerateMembers))]
        public void TEnv05_WithCustomProperties_NormalizaUndefinedYNullAAusente(string kind)
        {
            var source = Read(Envelope(",\"CustomProperties\":" + ValidDocument + ",\"Futuro\":1"));

            var result = RackEmbedComposer.WithCustomProperties(source, Degenerate(kind));

            Assert.False(result.CustomProperties.HasValue);
            Assert.Equal(Envelope(",\"Futuro\":1"), Write(result));
        }

        /// <summary>
        /// Por que normaliza el compositor (P-17): sin normalizar, <c>default(JsonElement)</c> hace lanzar al store y un
        /// elemento <c>Null</c> escribe <c>"CustomProperties":null</c>.
        /// </summary>
        [Fact]
        public void TEnv05_SinNormalizar_ElStoreLanzaOEscribeNull()
        {
            var envelope = Read(Envelope());

            envelope.CustomProperties = default(JsonElement);
            Assert.Throws<InvalidOperationException>(() => Write(envelope));

            envelope.CustomProperties = Json("null");
            Assert.EndsWith(",\"CustomProperties\":null}", Write(envelope));
        }

        /// <summary>Ningun sobre producido por el compositor hace lanzar al store ni repite o anula la clave del miembro.</summary>
        [Fact]
        public void TEnv05_TodoSobreDelCompositor_SeSerializaSinExcepcion()
        {
            var sources = new List<RackEmbedDocument>
            {
                null,
                Read(Envelope()),
                Read(Envelope(",\"CustomProperties\":" + ValidDocument)),
                Read(Envelope(",\"CustomProperties\":\"texto\",\"Futuro\":[1]")),
            };

            var undefined = Read(Envelope());
            undefined.CustomProperties = default(JsonElement);
            sources.Add(undefined);

            var nullKind = Read(Envelope());
            nullKind.CustomProperties = Json("null");
            sources.Add(nullKind);

            var values = new JsonElement?[] { null, default(JsonElement), Json("null"), Json("{}"), Json(ValidDocument), Json("\"s\"") };

            foreach (var source in sources)
            {
                var composed = Write(RackEmbedComposer.Compose(source, RackEmbedDocument.KindCama, IdA, "Cama 1", null, -1, "{}"));
                Assert.True(MemberKeys(composed) <= 1);
                Assert.DoesNotContain("\"CustomProperties\":null", composed);

                if (source == null)
                {
                    continue;
                }

                foreach (var value in values)
                {
                    var written = Write(RackEmbedComposer.WithCustomProperties(source, value));
                    Assert.True(MemberKeys(written) <= 1);
                    Assert.DoesNotContain("\"CustomProperties\":null", written);
                }
            }
        }

        // ================================================================ T-ENV-06 clave repetida

        /// <summary>Residual de D-08.4 (P-17.7): con la clave exacta repetida gana la ultima, en BASE y con el miembro.</summary>
        [Fact]
        public void TEnv06_ClaveExactaRepetida_GanaLaUltima_EnBaseYConElMiembro()
        {
            var json = Envelope(",\"CustomProperties\":{\"v\":1},\"CustomProperties\":{\"v\":2}");

            var envelope = Read(json);
            Assert.Equal("{\"v\":2}", envelope.CustomProperties.Value.GetRawText());
            Assert.Equal(Envelope(",\"CustomProperties\":{\"v\":2}"), Write(envelope));

            var older = BaseShaped.Deserialize(json);
            Assert.Equal("{\"v\":2}", older.ExtensionData[Member].GetRawText());
            Assert.Equal(Envelope(",\"CustomProperties\":{\"v\":2}"), BaseShaped.Serialize(older));
        }

        /// <summary>
        /// Residual de D-08.4 (P-33): con otra capitalizacion, el DTO de BASE conserva AMBAS claves en ExtensionData y las
        /// reemite; el sobre con el miembro las asigna las dos al miembro y se queda con la ultima. No se afirma «gana la
        /// ultima en todos los builds».
        /// </summary>
        [Fact]
        public void TEnv06_ClaveRepetidaConOtraCapitalizacion_BaseConservaAmbas_ElMiembroSeQuedaConLaUltima()
        {
            var json = Envelope(",\"CustomProperties\":{\"v\":1},\"customproperties\":{\"v\":2}");

            var envelope = Read(json);
            Assert.Equal("{\"v\":2}", envelope.CustomProperties.Value.GetRawText());
            Assert.Null(envelope.ExtensionData);
            Assert.Equal(Envelope(",\"CustomProperties\":{\"v\":2}"), Write(envelope));

            var older = BaseShaped.Deserialize(json);
            Assert.Equal(2, older.ExtensionData.Count);
            Assert.Equal(json, BaseShaped.Serialize(older));
        }

        [Fact]
        public void TEnv06_NingunEscritorDelCompositorEmiteLaClaveRepetida()
        {
            var source = Read(Envelope(",\"CustomProperties\":{\"v\":1},\"customproperties\":{\"v\":2},\"Futuro\":1"));

            Assert.Equal(1, MemberKeys(Write(ComposeFrom(source))));
            Assert.Equal(1, MemberKeys(Write(RackEmbedComposer.WithCustomProperties(source, Json(ValidDocument)))));
            Assert.Equal(0, MemberKeys(Write(RackEmbedComposer.WithCustomProperties(source, null))));
        }

        // ================================================================ T-ENV-07 colision con ExtensionData

        /// <summary>Los miembros declarados del sobre, por reflexion sobre el DTO de produccion (sin el de extension).</summary>
        private static IReadOnlyList<string> DeclaredMembers()
            => typeof(RackEmbedDocument).GetProperties()
                .Where(property => !property.IsDefined(typeof(JsonExtensionDataAttribute), false))
                .Select(property => property.Name)
                .ToList();

        [Fact]
        public void TEnv07_ElSobreDeclaraOchoMiembros_ElOctavoEsCustomProperties()
        {
            Assert.Equal(
                new[] { "SchemaVersion", "Kind", "View", "Section", "Id", "Name", "Design", "CustomProperties" },
                DeclaredMembers());

            var member = typeof(RackEmbedDocument).GetProperty(Member);
            Assert.Equal(typeof(JsonElement?), member.PropertyType);
            Assert.Equal(
                JsonIgnoreCondition.WhenWritingNull,
                Assert.Single(member.GetCustomAttributes(typeof(JsonIgnoreAttribute), false).Cast<JsonIgnoreAttribute>()).Condition);
            Assert.Equal("1.0", RackEmbedDocument.CurrentSchemaVersion);
        }

        public static TheoryData<string> CollidingKeys()
        {
            var data = new TheoryData<string>();

            foreach (var name in DeclaredMembers())
            {
                data.Add(name);
                data.Add(name.ToLowerInvariant());
                data.Add(name.ToUpperInvariant());
            }

            return data;
        }

        /// <summary>Leyendo, un miembro declarado nunca acaba en ExtensionData, ni exacto ni con otra capitalizacion (P-34).</summary>
        [Theory]
        [InlineData("customproperties", "{\"a\":1}")]
        [InlineData("CUSTOMPROPERTIES", "[1]")]
        [InlineData("kind", "\"cama\"")]
        [InlineData("DESIGN", "\"{}\"")]
        [InlineData("schemaversion", "\"1.3\"")]
        [InlineData("section", "5")]
        [InlineData("VIEW", "\"planta\"")]
        [InlineData("id", "\"otro\"")]
        [InlineData("nAmE", "\"Otro\"")]
        public void TEnv07_AlLeer_UnMiembroDeclaradoNuncaAcabaEnExtensionData(string key, string value)
        {
            var envelope = Read(Envelope(",\"Futuro\":1,\"" + key + "\":" + value));

            Assert.NotNull(envelope);
            Assert.Equal(new[] { "Futuro" }, envelope.ExtensionData.Keys);
        }

        [Theory]
        [MemberData(nameof(CollidingKeys))]
        public void TEnv07_Compose_RechazaUnOrigenConColision_SinDescartarLaClave(string key)
        {
            var source = Read(Envelope(",\"Futuro\":1"));
            source.ExtensionData[key] = Json("{\"e\":2}");

            Assert.Throws<InvalidOperationException>(() => ComposeFrom(source));
            Assert.Equal(new[] { "Futuro", key }, source.ExtensionData.Keys);
        }

        [Theory]
        [MemberData(nameof(CollidingKeys))]
        public void TEnv07_WithCustomProperties_RechazaUnOrigenConColision_SinDescartarLaClave(string key)
        {
            var source = Read(Envelope(",\"Futuro\":1"));
            source.ExtensionData[key] = Json("{\"e\":2}");

            Assert.Throws<InvalidOperationException>(() => RackEmbedComposer.WithCustomProperties(source, Json(ValidDocument)));
            Assert.Equal(new[] { "Futuro", key }, source.ExtensionData.Keys);
        }

        /// <summary>Por que se rechaza (P-17.8): el store emitiria dos claves y, al releer, ganaria la de ExtensionData.</summary>
        [Fact]
        public void TEnv07_SinRechazo_SeEmitiranDosClaves_YAlReleerGanaLaDeExtensionData()
        {
            var envelope = Read(Envelope(",\"CustomProperties\":{\"m\":1}"));
            envelope.ExtensionData = new Dictionary<string, JsonElement> { [Member] = Json("{\"e\":2}") };

            var written = Write(envelope);

            Assert.Equal(2, MemberKeys(written));
            Assert.Equal("{\"e\":2}", Read(written).CustomProperties.Value.GetRawText());
        }

        // ================================================================ T-ENV-08 preservacion en Compose

        [Fact]
        public void TEnv08_ConOrigen_ConservaElMiembroExacto_YTomaLoDemasDelLlamador()
        {
            var member = Doc("1.3", Entries(Entry(IdA, "Cliente", "ACME", ",\"Nota\":{\"n\":2.50}")), ",\"Raiz\":[1,{\"r\":true}]");
            var source = Read(Envelope(",\"CustomProperties\":" + member + ",\"Futuro\":1", version: "\"1.7\""));

            var composed = RackEmbedComposer.Compose(
                source, RackEmbedDocument.KindSelective, IdB, "Nuevo", RackEmbedDocument.ViewLateral, 2, "{\"d\":2}");

            Assert.Equal(member, composed.CustomProperties.Value.GetRawText());
            Assert.Equal(
                "{\"SchemaVersion\":\"1.7\",\"Kind\":\"selective\",\"View\":\"lateral\",\"Section\":2,\"Id\":\"" + IdB
                + "\",\"Name\":\"Nuevo\",\"Design\":\"{" + Q + "d" + Q + ":2}\",\"CustomProperties\":" + member + ",\"Futuro\":1}",
                Write(composed));
        }

        [Fact]
        public void TEnv08_DosVistasHermanas_CadaUnaConservaLaSuya()
        {
            var memberA = Doc("1.0", Entries(Entry(IdA, "Cliente", "ACME")));
            var memberB = Doc("1.0", Entries(Entry(IdB, "Cliente", "OTRO")));
            var frontal = Read(Envelope(",\"CustomProperties\":" + memberA));
            var lateral = Read(Envelope(",\"CustomProperties\":" + memberB));

            var composedFrontal = RackEmbedComposer.Compose(
                frontal, RackEmbedDocument.KindSelective, IdA, "Rack A", RackEmbedDocument.ViewFrontal, -1, "{}");
            var composedLateral = RackEmbedComposer.Compose(
                lateral, RackEmbedDocument.KindSelective, IdA, "Rack A", RackEmbedDocument.ViewLateral, 1, "{}");

            Assert.Equal(memberA, composedFrontal.CustomProperties.Value.GetRawText());
            Assert.Equal(memberB, composedLateral.CustomProperties.Value.GetRawText());
        }

        [Fact]
        public void TEnv08_SinOrigen_OConUnOrigenSinMiembro_NoHayMiembro()
        {
            var fresh = RackEmbedComposer.Compose(null, RackEmbedDocument.KindCama, IdA, "Cama 1", null, -1, "{}");
            Assert.False(fresh.CustomProperties.HasValue);
            Assert.Equal(0, MemberKeys(Write(fresh)));

            var withoutMember = ComposeFrom(Read(Envelope(",\"Futuro\":1")));
            Assert.False(withoutMember.CustomProperties.HasValue);
            Assert.Equal(Envelope(",\"Futuro\":1"), Write(withoutMember));
        }

        /// <summary>Compose no interpreta el miembro: hereda cualquier forma, tambien una que el store no puede leer.</summary>
        [Theory]
        [InlineData("\"texto\"")]
        [InlineData("2.50")]
        [InlineData("[1,{\"b\":null}]")]
        [InlineData("false")]
        [InlineData("{\"SchemaVersion\":\"9.0\",\"Otra\":\"forma\"}")]
        public void TEnv08_ConOrigen_HeredaCualquierFormaSinInterpretarla(string member)
        {
            var json = Envelope(",\"CustomProperties\":" + member);

            Assert.Equal(json, Write(ComposeFrom(Read(json))));
        }

        // ================================================================ T-ENV-09 WithCustomProperties

        [Fact]
        public void TEnv09_CambiaSoloElMiembro_YConservaTodoLoDemas_SinTocarElOrigen()
        {
            var quote = Backslash + "\"";
            var oldMember = Doc("1.0", Entries(Entry(IdA, "Cliente", "ACME")));
            var newMember = Doc("1.0", Entries(Entry(IdB, "Area", "Norte")));
            var source = Read(
                "{\"SchemaVersion\":\"1.7\",\"Kind\":\"pushback\",\"View\":\"lateral\",\"Section\":3,\"Id\":\"" + IdA
                + "\",\"Name\":\"Push Back 1\",\"Design\":\"{" + quote + "Lines" + quote + ":2}\",\"CustomProperties\":" + oldMember
                + ",\"Futuro\":{\"x\":[1,2.50]}}");
            var sourceBytes = Write(source);

            var result = RackEmbedComposer.WithCustomProperties(source, Json(newMember));

            Assert.NotSame(source, result);
            Assert.Equal(
                "{\"SchemaVersion\":\"1.7\",\"Kind\":\"pushback\",\"View\":\"lateral\",\"Section\":3,\"Id\":\"" + IdA
                + "\",\"Name\":\"Push Back 1\",\"Design\":\"{" + Q + "Lines" + Q + ":2}\",\"CustomProperties\":" + newMember
                + ",\"Futuro\":{\"x\":[1,2.50]}}",
                Write(result));

            Assert.Equal(sourceBytes, Write(source));
            Assert.Equal(oldMember, source.CustomProperties.Value.GetRawText());
        }

        /// <summary>El resultado no comparte el diccionario de extension con el origen: tocar uno no toca el otro.</summary>
        [Fact]
        public void TEnv09_ElResultadoNoCompartiraExtensionDataConElOrigen()
        {
            var source = Read(Envelope(",\"Futuro\":1"));

            var result = RackEmbedComposer.WithCustomProperties(source, Json(ValidDocument));
            result.ExtensionData["Nuevo"] = Json("2");

            Assert.Equal(new[] { "Futuro" }, source.ExtensionData.Keys);
        }

        /// <summary>La version se resuelve con la misma politica que <c>Compose</c> (T-CHR-02): nunca se degrada.</summary>
        [Theory]
        [InlineData("\"1.7\"", "1.7")]
        [InlineData("\"1.0\"", "1.0")]
        [InlineData(null, "1.0")]
        [InlineData("null", "1.0")]
        [InlineData("\"abc\"", "1.0")]
        [InlineData("\"1.0.5\"", "1.0")]
        [InlineData("\" 1.8 \"", "1.8")]
        [InlineData("\"1.99\"", "1.99")]
        public void TEnv09_LaVersionNoSeDegrada(string sourceVersion, string expected)
        {
            var source = Read(sourceVersion == null ? EnvelopeWithoutVersion() : Envelope(version: sourceVersion));

            var result = RackEmbedComposer.WithCustomProperties(source, Json(ValidDocument));

            Assert.Equal(expected, result.SchemaVersion);
            Assert.Equal(
                RackEmbedComposer.Compose(source, source.Kind, source.Id, source.Name, source.View, source.Section, source.Design).SchemaVersion,
                result.SchemaVersion);
        }

        [Fact]
        public void TEnv09_ConNull_QuitaElMiembro_YNoTocaLoDemas()
        {
            var source = Read(Envelope(",\"CustomProperties\":" + ValidDocument + ",\"Futuro\":1"));

            Assert.Equal(Envelope(",\"Futuro\":1"), Write(RackEmbedComposer.WithCustomProperties(source, null)));
        }

        [Fact]
        public void TEnv09_EnUnSobreSinMiembro_LoEscribeTrasDesign_YAntesDeExtensionData()
        {
            var source = Read(Envelope(",\"Futuro\":1"));

            Assert.Equal(
                Envelope(",\"CustomProperties\":" + ValidDocument + ",\"Futuro\":1"),
                Write(RackEmbedComposer.WithCustomProperties(source, Json(ValidDocument))));
        }

        [Fact]
        public void TEnv09_SinOrigen_EsUnErrorDeProgramacion()
        {
            Assert.Throws<ArgumentNullException>(() => RackEmbedComposer.WithCustomProperties(null, Json(ValidDocument)));
        }

        /// <summary>La costura con G3 sin acoplar el sobre al esquema interior: el texto del store viaja como elemento crudo.</summary>
        [Fact]
        public void TEnv09_ElDocumentoDelStoreViajaPorElSobre_YSeReleeIgual()
        {
            var document = Readable(Doc("1.2", Entries(Entry(IdA, "Cliente", "ACME"), Entry(IdB, "Area", "Norte"))));

            var envelope = Read(Write(RackEmbedComposer.WithCustomProperties(Read(Envelope()), Json(Properties.Serialize(document)))));
            var reread = Properties.ReadElement(envelope.CustomProperties);

            Assert.Equal(CustomPropertiesReadOutcome.Readable, reread.Outcome);
            Assert.Equal("1.2", reread.Document.SchemaVersion);
            Assert.Equal(new[] { "ACME", "Norte" }, reread.Document.Entries.Select(entry => entry.Value));
        }

        // ================================================================ T-ENV-10 build anterior

        private static string OlderBuildMember
            => Doc("1.3", Entries(Entry(IdA, "Cliente", "ACME", ",\"Nota\":\"x\"")), ",\"Raiz\":[1,2.50]");

        /// <summary>
        /// Un build I-11..I-53 (DTO con la forma de BASE) no conoce el miembro: lo guarda en ExtensionData y lo reemite
        /// exacto. No se afirma nada de builds anteriores a I-11, que no tienen ExtensionData.
        /// </summary>
        [Fact]
        public void TEnv10_UnBuildAnterior_GuardaElMiembroEnExtensionData_YLoReemiteExacto()
        {
            var written = Write(RackEmbedComposer.WithCustomProperties(Read(Envelope()), Json(OlderBuildMember)));
            Assert.Equal(Envelope(",\"CustomProperties\":" + OlderBuildMember), written);

            var older = BaseShaped.Deserialize(written);

            Assert.Null(typeof(BaseShapedEnvelope).GetProperty(Member));
            Assert.Equal(OlderBuildMember, older.ExtensionData[Member].GetRawText());
            Assert.Equal(written, BaseShaped.Serialize(older));
        }

        [Fact]
        public void TEnv10_PorElComposeDeUnBuildAnterior_ElMiembroSobrevive()
        {
            var older = BaseShaped.Deserialize(Envelope(",\"CustomProperties\":" + OlderBuildMember));

            var composed = BaseShaped.Serialize(BaseShaped.Compose(older, older.Kind, IdA, "Rack A", "lateral", 2, "{}"));

            Assert.Equal(OlderBuildMember, Read(composed).CustomProperties.Value.GetRawText());
        }

        [Fact]
        public void TEnv10_PorElRestampDelMismoObjetoEnUnBuildAnterior_ElMiembroSobrevive()
        {
            var older = BaseShaped.Deserialize(Envelope(",\"CustomProperties\":" + OlderBuildMember + ",\"Futuro\":1"));

            older.Id = IdB;
            older.Name = "Rack A (copia)";
            older.Design = "{}";
            var restamped = Read(BaseShaped.Serialize(older));

            Assert.Equal(IdB, restamped.Id);
            Assert.Equal(OlderBuildMember, restamped.CustomProperties.Value.GetRawText());
            Assert.Equal("1", restamped.ExtensionData["Futuro"].GetRawText());
        }

        // ================================================================ T-ENV-11 frontera de biblioteca

        private static readonly string[] LibraryTransportFiles =
        {
            "src/RackCad.Application/Persistence/RackDesignLibrary.cs",
            "src/RackCad.Application/Persistence/RackProjectDocument.cs",
            "src/RackCad.Application/Persistence/RackProjectStore.cs",
            "src/RackCad.Application/Persistence/RackFrameProjectStore.cs",
            "src/RackCad.Application/Systems/Selective/SelectiveLibraryExport.cs",
            "src/RackCad.UI/RackDesignLibraryWindow.xaml.cs",
        };

        /// <summary>
        /// Frontera de V1 (D-12): la biblioteca no lee ni escribe el miembro. Los archivos de transporte de
        /// <c>.rackcad.json</c> no nombran <c>CustomProperties</c> ni el sobre, ni en codigo ni en literales.
        /// </summary>
        [Fact]
        public void TEnv11_LaBibliotecaNoLeeNiEscribeElMiembro()
        {
            foreach (var relative in LibraryTransportFiles)
            {
                var path = Path.Combine(RepoRoot().FullName, relative);
                Assert.True(File.Exists(path), relative);

                var source = File.ReadAllText(path);
                var code = PluginSourceCode.Mask(source);

                Assert.DoesNotContain(Member, code);
                Assert.DoesNotMatch(@"\bRackEmbed(?:Document|Store|Composer)\b", code);
                Assert.DoesNotContain(PluginSourceCode.StringLiterals(source), literal => literal.Contains(Member));
            }

            Assert.Null(typeof(RackProjectDocument).GetProperty(Member));
            Assert.DoesNotContain(typeof(RackProjectDocument).GetProperties(), property => property.PropertyType == typeof(RackEmbedDocument));
        }

        [Fact]
        public void TEnv11_GuardarUnDisenoEnLaBiblioteca_NoEscribeElMiembro()
        {
            var cama = new FlowBedConfiguration { BedType = FlowBedType.Pushback, LaneDepth = 200.0, PalletDepth = 0.0, RollerId = "ROLLER_A" };

            var json = new RackProjectStore().Serialize(RackProject.ForCama(cama));

            Assert.DoesNotContain(Member, json, StringComparison.OrdinalIgnoreCase);
        }

        // ================================================================ T-ENV-12 Compose(null) solo donde se espera

        /// <summary>
        /// <c>Compose(null)</c> solo en rack nuevo e importacion de biblioteca (D-21). En <c>src/</c> ninguna llamada pasa un
        /// <c>null</c> literal: seis constructores de payload reciben el sobre como parametro y la propagacion de variables
        /// pasa <c>view.Embed</c> (T-GRD-02). Quien llama a esos constructores sin sobre queda fuera del alcance de Core,
        /// declarado en D-21: OV-03, OV-04, OV-07, OV-11 y OV-13.
        /// </summary>
        [Fact]
        public void TEnv12_ComposeNull_SoloLlegaPorElParametroDeLosConstructoresDePayload()
        {
            var calls = EnvelopeSourceGuards.ComposeCallsOutsideComposer(EnvelopeSourceGuards.ProductionSources());

            Assert.Equal(7, calls.Count);
            Assert.DoesNotContain(calls, call => call.FirstArgument == "null");
            Assert.Equal(6, calls.Count(call => call.Member.StartsWith("Build", StringComparison.Ordinal) || call.Member == "WrapSelectivePayload"));
            Assert.Single(calls, call => call.FirstArgument == "view.Embed");
            Assert.Empty(EnvelopeSourceGuards.ConstructionsOutsideComposer(EnvelopeSourceGuards.ProductionSources()));
        }

        // ================================================================ T-ENV-13 F-14b con el miembro

        /// <summary>
        /// Residual preexistente F-14b con la variante de I-54 (C-F2): el mismo que caracteriza T-CHR-04. El sobre se lee;
        /// reserializarlo y componer con el para serializar lanzan <see cref="JsonException"/>; y el store de propiedades
        /// clasifica el miembro sin lanzar. INV-07 excluye este caso; I-54 no lo corrige.
        /// </summary>
        [Theory]
        [MemberData(nameof(CustomPropertiesEnvelopeCharacterizationTests.EscapedLoneSurrogateCases), MemberType = typeof(CustomPropertiesEnvelopeCharacterizationTests))]
        public void TEnv13_F14bConElMiembro_EsElMismoResidualQueEnBase_YElStoreClasificaSinLanzar(string caso, string member)
        {
            var json = Envelope("," + member);

            var envelope = Read(json);
            Assert.True(envelope != null, caso);
            Assert.Throws<JsonException>(() => Write(envelope));
            Assert.Throws<JsonException>(() => Write(ComposeFrom(envelope)));

            var older = BaseShaped.Deserialize(json);
            Assert.True(older != null, caso);
            Assert.Throws<JsonException>(() => BaseShaped.Serialize(older));

            var outcome = Properties.ReadElement(envelope.CustomProperties).Outcome;

            if (caso.StartsWith("A", StringComparison.Ordinal))
            {
                Assert.True(envelope.CustomProperties.HasValue, caso);
                Assert.Equal(CustomPropertiesReadOutcome.PresentButUnreadable, outcome);
            }
            else
            {
                Assert.False(envelope.CustomProperties.HasValue, caso);
                Assert.Equal(CustomPropertiesReadOutcome.Absent, outcome);
            }
        }

        /// <summary>
        /// Escribir propiedades validas no «limpia» un sobre con F-14b en otro campo: serializar el resultado tambien lanza.
        /// Por eso el ejecutor de G5 serializa todos los payloads antes de la primera escritura (D-22.10).
        /// </summary>
        [Fact]
        public void TEnv13_PropiedadesValidasSobreUnSobreConF14bEnOtroCampo_TambienLanzanAlSerializar()
        {
            var envelope = Read(Envelope(",\"Futuro\":{\"nota\":\"" + JsonEscape(0xDC00) + "\"}"));

            var result = RackEmbedComposer.WithCustomProperties(envelope, Json(ValidDocument));

            Assert.Throws<JsonException>(() => Write(result));
        }

        // ================================================================ T-CPY-01 propiedad del store para el restamp

        /// <summary>
        /// La PROPIEDAD del <c>RackEmbedStore</c> de produccion de la que depende el restamp (D-11.5): mutar <c>Id</c>,
        /// <c>Name</c> y <c>Design</c> sobre el MISMO objeto deserializado y serializarlo conserva <c>CustomProperties</c> con su
        /// texto crudo exacto, <c>View</c>, <c>Section</c>, <c>SchemaVersion</c> y <c>ExtensionData</c>. No es una prueba de
        /// comportamiento de <c>RackEnvelopeRestamp</c>, que ninguna suite ejecuta: su forma la fija T-GRD-03.
        /// </summary>
        [Fact]
        public void TCpy01_MutarIdNombreYDisenoDelMismoObjeto_ConservaElMiembroYLoDemas()
        {
            var quote = Backslash + "\"";
            var member = Doc("1.3", Entries(Entry(IdA, "Cliente", "ACME", ",\"Nota\":{\"n\":2.50}")), ",\"Raiz\":[1,{\"r\":true}]");
            var envelope = Read(
                "{\"SchemaVersion\":\"1.7\",\"Kind\":\"selective\",\"View\":\"lateral\",\"Section\":4,\"Id\":\"" + IdA
                + "\",\"Name\":\"Rack A\",\"Design\":\"{" + quote + "Id" + quote + ":" + quote + IdA + quote + "}\",\"CustomProperties\":"
                + member + ",\"Futuro\":{\"x\":[1,2]}}");

            envelope.Id = IdB;
            envelope.Name = "Rack A (copia)";
            envelope.Design = "{\"Id\":\"" + IdB + "\"}";
            var written = Write(envelope);

            Assert.Equal(
                "{\"SchemaVersion\":\"1.7\",\"Kind\":\"selective\",\"View\":\"lateral\",\"Section\":4,\"Id\":\"" + IdB
                + "\",\"Name\":\"Rack A (copia)\",\"Design\":\"{" + Q + "Id" + Q + ":" + Q + IdB + Q + "}\",\"CustomProperties\":"
                + member + ",\"Futuro\":{\"x\":[1,2]}}",
                written);

            var reread = Read(written);
            Assert.Equal(member, reread.CustomProperties.Value.GetRawText());
            Assert.Equal(CustomPropertiesReadOutcome.Readable, Properties.ReadElement(reread.CustomProperties).Outcome);
        }
    }
}
