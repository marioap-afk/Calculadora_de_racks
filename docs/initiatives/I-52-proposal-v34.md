# I-52 — Proposal V34 — executable fixture closure

> **DESIGN / AUTHORITY ONLY. NO CT-DA EXECUTION.** Baseline exact I-52 `d1506ac68bddf5880fdf934ca39cf824d9d64a40`; parent V33 blobs: Proposal `ca8ab3f472f37dab7e2ba757684293ba83d89e69`, capability `0f7548005df35d6ab63b03faa055a129a3c6dfe7`, scheduler `97779278285a5bce939a728dba9097dbeda6047e`, NEC `1d79687e2d6a5ebc06325b2d015f0bf599e48e5d`, NPM `d6261f683456eea179953ee8c2978e4e8fd962a7`.

## 1. Status and V33 findings

Architect V33 required V34 for four findings: absent primary coverage for N-DB-MOD/N-DB-ERASE; veto classified OUT plus unsupported veto→cancel causality; absent APPCTX document-lock/T lifecycle; and erase-origin use of an erased protected target. V33 consensus is not reached and its closure claims are superseded.

## 2. Exact tuple and authority

ObjectARX 2025 SDK `25.0.58.0`, x64, v143 at `D:\Downloads\CDROM1`; host AutoCAD `R25.0.171.0.0`. Exact headers remain stronger than web docs. Governing SHA-256: `dbmain.h` `5B79A74DEF06A768B849D2C88F979BF4C15C307EA88D4ACF8FE2A8EE4CA586CD`; `dbtrans.h` `D3EBB73E88FB37E3205611818143D1643A24933815452C6B807E46D8C44999C6`; `acdocman.h` `4B71C08BA578F81FC67898AC3519D0D5069C638A74F52D256DD539D02BDE422D`; `aced.h` `C28C3974BBA868B49B9946F052957700CCC1655E7FB17689FEBE46AA15B319AD`.

## 3. V34 delta

V34 adds six concrete primary probes: `02NDBMOD-S/M/SM` and `02NDBERASE-S/M/SM`. Disposable trigger objects are disjoint from protected targets and governed by one-shot guards. All erase-origin deferred rows now erase a disposable trigger.

`veto()`, `lockDocument`, `unlockDocument`, document transaction-manager retrieval, and start/end/abort transaction APIs are IN fixture support actions. They create no fake event or scheduler identity.

Lock-veto and command cancellation are separate causal graphs. Cancellation uses the exact `beginExecuteInCommandContext` header guarantee that all outstanding commands are cancelled before delivery. The controlled command name is recorded and no keyboard/human input is used.

Every APPCTX mutation binds one immutable lock/T/unlock skeleton. CMDCTX owns a different fresh transaction and may not inherit APPCTX ownership.

## 4. Primary callback closure

`PrimaryCoverageComplete = TRUE` because NEC-V34 projects at least one `PRIMARY_FOR` probe for every one of 26 reachable EventIds. N-DB-MOD and N-DB-ERASE each have three. Runtime callback nondelivery or illegal mutation remains UNKNOWN and cannot become PASS.

## 5. Veto, cancellation and lock path

`documentLockModeWillChange -> SA-VETO -> require documentLockModeChangeVetoed`. `documentLockModeChanged` is only an observation when present. No edge to `commandCancelled` exists.

`HFV34_CANCEL -> NX-APPCTX-SYNC -> beginExecuteInCommandContext -> exact cancellation guarantee -> N-ED-CANCEL -> later CMDCTX delivery`. Failure to observe the exact cancellation is UNKNOWN. Each attempt uses a dedicated scratch process.

## 6. APPCTX and CMDCTX

APPCTX: delivery, exact D, `lockDocument(kWrite,...,prompt=false)`, non-null transaction manager, owned `T_APP`, mutation, end/abort, unlock, completion token. Any rejected/unknown stage is UNKNOWN. CMDCTX requires observed write-capable command ownership and owned `T_CMD`; APPCTX ownership never transfers.

## 7. Mechanical result

