using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Application.Systems.Dynamic;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Systems.Shared
{
    /// <summary>
    /// What a structural recompute did with each customized module, by module id. Every category is REPORTABLE:
    /// losing a customization is never silent (Owner, I-35).
    /// </summary>
    public sealed class RackModuleReconciliationResult
    {
        internal RackModuleReconciliationResult(
            IReadOnlyList<string> preserved,
            IReadOnlyList<string> adapted,
            IReadOnlyList<string> removed,
            IReadOnlyList<string> incompatible,
            IReadOnlyList<string> restored)
        {
            Preserved = preserved;
            Adapted = adapted;
            Removed = removed;
            Incompatible = incompatible;
            Restored = restored;
        }

        /// <summary>Modules whose customization survived: exact <c>ModuleId + Kind</c> match.</summary>
        public IReadOnlyList<string> Preserved { get; }

        /// <summary>Preserved cabeceras whose <c>Depth</c> and/or rack-wide PERALTE had to be brought to the new
        /// structure. A subset of <see cref="Preserved"/>: adapting is not losing.</summary>
        public IReadOnlyList<string> Adapted { get; }

        /// <summary>Customized modules the rebuilt rack no longer has. They lose their customization — explicitly.</summary>
        public IReadOnlyList<string> Removed { get; }

        /// <summary>Customized modules whose id survived but whose KIND changed (a cabecera became a separator, an end
        /// cabecera became an intermediate one…). Structurally incompatible, so they lose it — explicitly.</summary>
        public IReadOnlyList<string> Incompatible { get; }

        /// <summary>Modules the user explicitly restored. Nothing was carried BY REQUEST, which is a different fact
        /// from having lost it.</summary>
        public IReadOnlyList<string> Restored { get; }

        public bool PreservedAnything => Preserved.Count > 0;

        /// <summary>True when a customization was lost against the user's wish — the only case a surface must report
        /// loudly. An explicit restore is not a loss.</summary>
        public bool LostAnything => Removed.Count > 0 || Incompatible.Count > 0;

        /// <summary>One Spanish sentence for a status line; empty when the recompute changed nothing worth saying.</summary>
        public string Describe()
        {
            var parts = new List<string>();
            if (Preserved.Count > 0) parts.Add(Count(Preserved, "módulo conservado", "módulos conservados"));
            if (Adapted.Count > 0) parts.Add(Count(Adapted, "adaptado al nuevo tamaño", "adaptados al nuevo tamaño"));
            if (Restored.Count > 0) parts.Add(Count(Restored, "restaurado", "restaurados"));
            if (Removed.Count > 0) parts.Add(Count(Removed, "eliminado por el cambio", "eliminados por el cambio"));
            if (Incompatible.Count > 0) parts.Add(Count(Incompatible, "incompatible (cambió de tipo)", "incompatibles (cambiaron de tipo)"));
            return string.Join("; ", parts);
        }

        private static string Count(IReadOnlyList<string> ids, string singular, string plural)
            => string.Format(
                CultureInfo.CurrentCulture,
                "{0} {1} ({2})",
                ids.Count,
                ids.Count == 1 ? singular : plural,
                string.Join(", ", ids));
    }

    /// <summary>
    /// Reconciles the module customizations the user had against a FRESHLY REBUILT structure, matching by exact
    /// <c>ModuleId + Kind</c> (Owner decision, I-35).
    /// <para>
    /// The base reconciles by ORDINAL with <c>DynamicEditorDesignAssembler.SnapshotHeaderFondos</c> /
    /// <c>RestoreHeaderFondos</c>, and that pair carries the FONDO and nothing else: the snapshot is a list of
    /// nullable doubles, and the restore stamps <c>UseCalculatedHeaderConfiguration = true</c> and rebuilds the
    /// configuration from the factory. Two consequences it cannot avoid: a custom cabecera never survives a
    /// pallet/fondos change, and an ordinal match silently lands one module's edit on a DIFFERENT module whenever
    /// the sequence shifts. Matching by id and kind removes both.
    /// </para>
    /// <para>
    /// There is NO ordinary discard policy. A customization is carried across unless one of three things is true,
    /// and each is reported by name: the user restored the module explicitly, the module no longer exists, or its
    /// kind changed. Nothing is ever dropped in silence.
    /// </para>
    /// <para>
    /// This type does NOT replace the base pair — changing <c>RestoreHeaderFondos</c> would change the DYNAMIC
    /// editor, which I-35 must not touch. NEUTRAL: no <c>RackSystemKind</c> and no branch per system; cabeceras are
    /// copied with <see cref="RackFrameProjectStore.DeepCopy"/>, the single canonical clone (I-17).
    /// </para>
    /// </summary>
    public sealed class RackModuleReconciliation
    {
        private readonly RackFrameProjectStore clone = new RackFrameProjectStore();
        private readonly DynamicRackSystemBuilder builder;

        /// <param name="builder">The shared builder, used only to refresh the derived model and the coordinates of
        /// the reconciled system — the same closing step <c>RestoreHeaderFondos</c> performs.</param>
        public RackModuleReconciliation(DynamicRackSystemBuilder builder)
        {
            this.builder = builder ?? throw new ArgumentNullException(nameof(builder));
        }

        /// <summary>
        /// Apply the customizations in <paramref name="previous"/> onto <paramref name="rebuilt"/>.
        /// <list type="bullet">
        /// <item>A MANUAL LENGTH is carried for cabeceras AND separators alike: both consume longitudinal run, so
        /// both are editable and both must survive.</item>
        /// <item>A CUSTOM CABECERA is carried as an independent canonical copy with its provenance, and then ADAPTED
        /// to the rebuilt rack: its <c>Depth</c> follows the module's reconciled length and its PERALTE follows the
        /// rack-wide one. Adapting is what keeps a preserved cabecera physically coherent.</item>
        /// <item>A CALCULATED module is never touched: the rebuild already produced it at the new inputs.</item>
        /// <item>An explicitly RESTORED module is skipped on purpose and reported as restored.</item>
        /// </list>
        /// </summary>
        /// <param name="explicitlyRestoredModuleIds">Modules the user restored; their customization must NOT be
        /// carried across, and the result reports them apart from the ones that were lost.</param>
        public RackModuleReconciliationResult Reconcile(
            IReadOnlyList<DynamicRackModuleDesign> previous,
            DynamicRackSystem rebuilt,
            IEnumerable<string> explicitlyRestoredModuleIds = null)
        {
            if (rebuilt == null) throw new ArgumentNullException(nameof(rebuilt));

            var restoredIds = new HashSet<string>(
                explicitlyRestoredModuleIds ?? Enumerable.Empty<string>(),
                StringComparer.Ordinal);

            var byId = new Dictionary<string, DynamicRackModule>(StringComparer.Ordinal);
            foreach (var module in rebuilt.Modules.Where(module => module != null && !string.IsNullOrWhiteSpace(module.ModuleId)))
            {
                byId[module.ModuleId] = module;
            }

            var preserved = new List<string>();
            var adapted = new List<string>();
            var removed = new List<string>();
            var incompatible = new List<string>();
            var restored = new List<string>();

            foreach (var intent in previous ?? Array.Empty<DynamicRackModuleDesign>())
            {
                if (intent == null || string.IsNullOrWhiteSpace(intent.ModuleId))
                {
                    continue;
                }

                // An explicit restore is checked FIRST and reported even though the restored intent no longer shows a
                // customization: clearing it is precisely what the restore did, and the fact that the user asked for
                // it must reach the surface. Checking "is there anything to carry" first would swallow it silently.
                if (restoredIds.Contains(intent.ModuleId))
                {
                    restored.Add(intent.ModuleId);
                    continue;   // dropped BY REQUEST, not lost
                }

                var hasCustomLength = intent.IsManualOverride && intent.Length > 0.0;
                var hasCustomHeader = intent.IsHeader
                                      && !intent.UseCalculatedHeaderConfiguration
                                      && intent.HeaderConfiguration != null;

                if (!hasCustomLength && !hasCustomHeader)
                {
                    continue;   // nothing to carry; the rebuild's calculated module stands
                }

                if (!byId.TryGetValue(intent.ModuleId, out var module))
                {
                    removed.Add(intent.ModuleId);
                    continue;
                }

                if (module.Kind != intent.Kind)
                {
                    incompatible.Add(intent.ModuleId);
                    continue;
                }

                if (hasCustomLength)
                {
                    module.Length = intent.Length;
                    module.IsManualOverride = true;
                    module.IsCalculated = false;
                }

                if (hasCustomHeader)
                {
                    module.AssociatedFrameConfiguration = clone.DeepCopy(intent.HeaderConfiguration);
                    module.UseCalculatedHeaderConfiguration = false;

                    if (Adapt(module, rebuilt.PostPeralte))
                    {
                        adapted.Add(intent.ModuleId);
                    }
                }

                preserved.Add(intent.ModuleId);
            }

            // Same closing step as the base: the derived model is rebuilt and the longitudinal coordinates are laid
            // out again, so the reconciled system is immediately resolvable.
            rebuilt.RecalculatePositions();
            builder.Refresh(rebuilt);

            return new RackModuleReconciliationResult(preserved, adapted, removed, incompatible, restored);
        }

        /// <summary>
        /// Bring a preserved cabecera to the rebuilt rack: its FONDO is the module's reconciled length and its
        /// PERALTE is the rack-wide one. Returns true when either actually moved, so the caller can report it.
        /// <para>
        /// <c>builder.Refresh</c> assigns the depth for every header anyway; the point of doing it here is to observe
        /// the change while the previous value is still readable, and to leave the configuration coherent even for a
        /// caller that does not refresh.
        /// </para>
        /// </summary>
        private static bool Adapt(DynamicRackModule module, double rackPostPeralte)
        {
            var configuration = module.AssociatedFrameConfiguration;
            if (configuration == null)
            {
                return false;
            }

            var changed = false;

            if (module.Length > 0.0 && configuration.Depth != module.Length)
            {
                configuration.Depth = module.Length;
                changed = true;
            }

            if (rackPostPeralte > 0.0 && configuration.PostPeralte != rackPostPeralte)
            {
                configuration.PostPeralte = rackPostPeralte;
                changed = true;
            }

            return changed;
        }
    }
}
