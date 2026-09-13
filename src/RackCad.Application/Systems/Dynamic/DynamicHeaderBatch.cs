using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Dynamic;

namespace RackCad.Application.Systems.Dynamic
{
    /// <summary>
    /// I-53 (ID6 REUSE + ID7 BATCH DISTRIBUTION) — the protocol of the Dinamico over its own resolved system: PREPARE, MUTATE
    /// and the one RECOMPUTE (contract §3.3-§3.10, §7.4-§7.8; ADR-0037).
    /// <para>
    /// PREPARE decides everything and changes nothing observable. It checks that the request was taken on the sequence in
    /// force, resolves the source address and captures its CURRENT value, resolves <see cref="DynamicModuleTargets"/> against
    /// the sequence in Index order, omits what is not drawn and the source itself, rejects in the precedence of §3.7,
    /// validates every applicable destination and materializes one copy per destination. It does not normalize: the fondo and
    /// the peralte of a cabecera are the builder's recompute to impose, and imposing them here would be a second authority.
    /// </para>
    /// <para>
    /// MUTATE verifies, before the first write, that nothing it relies on moved —the same state, the same resolved system and
    /// the same signature— and then only assigns: each target receives its prepared copy and becomes custom. Length,
    /// <c>IsManualOverride</c>, <c>IsCalculated</c>, kind and line overrides are not touched, except for the manual length of an
    /// EDIT, precomputed by the editor's existing rule. The RECOMPUTE is the builder's own, run once after a commit and never
    /// after a rejection.
    /// </para>
    /// <para>
    /// No session, no generic executor and no UI: <c>RackModuleEditSession</c> is not used (AM-2), and the window is wired in
    /// G7.
    /// </para>
    /// </summary>
    public static class DynamicHeaderBatch
    {
        /// <summary>Inches under which an edited fondo equals the module length: the editor's historical tolerance.</summary>
        private const double LengthTolerance = 0.0001;

        /// <summary>
        /// The signature of a sequence (contract §7.6): the rebuild generation and, in Index order, every module's
        /// <c>ModuleId:Kind</c>. Values only, never a live object: two resolved systems with the same sequence and generation
        /// sign alike, which is why a recomposition without rebuild keeps a remembered source or target set valid, while a
        /// kind change or a rebuild does not.
        /// </summary>
        public static HeaderBatchSignature SequenceSignature(DynamicRackSystem system, long generation)
        {
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            return new HeaderBatchSignature(SequenceComponents(system, generation));
        }

        /// <summary>
        /// PREPARE (contract §3.3, §7.8): pure, and the only phase that can refuse. With no resolved system the gesture ends
        /// before PREPARE (<see cref="DynamicHeaderPreconditionFailure.NoResolvedSystem"/>); otherwise the answer is a rejected
        /// or a prepared plan, with the copies of a prepared one aligned to its targets.
        /// </summary>
        public static DynamicHeaderBatchPreparation Prepare(
            DynamicRackSystem system, DynamicHeaderBatchState state, DynamicHeaderBatchRequest request)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var operation = request.Operation;
            if (system == null)
            {
                return DynamicHeaderBatchPreparation.Ended(state, operation, DynamicHeaderPreconditionFailure.NoResolvedSystem);
            }

            var sequence = new Sequence(system);
            var current = SequenceSignature(system, state.Generation);

            // 0. The request must have been taken on the sequence in force. Ids are positional, so a stale request is not
            // evaluated piecewise: its ids may name other modules now (§3.7, §7.6).
            if ((operation == DynamicHeaderBatchOperation.Distribute && request.Source.Signature != current)
                || (request.TargetMode == DynamicModuleTargetMode.Explicit && request.TargetSignature != current))
            {
                return Reject(state, system, operation, HeaderRejectionCode.StaleTargets);
            }

