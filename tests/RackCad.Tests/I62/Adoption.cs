#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace RackCad.Tests
{
    /// <summary>
    /// The state-format changes of applicability and adoption (AUTOMATION_PLAN 16.28; Proposal V14 §8.7, T20, T21), which lie outside the pairs of
    /// <c>/v2</c>: a later adoption replaces a <c>/v1</c> DIRECT_ONLY state with a BOOTSTRAP of adoption, and a DIRECT_ONLY decision replaces a BOOTSTRAP
    /// with a <c>/v1</c> state. Both keep the nine fields of <c>automation_state</c>.
    /// </summary>
    public static class Adoption
    {
        public static readonly string[] NineFields =
            { "initiative", "branch", "claim_id", "current_phase", "state", "gate", "attempts", "next_action", "last_evidence_commit" };

        private static List<string> NineFieldProblems(YamlMap a, YamlMap b) =>
            NineFields.Where(f => !YamlSubset.DeepEquals(Y.Get(a, "automation_state." + f), Y.Get(b, "automation_state." + f)))
                .Select(f => "automation_state." + f + " is not kept").ToList();

        /// <summary>T20: the BOOTSTRAP of a later adoption from a <c>/v1</c> DIRECT_ONLY state.</summary>
        public static List<string> T20Problems(YamlMap v1, YamlMap bootstrap)
        {
            var problems = new List<string>();
            if (Y.S(v1, "schema") != "rackcad-automation-state/v1")
            {
                problems.Add("the adopted state is not /v1");
            }

            problems.AddRange(NineFieldProblems(v1, bootstrap));
            if (Y.S(bootstrap, "custody.point") != "BOOTSTRAP" || Y.N(bootstrap, "custody.record_version") != 1)
            {
                problems.Add("the adoption does not start with a BOOTSTRAP of record_version 1");
            }

            if (Y.S(bootstrap, "protocol.basis.adoption_at") != "MID_INITIATIVE")
            {
                problems.Add("protocol.basis.adoption_at is not MID_INITIATIVE");
            }

            if (Y.S(bootstrap, "protocol.g0_acceptance.state") != "PENDING" || Y.S(bootstrap, "custody.principal.acceptance.state") != "PENDING")
            {
                problems.Add("the acceptances of an adoption start PENDING");
            }

            if (Y.N(bootstrap, "custody.window.seq") != 0 || Y.L(bootstrap, "custody.chains").Count > 0
                || new[] { "correction_launches", "blocked_reruns", "rebase_recoveries", "invocations" }.Any(c => Y.L(bootstrap, "counters." + c).Count > 0))
            {
                problems.Add("window counters must start empty: earlier direct commits are not delegated work");
            }

            return problems;
        }

        /// <summary>T21: a DIRECT_ONLY decision on a BOOTSTRAP (both acceptances PENDING and no window) returns to an ordinary <c>/v1</c> state.</summary>
        public static List<string> T21Problems(YamlMap bootstrap, YamlMap v1)
        {
            var problems = new List<string>();
            if (Y.S(bootstrap, "custody.point") != "BOOTSTRAP" || Y.S(bootstrap, "protocol.g0_acceptance.state") != "PENDING"
                || Y.S(bootstrap, "custody.principal.acceptance.state") != "PENDING" || Y.N(bootstrap, "custody.window.seq") != 0)
            {
                problems.Add("T21 applies only to a BOOTSTRAP with both acceptances PENDING and window.seq 0");
            }

            if (Y.S(v1, "schema") != "rackcad-automation-state/v1" || v1.TryGetValue("protocol", out _) || v1.TryGetValue("custody", out _)
                || v1.TryGetValue("counters", out _) || v1.TryGetValue("orchestration", out _))
            {
                problems.Add("the returned state is not an ordinary /v1 (without protocol, custody, counters or orchestration)");
            }

            problems.AddRange(NineFieldProblems(bootstrap, v1));
            return problems;
        }
    }
}
