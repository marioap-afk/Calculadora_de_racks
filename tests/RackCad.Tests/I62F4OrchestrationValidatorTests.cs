#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Xunit;
using static RackCad.Tests.OrchestrationSamples;
using static RackCad.Tests.StateV2Samples;

namespace RackCad.Tests
{
    /// <summary>
    /// I-62 F4 (slice F4-D), obligation C-38 of the frozen Proposal V14 (Anexo C): the validator of <c>orchestration</c> (I-S18, I-P13) with the AGREED
    /// amendment A-1. Positive: the complete sequence F.8, GREEN at every point and every pair, plus the A-1 lifecycle positives (LOOP_CLOSED from
    /// ARCHITECT_SATISFIED, after EXPIRED and with the revocation in the closing decision; a new loop with a new authorization; substitution and
    /// continuation inside the loop; a REVIEWER loop authorized by its gate contract up to its closure). Negatives: the list of C-38 and the additions
    /// of A-1 §5 (C-38 rows of the first, second, third and fourth reviews). Each negative fails by the invariant (and A-1 clause) that guards it;
    /// disabling that rule in memory lets it through (mutation check).
    /// </summary>
    public class I62F4OrchestrationValidatorTests
    {
        private static readonly StateV2Validator Validator = new StateV2Validator();

        [Fact]
        public void I62_C38_TheCompleteSequenceF8IsGreenAtEveryPointAndEveryPair()
        {
            var f8 = F8();
            Assert.Equal(13, f8.Count);
            Assert.Empty(Problems(f8));
            Assert.Equal("ARCHITECT_SATISFIED", Y.S(f8[^1].State, "orchestration.loop.phase"));
            Assert.Equal(C2, Y.S(f8[^1].State, "orchestration.loop.object.commit"));
        }

        [Fact]
        public void I62_C38_ALoopClosesAfterArchitectSatisfiedAndANewLoopOpensWithANewAuthority()
        {
            var closed = CloseFromSatisfied(F8Step(17));
            var reopened = OpenSecondLoop(closed, "RLA-2");
            Assert.Empty(Problems(new List<StatePoint> { F8Step(17), closed, reopened }));
            Assert.Equal(2, Y.L(reopened.State, "orchestration.architect_budgets").Count);
            Assert.Equal("ARL-" + Rv(reopened), Y.S(reopened.State, "orchestration.loop.instance_id"));
        }

        [Fact]
        public void I62_C38_ALoopClosesAfterExpiredWithItsDecisionAndWithTheRevocationInTheClosingDecision()
        {
            var expired = Expire(F8Step(12));
            var closeExpired = CloseWithDecision(expired, revoke: false);
            Assert.Empty(Problems(new List<StatePoint> { F8Step(12), expired, closeExpired }));

            var closeRevoking = CloseWithDecision(F8Step(12), revoke: true);
            Assert.Empty(Problems(new List<StatePoint> { F8Step(12), closeRevoking }));
        }

        [Fact]
        public void I62_C38_SubstitutionAndContinuationStayInTheSameEntryWithoutReset()
        {
            var substituted = Substitute(F8Step(12), "RLA-2");
            Assert.Empty(Problems(new List<StatePoint> { F8Step(12), substituted }));

            var expired = Expire(F8Step(12));
            var continued = Substitute(expired, "RLA-2");
            Assert.Empty(Problems(new List<StatePoint> { F8Step(12), expired, continued }));
            var auths = Y.L((YamlMap)Y.L(continued.State, "orchestration.architect_budgets")[0]!, "authorizations").Cast<YamlMap>().ToList();
            Assert.Equal(new[] { "EXPIRED", null }, auths.Select(a => Y.S(a, "ended_reason")).ToArray());
        }

        [Fact]
        public void I62_C38_AReviewerLoopAuthorizedByItsGateContractRunsToItsClosure()
        {
            var loop = ReviewerSamples.Loop();
            Assert.Empty(Problems(loop));
            var closed = loop[^1];
            Assert.Single(Y.L(closed.State, "orchestration.reviewer_closures"));
            Assert.Empty(Y.L(closed.State, "orchestration.architect_budgets"));
            Assert.True(Orchestration.ReviewerRequirementSatisfied(closed.State, ReviewerSamples.AuthorityOf(
                (YamlMap)Y.Get((YamlMap)Y.L(closed.State, "orchestration.reviewer_closures")[0]!, "authorization")!)));
        }

