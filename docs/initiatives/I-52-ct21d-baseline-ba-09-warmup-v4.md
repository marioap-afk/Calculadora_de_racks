# I-52 — CT-21D Baseline Artifact BA-09: Warm-up Definition (DRAFT V4)

> **BASELINE ARTIFACT BA-09 V4 — DRAFT, NOT SEALED. Design / documentation only. No warm-up and no dry run has been run.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-WARMUP
> ARTIFACT_VERSION          = 4-DRAFT (supersedes 3-DRAFT, blob 5e23250bcec0a2a28084ffe2c8071b296f43fbc4, which stays as history)
> STATUS AUTHORITY          = the BA-11 registry entry of this artifact is the only authority for its status and hashes;
>                             this header is frozen, non-authoritative text of the candidate bytes (BA-11 V4 section 2)
> AUTHORITY_CONTRACT        = CT-21D V2.1 + V2.2 + V2.3 + V2.4 (draft revision 3) + V2.5 (draft revision 1); V2.4 and V2.5 pending
>                             Coordinator textual verification
> CONTRACT CLAUSES USED     = V2.1 2.1, 12.6, 13.1, 13.2, 14.1, 14.2, 21.1, 21.7; V2.2 PA-5, PA-12
> DELTA RULING APPLIED      = decisions section 214 (AR3-01, AR3-02, AR3-16, AR3-24, AR3-25, AR3-30);
>                             section 211 (AR2-42) where section 214 does not change it; V4 correction pass: open Architect
>                             questions AQ-V4-01, AQ-V4-02 and AQ-V4-06 referenced, none decided (sections 4 and 8)
> PHASE-2 RULING (V3, kept) = BA09-01 (cache-fill permission withdrawn), BA09-02 (per-scenario criterion), BA09-03 (warm-up-only library copy;
>                             normative rule in BA-08 V4 section 6.1), BA09-04 (E-11M handler by direct invocation), AR2-42
> EVIDENCE                  = docs/automation/evidence/I-52-ct21d-phase2-delta/sa4-static-state-census.json (static-state census with adversarial verification)
> DEPENDS_ON                = BA-01, BA-06, BA-07, BA-08   (registry entries only; BA-11 V4 section 3). BA-04 V4 depends on this artifact
>                             (its WU-DRY scenarios take their rule, mode and expectation from section 4)
> PREREQUISITES             = W-1 and W-9 (the Architect's answer to AQ-V4-02, or its recorded deferral) before the candidate
>                             (section 8); the other open items are execution prerequisites
> ORDER                     = warm-up -> record loads -> validate manifest -> subscribe -> baseline -> governed window
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 1. Nature and limits

`WARMUP_SIDE_DATABASE` is **`CONTROL_PLANE_ACTIVITY`** and **`NOT_PRODUCT_MUTATION`** (V2.2 PA-5). The warm-up claims **no isolation** and does **not** authorize a productive warm-up. `PRELOAD_SET` is an operational restriction, not isolation evidence.

## 2. Procedure (numbering of V2.1 13.1; not executed)

| Step | Action |
|---|---|
| 1 | **warm-up** on the `WARMUP_SIDE_DATABASE` (created and owned by the control plane, disposable, never saved, never registered as a document), in the order of section 3; at the end of the step the side database is discarded and every warm-up-only object is released |
| 2 | **record all loads** (assembly snapshot before and after) |
| 3 | **validate the manifest**: every warm-up load belongs to the manifest; otherwise refuse (O1; scenario `NC-WU-FOREIGN`) |
| 4 | **subscribe** E-11M and E-12 (the governed handler instances, created now with their initial sequence numbers) |
| 5 | **capture the E-10 baseline** (two snapshots) |
| 6 | enter the **governed window** |

From step 4 through CP there are no side-database writes by the product or the control plane; the warm-up does not mutate the `LibraryInputRecord` and does not change the library generation id; the seams receive the side database only as an argument; the P-5 guards are unchanged. The subscription code itself runs for the first time at step 4: a load it causes before the baseline is complete makes the baseline invalid and refuses the run (V2.1 13.2, O1); the dry runs of section 4 measure it.

## 3. Paths exercised

### 3.1 Order inside step 1 (normative)

| Order | Path | What is exercised |
|---|---|---|
| 1a | warm-up library input | the control plane copies the library file to a **warm-up-only** private copy and opens its own `Database`; it is **never** entered into the run's library cache or `LibraryInputRecord`; nothing goes through `BlockLibraryImporter`, `AutoCadExternalLibraryBlockQuery` or `BlockLibraryDatabaseCache` (BA-08 V4 section 6.1) |
| 1b | **SL-5** import | the import into the side database of every block the synthetic plans of section 3.2 require, from the warm-up-only copy. It runs **before** SL-1 and SL-2 because, on a fresh side database, `CreateSystemBlock` skips every piece whose block is absent (`LateralHeaderDrawer.cs:293-303`; `sa4` verifier, "missed" item 3), so without the import no piece reference would be created and `ApplyDynamicParameters` would never run (D-17) |
| 1c | **SL-1, SL-2** (caller-owned creation) | `LateralHeaderDrawer.CreateSystemBlock` on the side database with a side transaction: nested groups, piece references, dynamic parameters, texts, dimensions, the two RackCad layers, the envelope write and read-back. Every synthetic plan is created **twice** in the same side database, so the absent-layer and present-layer paths and the name-uniquifying path all run. **Repository statics:** none is touched (`sa4` CORE-15, verified for repository statics only). **Host-internal effects** of `DBText.AdjustAlignment` (`LateralHeaderDrawer.cs:280`), `RotatedDimension.RecomputeDimensionBlock` (`:375`) and dynamic-block evaluation (`:432-447`) are runtime behaviour the source does not establish: they belong to `WARMUP_STATIC_STATE_RESIDUAL` (section 6) (D-17) |
| 1d | **SL-4** | top-level reference placement on the side database, through the caller-owned SL-4 seam; `BlockPlacement` cannot be used (it is interactive and bound to the document, `sa4` verifier, "missed" item 2) |
| 1e | I-12 fingerprint, I-11 comparator, SL-6 reader, I-15 log writer, I-01 sequence | both fingerprint layers and the full record types of a synthetic run, on **warm-up-only instances writing to warm-up-only sinks**; counters and buffers are per instance (none is static) and none carries into the governed logs (D-16) |
| 1f | hashers, cryptography, file I/O | SHA-256 over files and over the warm-up copy (V2.1 12.6) |
| 1g | **E-12 handler code** (WRITE_CLASS, STRUCTURE_CLASS, TX_CLASS) | **invoked directly** by the control plane with synthetic event arguments of each category, on **warm-up-only handler instances writing to warm-up-only sinks**; no E-12 handler is subscribed or attached during the warm-up, so no event reaches the governed E-12 log (V2.2 PA-12 `WARMUP_BOUNDARY_CONTROL`). STRUCTURE_CLASS handlers (document-collection, document-command and editor events, V2.1 14.1) cannot be attached to a side database; direct invocation is the reading adopted for every E-12 handler (AR3-16; D-15) |
| 1h | **E-11M handler code** (BA09-04) | **invoked directly** by the control plane with a synthetic assembly-load argument, on a warm-up-only instance and sink; it is **not** subscribed during the warm-up. This is the accepted reading of V2.1 13.1 (AR2-42): the E-11M handler is an application-domain handler and cannot be attached to a side database |
| 1i | JSON serialization | `RackEmbedStore` and `SelectivePalletDesignStore` serialize and parse synthetic documents (their static options objects are executed code: section 6 row 7) |
| 1j | catalog code | **not exercised by `Load()`** (AR3-16 row 1): no `JsonRackCatalogProvider` is constructed or loaded and `RackCatalogLoader` is not called. **Only if** the `WU-DRY` runs show an assembly load at the first governed catalog read, the control plane runs the **type initializer** of `JsonRackCatalogProvider` (an empty `Cache`, the same state the window itself would create) and loads the assemblies the catalog code depends on, **adding no cache entry** |
| 1k | end of step 1 | the warm-up library `Database` is disposed and its copy released; the side database is discarded; every warm-up-only instance and sink is released and is not referenced by any governed object |

### 3.2 Synthetic warm-up plans (rule; data = W-1)

V2.1 13.1 asks for a synthetic minimal plan per family. This artifact uses, for every family, the plans of the fixture specifications that the governing scenarios use, so that no plan multiplicity of a governing scenario is left out of the warm-up (V2.1 13.1 lists rare plan multiplicities in its residual). The rule: **one plan set per fixture specification of BA-08 V4 section 12.3 that a governing scenario uses** (`FX-1F`, `FX-2F`, `FX-4F`, `FX-ANN`, `FX-ANN-BLANK`, `FX-DIM`, `FX-BIG`; `FX-IMP` once K-8 is decided), covering every plan-shape class of BA-06 V4 section 10.2 and the largest multiplicities of the catalog. Each plan set is the `HeaderRunPlan` of every view of that fixture's mirrored design, recorded once, offline, as a `PLAN_CAPTURE_RECORD` (BA-08 V4 section 11.1) from the bound SHA and the bound catalogs, and **replayed** by the control plane into plan objects in step 1: the warm-up calls **neither the catalog provider nor the builders**. The recorded plan data are part of the warm-up definition (V2.1 2.1: warm-up definition version and hash, BUILD_BOUND) and are registered with this artifact before its candidate (W-1).

## 4. Coverage criterion and residual (AR3-02, AR3-01, AR3-24)

**`WARMUP_COVERAGE_CRITERION` (per scenario, V2.1 13.1; AR2-42).** The warm-up set is sufficient **for a scenario** when, in the non-governing dry runs **of that scenario**, no assembly-load event occurs inside the governed window after steps 1 to 5. **No per-family reduction is used**, and no scenario borrows the dry runs of another.

**Which scenarios have dry runs (AR3-02).** A dry-run scenario `WU-DRY-X` is **mandatory for every GOVERNING scenario `X` that enters the governed window**: `RUN_GOVERNANCE = GOVERNING`, `MODE` = `CHARACTERIZATION_RUN` or `VALIDATION`, and `REACH` in `P0..CP` (P0, PL, PR, PP, PW, PS, PV or CP). This includes the scenarios that serve evidence groups; the eleven that V3 left without one are named here: `U-1`, `U-2`, `U-3`, `U-4`, `U-5`, `U-6b`, `U-7`, `U-8`, `S1-SAVE-REOPEN`, `PP-POSTCP` and `CL-CLEANUP-CHECK`. It also includes every governing scenario new in BA-04 V4 that meets the rule (for example `CL-CLEAN-FXANNBLANK-FP`, whose dry run is `WU-DRY-CL-CLEAN-FXANNBLANK-FP`). The V3 sentence "every governing host scenario X has its dry-run scenario WU-DRY-X" is **withdrawn**: the rule above replaces it, and BA-04 V4 lists the dry runs it produces. The rule has **no other qualifier**: being in the AR3-01 mode, or being a qualification scenario, does not exclude a scenario that meets it.

**Qualification vehicles `Q-I14-*` (correction pass).** `Q-I14-Y`, `Q-I14-X`, `Q-I14-SILENT-Y` and `Q-I14-SILENT-X` are, in BA-04 V4, `GOVERNING` scenarios with `MODE = CHARACTERIZATION_RUN` and `REACH = CP`: their product run enters the governed window, so they meet the rule and **have dry runs** (`WU-DRY-Q-I14-Y`, `WU-DRY-Q-I14-X`, `WU-DRY-Q-I14-SILENT-Y`, `WU-DRY-Q-I14-SILENT-X`). Their parents use the characterization build in the AR3-01 mode, a use that AR3-01 does not rule and that is pending the Architect's question **AQ-V4-01**; like their parents, these four dry runs are **`DRAFT` pending AQ-V4-01**. If the ruling makes the vehicles non-governing, they leave the rule and their dry runs are withdrawn.

**Declared exclusions (AR3-02).** No dry run is required for: the `NON_GOVERNING_BY_DESIGN` runs (the `WU-DRY-*` themselves, `E4-LEARN-R1-*`, `LK-*`, `HDM-CHAR-*`) and the exploratory runs; the **learning** runs (`MODE = LEARNING`, `REACH = LEARN`, the `EV-L-*` micro-scenarios), which are not mirror runs with a governed window; the no-product-run scenarios, qualification scenarios included (`REACH = NONE`, for example `WU-BOUNDARY`); and the synthetic evaluator scenarios (`REACH = SYN`). A qualification scenario with a product run in `P0..CP` is not excluded (the `Q-I14-*` vehicles above).

**Dry-run mode (AR3-01, AR3-02).** Each `WU-DRY-X` runs the plan, fixture specification, injections and designed stimuli of `X` on a private scratch copy of the fixture instance, with `BUILD_UNDER_TEST = CHARACTERIZATION_BUILD` and `RECORDED_NOT_ENFORCED = ["E-03","E-04","E-12 phase A","E-12 phase C","S-1","S-2","S-3"]`: those controls are recorded and do not govern the flow, so the run reaches PV, `Commit()`, V1 and PC wherever the path of `X` does. `EXPECTED_PRODUCT_OUTCOME = PROVISIONAL_NON_GOVERNING` (never O1..O7). **`PIN_SLOTS` = only `FIXTURE_INSTANCE` slots**: the fixture of `X` (`FIXTURE_INSTANCE@<FX>`) and, where `X` uses one, its variant library (`FIXTURE_INSTANCE@L-VAR-*`, for example `L-VAR-DIFF` or `L-VAR-PROXY`; BA-04 V4 section 2.1). No EVM, `HOST_DEFAULT_MAP`, E-04 or `LOCK_MODE` pin and no dependency on K-3 (AR3-02: "solo llevan la ranura `FIXTURE_INSTANCE`", read as the pin kind, which the variant libraries share). The measured expectation: **no assembly-load event inside the governed window** (the E-11M log and the E-10 snapshots are recorded).

**Count (AR3-24).** Per governing scenario, the number of dry runs is **`WU_DRY_RUNS = N_B`** of `PARAM-01` (BA-10 V4): the Owner fixes `p_B` and `alpha_B` (Q-O2) and the Coordinator records the value; it is **UNSET**. BA-04 V4 applies it; this artifact does not use its value.

**Consequences.** A load inside the window of a dry run is a design finding: the warm-up is revised (a new version of this artifact, and so of the baseline: the warm-up definition is a BUILD_BOUND tuple field, V2.1 2.1), adding the path only if it leaves no retained state (V2.2 PA-5 condition 11) or else preloading the dependent assembly. A load inside the governed window of a governing run is an E-11M `FAIL` (class B).

**Declared residual (lazy-load paths).** Error and abort paths not reached by the scenario's own dry runs, **including the branches a governing run takes only when a control of the `RECORDED_NOT_ENFORCED` list fails** (the dry run records that failure and does not abort, so it does not take that branch; whether such a branch must instead be forced by a harness switch is the open Architect question **AQ-V4-02**, not decided here); **code differences between the characterization build and the product build** (the dry runs measure the characterization build; the governing run's E-11M still fails on any load in its window); host modules loaded on demand for object types or features; culture resources (the solution has **no** `.resx` or satellite assemblies: evidence `B7` P13); code reachable only during a real document mutation.

