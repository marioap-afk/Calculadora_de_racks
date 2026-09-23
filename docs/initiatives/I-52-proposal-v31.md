# I-52 — Proposal V31: outcome determinista de 09N-D

> **DOCS / CONTRACT CORRECTION ONLY.** No ejecuta CT-DA, probes ni implementacion.

## 1. Identidades y preflight

| Campo | Valor |
|---|---|
| Starting I-52 / Proposal V30 | `0c7007685edae1bc17603029212fd415534322dc` |
| Proposal V30 blob | `f141abb9bc29936563f3fe4118a00735c2d20b99` |
| NEC-V30-1 blob | `f9536fdfc6b97dcb7051f958b6c4dcea5f6d3c32` |
| NPM-V30-1 blob | `b07e1270dd290328f52eb9f64b8c0e6b2a44675b` |
| Architect package V30 blob | `8d8790218d2b28074108063fe8a207d48e33e783` |
| Decisions V30 blob | `44f8b1778db6e0f327d181865111f07120697488` |
| `origin/main` | `7097057cf8685bf5ecc09083cba37379d4a4aae8` |
| I-55 | `08e45e0a2521a2e7feb4cf7c8d3fc7fc574aa74d` |
| I-57 object / target | `a5bc02210f740839e2fac37e63fc00512e5ccee6` / `7097057cf8685bf5ecc09083cba37379d4a4aae8` |
| Native probe matrix | `NPM-V31-1` |
| Native event projection | `NEC-V30-1`, incorporated by reference |
| Host fixture revision | `HF-V31-1` |

Preflight: branch, HEAD, upstream and I-52 match; the tree and stash are empty and no Git operation is incomplete.
Main, I-55 and I-57 match the identities above.

## 2. Architect V30 finding

```text
Architect =
CHANGES REQUIRED — PROPOSAL V31

BLOCKER =
09N-D / STATE-CMD-RESULT contains a hidden commit-or-abort semantic branch;
ExpectedAfter is not immutable before trigger

V18 =
GOVERNING
```

V30 otherwise closed NPM 29/29, 24 fields, 55 ContractIds, Event 89/89, Scheduler 10/10, deterministic
doc-lock and C15 cleanup, and the NativeReactorClassSet.

## 3. Narrow V31 delta and design decision

V31 keeps one ProbeId, `09N-D`, because `04N` already characterizes abort. Before `N-ED-END`, setup fixes
`ProbeOutcomeMode=COMMIT`, `ExpectedBefore=STATE-S-0` and `ExpectedAfter=STATE-S-1`. The callback starts the exact
short `T-FRESH`, performs `MUT-S`, commits/ends that transaction successfully, and returns only after transaction
completion. There is no accepted abort branch.

Primary authority remains `N-ED-END`; lifecycle markers, verifier `VER-S+VER-T` and cleanup `CLEAN-BASE` remain.
`CommandEnded` remains an event observation and is not inferred to be `C_host`.

## 4. Immutable outcome and oracle

Expected authority is fixed before trigger. Actual state, writer output and runtime outcome cannot select or redefine
it. A fresh independent verifier compares the DB with the predeclared value.

- **PASS:** `N-ED-END` fires; `T-FRESH` starts legally; `MUT-S` executes; commit/end succeeds; callback returns after
  transaction completion; fresh DB read equals `STATE-S-1`; required order/markers and cleanup complete.
- **FAIL:** observable callback, transaction or lifecycle ordering contradicts the contract, or a successfully
  committed transaction reports success while the DB differs from `STATE-S-1`.
- **UNKNOWN:** transaction start, mutation or commit is illegal/unavailable/unobserved, a required field is missing,
  verifier is incomplete or cleanup is incomplete, without a demonstrated contradiction.

Abort may only clean up an owned open transaction after FAIL/UNKNOWN. It cannot satisfy ExpectedAfter or PASS.
Abort characterization remains in `04N`.

## 5. STATE-CMD-RESULT disposition and ContractIds

V30 `STATE-CMD-RESULT` is explicitly superseded because it encoded a runtime-selected transaction outcome. No other
row uses it, so NPM-V31 removes it rather than preserve dead authority. The literal ContractId count becomes 54.

## 6. ExpectedStateDeterministic

```text
ExpectedStateDeterministic(c) iff
  c names exactly one immutable expected state/outcome
  AND c contains no alternative outcome branch
  AND c does not defer selection to runtime observation
  AND neither writer nor harness can select it after trigger.
```

Validation recursively resolves every referenced ExpectedAfter ContractId definition and rejects `or`, `either`,
`chosen`, `actual result`, `whatever occurred`, `recorded outcome`, a committed/aborted alternative, and equivalent
runtime selection.

