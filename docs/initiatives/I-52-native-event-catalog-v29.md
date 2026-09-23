# I-52 — Native Event Catalog V29

> **NEC-V29-1 / DEFINITION CONTRACT.** Parent `NEC-V28-1` blob
> `e2d1284c5266f5b7f2dd57129ab7552bbd881c74`. No callback or probe has been executed.

## 1. Authority snapshot

Only `OFFICIAL_EXACT_VERSION_DOC` and future `EXACT_HEADER` close definitions. All URLs below use Autodesk
`cloudhelp/2025`; HTML fingerprints identify observed responses and are not header hashes.

| Authority | Header | SHA-256 HTML observed |
|---|---|---|
| `AcDbObjectReactor` class | `dbmain.h` | `3597376f1ff19b634c1ab94e728c8ffba0ba310c50d7005540d3956a25231465` |
| `AcDbObjectReactor` methods | `dbmain.h` | `5ea36707a8af56903aba60041e9b9b13129f06e1e515688c638cdb0e4630d559` |
| `openedForModify` | `dbmain.h` | `f45e3baecf99ee817ca0d25e0856fb997748bb198b3713737d9c76aefa756bd0` |
| `modified` | `dbmain.h` | `3d9a2c51f7a78979e35b2e5347104e2bb53f0d813884e905e726fdb6b370298b` |
| `erased` | `dbmain.h` | `39f2906b386d2439f69181cd97507d2344c202dfbd35660a241fc801582a131e` |
| `objectClosed` | `dbmain.h` | `885c9130424e24e07c58f6e56b896fea9e2fbd85e74870f0d3714e6c6dd7a35e` |
| `cancelled` | `dbmain.h` | `5af6382aafdd5d62720b7fe188fd97662b09b652c881ad90740afa7e7a734cab` |
| `modifyUndone` | `dbmain.h` | `7511c32da337559d6fc30d455ae864e9c3befcdee4a08468c14622d8fb5f4f6f` |
| `goodbye` | `dbmain.h` | `84dc2d7be561ebb9ea95884b243464ba6cec604791759fd747689401913dd225` |
| `addReactor` | `dbObject.h`/declared API | `5e25c9348896687a59b0999341347d17ad3649b3a67aa933addf52b16c44cc53` |
| `removeReactor` | `dbObject.h`/declared API | `4b1ec014d477f5c33a64e8a740944c0e047643ad468ceeb4097e00982d82b243` |
| `AcDbEntityReactor` | `dbmain.h` | `598217bc392edb9190dece2d4e1f7f384c9f98a3b78439ca5283ee12123ceb83` |
| `modifiedGraphics` | `dbmain.h` | `acab6e49f298965e3138b0fd0101d47ab99a2926f991ee2e328b79cfb493b366` |
| ObjectARX 2025 reactor-family census | Developer Guide | `2917c6ddede2a56a7de0ae2818ae2cfd107355a9dae3c38c9d049130dcfe3cdc` |
| `AcApDocManagerReactor` class/methods | `acdocman.h` | `f3820bfb57019b268542d87f8e17019a841276770324c63bca817d65c72c1d1b` / `2dddfa23e120a08d707bcd45ec41b6168aa3036d70a0f7e6b331885b903ed238` |
| document lock will/changed/vetoed | `acdocman.h` | `bc6d597fa42499d56f530d3add59010ec1f8aab149986ec2571d5a031e5150f4` / `4154f9981f76c100876ef3a279b0974c4efbc85f9ebd62f0630fcfc1085ff8c1` / `6af1c699da74262b78835cca4954f6529d4aa1378277a5719dfca315444528bb` |

Normative URLs:

