# I-52 — Fixture Execution Contract V35

> **FEC-V35-1 / DRAFT / ARCHITECTURE CANDIDATE / NOT EXECUTED.** Parent FEC-V34-1 blob `9976806e301d3f060a8d99bd0fa12ecfa90e44cb`. V35 keeps the seven
> support actions and adds the resource taxonomy, the eighth fixture identity and the explicit legal contexts required by the
> Architect R3 review (B01-B12, C02).

## 1. Support-action taxonomy

The seven support actions are unchanged as identifiers and members. V35 states their legal contexts against catalog ids.

| ActionId | Exact member | V35 legal context | Delta |
|---|---|---|---|
| `SA-VETO` | AcApDocManagerReactor::veto | derived RR-DOC during its documentLockModeWillChange callback for the exact TRG-LOCK-VETO request (SET-ARM-VETO) | unchanged |
| `SA-LOCK` | AcApDocManager::lockDocument | application context with exact target document: APPCTX-EXEC-01 (APPCTX-LOCK-01) in an asynchronous NS-BEGIN-APPCTX delivery or, for 13A-SM only, in the synchronous NX-APPCTX-SYNC callback that NPM-V34 already binds to APPCTX-EXEC-01; or DRIVER-APP-01 delivery (DRIVER-LOCK-01) | extended by Architect B11 to the driver-owned APPCTX delivery; 13A-SM synchronous entry made explicit |
| `SA-UNLOCK` | AcApDocManager::unlockDocument | after owned T_APP ended/aborted (APPCTX-UNLOCK-01), or in DRIVER-APP-01 after the TRG-LOCK-CYCLE/TRG-LOCK-VETO lock step, where the driver opens no T | extended by Architect B11 (explicit driver unlock) |
| `SA-TX-RESOLVE` | AcApDocument::transactionManager | after exact document resolution | unchanged |
| `SA-TX-START` | AcDbTransactionManager::startTransaction | after legal lock/context: BOOT-01 or DRIVER-CMD-01 observed host command lock, APPCTX-LOCK-01 eOk, CMDCTX-LOCK-01, SEND-LOCK-01, EDC-LOCK-01 or CB-LOCK-01 observed write-capable lock | made explicit by Architect B08 |
| `SA-TX-END` | AcDbTransactionManager::endTransaction | owned active T success path | unchanged |
| `SA-TX-ABORT` | AcDbTransactionManager::abortTransaction | owned incomplete/abort path, or TRG-ABORT-PRIMARY-T on driver-owned T-PRIMARY | unchanged; trigger use made explicit |

Support actions are neither EventIds nor SchedulerIds. Count = 7; unresolved = 0.

## 2. Resource taxonomy

Every resource used by a row is one of six kinds. Only `PersistentFixtureIdentity` counts as a fixture identity (8).

