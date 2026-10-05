#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace RackCad.Tests
{
    /// <summary>
    /// What a pair of consecutive durable points cannot show by itself: facts of the window's journal that the caller reads from the custodied
    /// <c>relay-record/v2</c> chain (B.8.3). <see cref="WindowRebaseMaps"/> are the RebaseMaps of the rebases of 16.7 inside the window, in order
    /// (I-P12; A-1 D2-9); <see cref="TransferRecorded"/> says that the window registered a TRANSFER (T12a; I-P07, I-P09).
    /// </summary>
    public sealed record PairContext(bool TransferRecorded = false, IReadOnlyList<JsonObject>? WindowRebaseMaps = null)
    {
        public static readonly PairContext None = new PairContext();
    }

    public sealed partial class StateV2Validator
    {
        private static readonly Dictionary<string, string[]> AllowedTransitions = new Dictionary<string, string[]>(StringComparer.Ordinal)
        {
            ["BOOTSTRAP"] = new[] { "Q0", "QU", "QH", "QR" },
            ["Q0"] = new[] { "Q7", "QR" },
            ["Q7"] = new[] { "Q0", "QU", "QH", "QR" },
            ["QU"] = new[] { "Q0", "QU", "QH", "QR" },
            ["QH"] = new[] { "QR" },
            ["QR"] = new[] { "Q0", "QU", "QH", "QR" },
        };

        private static readonly string[] WindowCounters = { "blocked_reruns", "rebase_recoveries", "invocations" };

        /// <summary>
        /// Pair invariants of B.8.4 for consecutive points p → n (I-P01, I-P02, I-P04..I-P07, I-P09..I-P12) and I-P13 of the orchestration (with A-1).
        /// The file invariants of each point are a separate call; the history ones (I-P03, I-P08, I-H01, I-H02) need Git.
        /// </summary>
        public IReadOnlyList<StateViolation> ValidatePair(StatePoint p, StatePoint n, PairContext? context = null)
        {
            var ctx = context ?? PairContext.None;
            var v = new List<StateViolation>();
            void Add(string inv, bool ok, string msg, string clause = "")
            {
                if (!ok && On(inv))
                {
                    v.Add(new StateViolation(inv, clause, msg));
                }
            }

            var ps = p.State;
            var ns = n.State;
            var pp = Y.S(ps, "custody.point")!;
            var np = Y.S(ns, "custody.point")!;
            var rebase = Y.S(ns, "custody.point_kind") == "REBASE_RECONCILIATION";

            Add("I-P01", Y.N(ns, "custody.record_version") == Y.N(ps, "custody.record_version") + 1, "record_version is not p + 1");
            Add("I-P02", AllowedTransitions.TryGetValue(pp, out var to) && to.Contains(np), "transition " + pp + " → " + np + " is not allowed");
            if (np == "Q0")
            {
                Add("I-P04", Y.N(ns, "custody.window.seq") == Y.N(ps, "custody.window.seq") + 1, "Q0 without window.seq = p + 1");
            }

            foreach (var path in new[] { "protocol.set", "protocol.effective_sha", "protocol.basis", "automation_state.initiative", "automation_state.branch",
                "automation_state.claim_id" })
            {
                Add("I-P06", YamlSubset.DeepEquals(Y.Get(ps, path), Y.Get(ns, path)), path + " is immutable");
            }

            PairCounters(ps, ns, np, rebase, Add);
            PairPrincipal(p, n, ctx, Add);
            if (rebase)
            {
                PairReconciliation(p, n, Add);
            }

            if (pp == "Q0" && (np == "Q7" || np == "QR") && ctx.WindowRebaseMaps is { Count: > 0 } maps)
            {
                PairWindowRebase(p, n, maps, Add);
            }
            else if (!rebase)
            {
                Add("I-P11", YamlSubset.DeepEquals(Y.Get(ps, "custody.rebase_history"), Y.Get(ns, "custody.rebase_history"))
                             && YamlSubset.DeepEquals(Y.Get(ps, "custody.last_rebase"), Y.Get(ns, "custody.last_rebase")),
                    "rebase_history or last_rebase changes without a reconciliation", "D2-9");
            }

            PairOrchestration(p, n, ctx, v);
            return v;
        }

        private void PairCounters(YamlMap ps, YamlMap ns, string np, bool rebase, Action<string, bool, string, string> add)
        {
            void Add(string inv, bool ok, string msg) => add(inv, ok, msg, string.Empty);
            var pa = Y.N(ps, "automation_state.attempts") ?? 0;
            var na = Y.N(ns, "automation_state.attempts") ?? 0;
            var kind = Y.S(ns, "custody.task_intent.kind");
            if (On("I-P05:attempts"))
            {
                Add("I-P05", na >= pa, "attempts decreases");
                if (na != pa)
                {
                    var correction = np == "Q0" && kind == "CORRECTION" && na == pa + 1;
                    var direct = np == "QU" && Y.S(ns, "custody.point_kind") == "ORDINARY" && na > pa;
                    Add("I-P05", correction || direct, "attempts changes outside a Q0 of a CORRECTION (+1) or a QU outside delegated execution");
                }
            }

            var pcl = Y.L(ps, "counters.correction_launches");
            var ncl = Y.L(ns, "counters.correction_launches");
            Add("I-P05", ncl.Count >= pcl.Count && pcl.Select((e, i) => YamlSubset.DeepEquals(e, ncl[i])).All(x => x), "correction_launches is not append-only");
            if (ncl.Count != pcl.Count)
            {
                Add("I-P05", np == "Q0" && kind == "CORRECTION" && ncl.Count == pcl.Count + 1, "correction_launches grows outside the Q0 of a CORRECTION");
            }

            foreach (var c in Y.L(ps, "custody.chains").Cast<YamlMap>())
            {
                var m = Y.L(ns, "custody.chains").Cast<YamlMap>().FirstOrDefault(x => Y.S(x, "task_id") == Y.S(c, "task_id"));
                Add("I-P05", m != null && Y.L(c, "chain_red_files").All(f => Y.L(m, "chain_red_files").Contains(f)), "chain_red_files decreases or a chain disappears");
            }

            foreach (var key in WindowCounters)
            {
                var ids = key == "blocked_reruns" ? new[] { "task_id", "phase" } : key == "rebase_recoveries" ? new[] { "task_id" } : new[] { "scope" };
                var values = key == "invocations" ? new[] { "launched", "uncertain" } : new[] { "count" };
                foreach (var e in Y.L(ps, "counters." + key).Cast<YamlMap>())
                {
                    var m = Y.L(ns, "counters." + key).Cast<YamlMap>().FirstOrDefault(x => ids.All(k => Y.S(x, k) == Y.S(e, k)));
                    Add("I-P05", m != null && values.All(k => Y.N(m, k) >= Y.N(e, k)), "counter " + key + " decreases or disappears");
                }
            }

            if (np == "QU" && !rebase)
            {
                foreach (var path in new[] { "custody.window", "custody.last_window", "custody.chains", "counters.blocked_reruns", "counters.rebase_recoveries",
                    "counters.invocations" })
                {
                    Add("I-P05", YamlSubset.DeepEquals(Y.Get(ps, path), Y.Get(ns, path)), "a QU ORDINARY changes " + path);
                }
            }
        }

        private void PairPrincipal(StatePoint p, StatePoint n, PairContext ctx, Action<string, bool, string, string> add)
        {
            void Add(string inv, bool ok, string msg) => add(inv, ok, msg, string.Empty);
            var ps = p.State;
            var ns = n.State;
            var np = Y.S(ns, "custody.point");
            var newHolder = np == "QR" || (np == "Q7" && ctx.TransferRecorded);
            var pBinding = StateTreeReader.Json(p.Tree, Y.Get(ps, "custody.principal.binding"));
            var nBinding = StateTreeReader.Json(n.Tree, Y.Get(ns, "custody.principal.binding"));
            var pAcc = Y.S(ps, "custody.principal.acceptance.state");
            var nAcc = Y.S(ns, "custody.principal.acceptance.state");
            var acceptanceTransition = np == "QU" && pAcc == "PENDING" && nAcc != "PENDING";

            if (!YamlSubset.DeepEquals(Y.Get(ps, "custody.principal.binding"), Y.Get(ns, "custody.principal.binding")))
            {
                var sameIdDecided = acceptanceTransition && J.S(pBinding, "BindingId") != null && J.S(pBinding, "BindingId") == J.S(nBinding, "BindingId");
                Add("I-P07", newHolder || sameIdDecided, "principal.binding changes outside QR, a Q7 with TRANSFER or the QU of its acceptance (same BindingId)");
            }

            var pg = Y.S(ps, "protocol.g0_acceptance.state");
            var ng = Y.S(ns, "protocol.g0_acceptance.state");
            if (pg != ng && On("I-P09:g0-once"))
            {
                Add("I-P09", pg == "PENDING" && np == "QU", "g0_acceptance changes other than once, from PENDING, in a QU");
                if (ng == "ACCEPTED")
                {
                    var text = StateTreeReader.Text(n.Tree, Y.Get(ns, "protocol.g0_acceptance.decision")) ?? string.Empty;
                    Add("I-P09", text.Contains("I62-CLASSIFICATION: I62", StringComparison.Ordinal)
                                 && text.Contains("I62-DELEGATED-EXECUTION: I62_DELEGATED", StringComparison.Ordinal),
                        "g0_acceptance ACCEPTED without the markers I62-CLASSIFICATION: I62 and I62-DELEGATED-EXECUTION: I62_DELEGATED");
                }
            }

            if (pAcc != nAcc)
            {
                Add("I-P09", (pAcc == "PENDING" && np == "QU") || (newHolder && nAcc == "ACCEPTED"),
                    "principal.acceptance changes other than once per holder (PENDING → decided in a QU, or ACCEPTED for a new holder)");
                if (acceptanceTransition)
                {
                    var text = StateTreeReader.Text(n.Tree, Y.Get(ns, "custody.principal.acceptance.decision")) ?? string.Empty;
                    Add("I-P09", text.Contains("I62-PRINCIPAL-BINDING: " + J.S(nBinding, "BindingId") + " " + nAcc, StringComparison.Ordinal),
                        "principal acceptance without its I62-PRINCIPAL-BINDING marker");
                }
            }

            if (newHolder)
            {
                Add("I-P09", nAcc == "ACCEPTED", "a new holder does not enter ACCEPTED");
            }
        }

        // ------------------------------------------------------------------ reconciliation (I-P05 exception, I-P10; A-1 D2-1, D2-2, D2-7..D2-9)

        /// <summary>
        /// The SHA fields of StateFields (B.8.7) with their canonical names, plus the live orchestration commits that A-1 D2-1 adds. Historical
        /// attempts and terminal requests are not live (D2-1, D2-10).
        /// </summary>
        internal static List<(string Field, string Sha)> StateFieldShas(YamlMap s)
        {
            var f = new List<(string, string)>();
            foreach (var c in Y.L(s, "custody.chains").Cast<YamlMap>())
            {
                f.Add(("chains[" + Y.S(c, "task_id") + "].chain_base_sha", Y.S(c, "chain_base_sha")!));
                if (Y.S(c, "chain_red_sha") is string red)
                {
                    f.Add(("chains[" + Y.S(c, "task_id") + "].chain_red_sha", red));
                }
            }

            if (Y.S(s, "custody.last_window.verified_sha") is string verified)
            {
                f.Add(("last_window.verified_sha", verified));
            }

            var u = Y.L(s, "custody.unverified_commits").Cast<YamlMap>().ToList();
            for (var i = 0; i < u.Count; i++)
            {
                f.Add(("unverified_commits[" + i + "].sha", Y.S(u[i], "sha")!));
            }

            if (Y.S(s, "automation_state.last_evidence_commit") is string evidence)
            {
                f.Add(("last_evidence_commit", evidence));
            }

            return f;
        }

        internal static List<(string Field, string Sha)> LiveOrchestrationShas(StatePoint point)
        {
            var f = new List<(string, string)>();
            var s = point.State;
            if (Y.S(s, "orchestration.loop.object.commit") is string loopCommit)
            {
                f.Add(("orchestration.loop.object.commit", loopCommit));
            }

            foreach (var r in Y.L(s, "orchestration.review_requests").Cast<YamlMap>())
            {
                var id = Y.S(r, "logical_review_request_id");
                if (Y.S(r, "state") == "OPEN")
                {
                    f.Add(("orchestration.review_requests[" + id + "].object.commit", Y.S(r, "object.commit")!));
                }

                foreach (var a in Y.L(r, "attempts").Cast<YamlMap>())
                {
                    var st = Y.S(a, "state");
                    if (st == "INVOCATION_PLANNED" || st == "BUDGET_RESERVED")
                    {
                        var target = J.S(StateTreeReader.Json(point.Tree, a["invocation"]), "Target.commit");
                        if (target != null)
                        {
                            f.Add(("orchestration.review_requests[" + id + "].attempts[" + Y.N(a, "attempt_seq") + "].Target.commit", target));
                        }
                    }
                }
            }

            return f;
        }

        private static bool IsWindowResult(string field) => field == "last_window.verified_sha" || field.EndsWith(".chain_red_sha", StringComparison.Ordinal)
                                                             || field.EndsWith(".chain_base_sha", StringComparison.Ordinal) || field.StartsWith("unverified_commits[", StringComparison.Ordinal);

        private static Dictionary<string, string> Images(JsonObject? map)
        {
            var images = new Dictionary<string, string>(StringComparer.Ordinal);
            if (map?["Commits"] is JsonArray commits)
            {
                foreach (var c in commits.OfType<JsonObject>())
                {
                    var o = J.S(c, "OriginalSha");
                    var i = J.S(c, "ImageSha");
                    if (o != null && i != null && c["PatchIdEqual"] is JsonValue eq && eq.TryGetValue<bool>(out var equal) && equal)
                    {
                        images[o] = i;
                    }
                }
            }

            return images;
        }

        private void PairReconciliation(StatePoint p, StatePoint n, Action<string, bool, string, string> add)
        {
            var ps = p.State;
            var ns = n.State;
            var np = Y.S(ns, "custody.point");
            var map = StateTreeReader.Json(n.Tree, Y.Get(ns, "custody.last_rebase.map"));
            var images = Images(map);

            // I-P10: each SHA field of StateFields is the image of its value in p, or equal if not rewritten; none disappears or turns null.
            var nFields = StateFieldShas(ns).ToDictionary(x => x.Field, x => x.Sha, StringComparer.Ordinal);
            foreach (var (field, sha) in StateFieldShas(ps))
            {
                var ok = nFields.TryGetValue(field, out var now) && (now == sha || (images.TryGetValue(sha, out var img) && img == now));
                add("I-P10", ok, field + " is not the image of p's value by n.last_rebase.map (or disappears)", string.Empty);
            }

            // B.8.7: the map carries a StateFields entry for every SHA field, with the original and the image the state uses.
            var entries = (map?["StateFields"] as JsonArray)?.OfType<JsonObject>().ToList() ?? new List<JsonObject>();
            foreach (var (field, sha) in StateFieldShas(ps).Concat(LiveOrchestrationShas(p)))
            {
                var e = entries.Where(x => J.S(x, "Field") == field).ToList();
                var now = nFields.TryGetValue(field, out var x) ? x : LiveOrchestrationShas(n).FirstOrDefault(y => y.Field == field).Sha;
                add("I-P10", e.Count == 1 && J.S(e[0], "OriginalSha") == sha && J.S(e[0], "ImageSha") == now,
                    "the RebaseMap has no single StateFields entry {" + field + ", " + sha + ", " + now + "}", string.Empty);
            }

            foreach (var path in new[] { "automation_state.attempts", "counters.correction_launches", "counters.blocked_reruns", "counters.rebase_recoveries",
                "counters.invocations" })
            {
                add("I-P10", YamlSubset.DeepEquals(Y.Get(ps, path), Y.Get(ns, path)), path + " changes in a reconciliation", string.Empty);
            }

            foreach (var c in Y.L(ps, "custody.chains").Cast<YamlMap>())
            {
                var m = Y.L(ns, "custody.chains").Cast<YamlMap>().FirstOrDefault(x => Y.S(x, "task_id") == Y.S(c, "task_id"));
                add("I-P10", m != null && Y.S(m, "state") == Y.S(c, "state") && YamlSubset.DeepEquals(c["chain_red_files"], m["chain_red_files"])
                             && YamlSubset.DeepEquals(c["continues_task_id"], m["continues_task_id"]),
                    "a reconciliation changes a chain beyond its SHAs", string.Empty);
            }

            var pti = Y.M(ps, "custody.task_intent");
            var nti = Y.M(ns, "custody.task_intent");
            add("I-P10", YamlSubset.DeepEquals(pti, nti) || nti == null || Y.S(nti, "kind") == "REISSUE",
                "task_intent changes in a reconciliation to something other than a REISSUE or null", string.Empty);

            // A-1 D2-1/D2-4: each live orchestration commit is the image of p's value with the same path and blob, or equal. The stored next_action
            // is a copy derived from the state (I-S18: «NextAction es derivable de forma única»), so its target follows the object it copies.
            var nLive = LiveOrchestrationShas(n).ToDictionary(x => x.Field, x => x.Sha, StringComparer.Ordinal);
            foreach (var (field, sha) in LiveOrchestrationShas(p).Where(x => !x.Field.Contains("].attempts[", StringComparison.Ordinal)))
            {
                var ok = nLive.TryGetValue(field, out var now) && (now == sha || (images.TryGetValue(sha, out var img) && img == now));
                add("I-P10", ok, field + " is not the image of p's value by n.last_rebase.map (or disappears)", "D2-1");
            }

            var pTarget = Y.M(ps, "orchestration.next_action.target");
            var nTarget = Y.M(ns, "orchestration.next_action.target");
            if (!YamlSubset.DeepEquals(pTarget, nTarget))
            {
                var ok = pTarget != null && nTarget != null && Y.S(pTarget, "path") == Y.S(nTarget, "path") && Y.S(pTarget, "blob") == Y.S(nTarget, "blob")
                         && images.TryGetValue(Y.S(pTarget, "commit")!, out var img) && img == Y.S(nTarget, "commit");
                add("I-P10", ok, "next_action.target changes in a reconciliation other than to the image of its commit (same path and blob)", "D2-4");
            }

            // I-P05 exception: nothing else changes (A-1 D2-7 adds the orchestration's live commits, the replanning of §8.8 and rebase_history).
            var allowed = new List<string> { "custody.record_version", "custody.point", "custody.point_kind", "custody.last_rebase", "custody.task_intent",
                "custody.rebase_history", "custody.chains", "custody.last_window.verified_sha", "custody.unverified_commits", "automation_state.last_evidence_commit",
                "orchestration.loop.object.commit", "orchestration.review_requests", "orchestration.next_action.target.commit" };
            if (np == "QR")
            {
                allowed.AddRange(new[] { "custody.principal", "protocol.g0_acceptance.decision" });
                if (Y.S(ps, "custody.point") == "Q0")
                {
                    allowed.AddRange(new[] { "custody.window", "custody.last_window", "counters.invocations", "counters.blocked_reruns" });
                }
            }

            foreach (var changed in Changed(ps, ns).Where(d => !allowed.Any(a => d == a || d.StartsWith(a + ".", StringComparison.Ordinal)
                                                                              || d.StartsWith(a + "[", StringComparison.Ordinal))))
            {
                add("I-P05", false, "a reconciliation changes " + changed + ", outside the SHA fields of StateFields", "D2-7");
            }

            // A-1 D2-9: rebase_history receives exactly the new map, which is n.last_rebase.map.
            var ph = Y.L(ps, "custody.rebase_history");
            var nh = Y.L(ns, "custody.rebase_history");
            add("I-P11", nh.Count == ph.Count + 1 && ph.Select((e, i) => YamlSubset.DeepEquals(e, nh[i])).All(x => x)
                         && YamlSubset.DeepEquals(nh[nh.Count - 1], Y.Get(ns, "custody.last_rebase.map")),
                "rebase_history does not receive exactly the new map", "D2-9");
        }

        private void PairWindowRebase(StatePoint p, StatePoint n, IReadOnlyList<JsonObject> maps, Action<string, bool, string, string> add)
        {
            var ps = p.State;
            var ns = n.State;
            var lr = Y.M(ns, "custody.last_rebase");
            add("I-P12", lr != null && Y.N(lr, "record_version") == Y.N(ns, "custody.record_version"), "the closing point of a window with a rebase does not reconcile it",
                string.Empty);
            var last = maps[maps.Count - 1];
            var mapRef = lr?["map"];
            var custodied = StateTreeReader.Json(n.Tree, mapRef);
            add("I-P12", custodied != null && JsonNode.DeepEquals(custodied, last), "n.last_rebase.map is not the window's custodied RebaseMap", string.Empty);

            // Each SHA field that is not an own result of the window is the image of its value in p through every map of the window, in order.
            var nFields = StateFieldShas(ns).ToDictionary(x => x.Field, x => x.Sha, StringComparer.Ordinal);
            foreach (var (field, sha) in StateFieldShas(ps))
            {
                var img = sha;
                foreach (var m in maps)
                {
                    if (Images(m).TryGetValue(img, out var next))
                    {
                        img = next;
                    }
                }

                add("I-P12", nFields.TryGetValue(field, out var now) && (now == img || IsWindowResult(field)),
                    field + " is not the image of p's value through the window's maps (or disappears)", string.Empty);
            }

            // A-1 D2-9 (A62-A1R-O4): every map of the window, in order, never a composition that loses a step.
            var ph = Y.L(ps, "custody.rebase_history");
            var nh = Y.L(ns, "custody.rebase_history");
            var ok = nh.Count == ph.Count + maps.Count && ph.Select((e, i) => YamlSubset.DeepEquals(e, nh[i])).All(x => x);
            if (ok)
            {
                for (var i = 0; i < maps.Count; i++)
                {
                    var custodiedMap = StateTreeReader.Json(n.Tree, nh[ph.Count + i]);
                    ok &= custodiedMap != null && JsonNode.DeepEquals(custodiedMap, maps[i]);
                }
            }

            add("I-P11", ok, "the window's closing point does not custody every map of the window in order", "D2-9");
        }

        internal static IEnumerable<string> Changed(object? a, object? b, string path = "")
        {
            if (a is YamlMap ma && b is YamlMap mb)
            {
                foreach (var key in ma.Keys.Union(mb.Keys))
                {
                    ma.TryGetValue(key, out var va);
                    mb.TryGetValue(key, out var vb);
                    foreach (var d in Changed(va, vb, path.Length == 0 ? key : path + "." + key))
                    {
                        yield return d;
                    }
                }
            }
            else if (a is List<object?> la && b is List<object?> lb && la.Count == lb.Count)
            {
                for (var i = 0; i < la.Count; i++)
                {
                    foreach (var d in Changed(la[i], lb[i], path + "[" + i + "]"))
                    {
                        yield return d;
                    }
                }
            }
            else if (!YamlSubset.DeepEquals(a, b))
            {
                yield return path;
            }
        }
    }
}
