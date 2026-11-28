using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

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
    /// The ownership map of the persisted fields of a Push Back rack: a TABLE of data, with no composition logic. The
    /// editor owns everything the design carries; the envelope, the project and the draft session own the rest.
    /// </summary>
    public static class PushBackEditableStateOwnership
    {
        private const string EmbedDocument = "RackEmbedDocument.cs, RackEmbedDocument";
        private const string ProjectDocument = "RackProjectDocument.cs, RackProjectDocument";
        private const string PayloadDocument = "PushBackDesignDocument.cs, PushBackDesignDocument";
        private const string EditorSource = "RackPushBackSystemWindow.xaml.cs, Name_Changed";
        private const string DesignAssemblers =
            "PushBackEditorDesignAssembler.cs, BuildDesign; PushBackCompositeEditorAssembler.cs, BuildDesign; "
            + "DynamicEditorDesignAssembler.cs, BuildDesign";

        private static readonly Regex IndexPattern = new Regex(@"\[\d+\]", RegexOptions.CultureInvariant);

        public static IReadOnlyList<PushBackOwnershipRow> Table { get; } = BuildTable();

        /// <summary>The path with every index <c>[n]</c> replaced by <c>[*]</c>.</summary>
        public static string Normalize(string jsonPath)
            => jsonPath == null ? null : IndexPattern.Replace(jsonPath, "[*]");

        /// <summary>
        /// The owner of a path: the row of its level that covers it with the LONGEST prefix. A row <c>Q</c> covers
        /// <c>P</c> when <c>P == Q</c> or <c>P</c> starts with <c>Q.</c> or <c>Q[</c>. With no row, a path whose first
        /// property is not declared by the document of its level belongs to the extension data, which the envelope
        /// owns; anything else is unclassified.
        /// </summary>
        public static PushBackFieldOwner Of(PushBackDocumentLevel level, string jsonPath)
        {
            var path = Normalize(jsonPath);
            if (string.IsNullOrEmpty(path))
            {
                return PushBackFieldOwner.Unclassified;
            }

            PushBackOwnershipRow best = null;
            foreach (var row in Table)
            {
                if (row.Level != level || !Covers(row.Path, path))
                {
                    continue;
                }

                if (best == null || row.Path.Length > best.Path.Length)
                {
                    best = row;
                }
            }

            if (best != null)
            {
                return best.Owner;
            }

            var first = FirstProperty(path);
            var declared = Table.Any(row => row.Level == level
                && string.Equals(FirstProperty(row.Path), first, StringComparison.Ordinal));
            return !declared && first.Length > 0 ? PushBackFieldOwner.Envelope : PushBackFieldOwner.Unclassified;
        }

        private static bool Covers(string rowPath, string path)
            => string.Equals(path, rowPath, StringComparison.Ordinal)
               || path.StartsWith(rowPath + ".", StringComparison.Ordinal)
               || path.StartsWith(rowPath + "[", StringComparison.Ordinal);

        /// <summary>The first property name of a path <c>$.A.B[3].C</c>, which is <c>A</c>.</summary>
        private static string FirstProperty(string path)
        {
            if (!path.StartsWith("$.", StringComparison.Ordinal))
            {
                return string.Empty;
            }

            var rest = path.Substring(2);
            var end = rest.IndexOfAny(new[] { '.', '[' });
            return end < 0 ? rest : rest.Substring(0, end);
        }

        private static IReadOnlyList<PushBackOwnershipRow> BuildTable()
        {
            var rows = new List<PushBackOwnershipRow>();

            foreach (var path in new[] { "$.SchemaVersion", "$.Kind", "$.View", "$.Section", "$.Id", "$.CustomProperties" })
            {
                rows.Add(new PushBackOwnershipRow(PushBackDocumentLevel.Envelope, path, PushBackFieldOwner.Envelope, EmbedDocument));
            }

            rows.Add(new PushBackOwnershipRow(PushBackDocumentLevel.Envelope, "$.Name", PushBackFieldOwner.Editor, EditorSource));
            rows.Add(new PushBackOwnershipRow(
                PushBackDocumentLevel.Envelope, "$.Design", PushBackFieldOwner.Container,
                "RackEmbedDocument.cs, RackEmbedDocument.Design"));

            foreach (var path in new[]
                     {
                         "$.SchemaVersion", "$.Kind", "$.Header", "$.DynamicSystem", "$.SelectiveRack", "$.FlowBed",
                         "$.Larguero", "$.Cantilever",
                     })
            {
                rows.Add(new PushBackOwnershipRow(
                    PushBackDocumentLevel.Project, path, PushBackFieldOwner.Envelope,
                    ProjectDocument + "; RackPushBackSystemWindow.xaml.cs, SaveLibrary_Click (WithSourceMetadataFrom)"));
            }

            rows.Add(new PushBackOwnershipRow(
                PushBackDocumentLevel.Project, "$.PushBack", PushBackFieldOwner.Container,
                "RackProjectDocument.cs, RackProjectDocument.PushBack"));

            rows.Add(new PushBackOwnershipRow(
                PushBackDocumentLevel.Payload, "$.SchemaVersion", PushBackFieldOwner.Envelope, PayloadDocument));

            foreach (var path in new[]
                     {
                         "$.Structure", "$.Fronts", "$.LegacyHighEndBeamPeralte", "$.RearTopeSaque", "$.RearTopePieceId",
                         "$.DefensePieceId", "$.RearTopeOffCells", "$.SideB", "$.Composite",
                     })
            {
                rows.Add(new PushBackOwnershipRow(PushBackDocumentLevel.Payload, path, PushBackFieldOwner.Editor, DesignAssemblers));
            }

            return rows;
        }
    }
}
