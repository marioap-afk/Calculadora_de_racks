# I-52 — Proposal V35 — mechanically executable probe matrix

> **DRAFT, revision V35-A2 / READY FOR COORDINATOR FREEZE / NOT EXECUTED.** V35 candidate `0c609269b47adedd0f29d8b4cbbc010fae91d2e0`
> was reviewed by the Architect (AGREED WITH CORRECTIONS, 14 required corrections, decision §170); V35-A1 applies RC-01..RC-14 (§6).
> The Architect delta review of V35-A1 (`34b3b05b2dc4b32b9b04defe15def3d0fcc9a9c3`) concluded AGREED WITH SMALL CORRECTIONS
> (0 BLOCKER, 0 MAJOR, 4 MINOR); V35-A2 applies the errata M1..M4 (§6, decision §171).
> Baseline I-52 `5c4521c7d8f51e96f128188fd4f52b6437ca9bce`
> (main `016bf46715cec45e644f88a22ef091b311a2bef1`, I-55 `53d5d99103ae79cb8258016fa3aee16b1dcd75cc`). Parent V34 blobs:
> Proposal `aec39a7599e6c76613b247b21a8bf6e7f446bafa`, NPM `7a29a1c11d308879488e43dc460602df1426697b`, NEC `1f16dd0641c1706e6874a68b9136406c19e33848`, NSC `3ca54ef1992c3b15d2382a42f09693cd02cabdb6`, FEC `9976806e301d3f060a8d99bd0fa12ecfa90e44cb`. V35 is an incremental evolution of V34:
> it changes no ProbeId, EventId, SchedulerId or support action, and adds one fixture identity (7 -> 8).

## 1. Why V35

The R3 governed-executor discovery (§167) stopped before implementation. The Architect R3 review (§168) concluded that
V34 is not a closed executable contract: FEC-V34 §5 asserts `FixtureExecutionContractClosed = TRUE` by definition while
requiring "a deterministic trigger definition" that 100/100 rows lack. The hard-block count rose from 32 to 42 (R3-C01
promoted to R3-B11 with seven more rows; R3-B12 candidate-boundary on 10N-S/M/SM), R3-C02 was ruled not blocking but
given a cleanup order, and zero of 100 rows had a closed execution plan. The Coordinator accepted V35.

## 2. What V35 changes

> This section describes V35 as first published (`0c609269`). Where V35-A1 changed it, the text is **SUPERSEDED BY §6**
> and by the normative catalog; the row count and the result rule below are stated as they hold after V35-A1.

- **Execution catalog.** `I-52-execution-catalog-v35.json` (EXEC-CATALOG-V35-1) is the single normative source for every
  execution identifier: run environment and drivers, resources, observer registrations, guards, setup and trigger
  actions, scheduler chains, execution contexts, transaction/top/lock authorities, mutation actions, observation, UNKNOWN
  and FAIL predicates, result classes, cleanup steps and actions, completion fences and surfaces. Retained V34
  ContractIds, the 27 EventIds, 3 SchedulerIds and 7 support actions are imported as entries with their authority.
- **Row schema.** NPM-V35 rows have 36 fields instead of 24 (34 in V35; V35-A1 added `MarkerStageBindings` and
  `CompletionTokenIds`, SUPERSEDED BY §6), including `BodyObserverId`, the only registration that runs
  the probe body. Setup, Trigger, transaction, lock, mutation, PASS, FAIL and
  UNKNOWN prose is replaced by identifier lists; markers carry polarity (`+` MUST_BE_PRESENT, `-` MUST_BE_ABSENT). The
  columns PrimaryAuthorityId, ScheduleOriginEventId, HeaderAuthority, Surface, SchedulePoint, ExecutionPoint,
  ExpectedBefore, ExpectedAfter, Verifier, Threats and DA-P/H-P are byte-identical to V34.
