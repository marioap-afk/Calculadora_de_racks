# I-52 — CT-21D Baseline Artifact BA-09: Warm-up Definition (DRAFT V1)

> **BASELINE ARTIFACT BA-09 — DRAFT, NOT HASHED, NOT AGREED. Design / documentation only. Nothing here is executed or authorized. No warm-up has been run.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-WARMUP
> ARTIFACT_VERSION          = 1-DRAFT
> ARTIFACT_STATUS           = DRAFT (phase 1 of baseline preparation)
> ARTIFACT_HASH             = TO_BE_RECORDED_AT_SEAL   (see BA-11)
> AUTHORITY_CONTRACT        = CT-21D V2.1 section 13.1, 12.6 + V2.2 PA-5, PA-12 + V2.3 (AGREED_FOR_BASELINE_ARTIFACT_PREPARATION)
> ORDER                     = warm-up -> record loads -> validate manifest -> subscribe -> baseline -> governed window
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 1. Nature and limits

`WARMUP_SIDE_DATABASE` is **`CONTROL_PLANE_ACTIVITY`** and **`NOT_PRODUCT_MUTATION`**. The warm-up **claims no isolation**. It **does not authorize a productive warm-up** (a warm-up in product mode is out of scope and is reviewed with the replacement Freeze). `PRELOAD_SET` (the warm-up set) is an **operational restriction, not isolation evidence**: it shows that the product's own first-time loads happened before the governed window and says nothing about the absence of other code.

## 2. Procedure (not executed)

| Step (numbering of contract 13.1) | Action |
|---|---|
| 1 | **warm-up** on the **`WARMUP_SIDE_DATABASE`**: the control plane creates a scratch side `Database` (created and owned by the control plane, disposable, **never** the document `Database`), runs a synthetic minimal plan per family against it (paths of section 3) and, at the end of this step, **discards** it (disposal; never saved; never registered as a document). No document write and no product mutation on the governed document |
| 2 | **record all loads** of the warm-up: assembly snapshot before and after the warm-up; the difference is the warm-up load record |
| 3 | **check that every warm-up load belongs to the manifest**; a load outside the manifest refuses the run (O1) |
| 4 | **subscribe** E-11M and E-12 (only now); the side database no longer exists |
| 5 | **capture the E-10 baseline** (two snapshots) |
| 6 | enter the **governed window** |

The warm-up **does not mutate** the `LibraryInputRecord` and does **not** change the library generation id. From step 4 through CP there are **no side-database writes**, neither by the product nor by the control plane.

## 3. Paths exercised by the warm-up

| Path | What is exercised | Basis |
|---|---|---|
| seams SL-1, SL-2, SL-4 | receive the side `Database` and a side transaction **as arguments only**; they never open or create a database | contract 15.3, P-5 guards unchanged |
| SL-5 import | the import from the **private library copy** `Database` into the side database (block clone under `DuplicateRecordCloning.Ignore`) | BA-08 |
| fingerprint provider (I-12), comparator (I-11) | both layers over the side content | BA-05 |
| log writer (I-15), clock and sequence (I-01) | full record types of one synthetic run | contract 18.1 |
| hashers and cryptography, file I/O | SHA-256 over a file and over the copy | contract 12.6 |
| E-12 and E-11M handler code | handlers attached **to the side database only** and detached before step 4 | contract 13.1 |
| product assemblies | the assemblies reached by the paths above | section 5 |
| resource and satellite assemblies | for the message cultures the run may use | contract 13.1; **census required** (section 5) |
| JSON serialization of the payload and envelope | the composer and store used to write the payload | `Application/Persistence/RackEmbedDocument.cs` |

## 4. `WARMUP_COVERAGE_CRITERION` and residual

**Criterion.** The warm-up set is sufficient for a scenario when, in the **non-governing dry runs** of that scenario on a scratch document (group `WU`), **no assembly-load event occurs inside the governed window** after steps 1 to 5. The number of dry runs is a classified parameter (BA-10, `PARAM-01`). The criterion is **measured**, not asserted: a load inside the window is an E-11M `FAIL` (class B) and a design finding.

