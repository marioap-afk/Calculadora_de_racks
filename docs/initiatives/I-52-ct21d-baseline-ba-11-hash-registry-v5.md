# I-52 — CT-21D Baseline Artifact BA-11: Baseline Hash Registry and Sealing Workflow (DRAFT V5)

> **BASELINE ARTIFACT BA-11 V5 — DRAFT. It binds nothing yet: no artifact is a candidate, none is sealed, and no seal hash exists.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-HASH-REGISTRY
> ARTIFACT_VERSION          = 5-DRAFT   (supersedes 4-DRAFT, blob ff7ddc6cee102a183772722f10ecd4af82c04d5e, which stays as history)
> AUTHORITY_CONTRACT        = CT-21D V2.1 + V2.2 + V2.3 + V2.4 (draft revision 3, textually verified, decisions section 218)
>                             + V2.5 (draft revision 2, pending Coordinator textual verification)
> DELTA RULING APPLIED      = decisions section 217: AR4-17 (edges, GROUPS_SERVED definition, version-neutral sections 1-2.1, path form,
>                             precedence citations, check binding), AR4-18 (V2.5 revision 2 in section 4), AR4-22 (rule-5 agreement),
>                             AR4-06 and AR4-07 (pin sources), AR4-13 (pin rules), AR4-19 (BA-02a, BA-03)
> VERSION LABELS            = in sections 1, 2 and 2.1 an artifact is cited by its registry entry; a version label elsewhere in this file is
>                             frozen, non-authoritative text and denotes the version the entry of section 3 registers (AR2-44, AR4-17)
> BASE ITEM                 = BASE-11 (baseline hashes recorded): NOT MET
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 1. Hash procedure (normative)

| Aspect | Rule |
|---|---|
| file hash | SHA-256 of the file bytes after CRLF is normalized to LF, lowercase hexadecimal |
| path form | a path is **repository-relative**, with `/` separators, encoded as UTF-8 bytes, exactly as the Paths column of section 3 spells it |
| artifact hash, one file | the file hash |
| artifact hash, several files | SHA-256 of the concatenation, in ascending ordinal (UTF-8 byte) order of the paths, of `<path>` LF `<file hash>` LF |
| contract composite hash | SHA-256 over the LF-normalized texts of the blobs of the effective contract documents (V2.1, then each errata document in version order), then **one LF**, then the V5 blob id written as 40 lowercase ASCII hexadecimal characters with **no** trailing LF. It is the byte-level reading of the "plus the V5 blob id" approach of V2.2 0.1 / note N-1 of V2.2, extended by V2.3 section 0 / D-1 of V2.3. V2.4 section 0 and D-1, and V2.5 section 0 and D-1, cite this row for the procedure. Precedence of the *Contract hash* rows: V2.4's row governs over V2.2 and V2.3 by AR2-03; V2.5's row governs over V2.4's by V2.5 section 0 (AR3-13) |
| identity of the composite row (AR3-25, AR4-17) | the row above is **fixed by the blob of the BA-11 version that the BA-01 candidate ruling cites** (the same blob as the rule-5 agreement), as the procedure that D-1 of V2.4 and V2.5 section 0 and D-1 cite. A change to it is a change of the contract identity procedure and requires a **new contract version**, which cascades to BA-01 and to every dependent entry. This is the **single declared exception** to the acyclicity of section 2.1 |
| `SEALED_AT` of a scenario (V2.1 26.1; AR3-25) | SHA-256 of the canonical serialization of the scenario's expectation fields, as section 1 of the BA-04 version registered in section 3 defines them. It is computed and written into the catalog **before** the BA-04 candidate ruling, so it is inside the candidate bytes. The catalog is sealed **only at artifact level**; there is no per-scenario seal |
| `CURRENT_HASH` column | the artifact hash of the bytes at the commit of this version, **informational only**; it is not a candidate and not a seal |

## 2. Transitions and authority (normative)

```text
DRAFT  ->  CANDIDATE_FOR_SEALING  ->  RATIFIED  ->  SEALED
```

