# I-52 — CTDA_HOST_PASS(V35-A3): coverage assessment

> **Generated** from `I-52-ctda-host-pass-coverage-v35a3.json` by `docs/automation/evidence/I-52-ctda-host-pass-coverage-v35a3.gen.py`. Documentation / analysis only: no AutoCAD, no probe, no source change.
> The JSON is authoritative; this file is its reviewable rendering. Decision record: `docs/automation/decisions/I-52.md` sections 196 and 197.

## 1. Result

```text
CTDA_HOST_PASS(V35-A3) = NOT EVALUATED   (not TRUE; no disqualifying FAIL/UNKNOWN or recorded inability, so not FALSE)
Clause 1 fixture identity                 INCOMPLETE   native half SATISFIED (6 rows); managed half NOT SATISFIED
Clause 2 complete execution               INCOMPLETE   native 6/100 governing (2 deferred, 92 not executed); managed 0/18
Clause 3 result completeness              INCOMPLETE
Clause 4 observability DA-P7/P8/P10       INCOMPLETE   DA-P7: S satisfied; M, SM, NOD not
Clause 5 no FAIL/UNKNOWN                  INCOMPLETE   0 FAIL, 0 UNKNOWN among the 6 governing results
```

Three-state model (decisions 197.1): **TRUE** — all five clauses hold on governing evidence. **FALSE** — terminal and monotone: a governing FAIL, a governing UNKNOWN, or an explicit recorded inability determination for a required row, fixture or instrument. **NOT EVALUATED** — no disqualifier exists and at least one required row is unexecuted but still considered producible. The term "NOT EVALUABLE" is retired.

## 2. Counts

| Class | Native (100) | Managed required (18) | Required 118 |
|---|---|---|---|
| PASS-GOVERNED | 6 | 0 | 6 |
| FAIL-GOVERNED | 0 | 0 | 0 |
| UNKNOWN-GOVERNED | 0 | 0 | 0 |
| EXECUTED-NON-GOVERNING | 0 | 0 | 0 |
| NOT-EXECUTED | 92 | 18 | 110 |
| DEFERRED | 2 | 0 | 2 |

Required rows not executed (not executed + deferred): **112** = 92 native + 2 deferred native + 18 managed. *EXECUTED-NON-GOVERNING* is counted per ROW (a row whose only executions are non-governing): there is none. Per EXECUTION: 11 governing-tuple attempts, 6 governing and 5 not (section 3).

The nine `16{S,M,SM}-{SEND,CONTEXT,IDLE}` managed scheduler rows of HEC-V27-C1 are **OUTSIDE-PREDICATE** (decisions 197.2); they are listed in section 7 and are not counted above.

## 3. The 11 governing-tuple executions

| Id | ProbeId | Package / source | Freeze of package | Governing | Carried fwd | Baseline status | Fixture BOOT-01 / CLN-BASE | FRESH surfaces | Engine result | Reason / ruling |
|---|---|---|---|---|---|---|---|---|---|---|
| E01 | 09N-B | `6e445fa8` / `6e445fa8` | V35-A2 | NON-GOVERNING | no | SUPERSEDED-BUILD (baseline bef6091b) | 8/8 / 8 | S | PASS-T (engine) | NOT VALID: the operator confirmed that the save-changes dialog appeared and pressed 'Don't save' by hand (Coordinator ruling, section 181) |
| E02 | 02NDBMOD-S | `c4037098` / `c4037098` | V35-A2 | NON-GOVERNING | no | SUPERSEDED-BUILD (baseline bef6091b) | 8/8 / 8 | S | PASS-S (engine) | NOT ACCEPTED: D-2 (corrupt CommandIdentity, use-after-free in I52Ctda_TokenSet) |
| E03 | 02NDBMOD-S | `2887d6c5` / `2887d6c5` | V35-A2 | NON-GOVERNING | no | SUPERSEDED-BUILD (baseline bef6091b) | 8/8 / 8 | S | UNKNOWN | NOT VALID: D-3 led to an interactive exit and Save changed the scratch DWG (section 181) |
| E04 | 09N-B | `e864a093` / `e864a093` | V35-A2 | GOVERNING | yes | SUPERSEDED-BUILD (baseline bef6091b) | 8/8 / 8 | S | PASS-T | VALID PASS-T (Coordinator ruling on H-1, section 184) |
| E05 | 02NDBMOD-S | `e864a093` / `e864a093` | V35-A2 | GOVERNING | yes | SUPERSEDED-BUILD (baseline bef6091b) | 8/8 / 8 | S | PASS-S | VALID PASS-S |
| E06 | 02NO-S | `e864a093` / `e864a093` | V35-A2 | GOVERNING | yes | SUPERSEDED-BUILD (baseline bef6091b) | 8/8 / 8 | S | PASS-S | VALID PASS-S |
| E07 | 16N-S | `e864a093` / `e864a093` | V35-A2 | NON-GOVERNING | no | SUPERSEDED-BUILD (baseline bef6091b) | 8/8 / 8 | S | UNKNOWN | NOT VALID: operator interaction on the host (independent of the engine UNKNOWN) |
| E08 | 16N-S | `e864a093` / `e864a093` | V35-A2 | NON-GOVERNING | no | SUPERSEDED-BUILD (baseline bef6091b) | 8/8 / 8 | S | UNKNOWN | Historical A2 UNKNOWN (UNK-MARKER-BINDING): D-4 was a V35 contract defect (section 187); contract amended (A3, section 188); not re-graded. Superseded under the narrow rule of section 197.5 |
| E09 | 16N-S | `afa65bc0` / `afa65bc0` | V35-A3 | GOVERNING | no | SUPERSEDED-BUILD (baseline bef6091b) | 8/8 / 8 | S | PASS-S | VALID PASS-S under V35-A3 (D-4 SEND anchor) |
| E10 | 10N-S | `afa65bc0` / `afa65bc0` | V35-A3 | GOVERNING | no | SUPERSEDED-BUILD (baseline bef6091b) | 8/8 / 8 | S | PASS-S | VALID PASS-S under V35-A3 (D-4 FIXTURE anchor) |
| E11 | 16C-S | `afa65bc0` / `afa65bc0` | V35-A3 | GOVERNING | no | SUPERSEDED-BUILD (baseline bef6091b) | 8/8 / 8 | S | PASS-S | VALID PASS-S under V35-A3 (D-4 CMDCTX anchor) |

