#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using static RackCad.Tests.Orchestration;

namespace RackCad.Tests
{
    /// <summary>A branch reference of an invocation (A-1 D2-2, D2-11): a commit, with the path and blob it must hold when it names a file.</summary>
    public sealed record BranchRef(string Name, string Commit, string? Path, string? Blob);

    public sealed partial class StateV2Validator
    {
        /// <summary>
        /// Invariants of one point that need Git (B.8.4 «con historia»; A-1 D2-3, D2-10, D2-11): I-H01, I-H02 with the live orchestration commits, and
        /// the resolution by ResolveBranchRef of every branch reference of every attempt's invocation and of every materialized binding's
        /// AuthorizationRef. <paramref name="head"/> is the tip of the branch the point is read from.
        /// </summary>
        public IReadOnlyList<StateViolation> ValidateHistory(StatePoint point, IGitHistory git, string head)
        {
            var v = new List<StateViolation>();
            void Add(string inv, bool ok, string msg, string clause = "")
            {
                if (!ok && On(inv) && (clause.Length == 0 || On(clause)))
                {
                    v.Add(new StateViolation(inv, clause, msg));
                }
            }

            var s = point.State;
            var pt = Y.S(s, "custody.point");
            if (pt == "Q0" && Y.Get(s, "protocol.basis.claim_parent_contains_effective") is false)
            {
                Add("I-H01", git.IsAncestor(Y.S(s, "protocol.effective_sha")!, head), "Q0 on an obsolete base whose branch does not contain effective_sha");
            }

            if (pt != "Q0")
            {
                foreach (var (field, sha) in StateFieldShas(s))
                {
                    Add("I-H02", git.IsAncestor(sha, head), field + " = " + Short(sha) + " is not an ancestor of HEAD outside a window");
                }

                foreach (var (field, sha) in LiveOrchestrationShas(point))
                {
                    Add("I-H02", git.IsAncestor(sha, head), field + " = " + Short(sha) + " is not an ancestor of HEAD outside a window", "D2-3");
                }
            }

            var history = RebaseChain.History(point);
            if (history == null)
            {
                Add("I-H02", false, "a map of custody.rebase_history does not resolve in the tree", "D2-9");
                return v;
            }

            foreach (var (r, a) in Attempts(s))
            {
                var tag = Y.S(r, "logical_review_request_id") + "/" + Y.N(a, "attempt_seq");
                foreach (var reference in BranchRefs(Invocation(point, a)))
                {
                    var ok = Resolves(reference, history, head, git, out var why);
                    Add("I-S18", ok, tag + ": " + reference.Name + " does not resolve by ResolveBranchRef (" + why + ")", "D2-11");
                }

                var binding = StateTreeReader.Json(point.Tree, a["binding"]);
                if (J.S(binding, "Acceptance.Basis") == "AUTHORIZED_MATERIALIZATION" && binding?["Acceptance"]?["AuthorizationRef"] is JsonObject authRef)
                {
                    var reference = new BranchRef("AuthorizationRef", J.S(authRef, "Commit") ?? string.Empty, J.S(authRef, "Path"), J.S(authRef, "Blob"));
                    var ok = Resolves(reference, history, head, git, out var why);
                    Add("I-S18", ok, tag + ": the AuthorizationRef of its materialized binding does not resolve (" + why + ")", "D2-11");
                }
            }

            return v;
        }