| Transition | Required | Recorded |
|---|---|---|
| `DRAFT -> CANDIDATE_FOR_SEALING` | an **Architect ruling on the exact bytes** of the artifact; every entry of its `DEPENDS_ON` at least `CANDIDATE_FOR_SEALING`; every `PREREQUISITES` item whose gate is *before candidate* met and its record cited | `CANDIDATE_HASH` = the artifact hash of those bytes, and the ruling reference, **in this registry** (never in the artifact) |
| `CANDIDATE_FOR_SEALING -> RATIFIED` | a **Coordinator** ratification and an **Architect** ratification, **each citing the exact `CANDIDATE_HASH`**; for BA-03 also the seal conditions of V2.2 PA-14: its seal-time re-run is made after the current remote identities are fetched and compared (AR4-19), writes its output **outside** the artifact paths, and the ratification records cite that output's exit status, `identityDrift` and `failed` (a non-zero exit, any drift or any failure means BA-03 is re-prepared); for BA-02a the dated re-run of its byte-level comparison (AR3-29); all recorded in the ratification records, **never in the artifact** | both ratification records (location and fields: sections 2 and 5.2 of the BA-07 version registered in section 3), cited here |
| `RATIFIED -> SEALED` | the artifact hash recomputed on the committed bytes equals the `CANDIDATE_HASH`; every entry of its `DEPENDS_ON` is `SEALED` (their `SEAL_HASH` cited); every `PREREQUISITES` item whose gate is *before seal* met and its record cited | `SEAL_HASH` (normative) = `CANDIDATE_HASH` |
| any byte change after a candidate ruling | the candidate and any ratification **expire**; the artifact returns to `DRAFT` as a new version | the expired hash stays in the history |
| **cascade** | a new version of an entry that is **at least `CANDIDATE_FOR_SEALING`** (`CANDIDATE_FOR_SEALING`, `RATIFIED` or `SEALED`) **expires the candidates, the ratifications and the seals of every entry that depends on it** (directly or transitively) | the expiry is recorded here |
| change of a sealed artifact | a **new version** (new entry, new candidate, new ratifications, new seal); the old seal is retained and marked `SUPERSEDED` | both entries |

**Authority rules (AR2-44, AR3-25, AR4-17).**

