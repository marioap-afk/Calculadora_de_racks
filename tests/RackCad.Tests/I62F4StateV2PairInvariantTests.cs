#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Xunit;
using static RackCad.Tests.StateV2Samples;

namespace RackCad.Tests
{
    /// <summary>
    /// I-62 F4 (slice F4-C), obligation C-18 of the frozen Proposal V14 (Anexo C), pair part: the pair invariants I-P01, I-P02, I-P04..I-P07 and
    /// I-P09..I-P12 of B.8.4 on synthetic consecutive points, with the reconciliation of the AGREED A-1 for the custody fields (D2-7, D2-9). The three
    /// kinds of positive pair the obligation names are GREEN at the same time: a QU ORDINARY that keeps I-P05; a QU REBASE_RECONCILIATION that keeps
    /// I-P05 by its exception and I-P10, with the RebaseMap of the test tree; and the Q7 that closes a window with a rebase inside it and keeps I-P12.
    /// Each pair invariant has a negative case, and the mutation classes of the obligation (invert I-P02, accept a decreasing <c>attempts</c>, allow
    /// two transitions of <c>g0_acceptance</c>) are detected. History invariants (I-P03, I-P08, I-H01, I-H02) are never evaluated here.
    /// </summary>
    public class I62F4StateV2PairInvariantTests
    {
        private static readonly StateV2Validator Validator = new StateV2Validator();

        private static PairContext WindowContext() => new PairContext(WindowRebaseMaps: new[] { WindowRebaseMapJson() });

        [Fact]
        public void I62_C18_ConsecutivePointsOfAValidHistoryPassEveryPairInvariant()
        {
            var history = CustodyHistory();
            for (var i = 1; i < history.Count; i++)
            {
                var violations = Validator.ValidatePair(history[i - 1], history[i]);
                Assert.True(violations.Count == 0, "rv " + (i + 1) + ": " + string.Join("; ", violations));
            }
        }

        [Fact]
        public void I62_C18_TheThreeKindsOfPositivePairAreGreenAtTheSameTime()
        {
            var ordinary = Validator.ValidatePair(Point(4), Point(5));
            var reconciliation = Validator.ValidatePair(Point(5), Point(6));
            var windowClose = Validator.ValidatePair(Point(3), WindowRebaseClose(), WindowContext());

            Assert.Equal("ORDINARY", Y.S(Point(5).State, "custody.point_kind"));
            Assert.Equal("REBASE_RECONCILIATION", Y.S(Point(6).State, "custody.point_kind"));
            Assert.Equal(4L, Y.N(WindowRebaseClose().State, "custody.last_rebase.record_version"));
            Assert.True(ordinary.Count == 0 && reconciliation.Count == 0 && windowClose.Count == 0,
                string.Join("; ", ordinary.Concat(reconciliation).Concat(windowClose)));
            Assert.Empty(Validator.ValidateFile(WindowRebaseClose()));
        }