- **Lineage.** Every V34 executable clause of every row is recorded in `I-52-v35-clause-lineage.json` with class A
  (bound V34 ContractId, kept or relocated), B (natural language formalized by a catalog id) or C (explanatory clause
  enforced by a named invariant). Class D (unresolved) is zero.
- **Result rule (V35-A1, SUPERSEDED BY §6).** `RESULT-RULE-V35` is evaluated only by `CONTROL-PLANE-RESULT-01`, outside the
  scratch process and after the CompletionFence, in five ordered steps: (1) safety: with SAFETY-EVIDENCE-COMPLETE,
  `FP-CLEANUP-SAFETY` (the V34 §5 cleanup-safety exception) gives the row's FailClass; (2) UNKNOWN if EVIDENCE-COMPLETE is
  false, UNKNOWN-COMMON, a listed UnknownPredicate holds or a required observation is unavailable; (3) FAIL if another listed
  FailPredicate holds; (4) PASS if the PassClass, every ObservationPredicate, the fresh verifier against ExpectedAfter and
  the cleanup plus fence all hold; (5) otherwise UNKNOWN. The catalog entry `RESULT-RULE-V35` is normative. The V35 wording
  (UNKNOWN first, no safety step) no longer applies.
- **Closure is proven, not asserted.** No V35 document declares a closure predicate TRUE by definition. The static
  validator `eng/research/I52Ctda/validate-v35-catalog.ps1` computes them from the published files.

## 3. Architect decisions applied

| Group | Rows | V35 closure |
|---|---|---|
| R3-B01 | 5 | `F-TRIGGER-APPEND` is a DisposableTriggerResource created by `TRG-APPEND-TRIGGER`; `RG-DB-APPEND` armed with the in-memory instance; removed by `CLN-BASE` |
| R3-B02 | 2 | 02N: notifier A = `F-TRIGGER-MOD`, `RG-DB-OPEN`; 02NO-S: notifier A = `F-TRIGGER-XR` (8th identity) observed by `OR-TXR`, target B = `F-XR` |
| R3-B03 | 7 | `R-PAYLOAD-ARX` (NL-ARX-PATH, SHA-256 pinned by `SET-PAYLOAD-PATH`), `TRG-LOAD-PAYLOAD` / `SET-LOAD-PAYLOAD`, `OBS-LOAD` = RTNORM + loaded state + N-RX-LOADED, no unload, `FENCE-PROCESS-EXIT` |
| R3-B04 | 11 | `CLN-OBJ-REMOVE` removes each retained object/entity reactor while its notifier is live, non-eOk UNKNOWN, no goodbye dependency; 26 callbacks unchanged |
| R3-B05 | 11 | `OR-XR`, `OR-TXR`, `OR-REF-A`, `OR-REF-B`, `OR-TEO`, `ER-REF-A` are ObserverRegistration entries |
| R3-B06 | 1 | 02NO-CLOSE-SM `MUT-SM` write set is the §164 set {R-SM-LINK, F-REF-C}; B and C are membership operands |
| R3-B07 | 3 | 04N, 04NO-S, 04NO-UNDO-S are `EXEC-OBSERVE` with `MUT-NONE`; staging is `SET-STAGE-S-IN-PRIMARY`; "if legal" and "controlled S marker" removed |
| R3-B08 | 8 | `CB-EXEC-01` (T_CB via SA-TX-*, `CB-LOCK-01` observed write-capable lock else UNKNOWN, no SA-LOCK, top T observation only), `RG-TX` |
| R3-B09 | 1 | `MARK-APPCTX-CALLBACK-RETURN` replaces MARK-APPCTX-RETURN in C2APP2CMD-ALL |
| R3-B10 | 2 | `-N-DOC-LOCK-VETO` (MUST_BE_ABSENT) in 10NDOC-WILL-SM and 10NDOC-CHANGED-SM; NEC projects `MARKER_ABSENT_FOR` |
| R3-B11 | 9 | `TRG-LOCK-CYCLE` / `TRG-LOCK-VETO` run in `DRIVER-APP-01` delivery with `DRIVER-LOCK-01` (SA-LOCK kWrite on the scratch DWG, RR-DOC registered first, explicit SA-UNLOCK), veto in WILL via `SET-ARM-VETO` |
| R3-B12 | 3 | `OBS-CANDIDATE-BOUNDARY` (ACCEPTED / UNAVAILABLE / contradiction) and `FP-BOUNDARY` on 10N-S/M/SM |
| R3-C02 | 6 | every CleanupAction has exactly one terminal `CLN-BASE`: CLN-OBJ-REMOVE / CLN-DOC-REMOVE -> CLN-DEFER-DRAIN -> CLN-BASE (`INV-SINGLE-TERMINAL-BASE`) |