        // (case, builder of the negative point(s), required invariant/clause, allowed extra invariants)
        public static IEnumerable<object[]> Negatives()
        {
            // ---- the list of C-38 (V14)
            yield return Case("closure without a result with authority", () => Mutate(16, 17, n =>
            {
                var response = (YamlMap)Y.Get((YamlMap)Y.L(n.State, "orchestration.findings")[0]!, "response")!;
                ((YamlMap)Y.L(n.State, "orchestration.findings")[0]!)["closed_by"] = Clone(response);
            }), "I-S18", "I-P13");
            yield return Case("closure by a reviewer-result/v1", () => Mutate(16, 17, n =>
                ((YamlMap)Y.L(n.State, "orchestration.findings")[0]!)["closed_by"] = ReviewerSamples.ReviewerResult(n, "NO_FINDINGS", Obj(C2, X, B2))),
                "I-S18", "I-P13");
            yield return Case("downgrade by a non-issuer", () => Mutate(6, 7, n =>
            {
                var f = (YamlMap)Y.L(n.State, "orchestration.findings")[0]!;
                f["severity"] = "OPTIONAL";
                f["downgraded_by"] = Clone((YamlMap)Y.Get(n.State, "protocol.g0_acceptance.decision")!);
            }), "I-S18");
            yield return Case("omission treated as closure", () => Mutate(16, 17, null, s16 =>
                ArchitectResult(s16, "docs/automation/evidence/I-99-agent/review/L2/1/result.json", L2, Obj(C2, X, B2), "AGREED", new string[0],
                    new[] { ("A62-X-01", "CLOSED") })), "I-P13");
            yield return Case("ARCHITECT_SATISFIED with an omitted open lineage", () => Mutate(16, 17, n =>
            {
                var f = (YamlMap)Y.L(n.State, "orchestration.findings")[1]!;
                f["state"] = "OPEN";
                f["closed_by"] = null;
                f["last_disposition_in"] = null;
            }, s16 => ArchitectResult(s16, "docs/automation/evidence/I-99-agent/review/L2/1/result.json", L2, Obj(C2, X, B2), "AGREED", new string[0],
                new[] { ("A62-X-01", "CLOSED") })), "I-S18", "I-P13");
            yield return Case("closure by a result of another version", () => Mutate(16, 17, null, s16 =>
                ArchitectResult(s16, "docs/automation/evidence/I-99-agent/review/L2/1/result.json", L2, Obj(C1, X, B1), "AGREED", new string[0],
                    new[] { ("A62-X-01", "CLOSED"), ("A62-X-02", "CLOSED") })), "I-S18", "I-P13");
            yield return Case("ARCHITECT_SATISFIED without AGREED on the blob", () => Mutate(16, 17, n =>
            {
                foreach (var f in Y.L(n.State, "orchestration.findings").Cast<YamlMap>())
                {
                    f["state"] = "OPEN";
                    f["closed_by"] = null;
                    f["last_disposition_in"] = null;
                }
            }, s16 => ArchitectResult(s16, "docs/automation/evidence/I-99-agent/review/L2/1/result.json", L2, Obj(C2, X, B2), "CHANGES REQUIRED",
                new[] { "A62-X-03" }, new (string, string)[0])), "I-S18", "I-P13");
            yield return Case("inconsistent escalation", () => Mutate(6, 7, n => Orch(n)["next_action"] = NextAction("OWNER", "DECIDE", null)), "I-S18");
            yield return Case("cap exceeded", () => Mutate(12, 13, n =>
            {
                var rla = RlaEntry.Replace("Validity.Until", "Budget.review_rounds: 1\nValidity.Until", StringComparison.Ordinal);
                WriteDecisions(n, new[] { G0Entry, rla, DesignationEntry });
                ((YamlMap)Y.Get((YamlMap)Y.L(n.State, "orchestration.architect_budgets")[0]!, "caps")!)["review_rounds"] = 1L;
            }), "I-S18");
            yield return Case("counter reset", () => Mutate(13, 14, n => ((YamlMap)Y.L(n.State, "orchestration.architect_budgets")[0]!)["architect_launches"] = 1L),
                "I-S18", "I-P13");
            yield return Case("forbidden attempt edge", () => Mutate(3, 4, n => AttemptOf(n, L1, 1)["state"] = "INVOCATION_PLANNED"), "I-P13", "I-S18");
            yield return Case("late change of loop.object (REREVIEW_PENDING → ARCHITECT_INVOKED)", () => Mutate(13, 14, n => Loop(n)["object"] = Obj(C1, X, B1)),
                "I-P13");
            yield return Case("materialized binding with a criterion not SATISFIED", () => Mutate(0, 2, n =>
            {
                var a = AttemptOf(n, L1, 1);
                var json = StateTreeReader.Json(n.Tree, a["binding"])!;
                json["Acceptance"]!["MaterializationCheck"]!["Criteria"]![0]!["Result"] = "NOT_SATISFIED";
                a["binding"] = Tree(n).PutJson(Y.S((YamlMap)a["binding"]!, "path")!, json);
            }), "I-S18");
            yield return Case("materialized binding without AuthorizationRef", () => Mutate(0, 2, n =>
            {
                var a = AttemptOf(n, L1, 1);
                var json = StateTreeReader.Json(n.Tree, a["binding"])!;
                json["Acceptance"]!["AuthorizationRef"] = null;
                a["binding"] = Tree(n).PutJson(Y.S((YamlMap)a["binding"]!, "path")!, json);
            }), "I-S18");
            yield return Case("double ingestion with different blobs", () =>
            {
                var p = F8Step(17);
                var n = Next(p);
                AttemptOf(n, L2, 1)["result"] = Tree(n).Put("docs/automation/evidence/I-99-agent/review/L2/1/result-bis.json", "{\"Schema\": \"x\"}\n");
                return (p, n);
            }, "I-P13", "I-S18");
            yield return Case("ingestion of a CANCELLED_BEFORE_LAUNCH attempt", () => Mutate(12, 13, n =>
            {
                var a = AttemptOf(n, L2, 1);
                a["state"] = "CANCELLED_BEFORE_LAUNCH";
                a["result"] = Clone((YamlMap)AttemptOf(n, L1, 1)["result"]!);
                RequestOf(n, L2)["state"] = "CANCELLED";
            }), "I-S18", "I-P13");

            // ---- A-1 §5, C-38 (first review: per-loop identity and budgets)
            yield return Case("loop.object → null outside LOOP_CLOSED", () => Mutate(11, 12, n => Loop(n)["object"] = null), "I-P13", "I-S18");
            yield return Case("LOOP_CLOSED with an OPEN request and the validity OPEN without a decision", () =>
            {
                var p = F8Step(13);
                return (p, Blank(Next(p), closedBy: null));
            }, "I-P13", "I-S18");
            yield return Case("LOOP_CLOSED after EXPIRED without the decision", () =>
            {
                var p = Expire(F8Step(12));
                return (p, Blank(Next(p), closedBy: null));
            }, "I-P13");
            yield return Case("EXPIRED rewritten as SUPERSEDED in a continuation", () =>
            {
                var p = Expire(F8Step(12));
                var n = Substitute(p, "RLA-2");
                var e = (YamlMap)Y.L(n.State, "orchestration.architect_budgets")[0]!;
                ((YamlMap)Y.L(e, "authorizations")[0]!)["ended_reason"] = "SUPERSEDED";
                return (p, n);
            }, "I-P13");
            yield return Case("continuation that creates an entry", () =>
            {
                var p = F8Step(12);
                var n = Substitute(p, "RLA-2");
                var entry = Clone((YamlMap)Y.L(n.State, "orchestration.architect_budgets")[0]!);
                entry["loop_instance_id"] = "ARL-99";
                entry["closed_at"] = Rv(n);
                Y.L(n.State, "orchestration.architect_budgets").Add(entry);
                return (p, n);
            }, "I-P13", "I-S18");
            yield return Case("new action with the validity ended", () =>
            {
                var p = F8Step(17);
                var n = Next(p);
                Y.L(RequestOf(n, L2), "attempts").Add(Attempt(n, L2, 2, Obj(C2, X, B2), (YamlMap)AttemptOf(n, L2, 1)["binding"]!, Rv(n), new string[0], (2, 2, 3, 1, 1)));
                RequestOf(n, L2)["state"] = "OPEN";
                return (p, n);
            }, "I-P13", "I-S18");
            yield return Case("new loop with an authorization already used", () => OpenSecondLoopPair("RLA-1"), "I-P13", "I-S18");
            yield return Case("instance_id other than ARL-<record_version>", () =>
            {
                var (p, n) = OpenSecondLoopPair("RLA-2");
                Loop(n)["instance_id"] = "ARL-1";
                ((YamlMap)Y.L(n.State, "orchestration.architect_budgets")[1]!)["loop_instance_id"] = "ARL-1";
                RequestOf(n, "L20261008T010101Z-0003")["loop_instance_id"] = "ARL-1";
                return (p, n);
            }, "I-P13", "I-S18");
            yield return Case("request that changes loop", () => Mutate(12, 13, n => RequestOf(n, L1)["loop_instance_id"] = "ARL-99"), "I-P13", "I-S18");
            yield return Case("budgets counting an Architect request", () => Mutate(12, 13, n =>
            {
                var b = Y.M(n.State, "orchestration.budgets")!;
                b["architect_launches"] = 1L;
            }), "I-S18", "I-P13");
            yield return Case("closed entry that changes", () =>
            {
                var closed = CloseFromSatisfied(F8Step(17));
                var n = OpenSecondLoop(closed, "RLA-2");
                ((YamlMap)Y.L(n.State, "orchestration.architect_budgets")[0]!)["review_rounds"] = 3L;
                return (closed, n);
            }, "I-P13", "I-S18");
            yield return Case("ended authorization record over a rewritten (not appended) decisions file", () =>
            {
                var closed = CloseFromSatisfied(F8Step(17));
                var n = OpenSecondLoop(closed, "RLA-2");
                var rewritten = RlaEntry.Replace("Validity.Until: 2026-12-31T00:00:00Z", "Validity.Until: 2027-12-31T00:00:00Z", StringComparison.Ordinal);
                WriteDecisions(n, new[] { G0Entry, rewritten, DesignationEntry,
                    "```text\nI62-REVIEW-LOOP-AUTHORIZATION: RLA-2\nRole: ARCHITECT\nContinuesLoopInstanceId: null\nClaim-Id: " + ClaimId + "\n```" });
                return (closed, n);
            }, "I-P13");
            yield return Case("invocation of a launched attempt rewritten in place", () => Mutate(3, 4, n =>
            {
                var a = AttemptOf(n, L1, 1);
                a["invocation"] = Tree(n).PutJson("docs/automation/evidence/I-99-agent/review/L1/1/invocation-bis.json",
                    InvocationJson("I20261006T999999Z-0001", L1, 1, Obj(C1, X, B1), new string[0], (1, 1, 1, 0, 0)));
            }), "I-P13", "I-S18");
            yield return Case("replanning without a new InvocationId", () =>
            {
                var p = F8Step(2);
                var n = Next(p);
                var a = AttemptOf(n, L1, 1);
                var json = StateTreeReader.Json(n.Tree, a["invocation"])!;
                json["Gate"] = "F5";
                a["invocation"] = Tree(n).PutJson("docs/automation/evidence/I-99-agent/review/L1/1/invocation-replanned.json", json);
                return (p, n);
            }, "I-P13");
            yield return Case("BudgetSnapshot that is not the loop's entry", () => Mutate(12, 13, n =>
            {
                var a = AttemptOf(n, L2, 1);
                a["invocation"] = Tree(n).PutJson("docs/automation/evidence/I-99-agent/review/L2/1/invocation.json",
                    InvocationJson("I20261006T000014Z-0001", L2, 1, Obj(C2, X, B2), new[] { "LIN-1", "LIN-2" }, (1, 1, 1, 0, 0)));
            }), "I-P13");
            yield return Case("OpenFindings without an open lineage", () => Mutate(12, 13, n =>
            {
                var a = AttemptOf(n, L2, 1);
                a["invocation"] = Tree(n).PutJson("docs/automation/evidence/I-99-agent/review/L2/1/invocation.json",
                    InvocationJson("I20261006T000014Z-0001", L2, 1, Obj(C2, X, B2), new[] { "LIN-1" }, (2, 2, 2, 0, 1)));
            }), "I-P13");

            // ---- A-1 §5, C-38 (REVIEWER: OBS-A1-01 and the second, third and fourth reviews)
            yield return Case("REVIEWER_SATISFIED with a BLOCKING open", () => ReviewerMutate(3, 4, n =>
                Orch(n)["findings"] = L(M(("lineage_id", "LIN-R1"), ("finding_ids", L("R-01")), ("issuer", "REVIEWER"),
                    ("opened_in", Clone((YamlMap)AttemptOf(n, ReviewerSamples.R1, 1)["result"]!)), ("severity", "BLOCKING"), ("class", "defecto"),
                    ("affected_section", "§1"), ("state", "OPEN"), ("response", null), ("last_disposition_in", null), ("omitted_in", L()), ("closed_by", null),
                    ("downgraded_by", null)))), "I-S18", "I-P13");
            yield return Case("REVIEWER with a loop identity of the Architect", () => ReviewerMutate(0, 1, n => Loop(n)["instance_id"] = "ARL-6"), "I-S18", "I-P13");
            yield return Case("REVIEWER with ARCHITECT_SATISFIED", () => ReviewerMutate(3, 4, n =>
            {
                Loop(n)["phase"] = "ARCHITECT_SATISFIED";
                ((YamlMap)Loop(n)["action_validity"]!)["ended_reason"] = "ARCHITECT_SATISFIED";
            }), "I-S18", "I-P13");
            yield return Case("REVIEWER closure (E) after EXPIRED without the decision", () =>
            {
                var expired = ReviewerExpire(ReviewerSamples.Loop()[1]);
                return (expired, ReviewerClose(expired, decision: false));
            }, "I-P13");
            yield return Case("reopening with an ended REVIEWER authority", () =>
            {
                var closed = ReviewerSamples.Loop()[^1];
                return (closed, ReviewerReopen(closed, newContract: false));
            }, "I-P13");
            yield return Case("reopening with a SUPERSEDED REVIEWER authority (A62-A1T-O3)", () =>
            {
                var closed = ReviewerSamples.Loop()[^1];
                var n = ReviewerReopen(closed, newContract: true);
                var authority = Y.S(n.State, "orchestration.loop.action_validity.authorization_id");
                WriteDecisions(n, new[] { G0Entry, "```text\nI62-REVIEWER-AUTHORITY-SUPERSEDED: " + authority + "\n```" });
                return (closed, n);
            }, "I-P13");
            yield return Case("REVIEWER closure that drops reviewer_closures history", () =>
            {
                var loop = ReviewerSamples.Loop();
                var n = Next(loop[^1]);
                Y.L(n.State, "orchestration.reviewer_closures").Clear();
                return (loop[^1], n);
            }, "I-P13");
        }

