using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.RackFrames;
using RackCad.Application.Systems.Shared;
using RackCad.Domain.Systems.Dynamic;
using RackCad.Domain.Systems.Shared;

namespace RackCad.Application.Systems.Dynamic
{
    /// <summary>
    /// The inputs of a Dinamico rebuild (I-53, contract §7.9): what <c>DynamicRackSystemBuilder.BuildDefault</c> needs for the
    /// new structure, what <c>DynamicRackSystemResolver.Snapshot</c> needs to project the previous intent, and whether this
    /// is «Restaurar estandar».
    /// </summary>
    public sealed class DynamicRackRebuildRequest
    {
        public PalletSpecification Pallet { get; set; }

        public DynamicDepthLayout DepthLayout { get; set; }

        public string HeaderPostCatalogId { get; set; }

        /// <summary>The height of the calculated cabeceras of the new structure.</summary>
        public double HeaderHeight { get; set; }

        /// <summary>The rack-wide post peralte of the new structure.</summary>
        public double PostPeralte { get; set; }

        public int LoadLevels { get; set; }

        public double FirstLevelHeight { get; set; }

        public double BeamDepth { get; set; }

        /// <summary>«Restaurar estandar»: rebuild WITHOUT intents, so nothing of the previous modules is carried.</summary>
        public bool RestoreStandard { get; set; }
    }

    /// <summary>
    /// What a Dinamico rebuild did (I-53, contract §7.6, §7.9): the rebuilt and reconciled system, the reconciliation report
    /// —Preserved, Adapted, Removed, Incompatible and Restored—, the new generation, and whether the remembered source and an
    /// explicit target set were invalidated. Everything a surface has to tell the user is here, so nothing is lost in silence;
    /// showing it is the window's (G7).
    /// </summary>
    public sealed class DynamicRackRebuildResult
    {
        internal DynamicRackRebuildResult(
            DynamicRackSystem system,
            RackModuleReconciliationResult reconciliation,
            long generation,
            bool sourceInvalidated,
            bool explicitTargetsInvalidated)
        {
            System = system;
            Reconciliation = reconciliation;
            Generation = generation;
            SourceInvalidated = sourceInvalidated;
            ExplicitTargetsInvalidated = explicitTargetsInvalidated;
        }

        public DynamicRackSystem System { get; }

        public RackModuleReconciliationResult Reconciliation { get; }

        /// <summary>The generation the rebuild left the editor state at.</summary>
        public long Generation { get; }

        /// <summary>True when a remembered source address was dropped because its sequence no longer exists.</summary>
        public bool SourceInvalidated { get; }

        /// <summary>True when an explicit target set was dropped because its sequence no longer exists.</summary>
        public bool ExplicitTargetsInvalidated { get; }

        /// <summary>
        /// One Spanish sentence for a status line: the reconciliation's own report followed by what the rebuild invalidated;
        /// empty when there is nothing to say.
        /// </summary>
        public string Describe()
        {
            var parts = new List<string>();
            var reconciliation = Reconciliation.Describe();
            if (!string.IsNullOrEmpty(reconciliation))
            {
                parts.Add(reconciliation);
            }

            if (SourceInvalidated)
            {
                parts.Add("se descartó el origen recordado (la estructura se reconstruyó)");
            }

            if (ExplicitTargetsInvalidated)
            {
                parts.Add("se descartaron los destinos elegidos (la estructura se reconstruyó)");
            }

            return string.Join("; ", parts);
        }
    }

    /// <summary>
    /// I-53 — the REBUILD of the Dinamico with reconciliation (contract §7.9, OD-2.b approved by the Owner): what a change of
    /// pallet, a change of fondos or «Restaurar estandar» does to the module sequence.
    /// <code>
    /// intents    := DynamicRackSystemResolver.Snapshot(previous, ...).Modules    (none for «Restaurar estandar»)
    /// rebuilt    := DynamicRackSystemBuilder.BuildDefault(...)
    /// report     := RackModuleReconciliation.Reconcile(intents, rebuilt, restored = none)
    /// generation := generation + 1; the remembered source and an explicit target set are dropped
    /// </code>
    /// <para>
    /// The resolver's snapshot is THE projection of the Dinamico's editable intent —the one the editor persists— so there is
    /// no parallel mapper. The reconciliation is used as it is: it matches by <c>ModuleId + Kind</c>, carries custom
    /// cabeceras (adapted to the new fondo and peralte) and the manual lengths of cabeceras and separators, and reports what it
    /// could not carry. Nothing is restored explicitly, because «Calculada» resets on the spot and leaves nothing staged.
    /// </para>
    /// <para>
    /// A recomposition WITHOUT rebuild is not this: it keeps ids, kinds and the generation. G6 does not wire the window, which
    /// keeps its historical ordinal pair until G7.
    /// </para>
    /// </summary>
    public static class DynamicRackRebuild
    {
        public static DynamicRackRebuildResult Rebuild(
            DynamicRackSystem previous,
            DynamicHeaderBatchState state,
            DynamicRackRebuildRequest request,
            DynamicRackSystemBuilder builder,
            DynamicRackSystemResolver resolver)
        {
            if (state == null)
            {
                throw new ArgumentNullException(nameof(state));
            }

            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (resolver == null)
            {
                throw new ArgumentNullException(nameof(resolver));
            }

            IReadOnlyList<DynamicRackModuleDesign> intents = request.RestoreStandard || previous == null
                ? Array.Empty<DynamicRackModuleDesign>()
                : resolver.Snapshot(
                        previous,
                        request.LoadLevels,
                        request.FirstLevelHeight,
                        request.BeamDepth,
                        request.HeaderPostCatalogId)
                    .Modules
                    .ToList();

            var rebuilt = builder.BuildDefault(
                request.Pallet,
                request.DepthLayout,
                RackFrameTemplateCatalog.Default,
                request.HeaderPostCatalogId,
                request.HeaderHeight,
                request.PostPeralte);
            var report = new RackModuleReconciliation(builder).Reconcile(intents, rebuilt, Array.Empty<string>());

            // Only a rebuild that happened invalidates: a build that throws leaves the editor state as it was.
            state.NoteRebuild(out var sourceInvalidated, out var explicitTargetsInvalidated);
            return new DynamicRackRebuildResult(rebuilt, report, state.Generation, sourceInvalidated, explicitTargetsInvalidated);
        }
    }
}
