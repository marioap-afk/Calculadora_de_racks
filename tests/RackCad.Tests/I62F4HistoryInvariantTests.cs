#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Xunit;
using static RackCad.Tests.OrchestrationSamples;
using static RackCad.Tests.StateV2Samples;

namespace RackCad.Tests
{
    /// <summary>
    /// I-62 F4 (slice F4-E), C-15: the state/v2 invariants «con historia» (B.8.4: I-H01, I-H02, I-P03, I-P08) with the A-1 deltas that need Git —
    /// D2-3 (live orchestration commits in I-H02), D2-2 (cont.) with A62-A1T-01 (the Target of a LAUNCHING attempt resolves by ResolveBranchRef with the
    /// whole history of n and the rebased tip, across one and two reconciliations), D2-6 (2) (case B.1 replans on the image) and D2-11 (branch
    /// references of invocations and of materialized bindings resolve; a replanned invocation is rebuilt on the images). Each positive has its
    /// negative and, where a rule can be switched off, a mutation that silences it. The commit graph is synthetic; the reconciled points also pass
    /// the file and pair validators, so the scenario is one the protocol admits.
    /// </summary>
    public class I62F4HistoryInvariantTests
    {
        private const string StatePath = "docs/automation/state/I-99.yml";
        private const string E = "eeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeeee";
        private static readonly string K3 = H("3a"), M1 = H("e1"), C1i = H("c3"), K3i = H("3b"), Kn1 = H("4a"), M2 = H("e2"), C1ii = H("c4"), K3ii = H("3c"),
            Kn1i = H("4b"), Kn2 = H("5a"), Z = H("9f"), Q0k = H("0a"), Q7k = H("7a"), S = H("5e"), Q0ki = H("0b"), P1 = H("15"), Pk = H("16"), Pn = H("17"),
            Other = H("ff");

        private static string H(string two) => string.Concat(Enumerable.Repeat(two, 20));

        private static List<string> Ids(IEnumerable<StateViolation> v) => v.Select(x => x.Invariant + (x.Clause.Length > 0 ? "/" + x.Clause : string.Empty)).ToList();

        // ------------------------------------------------------------------ the F.8 loop reconciled twice

        // Sha ← Sha2 ← Sha3 (main before) ← C1 {X: B1} ← K3 (the commit of s3); main: Sha3 ← M1 ← M2. Rebase 1: M1 ← C1' ← K3' ← Kn1 (the commit of
        // the first reconciliation). Rebase 2: M2 ← C1'' {X: imageBlob} ← K3'' ← Kn1' ← Kn2. Z is an unrelated root. Sha holds the decisions blob that the
        // materialized binding's AuthorizationRef cites.
        private static SyntheticGitHistory LoopGraph(string authorizationBlob, string? imageBlob = null) => new SyntheticGitHistory()
            .Commit(Sha, null, null, (OrchestrationSamples.Decisions, authorizationBlob))
            .Commit(Sha2, Sha)
            .Commit(Sha3, Sha2)
            .Commit(C1, Sha3, P1, (X, B1))
            .Commit(K3, C1, Pk, (StatePath, "s3"))
            .Commit(M1, Sha3)
            .Commit(C1i, M1, P1, (X, B1))
            .Commit(K3i, C1i, Pk, (StatePath, "s3"))
            .Commit(Kn1, K3i, Pn, (StatePath, "n1"))
            .Commit(M2, M1)
            .Commit(C1ii, M2, P1, (X, imageBlob ?? B1))
            .Commit(K3ii, C1ii, Pk, (StatePath, "s3"))
            .Commit(Kn1i, K3ii, Pn, (StatePath, "n1"))
            .Commit(Kn2, Kn1i, null, (StatePath, "n2"))
            .Commit(Z, null);

        private static string AuthorizationBlob(StatePoint p) => Y.S((YamlMap)Loop(p)["authorization"]!, "blob")!;

