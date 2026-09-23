# I-52 — Fixture Execution Contract V34

> **FEC-V34-1 / NORMATIVE / NOT EXECUTED.** This contract corrects only V33 executability findings.

## 1. Support-action taxonomy

| ActionId | Exact member | Legal context | Success/ownership | Release/failure |
|---|---|---|---|---|
| SA-VETO | AcApDocManagerReactor::veto | derived controlled reactor during its documentLockModeWillChange callback | eOk; requests veto of current notification | VETOED must be observed; otherwise UNKNOWN |
| SA-LOCK | AcApDocManager::lockDocument | delivered APPCTX with exact target document | eOk creates one unlock obligation | SA-UNLOCK; any rejection UNKNOWN |
| SA-UNLOCK | AcApDocManager::unlockDocument | after owned T_APP ended/aborted | eOk releases exact owned lock | non-eOk UNKNOWN |
| SA-TX-RESOLVE | AcApDocument::transactionManager | after exact document resolution | non-null manager | null UNKNOWN |
| SA-TX-START | AcDbTransactionManager::startTransaction | after legal lock/context | non-null owned T | null UNKNOWN |
| SA-TX-END | AcDbTransactionManager::endTransaction | owned active T success path | eOk ends T | non-eOk UNKNOWN |
| SA-TX-ABORT | AcDbTransactionManager::abortTransaction | owned incomplete/abort path | eOk unwinds T | non-eOk UNKNOWN |

Support actions are neither EventIds nor SchedulerIds. Required actions cannot be OUT. Count = 7; unresolved = 0.

## 2. Database callback triggers

`F-TRIGGER-MOD`, `F-TRIGGER-ERASE-DB` and `F-TRIGGER-ERASE-OBJ` are disposable, uniquely identified and disjoint from protected S/M/SM targets. `RG-DB-MOD`, `RG-DB-ERASE` and `RG-OBJ-ERASE` provide one-shot reentrancy. Unexpected recursion is UNKNOWN. Erase-origin probes never open an erased protected target.

## 3. Veto and cancellation causal graphs

Lock path: `documentLockModeWillChange -> SA-VETO -> require documentLockModeChangeVetoed`. `documentLockModeChanged` is recorded only when observed. There is no edge to `commandCancelled`.

Cancel path: start exact `HFV34_CANCEL`; synchronously enter application context; call `beginExecuteInCommandContext`; the exact header says all outstanding commands are cancelled before its callback. Require `commandCancelled(L"HFV34_CANCEL")` before queued CMDCTX delivery. Each attempt uses a fresh scratch process and no human/keyboard input. Missing exact callback is UNKNOWN.

## 4. APPCTX/CMDCTX execution

APPCTX uses `APPCTX delivery -> D -> SA-LOCK -> SA-TX-RESOLVE -> SA-TX-START(T_APP) -> mutation -> SA-TX-END or SA-TX-ABORT -> SA-UNLOCK -> completion token`. CMDCTX observes a write-capable host command lock, owns fresh T_CMD, and never inherits APPCTX lock/T. All failure conditions in NPM are UNKNOWN.

## 5. Closure predicate

`FixtureExecutionContractClosed` iff every required probe has a deterministic trigger definition, legal call context, required lock/T path, live target, deterministic cleanup, zero support action OUT/UNKNOWN and no unsupported causal edge. V34 mechanical values are all zero for violations; therefore `FixtureExecutionContractClosed = TRUE` as a definition contract.