- <https://help.autodesk.com/cloudhelp/2025/ENU/OARX-RefGuide/files/OARX-RefGuide-AcDbObjectReactor.html>
- <https://help.autodesk.com/cloudhelp/2025/ENU/OARX-RefGuide/files/OARX-RefGuide-__MEMBERTYPE_Methods_AcDbObjectReactor.html>
- <https://help.autodesk.com/cloudhelp/2025/ENU/OARX-RefGuide/files/OARX-RefGuide-AcDbEntityReactor.html>
- <https://help.autodesk.com/cloudhelp/2025/ENU/OARX-RefGuide/files/OARX-RefGuide-AcDbEntityReactor__modifiedGraphics_AcDbEntity_.html>
- <https://help.autodesk.com/cloudhelp/2025/ENU/OARX-RefGuide/files/OARX-RefGuide-AcApDocManagerReactor.html>
- <https://help.autodesk.com/cloudhelp/2025/ENU/OARX-RefGuide/files/OARX-RefGuide-__MEMBERTYPE_Methods_AcApDocManagerReactor.html>
- <https://help.autodesk.com/cloudhelp/2025/ENU/OARX-DevGuide/files/GUID-8C93E824-4974-4FF7-9278-5BFE5318340F.htm>

The ObjectARX SDK remains absent. Every header SHA is `NOT OBSERVED`; acquisition and byte verification remain a
future authorized prerequisite.

## 2. Object/entity callback catalog

| EventId | Exact class/callback | Phase and notifier rule | HF role | Probe status |
|---|---|---|---|---|
| N-OBJ-OPEN | `AcDbObjectReactor::openedForModify(const AcDbObject*)` | `assertWriteEnabled`, before change; const notifier | T2-OBJ S | primary 02NO-S |
| N-OBJ-MOD | `AcDbObjectReactor::modified(const AcDbObject*)` | close/cancel after modification; const notifier | T2-OBJ M/T16 origin | primary 02NO-M |
| N-OBJ-ERASE | `AcDbObjectReactor::erased(const AcDbObject*,bool)` | close after erase/unerase | T2-OBJ SM | primary 02NO-SM |
| N-OBJ-CANCEL | `AcDbObjectReactor::cancelled(const AcDbObject*)` | cancel instead of close | T2-OBJ abort | primary 04NO-S |
| N-OBJ-UNDO | `AcDbObjectReactor::modifyUndone(const AcDbObject*)` | close after undo or cancel of modified object | T2-OBJ rollback | primary 04NO-UNDO-S |
| N-OBJ-CLOSED | `AcDbObjectReactor::objectClosed(const AcDbObjectId)` | close/cancel; same notifier cannot reopen write (`eWasNotifying`) | T2-OBJ SM | primary 02NO-CLOSE-SM |
| N-OBJ-GOODBYE | `AcDbObjectReactor::goodbye(const AcDbObject*)` | immediately before object destruction | cleanup marker only | marker |
| N-ENT-GFX | `AcDbEntityReactor::modifiedGraphics(const AcDbEntity*)` | close after write/graphics-recorded entity | T2-ENTITY M | primary 02NE-M |

No callback writes its const notifier. Each mutation opens a distinct fixture target by ObjectId through an explicit,
logged transaction; illegality is an experimental UNKNOWN/FAIL, not assumed success.

## 3. Registration contract

```cpp
Acad::ErrorStatus AcDbObject::addReactor(AcDbObjectReactor* pReactor) const;
Acad::ErrorStatus AcDbObject::removeReactor(AcDbObjectReactor* pReactor) const;
```

Registration opens the exact notifier read, retains the reactor instance in the helper and stores `(reactorId,
notifierObjectId, databaseId)`. Removal uses the same pointer before fixture deletion. `goodbye` atomically marks that
association dead; cleanup then deletes the helper-owned reactor without reaccessing the destroyed notifier. Transient
registration is the only HF mechanism. Persistent `AcDbObject::addPersistentReactor` is not a distinct callback phase;
its persistence/copy/undo semantics are outside HF and remain a product compatibility residual.

## 4. Document-lock callbacks

`AcApDocManagerReactor`, header `acdocman.h`, is IN because F-LOCK changes during the actual command lifecycle:

