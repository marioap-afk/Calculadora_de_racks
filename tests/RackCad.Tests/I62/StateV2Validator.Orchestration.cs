#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using static RackCad.Tests.Orchestration;

namespace RackCad.Tests
{
    public sealed partial class StateV2Validator
    {
        private static readonly string[] OpenLineage = { "OPEN", "STILL_OPEN" };

        // ================================================================== I-S18 (file), with A-1

        private void FileOrchestration(StatePoint point, List<StateViolation> v)
        {
            var s = point.State;
            void Add(bool ok, string msg, string clause = "")
            {
                if (!ok && On("I-S18") && (clause.Length == 0 || On(clause)))
                {
                    v.Add(new StateViolation("I-S18", clause, msg));
                }
            }

            var loop = Y.M(s, "orchestration.loop")!;
            var type = Y.S(loop, "type");
            var phase = Y.S(loop, "phase");
            var validity = Y.M(loop, "action_validity");
            var requests = Y.L(s, "orchestration.review_requests").Cast<YamlMap>().ToList();
            var findings = Y.L(s, "orchestration.findings").Cast<YamlMap>().ToList();
            var entries = Y.L(s, "orchestration.architect_budgets").Cast<YamlMap>().ToList();
            var history = RebaseChain.History(point) ?? new List<RebaseMapDoc>();

            // Loop shape by type (B.8.8; A-1 D1-1, D1-10, D1-13, D1-16).
            Add((phase == None) == (type == None), "loop.phase NONE ⇔ loop.type NONE");
            if (type == None)
            {
                Add(loop["object"] == null && loop["authorization"] == null && validity == null && loop["instance_id"] == null,
                    "a loop of type NONE keeps object, authorization, action_validity and instance_id null");
            }
            else if (type == ArchitectReview || type == Reviewer)
            {
                Add(loop["object"] != null && loop["authorization"] != null && validity != null, "an active review loop without object, authorization or action_validity");
                Add(StateV2Shape.ReviewPhases.Contains(phase), "loop.phase " + phase + " is not a review phase");
            }

            var reason = Y.S(validity, "ended_reason");
            Add(!((phase == "ARCHITECT_SATISFIED" || reason == "ARCHITECT_SATISFIED") && type != ArchitectReview)
                && !((phase == "REVIEWER_SATISFIED" || reason == "REVIEWER_SATISFIED") && type != Reviewer),
                "ARCHITECT_SATISFIED belongs only to ARCHITECT_REVIEW and REVIEWER_SATISFIED only to REVIEWER", "D1-16");
            var iid = Y.S(loop, "instance_id");
            Add((iid != null) == (type == ArchitectReview) && (iid == null || (iid.StartsWith("ARL-", StringComparison.Ordinal) && long.TryParse(iid.Substring(4), out _))),
                "loop.instance_id is ARL-<record_version> exactly with ARCHITECT_REVIEW", "D1-1");
            if (type == Reviewer || type == "EXECUTION")
            {
                Add(!requests.Any(r => Y.S(r, "state") == "OPEN" && r["loop_instance_id"] != null), "a REVIEWER or EXECUTION loop with an open Architect request", "D1-13");
            }

            if (validity != null)
            {
                var ended = Y.S(validity, "state") == "ENDED";
                Add(ended == (validity["ended_reason"] != null) && ended == (validity["ended_utc"] != null), "action_validity: ended_* are null exactly with OPEN");
            }

            // Requests and attempts (B.8.8 I-S18).
            Add(requests.Count(r => Y.S(r, "state") == "OPEN") <= 1, "more than one OPEN request");
            Add(requests.Select(r => Y.S(r, "logical_review_request_id")).Distinct().Count() == requests.Count, "review_requests not unique by logical_review_request_id");
            foreach (var r in requests)
            {
                var attempts = Y.L(r, "attempts").Cast<YamlMap>().ToList();
                var id = Y.S(r, "logical_review_request_id");
                Add(attempts.Count(a => !Terminal.Contains(Y.S(a, "state"))) <= 1, id + ": more than one non-terminal attempt");
                Add(attempts.Select(a => Y.N(a, "attempt_seq")).SequenceEqual(Enumerable.Range(1, attempts.Count).Select(i => (long?)i)), id + ": attempt_seq not consecutive from 1");
                foreach (var a in attempts)
                {
                    var st = Y.S(a, "state")!;
                    var tag = id + "/" + Y.N(a, "attempt_seq");
                    Add(!Launched.Contains(st) || a["run_id"] != null, tag + ": run_id null from LAUNCHING");
                    Add(st != "CANCELLED_BEFORE_LAUNCH" || (a["result"] == null && a["ingested_at"] == null), tag + ": a CANCELLED_BEFORE_LAUNCH attempt with a result");
                    Add(st != "CANCELLED_BEFORE_LAUNCH" || a["launching_utc"] == null || a["not_started_evidence"] != null,
                        tag + ": cancelled after LAUNCHING without the not-started evidence");
                    Add(Launched.Contains(st) || st == "CANCELLED_BEFORE_LAUNCH" || a["reserved_at"] != null || st == "INVOCATION_PLANNED",
                        tag + ": an attempt past INVOCATION_PLANNED without reserved_at");
                    if (NotLaunched.Contains(st))
                    {
                        Add(YamlSubset.DeepEquals(Target(point, a), r["object"]), tag + ": the Target of a not-launched attempt is not the object of its request");
                        Add(validity == null || Y.S(validity, "state") != "ENDED", tag + ": a not-launched attempt with the action validity ended");
                    }

                    if (Launched.Contains(st) && st != "LAUNCH_UNCERTAIN")
                    {
                        var fidelity = StateTreeReader.Json(point.Tree, a["input_fidelity"]);
                        var fphase = J.S(fidelity, "Fidelity.Phase");
                        var fstatus = J.S(fidelity, "Fidelity.FidelityStatus");
                        Add(fphase == "POSTRUN" || fstatus == "FAITHFUL" || fstatus == "FAITHFUL_NORMALIZED",
                            tag + ": launched without a FAITHFUL or FAITHFUL_NORMALIZED preflight");
                    }

                    if (st == "RESULT_INGESTED" && Y.S(a, "outcome") == "VALID")
                    {
                        var fs = Y.S(a, "fidelity_status");
                        Add(fs == "FAITHFUL" || fs == "FAITHFUL_NORMALIZED" || fs == "DEGRADED_BOUNDED", tag + ": a VALID ingestion without an admissible fidelity_status");
                        Add(fs != "DEGRADED_BOUNDED" || a["premise_independence"] != null, tag + ": DEGRADED_BOUNDED without premise_independence");
                        if (r["loop_instance_id"] != null)
                        {
                            Add(a["runtime_evidence"] != null, tag + ": an ingested ARCHITECT result without runtime evidence");
                        }
                    }

                    var binding = StateTreeReader.Json(point.Tree, a["binding"]);
                    if (J.S(binding, "Acceptance.Basis") == "AUTHORIZED_MATERIALIZATION")
                    {
                        var criteria = (binding?["Acceptance"]?["MaterializationCheck"]?["Criteria"] as JsonArray)?.OfType<JsonObject>().ToList();
                        Add(binding?["Acceptance"]?["AuthorizationRef"] is JsonObject && criteria is { Count: > 0 } && criteria.All(c => J.S(c, "Result") == "SATISFIED"),
                            tag + ": a materialized binding without AuthorizationRef or with a criterion not SATISFIED");
                    }
                }
            }

            if (type != None)
            {
                var open = requests.FirstOrDefault(r => Y.S(r, "state") == "OPEN");
                var live = open == null ? null : Y.L(open, "attempts").Cast<YamlMap>().FirstOrDefault(a => !Terminal.Contains(Y.S(a, "state")));
                if (phase == "REVIEW_PENDING" || phase == "REREVIEW_PENDING")
                {
                    Add(live == null || NotLaunched.Contains(Y.S(live, "state")), "a PENDING phase with a launched attempt");
                }
                else if (phase == "ARCHITECT_INVOKED")
                {
                    Add(live != null && new[] { "LAUNCHING", "LAUNCHED", "RESULT_RECEIVED" }.Contains(Y.S(live, "state")), "ARCHITECT_INVOKED without its launched attempt");
                }
            }

            // Lineages (§20.5.2; I-S18; P-20).
            foreach (var f in findings)
            {
                var lineage = Y.S(f, "lineage_id");
                var issuer = Y.S(f, "issuer");
                var state = Y.S(f, "state");
                if (state == "CLOSED" || state == "SUPERSEDED")
                {
                    var closedBy = f["closed_by"];
                    var result = StateTreeReader.Json(point.Tree, closedBy);
                    var ok = issuer switch
                    {
                        "ARCHITECT" => ResultSchema(result) == "rackcad-architect-review-result/v1",
                        "REVIEWER" => ResultSchema(result) == "rackcad-reviewer-result/v1",
                        "COORDINATOR" => IsDecisionsRef(closedBy) && StateTreeReader.Resolves(point.Tree, closedBy),
                        _ => false,
                    };
                    Add(ok, lineage + ": closed without a result with authority for an issuer " + issuer);
                    if (ok && issuer != "COORDINATOR")
                    {
                        var request = requests.FirstOrDefault(r => Y.S(r, "logical_review_request_id") == J.S(result, "LogicalReviewRequestId"));
                        Add(request != null && RebaseChain.Equivalent(Evaluated(result), Y.M(request, "object"), history),
                            lineage + ": closed by a result whose evaluated object is not equivalent to the object of its request", "D2-12");
                    }
                }

                if (f["downgraded_by"] != null)
                {
                    Add(IssuerKind(point, f["downgraded_by"]) == IssuerKind(point, f["opened_in"]), lineage + ": downgraded by someone other than its issuer");
                }

                if (issuer == "REVIEWER")
                {
                    Add(Y.S(f, "severity") == "BLOCKING" || Y.S(f, "severity") == "ADVISORY", lineage + ": a REVIEWER lineage without BLOCKING or ADVISORY", "D1-17");
                }
                else
                {
                    Add(Y.S(f, "severity") == "REQUIRED" || Y.S(f, "severity") == "OPTIONAL", lineage + ": a lineage of " + issuer + " with a REVIEWER severity");
                }
            }

            // ARCHITECT_SATISFIED (I-S18).
            if (phase == "ARCHITECT_SATISFIED")
            {
                var last = Attempts(s).Where(x => x.Request["loop_instance_id"] != null && Y.S(x.Attempt, "state") == "RESULT_INGESTED")
                    .OrderBy(x => Y.N(x.Attempt, "ingested_at")).LastOrDefault();
                var result = last.Attempt == null ? null : Result(point, last.Attempt);
                var obj = Y.M(loop, "object");
                Add(last.Attempt != null && Y.S(last.Attempt, "outcome") == "VALID" && ResultSchema(result) == "rackcad-architect-review-result/v1"
                    && J.S(result, "Verdict") == "AGREED" && J.S(result, "ReviewedPath") == Y.S(obj, "path") && J.S(result, "ReviewedBlob") == Y.S(obj, "blob")
                    && (Y.S(last.Attempt, "fidelity_status") == "FAITHFUL" || Y.S(last.Attempt, "fidelity_status") == "FAITHFUL_NORMALIZED"),
                    "ARCHITECT_SATISFIED without a VALID, faithful AGREED result on the blob of loop.object");
                Add(!findings.Any(f => Y.S(f, "severity") == "REQUIRED" && OpenLineage.Contains(Y.S(f, "state"))), "ARCHITECT_SATISFIED with a REQUIRED lineage open");
            }

            // REVIEWER_SATISFIED (D1-17).
            if (phase == "REVIEWER_SATISFIED")
            {
                var loopRequests = ReviewerLoopRequests(s);
                var lastRequest = loopRequests.LastOrDefault();
                Add(lastRequest != null && Y.S(lastRequest, "state") == "INGESTED"
                    && Y.L(lastRequest, "attempts").Cast<YamlMap>().Any(a => Y.S(a, "state") == "RESULT_INGESTED" && Y.S(a, "outcome") == "VALID"
                                                                          && ResultSchema(Result(point, a)) == "rackcad-reviewer-result/v1")
                    && loopRequests.All(r => Y.L(r, "attempts").Cast<YamlMap>().All(a => Terminal.Contains(Y.S(a, "state"))))
                    && !findings.Any(f => Y.S(f, "issuer") == "REVIEWER" && Y.S(f, "severity") == "BLOCKING" && OpenLineage.Contains(Y.S(f, "state")))
                    && Y.S(validity, "state") == "ENDED" && reason == "REVIEWER_SATISFIED",
                    "REVIEWER_SATISFIED without its conditions (VALID result on the last request, no live attempt, no open BLOCKING in the unit, validity ended)",
                    "D1-17");
            }

            // Escalation and next action (I-S18).
            var role = Y.S(s, "orchestration.next_action.role");
            var escalation = Y.S(s, "orchestration.escalation.state");
            Add((escalation == None) == (role != "OWNER" && role != "COORDINATOR"), "escalation NONE ⇔ next_action.role ∉ {OWNER, COORDINATOR}");
            Add(escalation != None || (Y.Get(s, "orchestration.escalation.reason") == null && Y.Get(s, "orchestration.escalation.required_decision") == null),
                "escalation NONE with a reason or a required decision");
            var (ambiguities, mismatches) = NextActionDerivation.Check(point);
            foreach (var a in ambiguities)
            {
                Add(false, "NextAction is not uniquely derivable: " + a, "P-17");
            }

            foreach (var m in mismatches)
            {
                Add(false, m, "NextAction");
            }

            // Per-loop budgets (A-1 D1-2, D1-4, D1-5).
            foreach (var e in entries)
            {
                var eid = Y.S(e, "loop_instance_id");
                var mine = requests.Where(r => Y.S(r, "loop_instance_id") == eid).ToList();
                CheckCounters(point, e, mine, Y.M(e, "caps")!.ToDictionary(kv => kv.Key, kv => (long)kv.Value!), "D1-5", Add);
                var caps = EffectiveCaps(point.Tree, Y.L(e, "authorizations").Cast<YamlMap>());
                Add(caps != null && caps.All(kv => Y.N(e, "caps." + kv.Key) == kv.Value), eid + ": caps ≠ the minimum of the frozen constants and each authorization's Budget", "D1-2");
            }

            if (type == ArchitectReview)
            {
                var e = entries.FirstOrDefault(x => Y.S(x, "loop_instance_id") == iid);
                var lastAuth = e == null ? null : Y.L(e, "authorizations").Cast<YamlMap>().LastOrDefault();
                Add(e != null && e["closed_at"] == null && lastAuth != null && SameValidity(lastAuth, validity),
                    "the active ARCHITECT_REVIEW loop has no open entry whose last authorization record is its action_validity", "D1-5");
            }

            Add(entries.All(e => e["closed_at"] != null || (type == ArchitectReview && Y.S(e, "loop_instance_id") == iid)),
                "an open budget entry that is not the active loop's", "D1-5");
            Add(entries.Select(e => Y.S(e, "loop_instance_id")).Distinct().Count() == entries.Count, "architect_budgets not unique by loop_instance_id", "D1-2");
            Add(requests.All(r => r["loop_instance_id"] == null || entries.Any(e => Y.S(e, "loop_instance_id") == Y.S(r, "loop_instance_id"))),
                "a request with loop_instance_id without its budget entry", "D1-5");
            var unitBudgets = Y.M(s, "orchestration.budgets")!;
            CheckCounters(point, unitBudgets, requests.Where(r => r["loop_instance_id"] == null).ToList(), FrozenCaps.ToDictionary(kv => kv.Key, kv => kv.Value), "D1-4", Add);

            // Reviewer closures (D1-19).
            foreach (var c in Y.L(s, "orchestration.reviewer_closures").Cast<YamlMap>())
            {
                Add(Y.S(c, "validity.state") == "ENDED" && c["validity"] is YamlMap cv && cv["ended_reason"] != null, "a reviewer_closures record without an ended validity", "D1-19");
            }
        }

