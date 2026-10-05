#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using static RackCad.Tests.Orchestration;

namespace RackCad.Tests
{
    /// <summary>
    /// One determination of the next action by a frozen rule. A null field is one the state does not determine through that rule (the stored value is
    /// then free); <see cref="TargetDetermined"/> and <see cref="PermissionDetermined"/> distinguish «determined as null» from «not determined».
    /// </summary>
    public sealed record NextActionDetermination(string Rule, string Role, string? Action, bool TargetDetermined, YamlMap? Target, bool PermissionDetermined,
        object? Permission, string? ExpectedOutput);

    /// <summary>
    /// <c>NextAction</c> as a function of the canonical state (Proposal V14 §20.4: «una sola acción»; B.8.8 I-S18: «derivable de forma única»; P-17).
    /// Only what the Freeze fixes is derived, never a new vocabulary: an escalation names the deciding role (I-S18); CORRECTING is
    /// PRINCIPAL / CORRECT_AND_REREVIEW on <c>loop.object</c> (§20.5); a pending review phase whose OPEN request has an attempt in INVOCATION_PLANNED or
    /// BUDGET_RESERVED under an OPEN validity invokes the loop's reviewer (ARCHITECT, or REVIEWER by A-1 D1-16) on that request's object, with the
    /// current authorization as <c>invocation_permission</c> (§20.4) and the output contract of its role (16.23: B.9, B.10). Two determinations with
    /// different roles are a material ambiguity (P-17). Every other state leaves the stored next action free within the other rules of I-S18.
    /// </summary>
    public static class NextActionDerivation
    {
        public static List<NextActionDetermination> Derive(StatePoint point)
        {
            var s = point.State;
            var result = new List<NextActionDetermination>();
            var escalation = Y.S(s, "orchestration.escalation.state");
            if (escalation == "OWNER" || escalation == "COORDINATOR")
            {
                result.Add(new NextActionDetermination("escalation", escalation, null, false, null, false, null, null));
            }

            var loop = Y.M(s, "orchestration.loop");
            var type = Y.S(loop, "type");
            var phase = Y.S(loop, "phase");
            if (type != ArchitectReview && type != Reviewer)
            {
                return result;
            }

            if (phase == "CORRECTING")
            {
                result.Add(new NextActionDetermination("CORRECTING", "PRINCIPAL_COORDINATOR", "CORRECT_AND_REREVIEW", true, Y.M(loop, "object"), false, null, null));
            }

            var open = Y.L(s, "orchestration.review_requests").Cast<YamlMap>().FirstOrDefault(r => Y.S(r, "state") == "OPEN");
            var reservable = open != null && Y.L(open, "attempts").Cast<YamlMap>().Any(a => Y.S(a, "state") is "INVOCATION_PLANNED" or "BUDGET_RESERVED");
            if ((phase == "REVIEW_PENDING" || phase == "REREVIEW_PENDING") && reservable && Y.S(loop, "action_validity.state") == "OPEN")
            {
                var architect = type == ArchitectReview;
                result.Add(new NextActionDetermination("pending review", architect ? "ARCHITECT" : "REVIEWER", null, true, Y.M(open!, "object"), true,
                    loop!["authorization"], architect ? "rackcad-architect-review-result/v1" : "rackcad-reviewer-result/v1"));
            }

            return result;
        }

        /// <summary>The I-S18 «derivable de forma única» problems of a point: ambiguities (P-17) and stored fields that contradict the determination.</summary>
        public static (List<string> Ambiguities, List<string> Mismatches) Check(StatePoint point)
        {
            var determinations = Derive(point);
            var ambiguities = new List<string>();
            var mismatches = new List<string>();
            if (determinations.Select(d => d.Role).Distinct(StringComparer.Ordinal).Count() > 1)
            {
                ambiguities.Add("the state determines more than one next role (" + string.Join(", ", determinations.Select(d => d.Rule + " → " + d.Role)) + ")");
                return (ambiguities, mismatches);
            }

            var stored = Y.M(point.State, "orchestration.next_action");
            foreach (var d in determinations)
            {
                if (Y.S(stored, "role") != d.Role)
                {
                    mismatches.Add(d.Rule + ": next_action.role is " + Y.S(stored, "role") + ", the state determines " + d.Role);
                }

                if (d.Action != null && Y.S(stored, "action") != d.Action)
                {
                    mismatches.Add(d.Rule + ": next_action.action is " + Y.S(stored, "action") + ", the state determines " + d.Action);
                }

                if (d.TargetDetermined && !YamlSubset.DeepEquals(stored?["target"], d.Target))
                {
                    mismatches.Add(d.Rule + ": next_action.target is not the object the state determines");
                }

                if (d.PermissionDetermined && !YamlSubset.DeepEquals(stored?["invocation_permission"], d.Permission))
                {
                    mismatches.Add(d.Rule + ": next_action.invocation_permission is not the current authorization");
                }

                if (d.ExpectedOutput != null && Y.S(stored, "expected_output") != d.ExpectedOutput)
                {
                    mismatches.Add(d.Rule + ": next_action.expected_output is not " + d.ExpectedOutput);
                }
            }

            return (ambiguities, mismatches);
        }
    }
}
