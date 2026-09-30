# I-52 — CT-21D Baseline Artifact BA-08: Selective Kind Authority Baseline Details (DRAFT V1)

> **BASELINE ARTIFACT BA-08 — DRAFT, NOT HASHED, NOT AGREED. Design / documentation only. Nothing here is executed or authorized. No evidence-derived value is selected.**
>
> ```text
> ARTIFACT_ID               = SELECTIVE_KIND_AUTHORITY_BASELINE   (baseline artifact CT21D-BASE-SELECTIVE-KIND)
> ARTIFACT_VERSION          = 1-DRAFT
> ARTIFACT_STATUS           = DRAFT (phase 1 of baseline preparation)
> ARTIFACT_HASH             = TO_BE_RECORDED_AT_SEAL   (see BA-11)
> AUTHORITY_CONTRACT        = CT-21D V2.1 sections 3.3, 3.4, 10, 11, 15 + V2.2 + V2.3 (AGREED_FOR_BASELINE_ARTIFACT_PREPARATION)
> SELECTIVE_AUTHORITY_DESIGN_STATUS = COMPLETE_FOR_BASELINE (contract level); this artifact makes the enumerations concrete
> CODE FACTS BOUND TO      = origin/main 3375aadbbf929427a6d106b2fff275d64863b89c (src extract; the seams do not exist)
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

Paths are relative to `src/`. **FACT** = seen in the code at the bound commit; **UNKNOWN** = the code does not determine it. Abbreviations: `LHD` = `RackCad.Plugin/Drawing/LateralHeaderDrawer.cs`; `SBW` = `RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs`; `BP` = `RackCad.Plugin/Drawing/BlockPlacement.cs`; `RBD` = `RackCad.Plugin/Systems/Shared/RackBlockData.cs`; `BLI` = `RackCad.Plugin/Drawing/BlockLibraryImporter.cs`.

## 1. Scope linkage

The kind covers exactly the sealed scope of BA-03: `SEL-FRONTAL` (one view per fondo `k`, `0 <= k < SelectiveDepthLayout.Count(system)`) and `SEL-PLANTA` (one whole-rack view). Lateral and the legacy `Section = -1` are out. `SelectiveDepthLayout.Count(system) = Math.Max(1, system?.DepthCount ?? 1)` (`Application/Systems/Selective/SelectiveDepthLayout.cs:18`); `MaxDepthCount = 4` (`Domain/Systems/Selective/SelectiveRackDefaults.cs:40`). A logical rack yields `Count(system)` frontal definitions and **one** planta definition (`RackSelectivoCommands.cs:233-244`), each with one model-space reference.

## 2. Seam authority references SL-1..SL-6

| Seam | Role | Caller-owned | Required by the sealed scope | Current-code precedent (bound commit) |
|---|---|---|---|---|
| SL-1 | create the frontal view definition (one per fondo) in the caller's transaction | yes | **required** | `SystemBlockWriter.CreateBlock` (`SBW:18-40`) runs lock, import, transaction, `CreateSystemBlock`, `RackBlockData.Write`, `Commit`; **edit-side caller-owned forms exist**: `SystemBlockWriter.RedefineInTransaction` (`SBW:104-116`) and `ViewBlockDraw.PrepareRedraw` (`ViewBlockDraw.cs:65-88`) |
| SL-2 | create the planta view definition in the caller's transaction | yes | **required** | same path; `SelectivePlantaDrawService` (`Systems/Selective/SelectivePlantaDrawService.cs:26-33`) |
| SL-3 | the lateral view definition: its own Selective contract | yes | **not required** (lateral out of scope) | `LateralHeaderDrawService` |
| SL-4 | place the block reference in the caller's transaction from a computed transform (no jig) | yes | **required** | `BlockPlacement.PlaceBlockWithJig` (`BP:174-201`) is jig-based; the non-jig `AppendReference` (`BP:79-95`) takes its own lock and transaction |
| SL-5 | PREPARE-W import with a structured result and a fresh acquire (outside `T_M`, under the lock) | callee transactions allowed | **required** | `BlockLibraryImporter.EnsureForPlan` (`BLI:25-39`); returns only an imported count and swallows failures (`BLI:88-90`, `96-100`, `117-120`, `126-131`) |
| SL-6 | read-only verifier and expected-versus-persisted comparator | read-only | **required** | `RackBlockFinder.ScanEnvelopes`, `RackBlockData.Read`, `SelectiveAuthoredAuthority.Resolve` (sibling consistency only; no expected-versus-persisted comparator exists) |

**Design clauses** (each a requirement on SL-1, SL-2, SL-4): the clauses of V2.1 section 15.3 apply unchanged (caller-owned `Database` and `Transaction`; identity by `IsTopTransaction`; no `Commit`, `Abort`, `Dispose`, `LockDocument`, `OpenCloseTransaction` or side-database write; typed PRE-WRITE and POST-WRITE failures; exact authored payload written verbatim; exact `NewRackId` policy; fresh reread support).

**SL-3 equivalence requirement (not required by the scope).** AUTH-15 may implement SL-3 only after **all** of: EQ-1 same plan type; EQ-2 same dispatcher and path; EQ-3 same caller-owned transaction semantics; EQ-4 exact Option-B host evidence on that path; EQ-5 same relevant authored and persistence contract. **Status: `NOT_DEMONSTRATED`** for every item. No AUTH-15 evidence transfers to SL-3 by family name.

## 3. What creating a Selective view produces (FACT unless marked)

| Item | Fact | Evidence |
|---|---|---|
| top-level definition name | frontal: `baseName` when `fondoCount <= 1`, else `baseName + " - frente F{fondo+1}"`; planta: `rackName.Trim() + " - planta"` when a name is set, else `"Selectivo planta - {N} frentes"`; sanitized by `BlockNaming.SanitizeBlockName` (replaces the reserved characters angle brackets, slashes, quotes, colon, semicolon, question mark, asterisk, vertical bar, comma, equals and back-tick by a space, and trims; an empty name becomes `"Cabecera"`) | `RackViewBaseName.cs:22-37, 186-191, 202`; `Application/BlockNaming.cs:12-29` |
| effective name uniquification | `UniqueBlockName` appends `_1`, `_2`, ... while `blockTable.Has(candidate)`; it never renames another block | `LHD:395-413` |
| nested definitions | created **before** the system block, each uniquified: frontal `SEL_FRONTAL_<Role>_<n>` (a piece signature occurring fewer than twice stays loose; annotations, dimensions and blank-block pieces are always loose); planta `PLANTA_CAB_<n>` (a cabecera with a custom override always gets its own group) | `LHD:36-54`; `SelectiveFrontalBuilder.cs:26,36`; `HeaderInstanceGrouper.cs:37-43,61-65,95-98`; `SelectivePlantaBuilder.cs:125-133,300` |
| entity kinds in a definition | `BlockReference` to library or catalog blocks, `BlockReference` to nested definitions (`ScaleFactors = 1`), `DBText`, `RotatedDimension`. **No** line, polyline, hatch, mtext, attribute definition or attribute reference is created by this path | `LHD:252-321, 338-376` |
| library blocks referenced | **no block-name constants**: names come from the data-driven catalog (`CatalogLookup.Block` -> `catalog.Blocks.FindBlock(pieceId, view).BlockName`, `Application/Catalogs/CatalogLookup.cs:19-20`); views `"FRONTAL"` (`Domain/Systems/Selective/SelectiveRackDefaults.cs:9`) and `"PLANTA"` (`SelectivePlantaBuilder.cs:26`); pieces: post (`system.PostId`), base plate, larguero (`level.BeamId`), tarima (`TARIMA_GENERICA` piece id), separador (`SEPARADOR_DE_CABECERA_FORMADA_DE_CINTA_CALIBRE_12`), safety elements. **UNKNOWN**: the `BlockName` each piece id maps to (`blocks.csv` is not in the repository) | `SelectiveFrontalBuilder.cs:53,98-99,461`; `SelectiveRackDefaults.cs:59`; `Domain/Systems/Dynamic/DynamicRackDefaults.cs:48` |
| dynamic-block parameters set | `LONGITUD`, `PERALTE`, `ALTURA`, `SAQUE`, `FRENTE`, `FONDO`; only on references **inside** definitions; names match case-insensitively, read-only skipped, values not found silently dropped; `RecordGraphicsModified(true)` if any applied. The top-level reference gets none | `SelectiveRackDefaults.cs:46-95`; `LHD:415-449` |
| layers created | `RACKCAD_ANOTACIONES` (ACI 2, texts) and `RACKCAD_COTAS` (ACI 1, dimensions); created lazily, only when a text or a dimension is emitted, inside the same transaction; no other property set. **UNKNOWN**: the layer assigned to block references and the text style of `DBText` (the code sets neither) | `LHD:324-331,270,356`; `LH.cs:16-28` |
| linetypes | none referenced on this path | grep of the Selective path |
| dimension style | a named `system.DimensionStyle` is used as-is when present in the drawing; otherwise the current `db.Dimstyle` plus per-dimension overrides (`Dimscale = 1`, `Dimtxt = h`, `Dimasz = 0.7h`, `Dimexe = 0.4h`, `Dimexo = 0.4h`, `Dimgap = 0.3h`, `Dimtad = 1`, `Dimdec = 2`) | `LHD:362-373,380-392` |
| attributes, annotative scale | none | grep |
| envelope | `RackEmbedDocument` (`Application/Persistence/RackEmbedDocument.cs:14-76`): `SchemaVersion`, `Kind = "selective"`, `View`, `Section`, `Id` (rack GUID), `Name`, `Design`, `CustomProperties`, `ExtensionData` (`[JsonExtensionData]`); the inner `Design` is `SelectivePalletDesignDocument` with its own `SchemaVersion` and `ExtensionData`; composed by `RackEmbedComposer.Compose` (throws if an extension key equals a declared member, `:103-120`) | `RackSelectivoCommands.cs:405-417` |
| envelope storage | an `Xrecord` under the key `RACKCAD_SELECTIVE` in the **extension dictionary of the block definition** (not the reference), chunks of at most 255 characters; the key is shared by all kinds; no XData, no NOD | `RBD:16-52`; `SBW:31-34` |
| sibling grouping | by `Embed.Id` (ordinal ignore-case); every sibling carries the identical `Design` JSON; `RackBlockFinder.ScanEnvelopes` skips layouts, anonymous and xref-owned records | `RackCommandSupport.cs:117-142`; `RackBlockFinder.cs:66` |
| view address | `RackViewCodec.Decode(kind, view, section)` never throws; Selective: frontal `Section >= 0` -> `Fondo(section)`; planta -> `Whole`; lateral -> `Post`; a negative frontal section is coerced to `Fondo(0)` (`SelectiveNegativeFondoToZero`) | `Application/Systems/Shared/RackViewCodec.cs:187-223` |
| reference placement | `new BlockReference(Point3d.Origin, definitionId)`; jig; appended to **model space** (`GetBlockModelSpaceId`); no scale, rotation, layer, dynamic property or attribute set | `BP:174-201` |
| NOD and project-variable writes | **none** in the creation path (project variables are written only by the edit path `ProjectVariableMutationExecutor`) | `ProjectVariablesData.cs`, `RackSelectivoCommands.cs:94-100,298-347` |

## 4. Sealed list (`VERIFY_ELEMENT_LIST`): enumeration for Selective

Sealed at PV (contract 10.2); each element has an expected value computed **before `Commit()`** from the plan and the oracle. Identity is by database handle with class and logical role. Layer of comparison: `SEM` = `SEMANTIC_COMPARISON`, `EXA` = `EXACT_STATE_FINGERPRINT` (BA-05).

| Class | Element | Identity | Expected value from | Layer |
|---|---|---|---|---|
| SL-D | each destination sibling **definition** of the new rack (frontal per fondo; planta) | handle of the block table record; effective name | plan and oracle (entities as a typed multiset with draw order where the plan determines it) | SEM |
| SL-N | each **nested definition** the seam creates (`SEL_FRONTAL_*`, `PLANTA_CAB_*`) | handle; effective name | plan | SEM |
| SL-R | each destination **reference** in model space: block name, position, scale factors, rotation, normal, layer | handle | plan and the computed transform `mu_k` (positive determinant, no negative scale) | SEM |
| SL-E | the authored **envelope** of every sibling (the `RACKCAD_SELECTIVE` Xrecord payload), including `ExtensionData` and unknown members | handle of the `Xrecord` | composed by the caller, written verbatim, read back and compared | SEM (V2.2 PA-8) |
| SL-A | the **view address** (`View`, `Section` -> `RackViewAddress`) and the grouping (`Embed.Id` = `NewRackId`, identical in every sibling of the logical rack) | handle of the definition | plan; `NewRackId` minted once per logical rack | SEM |
| SL-Y | layers **added by the seam** (`RACKCAD_ANOTACIONES`, `RACKCAD_COTAS`) when the plan emits texts or dimensions | handle of the layer record | plan (name, ACI colour) | SEM |
| SL-S | the **source read-set** elements: the source references, definitions and envelopes read at `S` | handles | unchanged from the `S` observation | EXA (S-4 / E-08) |
| SL-P | the **project-variable** entries the plan consumed (`PlanReadSet` of I-49; the registry key is `RACKCAD_PROJECT`) | key | unchanged | EXA |
| SL-C | the **containers** enumerated to check the list: block table name set, model-space handle set, the destination sibling set (`ScanEnvelopes` result), the extension dictionaries of the destination definitions | container handle | the sealed list itself | EXA (set) |

Excluded (host-derived, not fingerprinted as definitions): anonymous `*U` and `*D` block records; they are covered through their owning reference or dimension parameters (BA-05 section 2.4).

## 5. `PROTECTED_OBJECTS`

Per V2.2 PA-10, phase A is adjudicated over:

1. the **plan-time protected set**: the source objects (SL-S), the containers of SL-C that pre-exist (block table, model space, the destination sibling set as enumerated in PREPARE-R), and the project-variable entries of SL-P;
2. the **objects created inside `T_M`**: the handles that the seam results list for SL-D, SL-N, SL-R, SL-E, SL-Y.

At PV the sealed set is checked for consistency with this earlier domain (a member of the sealed set that the domain did not include aborts before `Commit()`). Events outside the domain are recorded and not adjudicated (V3 4.2 row E).

## 6. Abort read-set (`PRE_COMMAND_STATE_RECORD`), concretized

The scope statement of V2.1 11.1 applies: "unexpected mutation" is defined **only within this read-set**; content of non-RackCad entities outside it is not covered. Any component without an authoritative reader or comparator is an **execution blocker** (EXEC-4).

| Component | Compared | Reader / comparator status at the bound commit |
|---|---|---|
| block table names | the full name set | enumeration is trivial; **not present as an instrument: MISSING** |
| definitions | exact fingerprint of every RackCad-owned definition, every manifest definition, and every pre-existing homologue in the static dependency closure (section 7) | **canonical definition dump and fingerprint reader: MISSING** (BA-05) |
| model-space content | the full handle set of the space and the RackCad reference set with class | **MISSING** |
| RackCad payloads | scan of every definition's payload and envelope; the set of `RackId`; expected: no new `RackId` | `RackBlockData.Read` and `RackBlockFinder.ScanEnvelopes` exist, but `ScanEnvelopes` **skips layouts, anonymous and xref-owned records** (`RackBlockFinder.cs:66`), so payloads in those are outside the read-set; the aggregate scan comparator is **MISSING** |
| view addresses and grouping | per-view `RackViewAddress` and `Embed.Id` grouping | `RackViewCodec.Decode` and `RackCommandSupport.DecodeView` exist (code evidence); the host reader is **MISSING** |
| library-derived residue | every imported definition of the manifest and its dependency closure, exact fingerprint | **MISSING** (covered by the definition reader) |
| symbol tables | name sets of layers, linetypes, text styles, dimension styles and registered applications; exact fingerprint (or `ABSENT`) of each pre-existing record of the closure and of each record the seam uses or may touch: `RACKCAD_ANOTACIONES`, `RACKCAD_COTAS`, the dimension style in effect (the named `system.DimensionStyle` or the current `db.Dimstyle`), the text style in effect (**UNKNOWN which**) | **MISSING** |
| project-variable state | the entries of SL-P, exact fingerprint | the store exists (`ProjectVariablesData.cs`, key `RACKCAD_PROJECT`); the host aggregate reader is **MISSING** |

## 7. Static dependency closure

Computed in PREPARE-R **from the private library copy** (BA-04 scenario `PR-CLOSURE-COMPLETENESS`), by read-only traversal, without importing.

| Aspect | Rule |
|---|---|
| input | the required library block names, extracted by `RackBlockRequirementExtractors.HeaderRun.Extract` (pure; `Application/Systems/Shared/LibraryBlockRequirements.cs:53-80`); each block name is de-duplicated ignoring case |
| members | the required block definitions, their nested definitions, and the symbol-table records the traversal meets (layers, linetypes, text styles, dimension styles, registered applications), plus other cloned dependencies |
| classification | `ADD` (absent), `REUSE_AS_IS` (present, top-level required block), `REUSED_BOUND` (present, nested or dependency member), `REJECT` (name exists but as a layout, anonymous block or xref/overlay record) |
| name semantics | host symbol-table semantics (case-insensitive where the host applies it); the stored case is recorded |
| unclassifiable member | closure `UNKNOWN` -> refusal (O1) |
| import mode | `DuplicateRecordCloning.Ignore` or a proven non-replacing equivalent; the declared mode must be on the allow-list |
| what the seam adds itself | `RACKCAD_ANOTACIONES` and `RACKCAD_COTAS` are added by the seam inside `T_M` (SL-Y), **not** by the library import; they are not members of the library closure |
| **UNKNOWN today** | the contents of `blocks-library.dwg` and `blocks.csv`; the closure cannot be computed offline. **Library census required** (BA-05 item L-1) on the exact library file of the baseline (a tuple field) |

## 8. E-04 candidate set (criteria and candidates; the values are evidence-derived and UNSET)

Dimensions (contract 3.3): D-1 `CMDNAMES`; D-2 `CMDACTIVE`; D-3 registration flags; D-4 invocation route; D-5 nesting depth; D-6 invoking document is the active document.

| Route | Candidate | Fact at the bound commit |
|---|---|---|
| R-1 | typed at the command line | the existing Selective commands are `[CommandMethod("RS")]` and `[CommandMethod("RACKSELECTIVO")]` (`RackSelectivoCommands.cs:23,26`) with no `CommandFlags` anywhere in `src`; the future mirror command and the CT-21D harness command do not exist |
| R-2 | menu or ribbon (`RACKCAD`, `RK`) | `RackMenuCommands.cs:21,76-79` |
| R-3 | script | not used by the product |
| R-4 | LISP | not used by the product |
| R-5 | `SendStringToExecute` | not used by the product |
| R-6 | direct API call | not used by the product |

Designated routes: **`UNSET`** (candidate: R-1 for the harness command). Admitted values of D-1, D-2, D-3, D-5: **`UNSET`, evidence-derived**, fixed only before Owner Act 2. Verdict rules: contract 3.3.

## 9. `LOCK_MODE` candidate set

| Candidate | Description | Fact |
|---|---|---|
| LM-1 | parameterless `LockDocument()` | **every** `LockDocument` call site in `src` is parameterless (no `DocumentLockMode` argument, no `DocumentLock` variable): `CustomPropertiesExecutor.cs`, `Drawing/BlockPlacement.cs` (83, 135, 179), `Drawing/LateralHeaderDrawService.cs`, `Drawing/RackBlockRenamer.cs`, `InDocumentTransaction.cs`, `ProjectVariableMutationExecutor.cs`, `RackCantileverCommands.cs`, `RackCommandSupport.cs`, `RackInventarioCommands*.cs`, `RackLayoutCommands*.cs`, `RackSelectivoCommands.cs` (303), `RackVariablesCommands.cs`, `StructuralSectionCommandFlow.cs`, `Systems/Shared/SystemBlockWriter.cs` (23, 55). The default mode of `LockDocument()` is **UNKNOWN** from the code |
| LM-2 | explicit `DocumentLockMode.ExclusiveWrite` | no such site exists |
| LM-3 | other documented modes | only through a reviewed baseline amendment |

Selection: **`UNSET`**. Properties P-L1..P-L7 and the selection rule are those of contract 3.4 (selected by review, never by the implementer; if none demonstrates P-L1..P-L4, E-03 is not satisfiable and the class-A verdict is `FALSE`). The lock is held by the orchestrator from PL through V1; the existing separate placement lock (`BP:179`) disappears in the mirror path.

## 10. Semantic comparator and tolerance slots

The slots are those of BA-05 section 3.1; **every value is `TO_BE_FIXED_BY_SELECTIVE_KIND_BASELINE` and is `UNSET` here**.

| Slot | Compared quantity in this kind |
|---|---|
| `TOL_LENGTH`, `TOL_SCALE`, `TOL_NORMAL` | positions and scale factors of references and nested placements; the mirrored transform |
| `TOL_ANGLE`, `NORM_ANGLE` | reference rotation |
| `TOL_TEXT_HEIGHT` | height of `DBText` (annotation height is `6.0 x AnnotationScale`; rack-name label `x 1.5`, `Application/Systems/Selective/SelectiveAnnotations.cs:15,21`) |
| `TOL_DIM` | `RotatedDimension` points and overrides |
| `TOL_DYNAMIC` | dynamic-block parameter values (`LONGITUD`, `PERALTE`, `ALTURA`, `SAQUE`, `FRENTE`, `FONDO`) |

Order-sensitive: the entity sequence of a definition where the plan determines draw order. Unordered: the sibling set. Exact-case names. Gate (V2.2 PA-9): agreed before `BASELINE_READY`, sealed before the first governing scenario that uses them, not derived from governing outputs, any later change gives a new baseline.

## 11. Fingerprint composition (which layer for which element)

| Element | Layer |
|---|---|
| S-1 staged reads, S-2, S-3, SL-6 comparator (sealed-list classes SL-D, SL-N, SL-R, SL-E, SL-A, SL-Y) | `SEMANTIC_COMPARISON` |
| E-08 (SL-S, SL-P), containers SL-C, `PRE_COMMAND_STATE_RECORD`, `PREPARE_RESIDUE_MANIFEST`, `REUSED_BOUND`, `LIBRARY_EQUIVALENCE_OBSERVATION`, abort verification | `EXACT_STATE_FINGERPRINT` |

## 12. Fixture designs required (classes and constraints; no design is invented)

The designs themselves are `TO_BE_DEFINED`; each is a `SelectivePalletDesignDocument` inside the sealed scope that **avoids** the fail-closed conditions L-11 (corner fondos), L-12 (linked half-frentes), L-13 (topes) and L-14 (side protector, asymmetric deflector, overflowing grate or pallets), and the other V17 conditions listed in BA-03.

| Class | Constraint |
|---|---|
| `FX-1F` | one fondo, minimal levels; frontal and planta |
| `FX-NF` | two, three and four fondos (`MaxDepthCount = 4`); every frontal and the planta |
| `FX-ANN`, `FX-NOANN` | with and without annotations |
| `FX-DIM`, `FX-NODIM` | with and without dimensions (`DimensionViewPolicy.EffectiveDetail`) |
| `FX-IMP`, `FX-NOIMP` | library block absent (import needed) and present |
| `FX-LAY`, `FX-NOLAY` | annotation and dimension layers absent and present |
| `FX-BIG` | a plan whose quantities are above those of the learning fixtures (validation corpus, BA-06 section 4.3) |

## 13. Naming and identity facts the seams must respect

- The rack id is minted in the editor: `Guid.NewGuid().ToString()` in `RackEditorIdentity` (`RackCad.UI/Editor/RackEditorIdentity.cs:21`); the mirror's `NewRackId` is minted once per logical rack by the orchestrator (contract 15.5).
- Today the effective name is computed at draw time under the lock (`LHD:395-413`); in the mirror path PREPARE-R computes the expected effective names under the lock and records them (contract 15.5). **The final name of the mirror is an I-52 authority (V17), not fixed here.**

## 14. What is still missing before this artifact can support `BASELINE_READY`

| Id | Missing item |
|---|---|
| K-1 | the fixture designs of section 12 |
| K-2 | the library census (contents of `blocks-library.dwg` and `blocks.csv` on the exact baseline file) and, through it, the closure and the fingerprint type list |
| K-3 | the numeric tolerances and normalizations of section 10 |
| K-4 | the final plan quantity vector and shape classes (BA-06) |
| K-5 | the designated invocation routes and the `LOCK_MODE` selection are **evidence-derived** and cannot be fixed before characterization; the baseline needs only their **criteria and candidates** (this artifact) |
| K-6 | the readers and comparators marked MISSING in section 6 (execution blocker EXEC-4, not a baseline item) |
| K-7 | Coordinator and Architect agreement and the hash of this artifact |

```text
SELECTIVE KIND BASELINE = DRAFT ; NO EVIDENCE-DERIVED VALUE SELECTED
LOCK_MODE = UNSET ; E-04 ADMITTED SET = UNSET ; TOLERANCES = UNSET
CT21D_AUTHORITY_BASELINE_READY = FALSE
```