        /// <summary>
        /// Pair invariants that need Git (I-P03, I-P08; A-1 D2-2 (cont.), D2-6, D2-11 for reconciliations). <paramref name="pCommit"/> and
        /// <paramref name="nCommit"/> are the commits of the two points; the resolution of a reconciliation uses the candidate history of n and the rebased
        /// tip <c>n.last_rebase.branch_after</c>, never the commit of n itself (A62-A1T-01, conditions 9 and 10 of the Coordinator's order).
        /// </summary>
        public IReadOnlyList<StateViolation> ValidatePairHistory(StatePoint p, string pCommit, StatePoint n, string nCommit, IGitHistory git, string statePath)
        {
            var v = new List<StateViolation>();
            void Add(string inv, bool ok, string msg, string clause = "")
            {
                if (!ok && On(inv) && (clause.Length == 0 || On(clause)))
                {
                    v.Add(new StateViolation(inv, clause, msg));
                }
            }

            if (Y.S(p.State, "custody.point") == "Q0")
            {
                Add("I-P03", Y.N(n.State, "custody.last_window.seq") == Y.N(p.State, "custody.window.seq"), "the point after a Q0 does not close its window");

                // The window starts at the Q0, or at its image when the window's own rebase of 16.7 rewrote it (I-P12: last_rebase of n's record_version).
                var start = pCommit;
                if (!git.IsAncestor(pCommit, nCommit))
                {
                    var own = Y.N(n.State, "custody.last_rebase.record_version") == Y.N(n.State, "custody.record_version");
                    var map = own ? StateTreeReader.Json(n.Tree, Y.Get(n.State, "custody.last_rebase.map")) : null;
                    start = map == null ? null : new RebaseMapDoc(map).Entry(pCommit)?.Image;
                }

                if (start == null)
                {
                    Add("I-P03", false, "the Q0 is neither an ancestor of n nor rewritten by the window's own rebase");
                    return v;
                }

                // W-2: the commits of the window are the Worker's — the chain BaseSha..CurrentSha whose results n records (verified_sha,
                // unverified_commits[], the chain_red_sha of the window's task) — and their images; W-3: none of them writes the state file.
                var results = WindowResults(n.State);
                foreach (var c in git.Range(start, nCommit).Where(c => c != nCommit))
                {
                    Add("I-P03", results.Any(r => git.IsAncestor(c, r)), Short(c) + " is not a commit of the Worker inside the window (W-2)");
                    Add("I-P08", !git.ChangedPaths(c).Contains(statePath), Short(c) + ", a commit of the Worker, modifies the state file (W-3)");
                }
            }

            if (Y.S(n.State, "custody.point_kind") != "REBASE_RECONCILIATION")
            {
                return v;
            }

            var history = RebaseChain.History(n) ?? new List<RebaseMapDoc>();
            var tip = Y.S(n.State, "custody.last_rebase.branch_after")!;
            var preqs = Y.L(p.State, "orchestration.review_requests").Cast<YamlMap>().ToDictionary(r => Y.S(r, "logical_review_request_id")!, StringComparer.Ordinal);
            foreach (var r in Y.L(n.State, "orchestration.review_requests").Cast<YamlMap>())
            {
                if (!preqs.TryGetValue(Y.S(r, "logical_review_request_id")!, out var q))
                {
                    continue;
                }

                var qa = Y.L(q, "attempts").Cast<YamlMap>().ToDictionary(a => Y.N(a, "attempt_seq")!.Value);
                foreach (var a in Y.L(r, "attempts").Cast<YamlMap>())
                {
                    if (!qa.TryGetValue(Y.N(a, "attempt_seq")!.Value, out var b))
                    {
                        continue;
                    }

                    var tag = Y.S(r, "logical_review_request_id") + "/" + Y.N(a, "attempt_seq");
                    var st = Y.S(b, "state");
                    if (st == "LAUNCHING")
                    {
                        // D2-2 (cont.): the historical Target resolves by ResolveBranchRef with the whole ordered history of n; the attempt itself is untouched.
                        var target = Target(p, b);
                        var res = target == null
                            ? new ResolveResult(null, "no Target")
                            : RebaseChain.Resolve(Y.S(target, "commit")!, Y.S(target, "path")!, Y.S(target, "blob")!, history, tip, git);
                        Add("I-P13", res.Resolved, tag + ": the Target of a LAUNCHING attempt does not resolve by ResolveBranchRef with the history of n ("
                                                 + res.Reason + ")", "D2-2");
                    }

                    if ((st == "INVOCATION_PLANNED" || st == "BUDGET_RESERVED") && !YamlSubset.DeepEquals(b["invocation"], a["invocation"]))
                    {
                        // D2-2 / D2-11: a replanned invocation is rebuilt entirely on the images: every branch reference is an ancestor of the rebased tip.
                        foreach (var reference in BranchRefs(Invocation(n, a)))
                        {
                            Add("I-P13", git.IsAncestor(reference.Commit, tip) && (reference.Path == null || git.BlobAt(reference.Commit, reference.Path) == reference.Blob),
                                tag + ": the replanned invocation keeps " + reference.Name + " on a pre-rebase commit", "D2-11");
                        }
                    }
                }
            }

            return v;
        }

