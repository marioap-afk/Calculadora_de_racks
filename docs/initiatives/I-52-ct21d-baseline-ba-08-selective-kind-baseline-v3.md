# I-52 — CT-21D Baseline Artifact BA-08: Selective Kind Authority Baseline (DRAFT V3)

> **BASELINE ARTIFACT BA-08 V3 — DRAFT, NOT SEALED. Design / documentation only. Nothing here is executed or authorized. No evidence-derived value is selected.**
>
> ```text
> ARTIFACT_ID               = SELECTIVE_KIND_AUTHORITY_BASELINE   (baseline artifact CT21D-BASE-SELECTIVE-KIND)
> ARTIFACT_VERSION          = 3-DRAFT   (supersedes 2-DRAFT, blob e63263dbcee6002e2756bd7bd5294a8b0f0e5980, which stays as history)
> STATUS AUTHORITY          = the BA-11 registry entry of this artifact is the only authority for its status and hashes;
>                             this header is frozen, non-authoritative text of the candidate bytes (BA-11 V3 section 2)
> AUTHORITY_CONTRACT        = CT-21D V2.1 3.3, 3.4, 10, 11, 15 + V2.2 + V2.3 + V2.4
> CODE FACTS BOUND TO       = 3375aadbbf929427a6d106b2fff275d64863b89c, the bound historical SHA (origin/main at extraction time, not the current tip);
>                             the 3375aadb..1304101d delta touches only RackDefinitionCreator.cs and RackDefinitionCreationResult.cs (AR2-46)
> EVIDENCE                  = docs/automation/evidence/I-52-ct21d-phase2-static-analysis/ and docs/automation/evidence/I-52-ct21d-phase2-delta/ (sa2, sa3a, sa3b, sa4)
> PHASE-2 RULING APPLIED    = BA08-01..BA08-05, BA08-07..BA08-13, XC-F5, BA09-03 (normative rule of section 6.1), AR2-15, AR2-17, AR2-35, AR2-38, AR2-40, AR2-41, AR2-42;
>                             BA08-06 was refuted (keystroke delivery by the governed control plane is allowed automation, V2.1 21.5)
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

Paths are relative to `src/` of the bound commit unless stated. **FACT** = seen in the code; **UNKNOWN** = the code does not determine it. Abbreviations: `LHD` = `RackCad.Plugin/Drawing/LateralHeaderDrawer.cs`; `SBW` = `RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs`; `BP` = `RackCad.Plugin/Drawing/BlockPlacement.cs`; `RBD` = `RackCad.Plugin/Systems/Shared/RackBlockData.cs`; `BLI` = `RackCad.Plugin/Drawing/BlockLibraryImporter.cs`; `RDC` = `RackCad.Plugin/Systems/Shared/RackDefinitionCreator.cs`.

## 1. Scope linkage

The kind covers exactly the scope of BA-03 V3: `SEL-FRONTAL` (one view per fondo `k`, `0 <= k < SelectiveDepthLayout.Count(system)`) and `SEL-PLANTA` (one whole-rack view). **The lateral and the legacy frontal `Section = -1` (`SEL-LEGACY-M1`) are out; the planta, whose canonical section is `-1`, is in** (BA08-10). `Count(system) = Math.Max(1, system?.DepthCount ?? 1)` (`Application/Systems/Selective/SelectiveDepthLayout.cs:18`); `MaxDepthCount = 4` (`Domain/Systems/Selective/SelectiveRackDefaults.cs:40`).

## 2. Seam authority references SL-1..SL-6

| Seam | Role | Required by scope | Current-code precedent | Status |
|---|---|---|---|---|
| SL-1 | frontal view definition (one per fondo, called in ascending `k`) in the caller's transaction | required | the product path creates it in its **own** transaction (`SBW:18-40`); AUTH-15 overload A (`RDC`) runs the same drawer method `LHD.CreateSystemBlock` in the caller's verified top transaction and has **no production caller** (evidence `B6` F1.02..F1.15; `sa3b` F1.8) | not implemented; AUTH-15 is an **operation-class precedent only** (AR2-38) |
| SL-2 | planta view definition in the caller's transaction | required | as SL-1 (the planta plan is also a `HeaderRunPlan`) | as SL-1 |
| SL-3 | lateral definition, own Selective contract | **not required** | lateral path `LateralHeaderDrawService` | equivalence EQ-1..EQ-5 `NOT_DEMONSTRATED` |
| SL-4 | top-level reference in the caller's transaction from a computed transform | required | jig placement in its own lock and transaction (`BP:174-201`); sets only `Position` (`sa3a` B1) | missing |
| SL-5 | PREPARE-W import with a structured result, from the opened private-copy `Database` | required | `BLI.EnsureForPlan` returns an `int`, always resolves the configured library path through the static cache, and swallows failures (`sa3b` F1.3; `sa4` CORE-07) | missing |
| SL-6 | read-only verifier and expected-versus-persisted comparator | required | readers exist (`RackBlockData.Read`, `RackBlockFinder.ScanEnvelopes`, `RackViewCodec`); no expected-versus-persisted comparator | missing |

**Contract fact discrepancy (PH2-CF-1, for the Architect).** V2.1 15.1 fact F-2 ("frontal and planta have no caller-owned seam") is **incomplete**: AUTH-15 overload A accepts any `HeaderRunPlan`, so an unused caller-owned creation primitive exists. The design clauses of V2.1 15.3 are unchanged; **no AUTH-15 evidence transfers** (P-8 remains required). Differences between the product path and AUTH-15 overload A (evidence `B6` T2, `sa3a`, `sa3b`):

| Aspect | Product path | AUTH-15 overload A | Mirror requirement |
|---|---|---|---|
| import | inside the creation callee, before its transaction (`SBW:25`) | none | PREPARE-W (SL-5), before `T_M` |
| missing pieces | skipped (no reference created, no placeholder); **reported to the user on insert** ("Bloques no definidos en el dibujo (omitidos)"); **not reported** on the `RACKEDITAR` redraw nor in a variable propagation; creation **not failed** (`sa3b` F1.1, F1.4..F1.7) | typed `MissingLibraryBlocks`, no envelope (`sa3b` F1.8) | typed failure (V2.1 15.3, 15.4; compiled row C-136 of BA-06 V3) |
| missing catalog rows | some pieces are dropped by the builders before the drawer (planta largueros and separators without a row, tarimas, safety elements, the planta celosía with a blank block), never reported (`sa3b` F1.9) | same builders | the plan requirement set is what the seams receive; the drop happens before and is part of the plan (section 7) |
| envelope input | the command composes and **re-serializes** a `RackEmbedDocument` (`RackSelectivoCommands.cs:405-417`) | writes the **caller's bytes verbatim** | caller-composed envelope written verbatim (C-127) |
| envelope validation | none | refuses a blank `Id`, `Kind` or `Name` at `3375aadb`; a blank `Id` or `Kind` at `1304101d` (AUTH-15-C1) | `InvalidEnvelope` PRE-WRITE (C-rows of 15.4) |
| envelope write | only if non-empty, no read-back (`SBW:31-34`) | written, read back, ordinal compare | verbatim write and read-back (C-127..C-129) |
| naming | `RackViewBaseName` + `UniqueBlockName` | requested name passed to `UniqueBlockName` | effective names computed in PREPARE-R and returned; nested names follow section 15 |
| transaction | own `StartTransaction`, `Commit` | caller's top transaction verified | typed authority `IsTopTransaction` (V2.1 15.3, 4.2) |
| placement | jig, own lock and transaction | none | SL-4 in `T_M` |

## 3. What creating a Selective view produces (FACT unless marked)

| Item | Fact | Evidence |
|---|---|---|
| definition names | frontal: `LinkedSelectiveFrontal(name, fondo, fondoCount)` then `Standard` (base name = the rack name or `Selectivo`); planta: `name + " - planta"` or `"Selectivo planta - {N} frentes"`; sanitized; uniquified by `_n` suffixes against the live block table; origin `(0,0,0)` | `Application/Systems/Shared/RackViewBaseName.cs:22-37,186-191,202`; `Application/BlockNaming.cs:12-29`; `LHD:240-247,395-413`; `sa3a` B3 |
| nested definitions | frontal `SEL_FRONTAL_<Role>_<n>` (buckets of two or more identical pieces); planta `PLANTA_CAB_<n>` (one per distinct frame, even a single one); requested names **restart per plan**; effective names depend on the block table at the moment of the call (section 15) | `sa3b` F5.1, F5.2 and the thresholds noted by its verifier |
| empty nested definitions | when a grouped piece's block is absent, the nested definition is still created (empty or partial) and referenced at every placement | `sa3b` (verifier, "missed" item 1): `LHD:38-51,56-67` |
| entity types | `BlockReference` (pieces, nested placements), `DBText`, `RotatedDimension`; no attributes; negative scale only on nested piece references | `sa2` F05-G03, F05-S03..S06 |
| catalog `blocks.csv` | **exists** in the repository (94 rows: 30 FRONTAL, 35 LATERAL, 29 PLANTA); copied to the build output and resolved next to `RackCad.Application.dll`; the runtime copy can differ from the repository file | evidence `B3` F01, F24..F26 |
| library drawing `blocks-library.dwg` | **not versioned**; path from user settings, else next to the catalogs | `sa3b` F7.7 |
| dynamic-block parameters | FRONTAL: `LONGITUD`, `PERALTE`, `ALTURA`, `SAQUE`, `FRENTE`; PLANTA: `LONGITUD`, `PERALTE`, `SAQUE`; **`FONDO` only in the lateral view** (out of scope); applied only to catalog piece references, never to the references of the nested definitions; names matched case-insensitively; a name the block does not expose is silently ignored | `sa3b` F3.1..F3.5 (BA08-09) |
| layers | `RACKCAD_ANOTACIONES` (ACI 2) and `RACKCAD_COTAS` (ACI 1), created lazily only when a text or dimension is materialized and only if absent; only `Name` and `Color` are set | `sa3a` L1..L4 |
| envelope | `RackEmbedDocument` in an `Xrecord` under `RACKCAD_SELECTIVE` in the extension dictionary of the **definition**; text chunks (DXF code 1) of at most 255 UTF-16 units | `RBD:16-52`; `sa3a` N3 |
| NOD / project variables | creation neither reads nor writes `RACKCAD_PROJECT` or `RACKCAD_CUSTOM_PROPERTIES`; `RACKEDITAR` reads `RACKCAD_PROJECT` (never writes it) | `sa3a` N1..N7 |
| commands | **35** `[CommandMethod]` attributes in `src` (corrects the count 34 of evidence `B7` C1, BA08-08); no `CommandFlags`; no RACKMIRROR command | `sa3b` F2.1, F2.2 |
| plan mutability | `HeaderRunPlan` and `HeaderGroup` store the caller's list references; the runtime collections are `List<T>`; `HeaderBlockInstance` has public setters and a mutable `DynamicParameters` dictionary; **the plan types are mutable** | `sa3b` F4.1..F4.5 (BA08-02) |

**Tuple consequence (PH2-TP-1).** The runtime catalog folder is build-bound content that can change, so the **catalog folder files** must be pinned by SHA-256 in the BUILD_BOUND tuple together with the library file.

### 3.1 Writer-behaviour constants (for `ORACLE_SPEC`, BA08-03; evidence `sa3a`)

| Constant | Value | Evidence |
|---|---|---|
| annotation layer | `RACKCAD_ANOTACIONES`, ACI 2, created only if absent, only `Name` and `Color` set | `LHD:270,323-331`; `LayerHelper.cs:16-29` |
| dimension layer | `RACKCAD_COTAS`, ACI 1, same rule | `LHD:354-357` |
| text | `DBText`; height = `TextHeight` when > 0 else 3.0; `HorizontalMode = TextCenter`, `VerticalMode = TextVerticalMid`; `Position` = `AlignmentPoint` = the anchor (Z = 0); `AdjustAlignment` after append; **no** text style, rotation, width factor, oblique, colour, linetype or lineweight set | `LHD:265-281`; `sa3a` T1, T2 |
| Selective text sizing | base height `6 x scale` (scale <= 0 treated as 1); rack-name label `1.5 x h`; margin 4 in; label positions as in `sa3a` T5 (frontal) and T6 (planta) | `Application/Systems/Selective/SelectiveAnnotations.cs:15-32` |
| dimension geometry | `RotatedDimension` with `p1 = Insertion`, `p2 = ConnectionAnchor`; horizontal when `abs(dy) <= abs(dx)`, rotation 0 or pi/2; dimension-line point at `(midX, p1.Y + offset)` or `(p1.X + offset, midY)`; text `""` | `LHD:338-360`; `sa3a` D1 |
| dimension style | the named style (`system.DimensionStyle`) used as-is when present in the drawing; otherwise the drawing's current style with the automatic overrides; never created; a missing named style falls back silently | `LHD:351-354,378-392`; `sa3a` D2 |
| automatic overrides | `Dimscale = 1.0`, `Dimtxt = h`, `Dimasz = 0.7h`, `Dimexe = 0.4h`, `Dimexo = 0.4h`, `Dimgap = 0.3h`, `Dimtad = 1`, `Dimdec = 2`; `RecomputeDimensionBlock(true)` always | `LHD:362-375`; `sa3a` D3 |
| Selective dimension constants | chain gap 14, overall gap 22, elevation step 14 (times scale); offsets negative; zero-length segments (< 1e-6) skipped; per-view detail gating | `SelectiveDimensions.cs:22-30,60-130,205-264`; `sa3a` D4..D6 |
| nested and top-level references | **no** `LayerId`, colour, linetype or lineweight set; the top-level reference gets only `Position` (no rotation, scale or normal) | `LHD:60-63,145-148,308-314`; `BP:174-201,240-244`; `sa3a` L7, B1 |
| unexposed dynamic parameters | skipped with no log or message; `RecordGraphicsModified(true)` only when at least one value was applied | `LHD:415-449`; `sa3a` P1 |

## 4. Sealed list (`VERIFY_ELEMENT_LIST`) and its coverage

| Class | Element | Layer | Covered by scenarios (BA-04 V3) |
|---|---|---|---|
| SL-D | each destination view definition | SEM | `CL-CLEAN-*`, `WR-A-*`, `WR-E-*` |
| SL-N | each nested definition the seam creates | SEM | `CL-CLEAN-FX2F-ALL`, `CL-CLEAN-FX4F-ALL`, `WR-E-*` |
| SL-R | each top-level reference (name, position, scale, rotation, normal, layer) | SEM | `CL-CLEAN-*`, `KS-SL4-NEG-DET` |
| SL-E | each envelope, including `ExtensionData` and unknown members | SEM | `CL-CLEAN-*`, `WR-A-*` |
| SL-A | view address and grouping (`Embed.Id` = `NewRackId`) | SEM | `CL-CLEAN-FX1F-FP`, `CL-CLEAN-FX2F-ALL`, `WR-D-*` |
| SL-Y | the RackCad layers the created entities use: **reused** (present) or **added** (absent) | SEM | reused: `CL-CLEAN-FXANN-FP`, `CL-CLEAN-FXDIM-FP`; added: see the sealed reason below |
| SL-S | source read-set | EXA | `WR-G-*`, `NC-E08-Y0`, `NC-E08-Y2` |
| SL-P | the **consumed registry state** of `RACKCAD_PROJECT`, **including `ABSENT`** (AR2-15) | EXA | `WR-F-*` (the creation `ABSENT -> present`; the fixtures have no entry) |
| SL-C | containers (block table names, model-space handles, sibling set, extension dictionaries) | EXA | `WR-D-*`, `WR-E-*` |

**Sealed reason for SL-Y "added" (BA08-01).** For a mirror of a product-drawn rack, the layers a mirrored view needs are decided by the same inputs the source used (view kind, toggles, name, bays and levels, dimension settings); the source's own draw ensured them, so **no layer is added unless a RackCad layer was renamed or removed outside RackCad after the source was drawn** (evidence `sa3a` S5, S6 as corrected by its verifier). The rename case is **outside the CT-21D fixtures** (whether it is admissible under V17 L-29/L-33 is not determined here) and remains a **disclosed residual**. The "added" behaviour is exercised only as an operation class: `EV-L-OC-LAYER` on `FX-SCRATCH` and the `SH-LAY-ADDED` dry-run trace (BA-06 V3). The oracle predicts it by the rule of section 3.1 (created only if absent, ACI 2 or 1).

## 5. `PROTECTED_OBJECTS` and the phase-A domain (V2.2 PA-10)

Phase A adjudicates events on (1) the plan-time protected set (SL-S, SL-P, the pre-existing containers of SL-C) and (2) the objects created inside `T_M` (the handles returned by the seams for SL-D, SL-N, SL-R, SL-E, SL-Y). At PV the sealed `PROTECTED_OBJECTS` is checked for consistency with that domain; events outside it are recorded and not adjudicated. Anonymous blocks derived by the host (`*U`, `*D`) are outside the definitions list and inside the domain only through their owning reference or dimension.

## 6. Abort read-set: component specifications (readers are execution blockers)

The specification of each component is **fixed here**; the readers and comparators marked MISSING are **execution blockers** (EXEC-4), not baseline blockers.

| Component | Compared value | Layer | Scope | Reader status |
|---|---|---|---|---|
| block table names | the full name set (stored case) | EXA (`ENUMERATION`, sorted) | the whole block table | MISSING |
| definitions | `DEFINITION_DUMP` of every RackCad-owned definition, every manifest definition and every pre-existing homologue of the static closure | EXA | those definitions | MISSING (fingerprint reader) |
| model-space content | the handle set and the RackCad reference set with class | EXA (`ENUMERATION`, session-local) | model space | MISSING |
| RackCad payloads | `PAYLOAD_EXACT` of **every definition** that holds a RackCad payload; set of `RackId` (expected: no new id) | EXA | **every definition of the block table** (BA08-05, AR2-40); `ScanEnvelopes` skips layouts, anonymous and xref-owned records, so the **full-scope reader is an execution blocker (EXEC-4)** | MISSING (full-scope reader); `RBD.Read` exists |
| view addresses and grouping | decoded address and `Embed.Id` per view | EXA | as above | codec exists (`RackViewCodec`); host reader MISSING |
| library residue | the ADD set with its closure | EXA | manifest | MISSING |
| symbol tables | name sets of layers, linetypes, text styles, dimension styles and registered applications; `SYMBOL_RECORD_*` of: each pre-existing record of the closure; `RACKCAD_ANOTACIONES` and `RACKCAD_COTAS`; **every layer the created entities land on** (resolved through `HOST_DEFAULT_MAP` and the `PRE_COMMAND_STATE_RECORD`, for example the current layer); the dimension style and the text style in effect **and their transitive records** (the linetypes, text styles and arrow blocks they reference) | EXA | those records (BA08-05) | MISSING |
| project variables | `NOD_ENTRY` of `RACKCAD_PROJECT` and `RACKCAD_CUSTOM_PROPERTIES`, or `ABSENT` | EXA | the NOD entries named | readers exist (`ProjectVariablesData.Read`, `CustomPropertiesData.Read`); aggregate MISSING |

### 6.1 Normative rule for PREPARE-R and the warm-up (BA09-03, AR2-42)

The mirror's PREPARE-R and the warm-up **never** call `AutoCadExternalLibraryBlockQuery`, `BlockLibraryImporter` or `BlockLibraryDatabaseCache` (the library query of AUTH-12 and the process cache). The library content is read only from the per-run private copy `Database` that the orchestrator opens and disposes (V2.1 19.1); the warm-up uses its **own** warm-up-only copy and `Database`, never entered into the run cache or the `LibraryInputRecord`, disposed before step 4. Reason: the cache is a single process-wide slot, and acquiring any other path evicts and disposes the cached database (evidence `sa4` CORE-02, CORE-07). A static guard (EXEC-12) will assert that the seams and the warm-up harness do not reference those types.

## 7. Static dependency closure and the required-block set

**Extractor (BA08-11).** The plan requirement set is `RackBlockRequirementExtractors.HeaderRun.Extract(HeaderRunPlan)` (`Application/Systems/Shared/LibraryBlockRequirements.cs:45-80`): every `BlockName` of the loose instances and the header-group instances, blank names skipped, de-duplicated case-insensitively. **The PREPARE-R closure input is that set**; the **census scope** is the static universe below.

**Static universe (evidence `sa3b` F7.3, F7.4; names resolved through `blocks.csv` by piece id and view).** With the catalogs of the bound SHA a FRONTAL plan can require only these **23** names and a PLANTA plan only these **23** (disjoint; union **46**):

| Plan | Names |
|---|---|
| FRONTAL (23) | `POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA_FRONTAL`; `PLACA_BASE_DE_CABECERA_ATORNILLABLE_DE_PLACA_CALIBRE_3_16_FRONTAL`; `LARGUERO_ESCALON_CAL14_3_REMACHES_FRONTAL`; `LARGUERO_IN_OUT_C6_FRONTAL`; `LARGUERO_ESCALON_INFINITO_FRONTAL`; `LARGUERO_ESCALON_TROQUEL_REDONDO_FRONTAL`; `PROTECTOR_BOTA_H_3_16_18_FRONTAL`; `PROTECTOR_BOTA_C_4_FRONTAL`; `PROTECTOR_BOTA_C_6_FRONTAL`; `PROTECTOR_LATERAL_BOTA_H_3_16_18_FRONTAL`; `PROTECTOR_LATERAL_BOTA_C_4_FRONTAL`; `PROTECTOR_LATERAL_BOTA_C_6_FRONTAL`; `LARGUERO_ESCALON_TOPE_DE_3_FRONTAL`; `POSTE_3_1_5_8_TOPE_FRONTAL`; `DESVIADOR_A_3_FRONTAL`; `DESVIADOR_A_4_FRONTAL`; `DESVIADOR_L_3_FRONTAL`; `DESVIADOR_L_3_5_FRONTAL`; `DESVIADOR_L_4_FRONTAL`; `DESVIADOR_L_4_5_FRONTAL`; `DESVIADOR_L_5_FRONTAL`; `TARIMA_GENERICA`; `PARRILLA_GENERICA_FRONTAL` |
| PLANTA (23) | `POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA_PLANTA`; `PLACA_BASE_DE_CABECERA_ATORNILLABLE_DE_PLACA_CALIBRE_3_16_PLANTA`; `TRAVESANO_PARA_POSTE_OMEGA_DE_CINTA_CALIBRE_14_PLANTA`; `LARGUERO_ESCALON_CAL14_3_REMACHES_PLANTA`; `LARGUERO_IN_OUT_C6_PLANTA`; `LARGUERO_ESCALON_INFINITO_PLANTA`; `LARGUERO_ESCALON_TROQUEL_REDONDO_PLANTA`; `SEPARADOR_DE_CABECERA_FORMADA_DE_CINTA_CALIBRE_12_PLANTA`; `PROTECTOR_BOTA_H_3_16_18_PLANTA`; `PROTECTOR_BOTA_C_4_PLANTA`; `PROTECTOR_BOTA_C_6_PLANTA`; `PROTECTOR_LATERAL_BOTA_H_3_16_18_PLANTA`; `PROTECTOR_LATERAL_BOTA_C_4_PLANTA`; `PROTECTOR_LATERAL_BOTA_C_6_PLANTA`; `LARGUERO_ESCALON_TOPE_DE_3_PLANTA`; `POSTE_3_1_5_8_TOPE_PLANTA`; `DESVIADOR_A_3_PLANTA`; `DESVIADOR_A_4_PLANTA`; `DESVIADOR_L_3_PLANTA`; `DESVIADOR_L_3_5_PLANTA`; `DESVIADOR_L_4_PLANTA`; `DESVIADOR_L_4_5_PLANTA`; `DESVIADOR_L_5_PLANTA` |

**Catalog files that determine the names (SHA-256 of the blobs at the bound SHA):** `blocks.csv` `5ef806529b2dab33b057101dd344f2f250919ae7eda121bb88ae7c836f7318fb`; `secciones.csv` `94baea1be03a65ad4c74c90e6038251eafde7fd205b0d7cde79a91a7191449a5`; `base-plates.csv` `7f8d924296af0f834db821ba0a87e72e8340530155066be54996000de6af239b`; `seguridad.csv` `f438600d3e59075dfb62333b46bb88e6f8ae843bdc3f2aae3328921b5d737059`; `defaults.json` `fb06ea01e8d52b38c441cbcccfc73b8df1b2c1333637f010dbf6b128717279cf`. Geometry-only files that can gate the planta celosía: `connection-layout.csv` `5c63da5b64c63e3d3bc6687d53f481cf49a20957a61e3d0c4674bd46fbfa3d37`; `connection-points.csv` `473f0c205fb9fdb786ed445d4fd67dde9f596440d7d723d92d30deab47ec44f8`. **Rule:** before the census and before the seal of the catalog, the universe is **re-derived from, or asserted equal to**, the catalog files pinned in the tuple; a difference makes this table a new version.

**Clean fixtures require** (FRONTAL) the post, the plate (with `DrawBasePlate`) and the chosen beam's `LARGUERO_*_FRONTAL`; (PLANTA) the post, the plate, the celosía, the first level's `LARGUERO_*_PLANTA` and, with two or more fondos and a gap, the separator.

**Closure rule (normative for the kind).** In PREPARE-R, from the private library copy, read-only: for every name of the plan requirement set, collect the block definition, the transitive closure of its nested definitions, and the symbol records they reference; classify each member `ADD`, `REUSE_AS_IS`, `REUSED_BOUND` or `REJECT` with host symbol-table semantics and stored case (V2.1 11.3, V2.2 PA-7); an unclassifiable member makes the closure `UNKNOWN` and refuses the run (O1). The annotation and dimension layers are added by the seam in `T_M` (SL-Y), not by the import. The **contents** of the library are UNKNOWN until the census (BA-05 V3 section 5), which needs host work.

## 8. E-04: designated route (for Coordinator ratification)

| Route | Designation |
|---|---|
| **R-1** typed command at the command line | **DESIGNATED** (the only route) |
| R-2 menu or ribbon, R-3 script, R-4 LISP, R-5 `SendStringToExecute`, R-6 direct API call | **UNSUPPORTED** |

**Delivery in a governing run (PH2-E4-1).** A governing run forbids Owner touch (V2.1 21.5), so R-1 is delivered by the **governed control plane** as keystrokes to the command line: allowed automation of the governed control plane (the phase-2 review refuted the objection, BA08-06). **D-4 is recorded by the delivering control plane** (V2.1 3.3: recorded, not inferred); the `CMDACTIVE` bits are recorded as observations and are **not** used as the identity of the route. A script (`.scr`) delivery is R-3 and is not used. `Q-ROUTE-R1` measures the delivery. The Coordinator's ratification of R-1 and of its delivery remains pending (K-5).

**Source of the E-04 pin (BA08-13, AR2-11).** The admitted values of D-1, D-2, D-3, D-5 are pinned from the non-governing `E4-LEARN-R1-*` runs under the selected `LOCK_MODE`, never from a governing run or from hindsight. The membership condition of V2.1 3.3 over governing runs is checked **offline**: every pinned member must be observed in every governing R-1 run at its checkpoint; otherwise the E4 group verdict is `UNKNOWN` (the pin is not revised). Facts: 35 `[CommandMethod]` entries, no `CommandFlags`, no `SendStringToExecute`, `LispFunction` or context-switching API in `src`.

## 9. `LOCK_MODE` candidate set (no winner)

LM-1 (parameterless `LockDocument()`: every product site; default mode not visible in the code) and LM-2 (`DocumentLockMode.ExclusiveWrite`: no product site) are the candidate set, **sufficient for the baseline**; LM-3 only through a reviewed amendment. Selection follows V2.1 3.4 by review over the non-governing `LK-*` characterization (AR2-11); `NO_CANDIDATE` gives `LK = FAIL`.

## 10. Semantic comparison: tolerance and normalization (the values of V2.2 PA-9 live here; BA-05 V3 names the slots only)

| Slot | Value | Authority |
|---|---|---|
| `TOL_LENGTH` | **1e-9 in**, absolute | `GeometryTolerance.Length` (`Application/Geometry/Vector2D.cs:93`); V17 4.3 |
| `TOL_ANGLE`, `TOL_NORMAL` | **1e-9 rad**, absolute | `GeometryTolerance.Angle` (`Vector2D.cs:100`); V17 4.3 "rotaciones, Normal (ST-5)" |
| `TOL_TEXT_HEIGHT`, `TOL_DIM`, `TOL_DYNAMIC` (lengths) | = `TOL_LENGTH` | V17 4.3 (length) |
| `NORM_ANGLE` | both angles normalized to **(-pi, pi]** with the rule of `RackSourceTransformClassifier.NormalizePi` (`Application/Systems/Shared/RackSourceTransformFactsV2.cs:255-261`), then the wrapped difference is compared with `TOL_ANGLE` | code rule (evidence `B5` F16, T6) |
| **`TOL_SCALE`** | **UNSET — NO AUTHORITY FOR CT-21D** | V17 4.3: "Factor de escala: no existe"; the product value is **absolute, not relative**, and **fixed at G4 with test and record**. The phase-1 ruling requires a separate decision, test and record for CT-21D; the test needs host evidence (non-governing). **Not decided here** (AR2-35, K-3) |

**`TOL_CONTINUITY` (BA08-12, BA05-F09).** No Selective quantity is classed "computed by normalization": the oracle derives every expected value from the sealed plan capture and the computed transform (section 11), so every length uses `TOL_LENGTH`. The V2 row `TOL_CONTINUITY` is **removed**; `GeometryTolerance.Continuity` (1e-7 in) is not used by CT-21D.

Gate (V2.2 PA-9): agreed before `BASELINE_READY`; sealed before the first governing scenario that uses them; never derived from governing outputs; a change is a new baseline.

## 11. `ORACLE_SPEC` (independent of the writer and of `mu_k`)

**Inputs (BA08-02).** In **PREPARE-R, before any seam call**, the orchestrator takes a **deep, canonical, hashed capture** of each plan it built (the `HeaderRunPlan` of the mirrored design `D'`, serialized with every instance, group, placement and dynamic parameter; the plan types are mutable, section 3), and seals it then. **PV only checks the sealed hash**; a **plan-equality check** asserts that the plan each seam received equals the sealed capture (a difference is an abort before `Commit()`). The other inputs: the caller-composed envelope bytes, the computed top-level transform, the effective names computed in PREPARE-R, `NewRackId`, the `PRE_COMMAND_STATE_RECORD`, the pinned `HOST_DEFAULT_MAP`, and the constants of section 3.1.

