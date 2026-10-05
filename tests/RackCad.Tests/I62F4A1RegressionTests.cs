#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Nodes;
using Xunit;
using static RackCad.Tests.OrchestrationSamples;
using static RackCad.Tests.StateV2Samples;
using Hx = RackCad.Tests.I62F4HistoryInvariantTests;
using O = RackCad.Tests.I62F4OrchestrationValidatorTests;

namespace RackCad.Tests
{
    /// <summary>
    /// I-62 F4 (slice F4-H), the A-1 regression coverage ordered by the Coordinator verdict on A-1: every trace of the A-1 counterexample harness
    /// (<c>docs/automation/evidence/I-62-A1/a1-counterexamples-result.json</c>) for A62-A1-01..06, OBS-A1-01, A62-A1R-01..03, A62-A1S-01..02 and A62-A1T-01
    /// is replayed on the production validator of state/v2 (file, pair and history invariants) under its harness name and with the harness verdict. A
    /// VALID trace passes every point, pair and history check; an INVALID trace fails its defective step by the invariant and A-1 clause that guard it
    /// and by nothing outside the declared set. Traces that already exist as named negatives of the C-38 list are reused by name. The two traces that
    /// are not replayed are listed with the production test that proves them (real Git clean clones) or the reason (a counterexample against the
    /// literal V14 text). A62-A1U-O1, an F4 obligation rather than a harness finding, adds the fabricated composite map entry.
    /// </summary>
    public class I62F4A1RegressionTests
    {
        private const string HarnessResult = "docs/automation/evidence/I-62-A1/a1-counterexamples-result.json";
        private const string Run1 = "R20261006T040404Z-0001";
        private const string Run2 = "R20261006T050505Z-0002";
        private const string R0 = "L20261007T000000Z-r000";
        private const string R2 = "L20261007T020202Z-r002";
        private static readonly StateV2Validator V = new StateV2Validator();
        private static readonly string Kx1 = Hx.H("6a"), Kx2 = Hx.H("6b"), Ky1 = Hx.H("6c"), Ky2 = Hx.H("6d"), C9 = Hx.H("c9");
        private const string OtherBlob = "b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0b0";

        public static readonly string[] RequiredFindings =
        {
            "A62-A1-01", "A62-A1-02", "A62-A1-03", "A62-A1-04", "A62-A1-05", "A62-A1-06", "OBS-A1-01", "A62-A1R-01", "A62-A1R-02", "A62-A1R-03", "A62-A1S-01",
            "A62-A1S-02", "A62-A1T-01",
        };

        /// <summary>Harness traces of the required findings that are not replayed here, with the production test or the reason that covers them.</summary>
        public static readonly IReadOnlyDictionary<string, string> CoveredElsewhere = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["a62-a1-06-sucesor-en-clon-limpio"] = "I62F4RebaseChainTests.I62_C15_E_F_TwoRealRebasesResolveInACleanSuccessorCloneOnlyFromDurableCustody and "
                + "I62F4CustodyMcTests (real Git, --no-local clean clones without the original objects)",
            ["fc06-literal-referencias-tras-rebase"] = "a counterexample against the literal V14 I-S18 (the motivation of D2-11); under A-1 the same state is "
                + "a62-a1-06-referencias-tras-un-rebase, replayed here as VALID",
        };

        // ------------------------------------------------------------------ the replayed traces