        /// <summary>
        /// A QU REBASE_RECONCILIATION after <paramref name="p"/> (§8.8 with A-1 D2-1, D2-2, D2-4, D2-9): the live orchestration commits, the stored
        /// next_action target and the state SHA fields pass to their images; the map is custodied with one StateFields entry per field and appended to
        /// rebase_history. Attempts in LAUNCHING are untouched; the caller replans INVOCATION_PLANNED / BUDGET_RESERVED attempts itself.
        /// </summary>
        private static StatePoint Reconcile(StatePoint p, string runId, string mainBefore, string mainAfter, string branchBefore, string branchAfter,
            params (string O, string I, string Pid)[] commits)
        {
            var n = Next(p);
            var images = commits.ToDictionary(c => c.O, c => c.I, StringComparer.Ordinal);
            string Img(string sha) => images.TryGetValue(sha, out var i) ? i : sha;
            var fields = new JsonArray();
            foreach (var (field, sha) in StateV2Validator.StateFieldShas(p.State).Concat(StateV2Validator.LiveOrchestrationShas(p)))
            {
                fields.Add(new JsonObject { ["Field"] = field, ["OriginalSha"] = sha, ["ImageSha"] = Img(sha) });
            }

            var s = n.State;
            foreach (var obj in new[] { Y.M(s, "orchestration.loop.object"), Y.M(s, "orchestration.next_action.target") }
                         .Concat(Y.L(s, "orchestration.review_requests").Cast<YamlMap>().Where(r => Y.S(r, "state") == "OPEN").Select(r => Y.M(r, "object"))))
            {
                if (obj != null)
                {
                    obj["commit"] = Img(Y.S(obj, "commit")!);
                }
            }

            foreach (var c in Y.L(s, "custody.chains").Cast<YamlMap>())
            {
                c["chain_base_sha"] = Img(Y.S(c, "chain_base_sha")!);
                c["chain_red_sha"] = Y.S(c, "chain_red_sha") is string red ? Img(red) : null;
            }

            var lastWindow = Y.M(s, "custody.last_window");
            if (lastWindow != null && Y.S(lastWindow, "verified_sha") is string verified)
            {
                lastWindow["verified_sha"] = Img(verified);
            }

            var map = Tree(n).PutJson("docs/automation/evidence/I-99-agent/rebase/" + runId + "/rebase-map.json", new JsonObject
            {
                ["RunId"] = runId, ["TaskId"] = "SESSION", ["MainBeforeSha"] = mainBefore, ["MainAfterSha"] = mainAfter, ["BranchBeforeSha"] = branchBefore,
                ["BranchAfterSha"] = branchAfter,
                ["Commits"] = new JsonArray(commits.Select(c => (JsonNode)new JsonObject { ["OriginalSha"] = c.O, ["ImageSha"] = c.I, ["PatchId"] = c.Pid,
                    ["PatchIdEqual"] = true }).ToArray()),
                ["StateFields"] = fields, ["CiRuns"] = new JsonArray(), ["Unmapped"] = new JsonArray(),
            });
            var custody = (YamlMap)s["custody"]!;
            custody["point_kind"] = "REBASE_RECONCILIATION";
            custody["last_rebase"] = M(("map", map), ("main_before", mainBefore), ("main_after", mainAfter), ("branch_before", branchBefore),
                ("branch_after", branchAfter), ("record_version", Rv(n)));
            Y.L(s, "custody.rebase_history").Add(Clone(map));
            return n;
        }

        /// <summary>s3 of F.8 (a1.1 LAUNCHING on X {c1, b1}) reconciled once (n1, map R1) and, optionally, twice (n2, map R2).</summary>
        private static (StatePoint S3, StatePoint N1, StatePoint N2) Reconciled()
        {
            var s3 = F8Step(3);
            var n1 = Reconcile(s3, "R20261006T040404Z-0001", Sha3, M1, K3, K3i, (C1, C1i, P1), (K3, K3i, Pk));
            var n2 = Reconcile(n1, "R20261006T050505Z-0002", M1, M2, Kn1, Kn1i, (C1i, C1ii, P1), (K3i, K3ii, Pk), (Kn1, Kn1i, Pn));
            return (s3, n1, n2);
        }

        private static void DropFirstMap(StatePoint n) => Y.L(n.State, "custody.rebase_history").RemoveAt(0);