        private static YamlMap CloneMap(YamlMap m) => (YamlMap)YamlSubset.DeepClone(m)!;

        private static bool SameValidity(YamlMap record, YamlMap? validity) =>
            validity != null && new[] { "authorization_id", "state", "ended_at", "ended_utc", "ended_reason", "ended_by" }
                .All(k => YamlSubset.DeepEquals(record.TryGetValue(k, out var a) ? a : null, validity.TryGetValue(k, out var b) ? b : null));

        private static string IssuerKind(StatePoint point, object? stateRef)
        {
            if (IsDecisionsRef(stateRef))
            {
                return "COORDINATOR";
            }

            return ResultSchema(StateTreeReader.Json(point.Tree, stateRef)) switch
            {
                "rackcad-architect-review-result/v1" => "ARCHITECT",
                "rackcad-reviewer-result/v1" => "REVIEWER",
                _ => "UNKNOWN",
            };
        }

        /// <summary>I-S18 budgets for one counter set (a per-loop entry of A-1 D1-5, or the unit budgets of the REVIEWER, D1-4).</summary>
        private static void CheckCounters(StatePoint point, YamlMap budget, List<YamlMap> requests, Dictionary<string, long> caps, string clause,
            Action<bool, string, string> add)
        {
            var reserved = requests.Sum(r => Y.L(r, "attempts").Cast<YamlMap>().Count(a => a["reserved_at"] != null));
            var name = Y.S(budget, "loop_instance_id") ?? "budgets";
            add(Y.N(budget, "architect_launches") == reserved, name + ": architect_launches ≠ the reserved attempts of its requests", clause);
            add(Y.N(budget, "review_rounds") <= Y.N(budget, "logical_requests"), name + ": review_rounds > logical_requests", clause);
            foreach (var key in new[] { "review_rounds", "logical_requests", "architect_launches", "correction_rounds" })
            {
                add(Y.N(budget, key) <= caps[key], name + ": " + key + " above its cap (P-18)", clause);
            }

            var reruns = Y.L(budget, "transport_reruns").Cast<YamlMap>().ToList();
            foreach (var r in requests)
            {
                var count = Y.L(r, "attempts").Cast<YamlMap>().Count(a => a["reserved_at"] != null);
                if (count == 0)
                {
                    continue;
                }

                var entry = reruns.FirstOrDefault(x => Y.S(x, "logical_review_request_id") == Y.S(r, "logical_review_request_id"));
                add(entry != null && Y.N(entry, "count") == count - 1, name + ": transport_reruns(" + Y.S(r, "logical_review_request_id") + ") ≠ reserved attempts − 1", clause);
                add(count - 1 <= caps["transport_reruns_per_request"], name + ": transport reruns above their cap (P-18)", clause);
            }

            foreach (var l in Y.L(budget, "corrections_by_lineage").Cast<YamlMap>())
            {
                add(Y.N(l, "count") <= caps["corrections_per_lineage"], name + ": corrections of " + Y.S(l, "lineage_id") + " above their cap (P-18)", clause);
            }
        }

