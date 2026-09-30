# I-52 — CT-21D Baseline Artifact BA-06: EVM Preparation (DRAFT V2, STATIC REVIEW)

> **BASELINE ARTIFACT BA-06 V2 — DRAFT, NOT SEALED. Design / documentation only. Nothing here is executed or authorized.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-EVM-PREPARATION
> ARTIFACT_VERSION          = 2-DRAFT   (supersedes 1-DRAFT, blob 7b87c00414b42f4d711c18a45b9bd722af603533, which stays as history)
> ARTIFACT_STATUS           = DRAFT (phase 2)     CANDIDATE_HASH = UNSET     SEAL_HASH = UNSET
> AUTHORITY_CONTRACT        = CT-21D V2.1 27 + V2.2 PA-2, PA-10, PA-11 + V2.3 + V2.4 (STATIC_REVIEWED rule)
> INVENTORY_REVIEW_STATUS   = STATIC_REVIEWED PROPOSED   (conditions 1, 2, 5, 6, 7 of V2.2 PA-11 met for the design-time inventory;
>                                                          trace plan of conditions 3, 4 submitted; Architect approval pending)
> INVENTORY_COMPLETENESS    = NOT_ESTABLISHED   (until REVIEWED: dynamic trace on the implemented seams, before the first LEARNING run)
> LEARNED_EVM_CONTENT       = NONE
> BLOCKER                   = NB-4 (ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION; OPEN until STATIC_REVIEWED is approved and this artifact is sealed)
> EVIDENCE                  = docs/automation/evidence/I-52-ct21d-phase2-static-analysis/ (raw outputs of the independent review)
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 1. Two-stage review (V2.4)

| Stage | Object | Result | Gate |
|---|---|---|---|
| **E1 — design / static** | the **design-time inventory**: the reused product path at a bound SHA **plus** the operations required by the seam contract | `STATIC_REVIEWED` when conditions 1, 2, 5, 6, 7 of V2.2 PA-11 hold and the dynamic-trace plan of conditions 3, 4 is approved | **baseline** (this artifact) |
| **E2 — implemented-seam dynamic trace** | the implemented seams, every seam entry point and every plan-shape class | `REVIEWED` (complete) | **before the first `LEARNING` run**; needs the seams (implementation) and host work |

An operation added by the implemented seams after the design-time review reopens the review of its operation class (V2.4).

## 2. The independent static review (conditions of V2.2 PA-11)

| # | Condition | How it is met | State |
|---|---|---|---|
| 1 | static enumeration bound to the exact SHA | the reviewers read an extract of `origin/main` `3375aadbbf929427a6d106b2fff275d64863b89c` | MET |
| 2 | limits of reflection and dynamic dispatch declared | review limitations (evidence `B1a`, `B1b`) and residual table (section 5) | MET |
| 3 | dynamic trace covers every seam entry point | **plan** in section 7 (execution needs the implemented seams) | PLAN SUBMITTED |
| 4 | dynamic trace covers every plan-shape class | **plan** in section 7 | PLAN SUBMITTED |
| 5 | the reviewer did not compile or create the inventory | two **blind** reviewers (call-graph traversal; API-surface sweep) and a completeness critic, forbidden to read outside the extract, which does not contain the inventory; none compiled it. Independence is **procedural** (separate agent instances without access), which the Architect must accept | MET (subject to acceptance) |
| 6 | differences resolved explicitly | difference log, section 4; the critic adjudicated every A/B difference in the code (evidence `critic_ops`, table T2) | MET |
| 7 | facts seen by neither source remain residual and surface as unexplained learning events | section 5 | MET |

## 3. Design-time inventory V2

### 3.1 Reused product path (union adjudicated by the critic; evidence `critic_ops` T1)

Paths relative to `src/` of the bound SHA. `found_by`: `A` call-graph, `B` API sweep, `CRITIC` found only by the critic.

