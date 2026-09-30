# I-52 — CT-21D Baseline Artifact BA-08: Selective Kind Authority Baseline (DRAFT V2)

> **BASELINE ARTIFACT BA-08 V2 — DRAFT, NOT SEALED. Design / documentation only. Nothing here is executed or authorized. No evidence-derived value is selected.**
>
> ```text
> ARTIFACT_ID               = SELECTIVE_KIND_AUTHORITY_BASELINE   (baseline artifact CT21D-BASE-SELECTIVE-KIND)
> ARTIFACT_VERSION          = 2-DRAFT   (supersedes 1-DRAFT, blob 8dbce38a3e1733d3c93fcfd37cad03abcd7041af, which stays as history)
> ARTIFACT_STATUS           = DRAFT (phase 2)     CANDIDATE_HASH = UNSET     SEAL_HASH = UNSET
> AUTHORITY_CONTRACT        = CT-21D V2.1 3.3, 3.4, 10, 11, 15 + V2.2 + V2.3 + V2.4
> CODE FACTS BOUND TO       = origin/main 3375aadbbf929427a6d106b2fff275d64863b89c (evidence docs/automation/evidence/I-52-ct21d-phase2-static-analysis/)
> PHASE-1 RULING APPLIED    = parameterized fixtures; ORACLE_SPEC; tolerance mapping; designated E-04 route R-1; quantity vector and shape classes;
>                             dependency-closure rule; census schema; sealed-list coverage; blocks.csv fact corrected; LOCK_MODE candidates without a winner
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

Paths are relative to `src/` of the bound commit unless stated. **FACT** = seen in the code; **UNKNOWN** = the code does not determine it. Abbreviations: `LHD` = `RackCad.Plugin/Drawing/LateralHeaderDrawer.cs`; `SBW` = `RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs`; `BP` = `RackCad.Plugin/Drawing/BlockPlacement.cs`; `RBD` = `RackCad.Plugin/Systems/Shared/RackBlockData.cs`; `BLI` = `RackCad.Plugin/Drawing/BlockLibraryImporter.cs`; `RDC` = `RackCad.Plugin/Systems/Shared/RackDefinitionCreator.cs`.

## 1. Scope linkage

The kind covers exactly the scope of BA-03 V2: `SEL-FRONTAL` (one view per fondo `k`, `0 <= k < SelectiveDepthLayout.Count(system)`) and `SEL-PLANTA` (one whole-rack view). Lateral and `Section = -1` are out. `Count(system) = Math.Max(1, system?.DepthCount ?? 1)` (`Application/Systems/Selective/SelectiveDepthLayout.cs:18`); `MaxDepthCount = 4` (`Domain/Systems/Selective/SelectiveRackDefaults.cs:40`).

## 2. Seam authority references SL-1..SL-6

| Seam | Role | Required by scope | Current-code precedent | Status |
|---|---|---|---|---|
| SL-1 | frontal view definition (one per fondo) in the caller's transaction | required | the product path creates it in its **own** transaction (`SBW:18-40`); **AUTH-15 overload A** (`RDC`, `CreateInTransaction(Database, Transaction, LateralHeaderDrawer, HeaderRunPlan, string, RackEmbedDocument)`) runs **the same drawer method** `LHD.CreateSystemBlock` in the caller's verified top transaction, with an ordinal envelope read-back and a typed `MissingLibraryBlocks` failure, and has **no production caller** (evidence `B6` F1.02..F1.15) | not implemented as a mirror seam; implementation option: AUTH-15 overload A |
| SL-2 | planta view definition in the caller's transaction | required | as SL-1 (the planta plan is also a `HeaderRunPlan`, B6 F1.14) | as SL-1 |
| SL-3 | lateral definition, own Selective contract | **not required** | lateral path `LateralHeaderDrawService` | equivalence EQ-1..EQ-5 `NOT_DEMONSTRATED` |
| SL-4 | top-level reference in the caller's transaction from a computed transform | required | jig placement in its own lock and transaction (`BP:174-201`); sets only `Position` | missing |
| SL-5 | PREPARE-W import with a structured result, from the opened private-copy `Database` | required | `BLI.EnsureForPlan` returns an `int`; opens the shared library through a static cache; swallows failures | missing |
| SL-6 | read-only verifier and expected-versus-persisted comparator | required | readers exist (`RackBlockData.Read`, `RackBlockFinder.ScanEnvelopes`, `RackViewCodec`); no expected-versus-persisted comparator | missing |

**Contract fact discrepancy (PH2-CF-1, for the Architect).** V2.1 section 15.1 fact F-2 states that "frontal and planta have **no** caller-owned seam". At the bound commit this is **incomplete**: AUTH-15 overload A accepts any `HeaderRunPlan` and the Selective frontal and planta plans are `HeaderRunPlan`s, so a caller-owned creation primitive **exists** for them (unused). The design clauses of V2.1 15.3 are unchanged by this fact; the implementation prerequisite P-1 becomes: "use AUTH-15 overload A (or an equivalent) for SL-1 and SL-2, and resolve the differences listed below". **No AUTH-15 evidence transfers** (P-8 remains required). Differences between the product path and AUTH-15 overload A (evidence `B6` T2):

| Aspect | Product path | AUTH-15 overload A | Mirror requirement |
|---|---|---|---|
| import | inside the creation callee, before its transaction (`SBW:25`) | none | PREPARE-W (SL-5), before `T_M` |
| missing pieces | skipped silently, envelope still written (`LHD:293-303`) | typed `MissingLibraryBlocks`, no envelope | typed failure (V2.1 15.3, 15.4) |
| envelope | written only if non-empty, no read-back (`SBW:31-34`) | written, read back, ordinal compare | verbatim write and read-back (R-07, R-08) |
| naming | `RackViewBaseName` + `UniqueBlockName` | requested name passed to `UniqueBlockName`; blank refused | effective names computed in PREPARE-R and returned (R-06) |
| transaction | own `StartTransaction`, `Commit` | caller's top transaction verified by `UnmanagedObject` | typed authority `IsTopTransaction` (V2.2 RC-8) |
| placement | jig, own lock and transaction | none | SL-4 in `T_M` |

## 3. What creating a Selective view produces (FACT unless marked)

| Item | Fact | Evidence |
|---|---|---|
| definition names | frontal: `LinkedSelectiveFrontal(name, fondo, fondoCount)` then `Standard`; planta: `name + " - planta"` or `"Selectivo planta - {N} frentes"`; `BlockNaming.SanitizeBlockName`; uniquified by `_n` suffixes against the live block table; never renames another block | `Application/Systems/Shared/RackViewBaseName.cs:22-37,186-191,202`; `Application/BlockNaming.cs:12-29`; `LHD:395-413` |
| nested definitions | frontal `SEL_FRONTAL_<Role>_<n>` (pieces occurring at least twice); planta `PLANTA_CAB_<n>`; created before the view definition; names restart per plan | `LHD:36-54`; `Application/Drawing/HeaderInstanceGrouper.cs`; `Application/Systems/Selective/SelectivePlantaBuilder.cs:300` |
| entities | block references (pieces, nested placements), `DBText`, `RotatedDimension`; no attributes; negative scale only on nested piece references | `LHD:252-376`; evidence `critic_ops` F-24 |
| **catalog `blocks.csv`** | **EXISTS in the repository** (`assets/catalogs/blocks.csv`, 94 rows: 30 FRONTAL, 35 LATERAL, 29 PLANTA); copied to the build output `catalogs\` and resolved next to `RackCad.Application.dll` at run time; the runtime copy **can differ** from the repository file (live-edit re-read on change) | evidence `B3` F01, F24..F26; `B7` P7, L6 |
| library drawing `blocks-library.dwg` | **not versioned** in the repository; path from user settings, else next to the catalogs | evidence `B3` F27; `Application/Catalogs/BlockLibrary.cs:11-24` |
| dynamic-block parameters | `LONGITUD`, `PERALTE`, `ALTURA`, `SAQUE`, `FRENTE`, `FONDO` on nested references only; names matched case-insensitively; missing names silently ignored | evidence `B3` T3, F23 |
| layers | `RACKCAD_ANOTACIONES` (ACI 2) and `RACKCAD_COTAS` (ACI 1), created lazily in the same transaction | `LHD:270,324-331,356`; `RackCad.Plugin/LayerHelper.cs:16-28` |
| envelope | `RackEmbedDocument` in an `Xrecord` under `RACKCAD_SELECTIVE` in the extension dictionary of the **definition**; chunks of at most 255 characters; no XData, no NOD | `RBD:16-52`; `Application/Persistence/RackEmbedDocument.cs:14-76` |
| NOD / project variables | none written by creation | evidence `B6` F2.17; `RackSelectivoCommands.cs:94-100,298-347` |

**Tuple consequence.** Because the runtime catalog folder is build-bound content that can change, the **catalog folder files** (`catalogs\*.csv`, `*.json`) must be pinned by SHA-256 in the BUILD_BOUND tuple together with the library file (finding PH2-TP-1; the V2.1 2.1 tuple lists the library file but not the catalogs).

## 4. Sealed list (`VERIFY_ELEMENT_LIST`) and its coverage

| Class | Element | Layer | Covered by scenarios (BA-04 V2) |
|---|---|---|---|
| SL-D | each destination view definition | SEM | `CL-CLEAN-*`, `WR-A-*`, `WR-E-*` |
| SL-N | each nested definition the seam creates | SEM | `CL-CLEAN-FX2F-ALL`, `CL-CLEAN-FX4F-ALL`, `WR-E-*` |
| SL-R | each top-level reference (name, position, scale, rotation, normal, layer) | SEM | `CL-CLEAN-*`, `KS-SL4-NEG-DET` |
| SL-E | each envelope, including `ExtensionData` and unknown members | SEM | `CL-CLEAN-*`, `WR-A-*` (payload change) |
| SL-A | view address and grouping (`Embed.Id` = `NewRackId`) | SEM | `CL-CLEAN-FX1F-FP`, `CL-CLEAN-FX2F-ALL`, `WR-D-*` (unexpected sibling) |
| SL-Y | layers added by the seam | SEM | `CL-CLEAN-FXANN-FP`, `CL-CLEAN-FXANN-LAY-FP`, `CL-CLEAN-FXDIM-FP` |
| SL-S | source read-set | EXA | `WR-G-*`, `NC-E08-Y0`, `NC-E08-Y2` |
| SL-P | project-variable entries consumed (`RACKCAD_PROJECT`) | EXA | `WR-F-*` |
| SL-C | containers (block table names, model-space handles, sibling set, extension dictionaries) | EXA | `WR-D-*`, `WR-E-*` |

Every class has at least one concrete draft scenario; none is sealed.

## 5. `PROTECTED_OBJECTS` and the phase-A domain (V2.2 PA-10)

Phase A adjudicates events on (1) the plan-time protected set (SL-S, SL-P, the pre-existing containers of SL-C) and (2) the objects created inside `T_M` (the handles returned by the seams for SL-D, SL-N, SL-R, SL-E, SL-Y). At PV the sealed `PROTECTED_OBJECTS` is checked for consistency with that domain; events outside it are recorded and not adjudicated. Anonymous blocks derived by the host (`*U`, `*D`) are outside the definitions list and inside the domain only through their owning reference or dimension.

## 6. Abort read-set: component specifications (readers are execution blockers)

The specification of each component is **fixed here**; the readers and comparators marked MISSING are **execution blockers** (EXEC-4), not baseline blockers.

| Component | Compared value | Layer | Scope | Reader status |
|---|---|---|---|---|
| block table names | the full name set (stored case) | EXA (`ENUMERATION`, sorted) | the whole block table | MISSING |
| definitions | `DEFINITION_DUMP` of every RackCad-owned definition, every manifest definition and every pre-existing homologue of the static closure | EXA | those definitions | MISSING (fingerprint reader) |
| model-space content | the handle set and the RackCad reference set with class | EXA (`ENUMERATION`, session-local) | model space | MISSING |
| RackCad payloads | `PAYLOAD_EXACT` of every definition payload; set of `RackId` (expected: no new id) | EXA | every definition that `ScanEnvelopes` reaches; layouts, anonymous and xref-owned records are **outside** (declared limit) | reader exists (`RBD.Read`, `RackBlockFinder.ScanEnvelopes`); aggregate comparator MISSING |
| view addresses and grouping | decoded address and `Embed.Id` per view | EXA | as above | codec exists (`RackViewCodec`); host reader MISSING |
| library residue | the ADD set with its closure | EXA | manifest | MISSING |
| symbol tables | name sets of layers, linetypes, text styles, dimension styles, registered applications; `SYMBOL_RECORD` of each pre-existing record of the closure and of `RACKCAD_ANOTACIONES`, `RACKCAD_COTAS`, the dimension style in effect and the text style in effect | EXA | those tables | MISSING |
| project variables | `NOD_ENTRY` of `RACKCAD_PROJECT` (and `RACKCAD_CUSTOM_PROPERTIES` if present) | EXA | the NOD entries named | readers exist (`ProjectVariablesData.Read`, `CustomPropertiesData.Read`, caller transaction); aggregate MISSING |

## 7. Static dependency closure and the required-block set

**Required-block set (static derivation, evidence `B3`).** With the repository catalogs, a Selective **FRONTAL** plan can require **23** distinct block names and a **PLANTA** plan **23** (disjoint; union 46), listed in evidence `B3` table T2. A **clean fixture** (section 12) requires only:

| Plan | Blocks required by a clean fixture |
|---|---|
| FRONTAL | `POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA_FRONTAL`; `PLACA_BASE_DE_CABECERA_ATORNILLABLE_DE_PLACA_CALIBRE_3_16_FRONTAL` (with `DrawBasePlate`); the chosen beam's `LARGUERO_*_FRONTAL` |
| PLANTA | `POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA_PLANTA`; `PLACA_BASE_DE_CABECERA_ATORNILLABLE_DE_PLACA_CALIBRE_3_16_PLANTA`; `TRAVESANO_PARA_POSTE_OMEGA_DE_CINTA_CALIBRE_14_PLANTA`; the first level's `LARGUERO_*_PLANTA`; `SEPARADOR_DE_CABECERA_FORMADA_DE_CINTA_CALIBRE_12_PLANTA` when `DepthCount >= 2` with a positive gap |

**Closure rule (normative for the kind).** In PREPARE-R, from the private library copy, read-only: for every required block name, collect the block definition, the transitive closure of its nested definitions, and the symbol records they reference; classify each member `ADD`, `REUSE_AS_IS`, `REUSED_BOUND` or `REJECT` with host symbol-table semantics and stored case (V2.1 11.3, V2.2 PA-7); an unclassifiable member makes the closure `UNKNOWN` and refuses the run (O1). The two annotation/dimension layers are added by the seam in `T_M` (SL-Y), not by the import. The **contents** of the library (and therefore the concrete closure) are UNKNOWN until the census (BA-05 V2 section 5), which needs host work.

## 8. E-04: designated route (for Coordinator ratification)

| Route | Designation |
|---|---|
| **R-1** typed command at the command line | **DESIGNATED** (the only route) |
| R-2 menu or ribbon, R-3 script, R-4 LISP, R-5 `SendStringToExecute`, R-6 direct API call | **UNSUPPORTED** |

**Delivery in a governing run (finding PH2-E4-1).** A governing run forbids Owner touch (V2.1 21.5), so R-1 must be **delivered by the governed control plane** as keystrokes to the command line (allowed automation). A script (`.scr`) delivery, as used by the AUTH-15 host harness, is route **R-3** and sets the script bit of `CMDACTIVE`; it would **not** be R-1. The scenario `Q-ROUTE-R1` measures that the control-plane delivery produces the R-1 signature (no script, LISP, DDE or transparent bit), recorded and not inferred. If it cannot, E-04 is not satisfiable for R-1 as designed and a reviewed amendment is required. The admitted values of D-1, D-2, D-3, D-5 remain **evidence-derived** (pinned from `E4-LEARN-R1`). Facts: 34 `[CommandMethod]` entries, no `CommandFlags` anywhere, no `SendStringToExecute`, `LispFunction` or context-switching API in `src` (evidence `B7` C1, C2).

## 9. `LOCK_MODE` candidate set (no winner)

LM-1 (parameterless `LockDocument()`: every product site; default mode not visible in the code) and LM-2 (`DocumentLockMode.ExclusiveWrite`: no product site) are the candidate set, **sufficient for the baseline**; LM-3 only through a reviewed amendment. Selection follows V2.1 3.4 on evidence (`LK-LM1-*`, `LK-LM2-*`).

## 10. Semantic comparison: tolerance and normalization mapping

| Slot | Value | Authority |
|---|---|---|
| `TOL_LENGTH` | **1e-9 in** | `GeometryTolerance.Length` (`Application/Geometry/Vector2D.cs:93`); V17 4.3 |
| `TOL_ANGLE`, `TOL_NORMAL` | **1e-9 rad** | `GeometryTolerance.Angle` (`Vector2D.cs:100`); V17 4.3 maps it to "rotaciones, Normal (ST-5)" |
| `TOL_CONTINUITY` | **1e-7 in** | `GeometryTolerance.Continuity` (`Vector2D.cs:97`); V17 4.3 (values computed by normalization) |
| `TOL_TEXT_HEIGHT`, `TOL_DIM`, `TOL_DYNAMIC` (lengths) | = `TOL_LENGTH` | V17 4.3 (length) |
| `NORM_ANGLE` | both angles normalized to **(-pi, pi]** with the rule of the existing `RackSourceTransformClassifier.NormalizePi` (`Application/Systems/Shared/RackSourceTransformFactsV2.cs:255-261`), then the wrapped difference is compared with `TOL_ANGLE` (no numeric value introduced) | code rule (evidence `B5` F16, T6) |
| **`TOL_SCALE`** | **UNSET — NO AUTHORITY** | V17 4.3: "Factor de escala: no existe"; code: only caller-supplied test tolerances (evidence `B5` T7). A **separate decision, test and record** is required; it may use non-governing evidence; it is never derived from governing outputs |

Gate (V2.2 PA-9): agreed before `BASELINE_READY`; sealed before the first governing scenario that uses them; never derived from governing outputs.

## 11. `ORACLE_SPEC` (independent of the writer and of `mu_k`)

**Inputs** (captured and sealed at PV, before `Commit()`): for each selected view, the plan the orchestrator built (`HeaderRunPlan` of the mirrored design `D'`), the caller-composed envelope bytes, the computed top-level transform, the effective names computed in PREPARE-R, `NewRackId`, and the `PRE_COMMAND_STATE_RECORD`.

