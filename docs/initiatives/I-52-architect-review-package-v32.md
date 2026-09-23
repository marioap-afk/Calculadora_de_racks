# I-52 — Architect review package V32

Review exact published SHA only. Do not review mutable tip and do not execute CT-DA.

## Exact objects to verify

- `docs/initiatives/I-52-proposal-v32.md`
- `docs/initiatives/I-52-objectarx-actual-header-authority-v32.md`
- `docs/initiatives/I-52-native-probe-matrix-v32.md`
- `docs/initiatives/I-52-native-event-catalog-v32.md`
- `docs/automation/evidence/I-52-objectarx-header-reconciliation-v32.json`
- `docs/automation/decisions/I-52.md`, V32 entry

Compare against V31 SHA `6b94e438982fc1e5cebe26591a61d3ecf2681fd0`, NPM-V31-1 blob
`a288c3fc3efaa1dc65f477acb57ad07cb7b6ebe8`, NEC-V30-1 blob
`f9536fdfc6b97dcb7051f958b6c4dcea5f6d3c32`, and the exact SDK bytes under `D:\Downloads\CDROM1`.

## Refutation targets

1. Find any normative native declaration absent from the 50-item reconciliation.
2. Find any exact signature that differs from the acquired headers.
3. Show that an OUT callback can occur in the bounded HF operation set and affect a claimed property.
4. Show that `09N-B` and `09N-O` collapse distinct callbacks or that one required phase remains unrepresented.
5. Show any text that infers commit, durability, `C_decide`, `C_report` or `C_host` from the outermost callback.
6. Recompute all 30 rows, 24 fields, 55 ContractIds, 116 event triples and 10 scheduler pairs.
7. Find a nondeterministic ExpectedAfter, unresolved cleanup, wildcard ProbeId or unresolved definition.
8. Attempt `HostFixtureContractClosed=TRUE` while any exact-header IN callback lacks primary/marker representation.
9. Attempt to promote host definition closure into `CTDA_HOST_PASS` or product admission.

## Required review questions

- Does `dbtrans.h` declare `endCalledOnOutermostTransaction(int&,AcDbTransactionManager*)`? Required **YES**.
- Is it `N-TR-OUTERMOST-END-CALLED` and primary authority of concrete `09N-O`? Required **YES**.
- Does 09N-B remain explicitly limited to `transactionAboutToEnd(count==1)`? Required **YES**.
- Are transaction-start, database-append and linker-will-load drifts represented without becoming false primaries?
  Required **YES**.
- Is `AcEditorReactor2/3` treated as aliases, not additional class universes? Required **YES**.
- Is definition UNKNOWN zero only because every relevant callback is IN/MARKER or has an HF-bounded OUT reason?
  Required **YES**.
- Is `CTDA_HOST_PASS` still NOT EVALUATED? Required **YES**.
- Is LIMITED CT-DA still paused pending V32 review? Required **YES**.

## Severity and verdict format

For every finding report severity, exact file/location, affected contract, counterexample and correction. Use one:

```text
Architect: AGREED WITH PROPOSAL V32 — ACTUAL-HEADER RECONCILIATION CLOSED
```

or:

```text
Architect: CHANGES REQUIRED — PROPOSAL V33
```

or:

```text
Architect: REQUIRED AUTHORITY UNKNOWN — ARCHITECTURE REVIEW REQUIRED
```

Always preserve:

```text
CTDA_HOST_PASS = NOT EVALUATED
LIMITED CT-DA = PAUSED PENDING V32 REVIEW
PRODUCT KINDCONTRACTS = STILL OPEN
V18 = GOVERNING
G3 = STOPPED
G3B = NOT OPEN
CT-50 = NOT EXECUTED
```