| id | category | api_call | location | seam_role | transaction_context | found_by |
|---|---|---|---|---|---|---|
| U-01 | lock (callee-owned L1) | document.LockDocument() | src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs:23 | Target: orchestrator only. Forbidden in SL-1/SL-2. | Outside any tx; spans U-02..U-27 | BOTH (A tx-site, B OP-01) |
| U-02 | import call site | BlockLibraryImporter.EnsureForPlan(database, plan); int discarded | src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs:25 | SL-5, misplaced inside the SL-1/SL-2 callee | Under L1, before T_create; no tx open | BOTH (A OP-06, B chain) |
| U-03 | read tx (doc DB) | StartTransaction; BlockTable ForRead; Has(name); Commit | src/RackCad.Plugin/Drawing/BlockLibraryImporter.cs:67-80 | SL-5 (read) | Callee read tx under L1 | BOTH (A tx-site, B OP-02) |
| U-04 | side-DB create | new Database(false, true) | src/RackCad.Plugin/Drawing/BlockLibraryDatabaseCache.cs:33 (via BlockLibraryImporter.cs:96) | Today inside the SL-5 callee. Target: orchestrator/PREPARE precondition, NOT an SL-5 action. | No tx; L1 + lock(CacheGate) :19; static cache :11-15. Conditional: a block is missing, File.Exists, and the cache misses. | BOTH (A OP-01, B OP-03) |
| U-05 | side-DB load | fresh.ReadDwgFile(path, FileOpenMode.OpenForReadAndAllShare, true, null) on the SHARED configured path | src/RackCad.Plugin/Drawing/BlockLibraryDatabaseCache.cs:36-40; path from src/RackCad.Application/Catalogs/BlockLibrary.cs:19-23 | Same as U-04 | Same as U-04 | BOTH (A OP-02, B OP-03) |
| U-06 | side-DB dispose | fresh.Dispose() on read failure; cachedLibrary?.Dispose() when superseded | src/RackCad.Plugin/Drawing/BlockLibraryDatabaseCache.cs:45, :49 | Same as U-04 | Same as U-04 | BOTH (A OP-03, B OP-03) |
| U-07 | read tx (side DB) | source.StartTransaction; BlockTable ForRead; Has / indexer -> ids; Commit | src/RackCad.Plugin/Drawing/BlockLibraryImporter.cs:102-115 | SL-5 | Callee tx on the side DB, under L1 | BOTH (A OP-04, B OP-04) |
| U-08 | clone into doc DB | new IdMapping(); source.WblockCloneObjects(ids, db.BlockTableId, mapping, DuplicateRecordCloning.Ignore, false); returns ids.Count; exceptions -> 0 | src/RackCad.Plugin/Drawing/BlockLibraryImporter.cs:117-131 | SL-5 (the only PREPARE-W write) | NO RackCad tx; under L1; before T_create; not undone by a T_create rollback | BOTH (A OP-05, B OP-05) |
| U-09 | read tx (doc DB) | AutoCadLibraryBlockQuery.Query: StartTransaction; Has; Commit. Facts are discarded. | src/RackCad.Plugin/Drawing/AutoCadLibraryBlockQuery.cs:24-37; src/RackCad.Application/Systems/Shared/LibraryBlockRequirements.cs:178 | SL-5 (post-import facts) | Callee read tx under L1 | BOTH (A tx-site, B OP-06) |
| U-10 | tx open (T_create) | database.TransactionManager.StartTransaction() | src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs:27 | SL-1/SL-2 (should receive T_M instead) | Under L1 | BOTH (A tx-site, B OP-07) |
| U-11 | symbol table write-open | tr.GetObject(db.BlockTableId, OpenMode.ForWrite) | src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs:31 | SL-1/SL-2 | T_create | BOTH |
| U-12 | BTR add (nested ARRAY defs) | NewBlock: new BlockTableRecord{Name=UniqueBlockName(bt, group.Name), Origin}; blockTable.Add; AddNewlyCreatedDBObject | src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs:38-51 -> 240-247, 395-413 | SL-1/SL-2. Effective name not returned. | T_create. Conditional: plan.Headers non-empty (frontal needs 2 or more bitwise-identical pieces, HeaderInstanceGrouper.cs:61-64; planta one group per distinct frame, SelectivePlantaBuilder.cs:300). | BOTH |
| U-13 | BTR add (view def) | NewBlock(blockTable, tr, systemBlockName, out systemName, out systemId) | src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs:54 -> 240-247 | SL-1 (frontal of one fondo) / SL-2 (planta) | T_create | BOTH |
| U-14 | entity create (catalog piece ref) | new BlockReference(pt, defId){Rotation, ScaleFactors(+/-1, +/-1, 1)}; space.AppendEntity; AddNewlyCreatedDBObject | src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs:305-317 (callers :44 into header def, :72 into view def) | SL-1/SL-2, nested (can be negative scale) | T_create. Per instance: skipped if the block is absent (:293-303). | BOTH |
| U-15 | dynamic property write | DynamicBlockReferenceProperty.Value = value | src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs:319 -> 432-438 | SL-1/SL-2, nested only | T_create, after append. Conditional: IsDynamicBlock and values present (:417); property writable and name matches (:434). | BOTH |
| U-16 | graphics | reference.RecordGraphicsModified(true) | src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs:445-448 | SL-1/SL-2, nested only | T_create. Conditional: at least one value applied. | BOTH |
| U-17 | entity create (ARRAY placement refs) | new BlockReference(pt, headerId){ScaleFactors = Mirrored ? (-1,1,1) : 1}; systemDef.AppendEntity; AddNewlyCreatedDBObject | src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs:56-68 | SL-1/SL-2 (Selective is always mirrored:false) | T_create. Conditional: as U-12. | BOTH |
| U-18 | layer create | LayerHelper.EnsureLayer(db, tr, 'RACKCAD_ANOTACIONES', 2): UpgradeOpen; new LayerTableRecord; Add; AddNewlyCreatedDBObject | src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs:270, 330-331 -> src/RackCad.Plugin/LayerHelper.cs:18-28 | SL-1/SL-2 | T_create. Conditional: an annotation is present and the layer is absent. | BOTH |
| U-19 | entity create (DBText) | new DBText{Height, TextString, LayerId, H/V modes, Position, AlignmentPoint}; AppendEntity; AddNewlyCreatedDBObject | src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs:258-279 | SL-1/SL-2 | T_create. Conditional: toggles are on (default false) and the text is non-blank (:260-263). | BOTH |
| U-20 | entity modify | label.AdjustAlignment(space.Database) | src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs:280 | SL-1/SL-2 | T_create | BOTH |
| U-21 | layer create | LayerHelper.EnsureLayer(db, tr, 'RACKCAD_COTAS', 1) | src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs:356 -> src/RackCad.Plugin/LayerHelper.cs:18-28 | SL-1/SL-2 | T_create. Conditional: a dimension is present and the layer is absent. | BOTH |
| U-22 | entity create (RotatedDimension) | ResolveDimStyle (read); new RotatedDimension(rot, p1, p2, dimLine, '', styleId){LayerId}; AppendEntity; AddNewlyCreatedDBObject | src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs:353-360, 380-392 | SL-1/SL-2 | T_create. Conditional: EffectiveDetail is not None (default None). | BOTH |
| U-23 | entity setters (DIMVAR overrides) | Dimscale / Dimtxt / Dimasz / Dimexe / Dimexo / Dimgap / Dimtad / Dimdec | src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs:362-373 | SL-1/SL-2 | T_create. Conditional: no named style is present. | BOTH |
| U-24 | host-generated block | dimension.RecomputeDimensionBlock(true) | src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs:375 | SL-1/SL-2 | T_create. Conditional: as U-22. | BOTH |
| U-25 | extension dictionary create | GetObject(defId, ForWrite); CreateExtensionDictionary() | src/RackCad.Plugin/Systems/Shared/RackBlockData.cs:27-31 (caller src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs:31-34) | SL-1/SL-2 | T_create. Conditional: payload non-empty; otherwise silently skipped. | BOTH |
| U-26 | Xrecord add (envelope) | ExtDict ForWrite; ResultBuffer of DxfCode.Text chunks of 255 chars or fewer; new Xrecord{Data}; SetAt('RACKCAD_SELECTIVE'); AddNewlyCreatedDBObject | src/RackCad.Plugin/Systems/Shared/RackBlockData.cs:16,33-39,46-51 | SL-1/SL-2 (no read-back) | T_create. Conditional: as U-25. | BOTH |
| U-26x | NOT REACHED on create | existing Xrecord ForWrite; existing.Data = buffer | src/RackCad.Plugin/Systems/Shared/RackBlockData.cs:41-45 | n/a (redefine only) | n/a | A (not reached); B listed it inside OP-18 |
| U-27 | tx commit (T_create) | transaction.Commit() | src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs:36 | Forbidden in SL-1/SL-2 | Ends T_create; L1 released at :39 | BOTH (A tx-site, B OP-19) |
| U-28 | lock L2 + tx open (T_place) | document.LockDocument(); StartTransaction() | src/RackCad.Plugin/Drawing/BlockPlacement.cs:179-180 | SL-4 (should receive T_M, take no lock) | After T_create committed | BOTH (A tx-site, B OP-20) |
| U-29 | non-resident ref + jig setter | new BlockReference(Point3d.Origin, defId); editor.Drag(new HeaderInsertionJig(...)); Update: ((BlockReference)Entity).Position = position | src/RackCad.Plugin/Drawing/BlockPlacement.cs:182-184, 240-244 | SL-4 (interactive today; only Position is set) | Inside T_place, with L2 held during user interaction | BOTH (A OP-23, B OP-21) |
| U-30 | cancel (empty commit) | reference.Dispose(); transaction.Commit(); return ObjectId.Null | src/RackCad.Plugin/Drawing/BlockPlacement.cs:186-191 | No counterpart under the contract | T_place | BOTH (A tx-site, B OP-23) |
| U-31 | model space write-open | GetObject(SymbolUtilityServices.GetBlockModelSpaceId(db), ForWrite) | src/RackCad.Plugin/Drawing/BlockPlacement.cs:193-194 | SL-4 | T_place. Conditional: Drag returned OK. | BOTH |
| U-32 | entity append (top-level ref) | modelSpace.AppendEntity(reference); AddNewlyCreatedDBObject(reference, true) | src/RackCad.Plugin/Drawing/BlockPlacement.cs:195-196 | SL-4 (no layer, dynamic property or RGM set) | T_place. Conditional: OK. | BOTH |
| U-33 | tx commit (T_place) | transaction.Commit() | src/RackCad.Plugin/Drawing/BlockPlacement.cs:198 | Forbidden in SL-4 | T_place | BOTH (A tx-site, B OP-22) |
| U-34 | compensating erase | LockDocument (L3); StartTransaction; GetBlockReferenceIds(true, false); UpgradeOpen; Erase; Commit | src/RackCad.Plugin/Drawing/BlockPlacement.cs:37-42, 123-160 | NOT_IN_SEAM (post-commit compensation) | L3 + T_cleanup, after T_create and T_place committed. Conditional: jig not OK and the def has no direct refs. | BOTH (A OP-26, B OP-24) |
| U-35 | purge + erase | db.Purge(ids); ((BlockTableRecord)GetObject(id, ForWrite)).Erase(); Commit; exceptions swallowed | src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs:193-238 via src/RackCad.Plugin/Drawing/BlockPlacement.cs:162 | NOT_IN_SEAM | T_purge inside L3 | BOTH (A OP-27, B OP-25) |
| U-36 | effect of U-35 (scope correction) | Candidates = the BTR of EVERY direct BlockReference in the view def. This includes catalog piece defs (possibly just imported by U-08, or pre-existing with no other refs). The anonymous dimension blocks are NOT collected. | src/RackCad.Plugin/Drawing/BlockPlacement.cs:146-151; src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs:70-76, 107-112 (contrast) | NOT_IN_SEAM | T_purge | CRITIC |
| U-37 | pre-creation read | ReadDimensionStyleNames: LockDocument; StartTransaction; DimStyleTable ForRead; Commit | src/RackCad.Plugin/RackCommandSupport.cs:146-179 (callers src/RackCad.Plugin/RackSelectivoCommands.cs:32,77; src/RackCad.Plugin/RackMenuCommands.cs:29) | None (read-only) | Own lock + read tx | BOTH |
| U-38 | pre-creation read | document.Database.Insunits read; Editor.WriteMessage | src/RackCad.Plugin/RackUnitsGuard.cs:34-45 (callers src/RackCad.Plugin/RackSelectivoCommands.cs:38,112; src/RackCad.Plugin/RackMenuCommands.cs:48) | None (read-only) | No tx | B (A named the call only) |
| U-39 | lateral branch (same entries) | LockDocument; EnsureForPlan; StartTransaction; CreateSystemBlock; RackBlockData.Write; Commit; PlaceAndReport | src/RackCad.Plugin/Drawing/LateralHeaderDrawService.cs:222-223, 245-260 (via src/RackCad.Plugin/RackSelectivoCommands.cs:461-464, 588) | NOT_IN_SEAM (lateral) | Own lock and tx; reuses U-03..U-35 primitives | A (OP-28..31); B (not_reached) |
| U-40 | not a DB operation | WrapSelectivePayload -> RackEmbedComposer.Compose + RackEmbedStore.Serialize | src/RackCad.Plugin/RackSelectivoCommands.cs:405-417 (planta :469, frontal :519) | Orchestrator input (pure) | None | B (OP-26); A (method). Excluded from the DB-op union. |
| U-41 | RACKEDITAR 'Insertar' preliminaries (not creation) | RedrawInPlace xN; RackBlockRenamer.SyncName; EraseViewBlocks; Editor.Regen; ReadProjectVariables | src/RackCad.Plugin/RackSelectivoCommands.cs:185-191, 221-227, 238-241, 254, 264, 303-315 | Outside creation (committed before :279) | Own locks and txs | B (tx-sites); A (not_reached) |

