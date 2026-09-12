using System.Collections.Generic;
using RackCad.Application.Persistence;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>Why a register is, or is not, usable as an authority of <see cref="VariableId"/> identity.</summary>
    internal enum ProjectVariablesAccreditationOutcome
    {
        /// <summary>Every entry was projected and every identity is unique. Safe to look up.</summary>
        Usable = 1,

        /// <summary>
        /// The register is persistence-readable but a <see cref="VariableId"/> designates two entries. That is
        /// not an identity, so nothing is looked up, chosen or repaired.
        /// </summary>
        AmbiguousIdentity = 2,

        /// <summary>The read itself never produced a document this build may interpret.</summary>
        NotReadable = 3,

        /// <summary>A snapshot handed in directly was not a valid target.</summary>
        InvalidTarget = 4,
    }

    /// <summary>The accreditation result: a usable registry, or the visible reason there is none.</summary>
    internal sealed class ProjectVariablesAccreditation
    {
        private ProjectVariablesAccreditation(
            ProjectVariablesAccreditationOutcome outcome,
            UsableProjectVariablesRegistry registry,
            ProjectVariablesDocument document,
            string error)
        {
            Outcome = outcome;
            Registry = registry;
            Document = document;
            Error = error;
        }

        internal ProjectVariablesAccreditationOutcome Outcome { get; }

        /// <summary>The accredited registry. Null on EVERY failure — there is nothing safe to hand back.</summary>
        internal UsableProjectVariablesRegistry Registry { get; }

        internal string Error { get; }

        /// <summary>
        /// The document this accreditation VOUCHES for, and the only one a mutation may be applied to. It is
        /// non-null only when a real read was accredited: the pure target path has no document, so a fixture
        /// can never produce something committable.
        ///
        /// <para>
        /// It exists so that "apply the change to the accredited document" is expressible as a value instead of
        /// as a convention. A caller that holds a raw read has nothing to apply a mutation to.
        /// </para>
        /// </summary>
        internal ProjectVariablesDocument Document { get; }

        internal bool IsUsable => Outcome == ProjectVariablesAccreditationOutcome.Usable;

        internal static ProjectVariablesAccreditation Usable(
            UsableProjectVariablesRegistry registry, ProjectVariablesDocument document = null)
            => new ProjectVariablesAccreditation(
                ProjectVariablesAccreditationOutcome.Usable, registry, document, null);

        internal static ProjectVariablesAccreditation Failed(
            ProjectVariablesAccreditationOutcome outcome, string error)
            => new ProjectVariablesAccreditation(outcome, null, null, error);
    }

    /// <summary>
    /// A register PROVEN safe to use as the authority of <see cref="VariableId"/> identity (I-48 G4A,
    /// Proposal V8 R-01/R-02, V7-R01).
    ///
    /// <para>
    /// <b>persistence-readable is NOT semantic-usable.</b> <see cref="ProjectVariablesStore"/> remains the
    /// single authority of JSON, schema, supported persisted types, definition kind and readability; this type
    /// is a WITNESS of that verdict, never a second judge. What it adds on top is one precondition the store
    /// deliberately does not check: that every <see cref="VariableId"/> designates exactly one entry.
    /// </para>
    /// <para>
    /// The reason it is a type and not a rule is that the type makes the guarantee structural: a caller cannot
    /// look anything up without holding one, so an unaccredited document cannot reach a lookup by forgetting a
    /// check. In particular a NULL document can never become an empty registry — that step is exactly how a
    /// present-but-unreadable register would be read as absent, destroying bindings that another build resolves
    /// perfectly (I-47 invariant, ADR-0034 §8).
    /// </para>
    /// <para>
    /// Duplicate identity fails CLOSED and nothing is repaired: the document is not modified, no entry is
    /// removed and no first-wins/last-wins is chosen. The inherited behaviour of the surfaces that read
    /// duplicates today is not a single policy, so there is nothing to preserve — and choosing one silently
    /// would be an arbitrary resolution of an ambiguous identity.
    /// </para>
    /// </summary>
    internal sealed class UsableProjectVariablesRegistry
    {
        private readonly Dictionary<VariableId, VariableTargetSnapshot> _byId;

        private UsableProjectVariablesRegistry(Dictionary<VariableId, VariableTargetSnapshot> byId)
        {
            _byId = byId;
        }

        internal int Count => _byId.Count;

        /// <summary>
        /// PRODUCTION path. Accredits ONE concrete read.
        ///
        /// <para>
        /// The projection from the document is MECHANICAL and deliberately so: the store already proved the
        /// identity parses, the type token is supported and the literal is finite, so this reads what was
        /// accredited instead of validating it again. It does NOT go through <c>ProjectVariable.Create</c>,
        /// which would re-impose the supported-type check this layer has no business repeating.
        /// </para>
        /// <para>
        /// An accreditation belongs to the read it was given. A later read is a DIFFERENT document and needs
        /// its own accreditation — including the re-read a commit performs before mutating.
        /// </para>
        /// </summary>
        internal static ProjectVariablesAccreditation Accredit(ProjectVariablesReadResult read)
        {
            if (read == null)
            {
                return ProjectVariablesAccreditation.Failed(
                    ProjectVariablesAccreditationOutcome.NotReadable,
                    "No se consulto el registro de variables de proyecto de este dibujo.");
            }

            if (read.Outcome != ProjectVariablesReadOutcome.Absent &&
                read.Outcome != ProjectVariablesReadOutcome.Readable)
            {
                // PresentButUnreadable / IncompatibleMajor. Never an empty registry.
                return ProjectVariablesAccreditation.Failed(
                    ProjectVariablesAccreditationOutcome.NotReadable, read.Error);
            }

            if (read.Document == null)
            {
                return ProjectVariablesAccreditation.Failed(
                    ProjectVariablesAccreditationOutcome.NotReadable,
                    "El registro de variables de proyecto no entrego documento, asi que no se puede acreditar.");
            }

            var byId = new Dictionary<VariableId, VariableTargetSnapshot>();

            foreach (var entry in read.Document.Variables ?? new List<ProjectVariableDocument>())
            {
                if (!VariableId.TryParse(entry?.VariableId, out var id) ||
                    !VariableTypes.TryParseToken(entry.Type, out var type) ||
                    entry.Definition?.Value == null)
                {
                    // The store accredited this document, so reaching here is an invariant violation, not a
                    // state to reinterpret. Fail loud rather than silently dropping an entry: a registry
                    // missing an entry would report a live variable as MISSING.
                    return ProjectVariablesAccreditation.Failed(
                        ProjectVariablesAccreditationOutcome.NotReadable,
                        "El registro fue aceptado por el store pero una entrada no se puede proyectar ('" +
                        (entry?.VariableId ?? "<null>") + "').");
                }

                if (!VariableTargetSnapshot.TryCreate(
                        id, type, entry.Definition.Value.Value, out var snapshot, out var error, entry.Name))
                {
                    return ProjectVariablesAccreditation.Failed(
                        ProjectVariablesAccreditationOutcome.NotReadable, error);
                }

                if (byId.ContainsKey(id))
                {
                    return ProjectVariablesAccreditation.Failed(
                        ProjectVariablesAccreditationOutcome.AmbiguousIdentity,
                        "El registro declara dos veces la variable '" + id + "'. Un VariableId que designa dos " +
                        "entradas no es una identidad: no se elige ninguna, no se repara el registro y no se " +
                        "aplica ninguna mutacion.");
                }

                byId.Add(id, snapshot);
            }

            return ProjectVariablesAccreditation.Usable(
                new UsableProjectVariablesRegistry(byId), read.Document);
        }

        /// <summary>
        /// PURE path, for the kernel and its tests. Builds a registry from targets directly, with no
        /// <see cref="ProjectVariablesReadResult"/>, no store and no <see cref="ProjectVariable"/>.
        ///
        /// <para>
        /// It is a pure-function seam, not runtime extensibility: it is unreachable from Plugin and UI, it is
        /// never populated from configuration, and it never interprets a null document as an empty register —
        /// it takes targets or nothing. It enforces the same identity invariant as the production path so a
        /// fixture cannot prove a lookup over an ambiguous target.
        /// </para>
        /// </summary>
        internal static ProjectVariablesAccreditation FromTargets(IEnumerable<VariableTargetSnapshot> targets)
        {
            if (targets == null)
            {
                return ProjectVariablesAccreditation.Failed(
                    ProjectVariablesAccreditationOutcome.InvalidTarget,
                    "No se entrego ningun conjunto de targets.");
            }

            var byId = new Dictionary<VariableId, VariableTargetSnapshot>();

            foreach (var target in targets)
            {
                if (target == null)
                {
                    return ProjectVariablesAccreditation.Failed(
                        ProjectVariablesAccreditationOutcome.InvalidTarget,
                        "El conjunto de targets contiene una entrada vacia.");
                }

                if (byId.ContainsKey(target.VariableId))
                {
                    return ProjectVariablesAccreditation.Failed(
                        ProjectVariablesAccreditationOutcome.AmbiguousIdentity,
                        "El conjunto de targets declara dos veces la variable '" + target.VariableId + "'.");
                }

                byId.Add(target.VariableId, target);
            }

            return ProjectVariablesAccreditation.Usable(new UsableProjectVariablesRegistry(byId));
        }

        /// <summary>The target a reference names, when this register holds it. A miss is MISSING, never ambiguous.</summary>
        internal bool TryGetTarget(VariableId variableId, out VariableTargetSnapshot target)
            => _byId.TryGetValue(variableId, out target);

        /// <summary>
        /// Every accredited target, ordered by identity so a listing never depends on Dictionary enumeration
        /// order. This is how a surface that ENUMERATES variables consumes the same authority as one that looks
        /// a single target up.
        /// </summary>
        internal IReadOnlyList<VariableTargetSnapshot> Targets()
        {
            var targets = new List<VariableTargetSnapshot>(_byId.Values);
            targets.Sort(static (left, right) =>
                string.Compare(left.VariableId.Value, right.VariableId.Value, System.StringComparison.OrdinalIgnoreCase));
            return targets;
        }
    }
}
