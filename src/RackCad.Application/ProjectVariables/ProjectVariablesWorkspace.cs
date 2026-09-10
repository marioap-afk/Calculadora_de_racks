using System;
using System.Collections.Generic;
using RackCad.Application.Persistence;

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
            string rackId,
            string rackName,
            string propertyId,
            string variableId,
            double storedLiteral,
            string detail,
            bool rackCanRepair,
            string rackBlockingReason)
        {
            RackId = rackId;
            RackName = rackName;
            PropertyId = propertyId;
            VariableId = variableId;
            StoredLiteral = storedLiteral;
            Detail = detail;
            RackCanRepair = rackCanRepair;
            RackBlockingReason = rackBlockingReason;
        }

        public string RackId { get; }

        public string RackName { get; }

        public string PropertyId { get; }

        /// <summary>The variable the reference names, RAW: unreadable is not the same as absent.</summary>
        public string VariableId { get; }

        /// <summary>The frozen authored literal — what governs once the binding is removed.</summary>
        public double StoredLiteral { get; }

        public string Detail { get; }

        /// <summary>
        /// Whether the RACK this row belongs to can be repaired. Application decides it; a surface must NOT
        /// derive it by looking at the other rows, because that would put the meaning of FATAL in the UI.
        ///
        /// <para>
        /// False means this row is a DIAGNOSTIC: still worth showing, never an actionable repair. Repair is
        /// rack-scoped, so a single fatal state anywhere in the rack makes every row of it unactionable.
        /// </para>
        /// </summary>
        public bool RackCanRepair { get; }

        /// <summary>Why the rack cannot be repaired, when it cannot. Null when it can.</summary>
        public string RackBlockingReason { get; }
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

            var accreditation = UsableProjectVariablesRegistry.Accredit(registry);

            if (!accreditation.IsUsable)
            {
                // Includes the ambiguous identity: a register that is persistence-readable but declares a
                // VariableId twice cannot be administered, because every row and every count would have to
                // pick one of the two.
                return Blocked(accreditation.Error);
            }

            var usable = accreditation.Registry;

            // A definition that is NOT placed is not in the drawing, so it cannot be a consumer of anything.
            // Dropping it here is what keeps an old, unplaced, unreadable leftover from blocking the register.
            var present = Placed(entries);

            // The repairs are computed FIRST and travel even on a blocked workspace. Otherwise the one thing
            // that fixes an indeterminate drawing -- removing the broken binding -- would be unreachable
            // precisely because the drawing is indeterminate.
            var broken = FindBroken(present, usable);
            var unclassifiable = FirstPlacedUnclassifiable(present);

            if (unclassifiable != null)
            {
                return Blocked(unclassifiable, broken);
            }

            var rows = new List<ProjectVariableRow>();

            // The listing reads the SAME accredited authority a lookup reads. Before I-48 G4B it walked the
            // document on its own, which is how a listing and a resolution could disagree.
            foreach (var target in usable.Targets())
            {
                var discovery = ProjectVariableConsumerDiscovery.DiscoverConsumers(present, target.VariableId);

                if (!discovery.IsSuccess)
                {
                    // A count that cannot be established is not a count of zero.
                    return Blocked(discovery.Error, broken);
                }

                rows.Add(new ProjectVariableRow(
                    target.VariableId,
                    target.Name,
                    target.VariableType,
                    target.LiteralValue,
                    ProjectVariableMutationPreflight.Summarize(discovery.Consumers, target.VariableId)));
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
        /// Every binding of every rack that does NOT resolve, one row per rack and property (I-48 G4B).
        ///
        /// <para>
        /// It no longer stops at the first failure of a whole-document resolve. That shortcut reported ONE row
        /// per rack, so a rack with two broken references surfaced one, and the second only appeared after the
        /// first was fixed. The scan is now complete and every state is classified.
        /// </para>
        /// <para>
        /// <b>Repairability is a property of the RACK.</b> Each row carries the rack's verdict, computed over
        /// its complete scan, so a surface never has to derive "is there a fatal elsewhere" by inspecting the
        /// other rows — deriving it would move the meaning of FATAL into the UI. A row of a blocked rack is a
        /// diagnostic: shown, never actionable.
        /// </para>
        /// <para>
        /// The stored literal of each row comes from THAT property's descriptor. A single hardcoded field would
        /// show the user the wrong number at exactly the moment they must decide a repair.
        /// </para>
        /// </summary>
        private static IReadOnlyList<BrokenBindingRow> FindBroken(
            IReadOnlyList<ProjectVariableScanEntry> entries, UsableProjectVariablesRegistry registry)
        {
            var rows = new List<BrokenBindingRow>();

            if (entries == null)
            {
                return rows;
            }

            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var descriptors = SelectiveLinkedProperties.All;

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

                var assessment = SelectiveLinkedPropertyKernel.Assess(entry.Authored, descriptors, registry);

                if (assessment.Outcome == RackRepairability.Healthy)
                {
                    continue;
                }

                foreach (var inspection in assessment.Inspections)
                {
                    if (inspection.IsHealthy)
                    {
                        continue;
                    }

                    // The sibling views of a rack are the SAME row, not three.
                    if (!seen.Add(entry.RackId + "|" + (inspection.PropertyToken ?? string.Empty)))
                    {
                        continue;
                    }

                    var literal = descriptors.TryGetDescriptor(inspection.PropertyId, out var descriptor)
                        ? descriptor.ReadAuthored(entry.Authored)
                        : 0.0;

                    rows.Add(new BrokenBindingRow(
                        entry.RackId,
                        entry.Authored.Name,
                        inspection.PropertyToken,
                        inspection.RawVariableId,
                        literal,
                        inspection.Detail,
                        assessment.CanRepair,
                        assessment.CanRepair ? null : assessment.BlockingReason));
                }
            }

            return rows;
        }
    }
}
