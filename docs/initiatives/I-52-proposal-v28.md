# I-52 — Proposal V28: ObjectARX 2025 native authority

> **DESIGN + AUTHORITY DISCOVERY ONLY.** No ejecuta CT-DA, no implementa helper, RACKMIRROR ni AUTH-15.

## 1. Status e identidades

| Campo | Valor |
|---|---|
| Starting I-52 / V27 Correction | `fd665c68d299dcd079a6890ed680807c7303f38b` |
| Proposal V27 Correction blob | `59dc39cb2c0e4547df62ae3e86b66ad962d54801` |
| HEC-V27-C1 blob | `4a5555a3f051922b89fcf7a0124cebe2edda34de` |
| Architect package V27 Correction blob | `abe948cd3e89583a2f5eb2d576a48ff13ff05368` |
| Decisions V27 Correction blob | `e5b00328456d7171cd2f062f362ff6cfff865661` |
| origin/main | `7097057cf8685bf5ecc09083cba37379d4a4aae8` |
| I-55 | `08e45e0a2521a2e7feb4cf7c8d3fc7fc574aa74d` |
| I-57 object / target | `a5bc02210f740839e2fac37e63fc00512e5ccee6` / main SHA anterior |
| V28 authority | `OA-V28-1` |
| V28 native catalog | `NEC-V28-1` |
| Host fixture revision | `HF-V28-1`, V26 core + HEC-V27-C1 + NEC-V28-1 + HF-V27 residual gates |

## 2. V27 Correction consensus

```text
Coordinator =
AGREED WITH PROPOSAL V27 CORRECTION —
MANAGED PROBE CATALOG CLOSED

Architect =
AGREED WITH PROPOSAL V27 CORRECTION —
MANAGED PROBE CATALOG CLOSED

TECHNICAL CONSENSUS V27 CORRECTION =
REACHED
```

`ManagedProbeCatalogClosed=TRUE` remains normative through HEC-V27-C1.

## 3. V28 delta

V28 changes only native definition authority: exact 2025 classes/signatures, native probes, native schedulers,
T2/T4/T16 composition and closure calculations. It incorporates the HF-V26 fixture core, HEC-V27-C1 managed catalog
and HF-V27 HostToKind gates without redesign.

## 4. Authority conclusion

The ObjectARX SDK/header package is not locally installed. Autodesk nevertheless publishes exact 2025 Reference Guide
and Developer Guide pages for every native class/API claimed. Under the evidence classes established by V28,
`OFFICIAL_EXACT_VERSION_DOC` closes definition authority; runtime probes later validate behavior. No cross-version page
or managed analogy receives closure credit.

OA-V28-1 records exact URLs, signatures, documented phase, declared header and observed authority fingerprints.
Header/lib hashes remain mandatory tuple fields once the official SDK is acquired for helper compilation; they are not
fabricated now.

## 5. Native event and scheduler model

NEC-V28-1 binds:

- database: `objectOpenedForModify`, `objectModified`, `objectErased`;
- transaction: `transactionAboutToAbort`, `transactionAborted`, `transactionAboutToEnd`, `transactionEnded`;
- editor: `commandWillStart`, `commandEnded`, `commandCancelled`, `commandFailed`;
- module load marker: `rxAppLoaded` after initialization/registration;
- deferred native scheduler: `AcApDocManager::sendStringToExecute` with `activate=false`;
- application-context supplement: `beginExecuteInCommandContext`;
- exact path load action: `acedArxLoad`.

No `endCalledOnOutermostTransaction` callback is claimed. `09N-B` is the exact
`transactionAboutToEnd(..., count==1, ...)` specialization. No callback is called commit-phase beyond Autodesk's own
wording.

## 6. Native probes

| Probe | Primary authority | Purpose | Status |
|---|---|---|---|
| 02N | N-DB-OPEN | same-context semantic affiliation | DEFINED / NOT EXECUTED |
| 04N | N-TR-ABOUT-ABORT | pre/post abort rollback via N-TR-ABORTED marker | DEFINED / NOT EXECUTED |
| 09N-A | N-TR-ABOUT-END | nested pre-end visibility/order | DEFINED / NOT EXECUTED |
| 09N-B | N-TR-ABOUT-END + count==1 | outermost pre-end specialization | DEFINED / NOT EXECUTED |
| 09N-C | N-TR-ENDED | post-end visibility/order | DEFINED / NOT EXECUTED |
| 09N-D | N-ED-END | write attempt after command-ended notification | DEFINED / NOT EXECUTED |
| 10N-S/M/SM | N-ED-END | final-window semantic/material/mixed writes | DEFINED / NOT EXECUTED |
| 16N-S/M/SM | NS-SEND, origin N-DB-MOD | queued native cross-boundary writes | DEFINED / NOT EXECUTED |
| C15N16N-SM | NS-SEND after NL-ARX-PATH/N-RX-LOADED/N-DB-MOD | module-load + deferred composition | DEFINED / NOT EXECUTED |

