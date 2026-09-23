# I-52 — Native Event Catalog V32

> **NEC-V32-1 / NORMATIVE / NOT EXECUTED.** Exact-header projection of NPM-V32-1. Parent NEC-V30-1
> blob `f9536fdfc6b97dcb7051f958b6c4dcea5f6d3c32`. Event relations below are generated from the 30-row matrix.

## 1. Exact-header event inventory

| EventId | Exact class | Exact callback | Header | HF class |
|---|---|---|---|---|
| `N-DB-APPEND` | `AcDbDatabaseReactor` | `objectAppended` | `dbmain.h` | MARKER |
| `N-DB-ERASE` | `AcDbDatabaseReactor` | `objectErased` | `dbmain.h` | IN |
| `N-DB-MOD` | `AcDbDatabaseReactor` | `objectModified` | `dbmain.h` | IN |
| `N-DB-OPEN` | `AcDbDatabaseReactor` | `objectOpenedForModify` | `dbmain.h` | IN |
| `N-DOC-LOCK-CHANGED` | `AcApDocManagerReactor` | `documentLockModeChanged` | `acdocman.h` | IN |
| `N-DOC-LOCK-VETO` | `AcApDocManagerReactor` | `documentLockModeChangeVetoed` | `acdocman.h` | IN |
| `N-DOC-LOCK-WILL` | `AcApDocManagerReactor` | `documentLockModeWillChange` | `acdocman.h` | IN |
| `N-ED-CANCEL` | `AcEditorReactor` | `commandCancelled` | `aced.h` | IN |
| `N-ED-END` | `AcEditorReactor` | `commandEnded` | `aced.h` | IN |
| `N-ED-FAIL` | `AcEditorReactor` | `commandFailed` | `aced.h` | IN |
| `N-ED-WILL` | `AcEditorReactor` | `commandWillStart` | `aced.h` | IN |
| `N-ENT-GFX` | `AcDbEntityReactor` | `modifiedGraphics` | `dbmain.h` | IN |
| `N-OBJ-CANCEL` | `AcDbObjectReactor` | `cancelled` | `dbmain.h` | IN |
| `N-OBJ-CLOSED` | `AcDbObjectReactor` | `objectClosed` | `dbmain.h` | IN |
| `N-OBJ-ERASE` | `AcDbObjectReactor` | `erased` | `dbmain.h` | IN |
| `N-OBJ-MOD` | `AcDbObjectReactor` | `modified` | `dbmain.h` | IN |
| `N-OBJ-OPEN` | `AcDbObjectReactor` | `openedForModify` | `dbmain.h` | IN |
| `N-OBJ-UNDO` | `AcDbObjectReactor` | `modifyUndone` | `dbmain.h` | IN |
| `N-RX-LOADED` | `AcRxDLinkerReactor` | `rxAppLoaded` | `rxdlinkr.h` | MARKER |
| `N-RX-WILL-LOAD` | `AcRxDLinkerReactor` | `rxAppWillBeLoaded` | `rxdlinkr.h` | MARKER |
| `N-TR-ABORTED` | `AcTransactionReactor` | `transactionAborted` | `dbtrans.h` | MARKER |
| `N-TR-ABOUT-ABORT` | `AcTransactionReactor` | `transactionAboutToAbort` | `dbtrans.h` | IN |
| `N-TR-ABOUT-END` | `AcTransactionReactor` | `transactionAboutToEnd` | `dbtrans.h` | IN |
| `N-TR-ABOUT-START` | `AcTransactionReactor` | `transactionAboutToStart` | `dbtrans.h` | MARKER |
| `N-TR-ENDED` | `AcTransactionReactor` | `transactionEnded` | `dbtrans.h` | IN |
| `N-TR-OUTERMOST-END-CALLED` | `AcTransactionReactor` | `endCalledOnOutermostTransaction` | `dbtrans.h` | IN |
| `N-TR-STARTED` | `AcTransactionReactor` | `transactionStarted` | `dbtrans.h` | MARKER |

Distinct virtual callbacks have distinct EventIds. `N-TR-OUTERMOST-END-CALLED` is not an alias for
`N-TR-ABOUT-END`; neither identifier implies commit, durability or a success boundary. Marker-only events do not
become primary authority. Every declaration is bound to AH-V32-1 exact header bytes.

