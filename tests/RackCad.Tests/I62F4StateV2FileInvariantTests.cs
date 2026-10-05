#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json.Nodes;
using Xunit;
using static RackCad.Tests.StateV2Samples;

namespace RackCad.Tests
{
    /// <summary>
    /// I-62 F4 (slice F4-B), obligation C-18 of the frozen Proposal V14 (Anexo C), file part: the shape of B.8.1 (with the fields of the AGREED A-1) and
    /// the file invariants I-S01..I-S17 of B.8.4, evaluated by the production validator on synthetic points whose StateRefs resolve in the tree of
    /// their commit (I-S13 against the test tree). Every kind of durable point is a positive case; each invariant has a negative case that breaks
    /// exactly what it guards; disabling an invariant in memory lets its negative case through (mutation check). The real state files with
    /// <c>schema: rackcad-automation-state/v2</c> are validated against the working tree (today none: the empty set passes). History invariants
    /// (I-P03, I-P08, I-H01, I-H02) are never evaluated here.
    /// </summary>
    public class I62F4StateV2FileInvariantTests
    {
        private static readonly StateV2Validator Validator = new StateV2Validator();

        // (case, record_version of the point to mutate, mutation, invariants that must be reported, invariants that may also be reported)
        public static IEnumerable<object[]> Negatives()
        {
            yield return Case("B.8.1 unknown key", 5, p => Y.M(p.State, "custody")!.Add("owner", "x"), "B.8.1");
            yield return Case("B.8.1 missing field", 5, p => Y.M(p.State, "custody")!.Remove("chains"), "B.8.1");
            yield return Case("B.8.1 enum", 5, p => Y.M(p.State, "custody")!["point"] = "Q9", "B.8.1");
            yield return Case("B.8.1 rebase_history of A-1", 5, p => Y.M(p.State, "custody")!.Remove("rebase_history"), "B.8.1");
            yield return Case("B.8.1 architect_budgets of A-1", 5, p => Y.M(p.State, "orchestration")!.Remove("architect_budgets"), "B.8.1");
            yield return Case("I-S02 attempts", 5, p => Y.M(p.State, "automation_state")!["attempts"] = -1L, "I-S02");
            yield return Case("I-S02 gate", 5, p => Y.M(p.State, "automation_state")!["gate"] = "maybe", "I-S02");
            yield return Case("I-S01 claim_id", 5, p => Y.M(p.State, "protocol.basis")!["claim_id"] = "11111111-2222-3333-4444-555555555555", "I-S01");
            yield return Case("I-S01 BOOTSTRAP accepted", 1, p => Y.M(p.State, "custody.principal.acceptance")!["state"] = "ACCEPTED", "I-S01", "I-S14", "I-S16");
            yield return Case("I-S03 Q0 without OPENABLE", 3, p => Y.M(p.State, "custody.window")!["state"] = "CLOSED", "I-S03");
            yield return Case("I-S03 Q0 without task_intent", 3, p => Y.M(p.State, "custody")!["task_intent"] = null, "I-S03");
            yield return Case("I-S04 QH held", 7, p => Y.M(p.State, "custody.principal")!["state"] = "HELD", "I-S04");
            yield return Case("I-S05 attempt", 3, p => Y.M(p.State, "custody.task_intent")!["attempt"] = 1L, "I-S05");
            yield return Case("I-S05 correction without launch", 3, p =>
            {
                Y.M(p.State, "custody.task_intent")!["kind"] = "CORRECTION";
            }, "I-S05");
            yield return Case("I-S06 no controller", 3, p => ((List<object?>)Y.M(p.State, "custody.task_intent")!["planned_roles"]!).RemoveAt(0), "I-S06");
            yield return Case("I-S06 duplicated role", 3, p => ((List<object?>)Y.M(p.State, "custody.task_intent")!["planned_roles"]!)
                .Add(M(("role", "WORKER"), ("binding", null))), "I-S06");
            yield return Case("I-S07 seq", 4, p => Y.M(p.State, "custody.window")!["seq"] = 2L, "I-S07");
            yield return Case("I-S08 VERIFIED without sha", 4, p => Y.M(p.State, "custody.last_window")!["verified_sha"] = null, "I-S08");
            yield return Case("I-S08 ABANDONED without reconstruction", 4, p =>
            {
                var lw = Y.M(p.State, "custody.last_window")!;
                lw["closure"] = "ABANDONED";
                lw["closure_source"] = "COORDINATOR";
                lw["verified_sha"] = null;
            }, "I-S08");
            yield return Case("I-S09 duplicated chain", 4, p => Y.L(p.State, "custody.chains").Add(Clone((YamlMap)Y.L(p.State, "custody.chains")[0]!)), "I-S09");
            yield return Case("I-S09 RED without files", 4, p => ((List<object?>)((YamlMap)Y.L(p.State, "custody.chains")[0]!)["chain_red_files"]!).Clear(), "I-S09");
            yield return Case("I-S10 orphan unverified commit", 4, p => Y.L(p.State, "custody.unverified_commits").Add(M(("sha", Sha3), ("task_id", "T-77"),
                ("window_seq", 1L), ("reason", "JOURNAL_UNAVAILABLE"), ("superseded_by", null))), "I-S10");
            yield return Case("I-S11 reconstructed", 4, p => ((YamlMap)Y.L(p.State, "counters.invocations")[0]!)["reconstructed"] = true, "I-S11");
            yield return Case("I-S11 blocked_reruns duplicated", 4, p =>
            {
                var b = M(("task_id", "T-01"), ("phase", "WORK"), ("count", 1L));
                Y.L(p.State, "counters.blocked_reruns").Add(b);
                Y.L(p.State, "counters.blocked_reruns").Add(Clone(b));
            }, "I-S11");
            yield return Case("I-S12 seq from 2", 4, p => Y.L(p.State, "counters.correction_launches").Add(M(("seq", 2L), ("task_id", "T-01"), ("failure_class", "Tests"),
                ("correction_of_run_id", "R20261003T010101Z-ab12"), ("attempts_after", 0L), ("record_version", 3L))), "I-S12");
            yield return Case("I-S13 missing artifact", 5, p => ((InMemoryStateTree)p.Tree).Remove("docs/automation/evidence/I-99-agent/bootstrap/preflight.json"), "I-S13");
            yield return Case("I-S13 different blob", 5, p => ((InMemoryStateTree)p.Tree).Put("docs/automation/evidence/I-99-agent/T-01/w1/journal.json", "{\"Records\": 5}\n"),
                "I-S13");
            yield return Case("I-S14 binding role", 5, p =>
            {
                var tree = (InMemoryStateTree)p.Tree;
                Y.M(p.State, "custody.principal")!["binding"] = tree.PutJson("docs/automation/evidence/I-99-agent/bootstrap/binding-accepted.json", new JsonObject
                {
                    ["BindingId"] = PrincipalBindingId, ["Scope"] = "UNIT", ["Role"] = "ARCHITECT", ["Acceptance"] = new JsonObject { ["State"] = "ACCEPTED" },
                });
            }, "I-S14", "I-S16");
            yield return Case("I-S14 designation", 5, p => Y.M(p.State, "custody.principal")!["since_record_version"] = 4L, "I-S14");
            yield return Case("I-S15 Q0 pending", 3, p =>
            {
                Y.M(p.State, "custody.principal.acceptance")!["state"] = "PENDING";
                Y.M(p.State, "custody.principal.acceptance")!["decision"] = null;
            }, "I-S15", "I-S14");
            yield return Case("I-S16 decision without marker", 5, p =>
            {
                var refs = ((InMemoryStateTree)p.Tree).Put("docs/automation/decisions/I-99.md", Decisions("## G0\n\nI62-CLASSIFICATION: I62\n"));
                Y.M(p.State, "protocol.g0_acceptance")!["decision"] = Clone(refs);
                Y.M(p.State, "custody.principal.acceptance")!["decision"] = Clone(refs);
            }, "I-S16");
            yield return Case("I-S16 PENDING with decision", 1, p => Y.M(p.State, "protocol.g0_acceptance")!["decision"] =
                ((InMemoryStateTree)p.Tree).Put("docs/automation/decisions/I-99.md", Decisions(G0Entry)), "I-S16");
            yield return Case("I-S17 QU without kind", 5, p => Y.M(p.State, "custody")!["point_kind"] = null, "I-S17");
            yield return Case("I-S17 reconciliation of another point", 6, p => Y.M(p.State, "custody.last_rebase")!["record_version"] = 5L, "I-S17");
            yield return Case("I-S17/D2-9 history without the map", 6, p => Y.L(p.State, "custody.rebase_history").Clear(), "I-S17");
        }

