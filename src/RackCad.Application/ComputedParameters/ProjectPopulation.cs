using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Bom;
using RackCad.Application.Persistence;
using RackCad.Application.ProjectVariables;

namespace RackCad.Application.ComputedParameters
{
    /// <summary>Pertenencia de un rack a la poblacion cotizable (I-63 D-09).</summary>
    public enum RackMembershipKind
    {
        Included = 1,
        Excluded = 2,
        Undetermined = 3,
    }

    /// <summary>Razones cerradas de una exclusion (D-09). Una exclusion por regla no es parcialidad.</summary>
    public enum RackExclusionReason
    {
        NotPlaced = 1,
        KindAbsent = 2,
        KindUnknown = 3,
        SiblingsDivergent = 4,
        OutputDenied = 5,
    }

    /// <summary>Razones cerradas de una pertenencia <c>Undetermined</c> (D-09): dejan la cobertura sin acreditar.</summary>
    public enum RackUndeterminedReason
    {
        KindIncoherent = 1,
        DesignUnreadable = 2,
        CatalogUnavailable = 3,
        ResolveFailed = 4,
    }

    /// <summary><c>Included</c>, <c>Excluded(razon)</c> o <c>Undetermined(razon)</c>.</summary>
    public sealed class RackMembership : IEquatable<RackMembership>
    {
        private RackMembership(
            RackMembershipKind kind, RackExclusionReason? exclusion, RackUndeterminedReason? undetermined)
        {
            Kind = kind;
            ExclusionReason = exclusion;
            UndeterminedReason = undetermined;
        }

        public RackMembershipKind Kind { get; }

        public RackExclusionReason? ExclusionReason { get; }

        public RackUndeterminedReason? UndeterminedReason { get; }

        public static RackMembership Included { get; } = new RackMembership(RackMembershipKind.Included, null, null);

        public static RackMembership Excluded(RackExclusionReason reason)
            => new RackMembership(RackMembershipKind.Excluded, reason, null);

        public static RackMembership Undetermined(RackUndeterminedReason reason)
            => new RackMembership(RackMembershipKind.Undetermined, null, reason);

        public bool Equals(RackMembership other)
            => other != null
               && Kind == other.Kind
               && ExclusionReason == other.ExclusionReason
               && UndeterminedReason == other.UndeterminedReason;

        public override bool Equals(object obj) => Equals(obj as RackMembership);

        public override int GetHashCode()
            => (((int)Kind * 397) ^ ExclusionReason.GetHashCode()) * 397 ^ UndeterminedReason.GetHashCode();

        public override string ToString()
            => Kind == RackMembershipKind.Excluded ? "Excluded(" + ExclusionReason + ")"
                : Kind == RackMembershipKind.Undetermined ? "Undetermined(" + UndeterminedReason + ")"
                : Kind.ToString();
    }

    /// <summary>Codigos de los diagnosticos de la poblacion (I-63 D-09).</summary>
    public enum PopulationDiagnosticCode
    {
        /// <summary>Una definicion colocada sin identidad atribuible (E1): deja la cobertura sin acreditar.</summary>
        PlacedDefinitionWithoutIdentity = 1,

        /// <summary>Un rack excluido por regla, salvo <c>NotPlaced</c>.</summary>
        RackExcluded = 2,

        /// <summary>Un rack con pertenencia indeterminada.</summary>
        RackUndetermined = 3,
    }

    /// <summary>Un diagnostico de la poblacion: codigo, clave de definicion, RackId canonico (si existe) y razon.</summary>
    public sealed class PopulationDiagnostic
    {
        public PopulationDiagnostic(PopulationDiagnosticCode code, string rackId, string definitionKey, string reason)
        {
            Code = code;
            RackId = rackId;
            DefinitionKey = definitionKey;
            Reason = reason;
        }

        public PopulationDiagnosticCode Code { get; }

        /// <summary>La grafia canonica del RackId. Nula cuando no hay identidad atribuible.</summary>
        public string RackId { get; }

        public string DefinitionKey { get; }

        /// <summary>La razon, como dato: el nombre de la exclusion, de la indeterminacion o de la clasificacion de D-10a.</summary>
        public string Reason { get; }

        public override string ToString() => Code + "(" + RackId + ", " + DefinitionKey + ", " + Reason + ")";
    }

