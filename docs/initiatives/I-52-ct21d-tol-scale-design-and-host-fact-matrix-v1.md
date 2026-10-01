# I-52 — CT-21D TOL_SCALE Authority and Test Design, Host-Fact Matrix, G6 Implication Review and Instrument Specifications (V1)

> ```text
> DOCUMENT            = Architect design document, prepared for the Coordinator's review (decisions section 237, "NEXT WORK" item A:
>                       "disenar TOL_SCALE authority/test; revisar cualquier implicacion de G6; fijar el host-fact matrix exacto").
>                       Not a baseline artifact; not in the BA-11 registry; not a contract. It decides no value.
> ROLE                = ARCHITECT
> STATUS              = PROPOSED_FOR_COORDINATOR_REVIEW. NO HOST RUN HAS STARTED. NO INSTRUMENT HAS BEEN WRITTEN, BUILT OR LOADED.
>                       NOTHING BELOW IS AUTHORIZED TO RUN BEFORE THE COORDINATOR'S RULING (section 6)
> REVISION            = the SAME draft, revised in place after the adversarial reviews (it is untracked; no new version). Every finding is answered in the
>                       Review record (section 10): applied, rejected with a reason, or turned into an open question. No value was chosen; no authorization widened
> AUTHORITY_CONTRACT  = CT-21D-V2.6 AGREED (decisions section 230; composite 97100cff11868807f3c49f25bf25d0385c86e8873cba0cf2c1996338b9b98eb4)
> BOUND_PRODUCT_SHA   = 95690c28dc6268e61dff32a0cbc33cc9fde3d47f (non-tuple bound item; `src/` of this worktree is byte-identical to it,
>                       checked with `git diff --stat main HEAD -- src`)
> OWNER AUTHORITY     = decisions section 237 and docs/automation/evidence/I-52-ct21d-owner-decisions-machine-g6-instruments.md (verbatim):
>                       G6 = EXTEND (non-governing, READ-ONLY, host-type facts that the CURRENT BA-05 specification expressly requires);
>                       INSTRUMENT_IMPLEMENTATION_GATE = OPEN_AFTER_ARCHITECT_TOL_SCALE_DESIGN; R8_READING = ACCEPT; AR10_03B_READING = ACCEPT;
>                       EVIDENCE_REPETITION = 1 and PARAM-04 = 3 accepted with their conditions
> EARLIER CONTEXT     = docs/initiatives/I-52-ct21d-host-gate-execution-package-v1.md (decisions sections 234 to 236)
> SOURCES (git blob ids of the working tree, `git hash-object`; the full list with the verification command is section 9)
>                       docs/automation/decisions/I-52.md  858509b4e2c8e30d5f04676946b07f768406018a
>                       docs/automation/evidence/I-52-ct21d-owner-decisions-machine-g6-instruments.md  2feb0bec63fd712cb28cc5209b6dffb8429a0e3b
>                       docs/automation/evidence/I-52-ct21d-owner-decisions-q-o1-q-o5-and-host-gate.md  cdb1a13682c63571cf4a4b4bb03bbf0fb0dc19ec
>                       docs/initiatives/I-52-ct21d-host-gate-execution-package-v1.md  b23221a8b4cb8f2b30135b1e811456be2cbb976c
>                       docs/initiatives/I-52-ct21d-baseline-ba-05-fingerprint-specification-v5.md  b74af94ece4f0901a071ef5176f3c58bff01cfdc
>                       docs/initiatives/I-52-ct21d-baseline-ba-08-selective-kind-baseline-v7.md  57330c3a09d80e919a63a2b83a45d168da326140
>                       docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v7.md  07e18f5f5ac2d5ee4c4c613e5762f2f57d47aef4
>                       docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v7.json  03c55e11570ce622e91bf5d48bba075891446e96
>                       docs/initiatives/I-52-ct21d-baseline-ba-06-evm-preparation-v7.md  74fa270ccba8dff89ccbf3ac71a125bb8c142b44
>                       docs/initiatives/I-52-ct21d-baseline-ba-10-parameter-sheet-v8.md  820d90c8a2172ad32697e66f0c9b0f3eb5fadbdb
>                       docs/initiatives/I-52-ct21d-baseline-ba-11-hash-registry-v10.md  9984e9f8ff6675afa514656e49d5a2fa01756916
>                       docs/initiatives/I-52-ct21d-baseline-ba-11-hash-registry-v9.md  6d6212a4bbddc8cb7c2ef3cbde7a56a16822f515
>                       docs/initiatives/I-52-ct21d-authority-contract-v2.1.md  6259a3beb7445a63620636acd696cdad689965b3
>                       docs/initiatives/I-52-ct21d-authority-contract-v2.2.md  fd7260c57ec05e11f6bbb9f67a728e7dfdf1f609
>                       docs/initiatives/I-52-ct21d-authority-contract-v2.6.md  fd9c16873c526e5f009ad92beb8dd2fe236b964f
>                       docs/initiatives/I-52-proposal-v17.md  dd944d8e9c62b083eebc2b1292789977b880c368
>                       docs/initiatives/I-52-g3a-ct49-session-host-api-characterization.md  ceb9f7210de2bcfde2b2228a727853c4d739265b
>                       docs/automation/evidence/I-52-ct21d-architect-phase1-ruling.md  8a433a000a40640a4d1302a2af24da114b0faaef
>                       docs/automation/evidence/I-52-ct21d-phase2-static-analysis/B5-tolerance-and-transform.json  d3d673245c072acf0ae4cd8b2c7e457ec1f72470
>                       docs/automation/evidence/I-52-ct21d-precondition-verifier.py  bc90b55f20c530e8eff25ffd931eb4b85c9303ae
>                       src/RackCad.Application/Systems/Shared/RackSourceTransformFactsV2.cs  25e812ee1e7b20455dc760e6416397ac65769425
>                       src/RackCad.Application/Geometry/Vector2D.cs  c88c67cdbff1a4a4b3695732b22003e129daba74
>                       src/RackCad.Plugin/Views/RackProjectionSnapshotReader.cs  3e39cca2cbe68ec6e5793c9ab9a80936e2c3690f
>                       src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs  65db706081e7c50f920acbd1b9f8c414f5a82f21
>                       src/RackCad.Plugin/Drawing/RackSourcePlacementCaptureAdapter.cs  23286d03eafee950292818b2be0e6e945608c22a
>                       src/RackCad.Application/Catalogs/BlockLibrary.cs  ce8042144e645a6408d7c3284b37e8808d2b4632
>                       deploy/build-bundle.ps1  9a4e6adecf935b90b52f97b1450aac30970d48f1
>                       eng/research/I52Ctda/README.md  e8cd6df4ac04d5f991ab07f655778a1ee2162661
>                       eng/research/I52Ctda/build-r3.ps1  999a86106215571bd690db01d84077a9f848e978
>                       eng/validation/I52G3AAutoCadProbe/ProbeCommands.cs  51189e537606b26d13686ec7c7f246af2173fa89
>                       eng/validation/I52G3AApiCensus/Program.cs  a41235ab740eaffed87e7b01c427e34408a47e2c
> TOL_SCALE_DESIGN                 = PROPOSED_FOR_COORDINATOR_REVIEW
> INSTRUMENT_IMPLEMENTATION_GATE   = NOT_OPEN
> HOST_RUN_STARTED                 = FALSE
> OWNER_QUESTIONS_PENDING          = Q-O-0 ((a) is the R0 residual inside "solo lectura"; (b) are new result files under the evidence folder, written through the single
>                                    EvidenceWriter, inside it; gated set = HF-G1 M1, HF-C1, HF-C3, HF-T2 and the I-2 R0 build that carries them; HF-G3 and I-4 are
>                                    NOT gated by Q-O-0), Q-O-1 (what the Owner's "solo lectura" admits) and Q-O-2 (HF-G2); until answered, the Q-O-0 gated set,
>                                    I-3, I-5, I-6, the HF-C4 write probe and the OP2 file save are BLOCKED, and no host activity starts and no tuple record is
>                                    closed until the whole declared set is final (section 6, R-G)
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 0. Reading rules

- This document **designs and proposes**. It chooses no tolerance value, runs nothing, writes no instrument and changes no baseline artifact. Where
  a source leaves a point open and the design depends on it, the point is a numbered ruling request in section 6; the Architect does not decide
  it silently.
- **UNKNOWN** means: not established in the repository. No hash, build, path, count, value or approval is invented.
- **`[HOST-TO-CONFIRM]`** marks a statement about Windows or AutoCAD behaviour that the Architect makes from general product knowledge and that no
  file of this repository establishes. The raw output on the designated machine governs; a mismatch makes the affected fact `UNKNOWN`, never
  a corrected guess.
- Citations are `path:line`; every cited file has its git blob id in section 9, and every id was checked by running git. Spanish passages are
  quoted verbatim from the Owner or the sources.
- The Owner's word "solo lectura" / "READ-ONLY" is **not redefined** by this document. Two descriptive classes are used, and they classify the instruments; they decide
  nothing: **STRICT READ-ONLY** (no API write call of any kind in the instrument's code; no save; no write to, and no write-open of, a product database, an
  open-document database, or a file of the product, the catalog or the library (the system-variable reader I-4 only reads variables of the active document); the only database the instrument
  constructs is an **in-memory side database that `ReadDwgFile` fills from a private copy** of a file, never saved and never attached to a document; **and it writes only new result files under the evidence folder through the single `EvidenceWriter` class**) and **WRITES-SIDE**
  (writes only to side databases that exist in memory, never attached to a document, and saves only new files under the evidence folder; nothing is written to a product
  database, product file, catalog file, library file, template, registry setting or system variable). STRICT is therefore **consistent with what R0 does** (it reads a private
  copy into an in-memory side database). It does **not** mean that nothing is written anywhere in memory: the host may evaluate, cache or write inside that side database or
  inside the process (for example the anonymous blocks of dynamic references, internal caches, a demand-load of a vendor module for proxy objects `[HOST-TO-CONFIRM]`), and no
  API-symbol scan can see it. That **R0 residual** is disclosed (5.3 item 4) and is put to the Coordinator and the Owner as **Q-O-0(a)** (section 6). The writing of new result files under the evidence folder through the
  single `EvidenceWriter` (and, for I-1, the typed AutoLISP `open ... "w"` file write of 3.2) is a second, distinct, explicitly disclosed element, **Q-O-0(b)**. Which class the Owner's word
  admits is **the Owner's question (Q-O-1, section 6)**; whether the R0 residual lies inside "solo lectura" is **Q-O-0(a)**; whether the new result files are is **Q-O-0(b)**; none is an Architect definition or an Architect fact.
- The vocabulary of results follows the contract: V2.1 21.1 (`NON_GOVERNING`, `INVALID`), 21.1.1 (retention), 21.3 (no automatic retry;
  `INVALID` retried only by the Coordinator's per-occurrence authorization), 21.6 (any UNKNOWN required reading is UNKNOWN).

## 1. Grounded facts (what the repository establishes)

Every later section relies on these rows and on nothing else from the repository.

| Id | Fact | Source |
|---|---|---|
| G-01 | The layer-2 comparator routes `ENTITY_INSERT.scaleX/Y/Z` to the predicate `ABS_DIFF_LE(TOL_SCALE)`, "absolute"; class `SCALE`; the 157-field routing is total. `ABS_DIFF_LE(TOL)` is `\|a - b\| <= TOL`, **per scalar component (never a Euclidean distance; AR3-20), absolute (never relative); -0.0 equals +0.0; a non-finite value is UNKNOWN** | BA-05 V5:1059, 1079, 1103 |
| G-02 | `DIMSCALE` "is a dimensionless factor compared exactly, **not** a scale factor of `TOL_SCALE`"; `linetypeScale`, `widthFactor`, `xScale`, `lineSpacingFactor`, `bulge`, `shapeScale` are `EXACT_DOUBLE` | BA-05 V5:1104, 1106 |
| G-03 | BA-05 states **no tolerance value** and has **no slot-unit semantic vector for scale**: the slot-unit vectors SV-06, SV-07, SV-10, SV-11, SV-14, SV-15 cover `TOL_LENGTH`, `TOL_ANGLE` and `TOL_NORMAL` only. `TOL_SCALE` "is not a gate of BA-05; it is K-3 of this artifact" | BA-05 V5:1123 to 1144, 1199; BA-08 V7:392 |
| G-04 | BA-08 V7 section 10 row `TOL_SCALE`: "**UNSET — NO AUTHORITY FOR CT-21D**"; V17 4.3 "Factor de escala: no existe"; "the product value is **absolute, not relative**, and **fixed at G4 with test and record**"; "the test needs host evidence (non-governing). **Not decided here**" | BA-08 V7:390 |
| G-05 | K-3 "`TOL_SCALE` decision, test and record": "separate decision; the test needs non-governing host evidence"; gate "before the candidate (a value of section 10)"; K-3 is a **direct blocker of the scenario catalog** (to declare the S comparisons) | BA-08 V7:632, 640 |
| G-06 | BA-04 V7: "`TOL_SCALE` is **not** a pin ... Every governing scenario whose run adjudicates S-1, S-2 or S-3 carries `BLOCKED_BY = K-3`"; a scenario in the AR3-01 mode records S-1..S-3 without adjudication and carries no K-3. Verified on the JSON: **138** of 437 scenarios carry `K-3`, all `GOVERNING` (115 `CHARACTERIZATION_BUILD`, 23 `PRODUCT_BUILD`) | BA-04 V7:29, 68, 132, 261; BA-04 V7 JSON (`SCENARIOS[].BLOCKED_BY`) |
| G-07 | V17 original sentence (`§4.3`, table row): the table row has four cells: `Factor de escala (adimensional)`, `no existe`, `—` and `ST-7 y ST-8. G4 fija el valor, absoluto y no relativo, con prueba y registro` (the cell separator is not reproduced, to keep this table well formed). The repository authority uses **absolute** tolerances in inches and rejects the relative epsilon by design. ST-7: "Valores absolutos de `sx`, `sy` y `sz` distintos segun la tolerancia de escala (§4.3; valor en G4)" is a **source classification** (fail-closed); ST-8 canonizes `sx < 0` and `sy < 0` to `(s,s,s)` with `θ + π`; ST-11 admits a uniform positive `s != 1` and `P'` conserves `s` | V17:912, 915, 916, 962 to 963, 972 |
| G-08 | V17 `CreateReference` sets the written scale by **direct assignment**: "`ScaleFactors = (s, s, s)`" | V17:2039 |
| G-09 | Phase-1 ruling item 5: "`TOL_SCALE` no tiene autoridad ... necesita una decisión con prueba y registro. Se pueden apoyar en mediciones de dry runs no gobernantes, nunca en salidas gobernantes" | Phase-1 ruling:16, 103 |
| G-10 | V2.2 PA-9: tolerances "be **agreed before `BASELINE_READY`**; be **sealed before the first governing scenario that uses them**; **not be derived from the outputs of governing runs**; if changed later, produce a **new baseline**". V2.1 28.2: layer 2 "fixes **no numeric tolerance**; the values are a baseline artifact"; "A signed-zero or other bit-level difference between domain-equivalent values is not a difference of this layer ... may be recorded as a secondary finding `BIT_LEVEL_DIFFERENCE_DOMAIN_EQUIVALENT`" | V2.2:291 to 299; V2.1:1534 to 1537 |
| G-11 | **A second consumer of the same declared tolerance exists in the contract:** SL-4 "accepts only a transform with **strictly positive determinant** and **scale within the kind's declared tolerance** (no negative scale, ADR-0036)"; failure `InvalidTransform` is PRE-WRITE; BA-06 control C-146 "Accept only a scale within the kind's declared tolerance"; BA-06 DC-13 records the same | V2.1:903, 915; BA-06 V7:399, 504 |
| G-12 | Fixture universe: piece references carry `MirroredX`/`MirroredY` as scale signs `(±1, ±1, 1)`; group placements carry `Mirrored` as an X scale of -1; every source reference of a fixture is placed by the jig with rotation 0, scale `(1,1,1)`, normal +Z. `PLAN_TRANSFORM` scale factors have a "strictly positive determinant" | BA-08 V7:111, 449, 606; `LateralHeaderDrawer.cs:62,147,313` |
| G-13 | The product writes a reference's scale by property assignment (`ScaleFactors = new Scale3d(...)`) and reads it with `reference.ScaleFactors` | `LateralHeaderDrawer.cs:62,147,313`; `RackSourcePlacementCaptureAdapter.cs:21` |
| G-14 | **Product precedent not cited by BA-08 V7 section 10:** `RackSourceTransformTolerance` has three caller-supplied fields and no default (`RackSourceTransformFactsV2.cs:56`; the earlier static analysis B5 F13 says the same: "no defaults or constants"). Its one production caller supplies `GeometryTolerance.Length` (1e-9) as the **scale** tolerance, with the comment "Shared scale, angle and normal tolerance of the source placement facts (X-7)" (`RackProjectionSnapshotReader.cs:25-27`). The classifier uses it for the sign test, "uniform" (`maxScale - minScale <= tolerance.Scale`) and "unit" (`Math.Abs(absX - 1.0) <= tolerance.Scale`) (`RackSourceTransformFactsV2.cs:209-211, 236-239, 249-253`). BA-08 V7 cites the reader file only as an SL-6 reader (lines 85, 216). This is a **precedent for the product's source classification (ST-7-like)**, not an authority for the CT-21D comparator (see section 2.2) | files cited |
| G-15 | `GeometryTolerance` constants are absolute, in inches (ADR-0005): `Length` 1e-9, `Continuity` 1e-7, `Angle` 1e-9; the comment rejects a relative epsilon ("sections span from a 1/8 in. wall to a 44 in. depth") | `Vector2D.cs:83-100` |
| G-16 | The census (BA-05 V5 section 5 step 2) reads dynamic properties "**through a reference created in a separate scratch database of the census (never in the library copy)**". So BA-05's own census procedure **creates entities in a side database**; "read-only" in that text is read-only on the library copy | BA-05 V5:1180 |
| G-17 | BA-11 V10 3.3 row 1 says "machine-attribute observation (**read-only, no instrument, no manifest**)"; row 2 "`TOL_SCALE` test and record (K-3)" has **no read-only qualifier**; row 3 "library entity-type census (K-2), with the AR4-10 confirmations (dimension-override storage, order and equal-to-style behaviour)"; row 4 fixtures and `FX-IMP` (K-8). The rows are textually identical in V9 and V10 | BA-11 V10 3.3; package V1:53 to 62 |
| G-18 | The Owner's G6 text: "Amplío BA-11 §3.3 únicamente para caracterización host NO gobernante, READ-ONLY, de los hechos adicionales necesarios para BA-05: tipos/rangos RB; variables de contexto; cualquier otro host-type fact expresamente requerido por la especificación actual de BA-05". Instrument gate: "se abre la implementación de instrumentos de **solo lectura** ... limitada a los host facts ya autorizados" | Owner record 237, points 2 and 5 |
| G-19 | Coordinator rulings (section 236): R-3 write probe of AR4-10 (G5) **IN**, "sobre una base de datos de trabajo"; R-3 G6 **OUT until explicit extension**; R-4 only `FX-ANN-BLANK` (build `69daf03a`) and `FX-IMP` advance, other fixtures wait; R-5 tools: new folder under `eng/research/`, read-only, Architect review, final and loaded before H1; R-8 `NETLOAD` of a pinned build is a build-tuple fact (ratified by the Owner with the condition "declared before the run and no substitution during it") | decisions section 236, 237 |
| G-20 | The existing G3A probe is **not** a read-only model: it calls `Overrule.AddOverrule(...)` (`ProbeCommands.cs:122,155`) and writes a JSON file (`ProbeCommands.cs:98`). It does read `HostApplicationServices.Current.MachineRegistryProductRootKey` and `UserRegistryProductRootKey` (`ProbeCommands.cs:54-55`) and the system variables by `Application.GetSystemVariable` (`ProbeCommands.cs:69`). The G3A record shows, in Core Console, the registry identity `Software\Autodesk\AutoCAD\R25.0\ACAD-8101:409` (machine and user) and product / program / market version `AutoCAD` / `acad` / `2025` (observation of an earlier machine, a hypothesis for the CT-21D tuple, not transferable) | `ProbeCommands.cs`; G3A characterization:37-39 |

## 2. Part A — TOL_SCALE authority and test design

### 2.1 What TOL_SCALE bounds (TS-D-01, TS-D-02)

**TS-D-01 (quantity).** `TOL_SCALE` is one of the **eight tolerance slots** that BA-05 V5 3.1 names (`TOL_LENGTH`, `TOL_TEXT_HEIGHT`, `TOL_DIM`, `TOL_ANGLE`, `NORM_ANGLE`, `TOL_NORMAL`, `TOL_DYNAMIC`, `TOL_SCALE`; BA-08 V7:382) and it belongs to **layer 2, `SEMANTIC_COMPARISON`, only**; layer 1 (`EXACT_STATE_FINGERPRINT`) compares the same doubles bit for bit and never reads a tolerance. `TOL_SCALE` bounds exactly one thing: the difference between the **expected** and the **persisted** value of each of the three
fields `scaleX`, `scaleY`, `scaleZ` of the record `ENTITY_INSERT` (exact host class `AcDbBlockReference`, BA-05 V5 2.3.1) in the semantic
comparisons S-1, S-2 and S-3 (contract 28.2; BA-05 V5 3.1.2 class `SCALE`), for **every** `ENTITY_INSERT` of the compared state: the top-level mirrored reference (list element SL-R, BA-08 V7:279) and the nested piece references and group placements inside the compared definitions (SL-D, SL-N); no other record type has a scale field of this class. The expected value comes from the sealed plan capture and the oracle
(BA-08 V7 section 11 rule 1); the persisted value is what the authoritative reader reads back (`reference.ScaleFactors`).