        // ================================================================== I-P13 (pairs), with A-1

        private void PairOrchestration(StatePoint p, StatePoint n, PairContext ctx, List<StateViolation> v)
        {
            void Add(bool ok, string msg, string clause = "")
            {
                if (!ok && On("I-P13") && (clause.Length == 0 || On(clause)))
                {
                    v.Add(new StateViolation("I-P13", clause, msg));
                }
            }

            var ps = p.State;
            var ns = n.State;
            var pl = Y.M(ps, "orchestration.loop")!;
            var nl = Y.M(ns, "orchestration.loop")!;
            var (ptype, ntype) = (Y.S(pl, "type"), Y.S(nl, "type"));
            var (pphase, nphase) = (Y.S(pl, "phase")!, Y.S(nl, "phase")!);
            var rebase = Y.S(ns, "custody.point_kind") == "REBASE_RECONCILIATION";
            var opening = ptype == None && ntype == ArchitectReview;
            var rvOpening = ptype == None && ntype == Reviewer;
            var closing = ptype == ArchitectReview && ntype == None;
            var rvClosing = ptype == Reviewer && ntype == None;
            var same = ptype == ArchitectReview && ntype == ArchitectReview;
            var nrv = Y.N(ns, "custody.record_version");
            var pv = Y.M(pl, "action_validity");
            var nv = Y.M(nl, "action_validity");
            var nhist = RebaseChain.History(n) ?? new List<RebaseMapDoc>();
            var images = Images(StateTreeReader.Json(n.Tree, Y.Get(ns, "custody.last_rebase.map")));
            var eq = new PairRefs(p, n);

            // Phase edges (§20.5; SM-05; D1-9, D1-16, D1-18).
            if (pphase != nphase && !closing && !rvClosing && !(opening || rvOpening))
            {
                Add(PhaseEdges.Contains((pphase, nphase)), "phase " + pphase + " → " + nphase + " does not exist");
            }

            if (opening || rvOpening)
            {
                Add(nphase == "REVIEW_PENDING", "a loop opens in " + nphase + ", not REVIEW_PENDING");
            }

            if (ptype != None && ptype != ArchitectReview && ptype != Reviewer && ntype == None)
            {
                Add(false, "no variant of LOOP_CLOSED applies to an " + ptype + " loop", "D1-13");
            }

            if (ptype != None && ntype != None && ptype != ntype)
            {
                Add(false, "the loop type changes from " + ptype + " to " + ntype + " without closing");
            }

            // loop.object (I-P13; D1-10, D2-4).
            if (!YamlSubset.DeepEquals(pl["object"], nl["object"]))
            {
                var ok = ((pphase, nphase) == ("CORRECTING", "PUBLISHED") && nl["object"] != null)
                         || ((opening || rvOpening) && pl["object"] == null && nl["object"] != null)
                         || ((closing || rvClosing) && nl["object"] == null);
                if (rebase && pl["object"] is YamlMap po && nl["object"] is YamlMap no && pphase == nphase && Y.S(po, "path") == Y.S(no, "path")
                    && Y.S(po, "blob") == Y.S(no, "blob") && images.TryGetValue(Y.S(po, "commit")!, out var img) && img == Y.S(no, "commit"))
                {
                    ok = true;
                }

                Add(ok, "loop.object changes in " + pphase + " → " + nphase, "D1-10");
            }

            if ((pphase, nphase) == ("CORRECTING", "PUBLISHED"))
            {
                var pc = CorrectionRounds(ps, pl);
                var nc = CorrectionRounds(ns, nl);
                Add(nc == pc + 1, "PUBLISHED without correction_rounds + 1");
            }

            // Loop identity (D1-1).
            var piid = Y.S(pl, "instance_id");
            var niid = Y.S(nl, "instance_id");
            Add(!(opening && niid != "ARL-" + nrv) && !(same && niid != piid) && !(closing && niid != null), "instance_id wrong for " + ptype + " → " + ntype, "D1-1");

            // Per-loop budget entries (D1-6, D1-7, D1-8).
            var pe = Y.L(ps, "orchestration.architect_budgets").Cast<YamlMap>().ToDictionary(e => Y.S(e, "loop_instance_id")!, StringComparer.Ordinal);
            var ne = Y.L(ns, "orchestration.architect_budgets").Cast<YamlMap>().GroupBy(e => Y.S(e, "loop_instance_id")!)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
            var used = new HashSet<string>(pe.Values.SelectMany(e => Y.L(e, "authorizations").Cast<YamlMap>()).Select(a => Y.S(a, "authorization_id")!), StringComparer.Ordinal);
            var newIds = ne.Keys.Except(pe.Keys).ToList();
            if (opening)
            {
                var aid = Y.S(nv, "authorization_id");
                var e = niid != null && ne.TryGetValue(niid, out var x) ? x : null;
                var rla = aid == null ? null : Authorization(n.Tree, nl["authorization"], aid);
                var auths = e == null ? new List<YamlMap>() : Y.L(e, "authorizations").Cast<YamlMap>().ToList();
                Add(newIds.Count == 1 && newIds[0] == niid && e != null && auths.Count == 1 && Y.S(auths[0], "authorization_id") == aid && Y.S(auths[0], "state") == "OPEN"
                    && YamlSubset.DeepEquals(auths[0]["authorization"], nl["authorization"]) && rla != null
                    && (!rla.TryGetValue("ContinuesLoopInstanceId", out var cont) || cont == "null") && aid != null && !used.Contains(aid)
                    && new[] { "review_rounds", "logical_requests", "architect_launches", "correction_rounds" }.All(k => Y.N(e, k) <= 1),
                    "the opening does not create exactly one entry with a new, never used ReviewLoopAuthorization without ContinuesLoopInstanceId", "D1-7");
            }
            else
            {
                Add(newIds.Count == 0, "a budget entry appears outside the opening of an ARCHITECT_REVIEW loop", "D1-7");
            }

            if (same && pv != null && nv != null && Y.S(pv, "authorization_id") != Y.S(nv, "authorization_id"))
            {
                var a2 = Y.S(nv, "authorization_id")!;
                var rla = Authorization(n.Tree, nl["authorization"], a2);
                var ep = piid != null && pe.TryGetValue(piid, out var xp) ? xp : null;
                var en = niid != null && ne.TryGetValue(niid, out var xn) ? xn : null;
                var bad = rla == null || !rla.TryGetValue("ContinuesLoopInstanceId", out var cont) || cont != piid || used.Contains(a2) || ep == null || en == null;
                if (!bad)
                {
                    var pa = Y.L(ep!, "authorizations").Cast<YamlMap>().ToList();
                    var na = Y.L(en!, "authorizations").Cast<YamlMap>().ToList();
                    var last = pa[pa.Count - 1];
                    YamlMap? prev = null;
                    if (Y.S(last, "state") == "OPEN")
                    {
                        prev = CloneMap(last);
                        prev["state"] = "ENDED";
                        prev["ended_reason"] = "SUPERSEDED";
                        bad |= na.Count < pa.Count || Y.S(na[pa.Count - 1], "state") != "ENDED" || Y.S(na[pa.Count - 1], "ended_reason") != "SUPERSEDED"
                                || na[pa.Count - 1]["ended_by"] == null;
                    }
                    else if (Y.S(last, "ended_reason") is "EXPIRED" or "REVOKED")
                    {
                        prev = last;
                        bad |= na.Count < pa.Count || !eq.Eq(na[pa.Count - 1], last);
                    }
                    else
                    {
                        bad = true;
                    }

                    bad |= na.Count != pa.Count + 1 || pa.Take(pa.Count - 1).Select((r, i) => eq.Eq(r, na[i])).Any(eq => !eq)
                           || Y.S(na[na.Count - 1], "authorization_id") != a2 || Y.S(na[na.Count - 1], "state") != "OPEN";
                    bad |= new[] { "review_rounds", "logical_requests", "architect_launches", "correction_rounds" }.Any(k => Y.N(en!, k) != Y.N(ep!, k));
                }

                bad |= pphase != nphase || !YamlSubset.DeepEquals(pl["object"], nl["object"]);
                Add(!bad, "invalid substitution or continuation inside loop " + piid, "D1-8");
            }

            foreach (var (id, e) in pe)
            {
                if (!ne.TryGetValue(id, out var f))
                {
                    Add(false, "the budget entry " + id + " disappears", "D1-6");
                    continue;
                }

                var active = ptype == ArchitectReview && piid == id;
                Add(new[] { "review_rounds", "logical_requests", "architect_launches", "correction_rounds" }.All(k => Y.N(f, k) >= Y.N(e, k))
                    && (active || closing && piid == id || eq.Eq(e, f))
                    && Orchestration.FrozenCaps.Keys.All(k => Y.N(f, "caps." + k) <= Y.N(e, "caps." + k)),
                    "the entry " + id + " decreases, raises its caps or changes while closed", "D1-6");
                var pa = Y.L(e, "authorizations").Cast<YamlMap>().ToList();
                var na = Y.L(f, "authorizations").Cast<YamlMap>().ToList();
                for (var i = 0; i < pa.Count; i++)
                {
                    Add(i < na.Count && Y.S(na[i], "authorization_id") == Y.S(pa[i], "authorization_id")
                        && (Y.S(pa[i], "state") != "ENDED" || eq.Eq(pa[i], na[i])), "an authorization record of " + id + " changes", "D1-6");
                }
            }

            var pb = Y.M(ps, "orchestration.budgets")!;
            var nb = Y.M(ns, "orchestration.budgets")!;
            Add(new[] { "review_rounds", "logical_requests", "architect_launches", "correction_rounds" }.All(k => Y.N(nb, k) >= Y.N(pb, k)), "a counter of budgets decreases");

            // Requests and attempts (I-P13; D1-3, D1-14, D1-15, D2-2, D2-5, D2-6, D2-8, D2-12).
            var preqs = Y.L(ps, "orchestration.review_requests").Cast<YamlMap>().ToDictionary(r => Y.S(r, "logical_review_request_id")!, StringComparer.Ordinal);
            var nreqs = Y.L(ns, "orchestration.review_requests").Cast<YamlMap>().ToList();
            var pfind = Y.L(ps, "orchestration.findings").Cast<YamlMap>().ToDictionary(f => Y.S(f, "lineage_id")!, StringComparer.Ordinal);
            var nfind = Y.L(ns, "orchestration.findings").Cast<YamlMap>().ToList();
            foreach (var id in preqs.Keys.Where(id => nreqs.All(r => Y.S(r, "logical_review_request_id") != id)))
            {
                Add(false, "the request " + id + " disappears");
            }

            foreach (var lid in pfind.Keys.Where(l => nfind.All(f => Y.S(f, "lineage_id") != l)))
            {
                Add(false, "the lineage " + lid + " disappears");
            }

            var ingestedNow = new List<(YamlMap Request, YamlMap Attempt)>();
            foreach (var r in nreqs)
            {
                var id = Y.S(r, "logical_review_request_id")!;
                if (!preqs.TryGetValue(id, out var q))
                {
                    Add(YamlSubset.DeepEquals(r["object"], nl["object"]), "the new request " + id + " is not opened on loop.object", "A62-A1S-02");
                    foreach (var a in Y.L(r, "attempts").Cast<YamlMap>().Where(a => a["reserved_at"] != null))
                    {
                        NewReservation(n, r, a, Add);
                    }

                    continue;
                }

                Add(YamlSubset.DeepEquals(q["loop_instance_id"], r["loop_instance_id"]), "the loop_instance_id of " + id + " changes", "D1-3");
                if (!YamlSubset.DeepEquals(q["object"], r["object"]))
                {
                    var ok = rebase && Y.S(q, "state") == "OPEN" && Y.S(q, "object.path") == Y.S(r, "object.path") && Y.S(q, "object.blob") == Y.S(r, "object.blob")
                             && images.TryGetValue(Y.S(q, "object.commit")!, out var img) && img == Y.S(r, "object.commit");
                    Add(ok, "the object of " + id + " changes without its proven image", "D2-5");
                }

                var qa = Y.L(q, "attempts").Cast<YamlMap>().ToDictionary(a => Y.N(a, "attempt_seq")!.Value);
                foreach (var a in Y.L(r, "attempts").Cast<YamlMap>())
                {
                    var seq = Y.N(a, "attempt_seq")!.Value;
                    var tag = id + "/" + seq;
                    if (!qa.TryGetValue(seq, out var b))
                    {
                        if (a["reserved_at"] != null)
                        {
                            NewReservation(n, r, a, Add);
                        }

                        continue;
                    }

                    var (bs, ast) = (Y.S(b, "state")!, Y.S(a, "state")!);
                    var edge = (bs, ast);
                    var invChanged = !YamlSubset.DeepEquals(b["invocation"], a["invocation"]);
                    Add((bs == ast && !(bs == "INVOCATION_PLANNED" && invChanged && !rebase)) || AttemptEdges.Contains(edge)
                        || (rebase && edge == ("INVOCATION_PLANNED", "INVOCATION_PLANNED")), "attempt " + tag + " " + bs + " → " + ast, "D2-6");
                    Add(!Terminal.Contains(bs) || eq.Eq(a, b), "the terminal attempt " + tag + " changes");
                    Add(b["reserved_at"] == null || YamlSubset.DeepEquals(b["reserved_at"], a["reserved_at"]), "reserved_at of " + tag + " changes");
                    if (invChanged)
                    {
                        if (NotLaunched.Contains(bs) && ast == bs)
                        {
                            var bi = Invocation(p, b);
                            var ai = Invocation(n, a);
                            Add(J.S(bi, "InvocationId") != null && J.S(bi, "InvocationId") != J.S(ai, "InvocationId") && YamlSubset.DeepEquals(b["reserved_at"], a["reserved_at"]),
                                "replanning of " + tag + " without a new InvocationId or outside its reservation", "D2-6");
                            if (rebase)
                            {
                                var bt = Target(p, b);
                                var at = Target(n, a);
                                Add(bt != null && at != null && Y.S(bt, "path") == Y.S(at, "path") && Y.S(bt, "blob") == Y.S(at, "blob")
                                    && images.TryGetValue(Y.S(bt, "commit")!, out var img) && img == Y.S(at, "commit"),
                                    "the replanned Target of " + tag + " is not the proven image", "D2-2");
                                Add(JsonNode.DeepEquals(bi?["BudgetSnapshot"], ai?["BudgetSnapshot"]), "the replanning of " + tag + " changes its BudgetSnapshot", "D2-2");
                            }
                        }
                        else if (edge == ("LAUNCHING", "BUDGET_RESERVED"))
                        {
                            Add(J.S(Invocation(p, b), "InvocationId") != J.S(Invocation(n, a), "InvocationId") && YamlSubset.DeepEquals(b["reserved_at"], a["reserved_at"]),
                                "case B.1 of " + tag + " without a new invocation inside its reservation", "D2-6");
                        }
                        else
                        {
                            Add(false, "the invocation of " + tag + " (" + bs + ") is rewritten in place", "D2-6");
                        }
                    }

                    if (rebase)
                    {
                        Add(bs == ast && YamlSubset.DeepEquals(b["reserved_at"], a["reserved_at"]) && YamlSubset.DeepEquals(b["run_id"], a["run_id"]),
                            "the reconciliation changes the attempt " + tag + " (state, reserved_at or run_id)", "D2-8");
                    }

                    if (bs == "LAUNCHING" && (ast == "BUDGET_RESERVED" || ast == "CANCELLED_BEFORE_LAUNCH"))
                    {
                        Add(a["not_started_evidence"] != null, tag + " leaves LAUNCHING without the not-started evidence");
                    }

                    if (bs == "LAUNCHING" && ast == "BUDGET_RESERVED")
                    {
                        Add(nv != null && Y.S(nv, "state") == "OPEN", "case B.1 of " + tag + " with the action validity ended (it must be CANCELLED_BEFORE_LAUNCH)");
                    }

                    if (b["reserved_at"] == null && a["reserved_at"] != null)
                    {
                        NewReservation(n, r, a, Add);
                    }

                    if (ast == "LAUNCHING" && bs != "LAUNCHING")
                    {
                        Add(nv != null && Y.S(nv, "state") == "OPEN", "LAUNCHING of " + tag + " with the action validity not OPEN");
                    }

                    if (ast == "RESULT_RECEIVED" && bs != "RESULT_RECEIVED")
                    {
                        Add(a["runtime_evidence"] != null && a["read_audit"] != null && a["output_state"] != null && (a["result"] != null || Y.S(a, "output_state") == "ABSENT"),
                            tag + ": RESULT_RECEIVED without the custodied result, runtime evidence and read audit");
                    }

                    if (ast == "RESULT_INGESTED" && bs == "RESULT_RECEIVED")
                    {
                        Add(YamlSubset.DeepEquals(b["result"], a["result"]), tag + ": the ingested result is not the custodied one (S-04)");
                        ingestedNow.Add((r, a));
                    }
                }
            }

            PairIngestion(p, n, ingestedNow, pfind, nfind, nhist, Add);
            PairReviewerLoop(p, n, rvOpening, rvClosing, eq, Add);
            if (closing)
            {
                PairArchitectClose(p, n, pe, ne, eq, Add);
            }

            // Counter growth only with first reservations (§20.6; I-P13).
            PairCounterGrowth(p, n, Add);

            if (rebase)
            {
                Add(pphase == nphase && eq.Eq(Y.Get(ps, "orchestration.findings"), Y.Get(ns, "orchestration.findings"))
                    && eq.Eq(Y.Get(ps, "orchestration.budgets"), Y.Get(ns, "orchestration.budgets"))
                    && eq.Eq(Y.Get(ps, "orchestration.architect_budgets"), Y.Get(ns, "orchestration.architect_budgets")),
                    "the reconciliation changes the phase, the budgets or the lineages", "D2-8");
            }

            // Action validity (I-P13; D1-8, D1-20).
            if (pv != null && nv != null && Y.S(pv, "authorization_id") == Y.S(nv, "authorization_id"))
            {
                Add(!(Y.S(pv, "state") == "ENDED" && Y.S(nv, "state") == "OPEN"), "action validity ENDED → OPEN");
                Add(Y.S(pv, "state") != "ENDED" || eq.Eq(pv, nv), "an ended action validity is rewritten");
            }
        }