### 3.2 Operations required by the seam contract (evidence `critic_ops` T3; R-21 restored, section 4 D-09)

| id | required operation | seam | sources | verdict | current gap (evidence) |
|---|---|---|---|---|---|
| R-01 | The orchestrator takes ONE document lock spanning PREPARE-W and T_M; the seams take none. | ORCH | Derived from A CR-16, B OP-01 role, B CR-15 ('under the lock') | KEEP. CRITIC: neither review lists it as an operation; exact wording UNCERTAIN. | Three callee locks: src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs:23; src/RackCad.Plugin/Drawing/BlockPlacement.cs:135,179 |
| R-02 | The orchestrator opens T_M once and alone commits or aborts; no seam calls Commit, Abort or Dispose on T_M. | ORCH, SL-1/2/4 | A CR-16; B CR-01, CR-02, CR-17 | KEEP (merged) | src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs:36; src/RackCad.Plugin/Drawing/BlockPlacement.cs:189,198 |
| R-03 | Before any write, verify that TopTransaction.UnmanagedObject equals T_M.UnmanagedObject. | SL-1/2/4 | A CR-01; B CR-03 | KEEP | Exists only in the unreached src/RackCad.Plugin/Systems/Shared/RackDefinitionCreator.cs:151-164 |
| R-04 | Create the frontal definition for EVERY fondo k = 0..SelectiveDepthLayout.Count-1 in T_M. | SL-1 | A CR-02; B CR-04 | KEEP | One fondo per run, via prompt: src/RackCad.Plugin/RackSelectivoCommands.cs:493-515 |
| R-05 | Create the planta definition in the same T_M. | SL-2 | A CR-03; B CR-04 | KEEP | Separate run: src/RackCad.Plugin/RackSelectivoCommands.cs:467-472 |
| R-06 | Compute effective names (view and nested) deterministically and report them to the caller. | SL-1/2 (+ORCH) | A CR-07; B CR-12 | KEEP. B's empty-name sub-claim REJECTED for these paths. Conformance of UniqueBlockName UNCERTAIN. | src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs:40,54,80,395-413 |
| R-07 | Write the caller-composed envelope verbatim and never skip it silently. | SL-1/2 | A CR-04; B CR-10 | KEEP | src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs:31; src/RackCad.Plugin/Systems/Shared/RackBlockData.cs:22-25 |
| R-08 | Read the envelope back in T_M and compare ordinally; fail on mismatch. | SL-1/2 | A CR-05; B CR-10 | KEEP | Not on the create path; exists at src/RackCad.Plugin/Systems/Shared/RackDefinitionCreator.cs:204-211 |
| R-09 | The orchestrator mints NewRackId once, and it is identical in every sibling envelope. | ORCH | A CR-06; B CR-11 | KEEP | Minted in the UI: src/RackCad.UI/Editor/RackEditorSession.cs:126; or embed.Id: src/RackCad.Plugin/RackSelectivoCommands.cs:120 |
| R-10 | Append the top-level ref to model space via T_M, with no jig, prompt or Commit; exactly one ref per definition. | SL-4 | A CR-08; B CR-02, CR-04, CR-19 | KEEP | src/RackCad.Plugin/Drawing/BlockPlacement.cs:179-199 |
| R-11 | Set Position, ScaleFactors, Rotation and Normal from the computed transform. | SL-4 | A CR-09; B CR-05 | KEEP | Only Position is set: src/RackCad.Plugin/Drawing/BlockPlacement.cs:242 |
| R-12 | Guard the transform: determinant strictly positive and no negative scale. | SL-4 | A CR-09; B CR-06 | KEEP | No check exists. The pure src/RackCad.Application/Geometry/Transform2D.cs:77-83 is unused by placement. |
| R-13 | Set the layer and other entity properties from the plan. | SL-4 | A CR-10; B CR-07 | KEEP | None set: src/RackCad.Plugin/Drawing/BlockPlacement.cs:182-196 |
| R-14 | Apply dynamic properties to the top-level ref inside T_M. | SL-4 | A CR-11; B CR-08 | KEEP. It may be a no-op, since the view def is a plain BTR (UNCERTAIN, host). | src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs:243 (plain BTR); nothing on the top-level ref |
| R-15 | Call RecordGraphicsModified on the top-level ref inside T_M, with no post-commit graphics writes. | SL-4 | A CR-12; B CR-08, CR-09 | KEEP | Only on nested refs: src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs:447 |
| R-16 | Seams do not import; import is PREPARE-W, run before T_M. | SL-5 vs SL-1/2 | B CR-13; A OP-06 role | KEEP | src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs:25 |
| R-17 | SL-5 returns a structured result: Found/Missing per requirement, the imported count, and the failure cause. | SL-5 | A CR-13; B CR-14 (B CR-16 merged in) | KEEP | int only: src/RackCad.Plugin/Drawing/BlockLibraryImporter.cs:38; causes collapse to 0 at :88-91, 97-100, 126-131 |
| R-18 | SL-5 clones only from an already-opened library DB of a PRIVATE COPY. The orchestrator or PREPARE makes the copy, opens it, and disposes it after use. | SL-5 + ORCH | A CR-14; B CR-15. CRITIC adds the copy, open and dispose operations. | KEEP | src/RackCad.Plugin/Drawing/BlockLibraryDatabaseCache.cs:17-56; src/RackCad.Plugin/Drawing/BlockLibraryImporter.cs:87-96 |
| R-19 | No interaction, OpenCloseTransaction, side-DB write or compensating cleanup in SL-1/2/4. | SL-1/2/4 | B CR-19, CR-09; A CR-16 | KEEP | GetInteger: src/RackCad.Plugin/RackSelectivoCommands.cs:508; Drag: src/RackCad.Plugin/Drawing/BlockPlacement.cs:184; compensation: :37-42,123-170 |
| R-20 | SL-6 read-only verifier: names, envelope equality with expected, exactly one model-space ref per definition with the expected transform, layer and dynamic values. | SL-6 | A CR-15; B CR-18 | KEEP (non-mutating) | None. Read primitives exist: src/RackCad.Plugin/Systems/Shared/RackBlockData.cs:54-89; src/RackCad.Plugin/Drawing/RackSourcePlacementCaptureAdapter.cs:13-39 (no caller) |
| R-21 | Missing library pieces fail the seam. | SL-1/2 | B CR-16 | DEMOTE. Only 'surface them in R-17' is supported. Failing the seam is not in the quoted clauses (UNCERTAIN). | Pieces are skipped silently: src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs:293-303 |

