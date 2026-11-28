using System;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.PushBack.Pilot;

namespace RackCad.Application.Systems.PushBack.Draft
{
    /// <summary>
    /// I-64 F4B PB-T3a (D-PBT3-05) — the pair <see cref="PushBackCompositeEditorAssembler.BuildDesign"/> consumes, with the
    /// name of the draft, or the reason the draft cannot be composed. The objects of a composition belong to the caller:
    /// they are fresh objects, never one of the states of a draft.
    /// </summary>
    public sealed class PushBackFullWriteComposition
    {
        private PushBackFullWriteComposition(PushBackCompositeEditorState editor, PushBackEditorInputs inputs, string name, string reason)
        {
            Editor = editor;
            Inputs = inputs;
            Name = name;
            Reason = reason;
        }

        public bool IsComposed => Editor != null && Inputs != null;

        /// <summary>The composite state to hand to the assembler; null when the composition was rejected.</summary>
        public PushBackCompositeEditorState Editor { get; }

        /// <summary>The rack-wide inputs to hand to the assembler; null when the composition was rejected.</summary>
        public PushBackEditorInputs Inputs { get; }

        /// <summary>The name of the draft, as it is (it can be null or blank).</summary>
        public string Name { get; }

        /// <summary>Why the composition was rejected; null when it was composed.</summary>
        public string Reason { get; }

        public static PushBackFullWriteComposition Composed(PushBackCompositeEditorState editor, PushBackEditorInputs inputs, string name)
            => new PushBackFullWriteComposition(
                editor ?? throw new ArgumentNullException(nameof(editor)),
                inputs ?? throw new ArgumentNullException(nameof(inputs)),
                name,
                null);

        public static PushBackFullWriteComposition Rejected(string reason)
            => new PushBackFullWriteComposition(null, null, null, reason);
    }

    /// <summary>
    /// I-64 F4B PB-T3a (D-PBT3-05) — the composition to write and the composition of "keep my draft" for the FULL model,
    /// not connected to the panel. In the full model every editable field comes from the draft: nothing is compared with
    /// what "the editor does not edit", because there is nothing like that.
    /// </summary>
    public static class PushBackFullComposition
    {
        /// <summary>
        /// The composition to write. Pure and total: it mutates neither state and never throws. It is only safe AFTER the base
        /// matched a re-read (H-06): the plan of an update calls it then. A scale that is not finite or not greater than zero is
        /// rejected, and so is a base that, materialized again, no longer equals the initial state. The editor, the carriers
        /// and the name come from a copy of <paramref name="current"/>.
        /// </summary>
        public static PushBackFullWriteComposition ForFullWrite(PushBackFullDraftState initial, PushBackFullDraftState current, RackCatalog catalog)
        {
            try
            {
                if (initial == null || current == null) return PushBackFullWriteComposition.Rejected("Falta el estado inicial o el actual.");
                if (catalog == null) return PushBackFullWriteComposition.Rejected(PushBackPilotMaterializer.CatalogRequiredDiagnostic);

                var scale = current.Editable.AnnotationScale;
                if (double.IsNaN(scale) || double.IsInfinity(scale) || scale <= 0.0)
                    return PushBackFullWriteComposition.Rejected(PushBackPilotMaterializer.InvalidScaleReason);

                var fresh = PushBackFullDraftMaterializer.Materialize(initial.Origin, catalog);
                if (!fresh.IsAdmitted)
                    return PushBackFullWriteComposition.Rejected(fresh.Reason ?? PushBackPilotMaterializer.UnreadableOrFutureDiagnostic);
                if (!PushBackFullDraftStateComparer.Instance.Equals(initial, fresh.State))
                    return PushBackFullWriteComposition.Rejected(PushBackPilotMaterializer.BaseChangedReason);

                var copy = PushBackEditableStateComparer.Copy(current.Editable);
                return PushBackFullWriteComposition.Composed(copy.Editor, copy.BuildInputs(new PushBackEditorDesignAssembler(catalog)), copy.Name);
            }
            catch (Exception ex)
            {
                return PushBackFullWriteComposition.Rejected(PushBackPilotMaterializer.ComposeFailurePrefix + ex.Message);
            }
        }

        /// <summary>
        /// After the user keeps the draft on a newer version, the current state is a copy of the editable state of
        /// <paramref name="current"/> over the origin of <paramref name="newInitial"/>. Pure: it mutates neither argument.
        /// </summary>
        public static PushBackFullDraftState KeepOn(PushBackFullDraftState newInitial, PushBackFullDraftState current)
        {
            if (newInitial == null) throw new ArgumentNullException(nameof(newInitial));
            if (current == null) throw new ArgumentNullException(nameof(current));
            return new PushBackFullDraftState(PushBackEditableStateComparer.Copy(current.Editable), newInitial.Origin);
        }
    }
}
