# I-52 — Execution Catalog V35

> **EXEC-CATALOG-V35-1, revision V35-A1 / DRAFT / NOT EXECUTED.** Human-readable render of
> `I-52-execution-catalog-v35.json`, which is the normative source. Every identifier used by NPM-V35 resolves to exactly one
> entry. Retained V34 ContractIds (NPM-V34 §2-§4, §7, §8), the 27 EventIds, 3 SchedulerIds and 7 support actions are imported
> as entries of the JSON catalog with their authority.

## 1. Run environment and drivers

| Id | Kind | Definition |
|---|---|---|
| `RUN-ENV-01` | RunEnvironment | Every ProbeId execution runs in its own dedicated scratch AutoCAD 2025 process (one ProbeId per PID), started by the external control plane with the dedicated scratch profile, never in the user's active session. The process opens a scratch DWG created for the run, runs DRIVER-SCRIPT-01, loads R-NATIVE-ARX (and R-MANAGED-OBSERVER when the row registers RR-MANAGED-CMD), and exits through FIN-GATE-01 and CMD-FINISH. All modules log through LOG-SEQ-01 with LOG-RECORD-01 records. The control plane records the exact PID, verifies its exit and then runs CONTROL-PLANE-RESULT-01. Process exit never substitutes for CLN-BASE verification, except for state retained by CLN-OBJ-RETAIN-FENCED. (amendedBy: RC-07) |
| `BOOT-01` | Bootstrap | Script command I52CTDA_BOOT (CMD-BOOT) materializes, in one transaction, the eight persistent fixture identities of fixture-v35.json (F-REF-C declared absent and not created) and the StateCarrier R-SM-LINK, reaching STATE-ALL-0 plus the trigger initial states. Any duplicate, missing or failed object is UNKNOWN and no trigger runs. BOOT-01 performs no observer registration. (usesSupport: SA-TX-RESOLVE, SA-TX-START, SA-TX-END, SA-TX-ABORT) |
| `DRIVER-SCRIPT-01` | DriverScript | Fixed script, the only human-free input of the run: `_.FILEDIA` `0`; `(arxload "<RunDir>/I52CtdaNative.arx")`; `_.NETLOAD` `<RunDir>\I52Ctda.ManagedObserver.dll` only when the row registers RR-MANAGED-CMD; `I52CTDA_BOOT`; `I52CTDA_PROBE <ProbeId>`; then `I52CTDA_FIXTURE` only when TriggerActionId is TRG-RUN-FIXTURE-CMD, or `HFV34_CANCEL` only when TriggerActionId is TRG-CANCEL-CMDCTX. The script contains neither I52CTDA_FINISH nor QUIT: FIN-GATE-01 issues FINISH, and CMD-FINISH issues the exit. The script is not a scheduler and grants no scheduler credit. (amendedBy: RC-01) |
| `DRIVER-CMD-01` | Driver | I52CTDA_PROBE (CMD-PROBE, modal) runs in document command context and must observe a write-capable host command lock at entry (otherwise UNKNOWN, nothing is executed). Fixed phase order: REGISTER the row's ObserverRegistrationIds in catalog order (except RR-PAYLOAD-DB, which R-PAYLOAD-ARX registers itself during SET-LOAD-PAYLOAD) and FIN-GATE-01; SETUP (SetupActionIds in order); ARM the GuardIds (RG-DB-APPEND is armed by its trigger immediately before the append); TRIGGER; CALLBACK-WINDOW (every synchronous callback of the trigger returns); POST-TRIGGER-OBLIGATIONS (SET-OUTCOME-COMMIT and any other obligation fixed by setup); OUTCOME-RECORD (the driver transaction outcome is logged); DISARM (in finally, never before the obligations complete); TOKEN (TOK-OUTCOME when applicable, then TOK-PROBE-RETURN). For the script-issued triggers TRG-RUN-FIXTURE-CMD and TRG-CANCEL-CMDCTX the guards stay armed after I52CTDA_PROBE returns, their callback identity is bound to the exact command name, and CMD-FINISH disarms them as its first action (FINISH-ENTRY). The driver exclusively owns T-PRIMARY/T-NESTED/T-CONTROLLED when setup or trigger creates them. (produces: N-ED-WILL, N-ED-END; phaseOrder: REGISTER, SETUP, ARM, TRIGGER, CALLBACK-WINDOW, POST-TRIGGER-OBLIGATIONS, OUTCOME-RECORD, DISARM, TOKEN; amendedBy: RC-12) |
| `DRIVER-APP-01` | Driver | I52CTDA_PROBE registers observers, runs setup and calls beginExecuteInApplicationContext(driverBody, data) once (driver infrastructure, SchedulerUse=DRIVER-INFRA, no scheduler credit, no NS pair; non-eOk enqueue is UNKNOWN). driverBody runs in stage STG-DRIVER-APP: it records MARK-DRIVER-APP-ENTRY, requires isApplicationContext()==true (otherwise UNKNOWN), resolves the exact scratch document D, arms guards, executes TriggerActionId, disarms guards after the trigger returned, records MARK-DRIVER-APP-RETURN and sets TOK-DRIVER-APP-DONE. It never emits a governed marker; the governed stages it hosts (STG-TRIGGER, STG-SYNC-APPCTX, origin callbacks) record their own StageId. (produces: N-ED-WILL, N-ED-END; phaseOrder: ENQUEUE, DELIVERY, ARM, TRIGGER, CALLBACK-WINDOW, POST-TRIGGER-OBLIGATIONS, OUTCOME-RECORD, DISARM, TOKEN; amendedBy: RC-03) |
| `LOG-SEQ-01` | LoggingAuthority | One process-wide log and one sequencer. R-NATIVE-ARX exports the C ABI `I52Ctda_LogAppend(const I52CtdaRecord*)`, `I52Ctda_TokenSet(const wchar_t* tokenId, uint64_t deliveryId)` and `I52Ctda_QueryC15Arm(I52CtdaC15Arm*)`. `I52Ctda_LogAppend` assigns Sequence with one InterlockedIncrement64 on a single process-global counter inside the log critical section and appends the record to the single append-only log. R-PAYLOAD-ARX binds with GetModuleHandleW(L"I52CtdaNative.arx") + GetProcAddress; R-MANAGED-OBSERVER binds with [DllImport("I52CtdaNative.arx")], which resolves the already-loaded module. No module keeps its own counter or log file. A module that cannot bind LOG-SEQ-01 records nothing and the row is UNKNOWN (UNK-LOG-BINDING). (amendedBy: RC-07, RC-08) |
| `LOG-RECORD-01` | LoggingAuthority | Every log record carries Sequence (LOG-SEQ-01), ProbeId, StageId, DeliveryId (monotonic instance id of the stage activation), DriverId or SchedulerId, ModuleId, PID, TID, DocumentId, DatabaseId, CommandIdentity, EventOrMarkerId and a typed payload. A record missing any field is malformed evidence (EVIDENCE-COMPLETE false). (amendedBy: RC-03, RC-07) |
| `FIN-GATE-01` | FinishGate | FINISH gate, driver infrastructure owned by R-NATIVE-ARX and registered by I52CTDA_PROBE before its trigger. On each idle notification it checks, in application context, (a) every CompletionTokenId of the row is set in the token table and (b) the scratch document isQuiescent() (no command, script, LISP or ARX command active). When both hold it removes the idle hook and timer and issues `I52CTDA_FINISH <ProbeId>` with sendStringToExecute(D, ..., false, false, false) in FINISH-MODE-NORMAL. If 120 s have passed since TOK-PROBE-RETURN and the document is quiescent, it issues FINISH in FINISH-MODE-DRAIN (UNK-FINISH-TIMEOUT). If the control plane sees no FINISH record 300 s after process start it terminates the PID: UNKNOWN. This use of sendStringToExecute is FINISH-INFRA: no SchedulerId, no scheduler pair, no scheduler credit; FINISH and QUIT are excluded from every marker binding. Nothing else may issue I52CTDA_FINISH; a FINISH entered with any required token missing (FINISH-EARLY) runs in FINISH-MODE-DRAIN. After the FINISH record, the control plane waits at most 120 s (postFinishDeadlineSeconds) for the exact PID to exit; otherwise it terminates the PID and the row is UNKNOWN. A command-context delivery that cancels the queued QUIT therefore ends as UNKNOWN, never as a hung process. (amendedBy: RC-01) |
| `FINISH-FENCE-01` | FinishGate | Process-wide fence flag owned by R-NATIVE-ARX (exported with LOG-SEQ-01 as I52Ctda_FinishFenceSet) and set as the first action of CMD-FINISH. Every governed delivery or body entry (origin and primary callbacks, SEND/APPCTX/CMDCTX deliveries, EDC, RR-PAYLOAD-DB, RR-MANAGED-CMD) checks it first; when it is set the entry only appends one LATE-DELIVERY record (ReactorInstance or ModuleId, NotifierAccess=NONE; no lock, no transaction, no write, no scheduler call) and returns. A LATE-DELIVERY record whose reactor instance already has a recorded eOk removal, or whose module already passed its recorded fence point, is counted by FP-CLEANUP-SAFETY (the host delivered to something the contract had removed); every other LATE-DELIVERY record makes the row UNKNOWN (UNK-LATE-DELIVERY). (amendedBy: RC-01, RC-04) |

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
| `R-PAYLOAD-ARX` | PayloadResource | Architect B03 + RC-08. Second research module I52CtdaPayload.arx, owned by eng/research/I52Ctda (separate vcxproj), research-only, never deployed. NL-ARX-PATH = absolute path <RunDir>\I52CtdaPayload.arx of the dedicated run directory, SHA-256 pinned in the build tuple. Loaded only with acedArxLoad(NL-ARX-PATH) (stage STG-PAYLOAD-LOAD). At kInitAppMsg (stage STG-PAYLOAD-INIT) it binds LOG-SEQ-01 (else it registers nothing: UNK-LOG-BINDING) and evaluates PAYLOAD-DB-BINDING; only if it holds does it call I52Ctda_QueryC15Arm; only an armed answer (C15N16N-SM) naming F-TRIGGER-MOD and the retained callback data makes it register RR-PAYLOAD-DB on that database, append one MARK-REGISTRATION record and run the C15 origin body with RG-DB-MOD semantics; unarmed rows register no reactor. The exported I52CtdaPayload_RemoveReactor removes RR-PAYLOAD-DB while the database is live and returns its status; CLN-BASE calls it and any non-eOk forbids PASS (UNKNOWN). Load success = acedArxLoad returned RTNORM, the dynamic linker reports the module loaded, and N-RX-LOADED names the exact module. No in-process unload; process exit is the residency fence. (amendedBy: RC-08) |
| `CMD-BOOT` | CommandResource | Command `I52CTDA_BOOT` registered by the research ARX at load: modal; runs BOOT-01. |
| `CMD-PROBE` | CommandResource | Command `I52CTDA_PROBE` registered by the research ARX at load: modal; runs DRIVER-CMD-01 or the enqueue half of DRIVER-APP-01. |
| `CMD-FIXTURE` | CommandResource | Command `I52CTDA_FIXTURE` registered by the research ARX at load: modal; exact controlled successful fixture command; body performs no database write and returns. |
| `CMD-CANCEL` | CommandResource | Command `HFV34_CANCEL` registered by the research ARX at load: modal; exact controlled command of CANCEL-CMDCTX-01. |
| `CMD-QUEUED` | CommandResource | Command `I52CTDA_QUEUED` registered by the research ARX at load: modal; the only NS-SEND target; body is SEND-EXEC-01 over the retained callback data. |
| `CMD-FINISH` | CommandResource | Command `I52CTDA_FINISH` registered by R-NATIVE-ARX, entered only through FIN-GATE-01. First action: set FINISH-FENCE-01, then disarm any guard still armed (script-issued triggers) and record FINISH-MODE-NORMAL or FINISH-MODE-DRAIN. NORMAL: run the row's VerifierIds, then CleanupActionId. DRAIN (timeout or FINISH-EARLY): record UNK-FINISH-TIMEOUT, run no verifier comparison, run CleanupActionId (CLN-DEFER-DRAIN records any undrained work). Flush the log. FINISH never declares PASS, FAIL or UNKNOWN; CONTROL-PLANE-RESULT-01 does, outside the process. Last action: `_.QUIT` `_Y` through FINISH-INFRA, which discards and closes the scratch document and exits AutoCAD; the scratch document is never closed from inside FINISH. (amendedBy: RC-01) |
| `R-NATIVE-ARX` | RuntimeModule | Main research ARX (existing). Owns LOG-SEQ-01, the completion-token table, FIN-GATE-01, the commands of CommandResource and every native observer except RR-PAYLOAD-DB. Research-only; never deployed with RackCad. (amendedBy: RC-07) |
| `R-MANAGED-OBSERVER` | RuntimeModule | Managed research host module of RR-MANAGED-CMD (HEC-V27-C1 row 09E, M-DOC-END). It subscribes Autodesk.AutoCAD.ApplicationServices.Document.CommandEnded for the scratch document when the driver calls its registration entry, and writes every record through LOG-SEQ-01 (P/Invoke into R-NATIVE-ARX). Research-only; not part of RackCad, not in deploy/, not referenced by src/. (amendedBy: RC-07) |

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
| `RR-MANAGED-CMD` | ObserverRegistration | Managed CommandEnded observer of HEC-V27-C1 row 09E (M-DOC-END). ObserverRegistration hosted by the RuntimeModule R-MANAGED-OBSERVER (I52Ctda.ManagedObserver.dll), which DRIVER-SCRIPT-01 loads with _.NETLOAD only when the row lists it. The driver subscribes it for the scratch document through the module's exported registration entry (failure is UNKNOWN); it records through LOG-SEQ-01 and sets TOK-MANAGED-CMD-END. It is a marker source (MARK-MANAGED-CMD-END), not a native EventId: it changes no event, callback, scheduler or support-action count. CLN-BASE unsubscribes it; the assembly stays resident until process exit and then holds no governed state. Its callbacks never run a probe body. (reactorClass: Document.CommandEnded (managed); notifier: scratch document; amendedBy: RC-07) |

