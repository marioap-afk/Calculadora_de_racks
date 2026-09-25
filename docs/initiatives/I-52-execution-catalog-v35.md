# I-52 — Execution Catalog V35

> **EXEC-CATALOG-V35-1 / DRAFT / ARCHITECTURE CANDIDATE / NOT EXECUTED.** Human-readable render of
> `I-52-execution-catalog-v35.json`, which is the normative source. Every identifier used by NPM-V35 resolves to exactly one
> entry. Retained V34 ContractIds (NPM-V34 §2-§4, §7, §8), the 27 EventIds, 3 SchedulerIds and 7 support actions are imported
> as entries of the JSON catalog with their authority.

## 1. Run environment and drivers

| Id | Kind | Definition |
|---|---|---|
| `RUN-ENV-01` | RunEnvironment | Every ProbeId execution runs in its own dedicated scratch AutoCAD 2025 process (one ProbeId per PID), never in the user's active session. The process opens a scratch DWG created for the run, loads the canonical research ARX, executes DRIVER-SCRIPT-01 and exits. The append-only event log records a monotonic sequence number, thread id, document/application context, database identity and transaction count for every observation. Process exit is externally verified by exact PID. |
| `BOOT-01` | Bootstrap | Script command I52CTDA_BOOT (CMD-BOOT) materializes, in one transaction, the eight persistent fixture identities of fixture-v35.json (F-REF-C declared absent and not created) and the StateCarrier R-SM-LINK, reaching STATE-ALL-0 plus the trigger initial states. Any duplicate, missing or failed object is UNKNOWN and no trigger runs. BOOT-01 performs no observer registration. (usesSupport: SA-TX-RESOLVE, SA-TX-START, SA-TX-END, SA-TX-ABORT) |
| `DRIVER-SCRIPT-01` | DriverScript | Fixed script order: `I52CTDA_BOOT`; `I52CTDA_PROBE <ProbeId>`; then `I52CTDA_FIXTURE` only when TriggerActionId is TRG-RUN-FIXTURE-CMD, or `HFV34_CANCEL` only when TriggerActionId is TRG-CANCEL-CMDCTX; then `I52CTDA_FINISH <ProbeId>`; then close the scratch DWG without save and exit. No keyboard or human input. I52CTDA_FINISH requires every completion token of the row; a missing token is UNKNOWN (no waiting, no retry). The script is not a scheduler and grants no scheduler credit. |
| `DRIVER-CMD-01` | Driver | I52CTDA_PROBE (CMD-PROBE, modal) runs in document command context and must observe a write-capable host command lock at entry (otherwise UNKNOWN, nothing is executed). It registers the row's ObserverRegistrationIds in catalog order (except RR-PAYLOAD-DB, which R-PAYLOAD-ARX registers itself during SET-LOAD-PAYLOAD), executes the row's SetupActionIds in order, arms the row's GuardIds (a guard whose armTarget is a resource created by the trigger, RG-DB-APPEND, is armed by that trigger immediately before the append), executes TriggerActionId, runs post-trigger obligations fixed by setup and returns. Guards are disarmed in finally when TriggerActionId returns, except for the script-issued triggers TRG-RUN-FIXTURE-CMD and TRG-CANCEL-CMDCTX: their guards stay armed after I52CTDA_PROBE returns, their callback identity is bound to the exact command name of markerBinding (I52CTDA_FIXTURE, HFV34_CANCEL) and I52CTDA_FINISH disarms them. The driver exclusively owns T-PRIMARY/T-NESTED/T-CONTROLLED when setup or trigger creates them. (produces: N-ED-WILL, N-ED-END) |
| `DRIVER-APP-01` | Driver | I52CTDA_PROBE registers observers, runs setup and then calls beginExecuteInApplicationContext(driverBody, data) once (driver infrastructure, SchedulerUse=DRIVER-INFRA, no scheduler credit, no NS pair). Non-eOk enqueue is UNKNOWN. At delivery isApplicationContext()==true must be observed (otherwise UNKNOWN); driverBody resolves the exact scratch document D, arms guards, executes TriggerActionId, disarms guards in finally and emits the DRIVER-APP completion token. (produces: N-ED-WILL, N-ED-END) |

## 2. Superseded retained contracts

| Id | Kind | Definition |
|---|---|---|
| `VER-SM` | RetainedContract | V34 binding §164 (supersedes the NPM-V34 §2 wording 'VER-S + VER-M + enumeration'): fresh transaction; read R-SM-LINK by its NOD key and cross-check its bound ObjectId; read F-REF-A and F-REF-C freshly and record their Matrix3d/LayerId as evidence; enumerate Model Space again without opening F-REF-B; SmRules classifies (UNKNOWN / FAIL-SM / OK). VER-SM never opens F-REF-B; only VER-M reads F-REF-B. |

## 3. Resources

| Id | Kind | Definition |
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
| `CMD-BOOT` | CommandResource | Command `I52CTDA_BOOT` registered by the research ARX at load: modal; runs BOOT-01. |
| `CMD-PROBE` | CommandResource | Command `I52CTDA_PROBE` registered by the research ARX at load: modal; runs DRIVER-CMD-01 or the enqueue half of DRIVER-APP-01. |
| `CMD-FIXTURE` | CommandResource | Command `I52CTDA_FIXTURE` registered by the research ARX at load: modal; exact controlled successful fixture command; body performs no database write and returns. |
| `CMD-CANCEL` | CommandResource | Command `HFV34_CANCEL` registered by the research ARX at load: modal; exact controlled command of CANCEL-CMDCTX-01. |
| `CMD-QUEUED` | CommandResource | Command `I52CTDA_QUEUED` registered by the research ARX at load: modal; the only NS-SEND target; body is SEND-EXEC-01 over the retained callback data. |
| `CMD-FINISH` | CommandResource | Command `I52CTDA_FINISH` registered by the research ARX at load: modal; checks completion tokens, runs VerifierIds, runs CleanupActionId, flushes the log. |

## 4. Observer registrations

