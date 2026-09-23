# I-52 — Native Scheduler Catalog V34

> **NSC-V34-1 / NORMATIVE / NOT EXECUTED.** Parent V33 blob `97779278285a5bce939a728dba9097dbeda6047e`. Scheduler universe remains three; V34 closes delivery executability.

## 1. Exact scheduler universe

| SchedulerId | Exact API | Delivery | Required fixture contract |
|---|---|---|---|
| `NS-SEND` | `AcApDocManager::sendStringToExecute(AcApDocument*,const ACHAR*,bool,bool,bool)` | queued command in target document | delivered command owns/observes its command lock and opens a fresh transaction |
| `NS-BEGIN-CMDCTX` | `Acad::ErrorStatus beginExecuteInCommandContext(void (*)(void*),void*)` | queued command context after application-context caller returns | `CMDCTX-EXEC-01`; caller must observe application context; no APPCTX lock transfers |
| `NS-BEGIN-APPCTX` | `Acad::ErrorStatus beginExecuteInApplicationContext(void (*)(void*),void*)` | asynchronous application context after return to message loop | `APPCTX-EXEC-01` for every mutation |

`executeInApplicationContext` is synchronous `NX-APPCTX-SYNC`, not T16.

## 2. APPCTX delivery

Every direct or callback-origin APPCTX row binds `APPCTX-LOCK-01`, `APPCTX-TX-01`, `APPCTX-UNLOCK-01` and `APPCTX-EXEC-01`. The target is the exact scratch document. Lock failure, missing document, transaction failure, end/abort failure, unlock failure, nondelivery or incomplete drain is UNKNOWN. None may PASS.

## 3. CMDCTX delivery

`beginExecuteInCommandContext` is called only while `isApplicationContext()==true`. At delivery `CMDCTX-LOCK-01` requires observed write-capable command ownership; `CMDCTX-TX-01` owns a fresh T_CMD. APPCTX lock/T never transfers. Missing authority is UNKNOWN.

## 4. Closure

`SchedulerDefinitionClosed = TRUE`. `SchedulerDeliveryFixtureClosed = TRUE` as a definition contract. Therefore `T16NativeSchedulerSetClosed = TRUE`; execution remains NOT EVALUATED.
