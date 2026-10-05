#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Xunit;

namespace RackCad.Tests
{
    /// <summary>
    /// I-62 F4 (slice F4-E), C-15 (rebase) and C-38: the custodied chain of RebaseMaps of the AGREED A-1 — ResolveBranchRef (D2-10),
    /// EquivalentReviewedObject (D2-12) and the publication check of a map (B.8.7) with the Coordinator's obligation A62-A1U-O1 (decisions §43). The
    /// chain negatives A..H of the F4 opening order (§11) are each a test: A complete history X → X' → X'' resolves; B a missing X → X' fails closed;
    /// C a fabricated direct X → X'' without the real first rebase is rejected; D the same path with another blob fails; E/F two real rebases resolve in a
    /// clean successor clone only from durable custody, without the unreachable original commits; G a reordered history fails; H a broken patch identity
    /// fails. A–D, G and H use a synthetic commit graph; E and F use real Git in a disposable repository.
    /// </summary>
    public class I62F4RebaseChainTests
    {
        private const string D = "docs/initiatives/U-proposal.md";
        private const string Bx = "b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0";

        // m0 ← X ← Y (original branch); m0 ← m1 ← m2 (main); m1 ← X' ← Y' (rebase 1); m2 ← X'' ← Y'' (rebase 2). D holds Bx from X on.
        private static SyntheticGitHistory Graph(bool brokenPatch = false) => new SyntheticGitHistory()
            .Commit("m0", null)
            .Commit("X", "m0", "pX", (D, Bx))
            .Commit("Y", "X", "pY", ("src/y.cs", "y1"))
            .Commit("m1", "m0")
            .Commit("X1", "m1", "pX", (D, Bx))
            .Commit("Y1", "X1", "pY", ("src/y.cs", "y1"))
            .Commit("m2", "m1")
            .Commit("X2", "m2", brokenPatch ? "pZ" : "pX", (D, Bx))
            .Commit("Y2", "X2", "pY", ("src/y.cs", "y1"));

        private static RebaseMapDoc Map(string mainBefore, string mainAfter, string branchBefore, string branchAfter, params (string O, string I, bool Eq)[] commits) =>
            new RebaseMapDoc(new JsonObject
            {
                ["MainBeforeSha"] = mainBefore, ["MainAfterSha"] = mainAfter, ["BranchBeforeSha"] = branchBefore, ["BranchAfterSha"] = branchAfter,
                ["Commits"] = new JsonArray(commits.Select(c => (JsonNode)new JsonObject { ["OriginalSha"] = c.O, ["ImageSha"] = c.I,
                    ["PatchId"] = c.O.StartsWith('Y') ? "pY" : "pX", ["PatchIdEqual"] = c.Eq }).ToArray()),
                ["StateFields"] = new JsonArray(), ["CiRuns"] = new JsonArray(), ["Unmapped"] = new JsonArray(),
            });

        private static RebaseMapDoc M1(bool eq = true) => Map("m0", "m1", "Y", "Y1", ("X", "X1", eq), ("Y", "Y1", true));

        private static RebaseMapDoc M2() => Map("m1", "m2", "Y1", "Y2", ("X1", "X2", true), ("Y1", "Y2", true));

        [Fact]
        public void I62_C15_A_ACompleteOrderedHistoryResolvesXToItsSecondImage()
        {
            var r = RebaseChain.Resolve("X", D, Bx, new[] { M1(), M2() }, "Y2", Graph());
            Assert.True(r.Resolved, r.Reason);
            Assert.Equal("X2", r.Commit);
        }

        [Fact]
        public void I62_C15_B_AHistoryWithoutTheFirstMapFailsClosed()
        {
            var r = RebaseChain.Resolve("X", D, Bx, new[] { M2() }, "Y2", Graph());
            Assert.False(r.Resolved);
            Assert.Contains("not an OriginalSha", r.Reason, StringComparison.Ordinal);
        }

        [Fact]
        public void I62_C15_C_A62_A1U_O1_AFabricatedDirectEntryNeverReplacesTheMissingFirstRebase()
        {
            var fabricated = Map("m1", "m2", "Y1", "Y2", ("X", "X2", true), ("Y1", "Y2", true));
            var git = Graph();

            // The reconciling host, where the pre-rebase objects exist, rejects the map before publishing it (B.8.7; §8.8 step 4).
            var problems = RebaseChain.PublicationProblems(fabricated, git);
            Assert.Contains(problems, p => p.Contains("was not rewritten by this rebase", StringComparison.Ordinal));
            Assert.Empty(RebaseChain.PublicationProblems(M2(), git));
            Assert.Empty(RebaseChain.PublicationProblems(M1(), git));

            // And the chain never credits the composite entry, alone or after an incomplete first map.
            Assert.False(RebaseChain.Resolve("X", D, Bx, new[] { fabricated }, "Y2", git).Resolved);
            var m1WithoutX = Map("m0", "m1", "Y", "Y1", ("Y", "Y1", true));
            var r = RebaseChain.Resolve("X", D, Bx, new[] { m1WithoutX, fabricated }, "Y2", git);
            Assert.False(r.Resolved);
            Assert.Contains("A62-A1U-O1", r.Reason, StringComparison.Ordinal);
        }

        [Fact]
        public void I62_C15_D_TheSamePathWithAnotherBlobFails()
        {
            var r = RebaseChain.Resolve("X", D, "ffffffffffffffffffffffffffffffffffffffff", new[] { M1(), M2() }, "Y2", Graph());
            Assert.False(r.Resolved);
            Assert.Contains("blob", r.Reason, StringComparison.Ordinal);
        }