        public static IEnumerable<object[]> Traces()
        {
            // ---- A62-A1-01: per-loop identity, substitution inside the loop's entry
            yield return Existing("A62-A1-01", "a62-a1-01-instance-id-distinto-del-QU-de-apertura", "instance_id other than ARL-<record_version>");
            yield return Existing("A62-A1-01", "a62-a1-01-solicitud-cambia-de-bucle", "request that changes loop");
            yield return Valid("A62-A1-01", "a62-a1-01-sustitucion-continua-la-misma-entrada", v =>
            {
                var p = F8Step(12);
                return Seq(v, p, O.Substitute(p, "RLA-2"));
            });
            yield return Existing("A62-A1-01", "a62-a1-01-sustitucion-crea-entrada-nueva", "continuation that creates an entry");
            yield return Invalid("A62-A1-01", "a62-a1-01-sustitucion-reinicia-contadores", v =>
            {
                var p = F8Step(12);
                var n = O.Substitute(p, "RLA-2");
                var e = Entry0(n);
                foreach (var k in new[] { "review_rounds", "logical_requests", "correction_rounds" })
                {
                    e[k] = 0L;
                }

                return Step(v, p, n);
            }, new[] { "I-P13/D1-8", "I-P13/D1-6" }, "I-S18", "I-P13");
            yield return Invalid("A62-A1-01", "a62-a1-01-sustitucion-sube-topes", v =>
            {
                var p = F8Step(12);
                var n = O.Substitute(p, "RLA-2");
                ((YamlMap)Entry0(n)["caps"]!)["review_rounds"] = 4L;
                return Step(v, p, n);
            }, new[] { "I-S18/D1-2", "I-P13/D1-6" });
            yield return Existing("A62-A1-01", "fc01-enmendado-reinicia-A1", "counter reset");

            // ---- A62-A1-02: closure (LOOP_CLOSED) and continuation of an ended validity
            yield return Existing("A62-A1-02", "a62-a1-02-accion-nueva-con-vigencia-terminada", "new action with the validity ended");
            yield return Invalid("A62-A1-02", "a62-a1-02-cierre-con-escalada-sin-resolver", v =>
            {
                var p = F8Step(17);
                var n = O.CloseFromSatisfied(p);
                var escalation = Y.M(n.State, "orchestration.escalation")!;
                escalation["state"] = "OWNER";
                escalation["reason"] = "P-18";
                escalation["required_decision"] = "ampliar el presupuesto";
                Orch(n)["next_action"] = NextAction("OWNER", "DECIDE", null);
                return Step(v, p, n);
            }, new[] { "I-P13/D1-9" });
            yield return Valid("A62-A1-02", "a62-a1-02-cierre-que-revoca-la-vigente", v =>
            {
                var p = F8Step(12);
                return Seq(v, p, O.CloseWithDecision(p, revoke: true));
            });
            yield return Existing("A62-A1-02", "a62-a1-02-cierre-sin-decision", "LOOP_CLOSED after EXPIRED without the decision");
            yield return Invalid("A62-A1-02", "a62-a1-02-continuacion-crea-entrada", v =>
            {
                var p = O.Expire(F8Step(12));
                var n = O.Substitute(p, "RLA-2");
                var entry = Clone(Entry0(n));
                entry["loop_instance_id"] = "ARL-99";
                entry["closed_at"] = Rv(n);
                Y.L(n.State, "orchestration.architect_budgets").Add(entry);
                return Step(v, p, n);
            }, new[] { "I-P13/D1-7" }, "I-P13", "I-S18");
            yield return Valid("A62-A1-02", "a62-a1-02-expirada-y-cerrada", v =>
            {
                var expired = O.Expire(F8Step(12));
                return Seq(v, F8Step(12), expired, O.CloseWithDecision(expired, revoke: false));
            });
            yield return Valid("A62-A1-02", "a62-a1-02-expirada-y-continuada", v =>
            {
                var expired = O.Expire(F8Step(12));
                return Seq(v, F8Step(12), expired, O.Substitute(expired, "RLA-2"));
            });
            yield return Existing("A62-A1-02", "a62-a1-02-reescribe-EXPIRED-como-SUPERSEDED", "EXPIRED rewritten as SUPERSEDED in a continuation");
            yield return Valid("A62-A1-02", "a62-a1-02-revocada-y-cerrada", v =>
            {
                var p = F8Step(12);
                var revoked = Next(p);
                var d = WriteDecisions(revoked, new[] { G0Entry, RlaEntry, DesignationEntry, ArchitectRevocation });
                End((YamlMap)Loop(revoked)["action_validity"]!, "REVOKED", Rv(revoked), d);
                End((YamlMap)Y.L(Entry0(revoked), "authorizations")[^1]!, "REVOKED", Rv(revoked), d);
                var closed = Next(revoked);
                var cd = WriteDecisions(closed, new[] { G0Entry, RlaEntry, DesignationEntry, ArchitectRevocation, ArchitectClose });
                return Seq(v, p, revoked, O.Blank(closed, closedBy: cd));
            });
            yield return Valid("A62-A1-02", "a62-a1-o3-bucle-nuevo-hereda-linajes-abiertos", v =>
            {
                // Closed after EXPIRED with LIN-1 and LIN-2 still OPEN; the next loop's first reservation carries them in OpenFindings (D1-15).
                var expired = O.Expire(F8Step(12));
                var closed = O.CloseWithDecision(expired, revoke: false);
                var reopened = OpenArchitectLoop(closed, "RLA-2", new[] { G0Entry, RlaEntry, DesignationEntry, ArchitectClose, Rla("RLA-2") }, new[] { "LIN-1", "LIN-2" });
                return Seq(v, F8Step(12), expired, closed, reopened);
            });
            yield return Valid("A62-A1-02", "fc01-enmendado-cerrar-y-abrir-con-A2", v =>
            {
                var closed = O.CloseFromSatisfied(F8Step(17));
                return Seq(v, F8Step(17), closed, O.OpenSecondLoop(closed, "RLA-2"));
            });
            yield return Existing("A62-A1-02", "fc01-enmendado-cierra-con-solicitud-abierta", "LOOP_CLOSED with an OPEN request and the validity OPEN without a decision");

            // ---- A62-A1-03: the budgets of V14 belong to the REVIEWER; the Architect has its entry
            yield return Invalid("A62-A1-03", "a62-a1-03-architect-sin-entrada", v =>
            {
                var (p, n) = O.Mutate(12, 13, x => Y.L(x.State, "orchestration.architect_budgets").Clear());
                return Step(v, p, n);
            }, new[] { "I-S18/D1-5", "I-P13/D1-6" }, "I-S18", "I-P13");
            yield return Existing("A62-A1-03", "a62-a1-03-budgets-de-V14-cuenta-una-solicitud-del-architect", "budgets counting an Architect request");
            yield return Valid("A62-A1-03", "a62-a1-03-reviewer-autorizado-por-contrato-de-gate", v => Seq(v, ReviewerSamples.Loop().ToArray()));
            yield return Existing("A62-A1-03", "a62-a1-03-reviewer-con-identidad-de-architect", "REVIEWER with a loop identity of the Architect");

            // ---- A62-A1-04: a LAUNCHING attempt across a reconciliation
            yield return Invalid("A62-A1-04", "a62-a1-04-LAUNCHING-reescrito-en-la-reconciliacion", v =>
            {
                var (s3, n1, _) = Hx.Reconciled();
                var a = AttemptOf(n1, L1, 1);
                var json = Orchestration.Invocation(n1, a)!.DeepClone().AsObject();
                json["Target"] = new JsonObject { ["commit"] = Hx.C1i, ["path"] = X, ["blob"] = B1 };
                a["invocation"] = Tree(n1).PutJson("docs/automation/evidence/I-99-agent/review/" + L1 + "/1/invocation-rewritten.json", json);
                return HStep(v, Graph(s3), (s3, Hx.K3), (n1, Hx.Kn1));
            }, new[] { "I-P13/D2-6" });
            yield return Invalid("A62-A1-04", "a62-a1-04-LAUNCHING-sin-imagen-en-el-mapa", v =>
            {
                var s3 = F8Step(3);
                var n1 = Hx.Reconcile(s3, Run1, Sha3, Hx.M1, Hx.K3, Hx.K3i, (Hx.K3, Hx.K3i, Hx.Pk));
                return HStep(v, Graph(s3), (s3, Hx.K3), (n1, Hx.Kn1));
            }, new[] { "I-P13/D2-2" }, "I-H02/D2-3", "I-S18/D2-11");
            yield return Valid("A62-A1-04", "a62-a1-04-LAUNCHING-tras-rebase-arranco", v =>
            {
                var (s3, n1, _) = Hx.Reconciled();
                return HSeq(v, Graph(s3), (s3, Hx.K3), (n1, Hx.Kn1), (Launched(n1), Kx1));
            });
            yield return Valid("A62-A1-04", "a62-a1-04-LAUNCHING-tras-rebase-incierto", v =>
            {
                var (s3, n1, _) = Hx.Reconciled();
                return HSeq(v, Graph(s3), (s3, Hx.K3), (n1, Hx.Kn1), (Uncertain(n1), Kx1));
            });
            yield return Valid("A62-A1-04", "a62-a1-04-LAUNCHING-tras-rebase-no-arranco", v =>
            {
                var (s3, n1, _) = Hx.Reconciled();
                return HSeq(v, Graph(s3), (s3, Hx.K3), (n1, Hx.Kn1), (NotStarted(n1, Obj(Hx.C1i, X, B1)), Kx1));
            });
            yield return Invalid("A62-A1-04", "a62-a1-04-no-arranco-sin-replanificar", v =>
            {
                var (s3, n1, _) = Hx.Reconciled();
                return HStep(v, Graph(s3), (n1, Hx.Kn1), (NotStarted(n1, Obj(C1, X, B1)), Kx1));
            }, new[] { "I-P13/D2-6", "I-H02/D2-3", "I-S18" });

            // ---- A62-A1-05: EquivalentReviewedObject at the ingestion after a rebase
            yield return Invalid("A62-A1-05", "a62-a1-05-ingesta-con-imagen-no-probada", v =>
            {
                var t = IngestAfterRebase(Obj(C9, X, B1));
                return Step(v, t[^2], t[^1]);
            }, new[] { "I-P13/D2-12" });
            yield return Invalid("A62-A1-05", "a62-a1-05-ingesta-con-otro-blob", v =>
            {
                var t = IngestAfterRebase(Obj(C1, X, OtherBlob));
                return Step(v, t[^2], t[^1]);
            }, new[] { "I-P13/D2-12" }, "I-S18");
            yield return Valid("A62-A1-05", "a62-a1-05-ingesta-de-un-LAUNCHED-tras-rebase", v => Seq(v, IngestAfterRebase(Obj(C1, X, B1)).ToArray()));
            yield return Valid("A62-A1-05", "fc02-enmendado-LAUNCHED-conserva-su-Target", v =>
            {
                var s4 = F8Step(4);
                return Seq(v, s4, Hx.Reconcile(s4, Run1, Sha3, Hx.M1, Hx.K3, Hx.K3i, (C1, Hx.C1i, Hx.P1), (Hx.K3, Hx.K3i, Hx.Pk)));
            });

            // ---- A62-A1-06: branch references after one and two rebases
            yield return Invalid("A62-A1-06", "a62-a1-06-blob-cambiado", v =>
            {
                var (s3, _, n2) = Hx.Reconciled();
                return v.ValidateHistory(n2, Graph(s3, imageBlob: Hx.Other), Hx.Kn2);
            }, new[] { "I-S18/D2-11" });
            yield return Invalid("A62-A1-06", "a62-a1-06-historia-cambia-sin-reconciliacion", v =>
            {
                var n = Copy(Point(7));
                Y.L(n.State, "custody.rebase_history").Clear();
                return Step(v, Point(6), n);
            }, new[] { "I-P11" }, "I-S17");
            yield return Invalid("A62-A1-06", "a62-a1-06-historia-de-mapas-sin-last_rebase-al-final", v =>
            {
                var n = Copy(Point(6));
                Y.L(n.State, "custody.rebase_history").Clear();
                return v.ValidateFile(n);
            }, new[] { "I-S17" });
            yield return Invalid("A62-A1-06", "a62-a1-06-mapa-intermedio-ausente", v =>
            {
                var (s3, _, n2) = Hx.Reconciled();
                var missing = Copy(n2);
                Hx.DropFirstMap(missing);
                return v.ValidateHistory(missing, Graph(s3), Hx.Kn2);
            }, new[] { "I-S18/D2-11" });
            yield return Valid("A62-A1-06", "a62-a1-06-referencias-tras-dos-rebases", v =>
            {
                var (s3, n1, n2) = Hx.Reconciled();
                return HSeq(v, Graph(s3), (s3, Hx.K3), (n1, Hx.Kn1), (n2, Hx.Kn2));
            });
            yield return Valid("A62-A1-06", "a62-a1-06-referencias-tras-un-rebase", v =>
            {
                var (s3, n1, _) = Hx.Reconciled();
                return HSeq(v, Graph(s3), (s3, Hx.K3), (n1, Hx.Kn1));
            });
            yield return Invalid("A62-A1-06", "a62-a1-06-replanificada-con-referencias-originales", v =>
            {
                var s2 = F8Step(2);
                var kept = Hx.Reconcile(s2, Run1, Sha3, Hx.M1, Hx.K3, Hx.K3i, (C1, Hx.C1i, Hx.P1), (Hx.K3, Hx.K3i, Hx.Pk));
                FaithfulReplan(kept, L1, 1, Obj(C1, X, B1));
                return HStep(v, Graph(s2), (s2, Hx.K3), (kept, Hx.Kn1));
            }, new[] { "I-P13/D2-11", "I-P13/D2-2" }, "I-H02/D2-3", "I-S18", "I-P10");
            yield return Valid("A62-A1-06", "fc02-enmendado-reconcilia", v =>
            {
                var s2 = F8Step(2);
                var n = Hx.Reconcile(s2, Run1, Sha3, Hx.M1, Hx.K3, Hx.K3i, (C1, Hx.C1i, Hx.P1), (Hx.K3, Hx.K3i, Hx.Pk));
                FaithfulReplan(n, L1, 1, Obj(Hx.C1i, X, B1));
                return HSeq(v, Graph(s2), (s2, Hx.K3), (n, Hx.Kn1));
            });

            // ---- OBS-A1-01: the REVIEWER loop (R1..R10)
            yield return Existing("OBS-A1-01", "a62-a1-obs01-r1-NO_FINDINGS-no-es-ARCHITECT_SATISFIED", "REVIEWER with ARCHITECT_SATISFIED");
            yield return Valid("OBS-A1-01", "a62-a1-obs01-r1-reviewer-NO_FINDINGS-se-cierra", v =>
            {
                var loop = ReviewerSamples.Loop();
                return Seq(v, loop[3], loop[4], loop[5]);
            });
            yield return Invalid("OBS-A1-01", "a62-a1-obs01-r10-LOOP_CLOSED-no-se-aplica-a-EXECUTION", v =>
            {
                var ex = Execution(null);
                var n = Next(ex);
                var loop = Loop(n);
                loop["type"] = "NONE";
                loop["phase"] = "NONE";
                loop["authorization"] = null;
                loop["action_validity"] = null;
                return Step(v, ex, n);
            }, new[] { "I-P13/D1-13" }, "I-P13");
            yield return Invalid("OBS-A1-01", "a62-a1-obs01-r10-execution-con-identidad-de-architect", v =>
            {
                var ex = Execution(null);
                Loop(ex)["instance_id"] = "ARL-" + Rv(ex);
                return v.ValidateFile(ex);
            }, new[] { "I-S18/D1-1" });
            yield return Valid("OBS-A1-01", "a62-a1-obs01-r10-execution-sin-cambio", v =>
            {
                var ex = Execution(null);
                return Seq(v, ex, Next(ex));
            });
            yield return Valid("OBS-A1-01", "a62-a1-obs01-r2-solo-ADVISORY-se-cierra", v =>
            {
                var loop = ReviewerSamples.Loop(("R-01", "ADVISORY"));
                return Seq(v, loop.Skip(1).ToArray());
            });
            yield return Existing("OBS-A1-01", "a62-a1-obs01-r3-BLOCKING-abierto-impide-REVIEWER_SATISFIED", "REVIEWER_SATISFIED with a BLOCKING open");
            yield return Invalid("OBS-A1-01", "a62-a1-obs01-r3-BLOCKING-abierto-impide-el-cierre", v =>
            {
                var loop = ReviewerSamples.Loop();
                var (p, n) = (loop[4], loop[5]);
                foreach (var x in new[] { p, n })
                {
                    Y.L(x.State, "orchestration.findings").Add(ReviewerLineage(x, "LIN-R0", "R-00", "BLOCKING", "OPEN"));
                }

                return Step(v, p, n);
            }, new[] { "I-P13/D1-18" });
            yield return Existing("OBS-A1-01", "a62-a1-obs01-r4-reviewer-cierra-un-linaje-del-architect", "closure by a reviewer-result/v1");
            yield return Invalid("OBS-A1-01", "a62-a1-obs01-r5-reviewer-con-architect_budgets", v =>
            {
                var loop = ReviewerSamples.Loop();
                var n = loop[1];
                Y.L(n.State, "orchestration.architect_budgets").Add(Entry("ARL-" + Rv(n), (YamlMap)Loop(n)["authorization"]!, 1, 1, 1, new[] { (ReviewerSamples.R1, 0L) }, 0));
                return Step(v, loop[0], n);
            }, new[] { "I-S18/D1-5", "I-P13/D1-7" }, "I-S18", "I-P13");
            yield return Invalid("OBS-A1-01", "a62-a1-obs01-r7-EXPIRED-reescrito-como-SUPERSEDED", v =>
            {
                var expired = ReviewerEnd(ReviewerSamples.Loop()[1], "EXPIRED", null);
                var n = ReviewerCloseWith(expired, new[] { G0Entry, ReviewerClose });
                ((YamlMap)Last(n, "orchestration.reviewer_closures")["validity"]!)["ended_reason"] = "SUPERSEDED";
                return Step(v, expired, n);
            }, new[] { "I-P13/D1-18" });
            yield return Existing("OBS-A1-01", "a62-a1-obs01-r7-cierre-sin-decision", "REVIEWER closure (E) after EXPIRED without the decision");
            yield return Valid("OBS-A1-01", "a62-a1-obs01-r7-expired-se-cierra-conservando-el-motivo", v =>
            {
                var open = ReviewerSamples.Loop()[1];
                var expired = ReviewerEnd(open, "EXPIRED", null);
                return Seq(v, open, expired, ReviewerCloseWith(expired, new[] { G0Entry, ReviewerClose }));
            });
            yield return Valid("OBS-A1-01", "a62-a1-obs01-r7-revoked-se-cierra-conservando-el-motivo", v => Seq(v, ReviewerRevokedAndClosed().ToArray()));
            yield return Existing("OBS-A1-01", "a62-a1-obs01-r8-cierre-borra-historia", "REVIEWER closure that drops reviewer_closures history");
            yield return Invalid("OBS-A1-01", "a62-a1-obs01-r8-cierre-reinicia-budgets", v =>
            {
                var loop = ReviewerSamples.Loop();
                var n = loop[5];
                var b = Y.M(n.State, "orchestration.budgets")!;
                b["review_rounds"] = 0L;
                b["logical_requests"] = 0L;
                b["architect_launches"] = 0L;
                b["transport_reruns"] = L();
                return Step(v, loop[4], n);
            }, new[] { "I-P13/D1-18" }, "I-P13", "I-S18");
            yield return Valid("OBS-A1-01", "a62-a1-obs01-r9-architect-abre-tras-el-cierre-del-reviewer", v =>
            {
                var closed = ReviewerSamples.Loop()[^1];
                return Seq(v, closed, OpenArchitectLoop(closed, "RLA-1", new[] { G0Entry, RlaEntry, DesignationEntry }, new string[0]));
            });
            yield return Invalid("OBS-A1-01", "a62-a1-obs01-r9-sin-cierre-no-se-abre-otro-bucle", v =>
            {
                var satisfied = ReviewerSamples.Loop()[4];
                return Step(v, satisfied, OpenArchitectLoop(satisfied, "RLA-1", new[] { G0Entry, RlaEntry, DesignationEntry }, new string[0]));
            }, new[] { "I-P13/D1-7" }, "I-P13", "I-S18");

            // ---- A62-A1R-01: closure (E) after EXHAUSTED
            yield return Invalid("A62-A1R-01", "a62-a1r-01-exhausted-con-intento-vivo", v =>
            {
                var exhausted = Next(ReviewerSamples.Loop()[2]);
                End((YamlMap)Loop(exhausted)["action_validity"]!, "EXHAUSTED", Rv(exhausted), null);
                return Step(v, exhausted, ReviewerCloseWith(exhausted, new[] { G0Entry, ReviewerClose }));
            }, new[] { "I-P13/D1-18" });
            yield return Valid("A62-A1R-01", "a62-a1r-01-exhausted-se-cierra-conservando-el-motivo", v =>
            {
                var open = ReviewerSamples.Loop()[1];
                var exhausted = ReviewerEnd(open, "EXHAUSTED", null);
                return Seq(v, open, exhausted, ReviewerCloseWith(exhausted, new[] { G0Entry, ReviewerClose }));
            });
            yield return Invalid("A62-A1R-01", "a62-a1r-01-exhausted-sin-decision", v =>
            {
                var exhausted = ReviewerEnd(ReviewerSamples.Loop()[1], "EXHAUSTED", null);
                return Step(v, exhausted, ReviewerCloseWith(exhausted, null));
            }, new[] { "I-P13/D1-18" });
            yield return Invalid("A62-A1R-01", "a62-a1r-01-registro-finge-REVIEWER_SATISFIED", v =>
            {
                var exhausted = ReviewerEnd(ReviewerSamples.Loop()[1], "EXHAUSTED", null);
                var n = ReviewerCloseWith(exhausted, new[] { G0Entry, ReviewerClose });
                ((YamlMap)Last(n, "orchestration.reviewer_closures")["validity"]!)["ended_reason"] = "REVIEWER_SATISFIED";
                return Step(v, exhausted, n);
            }, new[] { "I-P13/D1-18" });

            // ---- A62-A1R-02: the rebase of loop.object for REVIEWER and EXECUTION
            yield return Valid("A62-A1R-02", "a62-a1r-02-execution-rebase-reconcilia", v =>
            {
                var ex = Execution(Obj(C1, X, B1));
                return HSeq(v, ExecutionGraph(), (ex, Hx.K3), (ReconcileR1(ex), Hx.Kn1));
            });
            yield return Invalid("A62-A1R-02", "a62-a1r-02-execution-rebase-sin-reconciliar", v =>
            {
                var n = ReconcileR1(Execution(Obj(C1, X, B1)));
                ((YamlMap)Loop(n)["object"]!)["commit"] = C1;
                return v.ValidateHistory(n, ExecutionGraph(), Hx.Kn1);
            }, new[] { "I-H02/D2-3" });
            yield return Valid("A62-A1R-02", "a62-a1r-02-reviewer-ingesta-tras-rebase", v => Seq(v, ReviewerIngestAfterRebase().ToArray()));
            yield return Invalid("A62-A1R-02", "a62-a1r-02-reviewer-rebase-loop-object-sin-reconciliar", v =>
            {
                var rv2 = ReviewerSamples.Loop()[2];
                var n = ReconcileR1(rv2);
                ((YamlMap)Loop(n)["object"]!)["commit"] = C1;
                return v.ValidateHistory(n, Graph(rv2), Hx.Kn1);
            }, new[] { "I-H02/D2-3" });
            yield return Valid("A62-A1R-02", "a62-a1r-02-reviewer-rebase-reconcilia", v =>
            {
                var rv2 = ReviewerSamples.Loop()[2];
                return HSeq(v, Graph(rv2), (rv2, Hx.K3), (ReconcileR1(rv2), Hx.Kn1));
            });

            // ---- A62-A1R-03: no resurrection of an ended REVIEWER authority
            yield return Valid("A62-A1R-03", "a62-a1r-03-nueva-autoridad-hereda-BLOCKING", v => Seq(v, NewAuthorityInheritsBlocking().ToArray()));
            yield return Invalid("A62-A1R-03", "a62-a1r-03-reapertura-tras-E-revocada", v =>
            {
                var closed = ReviewerRevokedAndClosed()[^1];
                return Step(v, closed, O.ReviewerReopen(closed, newContract: false));
            }, new[] { "I-P13/D1-20" });
            yield return Existing("A62-A1R-03", "a62-a1r-03-reapertura-tras-S", "reopening with an ended REVIEWER authority");
            yield return Invalid("A62-A1R-03", "a62-a1r-03-reapertura-tras-rebase", v =>
            {
                var reconciled = Hx.Reconcile(ReviewerSamples.Loop()[^1], Run1, Sha3, Hx.M1, Hx.K3, Hx.K3i, (Hx.K3, Hx.K3i, Hx.Pk));
                return Step(v, reconciled, O.ReviewerReopen(reconciled, newContract: false));
            }, new[] { "I-P13/D1-20" });

            // ---- A62-A1S-01: REVIEWER_SATISFIED needs every request of the loop terminal and no open BLOCKING in the unit
            yield return Invalid("A62-A1S-01", "a62-a1s-01-continuacion-satisfecha-con-STILL_OPEN-heredado", v =>
            {
                var loop = ReviewerSamples.Loop();
                var (p, n) = (loop[3], loop[4]);
                foreach (var x in new[] { p, n })
                {
                    Y.L(x.State, "orchestration.findings").Add(ReviewerLineage(x, "LIN-R0", "R-00", "BLOCKING", "STILL_OPEN"));
                }

                return Step(v, p, n);
            }, new[] { "I-S18/D1-17" });
            yield return Invalid("A62-A1S-01", "a62-a1s-01-sustitucion-con-intento-vivo-de-la-autoridad-sustituida", v =>
            {
                var loop = ReviewerSamples.Loop();
                var (p, n) = (loop[3], loop[4]);
                foreach (var x in new[] { p, n })
                {
                    AddLiveEarlierRequest(x);
                }

                return Step(v, p, n);
            }, new[] { "I-S18/D1-17" });

            // ---- A62-A1S-02: the REVIEWER correction cycle CORRECTING → PUBLISHED with a new request on the corrected object
            yield return Invalid("A62-A1S-02", "a62-a1s-02-re-revision-sobre-el-objeto-anterior", v =>
            {
                var t = ReviewerCorrection(rereviewOn: Obj(C1, X, B1));
                return Step(v, t[^2], t[^1]);
            }, new[] { "I-P13/A62-A1S-02" });
            yield return Valid("A62-A1S-02", "a62-a1s-02-reviewer-corrige-y-re-revisa-la-correccion", v => Seq(v, ReviewerCorrection(rereviewOn: Obj(C2, X, B2)).Skip(1).ToArray()));

            // ---- A62-A1T-01: the Target of a LAUNCHING attempt across two rebases (T1..T7)
            yield return Valid("A62-A1T-01", "a62-a1t-01-t1-un-rebase-sin-cambio", v =>
            {
                var (s3, n1, _) = Hx.Reconciled();
                return HSeq(v, Graph(s3), (s3, Hx.K3), (n1, Hx.Kn1));
            });
            yield return Valid("A62-A1T-01", "a62-a1t-01-t2-dos-rebases-resuelve-por-la-cadena", v =>
            {
                var (s3, n1, n2) = Hx.Reconciled();
                return HSeq(v, Graph(s3), (n1, Hx.Kn1), (n2, Hx.Kn2));
            });
            yield return Invalid("A62-A1T-01", "a62-a1t-01-t3a-cadena-sin-el-eslabon-de-X", v =>
            {
                var (s3, n1, _) = Hx.Reconciled();
                var n2 = Hx.Reconcile(n1, Run2, Hx.M1, Hx.M2, Hx.Kn1, Hx.Kn1i, (Hx.K3i, Hx.K3ii, Hx.Pk), (Hx.Kn1, Hx.Kn1i, Hx.Pn));
                return HStep(v, Graph(s3), (n1, Hx.Kn1), (n2, Hx.Kn2));
            }, new[] { "I-P13/D2-2" }, "I-H02/D2-3", "I-S18/D2-11");
            yield return Invalid("A62-A1T-01", "a62-a1t-01-t3b-M1-ausente-de-la-historia", v =>
            {
                var (s3, n1, n2) = Hx.Reconciled();
                var lastOnly = Copy(n2);
                Hx.DropFirstMap(lastOnly);
                return HStep(v, Graph(s3), (n1, Hx.Kn1), (lastOnly, Hx.Kn2));
            }, new[] { "I-P13/D2-2", "I-S18/D2-11" }, "I-P11");
            yield return Invalid("A62-A1T-01", "a62-a1t-01-t4-cadena-completa-con-otro-contenido", v =>
            {
                var (s3, n1, n2) = Hx.Reconciled();
                return HStep(v, Graph(s3, imageBlob: Hx.Other), (n1, Hx.Kn1), (n2, Hx.Kn2));
            }, new[] { "I-P13/D2-2" }, "I-S18/D2-11");
            yield return Invalid("A62-A1T-01", "a62-a1t-01-t5a-reescribe-la-invocacion", v => T5((n, a) =>
            {
                var json = Orchestration.Invocation(n, a)!.DeepClone().AsObject();
                json["Target"] = new JsonObject { ["commit"] = Hx.C1ii, ["path"] = X, ["blob"] = B1 };
                a["invocation"] = Tree(n).PutJson("docs/automation/evidence/I-99-agent/review/" + L1 + "/1/invocation-t5a.json", json);
            }, v), new[] { "I-P13/D2-6" });
            yield return Invalid("A62-A1T-01", "a62-a1t-01-t5b-cambia-el-RunId", v => T5((n, a) => a["run_id"] = "R20261006T060606Z-0003", v), new[] { "I-P13/D2-8" });
            yield return Invalid("A62-A1T-01", "a62-a1t-01-t5c-cambia-la-reserva", v => T5((n, a) => a["reserved_at"] = 7L, v), new[] { "I-P13/D2-8" }, "I-P13");
            yield return Invalid("A62-A1T-01", "a62-a1t-01-t5d-cambia-el-presupuesto", v => T5((n, a) => Entry0(n)["review_rounds"] = 2L, v), new[] { "I-P13/D2-8" }, "I-S18", "I-P05/D2-7", "I-P13");
            yield return Invalid("A62-A1T-01", "a62-a1t-01-t5e-cambia-el-estado-del-intento", v => T5((n, a) =>
            {
                a["state"] = "LAUNCHED";
                a["launch_evidence"] = Tree(n).Put("docs/automation/evidence/I-99-agent/review/" + L1 + "/1/launch.json", "{\"Schema\": \"rackcad-relay-record/v2\"}\n");
            }, v), new[] { "I-P13/D2-8" });
            yield return Invalid("A62-A1T-01", "a62-a1t-01-t5f-cambia-los-linajes", v => T5((n, a) =>
                Y.L(n.State, "orchestration.findings").Add(Lineage("LIN-9", "A62-X-09", (YamlMap)Loop(n)["authorization"]!)), v), new[] { "I-P13/D2-8" }, "I-P05/D2-7", "I-P13", "I-S18");
            yield return Invalid("A62-A1T-01", "a62-a1t-01-t5g-cambia-el-BudgetSnapshot", v => T5((n, a) =>
            {
                var json = Orchestration.Invocation(n, a)!.DeepClone().AsObject();
                json["BudgetSnapshot"]!["ReviewRounds"] = 2;
                a["invocation"] = Tree(n).PutJson("docs/automation/evidence/I-99-agent/review/" + L1 + "/1/invocation-t5g.json", json);
            }, v), new[] { "I-P13/D2-6" });
            yield return Valid("A62-A1T-01", "a62-a1t-01-t6-no-arranco-replanifica-sobre-X2", v =>
            {
                var (s3, n1, n2) = Hx.Reconciled();
                return HSeq(v, Graph(s3), (n1, Hx.Kn1), (n2, Hx.Kn2), (NotStarted(n2, Obj(Hx.C1ii, X, B1)), Ky1));
            });
            yield return Valid("A62-A1T-01", "a62-a1t-01-t7-arranque-desconocido-LAUNCH_UNCERTAIN", v =>
            {
                var (s3, n1, n2) = Hx.Reconciled();
                return HSeq(v, Graph(s3), (n1, Hx.Kn1), (n2, Hx.Kn2), (Uncertain(n2), Ky1));
            });
            yield return Invalid("A62-A1T-01", "a62-a1t-01-t7a-prueba-de-no-arranque-fabricada", v =>
            {
                var (s3, _, n2) = Hx.Reconciled();
                var replanned = NotStarted(n2, Obj(Hx.C1ii, X, B1));
                AttemptOf(replanned, L1, 1)["not_started_evidence"] = null;
                return HStep(v, Graph(s3), (n2, Hx.Kn2), (replanned, Ky1));
            }, new[] { "I-P13" });
            yield return Invalid("A62-A1T-01", "a62-a1t-01-t7b-relanza-el-incierto", v =>
            {
                var (s3, _, n2) = Hx.Reconciled();
                var uncertain = Uncertain(n2);
                var relaunched = Next(uncertain);
                var a = AttemptOf(relaunched, L1, 1);
                a["state"] = "LAUNCHING";
                a["outcome"] = null;
                Loop(relaunched)["phase"] = "ARCHITECT_INVOKED";
                return HStep(v, Graph(s3), (uncertain, Ky1), (relaunched, Ky2));
            }, new[] { "I-P13/D2-6" }, "I-P13");

            // ---- A62-A1U-O1 (F4 obligation): a fabricated composite entry X → X'' never replaces the missing first map
            yield return Invalid("A62-A1U-O1", "f4-a62-a1u-o1-entrada-compuesta-fabricada", v =>
            {
                var (s3, n1, _) = Hx.Reconciled();
                var fabricated = Hx.Reconcile(n1, Run2, Hx.M1, Hx.M2, Hx.Kn1, Hx.Kn1i, (C1, Hx.C1ii, Hx.P1), (Hx.K3i, Hx.K3ii, Hx.Pk), (Hx.Kn1, Hx.Kn1i, Hx.Pn));
                Hx.DropFirstMap(fabricated);
                return HStep(v, Graph(s3), (n1, Hx.Kn1), (fabricated, Hx.Kn2));
            }, new[] { "I-P13/D2-2" }, "I-P10", "I-P11", "I-S18/D2-11", "I-H02/D2-3", "I-P13");
        }