**Rules.**

1. The oracle derives the expected typed form of each sealed-list element **from those inputs only**: SL-D and SL-N from the captured plan's instances and header groups (block name, insertion, rotation, mirror flags as scale signs, dynamic parameters, texts and dimensions) **and the writer-behaviour constants of section 3.1** (layer names and colours, text defaults and alignment, the dimension geometry rule, the style choice and the automatic overrides); SL-R from the computed transform and the **layer of the top-level reference given by the I-52 authority** (V17 L-33: the copy keeps the presentation of the source; the source reference's layer is read into the `PRE_COMMAND_STATE_RECORD`); SL-E from the caller's envelope bytes parsed by the authoritative reader; SL-A from the view address and `NewRackId`; SL-Y from the constants and the record (present or absent); SL-S, SL-P and SL-C from the PS observation and the record plus the expected additions.
2. **Plan parameters the block does not expose** (the writer ignores them silently, section 3.1): the expected dynamic-property set of a reference is the intersection of the plan's parameters with the writable properties the library block exposes; the exposed set comes from the library census (BA-05 V3 section 5) and is `UNKNOWN` until then (K-2).
3. **Independence.** The oracle never calls the writer (`LateralHeaderDrawer`, `RackBlockData.Write`, the seams) and never reads what the writer wrote to compute an expectation; it reads the **sealed capture**, not the live plan objects. A T0 source guard (implementation gate) will assert that the oracle references no writer type.
4. **`mu_k` is given.** The oracle takes `D'` as input and does not evaluate `mu_k`; the correctness of the reflection is outside CT-21D (V-VIEW-ALL, G3).
5. **Host-default attributes (PH2-OR-1).** Attributes the drawers do not assign (the layer, colour, linetype and lineweight of nested and top-level references where the I-52 authority gives none; the text style, width factor and oblique of `DBText`) have no plan value. Their expected value comes from the **`HOST_DEFAULT_MAP`** (a pin slot, BA-04 V3 section 2.1), derived from non-governing characterization; the context values it refers to are read into the `PRE_COMMAND_STATE_RECORD`. Until pinned, the comparison of such an attribute is `UNKNOWN` (O5 path).
6. **Excluded from comparison.** Host-derived anonymous blocks (`*U`, `*D`) as definitions; a dynamic reference is compared by its parent name and property values.

