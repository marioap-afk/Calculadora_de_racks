using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Domain.RackFrames;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Systems.Shared
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
            bool standardRestoreRequested,
            IReadOnlyList<DynamicHeaderLineOverride> lineOverrides)
        {
            Modules = modules;
            RestoredModuleIds = restoredModuleIds;
            StandardRestoreRequested = standardRestoreRequested;
            LineOverrides = lineOverrides;
        }

        /// <summary>The accepted module intents, independent of the session's own copies.</summary>
        public IReadOnlyList<DynamicRackModuleDesign> Modules { get; }

        /// <summary>Modules the user restored individually. A reconciliation must NOT carry their customization, and
        /// must report them as restored rather than as lost.</summary>
        public IReadOnlyList<string> RestoredModuleIds { get; }

        /// <summary>The rack-wide "restaurar estándar": the caller passes it on as <c>forceRebuild</c>.</summary>
        public bool StandardRestoreRequested { get; }

        /// <summary>I-40: the accepted per-LINE cabecera configurations, independent of the session's own copies.</summary>
        public IReadOnlyList<DynamicHeaderLineOverride> LineOverrides { get; }
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

        /// <summary>I-40 — the per-LINE staged configurations, keyed by (line, module). Same transaction as the
        /// intents: staged until <see cref="Commit"/>, thrown away entirely by <see cref="Cancel"/>.</summary>
        private Dictionary<LineKey, RackFrameConfiguration> committedLines = new Dictionary<LineKey, RackFrameConfiguration>();
        private Dictionary<LineKey, RackFrameConfiguration> workingLines = new Dictionary<LineKey, RackFrameConfiguration>();

        private RackModuleEditSession(IEnumerable<DynamicRackModuleDesign> baseline)
        {
            committed = baseline.Select(CopyIntent).ToList();
            working = committed.Select(CopyIntent).ToList();
        }

        /// <summary>The identity of a physical cabecera: its LINE and its module.</summary>
        private readonly struct LineKey : IEquatable<LineKey>
        {
            public LineKey(int postIndex, string moduleId)
            {
                PostIndex = postIndex;
                ModuleId = moduleId ?? string.Empty;
            }

            public int PostIndex { get; }
            public string ModuleId { get; }

            public bool Equals(LineKey other)
                => PostIndex == other.PostIndex && string.Equals(ModuleId, other.ModuleId, StringComparison.Ordinal);

            public override bool Equals(object obj) => obj is LineKey other && Equals(other);

            public override int GetHashCode() => (PostIndex * 397) ^ ModuleId.GetHashCode();
        }

        /// <summary>Open a session over the modules of a resolved system. The system is READ, never captured.</summary>
        public static RackModuleEditSession Begin(DynamicRackSystem system)
        {
            if (system == null) throw new ArgumentNullException(nameof(system));
            var session = new RackModuleEditSession(
                system.Modules.Where(module => module != null).Select(ToIntent));

            // I-40: la sesion adopta tambien lo que el rack ya tiene por LINEA, para que reabrir muestre —y pueda
            // seguir editando— la configuracion real de cada linea y no solo la del modulo.
            foreach (var line in system.HeaderLineOverrides)
            {
                if (line?.Header != null && !string.IsNullOrWhiteSpace(line.ModuleId))
                {
                    var key = new LineKey(line.PostIndex, line.ModuleId);
                    session.committedLines[key] = session.clone.DeepCopy(line.Header);
                    session.workingLines[key] = session.clone.DeepCopy(line.Header);
                }
            }

            return session;
        }

        /// <summary>Open a session over module intents (a loaded design). The inputs are READ, never captured.</summary>
        public static RackModuleEditSession Begin(IEnumerable<DynamicRackModuleDesign> designs)
        {
            if (designs == null) throw new ArgumentNullException(nameof(designs));
            return new RackModuleEditSession(designs.Where(design => design != null));
        }

        /// <summary>The staged state, as read-only descriptors. This is what an editor renders.</summary>
        public IReadOnlyList<RackModuleDescriptor> Modules => RackModuleDescriptor.Describe(working);

        /// <summary>
        /// True when there is anything to confirm: staged intents that differ from the last committed ones, an
        /// individual restore, or the rack-wide standard restore. The last two are NOT visible in the intents —a
        /// restored module looks uncustomized and a standard restore changes nothing until it is applied— so reading
        /// only the intents would leave a confirmable action looking like nothing to confirm.
        /// </summary>
        public bool HasPendingChanges
            => !AreEquivalent(committed, working)
               || !AreEquivalentLines(committedLines, workingLines)
               || restored.Count > 0
               || standardRestoreRequested;

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
            => ApplyHeaderConfiguration(configuration, new[] { moduleId }).Applied;

        /// <summary>
        /// THE single conceptual operation behind both PBH-02 (apply to the selected cabecera or to all applicable
        /// ones) and PBH-03 (reuse another cabecera's configuration): validate every target, then hand each one its
        /// OWN independent canonical copy (I-40).
        /// <para>
        /// ATOMIC by construction: every target is checked BEFORE a single byte of the staged state moves, so a bad
        /// target leaves the session exactly as it was — there is no partial application to undo. The staged state
        /// is still just staged: only <see cref="Commit"/> hands it on, and <see cref="Cancel"/> throws the whole
        /// operation away with everything else the user had pending.
        /// </para>
        /// <para>
        /// Each target gets its OWN <see cref="RackFrameProjectStore.DeepCopy"/>, never a shared instance: editing
        /// one destination afterwards can therefore never reach the source or another destination. That
        /// independence is what makes PBH-03 a COPY and not a reference, and it is why nothing new is persisted —
        /// the copy lands in the module intent the design already carries.
        /// </para>
        /// </summary>
        /// <param name="configuration">The configuration to hand out. It is READ, never captured.</param>
        /// <param name="targetModuleIds">The modules to write. Duplicates collapse; the order is preserved.</param>
        public RackModuleHeaderApplyResult ApplyHeaderConfiguration(
            RackFrameConfiguration configuration,
            IEnumerable<string> targetModuleIds)
        {
            if (configuration == null)
            {
                return RackModuleHeaderApplyResult.Rejected("No hay una configuracion de cabecera que aplicar.");
            }

            var ids = (targetModuleIds ?? Enumerable.Empty<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (ids.Count == 0)
            {
                return RackModuleHeaderApplyResult.Rejected("No hay ninguna cabecera de destino.");
            }

            // ---- Validate EVERY target first. Nothing below this block may fail. ----
            var targets = new List<DynamicRackModuleDesign>(ids.Count);
            foreach (var id in ids)
            {
                var module = Find(id);
                if (module == null)
                {
                    return RackModuleHeaderApplyResult.Rejected(
                        "El modulo " + id + " ya no existe en este rack: no se aplico nada.");
                }

                if (!module.IsHeader)
                {
                    return RackModuleHeaderApplyResult.Rejected(
                        "El modulo " + id + " es un separador y no lleva cabecera: no se aplico nada.");
                }

                targets.Add(module);
            }

            // ---- Apply. ----
            foreach (var module in targets)
            {
                module.HeaderConfiguration = clone.DeepCopy(configuration);
                module.UseCalculatedHeaderConfiguration = false;
                module.IsManualOverride = true;
                restored.Remove(module.ModuleId);   // editing it again is no longer a restore
            }

            return RackModuleHeaderApplyResult.Success(targets.Select(module => module.ModuleId).ToList());
        }

        /// <summary>
        /// PBH-03: reuse the configuration of ANOTHER cabecera of this session on the given targets, as an
        /// independent copy. The source is read through <see cref="HeaderConfigurationCopy"/> — which already
        /// returns a copy — so the source module is never handed out and is never touched by a later edit of a
        /// destination. Nothing is stored anywhere: there is no header library and no persistent reference.
        /// </summary>
        public RackModuleHeaderApplyResult CopyHeaderConfiguration(
            string sourceModuleId,
            IEnumerable<string> targetModuleIds)
        {
            var source = Find(sourceModuleId);
            if (source == null)
            {
                return RackModuleHeaderApplyResult.Rejected(
                    "La cabecera de origen ya no existe en este rack: no se aplico nada.");
            }

            if (!source.IsHeader)
            {
                return RackModuleHeaderApplyResult.Rejected(
                    "El modulo de origen es un separador y no tiene cabecera que copiar: no se aplico nada.");
            }

            if (source.HeaderConfiguration == null)
            {
                return RackModuleHeaderApplyResult.Rejected(
                    "La cabecera de origen todavia no tiene una configuracion que copiar: no se aplico nada.");
            }

            return ApplyHeaderConfiguration(source.HeaderConfiguration, targetModuleIds);
        }

        /// <summary>The ids of every staged HEADER module, in longitudinal order. The set a surface offers as
        /// «origen» for PBH-03 and as the raw universe for PBH-02, before it removes the ones it knows are not
        /// applicable (physical presence lives on a resolved system, not here).</summary>
        public IReadOnlyList<string> HeaderModuleIds
            => working.Where(module => module.IsHeader).Select(module => module.ModuleId).ToList();

        /// <summary>
        /// I-40 — apply one configuration to a set of cabeceras OF ONE LINE. Same single operation as
        /// <see cref="ApplyHeaderConfiguration"/>: validate every target first, then hand each one its OWN
        /// independent canonical copy, so nothing is partially applied and no two destinations share an instance.
        /// <para>
        /// The difference is the ADDRESS. A module configuration is the cabecera on EVERY line; a line override is
        /// the cabecera on ONE line, and it wins where it exists. That is the distinction the model could not
        /// express before: one <c>DynamicRackModuleDesign</c> IS every instance of that cabecera.
        /// </para>
        /// </summary>
        public RackModuleHeaderApplyResult ApplyHeaderConfigurationToLine(
            RackFrameConfiguration configuration,
            int postIndex,
            IEnumerable<string> targetModuleIds)
        {
            if (configuration == null)
            {
                return RackModuleHeaderApplyResult.Rejected("No hay una configuracion de cabecera que aplicar.");
            }

            if (postIndex < 0)
            {
                return RackModuleHeaderApplyResult.Rejected("No hay una linea de cabeceras seleccionada.");
            }

            var ids = (targetModuleIds ?? Enumerable.Empty<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.Ordinal)
                .ToList();

            if (ids.Count == 0)
            {
                return RackModuleHeaderApplyResult.Rejected("No hay ninguna cabecera de destino en esta linea.");
            }

            // ---- Validate EVERY target first. Nothing below this block may fail. ----
            foreach (var id in ids)
            {
                var module = Find(id);
                if (module == null)
                {
                    return RackModuleHeaderApplyResult.Rejected(
                        "El modulo " + id + " ya no existe en este rack: no se aplico nada.");
                }

                if (!module.IsHeader)
                {
                    return RackModuleHeaderApplyResult.Rejected(
                        "El modulo " + id + " es un separador y no lleva cabecera: no se aplico nada.");
                }
            }

            // ---- Apply: una copia INDEPENDIENTE por destino. ----
            foreach (var id in ids)
            {
                workingLines[new LineKey(postIndex, id)] = clone.DeepCopy(configuration);
            }

            return RackModuleHeaderApplyResult.Success(ids);
        }

        /// <summary>
        /// The configuration a cabecera uses ON ONE LINE, as an independent copy: the line override when that line
        /// has one, otherwise the module's own. This is what an editor hands to the configurator and what «copiar
        /// de» reads, so what the user sees is what that physical cabecera actually draws.
        /// </summary>
        public RackFrameConfiguration HeaderConfigurationCopy(string moduleId, int postIndex)
        {
            if (postIndex >= 0
                && workingLines.TryGetValue(new LineKey(postIndex, moduleId), out var line)
                && line != null)
            {
                return clone.DeepCopy(line);
            }

            return HeaderConfigurationCopy(moduleId);
        }

        /// <summary>True when this cabecera has its OWN configuration on this line.</summary>
        public bool HasLineOverride(string moduleId, int postIndex)
            => postIndex >= 0 && workingLines.ContainsKey(new LineKey(postIndex, moduleId));

        /// <summary>The lines that carry at least one own cabecera, ascending.</summary>
        public IReadOnlyList<int> OverriddenLines
            => workingLines.Keys.Select(key => key.PostIndex).Distinct().OrderBy(line => line).ToList();

        /// <summary>Drop the per-line configurations of these modules, so they go back to the module's own. Applying
        /// to ALL cabeceras uses it: a rack made uniform again must not keep line exceptions alive.</summary>
        public void ClearLineOverrides(IEnumerable<string> moduleIds)
        {
            var ids = new HashSet<string>(
                (moduleIds ?? Enumerable.Empty<string>()).Where(id => !string.IsNullOrWhiteSpace(id)),
                StringComparer.Ordinal);

            foreach (var key in workingLines.Keys.Where(key => ids.Contains(key.ModuleId)).ToList())
            {
                workingLines.Remove(key);
            }
        }

        /// <summary>
        /// The ids of the cabeceras that carry the USER'S OWN configuration, in longitudinal order — the only ones
        /// that can be an ORIGIN for PBH-03.
        /// <para>
        /// Every cabecera holds a configuration, calculated ones included, so offering all of them as a source lets
        /// «copiar mi configuracion» hand out a standard cabecera without saying so — the destructive surprise the
        /// Owner hit in round 2. A source has to be a personalization that actually exists.
        /// </para>
        /// </summary>
        public IReadOnlyList<string> CustomHeaderModuleIds
            => working
                .Where(module => module.IsHeader
                                 && !module.UseCalculatedHeaderConfiguration
                                 && module.HeaderConfiguration != null)
                .Select(module => module.ModuleId)
                .ToList();

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
            workingLines = CopyLines(committedLines);
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
            committedLines = CopyLines(workingLines);
            var result = new RackModuleCommit(
                committed.Select(CopyIntent).ToList(),
                restored.ToList(),
                standardRestoreRequested,
                committedLines
                    .OrderBy(entry => entry.Key.PostIndex)
                    .ThenBy(entry => entry.Key.ModuleId, StringComparer.Ordinal)
                    .Select(entry => new DynamicHeaderLineOverride
                    {
                        PostIndex = entry.Key.PostIndex,
                        ModuleId = entry.Key.ModuleId,
                        Header = clone.DeepCopy(entry.Value)
                    })
                    .ToList());

            standardRestoreRequested = false;
            restored.Clear();
            return result;
        }

        private Dictionary<LineKey, RackFrameConfiguration> CopyLines(
            Dictionary<LineKey, RackFrameConfiguration> source)
        {
            var result = new Dictionary<LineKey, RackFrameConfiguration>();
            foreach (var entry in source)
            {
                result[entry.Key] = clone.DeepCopy(entry.Value);
            }

            return result;
        }

        /// <summary>Whether two staged line states are the same EDIT, compared through the persisted shape exactly
        /// like the intents are.</summary>
        private bool AreEquivalentLines(
            Dictionary<LineKey, RackFrameConfiguration> left,
            Dictionary<LineKey, RackFrameConfiguration> right)
        {
            if (left.Count != right.Count)
            {
                return false;
            }

            foreach (var entry in left)
            {
                if (!right.TryGetValue(entry.Key, out var other)
                    || !string.Equals(Persisted(entry.Value), Persisted(other), StringComparison.Ordinal))
                {
                    return false;
                }
            }

            return true;
        }

        private DynamicRackModuleDesign Find(string moduleId)
            => string.IsNullOrWhiteSpace(moduleId)
                ? null
                : working.FirstOrDefault(module => string.Equals(module.ModuleId, moduleId, StringComparison.Ordinal));

        /// <summary>
        /// THE INVARIANT of this session (I-40, ronda 3): a module that declares a CUSTOM cabecera must carry one.
        /// «Personalizada» with no configuration is the hybrid state that made the editor label a header custom
        /// while the configurator showed the standard values — and made a copy of it propagate the standard. A
        /// module with no configuration is CALCULATED, whatever its source claimed.
        /// </summary>
        private static bool ResolveProvenance(bool useCalculated, RackFrameConfiguration configuration)
            => useCalculated || configuration == null;

        private static DynamicRackModuleDesign ToIntent(DynamicRackModule module)
            => new DynamicRackModuleDesign
            {
                ModuleId = module.ModuleId,
                Kind = module.Kind,
                Length = module.Length,
                IsCalculated = module.IsCalculated,
                IsManualOverride = module.IsManualOverride,
                UseCalculatedHeaderConfiguration = ResolveProvenance(
                    module.UseCalculatedHeaderConfiguration, module.AssociatedFrameConfiguration),
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
                UseCalculatedHeaderConfiguration = ResolveProvenance(
                    source.UseCalculatedHeaderConfiguration, source.HeaderConfiguration),
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