            // 1-2. The source: the CURRENT value its address designates, or the configurator result of an EDIT.
            DynamicRackModule source = null;
            RackFrameConfiguration sourceConfiguration;
            if (operation == DynamicHeaderBatchOperation.Distribute)
            {
                source = sequence.ById(request.Source.Address.ModuleId);
                if (source == null || !source.IsHeader)
                {
                    return Reject(state, system, operation, HeaderRejectionCode.SourceNotFound);
                }

                if (!sequence.IsPhysicallyPresent(source)
                    || source.UseCalculatedHeaderConfiguration
                    || source.AssociatedFrameConfiguration == null)
                {
                    return Reject(state, system, operation, HeaderRejectionCode.SourceUnusable);
                }

                sourceConfiguration = source.AssociatedFrameConfiguration;
            }
            else
            {
                sourceConfiguration = request.EditResult;
            }

            if (!(HeaderConfigurationSnapshot.TryCapture(sourceConfiguration) is HeaderConfigurationCapture.Captured captured))
            {
                return Reject(state, system, operation, HeaderRejectionCode.SourceUnusable);
            }

            // 3. The universe of the targets, against the sequence in force.
            var universe = new List<DynamicRackModule>();
            switch (request.TargetMode)
            {
                case DynamicModuleTargetMode.All:
                    foreach (var module in sequence.Modules.Where(candidate => candidate.IsHeader))
                    {
                        if (!ReferenceEquals(sequence.ById(module.ModuleId), module))
                        {
                            // An id that designates no single module is not an address.
                            return Reject(state, system, operation, HeaderRejectionCode.MalformedTarget);
                        }

                        universe.Add(module);
                    }

                    if (universe.Count == 0)
                    {
                        return Reject(state, system, operation, HeaderRejectionCode.NoTargets);
                    }

                    break;

                case DynamicModuleTargetMode.Explicit:
                    if (request.TargetModuleIds.Count == 0)
                    {
                        return Reject(state, system, operation, HeaderRejectionCode.NoTargets);
                    }

                    foreach (var id in request.TargetModuleIds)
                    {
                        var module = sequence.ById(id);
                        if (module == null || !module.IsHeader)
                        {
                            return Reject(state, system, operation, HeaderRejectionCode.MalformedTarget);
                        }

                        universe.Add(module);
                    }

                    break;

                default:
                    // «Actual»: a selection that is not a cabecera counts as no selection (§7.7).
                    var selected = sequence.ById(request.SelectedModuleId);
                    if (selected == null || !selected.IsHeader)
                    {
                        return Reject(state, system, operation, HeaderRejectionCode.NoTargets);
                    }

                    universe.Add(selected);
                    break;
            }

            // 4. Omissions keep their place in the Index order; nothing is created, clamped or re-aimed.
            var targets = new List<DynamicRackModule>();
            var omitted = new List<HeaderOmission<DynamicHeaderAddress>>();
            foreach (var module in universe.Distinct().OrderBy(candidate => candidate.Index))
            {
                var address = new DynamicHeaderAddress(module.ModuleId);
                if (!sequence.IsPhysicallyPresent(module))
                {
                    omitted.Add(new HeaderOmission<DynamicHeaderAddress>(address, HeaderOmissionReason.NotPhysicallyPresent));
                }
                else if (ReferenceEquals(module, source))
                {
                    omitted.Add(new HeaderOmission<DynamicHeaderAddress>(address, HeaderOmissionReason.IsSource));
                }
                else
                {
                    targets.Add(module);
                }
            }

            if (targets.Count == 0)
            {
                return Reject(state, system, operation, HeaderRejectionCode.NoApplicableTargets);
            }

            // 5. Every applicable destination is validated BEFORE copying: one invalid length rejects the whole batch.
            foreach (var target in targets)
            {
                if (!IsValidLength(target.Length))
                {
                    return Reject(state, system, operation, HeaderRejectionCode.DestinationInvalid);
                }
            }

            // 6. One NEW materialization per destination, never the source or another destination's copy, and not normalized.
            // An EDIT precomputes the manual length the editor's rule gives it.
            var copies = new List<RackFrameConfiguration>(targets.Count);
            var editLengths = new List<double?>(targets.Count);
            foreach (var target in targets)
            {
                var copy = captured.Snapshot.Materialize();
                copies.Add(copy);
                editLengths.Add(operation == DynamicHeaderBatchOperation.Edit ? EditedLength(copy.Depth, target.Length) : null);
            }