Full tuple per execution (build tuple hash, native/payload/managed binary SHA-256, `R3_PRE_RUN_TUPLE_HASH`, `R3_HOST_EXECUTION_TUPLE_HASH`, row approval hash, verifier observations, evidence path and SHA-256) is in the JSON.
Every governing result ran on a build **other than the current baseline** `bef6091b` (sections 197.4 and 8).

### 3.1 Carry-forward conditions (A2 → A3, decisions 197.4), checked mechanically for E04–E06

| Execution | ProbeId | Row hash = oracle | Amended A3 entries referenced | Retained log reproduces result | Tuple recorded | carriedForward |
|---|---|---|---|---|---|---|
| E04 | 09N-B | yes | none | yes (research test D4PriorValidResults) | yes | true |
| E05 | 02NDBMOD-S | yes | none | yes (research test D4PriorValidResults) | yes | true |
| E06 | 02NO-S | yes | none | yes (research test D4PriorValidResults) | yes | true |

Provisionally governing. Before `CTDA_HOST_PASS` can be TRUE each carried or older-build result must be re-executed on the campaign build or carry an explicit Architect equivalence ruling.

## 4. Clause evaluation

| Clause | Status | Why |
|---|---|---|
| 1 — fixture identity | **INCOMPLETE** | Native half: each of the six governing logs records BOOT-01 declared/resolved = 8/8 and CLN-BASE FIXTURE-RESOURCES erased = 8, and binds the freeze package hash (A2 43DCE809.. or A3 D9FD41B4..) and the manifest rowApprovalHash (decisions 197.6). Managed half: HF-V31-1 with F-NOD and F-SRC (identities that exist only in the managed HF-V26 primitives, not in the eight native PersistentFixtureIdentity entries) has never been instantiated in a governed execution. |
| 2 — complete execution (100 native + 18 managed, exact governed identity/tuple) | **INCOMPLETE** | native 6/100 executed under a governing result (92 not executed, 2 deferred); managed 0/18. The three A2-package results are provisionally governing by conditional carry-forward and every governing result ran on a build other than the current baseline (decisions 197.4). |
| 3 — result completeness (H-P1..H-P12, IN T1..T16, T14 OUT, every tagged row PASS) | **INCOMPLETE** | H-P satisfied: none; IN T satisfied: none (rows tagged and all PASS). No FAIL among governing results. |
| 4 — observability (DA-P7 / DA-P8 / DA-P10) | **INCOMPLETE** | DA-P7: S SATISFIED; M, SM, NOD NOT SATISFIED; DA-P8 needs HEC row 11 (NOT-EXECUTED); DA-P10 needs HEC rows 14 and 15 (NOT-EXECUTED). |
| 5 — no FAIL / UNKNOWN anywhere in the governed matrix | **INCOMPLETE** | 0 FAIL-GOVERNED and 0 UNKNOWN-GOVERNED among the 6 governing results; nothing disqualifying is recorded. The absence cannot be established for the 112 required rows not executed. The two historical A2 UNKNOWN executions of 16N-S (E07, E08) remain in the execution history and are outside the governing matrix under the narrow supersession rule of decisions 197.5. |

### 4.1 DA-P7 / DA-P8 / DA-P10

Rule (decisions 197.7): a physical surface counts for DA-P7 only if a completed FRESH verifier read for it is recorded in the log of a governing PASS execution; catalog labels alone do not count.

| Surface | DA-P7 | Evidence (governing FRESH reads) |
|---|---|---|
| S | **SATISFIED** | E04 09N-B; E05 02NDBMOD-S; E06 02NO-S; E09 16N-S; E10 10N-S; E11 16C-S |
| M | **NOT SATISFIED** | — |
| SM | **NOT SATISFIED** | — |
| NOD | **NOT SATISFIED** | — |

- 09N-B is labelled Surface S/M, but its log has VER-S and VER-T FRESH reads and no VER-M: M is not credited. NOD is a surface of managed row 11 only.
- DA-P8: satisfied only by HEC row 11 (F-NOD): NOT-EXECUTED.
- DA-P10: satisfied only by HEC rows 14 and 15 (F-SRC): NOT-EXECUTED.

### 4.2 DA-P / H-P tag coverage (native + managed required 18; every tagged row must PASS)

| Tag | Rows tagged | of which native | of which managed | PASS-GOVERNED | Satisfied |
|---|---|---|---|---|---|
| P1 | 17 | 14 | 3 | 2 (02NO-S, 02NDBMOD-S) | no |
| P2 | 29 | 24 | 5 | 3 (09N-B, 02NO-S, 02NDBMOD-S) | no |
| P3 | 14 | 10 | 4 | 1 (09N-B) | no |
| P4 | 7 | 2 | 5 | 1 (02NO-S) | no |
| P5 | 1 | 0 | 1 | 0 (—) | no |
| P6 | 85 | 81 | 4 | 5 (09N-B, 10N-S, 16N-S, 16C-S, 02NDBMOD-S) | no |
| P7 | 0 | 0 | 0 | 0 (—) | no (cross-cutting property, no row carries it: see 4.1) |
| P8 | 1 | 0 | 1 | 0 (—) | no |
| P9 | 3 | 2 | 1 | 0 (—) | no |
| P10 | 2 | 0 | 2 | 0 (—) | no |
| P11 | 87 | 83 | 4 | 2 (16C-S, 02NDBMOD-S) | no |
| P12 | 82 | 75 | 7 | 3 (10N-S, 16N-S, 16C-S) | no |

### 4.3 Threat coverage (IN T1..T16; T14 OUT). `T2-DOC`, `T2-OBJ`, `T2-ENTITY` are counted under T2.

