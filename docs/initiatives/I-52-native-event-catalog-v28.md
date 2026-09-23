# I-52 — Native Event Catalog V28

> **NEC-V28-1 / DEFINITION CONTRACT.** Exact ObjectARX 2025 events, schedulers and probes. Every probe is
> `DEFINED / NOT EXECUTED`; no row asserts runtime PASS.

## 1. Native event catalog

For every callback, `PASS` means the future log and independent DB reread establish the row oracle. A callback that
does not execute, a required field that cannot be observed, an illegal mutation whose required property therefore
cannot be measured, or tuple drift yields `UNKNOWN/FAIL`, never omission.

| NativeEventId | Class / exact callback | Header | Trigger/phase | T/lock/write contract | Threats | ProbeIds |
|---|---|---|---|---|---|---|
| N-DB-OPEN | `AcDbDatabaseReactor::objectOpenedForModify(const AcDbDatabase*,const AcDbObject*)` | `dbmain.h` | before Modify operation | observe T/top-T/lock; mutate other fixture target only through explicit T | T2 | 02N |
| N-DB-MOD | `AcDbDatabaseReactor::objectModified(const AcDbDatabase*,const AcDbObject*)` | `dbmain.h` | after Modify operation completes | observe T/top-T/lock; schedule only after log | T2,T8,T16 | 16N-S; 16N-M; 16N-SM; C15N16N-SM |
| N-DB-ERASE | `AcDbDatabaseReactor::objectErased(const AcDbDatabase*,const AcDbObject*,bool)` | `dbmain.h` | after erase/unerase completes | observation marker; no assumed write legality | T2,T7,T12 | marker for SM probes |
| N-TR-ABOUT-ABORT | `AcTransactionReactor::transactionAboutToAbort(int&,AcDbTransactionManager*)` | `dbtrans.h` | before terminate processing starts | current count includes aborting T; attempt mutation only on fixture target | T2,T3,T4 | 04N |
| N-TR-ABORTED | `AcTransactionReactor::transactionAborted(int&,AcDbTransactionManager*)` | `dbtrans.h` | active T terminated | count excludes terminated T; verification marker | T2,T3 | 04N marker |
| N-TR-ABOUT-END | `AcTransactionReactor::transactionAboutToEnd(int&,AcDbTransactionManager*)` | `dbtrans.h` | before end processing starts | current count includes ending T; `count==1` is outermost specialization | T2,T4 | 09N-A; 09N-B |
| N-TR-ENDED | `AcTransactionReactor::transactionEnded(int&,AcDbTransactionManager*)` | `dbtrans.h` | T is ending; count excludes it | observe DB/T; any mutation attempt is explicit and fail-closed | T2,T4,T6 | 09N-C |
| N-ED-WILL | `AcEditorReactor::commandWillStart(const ACHAR*)` | `aced.h` | command about to begin | lifecycle marker | T2,T16 | all queued-command probes marker |
| N-ED-END | `AcEditorReactor::commandEnded(const ACHAR*)` | `aced.h` | command completed | exact docs permit transaction manipulation if ended/aborted before callback return | T2,T4,T6,T8 | 09N-D; 10N-S; 10N-M; 10N-SM; 16N markers |
| N-ED-CANCEL | `AcEditorReactor::commandCancelled(const ACHAR*)` | `aced.h` | command cancelled/unsuccessful | failure marker only | T2,T16 | lifecycle marker |
| N-ED-FAIL | `AcEditorReactor::commandFailed(const ACHAR*)` | `aced.h` | command failed | failure marker only | T2,T16 | lifecycle marker |
| N-RX-LOADED | `AcRxDLinkerReactor::rxAppLoaded(const ACHAR*)` | `rxdlinkr.h` | ARX loaded, initialized and classes registered | load marker, never mutation authority | T15 | C15N16N-SM marker |

Registration is exact: database `addReactor`, `actrTransactionManager->addReactor`, editor reactor registration and
dynamic-linker registration use one retained helper instance each; cleanup removes the same instances before unload.
Registration or cleanup failure makes every dependent probe `UNKNOWN`.

Normative publishers/registration:

| Family | Publisher/scope | Registration | Removal |
|---|---|---|---|
| N-DB | scratch `AcDbDatabase*` only | `scratchDb->addReactor(dbReactor)` | `scratchDb->removeReactor(dbReactor)` |
| N-TR | exact transaction manager associated with scratch document | `actrTransactionManager->addReactor(txReactor)` | `actrTransactionManager->removeReactor(txReactor)` |
| N-ED | AutoCAD editor singleton | `acedEditor->addReactor(editorReactor)` | `acedEditor->removeReactor(editorReactor)` |
| N-RX | ObjectARX dynamic linker singleton | `acrxDynamicLinker->addReactor(linkerReactor)` | `acrxDynamicLinker->removeReactor(linkerReactor)` |

