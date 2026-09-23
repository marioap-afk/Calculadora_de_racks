# I-52 — Native Scheduler Catalog V33

> **NSC-V33-1 / NORMATIVE / NOT EXECUTED.** Authority: exact `acdocman.h` SHA-256
> `4B71C08BA578F81FC67898AC3519D0D5069C638A74F52D256DD539D02BDE422D`, ObjectARX 25.0.58.0.

## 1. AcApDocManager callable-member inventory

| # | Member | Exact normalized declaration | Disposition | Reason |
|---:|---|---|---|---|
| 1 | `curDocument` | `virtual AcApDocument* curDocument() const = 0;` | OBSERVATION_ONLY | Return/query member used only to observe context or document state. |
| 2 | `mdiActiveDocument` | `virtual AcApDocument* mdiActiveDocument() const = 0;` | OBSERVATION_ONLY | Return/query member used only to observe context or document state. |
| 3 | `isApplicationContext` | `virtual bool isApplicationContext() const = 0;` | OBSERVATION_ONLY | Return/query member used only to observe context or document state. |
| 4 | `document` | `virtual AcApDocument* document(const AcDbDatabase* ) const = 0;` | OBSERVATION_ONLY | Return/query member used only to observe context or document state. |
| 5 | `lockDocument` | `virtual Acad::ErrorStatus lockDocument(AcApDocument* pDoc, AcAp::DocLockMode = AcAp::kWrite, const ACHAR* pGlobalCmdName = NULL, const ACHAR* pLocalCmdName = NULL, bool prompt = true) = 0;` | OUT_BY_OPERATION | The bounded fixture does not perform the operation that triggers or invokes this member during the measured interval. |
| 6 | `unlockDocument` | `virtual Acad::ErrorStatus unlockDocument(AcApDocument* pDoc) = 0;` | OUT_BY_OPERATION | The bounded fixture does not perform the operation that triggers or invokes this member during the measured interval. |
| 7 | `newAcApDocumentIterator` | `ADESK_DEPRECATE_FOR_INTERNAL_USE virtual AcApDocumentIterator* newAcApDocumentIterator() = 0;` | OBSERVATION_ONLY | Return/query member used only to observe context or document state. |
| 8 | `getDocumentIterator` | `auto getDocumentIterator() { ADESK_SUPPRESS_DEPRECATED return std::unique_ptr<AcApDocumentIterator>(newAcApDocumentIterator()); }` | OBSERVATION_ONLY | Return/query member used only to observe context or document state. |
| 9 | `addReactor` | `virtual void addReactor(AcApDocManagerReactor* ) = 0;` | OUT_BY_OPERATION | The bounded fixture does not perform the operation that triggers or invokes this member during the measured interval. |
| 10 | `removeReactor` | `virtual void removeReactor(AcApDocManagerReactor* ) = 0;` | OUT_BY_OPERATION | The bounded fixture does not perform the operation that triggers or invokes this member during the measured interval. |
| 11 | `setDefaultFormatForSave` | `virtual Acad::ErrorStatus setDefaultFormatForSave( AcApDocument::SaveFormat format) = 0;` | OUT_BY_OPERATION | The bounded fixture does not perform the operation that triggers or invokes this member during the measured interval. |
| 12 | `defaultFormatForSave` | `virtual AcApDocument::SaveFormat defaultFormatForSave() const = 0;` | OBSERVATION_ONLY | Return/query member used only to observe context or document state. |
| 13 | `setCurDocument` | `virtual Acad::ErrorStatus setCurDocument(AcApDocument* pDoc, AcAp::DocLockMode = AcAp::kNone, bool activate = false) = 0;` | OUT_BY_OPERATION | The bounded fixture does not perform the operation that triggers or invokes this member during the measured interval. |
| 14 | `activateDocument` | `virtual Acad::ErrorStatus activateDocument(AcApDocument* pAcTargetDocument, bool bPassScript = false) = 0;` | OUT_BY_OPERATION | The bounded fixture does not perform the operation that triggers or invokes this member during the measured interval. |
| 15 | `sendStringToExecute` | `virtual Acad::ErrorStatus sendStringToExecute(AcApDocument* pAcTargetDocument, const ACHAR * pszExecute, bool bActivate = true, bool bWrapUpInactiveDoc = false, bool bEchoString = true) = 0;` | IN_RETAINED | Exact AcApDocManager scheduling member used by the bounded T16 contract. |
| 16 | `appContextNewDocument` | `virtual Acad::ErrorStatus appContextNewDocument(const ACHAR *pszTemplateName) = 0;` | OUT_BY_OPERATION | The bounded fixture does not perform the operation that triggers or invokes this member during the measured interval. |
| 17 | `appContextOpenDocument` | `virtual Acad::ErrorStatus appContextOpenDocument(const ACHAR *pszDrawingName) = 0;` | OUT_BY_OPERATION | The bounded fixture does not perform the operation that triggers or invokes this member during the measured interval. |
| 18 | `appContextRecoverDocument` | `virtual Acad::ErrorStatus appContextRecoverDocument(const ACHAR *pszDrawingName) = 0;` | OUT_BY_OPERATION | The bounded fixture does not perform the operation that triggers or invokes this member during the measured interval. |
| 19 | `appContextPromptNewDocument` | `ACCORE_PORT Acad::ErrorStatus appContextPromptNewDocument();` | OUT_BY_OPERATION | The bounded fixture does not perform the operation that triggers or invokes this member during the measured interval. |
| 20 | `appContextPromptOpenDocument` | `ACCORE_PORT Acad::ErrorStatus appContextPromptOpenDocument();` | OUT_BY_OPERATION | The bounded fixture does not perform the operation that triggers or invokes this member during the measured interval. |
| 21 | `appContextCloseDocument` | `ACCORE_PORT Acad::ErrorStatus appContextCloseDocument(AcApDocument* pDoc);` | OUT_BY_OPERATION | The bounded fixture does not perform the operation that triggers or invokes this member during the measured interval. |
| 22 | `appContextOpenDocument` | `virtual Acad::ErrorStatus appContextOpenDocument(const DocOpenParams *pParams) = 0;` | OUT_BY_OPERATION | The bounded fixture does not perform the operation that triggers or invokes this member during the measured interval. |
| 23 | `newDocument` | `virtual Acad::ErrorStatus newDocument() = 0;` | OUT_BY_OPERATION | The bounded fixture does not perform the operation that triggers or invokes this member during the measured interval. |
| 24 | `openDocument` | `virtual Acad::ErrorStatus openDocument() = 0;` | OUT_BY_OPERATION | The bounded fixture does not perform the operation that triggers or invokes this member during the measured interval. |
| 25 | `closeDocument` | `virtual Acad::ErrorStatus closeDocument(AcApDocument* pAcTargetDocument) = 0;` | OUT_BY_OPERATION | The bounded fixture does not perform the operation that triggers or invokes this member during the measured interval. |
| 26 | `inputPending` | `virtual int inputPending(AcApDocument* pAcTargetDocument) = 0;` | OBSERVATION_ONLY | Return/query member used only to observe context or document state. |
| 27 | `disableDocumentActivation` | `virtual Acad::ErrorStatus disableDocumentActivation() = 0;` | OUT_BY_OPERATION | The bounded fixture does not perform the operation that triggers or invokes this member during the measured interval. |
| 28 | `enableDocumentActivation` | `virtual Acad::ErrorStatus enableDocumentActivation() = 0;` | OUT_BY_OPERATION | The bounded fixture does not perform the operation that triggers or invokes this member during the measured interval. |
| 29 | `isDocumentActivationEnabled` | `virtual bool isDocumentActivationEnabled() = 0;` | OBSERVATION_ONLY | Return/query member used only to observe context or document state. |
| 30 | `executeInApplicationContext` | `virtual void executeInApplicationContext(void (*procAddr)(void *), void *pData ) const = 0;` | IN_NEWLY_RECOGNIZED | Synchronous application-context bridge; not a deferred scheduler. |
| 31 | `beginExecuteInCommandContext` | `ACCORE_PORT Acad::ErrorStatus beginExecuteInCommandContext(void (*procAddr)(void *), void *pData);` | IN_NEWLY_RECOGNIZED | Exact AcApDocManager scheduling member used by the bounded T16 contract. |
| 32 | `beginExecuteInApplicationContext` | `ACCORE_PORT Acad::ErrorStatus beginExecuteInApplicationContext(void(*procAddr)(void *), void *pData);` | IN_NEWLY_RECOGNIZED | Exact AcApDocManager scheduling member used by the bounded T16 contract. |
| 33 | `documentCount` | `virtual int documentCount() const = 0;` | OBSERVATION_ONLY | Return/query member used only to observe context or document state. |
| 34 | `pushAcadResourceHandle` | `virtual void pushAcadResourceHandle() = 0;` | OUT_BY_OPERATION | The bounded fixture does not perform the operation that triggers or invokes this member during the measured interval. |
| 35 | `popResourceHandle` | `virtual void popResourceHandle() = 0;` | OUT_BY_OPERATION | The bounded fixture does not perform the operation that triggers or invokes this member during the measured interval. |
| 36 | `sendModelessInterrupt` | `virtual Acad::ErrorStatus sendModelessInterrupt(AcApDocument* pAcTargetDocument) = 0;` | OUT_BY_OPERATION | The bounded fixture does not perform the operation that triggers or invokes this member during the measured interval. |
| 37 | `setHost` | `ACCORE_PORT void setHost(IAcApDocManagerHost* pHost);` | OUT_BY_OPERATION | The bounded fixture does not perform the operation that triggers or invokes this member during the measured interval. |