New guards are exactly those named by the Architect (`RG-DB-APPEND`, `RG-DB-OPEN`, `RG-TX`) plus the generic
`RG-ONESHOT` of §4.

## 4. Decisions introduced by V35 (Architect verdicts)

Architect V35 review verdicts: 4, 6, 9 AGREE; all others AGREE WITH CORRECTION, applied by V35-A1 (§6): 1 by RC-04/RC-05,
2 by RC-07, 3 by RC-01/RC-03, 5 by RC-11, 7 by RC-05, 8 by RC-09, 10 by RC-10, 11 by RC-01/RC-12, 12 by RC-06, 13 by RC-14.

These are formalizations the Architect's decisions did not fix literally. Each is fully defined in the catalog; none
changes a frozen count. Any of them may be rejected without reopening the others. VER-SM is bound to its §164
definition (it never opens F-REF-B), which is a binding already in force, not a new decision.

1. **02NO-SM cleanup (`CLEAN-OBJ-FENCED`).** The trigger erases F-REF-B and §164 forbids reopening B, so OR-REF-B cannot
   be removed with CLN-OBJ-REMOVE. V35 retains the reactor instance unreleased, never touches the notifier and requires
   `FENCE-PROCESS-EXIT`.
2. **`RR-MANAGED-CMD`.** MARK-MANAGED-CMD-END (09N-D, 10N-S/M/SM) is a managed `Document.CommandEnded` observation
   (HEC-V27-C1, row 09E, M-DOC-END). V35 models it as an ObserverRegistration hosted by a managed observer (V35-A1: the RuntimeModule R-MANAGED-OBSERVER, loaded by a DRIVER-SCRIPT-01 `_.NETLOAD` line)
   for the scratch document and writing into the same log. This is a new build artifact for R3.
3. **Driver APPCTX delivery (`DRIVER-APP-01`).** B11 places the lock trigger in a driver-owned APPCTX delivery. V35 obtains
   it with `beginExecuteInApplicationContext`, classified `DRIVER-INFRA` (12 rows: nine lock rows and 16C-S/M/SM) with no
   scheduler credit and no NS pair. 16C uses it so that `beginExecuteInCommandContext` cancels no driver command.
4. **One process per ProbeId (`RUN-ENV-01`).** V34 required a dedicated process for some rows; V35 requires it for all 100,
   with `DRIVER-SCRIPT-01` (BOOT, PROBE, optional FIXTURE/HFV34_CANCEL; since V35-A1 FINISH and the exit are issued by FIN-GATE-01 and CMD-FINISH, not by the script) and no human input.
5. **`RG-ONESHOT`.** A generic one-shot guard for body-running callbacks outside the DB/TX families (object, entity, editor,
   document, dynamic linker, payload reactor, SA-VETO body), required so that "one-shot origin" has one meaning.
6. **`SEND-EXEC-01` and `CMD-QUEUED`.** NSC-V34 says the NS-SEND delivered command "owns/observes its command lock and opens
   a fresh transaction"; V35 names it (SEND-LOCK-01, T_SEND) and uses one exact queued command `I52CTDA_QUEUED` whose
   retained data selects MUT-S/M/SM/ALL, instead of one command per surface.