## 12. Fixtures (parameterized specifications; AR2-17, AR2-41)

**Rule of completeness (BA08-04).** A fixture's design is the vector below; **every field of `SelectivePalletDesign` (and of its bays, cells and pallets) that the vector does not list takes the default of the domain type at the bound SHA** (`Domain/Systems/Selective/SelectivePalletDesign.cs`). Common clean values (evidence `B4` table 2): envelope `SchemaVersion "1.0"`, `Kind "selective"`, no custom properties or extra keys; design `SchemaVersion "1.0"`, no `PropertyValues`; `PostId = POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA`; every bay `Segments = []` and no medio frente; the same `BeamId` in every cell without overrides; `SafetySelections = []`; `DrawPallets = false`; unlinked `PalletTolerance` and `VerticalClearance`; **no `RACKCAD_PROJECT` entry**; the source reference as RackCad inserted it (scale `(1,1,1)`, normal `+Z`, definition origin `0`); no XCLIP, attributes or extension dictionary on the reference; INSUNITS inches. These values avoid V17 L-06..L-08, L-11..L-14, L-20..L-26 and L-29..L-33.

| Fixture | Name | `DepthCount` | Bays x levels | Cell (every cell) | `DrawBasePlate` | Toggles | Dimensions | Overrides of the clean values | Purpose |
|---|---|---|---|---|---|---|---|---|---|
| `FX-1F` | `CT21D-FX1F` | 1 | 2 x 2 | `BeamId = LARGUERO_ESCALON_CAL14_3_REMACHES`, `BeamPeralte = 4.0`, `PalletCount = 2`, pallet `Frente = 40`, `Alto = 48` | true | all off | `None` | none | minimal frontal and planta |
| `FX-2F` | `CT21D-FX2F` | 2 | 2 x 2 | as `FX-1F` | true | all off | `None` | `ExtraFondoBays = []` (no corner), default separators | multi-fondo; separator |
| `FX-4F` | `CT21D-FX4F` | 4 | 2 x 2 | as `FX-1F` | true | all off | `None` | as `FX-2F` | largest fondo count; structurally outside learning |
| `FX-ANN` | `CT21D-FXANN` | 1 | 2 x 2 | as `FX-1F` | true | `NumberFronts`, `NumberLevels`, `DrawRackName` on | `None` | the toggles; non-annotative current text style; the annotation layer exists because the source drew annotations | texts; SL-Y reuse |
| `FX-DIM` | `CT21D-FXDIM` | 1 | 2 x 2 | as `FX-1F` | true | all off | `Standard`, `DimensionViews` unset, `DimensionStyle` unset | the dimension settings; non-annotative current dimension style with `DIMSCALE != 0`; the dimension layer exists because the source drew dimensions | dimensions; `*D` blocks |
| `FX-BIG` | `CT21D-FXBIG` | 3 | 5 x 4 | as `FX-1F` | true | all on | `Detailed` | the toggles and dimensions | validation only |
| `FX-SCRATCH` | (none) | — | — | — | — | — | — | an empty template (INSUNITS inches) with the library blocks of `FX-1F` imported and **no** RackCad content or layer | learning micro-scenarios needing absence; `SH-LAY-ADDED` traces |
| `FX-IMP` | `CT21D-FXIMP` | 1 | 2 x 2 | as `FX-1F` | true | all off | `None` | a template in which **at least two** blocks required by the mirrored plan are absent | SL-5 import path: **`SPECIFICATION_OPEN`** (K-8) |

