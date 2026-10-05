#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;

namespace RackCad.Tests
{
    /// <summary>A durable point as the validator sees it: the state document and the tree of its commit.</summary>
    public sealed record StatePoint(YamlMap State, IStateTree Tree);

    /// <summary>
    /// The semantic validator of <c>rackcad-automation-state/v2</c> (I-62 F4; Proposal V14 B.8.4 and B.8.8 with the AGREED amendment A-1, commit
    /// ca09ade8, blob c01899a7). It is the implementation that the Core guards C-18 and C-38 exercise and that the reproducible controls run on real
    /// points. File invariants need only the state and the tree of its commit; pair invariants need two consecutive points; history invariants need
    /// Git ancestry and are never evaluated by the Core guards on the repository's own (shallow) history.
    /// <para>
    /// <c>disabled</c> switches rules off in memory. It exists only so that the guards can show that each rule is load-bearing (the mutation checks
    /// of Anexo C); production callers never pass it.
    /// </para>
    /// </summary>
    public sealed partial class StateV2Validator
    {
        private readonly HashSet<string> disabled;

        public StateV2Validator(IEnumerable<string>? disabled = null)
        {
            this.disabled = new HashSet<string>(disabled ?? Array.Empty<string>(), StringComparer.Ordinal);
        }

        private bool On(string rule) => !disabled.Contains(rule);

        /// <summary>Shape (B.8.1) and file invariants I-S01..I-S18 of one durable point.</summary>
        public IReadOnlyList<StateViolation> ValidateFile(StatePoint point)
        {
            var v = new List<StateViolation>();
            var s = point.State;
            foreach (var shape in StateV2Shape.Check(s))
            {
                // I-S02: automation_state follows AUTOMATION_PLAN §8 (its nine fields, types and enums); the rest of the form is B.8.1.
                v.Add(shape.Message.StartsWith("$.automation_state", StringComparison.Ordinal) ? shape with { Invariant = "I-S02" } : shape);
            }

            if (v.Count > 0)
            {
                // A state that is not even well formed is not evaluated further: every later rule would read missing or mistyped fields.
                return v;
            }

            FileCustody(point, v);
            FileOrchestration(point, v);
            return v;
        }