| EventId | Exact callback | Phase | Probe |
|---|---|---|---|
| N-DOC-LOCK-WILL | `documentLockModeWillChange(AcApDocument*,AcAp::DocLockMode,AcAp::DocLockMode,AcAp::DocLockMode,const ACHAR*)` | before every lock change; cannot veto | 10NDOC-WILL-SM |
| N-DOC-LOCK-CHANGED | `documentLockModeChanged(AcApDocument*,AcAp::DocLockMode,AcAp::DocLockMode,AcAp::DocLockMode,const ACHAR*)` | lock established/removed; acquisition may be vetoed | 10NDOC-CHANGED-SM |
| N-DOC-LOCK-VETO | `documentLockModeChangeVetoed(AcApDocument*,const ACHAR*)` | after lock request veto | 10NDOC-VETO-SM |

Registration/removal use the exact document-manager singleton and one retained reactor. Callbacks filter scratch
document and research command name. They receive no assumed T/write permission; attempts acquire/log legal context or
produce UNKNOWN/FAIL.

## 5. Retained V28 events and schedulers

NEC-V28-1 exact IDs remain incorporated: N-DB-OPEN/MOD/ERASE, N-TR-ABOUT-ABORT/ABORTED/ABOUT-END/ENDED,
N-ED-WILL/END/CANCEL/FAIL, N-RX-LOADED, NS-SEND, NS-BEGIN-CMDCTX and NL-ARX-PATH. Their signatures, parameters and
authority fingerprints are unchanged. NPM-V29-1 replaces only their incomplete probe definitions/mappings.

## 6. HF-bounded reactor census

| Official family | Status | HF-bounded reason |
|---|---|---|
| `AcDbDatabaseReactor` | IN T2-DB | scratch DB object modification/erase |
| `AcDbObjectReactor` | IN T2-OBJ | attached to F-XR/F-REF/F-NEST |
| `AcDbEntityReactor` | IN T2-ENTITY | F-REF/F-NEST are entities; graphics notification |
| `AcTransactionReactor` | IN T2-TX | primary/nested/abort/end transaction paths |
| `AcEditorReactor` | IN T2-ED | fixture command start/end/cancel/fail |
| `AcEditorReactor2` | OUT as separate class | adds close/layout/view events absent from scratch command; inherited command callbacks are N-ED |
| `AcApDocManagerReactor` | IN T2-DOC | fixture lock acquisition/release |
| `AcRxDLinkerReactor` | IN T15 marker only | controlled payload load; no direct DB mutator credit |
| persistent `AcDbObject` reactor | OUT as separate event class | same object callback contract; HF registers transient only; persistence is residual |
| `AcApLongTransactionReactor` | OUT | HF executes no long transaction/REFEDIT |
| `AcDbLayoutManagerReactor` | OUT | HF never switches or mutates layout |
| `AcEdInputContextReactor` | OUT | critical fixture command has no prompt/input-context operation |
| `AcEdSSGetFilter` / `2` | OUT | no selection-set acquisition in fixture command |
| `AcRxEventReactor` | OUT | no deep-clone/wblock/database construction in measured interval |
| `AcDbSummaryInfoReactor` | OUT | no summary-info operation |
| `AcDbRasterImageDef*Reactor` | OUT | no raster-image primitive |
| publish/DMM/catalog/protocol families | OUT | no publish, DWF, catalog or protocol-extension operation |

`NativeReactorClassSetClosed(HF)` is TRUE only for this versioned primitive/operation manifest. Adding prompts,
layouts, long transactions, clone, custom subtype events or persistent reactor registration invalidates it.

## 7. Concrete Event→Probe inverse matrix