**Rules.**

1. The oracle derives the expected typed form of each sealed-list element **from those inputs only**: SL-D and SL-N from the plan's instances and header groups (block name, insertion, rotation, mirror flags as scale signs, dynamic parameters, texts and dimensions), SL-R from the transform and the plan layer, SL-E from the caller's envelope bytes parsed by the authoritative reader, SL-A from the view address and `NewRackId`, SL-Y from the plan (layer names and colours) and the record (absent or present), SL-S/SL-P/SL-C from the PS observation and the record plus the expected additions.
2. **Independence.** The oracle never calls the writer (`LateralHeaderDrawer`, `RackBlockData.Write`, the seams) and never reads what the writer wrote to compute an expectation; it shares only immutable plan data types. A T0 source guard (implementation gate) will assert that the oracle references no writer type.
3. **`mu_k` is given.** The oracle takes `D'` as input and does not evaluate `mu_k`; the correctness of the reflection is outside CT-21D (V-VIEW-ALL, G3).
4. **Host-default attributes (finding PH2-OR-1).** Attributes that the drawers do **not** assign (for example the layer, colour, linetype and lineweight of nested references and the text style of `DBText`; V17 L-36) have no plan value. Their expected value is given by a **`HOST_DEFAULT_MAP`** (attribute -> the context value or constant the host assigns), which is **evidence-derived** from a non-governing characterization and **pinned** before the first governing run (`SEALED_PENDING_PIN`); the context values it refers to are read into the `PRE_COMMAND_STATE_RECORD`. Until pinned, the comparison of such an attribute is `UNKNOWN` (O5 path).
5. **Excluded from comparison.** Host-derived anonymous blocks (`*U`, `*D`) as definitions; a dynamic reference is compared by its parent name and property values.

