using RackCad.Application.ProjectVariables;
using RackCad.Domain.Systems.Selective;

namespace RackCad.Application.Systems.Selective
{
    /// <summary>How resolving a selective design's effective state turned out (I-47 D-04-bis, D-08).</summary>
    public enum SelectiveEffectiveOutcome
    {
        /// <summary>Every binding resolved, and the effective design is ready for geometry, BOM and preview.</summary>
        Success = 1,

        /// <summary>A binding points at a variable that does not exist. NOT a payload problem: the design is perfectly readable, the VARIABLE is missing.</summary>
        BrokenProjectVariableReference = 2,

        /// <summary>The binding names a property this build does not know. It cannot claim the literal beside it is the right value.</summary>
        UnknownPropertyId = 3,

        /// <summary>The reference declares a kind this build cannot interpret — written by a newer build, most likely.</summary>
        UnknownReferenceKind = 4,

        /// <summary>The reference exists but its variable id is unreadable. Unknown is not absent.</summary>
        MalformedReference = 5,
    }

    /// <summary>
    /// The discriminated result of resolving authored into effective.
    ///
    /// <para>
    /// It is a RESULT and not an exception on purpose. These are EXPECTED states — a broken reference, an
    /// unknown property, a kind from the future — and an expected state travelling as a generic exception
    /// ends up decided by whichever <c>catch</c> happens to sit above it. The BOM already has one that turns
    /// anything thrown into "payload ilegible", which would report a rack whose payload is perfectly legible
    /// and whose VARIABLE is what is missing.
    /// </para>
    /// <para>
    /// On any failure <see cref="Design"/> is null. There is no partial effective design, and there is no
    /// fallback to the frozen authored literal: falling back would change the geometry in silence, which is
    /// the failure this whole contract exists to prevent. Repairing is an explicit, warned action of the
    /// user, and it does not happen here.
    /// </para>
    /// </summary>
    public sealed class SelectiveEffectiveResolution
    {
        private SelectiveEffectiveResolution(
            SelectiveEffectiveOutcome outcome,
            SelectivePalletDesign design,
            string error,
            PropertyId propertyId,
            string variableId)
        {
            Outcome = outcome;
            Design = design;
            Error = error;
            PropertyId = propertyId;
            VariableId = variableId;
        }

        public SelectiveEffectiveOutcome Outcome { get; }

        /// <summary>The EFFECTIVE design. Null on every failure.</summary>
        public SelectivePalletDesign Design { get; }

        /// <summary>The visible reason, naming the rack, the property and the variable. Null on success.</summary>
        public string Error { get; }

        /// <summary>The property the failure is about. Empty on success.</summary>
        public PropertyId PropertyId { get; }

        /// <summary>
        /// The variable the failed reference names, EXACTLY as persisted — an unreadable id travels raw
        /// rather than as null, because the raw text is what lets a user find the reference and repair it
        /// (I-47 G13). Null on success and where the reference carried no id at all.
        /// </summary>
        public string VariableId { get; }

        public bool IsSuccess => Outcome == SelectiveEffectiveOutcome.Success;

        public static SelectiveEffectiveResolution Success(SelectivePalletDesign design)
            => new SelectiveEffectiveResolution(SelectiveEffectiveOutcome.Success, design, null, default, null);

        public static SelectiveEffectiveResolution Failure(
            SelectiveEffectiveOutcome outcome,
            PropertyId propertyId,
            string error,
            string variableId = null)
            => new SelectiveEffectiveResolution(outcome, null, error, propertyId, variableId);
    }
}
