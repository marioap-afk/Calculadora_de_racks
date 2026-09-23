# I-52 — ObjectARX Member Capability V33

> **OMC-V33-1 / NORMATIVE / NOT EXECUTED.** Member-level reconciliation of ObjectARX 2025 SDK 25.0.58.0
> against AutoCAD R25.0.171.0.0. Machine-readable source:
> `docs/automation/evidence/I-52-objectarx-member-reconciliation-v33.json`.

## 1. Scope and exact identity

The census enumerates every callable declaration, with overloads separated, in:

- `AcDbDatabaseReactor`, `AcDbObjectReactor`, `AcDbEntityReactor`, `AcTransactionReactor`;
- `AcEditorReactor`, `AcApDocManagerReactor`, `AcRxDLinkerReactor`, `AcApDocManager`;
- both exact `AcEditorReactor2/3` aliases;
- exact database/object/transaction/editor/linker registration APIs;
- `acedArxLoad` and `acrxLoadApp`.

Data fields and nested-type fields are not callable API members. The eight governing header hashes remain exactly those
published by V32; the evidence generator aborted on any byte mismatch. `EXACT_HEADER` remains above web documentation.

## 2. Member-level counts

```text
Exact callable/alias/support members = 216
Header MATCH = 209
Header DRIFT retained from prior authority = 7
IN newly recognized = 11
OBSERVATION_ONLY members = 10
OUT_BY_OPERATION members = 166
UNKNOWN members = 0
Reachable callbacks = 26
Mutation-capable reachable callbacks = 26
Deferred-capable reachable callbacks = 26
Observation-only reachable callbacks = 0
OUT callbacks = 126
UNKNOWN callbacks = 0
```

MATCH/DRIFT is the declaration-history axis. IN/OBSERVATION_ONLY/OUT/UNKNOWN is the HF capability axis; the axes are
not added together. Seven individual declarations carry retained drift because V32 had grouped some of them.

## 3. Capability taxonomy

| Capability | Meaning | Closure obligation |
|---|---|---|
| `PRIMARY_MUTATION_CAPABLE` | controlled override code runs at a reachable callback; exact header does not forbid protected-state mutation | one or more primary probes |
| `DEFERRED_SCHEDULER_CAPABLE` | callback can invoke an IN scheduler unless exact authority/operation excludes it | material scheduler composition probes |
| `OBSERVATION_ONLY_BY_AUTHORITY` | exact declaration only returns/observes state and takes no callback code | no mutator probe |
| `OUT_BY_OPERATION` | bounded HF does not trigger/invoke the member during the measured interval | exact operation reason, no credit |
| `UNKNOWN` | capability or reachability unresolved | prevents closure |

`MARKER_FOR` is only a probe relation. It is never a capability classification.

## 4. Reachable callback inventory

