using System;
using RackCad.Application.Persistence;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>
    /// Turns one swept block definition into the pure entry the rest of ID22A reasons over.
    ///
    /// <para>
    /// It lives here, and not in the Plugin, because everything it decides is a judgement rather than a
    /// measurement: whether the envelope could be interpreted, whether the identity is usable, whether the
    /// inner design is readable. The Plugin measures — the definition id, the reference count, the raw
    /// payload — and no test suite can load it (ADR-0003), so the less that is decided there, the more of
    /// this contract the Core suite can actually prove.
    /// </para>
    /// <para>
    /// The rule it enforces is the one the sweep used to break: <b>a RackId is never invented</b>. An
    /// envelope this build cannot interpret, and one whose identity is blank, both come out unclassifiable
    /// with their physical facts intact — including whether the definition is PLACED, which is measured from
    /// the block table record and never from the payload.
    /// </para>
    /// </summary>
    public static class ProjectVariableScanProjection
    {
        /// <summary>
        /// Projects one definition. <paramref name="embed"/> is null when the outer envelope did not
        /// deserialize; <paramref name="directReferenceCount"/> is what the drawing measured, independently
        /// of that.
        /// </summary>
        public static ProjectVariableScanEntry Project(
            string definitionId,
            RackEmbedDocument embed,
            int directReferenceCount)
        {
            // No envelope, or an envelope whose identity cannot be used to classify it. Both are the same
            // thing downstream: a block carrying RackCad data that this build cannot attribute to a rack.
            if (embed == null ||
                string.IsNullOrWhiteSpace(embed.Id) ||
                string.IsNullOrWhiteSpace(embed.Kind))
            {
                return ProjectVariableScanEntry.UnreadableEnvelope(definitionId, directReferenceCount);
            }

            if (!string.Equals(embed.Kind, RackEmbedDocument.KindSelective, StringComparison.OrdinalIgnoreCase))
            {
                return ProjectVariableScanEntry.Foreign(definitionId, embed.Id, embed.Kind, directReferenceCount);
            }

            SelectivePalletDesignDocument authored;

            try
            {
                authored = new SelectivePalletDesignStore().Deserialize(embed.Design);
            }
            catch (InvalidOperationException)
            {
                // The design store SIGNALS by throwing — an empty payload, invalid JSON, a MAJOR above what
                // this build reads. Catching it here is not swallowing a failure: it becomes a TYPED state
                // that says the rack is known and its design is not, which is precisely what a later decision
                // needs in order to abort instead of quietly proceeding with the siblings it could read.
                return ProjectVariableScanEntry.SelectiveUnreadableDesign(definitionId, embed.Id, directReferenceCount);
            }

            return ProjectVariableScanEntry.Selective(definitionId, embed.Id, authored, directReferenceCount);
        }
    }
}
