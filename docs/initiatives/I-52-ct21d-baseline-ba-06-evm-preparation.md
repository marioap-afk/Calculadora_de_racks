# I-52 — CT-21D Baseline Artifact BA-06: EVM Preparation (DRAFT V1)

> **BASELINE ARTIFACT BA-06 — DRAFT, NOT HASHED, NOT AGREED. Design / documentation only. Nothing here is executed or authorized.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-EVM-PREPARATION
> ARTIFACT_VERSION          = 1-DRAFT
> ARTIFACT_STATUS           = DRAFT (phase 1 of baseline preparation)
> ARTIFACT_HASH             = TO_BE_RECORDED_AT_SEAL   (see BA-11)
> AUTHORITY_CONTRACT        = CT-21D V2.1 section 27 + V2.2 PA-2, PA-10, PA-11 + V2.3 (AGREED_FOR_BASELINE_ARTIFACT_PREPARATION)
> BLOCKER                   = NB-4  (ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION; OPEN until the inventory, the completeness review and the corpus definitions exist and are agreed)
> INVENTORY_COMPLETENESS    = NOT_ESTABLISHED   (no independent completeness review has been performed)
> LEARNED_EVM_CONTENT       = NONE              (produced later, by authorized characterization execution)
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

This artifact contains five parts required by the phase-1 gate: (1) the Selective seam-operation inventory, (2) the independent completeness-review procedure and result shell, (3) the learning-corpus definition, (4) the validation-corpus definition, (5) the EVM artifact schema. It does **not** claim that the inventory is complete and it contains **no learned event content**.

## 1. Seam-operation inventory (DRAFT, static enumeration)

### 1.1 Provenance

| Field | Value |
|---|---|
| bound to | `origin/main` commit `3375aadbbf929427a6d106b2fff275d64863b89c` (extract of `src/`); the seams do not exist, so the inventory enumerates the **current product creation path** that the seams SL-1, SL-2, SL-4 and SL-5 would reuse in caller-owned form |
| method | static reading of the call graph from the entry points `RACKSELECTIVO` (`RackSelectivoCommands.cs:23-46`) and `RackMenuCommands.cs:76-79` |
| compiled by | the phase-1 implementer with read-only search assistance; **the same party that compiled the inventory may not perform the completeness review** (V2.2 PA-11, condition 5) |
| reviewed by | `UNSET` (section 2) |
| date of compilation | 2026-09-29 |

Paths are relative to `src/`. Abbreviations: `LHD` = `RackCad.Plugin/Drawing/LateralHeaderDrawer.cs`; `SBW` = `RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs`; `BP` = `RackCad.Plugin/Drawing/BlockPlacement.cs`; `BLI` = `RackCad.Plugin/Drawing/BlockLibraryImporter.cs`; `RBD` = `RackCad.Plugin/Systems/Shared/RackBlockData.cs`; `LH` = `RackCad.Plugin/Drawing/LayerHelper.cs`.

### 1.2 Call graph

`DrawSelectiveViewFromAuthored` (`RackSelectivoCommands.cs:449`) dispatches to `SelectivePlantaDrawService.DrawAndPlace` (planta, `:467-470`) or `InsertSelectiveFrontal` (`:484`, frontal via `SelectiveFrontalDrawService.DrawAndPlace`, `:521`). Both services delegate to `ViewBlockDraw.DrawAndPlace` (`Systems/Shared/ViewBlockDraw.cs:28-57`): catalog load, `SystemBlockWriter.CreateBlock` (`SBW:18-40`), then `BlockPlacement.PlaceAndReport` (`BP:26-53`).
`SBW.CreateBlock`: `LockDocument` (`SBW:23`) → `BlockLibraryImporter.EnsureForPlan` (`SBW:25`, outside any transaction) → `StartTransaction` (`SBW:27`) → `LHD.CreateSystemBlock` (`SBW:29`) → `RackBlockData.Write` (`SBW:33`) → `Commit` (`SBW:36`). The plan builders `SelectiveFrontalBuilder.BuildPlan` and `SelectivePlantaBuilder.BuildPlan` are in the Application layer and are pure (no Autodesk reference).

### 1.3 Database-mutating operations

