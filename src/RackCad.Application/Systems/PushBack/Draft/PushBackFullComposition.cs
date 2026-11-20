using System;
using RackCad.Application.Catalogs;
using RackCad.Application.Systems.PushBack.Pilot;

namespace RackCad.Application.Systems.PushBack.Draft
{
    /// <summary>
    /// I-64 F4B PB-T3a (D-PBT3-05) — the pair <see cref="PushBackCompositeEditorAssembler.BuildDesign"/> consumes, with the
    /// name of the draft, or the reason the draft cannot be composed. The objects of a composition belong to the caller.
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

        public PushBackCompositeEditorState Editor { get; }

        public PushBackEditorInputs Inputs { get; }

        /// <summary>The name of the draft, as it is (it can be null or blank).</summary>
        public string Name { get; }

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

    /// <summary>I-64 F4B PB-T3a (D-PBT3-05) — the composition to write and the composition of "keep my draft", not connected.</summary>
    public static class PushBackFullComposition
    {
        public static PushBackFullWriteComposition ForFullWrite(PushBackFullDraftState initial, PushBackFullDraftState current, RackCatalog catalog)
            => PushBackFullWriteComposition.Rejected(PushBackPilotMaterializer.NonPilotContentReason);

        public static PushBackFullDraftState KeepOn(PushBackFullDraftState newInitial, PushBackFullDraftState current)
            => current;
    }
}