**Variants.**

| Variant | Construction | Used by | State |
|---|---|---|---|
| `FX-1F-XREF-UNRESOLVED` | `FX-1F` plus one attached external reference whose file is missing | `NC-E07` | defined |
| `FX-1F-XREF-HOMONYM` | `FX-1F` plus a **resolved** attached external reference named as one required block (the single wrong-type variant, BA04-F20) | `PW-CLONE-WRONG-TYPE` | defined |
| `FX-IMP-CASE` | `FX-IMP` with a required block present under a name differing only by case | `PW-CLONE-CASE` | `SPECIFICATION_OPEN` (depends on `FX-IMP`) |
| `FX-IMP-NESTED` | `FX-IMP` with the nested dependencies of an absent block present | `PW-CLONE-NESTED` | `SPECIFICATION_OPEN` |
| `FX-IMP-SYMTAB` | `FX-IMP` with closure layers, linetypes, text and dimension styles present with different properties | `PW-CLONE-SYMTAB` | `SPECIFICATION_OPEN` |
| `L-VAR-DIFF` | a copy of the baseline library whose content differs for a block, a nested block, a layer, a linetype and a text style (homonyms of the drawing) | `PW-CLONE-BASE` | defined (content depends on the census, K-2) |
| `L-VAR-PROXY` | a copy of the baseline library with a proxy entity inside one required block | `PR-REC-CLOSURE-UNKNOWN` | defined (content depends on the census) |

