# I-52 — CT-21D Baseline Artifact BA-08: Selective Kind Authority Baseline (DRAFT V4)

> **BASELINE ARTIFACT BA-08 V4 — DRAFT, NOT SEALED. Design / documentation only. Nothing here is executed or authorized. No evidence-derived value is selected.**
>
> ```text
> ARTIFACT_ID               = SELECTIVE_KIND_AUTHORITY_BASELINE   (baseline artifact CT21D-BASE-SELECTIVE-KIND)
> ARTIFACT_VERSION          = 4-DRAFT (supersedes 3-DRAFT, blob 69e33a898814df9afbb8d3c6ae63c569a84eb291, which stays as history)
> STATUS AUTHORITY          = the BA-11 registry entry of this artifact is the only authority for its status and hashes;
>                             this header is frozen, non-authoritative text of the candidate bytes (BA-11 V4 section 2)
> AUTHORITY_CONTRACT        = CT-21D V2.1 + V2.2 + V2.3 + V2.4 (draft revision 3) + V2.5 (draft revision 1); V2.4 and V2.5 pending
>                             Coordinator textual verification
> CONTRACT CLAUSES USED     = V2.1 2.1, 3.3, 3.4, 5, 6, 10, 11, 15; V2.2 PA-7, PA-8, PA-9, PA-10; V2.5 section 1 (CATALOG_FOLDER)
> CODE FACTS BOUND TO       = 3375aadbbf929427a6d106b2fff275d64863b89c, the bound historical SHA (origin/main at extraction time, not the current tip);
>                             the 3375aadb..1304101d delta touches only RackDefinitionCreator.cs and RackDefinitionCreationResult.cs (AR2-46)
> EVIDENCE                  = docs/automation/evidence/I-52-ct21d-phase2-static-analysis/ and docs/automation/evidence/I-52-ct21d-phase2-delta/ (sa2, sa3a, sa3b, sa4)
> DELTA RULING APPLIED      = decisions section 214 (AR3-01, AR3-02, AR3-07, AR3-11, AR3-12, AR3-13, AR3-14, AR3-15, AR3-17, AR3-25,
>                             AR3-26, AR3-30); section 211 (AR2-xx) where section 214 does not change it; V4 correction pass:
>                             open Architect questions AQ-V4-01, AQ-V4-03, AQ-V4-06..AQ-V4-10 referenced, none decided (section 16.4)
> PHASE-2 RULING (V3, kept) = BA08-01..BA08-05, BA08-07..BA08-13, XC-F5, BA09-03, AR2-15, AR2-17, AR2-35, AR2-38, AR2-40, AR2-41, AR2-42;
>                             BA08-06 was refuted (keystroke delivery by the governed control plane is allowed automation, V2.1 21.5)
> DEPENDS_ON                = BA-01, BA-03, BA-05, BA-06   (registry entries only; BA-11 V4 section 3)
> PREREQUISITES             = K-3, K-8 and K-6 (the Architect's rulings: AQ-V4-07 = Q-HDM-1, and AQ-V4-08, AQ-V4-09, AQ-V4-10
>                             where a ruling differs from the reading recorded here): before the candidate; K-2 and K-5: before
>                             the seal (section 16)
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

Paths are relative to `src/` of the bound commit unless stated. **FACT** = seen in the code; **UNKNOWN** = the code does not determine it. Abbreviations: `LHD` = `RackCad.Plugin/Drawing/LateralHeaderDrawer.cs`; `SBW` = `RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs`; `BP` = `RackCad.Plugin/Drawing/BlockPlacement.cs`; `RBD` = `RackCad.Plugin/Systems/Shared/RackBlockData.cs`; `BLI` = `RackCad.Plugin/Drawing/BlockLibraryImporter.cs`; `RDC` = `RackCad.Plugin/Systems/Shared/RackDefinitionCreator.cs`; `RSC` = `RackCad.Plugin/RackSelectivoCommands.cs`; `SES` = `RackCad.Application/Systems/Selective/SelectiveEditorState.cs`; `RSW` = `RackCad.UI/Systems/Selective/RackSelectiveWindow.xaml(.cs)`.

## 1. Scope linkage

The kind covers exactly the scope of BA-03 V4: `SEL-FRONTAL` (one view per fondo `k`, `0 <= k < SelectiveDepthLayout.Count(system)`) and `SEL-PLANTA` (one whole-rack view). **The lateral and the legacy frontal `Section = -1` (`SEL-LEGACY-M1`) are out; the planta, whose canonical section is `-1`, is in** (BA08-10). `Count(system) = Math.Max(1, system?.DepthCount ?? 1)` (`Application/Systems/Selective/SelectiveDepthLayout.cs:18`); `MaxDepthCount = 4` (`Domain/Systems/Selective/SelectiveRackDefaults.cs:40`).

## 2. Seam authority references SL-1..SL-6

| Seam | Role | Required by scope | Current-code precedent | Status |
|---|---|---|---|---|
| SL-1 | frontal view definition (one per fondo, called in ascending `k`) in the caller's transaction | required | the product path creates it in its **own** transaction (`SBW:18-40`); AUTH-15 overload A (`RDC:39-45`) runs the same drawer method `LHD.CreateSystemBlock` in the caller's verified top transaction and has **no production caller** (evidence `B6` F1.02..F1.15; `sa3b` F1.8) | not implemented; AUTH-15 is an **operation-class precedent only** (AR2-38) |
| SL-2 | planta view definition in the caller's transaction | required | as SL-1 (the planta plan is also a `HeaderRunPlan`) | as SL-1 |
| SL-3 | lateral definition, own Selective contract | **not required** | lateral path `LateralHeaderDrawService` | equivalence EQ-1..EQ-5 `NOT_DEMONSTRATED` |
| SL-4 | top-level reference in the caller's transaction from a computed transform | required | jig placement in its own lock and transaction (`BP:174-201`); sets only `Position` (`sa3a` B1) | missing |
| SL-5 | PREPARE-W import with a structured result, from the opened private-copy `Database` | required | `BLI.EnsureForPlan` returns an `int`, always resolves the configured library path through the static cache, and swallows failures (`sa3b` F1.3; `sa4` CORE-07) | missing |
| SL-6 | read-only verifier and expected-versus-persisted comparator | required | readers exist (`RackBlockData.Read`, `RackBlockFinder.ScanEnvelopes`, `RackViewCodec`); no expected-versus-persisted comparator | missing |

**Accepted fact correction (PH2-CF-1; AR3-12).** The Architect accepted PH2-CF-1 as a **correction of fact**: V2.1 15.1 fact F-2 ("frontal and planta have no caller-owned seam") is incomplete, because AUTH-15 overload A is a caller-owned creation primitive that accepts any `HeaderRunPlan` (`RDC:39-45`; the frontal and planta builders return that type) and has no production caller. Row AR3-12 of the decisions log (section 214) **is** the correction; no errata is needed. The design clauses of V2.1 15.3 are unchanged, **no AUTH-15 evidence transfers**, P-8 remains required, and AR2-38 applies. The differences below between the product path and AUTH-15 overload A (evidence `B6` T2, `sa3a`, `sa3b`; code at `3375aadb`, unchanged at `1304101d` except the `Name` check) are among the reasons no evidence transfers:

| Aspect | Product path | AUTH-15 overload A | Mirror requirement |
|---|---|---|---|
| import | inside the creation callee, before its transaction (`SBW:25`) | none | PREPARE-W (SL-5), before `T_M` |
| missing pieces | skipped (no reference created, no placeholder); **reported to the user on insert** ("Bloques no definidos en el dibujo (omitidos)"); **not reported** on the `RACKEDITAR` redraw nor in a variable propagation; creation **not failed** (`sa3b` F1.1, F1.4..F1.7) | the drawer skips them while it writes the definition in the caller's transaction; overload A then returns the typed `MissingLibraryBlocks` and writes no envelope (`RDC:67-72`): the failure is detected **after** writing | typed failure, PRE-WRITE when detectable from the declared requirements, otherwise POST-WRITE (V2.1 15.3, 15.4; compiled row C-136 of BA-06 V4) |
| missing catalog rows | some pieces are dropped by the builders before the drawer (planta largueros and separators without a row, tarimas, safety elements, the planta celosía with a blank block), never reported (`sa3b` F1.9) | same builders | the plan requirement set is what the seams receive; the drop happens before and is part of the plan (section 7) |
| envelope input | the command composes a `RackEmbedDocument` and serializes it **once** (`RSC:405-417`, serialization at `RSC:416`); `SBW.CreateBlock` receives that string and writes it **unchanged** | takes a `RackEmbedDocument` **object**, not bytes, and serializes it **itself** with `RackEmbedStore.Serialize` (`RDC:187`; the same statement at `1304101d`, line 186); it never receives caller bytes | caller-composed envelope written verbatim (C-127; PA-8 unknown members) |
| envelope validation | none | refuses a blank `Id`, `Kind` or `Name` at `3375aadb` (`RDC:177-183`); a blank `Id` or `Kind` at `1304101d` (AUTH-15-C1) | `InvalidEnvelope` PRE-WRITE (C-rows of 15.4) |
| envelope write | written only when the string is non-empty, **no read-back** (`SBW:31-34`) | writes **its own serialization** and reads it back with an ordinal compare **against that serialization** (`RDC:204-206`) | verbatim write, read-back and compare with the caller-composed envelope (C-127..C-129) |
| naming | `RackViewBaseName` + `UniqueBlockName` | requested name passed to `UniqueBlockName` | effective names computed in PREPARE-R and returned; nested names follow section 15 |
| transaction | own `StartTransaction`, `Commit` | the caller's transaction must be the top transaction of the target database, compared by `TopTransaction.UnmanagedObject` (`RDC:159-164`) | typed authority `IsTopTransaction` (V2.1 15.3, 4.2) |
| placement | jig, own lock and transaction | none | SL-4 in `T_M` |

**C-127 and the two current paths (D-02).** Neither current path is taken as meeting C-127..C-129. AUTH-15 writes its own serialization of an object it receives, never the caller's bytes, so it cannot show that what it writes is the caller's composed envelope byte for byte (unknown members under PA-8 included). The product seam writes the command's string unchanged, but only when it is non-empty and without read-back. Whether a future seam that receives an object and serializes it can meet C-127 is left to the seam design (PA-8); this artifact asserts C-127 conformance for neither current path. This is one more reason why no AUTH-15 evidence transfers.

## 3. What creating a Selective view produces (FACT unless marked)

| Item | Fact | Evidence |
|---|---|---|
| definition names | frontal: `LinkedSelectiveFrontal(name, fondo, fondoCount)` then `Standard` (base name = the rack name or `Selectivo`); planta: `name + " - planta"` or `"Selectivo planta - {N} frentes"`; sanitized; uniquified by `_n` suffixes against the live block table; origin `(0,0,0)` | `Application/Systems/Shared/RackViewBaseName.cs:22-37,186-191,202`; `Application/BlockNaming.cs:12-29`; `LHD:240-247,395-413`; `sa3a` B3 |
| nested definitions | frontal `SEL_FRONTAL_<Role>_<n>` (buckets of two or more identical pieces); planta `PLANTA_CAB_<n>` (one per distinct frame, even a single one); requested names **restart per plan**; effective names depend on the block table at the moment of the call (section 15) | `sa3b` F5.1, F5.2 and the thresholds noted by its verifier |
| empty nested definitions | when a grouped piece's block is absent, the nested definition is still created (empty or partial) and referenced at every placement | `sa3b` (verifier, "missed" item 1): `LHD:38-51,56-67` |
| entity types | `BlockReference` (pieces and nested group placements), `DBText`, `RotatedDimension`; no attributes. **A negative scale can appear on any reference inside the view definition or inside a nested definition**: piece references carry `MirroredX`/`MirroredY` as scale signs (`LHD:311-313`) and group placements carry `Mirrored` as an X scale of -1 (`LHD:60-63`, `LHD:145-148`). **The top-level reference never has a negative scale**: today it gets only `Position` (`BP:174-201`), and the mirror's SL-4 accepts only a strictly positive determinant (V2.1 15.4, 15.6) | `sa2` F05-G03, F05-S03..S06; code lines cited |
| catalog `blocks.csv` | **exists** in the repository (94 rows: 30 FRONTAL, 35 LATERAL, 29 PLANTA); copied to the build output and resolved next to `RackCad.Application.dll`; the runtime copy can differ from the repository file, so its deployed bytes are pinned by the tuple field `CATALOG_FOLDER` (V2.5) | evidence `B3` F01, F24..F26 |
| library drawing `blocks-library.dwg` | **not versioned**; path from user settings, else next to the catalogs | `sa3b` F7.7 |
| dynamic-block parameters | FRONTAL: `LONGITUD`, `PERALTE`, `ALTURA`, `SAQUE`, `FRENTE`; PLANTA: `LONGITUD`, `PERALTE`, `SAQUE`; **`FONDO` only in the lateral view** (out of scope); applied only to catalog piece references, never to the references of the nested definitions; names matched case-insensitively; a name the block does not expose is silently ignored; the value is assigned through `property.Value` (`LHD:436`) | `sa3b` F3.1..F3.5 (BA08-09) |
| layers | `RACKCAD_ANOTACIONES` (ACI 2) and `RACKCAD_COTAS` (ACI 1), created lazily only when a text or dimension is materialized and only if absent; only `Name` and `Color` are set | `sa3a` L1..L4 |
| envelope | `RackEmbedDocument` in an `Xrecord` under `RACKCAD_SELECTIVE` in the extension dictionary of the **definition**; text chunks (DXF code 1) of at most 255 UTF-16 units | `RBD:16-52`; `sa3a` N3 |
| NOD / project variables | creation neither reads nor writes `RACKCAD_PROJECT` or `RACKCAD_CUSTOM_PROPERTIES`; `RACKEDITAR` reads `RACKCAD_PROJECT` (never writes it) | `sa3a` N1..N7 |
| commands | **35** `[CommandMethod]` attributes in `src` (corrects the count 34 of evidence `B7` C1, BA08-08); no `CommandFlags`; no RACKMIRROR command | `sa3b` F2.1, F2.2 |
| plan mutability | `HeaderRunPlan` and `HeaderGroup` store the caller's list references; the runtime collections are `List<T>`; `HeaderBlockInstance` has public setters and a mutable `DynamicParameters` dictionary; **the plan types are mutable** | `sa3b` F4.1..F4.5 (BA08-02) |

**Catalog folder in the tuple (PH2-TP-1 accepted; AR3-13; V2.5).** The catalog folder is an input of the product path of the exact build, editable in place, and its cache key is name, size and write time with no content hash (`sa3b` F8.3). V2.5 section 1 (the Architect's pre-approved text) adds the BUILD_BOUND field **`CATALOG_FOLDER`**: the resolved catalog directory path of the build under test and the SHA-256 of the deployed bytes of every `*.csv` and `*.json` file directly in that directory (the scope of the provider's signature, `Application/Catalogs/JsonRackCatalogProvider.cs:220-247`), sampled before the library acquisition with the other BUILD_BOUND fields and re-checked after CP; a difference is a tuple mismatch (V2.1 2.2: `INVALID`). The resolved directory is what `CatalogDirectory.Resolve()` returns: the `catalogs` folder next to `RackCad.Application.dll`, else under `AppContext.BaseDirectory` (`Application/Catalogs/CatalogDirectory.cs:19-38`); the field records which one it was.

### 3.1 Writer-behaviour constants (for `ORACLE_SPEC`, BA08-03; evidence `sa3a`)

| Constant | Value | Evidence |
|---|---|---|
| annotation layer | `RACKCAD_ANOTACIONES`, ACI 2, created only if absent, only `Name` and `Color` set | `LHD:270,323-331`; `LayerHelper.cs:16-29` |
| dimension layer | `RACKCAD_COTAS`, ACI 1, same rule | `LHD:354-357` |
| text | `DBText`; height = `TextHeight` when > 0 else 3.0; `HorizontalMode = TextCenter`, `VerticalMode = TextVerticalMid`; `Position` = `AlignmentPoint` = the anchor (Z = 0); `AdjustAlignment` after append; **no** text style, rotation, width factor, oblique, colour, linetype or lineweight set | `LHD:265-281`; `sa3a` T1, T2 |
| Selective text sizing | base height `6 x scale` (scale <= 0 treated as 1); rack-name label `1.5 x h`; margin 4 in; label positions as in `sa3a` T5 (frontal) and T6 (planta); the rack-name label only when `DrawRackName` is on **and** the name is not blank | `Application/Systems/Selective/SelectiveAnnotations.cs:15-32`; `SelectiveFrontalBuilder.cs:342`; `SelectivePlantaBuilder.cs:338` |
| dimension geometry | `RotatedDimension` with `p1 = Insertion`, `p2 = ConnectionAnchor`; horizontal when `abs(dy) <= abs(dx)`, rotation 0 or pi/2; dimension-line point at `(midX, p1.Y + offset)` or `(p1.X + offset, midY)`; text `""` | `LHD:338-360`; `sa3a` D1 |
| dimension style | the named style (`system.DimensionStyle`) used as-is when present in the drawing; otherwise the drawing's current style with the automatic overrides; never created; a missing named style falls back silently | `LHD:351-354,378-392`; `sa3a` D2 |
| automatic overrides | `Dimscale = 1.0`, `Dimtxt = h`, `Dimasz = 0.7h`, `Dimexe = 0.4h`, `Dimexo = 0.4h`, `Dimgap = 0.3h`, `Dimtad = 1`, `Dimdec = 2`; `RecomputeDimensionBlock(true)` always | `LHD:362-375`; `sa3a` D3 |
| Selective dimension constants | chain gap 14, overall gap 22, elevation step 14 (times scale); offsets negative; zero-length segments (< 1e-6) skipped; per-view detail gating | `SelectiveDimensions.cs:22-30,60-130,205-264`; `sa3a` D4..D6 |
| nested and top-level references | **no** `LayerId`, colour, linetype or lineweight set; the top-level reference gets only `Position` (no rotation, scale or normal) | `LHD:60-63,145-148,308-314`; `BP:174-201,240-244`; `sa3a` L7, B1 |
| unexposed dynamic parameters | skipped with no log or message; `RecordGraphicsModified(true)` only when at least one value was applied | `LHD:415-449`; `sa3a` P1 |

## 4. Sealed list (`VERIFY_ELEMENT_LIST`) and its coverage

| Class | Element | Layer | Covered by scenarios (BA-04 V4) |
|---|---|---|---|
| SL-D | each destination view definition | SEM | `CL-CLEAN-*`, `WR-A-*`, `WR-E-*` |
| SL-N | each nested definition the seam creates | SEM | `CL-CLEAN-FX2F-ALL`, `CL-CLEAN-FX4F-ALL`, `WR-E-*` |
| SL-R | each top-level reference: definition name, position, scale, rotation, normal, and the **ST-13 presentation set** (`LayerId`, `Color`, `Linetype`, `LinetypeScale`, `LineWeight`, `Transparency`, `Visible`; `PlotStyleName` is a declared gap, section 11 rule 1, pending AQ-V4-09) (V17 ST-13; AR3-11; D-07) | SEM | `CL-CLEAN-*` |
| SL-E | each envelope, including `ExtensionData` and unknown members | SEM | `CL-CLEAN-*`, `WR-A-*` |
| SL-A | view address and grouping (`Embed.Id` = `NewRackId`) | SEM | `CL-CLEAN-FX1F-FP`, `CL-CLEAN-FX2F-ALL`, `WR-D-*` |
| SL-Y | the RackCad layers the created entities use: **reused** (present) or **added** (absent) | SEM | reused: `CL-CLEAN-FXANN-FP`, `CL-CLEAN-FXDIM-FP`; **added: `CL-CLEAN-FXANNBLANK-FP`** on `FX-ANN-BLANK` (AR3-15; section 4.1); operation class: `EV-L-OC-LAYER` (BA-06 V4) |
| SL-S | source read-set | EXA | `WR-G-*`, `NC-E08-Y0`, `NC-E08-Y2` |
| SL-P | the **consumed registry state** of `RACKCAD_PROJECT`, **including `ABSENT`** (AR2-15) | EXA | `WR-F-*` (the creation `ABSENT -> present`; the fixtures have no entry) |
| SL-C | containers (block table names, model-space handles, sibling set, extension dictionaries) | EXA | `WR-D-*`, `WR-E-*` |

**SL-4 transform refusal (not SL-R coverage).** `KS-SL4-NEG-DET` (BA-04 V4) exercises the SL-4 typed refusal `InvalidTransform` (V2.1 15.4, 15.6), induced at the SL-4 entry. It aborts at PRE-WRITE, before PV (`REACH PS`), so it compares no SL-R element and is not listed as SL-R coverage. Its group membership is BA-04 V4's and is pending AQ-V4-03.

### 4.1 SL-Y "added": constructions and residual (AR3-15; replaces the V3 sealed reason)

The V3 sealed reason ("no layer is added unless a RackCad layer was renamed or removed outside RackCad") is **withdrawn**. Its premise, that the mirror's layers are decided by the same inputs the source used, is false for the name: the mirror's logical name is `<base> - espejo` (V17 PDC-2 at `proposal-v17.md:352`, ID-4 at `:1060`, S-17 at `:1768`, T-M26 at `:3755`), which is never blank; the rack-name label is drawn only when `DrawRackName` is on and the name is not blank (`SelectiveFrontalBuilder.cs:342`, `SelectivePlantaBuilder.cs:338`); and `RACKCAD_ANOTACIONES` is ensured only for a non-blank label (`LHD:258-270`; `sa3a` L4, S4 CONFIRMED; S6 point (b), upheld by its verifier).

| Construction | Reachable with product operations | Status |
|---|---|---|
| a **blank-named** source with `DrawRackName` on, `NumberFronts` and `NumberLevels` off and `Dimensions = None`: the source draws no label and never creates `RACKCAD_ANOTACIONES`; its mirror draws `<base> - espejo` and creates the layer in `T_M` | **yes**, with no rename or removal: a blank name is a normal product state (the editor's name box has no default text and no validation: `RSW.xaml:33-34`, `RSW.xaml.cs:2587`; `RSC:459` sets `system.Name = name`; only the block names fall back to `Selectivo`); the delta review (D-01 verification) found no V17 rule that refuses a blank-named source | fixture **`FX-ANN-BLANK`** (section 12); governing clean scenario **`CL-CLEAN-FXANNBLANK-FP`** (BA-04 V4; `BLOCKED_BY` K-3 and K-6 = AQ-V4-07, section 11.2 HDM-8) |
| a RackCad layer renamed or removed outside RackCad after the source was drawn | only by a non-RackCad operation | **disclosed residual** (not a fixture; admissibility under V17 L-29/L-33 is not decided here) |
| a mirrored view of a kind the source did not draw, when every drawn source view emitted nothing (`sa3a` S6 point (a), as corrected by its verifier) | depends on whether the I-52 authority lets a mirror draw a view kind its source lacks; not decided here | **disclosed residual** |
| an empty scratch template (no RackCad content, the layer absent) | not a mirror of a product-drawn rack | operation class only: for SL-Y "added", `FX-SCRATCH` serves **only** the isolated learning construction `EV-L-OC-LAYER` (BA-06 V4 sections 3.3 and 10.2). The `SH-LAY-ADDED` dynamic-trace family and its validation run on `FX-ANN-BLANK`, not on this template (BA-06 V4 sections 7 and 10.2) |

The oracle predicts the added layer by the rule of section 3.1 (created only if absent; `Name` and `Color` set: ACI 2 for `RACKCAD_ANOTACIONES`, ACI 1 for `RACKCAD_COTAS`). Its other record fields are in the scope of `HOST_DEFAULT_MAP` (section 11.2); their source is the open Architect question AQ-V4-07 (Q-HDM-1; section 16.1, K-6).

## 5. `PROTECTED_OBJECTS` and the phase-A domain (V2.2 PA-10)

Phase A adjudicates events on the domain of V2.2 PA-10:

1. the **objects protected at plan time**: SL-S (the source read-set), SL-P (the consumed NOD state), the pre-existing containers of SL-C, **and every pre-existing record of the sealed list that is known from the plan: each reused SL-Y layer** (AR3-15, D-04). A layer is known as reused when the plan capture (section 11.1) contains an instance that the drawer materializes on it (a non-blank `Annotation` for `RACKCAD_ANOTACIONES`, a `Dimension` for `RACKCAD_COTAS`) and the `PRE_COMMAND_STATE_RECORD` holds that layer `PRESENT`, with its handle;
2. the **objects created inside `T_M`**: the handles the seams return for SL-D, SL-N, SL-R, SL-E and for every SL-Y layer they create (section 15).

At PV the sealed `PROTECTED_OBJECTS` is checked for consistency with that domain (PA-10): a member of the sealed set outside the domain aborts before `Commit()`. With part (1) as above, the reused layers of `CL-CLEAN-FXANN-FP` and `CL-CLEAN-FXDIM-FP` are inside the domain, so a correct run of those clean scenarios passes the check; a reused layer the plan does not reveal cannot occur, because the sealed list takes SL-Y from the same plan and record. Events outside the domain are recorded and not adjudicated. Anonymous blocks derived by the host (`*U`, `*D`) are outside the definitions list and inside the domain only through their owning reference or dimension.

## 6. Abort read-set: component specifications (readers are execution blockers)

The specification of each component is **fixed here**; the readers and comparators marked MISSING are **execution blockers** (EXEC-4), not baseline blockers. Every component is a member of the `PRE_COMMAND_STATE_RECORD` (V2.1 6; BA-05 V4 section 2.3.5), with its record kind and, for an `ENUMERATION`, its item form (BA-05 V4 section 2.3.6). The one exception is the row "view addresses and grouping": it has **no member of its own**, because it is carried by the `PAYLOAD` members of the row "RackCad payloads" (BA-05 V4 section 2.3.5).

**`CONTEXT` members (pending AQ-V4-08).** BA-05 V4 section 2.3.5 leaves to the kind baseline whether ABORT-VERIFY or E-08 compares a member of category `CONTEXT` (here: the source reference presentation and the consumed context variables, captured for the oracle, AR3-11). Whether those members are part of `EXPECTED_AFTER_ABORT` (V2.1 11) at all is the open Architect question **AQ-V4-08**; it is not decided here. This artifact's **proposed answer** is: **ABORT-VERIFY compares both kinds** (the mirror changes no drawing variable and not the source reference, so any change is an unexpected mutation); **E-08 compares the `REFERENCE` members** as part of the source read-set and does not compare the `CONTEXT_VARIABLE` members. A ruling that differs changes this section before the candidate (K-6, section 16.1).

| Component | Compared value | Layer | Scope | Reader status |
|---|---|---|---|---|
| block table names | the full name set (stored case) | EXA (`ENUMERATION`, itemForm `NAME`, `ordered = 0`: the encoder sorts; BA-05 V4 section 2.3.6) | the whole block table | MISSING |
| definitions | `DEFINITION_DUMP` of every RackCad-owned definition, every manifest definition and every pre-existing homologue of the static closure | EXA | those definitions | MISSING (fingerprint reader) |
| model-space content | the handle set (`ENUMERATION`, itemForm `HANDLE`) and the RackCad reference set with class (`ENUMERATION`, itemForm `HANDLE_CLASS`: the handle, a colon and the exact host class name) | EXA (`ENUMERATION`, session-local; BA-05 V4 section 2.3.6) | model space | MISSING |
| source reference presentation | the ST-13 set of each source reference (`LayerId`, `Color`, `Linetype`, `LinetypeScale`, `LineWeight`, `Transparency`, `Visible`) as a `CONTEXT` member of record kind `REFERENCE` (the exact `ENTITY_INSERT` of the reference, BA-05 V4 section 2.3.5); `PlotStyleName` is outside it (declared gap, section 11 rule 1, pending AQ-V4-09); the source of SL-R expectations | EXA | every source reference | MISSING |
| consumed context variables | the values of `CLAYER`, `CECOLOR`, `CELTYPE`, `CELTSCALE`, `CELWEIGHT`, `CETRANSPARENCY`, `CPLOTSTYLE` (one `CONTEXT` member of record kind `CONTEXT_VARIABLE` each, BA-05 V4 section 2.3.5) and the identities of the current text style and dimension style (their `STYLE` and `DIMSTYLE` records, row "symbol tables"): the keys of `HOST_DEFAULT_MAP` (section 11.2; AR3-11) | EXA | the document | MISSING |
| RackCad payloads | `PAYLOAD_EXACT` of **every definition** that holds a RackCad payload (one `PAYLOAD` member each; it also carries the view address and grouping of the definition, next row); set of `RackId` (expected: no new id) | EXA | **every definition of the block table** (BA08-05, AR2-40); `ScanEnvelopes` skips layouts, anonymous and xref-owned records, so the **full-scope reader is an execution blocker (EXEC-4)** | MISSING (full-scope reader); `RBD.Read` exists |
| view addresses and grouping | **no separate member**: the persisted address (`Kind`, `View`, `Section`) and the grouping identity (`Id` = `Embed.Id`) are envelope members inside the exact `RB` sequence of each view definition's `PAYLOAD` member (row "RackCad payloads"; BA-05 V4 section 2.3.5), so a change to them changes that digest. The decoded address (`RackViewCodec`) is **only** an input of the oracle (SL-A) and of the SL-6 comparison, not a layer-1 record | EXA (through the `PAYLOAD` members) | as row "RackCad payloads" | no reader of its own (the reader of row "RackCad payloads"); codec exists (`RackViewCodec`) for the oracle and SL-6 |
| library residue | the ADD set with its closure | EXA | manifest | MISSING |
| symbol tables | name sets (each an `ENUMERATION`, itemForm `NAME`, BA-05 V4 section 2.3.6) of layers, linetypes, text styles, dimension styles and registered applications; `SYMBOL_RECORD_*` of: each pre-existing record of the closure; `RACKCAD_ANOTACIONES` and `RACKCAD_COTAS`; **every layer the created entities land on** (the layer named by `CLAYER` and every layer `HOST_DEFAULT_MAP` resolves); the current text style and dimension style **and their transitive records** (the linetypes, text styles and arrow blocks they reference) | EXA | those records (BA08-05) | MISSING |
| project variables | `NOD_ENTRY` of `RACKCAD_PROJECT` and `RACKCAD_CUSTOM_PROPERTIES`, or `ABSENT` | EXA | the NOD entries named | readers exist (`ProjectVariablesData.Read`, `CustomPropertiesData.Read`); aggregate MISSING |

### 6.1 Normative rule for PREPARE-R and the warm-up (BA09-03, AR2-42, AR3-16)

The mirror's PREPARE-R and the warm-up **never** call `AutoCadExternalLibraryBlockQuery`, `BlockLibraryImporter` or `BlockLibraryDatabaseCache` (the library query of AUTH-12 and the process cache). The library content is read only from the per-run private copy `Database` that the orchestrator opens and disposes (V2.1 19.1); the warm-up uses its **own** warm-up-only copy and `Database`, never entered into the run cache or the `LibraryInputRecord`, disposed before step 4. Reason: the cache is a single process-wide slot, and acquiring any other path evicts and disposes the cached database (evidence `sa4` CORE-02, CORE-07).

**Static guard (D-09; K-9).** A static guard asserts that **the seams, the orchestrator (PREPARE-R included) and the warm-up harness** reference none of those three types, and that the warm-up harness constructs no `JsonRackCatalogProvider`, calls no `RackCatalogLoader` and calls no `UserSettingsStore` (BA-09 V4 section 6, rows 1 and 4; AR3-16). The guard is an **additional execution prerequisite derived from AR2-42 and AR3-16**; it is **not** EXEC-12, which V2.1 23 limits to the P-5 guards (no `Commit`, `Abort`, `Dispose`, `LockDocument`, `OpenCloseTransaction` or side-database write; `ReadBlockName` removed).

## 7. Static dependency closure and the required-block set

**Extractor (BA08-11).** The plan requirement set is `RackBlockRequirementExtractors.HeaderRun.Extract(HeaderRunPlan)` (`Application/Systems/Shared/LibraryBlockRequirements.cs:45-80`): every `BlockName` of the loose instances and the header-group instances, blank names skipped, de-duplicated case-insensitively. **The PREPARE-R closure input is that set.**

**Census universe (AR3-17).** The library census covers **every block definition of the library file** (BA-05 V4 section 5), a superset defined without reference to this artifact. The static universe below is the set of names a Selective plan can require with the bound catalogs; it serves the closure rule and the fixtures of this artifact, and a name of it that the census does not find is absent from the library.

**Static universe (evidence `sa3b` F7.3, F7.4; names resolved through `blocks.csv` by piece id and view).** With the catalogs of the bound SHA a FRONTAL plan can require only these **23** names and a PLANTA plan only these **23** (disjoint; union **46**):

| Plan | Names |
|---|---|
| FRONTAL (23) | `POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA_FRONTAL`; `PLACA_BASE_DE_CABECERA_ATORNILLABLE_DE_PLACA_CALIBRE_3_16_FRONTAL`; `LARGUERO_ESCALON_CAL14_3_REMACHES_FRONTAL`; `LARGUERO_IN_OUT_C6_FRONTAL`; `LARGUERO_ESCALON_INFINITO_FRONTAL`; `LARGUERO_ESCALON_TROQUEL_REDONDO_FRONTAL`; `PROTECTOR_BOTA_H_3_16_18_FRONTAL`; `PROTECTOR_BOTA_C_4_FRONTAL`; `PROTECTOR_BOTA_C_6_FRONTAL`; `PROTECTOR_LATERAL_BOTA_H_3_16_18_FRONTAL`; `PROTECTOR_LATERAL_BOTA_C_4_FRONTAL`; `PROTECTOR_LATERAL_BOTA_C_6_FRONTAL`; `LARGUERO_ESCALON_TOPE_DE_3_FRONTAL`; `POSTE_3_1_5_8_TOPE_FRONTAL`; `DESVIADOR_A_3_FRONTAL`; `DESVIADOR_A_4_FRONTAL`; `DESVIADOR_L_3_FRONTAL`; `DESVIADOR_L_3_5_FRONTAL`; `DESVIADOR_L_4_FRONTAL`; `DESVIADOR_L_4_5_FRONTAL`; `DESVIADOR_L_5_FRONTAL`; `TARIMA_GENERICA`; `PARRILLA_GENERICA_FRONTAL` |
| PLANTA (23) | `POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA_PLANTA`; `PLACA_BASE_DE_CABECERA_ATORNILLABLE_DE_PLACA_CALIBRE_3_16_PLANTA`; `TRAVESANO_PARA_POSTE_OMEGA_DE_CINTA_CALIBRE_14_PLANTA`; `LARGUERO_ESCALON_CAL14_3_REMACHES_PLANTA`; `LARGUERO_IN_OUT_C6_PLANTA`; `LARGUERO_ESCALON_INFINITO_PLANTA`; `LARGUERO_ESCALON_TROQUEL_REDONDO_PLANTA`; `SEPARADOR_DE_CABECERA_FORMADA_DE_CINTA_CALIBRE_12_PLANTA`; `PROTECTOR_BOTA_H_3_16_18_PLANTA`; `PROTECTOR_BOTA_C_4_PLANTA`; `PROTECTOR_BOTA_C_6_PLANTA`; `PROTECTOR_LATERAL_BOTA_H_3_16_18_PLANTA`; `PROTECTOR_LATERAL_BOTA_C_4_PLANTA`; `PROTECTOR_LATERAL_BOTA_C_6_PLANTA`; `LARGUERO_ESCALON_TOPE_DE_3_PLANTA`; `POSTE_3_1_5_8_TOPE_PLANTA`; `DESVIADOR_A_3_PLANTA`; `DESVIADOR_A_4_PLANTA`; `DESVIADOR_L_3_PLANTA`; `DESVIADOR_L_3_5_PLANTA`; `DESVIADOR_L_4_PLANTA`; `DESVIADOR_L_4_5_PLANTA`; `DESVIADOR_L_5_PLANTA` |

**Catalog files that determine the names (SHA-256 of the repository blobs at the bound SHA; path `assets/catalogs/`):** `blocks.csv` `5ef806529b2dab33b057101dd344f2f250919ae7eda121bb88ae7c836f7318fb`; `secciones.csv` `94baea1be03a65ad4c74c90e6038251eafde7fd205b0d7cde79a91a7191449a5`; `base-plates.csv` `7f8d924296af0f834db821ba0a87e72e8340530155066be54996000de6af239b`; `seguridad.csv` `f438600d3e59075dfb62333b46bb88e6f8ae843bdc3f2aae3328921b5d737059`; `defaults.json` `fb06ea01e8d52b38c441cbcccfc73b8df1b2c1333637f010dbf6b128717279cf`. Geometry-only files that can gate the planta celosía: `connection-layout.csv` `5c63da5b64c63e3d3bc6687d53f481cf49a20957a61e3d0c4674bd46fbfa3d37`; `connection-points.csv` `473f0c205fb9fdb786ed445d4fd67dde9f596440d7d723d92d30deab47ec44f8`. (Recomputed for V4 by a script: SHA-256, lowercase hex, of the bytes of `git show 3375aadb:assets/catalogs/<file>` with CRLF normalized to LF; the seven blobs contain no CR; the values equal V3.)

**Rule (D-08; AR3-13).** The values above identify the **repository content** from which the name universe is derived; they are **not** the deployed bytes. The repository works with `core.autocrlf=true` and `.gitattributes` at `3375aadb` marks only the structural-section files `-text`, so a deployed copy of these files can have CRLF line ends and other hashes. The deployed bytes are pinned separately by the tuple field **`CATALOG_FOLDER`** (V2.5), which covers the resolved directory and **every** `*.csv` and `*.json` directly in it (the provider reads them all, not only the seven above), and whose resolution can fall back to `AppContext.BaseDirectory` (section 3). What is compared is the **derived name universe**: before the census and before the seal of the scenario catalog, the 46 names are re-derived from the catalog folder of the exact build (the files `CATALOG_FOLDER` pins) and compared with this table; a difference makes this table a new version.

**Clean fixtures require** (FRONTAL) the post, the plate (with `DrawBasePlate`) and the chosen beam's `LARGUERO_*_FRONTAL`; (PLANTA) the post, the plate, the celosía, the first level's `LARGUERO_*_PLANTA` and, with two or more fondos and a gap, the separator.

**Closure rule (normative for the kind).** In PREPARE-R, from the private library copy, read-only: for every name of the plan requirement set, collect the block definition, the transitive closure of its nested definitions, and the symbol records they reference; classify each member `ADD`, `REUSE_AS_IS`, `REUSED_BOUND` or `REJECT` with host symbol-table semantics and stored case (V2.1 11.3, V2.2 PA-7); an unclassifiable member makes the closure `UNKNOWN` and refuses the run (O1). The annotation and dimension layers are added by the seam in `T_M` (SL-Y), not by the import. The **contents** of the library are UNKNOWN until the census (BA-05 V4 section 5), which needs host work.

## 8. E-04: designated route (for Coordinator ratification)

| Route | Designation |
|---|---|
| **R-1** typed command at the command line | **DESIGNATED** (the only route) |
| R-2 menu or ribbon, R-3 script, R-4 LISP, R-5 `SendStringToExecute`, R-6 direct API call | **UNSUPPORTED** |

**Delivery in a governing run (PH2-E4-1 accepted with conditions; AR3-14).** A governing run forbids Owner touch (V2.1 21.5), so R-1 is delivered by the **governed control plane** as keystrokes to the command line: allowed automation of the governed control plane (the phase-2 review refuted the objection, BA08-06). The `CMDACTIVE` bits are recorded as observations and are **not** used as the identity of the route. A script (`.scr`) delivery is R-3 and is not used. The five acceptance conditions of the Architect are normative for the route:

| Id | Condition (AR3-14) |
|---|---|
| E4-C1 | the delivering component and its mechanism are declared, recorded and hashed as part of the governed control plane (V2.1 21.5: "part of the contract and recorded") |
| E4-C2 | D-4 is written by the delivering control plane at delivery time (V2.1 3.3: recorded, not inferred) |
| E4-C3 | `Q-ROUTE-R1` shows that the host readings under keystroke delivery carry no script, LISP, DDE or transparent signature; this is a **necessary condition only**, not a proof of the route |
| E4-C4 | every difference between control-plane keystrokes and a human typing that the host could expose is disclosed (Owner Act 2 disclosures) |
| E4-C5 | the Coordinator's ratification of R-1 and of its delivery (K-5) remains required |

**Source of the E-04 pin (BA08-13, AR2-11).** The admitted values of D-1, D-2, D-3, D-5 are pinned from the non-governing `E4-LEARN-R1-*` runs under the selected `LOCK_MODE` (characterization build in the mode of AR3-01), never from a governing run or from hindsight. The membership condition of V2.1 3.3 over governing runs is checked **offline**: every pinned member must be observed in every governing R-1 run at its checkpoint; otherwise the E4 group verdict is `UNKNOWN` (the pin is not revised). Facts: 35 `[CommandMethod]` entries, no `CommandFlags`, no `SendStringToExecute`, `LispFunction` or context-switching API in `src`.

## 9. `LOCK_MODE` candidate set (no winner)

LM-1 (parameterless `LockDocument()`: every product site; default mode not visible in the code) and LM-2 (`DocumentLockMode.ExclusiveWrite`: no product site) are the candidate set, **sufficient for the baseline**; LM-3 only through a reviewed amendment. Selection follows V2.1 3.4 by review over the non-governing `LK-*` characterization (AR2-11; characterization build, mode of AR3-01); `NO_CANDIDATE` gives `LK = FAIL`. The `HDM-CHAR-*` runs of section 11.2 come after the selection (AR3-11).

## 10. Semantic comparison: tolerance and normalization (the values of V2.2 PA-9 live here; BA-05 V4 names the slots only; AR3-17)

The slots are exactly the eight that BA-05 V4 section 3.1 names: `TOL_LENGTH`, `TOL_TEXT_HEIGHT`, `TOL_DIM`, `TOL_ANGLE`, `NORM_ANGLE`, `TOL_NORMAL`, `TOL_DYNAMIC`, `TOL_SCALE`. This table gives each its value or rule, its authority and its status; BA-05 V4 states none of them.

| Slot | Value | Authority |
|---|---|---|
| `TOL_LENGTH` | **1e-9 in**, absolute | `GeometryTolerance.Length` (`Application/Geometry/Vector2D.cs:93`); V17 4.3 |
| `TOL_ANGLE`, `TOL_NORMAL` | **1e-9 rad**, absolute | `GeometryTolerance.Angle` (`Vector2D.cs:100`); V17 4.3 "rotaciones, Normal (ST-5)" |
| `TOL_TEXT_HEIGHT`, `TOL_DIM`, `TOL_DYNAMIC` (lengths) | = `TOL_LENGTH` | V17 4.3 (length) |
| `NORM_ANGLE` | a **reduction modulo 2pi**: both angles are reduced to **(-pi, pi]** with the rule of `RackSourceTransformClassifier.NormalizePi` (`Application/Systems/Shared/RackSourceTransformFactsV2.cs:255-261`: the remainder of the angle modulo 2pi, then one shift by 2pi into (-pi, pi]); the difference of the two reduced angles is reduced the same way (the wrapped difference), and its absolute value is compared with `TOL_ANGLE`. So two angles that differ by 0.5 x `TOL_ANGLE` across the wrap at 2pi are `EQUAL` (BA-05 V4 vector SV-10) | code rule (evidence `B5` F16, T6) |
| **`TOL_SCALE`** | **UNSET — NO AUTHORITY FOR CT-21D** | V17 4.3: "Factor de escala: no existe"; the product value is **absolute, not relative**, and **fixed at G4 with test and record**. The phase-1 ruling requires a separate decision, test and record for CT-21D; the test needs host evidence (non-governing). **Not decided here** (AR2-35, K-3) |

**Slot-unit vectors of BA-05 V4 (AR3-17).** BA-05 V4 names the slots only and carries **no tolerance value and no normalization rule**: its semantic vectors SV-06, SV-07, SV-10 and SV-11 are written in slot units (expected + 0.5 x TOL => `EQUAL`; expected + 2 x TOL => `DIFFERENT`). Their binary64 instances are generated by the harness of `Q-FPSPEC-VECTORS` (BA-04 V4) from the sealed values of this table. The dependency therefore runs only BA-08 -> BA-05. `TOL_SCALE` is not a gate of BA-05; it is K-3 of this artifact.

**`TOL_CONTINUITY` (BA08-12, BA05-F09, D-11).** In CT-21D comparisons **no quantity is computed by normalization**. V17 4.3 (`proposal-v17.md:968`, and `:2643`) gives `GeometryTolerance.Continuity` (1e-7 in) to values written by a declared normalization, N-S02, which is the Selective medio frente. N-S02 is part of the correctness of `mu_k`, which is outside CT-21D (section 11 rule 4), and the fixtures exclude medio frente (section 12). The oracle derives every expected value from the sealed plan capture and the computed transform (section 11), so every length uses `TOL_LENGTH`. The V2 row `TOL_CONTINUITY` stays **removed**; `GeometryTolerance.Continuity` is not used by CT-21D.

Gate (V2.2 PA-9): agreed before `BASELINE_READY`; sealed before the first governing scenario that uses them; never derived from governing outputs; a change is a new baseline.

## 11. `ORACLE_SPEC` (independent of the writer and of `mu_k`)

**Inputs (BA08-02).** In **PREPARE-R, before any seam call**, the orchestrator takes the **plan capture** (section 11.1): a deep, canonical, hashed record of every plan it built for the mirrored design `D'` (the plan types are mutable, section 3), with the caller-composed envelope bytes, the computed top-level transform and the effective names of each view. Its digest is logged in the PR checkpoint record and sealed then. **PV only checks the sealed digest**; a **plan-equality check** asserts that the plan each seam received equals the sealed capture (the received plan is encoded the same way and the digests compared; a difference is an abort before `Commit()`). The other inputs: `NewRackId`, the `PRE_COMMAND_STATE_RECORD` (including the ST-13 set of each source reference and the keys of section 11.2), the pinned `HOST_DEFAULT_MAP`, and the constants of section 3.1.