| Threat | Scope | Rows tagged | native | managed | PASS-GOVERNED | Satisfied |
|---|---|---|---|---|---|---|
| T1 | IN | 3 | 0 | 3 | 0 | no |
| T2 | IN | 98 | 96 | 2 | 5 | no |
| T3 | IN | 11 | 7 | 4 | 0 | no |
| T4 | IN | 11 | 9 | 2 | 1 | no |
| T5 | IN | 3 | 1 | 2 | 0 | no |
| T6 | IN | 4 | 2 | 2 | 0 | no |
| T7 | IN | 3 | 1 | 2 | 0 | no |
| T8 | IN | 16 | 12 | 4 | 2 | no |
| T9 | IN | 3 | 1 | 2 | 1 | no |
| T10 | IN | 2 | 0 | 2 | 0 | no |
| T11 | IN | 9 | 3 | 6 | 0 | no |
| T12 | IN | 6 | 3 | 3 | 0 | no |
| T13 | IN | 6 | 5 | 1 | 1 | no |
| T14 | OUT (diagnostic only; oracle: no state change) | 0 | 0 | 0 | 0 | n/a (OUT) |
| T15 | IN | 18 | 17 | 1 | 0 | no |
| T16 | IN | 63 | 63 | 0 | 2 | no |

## 5. Structural-UNKNOWN candidate rows (15; proposal V35 RC-14 / M4)

UNKNOWN is a legitimate possible outcome of a correct run for these rows, not a guaranteed one: `10N-S` returned PASS-S.

| ProbeId | Status | Anchor | Surface | Conditional host fact | UNKNOWN through |
|---|---|---|---|---|---|
| 09N-D | NOT-EXECUTED | CB-EXEC-01 | S | the command lock is still held at commandEnded | UNKNOWN-COMMON (CB-LOCK-01 lock unavailable, no mutation; RESULT-RULE-V35 step 2) |
| 10N-S | PASS-GOVERNED (E10) | CB-EXEC-01 | S | the command lock is still held at commandEnded | UNKNOWN-COMMON (CB-LOCK-01 lock unavailable, no mutation) and OBS-CANDIDATE-BOUNDARY = UNAVAILABLE (step 2) |
| 10N-M | NOT-EXECUTED | CB-EXEC-01 | M | the command lock is still held at commandEnded | UNKNOWN-COMMON (CB-LOCK-01 lock unavailable, no mutation) and OBS-CANDIDATE-BOUNDARY = UNAVAILABLE (step 2) |
| 10N-SM | NOT-EXECUTED | CB-EXEC-01 | SM | the command lock is still held at commandEnded | UNKNOWN-COMMON (CB-LOCK-01 lock unavailable, no mutation) and OBS-CANDIDATE-BOUNDARY = UNAVAILABLE (step 2) |
| 04NO-S | NOT-EXECUTED | EXEC-OBSERVE | S | the abort delivers cancelled/modifyUndone to OR-XR | OBS-PRIMARY-CALLBACK unavailable (RESULT-RULE-V35 step 2) |
| 04NO-UNDO-S | NOT-EXECUTED | EXEC-OBSERVE | S | the abort delivers cancelled/modifyUndone to OR-XR | OBS-PRIMARY-CALLBACK unavailable (RESULT-RULE-V35 step 2) |
| 10NDOC-WILL-SM | NOT-EXECUTED | CB-EXEC-01 | SM | a write-capable lock is observable before the request is granted | UNK-NO-WRITE-LOCK, UNK-ILLEGAL-CONTEXT |
| 13A-SM | DEFERRED | APPCTX-EXEC-01 | S+M+SM | the nested kWrite request returns eOk | UNK-LOCK-DOC-T-LEAK, UNK-ILLEGAL-CONTEXT |
| 02NAPP-SM | NOT-EXECUTED | CB-PRIMARY-01 | SM | a nested append to the same block table record is accepted inside objectAppended | UNKNOWN-COMMON (CB-PRIMARY-01 step failure) |
| 02NTAS-ALL | NOT-EXECUTED | CB-EXEC-01 | S+M+SM | the transaction manager accepts a start from that callback | UNK-ILLEGAL-CONTEXT |
| 02NTA-ALL | NOT-EXECUTED | CB-EXEC-01 | S+M+SM | the aborted T-PRIMARY is no longer active at the callback | UNK-NESTED-IN-ABORT |
| 02NEDW-ALL | NOT-EXECUTED | CB-EXEC-01 | S+M+SM | the command lock is already held at commandWillStart | UNK-NO-WRITE-LOCK, UNK-ILLEGAL-CONTEXT |
| COBJUNDO16SND-ALL | NOT-EXECUTED | SEND-EXEC-01 | S+M+SM | cancel() sends modifyUndone | OBS-ORIGIN-CALLBACK unavailable (step 2) |
| COBJUNDO16APP-ALL | NOT-EXECUTED | APPCTX-EXEC-01 | S+M+SM | cancel() sends modifyUndone | OBS-ORIGIN-CALLBACK unavailable (step 2) |
| 02NEDC-ALL | NOT-EXECUTED | EDC-EXEC-01 | S+M+SM | a write-capable command lock is observable in commandCancelled | UNK-NO-WRITE-LOCK, UNK-T-CANCEL-FAILURE |

## 6. Structural falsification campaign — DESIGN ONLY, NOT AUTHORIZED, NOT EXECUTED

Rules:

- Exactly one ProbeId per AutoCAD process.
- The same exact campaign build/tuple (baseline below) for every row; a build change stops the campaign for a Coordinator ruling.
- No result transfer between rows, families or anchors.
- No retry of an UNKNOWN or FAIL.
- A governing UNKNOWN => CTDA_HOST_PASS = FALSE (terminal). A governing FAIL => CTDA_HOST_PASS = FALSE (terminal).
- Stop the campaign immediately on the first governing UNKNOWN or FAIL. PASS => continue to the next row.
- A run that is not a valid governed execution (operator interaction, environment/tuple/package/trust failure, harness defect, timeout) is NON-GOVERNING: the campaign halts for a Coordinator ruling; it is neither PASS nor FALSE and is never retried automatically.
- Each row needs its own Coordinator authorization, the exact TRUSTEDPATHS entry, and the Owner no-touch declaration.
- 16A-S is NOT part of this group; 13A-SM is.

Order rule: Deterministic: ascending NPM-V35 section 5 row order (the catalog order). The corpus has no prior probability model of which host fact fails, so no risk-based order is invented. The order changes only how early a FALSE would appear, never the outcome, because FALSE is monotone; the Coordinator may reorder before authorization.