**Declared residual (lazy-load paths that the warm-up may not exercise).**

| Residual path | Why it may not be exercised |
|---|---|
| error and abort paths | assemblies for exception handling and logging are loaded only when a failure occurs |
| paths that depend on document-specific object types | the host loads its own modules on demand for a given object type or feature (for example dynamic-block evaluation, dimension regeneration) |
| rare plan multiplicities | code reached only for a multiplicity the synthetic plan does not produce |
| culture-specific resources | satellite assemblies of a culture not warmed |
| code reachable only during a real document mutation | cannot be reproduced against a scratch side database |

A load on such a path inside the window is a **finding** under E-11M; it is not tolerated and not hidden.

## 5. Load manifest of the warm-up (candidate names; final content is captured at execution)

Candidate managed assemblies observed in the source tree (`src/`): `RackCad.Domain`, `RackCad.Application`, `RackCad.Plugin`; `RackCad.UI` (WPF) is **not** expected in the governed window (the mirror path has no dialog) and its appearance would be a finding. The .NET runtime assemblies (for example JSON serialization and cryptography) and the host assemblies (`AcCoreMgd`, `AcDbMgd`, `AcMgd`) complete the set. The actual list, with paths and SHA-256, is **captured by the control plane** into the manifest (BA-07) and is **not asserted here**. Census items required before the criterion can be evaluated: the satellite assemblies present in the build, and the library entity types (BA-05 item L-1).

## 6. Boundary controls

| Control | Statement |
|---|---|
| the 11 conditions (V2.2 PA-5) | created and owned by the control plane; disposable; never saved; never registered as a document; discarded before step 4; no mutation of the `LibraryInputRecord`; no change of the generation id; from step 4 through CP no side-database writes by product or control plane; seams only receive the side database as argument; P-5 guards unchanged; no observable static state may alter the governed window |
| `WARMUP_BOUNDARY_CONTROL` (BA-04 `WU-BOUNDARY`) | document transaction count unchanged; no document created or registered; no event delivered to the governed E-11M/E-12 logs; side database discarded before the governed window |
| E-02 | the side database is not a document, so the single-document control is unaffected |
| E-06 | the side database's transactions are outside the normative document count and are logged separately as `WARMUP_SIDE_DB` records |

## 7. `WARMUP_STATIC_STATE_RESIDUAL` (must remain disclosed)

The warm-up must leave **no observable static state that alters the governed window**. Retained static state that the control plane cannot enumerate is the residual `WARMUP_STATIC_STATE_RESIDUAL` (Owner Act 2 disclosure, contract 24 item 15). Static state seen in the source tree that the warm-up **must not** populate or must reset:

| Static state | Where | Rule |
|---|---|---|
| single-slot library database cache (`cachedPath`, `cachedWriteUtc`, `cachedLength`, `cachedLibrary`, guarded by `CacheGate`) | `RackCad.Plugin/Drawing/BlockLibraryDatabaseCache.cs:11-15` | the mirror path does not use this cache (contract 19.1: the private copy is the authority); the warm-up must not fill it, or the control plane must show it is unchanged |
| log files under `%AppData%\RackCad\logs` | `Application/Diagnostics/RackLog.cs:46-48` | a filesystem side effect; the control plane records it, and it does not enter the governed window |
| any other static field found by the completeness review | BA-06 | recorded as residual |

## 8. What is still missing

| Id | Item |
|---|---|
| W-1 | the final warm-up scenario list per family (depends on the fixtures of BA-08) |
| W-2 | the classified number of dry runs (BA-10) |
| W-3 | the satellite-assembly census and the manifest capture (execution prerequisite EXEC-11) |
| W-4 | the qualification of `WARMUP_BOUNDARY_CONTROL` (BA-04 `WU-BOUNDARY`) |
| W-5 | the hash of this artifact and the agreement |

```text
WARMUP = DEFINED AS DRAFT ; NOT EXECUTED ; NO PRODUCTIVE WARM-UP AUTHORIZED
CT21D_AUTHORITY_BASELINE_READY = FALSE
```
