#nullable enable
using System;
using RackCad.Application.Catalogs;

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

        // RED skeleton: inert. It always answers Failed / Rejected.
        public static PushBackPilotMaterialization Materialize(string originText, RackCatalog catalog)
            => PushBackPilotMaterialization.Failed("skeleton");

        public static PushBackPilotComposition ComposeForWrite(
            PushBackPilotDraftState initial, PushBackPilotDraftState current, RackCatalog catalog)
            => PushBackPilotComposition.Rejected("skeleton");
    }
}