The inventory contains every callable member in the exact class declaration. Nested `DocOpenParams` data fields are
data-schema members, not callable scheduling surfaces; they remain represented by the overload declaration that consumes them.

## 2. Scheduler and context semantics

| AuthorityId | Exact API | Calling context | Return/execution timing | Target/context at delivery | Lock/T assumptions | Cancellation/drain | HF | Threats | Probes |
|---|---|---|---|---|---|---|---|---|---|
| `NS-SEND` | `sendStringToExecute(AcApDocument*,const ACHAR*,bool,bool,bool)` | document or application caller with exact target document | enqueue returns; command executes later | chosen target document command context | none inferred; observed at delivery | input/command drain must be proved | IN | T16 | retained 16N plus complete 26-origin composition coverage |
| `NS-BEGIN-CMDCTX` | `beginExecuteInCommandContext(void(*)(void*),void*)` | exact header requires application context | always deferred until caller returns; outstanding commands are cancelled first | MDI active document command context | no lock/T inherited; delivery observes both | no cancellation API; dedicated-process drain | IN | T13/T16 | `16C-S/M/SM`, `C2APP2CMD-ALL` |
| `NS-BEGIN-APPCTX` | `beginExecuteInApplicationContext(void(*)(void*),void*)` | no context restriction declared; exact origin logged | always asynchronous; callback waits until caller returns to main message loop | application context | DB write requires separately obtained legal document/lock/T | no cancellation API; dedicated-process drain | IN | T16 | `16A-S/M/SM` plus complete 26-origin composition coverage |
| `NX-APPCTX-SYNC` | `executeInApplicationContext(void(*)(void*),void*) const` | controlled document caller | synchronous context transition; not deferred | application context before caller resumes | lock/T never inferred | completion is ordinary synchronous return | IN, non-scheduler | T13 | `13A-SM` |

