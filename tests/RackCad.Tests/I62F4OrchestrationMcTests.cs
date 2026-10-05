#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using static RackCad.Tests.OrchestrationSamples;
using static RackCad.Tests.StateV2Samples;

namespace RackCad.Tests
{
    /// <summary>
    /// I-62 F4 (slice F4-G), the orchestration rows of the C matrix that F4 owns, run as guards over synthetic points of the F.8 loop: C-33 (a BLOCKED —
    /// OWNER DECISION result escalates with the exact decision and nothing is corrected or invoked), C-34 (every cap of §20.6 stops before reserving),
    /// C-35 (an invalid output of the Architect does not advance: no lineage, no phase), C-36 (a change of provider, model, Principal or label continues
    /// the counters) and C-40 in its F4 part (the action validity of an authorized materialization: a launched attempt completes after a revocation, an
    /// unlaunched one is cancelled and never launched; at the end of the validity the historical accreditation stands and each LAUNCHING attempt goes to
    /// the destination of the invoker's evidence, V14 §20.5.1: (e), (g), (j)–(n)), plus C-30 (e) in the state: a result with the output contract of another
    /// role is never ingested as VALID.
    /// </summary>
    public class I62F4OrchestrationMcTests
    {
        private static List<string> Ids(IEnumerable<StateViolation> v) => v.Select(x => x.Invariant + (x.Clause.Length > 0 ? "/" + x.Clause : string.Empty)).Distinct().ToList();

        private static List<string> Pair(StatePoint p, StatePoint n) =>
            Ids(new StateV2Validator().ValidateFile(n).Concat(new StateV2Validator().ValidatePair(p, n)));

        private static (StatePoint S6, StatePoint S7) Escalated()
        {
            var s6 = Next(F8Step(4));
            var res = ArchitectResult(s6, "docs/automation/evidence/I-99-agent/review/L1/1/result.json", L1, Obj(C1, X, B1), "BLOCKED — OWNER DECISION",
                required: new string[0], dispositions: new (string, string)[0]);
            Received(s6, AttemptOf(s6, L1, 1), res);
            var s7 = Next(s6);
            var a = AttemptOf(s7, L1, 1);
            a["state"] = "RESULT_INGESTED";
            a["outcome"] = "VALID";
            a["ingested_at"] = Rv(s7);
            RequestOf(s7, L1)["state"] = "INGESTED";
            Loop(s7)["phase"] = "ESCALATE_OWNER";
            Orch(s7)["escalation"] = M(("state", "OWNER"), ("reason", "el Architect reserva la decisión al Owner"),
                ("required_decision", "aceptar o rechazar el alcance de X v1"));
            Orch(s7)["next_action"] = NextAction("OWNER", "DECIDE", null);
            return (s6, s7);
        }

        [Fact]
        public void I62_C33_AnOwnerReservedResultEscalatesWithTheExactDecisionAndNothingIsCorrectedOrInvoked()
        {
            var (s6, s7) = Escalated();
            Assert.Empty(Pair(F8Step(4), s6));
            Assert.Empty(Pair(s6, s7));
            Assert.Equal("OWNER", NextActionDerivation.Derive(s7).Single().Role);

            // Without the escalation the role OWNER is inconsistent (I-S18); with it, the Principal cannot correct before the decision.
            var silent = Copy(s7);
            Orch(silent)["escalation"] = M(("state", "NONE"), ("reason", null), ("required_decision", null));
            Assert.Contains("I-S18", Pair(s6, silent));
            var corrected = Next(s7);
            Loop(corrected)["phase"] = "CORRECTING";
            Orch(corrected)["escalation"] = M(("state", "NONE"), ("reason", null), ("required_decision", null));
            Orch(corrected)["next_action"] = NextAction("PRINCIPAL_COORDINATOR", "CORRECT_AND_REREVIEW", Obj(C1, X, B1));
            Assert.Contains("I-P13", Pair(s7, corrected));
        }