Every callback filters the exact scratch database/document, fixture ids and research command names before acting. An
event from any other database/document is logged as out-of-scope and cannot satisfy a probe.

## 2. Native scheduler and load catalog

| Id | Exact API | Header | Enqueue/execution contract | Context/T/lock | Probes | Status |
|---|---|---|---|---|---|---|
| NS-SEND | `AcApDocManager::sendStringToExecute(AcApDocument*,const ACHAR*,bool,bool,bool)` | `acdocman.h` | from document context with `activate=false`, string is queued; later command delivery is observed | target current doc; callback command acquires its own normal command context/T | 16N-S; 16N-M; 16N-SM; C15N16N-SM | DEFINED |
| NS-BEGIN-CMDCTX | `AcApDocManager::beginExecuteInCommandContext(void(*)(void*),void*)` | `acdocman.h` | request queues from application context; caller must return before callback | exact application-context prerequisite; pointer lifetime retained | application-context T13/T16 supplement | DEFINED / SUPPLEMENTAL |
| NL-ARX-PATH | `acedArxLoad(const ACHAR*)` | `acedads.h` | loads helper path; return code logged | load action, not scheduler or mutation | C15N16N-SM setup | DEFINED |

NS-SEND parameters are fixed: current `AcApDocument*`, one exact research command string plus terminator,
`activate=false`, `wrapUpInactiveDoc=false`, `echo=false`. SchedulePoint is immediately before the call; EnqueueResult
is its `Acad::ErrorStatus`; ExecutionPoint is first instruction of the registered research command. `inputPending()`
may be logged but is not an oracle.

NS-SEND enqueue requires a live target document and document context. It creates no T at enqueue; the delivered
research command receives normal command context/lock and creates its own T. Expected phase is after the scheduling
callback and current command yield/return, with exact relation to N-ED-END determined by the probe. The documented API
exposes no cancellation token/handle used by this contract. N-ED-CANCEL/N-ED-FAIL and absence of delivery are recorded;
they yield FAIL/UNKNOWN, never PASS by timeout.

NS-BEGIN-CMDCTX requires application context, non-null callback/data lifetime and an active MDI document. Autodesk
documents queue acceptance and that the caller must return before delivery, with outstanding commands cancelled before
the callback. Lock/T at execution are observed and explicitly acquired; none is assumed from enqueue. It has no
contractual cancellation handle used by V28. Because these semantics differ materially from NS-SEND, its evidence is
kept separate and supplemental.

## 3. Normative native probe schema

```text
ProbeId; PrimaryAuthorityId; ScheduleOriginEventId; HeaderAuthority; Setup; Trigger;
PrimaryTransactionId; TopTransactionId; DocumentLock; Thread/Context; Mutation; S/M/SM;
SchedulePoint; ExecutionPoint; ExpectedBefore; ExpectedAfter; Verifier; LifecycleMarkers;
Threats; DA-P/H-P; PASS; FAIL; UNKNOWN; Cleanup.
```

Every row has one `PrimaryAuthorityId`: an exact NativeEventId or NativeSchedulerId. `ScheduleOriginEventId` is the
event that invokes a scheduler and is not a second primary authority.

## 4. Probe definitions

### 4.1 02N — native same-context affiliation

```text
PrimaryAuthorityId = N-DB-OPEN
Setup = primary T writes fixture notifier A; destination Xrecord B has immutable before bytes
Mutation = open B through exact ObjectId in observed transaction manager and replace semantic bytes
Class = S
Verifier = fresh DB reread + callback/primary/top T ids + lock/thread/context log
PASS = callback executed, mutation legal, bytes and T affiliation match the observed outcome contract
FAIL = write escapes required rollback/commit relationship or contradicts logged affiliation
UNKNOWN = callback absent, target cannot be opened, or T/lock identity unobservable
Cleanup = remove reactor; abort/commit only as owning scenario prescribes
```

The const notifier pointer is never written. Runtime legality of opening B is measured.

### 4.2 04N — abort

```text
PrimaryAuthorityId = N-TR-ABOUT-ABORT
LifecycleMarkers = N-TR-ABORTED; N-ED-CANCEL; N-ED-FAIL
Setup = 02N mutation staged inside primary T, then harness requests abort
Mutation = controlled S marker on separate fixture target during about-to-abort, if legal
Class = S/M as two fixture targets under one probe record
Verifier = post-abort fresh DB read plus before/about/after T counts
PASS = exact order observed and no staged fixture mutation survives abort
FAIL = a transaction-affiliated staged mutation survives or callbacks contradict order
UNKNOWN = either required callback/count/read unavailable
```