## 2. Event-to-probe projection

| EventId | Relation | ProbeId |
|---|---|---|
| `N-DB-APPEND` | `MARKER_FOR` | `10N-SM` |
| `N-DB-APPEND` | `MARKER_FOR` | `16N-SM` |
| `N-DB-APPEND` | `MARKER_FOR` | `C15N16N-SM` |
| `N-DB-ERASE` | `MARKER_FOR` | `02NO-SM` |
| `N-DB-MOD` | `MARKER_FOR` | `02N` |
| `N-DB-MOD` | `SCHEDULE_ORIGIN_FOR` | `16N-M` |
| `N-DB-MOD` | `SCHEDULE_ORIGIN_FOR` | `16N-S` |
| `N-DB-MOD` | `SCHEDULE_ORIGIN_FOR` | `16N-SM` |
| `N-DB-MOD` | `SCHEDULE_ORIGIN_FOR` | `C15N16N-SM` |
| `N-DB-OPEN` | `PRIMARY_FOR` | `02N` |
| `N-DOC-LOCK-CHANGED` | `MARKER_FOR` | `10NDOC-VETO-SM` |
| `N-DOC-LOCK-CHANGED` | `MARKER_FOR` | `10NDOC-WILL-SM` |
| `N-DOC-LOCK-CHANGED` | `MARKER_FOR` | `C2DOCW16N-SM` |
| `N-DOC-LOCK-CHANGED` | `PRIMARY_FOR` | `10NDOC-CHANGED-SM` |
| `N-DOC-LOCK-CHANGED` | `SCHEDULE_ORIGIN_FOR` | `C2DOCC16N-SM` |
| `N-DOC-LOCK-VETO` | `MARKER_FOR` | `10NDOC-CHANGED-SM` |
| `N-DOC-LOCK-VETO` | `MARKER_FOR` | `10NDOC-WILL-SM` |
| `N-DOC-LOCK-VETO` | `PRIMARY_FOR` | `10NDOC-VETO-SM` |
| `N-DOC-LOCK-WILL` | `MARKER_FOR` | `10NDOC-CHANGED-SM` |
| `N-DOC-LOCK-WILL` | `MARKER_FOR` | `10NDOC-VETO-SM` |
| `N-DOC-LOCK-WILL` | `MARKER_FOR` | `C2DOCC16N-SM` |
| `N-DOC-LOCK-WILL` | `PRIMARY_FOR` | `10NDOC-WILL-SM` |
| `N-DOC-LOCK-WILL` | `SCHEDULE_ORIGIN_FOR` | `C2DOCW16N-SM` |
| `N-ED-CANCEL` | `MARKER_FOR` | `04N` |
| `N-ED-CANCEL` | `MARKER_FOR` | `10NDOC-VETO-SM` |
| `N-ED-END` | `MARKER_FOR` | `09N-C` |
| `N-ED-END` | `MARKER_FOR` | `16N-M` |
| `N-ED-END` | `MARKER_FOR` | `16N-S` |
| `N-ED-END` | `MARKER_FOR` | `16N-SM` |
| `N-ED-END` | `MARKER_FOR` | `C15N16N-SM` |
| `N-ED-END` | `MARKER_FOR` | `C2DOCC16N-SM` |
| `N-ED-END` | `MARKER_FOR` | `C2DOCW16N-SM` |
| `N-ED-END` | `MARKER_FOR` | `C2ENT16N-M` |
| `N-ED-END` | `MARKER_FOR` | `C2OBJ16N-M` |
| `N-ED-END` | `MARKER_FOR` | `C2OBJ16N-S` |
| `N-ED-END` | `MARKER_FOR` | `C2OBJ16N-SM` |
| `N-ED-END` | `PRIMARY_FOR` | `09N-D` |
| `N-ED-END` | `PRIMARY_FOR` | `10N-M` |
| `N-ED-END` | `PRIMARY_FOR` | `10N-S` |
| `N-ED-END` | `PRIMARY_FOR` | `10N-SM` |
| `N-ED-FAIL` | `MARKER_FOR` | `04N` |
| `N-ED-WILL` | `MARKER_FOR` | `16N-M` |
| `N-ED-WILL` | `MARKER_FOR` | `16N-S` |
| `N-ED-WILL` | `MARKER_FOR` | `16N-SM` |
| `N-ED-WILL` | `MARKER_FOR` | `C15N16N-SM` |
| `N-ED-WILL` | `MARKER_FOR` | `C2DOCC16N-SM` |
| `N-ED-WILL` | `MARKER_FOR` | `C2DOCW16N-SM` |
| `N-ED-WILL` | `MARKER_FOR` | `C2ENT16N-M` |
| `N-ED-WILL` | `MARKER_FOR` | `C2OBJ16N-M` |
| `N-ED-WILL` | `MARKER_FOR` | `C2OBJ16N-S` |
| `N-ED-WILL` | `MARKER_FOR` | `C2OBJ16N-SM` |
| `N-ENT-GFX` | `MARKER_FOR` | `02NO-M` |
| `N-ENT-GFX` | `PRIMARY_FOR` | `02NE-M` |
| `N-ENT-GFX` | `SCHEDULE_ORIGIN_FOR` | `C2ENT16N-M` |
| `N-OBJ-CANCEL` | `MARKER_FOR` | `04NO-UNDO-S` |
| `N-OBJ-CANCEL` | `PRIMARY_FOR` | `04NO-S` |
| `N-OBJ-CLOSED` | `MARKER_FOR` | `02NE-M` |
| `N-OBJ-CLOSED` | `MARKER_FOR` | `02NO-M` |
| `N-OBJ-CLOSED` | `MARKER_FOR` | `02NO-S` |
| `N-OBJ-CLOSED` | `MARKER_FOR` | `02NO-SM` |
| `N-OBJ-CLOSED` | `MARKER_FOR` | `04NO-S` |
| `N-OBJ-CLOSED` | `MARKER_FOR` | `04NO-UNDO-S` |
| `N-OBJ-CLOSED` | `MARKER_FOR` | `C2ENT16N-M` |
| `N-OBJ-CLOSED` | `MARKER_FOR` | `C2OBJ16N-M` |
| `N-OBJ-CLOSED` | `MARKER_FOR` | `C2OBJ16N-S` |
| `N-OBJ-CLOSED` | `MARKER_FOR` | `C2OBJ16N-SM` |
| `N-OBJ-CLOSED` | `PRIMARY_FOR` | `02NO-CLOSE-SM` |
| `N-OBJ-ERASE` | `PRIMARY_FOR` | `02NO-SM` |
| `N-OBJ-MOD` | `MARKER_FOR` | `02NE-M` |
| `N-OBJ-MOD` | `MARKER_FOR` | `02NO-CLOSE-SM` |
| `N-OBJ-MOD` | `MARKER_FOR` | `02NO-S` |
| `N-OBJ-MOD` | `MARKER_FOR` | `04NO-S` |
| `N-OBJ-MOD` | `PRIMARY_FOR` | `02NO-M` |
| `N-OBJ-MOD` | `SCHEDULE_ORIGIN_FOR` | `C2OBJ16N-M` |
| `N-OBJ-MOD` | `SCHEDULE_ORIGIN_FOR` | `C2OBJ16N-S` |
| `N-OBJ-MOD` | `SCHEDULE_ORIGIN_FOR` | `C2OBJ16N-SM` |
| `N-OBJ-OPEN` | `MARKER_FOR` | `02NO-M` |
| `N-OBJ-OPEN` | `PRIMARY_FOR` | `02NO-S` |
| `N-OBJ-UNDO` | `MARKER_FOR` | `04NO-S` |
| `N-OBJ-UNDO` | `PRIMARY_FOR` | `04NO-UNDO-S` |
| `N-RX-LOADED` | `MARKER_FOR` | `C15N16N-SM` |
| `N-RX-WILL-LOAD` | `MARKER_FOR` | `C15N16N-SM` |
| `N-TR-ABORTED` | `MARKER_FOR` | `04N` |
| `N-TR-ABOUT-ABORT` | `PRIMARY_FOR` | `04N` |
| `N-TR-ABOUT-END` | `MARKER_FOR` | `09N-O` |
| `N-TR-ABOUT-END` | `PRIMARY_FOR` | `09N-A` |
| `N-TR-ABOUT-END` | `PRIMARY_FOR` | `09N-B` |
| `N-TR-ABOUT-START` | `MARKER_FOR` | `09N-A` |
| `N-TR-ABOUT-START` | `MARKER_FOR` | `09N-B` |
| `N-TR-ABOUT-START` | `MARKER_FOR` | `09N-C` |
| `N-TR-ABOUT-START` | `MARKER_FOR` | `09N-D` |
| `N-TR-ABOUT-START` | `MARKER_FOR` | `10N-M` |
| `N-TR-ABOUT-START` | `MARKER_FOR` | `10N-S` |
| `N-TR-ABOUT-START` | `MARKER_FOR` | `10N-SM` |
| `N-TR-ENDED` | `MARKER_FOR` | `09N-A` |
| `N-TR-ENDED` | `MARKER_FOR` | `09N-B` |
| `N-TR-ENDED` | `MARKER_FOR` | `09N-D` |
| `N-TR-ENDED` | `MARKER_FOR` | `09N-O` |
| `N-TR-ENDED` | `MARKER_FOR` | `10N-M` |
| `N-TR-ENDED` | `MARKER_FOR` | `10N-S` |
| `N-TR-ENDED` | `MARKER_FOR` | `10N-SM` |
| `N-TR-ENDED` | `PRIMARY_FOR` | `09N-C` |
| `N-TR-OUTERMOST-END-CALLED` | `MARKER_FOR` | `09N-B` |
| `N-TR-OUTERMOST-END-CALLED` | `MARKER_FOR` | `09N-C` |
| `N-TR-OUTERMOST-END-CALLED` | `MARKER_FOR` | `09N-D` |
| `N-TR-OUTERMOST-END-CALLED` | `MARKER_FOR` | `10N-M` |
| `N-TR-OUTERMOST-END-CALLED` | `MARKER_FOR` | `10N-S` |
| `N-TR-OUTERMOST-END-CALLED` | `MARKER_FOR` | `10N-SM` |
| `N-TR-OUTERMOST-END-CALLED` | `PRIMARY_FOR` | `09N-O` |
| `N-TR-STARTED` | `MARKER_FOR` | `09N-A` |
| `N-TR-STARTED` | `MARKER_FOR` | `09N-B` |
| `N-TR-STARTED` | `MARKER_FOR` | `09N-C` |
| `N-TR-STARTED` | `MARKER_FOR` | `09N-D` |
| `N-TR-STARTED` | `MARKER_FOR` | `10N-M` |
| `N-TR-STARTED` | `MARKER_FOR` | `10N-S` |
| `N-TR-STARTED` | `MARKER_FOR` | `10N-SM` |