| ResourceId | Kind | Definition |
|---|---|---|
| `F-TRIGGER-MOD` | PersistentFixtureIdentity | database modification and open-for-modify trigger; notifier A of 02N. Object: AcDbPoint in Model Space at (1000,0,0) on layer 0. Initial: live, position (1000,0,0). |
| `F-TRIGGER-ERASE-DB` | PersistentFixtureIdentity | database erasure trigger. Object: AcDbPoint in Model Space at (1010,0,0) on layer 0. Initial: live, not erased. |
| `F-TRIGGER-ERASE-OBJ` | PersistentFixtureIdentity | object-reactor erasure trigger. Object: AcDbPoint in Model Space at (1020,0,0) on layer 0. Initial: live, not erased. |
| `F-TRIGGER-XR` | PersistentFixtureIdentity | semantic notifier A of 02NO-S; distinct from F-XR. Object: AcDbXrecord in NOD key RACKCAD_CTDA_V35_F-TRIGGER-XR, one kDxfText resbuf. Initial: bytes HFV35:TRIGGER-XR:0. |
| `F-XR` | PersistentFixtureIdentity | protected semantic Xrecord (MUT-S target). Object: AcDbXrecord in NOD key RACKCAD_CTDA_V34_F-XR. Initial: STATE-S-0. |
| `F-REF-B` | PersistentFixtureIdentity | protected material reference (MUT-M target). Object: AcDbBlockReference of RACKCAD_CTDA_V34_REF. Initial: STATE-M-0. |
| `F-REF-A` | PersistentFixtureIdentity | protected mixed sibling A. Object: AcDbBlockReference of RACKCAD_CTDA_V34_REF. Initial: STATE-SM-0 member. |
| `F-REF-C` | PersistentFixtureIdentity | protected mixed sibling C, declared absent. Object: AcDbBlockReference of RACKCAD_CTDA_V34_REF appended by MUT-SM. Initial: absent. |
| `R-SM-LINK` | StateCarrier | V34 binding §164: single AcDbXrecord in NOD key RACKCAD_CTDA_V34_SM-LINK, one kDxfText resbuf HFV30:LINK:A,B (SM-0) / HFV30:LINK:A,C (SM-1). Not a fixture identity. Created by BOOT-01; removed by CLN-BASE (LINK-CARRIER-REMOVED). |
| `F-TRIGGER-APPEND` | DisposableTriggerResource | Architect B01: not a fixture identity. Created only by TRG-APPEND-TRIGGER as a new AcDbPoint at (1030,0,0) on layer 0 appended to Model Space outside the sibling set; its in-memory instance arms RG-DB-APPEND; it produces N-DB-APPEND; CLN-BASE erases it. |
| `R-PAYLOAD-ARX` | PayloadResource | Architect B03: second research module I52CtdaPayload.arx, built with the research ARX and recorded by SHA-256 in the build tuple. NL-ARX-PATH = absolute path <RunDir>\I52CtdaPayload.arx of the dedicated run directory. Loaded only with acedArxLoad(NL-ARX-PATH). On kInitAppMsg it registers RR-PAYLOAD-DB on the scratch database and appends one MARK-REGISTRATION record; its reactor applies RG-DB-MOD semantics and acts only when the main module exported the C15 arm record, otherwise it is inert. Load success = acedArxLoad returned RTNORM, the module is reported loaded by the dynamic linker, and N-RX-LOADED names the exact module. No in-process unload is ever attempted; process exit is the residency fence. |
| `OR-XR` | ObserverRegistration | transient object reactor on F-XR. Registration: open the notifier with acdbOpenObject kForRead, addReactor(exact retained instance), close; non-eOk is UNKNOWN. The instance is retained until its cleanup step; its callbacks always log; they run the probe body only when the row names it as BodyObserverId and the body guard is armed; otherwise the registration is observation-only. |
| `OR-TXR` | ObserverRegistration | transient object reactor on F-TRIGGER-XR (semantic notifier A of 02NO-S). Registration: open the notifier with acdbOpenObject kForRead, addReactor(exact retained instance), close; non-eOk is UNKNOWN. The instance is retained until its cleanup step; its callbacks always log; they run the probe body only when the row names it as BodyObserverId and the body guard is armed; otherwise the registration is observation-only. |
| `OR-REF-A` | ObserverRegistration | transient object reactor on F-REF-A. Registration: open the notifier with acdbOpenObject kForRead, addReactor(exact retained instance), close; non-eOk is UNKNOWN. The instance is retained until its cleanup step; its callbacks always log; they run the probe body only when the row names it as BodyObserverId and the body guard is armed; otherwise the registration is observation-only. |
| `OR-REF-B` | ObserverRegistration | transient object reactor on F-REF-B. Registration: open the notifier with acdbOpenObject kForRead, addReactor(exact retained instance), close; non-eOk is UNKNOWN. The instance is retained until its cleanup step; its callbacks always log; they run the probe body only when the row names it as BodyObserverId and the body guard is armed; otherwise the registration is observation-only. |
| `OR-TEO` | ObserverRegistration | transient object reactor on F-TRIGGER-ERASE-OBJ. Registration: open the notifier with acdbOpenObject kForRead, addReactor(exact retained instance), close; non-eOk is UNKNOWN. The instance is retained until its cleanup step; its callbacks always log; they run the probe body only when the row names it as BodyObserverId and the body guard is armed; otherwise the registration is observation-only. |
| `ER-REF-A` | ObserverRegistration | transient entity reactor on F-REF-A. Registration: open the notifier with acdbOpenObject kForRead, addReactor(exact retained instance), close; non-eOk is UNKNOWN. The instance is retained until its cleanup step; its callbacks always log; they run the probe body only when the row names it as BodyObserverId and the body guard is armed; otherwise the registration is observation-only. |
| `RR-DB` | ObserverRegistration | database reactor on the scratch database (main research module). Registration: add the exact retained instance to its notifier; non-success is UNKNOWN. The instance is retained until its cleanup step; its callbacks always log; they run the probe body only when the row names it as BodyObserverId and the body guard is armed; otherwise the registration is observation-only. |
| `RR-TX` | ObserverRegistration | transaction reactor on the scratch database transaction manager. Registration: add the exact retained instance to its notifier; non-success is UNKNOWN. The instance is retained until its cleanup step; its callbacks always log; they run the probe body only when the row names it as BodyObserverId and the body guard is armed; otherwise the registration is observation-only. |
| `RR-ED` | ObserverRegistration | editor reactor filtered to the scratch document. Registration: add the exact retained instance to its notifier; non-success is UNKNOWN. The instance is retained until its cleanup step; its callbacks always log; they run the probe body only when the row names it as BodyObserverId and the body guard is armed; otherwise the registration is observation-only. |
| `RR-DOC` | ObserverRegistration | document-manager reactor filtered to the scratch document. Registration: add the exact retained instance to its notifier; non-success is UNKNOWN. The instance is retained until its cleanup step; its callbacks always log; they run the probe body only when the row names it as BodyObserverId and the body guard is armed; otherwise the registration is observation-only. |
| `RR-DLINK` | ObserverRegistration | dynamic-linker reactor. Registration: add the exact retained instance to its notifier; non-success is UNKNOWN. The instance is retained until its cleanup step; its callbacks always log; they run the probe body only when the row names it as BodyObserverId and the body guard is armed; otherwise the registration is observation-only. |
| `RR-PAYLOAD-DB` | ObserverRegistration | database reactor registered by R-PAYLOAD-ARX at its load (C15 origin). Registration: registered by R-PAYLOAD-ARX at kInitAppMsg. The instance is retained until its cleanup step; its callbacks always log; they run the probe body only when the row names it as BodyObserverId and the body guard is armed; otherwise the registration is observation-only. |
| `RR-MANAGED-CMD` | ObserverRegistration | managed CommandEnded observer of HEC-V27-C1 row 09E (M-DOC-END), hosted by the managed research module I52CtdaManagedObserver.dll loaded by RUN-ENV-01 at process start; the driver subscribes it for the scratch document through the module's exported registration entry when the row lists it; it writes into the same append-only log. Registration: driver subscription through the managed module; failure is UNKNOWN. The instance is retained until its cleanup step; its callbacks always log; they run the probe body only when the row names it as BodyObserverId and the body guard is armed; otherwise the registration is observation-only. |
| `CMD-BOOT` | CommandResource | Command `I52CTDA_BOOT` registered by the research ARX at load: modal; runs BOOT-01. |
| `CMD-PROBE` | CommandResource | Command `I52CTDA_PROBE` registered by the research ARX at load: modal; runs DRIVER-CMD-01 or the enqueue half of DRIVER-APP-01. |
| `CMD-FIXTURE` | CommandResource | Command `I52CTDA_FIXTURE` registered by the research ARX at load: modal; exact controlled successful fixture command; body performs no database write and returns. |
| `CMD-CANCEL` | CommandResource | Command `HFV34_CANCEL` registered by the research ARX at load: modal; exact controlled command of CANCEL-CMDCTX-01. |
| `CMD-QUEUED` | CommandResource | Command `I52CTDA_QUEUED` registered by the research ARX at load: modal; the only NS-SEND target; body is SEND-EXEC-01 over the retained callback data. |
| `CMD-FINISH` | CommandResource | Command `I52CTDA_FINISH` registered by the research ARX at load: modal; checks completion tokens, runs VerifierIds, runs CleanupActionId, flushes the log. |