        [Theory]
        [InlineData("review_rounds")]
        [InlineData("logical_requests")]
        [InlineData("architect_launches")]
        [InlineData("correction_rounds")]
        public void I62_C34_EveryCapStopsTheLoop(string counter)
        {
            var s12 = F8Step(12);
            var s13 = F8Step(13);
            var entry = (YamlMap)Y.L(s13.State, "orchestration.architect_budgets")[0]!;
            entry[counter] = Y.N(entry, "caps." + counter)!.Value + 1;
            Assert.Contains(Pair(s12, s13), id => id.StartsWith("I-S18", StringComparison.Ordinal));
        }

        [Fact]
        public void I62_C34_ACapLoweredByTheAuthorizationAndAPerRequestRerunCapAreEnforced()
        {
            // A per-lineage cap lowered to 1 by the authorization's Budget: the entry's caps take the minimum (D1-2) and a second correction exceeds it.
            var s11 = F8Step(11);
            WriteDecisions(s11, new[] { G0Entry, RlaEntry.Replace("Validity.Until:", "Budget.corrections_per_lineage: 1\nValidity.Until:", StringComparison.Ordinal),
                DesignationEntry });
            Assert.Contains("I-S18/D1-2", Ids(new StateV2Validator().ValidateFile(s11)));
            var e = (YamlMap)Y.L(s11.State, "orchestration.architect_budgets")[0]!;
            ((YamlMap)e["caps"]!)["corrections_per_lineage"] = 1L;
            Assert.DoesNotContain("I-S18/D1-2", Ids(new StateV2Validator().ValidateFile(s11)));
            e["corrections_by_lineage"] = L(M(("lineage_id", "LIN-1"), ("count", 2L)), M(("lineage_id", "LIN-2"), ("count", 1L)));
            Assert.Contains(Ids(new StateV2Validator().ValidateFile(s11)), id => id.StartsWith("I-S18", StringComparison.Ordinal));

            // A third transport rerun of the same request exceeds transport_reruns_per_request.
            var s13 = F8Step(13);
            var reruns = (YamlMap)Y.L((YamlMap)Y.L(s13.State, "orchestration.architect_budgets")[0]!, "transport_reruns")[1]!;
            reruns["count"] = 3L;
            Assert.Contains(Ids(new StateV2Validator().ValidateFile(s13)), id => id.StartsWith("I-S18", StringComparison.Ordinal));
        }

        private static StatePoint IngestedAs(string outcome, Action<StatePoint>? change = null)
        {
            var s7 = Next(F8Step(6));
            var a = AttemptOf(s7, L1, 1);
            a["state"] = "RESULT_INGESTED";
            a["outcome"] = outcome;
            a["ingested_at"] = Rv(s7);
            RequestOf(s7, L1)["state"] = "INGESTED";
            Loop(s7)["phase"] = outcome == "VALID" ? "CORRECTING" : "REVIEW_PENDING";
            Orch(s7)["next_action"] = outcome == "VALID"
                ? NextAction("PRINCIPAL_COORDINATOR", "CORRECT_AND_REREVIEW", Obj(C1, X, B1))
                : NextAction("PRINCIPAL_COORDINATOR", "RERUN_TRANSPORT", Obj(C1, X, B1));
            change?.Invoke(s7);
            return s7;
        }

        [Fact]
        public void I62_C35_AnInvalidOutputOfTheArchitectDoesNotAdvanceTheLoopNorItsLineage()
        {
            var s6 = F8Step(6);
            var res1 = (YamlMap)AttemptOf(s6, L1, 1)["result"]!;
            void Lineages(StatePoint n) => Orch(n)["findings"] = L(Lineage("LIN-1", "A62-X-01", res1), Lineage("LIN-2", "A62-X-02", res1));

            // VALID CHANGES REQUIRED opens its lineages and moves to CORRECTING (the F.8 step).
            Assert.Empty(Pair(s6, IngestedAs("VALID", Lineages)));

            // An INVALID output: no lineage opens, and the only phase is a transport rerun; CORRECTING is an advance it cannot give (P-19).
            Assert.Empty(Pair(s6, IngestedAs("INVALID")));
            Assert.Contains("I-P13/P-19", Pair(s6, IngestedAs("INVALID", Lineages)));
            Assert.Contains("I-P13/P-19", Pair(s6, IngestedAs("INVALID", n => Loop(n)["phase"] = "CORRECTING")));
            Assert.Contains("I-P13/P-19", Pair(s6, IngestedAs("INPUT_FIDELITY_INVALID", Lineages)));

            // A lineage that names a finding the result does not report, and a VALID result whose phase is not its verdict's.
            Assert.Contains("I-P13/P-19", Pair(s6, IngestedAs("VALID", n => Orch(n)["findings"] = L(Lineage("LIN-1", "A62-X-99", res1)))));
            Assert.Contains("I-P13/P-19", Pair(s6, IngestedAs("VALID", n => { Lineages(n); Loop(n)["phase"] = "ARCHITECT_SATISFIED"; })));
            Assert.Empty(new StateV2Validator(new[] { "P-19" }).ValidatePair(s6, IngestedAs("INVALID", Lineages)).Where(v => v.Clause == "P-19"));
        }