7. **`PASS-T` / `FAIL-T`.** The four S/M transaction-observation rows (09N-A/B/O/C) had no surface result class; V35 adds one.
8. **Cancel and undo triggers.** 04NO-S and 04NO-UNDO-S (T-ABORTING, STATE-T-ABORT) characterize cancel/undo through
   `TRG-ABORT-PRIMARY-T` over a staged MUT-S; the ALL origins COBJCANCEL16*/COBJUNDO16* use `TRG-CANCEL-OPEN-XR`
   (`AcDbObject::cancel()` with no byte change) so the staged bytes cannot mask STATE-ALL-1.
9. **Open-for-modify origin target.** CDBOPEN16* and COBJOPEN16* keep V34's F-XR as the trigger target; `RG-DB-OPEN` arms on
   the row's trigger target.
10. **Trigger mechanics.** Object triggers use `acdbOpenObject`/`close()` (not T-PRIMARY) so notifications arrive
    synchronously while the driver's T-PRIMARY is open; triggers write fixed trigger values (for example F-TRIGGER-MOD
    position `(1000,1,0)`), which is safe because each process runs one ProbeId.
11. **Guard lifetime of script-issued triggers.** For `TRG-RUN-FIXTURE-CMD` and `TRG-CANCEL-CMDCTX` the guards stay armed
    after I52CTDA_PROBE returns, bound to the exact command name, and I52CTDA_FINISH disarms them. `RG-ONESHOT` has one
    instance per callback identity (so the SA-VETO body and a VETO origin body are distinct) and suppresses nesting caused
    by the body's own actions.
12. **13A-SM lock context.** NPM-V34 binds APPCTX-EXEC-01 to the synchronous NX-APPCTX-SYNC callback of 13A-SM; V35 states
    that SA-LOCK/APPCTX-LOCK-01 are legal there, which extends the FEC-V34 "delivered APPCTX" wording.
13. **Candidate-hostile contexts accepted as UNKNOWN.** CB-EXEC-01 in `documentLockModeWillChange` (10NDOC-WILL-SM) and in
    `commandWillStart` (02NEDW-ALL), and APPCTX-EXEC-01 inside synchronous `executeInApplicationContext` from a modal
    command (13A-SM), may be unable to observe a write-capable lock; V35 formalizes that outcome as UNKNOWN rather than
    adding lock acquisition the Architect excluded.

## 5. Mechanical result (V35-A1)

The validator parses NPM-V34 and NPM-V35, the catalog, NEC-V34/V35, fixture and traceability files, the lineage and R3
discovery evidence, and, since V35-A1, the architecture-review oracle `eng/research/I52Ctda/v35-oracle.json`. Its result is
recorded in `docs/automation/evidence/I-52-v35-mechanical-validation.json`:

```text
INPUT ROWS = 100 / PARSED = 100 / DERIVED PLANS = 100
UNBOUND IDS = 0 / EXECUTABLE FREE-TEXT = 0 / CLASS-D = 0 / CONTRADICTIONS = 0 / LINEAGE MISSING = 0
EVENT RELATIONS = 237 (MARKER_ABSENT_FOR = 2) / SCHEDULER PAIRS = 63 / PRIMARY COVERAGE = 26/26
CALLBACKS = 26 / SCHEDULERS = 3 / SUPPORT ACTIONS = 7 / FIXTURE IDENTITIES = 8
OPEN BLOCKER GROUPS = 0 (R3-B01..B12, R3-C02)
REFERENCED OBJECTARX MEMBERS = 22 / MISSING IN EXACT SDK HEADERS = 0
ExecutionCatalogClosed, TriggerAuthorityClosed, SetupAuthorityClosed, PredicateAuthorityClosed,
ResourceAuthorityClosed, FixtureExecutionContractClosedV35, PlanDerivationClosed = TRUE (validator)
```

