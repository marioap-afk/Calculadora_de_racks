#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace RackCad.Tests
{
    /// <summary>The launch decision of a closure: launchable or a STOP with its cause, and the health signals a waived local check leaves (C-41 (a)).</summary>
    public sealed record ClosureDecision(bool Launchable, string? Stop, IReadOnlyList<string> HealthSignals);

    /// <summary>
    /// The effective input closure of a role invocation (AUTOMATION_PLAN 16.24; Proposal V14 §20.3.1; <c>rackcad-input-closure/v1</c>), computed
    /// before any launch: the canonical inputs and the automatic instructions the adapter declares for the working directory; every obligation of a
    /// source, classified READ (a transitive input, option A), ACTION_COMPATIBLE, ACTION_INCOMPATIBLE (only omitted with an explicit exemption bounded
    /// to that invocation and action, option B) or CONDITIONAL_NOT_TRIGGERED; repeated to the fixed point; blobs fixed at the AuthorityRevision. The
    /// obligations are read from the normative sections whose form is fixed: AGENTS.md «Leer primero», CLAUDE.md «Lectura inicial» and «Comandos
    /// esenciales», and the <c>required_docs</c> / <c>optional_docs</c> of a context pack. It also audits the reads of a run against the closure (P-22),
    /// compares the declared and the observed identity (P-23) and says when a changed blob forces a recalculation.
    /// </summary>
    public static class InputClosure
    {
        private static readonly Regex Link = new Regex(@"\[[^\]]*\]\(([^)#\s]+)(#[^)]*)?\)");
        private static readonly Regex Command = new Regex("`((?:git|dotnet) [^`]+)`");
        private static readonly string[] ReadOnlyGit = { "log", "status", "show", "rev-parse", "diff", "cat-file", "ls-files", "blame" };

        private static readonly (string File, string Section)[] ObligationSections =
        {
            ("AGENTS.md", "## Leer primero"),
            ("CLAUDE.md", "## Lectura inicial"),
            ("CLAUDE.md", "## Comandos esenciales"),
        };

        private static string? Text(IStateTree tree, string path) => tree.TryRead(path, out var b) ? Encoding.UTF8.GetString(b).Replace("\r\n", "\n", StringComparison.Ordinal) : null;

        private static string? Blob(IStateTree tree, string path) => tree.KnownBlob(path) ?? (tree.TryRead(path, out var b) ? I62Repo.GitBlobSha1(b) : null);

        private static string Resolve(string from, string target)
        {
            var dir = from.Contains('/') ? from.Substring(0, from.LastIndexOf('/') + 1) : string.Empty;
            var parts = new List<string>();
            foreach (var seg in (dir + target).Split('/'))
            {
                if (seg == "..")
                {
                    if (parts.Count > 0)
                    {
                        parts.RemoveAt(parts.Count - 1);
                    }
                }
                else if (seg != "." && seg.Length > 0)
                {
                    parts.Add(seg);
                }
            }

            return string.Join("/", parts);
        }

        /// <summary>The classified obligations of one source file at the AuthorityRevision (empty for a file with no obligation section).</summary>
        public static List<JsonObject> ObligationsOf(IStateTree tree, string path, IReadOnlyList<string> unitContextPacks, bool readOnly)
        {
            var result = new List<JsonObject>();
            var text = Text(tree, path);
            var blob = Blob(tree, path);
            if (text == null)
            {
                return result;
            }

            JsonObject O(string section, string line, string cls, string? target) => new JsonObject
            {
                ["SourcePath"] = path, ["SourceSection"] = section, ["SourceBlob"] = blob, ["Text"] = line.Trim(), ["Class"] = cls,
                ["Resolution"] = cls switch { "READ" => "INCLUDED", "ACTION_COMPATIBLE" => "ALLOWED", "CONDITIONAL_NOT_TRIGGERED" => "NOT_TRIGGERED", _ => "EXEMPTED" },
                ["Target"] = target, ["Authority"] = null, ["Reason"] = null,
            };

            foreach (var (file, section) in ObligationSections.Where(s => s.File == path))
            {
                var lines = MarkdownSections.RawLines(text, section);
                foreach (var line in lines?.Skip(1).Where(l => l.Trim().Length > 0) ?? Enumerable.Empty<string>())
                {
                    foreach (Match m in Link.Matches(line))
                    {
                        result.Add(O(section, line, "READ", Resolve(path, m.Groups[1].Value)));
                    }

                    foreach (Match m in Command.Matches(line))
                    {
                        var words = m.Groups[1].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        var compatible = words[0] == "git" ? ReadOnlyGit.Contains(words.ElementAtOrDefault(1)) : !readOnly;
                        result.Add(O(section, line, compatible ? "ACTION_COMPATIBLE" : "ACTION_INCOMPATIBLE", m.Groups[1].Value));
                    }

                    if (line.Contains("Context Pack", StringComparison.OrdinalIgnoreCase) && line.Contains("declarad", StringComparison.OrdinalIgnoreCase))
                    {
                        result.AddRange(unitContextPacks.Select(p => O(section, line, "READ", p)));
                    }
                }
            }

            if (path.StartsWith("docs/context-packs/", StringComparison.Ordinal) && text.StartsWith("---\n", StringComparison.Ordinal))
            {
                var front = text.Substring(4, text.IndexOf("\n---", 4, StringComparison.Ordinal) - 4);
                string? list = null;
                foreach (var raw in front.Split('\n'))
                {
                    if (!raw.StartsWith(" ", StringComparison.Ordinal) && raw.EndsWith(":", StringComparison.Ordinal))
                    {
                        list = raw.TrimEnd(':');
                    }
                    else if (raw.TrimStart().StartsWith("- ", StringComparison.Ordinal) && list is "required_docs" or "optional_docs")
                    {
                        var target = raw.TrimStart().Substring(2).Trim().Trim('\'', '"');
                        result.Add(O("frontmatter " + list, raw, list == "required_docs" ? "READ" : "CONDITIONAL_NOT_TRIGGERED", target));
                    }
                    else if (raw.Length > 0 && !raw.StartsWith(" ", StringComparison.Ordinal))
                    {
                        list = null;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// The closure record (<c>rackcad-input-closure/v1</c>) and the launch decision. <paramref name="exemptions"/> maps an ACTION_INCOMPATIBLE
        /// command to its exemption reference (bounded to this invocation and action); without it there is no launch (STOP, COORDINATOR_DECISION).
        /// </summary>
        public static (JsonObject Closure, ClosureDecision Decision) Compute(IStateTree tree, string authorityRevision, IReadOnlyList<string> canonical,
            IReadOnlyList<(string Path, string Adapter)> automaticInstructions, IReadOnlyList<string> unitContextPacks, bool readOnly,
            IReadOnlyDictionary<string, string>? exemptions = null)
        {
            exemptions ??= new Dictionary<string, string>();
            var included = new HashSet<string>(canonical.Concat(automaticInstructions.Select(a => a.Path)), StringComparer.Ordinal);
            var obligations = new List<JsonObject>();
            var transitive = new List<JsonObject>();
            var queue = new Queue<string>(included);
            var iterations = 0;
            while (queue.Count > 0)
            {
                iterations++;
                var next = new Queue<string>();
                while (queue.Count > 0)
                {
                    var source = queue.Dequeue();
                    foreach (var o in ObligationsOf(tree, source, unitContextPacks, readOnly))
                    {
                        if (J.S(o, "Class") == "ACTION_INCOMPATIBLE" && exemptions.TryGetValue(J.S(o, "Target")!, out var exemption))
                        {
                            o["Authority"] = exemption;
                            o["Reason"] = "exención explícita de la autoridad, acotada a esta invocación y esta acción";
                        }

                        obligations.Add(o);
                        var target = J.S(o, "Target");
                        if (J.S(o, "Class") == "READ" && target != null && included.Add(target))
                        {
                            transitive.Add(new JsonObject
                            {
                                ["Path"] = target, ["Blob"] = Blob(tree, target) ?? string.Empty,
                                ["RequiredBy"] = new JsonObject { ["Path"] = source, ["Section"] = J.S(o, "SourceSection"), ["Blob"] = J.S(o, "SourceBlob") },
                            });
                            next.Enqueue(target);
                        }
                    }
                }

                queue = next;
            }

            var actions = obligations.Where(o => J.S(o, "Class") == "ACTION_COMPATIBLE" || (J.S(o, "Class") == "ACTION_INCOMPATIBLE" && o["Authority"] != null))
                .GroupBy(o => J.S(o, "Target")).Select(g => g.First())
                .Select(o => (JsonNode)new JsonObject
                {
                    ["Action"] = J.S(o, "Target"), ["RequiredBy"] = J.S(o, "SourcePath") + " «" + J.S(o, "SourceSection") + "»",
                    ["Class"] = J.S(o, "Class") == "ACTION_COMPATIBLE" ? "ACTION_COMPATIBLE" : "EXEMPTED",
                    ["ExemptionRef"] = o["Authority"]?.DeepClone(), ["ExemptionScope"] = o["Authority"] == null ? null : "esta invocación y esta acción",
                }).ToArray();
            var missing = obligations.Where(o => J.S(o, "Class") == "ACTION_INCOMPATIBLE" && o["Authority"] == null).Select(o => J.S(o, "Target")).Distinct().ToList();
            var waived = obligations.Where(o => J.S(o, "Class") == "ACTION_INCOMPATIBLE" && o["Authority"] != null).Select(o => J.S(o, "Target")!).Distinct().ToList();
            var closure = new JsonObject
            {
                ["Schema"] = "rackcad-input-closure/v1",
                ["AuthorityRevision"] = authorityRevision,
                ["CanonicalInputs"] = new JsonArray(canonical.Select(p => (JsonNode)new JsonObject { ["Path"] = p, ["Blob"] = Blob(tree, p) ?? string.Empty }).ToArray()),
                ["AutomaticInstructions"] = new JsonArray(automaticInstructions.Select(a => (JsonNode)new JsonObject { ["Path"] = a.Path, ["Blob"] = Blob(tree, a.Path) ?? string.Empty, ["Adapter"] = a.Adapter }).ToArray()),
                ["Obligations"] = new JsonArray(obligations.Select(o => (JsonNode)o).ToArray()),
                ["AllowedTransitiveInputs"] = new JsonArray(transitive.Select(t => (JsonNode)t).ToArray()),
                ["AllowedActions"] = new JsonArray(actions),
                ["DeclaredRuntimeContext"] = new JsonArray(),
                ["ForbiddenInputs"] = new JsonArray("transcripción y memoria del autor", "sesiones ajenas", "worktrees reales de otras unidades", "artefactos transitorios"),
                ["FixedPointIterations"] = iterations,
            };
            var decision = missing.Count > 0
                ? new ClosureDecision(false, "STOP (COORDINATOR_DECISION): obligación incompatible con la acción sin exención: " + string.Join(", ", missing), new List<string>())
                : new ClosureDecision(true, null, waived.Select(w => "PUBLICATION_CI: señal de salud de publicación en lugar de «" + w + "»; no es evidencia local, de gate, de Candidato ni de cierre").ToList());
            return (closure, decision);
        }

        /// <summary>The files a closure admits: canonical inputs, automatic instructions and transitive inputs.</summary>
        public static HashSet<string> Admitted(JsonObject closure) =>
            new[] { "CanonicalInputs", "AutomaticInstructions", "AllowedTransitiveInputs" }
                .SelectMany(k => (closure[k] as JsonArray)?.OfType<JsonObject>().Select(o => J.S(o, "Path")!) ?? Enumerable.Empty<string>())
                .ToHashSet(StringComparer.Ordinal);

        /// <summary>
        /// The read audit of a run against its closure (P-22): a read outside the closure (other than the run's own outputs) makes the result
        /// INVALID_REVIEW_CONTEXT and the attempt counts; a run without any recorded read leaves the context UNKNOWN; otherwise FAITHFUL_CONTEXT.
        /// </summary>
        public static (string Context, IReadOnlyList<string> Outside) AuditReads(JsonObject closure, IReadOnlyList<string> reads, Func<string, bool> ownOutput)
        {
            if (reads.Count == 0)
            {
                return ("UNKNOWN", new List<string>());
            }

            var admitted = Admitted(closure);
            var outside = reads.Where(r => !admitted.Contains(r) && !ownOutput(r)).Distinct().ToList();
            return (outside.Count > 0 ? "INVALID_REVIEW_CONTEXT" : "FAITHFUL_CONTEXT", outside);
        }

        /// <summary>
        /// P-23: the reviewer's declared identity against the identity the invoker observed. Both known and different → S-04 (no ingestion); declared
        /// UNKNOWN → no contradiction, the identity is the invoker's observation.
        /// </summary>
        public static (string Outcome, string Identity) CompareIdentity(string? declared, string observed) =>
            declared == null || declared == "UNKNOWN" ? ("NO_CONTRADICTION", observed)
            : declared == observed ? ("CONSISTENT", observed) : ("CONTRADICTION_S04", observed);

        /// <summary>A closure is recalculated before launching when any of its files has another blob now (16.24, step 4).</summary>
        public static List<string> Stale(JsonObject closure, IStateTree now) =>
            new[] { "CanonicalInputs", "AutomaticInstructions", "AllowedTransitiveInputs" }
                .SelectMany(k => (closure[k] as JsonArray)?.OfType<JsonObject>() ?? Enumerable.Empty<JsonObject>())
                .Where(o => Blob(now, J.S(o, "Path")!) != J.S(o, "Blob")).Select(o => J.S(o, "Path")!).ToList();
    }
}
