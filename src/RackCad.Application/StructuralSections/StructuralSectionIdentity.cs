using System;

namespace RackCad.Application.StructuralSections
{
    /// <summary>
    /// Everything that NAMES a section, separated from everything that MEASURES it.
    ///
    /// Two published designations are carried, both unaltered (decision 17 of the owner):
    /// - <see cref="EdiDesignation"/>, the AISC Naming Convention for Electronic Data Interchange form. It is
    ///   the basis of the id because its stated purpose is precisely stable electronic interchange.
    /// - <see cref="ManualLabel"/>, the label printed in the AISC Steel Construction Manual. It is what a user
    ///   recognises and types, so it is what a UI shows.
    ///
    /// For W, C and L the two coincide. For rectangular HSS they do NOT: the EDI writes the wall thickness as
    /// a decimal (<c>HSS4X4X.250</c>) and the manual as a fraction (<c>HSS4X4X1/4</c>). The id follows the EDI
    /// in all four families without exception, because a rule that changes its source per family stops being
    /// deterministic (ADR-0021 §6).
    /// </summary>
    public sealed class StructuralSectionIdentity
    {
        public StructuralSectionId SectionId { get; init; }

        public StructuralSectionFamily Family { get; init; }

        /// <summary>Original EDI designation, exactly as published.</summary>
        public string EdiDesignation { get; init; }

        /// <summary>Original AISC Manual label, exactly as published. This is the display name.</summary>
        public string ManualLabel { get; init; }

        /// <summary>FK to <see cref="StructuralSectionSource.SourceId"/>.</summary>
        public string SourceId { get; init; }

        /// <summary>Revision of the source this row was imported from. Never part of the id.</summary>
        public string SourceRevision { get; init; }

        /// <summary>Lookup key derived from <see cref="EdiDesignation"/>. Unique across the whole catalog.</summary>
        public string NormalizedEdiDesignation =>
            StructuralSectionDesignationNormalizer.Normalize(EdiDesignation);

        /// <summary>Lookup key derived from <see cref="ManualLabel"/>; may equal the EDI one when both coincide.</summary>
        public string NormalizedManualLabel =>
            StructuralSectionDesignationNormalizer.Normalize(ManualLabel);

        /// <summary>Rebuilds the id from family + EDI. Used to prove a stored id still matches its designation.</summary>
        public StructuralSectionId ExpectedSectionId =>
            StructuralSectionId.Create(Family, EdiDesignation);

        public override string ToString() => SectionId.Value + " (" + ManualLabel + ")";
    }
}