| Id | Kind | Definition |
|---|---|---|
| `OR-XR` | ObserverRegistration | transient object reactor on F-XR. Registration: open the notifier with acdbOpenObject kForRead, addReactor(exact retained instance), close; non-eOk is UNKNOWN. The instance is retained until its cleanup step; its callbacks always log; they run the probe body only when the row names it as BodyObserverId and the body guard is armed; otherwise the registration is observation-only. (reactorClass: AcDbObjectReactor; notifier: F-XR) |
| `OR-TXR` | ObserverRegistration | transient object reactor on F-TRIGGER-XR (semantic notifier A of 02NO-S). Registration: open the notifier with acdbOpenObject kForRead, addReactor(exact retained instance), close; non-eOk is UNKNOWN. The instance is retained until its cleanup step; its callbacks always log; they run the probe body only when the row names it as BodyObserverId and the body guard is armed; otherwise the registration is observation-only. (reactorClass: AcDbObjectReactor; notifier: F-TRIGGER-XR) |
| `OR-REF-A` | ObserverRegistration | transient object reactor on F-REF-A. Registration: open the notifier with acdbOpenObject kForRead, addReactor(exact retained instance), close; non-eOk is UNKNOWN. The instance is retained until its cleanup step; its callbacks always log; they run the probe body only when the row names it as BodyObserverId and the body guard is armed; otherwise the registration is observation-only. (reactorClass: AcDbObjectReactor; notifier: F-REF-A) |
| `OR-REF-B` | ObserverRegistration | transient object reactor on F-REF-B. Registration: open the notifier with acdbOpenObject kForRead, addReactor(exact retained instance), close; non-eOk is UNKNOWN. The instance is retained until its cleanup step; its callbacks always log; they run the probe body only when the row names it as BodyObserverId and the body guard is armed; otherwise the registration is observation-only. (reactorClass: AcDbObjectReactor; notifier: F-REF-B) |
| `OR-TEO` | ObserverRegistration | transient object reactor on F-TRIGGER-ERASE-OBJ. Registration: open the notifier with acdbOpenObject kForRead, addReactor(exact retained instance), close; non-eOk is UNKNOWN. The instance is retained until its cleanup step; its callbacks always log; they run the probe body only when the row names it as BodyObserverId and the body guard is armed; otherwise the registration is observation-only. (reactorClass: AcDbObjectReactor; notifier: F-TRIGGER-ERASE-OBJ) |
| `ER-REF-A` | ObserverRegistration | transient entity reactor on F-REF-A. Registration: open the notifier with acdbOpenObject kForRead, addReactor(exact retained instance), close; non-eOk is UNKNOWN. The instance is retained until its cleanup step; its callbacks always log; they run the probe body only when the row names it as BodyObserverId and the body guard is armed; otherwise the registration is observation-only. (reactorClass: AcDbEntityReactor; notifier: F-REF-A) |
| `RR-DB` | ObserverRegistration | database reactor on the scratch database (main research module). Registration: add the exact retained instance to its notifier; non-success is UNKNOWN. The instance is retained until its cleanup step; its callbacks always log; they run the probe body only when the row names it as BodyObserverId and the body guard is armed; otherwise the registration is observation-only. (reactorClass: AcDbDatabaseReactor; notifier: scratch database) |
| `RR-TX` | ObserverRegistration | transaction reactor on the scratch database transaction manager. Registration: add the exact retained instance to its notifier; non-success is UNKNOWN. The instance is retained until its cleanup step; its callbacks always log; they run the probe body only when the row names it as BodyObserverId and the body guard is armed; otherwise the registration is observation-only. (reactorClass: AcTransactionReactor; notifier: scratch transaction manager) |
| `RR-ED` | ObserverRegistration | editor reactor filtered to the scratch document. Registration: add the exact retained instance to its notifier; non-success is UNKNOWN. The instance is retained until its cleanup step; its callbacks always log; they run the probe body only when the row names it as BodyObserverId and the body guard is armed; otherwise the registration is observation-only. (reactorClass: AcEditorReactor; notifier: editor) |
| `RR-DOC` | ObserverRegistration | document-manager reactor filtered to the scratch document. Registration: add the exact retained instance to its notifier; non-success is UNKNOWN. The instance is retained until its cleanup step; its callbacks always log; they run the probe body only when the row names it as BodyObserverId and the body guard is armed; otherwise the registration is observation-only. (reactorClass: AcApDocManagerReactor; notifier: document manager) |
| `RR-DLINK` | ObserverRegistration | dynamic-linker reactor. Registration: add the exact retained instance to its notifier; non-success is UNKNOWN. The instance is retained until its cleanup step; its callbacks always log; they run the probe body only when the row names it as BodyObserverId and the body guard is armed; otherwise the registration is observation-only. (reactorClass: AcRxDLinkerReactor; notifier: dynamic linker) |
| `RR-PAYLOAD-DB` | ObserverRegistration | database reactor registered by R-PAYLOAD-ARX at its load (C15 origin). Registration: registered by R-PAYLOAD-ARX at kInitAppMsg. The instance is retained until its cleanup step; its callbacks always log; they run the probe body only when the row names it as BodyObserverId and the body guard is armed; otherwise the registration is observation-only. (reactorClass: AcDbDatabaseReactor; notifier: scratch database) |
| `RR-MANAGED-CMD` | ObserverRegistration | managed CommandEnded observer of HEC-V27-C1 row 09E (M-DOC-END), hosted by the managed research module I52CtdaManagedObserver.dll loaded by RUN-ENV-01 at process start; the driver subscribes it for the scratch document through the module's exported registration entry when the row lists it; it writes into the same append-only log. Registration: driver subscription through the managed module; failure is UNKNOWN. The instance is retained until its cleanup step; its callbacks always log; they run the probe body only when the row names it as BodyObserverId and the body guard is armed; otherwise the registration is observation-only. (reactorClass: Document.CommandEnded (managed); notifier: scratch document) |

## 5. Guards

| Id | Kind | Definition |
|---|---|---|
| `RG-DB-MOD` | Guard | V34 §8 unchanged: arm only for exact F-TRIGGER-MOD ObjectId; log nested notifications, suppress probe-body re-entry; other recursion UNKNOWN; disarm in finally. (family: N-DB-MOD) |
| `RG-DB-ERASE` | Guard | V34 §8 unchanged: arm only for disposable F-TRIGGER-ERASE-DB; protected targets stay live; suppress body re-entry; unexpected recursion UNKNOWN. (family: N-DB-ERASE) |
| `RG-OBJ-ERASE` | Guard | V34 §8 unchanged: same one-shot rule for disposable F-TRIGGER-ERASE-OBJ; no protected target is erased. (family: N-OBJ-ERASE) |
| `RG-DB-APPEND` | Guard | Architect B01: armed with the in-memory instance pointer of F-TRIGGER-APPEND before it is appended; the body runs once for objectAppended of that exact instance; other appends are logged only; recursion into the body is UNKNOWN; disarm in finally. (family: N-DB-APPEND) |
| `RG-DB-OPEN` | Guard | Architect B02: arm only for objectOpenedForModify of the exact ObjectId targeted by the row's TriggerActionId (F-TRIGGER-MOD in 02N; F-XR in CDBOPEN16SND-ALL/CDBOPEN16APP-ALL, as NPM-V34 names it); one body execution; nested notifications logged, body re-entry suppressed; other recursion UNKNOWN; disarm in finally. (family: N-DB-OPEN) |
| `RG-TX` | Guard | Architect B08: one-shot for the exact transaction-reactor callback identity of the row; notifications caused by the body's own actions (T_CB lifecycle, writes) are logged and never re-enter the body; any other re-entry into the body is UNKNOWN; disarm per DRIVER-CMD-01. (family: N-TR-) |
| `RG-ONESHOT` | Guard | V35 generic one-shot for any other body-running callback identity (object, entity, editor, document, dynamic-linker, payload database reactor, and the SA-VETO body). One guard instance exists per armed callback identity = (retained reactor instance, callback member, exact notifier, and for editor callbacks the command name bound by markerBinding); the SA-VETO body and an origin body in the same row are distinct instances. The first matching callback runs its body once; notifications caused by the body's own actions (writes, the OBS-EWASNOTIFYING attempt) are logged and never re-enter the body; later matching callbacks are logged only; any other re-entry into the body is UNKNOWN; disarm per DRIVER-CMD-01. (family: *) |

## 6. Setup actions

