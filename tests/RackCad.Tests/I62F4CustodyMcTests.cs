#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Xunit;
using static RackCad.Tests.JournalSamples;
using static RackCad.Tests.StateV2Samples;

using static RackCad.Tests.CustodyMc;

namespace RackCad.Tests
{
    /// <summary>
    /// I-62 F4 (slice F4-G), C-15 (reproducible control, run as a guard): custody, CAS and state semantics on a disposable Git repository with real local SHAs — a bare
    /// <c>origin</c>, the holder's clone and a clean successor clone (<c>--no-local</c>, without the rewritten objects). At every durable point the
    /// file, pair and history invariants run on the state and the tree of its own commit, and at every step of a window the derived delegation state
    /// of B.8.2 is evaluated from the chained journal.
    /// </summary>
    public class I62F4CustodyMcTests
    {
        [Fact]
        public void I62_C15_TheWindowAndARebaseBetweenWindowsAreCustodiedAndResumedFromACleanCloneWithoutTheOriginalObjects()
        {
            var (r, pts, red, green) = Window1();
            using var _ = r;
            var h = r.Holder;

            // Every durable point satisfies every invariant (file, pair and history) on its own commit.
            for (var i = 0; i < pts.Count; i++)
            {
                Clean(All(h, i == 0 ? null : pts[i - 1].Point, i == 0 ? null : pts[i - 1].Commit, pts[i].Point, pts[i].Commit, pts[i].Commit), "rv" + (i + 1));
            }

            // DS of B.8.2 at each step of window 1, from its chained journal; after the Q7 there is no delegation.
            var q0 = pts[2].Point.State;
            var journal = Chain(Planning(Pass()), Work(), Verification());
            Assert.Equal(new[] { "PLANNED", "ACCEPTED_OPEN", "ACCEPTED_OPEN", "CLOSED_PENDING_CUSTODY" },
                Enumerable.Range(0, 4).Select(k => DelegationJournal.Derive(q0, journal.Take(k).ToList())));
            Assert.Empty(DelegationJournal.ExitProblems(q0, journal));
            Assert.Equal("NONE", DelegationJournal.Derive(pts[3].Point.State, null));

            // main advances; the holder rebases out of a window and publishes with --force-with-lease.
            r.Git(h, "checkout", "-q", "main");
            var m1 = r.Commit(h, "main 1", ("docs/otra.md", "otra\n"));
            r.Git(h, "push", "-q", "origin", "main");
            r.Git(h, "checkout", "-q", "feature");
            var branchBefore = r.Head(h);
            var mainBefore = r.Git(h, "merge-base", "main", "feature");
            r.Git(h, "rebase", "-q", "main");
            var branchAfter = r.Head(h);
            var draft = r.Map(h, "R20261004T010101Z-cd34", mainBefore, m1, branchBefore, branchAfter, Array.Empty<(string, string, string)>());
            var images = ((JsonArray)draft["Commits"]!).Select(c => (J.S((JsonObject)c!, "OriginalSha")!, J.S((JsonObject)c!, "ImageSha")!)).ToDictionary(x => x.Item1, x => x.Item2);
            var p5i = r.Read(h, branchAfter);
            var fields = StateV2Validator.StateFieldShas(p5i.State).Concat(StateV2Validator.LiveOrchestrationShas(p5i))
                .Select(f => (f.Field, f.Sha, images.TryGetValue(f.Sha, out var img) ? img : f.Sha)).ToList();
            var map = r.Map(h, "R20261004T010101Z-cd34", mainBefore, m1, branchBefore, branchAfter, fields);
            var git = new GitProcessHistory(h);
            Assert.Empty(RebaseChain.PublicationProblems(new RebaseMapDoc(map), git));
            r.Git(h, "push", "-q", "--force-with-lease=feature:" + branchBefore, "origin", "feature");

            // Between the force-push and the reconciliation, HEAD holds unreconciled SHAs: no Q0 and no delegated action (I-H02).
            Assert.Contains("I-H02", Ids(new StateV2Validator().ValidateHistory(p5i, git, branchAfter)));

            // QU REBASE_RECONCILIATION: the map custodied and appended to rebase_history, every SHA field on its image.
            var s6 = Clone(p5i.State);
            var tree6 = new InMemoryStateTree();
            var mapRef = tree6.PutJson("docs/automation/evidence/I-99-agent/rebase/R20261004T010101Z-cd34/rebase-map.json", map);
            var custody = (YamlMap)s6["custody"]!;
            custody["record_version"] = 6L;
            custody["point"] = "QU";
            custody["point_kind"] = "REBASE_RECONCILIATION";
            custody["last_rebase"] = M(("map", mapRef), ("main_before", mainBefore), ("main_after", m1), ("branch_before", branchBefore), ("branch_after", branchAfter),
                ("record_version", 6L));
            custody["rebase_history"] = L(Clone(mapRef));
            ((YamlMap)Y.L(s6, "custody.chains")[0]!)["chain_red_sha"] = images[red];
            ((YamlMap)custody["last_window"]!)["verified_sha"] = images[green];
            var k6 = r.WritePoint(h, new StatePoint(s6, tree6), "I-99: QU REBASE_RECONCILIATION");
            var p6 = r.Read(h, k6);
            Clean(All(h, p5i, branchAfter, p6, k6, k6), "rv6");

            // A reconciliation that loses a SHA (the GREEN left on its original) is caught by the pair and by the history.
            var lost = Clone(p6.State);
            ((YamlMap)Y.M(lost, "custody.last_window")!)["verified_sha"] = green;
            var lostIds = Ids(All(h, p5i, branchAfter, new StatePoint(lost, p6.Tree), k6, k6));
            Assert.Contains("I-P10", lostIds);
            Assert.Contains("I-H02", lostIds);

            // A successor in a clean clone, without the original objects, validates the point and resolves the original RED through the custodied chain.
            var c = r.CleanClone("successor");
            var gitC = new GitProcessHistory(c);
            Assert.False(gitC.Exists(branchBefore));
            Assert.False(gitC.Exists(red));
            var p6c = r.Read(c, k6);
            Assert.Empty(new StateV2Validator().ValidateFile(p6c));
            Assert.Empty(new StateV2Validator().ValidateHistory(p6c, gitC, k6));
            var redBlob = r.Git(h, "rev-parse", red + ":" + RedFile);
            var resolved = RebaseChain.Resolve(red, RedFile, redBlob, RebaseChain.History(p6c)!, k6, gitC);
            Assert.Equal(images[red], resolved.Commit);

            // The successor publishes the Q0 of window 2 from the clean clone.
            var s7 = Clone(p6c.State);
            var tree7 = new InMemoryStateTree();
            var contract = tree7.Put("docs/automation/evidence/I-99-agent/T-02/gate-contract.json", "{\"Schema\": \"rackcad-gate-contract/v2\", \"TaskId\": \"T-02\"}\n");
            var c7 = (YamlMap)s7["custody"]!;
            c7["record_version"] = 7L;
            c7["point"] = "Q0";
            c7["point_kind"] = null;
            c7["window"] = M(("seq", 2L), ("state", "OPENABLE"));
            c7["task_intent"] = M(("task_id", "T-02"), ("attempt", 0L), ("kind", "FIRST"), ("contract", contract), ("continues_task_id", null),
                ("planned_roles", L(M(("role", "EXECUTION_CONTROLLER"), ("binding", null)), M(("role", "WORKER"), ("binding", null)))));
            var k7 = r.WritePoint(c, new StatePoint(s7, tree7), "I-99: Q0 ventana 2 (sucesor)");
            Clean(All(c, p6c, k6, r.Read(c, k7), k7, k7), "rv7");
            Assert.Equal("PLANNED", DelegationJournal.Derive(r.Read(c, k7).State, new List<byte[]>()));
        }
    }
}