        [Theory]
        [MemberData(nameof(Negatives))]
        public void I62_C38_EachNegativeFailsByTheInvariantThatGuardsIt(string name, Func<(StatePoint P, StatePoint N)> build, string[] required, string[] allowed)
        {
            var (p, n) = build();
            var reported = Validator.ValidateFile(n).Concat(Validator.ValidatePair(p, n)).Select(x => x.Invariant).Distinct().ToArray();

            Assert.True(required.All(reported.Contains), name + ": expected " + string.Join(", ", required) + ", got " + string.Join(", ", reported)
                                                         + "\n" + string.Join("\n", Validator.ValidateFile(n).Concat(Validator.ValidatePair(p, n))));
            Assert.True(reported.All(r => required.Contains(r) || allowed.Contains(r)), name + ": unexpected " + string.Join(", ", reported)
                                                                                         + "\n" + string.Join("\n", Validator.ValidateFile(n).Concat(Validator.ValidatePair(p, n))));
        }

        [Theory]
        [InlineData("I-S18", "inconsistent escalation")]
        [InlineData("I-S18", "materialized binding with a criterion not SATISFIED")]
        [InlineData("I-P13", "late change of loop.object (REREVIEW_PENDING → ARCHITECT_INVOKED)")]
        [InlineData("I-P13", "omission treated as closure")]
        [InlineData("I-P13", "new loop with an authorization already used")]
        [InlineData("I-P13", "reopening with a SUPERSEDED REVIEWER authority (A62-A1T-O3)")]
        [InlineData("I-S18", "REVIEWER_SATISFIED with a BLOCKING open")]
        public void I62_C38_DisablingTheGuardingInvariantInMemoryLetsTheNegativeThrough(string rule, string caseName)
        {
            var c = Negatives().Single(x => (string)x[0] == caseName);
            var (p, n) = ((Func<(StatePoint, StatePoint)>)c[1])();
            var all = Validator.ValidateFile(n).Concat(Validator.ValidatePair(p, n)).ToList();
            var mutated = new StateV2Validator(new[] { rule });
            var after = mutated.ValidateFile(n).Concat(mutated.ValidatePair(p, n)).ToList();

            Assert.Contains(all, x => x.Invariant == rule);
            Assert.DoesNotContain(after, x => x.Invariant == rule);
        }

