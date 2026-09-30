# I-52 — CT-21D Baseline Artifact BA-09: Warm-up Definition (DRAFT V3)

> **BASELINE ARTIFACT BA-09 V3 — DRAFT, NOT SEALED. Design / documentation only. No warm-up has been run.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-WARMUP
> ARTIFACT_VERSION          = 3-DRAFT   (supersedes 2-DRAFT, blob 6be044164563cf1b8530b17b77135d2cb5d1b888, which stays as history)
> STATUS AUTHORITY          = the BA-11 registry entry of this artifact is the only authority for its status and hashes;
>                             this header is frozen, non-authoritative text of the candidate bytes (BA-11 V3 section 2)
> AUTHORITY_CONTRACT        = CT-21D V2.1 13.1, 12.6 + V2.2 PA-5, PA-12 + V2.3 + V2.4
> PHASE-2 RULING APPLIED    = BA09-01 (permission withdrawn; retained state enumerated for the Architect's ruling WU-Q1), BA09-02 (per-scenario criterion),
>                             BA09-03 (warm-up-only library copy; normative rule in BA-08 V3 section 6.1), BA09-04 (E-11M handler by direct invocation), AR2-42
> EVIDENCE                  = docs/automation/evidence/I-52-ct21d-phase2-delta/sa4-static-state-census.json (static-state census with adversarial verification)
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
| SL-1, SL-2 (caller-owned creation) | `LateralHeaderDrawer.CreateSystemBlock` on the side database with a side transaction: nested groups, piece references, dynamic parameters, texts, dimensions, the two RackCad layers, the envelope write and read-back. The drawer declares no static field; its effects stay in the side database (evidence `sa4` CORE-15) |
| SL-4 | top-level reference placement on the side database |
| SL-5 | import from a **warm-up-only** private library copy: its own file copy and its own `Database`, opened and disposed by the control plane within step 1, **never** entered into the run's library cache or `LibraryInputRecord`; the import never goes through `BlockLibraryImporter`, `AutoCadExternalLibraryBlockQuery` or `BlockLibraryDatabaseCache` (BA-08 V3 section 6.1) |
| I-12 fingerprint, I-11 comparator, I-15 log writer, I-01 sequence | both fingerprint layers; full record types of a synthetic run |
| hashers, cryptography, file I/O | SHA-256 over files and over the private copy |
| E-12 handler code | attached to the side database only; detached before step 4 |
| **E-11M handler code** (BA09-04) | **invoked directly** by the control plane with a synthetic assembly-load argument; it is **not** subscribed during the warm-up, so no event reaches the governed E-11M log (V2.2 PA-12 `WARMUP_BOUNDARY_CONTROL`). This is the accepted reading of V2.1 13.1 (AR2-42): the E-11M handler is an application-domain handler and cannot be attached to a side database |
| JSON serialization and the catalog code | `RackEmbedStore`, `SelectivePalletDesignStore`, and the catalog provider **through a provider constructed on a warm-up-only copy of the catalog folder** (a different directory key), never through `RackCatalogLoader`, which always uses the governed key (evidence `sa4` CORE-10); see WU-Q1 |

## 4. Coverage criterion and residual

**`WARMUP_COVERAGE_CRITERION` (per scenario, V2.1 13.1; BA09-02).** The warm-up set is sufficient **for a scenario** when, in the non-governing dry runs **of that scenario** on a scratch document, no assembly-load event occurs inside the governed window after steps 1 to 5. **No per-family reduction is used.** Every governing host scenario `X` of BA-04 V3 has its dry-run scenario `WU-DRY-X` (same plan, fixture specification and injections), including `FX-BIG`, the abort, refusal and injection scenario classes. The number of dry runs per scenario is the Coordinator value `WU_DRY_RUNS` (BA-10 V3). A load inside the window is an E-11M `FAIL` (class B) and a design finding.

**Declared residual (lazy-load paths).** Error and abort paths not reached by the scenario's own dry runs; host modules loaded on demand for object types or features; culture resources (the solution has **no** `.resx` or satellite assemblies: evidence `B7` P13); code reachable only during a real document mutation.

## 5. Load manifest (candidates; captured at execution)

Product assemblies on the mirror path: `RackCad.Domain`, `RackCad.Application`, `RackCad.Plugin` (evidence `B7` F4). **`RackCad.UI`** is loaded by the product commands because they open WPF windows; the mirror and harness paths open **no** window, so a `RackCad.UI` load inside the governed window is a finding, detected by the loaded-assembly list at the window boundaries (BA-07 V3 manifest; BA-06 V3 section 6). Framework assemblies on the seam: `System.Text.Json`, `System.Collections.Concurrent`, `System.Linq` and core libraries (`B7` F1, F3; `System.Security.Cryptography` enters only through the instruments, `B7` F2). Host: `AcCoreMgd`, `AcDbMgd`, `AcMgd`. The actual list with paths and SHA-256 is captured by the control plane (BA-07 V3).

## 6. Retained state after the warm-up (V2.2 PA-5 condition 11): enumeration for the Architect's ruling WU-Q1

V2.2 PA-5 condition 11: "no observable static state may be left that alters the governed window (the code loaded is the intended effect; any other retained state is prohibited)". **The V2 permission to fill `JsonRackCatalogProvider.Cache` is withdrawn** (BA09-01). The static-state census (evidence `sa4`, adversarially verified) finds these members that a warm-up could touch; each row states the proposed treatment. **The Architect rules on each row (WU-Q1) in the delta review; until then none is accepted.**

| Member | Where | What the warm-up would leave | Proposed treatment |
|---|---|---|---|
| `JsonRackCatalogProvider.Cache` (process-wide dictionary keyed by the catalog directory string) | `Application/Catalogs/JsonRackCatalogProvider.cs:48-49` (`sa4` CORE-09) | an entry for the directory it read | read the catalog through a provider on a **warm-up-only copy of the catalog folder**: the entry is under a key the governed window never reads; the **governed key stays absent**, so the governed run's first catalog read is a genuine disk read (validated by hash in the tuple). Retained: one foreign-key entry. Question: is a foreign-key entry "retained state that alters the governed window"? The implementer's reading: it is retained state that the governed window never reads, therefore it does not alter it. Observation: none without reflection (declared) |
| `BlockLibraryDatabaseCache` (single slot: path, write time, length, database) | `Plugin/Drawing/BlockLibraryDatabaseCache.cs:11-15` (`sa4` CORE-02..05) | nothing, because the warm-up never calls it (section 3; BA-08 V3 section 6.1) | not touched; a static guard (EXEC-12) asserts it |
| `RackLog.sink` | `Application/Diagnostics/RackLog.cs:18` (`sa4` CORE-13) | written only through a test seam; in production it keeps its initial value; log **files** may be appended | file-system side effect only; the control plane records the log directory's file list and sizes before step 1 and after step 5 |
| `UserSettingsStore` (no static; reads `settings.json` on every load; a corrupt file is moved to `.bad`) | `Application/Settings/UserSettings.cs` (`sa4` CORE-14) | no in-memory state; possibly a quarantined file | file-system side effect only; the control plane records the hash of `settings.json` before and after |
| `CsvStructuralSectionCatalogProvider.Cache` | `Application/StructuralSections/CsvStructuralSectionCatalogProvider.cs:26-27` (`sa4` CORE-12) | nothing (not on the Selective path) | not touched |
| `RackFrameTemplateCatalog.All` (static built-in list, initialized once, never written) | `Application/RackFrames/RackFrameTemplateCatalog.cs:19-21` (`sa3b` F8.5) | the type's initialization | a consequence of loading and first executing code (tier B) |
| the static `JsonSerializerOptions` of the stores and the framework's per-type metadata caches | `sa4` CORE-17..CORE-19 | read-only options and primed framework caches keyed by CLR type | proposed to be read as a consequence of loading and first executing code (runtime, not repository state); the framework behaviour is UNCERTAIN in the evidence |
| the other Plugin statics (`KindHandlerRegistry.Default` and read-only delegates and arrays) | `sa4` CORE-21 | initialization only (tier B) | a consequence of loading code |
| the four tier-A statics of `RackCad.UI` | `sa4` UI-56 | nothing: the mirror path opens no window | not reached; a `RackCad.UI` load in the window is an E-11M finding |

**`WARMUP_STATIC_STATE_RESIDUAL`** (remains disclosed, Owner Act 2 item 15): host-internal static state and any static state the census cannot enumerate (framework and host caches, AutoCAD's own static state).

## 7. Boundary controls

The 11 conditions of V2.2 PA-5 and the scenario `WU-BOUNDARY` (document transaction count unchanged; no document created or registered; no governed-subscription event; side database discarded before step 4).

## 8. What remains

| Id | Item | Gate |
|---|---|---|
| W-1 | the warm-up synthetic plans per family (depend on the fixtures of BA-08 V3) | baseline preparation |
| W-2 | the number of dry runs per scenario (`WU_DRY_RUNS`, Coordinator value) | BA-10 V3 |
| W-3 | the manifest capture | execution prerequisite EXEC-11 |
| W-4 | qualification of `WU-BOUNDARY` | host |
| W-5 | the Architect's ruling WU-Q1 on the rows of section 6 | Architect (delta review) |
| W-6 | seal of this artifact | baseline review |

```text
WARMUP = DRAFT V3 ; NOT EXECUTED ; NO PRODUCTIVE WARM-UP AUTHORIZED ; WU-Q1 = OPEN (Architect)
CT21D_AUTHORITY_BASELINE_READY = FALSE
```