            // 10. The plan, signed with what this PREPARE read.
            var addresses = targets.Select(target => new DynamicHeaderAddress(target.ModuleId)).ToList();
            var plan = new HeaderBatchPlan<DynamicHeaderAddress>.Prepared(
                addresses,
                omitted,
                Array.Empty<HeaderBatchWarning<DynamicHeaderAddress>>(),
                PlanSignature(system, state.Generation, addresses));
            return DynamicHeaderBatchPreparation.Prepared(
                state,
                system,
                operation,
                plan,
                new ReadOnlyCollection<RackFrameConfiguration>(copies),
                new ReadOnlyCollection<double?>(editLengths));
        }

        /// <summary>
        /// MUTATE and the one RECOMPUTE of a Dinamico cabecera batch (contract §3.4, §3.9, §3.10, §7.8).
        /// <para>
        /// The one verification comes before the first write: the plan must be applied to the resolved system it was
        /// prepared on (a plan does not cross a recompute) and that system must still sign as it did (no kind change, rebuild
        /// or validated length moved in between). Otherwise the answer is <see cref="HeaderRejectionCode.StaleTargets"/>,
        /// with zero writes and no recompute. Nothing is re-resolved or re-aimed here.
        /// </para>
        /// <para>
        /// Each target then receives ITS prepared copy, in the plan's order and as it is, and becomes custom
        /// (<c>UseCalculatedHeaderConfiguration = false</c>); an EDIT also writes its precomputed manual length. Finally the
        /// builder's canonical recompute runs once: <c>ApplyPostPeralte</c> with the rack's own peralte, then <c>Refresh</c>,
        /// which imposes each cabecera's fondo, rebuilds the derived model and lays out the positions.
        /// </para>
        /// <para>
        /// A rejected plan returns its own rejection and writes nothing. Applying a preparation that has no plan, one prepared
        /// with another state, or one already used is a defect and throws.
        /// </para>
        /// </summary>
        public static HeaderBatchOutcome<DynamicHeaderAddress> Apply(
            DynamicHeaderBatchPreparation preparation,
            DynamicRackSystem system,
            DynamicHeaderBatchState state,
            DynamicRackSystemBuilder builder)
        {
            if (preparation == null)
            {
                throw new ArgumentNullException(nameof(preparation));
            }

            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (!ReferenceEquals(preparation.State, state))
            {
                throw new InvalidOperationException("El plan se preparo con otro estado del editor.");
            }

            preparation.Consume();

            if (preparation.Plan is HeaderBatchPlan<DynamicHeaderAddress>.Rejected rejected)
            {
                return new HeaderBatchOutcome<DynamicHeaderAddress>.Rejected(rejected.Code);
            }

            if (!(preparation.Plan is HeaderBatchPlan<DynamicHeaderAddress>.Prepared prepared))
            {
                throw new InvalidOperationException("El gesto termino antes de PREPARE: no hay plan que aplicar.");
            }

            // The only verification of MUTATE, before the first write.
            if (system == null
                || !ReferenceEquals(system, preparation.System)
                || PlanSignature(system, state.Generation, prepared.Targets) != prepared.Signature)
            {
                return new HeaderBatchOutcome<DynamicHeaderAddress>.Rejected(HeaderRejectionCode.StaleTargets);
            }

            var sequence = new Sequence(system);
            for (var i = 0; i < prepared.Targets.Count; i++)
            {
                var module = sequence.ById(prepared.Targets[i].ModuleId);
                module.AssociatedFrameConfiguration = preparation.PreparedCopies[i];
                module.UseCalculatedHeaderConfiguration = false;

                if (preparation.EditLengths[i] is double length)
                {
                    module.Length = length;
                    module.IsManualOverride = true;
                    module.IsCalculated = false;
                }
            }

            // RECOMPUTE: once, after the commit, by the builder's own normalization.
            builder.ApplyPostPeralte(system, system.PostPeralte);
            builder.Refresh(system);

            return new HeaderBatchOutcome<DynamicHeaderAddress>.Committed(prepared);
        }