        private static long CorrectionRounds(YamlMap s, YamlMap loop)
        {
            if (Y.S(loop, "type") == ArchitectReview)
            {
                return Y.L(s, "orchestration.architect_budgets").Cast<YamlMap>().Where(e => Y.S(e, "loop_instance_id") == Y.S(loop, "instance_id"))
                    .Select(e => Y.N(e, "correction_rounds") ?? 0).FirstOrDefault();
            }

            return Y.N(s, "orchestration.budgets.correction_rounds") ?? 0;
        }

        private void NewReservation(StatePoint n, YamlMap r, YamlMap a, Action<bool, string, string> add)
        {
            var tag = Y.S(r, "logical_review_request_id") + "/" + Y.N(a, "attempt_seq");
            var nv = Y.M(n.State, "orchestration.loop.action_validity");
            add(nv != null && Y.S(nv, "state") == "OPEN", "reservation of " + tag + " with the action validity not OPEN", string.Empty);
            add(Y.N(a, "reserved_at") == Y.N(n.State, "custody.record_version"), "reserved_at of " + tag + " is not the record_version of its reservation", string.Empty);

            var inv = Invocation(n, a);
            var issuers = r["loop_instance_id"] == null ? new[] { "REVIEWER" } : new[] { "ARCHITECT", "COORDINATOR" };
            var required = Y.L(n.State, "orchestration.findings").Cast<YamlMap>()
                .Where(f => OpenLineage.Contains(Y.S(f, "state")) && issuers.Contains(Y.S(f, "issuer"))).Select(f => Y.S(f, "lineage_id")!).ToHashSet();
            var given = (inv?["OpenFindings"] as JsonArray)?.OfType<JsonObject>().Select(o => J.S(o, "LineageId")!).ToHashSet() ?? new HashSet<string>();
            add(inv != null && given.SetEquals(required), "OpenFindings of " + tag + " is not the set of open lineages its reviewer must dispose", "D1-15");

            if (r["loop_instance_id"] != null)
            {
                var e = Y.L(n.State, "orchestration.architect_budgets").Cast<YamlMap>().FirstOrDefault(x => Y.S(x, "loop_instance_id") == Y.S(r, "loop_instance_id"));
                var snap = inv?["BudgetSnapshot"] as JsonObject;
                add(e != null && snap != null && J.N(snap, "ReviewRounds") == Y.N(e, "review_rounds") && J.N(snap, "LogicalRequests") == Y.N(e, "logical_requests")
                    && J.N(snap, "ArchitectLaunches") == Y.N(e, "architect_launches") && J.N(snap, "CorrectionRounds") == Y.N(e, "correction_rounds"),
                    "BudgetSnapshot of " + tag + " is not a copy of its loop's entry", "D1-14");
            }
        }