The NEC derivation rule still reproduces NEC-V34's 237 relations exactly when applied to NPM-V34. The negative-control
harness `eng/research/I52Ctda/validate-v35-negative-controls.ps1` runs 98 controls against temporary copies: the 24 V35
controls, the Architect's C1-C7, 20 RC-specific controls, 42 controls added by the V35-A1 pre-publication reviews
(L1-L6, X1-X13 without X10, N2-N10, V1-V7) and 5 V35-A2 controls for the fence read ABI (M1a-M1e). Since V35-A2 (M2) it runs
unchanged on an LF or a CRLF (`core.autocrlf=true`) checkout. Each control declares the validator check it must trigger; every control fails
the validator through its declared check and the unmodified baseline passes
(`docs/automation/evidence/I-52-v35-negative-controls.json`). RC-01..RC-14 closure, with rules, controls, affected rows
and remaining assumptions per RC, is in `docs/automation/evidence/I-52-v35-a1-rc-closure.json`. The V35 pre-publication review
(6 BLOCKING, 13 MAJOR, 5 MINOR, all corrected) is recorded in the V35 validation evidence at `0c609269`.

## 6. V35-A1 amendment (Architect RC-01..RC-14)

| RC | Closure | Main identifiers | Validator rules |
|---|---|---|---|
| RC-01 | FINISH is issued only by the idle/quiescence gate after the row's exact token set; timeout and FINISH-EARLY run DRAIN mode (UNKNOWN); the script contains no FINISH/QUIT | `FIN-GATE-01`, `CMD-FINISH`, `DRIVER-SCRIPT-01`, 17 `TOK-*`, column `CompletionTokenIds`, `UNK-FINISH-TIMEOUT` | FINISH-TOKEN-SET, FINISH-ORDER, FINISH-TIMEOUT, FINISH-DRAIN |
| RC-02 | MARK-LOCK-RELEASE binds per stage: for command stages, the first unlocked transition after commandEnded and before the next commandWillStart (FINISH closes the window); for APPCTX stages, the lock-mode change emitted by the stage's own APPCTX-UNLOCK-01 call, before the stage token (13A-SM: not necessarily to unlocked) | `LOCK-RELEASE-BIND-01`, `OBS-LOCK-RELEASE-BOUND`, `UNK-MARKER-BINDING` | LOCK-RELEASE-BINDING, MARKER-STAGE-BINDING |
| RC-03 | every MARK-*/N-ED-* marker binds one governed stage; DRIVER-APP-01 has its own marker namespace | column `MarkerStageBindings`, `MARKER-STAGE-BIND-01`, `STG-*`, `MARK-DRIVER-APP-*`, `LOG-RECORD-01` | MARKER-STAGE-BINDING, DRIVER-MARKER-DISJOINT, ASYNC-BOUNDARY-BINDING |
| RC-04 | V34 §5 safety exception restored on every row and evaluated before UNKNOWN; fenced retention is log-only | `FP-CLEANUP-SAFETY`, `SAFETY-EVIDENCE-COMPLETE`, `FENCED-RETENTION-SAFE` | SAFETY-PRECEDENCE |
| RC-05 | result rule in five ordered steps, closed EVIDENCE-COMPLETE, PASS-T/FAIL-T on the same evidence model, result computed outside the process | `RESULT-RULE-V35`, `EVIDENCE-COMPLETE`, `CONTROL-PLANE-RESULT-01`, `PASS-T`, `FAIL-T` | SAFETY-PRECEDENCE, CONTROL-PLANE-RESULT |
| RC-06 | every V34 ContractId carries RETAINED-UNCHANGED / RETAINED-EXTENDED / SUPERSEDED / UNUSED; APPCTX-LOCK-01 extended to APPCTX entry; T-FRESH superseded by T_CB | `retainedStatus` | RETAINED-AUTHORITY-STATUS, RETAINED-V34-DRIFT, RETAINED-EXTENSION-COMPATIBILITY |
| RC-07 | managed host module classified RuntimeModule with source, toolchain, identity, load and fence; one process-wide sequencer | `R-MANAGED-OBSERVER`, `R-NATIVE-ARX`, `LOG-SEQ-01`, `LOG-RECORD-01`, `UNK-LOG-BINDING` | MANAGED-HOST, RESOURCE-TYPE, LOG-SEQUENCE |
| RC-08 | payload project, toolchain, identity, interface, C15 handshake and exact working-database binding | `R-PAYLOAD-ARX`, `PAYLOAD-DB-BINDING`, `OBS-PAYLOAD-DB-BINDING`, `UNK-PAYLOAD-DB` | PAYLOAD-DB-BINDING |
| RC-09 | cancel trigger stages `HFV35:XR:CANCEL-STAGED` before cancel() and proves restoration | `TRG-CANCEL-OPEN-XR`, `OBS-CANCEL-RESTORED`, `UNK-CANCEL-STAGING` | CANCEL-STAGING-DISTINCT, CANCEL-STAGING-NOT-EXPECTED, CANCEL-STAGING-RESTORED |
| RC-10 | 02NAPP-SM appends through T-PRIMARY so MUT-SM can write Model Space in the callback | `TRG-APPEND-TRIGGER-IN-PRIMARY` | APPEND-TRANSACTION-COMPATIBILITY |
| RC-11 | key schema per guard and per RG-ONESHOT family (module path, request, command, notifier) | `keySchema` | GUARD-KEY-SCHEMA, GUARD-KEY-TARGET, GUARD-KEY-STAGE |
| RC-12 | fixed driver phase order; DISARM after the post-trigger obligations and the outcome record | `phaseOrder` | GUARD-DISARM-AFTER-OBLIGATIONS |
| RC-13 | architecture-review oracle: independent tables A-H and P (second implementation from NPM-V34, source in `eng/research/I52Ctda/oracle`) plus an approval freeze Z (canonical hash of every catalog entry and row, lineage/traceability/fixture blobs); the validator pins the oracle, its sources and the V34 inputs | `v35-oracle.json`, `oracle/*.py` | BODY-NOTIFIER, RESOURCE-TYPE, FP-APPLICABILITY, PREDICATE-SET, TRIGGER-ACTION, TRIGGER-TARGET, RETAINED-*, ASYNC-BOUNDARY-BINDING, FINISH-TOKEN-SET, PROCESS-FENCE, INPUT-PIN, CATALOG-APPROVAL, ROW-APPROVAL, TRACE-* |
| RC-14 | rows whose host context may legitimately end UNKNOWN are listed below | this section | none (documentary) |

