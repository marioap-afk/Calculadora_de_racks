using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Domain.Systems.Selective;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-47 gate G5 — quien consume una variable, cual es la autoridad de un rack, y sobre todo QUE NO SE
    /// PUEDE AFIRMAR.
    ///
    /// <para>
    /// Todo el gate gira sobre una sola regla: <b>lo desconocido no cae del lado del no</b>. <c>NEGATIVE</c>
    /// significa, literalmente, «se comprendio TODO el mapa de vinculos y ninguno usa el objetivo». Cualquier
    /// cosa que este build no entienda —un <c>PropertyId</c> nuevo, un <c>kind</c> del futuro, un id
    /// ilegible, un payload que no deserializa— es <c>INDETERMINATE</c>: una respuesta que esta version no
    /// puede dar. Colapsarla en <c>NEGATIVE</c> convertiria un dibujo escrito por una version posterior en un
    /// dibujo silenciosamente ajeno a la variable, y la propagacion dejaria vistas sin redibujar mostrando el
    /// valor viejo.
    /// </para>
    /// <para>
    /// Las dos familias existen por una razon concreta y asimetrica. En <b>target-variable</b> el conjunto de
    /// racks afectados SE DESCUBRE, asi que el probe corre PRIMERO: a un rack cuyas vistas dicen todas
    /// <c>NEGATIVE</c> no se le exige igualdad authored, porque su divergencia —real— es ajena a esta
    /// operacion. En <b>target-rack</b> el rack lo eligio el usuario, asi que el probe no decide nada: un
    /// rack todavia sin vincular da <c>NEGATIVE</c> en todas sus vistas, y <c>Link</c> es precisamente la
    /// operacion que se le aplica. Sin esa asimetria, <c>Link</c> no podria vincular nada.
    /// </para>
    /// </summary>
    public class ProjectVariableDiscoveryTests
    {
        private const string RackA = "3f2b1c9e-6d4a-4f38-9b71-0c2a5e8d1f44";
        private const string RackB = "5d9e2a10-77b4-4c31-8e06-2f9a4b7c1d38";
        private const string Target = "8a1d4e77-2c93-4b60-8f15-6e0b93a7c221";
        private const string Otra = "1c7f0b52-4a88-4d0e-9a33-77b2e6c40915";
        private const string Token = ProjectPropertyIds.SelectiveVerticalClearanceToken;

        private static VariableId Objetivo => VariableId.Parse(Target);

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

        private static SelectivePalletDesignDocument Doc(
            double clearance = 6.0,
            string rackId = RackA,
            SelectivePropertyValueDocument binding = null,
            string bindingKey = Token,
            string schemaVersion = null,
            string extensionKey = null)
        {
            var doc = SelectivePalletDesignDocument.From(Diseno(clearance), rackId, "Rack");

            if (binding != null || bindingKey != Token)
            {
                doc.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument> { [bindingKey] = binding };
            }

            if (schemaVersion != null)
            {
                doc.SchemaVersion = schemaVersion;
            }

            if (extensionKey != null)
            {
                doc.ExtensionData = new Dictionary<string, JsonElement>
                {
                    [extensionKey] = JsonDocument.Parse("1").RootElement,
                };
            }

            return doc;
        }

        private static SelectivePropertyValueDocument A(string variableId = Target)
            => SelectivePropertyValueDocument.ToProjectVariable(variableId);

        private static ProjectVariableScanEntry Vista(SelectivePalletDesignDocument doc, string def = "DEF-1", string rackId = RackA)
            => ProjectVariableScanEntry.Selective(def, rackId, doc);

        // ================================================================ probe tri-estado (6, 25, 26)

        [Fact]
        public void Probe_SinPropertyValues_ES_NEGATIVE()
            => Assert.Equal(ConsumerProbeOutcome.Negative, ProjectVariableConsumerProbe.Probe(Doc(), Objetivo));

        [Fact]
        public void Probe_ConReferenciaAlObjetivo_ES_POSITIVE()
            => Assert.Equal(ConsumerProbeOutcome.Positive, ProjectVariableConsumerProbe.Probe(Doc(binding: A()), Objetivo));

        [Fact]
        public void Probe_ConReferenciaAOTRA_VariableComprendida_ES_NEGATIVE()
            => Assert.Equal(ConsumerProbeOutcome.Negative, ProjectVariableConsumerProbe.Probe(Doc(binding: A(Otra)), Objetivo));

        [Fact]
        public void Probe_ElVariableIdSeComparaOrdinalIgnoreCase()
        {
            var mayusculas = A(Target.ToUpperInvariant());

            Assert.Equal(ConsumerProbeOutcome.Positive, ProjectVariableConsumerProbe.Probe(Doc(binding: mayusculas), Objetivo));
        }

        [Fact]
        public void Prueba25_PropertyIdDESCONOCIDO_ES_INDETERMINATE_JAMAS_NEGATIVE()
        {
            var doc = Doc(binding: A(Otra), bindingKey: "selective.palletDepth");

            Assert.Equal(ConsumerProbeOutcome.Indeterminate, ProjectVariableConsumerProbe.Probe(doc, Objetivo));
        }

        [Fact]
        public void Prueba26_KindDESCONOCIDO_ES_INDETERMINATE_JAMAS_NEGATIVE()
        {
            var doc = Doc(binding: new SelectivePropertyValueDocument { Kind = "rackProperty", VariableId = Otra });

            Assert.Equal(ConsumerProbeOutcome.Indeterminate, ProjectVariableConsumerProbe.Probe(doc, Objetivo));
        }

        [Fact]
        public void Probe_VariableIdMALFORMADO_ES_INDETERMINATE()
            => Assert.Equal(ConsumerProbeOutcome.Indeterminate, ProjectVariableConsumerProbe.Probe(Doc(binding: A("no-guid")), Objetivo));

        [Fact]
        public void Probe_EntradaNULA_ES_INDETERMINATE()
        {
            var doc = Doc();
            doc.PropertyValues = new Dictionary<string, SelectivePropertyValueDocument> { [Token] = null };

            Assert.Equal(ConsumerProbeOutcome.Indeterminate, ProjectVariableConsumerProbe.Probe(doc, Objetivo));
        }

        [Fact]
        public void Probe_PayloadILEGIBLE_ES_INDETERMINATE()
            => Assert.Equal(ConsumerProbeOutcome.Indeterminate, ProjectVariableConsumerProbe.Probe((SelectivePalletDesignDocument)null, Objetivo));

        /// <summary>
        /// La regla que hace que <c>NEGATIVE</c> signifique algo: el mapa se recorre ENTERO. Una entrada
        /// ininterpretable convierte la respuesta en <c>INDETERMINATE</c> aunque otra entrada si referencie el
        /// objetivo — no se puede afirmar nada sobre un mapa que no se comprende del todo.
        /// </summary>
        [Fact]
        public void Probe_LO_DESCONOCIDO_DOMINA_AUNQUE_HAYA_UN_POSITIVO()
        {
            var doc = Doc(binding: A());
            doc.PropertyValues["selective.futuro"] = A(Otra);

            Assert.Equal(ConsumerProbeOutcome.Indeterminate, ProjectVariableConsumerProbe.Probe(doc, Objetivo));
        }

        [Fact]
        public void Prueba21_SobreUnaEntradaDeBarrido_UnDisenoIlegibleEs_INDETERMINATE()
        {
            var entrada = ProjectVariableScanEntry.SelectiveUnreadableDesign("DEF-1", RackA);

            Assert.Equal(ConsumerProbeOutcome.Indeterminate, ProjectVariableConsumerProbe.Probe(entrada, Objetivo));
        }

        // ================================================================ comparador (15, 16, 28)

        [Fact]
        public void Prueba15_HermanasIGUALES_SonUnaSolaAutoridad()
        {
            Assert.True(SelectiveAuthoredAuthority.IsSameAuthority(new[] { Doc(), Doc() }));
        }

        [Fact]
        public void Prueba16_DivergenciaEnElLITERAL_AUTHORED_ES_DIVERGENCIA()
        {
            Assert.False(SelectiveAuthoredAuthority.IsSameAuthority(new[] { Doc(6.0), Doc(7.0) }));
        }

        [Fact]
        public void Prueba16_DivergenciaEn_PROPERTYVALUES_ES_DIVERGENCIA()
        {
            Assert.False(SelectiveAuthoredAuthority.IsSameAuthority(new[] { Doc(), Doc(binding: A()) }));
            Assert.False(SelectiveAuthoredAuthority.IsSameAuthority(new[] { Doc(binding: A()), Doc(binding: A(Otra)) }));
        }

        [Fact]
        public void Prueba16_DivergenciaEn_SCHEMAVERSION_ES_DIVERGENCIA()
        {
            // Una vista en 2.x junto a una en 1.x es el rastro de un Link interrumpido.
            Assert.False(SelectiveAuthoredAuthority.IsSameAuthority(new[] { Doc(schemaVersion: "1.0"), Doc(schemaVersion: "2.0") }));
        }

        [Fact]
        public void Prueba28_DivergenciaSOLO_EN_EXTENSIONDATA_ES_DIVERGENCIA()
        {
            // Severo a proposito: si una version posterior escribio autoridad en una vista y no en las otras,
            // este build NO puede saber cual manda.
            Assert.False(SelectiveAuthoredAuthority.IsSameAuthority(new[] { Doc(), Doc(extensionKey: "Futuro") }));
            Assert.False(SelectiveAuthoredAuthority.IsSameAuthority(new[] { Doc(extensionKey: "A"), Doc(extensionKey: "B") }));
        }

        /// <summary>
        /// Un objeto en memoria y otro reconstruido desde texto —con sus listas y objetos anidados como
        /// instancias distintas— son la MISMA autoridad. Con igualdad de referencias o de bytes esto no
        /// pasaria.
        /// </summary>
        [Fact]
        public void ElComparadorEsSEMANTICO_NoIgualdadDeReferenciasNiDeBytes()
        {
            var store = new SelectivePalletDesignStore();
            var enMemoria = Doc(binding: A());

            // Serializar ESTAMPA la version pegajosa sobre el propio documento, asi que a partir de aqui los
            // dos estan en la misma linea y lo unico que puede diferir es la forma, no el estado.
            var releido = store.Deserialize(store.Serialize(enMemoria));

            Assert.NotSame(enMemoria, releido);
            Assert.NotSame(enMemoria.Bays, releido.Bays);
            Assert.True(SelectiveAuthoredAuthority.IsSameAuthority(new[] { enMemoria, releido }));
        }

        /// <summary>
        /// El reverso, y no es un detalle: escribir un diseño VINCULADO lo promueve, asi que una hermana
        /// guardada y otra que no lo esta divergen de verdad. Es el rastro de un Link interrumpido, y el
        /// comparador tiene que verlo.
        /// </summary>
        [Fact]
        public void UnaHermanaGUARDADA_Y_OtraSIN_GUARDAR_DivergenTrasVincular()
        {
            var store = new SelectivePalletDesignStore();
            var sinGuardar = Doc(binding: A());
            var guardada = store.Deserialize(store.Serialize(Doc(binding: A())));

            Assert.Equal("1.0", sinGuardar.SchemaVersion);
            Assert.Equal("2.0", guardada.SchemaVersion);
            Assert.False(SelectiveAuthoredAuthority.IsSameAuthority(new[] { sinGuardar, guardada }));
        }

        [Fact]
        public void UnaSolaHermanaYaEsUnaAutoridad()
            => Assert.True(SelectiveAuthoredAuthority.IsSameAuthority(new[] { Doc() }));

        [Fact]
        public void LaAutoridadDeUnRackDevuelveElRepresentanteCuandoCOINCIDEN()
        {
            var r = SelectiveAuthoredAuthority.Resolve(RackA, new[] { Vista(Doc(), "D1"), Vista(Doc(), "D2") });

            Assert.Equal(AuthoredAuthorityOutcome.Single, r.Outcome);
            Assert.True(r.IsSingle);
            Assert.NotNull(r.Authored);
        }

        [Fact]
        public void LaAutoridadNOMBRA_EL_RACKID_CuandoDIVERGEN()
        {
            var r = SelectiveAuthoredAuthority.Resolve(RackA, new[] { Vista(Doc(6.0), "D1"), Vista(Doc(7.0), "D2") });

            Assert.Equal(AuthoredAuthorityOutcome.Divergent, r.Outcome);
            Assert.Null(r.Authored);
            Assert.Contains(RackA, r.Error);
        }

        [Fact]
        public void UnaHermanaILEGIBLE_NoSeFILTRA_ParaSeguirConLasLegibles()
        {
            var r = SelectiveAuthoredAuthority.Resolve(
                RackA,
                new[] { Vista(Doc(), "D1"), ProjectVariableScanEntry.SelectiveUnreadableDesign("D2", RackA) });

            Assert.Equal(AuthoredAuthorityOutcome.UnreadableSibling, r.Outcome);
            Assert.Null(r.Authored);
        }

        // ================================================================ Familia A (18, 19, 20 espejo)

        [Fact]
        public void FamiliaA_UnRackCON_LaVariable_ES_CONSUMIDOR()
        {
            var r = ProjectVariableConsumerDiscovery.DiscoverConsumers(
                new[] { Vista(Doc(binding: A()), "D1"), Vista(Doc(binding: A()), "D2") },
                Objetivo);

            Assert.True(r.IsSuccess);
            var consumidor = Assert.Single(r.Consumers);
            Assert.Equal(RackA, consumidor.RackId);
        }

        [Fact]
        public void FamiliaA_UnRackSIN_LaVariable_SE_IGNORA()
        {
            var r = ProjectVariableConsumerDiscovery.DiscoverConsumers(new[] { Vista(Doc(), "D1") }, Objetivo);

            Assert.True(r.IsSuccess);
            Assert.Empty(r.Consumers);
        }

        [Fact]
        public void Prueba18_DIVERGENTE_PERO_TODAS_NEGATIVE_SE_IGNORA_Y_LA_OPERACION_CONTINUA()
        {
            // La linea que cierra N1: la divergencia es real, pero es AJENA a esta variable.
            var ajenoDivergente = new[] { Vista(Doc(6.0), "D1"), Vista(Doc(7.0), "D2") };
            var consumidor = new[]
            {
                ProjectVariableScanEntry.Selective("D3", RackB, Doc(binding: A(), rackId: RackB)),
            };

            var r = ProjectVariableConsumerDiscovery.DiscoverConsumers(ajenoDivergente.Concat(consumidor).ToList(), Objetivo);

            Assert.True(r.IsSuccess);
            Assert.Equal(RackB, Assert.Single(r.Consumers).RackId);
        }

        [Fact]
        public void Prueba19_MEZCLA_POSITIVE_NEGATIVE_ENTRE_HERMANAS_ABORTA()
        {
            var r = ProjectVariableConsumerDiscovery.DiscoverConsumers(
                new[] { Vista(Doc(binding: A()), "D1"), Vista(Doc(), "D2") },
                Objetivo);

            Assert.False(r.IsSuccess);
            Assert.Contains(RackA, r.Error);
        }

        [Fact]
        public void Prueba21_UNA_HERMANA_INDETERMINATE_ABORTA_TODA_LA_OPERACION()
        {
            var r = ProjectVariableConsumerDiscovery.DiscoverConsumers(
                new[]
                {
                    Vista(Doc(binding: A()), "D1"),
                    ProjectVariableScanEntry.SelectiveUnreadableDesign("D2", RackA),
                },
                Objetivo);

            Assert.False(r.IsSuccess);
            Assert.Empty(r.Consumers);
        }

        [Fact]
        public void Prueba16_TODAS_POSITIVE_PERO_AUTHORED_DIVERGENTE_ABORTA()
        {
            var r = ProjectVariableConsumerDiscovery.DiscoverConsumers(
                new[] { Vista(Doc(6.0, binding: A()), "D1"), Vista(Doc(7.0, binding: A()), "D2") },
                Objetivo);

            Assert.False(r.IsSuccess);
            Assert.Contains(RackA, r.Error);
        }

        [Fact]
        public void FamiliaA_SoloParticipaElKindSELECTIVE()
        {
            var r = ProjectVariableConsumerDiscovery.DiscoverConsumers(
                new[]
                {
                    ProjectVariableScanEntry.Foreign("D9", "otro-rack", "pushback"),
                    Vista(Doc(binding: A()), "D1"),
                },
                Objetivo);

            Assert.True(r.IsSuccess);
            Assert.Equal(RackA, Assert.Single(r.Consumers).RackId);
        }

        // ================================================================ sobre de nivel dibujo (14)

        [Fact]
        public void Prueba29_UN_SOBRE_ININTERPRETABLE_ABORTA_ANTES_DE_NADA()
        {
            var r = ProjectVariableConsumerDiscovery.DiscoverConsumers(
                new[] { Vista(Doc(binding: A()), "D1"), ProjectVariableScanEntry.UnreadableEnvelope("DEF-X") },
                Objetivo);

            Assert.False(r.IsSuccess);
            Assert.Empty(r.Consumers);
        }

        [Fact]
        public void UnSobreININTERPRETABLE_NoEs_NEGATIVE_NI_RACK_AJENO()
        {
            var entrada = ProjectVariableScanEntry.UnreadableEnvelope("DEF-X");

            Assert.False(entrada.OuterEnvelopeInterpretable);
            Assert.Null(entrada.RackId);
            Assert.Null(entrada.Kind);
        }

        [Fact]
        public void ElDiagnosticoNO_ATRIBUIBLE_NombraLaDEFINICION_Y_DiceQueElRackIdNoSePuedeDeterminar()
        {
            var r = ProjectVariableConsumerDiscovery.DiscoverConsumers(
                new[] { ProjectVariableScanEntry.UnreadableEnvelope("DEF-X") },
                Objetivo);

            Assert.Contains("DEF-X", r.Error);
            Assert.DoesNotContain(RackA, r.Error);
        }

        [Fact]
        public void ElDiagnosticoATRIBUIBLE_SI_NombraElRackId()
        {
            var r = ProjectVariableConsumerDiscovery.DiscoverConsumers(
                new[]
                {
                    Vista(Doc(binding: A()), "D1"),
                    Vista(Doc(binding: A(), bindingKey: "selective.futuro"), "D2"),
                },
                Objetivo);

            Assert.False(r.IsSuccess);
            Assert.Contains(RackA, r.Error);
        }

        // ================================================================ Familia B (20)

        [Fact]
        public void Prueba20_TARGET_RACK_TODAS_NEGATIVE_SI_ENTRA_AL_PREFLIGHT()
        {
            // Sin esto, Link no podria vincular NADA: un rack no vinculado es NEGATIVE en todas sus vistas.
            var r = ProjectVariableConsumerDiscovery.ResolveTargetRack(
                new[] { Vista(Doc(), "D1"), Vista(Doc(), "D2") },
                RackA);

            Assert.True(r.IsSuccess);
            var rack = Assert.Single(r.Consumers);
            Assert.Equal(RackA, rack.RackId);
            Assert.Equal(2, rack.Siblings.Count);
        }

        [Fact]
        public void FamiliaB_EXIGE_UNA_SOLA_AUTORIDAD_AUTHORED()
        {
            var r = ProjectVariableConsumerDiscovery.ResolveTargetRack(
                new[] { Vista(Doc(6.0), "D1"), Vista(Doc(7.0), "D2") },
                RackA);

            Assert.False(r.IsSuccess);
            Assert.Contains(RackA, r.Error);
        }

        [Fact]
        public void FamiliaB_UnaHermanaILEGIBLE_ABORTA()
        {
            var r = ProjectVariableConsumerDiscovery.ResolveTargetRack(
                new[] { Vista(Doc(), "D1"), ProjectVariableScanEntry.SelectiveUnreadableDesign("D2", RackA) },
                RackA);

            Assert.False(r.IsSuccess);
        }

        [Fact]
        public void FamiliaB_UnSobreININTERPRETABLE_ABORTA_AunqueSeaDeOtroRack()
        {
            var r = ProjectVariableConsumerDiscovery.ResolveTargetRack(
                new[] { Vista(Doc(), "D1"), ProjectVariableScanEntry.UnreadableEnvelope("DEF-X") },
                RackA);

            Assert.False(r.IsSuccess);
        }

        [Fact]
        public void FamiliaB_UnRackINEXISTENTE_ABORTA()
        {
            var r = ProjectVariableConsumerDiscovery.ResolveTargetRack(new[] { Vista(Doc(), "D1") }, RackB);

            Assert.False(r.IsSuccess);
        }

        [Fact]
        public void FamiliaB_NO_USA_EL_PROBE_ParaDecidirSiInspecciona()
        {
            // Un rack vinculado a OTRA variable sigue siendo inspeccionable como target-rack.
            var r = ProjectVariableConsumerDiscovery.ResolveTargetRack(
                new[] { Vista(Doc(binding: A(Otra)), "D1") },
                RackA);

            Assert.True(r.IsSuccess);
        }
    }
}
