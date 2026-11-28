#nullable enable
using System;
using System.Collections.Generic;
using RackCad.Application.Workspace.Drafts;

namespace RackCad.Application.Systems.PushBack.Pilot
{
    /// <summary>
    /// I-64 F4-T2A (D-T2-01, ADR-0032 D6) — the pending-text adapter of the bounded Push Back pilot. It serves ONE field,
    /// <see cref="AnnotationScaleKey"/>, with the rule of the classic AnnotationScaleBox: a required number, greater than
    /// zero, parsed with <see cref="LocalizedNumberParser.TryDouble(string, out double)"/> in the current culture.
    ///
    /// <para>
    /// It keeps NO reference to any state: it has no instance field of a state type, so a single adapter can serve every
    /// draft. <see cref="Validate"/> mutates nothing, and <see cref="Apply"/> hands back the SAME object it received, edited
    /// in place, which is what the draft core expects of an adapter (the result is not copied).
    /// </para>
    /// </summary>
    public sealed class PushBackPilotFieldAdapter : IDraftPendingTextAdapter<PushBackPilotDraftState>
    {
        /// <summary>The only field key of the pilot, compared ordinally. Any other key is invalid.</summary>
        public const string AnnotationScaleKey = "AnnotationScale";

        /// <summary>Same wording as the classic control, so the two surfaces tell the user the same thing.</summary>
        public const string EmptyMessage = "Escribe un valor.";

        public const string NotANumberMessage = "Valor no numérico.";

        public const string NotPositiveMessage = "El valor debe ser > 0.";

        public const string UnknownFieldMessage = "Campo desconocido para el piloto.";

        // RED skeleton: inert. It accepts everything and changes nothing.
        public DraftFieldValidation Validate(string fieldKey, string text) => DraftFieldValidation.Valid;

        public PushBackPilotDraftState Apply(PushBackPilotDraftState current, IReadOnlyDictionary<string, string> pendingText) => current;
    }
}
