using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.ComputedParameters;
using RackCad.Application.Expressions;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Selective;
using Xunit;
using Xunit.Abstractions;
using static RackCad.Tests.ComputedParametersSummaryKit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-63 G4 - <c>ProjectSummary</c> <c>Full</c> (D-20), su provenance en memoria (D-21), la precedencia D-28 por
    /// <c>RackSummary.Metrics</c> (INV-34, resumen), el determinismo (INV-11 y INV-29 a) y el contador unico de
    /// evaluaciones de poblacion (INV-32). Todo sobre un contrato de entrada sintetico (D-25).
    /// </summary>
    public class ComputedParametersSummaryTests
    {
        private readonly ITestOutputHelper _output;

        public ComputedParametersSummaryTests(ITestOutputHelper output)
        {
            _output = output;
        }

        // ---------------------------------------------------------------- ayudas

        private static RackSummary RackOf(ProjectSummary summary, string rackId)
        {
            var rack = summary.Rack(rackId);
            Assert.True(rack != null, "El rack " + rackId + " no esta en el resumen (Racks: "
                + string.Join(", ", summary.Racks.Select(item => item.RackId)) + ").");
            return rack;
        }

        private static void AssertValue(MetricValue expected, MetricValue actual)
            => Assert.True(
                expected.Equals(actual),
                "Esperado " + Describe(expected) + " pero fue " + Describe(actual) + ".");

        private static void AssertBoth(RackSummary rack, MetricValue frentes, MetricValue frentesVacios)
        {
            AssertValue(frentes, rack.Metrics[RackMetricIds.Frentes]);
            AssertValue(frentesVacios, rack.Metrics[RackMetricIds.FrentesVacios]);
        }

        private static MetricValue Unavailable(UnavailableReasonKind kind) => MetricValue.Unavailable(UnavailableReason.Of(kind));

        private static IReadOnlyDictionary<string, RackMetricResults> MetricsOf(ProjectSummary summary)
            => summary.Racks.ToDictionary(rack => rack.RackId, rack => rack.Metrics, StringComparer.Ordinal);

        // ---------------------------------------------------------------- D-20

        [Fact]
        public void D20_Full_TieneTotalsRacksYDiagnostics_ConTodosLosRackIdsAtribuiblesEnOrdenCanonico()
        {
            var observation = Full(AccreditedMix());
            var summary = observation.Summary;

            // Racks: TODOS los RackIds atribuibles (el NotPlaced y los excluidos tambien), por RackId canonico Ordinal.
            Assert.Equal(new[] { R1, R2, R3, R4, R5, R6, R7 }, summary.Racks.Select(rack => rack.RackId));

            var one = RackOf(summary, R1);
            Assert.Equal(RackEmbedDocument.KindSelective, one.KindToken);
            Assert.Equal("Rack Uno", one.DisplayName);
            Assert.Equal(RackMembership.Included, one.Membership);
            Assert.Equal("K1-A", one.RepresentativeDefinitionId);

            Assert.Equal(RackMembership.Excluded(RackExclusionReason.NotPlaced), RackOf(summary, R2).Membership);
            Assert.Equal(RackMembership.Included, RackOf(summary, R3).Membership);
            Assert.Equal(RackEmbedDocument.KindPushBack, RackOf(summary, R3).KindToken);
            Assert.Equal(RackMembership.Included, RackOf(summary, R4).Membership);
            Assert.Equal(RackMembership.Included, RackOf(summary, R5).Membership);
            Assert.Equal(RackMembership.Excluded(RackExclusionReason.KindAbsent), RackOf(summary, R6).Membership);
            Assert.Null(RackOf(summary, R6).KindToken);
            Assert.Equal(RackMembership.Excluded(RackExclusionReason.SiblingsDivergent), RackOf(summary, R7).Membership);

            // Metrics (Rack, *): SIEMPRE presentes y segun D-28, tambien en los excluidos.
            AssertBoth(one, MetricValue.Available(4), MetricValue.Available(1));
            AssertBoth(RackOf(summary, R2), MetricValue.Available(2), MetricValue.Available(0));
            AssertBoth(RackOf(summary, R3), MetricValue.NotSupported(), MetricValue.NotSupported());
            AssertBoth(RackOf(summary, R4), MetricValue.NotApplicable(), MetricValue.NotApplicable());
            AssertBoth(RackOf(summary, R5), MetricValue.NotApplicable(), MetricValue.NotApplicable());
            AssertBoth(
                RackOf(summary, R6),
                Unavailable(UnavailableReasonKind.KindAbsent),
                Unavailable(UnavailableReasonKind.KindAbsent));
            AssertBoth(
                RackOf(summary, R7),
                Unavailable(UnavailableReasonKind.SiblingsDivergent),
                Unavailable(UnavailableReasonKind.SiblingsDivergent));
            Assert.All(summary.Racks, rack => Assert.Equal(RackMetricIds.RackMetrics, rack.Metrics.Metrics));

            // Totals (Project, totalRacks): R1, R3, R4 y R5.
            AssertValue(MetricValue.Available(4), summary.Totals.TotalRacks);

            // Diagnostics: (codigo, DefinitionKey Ordinal); NotPlaced no genera diagnostico.
            Assert.Equal(2, summary.Diagnostics.Count);
            Assert.Equal(
                new[] { (PopulationDiagnosticCode.RackExcluded, R6, "K6-A", "KindAbsent"), (PopulationDiagnosticCode.RackExcluded, R7, "K7-A", "SiblingsDivergent") },
                summary.Diagnostics.Select(item => (item.Code, item.RackId, item.DefinitionKey, item.Reason)));
        }

        [Fact]
        public void D20_BySystem_TieneLosSeisSistemasEnOrdenFijo_ConLaAgregacionDeG2AlimentadaPorLasMetricasPorRack()
        {
            var captures = AccreditedMix();
            var summary = Full(captures).Summary;

            Assert.Equal(ProjectPopulation.SystemOrder, summary.BySystem.Select(item => item.KindToken));
            Assert.Equal(6, summary.BySystem.Count);

            var selective = summary.BySystem.Single(item => item.KindToken == RackEmbedDocument.KindSelective);
            AssertValue(MetricValue.Available(1), selective.RackCount);
            AssertValue(MetricValue.Available(4), selective.TotalFrentes);
            AssertValue(MetricValue.Available(1), selective.TotalFrentesVacios);

            var pushBack = summary.BySystem.Single(item => item.KindToken == RackEmbedDocument.KindPushBack);
            AssertValue(MetricValue.Available(1), pushBack.RackCount);
            AssertValue(MetricValue.NotSupported(), pushBack.TotalFrentes);

            var cabecera = summary.BySystem.Single(item => item.KindToken == RackEmbedDocument.KindCabecera);
            AssertValue(MetricValue.NotApplicable(), cabecera.TotalFrentesVacios);

            var dynamic = summary.BySystem.Single(item => item.KindToken == RackEmbedDocument.KindDynamic);
            AssertValue(MetricValue.Available(0), dynamic.RackCount);

            // Es la agregacion de G2 (funcion pura), alimentada con las metricas por rack del resumen.
            var population = Population(captures).Population;
            var expected = ProjectPopulationAggregator.Aggregate(population, MetricsOf(summary));
            AssertValue(expected.TotalRacks, summary.Totals.TotalRacks);
            for (var index = 0; index < 6; index++)
            {
                AssertValue(expected.BySystem[index].RackCount, summary.BySystem[index].RackCount);
                AssertValue(expected.BySystem[index].TotalFrentes, summary.BySystem[index].TotalFrentes);
                AssertValue(expected.BySystem[index].TotalFrentesVacios, summary.BySystem[index].TotalFrentesVacios);
            }

            // Sigma rackCount = totalRacks cuando es Available.
            Assert.Equal(summary.Totals.TotalRacks.Value, summary.BySystem.Sum(item => item.RackCount.Value));
        }

        [Fact]
        public void D20_ConUnRackIndeterminado_LaCoberturaNoSeAcredita_YElRackSigueConSusMetricasD28()
        {
            var captures = AccreditedMix();
            captures.Add(Cap("K8-A", "rack-8", RackEmbedDocument.KindSelective, Garbage, refs: 1, name: "Ilegible"));

            var summary = Full(captures).Summary;

            var unreadable = RackOf(summary, "rack-8");
            Assert.Equal(RackMembership.Undetermined(RackUndeterminedReason.DesignUnreadable), unreadable.Membership);
            AssertBoth(
                unreadable,
                Unavailable(UnavailableReasonKind.DesignUnreadable),
                Unavailable(UnavailableReasonKind.DesignUnreadable));

            // Un agregado nunca es parcial: sin cobertura acreditada ninguno da un numero.
            AssertValue(Unavailable(UnavailableReasonKind.CoverageNotAccredited), summary.Totals.TotalRacks);
            Assert.All(
                summary.BySystem,
                item => AssertValue(Unavailable(UnavailableReasonKind.CoverageNotAccredited), item.RackCount));
            Assert.Contains(
                summary.Diagnostics,
                item => item.Code == PopulationDiagnosticCode.RackUndetermined && item.RackId == "rack-8");

            // Los demas racks conservan sus metricas.
            AssertBoth(RackOf(summary, R1), MetricValue.Available(4), MetricValue.Available(1));
        }

        // ---------------------------------------------------------------- INV-34 (resumen)

        private void AssertSummaryMatchesRequest(
            IReadOnlyList<RackDefinitionCapture> captures, string rackId, FullObservation observation, string scenario)
        {
            var rack = RackOf(observation.Summary, rackId);
            var request = Request(SiblingsOf(captures, rackId));

            _output.WriteLine(
                scenario + " | Full: lecturas=" + observation.Reads + " resoluciones=" + observation.Resolutions
                + " | frentes=" + Describe(rack.Metrics[RackMetricIds.Frentes])
                + " frentesVacios=" + Describe(rack.Metrics[RackMetricIds.FrentesVacios]));

            // MISMO resultado por RackMetricRequest y por RackSummary.Metrics.
            foreach (var metric in RackMetricIds.RackMetrics)
            {
                AssertValue(request[metric], rack.Metrics[metric]);
            }
        }

        [Fact]
        public void INV34_SelectivoValido_EsAvailable_ConUnaSolaResolucionYUnaLecturaPorVista()
        {
            var captures = AccreditedMix().Where(item => item.DefinitionKey.StartsWith("K1-", StringComparison.Ordinal)).ToList();
            var observation = Full(captures);

            AssertSummaryMatchesRequest(captures, R1, observation, "Selectivo valido");
            AssertBoth(RackOf(observation.Summary, R1), MetricValue.Available(4), MetricValue.Available(1));
            Assert.Equal(1, observation.Resolutions);
            Assert.Equal(3, observation.Reads);
        }

        [Fact]
        public void INV34_SelectivoConHermanasDivergentes_EsUnavailableSiblingsDivergent_ConCeroResoluciones()
        {
            var captures = AccreditedMix().Where(item => item.DefinitionKey.StartsWith("K7-", StringComparison.Ordinal)).ToList();
            var observation = Full(captures);

            AssertSummaryMatchesRequest(captures, R7, observation, "Selectivo divergente");
            AssertBoth(
                RackOf(observation.Summary, R7),
                Unavailable(UnavailableReasonKind.SiblingsDivergent),
                Unavailable(UnavailableReasonKind.SiblingsDivergent));
            Assert.Equal(0, observation.Resolutions);
        }

        [Fact]
        public void INV34_SelectivoIlegible_EsUnavailableDesignUnreadable_ConCeroResoluciones()
        {
            var captures = new List<RackDefinitionCapture>
            {
                Cap("K-A", R1, RackEmbedDocument.KindSelective, Garbage, refs: 1),
            };
            var observation = Full(captures);

            AssertSummaryMatchesRequest(captures, R1, observation, "Selectivo ilegible");
            AssertBoth(
                RackOf(observation.Summary, R1),
                Unavailable(UnavailableReasonKind.DesignUnreadable),
                Unavailable(UnavailableReasonKind.DesignUnreadable));
            Assert.Equal(0, observation.Resolutions);
        }

        [Fact]
        public void INV34_PushBackIlegible_EsNotSupported_ConCeroLecturasYCeroResoluciones()
        {
            // Control: ese texto SI es ilegible para el lector D-26. Si el resumen lo leyera antes del paso 2 de D-28,
            // daria Unavailable(DesignUnreadable) y no NotSupported.
            Assert.False(new RackMetricDesignReader().IsReadable(RackEmbedDocument.KindPushBack, Garbage));

            // Sin colocar: la pertenencia termina en E2 sin leer nada; las metricas D-28 tampoco leen (paso 2).
            var captures = new List<RackDefinitionCapture>
            {
                Cap("K-A", R3, RackEmbedDocument.KindPushBack, Garbage, refs: 0),
            };
            var observation = Full(captures);

            AssertSummaryMatchesRequest(captures, R3, observation, "Push Back ilegible");
            AssertBoth(RackOf(observation.Summary, R3), MetricValue.NotSupported(), MetricValue.NotSupported());
            Assert.Equal(0, observation.Reads);
            Assert.Equal(0, observation.Resolutions);
        }

        [Fact]
        public void INV34_PushBackIlegibleColocado_ConservaNotSupported_YLasMetricasNoAgreganLecturas()
        {
            var captures = new List<RackDefinitionCapture>
            {
                Cap("K-A", R3, RackEmbedDocument.KindPushBack, Garbage, refs: 1),
            };
            var observation = Full(captures);

            AssertSummaryMatchesRequest(captures, R3, observation, "Push Back ilegible colocado");
            AssertBoth(RackOf(observation.Summary, R3), MetricValue.NotSupported(), MetricValue.NotSupported());
            Assert.Equal(
                RackMembership.Undetermined(RackUndeterminedReason.DesignUnreadable),
                RackOf(observation.Summary, R3).Membership);

            // La unica lectura es la de la pertenencia (E5): las metricas no leen de nuevo.
            Assert.Equal(1, observation.Reads);
            Assert.Equal(0, observation.Resolutions);
        }

        [Fact]
        public void INV34_Cabecera_EsNotApplicable_ConCeroLecturasYCeroResoluciones()
        {
            var captures = new List<RackDefinitionCapture>
            {
                Cap("K-A", R4, RackEmbedDocument.KindCabecera, Garbage, refs: 0),
            };
            var observation = Full(captures);

            AssertSummaryMatchesRequest(captures, R4, observation, "Cabecera");
            AssertBoth(RackOf(observation.Summary, R4), MetricValue.NotApplicable(), MetricValue.NotApplicable());
            Assert.Equal(0, observation.Reads);
            Assert.Equal(0, observation.Resolutions);
        }

        [Fact]
        public void INV34_VariableRota_EsUnavailableEffectiveFailed_ConUnaResolucion_YKindIncoherenteNoResuelve()
        {
            var broken = new List<RackDefinitionCapture>
            {
                Cap("K-A", R1, RackEmbedDocument.KindSelective, BrokenReferenceJson(), refs: 1),
            };
            var brokenObservation = Full(broken);

            AssertSummaryMatchesRequest(broken, R1, brokenObservation, "Variable rota");
            var failed = MetricValue.Unavailable(
                UnavailableReason.EffectiveFailed(SelectiveEffectiveOutcome.BrokenProjectVariableReference));
            AssertBoth(RackOf(brokenObservation.Summary, R1), failed, failed);
            Assert.Equal(1, brokenObservation.Resolutions);

            var incoherent = new List<RackDefinitionCapture>
            {
                Cap("K-A", R1, RackEmbedDocument.KindSelective, SelectiveJson(6.0, 2, 2), refs: 1),
                Cap("K-B", R1, RackEmbedDocument.KindPushBack, PushBackJson(), refs: 0),
            };
            var incoherentObservation = Full(incoherent);

            AssertSummaryMatchesRequest(incoherent, R1, incoherentObservation, "Kind incoherente");
            AssertBoth(
                RackOf(incoherentObservation.Summary, R1),
                Unavailable(UnavailableReasonKind.KindIncoherent),
                Unavailable(UnavailableReasonKind.KindIncoherent));
            Assert.Equal(0, incoherentObservation.Resolutions);
            Assert.Equal(0, incoherentObservation.Reads);
        }

        // ---------------------------------------------------------------- INV-11 e INV-29 (a)

        private static List<RackDefinitionCapture> SpellingsAndSiblings()
        {
            var json = SelectiveJson(6.0, 2, 2, 2, 2);
            var divergent = SelectiveJson(9.0, 2, 2, 2, 2);
            var captures = AccreditedMix();
            captures.AddRange(new[]
            {
                // Rack-A: la hermana divergente NO esta colocada; grafias distintas del mismo RackId.
                Cap("A-1", "rack-a", RackEmbedDocument.KindSelective, divergent, refs: 0, name: ""),
                Cap("A-2", "Rack-A", RackEmbedDocument.KindSelective, json, refs: 1, name: "  Beta  "),
                // Rack-C: tres vistas, nombres en blanco y tres grafias.
                Cap("C-1", "rack-c", RackEmbedDocument.KindSelective, json, refs: 1, name: " "),
                Cap("C-2", "Rack-C", RackEmbedDocument.KindSelective, json, refs: 0, name: "  Gamma "),
                Cap("C-3", "RACK-C", RackEmbedDocument.KindSelective, json, refs: 2, name: "Alfa"),
            });
            return captures;
        }

        private static IEnumerable<List<RackDefinitionCapture>> OtherOrders(List<RackDefinitionCapture> captures)
        {
            yield return captures.AsEnumerable().Reverse().ToList();
            yield return captures.Skip(3).Concat(captures.Take(3)).ToList();
            yield return captures.Skip(7).Concat(captures.Take(7)).ToList();
            yield return captures.Where((_, index) => index % 2 == 0).Concat(captures.Where((_, index) => index % 2 == 1)).ToList();
        }

        [Fact]
        public void INV11_Full_ConElSnapshotInvertido_EsIgualAlOriginal_ConRepresentanteNombreGrafiaMetricasYDiagnosticos()
        {
            var captures = SpellingsAndSiblings();
            var original = Full(captures).Summary;

            // El resumen no esta vacio: el oraculo no puede ser dos resumenes vacios iguales.
            Assert.Equal(9, original.Racks.Count);
            Assert.Equal(new[] { "RACK-C", "Rack-A" }, original.Racks.Select(rack => rack.RackId).Take(2));

            var rackA = RackOf(original, "Rack-A");
            Assert.Equal(RackMembership.Excluded(RackExclusionReason.SiblingsDivergent), rackA.Membership);
            Assert.Equal("Beta", rackA.DisplayName);

            var rackC = RackOf(original, "RACK-C");
            Assert.Equal(RackMembership.Included, rackC.Membership);
            Assert.Equal("C-1", rackC.RepresentativeDefinitionId);
            Assert.Equal("Gamma", rackC.DisplayName);
            Assert.Equal("C-1", rackC.ProvenanceOf(RackMetricIds.Frentes)?.RepresentativeDefinitionKey);

            foreach (var order in OtherOrders(captures))
            {
                var other = Full(order).Summary;

                Assert.Equal(original.Racks.Select(rack => rack.RackId), other.Racks.Select(rack => rack.RackId));
                Assert.Equal(original.Racks.Select(rack => rack.Membership), other.Racks.Select(rack => rack.Membership));
                Assert.Equal(
                    original.Racks.Select(rack => rack.RepresentativeDefinitionId),
                    other.Racks.Select(rack => rack.RepresentativeDefinitionId));
                Assert.Equal(original.Racks.Select(rack => rack.DisplayName), other.Racks.Select(rack => rack.DisplayName));
                Assert.Equal(
                    original.Diagnostics.Select(item => item.ToString()), other.Diagnostics.Select(item => item.ToString()));
                Assert.Equal(
                    original.Racks.SelectMany(rack => RackMetricIds.RackMetrics.Select(metric => Describe(rack.Metrics[metric]))),
                    other.Racks.SelectMany(rack => RackMetricIds.RackMetrics.Select(metric => Describe(rack.Metrics[metric]))));

                // Y el resumen completo, provenance incluida.
                Assert.NotEqual(string.Empty, original.Describe());
                Assert.Equal(original.Describe(), other.Describe());
                Assert.True(original.Equals(other), "Full(original) debe ser igual a Full(invertido).");
            }
        }

        [Fact]
        public void INV29_ElMismoSnapshotEnOtroOrden_DaElMismoProjectSummary_YPopulationNoTieneMetricasPorRack()
        {
            var captures = SpellingsAndSiblings();
            var first = Full(captures).Summary;
            Assert.Equal(9, first.Racks.Count);

            foreach (var order in OtherOrders(captures))
            {
                var other = Full(order).Summary;
                Assert.Equal(first.Describe(), other.Describe());
                Assert.True(first.Equals(other));
                Assert.Equal(first.GetHashCode(), other.GetHashCode());
            }

            // (a, segunda parte) UNA sola funcion: los campos de metricas por rack de un tipo. Control positivo con la
            // MISMA funcion: RackSummary SI los tiene; el objetivo (ProjectPopulation y su rack) no.
            Assert.NotEmpty(PerRackMetricMembers(typeof(RackSummary)));
            Assert.Empty(PerRackMetricMembers(typeof(PopulationRack)));
            Assert.Empty(PerRackMetricMembers(typeof(ProjectPopulation)));

            var population = Population(captures).Population;
            Assert.Equal(first.Racks.Select(rack => rack.RackId), population.Racks.Select(rack => rack.RackId));
        }

        /// <summary>Las propiedades publicas de un tipo cuyo tipo (o argumento de tipo) lleva metricas por rack.</summary>
        private static IReadOnlyList<string> PerRackMetricMembers(Type type)
        {
            bool Carries(Type member)
                => member == typeof(RackMetricResults)
                   || member == typeof(MetricProvenance)
                   || member == typeof(RackSummary)
                   || (member.IsGenericType && member.GetGenericArguments().Any(Carries));

            return type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Where(property => Carries(property.PropertyType))
                .Select(property => property.Name)
                .ToList();
        }

        // ---------------------------------------------------------------- INV-32

        [Fact]
        public void INV32_RackMetricRequestDirecto_NoEvaluaPoblacion_YProjectSummaryFullSobreElMismoRackSi()
        {
            var captures = AccreditedMix();
            var siblings = SiblingsOf(captures, R1);
            Assert.Equal(3, siblings.Count);

            using (var direct = RackPopulationEvaluationCounter.Begin())
            {
                var results = Request(siblings);
                Assert.Equal(MetricStatus.Available, results[RackMetricIds.Frentes].Status);
                Assert.Equal(0, direct.Count);
            }

            // Control G2 con el MISMO contador: ProjectPopulation anota > 0.
            using (var population = RackPopulationEvaluationCounter.Begin())
            {
                ProjectPopulation.Evaluate(Input(siblings));
                Assert.True(population.Count > 0);
            }

            // Control G4 con el MISMO contador: ProjectSummary Full sobre el mismo rack anota > 0 (una evaluacion).
            using (var full = RackPopulationEvaluationCounter.Begin())
            {
                var summary = ProjectSummary.Evaluate(Input(siblings));
                Assert.Equal(new[] { R1 }, summary.Racks.Select(rack => rack.RackId));
                Assert.True(full.Count > 0);
                Assert.Equal(1, full.Count);
            }
        }

        // ---------------------------------------------------------------- D-21

        [Fact]
        public void D21_CadaMetricaDelResumen_ConservaMetricIdSymbolIdRackIdAutoridadFaseRepresentanteYOutcomes()
        {
            var summary = Full(AccreditedMix()).Summary;

            var one = RackOf(summary, R1);
            Assert.Equal(RackMetricIds.RackMetrics.Count, one.Provenance.Count);

            var bays = one.ProvenanceOf(RackMetricIds.Frentes);
            Assert.NotNull(bays);
            Assert.Equal(RackMetricIds.Frentes, bays.MetricId);
            Assert.Equal(new SymbolId(SymbolNamespace.Rack, RackMetricIds.FrentesToken), bays.SymbolId);
            Assert.Equal(R1, bays.RackId);
            Assert.Equal("selective.resolved.fondo0.bays", bays.AuthorityId);
            Assert.Equal(RackMetricPhase.Resolved, bays.Phase);
            Assert.Equal("K1-A", bays.RepresentativeDefinitionKey);
            Assert.Equal(BomAuthorityOutcome.Success, bays.AuthorityOutcome);
            Assert.Equal(RackOutputVerdictKind.Allow, bays.OutputVerdict);
            Assert.Equal(RackMetricResolutionOutcome.Resolved, bays.ResolutionOutcome);
            Assert.Null(bays.EffectiveOutcome);

            var empty = one.ProvenanceOf(RackMetricIds.FrentesVacios);
            Assert.NotNull(empty);
            Assert.Equal(new SymbolId(SymbolNamespace.Rack, RackMetricIds.FrentesVaciosToken), empty.SymbolId);
            Assert.Equal("selective.resolved.fondo0.emptyBays", empty.AuthorityId);

            // Un Selectivo sin colocar: E4 y la resolucion las hacen las metricas; E6 no se evaluo.
            var notPlaced = RackOf(summary, R2).ProvenanceOf(RackMetricIds.Frentes);
            Assert.NotNull(notPlaced);
            Assert.Equal("K2-A", notPlaced.RepresentativeDefinitionKey);
            Assert.Equal(BomAuthorityOutcome.Success, notPlaced.AuthorityOutcome);
            Assert.Equal(RackMetricResolutionOutcome.Resolved, notPlaced.ResolutionOutcome);

            // Hermanas divergentes: el outcome de E4 y ningun representante ni resolucion.
            var divergent = RackOf(summary, R7).ProvenanceOf(RackMetricIds.Frentes);
            Assert.NotNull(divergent);
            Assert.Equal(BomAuthorityOutcome.DivergentSiblings, divergent.AuthorityOutcome);
            Assert.Null(divergent.RepresentativeDefinitionKey);
            Assert.Null(divergent.ResolutionOutcome);

            // Push Back y Cabecera: NotSupported / NotApplicable no salen de ninguna autoridad fuente ni fase; el veredicto E6 si consta.
            var pushBack = RackOf(summary, R3).ProvenanceOf(RackMetricIds.Frentes);
            Assert.NotNull(pushBack);
            Assert.Null(pushBack.AuthorityId);
            Assert.Null(pushBack.Phase);
            Assert.Equal("K3-A", pushBack.RepresentativeDefinitionKey);
            Assert.Equal(RackOutputVerdictKind.Allow, pushBack.OutputVerdict);
            Assert.Null(pushBack.ResolutionOutcome);
        }

        [Fact]
        public void D21_ElFalloDelEfectivo_ConstaEnLaProvenanceDeLaMetrica()
        {
            var captures = new List<RackDefinitionCapture>
            {
                Cap("K-A", R1, RackEmbedDocument.KindSelective, BrokenReferenceJson(), refs: 1),
            };

            var provenance = RackOf(Full(captures).Summary, R1).ProvenanceOf(RackMetricIds.Frentes);

            Assert.NotNull(provenance);
            Assert.Equal(RackMetricResolutionOutcome.EffectiveFailed, provenance.ResolutionOutcome);
            Assert.Equal(SelectiveEffectiveOutcome.BrokenProjectVariableReference, provenance.EffectiveOutcome);
            Assert.Equal("K-A", provenance.RepresentativeDefinitionKey);
            Assert.Equal(BomAuthorityOutcome.Success, provenance.AuthorityOutcome);
        }

        [Fact]
        public void D21_CadaAgregado_ConservaLosRackIdsIncluidosYLosExcluidosConSuMotivo()
        {
            var summary = Full(AccreditedMix()).Summary;
            Assert.NotNull(summary.Provenance);

            var total = summary.Provenance.Of(RackMetricIds.TotalRacks);
            Assert.NotNull(total);
            Assert.Equal(MetricAuthorityIds.PopulationCotizable, total.AuthorityId);
            Assert.Equal(new[] { R1, R3, R4, R5 }, total.IncludedRackIds);
            Assert.Equal(
                new[]
                {
                    (R2, RackExclusionReason.NotPlaced),
                    (R6, RackExclusionReason.KindAbsent),
                    (R7, RackExclusionReason.SiblingsDivergent),
                },
                total.Excluded.Select(item => (item.RackId, item.Reason)));

            var sum = summary.Provenance.Of(RackMetricIds.TotalFrentes, RackEmbedDocument.KindSelective);
            Assert.NotNull(sum);
            Assert.Equal(MetricAuthorityIds.AggregateSum, sum.AuthorityId);
            Assert.Equal(new[] { R1 }, sum.IncludedRackIds);
            Assert.Equal(
                new[] { (R2, RackExclusionReason.NotPlaced), (R7, RackExclusionReason.SiblingsDivergent) },
                sum.Excluded.Select(item => (item.RackId, item.Reason)));

            var empties = summary.Provenance.Of(RackMetricIds.TotalFrentesVacios, RackEmbedDocument.KindSelective);
            Assert.NotNull(empties);
            Assert.Equal(MetricAuthorityIds.AggregateSum, empties.AuthorityId);

            var count = summary.Provenance.Of(RackMetricIds.RackCount, RackEmbedDocument.KindPushBack);
            Assert.NotNull(count);
            Assert.Equal(MetricAuthorityIds.PopulationCotizable, count.AuthorityId);
            Assert.Equal(new[] { R3 }, count.IncludedRackIds);

            // NotSupported no sale de ninguna suma.
            var pushBackSum = summary.Provenance.Of(RackMetricIds.TotalFrentes, RackEmbedDocument.KindPushBack);
            Assert.NotNull(pushBackSum);
            Assert.Null(pushBackSum.AuthorityId);
        }

        [Fact]
        public void D21_LaProvenanceNoEntraEnLaIgualdadDeMetricValue_NiLaCambia()
        {
            // MetricValue conserva exactamente su forma de G1: estado, valor y razon. La provenance es un metadato aparte.
            Assert.Equal(
                new[] { "Reason", "Status", "Value" },
                typeof(MetricValue).GetProperties().Select(property => property.Name).OrderBy(name => name, StringComparer.Ordinal));

            var summary = Full(AccreditedMix()).Summary;
            var rack = RackOf(summary, R1);

            // El valor del resumen CON provenance es igual (y de igual hash) al MetricValue sin ella.
            var value = rack.Metrics[RackMetricIds.Frentes];
            Assert.NotNull(rack.ProvenanceOf(RackMetricIds.Frentes));
            Assert.True(MetricValue.Available(4).Equals(value));
            Assert.Equal(MetricValue.Available(4).GetHashCode(), value.GetHashCode());

            // Y es el mismo resultado que da la peticion por rack, que no lleva provenance.
            AssertValue(Request(SiblingsOf(AccreditedMix(), R1))[RackMetricIds.Frentes], value);
            Assert.Equal(typeof(MetricValue), value.GetType());
        }

        [Fact]
        public void INV31_LaTablaCerradaDeAutoridades_CoincideConLaDeLaPrueba_EnLosDosSentidos()
        {
            var expected = new[]
            {
                "selective.resolved.fondo0.bays",
                "selective.resolved.fondo0.emptyBays",
                "population.cotizable",
                "aggregate.sum",
            };

            // Sentido 1: la tabla de produccion es exactamente la de la prueba.
            Assert.Equal(expected.OrderBy(id => id, StringComparer.Ordinal), MetricAuthorityIds.All.OrderBy(id => id, StringComparer.Ordinal));

            // Sentido 2: todo identificador que el resumen emite esta en la tabla, y la tabla entera se emite.
            var summary = Full(AccreditedMix()).Summary;
            var emitted = summary.Racks
                .SelectMany(rack => rack.Provenance)
                .Select(item => item.AuthorityId)
                .Concat(summary.Provenance.Aggregates.Select(item => item.AuthorityId))
                .Where(id => id != null)
                .Distinct(StringComparer.Ordinal)
                .ToList();

            Assert.NotEmpty(emitted);
            Assert.All(emitted, id => Assert.Contains(id, expected));
            Assert.All(expected, id => Assert.Contains(id, emitted));
        }
    }
}