        private static void Replan(StatePoint n, string request, long seq, YamlMap target, string to = "BUDGET_RESERVED")
        {
            var a = AttemptOf(n, request, seq);
            var old = Orchestration.Invocation(n, a)!;
            var json = InvocationJson("I20261006T999999Z-" + seq.ToString("0000", System.Globalization.CultureInfo.InvariantCulture), request, seq, target,
                new string[0], (1, 1, 1, 0, 0));
            json["BudgetSnapshot"] = old["BudgetSnapshot"]!.DeepClone();
            a["invocation"] = Tree(n).PutJson("docs/automation/evidence/I-99-agent/review/" + request + "/" + seq + "/invocation-replanned-" + Rv(n) + ".json", json);
            a["state"] = to;
            a["run_id"] = null;
            a["launching_utc"] = null;
        }

        [Fact]
        public void I62_C15_TheReconciledLoopPointsAreAdmittedByTheFileAndPairValidators()
        {
            var (s3, n1, n2) = Reconciled();
            var validator = new StateV2Validator();
            Assert.Empty(validator.ValidateFile(n1));
            Assert.Empty(validator.ValidateFile(n2));
            Assert.Empty(validator.ValidatePair(s3, n1));
            Assert.Empty(validator.ValidatePair(n1, n2));
        }

        [Fact]
        public void I62_C15_IH02_D2_3_ShaFieldsAndLiveOrchestrationCommitsAreAncestorsOfHeadOutsideAWindow()
        {
            var (s3, n1, n2) = Reconciled();
            var git = LoopGraph(AuthorizationBlob(s3));
            var validator = new StateV2Validator();
            Assert.Empty(validator.ValidateHistory(s3, git, K3));
            Assert.Empty(validator.ValidateHistory(n1, git, Kn1));
            Assert.Empty(validator.ValidateHistory(n2, git, Kn2));

            // Between the force-push and the reconciliation (V14 §8.8 steps 5–6), HEAD holds the image of s3 with unreconciled live commits.
            var unreconciled = Ids(validator.ValidateHistory(s3, git, K3i));
            Assert.Contains("I-H02/D2-3", unreconciled);
            Assert.DoesNotContain(unreconciled, x => x == "I-H02");

            // A live commit left on the original after the reconciliation: I-H02 by D2-3; without D2-3 the live commits are not checked.
            var stale = Copy(n1);
            ((YamlMap)Loop(stale)["object"]!)["commit"] = C1;
            Assert.Contains("I-H02/D2-3", Ids(validator.ValidateHistory(stale, git, Kn1)));
            Assert.DoesNotContain("I-H02/D2-3", Ids(new StateV2Validator(new[] { "D2-3" }).ValidateHistory(stale, git, Kn1)));
        }

        [Fact]
        public void I62_C15_IH02_ADurableShaFieldOffTheRebasedBranchFailsUntilTheReconciliation()
        {
            // F.6: main moves from Sha to E; the branch Sha ← Sha2 ← Sha3 is rebased onto E as Img2 ← Img3. rv5 is the last point before the rebase and
            // rv6 its REBASE_RECONCILIATION (Sha2 → Img2, Sha3 → Img3).
            var git = new SyntheticGitHistory().Commit(Sha, null).Commit(Sha2, Sha).Commit(Sha3, Sha2).Commit(E, Sha).Commit(Img2, E).Commit(Img3, Img2);
            var validator = new StateV2Validator();
            Assert.Empty(validator.ValidateHistory(Point(5), git, Sha3));
            Assert.Empty(validator.ValidateHistory(Point(6), git, Img3));
            var unreconciled = validator.ValidateHistory(Point(5), git, Img3);
            Assert.Equal(new[] { "chains[T-01].chain_red_sha", "last_window.verified_sha" },
                unreconciled.Where(x => x.Invariant == "I-H02").Select(x => x.Message.Split(' ')[0]).OrderBy(x => x, StringComparer.Ordinal));
            Assert.Empty(new StateV2Validator(new[] { "I-H02" }).ValidateHistory(Point(5), git, Img3));

            // Inside a window (last point Q0) the state SHAs may stay unreconciled after a rebase of 16.7: I-H02 does not apply.
            Assert.DoesNotContain("I-H02", Ids(validator.ValidateHistory(Point(3), git, Img3)));
        }