### 3.3 Operation classes for the event model

| Class | Operations | Seam |
|---|---|---|
| `OC-BT-OPEN` | U-11 | SL-1, SL-2 |
| `OC-BTR-NESTED` | U-12 | SL-1, SL-2 |
| `OC-BTR-VIEW` | U-13 | SL-1, SL-2 |
| `OC-REF-PIECE` | U-14 | SL-1, SL-2 |
| `OC-DYN` | U-15, U-16 | SL-1, SL-2 |
| `OC-REF-ARRAY` | U-17 | SL-1, SL-2 |
| `OC-LAYER` | U-18, U-21 | SL-1, SL-2 |
| `OC-TEXT` | U-19, U-20 | SL-1, SL-2 |
| `OC-DIM` | U-22, U-23, U-24 | SL-1, SL-2 |
| `OC-ENVELOPE` | U-25, U-26, R-07, R-08 | SL-1, SL-2 |
| `OC-IMPORT` | U-03, U-07, U-08, U-09, R-16..R-18 | SL-5 (PREPARE phase: log only for E-12, V3 4.2) |
| `OC-REF-TOP` | U-31, U-32, R-10..R-15 | SL-4 |
| `OC-VERIFY-READ` | R-20 | SL-6 (expected write events: none) |

## 4. Difference log (BA-06 V1 inventory versus the independent review) and resolutions

