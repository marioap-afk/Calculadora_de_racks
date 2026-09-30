# I-52 — CT-21D Baseline Artifact BA-09: Warm-up Definition (DRAFT V2)

> **BASELINE ARTIFACT BA-09 V2 — DRAFT, NOT SEALED. Design / documentation only. No warm-up has been run.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-WARMUP
> ARTIFACT_VERSION          = 2-DRAFT   (supersedes 1-DRAFT, blob 250a5c78483f2d326d36086fdb4c8bba6f0a4a80, which stays as history)
> ARTIFACT_STATUS           = DRAFT (phase 2)     CANDIDATE_HASH = UNSET     SEAL_HASH = UNSET
> AUTHORITY_CONTRACT        = CT-21D V2.1 13.1, 12.6 + V2.2 PA-5, PA-12 + V2.3 + V2.4
> PHASE-1 RULING APPLIED    = linked to the mutable static-state census of BA-06 V2; residual not limited to the library cache
> ORDER                     = warm-up -> record loads -> validate manifest -> subscribe -> baseline -> governed window
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 1. Nature and limits

`WARMUP_SIDE_DATABASE` is **`CONTROL_PLANE_ACTIVITY`** and **`NOT_PRODUCT_MUTATION`** (V2.2 PA-5). The warm-up claims **no isolation** and does **not** authorize a productive warm-up. `PRELOAD_SET` is an operational restriction, not isolation evidence.

## 2. Procedure (numbering of V2.1 13.1; not executed)

| Step | Action |
|---|---|
| 1 | **warm-up** on the `WARMUP_SIDE_DATABASE` (created and owned by the control plane, disposable, never saved, never registered as a document): a synthetic minimal plan per fixture family exercises the paths of section 3; at the end of the step the side database is discarded |
| 2 | **record all loads** (assembly snapshot before and after) |
| 3 | **validate the manifest**: every warm-up load belongs to the manifest; otherwise refuse (O1; scenario `NC-WU-FOREIGN`) |
| 4 | **subscribe** E-11M and E-12 |
| 5 | **capture the E-10 baseline** (two snapshots) |
| 6 | enter the **governed window** |

From step 4 through CP there are no side-database writes by the product or the control plane; the warm-up does not mutate the `LibraryInputRecord` and does not change the library generation id; the seams receive the side database only as an argument; the P-5 guards are unchanged.

## 3. Paths exercised

| Path | What is exercised |
|---|---|
| SL-1, SL-2 (caller-owned creation; AUTH-15 overload A if chosen) | `LateralHeaderDrawer.CreateSystemBlock` on the side database with a side transaction, including nested groups, piece references, dynamic parameters, texts, dimensions, the two RackCad layers and the envelope write and read-back |
| SL-4 | top-level reference placement on the side database |
| SL-5 | import from the private library copy `Database` into the side database |
| I-12 fingerprint, I-11 comparator, I-15 log writer, I-01 sequence | both fingerprint layers; full record types of a synthetic run |
| hashers, cryptography, file I/O | SHA-256 over files and over the private copy |
| E-12 and E-11M handler code | attached to the side database only; detached before step 4 |
| JSON serialization | `RackEmbedStore`, `SelectivePalletDesignStore`, catalog provider (System.Text.Json) |

## 4. Coverage criterion and residual

**`WARMUP_COVERAGE_CRITERION`.** No assembly-load event inside the governed window in the non-governing dry runs of each fixture family (`WU-DRY-*`, BA-04 V2); the number of dry runs follows `PARAM-01` (UNSET). A load inside the window is an E-11M `FAIL` (class B) and a design finding.

**Declared residual (lazy-load paths).** Error and abort paths; host modules loaded on demand for object types or features (dynamic-block evaluation, dimension regeneration); rare plan multiplicities; culture resources (the solution has **no** `.resx` or satellite assemblies: evidence `B7` P13, so only framework and host satellites apply); code reachable only during a real document mutation.