    /// <summary>Un RackId atribuible con su pertenencia (I-63 D-11 / D-20). Sin metricas por rack: esas son de G4.</summary>
    public sealed class PopulationRack
    {
        public PopulationRack(
            string rackId,
            string kindToken,
            string displayName,
            RackMembership membership,
            string representativeDefinitionId,
            IReadOnlyList<string> definitionKeys)
        {
            RackId = rackId;
            KindToken = kindToken;
            DisplayName = displayName;
            Membership = membership;
            RepresentativeDefinitionId = representativeDefinitionId;
            DefinitionKeys = definitionKeys;
        }

        /// <summary>La grafia canonica: la minima en Ordinal de las observadas (D-11.2).</summary>
        public string RackId { get; }

        /// <summary>El token de kind coherente del rack. Nulo con Kind ausente o con kinds mezclados.</summary>
        public string KindToken { get; }

        /// <summary>El primer <c>Name</c> no vacio en orden canonico, con <c>Trim()</c> (D-11.6). Solo presentacion.</summary>
        public string DisplayName { get; }

        public RackMembership Membership { get; }

        /// <summary>La vista que aprobo E4 (la primera en orden canonico). Nula si E4 no llego a decidir.</summary>
        public string RepresentativeDefinitionId { get; }

        /// <summary>Las claves de todas las definiciones del RackId, colocadas o no, en orden canonico.</summary>
        public IReadOnlyList<string> DefinitionKeys { get; }
    }

    /// <summary>La columna <c>(Project, rackCount)</c> de un sistema.</summary>
    public sealed class SystemRackCount
    {
        public SystemRackCount(string kindToken, MetricValue rackCount)
        {
            KindToken = kindToken;
            RackCount = rackCount;
        }

        public string KindToken { get; }

        public MetricValue RackCount { get; }
    }

    /// <summary>
    /// La poblacion cotizable de un proyecto (I-63 D-10..D-12, D-20 nivel <c>Population</c>): pertenencia por RackId,
    /// <c>(Project, totalRacks)</c> y <c>(Project, rackCount)</c> por sistema. NO tiene campos de metricas por rack y
    /// nunca ejecuta Phi2 ni Phi3 de metricas: solo la Phi3 del veredicto de salida de Push Back (E6).
    /// </summary>
    public sealed class ProjectPopulation
    {
        /// <summary>Los seis sistemas, en el orden fijo de D-12.</summary>
        public static IReadOnlyList<string> SystemOrder { get; } = new[]
        {
            RackEmbedDocument.KindSelective,
            RackEmbedDocument.KindDynamic,
            RackEmbedDocument.KindPushBack,
            RackEmbedDocument.KindCantilever,
            RackEmbedDocument.KindCabecera,
            RackEmbedDocument.KindCama,
        };

        private ProjectPopulation(
            IReadOnlyList<PopulationRack> racks,
            IReadOnlyList<PopulationDiagnostic> diagnostics,
            bool coverageAccredited,
            MetricValue totalRacks,
            IReadOnlyList<SystemRackCount> rackCountBySystem)
        {
            Racks = racks;
            Diagnostics = diagnostics;
            CoverageAccredited = coverageAccredited;
            TotalRacks = totalRacks;
            RackCountBySystem = rackCountBySystem;
        }

        /// <summary>Todos los RackIds atribuibles (incluidos, excluidos e indeterminados), por RackId canonico Ordinal.</summary>
        public IReadOnlyList<PopulationRack> Racks { get; }

        /// <summary>Por (codigo, clave de definicion Ordinal, RackId Ordinal).</summary>
        public IReadOnlyList<PopulationDiagnostic> Diagnostics { get; }

        /// <summary>Ninguna definicion colocada sin identidad atribuible y ningun rack <c>Undetermined</c>.</summary>
        public bool CoverageAccredited { get; }

        /// <summary><c>(Project, totalRacks)</c>.</summary>
        public MetricValue TotalRacks { get; }

        /// <summary><c>(Project, rackCount)</c> de los seis sistemas, siempre presentes y en <see cref="SystemOrder"/>.</summary>
        public IReadOnlyList<SystemRackCount> RackCountBySystem { get; }

        /// <summary>
        /// El orquestador real de la poblacion. Anota UNA evaluacion en <see cref="RackPopulationEvaluationCounter"/>
        /// por llamada (INV-32). <paramref name="designReader"/> es el costado donde se observan las lecturas D-26.
        /// </summary>
        public static ProjectPopulation Evaluate(
            RackMetricPopulationInput input, IRackMetricDesignReader designReader = null)
            => EvaluateAssessed(input, designReader).Population;

