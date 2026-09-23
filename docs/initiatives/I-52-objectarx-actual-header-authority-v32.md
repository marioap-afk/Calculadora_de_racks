# I-52 — ObjectARX 2025 actual-header authority V32

> **AH-V32-1 / EXACT_HEADER / DEFINITION ONLY.** Source baseline
> `bd32692f761ed889c49a11312b0145013c36e3c9`. No helper, harness, probe or AutoCAD CT-DA was executed.

## 1. Tuple and precedence

| Item | Exact identity |
|---|---|
| SDK root | `D:\Downloads\CDROM1` |
| Product | Autodesk AutoCAD 2025 Object ARX SDK and Documentation |
| SDK version / platform / toolset | `25.0.58.0` / x64 / v143 |
| Host | AutoCAD `R25.0.171.0.0` |
| `acad.exe` SHA-256 | `2A75996FD2A5C5EE0376FD5BA7CCED227A098193FF495E8CEEDA3A96FE326422` |

For this tuple, `EXACT_HEADER` governs API definition over `OFFICIAL_EXACT_VERSION_DOC` when they conflict. Exact
version documentation remains explanatory evidence. Runtime order, write legality and persistence that the header does
not specify remain future probe questions; the header does not manufacture behavioral PASS.

## 2. Governing bytes

| Header | SHA-256 |
|---|---|
| `dbmain.h` | `5B79A74DEF06A768B849D2C88F979BF4C15C307EA88D4ACF8FE2A8EE4CA586CD` |
| `dbtrans.h` | `D3EBB73E88FB37E3205611818143D1643A24933815452C6B807E46D8C44999C6` |
| `dbObject.h` | `A4EAF108D93FB7F30590EA059ACD5FC7DAA55C639F7807833974B0BB77D8E515` |
| `aced.h` | `C28C3974BBA868B49B9946F052957700CCC1655E7FB17689FEBE46AA15B319AD` |
| `acdocman.h` | `4B71C08BA578F81FC67898AC3519D0D5069C638A74F52D256DD539D02BDE422D` |
| `rxdlinkr.h` | `CD6D76037BC2DBA192D3ECDBC703ACCFBD2E67D9AB5AC130901C6A3EA4308F3A` |
| `acedads.h` | `1B5E3B525389BEB4A20730ECF7054E597362E8A1418922394561B88ADFA33370` |
| `rxregsvc.h` | `CABD20D3CD27C3DFAF2F7E6B111903C87AA725FD54DA8C120E12EA526A2C34C3` |

Candidate libraries are available but are not yet claimed as linked: `acdb25.lib`
`0A4749A9942D7DB38CCC351A2BFE88989A36504247B35FF367C2A8655499FABA`, `accore.lib`
`893BC5275F4EFF2797E0F43F66560880C723E6DBF318D3CE2BCECE86F044D121`, `acad.lib`
`8E6C83AD3306A2DC59D8E7C3096A86837B729DFEA7449B19ADE44FA71D052EAE`, and `rxapi.lib`
`8FA74034FB35496F6122EA43F7D3CD8E3CB1F32760A0944CE8D3E592756B10D9`.

## 3. Complete reconciliation table

The normative surface contains 50 items: 35 `MATCH`, 5 `DRIFT`, 10 explicitly bounded `OUT`, and 0 unresolved
definition items.