        /// <summary>
        /// The signature of a prepared plan (contract §3.9): the sequence signature plus the length of every destination.
        /// PREPARE validated those lengths and an EDIT precomputed its manual length from them, and MUTATE validates nothing,
        /// so a length that moved must make the plan stale. Values only.
        /// </summary>
        internal static HeaderBatchSignature PlanSignature(
            DynamicRackSystem system, long generation, IEnumerable<DynamicHeaderAddress> targets)
        {
            var sequence = new Sequence(system);
            var components = SequenceComponents(system, generation);
            foreach (var target in targets)
            {
                var module = sequence.ById(target.ModuleId);
                components.Add(
                    "longitud[" + target.ModuleId + "]="
                    + (module == null ? "?" : module.Length.ToString("R", CultureInfo.InvariantCulture)));
            }

            return new HeaderBatchSignature(components);
        }

        private static List<string> SequenceComponents(DynamicRackSystem system, long generation)
        {
            var components = new List<string> { "generacion=" + generation.ToString(CultureInfo.InvariantCulture) };
            foreach (var module in system.Modules.Where(candidate => candidate != null).OrderBy(candidate => candidate.Index))
            {
                components.Add((module.ModuleId ?? string.Empty) + ":" + module.Kind.ToString());
            }

            return components;
        }

        /// <summary>A module length a cabecera can take: a finite, positive number of inches.</summary>
        private static bool IsValidLength(double length) => double.IsFinite(length) && length > 0.0;

        /// <summary>
        /// The editor's existing rule for an edited cabecera (ADR-0037): a fondo that differs from the module length by more
        /// than the tolerance becomes its manual length; otherwise the length and its flags stay as they are (null).
        /// </summary>
        private static double? EditedLength(double editedDepth, double length)
            => editedDepth > 0.0 && Math.Abs(editedDepth - length) > LengthTolerance ? editedDepth : (double?)null;

        private static DynamicHeaderBatchPreparation Reject(
            DynamicHeaderBatchState state, DynamicRackSystem system, DynamicHeaderBatchOperation operation, HeaderRejectionCode code)
            => DynamicHeaderBatchPreparation.Rejected(state, system, operation, code);

        /// <summary>
        /// A read-only view of a resolved system: its modules in Index order, the module an id designates —none when the id
        /// is empty, unknown or shared by several modules— and physical presence, read from the existing authority.
        /// </summary>
        private sealed class Sequence
        {
            private readonly DynamicRackSystem system;
            private readonly Dictionary<string, DynamicRackModule> byId = new Dictionary<string, DynamicRackModule>(StringComparer.Ordinal);
            private HashSet<DynamicRackModule> present;

            internal Sequence(DynamicRackSystem system)
            {
                this.system = system;
                Modules = system.Modules.Where(candidate => candidate != null).OrderBy(candidate => candidate.Index).ToList();
                foreach (var module in Modules)
                {
                    var id = module.ModuleId ?? string.Empty;
                    byId[id] = byId.ContainsKey(id) ? null : module;
                }
            }

            internal IReadOnlyList<DynamicRackModule> Modules { get; }

            internal DynamicRackModule ById(string moduleId)
                => !string.IsNullOrWhiteSpace(moduleId) && byId.TryGetValue(moduleId, out var module) ? module : null;

            /// <summary>
            /// <see cref="RackModuleDescriptor.Describe(DynamicRackSystem)"/> decides: a module is drawn when a post that I-33
            /// builds covers its position, so the editor and the drawing cannot disagree. Read once, never mutated.
            /// </summary>
            internal bool IsPhysicallyPresent(DynamicRackModule module)
            {
                if (present == null)
                {
                    present = new HashSet<DynamicRackModule>();
                    var modules = system.Modules.Where(candidate => candidate != null).ToList();
                    var descriptors = RackModuleDescriptor.Describe(system);
                    for (var i = 0; i < descriptors.Count; i++)
                    {
                        if (descriptors[i].IsPhysicallyPresent)
                        {
                            present.Add(modules[i]);
                        }
                    }
                }

                return present.Contains(module);
            }
        }
    }
}