```text
Referenced ExpectedAfter ContractIds = 8
Semantically deterministic = 8
Non-deterministic = 0
Runtime-selected = 0
OR-branch definitions = 0
Post-trigger expected-selection paths = 0
```

## 7. Mechanical validation and projections

```text
ProbeIds / unique = 29 / 29
fields != 24 = 0
blank cells = 0
zero/multiple primary authority = 0
ContractId definitions = 54
duplicate ContractIds = 0
missing ContractIds = 0
unresolved ExpectedBefore/After operands = 0
unresolved Cleanup = 0
normative wildcard/range = 0
Event relation triples expected/actual = 89/89
Event missing/extra = 0/0
Scheduler pairs expected/actual = 10/10
Scheduler missing/extra = 0/0
```

Because the ProbeId, primary authority, origin and markers of `09N-D` do not change, the derived Event and Scheduler
projections remain exactly NEC-V30-1. V31 incorporates that artifact by reference and does not publish a duplicate NEC.

## 8. Preserved V30 contracts

Doc-lock outcomes remain `STATE-SM-1`, `STATE-SM-1`, `STATE-SM-0` for WILL, CHANGED and VETO. C15 remains bound to
`CLEAN-C15-PROCESS`. PASS/FAIL/UNKNOWN ContractIds, projection mechanics, the 29-row shape and
`NativeReactorClassSetClosed(HF-V29-1)=TRUE` remain unchanged.

## 9. Closure versus execution

```text
NativeReactorClassSetClosed(HF-V29-1) = TRUE
NativeProbeCatalogClosed(NEC-V30-1,NPM-V31-1) = TRUE
ManagedProbeCatalogClosed(HEC-V27-C1) = TRUE
EventCatalogClosed = TRUE
HostFixtureContractClosed(HF-V31-1) = TRUE
CTDA_HOST_PASS = NOT EVALUATED
```

These are definition closures. Every probe remains `DEFINED / NOT EXECUTED`; V31 does not authorize CT-DA.

## 10. HostToKind, product and I-55

HostToKind gates and DA-P7/8/10 product residuals do not change. Selective, Dynamic, PushBack, Cantilever, Header and
Flow Bed remain OPEN.

I-55 remains `08e45e0a2521a2e7feb4cf7c8d3fc7fc574aa74d`: native host contract `NON-MATERIAL`, HostToKind and KindContract
`MATERIAL`, future reconciliation `REQUIRED`, evidence transfer `NONE`. Foundation AUTH-01..13 remain referenced;
I-49 remains `SymbolId + complete RootCauses(variable)` per variable actually read; AUTH-15 remains
`I-52 OWNED / NOT IMPLEMENTED`.

## 11. Dispositions and process

V30 is historical. V31 supersedes only 09N-D outcome semantics, STATE-CMD-RESULT, the ContractId count, recursive
ExpectedAfter validation and resulting closure. V29 class census and HEC-V27-C1 remain incorporated; V25 KindContracts,
V24 lifecycle/resources and V20 `PRESERVED / RESTRICTED` remain normative in their scope.

V18, Freeze V18 and O-1 V18 remain governing; ADR-0036 remains `PROPOSED / AMENDED FOR V18`. `G3=STOPPED`,
`G3B=NOT OPEN`, `CT-50=NOT EXECUTED`.

After exact Coordinator and Architect review and technical consensus, the next gate is LIMITED CT-DA RESEARCH
AUTHORIZATION. No execution follows automatically.

## 12. Self-critique and review checklist

1. Can 09N-D PASS after abort? **NO**.
2. Is ExpectedAfter fixed before N-ED-END? **YES**.
3. Does runtime outcome choose ExpectedAfter? **NO**.
4. Does a referenced ExpectedAfter contain an alternative outcome branch? **NO**.
5. Does expected authority defer to a chosen outcome? **NO**.
6. Is abort characterized elsewhere? **YES, 04N**.
7. Were class census, doc-lock or C15 reopened? **NO**.
8. Does fixture closure imply runtime PASS? **NO**.
9. Does V31 authorize CT-DA? **NO**.
10. Do product KindContracts remain open? **YES**.

```text
PROPOSAL V31 = PUBLISHED / REVIEW REQUIRED
NATIVEREACTORCLASSSET = CLOSED
NATIVEPROBECATALOG = CLOSED
EVENTCATALOG = CLOSED
HOSTFIXTURECONTRACT = CLOSED
CT-DA-HOST RESEARCH CONTRACT = READY FOR REVIEW
PRODUCT KINDCONTRACTS = STILL OPEN
TECHNICAL CONSENSUS = NOT REACHED

V18 = GOVERNING
G3 = STOPPED
G3B = NOT OPEN
CT-50 = NOT EXECUTED
SUBSTANTIVE IMPLEMENTATION = BLOCKED
```
