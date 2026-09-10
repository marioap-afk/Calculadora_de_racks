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
    }
}