**Rules.**

1. The oracle derives the expected typed form of each sealed-list element **from those inputs only**: SL-D and SL-N from the captured plan's instances and header groups (block name, insertion, rotation, mirror flags as scale signs, dynamic parameters, texts and dimensions) **and the writer-behaviour constants of section 3.1** (layer names and colours, text defaults and alignment, the dimension geometry rule, the style choice and the automatic overrides; the overrides are compared as the dimension stores them, the stored `DSTYLE` section of BA-05 V4 section 2.3.2, a stored-state reading whose confirmation is the open Architect question AQ-V4-10); **SL-R from the computed transform and the ST-13 set of its source reference** (V17 ST-13, `proposal-v17.md:918`: the new reference keeps, explicitly assigned, `LayerId`, `Color`, `Linetype`, `LinetypeScale`, `LineWeight`, `Transparency` and `Visible` of **its** source reference; `PlotStyleName` is preserved under named plot styles (STB) and, under colour-dependent styles (CTB), derives from `Color` and is then not an independent expected value; V17 L-33: the copy keeps the complete presentation of the source or is not created), every one read from the source reference into the `PRE_COMMAND_STATE_RECORD` (D-07; AR3-11 condition 2); SL-E from the caller's envelope bytes parsed by the authoritative reader; SL-A from the view address and `NewRackId`; SL-Y from the constants and the record (present or absent); SL-S, SL-P and SL-C from the PS observation and the record plus the expected additions. **Declared gap (plot style name; pending AQ-V4-09):** BA-05 V4 keeps the plot style name outside layer 1 (its sections 2.3.5 and 2.4), so neither the record nor the comparison covers `PlotStyleName`. Whether BA-05 adds the field before the seal or the gap stays declared here is the open Architect question AQ-V4-09; this artifact declares the gap and does not decide it. The fixtures use colour-dependent plot styles (`PSTYLEMODE = 1`, section 12.2), under which ST-13 derives the plot style from `Color`, so the gap does not arise in CT-21D; a drawing with named plot styles is outside the fixtures, and the gap is disclosed for it.
2. **Exposed dynamic parameters and value sets (D-13).** The writer ignores a plan parameter the block does not expose (section 3.1), so the expected dynamic-property set of a reference is the intersection of the plan's parameters with the writable properties the library block exposes; the exposed set comes from the library census (BA-05 V4 section 5) and is `UNKNOWN` until then (K-2). The writer assigns `property.Value` (`LHD:436`); a writable property with a **value set** (a list or an increment) can store a value other than the one assigned. The census therefore records, per dynamic property, the value set the host reports (its allowed values, or none) (BA-05 V4 section 5); a property whose value set the census could not read counts as constrained. The expected value of an **unconstrained** property is the plan value; the expected value of a **constrained** property is **`UNKNOWN` (O5 path)** until a non-governing record, cited by the census (K-2), shows the value the host stores for the plan value.
3. **Independence.** The oracle never calls the writer (`LateralHeaderDrawer`, `RackBlockData.Write`, the seams) and never reads what the writer wrote to compute an expectation; it reads the **sealed capture**, not the live plan objects. A T0 source guard (implementation gate) will assert that the oracle references no writer type.
4. **`mu_k` is given.** The oracle takes `D'` as input and does not evaluate `mu_k`; the correctness of the reflection is outside CT-21D (V-VIEW-ALL, G3).
5. **Host-default attributes (PH2-OR-1 accepted with conditions; AR3-11; D-06).** Attributes of created objects that neither the captured plan, nor the writer constants of section 3.1, nor the I-52 authority determine have no plan value. Their expected value comes from the pinned **`HOST_DEFAULT_MAP`**, under the pin rule of section 11.2. The ST-13 set of the top-level reference is **not** in its scope (rule 1). Until the map is pinned, the comparison of such an attribute is `UNKNOWN` (O5 path).
6. **Excluded from comparison.** Host-derived anonymous blocks (`*U`, `*D`) as definitions; a dynamic reference is compared by its parent name and property values.