        [Fact]
        public void I62_C18_EveryKindOfDurablePointOfAValidHistoryPassesEveryFileInvariant()
        {
            var history = CustodyHistory();
            Assert.Equal(new[] { "BOOTSTRAP", "QU", "Q0", "Q7", "QU", "QU", "QH", "QR" }, history.Select(p => Y.S(p.State, "custody.point")).ToArray());
            Assert.Contains(history, p => Y.S(p.State, "custody.point_kind") == "REBASE_RECONCILIATION");
            foreach (var point in history)
            {
                var violations = Validator.ValidateFile(point);
                Assert.True(violations.Count == 0, "rv " + Y.N(point.State, "custody.record_version") + ": " + string.Join("; ", violations));
            }
        }

        [Theory]
        [MemberData(nameof(Negatives))]
        public void I62_C18_EachFileInvariantRejectsItsNegativeCase(string name, long rv, Action<StatePoint> mutation, string[] required, string[] allowed)
        {
            var point = Copy(Point(rv));
            mutation(point);
            var reported = Validator.ValidateFile(point).Select(x => x.Invariant).Distinct().ToArray();

            Assert.True(required.All(reported.Contains), name + ": expected " + string.Join(", ", required) + ", got " + string.Join(", ", reported));
            Assert.True(reported.All(r => required.Contains(r) || allowed.Contains(r)), name + ": unexpected " + string.Join(", ", reported));
        }