        /// <summary>
        /// D2-6 (2): after a reconciliation, case B.1 (LAUNCHING → BUDGET_RESERVED) replans on the image — the Target passes to the image of
        /// ResolveBranchRef when the original is no longer an ancestor of HEAD — with a new InvocationId and the same reservation.
        /// </summary>
        public IReadOnlyList<StateViolation> ValidateB1History(StatePoint p, StatePoint n, IGitHistory git, string head)
        {
            var v = new List<StateViolation>();
            var history = RebaseChain.History(n) ?? new List<RebaseMapDoc>();
            var pAtt = Attempts(p.State).ToDictionary(x => Y.S(x.Request, "logical_review_request_id") + "/" + Y.N(x.Attempt, "attempt_seq"), x => x.Attempt);
            foreach (var (r, a) in Attempts(n.State))
            {
                var tag = Y.S(r, "logical_review_request_id") + "/" + Y.N(a, "attempt_seq");
                if (!pAtt.TryGetValue(tag, out var b) || Y.S(b, "state") != "LAUNCHING" || Y.S(a, "state") != "BUDGET_RESERVED")
                {
                    continue;
                }

                var original = Target(p, b);
                var now = Target(n, a);
                var expected = original == null ? null : RebaseChain.Resolve(Y.S(original, "commit")!, Y.S(original, "path")!, Y.S(original, "blob")!, history, head, git);
                var ok = expected is { Resolved: true } && now != null && Y.S(now, "commit") == expected.Commit && Y.S(now, "path") == Y.S(original, "path")
                         && Y.S(now, "blob") == Y.S(original, "blob");
                if (!ok && On("I-P13") && On("D2-6"))
                {
                    v.Add(new StateViolation("I-P13", "D2-6", tag + ": case B.1 does not replan the Target on the image of ResolveBranchRef"));
                }
            }

            return v;
        }

        /// <summary>The branch references of an invocation (D2-2, D2-11): Target, AuthorityRevision, Binding.Location (CUSTODIED) and ReviewSubject.</summary>
        public static List<BranchRef> BranchRefs(JsonObject? invocation)
        {
            var refs = new List<BranchRef>();
            if (invocation == null)
            {
                return refs;
            }

            if (invocation["Target"] is JsonObject t && J.S(t, "commit") is string tc)
            {
                refs.Add(new BranchRef("Target", tc, J.S(t, "path"), J.S(t, "blob")));
            }

            if (J.S(invocation, "AuthorityRevision") is string ar)
            {
                refs.Add(new BranchRef("AuthorityRevision", ar, null, null));
            }

            if (invocation["Binding"]?["Location"] is JsonObject loc && J.S(loc, "Kind") == "CUSTODIED" && J.S(loc, "Commit") is string bc)
            {
                refs.Add(new BranchRef("Binding.Location", bc, J.S(loc, "Path"), J.S(loc, "Blob")));
            }

            if (invocation["IndependenceRequirements"]?["ReviewSubject"] is JsonObject subject)
            {
                foreach (var key in new[] { "Commit", "RangeBase" })
                {
                    if (J.S(subject, key) is string c)
                    {
                        refs.Add(new BranchRef("ReviewSubject." + key, c, null, null));
                    }
                }

                foreach (var commits in (subject["Authors"] as JsonArray)?.OfType<JsonObject>().Select(x => x["Commits"] as JsonArray) ?? Enumerable.Empty<JsonArray?>())
                {
                    foreach (var c in commits?.Select(x => x?.GetValue<string>()).OfType<string>() ?? Enumerable.Empty<string>())
                    {
                        refs.Add(new BranchRef("ReviewSubject.Authors.Commits", c, null, null));
                    }
                }
            }

            return refs;
        }

        private static List<string> WindowResults(YamlMap s)
        {
            var results = new List<string>();
            if (Y.S(s, "custody.last_window.verified_sha") is string verified)
            {
                results.Add(verified);
            }

            results.AddRange(Y.L(s, "custody.unverified_commits").Cast<YamlMap>().Select(u => Y.S(u, "sha")).OfType<string>());
            var task = Y.S(s, "custody.last_window.task_id");
            results.AddRange(Y.L(s, "custody.chains").Cast<YamlMap>().Where(c => Y.S(c, "task_id") == task).Select(c => Y.S(c, "chain_red_sha")).OfType<string>());
            return results;
        }

        private static bool Resolves(BranchRef reference, IReadOnlyList<RebaseMapDoc> history, string head, IGitHistory git, out string why)
        {
            if (reference.Path != null && reference.Blob != null)
            {
                var res = RebaseChain.Resolve(reference.Commit, reference.Path, reference.Blob, history, head, git);
                why = res.Reason;
                return res.Resolved;
            }

            // A commit-only reference (AuthorityRevision, ReviewSubject): D2-10 without the blob step.
            if (git.IsAncestor(reference.Commit, head))
            {
                why = string.Empty;
                return true;
            }

            var image = RebaseChain.Image(reference.Commit, history, git, out why);
            if (image != null && !git.IsAncestor(image, head))
            {
                why = Short(image) + " is not an ancestor of HEAD";
                return false;
            }

            return image != null;
        }

        private static string Short(string sha) => sha.Length > 8 ? sha.Substring(0, 8) : sha;
    }
}