### 11.1 `PLAN_CAPTURE_RECORD` (the sealed plan capture; D-12)

**Record.** One `PLAN_CAPTURE_RECORD` per logical mirrored rack, taken in PREPARE-R after the effective names and the transform are computed and **before** the first seam call. Its digest is logged in the PR checkpoint record (V2.1 5) and sealed.

**Domain (owned and versioned by BA-08).** The plan-capture records have their **own domain separator**, `CT21D-PLANCAP-V1|`: `stream = "CT21D-PLANCAP-V1|" + <record type> + "|" + fields`, with the fields in ordinal (UTF-8 byte) order of their names, each as `<name>` `=` `<value>` LF. This domain is owned and versioned by this artifact: before the seal of BA-08 a change of the record types below is a new `ARTIFACT_VERSION` of BA-08; after the seal any change is `CT21D-PLANCAP-V2` (a new domain separator) and a new baseline. The stream is **never** in the `FPSPEC-V1|EXACT|` domain of BA-05 V4 (its section 2.1), whose closed type list does not contain these record types (BA-05 V4 section 2.6).

**Encoding (BA-05 V4 value kinds, by reference).** Each field value is encoded by the value-kind table of BA-05 V4 section 2.1, reused by reference and not restated here: integer, boolean, double (the 16 lowercase hexadecimal digits of the binary64 bit pattern), string and enum (with the UTF-8 byte count), raw bytes, absent (`~`), ordered sequence (here in plan order), unordered set of records (sorted by the declared key), nested record inline, and `TYPED_VALUE`. The BA-05 V4 helper types used below (`POINT3D`, `DYNPROP`, `TYPED_VALUE`) are used **only as inline value encodings** inside this domain; they are not BA-05 records here and produce no BA-05 digest. A change of a reused value-kind encoding in BA-05 is a change of this domain's encoding and follows the versioning rule above. **Digest:** SHA-256 of the stream, lowercase hexadecimal. A NaN or an infinity, or a string that is not well-formed UTF-16, makes the record `UNKNOWN`, and PREPARE-R refuses the run (O1). The record types are defined here; BA-05 V4 is not changed by them (BA-08 depends on BA-05, not the reverse).