        // ------------------------------------------------------------------ scenario builders

        internal static List<string> Problems(List<StatePoint> points)
        {
            var problems = new List<string>();
            for (var i = 0; i < points.Count; i++)
            {
                problems.AddRange(Validator.ValidateFile(points[i]).Select(x => "point rv " + Rv(points[i]) + ": " + x));
                if (i > 0)
                {
                    problems.AddRange(Validator.ValidatePair(points[i - 1], points[i]).Select(x => "pair rv " + Rv(points[i - 1]) + " → " + Rv(points[i]) + ": " + x));
                }
            }

            return problems;
        }

        private static readonly int[] Steps = { 0, 2, 3, 4, 6, 7, 9, 11, 12, 13, 14, 16, 17 };

        /// <summary>Takes the F.8 pair (pStep → nStep), lets <paramref name="early"/> rewrite an artifact of the step before n, and mutates n.</summary>
        internal static (StatePoint P, StatePoint N) Mutate(int pStep, int nStep, Action<StatePoint>? mutation, Func<StatePoint, YamlMap>? early = null)
        {
            var f8 = F8();
            var p = f8[Array.IndexOf(Steps, pStep)];
            var n = f8[Array.IndexOf(Steps, nStep)];
            if (early != null)
            {
                // Rewrites the result received at the step before n (16) in both points, so the pair sees the custodied (wrong) result.
                var prev = f8[Array.IndexOf(Steps, nStep) - 1];
                var r = early(prev);
                Tree(n).Put(Y.S(r, "path")!, System.Text.Encoding.UTF8.GetString(Bytes(prev, Y.S(r, "path")!)));
                foreach (var point in new[] { prev, n })
                {
                    foreach (var (_, stateRef) in Y.StateRefs(point.State, "$").ToList())
                    {
                        if (Y.S(stateRef, "path") == Y.S(r, "path"))
                        {
                            stateRef["blob"] = r["blob"];
                        }
                    }
                }

                if (p == prev)
                {
                    p = prev;
                }
            }

            mutation?.Invoke(n);
            return (p, n);
        }