**`FX-IMP` (K-8, AR2-41).** Its construction must be **reachable with product operations** (for example a source drawn in a drawing whose library later lacks a block that the mirror needs), or be **declared artificial with its reason**; it is decided with the fixture-instance gate (host). Until then it is `SPECIFICATION_OPEN` and every scenario that uses it carries `BLOCKED_BY = K-8` (BA-04 V3).

**Templates and instances (AR2-17).** Each fixture is instantiated as a template drawing containing the source rack drawn by the exact build, the required library blocks as imported by that drawing, and nothing else. The instance hash is a **pin slot** `FIXTURE_INSTANCE@<FX>` (BA-04 V3 section 2.1): pinned before the first governing run, with its conformance to this specification recorded. **Instances require host work (not authorized).**

## 13. Quantity vector and shape classes

Fixed in BA-06 V3 sections 10.2 and 11 (`q1..q12`; `SH-F1`, `SH-FN`, `SH-FALL`, `SH-P`, `SH-FP`, `SH-ANN`, `SH-DIM`, `SH-IMP`, `SH-LAY-ADDED`).

## 14. Library census and product-drawn types

The census procedure and record schema are fixed in BA-05 V3 section 5; the census needs host work. The entity types RackCad itself draws (`BlockReference`, `DBText`, `RotatedDimension`, `Polyline`, `Circle`; evidence `sa2`) are the second source of the type list (BA-05 V3 section 2.5).