        private void FileCustody(StatePoint point, List<StateViolation> v)
        {
            var s = point.State;
            var tree = point.Tree;
            void Add(string inv, bool ok, string msg)
            {
                if (!ok && On(inv))
                {
                    v.Add(new StateViolation(inv, string.Empty, msg));
                }
            }

            var pt = Y.S(s, "custody.point");
            var rv = Y.N(s, "custody.record_version");
            var attempts = Y.N(s, "automation_state.attempts");
            var claim = Y.S(s, "automation_state.claim_id");

            Add("I-S01", Y.S(s, "schema") == StateV2Shape.Schema && Y.S(s, "protocol.set") == StateV2Shape.ProtocolSet, "schema /v2 ⇔ protocol.set I62");
            Add("I-S01", Y.S(s, "protocol.basis.claim_id") == claim, "protocol.basis.claim_id ≠ automation_state.claim_id");
            if (pt == "BOOTSTRAP")
            {
                Add("I-S01", Y.S(s, "protocol.g0_acceptance.state") == "PENDING" && Y.S(s, "custody.principal.acceptance.state") == "PENDING",
                    "BOOTSTRAP with an acceptance that is not PENDING");
            }

            Add("I-S03", (pt == "Q0") == (Y.S(s, "custody.window.state") == "OPENABLE"), "point Q0 ⇔ window OPENABLE");
            if (pt == "Q0")
            {
                Add("I-S03", Y.M(s, "custody.task_intent") != null && Y.S(s, "custody.principal.state") == "HELD", "Q0 without task_intent or without a HELD principal");
            }

            Add("I-S04", (Y.S(s, "custody.principal.state") == "RELEASED") == (pt == "QH"), "principal RELEASED ⇔ point QH");

            var ti = Y.M(s, "custody.task_intent");
            var launches = Y.L(s, "counters.correction_launches").Cast<YamlMap>().ToList();
            if (ti != null)
            {
                Add("I-S05", Y.N(ti, "attempt") == attempts, "task_intent.attempt ≠ automation_state.attempts");
                if (Y.S(ti, "kind") == "CORRECTION" && pt == "Q0")
                {
                    Add("I-S05", launches.Any(e => Y.N(e, "record_version") == rv && Y.N(e, "attempts_after") == attempts),
                        "Q0 of a CORRECTION without its correction_launches entry at this record_version");
                }

                var roles = Y.L(ti, "planned_roles").Cast<YamlMap>().Select(r => Y.S(r, "role")).ToList();
                Add("I-S06", roles.Count == roles.Distinct().Count() && roles.Contains("EXECUTION_CONTROLLER"), "planned_roles not unique by role or without EXECUTION_CONTROLLER");
            }

            var seq = Y.N(s, "custody.window.seq");
            var lw = Y.M(s, "custody.last_window");
            if (pt != "Q0")
            {
                Add("I-S07", (lw == null) == (seq == 0) && (lw == null || Y.N(lw, "seq") == seq), "last_window and window.seq disagree outside Q0");
            }
            else
            {
                Add("I-S07", (lw == null) == (seq == 1) && (lw == null || Y.N(lw, "seq") == seq - 1), "last_window and window.seq disagree in Q0");
            }

            if (lw != null)
            {
                var closure = Y.S(lw, "closure");
                Add("I-S08", closure != "VERIFIED" || (lw["verification"] != null && lw["verified_sha"] != null), "VERIFIED without verification or verified_sha");
                Add("I-S08", (closure != "NOT_ACCEPTED" && closure != "WITHDRAWN") || lw["delegation_run_id"] == null, "NOT_ACCEPTED or WITHDRAWN with a delegation_run_id");
                Add("I-S08", (closure == "ABANDONED") == (lw["reconstruction"] != null), "ABANDONED ⇔ reconstruction");
                Add("I-S08", lw["journal"] != null || closure == "ABANDONED", "journal null without ABANDONED");
                Add("I-S08", closure == "VERIFIED" || lw["verified_sha"] == null, "verified_sha outside VERIFIED");
            }

            var chains = Y.L(s, "custody.chains").Cast<YamlMap>().ToList();
            Add("I-S09", chains.Select(c => Y.S(c, "task_id")).Distinct().Count() == chains.Count, "chains not unique by task_id");
            Add("I-S09", chains.All(c => c["chain_red_sha"] == null || Y.L(c, "chain_red_files").Count > 0), "chain_red_sha without chain_red_files");

            foreach (var u in Y.L(s, "custody.unverified_commits").Cast<YamlMap>())
            {
                Add("I-S10", chains.Any(c => Y.S(c, "task_id") == Y.S(u, "task_id")), "unverified commit of a task without chain: " + Y.S(u, "task_id"));
                var by = Y.S(u, "superseded_by");
                if (by != null && lw != null && Y.S(lw, "delegation_run_id") == by)
                {
                    Add("I-S10", Y.S(lw, "closure") == "VERIFIED", "superseded_by names a window that is not VERIFIED");
                }
            }

            var blocked = Y.L(s, "counters.blocked_reruns").Cast<YamlMap>().ToList();
            var recoveries = Y.L(s, "counters.rebase_recoveries").Cast<YamlMap>().ToList();
            var invocations = Y.L(s, "counters.invocations").Cast<YamlMap>().ToList();
            Add("I-S11", blocked.Select(b => Y.S(b, "task_id") + "\u0000" + Y.S(b, "phase")).Distinct().Count() == blocked.Count, "blocked_reruns not unique by (task_id, phase)");
            Add("I-S11", recoveries.Select(b => Y.S(b, "task_id")).Distinct().Count() == recoveries.Count, "rebase_recoveries not unique by task_id");
            Add("I-S11", invocations.Select(b => Y.S(b, "scope")).Distinct().Count() == invocations.Count, "invocations not unique by scope");
            Add("I-S11", invocations.All(i => (i["reconstructed"] is true) == (i["reconstruction"] != null)), "reconstructed ⇔ reconstruction");

            Add("I-S12", launches.Select(e => Y.N(e, "seq")).SequenceEqual(Enumerable.Range(1, launches.Count).Select(i => (long?)i)), "correction_launches.seq not consecutive from 1");
            Add("I-S12", launches.Zip(launches.Skip(1), (a, b) => Y.N(a, "attempts_after") < Y.N(b, "attempts_after")).All(x => x)
                         && launches.All(e => Y.N(e, "attempts_after") <= attempts), "attempts_after not strictly increasing or above attempts");

            foreach (var (path, stateRef) in Y.StateRefs(s, "$"))
            {
                Add("I-S13", StateTreeReader.Resolves(tree, stateRef), path + " = " + ((YamlMap)stateRef)["path"] + " does not exist in the tree with its blob");
            }

            var binding = StateTreeReader.Json(tree, Y.Get(s, "custody.principal.binding"));
            Add("I-S14", binding != null && J.S(binding, "Scope") == "UNIT" && J.S(binding, "Role") == "PRINCIPAL_COORDINATOR"
                         && J.S(binding, "Acceptance.State") == Y.S(s, "custody.principal.acceptance.state"),
                "principal.binding is not a UNIT PRINCIPAL_COORDINATOR binding/v1 whose Acceptance.State = principal.acceptance.state");
            Add("I-S14", (Y.Get(s, "custody.principal.designation") == null) == (Y.N(s, "custody.principal.since_record_version") == 1),
                "designation = null ⇔ since_record_version = 1");

            if (pt == "Q0")
            {
                Add("I-S15", Y.S(s, "protocol.g0_acceptance.state") == "ACCEPTED" && Y.S(s, "custody.principal.acceptance.state") == "ACCEPTED", "Q0 without both acceptances ACCEPTED");
            }

            foreach (var (path, marker) in new[] { ("protocol.g0_acceptance", "I62-CLASSIFICATION"), ("custody.principal.acceptance", "I62-PRINCIPAL-BINDING") })
            {
                var state = Y.S(s, path + ".state");
                var decision = Y.Get(s, path + ".decision");
                Add("I-S16", (decision == null) == (state == "PENDING"), path + ": decision = null ⇔ PENDING");
                if (decision != null && On("I-S16"))
                {
                    var text = StateTreeReader.Text(tree, decision) ?? string.Empty;
                    var expected = marker == "I62-CLASSIFICATION"
                        ? "I62-CLASSIFICATION: " + (state == "ACCEPTED" ? "I62" : "REJECTED")
                        : "I62-PRINCIPAL-BINDING: " + (J.S(binding, "BindingId") ?? "<BindingId>") + " " + state;
                    Add("I-S16", text.Contains(expected, StringComparison.Ordinal) && claim != null && text.Contains(claim, StringComparison.Ordinal),
                        path + ".decision does not contain «" + expected + "» and the Claim-Id");
                }
            }

            var pk = Y.S(s, "custody.point_kind");
            Add("I-S17", (pk != null) == (pt == "QU" || pt == "QR"), "point_kind ≠ null ⇔ point ∈ {QU, QR}");
            var lr = Y.M(s, "custody.last_rebase");
            if (pk == "REBASE_RECONCILIATION")
            {
                Add("I-S17", lr != null && Y.N(lr, "record_version") == rv && StateTreeReader.Resolves(tree, lr["map"]),
                    "REBASE_RECONCILIATION without its last_rebase of this record_version or without the map in the tree");
            }

            // A-1 D2-9: «last_rebase ≠ null ⇒ el último elemento de rebase_history[] es last_rebase.map».
            var history = Y.L(s, "custody.rebase_history");
            if (lr != null && On("I-S17"))
            {
                if (history.Count == 0 || !YamlSubset.DeepEquals(history[history.Count - 1], lr["map"]))
                {
                    v.Add(new StateViolation("I-S17", "D2-9", "rebase_history does not end with last_rebase.map"));
                }
            }
        }
    }