| Id | Kind | Definition |
|---|---|---|
| `SET-OPEN-PRIMARY-T` | SetupAction | Driver resolves the scratch document transaction manager and starts T-PRIMARY as the outermost transaction; active count must be 1 after start, otherwise UNKNOWN. (produces: N-TR-ABOUT-START, N-TR-STARTED; usesSupport: SA-TX-RESOLVE, SA-TX-START) |
| `SET-OPEN-NESTED-T` | SetupAction | Inside T-PRIMARY the driver starts T-NESTED; active count must be 2, otherwise UNKNOWN. (produces: N-TR-ABOUT-START, N-TR-STARTED; usesSupport: SA-TX-START) |
| `SET-STAGE-S-IN-PRIMARY` | SetupAction | Driver getObject(F-XR, kForWrite) through T-PRIMARY, writes exactly the MUT-S bytes HFV30:S:1 without ending T-PRIMARY (staged state) and reads them back through T-PRIMARY before the trigger (OBS-STAGED); any non-eOk or different read-back is UNKNOWN (UNK-STAGING) and no trigger runs. Replaces V34 'stage MUT-S', 'stage semantic modification' and 'modify then cancel/controlled undo'. (produces: N-DB-OPEN, N-OBJ-OPEN, N-DB-MOD, N-OBJ-MOD) |
| `SET-OUTCOME-COMMIT` | SetupAction | Immutable ProbeOutcomeMode=COMMIT fixed before the trigger: after TriggerActionId returns the driver ends T-PRIMARY with SA-TX-END; non-eOk is UNKNOWN. No abort outcome can PASS. (produces: N-TR-ABOUT-END, N-TR-OUTERMOST-END-CALLED, N-TR-ENDED, N-DB-MOD, N-OBJ-MOD, N-OBJ-CLOSED; usesSupport: SA-TX-END) |
| `SET-RETAIN-CALLBACK-DATA` | SetupAction | Before the trigger allocate one retained record {ProbeId, MutationActionId, target ObjectIds, token slots} passed to the scheduler as callback data (NS-SEND: the command string is exactly `I52CTDA_QUEUED ` for CMD-QUEUED, activate=false, wrapUpInactiveDoc=false, echo=false). Released only by the row's cleanup fence. |
| `SET-RETAIN-TWO-STAGE-DATA` | SetupAction | As SET-RETAIN-CALLBACK-DATA with two stage slots: stage 1 APPCTX delivery (no mutation) calls NS-BEGIN-CMDCTX with stage 2; stage 2 is CMDCTX-EXEC-01. |
| `SET-PAYLOAD-PATH` | SetupAction | Resolve NL-ARX-PATH of R-PAYLOAD-ARX, require the file to exist and its SHA-256 to equal the build-tuple payload hash before the trigger; mismatch is UNKNOWN. |
| `SET-LOAD-PAYLOAD` | SetupAction | C15 only: acedArxLoad(NL-ARX-PATH) during setup; require RTNORM, loaded state, N-RX-LOADED for the exact module and one MARK-REGISTRATION record before the trigger; otherwise UNKNOWN and no trigger. Then export the C15 arm record. (produces: N-RX-WILL-LOAD, N-RX-LOADED) |
| `SET-ARM-VETO` | SetupAction | RR-DOC documentLockModeWillChange for the exact scratch document and the exact TRG-LOCK-VETO request calls SA-VETO once (RG-ONESHOT). (usesSupport: SA-VETO) |

## 7. Trigger actions

| Id | Kind | Definition |
|---|---|---|
| `TRG-MODIFY-TRIGGER-MOD` | TriggerAction | acdbOpenObject(F-TRIGGER-MOD, kForWrite); setPosition((1000,1,0)); close(). Open/close, never through T-PRIMARY. (target: F-TRIGGER-MOD; produces: N-DB-OPEN, N-DB-MOD; driver: DRIVER-CMD-01) |
| `TRG-ASSERT-WRITE-TRIGGER-XR` | TriggerAction | acdbOpenObject(F-TRIGGER-XR, kForWrite); assertWriteEnabled(); write bytes HFV35:TRIGGER-XR:1; close(). (target: F-TRIGGER-XR; produces: N-OBJ-OPEN, N-DB-OPEN, N-OBJ-MOD, N-DB-MOD, N-OBJ-CLOSED; driver: DRIVER-CMD-01) |
| `TRG-ERASE-TRIGGER-DB` | TriggerAction | acdbOpenObject(F-TRIGGER-ERASE-DB, kForWrite); erase(true); close(). (target: F-TRIGGER-ERASE-DB; produces: N-DB-ERASE; driver: DRIVER-CMD-01) |
| `TRG-ERASE-TRIGGER-OBJ` | TriggerAction | acdbOpenObject(F-TRIGGER-ERASE-OBJ, kForWrite); erase(true); close(). (target: F-TRIGGER-ERASE-OBJ; produces: N-OBJ-ERASE, N-DB-ERASE; driver: DRIVER-CMD-01) |
| `TRG-ERASE-REF-B` | TriggerAction | 02NO-SM only: acdbOpenObject(F-REF-B, kForWrite); erase(true); close(). F-REF-B is never reopened afterwards (V34 binding §164). (target: F-REF-B; produces: N-OBJ-ERASE, N-DB-ERASE, N-OBJ-CLOSED; driver: DRIVER-CMD-01) |
| `TRG-APPEND-TRIGGER` | TriggerAction | Create F-TRIGGER-APPEND in memory, arm RG-DB-APPEND with its instance pointer, open Model Space kForWrite with acdbOpenObject, appendAcDbEntity, close the point and Model Space. (target: F-TRIGGER-APPEND; produces: N-DB-APPEND, N-DB-MOD; driver: DRIVER-CMD-01) |
| `TRG-OPEN-FOR-MODIFY-XR` | TriggerAction | acdbOpenObject(F-XR, kForWrite); assertWriteEnabled(); no byte is changed; close(). (target: F-XR; produces: N-DB-OPEN, N-OBJ-OPEN; driver: DRIVER-CMD-01) |
| `TRG-MODIFY-CLOSE-REF-A` | TriggerAction | acdbOpenObject(F-REF-A, kForWrite); assertWriteEnabled() with its default arguments (the graphics-modified flag is left as it sets it); no property value changes; close(). (target: F-REF-A; produces: N-OBJ-OPEN, N-DB-OPEN, N-OBJ-MOD, N-ENT-GFX, N-OBJ-CLOSED, N-DB-MOD; driver: DRIVER-CMD-01) |
| `TRG-TRANSFORM-CLOSE-REF-A` | TriggerAction | acdbOpenObject(F-REF-A, kForWrite); transformBy(AcGeMatrix3d::kIdentity); close(). (target: F-REF-A; produces: N-OBJ-OPEN, N-DB-OPEN, N-ENT-GFX, N-OBJ-MOD, N-DB-MOD, N-OBJ-CLOSED; driver: DRIVER-CMD-01) |
| `TRG-CANCEL-OPEN-XR` | TriggerAction | acdbOpenObject(F-XR, kForWrite); assertWriteEnabled(); no byte is changed; cancel(). (target: F-XR; produces: N-OBJ-CANCEL, N-OBJ-UNDO, N-OBJ-CLOSED; driver: DRIVER-CMD-01) |
| `TRG-START-OUTER-T` | TriggerAction | Require active count 0 (otherwise UNKNOWN); SA-TX-START T-CONTROLLED as outermost; after it returns SA-TX-END T-CONTROLLED. (produces: N-TR-ABOUT-START, N-TR-STARTED, N-TR-ABOUT-END, N-TR-OUTERMOST-END-CALLED, N-TR-ENDED; driver: DRIVER-CMD-01; usesSupport: SA-TX-START, SA-TX-END) |
| `TRG-ABORT-PRIMARY-T` | TriggerAction | SA-TX-ABORT on driver-owned T-PRIMARY. (produces: N-TR-ABOUT-ABORT, N-TR-ABORTED, N-OBJ-CANCEL, N-OBJ-UNDO, N-OBJ-CLOSED; driver: DRIVER-CMD-01; usesSupport: SA-TX-ABORT) |
| `TRG-END-NESTED-T` | TriggerAction | SA-TX-END on T-NESTED while T-PRIMARY is active (count 2 -> 1). T-PRIMARY is ended only by SET-OUTCOME-COMMIT. (produces: N-TR-ABOUT-END, N-TR-ENDED; driver: DRIVER-CMD-01; usesSupport: SA-TX-END) |
| `TRG-END-PRIMARY-T` | TriggerAction | SA-TX-END on driver-owned T-PRIMARY with active count 1 (commit/end). (produces: N-TR-ABOUT-END, N-TR-OUTERMOST-END-CALLED, N-TR-ENDED; driver: DRIVER-CMD-01; usesSupport: SA-TX-END) |
| `TRG-RUN-FIXTURE-CMD` | TriggerAction | DRIVER-SCRIPT-01 issues `I52CTDA_FIXTURE` (CMD-FIXTURE) after I52CTDA_PROBE returned; the command succeeds without database writes. (produces: N-ED-WILL, N-ED-END; driver: DRIVER-CMD-01) |
| `TRG-LOCK-CYCLE` | TriggerAction | Architect B11: in DRIVER-APP-01 delivery with the document reactor already registered, SA-LOCK(D, kWrite, L"RACKCAD_CTDA_V35", L"RACKCAD_CTDA_V35", false); eOk creates one DRIVER-LOCK-01 obligation; then SA-UNLOCK(D) explicitly. (produces: N-DOC-LOCK-WILL, N-DOC-LOCK-CHANGED; driver: DRIVER-APP-01; usesSupport: SA-LOCK, SA-UNLOCK) |
| `TRG-LOCK-VETO` | TriggerAction | Architect B11: as TRG-LOCK-CYCLE with SET-ARM-VETO; the WILL callback vetoes; lockDocument is expected non-eOk and creates no obligation. An eOk result creates the obligation, is released by SA-UNLOCK in finally and is evidence for FP-VETO-BYPASS. (produces: N-DOC-LOCK-WILL, N-DOC-LOCK-VETO; driver: DRIVER-APP-01; usesSupport: SA-LOCK, SA-UNLOCK) |
| `TRG-APPCTX-SYNC` | TriggerAction | From DRIVER-CMD-01 document context call executeInApplicationContext(cb, retained data) synchronously (NX-APPCTX-SYNC); cb emits MARK-APPCTX-ENTRY, runs APPCTX-EXEC-01 and returns; the caller logs MARK-APPCTX-RETURN. (driver: DRIVER-CMD-01) |
| `TRG-APPCTX-SYNC-BEGIN-CMDCTX` | TriggerAction | From DRIVER-APP-01 delivery (no outstanding command) call executeInApplicationContext(cb) synchronously; cb emits MARK-APPCTX-ENTRY, observes isApplicationContext()==true, calls beginExecuteInCommandContext(cmdcb, retained data) capturing its status at SP-CMDCTX and returns; the caller logs MARK-APPCTX-RETURN. (driver: DRIVER-APP-01) |
| `TRG-CANCEL-CMDCTX` | TriggerAction | CANCEL-CMDCTX-01 unchanged: DRIVER-SCRIPT-01 issues HFV34_CANCEL; its body synchronously enters application context and calls beginExecuteInCommandContext(completion callback); the exact acdocman.h guarantee cancels outstanding commands before delivery; N-ED-CANCEL for HFV34_CANCEL must precede the queued delivery. SchedulerUse=TRIGGER-INFRA, no scheduler credit. (produces: N-ED-WILL, N-ED-CANCEL; driver: DRIVER-CMD-01) |
| `TRG-LOAD-PAYLOAD` | TriggerAction | acedArxLoad(NL-ARX-PATH) once; record the return value and loaded state as MARK-LOAD-RESULT. No unload. (target: R-PAYLOAD-ARX; produces: N-RX-WILL-LOAD, N-RX-LOADED; driver: DRIVER-CMD-01) |