**TS-D-02 (form).** The bound is **absolute**, **dimensionless** and **per scalar component**:
a component is `EQUAL` when `|persisted - expected| <= TOL_SCALE`, never a Euclidean distance over the triple, never relative to the magnitude,
`-0.0 = +0.0`, a non-finite value `UNKNOWN`. Unit: a scale factor is a ratio of inserted size to definition size, so it has no inch unit
(contrast `TOL_LENGTH`, 1e-9 **in**); the "absolute, not relative" of V17 4.3 means that one number applies at every magnitude of the scale.

**What TOL_SCALE does not bound** (so the instrument must never record these as scale observations): `DIMSCALE` and every other dimension
variable (BA-05 V5:1106), `linetypeScale` and `CELTSCALE`, `widthFactor`, `xScale`, `shapeScale`, `lineSpacingFactor`, `bulge` (all `EXACT_DOUBLE`,
G-02); the annotation scale (`AnnotationScale = 1.0` in the fixtures); the net scale of `BlockTransform`; any unit-conversion factor the
host applies when a block is inserted by a command into a drawing of other units; the **number** of scale components that are negative (a sign
is a value, compared with the same predicate).

### 2.2 Consumers and scope (TS-D-03) — one slot, three possible uses

The repository shows **three** uses of "the scale tolerance". They are different questions and they must not be mixed in one measurement.

| Use | Question it asks | Where | In this design |
|---|---|---|---|
| C1 comparator | is the persisted scale equal to the expected scale (host and arithmetic noise only)? | BA-05 V5 3.1 and 3.1.2; BA-08 V7 section 10 | **YES — this design** |
| C2 SL-4 acceptance | is the supplied transform's scale inside "the kind's declared tolerance" (PRE-WRITE `InvalidTransform`)? | V2.1 15.6, 903; BA-06 C-146 | **NOT decided by this design. Slot split proposed**: the contract text gives no reference quantity for it (R-C) |
| C3 product source classification | is the source reference's scale uniform / unit (ST-7, ST-8, ST-11)? | V17 4.3; `RackSourceTransformFactsV2.cs:236-239` | **NO** — a product decision ("G4"), outside CT-21D evidence; the I-55 precedent (G-14) is cited, not adopted |

**TS-D-03 (C1 only; C2 is split off).** The design measures what C1 needs: the deviation that **host storage and the arithmetic between the sealed plan and the written
value** introduce. It does not measure what scale the product should accept (C2, C3). The Architect **withdraws** the earlier position that C2 reads the same value, for three reasons:

- (a) V2.1 15.6 and BA-06 C-146 say "scale within the kind's declared tolerance" without saying within tolerance **of what**: of 1, of the plan's scale `s`, of `|s - 1|`, or of the
  uniformity of the three components. The reference quantity of C2 is undefined, and a tolerance without a reference quantity is not a value;
- (b) a noise floor of host storage is evidence about C1 (the difference between an assigned and a re-read value). It is not evidence for a PRE-WRITE acceptance threshold on a
  transform that has not been written yet;
- (c) if the slot is retired for scale under branch R1(b) (route `scaleX/Y/Z` to `EXACT_DOUBLE`), "the kind's declared tolerance" of V2.1 15.6, C-146 and DC-13 would have **no referent**.

Therefore `TOL_SCALE` is the **C1 slot only**. C2 is a separate item that the next BA-08 version must either (i) declare as its own row with a defined reference quantity and its
own authority (candidates, listed without choosing: the strict positivity of each decomposed scale component; the uniformity `max - min` of the three column lengths; the unit-ness
`|c - 1|` for the fixture universe), or (ii) leave `UNSET`, in which case the SL-4 acceptance of BA-06 C-146 cannot be declared (the Architect's inference from BA-05 V5 3.1,
"a comparison whose slot has no sealed value ... cannot be declared", applied by analogy; the Coordinator verifies). The value of C2 is **not** derived by this design and is never derived from host noise.
This is a gap of the contract text that belongs to the Coordinator (R-C); it is stated here and not patched in a footnote. If the Coordinator instead rules to keep **one** slot, the
ruling must first define the C2 reference quantity and give a basis for the pre-write threshold that is not host noise, and R1(b) would still need a C2 row.

**C3 and the I-55 reader (precedent, not authority).** V17 4.3, ST-7, ST-8 and "G4" (V17:912, 972) are the original home of this slot: that is C3, a product decision outside CT-21D evidence.
The I-55 reader that is already integrated in `main` supplies `1e-9` as the scale tolerance of the source placement facts (`RackProjectionSnapshotReader.cs:25-27`; G-14), while BA-08 V7 does not cite it.
This design does not adopt that value for C1 or C2 and does not use it as a floor or a default. The next BA-08 version must **cite G-14** so that the baseline's value and the product's `1e-9` are never read as one decision;
whether C3 shares the slot is a product decision ("G4") outside CT-21D (R-C(b)).

**Authority of the scope.** The authority of `TOL_SCALE` extends only to the scale values of the sealed fixtures: the expected set is `{+1, -1}` per component. The fixtures place every
source reference at scale `(1,1,1)` (BA-08 V7:606, item 5), and the only change to a scale component that the fixtures need is its sign (G-12). A uniform `s != 1` (V17 ST-11) is **not** in the
fixture universe, and the plan's `s` is not an expected value of any fixture. The non-unit probe values of section 2.4 (PF-2 bases other than 1) are **characterization only** and do not extend
the authority to an `s != 1` governing scenario; such a scenario would need a new baseline version.

### 2.3 How a value will be derived from non-governing evidence (TS-D-04)

**No number is chosen here.** The derivation is a **closed rule**, applied by the Architect **after** the sealed evidence exists to inputs that are fixed **before the host run** (TS-D-05).
The rule defines the measurement, the rows in scope, the statistic, the acceptance criterion, the margin function and the decider; no parameter is left to be chosen once the evidence is open.

**Authority status of the rule (stated once).** The closed margin rule (the smallest decade strictly above `F`, no free factor) and the ceiling `tau <= TOL_LENGTH / E_max` are **Architect design proposals without repository authority**: no repository source states them, and the Coordinator may rule another closed function (R-M). The only sourced element is the value `TOL_LENGTH = 1e-9` inches (BA-08 V7 section 10; `src/RackCad.Application/Geometry/Vector2D.cs:93`).

**Measurement.** For each probe row `r` the instrument knows the intended value `a_r` (the exact binary64 that it assigned) and reads back `b_r`.
`delta_r = |b_r - a_r|` in binary64; it is **exact** when `a_r` and `b_r` have the same sign and `a_r/2 <= b_r <= 2 a_r` (Sterbenz), and the record flags
`deltaExact`. The deviation is taken at **both** observation points: OP1 (re-read of the same in-memory side database after the commit) and OP2 (save, reopen, re-read), see the next paragraph.

**Observation points and the persisted state.** The contract's comparator reads "persisted" state with a "fresh reread" (V2.1 15.7: SL-6 compares "expected versus persisted"). Neither V2.1 15.7 nor BA-08 V7 section 11
says whether the governing comparator reads only state that is in memory after the commit or state that has been through a DWG save and reload. The design does **not assume** either: OP2 is **mandatory** and
`D_A` is the maximum over **both** points. A conclusion "the host stores the scale exactly" drawn from OP1 alone would be unsound if the comparator reads reloaded state. If an authority later states
that the comparator reads only in-memory state, a new version of this design may drop OP2; it is not dropped here.

**Rows in scope.** The authority extends to the fixture universe only (TS-D-03: expected components `+1` or `-1`). The in-scope rows are PF-1 (132 rows, base magnitude 1) and the three PF-2 rows of base 1: **135 rows**.
The other **12** PF-2 rows (bases 0.5, 2, 25.4 and 1/25.4 with their neighbours) are **characterization**: they must still be observed (an unobserved row is UNKNOWN, so completeness is kept), their deviation is recorded as `DA_char`,
and they enter **neither** `D_A` **nor** `K_pres` **nor** any branch of the rule. A positive `DA_char` is a recorded finding for a future baseline version; it does not extend the authority to a non-unit scale.

| Symbol | Definition | Source of the number |
|---|---|---|
| `D_A` | `max(D_A,op1, D_A,op2)`, where `D_A,op1` and `D_A,op2` are the maximum `delta_r` over the **135 in-scope rows** at OP1 and at OP2 | host evidence |
| `DA_char` | the same maximum over the 12 characterization rows, both points; **informative only** | host evidence |
| `K_pres` | the **finest** ladder level `k` (largest `k`) such that every in-scope PF-1 row at every level not finer than `k` (levels `k' <= k`) is read back **bit-identical at both points**. `K_pres = 52` means every in-scope PF-1 row is bit-identical. For these rows `D_A = 0` is equivalent to `K_pres = 52` (no zero scale is ever assigned, FM-3); the offline recomputation checks the equivalence and a mismatch is INVALID evidence | host evidence |
| `I_max` | for the inventory command (P-R): the largest `\|value - nearest member of {+1, -1}\|` over the scale components of the references of the private library copy | host evidence (informative only) |
| `tau_arith` | the largest difference that the **declared formulas** can create between the value the oracle expects and the value the writer assigns (binary64 operation count times the ulp of the largest magnitude); **0** when the written scale is a bit copy of a plan scale, as V17 `CreateReference` and ST-11 describe (G-08) | **analysis by the Architect from the sealed oracle and writer rules; not host evidence**; derived and sealed **before** the host run (PRE-1) |
| `E_max` | an upper bound, in inches, on the distance from a scaled reference's insertion point to any point of its definition's geometry over every expected value of the sealed fixtures | **declared by the Architect from the sealed fixture plan parameters** (BA-08 V7 section 12), before the host run (PRE-3). No extents are read from the host (the optional extents field is **struck**) |

**Decision rule (closed; the Architect applies it after the evidence; the Coordinator verifies the text).**

| Branch | Condition | Result |
|---|---|---|
| **R1 EXACT** | `D_A = 0`, `K_pres = 52` and `tau_arith = 0` (sealed) | the host adds no noise and the writer copies the scale bit for bit: the only defensible absolute bound is **zero**. The Architect chooses (a) inscribe `TOL_SCALE = 0` or (b) retire the slot for scale and route `scaleX/Y/Z` to `EXACT_DOUBLE` in BA-05. The preference between (a) and (b) is fixed in the pre-registration (the design prefers (b): a zero slot cannot carry the "half the slot is EQUAL" vector pattern of BA-05 3.3). Either is a **new version** of BA-08 and of BA-05; for C2 see TS-D-03 (c). **(b) changes a comparator class, not only a value**: `scaleX/Y/Z` move from `SCALE` (`ABS_DIFF_LE(TOL_SCALE)`) to `EXACT_DOUBLE` (G-01, G-02), a change of the layer-2 routing; it needs the **Coordinator's ruling before the host run** (R-M) and is recorded in the pre-registration; until it is ruled, only (a) is available to a result R1 |
| **R2 BOUNDED** | `F = max(D_A, tau_arith) > 0` and `tau` (below) does not exceed the ceiling C-LEN | **one positive absolute value `tau`: the smallest member of the decade ladder `{10^-n, n integer}` that is strictly greater than `F`.** There is **no free margin factor**: the margin is structural (the value is above `F` by one decade step, a factor in (1, 10]) and **cannot be chosen after the evidence**. The written justification must still address (i) one tuple, (ii) a finite probe set, (iii) one execution, but it cannot change the value. **Ceiling C-LEN: `tau <= TOL_LENGTH / E_max`**: the placed geometry of a reference whose scale is wrong by `tau` moves by up to `tau x E_max` inches, and CT-21D asserts geometric equality at `TOL_LENGTH` (G-15), so a larger `tau` would make the scale comparison weaker than the length comparison it feeds. If `tau` exceeds the ceiling the branch is **R3** (there is no 1-2-5 step and no other relief). `E_max` is expected to exceed 1 in. for every rack view (G-15 names a 44 in. section depth; to be fixed by the pre-registration), in which case C-LEN is **stricter than the I-55 precedent of 1e-9** (G-14); the precedent can be adopted only through this rule, never by itself |
| **R3 UNUSABLE** | `tau` exceeds the ceiling, or the execution is UNKNOWN, FAIL or INVALID (section 2.5), or the host does not preserve small deviations in a way that lets the instrument see the quantity, or a sealed prerequisite (below) is missing | `TOL_SCALE` **stays UNSET** (section 2.8) |

**Sealed prerequisites of H4 and of the decision (TS-D-05a).** `tau_arith` and `E_max` depend on formulas and parameters that are not sealed today. Without them neither R1 (`tau_arith = 0` is an assumption until the writer
rule is sealed) nor R2 can be applied. The host run of P-A is therefore gated:

| Id | Prerequisite | State today |
|---|---|---|
| PRE-1 | the oracle's and the writer's scale rules (the SL-4 / `mu_k` formulas) are sealed in a baseline artifact, so that `tau_arith` can be derived with a written derivation | NOT MET: nothing is implemented (`grep RACKMIRROR src` finds nothing; package V1 section 3.4) |
| PRE-2 | the **route by which the SL-4 writer produces the scale** is fixed (assignment of `ScaleFactors`, `TransformBy`, clone or import). The probe uses the assignment route (V17 2039); a different final route needs a new test version, so **K-3 cannot close before the route is fixed** | NOT MET |
| PRE-3 | the fixture plan parameters of BA-08 V7 section 12 are sealed, so that `E_max` can be derived | NOT MET as sealed inputs |
| PRE-4 | the **pre-registration entry** (below) is written and sealed | NOT WRITTEN |

If PRE-1 to PRE-3 are not met the P-A host run does not start and `TOL_SCALE` stays UNSET; the Coordinator may rule a different order (R-M), but the pre-registration entry must exist before any run.

**Pre-registration (TS-D-05).** To keep the rule from being tuned to the result (PA-9: "never derived from governing outputs"; the same discipline applies to a non-governing number), one decisions-log entry is written and
sealed **before the host run of P-A**, not only before the evidence is opened. It fixes: `E_max` (defined in the symbol table above: an upper bound, in inches, on the distance from a scaled reference's insertion point to any point of its definition's geometry over every expected value of the sealed fixtures) with its derivation, and the ceiling `tau <= TOL_LENGTH / E_max`; the derivation of `tau_arith`; the closed margin function above (or the alternative closed function that the Coordinator rules,
R-M); the preference between R1(a) and R1(b), where R1(b) needs the Coordinator's ruling before the host run because it changes a comparator class (R-M); and the in-scope row set. The raw evidence folder is sealed by hash (`HASHES.sha256`) after the run and opened after the Architect confirms that the entry predates it. The decision
entry names the evidence by SHA-256.

**Who decides.** The Architect decides the value; the Coordinator verifies the text and inscribes it; the Owner is not asked for the value. The Owner stays the
decider of anything that changes a residual-risk disclosure (Owner Act 2 is reserved). No automatic adoption of a number by any tool.

**Transfer to other tuples (residual-risk disclosure).** The evidence is bound to the tuple of the run (no RackCad build loaded, no product module). Governing runs use other tuples, and the Owner's decision
(decisions section 237 point 3) is that evidence is **not transferred automatically** between builds. The value derived here is applied to the baseline by the Architect's declaration, not by an evidence transfer. That
application is a residual-risk disclosure item (the claim covers one tuple, 135 rows, one execution): the Coordinator records whether it accepts it in the decision record, and the final acceptance of residual risk stays with the Owner (Owner Act 2 is reserved).

### 2.4 The test as a host-run procedure (TS-D-06)

**Environment.** The machine and tuple of the CAD manager's record; a **fresh interactive `acad.exe`** (proposed, R-I; V2.1 2.3: Core Console observations are hypotheses, and this test is non-governing; the Coordinator's R-9, section 236, already admits the Core Console for the census when it is sufficient, and this design does not propose it for this test); no RackCad build is loaded (the probe does not need one); the instrument
DLLs of section 5 are **already in the declared set of the tuple record before H1** (R-5 of section 236; package V1 P-0.3; section 3.3) and are loaded by `NETLOAD` from the trusted folder declared in the tuple; an **unsaved empty drawing** as the active document (a new drawing from
the designated template copy; never a product file). Process hygiene as package V1 section 6 tension 4.

**What writes and what does not (stated, not ruled).**

| Part | Command / instrument | Operation | Class | Confinement |
|---|---|---|---|---|
| P-A | `CT21DHG_TOLSCALE` (I-6, RS DLL) | creates `BlockReference` entities with chosen scale factors, commits them, reads them back, **saves the side database to a DWG file and reopens it (OP2)** | **WRITES-SIDE. Not STRICT READ-ONLY.** | side database `new Database(true, true)`, never attached to a document, never a product database or product path; the active document is untouched (`DBMOD` equal before and after); the OP2 file is a new file under the evidence folder |
| P-R | `CT21DHG_INVENTORY` (I-2, R0 DLL; **a separate command with its own record**) | opens a private copy of the library file in a side database with `ReadDwgFile` and reads the scale of every `AcDbBlockReference` | STRICT READ-ONLY as defined in section 0 (no API write call, no save; the R0 residual is Q-O-0(a) and P-R is in the Q-O-0 gated set) | private copy hashed before and after |

**Owner question Q-O-1 (not decided here, and not an Architect definition).** The Owner's wording is "solo lectura" three times: decisions section 237 point 5 ("instrumentos de solo lectura"), point 8 item 5 ("instrumentos de solo lectura implementados y revisados")
and, in the Coordinator's section 236 list of the Owner decisions that remain, item (d) ("autorizar la compuerta de implementación de instrumentos de solo lectura"); the G6 text says "READ-ONLY" (G-18). The Coordinator's own section 236 says that what exceeds the Owner's authorization "se devuelve al Owner". P-A, I-3 and I-5 write to side databases and
P-A also saves a DWG file, so by their operations they are **not** STRICT READ-ONLY; whether the Owner's word admits them is the Owner's decision, not the Architect's and not a Coordinator-by-analogy reading. The textual evidence that exists is only evidence, not an answer: BA-11 3.3 row 2 ("`TOL_SCALE` test and record") has no read-only qualifier while row 1 has
one (G-17); BA-05 itself requires scratch references for the census (G-16); and the Coordinator's R-3 (section 236) admitted the write-back as a host **activity** on a work database, which is not the same as admitting a non-read-only **instrument** under the Owner's instrument gate.
**Until the Owner answers, I-3, I-5, I-6, the HF-C4 write probe and the OP2 save stay blocked** (R-A, R-B, R-G). Consequence to tell the Owner in one sentence: if the answer is STRICT, P-A cannot run, `TOL_SCALE` stays UNSET by section 2.8, the dynamic-property census
and the write-back probe cannot run, and BA-05 L-1 (dynamic part) and L-4 (write-back part) cannot close unless BA-05 is re-prepared without them.

**Inputs.**

*Ladder levels* `K = {52, 48, 44, 40, 36, 32, 30, 29, 28, 24, 20}`. The offset `2^-k` is exactly representable; `1 + 2^-k` is exactly representable for `k <= 52`.
The ladder runs from one binary64 ulp at 1.0 (`k = 52`) to `2^-20`, and straddles `2^-30` (9.31e-10) and `2^-29` (1.86e-9) so that the evidence shows whether the host preserves deviations below, at and above the order of the I-55 precedent;
the coarse levels `k = 24, 20` are visibility canaries.

*Probe table PF-1 (resolution ladder), 132 rows, in scope.* For each sign pattern `sigma` of `(X, Y)` in `{(+,+), (-,+), (+,-), (-,-)}` (Z always `+`), each component `c` in `{X, Y, Z}` and each `k` in `K`:
intended triple = `sigma` times `(1, 1, 1)` with component `c` replaced by `sigma_c x (1 + 2^-k)`. Sign patterns include the negative scales that the product writes on nested pieces and group placements (G-12) and the pair `(-,-)` that ST-8 canonizes.

*Probe table PF-2 (magnitude), 15 rows (3 in scope, 12 characterization).* For each base `b` in `B`: the uniform triple `(b, b, b)`; and, for each `b`, the two triples with `X` replaced by `nextUp(b)` and by `nextDown(b)`. `B` is given by exact bit patterns; **only base 1 is in scope**, the other four bases are characterization (section 2.3):

| Base | Why | Bits (binary64, hex) | nextUp | nextDown |
|---|---|---|---|---|
| 1 | the fixture universe (G-12); **in scope** | 3FF0000000000000 | 3FF0000000000001 | 3FEFFFFFFFFFFFFF |
| 0.5 | power of two, different ulp; characterization | 3FE0000000000000 | 3FE0000000000001 | 3FDFFFFFFFFFFFFF |
| 2 | power of two, different ulp; characterization | 4000000000000000 | 4000000000000001 | 3FFFFFFFFFFFFFFF |
| 25.4 | the inch/millimetre ratio: the drawing-unit hazard of ADR-0005 (`Vector2D.cs:86`); characterization | 4039666666666666 | 4039666666666667 | 4039666666666665 |
| 1/25.4 | the inverse ratio, `1.0 / 25.4` in binary64 division; characterization | 3FA42850A142850A | 3FA42850A142850B | 3FA42850A1428509 |

The probe values are **test inputs**, defined by bit pattern so that no decimal parsing is involved; they are not tolerances. Total **147 rows** (PF-1 132 + PF-2 15), of which **135 are in scope**. The sets are fixed by this design; a change is a new version of the design. **Row count note (duplicate triple).** The 147 rows are 147 distinct rows with distinct ids but **146 distinct intended triples**: the PF-1 row (sign pattern `(+,+)`, component X, `k = 52`) and the PF-2 row (base 1, X replaced by `nextUp`) assign the same triple `(3FF0000000000001, 3FF0000000000000, 3FF0000000000000)`. Both rows are kept (each is observed and recorded; nothing is merged) and both are in scope, so the 135 in-scope rows hold 134 distinct triples. `D_A` is a maximum and `K_pres` uses PF-1 only, so the duplicate changes no statistic; it is a second observation of one triple within one execution and is not a repetition (TS-D-07).