**Pre-publication adversarial review of V35-A1.** Two read-only reviewers (validator independence; lifecycle, scheduler,
transaction and cleanup ordering) and a fix-verification reviewer attacked the draft; every BLOCKING and MAJOR finding
was corrected before publication:
- lifecycle: APPCTX lock-release anchor, no document close inside FINISH (QUIT-DISCARD plus a post-FINISH deadline),
  FINISH-FENCE-01 for late deliveries, payload reactor registered only when armed and removable through an export,
  synchronous stage owning the caller's return, cleanup records outside the safety scope;
- independence: exact per-row predicate sets and trigger in the oracle, pinned V34 inputs and oracle, RETAINED-EXTENDED
  entries carrying the V34 text verbatim and approved by a canonical hash, SA-LOCK requiring SA-UNLOCK, two-sided
  traceability, full driver phase order, controls that assert their check;
- verification: late deliveries after a recorded removal or fence still count as the V34 §5 safety contradiction,
  approval freeze of every catalog entry and row, Cleanup retained against V34, canonical setup order, exact lock and
  guard sets, non-empty lineage, traceability identity, pinned oracle sources.

The oracle and the generator still share one author; the approval hashes and pins are the author's proposal and need the
Architect's approval in the delta review.

**V35-A2 errata (Architect delta review of V35-A1: 0 BLOCKER, 0 MAJOR, 4 MINOR).**
- M1: FINISH-FENCE-01 is read through the LOG-SEQ-01 export `int32_t I52Ctda_FinishFenceIsSet(void)`; the setter is internal
  to R-NATIVE-ARX (CMD-FINISH only) and is not exported; R-PAYLOAD-ARX and R-MANAGED-OBSERVER import the read export
  (`imports`, `interface`). Only FINISH-FENCE-01, LOG-SEQ-01, R-PAYLOAD-ARX and R-MANAGED-OBSERVER changed; validator rule
  FINISH-FENCE-READ-ABI.