        [Theory]
        [InlineData("I-S03", "I-S03 Q0 without OPENABLE")]
        [InlineData("I-S15", "I-S15 Q0 pending")]
        [InlineData("I-S13", "I-S13 missing artifact")]
        [InlineData("I-S16", "I-S16 decision without marker")]
        [InlineData("I-S17", "I-S17/D2-9 history without the map")]
        [InlineData("I-S08", "I-S08 VERIFIED without sha")]
        public void I62_C18_DisablingAFileInvariantInMemoryLetsItsNegativeCaseThrough(string rule, string caseName)
        {
            var c = Negatives().Single(x => (string)x[0] == caseName);
            var point = Copy(Point((long)c[1]));
            ((Action<StatePoint>)c[2])(point);

            Assert.Contains(Validator.ValidateFile(point), x => x.Invariant == rule);
            Assert.DoesNotContain(new StateV2Validator(new[] { rule }).ValidateFile(point), x => x.Invariant == rule);
        }

        [Fact]
        public void I62_C18_EveryRealV2StateFileOfTheTreePassesTheFileInvariants()
        {
            var files = Directory.GetFiles(I62Repo.FullPath("docs/automation/state"), "*.yml");
            Assert.True(files.Length > 0, "the state directory must be enumerated (selection > 0)");
            var problems = new List<string>();
            var v2 = 0;
            foreach (var file in files)
            {
                var text = File.ReadAllText(file);
                var header = YamlSubset.Read(text, YamlReadMode.Header);
                if (header.TryGetValue("schema", out var schema) && (string?)schema == StateV2Shape.Schema)
                {
                    v2++;
                    var violations = Validator.ValidateFile(new StatePoint(YamlSubset.Read(text), new WorkingTreeStateTree()));
                    problems.AddRange(violations.Select(x => Path.GetFileName(file) + ": " + x));
                }
            }

            Assert.True(problems.Count == 0, string.Join("; ", problems));
            Assert.True(v2 >= 0);
        }

        private static object[] Case(string name, long rv, Action<StatePoint> mutation, string required, params string[] allowed) =>
            new object[] { name, rv, mutation, new[] { required }, allowed };
    }
}