### 4.3 09N-A — transaction about-to-end

`PrimaryAuthorityId=N-TR-ABOUT-END`. A nested T is ended with `numTransactions>1`; the probe logs callback order,
top-T identity and semantic/physical visibility. It never labels the callback commit-phase. PASS requires the exact
pre-end notification and expected nested visibility; contradiction is FAIL; missing count/identity is UNKNOWN.

### 4.4 09N-B — outermost specialization

`PrimaryAuthorityId=N-TR-ABOUT-END`, predicate `numTransactions==1`. This is not a second callback and not named
`endCalledOnOutermostTransaction`. PASS means the exact outermost specialization and final pre-end visibility are
observed. Any distinct-callback assumption or absent count is UNKNOWN/FAIL.

### 4.5 09N-C — transaction-ended

`PrimaryAuthorityId=N-TR-ENDED`. Verifier records that count excludes the ending T and rereads committed fixture state
from a fresh T when legal. It characterizes post-end visibility, not an undocumented postcommit guarantee.

### 4.6 09N-D — editor command-ended

`PrimaryAuthorityId=N-ED-END`. The callback starts an explicit short transaction, attempts a controlled fixture write,
ends/aborts before returning, and records relation to method return, managed/native CommandEnded, lock release and
`C_decide/C_report/C_host`. PASS requires the documented callback and complete observed order. `N-ED-END` is not
automatically `C_host` or SUCCESS.

### 4.7 10N-S / 10N-M / 10N-SM

| ProbeId | Primary authority | Exact mutation | Verifier | DA-P |
|---|---|---|---|---|
| 10N-S | N-ED-END | destination Xrecord semantic bytes | SemanticVerifier + LifecycleVerifier | P6,P12 |
| 10N-M | N-ED-END | outer BlockReference transform then dedicated layer assignment | PhysicalVerifier + LifecycleVerifier | P11,P12 |
| 10N-SM | N-ED-END | add/erase sibling reference and update controlled linkage Xrecord | SemanticVerifier + SiblingVerifier + PhysicalVerifier | P6,P11,P12 |

Each uses a fresh explicit T that ends before callback return. PASS requires mutation result and total order relative to
all C points. Mutation after irreversible success makes the candidate boundary FAIL; inability to locate a usable
pre-success fence is UNKNOWN/FAIL, never catalog incompleteness.

### 4.8 16N-S / 16N-M / 16N-SM

| ProbeId | Primary authority | Schedule origin | Deferred command mutation | DA-P |
|---|---|---|---|---|
| 16N-S | NS-SEND | N-DB-MOD | destination Xrecord semantic bytes | P6,P12 |
| 16N-M | NS-SEND | N-DB-MOD | transform/dedicated layer | P11,P12 |
| 16N-SM | NS-SEND | N-DB-MOD | sibling add/erase plus linkage Xrecord | P6,P11,P12 |

Setup registers three research-only commands. The N-DB-MOD callback records SchedulePoint and calls NS-SEND. The
queued command records ExecutionPoint, obtains its own legal command T, mutates, commits, and returns. Verifiers record:
schedule, enqueue result, native/managed command will/end, source method return, lock release, C points, deferred
execution, fresh DB state and cleanup. No sleep is an oracle.

PASS for definition execution means all markers are observed and the cross-boundary relation is unambiguous. For host
admission, any successful causal mutation beyond the proposed C_host or after C_report makes P6/P11/P12 FAIL. No
delivery or incomplete markers is UNKNOWN.

## 5. Threat mappings and composition

| Threat | Native mechanism | Probe | Resolution |
|---|---|---|---|
| T1 | managed same-context | HEC-V27-C1 | managed authority retained |
| T2 | exact database/transaction/editor reactors above | 02N;04N;09N;10N | DEFINED |
| T3 | transaction reactor plus nested/abort setup | 04N;09N-A/B/C | DEFINED |
| T4 | N-TR-ABOUT-END/N-TR-ENDED/N-ED-END, without invented commit callback | 09N-A/B/C/D | DEFINED |
| T5 | commit visibility | 09N-C/D plus managed 05/09 | DEFINED |
| T6 | post-end interval | 09N-C/D | DEFINED |
| T7 | interleaved scans | managed 08/12 plus native marker injection | DEFINED |
| T8 | postverification mutation | 10N and 16N | DEFINED |
| T9 | semantic/NOD mutation | 10N-S/16N-S plus managed 11 | DEFINED |
| T10 | source mutation | managed 14; native scheduler can target controlled source | DEFINED |
| T11 | semantic/linkage | 10N-SM/16N-SM | DEFINED |
| T12 | physical-only | 10N-M/16N-M | DEFINED |
| T13 | other context | NS-BEGIN-CMDCTX supplement + managed MS-CONTEXT | DEFINED, exact scopes separate |
| T14 | tuple drift | exact tuple invalidation | DEFINED |
| T15 | controlled module load/registration | NL-ARX-PATH + N-RX-LOADED | DEFINED |
| T16 | native queued command | NS-SEND + 16N-S/M/SM | DEFINED |

