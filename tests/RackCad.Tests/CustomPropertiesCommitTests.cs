using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using RackCad.Application.CustomProperties;
using RackCad.Application.Persistence;
using Xunit;
using static RackCad.Tests.CustomPropertiesAuthorityTestKit;
using static RackCad.Tests.CustomPropertiesTestKit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-54 G5 — preflight sobre la instantanea y commit de Application sobre la proyeccion fresca (Proposal V5 D-09.10,
    /// D-10 y D-22; §12.4 T-MUT-07, T-MUT-08 y T-MUT-10). El commit devuelve un plan <c>(handle, payload)</c> completo o
    /// una negativa tipada; nunca escribe, y el Plugin de G6 solo ejecutara lo que reciba.
    /// </summary>
    public class CustomPropertiesCommitTests
    {
        private static readonly string Base = Props(Entry(IdA, "Cliente", "ACME"), Entry(IdB, "Area", "Norte"));

        private static readonly string Other = Props(Entry(IdA, "Cliente", "OTRO"));

        private static RackCustomPropertiesDefinition[] Snapshot()
            => new[] { View("1", properties: Base), View("2", properties: Base, view: "lateral", section: 1) };

        private static RackCustomPropertiesDisplayedState Displayed(IEnumerable<RackCustomPropertiesDefinition> snapshot)
            => RackCustomPropertiesDisplayedState.Capture(Authority(snapshot, "1"));

        private static CustomPropertiesCommitResult Commit(
            IEnumerable<RackCustomPropertiesDefinition> fresh, RackCustomPropertiesDisplayedState displayed, CustomPropertiesIntent intent, Func<string, bool> known = null)
            => CustomPropertiesCommit.ForRack(fresh, Pick("1"), known ?? KnownKinds, displayed, intent);

        private static void AssertRefused(CustomPropertiesCommitResult result, CustomPropertiesCommitRefusal refusal)
        {
            Assert.False(result.IsPlanned);
            Assert.Equal(refusal, result.Refusal);
            Assert.Empty(result.Plan);
            Assert.Null(result.Document);
            Assert.False(string.IsNullOrWhiteSpace(result.Error));
        }

        // ================================================================ preflight sobre la instantanea

        [Fact]
        public void Preflight_UnIntentValidoSobreSingle_SeAcepta()
        {
            var snapshot = Authority(Snapshot(), "1");

            var result = CustomPropertiesPreflight.ForRack(snapshot, CustomPropertiesIntent.Rename(Id(IdA), "Cliente final"));

            Assert.True(result.IsAccepted);
            Assert.Equal(CustomPropertiesCommitRefusal.None, result.Refusal);
        }

        [Fact]
        public void Preflight_UnIntentQueG3Rechaza_SeRechazaConElMotivoTipado()
        {
            var snapshot = Authority(Snapshot(), "1");

            var result = CustomPropertiesPreflight.ForRack(snapshot, CustomPropertiesIntent.Rename(Id(IdA), "Area"));

            Assert.False(result.IsAccepted);
            Assert.Equal(CustomPropertiesCommitRefusal.MutationRejected, result.Refusal);
            Assert.Equal(CustomPropertiesRejection.NameCollision, result.MutationRejection);
        }

        [Fact]
        public void Preflight_SinSingle_SeRechaza()
        {
            var snapshot = Authority("1", View("1", properties: Base), View("2", properties: Other));

            var result = CustomPropertiesPreflight.ForRack(snapshot, CustomPropertiesIntent.Create("Nueva", "x"));

            Assert.False(result.IsAccepted);
            Assert.Equal(CustomPropertiesCommitRefusal.AuthorityNotWritable, result.Refusal);
        }

        // ================================================================ T-MUT-07 instantanea obsoleta

        public static TheoryData<string, CustomPropertiesCommitRefusal> StaleSnapshots()
            => new TheoryData<string, CustomPropertiesCommitRefusal>
            {
                { "entrada-borrada", CustomPropertiesCommitRefusal.MutationRejected },
                { "nombre-ocupado", CustomPropertiesCommitRefusal.MutationRejected },
                { "miembro-ilegible", CustomPropertiesCommitRefusal.AuthorityNotWritable },
                { "payload-ilegible-nuevo", CustomPropertiesCommitRefusal.AuthorityNotWritable },
                { "autoridad-divergent", CustomPropertiesCommitRefusal.AuthorityNotWritable },
                { "miembro-anadido", CustomPropertiesCommitRefusal.MemberSetChanged },
                { "miembro-eliminado", CustomPropertiesCommitRefusal.MemberSetChanged },
                { "kind-desconocido", CustomPropertiesCommitRefusal.AuthorityNotWritable },
                { "profundidad", CustomPropertiesCommitRefusal.AuthorityNotWritable },
                { "seleccion-desaparecida", CustomPropertiesCommitRefusal.SelectionMissing },
                { "rack-cambiado", CustomPropertiesCommitRefusal.RackChanged },
            };

        /// <summary>
        /// Con el intent aceptado por el preflight sobre la instantanea, la lectura fresca difiere: el commit aborta sin plan.
        /// El preflight es aviso temprano y nunca autoridad (D-22).
        /// </summary>
        [Theory]
        [MemberData(nameof(StaleSnapshots))]
        public void TMut07_LaLecturaFrescaDifiere_ElCommitAbortaSinPlan(string caso, CustomPropertiesCommitRefusal refusal)
        {
            var snapshot = Snapshot();
            var displayed = Displayed(snapshot);
            var intent = caso == "nombre-ocupado"
                ? CustomPropertiesIntent.Create("Codigo", "C-1")
                : CustomPropertiesIntent.ChangeValue(Id(IdA), "Nuevo cliente");

            Assert.True(CustomPropertiesPreflight.ForRack(Authority(snapshot, "1"), intent).IsAccepted, caso);

            IReadOnlyList<RackCustomPropertiesDefinition> fresh;
            Func<string, bool> known = KnownKinds;
            var deleted = Props(Entry(IdB, "Area", "Norte"));
            var occupied = Props(Entry(IdA, "Cliente", "ACME"), Entry(IdB, "Area", "Norte"), Entry(IdC, "Codigo", "X"));

            switch (caso)
            {
                case "entrada-borrada":
                    fresh = new[] { View("1", properties: deleted), View("2", properties: deleted, view: "lateral", section: 1) };
                    break;
                case "nombre-ocupado":
                    fresh = new[] { View("1", properties: occupied), View("2", properties: occupied, view: "lateral", section: 1) };
                    break;
                case "miembro-ilegible":
                    fresh = new[] { View("1", properties: Base), View("2", properties: "\"roto\"", view: "lateral", section: 1) };
                    break;
                case "payload-ilegible-nuevo":
                    fresh = snapshot.Concat(new[] { Uninterpretable("99", placed: false) }).ToList();
                    break;
                case "autoridad-divergent":
                    fresh = new[] { View("1", properties: Base), View("2", properties: Other, view: "lateral", section: 1) };
                    break;
                case "miembro-anadido":
                    fresh = snapshot.Concat(new[] { View("3", properties: Base, view: "planta") }).ToList();
                    break;
                case "miembro-eliminado":
                    fresh = new[] { snapshot[0] };
                    break;
                case "kind-desconocido":
                    fresh = snapshot;
                    known = kind => false;
                    break;
                case "profundidad":
                    fresh = new[] { View("1", properties: Base), View("2", properties: Doc("1.0", "[]", ",\"P\":" + Nested(16)), view: "lateral", section: 1) };
                    break;
                case "seleccion-desaparecida":
                    fresh = new[] { snapshot[1] };
                    break;
                default:
                    fresh = new[] { View("1", rackId: RackB, properties: Base), View("2", properties: Base, view: "lateral", section: 1) };
                    break;
            }

            AssertRefused(Commit(fresh, displayed, intent, known), refusal);
        }

        [Fact]
        public void TMut07_LasNegativasDeG3LlevanSuMotivo()
        {
            var displayed = Displayed(Snapshot());
            var deleted = Props(Entry(IdB, "Area", "Norte"));

            var result = Commit(new[] { View("1", properties: deleted), View("2", properties: deleted, view: "lateral", section: 1) }, displayed,
                CustomPropertiesIntent.Delete(Id(IdA)));

            Assert.Equal(CustomPropertiesRejection.EntryNotFound, result.MutationRejection);
        }

        [Fact]
        public void TMut07_ElNombreNuncaLocaliza_SiElIdYaNoExiste_AunqueOtraEntradaSeLlameIgual()
        {
            var displayed = Displayed(Snapshot());
            var renamedId = Props(Entry(IdC, "Cliente", "ACME"), Entry(IdB, "Area", "Norte"));

            var result = Commit(
                new[] { View("1", properties: renamedId), View("2", properties: renamedId, view: "lateral", section: 1) },
                displayed,
                CustomPropertiesIntent.ChangeValue(Id(IdA), "Nuevo"));

            AssertRefused(result, CustomPropertiesCommitRefusal.MutationRejected);
            Assert.Equal(CustomPropertiesRejection.EntryNotFound, result.MutationRejection);
        }

        [Fact]
        public void TMut07_UnificarConElOrigenCambiado_OConCualquierDestinoCambiado_Aborta()
        {
            var snapshot = new[] { View("1", properties: Base), View("2", properties: Other, view: "lateral"), View("3", properties: Base, view: "planta") };
            var intent = RackCustomPropertiesUnifyIntent.Create(Displayed(snapshot), "1", confirmed: true);

            foreach (var fresh in new[]
            {
                new[] { View("1", properties: Props(Entry(IdA, "Cliente", "CAMBIADO"))), snapshot[1], snapshot[2] },
                new[] { snapshot[0], View("2", properties: Props(Entry(IdA, "Cliente", "OTRO2")), view: "lateral"), snapshot[2] },
                new[] { snapshot[0], snapshot[1], View("3", properties: Props(Entry(IdC, "Nueva", "1")), view: "planta") },
            })
            {
                AssertRefused(
                    CustomPropertiesCommit.ForRackUnify(fresh, Pick("1"), KnownKinds, intent),
                    CustomPropertiesCommitRefusal.DisplayedStateChanged);
            }
        }

        // ================================================================ T-MUT-08 plan y atomicidad logica

        [Fact]
        public void TMut08_AlEditar_ElPlanIncluyeTodosLosMiembros_EnOrdenDeterminista()
        {
            var fresh = new[]
            {
                View("C", properties: PropsAt("1.7", Entry(IdA, "Cliente", "ACME"), Entry(IdB, "Area", "Norte")), view: "planta"),
                View("A", properties: Base),
                View("B", properties: Base, view: "lateral", section: 2, extra: ",\"Futuro\":{\"x\":[1,2.50]}"),
            };
            var displayed = RackCustomPropertiesDisplayedState.Capture(Authority(fresh, "A"));

            var result = CustomPropertiesCommit.ForRack(fresh, Pick("A"), KnownKinds, displayed, CustomPropertiesIntent.Rename(Id(IdA), "Cliente final"));

            Assert.True(result.IsPlanned);
            Assert.Equal(CustomPropertiesCommitRefusal.None, result.Refusal);
            Assert.Equal(new[] { "A", "B", "C" }, result.Plan.Select(entry => entry.Handle));
            Assert.Equal("1.7", result.Document.SchemaVersion);
            Assert.Equal(new[] { "Cliente final", "Area" }, result.Document.Entries.Select(entry => entry.Name));
        }

        /// <summary>Cada payload parte del sobre FRESCO de su miembro y solo cambia en el miembro de propiedades (D-22.3, INV-04).</summary>
        [Fact]
        public void TMut08_CadaPayloadEsElSobreFrescoDeSuMiembro_ConSoloElMiembroDePropiedadesCambiado()
        {
            var fresh = new[]
            {
                View("A", properties: Base),
                View("B", properties: Base, kind: "Selective", view: "lateral", section: 2, extra: ",\"Futuro\":{\"x\":[1,2.50]}"),
            };
            var displayed = RackCustomPropertiesDisplayedState.Capture(Authority(fresh, "A"));

            var result = CustomPropertiesCommit.ForRack(fresh, Pick("A"), KnownKinds, displayed, CustomPropertiesIntent.ChangeValue(Id(IdB), "Sur"));
            var store = new RackEmbedStore();
            var expectedMember = new CustomPropertiesStore().Serialize(result.Document);

            foreach (var entry in result.Plan)
            {
                var before = fresh.Single(definition => definition.Handle == entry.Handle).Envelope;
                var after = store.Deserialize(entry.Payload);

                Assert.Equal(before.Kind, after.Kind);
                Assert.Equal(before.Id, after.Id);
                Assert.Equal(before.Name, after.Name);
                Assert.Equal(before.View, after.View);
                Assert.Equal(before.Section, after.Section);
                Assert.Equal(before.Design, after.Design);
                Assert.Equal(before.SchemaVersion, after.SchemaVersion);
                Assert.Equal(
                    before.ExtensionData?.Select(pair => pair.Key + "=" + pair.Value.GetRawText()) ?? Enumerable.Empty<string>(),
                    after.ExtensionData?.Select(pair => pair.Key + "=" + pair.Value.GetRawText()) ?? Enumerable.Empty<string>());
                Assert.Equal(expectedMember, after.CustomProperties.Value.GetRawText());
            }
        }

        [Fact]
        public void TMut08_TrasEjecutarElPlan_TodosLosMiembrosQuedanCanonicamenteIgualesAlDestino()
        {
            var fresh = new[]
            {
                View("1"),
                View("2", properties: EmptyAt("1.2"), view: "lateral"),
                View("3", properties: EmptyAt("1.0"), view: "planta"),
            };
            var displayed = RackCustomPropertiesDisplayedState.Capture(Authority(fresh, "1"));

            var result = CustomPropertiesCommit.ForRack(fresh, Pick("1"), KnownKinds, displayed, CustomPropertiesIntent.Create("Cliente", "ACME"));

            Assert.True(result.IsPlanned);
            Assert.NotEqual(default(CustomPropertyId), result.CreatedId);

            var after = Authority(AfterPlan(fresh, result), "1");
            Assert.Equal(RackCustomPropertiesAuthorityOutcome.Single, after.Outcome);
            Assert.Equal(
                CustomPropertiesCanonicalForm.Of(new CustomPropertiesStore().ReadElement(Element(new CustomPropertiesStore().Serialize(result.Document)))),
                CustomPropertiesCanonicalForm.Of(after.Collection));
            Assert.Equal("1.2", after.WriteVersion);
        }

        [Fact]
        public void TMut08_NuncaEntranDependientesDeXref()
        {
            var fresh = new[] { View("1", properties: Base), View("2", properties: Base, dependent: true) };
            var displayed = RackCustomPropertiesDisplayedState.Capture(Authority(fresh, "1"));

            var result = CustomPropertiesCommit.ForRack(fresh, Pick("1"), KnownKinds, displayed, CustomPropertiesIntent.Delete(Id(IdB)));

            Assert.Equal(new[] { "1" }, result.Plan.Select(entry => entry.Handle));
        }

        [Fact]
        public void TMut08_AlUnificar_SoloEntranLosMiembrosDistintosDelOrigen_YUnBtrYaIgualNoSeReescribe()
        {
            var fresh = new[]
            {
                View("1", properties: PropsAt("1.3", Entry(IdA, "Cliente", "ACME"))),
                View("2", properties: Other, view: "lateral"),
                View("3", properties: PropsAt("1.0", Entry(IdA, "Cliente", "ACME")), view: "planta"),
            };
            var snapshot = Authority(fresh, "1");

            var result = CustomPropertiesCommit.ForRackUnify(
                fresh, Pick("1"), KnownKinds, RackCustomPropertiesUnifyIntent.Create(RackCustomPropertiesDisplayedState.Capture(snapshot), "1", confirmed: true));

            Assert.True(result.IsPlanned);
            Assert.Equal(new[] { "2" }, result.Plan.Select(entry => entry.Handle));

            var after = Authority(AfterPlan(fresh, result), "1");
            Assert.Equal(RackCustomPropertiesAuthorityOutcome.Single, after.Outcome);
            Assert.Equal("1.3", after.WriteVersion);
            Assert.Equal("1.0", AfterPlan(fresh, result).Single(definition => definition.Handle == "3").Envelope.CustomProperties.Value.GetProperty("SchemaVersion").GetString());
        }

        /// <summary>
        /// D-22.10, mitad de Application: si el sobre fresco de un miembro no se puede reserializar (F-14b en otro campo),
        /// la excepcion se propaga y no hay plan, ni parcial: el Plugin no recibe nada que escribir. No se corrige F-14b.
        /// </summary>
        [Fact]
        public void TMut08_SiSerializarUnMiembroLanza_LaExcepcionSePropaga_YNoHayPlan()
        {
            var brokenOtherField = ",\"Futuro\":{\"nota\":\"" + JsonEscape(0xDC00) + "\"}";
            var fresh = new[] { View("1", properties: Base), View("2", properties: Base, view: "lateral", extra: brokenOtherField) };
            var displayed = RackCustomPropertiesDisplayedState.Capture(Authority(fresh, "1"));

            Assert.Throws<JsonException>(() =>
                CustomPropertiesCommit.ForRack(fresh, Pick("1"), KnownKinds, displayed, CustomPropertiesIntent.ChangeValue(Id(IdA), "Nuevo")));
        }

        [Fact]
        public void TMut08_SiSerializarUnDestinoDeLaUnificacionLanza_LaExcepcionSePropaga()
        {
            var brokenOtherField = ",\"Futuro\":{\"" + JsonEscape(0xD800) + "\":1}";
            var fresh = new[] { View("1", properties: Base), View("2", properties: Other, view: "lateral", extra: brokenOtherField) };
            var intent = RackCustomPropertiesUnifyIntent.Create(RackCustomPropertiesDisplayedState.Capture(Authority(fresh, "1")), "1", confirmed: true);

            Assert.Throws<JsonException>(() => CustomPropertiesCommit.ForRackUnify(fresh, Pick("1"), KnownKinds, intent));
        }

        // ================================================================ T-MUT-10 commit de unificacion

        private static RackCustomPropertiesDefinition[] Divergent()
            => new[]
            {
                View("1", properties: Base),
                View("2", properties: Other, view: "lateral", section: 1),
                View("3", properties: Base, view: "planta"),
            };

        private static RackCustomPropertiesUnifyIntent Unify(IEnumerable<RackCustomPropertiesDefinition> snapshot, string source, bool confirmed = true)
            => RackCustomPropertiesUnifyIntent.Create(RackCustomPropertiesDisplayedState.Capture(Authority(snapshot, "1")), source, confirmed);

        [Fact]
        public void TMut10_ConTodasLasVistasComoSeMostraron_ElCommitTieneExito()
        {
            var snapshot = Divergent();
            var intent = Unify(snapshot, "1");

            Assert.True(CustomPropertiesPreflight.ForRackUnify(Authority(snapshot, "1"), intent).IsAccepted);

            var result = CustomPropertiesCommit.ForRackUnify(Divergent(), Pick("1"), KnownKinds, intent);

            Assert.True(result.IsPlanned);
            Assert.Equal(new[] { "2" }, result.Plan.Select(entry => entry.Handle));
            Assert.Equal(new[] { "ACME", "Norte" }, result.Document.Entries.Select(entry => entry.Value));

            var after = Authority(AfterPlan(Divergent(), result), "1");
            Assert.Equal(RackCustomPropertiesAuthorityOutcome.Single, after.Outcome);
        }

        [Fact]
        public void TMut10_ConElOrigenCambiado_Aborta()
        {
            var intent = Unify(Divergent(), "1");
            var fresh = Divergent();
            fresh[0] = View("1", properties: Props(Entry(IdA, "Cliente", "CAMBIADO")));

            AssertRefused(CustomPropertiesCommit.ForRackUnify(fresh, Pick("1"), KnownKinds, intent), CustomPropertiesCommitRefusal.DisplayedStateChanged);
        }

        [Theory]
        [InlineData("2")]
        [InlineData("3")]
        public void TMut10_ConCualquierDestinoCambiado_Aborta_AunqueEsaVistaFueraIgualAlOrigen(string destination)
        {
            var intent = Unify(Divergent(), "1");
            var fresh = Divergent();
            var index = destination == "2" ? 1 : 2;
            fresh[index] = View(destination, properties: Props(Entry(IdC, "Distinta", "!")), view: fresh[index].Envelope.View, section: fresh[index].Envelope.Section);

            AssertRefused(CustomPropertiesCommit.ForRackUnify(fresh, Pick("1"), KnownKinds, intent), CustomPropertiesCommitRefusal.DisplayedStateChanged);
        }

        [Fact]
        public void TMut10_ConElConjuntoDeMiembrosCambiado_Aborta()
        {
            var intent = Unify(Divergent(), "1");

            AssertRefused(
                CustomPropertiesCommit.ForRackUnify(Divergent().Concat(new[] { View("4", properties: Base) }), Pick("1"), KnownKinds, intent),
                CustomPropertiesCommitRefusal.MemberSetChanged);
            AssertRefused(
                CustomPropertiesCommit.ForRackUnify(Divergent().Take(2), Pick("1"), KnownKinds, intent),
                CustomPropertiesCommitRefusal.MemberSetChanged);
        }

        [Fact]
        public void TMut10_ConElOrigenAbsent_SeEscribeElVacioCanonico_SoloEnLosMiembrosConContenidoDistinto()
        {
            var snapshot = new[]
            {
                View("1"),
                View("2", properties: Props(Entry(IdA, "Cliente", "ACME")), view: "lateral"),
                View("3", properties: EmptyAt("1.0"), view: "planta"),
            };
            var result = CustomPropertiesCommit.ForRackUnify(snapshot, Pick("1"), KnownKinds, Unify(snapshot, "1"));

            Assert.True(result.IsPlanned);
            Assert.Equal(new[] { "2" }, result.Plan.Select(entry => entry.Handle));
            Assert.Equal(
                "{\"SchemaVersion\":\"1.0\",\"Entries\":[]}",
                new RackEmbedStore().Deserialize(result.Plan[0].Payload).CustomProperties.Value.GetRawText());
        }

        /// <summary>Las condiciones de D-09.10 se reevaluan en fresco: con las formas mostradas intactas, un minor mayor bloquea.</summary>
        [Fact]
        public void TMut10_LasCondicionesSeReevaluanEnFresco()
        {
            var intent = Unify(Divergent(), "1");
            var fresh = Divergent();
            fresh[1] = View("2", properties: PropsAt("1.5", Entry(IdA, "Cliente", "OTRO")), view: "lateral", section: 1);

            AssertRefused(CustomPropertiesCommit.ForRackUnify(fresh, Pick("1"), KnownKinds, intent), CustomPropertiesCommitRefusal.UnifyUnavailable);
        }

        [Fact]
        public void TMut10_SinConfirmar_NoHayPlan()
        {
            var intent = Unify(Divergent(), "1", confirmed: false);

            Assert.False(CustomPropertiesPreflight.ForRackUnify(Authority(Divergent(), "1"), intent).IsAccepted);
            AssertRefused(CustomPropertiesCommit.ForRackUnify(Divergent(), Pick("1"), KnownKinds, intent), CustomPropertiesCommitRefusal.NotConfirmed);
        }

        [Fact]
        public void TMut10_ElOrigenEsExplicito_NoHayOrigenPorDefecto()
        {
            var displayed = RackCustomPropertiesDisplayedState.Capture(Authority(Divergent(), "1"));

            Assert.Throws<ArgumentException>(() => RackCustomPropertiesUnifyIntent.Create(displayed, null, confirmed: true));
            Assert.Throws<ArgumentException>(() => RackCustomPropertiesUnifyIntent.Create(displayed, " ", confirmed: true));
            Assert.Throws<ArgumentException>(() => RackCustomPropertiesUnifyIntent.Create(displayed, "no-es-miembro", confirmed: true));
        }

        [Fact]
        public void TMut10_UnificarExigeDivergentSeguro_EnLaInstantaneaYEnFresco()
        {
            var readOnly = Authority("1", View("1", properties: Base), View("2", properties: "\"x\""));

            Assert.Throws<InvalidOperationException>(() => RackCustomPropertiesDisplayedState.Capture(readOnly));

            var unsafeSnapshot = new[]
            {
                View("1", properties: Base),
                View("2", properties: Doc("1.0", Entries(Entry(IdA, "Cliente", "OTRO")), ",\"Raiz\":1"), view: "lateral"),
            };
            var intent = Unify(unsafeSnapshot, "1");

            Assert.False(CustomPropertiesPreflight.ForRackUnify(Authority(unsafeSnapshot, "1"), intent).IsAccepted);
            AssertRefused(CustomPropertiesCommit.ForRackUnify(unsafeSnapshot, Pick("1"), KnownKinds, intent), CustomPropertiesCommitRefusal.UnifyUnavailable);
        }

        [Fact]
        public void TMut10_UnaLecturaFrescaDeSoloLectura_AbortaAntesDeComparar()
        {
            var intent = Unify(Divergent(), "1");
            var fresh = Divergent().Concat(new[] { Uninterpretable("99") });

            var result = CustomPropertiesCommit.ForRackUnify(fresh, Pick("1"), KnownKinds, intent);

            AssertRefused(result, CustomPropertiesCommitRefusal.AuthorityNotWritable);
            Assert.Equal(RackCustomPropertiesAuthorityOutcome.IndeterminateMembership, result.FreshOutcome);
        }
    }
}
