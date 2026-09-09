using System.Text.Json;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-47 gate G2 — el registro del dibujo como documento persistido de pleno derecho, y la regla que
    /// hace implementable todo lo demas (C4.8-1).
    ///
    /// <para>
    /// La regla, y por que existe: los ocho DTO versionados del repositorio declaran
    /// <c>SchemaVersion { get; set; } = CurrentSchemaVersion</c>, y <c>System.Text.Json</c> NO toca una
    /// propiedad ausente del JSON — conserva el inicializador. Con ese patron, un registro cuyo JSON no
    /// traiga el campo deserializa con <c>"1.0"</c>, <b>indistinguible</b> de uno que lo declaro. Eso es un
    /// valor por defecto convirtiendo un estado DESCONOCIDO en un exito, que es exactamente lo que la
    /// doctrina de fallo de autoridad prohibe.
    /// </para>
    /// <para>
    /// Las pruebas 41 y 42 se leen JUNTAS y ninguna vale por separado: la 41 sola la satisface tambien una
    /// implementacion que rechace TODO documento. Emparejada con la 42 —que exige que un <c>"1.0"</c>
    /// explicito SI se lea— lo que queda fijado es la <b>distincion</b>.
    /// </para>
    /// <para>
    /// Alcance: C4.8-1 gobierna <b>solo</b> este documento, que nace en ID22A y por tanto siempre escribe su
    /// version. Los documentos historicos nacieron antes de que hubiera version, para ellos "ausente"
    /// significa legado, y ni su patron ni <c>SchemaVersionPolicy</c> se tocan.
    /// </para>
    /// </summary>
    public class ProjectVariablesDocumentTests
    {
        private static readonly string GuidA = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";
        private static readonly string GuidB = "8a1d4e77-2c93-4b60-8f15-6e0b93a7c221";

        private static string Json(string schemaVersionProperty, string variables = "[]", string extra = "")
            => "{" + schemaVersionProperty + "\"Variables\":" + variables + extra + "}";

        private static string UnaVariable(string type = "\"Length\"", string kind = "\"literal\"")
            => "[{\"VariableId\":\"" + GuidA + "\",\"Name\":\"Holgura\",\"Type\":" + type +
               ",\"Definition\":{\"Kind\":" + kind + ",\"Value\":6.0}}]";

        // ================================================================ prueba 42 (centinela POSITIVO)

        [Fact]
        public void Prueba42_PresenteConSchemaVersionExplicita_ES_LEGIBLE()
        {
            var result = new ProjectVariablesStore().Deserialize(Json("\"SchemaVersion\":\"1.0\","));

            Assert.Equal(ProjectVariablesReadOutcome.Readable, result.Outcome);
            Assert.NotNull(result.Document);
            Assert.Equal("1.0", result.Document.SchemaVersion);
            Assert.True(result.CanWrite);
        }

        // ================================================================ prueba 41 (centinela NEGATIVO)

        [Fact]
        public void Prueba41_PresenteSinLaPropiedadSchemaVersion_ES_ERROR_DURO_SIN_ESCRITURA()
        {
            var result = new ProjectVariablesStore().Deserialize(Json(string.Empty));

            Assert.Equal(ProjectVariablesReadOutcome.PresentButUnreadable, result.Outcome);
            Assert.Null(result.Document);
            Assert.False(result.CanWrite);
            Assert.False(string.IsNullOrWhiteSpace(result.Error));
        }

        [Theory]
        [InlineData("\"SchemaVersion\":null,")]
        [InlineData("\"SchemaVersion\":\"\",")]
        [InlineData("\"SchemaVersion\":\"   \",")]
        [InlineData("\"SchemaVersion\":\"no-es-version\",")]
        [InlineData("\"SchemaVersion\":\"x.y\",")]
        [InlineData("\"SchemaVersion\":\"0.9\",")]
        public void Prueba41_SchemaVersionNulaEnBlancoONoParseable_ES_ERROR_DURO(string schemaVersionProperty)
        {
            var result = new ProjectVariablesStore().Deserialize(Json(schemaVersionProperty));

            Assert.Equal(ProjectVariablesReadOutcome.PresentButUnreadable, result.Outcome);
            Assert.Null(result.Document);
            Assert.False(result.CanWrite);
        }

        /// <summary>
        /// La pareja 41+42 en una sola asercion: si esta prueba pasa, la implementacion NO es un
        /// "rechaza todo" y NO es un "acepta todo". Es la distincion en si.
        /// </summary>
        [Fact]
        public void MissingNoEsLoMismoQueExplicito_1_0()
        {
            var store = new ProjectVariablesStore();

            var ausente = store.Deserialize(Json(string.Empty));
            var explicita = store.Deserialize(Json("\"SchemaVersion\":\"1.0\","));

            Assert.NotEqual(explicita.Outcome, ausente.Outcome);
            Assert.Equal(ProjectVariablesReadOutcome.PresentButUnreadable, ausente.Outcome);
            Assert.Equal(ProjectVariablesReadOutcome.Readable, explicita.Outcome);
        }

        // ================================================================ prueba 30

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("{no es json")]
        [InlineData("[]")]
        [InlineData("null")]
        public void Prueba30_ContenidoCorruptoOVacio_ES_ERROR_DURO_NO_DOCUMENTO_VACIO(string json)
        {
            var result = new ProjectVariablesStore().Deserialize(json);

            Assert.Equal(ProjectVariablesReadOutcome.PresentButUnreadable, result.Outcome);
            Assert.Null(result.Document);
            Assert.False(result.CanWrite);
        }

        [Fact]
        public void Prueba30_LaAUSENCIA_DE_LA_ENTRADA_SI_ES_REGISTRO_VACIO()
        {
            var result = ProjectVariablesReadResult.Absent();

            Assert.Equal(ProjectVariablesReadOutcome.Absent, result.Outcome);
            Assert.NotNull(result.Document);
            Assert.Empty(result.Document.Variables);
            Assert.True(result.CanWrite);
        }

        // ================================================================ prueba 1 — los tres errores duros

        [Fact]
        public void Prueba1_MajorSuperiorAlSoportado_ES_ERROR_Y_NO_SE_ESCRIBE()
        {
            var result = new ProjectVariablesStore().Deserialize(Json("\"SchemaVersion\":\"2.0\","));

            Assert.Equal(ProjectVariablesReadOutcome.IncompatibleMajor, result.Outcome);
            Assert.Null(result.Document);
            Assert.False(result.CanWrite);
        }

        [Fact]
        public void Prueba1_VariableTypeDesconocido_ES_ERROR_DURO()
        {
            var result = new ProjectVariablesStore()
                .Deserialize(Json("\"SchemaVersion\":\"1.0\",", UnaVariable(type: "\"Volumen\"")));

            Assert.Equal(ProjectVariablesReadOutcome.PresentButUnreadable, result.Outcome);
            Assert.Null(result.Document);
        }

        [Fact]
        public void Prueba1_DefinitionKindDesconocido_ES_ERROR_DURO()
        {
            var result = new ProjectVariablesStore()
                .Deserialize(Json("\"SchemaVersion\":\"1.0\",", UnaVariable(kind: "\"expression\"")));

            Assert.Equal(ProjectVariablesReadOutcome.PresentButUnreadable, result.Outcome);
            Assert.Null(result.Document);
        }

        [Fact]
        public void Prueba1_VariableIdQueNoEsGuid_ES_ERROR_DURO()
        {
            var json = "{\"SchemaVersion\":\"1.0\",\"Variables\":[{\"VariableId\":\"no-guid\"," +
                       "\"Name\":\"Holgura\",\"Type\":\"Length\",\"Definition\":{\"Kind\":\"literal\",\"Value\":6.0}}]}";

            Assert.Equal(
                ProjectVariablesReadOutcome.PresentButUnreadable,
                new ProjectVariablesStore().Deserialize(json).Outcome);
        }

        [Fact]
        public void Prueba1_UnRegistroLegibleSeProyectaAlModeloPuroDeG1()
        {
            var result = new ProjectVariablesStore()
                .Deserialize(Json("\"SchemaVersion\":\"1.0\",", UnaVariable()));

            var variable = Assert.Single(result.Document.ToProjectVariables());

            Assert.Equal(VariableId.Parse(GuidA), variable.Id);
            Assert.Equal("Holgura", variable.Name);
            Assert.Equal(VariableType.Length, variable.Type);
            Assert.Equal(6.0, variable.Definition.LiteralValue);
        }

        // ================================================================ CREATE / WRITE

        [Fact]
        public void UnRegistroNUEVO_SeEstampaExplicitamenteCon_1_0()
        {
            var documento = ProjectVariablesDocument.CreateNew();

            Assert.Equal("1.0", documento.SchemaVersion);
            Assert.Contains("\"SchemaVersion\":\"1.0\"", new ProjectVariablesStore().Serialize(documento));
        }

        [Fact]
        public void EscribirEstampaLaVersionAunqueElDocumentoLlegueSinElla()
        {
            var documento = new ProjectVariablesDocument();

            var json = new ProjectVariablesStore().Serialize(documento);

            Assert.Contains("\"SchemaVersion\":\"1.0\"", json);
            Assert.Equal("1.0", documento.SchemaVersion);
        }

        [Fact]
        public void UnMinorMayorDelMismoMajorSePRESERVA_AlReescribir()
        {
            var store = new ProjectVariablesStore();
            var leido = store.Deserialize(Json("\"SchemaVersion\":\"1.7\",")).Document;

            Assert.Equal("1.7", leido.SchemaVersion);
            Assert.Contains("\"SchemaVersion\":\"1.7\"", store.Serialize(leido));
        }

        // ================================================================ ExtensionData

        [Fact]
        public void ExtensionDataDeLaRaizSobreviveAlRoundTrip()
        {
            var store = new ProjectVariablesStore();
            var json = Json("\"SchemaVersion\":\"1.1\",", "[]", ",\"AlgoDeUnBuildPosterior\":42");

            var leido = store.Deserialize(json);
            Assert.Equal(ProjectVariablesReadOutcome.Readable, leido.Outcome);

            var reescrito = store.Serialize(leido.Document);
            Assert.Contains("AlgoDeUnBuildPosterior", reescrito);
            Assert.Contains("\"SchemaVersion\":\"1.1\"", reescrito);
        }

        [Fact]
        public void UnRoundTripCompletoConservaLasVariables()
        {
            var store = new ProjectVariablesStore();
            var original = ProjectVariablesDocument.CreateNew();
            original.Variables.Add(new ProjectVariableDocument
            {
                VariableId = GuidB,
                Name = "Holgura estandar",
                Type = "Length",
                Definition = new ProjectVariableDefinitionDocument { Kind = "literal", Value = 7.5 },
            });

            var leido = store.Deserialize(store.Serialize(original));

            Assert.Equal(ProjectVariablesReadOutcome.Readable, leido.Outcome);
            var variable = Assert.Single(leido.Document.ToProjectVariables());
            Assert.Equal(VariableId.Parse(GuidB), variable.Id);
            Assert.Equal(7.5, variable.Definition.LiteralValue);
        }

        // ================================================================ alcance de C4.8-1

        [Fact]
        public void C4_8_1_NoAlcanzaALosDTO_HISTORICOS()
        {
            // El patron historico se conserva: una propiedad ausente sigue significando legado ahi, porque
            // esos documentos nacieron antes de que hubiera version. Solo el registro cambia de regla.
            var selectivo = JsonSerializer.Deserialize<SelectivePalletDesignDocument>("{}");

            Assert.Equal(SelectivePalletDesignDocument.CurrentSchemaVersion, selectivo.SchemaVersion);
        }

        [Fact]
        public void ElRegistroNoLlevaInicializadorDeVersion()
        {
            Assert.Null(new ProjectVariablesDocument().SchemaVersion);
        }
    }
}