### 5.1 T2+T4

09N-A/B/C are native transaction callbacks and therefore directly represent the composition. No extra probe is
required: the event providing T4 timing is itself T2. 09N-D covers the separate editor-end class. Independent results
are not merged across callback classes.

### 5.2 T2+T16

16N-S/M/SM are the composed probes: N-DB-MOD invokes NS-SEND before returning; the exact queued command executes the
mutation later. Managed emulation gets no credit. This composition is IN and DEFINED.

### 5.3 T15+T16 native

`C15N16N-SM` setup calls `NL-ARX-PATH`; `N-RX-LOADED` proves load completed after initialization. The controlled module
registers N-DB-MOD; that callback invokes NS-SEND; the queued research command performs the SM mutation. Load,
registration, schedule, execution, mutation and verification have distinct markers. Failure at any link is
UNKNOWN/FAIL. Arbitrary DLL behavior is not inferred.

### 5.4 Composition rule

```text
ComposedProbeRequired(A,B) iff
  order or interference of A+B can produce an outcome not entailed by the independently observed contracts.
```

T2+T4 is directly embodied by 09N callback classes. T2+T16 and T15+T16 require and receive composed probes. T3+T8
and T13+T16 remain covered by their exact transaction/context setup plus 10/16 verifier when no new scheduler/event
semantics are introduced; a new class invalidates this conclusion.

## 6. Lifecycle integration

Every native log record contains monotonic sequence, UTC timestamp, native thread id, document/database identity,
callback/scheduler id, command name, primary/top T addresses represented as opaque ids, active-T count, lock/context
observations, SchedulePoint, ExecutionPoint, method return, N/M CommandEnded, lock release, `F_usable`, `C_decide`,
`C_report`, candidate `C_host`, verifier read and cleanup.

Native callbacks do not create a success model. V24 remains authoritative:

```text
F_usable -> C_decide -> C_report
C_host evidence must already be usable at F_usable
```

If 10N or 16N can mutate after a candidate while causally scheduled before it, that candidate fails. If the only
usable evidence arrives after C_report, P12 fails.

## 7. Closure predicates

```text
NativeProbeCatalogClosed(NEC-V28-1) iff
  every required NativeEventId has OFFICIAL_EXACT_VERSION_DOC or EXACT_HEADER
  AND every callback signature and phase is exact
  AND every native ProbeId has exactly one primary authority
  AND every required NativeSchedulerId is exact
  AND every probe defines setup, mutation, verifier, oracle and cleanup
  AND every material composition is defined or proven unnecessary
  AND no candidate, cross-version authority, wildcard or placeholder remains
  AND no UNKNOWN exists in definition authority

EventCatalogClosed =
  ManagedProbeCatalogClosed(HEC-V27-C1)
  AND NativeProbeCatalogClosed(NEC-V28-1)

HostFixtureContractClosed(HF-V28-1) =
  HF-V26FixtureCoreClosed
  AND EventCatalogClosed
  AND composed-threat definitions closed
  AND HF-V27 host-to-kind residual model closed
```

Result of definition review:

```text
ManagedProbeCatalogClosed = TRUE
NativeProbeCatalogClosed = TRUE
EventCatalogClosed = TRUE
HostFixtureContractClosed(HF-V28-1) = TRUE
CTDA_HOST_PASS = NOT EVALUATED
```

## 8. No universal inference

The helper characterizes only exact host callback/scheduler classes on the exact tuple. It does not enumerate plugins,
prove arbitrary plugin behavior or establish context isolation. A product/event/scheduler class absent from NEC/HEC
must pass the mandatory HF-V27 HostToKind gate or get kind-specific evidence.

## 9. Invalidation

Invalidate affected definition/evidence on change to: official 2025 authority/signature, acquired header/lib bytes,
SDK/toolset/platform, helper registration, callback/scheduler binding, schedule parameters, command flags, mutation,
verifier/oracle, C-point logging, host tuple, helper/harness/probe SHA, fixture contract or composition mapping.
