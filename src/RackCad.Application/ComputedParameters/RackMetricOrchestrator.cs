using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Selective;

namespace RackCad.Application.ComputedParameters
{
    /// <summary>
    /// Lo que la pertenencia (G2) ya decidio sobre las hermanas de UN RackId y que la tabla D-28 NO debe repetir
    /// (I-63 D-24: por RackId una lectura por vista y una autoridad). Cada campo es nulo mientras esa etapa no se haya
    /// evaluado para el rack. No es una cache: es el resultado de la unica evaluacion de la peticion, compartido.
    /// </summary>
    internal sealed class RackAssessedStages
    {
        /// <summary>Sin etapas previas: la peticion por rack (D-17) evalua todo por si misma.</summary>
        public static RackAssessedStages None => new RackAssessedStages();

        /// <summary>E5: el diseno de CADA hermana es legible. Nulo si la pertenencia no llego a leerlo.</summary>
        public bool? DesignReadable { get; set; }

        /// <summary>E4: la autoridad authored (exito o fallo). Nula si la pertenencia no llego a pedirla.</summary>
        public BomAuthorityResult Authority { get; set; }

        /// <summary>E6: el veredicto de salida. Nulo si la pertenencia no llego a evaluarlo.</summary>
        public RackOutputDecision OutputVerdict { get; set; }
    }

    /// <summary>Lo observado al calcular las metricas de un rack, para la provenance de D-21.</summary>
    internal sealed class RackMetricTrace
    {
        public string RepresentativeDefinitionKey { get; set; }

        public BomAuthorityOutcome? AuthorityOutcome { get; set; }

        public RackOutputVerdictKind? OutputVerdict { get; set; }

        public RackMetricResolutionOutcome? ResolutionOutcome { get; set; }

        public SelectiveEffectiveOutcome? EffectiveOutcome { get; set; }
    }

    /// <summary>El resultado de la tabla D-28 para un rack, mas lo observado y el provider que atendio su kind.</summary>
    internal sealed class RackMetricEvaluation
    {
        public RackMetricEvaluation(
            RackMetricResults results, RackMetricTrace trace, string kindToken, IRackMetricProvider provider)
        {
            Results = results;
            Trace = trace;
            KindToken = kindToken;
            Provider = provider;
        }

        public RackMetricResults Results { get; }

        public RackMetricTrace Trace { get; }

        /// <summary>El token de kind coherente. Nulo si el paso 1 de D-28 no lo decidio.</summary>
        public string KindToken { get; }

        /// <summary>El provider del kind. Nulo si el paso 1 de D-28 termino en un fallo de kind.</summary>
        public IRackMetricProvider Provider { get; }
    }

    /// <summary>
    /// La tabla de precedencia D-28 por rack, UNA sola vez (I-63 D-17, D-20 y D-28): la usan
    /// <see cref="RackMetricRequest"/> y <see cref="ProjectSummary"/> (<c>RackSummary.Metrics</c>), sin otra regla.
    /// </summary>
    internal static class RackMetricOrchestrator
    {
        public static RackMetricEvaluation Compute(
            string rackId,
            IReadOnlyList<RackMetricDefinitionProjection> members,
            ProjectVariablesReadResult registry,
            RackCatalogInput catalog,
            IRackMetricDesignReader designReader,
            IRackMetricResolutionSide resolutionSide,
            RackMetricProviderRegistry providers,
            RackAssessedStages stages)
        {
            var trace = new RackMetricTrace();
            if (stages.Authority != null)
            {
                trace.AuthorityOutcome = stages.Authority.Outcome;
                trace.RepresentativeDefinitionKey = stages.Authority.IsSuccess
                    ? stages.Authority.RepresentativeDefinitionId
                    : null;
            }

            if (stages.OutputVerdict != null)
            {
                trace.OutputVerdict = stages.OutputVerdict.Kind;
            }

            // Paso 1 de D-28: identidad y kind.
            var kindFailure = RackMetricRequest.ClassifyKind(members, out var kindToken);
            if (kindFailure != null)
            {
                return new RackMetricEvaluation(
                    RackMetricResults.Uniform(rackId, MetricValue.Unavailable(UnavailableReason.Of(kindFailure.Value))),
                    trace,
                    null,
                    null);
            }

            if (!providers.TryGet(kindToken, out var provider))
            {
                return new RackMetricEvaluation(
                    RackMetricResults.Uniform(
                        rackId, MetricValue.Unavailable(UnavailableReason.Of(UnavailableReasonKind.KindUnknown))),
                    trace,
                    null,
                    null);
            }

            // Paso 2: el soporte lo declara el provider. Sin ninguna metrica Supported, no se lee ni se resuelve.
            var needsPrerequisite = RackMetricIds.RackMetrics
                .Any(metric => provider.Declare(metric).Support == RackMetricSupport.Supported);

            var prerequisite = needsPrerequisite
                ? BuildPrerequisite(
                    rackId, kindToken, members, registry, catalog, designReader, resolutionSide, stages, trace)
                : null;

            return new RackMetricEvaluation(
                provider.Compute(new RackMetricInput(rackId, kindToken, prerequisite)), trace, kindToken, provider);
        }