        [Fact]
        public void I62_C15_IH01_AQ0OnAnObsoleteBaseNeedsTheBranchToContainEffectiveSha()
        {
            var git = new SyntheticGitHistory().Commit(Sha, null).Commit(Sha2, Sha).Commit(Z, null);
            var q0 = Point(3);
            ((YamlMap)Y.M(q0.State, "protocol.basis")!)["claim_parent_contains_effective"] = false;
            var validator = new StateV2Validator();
            Assert.Empty(validator.ValidateHistory(q0, git, Sha2));
            Assert.Contains("I-H01", Ids(validator.ValidateHistory(q0, git, Z)));
            Assert.Empty(new StateV2Validator(new[] { "I-H01" }).ValidateHistory(q0, git, Z));

            // With claim_parent_contains_effective = true the base is not obsolete and I-H01 does not apply.
            Assert.Empty(validator.ValidateHistory(Point(3), git, Z));
        }

        [Fact]
        public void I62_C15_D2_11_InvocationAndAuthorizationReferencesResolveByResolveBranchRef()
        {
            var (s3, n1, n2) = Reconciled();
            var auth = AuthorizationBlob(s3);
            var validator = new StateV2Validator();

            // The LAUNCHING attempt keeps Target = X {c1, b1}: no ancestry requirement (D2-3), but it resolves through R1 and through R1 + R2.
            Assert.Empty(validator.ValidateHistory(n1, LoopGraph(auth), Kn1));
            Assert.Empty(validator.ValidateHistory(n2, LoopGraph(auth), Kn2));

            // Without R1 in n2's history, the chain X → X' → X'' is not proven: the historical Target is UNRESOLVED.
            var missing = Copy(n2);
            DropFirstMap(missing);
            Assert.Contains("I-S18/D2-11", Ids(validator.ValidateHistory(missing, LoopGraph(auth), Kn2)));

            // The image holds another blob of X: UNRESOLVED.
            Assert.Contains("I-S18/D2-11", Ids(validator.ValidateHistory(n2, LoopGraph(auth, imageBlob: Other), Kn2)));

            // The AuthorizationRef of the materialized binding cites a blob that its commit does not hold: UNRESOLVED.
            Assert.Contains("I-S18/D2-11", Ids(validator.ValidateHistory(n1, LoopGraph(Other), Kn1)));
            Assert.DoesNotContain("I-S18/D2-11", Ids(new StateV2Validator(new[] { "D2-11" }).ValidateHistory(n1, LoopGraph(Other), Kn1)));
        }

        [Fact]
        public void I62_C15_A62_A1T_01_ALaunchingTargetResolvesWithTheWholeHistoryOfNAndTheRebasedTip()
        {
            var (s3, n1, n2) = Reconciled();
            var git = LoopGraph(AuthorizationBlob(s3));
            var validator = new StateV2Validator();

            // T1: one reconciliation; T2: two, where the Target is still the original X and needs R1 and R2.
            Assert.Empty(validator.ValidatePairHistory(s3, K3, n1, Kn1, git, StatePath));
            Assert.Empty(validator.ValidatePairHistory(n1, Kn1, n2, Kn2, git, StatePath));

            // Only the last map (the A62-A1T-01 defect): the first link is missing, UNRESOLVED.
            var lastOnly = Copy(n2);
            DropFirstMap(lastOnly);
            Assert.Contains("I-P13/D2-2", Ids(validator.ValidatePairHistory(n1, Kn1, lastOnly, Kn2, git, StatePath)));

            // Same path, another blob at the image: UNRESOLVED.
            Assert.Contains("I-P13/D2-2", Ids(validator.ValidatePairHistory(n1, Kn1, n2, Kn2, LoopGraph(AuthorizationBlob(s3), imageBlob: Other), StatePath)));

            // A fabricated composite entry X → X'' in the second map, with the first map absent: never creditable (A62-A1U-O1).
            var fabricated = Reconcile(n1, "R20261006T050505Z-0002", M1, M2, Kn1, Kn1i, (C1, C1ii, P1), (K3i, K3ii, Pk), (Kn1, Kn1i, Pn));
            DropFirstMap(fabricated);
            Assert.Contains("I-P13/D2-2", Ids(validator.ValidatePairHistory(n1, Kn1, fabricated, Kn2, git, StatePath)));
            Assert.Empty(new StateV2Validator(new[] { "D2-2" }).ValidatePairHistory(n1, Kn1, lastOnly, Kn2, git, StatePath));
        }

