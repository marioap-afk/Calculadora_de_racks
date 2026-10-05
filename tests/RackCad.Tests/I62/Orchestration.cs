#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json.Nodes;

namespace RackCad.Tests
{
    /// <summary>
    /// Constants and readers of the orchestration of <c>rackcad-automation-state/v2</c> (Proposal V14 §20.4-§20.6, B.8.8, with the AGREED A-1): the
    /// frozen caps, the phase and attempt edges, and the custodied decision entries (markers) that the validator reads by StateRef.
    /// </summary>
    public static class Orchestration
    {
        public const string ArchitectReview = "ARCHITECT_REVIEW";
        public const string Reviewer = "REVIEWER";
        public const string None = "NONE";

        /// <summary>§20.6 frozen caps, including the two derived ones (MAX_REVIEW_ROUNDS − 1 and MAX_LOGICAL × (1 + MAX_RERUNS)).</summary>
        public static readonly IReadOnlyDictionary<string, long> FrozenCaps = new Dictionary<string, long>(StringComparer.Ordinal)
        {
            ["review_rounds"] = 3, ["logical_requests"] = 3, ["transport_reruns_per_request"] = 2, ["corrections_per_lineage"] = 2,
            ["correction_rounds"] = 2, ["architect_launches"] = 9,
        };

        public static readonly string[] Terminal = { "RESULT_INGESTED", "LAUNCH_UNCERTAIN", "CANCELLED_BEFORE_LAUNCH" };
        public static readonly string[] NotLaunched = { "INVOCATION_PLANNED", "BUDGET_RESERVED" };
        public static readonly string[] Launched = { "LAUNCHING", "LAUNCHED", "LAUNCH_UNCERTAIN", "RESULT_RECEIVED", "RESULT_INGESTED" };

        /// <summary>
        /// Phase edges: the diagram of §20.5 (the ingestion QU publishes the next phase directly), the two recovery edges of SM-05, the transport rerun
        /// after an INVALID result and the second request after an Owner decision (§20.6), and REVIEWER_SATISFIED for the REVIEWER (A-1 D1-16).
        /// LOOP_CLOSED (→ NONE) and the opening (NONE → REVIEW_PENDING) are checked by their own rules.
        /// </summary>
        public static readonly HashSet<(string, string)> PhaseEdges = new HashSet<(string, string)>
        {
            ("REVIEW_PENDING", "ARCHITECT_INVOKED"), ("ARCHITECT_INVOKED", "RESULT_INGESTED"), ("ARCHITECT_INVOKED", "ARCHITECT_SATISFIED"),
            ("ARCHITECT_INVOKED", "CORRECTING"), ("ARCHITECT_INVOKED", "ESCALATE_OWNER"), ("RESULT_INGESTED", "ARCHITECT_SATISFIED"),
            ("RESULT_INGESTED", "CORRECTING"), ("RESULT_INGESTED", "ESCALATE_OWNER"), ("CORRECTING", "PUBLISHED"), ("PUBLISHED", "CI_VERIFIED"),
            ("CI_VERIFIED", "REREVIEW_PENDING"), ("REREVIEW_PENDING", "ARCHITECT_INVOKED"), ("ARCHITECT_INVOKED", "REVIEW_PENDING"),
            ("ARCHITECT_INVOKED", "REREVIEW_PENDING"), ("RESULT_INGESTED", "REVIEW_PENDING"), ("RESULT_INGESTED", "REREVIEW_PENDING"),
            ("ESCALATE_OWNER", "REREVIEW_PENDING"), ("ARCHITECT_INVOKED", "REVIEWER_SATISFIED"), ("RESULT_INGESTED", "REVIEWER_SATISFIED"),
        };

        public static readonly HashSet<(string, string)> AttemptEdges = new HashSet<(string, string)>
        {
            ("INVOCATION_PLANNED", "BUDGET_RESERVED"), ("INVOCATION_PLANNED", "CANCELLED_BEFORE_LAUNCH"), ("BUDGET_RESERVED", "LAUNCHING"),
            ("BUDGET_RESERVED", "CANCELLED_BEFORE_LAUNCH"), ("BUDGET_RESERVED", "BUDGET_RESERVED"), ("LAUNCHING", "LAUNCHED"), ("LAUNCHING", "RESULT_RECEIVED"),
            ("LAUNCHING", "LAUNCH_UNCERTAIN"), ("LAUNCHING", "BUDGET_RESERVED"), ("LAUNCHING", "CANCELLED_BEFORE_LAUNCH"), ("LAUNCHED", "RESULT_RECEIVED"),
            ("LAUNCHED", "LAUNCH_UNCERTAIN"), ("RESULT_RECEIVED", "RESULT_INGESTED"),
        };