        /// <summary>Pasos 3 a 5 de D-28, una sola vez por peticion y por rack.</summary>
        private static RackMetricPrerequisite BuildPrerequisite(
            string rackId,
            string kindToken,
            IReadOnlyList<RackMetricDefinitionProjection> members,
            ProjectVariablesReadResult registry,
            RackCatalogInput catalog,
            IRackMetricDesignReader designReader,
            IRackMetricResolutionSide resolutionSide,
            RackAssessedStages stages,
            RackMetricTrace trace)
        {
            if (!string.Equals(kindToken, RackEmbedDocument.KindSelective, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("En V1 solo el Selectivo declara metricas Supported.");
            }

            // Paso 3 (E5): diseno legible para cada hermana, con el lector D-26 (o lo que la pertenencia ya leyo).
            var readable = stages.DesignReadable
                           ?? members.All(member => designReader.IsReadable(kindToken, member.Envelope.Design));
            if (!readable)
            {
                return Failure(UnavailableReasonKind.DesignUnreadable);
            }

            // Paso 4 (E4): autoridad authored. La proyeccion vigente solo ve hermanas Known, coherentes y legibles.
            var authority = stages.Authority;
            if (authority == null)
            {
                var entries = members
                    .Select(member => ProjectVariableScanProjection.Project(
                        member.DefinitionKey, member.Envelope, member.DirectReferenceCount))
                    .ToList();
                authority = BomAuthoredAuthority.Resolve(rackId, entries);
            }

            trace.AuthorityOutcome = authority.Outcome;
            trace.RepresentativeDefinitionKey = authority.IsSuccess ? authority.RepresentativeDefinitionId : null;

            if (!authority.IsSuccess)
            {
                return Failure(authority.Outcome == BomAuthorityOutcome.DivergentSiblings
                    ? UnavailableReasonKind.SiblingsDivergent
                    : UnavailableReasonKind.DesignUnreadable);
            }

            // Paso 5: efectivo y resuelto, UNA vez.
            var resolution = resolutionSide.Resolve(authority.Authored, registry, catalog);
            trace.ResolutionOutcome = resolution.Outcome;
            trace.EffectiveOutcome = resolution.EffectiveOutcome;

            switch (resolution.Outcome)
            {
                case RackMetricResolutionOutcome.Resolved:
                    return RackMetricPrerequisite.Resolved(resolution.ResolvedSystem);

                case RackMetricResolutionOutcome.RegistryUnreadable:
                    return Failure(UnavailableReasonKind.RegistryUnreadable);

                case RackMetricResolutionOutcome.EffectiveFailed:
                    return RackMetricPrerequisite.Unavailable(
                        UnavailableReason.EffectiveFailed(resolution.EffectiveOutcome.Value));

                case RackMetricResolutionOutcome.CatalogUnavailable:
                    return Failure(UnavailableReasonKind.CatalogUnavailable);

                default:
                    return Failure(UnavailableReasonKind.ResolveFailed);
            }
        }

        private static RackMetricPrerequisite Failure(UnavailableReasonKind kind)
            => RackMetricPrerequisite.Unavailable(UnavailableReason.Of(kind));
    }
}
