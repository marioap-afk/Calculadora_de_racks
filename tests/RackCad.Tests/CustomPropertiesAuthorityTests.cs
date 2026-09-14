using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.CustomProperties;
using RackCad.Application.Persistence;
using Xunit;
using static RackCad.Tests.CustomPropertiesAuthorityTestKit;
using static RackCad.Tests.CustomPropertiesTestKit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-54 G5 — autoridad de las propiedades por <c>RackId</c> (Proposal V5 D-09, §12.3 T-AUT-01..15; ADR-0039 §8).
    /// Todo es Application pura sobre la proyeccion plana de D-22, con <c>isKnownKind</c> inyectado.
    /// </summary>
    public class CustomPropertiesAuthorityTests
    {
        private static readonly string Base = Props(Entry(IdA, "Cliente", "ACME"), Entry(IdB, "Area", "Norte"));

        // ================================================================ T-AUT-01 Single con miembros iguales

        [Fact]
        public void TAut01_TodosLosMiembrosLegiblesEIguales_EsSingle()
        {
            var result = Authority(
                "10",
                View("10", properties: Base),
                View("11", properties: Base, view: "lateral", section: 0),
                View("12", properties: Base, view: "planta"));

            Assert.Equal(RackCustomPropertiesAuthorityOutcome.Single, result.Outcome);
            Assert.True(result.IsWritable);
            Assert.Equal(RackA, result.RackId);
            Assert.Equal("selective", result.Kind);
            Assert.Null(result.Error);
            Assert.Equal(new[] { "10", "11", "12" }, result.Members.Select(member => member.Handle));
            Assert.Equal(new[] { "ACME", "Norte" }, result.Collection.Document.Entries.Select(entry => entry.Value));
            Assert.Equal("1.0", result.WriteVersion);
            Assert.Empty(result.UnifyOptions);
        }

        [Fact]
        public void TAut01_TodosAusentes_EsSingleConLaColeccionVacia()
        {
            var result = Authority("1", View("1"), View("2", view: "lateral", section: 2));

            Assert.Equal(RackCustomPropertiesAuthorityOutcome.Single, result.Outcome);
            Assert.Empty(result.Collection.Document.Entries);
            Assert.Equal("1.0", result.WriteVersion);
        }

        [Fact]
        public void TAut01_OtrosRacks_NoParticipan()
        {
            var result = Authority(
                "1",
                View("1", properties: Base),
                View("2", properties: Base, view: "lateral"),
                View("3", rackId: RackB, properties: Props(Entry(IdC, "Otro", "rack"))));

            Assert.Equal(RackCustomPropertiesAuthorityOutcome.Single, result.Outcome);
            Assert.Equal(new[] { "1", "2" }, result.Members.Select(member => member.Handle));
        }

        [Fact]
        public void TAut01_ElRackIdSeComparaSinDistinguirMayusculas()
        {
            var result = Authority("1", View("1", properties: Base), View("2", rackId: RackA.ToUpperInvariant(), properties: Base));

            Assert.Equal(RackCustomPropertiesAuthorityOutcome.Single, result.Outcome);
            Assert.Equal(2, result.Members.Count);
        }

        // ================================================================ T-AUT-02 ausente ≡ vacio con cualquier minor

        [Fact]
        public void TAut02_AusenteVacio10YVacio13_SonSingle_ConTransitividad()
        {
            var absent = View("1");
            var empty10 = View("2", properties: EmptyAt("1.0"), view: "lateral");
            var empty13 = View("3", properties: EmptyAt("1.3"), view: "planta");

            var all = Authority("1", absent, empty10, empty13);

            Assert.Equal(RackCustomPropertiesAuthorityOutcome.Single, all.Outcome);
            Assert.Empty(all.Collection.Document.Entries);
            Assert.Equal("1.3", all.WriteVersion);

            foreach (var pair in new[] { (absent, empty10), (absent, empty13), (empty10, empty13) })
            {
                Assert.Equal(
                    RackCustomPropertiesAuthorityOutcome.Single,
                    Authority(pair.Item1.Handle, pair.Item1, pair.Item2).Outcome);
            }
        }

        /// <summary>Caso entre builds: vacia escrita 1.0 por I-54 frente a vacia leida a 1.1.</summary>
        [Fact]
        public void TAut02_EntreBuilds_Vacia10FrenteAVacia11_EsSingleA11()
        {
            var result = Authority("1", View("1", properties: EmptyAt("1.0")), View("2", properties: EmptyAt("1.1"), view: "lateral"));

            Assert.Equal(RackCustomPropertiesAuthorityOutcome.Single, result.Outcome);
            Assert.Equal("1.1", result.WriteVersion);
        }

        [Fact]
        public void TAut02_UnaColeccionVaciaConExtensionDataDeRaiz_NoEsElVacioCanonico()
        {
            var result = Authority("1", View("1"), View("2", properties: Doc("1.0", "[]", ",\"Raiz\":1"), view: "lateral"));

            Assert.Equal(RackCustomPropertiesAuthorityOutcome.Divergent, result.Outcome);
        }

        // ================================================================ T-AUT-03 divergencia legible

        public static TheoryData<string, string> ReadableDivergences()
            => new TheoryData<string, string>
            {
                { "valor", Props(Entry(IdA, "Cliente", "OTRO"), Entry(IdB, "Area", "Norte")) },
                { "nombre", Props(Entry(IdA, "Clientes", "ACME"), Entry(IdB, "Area", "Norte")) },
                { "nombre-mayusculas", Props(Entry(IdA, "cliente", "ACME"), Entry(IdB, "Area", "Norte")) },
                { "orden", Props(Entry(IdB, "Area", "Norte"), Entry(IdA, "Cliente", "ACME")) },
                { "id", Props(Entry(IdC, "Cliente", "ACME"), Entry(IdB, "Area", "Norte")) },
                { "conteo", Props(Entry(IdA, "Cliente", "ACME")) },
                { "ausente", null },
            };

        [Theory]
        [MemberData(nameof(ReadableDivergences))]
        public void TAut03_DivergenciaLegible_EsDivergent_ConEstadoPorVista(string caso, string other)
        {
            var result = Authority("1", View("1", properties: Base), View("2", properties: other, view: "lateral", section: 3));

            Assert.True(result.Outcome == RackCustomPropertiesAuthorityOutcome.Divergent, caso);
            Assert.False(result.IsWritable);
            Assert.Null(result.Collection);
            Assert.Null(result.WriteVersion);
            Assert.False(string.IsNullOrWhiteSpace(result.Error));

            var lateral = Assert.Single(result.Members, member => member.Handle == "2");
            Assert.Equal("lateral", lateral.View);
            Assert.Equal(3, lateral.Section);
            Assert.Equal("RACK_2", lateral.BlockName);
            Assert.True(lateral.Collection.CanWrite);
        }

        [Fact]
        public void TAut03_ElIdSeComparaPorValorDeGuid_NoPorSuTexto()
        {
            var upper = Props(Entry(IdA.ToUpperInvariant(), "Cliente", "ACME"), Entry(IdB, "Area", "Norte"));

            Assert.Equal(
                RackCustomPropertiesAuthorityOutcome.Single,
                Authority("1", View("1", properties: Base), View("2", properties: upper, view: "lateral")).Outcome);
        }

        // ================================================================ T-AUT-04 ExtensionData en profundidad

        [Fact]
        public void TAut04_ExtensionDataDeRaizDistinto_EsDivergent()
        {
            var one = Doc("1.0", Entries(Entry(IdA, "Cliente", "ACME")), ",\"Raiz\":{\"a\":1}");
            var two = Doc("1.0", Entries(Entry(IdA, "Cliente", "ACME")), ",\"Raiz\":{\"a\":2}");

            Assert.Equal(RackCustomPropertiesAuthorityOutcome.Divergent, Authority("1", View("1", properties: one), View("2", properties: two)).Outcome);
        }

        [Fact]
        public void TAut04_ExtensionDataDeEntradaDistinto_EsDivergent()
        {
            var one = Props(Entry(IdA, "Cliente", "ACME", ",\"Nota\":\"x\""));
            var two = Props(Entry(IdA, "Cliente", "ACME", ",\"Nota\":\"y\""));

            Assert.Equal(RackCustomPropertiesAuthorityOutcome.Divergent, Authority("1", View("1", properties: one), View("2", properties: two)).Outcome);
        }

        [Fact]
        public void TAut04_IgualdadEnProfundidad_SinImportarElOrdenDeMiembros_ConAnidadosYArraysDeObjetos()
        {
            var one = Doc(
                "1.0",
                Entries(Entry(IdA, "Cliente", "ACME", ",\"Nota\":{\"b\":[{\"x\":1,\"y\":\"z\"}],\"a\":true}")),
                ",\"Raiz\":{\"lista\":[{\"p\":null,\"q\":[1,2]}],\"n\":2.50}");
            var two = Doc(
                "1.0",
                Entries(Entry(IdA, "Cliente", "ACME", ",\"Nota\":{\"a\":true,\"b\":[{\"y\":\"z\",\"x\":1}]}")),
                ",\"Raiz\":{\"n\":2.50,\"lista\":[{\"q\":[1,2],\"p\":null}]}");

            Assert.Equal(RackCustomPropertiesAuthorityOutcome.Single, Authority("1", View("1", properties: one), View("2", properties: two)).Outcome);
        }

        [Fact]
        public void TAut04_LosArraysSeComparanEnOrden()
        {
            var one = Doc("1.0", "[]", ",\"Raiz\":[1,2]");
            var two = Doc("1.0", "[]", ",\"Raiz\":[2,1]");

            Assert.Equal(RackCustomPropertiesAuthorityOutcome.Divergent, Authority("1", View("1", properties: one), View("2", properties: two)).Outcome);
        }

        [Fact]
        public void TAut04_LosNumerosSeComparanPorTextoCrudo()
        {
            var one = Doc("1.0", "[]", ",\"Raiz\":{\"n\":1.0}");
            var two = Doc("1.0", "[]", ",\"Raiz\":{\"n\":1}");

            Assert.Equal(RackCustomPropertiesAuthorityOutcome.Divergent, Authority("1", View("1", properties: one), View("2", properties: two)).Outcome);
        }

        [Fact]
        public void TAut04_LosStringsSeComparanPorValor_AunqueElEscapeSeaDistinto()
        {
            var one = Doc("1.0", "[]", ",\"Raiz\":\"" + JsonEscape(0x0041) + "B\"");
            var two = Doc("1.0", "[]", ",\"Raiz\":\"AB\"");

            Assert.Equal(RackCustomPropertiesAuthorityOutcome.Single, Authority("1", View("1", properties: one), View("2", properties: two)).Outcome);
        }

        /// <summary>Un documento con nombres repetidos nunca llega a compararse: el store ya lo clasifica ilegible (C-4).</summary>
        [Fact]
        public void TAut04_UnDocumentoConNombresRepetidos_NoLlegaACompararse()
        {
            var repeated = Doc("1.0", "[]", ",\"Raiz\":{\"a\":1,\"a\":2}");

            var result = Authority("1", View("1", properties: Base), View("2", properties: repeated));

            Assert.Equal(RackCustomPropertiesAuthorityOutcome.CustomPropertiesReadOnly, result.Outcome);
            Assert.Equal(
                CustomPropertiesReadOutcome.PresentButUnreadable,
                Assert.Single(result.Members, member => member.Handle == "2").Collection.Outcome);
        }

        // ================================================================ T-AUT-05 solo el minor

        [Fact]
        public void TAut05_SoloCambiaElMinor_EsSingle_ConElMinorMayorComoVersionDeEscritura()
        {
            var result = Authority(
                "1",
                View("1", properties: PropsAt("1.0", Entry(IdA, "Cliente", "ACME"))),
                View("2", properties: PropsAt("1.7", Entry(IdA, "Cliente", "ACME")), view: "lateral"),
                View("3", properties: PropsAt("1.2", Entry(IdA, "Cliente", "ACME")), view: "planta"));

            Assert.Equal(RackCustomPropertiesAuthorityOutcome.Single, result.Outcome);
            Assert.Equal("1.7", result.WriteVersion);
            Assert.Equal("1.7", result.Collection.Document.SchemaVersion);
        }

        [Fact]
        public void TAut05_ElMinorSeComparaComoNumero()
        {
            var result = Authority(
                "1",
                View("1", properties: PropsAt("1.9", Entry(IdA, "Cliente", "ACME"))),
                View("2", properties: PropsAt("1.10", Entry(IdA, "Cliente", "ACME"))));

            Assert.Equal("1.10", result.WriteVersion);
        }

        [Fact]
        public void TAut05_LaVersionDeEscrituraNuncaBajaDeLaActual()
        {
            Assert.Equal(
                CustomPropertiesDocument.CurrentSchemaVersion,
                Authority("1", View("1"), View("2", view: "lateral")).WriteVersion);
        }

        // ================================================================ T-AUT-06 / T-AUT-07 pertenencia indeterminada

        [Fact]
        public void TAut06_PayloadIlegibleColocadoDeOtroRack_EsIndeterminateMembershipParaTodoRack()
        {
            var definitions = new[]
            {
                View("1", properties: Base),
                View("2", rackId: RackB, properties: Base),
                Uninterpretable("9", placed: true),
            };

            foreach (var selected in new[] { "1", "2" })
            {
                var result = Authority(definitions, selected);

                Assert.Equal(RackCustomPropertiesAuthorityOutcome.IndeterminateMembership, result.Outcome);
                Assert.False(result.IsWritable);
                Assert.Null(result.Collection);
                Assert.Empty(result.UnifyOptions);

                var diagnostic = Assert.Single(result.UninterpretableDefinitions);
                Assert.Equal("9", diagnostic.Handle);
                Assert.Equal("RACK_9", diagnostic.BlockName);
                Assert.True(diagnostic.IsPlaced);
                Assert.Contains("RACK_9", result.Error);
            }
        }

        [Fact]
        public void TAut06_ElBloqueElegidoIlegible_EsIndeterminateMembership()
        {
            var result = Authority("9", View("1", properties: Base), Uninterpretable("9"));

            Assert.Equal(RackCustomPropertiesAuthorityOutcome.IndeterminateMembership, result.Outcome);
            Assert.Null(result.RackId);
            Assert.Empty(result.Members);
        }

        [Fact]
        public void TAut06_ElDiagnosticoListaCadaDefinicionIlegible_EnOrdenDeterminista()
        {
            var result = Authority("1", Uninterpretable("F2"), View("1"), Uninterpretable("0A", placed: false));

            Assert.Equal(new[] { "0A", "F2" }, result.UninterpretableDefinitions.Select(definition => definition.Handle));
            Assert.Equal(new[] { false, true }, result.UninterpretableDefinitions.Select(definition => definition.IsPlaced));
        }

        [Fact]
        public void TAut07_PayloadIlegibleSinColocar_TambienEsIndeterminateMembership()
        {
            var result = Authority("1", View("1", properties: Base), View("2", properties: Base, view: "lateral"), Uninterpretable("7", placed: false));

            Assert.Equal(RackCustomPropertiesAuthorityOutcome.IndeterminateMembership, result.Outcome);
            Assert.False(Assert.Single(result.UninterpretableDefinitions).IsPlaced);
            Assert.Contains("RACK_7", result.Error);
        }

        // ================================================================ T-AUT-08 RackId en blanco

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void TAut08_RackIdEnBlanco_NoEsMiembroNiBloquea_YElegidoEsNoIdentity(string blank)
        {
            var definitions = new[]
            {
                View("1", properties: Base),
                View("2", rackId: blank, properties: Props(Entry(IdC, "Otra", "cosa"))),
            };

            var fromRack = Authority(definitions, "1");
            Assert.Equal(RackCustomPropertiesAuthorityOutcome.Single, fromRack.Outcome);
            Assert.Equal(new[] { "1" }, fromRack.Members.Select(member => member.Handle));

            var fromBlank = Authority(definitions, "2");
            Assert.Equal(RackCustomPropertiesAuthorityOutcome.NoIdentity, fromBlank.Outcome);
            Assert.False(fromBlank.IsWritable);
            Assert.Null(fromBlank.RackId);
            Assert.Empty(fromBlank.Members);
            Assert.False(string.IsNullOrWhiteSpace(fromBlank.Error));
        }

        // ================================================================ T-AUT-09 Kind en blanco

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        public void TAut09_KindEnBlancoEnUnMiembro_EsMixedKind_ConDiagnosticoDeLaDefinicion(string blank)
        {
            var result = Authority("1", View("1", properties: Base), View("2", properties: Base, kind: blank, view: "lateral"));

            Assert.Equal(RackCustomPropertiesAuthorityOutcome.MixedKind, result.Outcome);
            Assert.False(result.IsWritable);
            Assert.Null(result.Collection);
            Assert.Equal(new[] { "2" }, result.BlankKindDefinitions.Select(definition => definition.Handle));
            Assert.Contains("RACK_2", result.Error);
            Assert.Contains("RACKEDITAR", result.Error);
        }

        /// <summary>El payload Selectivo anterior al sobre se lee como sobre legible sin <c>Kind</c> ni <c>Design</c> (P-23).</summary>
        [Fact]
        public void TAut09_ElPayloadSelectivoAnteriorAlSobre_EsMixedKind()
        {
            var legacy = Definition("2", "{\"SchemaVersion\":\"1.0\",\"Id\":\"" + RackA + "\",\"Name\":\"Rack A\"}");

            Assert.Null(legacy.Envelope.Kind);
            Assert.Null(legacy.Envelope.Design);

            var result = Authority("1", View("1", properties: Base), legacy);

            Assert.Equal(RackCustomPropertiesAuthorityOutcome.MixedKind, result.Outcome);
            Assert.Equal("2", Assert.Single(result.BlankKindDefinitions).Handle);
        }

        /// <summary>PR-02: el mensaje remite a <c>RACKEDITAR</c> sin prometer una reparacion.</summary>
        [Fact]
        public void TAut09_ElDiagnosticoNoPrometeReparacion()
        {
            var result = Authority("1", View("1"), View("2", kind: null));

            Assert.DoesNotContain("repar", result.Error, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("arregl", result.Error, StringComparison.OrdinalIgnoreCase);
        }

        // ================================================================ T-AUT-10 matriz de kind

        [Fact]
        public void TAut10a_KindConocidoYComun_Continua_YConColeccionesIgualesEsSingle()
        {
            Assert.Equal(
                RackCustomPropertiesAuthorityOutcome.Single,
                Authority("1", View("1", properties: Base, kind: "pushback"), View("2", properties: Base, kind: "pushback")).Outcome);
        }

        [Fact]
        public void TAut10b_AlgunKindEnBlanco_EsMixedKind()
        {
            Assert.Equal(
                RackCustomPropertiesAuthorityOutcome.MixedKind,
                Authority("1", View("1", kind: "dynamic"), View("2", kind: "")).Outcome);
        }

        [Fact]
        public void TAut10c_DosKindsConocidosDistintos_EsMixedKind()
        {
            var result = Authority("1", View("1", kind: "selective"), View("2", kind: "dynamic"));

            Assert.Equal(RackCustomPropertiesAuthorityOutcome.MixedKind, result.Outcome);
            Assert.Empty(result.BlankKindDefinitions);
        }

        [Fact]
        public void TAut10d_UnKindConocidoYUnoDesconocido_EsMixedKind()
        {
            Assert.Equal(
                RackCustomPropertiesAuthorityOutcome.MixedKind,
                Authority("1", View("1", kind: "selective"), View("2", kind: "futuro")).Outcome);
        }

        [Fact]
        public void TAut10e_UnKindDesconocidoComun_EsUnknownKind_SoloLectura_SinTocarElSobre()
        {
            var definitions = new[] { View("1", properties: Base, kind: "futuro"), View("2", properties: Base, kind: "Futuro", view: "lateral") };
            var before = definitions.Select(definition => new RackEmbedStore().Serialize(definition.Envelope)).ToList();

            var result = Authority(definitions, "1");

            Assert.Equal(RackCustomPropertiesAuthorityOutcome.UnknownKind, result.Outcome);
            Assert.False(result.IsWritable);
            Assert.Null(result.Collection);
            Assert.Empty(result.UnifyOptions);
            Assert.Equal("futuro", result.Kind, ignoreCase: true);
            Assert.Contains("futuro", result.Error, StringComparison.OrdinalIgnoreCase);

            foreach (var intent in new[]
            {
                CustomPropertiesIntent.Create("Nueva", "x"),
                CustomPropertiesIntent.Rename(Id(IdA), "Otro"),
                CustomPropertiesIntent.ChangeValue(Id(IdA), "Otro"),
                CustomPropertiesIntent.Delete(Id(IdA)),
            })
            {
                Assert.False(CustomPropertiesPreflight.ForRack(result, intent).IsAccepted);
            }

            Assert.Equal(before, definitions.Select(definition => new RackEmbedStore().Serialize(definition.Envelope)));
        }

        [Fact]
        public void TAut10f_UnaDiferenciaSoloDeMayusculas_EsElMismoKind()
        {
            Assert.Equal(
                RackCustomPropertiesAuthorityOutcome.Single,
                Authority("1", View("1", kind: "Selective"), View("2", kind: "SELECTIVE")).Outcome);
        }

        /// <summary>El predicado de verdad decide: Application no tiene su propia lista de kinds (C-1).</summary>
        [Fact]
        public void TAut10_ElPredicadoInyectadoDecide_YRecibeElKindComun()
        {
            var asked = new List<string>();
            var result = RackCustomPropertiesAuthority.Evaluate(
                new[] { View("1", kind: "selective"), View("2", kind: "Selective") },
                Pick("1"),
                kind =>
                {
                    asked.Add(kind);
                    return false;
                });

            Assert.Equal(RackCustomPropertiesAuthorityOutcome.UnknownKind, result.Outcome);
            Assert.NotEmpty(asked);
            Assert.All(asked, kind => Assert.Equal("selective", kind, ignoreCase: true));
        }

        // ================================================================ T-AUT-11 xref

        [Fact]
        public void TAut11_UnaDefinicionDependiente_NoEsMiembro_NoBloqueaYNoEntraEnUnPlan()
        {
            var fresh = new[]
            {
                View("1", properties: Base),
                View("2", properties: Props(Entry(IdC, "Otra", "cosa")), dependent: true),
                Uninterpretable("3", dependent: true),
            };

            var result = Authority(fresh, "1");

            Assert.Equal(RackCustomPropertiesAuthorityOutcome.Single, result.Outcome);
            Assert.Equal(new[] { "1" }, result.Members.Select(member => member.Handle));
            Assert.Empty(result.UninterpretableDefinitions);

            var commit = CustomPropertiesCommit.ForRack(
                fresh, Pick("1"), KnownKinds, RackCustomPropertiesDisplayedState.Capture(result), CustomPropertiesIntent.ChangeValue(Id(IdA), "Nuevo"));

            Assert.True(commit.IsPlanned);
            Assert.Equal(new[] { "1" }, commit.Plan.Select(entry => entry.Handle));
        }

        [Fact]
        public void TAut11_ElegirUnaReferenciaExterna_EsXrefRejected()
        {
            var result = RackCustomPropertiesAuthority.Evaluate(new[] { View("1", properties: Base) }, Pick("X1", fromExternalReference: true), KnownKinds);

            Assert.Equal(RackCustomPropertiesAuthorityOutcome.XrefRejected, result.Outcome);
            Assert.False(result.IsWritable);
            Assert.Empty(result.Members);
            Assert.False(string.IsNullOrWhiteSpace(result.Error));
        }

        [Fact]
        public void TAut11_ElegirUnaDefinicionDependiente_EsXrefRejected()
        {
            var result = Authority("2", View("1", properties: Base), View("2", properties: Base, dependent: true));

            Assert.Equal(RackCustomPropertiesAuthorityOutcome.XrefRejected, result.Outcome);
        }

        // ================================================================ T-AUT-12 colecciones no escribibles

        public static TheoryData<string, string, CustomPropertiesReadOutcome> ReadOnlyCollections()
            => new TheoryData<string, string, CustomPropertiesReadOutcome>
            {
                { "ilegible", "\"texto\"", CustomPropertiesReadOutcome.PresentButUnreadable },
                { "identidad-ambigua", Props(Entry(IdA, "Uno", "1"), Entry(IdA.ToUpperInvariant(), "Dos", "2")), CustomPropertiesReadOutcome.AmbiguousIdentity },
                { "major-incompatible", EmptyAt("2.0"), CustomPropertiesReadOutcome.IncompatibleMajor },
                { "profundidad", Doc("1.0", "[]", ",\"Profundo\":" + Nested(16)), CustomPropertiesReadOutcome.DepthLimitExceeded },
            };

        [Theory]
        [MemberData(nameof(ReadOnlyCollections))]
        public void TAut12_UnaColeccionNoEscribible_EsCustomPropertiesReadOnly_SinOmitirEseMiembro(
            string caso, string collection, CustomPropertiesReadOutcome state)
        {
            var result = Authority("1", View("1", properties: Base), View("2", properties: collection, view: "lateral"), View("3", properties: Base, view: "planta"));

            Assert.True(result.Outcome == RackCustomPropertiesAuthorityOutcome.CustomPropertiesReadOnly, caso);
            Assert.False(result.IsWritable);
            Assert.Null(result.Collection);
            Assert.Empty(result.UnifyOptions);
            Assert.Equal(new[] { "1", "2", "3" }, result.Members.Select(member => member.Handle));
            Assert.Equal(state, Assert.Single(result.Members, member => member.Handle == "2").Collection.Outcome);
            Assert.Null(Assert.Single(result.Members, member => member.Handle == "2").CanonicalForm);
        }

        // ================================================================ T-AUT-13 sin hermana arbitraria

        public static TheoryData<string> Scenarios() => new TheoryData<string>
        {
            "single-con-minors", "divergent", "readonly", "mixed", "unknown", "indeterminate", "noidentity",
        };

        private static IReadOnlyList<RackCustomPropertiesDefinition> Scenario(string name)
        {
            switch (name)
            {
                case "single-con-minors":
                    return new[]
                    {
                        View("1", properties: PropsAt("1.2", Entry(IdA, "Cliente", "ACME"))),
                        View("2", properties: PropsAt("1.5", Entry(IdA, "Cliente", "ACME")), view: "lateral"),
                        View("3", properties: PropsAt("1.0", Entry(IdA, "Cliente", "ACME")), view: "planta"),
                    };
                case "divergent":
                    return new[]
                    {
                        View("1", properties: Base),
                        View("2", properties: Props(Entry(IdA, "Cliente", "OTRO")), view: "lateral"),
                        View("3", view: "planta"),
                    };
                case "readonly":
                    return new[] { View("1", properties: Base), View("2", properties: "[1]", view: "lateral"), View("3", properties: Props(), view: "planta") };
                case "mixed":
                    return new[] { View("1", kind: "selective"), View("2", kind: "dynamic", view: "lateral"), View("3", kind: null, view: "planta") };
                case "unknown":
                    return new[] { View("1", kind: "futuro"), View("2", kind: "FUTURO", view: "lateral"), View("3", kind: "futuro", properties: "true") };
                case "indeterminate":
                    return new[] { View("1", properties: Base), Uninterpretable("2", placed: false), Uninterpretable("3") };
                default:
                    return new[] { View("1", rackId: " ", properties: Base), View("2", properties: Base), Uninterpretable("3") };
            }
        }

        [Theory]
        [MemberData(nameof(Scenarios))]
        public void TAut13_PermutarElOrdenDelBarrido_NoCambiaElResultado(string escenario)
        {
            var definitions = Scenario(escenario);
            var expected = Signature(Authority(definitions, "1"));

            foreach (var permutation in Permutations(definitions))
            {
                Assert.Equal(expected, Signature(Authority(permutation, "1")));
            }
        }

        [Theory]
        [MemberData(nameof(Scenarios))]
        public void TAut13_NingunResultadoDistintoDeSingle_ExponeUnaColeccionComoLaDelRack(string escenario)
        {
            var result = Authority(Scenario(escenario), "1");

            if (result.Outcome == RackCustomPropertiesAuthorityOutcome.Single)
            {
                Assert.NotNull(result.Collection);
                return;
            }

            Assert.Null(result.Collection);
            Assert.Null(result.WriteVersion);
            Assert.False(result.IsWritable);
            Assert.Empty(CustomPropertiesWorkspace.ForRack(result).Rows);
        }

        // ================================================================ T-AUT-14 matriz de unificacion

        private static readonly string Clean = Props(Entry(IdA, "Cliente", "ACME"));

        private static readonly string Different = Props(Entry(IdA, "Cliente", "OTRO"));

        private static RackCustomPropertiesUnifyOption Option(RackCustomPropertiesAuthorityResult result, string source)
            => Assert.Single(result.UnifyOptions, option => option.SourceHandle == source);

        [Fact]
        public void TAut14_DivergentConColeccionesLimpias_CualquierVistaPuedeSerOrigen_SinOrigenPorDefecto()
        {
            var result = Authority("1", View("1", properties: Clean), View("2", properties: Different, view: "lateral"));

            Assert.Equal(RackCustomPropertiesAuthorityOutcome.Divergent, result.Outcome);
            Assert.Equal(new[] { "1", "2" }, result.UnifyOptions.Select(option => option.SourceHandle));
            Assert.All(result.UnifyOptions, option => Assert.True(option.IsAvailable));
            Assert.All(result.UnifyOptions, option => Assert.Empty(option.BlockingHandles));
        }

        [Fact]
        public void TAut14_ExtensionDataDeRaizEnUnaVistaQueSeSobrescribiria_BloqueaEseOrigen_PeroPuedeSerOrigen()
        {
            var withExtension = Doc("1.0", Entries(Entry(IdA, "Cliente", "OTRO")), ",\"Raiz\":{\"futuro\":1}");
            var result = Authority("1", View("1", properties: Clean), View("2", properties: withExtension, view: "lateral"));

            Assert.False(Option(result, "1").IsAvailable);
            Assert.Equal(new[] { "2" }, Option(result, "1").BlockingHandles);
            Assert.True(Option(result, "2").IsAvailable);
        }

        [Fact]
        public void TAut14_ExtensionDataDeEntradaEnUnaVistaQueSeSobrescribiria_BloqueaEseOrigen()
        {
            var withEntryExtension = Props(Entry(IdA, "Cliente", "OTRO", ",\"Nota\":1"));
            var result = Authority("1", View("1", properties: Clean), View("2", properties: withEntryExtension, view: "lateral"));

            Assert.False(Option(result, "1").IsAvailable);
            Assert.True(Option(result, "2").IsAvailable);
        }

        [Fact]
        public void TAut14_UnMinorMayorEnUnaVistaQueSeSobrescribiria_BloqueaEseOrigen()
        {
            var result = Authority(
                "1",
                View("1", properties: PropsAt("1.1", Entry(IdA, "Cliente", "ACME"))),
                View("2", properties: PropsAt("1.4", Entry(IdA, "Cliente", "OTRO")), view: "lateral"));

            Assert.False(Option(result, "1").IsAvailable);
            Assert.True(Option(result, "2").IsAvailable);
        }

        /// <summary>Una vista canonicamente igual al origen no se sobrescribe: su contenido desconocido no bloquea.</summary>
        [Fact]
        public void TAut14_UnaVistaIgualAlOrigen_NoSeSobrescribe_YSuExtensionDataNoBloquea()
        {
            var withExtension = Doc("1.3", Entries(Entry(IdA, "Cliente", "ACME")), ",\"Raiz\":{\"futuro\":1}");
            var sameAsSource = Doc("1.0", Entries(Entry(IdA, "Cliente", "ACME")), ",\"Raiz\":{\"futuro\":1}");
            var result = Authority(
                "1",
                View("1", properties: withExtension),
                View("2", properties: sameAsSource, view: "lateral"),
                View("3", properties: Different, view: "planta"));

            Assert.True(Option(result, "1").IsAvailable);
            Assert.False(Option(result, "3").IsAvailable);
            Assert.Equal(new[] { "1", "2" }, Option(result, "3").BlockingHandles);
        }

        [Fact]
        public void TAut14_OrigenAbsent_TieneElMinorActual_YUnaVistaA13LoBloquea()
        {
            var result = Authority(
                "1",
                View("1"),
                View("2", properties: PropsAt("1.3", Entry(IdA, "Cliente", "ACME")), view: "lateral"),
                View("3", properties: PropsAt("1.0", Entry(IdA, "Cliente", "OTRO")), view: "planta"));

            Assert.Equal(RackCustomPropertiesAuthorityOutcome.Divergent, result.Outcome);
            Assert.False(Option(result, "1").IsAvailable);
            Assert.Contains("2", Option(result, "1").BlockingHandles);
        }

        /// <summary>Origen <c>Absent</c>: el documento origen es el vacio canonico (C-5A).</summary>
        [Fact]
        public void TAut14_OrigenAbsent_ElDocumentoOrigenEsElVacioCanonico()
        {
            var fresh = new[] { View("1"), View("2", properties: Clean, view: "lateral") };
            var snapshot = Authority(fresh, "1");
            Assert.True(Option(snapshot, "1").IsAvailable);

            var commit = CustomPropertiesCommit.ForRackUnify(
                fresh, Pick("1"), KnownKinds, RackCustomPropertiesUnifyIntent.Create(RackCustomPropertiesDisplayedState.Capture(snapshot), "1", confirmed: true));

            Assert.True(commit.IsPlanned);
            Assert.Equal(CustomPropertiesDocument.CurrentSchemaVersion, commit.Document.SchemaVersion);
            Assert.Empty(commit.Document.Entries);
            Assert.True(commit.Document.ExtensionData == null || commit.Document.ExtensionData.Count == 0);
        }

        public static TheoryData<string> BlockedStates() => new TheoryData<string> { "readonly", "profundidad", "unknown", "mixed", "indeterminate" };

        [Theory]
        [MemberData(nameof(BlockedStates))]
        public void TAut14_SinDivergentNoHayUnificacion_NiConMiembrosNoEscribiblesNiConKindDesconocido(string caso)
        {
            RackCustomPropertiesDefinition[] definitions;

            switch (caso)
            {
                case "readonly":
                    definitions = new[] { View("1", properties: Clean), View("2", properties: Different), View("3", properties: "\"x\"") };
                    break;
                case "profundidad":
                    definitions = new[] { View("1", properties: Clean), View("2", properties: Different), View("3", properties: Doc("1.0", "[]", ",\"P\":" + Nested(16))) };
                    break;
                case "unknown":
                    definitions = new[] { View("1", properties: Clean, kind: "futuro"), View("2", properties: Different, kind: "futuro") };
                    break;
                case "mixed":
                    definitions = new[] { View("1", properties: Clean, kind: "selective"), View("2", properties: Different, kind: "dynamic") };
                    break;
                default:
                    definitions = new[] { View("1", properties: Clean), View("2", properties: Different), Uninterpretable("3") };
                    break;
            }

            var result = Authority(definitions, "1");

            Assert.NotEqual(RackCustomPropertiesAuthorityOutcome.Divergent, result.Outcome);
            Assert.Empty(result.UnifyOptions);
        }

        // ================================================================ T-AUT-15 orden unico

        public static TheoryData<string, RackCustomPropertiesAuthorityOutcome> Precedences()
            => new TheoryData<string, RackCustomPropertiesAuthorityOutcome>
            {
                { "xref-gana-a-todo", RackCustomPropertiesAuthorityOutcome.XrefRejected },
                { "noidentity-gana-a-indeterminate", RackCustomPropertiesAuthorityOutcome.NoIdentity },
                { "indeterminate-gana-a-mixed-unknown-readonly-divergent", RackCustomPropertiesAuthorityOutcome.IndeterminateMembership },
                { "mixed-gana-a-unknown-readonly-divergent", RackCustomPropertiesAuthorityOutcome.MixedKind },
                { "unknown-gana-a-readonly-divergent", RackCustomPropertiesAuthorityOutcome.UnknownKind },
                { "readonly-gana-a-divergent", RackCustomPropertiesAuthorityOutcome.CustomPropertiesReadOnly },
                { "divergent-gana-a-single", RackCustomPropertiesAuthorityOutcome.Divergent },
                { "single", RackCustomPropertiesAuthorityOutcome.Single },
            };

        private static IReadOnlyList<RackCustomPropertiesDefinition> Precedence(string caso)
        {
            switch (caso)
            {
                case "xref-gana-a-todo":
                case "noidentity-gana-a-indeterminate":
                    return new[]
                    {
                        View("1", rackId: caso.StartsWith("noidentity", StringComparison.Ordinal) ? "" : RackA, properties: Clean,
                            dependent: caso.StartsWith("xref", StringComparison.Ordinal)),
                        Uninterpretable("2"),
                        View("3", kind: null),
                        View("4", properties: "\"x\"", kind: "futuro"),
                    };
                case "indeterminate-gana-a-mixed-unknown-readonly-divergent":
                    return new[] { View("1", properties: Clean), Uninterpretable("2", placed: false), View("3", kind: null), View("4", properties: "\"x\""), View("5", properties: Different) };
                case "mixed-gana-a-unknown-readonly-divergent":
                    return new[] { View("1", properties: Clean, kind: "futuro"), View("2", kind: null, properties: "\"x\""), View("3", properties: Different, kind: "futuro") };
                case "unknown-gana-a-readonly-divergent":
                    return new[] { View("1", properties: Clean, kind: "futuro"), View("2", properties: "\"x\"", kind: "futuro"), View("3", properties: Different, kind: "futuro") };
                case "readonly-gana-a-divergent":
                    return new[] { View("1", properties: Clean), View("2", properties: "\"x\""), View("3", properties: Different) };
                case "divergent-gana-a-single":
                    return new[] { View("1", properties: Clean), View("2", properties: Different), View("3", properties: Clean) };
                default:
                    return new[] { View("1", properties: Clean), View("2", properties: Clean), View("3", properties: PropsAt("1.4", Entry(IdA, "Cliente", "ACME"))) };
            }
        }

        [Theory]
        [MemberData(nameof(Precedences))]
        public void TAut15_ConVariasCausas_GanaLaPrimeraDelOrdenUnico_SeaCualSeaElOrdenDelBarrido(
            string caso, RackCustomPropertiesAuthorityOutcome expected)
        {
            var definitions = Precedence(caso);

            foreach (var permutation in Permutations(definitions))
            {
                Assert.Equal(expected, Authority(permutation, "1").Outcome);
            }
        }

        [Fact]
        public void TAut15_LosOchoResultadosTienenElOrdenDeD098()
        {
            Assert.Equal(
                new[]
                {
                    "XrefRejected", "NoIdentity", "IndeterminateMembership", "MixedKind",
                    "UnknownKind", "CustomPropertiesReadOnly", "Divergent", "Single",
                },
                Enum.GetValues(typeof(RackCustomPropertiesAuthorityOutcome))
                    .Cast<RackCustomPropertiesAuthorityOutcome>()
                    .OrderBy(outcome => (int)outcome)
                    .Select(outcome => outcome.ToString()));
        }

        // ================================================================ igualdad canonica (D-09.7)

        [Fact]
        public void IgualdadCanonica_SoloExisteParaColeccionesEscribibles()
        {
            var unreadable = ReadAsElement("\"texto\"");

            Assert.Throws<InvalidOperationException>(() => CustomPropertiesCanonicalForm.Of(unreadable));
            Assert.Equal(CustomPropertiesCanonicalForm.Of(AbsentResult()), CustomPropertiesCanonicalForm.Of(ReadAsElement(EmptyAt("1.9"))));
            Assert.True(CustomPropertiesCanonicalForm.AreEqual(AbsentResult(), ReadAsElement(EmptyAt("1.0"))));
            Assert.False(CustomPropertiesCanonicalForm.AreEqual(AbsentResult(), ReadAsElement(Clean)));
        }
    }
}