        // (case, p rv, n rv, mutation of n (and p), context, required invariant, invariants that may also be reported)
        public static IEnumerable<object[]> Negatives()
        {
            yield return Case("I-P01 record_version skipped", 4, 5, (p, n) => Y.M(n.State, "custody")!["record_version"] = 6L, "I-P01");
            yield return Case("I-P02 Q0 omitted (QU → Q7)", 2, 4, (p, n) => Y.M(n.State, "custody")!["record_version"] = 3L, "I-P02", "I-P05");
            yield return Case("I-P02 two Q0 in a window", 3, 3, (p, n) =>
            {
                Y.M(n.State, "custody")!["record_version"] = 4L;
                Y.M(n.State, "custody.window")!["seq"] = 2L;
            }, "I-P02");
            yield return Case("I-P02 QH → QU", 7, 8, (p, n) => Y.M(n.State, "custody")!["point"] = "QU", "I-P02", "I-P07", "I-P09");
            yield return Case("I-P04 Q0 without seq + 1", 2, 3, (p, n) => Y.M(n.State, "custody.window")!["seq"] = 2L, "I-P04");
            yield return Case("I-P05 attempts decreases", 4, 5, (p, n) => Y.M(p.State, "automation_state")!["attempts"] = 1L, "I-P05");
            yield return Case("I-P05 attempts in a Q0 that is not a correction", 2, 3, (p, n) =>
            {
                Y.M(n.State, "automation_state")!["attempts"] = 1L;
                Y.M(n.State, "custody.task_intent")!["attempt"] = 1L;
            }, "I-P05");
            yield return Case("I-P05 QU ORDINARY touches the window", 4, 5, (p, n) => Y.M(n.State, "custody.last_window")!["attempt"] = 1L, "I-P05");
            yield return Case("I-P05 QU ORDINARY touches a counter", 4, 5, (p, n) => ((YamlMap)Y.L(n.State, "counters.invocations")[0]!)["launched"] = 3L, "I-P05");
            yield return Case("I-P05 counter decreases", 4, 5, (p, n) => ((YamlMap)Y.L(p.State, "counters.invocations")[0]!)["launched"] = 4L, "I-P05");
            yield return Case("I-P05 correction_launches rewritten", 4, 5, (p, n) =>
            {
                var launch = M(("seq", 1L), ("task_id", "T-01"), ("failure_class", "Tests"), ("correction_of_run_id", "R1"), ("attempts_after", 0L), ("record_version", 3L));
                Y.L(p.State, "counters.correction_launches").Add(launch);
                Y.L(n.State, "counters.correction_launches").Add(M(("seq", 1L), ("task_id", "T-01"), ("failure_class", "Ci"), ("correction_of_run_id", "R1"),
                    ("attempts_after", 0L), ("record_version", 3L)));
            }, "I-P05");
            yield return Case("I-P06 effective_sha", 4, 5, (p, n) => Y.M(n.State, "protocol")!["effective_sha"] = Sha2, "I-P06");
            yield return Case("I-P07 binding in a QU ORDINARY", 4, 5, (p, n) =>
                Y.M(n.State, "custody.principal")!["binding"] = ((InMemoryStateTree)n.Tree).PutJson("docs/automation/evidence/I-99-agent/qu/binding.json",
                    new JsonObject { ["BindingId"] = "B20261009T010101Z-0000", ["Scope"] = "UNIT", ["Role"] = "PRINCIPAL_COORDINATOR",
                        ["Acceptance"] = new JsonObject { ["State"] = "ACCEPTED" } }), "I-P07");
            yield return Case("I-P09 second g0 transition", 2, 3, (p, n) => Y.M(n.State, "protocol.g0_acceptance")!["state"] = "REJECTED", "I-P09");
            yield return Case("I-P09 acceptance without the delegation marker", 1, 2, (p, n) =>
            {
                var decisions = ((InMemoryStateTree)n.Tree).Put("docs/automation/decisions/I-99.md",
                    Decisions(G0Entry.Replace("I62-DELEGATED-EXECUTION: I62_DELEGATED\n", string.Empty, StringComparison.Ordinal)));
                Y.M(n.State, "protocol.g0_acceptance")!["decision"] = Clone(decisions);
                Y.M(n.State, "custody.principal.acceptance")!["decision"] = Clone(decisions);
            }, "I-P09");
            yield return Case("I-P10 reconciliation loses verified_sha", 5, 6, (p, n) => Y.M(n.State, "custody.last_window")!["verified_sha"] = Sha3, "I-P10");
            yield return Case("I-P10 chain RED is not the image", 5, 6, (p, n) => ((YamlMap)Y.L(n.State, "custody.chains")[0]!)["chain_red_sha"] = Sha, "I-P10");
            yield return Case("I-P10 reconciliation changes attempts", 5, 6, (p, n) => Y.M(n.State, "automation_state")!["attempts"] = 1L, "I-P10", "I-P05");
            yield return Case("I-P10 map without a StateFields entry", 5, 6, (p, n) =>
            {
                var map = RebaseMapJson();
                ((JsonArray)map["StateFields"]!).RemoveAt(2);
                var refMap = ((InMemoryStateTree)n.Tree).PutJson(RebaseMapPath, map);
                Y.M(n.State, "custody.last_rebase")!["map"] = refMap;
                Y.L(n.State, "custody.rebase_history")[0] = Clone(refMap);
            }, "I-P10");
            yield return Case("I-P05/D2-7 reconciliation changes the phase", 5, 6, (p, n) => Y.M(n.State, "automation_state")!["current_phase"] = "F5", "I-P05");
            yield return Case("I-P11/D2-9 reconciliation without the new map in the history", 5, 6, (p, n) => Y.L(n.State, "custody.rebase_history").Clear(), "I-P11");
            yield return Case("I-P11/D2-9 history changes without a reconciliation", 6, 7, (p, n) => Y.L(n.State, "custody.rebase_history").Clear(), "I-P11");
        }