        [Fact]
        public void I62_C42_5_8_AnUnaccreditedFindingNeverOpensALineageAndEveryAttemptCarriesItsOwnFidelity()
        {
            // (5) DEGRADED_BOUNDED: the finding whose only premise is degraded is UNACCREDITED and cannot open its lineage (P-25); the other one can.
            var s6 = F8Step(6);
            var res1 = (YamlMap)AttemptOf(s6, L1, 1)["result"]!;
            StatePoint Bounded(params string[] lineages) => IngestedAs("VALID", n =>
            {
                var a = AttemptOf(n, L1, 1);
                a["fidelity_status"] = "DEGRADED_BOUNDED";
                a["unaccredited"] = L("A62-X-01");
                a["premise_independence"] = Tree(n).Put("docs/automation/evidence/I-99-agent/review/L1/1/premise-independence.json", "{\"Premises\": []}\n");
                Orch(n)["findings"] = new List<object?>(lineages.Select(l => (object?)Lineage(l, l == "LIN-1" ? "A62-X-01" : "A62-X-02", res1)));
            });
            Assert.Empty(Pair(s6, Bounded("LIN-2")));
            Assert.Contains("I-P13/P-25", Pair(s6, Bounded("LIN-1", "LIN-2")));

            // (8) after a change of provider the new attempt needs its own fidelity evidence: reusing the previous attempt's is caught (I-S18).
            var s13 = F8Step(13);
            AttemptOf(s13, L2, 1)["input_fidelity"] = Clone((YamlMap)AttemptOf(s13, L1, 1)["input_fidelity"]!);
            Assert.Contains("I-S18", Ids(new StateV2Validator().ValidateFile(s13)));
            Assert.Empty(Ids(new StateV2Validator().ValidateFile(F8Step(13))));
        }

        [Fact]
        public void I62_C36_ChangingProviderModelPrincipalOrLabelsContinuesTheCountersAndTheLineage()
        {
            var points = F8();
            var s7 = points[5];
            var s9 = points[6];
            var s13 = points[9];
            Assert.NotEqual(Y.Get(s7.State, "custody.principal.binding"), Y.Get(s9.State, "custody.principal.binding"));
            var before = (YamlMap)Y.L(s7.State, "orchestration.architect_budgets")[0]!;
            var after = (YamlMap)Y.L(s13.State, "orchestration.architect_budgets")[0]!;
            Assert.True(Y.N(after, "review_rounds") > Y.N(before, "review_rounds") && Y.N(after, "architect_launches") > Y.N(before, "architect_launches"));
            Assert.Equal(new[] { "LIN-1", "LIN-2" }, Y.L(s13.State, "orchestration.findings").Cast<YamlMap>().Select(f => Y.S(f, "lineage_id")));
            for (var i = 6; i < 10; i++)
            {
                Assert.Empty(Pair(points[i - 1], points[i]));
            }

            // A reset after the change of Principal is caught (I-P13).
            var reset = Copy(points[7]);
            ((YamlMap)Y.L(reset.State, "orchestration.architect_budgets")[0]!)["review_rounds"] = 0L;
            Assert.Contains("I-P13", Pair(s9, reset));
        }