*Inventory command P-R inputs.* A private copy of the library file (hash before and after). The sealed fixture templates are **not** a source: they are built under ROW4 and, apart from `FX-ANN-BLANK` and `FX-IMP`, are OUT for now (R-4). No block extents are read (struck).

**Steps (P-A, then P-R).**

1. S0. Verify the tuple: the declared set (the R0 and RS DLLs and the package manifest) is already recorded; record the machine class label, `DBMOD`, `SECURELOAD`, `TRUSTEDPATHS`, profile; the module list **before any `NETLOAD`** (host baseline); hash the private files; verify no other `acad.exe`.
2. S1. **Load route (precondition).** The instrument folder is covered by `TRUSTEDPATHS` as set before the six strings were read (A1-R6, option (a)), so that no prompt is expected under `SECURELOAD` `[HOST-TO-CONFIRM]`, and `NETLOAD` is issued by a **script file or a typed full-path command** that needs no interactive answer `[HOST-TO-CONFIRM]`. The load outcome is recorded (loaded / prompt / refused). A prompt or a manual answer makes the run INVALID for P-A (section 2.5); a new attempt then needs the Coordinator's authorization after the route is repaired. Loading a declared module is an expected event of the declared set, not a plugin change (R-N).
3. S2. Run the command `CT21DHG_TOLSCALE`. It creates the side database and a target block table record `CT21D_TOLSCALE_TARGET` holding one `Line`.
4. S3. **T1**: for each row, construct `new BlockReference(point_i, targetId)` and assign `ScaleFactors = new Scale3d(x, y, z)` (the product's own construction and assignment pattern, G-13), append to the side database's model space, record the handle; **commit**.
5. S4. **T2** (a new transaction, objects opened for read): read `ScaleFactors`, `Rotation` (must be 0) and `Normal` (must be +Z) of every reference; abort (nothing is committed in T2). This is **OP1**, the re-read of the same in-memory database after the commit.
6. S5. Witness: for the rows with `k <= 36` (offsets of about 1.5e-11 and larger, which the arithmetic of the witness can resolve), compare the scale to the column lengths of `BlockTransform`; the witness bound is the witness's own arithmetic error and is stated in the instrument specification. A disagreement makes the row UNKNOWN, not noise.
7. S6. **OP2 (mandatory):** save the side database to a new file in the evidence folder, reopen it in a second side database, read again (T3). The two observation points are recorded in separate columns and both enter `D_A`. The saved DWG is **not byte-deterministic** (GUIDs, timestamps): it is excluded from `contentSha256` and hashed only as a raw file in `HASHES.sha256`.
8. S7. Run the **separate command** `CT21DHG_INVENTORY` (I-2, R0 DLL) in the same session and under the same tuple: the read-only traversal of the private library copy, recording per `AcDbBlockReference` the three scale components as bit patterns; summary: count, distinct triples, `I_max`. Its record is `inventory-raw.json`; it does not decide the class of P-A.
9. S8. Write the records, verify `DBMOD` unchanged, the private-file hashes unchanged, seal the folder, end the process, record the process list.

Not included, deliberately: `TransformBy(Matrix3d)` as a way to produce the scale (V17 2039 assigns directly; V17 R-53 lists `TransformBy` among calls to classify); a clone / import between databases (PREPARE-W). **The SL-4 writer route is not fixed (PRE-2); `TOL_SCALE` and K-3 cannot close before it is fixed**, and if it is `TransformBy` or clone / import the test design gets a new version (see section 2.7, FM-9).

**Repetitions (TS-D-07).** **One execution** (`EVIDENCE_REPETITION = 1`, Owner record 237 point 7). First principles: the quantity is a deterministic copy of
a double through a host API; one execution observes every probe row once; a second execution adds information only if the host were non-deterministic. The
claim that the evidence supports is therefore the **worst case over a finite, fixed probe set on one tuple**, not a statistical bound, and the strict decade step of branch R2 gives the structural margin that says so (no discretionary factor). **This count is not one of the statistical
repetition counts** (`N_A = 44`, `N_B = 29`, `N_det = 90`, `N_clean = 59`), which this activity neither uses nor reduces. If any `delta_r != 0` is observed, the Architect may ask the Coordinator
for **one additional authorized execution** (new run id, recorded cause, same package) to check that the non-zero rows repeat; it is a ruling request, never an automatic retry.

**Record format (JSON; `ct21d.tolscale.v1`).** One raw file `tolscale-raw.json` per execution. Doubles are 16-hex-digit binary64 patterns (as in the BA-05 vectors) plus a decimal rendering for reading only.

```json
{
  "schema": "ct21d.tolscale.v1",
  "governing": false,
  "gate": "HOST_GATE_BA11_3_3",
  "tuple": { "machineClassLabel": "MC-<12 hex> or UNSET", "buildTupleDigest": "<sha256>", "sessionId": "<id>",
             "runId": "HGP-H4-<yyyymmddThhmmssZ>-<nn>", "attempt": 1,
             "instrument": { "name": "...", "sha256": "<sha256>", "packageManifestSha256": "<sha256>",
                             "declaredSetSha256": "<sha256 of the declared R0 + RS instrument set>" } },
  "design": { "designBlob": "<git blob id of the ruled design>", "ba05Blob": "b74af94ece4f0901a071ef5176f3c58bff01cfdc",
              "probeTableSha256": "<sha256 of the PF-1/PF-2 table file>" },
  "rows": [ { "id": "PF1-0001", "family": "PF1|PF2", "signPattern": "++|-+|+-|--", "component": "X|Y|Z|ALL", "k": 52, "inScope": true,
              "intended": ["<hex>","<hex>","<hex>"], "op1": ["<hex>","<hex>","<hex>"], "op2": ["<hex>","<hex>","<hex>"],
              "rotationHex": "0000000000000000", "normal": ["<hex>","<hex>","<hex>"],
              "deltaOp1": ["<hex>","<hex>","<hex>"], "deltaOp2": ["<hex>","<hex>","<hex>"], "deltaExact": true, "bitIdentical": true,
              "witness": "AGREE|DISAGREE|NOT_APPLICABLE", "status": "OBSERVED|UNKNOWN", "reason": "" } ],
  "summary": { "rowsExpected": 147, "rowsObserved": 147, "inScopeRows": 135, "DA_op1": "<hex>", "DA_op2": "<hex>", "DA": "<hex = max(DA_op1, DA_op2)>",
               "DA_char": "<hex>", "Kpres": 52, "bitLevelSignedZeroRows": 0 },
  "checks": { "dbmodBefore": 0, "dbmodAfter": 0, "privateFilesUnchanged": true, "otherAcadProcess": false, "manualInput": false },
  "result": "PASS|FAIL|UNKNOWN|INVALID",
  "decisionEligible": true,
  "volatile": { "capturedUtc": "<ISO 8601>", "pid": 0 }
}
```

The decision statistics `D_A`, `K_pres` and `DA_char` are computed by the instrument from the rows and **recomputed offline** by the pure `core` library of section 5 (a mismatch is INVALID evidence). The file's `contentSha256` excludes `volatile`. The inventory of P-R is **not** part of this record: it is the separate file `inventory-raw.json` of the separate command `CT21DHG_INVENTORY` (I-2), written under the same `buildTupleDigest` (the declared set lists both DLLs), with its own schema (`schemas/`).

**Formal schema (JSON Schema 2020-12) of `tolscale-raw.json`.** The offline `core` library validates every record against it; a record that does not validate is INVALID evidence. `decisionEligible` is true exactly when `result` is `PASS` (the verdict and the eligibility are separate fields).

```json
{
  "$schema": "https://json-schema.org/draft/2020-12/schema",
  "$id": "ct21d.tolscale.v1",
  "type": "object",
  "additionalProperties": false,
  "required": ["schema", "governing", "gate", "tuple", "design", "rows", "summary", "checks", "result", "decisionEligible", "volatile"],
  "properties": {
    "schema": { "const": "ct21d.tolscale.v1" },
    "governing": { "const": false },
    "gate": { "const": "HOST_GATE_BA11_3_3" },
    "tuple": {
      "type": "object",
      "additionalProperties": false,
      "required": ["machineClassLabel", "buildTupleDigest", "sessionId", "runId", "attempt", "instrument"],
      "properties": {
        "machineClassLabel": { "type": "string", "pattern": "^(MC-[0-9a-f]{12}|UNSET)$" },
        "buildTupleDigest": { "$ref": "#/$defs/sha256" },
        "sessionId": { "type": "string", "minLength": 1 },
        "runId": { "type": "string", "pattern": "^HGP-H4-[0-9]{8}T[0-9]{6}Z-[0-9]{2}$" },
        "attempt": { "type": "integer", "minimum": 1 },
        "instrument": {
          "type": "object",
          "additionalProperties": false,
          "required": ["name", "sha256", "packageManifestSha256", "declaredSetSha256"],
          "properties": { "name": { "type": "string" }, "sha256": { "$ref": "#/$defs/sha256" },
                          "packageManifestSha256": { "$ref": "#/$defs/sha256" },
                          "declaredSetSha256": { "$ref": "#/$defs/sha256" } }
        }
      }
    },
    "design": {
      "type": "object",
      "additionalProperties": false,
      "required": ["designBlob", "ba05Blob", "probeTableSha256"],
      "properties": { "designBlob": { "$ref": "#/$defs/blob" }, "ba05Blob": { "$ref": "#/$defs/blob" },
                      "probeTableSha256": { "$ref": "#/$defs/sha256" } }
    },
    "rows": { "type": "array", "minItems": 147, "maxItems": 147, "items": { "$ref": "#/$defs/row" } },
    "summary": {
      "type": "object",
      "additionalProperties": false,
      "required": ["rowsExpected", "rowsObserved", "inScopeRows", "DA_op1", "DA_op2", "DA", "DA_char", "Kpres", "bitLevelSignedZeroRows"],
      "properties": {
        "rowsExpected": { "const": 147 },
        "rowsObserved": { "type": "integer", "minimum": 0, "maximum": 147 },
        "inScopeRows": { "const": 135 },
        "DA_op1": { "$ref": "#/$defs/hex64n" },
        "DA_op2": { "$ref": "#/$defs/hex64n" },
        "DA": { "$ref": "#/$defs/hex64n" },
        "DA_char": { "$ref": "#/$defs/hex64n" },
        "Kpres": { "type": "integer", "minimum": 0, "maximum": 52 },
        "bitLevelSignedZeroRows": { "type": "integer", "minimum": 0 }
      }
    },
    "checks": {
      "type": "object",
      "additionalProperties": false,
      "required": ["dbmodBefore", "dbmodAfter", "privateFilesUnchanged", "otherAcadProcess", "manualInput"],
      "properties": { "dbmodBefore": { "type": "integer" }, "dbmodAfter": { "type": "integer" },
                      "privateFilesUnchanged": { "type": "boolean" }, "otherAcadProcess": { "type": "boolean" },
                      "manualInput": { "type": "boolean" } }
    },
    "result": { "enum": ["PASS", "FAIL", "UNKNOWN", "INVALID"] },
    "decisionEligible": { "type": "boolean" },
    "volatile": { "type": "object", "additionalProperties": false, "required": ["capturedUtc", "pid"],
                  "properties": { "capturedUtc": { "type": "string", "format": "date-time" }, "pid": { "type": "integer" } } }
  },
  "allOf": [
    { "if": { "properties": { "result": { "const": "PASS" } }, "required": ["result"] },
      "then": { "properties": { "decisionEligible": { "const": true } } },
      "else": { "properties": { "decisionEligible": { "const": false } } } }
  ],
  "$defs": {
    "hex64": { "type": "string", "pattern": "^[0-9A-F]{16}$" },
    "hex64n": { "oneOf": [ { "$ref": "#/$defs/hex64" }, { "type": "null" } ] },
    "hex3": { "type": "array", "items": { "$ref": "#/$defs/hex64" }, "minItems": 3, "maxItems": 3 },
    "hex3n": { "oneOf": [ { "$ref": "#/$defs/hex3" }, { "type": "null" } ] },
    "sha256": { "type": "string", "pattern": "^[0-9a-f]{64}$" },
    "blob": { "type": "string", "pattern": "^[0-9a-f]{40}$" },
    "row": {
      "type": "object",
      "additionalProperties": false,
      "required": ["id", "family", "signPattern", "component", "k", "inScope", "intended", "op1", "op2", "rotationHex", "normal",
                   "deltaOp1", "deltaOp2", "deltaExact", "bitIdentical", "witness", "status"],
      "properties": {
        "id": { "type": "string", "pattern": "^PF[12]-[0-9]{4}$" },
        "family": { "enum": ["PF1", "PF2"] },
        "signPattern": { "enum": ["++", "-+", "+-", "--"] },
        "component": { "enum": ["X", "Y", "Z", "ALL"] },
        "k": { "oneOf": [ { "type": "integer", "minimum": 20, "maximum": 52 }, { "type": "null" } ] },
        "inScope": { "type": "boolean" },
        "intended": { "$ref": "#/$defs/hex3" },
        "op1": { "$ref": "#/$defs/hex3n" },
        "op2": { "$ref": "#/$defs/hex3n" },
        "rotationHex": { "$ref": "#/$defs/hex64n" },
        "normal": { "$ref": "#/$defs/hex3n" },
        "deltaOp1": { "$ref": "#/$defs/hex3n" },
        "deltaOp2": { "$ref": "#/$defs/hex3n" },
        "deltaExact": { "type": "boolean" }, "bitIdentical": { "type": "boolean" },
        "witness": { "enum": ["AGREE", "DISAGREE", "NOT_APPLICABLE"] },
        "status": { "enum": ["OBSERVED", "UNKNOWN"] }, "reason": { "type": "string" }
      },
      "allOf": [
        { "if": { "properties": { "status": { "const": "OBSERVED" } }, "required": ["status"] },
          "then": { "properties": { "op1": { "$ref": "#/$defs/hex3" }, "op2": { "$ref": "#/$defs/hex3" },
                                    "deltaOp1": { "$ref": "#/$defs/hex3" }, "deltaOp2": { "$ref": "#/$defs/hex3" },
                                    "rotationHex": { "$ref": "#/$defs/hex64" }, "normal": { "$ref": "#/$defs/hex3" } } } }
      ]
    }
  }
}
```

### 2.5 Classification of a non-governing characterization (TS-D-08)

The classes below are statements about the **measurement**; none is a verdict on the product or on a CT-21D control, and none counts toward acceptance (V2.1 21.1: "a valid execution that by design does not count toward acceptance").

| Class | Condition | Consequence |
|---|---|---|
| **PASS** (decision-eligible) | all of V1..V8: V1 tuple equal to the record (the declared instrument set included) and stable before/after; V2 all 147 P-A rows observed with finite values at **OP1 and OP2** (the 135 in-scope rows enter the rule; the 12 characterization rows must also be observed); V3 `Rotation` = 0 and `Normal` = +Z for every row; V4 the witness agrees on every row where it applies; V5 `DBMOD` unchanged, private files' hashes unchanged, no write outside the side database and the evidence folder; V6 the offline recomputation of the statistics equals the instrument's; V7 evidence sealed; V8 the load route of S1 was scripted or pre-trusted (no manual answer) | the Architect may apply the rule of section 2.3 |
| **FAIL** | a valid, complete execution whose observation **contradicts a design assumption** so that the rule cannot be applied as written: a row read back with a different sign or a magnitude that differs from the intended by more than the offset of its own level (the host transforms or canonizes the value; this is not noise); or the host rejects a probe construction as an error that is part of the data | recorded as a finding with the raw evidence; **no retry**; the Architect revises the design as a new version; `TOL_SCALE` stays UNSET meanwhile |
| **UNKNOWN** | a required observation is missing: an unreadable property, an exception, a non-finite value, a witness disagreement, a partial table, an instrument that cannot show it sees a deviation | the affected rows are `UNKNOWN`; any `UNKNOWN` row in PF-1 or PF-2 makes the execution `UNKNOWN` (fail-closed, V2.1 21.6); no substitute value |
| **INVALID** | tuple drift or mismatch, instrument error or crash, another `acad.exe` or interfering process, unsealed or unwritable evidence, any write outside the side database and the evidence folder, `DBMOD` changed, an unscripted dialog answered by hand, a SECURELOAD / TRUSTEDPATHS / profile difference between the before and after reads | all observed facts and logs retained (V2.1 21.1.1); cannot support a conclusion; **a rerun needs the Coordinator's per-occurrence authorization** with a new run id and a recorded cause; a crash needs a ruling (`CRASH_FINDING` or `CRASH_ENVIRONMENT`); never automatic |

The P-R inventory (`inventory-raw.json`, a separate record of a separate command) has its own record status (`OBSERVED` or `UNKNOWN` per reference) and does not decide the class of P-A.

### 2.6 What "record" means and how the result flows (TS-D-09)

"Test and record" (V17 4.3; BA-11 3.3 row 2; BA-08 V7 section 10) is three things, in this order:

1. **The evidence record**: the sealed raw folder of the execution (`tolscale-raw.json`, instrument log, `HASHES.sha256`), labelled `GOVERNING = FALSE`.
2. **The decision record**: the pre-registration entry (section 2.3), then the Architect's decision entry in the decisions log naming the evidence by SHA-256, the derived statistics, the branch (R1, R2 or R3), the value if any, the margin justification and the derivation of `tau_arith` and `E_max`.
3. **The inscription**: the value, its authority text and its status written into the `TOL_SCALE` row of BA-08 section 10 **in a new version of BA-08**. A silent edit of BA-08 V7 is not allowed (V7 stays as history).

| Artifact | What changes when the decision is R1 or R2 | If R3 |
|---|---|---|
| **BA-08** (new version, the next after V7) | section 10 row `TOL_SCALE`: value or "exact" with the authority text citing the records; section 16.1 K-3 closed; the header and the delta table; the C2 row or its UNSET status and the citation of G-14 (TS-D-03); the version is a new candidate input hash | row stays "UNSET"; K-3 open; text records the FAIL / UNKNOWN / INVALID outcome and cites the evidence |
| **BA-05** (its next version, which changes anyway for L-1 and L-4) | BA-05 V5 L-2 (V5:1199, AR3-17) and BA-08 V7:392 say that `TOL_SCALE` is **not a gate of BA-05** and that its value and status live only in BA-08. That stays true: the BA-05 changes below are consequences of the **form** of the value, not a gate. R2: the slot-unit vectors for `SCALE` that G-03 shows missing (an `EQUAL` vector at half the slot and a `DIFFERENT` vector at twice the slot, on a base far from 1 so that the absolute reading is separated from a relative one, as SV-07 does) are produced through the mechanism that BA-08 V7:392 describes (the harness generates the binary64 instances from the sealed value of the BA-08 table) or are kept in BA-08, as the Coordinator rules (R-C(c)); R1(b): the routing of `scaleX/Y/Z` moves to `EXACT_DOUBLE` in BA-05 and the `SCALE` class is dropped. Either way a new BA-05 hash changes the BA-05 candidate and, because BA-08 is bound to BA-05, the BA-08 candidate input hash, so the chain BA-05 -> BA-08 -> BA-04 is re-sealed in that order | no change |
| **BA-04** (its next version) | the "declaration of the S comparisons": for every scenario that adjudicates S-1..S-3, the compared scale fields now have a valued predicate, so `BLOCKED_BY` drops `K-3` for those scenarios (138 in V7) and the mechanical checks of the catalog are rerun; `STATUS` follows the catalog's own rules | the 138 scenarios keep `BLOCKED_BY = K-3`; the catalog cannot be sealed |
| **BA-06, BA-07, BA-09** | relink only: BA-06 C-146 reads "the kind's declared tolerance" and gets its cited BA-08 version | none |
| **BA-10** | state line "BASE-18 = NOT MET (... the BA-08 tolerances, K-3)" updated (line 330 of V8) | unchanged |
| **BA-11 registry** | new entries and hashes for each new version; sealing order as the registry's workflow | unchanged |

**What "declaration of the S comparisons" means for BA-04.** BA-04 V7 can state, for a scenario that adjudicates S-1, S-2 or S-3, the expected result of the comparison only if every predicate that the comparison uses has a sealed value in the kind baseline (BA-05 V5 3.1: "A comparison whose slot has no sealed value in the kind baseline cannot be declared"). The "declaration" is therefore the catalog entry's statement that its compared fields, including `scaleX/Y/Z` when the scenario's states hold references, are judged by the valued predicate of the BA-08 version it cites. Removing `K-3` from `BLOCKED_BY` is allowed only when that BA-08 version carries a value or an explicit "exact" for the scale predicate; it is a mechanical check of the catalog, not a judgement.

The value is sealed before the first governing scenario that uses it and is never derived from a governing output (PA-9).

### 2.7 Failure modes of the test itself

| Id | Failure mode | Treatment in this design |
|---|---|---|
| FM-1 | **The instrument cannot see the quantity**: it reads a cached or defaulted value, or the host reports the scale of another object (for example the anonymous representation of a dynamic block) | the ladder rows at `k = 24` and `k = 20` are visibility canaries; the `BlockTransform` witness is an independent path; a row whose offset is invisible at OP1 while the intended offset was assigned is `UNKNOWN`, never "noise zero"; P-A uses a plain (non-dynamic, non-anonymous) target |
| FM-2 | **Host rounding or quantization**: the host stores scale as fewer bits or snaps values near 1 | this is exactly what `K_pres` measures; a host that preserves fewer bits gives `D_A > 0` and branch R2 (or R3 if the floor exceeds C-LEN) |
| FM-3 | **Signed-zero lesson (V2.1 28.2; BA-05 SV-05, SV-18)** | a scale of zero is never assigned (degenerate in the classifier, G-14); `delta` is `Math.Abs` of the difference and is recorded as `+0.0`; a row whose bits differ only by the sign of zero counts in `bitLevelSignedZeroRows` and is the secondary finding `BIT_LEVEL_DIFFERENCE_DOMAIN_EQUIVALENT`, **never** `D_A` noise |
| FM-4 | **Dimension scale versus block scale** | the instrument reads only `BlockReference.ScaleFactors`; it never reads or records `DIMSCALE`, `LTSCALE`, `CELTSCALE`, `CANNOSCALE` as scale (G-02); the schema has no field for them |
| FM-5 | **Unit-conversion scale**: a command-level insert multiplies the scale by the ratio of block units to drawing units, an API construction does not | P-A constructs references through the API (the product's pattern); the probe base `25.4` and `1/25.4` show how the host stores an assigned conversion-sized value; conversion by `INSERT` is out of scope and, if the product ever relies on it, a new test version |
| FM-6 | **Intended value corrupted by text parsing** | the probe table is defined by bit patterns (section 2.4); no decimal literal is parsed at run time; the offline recomputation uses the same bits |
| FM-7 | **Scale versus `Normal` / `Rotation`**: scale is in the object coordinate system | `Normal` = +Z and `Rotation` = 0 are set and verified on every row (V3); any other value is UNKNOWN |
| FM-8 | **Finite probe set, one tuple, one execution** | stated in the claim (section 2.4); covered by the strict decade step of R2; no confidence statement is made; the application of the value to other tuples is a disclosed residual risk (section 2.3, "Transfer to other tuples") |
| FM-9 | **The probe produces the scale differently from the future writer** (assignment versus `TransformBy` versus clone) | TS-D-06 fixes the assignment path of V17 2039; a different path in the final SL-4 is a new test version before sealing, and K-3 cannot close before the route is fixed (PRE-2) |
| FM-10 | **Leak into the active document or a product file** | side database only; `DBMOD`, file hashes, and the static forbidden-API gate of section 5.3 |
| FM-11 | **The arithmetic term is hidden**: `D_A = 0` shows the host adds nothing, not that the oracle and the writer agree | `tau_arith` is a separate, analytical input to the rule, with a derivation in the decision record; R1 requires it to be 0, derived and sealed before the host run (PRE-1) |

### 2.8 Fail-closed fallback (TS-D-10)

If no usable absolute tolerance can be derived (branch R3, or the execution is FAIL, UNKNOWN or INVALID and no authorized rerun reaches PASS):

- `TOL_SCALE` **stays UNSET — NO AUTHORITY FOR CT-21D**; BA-08's row, K-3 and the 138 BA-04 `BLOCKED_BY = K-3` markers stay as they are;
- the S comparisons that touch `scaleX/Y/Z` are **not admissible**: BA-05 V5 3.1 says "A comparison whose slot has no sealed value in the kind baseline cannot be declared", and the routing is total, so a scale field cannot be dropped from the comparison;
- no scenario that adjudicates S-1..S-3 can be sealed as governing; `BASELINE_READY` stays FALSE; `CT21D_EXECUTION` stays NOT_AUTHORIZED; `CIA` stays UNKNOWN; `ALT-21E` stays the fallback;
- the design does **not** relax the absolute rule, does not substitute the I-55 precedent, does not use a governing output, and does not narrow the scope; a scope change is the Owner's;
- the evidence stays retained (V2.1 21.1.1) and the Architect issues a new design version for the cause found.

## 3. Part B — Host-fact matrix (exact)

### 3.1 Conventions

**Tuple binding codes** (every observation carries them, Owner record 237 point 2: "Toda observación debe quedar ligada a build/machine/session tuple exacta"):

| Code | Fields |
|---|---|
| TB-C | `machineClassLabel` (the `MC-` label, or `UNSET` before it exists) and the digest of the six strings |
| TB-B | `buildTupleDigest` = SHA-256 of the canonical serialization of: AutoCAD version/build, `acad.exe` SHA-256, API assembly versions and SHA-256, every instrument DLL SHA-256 and the instrument package manifest SHA-256, the RackCad build identity when one is loaded (source SHA, Plugin and UI DLL SHA-256, configuration), the loaded-module baseline hash |
| TB-S | `sessionId`, `pid`, `processStartUtc`, `runId`, `attempt`, document and database identity |
| TB-L | library source path and SHA-256 before and after; private copy path and SHA-256 before and after; catalog-folder file hashes |
| TB-R | the git blob ids of the ruled design and package, and the **blob id of BA-05 as used** (`b74af94e...` for V5): a new BA-05 version makes the matrix stale and requires re-derivation |

**Binding rule.** `TB-C` alone is never a sufficient binding: every observation row also carries `TB-S` (the session identity of the capture), because the Owner requires every observation to be bound to the exact build, machine and session tuple (decisions section 237 point 2).

**Fact-level result vocabulary** (a fact is not a scenario; this is the package V1 4.0 treatment with the facts named): `OBSERVED` (read, value recorded); `OBSERVED_DIFFERS` (read; differs from BA-05's expectation — not a FAIL, it changes BA-05 under its version rule); `NOT_OBSERVED` (no data in the scope of the read, for example an RB range with no occurrence; never filled by a guess); `UNKNOWN` (could not be read); `INVALID` (stop condition or tuple problem; Coordinator-authorized rerun only); `NOT_OBSERVABLE` (no read-only source can yield the fact; the matrix states in advance the source it depends on; it is not `UNKNOWN`, which is a failed read).

**Common record** (`ct21d.hostfact.v1`):

```json
{ "schema": "ct21d.hostfact.v1", "factId": "HF-...", "gate": "HOST_GATE_BA11_3_3", "governing": false,
  "tuple": { "TB-C": {}, "TB-B": {}, "TB-S": {}, "TB-L": {}, "TB-R": {} },
  "observation": {}, "status": "OBSERVED|OBSERVED_DIFFERS|NOT_OBSERVED|NOT_OBSERVABLE|UNKNOWN|INVALID",
  "rawRefs": [ { "path": "<relative>", "sha256": "<sha256>" } ], "volatile": { "capturedUtc": "<ISO 8601>" } }
```

**In-gate basis codes.** `ROW1..ROW4` = BA-11 3.3 rows 1 to 4 (G-17). `G6` = the Owner's extension, with the BA-05 sentence that expressly requires the fact. `R-3` = the Coordinator's ruling of section 236 for the write probe. `OUT` = excluded. `NEEDS-OWNER` / `NEEDS-COORDINATOR` = not classifiable from the text alone; flagged, not decided.

### 3.2 Resolution of A-1: the exact string of each PARAM-05 attribute (proposal A1-R1..A1-R6)

The label hashes the exact text, so the text must be fixed. Package V1 A-1 and AR10-03 (which deferred it to BA-10 V9 "junto con la etiqueta") are answered here.
Each read is performed by the CAD manager (BA-11 row 1: "read-only, no instrument, no manifest"), and the record keeps **the command, the raw output, the extracted string and the time**. The string that enters the label is the **extracted string**, written as one raw file per attribute whose bytes are exactly the string in UTF-8 without BOM plus one line terminator. No trimming, no case folding, no path normalization, no reordering. An empty string is a valid value (`TRUSTEDPATHS`). A value that is not valid UTF-8 is `UNKNOWN`.

**Transfer mechanism (one, fixed).** Copying text from the command-line window, the clipboard or any console is **not** a transfer mechanism (it can wrap, truncate or re-encode a long `TRUSTEDPATHS`). For the registry reads (A1-R1, A1-R2) the CAD manager writes the file from 64-bit PowerShell with an explicit encoding without BOM, for example `[System.IO.File]::WriteAllText(path, text + "\n", (New-Object System.Text.UTF8Encoding($false)))`. For the AutoCAD reads (A1-R3 to A1-R6) the CAD manager types one AutoLISP expression that writes the file itself with an explicit encoding, for example `(setq f (open "<path>" "w" "UTF-8")) (write-line <string-expression> f) (close f)` `[HOST-TO-CONFIRM: the encoding argument of open and the terminator of write-line]` (this typed file write is the Q-O-0(b) element: a new result file under the evidence folder). The AutoLISP default file encoding is not assumed to be UTF-8, which is why the encoding is explicit; a Spanish-locale path with accents is covered by the checks below. The offline helper (i) rejects invalid UTF-8, (ii) strips exactly one trailing line terminator (LF or CRLF) and nothing else, and (iii) compares the number of characters it decoded with the number that the CAD manager recorded from `(strlen <string-expression>)` typed in the same session `[HOST-TO-CONFIRM: that strlen counts characters and not bytes]`; a mismatch makes the attribute UNKNOWN. Whether a typed AutoLISP file write and the OS-level script stay inside the words "no instrument" of BA-11 row 1 is a ruling item (R-H(b)).

| Id | Attribute (BA-10 key) | The exact text that is read | Example read `[HOST-TO-CONFIRM]` | Why this text |
|---|---|---|---|---|
| A1-R1 | `MachineGuid` | the REG_SZ data of the value `MachineGuid` under `HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Cryptography`, **64-bit registry view**, exactly as stored | `reg query "HKLM\SOFTWARE\Microsoft\Cryptography" /v MachineGuid /reg:64`; or `Get-ItemPropertyValue` in 64-bit PowerShell | the Windows installation identity named by BA-10; the 64-bit view is fixed because a 32-bit process can be redirected |
| A1-R2 | `OsVersionBuild` | **Proposal (i):** the text of the **single read** of `Win32_OperatingSystem.Version` (for example the form `10.0.<build>`), exactly as returned, **no join and no `UBR`** | `(Get-CimInstance Win32_OperatingSystem).Version` in 64-bit PowerShell `[HOST-TO-CONFIRM]` | BA-10 (2) says "OS version and build", and the label hashes "the exact text that the observation read" (BA-10 V8 PARAM-05): a single read string is read text, whereas a joined four-part string would be a derived text. The Owner's AR10_03B_READING says that a software update is a new SESSION/BUILD TUPLE and does **not** automatically change the label: with this text a cumulative update (which changes `UBR`, not the build number) does not change the label, and `UBR`, `DisplayVersion` and `ProductName` are recorded in TB-B as supplementary build facts. **Alternative (ii)**, to be ruled **before the label is computed** (R-H(a)): the four decimal integers `CurrentMajorVersionNumber`, `CurrentMinorVersionNumber`, `CurrentBuildNumber`, `UBR` joined by dots (HKLM `SOFTWARE\Microsoft\Windows NT\CurrentVersion`, 64-bit view); it is a derived text and **every Windows cumulative update would change the label** |
| A1-R3 | `AutoCadProduct` | the **product registry root key** of the running AutoCAD, relative to the hive: starts with `Software\`, backslash separators, no hive prefix, no leading or trailing backslash; the repository observation of an earlier machine has the form `Software\Autodesk\AutoCAD\R25.0\ACAD-8101:409` (G-20) | `(vlax-product-key)` typed in the command line `[HOST-TO-CONFIRM]`; cross-check: the registry subkey of `HKLM\SOFTWARE\Autodesk\AutoCAD\R25.0` (64-bit view) that matches the `AcadLocation` of the running `acad.exe`. **Comparison rule of the cross-check (the only use of case folding; the extracted string itself is never folded):** the registry part after the hive and the `(vlax-product-key)` value are compared by an ordinal, case-insensitive comparison (the registry path is case-insensitive), with the hive prefix removed; a `(vlax-product-key)` value that does not begin with `Software\` (ordinal, ignoring case) makes the attribute UNKNOWN; a path that contains `WOW6432Node` makes it UNKNOWN (the 64-bit view is the one read); two reads that differ under this rule make the attribute UNKNOWN. *Optional cross-check 2* (`HostApplicationServices.Current.MachineRegistryProductRootKey`) needs an instrument and is therefore **outside ROW1**: this design does not require it | it identifies release, product and language variant and **does not change with a build update** (`AutoCAD version/build` is `BUILD_BOUND`, BA-10 section 2.2, so it must stay out of the class). `acad.exe` file version, `ACADVER` and `_VERNUM` are therefore recorded in the build tuple, not here |
| A1-R4 | `AutoCadProfile` | the exact text returned by the system variable `CPROFILE` (the current profile name) | `(getvar "CPROFILE")` | the name is what a user can read and what BA-10 calls the profile identity. **Weakness disclosed:** a profile edited in place keeps its name. Mitigation outside the label: the CAD manager records a **digest of the exported profile registry subtree** before and after every session as a `MACHINE_PROFILE_BOUND` fact (HF-M4b); a difference stops the gate |
| A1-R5 | `SECURELOAD` | the decimal digits of the integer returned by `SECURELOAD` | `(getvar "SECURELOAD")` | BA-10: "`SECURELOAD` as its decimal digits" |
| A1-R6 | `TRUSTEDPATHS` | the exact string returned by `TRUSTEDPATHS`, byte for byte, including its separators and any trailing `...` markers | `(getvar "TRUSTEDPATHS")` written to the raw file | BA-10: the exact text read. `TRUSTEDDOMAINS` is not in the closed list of six and is not read for the label |

**Ordering consequence (restates package V1 section 4, H1 note, now binding on the design).** `TRUSTEDPATHS` and `SECURELOAD` are class-defining, so the **trusted folder(s) for every
instrument and for every RackCad build declared for the campaign must be decided and set before the six strings are read**. Options for the Coordinator and the CAD manager (R-F):
(a) one recursive trusted root under which the instrument packages and each pinned RackCad build package are placed (a build change then needs no change of `TRUSTEDPATHS`; the weaker posture is compensated by hash pinning at load);
(b) one trusted entry per package declared in advance (every addition after H1 changes the label); (c) a different load mechanism that needs no `TRUSTEDPATHS` entry `[HOST-TO-CONFIRM]`. Lowering `SECURELOAD` to avoid the question is not proposed (it is also class-defining and weakens the host).
The Architect recommends (a).

**Offline test vector for the label** (SYNTHETIC; it is not a fact about any machine; computed with sorted keys, compact separators, UTF-8):

```text
input strings  : AutoCadProduct=SYNTHETIC-PRODUCT   AutoCadProfile=SYNTHETIC-PROFILE   MachineGuid=00000000-0000-0000-0000-000000000000
                 OsVersionBuild=0.0.0.0   SECURELOAD=1   TRUSTEDPATHS=C:\SYNTHETIC\modules\...
serialization  : {"AutoCadProduct":"SYNTHETIC-PRODUCT","AutoCadProfile":"SYNTHETIC-PROFILE","MachineGuid":"00000000-0000-0000-0000-000000000000","OsVersionBuild":"0.0.0.0","SECURELOAD":"1","TRUSTEDPATHS":"C:\\SYNTHETIC\\modules\\..."}
SHA-256        : 47229d23a125f9bc7bb947aa8d2eef9ee05f8e2484ea82c61c5f2ba31f593780
label          : MC-47229d23a125
same, TRUSTEDPATHS="" (empty string): MC-c7edfcd1078e
```

The helper must reproduce both labels; a real implementation must also pass the RFC 8785 vectors for the escaping of control characters and non-ASCII text.

### 3.3 MACHINE_CLASS and SESSION_BUILD_TUPLE recorded separately (Owner AR10-03(b))

| Record | Contents | Changes when | Digest |
|---|---|---|---|
| **MACHINE_CLASS** | the six strings (HF-M1..M6), the commands and raw outputs, the label | a class-defining attribute changes (BA-10 section 2.2 list of six) | the label |
| **SESSION_BUILD_TUPLE** — build part | AutoCAD version/build and `acad.exe` SHA-256 and API assemblies (BUILD_BOUND); the loaded-module / plugin baseline; every instrument DLL and the package manifest; the RackCad build declared for the run (source SHA, Plugin/UI DLL SHA-256, configuration); catalog folder and library file hashes | a module or plugin is added, removed or updated; a software update; a different build (Owner: "nueva SESSION/BUILD TUPLE" without changing the label) | `buildTupleDigest` (TB-B) |
| **SESSION_BUILD_TUPLE** — session part | `pid`, process start, document and database identity, private copy hashes before and after, run id and attempt, profile and variable reads before and after | every process | not a digest; fields of TB-S |
| **Facts of the CAD manager** (AR10-03(a)) | hardware identity/class, Windows account, configuration inputs: recorded as `MACHINE_PROFILE_BOUND` facts, **not class-defining**, compared through the manifest | any change is a stop and a new tuple record (no silent transfer) | included in the tuple record |

The Owner's R8 text (decisions section 237 point 3) names **a RackCad build** fixed for a run. The Coordinator's R-5 (decisions section 236, line 10016) says that the instruments are "finales y cargadas en la linea base de modulos antes de H1", and package V1 P-0.3 (line 212) says that an instrument loaded after the baseline is a plugin change. This design therefore requires that **the whole instrument set (the R0 and the RS DLLs, and the package manifest) is final, reviewed and listed in the tuple record before H1**, so that the evidence of every instrument sits on one `TB-B`. **The declared set for H1 does not contain I-7** (the fixture conformance reader): see R-N(b). Applying the R8 condition to instruments is the reading of R-5 and P-0.3 and an **extension of the Owner's R8 wording, stated as such** (ruling R-N); it is not the Owner's text. An instrument that is added later is a new `SESSION_BUILD_TUPLE` and its evidence is a new observation. A RackCad build declared for a run follows the same rule (declared before the run, hash fixed, the session starts under that identity, no silent substitution).

### 3.4 The matrix

Columns: **ID**, **Fact** (the exact text it represents), **Required by** (artifact and section), **In-gate basis**, **Read-only?**, **Observation method**, **Instrument**, **Raw output field**, **Consumer field**, **Tuple binding**, **Classification rule**. Instruments: see section 5 (`I-1` observation protocol, `I-2` read-only census, `I-3` side-database census, `I-4` context-variable reader, `I-5` write-back probe, `I-6` TOL_SCALE probe, `I-7` fixture conformance reader, `I-8` build and pin, `I-9` evidence seal). Raw output fields are JSON pointers into `ct21d.hostfact.v1` / the instrument's own record.

**Q-O-0 gating of the matrix (one explicit set).** The Q-O-0 gated set is {HF-G1 (M1), HF-C1, HF-C3, HF-T2 and the I-2 R0 build (`CT21DHG_CENSUS_R`, `CT21DHG_INVENTORY`) that carries them}; HF-G3 and I-4 are **not** in it. These four rows read through an in-memory side database filled by `ReadDwgFile` and therefore carry the R0 residual, Q-O-0(a). HF-G3 reads system variables of the active document only, constructs no database, is untouched by the R0 residual and is **not** gated by Q-O-0(a). Q-O-0(b) (new result files through the single `EvidenceWriter`) is common to every instrument that writes a result file and does not change the set; see section 6.

**Group M — machine and session facts**

| ID | Fact | Required by | In-gate basis | Read-only? | Observation method | Instrument | Raw output field | Consumer field | Tuple binding | Classification rule |
|---|---|---|---|---|---|---|---|---|---|---|
| HF-M1 | `MachineGuid` text (A1-R1) | BA-10 V8 2.2 `PARAM-05` (the label is "written into these bytes", header prerequisite 1) | ROW1 | yes (strict) | registry value read, 64-bit view, CAD manager | I-1 (no instrument) | `observation.machineGuid.text`, `.command`, `.rawOutputSha256` | BA-10 label input; local evidence (clear GUID stays local, BA-10 label row) | TB-C, TB-S | OBSERVED / UNKNOWN (missing or invalid UTF-8) |
| HF-M2 | `OsVersionBuild` text (A1-R2) | BA-10 V8 2.2 | ROW1 | yes (strict) | single read of `Win32_OperatingSystem.Version` (A1-R2, proposal (i)) | I-1 | `observation.osVersionBuild.text`, `.supplementary.ubr` | BA-10 label input | TB-C, TB-S | as HF-M1 |
| HF-M3 | `AutoCadProduct` text (A1-R3) | BA-10 V8 2.2 | ROW1 | yes (strict) | `(vlax-product-key)` and the registry cross-check under the comparison rule of A1-R3 `[HOST-TO-CONFIRM]` | I-1 (no instrument) | `observation.autoCadProduct.text`, `.crossChecks[]` | BA-10 label input | TB-C, TB-S | OBSERVED when all reads agree after the mapping rule; any disagreement is UNKNOWN |
| HF-M4 | `AutoCadProfile` text (A1-R4) | BA-10 V8 2.2 | ROW1 | yes (strict) | `(getvar "CPROFILE")` | I-1 | `observation.autoCadProfile.text` | BA-10 label input | TB-C, TB-S | as HF-M1 |
| HF-M4b | digest of the exported profile registry subtree, before and after each session (supplementary, not in the label) | tuple record: Owner Q-O5 "AutoCAD profile identity" and "configuration inputs"; stop condition 2 of package V1 4.0 | Owner gate 233 item "machine/session fact capture" (package row G2); **NEEDS-COORDINATOR**: whether a supplementary registry-subtree digest is inside the G2 phrase "configuration inputs" (cited above from Owner Q-O5) is not established by the text and is not decided here | yes (strict) | registry export of the profile key and hash, CAD manager | I-1 / I-9 | `observation.profileDigest.before/after` | tuple record, stop condition | TB-C, TB-S | OBSERVED; a before/after difference is INVALID for the session |
| HF-M5 | `SECURELOAD` decimal digits (A1-R5) | BA-10 V8 2.2 | ROW1 | yes (strict) | `(getvar "SECURELOAD")` | I-1 | `observation.secureLoad.text` | BA-10 label input | TB-C, TB-S | as HF-M1; before/after read must be equal |
| HF-M6 | `TRUSTEDPATHS` exact string (A1-R6) | BA-10 V8 2.2 | ROW1 | yes (strict) | `(getvar "TRUSTEDPATHS")` | I-1 | `observation.trustedPaths.text` | BA-10 label input | TB-C, TB-S | as HF-M1; empty string is a valid OBSERVED value |
| HF-M7 | the `MC-` label (derived, not observed) | BA-10 V8 2.2 label row | ROW1 (the computation) | n/a (offline) | recomputed from the six raw files by the offline helper | I-0 `core` | `label`, `serializationSha256` | BA-10 V9 `PARAM-05` value; V2.1 2.1 "machine identity class" | TB-C | computed only when all six are OBSERVED; otherwise `UNSET`, never partial |
| HF-M8 | AutoCAD version/build, `acad.exe` SHA-256, API assembly versions and SHA-256 | V2.1 2.1 BUILD_BOUND; Owner Q-O5 "AutoCAD version/build" | Owner gate 233 "machine/session fact capture" (package row G2) | yes (strict) | file hashes and version resources; host identity read | I-1 (script) | `observation.build.*` | build tuple (TB-B) | TB-B | OBSERVED / UNKNOWN |
| HF-M9 | module baseline and the declared set: process module list at session start (before any `NETLOAD`), after each `NETLOAD` and at the end; the instrument DLLs (R0 and RS), the RackCad build identity declared for the run | V2.1 2.1; Owner Q-O5; R8 precision; R-5 and package V1 P-0.3 | package row G2 | yes (strict) | process module list and file hashes | I-1 / I-8 | `observation.modules[]`, `.hostLoaded[]`, `.loadOutcomes[]` (loaded / prompt / refused for each `NETLOAD`) | build tuple (TB-B) | TB-B, TB-S | OBSERVED. A module that is outside the declared set is INVALID, **except** a module loaded by the host itself (native or ObjectDBX modules that AutoCAD can demand-load when `ReadDwgFile` meets proxy or custom objects) that resides under the AutoCAD installation directory and is signed by the vendor `[HOST-TO-CONFIRM]`: it is listed in `observation.hostLoaded[]` and is not INVALID |
| HF-M10 | hardware identity/class, Windows account, configuration inputs (catalog folder and file hashes, library path/SHA) | Owner Q-O5; AR10-03(a) | package row G2 | yes (strict) | CAD manager commands; file hashes | I-1 | `observation.profileBound.*` | tuple record (not class) | TB-B, TB-L | OBSERVED / UNKNOWN |
| HF-M11 | session identity: pid, start time, document and database identity, private-file hashes before/after | V2.1 2.1 SESSION_BOUND | package row G2 | yes (strict) | process query; hashes | I-1 / I-9, and each instrument at start | `tuple.TB-S` | every record | TB-S, TB-L | OBSERVED; drift is INVALID |

**Group C — library census (K-2) with the AR4-10 confirmations**

| ID | Fact | Required by | In-gate basis | Read-only? | Observation method | Instrument | Raw output field | Consumer field | Tuple binding | Classification rule |
|---|---|---|---|---|---|---|---|---|---|---|
| HF-C1 | every block table record of the library file (named, anonymous, layout): exact host class (`RXClass` name) and DXF name of every entity with counts, nested block names, symbol records referenced, proxy/custom presence, whether a reference's definition is anonymous and not a dynamic representation, `isLayout` / `isAnonymous` / `isDynamic` | BA-05 V5 section 5 steps 1 to 4, schema `blocks[]`, `classUniverse[]`, `classesWithoutTable[]` (V5:1177-1192); BA-08 V7 K-2, section 14 | ROW3 | **STRICT READ-ONLY on disk** (no API write call, no save): opens the private copy in a side database for read; **Q-O-0(a) gated** (same R0 residual flag as HF-G1) | `Database.ReadDwgFile` of the private copy into a side database; read-only traversal | I-2 | `blocks[].entityClasses[]`, `classUniverse[]`, `classesWithoutTable[]` | BA-05 class map and tables (new version); BA-08 closure and fingerprint type list | TB-B, TB-S, TB-L, TB-R | OBSERVED; an unreadable record or an entity class the instrument cannot name is UNKNOWN for that record; an incomplete census is INVALID as "every block" (BA-05 V5 3) |
| HF-C2 | per dynamic block definition: the dynamic properties a reference exposes — name, read-only flag, host value type, units type (distance / angular / area / none), the value set the host reports, whether two properties share a name | BA-05 V5 section 5 step 2 and schema `dynamicProperties[]` (V5:1180, 1190); BA-08 V7 section 11 rule 2 (V7:405) | ROW3 | **WRITES-SIDE (not STRICT)**: BA-05 itself requires "a reference created in a separate scratch database of the census (never in the library copy)" (G-16) | clone the definition into a scratch side database, insert a reference there, read `DynamicBlockReferencePropertyCollection` | I-3 | `blocks[].dynamicProperties[]` | BA-08 expected dynamic-property set and value sets; BA-05 DYNPROP typing | TB-B, TB-S, TB-L, TB-R | OBSERVED; a property whose value set cannot be read is recorded as constrained-unknown (BA-08 V7:405); **NEEDS-OWNER (Q-O-1)**: blocked until the Owner answers |
| HF-C3 | `DSTYLE` storage of per-entity dimension overrides of the library's own dimensions: storage location (the `ACAD` extended data), the section delimiters, the `(code, host value type)` pairs **in host order** (a code and a type, **no variable name**: see HF-C5) | BA-05 V5 2.3.2 and section 5 step 5 (a) (V5:847, 1183); BA-11 3.3 row 3 parenthesis | ROW3 only (BA-05 V5 section 5 step 5 (a), V5:1183, itself requires the `(code, host value type)` pairs in host order in the census; no G6 basis) | **STRICT READ-ONLY on disk** (no API write call, no save); **Q-O-0(a) gated** (same flag as HF-G1) | read the extended data of every `AcDbRotatedDimension` / `AcDbAlignedDimension` in the side database copy | I-2 | `blocks[].dimensionOverrides[]` (`hasDstyleSection`, `pairs[]` of `code`, `hostValueType`) | BA-05 `overrides` and 3.1.2; BA-08 oracle (V7 11, rule 1 overrides) | TB-B, TB-S, TB-L, TB-R | OBSERVED / OBSERVED_DIFFERS (a location other than `DSTYLE` of `ACAD`); the library may hold no dimension with overrides: then NOT_OBSERVED for the case, and HF-C4 is the only source |
| HF-C4 | **equal-to-style behaviour**: whether the host stores or drops an override whose value equals the style value; storage and order for each override the product writes (`DIMSCALE`, `DIMASZ`, `DIMEXO`, `DIMEXE`, `DIMTAD`, `DIMTXT`, `DIMGAP`, `DIMDEC`, BA-05 V5:1115), once different from the style and once equal | BA-05 V5 5 step 5 (a), L-4 (V5:1183, 1201); BA-11 3.3 row 3 parenthesis "equal-to-style behaviour" | ROW3 by the Coordinator's R-3 (section 236); **not G6** | **NO: it writes** — to a scratch side database only (never to the library copy) | create a `RotatedDimension`, set each override by the property setters the product uses, read the extended data back | I-5 | `probe[].variable`, `.written`, `.storedPairs[]` in host order, `.storedWhenEqualToStyle` | BA-05 `overrides` and the oracle's predicted stored set | TB-B, TB-S, TB-R | OBSERVED / OBSERVED_DIFFERS; **NEEDS-OWNER (Q-O-1)**: R-3 admitted the write-back as a host activity on a work database, not a non-read-only instrument under the Owner's instrument gate; blocked until answered |
| HF-C5 | **group-code designation** of the dimension variables: the assignment code -> variable name (for example code 40 = `DIMSCALE`), for the variables the product writes | BA-05 V5 3.1.2 last paragraph (V5:1115) and section 5 step 5 (a) (V5:1183: "together with the group-code designation of section 3.1.2") | **ROW3** (BA-05 step 5 (a) names it in the same census and qualification); **not G6** (a code assignment is not a host type) | **no read-only source exists: NOT_OBSERVABLE.** The one observation that yields the assignment writes (HF-C4) | The HF-C3 ROW3 pairs are `(code, host value type)` in host order: a code and a type, **no variable name**, so the assignment code -> variable **cannot be derived from them** (an earlier revision said it could; corrected, Review record R2-MAJOR-2). The assignment is read only from the stored pairs of the HF-C4 write probe: set a known variable by its setter, read the stored code back | I-5 only (the HF-C4 write probe); **I-2 yields no designation** | `designation[].variable`, `.code`, `.source` (must be `HF-C4`; a record with another source is rejected) | BA-05 `OVERRIDE_MAP_EQUAL` | TB-B, TB-S, TB-R | `NOT_OBSERVABLE` while HF-C4 has not run; after HF-C4: OBSERVED / OBSERVED_DIFFERS against the codes BA-05 states (V5:1115: 40, 41, 42, 44, 77, 140, 147, 271, "to be confirmed by the same step"). A second read-only source (compare each override value of the HF-C3 pairs with the dimension's own value of each variable) is **not adopted**: it is an inference, not an observation; it fails when two variables hold the same value, when an override equals the style value (the override may be absent, which is what HF-C4 asks) and when a type is not distinctive; and no matrix row or instrument reads those dimension values. It goes to the Coordinator as R-D(b); if admitted it could only be `CORROBORATING_ONLY` (never OBSERVED, never a confirmation of the designation, UNKNOWN on any ambiguity) and would need its own ROW3 basis and instrument field. **If Q-O-1 is STRICT or HF-C4 does not run, the designation stays unconfirmed: the designation part of BA-05 L-4 (a) cannot close, and BA-05 must be re-prepared without it or carry the codes as an unconfirmed statement; that is a BA-05 decision, outside this document** |

**Group G — G6: host-type facts expressly required by the current BA-05 text**

| ID | Fact | Required by | In-gate basis | Read-only? | Observation method | Instrument | Raw output field | Consumer field | Tuple binding | Classification rule |
|---|---|---|---|---|---|---|---|---|---|---|
| HF-G1 | host value type of each `RB` code range of the table (32 ranges: the rows of the table at V5:72-105) as the host **returns** it when it reads **stored** data — M1 (read of existing stored result buffers: Xrecords in the dictionaries and extended data of entities of the private library copy; fixtures are not a source: they are built under ROW4 and, apart from `FX-ANN-BLANK` and `FX-IMP`, are OUT for now (R-4), so a G6 read of a fixture would need its own gate) | BA-05 V5 2.1.2 and section 5 step 5 (b); L-4 (V5:68-109, 1183, 1201) | **G6** (BA-05 V5:107 "The host types of the ranges are confirmed on the exact build ... before the candidate") | **R0 (STRICT as defined in section 0); the R0 residual is Q-O-0(a): this read is in the Q-O-0 gated set and proceeds only on the Coordinator's or the Owner's confirmation** | traverse stored `ResultBuffer`s read-only; tally `(code, runtime type)` | I-2 | `rbTypes[].range`, `.codesSeen[]`, `.hostTypes[]`, `.count` | BA-05 table 2.1.2 (confirm or correct, new version) | TB-B, TB-S, TB-L, TB-R | per range: OBSERVED (type equals the table's), OBSERVED_DIFFERS, or **NOT_OBSERVED (no occurrence)**; a range that is NOT_OBSERVED is **not confirmed**. Honest limit: existing data cannot be assumed to cover all 32 ranges |
| HF-G2 | the same host types for the ranges that M1 cannot reach — M2 (write one value per range into an `Xrecord` of a scratch database and read it back) | BA-05 V5 section 5 step 5 (b) (the same sentence) | **NEEDS-OWNER**: it writes, so it is **not** READ-ONLY as G6 is worded; BA-11 row 3 does not name it; the Coordinator's R-3 admitted only the dimension write-back as "equal-to-style". The Architect does **not** stretch G6 to cover it | **no** (scratch write) | would be the scratch round trip | I-5 (not to be implemented unless ruled) | `rbTypes[].roundTrip[]` | BA-05 table 2.1.2 | TB-B, TB-S, TB-R | not run. **Fallback F-RB that needs no write:** BA-05's next version keeps in table 2.1.2 only the ranges that M1 confirmed; every other code falls under the existing rule "every code outside the table is UNKNOWN" (V5:107), a fail-closed availability cost disclosed in the artifact |
| HF-G3 | host type of the value returned by `Application.GetSystemVariable(name)` for `CLAYER`, `CECOLOR`, `CELTYPE`, `CELTSCALE`, `CELWEIGHT`, `CETRANSPARENCY`, `CPLOTSTYLE`, `TEXTSTYLE`, `DIMSTYLE`, `PSTYLEMODE` (expectation table V5:127-138) | BA-05 V5 2.1.3 ("The read of a context variable (normative)") and section 5 step 5 (c) (V5:125-140, 1183) | **G6** | **R0 (STRICT as defined in section 0; no side database, system-variable reads of the active document only); NOT gated by Q-O-0(a): the R0 residual does not arise without a side database (its result file is the Q-O-0(b) element common to all instruments)** | in the interactive session, a **non-Session command** (so the document lock is held by the host) reads each name; records the runtime type full name and the value text | I-4 | `ctxVars[].name`, `.hostType`, `.valueText`, `.expectedType`, `.matches` | BA-05 2.1.3 table; BA-08 `HOST_DEFAULT_MAP` keys | TB-B, TB-S (**document identity: the values are document state**), TB-R | per variable: OBSERVED (type matches the expectation) / OBSERVED_DIFFERS / UNKNOWN; the **type** is the fact; the value is context. A variable the kind baseline lists and the table does not is recorded the same way (V5:140) |
| HF-G4 | **any other** host-type fact expressly required by the current BA-05 text | the Owner's third bullet of G6 | G6 | yes | the scan below | none needed | `scan.candidates[]` (the table in section 3.5) | the Coordinator's confirmation | TB-R | the Architect's scan found **no additional G6 fact** beyond HF-G1 (M1) and HF-G3; HF-C3 and HF-C5 were examined and are ROW3, not G6 (HF-C5: NOT_OBSERVABLE read-only); the rows excluded are listed in 3.5 with the reason |

**Group T — TOL_SCALE (K-3)**

| ID | Fact | Required by | In-gate basis | Read-only? | Observation method | Instrument | Raw output field | Consumer field | Tuple binding | Classification rule |
|---|---|---|---|---|---|---|---|---|---|---|
| HF-T1 | the host-stored scale factors read back for assigned scale triples (P-A: 147 rows, 135 in scope, OP1 and OP2) | BA-11 3.3 row 2; BA-08 V7 section 10 and K-3; BA-04 V7 declarations of S-1..S-3 | **ROW2** (not G6); the Owner's gate-233 list "TOL_SCALE non-governing characterization" | **no** (WRITES-SIDE; OP2 saves a DWG file); **NEEDS-OWNER (Q-O-1)** | section 2.4 | I-6 | `tolscale-raw.json` (`rows[]`, `summary`) | the Architect's decision (BA-08 next version §10) | TB-B, TB-S, TB-R | section 2.5 (PASS / FAIL / UNKNOWN / INVALID) |
| HF-T2 | scale components of the existing `AcDbBlockReference`s of the private library copy (no block extents: the field is struck; `E_max` comes from the sealed plan parameters) | BA-11 3.3 row 2 (informative evidence for `I_max`) | ROW2 | **STRICT READ-ONLY on disk** (no API write call, no save); **Q-O-0(a) gated** (same flag as HF-G1) | traversal of the side database copy in a **separate command** | I-2 (`CT21DHG_INVENTORY`, own record `inventory-raw.json`) | `inventory.*` | the Architect's decision (informative) | TB-B, TB-S, TB-L, TB-R | OBSERVED / UNKNOWN per reference |

**Group F — fixtures (K-8; BA-11 3.3 row 4)**

| ID | Fact | Required by | In-gate basis | Read-only? | Observation method | Instrument | Raw output field | Consumer field | Tuple binding | Classification rule |
|---|---|---|---|---|---|---|---|---|---|---|
| HF-F1 | the `FX-ANN-BLANK` instance built in the pinned two-step sequence (step 1 on a clean build of `69daf03a35c630e453e1d9e98136f128bd0325a4`, tree `3c34dc9be113e98088acf56653aa5181437d0893`; step 2 on the exact build) and its conformance record (BA-08 V7 12.6 items 1 to 6) | BA-08 V7 12.1, 12.6; BA-04 V7 pin `FIXTURE_INSTANCE@FX-ANN-BLANK` | ROW4; Coordinator R-4 (section 236): **IN** | **no** (the product's commands draw a new drawing); not a G6 fact | manual operation of the product commands by the Owner / CAD manager (package V1 H5); conformance read by the instrument | I-7 (reader; **not in the H1 declared set**, R-N(b): runs in a later session under its own declared tuple) + I-8 (builds) | `conformance.items[1..6]` | BA-08 12.6; BA-04 pin | TB-B (both builds), TB-S, TB-L | non-conformance is recorded as non-conformance with the raw evidence, not repaired; the build identity is a tuple fact (R8 precision) |
| HF-F2 | construction of `FX-IMP` and its four variants (`SPECIFICATION_OPEN`): reachable with product operations or declared artificial with its reason | BA-08 V7 12.5, K-8; BA-04 V7 `BLOCKED_BY` K-8 (45 scenarios) | ROW4; R-4: **IN** | **no** | package V1 H7; needs the written construction specification (T-6), not an instrument | I-7 (**not in the H1 declared set**, R-N(b)) | `conformance.*`, `construction.specification` | BA-08 12.5; BA-04 | as HF-F1 | "not constructible with product operations" is a valid recorded result |
| HF-F3 | instances of `FX-1F`, `FX-2F`, `FX-4F`, `FX-ANN`, `FX-DIM`, `FX-BIG`, `FX-SCRATCH` and the variants | BA-08 V7 12.3, 16.2 K-1 | **OUT for now** (R-4, section 236: "las instancias de los demás fixtures esperan a la build exacta") | n/a | none | none | none | none | none | not observed; not a baseline blocker (BA-08 16.2) |

### 3.5 Scan for "any other host-type fact" (basis of HF-G4) and the boundary test

The Architect read BA-05 V5 for every sentence that depends on what the host returns or stores. Result:

| Candidate in BA-05 V5 | Expressly requires host confirmation? | Classification |
|---|---|---|
| RB range host types (2.1.2) | yes, 5 step 5 (b), L-4 | HF-G1 (M1 in G6; M2 NEEDS-OWNER) |
| context-variable host types (2.1.3) | yes, 5 step 5 (c), L-4 | HF-G3 (G6) |
| stored override storage / order / equal-to-style (2.3.2) | yes, 5 step 5 (a), L-4 | HF-C3 (read), HF-C4 (write, R-3) |
| group-code designation of dimension variables (3.1.2) | yes, "which the census and the I-12 qualification confirm" (V5:1115) | HF-C5 (not G6; ROW3 by BA-05 step 5 (a); **NOT_OBSERVABLE from read-only sources** because the HF-C3 pairs carry no variable name; observed only by the HF-C4 write probe; R-D) |
| dynamic-property name, read-only flag, host type, units type, value set (2.3.2, 5 step 2) | yes, census | HF-C2 (ROW3) |
| exact `RXClass` names and DXF names (2.3.1, 5 step 2) | yes, census | HF-C1 (ROW3) |
| stored enum names (`LineWeight`, `ColorMethod`, `TransparencyMethod`, `UnitsValue`, `AnnotativeStates`, `BlockScaling`, text modes ...) | **no**: the tables state the rule "stored name of the host value" and no confirmation sentence | **OUT** — not a G6 fact; no row |
| host iteration order of entity containers (2.2) | no: a definition, not a confirmation | OUT |
| `Point3d` as the type of point codes | inside HF-G1 | HF-G1 |

**Boundary test an auditor applies (G6 scope creep guard).** A proposed host observation is inside the G6 extension only if **all five** answers are yes:
1. Is it a **host-type fact**: a host value type, or the range of types or codes that the host accepts or returns (the Owner's examples are "tipos/rangos RB" and "variables de contexto")? A code-to-variable designation, a storage location, an ordering or a behaviour is **not** a host-type fact, and neither is "what the host returns or stores" in general.
2. Does a **sentence of the current BA-05** (the version in the registry at the time of the run) **expressly require** it? The record must cite the section and the line, for example "BA-05 V5:107" or "V5:1183".
3. Is it **STRICT READ-ONLY as defined in section 0**: no API write call, no save, no write to or write-open of a product or open-document database, and, if it needs a database at all, only an in-memory side database that `ReadDwgFile` fills from a private copy (the R0 residual, **Q-O-0**)? If it creates objects or writes values in a side database it is not G6; classify it by the original BA-11 3.3 text and flag it (HF-G2, HF-C4). A "yes" to this question is conditional on Q-O-0 for every observation that uses the side database.
4. Is it **non-governing**, bound to the exact tuple (TB-C, TB-B, TB-S, TB-R) and confined to the facts concrete to BA-05?
5. Is it **absent from the Owner's "NO autoriza" lists** (governing CT-21D, product admission, product writes, scope beyond the concrete facts)?

A "no" to any question means the observation is outside G6 and goes to the Coordinator, not to the instrument list. Applied to this document: HF-G1 (M1) and HF-G3 answer yes to questions 1, 2, 4 and 5 on the text of BA-05 V5 (V5:107; V5:125-140; V5:1183). **Question 3 is not asserted by the Architect as a fact.** For HF-G1 (M1) it is yes only under the STRICT definition of section 0, whose R0 residual (the read goes through a `ReadDwgFile`'d side database that the host may evaluate or cache in memory) is **Q-O-0(a)**: HF-G1 (M1) is in the Q-O-0 gated set (with HF-C1, HF-C3, HF-T2 and the I-2 R0 build that carries them) and proceeds only on the Coordinator's or the Owner's confirmation. HF-G3 uses no side database, only system-variable reads of the active document, so the R0 residual does not arise and HF-G3 is **not** gated by Q-O-0(a). For both, the writing of the result file through the single `EvidenceWriter` is the element Q-O-0(b), common to every instrument. The Architect therefore does **not** state that HF-G1 and HF-G3 "pass all five". HF-C3 fails question 1 (ROW3 by BA-11 row 3 and BA-05 step 5 (a)); HF-C5 fails question 1 (a code assignment is not a host type) and is in addition NOT_OBSERVABLE from read-only sources (3.4); HF-G2 and the HF-C4 write probe fail question 3 (they write to a side database).

## 4. Part C — G6 implication review

### 4.1 What G6 changes in the host package

| Item | Before (package V1 and the Coordinator's section 236) | After G6 |
|---|---|---|
| Package row G6 (RB ranges and context variables) | RULING; Coordinator (section 236, R-3): OUT until an explicit extension | **IN** by the Owner's G6, for the read-only part only: HF-G1 (M1) and HF-G3 only; HF-G1 (M1) **only on the confirmation of Q-O-0(a)**, HF-G3 **not gated by Q-O-0** (HF-C3 is ROW3 and in the Q-O-0 gated set; HF-C5 is ROW3 and not observable read-only) |
| Activity H3 | one activity "host-type and write-back confirmations" (G5 + G6) | **split**: H3a = the write-back probe (HF-C4, which also yields the HF-C5 designation; scratch side database; ROW3 by R-3 as a host activity, blocked as an instrument until Q-O-1); H3b = the G6 read-only facts (HF-G1 M1, HF-G3) |
| Tooling | T-3 one probe | T-3 splits into `I-5` (write-back, scratch), `I-4` (context variables, strict read-only) and the RB-type read inside `I-2` |
| Tuple binding | machine / session tuple | each G6 record also carries the **active document identity** (the value of a context variable is document state), the exact BA-05 blob (TB-R) and the instrument SHA-256 (TB-B) |
| BA-05 candidate path | blocked by L-4 | L-4 (b) closes only for the ranges M1 observes; for the rest either the Owner extends G6 to a scratch round trip (HF-G2) or BA-05 takes fallback F-RB |
| BA-11 3.3 | plan rows 1 to 4 | the registry's plan table should gain a row for the G6 extension with the Owner's wording (a Coordinator inscription; the registry itself authorizes nothing) |
| Package v2 (AR10-07 and section 237) | not written | must carry: the three minors of AR10-07 ("INVALID o caida" in the last row of 4.0; H4, H5 step 2 and H6 as "IN, condicional" in section 8; a sentence on `I52G3AAutoCadProbe` as a reference probe only, not a census, and **not read-only** because of `AddOverrule`, G-20); the H3 split; the R8 condition; the two identities (MACHINE_CLASS, SESSION_BUILD_TUPLE); this matrix as its section 2 |

**Q-O-0 gated set in this review (one explicit set).** The Q-O-0 gated set is {HF-G1 (M1), HF-C1, HF-C3, HF-T2 and the I-2 R0 build (`CT21DHG_CENSUS_R`, `CT21DHG_INVENTORY`) that carries them}; HF-G3 and I-4 are **not** in it. Until Q-O-0 is answered the gated set does not run and the I-2 R0 build is not started; I-0, I-1, I-4 and I-9 are outside the gated set (but see the sequencing rule of R-G: no host activity starts and no tuple record is closed until the whole declared set is final).

### 4.2 What G6 does not authorize

Governing CT-21D; product admission; any product write (the Owner: "NO autoriza escrituras de producto"); any observation outside the facts that the current BA-05 needs; anything written to a side database (HF-G2, HF-C4 are classified by their own basis, not by G6); the TOL_SCALE test (a row-2 item); fixture work; Core Console as a substitute for the interactive session unless the Coordinator rules it; carrying evidence to another build or machine (R8 and AR10-03(b)). The `DSTYLE` storage and the group-code designation (HF-C3, HF-C5) are ROW3, not G6.

### 4.3 Scope-creep risks

| Risk | Control |
|---|---|
| The phrase "cualquier otro host-type fact" is read as an open category | the boundary test of 3.5 with a citation of a BA-05 sentence; the scan of 3.5 closes the list for BA-05 V5; a BA-05 V6 reopens the question |
| M2 (the round trip for unobserved ranges) is added because it is "just a read-back" | it writes; HF-G2 is NEEDS-OWNER and `I-5` does not implement it unless ruled |
| The census instrument grows extra sections (extents, proxy detail, anything "interesting") | every output field maps to a matrix row; a field without a row is rejected in review (section 5.3) |
| Reading context variables in a document of convenience | the document identity is a tuple field; the document is declared in the tuple; a different document is a new observation |
| Re-using the G3A probe, which writes (`AddOverrule`, a JSON file) | it is a reference of API names only; it is not loaded |
| Evidence transferred to another build because "the type cannot differ" | R8 and AR10-03(b): another build is a new tuple; no automatic transfer |

## 5. Part D — Instrument specifications (replacing T-1 to T-9)

Nothing here is implemented or authorized. The implementation gate opens only after the Coordinator's PASS on this design (Owner record 237 point 5), and then only for the instruments the Coordinator lists in R-G.

### 5.1 Common rules

**Terms (descriptive; the Owner's word is not redefined here).** *R0 (STRICT READ-ONLY, as defined in section 0)*: no API write call of any kind in the instrument's code, no save, no write to or write-open of a product or open-document database; its only database construction is `new Database(false, true)` followed by `ReadDwgFile` of a private copy into an in-memory side database (never saved, never attached to a document); **it writes only new result files under the evidence folder through the single `EvidenceWriter` (the element Q-O-0(b))**; an instrument that only reads system variables (I-4) constructs no database.
*RS (WRITES-SIDE)*: writes only to side databases created in memory by the instrument (never attached to a document) and saves only new files under the evidence folder; it writes nothing to a product database, product file, catalog file, library file, template, registry setting or system variable, and uses no `Overrule`, no command, no document open and no UI change.
The two classes are built as **two separate DLLs** so that a reviewer can verify R0 without reading RS. **Which class the Owner's "solo lectura" admits is Q-O-1; until it is answered the RS DLL is not implemented. Whether the R0 residual is inside "solo lectura" is Q-O-0(a), and whether new result files through the single `EvidenceWriter` are is Q-O-0(b); the Q-O-0 gated set is {HF-G1 (M1), HF-C1, HF-C3, HF-T2 and the I-2 R0 build (`CT21DHG_CENSUS_R`, `CT21DHG_INVENTORY`) that carries them}, and HF-G3 and I-4 are not in it; the gated set proceeds only on the confirmation of Q-O-0.** The R0 claim is "no API write call and no save", **not** "nothing is written in memory" (5.3 item 4). Instruments write their results to files only; the command line shows at most a one-line status. Package V1 (line 464) calls the instrument "console-less"; if that means no command-line echo at all, the status line is struck (R-I).

**Location (proposal; the Coordinator may rename).** `eng/research/I52Ct21dHostFacts/`, with the same isolation as `eng/research/I52Ctda` (README: "Nothing in this directory is referenced by a product project, by `RackCad.sln` or by `deploy/`"):

| Subfolder | Contents |
|---|---|
| `core/` | pure .NET library, **no AutoCAD reference**: label computation, probe tables, statistics (`D_A`, `K_pres`, `I_max`), RB expectation table, context-variable expectation table, classification rules, record builders and schema validation, canonical JSON (RFC 8785) |
| `r0/` | the R0 plugin DLL (net8.0-windows, x64): commands for I-2 and I-4 |
| `rs/` | the RS plugin DLL: commands for I-3, I-5, I-6 |
| `conformance/` | I-7 (loads the product's Application-layer reader; section 5.2); **outside the H1 declared set (R-N(b))** |
| `tools/` | offline: `forbidden-api-scan`, `imports-allowlist-check`, `seal-evidence` (I-9), `label-helper` |
| `tests/` | tests that need no AutoCAD |
| `schemas/` | JSON schemas of the records |
| `README.md` | research-only statement and the read-only definition |

**Host and language (decision, TS-D-11).** The census, the type readers and the probes are **managed .NET plugin DLLs loaded by `NETLOAD`** in the interactive `acad.exe`. Reason: BA-05 speaks of **.NET host types** (`System.Int16`, `Int32`, `Int64`, `Double`, `String`, `Point3d`, `byte[]`) and of `RXClass` names; AutoLISP returns LISP types and cannot show which .NET type the managed API returns, and a Core Console script runs the same managed code but its observations are hypotheses for the CT-21D tuple (V2.1 2.3). The machine-attribute observation is **not** an instrument (BA-11 row 1): typed commands and a documented protocol, plus an offline helper.

**How an instrument enters the baseline and the tuple.** The instrument DLLs, the package manifest and the pins are listed in the tuple record **before H1** (TB-B; V2.1 2.1: "any payload / observer / harness DLL SHA-256"), **all of them**, R0 and RS, final and reviewed (I-7 is not in this set: R-N(b)) (R-5 of section 236; package V1 P-0.3; section 3.3; R-N). They are loaded by `NETLOAD` from a folder that `TRUSTEDPATHS` covers **before the six strings are read** (A1-R6 and the options (a), (b), (c)); loading a declared module changes the `SESSION_BUILD_TUPLE`, not the label (AR10-03(b)), unless the trusted path list changes. Under `SECURELOAD` the load may prompt or be refused `[HOST-TO-CONFIRM]`; the outcome is recorded; a prompt answered by hand is recorded as manual input and makes the TOL_SCALE test INVALID (S1) and, for any other instrument, the activity that needed the load.

**Pinning and determinism.** The build is a canonical script modeled on `eng/research/I52Ctda/build-r3.ps1` (which publishes to a parameterized `OutputRoot`, line 66, and does not set `SourceRevisionId`): from an exact committed SHA, into a versioned package **outside** the repository, with `package-manifest.json` holding the SHA-256 of every file. The instrument projects **inherit** the root `Directory.Build.props` (`Deterministic` true, `Nullable` disable, `LangVersion` latest) and the root `Directory.Build.targets`, which stamps `SourceRevisionId` from `git rev-parse HEAD` into the `InformationalVersion`, so the DLL bytes depend on the repository HEAD: the build script **pins `SourceRevisionId` explicitly** to the committed SHA named in the manifest (or builds outside the repository tree). The compile references to `AcDbMgd`, `AcMgd` and `AcCoreMgd` come from the AutoCAD installation (`AutoCADInstallDir`, as the G3A probe project does), so their versions must equal the designated machine's build, which is recorded in TB-B. Precedents: `eng/research/I52Ctda` exists on the feature-branch HEAD but **not on `main`** (no file of it is in `main`), and its `managed-observer` project (`ObserverApplication.cs`) is the managed `NETLOAD` precedent.
**Self-hash pinning** at command start compares the instrument's own file on disk with a sidecar pin in the same trusted folder. It protects against an accidental mismatch only: it does not protect against the substitution of the DLL together with its sidecar, and it checks the file, not the loaded image. The independent pins are the pre-H1 tuple record (the SHA-256 of each DLL) and the module-list hash, checked from outside the process (I-1 / I-8). Whether `Assembly.Location` is a real file path under `NETLOAD` in AutoCAD 2025 is `[HOST-TO-CONFIRM]`. Outputs are deterministic functions of the inputs: canonical JSON with sorted keys, arrays in host iteration order with an explicit index, no random values, no timestamps inside the hashed part (`volatile` is separate and excluded from `contentSha256`); the OP2 side-database DWG is not byte-deterministic and is hashed only as a raw file.

### 5.2 The instruments

| Id | Replaces | Purpose and facts | Class / host / language | Inputs and outputs | Read-only property and how a reviewer verifies it |
|---|---|---|---|---|---|
| **I-0** `core` | part of T-1, T-4 | label computation; statistics; tables; classification; schemas; canonical JSON; the `PF-1`/`PF-2` probe table generator | pure library, offline | in: strings, rows; out: label, statistics, validated records | no host access at all; unit tests |
| **I-1** observation protocol | T-1 | HF-M1..M11: the six strings, the machine layer, the session layer | **none loaded** (BA-11 row 1 "no instrument"): a written protocol of commands for the CAD manager (registry reads, `getvar` calls typed at the command line), an offline **label helper** (I-0) and an OS-level script for hashes, process list and module list (**flag R-H(b):** this script and the typed AutoLISP file writes of section 3.2 go beyond the words "no instrument" of BA-11 row 1) | in: the designation record; out: six raw files, the machine record, the session record | the commands are reads; the OS script uses only `Get-*` cmdlets and `Get-FileHash` and writes only to the evidence folder; the typed AutoLISP `open ... "w"` file write of 3.2 is the same disclosed element as Q-O-0(b) (new result files under the evidence folder; a typed command, not the `EvidenceWriter` class); reviewed by reading |
| **I-2** census-read | T-2 (read part) | HF-C1, HF-C3, HF-G1 (M1) (HF-C5 is not read here: NOT_OBSERVABLE read-only); and, by a **separate command with its own record**, HF-T2 | **R0**, `NETLOAD` plugin, commands `CT21DHG_CENSUS_R` and `CT21DHG_INVENTORY`; **in the Q-O-0 gated set** (the R0 build that carries HF-G1 M1, HF-C1, HF-C3, HF-T2) | in: a private copy of the library; out: the census record of BA-05 V5 section 5 plus `rbTypes[]` (from `CT21DHG_CENSUS_R`) and `inventory-raw.json` (from `CT21DHG_INVENTORY`) | opens the private copy with `ReadDwgFile` into a side database, `OpenMode.ForRead`, no save; hashes before and after; see 5.3 |
| **I-3** census-dynamic | T-2 (dynamic part) | HF-C2 | **RS** (blocked until Q-O-1), command `CT21DHG_CENSUS_DYN` | in: the private copy; out: `dynamicProperties[]` | clones each dynamic definition into a **scratch side database** and reads the properties from a reference created there (BA-05 V5:1180); nothing is written to the copy; see 5.3 |
| **I-4** context-variable reader | T-3 (type readers) | HF-G3 | **R0**, a **non-Session** command `CT21DHG_CTXVARS` (the host holds the document lock); **not gated by Q-O-0(a)** (no side database) | in: the active document (declared in the tuple); out: `ctxVars[]` | calls `Application.GetSystemVariable` only; `SetSystemVariable` is a forbidden symbol; `DBMOD` equal before and after |
| **I-5** write-back probe | T-3 (write part) | HF-C4, HF-C5 (the designation, read from the stored pairs of the probe); HF-G2 only if the Owner extends G6 | **RS** (blocked until Q-O-1), command `CT21DHG_DIMWRITEBACK` | in: the product's override list (BA-05 V5:1115) and a style; out: `probe[]` | creates a `RotatedDimension` **in a scratch side database**, sets the overrides with the property setters the product uses, reads the extended data back; never touches the library copy |
| **I-6** TOL_SCALE probe | T-4 (instrument; the design is this document) | HF-T1 | **RS** (blocked until Q-O-1), command `CT21DHG_TOLSCALE`; the P-R pass is the separate I-2 command `CT21DHG_INVENTORY` (own record, same tuple) | in: the probe table; out: `tolscale-raw.json` | section 2.4: side database only; active document untouched |
| **I-7** fixture conformance reader | T-5 | HF-F1, HF-F2 conformance (BA-08 V7 12.6) | **R0 + product reader**, `conformance/` | in: a fixture instance (private copy); out: the conformance record | reads the authored bytes through the host API and passes the **strings** to the product's pure Application-layer reader (no Plugin call); the constraints are fixed here, the details belong to package v2 and the implementer; **it requires a RackCad build loaded or referenced, so it follows R8**; loading or referencing the product's Application-layer DLL inside the instrument process raises assembly identity and duplication questions if a pinned RackCad build is also `NETLOAD`ed in the same session, and that DLL's hash must enter TB-B; none of this is settled here, so **I-7 is not implementable from this document alone** (package v2). **I-7 is not in the declared set for H1 (R-N(b))**: the HF-F1 and HF-F2 conformance reads run in a later session under their own declared tuple |
| **I-8** build and pin | T-7 | the instrument packages and the `FIXTURE_CONSTRUCTION_BUILD` (clean checkout of `69daf03a`) and the exact build: who builds, where, configuration (Release proposed, R-11), DLL hashes captured | not an instrument: a procedure; the product bundle script exists (`deploy/build-bundle.ps1`, `-InventoryOutPath`), needs AutoCAD installed for the compile references and closed for replacing a loaded DLL | in: a committed SHA; out: package, manifest, pins | the build runs on the build side, outside the gate's read-only definition; the output is hashed and declared in the tuple |
| **I-9** evidence seal | T-9 | `HASHES.sha256`, read-only folder, process list | offline script (PowerShell), `tools/seal-evidence` | in: an evidence folder; out: `HASHES.sha256` and its own hash | writes only inside the evidence folder; re-run on a sealed folder must report no change |
| (not an instrument) | T-6 | the construction specification of `FX-IMP` and its variants | a document by the Architect / Coordinator | — | outside this design; HF-F2 waits for it |
| (exists) | T-8 | the precondition verifier (`docs/automation/evidence/I-52-ct21d-precondition-verifier.py`) | already in the repository | — | no work |

### 5.3 How a reviewer verifies the read-only property

1. **Semantic scan** (`tools/forbidden-api-scan`; it runs without a running AutoCAD but needs the AutoCAD managed reference assemblies at build time, as `eng/validation/I52G3AApiCensus` does). That repository tool is a Roslyn semantic census that already classifies call sites as `DECLARED_MUTATOR`, including "Object is opened ForWrite at this call site" (`Program.cs:222`) and "Explicit write-open marker" (`Program.cs:197`), and is the model. The scan resolves symbols and **constant values**: an `OpenMode` argument is resolved with the compiler's constant evaluator, so `OpenMode.ForWrite`, `(OpenMode)1` and a named constant are all seen as a write; an `OpenMode` argument that is **not** a compile-time constant is rejected as unresolvable. Forbidden in **every** instrument project (the exception for `rs/` is stated in item 3): `Database.Save`, `Database.SaveAs`, `Database.Wblock*` to a file, a write open or `UpgradeOpen`, `Transaction.Commit`, `Application.SetSystemVariable`, a setter of `HostApplicationServices.WorkingDatabase` (global host state, a hidden write that `DBMOD` and the hashes do not catch), `Overrule.*`, `Editor.Command`, `SendStringToExecute`, `DocumentManager.Open`/`Add`/`MdiActiveDocument` setters, `LockDocument` (the command is non-Session, so the host holds the lock), `Registry` writes, `Process.Start`, network types, `Assembly.Load*` of an unpinned file, `DllImport` / P/Invoke, and reflection that invokes members (`MethodInfo.Invoke`, `Activator.CreateInstance` on a non-declared type), and any `File` write whose path is not produced by the single `EvidenceWriter` class (which checks the evidence-folder prefix). **In `r0/`, additionally:** the only database construction is `new Database(false, true)` followed by `ReadDwgFile` of a private copy; no `new Database(true, ...)`, no side-database wrapper, no `Commit`, no save of any kind. The existing `I52G3AAutoCadProbe` fails this scan (`AddOverrule`), which is why it is not reused.
2. **Imports allowlist** (`tools/imports-allowlist-check`, a **backstop**): the member references of the built DLL (read with `System.Reflection.Metadata`) must be a subset of `allowed-apis.txt`, a file pinned in the package manifest. This layer cannot see a write open on its own: `OpenMode.ForWrite` is an enum literal that the C# compiler inlines as an integer constant and leaves **no member reference**, so `GetObject(id, OpenMode.ForWrite)` is indistinguishable from a read there. The write-open check is therefore made by item 1 or by an IL scan that tracks the constant operand at the `GetObject` / `UpgradeOpen` call sites; the metadata layer is claimed only for API membership.
3. **Structural properties are enforced by rules the scan can check, not by intention.** (a) `ForWrite`, `Commit`, `new Database(true, ...)` and `Save` call sites may appear in `rs/` only inside the `SideDb` wrapper and the `EvidenceSideDbSaver` class (the scan checks the type that contains the call site); (b) the constructors of those types are `internal` and take their database only from the single factory that creates side databases, so the wrapper cannot be built over a product database; (c) `File` and stream write call sites may appear only inside `EvidenceWriter`; (d) `EvidenceSideDbSaver` accepts only a new path under the evidence folder. Each rule is covered by a **negative control**: a test source with an injected violation must make the scan fail.
4. **What the R0 claim is.** R0 claims "no API write call and no save", **not** "nothing is written in memory". A side database that `ReadDwgFile` fills can be evaluated by the host (for example the anonymous blocks of dynamic references, internal caches) in memory, and no API-symbol scan can see that. This residual is disclosed. The controls are that the side database is never saved, `DBMOD` of the active document and the hashes of the private files and of the source library are equal before and after, and the evidence folder holds only the declared outputs. Whether this residual lies inside the Owner's "solo lectura" is **Q-O-0(a)** (section 6): the Q-O-0 gated set, i.e. every R0 read that uses a side database (HF-G1 M1, HF-C1, HF-C3, HF-T2, all carried by the I-2 R0 build), proceeds only on the Coordinator's or the Owner's confirmation. HF-G3 (I-4) uses no side database, the residual does not arise for it, and it is not gated by Q-O-0. R0 also writes **new result files under the evidence folder, only through the single `EvidenceWriter`** (rule (c) below); that is a separate disclosed element, **Q-O-0(b)**, common to all instruments, and no scan can show that the host is indifferent to it: it is put to the Coordinator and the Owner, not asserted.
5. **Review by the Architect** of the source against the matrix: every output field maps to a row of 3.4; a field without a row is rejected.
6. **Host-run checks**: SHA-256 of the private files and of the source library before and after (equal); `DBMOD` of the active document before and after (equal); `SECURELOAD`, `TRUSTEDPATHS`, profile before and after (equal); the evidence folder contains only the declared outputs.

### 5.4 Tests that need no running AutoCAD, and what only the host can validate

| Instrument | Tests without a running AutoCAD (only `core/` needs no AutoCAD files at all) | Only on the host |
|---|---|---|
| I-0 / I-1 | the two synthetic label vectors of 3.2; RFC 8785 escaping vectors (control characters, non-ASCII, backslash, empty string); rejection of invalid UTF-8; the probe-table generator reproduces the 147 rows and the bit patterns of 2.4; the statistics `D_A`, `K_pres`, `I_max` on hand-made row sets, including the Sterbenz-exact flag, the signed-zero row, a snapped-host row set and an all-exact row set; the classification state machine of 2.5; the offline recomputation equals the instrument's | that the registry values and variables exist and read as defined in A1-R1..R6 |
| I-2 / I-3 | a fake drawing reader behind an interface (block table, entity classes, dimension extended data) drives the census record builder; schema validation of the output; the RB expectation table is **mechanically compared with the table in the BA-05 V5 document** (parse the markdown, compare ranges and types); order preservation | the real `RXClass` names, the DXF names, the dynamic-property reading in a scratch reference, the `DSTYLE` storage and order, which RB ranges actually occur |
| I-4 | the expectation table equals BA-05 V5:127-138; the record builder; "matches" logic | the runtime types returned by `GetSystemVariable` on the exact build |
| I-5 | the override list equals BA-05 V5:1115; the readback parser on synthetic extended data | whether the host stores or drops an equal-to-style override; the group codes; the order |
| I-6 | everything under I-0 for the probe and statistics; the writer of `tolscale-raw.json`; schema validation | the host's storage of the scale, the witness, OP2 |
| I-7 | reading a synthetic fixture description through the same interface | the real fixture instance |
| I-8 / I-9 | build twice and compare hashes (deterministic build; the compile needs the AutoCAD managed assemblies installed, not a running AutoCAD); `HASHES.sha256` round trip; a sealed folder re-run reports no change | that the loaded DLL equals the pinned hash; the `NETLOAD` outcome under the recorded `SECURELOAD` / `TRUSTEDPATHS` |

## 6. Rulings the Coordinator is asked to make

| Id | Question | Architect's proposal |
|---|---|---|
| **Q-O-0** (COORDINATOR, or the Owner if the Coordinator does not settle it) | Two disclosed elements of the R0 class, asked together. **(a) the R0 residual:** is it inside the Owner's "solo lectura" / "READ-ONLY" of G6? R0 reads a private copy of a file into an in-memory side database (`ReadDwgFile`); it makes no API write call, no save and writes to no product or open-document database, but the host may evaluate, cache or write in memory inside that side database or the process (5.3 item 4), which no scan can exclude. **(b) result files:** is it inside "solo lectura" that an R0 or RS instrument (and the I-1 typed AutoLISP file write) writes **new result files under the evidence folder through the single `EvidenceWriter`**? It writes no product, catalog, library or open-document file. **Gated set (one explicit list, repeated in the header, 3.4, 4.1, 5.1, R-A, R-G, 7 and 8):** the Q-O-0 gated set is {HF-G1 (M1), HF-C1, HF-C3, HF-T2 and the I-2 R0 build (`CT21DHG_CENSUS_R`, `CT21DHG_INVENTORY`) that carries them}; HF-G3 and I-4 are **not** in it. Consequence of (a) = no: HF-G1 (M1), HF-C1, HF-C3, HF-T2 and the I-2 R0 build cannot run as instruments, and only HF-G3 (system-variable reads, no side database) and the CAD manager's typed reads remain; BA-05 L-4 (b) and the census would need another route. Consequence of (b) = no: no instrument and no typed command can record a result file, so no record could be closed by the route of this design; another recording route would have to be ruled. **Sequencing consequence (stated once):** no host activity starts and no tuple record is closed until the whole declared set is final; the declared set is final only when Q-O-0 (a) and (b) and Q-O-1 are answered, so H1 waits on Q-O-1 while the RS DLL is declared; if Q-O-1 = STRICT only, the RS DLL is struck from the declared set (R0 only) and H1 waits only for Q-O-0 and for the R0 DLL, and if Q-O-0(a) = no the I-2 R0 build is also struck | The Architect does not answer it and does **not** assert that the gated set satisfies the Owner's boundary: the gated set proceeds only on this confirmation. HF-G3 is not gated by Q-O-0(a) because no side database exists in I-4 (it is still subject to (b) and to the sequencing rule). Textual evidence only: the Owner's third G6 bullet speaks of host-type facts read "READ-ONLY"; BA-05 V5:1180 itself reads through a side database; the controls of 5.3 item 4 (never saved; `DBMOD` and hashes equal; the evidence folder holds only the declared outputs) bound the residual but do not remove it |
| **Q-O-1** (OWNER, taken by the Coordinator) | What does the Owner's "solo lectura" (decisions section 237 point 5, point 8 item 5; section 236 Owner decision (d)) and "READ-ONLY" of G6 admit: only STRICT READ-ONLY instruments (no write call, no save), or also instruments that write to side databases in memory and save new files under the evidence folder (RS: I-3, I-5, I-6, the HF-C4 write probe, the OP2 save)? One-sentence consequence for the Owner: if the answer is STRICT, the TOL_SCALE host test cannot run (TOL_SCALE stays UNSET), the dynamic-property census and the write-back probe cannot run, and BA-05 L-1 (dynamic part) and L-4 (write-back part) cannot close unless BA-05 is re-prepared without them | The Architect does not propose an answer to the Owner's word; the textual evidence is in section 2.4 (BA-11 row 2 without qualifier, BA-05's own scratch-reference census, R-3 admitting a host activity) and decides nothing |
| **Q-O-2** (OWNER, conditional) | HF-G2 (round trip of the RB ranges that stored data cannot show): does the Owner extend G6 to a scratch round trip? | do not ask now; accept fallback F-RB (BA-05 keeps only the ranges that M1 confirmed); ask only if the M1 coverage is too small for BA-05 to be useful |
| R-A | The Coordinator settles or takes Q-O-0, takes Q-O-1 and Q-O-2 to the Owner, and confirms that the Q-O-0 gated set {HF-G1 (M1), HF-C1, HF-C3, HF-T2 and the I-2 R0 build (`CT21DHG_CENSUS_R`, `CT21DHG_INVENTORY`) that carries them} (HF-G3 and I-4 are not in it) waits for Q-O-0 and that **I-3, I-5, I-6, the HF-C4 write probe and the OP2 save stay blocked until the Owner answers**; the Coordinator does not settle the Owner's wording by analogy or by reading R-3 as covering an instrument | yes |
| R-B | Is the TOL_SCALE test (P-A and P-R, section 2.4: 147 rows, 135 in scope, OP1 and OP2 mandatory, one execution, the classification of 2.5) approved as the H4 activity, **conditional on Q-O-1 and on PRE-1 to PRE-4**? | yes, conditional |
| R-C | (a) Split `TOL_SCALE` (C1, the comparator) from the SL-4 acceptance bound (C2), with C2 getting its own defined reference quantity and its own authority or staying UNSET (TS-D-03)? (b) The product's source classification (C3, V17 ST-7, "G4") a separate decision, with the I-55 precedent (`RackProjectionSnapshotReader.cs:25-27`) acknowledged and cited in the next BA-08 but not adopted? (c) Where the SCALE slot-unit vectors of branch R2 live (BA-05 through the BA-08 harness mechanism, or BA-08)? | (a) split; (b) yes; (c) through the mechanism of BA-08 V7:392 |
| R-D | HF-C5: (a) is the group-code designation accepted as ROW3 (BA-05 step 5 (a)), although BA-11 row 3 names storage, order and equal-to-style but not the designation, and **declared NOT_OBSERVABLE from read-only sources** (the HF-C3 pairs carry no variable name), observable only by the HF-C4 write probe, which is blocked by Q-O-1? (b) is the corroborating comparison of the override values with the dimension's own variable values (`CORROBORATING_ONLY`, never a confirmation) wanted as an extra read with its own basis and instrument field, or not? | (a) yes; (b) no, not adopted; if Q-O-1 is STRICT the designation stays unconfirmed and BA-05 decides how to carry it |
| R-E | (merged into Q-O-2) | see Q-O-2 |
| R-F | `TRUSTEDPATHS`: option (a) one recursive trusted root decided before H1 (instrument packages and each pinned RackCad build placed under it), (b) per-package entries, or (c) another mechanism? | (a), with the CAD manager choosing the path and the hash pinning compensating |
| R-G | Which instruments may be implemented first? Implementation may proceed in phases, but **no host activity starts and no tuple record is closed until the whole declared set is final** (R-N): phase 1: I-0, I-1 (protocol and helper), I-4, I-9 (STRICT as defined in section 0, or offline; not gated by Q-O-0(a), but subject to Q-O-0(b) because they write result files) may start; **the I-2 R0 build (`CT21DHG_CENSUS_R`, `CT21DHG_INVENTORY`) cannot start before Q-O-0(a)**, because it carries HF-G1 (M1), HF-C1, HF-C3 and HF-T2, the Q-O-0 gated set; phase 2: I-3, I-5, I-6 **only after Q-O-1 is answered in their favour**; since no host activity starts and no tuple record is closed until the whole declared set is final, H1 waits on Q-O-1 while the RS DLL is declared and on Q-O-0; **I-7 is not in the declared set for H1** (R-N(b)); I-8 is a procedure, not a loaded instrument; both follow package v2. If Q-O-1 is STRICT, the RS DLL never exists, the declared set is R0 only and H1 no longer waits for RS (it still waits for Q-O-0 and for the R0 DLL); if Q-O-0(a) is no, the I-2 R0 build is struck as well | as stated |
| R-H | A-1: are A1-R1..A1-R6 accepted as the exact reads of the six attributes (written into BA-10 V9 with the label)? (a) A1-R2: proposal (i) (`Win32_OperatingSystem.Version` as one read; a cumulative update does not change the label, in line with the Owner's AR10_03B_READING) or alternative (ii) (the four-part join with `UBR`; every cumulative update changes the label)? To be ruled before the label is computed. (b) Do the one fixed transfer mechanism of section 3.2 (a typed AutoLISP file write; PowerShell for the registry reads) and the OS-level script of I-1 stay inside the words "no instrument, no manifest" of BA-11 row 1? (c) The comparison rule of the A1-R3 cross-check | accept with (a) proposal (i); (b) yes, as typed commands and a documented protocol; (c) accept; `[HOST-TO-CONFIRM]` items are resolved by the CAD manager's raw outputs |
| R-I | Interactive `acad.exe` for the TOL_SCALE test, the type readers and the census, noting that the Coordinator's R-9 (section 236, "Core Console para el censo si es suficiente") already admits the Core Console for the census; and, for package V1's "console-less", is a one-line command-line status allowed? | interactive for all; a one-line status allowed |
| R-J | Decider and evidence discipline of the TOL_SCALE value: the Architect decides after the pre-registration entry (2.3); the Coordinator verifies the text; the Owner is not asked for the value | accept |
| R-K | After a non-zero `delta` in P-A, may the Architect request one additional authorized execution (a new run id, not a retry) to check repeatability? | yes, by ruling, never automatic |
| R-L | BA-11: inscribe the G6 extension in the registry's section 3.3 plan table in the next registry version | yes |
| R-M | Prerequisites PRE-1 to PRE-4 of section 2.3 as a gate of H4 and of the decision, and the closed margin function (the smallest decade strictly above `F`, no free factor) fixed in the pre-registration entry **together with the ceiling `tau <= TOL_LENGTH / E_max`**, where `E_max` is an upper bound, in inches, on the distance from a scaled reference's insertion point to any point of its definition's geometry over every expected value of the sealed fixtures, declared by the Architect from the sealed fixture plan parameters before the host run (PRE-3; no extents are read from the host); or another closed function ruled by the Coordinator **before** the host run. **Also ruled before the host run:** routing `scaleX/Y/Z` to `EXACT_DOUBLE` (branch R1(b)) changes a **comparator class** (G-01, G-02), not only a value, so R1(b) is available only if the Coordinator rules it beforehand; otherwise a result R1 can only inscribe (a) | accept; the Architect prefers R1(b) and asks the Coordinator to rule on it before the run |
| R-N | Reading of R-5 and package V1 P-0.3: (a) the "baseline" is the **declared set** (all R0 and RS DLLs, the package manifest, and the declared RackCad build of a run that loads one: none in H1, package V1 section 3 row H1), final and recorded before H1; the `NETLOAD` of a declared module is an expected event, not a plugin change; and this applies the Owner's R8 condition to instruments as a stated extension. **(b) I-7 scope:** **I-7 (the fixture conformance reader) is NOT in the declared set for H1** (it needs a RackCad Application-layer DLL, its assembly identity is unsettled, and package v2 specifies it). Consequently the conformance reads HF-F1 and HF-F2 (BA-08 V7 12.6) are **not in H1**; they run in a **later session under its own declared SESSION_BUILD_TUPLE** that lists I-7 (its DLL SHA-256, the manifest and the referenced RackCad DLL, TB-B) **before that later run**; the evidence of H1 is not transferred to it and the evidence of that session is not transferred back (the Owner's R8 reading, decisions section 237 point 3). H5 step 2, H6 and H7 need the exact PRODUCT_BUILD, which does not exist (package V1 section 3 row at line 311), so they could not be in H1 in any case; where the construction of `FX-ANN-BLANK` step 1 runs is left to package v2 and is not decided here. The rule "the whole declared set is final and in the tuple before H1" (R-G, R-N(a), R-5, P-0.3) therefore applies to the set without I-7 | accept |

## 7. Residual open questions

1. **`E_max` and `tau_arith` are sealed prerequisites** (PRE-1, PRE-3), not open parameters of the rule: until the SL-4 / `mu_k` formulas and the fixture plan parameters are sealed, R1 (`tau_arith = 0`) is an assumption stated by V17 2039 and ST-11, and R2 cannot be applied. The H4 run is gated (R-M). The Architect can compute `E_max` offline from BA-08 section 12 once those parameters are sealed.
2. **The SL-4 writer route** (assignment, `TransformBy`, clone / import) is not fixed (PRE-2); K-3 cannot close before it is.
3. **The scope of the Owner's "solo lectura"** is the Owner's question (Q-O-1). If strict, P-A, the dynamic-property census and the write-back probe cannot run, and BA-05 L-1 and L-4 cannot close; the Owner is told that consequence in one sentence.
4. **Coverage of RB ranges by existing data (M1).** Unknown until the census runs; it decides whether F-RB is acceptable (Q-O-2).
5. **`NETLOAD` behaviour under the designated machine's `SECURELOAD` and `TRUSTEDPATHS`.** Unknown; recorded at the first instrument load; a prompt makes the TOL_SCALE test INVALID (S1).
6. **Which Windows or AutoCAD facts of section 3.2 differ on the designated machine** (`[HOST-TO-CONFIRM]` items): resolved by the CAD manager's raw outputs, never by this document.
7. **C2 and C3** (TS-D-03, R-C): the reference quantity of the SL-4 acceptance is undefined by the contract text; the I-55 `1e-9` must be cited by the next BA-08.
8. **Application of the derived value to other tuples** is a disclosed residual risk (section 2.3); Owner Act 2 is reserved.
9. **Package v2 / AR10-07** is not written; this document supplies its sections 2 and 5.
10. **I-7** (fixture conformance reader) is not implementable from this document alone (assembly identity and duplication with a pinned RackCad build; package v2). It is **not in the H1 declared set** (R-N(b)); HF-F1 and HF-F2 conformance run in a later session under its own declared tuple.
11. **Q-O-0** ((a) the R0 residual; (b) new result files through the single `EvidenceWriter`): until the Coordinator or the Owner confirms that they lie inside "solo lectura", the Q-O-0 gated set {HF-G1 (M1), HF-C1, HF-C3, HF-T2 and the I-2 R0 build (`CT21DHG_CENSUS_R`, `CT21DHG_INVENTORY`) that carries them} does not start; HF-G3 and I-4 are not in it; the Architect asserts no pass of the boundary test for the gated set.
12. **HF-C5** (the designation) is NOT_OBSERVABLE from read-only sources and depends on the HF-C4 write probe (Q-O-1); the BA-05 treatment of an unconfirmed designation is a BA-05 decision (R-D).

## 8. Status

```text
TOL_SCALE_DESIGN                 = PROPOSED_FOR_COORDINATOR_REVIEW (revised in place after the adversarial reviews; Review record, section 10)
INSTRUMENT_IMPLEMENTATION_GATE   = NOT_OPEN
HOST_RUN_STARTED                 = FALSE
TOL_SCALE VALUE                  = UNSET (no value chosen; closed derivation rule and test designed; decider: the Architect after the sealed evidence; H4 gated by PRE-1 to PRE-4)
OWNER_QUESTIONS_PENDING          = Q-O-0 ((a) R0 residual, (b) result files via EvidenceWriter; the gated set HF-G1 M1, HF-C1, HF-C3, HF-T2 and the I-2 R0 build waits for it; HF-G3 and I-4 do not), Q-O-1 (scope of "solo lectura"), Q-O-2 (HF-G2); the gated set, I-3, I-5, I-6, the HF-C4 write probe and the OP2 save are BLOCKED until answered; no host activity starts and no tuple record is closed until the whole declared set is final
G6                               = EXTEND (Owner, read-only host-type facts of the current BA-05); in this design the G6 facts are HF-G1 (M1) and HF-G3 only; HF-G2 is NEEDS-OWNER;
                                   HF-C3 and HF-C5 are ROW3, not G6 (HF-C5 is NOT_OBSERVABLE read-only); HF-G1 (M1) proceeds only on the confirmation of Q-O-0(a) and HF-G3 is not gated by Q-O-0; HF-C4 is ROW3 by R-3 as a host activity and NEEDS-OWNER as an instrument
MACHINE_DESIGNATION              = DELEGATED_TO_CAD_MANAGER (no machine designated; no tuple record)
NO INSTRUMENT WRITTEN / BUILT / LOADED; NO AUTOCAD STARTED; NO FILE UNDER src/, tests/ OR eng/ CHANGED BY THIS DOCUMENT
CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
CIA = UNKNOWN     SafeOperationalState = FALSE_FOR_ADMISSION     SCOPE = NONE     G3 = STOPPED     FALLBACK = ALT-21E
```

## 9. Verification log

Every path below was read in this worktree (HEAD `e2079ed1`, branch `feature/rackmirror-espejo-semantico`); the blob id is the output of `git hash-object <path>` of the working-tree file. The two files whose relation says "working tree" (the decisions log, which carries the uncommitted section 237, and the untracked Owner record of section 237) are not yet in a commit as written. The bound product `95690c28dc6268e61dff32a0cbc33cc9fde3d47f` equals `git rev-parse main`; `git diff --stat main HEAD -- src` is empty. Commit `69daf03a`: tree `3c34dc9be113e98088acf56653aa5181437d0893` (`git rev-parse 69daf03a^{tree}`).

| Path | Blob id (`git hash-object`) | Relation to HEAD | Used for |
|---|---|---|---|
| `docs/automation/decisions/I-52.md` | `858509b4e2c8e30d5f04676946b07f768406018a` | working tree (differs from HEAD `374d86be`) | decisions log, sections 230, 234 to 237 (uncommitted: section 237 is a working-tree change) |
| `docs/automation/evidence/I-52-ct21d-owner-decisions-machine-g6-instruments.md` | `2feb0bec63fd712cb28cc5209b6dffb8429a0e3b` | working tree (differs from or absent in HEAD) | Owner record of 2026-09-30 (decisions section 237; untracked when written) |
| `docs/automation/evidence/I-52-ct21d-owner-decisions-q-o1-q-o5-and-host-gate.md` | `cdb1a13682c63571cf4a4b4bb03bbf0fb0dc19ec` | HEAD | Owner record (decisions section 233) |
| `docs/initiatives/I-52-ct21d-host-gate-execution-package-v1.md` | `b23221a8b4cb8f2b30135b1e811456be2cbb976c` | HEAD | host-gate package V1 (section 234) |
| `docs/initiatives/I-52-ct21d-baseline-ba-05-fingerprint-specification-v5.md` | `b74af94ece4f0901a071ef5176f3c58bff01cfdc` | HEAD | BA-05 V5 |
| `docs/initiatives/I-52-ct21d-baseline-ba-08-selective-kind-baseline-v7.md` | `57330c3a09d80e919a63a2b83a45d168da326140` | HEAD | BA-08 V7 |
| `docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v7.md` | `07e18f5f5ac2d5ee4c4c613e5762f2f57d47aef4` | HEAD | BA-04 V7 (text) |
| `docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v7.json` | `03c55e11570ce622e91bf5d48bba075891446e96` | HEAD | BA-04 V7 (JSON; counts verified by script) |
| `docs/initiatives/I-52-ct21d-baseline-ba-06-evm-preparation-v7.md` | `74fa270ccba8dff89ccbf3ac71a125bb8c142b44` | HEAD | BA-06 V7 (C-146, DC-13) |
| `docs/initiatives/I-52-ct21d-baseline-ba-10-parameter-sheet-v8.md` | `820d90c8a2172ad32697e66f0c9b0f3eb5fadbdb` | HEAD | BA-10 V8 (PARAM-05) |
| `docs/initiatives/I-52-ct21d-baseline-ba-11-hash-registry-v10.md` | `9984e9f8ff6675afa514656e49d5a2fa01756916` | HEAD | BA-11 V10 (section 3.3 rows identical to V9) |
| `docs/initiatives/I-52-ct21d-baseline-ba-11-hash-registry-v9.md` | `6d6212a4bbddc8cb7c2ef3cbde7a56a16822f515` | HEAD | BA-11 V9 (the version the package quoted) |
| `docs/initiatives/I-52-ct21d-authority-contract-v2.1.md` | `6259a3beb7445a63620636acd696cdad689965b3` | HEAD | contract V2.1 |
| `docs/initiatives/I-52-ct21d-authority-contract-v2.2.md` | `fd7260c57ec05e11f6bbb9f67a728e7dfdf1f609` | HEAD | contract V2.2 (PA-9) |
| `docs/initiatives/I-52-ct21d-authority-contract-v2.6.md` | `fd9c16873c526e5f009ad92beb8dd2fe236b964f` | HEAD | contract V2.6 (effective, section 230) |
| `docs/initiatives/I-52-proposal-v17.md` | `dd944d8e9c62b083eebc2b1292789977b880c368` | HEAD | V17 (ST-7, ST-8, ST-11, 4.3, CreateReference) |
| `docs/initiatives/I-52-g3a-ct49-session-host-api-characterization.md` | `ceb9f7210de2bcfde2b2228a727853c4d739265b` | HEAD | G3A characterization (registry identity) |
| `docs/automation/evidence/I-52-ct21d-architect-phase1-ruling.md` | `8a433a000a40640a4d1302a2af24da114b0faaef` | HEAD | Architect phase-1 ruling (item 5) |
| `docs/automation/evidence/I-52-ct21d-phase2-static-analysis/B5-tolerance-and-transform.json` | `d3d673245c072acf0ae4cd8b2c7e457ec1f72470` | HEAD | static analysis B5 (F13, tolerance without default) |
| `docs/automation/evidence/I-52-ct21d-precondition-verifier.py` | `bc90b55f20c530e8eff25ffd931eb4b85c9303ae` | HEAD | precondition verifier (T-8, exists) |
| `src/RackCad.Application/Systems/Shared/RackSourceTransformFactsV2.cs` | `25e812ee1e7b20455dc760e6416397ac65769425` | HEAD | scale tolerance type and classifier |
| `src/RackCad.Application/Geometry/Vector2D.cs` | `c88c67cdbff1a4a4b3695732b22003e129daba74` | HEAD | GeometryTolerance |
| `src/RackCad.Plugin/Views/RackProjectionSnapshotReader.cs` | `3e39cca2cbe68ec6e5793c9ab9a80936e2c3690f` | HEAD | I-55 reader: scale tolerance supplied |
| `src/RackCad.Plugin/Drawing/LateralHeaderDrawer.cs` | `65db706081e7c50f920acbd1b9f8c414f5a82f21` | HEAD | scale assignment by the product |
| `src/RackCad.Plugin/Drawing/RackSourcePlacementCaptureAdapter.cs` | `23286d03eafee950292818b2be0e6e945608c22a` | HEAD | scale read by the product |
| `src/RackCad.Application/Catalogs/BlockLibrary.cs` | `ce8042144e645a6408d7c3284b37e8808d2b4632` | HEAD | library file name |
| `deploy/build-bundle.ps1` | `9a4e6adecf935b90b52f97b1450aac30970d48f1` | HEAD | product bundle script (I-8) |
| `eng/research/I52Ctda/README.md` | `e8cd6df4ac04d5f991ab07f655778a1ee2162661` | HEAD | CT-DA research folder isolation (location model) |
| `eng/research/I52Ctda/build-r3.ps1` | `999a86106215571bd690db01d84077a9f848e978` | HEAD | canonical build script (build model) |
| `eng/validation/I52G3AAutoCadProbe/ProbeCommands.cs` | `51189e537606b26d13686ec7c7f246af2173fa89` | HEAD | G3A probe (not read-only: AddOverrule) |
| `eng/validation/I52G3AApiCensus/Program.cs` | `a41235ab740eaffed87e7b01c427e34408a47e2c` | HEAD | static API census (model for the imports check) |
| `Directory.Build.props` | `69223076ee12a375cef376e0a8eb8a14ba5f1331` | HEAD | inherited build settings of the instrument projects (section 5.1) |
| `Directory.Build.targets` | `58d1c335035665d98fc8b9e31b090b2f0f7a4222` | HEAD | `SourceRevisionId` stamped from `git rev-parse HEAD` (section 5.1) |
| `eng/research/I52Ctda/managed-observer/ObserverApplication.cs` | `7482a3f56b045f46814f63ee070ff10948e1b10e` | HEAD (not in `main`) | managed `NETLOAD` precedent (section 5.1) |

Commands run to produce and check the table: `git hash-object <path>` for each row; `git rev-parse HEAD:<path>` to compare with the committed blob; `git rev-parse main` (= `95690c28dc6268e61dff32a0cbc33cc9fde3d47f`); `git diff --stat main HEAD -- src` (empty); `git rev-parse 69daf03a^{tree}`. Line citations were checked by reading the cited line for the quoted text. The rows added by the revision (`Directory.Build.props`, `Directory.Build.targets`, `ObserverApplication.cs`) were hashed with `git hash-object`; `git ls-tree -r main --name-only` shows no file under `eng/research/I52Ctda`, and the same listing of `HEAD` shows 70. The citations of the revision (decisions log lines 10016, 10019 and 10027; package V1 lines 212 and 464; BA-05 V5:1199; BA-08 V7:392 and 606; V2.1 15.6 and 15.7; `Program.cs:197` and `222` of the API census; `RackProjectionSnapshotReader.cs:25-27`; `build-r3.ps1:66`) were read in the working tree.

Citations touched by revision 2, re-read with git and `sed` in this worktree: BA-05 V5:107, 1115, 1183 and 1201; package V1 lines 212, 307, 311 and 464; decisions log lines 10016, 10019, 10027, 10047 and 10050; the Owner record lines 42, 184 and 203. `git hash-object` of the decisions log (`858509b4e2c8e30d5f04676946b07f768406018a`) and of the Owner record (`2feb0bec63fd712cb28cc5209b6dffb8429a0e3b`) is unchanged. The duplicate triple of the PF-1/PF-2 row count was computed by script (147 rows, 146 distinct triples; 135 in-scope rows, 134 distinct triples).

Counts verified by script on the BA-04 V7 JSON: 437 scenarios, 138 with `BLOCKED_BY` containing `K-3`, 45 with `K-8`, all 138 `K-3` scenarios `GOVERNING`.

## 10. Review record

The same draft was revised in place after three adversarial reviews (Reviewer A: statistic, Owner scope, C2, rule; Reviewer B: G6 boundary and matrix; Reviewer C: instruments, baseline, verification) and, in a second round, after a second adversarial review whose three remaining MAJOR findings and accepted minors are answered in the table headed "Second review" at the end of this section; a third review (two MAJOR findings and minors) is answered in the table headed "Third review". Disposition codes: **APPLIED**, **REJECTED** (with the reason), **OPEN** (turned into an open question or a ruling request). No value was chosen, no authorization was widened, no fail-closed behaviour was weakened.

| Finding | Disposition | Where |
|---|---|---|
| A-MAJOR-1: `D_A` at OP1 only; OP2 optional | **APPLIED.** OP2 is mandatory and `D_A = max(OP1, OP2)`. The design states, with the citation of V2.1 15.7 and BA-08 section 11, that neither settles whether the comparator reads reloaded state, and takes the stricter reading; the schema requires `op2` and `DA_op2` | 2.3, 2.4 (S6), 2.5 (PASS V2), schema |
| A-MAJOR-2: Owner scope decided at the wrong level | **APPLIED.** "READ-ONLY (PRODUCT)" is no longer the Architect's definition or proposal; the Owner's word is not redefined. The classes STRICT and WRITES-SIDE are descriptive. New Owner question Q-O-1 (and Q-O-2); I-3, I-5, I-6, the HF-C4 write probe and the OP2 save are BLOCKED until the Owner answers; R-A, R-B and R-G say so; R-3 is stated as admitting a host activity, not a non-read-only instrument | 0, 2.4, 5.1, 6 (Q-O-1, R-A, R-B, R-G), 8 |
| A-MAJOR-3: C2 bound to an underived value | **APPLIED** for the split and the consequences, **OPEN** for the C2 definition. The slot is split: `TOL_SCALE` is C1 only; the C2 reference quantity is stated to be undefined by the contract text and is not invented here (candidates listed, none chosen); the R1(b) consequence for C2 is stated; the I-55 `1e-9` is cited, and the next BA-08 must cite it. The C2 definition and the C3 question go to R-C | 2.2 (TS-D-03), 2.3 (R1), 6 (R-C), 7 |
| A-MAJOR-4: rule not applicable; `M` post hoc | **APPLIED.** `M` is gone: the margin is a closed function (the smallest decade strictly above `F`, no 1-2-5 relief). `tau_arith`, `E_max`, the scale formulas, the writer route and the fixture plan parameters are sealed prerequisites PRE-1 to PRE-4 and a gate of H4 and of the decision. The pre-registration is sealed before the host run. The closed function is the Architect's proposal; the Coordinator may rule another one before the run (R-M) | 2.3, 6 (R-M), 7 |
| A-MINOR-1: BA-05 L-2 not reconciled | **APPLIED** | 2.6 (BA-05 row) |
| A-MINOR-2: HF-C5 read part in G6 | **APPLIED** (see B-MAJOR-1); the "derived from HF-C3" part is superseded by R2-MAJOR-2 | 3.4, 3.5, 4.1, 6 (R-D) |
| A-MINOR-3: PF-2 and `D_A` | **APPLIED.** 135 rows in scope; the 12 PF-2 rows of other bases are characterization (`DA_char`), observed but outside the rule | 2.3, 2.4, schema |
| A-MINOR-4: transfer to other tuples | **APPLIED** as a residual-risk disclosure; Owner Act 2 reserved | 2.3 (Transfer), FM-8, 7 |
| A-MINOR-5: prompt makes P-A unrunnable | **APPLIED.** S1 requires a scripted or pre-trusted route; V8 in the PASS conditions | 2.4 (S1), 2.5 |
| A-MINOR-6: `K_pres` wording | **APPLIED** (finest level such that all levels not finer are bit-identical; equivalence with `D_A = 0` checked) | 2.3 |
| A-MINOR-7: schema and procedure consistency | **APPLIED.** `op2` required (nullable only for UNKNOWN rows), `summary` closed, `decisionEligible` separate from `result`; Core Console reconciled through R-I citing R-9 | 2.4, schema, 6 |
| A-MINOR-8: expected scale set | **APPLIED.** `{+1, -1}`; the plan's `s` is no longer listed as an expected value (BA-08 V7:606) | 2.2 |
| A-MINOR-9: writer route not fixed | **APPLIED.** PRE-2; K-3 cannot close before the route is fixed | 2.3, 2.4, FM-9, 7 |
| B-MAJOR-1: HF-C5 read part inside G6; "no G6 fact stretched" | **APPLIED.** The read part was said to be derived from the ROW3 pairs of HF-C3 (found impossible in round 2: R2-MAJOR-2) and is not G6; the claim is removed; boundary question 1 is tightened to host value types and ranges; R-D asks the Coordinator | 3.4 (HF-C5, HF-G4), 3.5, 4.1, 8 |
| B-MAJOR-2: HF-C3 dual basis | **APPLIED.** ROW3 only (BA-05 step 5 (a)) | 3.4 (HF-C3) |
| B-MINOR-1: capture of the six strings | **APPLIED.** One fixed transfer mechanism per source, explicit UTF-8, a character-count check; the `open` encoding argument and `strlen` semantics stay `[HOST-TO-CONFIRM]`; whether this stays inside BA-11 row 1 is R-H(b) | 3.2 |
| B-MINOR-2: A1-R3 comparison rule | **APPLIED.** The cross-check rule is written; cross-check 2 is marked outside ROW1 | 3.2 |
| B-MINOR-3: UBR in A1-R2 | **APPLIED, with a changed proposal.** Proposal (i) is one read of `Win32_OperatingSystem.Version` (read text, no join, in line with AR10_03B); the four-part join with `UBR` is alternative (ii); the ruling is R-H(a), before the label is computed | 3.2, 6 |
| B-MINOR-4: TB-C-only binding | **APPLIED.** Binding rule: `TB-S` always accompanies `TB-C`; M1 to M6 carry it | 3.1, 3.4 |
| B-MINOR-5: undefined TB-F | **APPLIED** (removed from HF-F1) | 3.4 |
| B-MINOR-6: HF-T2 inside I-2 | **APPLIED.** A separate command and a separate record; the extents field is struck | 2.4, 3.4, 5.2 |
| B-MINOR-7: fixtures as an RB source | **APPLIED.** Dropped as a source | 3.4 (HF-G1) |
| B-MINOR-8 (positive spot check of HF-M4b) | no change | - |
| C-MAJOR-1: R-5 and P-0.3 not reconciled | **APPLIED.** The whole instrument set is final and in the tuple before H1; R-G no longer places phase 2 after H1; S0 and S1 follow; the extension of R8 to instruments is stated as such (R-N) | 3.3, 5.1, 6 (R-G, R-N), 2.4 |
| C-MAJOR-2: instrument split inconsistent | **APPLIED.** P-A is I-6 (RS) and P-R is a separate I-2 (R0) command with its own record; both are declared in one tuple; the tolscale schema has no `inventory` | 2.4, 5.2, schema |
| C-MAJOR-3: read-only verification overclaimed | **APPLIED.** Semantic scan with constant evaluation (model: the repository's Roslyn census and its `DECLARED_MUTATOR`), the metadata layer reduced to an API-membership backstop, structural rules plus negative controls, and the R0 claim restated as "no API write call and no save" with the in-memory residual disclosed | 5.3 |
| C-MINOR-1: "without AutoCAD" | **APPLIED** (without a running AutoCAD; only `core/` needs no AutoCAD files) | 5.4 |
| C-MINOR-2: inherited build files | **APPLIED** (explicit `SourceRevisionId`, AutoCAD assembly versions in TB-B) | 5.1 |
| C-MINOR-3: self-hash pinning | **APPLIED** (limits stated; independent pins; `Assembly.Location` `[HOST-TO-CONFIRM]`) | 5.1 |
| C-MINOR-4: forbidden list | **APPLIED** (`WorkingDatabase` setter, P/Invoke, reflection) | 5.3 |
| C-MINOR-5: host-loaded modules | **APPLIED** (`hostLoaded[]` rule) | 3.4 (HF-M9) |
| C-MINOR-6: Core Console and R-9 | **APPLIED** (R-I cites R-9) | 2.4, 6 |
| C-MINOR-7: I-1 tooling and "console-less" | **OPEN.** Flagged in R-H(b) and R-I | 5.1, 5.2, 6 |
| C-MINOR-8: I-7 assembly identity | **OPEN.** Stated as not implementable from this document alone; package v2 | 5.2, 7 |
| C-MINOR-9: OP2 DWG not deterministic | **APPLIED** | 2.4 (S6), 5.1 |
| C-MINOR-10: precedent folders not on `main` | **APPLIED** (stated; managed-observer cited) | 5.1, 9 |
| C-MINOR-11 (spot check of the load checks) | no change | - |

### Second review (three remaining MAJOR findings and accepted minors)

| Finding | Disposition | Where |
|---|---|---|
| R2-MAJOR-1: the R0 "STRICT READ-ONLY" definition contradicts what R0 does (a `ReadDwgFile`'d side database in memory; the host may evaluate, cache or write in memory); and the Architect asserted that the G6 facts "pass all five" boundary questions | **APPLIED and OPEN.** STRICT is redefined to be consistent with R0: no API write call, no save, no write to or write-open of a product or open-document database; the only database is an in-memory side database filled by `ReadDwgFile` from a private copy, never saved or attached. The in-memory residual is named the **R0 residual** and put as the explicit question **Q-O-0** to the Coordinator or the Owner. The claim that HF-G1 (M1) and HF-G3 "pass all five" is **removed**: question 3 is not asserted as an Architect fact, and the G6 reads proceed only on the Coordinator's or the Owner's confirmation of Q-O-0. Nothing is weakened: the reads stay blocked, not widened. The same residual is flagged for HF-C1, HF-C3 and HF-T2 | 0, 3.4 (HF-G1, HF-G3), 3.5 (question 3, applied paragraph), 4.1, 5.1, 5.3 item 4, 6 (Q-O-0, R-A, R-G), 7, 8 |
| R2-MAJOR-2: the group-code -> variable-name designation (HF-C5 read part) cannot be derived from the HF-C3 pairs `(code, host value type)` | **APPLIED** (declared NOT_OBSERVABLE) and **OPEN** (the optional second source). The HF-C3 pairs carry no variable name, so the earlier "derived from HF-C3" is wrong and is corrected in HF-C5, the 3.5 text, the I-2 row (no `designation[]` from I-2) and R-D. The designation is observable only by the HF-C4 write probe (I-5), blocked on Q-O-1; `NOT_OBSERVABLE` is added to the fact vocabulary. A second read-only source (compare the override values with the dimension's own variable values) is **not adopted** because it is an inference, not an observation, and is put to the Coordinator as R-D(b); if admitted it is `CORROBORATING_ONLY`. The consequence if the probe cannot run (the designation stays unconfirmed; BA-05 decides how to carry it) is stated, and no BA-05 rule is invented | 3.1, 3.4 (HF-C3, HF-C5, HF-G4), 3.5, 4.1, 5.2, 6 (R-D), 7, 8 |
| R2-MAJOR-3: I-7 versus the rule that the whole declared instrument set is final and in the tuple before H1 | **APPLIED.** Stated explicitly: **I-7 is not in the declared set for H1** (R-N(b)). Therefore the HF-F1 and HF-F2 conformance reads are not in H1 and run in a later session under its own declared SESSION_BUILD_TUPLE, with I-7 declared in it before that run and no transfer of evidence either way (Owner's R8 reading). The I-7 scope is an explicit R-N(b) item, referenced from R-G, 3.3, 3.4 (HF-F1, HF-F2), 5.1, 5.2 and section 7. The placement of the `FX-ANN-BLANK` step-1 construction is left to package v2 | 3.3, 3.4, 5.1, 5.2, 6 (R-G, R-N), 7 |
| R2-MINOR-1: ceiling formula and `E_max` definition missing from R-M and the pre-registered items | **APPLIED.** `tau <= TOL_LENGTH / E_max` and the definition of `E_max` are in R-M and in the pre-registration paragraph. No number is chosen | 2.3, 6 (R-M) |
| R2-MINOR-2: routing `scaleX/Y/Z` to `EXACT_DOUBLE` changes a comparator class | **APPLIED.** Stated in R1, the pre-registration paragraph and R-M: R1(b) needs the Coordinator's ruling before the host run; until then only R1(a) is available. The fail-closed direction is unchanged | 2.3, 6 (R-M) |
| R2-MINOR-3: PF-1/PF-2 duplicate triple not noted in the row count | **APPLIED.** 147 rows, 146 distinct triples (and 135 in-scope rows, 134 distinct triples); no statistic changes | 2.4 |

Checks of the revision: every JSON block parses; the revised JSON Schema was exercised with a hand-written validator of the keyword subset it uses (the `jsonschema` package is not installed and was not downloaded) on a generated 147-row sample and on negative cases; the Markdown table column counts were checked; the file has LF line endings only. Checks of revision 2: the Markdown table column counts were rechecked for the rows touched (HF-C5 has 11 cells; the Q-O-0 row has 3); the file still has LF line endings only.

### Third review (two MAJOR findings and accepted minors)

| Finding | Disposition | Where |
|---|---|---|
| R3-MAJOR-1: the Q-O-0 blocking list is inconsistent (HF-G3 gated in some places, HF-C1, HF-C3 and HF-T2 not flagged, the R-G phase 1 silent on I-2) | **APPLIED.** One explicit gated set, decided on the merits: HF-G3 uses no side database and is untouched by the R0 residual, so it is **removed** from the Q-O-0 gate everywhere; the side-database R0 reads HF-G1 (M1), HF-C1, HF-C3, HF-T2 and the I-2 R0 build that carries them are gated, and the matrix rows HF-C1, HF-C3 and HF-T2 carry the same Q-O-0(a) flag as HF-G1. The same list is repeated in the header, 3.4, 3.5, 4.1, 5.1, 5.2, 5.3 item 4, section 6 (Q-O-0, R-A, R-G), 7 and 8; R-G phase 1 says that the I-2 R0 build cannot start before Q-O-0(a). The sequencing consequence is stated once (Q-O-0 row): no host activity starts and no tuple record is closed until the whole declared set is final, H1 waits on Q-O-1 while the RS DLL is declared, and if Q-O-1 = STRICT only the RS DLL is struck from the set. No fail-closed behaviour is weakened and no authorization is widened: the gated set stays blocked, HF-G3 stays subject to the same Owner G6 and to the sequencing rule | header, 3.4, 3.5, 4.1, 5.1, 5.2, 5.3, 6, 7, 8 |
| R3-MAJOR-2: the STRICT / R0 definition is silent about result files | **APPLIED and OPEN.** "Writes only new result files under the evidence folder through the single `EvidenceWriter`" is added to the STRICT / R0 definition (section 0, 5.1) and named as a disclosed element, sub-question **Q-O-0(b)**, distinct from the in-memory residual Q-O-0(a); the I-1 typed AutoLISP `open ... "w"` write is the same element (3.2, 5.2). Q-O-0(b) does not change the gated set; its consequence if the answer is no is stated in the Q-O-0 row | 0, 3.2, 5.1, 5.2, 5.3, 6 |
| R3-MINOR-1: Q-O-0 pointers on HF-C1, HF-C3, HF-T2 | **APPLIED** | 3.4 |
| R3-MINOR-2: HF-M4b basis | **APPLIED.** Tagged NEEDS-COORDINATOR, citing the "configuration inputs" phrase of Owner Q-O5 as written in the Required-by cell | 3.4 |
| R3-MINOR-3: authority of the margin rule and the ceiling | **APPLIED.** Stated once after the opening of 2.3: Architect design proposals without repository authority; `TOL_LENGTH = 1e-9` sourced from BA-08 V7 section 10 and `Vector2D.cs:93` | 2.3 |
| R3-MINOR-4: G-07 table escaping | **APPLIED.** The verbatim row is given cell by cell in code spans, with no pipe character | 1 (G-07) |
| R3-MINOR-5: I-7 `conformance/` row | **APPLIED.** Pointer: outside the H1 declared set (R-N(b)) | 5.1 |

Checks of revision 3: Q-O-0, HF-G3, HF-C1, HF-C3, HF-T2 and EvidenceWriter were grepped over the whole file for consistency; the Markdown table rows touched keep their cell counts; the file has LF line endings only.