## 12. Fixtures (parameterized specifications)

All fixtures satisfy the **consolidated clean values** of evidence `B4` (table 2): envelope `SchemaVersion "1.0"`, `Kind "selective"`, no custom properties or extra keys; design `SchemaVersion "1.0"`, no `PropertyValues`; one catalog post; every bay with `Segments = []` and `MedioFrenteLength = 0`; the same `BeamId` in every cell without overrides; `SafetySelections = []`; `DrawPallets = false`; unlinked `PalletTolerance` and `VerticalClearance`; no `RACKCAD_PROJECT` NOD entry; the source reference as RackCad inserted it (scale `(1,1,1)`, normal `+Z`, definition origin `0`); no XCLIP, attributes or extension dictionary on the reference. These avoid V17 L-06..L-08, L-11..L-14, L-20..L-26 and L-29..L-33.

| Fixture | Parameters (only the differences from the clean values) | Purpose |
|---|---|---|
| `FX-1F` | `DepthCount = 1`; 2 bays; 2 loaded levels per bay; `DrawBasePlate = true`; annotations and dimensions off | minimal frontal and planta |
| `FX-2F` | `DepthCount = 2`; `ExtraFondoBays = []` (every fondo inherits fondo 0: no corner); default separators | multi-fondo; separator block in the planta |
| `FX-4F` | `DepthCount = 4` (`MaxDepthCount`) | largest fondo count; structurally outside learning |
| `FX-ANN` | `FX-1F` with `NumberFronts`, `NumberLevels`, `DrawRackName = true`; non-annotative current text style | texts and the annotation layer |
| `FX-DIM` | `FX-1F` with `Dimensions` not `None`, `DimensionStyle` unset; non-annotative current dimension style with `DIMSCALE != 0` (V17 row "anotación o cota con huella dependiente del entorno") | dimensions, anonymous dimension blocks, the dimension layer |
| `FX-IMP` | a template in which a block required by the mirrored plan is **absent** | SL-5 import path (construction open: K-8) |
| `FX-BIG` | `DepthCount = 3`; 5 bays; 4 levels; annotations and dimensions on | validation only (quantities above learning) |