        // ------------------------------------------------------------------ the checks

        [Theory]
        [MemberData(nameof(Traces))]
        public void I62_A1_EachHarnessTraceKeepsItsVerdictOnTheProductionValidator(string finding, string trace, Func<StateV2Validator, IEnumerable<StateViolation>> check,
            string[] required, string[] allowed)
        {
            var violations = check(V).ToList();
            var ids = violations.Select(Id).Distinct(StringComparer.Ordinal).ToList();
            var detail = finding + " " + trace + "\n" + string.Join("\n", violations);
            if (required.Length == 0)
            {
                Assert.True(ids.Count == 0, "VALID trace with violations: " + detail);
                return;
            }

            Assert.True(required.All(r => ids.Any(x => Matches(x, r))), "INVALID trace without " + string.Join(", ", required) + ": " + detail);
            Assert.True(ids.All(x => required.Concat(allowed).Any(r => Matches(x, r))), "INVALID trace with an undeclared violation: " + detail);
        }

        [Fact]
        public void I62_A1_EveryTraceOfTheRequiredFindingsIsReplayedOrCoveredByANamedTest()
        {
            var coverage = JsonNode.Parse(I62Repo.ReadText(HarnessResult))!["Coverage"]!.AsObject();
            var traces = Traces().ToList();
            var replayed = traces.Select(x => (string)x[1]).ToList();
            Assert.Equal(replayed.Count, replayed.Distinct(StringComparer.Ordinal).Count());

            var missing = new List<string>();
            foreach (var f in RequiredFindings)
            {
                var names = coverage[f]!.AsArray().Select(t => (string)t!).ToList();
                Assert.NotEmpty(names);
                Assert.Contains(names, replayed.Contains);
                missing.AddRange(names.Where(t => !replayed.Contains(t) && !CoveredElsewhere.ContainsKey(t)).Select(t => f + ": " + t));
            }

            Assert.True(missing.Count == 0, "harness traces neither replayed nor covered:\n" + string.Join("\n", missing));
            var known = coverage.SelectMany(kv => kv.Value!.AsArray().Select(t => (string)t!)).ToHashSet(StringComparer.Ordinal);
            Assert.All(traces.Where(x => (string)x[0] != "A62-A1U-O1"), x => Assert.Contains((string)x[1], known));
            Assert.Contains(traces, x => (string)x[0] == "A62-A1U-O1");
            Assert.All(CoveredElsewhere.Keys, k => Assert.Contains(k, known));
        }