One exact primary authority per probe remains mandatory. Lifecycle and load markers are not second triggers.

## 7. T2/T4/T16 closure

- **T2:** represented by exact native database, transaction and editor callbacks.
- **T2+T4:** 09N-A/B/C directly use native transaction callbacks; 09N-D separately covers editor command end.
- **T2+T16:** the N-DB-MOD callback invokes NS-SEND; the queued research command later performs S/M/SM mutation.
- **T15+T16 native:** path load, post-initialization marker, exact callback registration, NS-SEND and deferred mutation
  are separately logged.

The scheduler API is exact and documented. Whether its causal work invalidates every usable C_host is an experimental
result. A future FAIL does not make this definition contract open.

## 8. Closure results

```text
NativeProbeCatalogClosed(NEC-V28-1) = TRUE
ManagedProbeCatalogClosed(HEC-V27-C1) = TRUE
EventCatalogClosed = TRUE
HostFixtureContractClosed(HF-V28-1) = TRUE
CTDA_HOST_PASS = NOT EVALUATED
```

`HostFixtureContractClosed` means the research instrument contract is defined. It is not execution authorization and
not a host PASS.

## 9. Host→kind gates

V27 remains normative:

```text
HostToKindCompatibilityPass(k) iff
  SurfaceWriteCoverage(k)=PASS
  AND TransactionStrategyCompatible(k)=PASS
  AND StoragePathEquivalent(k)=PASS
  AND ReaderPathEquivalent(k)=PASS
  AND SemanticScanStrategyCompatible(k)=PASS
  AND PhysicalScanStrategyCompatible(k)=PASS
  AND EventClassCoverageCompatible(k)=PASS
  AND SchedulerCoverageCompatible(k)=PASS
  AND PrimitiveSetCoverageCompatible(k)=PASS
  AND LifecycleIntegrationCompatible(k)=PASS
  AND LockingStrategyCompatible(k)=PASS
  AND SourceOrderingModelCompatible(k)=PASS
```

`EventClassCoverageCompatible`, `SchedulerCoverageCompatible`, `PrimitiveSetCoverageCompatible` and
`LifecycleIntegrationCompatible` now compare kinds against the exact HEC/NEC class manifests. New native/managed
events, schedulers or primitives require kind-specific evidence. PARTIAL still requires a named mandatory residual.

```text
KindSpecificCoveragePass(k) iff
  DA-P7_KIND_COVERAGE(k)=PASS
  AND DA-P8_KIND_APPLICABILITY(k) resolved/PASS
  AND DA-P10_KIND_READSET(k)=PASS
  AND HostToKindCompatibilityPass(k)=PASS
  AND all required product-specific probes=PASS

CTDA_PASS(k) iff
  CTDA_HOST_PASS(HF-V28-1)
  AND KindContractClosed(k)
  AND KindSpecificCoveragePass(k)
  AND all product-kind fixtures=PASS
  AND exact tuple compatibility=PASS
```

Thus native host evidence cannot admit Selective or any other kind.

## 10. Exact tuple, version and invalidation

Future execution binds AutoCAD 2025, API `25.0.0.0`, exact `acad.exe`, `acdbmgd`, `acmgd`, vertical, registry
identity, OS/runtime, ObjectARX SDK/header/lib hashes, compiler/toolset, x64 helper SHA, harness/probe SHA, OA/HEC/NEC/HF
blobs and scratch DWG hash. Any material drift invalidates affected evidence. A downloaded SDK whose headers disagree
with OA-V28-1 reopens the definition before build.

## 11. C points and runtime boundedness

All native probes use V24 `F_usable`, `C_decide`, `C_report`, `C_host`. N-ED-END is a marker, never an automatic fence.
NS-SEND intentionally creates the decisive queued-work counterexample. If it can mutate after SUCCESS while remaining
IN causal scope, DA-P6/11/12 fail. If only a post-report fence is usable, DA-P12 fails. No universal plugin isolation is
claimed; exact host classes are characterized and product residuals stay gated.