- M2: the negative-control harness normalizes line endings for exact text edits and runs on LF and CRLF checkouts.
- M3: this Proposal's references and §2 now agree with V35-A1 (36 fields, five-step result rule).
- M4: the RC-14 table below states its inclusion rule (structural versus defensive UNKNOWN).

**Global logging and sequencing (LOG-SEQ-01).** One process-wide log and one sequencer owned by `R-NATIVE-ARX`:
`I52Ctda_LogAppend` assigns Sequence with a single InterlockedIncrement64 inside the log critical section. The payload
binds it with GetModuleHandleW + GetProcAddress, the managed observer with `[DllImport("I52CtdaNative.arx")]`. No module
keeps its own counter. Both read FINISH-FENCE-01 through `I52Ctda_FinishFenceIsSet` (V35-A2, M1). Every record carries Sequence, ProbeId, StageId, DeliveryId, DriverId or SchedulerId, ModuleId, PID,
TID, DocumentId, DatabaseId, CommandIdentity and EventOrMarkerId (LOG-RECORD-01).

**Result authority (CONTROL-PLANE-RESULT-01).** The external research control plane classifies each row after the scratch
process exited, from the single log, the exact-PID exit evidence, the scratch-DWG integrity evidence and the cleanup and
fence records. I52CTDA_FINISH only prepares evidence and never declares PASS, FAIL or UNKNOWN.

**Rows that may legitimately classify UNKNOWN (RC-14).** Every plan below is mechanically complete; the open fact is host
behavior.

Inclusion rule (V35-A2, M4). The STRUCTURAL UNKNOWN table contains only rows where the architecture knowingly depends on a
host fact that may legitimately be unavailable even when the plan is correctly implemented: the execution context, trigger
or callback is one the Architect accepted as candidate-hostile, so UNKNOWN is an expected outcome of a correct run. It
excludes DEFENSIVE UNKNOWN predicates: `UNK-NO-WRITE-LOCK`, `UNK-ILLEGAL-CONTEXT`, `UNK-LOCK-DOC-T-LEAK` and similar
predicates on rows whose required authority is expected under normal execution (the SEND, APPCTX and CMDCTX deliveries,
02NTS-ALL, 02NRXW-ALL, 02NRXL-ALL, 10NDOC-CHANGED-SM after the lock is granted, and every other row carrying such a predicate).
Those predicates protect against unexpected host or runtime failure and are not listed; they do not make a row structurally
UNKNOWN. Applied to all 100 rows, the rule keeps exactly the 15 rows below and adds none. V35-A2 also reverified the last
column against each row: 09N-D and 10N-S/M/SM do not list UNK-NO-WRITE-LOCK, so their lock-absent path is UNKNOWN-COMMON
through CB-LOCK-01 (the V35-A1 wording named UNK-NO-WRITE-LOCK; the rows are unchanged). V35 records its absence as UNKNOWN rather than adding authority the Architect excluded (for example SA-LOCK in a
callback, B08). This does not make the architecture incomplete, and no row is claimed to reach PASS before host evidence.

