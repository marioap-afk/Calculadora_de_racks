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
            if (authored == null)
            {
                throw new ArgumentNullException(nameof(authored));
            }

            if (catalog == null)
            {
                throw new ArgumentNullException(nameof(catalog));
            }

            if (registry == null
                || registry.Outcome == ProjectVariablesReadOutcome.PresentButUnreadable
                || registry.Outcome == ProjectVariablesReadOutcome.IncompatibleMajor)
            {
                return RackMetricResolutionResult.Failed(RackMetricResolutionOutcome.RegistryUnreadable);
            }

            var effective = new SelectiveEffectiveDesignResolver().ResolveAccredited(authored, registry);
            if (!effective.IsSuccess)
            {
                return RackMetricResolutionResult.EffectiveFailed(effective.Outcome);
            }

            if (!catalog.IsLoaded)
            {
                return RackMetricResolutionResult.Failed(RackMetricResolutionOutcome.CatalogUnavailable);
            }

            try
            {
                return RackMetricResolutionResult.Resolved(
                    new SelectiveGeometryResolver().Resolve(effective.Design, catalog.Catalog));
            }
            catch (Exception)
            {
                return RackMetricResolutionResult.Failed(RackMetricResolutionOutcome.ResolveFailed);
            }
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
            // D-11: orden canonico por DefinitionKey (Ordinal) ANTES de clasificar, leer o pedir la autoridad.
            var ordered = RackMetricDefinitionProjection.CanonicalOrder(_siblings);

            // D-10a + O-PV2-4: una hermana sin identidad atribuible no pertenece a ningun RackId.
            var members = ordered
                .Select(RackMetricDefinitionProjection.Project)
                .Where(projection => projection.HasIdentity)
                .ToList();

            // D-17: la peticion recibe las hermanas de UN RackId; sin ninguna identificable viola la precondicion.
            if (members.Count == 0)
            {
                throw new ArgumentException("La peticion por rack exige hermanas con un RackId identificable.", "siblings");
            }

            var spellings = members.Select(member => member.RackId).Distinct(StringComparer.Ordinal).ToList();
            if (spellings.Distinct(StringComparer.OrdinalIgnoreCase).Count() > 1)
            {
                throw new ArgumentException("La peticion por rack recibe las hermanas de UN solo RackId.", "siblings");
            }

            // D-11.2: la grafia canonica es la minima en Ordinal.
            var rackId = spellings.OrderBy(spelling => spelling, StringComparer.Ordinal).First();

            // Paso 1 de D-28: identidad y kind.
            var kindFailure = ClassifyKind(members, out var kindToken);
            if (kindFailure != null)
            {
                return RackMetricResults.Uniform(rackId, MetricValue.Unavailable(UnavailableReason.Of(kindFailure.Value)));
            }

            if (!_providers.TryGet(kindToken, out var provider))
            {
                return RackMetricResults.Uniform(
                    rackId, MetricValue.Unavailable(UnavailableReason.Of(UnavailableReasonKind.KindUnknown)));
            }

            // Paso 2: el soporte lo declara el provider. Sin ninguna metrica Supported, no se lee ni se resuelve.
            var needsPrerequisite = RackMetricIds.RackMetrics
                .Any(metric => provider.Declare(metric).Support == RackMetricSupport.Supported);

            var prerequisite = needsPrerequisite ? BuildPrerequisite(rackId, kindToken, members) : null;

            return provider.Compute(new RackMetricInput(rackId, kindToken, prerequisite));
        }

        /// <summary>Paso 1 de D-28. Devuelve la razon si el kind no decide, o null y el token coherente.</summary>
        private static UnavailableReasonKind? ClassifyKind(
            IReadOnlyList<RackMetricDefinitionProjection> members, out string kindToken)
        {
            kindToken = null;

            if (members.All(member => member.Classification == RackDefinitionClass.KindAbsent))
            {
                return UnavailableReasonKind.KindAbsent;
            }

            var firstToken = members[0].KindToken;
            var sameToken = members.All(member => string.Equals(member.KindToken, firstToken, StringComparison.Ordinal));

            if (members.All(member => member.Classification == RackDefinitionClass.KindUnknown) && sameToken)
            {
                return UnavailableReasonKind.KindUnknown;
            }

            if (members.All(member => member.Classification == RackDefinitionClass.Known) && sameToken)
            {
                kindToken = firstToken;
                return null;
            }

            return UnavailableReasonKind.KindIncoherent;
        }

        /// <summary>Pasos 3 a 5 de D-28, una sola vez por peticion.</summary>
        private RackMetricPrerequisite BuildPrerequisite(
            string rackId, string kindToken, IReadOnlyList<RackMetricDefinitionProjection> members)
        {
            if (!string.Equals(kindToken, RackEmbedDocument.KindSelective, StringComparison.Ordinal))
            {
                throw new InvalidOperationException("En V1 solo el Selectivo declara metricas Supported.");
            }

            // Paso 3 (E5): diseno legible para cada hermana, con el lector D-26.
            foreach (var member in members)
            {
                if (!_designReader.IsReadable(kindToken, member.Envelope.Design))
                {
                    return Failure(UnavailableReasonKind.DesignUnreadable);
                }
            }

            // Paso 4 (E4): autoridad authored. La proyeccion vigente solo ve hermanas Known, coherentes y legibles.
            var entries = members
                .Select(member => ProjectVariableScanProjection.Project(
                    member.DefinitionKey, member.Envelope, member.DirectReferenceCount))
                .ToList();

            var authority = BomAuthoredAuthority.Resolve(rackId, entries);
            if (!authority.IsSuccess)
            {
                return Failure(authority.Outcome == BomAuthorityOutcome.DivergentSiblings
                    ? UnavailableReasonKind.SiblingsDivergent
                    : UnavailableReasonKind.DesignUnreadable);
            }

            // Paso 5: efectivo y resuelto, UNA vez.
            var resolution = _resolutionSide.Resolve(authority.Authored, _registry, _catalog);
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