| EventId | Exact callback | Capabilities | V33 primary coverage | Deferred coverage |
|---|---|---|---|---|
| `N-DB-APPEND` | `AcDbDatabaseReactor::objectAppended` | mutation + deferred | `02NAPP-S/M/SM` | `CAPP16SND-ALL + CAPP16APP-ALL` |
| `N-DB-ERASE` | `AcDbDatabaseReactor::objectErased` | mutation + deferred | `retained NPM-V32 primary probe` | `exact NS-SEND and NS-BEGIN-APPCTX origin rows in NPM-V33` |
| `N-DB-MOD` | `AcDbDatabaseReactor::objectModified` | mutation + deferred | `retained NPM-V32 primary probe` | `exact NS-SEND and NS-BEGIN-APPCTX origin rows in NPM-V33` |
| `N-DB-OPEN` | `AcDbDatabaseReactor::objectOpenedForModify` | mutation + deferred | `retained NPM-V32 primary probe` | `exact NS-SEND and NS-BEGIN-APPCTX origin rows in NPM-V33` |
| `N-DOC-LOCK-CHANGED` | `AcApDocManagerReactor::documentLockModeChanged` | mutation + deferred | `retained NPM-V32 primary probe` | `exact NS-SEND and NS-BEGIN-APPCTX origin rows in NPM-V33` |
| `N-DOC-LOCK-VETO` | `AcApDocManagerReactor::documentLockModeChangeVetoed` | mutation + deferred | `retained NPM-V32 primary probe` | `exact NS-SEND and NS-BEGIN-APPCTX origin rows in NPM-V33` |
| `N-DOC-LOCK-WILL` | `AcApDocManagerReactor::documentLockModeWillChange` | mutation + deferred | `retained NPM-V32 primary probe` | `exact NS-SEND and NS-BEGIN-APPCTX origin rows in NPM-V33` |
| `N-ED-CANCEL` | `AcEditorReactor::commandCancelled` | mutation + deferred | `02NEDC-ALL` | `CEDC16SND-ALL + CEDC16APP-ALL` |
| `N-ED-END` | `AcEditorReactor::commandEnded` | mutation + deferred | `retained NPM-V32 primary probe` | `exact NS-SEND and NS-BEGIN-APPCTX origin rows in NPM-V33` |
| `N-ED-WILL` | `AcEditorReactor::commandWillStart` | mutation + deferred | `02NEDW-ALL` | `CEDW16SND-ALL + CEDW16APP-ALL` |
| `N-ENT-GFX` | `AcDbEntityReactor::modifiedGraphics` | mutation + deferred | `retained NPM-V32 primary probe` | `exact NS-SEND and NS-BEGIN-APPCTX origin rows in NPM-V33` |
| `N-OBJ-CANCEL` | `AcDbObjectReactor::cancelled` | mutation + deferred | `retained NPM-V32 primary probe` | `exact NS-SEND and NS-BEGIN-APPCTX origin rows in NPM-V33` |
| `N-OBJ-CLOSED` | `AcDbObjectReactor::objectClosed` | mutation + deferred | `retained NPM-V32 primary probe` | `exact NS-SEND and NS-BEGIN-APPCTX origin rows in NPM-V33` |
| `N-OBJ-ERASE` | `AcDbObjectReactor::erased` | mutation + deferred | `retained NPM-V32 primary probe` | `exact NS-SEND and NS-BEGIN-APPCTX origin rows in NPM-V33` |
| `N-OBJ-MOD` | `AcDbObjectReactor::modified` | mutation + deferred | `retained NPM-V32 primary probe` | `exact NS-SEND and NS-BEGIN-APPCTX origin rows in NPM-V33` |
| `N-OBJ-OPEN` | `AcDbObjectReactor::openedForModify` | mutation + deferred | `retained NPM-V32 primary probe` | `exact NS-SEND and NS-BEGIN-APPCTX origin rows in NPM-V33` |
| `N-OBJ-UNDO` | `AcDbObjectReactor::modifyUndone` | mutation + deferred | `retained NPM-V32 primary probe` | `exact NS-SEND and NS-BEGIN-APPCTX origin rows in NPM-V33` |
| `N-RX-LOADED` | `AcRxDLinkerReactor::rxAppLoaded` | mutation + deferred | `02NRXL-ALL` | `CRXL16SND-ALL + CRXL16APP-ALL` |
| `N-RX-WILL-LOAD` | `AcRxDLinkerReactor::rxAppWillBeLoaded` | mutation + deferred | `02NRXW-ALL` | `CRXW16SND-ALL + CRXW16APP-ALL` |
| `N-TR-ABORTED` | `AcTransactionReactor::transactionAborted` | mutation + deferred | `02NTA-ALL` | `CTA16SND-ALL + CTA16APP-ALL` |
| `N-TR-ABOUT-ABORT` | `AcTransactionReactor::transactionAboutToAbort` | mutation + deferred | `retained NPM-V32 primary probe` | `exact NS-SEND and NS-BEGIN-APPCTX origin rows in NPM-V33` |
| `N-TR-ABOUT-END` | `AcTransactionReactor::transactionAboutToEnd` | mutation + deferred | `retained NPM-V32 primary probe` | `exact NS-SEND and NS-BEGIN-APPCTX origin rows in NPM-V33` |
| `N-TR-ABOUT-START` | `AcTransactionReactor::transactionAboutToStart` | mutation + deferred | `02NTAS-ALL` | `CTAS16SND-ALL + CTAS16APP-ALL` |
| `N-TR-ENDED` | `AcTransactionReactor::transactionEnded` | mutation + deferred | `retained NPM-V32 primary probe` | `exact NS-SEND and NS-BEGIN-APPCTX origin rows in NPM-V33` |
| `N-TR-OUTERMOST-END-CALLED` | `AcTransactionReactor::endCalledOnOutermostTransaction` | mutation + deferred | `retained NPM-V32 primary probe` | `exact NS-SEND and NS-BEGIN-APPCTX origin rows in NPM-V33` |
| `N-TR-STARTED` | `AcTransactionReactor::transactionStarted` | mutation + deferred | `02NTS-ALL` | `CTS16SND-ALL + CTS16APP-ALL` |

All 26 reachable callback origins receive both SEND and APPCTX deferred coverage. The distinct
APPCTX→CMDCTX second leg is characterized by `C2APP2CMD-ALL`. `MUT-ALL` executes S, M and SM independently; it does
not infer that SM entails the other classes.

## 5. OUT callback rule

The remaining 126 callback members are individually recorded in the JSON with an operation-specific reason.
`commandCancelled` is reachable through the controlled lock-veto/cancellation path and has primary plus deferred
characterization. `commandFailed` is OUT because no bounded HF operation is allowed to fail the command. Copy,
undo/redo append, save, document lifecycle, xref, modeless, unload and teardown families remain OUT only for their exact
absent operation.

## 6. Closure

```text
ReachableCallbackCapabilityClosed(HF-V33) iff
  every reachable callback is primary-characterized
  AND every reachable callback has concrete deferred coverage or a material composition proof
  AND every OUT callback has an operation-specific member row
  AND no capability UNKNOWN remains.

NativeCallbackCapabilitySetClosed(HF-V33) = TRUE
NativeReactorClassSetClosed(HF-V33) = TRUE
```

The predicate closes definition coverage only. A future illegal mutation, rejected enqueue, missing callback or incomplete
cleanup produces probe UNKNOWN/FAIL under NPM-V33 and cannot create nominal CTDA_HOST_PASS.