        internal static (StatePoint P, StatePoint N) ReviewerMutate(int pIndex, int nIndex, Action<StatePoint> mutation)
        {
            var loop = ReviewerSamples.Loop();
            var n = loop[nIndex];
            mutation(n);
            return (loop[pIndex], n);
        }

        private static byte[] Bytes(StatePoint p, string path) => p.Tree.TryRead(path, out var b) ? b : Array.Empty<byte>();

        private static object[] Case(string name, Func<(StatePoint, StatePoint)> build, string required, params string[] allowed) =>
            new object[] { name, build, new[] { required }, allowed };

        internal static StatePoint CloseFromSatisfied(StatePoint satisfied)
        {
            var n = Next(satisfied);
            var e = (YamlMap)Y.L(n.State, "orchestration.architect_budgets")[0]!;
            e["closed_at"] = Rv(n);
            e["closed_by"] = null;
            return Blank(n, closedBy: null, touchEntry: false);
        }

        internal static StatePoint Blank(StatePoint n, YamlMap? closedBy, bool touchEntry = true)
        {
            var loop = Loop(n);
            if (touchEntry)
            {
                var e = Y.L(n.State, "orchestration.architect_budgets").Cast<YamlMap>().First(x => Y.S(x, "loop_instance_id") == Y.S(loop, "instance_id"));
                e["closed_at"] = Rv(n);
                e["closed_by"] = closedBy == null ? null : Clone(closedBy);
            }

            loop["type"] = "NONE";
            loop["phase"] = "NONE";
            loop["instance_id"] = null;
            loop["object"] = null;
            loop["authorization"] = null;
            loop["action_validity"] = null;
            Orch(n)["next_action"] = NextAction("PRINCIPAL_COORDINATOR", "NEXT_GATE", null);
            return n;
        }