| Id | Topic | BA-06 V1 inventory | Independent review | Resolution |
|---|---|---|---|---|
| D-01 | entry points | RACKSELECTIVO/RS and RACKCAD/RK | adds RACKEDITAR/RED "Insertar" (critic F-02) | ADDED as a product entry to the reused path; the mirror path has its own harness/command entry; not a seam operation |
| D-02 | block table record creation | OP-02, one class | U-12 nested ARRAY definitions and U-13 view definition | SPLIT into two operation classes: nested definitions carry no envelope and their names restart per plan (critic F-19) |
| D-03 | library side database | OP-16 classified SL-5 | U-04..U-06: today inside the importer callee; target: PREPARE precondition | RECLASSIFIED to the orchestrator (PREPARE-R acquisition of the private copy, V2.1 19.1 and V2.2 PA-6); SL-5 receives the opened Database; the static cache is not used by the mirror path |
| D-04 | pre-creation reads | not listed | U-37 ReadDimensionStyleNames (own lock and read transaction), U-38 INSUNITS read | ADDED as non-mutating pre-creation reads of the product path; in the mirror path the orchestrator performs any such read under its own lock, never inside a seam |
| D-05 | cancel-path erase and purge | NX-02 described as purging nested definitions | U-34..U-36 and F-13..F-15: the purge candidates include catalog piece definitions (possibly library-imported or pre-existing unreferenced ones); anonymous dimension blocks are not collected | CORRECTED fact of the product path; NOT in the mirror path (the mirror aborts T_M and never erases after commit; contract requirement R-19) |
| D-06 | envelope write | OP-12..OP-14 without the skip | F-09: the envelope is skipped silently when the payload is null or empty | ADDED as a product fact; the seam contract requires a verbatim write with no silent skip (R-07) and a read-back (R-08) |
| D-07 | Xrecord overwrite branch | OP-14: "not expected on a fresh definition (inference)" | F-10: FACT, unreachable on create | UPDATED to FACT |
| D-08 | operations required by the seam contract | absent | R-01..R-21 (critic T3) | ADDED as the contract-derived part of the design-time inventory (V2.4: "the reused product path at a bound SHA plus the operations required by the seam contract") |
| D-09 | R-21 "missing library pieces fail the seam" | not stated | critic DEMOTED it for lack of a quoted clause (critic F-26: the contract is not in the extract) | RESTORED: V2.1 15.3 ("a missing required block is a typed failure") and 15.4 (`MissingLibraryBlocks`) require it; the product path skips missing pieces silently today (U-14) |
| D-10 | R-14 dynamic properties on the top-level reference | not stated | critic: may be a no-op (the view definition is a plain block table record) | KEPT as a contract clause; the expected behaviour for Selective views is a no-op unless the plan sets properties; recorded for the event model |
| D-11 | lateral branch and RACKEDITAR preliminaries | not listed | U-39, U-41 | OUT OF SCOPE (lateral OUT, BA-03 V2; edit path) |
| D-12 | envelope composition | not listed | U-40 (pure Application code) | EXCLUDED from the database-operation inventory; it is an orchestrator input |
| D-13 | transaction and lock sites | listed as sites (TX-A..TX-E) | B1b listed them as operations | CLASSIFICATION only; kept as sites for E-06 sampling |
| D-14 | empty effective name | not listed | B1b raised it; critic F-20: unreachable on the Selective frontal and planta paths | RECORDED; the seam keeps the non-empty-name postcondition |
| D-15 | rack id | not listed | F-21: minted in the UI; no check that siblings share the id | contract requirement R-09 (the orchestrator mints `NewRackId` once and it is identical in every sibling) |
| D-16 | negative scale | not listed | F-24: negative scale only on nested piece references; the top-level reference gets none today | RECORDED; the positive-determinant rule of SL-4 applies to the top-level transform (R-12); nested mirrored pieces are product geometry |

## 5. Residual facts no static method can see (condition 7; evidence `critic_ops` T4)

