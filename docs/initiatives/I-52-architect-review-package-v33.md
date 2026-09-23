# I-52 — Architect Review Package — Proposal V33

> Review the exact published SHA only. Do not execute CT-DA and do not implement helper, harness, fixture, probes,
> RACKMIRROR or AUTH-15.

## 1. Objects

- `docs/initiatives/I-52-proposal-v33.md`;
- `docs/initiatives/I-52-objectarx-member-capability-v33.md`;
- `docs/initiatives/I-52-native-scheduler-catalog-v33.md`;
- `docs/initiatives/I-52-native-event-catalog-v33.md`;
- `docs/initiatives/I-52-native-probe-matrix-v33.md`;
- `docs/automation/evidence/I-52-objectarx-member-reconciliation-v33.json`;
- `docs/automation/decisions/I-52.md`, V32 review and V33 publication sections.

Parent SHA is `a216c8c546a7bae9be95f53ad3d0d0684c72e089`. Exact SDK root is `D:\Downloads\CDROM1`; all
eight header hashes and the host executable hash must be reverified before review.

## 2. Decisive refutations

Architect should attempt each contradiction:

```text
NativeProbeCatalogClosed = TRUE
AND an AcApDocManager callable member is omitted.

NativeProbeCatalogClosed = TRUE
AND a reachable callback lacks a primary probe.

NativeProbeCatalogClosed = TRUE
AND a reachable callback lacks NS-SEND or NS-BEGIN-APPCTX origin coverage.

T16NativeSchedulerSetClosed = TRUE
AND beginExecuteInApplicationContext is OUT or unprobed.

T16NativeSchedulerSetClosed = TRUE
AND beginExecuteInCommandContext inherits NS-SEND without exact equivalence.

09N-O = FAIL solely because a complete B/O/C order differs from an unspecified sequence.

HostFixtureContractClosed = TRUE
AND cleanup can leave queued callback data alive without UNKNOWN.

CTDA_HOST_PASS = PASS or any product kind admitted without execution/kind closure.
```

Every conjunction must be impossible.

## 3. Mechanical reconstruction

Recompute independently:

- member records `216`, header axis `209 MATCH / 7 DRIFT`, capability UNKNOWN `0`;
- callbacks `152` total, `26` reachable, `126` OUT, `0` UNKNOWN;
- NPM `94` ProbeIds, unique `94`, exactly 24 fields each;
- one primary authority per probe;
- ContractIds `76`, duplicates/missing `0/0`;
- ExpectedAfter states `10/10` deterministic;
- cleanup unresolved `0`;
- EventIds `27`;
- Event projection `234/234`, missing/extra `0/0`;
- SchedulerIds `3`;
- Scheduler projection `63/63`, missing/extra `0/0`;
- every reachable EventId appears as ScheduleOrigin for NS-SEND and NS-BEGIN-APPCTX.

Do not accept the counts by assertion; derive NEC projections from NPM-V33.

## 4. Authority review

Check the exact `acdocman.h` declarations and comments for:

1. `sendStringToExecute`;
2. `executeInApplicationContext`;
3. `beginExecuteInCommandContext`;
4. `beginExecuteInApplicationContext`.

Confirm the last API is asynchronous until return to the main message loop. Confirm command-context execution requires
application context and differs from NS-SEND in precondition/target/cancellation. Confirm the synchronous API receives no
T16 credit.

For `objectAppended`, transaction about-start/started/aborted, linker will-load/loaded and command-will-start, verify
that capability no longer closes through a marker relation. Each must have primary and deferred coverage.

## 5. Oracle, UNKNOWN and cleanup

For `09N-O`, check that `OBSERVED-ORDER-COMPLETE` is independent from
`EXPECTED-ORDER-INVARIANT-09N-O`. No unpublished B/O/C sequence may cause FAIL.

For every new scheduler row, rejection, nondelivery, illegal mutation context or incomplete drain must produce UNKNOWN.
PASS requires deterministic verification and dedicated-process cleanup. Definition closure must remain separate from an
experimental PASS.

## 6. Required state

```text
CTDA_HOST_PASS = NOT EVALUATED
LIMITED CT-DA = ACTIVE / PAUSED FOR V33 REVIEW
PRODUCT KINDCONTRACTS = STILL OPEN
V18 = GOVERNING
G3 = STOPPED
G3B = NOT OPEN
CT-50 = NOT EXECUTED
```

An agreement reaches technical consensus on the research contract only. Resuming implementation/execution remains a
separate explicit gate.