| # | Normative item | Previous authority/assumption | Actual header declaration | Status | Impact/correction |
|---:|---|---|---|---|---|
| 1 | `AcDbDatabaseReactor` class | OA-V28 docs | `dbmain.h`, derives `AcRxObject` | MATCH | retained |
| 2 | `objectOpenedForModify` | two const pointers | `(const AcDbDatabase*,const AcDbObject*)` | MATCH | `N-DB-OPEN` |
| 3 | `objectModified` | two const pointers | `(const AcDbDatabase*,const AcDbObject*)` | MATCH | `N-DB-MOD` |
| 4 | `objectErased` | pointers + bool | `(const AcDbDatabase*,const AcDbObject*,bool)` | MATCH | `N-DB-ERASE` |
| 5 | `objectAppended` | omitted from event census | `(const AcDbDatabase*,const AcDbObject*)` | DRIFT | add marker `N-DB-APPEND` for fixture sibling creation |
| 6 | DB unappend/reappend | not claimed | `objectUnAppended`, `objectReAppended` | OUT | HF performs no UNDO/REDO append path |
| 7 | DB sysvar/proxy/goodbye family | not claimed | four exact callbacks | OUT | fixture changes no header sysvar/proxy and DB teardown is after evidence |
| 8 | database add/remove reactor | registration assumed | exact database reactor registration APIs | MATCH | retained |
| 9 | `AcDbObjectReactor` class | NEC-V30 docs | `dbmain.h`, derives `AcRxObject` | MATCH | retained |
| 10 | `openedForModify` | const object | `(const AcDbObject*)` | MATCH | `N-OBJ-OPEN` |
| 11 | `modified` | const object | `(const AcDbObject*)` | MATCH | `N-OBJ-MOD` |
| 12 | `erased` | const object + bool | `(const AcDbObject*,bool)` | MATCH | `N-OBJ-ERASE` |
| 13 | `cancelled` | const object | `(const AcDbObject*)` | MATCH | `N-OBJ-CANCEL` |
| 14 | `modifyUndone` | const object | `(const AcDbObject*)` | MATCH | `N-OBJ-UNDO` |
| 15 | `objectClosed` | ObjectId by value | `(const AcDbObjectId)` | MATCH | `N-OBJ-CLOSED` |
| 16 | `goodbye` | const object | `(const AcDbObject*)` | MATCH | cleanup marker only |
| 17 | copied/subobject/XData | OUT in V29 | exact callbacks present | OUT | no copy, subobject or XData operation in HF |
| 18 | object unappend/reappend | OUT in V29 | exact callbacks present | OUT | no object UNDO/REDO append path |
| 19 | object add/remove reactor | registration assumed | `dbObject.h`, `Acad::ErrorStatus ... (AcDbObjectReactor*) const` | MATCH | exact registration/removal authority added |
| 20 | `AcDbEntityReactor` inheritance | derives object reactor | `public AcDbObjectReactor` | MATCH | retained |
| 21 | `modifiedGraphics` | const entity | `(const AcDbEntity*)` | MATCH | `N-ENT-GFX` |
| 22 | `dragCloneToBeDeleted` | not claimed | two const entity pointers | OUT | fixture performs no drag clone |
| 23 | `AcTransactionReactor` class | OA-V28 docs | `dbtrans.h`, derives `AcRxObject` | MATCH | retained |
| 24 | transaction start pair | omitted | `transactionAboutToStart`, `transactionStarted` | DRIFT | add exact marker EventIds |
| 25 | `transactionAboutToEnd` | `(int&,manager*)` | exact match | MATCH | `09N-A/B` retained |
| 26 | `transactionEnded` | `(int&,manager*)` | exact match | MATCH | `09N-C` retained |
| 27 | `transactionAboutToAbort` | `(int&,manager*)` | exact match | MATCH | `04N` primary |
| 28 | `transactionAborted` | `(int&,manager*)` | exact match | MATCH | abort marker |
| 29 | `endCalledOnOutermostTransaction` | explicitly assumed absent | `(int&,AcDbTransactionManager*)` | DRIFT | new EventId and `09N-O` |
| 30 | `objectIdSwapped` | not claimed | two const objects + manager | OUT | HF does not swap ObjectIds |
| 31 | transaction add/remove reactor | registration assumed | exact manager reactor APIs | MATCH | retained |
| 32 | `AcEditorReactor` class | OA-V28 docs | `aced.h`, derives `AcRxEventReactor` | MATCH | retained |
| 33 | four command callbacks | WILL/END/CANCEL/FAIL | four `(const ACHAR*)` virtuals | MATCH | retained |
| 34 | editor Lisp/DXF/DWG/save/xref/etc. | bounded OUT | exact neighboring families present | OUT | no such operation inside measured HF interval |
| 35 | `AcEditorReactor2/3` | treated as possible added classes | preprocessor aliases to `AcEditorReactor` | DRIFT | no separate class/event universe; alias rationale corrected |
| 36 | editor add/remove reactor | event registration assumed | `AcEditor` accepts `AcRxEventReactor*` | MATCH | retained |
| 37 | `AcApDocManagerReactor` class | NEC-V30 docs | exact class in `acdocman.h` | MATCH | retained |
| 38 | three lock callbacks | exact four/five args | exact signatures and `DocLockMode` parameters | MATCH | retained |
| 39 | other document callbacks | bounded OUT | create/destroy/activate/current families present | OUT | scratch document lifecycle is outside measured interval; no mid-probe switch |
| 40 | doc-manager add/remove reactor | registration assumed | exact manager APIs | MATCH | retained |
| 41 | `AcRxDLinkerReactor` class | OA-V28 docs | exact class in `rxdlinkr.h` | MATCH | retained |
| 42 | pre-load callback | omitted | `rxAppWillBeLoaded(const ACHAR*)` | DRIFT | add marker `N-RX-WILL-LOAD` |
| 43 | `rxAppLoaded` | const ACHAR pointer | exact match | MATCH | retained |
| 44 | load-abort/unload family | not used | five exact callbacks | OUT | no closure credit; failed load is FAIL/UNKNOWN and cleanup uses process exit |
| 45 | linker add/remove reactor | registration assumed | exact dynamic-linker APIs | MATCH | retained |
| 46 | `sendStringToExecute` | five parameters/defaults | exact pure virtual signature | MATCH | `NS-SEND` retained |
| 47 | `beginExecuteInCommandContext` | callback + data | exact `ACCORE_PORT` signature and queued semantics | MATCH | catalogued, no NPM primary row |
| 48 | `beginExecuteInApplicationContext` | explicitly excluded | exact asynchronous API exists | OUT | outside claimed scheduler scope; cannot inherit PASS |
| 49 | `acedArxLoad` | path load function | exact declaration in `acedads.h` | MATCH | T15 load action retained |
| 50 | `acrxLoadApp` | supporting load API | exact declaration in `rxregsvc.h` | MATCH | supporting authority, not mutation trigger |