| residual | affects | why not statically visible |
|---|---|---|
| Which dependent records WblockCloneObjects clones (layers, linetypes, text and dim styles, nested BTRs, regapps, dictionaries, dynamic-block data); whether it uses an internal transaction or undo; whether it touches the source side DB; state after a partial failure | U-08 | Host-internal behaviour of the AutoCAD API |
| That Transaction.Dispose without Commit rolls back, which is the only 'abort' on these paths; how UNDO groups the 3 to 5 separate commits of one command | U-10, U-27..U-35 | Host transaction and undo semantics |
| The implicit document lock of modal commands (no CommandFlags) and how explicit LockDocument calls nest inside it | U-01, U-28, U-34, U-37 | Host command runtime |
| Defaults applied on AppendEntity without SetDatabaseDefaults (layer, color, linetype, lineweight, and the DBText text style) for the nested refs, DBText, dimensions and the top-level ref | U-14, U-19, U-22, U-32 | Host defaults |
| BlockReference constructor defaults (Normal, Rotation, ScaleFactors); the coordinate system of the jig's AcquirePoint (UCS vs WCS) and the resulting real transform | U-29, U-32 | Host defaults and user UCS at run time |
| New BTR defaults (Units, BlockScaling, Explodable) and how they interact with INSUNITS | U-12, U-13 | Host defaults |
| Creation and naming of the anonymous dimension block by RecomputeDimensionBlock; how the DIMVAR overrides are persisted | U-22..U-24, F-15 | Host-internal |
| Anonymous representation blocks and enhanced-block data created by setting DynamicBlockReferenceProperty.Value; whether IsDynamicBlock is false for the plain view def | U-15, R-14 | Host-internal |
| Database.Purge semantics (references from erased owners, anonymous blocks), which decide the real extent of U-35/U-36 | U-35, U-36 | Host-internal |
| Round-trip fidelity of the 255-UTF-16-unit Xrecord chunks (a surrogate pair split at a boundary, per-string storage limits) | U-26, R-07, R-08 | Host storage behaviour |
| Library cache validity: a replacement file with the same mtime and length goes undetected; file sharing and locking under OpenForReadAndAllShare; the side DB is never disposed at unload (Terminate is empty) | U-04..U-06 | File-system and host state; src/RackCad.Plugin/PluginInitializer.cs:11-13 |
| Run-time data: live block-table contents (which blocks are missing, which suffixes apply); the user-settings library path; catalog contents; UI toggle values; user answers (fondo prompt, jig OK, cancel or Enter) | U-04..U-26, U-34..U-36 | Data-dependent |
| Whether the interactive prompts (GetInteger, Drag) update DB header system variables such as LASTPOINT | U-29, F-03 | Host-internal |
| TopTransaction wrapper identity and UnmanagedObject semantics, on which the R-03 check relies | R-03 | Host-internal |
| Selectability and extents of the placed ref without a Regen after commit | U-32, F-17 | Host display pipeline |

Each residual is expected to surface, if it produces events, as an **unexplained event** in learning (V2.1 27.2 step 4), which blocks the freeze of the model until it is explained.

## 6. Mutable static-state census (evidence `B2a`..`B2d`, `critic_census`)

| Project | cs files | Lines with word 'static' (incl. comments) | State-holding static members (fields + props) | Computed static props (=>, no state) | Tier A: written at runtime after init | Tier B: mutable by type, never written by repo code | Tier C: immutable | Strict mutable count (A) | Broad mutable count (A+B) |
|---|---|---|---|---|---|---|---|---|---|
| RackCad.Domain | 61 | 30 | 1 (0 fields + 1 prop) | 1 | 0 | 0 | 1 (SafetyFacePiece.None) | 0 | 0 |
| RackCad.Application | 457 | 2678 | 115 (85 fields + 30 props) | 31 | 3 (RackLog.sink; JsonRackCatalogProvider.Cache; CsvStructuralSectionCatalogProvider.Cache) | 43 | 69 | 3 | 46 |
| RackCad.Plugin | 70 | 248 | 8 (7 fields + 1 prop) | 1 | 4 (BlockLibraryDatabaseCache.cachedPath/cachedWriteUtc/cachedLength/cachedLibrary) | 0 | 4 (CacheGate, IsKnownKind, NoMissing, KindHandlerRegistry.Default) | 4 | 4 |
| TOTAL | 588 | 2956 | 124 | 33 | 7 | 43 | 74 | 7 | 50 |

| Tier | Member(s) | Location | Kind | In census? |
|---|---|---|---|---|
| A | RackLog.sink | src/RackCad.Application/Diagnostics/RackLog.cs:18 (writes :39, :61) | STATIC_MUTABLE_FIELD (volatile) | YES (B2b) |
| A | JsonRackCatalogProvider.Cache | src/RackCad.Application/Catalogs/JsonRackCatalogProvider.cs:48-49 (writes :75, :85-86) | LAZY_OR_CACHE | YES (B2b) |
| A | CsvStructuralSectionCatalogProvider.Cache | src/RackCad.Application/StructuralSections/CsvStructuralSectionCatalogProvider.cs:26-27 (writes :64, :76-77) | LAZY_OR_CACHE | YES (B2b) |
| A | BlockLibraryDatabaseCache.cachedPath / cachedWriteUtc / cachedLength / cachedLibrary | src/RackCad.Plugin/Drawing/BlockLibraryDatabaseCache.cs:12-15 (writes :49-53) | STATIC_MUTABLE_FIELD / LAZY_OR_CACHE | YES (B2c) |
| B | 14 static readonly JsonSerializerOptions: JsonRackCatalogProvider.SerializerOptions, CatalogBlockManifest.JsonOptions, FlowBedConfigurationStore, ProjectVariablesStore, RackEmbedStore.Options, RackFrameProjectStore, RackProjectStore, SelectivePalletDesignStore, ProjectVariableCloning.Options, SelectiveAuthoredAuthority.ComparisonOptions, RackFrameTemplateProvider, UserTemplateStore, UserSettingsStore.Options, AuthoredRawReader.Options | Application: Catalogs/JsonRackCatalogProvider.cs:32; Catalogs/Validation/CatalogBlockManifest.cs:48; Persistence/FlowBedConfigurationStore.cs:17; Persistence/ProjectVariablesStore.cs:32; Persistence/RackEmbedDocument.cs:81; Persistence/RackFrameProjectStore.cs:19; Persistence/RackProjectStore.cs:24; Persistence/SelectivePalletDesignStore.cs:15; ProjectVariables/MutationPlan.cs:256; ProjectVariables/SelectiveAuthoredAuthority.cs:86; RackFrames/RackFrameTemplateProvider.cs:19; RackFrames/UserTemplateStore.cs:21; Settings/UserSettings.cs:55; Systems/Shared/AuthoredRawReader.cs:13 | STATIC_READONLY_MUTABLE_OBJECT | YES (B2b) |
| B | 19 non-empty arrays: OperatorCharacters; CustomPropertiesStore.RootMembers/EntryMembers; RackEmbedComposer.DeclaredMembers; RetiredCantileverPunchMargins; VariableType.Supported; 9 StructuralSectionCsvSchema columns; StructuralSectionFamily.All; StructuralSectionSearch.Families; SectionProjectionCanonicalizer.Priority; DynamicSafetyDefaults.Families | Application: Expressions/OperatorInNameDetector.cs:32; Persistence/CustomPropertiesStore.cs:41-42; Persistence/RackEmbedComposer.cs:37; Persistence/RackProjectStore.cs:374; ProjectVariables/VariableType.cs:32; StructuralSections/StructuralSectionCsvSchema.cs:61,68,74,83,90,98,117,124,129; StructuralSections/StructuralSectionFamily.cs:56; StructuralSections/StructuralSectionSearch.cs:51; StructuralSections/Geometry/SectionProjectionCanonicalizer.cs:34; Systems/Dynamic/DynamicSafetyDefaults.cs:14 | STATIC_READONLY_MUTABLE_OBJECT | YES (B2b) |
| B | 7 empty List/Dictionary sentinels: PushBackBootPlan.None, PushBackDefensePlan.None, PushBackDiverterPlan.NoDiverters, PushBackPalletProjection.None, RackLevelElevations.Empty/NoEnvelope, SelectiveEditorOpen.NoStates | Application: Systems/PushBack/PushBackBootPlan.cs:82; PushBackDefensePlan.cs:59; PushBackDiverterPlan.cs:80; PushBackPalletProjection.cs:100; Systems/Shared/RackLevelElevations.cs:13,72; Systems/Selective/SelectiveEditorOpen.cs:70 | STATIC_READONLY_MUTABLE_OBJECT | YES (B2b) |
| B | RackFrameTemplateCatalog.All (mutable RackFrameTemplate graph) | src/RackCad.Application/RackFrames/RackFrameTemplateCatalog.cs:21 | STATIC_READONLY_MUTABLE_OBJECT | YES (B2b) |
| B | CantileverArmSectionPolicy.Empty, CantileverColumnBaseSectionPolicy.Empty (expose an internal List through IReadOnlyList) | src/RackCad.Application/Systems/Cantilever/CantileverArmSectionPolicy.cs:119; CantileverColumnBaseSectionPolicy.cs:90 | SINGLETON (latent-mutable) | YES (B2b) |