**`PLAN_CAPTURE_RECORD`**

| Field | Value kind | Content |
|---|---|---|
| `kind` | string | `selective` |
| `newRackId` | string | `NewRackId` (section 15) |
| `views` | ordered sequence of inline `PLAN_VIEW` records | the views the run mirrors, in seam call order: SL-1 in ascending `k`, then SL-2 (section 15) |

**`PLAN_VIEW`**

| Field | Value kind | Content |
|---|---|---|
| `viewKind` | enum (stored name as a string) | `FRONTAL` or `PLANTA` |
| `section` | integer | `k` for a frontal, `-1` for the planta |
| `effectiveName` | string | the effective definition name computed in PREPARE-R |
| `envelopeBytes` | raw bytes | the UTF-8 bytes of the caller-composed envelope for this view |
| `transform` | inline record `PLAN_TRANSFORM` | the computed top-level transform |
| `headers` | ordered sequence of inline `PLAN_GROUP` records | `HeaderRunPlan.Headers`, in plan order |
| `looseInstances` | ordered sequence of inline `PLAN_INSTANCE` records | `HeaderRunPlan.LooseInstances`, in plan order |

**`PLAN_TRANSFORM`**

| Field | Value kind | Content |
|---|---|---|
| `position` | inline `POINT3D` (BA-05 V4 helper type, as an inline value encoding only) | insertion point |
| `rotation` | double | radians |
| `scaleX`, `scaleY`, `scaleZ` | double | scale factors (strictly positive determinant, V2.1 15.6) |
| `normalX`, `normalY`, `normalZ` | double | normal |

