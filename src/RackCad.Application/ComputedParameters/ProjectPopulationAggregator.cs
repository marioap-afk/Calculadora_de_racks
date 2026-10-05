using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;

namespace RackCad.Application.ComputedParameters
{
    /// <summary>
    /// Las tres metricas de proyecto de UN sistema (I-63 D-12): <c>(Project, rackCount)</c>,
    /// <c>(Project, totalFrentes)</c> y <c>(Project, totalFrentesVacios)</c>, siempre presentes.
    /// </summary>
    public sealed class SystemAggregate
    {
        public SystemAggregate(
            string kindToken, MetricValue rackCount, MetricValue totalFrentes, MetricValue totalFrentesVacios)
        {
            KindToken = kindToken;
            RackCount = rackCount;
            TotalFrentes = totalFrentes;
            TotalFrentesVacios = totalFrentesVacios;
        }

        public string KindToken { get; }

        public MetricValue RackCount { get; }

        public MetricValue TotalFrentes { get; }

        public MetricValue TotalFrentesVacios { get; }
    }

    /// <summary>Los agregados materializados por G2: <c>totalRacks</c> y <c>BySystem</c> con los seis sistemas.</summary>
    public sealed class ProjectPopulationAggregates
    {
        public ProjectPopulationAggregates(MetricValue totalRacks, IReadOnlyList<SystemAggregate> bySystem)
        {
            TotalRacks = totalRacks;
            BySystem = bySystem;
        }

        /// <summary><c>(Project, totalRacks)</c>.</summary>
        public MetricValue TotalRacks { get; }

        /// <summary>Los seis sistemas, siempre presentes y en el orden fijo de <see cref="ProjectPopulation.SystemOrder"/>.</summary>
        public IReadOnlyList<SystemAggregate> BySystem { get; }

        /// <summary>El agregado de un sistema, o null si el token no es uno de los seis.</summary>
        public SystemAggregate For(string kindToken)
            => BySystem.FirstOrDefault(item => string.Equals(item.KindToken, kindToken, StringComparison.Ordinal));
    }

    /// <summary>
    /// La agregacion de proyecto (I-63 D-12) como funcion PURA sobre la pertenencia de <see cref="ProjectPopulation"/> y
    /// las metricas por rack que le entrega el llamador (en G4, <c>ProjectSummary</c> <c>Full</c>). No resuelve nada.
    /// Un agregado es <c>Available</c> solo con la cobertura acreditada y todos sus miembros <c>Available</c>: nunca un parcial.
    /// </summary>
    public static class ProjectPopulationAggregator
    {
        public static ProjectPopulationAggregates Aggregate(
            ProjectPopulation population, IReadOnlyDictionary<string, RackMetricResults> rackMetrics)
        {
            if (population == null)
            {
                throw new ArgumentNullException(nameof(population));
            }

            var metrics = rackMetrics ?? new Dictionary<string, RackMetricResults>();
            var bySystem = ProjectPopulation.SystemOrder
                .Select(token => AggregateSystem(population, token, metrics))
                .ToList();

            return new ProjectPopulationAggregates(population.TotalRacks, bySystem);
        }

        private static SystemAggregate AggregateSystem(
            ProjectPopulation population, string kindToken, IReadOnlyDictionary<string, RackMetricResults> metrics)
        {
            var rackCount = population.RackCountBySystem
                .First(item => string.Equals(item.KindToken, kindToken, StringComparison.Ordinal))
                .RackCount;

            switch (kindToken)
            {
                case RackEmbedDocument.KindSelective:
                    return new SystemAggregate(
                        kindToken,
                        rackCount,
                        Sum(population, kindToken, metrics, RackMetricIds.Frentes),
                        Sum(population, kindToken, metrics, RackMetricIds.FrentesVacios));

                case RackEmbedDocument.KindCabecera:
                case RackEmbedDocument.KindCama:
                    return new SystemAggregate(
                        kindToken, rackCount, MetricValue.NotApplicable(), MetricValue.NotApplicable());

                default:
                    return new SystemAggregate(
                        kindToken, rackCount, MetricValue.NotSupported(), MetricValue.NotSupported());
            }
        }

        /// <summary>Suma de una metrica por rack de los incluidos del sistema; nunca parcial (INV-08).</summary>
        private static MetricValue Sum(
            ProjectPopulation population,
            string kindToken,
            IReadOnlyDictionary<string, RackMetricResults> metrics,
            MetricId metric)
        {
            if (!population.CoverageAccredited)
            {
                return MetricValue.Unavailable(UnavailableReason.Of(UnavailableReasonKind.CoverageNotAccredited));
            }

            var members = population.Racks
                .Where(rack => rack.Membership.Kind == RackMembershipKind.Included
                               && string.Equals(rack.KindToken, kindToken, StringComparison.Ordinal))
                .ToList();

            var total = 0.0;
            var failed = new List<string>();
            foreach (var rack in members)
            {
                if (metrics.TryGetValue(rack.RackId, out var results)
                    && results != null
                    && results.TryGet(metric, out var value)
                    && value.Status == MetricStatus.Available)
                {
                    total += value.Value;
                }
                else
                {
                    failed.Add(rack.RackId);
                }
            }

            return failed.Count == 0
                ? MetricValue.Available(total)
                : MetricValue.Unavailable(UnavailableReason.MemberMetricUnavailable(failed));
        }
    }
}