**Seam-relevant findings.** (1) `BlockLibraryDatabaseCache` (4 fields, single slot keyed by path, last-write time and length) is **not used by the mirror path** (the private-copy authority replaces it; V2.1 19.1); the warm-up must not fill it (BA-09 V2). (2) `JsonRackCatalogProvider.Cache` memoizes the catalog per directory and re-reads on a change of name, size or mtime of any catalog file: the **catalog folder content is build-bound** (it is copied with the build) and must be pinned by hash in the tuple (BA-08 V2 section 3). (3) `RackLog.sink` fixes the log directory at first use (file-system side effect only). (4) `UserSettingsStore` moves a corrupt `settings.json` to `.bad` on load (file-system side effect). (5) `RackCad.UI` has at least 4 non-readonly static fields (critic F19); the mirror path opens no UI, so they are outside the governed window, but the UI assembly load is a finding if it occurs inside the window. (6) No static constructor, module initializer, thread-static or static event exists in Domain, Application or Plugin.

## 7. Dynamic-trace plan (conditions 3 and 4; submitted for approval)

**Method (chosen).** Each trace is a **non-governing dry run** on the implemented seams, in a scratch document at the exact build:

| Source | Role in the trace | Why |
|---|---|---|
| T-1 database event log (E-12 instrument I-10: appended, modified, erased, opened-for-modify, reappended, unappended) | **primary** | effects on database objects, including host-internal secondary effects (anonymous blocks, cloned dependencies) |
| T-2 transaction-manager events | **primary** | transaction boundaries per seam call (E-06 sampling sites) |
| T-4 exact full-database fingerprint before and after (BA-05) | **complementary** | net effects without delivered events |
| T-3 external managed method-entry tracing | **rejected** | tooling does not exist and it would load code into the governed process |

**Coverage matrix.** Rows are seam entry points; columns are plan-shape classes (section 10.2). Every cell is one dry-run family; the evidence reference is `UNSET` until executed.

| Entry point | SH-F1 | SH-FN (2) | SH-FN (4) | SH-P | SH-FP | SH-ANN | SH-DIM | SH-IMP | SH-LAY-PRESENT |
|---|---|---|---|---|---|---|---|---|---|
| SL-1 (per fondo `k`) | UNSET | UNSET | UNSET | n/a | UNSET | UNSET | UNSET | UNSET | UNSET |
| SL-2 | n/a | n/a | n/a | UNSET | UNSET | UNSET | UNSET | UNSET | UNSET |
| SL-4 | UNSET | UNSET | UNSET | UNSET | UNSET | UNSET | UNSET | UNSET | UNSET |
| SL-5 (import needed) | n/a | n/a | n/a | n/a | n/a | n/a | n/a | UNSET | n/a |
| SL-5 (nothing to import) | UNSET | UNSET | UNSET | UNSET | UNSET | UNSET | UNSET | n/a | UNSET |
| SL-6 (read-only verify; expected write events: none) | UNSET | UNSET | UNSET | UNSET | UNSET | UNSET | UNSET | UNSET | UNSET |

**Reconciliation rule.** Every event class observed in a trace must map to an operation class of section 3.3; an unmapped class creates a new operation class, **reopens the review** of that class (V2.4) and is an unexplained event in learning. Every operation class with no observed event is recorded (a class that emits nothing is a fact, not a gap).

## 8. `RackDefinitionCreator` (AUTH-15) as an operation-class precedent only

Evidence `B6` (F1.14, F1.15): the Selective frontal and planta builders return `HeaderRunPlan`, and AUTH-15 overload A (`RackDefinitionCreator.CreateInTransaction(Database, Transaction, LateralHeaderDrawer, HeaderRunPlan, string, RackEmbedDocument)`) executes **the same drawer method** (`LateralHeaderDrawer.CreateSystemBlock`) as the Selective creation path, inside the caller's verified top transaction, with an ordinal envelope read-back and a typed `MissingLibraryBlocks` failure. It has **no production caller**. It is therefore an **implementation option** for SL-1 and SL-2 and an **operation-class precedent** (`OC-BTR-*`, `OC-REF-*`, `OC-DYN`, `OC-LAYER`, `OC-TEXT`, `OC-DIM`, `OC-ENVELOPE`). **No evidence transfers by family name**: the AUTH-15 host evidence (RUN-3) is not evidence for the Selective seams (P-8 remains required).