        [Theory]
        [InlineData("D1-8", "a62-a1-01-sustitucion-reinicia-contadores")]
        [InlineData("D1-9", "a62-a1-02-cierre-con-escalada-sin-resolver")]
        [InlineData("D1-5", "a62-a1-03-architect-sin-entrada")]
        [InlineData("D2-2", "a62-a1-04-LAUNCHING-sin-imagen-en-el-mapa")]
        [InlineData("D2-12", "a62-a1-05-ingesta-con-imagen-no-probada")]
        [InlineData("D2-11", "a62-a1-06-mapa-intermedio-ausente")]
        [InlineData("D1-13", "a62-a1-obs01-r10-LOOP_CLOSED-no-se-aplica-a-EXECUTION")]
        [InlineData("D1-18", "a62-a1r-01-exhausted-sin-decision")]
        [InlineData("D2-3", "a62-a1r-02-reviewer-rebase-loop-object-sin-reconciliar")]
        [InlineData("D1-20", "a62-a1r-03-reapertura-tras-rebase")]
        [InlineData("D1-17", "a62-a1s-01-sustitucion-con-intento-vivo-de-la-autoridad-sustituida")]
        [InlineData("A62-A1S-02", "a62-a1s-02-re-revision-sobre-el-objeto-anterior")]
        [InlineData("D2-8", "a62-a1t-01-t5b-cambia-el-RunId")]
        [InlineData("D2-2", "f4-a62-a1u-o1-entrada-compuesta-fabricada")]
        public void I62_A1_DisablingTheGuardingClauseInMemoryLetsTheRegressionThrough(string clause, string trace)
        {
            var c = Traces().Single(x => (string)x[1] == trace);
            var check = (Func<StateV2Validator, IEnumerable<StateViolation>>)c[2];
            Assert.Contains(check(V), x => x.Clause == clause);
            Assert.DoesNotContain(check(new StateV2Validator(new[] { clause })), x => x.Clause == clause);
        }