        [Fact]
        public void I62_C15_D2_11_AReplannedInvocationIsRebuiltOnTheImages()
        {
            // s2: a1.1 BUDGET_RESERVED on X {c1, b1}; the reconciliation replans it (D2-2) with a new InvocationId and the same reservation.
            var s2 = F8Step(2);
            var git = LoopGraph(AuthorizationBlob(s2));
            var n = Reconcile(s2, "R20261006T040404Z-0001", Sha3, M1, K3, K3i, (C1, C1i, P1), (K3, K3i, Pk));
            Replan(n, L1, 1, Obj(C1i, X, B1));
            var validator = new StateV2Validator();
            Assert.Empty(validator.ValidatePairHistory(s2, K3, n, Kn1, git, StatePath));

            // A replanned invocation that keeps the pre-rebase Target is not rebuilt on the images.
            var kept = Reconcile(s2, "R20261006T040404Z-0001", Sha3, M1, K3, K3i, (C1, C1i, P1), (K3, K3i, Pk));
            Replan(kept, L1, 1, Obj(C1, X, B1));
            Assert.Contains("I-P13/D2-11", Ids(validator.ValidatePairHistory(s2, K3, kept, Kn1, git, StatePath)));
            Assert.Empty(new StateV2Validator(new[] { "D2-11" }).ValidatePairHistory(s2, K3, kept, Kn1, git, StatePath));
        }

        [Fact]
        public void I62_C15_D2_6_CaseB1ReplansTheLaunchingAttemptOnTheImageOfResolveBranchRef()
        {
            // After n1, the invoker's evidence says the process never started: LAUNCHING → BUDGET_RESERVED with a new invocation on the image.
            var (s3, n1, _) = Reconciled();
            var git = LoopGraph(AuthorizationBlob(s3));
            var b1 = Next(n1);
            Replan(b1, L1, 1, Obj(C1i, X, B1));
            var validator = new StateV2Validator();
            Assert.Empty(validator.ValidateB1History(n1, b1, git, Kn1));

            var stale = Next(n1);
            Replan(stale, L1, 1, Obj(C1, X, B1));
            Assert.Contains("I-P13/D2-6", Ids(validator.ValidateB1History(n1, stale, git, Kn1)));
            Assert.Empty(new StateV2Validator(new[] { "D2-6" }).ValidateB1History(n1, stale, git, Kn1));
        }

        // ------------------------------------------------------------------ a window: I-P03 and I-P08

        // Sha ← Q0k (the Q0) ← Sha3 (RED of T-01) ← Sha2 (GREEN, verified) [← S, a session commit] ← Q7k (the Q7).
        private static SyntheticGitHistory WindowGraph(bool sessionCommit = false, bool workerWritesState = false)
        {
            var g = new SyntheticGitHistory()
                .Commit(Sha, null)
                .Commit(Q0k, Sha, null, (StatePath, "q0"))
                .Commit(Sha3, Q0k, null, workerWritesState ? new[] { ("tests/RackCad.Tests/EjemploTests.cs", "r"), (StatePath, "w") } : new[] { ("tests/RackCad.Tests/EjemploTests.cs", "r") })
                .Commit(Sha2, Sha3, null, ("src/Ejemplo.cs", "g"));
            if (sessionCommit)
            {
                g.Commit(S, Sha2, null, ("docs/automation/evidence/I-99-agent/nota.md", "s"));
            }

            return g.Commit(Q7k, sessionCommit ? S : Sha2, null, (StatePath, "q7"));
        }

