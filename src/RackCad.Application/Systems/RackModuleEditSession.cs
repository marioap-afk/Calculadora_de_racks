using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>
    /// What a <see cref="RackModuleEditSession.Commit"/> accepted: the module intents plus the two restore requests
    /// that cannot be read off the intents themselves, since a restored module is indistinguishable from a module
    /// that was never customized.
    /// </summary>
    public sealed class RackModuleCommit
    {
        internal RackModuleCommit(
            IReadOnlyList<DynamicRackModuleDesign> modules,
            IReadOnlyList<string> restoredModuleIds,
            bool standardRestoreRequested)
        {
            Modules = modules;
            RestoredModuleIds = restoredModuleIds;
            StandardRestoreRequested = standardRestoreRequested;
        }

        /// <summary>The accepted module intents, independent of the session's own copies.</summary>
        public IReadOnlyList<DynamicRackModuleDesign> Modules { get; }

        /// <summary>Modules the user restored individually. A reconciliation must NOT carry their customization, and
        /// must report them as restored rather than as lost.</summary>
        public IReadOnlyList<string> RestoredModuleIds { get; }

        /// <summary>The rack-wide "restaurar estándar": the caller passes it on as <c>forceRebuild</c>.</summary>
        public bool StandardRestoreRequested { get; }
    }

    /// <summary>
    /// A TRANSACTIONAL edit of a rack's longitudinal modules: stage changes, then <see cref="Commit"/> them or
    /// <see cref="Cancel"/> them wholesale (I-35).
    /// <para>
    /// The base has no such thing. <c>RackFrameConfiguratorWindow</c> receives the caller's
    /// <see cref="RackFrameConfiguration"/> BY REFERENCE, mutates it through its ViewModel and exposes neither
    /// Aceptar nor Cancelar, so closing it without wanting the change leaves the change applied; the dynamic editor
    /// serializes a snapshot before opening it, but only to DETECT whether anything changed — it never restores from
    /// it. This session gives an editor a place to put edits that the user has not confirmed yet, WITHOUT touching
    /// that shared configurator (Owner, I-35: confirm/cancel belongs to the session and to the Push Back surface).
    /// </para>
    /// <para>
    /// Every <see cref="RackFrameConfiguration"/> that crosses this boundary is copied with
    /// <see cref="RackFrameProjectStore.DeepCopy"/> — the single canonical clone (I-17), which alone re-attaches the
    /// runtime-only <see cref="RackFrameConfiguration.Exceptions"/> the persistence document does not carry. The
    /// session therefore never hands out, and never holds, an instance the caller can mutate behind its back.
    /// </para>
    /// <para>
    /// NEUTRAL: no <c>RackSystemKind</c>, no branch per system, no WPF and no AutoCAD. It edits the shared
    /// longitudinal sequence, which is a property of a rack and not of a system family — there is no per-front or
    /// per-post module to edit.
    /// </para>
    /// </summary>
    public sealed class RackModuleEditSession
    {
        private readonly RackFrameProjectStore clone = new RackFrameProjectStore();
        private readonly List<string> restored = new List<string>();
        private List<DynamicRackModuleDesign> committed;
        private List<DynamicRackModuleDesign> working;
        private bool standardRestoreRequested;

        private RackModuleEditSession(IEnumerable<DynamicRackModuleDesign> baseline)
        {
            committed = baseline.Select(CopyIntent).ToList();
            working = committed.Select(CopyIntent).ToList();
        }

        /// <summary>Open a session over the modules of a resolved system. The system is READ, never captured.</summary>
        public static RackModuleEditSession Begin(DynamicRackSystem system)
        {
            if (system == null) throw new ArgumentNullException(nameof(system));
            return new RackModuleEditSession(system.Modules.Where(module => module != null).Select(ToIntent));
        }

        /// <summary>Open a session over module intents (a loaded design). The inputs are READ, never captured.</summary>
        public static RackModuleEditSession Begin(IEnumerable<DynamicRackModuleDesign> designs)
        {
            if (designs == null) throw new ArgumentNullException(nameof(designs));
            return new RackModuleEditSession(designs.Where(design => design != null));
        }

        /// <summary>The staged state, as read-only descriptors. This is what an editor renders.</summary>
        public IReadOnlyList<RackModuleDescriptor> Modules => RackModuleDescriptor.Describe(working);

        /// <summary>True when the staged state differs from the last committed one.</summary>
        public bool HasPendingChanges => !AreEquivalent(committed, working) || restored.Count > 0;

        /// <summary>Modules restored individually since the last commit, in the order they were restored.</summary>
        public IReadOnlyList<string> RestoredModuleIds => restored.ToList();

        /// <summary>True when this module has an individual restore staged.</summary>
        public bool IsRestored(string moduleId) => restored.Contains(moduleId);

        /// <summary>
        /// True when the user asked for the standard layout back. It is an INTENT, not an action: the session does
        /// not rebuild anything — the caller passes it on as the <c>forceRebuild</c> the assembler already accepts.
        /// </summary>
        public bool StandardRestoreRequested => standardRestoreRequested;

        /// <summary>
        /// Stage a new longitudinal length for one module — a cabecera's FONDO or the run a separator physically
        /// consumes. Both kinds are editable because both consume length; a separator has nothing else to edit.
        /// </summary>
        public bool SetLength(string moduleId, double length)
        {
            if (length <= 0.0)
            {
                return false;
            }

            var module = Find(moduleId);
            if (module == null)
            {
                return false;
            }

            module.Length = length;
            module.IsManualOverride = true;
            module.IsCalculated = false;
            restored.Remove(moduleId);   // editing it again is no longer a restore
            return true;
        }

        /// <summary>
        /// Stage a custom cabecera for a HEADER module: an independent canonical copy, with the provenance flag
        /// flipped so a later recompute must preserve it instead of regenerating a standard one.
        /// </summary>
        public bool SetHeaderConfiguration(string moduleId, RackFrameConfiguration configuration)
        {
            if (configuration == null)
            {
                return false;
            }

            var module = Find(moduleId);
            if (module == null || !module.IsHeader)
            {
                return false;
            }

            module.HeaderConfiguration = clone.DeepCopy(configuration);
            module.UseCalculatedHeaderConfiguration = false;
            module.IsManualOverride = true;
            restored.Remove(moduleId);   // editing it again is no longer a restore
            return true;
        }

        /// <summary>
        /// Stage the module back to a CALCULATED cabecera: the provenance returns to derived and the configuration
        /// is dropped, so whoever resolves next builds the standard one at the current inputs. This touches the
        /// CONFIGURATION only and leaves a manual length in place — the full per-module reset is
        /// <see cref="RestoreModule"/>.
        /// </summary>
        public bool ResetHeaderToCalculated(string moduleId)
        {
            var module = Find(moduleId);
            if (module == null || !module.IsHeader)
            {
                return false;
            }

            module.HeaderConfiguration = null;
            module.UseCalculatedHeaderConfiguration = true;
            return true;
        }

        /// <summary>
        /// FULL individual restore of ANY module — cabecera or separator: the length override AND, for a cabecera,
        /// the custom configuration and its provenance all go back to calculated, so the next recompute rebuilds the
        /// module from the current inputs.
        /// <para>
        /// The module is recorded in <see cref="RestoredModuleIds"/> because once restored it looks exactly like a
        /// module that was never customized, and the difference matters downstream: a reconciliation must report a
        /// restore as a restore and not as a loss (Owner, I-35: the only ordinary ways to lose a customization are
        /// asking for it and a structural incompatibility). The session does NOT know the calculated length — that
        /// comes from the rebuild — so recomputing it is left to the assembler.
        /// </para>
        /// </summary>
        public bool RestoreModule(string moduleId)
        {
            var module = Find(moduleId);
            if (module == null)
            {
                return false;
            }

            module.HeaderConfiguration = null;
            module.UseCalculatedHeaderConfiguration = true;
            module.IsManualOverride = false;
            module.IsCalculated = true;

            if (!restored.Contains(module.ModuleId))
            {
                restored.Add(module.ModuleId);
            }

            return true;
        }

        /// <summary>An independent canonical copy of a staged cabecera, for an editor to hand to a configurator
        /// without exposing the session's own instance. Null when the module has none.</summary>
        public RackFrameConfiguration HeaderConfigurationCopy(string moduleId)
        {
            var module = Find(moduleId);
            return module?.HeaderConfiguration == null ? null : clone.DeepCopy(module.HeaderConfiguration);
        }

        /// <summary>Ask for the standard layout back. Cleared by <see cref="Cancel"/> and by <see cref="Commit"/>.</summary>
        public void RequestStandardRestore() => standardRestoreRequested = true;

        /// <summary>Discard every staged change, every individual restore and the rack-wide restore request; the
        /// session returns to its last committed state. This is the rollback the base does not have anywhere.</summary>
        public void Cancel()
        {
            working = committed.Select(CopyIntent).ToList();
            standardRestoreRequested = false;
            restored.Clear();
        }

        /// <summary>
        /// Accept the staged state and hand it over as independent intents plus the restore requests. The session
        /// re-baselines on the result, so a later <see cref="Cancel"/> reverts to THIS state and not to the original.
        /// </summary>
        public RackModuleCommit Commit()
        {
            committed = working.Select(CopyIntent).ToList();
            var result = new RackModuleCommit(
                committed.Select(CopyIntent).ToList(),
                restored.ToList(),
                standardRestoreRequested);

            standardRestoreRequested = false;
            restored.Clear();
            return result;
        }

        private DynamicRackModuleDesign Find(string moduleId)
            => string.IsNullOrWhiteSpace(moduleId)
                ? null
                : working.FirstOrDefault(module => string.Equals(module.ModuleId, moduleId, StringComparison.Ordinal));

        private static DynamicRackModuleDesign ToIntent(DynamicRackModule module)
            => new DynamicRackModuleDesign
            {
                ModuleId = module.ModuleId,
                Kind = module.Kind,
                Length = module.Length,
                IsCalculated = module.IsCalculated,
                IsManualOverride = module.IsManualOverride,
                UseCalculatedHeaderConfiguration = module.UseCalculatedHeaderConfiguration,
                HeaderConfiguration = module.AssociatedFrameConfiguration,
                Notes = module.Notes
            };

        private DynamicRackModuleDesign CopyIntent(DynamicRackModuleDesign source)
            => new DynamicRackModuleDesign
            {
                ModuleId = source.ModuleId,
                Kind = source.Kind,
                Length = source.Length,
                IsCalculated = source.IsCalculated,
                IsManualOverride = source.IsManualOverride,
                UseCalculatedHeaderConfiguration = source.UseCalculatedHeaderConfiguration,
                HeaderConfiguration = clone.DeepCopy(source.HeaderConfiguration),
                Notes = source.Notes
            };

        /// <summary>
        /// Whether two staged states are the same EDIT. Compares the module intent plus the persisted shape of each
        /// cabecera through the same store the clone uses, so a configuration edited and then undone by hand reads
        /// as no change — the property the dynamic editor approximates today with its own serialized snapshot.
        /// </summary>
        private bool AreEquivalent(
            IReadOnlyList<DynamicRackModuleDesign> left,
            IReadOnlyList<DynamicRackModuleDesign> right)
        {
            if (left.Count != right.Count)
            {
                return false;
            }

            for (var index = 0; index < left.Count; index++)
            {
                var a = left[index];
                var b = right[index];

                if (!string.Equals(a.ModuleId, b.ModuleId, StringComparison.Ordinal)
                    || a.Kind != b.Kind
                    || a.Length != b.Length
                    || a.IsCalculated != b.IsCalculated
                    || a.IsManualOverride != b.IsManualOverride
                    || a.UseCalculatedHeaderConfiguration != b.UseCalculatedHeaderConfiguration
                    || !string.Equals(a.Notes, b.Notes, StringComparison.Ordinal)
                    || !string.Equals(Persisted(a.HeaderConfiguration), Persisted(b.HeaderConfiguration), StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        private string Persisted(RackFrameConfiguration configuration)
            => configuration == null ? string.Empty : clone.Serialize(configuration);
    }
}
