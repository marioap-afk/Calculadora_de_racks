using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using RackCad.Application.Expressions;

namespace RackCad.Application.ComputedParameters
{
    /// <summary>
    /// Un RackId atribuible dentro de <see cref="ProjectSummary"/> (I-63 D-20): identidad canonica, kind, nombre,
    /// pertenencia y las metricas <c>(Rack, *)</c> segun la tabla D-28 (la misma de <see cref="RackMetricRequest"/>).
    /// </summary>
    public sealed class RackSummary
    {
        public RackSummary(
            string rackId,
            string kindToken,
            string displayName,
            RackMembership membership,
            string representativeDefinitionId,
            RackMetricResults metrics,
            IReadOnlyList<MetricProvenance> provenance)
        {
            RackId = rackId;
            KindToken = kindToken;
            DisplayName = displayName;
            Membership = membership;
            RepresentativeDefinitionId = representativeDefinitionId;
            Metrics = metrics;
            Provenance = provenance;
        }

        /// <summary>La grafia canonica (D-11.2).</summary>
        public string RackId { get; }

        /// <summary>El token de kind coherente. Nulo con Kind ausente o con kinds mezclados.</summary>
        public string KindToken { get; }

        /// <summary>El primer <c>Name</c> no vacio en orden canonico, con <c>Trim()</c> (D-11.6). Solo presentacion.</summary>
        public string DisplayName { get; }

        public RackMembership Membership { get; }

        /// <summary>La vista que aprobo E4 (la primera en orden canonico). Nula si E4 no llego a decidir.</summary>
        public string RepresentativeDefinitionId { get; }

        /// <summary>Un valor por CADA metrica <c>(Rack, *)</c>, siempre presentes.</summary>
        public RackMetricResults Metrics { get; }

        /// <summary>La provenance de cada metrica de <see cref="Metrics"/>, en el orden del catalogo. Fuera de la igualdad de <see cref="MetricValue"/>.</summary>
        public IReadOnlyList<MetricProvenance> Provenance { get; }

        /// <summary>La provenance de una metrica de rack, o nula.</summary>
        public MetricProvenance ProvenanceOf(MetricId metric)
            => Provenance == null ? null : Provenance.FirstOrDefault(item => item.MetricId == metric);
    }

    /// <summary>Los totales de proyecto de <see cref="ProjectSummary"/>: <c>(Project, totalRacks)</c>.</summary>
    public sealed class ProjectSummaryTotals
    {
        public ProjectSummaryTotals(MetricValue totalRacks)
        {
            TotalRacks = totalRacks;
        }

        /// <summary><c>(Project, totalRacks)</c>.</summary>
        public MetricValue TotalRacks { get; }
    }

    /// <summary>
    /// El resumen de proyecto <c>Full</c> (I-63 D-20): <c>Totals</c>, <c>BySystem</c> con los seis sistemas, <c>Racks</c>
    /// con TODOS los RackIds atribuibles y <c>Diagnostics</c>, mas la provenance en memoria (D-21). Puro, inmutable, en
    /// memoria y no persistido; sin UI ni textos localizados. El nivel <c>Population</c> es otro tipo
    /// (<see cref="ProjectPopulation"/>) y nunca ejecuta Phi2 ni Phi3 de metricas.
    /// </summary>
    public sealed class ProjectSummary : IEquatable<ProjectSummary>
    {
        private string _description;

        public ProjectSummary(
            ProjectSummaryTotals totals,
            IReadOnlyList<SystemAggregate> bySystem,
            IReadOnlyList<RackSummary> racks,
            IReadOnlyList<PopulationDiagnostic> diagnostics,
            ProjectSummaryProvenance provenance)
        {
            Totals = totals;
            BySystem = bySystem;
            Racks = racks;
            Diagnostics = diagnostics;
            Provenance = provenance;
        }

        public ProjectSummaryTotals Totals { get; }

        /// <summary>Los seis sistemas, siempre presentes y en el orden fijo de <see cref="ProjectPopulation.SystemOrder"/>.</summary>
        public IReadOnlyList<SystemAggregate> BySystem { get; }

        /// <summary>Todos los RackIds atribuibles (incluidos, excluidos tambien <c>NotPlaced</c>, e indeterminados), por RackId canonico Ordinal.</summary>
        public IReadOnlyList<RackSummary> Racks { get; }

        /// <summary>Por (codigo, clave de definicion Ordinal, RackId Ordinal).</summary>
        public IReadOnlyList<PopulationDiagnostic> Diagnostics { get; }

        /// <summary>La provenance de los agregados (D-21), fuera de la igualdad de <see cref="MetricValue"/>.</summary>
        public ProjectSummaryProvenance Provenance { get; }

