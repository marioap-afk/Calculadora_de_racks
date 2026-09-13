using System;
using System.Collections.Generic;
using RackCad.Application.CustomProperties;

namespace RackCad.Application.Persistence
{
    /// <summary>How reading a collection of custom properties turned out (I-54 D-07.3 / ADR-0039 §3, §10).</summary>
    public enum CustomPropertiesReadOutcome
    {
        /// <summary>There is no collection: an empty one, and writable. Not an error.</summary>
        Absent = 1,

        /// <summary>The collection was read and this build understands all of it. Writable.</summary>
        Readable = 2,

        /// <summary>The collection exists but its content is not a valid 1.x document. Read-only; the bytes stay untouched.</summary>
        PresentButUnreadable = 3,

        /// <summary>Written by a newer MAJOR. Read-only; never rewritten, never downgraded.</summary>
        IncompatibleMajor = 4,

        /// <summary>Valid in every other respect, but two entries share one GUID. Read-only: no operation may pick one.</summary>
        AmbiguousIdentity = 5,

        /// <summary>
        /// A syntactically readable, structurally valid major-1 document deeper than <see cref="CustomPropertiesDocument.MaxDepth"/>.
        /// Read-only from the read itself: no write of any kind, delete and empty included.
        /// </summary>
        DepthLimitExceeded = 6,
    }

    /// <summary>
    /// The discriminated result of reading a collection. <b>UNKNOWN is not EMPTY</b>: only <see cref="CustomPropertiesReadOutcome.Absent"/>
    /// and <see cref="CustomPropertiesReadOutcome.Readable"/> carry a document and allow writing; every other outcome is
    /// read-only, carries no document and has no destructive way out in V1.
    ///
    /// <para>
    /// The factories are internal on purpose: a result is something the store DETERMINES, and a caller outside Application
    /// that could fabricate a readable one would bypass every rule the classification enforces.
    /// </para>
    /// </summary>
    public sealed class CustomPropertiesReadResult
    {
        private static readonly IReadOnlyList<CustomPropertyId> NoIds = Array.Empty<CustomPropertyId>();

        private CustomPropertiesReadResult(
            CustomPropertiesReadOutcome outcome,
            CustomPropertiesDocument document,
            string error,
            IReadOnlyList<CustomPropertyId> repeatedNameEntryIds)
        {
            Outcome = outcome;
            Document = document;
            Error = error;
            RepeatedNameEntryIds = repeatedNameEntryIds;
        }

        public CustomPropertiesReadOutcome Outcome { get; }

        /// <summary>The collection: a new empty one for Absent, the read one for Readable, null for every read-only outcome.</summary>
        public CustomPropertiesDocument Document { get; }

        /// <summary>The visible reason for a read-only outcome. Null when the collection is writable.</summary>
        public string Error { get; }

        /// <summary>
        /// The diagnostic of a Readable collection (D-05.2): the ids of the entries whose names repeat, compared in NFC with
        /// <c>OrdinalIgnoreCase</c>, in document order. A diagnostic, not an error: the collection stays writable.
        /// </summary>
        public IReadOnlyList<CustomPropertyId> RepeatedNameEntryIds { get; }

        /// <summary>True only for Absent and Readable. Writing after any other outcome would overwrite what this build could not read.</summary>
        public bool CanWrite
            => Outcome == CustomPropertiesReadOutcome.Absent || Outcome == CustomPropertiesReadOutcome.Readable;

        internal static CustomPropertiesReadResult Absent()
            => new CustomPropertiesReadResult(CustomPropertiesReadOutcome.Absent, CustomPropertiesDocument.CreateNew(), null, NoIds);

        internal static CustomPropertiesReadResult Readable(
            CustomPropertiesDocument document,
            IReadOnlyList<CustomPropertyId> repeatedNameEntryIds)
            => new CustomPropertiesReadResult(
                CustomPropertiesReadOutcome.Readable,
                document,
                null,
                repeatedNameEntryIds ?? NoIds);

        internal static CustomPropertiesReadResult PresentButUnreadable(string error)
            => new CustomPropertiesReadResult(CustomPropertiesReadOutcome.PresentButUnreadable, null, error, NoIds);

        internal static CustomPropertiesReadResult IncompatibleMajor(string error)
            => new CustomPropertiesReadResult(CustomPropertiesReadOutcome.IncompatibleMajor, null, error, NoIds);

        internal static CustomPropertiesReadResult AmbiguousIdentity(string error)
            => new CustomPropertiesReadResult(CustomPropertiesReadOutcome.AmbiguousIdentity, null, error, NoIds);

        internal static CustomPropertiesReadResult DepthLimitExceeded(string error)
            => new CustomPropertiesReadResult(CustomPropertiesReadOutcome.DepthLimitExceeded, null, error, NoIds);
    }
}