        private void PairIngestion(StatePoint p, StatePoint n, List<(YamlMap Request, YamlMap Attempt)> ingestedNow, Dictionary<string, YamlMap> pfind,
            List<YamlMap> nfind, IReadOnlyList<RebaseMapDoc> nhist, Action<bool, string, string> add)
        {
            var closedNow = nfind.Where(f => (Y.S(f, "state") is "CLOSED" or "SUPERSEDED")
                                             && !(pfind.TryGetValue(Y.S(f, "lineage_id")!, out var q) && Y.S(q, "state") is "CLOSED" or "SUPERSEDED")).ToList();
            foreach (var f in closedNow)
            {
                var lid = Y.S(f, "lineage_id");
                if (Y.S(f, "issuer") == "COORDINATOR")
                {
                    add(IsDecisionsRef(f["closed_by"]), lid + ": a COORDINATOR lineage closed without a decision", string.Empty);
                    continue;
                }

                var source = ingestedNow.FirstOrDefault(x => YamlSubset.DeepEquals(x.Attempt["result"], f["closed_by"]));
                add(source.Attempt != null, lid + ": closed in a pair where no attempt ingests the result that closes it", string.Empty);
                if (source.Attempt == null)
                {
                    continue;
                }

                var result = Result(n, source.Attempt);
                var ids = Y.L(f, "finding_ids").Cast<string>().ToHashSet();
                var disposition = (result?["FindingDispositions"] as JsonArray)?.OfType<JsonObject>()
                    .FirstOrDefault(d => ids.Contains(J.S(d, "FindingId") ?? string.Empty) || J.S(d, "LineageId") == lid);
                var unaccredited = Y.L(source.Attempt, "unaccredited").Cast<string>().ToHashSet();
                add(disposition != null && (J.S(disposition, "State") is "CLOSED" or "SUPERSEDED") && !ids.Overlaps(unaccredited)
                    && Y.S(source.Attempt, "outcome") == "VALID",
                    lid + ": closed without a CLOSED disposition with faithful premises in a VALID result (omission is not closure)", string.Empty);
                if (Y.S(f, "issuer") == "ARCHITECT")
                {
                    add(ResultSchema(result) == "rackcad-architect-review-result/v1", lid + ": an ARCHITECT lineage closed by a REVIEWER result (P-20)", "P-20");
                }
            }

            // P-19 and §20.5.2: a lineage opens only from a finding of a VALID result ingested in this pair (a COORDINATOR lineage, from a decision);
            // an invalid or incomplete output never changes the lineage.
            foreach (var f in nfind.Where(f => !pfind.ContainsKey(Y.S(f, "lineage_id")!)))
            {
                var lid = Y.S(f, "lineage_id");
                if (Y.S(f, "issuer") == "COORDINATOR")
                {
                    add(IsDecisionsRef(f["opened_in"]), lid + ": a COORDINATOR lineage opened without a decision", "P-19");
                    continue;
                }

                var source = ingestedNow.FirstOrDefault(x => YamlSubset.DeepEquals(x.Attempt["result"], f["opened_in"]));
                var result = source.Attempt == null ? null : Result(n, source.Attempt);
                var reported = new[] { "RequiredFindings", "OptionalFindings", "Findings" }
                    .SelectMany(k => (result?[k] as JsonArray)?.OfType<JsonObject>().Select(o => J.S(o, "FindingId")) ?? Enumerable.Empty<string?>())
                    .OfType<string>().ToHashSet();
                add(source.Attempt != null && Y.S(source.Attempt, "outcome") == "VALID" && Y.L(f, "finding_ids").Cast<string>().All(reported.Contains),
                    lid + ": a lineage opened outside a finding of a VALID result ingested in this pair", "P-19");
            }

            // §20.5: the ingestion point publishes the phase its verdict gives; an INVALID output only allows a transport rerun (or an escalation).
            var loop = Y.M(n.State, "orchestration.loop");
            if (ingestedNow.Count > 0 && Y.S(loop, "type") == ArchitectReview)
            {
                var (_, last) = ingestedNow[ingestedNow.Count - 1];
                var nphase = Y.S(loop, "phase");
                var verdict = J.S(Result(n, last), "Verdict");
                var expected = Y.S(last, "outcome") != "VALID" ? new[] { "REVIEW_PENDING", "REREVIEW_PENDING" }
                    : verdict switch
                    {
                        "AGREED" => new[] { "ARCHITECT_SATISFIED" },
                        "CHANGES REQUIRED" => new[] { "CORRECTING" },
                        "BLOCKED — OWNER DECISION" => new[] { "ESCALATE_OWNER" },
                        _ => Array.Empty<string>(),
                    };
                add(expected.Contains(nphase) || (Y.S(last, "outcome") != "VALID" && Y.S(n.State, "orchestration.escalation.state") != None),
                    "the ingestion of an " + Y.S(last, "outcome") + " result publishes the phase " + nphase + ", not the one its verdict gives", "P-19");
            }

            foreach (var (r, a) in ingestedNow)
            {
                var tag = Y.S(r, "logical_review_request_id") + "/" + Y.N(a, "attempt_seq");
                var result = Result(n, a);
                if (Y.S(a, "outcome") == "VALID")
                {
                    add(RebaseChain.Equivalent(Evaluated(result), Y.M(r, "object"), nhist), tag + ": a VALID result whose evaluated object is not equivalent to its request's",
                        "D2-12");
                    var architect = r["loop_instance_id"] != null;
                    add(ResultSchema(result) == (architect ? "rackcad-architect-review-result/v1" : "rackcad-reviewer-result/v1"),
                        tag + ": a VALID result with the output contract of another role", string.Empty);
                    if (architect && J.S(result, "Verdict") == "AGREED")
                    {
                        var dispositions = (result?["FindingDispositions"] as JsonArray)?.OfType<JsonObject>().Select(d => J.S(d, "FindingId") ?? string.Empty).ToHashSet()
                                           ?? new HashSet<string>();
                        var required = (result?["RequiredFindings"] as JsonArray)?.Count ?? 0;
                        var omitted = pfind.Values.Where(f => OpenLineage.Contains(Y.S(f, "state")) && Y.S(f, "issuer") != "REVIEWER")
                            .Where(f => !Y.L(f, "finding_ids").Cast<string>().Any(dispositions.Contains)).Select(f => Y.S(f, "lineage_id")).ToList();
                        add(required == 0 && omitted.Count == 0, tag + ": a VALID AGREED result with a REQUIRED finding or an open lineage omitted ("
                            + string.Join(", ", omitted) + ")", string.Empty);
                    }
                }
            }
        }