## 3. Scheduler-to-probe projection

| SchedulerId | ProbeId |
|---|---|
| `NS-SEND` | `16N-M` |
| `NS-SEND` | `16N-S` |
| `NS-SEND` | `16N-SM` |
| `NS-SEND` | `C15N16N-SM` |
| `NS-SEND` | `C2DOCC16N-SM` |
| `NS-SEND` | `C2DOCW16N-SM` |
| `NS-SEND` | `C2ENT16N-M` |
| `NS-SEND` | `C2OBJ16N-M` |
| `NS-SEND` | `C2OBJ16N-S` |
| `NS-SEND` | `C2OBJ16N-SM` |

## 4. Mechanical closure

```text
Native EventIds = 27
Event relation triples expected/actual = 116/116
Event missing/extra = 0/0
Scheduler pairs expected/actual = 10/10
Scheduler missing/extra = 0/0
Non-ProbeId labels in ProbeId cells = 0
NativeReactorClassSetClosed = TRUE
NativeProbeCatalogClosed(NEC-V32-1,NPM-V32-1) = TRUE
ManagedProbeCatalogClosed(HEC-V27-C1) = TRUE
EventCatalogClosed = TRUE
HostFixtureContractClosed = TRUE
CTDA_HOST_PASS = NOT EVALUATED
```

All callbacks classified OUT by AH-V32-1 are excluded by an HF-bounded operation rule and receive no evidence credit.