## 3. Equivalence decision

`beginExecuteInCommandContext` is **not** treated as equivalent to `sendStringToExecute`. The header compares the
command-registration effect, but the APIs differ in application-context precondition, target-document selection, outstanding-command
cancellation and callback shape. `beginExecuteInApplicationContext` is also distinct because it delivers application-context
code after return to the message loop. Each therefore receives its own SchedulerId and probes.

## 4. Legal origins and compositions

- `NS-BEGIN-APPCTX`: every V33 native origin calls it directly; enqueue status, origin return and later delivery are logged.
- `NS-BEGIN-CMDCTX`: `NX-APPCTX-SYNC` establishes and observes the exact required application context before enqueue.
- `C2APP2CMD-ALL`: proves the material two-stage callback → application context → command context composition.
- The command-context second leg consumes only the immutable callback pointer/data and observed application context. Once
  that interface is reached, origin-specific behavior is fully captured by the first-leg probes; no unproved origin independence is used.
- Rejection, nondelivery, illegal DB context or incomplete drain produces UNKNOWN during execution.

## 5. Closure predicates

```text
T16NativeSchedulerSetClosed(HF-V33) iff
  all 37 AcApDocManager callable members are individually classified
  AND NS-SEND, NS-BEGIN-CMDCTX and NS-BEGIN-APPCTX have concrete probes
  AND NX-APPCTX-SYNC is characterized without scheduler credit
  AND all 26 reachable native callbacks have SEND and APPCTX composition coverage
  AND the material APPCTX→CMDCTX chain has a concrete probe
  AND all other execution/context members are OBSERVATION_ONLY or OUT_BY_OPERATION
  AND no scheduler definition UNKNOWN remains.

T16NativeSchedulerSetClosed(HF-V33) = TRUE
```

This is definition closure. No scheduler probe has run and no rejection/delivery behavior is asserted as PASS.