**`PLAN_GROUP`**

| Field | Value kind | Content |
|---|---|---|
| `name` | string | `HeaderGroup.Name` (the requested nested name) |
| `instances` | ordered sequence of inline `PLAN_INSTANCE` records | in plan order |
| `placements` | ordered sequence of inline `PLAN_PLACEMENT` records | in plan order |

**`PLAN_PLACEMENT`** (`HeaderPlacement`, `Application/Drawing/HeaderRunPlan.cs:85-102`)

| Field | Value kind | Content |
|---|---|---|
| `insertionX`, `insertionY` | double | - |
| `mirrored` | boolean | - |

**`PLAN_INSTANCE`** (every member of `HeaderBlockInstance`, `Application/Drawing/HeaderBlockInstance.cs:57-102`)

| Field | Value kind | Content |
|---|---|---|
| `role` | enum (stored name as a string) | `HeaderBlockRole` |
| `pieceId`, `blockName`, `view` | string | or absent |
| `insertionX`, `insertionY` | double | `Insertion` |
| `connectionAnchorX`, `connectionAnchorY` | double | `ConnectionAnchor` |
| `rotationRadians` | double | - |
| `mirroredX`, `mirroredY` | boolean | - |
| `dynamicParameters` | unordered set of inline `DYNPROP` values (BA-05 V4 helper type, as an inline value encoding only) keyed by `name` | each value a `TYPED_VALUE` of kind `DOUBLE`; empty when none |
| `text` | string | or absent |
| `textHeight` | double | - |
| `dimensionOffset` | double | - |
| `dimensionStyleName` | string | or absent |

### 11.2 `HOST_DEFAULT_MAP`: the pin rule (AR3-11; PH2-OR-1; D-06, D-07)

`HOST_DEFAULT_MAP` is a **pin** (AR2-16): a named slot in the sealed scenarios (BA-04 V4 section 2.1); its value lives in a separate pin record. This section is the rule the pin record must follow.