        /// <summary>
        /// La misma evaluacion (UNA anotada en <see cref="RackPopulationEvaluationCounter"/>) que ademas entrega, por rack,
        /// las etapas E4/E5/E6 que ya decidio, para que <see cref="ProjectSummary"/> no las repita (D-24).
        /// </summary>
        internal static AssessedPopulation EvaluateAssessed(
            RackMetricPopulationInput input, IRackMetricDesignReader designReader = null)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            RackPopulationEvaluationCounter.Record();
            var reader = designReader ?? new RackMetricDesignReader();
            var diagnostics = new List<PopulationDiagnostic>();

            // D-11.4: orden canonico por DefinitionKey (Ordinal) ANTES de E3, E4, E5 y E6.
            var projections = RackMetricDefinitionProjection.CanonicalOrder(input.Definitions)
                .Select(RackMetricDefinitionProjection.Project)
                .ToList();

            // E1: una definicion colocada sin identidad deja la cobertura sin acreditar; sin colocar se ignora.
            var coverageGap = false;
            foreach (var projection in projections.Where(item => !item.HasIdentity && item.DirectReferenceCount > 0))
            {
                coverageGap = true;
                diagnostics.Add(new PopulationDiagnostic(
                    PopulationDiagnosticCode.PlacedDefinitionWithoutIdentity,
                    null,
                    projection.DefinitionKey,
                    projection.Classification.ToString()));
            }

            // D-11.1: igualdad de RackId OrdinalIgnoreCase. D-11.3: hermanas colocadas o no.
            var groups = projections
                .Where(item => item.HasIdentity)
                .GroupBy(item => item.RackId, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.ToList())
                .OrderBy(CanonicalSpelling, StringComparer.Ordinal)
                .ToList();

            var racks = new List<PopulationRack>();
            var assessments = new List<RackAssessment>();
            foreach (var members in groups)
            {
                var stages = new RackAssessedStages();
                racks.Add(EvaluateRack(members, input.Catalog, reader, diagnostics, stages));
                assessments.Add(new RackAssessment(members, stages));
            }

            var accredited = !coverageGap
                             && racks.All(rack => rack.Membership.Kind != RackMembershipKind.Undetermined);

            var included = racks.Where(rack => rack.Membership.Kind == RackMembershipKind.Included).ToList();

            var totalRacks = accredited
                ? MetricValue.Available(included.Count)
                : MetricValue.Unavailable(UnavailableReason.Of(UnavailableReasonKind.CoverageNotAccredited));

            var rackCounts = SystemOrder
                .Select(token => new SystemRackCount(
                    token,
                    accredited
                        ? MetricValue.Available(included.Count(
                            rack => string.Equals(rack.KindToken, token, StringComparison.Ordinal)))
                        : MetricValue.Unavailable(UnavailableReason.Of(UnavailableReasonKind.CoverageNotAccredited))))
                .ToList();

            var orderedDiagnostics = diagnostics
                .OrderBy(item => (int)item.Code)
                .ThenBy(item => item.DefinitionKey ?? string.Empty, StringComparer.Ordinal)
                .ThenBy(item => item.RackId ?? string.Empty, StringComparer.Ordinal)
                .ToList();

            return new AssessedPopulation(
                new ProjectPopulation(racks, orderedDiagnostics, accredited, totalRacks, rackCounts), assessments);
        }

        /// <summary>D-11.2: la grafia canonica de un grupo es la minima en Ordinal de las observadas.</summary>
        private static string CanonicalSpelling(IReadOnlyList<RackMetricDefinitionProjection> members)
            => members.Select(member => member.RackId).OrderBy(spelling => spelling, StringComparer.Ordinal).First();

