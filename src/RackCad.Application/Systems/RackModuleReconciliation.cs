using System;
using System.Collections.Generic;
using System.Linq;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems;

namespace RackCad.Application.Systems
{
    /// <summary>What a rebuild does with a cabecera the user had customized. The caller CHOOSES; this type does not
    /// pick a default, because the answer is an Owner decision (I-35, contract section 12, question b).</summary>
    public enum RackModuleCustomizationPolicy
    {
        /// <summary>Carry the user's cabecera across the rebuild, provenance included.</summary>
        Preserve = 0,

        /// <summary>Drop it and let the rebuild's standard cabecera stand — what the base does today, silently.</summary>
        Discard = 1
    }

    /// <summary>Counts of what a reconciliation actually did, so a caller can report it instead of guessing.</summary>
    public sealed class RackModuleReconciliationResult
    {
        public RackModuleReconciliationResult(
            int fondosRestored,
            int configurationsPreserved,
            int configurationsDiscarded,
            int intentsDropped)
        {
            FondosRestored = fondosRestored;
            ConfigurationsPreserved = configurationsPreserved;
            ConfigurationsDiscarded = configurationsDiscarded;
            IntentsDropped = intentsDropped;
        }

        /// <summary>Manual fondos re-applied onto the rebuilt headers.</summary>
        public int FondosRestored { get; }

        /// <summary>Custom cabeceras carried across the rebuild.</summary>
        public int ConfigurationsPreserved { get; }

        /// <summary>Custom cabeceras deliberately dropped by the chosen policy.</summary>
        public int ConfigurationsDiscarded { get; }

        /// <summary>Previous header intents with no counterpart because the rebuilt rack has fewer headers.</summary>
        public int IntentsDropped { get; }

        public bool ChangedAnything => FondosRestored > 0 || ConfigurationsPreserved > 0;
    }

    /// <summary>
    /// Reconciles the module intents the user had against a FRESHLY REBUILT structure, by ordinal among headers
    /// (I-35).
    /// <para>
    /// The base reconciles with <c>DynamicEditorDesignAssembler.SnapshotHeaderFondos</c> /
    /// <c>RestoreHeaderFondos</c>, and that pair carries the FONDO and nothing else: the snapshot is a list of
    /// nullable doubles, and the restore stamps <c>UseCalculatedHeaderConfiguration = true</c> and rebuilds the
    /// configuration from the factory. A custom cabecera therefore cannot survive a pallet/fondos change. That is
    /// inert while no Push Back cabecera can be custom, and stops being inert the moment PB-011 lands — the
    /// dependency <c>ideas-futuras.md</c> asks to review in the same change.
    /// </para>
    /// <para>
    /// This type is the reconciliation that carries fondo, configuration AND provenance together. It does NOT
    /// replace the existing pair and nothing calls it yet: changing <c>RestoreHeaderFondos</c> would change the
    /// DYNAMIC editor, which I-35 must not touch. It is offered so the adopting phase can wire it for Push Back
    /// alone, with the policy the Owner picks.
    /// </para>
    /// <para>
    /// NEUTRAL: no <c>RackSystemKind</c> and no branch per system; it reconciles the shared longitudinal sequence.
    /// Cabeceras are copied with <see cref="RackFrameProjectStore.DeepCopy"/>, the single canonical clone (I-17).
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
        /// Apply <paramref name="previous"/> onto <paramref name="rebuilt"/>, matching HEADERS BY ORDINAL — the same
        /// correspondence the base uses, so a rebuild that keeps the header count lands every intent on the header
        /// that inherits its place.
        /// <list type="bullet">
        /// <item>A manual fondo is re-applied and keeps its override flags.</item>
        /// <item>A custom cabecera is carried over as an independent canonical copy WITH its provenance under
        /// <see cref="RackModuleCustomizationPolicy.Preserve"/>, and left to the rebuild's standard one under
        /// <see cref="RackModuleCustomizationPolicy.Discard"/>.</item>
        /// <item>A calculated cabecera is never touched: the rebuild already produced it at the new inputs.</item>
        /// <item>Extra previous headers are counted as dropped, never forced onto a shorter rack.</item>
        /// </list>
        /// </summary>
        public RackModuleReconciliationResult Reconcile(
            IReadOnlyList<DynamicRackModuleDesign> previous,
            DynamicRackSystem rebuilt,
            RackModuleCustomizationPolicy policy)
        {
            if (rebuilt == null) throw new ArgumentNullException(nameof(rebuilt));

            var intents = (previous ?? Array.Empty<DynamicRackModuleDesign>())
                .Where(design => design != null && design.IsHeader)
                .ToList();
            var headers = rebuilt.Modules.Where(module => module != null && module.IsHeader).ToList();

            var fondos = 0;
            var preserved = 0;
            var discarded = 0;

            for (var ordinal = 0; ordinal < Math.Min(intents.Count, headers.Count); ordinal++)
            {
                var intent = intents[ordinal];
                var header = headers[ordinal];

                if (intent.IsManualOverride && intent.Length > 0.0)
                {
                    header.Length = intent.Length;
                    header.IsManualOverride = true;
                    header.IsCalculated = false;
                    fondos++;
                }

                var wasCustom = !intent.UseCalculatedHeaderConfiguration && intent.HeaderConfiguration != null;
                if (!wasCustom)
                {
                    continue;
                }

                if (policy == RackModuleCustomizationPolicy.Preserve)
                {
                    header.AssociatedFrameConfiguration = clone.DeepCopy(intent.HeaderConfiguration);
                    header.UseCalculatedHeaderConfiguration = false;
                    preserved++;
                }
                else
                {
                    discarded++;
                }
            }

            // Same closing step as the base: the depth of each cabecera follows its module length, the derived model
            // is rebuilt and the longitudinal coordinates are laid out again.
            rebuilt.RecalculatePositions();
            builder.Refresh(rebuilt);

            return new RackModuleReconciliationResult(
                fondos,
                preserved,
                discarded,
                Math.Max(0, intents.Count - headers.Count));
        }
    }
}