| # | ProbeId | Anchor | Chain | Surface | Verifiers | Expected possible structural outcome | Why it is in this campaign |
|---|---|---|---|---|---|---|---|
| 1 | 09N-D | CB-EXEC-01 | CHAIN-NONE | S | VER-S,VER-T | UNKNOWN through UNKNOWN-COMMON (CB-LOCK-01 lock unavailable, no mutation; RESULT-RULE-V35 step 2) if the conditional host fact does not hold; otherwise the row may reach its PASS class | RC-14 structural-UNKNOWN candidate (proposal V35 table): the architecture depends on a host fact that may legitimately be unavailable. A governing UNKNOWN or FAIL makes CTDA_HOST_PASS FALSE (terminal) without needing the remaining ~94 native rows or the managed authority. |
| 2 | 10N-M | CB-EXEC-01 | CHAIN-NONE | M | VER-M,VER-T | UNKNOWN through UNKNOWN-COMMON (CB-LOCK-01 lock unavailable, no mutation) and OBS-CANDIDATE-BOUNDARY = UNAVAILABLE (step 2) if the conditional host fact does not hold; otherwise the row may reach its PASS class | RC-14 structural-UNKNOWN candidate (proposal V35 table): the architecture depends on a host fact that may legitimately be unavailable. A governing UNKNOWN or FAIL makes CTDA_HOST_PASS FALSE (terminal) without needing the remaining ~94 native rows or the managed authority. |
| 3 | 10N-SM | CB-EXEC-01 | CHAIN-NONE | SM | VER-SM,VER-T | UNKNOWN through UNKNOWN-COMMON (CB-LOCK-01 lock unavailable, no mutation) and OBS-CANDIDATE-BOUNDARY = UNAVAILABLE (step 2) if the conditional host fact does not hold; otherwise the row may reach its PASS class | RC-14 structural-UNKNOWN candidate (proposal V35 table): the architecture depends on a host fact that may legitimately be unavailable. A governing UNKNOWN or FAIL makes CTDA_HOST_PASS FALSE (terminal) without needing the remaining ~94 native rows or the managed authority. |
| 4 | 04NO-S | EXEC-OBSERVE | CHAIN-NONE | S | VER-S,VER-T | UNKNOWN through OBS-PRIMARY-CALLBACK unavailable (RESULT-RULE-V35 step 2) if the conditional host fact does not hold; otherwise the row may reach its PASS class | RC-14 structural-UNKNOWN candidate (proposal V35 table): the architecture depends on a host fact that may legitimately be unavailable. A governing UNKNOWN or FAIL makes CTDA_HOST_PASS FALSE (terminal) without needing the remaining ~94 native rows or the managed authority. |
| 5 | 04NO-UNDO-S | EXEC-OBSERVE | CHAIN-NONE | S | VER-S,VER-T | UNKNOWN through OBS-PRIMARY-CALLBACK unavailable (RESULT-RULE-V35 step 2) if the conditional host fact does not hold; otherwise the row may reach its PASS class | RC-14 structural-UNKNOWN candidate (proposal V35 table): the architecture depends on a host fact that may legitimately be unavailable. A governing UNKNOWN or FAIL makes CTDA_HOST_PASS FALSE (terminal) without needing the remaining ~94 native rows or the managed authority. |
| 6 | 10NDOC-WILL-SM | CB-EXEC-01 | CHAIN-NONE | SM | VER-SM,VER-T | UNKNOWN through UNK-NO-WRITE-LOCK, UNK-ILLEGAL-CONTEXT if the conditional host fact does not hold; otherwise the row may reach its PASS class | RC-14 structural-UNKNOWN candidate (proposal V35 table): the architecture depends on a host fact that may legitimately be unavailable. A governing UNKNOWN or FAIL makes CTDA_HOST_PASS FALSE (terminal) without needing the remaining ~94 native rows or the managed authority. |
| 7 | 13A-SM | APPCTX-EXEC-01 | CHAIN-SYNC | S+M+SM | VER-ALL,VER-T | UNKNOWN through UNK-LOCK-DOC-T-LEAK, UNK-ILLEGAL-CONTEXT if the conditional host fact does not hold; otherwise the row may reach its PASS class | RC-14 structural-UNKNOWN candidate (proposal V35 table): the architecture depends on a host fact that may legitimately be unavailable. A governing UNKNOWN or FAIL makes CTDA_HOST_PASS FALSE (terminal) without needing the remaining ~94 native rows or the managed authority. |
| 8 | 02NAPP-SM | CB-PRIMARY-01 | CHAIN-NONE | SM | VER-SM,VER-T | UNKNOWN through UNKNOWN-COMMON (CB-PRIMARY-01 step failure) if the conditional host fact does not hold; otherwise the row may reach its PASS class | RC-14 structural-UNKNOWN candidate (proposal V35 table): the architecture depends on a host fact that may legitimately be unavailable. A governing UNKNOWN or FAIL makes CTDA_HOST_PASS FALSE (terminal) without needing the remaining ~94 native rows or the managed authority. |
| 9 | 02NTAS-ALL | CB-EXEC-01 | CHAIN-NONE | S+M+SM | VER-ALL,VER-T | UNKNOWN through UNK-ILLEGAL-CONTEXT if the conditional host fact does not hold; otherwise the row may reach its PASS class | RC-14 structural-UNKNOWN candidate (proposal V35 table): the architecture depends on a host fact that may legitimately be unavailable. A governing UNKNOWN or FAIL makes CTDA_HOST_PASS FALSE (terminal) without needing the remaining ~94 native rows or the managed authority. |
| 10 | 02NTA-ALL | CB-EXEC-01 | CHAIN-NONE | S+M+SM | VER-ALL,VER-T | UNKNOWN through UNK-NESTED-IN-ABORT if the conditional host fact does not hold; otherwise the row may reach its PASS class | RC-14 structural-UNKNOWN candidate (proposal V35 table): the architecture depends on a host fact that may legitimately be unavailable. A governing UNKNOWN or FAIL makes CTDA_HOST_PASS FALSE (terminal) without needing the remaining ~94 native rows or the managed authority. |
| 11 | 02NEDW-ALL | CB-EXEC-01 | CHAIN-NONE | S+M+SM | VER-ALL,VER-T | UNKNOWN through UNK-NO-WRITE-LOCK, UNK-ILLEGAL-CONTEXT if the conditional host fact does not hold; otherwise the row may reach its PASS class | RC-14 structural-UNKNOWN candidate (proposal V35 table): the architecture depends on a host fact that may legitimately be unavailable. A governing UNKNOWN or FAIL makes CTDA_HOST_PASS FALSE (terminal) without needing the remaining ~94 native rows or the managed authority. |
| 12 | COBJUNDO16SND-ALL | SEND-EXEC-01 | CHAIN-SEND | S+M+SM | VER-ALL,VER-T | UNKNOWN through OBS-ORIGIN-CALLBACK unavailable (step 2) if the conditional host fact does not hold; otherwise the row may reach its PASS class | RC-14 structural-UNKNOWN candidate (proposal V35 table): the architecture depends on a host fact that may legitimately be unavailable. A governing UNKNOWN or FAIL makes CTDA_HOST_PASS FALSE (terminal) without needing the remaining ~94 native rows or the managed authority. |
| 13 | COBJUNDO16APP-ALL | APPCTX-EXEC-01 | CHAIN-APPCTX | S+M+SM | VER-ALL,VER-T | UNKNOWN through OBS-ORIGIN-CALLBACK unavailable (step 2) if the conditional host fact does not hold; otherwise the row may reach its PASS class | RC-14 structural-UNKNOWN candidate (proposal V35 table): the architecture depends on a host fact that may legitimately be unavailable. A governing UNKNOWN or FAIL makes CTDA_HOST_PASS FALSE (terminal) without needing the remaining ~94 native rows or the managed authority. |
| 14 | 02NEDC-ALL | EDC-EXEC-01 | CHAIN-NONE | S+M+SM | VER-ALL,VER-T | UNKNOWN through UNK-NO-WRITE-LOCK, UNK-T-CANCEL-FAILURE if the conditional host fact does not hold; otherwise the row may reach its PASS class | RC-14 structural-UNKNOWN candidate (proposal V35 table): the architecture depends on a host fact that may legitimately be unavailable. A governing UNKNOWN or FAIL makes CTDA_HOST_PASS FALSE (terminal) without needing the remaining ~94 native rows or the managed authority. |