## 4. Full transaction-reactor inventory

```cpp
virtual void transactionAboutToStart(int&, AcDbTransactionManager*) {}
virtual void transactionStarted(int&, AcDbTransactionManager*) {}
virtual void transactionAboutToEnd(int&, AcDbTransactionManager*) {}
virtual void transactionEnded(int&, AcDbTransactionManager*) {}
virtual void transactionAboutToAbort(int&, AcDbTransactionManager*) {}
virtual void transactionAborted(int&, AcDbTransactionManager*) {}
virtual void endCalledOnOutermostTransaction(int&, AcDbTransactionManager*) {}
virtual void objectIdSwapped(const AcDbObject*, const AcDbObject*, AcDbTransactionManager*) {}
```

Classification relative to HF-V31:

| Callback | Class | Contract treatment |
|---|---|---|
| `transactionAboutToStart` | MARKER | `N-TR-ABOUT-START` on controlled T creation |
| `transactionStarted` | MARKER | `N-TR-STARTED` |
| `transactionAboutToEnd` | IN | `N-TR-ABOUT-END`, probes 09N-A/B |
| `transactionEnded` | IN | `N-TR-ENDED`, probe 09N-C and markers |
| `transactionAboutToAbort` | IN | `N-TR-ABOUT-ABORT`, probe 04N |
| `transactionAborted` | MARKER | `N-TR-ABORTED` |
| `endCalledOnOutermostTransaction` | IN | new `N-TR-OUTERMOST-END-CALLED`, probe 09N-O |
| `objectIdSwapped` | OUT | no ObjectId swap in the bounded fixture |

The only nearby header comment says the class is a reactor for transaction management. The header names the first
parameter `numTransactions`, but provides no ordering guarantee between about-to-end, outermost-end-called and ended;
it does not state successful commit, durability, mutation legality, `C_decide`, `C_report` or `C_host`. Those facts
remain runtime-characterized.

## 5. 09N, T2/T4 and property mapping

`09N-B` stays on `N-TR-ABOUT-END` with `count==1`; this measures the final about-to-end specialization. `09N-O` is a
new independent observation of the distinct outermost callback. `09N-C` remains the ended phase and `09N-D` remains
the command-ended fresh-transaction probe. The known phase identities are distinct; their exact total order is an
experimental result.

- **T2:** every IN native callback has a primary probe or explicit marker role; managed emulation receives no credit.
- **T4:** about-to-end, outermost-end-called, ended and command-ended are separate phases; none is named commit.
- **T2+T4:** 09N-A/B/O/C/D form the concrete composition; no family shorthand is used.
- **P2:** 09N-B/O/C observe callback visibility and state around transaction end.
- **P3:** 09N-A distinguishes nested containment; 09N-B/O/C distinguish the controlled outermost path.
- **P6:** 09N-B/O/C contribute lifecycle evidence only; final semantic fence still requires the complete V24 model.

## 6. Scheduler and load authority

`sendStringToExecute(AcApDocument*,const ACHAR*,bool,bool,bool)` matches the contract. The actual header says
`beginExecuteInCommandContext(void(*)(void*),void*)` queues execution, requires application context, requires the
caller to return and cancels outstanding commands before invocation. `beginExecuteInApplicationContext` also exists
and is asynchronous, but remains OUT of the claimed scheduler scope.

`AcRxDLinkerReactor` exposes will-load, loaded, load-aborted, will-unload, unloaded and unload-aborted callbacks.
Only `rxAppWillBeLoaded` and `rxAppLoaded` enter the successful controlled-load path. `acedArxLoad(const ACHAR*)`
remains the load action; module load itself is not the mutation trigger.

## 7. Closure

All native definition items used by the bounded fixture now resolve to exact headers or explicit HF-bounded OUT
reasons. There is no definition UNKNOWN. Therefore:

```text
ActualHeaderReconciliationComplete = TRUE
NativeReactorClassSetClosed = TRUE
```

This is definition closure only. No research implementation exists and `CTDA_HOST_PASS = NOT EVALUATED`.