`Context today` says where the operation runs in the current product; `In the seam` says what a caller-owned seam would do with it (the seam receives the caller's `Database` and `Transaction`; it never locks, starts, commits, aborts or disposes).

| Op | Category | API call and location | Reached by | Seam role | Context today | In the seam |
|---|---|---|---|---|---|---|
| OP-01 | open block table for write | `tr.GetObject(db.BlockTableId, OpenMode.ForWrite)` (`LHD:31`) | frontal, planta | SL-1, SL-2 | inside TX-A | kept, on the caller transaction |
| OP-02 | create block table record and add it | `new BlockTableRecord {Name, Origin}` (`LHD:243`); `blockTable.Add` (`LHD:244`); `AddNewlyCreatedDBObject` (`LHD:245`) in `NewBlock` (`LHD:240-247`); called per header group (`LHD:40`) and once for the system block (`LHD:54`) | frontal, planta | SL-1, SL-2 | inside TX-A | kept; the effective name comes from the naming policy computed in PREPARE-R |
| OP-03 | block-reference append, header nesting | `new BlockReference(...)`, `systemDef.AppendEntity`, `AddNewlyCreatedDBObject` (`LHD:58-67`) | frontal, planta | SL-1, SL-2 | inside TX-A | kept |
| OP-04 | block-reference append, piece instances | `new BlockReference(pt, definitionId)`, `space.AppendEntity`, `AddNewlyCreatedDBObject` (`LHD:308-317`, in `AppendInstance` called at `LHD:44` and `LHD:72`); a missing block is skipped and recorded (`LHD:293-303`) | frontal, planta | SL-1, SL-2 | inside TX-A | kept; a missing block becomes a typed failure (contract 15.4), not a silent skip |
| OP-05 | entity append, text | `new DBText`, `space.AppendEntity`, `AddNewlyCreatedDBObject` (`LHD:266-279`) | frontal, planta | SL-1, SL-2 | inside TX-A | kept |
| OP-06 | entity property modification | `label.AdjustAlignment(space.Database)` (`LHD:280`) | frontal, planta | SL-1, SL-2 | inside TX-A | kept |
| OP-07 | entity append, dimension | `new RotatedDimension(...)`, `AppendEntity`, `AddNewlyCreatedDBObject` (`LHD:354-360`, in `AppendDimension`, called at `LHD:289`) | frontal, planta | SL-1, SL-2 | inside TX-A | kept |
| OP-08 | entity property modification, dimension | `Dimscale`, `Dimtxt`, `Dimasz`, `Dimexe`, `Dimexo`, `Dimgap`, `Dimtad`, `Dimdec` (`LHD:365-372`); `RecomputeDimensionBlock(true)` (`LHD:375`); skipped when a named `DimensionStyleName` exists in the drawing (`LHD:362`) | frontal, planta | SL-1, SL-2 | inside TX-A | kept; the host creates an anonymous `*D` block (invisible to the code) |
| OP-09 | dynamic-property set | `property.Value = value` in `ApplyDynamicParameters` (`LHD:415-449`, set at `LHD:436`; called from `LHD:319`); names match case-insensitively, read-only skipped, values not found silently dropped | frontal, planta | SL-1, SL-2 | inside TX-A | kept; the silent drop of a missing parameter must become a comparator-visible fact |
| OP-10 | `RecordGraphicsModified` | `reference.RecordGraphicsModified(true)` (`LHD:447`), only when a value was applied | frontal, planta | SL-1, SL-2 | inside TX-A | kept |
| OP-11 | add symbol-table record, layer | `LH.EnsureLayer`: layer table `UpgradeOpen` (`LH:24`), `new LayerTableRecord {Name, Color}` (`LH:25`), `layerTable.Add` (`LH:26`), `AddNewlyCreatedDBObject` (`LH:27`); callers `LHD:270` (`RACKCAD_ANOTACIONES`, ACI 2) and `LHD:356` (`RACKCAD_COTAS`, ACI 1); created lazily and only if absent | frontal, planta | SL-1, SL-2 | inside TX-A | kept; the layers are part of the closure of what the seam itself may add |
| OP-12 | extension dictionary creation | `entity.CreateExtensionDictionary()` (`RBD:29-30`, guarded by `IsNull`), on the block **definition** | frontal, planta | SL-1, SL-2 | `SBW:33`, inside TX-A | kept |
| OP-13 | `Xrecord` data write | `new ResultBuffer` in chunks of at most 255 characters, `new Xrecord {Data}` (`RBD:35-48`), key `RACKCAD_SELECTIVE` (`RBD:19`) | frontal, planta | SL-1, SL-2 | inside TX-A | kept; the payload is composed by the caller and written verbatim (contract 15.3) |
| OP-14 | dictionary entry write | `dictionary.SetAt(DictKey, xrecord)` (`RBD:49`); `AddNewlyCreatedDBObject(xrecord, true)` (`RBD:50`); the overwrite branch `existing.Data = buffer` (`RBD:44`) exists in the code | frontal, planta | SL-1, SL-2 | inside TX-A | kept; the overwrite branch is not expected on a fresh definition (inference, not proven) |
| OP-15 | deep-clone import | `source.WblockCloneObjects(ids, db.BlockTableId, mapping, DuplicateRecordCloning.Ignore, deferTranslation: false)` (`BLI:123`); silent 0 when the file or database is missing, no requested name exists, or on exception (`BLI:88-90`, `96-100`, `117-120`, `126-131`) | frontal, planta | SL-5 | inside the document lock, **outside any transaction** (`SBW:23-39`) | replaced by an import that returns a structured result, takes the opened `Database` of the private copy, and never a path to the original |
| OP-16 | side database read | `new Database(false, true)`, `ReadDwgFile(path, OpenForReadAndAllShare, ...)` (`Drawing/BlockLibraryDatabaseCache.cs:33-40`); one static cache slot keyed by path, last-write time and length | frontal, planta | SL-5 | in the import step | replaced by the private-copy authority (contract 19.1); the static cache is **not** used by the mirror path |
| OP-17 | reference append in model space | `modelSpace.AppendEntity(reference)`, `AddNewlyCreatedDBObject` (`BP:193-198`) on the model space id from `SymbolUtilityServices.GetBlockModelSpaceId` (`BP:193-194`); no layer, scale, rotation, attribute or dictionary is set on the reference | frontal, planta | SL-4 | TX-E under a separate `LockDocument` (`BP:179`) | non-jig: the position comes from the computed transform; sets what the plan says; applies `RecordGraphicsModified` inside `T_M` (contract 15.6) |

### 1.4 Operations that exist today and are **not** in the mirror path

| Op | What | Location | Why not in the seam |
|---|---|---|---|
| NX-01 | interactive jig `editor.Drag(jig)` and `AcquirePoint` | `BP:184`, `BP:224` | the mirror uses a computed transform, no interaction |
| NX-02 | cancel-path cleanup: `EraseUnreferencedDefinition` (`BP:123-170`) erases the unreferenced definition, and `PurgeUnreferenced` (`LHD:193-238`, called at `BP:162`) purges nested definitions | `BP:41`, `BP:162` | the mirror aborts `T_M`; it never erases after commit |
| NX-03 | edit/redefine family: `LHD.RedefineSystemBlock` (`LHD:88-182`), `SBW.RedrawInPlace` (`SBW:45-80`), `SBW.RedefineInTransaction` (`SBW:104-116`), `ViewBlockDraw.PrepareRedraw` (`ViewBlockDraw.cs:65-88`) | | edit path; **precedent patterns** for a caller-owned form, not creation |
| NX-04 | `RackBlockRenamer.SyncName`, `RackCommandSupport.EraseViewBlocks` | `Drawing/RackBlockRenamer.cs:21-58`, `RackCommandSupport.cs:218-` | edit path |
| NX-05 | filesystem writes: `RackLog` appends to `%AppData%\RackCad\logs` (`Application/Diagnostics/RackLog.cs:46-48`); `CorruptFile.Quarantine` moves a corrupt settings file (`Application/Diagnostics/CorruptFile.cs:28,31`, reached through `Application/Settings/UserSettings.cs:111`) | | not database mutations; they are **side effects the control plane must know** (warm-up static state, BA-09) |
| NX-06 | `BlockPlacement.AppendReference` (non-jig, own lock and transaction) | `BP:79-95`; caller `Drawing/LateralHeaderDrawService.cs:182` | lateral only; out of the Selective scope (BA-03) |
| NX-07 | `RackDefinitionCreator` (caller-transaction creation helper, writes `RackBlockData.Write` at `Systems/Shared/RackDefinitionCreator.cs:169`) | | AUTH-15 precedent for HeaderRun and Cantilever; not reached by the Selective entry points |

### 1.5 Transaction and lock sites on the path (for E-06 sampling)

| Site | Where | Nature |
|---|---|---|
| TX-A | `SBW:27` | writes (all of OP-01..OP-14); `Commit` at `SBW:36` |
| TX-B | `BLI:68` | read-only, `blockTable.Has`; `Commit` at `BLI:79` |
| TX-C | `BLI:103` | read-only on the side database; `Commit` at `BLI:114` |
| TX-D | `Drawing/AutoCadLibraryBlockQuery.cs:24` | read-only query after the import; `Commit` at `:36` |
| TX-E | `BP:180` | writes (OP-17); `Commit` at `BP:189` or `BP:198` |
| locks | `SBW:23`, `BP:83`, `BP:135`, `BP:179` | parameterless `LockDocument()`; the default mode is not visible in the code |

No `StartOpenCloseTransaction`, no `Abort()` call and no `DocumentLockMode` argument exists on these paths. The seams and the orchestrator replace TX-A/TX-E by the single caller-owned `T_M`.

### 1.6 Limitations of the static enumeration (declared)

1. **Host-internal secondary operations are invisible to the code**: setting a dynamic-property value or `RecordGraphicsModified` may make the host create anonymous `*U` block records; `RecomputeDimensionBlock` and the dimension append make the host create or update an anonymous `*D` block; `WblockCloneObjects` may clone dependent objects (layers, linetypes, text styles, dimension styles, nested blocks and their extension dictionaries) whose set depends on the library file contents; `DuplicateRecordCloning.Ignore` skips existing same-named records; `AddNewlyCreatedDBObject` and `Commit` can fire host database events (no `ObjectAppended`, `ObjectModified` or `CommandEnded` handler was found in the extract, but other loaded assemblies could subscribe).
2. **Delegate dispatch**: the plan and name delegates of `ViewBlockDraw` are bound only in the two draw services; the edit path binds `RedrawInTransaction` delegates in `ProjectVariableMutationExecutor.cs:243-256`.
3. **Interface dispatch**: `ILibraryBlockImporter.Ensure` and `ILibraryBlockQuery.Query` (`Application/Systems/Shared/LibraryBlockRequirements.cs:175-178`) resolve to `ImportAdapter` and `AutoCadLibraryBlockQuery`; `LibraryPieceRequirementsV2.cs:341` takes an `ILibraryBlockImporter` whose supplier was not traced.
4. **Reflection**: `RackEmbedComposer.cs:37-39` and `CsvCatalogReader.cs:38-39` reflect over properties without database effect; command discovery uses `[CommandMethod]` attributes.
5. **Data-dependent branches** that change which primitives run: missing blocks are skipped; annotations, dimensions and dynamic parameters depend on the plan; the named dimension style branch; the layer-exists check; library-file existence; the `payloadJson` emptiness check.
6. **Unverified**: the `LockDocument()` default mode; the outcome of `Database.Purge` for library-imported definitions on the cancel path; `HeaderInstanceGrouper` and `RackViewBaseName` bodies were not read, so the exact set of nested definition names is not established here.
7. **Unknown contents**: `blocks.csv` (catalog block names) and `blocks-library.dwg` are not in the repository (V17 L-28); the library census (BA-05 item L-1) is required.

## 2. Independent completeness review: procedure and result shell

### 2.1 Procedure (V2.2 PA-11; all seven conditions are mandatory)

| # | Condition | How this procedure meets it |
|---|---|---|
| 1 | static enumeration bound to the exact source SHA | section 1 is bound to `3375aadb...`; the review re-derives it on the SHA of the build under evaluation |
| 2 | limits of reflection and dynamic dispatch declared | section 1.6 items 2 to 4 |
| 3 | dynamic trace covers every seam entry point | trace matrix, section 2.2 |
| 4 | dynamic trace covers all plan-shape classes | section 4.2 lists the classes; the matrix maps each |
| 5 | the reviewer did not compile or create the inventory | reviewer identity recorded in section 2.3; the compiler of section 1 is not eligible |
| 6 | differences between the static and the dynamic source are resolved explicitly | a difference log (section 2.3) with a resolution for every primitive found by only one source |
| 7 | facts seen by neither source remain residual and surface as unexplained events in learning | section 2.4 |

### 2.2 Dynamic-trace plan (candidate methods; the reviewer fixes the method)

| Method | What it sees | Limitation |
|---|---|---|
| T-1 database event log (`ObjectAppended`, `ObjectModified`, `ObjectErased`, related categories) during **non-governing dry runs** on a scratch document | effects on database objects | events show effects, not calls; requires the E-12 subscription instruments (I-10) |
| T-2 transaction-manager events | start and end of transactions (E-06 sampling sites) | does not show writes |
| T-3 external managed method-entry tracing of `Autodesk.*` calls (CLR event tracing from outside the process) | calls issued by the product | tooling does not exist today; must not load code into the governed window; not usable in a governing run |
| T-4 difference of full-database state before and after (exact fingerprint of the scratch document) | net effect | hides transient operations |

Coverage matrix to be completed by the reviewer: rows = seam entry points (SL-1 frontal creation, SL-2 planta creation, SL-4 reference placement, SL-5 import, SL-6 read-only verifier); columns = plan-shape classes of section 4.2; cell = the trace evidence reference.

### 2.3 Result shell (all fields `UNSET`)

| Field | Value |
|---|---|
| reviewer (name, role) | `UNSET` |
| review date | `UNSET` |
| source SHA of the build reviewed | `UNSET` |
| static enumeration result (primitives found) | `UNSET` |
| dynamic-trace method chosen and evidence | `UNSET` |
| coverage matrix (entry points by shape classes) | `UNSET` |
| difference log (primitive, found by static / dynamic, resolution) | `UNSET` |
| residual facts (seen by neither source) | `UNSET` |
| conclusion | `INVENTORY_COMPLETENESS = NOT_ESTABLISHED` until this shell is filled and accepted |

### 2.4 Residual facts

A fact seen by neither source is recorded as a residual. During learning and validation it surfaces as an **unexplained event** (`UNEXPLAINED_EVENTS`), which **blocks the freeze** of the model (V2.1 27.2 step 4) and is never admitted automatically.

## 3. Learning-corpus definition

The learning corpus is the set of **micro-scenarios**, their fixtures and the sessions that ran them, sealed by hash (V2.1 27.3). Each operation of the inventory is executed **in isolation** in the controlled environment (manifest-attested, no adversarial probes, in a scratch document) to map it to host events, over **several isolated repetitions across independent sessions** (the number is a classified parameter, BA-10).

| Learning scenario class | Operation(s) isolated | Notes |
|---|---|---|
| `EV-L-OP01` | OP-01 | open block table for write |
| `EV-L-OP02` | OP-02 | create and add a block table record |
| `EV-L-OP03-04` | OP-03, OP-04 | block-reference append inside a definition (header nesting; piece instance) |
| `EV-L-OP05-06` | OP-05, OP-06 | text append and alignment |
| `EV-L-OP07-08` | OP-07, OP-08 | dimension append and property modification, including the `*D` block |
| `EV-L-OP09-10` | OP-09, OP-10 | dynamic-property set and `RecordGraphicsModified`, including any `*U` block |
| `EV-L-OP11` | OP-11 | layer creation |
| `EV-L-OP12-14` | OP-12, OP-13, OP-14 | extension dictionary, `Xrecord` data, dictionary entry |
| `EV-L-OP15` | OP-15 | deep-clone import (into a scratch document; PREPARE phase, log only) |
| `EV-L-OP17` | OP-17 | reference append in model space |
| `EV-L-COMPOSED` | OP-01..OP-14 in the order of one view creation | to check that the composition of isolated entries explains the composed sequence |

Each class needs a fixture (a scratch template and, for OP-04/OP-09/OP-15, the library copy); the fixtures are `TO_BE_DEFINED` (BA-08). Learning results are **not** written in this artifact.

## 4. Validation-corpus definition

### 4.1 Rules

- **Disjoint** from the learning corpus: different sessions and fresh fixtures (V2.1 27.3); no run teaches and validates.
- Validation runs are **governing** only against a `FROZEN(hash)` model (V2.2 PA-2); without one, `E12-V = NOT_EVALUATED`.
- The validation plans must include plans **structurally outside** the learning corpus (section 4.3).

### 4.2 Plan-shape classes (candidates; the kind baseline fixes the final list)

| Shape class | Description |
|---|---|
| `SH-F1` | one frontal view of a single-fondo rack |
| `SH-FN` | one frontal view of a multi-fondo rack (fondo count from 2 to `MaxDepthCount = 4`, `Domain/Systems/Selective/SelectiveRackDefaults.cs:40`) |
| `SH-FALL` | all frontal views of a multi-fondo rack (one per fondo) |
| `SH-P` | the planta view |
| `SH-FP` | frontal view(s) and planta together (one logical rack) |
| `SH-ANN`, `SH-NOANN` | with and without annotations (frente numbers, level numbers, rack name) |
| `SH-DIM`, `SH-NODIM` | with and without dimensions |
| `SH-IMP`, `SH-NOIMP` | library import needed (block absent) and not needed (block present) |
| `SH-LAY`, `SH-NOLAY` | annotation/dimension layers absent (created) and present (reused) |

### 4.3 `STRUCTURALLY_OUTSIDE_LEARNING_CORPUS` (V2.1 27.8, operational form)

A validation plan is structurally outside the learning corpus when, for at least one dimension of the plan quantity vector (section 5), its value is not in the set of values seen in learning (an unseen multiplicity, including one above the maximum seen), or its shape class is not represented in learning. The validation corpus must contain at least one plan of each kind where the sealed scope allows it (a multi-fondo plan with a fondo count above the largest seen in learning is an example).

## 5. `f(plan)` and the plan quantity vector

`f(plan)` is the multiset union over the ordered list `Ops(plan)` of seam operation instances derived from the plan (V2.1 27.9). Candidate **plan quantity dimensions** observed in the current code (the kind baseline fixes the final vector):

| Quantity | Source |
|---|---|
| number of frontal views (fondos, `SelectiveDepthLayout.Count`) | `Application/Systems/Selective/SelectiveDepthLayout.cs:18` |
| number of planta views (one per rack) | `RackSelectivoCommands.cs:233-244` |
| number of nested header groups (`SEL_FRONTAL_<Role>_<n>`; `PLANTA_CAB_<n>`) | `Application/Systems/Selective/SelectiveFrontalBuilder.cs:26,36`; `Application/Drawing/HeaderInstanceGrouper.cs`; `Application/Systems/Selective/SelectivePlantaBuilder.cs:300` |
| number of loose piece instances and nested placements | `LHD:44`, `LHD:58-67`, `LHD:72` |
| number of `DBText` annotations | `Application/Systems/Selective/SelectiveAnnotations.cs` |
| number of `RotatedDimension` entities | `Application/Systems/Selective/SelectiveDimensions.cs:60` (frontal), `:205` (planta) |
| number of dynamic-property applications | `LHD:415-449` |
| number of distinct library blocks required | `Application/Systems/Shared/LibraryBlockRequirements.cs:53-80` |
| number of layers created (0 to 2) | `LH.EnsureLayer` callers `LHD:270`, `LHD:356` |
| envelope size (payload chunks of at most 255 characters) | `RBD:35-39` |

Each multiplicity function must be affine in the declared quantities or a table validated over structurally distinct shapes (V2.1 27.9).

## 6. EVM artifact schema (from V2.1 27.6)

| Field | Content |
|---|---|
| `EVM_ID`, `VERSION`, `HASH` | identity of the model and the hash of its canonical serialization |
| `LEARNING_CORPUS_HASH` | hash of the sealed learning corpus |
| `ENTRY_ID` | identity of one entry |
| `OPERATION_CLASS` | the inventory operation category (section 1.3) |
| `EXPECTED_EVENT_SEQUENCE/SET` | multiset of (event kind, target selector, step) with the multiplicity as a function of the plan quantities, and the partial order if any |
| `EXPLANATION_TYPE` | `PRODUCT_OPERATION`, `HOST_DOCUMENTED` or `HOST_OBSERVED_STABLE` |
| `AUTHORITY/CITATION` | the product operation, the host documentation citation, or the micro-scenario identities that show a companion behaviour |
| `RAW_EVIDENCE_REF` | reference to the raw event logs |
| `RISK_LABEL` | `NONE`, or the declared risk (mandatory for `HOST_OBSERVED_STABLE`) |
| `REVIEW_STATUS` | `DRAFT`, `UNDER_REVIEW`, `ACCEPTED` or `REJECTED`, with the reviewer role |

`HOST_OBSERVED_STABLE` may appear **only** as accompanying host behaviour of a product operation, with raw evidence, a citation to isolated micro-scenarios stable across independent sessions, an Architect review and a risk label; it is **never** a free-standing permission. `EVM_STATUS` is `DRAFT`, `UNDER_REVIEW` or `FROZEN(hash)`; only `FROZEN` models are used by governing validation.

## 7. What is still missing before NB-4 can close

| Id | Item |
|---|---|
| N4-1 | the independent completeness review (section 2) performed by a non-compiler, and its result accepted |
| N4-2 | the fixtures for the learning and validation scenario classes (BA-08) |
| N4-3 | the final plan quantity vector and the final shape-class list (BA-08) |
| N4-4 | the classified number of learning repetitions and determinism repetitions (BA-10) |
| N4-5 | the hash and the Coordinator and Architect agreement of this artifact |

```text
NB-4 = ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION ; OPEN
INVENTORY_COMPLETENESS = NOT_ESTABLISHED
LEARNED_EVM_CONTENT = NONE
CT21D_AUTHORITY_BASELINE_READY = FALSE
```