### 6.1 Campaign baseline identity (from the published zero-probe smoke evidence; package verified read-only)

| Field | Value |
|---|---|
| CAMPAIGN_SOURCE_SHA | `bef6091b282495a08226bf78c9671a0abff0607a` |
| PACKAGE_PATH | `D:\I52-CTDA-R2H\bef6091b` |
| PACKAGE_ZIP_SHA256 | `021F20923D60BE561C94F62AEEF390153784F1E955F862B8877D6060F09E55F7` |
| NATIVE (R-NATIVE-ARX) SHA-256 | `1493B2FE37CD4B1C5154E31F9EA67271D71DFBCFD4FF724451EAA8CF5E0CFDEF` |
| PAYLOAD (R-PAYLOAD-ARX) SHA-256 | `597F5BAF1DF47296B4A059CB87EB203C7636E6425050D689A5BB43B8628CAD24` |
| MANAGED (R-MANAGED-OBSERVER) SHA-256 | `3B4C7A191E2AF1FE16277210B51C9B0DC406CD2742137DBC17B40E58DD21A061` |
| BUILD_TUPLE | `4D80F95490DBA768D0B4160F6C3431F9C91881FF9E3E62FD1AD79EB138B1EAF0` |
| V35-A3 freeze package hash | `D9FD41B4CEBE5F698C2A597A96E4AAE9221A9DC32060ED791B73232636A47554` |
| acad.exe SHA-256 | `2A75996FD2A5C5EE0376FD5BA7CCED227A098193FF495E8CEEDA3A96FE326422` |
| SHA256SUMS | `41/41 (41 entries)` |
| zero-probe smoke R3_PRE_RUN_TUPLE_HASH | `47B7914EE1999CA9775577D7225F31C0257334F5136218429DF50C2EBCAA1BD9` |

R3_HOST_EXECUTION_TUPLE_HASH is generated per execution (it binds ProbeId, row hash and scratch) and is not part of the baseline. Source drift since the baseline: none (eng/ and the frozen V35 documents are byte-identical between bef6091b and the published branch).

## 7. Managed rows

Required for CTDA_HOST_PASS(V35-A3): exactly 18 — `01`, `02`, `03`, `04`, `05`, `06`, `07`, `08`, `09C`, `09E`, `10S`, `10M`, `10SM`, `11`, `12`, `13`, `14`, `15`. Fixture `HF-V31-1`. Every required row is NOT-EXECUTED. Executable authority is MISSING but presently considered IMPLEMENTABLE (decisions 197.3).