        // ------------------------------------------------------------------ case factories and validation scopes

        private static object[] Valid(string finding, string trace, Func<StateV2Validator, IEnumerable<StateViolation>> check) =>
            new object[] { finding, trace, check, Array.Empty<string>(), Array.Empty<string>() };

        private static object[] Invalid(string finding, string trace, Func<StateV2Validator, IEnumerable<StateViolation>> check, string[] required, params string[] allowed) =>
            new object[] { finding, trace, check, required, allowed };

        /// <summary>A harness trace that is already a named negative of the C-38 list: replayed through that builder, with its required and allowed invariants.</summary>
        private static object[] Existing(string finding, string trace, string negative)
        {
            var c = O.Negatives().Single(x => (string)x[0] == negative);
            var build = (Func<(StatePoint, StatePoint)>)c[1];
            return new object[] { finding, trace, (Func<StateV2Validator, IEnumerable<StateViolation>>)(v =>
            {
                var (p, n) = build();
                return Step(v, p, n);
            }), (string[])c[2], (string[])c[3] };
        }

        private static string Id(StateViolation x) => x.Clause.Length == 0 ? x.Invariant : x.Invariant + "/" + x.Clause;

        private static bool Matches(string reported, string expected) =>
            reported == expected || reported.StartsWith(expected + "/", StringComparison.Ordinal);