## 5. Guards

| Id | Kind | Definition |
|---|---|---|
| `RG-DB-MOD` | Guard | V34 §8 unchanged: arm only for exact F-TRIGGER-MOD ObjectId; log nested notifications, suppress probe-body re-entry; other recursion UNKNOWN; disarm in finally. (family: N-DB-MOD; armTarget: F-TRIGGER-MOD; keySchema: ProbeId, GuardId, DatabaseId, EventId, CallbackMember, TargetObjectId, StageId; amendedBy: RC-11) |
| `RG-DB-ERASE` | Guard | V34 §8 unchanged: arm only for disposable F-TRIGGER-ERASE-DB; protected targets stay live; suppress body re-entry; unexpected recursion UNKNOWN. (family: N-DB-ERASE; armTarget: F-TRIGGER-ERASE-DB; keySchema: ProbeId, GuardId, DatabaseId, EventId, CallbackMember, TargetObjectId, ErasingFlag, StageId; amendedBy: RC-11) |
| `RG-OBJ-ERASE` | Guard | V34 §8 unchanged: same one-shot rule for disposable F-TRIGGER-ERASE-OBJ; no protected target is erased. (family: N-OBJ-ERASE; armTarget: F-TRIGGER-ERASE-OBJ; keySchema: ProbeId, GuardId, ReactorInstance, EventId, CallbackMember, TargetObjectId, StageId; amendedBy: RC-11) |
| `RG-DB-APPEND` | Guard | Architect B01: armed with the in-memory instance pointer of F-TRIGGER-APPEND before it is appended; the body runs once for objectAppended of that exact instance; other appends are logged only; recursion into the body is UNKNOWN; disarm in finally. (family: N-DB-APPEND; armTarget: F-TRIGGER-APPEND; keySchema: ProbeId, GuardId, DatabaseId, EventId, CallbackMember, TargetInstancePointer, StageId; amendedBy: RC-11) |
| `RG-DB-OPEN` | Guard | Architect B02: arm only for objectOpenedForModify of the exact ObjectId targeted by the row's TriggerActionId (F-TRIGGER-MOD in 02N; F-XR in CDBOPEN16SND-ALL/CDBOPEN16APP-ALL, as NPM-V34 names it); one body execution; nested notifications logged, body re-entry suppressed; other recursion UNKNOWN; disarm in finally. (family: N-DB-OPEN; armTarget: TRIGGER-TARGET; keySchema: ProbeId, GuardId, DatabaseId, EventId, CallbackMember, TargetObjectId, StageId; amendedBy: RC-11) |
| `RG-TX` | Guard | Architect B08: one-shot for the exact transaction-reactor callback identity of the row; notifications caused by the body's own actions (T_CB lifecycle, writes) are logged and never re-enter the body; any other re-entry into the body is UNKNOWN; disarm per DRIVER-CMD-01. (family: N-TR-; armTarget: CALLBACK-IDENTITY; keySchema: ProbeId, GuardId, TransactionManagerId, EventId, CallbackMember, TransactionIdentity, StageId; amendedBy: RC-11) |
| `RG-ONESHOT` | Guard | V35 generic one-shot, parameterized per family by keySchema (object/entity: reactor instance + notifier ObjectId; editor: DocumentId + exact bound CommandName; document: DocumentId + the exact TRG-LOCK-* RequestId; dynamic linker: exact ModulePath NL-ARX-PATH, never the linker singleton; payload database reactor: reactor instance + DatabaseId + F-TRIGGER-MOD; SA-VETO body: DocumentId + RequestId). One guard instance exists per full key; the SA-VETO body and an origin body in the same row are distinct instances. The first callback matching the full key runs its body once; notifications caused by the body's own actions are logged and never re-enter the body; later matching callbacks are logged only; callbacks that differ in any key component never consume the guard; any other re-entry into the body is UNKNOWN; disarm per DRIVER-CMD-01 phase order. (family: *; armTarget: CALLBACK-IDENTITY; keySchema: N-OBJ-=ProbeId/GuardId/ReactorInstance/EventId/CallbackMember/TargetObjectId/StageId; N-ENT-=ProbeId/GuardId/ReactorInstance/EventId/CallbackMember/TargetObjectId/StageId; N-ED-=ProbeId/GuardId/DocumentId/EventId/CallbackMember/CommandName/StageId; N-DOC-=ProbeId/GuardId/DocumentId/EventId/CallbackMember/RequestId/StageId; N-RX-=ProbeId/GuardId/EventId/CallbackMember/ModulePath/StageId; N-DB-=ProbeId/GuardId/ReactorInstance/DatabaseId/EventId/CallbackMember/TargetObjectId/StageId; SA-VETO=ProbeId/GuardId/DocumentId/EventId/CallbackMember/RequestId/StageId; amendedBy: RC-11) |

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
| `TRG-MODIFY-TRIGGER-MOD` | TriggerAction | acdbOpenObject(F-TRIGGER-MOD, kForWrite); setPosition((1000,1,0)); close(). Open/close, never through T-PRIMARY. (target: F-TRIGGER-MOD; targetClass: F-TRIGGER-MOD; openMode: OPEN-CLOSE; produces: N-DB-OPEN, N-DB-MOD; driver: DRIVER-CMD-01) |
| `TRG-ASSERT-WRITE-TRIGGER-XR` | TriggerAction | acdbOpenObject(F-TRIGGER-XR, kForWrite); assertWriteEnabled(); write bytes HFV35:TRIGGER-XR:1; close(). (target: F-TRIGGER-XR; targetClass: F-TRIGGER-XR; openMode: OPEN-CLOSE; produces: N-OBJ-OPEN, N-DB-OPEN, N-OBJ-MOD, N-DB-MOD, N-OBJ-CLOSED; driver: DRIVER-CMD-01) |
| `TRG-ERASE-TRIGGER-DB` | TriggerAction | acdbOpenObject(F-TRIGGER-ERASE-DB, kForWrite); erase(true); close(). (target: F-TRIGGER-ERASE-DB; targetClass: F-TRIGGER-ERASE-DB; openMode: OPEN-CLOSE; produces: N-DB-ERASE; driver: DRIVER-CMD-01) |
| `TRG-ERASE-TRIGGER-OBJ` | TriggerAction | acdbOpenObject(F-TRIGGER-ERASE-OBJ, kForWrite); erase(true); close(). (target: F-TRIGGER-ERASE-OBJ; targetClass: F-TRIGGER-ERASE-OBJ; openMode: OPEN-CLOSE; produces: N-OBJ-ERASE, N-DB-ERASE; driver: DRIVER-CMD-01) |
| `TRG-ERASE-REF-B` | TriggerAction | 02NO-SM only: acdbOpenObject(F-REF-B, kForWrite); erase(true); close(). F-REF-B is never reopened afterwards (V34 binding §164). (target: F-REF-B; targetClass: F-REF-B; openMode: OPEN-CLOSE; produces: N-OBJ-ERASE, N-DB-ERASE, N-OBJ-CLOSED; driver: DRIVER-CMD-01) |
| `TRG-APPEND-TRIGGER` | TriggerAction | Create F-TRIGGER-APPEND in memory, arm RG-DB-APPEND with its instance pointer, open Model Space kForWrite with acdbOpenObject, appendAcDbEntity, close the point and Model Space. (target: F-TRIGGER-APPEND; targetClass: F-TRIGGER-APPEND; openMode: OPEN-CLOSE; produces: N-DB-APPEND, N-DB-MOD; driver: DRIVER-CMD-01; amendedBy: RC-10) |
| `TRG-OPEN-FOR-MODIFY-XR` | TriggerAction | acdbOpenObject(F-XR, kForWrite); assertWriteEnabled(); no byte is changed; close(). (target: F-XR; targetClass: F-XR; openMode: OPEN-CLOSE; produces: N-DB-OPEN, N-OBJ-OPEN; driver: DRIVER-CMD-01) |
| `TRG-MODIFY-CLOSE-REF-A` | TriggerAction | acdbOpenObject(F-REF-A, kForWrite); assertWriteEnabled() with its default arguments (the graphics-modified flag is left as it sets it); no property value changes; close(). (target: F-REF-A; targetClass: F-REF-A; openMode: OPEN-CLOSE; produces: N-OBJ-OPEN, N-DB-OPEN, N-OBJ-MOD, N-ENT-GFX, N-OBJ-CLOSED, N-DB-MOD; driver: DRIVER-CMD-01) |
| `TRG-TRANSFORM-CLOSE-REF-A` | TriggerAction | acdbOpenObject(F-REF-A, kForWrite); transformBy(AcGeMatrix3d::kIdentity); close(). (target: F-REF-A; targetClass: F-REF-A; openMode: OPEN-CLOSE; produces: N-OBJ-OPEN, N-DB-OPEN, N-ENT-GFX, N-OBJ-MOD, N-DB-MOD, N-OBJ-CLOSED; driver: DRIVER-CMD-01) |
| `TRG-CANCEL-OPEN-XR` | TriggerAction | acdbOpenObject(F-XR, kForWrite); assertWriteEnabled(); write exactly one kDxfText resbuf `HFV35:XR:CANCEL-STAGED` (trigger-only staging value, never a STATE value); cancel(). Then, before the origin body's scheduled delivery can run, open F-XR kForRead and record its bytes (OBS-CANCEL-RESTORED). The staged value is never an ExpectedBefore/ExpectedAfter value. (target: F-XR; targetClass: F-XR; openMode: OPEN-CLOSE; stagedBytes: HFV35:XR:CANCEL-STAGED; produces: N-OBJ-OPEN, N-DB-OPEN, N-OBJ-MOD, N-OBJ-CANCEL, N-OBJ-UNDO, N-OBJ-CLOSED; driver: DRIVER-CMD-01; amendedBy: RC-09) |
| `TRG-START-OUTER-T` | TriggerAction | Require active count 0 (otherwise UNKNOWN); SA-TX-START T-CONTROLLED as outermost; after it returns SA-TX-END T-CONTROLLED. (targetClass: T-CONTROLLED; openMode: N/A; produces: N-TR-ABOUT-START, N-TR-STARTED, N-TR-ABOUT-END, N-TR-OUTERMOST-END-CALLED, N-TR-ENDED; driver: DRIVER-CMD-01; usesSupport: SA-TX-START, SA-TX-END) |
| `TRG-ABORT-PRIMARY-T` | TriggerAction | SA-TX-ABORT on driver-owned T-PRIMARY. (targetClass: T-PRIMARY; openMode: N/A; produces: N-TR-ABOUT-ABORT, N-TR-ABORTED, N-OBJ-CANCEL, N-OBJ-UNDO, N-OBJ-CLOSED; driver: DRIVER-CMD-01; usesSupport: SA-TX-ABORT) |
| `TRG-END-NESTED-T` | TriggerAction | SA-TX-END on T-NESTED while T-PRIMARY is active (count 2 -> 1). T-PRIMARY is ended only by SET-OUTCOME-COMMIT. (targetClass: T-NESTED; openMode: N/A; produces: N-TR-ABOUT-END, N-TR-ENDED; driver: DRIVER-CMD-01; usesSupport: SA-TX-END) |
| `TRG-END-PRIMARY-T` | TriggerAction | SA-TX-END on driver-owned T-PRIMARY with active count 1 (commit/end). (targetClass: T-PRIMARY; openMode: N/A; produces: N-TR-ABOUT-END, N-TR-OUTERMOST-END-CALLED, N-TR-ENDED; driver: DRIVER-CMD-01; usesSupport: SA-TX-END) |
| `TRG-RUN-FIXTURE-CMD` | TriggerAction | DRIVER-SCRIPT-01 issues `I52CTDA_FIXTURE` (CMD-FIXTURE) after I52CTDA_PROBE returned; the command succeeds without database writes. (targetClass: CMD:I52CTDA_FIXTURE; openMode: N/A; produces: N-ED-WILL, N-ED-END; driver: DRIVER-CMD-01) |
| `TRG-LOCK-CYCLE` | TriggerAction | Architect B11: in DRIVER-APP-01 delivery with the document reactor already registered, SA-LOCK(D, kWrite, L"RACKCAD_CTDA_V35", L"RACKCAD_CTDA_V35", false); eOk creates one DRIVER-LOCK-01 obligation; then SA-UNLOCK(D) explicitly. (targetClass: DOC-LOCK; openMode: N/A; produces: N-DOC-LOCK-WILL, N-DOC-LOCK-CHANGED; driver: DRIVER-APP-01; usesSupport: SA-LOCK, SA-UNLOCK) |
| `TRG-LOCK-VETO` | TriggerAction | Architect B11: as TRG-LOCK-CYCLE with SET-ARM-VETO; the WILL callback vetoes; lockDocument is expected non-eOk and creates no obligation. An eOk result creates the obligation, is released by SA-UNLOCK in finally and is evidence for FP-VETO-BYPASS. (targetClass: DOC-LOCK; openMode: N/A; produces: N-DOC-LOCK-WILL, N-DOC-LOCK-VETO; driver: DRIVER-APP-01; usesSupport: SA-LOCK, SA-UNLOCK) |
| `TRG-APPCTX-SYNC` | TriggerAction | From DRIVER-CMD-01 document context call executeInApplicationContext(cb, retained data) synchronously (NX-APPCTX-SYNC); cb emits MARK-APPCTX-ENTRY, runs APPCTX-EXEC-01 and returns; the caller then logs MARK-APPCTX-RETURN with StageId STG-SYNC-APPCTX (same activation). (targetClass: SYNC-APPCTX; openMode: N/A; driver: DRIVER-CMD-01; amendedBy: RC-03) |
| `TRG-APPCTX-SYNC-BEGIN-CMDCTX` | TriggerAction | From DRIVER-APP-01 delivery (no outstanding command) call executeInApplicationContext(cb) synchronously; cb emits MARK-APPCTX-ENTRY, observes isApplicationContext()==true, calls beginExecuteInCommandContext(cmdcb, retained data) capturing its status at SP-CMDCTX and returns; the caller then logs MARK-APPCTX-RETURN with StageId STG-SYNC-APPCTX (same activation). (targetClass: SYNC-APPCTX; openMode: N/A; driver: DRIVER-APP-01; amendedBy: RC-03) |
| `TRG-CANCEL-CMDCTX` | TriggerAction | CANCEL-CMDCTX-01 unchanged: DRIVER-SCRIPT-01 issues HFV34_CANCEL; its body synchronously enters application context and calls beginExecuteInCommandContext(completion callback); the exact acdocman.h guarantee cancels outstanding commands before delivery; N-ED-CANCEL for HFV34_CANCEL must precede the queued delivery. SchedulerUse=TRIGGER-INFRA, no scheduler credit. (targetClass: CMD:HFV34_CANCEL; openMode: N/A; produces: N-ED-WILL, N-ED-CANCEL; driver: DRIVER-CMD-01) |
| `TRG-LOAD-PAYLOAD` | TriggerAction | acedArxLoad(NL-ARX-PATH) once; record the return value and loaded state as MARK-LOAD-RESULT. No unload. (target: R-PAYLOAD-ARX; targetClass: R-PAYLOAD-ARX; openMode: N/A; produces: N-RX-WILL-LOAD, N-RX-LOADED; driver: DRIVER-CMD-01) |
| `TRG-APPEND-TRIGGER-IN-PRIMARY` | TriggerAction | 02NAPP-SM only. Phase order: T-PRIMARY is open (SET-OPEN-PRIMARY-T); create F-TRIGGER-APPEND in memory and arm RG-DB-APPEND with its instance pointer; getObject(Model Space, kForWrite) through T-PRIMARY; appendAcDbEntity(F-TRIGGER-APPEND); objectAppended fires and the body (CB-PRIMARY-01) runs MUT-SM through the same T-PRIMARY (getObject of Model Space again through T-PRIMARY is the same transaction's ownership, and appends F-REF-C, which is a different object from F-TRIGGER-APPEND); the nested objectAppended of F-REF-C is logged and never runs the body; the callback returns; addNewlyCreatedDBRObject(F-TRIGGER-APPEND); the trigger returns; SET-OUTCOME-COMMIT ends T-PRIMARY; the guard is disarmed; postconditions are read at FINISH. (target: F-TRIGGER-APPEND; targetClass: F-TRIGGER-APPEND; openMode: T-PRIMARY; produces: N-DB-APPEND, N-DB-MOD; driver: DRIVER-CMD-01; amendedBy: RC-10) |

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
| `APPCTX-LOCK-01` | DocumentLockAuthority | V35 extension of NPM-V34 §8 APPCTX-LOCK-01. At APPCTX entry (the asynchronous NS-BEGIN-APPCTX delivery, or for 13A-SM only the synchronous NX-APPCTX-SYNC callback that NPM-V34 binds to APPCTX-EXEC-01) resolve the exact scratch D and call lockDocument(D, kWrite, L"RACKCAD_CTDA_V34", L"RACKCAD_CTDA_V34", false). Only eOk creates one fixture-owned unlock obligation (APPCTX-UNLOCK-01); no mutation precedes success; any other status is UNKNOWN. (retainedStatus: RETAINED-EXTENDED; amendedBy: RC-06) |
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
| `MARK-DRIVER-APP-ENTRY` | Marker | DRIVER-APP-01 delivery entry (driver namespace). Never listed in a row's Markers and never satisfies a governed marker. (namespace: DRIVER; amendedBy: RC-03) |
| `MARK-DRIVER-APP-RETURN` | Marker | DRIVER-APP-01 delivery return (driver namespace). Never listed in a row's Markers and never satisfies a governed marker. (namespace: DRIVER; amendedBy: RC-03) |

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
| `OBS-LOCK-RELEASE-BOUND` | ObservationPredicate | MARK-LOCK-RELEASE resolved by LOCK-RELEASE-BIND-01 to exactly one transition of the scratch document inside its window (amendedBy: RC-02) |
| `OBS-PAYLOAD-DB-BINDING` | ObservationPredicate | PAYLOAD-DB-BINDING recorded TRUE at STG-PAYLOAD-INIT (amendedBy: RC-08) |
| `OBS-CANCEL-RESTORED` | ObservationPredicate | after cancel() returned, F-XR read kForRead holds STATE-S-0 bytes (HFV30:S:0) and not `HFV35:XR:CANCEL-STAGED` (amendedBy: RC-09) |

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
| `UNK-FINISH-TIMEOUT` | UnknownPredicate | FIN-GATE-01 timed out, FINISH ran in FINISH-MODE-DRAIN (including FINISH-EARLY), or the control plane terminated the PID before a FINISH record -> UNKNOWN. (amendedBy: RC-01) |
| `UNK-LOG-BINDING` | UnknownPredicate | a module could not bind LOG-SEQ-01, or a record is malformed under LOG-RECORD-01 -> UNKNOWN. (amendedBy: RC-07) |
| `UNK-MARKER-BINDING` | UnknownPredicate | a bound marker matched two or more records of its stage window, or a record lacks the stage/delivery identity required by MARKER-STAGE-BIND-01 -> UNKNOWN. (amendedBy: RC-02, RC-03) |
| `UNK-PAYLOAD-DB` | UnknownPredicate | PAYLOAD-DB-BINDING false or not recorded -> UNKNOWN. (amendedBy: RC-08) |
| `UNK-CANCEL-STAGING` | UnknownPredicate | the cancel staging write or its read-back did not complete -> UNKNOWN. (amendedBy: RC-09) |
| `UNK-LATE-DELIVERY` | UnknownPredicate | a governed delivery or body entry arrived after FINISH-FENCE-01 was set (LATE-DELIVERY record) -> UNKNOWN. (amendedBy: RC-01, RC-04) |

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
| `FP-CLEANUP-SAFETY` | FailPredicate | V34 §5 safety exception. With SAFETY-EVIDENCE-COMPLETE, from the first step of CleanupActionId to process exit, any of: (a) a record whose StageId is in the GOVERNED namespace shows a probe body executing, a write to a fixture resource, a retained reactor opening or dereferencing its notifier (FENCED-RETENTION-SAFE false), or a module record after its fence point; (b) a LATE-DELIVERY record under FINISH-FENCE-01 for a reactor instance whose eOk removal is already recorded, or for a module past its recorded fence point. Cleanup records carry StageId STG-FINISH (INFRA namespace) and are never counted; any other LATE-DELIVERY record is UNK-LATE-DELIVERY. Cleanup that is merely incomplete or unobserved is not this predicate (it is UNKNOWN). (amendedBy: RC-04) |

## 18. Result classes and rule

| Id | Kind | Definition |
|---|---|---|
| `PASS-T` | ResultClass | V35 result class for the S/M transaction-observation rows (09N-A, 09N-B, 09N-O, 09N-C), applied by RESULT-RULE-V35 step 4: every ObservationPredicateId is true and the verifier comparison of RESULT-RULE-V35 holds, where STATE-T-NESTED-PREEND, STATE-T-OUTERMOST-PREEND and STATE-T-OUTERMOST-END-CALLED are compared against the callback-time VER-T event evidence and STATE-S-0 against the fresh FINISH VER-S read, and STATE-T-ENDED against fresh FINISH reads; cleanup and fence complete. (amendedBy: RC-05) |
| `FAIL-T` | ResultClass | V35 result class for the same four rows, applied by RESULT-RULE-V35 step 1 or step 3: EVIDENCE-COMPLETE (or SAFETY-EVIDENCE-COMPLETE for FP-CLEANUP-SAFETY) and a listed FailPredicateId holds. (amendedBy: RC-05) |
| `RESULT-RULE-V35` | ResultRule | Evaluated only by CONTROL-PLANE-RESULT-01 after the CompletionFence evidence exists. Step 1 (safety): if SAFETY-EVIDENCE-COMPLETE and FP-CLEANUP-SAFETY holds, the result is the row's FailClass. Step 2 (UNKNOWN): otherwise, if EVIDENCE-COMPLETE is false, or UNKNOWN-COMMON, or any UnknownPredicateId holds, or any ObservationPredicateId is unavailable, the result is UNKNOWN. Step 3 (FAIL): otherwise, if any other FailPredicateId holds, the result is the row's FailClass. Step 4 (PASS): otherwise, if the PassClass holds, every ObservationPredicateId is true, the verifier comparison equals ExpectedAfter and the CleanupActionId and CompletionFence completed, the result is the row's PassClass. Step 5: otherwise UNKNOWN (an available but false observation without a matching FailPredicate establishes no contrary host property). Verifier comparison: for ExpectedAfter STATE-T-NESTED-PREEND, STATE-T-OUTERMOST-PREEND and STATE-T-OUTERMOST-END-CALLED the STATE-T part is event evidence (the VER-T record captured at the primary callback: transaction ids, active count, pre-end read through the ending transaction) and the STATE-S part is the fresh VER-S read at FINISH, which must equal STATE-S-0; every other ExpectedAfter is compared only with fresh FINISH reads. The results are disjoint; the steps are exhaustive. (amendedBy: RC-05) |
| `EVIDENCE-COMPLETE` | ResultPredicate | Closed predicate, TRUE iff all hold: (1) the phase records of RUN-ENV-01, I52CTDA_BOOT, I52CTDA_PROBE, the script-issued trigger command if any, FIN-GATE-01 and I52CTDA_FINISH are present; (2) every CompletionTokenId of the row is set; (3) every + marker resolved to exactly one bound record and every - marker was absent over the whole run window; (4) every VerifierId read completed; (5) every cleanup step recorded success or a classified failure; (6) the control plane holds the exact-PID exit evidence and the scratch-DWG integrity evidence; (7) Sequence is contiguous from 1 to the last record with no gap and records from every loaded module; (8) every record is well formed under LOG-RECORD-01 and every authority id in it resolves in EXEC-CATALOG-V35-1. (amendedBy: RC-05) |
| `SAFETY-EVIDENCE-COMPLETE` | ResultPredicate | TRUE iff items (6), (7) and (8) of EVIDENCE-COMPLETE hold for the records from the start of CleanupActionId to process exit, and every CleanupStep of the row has a start record. (amendedBy: RC-04) |
| `FENCED-RETENTION-SAFE` | ResultPredicate | For a reactor retained by CLN-OBJ-RETAIN-FENCED: every notification it receives after CLN-BASE starts (including goodbye and other callbacks outside the 26) is only logged with NotifierAccess=NONE; it runs no body, opens no object and dereferences no notifier. When this holds, such notifications are not FP-CLEANUP-SAFETY. (amendedBy: RC-04) |
| `CONTROL-PLANE-RESULT-01` | ResultAuthority | The only authority that classifies a row. It runs in the external research control plane (eng/research/I52Ctda/control-plane) after the scratch process has exited (or was terminated) and applies RESULT-RULE-V35 to its inputs: the single LOG-SEQ-01 log (records of R-NATIVE-ARX, R-PAYLOAD-ARX and R-MANAGED-OBSERVER), the exact-PID exit evidence, the scratch-DWG integrity evidence (the drawing was not saved) and the cleanup/fence records. In-process code, including CMD-FINISH, only prepares evidence. (amendedBy: RC-05) |
| `PAYLOAD-DB-BINDING` | ResultPredicate | At R-PAYLOAD-ARX kInitAppMsg, acdbHostApplicationServices()->workingDatabase() is the exact scratch database recorded by BOOT-01 for this ProbeId and acDocManager->curDocument() is the exact scratch document; otherwise the payload registers nothing and the row is UNKNOWN (UNK-PAYLOAD-DB) with no governed execution. (amendedBy: RC-08) |

## 19. Cleanup steps

| Id | Kind | Definition |
|---|---|---|
| `CLN-OBJ-REMOVE` | CleanupStep | Architect B04: for each retained object/entity reactor of the row, open its notifier while live (kForRead; the disposable erased F-TRIGGER-ERASE-OBJ with openErased=true) and removeReactor(exact instance); non-eOk is UNKNOWN; never depends on N-OBJ-GOODBYE; the 26 callbacks are unchanged |
| `CLN-OBJ-RETAIN-FENCED` | CleanupStep | 02NO-SM only: F-REF-B is erased by the trigger and never reopened (§164); OR-REF-B is not removed, its instance is retained unreleased until process exit and nothing accesses the notifier; FENCE-PROCESS-EXIT is mandatory |
| `CLN-DOC-REMOVE` | CleanupStep | remove the retained RR-DOC from the document manager and verify removal |
| `CLN-DEFER-DRAIN` | CleanupStep | after the delivery token, unregister the origin reactor unless an earlier step of the same CleanupAction already removed it, and verify no queued command/callback remains; in FINISH-MODE-DRAIN, undrained deferred work is recorded (never waited for) and is ended by the process fence; inability to prove drain is UNKNOWN (amendedBy: RC-01) |
| `CLN-BASE` | CleanupStep | single terminal step, run inside CMD-FINISH with StageId STG-FINISH: abort any owned open T; verify no outstanding driver or APPCTX lock obligation; call the R-PAYLOAD-ARX removal export when the payload registered RR-PAYLOAD-DB; unregister each remaining registered observer except those retained by CLN-OBJ-RETAIN-FENCED (an object/entity reactor still registered is removed as CLN-OBJ-REMOVE prescribes, openErased=true only for a disposable erase trigger); erase each live fixture resource incl. F-TRIGGER-APPEND, never reopening an object the trigger erased (F-REF-B in 02NO-SM, the disposable erase triggers); remove R-SM-LINK (LINK-CARRIER-REMOVED); disarm any guard still armed; flush the log; verify each step. CLN-BASE never closes the scratch document: CMD-FINISH's final `_.QUIT` `_Y` discards and closes it after the flush, and the control plane checks the scratch-DWG integrity externally. (amendedBy: RC-04) |
| `CLN-PROCESS-EXIT` | CleanupStep | control-plane step, not in-process: after CMD-FINISH's QUIT-DISCARD, the control plane verifies that the exact PID exited within FIN-GATE-01.postFinishDeadlineSeconds (otherwise it terminates the PID and the row is UNKNOWN), verifies the scratch-DWG integrity and deletes or quarantines the scratch artifacts (amendedBy: RC-05) |

## 20. Cleanup actions

| Id | Kind | Definition |
|---|---|---|
| `CLEAN-BASE` | CleanupAction | Ordered steps CLN-BASE; fence FENCE-CLEANUP-TOKEN. V35 CLEAN-BASE. V35-A1: the scratch DWG is never closed by a cleanup step; CMD-FINISH's QUIT-DISCARD closes it without saving after the flush, and any V34 wording that places the close inside cleanup is superseded. (steps: CLN-BASE; fence: FENCE-CLEANUP-TOKEN; amendedBy: RC-04) |
| `CLEAN-OBJ` | CleanupAction | Ordered steps CLN-OBJ-REMOVE -> CLN-BASE; fence FENCE-CLEANUP-TOKEN. B04: no goodbye dependency. V35-A1: the scratch DWG is never closed by a cleanup step; CMD-FINISH's QUIT-DISCARD closes it without saving after the flush, and any V34 wording that places the close inside cleanup is superseded. (steps: CLN-OBJ-REMOVE, CLN-BASE; fence: FENCE-CLEANUP-TOKEN; amendedBy: RC-04) |
| `CLEAN-OBJ-FENCED` | CleanupAction | Ordered steps CLN-OBJ-RETAIN-FENCED -> CLN-BASE -> CLN-PROCESS-EXIT; fence FENCE-PROCESS-EXIT. new for 02NO-SM (Architect confirmation requested). V35-A1: the scratch DWG is never closed by a cleanup step; CMD-FINISH's QUIT-DISCARD closes it without saving after the flush, and any V34 wording that places the close inside cleanup is superseded. (steps: CLN-OBJ-RETAIN-FENCED, CLN-BASE, CLN-PROCESS-EXIT; fence: FENCE-PROCESS-EXIT; amendedBy: RC-04) |
| `CLEAN-DEFER` | CleanupAction | Ordered steps CLN-DEFER-DRAIN -> CLN-BASE; fence FENCE-CLEANUP-TOKEN. V34 unchanged. V35-A1: the scratch DWG is never closed by a cleanup step; CMD-FINISH's QUIT-DISCARD closes it without saving after the flush, and any V34 wording that places the close inside cleanup is superseded. (steps: CLN-DEFER-DRAIN, CLN-BASE; fence: FENCE-CLEANUP-TOKEN; amendedBy: RC-04) |
| `CLEAN-DOC` | CleanupAction | Ordered steps CLN-DOC-REMOVE -> CLN-BASE; fence FENCE-CLEANUP-TOKEN. V34 unchanged. V35-A1: the scratch DWG is never closed by a cleanup step; CMD-FINISH's QUIT-DISCARD closes it without saving after the flush, and any V34 wording that places the close inside cleanup is superseded. (steps: CLN-DOC-REMOVE, CLN-BASE; fence: FENCE-CLEANUP-TOKEN; amendedBy: RC-04) |
| `CLEAN-OBJ-DEFER` | CleanupAction | Ordered steps CLN-OBJ-REMOVE -> CLN-DEFER-DRAIN -> CLN-BASE; fence FENCE-CLEANUP-TOKEN. C02: one terminal CLN-BASE. V35-A1: the scratch DWG is never closed by a cleanup step; CMD-FINISH's QUIT-DISCARD closes it without saving after the flush, and any V34 wording that places the close inside cleanup is superseded. (steps: CLN-OBJ-REMOVE, CLN-DEFER-DRAIN, CLN-BASE; fence: FENCE-CLEANUP-TOKEN; amendedBy: RC-04) |
| `CLEAN-DOC-DEFER` | CleanupAction | Ordered steps CLN-DOC-REMOVE -> CLN-DEFER-DRAIN -> CLN-BASE; fence FENCE-CLEANUP-TOKEN. C02: one terminal CLN-BASE. V35-A1: the scratch DWG is never closed by a cleanup step; CMD-FINISH's QUIT-DISCARD closes it without saving after the flush, and any V34 wording that places the close inside cleanup is superseded. (steps: CLN-DOC-REMOVE, CLN-DEFER-DRAIN, CLN-BASE; fence: FENCE-CLEANUP-TOKEN; amendedBy: RC-04) |
| `CLEAN-APPCTX-PROCESS` | CleanupAction | Ordered steps CLN-DEFER-DRAIN -> CLN-BASE -> CLN-PROCESS-EXIT; fence FENCE-PROCESS-EXIT. V34 semantics; T_APP unwound and APPCTX-UNLOCK-01 satisfied before CLN-BASE. V35-A1: the scratch DWG is never closed by a cleanup step; CMD-FINISH's QUIT-DISCARD closes it without saving after the flush, and any V34 wording that places the close inside cleanup is superseded. (steps: CLN-DEFER-DRAIN, CLN-BASE, CLN-PROCESS-EXIT; fence: FENCE-PROCESS-EXIT; amendedBy: RC-04) |
| `CLEAN-APPCTX-LOCK-PROCESS` | CleanupAction | Ordered steps CLN-DEFER-DRAIN -> CLN-BASE -> CLN-PROCESS-EXIT; fence FENCE-PROCESS-EXIT. CLEAN-APPCTX-PROCESS plus explicit active-T count zero and no owned lock obligation (V34 §8). V35-A1: the scratch DWG is never closed by a cleanup step; CMD-FINISH's QUIT-DISCARD closes it without saving after the flush, and any V34 wording that places the close inside cleanup is superseded. (steps: CLN-DEFER-DRAIN, CLN-BASE, CLN-PROCESS-EXIT; fence: FENCE-PROCESS-EXIT; amendedBy: RC-04) |
| `CLEAN-CMDCTX-PROCESS` | CleanupAction | Ordered steps CLN-DEFER-DRAIN -> CLN-BASE -> CLN-PROCESS-EXIT; fence FENCE-PROCESS-EXIT. V34 semantics. V35-A1: the scratch DWG is never closed by a cleanup step; CMD-FINISH's QUIT-DISCARD closes it without saving after the flush, and any V34 wording that places the close inside cleanup is superseded. (steps: CLN-DEFER-DRAIN, CLN-BASE, CLN-PROCESS-EXIT; fence: FENCE-PROCESS-EXIT; amendedBy: RC-04) |
| `CLEAN-CHAIN-PROCESS` | CleanupAction | Ordered steps CLN-DEFER-DRAIN -> CLN-BASE -> CLN-PROCESS-EXIT; fence FENCE-PROCESS-EXIT. V34 semantics; all stage tokens; retained object/document reactors removed inside CLN-BASE. V35-A1: the scratch DWG is never closed by a cleanup step; CMD-FINISH's QUIT-DISCARD closes it without saving after the flush, and any V34 wording that places the close inside cleanup is superseded. (steps: CLN-DEFER-DRAIN, CLN-BASE, CLN-PROCESS-EXIT; fence: FENCE-PROCESS-EXIT; amendedBy: RC-04) |
| `CLEAN-C15-PROCESS` | CleanupAction | Ordered steps CLN-BASE -> CLN-PROCESS-EXIT; fence FENCE-PROCESS-EXIT. V34 semantics: retained-reactor removal attempted while notifier live and recorded, non-success forbids PASS; no in-process unload. V35-A1: the scratch DWG is never closed by a cleanup step; CMD-FINISH's QUIT-DISCARD closes it without saving after the flush, and any V34 wording that places the close inside cleanup is superseded. (steps: CLN-BASE, CLN-PROCESS-EXIT; fence: FENCE-PROCESS-EXIT; amendedBy: RC-04) |
| `CLEAN-CANCEL-PROCESS` | CleanupAction | Ordered steps CLN-DEFER-DRAIN -> CLN-BASE -> CLN-PROCESS-EXIT; fence FENCE-PROCESS-EXIT. V34 semantics. V35-A1: the scratch DWG is never closed by a cleanup step; CMD-FINISH's QUIT-DISCARD closes it without saving after the flush, and any V34 wording that places the close inside cleanup is superseded. (steps: CLN-DEFER-DRAIN, CLN-BASE, CLN-PROCESS-EXIT; fence: FENCE-PROCESS-EXIT; amendedBy: RC-04) |

## 21. Completion fences

| Id | Kind | Definition |
|---|---|---|
| `FENCE-CLEANUP-TOKEN` | CompletionFence | All governed runtime state is released and verified in-process by the terminal CLN-BASE; CMD-FINISH then exits AutoCAD with QUIT-DISCARD. The exact-PID exit is evidence completeness (EVIDENCE-COMPLETE item 6), not a PASS condition of its own. (amendedBy: RC-05) |
| `FENCE-PROCESS-EXIT` | CompletionFence | PASS additionally requires the externally verified exit of the exact scratch PID. |

## 22. Surfaces

| Id | Kind | Definition |
|---|---|---|
| `S` | Surface | NPM-V34 §1 surface label: semantic surface. |
| `M` | Surface | NPM-V34 §1 surface label: physical surface. |
| `SM` | Surface | NPM-V34 §1 surface label: mixed sibling/linkage surface. |
| `S/M` | Surface | NPM-V34 §1 surface label: transaction observation over S and M (no mutation). |
| `S+M+SM` | Surface | NPM-V34 §1 surface label: atomic ALL execution over three distinct protected targets. |

## 23. Completion tokens (V35-A1)

| Id | Kind | Definition |
|---|---|---|
| `TOK-PROBE-RETURN` | CompletionToken | I52CTDA_PROBE returned after its post-trigger obligations and guard disarm (or guard hand-over to CMD-FINISH for script-issued triggers). Set by the stage through LOG-SEQ-01 (`I52Ctda_TokenSet`); FIN-GATE-01 reads the in-process token table. (stage: STG-PROBE-CMD; amendedBy: RC-01) |
| `TOK-DRIVER-APP-DONE` | CompletionToken | DRIVER-APP-01 delivery returned (trigger done, guards disarmed, driver lock obligation settled). Set by the stage through LOG-SEQ-01 (`I52Ctda_TokenSet`); FIN-GATE-01 reads the in-process token table. (stage: STG-DRIVER-APP; amendedBy: RC-01) |
| `TOK-FIXTURE-CMD-END` | CompletionToken | commandEnded of I52CTDA_FIXTURE observed by RR-ED. Set by the stage through LOG-SEQ-01 (`I52Ctda_TokenSet`); FIN-GATE-01 reads the in-process token table. (stage: STG-FIXTURE-CMD; amendedBy: RC-01) |
| `TOK-CANCEL-OBSERVED` | CompletionToken | commandCancelled(L"HFV34_CANCEL") observed by RR-ED. Set by the stage through LOG-SEQ-01 (`I52Ctda_TokenSet`); FIN-GATE-01 reads the in-process token table. (stage: STG-CANCEL-CMD; amendedBy: RC-01) |
| `TOK-CANCEL-CMDCTX-DONE` | CompletionToken | the CANCEL-CMDCTX-01 command-context completion callback returned. Set by the stage through LOG-SEQ-01 (`I52Ctda_TokenSet`); FIN-GATE-01 reads the in-process token table. (stage: STG-CANCEL-CMD; amendedBy: RC-01) |
| `TOK-PAYLOAD-LOAD-RESULT` | CompletionToken | acedArxLoad(NL-ARX-PATH) returned and MARK-LOAD-RESULT was recorded. Set by the stage through LOG-SEQ-01 (`I52Ctda_TokenSet`); FIN-GATE-01 reads the in-process token table. (stage: STG-PAYLOAD-LOAD; amendedBy: RC-01) |
| `TOK-ENQUEUE-SEND` | CompletionToken | NS-SEND returned in the origin body (success or rejection recorded). Set by the stage through LOG-SEQ-01 (`I52Ctda_TokenSet`); FIN-GATE-01 reads the in-process token table. (stage: STG-ORIGIN; amendedBy: RC-01) |
| `TOK-SEND-CMD-END` | CompletionToken | commandEnded of I52CTDA_QUEUED observed after SEND-EXEC-01 finished. Set by the stage through LOG-SEQ-01 (`I52Ctda_TokenSet`); FIN-GATE-01 reads the in-process token table. (stage: STG-SEND-DELIVERY; amendedBy: RC-01) |
| `TOK-ENQUEUE-APPCTX` | CompletionToken | NS-BEGIN-APPCTX returned in the origin body (success or rejection recorded). Set by the stage through LOG-SEQ-01 (`I52Ctda_TokenSet`); FIN-GATE-01 reads the in-process token table. (stage: STG-ORIGIN; amendedBy: RC-01) |
| `TOK-APPCTX-DELIVERY-DONE` | CompletionToken | the probe APPCTX delivery callback returned. Set by the stage through LOG-SEQ-01 (`I52Ctda_TokenSet`); FIN-GATE-01 reads the in-process token table. (stage: STG-APPCTX-DELIVERY; amendedBy: RC-01) |
| `TOK-ENQUEUE-CMDCTX` | CompletionToken | NS-BEGIN-CMDCTX returned in its caller (success or rejection recorded). Set by the stage through LOG-SEQ-01 (`I52Ctda_TokenSet`); FIN-GATE-01 reads the in-process token table. (stage: STG-SYNC-APPCTX\|STG-APPCTX-DELIVERY; amendedBy: RC-01) |
| `TOK-CMDCTX-DELIVERY-DONE` | CompletionToken | the CMDCTX delivery callback returned. Set by the stage through LOG-SEQ-01 (`I52Ctda_TokenSet`); FIN-GATE-01 reads the in-process token table. (stage: STG-CMDCTX-DELIVERY; amendedBy: RC-01) |
| `TOK-SYNC-RETURN` | CompletionToken | executeInApplicationContext returned to its caller. Set by the stage through LOG-SEQ-01 (`I52Ctda_TokenSet`); FIN-GATE-01 reads the in-process token table. (stage: STG-SYNC-APPCTX; amendedBy: RC-01) |
| `TOK-EXEC-DONE` | CompletionToken | the ExecutionContextId finished: execution transaction ended or aborted and any owned lock released. Set by the stage through LOG-SEQ-01 (`I52Ctda_TokenSet`); FIN-GATE-01 reads the in-process token table. (stage: STG-EXEC; amendedBy: RC-01) |
| `TOK-OUTCOME` | CompletionToken | the driver transaction lifecycle finished: SET-OUTCOME-COMMIT or the trigger's own end/abort of T-PRIMARY/T-NESTED/T-CONTROLLED returned. Set by the stage through LOG-SEQ-01 (`I52Ctda_TokenSet`); FIN-GATE-01 reads the in-process token table. (stage: STG-PROBE-CMD; amendedBy: RC-01) |
| `TOK-MANAGED-CMD-END` | CompletionToken | RR-MANAGED-CMD recorded Document.CommandEnded of I52CTDA_FIXTURE. Set by the stage through LOG-SEQ-01 (`I52Ctda_TokenSet`); FIN-GATE-01 reads the in-process token table. (stage: STG-FIXTURE-CMD; amendedBy: RC-01) |
| `TOK-LOCK-RELEASE` | CompletionToken | MARK-LOCK-RELEASE resolved by LOCK-RELEASE-BIND-01 at the anchor of the row's bound stage (COMMAND-END-WINDOW or APPCTX-UNLOCK-01-CALL). Set through LOG-SEQ-01 (`I52Ctda_TokenSet`); FIN-GATE-01 reads the in-process token table. (stage: BOUND:MARK-LOCK-RELEASE; amendedBy: RC-01, RC-02) |

## 24. Stages (V35-A1)

| Id | Kind | Definition |
|---|---|---|
| `STG-PROBE-CMD` | Stage | Stage identity: activation of I52CTDA_PROBE. Namespace GOVERNED. Every record of this stage carries StageId=STG-PROBE-CMD and a DeliveryId. (namespace: GOVERNED; requires: always; amendedBy: RC-01, RC-03) |
| `STG-TRIGGER` | Stage | Stage identity: execution of TriggerActionId. Namespace GOVERNED. Every record of this stage carries StageId=STG-TRIGGER and a DeliveryId. (namespace: GOVERNED; requires: always; amendedBy: RC-01, RC-03) |
| `STG-PRIMARY-CALLBACK` | Stage | Stage identity: the body-running PrimaryAuthority callback. Namespace GOVERNED. Every record of this stage carries StageId=STG-PRIMARY-CALLBACK and a DeliveryId. (namespace: GOVERNED; requires: primaryBody; amendedBy: RC-01, RC-03) |
| `STG-ORIGIN` | Stage | Stage identity: the ScheduleOrigin callback. Namespace GOVERNED. Every record of this stage carries StageId=STG-ORIGIN and a DeliveryId. (namespace: GOVERNED; requires: origin; amendedBy: RC-01, RC-03) |
| `STG-SEND-DELIVERY` | Stage | Stage identity: activation of I52CTDA_QUEUED (NS-SEND delivery). Namespace GOVERNED. Every record of this stage carries StageId=STG-SEND-DELIVERY and a DeliveryId. (namespace: GOVERNED; requires: chain:NS-SEND; amendedBy: RC-01, RC-03) |
| `STG-APPCTX-DELIVERY` | Stage | Stage identity: the probe's NS-BEGIN-APPCTX delivery (the only stage of CHAIN-APPCTX; stage 1 of CHAIN-APPCTX-CMDCTX). Namespace GOVERNED. Every record of this stage carries StageId=STG-APPCTX-DELIVERY and a DeliveryId. (namespace: GOVERNED; requires: chain:NS-BEGIN-APPCTX; amendedBy: RC-01, RC-03) |
| `STG-CMDCTX-DELIVERY` | Stage | Stage identity: the NS-BEGIN-CMDCTX delivery. Namespace GOVERNED. Every record of this stage carries StageId=STG-CMDCTX-DELIVERY and a DeliveryId. (namespace: GOVERNED; requires: chain:NS-BEGIN-CMDCTX; amendedBy: RC-01, RC-03) |
| `STG-SYNC-APPCTX` | Stage | Stage identity: one activation spans from the executeInApplicationContext call to its return in the caller; the callback records (MARK-APPCTX-ENTRY) and the caller's post-return record (MARK-APPCTX-RETURN, TOK-SYNC-RETURN) carry StageId=STG-SYNC-APPCTX and the same DeliveryId. Covers the NX-APPCTX-SYNC entry of the row's chain and the HFV34_CANCEL entry of CANCEL-CMDCTX-01. Namespace GOVERNED. (namespace: GOVERNED; requires: syncEntry; amendedBy: RC-01, RC-03) |
| `STG-FIXTURE-CMD` | Stage | Stage identity: activation of I52CTDA_FIXTURE. Namespace GOVERNED. Every record of this stage carries StageId=STG-FIXTURE-CMD and a DeliveryId. (namespace: GOVERNED; requires: trigger:TRG-RUN-FIXTURE-CMD; amendedBy: RC-01, RC-03) |
| `STG-CANCEL-CMD` | Stage | Stage identity: activation of HFV34_CANCEL. Namespace GOVERNED. Every record of this stage carries StageId=STG-CANCEL-CMD and a DeliveryId. (namespace: GOVERNED; requires: trigger:TRG-CANCEL-CMDCTX; amendedBy: RC-01, RC-03) |
| `STG-PAYLOAD-LOAD` | Stage | Stage identity: the acedArxLoad(NL-ARX-PATH) call. Namespace GOVERNED. Every record of this stage carries StageId=STG-PAYLOAD-LOAD and a DeliveryId. (namespace: GOVERNED; requires: payload; amendedBy: RC-01, RC-03) |
| `STG-PAYLOAD-INIT` | Stage | Stage identity: R-PAYLOAD-ARX kInitAppMsg. Namespace GOVERNED. Every record of this stage carries StageId=STG-PAYLOAD-INIT and a DeliveryId. (namespace: GOVERNED; requires: payload; amendedBy: RC-01, RC-03) |
| `STG-EXEC` | Stage | Stage identity: the ExecutionContextId activation. Namespace GOVERNED. Every record of this stage carries StageId=STG-EXEC and a DeliveryId. (namespace: GOVERNED; requires: exec; amendedBy: RC-01, RC-03) |
| `STG-DRIVER-APP` | Stage | Stage identity: DRIVER-APP-01 delivery (driver namespace). Namespace DRIVER. Every record of this stage carries StageId=STG-DRIVER-APP and a DeliveryId. (namespace: DRIVER; requires: driver:DRIVER-APP-01; amendedBy: RC-01, RC-03) |
| `STG-FIN-GATE` | Stage | Stage identity: FIN-GATE-01 idle checks. Namespace INFRA. Every record of this stage carries StageId=STG-FIN-GATE and a DeliveryId. (namespace: INFRA; requires: always; amendedBy: RC-01, RC-03) |
| `STG-FINISH` | Stage | Stage identity: activation of I52CTDA_FINISH and the exit. Namespace INFRA. Every record of this stage carries StageId=STG-FINISH and a DeliveryId. (namespace: INFRA; requires: always; amendedBy: RC-01, RC-03) |

## 25. Marker binding rules (V35-A1)

| Id | Kind | Definition |
|---|---|---|
| `MARKER-STAGE-BIND-01` | MarkerBindingRule | A bound marker `M@S` in MarkerStageBindings is satisfied only by a LOG-RECORD-01 record with EventOrMarkerId M, StageId S, the row's ProbeId and a DeliveryId of the row's own activation of S. Records of DRIVER or INFRA stages never satisfy a governed marker; DRIVER-APP-01 emits only MARK-DRIVER-APP-ENTRY/RETURN. Every MARK-* marker and every N-ED-* marker of a row has exactly one binding; zero matching records is marker absence, two or more is UNK-MARKER-BINDING. N-ED-WILL/N-ED-END bind to the command of the bound stage (I52CTDA_QUEUED, I52CTDA_FIXTURE, HFV34_CANCEL, the unique command enclosing EP-CMDCTX and its token excluding I52CTDA_BOOT/PROBE/FIXTURE/QUEUED/FINISH and HFV34_CANCEL, or I52CTDA_PROBE). MARK-MANAGED-CMD-END@STG-FIXTURE-CMD binds to Document.CommandEnded of I52CTDA_FIXTURE. (amendedBy: RC-03) |
| `LOCK-RELEASE-BIND-01` | MarkerBindingRule | MARK-LOCK-RELEASE@S has two anchor kinds, fixed per stage by `anchors`. COMMAND-END-WINDOW (STG-SEND-DELIVERY, STG-FIXTURE-CMD, STG-CMDCTX-DELIVERY): the FIRST documentLockModeChanged of the scratch document to an unlocked mode strictly after commandEnded of the stage's command and strictly before the next commandWillStart on the scratch document; FINISH's activation closes the window at the latest, so a FINISH unlock can never satisfy it. APPCTX-UNLOCK-01-CALL (STG-APPCTX-DELIVERY, STG-SYNC-APPCTX): the documentLockModeChanged of the scratch document emitted between entry and return of the stage's own APPCTX-UNLOCK-01 unlockDocument call (for 13A-SM the new mode is the enclosing I52CTDA_PROBE command lock, not necessarily unlocked); it precedes the stage's completion token, so TOK-LOCK-RELEASE never waits on a later event. No candidate is marker absence (UNKNOWN); two candidates in one window are UNK-MARKER-BINDING. (amendedBy: RC-02) |

## 26. Invariants and marker binding

| Id | Definition |
|---|---|
| `INV-TRIGGER-DISJOINT` | For rows whose body executes inside the trigger's callback (CB-PRIMARY-01, CB-EXEC-01, EDC-EXEC-01), the trigger target identity is not in the MutationActionId write set. |
| `INV-ERASE-TRIGGER-DISJOINT` | For every row, the target of an erase trigger (TRG-ERASE-TRIGGER-DB, TRG-ERASE-TRIGGER-OBJ, TRG-ERASE-REF-B) is not in the MutationActionId write set. |
| `INV-SINGLE-TERMINAL-BASE` | Every CleanupAction contains exactly one CLN-BASE and nothing but CLN-PROCESS-EXIT after it (R3-C02). |
| `INV-MARKER-OBSERVER` | Every marker has an observer registration of its family in the row. |
| `INV-BODY-OBSERVER` | A row that runs a probe body names exactly one BodyObserverId, registered by the row, whose family matches the body callback (PrimaryAuthority or ScheduleOrigin EventId), plus the family guard; every other registration of the row is observation-only. |
| `INV-MARKER-PRODUCER` | Every + marker that is an EventId is a candidate product (produces attribute, per exact header semantics) of the row's driver, setup, trigger, scheduler chain, execution context or mutation; an object/entity marker additionally requires that producer to touch an object watched by one of the row's object/entity observers (touches attribute: trigger target, staged F-XR for TRG-ABORT-PRIMARY-T, mutation write set); runtime absence remains UNKNOWN. |
| `INV-FINISH-GATED` | I52CTDA_FINISH is entered only through FIN-GATE-01 after every CompletionTokenId of the row, or in FINISH-MODE-DRAIN (UNKNOWN). |
| `INV-DRIVER-MARKER-DISJOINT` | No governed marker binds to a DRIVER or INFRA stage; DRIVER-APP-01 emits only MARK-DRIVER-APP-*. |
| `INV-APPEND-TRANSACTION` | A CB-PRIMARY-01 row whose mutation appends to Model Space uses a Model-Space trigger in openMode T-PRIMARY. |
| `INV-CONTROL-PLANE-RESULT` | Only CONTROL-PLANE-RESULT-01 classifies, after the process exited. |

| Marker family | Binding |
|---|---|
| `all bound markers` | MARKER-STAGE-BIND-01 (per-row MarkerStageBindings column) |
| `MARK-LOCK-RELEASE` | LOCK-RELEASE-BIND-01 |
| `N-DOC-*` | exact scratch document |
| `N-DB-*` | scratch database |
| `N-TR-*` | scratch database transaction manager |
| `N-OBJ-*/N-ENT-GFX` | an object/entity reactor registered by the row on its notifier |
| `N-RX-*` | exact NL-ARX-PATH module |

## 27. Referenced ObjectARX members

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
| `acedRegisterOnIdleWinMsg` | `core_rxmfcapi.h` |
| `acedRemoveOnIdleWinMsg` | `core_rxmfcapi.h` |
| `AcApDocument::isQuiescent` | `acdocman.h` |
