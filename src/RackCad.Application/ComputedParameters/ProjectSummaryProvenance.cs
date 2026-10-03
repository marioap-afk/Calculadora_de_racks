using System;
using System.Collections.Generic;
using RackCad.Application.Bom;
using RackCad.Application.Expressions;
using RackCad.Application.Systems.Selective;

namespace RackCad.Application.ComputedParameters
{
    /// <summary>
    /// La tabla CERRADA de identificadores de autoridad fuente (I-63 D-21, INV-31). Un identificador nombra de donde sale
    /// el valor de una metrica; no es un nombre visible ni un token de catalogo. Ampliarla exige una enmienda.
    /// </summary>
    public static class MetricAuthorityIds
    {
        public const string SelectiveFondo0Bays = "selective.resolved.fondo0.bays";
        public const string SelectiveFondo0EmptyBays = "selective.resolved.fondo0.emptyBays";
        public const string PopulationCotizable = "population.cotizable";
        public const string AggregateSum = "aggregate.sum";

        /// <summary>Los cuatro identificadores, en el orden de D-21.</summary>
        public static IReadOnlyList<string> All { get; } = new[]
        {
            SelectiveFondo0Bays, SelectiveFondo0EmptyBays, PopulationCotizable, AggregateSum,
        };
    }

    /// <summary>
    /// La provenance en memoria de UN <see cref="MetricValue"/> de rack (D-21). Es un metadato asociado: NO participa de
    /// <see cref="MetricValue.Equals(MetricValue)"/> ni se persiste (ADR-0043 D24).
    /// </summary>
    public sealed class MetricProvenance
    {
        public MetricProvenance(
            MetricId metricId,
            SymbolId symbolId,
            string rackId,
            string authorityId,
            RackMetricPhase? phase,
            string representativeDefinitionKey,
            BomAuthorityOutcome? authorityOutcome,
            RackOutputVerdictKind? outputVerdict,
            RackMetricResolutionOutcome? resolutionOutcome,
            SelectiveEffectiveOutcome? effectiveOutcome)
        {
            MetricId = metricId;
            SymbolId = symbolId;
            RackId = rackId;
            AuthorityId = authorityId;
            Phase = phase;
            RepresentativeDefinitionKey = representativeDefinitionKey;
            AuthorityOutcome = authorityOutcome;
            OutputVerdict = outputVerdict;
            ResolutionOutcome = resolutionOutcome;
            EffectiveOutcome = effectiveOutcome;
        }

        public MetricId MetricId { get; }

        /// <summary>El <see cref="SymbolId"/> de la metrica (namespace <c>rack</c>, D-16). Nulo en las metricas de proyecto.</summary>
        public SymbolId SymbolId { get; }

        /// <summary>El RackId canonico (D-11).</summary>
        public string RackId { get; }

        /// <summary>Un identificador de <see cref="MetricAuthorityIds"/>. Nulo cuando ninguna autoridad fuente aporta el valor.</summary>
        public string AuthorityId { get; }

        /// <summary>La fase minima de la metrica (D-13). Nula cuando no es <c>Supported</c>.</summary>
        public RackMetricPhase? Phase { get; }

        /// <summary>La <c>DefinitionKey</c> del representante que aprobo E4. Nula si E4 no llego a decidir.</summary>
        public string RepresentativeDefinitionKey { get; }

        /// <summary>El outcome de E4 (autoridad authored). Nulo si E4 no se evaluo.</summary>
        public BomAuthorityOutcome? AuthorityOutcome { get; }

        /// <summary>El veredicto E6 de la poblacion. Nulo si E6 no se evaluo.</summary>
        public RackOutputVerdictKind? OutputVerdict { get; }

        /// <summary>Como termino la resolucion Phi2+Phi3. Nulo si no se resolvio.</summary>
        public RackMetricResolutionOutcome? ResolutionOutcome { get; }

        /// <summary>El outcome del efectivo. Solo con <see cref="RackMetricResolutionOutcome.EffectiveFailed"/>.</summary>
        public SelectiveEffectiveOutcome? EffectiveOutcome { get; }
    }

    /// <summary>Un RackId excluido de la poblacion cotizable, con su motivo (D-21).</summary>
    public sealed class ExcludedRackProvenance
    {
        public ExcludedRackProvenance(string rackId, RackExclusionReason reason)
        {
            RackId = rackId;
            Reason = reason;
        }

        public string RackId { get; }

        public RackExclusionReason Reason { get; }
    }

    /// <summary>La provenance de una metrica de proyecto: su autoridad, los RackIds incluidos y los excluidos con su motivo (D-21).</summary>
    public sealed class AggregateProvenance
    {
        public AggregateProvenance(
            MetricId metricId,
            string kindToken,
            string authorityId,
            IReadOnlyList<string> includedRackIds,
            IReadOnlyList<ExcludedRackProvenance> excluded)
        {
            MetricId = metricId;
            KindToken = kindToken;
            AuthorityId = authorityId;
            IncludedRackIds = includedRackIds;
            Excluded = excluded;
        }

        public MetricId MetricId { get; }

        /// <summary>El sistema del agregado. Nulo en <c>(Project, totalRacks)</c>.</summary>
        public string KindToken { get; }

        /// <summary>Un identificador de <see cref="MetricAuthorityIds"/>. Nulo cuando el valor no sale de una suma ni de la poblacion.</summary>
        public string AuthorityId { get; }

        /// <summary>Los RackIds canonicos incluidos, en orden Ordinal.</summary>
        public IReadOnlyList<string> IncludedRackIds { get; }

        /// <summary>Los RackIds canonicos excluidos con su motivo, en orden Ordinal.</summary>
        public IReadOnlyList<ExcludedRackProvenance> Excluded { get; }
    }

    /// <summary>La provenance de los agregados de un <see cref="ProjectSummary"/>: <c>totalRacks</c> y, por sistema, sus tres metricas.</summary>
    public sealed class ProjectSummaryProvenance
    {
        public ProjectSummaryProvenance(IReadOnlyList<AggregateProvenance> aggregates)
        {
            Aggregates = aggregates ?? throw new ArgumentNullException(nameof(aggregates));
        }

        /// <summary><c>totalRacks</c> y luego, en el orden fijo de los seis sistemas, <c>rackCount</c>, <c>totalFrentes</c> y <c>totalFrentesVacios</c>.</summary>
        public IReadOnlyList<AggregateProvenance> Aggregates { get; }

        /// <summary>La provenance de una metrica de proyecto (<paramref name="kindToken"/> nulo para <c>totalRacks</c>), o nula.</summary>
        public AggregateProvenance Of(MetricId metric, string kindToken = null)
        {
            foreach (var aggregate in Aggregates)
            {
                if (aggregate.MetricId == metric
                    && string.Equals(aggregate.KindToken, kindToken, StringComparison.Ordinal))
                {
                    return aggregate;
                }
            }

            return null;
        }
    }
}
