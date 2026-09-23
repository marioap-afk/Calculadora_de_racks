# I-52 — Proposal V33 — Native scheduler and reachable callback closure

> **DESIGN / AUTHORITY ONLY — NOT EXECUTED.** Parent Proposal V32 SHA
> `a216c8c546a7bae9be95f53ad3d0d0684c72e089`. V33 neither implements nor executes CT-DA.

## 1. Status and exact identities

- source baseline: `a216c8c546a7bae9be95f53ad3d0d0684c72e089`;
- SDK: ObjectARX 2025 `25.0.58.0`, x64, v143, root `D:\Downloads\CDROM1`;
- host: AutoCAD `R25.0.171.0.0`, `acad.exe` SHA-256
  `2A75996FD2A5C5EE0376FD5BA7CCED227A098193FF495E8CEEDA3A96FE326422`;
- authority: exact installed headers precede web documentation;
- execution: `NOT STARTED`;
- `CTDA_HOST_PASS = NOT EVALUATED`;
- limited authorization: `ACTIVE / PAUSED FOR V33 REVIEW`.

The eight governing header hashes are unchanged from V32 and are reverified by the V33 machine-readable evidence.

## 2. Architect V32 findings and V33 delta

V33 registers and corrects all four findings:

1. `beginExecuteInApplicationContext` is an IN T16 scheduler, not OUT;
2. every reachable callback receives a capability classification, primary coverage and deferred coverage;
3. the API census is member-level rather than grouped by family;
4. `09N-O` records runtime order without failing an unspecified sequence.

V32's exact outermost callback, `09N-O`, aliases, hashes and header-precedence findings remain incorporated. Its closure
claims are superseded.

## 3. Member-level authority

`OMC-V33-1` and its JSON evidence enumerate 216 exact callable/alias/support members. Overloads are separate. The
census contains 209 declaration MATCH and seven retained declaration DRIFT items; the independent capability axis has
no UNKNOWN. There are 26 reachable callbacks, all conservatively classified both
`PRIMARY_MUTATION_CAPABLE` and `DEFERRED_SCHEDULER_CAPABLE`. Exact headers prove none observation-only.

`MARKER_FOR` is a probe relation only. It never closes callback capability.

```text
ReachableCallbackCapabilityClosed(HF) iff
  every reachable callback has primary mutation characterization
  AND every reachable callback has concrete coverage for each materially distinct scheduler
  AND every OUT callback has an individual operation reason
  AND no capability UNKNOWN remains.
```

For the bounded HF path, `commandCancelled` is reachable through the deterministic lock-veto/cancellation path and is
covered as a primary and deferred-capable callback. `commandFailed` alone is `OUT_BY_OPERATION`: no bounded operation
is allowed to fail the command. All 26 reachable catalogued callbacks are covered.

## 4. Native scheduler universe

The complete relevant scheduler set is:

| SchedulerId | Exact member | Decision |
|---|---|---|
| `NS-SEND` | `sendStringToExecute` | retained IN |
| `NS-BEGIN-CMDCTX` | `beginExecuteInCommandContext` | IN with own probes; not equivalent to NS-SEND |
| `NS-BEGIN-APPCTX` | `beginExecuteInApplicationContext` | IN T16; always asynchronous |

`executeInApplicationContext` is the distinct synchronous context action `NX-APPCTX-SYNC`. It receives probe
`13A-SM` and no T16 scheduler credit.

The command-context API is not equivalent to NS-SEND: its exact application-context precondition, MDI-active target,
outstanding-command cancellation and callback delivery differ. Its legal origin is established through the exact
synchronous application-context bridge. The application-context asynchronous API has no declared context restriction;
every enqueue still records status and a rejected call is future UNKNOWN.

```text
T16NativeSchedulerSetClosed(HF) iff
  every reachable scheduling/context member is individually classified
  AND every IN scheduler has concrete S/M/SM probes
  AND every reachable callback origin is crossed with NS-SEND and NS-BEGIN-APPCTX
  AND the material APPCTX -> CMDCTX chain has a concrete probe
  AND rejection, nondelivery and incomplete drain cannot PASS
  AND no scheduler definition UNKNOWN remains.
```

## 5. Callback coverage and compositions

V33 adds primary coverage for `objectAppended`, transaction about-start/started/aborted, linker will-load/loaded and
editor command-will-start. `objectAppended` has distinct S, M and SM probes. The other callbacks use `MUT-ALL`, which
executes and independently verifies S, M and SM; it does not infer one classification from another.

Every one of the 26 reachable callbacks has a concrete origin row for both NS-SEND and NS-BEGIN-APPCTX. This avoids an
unproved origin-independence assumption. `C2APP2CMD-ALL` covers the materially different callback -> application-context
-> command-context chain. Once exact application context is observed, the command scheduler consumes only its callback
and immutable data; origin-specific behavior has already been characterized by the first leg.

T2 is recalculated from capability rather than marker labels. T16 is recalculated from the exact 37-member
`AcApDocManager` census. T15+T16 includes separate will-load and loaded origins for both directly callable schedulers.

## 6. 09N-O order oracle

`OBSERVED-ORDER-COMPLETE` requires immutable monotonic sequence numbers, callback identity, transaction count and state
for every required B/O/C observation. `EXPECTED-ORDER-INVARIANT-09N-O` contains no B/O/C relative ordering because the
exact header declares none.