        private static StatePoint Revoked(StatePoint p)
        {
            var n = Next(p);
            var entries = new List<string> { G0Entry, RlaEntry, "## Revocación de RLA-1\n\n```text\nI62-REVIEW-LOOP-REVOCATION: RLA-1\nClaim-Id: " + ClaimId + "\n```" };
            var decisions = WriteDecisions(n, entries);
            OrchestrationSamples.End((YamlMap)Loop(n)["action_validity"]!, "REVOKED", Rv(n), Clone(decisions));
            OrchestrationSamples.End((YamlMap)Y.L((YamlMap)Y.L(n.State, "orchestration.architect_budgets")[0]!, "authorizations")[0]!, "REVOKED", Rv(n), Clone(decisions));
            return n;
        }

        [Fact]
        public void I62_C40_ALaunchedAttemptCompletesAfterARevocationAndAnUnlaunchedOneIsCancelledNeverLaunched()
        {
            // (i) LAUNCHED before the revocation: its result is received and ingested under the historical accreditation of its binding.
            var r4 = Revoked(F8Step(4));
            Assert.Empty(Pair(F8Step(4), r4));
            var r6 = Next(r4);
            Received(r6, AttemptOf(r6, L1, 1), ArchitectResult(r6, "docs/automation/evidence/I-99-agent/review/L1/1/result.json", L1, Obj(C1, X, B1),
                "CHANGES REQUIRED", required: new[] { "A62-X-01" }, dispositions: new (string, string)[0]));
            Assert.Empty(Pair(r4, r6));

            // (f) BUDGET_RESERVED when the validity is revoked: the revocation point cancels it before launch (I-S18 forbids keeping it reserved),
            // and launching in that point is a new action under an ended validity (I-P13).
            var kept = Revoked(F8Step(2));
            Assert.Contains("I-S18", Pair(F8Step(2), kept));
            var launched = Revoked(F8Step(2));
            var a = AttemptOf(launched, L1, 1);
            a["state"] = "LAUNCHING";
            a["run_id"] = "R20261006T030303Z-0001";
            a["launching_utc"] = "2026-10-06T03:03:03Z";
            Loop(launched)["phase"] = "ARCHITECT_INVOKED";
            Assert.Contains("I-P13", Pair(F8Step(2), launched));
            var cancelled = Revoked(F8Step(2));
            AttemptOf(cancelled, L1, 1)["state"] = "CANCELLED_BEFORE_LAUNCH";
            RequestOf(cancelled, L1)["state"] = "CANCELLED";
            Assert.Empty(Pair(F8Step(2), cancelled));
        }

        [Fact]
        public void I62_C30_AResultWithTheOutputContractOfAnotherRoleIsNeverIngestedAsValid()
        {
            // (e) in the state: the ARCHITECT request ingests a reviewer-result/v1 (the correspondence of §20.7 is crossed): INVALID, never VALID (P-19).
            var s4 = F8Step(4);
            var received = Next(s4);
            Received(received, AttemptOf(received, L1, 1), ReviewerSamples.ReviewerResult(received, "NO_FINDINGS", Obj(C1, X, B1)));
            var valid = Next(received);
            var a = AttemptOf(valid, L1, 1);
            a["state"] = "RESULT_INGESTED";
            a["outcome"] = "VALID";
            a["ingested_at"] = Rv(valid);
            RequestOf(valid, L1)["state"] = "INGESTED";
            Loop(valid)["phase"] = "REVIEW_PENDING";
            Orch(valid)["next_action"] = NextAction("PRINCIPAL_COORDINATOR", "RERUN_TRANSPORT", Obj(C1, X, B1));
            Assert.Contains("I-P13", Pair(received, valid));
            var invalid = Copy(valid);
            AttemptOf(invalid, L1, 1)["outcome"] = "INVALID";
            Assert.Empty(Pair(received, invalid));
        }