## 5. Load manifest (candidates; captured at execution)

Product assemblies on the mirror path: `RackCad.Domain`, `RackCad.Application`, `RackCad.Plugin` (evidence `B7` F4). **`RackCad.UI`** is loaded by the product commands because they open WPF windows; the mirror and harness paths open **no** window, so a `RackCad.UI` load inside the governed window is a finding, detected by the loaded-assembly list at the window boundaries (BA-07 V4 manifest; BA-06 V4 section 6). Framework assemblies on the seam: `System.Text.Json`, `System.Collections.Concurrent`, `System.Linq` and core libraries (`B7` F1, F3; `System.Security.Cryptography` enters only through the instruments, `B7` F2). Host: `AcCoreMgd`, `AcDbMgd`, `AcMgd`. The actual list with paths and SHA-256 is captured by the control plane (BA-07 V4).

## 6. Retained state after the warm-up (V2.2 PA-5 condition 11): the Architect's ruling WU-Q1 (AR3-16)

V2.2 PA-5 condition 11: "no observable static state may be left that alters the governed window (the code loaded is the intended effect; any other retained state is prohibited)". The V2 permission to fill `JsonRackCatalogProvider.Cache` stays **withdrawn** (BA09-01). The Architect ruled each row of the V3 enumeration (decisions section 214, AR3-16); the treatment below is **normative**.