        [Fact]
        public void I62_C15_IP03_IP08_BetweenAQ0AndItsClosureOnlyTheWorkersCommitsAndNoneWritesTheState()
        {
            var validator = new StateV2Validator();
            Assert.Empty(validator.ValidatePairHistory(Point(3), Q0k, Point(4), Q7k, WindowGraph(), StatePath));

            // W-2: a session commit inside the window.
            Assert.Contains("I-P03", Ids(validator.ValidatePairHistory(Point(3), Q0k, Point(4), Q7k, WindowGraph(sessionCommit: true), StatePath)));

            // W-3: a commit of the Worker writes the state file.
            var w3 = Ids(validator.ValidatePairHistory(Point(3), Q0k, Point(4), Q7k, WindowGraph(workerWritesState: true), StatePath));
            Assert.Contains("I-P08", w3);
            Assert.DoesNotContain("I-P03", w3);
            Assert.Empty(new StateV2Validator(new[] { "I-P08" }).ValidatePairHistory(Point(3), Q0k, Point(4), Q7k, WindowGraph(workerWritesState: true), StatePath));

            // The point after the Q0 does not close its window.
            var other = Point(4);
            ((YamlMap)Y.M(other.State, "custody.last_window")!)["seq"] = 2L;
            Assert.Contains("I-P03", Ids(validator.ValidatePairHistory(Point(3), Q0k, other, Q7k, WindowGraph(), StatePath)));

            // Outside a window neither applies: the commits between two ordinary points are the session's.
            Assert.Empty(validator.ValidatePairHistory(Point(4), Q0k, Point(5), Q7k, WindowGraph(sessionCommit: true, workerWritesState: true), StatePath));
        }

        [Fact]
        public void I62_C15_IP03_AWindowWithItsOwnRebaseStartsAtTheImageOfTheQ0()
        {
            // Rebase of 16.7 inside the window: main Sha ← E; Q0k ← Sha2 (W1) is rewritten as E ← Q0k' ← Img2; the Q7 closes on the rebased branch (I-P12).
            var git = new SyntheticGitHistory()
                .Commit(Sha, null).Commit(Q0k, Sha, null, (StatePath, "q0")).Commit(Sha2, Q0k, null, ("src/Ejemplo.cs", "g"))
                .Commit(E, Sha).Commit(Q0ki, E, null, (StatePath, "q0")).Commit(Img2, Q0ki, null, ("src/Ejemplo.cs", "g")).Commit(Q7k, Img2, null, (StatePath, "q7"));
            var q7 = Point(4);
            var map = Tree(q7).PutJson(WindowMapPath, new JsonObject
            {
                ["RunId"] = "R20261003T020202Z-ef01", ["TaskId"] = "T-01", ["MainBeforeSha"] = Sha, ["MainAfterSha"] = E, ["BranchBeforeSha"] = Sha2,
                ["BranchAfterSha"] = Img2,
                ["Commits"] = new JsonArray(
                    new JsonObject { ["OriginalSha"] = Q0k, ["ImageSha"] = Q0ki, ["PatchId"] = Pk, ["PatchIdEqual"] = true },
                    new JsonObject { ["OriginalSha"] = Sha2, ["ImageSha"] = Img2, ["PatchId"] = P1, ["PatchIdEqual"] = true }),
                ["StateFields"] = new JsonArray(), ["CiRuns"] = new JsonArray(), ["Unmapped"] = new JsonArray(),
            });
            var custody = (YamlMap)q7.State["custody"]!;
            custody["last_rebase"] = M(("map", map), ("main_before", Sha), ("main_after", E), ("branch_before", Sha2), ("branch_after", Img2), ("record_version", 4L));
            custody["rebase_history"] = L(Clone(map));
            ((YamlMap)custody["last_window"]!)["verified_sha"] = Img2;
            var validator = new StateV2Validator();
            Assert.Empty(validator.ValidatePairHistory(Point(3), Q0k, q7, Q7k, git, StatePath));

            // Without the window's own map the Q0 is neither an ancestor of the closure nor rewritten: I-P03 fails closed.
            var unmapped = Point(4);
            ((YamlMap)unmapped.State["custody"]!)["last_window"] = Clone((YamlMap)custody["last_window"]!);
            Assert.Contains("I-P03", Ids(validator.ValidatePairHistory(Point(3), Q0k, unmapped, Q7k, git, StatePath)));
        }
    }
}