| Id | Rule |
|---|---|
| HDM-1 Scope (AR3-11 condition 2) | only attributes of objects created in `T_M` that **neither the captured plan, nor the writer constants of section 3.1, nor the I-52 authority** determine: of nested piece references and group placements, `LayerId`, `Color`, `Linetype`, `LinetypeScale`, `LineWeight`, `Transparency` and `Visible` (`sa3a` L7); of `DBText`, the text style and the fields it drives (width factor, oblique), `Rotation`, `Color`, `Linetype`, `LinetypeScale`, `LineWeight`, `Transparency` (`sa3a` T2); of `RotatedDimension`, `Color`, `Linetype`, `LinetypeScale`, `LineWeight`, `Transparency`, and the dimension style when no named style is used (the current style); of a layer the seam creates (SL-Y "added"), every field of its record other than `Name` and `Color`; and any other field of a created object's typed form (BA-05 V4) that none of the three sources determines. The plot style name of created objects is not typed by BA-05 V4 (the declared gap of section 11 rule 1, pending AQ-V4-09). **Excluded:** the ST-13 set of the top-level reference, which is read from the source reference into the `PRE_COMMAND_STATE_RECORD` (section 11 rule 1; V17; D-07); every writer-assigned attribute; every plan value |
| HDM-2 Keys (condition 3) | the consumed context variables `CLAYER`, `CECOLOR`, `CELTYPE`, `CELTSCALE`, `CELWEIGHT`, `CETRANSPARENCY`, `CPLOTSTYLE`, and the current text style and dimension style, all captured in the `PRE_COMMAND_STATE_RECORD` (section 6). For the two styles the key component is the record identity together with the exact fingerprint of the record and of its transitive records (this artifact's reading of "the current text style and dimension style" of AR3-11, reconciled with V17 L-36, which names the effective style records). A key the record cannot read is `UNKNOWN`, and so is every attribute looked up with it |
| HDM-3 Source (condition 1) | **only** the non-governing scenarios `HDM-CHAR-FX1F` (piece references and group placements), `HDM-CHAR-FXANN` (`DBText`) and `HDM-CHAR-FXDIM` (`RotatedDimension`) of BA-04 V4: `NON_GOVERNING_BY_DESIGN`, `GROUP = HDM`, `GROUPS_SERVED = []`, `BUILD_UNDER_TEST = CHARACTERIZATION_BUILD`, in the mode of AR3-01 (`RECORDED_NOT_ENFORCED` = E-03, E-04, E-12 phase A, E-12 phase C, S-1, S-2, S-3; the run reaches PV, `Commit()`, V1 and PC; product outcome `PROVISIONAL_NON_GOVERNING`; no dependency on EVM, `HOST_DEFAULT_MAP` or K-3). They run **after the `LOCK_MODE` selection and before any governing run that uses S-1..S-3**. Each records every in-scope attribute of every object its mirror creates (every object class it creates, not only the class named above), read after PC as typed forms of BA-05 V4, together with the key values of its own `PRE_COMMAND_STATE_RECORD`. The map is **never** derived from a governing output or from the scenario that uses it |
| HDM-4 Varied settings | each `HDM-CHAR-*` scenario runs at least once with the key values **as pinned in the fixture instances** (the values every governing run on those instances presents; section 12 requires them equal in every template), and once or more with each key variable changed; the changed runs show which attributes depend on which key and are informative |
| HDM-5 Form and lookup | the pin record holds entries (object class, attribute, key tuple) -> effective value, with the run ids, their tuples, the identity of the characterization build and the scenario ids. A governing comparison looks up the key tuple of its own `PRE_COMMAND_STATE_RECORD`; a key tuple, object class or attribute without an entry is `UNKNOWN` (O5 path). There is **no** interpolation and no generalization across key values |
| HDM-6 Until pinned (condition 4) | every comparison of an in-scope attribute is `UNKNOWN` (O5 path) |
| HDM-7 V17 L-36 / CT-50 | V17 L-36 (`proposal-v17.md:421`) assigns to CT-50 the characterization of the mechanism and the effective values AutoCAD 2025 applies to these unassigned properties, and of the observations each producer consumes (`MaterializationContextReadSet`); CT-50 is **NOT EXECUTED** (V2.5 header). The `HDM-CHAR-*` runs neither execute nor replace CT-50: they characterize, for the exact CT-21D tuple and fixtures only, the values the map needs. Their key set is the read set of L-36 restricted to what these fixtures consume: `CANNOSCALE` enters L-36 only if CT-50 shows its consumption, and every fixture uses non-annotative styles (section 12); the transitive style records of L-36 are part of the style keys (HDM-2). A dependence on anything outside the key set is not detected by design; it stays with CT-50 and is disclosed |
| HDM-8 Coverage gap (AQ-V4-07 = Q-HDM-1) | the mirrors of the three scenarios named by AR3-11 create no layer (their templates hold every layer they use), so no source exists for the in-scope fields of a created SL-Y layer (AR3-15). Until the Architect rules AQ-V4-07 (section 16.1, K-6), those fields are `UNKNOWN` (O5 path), and the SL-Y comparison of `CL-CLEAN-FXANNBLANK-FP` cannot reach `EQUAL`; that scenario therefore carries K-6 (AQ-V4-07) as a blocker in BA-04 V4, and its `O3` stands only as the expectation that holds once the ruling gives the field source. Proposed ruling (not decided here): also run `HDM-CHAR-FXANN` on `FX-ANN-BLANK`, whose mirror creates `RACKCAD_ANOTACIONES` |

## 12. Fixtures (parameterized specifications; AR2-17, AR2-41)

### 12.1 Completeness by authored output (D-03; replaces the V3 rule of completeness)

A fixture is specified by its **editor inputs** in the exact build's Selective editor for a **new rack** (`RSW` at `3375aadb`): the inputs of sections 12.2 and 12.3 are set, and every other input keeps its initial value at the bound SHA. The fixture's design is the **authored output** of those inputs, the `SelectivePalletDesign` that `SelectiveEditorState.BuildDesign` writes (`SES:1327-1400`), **not** the defaults of the domain type. The authored-field table of section 12.2 is normative. The authored document bytes (the `SelectivePalletDesignDocument` inside the envelope of each source view) are read from the instance and recorded in its `FIXTURE_INSTANCE` pin record as the conformance evidence (AR2-17); they are never assumed here.

### 12.2 Common editor inputs and authored fields

| Input (editor) | Value | Authored field(s) (`SES:1327-1400` unless stated) |
|---|---|---|
| post | `POSTE_OMEGA_ATORNILLABLE_CON_TROQUEL_GOTA_DE_AGUA` | `PostId` |
| post peralte | `3` (the initial value, `RSW.xaml:85`) | `PostPeralte = 3.0` (> 0; the editor refuses a value <= 0, `RSW.xaml.cs:2789`) |
| per-post peralte and cabecera | not touched | `PostPeraltes` = N+1 entries `0.0` (0 = inherit `PostPeralte`); `PostCabeceras` = N+1 `null` entries (standard); `ExtraFondoPostCabeceras` = one empty row per extra fondo (standard) (`SES:740-763`; N = the frente count) |
| horizontal tolerance, vertical clearance | the literals a new rack starts with, unlinked (no project variable is offered to a new rack) | `PalletTolerance = 4.0`, `VerticalClearance = 6.0` (`RSW.xaml.cs:474-482`) |
| legacy floor-beam rise | not editable | `FloorBeamRise = 4.0` |
| fondo depth, cabecera fondo | `48`, blank | `PalletDepth = 48.0`; `CabeceraFondoOverrides[0] = 0.0` (automatic) |
| number of fondos | `D` of the fixture | `DepthCount = D`; for each extra fondo `k`: `ExtraFondoBays[k-1]` = a copy of fondo 0's bays, `ExtraFondoDepths[k-1] = 48.0`, `CabeceraFondoOverrides[k] = 0.0` (`SES:1356-1366`; a new fondo clones fondo 0, `SES:217-241`) |
| separators | not touched | `SeparatorLengths` = `D-1` entries `12.0` (`SelectiveRackDefaults.DefaultSeparator`; `RSW.xaml.cs:721-782`) |
| frentes x levels | `B x L` of the fixture, the **same in every fondo** (no corner) | `Bays` and each `ExtraFondoBays` entry: `B` bays of `L` levels |
| per frente | floor beam off, rise `4`, no height, no medio frente | `FloorBeam = false`; `FloorBeamRiseOverride = 4.0` **explicit in every bay** (`SES:152`); `HeightOverride = null`; `Segments = []` |
| cell (every cell) | beam `LARGUERO_ESCALON_CAL14_3_REMACHES`, peralte `4`, pallets per level `2`, pallet frente `40`, alto `48`, beam length and clear opening blank | `BeamId`, `BeamPeralte = 4.0`, `PalletCount = 2`, `Pallet = {Frente 40.0, Alto 48.0}`, `BeamLengthOverride = null`, `ClearOverride = null` |
| toggles | `DrawBasePlate` on; `NumberFronts`, `NumberLevels`, `DrawRackName`, `DrawPallets` off unless section 12.3 says otherwise | the same booleans; **`DrawPallets = false` in every fixture** |
| annotation scale | `1` | `AnnotationScale = 1.0` |
| dimensions, dimension views, dimension style | `None` unless section 12.3 says otherwise; view boxes not touched; `(Automático)` | `Dimensions`; `DimensionViews = null` (a new rack is legacy and untouched, `RSW.xaml.cs:263,2162-2169`); `DimensionStyle = null` |
| safety | none | `SafetySelections = []` |
| name | the fixture's name | the envelope and design `Name` |

**Envelope and document.** Envelope `SchemaVersion "1.0"`, `Kind "selective"`, view and section per view, no custom properties or extra keys; design document `SchemaVersion "1.0"`, no `PropertyValues` (a new rack's document carries no bindings, `sa3a` N3). **Drawing.** No `RACKCAD_PROJECT` entry; the source views drawn by the product, each inserted once (the frontal of every fondo and the planta); the source reference as RackCad inserted it (scale `(1,1,1)`, normal `+Z`, definition origin `0`); no XCLIP, attributes, XData or extension dictionary on the reference; INSUNITS inches; colour-dependent plot styles (`PSTYLEMODE = 1`; section 11 rule 1); the keys of section 11.2 **equal in every template**, with a non-annotative current text style and a non-annotative current dimension style whose `DIMSCALE != 0` (the values are recorded in the conformance record; none is chosen here).

### 12.3 The fixtures

| Fixture | Name | `D` | `B x L` | Toggles on (others off) | Dimensions | Overrides of the clean values of evidence `B4` table 2 | Purpose |
|---|---|---|---|---|---|---|---|
| `FX-1F` | `CT21D-FX1F` | 1 | 2 x 2 | `DrawBasePlate` | `None` | none | minimal frontal and planta |
| `FX-2F` | `CT21D-FX2F` | 2 | 2 x 2 | `DrawBasePlate` | `None` | `DepthCount = 2` with its fondo lists (no corner) | multi-fondo; separator |
| `FX-4F` | `CT21D-FX4F` | 4 | 2 x 2 | `DrawBasePlate` | `None` | `DepthCount = 4` with its fondo lists (no corner) | largest fondo count; structurally outside learning |
| `FX-ANN` | `CT21D-FXANN` | 1 | 2 x 2 | `DrawBasePlate`, `NumberFronts`, `NumberLevels`, `DrawRackName` | `None` | the three text toggles; the annotation layer exists because the source drew annotations | texts; SL-Y reuse |
| **`FX-ANN-BLANK`** | **blank** | 1 | 2 x 2 | `DrawBasePlate`, `DrawRackName` | `None` | `DrawRackName` with a **blank** logical `Name`: the source draws no label and its drawing has **no** `RACKCAD_ANOTACIONES` (checked in the conformance record; if the imported library blocks bring that layer, the instance does not conform); the view block names use the product fallbacks for a blank name (section 3) | **SL-Y "added"** (AR3-15): the mirror, named `<base> - espejo` (V17, never blank), creates the layer |
| `FX-DIM` | `CT21D-FXDIM` | 1 | 2 x 2 | `DrawBasePlate` | `Standard` (`DimensionViews = null`, `DimensionStyle = null`) | `Dimensions = Standard`; the dimension layer exists because the source drew dimensions | dimensions; `*D` blocks |
| `FX-BIG` | `CT21D-FXBIG` | 3 | 5 x 4 | `DrawBasePlate`, `NumberFronts`, `NumberLevels`, `DrawRackName` (**`DrawPallets` off**) | `Detailed` | `DepthCount = 3`, the three text toggles, `Dimensions = Detailed` | validation only |
| `FX-SCRATCH` | (none) | — | — | — | — | an empty template (INSUNITS inches; the keys of section 11.2 as in the other templates) with the library blocks of `FX-1F` imported and **no** RackCad content or layer | learning micro-scenarios only: for SL-Y "added", **only** the isolated learning construction `EV-L-OC-LAYER` (the layer absent); besides it, `EV-L-OC-TM` (BA-06 V4 section 3.3; BA-04 V4). No trace family and no validation run uses it: the `SH-LAY-ADDED` trace family runs on `FX-ANN-BLANK` (BA-06 V4 sections 7 and 10.2) |
| `FX-IMP` | `CT21D-FXIMP` | 1 | 2 x 2 | `DrawBasePlate` | `None` | a template in which **at least two** blocks required by the mirrored plan are absent | SL-5 import path: **`SPECIFICATION_OPEN`** (K-8) |

### 12.4 V17 limitations per fixture (D-03; replaces the V3 blanket statement)

The V3 sentence "these values avoid V17 L-06..L-08, L-11..L-14, L-20..L-26 and L-29..L-33" is **withdrawn**: several of those limitations are not decided by the fixture's values. The limitations are grouped as follows (evidence `B4` table 2 maps each clean value to the limitation it avoids).

| Group | Limitations | Reason |
|---|---|---|
| A: avoided by the common values | L-06, L-07, L-08, L-33 (the source reference and definition as RackCad inserted them); L-11 (no corner: every fondo has fondo 0's frente count); L-12 (no medio frente; one post id; per-post peraltes `0.0` = inherit; standard cabeceras); L-13, L-14 (`SafetySelections = []`; `DrawPallets = false`, also in `FX-BIG`); L-20 (clean schema, no extra keys, `DimensionViews = null`, `DimensionStyle = null`); L-21 (a freshly authored payload with an explicit `FloorBeamRiseOverride` in every bay); L-23 (unlinked tolerances); L-24 (no `RACKCAD_PROJECT` entry); L-25 (standard view and section encodings) | decided by the values of section 12.2 |
| B: not decided by the fixture's values | L-22, L-26, L-28, L-29, L-30 (piece classes), L-31, L-32, L-34, L-35, L-36, L-37 | they depend on the library content (census K-2), on the host characterizations CT-49 and CT-50, or on the G3 probes; CT-21D takes `D'` as given (section 11 rule 4) and claims none of them avoided. L-36 is the characterization that `HOST_DEFAULT_MAP` reconciles (section 11.2, HDM-7) |

| Fixture | Group A | Outside a `B4` clean value (and what follows) |
|---|---|---|
| `FX-1F` | all | none |
| `FX-2F`, `FX-4F` | all | `DepthCount > 1` with its fondo lists (`B4` maps it to L-11 and L-22): L-11 stays avoided (no corner); L-22 is group B |
| `FX-ANN` | all | the three text toggles (`B4` maps them to L-30): whether V17 admits these annotations is decided by its representability rules, not by CT-21D |
| `FX-ANN-BLANK` | all | `DrawRackName` on (as `FX-ANN`, L-30 per `B4`); a blank logical `Name`, which no V17 rule was found to refuse (section 4.1) |
| `FX-DIM` | all | `Dimensions = Standard` (`B4` maps `Dimensions` to L-20 and L-30): the L-20 part stays avoided (`DimensionViews` null, `DimensionStyle` null, `AnnotationScale` 1); L-30 as for `FX-ANN` |
| `FX-BIG` | all | `DepthCount = 3` (as `FX-2F`), the three text toggles (as `FX-ANN`), `Dimensions = Detailed` (as `FX-DIM`) |
| `FX-IMP` | all | required blocks absent before the import by design; L-26 concerns blocks absent **after** the import, which the fixture relies on the import to supply (K-8) |

### 12.5 Variants

| Variant | Construction | Used by | State |
|---|---|---|---|
| `FX-1F-XREF-UNRESOLVED` | `FX-1F` plus one attached external reference whose file is missing | `NC-E07` | defined |
| `FX-1F-XREF-HOMONYM` | **withdrawn** (V4 correction pass): every required name is already an ordinary block definition in `FX-1F` (the source drawing imported every block its plan required), so a resolved attached external reference cannot take a required name there and the variant cannot be built (BA04V3-11) | none | withdrawn |
| `FX-IMP-XREF-HOMONYM` | an `FX-IMP` base in which **one** required name that `FX-IMP` lacks as an ordinary block definition is taken by a **resolved** attached external-reference block record (the single wrong-type variant, BA04-F20; BA04V3-11) | `PW-CLONE-WRONG-TYPE` | `SPECIFICATION_OPEN` (depends on `FX-IMP`, K-8) |
| `FX-IMP-CASE` | `FX-IMP` with a required block present under a name differing only by case | `PW-CLONE-CASE` | `SPECIFICATION_OPEN` (depends on `FX-IMP`) |
| `FX-IMP-NESTED` | `FX-IMP` with the nested dependencies of an absent block present | `PW-CLONE-NESTED`, `NC-CLONE-REPLACE` | `SPECIFICATION_OPEN` |
| `FX-IMP-SYMTAB` | `FX-IMP` with closure layers, linetypes, text and dimension styles present with different properties | `PW-CLONE-SYMTAB` | `SPECIFICATION_OPEN` |
| `L-VAR-DIFF` | a copy of the baseline library whose content differs for a block, a nested block, a layer, a linetype and a text style (homonyms of the drawing) | `PW-CLONE-BASE` | defined (content depends on the census, K-2) |
| `L-VAR-PROXY` | a copy of the baseline library with a proxy entity inside one required block | `PR-REC-CLOSURE-UNKNOWN` | defined (content depends on the census) |

**`FX-IMP` (K-8, AR2-41; D-10).** Its construction must be **reachable with product operations** or be **declared artificial with its reason**; it is decided with the fixture-instance gate (host). A candidate reachable construction (not decided here): the source is drawn while the configured library lacks at least two blocks its plan requires; the drawer omits those pieces and the product reports them on insert (`sa3b` F1.1..F1.5); the library later supplies them, so the mirror's plan requires blocks the drawing lacks and PREPARE-W imports them. Whether a source with omitted pieces is an admissible mirror source is part of K-8. The V3 example (a library that later lacks a block) is **withdrawn**: the drawing would still hold the block, and a drawing that also lacked it would give a `MissingLibraryBlocks` refusal, not the import path. Until K-8 is decided `FX-IMP` and its variants `FX-IMP-CASE`, `FX-IMP-NESTED`, `FX-IMP-SYMTAB` and `FX-IMP-XREF-HOMONYM` are `SPECIFICATION_OPEN`, and every scenario that uses one of them carries `BLOCKED_BY = K-8` (BA-04 V4).

### 12.6 Templates and instances (AR2-17, AR3-26)

Each fixture is instantiated as a template drawing containing the source rack drawn by the exact build through its editor with the inputs of sections 12.2 and 12.3, the required library blocks as that drawing imported them, and nothing else. The instance hash is a **pin slot** `FIXTURE_INSTANCE@<FX>` (BA-04 V4 section 2.1), **fixed before the first governing run** (not a blocker of the baseline or of the catalog seal, AR3-26), with its conformance record: the authored document bytes read from every source view envelope and their comparison with section 12.2; the values of the keys of section 11.2; and, for `FX-ANN-BLANK`, the absence of `RACKCAD_ANOTACIONES`. **Instances require host work (not authorized).**

## 13. Quantity vector and shape classes

Fixed in BA-06 V4 sections 10.2 and 11 (`q1..q12`; `SH-F1`, `SH-FN`, `SH-FALL`, `SH-P`, `SH-FP`, `SH-ANN`, `SH-DIM`, `SH-IMP`, `SH-LAY-ADDED`). The `SH-LAY-ADDED` trace family and its validation run on `FX-ANN-BLANK`, a mirror of a product-drawn rack (section 4.1; BA-06 V4 sections 7 and 10.2); for SL-Y "added", `FX-SCRATCH` holds only the learning construction `EV-L-OC-LAYER`.

## 14. Library census and product-drawn types

The census procedure and record schema are fixed in BA-05 V4 section 5; its universe is **every block definition of the library file** (AR3-17); it needs host work. For this artifact the census records the value set of every dynamic property (section 11 rule 2). The entity types RackCad itself draws (`BlockReference`, `DBText`, `RotatedDimension`, `Polyline`, `Circle`; evidence `sa2`) are the second source of the type list (BA-05 V4 section 2.5).

## 15. Naming and identity

`NewRackId` is minted once per logical mirrored rack by the orchestrator and is identical in every sibling envelope. Effective view names are computed in PREPARE-R under the lock and returned by the seams. **Nested names (BA06-F11):** the requested names restart per plan and the effective names depend on the block table at the call; the orchestrator therefore calls SL-1 in **ascending `k`**, then SL-2, and every seam returns the handles of **all** objects it created (nested definitions and SL-Y layers included), from which the sealed list takes the effective names and phase A its domain (section 5). The final mirror name is an I-52 authority (V17: `<base> - espejo`, never blank; the reused I-51 allocator falls back to `Rack` for a blank base, `Application/Persistence/RackDuplicationPlan.cs:127,421`), not fixed here.

## 16. What remains (separated by gate; AR3-25, AR3-26)

### 16.1 Items that gate `BASELINE_READY` (prerequisites of this registry entry)

| Id | Missing item | Gate | Registry gate (BA-11 V4 `PREREQUISITES`) |
|---|---|---|---|
| K-2 | library census (every block of the library file), closure contents, fingerprint type list, dynamic-property value sets | host (not authorized) | before the seal |
| K-3 | `TOL_SCALE` decision, test and record | separate decision; the test needs non-governing host evidence (not decided by this session) | before the candidate (a value of section 10) |
| K-5 | Coordinator ratification of the designated route R-1 and of its control-plane delivery (condition E4-C5) | Coordinator | before the seal |
| K-6 | the Architect's rulings on the open questions of this artifact (section 16.4): **AQ-V4-07** (= Q-HDM-1): the source of the in-scope fields of a created SL-Y layer (section 11.2, HDM-8); and AQ-V4-08, AQ-V4-09 and AQ-V4-10 where a ruling differs from the reading recorded here (sections 6 and 11 rule 1). None is decided here. The phase-2 rulings PH2-CF-1, PH2-TP-1, PH2-OR-1 and PH2-E4-1 are **decided** (AR3-12, AR3-13, AR3-11, AR3-14) | Architect | before the candidate (a ruling changes section 11.2, 6 or 11) |
| K-8 | construction of `FX-IMP` and of its variants `FX-IMP-CASE`, `FX-IMP-NESTED`, `FX-IMP-SYMTAB` and `FX-IMP-XREF-HOMONYM` (section 12.5) | fixture-instance gate (host) | before the candidate (it changes section 12) |

**Scenario catalog seal (AR3-26).** Of this artifact's items, only **K-3** (to declare the S comparisons), **K-8** and the Architect's rulings (K-6) block the seal of the scenario catalog BA-04 V4; K-1 and K-4 do not. **K-6 blocks the catalog seal only through `CL-CLEAN-FXANNBLANK-FP`**, through AQ-V4-07: it is the only scenario whose declared expectation needs the fields of a created SL-Y layer (HDM-8), so BA-04 V4 carries K-6 (AQ-V4-07) in its `BLOCKED_BY`. AQ-V4-08, AQ-V4-09 and AQ-V4-10 gate the candidate of this artifact (and of BA-05 V4, BA-11 V4); this artifact does not read them as seal blockers of the catalog.

### 16.2 Fixed before the first governing run (not baseline blockers; AR3-26)

| Id | Item |
|---|---|
| K-1 | fixture instances: pin slots `FIXTURE_INSTANCE@*`, `FX-ANN-BLANK` included (and `FX-IMP-XREF-HOMONYM` with the other `FX-IMP` variants once K-8 is decided), each with its conformance record (section 12.6) |
| K-4 | `HOST_DEFAULT_MAP`: pin slot; sources `HDM-CHAR-FX1F`, `HDM-CHAR-FXANN`, `HDM-CHAR-FXDIM` (section 11.2); fixed before the first governing run that uses S-1..S-3 |

### 16.3 Execution prerequisites

| Id | Item | Class |
|---|---|---|
| K-7 | the readers and comparators marked MISSING in section 6, including the full-scope payload reader and the readers of the source reference presentation and of the context variables | EXEC-4 |
| K-9 | the static guard of section 6.1 (no reference to the library cache, the importer or the AUTH-12 external query in the seams, the orchestrator, PREPARE-R and the warm-up harness; no catalog provider, catalog loader or settings store in the warm-up harness) | additional execution prerequisite derived from AR2-42 and AR3-16 (not EXEC-12) |
| K-10 | the **characterization build** (AR3-01; BA-04 V4 sections 1.1 and 1.2): a build of the product at the exact source SHA with harness-only switches and injection points, disabled unless a scenario names them. **Users:** every BA-04 V4 scenario with `BUILD_UNDER_TEST = CHARACTERIZATION_BUILD`, not a closed list of families: the runs in the AR3-01 mode (`E4-LEARN-R1-*`, `LK-*`, `HDM-CHAR-*`, every `WU-DRY-*`, and the other scenarios BA-04 V4 section 1.1 places in that mode) and every scenario one of whose injections needs a switch or an injection point compiled into the build (BA-04 V4 section 1.2); and the dynamic-trace dry runs of BA-06 V4 section 7 (non-governing; their use of the AR3-01 mode is part of the trace plan the Architect approves with `STATIC_REVIEWED`, AR3-22). **Required switch and injection-point set** (it is part of the build identity): the recording without enforcement of the AR3-01 set (BA-04 V4 section 1.1), and the switches and injection points of every writer kind that BA-04 V4 section 1.2 marks as a build switch: `Wr-A`..`Wr-G` and `Wr-S` (the I-14 writers of V2.1 10.6 at the X and Y positions), `Wr-OCT`, `XDATA_WRITE`, `HARNESS_FAULT`, `CANARY_LOAD`, `SOURCE_SWAP`, `COPY_MODIFY`, `SOURCE_MODIFY_DURING_ACQUISITION`, `CLONE_MODE_SWITCH` (`NC-CLONE-REPLACE`, AR3-07) and `CONFLICT_PROBE`. The kinds `CONTROL_PLANE_SETUP`, `FIXTURE_VARIANT` and `LIBRARY_VARIANT` need no switch. **Pending rulings (not decided here):** the use of this build by the **governing** learning runs `EV-L-*` and qualification vehicles `Q-I14-*` is pending **AQ-V4-01**; its use by the **other governing** scenarios, and the two build identities it puts in the baseline tuple, are pending **AQ-V4-06** | execution prerequisite (its identity, `CHARACTERIZATION_TUPLE` of BA-04 V4 section 4, is a tuple field of every run that uses it) |

### 16.4 Open questions for the Architect referenced here (round V4 ids; none decided)

| Id | Question | Where this artifact depends on it | Reading recorded here until the ruling |
|---|---|---|---|
| AQ-V4-07 | Q-HDM-1: the pin source of the unassigned fields of a created SL-Y layer | sections 4.1, 11.2 (HDM-8), 16.1 (K-6) | those fields are `UNKNOWN` (O5 path); proposed ruling: also run `HDM-CHAR-FXANN` on `FX-ANN-BLANK` |
| AQ-V4-08 | whether the `CONTEXT` members are part of `EXPECTED_AFTER_ABORT` (V2.1 11; BA-05 V4 section 2.3.5 leaves the choice to the kind baseline) | section 6 | proposed answer: ABORT-VERIFY compares both `CONTEXT` kinds; E-08 compares the `REFERENCE` members only |
| AQ-V4-09 | the plot style name for ST-13: a field added to BA-05 before its seal, or a gap declared by the kind baseline (BA-05 V4 sections 2.3.5 and 2.4) | sections 4 (SL-R), 6, 11 rule 1, 11.2 (HDM-1) | the gap is declared; the fixtures use colour-dependent plot styles (`PSTYLEMODE = 1`) |
| AQ-V4-10 | the source of dimension overrides: the stored `DSTYLE` section or the effective value compared with the style (BA-05 V4 section 2.3.2) | section 11 rule 1 (automatic overrides of section 3.1) | the stored `DSTYLE` section (BA-05 V4 section 2.3.2) |
| AQ-V4-01 | the `RECORDED_NOT_ENFORCED` set and build of the governing learning runs `EV-L-*` and qualification vehicles `Q-I14-*` | section 16.3 (K-10) | not read here; BA-04 V4 and BA-06 V4 hold the proposal |
| AQ-V4-06 | governing scenarios on the characterization build and two build identities in the tuple | section 16.3 (K-10) | not read here; BA-04 V4 holds the proposal |

```text
SELECTIVE KIND BASELINE = DRAFT V4 ; NO EVIDENCE-DERIVED VALUE SELECTED
LOCK_MODE = UNSET (candidates LM-1, LM-2) ; E-04 ADMITTED SET = UNSET ; ROUTE R-1 = DESIGNATED, PENDING RATIFICATION (K-5) ; TOL_SCALE = UNSET (no authority)
HOST_DEFAULT_MAP = PIN SLOT, UNSET (sources HDM-CHAR-*; AQ-V4-07 = Q-HDM-1 open) ; FIXTURE INSTANCES = PIN SLOTS, UNSET ; FX-IMP AND VARIANTS = SPECIFICATION_OPEN (K-8)
CT21D_AUTHORITY_BASELINE_READY = FALSE
```

## 17. Delta V4 (decisions section 214)

Section numbers 1 to 16 are kept. New subsections: 4.1, 11.1, 11.2 and 12.1 to 12.6 (the V3 section 12 is split; its variants table is now 12.5). Section 17 is new. The V4 correction pass (rows marked "correction pass") adds subsection 16.4 and renumbers nothing.

| Finding id | Decision | Fix (section / field) |
|---|---|---|
| CLOSURE BA08-01 (PARTIALLY_FIXED: SL-Y sealed reason unsound) | AR3-15 | FIXED: the sealed reason is withdrawn and replaced by section 4.1; SL-Y "added" is covered by `CL-CLEAN-FXANNBLANK-FP` on `FX-ANN-BLANK` (section 4 row SL-Y; section 12.3); the rename case stays a disclosed residual |
| D-01 (MAJOR) | AR3-15 | FIXED: section 4.1 (blank-name construction, residual rows), section 4 row SL-Y, section 12.3 `FX-ANN-BLANK`, section 13; the wording of `SH-LAY-ADDED` in BA-06 belongs to BA-06 V4 (cross-artifact note) |
| CLOSURE BA08-04 (PARTIALLY_FIXED: completeness rule and global limitation statement) | AR2-41, AR3-30 | FIXED: sections 12.1, 12.2 (authored output), 12.4 (per fixture) |
| D-03 (MAJOR) | AR3-30 (required fix), AR2-41 | FIXED: completeness by authored output (section 12.1); `PostPeralte = 3.0`; explicit `FloorBeamRiseOverride = 4.0` per bay; `ExtraFondoBays`, `ExtraFondoDepths`, `CabeceraFondoOverrides`, `SeparatorLengths`, `PostPeraltes`, `PostCabeceras`, `ExtraFondoPostCabeceras` as the editor writes them (section 12.2); `DrawPallets = false` stated for `FX-BIG` (sections 12.2, 12.3); V17 limitations per fixture (section 12.4) |
| CLOSURE BA08-06 (NOT_APPLICABLE, refuted in phase 2) | AR3-14 | NOT_APPLICABLE as a finding; the five conditions of PH2-E4-1 are recorded in section 8 |
| CLOSURE BA08-07 (PARTIALLY_FIXED: envelope input row) | AR3-12, AR3-30 | FIXED: section 2, rows "envelope input", "envelope write", "missing pieces", "transaction", and the paragraph "C-127 and the two current paths" |
| D-02 (MAJOR) | AR3-12 | FIXED: AUTH-15 takes a `RackEmbedDocument`, serializes it itself (`RDC:187`), writes and reads back its own serialization with an ordinal compare (`RDC:204-206`); the product command serializes once (`RSC:416`) and `SBW` writes that string unchanged when non-empty; C-127 asserted for neither path, one more reason no evidence transfers (section 2) |
| D-04 (MAJOR) | AR3-15 | FIXED: section 5, domain part (1) includes the reused SL-Y layers known from the plan capture and the `PRE_COMMAND_STATE_RECORD`; part (2) includes created layers; section 15 |
| D-06 (MAJOR) | AR3-11 | FIXED: section 11 rule 5 and section 11.2 (source = `HDM-CHAR-FX1F`, `HDM-CHAR-FXANN`, `HDM-CHAR-FXDIM`; keys; varied settings; form and lookup; reconciliation with V17 L-36 / CT-50; `UNKNOWN` until pinned); section 16.2 K-4 |
| D-07 (MINOR) | AR3-11 (condition 2) | FIXED: ST-13 cited; section 4 row SL-R and section 11 rule 1 extended to the full ST-13 set read from the source reference into the `PRE_COMMAND_STATE_RECORD` (`PlotStyleName`, which BA-05 V4 does not type, is a declared gap that the colour-dependent plot styles of the fixtures avoid, section 12.2); section 6 row "source reference presentation" and the `CONTEXT` comparison rule; `HOST_DEFAULT_MAP` scope restricted to nested references, `DBText`, dimensions and created layers, excluding the ST-13 set (section 11.2 HDM-1) |
| D-08 (MINOR) | AR3-13 | FIXED: section 3 (tuple field `CATALOG_FOLDER`, V2.5, resolved directory and fallback) and section 7 rule (blob hashes versus deployed bytes; comparison of the derived name universe) |
| D-09 (MINOR; BA-08 part) | AR3-16 row 2, AR3-30 | FIXED: section 6.1 guard extended to the orchestrator and PREPARE-R and registered as an additional execution prerequisite derived from AR2-42 and AR3-16, not EXEC-12; section 16.3 K-9 |
| D-10 (MINOR) | AR3-30 | FIXED: section 12.5 `FX-IMP` paragraph (reverse construction; V3 example withdrawn) |
| D-11 (MINOR) | AR3-30 | FIXED: section 10 `TOL_CONTINUITY` reworded (N-S02 outside CT-21D; fixtures exclude medio frente) |
| D-12 (MINOR) | AR3-30 | FIXED: section 11.1 `PLAN_CAPTURE_RECORD` (fields, order, number encoding, digest; digest logged at PR) |
| D-13 (MINOR) | AR3-30 | FIXED: section 11 rule 2 (value sets in the census; constrained properties `UNKNOWN` until characterized); section 14 |
| D-14 (MINOR) | AR3-30 | FIXED: section 3 row "entity types" |
| PH2-CF-1 (ruling) | AR3-12 | FIXED: section 2 records the accepted fact correction (no change to 15.3, no evidence transfer, P-8 required, AR2-38) |
| PH2-TP-1 (ruling) | AR3-13 | FIXED: sections 3 and 7 cite V2.5 `CATALOG_FOLDER`, not the V3 proposal |
| PH2-OR-1 (ruling) | AR3-11 | FIXED: section 11.2 |
| PH2-E4-1 (ruling) | AR3-14 | FIXED: section 8, conditions E4-C1..E4-C5 |
| BA11V3-01 (BA-08 part) | AR3-17 | FIXED: section 10 keeps the values and states that BA-05 V4 vectors are in slot units, instantiated by the `Q-FPSPEC-VECTORS` harness from section 10; sections 7 and 14: census universe = every block of the library file; `TOL_SCALE` is not a BA-05 gate |
| BA11V3-04 (BA-08 part) | AR3-26 | FIXED: section 16.2 (K-1, K-4 fixed before the first governing run) and the catalog-seal paragraph of section 16.1 (only K-3, K-8 and the Architect's rulings block it) |
| BA11V3-07 (row BA-08) | AR3-25 | FIXED: header `DEPENDS_ON` (registry entries only) and `PREREQUISITES`; section 16.1 registry-gate column |
| BA11V3-10 | AR3-13 | FIXED: section 3 cites the V2.5 field, not the pending proposal |
| D-05, D-15..D-19, BA11V3-02, BA11V3-13 | — | NOT_APPLICABLE to BA-08 (BA-09 V4 and BA-10 V4 items) |
| BA04V3-11 / CLOSURE BA04-F20, BA-08 part (critic: fixture authority lacks `FX-IMP-XREF-HOMONYM`) (correction pass) | AR3-30 | FIXED: section 12.5 withdraws `FX-1F-XREF-HOMONYM` with its reason (every required name is already an ordinary block definition in `FX-1F`) and adds `FX-IMP-XREF-HOMONYM` (an `FX-IMP` base; one required name that `FX-IMP` lacks as an ordinary block definition taken by a resolved attached external-reference block record; `SPECIFICATION_OPEN`, depends on `FX-IMP`, K-8; user `PW-CLONE-WRONG-TYPE`); `FX-IMP` paragraph; section 16.1 K-8 scope; section 16.2 K-1 |
| BA04V3-09 consequence / D-07 (critic: `KS-SL4-NEG-DET` listed as SL-R coverage) (correction pass) | AR3-30 | FIXED: section 4 row SL-R coverage is `CL-CLEAN-*`; `KS-SL4-NEG-DET` is cited separately, after the table, as the SL-4 transform refusal (PRE-WRITE, before PV); its grouping is BA-04 V4's, pending AQ-V4-03 |
| D-01 (BA-06 wording) / AR3-15 (critic: `FX-SCRATCH` named for the `SH-LAY-ADDED` trace) (correction pass) | AR3-15 | FIXED: section 4.1 last row, section 12.3 `FX-SCRATCH` purpose and section 13: for SL-Y "added" `FX-SCRATCH` serves only the learning construction `EV-L-OC-LAYER`; the `SH-LAY-ADDED` trace family runs on `FX-ANN-BLANK` (BA-06 V4 sections 7 and 10.2). `EV-L-OC-TM`, which BA-06 V4 section 3.3 and BA-04 V4 also place on `FX-SCRATCH`, is kept as a user of the template |
| BA05V3-N07, BA-08 part (critic: separate address member; item forms) (correction pass) | AR3-30 | FIXED: section 6 introduction and rows: "view addresses and grouping" has no member of its own and is carried by the `PAYLOAD` members of row "RackCad payloads" (BA-05 V4 section 2.3.5), the decoded address being only an oracle and SL-6 input; model-space content = `ENUMERATION` item forms `HANDLE` and `HANDLE_CLASS`; block table names and symbol-table name sets = `ENUMERATION` item form `NAME` (BA-05 V4 section 2.3.6) |
| D-12 / AR3-17 (critic: plan capture in the `FPSPEC-V1\|EXACT\|` domain) (correction pass) | AR3-17, AR3-30 | FIXED: section 11.1 gives `PLAN_CAPTURE_RECORD` its own domain separator `CT21D-PLANCAP-V1\|<record type>\|`, owned and versioned by BA-08; BA-05 V4 value-kind encodings reused by reference; the BA-05 helper types (`POINT3D`, `DYNPROP`, `TYPED_VALUE`) used only as inline value encodings; never the `FPSPEC-V1\|EXACT\|` domain |
| AR3-01 / AR3-07 (critic: K-10 names only a subset of the users of the characterization build) (correction pass) | AR3-01 | FIXED: section 16.3 K-10 covers every BA-04 V4 scenario with `BUILD_UNDER_TEST = CHARACTERIZATION_BUILD` (BA-04 V4 sections 1.1 and 1.2) and lists the required switch and injection-point set (the recording without enforcement of the AR3-01 set and the build-switch writer kinds of BA-04 V4 section 1.2); the use by governing `EV-L-*` and `Q-I14-*` is pending AQ-V4-01, by the other governing scenarios pending AQ-V4-06; BA-09 V4 W-7 aligned |
| AR3-15 / AR3-26 / D-01 scenario part (critic: K-6 not carried as a catalog blocker) (correction pass) | AR3-26 | FIXED on the BA-08 side: section 16.1 K-6 = the Architect's rulings, AQ-V4-07 (= Q-HDM-1) included; K-6 blocks the catalog seal only through `CL-CLEAN-FXANNBLANK-FP` (which carries K-6 in BA-04 V4); section 11.2 HDM-8; section 4.1; header `PREREQUISITES` |
| BA-05 V4 cross note on section 10 (slot names; `NORM_ANGLE`) (correction pass) | AR3-17 | FIXED: section 10 heading and text "BA-05 V4 names the slots only"; the eight slot names exactly as BA-05 V4 section 3.1 (`TOL_LENGTH`, `TOL_TEXT_HEIGHT`, `TOL_DIM`, `TOL_ANGLE`, `NORM_ANGLE`, `TOL_NORMAL`, `TOL_DYNAMIC`, `TOL_SCALE`); `NORM_ANGLE` stated as a reduction modulo 2pi (checked against `NormalizePi` at `3375aadb`: remainder modulo 2pi, then one shift by 2pi into (-pi, pi]), with the wrapped difference (SV-10 `EQUAL`). No value changed |
| Architect open questions (round V4 ids) (correction pass) | - | APPLIED: AQ-V4-07 (sections 4.1, 11.2 HDM-8, 16.1 K-6), AQ-V4-08 (section 6: this artifact's reading is a proposed answer, not a ruling), AQ-V4-09 (sections 4, 6, 11 rule 1, 11.2 HDM-1), AQ-V4-10 (section 11 rule 1), AQ-V4-01 and AQ-V4-06 (section 16.3 K-10), AQ-V4-03 (section 4 note); all listed in the new section 16.4; none is decided here |
| AR3-01 / AR3-22 (K-10 users omit the dynamic-trace dry runs of BA-06 V4 section 7) (correction pass, minor) | AR3-01; AR3-22 | FIXED: section 16.3 K-10 adds to its users the dynamic-trace dry runs of BA-06 V4 section 7 (non-governing; their use of the AR3-01 mode is part of the trace plan the Architect approves with `STATIC_REVIEWED`, AR3-22); the required switch and injection-point set and the pending AQ-V4-01 and AQ-V4-06 are unchanged; BA-09 V4 W-7 aligned by reference |
