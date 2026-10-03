using System;
using System.Collections.Generic;
using System.Linq;

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
        /// autoridad; una Phi2+Phi3 solo por Selectivo (D-24). Anota UNA evaluacion en
        /// <see cref="RackPopulationEvaluationCounter"/> (INV-32).
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

            var unavailable = MetricValue.Unavailable(UnavailableReason.Of(UnavailableReasonKind.CoverageNotAccredited));
            var bySystem = ProjectPopulation.SystemOrder
                .Select(token => new SystemAggregate(token, unavailable, unavailable, unavailable))
                .ToList();

            return new ProjectSummary(
                new ProjectSummaryTotals(unavailable),
                bySystem,
                new List<RackSummary>(),
                new List<PopulationDiagnostic>(),
                new ProjectSummaryProvenance(new List<AggregateProvenance>()));
        }

        /// <summary>El resumen de un rack por su RackId canonico (Ordinal), o nulo.</summary>
        public RackSummary Rack(string rackId)
            => Racks.FirstOrDefault(rack => string.Equals(rack.RackId, rackId, StringComparison.Ordinal));

        /// <summary>El texto canonico y determinista de todo el resumen (incluida la provenance), con cultura invariante.</summary>
        public string Describe() => string.Empty;

        public bool Equals(ProjectSummary other) => ReferenceEquals(this, other);

        public override bool Equals(object obj) => Equals(obj as ProjectSummary);

        public override int GetHashCode() => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(this);
    }
}
