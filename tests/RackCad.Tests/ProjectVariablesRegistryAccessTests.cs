using RackCad.Application.Persistence;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-47 gate G7 — la frontera entre lo que el dibujo ALMACENA y lo que el registro SIGNIFICA.
    ///
    /// <para>
    /// El Plugin es el unico que puede abrir un diccionario de AutoCAD, y ninguna suite lo carga (ADR-0003).
    /// Por eso la mitad fisica se reduce a lo minimo —buscar la entrada y reensamblar el texto— y todo lo
    /// demas vive aqui: que un payload ausente es un registro vacio, que uno presente e ilegible falla
    /// cerrado, y que despues de una lectura fallida NO se escribe.
    /// </para>
    /// <para>
    /// La distincion que tiene que sobrevivir a esa frontera es <c>ABSENT</c> frente a
    /// <c>PRESENT_BUT_UNREADABLE</c>. Fundirlas dejaria que un registro presente y corrupto se leyera como
    /// "cero variables", y la siguiente escritura borraria todas las del dibujo sin que nadie lo note. Por
    /// eso el guard de escritura no es una convencion del llamador: es una funcion que decide, y esta aqui.
    /// </para>
    /// </summary>
    public class ProjectVariablesRegistryAccessTests
    {
        private const string Guid1 = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";

        private static string JsonValido(string schemaVersion = "1.0")
            => "{\"SchemaVersion\":\"" + schemaVersion + "\",\"Variables\":[{\"VariableId\":\"" + Guid1 +
               "\",\"Name\":\"Holgura\",\"Type\":\"Length\",\"Definition\":{\"Kind\":\"literal\",\"Value\":6.0}}]}";

        // ================================================================ payload -> resultado

        [Fact]
        public void UnPayloadAUSENTE_ES_REGISTRO_VACIO()
        {
            var r = new ProjectVariablesStore().Read(ProjectVariablesPayload.Absent());

            Assert.Equal(ProjectVariablesReadOutcome.Absent, r.Outcome);
            Assert.NotNull(r.Document);
            Assert.Empty(r.Document.Variables);
            Assert.True(r.CanWrite);
        }

        [Fact]
        public void UnPayloadPRESENTE_Y_LEGIBLE_SE_DESERIALIZA()
        {
            var r = new ProjectVariablesStore().Read(ProjectVariablesPayload.Present(JsonValido()));

            Assert.Equal(ProjectVariablesReadOutcome.Readable, r.Outcome);
            Assert.Single(r.Document.ToProjectVariables());
            Assert.True(r.CanWrite);
        }

        [Fact]
        public void UnPayloadPRESENTE_PERO_ILEGIBLE_NO_ES_AUSENTE()
        {
            var r = new ProjectVariablesStore().Read(
                ProjectVariablesPayload.Unreadable("El Xrecord no contiene texto."));

            Assert.Equal(ProjectVariablesReadOutcome.PresentButUnreadable, r.Outcome);
            Assert.Null(r.Document);
            Assert.False(r.CanWrite);
            Assert.Contains("Xrecord", r.Error);
        }

        [Fact]
        public void UnPayloadPRESENTE_CON_JSON_CORRUPTO_FALLA_CERRADO()
        {
            var r = new ProjectVariablesStore().Read(ProjectVariablesPayload.Present("{no es json"));

            Assert.Equal(ProjectVariablesReadOutcome.PresentButUnreadable, r.Outcome);
            Assert.Null(r.Document);
            Assert.False(r.CanWrite);
        }

        [Fact]
        public void UnPayloadPRESENTE_SIN_SCHEMAVERSION_FALLA_CERRADO()
        {
            var r = new ProjectVariablesStore().Read(ProjectVariablesPayload.Present("{\"Variables\":[]}"));

            Assert.Equal(ProjectVariablesReadOutcome.PresentButUnreadable, r.Outcome);
            Assert.False(r.CanWrite);
        }

        [Fact]
        public void UnPayloadPRESENTE_DE_MAJOR_SUPERIOR_ES_INCOMPATIBLE()
        {
            var r = new ProjectVariablesStore().Read(ProjectVariablesPayload.Present(JsonValido("2.0")));

            Assert.Equal(ProjectVariablesReadOutcome.IncompatibleMajor, r.Outcome);
            Assert.False(r.CanWrite);
        }

        [Fact]
        public void UnPayloadNULO_SeTrataComoPresenteEIlegible_NoComoAusente()
        {
            var r = new ProjectVariablesStore().Read(null);

            Assert.Equal(ProjectVariablesReadOutcome.PresentButUnreadable, r.Outcome);
            Assert.False(r.CanWrite);
        }

        // ================================================================ guard de escritura

        [Fact]
        public void SePUEDE_ESCRIBIR_TrasUnaLecturaAUSENTE()
        {
            Assert.True(ProjectVariablesWriteGuard.CanOverwrite(
                ProjectVariablesReadResult.Absent(), out var error));
            Assert.Null(error);
        }

        [Fact]
        public void SePUEDE_ESCRIBIR_TrasUnaLecturaLEGIBLE()
        {
            var leido = new ProjectVariablesStore().Read(ProjectVariablesPayload.Present(JsonValido()));

            Assert.True(ProjectVariablesWriteGuard.CanOverwrite(leido, out _));
        }

        [Fact]
        public void NO_SE_ESCRIBE_TrasUnaLecturaILEGIBLE()
        {
            var leido = new ProjectVariablesStore().Read(ProjectVariablesPayload.Present("{roto"));

            Assert.False(ProjectVariablesWriteGuard.CanOverwrite(leido, out var error));
            Assert.False(string.IsNullOrWhiteSpace(error));
        }

        [Fact]
        public void NO_SE_ESCRIBE_TrasUnMajorINCOMPATIBLE()
        {
            var leido = new ProjectVariablesStore().Read(ProjectVariablesPayload.Present(JsonValido("2.0")));

            Assert.False(ProjectVariablesWriteGuard.CanOverwrite(leido, out var error));
            Assert.Contains("más nueva", error);
        }

        [Fact]
        public void NO_SE_ESCRIBE_SIN_HABER_LEIDO()
        {
            // La capa fisica no puede convertir "no lei" en "creo uno nuevo".
            Assert.False(ProjectVariablesWriteGuard.CanOverwrite(null, out var error));
            Assert.False(string.IsNullOrWhiteSpace(error));
        }

        // ================================================================ ida y vuelta completa

        [Fact]
        public void UnRegistroEscritoYVueltoALeer_ConservaSuContenido()
        {
            var store = new ProjectVariablesStore();

            var creado = ProjectVariablesDocument.CreateNew();
            creado.Variables.Add(new ProjectVariableDocument
            {
                VariableId = Guid1,
                Name = "Holgura estandar",
                Type = "Length",
                Definition = new ProjectVariableDefinitionDocument { Kind = "literal", Value = 7.5 },
            });

            var releido = store.Read(ProjectVariablesPayload.Present(store.Serialize(creado)));

            Assert.Equal(ProjectVariablesReadOutcome.Readable, releido.Outcome);
            var variable = Assert.Single(releido.Document.ToProjectVariables());
            Assert.Equal("Holgura estandar", variable.Name);
            Assert.Equal(7.5, variable.Definition.LiteralValue);
        }

        /// <summary>
        /// El troceado a 255 caracteres del almacen fisico no puede cambiar el contenido, asi que el texto
        /// tiene que sobrevivir a un registro grande. Se comprueba sobre el reensamblado, que es lo unico que
        /// esta capa promete.
        /// </summary>
        [Fact]
        public void UnRegistroGRANDE_SobreviveAlReensamblado()
        {
            var store = new ProjectVariablesStore();
            var creado = ProjectVariablesDocument.CreateNew();

            for (var i = 0; i < 40; i++)
            {
                creado.Variables.Add(new ProjectVariableDocument
                {
                    VariableId = System.Guid.NewGuid().ToString(),
                    Name = "Variable numero " + i,
                    Type = "Length",
                    Definition = new ProjectVariableDefinitionDocument { Kind = "literal", Value = i + 0.5 },
                });
            }

            var json = store.Serialize(creado);
            Assert.True(json.Length > 255 * 4, "el registro de prueba debe superar varios trozos");

            var releido = store.Read(ProjectVariablesPayload.Present(Trocear(json)));

            Assert.Equal(ProjectVariablesReadOutcome.Readable, releido.Outcome);
            Assert.Equal(40, releido.Document.ToProjectVariables().Count);
        }

        /// <summary>Reproduce el troceado/reensamblado del almacen fisico sin necesitar AutoCAD.</summary>
        private static string Trocear(string json)
        {
            var builder = new System.Text.StringBuilder();

            for (var i = 0; i < json.Length; i += 255)
            {
                builder.Append(json.Substring(i, System.Math.Min(255, json.Length - i)));
            }

            return builder.ToString();
        }
    }
}