        private void PairReviewerLoop(StatePoint p, StatePoint n, bool rvOpening, bool rvClosing, PairRefs eq, Action<bool, string, string> add)
        {
            var ps = p.State;
            var ns = n.State;
            var nl = Y.M(ns, "orchestration.loop")!;
            var pl = Y.M(ps, "orchestration.loop")!;
            var pclos = Y.L(ps, "orchestration.reviewer_closures");
            var nclos = Y.L(ns, "orchestration.reviewer_closures");
            add(nclos.Count == pclos.Count + (rvClosing ? 1 : 0) && pclos.Select((c, i) => eq.Eq(c, nclos[i])).All(x => x),
                "reviewer_closures changes outside a REVIEWER LOOP_CLOSED or loses history", "D1-19");

            if (rvOpening)
            {
                var nv = Y.M(nl, "action_validity");
                var authority = ReviewerAuthorityId(nl["authorization"]);
                var ended = pclos.Cast<YamlMap>().Select(c => Y.S(c, "validity.authorization_id")).ToHashSet();
                var decisions = StateTreeReader.Text(n.Tree, DecisionsRef(n)) ?? string.Empty;
                var superseded = authority != null && decisions.Contains("I62-REVIEWER-AUTHORITY-SUPERSEDED: " + authority, StringComparison.Ordinal);
                add(nv != null && Y.S(nv, "state") == "OPEN" && Y.S(nv, "authorization_id") == authority && !ended.Contains(authority) && !superseded,
                    "a REVIEWER loop opens with an ended, superseded or different authority (no resurrection)", "D1-20");
            }

            if (Y.S(nl, "type") == Reviewer && Y.S(nl, "phase") == "REVIEWER_SATISFIED" && Y.S(pl, "phase") != "REVIEWER_SATISFIED")
            {
                var nv = Y.M(nl, "action_validity");
                add(nv != null && Y.S(nv, "state") == "ENDED" && Y.S(nv, "ended_reason") == "REVIEWER_SATISFIED"
                    && Y.N(nv, "ended_at") == Y.N(ns, "custody.record_version"), "REVIEWER_SATISFIED does not end the validity in its own point", "D1-17");
            }

            if (!rvClosing)
            {
                return;
            }

            var loopRequests = ReviewerLoopRequests(ps);
            var pv = Y.M(pl, "action_validity");
            var bad = loopRequests.Count == 0 || loopRequests.Any(r => Y.S(r, "state") == "OPEN")
                      || loopRequests.Any(r => Y.L(r, "attempts").Cast<YamlMap>().Any(a => !Terminal.Contains(Y.S(a, "state"))));
            bad |= Y.S(ns, "orchestration.escalation.state") != None;
            var record = nclos.Count > 0 ? nclos[nclos.Count - 1] as YamlMap : null;
            var lastRequest = loopRequests.LastOrDefault();
            var expectedValidity = pv == null ? null : CloneMap(pv);
            if (pv != null && Y.S(pv, "state") == "ENDED" && Y.S(pv, "ended_reason") == "REVIEWER_SATISFIED" && Y.S(pl, "phase") == "REVIEWER_SATISFIED")
            {
                bad |= Y.L(ps, "orchestration.findings").Cast<YamlMap>().Any(f => Y.S(f, "issuer") == "REVIEWER" && Y.S(f, "severity") == "BLOCKING"
                                                                                  && OpenLineage.Contains(Y.S(f, "state")));
                bad |= record == null || record["closed_by"] != null;
            }
            else if (pv != null && (Y.S(pv, "state") == "ENDED" && Y.S(pv, "ended_reason") is "EXHAUSTED" or "EXPIRED" or "REVOKED" || Y.S(pv, "state") == "OPEN"))
            {
                var marker = "I62-REVIEWER-LOOP-CLOSE: " + Y.S(lastRequest, "logical_review_request_id");
                var block = record == null ? null : DecisionBlock(n.Tree, record["closed_by"], marker);
                bad |= block == null;
                if (Y.S(pv, "state") == "OPEN")
                {
                    bad |= block == null || !block.ContainsKey("I62-REVIEW-LOOP-REVOCATION");
                    expectedValidity!["state"] = "ENDED";
                    expectedValidity["ended_reason"] = "REVOKED";
                    expectedValidity["ended_at"] = Y.N(ns, "custody.record_version");
                    expectedValidity["ended_by"] = record?["closed_by"] is YamlMap cb ? CloneMap(cb) : null;
                    expectedValidity["ended_utc"] = record == null ? null : Y.Get((YamlMap?)record["validity"], "ended_utc");
                }
            }
            else
            {
                bad = true;
            }

            bad |= nl["authorization"] != null || nl["action_validity"] != null || nl["object"] != null || Y.S(nl, "phase") != None;
            bad |= !eq.Eq(Y.Get(ps, "orchestration.review_requests"), Y.Get(ns, "orchestration.review_requests"))
                   || !eq.Eq(Y.Get(ps, "orchestration.findings"), Y.Get(ns, "orchestration.findings"))
                   || !eq.Eq(Y.Get(ps, "orchestration.budgets"), Y.Get(ns, "orchestration.budgets"));
            bad |= record == null || Y.S(record, "last_request") != Y.S(lastRequest, "logical_review_request_id")
                   || !eq.Eq(record["authorization"], pl["authorization"]) || !eq.Eq(record["validity"], expectedValidity)
                   || Y.N(record, "closed_at") != Y.N(ns, "custody.record_version");
            add(!bad, "REVIEWER LOOP_CLOSED without its conditions", "D1-18");
        }