        /// <summary>D-10, E2..E6 en el orden congelado; la primera condicion que decide gana.</summary>
        private static PopulationRack EvaluateRack(
            IReadOnlyList<RackMetricDefinitionProjection> members,
            RackCatalogInput catalog,
            IRackMetricDesignReader reader,
            List<PopulationDiagnostic> diagnostics,
            RackAssessedStages stages)
        {
            var rackId = CanonicalSpelling(members);
            var keys = members.Select(member => member.DefinitionKey).ToList();

            // D-11.6: primer Name no vacio en orden canonico, con Trim().
            var displayName = members
                .Select(member => member.Envelope?.Name)
                .FirstOrDefault(name => !string.IsNullOrWhiteSpace(name))
                ?.Trim();

            var kindFailure = RackMetricRequest.ClassifyKind(members, out var coherentKind);
            var kindToken = kindFailure == UnavailableReasonKind.KindUnknown ? members[0].KindToken : coherentKind;

            // La vista que aprobo E4; queda nula mientras E4 no haya decidido.
            string representative = null;

            PopulationRack Result(RackMembership membership)
                => new PopulationRack(rackId, kindToken, displayName, membership, representative, keys);

            PopulationRack Excluded(RackExclusionReason reason)
            {
                if (reason != RackExclusionReason.NotPlaced)
                {
                    diagnostics.Add(new PopulationDiagnostic(
                        PopulationDiagnosticCode.RackExcluded, rackId, keys[0], reason.ToString()));
                }

                return Result(RackMembership.Excluded(reason));
            }

            PopulationRack Undetermined(RackUndeterminedReason reason)
            {
                diagnostics.Add(new PopulationDiagnostic(
                    PopulationDiagnosticCode.RackUndetermined, rackId, keys[0], reason.ToString()));
                return Result(RackMembership.Undetermined(reason));
            }

            // E2: colocado, con alguna hermana con DirectReferenceCount > 0.
            if (!members.Any(member => member.DirectReferenceCount > 0))
            {
                return Excluded(RackExclusionReason.NotPlaced);
            }

            // E3: kind coherente.
            switch (kindFailure)
            {
                case UnavailableReasonKind.KindAbsent:
                    return Excluded(RackExclusionReason.KindAbsent);

                case UnavailableReasonKind.KindUnknown:
                    return Excluded(RackExclusionReason.KindUnknown);

                case UnavailableReasonKind.KindIncoherent:
                    return Undetermined(RackUndeterminedReason.KindIncoherent);
            }

            // E5: diseno legible para CADA hermana (D-26).
            var readable = true;
            foreach (var member in members)
            {
                if (!reader.IsReadable(coherentKind, member.Envelope.Design))
                {
                    readable = false;
                    break;
                }
            }

            stages.DesignReadable = readable;
            if (!readable)
            {
                return Undetermined(RackUndeterminedReason.DesignUnreadable);
            }

            // E4: autoridad authored sobre la entrada tipada, en orden canonico.
            var entries = members
                .Select(member => ProjectVariableScanProjection.Project(
                    member.DefinitionKey, member.Envelope, member.DirectReferenceCount))
                .ToList();

            var authority = BomAuthoredAuthority.Resolve(rackId, entries);
            stages.Authority = authority;
            if (!authority.IsSuccess)
            {
                return authority.Outcome == BomAuthorityOutcome.DivergentSiblings
                    ? Excluded(RackExclusionReason.SiblingsDivergent)
                    : Undetermined(RackUndeterminedReason.DesignUnreadable);
            }

            // E6: el veredicto de salida, sobre el diseno del representante.
            representative = authority.RepresentativeDefinitionId;
            var representativeView = members.First(member => member.DefinitionKey == representative);
            var verdict = RackOutputVerdict.Evaluate(coherentKind, representativeView.Envelope.Design, catalog);
            stages.OutputVerdict = verdict;
            switch (verdict.Kind)
            {
                case RackOutputVerdictKind.Deny:
                    return Excluded(RackExclusionReason.OutputDenied);

                case RackOutputVerdictKind.Undetermined:
                    return Undetermined(MapUndetermined(verdict.UndeterminedReason.Value));

                default:
                    return Result(RackMembership.Included);
            }
        }

        private static RackUndeterminedReason MapUndetermined(RackOutputUndeterminedReason reason)
        {
            switch (reason)
            {
                case RackOutputUndeterminedReason.DesignUnreadable:
                    return RackUndeterminedReason.DesignUnreadable;

                case RackOutputUndeterminedReason.CatalogUnavailable:
                    return RackUndeterminedReason.CatalogUnavailable;

                default:
                    return RackUndeterminedReason.ResolveFailed;
            }
        }
    }

    /// <summary>La poblacion y, por rack (en el mismo orden de <see cref="ProjectPopulation.Racks"/>), lo que la pertenencia decidio.</summary>
    internal sealed class AssessedPopulation
    {
        public AssessedPopulation(ProjectPopulation population, IReadOnlyList<RackAssessment> assessments)
        {
            Population = population;
            Assessments = assessments;
        }

        public ProjectPopulation Population { get; }

        public IReadOnlyList<RackAssessment> Assessments { get; }
    }

    /// <summary>Las hermanas de un RackId, en orden canonico, y las etapas que la pertenencia ya evaluo sobre ellas.</summary>
    internal sealed class RackAssessment
    {
        public RackAssessment(IReadOnlyList<RackMetricDefinitionProjection> members, RackAssessedStages stages)
        {
            Members = members;
            Stages = stages;
        }

        public IReadOnlyList<RackMetricDefinitionProjection> Members { get; }

        public RackAssessedStages Stages { get; }
    }
}