| ProbeId | Predicate status | Class | Trigger / scheduler / direct action | Surface | Tags | Would contribute if a governing PASS |
|---|---|---|---|---|---|---|
| 01 | REQUIRED | NOT-EXECUTED | M-DB-MOD | S | P1,P2 | — |
| 02 | REQUIRED | NOT-EXECUTED | M-DB-OPEN | S | P1,P4 | — |
| 03 | REQUIRED | NOT-EXECUTED | M-DB-MOD | S,M | P3 | — |
| 04 | REQUIRED | NOT-EXECUTED | MD-ABORT | S,M | P3,P4 | — |
| 05 | REQUIRED | NOT-EXECUTED | MD-COMMIT | S,M | P2,P3,P4 | — |
| 06 | REQUIRED | NOT-EXECUTED | M-DB-OPEN | S | P1,P2,P4 | — |
| 07 | REQUIRED | NOT-EXECUTED | MD-RYOW | S,M | P4 | — |
| 08 | REQUIRED | NOT-EXECUTED | M-DB-MOD | S,SM | P5 | — |
| 09C | REQUIRED | NOT-EXECUTED | M-DB-MOD | S,M | P2 | — |
| 09E | REQUIRED | NOT-EXECUTED | M-DOC-END | S,M | P2,P6,P11,P12 | — |
| 10S | REQUIRED | NOT-EXECUTED | M-DB-MOD | S | P6,P12 | — |
| 10M | REQUIRED | NOT-EXECUTED | M-DB-MOD | M | P11,P12 | — |
| 10SM | REQUIRED | NOT-EXECUTED | M-DB-MOD | SM | P6,P11,P12 | — |
| 11 | REQUIRED | NOT-EXECUTED | M-DB-MOD | S | P8 | DA-P8 (+NOD surface for DA-P7) |
| 12 | REQUIRED | NOT-EXECUTED | M-DB-MOD | M,SM | P9 | — |
| 13 | REQUIRED | NOT-EXECUTED | MS-CONTEXT | S,M,SM | P3,P12 | — |
| 14 | REQUIRED | NOT-EXECUTED | M-DB-MOD | S | P10,P12 | DA-P10 |
| 15 | REQUIRED | NOT-EXECUTED | M-DB-MOD | SM | P6,P10,P11,P12 | DA-P10 |
| 16S-SEND | OUTSIDE-PREDICATE | OUTSIDE-PREDICATE | MS-SEND | S | P6,P12 | — |
| 16M-SEND | OUTSIDE-PREDICATE | OUTSIDE-PREDICATE | MS-SEND | M | P11,P12 | — |
| 16SM-SEND | OUTSIDE-PREDICATE | OUTSIDE-PREDICATE | MS-SEND | SM | P6,P11,P12 | — |
| 16S-CONTEXT | OUTSIDE-PREDICATE | OUTSIDE-PREDICATE | MS-CONTEXT | S | P6,P12 | — |
| 16M-CONTEXT | OUTSIDE-PREDICATE | OUTSIDE-PREDICATE | MS-CONTEXT | M | P11,P12 | — |
| 16SM-CONTEXT | OUTSIDE-PREDICATE | OUTSIDE-PREDICATE | MS-CONTEXT | SM | P6,P11,P12 | — |
| 16S-IDLE | OUTSIDE-PREDICATE | OUTSIDE-PREDICATE | MS-IDLE | S | P6,P12 | — |
| 16M-IDLE | OUTSIDE-PREDICATE | OUTSIDE-PREDICATE | MS-IDLE | M | P11,P12 | — |
| 16SM-IDLE | OUTSIDE-PREDICATE | OUTSIDE-PREDICATE | MS-IDLE | SM | P6,P11,P12 | — |

## 8. Deferred probes

- **16A-S** — family `H-APPCTX,H-DB`, chain `CHAIN-APPCTX`, anchor `APPCTX-EXEC-01`, surface `S`, verifiers `VER-S,VER-T`, threats `T2,T8,T16`, tags `P6,P11,P12`. APPCTX anchor characterization (D-4 text unchanged for APPCTX); deferred in decisions sections 194/195 and HANDOFF; NOT in the structural group
- **13A-SM** — family `H-APPCTX`, chain `CHAIN-SYNC`, anchor `APPCTX-EXEC-01`, surface `S+M+SM`, verifiers `VER-ALL,VER-T`, threats `T13`, tags `P6,P11,P12`. APPCTX synchronous chain (nested lock request while the command lock is held); deferred in decisions sections 194/195; IS in the structural group (not yet authorized)

## 9. Native matrix (100 rows)

Fixture identity for every native row: `fixture-v35 / FEC-V35-1`. "Anchor" is the row's `ExecutionContextId`; "family" is its `HeaderAuthority`. "S*" marks the 15 structural-UNKNOWN candidates. "D-4" is the A3 anchor characterization. Results are never transferred between families or anchors.