        private void PairArchitectClose(StatePoint p, StatePoint n, Dictionary<string, YamlMap> pe, Dictionary<string, YamlMap> ne, PairRefs eq,
            Action<bool, string, string> add)
        {
            var ps = p.State;
            var ns = n.State;
            var pl = Y.M(ps, "orchestration.loop")!;
            var nl = Y.M(ns, "orchestration.loop")!;
            var iid = Y.S(pl, "instance_id")!;
            var ep = pe.TryGetValue(iid, out var x) ? x : null;
            var en = ne.TryGetValue(iid, out var y) ? y : null;
            var reqs = Y.L(ps, "orchestration.review_requests").Cast<YamlMap>().Where(r => Y.S(r, "loop_instance_id") == iid).ToList();
            var bad = reqs.Any(r => Y.S(r, "state") == "OPEN") || reqs.Any(r => Y.L(r, "attempts").Cast<YamlMap>().Any(a => !Terminal.Contains(Y.S(a, "state"))));
            bad |= Y.S(ns, "orchestration.escalation.state") != None || ep == null || en == null || Y.N(en, "closed_at") != Y.N(ns, "custody.record_version");
            var pv = Y.M(pl, "action_validity");
            var needDecision = true;
            if (!bad && pv != null && Y.S(pv, "state") == "ENDED" && Y.S(pv, "ended_reason") is "ARCHITECT_SATISFIED" or "EXHAUSTED" or "EXPIRED" or "REVOKED")
            {
                needDecision = Y.S(pv, "ended_reason") != "ARCHITECT_SATISFIED";
                bad |= !eq.Eq(Y.Get(ep, "authorizations"), Y.Get(en, "authorizations"));
                bad |= !needDecision && en!["closed_by"] != null;
            }
            else if (!bad && pv != null && Y.S(pv, "state") == "OPEN")
            {
                var block = DecisionBlock(n.Tree, en!["closed_by"], "I62-REVIEW-LOOP-CLOSE: " + iid);
                bad |= block == null || !block.TryGetValue("I62-REVIEW-LOOP-REVOCATION", out var revoked) || revoked != Y.S(pv, "authorization_id");
                var pa = Y.L(ep, "authorizations").Cast<YamlMap>().ToList();
                var na = Y.L(en, "authorizations").Cast<YamlMap>().ToList();
                bad |= na.Count != pa.Count || Y.S(na[na.Count - 1], "state") != "ENDED" || Y.S(na[na.Count - 1], "ended_reason") != "REVOKED"
                       || !YamlSubset.DeepEquals(na[na.Count - 1]["ended_by"], en["closed_by"]);
            }
            else
            {
                bad = true;
            }

            if (!bad && needDecision)
            {
                bad |= DecisionBlock(n.Tree, en!["closed_by"], "I62-REVIEW-LOOP-CLOSE: " + iid) == null;
            }

            bad |= nl["authorization"] != null || nl["action_validity"] != null || nl["object"] != null || Y.S(nl, "phase") != None;
            bad |= !eq.Eq(Y.Get(ps, "orchestration.review_requests"), Y.Get(ns, "orchestration.review_requests"))
                   || !eq.Eq(Y.Get(ps, "orchestration.findings"), Y.Get(ns, "orchestration.findings"));
            bad |= ep != null && en != null && new[] { "review_rounds", "logical_requests", "architect_launches", "correction_rounds" }.Any(k => Y.N(en, k) != Y.N(ep, k));
            add(!bad, "LOOP_CLOSED of " + iid + " without its conditions", "D1-9");
        }

