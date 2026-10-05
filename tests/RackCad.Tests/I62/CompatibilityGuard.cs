#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace RackCad.Tests
{
    /// <summary>The files a compatibility check reads: a working tree, or an in-memory overlay of it (mutation controls).</summary>
    public interface ISurfaceTree
    {
        /// <summary>The text of a repository path, or null when absent.</summary>
        string? Text(string path);

        /// <summary>The Git blob id of a path's content as the repository stores it (CRLF → LF), or null when absent.</summary>
        string? Blob(string path);

        /// <summary>Every file path under the given surfaces (repository-relative, with <c>/</c>).</summary>
        IEnumerable<string> FilesUnder(IEnumerable<string> surfaces);
    }

    /// <summary>The working tree of the repository.</summary>
    public sealed class WorkingSurfaceTree : ISurfaceTree
    {
        public string? Text(string path) => File.Exists(I62Repo.FullPath(path)) ? File.ReadAllText(I62Repo.FullPath(path)).Replace("\r\n", "\n", StringComparison.Ordinal) : null;

        public string? Blob(string path)
        {
            var full = I62Repo.FullPath(path);
            if (!File.Exists(full))
            {
                return null;
            }

            var bytes = File.ReadAllBytes(full);
            var lf = new List<byte>(bytes.Length);
            for (var i = 0; i < bytes.Length; i++)
            {
                if (!(bytes[i] == (byte)'\r' && i + 1 < bytes.Length && bytes[i + 1] == (byte)'\n'))
                {
                    lf.Add(bytes[i]);
                }
            }

            return I62Repo.GitBlobSha1(lf.ToArray());
        }

        public IEnumerable<string> FilesUnder(IEnumerable<string> surfaces)
        {
            var root = I62Repo.Root();
            foreach (var s in surfaces)
            {
                var full = I62Repo.FullPath(s);
                if (s.EndsWith('/') && Directory.Exists(full))
                {
                    foreach (var f in Directory.GetFiles(full, "*", SearchOption.AllDirectories).OrderBy(f => f, StringComparer.Ordinal))
                    {
                        yield return Path.GetRelativePath(root, f).Replace('\\', '/');
                    }
                }
                else if (File.Exists(full))
                {
                    yield return s;
                }
            }
        }
    }

    /// <summary>A working tree with some paths replaced (text) or removed (null): the mutation controls of C-20a.</summary>
    public sealed class OverlaySurfaceTree : ISurfaceTree
    {
        private readonly ISurfaceTree under;
        private readonly Dictionary<string, string?> overrides = new Dictionary<string, string?>(StringComparer.Ordinal);

        public OverlaySurfaceTree(ISurfaceTree under) => this.under = under;

        public OverlaySurfaceTree With(string path, string? text)
        {
            overrides[path] = text?.Replace("\r\n", "\n", StringComparison.Ordinal);
            return this;
        }

        public string? Text(string path) => overrides.TryGetValue(path, out var t) ? t : under.Text(path);

        public string? Blob(string path) => overrides.TryGetValue(path, out var t) ? (t == null ? null : I62Repo.GitBlobSha1(t)) : under.Blob(path);

        public IEnumerable<string> FilesUnder(IEnumerable<string> surfaces)
        {
            var list = surfaces.ToList();
            bool In(string p) => list.Any(s => p == s || (s.EndsWith('/') && p.StartsWith(s, StringComparison.Ordinal)));
            return under.FilesUnder(list).Where(p => !overrides.TryGetValue(p, out var t) || t != null)
                .Concat(overrides.Where(o => o.Value != null && In(o.Key)).Select(o => o.Key)).Distinct(StringComparer.Ordinal);
        }
    }

    /// <summary>
    /// C-20a (Proposal V14 §17 and Anexo E.1, E.4, E.7): the history-free guard of the compatibility adoption. The WORKFLOW entry point exists once and
    /// names §16.13 by its exact heading line; §16.13 exists once; the clause map validates against its schema; <c>Files</c> and <c>Entries</c> are
    /// unique; <c>Surfaces</c> is the closed list of §16.13; ENTRY is exactly the closed list; every <c>EffBlob</c> is the blob of the tree content;
    /// the pointers open §16 and close §16.3; §16.3 without its pointer is the I-61 text (pinned as test data with its source blob); headings are
    /// unique per file in the surfaces. The derivation against <c>EFF^1</c> (MV-3..MV-6) needs history and is C-20b.
    /// </summary>
    public static class CompatibilityGuard
    {
        public const string Workflow = "docs/WORKFLOW.md";
        public const string Plan = "docs/AUTOMATION_PLAN.md";
        public const string EntryHeading = "## 12. Coexistencia de protocolos de ejecución delegada (I61/I62)";
        public const string ResolverHeading = "### 16.13 Compatibilidad de protocolos de ejecución delegada";
        public const string Section16 = "## 16. Ejecución delegada bajo orden del Coordinator";
        public const string Section163 = "### 16.3 Lectura de autoridades";
        public const string Pointer = "Antes de aplicar esta sección, toda unidad aplica §16.13.";
        public const string MapPath = "docs/automation/agent-execution/compatibility/I62-clause-map.json";
        public const string SchemaPath = "docs/automation/agent-execution/compatibility/clause-map.schema.json";
        public const string SurfacesMarker = "**Superficies** (lista cerrada):";

        /// <summary>The closed list of Proposal V14 Anexo E.4, in its order.</summary>
        public static readonly string[] FrozenSurfaces =
        {
            "AGENTS.md", "CLAUDE.md", "docs/AUTOMATION_PLAN.md", "docs/FOUNDATIONS.md", "docs/INITIATIVE_LIFECYCLE.md", "docs/WORKFLOW.md", "docs/adr/",
            "docs/automation/agent-execution/", "docs/initiatives/PROMPT_TEMPLATES.md",
        };

        public static bool InSurfaces(string path) => FrozenSurfaces.Any(s => path == s || (s.EndsWith('/') && path.StartsWith(s, StringComparison.Ordinal)));

        /// <summary>The closed surface list as §16.13 states it: the backticked paths of the paragraph that starts with <see cref="SurfacesMarker"/>.</summary>
        public static List<string>? SurfacesOf(string? plan)
        {
            var lines = plan == null ? null : MarkdownSections.RawLines(plan, ResolverHeading);
            var start = lines?.FindIndex(l => l.StartsWith(SurfacesMarker, StringComparison.Ordinal)) ?? -1;
            if (start < 0)
            {
                return null;
            }

            var paragraph = string.Join(" ", lines!.Skip(start).TakeWhile(l => l.Trim().Length > 0));
            return Regex.Matches(paragraph, "`([^`]+)`").Select(m => m.Groups[1].Value).ToList();
        }

        public static List<string> Check(ISurfaceTree tree, string i61Section163)
        {
            var v = new List<string>();
            void Add(string id, bool ok, string msg)
            {
                if (!ok)
                {
                    v.Add(id + ": " + msg);
                }
            }

            var workflow = tree.Text(Workflow);
            var plan = tree.Text(Plan);
            var entries = MarkdownSections.Match(workflow, EntryHeading);
            Add("ENTRY-ONCE", entries.Count == 1, "the WORKFLOW entry point appears " + entries.Count + " times");
            if (entries.Count == 1)
            {
                var named = Regex.Matches(entries[0].Text, "`(### 16\\.13 [^`]+)`").Select(m => m.Groups[1].Value).Distinct(StringComparer.Ordinal).ToList();
                Add("ENTRY-NAMES-16.13", named.Count == 1 && named[0] == ResolverHeading, "the entry point does not name §16.13 by its exact heading line");
            }

            var resolver = MarkdownSections.Match(plan, ResolverHeading);
            Add("16.13-ONCE", resolver.Count == 1, "§16.13 appears " + resolver.Count + " times");
            Add("SURFACES-16.13", SurfacesOf(plan)?.SequenceEqual(FrozenSurfaces) == true, "§16.13 does not state the closed surface list of Anexo E.4");

            // Pointers: the first sentence of §16 and the last sentence of §16.3; §16.3 without its pointer is the I-61 text.
            var s16 = plan == null ? null : MarkdownSections.RawLines(plan, Section16);
            var firstBody = s16?.Skip(1).FirstOrDefault(l => l.Trim().Length > 0);
            Add("POINTER-16", firstBody != null && firstBody.StartsWith(Pointer, StringComparison.Ordinal), "§16 does not open with the pointer to §16.13");
            var s163 = MarkdownSections.Match(plan, Section163);
            var ends = s163.Count == 1 && s163[0].Text.EndsWith(" " + Pointer, StringComparison.Ordinal);
            Add("POINTER-16.3", ends, "§16.3 does not close with the pointer to §16.13");
            Add("16.3-LITERAL", s163.Count == 1 && MarkdownSections.Norm(s163[0].Text.Replace(Pointer, string.Empty, StringComparison.Ordinal)) == MarkdownSections.Norm(i61Section163),
                "§16.3 without its pointer is not the I-61 text");

            // The clause map and its schema.
            JsonNode? map = null;
            JsonNode? schema = null;
            try
            {
                map = tree.Text(MapPath) is string m ? JsonNode.Parse(m) : null;
                schema = tree.Text(SchemaPath) is string sc ? JsonNode.Parse(sc) : null;
            }
            catch (System.Text.Json.JsonException e)
            {
                Add("MAP-SCHEMA", false, "unreadable JSON: " + e.Message);
            }

            Add("MAP-SCHEMA", map != null && schema != null, "the clause map or its schema is absent");
            if (map == null || schema == null)
            {
                return v;
            }

            var schemaProblems = MiniJsonSchema.Validate(schema, map);
            Add("MAP-SCHEMA", schemaProblems.Count == 0, string.Join("; ", schemaProblems.Take(5)));
            if (schemaProblems.Count > 0)
            {
                return v;
            }

            var files = ((JsonArray)map["Files"]!).Select(f => (JsonObject)f!).ToList();
            var mapEntries = ((JsonArray)map["Entries"]!).Select(e => (JsonObject)e!).ToList();
            var surfaces = ((JsonArray)map["Surfaces"]!).Select(s => (string)s!).ToList();
            Add("SURFACES-MAP", surfaces.SequenceEqual(FrozenSurfaces), "the map's Surfaces is not the closed list");
            var paths = files.Select(f => (string)f["Path"]!).ToList();
            Add("FILES-UNIQUE", paths.Distinct(StringComparer.Ordinal).Count() == paths.Count, "a file is listed twice");
            Add("FILES-NOT-MAP", !paths.Contains(MapPath), "Files lists the map itself");
            var keys = mapEntries.Select(e => (string)e["Path"]! + "\n" + (string)e["Section"]!).ToList();
            Add("ENTRIES-UNIQUE", keys.Distinct(StringComparer.Ordinal).Count() == keys.Count, "an entry (Path, Section) is listed twice");

            var entryKinds = mapEntries.Where(e => (string)e["Kind"]! == "ENTRY").Select(e => (string)e["Path"]! + "\n" + (string)e["Section"]!).OrderBy(x => x, StringComparer.Ordinal);
            var closed = new[] { Plan + "\n" + MarkdownSections.Norm(ResolverHeading), Workflow + "\n" + MarkdownSections.Norm(EntryHeading) }.OrderBy(x => x, StringComparer.Ordinal);
            Add("ENTRY-CLOSED", entryKinds.SequenceEqual(closed), "the ENTRY entries are not exactly §16.13 and the WORKFLOW entry point");
            Add("ENTRY-FILE-CLOSED", files.Where(f => (string)f["FileKind"]! == "ENTRY").All(f => (string)f["Path"]! == SchemaPath)
                                     && files.Any(f => (string)f["Path"]! == SchemaPath && (string)f["FileKind"]! == "ENTRY"),
                "the ENTRY files are not exactly the map's schema");

            foreach (var f in files)
            {
                var path = (string)f["Path"]!;
                var kind = (string)f["FileKind"]!;
                Add("FILE-IN-SURFACES", InSurfaces(path), path + " is outside the surfaces");
                Add("EFFBLOB", tree.Blob(path) == (string?)f["EffBlob"], path + ": EffBlob is not the blob of the tree content");
                Add("BASEBLOB-NULL", (kind == "MODIFIED") == (f["BaseBlob"] != null), path + ": BaseBlob must be present exactly for MODIFIED");
            }

            var modified = new HashSet<string>(files.Where(f => (string)f["FileKind"]! == "MODIFIED").Select(f => (string)f["Path"]!), StringComparer.Ordinal);
            Add("ENTRY-PATH-MODIFIED", mapEntries.All(e => modified.Contains((string)e["Path"]!)), "an entry belongs to a file that is not MODIFIED");

            foreach (var file in tree.FilesUnder(FrozenSurfaces).Where(p => p.EndsWith(".md", StringComparison.Ordinal)))
            {
                var dup = MarkdownSections.DuplicateHeadings(tree.Text(file) ?? string.Empty);
                Add("HEADINGS-UNIQUE", dup.Count == 0, file + " repeats " + string.Join(" | ", dup));
            }

            return v;
        }
    }
}