        internal static StatePoint Expire(StatePoint p)
        {
            var n = Next(p);
            var at = Rv(n);
            End((YamlMap)Loop(n)["action_validity"]!, "EXPIRED", at, null);
            End((YamlMap)Y.L((YamlMap)Y.L(n.State, "orchestration.architect_budgets")[0]!, "authorizations")[^1]!, "EXPIRED", at, null);
            return n;
        }

        internal static StatePoint CloseWithDecision(StatePoint p, bool revoke)
        {
            var n = Next(p);
            var block = "```text\nI62-REVIEW-LOOP-CLOSE: ARL-6\n" + (revoke ? "I62-REVIEW-LOOP-REVOCATION: RLA-1\n" : string.Empty) + "Claim-Id: " + ClaimId + "\n```";
            var decisions = WriteDecisions(n, new[] { G0Entry, RlaEntry, DesignationEntry, block });
            var e = (YamlMap)Y.L(n.State, "orchestration.architect_budgets")[0]!;
            if (revoke)
            {
                End((YamlMap)Y.L(e, "authorizations")[^1]!, "REVOKED", Rv(n), decisions);
            }

            return Blank(n, closedBy: decisions);
        }

        internal static StatePoint Substitute(StatePoint p, string newId)
        {
            var n = Next(p);
            var rla2 = "```text\nI62-REVIEW-LOOP-AUTHORIZATION: " + newId + "\nRole: ARCHITECT\nContinuesLoopInstanceId: ARL-6\nClaim-Id: " + ClaimId + "\n```";
            var decisions = WriteDecisions(n, new[] { G0Entry, RlaEntry, DesignationEntry, rla2 });
            var e = (YamlMap)Y.L(n.State, "orchestration.architect_budgets")[0]!;
            var auths = Y.L(e, "authorizations");
            var last = (YamlMap)auths[^1]!;
            if (Y.S(last, "state") == "OPEN")
            {
                End(last, "SUPERSEDED", Rv(n), decisions);
            }

            auths.Add(M(("authorization_id", newId), ("authorization", Clone(decisions)), ("state", "OPEN"), ("ended_at", null), ("ended_utc", null),
                ("ended_reason", null), ("ended_by", null)));
            Loop(n)["authorization"] = Clone(decisions);
            Loop(n)["action_validity"] = Validity(newId);
            return n;
        }

