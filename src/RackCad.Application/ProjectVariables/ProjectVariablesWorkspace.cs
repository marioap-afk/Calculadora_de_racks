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
    /// One binding a repair would remove: the property, the reference it carries RAW, and the literal that
    /// governs once the binding is gone.
    /// </summary>
    public sealed class RepairBinding
    {
        public RepairBinding(string propertyId, string variableId, double storedLiteral)
        {
            PropertyId = propertyId;
            VariableId = variableId;
            StoredLiteral = storedLiteral;
        }

        public string PropertyId { get; }

        /// <summary>The variable the reference names, RAW: unreadable is not the same as absent.</summary>
        public string VariableId { get; }

        /// <summary>The frozen authored literal — what governs once this binding is removed.</summary>
        public double StoredLiteral { get; }
    }

    /// <summary>
    /// The COMPLETE set a repair of one rack would apply — the contract calls it B (I-48 G4B.1).
    ///
    /// <para>
    /// It exists because the operation is rack-scoped and the list is property-scoped. Repairing from a
    /// selected row removes EVERY repairable broken binding of that rack, so a surface that only showed the
    /// selected row would collect a consent narrower than the action. Application decides B, from the one
    /// assessment it already ran; a surface must never rebuild it by grouping rows on its own, because that
    /// would be a second answer to "what does this repair touch".
    /// </para>
    /// <para>
    /// It exists ONLY for a rack that can actually be repaired. A rack carrying any fatal state has no batch
    /// at all, which is what makes <see cref="BrokenBindingRow.RackCanRepair"/> unable to disagree with it.
    /// </para>
    /// </summary>
    public sealed class RackRepairBatch
    {
        public RackRepairBatch(string rackId, string rackName, IReadOnlyList<RepairBinding> bindings)
        {
            RackId = rackId;
            RackName = rackName;
            Bindings = bindings ?? new RepairBinding[0];
        }

        public string RackId { get; }

        public string RackName { get; }

        /// <summary>Every binding the repair removes. All of them, never a subset.</summary>
        public IReadOnlyList<RepairBinding> Bindings { get; }

        public int Count => Bindings.Count;
    }

    /// <summary>
    /// A rack whose views do NOT establish a single authored state, so nothing about it can be presented as
    /// authority (I-48 G4B.1).
    ///
    /// <para>
    /// Divergent siblings, or one sibling whose design cannot be read, leave the rack without an authored
    /// authority. There is no safe way to pick one — not the frontal, not the first, not the majority — so the
    /// rack contributes NO repair row: showing one would mean showing a <c>VariableId</c> and a stored literal
    /// taken from a sibling chosen at random, at exactly the moment the user decides a repair.
    /// </para>
    /// <para>
    /// It does not block the register either. An unrelated broken rack must not stop every variable operation
    /// in the drawing — that asymmetry is the inherited one, and it is deliberate. What it does is stay VISIBLE.
    /// </para>
    /// </summary>
    public sealed class RackWithoutAuthority
    {
        public RackWithoutAuthority(string rackId, string reason)
        {
            RackId = rackId;
            Reason = reason;
        }

        public string RackId { get; }

        /// <summary>Why the rack has no authority, in the words of the inherited authority itself.</summary>
        public string Reason { get; }
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
            RackRepairBatch repairBatch,
            string rackBlockingReason)
        {
            RackId = rackId;
            RackName = rackName;
            PropertyId = propertyId;
            VariableId = variableId;
            StoredLiteral = storedLiteral;
            Detail = detail;
            RepairBatch = repairBatch;
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
        /// The COMPLETE set a repair from this row would remove, or null when the rack cannot be repaired.
        /// It is the same batch for every row of the rack: the unit of the operation is the RACK.
        /// </summary>
        public RackRepairBatch RepairBatch { get; }

        /// <summary>
        /// Whether the RACK this row belongs to can be repaired. Application decides it; a surface must NOT
        /// derive it by looking at the other rows, because that would put the meaning of FATAL in the UI.
        ///
        /// <para>
        /// It is the presence of a batch and not a parallel flag, so "can repair" and "what the repair touches"
        /// cannot drift apart. False means this row is a DIAGNOSTIC: still worth showing, never an actionable
        /// repair. Repair is rack-scoped, so one fatal state anywhere in the rack makes every row unactionable.
        /// </para>
        /// </summary>
        public bool RackCanRepair => RepairBatch != null;

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
        private static readonly RackWithoutAuthority[] NoUnresolvable = new RackWithoutAuthority[0];

        private ProjectVariablesWorkspace(
            ProjectVariablesWorkspaceState state,
            IReadOnlyList<ProjectVariableRow> variables,
            IReadOnlyList<BrokenBindingRow> brokenBindings,
            IReadOnlyList<RackWithoutAuthority> unresolvableRacks,
            string error)
        {
            State = state;
            Variables = variables;
            BrokenBindings = brokenBindings;
            UnresolvableRacks = unresolvableRacks;
            Error = error;
        }

        public ProjectVariablesWorkspaceState State { get; }

        public IReadOnlyList<ProjectVariableRow> Variables { get; }

        public IReadOnlyList<BrokenBindingRow> BrokenBindings { get; }

        /// <summary>
        /// The racks that have no single authored authority. They produce NO repair row — nothing about them
        /// may be presented as authority — and they do not block the register.
        /// </summary>
        public IReadOnlyList<RackWithoutAuthority> UnresolvableRacks { get; }

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
            var repairs = FindBroken(present, usable);
            var broken = repairs.Rows;
            var unclassifiable = FirstPlacedUnclassifiable(present);

            if (unclassifiable != null)
            {
                return Blocked(unclassifiable, broken, repairs.Unresolvable);
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
                    return Blocked(discovery.Error, broken, repairs.Unresolvable);
                }

                rows.Add(new ProjectVariableRow(
                    target.VariableId,
                    target.Name,
                    target.VariableType,
                    target.LiteralValue,
                    ProjectVariableMutationPreflight.Summarize(discovery.Consumers, target.VariableId)));
            }

            return new ProjectVariablesWorkspace(
                ProjectVariablesWorkspaceState.Editable, rows, broken, repairs.Unresolvable, null);
        }

        private static ProjectVariablesWorkspace Blocked(
            string error,
            IReadOnlyList<BrokenBindingRow> broken = null,
            IReadOnlyList<RackWithoutAuthority> unresolvable = null)
            => new ProjectVariablesWorkspace(
                ProjectVariablesWorkspaceState.Blocked,
                NoVariables,
                broken ?? NoBroken,
                unresolvable ?? NoUnresolvable,
                error);

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

        /// <summary>What the repair area of the window shows: the rows, and the racks that have no authority.</summary>
        private sealed class RepairProjection
        {
            internal RepairProjection(
                IReadOnlyList<BrokenBindingRow> rows, IReadOnlyList<RackWithoutAuthority> unresolvable)
            {
                Rows = rows;
                Unresolvable = unresolvable;
            }

            internal IReadOnlyList<BrokenBindingRow> Rows { get; }

            internal IReadOnlyList<RackWithoutAuthority> Unresolvable { get; }
        }

        /// <summary>
        /// Every binding that does NOT resolve, projected from ONE authored authority per rack (I-48 G4B.1).
        ///
        /// <para>
        /// The rack is the unit, and that decides the shape of the whole function. Views are grouped first, the
        /// inherited authority is asked whether they agree, and only a <c>Single</c> answer is read. Walking the
        /// views one by one and de-duplicating afterwards looked equivalent and was not: the first sibling
        /// visited won, so with two siblings pointing at different variables the row presented one of them as
        /// the authority of the rack, and repairing would have applied a literal taken from whichever view the
        /// sweep happened to reach first.
        /// </para>
        /// <para>
        /// Divergent siblings, or one whose design cannot be read, mean there is NO authority: the rack
        /// contributes no row, is reported as <see cref="RackWithoutAuthority"/>, and can never yield an
        /// actionable repair. Filtering the unreadable sibling to carry on with the readable ones is precisely
        /// what makes a partial answer look complete.
        /// </para>
        /// <para>
        /// It also stopped needing to stop at the first failure of a whole-document resolve. That shortcut
        /// reported ONE row per rack, so a rack with two broken references surfaced one, and the second only
        /// appeared after the first was fixed.
        /// </para>
        /// <para>
        /// <b>Repairability is a property of the RACK.</b> Each row carries the rack's verdict and, when the
        /// rack can be repaired, the COMPLETE batch that repair would remove — both from the same single
        /// assessment. A surface never has to derive either by inspecting the other rows, because deriving them
        /// would move the meaning of FATAL, and the scope of the operation, into the UI.
        /// </para>
        /// <para>
        /// The stored literal of each row and of each batch entry comes from THAT property's descriptor. A
        /// single hardcoded field would show the user the wrong number at exactly the moment they must decide.
        /// </para>
        /// </summary>
        private static RepairProjection FindBroken(
            IReadOnlyList<ProjectVariableScanEntry> entries, UsableProjectVariablesRegistry registry)
        {
            var rows = new List<BrokenBindingRow>();
            var unresolvable = new List<RackWithoutAuthority>();
            var descriptors = SelectiveLinkedProperties.All;

            foreach (var group in ProjectVariableConsumerDiscovery.GroupSelectiveByRack(entries))
            {
                var authority = SelectiveAuthoredAuthority.Resolve(group.Key, group.Value);

                if (!authority.IsSingle)
                {
                    // No sibling is chosen, and nothing of this rack is presented as authority.
                    unresolvable.Add(new RackWithoutAuthority(group.Key, authority.Error));
                    continue;
                }

                var authored = authority.Authored;

                if (!authored.HasPropertyValues)
                {
                    continue;
                }

                var assessment = SelectiveLinkedPropertyKernel.Assess(authored, descriptors, registry);

                if (assessment.Outcome == RackRepairability.Healthy)
                {
                    continue;
                }

                // ONE batch per rack, from the SAME assessment that produced the verdict.
                var batch = assessment.CanRepair
                    ? BuildBatch(group.Key, authored, assessment, descriptors)
                    : null;

                foreach (var inspection in assessment.Inspections)
                {
                    if (inspection.IsHealthy)
                    {
                        continue;
                    }

                    rows.Add(new BrokenBindingRow(
                        group.Key,
                        authored.Name,
                        inspection.PropertyToken,
                        inspection.RawVariableId,
                        LiteralOf(inspection, authored, descriptors),
                        inspection.Detail,
                        batch,
                        assessment.CanRepair ? null : assessment.BlockingReason));
                }
            }

            return new RepairProjection(rows, unresolvable);
        }

        /// <summary>
        /// The COMPLETE set B a repair of this rack removes, in the deterministic order of the scan. It is
        /// built once and shared by every row of the rack, so no surface can present a narrower scope than the
        /// operation actually has.
        /// </summary>
        private static RackRepairBatch BuildBatch(
            string rackId,
            SelectivePalletDesignDocument authored,
            RackRepairabilityAssessment assessment,
            LinkedPropertyDescriptorSet descriptors)
        {
            var bindings = new List<RepairBinding>();

            foreach (var missing in assessment.Missing)
            {
                bindings.Add(new RepairBinding(
                    missing.PropertyToken,
                    missing.RawVariableId,
                    LiteralOf(missing, authored, descriptors)));
            }

            return new RackRepairBatch(rackId, authored.Name, bindings);
        }

        /// <summary>
        /// The authored literal of the inspected property, read through ITS descriptor. A property this build
        /// does not declare has no descriptor and therefore no literal to promise.
        /// </summary>
        private static double LiteralOf(
            BindingInspection inspection,
            SelectivePalletDesignDocument authored,
            LinkedPropertyDescriptorSet descriptors)
            => descriptors.TryGetDescriptor(inspection.PropertyId, out var descriptor)
                ? descriptor.ReadAuthored(authored)
                : 0.0;
    }
}