        [Fact]
        public void I62_C15_G_AReorderedHistoryCannotProveTheChain()
        {
            var r = RebaseChain.Resolve("X", D, Bx, new[] { M2(), M1() }, "Y2", Graph());
            Assert.False(r.Resolved);
        }

        [Fact]
        public void I62_C15_H_ABrokenPatchIdentityFails()
        {
            Assert.False(RebaseChain.Resolve("X", D, Bx, new[] { M1(eq: false), M2() }, "Y2", Graph()).Resolved);
            var recomputed = RebaseChain.Resolve("X", D, Bx, new[] { M1(), M2() }, "Y2", Graph(brokenPatch: true));
            Assert.False(recomputed.Resolved);
            Assert.Contains("PatchId", recomputed.Reason, StringComparison.Ordinal);
        }

        [Fact]
        public void I62_C15_EquivalentReviewedObjectNeedsTheSamePathAndBlobAndAProvenImage()
        {
            YamlMap Obj(string c, string blob) => StateV2Samples.Obj(c, D, blob);
            var history = new[] { M1(), M2() };
            Assert.True(RebaseChain.Equivalent(Obj("X", Bx), Obj("X2", Bx), history, Graph()));
            Assert.True(RebaseChain.Equivalent(Obj("X2", Bx), Obj("X", Bx), history, Graph()));
            Assert.False(RebaseChain.Equivalent(Obj("X", Bx), Obj("X2", "ffffffffffffffffffffffffffffffffffffffff"), history, Graph()));
            Assert.False(RebaseChain.Equivalent(Obj("X", Bx), Obj("X2", Bx), new[] { M2() }, Graph()));
        }

        [Fact]
        public void I62_C15_E_F_TwoRealRebasesResolveInACleanSuccessorCloneOnlyFromDurableCustody()
        {
            using var g = new GitScratch();
            var src = System.IO.Path.Combine(g.Root, "src");
            System.IO.Directory.CreateDirectory(src);
            g.Run(src, "init", "-q", "-b", "main");
            g.Write(src, "README.md", "base\n");
            g.CommitAll(src, "m0");
            g.Run(src, "checkout", "-q", "-b", "feature");
            g.Write(src, D, "# U\n\nversión revisada X\n");
            var x = g.CommitAll(src, "X");
            var blob = g.Run(src, "rev-parse", x + ":" + D);

            var maps = new List<string>();
            string Rebase(string tag)
            {
                g.Run(src, "checkout", "-q", "main");
                g.Write(src, "main-" + tag + ".txt", tag + "\n");
                var mainAfter = g.CommitAll(src, "main " + tag);
                var mainBefore = g.Run(src, "merge-base", "main", "feature");
                var branchBefore = g.Run(src, "rev-parse", "feature");
                var before = g.Run(src, "rev-list", "--reverse", mainBefore + ".." + branchBefore).Split('\n');
                g.Run(src, "rebase", "-q", "main", "feature");
                var branchAfter = g.Run(src, "rev-parse", "feature");
                var after = g.Run(src, "rev-list", "--reverse", mainAfter + ".." + branchAfter).Split('\n');
                var history = new GitProcessHistory(src);
                var json = new JsonObject
                {
                    ["MainBeforeSha"] = mainBefore, ["MainAfterSha"] = mainAfter, ["BranchBeforeSha"] = branchBefore, ["BranchAfterSha"] = branchAfter,
                    ["Commits"] = new JsonArray(before.Zip(after, (o, i) => (JsonNode)new JsonObject
                        { ["OriginalSha"] = o, ["ImageSha"] = i, ["PatchId"] = history.PatchId(i), ["PatchIdEqual"] = history.PatchId(o) == history.PatchId(i) }).ToArray()),
                    ["StateFields"] = new JsonArray(), ["CiRuns"] = new JsonArray(), ["Unmapped"] = new JsonArray(),
                };
                Assert.Empty(RebaseChain.PublicationProblems(new RebaseMapDoc(json), history));
                var path = "docs/automation/evidence/U-agent/rebase/" + tag + "/rebase-map.json";
                g.Write(src, path, json.ToJsonString() + "\n");
                maps.Add(path);
                g.CommitAll(src, "reconcile " + tag);
                return branchAfter;
            }

            Rebase("r1");
            Rebase("r2");
            var clone = System.IO.Path.Combine(g.Root, "clone");
            g.Run(g.Root, "clone", "-q", "--no-local", "-c", "core.autocrlf=false", "--single-branch", "--branch", "feature", src, clone);
            var cloneGit = new GitProcessHistory(clone);
            var head = g.Run(clone, "rev-parse", "HEAD");

            // F: the original X is unreachable in the clean clone; nothing private (object store, reflog, memory) is consulted.
            Assert.False(cloneGit.Exists(x));
            var custodied = maps.Select(m => new RebaseMapDoc((JsonObject)JsonNode.Parse(g.Run(clone, "show", "HEAD:" + m))!)).ToList();
            Assert.False(cloneGit.Exists(custodied[0].Commits[0].Image!));

            // E: the successor resolves X → X'' only from the custodied maps, recomputing the patch identity on the image.
            var r = RebaseChain.Resolve(x, D, blob, custodied, head, cloneGit);
            Assert.True(r.Resolved, r.Reason);
            Assert.True(cloneGit.IsAncestor(r.Commit!, head));
            Assert.False(RebaseChain.Resolve(x, D, blob, custodied.Skip(1).ToList(), head, cloneGit).Resolved);
            Assert.False(RebaseChain.Resolve(x, D, "ffffffffffffffffffffffffffffffffffffffff", custodied, head, cloneGit).Resolved);
        }
    }
}
