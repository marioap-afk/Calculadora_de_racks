#nullable enable
using System;

namespace RackCad.Application.Systems.PushBack.Pilot
{
    /// <summary>
    /// I-64 F4-T2A (D-T2-03) — the editable state of the bounded Push Back pilot (F4-PILOT-PUSHBACK-01). It GROUPS the
    /// state that already exists and defines no parallel model: ONE <see cref="PushBackCompositeEditorState"/>, ONE
    /// <see cref="PushBackEditorInputs"/> and the immutable <see cref="Origin"/>. It persists nothing and exposes no
    /// operation that assembles a design: a design is only ever assembled from a composition made by
    /// <see cref="PushBackPilotMaterializer.ComposeForWrite"/>, never directly over the state of a draft.
    /// </summary>
    public sealed class PushBackPilotDraftState
    {
        public PushBackPilotDraftState(PushBackCompositeEditorState editor, PushBackEditorInputs inputs, string origin)
        {
            Editor = editor ?? throw new ArgumentNullException(nameof(editor));
            Inputs = inputs ?? throw new ArgumentNullException(nameof(inputs));
            Origin = origin ?? throw new ArgumentNullException(nameof(origin));
        }

        /// <summary>The composite editor state the classic window edits. A single-sided rack uses side A only.</summary>
        public PushBackCompositeEditorState Editor { get; }

        /// <summary>The rack-wide inputs the classic window reads from its controls (annotations included).</summary>
        public PushBackEditorInputs Inputs { get; }

        /// <summary>
        /// The raw authored envelope text this state was materialized from: an immutable string. In the initial state of
        /// a draft it is always the CURRENT base, because every read that rebases the draft replaces it.
        /// </summary>
        public string Origin { get; }
    }
}
