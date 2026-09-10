using System.Collections.Generic;
using System.Text.Json;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-47 gate G3 — el diseño Selectivo authored aprende a LLEVAR un binding sin resolverlo todavia.
    ///
    /// <para>
    /// Dos cosas distintas viven aqui, y confundirlas fue el defecto que una revision adversarial encontro:
    /// el <b>literal authored</b> —lo que el usuario tecleo, persistido en <c>VerticalClearance</c>— y el
    /// <b>binding</b> —la referencia que gobernara el valor efectivo—. Mientras hay binding el literal queda
    /// <b>congelado e inactivo</b>; no se borra, no se sobrescribe y no se usa. Quien lo resuelve es G4;
    /// este gate solo garantiza que ninguno de los dos se pierde en el viaje.
    /// </para>
    /// <para>
    /// La promocion de schema es <b>pegajosa</b>, es decir monotona: sube y no baja. Una regla que mirase
    /// solo el contenido haria oscilar el documento <c>1.0 → 2.0 → 1.0</c> al vincular y desvincular, y «que
    /// versiones pueden abrir este rack» pasaria a depender de estado transitorio. El coste, aceptado y
    /// dicho: un rack vinculado UNA vez queda fuera del alcance de las versiones anteriores para siempre.
    /// </para>
    /// <para>
    /// C4.8-1 NO aplica a este documento: <c>SchemaVersion</c> conserva su inicializador historico, porque
    /// nacio antes de que hubiera version y para el "ausente" si significa legado.
    /// </para>
    /// </summary>
    public class SelectiveAuthoredBindingTests
    {
        private const string Guid1 = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";
        private const string Guid2 = "8a1d4e77-2c93-4b60-8f15-6e0b93a7c221";

        private static SelectivePalletDesign Diseno(double clearance = 6.0)
        {
            var design = new SelectivePalletDesign { VerticalClearance = clearance };
            var bay = new SelectiveBayDesign();
            bay.Levels.Add(new SelectiveCell
            {
                Pallet = new Tarima { Frente = 48, Alto = 50 },
                PalletCount = 1,
                BeamId = "BEAM_A",
                BeamPeralte = 4.5,
            });
            design.Bays.Add(bay);
            return design;
        }

        private static SelectivePalletDesignDocument Documento(double clearance = 6.0)
            => SelectivePalletDesignDocument.From(Diseno(clearance), Guid1, "Rack 1");

        private static SelectivePalletDesignDocument Vinculado(double literalCongelado = 6.0)
        {
            var doc = Documento(literalCongelado);
            doc.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
            {
                [ProjectPropertyIds.SelectiveVerticalClearanceToken] =
                    new SelectivePropertyValueDocument { Kind = "projectVariable", VariableId = Guid2 },
            };
            return doc;
        }

        // ================================================================ prueba 10 — sticky, las 4 ramas

        [Fact]
        public void Prueba10_Rama1_SinPropertyValuesSeQuedaEnLaLinea_1x()
        {
            Assert.Equal("1.0", SelectiveDesignSchema.ResolveWriteVersion("1.0", hasPropertyValues: false));
            Assert.Equal("1.4", SelectiveDesignSchema.ResolveWriteVersion("1.4", hasPropertyValues: false));
        }

        [Fact]
        public void Prueba10_Rama2_IntroducirPropertyValues_PROMUEVE_A_2_0()
        {
            Assert.Equal("2.0", SelectiveDesignSchema.ResolveWriteVersion("1.0", hasPropertyValues: true));
            Assert.Equal("2.0", SelectiveDesignSchema.ResolveWriteVersion(null, hasPropertyValues: true));
        }

        [Fact]
        public void Prueba10_Rama3_UnDocumentoYA_2x_SE_QUEDA_2x_AUNQUE_SE_QUITE_EL_ULTIMO_BINDING()
        {
            Assert.Equal("2.0", SelectiveDesignSchema.ResolveWriteVersion("2.0", hasPropertyValues: true));

            // La rama que hace que la promocion sea MONOTONA y no un balanceo:
            Assert.Equal("2.0", SelectiveDesignSchema.ResolveWriteVersion("2.0", hasPropertyValues: false));
            Assert.Equal("2.3", SelectiveDesignSchema.ResolveWriteVersion("2.3", hasPropertyValues: false));
        }

        [Fact]
        public void Prueba10_Rama4_MajorSuperiorAlSOPORTADO_ES_ERROR_Y_NO_DEGRADA()
        {
            var ex = Assert.Throws<System.InvalidOperationException>(
                () => SelectiveDesignSchema.ResolveWriteVersion("3.0", hasPropertyValues: false));

            Assert.Contains("3.0", ex.Message);
        }

        [Fact]
        public void Prueba10_LaInvarianteEsQueElResultadoNUNCA_BAJA_DE_MAJOR()
        {
            foreach (var stored in new[] { "1.0", "1.9", "2.0", "2.7" })
            {
                foreach (var bound in new[] { true, false })
                {
                    var escrito = SelectiveDesignSchema.ResolveWriteVersion(stored, bound);
                    Assert.True(
                        SchemaVersionPolicy.MajorOf(escrito) >= SchemaVersionPolicy.MajorOf(stored),
                        stored + " + " + bound + " -> " + escrito);
                }
            }
        }

        [Fact]
        public void ElBuildLEE_la_linea_2x_QueElMismoPromueve()
        {
            Assert.Equal(2, SelectivePalletDesignDocument.SupportedReadMajor);
            Assert.Equal("1.0", SelectivePalletDesignDocument.CurrentSchemaVersion);
            Assert.Equal("2.0", SelectivePalletDesignDocument.PromotedSchemaVersion);
        }

        // ================================================================ prueba 11 — el portador

        [Fact]
        public void Prueba11_LoadEditSave_CONSERVA_version_binding_literal_y_extensionData()
        {
            var store = new SelectivePalletDesignStore();

            var original = Vinculado(literalCongelado: 6.0);
            original.SchemaVersion = "2.4";
            original.ExtensionData = new Dictionary<string, JsonElement>
            {
                ["AlgoDeUnBuildPosterior"] = JsonDocument.Parse("42").RootElement,
            };

            var authored = store.Deserialize(store.Serialize(original));

            // el usuario edita OTRA cosa del diseño; el clearance efectivo que ve es el de la variable
            var editado = Diseno(clearance: 99.0);
            editado.Bays[0].Levels.Add(new SelectiveCell
            {
                Pallet = new Tarima { Frente = 48, Alto = 50 },
                PalletCount = 1,
                BeamId = "BEAM_A",
                BeamPeralte = 4.5,
            });

            var guardado = authored.WithDesign(editado);
            var releido = store.Deserialize(store.Serialize(guardado));

            Assert.Equal("2.4", releido.SchemaVersion);
            Assert.True(releido.IsBound(ProjectPropertyIds.SelectiveVerticalClearance));
            Assert.Equal(6.0, releido.VerticalClearance);
            Assert.True(releido.ExtensionData.ContainsKey("AlgoDeUnBuildPosterior"));
            Assert.Equal(2, releido.Bays[0].Levels.Count);
        }

        [Fact]
        public void Prueba11_ConBinding_ELLITERAL_AUTHORED_NO_SE_SOBRESCRIBE()
        {
            var authored = Vinculado(literalCongelado: 6.0);

            var guardado = authored.WithDesign(Diseno(clearance: 99.0));

            Assert.Equal(6.0, guardado.VerticalClearance);
        }

        [Fact]
        public void SinBinding_ELLITERAL_SI_SE_ACTUALIZA()
        {
            var authored = Documento(6.0);

            var guardado = authored.WithDesign(Diseno(clearance: 99.0));

            Assert.Equal(99.0, guardado.VerticalClearance);
        }

        [Fact]
        public void WithDesign_CONSERVA_LaIdentidadDelRack()
        {
            var guardado = Vinculado().WithDesign(Diseno(7.0));

            Assert.Equal(Guid1, guardado.Id);
            Assert.Equal("Rack 1", guardado.Name);
        }

        // ================================================================ PropertyValues

        [Fact]
        public void PropertyValuesSobreviveAlRoundTrip_ConSuKindYSuVariableId()
        {
            var store = new SelectivePalletDesignStore();

            var releido = store.Deserialize(store.Serialize(Vinculado()));

            var entrada = Assert.Single(releido.PropertyValues);
            Assert.Equal(ProjectPropertyIds.SelectiveVerticalClearanceToken, entrada.Key);
            Assert.Equal("projectVariable", entrada.Value.Kind);
            Assert.Equal(Guid2, entrada.Value.VariableId);
        }

        [Fact]
        public void UnDocumentoSinBindingNoEmitePropertyValues()
        {
            var json = new SelectivePalletDesignStore().Serialize(Documento());

            Assert.DoesNotContain("PropertyValues", json);
        }

        [Fact]
        public void ElBindingSeConsultaPorPropertyIdTipado()
        {
            var vinculado = Vinculado();

            Assert.True(vinculado.IsBound(ProjectPropertyIds.SelectiveVerticalClearance));
            Assert.True(vinculado.TryGetBinding(ProjectPropertyIds.SelectiveVerticalClearance, out var id));
            Assert.Equal(VariableId.Parse(Guid2), id);

            Assert.False(Documento().IsBound(ProjectPropertyIds.SelectiveVerticalClearance));
        }

        [Fact]
        public void HasPropertyValuesSeEvaluaSobreElDocumentoQueSeVaAESCRIBIR()
        {
            Assert.True(Vinculado().HasPropertyValues);
            Assert.False(Documento().HasPropertyValues);

            var sinEntradas = Documento();
            sinEntradas.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>();
            Assert.False(sinEntradas.HasPropertyValues);
        }

        // ================================================================ escritura pegajosa real

        [Fact]
        public void VincularYGuardarEnElMismoPaso_PROMUEVE()
        {
            var json = new SelectivePalletDesignStore().Serialize(Vinculado());

            Assert.Contains("\"SchemaVersion\":\"2.0\"", json);
        }

        [Fact]
        public void UnDocumentoNoVinculadoSIGUE_EN_1x_YLoAbreUnaVersionAnterior()
        {
            var json = new SelectivePalletDesignStore().Serialize(Documento());

            Assert.Contains("\"SchemaVersion\":\"1.0\"", json);
        }

        [Fact]
        public void UnDocumentoPROMOVIDO_SE_PUEDE_LEER_PorEsteBuild()
        {
            var store = new SelectivePalletDesignStore();

            var releido = store.Deserialize(store.Serialize(Vinculado()));

            Assert.Equal("2.0", releido.SchemaVersion);
        }

        [Fact]
        public void UnDocumentoDeMajorSUPERIOR_SE_RECHAZA_AlLeer()
        {
            var doc = Documento();
            doc.SchemaVersion = "3.0";
            var json = JsonSerializer.Serialize(doc);

            Assert.Throws<System.InvalidOperationException>(
                () => new SelectivePalletDesignStore().Deserialize(json));
        }

        // ================================================================ ExtensionData

        [Fact]
        public void ExtensionDataDelSelectivoSobreviveAlRoundTrip()
        {
            var store = new SelectivePalletDesignStore();
            var doc = Documento();
            var json = store.Serialize(doc);
            var conExtra = json.Substring(0, json.Length - 1) + ",\"CampoDeUnBuildPosterior\":\"x\"}";

            var releido = store.Deserialize(conExtra);

            Assert.NotNull(releido.ExtensionData);
            Assert.True(releido.ExtensionData.ContainsKey("CampoDeUnBuildPosterior"));
            Assert.Contains("CampoDeUnBuildPosterior", store.Serialize(releido));
        }

        /// <summary>
        /// El <c>ExtensionData</c> no es solo cortesia con builds futuros: G5 compara la autoridad authored
        /// COMPLETA, asi que un portador que lo perdiera produciria divergencias fantasma entre hermanas.
        /// </summary>
        [Fact]
        public void WithDesign_CONSERVA_ExtensionData()
        {
            var authored = Documento();
            authored.ExtensionData = new Dictionary<string, JsonElement>
            {
                ["Futuro"] = JsonDocument.Parse("\"v\"").RootElement,
            };

            Assert.True(authored.WithDesign(Diseno(7.0)).ExtensionData.ContainsKey("Futuro"));
        }

        // ================================================================ alcance

        [Fact]
        public void C4_8_1_NO_APLICA_AQUI_ElInicializadorHistoricoSeCONSERVA()
        {
            var recienDeserializado = JsonSerializer.Deserialize<SelectivePalletDesignDocument>("{}");

            Assert.Equal(SelectivePalletDesignDocument.CurrentSchemaVersion, recienDeserializado.SchemaVersion);
        }
    }
}