## 8. Scheduler chains

| Id | Kind | Definition |
|---|---|---|
| `CHAIN-NONE` | SchedulerChain | No scheduler: the body runs in the callback or observation only. |
| `CHAIN-SEND` | SchedulerChain | Origin callback body (one-shot) calls NS-SEND once with the retained data, captures the returned status at SP-SEND and returns (MARK-ORIGIN-RETURN); delivery is CMD-QUEUED running SEND-EXEC-01. (produces: N-ED-WILL, N-ED-END; sequence: NS-SEND) |
| `CHAIN-APPCTX` | SchedulerChain | Origin callback body (one-shot) calls beginExecuteInApplicationContext once with the retained data, captures status at SP-APPCTX, returns (MARK-ORIGIN-RETURN); delivery runs APPCTX-EXEC-01 and emits MARK-APPCTX-DELIVERY. (sequence: NS-BEGIN-APPCTX) |
| `CHAIN-APPCTX-CMDCTX` | SchedulerChain | Origin body calls NS-BEGIN-APPCTX (stage 1); the APPCTX delivery (MARK-APPCTX-DELIVERY, no mutation, no lock) calls NS-BEGIN-CMDCTX at SP-CMDCTX and returns (MARK-APPCTX-CALLBACK-RETURN); stage 2 delivery runs CMDCTX-EXEC-01. (produces: N-ED-WILL, N-ED-END; sequence: NS-BEGIN-APPCTX, NS-BEGIN-CMDCTX) |
| `CHAIN-SYNC` | SchedulerChain | Synchronous executeInApplicationContext inside the trigger; never a scheduler. (sequence: NX-APPCTX-SYNC) |
| `CHAIN-SYNC-CMDCTX` | SchedulerChain | Synchronous application-context callback calls NS-BEGIN-CMDCTX; delivery runs CMDCTX-EXEC-01. (produces: N-ED-WILL, N-ED-END; sequence: NX-APPCTX-SYNC, NS-BEGIN-CMDCTX) |

## 9. Execution contexts

| Id | Kind | Definition |
|---|---|---|
| `EXEC-OBSERVE` | ExecutionContext | Observation only: callbacks log identity, order, count, lock, context and top T; no mutation and no transaction is started. (transaction: T-NONE) |
| `CB-PRIMARY-01` | ExecutionContext | Inside the exact body callback: observe a write-capable lock (CB-LOCK-01), require topTransaction()==T-PRIMARY, getObject each MutationActionId target kForWrite through T-PRIMARY, write, return. T-PRIMARY outcome is SET-OUTCOME-COMMIT. Failure of any step is UNKNOWN. (transaction: T-PRIMARY; locks: CB-LOCK-01) |
| `CB-EXEC-01` | ExecutionContext | Architect B08: inside the exact body callback resolve D, observe the lock mode; a write-capable mode is required (otherwise UNKNOWN, no mutation); no SA-LOCK; if the row's PrimaryTransactionId is T-ABORTING and numActiveTransactions() > 0 at this point, T_CB would nest in an aborting transaction: UNKNOWN (UNK-NESTED-IN-ABORT), no mutation; otherwise SA-TX-RESOLVE, SA-TX-START T_CB (CB-TX-01), mutation, SA-TX-END (ProbeOutcomeMode=COMMIT) before callback return; failure unwinds with SA-TX-ABORT and is UNKNOWN; topTransaction is observation only. (produces: N-TR-ABOUT-START, N-TR-STARTED, N-TR-ABOUT-END, N-TR-OUTERMOST-END-CALLED, N-TR-ENDED; transaction: T_CB; locks: CB-LOCK-01; usesSupport: SA-TX-RESOLVE, SA-TX-START, SA-TX-END, SA-TX-ABORT) |
| `APPCTX-EXEC-01` | ExecutionContext | V34 §8: APPCTX entry -> D -> APPCTX-LOCK-01 -> APPCTX-TX-01 -> mutation -> end/abort -> APPCTX-UNLOCK-01 -> completion token. The APPCTX entry is the asynchronous NS-BEGIN-APPCTX delivery or, for 13A-SM only, the synchronous NX-APPCTX-SYNC callback of TRG-APPCTX-SYNC (NPM-V34 already binds APPCTX-EXEC-01 there). (produces: N-TR-ABOUT-START, N-TR-STARTED, N-TR-ABOUT-END, N-TR-OUTERMOST-END-CALLED, N-TR-ENDED; transaction: T_APP; locks: APPCTX-LOCK-01; usesSupport: SA-LOCK, SA-TX-RESOLVE, SA-TX-START, SA-TX-END, SA-TX-ABORT, SA-UNLOCK) |
| `CMDCTX-EXEC-01` | ExecutionContext | V34 §8 unchanged: CMDCTX delivery -> D -> CMDCTX-LOCK-01 -> CMDCTX-TX-01 -> mutation -> end/abort -> completion token. (produces: N-TR-ABOUT-START, N-TR-STARTED, N-TR-ABOUT-END, N-TR-OUTERMOST-END-CALLED, N-TR-ENDED; transaction: T_CMD; locks: CMDCTX-LOCK-01; usesSupport: SA-TX-RESOLVE, SA-TX-START, SA-TX-END, SA-TX-ABORT) |
| `EDC-EXEC-01` | ExecutionContext | V34 §8 unchanged: in N-ED-CANCEL resolve exact scratch D, require observed write-capable command lock, own fresh T_CANCEL, mutate and end/abort before return; absent authority is UNKNOWN. (produces: N-TR-ABOUT-START, N-TR-STARTED, N-TR-ABOUT-END, N-TR-OUTERMOST-END-CALLED, N-TR-ENDED; transaction: T_CANCEL; locks: EDC-LOCK-01; usesSupport: SA-TX-RESOLVE, SA-TX-START, SA-TX-END, SA-TX-ABORT) |
| `SEND-EXEC-01` | ExecutionContext | NSC NS-SEND contract made explicit: CMD-QUEUED delivery (EP-COMMAND) -> MDI active D must be the scratch D -> SEND-LOCK-01 -> SA-TX-RESOLVE -> SA-TX-START T_SEND -> mutation -> SA-TX-END (abort on failure, UNKNOWN) -> completion token. (produces: N-TR-ABOUT-START, N-TR-STARTED, N-TR-ABOUT-END, N-TR-OUTERMOST-END-CALLED, N-TR-ENDED; transaction: T_SEND; locks: SEND-LOCK-01; usesSupport: SA-TX-RESOLVE, SA-TX-START, SA-TX-END, SA-TX-ABORT) |