`fixture-v35.json` lists the eight identities; `traceability-v35.json` lists, per ProbeId, every resource with its kind.

## 3. Drivers, triggers and execution contexts

`RUN-ENV-01`, `BOOT-01`, `DRIVER-SCRIPT-01`, `DRIVER-CMD-01` and `DRIVER-APP-01` fix how a row starts. Setup, trigger,
guard, scheduler chain, execution context, transaction, lock, cleanup and fence identifiers are defined only in
EXEC-CATALOG-V35-1 (`I-52-execution-catalog-v35.json`, rendered in `I-52-execution-catalog-v35.md`). The veto and
cancellation causal graphs of FEC-V34 §3 are unchanged; the lock path now starts from the driver-owned APPCTX delivery
(Architect B11).

## 4. Closure predicate

`FixtureExecutionContractClosedV35` holds iff the validator `eng/research/I52Ctda/validate-v35-catalog.ps1` reports, for
the exact published blobs: 100 parsed rows, 100 derived plans, 0 unbound identifiers, 0 executable free-text clauses,
0 class-D clauses, 0 contradictions, 0 open blocker groups and the frozen counts 100/26/3/7/8. Unlike FEC-V34 §5, the
predicate is never asserted by definition; its value is the validator's output recorded in
`docs/automation/evidence/I-52-v35-mechanical-validation.json`.