| Row | Member | Where (evidence `sa4`) | Ruling (AR3-16) | Normative treatment |
|---|---|---|---|---|
| 1 | `JsonRackCatalogProvider.Cache` (process-wide dictionary keyed by the raw catalog directory string; `GetOrAdd` inserts the entry even when the load throws) | `Application/Catalogs/JsonRackCatalogProvider.cs:48-49` (CORE-09 and its verifier) | **NOT COMPATIBLE** as proposed in V3 (a `Load()` on a copy of the catalog) | the warm-up performs **no `Load()`** on any copy of the catalog, constructs no provider and calls no `RackCatalogLoader` (section 3.1 row 1j); if the `WU-DRY` runs show a load at the first governed catalog read, only the type initializer (empty cache) and the dependent assemblies are warmed, with **no entry added**. The governed key stays absent, so the first governed catalog read is a genuine disk read of the folder that the tuple field `CATALOG_FOLDER` pins (V2.5). A static guard over the warm-up harness asserts it (BA-08 V4 K-9) |
| 2 | `BlockLibraryDatabaseCache` (single slot: path, write time, length, database) | `Plugin/Drawing/BlockLibraryDatabaseCache.cs:11-15` (CORE-02..CORE-05) | **COMPATIBLE** (not touched), with the guard extended to the orchestrator and PREPARE-R | never called by the warm-up, the seams, the orchestrator or PREPARE-R (BA-08 V4 section 6.1). The static guard covers all four; it is an **additional execution prerequisite derived from AR2-42 and AR3-16** (BA-08 V4 K-9), **not** EXEC-12 (D-09) |
| 3 | `RackLog.sink` | `Application/Diagnostics/RackLog.cs:18` (CORE-13) | **COMPATIBLE** | written only through a test seam; in production it keeps its initial value. File-system effect only: the control plane records the log directory's file list and sizes before step 1 and after step 5 |
| 4 | `UserSettingsStore` (no static; reads `settings.json` on every call; a corrupt file is moved to `.bad`) | `Application/Settings/UserSettings.cs` (CORE-14) | the warm-up **does not call** it | not called (the static guard over the warm-up harness asserts it). The control plane records the SHA-256 of `settings.json` (or `ABSENT`) **before step 1 and at CP**; a difference makes the run **`INVALID`** (environment change; V2.1 21.1). Reason: a quarantined file changes how the governed window resolves the library path (D-18) |
| 5 | `CsvStructuralSectionCatalogProvider.Cache` | `Application/StructuralSections/CsvStructuralSectionCatalogProvider.cs:26-27` (CORE-12) | **COMPATIBLE** (not reached) | not touched: reached only through the structural-section and Cantilever paths |
| 6 | `RackFrameTemplateCatalog.All` (static built-in list, initialized once, never written) | `Application/RackFrames/RackFrameTemplateCatalog.cs:19-21` (`sa3b` F8.5) | **COMPATIBLE** | a consequence of executing the loaded code (type initialization of a constant, input-independent list). The tier-B classification rests on pattern searches (declared limitation of `sa4`) |
| 7 | the static `JsonSerializerOptions` of the stores; the framework's per-type metadata caches and the locking of the options | CORE-17..CORE-19 (CORE-19 `UNCERTAIN`: framework behaviour) | repository options = **executed code**; framework caches = **`WARMUP_STATIC_STATE_RESIDUAL`** (disclosed) | the repository options objects (tier B) are **executed code**; the framework per-type metadata caches and the option locking are **`WARMUP_STATIC_STATE_RESIDUAL`** (disclosed), not "code loaded" |
| 8 | the tier-B statics of **Plugin, Application and Domain** (`KindHandlerRegistry.Default`, read-only delegates and arrays; `SystemRegistry.Default`, `FunctionRegistry.Productive`, `LengthUnits.Authority` and the static read-only sentinels) | CORE-21; `sa4` limitations | **COMPATIBLE**, extended to Application and Domain (D-16) | initialization only, a consequence of executing the loaded code; no run-time write was found |
| 9 | the four tier-A statics of `RackCad.UI` | UI-56 | **COMPATIBLE** (not reached) | the mirror and harness paths open no window; E-11M and E-10 keep detecting a `RackCad.UI` load in the window |
| 10 | control-plane instrument state: I-01 sequence counters, I-15 writers and buffers, the E-11M and E-12 handler instances, their sinks and sequences | this artifact (D-16; V2.1 13.2, 14.2: a sequence gap is `UNKNOWN`) | covered by AR3-16 (E-12 handlers exercised with warm-up-only instances and sinks) | warm-up-only instances and sinks (section 3.1 rows 1e, 1g, 1h), released at the end of step 1; the governed instances start at step 4 with their initial sequence numbers; no counter or buffer carries into the governed logs; `WU-BOUNDARY` checks it (section 7) |