        /// <summary>
        /// The fields of one decision entry: the fenced block of a decisions file whose lines contain exactly <paramref name="markerLine"/>. Lines are
        /// <c>Key: value</c>; the marker line itself is kept under its marker key. Null when the reference does not resolve or no block carries it.
        /// </summary>
        public static Dictionary<string, string>? DecisionBlock(IStateTree tree, object? stateRef, string markerLine)
        {
            var text = StateTreeReader.Text(tree, stateRef);
            if (text == null)
            {
                return null;
            }

            var lines = text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
            for (var i = 0; i < lines.Length; i++)
            {
                if (!lines[i].StartsWith("```", StringComparison.Ordinal))
                {
                    continue;
                }

                var block = new List<string>();
                var j = i + 1;
                while (j < lines.Length && !lines[j].StartsWith("```", StringComparison.Ordinal))
                {
                    block.Add(lines[j]);
                    j++;
                }

                if (block.Any(l => l.Trim() == markerLine))
                {
                    var fields = new Dictionary<string, string>(StringComparer.Ordinal);
                    foreach (var l in block)
                    {
                        var k = l.IndexOf(": ", StringComparison.Ordinal);
                        if (k > 0)
                        {
                            fields[l.Substring(0, k).Trim()] = l.Substring(k + 2).Trim();
                        }
                    }

                    return fields;
                }

                i = j;
            }

            return null;
        }

        /// <summary>The ReviewLoopAuthorization entry of an authorization id (marker of §20.5; fields of D1-12), or null.</summary>
        public static Dictionary<string, string>? Authorization(IStateTree tree, object? stateRef, string authorizationId) =>
            DecisionBlock(tree, stateRef, "I62-REVIEW-LOOP-AUTHORIZATION: " + authorizationId);

        /// <summary>D1-2: caps = the component-wise minimum of the frozen constants and the <c>Budget.*</c> of each authorization.</summary>
        public static Dictionary<string, long>? EffectiveCaps(IStateTree tree, IEnumerable<YamlMap> authorizations)
        {
            var caps = FrozenCaps.ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.Ordinal);
            foreach (var a in authorizations)
            {
                var entry = Authorization(tree, a["authorization"], Y.S(a, "authorization_id") ?? string.Empty);
                if (entry == null)
                {
                    return null;
                }

                foreach (var key in FrozenCaps.Keys)
                {
                    if (entry.TryGetValue("Budget." + key, out var raw))
                    {
                        if (!long.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out var value))
                        {
                            return null;
                        }

                        caps[key] = Math.Min(caps[key], value);
                    }
                }
            }

