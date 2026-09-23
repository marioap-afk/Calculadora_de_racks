# I-52 — Proposal V32: ObjectARX 2025 actual-header reconciliation

> **DOCS / ARCHITECTURE ONLY.** No helper, harness, fixture, probe, AutoCAD CT-DA or product implementation.

## 1. Status and identities

| Item | Value |
|---|---|
| Starting I-52 | `bd32692f761ed889c49a11312b0145013c36e3c9` |
| Normative V31 | `6b94e438982fc1e5cebe26591a61d3ecf2681fd0` |
| Proposal V31 blob | `b6ef60d7fada01f461b1d170ee8ff2c0606bf893` |
| NPM-V31-1 blob | `a288c3fc3efaa1dc65f477acb57ad07cb7b6ebe8` |
| NEC-V30-1 blob | `f9536fdfc6b97dcb7051f958b6c4dcea5f6d3c32` |
| SDK evidence blob | `43f941109005b23c5442fc21d4992fb62f3e638d` |
| Decisions input blob | `b66c5865a3ddb403d9a76fa237d5609b5883600c` |
| `origin/main` | `7097057cf8685bf5ecc09083cba37379d4a4aae8` |
| I-55 | `08e45e0a2521a2e7feb4cf7c8d3fc7fc574aa74d` |
| I-57 object / target | `a5bc02210f740839e2fac37e63fc00512e5ccee6` / `7097057cf8685bf5ecc09083cba37379d4a4aae8` |

At entry, the branch, HEAD, upstream and refs matched; the tree and stash were empty and no Git operation was
incomplete. WORKFLOW, ROADMAP, HANDOFF, decisions and SDK evidence were present.

## 2. V32 delta and authority

V32 reconciles the entire native normative surface against the official extracted SDK at `D:\Downloads\CDROM1`:
ObjectARX 2025 SDK `25.0.58.0`, x64, v143, with AutoCAD host `R25.0.171.0.0`. AH-V32-1 records eight exact header
hashes, four candidate library hashes and all 50 native API items relied upon or explicitly excluded by the fixture.

For this tuple, `EXACT_HEADER` takes precedence over conflicting exact-version web documentation. Documentation may
explain semantics absent from declarations, but cannot negate a callback present in official header bytes.

## 3. Reconciliation result

```text
Complete native API items = 50
MATCH = 35
DRIFT = 5
OUT with HF-bounded reason = 10
Unresolved definition items = 0
ActualHeaderReconciliationComplete = TRUE
```

The five corrected drifts are: `objectAppended` marker coverage; transaction start markers; distinct
`endCalledOnOutermostTransaction`; actual alias status of `AcEditorReactor2/3`; and the linker will-load marker.
No other exact signature used by NPM changed.

## 4. Transaction reactor and outermost callback

The actual `dbtrans.h` declares eight callbacks. Start callbacks are markers; about-end, ended, about-abort and the
outermost callback are IN; aborted is a marker; ObjectId swap is OUT because the bounded fixture never swaps ids.

```cpp
virtual void endCalledOnOutermostTransaction(int&, AcDbTransactionManager*) {}
```

The header supplies no relative-order, successful-commit, durability or mutation-legality guarantee. V32 therefore
creates `N-TR-OUTERMOST-END-CALLED` and observation probe `09N-O`, while keeping `09N-B` on the distinct
`transactionAboutToEnd(count==1)` callback. This is outcome **C**, a split because the APIs expose materially distinct
notifications. `09N-O` has all 24 fields, immutable `STATE-T-OUTERMOST-END-CALLED`, deterministic cleanup and no
commit inference.

## 5. T2, T4, T2+T4 and P2/P3/P6

- T2 includes every exact IN native callback; marker-only callbacks contribute order but cannot grant primary credit.
- T4 is the sequence of distinct about-to-end, outermost-end-called, ended and command-ended phases. Only identities
  and header-declared signatures are definition facts; runtime establishes total order and state.
- T2+T4 is concretely covered by `09N-A`, `09N-B`, `09N-O`, `09N-C` and `09N-D`.
- P2 uses B/O/C for callback visibility and state; P3 uses A for nested containment and B/O/C for outermost behavior;
  P6 receives lifecycle evidence from B/O/C but still requires V24 final-fence proof.

## 6. Other class reconciliations

- **Database reactor:** open/modify/erase signatures match; `objectAppended` becomes `N-DB-APPEND` marker; UNDO append,
  sysvar, proxy and teardown callbacks remain OUT.
- **Object reactor:** open/modified/erased/cancelled/modifyUndone/objectClosed/goodbye match. Copy, subobject, XData and
  unappend/reappend stay OUT. Registration/removal is fixed to `dbObject.h` exact authority.
- **Entity reactor:** inheritance from `AcDbObjectReactor` and `modifiedGraphics(const AcDbEntity*)` match; drag clone
  remains OUT.
- **Editor reactor:** four command callbacks match. `AcEditorReactor2` and `AcEditorReactor3` are aliases, not extra
  class universes. `CommandEnded` remains an event, never an automatic `C_host`.
- **Document manager reactor:** three lock signatures match; document create/destroy/activation callbacks remain OUT
  because scratch-document lifecycle lies outside the measured interval.
