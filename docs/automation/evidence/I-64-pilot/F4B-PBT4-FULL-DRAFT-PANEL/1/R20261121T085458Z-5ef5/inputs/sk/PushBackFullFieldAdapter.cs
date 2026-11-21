using System;
using System.Collections.Generic;
using RackCad.Application.Formatting;
using RackCad.Application.Workspace.Drafts;

namespace RackCad.Application.Systems.PushBack.Draft
{
    /// <summary>
    /// I-64 F4B PB-T4b (D-PBT4-04, D-PBT4-06) — the pending-text adapter of the field of the panel on the FULL draft. It serves the one field of
    /// the panel, <see cref="AnnotationScaleKey"/>, with <see cref="PushBackAnnotationScaleRule"/>. It keeps no reference to any state, so one
    /// static adapter serves every draft; <see cref="Validate"/> changes nothing, and <see cref="Apply"/> edits the state it receives and hands
    /// back the same object, which is what the draft core expects of an adapter.
    /// </summary>
    public sealed class PushBackFullFieldAdapter : IDraftPendingTextAdapter<PushBackFullDraftState>
    {
        /// <summary>The only field key of the panel, compared ordinally. Any other key is invalid.</summary>
        public const string AnnotationScaleKey = "AnnotationScale";

        public const string EmptyMessage = NumericTextRule.EmptyRequiredMessage;

        public const string NotANumberMessage = NumericTextRule.NotANumberMessage;

        /// <summary>The message of the rule for a number that is not greater than zero.</summary>
        public const string NotPositiveMessage = "El valor debe ser > 0.";

        public const string UnknownFieldMessage = "Campo desconocido para el panel.";

        // INERT in the RED of PB-T4b.
        public DraftFieldValidation Validate(string fieldKey, string text) => DraftFieldValidation.Invalid(UnknownFieldMessage);

        public PushBackFullDraftState Apply(PushBackFullDraftState current, IReadOnlyDictionary<string, string> pendingText) => current;
    }
}