    /// <summary>Path access to the YAML tree of a state.</summary>
    internal static class Y
    {
        public static object? Get(YamlMap? map, string dotted)
        {
            object? cur = map;
            foreach (var part in dotted.Split('.'))
            {
                if (cur is not YamlMap m || !m.TryGetValue(part, out cur))
                {
                    return null;
                }
            }

            return cur;
        }

        public static string? S(YamlMap? map, string dotted) => Get(map, dotted) as string;

        public static long? N(YamlMap? map, string dotted) => Get(map, dotted) as long?;

        public static YamlMap? M(YamlMap? map, string dotted) => Get(map, dotted) as YamlMap;

        public static List<object?> L(YamlMap? map, string dotted) => Get(map, dotted) as List<object?> ?? new List<object?>();

        /// <summary>Every StateRef {path, blob} of a tree, with its path (branch objects {commit, path, blob} are not StateRefs).</summary>
        public static IEnumerable<(string Path, YamlMap Ref)> StateRefs(object? node, string path)
        {
            switch (node)
            {
                case YamlMap m when StateV2Shape.IsStateRef(m):
                    yield return (path, m);
                    break;
                case YamlMap m:
                    foreach (var kv in m)
                    {
                        if (path == "$" && kv.Key == "execution_context")
                        {
                            continue; // descriptive; no reader of the protocol consumes it
                        }

                        foreach (var x in StateRefs(kv.Value, path + "." + kv.Key))
                        {
                            yield return x;
                        }
                    }

                    break;
                case List<object?> l:
                    for (var i = 0; i < l.Count; i++)
                    {
                        foreach (var x in StateRefs(l[i], path + "[" + i + "]"))
                        {
                            yield return x;
                        }
                    }

                    break;
            }
        }
    }

    /// <summary>Path access to custodied JSON artifacts.</summary>
    internal static class J
    {
        public static JsonNode? Get(JsonNode? node, string dotted)
        {
            var cur = node;
            foreach (var part in dotted.Split('.'))
            {
                if (cur is not JsonObject o || !o.TryGetPropertyValue(part, out cur))
                {
                    return null;
                }
            }

            return cur;
        }

        public static string? S(JsonNode? node, string dotted) => Get(node, dotted) is JsonValue v && v.TryGetValue<string>(out var s) ? s : null;

        public static long? N(JsonNode? node, string dotted) => Get(node, dotted) is JsonValue v && v.TryGetValue<long>(out var n) ? n : null;
    }
}
