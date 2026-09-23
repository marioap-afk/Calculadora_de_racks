# I-52 — Architect review package Proposal V28

> Review exacta de OA-V28-1 / NEC-V28-1 / HF-V28-1. No ejecutar probes ni modificar archivos.

## 1. Exact object

El reviewer debe usar el SHA y blobs publicados con V28, no la punta mutable. Autoridades de entrada:

```text
Parent I-52 = fd665c68d299dcd079a6890ed680807c7303f38b
Proposal V27 Correction blob = 59dc39cb2c0e4547df62ae3e86b66ad962d54801
HEC-V27-C1 blob = 4a5555a3f051922b89fcf7a0124cebe2edda34de
Architect package V27 Correction blob = abe948cd3e89583a2f5eb2d576a48ff13ff05368
Decisions parent blob = e5b00328456d7171cd2f062f362ff6cfff865661
```

## 2. Central refutation

Intentar satisfacer cualquiera:

```text
NativeProbeCatalogClosed = TRUE
AND a required callback/scheduler lacks OFFICIAL_EXACT_VERSION_DOC or EXACT_HEADER

NativeProbeCatalogClosed = TRUE
AND 16N-S/M/SM lacks one exact native scheduler, origin, mutation or oracle

HostFixtureContractClosed = TRUE
AND T2+T16 remains definition-UNKNOWN while IN

CTDA_HOST_PASS = TRUE merely because the contract is CLOSED

CTDA_PASS(k) = TRUE with KindContractClosed(k) = FALSE
```

Any satisfiable case is BLOCKER.

## 3. Authority review

1. Confirm local SDK/header absence is stated without invented hashes.
2. Resolve every normative Autodesk URL under exact `2025/ENU`.
3. Compare exact signatures for database, transaction, editor, document-manager and linker APIs.
4. Confirm declared headers: `dbmain.h`, `dbtrans.h`, `aced.h`, `acdocman.h`, `rxdlinkr.h`, `acedads.h`.
5. Ensure cross-version docs/inference receive no closure credit.
6. Confirm official docs, not webpage fingerprints, are the evidence class.

## 4. Event review targets

- Can `objectOpenedForModify` support the defined attempt without mutating its const notifier?
- Are `transactionAboutToEnd` counts sufficient to specialize outermost without inventing a callback?
- Is `transactionEnded` described without upgrading it to postcommit certainty?
- Does `commandEnded` exact authority permit an explicit T and require it to finish before callback return?
- Is any event automatically equated to `C_host` or SUCCESS?

## 5. Scheduler review targets

- Verify exact `sendStringToExecute` signature and the 2025 document-context rule: `activate=false` queues.
- Verify NS-SEND target and booleans are fixed.
- Confirm NS-BEGIN-CMDCTX is supplemental and not used from a document-context reactor.
- Confirm `executeInApplicationContext`, ambiguous begin-app wording, managed Idle and sleeps get no 16N credit.
- Confirm queued command causality is tested rather than assumed safe.

## 6. Probe review targets

For 02N, 04N, 09N-A/B/C/D, 10N-S/M/SM, 16N-S/M/SM verify:

- exactly one primary authority;
- exact setup/trigger/surface;
- observed primary/top T, lock, thread/context;
- independent DB/log verifier;
- PASS/FAIL/UNKNOWN;
- cleanup;
- result still `NOT EXECUTED`.

For 09N-B confirm `N-TR-ABOUT-END + count==1`, not `endCalledOnOutermostTransaction`.

## 7. Threat composition

- T2+T4: transaction callbacks directly embody the composition; no double credit.
- T2+T16: N-DB-MOD calls NS-SEND; queued command mutates later.
- T15+T16: `acedArxLoad` → `rxAppLoaded` → registration → N-DB-MOD → NS-SEND → mutation.
- Arbitrary third-party DLL behavior is not inferred.

Challenge whether another native scheduler/event class is required for a claimed property. A new class may remain a
product residual, but if it can change the truth of the claimed host subproperty it is a V28 finding.

## 8. Closure vs execution

V28 claims only definition closure:

```text
NativeProbeCatalogClosed = TRUE
EventCatalogClosed = TRUE
HostFixtureContractClosed = TRUE
CTDA_HOST_PASS = NOT EVALUATED
```

A future probe may FAIL/UNKNOWN without retroactively making a well-defined contract open. Undefined authority/oracle
today would make it open.

## 9. Product and residual gates

Verify HF-V27 gates remain mandatory and only receive exact native manifest inputs. Confirm:

```text
KindContractClosed(Selective) = FALSE
Dynamic/PushBack/Cantilever/Header/Flow Bed = OPEN
```

Host PASS cannot discharge DA-P7 kind coverage, DA-P8 applicability, DA-P10 readset or any HostToKind compatibility
predicate.

## 10. Process/status review

- V28 does not accept the SDK EULA, download SDK, compile helper or run CT-DA.
- Consensus does not itself authorize research execution; a LIMITED CT-DA RESEARCH AUTHORIZATION gate remains.
- V18/Freeze/O-1 govern; ADR-0036 stays proposed/amended for V18.
- G3 stopped; G3B not open; CT-50 not executed.

## 11. Expected report

Report `BLOCKER/HIGH/MEDIUM/LOW`, exact location, counterexample and correction. Then choose:

1. `AGREED WITH PROPOSAL V28 — HOST FIXTURE CONTRACT READY`;
2. `CHANGES REQUIRED — PROPOSAL V29`;
3. `ALT-21C REJECTED — V18 REMAINS GOVERNING`.