| NativeEventId | PRIMARY FOR | SCHEDULE ORIGIN FOR | MARKER FOR | INCIDENTAL FOR |
|---|---|---|---|---|
| N-DB-OPEN | 02N | NONE | NONE | NONE |
| N-DB-MOD | NONE | 16N-S;16N-M;16N-SM;C15N16N-SM | NONE | 02N |
| N-TR-ABOUT-ABORT | 04N | NONE | NONE | NONE |
| N-TR-ABORTED | NONE | NONE | 04N | NONE |
| N-TR-ABOUT-END | 09N-A;09N-B | NONE | NONE | NONE |
| N-TR-ENDED | 09N-C | NONE | 09N-D | NONE |
| N-ED-END | 09N-D;10N-S;10N-M;10N-SM | NONE | all 16N/C2 deferred probes | NONE |
| N-OBJ-OPEN | 02NO-S | NONE | NONE | 02NO-M |
| N-OBJ-MOD | 02NO-M | C2OBJ16N-S;C2OBJ16N-M;C2OBJ16N-SM | 02NO-CLOSE-SM | NONE |
| N-OBJ-ERASE | 02NO-SM | NONE | 02NO-CLOSE-SM | NONE |
| N-OBJ-CANCEL | 04NO-S | NONE | 04NO-UNDO-S | NONE |
| N-OBJ-UNDO | 04NO-UNDO-S | NONE | 04NO-S | NONE |
| N-OBJ-CLOSED | 02NO-CLOSE-SM | NONE | 02NO-M;02NO-SM | NONE |
| N-OBJ-GOODBYE | NONE | NONE | object-reactor cleanup | NONE |
| N-ENT-GFX | 02NE-M | C2ENT16N-M | NONE | 02NO-M |
| N-DOC-LOCK-WILL | 10NDOC-WILL-SM | C2DOCW16N-SM | all lifecycle probes | NONE |
| N-DOC-LOCK-CHANGED | 10NDOC-CHANGED-SM | C2DOCC16N-SM | all lifecycle probes | NONE |
| N-DOC-LOCK-VETO | 10NDOC-VETO-SM | NONE | failure lifecycle | NONE |
| N-RX-LOADED | NONE | NONE | C15N16N-SM | NONE |

IDs not shown as primary retain their exact marker mappings from NEC-V28-1/NPM-V29-1. Every list contains concrete
ProbeIds; family shorthand has no normative force.

## 8. Scheduler→Probe inverse matrix

| SchedulerId | PRIMARY FOR |
|---|---|
| NS-SEND | 16N-S;16N-M;16N-SM;C15N16N-SM;C2OBJ16N-S;C2OBJ16N-M;C2OBJ16N-SM;C2ENT16N-M;C2DOCW16N-SM;C2DOCC16N-SM |
| NS-BEGIN-CMDCTX | NONE in NPM-V29-1; retained supplemental characterization only |

## 9. Threat composition

- T2+T4: 04N/09N plus 04NO callbacks. Object callbacks have no transaction-end event; their callback result and T
  outcome compose without another scheduler/event semantic.
- T2+T16: DB, object, entity and document-lock origins each have a concrete NS-SEND composition in NPM-V29-1.
- T15+T16: C15N16N-SM loads a controlled payload that registers specifically N-DB-MOD; object-reactor load behavior
  is already characterized by C2OBJ and does not add a different load/scheduler ordering.
- T2+T15: N-RX-LOADED is a marker; mutation authority remains the exact callback registered by the payload.

## 10. Closure

```text
T2NativeClassCoverageClosed = TRUE
NativeReactorClassSetClosed(HF-V29-1) = TRUE
NativeProbeCatalogClosed(NEC-V29-1,NPM-V29-1) = TRUE
ManagedProbeCatalogClosed(HEC-V27-C1) = TRUE
EventCatalogClosed = TRUE
HostFixtureContractClosed(HF-V29-1) = TRUE
CTDA_HOST_PASS = NOT EVALUATED
```

No row asserts runtime legality or PASS. A callback not firing, illegal mutation, incomplete order, unavailable field,
cleanup failure or tuple drift yields probe UNKNOWN/FAIL and prevents CTDA_HOST_PASS.