- **Schedulers:** `sendStringToExecute` and `beginExecuteInCommandContext` match. The latter remains catalogued without
  an NPM primary row. `beginExecuteInApplicationContext` is exact but OUT of the claimed scheduler scope.
- **Load/linker:** `rxAppWillBeLoaded` is a new marker before `rxAppLoaded`; `acedArxLoad` stays a load action, not a
  mutation trigger. Abort/unload callbacks receive no closure credit.

## 7. NPM-V32-1 and NEC-V32-1

NPM-V32-1 incorporates V31 and changes only rows affected by exact headers. It adds `09N-O` and one immutable state
ContractId. NEC-V32-1 is regenerated mechanically from NPM, including the new transaction, append and will-load
EventIds.

```text
ProbeIds / unique = 30 / 30
Fields != 24 = 0
Zero/multiple primary authority = 0
ContractId definitions / duplicates / missing = 55 / 0 / 0
ExpectedAfter referenced / deterministic / non-deterministic = 9 / 9 / 0
Unresolved cleanup = 0
Normative wildcard/range = 0
Native EventIds = 27
Event projection expected/actual = 116/116
Event missing/extra = 0/0
Scheduler projection expected/actual = 10/10
Scheduler missing/extra = 0/0
```

Expected-state validation recursively rejects runtime-selected alternatives. All rows remain
`DEFINED / NOT EXECUTED`; cleanup is one literal deterministic ContractId per row.

## 8. Definition closure

```text
ActualHeaderReconciliationComplete = TRUE
NativeReactorClassSetClosed = TRUE
NativeProbeCatalogClosed(NEC-V32-1,NPM-V32-1) = TRUE
ManagedProbeCatalogClosed(HEC-V27-C1) = TRUE
EventCatalogClosed = TRUE
HostFixtureContractClosed = TRUE
CTDA_HOST_PASS = NOT EVALUATED
```

This closes the research instrument definition, not its empirical result. The limited authorization remains
`ACTIVE / PAUSED FOR ARCHITECTURE RECONCILIATION`; implementation cannot resume before exact V32 reviews and
technical consensus.

## 9. HostToKind, products and adjacent authorities

V27 residual gates remain unchanged. A future kind using any V32-added event must satisfy
`EventClassCoverageCompatible(k)` and related compatibility gates; host closure never admits a product kind.
Selective, Dynamic, PushBack, Cantilever, Header and Flow Bed remain OPEN.

I-55 is unchanged. Native host impact is NON-MATERIAL; the expanded EventId universe makes HostToKind/KindContract
coordination MATERIAL; future reconciliation is REQUIRED and evidence transfer is NONE. Foundation AUTH-01..13 stay
by reference. I-49 remains `SymbolId + complete RootCauses(variable)` for each variable actually read. AUTH-15 remains
`I-52 OWNED / NOT IMPLEMENTED`.

## 10. Dispositions

V31 keeps historical consensus, but its native definition closure is superseded for SDK 25.0.58.0 by actual-header
authority. OA-V28's statement that `endCalledOnOutermostTransaction` is absent is
`SUPERSEDED / INCORRECT FOR SDK 25.0.58.0`. NEC-V30 and NPM-V31 remain historical for native actual-header semantics;
unaffected ContractIds are incorporated.

V25 KindContracts, V24 success-boundary/resource rules, V20 `PRESERVED / RESTRICTED`, V18, Freeze V18 and O-1 V18
remain governing. ADR-0036 remains `PROPOSED / AMENDED FOR V18`. G3 is STOPPED, G3B NOT OPEN and CT-50 NOT EXECUTED.

## 11. Process and self-critique

After exact Coordinator and Architect review, technical consensus may remove the architecture pause and allow the
already granted LIMITED CT-DA authorization to proceed through its own controlled research steps. V32 itself starts
no implementation or execution.

1. Actual dbtrans.h contains `endCalledOnOutermostTransaction`: **YES**.
2. Distinct callback has a distinct EventId: **YES**.
3. All normative headers were reconciled: **YES**.
4. 09N-B was explicitly reconsidered: **YES; it stays and 09N-O is added**.
5. Can docs override conflicting exact header? **NO**.
6. Did implementation resume? **NO**.
7. Is CTDA_HOST_PASS still NOT EVALUATED? **YES**.
8. Are product KindContracts still OPEN? **YES**.

```text
PROPOSAL V32 = PUBLISHED / REVIEW REQUIRED
ACTUAL-HEADER RECONCILIATION = COMPLETE
NATIVEREACTORCLASSSET = CLOSED
NATIVEPROBECATALOG = CLOSED
EVENTCATALOG = CLOSED
HOSTFIXTURECONTRACT = CLOSED
CTDA_HOST_PASS = NOT EVALUATED
LIMITED CT-DA = PAUSED PENDING V32 REVIEW
PRODUCT KINDCONTRACTS = STILL OPEN
TECHNICAL CONSENSUS V32 = NOT REACHED

V18 = GOVERNING
G3 = STOPPED
G3B = NOT OPEN
CT-50 = NOT EXECUTED
PRODUCT KINDCONTRACTS = STILL OPEN
```