        /// <summary>The defective step of an INVALID trace: the file invariants of n and the pair invariants of (p, n).</summary>
        private static IEnumerable<StateViolation> Step(StateV2Validator v, StatePoint p, StatePoint n) => v.ValidateFile(n).Concat(v.ValidatePair(p, n)).ToList();

        /// <summary>A whole trace without Git: every point and every consecutive pair.</summary>
        private static IEnumerable<StateViolation> Seq(StateV2Validator v, params StatePoint[] points)
        {
            var all = new List<StateViolation>();
            for (var i = 0; i < points.Length; i++)
            {
                all.AddRange(v.ValidateFile(points[i]));
                if (i > 0)
                {
                    all.AddRange(v.ValidatePair(points[i - 1], points[i]));
                }
            }

            return all;
        }

        /// <summary>One step with Git: the file and history invariants of n, and the pair, pair-history and case B.1 invariants of (p, n).</summary>
        private static IEnumerable<StateViolation> HStep(StateV2Validator v, SyntheticGitHistory git, (StatePoint P, string Head) p, (StatePoint P, string Head) n) =>
            v.ValidateFile(n.P).Concat(v.ValidateHistory(n.P, git, n.Head)).Concat(v.ValidatePair(p.P, n.P))
                .Concat(v.ValidatePairHistory(p.P, p.Head, n.P, n.Head, git, Hx.StatePath)).Concat(v.ValidateB1History(p.P, n.P, git, n.Head)).ToList();

        /// <summary>A whole trace with Git: every point (file and history) and every consecutive pair (pair, pair history and case B.1).</summary>
        private static IEnumerable<StateViolation> HSeq(StateV2Validator v, SyntheticGitHistory git, params (StatePoint P, string Head)[] points)
        {
            var all = new List<StateViolation>(v.ValidateFile(points[0].P).Concat(v.ValidateHistory(points[0].P, git, points[0].Head)));
            for (var i = 1; i < points.Length; i++)
            {
                all.AddRange(HStep(v, git, points[i - 1], points[i]));
            }

            return all;
        }

        // ------------------------------------------------------------------ scenario builders

        private const string ArchitectRevocation = "```text\nI62-REVIEW-LOOP-REVOCATION: RLA-1\nClaim-Id: " + ClaimId + "\n```";
        private const string ArchitectClose = "```text\nI62-REVIEW-LOOP-CLOSE: ARL-6\nClaim-Id: " + ClaimId + "\n```";
        private const string ReviewerClose = "```text\nI62-REVIEWER-LOOP-CLOSE: " + ReviewerSamples.R1 + "\n```";

        private static string Rla(string id) =>
            "```text\nI62-REVIEW-LOOP-AUTHORIZATION: " + id + "\nRole: ARCHITECT\nContinuesLoopInstanceId: null\nClaim-Id: " + ClaimId + "\n```";

        private static YamlMap Entry0(StatePoint n) => (YamlMap)Y.L(n.State, "orchestration.architect_budgets")[0]!;

        private static YamlMap Last(StatePoint n, string path) => (YamlMap)Y.L(n.State, path)[^1]!;

        /// <summary>The synthetic graph of the F.8 loop reconciled twice, plus two later commits after each reconciliation (Kx1, Kx2 after Kn1; Ky1, Ky2 after Kn2).</summary>
        private static SyntheticGitHistory Graph(StatePoint s, string? imageBlob = null) =>
            Hx.LoopGraph(Hx.AuthorizationBlob(s), imageBlob)
                .Commit(Kx1, Hx.Kn1, null, (Hx.StatePath, "x1")).Commit(Kx2, Kx1, null, (Hx.StatePath, "x2"))
                .Commit(Ky1, Hx.Kn2, null, (Hx.StatePath, "y1")).Commit(Ky2, Ky1, null, (Hx.StatePath, "y2"));

        private static SyntheticGitHistory ExecutionGraph() => Hx.LoopGraph("0123456789abcdef0123456789abcdef01234567");

        /// <summary>The first reconciliation of the F.8 graph (C1 → C1', K3 → K3') applied to <paramref name="p"/>.</summary>
        private static StatePoint ReconcileR1(StatePoint p) =>
            Hx.Reconcile(p, Run1, Sha3, Hx.M1, Hx.K3, Hx.K3i, (C1, Hx.C1i, Hx.P1), (Hx.K3, Hx.K3i, Hx.Pk));