## 10. Transactions

| Id | Kind | Definition |
|---|---|---|
| `T-PRIMARY` | TransactionId | driver-owned outermost transaction opened by SET-OPEN-PRIMARY-T |
| `T-NESTED` | TransactionId | driver-owned nested transaction opened by SET-OPEN-NESTED-T |
| `T-ABORTING` | TransactionId | T-PRIMARY while TRG-ABORT-PRIMARY-T aborts it; callback active count logged |
| `T-ENDED` | TransactionId | T-PRIMARY after TRG-END-PRIMARY-T; absent from active count |
| `T-CONTROLLED` | TransactionId | outermost transaction started and ended by TRG-START-OUTER-T |
| `NONE-OPENED` | TransactionId | the driver opens no transaction before the trigger; any host transaction is observation only |
| `T_CB` | TransactionId | callback-owned transaction of CB-EXEC-01 (replaces V34 T-FRESH) |
| `T_APP` | TransactionId | APPCTX-TX-01 transaction |
| `T_CMD` | TransactionId | CMDCTX-TX-01 transaction |
| `T_CANCEL` | TransactionId | EDC-EXEC-01 transaction |
| `T_SEND` | TransactionId | SEND-EXEC-01 transaction |
| `T-NONE` | TransactionId | no execution transaction (observation-only row) |

## 11. Top-transaction authorities

| Id | Kind | Definition |
|---|---|---|
| `TOP-OBS` | TopTransactionAuthority | record topTransaction() id at the body/primary callback. Top authorities are records only and never classify PASS/FAIL/UNKNOWN by themselves. |
| `TOP-OBS-ORIGIN` | TopTransactionAuthority | record topTransaction() id at the origin callback. Top authorities are records only and never classify PASS/FAIL/UNKNOWN by themselves. |
| `TOP-REL-OUTER` | TopTransactionAuthority | record the top id and its relation to T-PRIMARY (outer) at the callback. Top authorities are records only and never classify PASS/FAIL/UNKNOWN by themselves. |
| `TOP-REL-PRIMARY` | TopTransactionAuthority | record the top id and its relation to T-PRIMARY at the callback. Top authorities are records only and never classify PASS/FAIL/UNKNOWN by themselves. |
| `TOP-REL-OWNED` | TopTransactionAuthority | record the top id after the owned execution transaction starts. Top authorities are records only and never classify PASS/FAIL/UNKNOWN by themselves. |
| `TOP-OBS-AFTER-END` | TopTransactionAuthority | record the top id after the trigger transaction ended. Top authorities are records only and never classify PASS/FAIL/UNKNOWN by themselves. |
| `TOP-NONE-EXPECTED` | TopTransactionAuthority | record the top id; the fixture opens no transaction on this path. Top authorities are records only and never classify PASS/FAIL/UNKNOWN by themselves. |

## 12. Document-lock authorities

| Id | Kind | Definition |
|---|---|---|
| `LOCK-OBS` | DocumentLockAuthority | V34 unchanged: actual lock mode logged; no permission inferred |
| `CB-LOCK-01` | DocumentLockAuthority | Architect B08: the body callback observes the scratch document lock mode; a write-capable mode (kWrite, kAutoWrite or kProtectedAutoWrite) is required before any mutation, otherwise UNKNOWN; no SA-LOCK |
| `APPCTX-LOCK-01` | DocumentLockAuthority | V34 §8 unchanged (with APPCTX-UNLOCK-01 obligation) |
| `CMDCTX-LOCK-01` | DocumentLockAuthority | V34 §8 unchanged |
| `EDC-LOCK-01` | DocumentLockAuthority | EDC-EXEC-01 lock clause: observed write-capable command lock inside N-ED-CANCEL |
| `SEND-LOCK-01` | DocumentLockAuthority | at CMD-QUEUED delivery observe a write-capable host command lock on the scratch D; absence is UNKNOWN; no ownership transfer |
| `DRIVER-LOCK-01` | DocumentLockAuthority | Architect B11: SA-LOCK kWrite by DRIVER-APP-01 on the scratch DWG; eOk creates exactly one driver-owned obligation released by explicit SA-UNLOCK; an outstanding obligation at cleanup is UNKNOWN |

## 13. Mutation actions

| Id | Kind | Definition |
|---|---|---|
| `MUT-S` | MutationAction | NPM-V34 MUT-S with the §164 write set ['F-XR'] (produces: N-DB-OPEN, N-DB-MOD, N-OBJ-OPEN, N-OBJ-MOD, N-OBJ-CLOSED; writeSet: F-XR) |
| `MUT-M` | MutationAction | NPM-V34 MUT-M with the §164 write set ['F-REF-B'] (produces: N-DB-OPEN, N-DB-MOD, N-OBJ-OPEN, N-OBJ-MOD, N-ENT-GFX, N-OBJ-CLOSED; writeSet: F-REF-B) |
| `MUT-SM` | MutationAction | NPM-V34 MUT-SM with the §164 write set ['R-SM-LINK', 'F-REF-C'] (produces: N-DB-OPEN, N-DB-MOD, N-DB-APPEND; writeSet: R-SM-LINK, F-REF-C) |
| `MUT-ALL` | MutationAction | NPM-V34 MUT-ALL with the §164 write set ['F-XR', 'F-REF-B', 'R-SM-LINK', 'F-REF-C'] (produces: N-DB-OPEN, N-DB-MOD, N-DB-APPEND, N-OBJ-OPEN, N-OBJ-MOD, N-ENT-GFX, N-OBJ-CLOSED; writeSet: F-XR, F-REF-B, R-SM-LINK, F-REF-C) |
| `MUT-NONE` | MutationAction | no mutation |

## 14. Markers

| Id | Kind | Definition |
|---|---|---|
| `MARK-APPCTX-CALLBACK-RETURN` | Marker | Architect B09: return of the asynchronous beginExecuteInApplicationContext delivery callback (stage 1 of CHAIN-APPCTX-CMDCTX), logged as its last instruction. |

## 15. Observation predicates