NPM-V34 has 100 unique ProbeIds, 24 fields per row, deterministic expected state and cleanup. NEC-V34 has 27 EventIds, 26 reachable callbacks, primary coverage 26/26, 237 exact event relations, three SchedulerIds and 63 scheduler pairs. Support actions = 7, unresolved = 0. Patched exact-member census = 220 (213 MATCH, 7 DRIFT), UNKNOWN = 0.

## 8. Closure predicates

```text
FixtureExecutionContractClosed = TRUE
ActualHeaderReconciliationComplete = TRUE
NativeCallbackCapabilitySetClosed = TRUE
SchedulerDefinitionClosed = TRUE
SchedulerDeliveryFixtureClosed = TRUE
T16NativeSchedulerSetClosed = TRUE
NativeReactorClassSetClosed = TRUE
NativeProbeCatalogClosed = TRUE
ManagedProbeCatalogClosed = TRUE
EventCatalogClosed = TRUE
HostFixtureContractClosed = TRUE
CTDA_HOST_PASS = NOT EVALUATED
```

These are definition-contract results only. No helper, harness, fixture or probe has been implemented or run.

## 9. Architect V33 finding closure

| Finding | Root cause | V34 correction | Affected probes/contracts/member rows | Mechanical result | Status |
|---|---|---|---|---|---|
| BLOCKER-1 | N-DB-MOD and N-DB-ERASE were marker/origin only | six concrete primary probes with distinct triggers and guards | `02NDBMOD-S/M/SM`, `02NDBERASE-S/M/SM`, `RG-DB-MOD`, `RG-DB-ERASE` | reachable primary coverage `26/26` | CLOSED |
| BLOCKER-2 | required `veto()` was OUT and veto was treated as cancel cause | SA-VETO is IN; veto graph has no cancel edge; `CANCEL-CMDCTX-01` uses exact CMDCTX cancellation authority | `10NDOC-VETO-SM`, `02NEDC-ALL`, `CEDC16SND-ALL`, `CEDC16APP-ALL`, veto member row | undocumented-causality dependency `0` | CLOSED |
| BLOCKER-3 | APPCTX mutations had no owned lock/T lifecycle | immutable APPCTX lock, T_APP, unlock and cleanup contracts; CMDCTX owns separate T_CMD | all 28 NS-BEGIN-APPCTX rows; lock/unlock/transaction member rows | missing APPCTX lock `0`; missing APPCTX T `0`; support unknown `0` | CLOSED |
| HIGH-1 | erase-origin rows erased a protected MUT-ALL target | disposable DB/object erase triggers disjoint from live protected targets | `CDBERASE16SND-ALL`, `COBJERASE16SND-ALL`, `CDBERASE16APP-ALL`, `COBJERASE16APP-ALL` | erased protected target uses `0` | CLOSED |

## 10. Host-to-kind and product boundary

The 27 EventIds, three SchedulerIds and seven support actions enter future compatibility comparison where applicable. No host result admits a kind. Selective, Dynamic, PushBack, Cantilever, Header and Flow Bed remain OPEN. I-55 remains unchanged: host research impact NON-MATERIAL; HostToKind/KindContract coordination MATERIAL; future reconciliation REQUIRED; evidence transfer NONE.

Foundation AUTH-01..13 and I-49 (`SymbolId + complete RootCauses(variable)` per actually read variable) remain unchanged. AUTH-15 is I-52 OWNED / NOT IMPLEMENTED.

## 11. Disposition and governance

V33 member census and three-scheduler universe remain incorporated; its closure assertions are superseded. V32 outermost callback, hashes and exact-header precedence remain. V31 is historical; OA-V28 absence is superseded for SDK 25.0.58.0.

V18, Freeze V18 and O-1 V18 remain governing. ADR-0036 remains PROPOSED / AMENDED FOR V18. G3 is STOPPED; G3B NOT OPEN; CT-50 NOT EXECUTED. LIMITED CT-DA stays ACTIVE / PAUSED PENDING V34 ARCHITECT REVIEW. Technical consensus V34 is not reached.