        /// <summary>A replanning (D2-2, D2-6) whose input fidelity evidence follows the new invocation (§20.3.3: every attempt carries its own).</summary>
        private static void FaithfulReplan(StatePoint n, string request, long seq, YamlMap target)
        {
            Hx.Replan(n, request, seq, target);
            var a = AttemptOf(n, request, seq);
            a["input_fidelity"] = Tree(n).PutJson("docs/automation/evidence/I-99-agent/review/" + request + "/" + seq + "/input-fidelity-" + Rv(n) + ".json", new JsonObject
            {
                ["Schema"] = "rackcad-input-fidelity/v1", ["Kind"] = "FIDELITY", ["InvocationId"] = J.S(Orchestration.Invocation(n, a), "InvocationId"),
                ["Fidelity"] = new JsonObject { ["Phase"] = "PREFLIGHT", ["FidelityStatus"] = "FAITHFUL" },
            });
        }

        /// <summary>The invoker's evidence says a1.1 never started (case B.1): BUDGET_RESERVED with a new invocation on <paramref name="target"/>; REVIEW_PENDING.</summary>
        private static StatePoint NotStarted(StatePoint p, YamlMap target)
        {
            var n = Next(p);
            FaithfulReplan(n, L1, 1, target);
            AttemptOf(n, L1, 1)["not_started_evidence"] = Tree(n).Put("docs/automation/evidence/I-99-agent/review/" + L1 + "/1/not-started.json",
                "{\"Schema\": \"rackcad-relay-record/v2\", \"Started\": false}\n");
            Loop(n)["phase"] = "REVIEW_PENDING";
            return n;
        }

        private static StatePoint Launched(StatePoint p)
        {
            var n = Next(p);
            var a = AttemptOf(n, L1, 1);
            a["state"] = "LAUNCHED";
            a["launch_evidence"] = Tree(n).Put("docs/automation/evidence/I-99-agent/review/" + L1 + "/1/launch.json", "{\"Schema\": \"rackcad-relay-record/v2\"}\n");
            return n;
        }

        /// <summary>The start is unknown: LAUNCH_UNCERTAIN with its original Target (counted conservatively); the phase returns to REVIEW_PENDING (SM-05).</summary>
        private static StatePoint Uncertain(StatePoint p)
        {
            var n = Next(p);
            var a = AttemptOf(n, L1, 1);
            a["state"] = "LAUNCH_UNCERTAIN";
            a["outcome"] = "UNCERTAIN";
            Loop(n)["phase"] = "REVIEW_PENDING";
            return n;
        }

        /// <summary>T5: a mutation of the second reconciliation n2 of the LAUNCHING attempt (the pair n1 → n2 must leave the attempt and the budgets intact).</summary>
        private static IEnumerable<StateViolation> T5(Action<StatePoint, YamlMap> mutation, StateV2Validator v)
        {
            var (_, n1, n2) = Hx.Reconciled();
            var n = Copy(n2);
            mutation(n, AttemptOf(n, L1, 1));
            return Step(v, n1, n);
        }

        /// <summary>s3 reconciled once, then the result of a1.1 received (evaluated object <paramref name="evaluated"/>) and ingested AGREED.</summary>
        private static List<StatePoint> IngestAfterRebase(YamlMap evaluated)
        {
            var (s3, n1, _) = Hx.Reconciled();
            var received = Next(n1);
            Received(received, AttemptOf(received, L1, 1), ArchitectResult(received, "docs/automation/evidence/I-99-agent/review/L1/1/result.json", L1, evaluated, "AGREED",
                new string[0], new (string, string)[0]));
            var done = Next(received);
            var a = AttemptOf(done, L1, 1);
            a["state"] = "RESULT_INGESTED";
            a["outcome"] = "VALID";
            a["ingested_at"] = Rv(done);
            RequestOf(done, L1)["state"] = "INGESTED";
            Loop(done)["phase"] = "ARCHITECT_SATISFIED";
            End((YamlMap)Loop(done)["action_validity"]!, "ARCHITECT_SATISFIED", Rv(done), null);
            End((YamlMap)Y.L(Entry0(done), "authorizations")[0]!, "ARCHITECT_SATISFIED", Rv(done), null);
            Orch(done)["next_action"] = NextAction("PRINCIPAL_COORDINATOR", "NEXT_GATE", (YamlMap)Loop(done)["object"]!);
            return new List<StatePoint> { s3, n1, received, done };
        }

        /// <summary>A copy of <see cref="I62F4OrchestrationValidatorTests.OpenSecondLoop"/> that appends to the decisions file and sets the first OpenFindings.</summary>
        private static StatePoint OpenArchitectLoop(StatePoint closed, string authorizationId, string[] decisionEntries, string[] openFindings)
        {
            var n = Next(closed);
            var decisions = WriteDecisions(n, decisionEntries);
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
            var req = Request(request, obj, 3, Attempt(n, request, 1, obj, binding, Rv(n), openFindings, (1, 1, 1, 0, 0)));
            req["loop_instance_id"] = iid;
            Y.L(n.State, "orchestration.review_requests").Add(req);
            var entry = Entry(iid, decisions, rounds: 1, requests: 1, launches: 1, reruns: new[] { (request, 0L) }, corrections: 0);
            ((YamlMap)Y.L(entry, "authorizations")[0]!)["authorization_id"] = authorizationId;
            Y.L(n.State, "orchestration.architect_budgets").Add(entry);
            Orch(n)["next_action"] = NextAction("ARCHITECT", "REVIEW", obj, decisions);
            return n;
        }

        /// <summary>An EXECUTION loop (its authority and phase from §8 and §16; the phase value is symbolic, as in the harness), optionally with a loop.object.</summary>
        private static StatePoint Execution(YamlMap? obj)
        {
            var n = Next(Point(5));
            var contract = ReviewerSamples.Contract(n, "T-01");
            Orch(n)["loop"] = M(("type", "EXECUTION"), ("phase", "WINDOW_OPEN"), ("instance_id", null), ("object", obj == null ? null : Clone(obj)),
                ("authorization", Clone(contract)), ("action_validity", Validity(ReviewerSamples.AuthorityOf(contract))));
            return n;
        }

        /// <summary>A REVIEWER lineage opened by an earlier reviewer result (custodied in the point's tree).</summary>
        private static YamlMap ReviewerLineage(StatePoint p, string lineage, string finding, string severity, string state)
        {
            var earlier = Tree(p).PutJson("docs/automation/evidence/I-99-agent/review/" + R0 + "/1/result.json", new JsonObject
            {
                ["Schema"] = "rackcad-reviewer-result/v1", ["ResultId"] = "RES-" + R0, ["LogicalReviewRequestId"] = R0, ["AttemptSeq"] = 1, ["RequestedRole"] = "REVIEWER",
                ["Action"] = "REVIEW_CHANGE", ["ReviewedUnit"] = Unit, ["ReviewedCommit"] = Sha, ["ReviewedPath"] = X, ["ReviewedBlob"] = B1, ["Disposition"] = "FINDINGS",
                ["Findings"] = new JsonArray(new JsonObject { ["FindingId"] = finding, ["Severity"] = severity }), ["FindingDispositions"] = new JsonArray(),
            });
            return M(("lineage_id", lineage), ("finding_ids", L(finding)), ("issuer", "REVIEWER"), ("opened_in", earlier), ("severity", severity), ("class", "defecto"),
                ("affected_section", "§1"), ("state", state), ("response", null), ("last_disposition_in", null), ("omitted_in", L()), ("closed_by", null),
                ("downgraded_by", null));
        }

