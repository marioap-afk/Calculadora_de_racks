#nullable enable
using System;
using System.Linq;
using RackCad.Application.Catalogs;
using RackCad.Application.Persistence;
using RackCad.Domain.Systems.PushBack;

namespace RackCad.Application.Systems.PushBack.Pilot
{
    public enum PushBackPilotMaterializationKind
    {
        /// <summary>The envelope is a single-sided Push Back the pilot can edit: <see cref="PushBackPilotMaterialization.State"/> is set.</summary>
        Admitted,

        /// <summary>A readable Push Back the pilot does not serve (an A/B rack, or one that parks a dormant tail): it is read-only here.</summary>
        OutOfPilot,

        /// <summary>The text could not be read or resolved: <see cref="PushBackPilotMaterialization.Reason"/> says why.</summary>
        Failed,
    }

    /// <summary>
    /// The typed result of <see cref="PushBackPilotMaterializer.Materialize"/>: Admitted(state), OutOfPilot(reason) or
    /// Failed(reason). Nothing is ever thrown to the caller.
    /// </summary>
    public sealed class PushBackPilotMaterialization
    {
        private PushBackPilotMaterialization(PushBackPilotMaterializationKind kind, PushBackPilotDraftState? state, string? reason)
        {
            Kind = kind;
            State = state;
            Reason = reason;
        }

        public PushBackPilotMaterializationKind Kind { get; }

        public bool IsAdmitted => Kind == PushBackPilotMaterializationKind.Admitted;

        /// <summary>The materialized state; null unless the result is Admitted.</summary>
        public PushBackPilotDraftState? State { get; }

        /// <summary>Why the result is not Admitted; null for Admitted.</summary>
        public string? Reason { get; }

        public static PushBackPilotMaterialization Admitted(PushBackPilotDraftState state)
            => new PushBackPilotMaterialization(PushBackPilotMaterializationKind.Admitted, state ?? throw new ArgumentNullException(nameof(state)), null);

        public static PushBackPilotMaterialization OutOfPilot(string reason)
            => new PushBackPilotMaterialization(PushBackPilotMaterializationKind.OutOfPilot, null, reason);

        public static PushBackPilotMaterialization Failed(string reason)
            => new PushBackPilotMaterialization(PushBackPilotMaterializationKind.Failed, null, reason);
    }

    /// <summary>
    /// The pair <see cref="PushBackCompositeEditorAssembler.BuildDesign"/> consumes, or the reason the base cannot be
    /// composed. The state and the inputs of a composition belong to the caller: they are fresh objects, never one of the
    /// states of a draft.
    /// </summary>
    public sealed class PushBackPilotComposition
    {
        private PushBackPilotComposition(PushBackCompositeEditorState? editor, PushBackEditorInputs? inputs, string? reason)
        {
            Editor = editor;
            Inputs = inputs;
            Reason = reason;
        }

        public bool IsComposed => Editor != null && Inputs != null;

        /// <summary>The composite state to hand to the assembler; null when the composition was rejected.</summary>
        public PushBackCompositeEditorState? Editor { get; }

        /// <summary>The rack-wide inputs to hand to the assembler; null when the composition was rejected.</summary>
        public PushBackEditorInputs? Inputs { get; }

        /// <summary>Why the composition was rejected; null when it was composed.</summary>
        public string? Reason { get; }

        public static PushBackPilotComposition Composed(PushBackCompositeEditorState editor, PushBackEditorInputs inputs)
            => new PushBackPilotComposition(
                editor ?? throw new ArgumentNullException(nameof(editor)),
                inputs ?? throw new ArgumentNullException(nameof(inputs)),
                null);

        public static PushBackPilotComposition Rejected(string reason)
            => new PushBackPilotComposition(null, null, reason);
    }

    /// <summary>
    /// I-64 F4-T2A (D-T2-03, D-T2-04) — materializes the pilot state from the authored envelope text and composes the
    /// state to write. It owns NO parser: it reads through the existing RACKEDITAR path (<see cref="RackEmbedStore"/>, the
    /// kind check and <see cref="RackProjectStore"/>) and reproduces the Application part of what the classic window does
    /// to load a design, in the same order. It does not reproduce what only the controls do: LoadFromModel and the first
    /// recompute.
    /// </summary>
    public static class PushBackPilotMaterializer
    {
        /// <summary>The single diagnostic of INV-17: an unreadable envelope, or one written by a future version.</summary>
        public const string UnreadableOrFutureDiagnostic = "Ilegible o de versión futura.";

        public const string KindDivergentDiagnostic = "El sobre no es un sistema Push Back.";

        public const string NoDesignDiagnostic = "El sobre no trae un diseño Push Back legible.";

        public const string CompositeOutOfPilotReason = "Rack de dos lados (A/B): fuera del piloto.";

        public const string DormantTailOutOfPilotReason = "El diseño aparca una cola compuesta: fuera del piloto.";

        public const string CatalogRequiredDiagnostic = "Se necesita el catálogo para materializar el diseño.";

        public const string NonPilotContentReason = "El borrador cambia contenido que el piloto no edita: no se compone.";

