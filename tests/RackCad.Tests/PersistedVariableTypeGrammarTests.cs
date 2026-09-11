using System.Collections.Generic;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-48 gate G4A.1 — el LENGUAJE del token de tipo persistido, y la composicion real de las dos
    /// autoridades que gobiernan un registro.
    ///
    /// <para>
    /// G4A centralizo el mapping «token persistido -> VariableType» en una sola primitiva, que era el
    /// objetivo correcto, pero lo implemento con <c>Enum.TryParse</c>. Eso NO es la politica que I-47
    /// escribio: <c>Enum.TryParse</c> acepta la representacion NUMERICA del enum, recorta espacios y
    /// admite listas separadas por coma. La regla historica del store era una comparacion textual contra
    /// el nombre declarado, y nada mas:
    /// </para>
    /// <code>
    /// string.Equals(type, VariableType.Length.ToString(), StringComparison.OrdinalIgnoreCase)
    /// </code>
    /// <para>
    /// Centralizar una politica no autoriza a ampliarla. Un documento cuyo <c>Type</c> vale <c>"1"</c> era
    /// ILEGIBLE antes de I-48 y debe seguir siendolo: el token es un contrato de persistencia, y ensancharlo
    /// significa que esta build acepta documentos que la anterior rechazaba, sin que nadie lo decidiera.
    /// </para>
    /// <para>
    /// La segunda mitad de este archivo prueba algo que un test de unidad no puede: que
    /// <b>persistence-readable y semantic-usable son dos autoridades distintas EN COMPOSICION</b>. Fabricar
    /// un <c>ReadResult.Readable(...)</c> a mano demuestra la segunda, pero da por supuesta la primera. Aqui
    /// el documento recorre el store de verdad.
    /// </para>
    /// </summary>
    public class PersistedVariableTypeGrammarTests
    {
        private const string GuidA = "11111111-1111-1111-1111-111111111111";
        private const string GuidB = "22222222-2222-2222-2222-222222222222";

        private static ProjectVariableDocument Entry(string guid, string type, double value = 6.0)
            => new ProjectVariableDocument
            {
                VariableId = guid,
                Name = "Holgura",
                Type = type,
                Definition = new ProjectVariableDefinitionDocument { Kind = "literal", Value = value },
            };

        private static ProjectVariablesDocument Document(params ProjectVariableDocument[] entries)
        {
            var document = ProjectVariablesDocument.CreateNew();
            document.Variables = new List<ProjectVariableDocument>(entries);
            return document;
        }

        /// <summary>Recorre el limite de persistencia de verdad: documento -> JSON -> store.</summary>
        private static ProjectVariablesReadResult RoundTrip(ProjectVariablesDocument document)
        {
            var store = new ProjectVariablesStore();
            return store.Deserialize(store.Serialize(document));
        }

        // ================================================================ gramatica del token

        [Theory]
        [InlineData("Length")]
        [InlineData("length")]
        [InlineData("LENGTH")]
        public void ElNombreDeclaradoSeAceptaEnCualquierCaja(string token)
        {
            Assert.True(VariableTypes.TryParseToken(token, out var type));
            Assert.Equal(VariableType.Length, type);
        }

        [Theory]
        [InlineData("1")]              // representacion NUMERICA del enum
        [InlineData(" Length ")]       // espacios: Enum.TryParse los recorta, la regla historica no
        [InlineData("Length,Length")]  // lista separada por coma
        [InlineData("9001")]
        [InlineData("Dimension")]
        [InlineData("")]
        [InlineData(null)]
        public void LoQueNoEsElNombreDeclaradoSeRechaza(string token)
        {
            Assert.False(VariableTypes.TryParseToken(token, out var type));
            Assert.Equal(default, type);
        }

        [Fact]
        public void ElMappingYIsSupportedNoSePuedenContradecir()
        {
            // Una sola tabla: lo que el mapping acepta es exactamente lo que IsSupported reconoce.
            Assert.True(VariableTypes.TryParseToken(VariableType.Length.ToString(), out var soportado));
            Assert.True(VariableTypes.IsSupported(soportado));

            Assert.False(VariableTypes.IsSupported((VariableType)9001));
            Assert.False(VariableTypes.TryParseToken("9001", out _));
        }

        // ================================================================ el store conserva su lenguaje

        [Theory]
        [InlineData("Length")]
        [InlineData("length")]
        public void UnTypeConElNombreDeclaradoSigueSiendoLegible(string token)
        {
            var read = RoundTrip(Document(Entry(GuidA, token)));

            Assert.Equal(ProjectVariablesReadOutcome.Readable, read.Outcome);
        }

        [Theory]
        [InlineData("1")]
        [InlineData(" Length ")]
        [InlineData("Length,Length")]
        public void UnTypeQueNoEsElNombreDeclaradoNoEsLegible(string token)
        {
            // RED sobre 8a56e330: Enum.TryParse aceptaba los tres y el documento pasaba a Readable.
            var read = RoundTrip(Document(Entry(GuidA, token)));

            Assert.NotEqual(ProjectVariablesReadOutcome.Readable, read.Outcome);
            Assert.Equal(ProjectVariablesReadOutcome.PresentButUnreadable, read.Outcome);
        }

        // ================================================================ las dos autoridades, en composicion

        [Fact]
        public void UnTypeEnMinusculasAtraviesaStoreYAcreditacionYConservaSuTipo()
        {
            var read = RoundTrip(Document(Entry(GuidA, "length", 7.5)));
            Assert.Equal(ProjectVariablesReadOutcome.Readable, read.Outcome);

            var accreditation = UsableProjectVariablesRegistry.Accredit(read);

            Assert.True(accreditation.IsUsable);
            Assert.True(accreditation.Registry.TryGetTarget(VariableId.Parse(GuidA), out var target));
            Assert.Equal(VariableType.Length, target.VariableType);
            Assert.Equal(7.5, target.LiteralValue);
        }

        [Fact]
        public void UnDocumentoPERSISTIDO_ConVariableIdDuplicado_EsLegibleParaElStorePeroNoSeAcredita()
        {
            // Esta es la prueba que demuestra las DOS autoridades. No fabrica el ReadResult: el documento
            // se serializa y se vuelve a leer por el store real.
            var read = RoundTrip(Document(Entry(GuidA, "Length", 6.0), Entry(GuidA, "Length", 8.0)));

            // AUTORIDAD DE PERSISTENCIA: el store NO valida unicidad y no se le cambia para que lo haga.
            Assert.Equal(ProjectVariablesReadOutcome.Readable, read.Outcome);
            Assert.NotNull(read.Document);
            Assert.Equal(2, read.Document.Variables.Count);

            // AUTORIDAD DE IDENTIDAD: falla despues, y no devuelve nada utilizable.
            var accreditation = UsableProjectVariablesRegistry.Accredit(read);

            Assert.False(accreditation.IsUsable);
            Assert.Equal(ProjectVariablesAccreditationOutcome.AmbiguousIdentity, accreditation.Outcome);
            Assert.Null(accreditation.Registry);
            Assert.Contains(GuidA, accreditation.Error);
        }

        [Fact]
        public void UnDocumentoPERSISTIDO_ConIdsDistintos_AtraviesaLasDosAutoridades()
        {
            var read = RoundTrip(Document(Entry(GuidA, "Length"), Entry(GuidB, "Length")));
            Assert.Equal(ProjectVariablesReadOutcome.Readable, read.Outcome);

            var accreditation = UsableProjectVariablesRegistry.Accredit(read);

            Assert.True(accreditation.IsUsable);
            Assert.Equal(2, accreditation.Registry.Count);
        }

        // ================================================================ la factory consume la MISMA primitiva

        [Fact]
        public void UnaLecturaFabricadaConTypeNumericoNoSePuedeAcreditar()
        {
            // No sustituye al test real del store: demuestra que la factory productiva consume el mismo
            // mapping, asi que un estado que el store jamas produciria tampoco se acredita aqui.
            var read = ProjectVariablesReadResult.Readable(Document(Entry(GuidA, "1")));

            var accreditation = UsableProjectVariablesRegistry.Accredit(read);

            Assert.False(accreditation.IsUsable);
            Assert.Equal(ProjectVariablesAccreditationOutcome.NotReadable, accreditation.Outcome);
            Assert.Null(accreditation.Registry);
        }

        // ================================================================ I-48 G4D: la TERCERA superficie

        /// <summary>
        /// G4D. El camino REAL y completo: documento -> JSON -> store -> <c>Readable</c> -> proyeccion. El tipo
        /// que sale es el que estaba PERSISTIDO, no uno fabricado.
        ///
        /// <para>
        /// El token va en minuscula a proposito: si la proyeccion respetase el token pero con una regla propia
        /// mas estrecha, este caso -que el store SI acepta- se rompiria. Y si lo fabricase, pasaria sin mirarlo.
        /// La prueba solo pasa cuando las dos superficies comparten la misma primitiva.
        /// </para>
        /// </summary>
        [Fact]
        public void EL_CAMINO_REAL_PROYECTA_EL_TIPO_PERSISTIDO_Y_NO_UNO_FABRICADO()
        {
            var read = RoundTrip(Document(Entry(GuidA, "length", 7.5)));

            Assert.Equal(ProjectVariablesReadOutcome.Readable, read.Outcome);

            var variable = Assert.Single(read.Document.ToProjectVariables());

            Assert.Equal(VariableType.Length, variable.Type);
            Assert.Equal(VariableId.Parse(GuidA), variable.Id);
            Assert.Equal("Holgura", variable.Name);
            Assert.Equal(7.5, variable.Definition.LiteralValue);
        }

        /// <summary>
        /// G4D. Un documento FABRICADO con un tipo que esta build no soporta es una violacion de invariante, no
        /// un estado que reinterpretar.
        ///
        /// <para>
        /// El store jamas produce esto: rechaza el documento entero al leerlo. Por eso hay que fabricarlo — y
        /// por eso la respuesta correcta es fallar ruidosamente. Devolver <c>Length</c> convertiria un documento
        /// inconsistente en un registro que parece sano, y el rack que lo consumiese tomaria un numero cuyo
        /// significado nadie ha declarado.
        /// </para>
        /// </summary>
        [Fact]
        public void UN_TIPO_NO_SOPORTADO_EN_UN_DOCUMENTO_FABRICADO_FALLA_RUIDOSAMENTE()
        {
            var document = Document(Entry(GuidA, "unsupported-type"));

            var error = Assert.Throws<System.InvalidOperationException>(() => document.ToProjectVariables());

            // Con contexto suficiente para localizar el documento inconsistente.
            Assert.Contains(GuidA, error.Message);
            Assert.Contains("unsupported-type", error.Message);
        }

        /// <summary>
        /// G4D. La forma NUMERICA del enum tampoco se proyecta. Es la misma trampa de <c>Enum.TryParse</c> que
        /// G4A.1 saco del store, y aqui protege la tercera superficie: si la proyeccion tuviera su propia tabla,
        /// podria volver a aceptarla sin que el store lo notase.
        /// </summary>
        [Fact]
        public void EL_TOKEN_NUMERICO_NO_SE_PROYECTA()
        {
            var document = Document(Entry(GuidA, "1"));

            var error = Assert.Throws<System.InvalidOperationException>(() => document.ToProjectVariables());

            Assert.Contains("'1'", error.Message);
        }

        /// <summary>G4D. Espacios alrededor y lista con coma: rechazados igual que en el store.</summary>
        [Theory]
        [InlineData(" Length ")]
        [InlineData("Length,Length")]
        [InlineData("Dimension")]
        [InlineData("")]
        public void LOS_TOKENS_QUE_EL_STORE_RECHAZA_TAMPOCO_SE_PROYECTAN(string token)
        {
            var document = Document(Entry(GuidA, token));

            Assert.Throws<System.InvalidOperationException>(() => document.ToProjectVariables());
        }

        [Fact]
        public void UN_TIPO_AUSENTE_TAMPOCO_SE_PROYECTA()
        {
            var document = Document(Entry(GuidA, null));

            Assert.Throws<System.InvalidOperationException>(() => document.ToProjectVariables());
        }

        // ================================================================ la matriz compartida

        /// <summary>
        /// G4D — la prueba de AUTORIDAD COMPARTIDA. Para cada token, el veredicto del store y el de la
        /// proyeccion se miden en la MISMA prueba, asi que no pueden divergir sin que salte.
        ///
        /// <para>
        /// Es la propiedad que <c>V7-R04</c> exige y que ninguna de las dos superficies puede garantizar sola:
        /// lo que el store declara legible, la proyeccion lo proyecta; lo que el store rechaza, la proyeccion
        /// -si alguien fabrica ese documento- falla ruidosamente. Ni una tabla mas, ni una excepcion menos.
        /// </para>
        /// </summary>
        [Theory]
        [InlineData("Length", true)]
        [InlineData("length", true)]
        [InlineData("LENGTH", true)]
        [InlineData("1", false)]
        [InlineData(" Length ", false)]
        [InlineData("Length,Length", false)]
        [InlineData("Dimension", false)]
        [InlineData("", false)]
        public void EL_STORE_Y_LA_PROYECCION_NO_MANTIENEN_SEMANTICAS_DIVERGENTES(string token, bool aceptado)
        {
            var document = Document(Entry(GuidA, token));

            // 1) el veredicto del store, recorriendo el limite de persistencia de verdad.
            var read = RoundTrip(document);
            var legible = read.Outcome == ProjectVariablesReadOutcome.Readable;

            Assert.Equal(aceptado, legible);

            // 2) el de la proyeccion, sobre el MISMO token.
            if (aceptado)
            {
                Assert.Equal(VariableType.Length, Assert.Single(read.Document.ToProjectVariables()).Type);
                return;
            }

            // El store no entrego documento, asi que la proyeccion se ejercita sobre el fabricado: es la unica
            // forma de alcanzar el estado, y tiene que ser fail-loud.
            Assert.Throws<System.InvalidOperationException>(() => document.ToProjectVariables());
        }
    }
}