| Id | Kind | Definition |
|---|---|---|
| `OBS-PRIMARY-CALLBACK` | ObservationPredicate | exactly one body execution (or, for observation rows, at least one observation) of the PrimaryAuthority callback for the exact identity after arming |
| `OBS-ORIGIN-CALLBACK` | ObservationPredicate | exactly one body execution of the ScheduleOrigin callback for the exact identity after arming |
| `OBS-ENQUEUE-OK` | ObservationPredicate | every scheduler call of the SchedulerChain returned success at its SchedulePoint |
| `OBS-DELIVERY` | ObservationPredicate | every chain stage delivery observed at its ExecutionPoint strictly after the scheduling caller returned (post-return delivery) |
| `OBS-CAUSAL-ORDER` | ObservationPredicate | total order trigger < origin entry < enqueue < origin return < delivery entry < mutation < completion token, for every stage |
| `OBS-ORDER-RULES` | ObservationPredicate | every present pair among required events satisfies the header will/about -> did order: TR ABOUT-START<STARTED, ABOUT-ABORT<ABORTED, ABOUT-END<ENDED, DOC WILL<CHANGED, WILL<VETO, ED WILL<END, WILL<CANCEL, RX WILL-LOAD<LOADED; no other relative order is required |
| `OBS-ORDER-COMPLETE` | ObservationPredicate | V34 §7 OBSERVED-ORDER-COMPLETE |
| `OBS-ORDER-INVARIANT-09N-O` | ObservationPredicate | V34 §7 EXPECTED-ORDER-INVARIANT-09N-O |
| `OBS-T-AFFILIATION` | ObservationPredicate | the mutation is logged inside the row's execution transaction and that transaction's end/abort result is recorded before the fresh verifier |
| `OBS-COUNT` | ObservationPredicate | active transaction count at the primary callback recorded and equal to the value implied by ExpectedAfter (NESTED-PREEND >1, OUTERMOST-PREEND ==1, OUTERMOST-END-CALLED ==1, ENDED absent) |
| `OBS-VISIBILITY` | ObservationPredicate | at the primary callback a read through the transaction being ended returns the ExpectedBefore semantic bytes (STATE-S-0) and, for STATE-T-NESTED-PREEND, the nested transaction is present in the active set while the outer transaction is active |
| `OBS-REREAD` | ObservationPredicate | a fresh legal database read after the controlled end is recorded |
| `OBS-GUARD-RESET` | ObservationPredicate | every armed guard is disarmed in finally and every suppressed re-entry is logged |
| `OBS-LOCK-MODES` | ObservationPredicate | current/new lock modes of every document-lock callback are recorded |
| `OBS-VETO-OK` | ObservationPredicate | SA-VETO returned eOk, N-DOC-LOCK-VETO observed for the exact request and lockDocument returned non-eOk |
| `OBS-ERASE-FLAG` | ObservationPredicate | erased(pErasing=true) for F-REF-B and N-DB-ERASE for the same ObjectId are recorded |
| `OBS-CONTEXT` | ObservationPredicate | every ThreadContext element is observed in order (isApplicationContext() for CTX-APPLICATION, command context for CTX-COMMAND) |
| `OBS-SYNC-ENTRY-RETURN` | ObservationPredicate | MARK-APPCTX-ENTRY occurs before the synchronous caller returns and MARK-APPCTX-RETURN after the callback completes |
| `OBS-CANCELLATION` | ObservationPredicate | commandCancelled(L"HFV34_CANCEL") observed before the queued CMDCTX delivery |
| `OBS-LOAD` | ObservationPredicate | MARK-LOAD-RESULT success (RTNORM and loaded state) and N-RX-LOADED for the exact module |
| `OBS-REGISTRATION` | ObservationPredicate | exactly one MARK-REGISTRATION record (event, object, database, reactor instance) from R-PAYLOAD-ARX, and the origin body executed in that same reactor instance |
| `OBS-MARKERS` | ObservationPredicate | every + marker observed at least once and every - marker never observed within the run window, under the marker binding rules |
| `OBS-OUTCOME-COMMIT` | ObservationPredicate | SET-OUTCOME-COMMIT SA-TX-END returned eOk |
| `OBS-CANDIDATE-BOUNDARY` | ObservationPredicate | Architect B12: classification ACCEPTED iff (i) the exact commandEnded callback entry precedes MARK-LOCK-RELEASE, (ii) CB-LOCK-01 observed a write-capable mode, (iii) T_CB started non-null and ended eOk before callback return, and (iv) the fresh verifier equals ExpectedAfter; (i), (ii) or (iii) false -> UNAVAILABLE (UNKNOWN); (i)-(iii) true and (iv) false -> contradiction (FP-BOUNDARY) |
| `OBS-NOTIFIER-WRITE` | ObservationPredicate | after the run F-TRIGGER-XR holds only the trigger bytes HFV35:TRIGGER-XR:1 and the body wrote no object other than the MutationActionId write set |
| `OBS-EWASNOTIFYING` | ObservationPredicate | inside the body callback, before the mutation, acdbOpenObject(F-REF-A, kForWrite) is attempted once and its ErrorStatus recorded (closed at once without change if eOk); the value never classifies |
| `OBS-STAGED` | ObservationPredicate | SET-STAGE-S-IN-PRIMARY read back HFV30:S:1 through T-PRIMARY before the trigger |
| `OBS-EXEC-COMPLETE` | ObservationPredicate | every step of ExecutionContextId succeeded and every completion token of the row is present |

## 16. UNKNOWN predicates

| Id | Kind | Definition |
|---|---|---|
| `UNK-REJECTION` | UnknownPredicate | a scheduler or support call returned a non-success status -> UNKNOWN. |
| `UNK-NONDELIVERY` | UnknownPredicate | a required delivery or completion token is absent at I52CTDA_FINISH -> UNKNOWN. |
| `UNK-ILLEGAL-CONTEXT` | UnknownPredicate | the execution context's lock/T prerequisites could not be established -> UNKNOWN. |
| `UNK-ILLEGAL-DB-CONTEXT` | UnknownPredicate | the exact scratch document or database could not be resolved at delivery -> UNKNOWN. |
| `UNK-ILLEGAL-ENQUEUE` | UnknownPredicate | a scheduler was called outside its legal caller context -> UNKNOWN. |
| `UNK-UNDRAINED` | UnknownPredicate | queued work remains or drain cannot be proven -> UNKNOWN. |
| `UNK-LOCK-DOC-T-LEAK` | UnknownPredicate | lock, document, T, end or unlock rejection, or an outstanding lock/T obligation -> UNKNOWN. |
| `UNK-ILLEGAL-MUTATION` | UnknownPredicate | the mutation could not be performed legally or was not observed -> UNKNOWN. |
| `UNK-RECURSION` | UnknownPredicate | recursion into a body not contained by its guard -> UNKNOWN. |
| `UNK-MISSING-CALLBACK` | UnknownPredicate | a required callback was not observed -> UNKNOWN. |
| `UNK-ORDER-INCOMPLETE` | UnknownPredicate | order capture incomplete -> UNKNOWN. |
| `UNK-T-START` | UnknownPredicate | a required transaction start returned null or was not observed -> UNKNOWN. |
| `UNK-T-END` | UnknownPredicate | a required transaction end/commit was unavailable or not observed -> UNKNOWN. |
| `UNK-LOAD` | UnknownPredicate | payload load not proven -> UNKNOWN. |
| `UNK-REGISTRATION` | UnknownPredicate | payload registration not proven -> UNKNOWN. |
| `UNK-CLEANUP-PROOF` | UnknownPredicate | cleanup or process-exit proof missing -> UNKNOWN. |
| `UNK-NO-WRITE-LOCK` | UnknownPredicate | no write-capable lock observed where required -> UNKNOWN. |
| `UNK-T-CMD-FAILURE` | UnknownPredicate | T_CMD start/end failure -> UNKNOWN. |
| `UNK-T-CANCEL-FAILURE` | UnknownPredicate | T_CANCEL start/end failure -> UNKNOWN. |
| `UNK-INVALID-APP-CONTEXT` | UnknownPredicate | isApplicationContext() not observed true where required -> UNKNOWN. |
| `UNK-CANCELLATION-MISSING` | UnknownPredicate | exact N-ED-CANCEL not observed before delivery -> UNKNOWN. |
| `UNK-NESTED-IN-ABORT` | UnknownPredicate | the execution transaction would nest inside a transaction that is being aborted -> UNKNOWN. |
| `UNK-STAGING` | UnknownPredicate | SET-STAGE-S-IN-PRIMARY could not open, write or read back the staged bytes -> UNKNOWN. |

## 17. FAIL predicates

