using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.RackFrames;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-53 (ID6 REUSE + ID7 BATCH DISTRIBUTION) G3 — el <see cref="HeaderConfigurationSnapshot"/> del nucleo compartido:
    /// pruebas C-01, C-02, C-03, C-04 y C-09 de la matriz congelada (Proposal V2 §13.2; contrato §4).
    ///
    /// <para>
    /// Lo que fijan es la unica garantia que el Snapshot vende: cada destino recibe una instancia INDEPENDIENTE, tomada
    /// por la copia canonica <see cref="RackFrameProjectStore.DeepCopy"/> despues de validar la fuente. La independencia
    /// se comprueba por identidad de referencias sobre el grafo entero (no por una lista de propiedades escrita a mano) y
    /// por mutacion de todas sus partes; la ruta de copia, por su efecto y por las referencias del IL, no por el texto de
    /// la fuente.
    /// </para>
    /// <para>
    /// No hay aqui nada de Selectivo ni de Dinamico: ni fondos, ni postes, ni modulos. Si una prueba lo necesitara, estaria
    /// mal ubicada (eso es G4/G6).
    /// </para>
    /// </summary>
    public class HeaderConfigurationSnapshotTests
    {
        private static HeaderConfigurationSnapshot Capture(RackFrameConfiguration source)
        {
            var captured = Assert.IsType<HeaderConfigurationCapture.Captured>(HeaderConfigurationSnapshot.TryCapture(source));
            Assert.NotNull(captured.Snapshot);
            return captured.Snapshot;
        }

        /// <summary>[Serializable] es un pseudoatributo: la reflexion lo entrega en los datos de atributos del tipo.</summary>
        private static bool IsMarkedSerializable(Type type)
            => type.GetCustomAttributesData().Any(attribute => attribute.AttributeType == typeof(SerializableAttribute));

        private static RackFrameConfiguration MaterializeNotNull(HeaderConfigurationSnapshot snapshot)
        {
            var copy = snapshot.Materialize();
            Assert.NotNull(copy);
            return copy;
        }

        /// <summary>El estado observable de una configuracion: proyeccion persistida, derivado y runtime.</summary>
        private static (string Wire, IReadOnlyList<string> Members, IReadOnlyList<string> Exceptions) Observed(RackFrameConfiguration configuration)
            => (HeaderConfigurationFixtures.Wire(configuration),
                HeaderConfigurationFixtures.MembersFingerprint(configuration),
                HeaderConfigurationFixtures.ExceptionsFingerprint(configuration));

        private static void AssertObserved(
            (string Wire, IReadOnlyList<string> Members, IReadOnlyList<string> Exceptions) expected,
            RackFrameConfiguration actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.Wire, HeaderConfigurationFixtures.Wire(actual));
            Assert.Equal(expected.Members, HeaderConfigurationFixtures.MembersFingerprint(actual));
            Assert.Equal(expected.Exceptions, HeaderConfigurationFixtures.ExceptionsFingerprint(actual));
        }

        // ===== C-01 — TryCapture: fallos cerrados, sin lanzar, sin copiar, sin mutar ==================================

        [Fact]
        public void C01_TRYCAPTURE_DE_NULL_FALLA_CON_NULLSOURCE_SIN_LANZAR()
        {
            HeaderConfigurationCapture result = null;
            var thrown = Record.Exception(() => result = HeaderConfigurationSnapshot.TryCapture(null));

            Assert.Null(thrown);
            var failed = Assert.IsType<HeaderConfigurationCapture.Failed>(result);
            Assert.Equal(HeaderCaptureFailure.NullSource, failed.Reason);
        }

        public static IEnumerable<object[]> UnusableDefects()
        {
            yield return new object[] { "Height=0" };
            yield return new object[] { "Height<0" };
            yield return new object[] { "Depth=0" };
            yield return new object[] { "Depth<0" };
            yield return new object[] { "LeftPost=null" };
            yield return new object[] { "RightPost=null" };
        }

        private static RackFrameConfiguration Unusable(string defect)
        {
            var configuration = HeaderConfigurationFixtures.Rich();
            switch (defect)
            {
                case "Height=0": configuration.Height = 0.0; break;
                case "Height<0": configuration.Height = -12.0; break;
                case "Depth=0": configuration.Depth = 0.0; break;
                case "Depth<0": configuration.Depth = -4.0; break;
                case "LeftPost=null": configuration.LeftPost = null; break;
                case "RightPost=null": configuration.RightPost = null; break;
                default: throw new ArgumentOutOfRangeException(nameof(defect), defect, "Defecto de fixture desconocido.");
            }

            return configuration;
        }

        [Theory]
        [MemberData(nameof(UnusableDefects))]
        public void C01_TRYCAPTURE_DE_UNA_CABECERA_NO_USABLE_FALLA_CON_UNUSABLEHEADER_SIN_LANZAR_NI_COPIAR_NI_MUTAR(string defect)
        {
            var source = Unusable(defect);
            Assert.False(RackDesignValidation.IsUsableHeader(source));

            // Premisa explicita: la copia canonica NO puede copiar esta fuente (Deserialize valida la cabecera). Si
            // TryCapture copiara antes de validar, esta prueba veria la excepcion: no hay catch-all que la oculte.
            Assert.ThrowsAny<Exception>(() => new RackFrameProjectStore().DeepCopy(source));

            var observedBefore = Observed(source);
            var graphBefore = HeaderConfigurationFixtures.MutableGraph(source);

            HeaderConfigurationCapture result = null;
            var thrown = Record.Exception(() => result = HeaderConfigurationSnapshot.TryCapture(source));

            Assert.Null(thrown);
            var failed = Assert.IsType<HeaderConfigurationCapture.Failed>(result);
            Assert.Equal(HeaderCaptureFailure.UnusableHeader, failed.Reason);

            // Sin mutar: mismo estado observable y exactamente los mismos objetos (nada reemplazado ni refrescado).
            AssertObserved(observedBefore, source);
            Assert.True(graphBefore.SetEquals(HeaderConfigurationFixtures.MutableGraph(source)), "TryCapture reemplazo objetos de la fuente.");
        }

        [Fact]
        public void C01_TRAS_VALIDAR_UNA_EXCEPCION_DE_LA_COPIA_SE_PROPAGA_SIN_CATCH_ALL()
        {
            // Una cabecera USABLE con un grafo defectuoso (una horizontal nula): la validacion explicita la deja pasar y la
            // copia canonica falla. Eso es un defecto, y el contrato (§4 regla 5, I-03) exige que se propague tal cual
            // en vez de convertirse en un Failed.
            var source = HeaderConfigurationFixtures.Rich();
            source.Horizontals.Add(null);
            Assert.True(RackDesignValidation.IsUsableHeader(source));

            var direct = Record.Exception(() => new RackFrameProjectStore().DeepCopy(source));
            Assert.NotNull(direct); // premisa: si DeepCopy dejara de fallar aqui, esta prueba debe elegir otro defecto

            var viaSnapshot = Record.Exception(() => HeaderConfigurationSnapshot.TryCapture(source));

            Assert.NotNull(viaSnapshot);
            Assert.IsType(direct.GetType(), viaSnapshot);
        }

        [Fact]
        public void C01_GUARDA_LA_CAPTURA_TIENE_EXACTAMENTE_DOS_FORMAS_Y_EL_FALLO_NO_LLEVA_SNAPSHOT()
        {
            SharedFoundationInspection.AssertClosedBase(typeof(HeaderConfigurationCapture));
            Assert.Equal(new[] { "Captured", "Failed" }, SharedFoundationInspection.VariantsOf(typeof(HeaderConfigurationCapture)));

            Assert.Equal(new[] { "Snapshot" }, SharedFoundationInspection.PublicInstanceProperties(typeof(HeaderConfigurationCapture.Captured)));
            Assert.Equal(new[] { "Reason" }, SharedFoundationInspection.PublicInstanceProperties(typeof(HeaderConfigurationCapture.Failed)));
            Assert.DoesNotContain(
                typeof(HeaderConfigurationCapture.Failed).GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance),
                member => member is PropertyInfo property && property.PropertyType == typeof(HeaderConfigurationSnapshot)
                          || member is FieldInfo field && field.FieldType == typeof(HeaderConfigurationSnapshot));
        }

        // ===== C-02 — cada Materialize es un grafo nuevo ================================================================

        [Fact]
        public void C02_CADA_MATERIALIZE_DEVUELVE_UN_GRAFO_NUEVO_SIN_NADA_COMPARTIDO_CON_EL_ORIGEN_NI_ENTRE_COPIAS()
        {
            var source = HeaderConfigurationFixtures.Rich();
            var expected = Observed(new RackFrameProjectStore().DeepCopy(source));
            Assert.Equal(HeaderConfigurationFixtures.Wire(source), expected.Wire);
            var snapshot = Capture(source);

            var a = MaterializeNotNull(snapshot);
            var b = MaterializeNotNull(snapshot);

            // I1/I3: instancias distintas, y cada una reproduce la receta capturada.
            Assert.NotSame(source, a);
            Assert.NotSame(source, b);
            Assert.NotSame(a, b);
            AssertObserved(expected, a);
            AssertObserved(expected, b);

            // Las colecciones y los objetos authored, uno por uno, para que el fallo sea legible...
            foreach (var (left, right) in new[] { (source, a), (source, b), (a, b) })
            {
                Assert.NotSame(left.Horizontals, right.Horizontals);
                Assert.NotSame(left.BracingPanels, right.BracingPanels);
                Assert.NotSame(left.Members, right.Members);
                Assert.NotSame(left.Exceptions, right.Exceptions);
                Assert.NotSame(left.LeftPost, right.LeftPost);
                Assert.NotSame(left.RightPost, right.RightPost);
                Assert.NotSame(left.LeftBasePlate, right.LeftBasePlate);
                Assert.NotSame(left.RightBasePlate, right.RightBasePlate);
                Assert.NotSame(left.Horizontals[0], right.Horizontals[0]);
                Assert.NotSame(left.BracingPanels[0], right.BracingPanels[0]);
                Assert.NotSame(left.BracingPanels[0].Members, right.BracingPanels[0].Members);

                // ...y el grafo ENTERO por identidad (I2, I3, I6), que ve tambien lo que la lista de arriba no nombra.
                HeaderConfigurationFixtures.AssertNoSharedMutableState(left, right);
            }
        }

        [Fact]
        public void C02_MEMBERS_SE_RECONSTRUYE_POR_LA_RUTA_CANONICA_Y_NUNCA_SE_COPIA_DEL_ORIGEN()
        {
            var source = HeaderConfigurationFixtures.Rich();
            var canonicalMembers = HeaderConfigurationFixtures.MembersFingerprint(new RackFrameProjectStore().DeepCopy(source));
            Assert.NotEmpty(canonicalMembers);

            // Se corrompe SOLO el derivado del origen: la receta authored no cambia. Un Snapshot que copiara Members en vez
            // de reconstruirlo entregaria este miembro falso.
            source.Members.Clear();
            source.Members.Add(new FrameMember { CatalogId = "DERIVADO-FALSO", Length = 1.0 });
            foreach (var panel in source.BracingPanels)
            {
                panel.Members.Clear();
            }

            var snapshot = Capture(source);
            var a = MaterializeNotNull(snapshot);
            var b = MaterializeNotNull(snapshot);

            foreach (var copy in new[] { a, b })
            {
                Assert.DoesNotContain(copy.Members, member => member.CatalogId == "DERIVADO-FALSO");
                Assert.Equal(canonicalMembers, HeaderConfigurationFixtures.MembersFingerprint(copy));
                Assert.Equal(
                    HeaderConfigurationFixtures.MembersFingerprint(new RackFrameProjectStore().DeepCopy(source)),
                    HeaderConfigurationFixtures.MembersFingerprint(copy));
            }

            // Reconstruido en cada copia, nunca la misma coleccion ni los mismos miembros.
            Assert.NotSame(a.Members, b.Members);
            Assert.All(a.Members, member => Assert.DoesNotContain(b.Members, other => ReferenceEquals(member, other)));
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void C02_EXCEPTIONS_SIGUE_EXACTAMENTE_LA_POLITICA_DE_DEEPCOPY(bool withExceptions)
        {
            var source = HeaderConfigurationFixtures.Rich();
            if (!withExceptions)
            {
                source.Exceptions.Clear();
            }

            // La politica es la de DeepCopy y ninguna otra: la referencia es su propia salida, no una regla reescrita aqui.
            var canonical = HeaderConfigurationFixtures.ExceptionsFingerprint(new RackFrameProjectStore().DeepCopy(source));
            Assert.Equal(HeaderConfigurationFixtures.ExceptionsFingerprint(source), canonical);

            var snapshot = Capture(source);
            var a = MaterializeNotNull(snapshot);
            var b = MaterializeNotNull(snapshot);

            Assert.Equal(canonical, HeaderConfigurationFixtures.ExceptionsFingerprint(a));
            Assert.Equal(canonical, HeaderConfigurationFixtures.ExceptionsFingerprint(b));
            Assert.NotSame(source.Exceptions, a.Exceptions);
            Assert.NotSame(a.Exceptions, b.Exceptions);
            for (var i = 0; i < source.Exceptions.Count; i++)
            {
                Assert.NotSame(source.Exceptions[i], a.Exceptions[i]);
                Assert.NotSame(a.Exceptions[i], b.Exceptions[i]);
            }
        }

        [Fact]
        public void C02_LA_COPIA_PRIVADA_NUNCA_ES_EL_ORIGEN_NI_SE_ENTREGA_A_NINGUN_DESTINO()
        {
            var source = HeaderConfigurationFixtures.Rich();
            var snapshot = Capture(source);
            var privateCopy = HeaderConfigurationFixtures.PrivateCopyOf(snapshot);

            // Regla 4 e I2: la copia privada existe, es canonica y no es la fuente.
            Assert.NotNull(privateCopy);
            HeaderConfigurationFixtures.AssertNoSharedMutableState(source, privateCopy);

            // Reglas 6 y 7: cada Materialize es una copia NUEVA de la privada, nunca la privada.
            var a = MaterializeNotNull(snapshot);
            var b = MaterializeNotNull(snapshot);
            HeaderConfigurationFixtures.AssertNoSharedMutableState(privateCopy, a);
            HeaderConfigurationFixtures.AssertNoSharedMutableState(privateCopy, b);
        }

        // ===== C-03 — mutar despues no cambia a nadie mas ===============================================================

        [Fact]
        public void C03_MUTAR_EL_ORIGEN_TRAS_CAPTURAR_NO_CAMBIA_EL_SNAPSHOT_NI_LAS_COPIAS_FUTURAS()
        {
            var source = HeaderConfigurationFixtures.Rich();
            var snapshot = Capture(source);
            var captured = Observed(new RackFrameProjectStore().DeepCopy(source));

            HeaderConfigurationFixtures.MutateEverywhere(source);
            Assert.NotEqual(captured.Wire, HeaderConfigurationFixtures.Wire(source)); // premisa: la mutacion es real

            // I4: la copia refleja lo capturado, no la modificacion posterior; la copia privada tampoco se movio.
            AssertObserved(captured, MaterializeNotNull(snapshot));
            AssertObserved(captured, HeaderConfigurationFixtures.PrivateCopyOf(snapshot));
        }

        [Fact]
        public void C03_MUTAR_UNA_COPIA_NO_CAMBIA_LAS_DEMAS_NI_EL_ORIGEN_NI_EL_SNAPSHOT()
        {
            var source = HeaderConfigurationFixtures.Rich();
            var sourceBefore = Observed(source);
            var snapshot = Capture(source);
            var captured = Observed(new RackFrameProjectStore().DeepCopy(source));

            var a = MaterializeNotNull(snapshot);
            var b = MaterializeNotNull(snapshot);

            HeaderConfigurationFixtures.MutateEverywhere(a);
            Assert.NotEqual(captured.Wire, HeaderConfigurationFixtures.Wire(a)); // premisa: la mutacion es real

            // I5/I6: ni la otra copia, ni el origen, ni el Snapshot, ni una copia posterior.
            AssertObserved(captured, b);
            AssertObserved(sourceBefore, source);
            AssertObserved(captured, HeaderConfigurationFixtures.PrivateCopyOf(snapshot));
            AssertObserved(captured, MaterializeNotNull(snapshot));
        }

        // ===== C-04 — el Snapshot no expone nada de su copia privada ====================================================

        [Fact]
        public void C04_GUARDA_EL_SNAPSHOT_NO_EXPONE_SU_COPIA_PRIVADA_COLECCIONES_SETTERS_NI_OBJETOS_AUTHORED()
        {
            var type = typeof(HeaderConfigurationSnapshot);

            // Sellado, sin constructor accesible (solo nace por TryCapture) y sin procedencia: un unico estado, la copia.
            Assert.True(type.IsSealed);
            Assert.All(
                type.GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic),
                constructor => Assert.True(constructor.IsPrivate, "El Snapshot expone un constructor no privado."));
            var field = Assert.Single(type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic));
            Assert.True(field.IsPrivate && field.IsInitOnly, "La copia privada debe ser un campo privado readonly.");
            Assert.Equal(typeof(RackFrameConfiguration), field.FieldType);
            Assert.Empty(type.GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));

            // Ninguna propiedad (ni getter que entregue la copia o sus colecciones, ni setter de ningun tipo).
            Assert.Empty(type.GetProperties(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic));

            // Ningun miembro no privado entrega un objeto del grafo authored ni una coleccion, salvo Materialize, que
            // devuelve una copia NUEVA (C-02, C-03).
            var exposed = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly)
                .Where(method => !method.IsPrivate)
                .ToList();
            Assert.Equal(
                new[] { nameof(HeaderConfigurationSnapshot.Materialize), nameof(HeaderConfigurationSnapshot.TryCapture) },
                exposed.Select(method => method.Name).OrderBy(name => name, StringComparer.Ordinal));
            Assert.All(exposed, method =>
            {
                var returned = method.ReturnType;
                Assert.False(
                    returned != typeof(string) && typeof(IEnumerable).IsAssignableFrom(returned),
                    method.Name + " devuelve una coleccion.");
                Assert.False(
                    returned.Namespace == typeof(RackFrameConfiguration).Namespace && method.Name != nameof(HeaderConfigurationSnapshot.Materialize),
                    method.Name + " devuelve un objeto del grafo authored.");
            });

            // Transitorio y no persistido: ninguna marca de serializacion.
            Assert.True(IsMarkedSerializable(typeof(Exception))); // premisa: la inspeccion ve [Serializable]
            Assert.False(IsMarkedSerializable(type));
            Assert.DoesNotContain(
                type.GetCustomAttributesData(),
                attribute => attribute.AttributeType.Namespace != null
                             && (attribute.AttributeType.Namespace.StartsWith("System.Text.Json", StringComparison.Ordinal)
                                 || attribute.AttributeType.Namespace.StartsWith("System.Runtime.Serialization", StringComparison.Ordinal)));
        }

        // ===== C-09 — la ruta de copia es DeepCopy, y ninguna otra ======================================================

        [Fact]
        public void C09_GUARDA_EL_NUCLEO_NO_COPIA_POR_CLONEHEADER_TOCONFIGURATION_NI_SERIALIZACION_O_DTO_PROPIOS()
        {
            var types = SharedFoundationInspection.TypesWithNested();
            var offenders = new List<string>();

            foreach (var type in types)
            {
                foreach (var member in SharedFoundationInspection.ReferencedMembers(type))
                {
                    var declaring = member as Type ?? member.DeclaringType;
                    var reason =
                        member.Name == "CloneHeader" ? "CloneHeader"
                        : member.Name == "MemberwiseClone" ? "MemberwiseClone"
                        : declaring == typeof(RackFrameProjectDocument) ? "RackFrameProjectDocument (ToConfiguration/FromConfiguration)"
                        : declaring == typeof(RackFrameProjectStore) && member.Name != nameof(RackFrameProjectStore.DeepCopy) && !(member is ConstructorInfo)
                            ? "RackFrameProjectStore." + member.Name + " (solo DeepCopy es la ruta canonica)"
                        : declaring != null && declaring.Name == "BracingPanelMemberBuilder" ? "refresco manual del derivado"
                        : declaring?.Namespace != null && declaring.Namespace.StartsWith("System.Text.Json", StringComparison.Ordinal) ? "serializacion propia"
                        : declaring != null && (declaring.Name.EndsWith("Document", StringComparison.Ordinal) || declaring.Name.EndsWith("Dto", StringComparison.Ordinal)) ? "DTO"
                        : null;
                    if (reason != null)
                    {
                        offenders.Add(type.Name + " -> " + (declaring?.Name ?? "?") + "." + member.Name + ": " + reason);
                    }
                }

                // Ni atributos de serializacion ni tipos DTO nuevos entre los de G3.
                if (type.IsClass && IsMarkedSerializable(type))
                {
                    offenders.Add(type.Name + ": [Serializable]");
                }

                offenders.AddRange(type.GetCustomAttributesData()
                    .Where(attribute => attribute.AttributeType.Namespace != null
                                        && (attribute.AttributeType.Namespace.StartsWith("System.Text.Json", StringComparison.Ordinal)
                                            || attribute.AttributeType.Namespace.StartsWith("System.Runtime.Serialization", StringComparison.Ordinal)))
                    .Select(attribute => type.Name + ": " + attribute.AttributeType.Name));

                if (type.Name.EndsWith("Document", StringComparison.Ordinal) || type.Name.EndsWith("Dto", StringComparison.Ordinal))
                {
                    offenders.Add(type.Name + ": tipo DTO");
                }
            }

            Assert.Empty(offenders);
        }

        [Fact]
        public void C09_EL_SNAPSHOT_COPIA_POR_LA_RUTA_CANONICA_DEEPCOPY_POR_EFECTO_Y_POR_DEPENDENCIA()
        {
            // Por dependencia: el IL del Snapshot llama a la validacion explicita y a la copia canonica.
            var referenced = SharedFoundationInspection.ReferencedMembers(typeof(HeaderConfigurationSnapshot));
            Assert.Contains(referenced, member => member.DeclaringType == typeof(RackDesignValidation) && member.Name == nameof(RackDesignValidation.IsUsableHeader));
            Assert.Contains(referenced, member => member.DeclaringType == typeof(RackFrameProjectStore) && member.Name == nameof(RackFrameProjectStore.DeepCopy));

            // Por efecto: la copia materializada es indistinguible de la salida de DeepCopy (persistido, derivado y runtime)...
            var source = HeaderConfigurationFixtures.Rich();
            var canonical = Observed(new RackFrameProjectStore().DeepCopy(source));
            var copy = MaterializeNotNull(Capture(source));
            AssertObserved(canonical, copy);

            // ...y distinguible de la ruta NO canonica (documento sin refresco): esa pierde el derivado y el runtime. Si el
            // Snapshot copiara por ahi, las dos aserciones anteriores habrian fallado.
            var nonCanonical = RackFrameProjectDocument.FromConfiguration(source).ToConfiguration();
            Assert.Empty(nonCanonical.Members);
            Assert.Empty(nonCanonical.Exceptions);
            Assert.NotEmpty(copy.Members);
            Assert.NotEmpty(copy.Exceptions);
        }
    }
}
