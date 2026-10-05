#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace RackCad.Tests
{
    /// <summary>One read of the representation delivered to the reviewer: a file, the canonical line range the read covered and the lines it delivered.</summary>
    public sealed record DeliveredRead(string Path, int From, int To, IReadOnlyDictionary<int, string> Lines);

    /// <summary>One degraded span (Proposal V14 §20.3.3): canonical line, kind CHANGED | DELETED | INSERTED, canonical and visible text, and class.</summary>
    public sealed record DegradedSpan(string Path, int Line, string Kind, string Canonical, string Visible, string Class);

    /// <summary>A premise of a finding (B.10.1): the complete normative proposition, located by path, section and lines.</summary>
    public sealed record PremiseRef(string Path, string Section, int LineStart, int LineEnd, string Quote);

    /// <summary>A canonical normative unit (<c>NormativeUnitRef</c>, without the revision, which the closure takes from the manifest).</summary>
    public sealed record UnitRef(string Document, string UnitId);

    /// <summary>
    /// The fidelity of the canonical inputs (AUTOMATION_PLAN 16.24; Proposal V14 §20.3.3; <c>rackcad-input-fidelity/v1</c>; C-42): the preflight by
    /// the reviewer's own reading path (P-24), the comparison of the representation delivered to the reviewer with the canonical text (DegradedSpans,
    /// FidelityStatus), the premise envelope, the canonical units of the B.11 manifest projected onto their lines, the normative dependency closure
    /// over the declared control edges only, the canonical resolution of a reference and the independence of each premise (INDEPENDENT, INVALID_PREMISE
    /// or UNKNOWN). The only allowed normalization is the end of line.
    /// </summary>
    public static class InputFidelity
    {
        private static string[] Lines(string text) => text.Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd('\n').Split('\n');

        private static string Normalized(string text) => text.Replace("\r\n", "\n", StringComparison.Ordinal).TrimEnd('\n');

        // ------------------------------------------------------------------ preflight (before launching)

        /// <summary>
        /// The preflight of the reading path: FAITHFUL when the transported text is byte-equal, FAITHFUL_NORMALIZED when only the end of line differs,
        /// NOT_ACCREDITED otherwise (P-24: no launch). <c>CharacterClassesChecked</c> counts every distinct non-ASCII character of the corpus.
        /// </summary>
        public static (string Status, IReadOnlyList<(char Character, int Canonical, int Transport)> Classes) Preflight(
            IReadOnlyDictionary<string, string> canonical, IReadOnlyDictionary<string, string?> transport)
        {
            var corpus = string.Concat(canonical.Values);
            var delivered = string.Concat(canonical.Keys.Select(k => transport.TryGetValue(k, out var t) ? t ?? string.Empty : string.Empty));
            var classes = corpus.Where(c => c > 127).Distinct().OrderBy(c => c)
                .Select(c => (c, corpus.Count(x => x == c), delivered.Count(x => x == c))).ToList();
            var status = canonical.All(kv => transport.TryGetValue(kv.Key, out var t) && t == kv.Value) ? "FAITHFUL"
                : canonical.All(kv => transport.TryGetValue(kv.Key, out var t) && t != null && Normalized(t) == Normalized(kv.Value)) ? "FAITHFUL_NORMALIZED"
                : "NOT_ACCREDITED";
            return (status, classes);
        }

        public static bool MayLaunch(string preflightStatus) => preflightStatus is "FAITHFUL" or "FAITHFUL_NORMALIZED";

        // ------------------------------------------------------------------ after the run

        private static readonly Regex Negation = new Regex(@"(?i)\b(no|nunca|ningún|ninguna|ninguno|sin|not|never)\b");
        private static readonly Regex Modality = new Regex(@"\b(DEBE|DEBEN|PUEDE|PUEDEN|NUNCA|SIEMPRE|debe|deben|puede|pueden)\b");
        private static readonly Regex Quantifier = new Regex(@"(?i)\b(todo|toda|todos|todas|cada|ningún|ninguna|alguno|alguna|cualquier)\b");
        private const string Operators = "≤≥≠⇒⇔→∈<>=";

        /// <summary>The class of a degraded line (for the report; any span inside an envelope or a closure invalidates, whatever its class).</summary>
        public static string Classify(string canonical, string visible)
        {
            if (visible.Length == 0)
            {
                return "STRUCTURE";
            }

            int Count(Regex rx, string s) => rx.Matches(s).Count;
            if (Count(Negation, canonical) != Count(Negation, visible))
            {
                return "NEGATION";
            }

            if (Operators.Any(o => canonical.Count(c => c == o) != visible.Count(c => c == o)))
            {
                return "OPERATOR";
            }

            if (Count(Modality, canonical) != Count(Modality, visible))
            {
                return "MODALITY";
            }

            if (Count(Quantifier, canonical) != Count(Quantifier, visible))
            {
                return "QUANTIFIER";
            }

            if (canonical.TrimStart().StartsWith("#", StringComparison.Ordinal))
            {
                return "SCOPE";
            }

            if (canonical.TrimStart().StartsWith("|", StringComparison.Ordinal))
            {
                return "TABLE_ASSOCIATION";
            }

            var ticks = new Regex("`[^`]+`");
            if (!ticks.Matches(canonical).Select(m => m.Value).SequenceEqual(ticks.Matches(visible).Select(m => m.Value)))
            {
                return "IDENTIFIER";
            }

            return visible.Contains((char)0xFFFD) || canonical.Length == visible.Length ? "LETTER" : "OTHER";
        }

        /// <summary>
        /// The comparison of the representation delivered to the reviewer with the canonical text. A line altered in any read is degraded (CHANGED); a line
        /// a read covered but did not deliver is a STRUCTURE span (DELETED); a delivered line beyond the canonical text is INSERTED. Without a delivered
        /// representation (only a full capture, or none) the status is UNVERIFIED. Returns the status (FAITHFUL, FAITHFUL_NORMALIZED, DEGRADED_BOUNDED,
        /// UNVERIFIED), the spans and the visibility of each canonical line (received intact in some read and never altered).
        /// </summary>
        public static (string Status, List<DegradedSpan> Spans, Func<string, int, bool> Visible) Compare(IReadOnlyDictionary<string, string> canonical,
            IReadOnlyList<DeliveredRead>? delivered)
        {
            var spans = new List<DegradedSpan>();
            if (delivered == null)
            {
                return ("UNVERIFIED", spans, (_, _) => false);
            }

            var intact = new HashSet<(string, int)>();
            var altered = new HashSet<(string, int)>();
            var normalizedOnly = false;
            foreach (var read in delivered)
            {
                var lines = canonical.TryGetValue(read.Path, out var text) ? Lines(text) : Array.Empty<string>();
                for (var n = read.From; n <= read.To; n++)
                {
                    var canon = n - 1 < lines.Length ? lines[n - 1] : null;
                    if (!read.Lines.TryGetValue(n, out var got))
                    {
                        if (canon != null)
                        {
                            spans.Add(new DegradedSpan(read.Path, n, "DELETED", canon, string.Empty, "STRUCTURE"));
                        }

                        continue;
                    }

                    if (canon == null)
                    {
                        spans.Add(new DegradedSpan(read.Path, n, "INSERTED", string.Empty, got, "STRUCTURE"));
                    }
                    else if (got == canon)
                    {
                        intact.Add((read.Path, n));
                    }
                    else if (got.TrimEnd('\r') == canon)
                    {
                        intact.Add((read.Path, n));
                        normalizedOnly = true;
                    }
                    else
                    {
                        altered.Add((read.Path, n));
                        spans.Add(new DegradedSpan(read.Path, n, "CHANGED", canon, got, Classify(canon, got)));
                    }
                }

                foreach (var extra in read.Lines.Keys.Where(k => k < read.From || k > read.To))
                {
                    spans.Add(new DegradedSpan(read.Path, extra, "INSERTED", string.Empty, read.Lines[extra], "STRUCTURE"));
                }
            }

            // A line omitted by one read but delivered whole by another was available; the omission span stays only where it was never delivered.
            spans.RemoveAll(s => s.Kind == "DELETED" && intact.Contains((s.Path, s.Line)) && !altered.Contains((s.Path, s.Line)));
            var status = spans.Count > 0 ? "DEGRADED_BOUNDED" : normalizedOnly ? "FAITHFUL_NORMALIZED" : "FAITHFUL";
            return (status, spans, (p, n) => intact.Contains((p, n)) && !altered.Contains((p, n)));
        }

        // ------------------------------------------------------------------ canonical units (B.11) and envelopes

        private static (string? Anchor, int Level) AnchorOf(string line)
        {
            var m = Regex.Match(line, @"^(#{2,4}) (?:Anexo ([A-G]) —|([0-9]+(?:\.[0-9]+)*)\.? |([A-G](?:\.[0-9]+)+) )");
            if (!m.Success)
            {
                return (null, 0);
            }

            var level = m.Groups[1].Value.Length;
            return m.Groups[2].Success ? ("Anexo " + m.Groups[2].Value, level) : m.Groups[3].Success ? ("§" + m.Groups[3].Value, level) : (m.Groups[4].Value, level);
        }

        /// <summary>
        /// The canonical units of a document with the segmentation of the B.11 generator (I-62-F3/gen-manifest.py, U1): each section by its anchor, and
        /// inside it every fenced block (#code), table row with its header (#row), list item (#li) and paragraph (#p), as 1-based line ranges. A row with
        /// its header is returned as the header line plus the row line.
        /// </summary>
        public static Dictionary<string, List<int>> Units(string text)
        {
            var lines = Lines(text);
            var sections = new List<(string Anchor, int Level, int Start)>();
            var fence = false;
            for (var k = 0; k < lines.Length; k++)
            {
                if (lines[k].TrimStart().StartsWith("```", StringComparison.Ordinal))
                {
                    fence = !fence;
                    continue;
                }

                var (anchor, level) = fence ? (null, 0) : AnchorOf(lines[k]);
                if (anchor != null)
                {
                    sections.Add((anchor, level, k));
                }
            }

            var units = new Dictionary<string, List<int>>(StringComparer.Ordinal);
            for (var s = 0; s < sections.Count; s++)
            {
                var (anchor, level, start) = sections[s];
                var bodyEnd = s + 1 < sections.Count ? sections[s + 1].Start : lines.Length;
                var sectionEnd = sections.Skip(s + 1).Where(x => x.Level <= level).Select(x => x.Start).DefaultIfEmpty(lines.Length).First();
                units[anchor] = Enumerable.Range(start + 1, sectionEnd - start).ToList();
                var counters = new Dictionary<string, int> { ["row"] = 0, ["li"] = 0, ["p"] = 0, ["code"] = 0 };
                var k = start + 1;
                while (k < bodyEnd)
                {
                    var line = lines[k];
                    if (line.Trim().Length == 0)
                    {
                        k++;
                        continue;
                    }

                    int j;
                    if (line.TrimStart().StartsWith("```", StringComparison.Ordinal))
                    {
                        j = k + 1;
                        while (j < bodyEnd && !lines[j].TrimStart().StartsWith("```", StringComparison.Ordinal))
                        {
                            j++;
                        }

                        units[anchor + "#code" + ++counters["code"]] = Enumerable.Range(k + 1, Math.Min(j, bodyEnd - 1) - k + 1).ToList();
                        k = j + 1;
                        continue;
                    }

                    if (line.StartsWith("|", StringComparison.Ordinal))
                    {
                        j = k;
                        while (j < bodyEnd && lines[j].StartsWith("|", StringComparison.Ordinal))
                        {
                            j++;
                        }

                        for (var r = k + 2; r < j; r++)
                        {
                            units[anchor + "#row" + ++counters["row"]] = new List<int> { k + 1, r + 1 };
                        }

                        k = j;
                        continue;
                    }

                    var item = Regex.IsMatch(line, @"^(- |\d+\. )");
                    j = k + 1;
                    while (j < bodyEnd && lines[j].Trim().Length > 0 && !Regex.IsMatch(lines[j], item ? @"^(- |\d+\. |\||#)" : @"^(- |\d+\. |\||```)"))
                    {
                        j++;
                    }

                    units[anchor + (item ? "#li" + ++counters["li"] : "#p" + ++counters["p"])] = Enumerable.Range(k + 1, j - k).ToList();
                    k = j;
                }
            }

            return units;
        }

        /// <summary>
        /// The premise envelope (point 2): the complete structural unit that contains the proposition (a list item with its continuation lines and its
        /// parent item, a whole paragraph, a table row with its header, a whole code block) and the chain of headings that governs it. Null when the
        /// location is ambiguous (the quote appears more than once and the range does not single it out) or the quote is not on those lines.
        /// </summary>
        public static HashSet<int>? Envelope(string text, PremiseRef premise)
        {
            var lines = Lines(text);
            var quote = Regex.Replace(premise.Quote, @"\s+", " ").Trim();
            string Range(int a, int b) => Regex.Replace(string.Join(" ", lines.Skip(a - 1).Take(b - a + 1)), @"\s+", " ");
            if (premise.LineStart < 1 || premise.LineEnd > lines.Length || !Range(premise.LineStart, premise.LineEnd).Contains(quote, StringComparison.Ordinal))
            {
                return null;
            }

            // The range must single out one occurrence of the proposition.
            var inRange = Range(premise.LineStart, premise.LineEnd);
            if (inRange.IndexOf(quote, StringComparison.Ordinal) != inRange.LastIndexOf(quote, StringComparison.Ordinal))
            {
                return null;
            }

            var units = Units(text).Where(u => u.Key.Contains('#'));
            var envelope = new HashSet<int>();
            foreach (var (id, unitLines) in units)
            {
                if (unitLines.Any(n => n >= premise.LineStart && n <= premise.LineEnd))
                {
                    envelope.UnionWith(unitLines);
                    if (id.Contains("#li", StringComparison.Ordinal) && lines[unitLines[0] - 1].StartsWith(" ", StringComparison.Ordinal))
                    {
                        var parent = Enumerable.Range(1, unitLines[0] - 1).Reverse().FirstOrDefault(n => Regex.IsMatch(lines[n - 1], @"^(- |\d+\. )"));
                        if (parent > 0)
                        {
                            envelope.Add(parent);
                        }
                    }
                }
            }

            // The governing headings (the scope).
            var level = 7;
            for (var n = premise.LineStart - 1; n >= 1; n--)
            {
                var h = MarkdownSections.HeadingLevel(lines[n - 1]);
                if (h > 0 && h < level)
                {
                    envelope.Add(n);
                    level = h;
                }
            }

            return envelope;
        }

        // ------------------------------------------------------------------ closure over the manifest and canonical resolution

        /// <summary>
        /// NormativeDependencyClosure (point 6) from <paramref name="start"/> over the declared control edges of the manifest: a queue, a visited set,
        /// cycles ending on it, duplicates merged by identity. A unit without entry or with <c>Complete</c> = false is INCOMPLETE_METADATA; an edge to a
        /// unit of a document that does not exist is UNRESOLVED_REFERENCE; a whole document is followed through its bounded entry set or composite rule
        /// and is WHOLE_DOCUMENT_UNBOUNDED without them; a proposed target resolves to its design unit in the Proposal (§3.1).
        /// </summary>
        public static (List<UnitRef> Units, List<string> Problems) Closure(JsonObject manifest, IEnumerable<UnitRef> start, Func<string, bool> documentExists,
            string proposal)
        {
            var entries = ((JsonArray)manifest["Entries"]!).OfType<JsonObject>()
                .GroupBy(e => new UnitRef(J.S(e, "Source.Document")!, J.S(e, "Source.UnitId")!)).ToDictionary(g => g.Key, g => g.First());
            var visited = new List<UnitRef>();
            var seen = new HashSet<UnitRef>();
            var problems = new List<string>();
            var queue = new Queue<UnitRef>(start);
            while (queue.Count > 0)
            {
                var u = queue.Dequeue();
                if (!seen.Add(u))
                {
                    continue;
                }

                visited.Add(u);
                if (!entries.TryGetValue(u, out var entry) || entry["Complete"]?.GetValue<bool>() != true)
                {
                    problems.Add("INCOMPLETE_METADATA: " + u.Document + " " + u.UnitId);
                    continue;
                }

                foreach (var d in (entry["DependsOn"] as JsonArray)?.OfType<JsonObject>() ?? Enumerable.Empty<JsonObject>())
                {
                    var kind = J.S(d, "Target.Kind");
                    if (kind == "UNIT")
                    {
                        var t = new UnitRef(J.S(d, "Target.Unit.Document")!, J.S(d, "Target.Unit.UnitId")!);
                        if (!documentExists(t.Document))
                        {
                            problems.Add("UNRESOLVED_REFERENCE: " + t.Document + " " + t.UnitId);
                            continue;
                        }

                        queue.Enqueue(t);
                    }
                    else if (kind == "PROPOSED")
                    {
                        queue.Enqueue(new UnitRef(proposal, J.S(d, "Target.Proposed.DesignSource")!));
                    }
                    else if (kind == "WHOLE_DOCUMENT")
                    {
                        var whole = d["Target"]?["WholeDocument"] as JsonObject;
                        var set = (whole?["EntrySet"] as JsonArray)?.Select(x => x?.GetValue<string>()).OfType<string>().ToList() ?? new List<string>();
                        if (whole == null || set.Count == 0)
                        {
                            problems.Add("WHOLE_DOCUMENT_UNBOUNDED: " + (J.S(whole, "Document") ?? "?"));
                            continue;
                        }

                        foreach (var id in set)
                        {
                            queue.Enqueue(new UnitRef(J.S(whole, "Document")!, id));
                        }
                    }
                }
            }

            return (visited, problems);
        }

        /// <summary>
        /// Canonical resolution of a section reference (point 2): qualified → that document; unqualified in the same document → that document; a proposed
        /// section mapped in §3.1 → its design unit; otherwise, a candidate in another document of the closure → AMBIGUOUS_REFERENCE (no namespace, even
        /// with one candidate), and none → UNRESOLVED_REFERENCE.
        /// </summary>
        public static (string Status, UnitRef? Target) Resolve(string section, string? qualifier, string sourceDocument, IReadOnlyDictionary<string, string> documents,
            IReadOnlyDictionary<string, string> proposedDesignSource, string proposal)
        {
            bool Has(string doc) => documents.TryGetValue(doc, out var t) && Units(t).ContainsKey(section);
            if (qualifier != null)
            {
                return Has(qualifier) ? ("RESOLVED", new UnitRef(qualifier, section))
                    : proposedDesignSource.TryGetValue(section, out var ds1) ? ("RESOLVED", new UnitRef(proposal, ds1)) : ("UNRESOLVED_REFERENCE", null);
            }

            if (Has(sourceDocument))
            {
                return ("RESOLVED", new UnitRef(sourceDocument, section));
            }

            if (proposedDesignSource.TryGetValue(section, out var ds))
            {
                return ("RESOLVED", new UnitRef(proposal, ds));
            }

            return documents.Keys.Any(d => d != sourceDocument && Has(d)) ? ("AMBIGUOUS_REFERENCE", null) : ("UNRESOLVED_REFERENCE", null);
        }

        // ------------------------------------------------------------------ independence

        /// <summary>
        /// The independence of one premise (point 3) with the inputs in DEGRADED_BOUNDED: INVALID_PREMISE when its envelope or a unit of its closure
        /// overlaps a span or was not visible, or its location is ambiguous; UNKNOWN when a dependency did not resolve deterministically; INDEPENDENT
        /// otherwise. <paramref name="start"/> are the canonical units of the envelope (the manifest units the proposition belongs to).
        /// </summary>
        public static string Independence(PremiseRef premise, IReadOnlyDictionary<string, string> canonical, IReadOnlyList<DegradedSpan> spans,
            Func<string, int, bool> visible, JsonObject manifest, IEnumerable<UnitRef> start, string proposal)
        {
            if (!canonical.TryGetValue(premise.Path, out var text))
            {
                return "UNKNOWN";
            }

            var envelope = Envelope(text, premise);
            if (envelope == null)
            {
                return "INVALID_PREMISE";
            }

            bool Faithful(string path, IEnumerable<int> lines) => lines.All(n => visible(path, n) && !spans.Any(s => s.Path == path && s.Line == n));
            if (!Faithful(premise.Path, envelope))
            {
                return "INVALID_PREMISE";
            }

            var (units, problems) = Closure(manifest, start, canonical.ContainsKey, proposal);
            foreach (var u in units)
            {
                if (!canonical.TryGetValue(u.Document, out var doc) || !Units(doc).TryGetValue(u.UnitId, out var lines))
                {
                    problems.Add("UNRESOLVED_REFERENCE: " + u.Document + " " + u.UnitId);
                    continue;
                }

                if (!Faithful(u.Document, lines))
                {
                    return "INVALID_PREMISE";
                }
            }

            return problems.Count > 0 ? "UNKNOWN" : "INDEPENDENT";
        }

        /// <summary>
        /// The ingestion of a reviewed result by its fidelity (§20.3.3, table and point 6): FAITHFUL or FAITHFUL_NORMALIZED → normal; DEGRADED_BOUNDED →
        /// each finding by its premises (all INDEPENDENT → ingested; otherwise UNACCREDITED, never changing a lineage); DEGRADED_UNBOUNDED, UNVERIFIED, or a
        /// finding without premises → INPUT_FIDELITY_INVALID for the whole result.
        /// </summary>
        public static (string Result, IReadOnlyList<string> Unaccredited) Ingest(string status, IReadOnlyDictionary<string, IReadOnlyList<string>> findingPremiseOutcomes)
        {
            if (status is "FAITHFUL" or "FAITHFUL_NORMALIZED")
            {
                return ("NORMAL", new List<string>());
            }

            if (status != "DEGRADED_BOUNDED" || findingPremiseOutcomes.Values.Any(p => p.Count == 0))
            {
                return ("INPUT_FIDELITY_INVALID", findingPremiseOutcomes.Keys.ToList());
            }

            var unaccredited = findingPremiseOutcomes.Where(f => f.Value.Any(o => o != "INDEPENDENT")).Select(f => f.Key).OrderBy(x => x, StringComparer.Ordinal).ToList();
            return ("BOUNDED", unaccredited);
        }
    }
}
