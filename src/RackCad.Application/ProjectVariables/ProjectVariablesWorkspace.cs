using System;
using System.Collections.Generic;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Selective;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>Whether the central window may edit anything at all.</summary>
    public enum ProjectVariablesWorkspaceState
    {
        Editable = 1,

        /// <summary>Something could not be read safely. The window shows why and edits nothing.</summary>
        Blocked = 2,
    }

    /// <summary>One variable as the window presents it, with the racks that depend on it.</summary>
    public sealed class ProjectVariableRow
    {
        public ProjectVariableRow(
            VariableId id,
            string name,
            VariableType type,
            double literalValue,
            IReadOnlyList<VariableConsumerSummary> consumers)
        {
            Id = id;
            Name = name;
            Type = type;
            LiteralValue = literalValue;
            Consumers = consumers ?? new VariableConsumerSummary[0];
        }

        /// <summary>The authority. Every operation travels by this, never by <see cref="Name"/>.</summary>
        public VariableId Id { get; }

        public string Name { get; }

        public VariableType Type { get; }

        public double LiteralValue { get; }

        /// <summary>The racks bound to it, named so the user can act on them.</summary>
        public IReadOnlyList<VariableConsumerSummary> Consumers { get; }

        public int ConsumerCount => Consumers.Count;

        public bool HasConsumers => Consumers.Count > 0;
    }

    /// <summary>
    /// One binding this build cannot resolve, with everything a repair needs — including the literal that
    /// would govern afterwards, because the user has to decide knowing it.
    /// </summary>
    public sealed class BrokenBindingRow
    {
        public BrokenBindingRow(
            string rackId, string rackName, string propertyId, string variableId, double storedLiteral, string detail)
        {
            RackId = rackId;
            RackName = rackName;
            PropertyId = propertyId;
            VariableId = variableId;
            StoredLiteral = storedLiteral;
            Detail = detail;
        }

        public string RackId { get; }

        public string RackName { get; }

        public string PropertyId { get; }

        /// <summary>The variable the reference names, RAW: unreadable is not the same as absent.</summary>
        public string VariableId { get; }

        /// <summary>The frozen authored literal — what governs once the binding is removed.</summary>
        public double StoredLiteral { get; }

        public string Detail { get; }
    }

    /// <summary>
    /// Everything the central window sees, projected purely (I-47 G16).
    ///
    /// <para>
    /// The window decides nothing. Who consumes a variable is G5's answer; what an operation may do is G6's;
    /// writing is G11's. What this adds is a PROJECTION — and it adds it precisely so the window can be a
    /// window: it never touches the drawing, never resolves a binding and never holds a mutable model that
    /// disagrees with the DWG.
    /// </para>
    /// <para>
    /// Blocking is not decoration. A register that EXISTS and cannot be read does not get offered a "create
    /// your first variable" button: writing over it would destroy everything it holds, silently. And a placed
    /// definition whose envelope cannot be interpreted blocks too — without its identity there is no way to
    /// show it is not a consumer of one of these variables, so no count here would be honest. One that is not
    /// placed is not in the drawing and does not count: the same asymmetry the BOM uses.
    /// </para>
    /// </summary>
    public sealed class ProjectVariablesWorkspace
    {
        private static readonly ProjectVariableRow[] NoVariables = new ProjectVariableRow[0];
        private static readonly BrokenBindingRow[] NoBroken = new BrokenBindingRow[0];

        private ProjectVariablesWorkspace(
            ProjectVariablesWorkspaceState state,
            IReadOnlyList<ProjectVariableRow> variables,
            IReadOnlyList<BrokenBindingRow> brokenBindings,
            string error)
        {
            State = state;
            Variables = variables;
            BrokenBindings = brokenBindings;
            Error = error;
        }

        public ProjectVariablesWorkspaceState State { get; }

        public IReadOnlyList<ProjectVariableRow> Variables { get; }

        public IReadOnlyList<BrokenBindingRow> BrokenBindings { get; }

        /// <summary>The visible reason the window is blocked. Null when editable.</summary>
        public string Error { get; }

        public bool IsEditable => State == ProjectVariablesWorkspaceState.Editable;

        public static ProjectVariablesWorkspace Build(
            ProjectVariablesReadResult registry, IReadOnlyList<ProjectVariableScanEntry> entries)
        {
            if (registry == null)
            {
                return Blocked(
                    "No se consultó el registro de variables de proyecto de este dibujo, así que no hay nada " +
                    "que administrar con seguridad.");
            }

            if (registry.Outcome != ProjectVariablesReadOutcome.Absent &&
                registry.Outcome != ProjectVariablesReadOutcome.Readable)
            {
                // PresentButUnreadable / IncompatibleMajor. No editing, and above all no "create the first
                // variable": that write would replace a register whose contents this build cannot see.
                return Blocked(registry.Error);
            }

            var document = registry.Document;

            // A definition that is NOT placed is not in the drawing, so it cannot be a consumer of anything.
            // Dropping it here is what keeps an old, unplaced, unreadable leftover from blocking the register.
            var present = Placed(entries);

            // The repairs are computed FIRST and travel even on a blocked workspace. Otherwise the one thing
            // that fixes an indeterminate drawing -- removing the broken binding -- would be unreachable
            // precisely because the drawing is indeterminate.
            var broken = FindBroken(present, document);
            var unclassifiable = FirstPlacedUnclassifiable(present);

            if (unclassifiable != null)
            {
                return Blocked(unclassifiable, broken);
            }

            var rows = new List<ProjectVariableRow>();

            if (document != null)
            {
                foreach (var variable in document.ToProjectVariables())
                {
                    var discovery = ProjectVariableConsumerDiscovery.DiscoverConsumers(present, variable.Id);

                    if (!discovery.IsSuccess)
                    {
                        // A count that cannot be established is not a count of zero.
                        return Blocked(discovery.Error, broken);
                    }

                    rows.Add(new ProjectVariableRow(
                        variable.Id,
                        variable.Name,
                        variable.Type,
                        variable.Definition.LiteralValue,
                        ProjectVariableMutationPreflight.Summarize(discovery.Consumers, variable.Id)));
                }
            }

            return new ProjectVariablesWorkspace(
                ProjectVariablesWorkspaceState.Editable, rows, broken, null);
        }

        private static ProjectVariablesWorkspace Blocked(
            string error, IReadOnlyList<BrokenBindingRow> broken = null)
            => new ProjectVariablesWorkspace(
                ProjectVariablesWorkspaceState.Blocked, NoVariables, broken ?? NoBroken, error);

        /// <summary>
        /// The entries the drawing actually HAS. An unclassifiable definition with no references is a leftover
        /// of something drawn and erased: it is not evidence of anything, and it must not block the register.
        /// </summary>
        private static IReadOnlyList<ProjectVariableScanEntry> Placed(IReadOnlyList<ProjectVariableScanEntry> entries)
        {
            var present = new List<ProjectVariableScanEntry>();

            if (entries == null)
            {
                return present;
            }

            foreach (var entry in entries)
            {
                if (entry != null && (entry.OuterEnvelopeInterpretable || entry.DirectReferenceCount > 0))
                {
                    present.Add(entry);
                }
            }

            return present;
        }

        /// <summary>
        /// A definition carrying RackCad data this build cannot interpret AND placed in the drawing. It has no
        /// identity, so it cannot be shown to be unrelated — and a RackId is never invented for it.
        /// </summary>
        private static string FirstPlacedUnclassifiable(IReadOnlyList<ProjectVariableScanEntry> entries)
        {
            if (entries == null)
            {
                return null;
            }

            foreach (var entry in entries)
            {
                if (entry != null && !entry.OuterEnvelopeInterpretable && entry.DirectReferenceCount > 0)
                {
                    return "La definición de bloque '" + entry.DefinitionId + "' está colocada en el dibujo y " +
                           "lleva datos de RackCad que esta versión no puede interpretar, así que no se puede " +
                           "determinar a qué rack pertenece. No se administra el registro a ciegas.";
                }
            }

            return null;
        }

        /// <summary>
        /// The bindings that do not resolve, one row per rack and property — the sibling views of a rack are
        /// the SAME repair, not three. Resolution is asked of the one resolver; nothing about what a binding
        /// means is decided here.
        /// </summary>
        private static IReadOnlyList<BrokenBindingRow> FindBroken(
            IReadOnlyList<ProjectVariableScanEntry> entries, ProjectVariablesDocument document)
        {
            var rows = new List<BrokenBindingRow>();

            if (entries == null)
            {
                return rows;
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var resolver = new SelectiveEffectiveDesignResolver();

            foreach (var entry in entries)
            {
                if (entry == null || !entry.IsSelective || !entry.AuthoredReadable || entry.Authored == null)
                {
                    continue;
                }

                if (!entry.Authored.HasPropertyValues)
                {
                    continue;
                }

                var resolution = resolver.Resolve(entry.Authored, document);

                if (resolution.IsSuccess)
                {
                    continue;
                }

                var propertyId = resolution.PropertyId.Value ?? string.Empty;

                if (!seen.Add(entry.RackId + "|" + propertyId))
                {
                    continue;
                }

                rows.Add(new BrokenBindingRow(
                    entry.RackId,
                    entry.Authored.Name,
                    propertyId,
                    resolution.VariableId,
                    entry.Authored.VerticalClearance,
                    resolution.Error));
            }

            return rows;
        }
    }
}
