# I-52 — Proposal V35 — mechanically executable probe matrix

> **DRAFT / ARCHITECTURE CANDIDATE / NOT EXECUTED.** Baseline I-52 `5c4521c7d8f51e96f128188fd4f52b6437ca9bce`
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

- **Execution catalog.** `I-52-execution-catalog-v35.json` (EXEC-CATALOG-V35-1) is the single normative source for every
  execution identifier: run environment and drivers, resources, observer registrations, guards, setup and trigger
  actions, scheduler chains, execution contexts, transaction/top/lock authorities, mutation actions, observation, UNKNOWN
  and FAIL predicates, result classes, cleanup steps and actions, completion fences and surfaces. Retained V34
  ContractIds, the 27 EventIds, 3 SchedulerIds and 7 support actions are imported as entries with their authority.
- **Row schema.** NPM-V35 rows have 34 fields instead of 24, including `BodyObserverId`, the only registration that runs
  the probe body. Setup, Trigger, transaction, lock, mutation, PASS, FAIL and
  UNKNOWN prose is replaced by identifier lists; markers carry polarity (`+` MUST_BE_PRESENT, `-` MUST_BE_ABSENT). The
  columns PrimaryAuthorityId, ScheduleOriginEventId, HeaderAuthority, Surface, SchedulePoint, ExecutionPoint,
  ExpectedBefore, ExpectedAfter, Verifier, Threats and DA-P/H-P are byte-identical to V34.
- **Lineage.** Every V34 executable clause of every row is recorded in `I-52-v35-clause-lineage.json` with class A
  (bound V34 ContractId, kept or relocated), B (natural language formalized by a catalog id) or C (explanatory clause
  enforced by a named invariant). Class D (unresolved) is zero.
- **Result rule.** `RESULT-RULE-V35`: UNKNOWN iff UNKNOWN-COMMON, a listed UnknownPredicate, or any required observation
  predicate unavailable; otherwise FAIL iff evidence is complete and a listed FailPredicate holds; otherwise PASS iff the
  PassClass, every ObservationPredicate, the fresh verifier against ExpectedAfter and the cleanup plus fence all hold.
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

## 4. Decisions introduced by V35 that need Architect confirmation

These are formalizations the Architect's decisions did not fix literally. Each is fully defined in the catalog; none
changes a frozen count. Any of them may be rejected without reopening the others. VER-SM is bound to its §164
definition (it never opens F-REF-B), which is a binding already in force, not a new decision.

1. **02NO-SM cleanup (`CLEAN-OBJ-FENCED`).** The trigger erases F-REF-B and §164 forbids reopening B, so OR-REF-B cannot
   be removed with CLN-OBJ-REMOVE. V35 retains the reactor instance unreleased, never touches the notifier and requires
   `FENCE-PROCESS-EXIT`.
2. **`RR-MANAGED-CMD`.** MARK-MANAGED-CMD-END (09N-D, 10N-S/M/SM) is a managed `Document.CommandEnded` observation
   (HEC-V27-C1, row 09E, M-DOC-END). V35 models it as an ObserverRegistration hosted by a managed observer loaded by BOOT
   for the scratch document and writing into the same log. This is a new build artifact for R3.
3. **Driver APPCTX delivery (`DRIVER-APP-01`).** B11 places the lock trigger in a driver-owned APPCTX delivery. V35 obtains
   it with `beginExecuteInApplicationContext`, classified `DRIVER-INFRA` (12 rows: nine lock rows and 16C-S/M/SM) with no
   scheduler credit and no NS pair. 16C uses it so that `beginExecuteInCommandContext` cancels no driver command.
4. **One process per ProbeId (`RUN-ENV-01`).** V34 required a dedicated process for some rows; V35 requires it for all 100,
   with `DRIVER-SCRIPT-01` (BOOT, PROBE, optional FIXTURE/HFV34_CANCEL, FINISH) and no human input.
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

## 5. Mechanical result

The validator parses NPM-V34 and NPM-V35, the catalog, NEC-V34/V35, fixture and traceability files, the lineage
evidence and the R3 discovery evidence. Its result for this draft is recorded in
`docs/automation/evidence/I-52-v35-mechanical-validation.json`:

```text
INPUT ROWS = 100 / PARSED = 100 / DERIVED PLANS = 100
UNBOUND IDS = 0 / EXECUTABLE FREE-TEXT = 0 / CLASS-D = 0 / CONTRADICTIONS = 0 / LINEAGE MISSING = 0
EVENT RELATIONS = 237 (MARKER_ABSENT_FOR = 2) / SCHEDULER PAIRS = 63 / PRIMARY COVERAGE = 26/26
CALLBACKS = 26 / SCHEDULERS = 3 / SUPPORT ACTIONS = 7 / FIXTURE IDENTITIES = 8
OPEN BLOCKER GROUPS = 0 (R3-B01..B12, R3-C02)
REFERENCED OBJECTARX MEMBERS = 19 / MISSING IN EXACT SDK HEADERS = 0
ExecutionCatalogClosed, TriggerAuthorityClosed, SetupAuthorityClosed, PredicateAuthorityClosed,
ResourceAuthorityClosed, FixtureExecutionContractClosedV35, PlanDerivationClosed = TRUE (validator)
```

The derivation rule used for NEC-V35 reproduces NEC-V34's 237 relations exactly when applied to NPM-V34. Negative
controls: 24 deliberate corruptions of the published files (free text, "or NONE", dropped observer, wrong guard, reverted
veto polarity, duplicated CLN-BASE, ninth identity, dropped NEC relation, marker without producer, second body observer,
dropped staging UNKNOWN, among others) each make the validator fail.

Before publication, two independent read-only adversarial reviewers attacked the draft (execution semantics against the
exact SDK headers; V34 fidelity, results, cleanup and lineage). They reported 6 BLOCKING, 13 MAJOR and 5 MINOR findings,
3 of them overlapping. All were corrected, and the validator gained the checks that would have caught them; the list
is in the validation evidence (`prePublicationReview`).

## 6. Non-claims

V35 is documentation, catalog and a static validator. Nothing native was written or built; the executor, the payload
module and the managed observer do not exist yet; AutoCAD was not started; no ProbeId was executed. `fixture-v34.json`,
`traceability-v34.json` and `ProbeDispatchTable.inc` are unchanged; `fixture-v35.json` and `traceability-v35.json` are
new files that no build consumes yet. The canonical ARX (`7F9C9C05…9ED421`) and build tuple (`375B3D5C…46E25`) remain
valid. Validator TRUE values mean the contract is mechanically closed as written; they do not mean any probe can PASS.

```text
V35 GOVERNANCE = ARCHITECTURE CANDIDATE
R3 IMPLEMENTATION = BLOCKED PENDING ARCHITECT REVIEW
GOVERNING PROBES EXECUTED = 0
CTDA_HOST_PASS = NOT EVALUATED
CURRENT ARX = STILL CANONICAL
PRODUCT DIFF = NONE
```