        /// <summary>
        /// La peticion <c>Full</c>: una captura, una lectura del registro y un catalogo; por RackId una proyeccion y una
        /// autoridad; una Phi2+Phi3 solo por Selectivo y un veredicto E6 por Push Back; nunca por vista (D-24). Anota UNA
        /// evaluacion en <see cref="RackPopulationEvaluationCounter"/> (INV-32). Las metricas por rack usan la MISMA
        /// tabla D-28 que <see cref="RackMetricRequest"/>, reutilizando lo que la pertenencia ya decidio, sin cachear
        /// ni repetir ninguna lectura o resolucion.
        /// </summary>
        public static ProjectSummary Evaluate(
            RackMetricPopulationInput input,
            IRackMetricDesignReader designReader = null,
            IRackMetricResolutionSide resolutionSide = null,
            RackMetricProviderRegistry providers = null)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            var reader = designReader ?? new RackMetricDesignReader();
            var side = resolutionSide ?? new RackMetricResolutionSide();
            var registry = providers ?? RackMetricProviderRegistry.Default;

            // La unica evaluacion de poblacion de la peticion: pertenencia, E1..E6 y las etapas que decidio.
            var assessed = ProjectPopulation.EvaluateAssessed(input, reader);
            var population = assessed.Population;

            var racks = new List<RackSummary>(population.Racks.Count);
            var metricsByRack = new Dictionary<string, RackMetricResults>(StringComparer.Ordinal);
            for (var index = 0; index < population.Racks.Count; index++)
            {
                var member = population.Racks[index];
                var assessment = assessed.Assessments[index];

                var evaluation = RackMetricOrchestrator.Compute(
                    member.RackId, assessment.Members, input.Registry, input.Catalog, reader, side, registry, assessment.Stages);

                metricsByRack.Add(member.RackId, evaluation.Results);
                racks.Add(new RackSummary(
                    member.RackId,
                    member.KindToken,
                    member.DisplayName,
                    member.Membership,
                    member.RepresentativeDefinitionId,
                    evaluation.Results,
                    RackProvenance(member.RackId, evaluation)));
            }

            // D-12: la agregacion de G2 (funcion pura), alimentada con las metricas por rack del resumen.
            var aggregates = ProjectPopulationAggregator.Aggregate(population, metricsByRack);

            return new ProjectSummary(
                new ProjectSummaryTotals(aggregates.TotalRacks),
                aggregates.BySystem,
                racks,
                population.Diagnostics,
                new ProjectSummaryProvenance(AggregateProvenances(population, aggregates)));
        }

        private static IReadOnlyList<MetricProvenance> RackProvenance(string rackId, RackMetricEvaluation evaluation)
        {
            var provenance = new List<MetricProvenance>();
            foreach (var metric in RackMetricIds.RackMetrics)
            {
                string authorityId = null;
                RackMetricPhase? phase = null;

                var declaration = evaluation.Provider?.Declare(metric);
                if (declaration != null && declaration.Support == RackMetricSupport.Supported)
                {
                    authorityId = MetricAuthorityIds.ForRackMetric(evaluation.KindToken, metric);
                    phase = declaration.MinimumPhase;
                }

                provenance.Add(new MetricProvenance(
                    metric,
                    new SymbolId(SymbolNamespace.Rack, metric.Token),
                    rackId,
                    authorityId,
                    phase,
                    evaluation.Trace.RepresentativeDefinitionKey,
                    evaluation.Trace.AuthorityOutcome,
                    evaluation.Trace.OutputVerdict,
                    evaluation.Trace.ResolutionOutcome,
                    evaluation.Trace.EffectiveOutcome));
            }

            return provenance;
        }

        /// <summary><c>totalRacks</c> y, por sistema, sus tres metricas: autoridad, RackIds incluidos y excluidos con su motivo (D-21).</summary>
        private static IReadOnlyList<AggregateProvenance> AggregateProvenances(
            ProjectPopulation population, ProjectPopulationAggregates aggregates)
        {
            AggregateProvenance Of(MetricId metric, string kindToken, string authorityId)
            {
                var scoped = population.Racks
                    .Where(rack => kindToken == null || string.Equals(rack.KindToken, kindToken, StringComparison.Ordinal))
                    .ToList();

                return new AggregateProvenance(
                    metric,
                    kindToken,
                    authorityId,
                    scoped.Where(rack => rack.Membership.Kind == RackMembershipKind.Included)
                        .Select(rack => rack.RackId)
                        .ToList(),
                    scoped.Where(rack => rack.Membership.Kind == RackMembershipKind.Excluded)
                        .Select(rack => new ExcludedRackProvenance(rack.RackId, rack.Membership.ExclusionReason.Value))
                        .ToList());
            }

            string SumAuthority(MetricValue value)
                => value.Status == MetricStatus.NotSupported || value.Status == MetricStatus.NotApplicable
                    ? null
                    : MetricAuthorityIds.AggregateSum;

            var result = new List<AggregateProvenance>
            {
                Of(RackMetricIds.TotalRacks, null, MetricAuthorityIds.PopulationCotizable),
            };

            foreach (var system in aggregates.BySystem)
            {
                result.Add(Of(RackMetricIds.RackCount, system.KindToken, MetricAuthorityIds.PopulationCotizable));
                result.Add(Of(RackMetricIds.TotalFrentes, system.KindToken, SumAuthority(system.TotalFrentes)));
                result.Add(Of(RackMetricIds.TotalFrentesVacios, system.KindToken, SumAuthority(system.TotalFrentesVacios)));
            }

            return result;
        }