## 15. Naming and identity

`NewRackId` is minted once per logical mirrored rack by the orchestrator and is identical in every sibling envelope. Effective view names are computed in PREPARE-R under the lock and returned by the seams. **Nested names (BA06-F11):** the requested names restart per plan and the effective names depend on the block table at the call; the orchestrator therefore calls SL-1 in **ascending `k`**, then SL-2, and every seam returns the handles of all objects it created (nested definitions included), from which the sealed list takes the effective names. The final mirror name is an I-52 authority (V17), not fixed here.

## 16. What remains (XC-F5: separated by gate)

### 16.1 Items that gate `BASELINE_READY`

| Id | Missing item | Gate |
|---|---|---|
| K-2 | library census, closure contents, fingerprint type list | host (not authorized) |
| K-3 | `TOL_SCALE` decision, test and record | separate decision; the test needs non-governing host evidence (not decided by this session) |
| K-5 | Coordinator ratification of the designated route R-1 and of its control-plane delivery | Coordinator |
| K-6 | Architect ruling on PH2-CF-1 (AUTH-15 fact), PH2-TP-1 (catalog tuple field), PH2-OR-1 (host defaults), PH2-E4-1 (route delivery) | Architect |
| K-8 | construction of `FX-IMP` | fixture-instance gate (host) |

### 16.2 Pinned before the first governing run (not baseline blockers)

| Id | Item |
|---|---|
| K-1 | fixture instances: pin slots `FIXTURE_INSTANCE@*` |
| K-4 | `HOST_DEFAULT_MAP`: pin slot, from non-governing characterization |

### 16.3 Execution prerequisites (EXEC-4, EXEC-12)

| Id | Item |
|---|---|
| K-7 | the readers and comparators marked MISSING in section 6, including the full-scope payload reader |
| K-9 | the static guard of section 6.1 (no reference to the library cache, importer or AUTH-12 external query in the seams and the warm-up harness) |

```text
SELECTIVE KIND BASELINE = DRAFT V3 ; NO EVIDENCE-DERIVED VALUE SELECTED
LOCK_MODE = UNSET (candidates LM-1, LM-2) ; E-04 ADMITTED SET = UNSET ; ROUTE R-1 = DESIGNATED, PENDING RATIFICATION ; TOL_SCALE = UNSET (no authority)
CT21D_AUTHORITY_BASELINE_READY = FALSE
```
