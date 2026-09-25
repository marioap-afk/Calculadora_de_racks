# I-52 — Native Scheduler Catalog V35

> **NSC-V35-1 / DRAFT / ARCHITECTURE CANDIDATE / NOT EXECUTED.** Parent NSC-V34-1 blob `3ca54ef1992c3b15d2382a42f09693cd02cabdb6`. The scheduler
> universe remains three. V35 names the NS-SEND delivery contract and separates probe scheduler pairs from infrastructure use.

## 1. Exact scheduler universe

| SchedulerId | Exact API | Delivery | V35 execution context |
|---|---|---|---|
| `NS-SEND` | `AcApDocManager::sendStringToExecute(AcApDocument*,const ACHAR*,bool,bool,bool)` | queued `I52CTDA_QUEUED` in the scratch document | `SEND-EXEC-01` (SEND-LOCK-01, owned T_SEND) |
| `NS-BEGIN-CMDCTX` | `Acad::ErrorStatus beginExecuteInCommandContext(void (*)(void*),void*)` | queued command context after the application-context caller returns | `CMDCTX-EXEC-01` |
| `NS-BEGIN-APPCTX` | `Acad::ErrorStatus beginExecuteInApplicationContext(void (*)(void*),void*)` | asynchronous application context after return to the message loop | `APPCTX-EXEC-01` |

`executeInApplicationContext` remains the synchronous context action `NX-APPCTX-SYNC`, never a scheduler.

## 2. Scheduler use classes

| Class | Meaning | Uses |
|---|---|---|
| `PRIMARY` | the row's PrimaryAuthorityId; the only source of scheduler pairs | 63 |
| `CHAIN-STAGE` | an earlier stage of the row's SchedulerChainId (C2APP2CMD-ALL stage 1) | 1 |
| `DRIVER-INFRA` | DRIVER-APP-01 delivery used by Architect B11 lock triggers and 16C | 12 |
| `TRIGGER-INFRA` | CANCEL-CMDCTX-01 (TRG-CANCEL-CMDCTX) | 3 |

Only `PRIMARY` creates scheduler pairs. Infrastructure uses are listed per ProbeId in `traceability-v35.json`.

## 3. Delivery contracts

APPCTX and CMDCTX delivery are unchanged from NSC-V34 §2-§3. NS-SEND delivery is `SEND-EXEC-01`: CMD-QUEUED delivered at
EP-COMMAND, MDI active document equals the scratch document, SEND-LOCK-01 observed, fresh owned T_SEND, mutation, end/abort,
completion token. Every rejection, nondelivery, missing lock or transaction failure is UNKNOWN.

## 4. Closure

Scheduler pairs = 63 (NS-BEGIN-APPCTX 28, NS-BEGIN-CMDCTX 4, NS-SEND 31).
`SchedulerDefinitionClosed` and `SchedulerDeliveryFixtureClosed` are proven by the V35 validator, not by definition.