        public const string MaterializeFailurePrefix = "No se pudo materializar el diseño: ";

        public const string ComposeFailurePrefix = "No se pudo componer el diseño: ";

        /// <summary>
        /// Materializes the state of the pilot from the raw authored envelope text. It never throws: the outcome is a typed
        /// result, and a failure of the existing reader or of the resolver becomes Failed with its message.
        /// </summary>
        public static PushBackPilotMaterialization Materialize(string originText, RackCatalog catalog)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(originText)) return PushBackPilotMaterialization.Failed(UnreadableOrFutureDiagnostic);
                if (catalog == null) return PushBackPilotMaterialization.Failed(CatalogRequiredDiagnostic);

                // 1. The existing reading path of RACKEDITAR, without a parser of its own.
                var embed = new RackEmbedStore().Deserialize(originText);
                if (embed == null) return PushBackPilotMaterialization.Failed(UnreadableOrFutureDiagnostic);
                if (!string.Equals(embed.Kind, RackEmbedDocument.KindPushBack, StringComparison.Ordinal))
                    return PushBackPilotMaterialization.Failed(KindDivergentDiagnostic);

                if (!new RackProjectStore().TryDeserialize(embed.Design, out var project, out var incompatibleMajor))
                    return PushBackPilotMaterialization.Failed(incompatibleMajor ? UnreadableOrFutureDiagnostic : NoDesignDiagnostic);

                var design = project?.PushBackDesign;
                if (design == null) return PushBackPilotMaterialization.Failed(NoDesignDiagnostic);

                // A single-sided design is `!IsComposite` (the composite assembler always writes a Composite block).
                if (design.IsComposite) return PushBackPilotMaterialization.OutOfPilot(CompositeOutOfPilotReason);

                // 2. The Application part of RackPushBackSystemWindow.LoadExisting, in the same order.
                var resolver = new PushBackResolver(catalog);
                var editor = new PushBackCompositeEditorState();
                editor.SetActiveSide(PushBackSide.A);
                editor.SetSideBPresent(false);
                var inputs = editor.SideA.LoadFromDesign(design, resolver);

                editor.SetSideBPresent(false);
                editor.LoadComposite(design.Composite);

                editor.ClearDormantTail();
                var structure = design.Structure;
                if (structure != null)
                {
                    editor.ParkDormantTail(
                        structure.Modules.Where(module => module != null && PushBackCompositeStructure.IsCompositeTailId(module.ModuleId)),
                        structure.HeaderLineOverrides.Where(line => line != null && PushBackCompositeStructure.IsCompositeTailId(line.ModuleId)));
                }

                if (editor.DormantCompositeTail.Count > 0 || editor.DormantTailLineOverrides.Count > 0)
                    return PushBackPilotMaterialization.OutOfPilot(DormantTailOutOfPilotReason);

                editor.AlignSlotGrid();

                // 3. LoadFromModel and the first Recompute are NOT reproduced: they belong to the controls.
                return PushBackPilotMaterialization.Admitted(new PushBackPilotDraftState(editor, inputs, originText));
            }
            catch (Exception ex)
            {
                return PushBackPilotMaterialization.Failed(MaterializeFailurePrefix + ex.Message);
            }
        }

        /// <summary>
        /// The composition to write (IC-07 point 2): it materializes AGAIN from <c>initial.Origin</c> — the current base, which
        /// gives a fresh module authority — and transfers to it the editable content of <paramref name="current"/>. It returns
        /// the pair <see cref="PushBackCompositeEditorAssembler.BuildDesign"/> consumes. Pure and total: it mutates neither
        /// state, it ignores <c>current.Origin</c> and it never throws; a base that no longer materializes gives a typed
        /// rejection.
        /// </summary>
        public static PushBackPilotComposition ComposeForWrite(
            PushBackPilotDraftState initial, PushBackPilotDraftState current, RackCatalog catalog)
        {
            try
            {
                if (initial == null || current == null) return PushBackPilotComposition.Rejected("Falta el estado inicial o el actual.");

                var fresh = Materialize(initial.Origin, catalog);
                if (!fresh.IsAdmitted || fresh.State == null)
                    return PushBackPilotComposition.Rejected(fresh.Reason ?? UnreadableOrFutureDiagnostic);

                // Restore() carries the editable content and leaves the authority of the fresh state (baseline, modules,
                // tail) untouched: what the pilot does not edit stays what the current base says.
                fresh.State.Editor.Restore(current.Editor.Snapshot());
                var inputs = PushBackPilotStateComparer.Copy(current).Inputs;
                return PushBackPilotComposition.Composed(fresh.State.Editor, inputs);
            }
            catch (Exception ex)
            {
                return PushBackPilotComposition.Rejected(ComposeFailurePrefix + ex.Message);
            }
        }

        /// <summary>
        /// Skeleton: the new base with the scale of the draft, for the surface to set as the current state after KeepMine.
        /// </summary>
        public static PushBackPilotDraftState KeepPilotEditsOn(PushBackPilotDraftState newInitial, PushBackPilotDraftState current)
        {
            return PushBackPilotStateComparer.Copy(newInitial);
        }
    }
}
