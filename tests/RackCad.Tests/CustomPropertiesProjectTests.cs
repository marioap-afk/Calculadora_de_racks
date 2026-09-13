using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RackCad.Application.CustomProperties;
using RackCad.Application.Persistence;
using Xunit;
using static RackCad.Tests.CustomPropertiesTestKit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-54 G6 — el alcance Proyecto en el borde fisico (Proposal V5 §12.6: T-PRJ-01..04; D-07; ADR-0039 §3 y §10).
    ///
    /// <para>
    /// El lector, el escritor y el ejecutor viven en el Plugin, que ninguna suite carga (ADR-0003): se fijan por su FORMA,
    /// con guardas sobre el codigo enmascarado que se demuestran en rojo contra variantes del archivo real. Lo que SI se
    /// alcanza sin AutoCAD se prueba por comportamiento: el contrato puro que el borde consume —payload tri-estado,
    /// clasificacion del store, guarda de escritura y serializacion ASCII que el troceado presupone— y la independencia de
    /// las dos autoridades de nivel dibujo. El comportamiento fisico es de G9 (OV-01 y OV-10); nada de esto finge una
    /// ejecucion de AutoCAD.
    /// </para>
    /// </summary>
    public class CustomPropertiesProjectTests
    {
        /// <summary>Un registro de variables de proyecto valido, con la forma que lee su store.</summary>
        private static readonly string ValidRegister =
            "{\"SchemaVersion\":\"1.0\",\"Variables\":[{\"VariableId\":\"" + IdA +
            "\",\"Name\":\"Holgura\",\"Type\":\"Length\",\"Definition\":{\"Kind\":\"literal\",\"Value\":6.0}}]}";

        // ================================================================ T-PRJ-01 lector del NOD

        [Fact]
        public void TPrj01_ElLectorDelNodEsTriEstado_NoLanzaPorElContenido_YEscribeUnXrecordDirecto()
        {
            Assert.Empty(CustomPropertiesEdgeSources.ReaderViolations(CustomPropertiesEdgeSources.DataText()));
        }

        public static TheoryData<string, string, string> ReaderMutations() => new TheoryData<string, string, string>
        {
            { "otra clave", "public const string DictKey = \"RACKCAD_CUSTOM_PROPERTIES\";", "public const string DictKey = \"RACKCAD_PROPERTIES\";" },
            { "otra clave en la busqueda", "if (!dictionary.Contains(DictKey))", "if (!dictionary.Contains(\"RACKCAD_CUSTOM_PROPERTIES\"))" },
            { "captura cualquier excepcion", "catch (Autodesk.AutoCAD.Runtime.Exception ex)", "catch (System.Exception ex)" },
            { "captura sin nombrar la familia", "catch (Autodesk.AutoCAD.Runtime.Exception ex)", "catch (Exception ex)" },
            { "lanza en vez de responder ausente", "return CustomPropertiesPayload.Absent();", "throw new InvalidOperationException();" },
            { "convierte con un cast que lanza", "if (!(transaction.GetObject(dictionary.GetAt(DictKey), OpenMode.ForRead) is Xrecord xrecord))", "var xrecord = (Xrecord)transaction.GetObject(dictionary.GetAt(DictKey), OpenMode.ForRead); if (xrecord == null)" },
            { "un Xrecord sin texto es ausente", "? CustomPropertiesPayload.Unreadable(", "? CustomPropertiesPayload.Absent(" },
            { "interpreta el JSON", "var builder = new StringBuilder();", "var builder = new StringBuilder(); System.Text.Json.JsonDocument.Parse(\"{}\");" },
            { "guarda un subdiccionario", "dictionary.SetAt(DictKey, xrecord);", "dictionary.SetAt(DictKey, new DBDictionary());" },
            { "trozos de mas de 255", "private const int ChunkSize = 255;", "private const int ChunkSize = 256;" },
            { "escribe sin trocear", "text.Substring(start, Math.Min(ChunkSize, text.Length - start))", "text" },
            { "trozos que no son texto", "buffer.Add(new TypedValue((int)DxfCode.Text,", "buffer.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString," },
            { "sustituye una entrada que no es Xrecord", "throw new InvalidOperationException(", "dictionary.Remove(DictKey); throw new InvalidOperationException(" },
        };

        [Theory]
        [MemberData(nameof(ReaderMutations))]
        public void TPrj01_LaGuardaDetectaUnAccesoFisicoQueSeSaleDelContrato(string caso, string original, string replacement)
        {
            var text = CustomPropertiesEdgeSources.DataText();

            Assert.Contains(original, text);
            Assert.True(
                CustomPropertiesEdgeSources.ReaderViolations(text.Replace(original, replacement)).Count > 0,
                "la guarda no detecta: " + caso);
        }

        /// <summary>
        /// Cada caso fisico llega al store con su distincion: ausente es escribible, y una entrada que no es Xrecord, un
        /// Xrecord sin datos o sin texto y un fallo de AutoCAD llegan como ilegibles, con su motivo, y nunca se sobrescriben.
        /// </summary>
        [Fact]
        public void TPrj01_CadaCasoFisicoLlegaAlStoreSinPerderLaDistincionEntreAusenteEIlegible()
        {
            var store = new CustomPropertiesStore();

            var absent = store.Read(CustomPropertiesPayload.Absent());

            Assert.Equal(CustomPropertiesReadOutcome.Absent, absent.Outcome);
            Assert.True(absent.CanWrite);

            foreach (var reason in new[] { "no es un Xrecord", "no contiene datos", "no contiene texto legible", "eWasErased" })
            {
                var unreadable = store.Read(CustomPropertiesPayload.Unreadable(reason));

                Assert.Equal(CustomPropertiesReadOutcome.PresentButUnreadable, unreadable.Outcome);
                Assert.False(unreadable.CanWrite);
                Assert.Contains(reason, unreadable.Error);
                Assert.False(CustomPropertiesWriteGuard.CanOverwrite(unreadable, out var refusal));
                Assert.Contains(reason, refusal);
            }
        }

        /// <summary>
        /// El troceado de 255 no necesita saber de Unicode porque el store escribe ASCII: nombres y valores con acentos y con
        /// pares de surrogates salen escapados, ningun trozo corta un par, y el texto vuelto a unir se lee igual.
        /// </summary>
        [Fact]
        public void TPrj01_ElTextoTroceadoEn255YVueltoAUnirSeLeeIgual_PorqueElStoreEscribeAscii()
        {
            var original = ReadableResult(Doc("1.0", Entries(
                Entry(IdA, AreaNfc, new string('x', 300) + Scalar(0x1F600)),
                Entry(IdB, "Cliente", Scalar(0x1F4E6) + " ACME " + Unit(0x00F1)))));
            var text = new CustomPropertiesStore().Serialize(original.Document);

            Assert.True(text.Length > 2 * 255, "el caso necesita varios trozos.");
            Assert.All(text, character => Assert.True(character < 0x80, "el store escribe un caracter fuera de ASCII."));

            var chunks = new List<string>();

            for (var start = 0; start < text.Length; start += 255)
            {
                chunks.Add(text.Substring(start, Math.Min(255, text.Length - start)));
            }

            Assert.All(chunks, chunk =>
            {
                Assert.InRange(chunk.Length, 1, 255);
                Assert.False(char.IsHighSurrogate(chunk[chunk.Length - 1]));
            });

            var reread = new CustomPropertiesStore().Read(CustomPropertiesPayload.Present(string.Concat(chunks)));

            Assert.Equal(CustomPropertiesReadOutcome.Readable, reread.Outcome);
            Assert.Equal(CustomPropertiesCanonicalForm.Of(original), CustomPropertiesCanonicalForm.Of(reread));
        }

        // ================================================================ T-PRJ-02 independiente de un RACKCAD_PROJECT corrupto

        [Fact]
        public void TPrj02_UnRegistroDeVariablesCorruptoNoBloqueaLasPropiedadesDeProyecto()
        {
            var register = new ProjectVariablesStore().Read(ProjectVariablesPayload.Present("{no es json"));

            Assert.Equal(ProjectVariablesReadOutcome.PresentButUnreadable, register.Outcome);
            Assert.False(register.CanWrite);

            // Nada de ese registro es entrada de las propiedades: con el mismo dibujo, la coleccion se lee, se acredita y se
            // edita igual, este ausente o presente.
            foreach (var payload in new[]
                     {
                         CustomPropertiesPayload.Absent(),
                         CustomPropertiesPayload.Present(Doc("1.0", Entries(Entry(IdB, "Cliente", "ACME")))),
                     })
            {
                var collection = new CustomPropertiesStore().Read(payload);

                Assert.True(collection.CanWrite);
                Assert.True(CustomPropertiesWriteGuard.CanOverwrite(collection, out _));
                Assert.Equal(CustomPropertiesWorkspaceState.Editable, CustomPropertiesWorkspace.ForProject(collection).State);
            }
        }

        [Fact]
        public void TPrj02_LasEntradasDeLasPropiedadesSonSoloSuPayload()
        {
            Assert.Equal(new[] { typeof(CustomPropertiesPayload) }, ParameterTypes(typeof(CustomPropertiesStore), nameof(CustomPropertiesStore.Read)));
            Assert.Equal(new[] { typeof(CustomPropertiesReadResult) }, ParameterTypes(typeof(CustomPropertiesWorkspace), nameof(CustomPropertiesWorkspace.ForProject)));

            var propertyTypes = ApplicationTypes().Where(IsPropertyType).ToList();

            Assert.True(propertyTypes.Count >= 20, "la barrida apenas ve tipos de propiedades.");
            Assert.Empty(SignatureCrossings(propertyTypes, IsProjectVariablesType));

            var sources = CustomPropertiesEdgeSources.ProductionSources();

            Assert.Empty(CustomPropertiesEdgeSources.IndependenceViolations(
                sources.Where(source => CustomPropertiesEdgeSources.IsPropertyFile(source.Path))));
            Assert.Contains(sources, source => source.Path == CustomPropertiesEdgeSources.DataPath);
        }

        // ================================================================ T-PRJ-03 RACKCAD_PROJECT independiente de propiedades corruptas

        [Fact]
        public void TPrj03_UnaColeccionDePropiedadesCorruptaNoBloqueaElRegistroDeVariables()
        {
            foreach (var payload in new[]
                     {
                         CustomPropertiesPayload.Present("{no es json"),
                         CustomPropertiesPayload.Unreadable("no es un Xrecord"),
                     })
            {
                var collection = new CustomPropertiesStore().Read(payload);

                Assert.Equal(CustomPropertiesReadOutcome.PresentButUnreadable, collection.Outcome);
                Assert.Equal(CustomPropertiesWorkspaceState.ReadOnly, CustomPropertiesWorkspace.ForProject(collection).State);
            }

            var register = new ProjectVariablesStore().Read(ProjectVariablesPayload.Present(ValidRegister));

            Assert.Equal(ProjectVariablesReadOutcome.Readable, register.Outcome);
            Assert.True(register.CanWrite);
        }

        /// <summary>
        /// Del lado de Project Variables, ni su superficie publica ni sus archivos conocen las propiedades. Que el diff del
        /// candidato no toque ningun archivo <c>ProjectVariables*</c> se comprueba sobre el diff al commitear, no aqui.
        /// </summary>
        [Fact]
        public void TPrj03_ProjectVariablesNoConoceLasPropiedades()
        {
            Assert.Equal(new[] { typeof(ProjectVariablesPayload) }, ParameterTypes(typeof(ProjectVariablesStore), nameof(ProjectVariablesStore.Read)));

            var registerTypes = ApplicationTypes().Where(IsProjectVariablesType).ToList();

            Assert.True(registerTypes.Count >= 20, "la barrida apenas ve tipos de Project Variables.");
            Assert.Empty(SignatureCrossings(registerTypes, IsPropertyType));

            var sources = CustomPropertiesEdgeSources.ProductionSources()
                .Where(source => CustomPropertiesEdgeSources.IsProjectVariablesFile(source.Path))
                .ToList();

            Assert.Contains(sources, source => source.Path == "src/RackCad.Plugin/ProjectVariablesData.cs");
            Assert.Empty(CustomPropertiesEdgeSources.IndependenceViolations(sources));
        }

        // ================================================================ T-PRJ-04 ejecutor de Proyecto

        [Fact]
        public void TPrj04_ElEjecutorDeProyectoRelee_Acredita_AplicaPorId_ConsultaLaGuarda_Escribe_YConfirmaUnaVez()
        {
            Assert.Empty(CustomPropertiesEdgeSources.ProjectExecutorViolations(CustomPropertiesEdgeSources.ExecutorCode()));
        }

        public static TheoryData<string, string, string> ProjectExecutorMutations() => new TheoryData<string, string, string>
        {
            { "escribe sin releer", "var fresh = store.Read(CustomPropertiesData.Read(transaction, database));", "var fresh = store.Read(CustomPropertiesPayload.Absent());" },
            { "no exige una lectura escribible", "if (!fresh.CanWrite)", "if (fresh == null)" },
            { "no consulta la guarda", "if (!CustomPropertiesWriteGuard.CanOverwrite(fresh, out var refusal))", "if (false)" },
            { "consulta la guarda dos veces", "var mutation = CustomPropertiesMutations.Apply(fresh, intent);", "CustomPropertiesWriteGuard.CanOverwrite(fresh, out _); var mutation = CustomPropertiesMutations.Apply(fresh, intent);" },
            { "confirma antes de escribir", "CustomPropertiesData.Write(transaction, database, store.Serialize(mutation.Document));", "transaction.Commit(); CustomPropertiesData.Write(transaction, database, store.Serialize(mutation.Document));" },
            { "escribe otra cosa que lo serializado", "CustomPropertiesData.Write(transaction, database, store.Serialize(mutation.Document));", "CustomPropertiesData.Write(transaction, database, fresh.Error); store.Serialize(mutation.Document);" },
            { "regenera", "return CustomPropertiesProjectExecution.Written(", "document.Editor.Regen(); return CustomPropertiesProjectExecution.Written(" },
            { "toca Project Variables", "var store = new CustomPropertiesStore();", "var store = new CustomPropertiesStore(); ProjectVariablesRegistry.Read(null, null);" },
            { "abre una segunda transaccion", "var store = new CustomPropertiesStore();", "var store = new CustomPropertiesStore(); using (var otra = database.TransactionManager.StartTransaction()) { }" },
        };

        [Theory]
        [MemberData(nameof(ProjectExecutorMutations))]
        public void TPrj04_LaGuardaDetectaUnEjecutorDeProyectoQueSeSaleDelContrato(string caso, string original, string replacement)
        {
            var text = CustomPropertiesEdgeSources.ExecutorText();

            Assert.Contains(original, text);
            Assert.True(
                CustomPropertiesEdgeSources.ProjectExecutorViolations(PluginSourceCode.Mask(text.Replace(original, replacement))).Count > 0,
                "la guarda no detecta: " + caso);
        }

        /// <summary>
        /// Lo que el ejecutor encadena, sobre cada lectura fresca no escribible: la mutacion se niega sin documento y la guarda
        /// tampoco deja escribir, asi que no hay nada que serializar ni que llevar al NOD.
        /// </summary>
        [Fact]
        public void TPrj04_SobreUnaLecturaFrescaNoEscribible_NoHayDocumentoQueEscribir()
        {
            var cases = new[]
            {
                CustomPropertiesPayload.Unreadable("no es un Xrecord"),
                CustomPropertiesPayload.Present("{no es json"),
                CustomPropertiesPayload.Present(Doc("2.0", "[]")),
                CustomPropertiesPayload.Present(Doc("1.0", Entries(Entry(IdA, "A", "1"), Entry(IdA.ToUpperInvariant(), "B", "2")))),
                CustomPropertiesPayload.Present(Doc("1.0", "[]", ",\"Profundo\":" + Nested(17))),
            };

            foreach (var payload in cases)
            {
                var fresh = new CustomPropertiesStore().Read(payload);

                Assert.False(fresh.CanWrite, fresh.Outcome.ToString());

                var mutation = CustomPropertiesMutations.Apply(fresh, CustomPropertiesIntent.Create("Cliente", "ACME"));

                Assert.Equal(CustomPropertiesRejection.NotWritable, mutation.Rejection);
                Assert.Null(mutation.Document);
                Assert.False(CustomPropertiesWriteGuard.CanOverwrite(fresh, out _));
            }
        }

        /// <summary>
        /// Lo que el ejecutor encadena sobre una lectura fresca escribible: aplica por id, la guarda deja escribir, y el texto
        /// serializado —lo que el NOD devolveria en la siguiente lectura— se lee con el cambio. Un id que no existe se niega
        /// sin documento: el nombre nunca localiza.
        /// </summary>
        [Fact]
        public void TPrj04_SobreUnaLecturaFrescaEscribible_LoAplicadoPorIdEsLoQueSeRelee()
        {
            var store = new CustomPropertiesStore();

            var absent = store.Read(CustomPropertiesPayload.Absent());
            var created = CustomPropertiesMutations.Apply(absent, CustomPropertiesIntent.Create("Cliente", "ACME"));

            Assert.True(created.Succeeded);
            Assert.True(CustomPropertiesWriteGuard.CanOverwrite(absent, out _));

            var afterCreate = store.Read(CustomPropertiesPayload.Present(store.Serialize(created.Document)));

            Assert.Equal(CustomPropertiesReadOutcome.Readable, afterCreate.Outcome);
            Assert.Equal(created.CreatedId, Id(Assert.Single(afterCreate.Document.Entries).Id));

            var renamed = CustomPropertiesMutations.Apply(afterCreate, CustomPropertiesIntent.Rename(created.CreatedId, "Cliente final"));

            Assert.True(renamed.Succeeded);
            Assert.True(CustomPropertiesWriteGuard.CanOverwrite(afterCreate, out _));

            var afterRename = store.Read(CustomPropertiesPayload.Present(store.Serialize(renamed.Document)));
            var entry = Assert.Single(afterRename.Document.Entries);

            Assert.Equal(created.CreatedId, Id(entry.Id));
            Assert.Equal("Cliente final", entry.Name);
            Assert.Equal("ACME", entry.Value);

            var missing = CustomPropertiesMutations.Apply(afterRename, CustomPropertiesIntent.Delete(Id(IdC)));

            Assert.Equal(CustomPropertiesRejection.EntryNotFound, missing.Rejection);
            Assert.Null(missing.Document);
        }

        // ================================================================ utilidades

        private static Type[] ParameterTypes(Type type, string method)
            => type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly)
                .Single(candidate => candidate.Name == method)
                .GetParameters()
                .Select(parameter => parameter.ParameterType)
                .ToArray();

        private static IEnumerable<Type> ApplicationTypes()
            => typeof(CustomPropertiesStore).Assembly.GetTypes().Where(type => type.IsPublic || type.IsNestedPublic);

        private static bool IsPropertyType(Type type)
            => type.Namespace == "RackCad.Application.CustomProperties"
               || (type.Namespace == "RackCad.Application.Persistence" && type.Name.StartsWith("CustomPropert", StringComparison.Ordinal));

        private static bool IsProjectVariablesType(Type type)
            => type.Namespace == "RackCad.Application.ProjectVariables"
               || type.Name.Contains("ProjectVariable", StringComparison.Ordinal);

        /// <summary>Cada tipo que la superficie publica de <paramref name="types"/> expone y cumple <paramref name="foreign"/>.</summary>
        private static IReadOnlyList<string> SignatureCrossings(IEnumerable<Type> types, Func<Type, bool> foreign)
        {
            const BindingFlags Flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly;
            var crossings = new List<string>();

            foreach (var type in types)
            {
                var exposed = new List<(string Member, Type Type)> { ("base", type.BaseType) };

                exposed.AddRange(type.GetInterfaces().Select(contract => ("interfaz", contract)));

                foreach (var method in type.GetMethods(Flags))
                {
                    exposed.Add((method.Name, method.ReturnType));
                    exposed.AddRange(method.GetParameters().Select(parameter => (method.Name, parameter.ParameterType)));
                }

                foreach (var constructor in type.GetConstructors(Flags))
                {
                    exposed.AddRange(constructor.GetParameters().Select(parameter => (".ctor", parameter.ParameterType)));
                }

                exposed.AddRange(type.GetProperties(Flags).Select(property => (property.Name, property.PropertyType)));
                exposed.AddRange(type.GetFields(Flags).Select(field => (field.Name, field.FieldType)));

                foreach (var (member, exposedType) in exposed)
                {
                    foreach (var part in Unwrap(exposedType).Where(foreign))
                    {
                        crossings.Add(type.FullName + "." + member + " expone " + part.FullName);
                    }
                }
            }

            return crossings;
        }

        private static IEnumerable<Type> Unwrap(Type type)
        {
            if (type == null)
            {
                yield break;
            }

            if (type.HasElementType)
            {
                foreach (var inner in Unwrap(type.GetElementType()))
                {
                    yield return inner;
                }

                yield break;
            }

            if (type.IsGenericType)
            {
                foreach (var inner in type.GetGenericArguments().SelectMany(Unwrap))
                {
                    yield return inner;
                }

                yield return type.GetGenericTypeDefinition();
                yield break;
            }

            yield return type;
        }
    }
}