| ProbeId | S* | Class | Family | Chain | Anchor | Surface | Tuple (pkg / source / freeze) | Carried fwd | Result | D-4 | FRESH surfaces | Non-governing exec |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| 02N |  | NOT-EXECUTED | H-DB | CHAIN-NONE | CB-PRIMARY-01 | S | — | — | — | — | — | — |
| 04N |  | NOT-EXECUTED | H-TX | CHAIN-NONE | EXEC-OBSERVE | S | — | — | — | — | — | — |
| 09N-A |  | NOT-EXECUTED | H-TX | CHAIN-NONE | EXEC-OBSERVE | S/M | — | — | — | — | — | — |
| 09N-B |  | PASS-GOVERNED | H-TX | CHAIN-NONE | EXEC-OBSERVE | S/M | `e864a093` / `e864a093` / V35-A2 | yes | PASS-T | — | S | E01 |
| 09N-O |  | NOT-EXECUTED | H-TX | CHAIN-NONE | EXEC-OBSERVE | S/M | — | — | — | — | — | — |
| 09N-C |  | NOT-EXECUTED | H-TX | CHAIN-NONE | EXEC-OBSERVE | S/M | — | — | — | — | — | — |
| 09N-D | S* | NOT-EXECUTED | H-ED | CHAIN-NONE | CB-EXEC-01 | S | — | — | — | — | — | — |
| 10N-S | S* | PASS-GOVERNED | H-ED | CHAIN-NONE | CB-EXEC-01 | S | `afa65bc0` / `afa65bc0` / V35-A3 | no | PASS-S | FIXTURE | S | — |
| 10N-M | S* | NOT-EXECUTED | H-ED | CHAIN-NONE | CB-EXEC-01 | M | — | — | — | — | — | — |
| 10N-SM | S* | NOT-EXECUTED | H-ED | CHAIN-NONE | CB-EXEC-01 | SM | — | — | — | — | — | — |
| 16N-S |  | PASS-GOVERNED | H-SEND,H-DB | CHAIN-SEND | SEND-EXEC-01 | S | `afa65bc0` / `afa65bc0` / V35-A3 | no | PASS-S | SEND | S | E07,E08 |
| 16N-M |  | NOT-EXECUTED | H-SEND,H-DB | CHAIN-SEND | SEND-EXEC-01 | M | — | — | — | — | — | — |
| 16N-SM |  | NOT-EXECUTED | H-SEND,H-DB | CHAIN-SEND | SEND-EXEC-01 | SM | — | — | — | — | — | — |
| C15N16N-SM |  | NOT-EXECUTED | H-LOAD,H-SEND,H-DB | CHAIN-SEND | SEND-EXEC-01 | SM | — | — | — | — | — | — |
| 02NO-S |  | PASS-GOVERNED | H-OBJ | CHAIN-NONE | CB-PRIMARY-01 | S | `e864a093` / `e864a093` / V35-A2 | yes | PASS-S | — | S | — |
| 02NO-M |  | NOT-EXECUTED | H-OBJ | CHAIN-NONE | CB-PRIMARY-01 | M | — | — | — | — | — | — |
| 02NO-SM |  | NOT-EXECUTED | H-OBJ | CHAIN-NONE | CB-PRIMARY-01 | SM | — | — | — | — | — | — |
| 02NO-CLOSE-SM |  | NOT-EXECUTED | H-OBJ | CHAIN-NONE | CB-PRIMARY-01 | SM | — | — | — | — | — | — |
| 04NO-S | S* | NOT-EXECUTED | H-OBJ | CHAIN-NONE | EXEC-OBSERVE | S | — | — | — | — | — | — |
| 04NO-UNDO-S | S* | NOT-EXECUTED | H-OBJ | CHAIN-NONE | EXEC-OBSERVE | S | — | — | — | — | — | — |
| 02NE-M |  | NOT-EXECUTED | H-OBJ | CHAIN-NONE | CB-PRIMARY-01 | M | — | — | — | — | — | — |
| 10NDOC-WILL-SM | S* | NOT-EXECUTED | H-DOC | CHAIN-NONE | CB-EXEC-01 | SM | — | — | — | — | — | — |
| 10NDOC-CHANGED-SM |  | NOT-EXECUTED | H-DOC | CHAIN-NONE | CB-EXEC-01 | SM | — | — | — | — | — | — |
| 10NDOC-VETO-SM |  | NOT-EXECUTED | H-DOC | CHAIN-NONE | EXEC-OBSERVE | SM | — | — | — | — | — | — |
| C2OBJ16N-S |  | NOT-EXECUTED | H-OBJ,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S | — | — | — | — | — | — |
| C2OBJ16N-M |  | NOT-EXECUTED | H-OBJ,H-SEND | CHAIN-SEND | SEND-EXEC-01 | M | — | — | — | — | — | — |
| C2OBJ16N-SM |  | NOT-EXECUTED | H-OBJ,H-SEND | CHAIN-SEND | SEND-EXEC-01 | SM | — | — | — | — | — | — |
| C2ENT16N-M |  | NOT-EXECUTED | H-OBJ,H-SEND | CHAIN-SEND | SEND-EXEC-01 | M | — | — | — | — | — | — |
| C2DOCW16N-SM |  | NOT-EXECUTED | H-DOC,H-SEND | CHAIN-SEND | SEND-EXEC-01 | SM | — | — | — | — | — | — |
| C2DOCC16N-SM |  | NOT-EXECUTED | H-DOC,H-SEND | CHAIN-SEND | SEND-EXEC-01 | SM | — | — | — | — | — | — |
| 13A-SM | S* | DEFERRED | H-APPCTX | CHAIN-SYNC | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| 16A-S |  | DEFERRED | H-APPCTX,H-DB | CHAIN-APPCTX | APPCTX-EXEC-01 | S | — | — | — | — | — | — |
| 16A-M |  | NOT-EXECUTED | H-APPCTX,H-DB | CHAIN-APPCTX | APPCTX-EXEC-01 | M | — | — | — | — | — | — |
| 16A-SM |  | NOT-EXECUTED | H-APPCTX,H-DB | CHAIN-APPCTX | APPCTX-EXEC-01 | SM | — | — | — | — | — | — |
| 16C-S |  | PASS-GOVERNED | H-CMDCTX,H-APPCTX | CHAIN-SYNC-CMDCTX | CMDCTX-EXEC-01 | S | `afa65bc0` / `afa65bc0` / V35-A3 | no | PASS-S | CMDCTX | S | — |
| 16C-M |  | NOT-EXECUTED | H-CMDCTX,H-APPCTX | CHAIN-SYNC-CMDCTX | CMDCTX-EXEC-01 | M | — | — | — | — | — | — |
| 16C-SM |  | NOT-EXECUTED | H-CMDCTX,H-APPCTX | CHAIN-SYNC-CMDCTX | CMDCTX-EXEC-01 | SM | — | — | — | — | — | — |
| 02NAPP-S |  | NOT-EXECUTED | H-DB | CHAIN-NONE | CB-PRIMARY-01 | S | — | — | — | — | — | — |
| 02NAPP-M |  | NOT-EXECUTED | H-DB | CHAIN-NONE | CB-PRIMARY-01 | M | — | — | — | — | — | — |
| 02NAPP-SM | S* | NOT-EXECUTED | H-DB | CHAIN-NONE | CB-PRIMARY-01 | SM | — | — | — | — | — | — |
| 02NTAS-ALL | S* | NOT-EXECUTED | H-TX | CHAIN-NONE | CB-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| 02NTS-ALL |  | NOT-EXECUTED | H-TX | CHAIN-NONE | CB-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| 02NTA-ALL | S* | NOT-EXECUTED | H-TX | CHAIN-NONE | CB-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| 02NRXW-ALL |  | NOT-EXECUTED | H-LOAD | CHAIN-NONE | CB-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| 02NRXL-ALL |  | NOT-EXECUTED | H-LOAD | CHAIN-NONE | CB-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| 02NEDW-ALL | S* | NOT-EXECUTED | H-ED | CHAIN-NONE | CB-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CAPP16SND-ALL |  | NOT-EXECUTED | H-DB,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CAPP16APP-ALL |  | NOT-EXECUTED | H-DB,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CTAS16SND-ALL |  | NOT-EXECUTED | H-TX,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CTAS16APP-ALL |  | NOT-EXECUTED | H-TX,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CTS16SND-ALL |  | NOT-EXECUTED | H-TX,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CTS16APP-ALL |  | NOT-EXECUTED | H-TX,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CTA16SND-ALL |  | NOT-EXECUTED | H-TX,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CTA16APP-ALL |  | NOT-EXECUTED | H-TX,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CRXW16SND-ALL |  | NOT-EXECUTED | H-LOAD,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CRXW16APP-ALL |  | NOT-EXECUTED | H-LOAD,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CRXL16SND-ALL |  | NOT-EXECUTED | H-LOAD,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CRXL16APP-ALL |  | NOT-EXECUTED | H-LOAD,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CEDW16SND-ALL |  | NOT-EXECUTED | H-ED,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CEDW16APP-ALL |  | NOT-EXECUTED | H-ED,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| C2APP2CMD-ALL |  | NOT-EXECUTED | H-DB,H-APPCTX,H-CMDCTX | CHAIN-APPCTX-CMDCTX | CMDCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CDBERASE16SND-ALL |  | NOT-EXECUTED | H-DB,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CDBOPEN16SND-ALL |  | NOT-EXECUTED | H-DB,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CDOCLOCKVETO16SND-ALL |  | NOT-EXECUTED | H-DOC,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CEDEND16SND-ALL |  | NOT-EXECUTED | H-ED,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| COBJCANCEL16SND-ALL |  | NOT-EXECUTED | H-OBJ,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| COBJCLOSED16SND-ALL |  | NOT-EXECUTED | H-OBJ,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| COBJERASE16SND-ALL |  | NOT-EXECUTED | H-OBJ,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| COBJOPEN16SND-ALL |  | NOT-EXECUTED | H-OBJ,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| COBJUNDO16SND-ALL | S* | NOT-EXECUTED | H-OBJ,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CTRABOUTABORT16SND-ALL |  | NOT-EXECUTED | H-TX,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CTRABOUTEND16SND-ALL |  | NOT-EXECUTED | H-TX,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CTRENDED16SND-ALL |  | NOT-EXECUTED | H-TX,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CTROUTERMOSTENDCALLED16SND-ALL |  | NOT-EXECUTED | H-TX,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CDBERASE16APP-ALL |  | NOT-EXECUTED | H-DB,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CDBOPEN16APP-ALL |  | NOT-EXECUTED | H-DB,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CDOCLOCKCHANGED16APP-ALL |  | NOT-EXECUTED | H-DOC,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CDOCLOCKVETO16APP-ALL |  | NOT-EXECUTED | H-DOC,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CDOCLOCKWILL16APP-ALL |  | NOT-EXECUTED | H-DOC,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CEDEND16APP-ALL |  | NOT-EXECUTED | H-ED,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CENTGFX16APP-ALL |  | NOT-EXECUTED | H-OBJ,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| COBJCANCEL16APP-ALL |  | NOT-EXECUTED | H-OBJ,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| COBJCLOSED16APP-ALL |  | NOT-EXECUTED | H-OBJ,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| COBJERASE16APP-ALL |  | NOT-EXECUTED | H-OBJ,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| COBJMOD16APP-ALL |  | NOT-EXECUTED | H-OBJ,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| COBJOPEN16APP-ALL |  | NOT-EXECUTED | H-OBJ,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| COBJUNDO16APP-ALL | S* | NOT-EXECUTED | H-OBJ,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CTRABOUTABORT16APP-ALL |  | NOT-EXECUTED | H-TX,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CTRABOUTEND16APP-ALL |  | NOT-EXECUTED | H-TX,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CTRENDED16APP-ALL |  | NOT-EXECUTED | H-TX,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CTROUTERMOSTENDCALLED16APP-ALL |  | NOT-EXECUTED | H-TX,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| 02NEDC-ALL | S* | NOT-EXECUTED | H-ED | CHAIN-NONE | EDC-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CEDC16SND-ALL |  | NOT-EXECUTED | H-ED,H-SEND | CHAIN-SEND | SEND-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| CEDC16APP-ALL |  | NOT-EXECUTED | H-ED,H-APPCTX | CHAIN-APPCTX | APPCTX-EXEC-01 | S+M+SM | — | — | — | — | — | — |
| 02NDBMOD-S |  | PASS-GOVERNED | H-DB | CHAIN-NONE | CB-PRIMARY-01 | S | `e864a093` / `e864a093` / V35-A2 | yes | PASS-S | — | S | E02,E03 |
| 02NDBMOD-M |  | NOT-EXECUTED | H-DB | CHAIN-NONE | CB-PRIMARY-01 | M | — | — | — | — | — | — |
| 02NDBMOD-SM |  | NOT-EXECUTED | H-DB | CHAIN-NONE | CB-PRIMARY-01 | SM | — | — | — | — | — | — |
| 02NDBERASE-S |  | NOT-EXECUTED | H-DB | CHAIN-NONE | CB-PRIMARY-01 | S | — | — | — | — | — | — |
| 02NDBERASE-M |  | NOT-EXECUTED | H-DB | CHAIN-NONE | CB-PRIMARY-01 | M | — | — | — | — | — | — |
| 02NDBERASE-SM |  | NOT-EXECUTED | H-DB | CHAIN-NONE | CB-PRIMARY-01 | SM | — | — | — | — | — | — |

