# I-52 — ObjectARX Member Capability V34

> Additive patch to V33 blob `0f7548005df35d6ab63b03faa055a129a3c6dfe7`; unaffected 216-member rows remain incorporated.

## 1. Affected exact declarations

| Member | Exact declaration | Header | V34 disposition |
|---|---|---|---|
| `AcApDocManagerReactor::veto` | `protected: Acad::ErrorStatus veto();` | `acdocman.h` | IN `FIXTURE_SUPPORT_ACTION` only from controlled `documentLockModeWillChange` reactor |
| `AcApDocManager::lockDocument` | `virtual Acad::ErrorStatus lockDocument(AcApDocument*,AcAp::DocLockMode=AcAp::kWrite,const ACHAR*=NULL,const ACHAR*=NULL,bool=true)=0;` | `acdocman.h` | IN `FIXTURE_SUPPORT_ACTION` |
| `AcApDocManager::unlockDocument` | `virtual Acad::ErrorStatus unlockDocument(AcApDocument*)=0;` | `acdocman.h` | IN `FIXTURE_SUPPORT_ACTION` |
| `AcApDocument::transactionManager` | `virtual AcTransactionManager* transactionManager() const=0;` | `acdocman.h` | IN `FIXTURE_SUPPORT_ACTION` |
| `AcDbTransactionManager::startTransaction` | `virtual AcTransaction* startTransaction()=0;` | `dbtrans.h` | IN `FIXTURE_SUPPORT_ACTION` |
| `AcDbTransactionManager::endTransaction` | `virtual Acad::ErrorStatus endTransaction()=0;` | `dbtrans.h` | IN `FIXTURE_SUPPORT_ACTION` |
| `AcDbTransactionManager::abortTransaction` | `virtual Acad::ErrorStatus abortTransaction()=0;` | `dbtrans.h` | IN `FIXTURE_SUPPORT_ACTION` |

The last four declarations are newly added individual member rows; the first three reclassify V33 OUT rows. Patched census: exact members 220; MATCH 213; DRIFT 7; newly recognized IN 18; OUT_BY_OPERATION 162; UNKNOWN 0. Axes remain orthogonal. Callback universe remains 152 total / 26 reachable / 126 OUT / 0 UNKNOWN.

## 2. Callback corrections

`N-DB-MOD` and `N-DB-ERASE` now each project to three concrete PRIMARY probes. All 26 reachable callbacks have primary characterization. Capability taxonomy remains conservative; runtime illegality yields UNKNOWN and never definition PASS.

## 3. Predicates

`ActualHeaderReconciliationComplete = TRUE` and `NativeCallbackCapabilitySetClosed = TRUE` as definition predicates because all affected declarations are exact, every reachable callback has primary coverage, and support-action UNKNOWN count is zero.