## 5. Load manifest (candidates; captured at execution)

Product assemblies on the mirror path: `RackCad.Domain`, `RackCad.Application`, `RackCad.Plugin` (evidence `B7` F4). **`RackCad.UI`** is loaded by the product commands because they open WPF windows (`B7` S3); the mirror/harness path opens **no** window, so a `RackCad.UI` load inside the governed window is a **finding** (and `RackCad.UI` has non-readonly static fields: BA-06 V2 section 6). Framework assemblies on the seam: `System.Text.Json`, `System.Collections.Concurrent`, `System.Linq` and core libraries (`B7` F1, F3; `System.Security.Cryptography` is **not** on the product seam, `B7` F2, so it enters only through the instruments). Host: `AcCoreMgd`, `AcDbMgd`, `AcMgd` (hint path references, `B7` P5). The actual list with paths and SHA-256 is captured by the control plane (BA-07 V2).

## 6. Mutable static state and `WARMUP_STATIC_STATE_RESIDUAL` (linked to BA-06 V2 section 6)

The census of BA-06 V2 found **7** members written at run time (tier A) and **43** mutable by type but never written by repository code (tier B) across Domain, Application and Plugin; **none** in Domain; no static constructor, module initializer, thread-static or static event. The rule for each seam-relevant member:

| Member | Where | Warm-up rule |
|---|---|---|
| `BlockLibraryDatabaseCache` (`cachedPath`, `cachedWriteUtc`, `cachedLength`, `cachedLibrary`) | `RackCad.Plugin/Drawing/BlockLibraryDatabaseCache.cs:11-15` | the mirror path does **not** use it (private-copy authority); the warm-up must not fill it, or the control plane records that it is unchanged across steps 1..6 |
| `JsonRackCatalogProvider.Cache` (per-directory catalog cache, re-read on change) | `RackCad.Application/Catalogs/JsonRackCatalogProvider.cs:48-49` | allowed to be filled by the warm-up (same catalog directory, pinned by hash in the tuple, BA-08 V2 PH2-TP-1); the governed run re-validates the catalog files by hash |
| `RackLog.sink` (log directory fixed at first use) | `RackCad.Application/Diagnostics/RackLog.cs:18` | file-system side effect only; recorded |
| `UserSettingsStore` (moves a corrupt `settings.json` to `.bad`) | `RackCad.Application/Settings/UserSettings.cs:111` | file-system side effect; the control plane records the settings file hash before and after |
| JSON serializer options (14 static readonly) | Application (evidence `critic_census`) | framework metadata caching only; no output effect |
| `RackFrameTemplateCatalog.All` | `RackCad.Application/RackFrames/RackFrameTemplateCatalog.cs:21` | mutable by type, never written (evidence `critic_census` F13); recorded |
| `RackCad.UI` static fields | UI | outside the mirror path; a load is a finding |

**`WARMUP_STATIC_STATE_RESIDUAL`** (remains disclosed, Owner Act 2 item 15): host-internal static state and any static state the census cannot enumerate (framework and host caches, AutoCAD's own static state).

## 7. Boundary controls

The 11 conditions of V2.2 PA-5 and the scenario `WU-BOUNDARY` (document transaction count unchanged; no document created or registered; no governed-subscription event; side database discarded before step 4).

## 8. What remains

| Id | Item | Gate |
|---|---|---|
| W-1 | the warm-up synthetic plans per family (depend on the fixtures of BA-08 V2) | baseline preparation |
| W-2 | the number of dry runs (`PARAM-01`) | Owner decisions |
| W-3 | the manifest capture | execution prerequisite EXEC-11 |
| W-4 | qualification of `WU-BOUNDARY` | host |
| W-5 | seal of this artifact | baseline review |

```text
WARMUP = DRAFT V2 ; NOT EXECUTED ; NO PRODUCTIVE WARM-UP AUTHORIZED
CT21D_AUTHORITY_BASELINE_READY = FALSE
```
