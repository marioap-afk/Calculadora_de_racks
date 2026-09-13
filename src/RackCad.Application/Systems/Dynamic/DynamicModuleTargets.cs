using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Dynamic;

namespace RackCad.Application.Systems.Dynamic
{
    /// <summary>How the destination modules of a Dinamico distribution were chosen (I-53, contract §7.5).</summary>
    public enum DynamicModuleTargetMode
    {
        /// <summary>«Actual»: the cabecera module selected in the editor when the batch is applied.</summary>
        FollowCurrent,

        /// <summary>«Explicito»: a deliberate set of modules, signed with the sequence it was chosen on.</summary>
        Explicit,

        /// <summary>«Todas»: every cabecera module of the sequence in force, expanded when the batch is prepared.</summary>
        All
    }

    /// <summary>
    /// The MODULE axis of a Dinamico cabecera distribution (I-53, contract §7.5-§7.7).
    /// <para>
    /// «Actual» follows the editor's selection, which every recomposition clears. «Todas» is a mode rather than a snapshot, so
    /// it re-expands against the sequence in force. An explicit set is kept AS CHOSEN —distinct ids, nothing dropped and
    /// nothing reordered— together with the signature of the sequence it was chosen on: whether each id is a cabecera, is
    /// drawn or is the source is decided when the batch is prepared, where a malformed id rejects the batch and the rest is
    /// ordered by the modules' Index. A rebuild invalidates the explicit set (<see cref="DynamicRackRebuild"/>).
    /// </para>
    /// <para>
    /// Runtime state of the editor and nothing else: not persisted, not part of the design and not remembered between
    /// sessions. A new instance opens on «Actual».
    /// </para>
    /// </summary>
    public sealed class DynamicModuleTargets
    {
        private IReadOnlyList<string> explicitModuleIds = Array.Empty<string>();

        public DynamicModuleTargetMode Mode { get; private set; } = DynamicModuleTargetMode.FollowCurrent;

        /// <summary>The explicit ids as chosen, distinct; empty unless <see cref="Mode"/> is Explicit.</summary>
        public IReadOnlyList<string> ExplicitModuleIds => explicitModuleIds;

        /// <summary>The signature of the sequence the explicit set was chosen on; null unless <see cref="Mode"/> is Explicit.</summary>
        public HeaderBatchSignature ExplicitSignature { get; private set; }

        /// <summary>Choose «Actual».</summary>
        public void FollowCurrentModule() => Follow(DynamicModuleTargetMode.FollowCurrent);

        /// <summary>Choose «Todas».</summary>
        public void FollowAllModules() => Follow(DynamicModuleTargetMode.All);

        /// <summary>
        /// Choose specific modules of <paramref name="system"/>, signed with its sequence at <paramref name="generation"/>. The
        /// set is kept as given: an empty one has no destinations, and an id that is not a cabecera of that sequence is
        /// malformed. Both are answered when the batch is prepared, never repaired here.
        /// </summary>
        public void SetTargetModules(IEnumerable<string> moduleIds, DynamicRackSystem system, long generation)
        {
            if (system == null)
            {
                throw new ArgumentNullException(nameof(system));
            }

            explicitModuleIds = Array.AsReadOnly(
                (moduleIds ?? Enumerable.Empty<string>())
                .Select(id => id ?? string.Empty)
                .Distinct(StringComparer.Ordinal)
                .ToArray());
            ExplicitSignature = DynamicHeaderBatch.SequenceSignature(system, generation);
            Mode = DynamicModuleTargetMode.Explicit;
        }

        /// <summary>
        /// A rebuild replaced the sequence: an explicit set named modules of a sequence that no longer exists, so it is
        /// dropped and the choice falls back to «Actual». «Actual» and «Todas» name no module and have nothing to drop.
        /// Returns whether an explicit set was dropped.
        /// </summary>
        internal bool Invalidate()
        {
            if (Mode != DynamicModuleTargetMode.Explicit)
            {
                return false;
            }

            FollowCurrentModule();
            return true;
        }

        private void Follow(DynamicModuleTargetMode mode)
        {
            Mode = mode;
            explicitModuleIds = Array.Empty<string>();
            ExplicitSignature = null;
        }
    }
}