        internal static StatePoint OpenSecondLoop(StatePoint closed, string authorizationId)
        {
            var n = Next(closed);
            var rla = "```text\nI62-REVIEW-LOOP-AUTHORIZATION: " + authorizationId + "\nRole: ARCHITECT\nContinuesLoopInstanceId: null\nClaim-Id: " + ClaimId + "\n```";
            var entries = authorizationId == "RLA-1" ? new[] { G0Entry, RlaEntry, DesignationEntry } : new[] { G0Entry, RlaEntry, DesignationEntry, rla };
            var decisions = WriteDecisions(n, entries);
            var iid = "ARL-" + Rv(n);
            var obj = Obj(C2, X, B2);
            var request = "L20261008T010101Z-0003";
            var binding = MaterializedBinding(n, "B20261008T010101Z-e001", decisions);
            var loop = Loop(n);
            loop["type"] = "ARCHITECT_REVIEW";
            loop["phase"] = "REVIEW_PENDING";
            loop["instance_id"] = iid;
            loop["object"] = Clone(obj);
            loop["authorization"] = Clone(decisions);
            loop["action_validity"] = Validity(authorizationId);
            var attempt = Attempt(n, request, 1, obj, binding, Rv(n), new string[0], (1, 1, 1, 0, 0));
            var req = Request(request, obj, 3, attempt);
            req["loop_instance_id"] = iid;
            Y.L(n.State, "orchestration.review_requests").Add(req);
            var entry = Entry(iid, decisions, rounds: 1, requests: 1, launches: 1, reruns: new[] { (request, 0L) }, corrections: 0);
            ((YamlMap)Y.L(entry, "authorizations")[0]!)["authorization_id"] = authorizationId;
            Y.L(n.State, "orchestration.architect_budgets").Add(entry);
            Orch(n)["next_action"] = NextAction("ARCHITECT", "REVIEW", obj, decisions);
            return n;
        }