        /// <summary>An earlier request of the same REVIEWER loop (substituted authority) whose attempt is still LAUNCHED; the budgets count it.</summary>
        private static void AddLiveEarlierRequest(StatePoint p)
        {
            var obj = Obj(C1, X, B1);
            var attempt = Attempt(p, R0, 1, obj, (YamlMap)AttemptOf(p, ReviewerSamples.R1, 1)["binding"]!, reservedAt: 5, openFindings: new string[0], snapshot: (1, 1, 1, 0, 0));
            attempt["state"] = "LAUNCHED";
            attempt["run_id"] = "R20261007T000000Z-r000";
            attempt["launching_utc"] = "2026-10-07T00:00:00Z";
            attempt["launch_evidence"] = Tree(p).Put("docs/automation/evidence/I-99-agent/review/" + R0 + "/1/launch.json", "{\"Schema\": \"rackcad-relay-record/v2\"}\n");
            var request = M(("logical_review_request_id", R0), ("loop_instance_id", null), ("object", Clone(obj)), ("round", 1L), ("state", "OPEN"), ("attempts", L(attempt)));
            Y.L(p.State, "orchestration.review_requests").Insert(0, request);
            var b = Y.M(p.State, "orchestration.budgets")!;
            b["logical_requests"] = 2L;
            b["architect_launches"] = 2L;
            Y.L(b, "transport_reruns").Insert(0, M(("logical_review_request_id", R0), ("count", 0L)));
        }

        /// <summary>The validity of the open REVIEWER loop ends (<paramref name="reason"/>) and its unlaunched attempt is cancelled in the same point.</summary>
        private static StatePoint ReviewerEnd(StatePoint open, string reason, string[]? decisionEntries)
        {
            var n = Next(open);
            AttemptOf(n, ReviewerSamples.R1, 1)["state"] = "CANCELLED_BEFORE_LAUNCH";
            RequestOf(n, ReviewerSamples.R1)["state"] = "CANCELLED";
            var by = decisionEntries == null ? null : WriteDecisions(n, decisionEntries);
            End((YamlMap)Loop(n)["action_validity"]!, reason, Rv(n), by);
            return n;
        }

        /// <summary>REVIEWER LOOP_CLOSED, with the closing decision appended to <paramref name="decisionEntries"/> (null: without a decision).</summary>
        private static StatePoint ReviewerCloseWith(StatePoint p, string[]? decisionEntries)
        {
            var n = Next(p);
            var closedBy = decisionEntries == null ? null : WriteDecisions(n, decisionEntries);
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

        /// <summary>[open, REVOKED by a decision, LOOP_CLOSED (E) keeping REVOKED].</summary>
        private static List<StatePoint> ReviewerRevokedAndClosed()
        {
            var open = ReviewerSamples.Loop()[1];
            var revocation = "```text\nI62-REVIEW-LOOP-REVOCATION: " + Y.S(Loop(open), "action_validity.authorization_id") + "\n```";
            var revoked = ReviewerEnd(open, "REVOKED", new[] { G0Entry, revocation });
            return new List<StatePoint> { open, revoked, ReviewerCloseWith(revoked, new[] { G0Entry, revocation, ReviewerClose }) };
        }

        /// <summary>
        /// A REVIEWER loop whose result opens a BLOCKING lineage (CORRECTING), expires and closes (E); a new gate contract (T-03) opens the next REVIEWER loop
        /// and its first reservation carries the inherited BLOCKING lineage in OpenFindings (D1-15, D1-20).
        /// </summary>
        private static List<StatePoint> NewAuthorityInheritsBlocking()
        {
            var loop = ReviewerSamples.Loop(("R-01", "BLOCKING"));
            var correcting = loop[^1];
            var expired = Next(correcting);
            End((YamlMap)Loop(expired)["action_validity"]!, "EXPIRED", Rv(expired), null);
            var closed = ReviewerCloseWith(expired, new[] { G0Entry, ReviewerClose });
            var reopened = O.ReviewerReopen(closed, newContract: true);
            var obj = Obj(C1, X, B1);
            var binding = (YamlMap)AttemptOf(reopened, ReviewerSamples.R1, 1)["binding"]!;
            Y.L(reopened.State, "orchestration.review_requests").Add(M(("logical_review_request_id", R2), ("loop_instance_id", null), ("object", Clone(obj)), ("round", 2L),
                ("state", "OPEN"), ("attempts", L(Attempt(reopened, R2, 1, obj, binding, Rv(reopened), new[] { "LIN-R1" }, (2, 2, 2, 0, 0))))));
            var b = Y.M(reopened.State, "orchestration.budgets")!;
            b["review_rounds"] = 2L;
            b["logical_requests"] = 2L;
            b["architect_launches"] = 2L;
            Y.L(b, "transport_reruns").Add(M(("logical_review_request_id", R2), ("count", 0L)));
            return new List<StatePoint> { correcting, expired, closed, reopened };
        }

        /// <summary>
        /// The REVIEWER correction cycle: [base, open, LAUNCHING, RESULT_RECEIVED (BLOCKING R-01), CORRECTING, PUBLISHED (X2), CI_VERIFIED, REREVIEW_PENDING]
        /// with the new request R2 opened on <paramref name="rereviewOn"/> (the corrected object, or the previous one for the A62-A1S-02 negative).
        /// </summary>
        private static List<StatePoint> ReviewerCorrection(YamlMap rereviewOn)
        {
            var points = ReviewerSamples.Loop(("R-01", "BLOCKING"));
            var obj2 = Obj(C2, X, B2);
            var published = Next(points[^1]);
            Loop(published)["phase"] = "PUBLISHED";
            Loop(published)["object"] = Clone(obj2);
            Y.M(published.State, "orchestration.budgets")!["correction_rounds"] = 1L;
            Orch(published)["next_action"] = NextAction("PRINCIPAL_COORDINATOR", "VERIFY_CI", obj2);
            points.Add(published);

            var verified = Next(published);
            Loop(verified)["phase"] = "CI_VERIFIED";
            points.Add(verified);

            var rereview = Next(verified);
            var contract = (YamlMap)Loop(rereview)["authorization"]!;
            var binding = (YamlMap)AttemptOf(rereview, ReviewerSamples.R1, 1)["binding"]!;
            Loop(rereview)["phase"] = "REREVIEW_PENDING";
            Y.L(rereview.State, "orchestration.review_requests").Add(M(("logical_review_request_id", R2), ("loop_instance_id", null), ("object", Clone(rereviewOn)),
                ("round", 2L), ("state", "OPEN"), ("attempts", L(Attempt(rereview, R2, 1, rereviewOn, binding, Rv(rereview), new[] { "LIN-R1" }, (2, 2, 2, 0, 1))))));
            var b = Y.M(rereview.State, "orchestration.budgets")!;
            b["review_rounds"] = 2L;
            b["logical_requests"] = 2L;
            b["architect_launches"] = 2L;
            Y.L(b, "transport_reruns").Add(M(("logical_review_request_id", R2), ("count", 0L)));
            Orch(rereview)["next_action"] = NextAction("REVIEWER", "REVIEW_CHANGE", rereviewOn, contract);
            points.Add(rereview);
            return points;
        }

        /// <summary>The REVIEWER loop in LAUNCHING reconciled once, then its result (on the original Target) received and ingested: REVIEWER_SATISFIED.</summary>
        private static List<StatePoint> ReviewerIngestAfterRebase()
        {
            var rv2 = ReviewerSamples.Loop()[2];
            var reconciled = ReconcileR1(rv2);
            var received = Next(reconciled);
            Received(received, AttemptOf(received, ReviewerSamples.R1, 1), ReviewerSamples.ReviewerResult(received, "NO_FINDINGS", Obj(C1, X, B1)));
            var done = Next(received);
            var a = AttemptOf(done, ReviewerSamples.R1, 1);
            a["state"] = "RESULT_INGESTED";
            a["outcome"] = "VALID";
            a["ingested_at"] = Rv(done);
            RequestOf(done, ReviewerSamples.R1)["state"] = "INGESTED";
            Loop(done)["phase"] = "REVIEWER_SATISFIED";
            End((YamlMap)Loop(done)["action_validity"]!, "REVIEWER_SATISFIED", Rv(done), null);
            Orch(done)["next_action"] = NextAction("PRINCIPAL_COORDINATOR", "NEXT_GATE", null);
            return new List<StatePoint> { rv2, reconciled, received, done };
        }
    }
}