## 9. Learning-corpus definition

The learning corpus is the set of isolated micro-scenarios of the operation classes of section 3.3, their fixtures and the sessions that ran them, sealed by hash. Scenario classes (BA-04 V2): `EV-L-OP01`, `EV-L-OP02`, `EV-L-OP03-04`, `EV-L-OP05-06`, `EV-L-OP07-08`, `EV-L-OP09-10`, `EV-L-OP11`, `EV-L-OP12-14`, `EV-L-OP15`, `EV-L-OP17`, `EV-L-SEAM-REF-PROPS`, `EV-L-COMPOSED` (the V1 labels `OPxx` map to the classes of 3.3). Repetitions: `PARAM-01` (UNSET). **Governing learning requires `REVIEWED` (V2.4).**

## 10. Validation-corpus definition

### 10.1 Rules

Disjoint from learning (different sessions, fresh fixtures); governing only against a `FROZEN(hash)` model (V2.2 PA-2); includes plans structurally outside the learning corpus.

### 10.2 Plan-shape classes (final list for the baseline)

| Class | Description | Fixture (BA-08 V2) |
|---|---|---|
| `SH-F1` | one frontal view, single fondo | `FX-1F` |
| `SH-FN` | one frontal view of a multi-fondo rack (2 or 4 fondos) | `FX-2F`, `FX-4F` |
| `SH-FALL` | every frontal view of a multi-fondo rack | `FX-2F`, `FX-4F` |
| `SH-P` | the planta view | `FX-1F` |
| `SH-FP` | frontal view(s) and planta in one logical rack | `FX-1F`, `FX-2F` |
| `SH-ANN` | annotations present (texts; annotation layer created or reused) | `FX-ANN` |
| `SH-DIM` | dimensions present (dimension layer; anonymous dimension blocks) | `FX-DIM` |
| `SH-IMP` | library import needed | `FX-IMP` (construction open, BA-08 V2 K-8) |
| `SH-LAY-PRESENT` | annotation/dimension layers already present | `FX-ANN` with the layer present |

### 10.3 `STRUCTURALLY_OUTSIDE_LEARNING_CORPUS`

A validation plan is structurally outside when, for some dimension of the quantity vector (section 11), its value is not among the learning values (including a value above the maximum seen), or its shape class is not represented in learning. `FX-4F` and `FX-BIG` (BA-08 V2) are the designated structurally-outside validation fixtures.

## 11. `f(plan)` and the quantity vector (final candidate list)

`f(plan)` = the multiset union over the ordered seam operation instances of the plan of `E(operation class, Q(plan))` (V2.1 27.9), each multiplicity affine in `Q` or tabulated over structurally distinct shapes.

| `q` | Quantity | Source |
|---|---|---|
| q1 | frontal views selected (fondos) | `SelectiveDepthLayout.Count` (`Application/Systems/Selective/SelectiveDepthLayout.cs:18`) |
| q2 | planta views selected (0 or 1) | `RackSelectivoCommands.cs:233-244` |
| q3 | nested header groups per view | `HeaderInstanceGrouper` (frontal), `SelectivePlantaBuilder.cs:300` (planta) |
| q4 | loose piece instances per view | `LateralHeaderDrawer.cs:70-76` |
| q5 | nested placements per view | `LateralHeaderDrawer.cs:56-68` |
| q6 | annotation texts | `SelectiveAnnotations` |
| q7 | dimensions | `SelectiveDimensions` |
| q8 | dynamic-property applications | `LateralHeaderDrawer.cs:415-449` |
| q9 | distinct required library blocks | `LibraryBlockRequirements.cs:53-80` |
| q10 | layers created (0 to 2) | `LayerHelper.EnsureLayer` callers |
| q11 | envelope chunks (255 characters each) | `RackBlockData.cs:35-39` |
| q12 | top-level references placed (= views) | SL-4 |

## 12. EVM artifact schema (V2.1 27.6)

| Field | Content |
|---|---|
| `EVM_ID`, `VERSION`, `HASH` | identity of the model and hash of its canonical serialization |
| `LEARNING_CORPUS_HASH` | hash of the sealed learning corpus |
| `ENTRY_ID` | identity of one entry |
| `OPERATION_CLASS` | a class of section 3.3 |
| `EXPECTED_EVENT_SEQUENCE/SET` | multiset of (event kind, target selector, step) with multiplicity as a function of `Q`, and the partial order if any |
| `EXPLANATION_TYPE` | `PRODUCT_OPERATION`, `HOST_DOCUMENTED` or `HOST_OBSERVED_STABLE` (only as companion behaviour of a product operation, with evidence, citation, Architect review and risk label; never a free-standing permission) |
| `AUTHORITY/CITATION` | the operation, the documentation citation, or the micro-scenarios |
| `RAW_EVIDENCE_REF` | raw event logs |
| `RISK_LABEL` | `NONE` or the declared risk |
| `REVIEW_STATUS` | `DRAFT`, `UNDER_REVIEW`, `ACCEPTED`, `REJECTED` |

## 13. Status

| Item | State |
|---|---|
| design-time inventory | complete for review (sections 3, 4) |
| independent review (conditions 1, 2, 5, 6, 7) | met (independence subject to Architect acceptance) |
| dynamic-trace plan (conditions 3, 4) | submitted; approval pending |
| `STATIC_REVIEWED` | **proposed**; granted only by the Architect's approval of this artifact |
| `REVIEWED` / completeness | NOT ESTABLISHED (E2 needs implemented seams and host work) |

```text
NB-4 = ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION ; OPEN
INVENTORY_REVIEW_STATUS = STATIC_REVIEWED (PROPOSED)     INVENTORY_COMPLETENESS = NOT_ESTABLISHED     LEARNED_EVM_CONTENT = NONE
CT21D_AUTHORITY_BASELINE_READY = FALSE
```