| Id | Kind | Definition |
|---|---|---|
| `FP-STATE` | FailPredicate | complete evidence and the fresh verifier contradicts ExpectedAfter (for SM, SmRules FAIL-SM classification). |
| `FP-ORDER` | FailPredicate | complete order capture contradicts an OBS-ORDER-RULES pair or a required callback identity. |
| `FP-AFFILIATION` | FailPredicate | the mutation is logged inside the row's execution transaction, that transaction's SA-TX-END (or SET-OUTCOME-COMMIT for T-PRIMARY) returned eOk, and the fresh verifier contradicts the row's fixed ExpectedAfter. |
| `FP-COUNT` | FailPredicate | complete evidence and the recorded active count contradicts OBS-COUNT. |
| `FP-VISIBILITY` | FailPredicate | complete evidence contradicts OBS-VISIBILITY. |
| `FP-INVARIANT-09N-O` | FailPredicate | complete evidence contradicts EXPECTED-ORDER-INVARIANT-09N-O; an unspecified B/O/C order is never FAIL. |
| `FP-BOUNDARY` | FailPredicate | OBS-CANDIDATE-BOUNDARY contradiction branch. |
| `FP-CAUSAL` | FailPredicate | a delivery entry precedes its scheduling caller's return, or the deferred mutation is visible before delivery. |
| `FP-CONTEXT` | FailPredicate | a delivered callback whose exact header declares its context runs, with complete evidence, in a different context. |
| `FP-SYNC` | FailPredicate | complete evidence shows the synchronous callback did not run before executeInApplicationContext returned. |
| `FP-NOTIFIER-WRITE` | FailPredicate | OBS-NOTIFIER-WRITE contradicted with complete evidence. |
| `FP-VETO-BYPASS` | FailPredicate | SA-VETO returned eOk and lockDocument still returned eOk, or state changed under vetoed authority. |
| `FP-CLEANUP-SAFETY` | FailPredicate | cleanup establishes a safety contradiction (retained reactor fired on a destroyed notifier, module residency outlived the process fence). |

## 18. Result classes and rule

| Id | Kind | Definition |
|---|---|---|
| `PASS-T` | ResultClass | V35 result class for S/M transaction-observation rows: all required observation predicates hold, the fresh VER-T+VER-S equals ExpectedAfter and cleanup completes. |
| `FAIL-T` | ResultClass | V35 result class for S/M transaction-observation rows: complete evidence and a listed FailPredicate holds. |
| `RESULT-RULE-V35` | ResultRule | UNKNOWN iff UNKNOWN-COMMON, any UnknownPredicateId, or any required ObservationPredicateId unavailable. Otherwise FAIL iff evidence is complete and any FailPredicateId holds. Otherwise PASS iff PassClass holds, every ObservationPredicateId holds, the verifier equals ExpectedAfter and the CleanupActionId plus CompletionFence complete. Otherwise UNKNOWN (an available but false observation predicate without a matching FailPredicate establishes no contrary host property). For ExpectedAfter STATE-T-NESTED-PREEND, STATE-T-OUTERMOST-PREEND and STATE-T-OUTERMOST-END-CALLED the comparison uses the callback-time VER-T record (OBS-COUNT, OBS-VISIBILITY) and the FINISH verifier additionally requires STATE-S-0 unchanged. Precedence UNKNOWN > FAIL > PASS; the three are disjoint and exhaustive. |

## 19. Cleanup steps

| Id | Kind | Definition |
|---|---|---|
| `CLN-OBJ-REMOVE` | CleanupStep | Architect B04: for each retained object/entity reactor of the row, open its notifier while live (kForRead; the disposable erased F-TRIGGER-ERASE-OBJ with openErased=true) and removeReactor(exact instance); non-eOk is UNKNOWN; never depends on N-OBJ-GOODBYE; the 26 callbacks are unchanged |
| `CLN-OBJ-RETAIN-FENCED` | CleanupStep | 02NO-SM only: F-REF-B is erased by the trigger and never reopened (§164); OR-REF-B is not removed, its instance is retained unreleased until process exit and nothing accesses the notifier; FENCE-PROCESS-EXIT is mandatory |
| `CLN-DOC-REMOVE` | CleanupStep | remove the retained RR-DOC from the document manager and verify removal |
| `CLN-DEFER-DRAIN` | CleanupStep | after the delivery token, unregister the origin reactor unless an earlier step of the same CleanupAction already removed it, and verify no queued command/callback remains; inability to prove drain is UNKNOWN |
| `CLN-BASE` | CleanupStep | single terminal step: abort any owned open T; verify no outstanding driver or APPCTX lock obligation; unregister each remaining registered observer except those retained by CLN-OBJ-RETAIN-FENCED (an object/entity reactor still registered is removed as CLN-OBJ-REMOVE prescribes, openErased=true only for a disposable erase trigger); erase each live fixture resource incl. F-TRIGGER-APPEND, never reopening an object the trigger erased (F-REF-B in 02NO-SM, the disposable erase triggers); remove R-SM-LINK (LINK-CARRIER-REMOVED); disarm any guard still armed; close the scratch DWG without save; flush the log; verify each step |
| `CLN-PROCESS-EXIT` | CleanupStep | request clean process exit, externally verify the exact PID exited (so no retained callback data, reactor instance or module residency remains) and delete/quarantine scratch artifacts |

## 20. Cleanup actions

| Id | Kind | Definition |
|---|---|---|
| `CLEAN-BASE` | CleanupAction | Ordered steps CLN-BASE; fence FENCE-CLEANUP-TOKEN. V35 CLEAN-BASE. (steps: CLN-BASE; fence: FENCE-CLEANUP-TOKEN) |
| `CLEAN-OBJ` | CleanupAction | Ordered steps CLN-OBJ-REMOVE -> CLN-BASE; fence FENCE-CLEANUP-TOKEN. B04: no goodbye dependency. (steps: CLN-OBJ-REMOVE, CLN-BASE; fence: FENCE-CLEANUP-TOKEN) |
| `CLEAN-OBJ-FENCED` | CleanupAction | Ordered steps CLN-OBJ-RETAIN-FENCED -> CLN-BASE -> CLN-PROCESS-EXIT; fence FENCE-PROCESS-EXIT. new for 02NO-SM (Architect confirmation requested). (steps: CLN-OBJ-RETAIN-FENCED, CLN-BASE, CLN-PROCESS-EXIT; fence: FENCE-PROCESS-EXIT) |
| `CLEAN-DEFER` | CleanupAction | Ordered steps CLN-DEFER-DRAIN -> CLN-BASE; fence FENCE-CLEANUP-TOKEN. V34 unchanged. (steps: CLN-DEFER-DRAIN, CLN-BASE; fence: FENCE-CLEANUP-TOKEN) |
| `CLEAN-DOC` | CleanupAction | Ordered steps CLN-DOC-REMOVE -> CLN-BASE; fence FENCE-CLEANUP-TOKEN. V34 unchanged. (steps: CLN-DOC-REMOVE, CLN-BASE; fence: FENCE-CLEANUP-TOKEN) |
| `CLEAN-OBJ-DEFER` | CleanupAction | Ordered steps CLN-OBJ-REMOVE -> CLN-DEFER-DRAIN -> CLN-BASE; fence FENCE-CLEANUP-TOKEN. C02: one terminal CLN-BASE. (steps: CLN-OBJ-REMOVE, CLN-DEFER-DRAIN, CLN-BASE; fence: FENCE-CLEANUP-TOKEN) |
| `CLEAN-DOC-DEFER` | CleanupAction | Ordered steps CLN-DOC-REMOVE -> CLN-DEFER-DRAIN -> CLN-BASE; fence FENCE-CLEANUP-TOKEN. C02: one terminal CLN-BASE. (steps: CLN-DOC-REMOVE, CLN-DEFER-DRAIN, CLN-BASE; fence: FENCE-CLEANUP-TOKEN) |
| `CLEAN-APPCTX-PROCESS` | CleanupAction | Ordered steps CLN-DEFER-DRAIN -> CLN-BASE -> CLN-PROCESS-EXIT; fence FENCE-PROCESS-EXIT. V34 semantics; T_APP unwound and APPCTX-UNLOCK-01 satisfied before CLN-BASE. (steps: CLN-DEFER-DRAIN, CLN-BASE, CLN-PROCESS-EXIT; fence: FENCE-PROCESS-EXIT) |
| `CLEAN-APPCTX-LOCK-PROCESS` | CleanupAction | Ordered steps CLN-DEFER-DRAIN -> CLN-BASE -> CLN-PROCESS-EXIT; fence FENCE-PROCESS-EXIT. CLEAN-APPCTX-PROCESS plus explicit active-T count zero and no owned lock obligation (V34 §8). (steps: CLN-DEFER-DRAIN, CLN-BASE, CLN-PROCESS-EXIT; fence: FENCE-PROCESS-EXIT) |
| `CLEAN-CMDCTX-PROCESS` | CleanupAction | Ordered steps CLN-DEFER-DRAIN -> CLN-BASE -> CLN-PROCESS-EXIT; fence FENCE-PROCESS-EXIT. V34 semantics. (steps: CLN-DEFER-DRAIN, CLN-BASE, CLN-PROCESS-EXIT; fence: FENCE-PROCESS-EXIT) |
| `CLEAN-CHAIN-PROCESS` | CleanupAction | Ordered steps CLN-DEFER-DRAIN -> CLN-BASE -> CLN-PROCESS-EXIT; fence FENCE-PROCESS-EXIT. V34 semantics; all stage tokens; retained object/document reactors removed inside CLN-BASE. (steps: CLN-DEFER-DRAIN, CLN-BASE, CLN-PROCESS-EXIT; fence: FENCE-PROCESS-EXIT) |
| `CLEAN-C15-PROCESS` | CleanupAction | Ordered steps CLN-BASE -> CLN-PROCESS-EXIT; fence FENCE-PROCESS-EXIT. V34 semantics: retained-reactor removal attempted while notifier live and recorded, non-success forbids PASS; no in-process unload. (steps: CLN-BASE, CLN-PROCESS-EXIT; fence: FENCE-PROCESS-EXIT) |
| `CLEAN-CANCEL-PROCESS` | CleanupAction | Ordered steps CLN-DEFER-DRAIN -> CLN-BASE -> CLN-PROCESS-EXIT; fence FENCE-PROCESS-EXIT. V34 semantics. (steps: CLN-DEFER-DRAIN, CLN-BASE, CLN-PROCESS-EXIT; fence: FENCE-PROCESS-EXIT) |

