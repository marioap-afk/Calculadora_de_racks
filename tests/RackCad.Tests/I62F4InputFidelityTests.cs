#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-62 F4 (slice F4-G), C-42 in its F4 part: the fidelity of the canonical inputs (<see cref="InputFidelity"/>; Proposal V14 §20.3.3) over a
    /// synthetic corpus — the preflight (1)-(4), the bounded invalidation by finding with premise envelopes (a)-(g), the normative dependency closure
    /// over a manifest (h)-(n), the delivered representation (o), a context compaction (p), the canonical and bounded graph (q)-(z), and the reconstruction from custody (7). The
    /// orchestration cases (5) and (8) are in <see cref="I62F4OrchestrationMcTests"/>.
    /// </summary>
    public class I62F4InputFidelityTests
    {
        private const string A = "docs/initiatives/U-proposal.md";
        private const string B = "docs/U-b.md";
        private const string C = "docs/U-c.md";

        private static readonly string TextA = string.Join("\n", new[]
        {
            "# U", "", "## 1. Reglas", "", "La sesión NO debe publicar un Q0 sin las dos aceptaciones.", "", "| Id | Condición | Efecto |", "|---|---|---|",
            "| R-1 | contador ≤ tope | continúa |", "| R-2 | contador ≠ tope | STOP |", "", "- Primer elemento: x → y.", "  continuación del primero.",
            "- Segundo elemento ∈ conjunto.", "", "## 2. Alcance de las reglas", "", "Las reglas de §1 aplican solo si §3 lo permite.", "", "## 3. Permiso", "",
            "El permiso se concede según la sección 3.", "",
        });

        private static readonly string TextB = "# B\n\n## 1. Entrada\n\nLa entrada acotada de B.\n\n## 2. Otra\n\nUna cláusula de B sin relación (cita histórica de C §1).\n";
        private static readonly string TextC = "# C\n\n## 1. Base\n\nLa base de C.\n";

        private static Dictionary<string, string> Corpus => new Dictionary<string, string> { [A] = TextA, [B] = TextB, [C] = TextC };

        private static string[] Lines(string t) => t.TrimEnd('\n').Split('\n');

        /// <summary>The delivered representation: every file read whole, with some lines replaced (or removed) as the runtime delivered them.</summary>
        private static List<DeliveredRead> Delivered(params (string Path, int Line, string? Text)[] changes)
        {
            var reads = new List<DeliveredRead>();
            foreach (var (path, text) in Corpus)
            {
                var lines = Lines(text);
                var map = Enumerable.Range(1, lines.Length).ToDictionary(n => n, n => lines[n - 1]);
                foreach (var (p, n, t) in changes.Where(c => c.Path == path))
                {
                    if (t == null)
                    {
                        map.Remove(n);
                    }
                    else
                    {
                        map[n] = t;
                    }
                }

                reads.Add(new DeliveredRead(path, 1, lines.Length, map));
            }

            return reads;
        }

        private static JsonObject Unit(string doc, string id) => new JsonObject
            { ["Document"] = doc, ["UnitId"] = id, ["Anchor"] = id.Split('#')[0], ["Revision"] = new JsonObject { ["Commit"] = new string('0', 40), ["Blob"] = new string('1', 40) } };

        private static JsonObject Edge(string doc, string id) => new JsonObject
            { ["Target"] = new JsonObject { ["Kind"] = "UNIT", ["Unit"] = Unit(doc, id), ["Proposed"] = null, ["WholeDocument"] = null }, ["EdgeKind"] = "SUBJECT_TO" };

        private static JsonObject Whole(string doc, params string[] entrySet) => new JsonObject
        {
            ["Target"] = new JsonObject { ["Kind"] = "WHOLE_DOCUMENT", ["Unit"] = null, ["Proposed"] = null,
                ["WholeDocument"] = entrySet.Length == 0 ? null : new JsonObject { ["Document"] = doc, ["Class"] = "BOUNDED_ENTRY_SET",
                    ["EntrySet"] = new JsonArray(entrySet.Select(e => (JsonNode)e).ToArray()), ["CompositeRuleRef"] = null } },
            ["EdgeKind"] = "SUBJECT_TO",
        };

        private static JsonObject Manifest(params (string Doc, string Id, JsonObject[] DependsOn, bool Complete)[] entries) => new JsonObject
        {
            ["Schema"] = "rackcad-normative-dependency-manifest/v1",
            ["Entries"] = new JsonArray(entries.Select(e => (JsonNode)new JsonObject
                { ["Source"] = Unit(e.Doc, e.Id), ["DependsOn"] = new JsonArray(e.DependsOn.Select(d => (JsonNode)d).ToArray()), ["Complete"] = e.Complete }).ToArray()),
        };

        // §2#p1 is subject to §1 and only applies if §3#p1 allows it; §1 contains its units; leaves have no control dependency.
        private static JsonObject BaseManifest(params (string Doc, string Id, JsonObject[] DependsOn, bool Complete)[] extra) => Manifest(new[]
        {
            (A, "§1#p1", Array.Empty<JsonObject>(), true), (A, "§1#row1", Array.Empty<JsonObject>(), true), (A, "§1#row2", Array.Empty<JsonObject>(), true),
            (A, "§1#li1", Array.Empty<JsonObject>(), true), (A, "§1#li2", Array.Empty<JsonObject>(), true), (A, "§3#p1", Array.Empty<JsonObject>(), true),
            (A, "§2#p1", new[] { Edge(A, "§3#p1") }, true),
        }.Where(e => !extra.Any(x => x.Doc == e.Item1 && x.Id == e.Item2)).Concat(extra).ToArray());

        private static PremiseRef Premise(int line, string quote, string section = "§1") => new PremiseRef(A, section, line, line, quote);

        private static string Independence(PremiseRef premise, List<DeliveredRead> delivered, JsonObject manifest, params UnitRef[] start)
        {
            var (_, spans, visible) = InputFidelity.Compare(Corpus, delivered);
            return InputFidelity.Independence(premise, Corpus, spans, visible, manifest, start, A);
        }

        [Fact]
        public void I62_C42_1_4_ThePreflightKeepsEveryNonAsciiCharacterOrStopsTheLaunch()
        {
            var canonical = new Dictionary<string, string> { [A] = TextA };
            var (faithful, classes) = InputFidelity.Preflight(canonical, new Dictionary<string, string?> { [A] = TextA });
            Assert.Equal("FAITHFUL", faithful);
            foreach (var ch in "≤≠→∈óú§")
            {
                Assert.Contains(classes, c => c.Character == ch && c.Canonical == c.Transport && c.Canonical > 0);
            }

            Assert.Equal("FAITHFUL_NORMALIZED", InputFidelity.Preflight(canonical, new Dictionary<string, string?> { [A] = TextA.Replace("\n", "\r\n", StringComparison.Ordinal) }).Status);
            var lossy = new string(TextA.Select(c => c > 127 ? '?' : c).ToArray());
            var (status, lost) = InputFidelity.Preflight(canonical, new Dictionary<string, string?> { [A] = lossy });
            Assert.Equal("NOT_ACCREDITED", status);
            Assert.False(InputFidelity.MayLaunch(status));
            Assert.Contains(lost, c => c.Character == '≤' && c.Transport == 0);
        }

        [Fact]
        public void I62_C42_a_f_g_AFindingIsIndependentOnlyWhenItsWholeEnvelopeIsFaithful()
        {
            var manifest = BaseManifest();
            var start = new UnitRef(A, "§1#p1");
            // (a)/(f) an exact premise outside an unrelated degraded span.
            var unrelated = Delivered((A, 22, "El permiso se concede segun la seccion 3."));
            Assert.Equal("INDEPENDENT", Independence(Premise(5, "La sesión NO debe publicar un Q0 sin las dos aceptaciones."), unrelated, manifest, start));
            // (b) the quote is intact but the preceding negation is degraded.
            var negation = Delivered((A, 5, "La sesión debe publicar un Q0 sin las dos aceptaciones."));
            Assert.Equal("INVALID_PREMISE", Independence(Premise(5, "debe publicar un Q0 sin las dos aceptaciones."), negation, manifest, start));
            Assert.Equal("NEGATION", InputFidelity.Compare(Corpus, negation).Spans.Single().Class);
            // (c) an operator degraded outside the quote, in the same row; (d) the cell intact but the column label degraded.
            Assert.Equal("INVALID_PREMISE", Independence(Premise(9, "continúa"), Delivered((A, 9, "| R-1 | contador = tope | continúa |")), manifest, new UnitRef(A, "§1#row1")));
            Assert.Equal("INVALID_PREMISE", Independence(Premise(9, "continúa"), Delivered((A, 7, "| Id | Condicion | Efecto |")), manifest, new UnitRef(A, "§1#row1")));
            // (e) the title that sets the scope, degraded.
            Assert.Equal("INVALID_PREMISE", Independence(Premise(18, "Las reglas de §1 aplican solo si §3 lo permite.", "§2"), Delivered((A, 16, "## 2. Alcance")),
                manifest, new UnitRef(A, "§2#p1")));
            // (g) a finding without premises: independence cannot be established for the review → the whole result is INPUT_FIDELITY_INVALID.
            Assert.Equal("INPUT_FIDELITY_INVALID", InputFidelity.Ingest("DEGRADED_BOUNDED", new Dictionary<string, IReadOnlyList<string>>
                { ["A62-X-01"] = new[] { "INDEPENDENT" }, ["A62-X-02"] = Array.Empty<string>() }).Result);
            // An ambiguous location (the range does not single out the proposition) is never independent.
            Assert.Equal("INVALID_PREMISE", Independence(new PremiseRef(A, "§1", 9, 10, "contador"), unrelated, manifest, start));
        }

        [Fact]
        public void I62_C42_h_n_TheClosureFollowsOnlyDeclaredControlEdgesAndAnyDegradedUnitInItInvalidates()
        {
            var premise = Premise(18, "Las reglas de §1 aplican solo si §3 lo permite.", "§2");
            var start = new UnitRef(A, "§2#p1");
            // (i) the clause it depends on is faithful; (h) its proposition and title are faithful but its body is degraded.
            Assert.Equal("INDEPENDENT", Independence(premise, Delivered(), BaseManifest(), start));
            Assert.Equal("INVALID_PREMISE", Independence(premise, Delivered((A, 22, "El permiso se deniega según la sección 3.")), BaseManifest(), start));
            // (j) a chain A → B → C with C degraded; (k) a cycle with both faithful terminates.
            var chain = BaseManifest((A, "§3#p1", new[] { Edge(A, "§1#li2") }, true));
            Assert.Equal("INVALID_PREMISE", Independence(premise, Delivered((A, 14, "- Segundo elemento = conjunto.")), chain, start));
            var cycle = BaseManifest((A, "§3#p1", new[] { Edge(A, "§2#p1") }, true));
            Assert.Equal("INDEPENDENT", Independence(premise, Delivered(), cycle, start));
            // (l) an edge to a document that does not exist, and a unit without entry: UNKNOWN, not credited.
            Assert.Equal("UNKNOWN", Independence(premise, Delivered(), BaseManifest((A, "§3#p1", new[] { Edge("docs/no-existe.md", "§1#p1") }, true)), start));
            Assert.Equal("UNKNOWN", Independence(premise, Delivered(), BaseManifest((A, "§3#p1", Array.Empty<JsonObject>(), false)), start));
            // (m) a degraded clause outside the closure leaves it valid.
            Assert.Equal("INDEPENDENT", Independence(premise, Delivered((A, 12, "- Primer elemento: x y.")), BaseManifest(), start));
            // (n) a table rule that refers to an enumeration elsewhere, degraded.
            var rule = BaseManifest((A, "§1#row2", new[] { Edge(A, "§1#li2") }, true));
            Assert.Equal("INVALID_PREMISE", Independence(Premise(10, "STOP"), Delivered((A, 14, "- Segundo elemento ? conjunto.")), rule, new UnitRef(A, "§1#row2")));
        }

        [Fact]
        public void I62_C42_o_7_TheEvidenceDescribesWhatTheReviewerReceivedAndASuccessorRecomputesIt()
        {
            // (o) the reviewer's tool truncated lines 12-14: STRUCTURE spans; a premise there is INVALID_PREMISE; with only a full capture, UNVERIFIED.
            var truncated = Delivered((A, 12, null), (A, 13, null), (A, 14, null));
            var (status, spans, _) = InputFidelity.Compare(Corpus, truncated);
            Assert.Equal("DEGRADED_BOUNDED", status);
            Assert.Equal(new[] { 12, 13, 14 }, spans.Where(s => s.Class == "STRUCTURE").Select(s => s.Line));
            Assert.Equal("INVALID_PREMISE", Independence(Premise(12, "- Primer elemento: x → y."), truncated, BaseManifest(), new UnitRef(A, "§1#li1")));
            Assert.Equal("UNVERIFIED", InputFidelity.Compare(Corpus, null).Status);
            // A line omitted by one read and delivered whole by another was available.
            var reread = truncated.Append(new DeliveredRead(A, 12, 14, new Dictionary<int, string> { [12] = Lines(TextA)[11], [13] = Lines(TextA)[12], [14] = Lines(TextA)[13] })).ToList();
            Assert.Equal("FAITHFUL", InputFidelity.Compare(Corpus, reread).Status);
            // (7) a successor recomputes the same status and the same unaccredited findings from the custodied representation.
            var again = InputFidelity.Compare(Corpus, Delivered((A, 12, null), (A, 13, null), (A, 14, null)));
            Assert.Equal(status, again.Status);
            Assert.Equal(spans, again.Spans);
            var outcomes = new Dictionary<string, IReadOnlyList<string>> { ["A62-X-01"] = new[] { "INVALID_PREMISE" }, ["A62-X-02"] = new[] { "INDEPENDENT" } };
            Assert.Equal(InputFidelity.Ingest(status, outcomes).Result, InputFidelity.Ingest(again.Status, outcomes).Result);
            Assert.Equal(InputFidelity.Ingest(status, outcomes).Unaccredited, InputFidelity.Ingest(again.Status, outcomes).Unaccredited);
            Assert.Equal(new[] { "A62-X-01" }, InputFidelity.Ingest(status, outcomes).Unaccredited);
            Assert.Equal("INPUT_FIDELITY_INVALID", InputFidelity.Ingest("UNVERIFIED", outcomes).Result);
            Assert.Equal("INPUT_FIDELITY_INVALID", InputFidelity.Ingest("DEGRADED_UNBOUNDED", outcomes).Result);
            Assert.Equal("NORMAL", InputFidelity.Ingest("FAITHFUL_NORMALIZED", outcomes).Result);
        }

        [Fact]
        public void I62_C42_p_ACompactionIsRecordedAndDoesNotInvalidateByItselfButItsSummaryIsNeverAPremise()
        {
            // The reviewer's context was compacted after reading A; the runtime evidence records it. The comparison reads only what was delivered, so the
            // compaction by itself changes neither the status nor the spans.
            var runtime = new JsonObject { ["ObservedActor"] = "other", ["Compactions"] = new JsonArray(new JsonObject { ["AfterRead"] = A, ["Summary"] = "ENCRYPTED" }) };
            Assert.Single((JsonArray)runtime["Compactions"]!);
            Assert.Equal("FAITHFUL", InputFidelity.Compare(Corpus, Delivered()).Status);
            var degraded = Delivered((A, 13, null));
            var (status, spans, _) = InputFidelity.Compare(Corpus, degraded);
            Assert.Equal("DEGRADED_BOUNDED", status);

            // A premise used after the compaction is evaluated with the visible evidence like any other: independent when its envelope is faithful.
            Assert.Equal("INDEPENDENT", Independence(Premise(5, "La sesión NO debe publicar un Q0 sin las dos aceptaciones."), degraded, BaseManifest(), new UnitRef(A, "§1#p1")));

            // A premise that cites the compacted summary instead of the canonical text is not resolvable: UNKNOWN, and its finding is not accredited.
            var summary = new PremiseRef("<resumen de compactación>", "§1", 1, 1, "La sesión NO debe publicar un Q0");
            var (_, s, visible) = InputFidelity.Compare(Corpus, degraded);
            Assert.Equal("UNKNOWN", InputFidelity.Independence(summary, Corpus, s, visible, BaseManifest(), new[] { new UnitRef(A, "§1#p1") }, A));
            var outcomes = new Dictionary<string, IReadOnlyList<string>> { ["A62-X-01"] = new[] { "UNKNOWN" }, ["A62-X-02"] = new[] { "INDEPENDENT" } };
            Assert.Equal(new[] { "A62-X-01" }, InputFidelity.Ingest(status, outcomes).Unaccredited);
            Assert.Equal(spans, s);
        }

        [Fact]
        public void I62_C42_q_z_TheGraphIsCanonicalAndBoundedAndResolvesWithoutGuessing()
        {
            var premise = Premise(18, "Las reglas de §1 aplican solo si §3 lo permite.", "§2");
            var start = new UnitRef(A, "§2#p1");
            JsonObject WithB(JsonObject whole, params (string, string, JsonObject[], bool)[] extra) =>
                BaseManifest(new[] { (A, "§2#p1", new[] { whole }, true), (B, "§1#p1", new[] { Edge(C, "§1#p1") }, true), (B, "§2#p1", Array.Empty<JsonObject>(), true),
                    (C, "§1#p1", Array.Empty<JsonObject>(), true) }.Concat(extra).ToArray());

            // (q) A → whole document B → its entry unit B1 → C, with C degraded; (r) B2, outside the entry set, degraded, does not matter.
            Assert.Equal("INVALID_PREMISE", Independence(premise, Delivered((C, 5, "La base de X.")), WithB(Whole(B, "§1#p1")), start));
            Assert.Equal("INDEPENDENT", Independence(premise, Delivered((B, 9, "Una clausula de B.")), WithB(Whole(B, "§1#p1")), start));
            // (s) a whole document without an entry set or a composite rule: UNKNOWN, never expanded.
            Assert.Equal("UNKNOWN", Independence(premise, Delivered(), WithB(Whole(B)), start));
            // (x) a historical citation inside B is not a declared edge and never widens the closure.
            Assert.Equal("INDEPENDENT", Independence(premise, Delivered((C, 5, "La base de X.")), WithB(Whole(B, "§2#p1")), start));
            // (w) a cycle A → B → C → A is finite; (z) every canonical node faithful: credited.
            var cycle = WithB(Whole(B, "§1#p1"), (C, "§1#p1", new[] { Edge(A, "§2#p1") }, true));
            Assert.Equal("INDEPENDENT", Independence(premise, Delivered(), cycle, start));
            var credited = InputFidelity.Ingest("DEGRADED_BOUNDED", new Dictionary<string, IReadOnlyList<string>> { ["A62-X-01"] = new[] { "INDEPENDENT" } });
            Assert.Equal("BOUNDED", credited.Result);
            Assert.Empty(credited.Unaccredited);
            // (y) the manifest declares B → C but C does not exist.
            var noC = new Dictionary<string, string> { [A] = TextA, [B] = TextB };
            var (_, spans, visible) = InputFidelity.Compare(noC, Delivered().Where(d => d.Path != C).ToList());
            Assert.Equal("UNKNOWN", InputFidelity.Independence(premise, noC, spans, visible, WithB(Whole(B, "§1#p1")), new[] { start }, A));

            // (t) a proposed section resolves to the design unit of the Proposal; (u) an unqualified «§16.3» with candidates elsewhere is ambiguous;
            // (v) the same reference inside its own document resolves there.
            var documents = new Dictionary<string, string>
            {
                ["docs/AUTOMATION_PLAN.md"] = "# P\n\n## 16. Ejecución\n\n### 16.3 Lectura\n\nTexto.\n",
                ["docs/WORKFLOW.md"] = "# W\n\n## 16. Otra\n\n### 16.3 Otra lectura\n\nTexto.\n",
                [A] = TextA,
            };
            var proposed = new Dictionary<string, string> { ["§16.13"] = "E.3" };
            Assert.Equal(("RESOLVED", new UnitRef(A, "E.3")), InputFidelity.Resolve("§16.13", null, A, documents, proposed, A));
            Assert.Equal("AMBIGUOUS_REFERENCE", InputFidelity.Resolve("§16.3", null, A, documents, proposed, A).Status);
            Assert.Equal(("RESOLVED", new UnitRef("docs/AUTOMATION_PLAN.md", "§16.3")), InputFidelity.Resolve("§16.3", null, "docs/AUTOMATION_PLAN.md", documents, proposed, A));
            Assert.Equal(("RESOLVED", new UnitRef("docs/WORKFLOW.md", "§16.3")), InputFidelity.Resolve("§16.3", "docs/WORKFLOW.md", A, documents, proposed, A));
            Assert.Equal("UNRESOLVED_REFERENCE", InputFidelity.Resolve("§99", null, A, documents, proposed, A).Status);
            var proposedEdge = new JsonObject
            {
                ["Target"] = new JsonObject { ["Kind"] = "PROPOSED", ["Unit"] = null, ["WholeDocument"] = null,
                    ["Proposed"] = new JsonObject { ["FutureDocument"] = "docs/AUTOMATION_PLAN.md", ["FutureAnchor"] = "§16.13", ["DesignSource"] = "§3#p1", ["State"] = "PROPOSED", ["MaterializedAnchor"] = null } },
                ["EdgeKind"] = "SUBJECT_TO",
            };
            var (units, problems) = InputFidelity.Closure(BaseManifest((A, "§2#p1", new[] { proposedEdge }, true)), new[] { start }, Corpus.ContainsKey, A);
            Assert.Contains(new UnitRef(A, "§3#p1"), units);
            Assert.Empty(problems);
        }

        [Fact]
        public void I62_C42_TheCanonicalUnitsAreTheSegmentationOfTheB11Generator()
        {
            var units = InputFidelity.Units(TextA);
            Assert.Equal(new[] { 5 }, units["§1#p1"]);
            Assert.Equal(new[] { 7, 9 }, units["§1#row1"]);
            Assert.Equal(new[] { 7, 10 }, units["§1#row2"]);
            Assert.Equal(new[] { 12, 13 }, units["§1#li1"]);
            Assert.Equal(new[] { 14 }, units["§1#li2"]);
            Assert.Equal(new[] { 18 }, units["§2#p1"]);

            // The real B.11 manifest of V14 projects onto the frozen Proposal: every complete unit of the manifest names a unit the segmentation finds.
            var manifest = (JsonObject)JsonNode.Parse(I62Repo.ReadText("docs/initiatives/I-62-normative-dependency-manifest.json"))!;
            var proposal = InputFidelity.Units(I62Repo.ReadText("docs/initiatives/I-62-proposal-v14.md").Replace("\r\n", "\n", StringComparison.Ordinal));
            var generated = ((JsonArray)manifest["Entries"]!).OfType<JsonObject>()
                .Where(e => J.S(e, "Source.Document") == "docs/initiatives/I-62-proposal-v14.md" && J.S(e, "Source.UnitId")!.Contains("#p", StringComparison.Ordinal))
                .Select(e => J.S(e, "Source.UnitId")!).Take(200).ToList();
            Assert.NotEmpty(generated);
            Assert.All(generated, id => Assert.True(proposal.ContainsKey(id), id));
        }
    }
}