An unexpected but fully captured order is a research result. It is not FAIL. FAIL requires contradiction of an exact
declared invariant or immutable expected state; incomplete observation is UNKNOWN.

`09N-B` and `09N-O` remain distinct callbacks and probes. Neither implies commit, durability, `C_decide`, `C_report` or
`C_host`.

## 7. NPM-V33-1 and NEC-V33-1

NPM-V33-1 incorporates the 30 V32 rows, normalizes `04N` markers and `09N-O`, and adds 64 concrete rows:

```text
ProbeIds / unique = 94 / 94
Fields != 24 = 0
Zero/multiple primary authority = 0
ContractId definitions / duplicates / missing = 76 / 0 / 0
ExpectedAfter referenced / deterministic / non-deterministic = 10 / 10 / 0
Unresolved cleanup = 0
Normative wildcard/range = 0
Result state = DEFINED / NOT EXECUTED
```

NEC-V33-1 is mechanically regenerated:

```text
EventIds = 27
Reachable callbacks = 26
Event projection expected/actual = 234 / 234
Event missing/extra = 0 / 0
SchedulerIds = 3
Scheduler projection expected/actual = 63 / 63
Scheduler missing/extra = 0 / 0
Reachable callback x NS-SEND gaps = 0
Reachable callback x NS-BEGIN-APPCTX gaps = 0
```

Queued application/command work uses a dedicated scratch AutoCAD process. PASS requires completion tokens, state
verification, deterministic cleanup and externally verified process exit. Rejection, nondelivery, illegal DB context or
unproved drain is UNKNOWN.

## 8. Closure predicates

```text
ActualHeaderReconciliationComplete =
  MemberCensusComplete
  AND ReachableCallbackCapabilityClosed
  AND T16NativeSchedulerSetClosed
  AND no relevant definition UNKNOWN.

NativeProbeCatalogClosed =
  ActualHeaderReconciliationComplete
  AND NativeCallbackCapabilitySetClosed
  AND T16NativeSchedulerSetClosed
  AND NPM complete
  AND projections exact
  AND ExpectedStateDeterministic
  AND cleanup deterministic.
```

Therefore, as a definition contract only:

```text
ActualHeaderReconciliationComplete = TRUE
NativeCallbackCapabilitySetClosed = TRUE
T16NativeSchedulerSetClosed = TRUE
NativeReactorClassSetClosed = TRUE
NativeProbeCatalogClosed = TRUE
ManagedProbeCatalogClosed = TRUE
EventCatalogClosed = TRUE
HostFixtureContractClosed = TRUE
CTDA_HOST_PASS = NOT EVALUATED
```

No runtime behavior, callback legality, scheduler delivery or host invariant is claimed PASS.

## 9. Host-to-kind and product boundary

The new SchedulerIds and capability universe enter future `EventClassCoverageCompatible(k)`,
`SchedulerCoverageCompatible(k)`, `PrimitiveSetCoverageCompatible(k)` and `LifecycleIntegrationCompatible(k)` checks.
No HostFixture result admits a product kind.

Selective, Dynamic/PalletFlow, PushBack, Cantilever, Header/Cabecera and Flow Bed/Cama remain OPEN and not admissible.
V25 remains normative for product KindContracts.

## 10. I-55, Foundation, I-49 and AUTH-15

I-55 remains at `08e45e0a2521a2e7feb4cf7c8d3fc7fc574aa74d`: host research impact NON-MATERIAL;
HostToKind/KindContract coordination MATERIAL because the scheduler universe changed; future reconciliation REQUIRED;
evidence transfer NONE. I-55 partial placement does not alter I-52 one-T/one-commit/all-or-nothing semantics.

Foundation AUTH-01..13 remain referenced. I-49 remains `SymbolId + complete RootCauses(variable)` per variable actually
read. AUTH-15 remains `I-52 OWNED / NOT IMPLEMENTED`.

## 11. Dispositions and process

- V32 actual-header findings remain incorporated; only its closure claims are superseded.
- V31 remains historical; OA-V28's absence claim remains superseded for SDK 25.0.58.0.
- V24 retains C points, DA-P12, T16, S/M/SM and resource rules.
- V20 remains `PRESERVED / RESTRICTED`.
- V18, Freeze V18 and O-1 V18 remain governing; ADR-0036 remains proposed/amended for V18.
- G3 is STOPPED; G3B is NOT OPEN; CT-50 is NOT EXECUTED.

If exact V33 reviews reach consensus, the limited CT-DA authorization may be explicitly resumed. V33 itself does not
resume it, implement the research instrument, execute probes or authorize product work.

## 12. Invalidators and review checklist

Any header/host hash, SDK, scheduler signature/comment, callback inventory, probe row, expected-state, cleanup,
projection or HF operation-set change invalidates the affected closure and requires exact recensus.

Architect must attempt to refute:

1. all 216 member identities and OUT reasons;
2. all 26 reachable callback capability decisions;
3. the exact scheduler universe and command-context legal origin;
4. 26 x SEND and 26 x APPCTX origin coverage;
5. APPCTX -> CMDCTX composition;
6. `09N-O` observation/invariant separation;
7. 94-row NPM and 234/63 projections;
8. fail-closed UNKNOWN and cleanup behavior;
9. absence of any inference to CTDA_HOST_PASS or product admission.