## 12. SDK-unavailable fallback

There is a credible next evidence path: after consensus and explicit limited authorization, obtain the official SDK
2025 under the user's accepted license, hash headers/libs, implement the research-only helper, compile against those
headers and execute NEC probes. SDK absence today does not justify V29 churn because exact official 2025 docs already
close definition. If SDK acquisition fails or headers contradict the catalog, research stops `UNKNOWN` and the
contract reopens; no fallback to cross-version docs.

## 13. I-55, Foundation, I-49 and AUTH-15

I-55 remains `08e45e0a2521a2e7feb4cf7c8d3fc7fc574aa74d`: native host authority impact `NON-MATERIAL`, HostToKind and
KindContract coordination `MATERIAL`, reconciliation `REQUIRED`, evidence transfer `NONE`. Its partial-placement policy
does not transfer to I-52 one-T/one-commit/all-or-nothing.

Foundation AUTH-01..13 remain referenced. Product DA-P10 remains `SymbolId + complete RootCauses(variable)` per
actually read variable. Fixture ordering does not prove that readset. AUTH-15 remains `I-52 OWNED / NOT IMPLEMENTED`.

## 14. Product status

```text
KindContractClosed(Selective) = FALSE
Dynamic = OPEN / NOT ADMISSIBLE
PushBack = OPEN / NOT ADMISSIBLE
Cantilever = OPEN / NOT ADMISSIBLE
Header = OPEN / NOT ADMISSIBLE
Flow Bed = OPEN / NOT ADMISSIBLE
```

## 15. Dispositions

- V27 Correction remains normative for managed events and has technical consensus.
- V28 supersedes only native authority/probes/schedulers, native composition and event/fixture closure status.
- V27 original remains historical; its HostToKind gates remain incorporated.
- V25 remains normative for product KindContracts.
- V24 retains C points, DA-P12, T16, S/M/SM and resource-assignment rules.
- V20 architecture remains `PRESERVED / RESTRICTED`.
- V18, Freeze V18 and O-1 V18 remain governing; ADR-0036 remains `PROPOSED / AMENDED FOR V18`.

## 16. Process and G3

After exact V28 reviews and technical consensus, the next gate is explicit **LIMITED CT-DA RESEARCH AUTHORIZATION**.
Only that gate may authorize official SDK acquisition, research helper/harness/logging implementation and CT-DA
execution. It never authorizes RACKMIRROR, AUTH-15, product guards or product behavior.

```text
G3 = STOPPED
G3B = NOT OPEN
CT-50 = NOT EXECUTED
```

## 17. Mandatory self-critique

1. All native callback names/signatures tied to exact 2025 authority: **YES**.
2. Cross-version docs can close native authority: **NO**.
3. Managed emulation can satisfy native probes: **NO**.
4. Every native probe has one exact callback/scheduler: **YES**.
5. 16N can remain undefined while native catalog closes: **NO; all three are defined**.
6. T2+T16 can remain UNKNOWN while fixture closes: **NO; definition is closed, execution is unevaluated**.
7. Hard-to-implement threat can be OUT without authority: **NO**.
8. Helper proves arbitrary plugin behavior: **NO**.
9. Native host PASS admits Selective: **NO**.
10. V28 executes CT-DA: **NO**.

## 18. Self-refutation

- native catalog true + missing exact authority: impossible by predicate;
- native catalog true + undefined 16N: impossible;
- fixture closed + T2+T16 definition UNKNOWN while IN: impossible;
- event catalog true + native catalog false: impossible;
- `CTDA_PASS(Selective)=true` with open KindContract: impossible by formula.

## 19. Review checklist

- [ ] verify V27 Correction exact identities and consensus registration;
- [ ] verify every official URL is under `cloudhelp/2025` or APS exact 2025 section;
- [ ] verify callback signatures and declared headers;
- [ ] verify 09N-B is a count specialization, not an invented callback;
- [ ] verify NS-SEND exact parameters and document-context queue rule;
- [ ] verify 02N/04N/09N/10N/16N oracle and cleanup;
- [ ] verify T2+T16 and T15+T16 composition;
- [ ] verify no product admission, CT-DA execution or universal plugin claim;
- [ ] verify exact tuple/invalidation and product residual gates.

```text
PROPOSAL V28 = PUBLISHED / REVIEW REQUIRED
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