## 21. Completion fences

| Id | Kind | Definition |
|---|---|---|
| `FENCE-CLEANUP-TOKEN` | CompletionFence | In-process cleanup token after the terminal CLN-BASE; DRIVER-SCRIPT-01 then exits the process. |
| `FENCE-PROCESS-EXIT` | CompletionFence | PASS additionally requires the externally verified exit of the exact scratch PID. |

## 22. Surfaces

| Id | Kind | Definition |
|---|---|---|
| `S` | Surface | NPM-V34 §1 surface label: semantic surface. |
| `M` | Surface | NPM-V34 §1 surface label: physical surface. |
| `SM` | Surface | NPM-V34 §1 surface label: mixed sibling/linkage surface. |
| `S/M` | Surface | NPM-V34 §1 surface label: transaction observation over S and M (no mutation). |
| `S+M+SM` | Surface | NPM-V34 §1 surface label: atomic ALL execution over three distinct protected targets. |

## 23. Invariants and marker binding

| Id | Definition |
|---|---|
| `INV-TRIGGER-DISJOINT` | For rows whose body executes inside the trigger's callback (CB-PRIMARY-01, CB-EXEC-01, EDC-EXEC-01), the trigger target identity is not in the MutationActionId write set. |
| `INV-ERASE-TRIGGER-DISJOINT` | For every row, the target of an erase trigger (TRG-ERASE-TRIGGER-DB, TRG-ERASE-TRIGGER-OBJ, TRG-ERASE-REF-B) is not in the MutationActionId write set. |
| `INV-SINGLE-TERMINAL-BASE` | Every CleanupAction contains exactly one CLN-BASE and nothing but CLN-PROCESS-EXIT after it (R3-C02). |
| `INV-MARKER-OBSERVER` | Every marker has an observer registration of its family in the row. |
| `INV-BODY-OBSERVER` | A row that runs a probe body names exactly one BodyObserverId, registered by the row, whose family matches the body callback (PrimaryAuthority or ScheduleOrigin EventId), plus the family guard; every other registration of the row is observation-only. |
| `INV-MARKER-PRODUCER` | Every + marker that is an EventId is a candidate product (produces attribute, per exact header semantics) of the row's driver, setup, trigger, scheduler chain, execution context or mutation; an object/entity marker additionally requires that producer to touch an object watched by one of the row's object/entity observers (touches attribute: trigger target, staged F-XR for TRG-ABORT-PRIMARY-T, mutation write set); runtime absence remains UNKNOWN. |

| Marker family | Binding |
|---|---|
| `N-ED-WILL/N-ED-END` | bind to I52CTDA_QUEUED when ExecutionContext is SEND-EXEC-01; to I52CTDA_FIXTURE when Trigger is TRG-RUN-FIXTURE-CMD; to HFV34_CANCEL when Trigger is TRG-CANCEL-CMDCTX; to the unique commandWillStart/commandEnded pair whose interval contains the CMDCTX delivery entry (EP-CMDCTX) and its completion token, excluding I52CTDA_BOOT, I52CTDA_PROBE, I52CTDA_FIXTURE, I52CTDA_QUEUED, I52CTDA_FINISH and HFV34_CANCEL (zero or several such pairs are UNKNOWN; the name is recorded) when ExecutionContext is CMDCTX-EXEC-01; otherwise to I52CTDA_PROBE |
| `N-DOC-*` | exact scratch document |
| `N-DB-*` | scratch database |
| `N-TR-*` | scratch database transaction manager |
| `N-OBJ-*/N-ENT-GFX` | an object/entity reactor registered by the row |
| `N-RX-*` | exact NL-ARX-PATH module |
| `MARK-LOCK-RELEASE` | documentLockModeChanged of the scratch document to an unlocked mode after the row's last delivery |
| `MARK-MANAGED-CMD-END` | RR-MANAGED-CMD CommandEnded for the same command bound for N-ED-END |

## 24. Referenced ObjectARX members

Checked against the exact SDK headers by the validator when `-SdkRoot` is given.

| Member | Header |
|---|---|
| `AcDbObject::cancel` | `dbObject.h` |
| `AcDbObject::assertWriteEnabled` | `dbObject.h` |
| `AcDbObject::erase` | `dbObject.h` |
| `AcDbObject::addReactor` | `dbObject.h` |
| `AcDbObject::removeReactor` | `dbObject.h` |
| `AcDbEntity::transformBy` | `dbmain.h` |
| `AcDbEntityReactor::modifiedGraphics` | `dbmain.h` |
| `acdbOpenObject` | `dbmain.h` |
| `AcDbPoint::setPosition` | `dbents.h` |
| `AcGeMatrix3d::kIdentity` | `gemat3d.h` |
| `AcApDocManager::executeInApplicationContext` | `acdocman.h` |
| `AcApDocManager::beginExecuteInApplicationContext` | `acdocman.h` |
| `AcApDocManager::beginExecuteInCommandContext` | `acdocman.h` |
| `AcApDocManager::isApplicationContext` | `acdocman.h` |
| `AcApDocManager::sendStringToExecute` | `acdocman.h` |
| `acedArxLoad` | `acedads.h` |
| `AcDbTransactionManager::topTransaction` | `dbtrans.h` |
| `AcDbTransactionManager::numActiveTransactions` | `dbtrans.h` |
| `RTNORM` | `adscodes.h` |