## 10. Inputs (SHA-256 of LF-normalized content)

- `docs/initiatives/I-52-native-probe-matrix-v35.md` — `41BAE1512A9A8D1559320F427BA15879537F0FF43C972ED5E7C53D252F9DE092`
- `docs/initiatives/I-52-host-event-catalog-v27-correction.md` — `D624156F3461061E62076E8C25792D966CA34A5CF4840E5868ADF4A559506E01`
- `docs/initiatives/I-52-host-event-catalog-v27.md` — `357569C9C057F9C94D77C5E13A1C6E5319FBEBB4E12BDD4895F0FB2A5220F9AB`
- `docs/initiatives/I-52-execution-catalog-v35.json` — `428F1306DB50D1E37701912C377AF85436DB1C0E75C3DE9F209315AC14764FFA`
- `docs/initiatives/I-52-fixture-execution-contract-v35.md` — `D0F854A08A753CE60BAD3CF040BA6A776A4BC24DFF7BD1241599152FD40760CC`
- `docs/initiatives/I-52-proposal-v35.md` — `E5525B1991E6BCB7DACF100BF6BED9713A80CF17FB41358467AF55667FA9E57B`
- `eng/research/I52Ctda/v35-oracle.json` — `995D69342CE14683002133319481AA1653DD3D6E794A1BAB999BD582586F8149`
- `docs/automation/evidence/I-52-r3-host-smoke-bef6091b.json` — `E3538271ED741731750DDB65538E31E80976EA0D28FF068F61D2513372DC8F9E`