| Row | Reason | Conditional host fact | UNKNOWN through |
|---|---|---|---|
| 10NDOC-WILL-SM | CB-EXEC-01 runs inside documentLockModeWillChange raised by the driver's own SA-LOCK request | a write-capable lock is observable before the request is granted | UNK-NO-WRITE-LOCK, UNK-ILLEGAL-CONTEXT |
| 02NEDW-ALL | CB-EXEC-01 runs inside commandWillStart of I52CTDA_FIXTURE | the command lock is already held at commandWillStart | UNK-NO-WRITE-LOCK, UNK-ILLEGAL-CONTEXT |
| 02NEDC-ALL | EDC-EXEC-01 runs inside commandCancelled of HFV34_CANCEL | a write-capable command lock is observable in commandCancelled | UNK-NO-WRITE-LOCK, UNK-T-CANCEL-FAILURE |
| 13A-SM | APPCTX-LOCK-01 calls lockDocument(kWrite) in a synchronous APPCTX entry while I52CTDA_PROBE holds the command lock | the nested kWrite request returns eOk | UNK-LOCK-DOC-T-LEAK, UNK-ILLEGAL-CONTEXT |
| 02NTAS-ALL | CB-EXEC-01 starts T_CB inside transactionAboutToStart | the transaction manager accepts a start from that callback | UNK-ILLEGAL-CONTEXT |
| 02NTA-ALL | CB-EXEC-01 in transactionAborted | the aborted T-PRIMARY is no longer active at the callback | UNK-NESTED-IN-ABORT |
| 02NAPP-SM | MUT-SM appends F-REF-C to Model Space inside objectAppended of the F-TRIGGER-APPEND append, in the same T-PRIMARY (RC-10) | a nested append to the same block table record is accepted inside objectAppended | UNKNOWN-COMMON (CB-PRIMARY-01 step failure) |
| 04NO-S, 04NO-UNDO-S | the primary callback is cancelled/modifyUndone produced by aborting T-PRIMARY over a staged write | the abort delivers cancelled/modifyUndone to OR-XR | OBS-PRIMARY-CALLBACK unavailable (RESULT-RULE-V35 step 2) |
| COBJUNDO16SND-ALL, COBJUNDO16APP-ALL | the origin is modifyUndone produced by cancel() after the RC-09 staging write | cancel() sends modifyUndone | OBS-ORIGIN-CALLBACK unavailable (step 2) |
| 09N-D | CB-EXEC-01 in commandEnded of I52CTDA_FIXTURE | the command lock is still held at commandEnded | UNKNOWN-COMMON (CB-LOCK-01 lock unavailable, no mutation; RESULT-RULE-V35 step 2) |
| 10N-S, 10N-M, 10N-SM | CB-EXEC-01 in commandEnded of I52CTDA_FIXTURE; candidate boundary (B12) | the command lock is still held at commandEnded | UNKNOWN-COMMON (CB-LOCK-01 lock unavailable, no mutation) and OBS-CANDIDATE-BOUNDARY = UNAVAILABLE (step 2) |

## 7. Non-claims

V35 is documentation, catalog and a static validator. Nothing native was written or built; the executor, the payload
module and the managed observer do not exist yet; AutoCAD was not started; no ProbeId was executed. `fixture-v34.json`,
`traceability-v34.json` and `ProbeDispatchTable.inc` are unchanged; `fixture-v35.json` and `traceability-v35.json` are
new files that no build consumes yet. The canonical ARX (`7F9C9C05…9ED421`) and build tuple (`375B3D5C…46E25`) remain
valid. Validator TRUE values mean the contract is mechanically closed as written; they do not mean any probe can PASS.

```text
V35 = READY FOR ARCHITECT DELTA REVIEW (V35-A1)
R3 IMPLEMENTATION = BLOCKED
GOVERNING PROBES EXECUTED = 0
CTDA_HOST_PASS = NOT EVALUATED
CURRENT ARX = STILL CANONICAL
PRODUCT DIFF = NONE
```