        /// <summary>El resumen de un rack por su RackId canonico (Ordinal), o nulo.</summary>
        public RackSummary Rack(string rackId)
            => Racks.FirstOrDefault(rack => string.Equals(rack.RackId, rackId, StringComparison.Ordinal));

        /// <summary>El texto canonico y determinista de todo el resumen (incluida la provenance), con cultura invariante.</summary>
        public string Describe()
        {
            if (_description == null)
            {
                _description = BuildDescription();
            }

            return _description;
        }

        private string BuildDescription()
        {
            var text = new StringBuilder();
            text.Append("totalRacks=").Append(Text(Totals.TotalRacks)).Append('\n');

            foreach (var system in BySystem)
            {
                text.Append("system ").Append(system.KindToken)
                    .Append(" rackCount=").Append(Text(system.RackCount))
                    .Append(" totalFrentes=").Append(Text(system.TotalFrentes))
                    .Append(" totalFrentesVacios=").Append(Text(system.TotalFrentesVacios))
                    .Append('\n');
            }

            foreach (var rack in Racks)
            {
                text.Append("rack ").Append(rack.RackId)
                    .Append(" kind=").Append(rack.KindToken ?? "-")
                    .Append(" name=").Append(rack.DisplayName ?? "-")
                    .Append(" membership=").Append(rack.Membership)
                    .Append(" representative=").Append(rack.RepresentativeDefinitionId ?? "-");
                foreach (var metric in rack.Metrics.Metrics)
                {
                    text.Append(' ').Append(metric.Token).Append('=').Append(Text(rack.Metrics[metric]));
                }

                text.Append('\n');

                foreach (var item in rack.Provenance ?? new List<MetricProvenance>())
                {
                    text.Append("  provenance ").Append(item.MetricId)
                        .Append(" symbol=").Append(item.SymbolId == null ? "-" : item.SymbolId.Namespace + "/" + item.SymbolId.Key)
                        .Append(" rack=").Append(item.RackId)
                        .Append(" authority=").Append(item.AuthorityId ?? "-")
                        .Append(" phase=").Append(item.Phase?.ToString() ?? "-")
                        .Append(" representative=").Append(item.RepresentativeDefinitionKey ?? "-")
                        .Append(" e4=").Append(item.AuthorityOutcome?.ToString() ?? "-")
                        .Append(" e6=").Append(item.OutputVerdict?.ToString() ?? "-")
                        .Append(" resolution=").Append(item.ResolutionOutcome?.ToString() ?? "-")
                        .Append(" effective=").Append(item.EffectiveOutcome?.ToString() ?? "-")
                        .Append('\n');
                }
            }

            foreach (var diagnostic in Diagnostics)
            {
                text.Append("diagnostic ").Append(diagnostic).Append('\n');
            }

            foreach (var aggregate in Provenance?.Aggregates ?? new List<AggregateProvenance>())
            {
                text.Append("aggregate ").Append(aggregate.MetricId)
                    .Append(" kind=").Append(aggregate.KindToken ?? "-")
                    .Append(" authority=").Append(aggregate.AuthorityId ?? "-")
                    .Append(" included=[").Append(string.Join(",", aggregate.IncludedRackIds)).Append(']')
                    .Append(" excluded=[").Append(string.Join(",", aggregate.Excluded.Select(item => item.RackId + ":" + item.Reason)))
                    .Append("]\n");
            }

            return text.ToString();
        }

        private static string Text(MetricValue value)
            => value == null ? "-"
                : value.Status == MetricStatus.Available
                    ? "Available(" + value.Value.ToString("R", CultureInfo.InvariantCulture) + ")"
                    : value.ToString();

        public bool Equals(ProjectSummary other)
            => other != null && string.Equals(Describe(), other.Describe(), StringComparison.Ordinal);

        public override bool Equals(object obj) => Equals(obj as ProjectSummary);

        public override int GetHashCode() => StringComparer.Ordinal.GetHashCode(Describe());
    }
}
