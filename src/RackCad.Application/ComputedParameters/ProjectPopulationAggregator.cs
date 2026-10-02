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

            // Esqueleto del RED: los seis sistemas, pero sin agregar nada.
            var bySystem = ProjectPopulation.SystemOrder
                .Select(token => new SystemAggregate(
                    token, MetricValue.NotSupported(), MetricValue.NotSupported(), MetricValue.NotSupported()))
                .ToList();

            return new ProjectPopulationAggregates(population.TotalRacks, bySystem);
        }
    }
}