        /// <summary>s3 (a1.1 LAUNCHING) with the action validity ended EXPIRED in the next point, the attempt moved as <paramref name="destination"/> says.</summary>
        private static StatePoint ExpiredWhileLaunching(Action<StatePoint, YamlMap> destination)
        {
            var n = Next(F8Step(3));
            OrchestrationSamples.End((YamlMap)Loop(n)["action_validity"]!, "EXPIRED", Rv(n), null);
            OrchestrationSamples.End((YamlMap)Y.L((YamlMap)Y.L(n.State, "orchestration.architect_budgets")[0]!, "authorizations")[0]!, "EXPIRED", Rv(n), null);
            destination(n, AttemptOf(n, L1, 1));
            return n;
        }

        private static StatePoint NotStartedEvidence(StatePoint n, YamlMap a)
        {
            a["not_started_evidence"] = Tree(n).Put("docs/automation/evidence/I-99-agent/review/L1/1/not-started.json", "{\"Schema\": \"rackcad-relay-record/v2\", \"Started\": false}\n");
            return n;
        }

        [Fact]
        public void I62_C40_TheEndOfTheValidityKeepsTheHistoricalAccreditationAndSendsEachLaunchingAttemptToItsDestination()
        {
            // (e) The authorization ends after AGREED: the result and ARCHITECT_SATISFIED stay accredited, and the ended validity is never rewritten.
            var s17 = F8Step(17);
            Assert.Empty(Pair(s17, Next(s17)));
            var rewritten = Next(s17);
            ((YamlMap)Loop(rewritten)["action_validity"]!)["ended_reason"] = "EXPIRED";
            Assert.Contains("I-P13", Pair(s17, rewritten));

            // (g) A successor that reads only the custodied state of the closed loop finds the old review accredited (C-29 reconstructs it in a clean clone).
            var closed = Next(s17);
            var e = (YamlMap)Y.L(closed.State, "orchestration.architect_budgets")[0]!;
            e["closed_at"] = Rv(closed);
            var loop = Loop(closed);
            foreach (var k in new[] { "object", "authorization", "action_validity", "instance_id" })
            {
                loop[k] = null;
            }

            loop["type"] = "NONE";
            loop["phase"] = "NONE";
            Orch(closed)["next_action"] = NextAction("PRINCIPAL_COORDINATOR", "NEXT_GATE", null);
            Assert.Empty(Pair(s17, closed));
            Assert.All(Y.L(closed.State, "orchestration.findings").Cast<YamlMap>(), f => Assert.Equal("CLOSED", Y.S(f, "state")));

            // (j) LAUNCHING at the end, started before it: the attempt completes and its result is ingested with the existing rules.
            var started = ExpiredWhileLaunching((n, a) =>
            {
                a["state"] = "LAUNCHED";
                a["launch_evidence"] = Tree(n).Put("docs/automation/evidence/I-99-agent/review/L1/1/launch.json", "{\"Schema\": \"rackcad-relay-record/v2\"}\n");
            });
            Assert.Empty(Pair(F8Step(3), started));
            var received = Next(started);
            var res = ArchitectResult(received, "docs/automation/evidence/I-99-agent/review/L1/1/result.json", L1, Obj(C1, X, B1), "CHANGES REQUIRED",
                required: new[] { "A62-X-01" }, dispositions: new (string, string)[0]);
            Received(received, AttemptOf(received, L1, 1), res);
            Assert.Empty(Pair(started, received));
            var ingested = Next(received);
            var ia = AttemptOf(ingested, L1, 1);
            ia["state"] = "RESULT_INGESTED";
            ia["outcome"] = "VALID";
            ia["ingested_at"] = Rv(ingested);
            RequestOf(ingested, L1)["state"] = "INGESTED";
            Orch(ingested)["findings"] = L(Lineage("LIN-1", "A62-X-01", res));
            Loop(ingested)["phase"] = "CORRECTING";
            Orch(ingested)["next_action"] = NextAction("PRINCIPAL_COORDINATOR", "CORRECT_AND_REREVIEW", Obj(C1, X, B1));
            Assert.Empty(Pair(received, ingested));

            // (k) Never started, with the invoker's proof: CANCELLED_BEFORE_LAUNCH, the reservation stays counted; without the proof it is rejected.
            YamlMap Cancel(StatePoint n, YamlMap a)
            {
                a["state"] = "CANCELLED_BEFORE_LAUNCH";
                RequestOf(n, L1)["state"] = "CANCELLED";
                Loop(n)["phase"] = "REVIEW_PENDING";
                return a;
            }

            var notStarted = ExpiredWhileLaunching((n, a) => NotStartedEvidence(n, Cancel(n, a)));
            Assert.Empty(Pair(F8Step(3), notStarted));
            Assert.Equal(1L, Y.N((YamlMap)Y.L(notStarted.State, "orchestration.architect_budgets")[0]!, "architect_launches"));
            var unproven = ExpiredWhileLaunching((n, a) => Cancel(n, a));
            Assert.Contains("I-S18", Pair(F8Step(3), unproven));
            Assert.Contains("I-P13", Pair(F8Step(3), unproven));

            // (l) Indeterminate: LAUNCH_UNCERTAIN (counted conservatively), never retried silently under the ended validity.
            var uncertain = ExpiredWhileLaunching((n, a) =>
            {
                a["state"] = "LAUNCH_UNCERTAIN";
                a["outcome"] = "UNCERTAIN";
                Loop(n)["phase"] = "REVIEW_PENDING";
            });
            Assert.Empty(Pair(F8Step(3), uncertain));
            var retried = Next(uncertain);
            var ra = AttemptOf(retried, L1, 1);
            ra["state"] = "LAUNCHING";
            ra["outcome"] = null;
            Loop(retried)["phase"] = "ARCHITECT_INVOKED";
            Assert.Contains("I-P13/D2-6", Pair(uncertain, retried));
            var second = Next(uncertain);
            Y.L(RequestOf(second, L1), "attempts").Add(Attempt(second, L1, 2, Obj(C1, X, B1), (YamlMap)AttemptOf(second, L1, 1)["binding"]!, Rv(second), new string[0], (1, 1, 2, 1, 0)));
            Assert.Contains("I-P13", Pair(uncertain, second));

            // (m) The attempt cancelled as not started later yields a result: it is never ingested (S-04).
            var late = Next(notStarted);
            var la = AttemptOf(late, L1, 1);
            Received(late, la, ArchitectResult(late, "docs/automation/evidence/I-99-agent/review/L1/1/result.json", L1, Obj(C1, X, B1), "AGREED",
                required: new string[0], dispositions: new (string, string)[0]));
            Assert.Contains("I-P13", Pair(notStarted, late));
            la["state"] = "CANCELLED_BEFORE_LAUNCH";
            Assert.Contains("I-S18", Pair(notStarted, late));

            // (n) Started after the end (the invoker's classification UNAUTHORIZED_LAUNCH): the result never opens a lineage, never satisfies and never advances.
            var unauthorized = Next(received);
            var ua = AttemptOf(unauthorized, L1, 1);
            ua["state"] = "RESULT_INGESTED";
            ua["outcome"] = "UNAUTHORIZED_LAUNCH";
            ua["ingested_at"] = Rv(unauthorized);
            RequestOf(unauthorized, L1)["state"] = "INGESTED";
            Loop(unauthorized)["phase"] = "REVIEW_PENDING";
            Orch(unauthorized)["escalation"] = M(("state", "COORDINATOR"), ("reason", "P-20: arranque posterior al fin de la vigencia"), ("required_decision", "nueva autorización"));
            Orch(unauthorized)["next_action"] = NextAction("COORDINATOR", "DECIDE", null);
            Assert.Empty(Pair(received, unauthorized));
            var opened = Copy(unauthorized);
            Orch(opened)["findings"] = L(Lineage("LIN-1", "A62-X-01", res));
            Assert.Contains("I-P13/P-19", Pair(received, opened));
            var advanced = Copy(unauthorized);
            Loop(advanced)["phase"] = "CORRECTING";
            Orch(advanced)["escalation"] = M(("state", "NONE"), ("reason", null), ("required_decision", null));
            Orch(advanced)["next_action"] = NextAction("PRINCIPAL_COORDINATOR", "CORRECT_AND_REREVIEW", Obj(C1, X, B1));
            Assert.Contains("I-P13/P-19", Pair(received, advanced));
        }
    }
}