1. **This registry is the only authority** for the status and the hashes of every artifact entry of section 3. The status and hash text inside an artifact is **frozen, non-authoritative text** of its candidate bytes. An artifact never records its own candidate, ratifications or seal. The per-scenario `SEALED_AT` values of BA-04 are expectation hashes written before the candidate (section 1), not status.
2. **BA-11's own status and hashes live outside BA-11**: its status, its `CANDIDATE_HASH`, its two ratification records and its `SEAL_HASH` are established by its ratification records and by the custody entries `BASELINE_RECORDED`, `BASELINE_APPROVED` and `ANCHOR_RECORDED` of the BA-07 version registered in section 3 (its sections 8 and 9; anchoring is derived from `ANCHOR_RECORDED`). The BA-11 row of section 3 says so and stays constant.
3. **BA-01 is the contract**: its `CANDIDATE_HASH` and `SEAL_HASH` are the **contract composite hash** (section 4); it passes through the same transitions (the Coordinator's textual verification of V2.4 and V2.5 and the agreement of the effective contract precede its candidate ruling). `BASE-8` and `BASE-16` (criteria) are **`SATISFIED_BY_CONTRACT` at the BA-01 `SEAL_HASH`** and reopen with any new contract version.
4. **The final registry** is sealed **last**, after every other **artifact entry of section 3** is `SEALED` ("every other entry" means those entries only; the pin slots of section 3.1 are not entries). Its seal hash enters the custody genesis entry and the remote anchor; any later change is a new registry version with a new anchor.
5. **Sealing workflow agreement** (AR3-25, AR4-22): before the **first** candidate ruling, the Architect agrees sections 1, 2 and 2.1 of this registry as the governing sealing workflow, on the exact bytes of the BA-11 version that the agreement cites; the agreement record cites the blob, the SHA-256 of the LF-normalized text of sections 1, 2 and 2.1, and the SHA-256 of the graph check and of its output. A later change to those sections **reopens every candidate issued under them**. Inscriptions in section 3 (statuses, hashes, ratification references) are updates outside the agreed sections.

A candidate hash is **never normative**; only a `SEAL_HASH` is.

### 2.1 `DEPENDS_ON`, `PREREQUISITES` and the seal order (normative)

`DEPENDS_ON` lists **content dependencies between entries of this registry only**: entry `X` depends on `Y` when a normative value of `X` is defined in `Y`. A mention of a name defined elsewhere (a fixture name, a scenario id) or an informative pointer is not a dependency. `DEPENDS_ON` is governed by the gating and cascade rules of section 2. `PREREQUISITES` lists **external inputs** (host observations, Owner and Coordinator decisions, Architect rulings, construction work); each names its **gate** (*before candidate* or *before seal*) and the record that proves it; a prerequisite has no registry state.

Complete edge list (`X -> Y` means `X` depends on `Y`):

```text
BA-02a -> BA-01
BA-02b -> BA-01
BA-02b -> BA-02a
BA-02b -> BA-04
BA-03 -> BA-01
BA-04 -> BA-01
BA-04 -> BA-02a
BA-04 -> BA-03
BA-04 -> BA-05
BA-04 -> BA-06
BA-04 -> BA-07
BA-04 -> BA-08
BA-04 -> BA-09
BA-04 -> BA-10
BA-05 -> BA-01
BA-06 -> BA-01
BA-06 -> BA-03
BA-06 -> BA-05
BA-06 -> BA-07
BA-07 -> BA-01
BA-08 -> BA-01
BA-08 -> BA-03
BA-08 -> BA-05
BA-08 -> BA-06
BA-09 -> BA-01
BA-09 -> BA-06
BA-09 -> BA-07
BA-09 -> BA-08
BA-10 -> BA-01
BA-11 -> BA-01
BA-11 -> BA-02a
BA-11 -> BA-02b
BA-11 -> BA-03
BA-11 -> BA-04
BA-11 -> BA-05
BA-11 -> BA-06
BA-11 -> BA-07
BA-11 -> BA-08
BA-11 -> BA-09
BA-11 -> BA-10
```

The graph is **acyclic**, with the single declared exception of section 1 (the composite row, fixed by blob). **Seal order** (a topological order of the graph):

```text
BA-01  ->  BA-02a, BA-03, BA-05, BA-07, BA-10  ->  BA-06  ->  BA-08  ->  BA-09  ->  BA-04  ->  BA-02b  ->  BA-11 (last)
```

*Informative (not part of the agreed text).* The graph is checked mechanically by `docs/automation/evidence/I-52-ct21d-ba11-v5-graph-check.py` (it takes the registry path as an argument; output `docs/automation/evidence/I-52-ct21d-ba11-v5-graph-check.json`): no cycle, a topological seal order, section 3 cells equal to this list, each artifact header's `DEPENDS_ON` line equal to its cell, and the forbidden edges BA-05 -> BA-08, BA-07 -> BA-05, BA-10 -> BA-04 and BA-04 -> BA-02b absent. The rationale: BA-05 writes its semantic vectors in slot units and takes the census universe from the library file (AR3-17); BA-07 defines its own canonical serialization (AR3-23); BA-10 is symbolic (AR3-24); `GROUPS_SERVED` and `CONTRACT_ASSIGNED_GROUPS` are defined in BA-04 section 1 and BA-02b applies that definition (AR4-17).

## 3. Registry entries

| Entry | ARTIFACT_ID | Paths | STATUS | CURRENT_HASH (informational) | CANDIDATE_HASH | COORDINATOR | ARCHITECT | SEAL_HASH | DEPENDS_ON | PREREQUISITES (gate) | BASE items | Blocker |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| BA-01 | authority contract `CT-21D-V2.5` | the five contract documents (section 4) | DRAFT (V2.1..V2.3 agreed for preparation; V2.4 revision 3 textually verified; V2.5 pending textual verification) | composite in section 4 | UNSET | UNSET | UNSET | UNSET | none | Coordinator textual verification of V2.5 and agreement of the effective contract; the rule-5 agreement on the BA-11 blob that the candidate ruling cites (before candidate) | BASE-1, BASE-8, BASE-16 (criteria) | none |
| BA-02a | `CT21D-BASE-CONTROL-REGISTRY` | `docs/initiatives/I-52-ct21d-baseline-ba-02a-control-registry-v3.md` | DRAFT | `acb5e01de3c1b722e9757bfb26aafb6a707599b37dd2902e4c64676c1bd2e3c3` | UNSET | UNSET | UNSET | UNSET | BA-01 | the dated re-run of its byte-level comparison with the contract (AR3-29; recorded in the ratification records, before seal) | BASE-19 | none |
| BA-03 | `SELECTIVE_VIEW_FAMILY_SCOPE_V1` | `docs/initiatives/I-52-ct21d-baseline-ba-03-selective-scope-v4.md`<br>`docs/automation/evidence/I-52-ct21d-ba03-v4-checks.json`<br>`docs/automation/evidence/I-52-ct21d-ba03-v4-checks.py`<br>`docs/automation/evidence/I-52-ct21d-ba03-v4-checks-output.json` | DRAFT | `fbbb5f4c688abecedaea6747c00234b4bb786849e66b9c6cd0e6cf7234399c4f` | UNSET | UNSET | UNSET | UNSET | BA-01 | V2.2 PA-14 conditions 3 and 4: the seal-time re-run after the current remote identities are fetched and compared, and the post-I-57 reading (AR4-19; ratification records, before seal) | BASE-12 | NB-1 |
| BA-05 | `CT21D-BASE-FINGERPRINT-SPEC (FPSPEC-V1)` | `docs/initiatives/I-52-ct21d-baseline-ba-05-fingerprint-specification-v5.md`<br>`docs/automation/evidence/I-52-ct21d-fpspec-v1-vectors-v5.gen.py`<br>`docs/automation/evidence/I-52-ct21d-fpspec-v1-vectors-v5.json` | DRAFT | `89736f4fbff4bc276c028626a40e866e2c2f64fcc38dd3726ed1404b00f5e088` | UNSET | UNSET | UNSET | UNSET | BA-01 | the library entity-type census over every block of the library file and the AR4-10 confirmations by the census and the I-12 qualification on the exact build (host, K-2; section 3.3; before candidate) | BASE-14 | NB-3 |
| BA-07 | `CT21D-BASE-MANIFEST-CUSTODY` | `docs/initiatives/I-52-ct21d-baseline-ba-07-manifest-custody-v5.md`<br>`docs/automation/evidence/I-52-ct21d-ba07-v5-jcs-vector.json`<br>`docs/automation/evidence/I-52-ct21d-ba07-v5-jcs-vector.py` | DRAFT | `9e3ba450a7795bfcf23964cf15443d4e748f34b8b0cc59a515b445a49d8bc151` | UNSET | UNSET | UNSET | UNSET | BA-01 | the Architect's rulings on AQ-V5-04 and AQ-V5-01 (before candidate); the Coordinator's designation of the independent copy (location and holder) in the BA-07 candidate or ratification record (before seal) | BASE-17 | NB-6 |
| BA-10 | `CT21D-BASE-PARAMETER-SHEET` | `docs/initiatives/I-52-ct21d-baseline-ba-10-parameter-sheet-v5.md` | DRAFT | `96b3aceb846427d9c00b0c06275e54e8df3feaa5aaa00d2d5136e29200b24275` | UNSET | UNSET | UNSET | UNSET | BA-01 | Owner Q-O1..Q-O5 (Q-O5 by the Owner or the CAD manager the Owner designates); Coordinator PARAM-04, EVIDENCE_REPETITION and the recording of N_A, N_B, N_det and n (WU_DRY_RUNS = N_B); the machine-attribute observation (section 3.3); the Architect's rulings on AQ-V5-06, AQ-V5-01, AQ-V5-02 and AQ-V5-05 (all before candidate: the values enter the bytes) | BASE-9, BASE-18 | none of its own |
| BA-06 | `CT21D-BASE-EVM-PREPARATION` | `docs/initiatives/I-52-ct21d-baseline-ba-06-evm-preparation-v5.md` | DRAFT | `6f7eaf5b0fce07c13f5e0f32698f3b8c0d5d7eeab75f31bf8eff713c71d2e934` | UNSET | UNSET | UNSET | UNSET | BA-01, BA-03, BA-05, BA-07 | the Architect's exact delta check confirming the carry-over of STATIC_REVIEWED (AR4-12) and the rulings on AQ-V5-03, AQ-V5-07, AQ-V5-04 and AQ-V5-01 (before candidate) | BASE-15, BASE-5 | NB-4 |
| BA-08 | `SELECTIVE_KIND_AUTHORITY_BASELINE` | `docs/initiatives/I-52-ct21d-baseline-ba-08-selective-kind-baseline-v5.md` | DRAFT | `9cd61e640c07845e2a024ba2222dfe3b5c87c9dd5de5bea81fcd90dcce442964` | UNSET | UNSET | UNSET | UNSET | BA-01, BA-03, BA-05, BA-06 | K-3 (TOL_SCALE decision, test and record; section 3.3), K-8 (FX-IMP and its variants) and the Architect's rulings on AQ-V5-05 and AQ-V5-02 (before candidate); K-2 (library census, in effect met before the candidate through BA-05) and K-5 (Coordinator ratification of route R-1 with the AR3-14 conditions) (before seal) | BASE-2, BASE-3, BASE-4, BASE-6, BASE-7, BASE-10, BASE-18 (tolerances) | none of its own |
| BA-09 | `CT21D-BASE-WARMUP` | `docs/initiatives/I-52-ct21d-baseline-ba-09-warmup-v5.md` | DRAFT | `ff34c372a69e63ca1dbba77a9f40515e436deb465875ea4b8926936a4e3fd28c` | UNSET | UNSET | UNSET | UNSET | BA-01, BA-06, BA-07, BA-08 | W-1 (the recorded synthetic warm-up plan data; its file becomes a path of this entry when produced) and the Architect's ruling on AQ-V5-02 for WU-BOUNDARY (both before candidate) | BASE-5, BASE-10 | none of its own |
| BA-04 | `CT21D-BASE-SCENARIO-CATALOG` | `docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v5.json`<br>`docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v5.md` | DRAFT | `252d984909899b89df7d77824d3f7bab3f4ebea3ce44fc74af291cbf55b2373e` | UNSET | UNSET | UNSET | UNSET | BA-01, BA-02a, BA-03, BA-05, BA-06, BA-07, BA-08, BA-09, BA-10 | K-3 (declaration of the S comparisons) and K-8, and the Architect's rulings on AQ-V5-01, AQ-V5-02, AQ-V5-04, AQ-V5-06 and AQ-V5-08 (before candidate: the declarations enter the bytes); fixture instances, HOST_DEFAULT_MAP and the other pins are fixed before the first governing run, not prerequisites | BASE-13, BASE-16 (scenarios) | NB-2 |
| BA-02b | `CT21D-BASE-COVERAGE-MATRIX` | `docs/initiatives/I-52-ct21d-baseline-ba-02b-coverage-matrix-v5.md`<br>`docs/automation/evidence/I-52-ct21d-ba04-v5-checks.py`<br>`docs/automation/evidence/I-52-ct21d-ba04-v5-checks-output.json` | DRAFT | `04b11c6188ffdd44b1b6209f996d23e32e93e337070b7ebc72d2db3e9ff1fa65` | UNSET | UNSET | UNSET | UNSET | BA-01, BA-02a, BA-04 | none beyond its DEPENDS_ON; the open item AQ-V5-02 of the BA-04 entry can change its tables through BA-04 (before candidate) | BASE-20 | NB-2 |
| BA-11 | `CT21D-BASE-HASH-REGISTRY` (this file) | this file | recorded outside (rule 2) | recorded outside | recorded outside | recorded outside | recorded outside | recorded outside | every other artifact entry of this section | the Architect's agreement of sections 1, 2 and 2.1 (rule 5; before the first candidate ruling of any entry) | BASE-11 | none of its own |

### 3.1 Pin slots (sealed with BA-11; AR2-16, AR2-17, AR3-23, AR4-13)

This table is a **slot table sealed with BA-11**: it fixes the slot names and the rule for where their values are recorded. A pin is a named slot in the sealed catalog (the BA-04 entry, section 2.1). **After the baseline, a pin's value, state and record hash are authoritative only in its pin record and in the BA-07 custody log; they are never written into BA-11.** The pin's own seal is the pair of ratification records, **one COORDINATOR and one ARCHITECT**, decision `RATIFIED`, `subjectKind PIN`, `subject` = the slot, `subjectHash` = the hash of the pin record, cited by its `PIN_RECORDED` entry. State machine `RECORDED -> RATIFIED -> ANCHORED` (`RECORDED` = the pin record, with no custody entry of its own; `ANCHORED` derived from a later `ANCHOR_RECORDED`). The pin record names the **baseline version in force**; a pin is usable only under that version; **a slot has at most one live pin**; carrying a pin to a new baseline version is a new pin version with its own ratifications and `PIN_RECORDED`, and a pin not carried over is unusable (fail-closed, AR4-13). A pin is usable by a governing run only when its `PIN_RECORDED` entry is covered by an anchor recorded before the run starts, and the run lists the pin-record hashes it consumes. Writing a pin never changes the bytes of the catalog or of this registry.

| Pin slot | Value | Source | Where the value and state live |
|---|---|---|---|
| `E04_ADMITTED_SET@R-1` | the E-04 admitted set of the designated route | E4-LEARN-R1-* under the selected `LOCK_MODE` (AR2-11; characterization build, AR3-01) | its pin record and the BA-07 custody log (`PIN_RECORDED` citing the ratification pair; state `RECORDED -> RATIFIED -> ANCHORED`) |
| `LOCK_MODE` | the selected lock mode | the selection review of V2.1 3.4 over LK-* (characterization build, AR3-01) | its pin record and the BA-07 custody log (`PIN_RECORDED` citing the ratification pair; state `RECORDED -> RATIFIED -> ANCHORED`) |
| `EVM_FROZEN` | the frozen event model | learning `EV-L-*` on the product build after `REVIEWED` (V2.4; V2.1 27.2; AR4-01); the pin record cites the EVM field `INVENTORY_REVIEW_RECORDS` (AR3-22) | its pin record and the BA-07 custody log (`PIN_RECORDED` citing the ratification pair; state `RECORDED -> RATIFIED -> ANCHORED`) |
| `HOST_DEFAULT_MAP` | the host-default attribute map (rule of the pin in the BA-08 entry) | only the non-governing characterization scenarios `HDM-CHAR-FX1F`, `HDM-CHAR-FXANN`, `HDM-CHAR-FXDIM` and `HDM-CHAR-FXANNBLANK` (AR3-11, AR4-07); varied-key entries are informative (AR4-14) | its pin record and the BA-07 custody log (`PIN_RECORDED` citing the ratification pair; state `RECORDED -> RATIFIED -> ANCHORED`) |
| `FIXTURE_INSTANCE@<FX>` | the hash of each fixture instance (template drawing and library copy); `<FX>` ranges over the fixtures and variants of the BA-08 entry (section 12) and the variant libraries `L-VAR-*` the BA-04 entry uses | host work under a future gate, with conformance to the BA-08 entry recorded | its pin record and the BA-07 custody log (`PIN_RECORDED` citing the ratification pair; state `RECORDED -> RATIFIED -> ANCHORED`) |

`TOL_SCALE` is **not** a pin: it is a value of the Selective kind baseline (V2.2 PA-9, the BA-08 entry, section 10) without authority today (K-3).

### 3.2 Bound items that are not artifacts of this registry

| Item | Kind | Status |
|---|---|---|
| block-library file (path, SHA-256) | tuple field (BUILD_BOUND, V2.1 2.1) | UNSET: fixed per build under test at execution preparation; the file is not versioned in the repository |
| catalog folder (`CATALOG_FOLDER`) | tuple field (BUILD_BOUND) added by **V2.5** (AR3-13), sampled for the build under test of each run | UNSET |
| build profiles (one admissible BUILD_BOUND profile per `BUILD_UNDER_TEST`: product build and characterization build; AR4-06) | tuple | UNSET: fixed at execution preparation, with the recorded build equivalence of AR4-06 (4) |
| library entity-type census (universe: every block of the library file) | prerequisite of BA-05 (before candidate) and of BA-08 (before seal) | NOT PERFORMED (host; not authorized) |
| machine-attribute observation (`PARAM-05` of the BA-10 entry) | prerequisite of BA-10 (before candidate: the value enters the bytes) | NOT PERFORMED (host; not authorized) |
| custody log | created at the baseline by its genesis entry (the BA-07 entry, section 8) | NOT CREATED |
| manifest (one per build under test) | execution prerequisite EXEC-11 | NOT CAPTURED |
| baseline anchor (`ANCHOR_RECORDED`) | custody | NOT CREATED (created only after every artifact entry is sealed) |

### 3.3 Plan of a future non-governing host gate (BASE-18; AR3-25)

`BASELINE_READY` cannot be reached without host work that is **not authorized** today. The items below are recorded as the **plan** of one future **non-governing** host gate; this registry authorizes none of them, and each needs an explicit authorization.

| Item | Needed by | What it produces |
|---|---|---|
| machine-attribute observation (read-only, no instrument, no manifest) | the BA-10 entry (`PARAM-05` label); Owner Q-O5 | the attribute values of the machine class |
| `TOL_SCALE` test and record (K-3) | the BA-08 entry (section 10); the BA-04 entry (declaration of the S comparisons) | the evidence for the Architect's `TOL_SCALE` decision |
| library entity-type census (K-2), with the AR4-10 confirmations (dimension-override storage, order and equal-to-style behaviour) | the BA-05 entry (section 5); the BA-08 entry | the census record over every block of the library file |
| fixture instances and the `FX-IMP` construction (K-8) | the BA-08 entry (section 12); the BA-04 entry (`BLOCKED_BY` K-8) | the instances and their conformance records (instance hashes become the `FIXTURE_INSTANCE@<FX>` pins) |

## 4. Contract identity

| Document | Git blob |
|---|---|
| `docs/initiatives/I-52-ct21d-authority-contract-v2.1.md` | `6259a3beb7445a63620636acd696cdad689965b3` |
| `docs/initiatives/I-52-ct21d-authority-contract-v2.2.md` | `fd7260c57ec05e11f6bbb9f67a728e7dfdf1f609` |
| `docs/initiatives/I-52-ct21d-authority-contract-v2.3.md` | `bc3d67a957b784a874b5bab97ce22116af1eed70` |
| `docs/initiatives/I-52-ct21d-authority-contract-v2.4.md` | `b99f89d209469362ef585508b2dfb2370f0db7d8` |
| `docs/initiatives/I-52-ct21d-authority-contract-v2.5.md` | `a28f0202f4fcf2225816bffa223fce94522b14f4` |
| Proposal ALT-21D V5 (guarantee authority) | `f73157d9ce2d5013f4f91fb2d4f4d127a4f879a1` |

```text
CANDIDATE_COMPOSITE_HASH (V2.1 + V2.2 + V2.3 + V2.4 draft revision 3 + V2.5 draft revision 2 + one LF + V5 blob id;
                          informational until the BA-01 candidate ruling)
  = e433cb13452f0d932d3981f5dceae627b6ea7f2c854f3d1f7fc82e86561a5da3
```

## 5. `BASE-1 .. BASE-20` after the phase-2 delta 3

| Base | Requirement | State |
|---|---|---|
| BASE-1 | the authority contract is complete and agreed | V2.1..V2.3 agreed for preparation; V2.4 revision 3 textually verified (decisions section 218); V2.5 revision 2 pending textual verification; BA-01 not a candidate |
| BASE-2 | the Selective kind authority design is complete and agreed | BA-08 draft; K-2, K-3, K-5, K-8 open; the open Architect items are listed in the BA-08 entry |
| BASE-3 | E-04 criteria fixed | criteria in the contract; route R-1 pending ratification (K-5) with the conditions of AR3-14; values are the pin `E04_ADMITTED_SET@R-1` |
| BASE-4 | `LOCK_MODE` candidate contract fixed | candidate set LM-1/LM-2 sufficient; selection is the pin `LOCK_MODE` |
| BASE-5 | event instrumentation designed | BA-06: **`STATIC_REVIEWED` approved** on BA-06 V4 with conditions (AR4-12), carry-over to BA-06 V5 subject to the Architect's exact delta check; BA-09 (WU-Q1 ruled by AR3-16) |
| BASE-6 | abort read-set fixed | specification in the BA-08 entry (section 6); E-08 compares the REFERENCE and CONTEXT_VARIABLE members (AR4-08); the MISSING readers are execution blockers (K-7) |
| BASE-7 | composition and reuse rules fixed | rules in the BA-08 entry; library census (K-2) pending |
| BASE-8 | UNDO evidence plan fixed | **SATISFIED_BY_CONTRACT at the BA-01 `SEAL_HASH`** (V2.1 16); pending that seal |
| BASE-9 | governing-result, retry and attempt-log policies fixed | policies in the contract; `MAX_ATTEMPTS` per repetition slot (AR3-24) in BASE-18 |
| BASE-10 | every `BEFORE_CT21D_BASELINE` item resolved | BA-08 and BA-09 drafts |
| BASE-11 | baseline hashes recorded | this registry; nothing is a candidate |
| BASE-12 | OD-1 closed (sealed Selective scope) | BA-03 V4 ready in its bytes for candidate (AR4-19) |
| BASE-13 | scenario catalog complete with expected results and authority | BA-04 draft; K-3, K-8 and the open Architect items it lists block its candidate |
| BASE-14 | fingerprint specification agreed | BA-05 draft; census and the AR4-10 confirmations pending (host; before candidate) |
| BASE-15 | E-12 procedure and corpus definition agreed | BA-06 draft; `STATIC_REVIEWED` approved (AR4-12) |
| BASE-16 | instrument qualification acceptance criteria agreed | criteria **SATISFIED_BY_CONTRACT at the BA-01 `SEAL_HASH`**; the `Q-*` scenarios (per build under test, AR4-06) are sealed with the catalog (BASE-13) |
| BASE-17 | manifest capture and custody procedure agreed | BA-07 draft (AR4-13 applied); the independent-copy designation is recorded in its candidate or ratification record |
| BASE-18 | classified parameters fixed | BA-10 all `UNSET`. Needs: **Owner** Q-O1..Q-O5 (Q-O2 also fixes `WU_DRY_RUNS` = `N_B`); **Coordinator** `PARAM-04` cap and `EVIDENCE_REPETITION`; the **machine-attribute observation** (section 3.3); the **BA-08 tolerances with the `TOL_SCALE` decision, test and record** (K-3, section 3.3) |
| BASE-19 | control-class registry agreed | BA-02a V3 (unchanged) ready in its bytes for candidate (AR4-19) |
| BASE-20 | coverage matrix agreed | BA-02b draft; depends on the catalog |

```text
BASE-11 = NOT MET     SEALED ENTRIES = 0     CANDIDATE ENTRIES = 0
CT21D_AUTHORITY_BASELINE_READY = FALSE
```

## 6. Delta V5 (decisions section 217)

| Finding | Decision | Fix |
|---|---|---|
| R1:BA11V4-01, R2:BA11V4-01, CLOSURE BA11-F3 | AR4-17 | edges BA-02b -> BA-01 and BA-04 -> BA-07 added; `GROUPS_SERVED` defined in BA-04 (no BA-04 -> BA-02b edge); the graph check also compares each artifact header's `DEPENDS_ON` line |
| R1:BA11V4-02 | AR4-17 | section 3.2 gates corrected (census: BA-05 before candidate, BA-08 before seal; machine attributes: BA-10 before candidate); section 3 rows aligned with the artifacts' own headers |
| R2:BA11V4-02, R1:BA11V4-03 | AR4-17 | sections 1, 2 and 2.1 cite registry entries, not draft versions; version labels elsewhere are frozen, non-authoritative text (header); path form stated; precedence citations (AR2-03 for V2.4, V2.5 section 0 for V2.5); V2.5 section 0 and D-1 named in the identity row; the 2.1 rationale and the check are informative |
| R1:BA11V4-04 | AR4-17 | the BA-04 check and its output are paths of the BA-02b entry; the graph check is bound in the rule-5 agreement record |
| R1:BA11V4-05 | — | HANDOFF restored the reconciliation statement (decisions section 217) |
| V24R3-02 | AR4-18 | composite-row precedence citation corrected |
| AR4-18 | V2.5 revision 2 | section 4 regenerated |
| AR4-06, AR4-07, AR4-13 | pins and bound items | section 3.1 (pin rules, four HDM-CHAR sources, product-build EVM learning) and 3.2 (one build profile per build under test; one manifest per build) |
| other confirmed minors of both passes | AR4-20 | applied in the rows above where they concern BA-11 |