**`WARMUP_STATIC_STATE_RESIDUAL`** (remains disclosed, Owner Act 2 item 15): host-internal static state, including any process state left by `AdjustAlignment`, `RecomputeDimensionBlock` and dynamic-block evaluation on the side database (section 3.1 row 1c); the framework per-type metadata caches and option locking (row 7); and any static state the census cannot enumerate (framework and host caches, AutoCAD's own static state).

## 7. Boundary controls

The 11 conditions of V2.2 PA-5 and the scenario `WU-BOUNDARY`, which checks:

| Check | Acceptance |
|---|---|
| document transaction count (PA-12) | no change of the document database's count |
| document collection (PA-12) | no document created or registered |
| governed subscriptions (PA-12) | no event generated in the E-11M or E-12 governed logs by the warm-up |
| side database (PA-12) | discarded before step 4 |
| governed log sequence (D-16) | the governed E-11M and E-12 logs and the I-15 run log start at their initial sequence number and hold no record written during step 1 |
| warm-up-only objects (D-16) | no governed subscription or writer references a warm-up-only instance or sink |

The `settings.json` rule (section 6 row 4) and the log-directory record (row 3) are control-plane records of every run, not checks of `WU-BOUNDARY`.

## 8. What remains

| Id | Item | Gate |
|---|---|---|
| W-1 | the recorded synthetic plan data of section 3.2 | **before the candidate** of this artifact (part of the warm-up definition, V2.1 2.1); depends on BA-08 V4 section 12 (the `FX-IMP` set waits for K-8) |
| W-2 | the number of dry runs per governing scenario: `WU_DRY_RUNS = N_B` (BA-10 V4 `PARAM-01`; Owner, Q-O2; recorded by the Coordinator; UNSET) | Owner (BA-10 V4); applied by BA-04 V4 |
| W-3 | the manifest capture | execution prerequisite EXEC-11 |
| W-4 | qualification of `WU-BOUNDARY` | host (execution prerequisite) |
| W-5 | the Architect's ruling WU-Q1 on the rows of section 6 | **CLOSED** (AR3-16) |
| W-6 | the static guard over the warm-up harness, the seams, the orchestrator and PREPARE-R (section 6 rows 1, 2, 4) | additional execution prerequisite (BA-08 V4 K-9), not EXEC-12 |
| W-7 | the characterization build (AR3-01); W-7 follows BA-08 V4 K-10 by reference, users included: the build K-10 defines, used by every BA-04 V4 scenario with `BUILD_UNDER_TEST = CHARACTERIZATION_BUILD` (BA-04 V4 sections 1.1 and 1.2), the `WU-DRY` runs of this artifact among them, and by the dynamic-trace dry runs of BA-06 V4 section 7 (non-governing; their use of the AR3-01 mode is part of the trace plan the Architect approves with `STATIC_REVIEWED`, AR3-22); it contains the recording without enforcement of the AR3-01 set and the build-switch writer kinds of BA-04 V4 section 1.2; its use by the governing `EV-L-*` and `Q-I14-*` is pending AQ-V4-01 and by the other governing scenarios pending AQ-V4-06 | execution prerequisite (BA-04 V4 sections 1.1 and 1.2; BA-08 V4 K-10) |
| W-8 | seal of this artifact | baseline review |
| W-9 | the Architect's answer to AQ-V4-02 (whether the branches of a `RECORDED_NOT_ENFORCED` control stay a declared residual of the dry runs, section 4, or must be forced by a harness switch), or its recorded deferral | before the candidate (a ruling that forces the branch changes section 4) |

```text
WARMUP = DRAFT V4 ; NOT EXECUTED ; NO PRODUCTIVE WARM-UP AUTHORIZED ; WU-Q1 = RULED (AR3-16) ; WU_DRY_RUNS = N_B (UNSET, Owner Q-O2)
CT21D_AUTHORITY_BASELINE_READY = FALSE
```

## 9. Delta V4 (decisions section 214)

Sections 1 to 8 keep their numbers; section 3 is split into 3.1 (order of step 1, now normative) and 3.2 (synthetic plans); section 9 is new. The V4 correction pass (rows marked "correction pass") renumbers nothing; it adds the paragraph on the `Q-I14-*` vehicles to section 4 and the item W-9 to section 8.

| Finding id | Decision | Fix (section / field) |
|---|---|---|
| CLOSURE BA09-02 (PARTIALLY_FIXED: universal claim false for 11 scenarios) | AR3-02 | FIXED: section 4 (rule by governance, mode and reach; the 11 named; declared exclusions) |
| D-05 (MAJOR) | AR3-02 | FIXED: section 4: `WU-DRY` for every GOVERNING scenario entering the governed window (P0..CP), the 11 evidence-group scenarios named, the universal V3 sentence withdrawn, learning (`EV-L-*`, reach `LEARN`) declared excluded, dry-run mode of AR3-01; the matching sentence of BA-04 section 5.2 belongs to BA-04 V4 (cross-artifact note) |
| D-09 (MINOR; BA-09 part) | AR3-16 row 2 | FIXED: section 6 row 2 (guard extended to the orchestrator and PREPARE-R; additional execution prerequisite, not EXEC-12); section 8 W-6 |
| D-15 (MINOR) | AR3-16 | FIXED: section 3.1 row 1g (every E-12 handler by direct invocation with warm-up-only instances and sinks; none attached to the side database) |
| D-16 (MINOR) | AR3-16 | FIXED: section 3.1 rows 1e, 1g, 1h, 1k; section 6 rows 8 (Application and Domain tier-B statics) and 10 (instrument state); section 7 (governed logs start at the initial sequence number) |
| D-17 (MINOR) | AR3-30 | FIXED: section 3.1 row 1b (SL-5 import precedes SL-1 and SL-2) and row 1c (claim limited to repository statics; host-internal effects to `WARMUP_STATIC_STATE_RESIDUAL`); section 6 residual paragraph |
| D-18 (MINOR) | AR3-16 row 4 | FIXED: section 6 row 4 (not called; `settings.json` hash before step 1 and at CP; a difference makes the run `INVALID`) |
| D-19 (MINOR) | AR3-24 | NOT_APPLICABLE here (assigned to BA-10 V4 by the Coordinator); the wording of this artifact is aligned anyway: section 4 "Count" and W-2 say `WU_DRY_RUNS = N_B` (Owner, Q-O2; recorded by the Coordinator) |
| WU-Q1 row 1 | AR3-16 | FIXED: section 6 row 1 (NOT COMPATIBLE; no `Load()`; conditional type-initializer warm-up with no entry); section 3.1 row 1j |
| WU-Q1 rows 2, 3, 5, 6, 8, 9 | AR3-16 | FIXED: section 6 rows 2, 3, 5, 6, 8, 9 (COMPATIBLE; row 2 guard extended; row 8 extended to Application and Domain) |
| WU-Q1 row 4 | AR3-16 | FIXED: section 6 row 4 |
| WU-Q1 row 7 | AR3-16 | FIXED: section 6 row 7 (options = executed code; framework caches = disclosed residual) and the residual paragraph |
| BA11V3-02 (edge BA-04 -> BA-09) | AR3-25 | FIXED for this artifact: the header records that BA-04 V4 depends on BA-09; the edge itself is in BA-11 V4 |
| BA11V3-07 (row BA-09) | AR3-25 | FIXED: header `DEPENDS_ON` (registry entries only) and `PREREQUISITES`; section 8 gate column |
| BA11V3-13 (who fixes `WU_DRY_RUNS`) | AR3-24 | FIXED: section 4 "Count"; section 8 W-2 |
| AR3-01 (dry runs in the characterization-build mode) | AR3-01 | FIXED: section 4 "Dry-run mode"; section 8 W-7 |
| D-01..D-04, D-06..D-08, D-10..D-14, BA11V3-01, BA11V3-04, BA11V3-10 | — | NOT_APPLICABLE to BA-09 (BA-08 V4 items) |
| AR3-02 / D-05 (critic: the `Q-I14-*` vehicles fall between the BA-09 and BA-04 dry-run rules) (correction pass) | AR3-02; AQ-V4-01 | FIXED on the BA-09 side: section 4 keeps the rule (`GOVERNING`, `MODE` `CHARACTERIZATION_RUN` or `VALIDATION`, `REACH` in `P0..CP`) with no further qualifier; the exclusions stay non-governing, learning, `REACH = NONE` and `REACH = SYN`; the `Q-I14-*` vehicles (`REACH = CP`) therefore have dry runs, `DRAFT` pending AQ-V4-01 like their parents. BA-04 V4 must list the four dry runs and drop its extra qualifier (cross-artifact) |
| AR3-02 (critic: `PIN_SLOTS` stated as a single slot) (correction pass) | AR3-02 | FIXED: section 4 "Dry-run mode": only `FIXTURE_INSTANCE` slots, the fixture of `X` and, where `X` uses one, its variant library (`FIXTURE_INSTANCE@L-VAR-*`), as BA-04 V4 section 5.2 |
| AR3-01 / AR3-07 (critic: W-7 names only the `WU-DRY` users of the characterization build) (correction pass) | AR3-01 | FIXED: section 8 W-7 aligned with BA-08 V4 K-10 (every BA-04 V4 scenario with `BUILD_UNDER_TEST = CHARACTERIZATION_BUILD`, BA-04 V4 sections 1.1 and 1.2; required switch set; governing `EV-L-*` and `Q-I14-*` pending AQ-V4-01, other governing scenarios pending AQ-V4-06) |
| Architect open questions (round V4 ids) (correction pass) | - | APPLIED: AQ-V4-02 (section 4 declared residual; section 8 W-9; header `PREREQUISITES`), AQ-V4-01 (section 4 `Q-I14-*` paragraph; W-7), AQ-V4-06 (W-7); none is decided here |
| AR3-01 / AR3-22 (W-7 users not aligned with BA-08 V4 K-10 after its correction) (correction pass, minor) | AR3-01; AR3-22 | FIXED: section 8 W-7 follows BA-08 V4 K-10 by reference, users included, so it now also names the dynamic-trace dry runs of BA-06 V4 section 7 (non-governing; their use of the AR3-01 mode is part of the trace plan the Architect approves with `STATIC_REVIEWED`, AR3-22); the switch content and the pending AQ-V4-01 and AQ-V4-06 are unchanged |