        private void PairCounterGrowth(StatePoint p, StatePoint n, Action<bool, string, string> add)
        {
            var ps = p.State;
            var ns = n.State;
            var pAtt = Attempts(ps).ToDictionary(x => Y.S(x.Request, "logical_review_request_id") + "/" + Y.N(x.Attempt, "attempt_seq"), x => x.Attempt);
            var newly = Attempts(ns).Where(x => x.Attempt["reserved_at"] != null
                                                 && (!pAtt.TryGetValue(Y.S(x.Request, "logical_review_request_id") + "/" + Y.N(x.Attempt, "attempt_seq"), out var b)
                                                     || b["reserved_at"] == null)).ToList();
            var pReqs = Y.L(ps, "orchestration.review_requests").Cast<YamlMap>().ToList();
            foreach (var (key, budgetOf) in BudgetSets(ns))
            {
                var pb = BudgetSets(ps).FirstOrDefault(x => x.Key == key).Budget;
                var mine = newly.Where(x => (Y.S(x.Request, "loop_instance_id") ?? "budgets") == key).ToList();
                var dl = (Y.N(budgetOf, "architect_launches") ?? 0) - (Y.N(pb, "architect_launches") ?? 0);
                var dr = (Y.N(budgetOf, "logical_requests") ?? 0) - (Y.N(pb, "logical_requests") ?? 0);
                var dv = (Y.N(budgetOf, "review_rounds") ?? 0) - (Y.N(pb, "review_rounds") ?? 0);
                var firstAttempts = mine.Where(x => Y.N(x.Attempt, "attempt_seq") == 1).ToList();
                var reservedRounds = pReqs.Where(r => (Y.S(r, "loop_instance_id") ?? "budgets") == key && Y.L(r, "attempts").Cast<YamlMap>().Any(a => a["reserved_at"] != null))
                    .Select(r => Y.N(r, "round")).ToHashSet();
                var newRounds = firstAttempts.Select(x => Y.N(x.Request, "round")).Where(rd => !reservedRounds.Contains(rd)).Distinct().Count();
                add(dl == mine.Count, key + ": architect_launches does not grow exactly with the new reservations", string.Empty);
                add(dr == firstAttempts.Count, key + ": logical_requests does not grow exactly with the first reservation of each request", string.Empty);
                add(dv == newRounds, key + ": review_rounds does not grow exactly with the first reservation on a new version", string.Empty);
            }
        }

        private static IEnumerable<(string Key, YamlMap Budget)> BudgetSets(YamlMap s)
        {
            foreach (var e in Y.L(s, "orchestration.architect_budgets").Cast<YamlMap>())
            {
                yield return (Y.S(e, "loop_instance_id")!, e);
            }

            yield return ("budgets", Y.M(s, "orchestration.budgets")!);
        }
    }
}
