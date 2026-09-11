using System;
using System.Collections.Generic;
using RackCad.Application.Persistence;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>Whether the racks an operation needs could be determined at all.</summary>
    public enum ConsumerDiscoveryOutcome
    {
        Success = 1,

        /// <summary>Something could not be determined safely. Nothing is planned and nothing is mutated.</summary>
        Abort = 2,
    }

    /// <summary>One rack an operation will act on, with the single authored authority it was proven to have.</summary>
    public sealed class ProjectVariableConsumer
    {
        public ProjectVariableConsumer(
            string rackId,
            SelectivePalletDesignDocument authored,
            IReadOnlyList<ProjectVariableScanEntry> siblings)
        {
            RackId = rackId;
            Authored = authored;
            Siblings = siblings;
        }

        public string RackId { get; }

        /// <summary>The ONE logical authored state of the rack — proven equal across every present sibling.</summary>
        public SelectivePalletDesignDocument Authored { get; }

        /// <summary>Every present view of the rack. The destinations a redraw would have to cover.</summary>
        public IReadOnlyList<ProjectVariableScanEntry> Siblings { get; }
    }

    /// <summary>The racks in scope, or the reason the question could not be answered.</summary>
    public sealed class ConsumerDiscoveryResult
    {
        private static readonly ProjectVariableConsumer[] None = new ProjectVariableConsumer[0];

        private ConsumerDiscoveryResult(
            ConsumerDiscoveryOutcome outcome,
            IReadOnlyList<ProjectVariableConsumer> consumers,
            string error)
        {
            Outcome = outcome;
            Consumers = consumers;
            Error = error;
        }

        public ConsumerDiscoveryOutcome Outcome { get; }

        /// <summary>The racks in scope. ALWAYS empty on abort — there is no partial answer.</summary>
        public IReadOnlyList<ProjectVariableConsumer> Consumers { get; }

        public string Error { get; }

        public bool IsSuccess => Outcome == ConsumerDiscoveryOutcome.Success;

        public static ConsumerDiscoveryResult Success(IReadOnlyList<ProjectVariableConsumer> consumers)
            => new ConsumerDiscoveryResult(ConsumerDiscoveryOutcome.Success, consumers ?? None, null);

        public static ConsumerDiscoveryResult Abort(string error)
            => new ConsumerDiscoveryResult(ConsumerDiscoveryOutcome.Abort, None, error);
    }

    /// <summary>
    /// Finds the racks an operation has to touch. PURE: it reasons over a projection of the sweep, never over
    /// the drawing.
    ///
    /// <para>
    /// There are TWO families and the asymmetry is not a convenience. In a <b>target-variable</b> operation
    /// the affected racks are DISCOVERED, so the probe runs FIRST: a rack whose views all say NEGATIVE is
    /// ignored without being asked for authored equality, because its divergence — real as it may be — is
    /// none of this variable's business, and demanding equality there would let an unrelated broken rack
    /// block every variable operation in the drawing.
    /// </para>
    /// <para>
    /// In a <b>target-rack</b> operation the user picked the rack, so the probe decides nothing. That matters
    /// because a rack that is not bound yet answers NEGATIVE on every view, and binding is precisely the
    /// operation applied to a rack that is not bound. Under the first family's rules it would be ignored, and
    /// <c>Link</c> could never link anything.
    /// </para>
    /// <para>
    /// Both families share one precondition that runs before everything else: a definition carrying RackCad
    /// data this build cannot interpret ABORTS. It is not negative, it is not a foreign rack, and it is not
    /// ignorable — because without its identity there is no way to prove it is not a sibling of a rack that
    /// IS in scope. Letting it through would produce a propagation that looks complete while one view keeps
    /// showing the old value.
    /// </para>
    /// </summary>
    public static class ProjectVariableConsumerDiscovery
    {
        /// <summary>
        /// FAMILY A — target-variable (change a value, find what a delete would block, unlink-all-and-delete).
        /// The scope is discovered, so the probe runs before any equality is demanded.
        /// </summary>
        public static ConsumerDiscoveryResult DiscoverConsumers(
            IReadOnlyList<ProjectVariableScanEntry> entries,
            VariableId target)
        {
            var unclassifiable = FirstUnclassifiable(entries);

            if (unclassifiable != null)
            {
                return ConsumerDiscoveryResult.Abort(unclassifiable);
            }

            var consumers = new List<ProjectVariableConsumer>();

            foreach (var group in GroupSelectiveByRack(entries))
            {
                var probes = new List<ConsumerProbeOutcome>();

                foreach (var sibling in group.Value)
                {
                    probes.Add(ProjectVariableConsumerProbe.Probe(sibling, target));
                }

                if (probes.Contains(ConsumerProbeOutcome.Indeterminate))
                {
                    return ConsumerDiscoveryResult.Abort(
                        "El rack " + group.Key + " tiene una vista que esta versión no puede interpretar, " +
                        "así que no se puede afirmar si usa la variable. La operación se cancela sin tocar nada.");
                }

                var positives = probes.FindAll(p => p == ConsumerProbeOutcome.Positive).Count;

                if (positives == 0)
                {
                    // Demonstrably unrelated to THIS variable: ignored, and no equality is demanded of it.
                    continue;
                }

                if (positives != probes.Count)
                {
                    return ConsumerDiscoveryResult.Abort(
                        "Las vistas del rack " + group.Key + " discrepan sobre el vínculo con esta variable: " +
                        "unas la usan y otras no. No hay respuesta correcta que no sea cancelar.");
                }

                var authority = SelectiveAuthoredAuthority.Resolve(group.Key, group.Value);

                if (!authority.IsSingle)
                {
                    return ConsumerDiscoveryResult.Abort(authority.Error);
                }

                consumers.Add(new ProjectVariableConsumer(group.Key, authority.Authored, group.Value));
            }

            return ConsumerDiscoveryResult.Success(consumers);
        }

        /// <summary>
        /// FAMILY B — target-rack (link, unlink, repair). The rack is in scope because the user picked it, so
        /// the probe is NOT used to decide whether to inspect it. A single authored authority is still required.
        /// </summary>
        public static ConsumerDiscoveryResult ResolveTargetRack(
            IReadOnlyList<ProjectVariableScanEntry> entries,
            string rackId)
        {
            var unclassifiable = FirstUnclassifiable(entries);

            if (unclassifiable != null)
            {
                return ConsumerDiscoveryResult.Abort(unclassifiable);
            }

            foreach (var group in GroupSelectiveByRack(entries))
            {
                if (!string.Equals(group.Key, rackId, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var authority = SelectiveAuthoredAuthority.Resolve(group.Key, group.Value);

                return authority.IsSingle
                    ? ConsumerDiscoveryResult.Success(
                        new[] { new ProjectVariableConsumer(group.Key, authority.Authored, group.Value) })
                    : ConsumerDiscoveryResult.Abort(authority.Error);
            }

            return ConsumerDiscoveryResult.Abort(
                "El rack " + rackId + " no tiene ninguna vista Selectivo presente en el dibujo.");
        }

        /// <summary>
        /// The drawing-level precondition, and it runs before filtering by kind and before grouping: a block
        /// whose envelope cannot be interpreted has no identity, so it cannot be shown to be unrelated.
        /// The diagnostic names the DEFINITION and says the rack could not be determined — a RackId is never
        /// invented.
        /// </summary>
        private static string FirstUnclassifiable(IReadOnlyList<ProjectVariableScanEntry> entries)
        {
            if (entries == null)
            {
                return null;
            }

            foreach (var entry in entries)
            {
                if (entry != null && !entry.OuterEnvelopeInterpretable)
                {
                    return "La definición de bloque '" + entry.DefinitionId + "' lleva datos de RackCad que " +
                           "esta versión no puede interpretar, así que su rack no se puede determinar. " +
                           "La operación se cancela: no hay forma de demostrar que esa vista no pertenezca a " +
                           "un rack afectado.";
                }
            }

            return null;
        }

        /// <summary>
        /// Selective views grouped by rack, preserving the sweep order so diagnostics are reproducible.
        ///
        /// <para>
        /// INTERNAL since I-48 G4B.1 so that the central window's repair list groups a rack exactly like every
        /// operation does. A second copy of this rule is how one surface starts calling three views a rack and
        /// another calls them three racks. Nothing unreadable is filtered here on purpose: the authority has to
        /// SEE an unreadable sibling to be able to say there is no authority.
        /// </para>
        /// </summary>
        internal static List<KeyValuePair<string, List<ProjectVariableScanEntry>>> GroupSelectiveByRack(
            IReadOnlyList<ProjectVariableScanEntry> entries)
        {
            var order = new List<KeyValuePair<string, List<ProjectVariableScanEntry>>>();
            var index = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            if (entries == null)
            {
                return order;
            }

            foreach (var entry in entries)
            {
                if (entry == null || !entry.IsSelective || string.IsNullOrWhiteSpace(entry.RackId))
                {
                    continue;
                }

                if (!index.TryGetValue(entry.RackId, out var at))
                {
                    index[entry.RackId] = order.Count;
                    order.Add(new KeyValuePair<string, List<ProjectVariableScanEntry>>(
                        entry.RackId,
                        new List<ProjectVariableScanEntry> { entry }));
                    continue;
                }

                order[at].Value.Add(entry);
            }

            return order;
        }
    }
}
