using RackCad.Application.Persistence;

namespace RackCad.Application.ProjectVariables
{
    /// <summary>
    /// One block definition as the drawing-wide sweep saw it, projected PURELY so every decision about it is
    /// testable without AutoCAD. Extracting this from a real drawing belongs to a later gate; what it MEANS
    /// is decided here.
    ///
    /// <para>
    /// The shape exists to keep three failures apart that a tolerant reader collapses into one. A definition
    /// whose outer envelope cannot be interpreted has NO <see cref="RackId"/> and NO <see cref="Kind"/> — and
    /// that is not the same as being a foreign rack, and not the same as being absent. It is a block this
    /// build cannot classify, and nothing downstream may pretend otherwise.
    /// </para>
    /// <para>
    /// <see cref="DefinitionId"/> is always present, because it is physical: it is what a diagnostic can name
    /// when the identity cannot be read. <b>A RackId is never invented.</b>
    /// </para>
    /// </summary>
    public sealed class ProjectVariableScanEntry
    {
        private ProjectVariableScanEntry(
            string definitionId,
            bool outerEnvelopeInterpretable,
            string rackId,
            string kind,
            bool authoredReadable,
            SelectivePalletDesignDocument authored)
        {
            DefinitionId = definitionId;
            OuterEnvelopeInterpretable = outerEnvelopeInterpretable;
            RackId = rackId;
            Kind = kind;
            AuthoredReadable = authoredReadable;
            Authored = authored;
        }

        /// <summary>The physical identifier of the block definition. Always known, even when nothing else is.</summary>
        public string DefinitionId { get; }

        /// <summary>False when the envelope carried RackCad data this build could not interpret.</summary>
        public bool OuterEnvelopeInterpretable { get; }

        /// <summary>The rack this view belongs to. NULL when the envelope could not be interpreted.</summary>
        public string RackId { get; }

        /// <summary>The rack kind. NULL when the envelope could not be interpreted.</summary>
        public string Kind { get; }

        /// <summary>False when the inner selective design could not be deserialized.</summary>
        public bool AuthoredReadable { get; }

        /// <summary>The authored design. Null unless this is a selective view whose design was readable.</summary>
        public SelectivePalletDesignDocument Authored { get; }

        /// <summary>
        /// RackCad data is PRESENT but the envelope cannot be interpreted — invalid JSON, or a MAJOR from the
        /// future. Not absent, not foreign, not negative: unclassifiable.
        /// </summary>
        public static ProjectVariableScanEntry UnreadableEnvelope(string definitionId)
            => new ProjectVariableScanEntry(definitionId, false, null, null, false, null);

        /// <summary>A rack of another kind. ID22A considers only selective racks for bindings.</summary>
        public static ProjectVariableScanEntry Foreign(string definitionId, string rackId, string kind)
            => new ProjectVariableScanEntry(definitionId, true, rackId, kind, false, null);

        /// <summary>A selective view whose authored design was read.</summary>
        public static ProjectVariableScanEntry Selective(
            string definitionId,
            string rackId,
            SelectivePalletDesignDocument authored)
            => new ProjectVariableScanEntry(
                definitionId,
                true,
                rackId,
                RackEmbedDocument.KindSelective,
                authored != null,
                authored);

        /// <summary>
        /// A selective view whose envelope was read — so the rack IS known — but whose inner design was not.
        /// It is a sibling that exists and cannot be understood, which is never the same as one that is absent.
        /// </summary>
        public static ProjectVariableScanEntry SelectiveUnreadableDesign(string definitionId, string rackId)
            => new ProjectVariableScanEntry(
                definitionId,
                true,
                rackId,
                RackEmbedDocument.KindSelective,
                false,
                null);

        /// <summary>True when this entry is a selective view, readable or not.</summary>
        public bool IsSelective
            => OuterEnvelopeInterpretable &&
               string.Equals(Kind, RackEmbedDocument.KindSelective, System.StringComparison.OrdinalIgnoreCase);
    }
}
