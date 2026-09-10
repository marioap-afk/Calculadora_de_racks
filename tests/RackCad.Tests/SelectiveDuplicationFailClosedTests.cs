using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-47 gate G14 — duplicar es indivisible, o no se duplica.
    ///
    /// <para>
    /// El re-estampado podía fallar y devolver el JSON ORIGINAL: la copia salía con un RackId nuevo en el
    /// sobre y la identidad vieja dentro. Dos racks con identidad semántica mezclada, y el defecto solo se ve
    /// la primera vez que alguien abre la copia y guarda —momento en el que ya no hay forma de saber cuál era
    /// cuál—. Con vínculos de proyecto encima, esa copia además arrastra un binding cuyo dueño ya no es quien
    /// dice ser.
    /// </para>
    /// <para>
    /// Así que la transformación que puede fallar se decide ANTES de materializar nada: o sale entera —sobre
    /// nuevo, identidad interior re-estampada, vínculo y literal congelado y versión y campos desconocidos
    /// intactos— o no sale ninguna copia y el usuario lo ve.
    /// </para>
    /// <para>
    /// Y la asimetría que hay que decir en voz alta: <b>la identidad del RACK cambia, la de la VARIABLE no</b>.
    /// Una copia es otro rack; la variable de proyecto que gobierna su holgura sigue siendo la misma variable
    /// del mismo dibujo.
    /// </para>
    /// </summary>
    public class SelectiveDuplicationFailClosedTests
    {
        private const string RackA = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";
        private const string CopyId = "9d5e2a10-7b44-4c81-a0f3-51c6e7b29a88";
        private const string VarId = "8a1d4e77-2c93-4b60-8f15-6e0b93a7c221";
        private const string Token = ProjectPropertyIds.SelectiveVerticalClearanceToken;

        private static SelectivePalletDesign Diseno(double clearance)
        {
            var design = new SelectivePalletDesign { VerticalClearance = clearance, PalletDepth = 48.0 };
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

        private static string Json(bool bound, string extensionKey = null)
        {
            var doc = SelectivePalletDesignDocument.From(Diseno(6.0), RackA, "Rack original");

            if (bound)
            {
                doc.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument>
                {
                    [Token] = SelectivePropertyValueDocument.ToProjectVariable(VarId),
                };
                doc.SchemaVersion = SelectivePalletDesignDocument.PromotedSchemaVersion;
            }

            if (extensionKey != null)
            {
                doc.ExtensionData = new Dictionary<string, JsonElement>
                {
                    [extensionKey] = JsonDocument.Parse("7").RootElement,
                };
            }

            return new SelectivePalletDesignStore().Serialize(doc);
        }

        private static SelectivePalletDesignDocument Copia(bool bound, string extensionKey = null)
        {
            var result = SelectiveAuthoredRestamp.Restamp(Json(bound, extensionKey), CopyId, "Rack copia");

            Assert.True(result.IsSuccess);
            return new SelectivePalletDesignStore().Deserialize(result.DesignJson);
        }

        // ================================================================ 1-3: la identidad que SÍ cambia

        [Fact]
        public void UNA_COPIA_SIN_VINCULO_ESTRENA_IDENTIDAD()
        {
            var copia = Copia(bound: false);

            Assert.Equal(CopyId, copia.Id);
            Assert.Equal("Rack copia", copia.Name);
        }

        [Fact]
        public void UNA_COPIA_VINCULADA_ESTRENA_IDENTIDAD_DE_RACK()
        {
            var copia = Copia(bound: true);

            Assert.Equal(CopyId, copia.Id);
            Assert.NotEqual(RackA, copia.Id);
            Assert.Equal("Rack copia", copia.Name);
        }

        /// <summary>La identidad del RACK cambia; la de la VARIABLE no. Son dos cosas distintas.</summary>
        [Fact]
        public void UNA_COPIA_VINCULADA_CONSERVA_LA_MISMA_VARIABLE()
        {
            var copia = Copia(bound: true);

            Assert.True(copia.TryGetBinding(ProjectPropertyIds.SelectiveVerticalClearance, out var id));
            Assert.Equal(VariableId.Parse(VarId), id);
        }

        // ================================================================ 4-7: lo que sobrevive intacto

        [Fact]
        public void LA_COPIA_CONSERVA_EL_LITERAL_CONGELADO()
        {
            Assert.Equal(6.0, Copia(bound: true).VerticalClearance);
        }

        [Fact]
        public void LA_COPIA_CONSERVA_LA_VERSION_PROMOCIONADA()
        {
            Assert.Equal(SelectivePalletDesignDocument.PromotedSchemaVersion, Copia(bound: true).SchemaVersion);
        }

        [Fact]
        public void LA_COPIA_CONSERVA_LOS_CAMPOS_DESCONOCIDOS()
        {
            var copia = Copia(bound: true, extensionKey: "DeUnBuildPosterior");

            Assert.NotNull(copia.ExtensionData);
            Assert.True(copia.ExtensionData.ContainsKey("DeUnBuildPosterior"));
        }

        [Fact]
        public void LA_COPIA_CONSERVA_EL_RESTO_DEL_DISENO()
        {
            var copia = Copia(bound: false);

            Assert.Single(copia.Bays);
            Assert.Equal(48.0, copia.PalletDepth);
        }

        // ================================================================ 8, 9: fallar sin copiar

        /// <summary>
        /// Lo que NUNCA puede volver: un diseño ilegible que devuelve el JSON original. Esa copia llevaría el
        /// RackId nuevo por fuera y el viejo por dentro.
        /// </summary>
        [Fact]
        public void UN_DISENO_ILEGIBLE_NO_DEVUELVE_EL_ORIGINAL()
        {
            var result = SelectiveAuthoredRestamp.Restamp("{ esto no es json", CopyId, "Rack copia");

            Assert.False(result.IsSuccess);
            Assert.Null(result.DesignJson);
            Assert.False(string.IsNullOrWhiteSpace(result.Error));
        }

        [Fact]
        public void UN_DISENO_AUSENTE_TAMPOCO_PRODUCE_COPIA()
        {
            Assert.False(SelectiveAuthoredRestamp.Restamp(null, CopyId, "Rack copia").IsSuccess);
            Assert.False(SelectiveAuthoredRestamp.Restamp("   ", CopyId, "Rack copia").IsSuccess);
        }

        /// <summary>Un MAJOR del futuro es ilegible para este build, y no se copia a ciegas.</summary>
        [Fact]
        public void UN_MAJOR_DEL_FUTURO_NO_PRODUCE_COPIA()
        {
            // Escrito a mano: el store lee sin distinguir mayusculas, y la guarda de version corre ANTES que
            // cualquier otra comprobacion, asi que esto mide exactamente el major y nada mas.
            var futuro = "{\"schemaVersion\":\"9.0\",\"id\":\"" + RackA + "\",\"name\":\"Rack original\",\"bays\":[]}";

            Assert.False(SelectiveAuthoredRestamp.Restamp(futuro, CopyId, "Rack copia").IsSuccess);
        }

        [Fact]
        public void EL_RESULTADO_TIPADO_DISTINGUE_LOS_DOS_ESTADOS()
        {
            Assert.True(RestampResult.Success("{}").IsSuccess);
            Assert.False(RestampResult.Failure("no").IsSuccess);
            Assert.Null(RestampResult.Failure("no").DesignJson);
        }

        // ================================================================ guardas de fuente

        private static DirectoryInfo RepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);

            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "RackCad.sln")))
            {
                dir = dir.Parent;
            }

            Assert.NotNull(dir);
            return dir;
        }

        private static string Plugin(params string[] relative)
            => File.ReadAllText(Path.Combine(
                new[] { RepoRoot().FullName, "src", "RackCad.Plugin" }.Concat(relative).ToArray()));

        private static int At(string source, string token)
        {
            var at = source.IndexOf(token, StringComparison.Ordinal);
            Assert.True(at >= 0, "no se encontró: " + token);
            return at;
        }

        [Fact]
        public void GUARDA_EL_CONTRATO_DE_RESTAMP_ES_TIPADO()
        {
            Assert.Contains("RestampResult RestampDesign(", Plugin("KindHandlers", "IRackKindHandler.cs"));
        }

        [Fact]
        public void GUARDA_EL_HANDLER_SELECTIVO_DELEGA_EN_LA_CAPA_PURA()
        {
            Assert.Contains("SelectiveAuthoredRestamp.Restamp(", Plugin("KindHandlers", "SelectiveKindHandler.cs"));
        }

        /// <summary>
        /// La regla entera en una guarda: el helper compartido ya no tiene un <c>catch</c> que devuelva el
        /// original. Un fallo de re-estampado no se degrada a "copia con la identidad vieja".
        /// </summary>
        [Fact]
        public void GUARDA_EL_RESTAMP_COMPARTIDO_NO_TIENE_MEJOR_ESFUERZO()
        {
            var source = Plugin("RackEnvelopeRestamp.cs");

            // La palabra suelta aparece en la documentacion que explica lo que se retiro; lo que no puede
            // volver es un manejador real.
            Assert.DoesNotContain("catch (", source);
            Assert.DoesNotContain("return designJson;", source);
            Assert.Contains("RestampResult RestampEnvelope(", source);
        }

        [Fact]
        public void GUARDA_RACKDUPLICAR_DECIDE_ANTES_DE_CLONAR()
        {
            var source = Plugin("RackDuplicarCommands.cs");

            Assert.True(At(source, "RestampEnvelope(") < At(source, "RackCloner.CloneDefinition"));
            Assert.Contains("IsSuccess", source);
        }

        [Fact]
        public void GUARDA_RACKLAYOUT_DECIDE_ANTES_DE_CLONAR()
        {
            var source = Plugin("RackLayoutCommands.cs");

            Assert.True(At(source, "RestampEnvelope(") < At(source, "RackCloner.CloneDefinition"));
            Assert.Contains("IsSuccess", source);
        }

        /// <summary>Ningún camino de copia se queda con el payload de origen cuando el re-estampado falla.</summary>
        [Fact]
        public void GUARDA_NINGUN_CAMINO_DE_COPIA_CAE_AL_PAYLOAD_DE_ORIGEN()
        {
            foreach (var file in new[] { "RackDuplicarCommands.cs", "RackLayoutCommands.cs" })
            {
                var source = Plugin(file);

                Assert.DoesNotContain("source.Payload, copyName)", source.Replace("RestampEnvelope(source.Payload, copyName)", string.Empty));
                Assert.DoesNotContain("CloneDefinition(database, transaction, source.DefinitionId, copyName, source.Payload", source);
                Assert.DoesNotContain("CloneDefinition(database, transaction, seed.DefinitionId, copyName, seed.Payload", source);
            }
        }
    }
}
