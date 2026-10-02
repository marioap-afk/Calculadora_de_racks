using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;
using RackCad.Application.Systems.Selective;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.ComputedParameters
{
    /// <summary>Como termino la resolucion Phi2+Phi3 de un Selectivo.</summary>
    public enum RackMetricResolutionOutcome
    {
        Resolved = 1,
        RegistryUnreadable = 2,
        EffectiveFailed = 3,
        CatalogUnavailable = 4,
        ResolveFailed = 5,
    }

    /// <summary>El resultado de una resolucion Phi2+Phi3: el sistema resuelto o el motivo del fallo (D-28, paso 5).</summary>
    public sealed class RackMetricResolutionResult
    {
        private RackMetricResolutionResult(
            RackMetricResolutionOutcome outcome, SelectiveRackSystem system, SelectiveEffectiveOutcome? effectiveOutcome)
        {
            Outcome = outcome;
            ResolvedSystem = system;
            EffectiveOutcome = effectiveOutcome;
        }

        public RackMetricResolutionOutcome Outcome { get; }

        /// <summary>El sistema resuelto. Nulo en todo fallo.</summary>
        public SelectiveRackSystem ResolvedSystem { get; }

        /// <summary>El outcome del efectivo; solo con <see cref="RackMetricResolutionOutcome.EffectiveFailed"/>.</summary>
        public SelectiveEffectiveOutcome? EffectiveOutcome { get; }

        public static RackMetricResolutionResult Resolved(SelectiveRackSystem system)
            => new RackMetricResolutionResult(RackMetricResolutionOutcome.Resolved, system, null);

        public static RackMetricResolutionResult Failed(RackMetricResolutionOutcome outcome)
            => new RackMetricResolutionResult(outcome, null, null);

        public static RackMetricResolutionResult EffectiveFailed(SelectiveEffectiveOutcome outcome)
            => new RackMetricResolutionResult(RackMetricResolutionOutcome.EffectiveFailed, null, outcome);
    }

    /// <summary>
    /// El costado de resolucion (I-63 D-24, A-1.3): lo que ejecuta Phi2+Phi3 del Selectivo. La peticion lo invoca
    /// COMO MAXIMO una vez; una prueba lo envuelve para contar, sin tocar resolvers ni stores.
    /// </summary>
    public interface IRackMetricResolutionSide
    {
        RackMetricResolutionResult Resolve(
            SelectivePalletDesignDocument authored, ProjectVariablesReadResult registry, RackCatalogInput catalog);
    }

    /// <summary>Implementacion real: efectivo (<c>ResolveAccredited</c>) y luego resuelto (<c>SelectiveGeometryResolver</c>).</summary>
    public sealed class RackMetricResolutionSide : IRackMetricResolutionSide
    {
        public RackMetricResolutionResult Resolve(
            SelectivePalletDesignDocument authored, ProjectVariablesReadResult registry, RackCatalogInput catalog)
        {
            // ESQUELETO RED: no resuelve nada.
            return RackMetricResolutionResult.Failed(RackMetricResolutionOutcome.ResolveFailed);
        }
    }

    /// <summary>
    /// La operacion por rack (I-63 D-17): <c>RackMetricRequest(hermanas de UN RackId, lectura del registro, catalogo)
    /// -> RackMetricResults</c>. No enumera el proyecto ni evalua poblacion. Aplica la precedencia D-28 y resuelve
    /// COMO MAXIMO ese rack: una sola Phi2+Phi3, y solo si D-28 llega al paso 5.
    /// </summary>
    public sealed class RackMetricRequest
    {
        private readonly IReadOnlyList<RackDefinitionCapture> _siblings;
        private readonly ProjectVariablesReadResult _registry;
        private readonly RackCatalogInput _catalog;
        private readonly IRackMetricDesignReader _designReader;
        private readonly IRackMetricResolutionSide _resolutionSide;
        private readonly RackMetricProviderRegistry _providers;

        public RackMetricRequest(
            IReadOnlyList<RackDefinitionCapture> siblings,
            ProjectVariablesReadResult registry,
            RackCatalogInput catalog,
            IRackMetricDesignReader designReader = null,
            IRackMetricResolutionSide resolutionSide = null,
            RackMetricProviderRegistry providers = null)
        {
            _siblings = siblings ?? throw new ArgumentNullException(nameof(siblings));
            _catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
            _registry = registry;
            _designReader = designReader ?? new RackMetricDesignReader();
            _resolutionSide = resolutionSide ?? new RackMetricResolutionSide();
            _providers = providers ?? RackMetricProviderRegistry.Default;
        }

        public RackMetricResults Execute()
        {
            // ESQUELETO RED: toda metrica en un estado fijo que las pruebas rechazan.
            return RackMetricResults.Uniform(
                null, MetricValue.Unavailable(UnavailableReason.Of(UnavailableReasonKind.EnvelopeUnreadable)));
        }
    }
}