        internal static (StatePoint, StatePoint) OpenSecondLoopPair(string authorizationId)
        {
            var closed = CloseFromSatisfied(F8Step(17));
            return (closed, OpenSecondLoop(closed, authorizationId));
        }

        internal static StatePoint ReviewerExpire(StatePoint open)
        {
            var n = Next(open);
            var a = AttemptOf(n, ReviewerSamples.R1, 1);
            a["state"] = "CANCELLED_BEFORE_LAUNCH";
            RequestOf(n, ReviewerSamples.R1)["state"] = "CANCELLED";
            End((YamlMap)Loop(n)["action_validity"]!, "EXPIRED", Rv(n), null);
            return n;
        }

        internal static StatePoint ReviewerClose(StatePoint expired, bool decision)
        {
            var n = Next(expired);
            YamlMap? closedBy = null;
            if (decision)
            {
                closedBy = WriteDecisions(n, new[] { G0Entry, "```text\nI62-REVIEWER-LOOP-CLOSE: " + ReviewerSamples.R1 + "\n```" });
            }

            var loop = Loop(n);
            Y.L(n.State, "orchestration.reviewer_closures").Add(M(("last_request", ReviewerSamples.R1), ("authorization", Clone((YamlMap)loop["authorization"]!)),
                ("validity", Clone((YamlMap)loop["action_validity"]!)), ("closed_at", Rv(n)), ("closed_by", closedBy)));
            loop["type"] = "NONE";
            loop["phase"] = "NONE";
            loop["object"] = null;
            loop["authorization"] = null;
            loop["action_validity"] = null;
            Orch(n)["next_action"] = NextAction("PRINCIPAL_COORDINATOR", "NEXT_GATE", null);
            return n;
        }

        internal static StatePoint ReviewerReopen(StatePoint closed, bool newContract)
        {
            var n = Next(closed);
            var contract = newContract ? ReviewerSamples.Contract(n, "T-03") : Clone((YamlMap)Y.Get((YamlMap)Y.L(n.State, "orchestration.reviewer_closures")[0]!, "authorization")!);
            var obj = Obj(C1, X, B1);
            var loop = Loop(n);
            loop["type"] = "REVIEWER";
            loop["phase"] = "REVIEW_PENDING";
            loop["object"] = Clone(obj);
            loop["authorization"] = Clone(contract);
            loop["action_validity"] = Validity(ReviewerSamples.AuthorityOf(contract));
            Orch(n)["next_action"] = NextAction("REVIEWER", "REVIEW_CHANGE", obj, contract);
            return n;
        }
    }
}