            return caps;
        }

        /// <summary>D1-20: the identity of a REVIEWER authority is the StateRef of its gate contract plus the role.</summary>
        public static string? ReviewerAuthorityId(object? authorization) =>
            StateV2Shape.IsStateRef(authorization) ? Y.S((YamlMap)authorization!, "path") + "@" + Y.S((YamlMap)authorization!, "blob") + "#REVIEWER" : null;

        /// <summary>The decisions file of the unit in a tree (where supersession markers are read), as a StateRef, or null.</summary>
        public static YamlMap? DecisionsRef(StatePoint point)
        {
            var path = "docs/automation/decisions/" + Y.S(point.State, "automation_state.initiative") + ".md";
            if (!point.Tree.TryRead(path, out var bytes))
            {
                return null;
            }

            var m = new YamlMap();
            m.Add("path", path);
            m.Add("blob", I62Repo.GitBlobSha1(bytes));
            return m;
        }

        /// <summary>
        /// Equality of state fragments across a pair p → n that tolerates the refresh of a StateRef to a decisions file. Decisions files only grow by
        /// appended entries, and I-S13 obliges every point to cite the blob of its own tree; so a StateRef to a decisions file whose blob changed from
        /// p to n is the same reference when n's file is an append-only extension of p's (both blobs are in the pair's trees). Any other change,
        /// including a rewrite of an earlier entry, still differs.
        /// </summary>
        public sealed class PairRefs
        {
            private readonly HashSet<string> appendOnly = new HashSet<string>(StringComparer.Ordinal);

            public PairRefs(StatePoint p, StatePoint n)
            {
                foreach (var (_, r) in Y.StateRefs(p.State, "$"))
                {
                    var path = Y.S(r, "path")!;
                    if (IsDecisionsRef(r) && !appendOnly.Contains(path) && p.Tree.TryRead(path, out var before) && n.Tree.TryRead(path, out var after)
                        && after.Length >= before.Length && after.AsSpan(0, before.Length).SequenceEqual(before))
                    {
                        appendOnly.Add(path);
                    }
                }
            }

            public bool Eq(object? a, object? b) => YamlSubset.DeepEquals(Norm(a), Norm(b));

            private object? Norm(object? node)
            {
                switch (node)
                {
                    case YamlMap m when StateV2Shape.IsStateRef(m) && appendOnly.Contains(Y.S(m, "path")!):
                        var c = new YamlMap();
                        c.Add("path", m["path"]);
                        c.Add("blob", "append-only");
                        return c;
                    case YamlMap m:
                        var copy = new YamlMap();
                        foreach (var kv in m)
                        {
                            copy.Add(kv.Key, Norm(kv.Value));
                        }

                        return copy;
                    case List<object?> l:
                        return l.Select(Norm).ToList();
                    default:
                        return node;
                }
            }
        }

        public static JsonObject? Invocation(StatePoint point, YamlMap attempt) => StateTreeReader.Json(point.Tree, attempt["invocation"]);

        public static JsonObject? Result(StatePoint point, YamlMap attempt) => StateTreeReader.Json(point.Tree, attempt["result"]);

        /// <summary>The Target of an attempt's invocation as {commit, path, blob}, or null.</summary>
        public static YamlMap? Target(StatePoint point, YamlMap attempt)
        {
            var t = Invocation(point, attempt)?["Target"] as JsonObject;
            if (t == null || J.S(t, "commit") == null)
            {
                return null;
            }

            var m = new YamlMap();
            m.Add("commit", J.S(t, "commit"));
            m.Add("path", J.S(t, "path"));
            m.Add("blob", J.S(t, "blob"));
            return m;
        }

        /// <summary>The object a review result evaluated (B.10.0: its Reviewed* fields), or null.</summary>
        public static YamlMap? Evaluated(JsonObject? result)
        {
            if (result == null || J.S(result, "ReviewedCommit") == null)
            {
                return null;
            }

            var m = new YamlMap();
            m.Add("commit", J.S(result, "ReviewedCommit"));
            m.Add("path", J.S(result, "ReviewedPath"));
            m.Add("blob", J.S(result, "ReviewedBlob"));
            return m;
        }

        public static string? ResultSchema(JsonObject? result) => J.S(result, "Schema");

        public static bool IsDecisionsRef(object? stateRef) =>
            StateV2Shape.IsStateRef(stateRef) && (Y.S((YamlMap)stateRef!, "path") ?? string.Empty).StartsWith("docs/automation/decisions/", StringComparison.Ordinal);

        public static IEnumerable<(YamlMap Request, YamlMap Attempt)> Attempts(YamlMap state) =>
            Y.L(state, "orchestration.review_requests").Cast<YamlMap>().SelectMany(r => Y.L(r, "attempts").Cast<YamlMap>().Select(a => (r, a)));

        /// <summary>
        /// A-1 D1-17 (with A62-A1T-O1): the requests of the active REVIEWER loop — loop_instance_id null, opened in the pair that opened the loop or later,
        /// reconstructed without the pair history as the null-loop requests after the last_request of the last reviewer_closures record (all of them if
        /// there is none), in the append-only order of review_requests[].
        /// </summary>
        public static List<YamlMap> ReviewerLoopRequests(YamlMap state)
        {
            var nullLoop = Y.L(state, "orchestration.review_requests").Cast<YamlMap>().Where(r => r["loop_instance_id"] == null).ToList();
            var closures = Y.L(state, "orchestration.reviewer_closures").Cast<YamlMap>().ToList();
            if (closures.Count == 0)
            {
                return nullLoop;
            }

            var last = Y.S(closures[closures.Count - 1], "last_request");
            var k = nullLoop.FindIndex(r => Y.S(r, "logical_review_request_id") == last);
            return k < 0 ? new List<YamlMap>() : nullLoop.Skip(k + 1).ToList();
        }

        /// <summary>
        /// D1-17/D1-19 (A62-A1R-O5): the operational requirement of a REVIEWER authority is satisfied ONLY with positive REVIEWER_SATISFIED evidence of
        /// that authority — the live phase or a closure record with that ended_reason — never inferred from the absence of an open BLOCKING.
        /// </summary>
        public static bool ReviewerRequirementSatisfied(YamlMap state, string authorizationId)
        {
            var loop = Y.M(state, "orchestration.loop");
            if (Y.S(loop, "type") == Reviewer && Y.S(loop, "phase") == "REVIEWER_SATISFIED" && Y.S(loop, "action_validity.authorization_id") == authorizationId)
            {
                return true;
            }

            return Y.L(state, "orchestration.reviewer_closures").Cast<YamlMap>()
                .Any(c => Y.S(c, "validity.authorization_id") == authorizationId && Y.S(c, "validity.ended_reason") == "REVIEWER_SATISFIED");
        }
    }
}
