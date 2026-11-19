using System.Collections.Generic;

namespace RackCad.Application.Systems.PushBack
{
    /// <summary>Who owns a field of the persisted documents of a Push Back rack.</summary>
    public enum PushBackFieldOwner
    {
        /// <summary>No row covers the field: a field nobody has declared.</summary>
        Unclassified = 0,

        /// <summary>The editor writes it, whole, on every output.</summary>
        Editor = 1,

        /// <summary>The envelope, the project or the draft session owns it; the editor only carries it.</summary>
        Envelope = 2,

        /// <summary>A container whose content is classified in the document of the next level.</summary>
        Container = 3
    }

    /// <summary>The three JSON documents a Push Back rack travels in, from the outermost to the innermost.</summary>
    public enum PushBackDocumentLevel
    {
        /// <summary>The root of the envelope JSON.</summary>
        Envelope = 0,

        /// <summary>The root of the JSON of the envelope's <c>Design</c> property.</summary>
        Project = 1,

        /// <summary>The <c>PushBack</c> object of the project.</summary>
        Payload = 2
    }

    /// <summary>One row of the ownership map: a path of one document level, its owner and where that is decided.</summary>
    public sealed class PushBackOwnershipRow
    {
        public PushBackOwnershipRow(PushBackDocumentLevel level, string path, PushBackFieldOwner owner, string source)
        {
            Level = level;
            Path = path;
            Owner = owner;
            Source = source;
        }

        public PushBackDocumentLevel Level { get; }
        public string Path { get; }
        public PushBackFieldOwner Owner { get; }
        public string Source { get; }
    }

    /// <summary>
    /// The ownership map of the persisted fields of a Push Back rack: a TABLE of data, with no composition logic. SKELETON
    /// (I-64 F4B PB-T1b, the RED): the table is empty, <see cref="Normalize"/> returns its argument and every path is
    /// unclassified.
    /// </summary>
    public static class PushBackEditableStateOwnership
    {
        public static IReadOnlyList<PushBackOwnershipRow> Table { get; } = new List<PushBackOwnershipRow>();

        public static string Normalize(string jsonPath) => jsonPath;

        public static PushBackFieldOwner Of(PushBackDocumentLevel level, string jsonPath) => PushBackFieldOwner.Unclassified;
    }
}
