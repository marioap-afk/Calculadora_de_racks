using RackCad.Application.Formatting;
using RackCad.Application.Workspace.Drafts;

namespace RackCad.Application.Systems.PushBack.Draft
{
    /// <summary>
    /// I-64 F4B PB-T4b (D-PBT4-06; B-02, B-07; Q-PBT2-3) — the one rule of the annotation scale of a Push Back rack: a required number greater
    /// than zero, checked by <see cref="NumericTextRule"/>, the rule the numeric boxes delegate to. The field of the panel takes it through the
    /// adapter of the full draft, by default: nothing assigns it.
    /// </summary>
    public static class PushBackAnnotationScaleRule
    {
        public const double Minimum = 0.0;

        public const bool MinimumInclusive = false;

        // INERT in the RED of PB-T4b.
        public static DraftFieldValidation Validate(string text) => DraftFieldValidation.Invalid(string.Empty);
    }
}