        [Theory]
        [MemberData(nameof(Negatives))]
        public void I62_C18_EachPairInvariantRejectsItsNegativeCase(string name, long prv, long nrv, Action<StatePoint, StatePoint> mutation, string[] required,
            string[] allowed)
        {
            var (p, n) = Pair(prv, nrv, mutation);
            var reported = Validator.ValidatePair(p, n).Select(x => x.Invariant).Distinct().ToArray();

            Assert.True(required.All(reported.Contains), name + ": expected " + string.Join(", ", required) + ", got " + string.Join(", ", reported));
            Assert.True(reported.All(r => required.Contains(r) || allowed.Contains(r)), name + ": unexpected " + string.Join(", ", reported));
        }

        [Fact]
        public void I62_C18_I_P12_TheWindowCloseMustReconcileTheWindowsMapsInOrder()
        {
            var ownPoint = WindowRebaseClose();
            Y.M(ownPoint.State, "custody.last_rebase")!["record_version"] = 3L;
            Assert.Contains(Validator.ValidatePair(Point(3), ownPoint, WindowContext()), x => x.Invariant == "I-P12");

            var otherMap = WindowRebaseClose();
            var map = WindowRebaseMapJson();
            map["RunId"] = "R20261003T030303Z-0000";
            var refMap = ((InMemoryStateTree)otherMap.Tree).PutJson(WindowMapPath, map);
            Y.M(otherMap.State, "custody.last_rebase")!["map"] = refMap;
            Y.L(otherMap.State, "custody.rebase_history")[0] = Clone(refMap);
            var reported = Validator.ValidatePair(Point(3), otherMap, WindowContext()).Select(x => x.Invariant).ToList();
            Assert.Contains("I-P12", reported);
            Assert.Contains("I-P11", reported);

            var twoMaps = new PairContext(WindowRebaseMaps: new[] { WindowRebaseMapJson(), WindowRebaseMapJson() });
            Assert.Contains(Validator.ValidatePair(Point(3), WindowRebaseClose(), twoMaps), x => x.Invariant == "I-P11" && x.Clause == "D2-9");
        }

        [Theory]
        [InlineData("I-P02", "I-P02 Q0 omitted (QU → Q7)")]
        [InlineData("I-P05:attempts", "I-P05 attempts decreases")]
        [InlineData("I-P09:g0-once", "I-P09 second g0 transition")]
        [InlineData("I-P10", "I-P10 reconciliation loses verified_sha")]
        [InlineData("I-P11", "I-P11/D2-9 reconciliation without the new map in the history")]
        public void I62_C18_TheMutationClassesOfThePairInvariantsAreDetected(string mutation, string caseName)
        {
            var c = Negatives().Single(x => (string)x[0] == caseName);
            var (p, n) = Pair((long)c[1], (long)c[2], (Action<StatePoint, StatePoint>)c[3]);
            var invariant = mutation.Split(':')[0];

            Assert.Contains(Validator.ValidatePair(p, n), x => x.Invariant == invariant);
            Assert.DoesNotContain(new StateV2Validator(new[] { mutation }).ValidatePair(p, n), x => x.Invariant == invariant);
        }

        private static (StatePoint P, StatePoint N) Pair(long prv, long nrv, Action<StatePoint, StatePoint> mutation)
        {
            var p = Copy(Point(prv));
            var n = Copy(Point(nrv));
            mutation(p, n);
            return (p, n);
        }

        private static object[] Case(string name, long prv, long nrv, Action<StatePoint, StatePoint> mutation, string required, params string[] allowed) =>
            new object[] { name, prv, nrv, mutation, new[] { required }, allowed };
    }
}