**Templates.** Each fixture is instantiated as a **template drawing** (INSUNITS inches) containing the **source rack drawn by the exact build**, the required library blocks as imported by that drawing, and nothing else; plus the variant library `L-VAR` (a copy of the baseline library whose content differs for the homonym scenarios). **Instances require host work (not authorized).**

## 13. Quantity vector and shape classes

Fixed in BA-06 V2 sections 10.2 and 11 (`q1..q12`; `SH-F1`, `SH-FN`, `SH-FALL`, `SH-P`, `SH-FP`, `SH-ANN`, `SH-DIM`, `SH-IMP`, `SH-LAY-PRESENT`).

## 14. Library census schema

Fixed in BA-05 V2 section 5 (procedure and record schema); the census itself needs host work.

## 15. Naming and identity

`NewRackId` is minted once per logical mirrored rack by the orchestrator and is identical in every sibling envelope (today the product mints the id in the UI and has no sibling check: evidence `critic_ops` F-21). Effective names are computed in PREPARE-R under the lock and returned by the seams; an empty effective view name is unreachable on these paths (`critic_ops` F-20) and remains a postcondition. The final mirror name is an I-52 authority (V17), not fixed here.

## 16. What remains before this artifact can support `BASELINE_READY`

| Id | Missing item | Gate |
|---|---|---|
| K-1 | fixture **instances** (templates, library copies, `L-VAR`) | host (not authorized) |
| K-2 | library census, closure contents, fingerprint type list | host (not authorized) |
| K-3 | `TOL_SCALE` decision, test and record | separate decision (no authority) |
| K-4 | `HOST_DEFAULT_MAP` characterization (pinned value) | host, non-governing |
| K-5 | Coordinator ratification of the designated route R-1 and of its control-plane delivery | Coordinator |
| K-6 | Architect ruling on PH2-CF-1 (AUTH-15 fact), PH2-TP-1 (catalog tuple field), PH2-OR-1 (host defaults), PH2-E4-1 (route delivery) | Architect |
| K-7 | the readers and comparators marked MISSING (execution blockers) | implementation |
| K-8 | construction procedure of `FX-IMP` (natural occurrence UNKNOWN: a mirrored plan needs a block the source drawing lacks only if the mirror uses different pieces) | Architect ruling |

```text
SELECTIVE KIND BASELINE = DRAFT V2 ; NO EVIDENCE-DERIVED VALUE SELECTED
LOCK_MODE = UNSET (candidates LM-1, LM-2) ; E-04 ADMITTED SET = UNSET ; ROUTE R-1 = DESIGNATED, PENDING RATIFICATION ; TOL_SCALE = UNSET (no authority)
CT21D_AUTHORITY_BASELINE_READY = FALSE
```
