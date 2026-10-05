#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace RackCad.Tests
{
    /// <summary>P1: the identity facts of a delivery (AUTOMATION_PLAN 16.9 #5, the part that needs no remote).</summary>
    public sealed record IdentityFacts(string? CurrentSha, string? ObjectType, bool? BaseShaIsAncestor, bool? RedShaBetween, IReadOnlyList<string> Failures)
    {
        public string Status => Failures.Count == 0 ? "PASS" : "FAIL";
    }

    /// <summary>One changed path of <c>BaseSha..CurrentSha</c> against the delegation's scopes.</summary>
    public sealed record ScopeRow(string Path, string GitStatus, bool Allowed, IReadOnlyList<string> Forbidden);

    /// <summary>P2: the scope facts of a delivery (AUTOMATION_PLAN 16.9 #7). NOT_EVALUATED never passes.</summary>
    public sealed record ScopeFacts(string Range, IReadOnlyList<ScopeRow> Paths, IReadOnlyList<string> Gaps, IReadOnlyList<string> Failures, string Status);

    /// <summary>
    /// The deterministic helpers P1, P2 and P3 that the Coordinator placed inside the Controller's verification boundary (decisions §43: I-63 P1/P2/P3,
    /// IN_SCOPE_CURRENT_FREEZE). They <b>extract facts</b> for checks #5 (<c>Identity</c>) and #7 (<c>Scope</c>) of 16.9 exactly as frozen; they are
    /// not a role, service, daemon or authority, and never produce <c>Classification</c>, <c>Disposition</c> or any verdict: judgement, independence and
    /// classification stay with the Controller (P3). Known gaps are reported, never resolved with new semantics (no case folding, no glob or link
    /// interpretation). Port of the F4 preparation prototype <c>I-62-prep/night-2026-10-05/helpers/i62_helpers.py</c>.
    /// </summary>
    public static class VerificationFacts
    {
        private static readonly Regex Hex40 = new Regex("^[0-9a-f]{40}$");

        public static readonly string[] VerdictKeys = { "Classification", "Disposition", "VerifiedSha", "GatePass", "Verdict", "FailureClass" };

        public static readonly string[] VerdictValues =
            { "EXECUTION_VERIFIED", "EXECUTION_REWORK_REQUIRED", "EXECUTION_BLOCKED", "GATE PASS", "GATE_PASS", "AGREED", "CONFORMING" };

        /// <summary>
        /// P1. <c>CurrentSha</c> comes only from the structured delivery and is checked against Git: a 40-hex lowercase commit that exists, with
        /// <c>BaseSha</c> (of the delegation, else of the delivery) as an ancestor and <c>RedSha</c>, when present, between both. There is deliberately
        /// no prompt or narrative parameter: a SHA written in prose never rescues an invalid structured field, and is never substituted for it.
        /// </summary>
        public static IdentityFacts Identity(JsonObject handoff, JsonObject? delegation, IGitHistory git)
        {
            var failures = new List<string>();
            var sha = handoff["CurrentSha"] is JsonValue v && v.TryGetValue<string>(out var s) ? s : null;
            if (sha == null || !Hex40.IsMatch(sha))
            {
                failures.Add("CurrentSha is absent or not a 40-hex lowercase SHA");
                return new IdentityFacts(sha, null, null, null, failures);
            }

            var type = git.ObjectType(sha);
            if (type != "commit")
            {
                failures.Add("CurrentSha is not an existing commit");
                return new IdentityFacts(sha, type, null, null, failures);
            }

            var baseSha = J.S(delegation, "BaseSha") ?? J.S(handoff, "BaseSha");
            bool? ancestor = null;
            if (baseSha != null)
            {
                ancestor = git.IsAncestor(baseSha, sha);
                if (ancestor == false)
                {
                    failures.Add("BaseSha is not an ancestor of CurrentSha");
                }
            }

            bool? between = null;
            if (J.S(handoff, "RedSha") is string red && baseSha != null)
            {
                between = git.IsAncestor(baseSha, red) && git.IsAncestor(red, sha);
                if (between == false)
                {
                    failures.Add("RedSha is not between BaseSha and CurrentSha");
                }
            }

            return new IdentityFacts(sha, type, ancestor, between, failures);
        }

        private static bool Matches(string path, string entry) => entry.EndsWith('/') ? path.StartsWith(entry, StringComparison.Ordinal) : path == entry;

        private static bool MatchesIgnoringCase(string path, string entry) =>
            entry.EndsWith('/') ? path.StartsWith(entry, StringComparison.OrdinalIgnoreCase) : string.Equals(path, entry, StringComparison.OrdinalIgnoreCase);

        /// <summary>
        /// P2. The changed paths of <c>BaseSha..CurrentSha</c> (no rename detection: a rename is a delete plus an add; deletions and mode changes are
        /// writes) against <c>AllowedWriteScope</c> and <c>ForbiddenWriteScope</c> of the delegation being verified; Forbidden wins; exact,
        /// case-sensitive comparison, a prefix only when the entry ends in <c>/</c>. Absent scope, an unrun comparison or an uninterpreted entry
        /// (glob characters) is NOT_EVALUATED, which never passes.
        /// </summary>
        public static ScopeFacts Scope(string? baseSha, string? currentSha, JsonObject delegation, IGitHistory git)
        {
            var range = baseSha + ".." + currentSha;
            var gaps = new List<string>();
            var failures = new List<string>();
            var rows = new List<ScopeRow>();
            var allowed = (delegation["AllowedWriteScope"] as JsonArray)?.Select(x => x?.GetValue<string>()).OfType<string>().ToList();
            var forbidden = (delegation["ForbiddenWriteScope"] as JsonArray)?.Select(x => x?.GetValue<string>()).OfType<string>().ToList() ?? new List<string>();
            if (allowed == null || allowed.Count == 0)
            {
                failures.Add("the delegation has no AllowedWriteScope");
                return new ScopeFacts(range, rows, gaps, failures, "NOT_EVALUATED");
            }

            gaps.AddRange(allowed.Concat(forbidden).Where(e => e.IndexOfAny(new[] { '*', '?', '[' }) >= 0).Select(e => "entry with glob characters not interpreted: " + e));
            if (gaps.Count > 0)
            {
                return new ScopeFacts(range, rows, gaps, failures, "NOT_EVALUATED");
            }

            var diff = baseSha == null || currentSha == null ? null : git.DiffRaw(baseSha, currentSha);
            if (diff == null)
            {
                failures.Add("the comparison was not run");
                return new ScopeFacts(range, rows, gaps, failures, "NOT_EVALUATED");
            }

            foreach (var d in diff)
            {
                var isAllowed = allowed.Any(e => Matches(d.Path, e));
                var hits = forbidden.Where(e => Matches(d.Path, e)).ToList();
                rows.Add(new ScopeRow(d.Path, d.Status, isAllowed, hits));
                if (d.OldMode is "120000" or "160000" || d.NewMode is "120000" or "160000")
                {
                    gaps.Add("non-regular entry (link or gitlink), not resolved: " + d.Path);
                }

                if (!isAllowed && allowed.Any(e => MatchesIgnoringCase(d.Path, e)))
                {
                    gaps.Add("match only ignoring case (not interpreted): " + d.Path);
                }

                if (!isAllowed)
                {
                    failures.Add("outside AllowedWriteScope: " + d.Path + " (" + d.Status + ")");
                }

                if (hits.Count > 0)
                {
                    failures.Add("in ForbiddenWriteScope: " + d.Path);
                }
            }

            var status = failures.Count > 0 ? "FAIL" : gaps.Count > 0 ? "NOT_EVALUATED" : "PASS";
            return new ScopeFacts(range, rows, gaps, failures, status);
        }

        /// <summary>P3: every verdict field or value in a helper output (empty = facts only).</summary>
        public static List<string> VerdictProblems(JsonNode? node, string at = "$")
        {
            var problems = new List<string>();
            switch (node)
            {
                case JsonObject o:
                    foreach (var (key, child) in o)
                    {
                        if (VerdictKeys.Contains(key, StringComparer.Ordinal))
                        {
                            problems.Add("verdict field in a helper output: " + at + "." + key);
                        }

                        problems.AddRange(VerdictProblems(child, at + "." + key));
                    }

                    break;
                case JsonArray a:
                    for (var i = 0; i < a.Count; i++)
                    {
                        problems.AddRange(VerdictProblems(a[i], at + "[" + i + "]"));
                    }

                    break;
                case JsonValue value when value.TryGetValue<string>(out var text) && VerdictValues.Contains(text.Trim().ToUpperInvariant(), StringComparer.Ordinal):
                    problems.Add("verdict value in a helper output: " + at + " = " + text);
                    break;
            }

            return problems;
        }

        /// <summary>P3: the facts bundle for the Controller — P1, and P2 only when the identity holds. It carries no verdict (checked).</summary>
        public static JsonObject Facts(JsonObject handoff, JsonObject delegation, IGitHistory git)
        {
            var identity = Identity(handoff, delegation, git);
            var scope = identity.Status == "PASS"
                ? Scope(J.S(delegation, "BaseSha"), identity.CurrentSha, delegation, git)
                : new ScopeFacts(string.Empty, new List<ScopeRow>(), new List<string>(), new List<string> { "identity not established" }, "NOT_EVALUATED");
            var facts = new JsonObject
            {
                ["Kind"] = "FACTS",
                ["Identity"] = new JsonObject
                {
                    ["Status"] = identity.Status, ["CurrentSha"] = identity.CurrentSha, ["ObjectType"] = identity.ObjectType,
                    ["BaseShaIsAncestor"] = identity.BaseShaIsAncestor, ["RedShaBetween"] = identity.RedShaBetween,
                    ["Failures"] = new JsonArray(identity.Failures.Select(f => (JsonNode)f).ToArray()),
                },
                ["Scope"] = new JsonObject
                {
                    ["Status"] = scope.Status, ["Range"] = scope.Range,
                    ["Paths"] = new JsonArray(scope.Paths.Select(r => (JsonNode)new JsonObject { ["Path"] = r.Path, ["GitStatus"] = r.GitStatus, ["Allowed"] = r.Allowed,
                        ["Forbidden"] = new JsonArray(r.Forbidden.Select(f => (JsonNode)f).ToArray()) }).ToArray()),
                    ["Gaps"] = new JsonArray(scope.Gaps.Select(g => (JsonNode)g).ToArray()),
                    ["Failures"] = new JsonArray(scope.Failures.Select(f => (JsonNode)f).ToArray()),
                },
                ["Note"] = "mechanical facts; judgement, independence and classification belong to the Controller",
            };
            var problems = VerdictProblems(facts);
            if (problems.Count > 0)
            {
                throw new InvalidOperationException(string.Join("; ", problems));
            }

            return facts;
        }
    }
}
