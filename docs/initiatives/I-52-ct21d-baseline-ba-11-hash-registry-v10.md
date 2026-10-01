# I-52 — CT-21D Baseline Artifact BA-11: Baseline Hash Registry and Sealing Workflow (DRAFT V10)

> **BASELINE ARTIFACT BA-11 V10 — DRAFT. Four entries are candidates (BA-01, BA-02a, BA-03: decisions section 231; BA-07: section 235); none is ratified or sealed, and no seal hash exists.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-HASH-REGISTRY
> ARTIFACT_VERSION          = 10-DRAFT   (supersedes 9-DRAFT, blob 6d6212a4bbddc8cb7c2ef3cbde7a56a16822f515, which stays as history)
> AUTHORITY_CONTRACT        = CT-21D V2.1 + V2.2 + V2.3 + V2.4 (draft revision 3, textually verified, decisions section 218)
>                             + V2.5 (draft revision 2, textually verified, section 221) + V2.6 (draft revision 3, pre-approved by
>                             AR8-01 and textually verified, section 230): effective contract CT-21D-V2.6 AGREED
> BOUND_PRODUCT_SHA         = 95690c28dc6268e61dff32a0cbc33cc9fde3d47f (AR6-01; a bound item of section 3.2, never a tuple value)
> DELTA RULING APPLIED      = decisions section 217: AR4-17 (edges, GROUPS_SERVED definition, version-neutral sections 1-2.1, path form,
>                             precedence citations, check binding), AR4-18 (V2.5 revision 2 in section 4), AR4-22 (rule-5 agreement),
>                             AR4-06 and AR4-07 (pin sources), AR4-13 (pin rules), AR4-19 (BA-02a, BA-03); V6: decisions sections 223
>                             and 224 (AR6-01 bound SHA, AR6-02 fixture construction build, AR6-05/AR6-06 V2.6 and rule 5), AR5-12
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
| contract composite hash | SHA-256 over the LF-normalized texts of the blobs of the effective contract documents (V2.1, then each errata document in version order), then **one LF**, then the V5 blob id written as 40 lowercase ASCII hexadecimal characters with **no** trailing LF. It is the byte-level reading of the "plus the V5 blob id" approach of V2.2 0.1 / note N-1 of V2.2, extended by V2.3 section 0 / D-1 of V2.3. V2.4 section 0 and D-1, and V2.5 section 0 and D-1, cite this row for the procedure. Precedence of the *Contract hash* rows: V2.4's row governs over V2.2 and V2.3 by AR2-03; V2.5's row governs over V2.4's by V2.5 section 0 (AR3-13); V2.6's row governs over V2.5's by V2.6 section 0 (AR6-05, AR7-06); a later errata document's row governs over the earlier ones by its own section 0 |
| identity of the composite row (AR3-25, AR4-17) | the row above is **fixed by the blob of the BA-11 version that the BA-01 candidate ruling cites** (the same blob as the rule-5 agreement), as the procedure that D-1 of V2.4, V2.5 section 0 and D-1, and the section 0 of every later errata document cite. A change to it is a change of the contract identity procedure and requires a **new contract version**, which cascades to BA-01 and to every dependent entry. This is the **single declared exception** to the acyclicity of section 2.1 |
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
3. **BA-01 is the contract**: its `CANDIDATE_HASH` and `SEAL_HASH` are the **contract composite hash** (section 4); it passes through the same transitions (the Coordinator's textual verification of V2.4, V2.5, V2.6 and of every later errata document and the agreement of the effective contract precede its candidate ruling). `BASE-8` and `BASE-16` (criteria) are **`SATISFIED_BY_CONTRACT` at the BA-01 `SEAL_HASH`** and reopen with any new contract version.
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

*Informative (not part of the agreed text).* The graph is checked mechanically by `docs/automation/evidence/I-52-ct21d-ba11-v5-graph-check.py` (it takes the registry path as an argument; output `docs/automation/evidence/I-52-ct21d-ba11-v7-graph-check.json`): no cycle, a topological seal order, section 3 cells equal to this list, each artifact header's `DEPENDS_ON` line equal to its cell, and the forbidden edges BA-05 -> BA-08, BA-07 -> BA-05, BA-10 -> BA-04 and BA-04 -> BA-02b absent. The rationale: BA-05 writes its semantic vectors in slot units and takes the census universe from the library file (AR3-17); BA-07 defines its own canonical serialization (AR3-23); BA-10 is symbolic (AR3-24); `GROUPS_SERVED` and `CONTRACT_ASSIGNED_GROUPS` are defined in BA-04 section 1 and BA-02b applies that definition (AR4-17).

## 3. Registry entries

| Entry | ARTIFACT_ID | Paths | STATUS | CURRENT_HASH (informational) | CANDIDATE_HASH | COORDINATOR | ARCHITECT | SEAL_HASH | DEPENDS_ON | PREREQUISITES (gate) | BASE items | Blocker |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| BA-01 | authority contract `CT-21D-V2.6` | the six contract documents (section 4) | CANDIDATE_FOR_SEALING (AR9-02, decisions section 231; rule-5 BA-11 blob 9f5d36cf; effective contract agreed, section 230) | composite in section 4 | `97100cff11868807f3c49f25bf25d0385c86e8873cba0cf2c1996338b9b98eb4` | UNSET | UNSET | UNSET | none | met: AR8-01 pre-approval, section 230 textual verification and agreement at the bound SHA, AR9-01 rule-5 agreement (before candidate) | BASE-1, BASE-8, BASE-16 (criteria) | none |
| BA-02a | `CT21D-BASE-CONTROL-REGISTRY` | `docs/initiatives/I-52-ct21d-baseline-ba-02a-control-registry-v3.md` | CANDIDATE_FOR_SEALING (AR9-03, decisions sections 231 and 235; rule-5 BA-11 blob 9f5d36cf) | `acb5e01de3c1b722e9757bfb26aafb6a707599b37dd2902e4c64676c1bd2e3c3` | `acb5e01de3c1b722e9757bfb26aafb6a707599b37dd2902e4c64676c1bd2e3c3` | UNSET | UNSET | UNSET | BA-01 | the dated re-run of its byte-level comparison with the contract (AR3-29; recorded in the ratification records, before seal) | BASE-19 | none |
| BA-03 | `SELECTIVE_VIEW_FAMILY_SCOPE_V1` | `docs/initiatives/I-52-ct21d-baseline-ba-03-selective-scope-v5.md`<br>`docs/automation/evidence/I-52-ct21d-ba03-v5-checks.json`<br>`docs/automation/evidence/I-52-ct21d-ba03-v5-checks.py`<br>`docs/automation/evidence/I-52-ct21d-ba03-v5-checks-output.json` | CANDIDATE_FOR_SEALING (AR9-04, decisions sections 231 and 235; rule-5 BA-11 blob 9f5d36cf) | `0afb5d9d28e2bb7ad9ceb6bdbc407f34ff1e2514359feb3aaf4bb9c8ab0949e6` | `0afb5d9d28e2bb7ad9ceb6bdbc407f34ff1e2514359feb3aaf4bb9c8ab0949e6` | UNSET | UNSET | UNSET | BA-01 | V2.2 PA-14 conditions 3 and 4: the seal-time re-run after the current remote identities are fetched and compared, and the post-I-57 reading (AR4-19; ratification records, before seal). AQ-V6-01 is ruled (AR7-03) | BASE-12 | NB-1 |
| BA-05 | `CT21D-BASE-FINGERPRINT-SPEC (FPSPEC-V1)` | `docs/initiatives/I-52-ct21d-baseline-ba-05-fingerprint-specification-v5.md`<br>`docs/automation/evidence/I-52-ct21d-fpspec-v1-vectors-v5.gen.py`<br>`docs/automation/evidence/I-52-ct21d-fpspec-v1-vectors-v5.json` | DRAFT | `89736f4fbff4bc276c028626a40e866e2c2f64fcc38dd3726ed1404b00f5e088` | UNSET | UNSET | UNSET | UNSET | BA-01 | the library entity-type census over every block of the library file and the AR4-10 confirmations by the census and the I-12 qualification on the exact build (host, K-2; section 3.3; before candidate) | BASE-14 | NB-3 |
| BA-07 | `CT21D-BASE-MANIFEST-CUSTODY` | `docs/initiatives/I-52-ct21d-baseline-ba-07-manifest-custody-v8.md`<br>`docs/automation/evidence/I-52-ct21d-ba07-v5-jcs-vector.json`<br>`docs/automation/evidence/I-52-ct21d-ba07-v5-jcs-vector.py` | CANDIDATE_FOR_SEALING (AR10-02, decisions sections 231 and 235; rule-5 BA-11 blob 9f5d36cf) | `ecc2454923b5a3492c8b5144e22b403b9946f1afba114191a2efed88ea2dc0a8` | `ecc2454923b5a3492c8b5144e22b403b9946f1afba114191a2efed88ea2dc0a8` | UNSET | UNSET | UNSET | BA-01 | met: the V7-01 text change (BA-07 V8) and the AR8-02 confirmation of the BA-09 extension of the precondition union (before candidate); the Coordinator's designation of the independent copy (location and holder) in the BA-07 candidate or ratification record (before seal) | BASE-17 | NB-6 |
| BA-10 | `CT21D-BASE-PARAMETER-SHEET` | `docs/initiatives/I-52-ct21d-baseline-ba-10-parameter-sheet-v8.md` | DRAFT | `867fc4adb332f0a22ade4a24cb74309e5c76f39a90f5b24ab50f605144889dd5` | UNSET | UNSET | UNSET | UNSET | BA-01 | the `PARAM-05` label value from the read-only machine-attribute observation on the machine the CAD manager designates (host gate of section 3.3; before candidate: the value enters the bytes); the Architect's review of BA-10 V8 (AQ-V8-01, AQ-V8-02: ruled by AR10-03) and of the Coordinator values. Owner answers Q-O1..Q-O5 are recorded (section 233) | BASE-9, BASE-18 | none of its own |
| BA-06 | `CT21D-BASE-EVM-PREPARATION` | `docs/initiatives/I-52-ct21d-baseline-ba-06-evm-preparation-v7.md` | DRAFT | `7fd22ab0d3c1fd84db2bd348263fc2903a5708bf223a56e3b1871ba0ef5accb1` | UNSET | UNSET | UNSET | UNSET | BA-01, BA-03, BA-05, BA-07 | the Architect's exact delta check of V7 against V6 (AR7-01: only BA06V6-01..04 and AR7-02 edits) (before candidate) | BASE-15, BASE-5 | NB-4 |
| BA-08 | `SELECTIVE_KIND_AUTHORITY_BASELINE` | `docs/initiatives/I-52-ct21d-baseline-ba-08-selective-kind-baseline-v7.md` | DRAFT | `552e337ea6dfbbeea0ae730f609b7b2207976c7428d10bc267644955f723e72d` | UNSET | UNSET | UNSET | UNSET | BA-01, BA-03, BA-05, BA-06 | K-3 (TOL_SCALE decision, test and record; section 3.3) and K-8 (FX-IMP and its variants) (before candidate); K-2 (library census, in effect met before the candidate through BA-05) and K-5 (Coordinator ratification of route R-1 with the AR3-14 conditions) (before seal). The fixture construction build (AR6-02) and the conformance records are execution prerequisites, not registry gates | BASE-2, BASE-3, BASE-4, BASE-6, BASE-7, BASE-10, BASE-18 (tolerances) | none of its own |
| BA-09 | `CT21D-BASE-WARMUP` | `docs/initiatives/I-52-ct21d-baseline-ba-09-warmup-v7.md` | DRAFT | `a68189efb7bce6b34167194e9aa982378e394b342e22ff10760291d564c53ebb` | UNSET | UNSET | UNSET | UNSET | BA-01, BA-06, BA-07, BA-08 | W-1 (the recorded synthetic warm-up plan data; its file becomes a path of this entry when produced) (before candidate) | BASE-5, BASE-10 | none of its own |
| BA-04 | `CT21D-BASE-SCENARIO-CATALOG` | `docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v7.json`<br>`docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v7.md` | DRAFT | `1971e8db44bfba217a190d3ec83a54388840d39be02935d45679127d519913ee` | UNSET | UNSET | UNSET | UNSET | BA-01, BA-02a, BA-03, BA-05, BA-06, BA-07, BA-08, BA-09, BA-10 | K-3 (declaration of the S comparisons) and K-8 (before candidate: the declarations enter the bytes); fixture instances, HOST_DEFAULT_MAP and the other pins are fixed before the first governing run, not prerequisites | BASE-13, BASE-16 (scenarios) | NB-2 |
| BA-02b | `CT21D-BASE-COVERAGE-MATRIX` | `docs/initiatives/I-52-ct21d-baseline-ba-02b-coverage-matrix-v7.md`<br>`docs/automation/evidence/I-52-ct21d-ba04-v7-checks.py`<br>`docs/automation/evidence/I-52-ct21d-ba04-v7-checks-output.json` | DRAFT | `0dba7a53f1dff032ff2232153f4f23ec6bcacc8d755c97ecec16d1b5aa234a5d` | UNSET | UNSET | UNSET | UNSET | BA-01, BA-02a, BA-04 | none beyond its DEPENDS_ON | BASE-20 | NB-2 |
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
| bound product SHA (`BOUND_PRODUCT_SHA`, AR6-01) | the product commit from which every static code fact of the baseline is taken; never a tuple value (AR5-12) | `95690c28dc6268e61dff32a0cbc33cc9fde3d47f` |
| per-build bound-SHA precondition (AR5-12) | before each build under test: every cited (path, blob) of the reused product route equals the pair at the build's source SHA, or the delta is recorded and processed; the result is a review record in BA-07 custody (the BA-07 entry, section 5.4) | NOT PRODUCED (execution preparation) |
| fixture construction build (`FIXTURE_CONSTRUCTION_BUILD`, AR6-02) | a third build role, not a build under test and not a characterization build: builds `FX-ANN-BLANK` step 1 from `69daf03a35c630e453e1d9e98136f128bd0325a4`; its identity is recorded in the fixture conformance record | NOT BUILT |
| catalog folder (`CATALOG_FOLDER`) | tuple field (BUILD_BOUND) added by **V2.5** (AR3-13), sampled for the build under test of each run | UNSET |
| build profiles (one admissible BUILD_BOUND profile per `BUILD_UNDER_TEST`: product build and characterization build; AR4-06) | tuple | UNSET: fixed at execution preparation, with the recorded build equivalence of AR4-06 (4) |
| library entity-type census (universe: every block of the library file) | prerequisite of BA-05 (before candidate) and of BA-08 (before seal) | NOT PERFORMED (host; not authorized) |
| machine-attribute observation (`PARAM-05` of the BA-10 entry) | prerequisite of BA-10 (before candidate: the value enters the bytes) | NOT PERFORMED (host; not authorized) |
| custody log | created at the baseline by its genesis entry (the BA-07 entry, section 8) | NOT CREATED |
| manifest (one per build under test) | execution prerequisite EXEC-11 | NOT CAPTURED |
| baseline anchor (`ANCHOR_RECORDED`) | custody | NOT CREATED (created only after every artifact entry is sealed) |

### 3.3 Plan of a future non-governing host gate (BASE-18; AR3-25)

`BASELINE_READY` cannot be reached without host work. The Owner authorized one **non-governing** host gate for the items below (decisions section 233, `HOST_GATE_BA11_3_3 = AUTHORIZED`, non-governing baseline-preparation evidence only, on the one machine the CAD manager designates). **No host run has started or may start** until the CAD manager designates the machine and the exact machine/session tuple is recorded, and the Coordinator rules on the execution package (`docs/initiatives/I-52-ct21d-host-gate-execution-package-v1.md`, section 236). This registry itself authorizes nothing; the authorization is the Owner's.

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
| `docs/initiatives/I-52-ct21d-authority-contract-v2.6.md` | `fd9c16873c526e5f009ad92beb8dd2fe236b964f` |
| Proposal ALT-21D V5 (guarantee authority) | `f73157d9ce2d5013f4f91fb2d4f4d127a4f879a1` |

```text
CANDIDATE_COMPOSITE_HASH (V2.1 + V2.2 + V2.3 + V2.4 rev. 3 + V2.5 rev. 2 + V2.6 draft revision 3 + one LF + V5 blob id;
                          informational until the BA-01 candidate ruling)
  = 97100cff11868807f3c49f25bf25d0385c86e8873cba0cf2c1996338b9b98eb4
```

## 5. `BASE-1 .. BASE-20` after the relink round (V6)

| Base | Requirement | State |
|---|---|---|
| BASE-1 | the authority contract is complete and agreed | V2.1..V2.3 agreed for preparation; V2.4 revision 3 and V2.5 revision 2 textually verified (sections 218, 221); V2.6 revision 3 pre-approved and textually verified (sections 229, 230): CT-21D-V2.6 agreed; BA-01 CANDIDATE_FOR_SEALING (AR9-02) |
| BASE-2 | the Selective kind authority design is complete and agreed | BA-08 draft; K-2, K-3, K-5, K-8 open; the open Architect items are listed in the BA-08 entry |
| BASE-3 | E-04 criteria fixed | criteria in the contract; route R-1 pending ratification (K-5) with the conditions of AR3-14; values are the pin `E04_ADMITTED_SET@R-1` |
| BASE-4 | `LOCK_MODE` candidate contract fixed | candidate set LM-1/LM-2 sufficient; selection is the pin `LOCK_MODE` |
| BASE-5 | event instrumentation designed | BA-06 V6 re-derived at the bound SHA 95690c28 (AR6-03): `STATIC_REVIEWED` carries per class only through the Architect's exact delta check; OC-IMPORT, OC-REF-TOP, OC-ENVELOPE, OC-PREP-READ and OC-VERIFY-READ are re-reviewed; BA-09 (WU-Q1 ruled by AR3-16) |
| BASE-6 | abort read-set fixed | specification in the BA-08 entry (section 6); E-08 compares the REFERENCE and CONTEXT_VARIABLE members (AR4-08); the MISSING readers are execution blockers (K-7) |
| BASE-7 | composition and reuse rules fixed | rules in the BA-08 entry; library census (K-2) pending |
| BASE-8 | UNDO evidence plan fixed | **SATISFIED_BY_CONTRACT at the BA-01 `SEAL_HASH`** (V2.1 16); pending that seal |
| BASE-9 | governing-result, retry and attempt-log policies fixed | policies in the contract; `MAX_ATTEMPTS` per repetition slot (AR3-24) in BASE-18 |
| BASE-10 | every `BEFORE_CT21D_BASELINE` item resolved | BA-08 and BA-09 drafts |
| BASE-11 | baseline hashes recorded | this registry; candidates BA-01, BA-02a, BA-03 (section 231) and BA-07 (section 235); nothing ratified or sealed |
| BASE-12 | OD-1 closed (sealed Selective scope) | BA-03 V5 re-prepared at the bound SHA (runner exit 0, no drift); CANDIDATE_FOR_SEALING (AR9-04); seal needs the PA-14 conditions 3 and 4 |
| BASE-13 | scenario catalog complete with expected results and authority | BA-04 draft; K-3, K-8 and the open Architect items it lists block its candidate |
| BASE-14 | fingerprint specification agreed | BA-05 draft; census and the AR4-10 confirmations pending (host; before candidate) |
| BASE-15 | E-12 procedure and corpus definition agreed | BA-06 V7 draft; `STATIC_REVIEWED` approved per class at the bound SHA 95690c28 (AR7-01) |
| BASE-16 | instrument qualification acceptance criteria agreed | criteria **SATISFIED_BY_CONTRACT at the BA-01 `SEAL_HASH`**; the `Q-*` scenarios (per build under test, AR4-06) are sealed with the catalog (BASE-13) |
| BASE-17 | manifest capture and custody procedure agreed | BA-07 V8 CANDIDATE_FOR_SEALING (AR10-02); the independent-copy designation is recorded in its candidate or ratification record (before seal) |
| BASE-18 | classified parameters fixed | BA-10 V8: Owner values Q-O1..Q-O5 recorded (`N_A` 44, `N_B` 29, `WU_DRY_RUNS` 29, `N_det` 90, `n` 59; section 233) and Coordinator values (`PARAM-04` m 3, `EVIDENCE_REPETITION` 1; reviewed, AR10-05); `PARAM-05` label value `UNSET`: it needs the **machine-attribute observation** on the machine the CAD manager designates (section 3.3). Also needs the **BA-08 tolerances with the `TOL_SCALE` decision, test and record** (K-3, section 3.3). NOT MET |
| BASE-19 | control-class registry agreed | BA-02a V3 CANDIDATE_FOR_SEALING (AR9-03); seal needs the dated AR3-29 re-run |
| BASE-20 | coverage matrix agreed | BA-02b draft; depends on the catalog |

```text
BASE-11 = NOT MET     SEALED ENTRIES = 0     CANDIDATE ENTRIES = 4 (BA-01, BA-02a, BA-03, BA-07)
CT21D_AUTHORITY_BASELINE_READY = FALSE
```

## 6. Delta V10 (decisions sections 233 to 236)

| Item | Decision | Fix |
|---|---|---|
| BA-07 | AR10-02 | entry relinked to BA-07 V8 (V7-01 applied, AR8-02): STATUS `CANDIDATE_FOR_SEALING`, `CANDIDATE_HASH` inscribed; the V7-01 prerequisite is met |
| BA-10 | section 233, AR10-05 | entry relinked to BA-10 V8 (Owner values and Coordinator values recorded); stays `DRAFT`: the `PARAM-05` label value needs the host observation |
| section 3.3 | section 233 | records the Owner's authorization of the non-governing host gate and that no run may start before the machine designation and the Coordinator's ruling on the package |
| BASE-11, BASE-17, BASE-18, status block | AR10-02, AR10-05 | updated to the above |
| sections 1..2.1 | AR9-01 | unchanged (byte-identical to V9, the rule-5 blob 9f5d36cf) |

## 6e. Delta V9 (decisions section 231; history)

| Item | Decision | Fix |
|---|---|---|
| candidate inscription | AR9-02, AR9-03, AR9-04, AR9-06 | BA-01, BA-02a, BA-03: STATUS `CANDIDATE_FOR_SEALING` and `CANDIDATE_HASH`; header, BASE-1, BASE-11, BASE-12, BASE-19 and the status block updated; BA-01 prerequisites recorded as met |
| contract status | section 230 | header: effective contract CT-21D-V2.6 agreed |
| sections 1..2.1 | AR9-01 | unchanged (byte-identical to V8, the rule-5 blob 9f5d36cf) |

## 6d. Delta V8 (decisions section 229; history)

| Item | Decision | Fix |
|---|---|---|
| V2.6 revision 3 | AR8-01 | section 4 lists blob of revision 3; candidate composite recomputed |
| V7-04 | section 229 | BA-03 and BA-07 prerequisite cells: AQ-V6-01 ruled by AR7-03 and the BA-09 extension confirmed by AR8-02 |
| V7-01 | section 229 (AR8-04) | BA-07 prerequisite cell: adds the V7-01 text change (BA-09 in `citedArtifactsUnchanged`, in a new BA-07 version) as a before-candidate prerequisite; the BA-07 V7 header predates that finding, so the cell departs from it only by this item (BA07V6-01 (4) otherwise kept) |
| sections 1..2.1 | AR8-04 | unchanged (byte-identical to V7); agreed-text hash for rule 5 as in AR5-15 (without the informative line) |

## 6c. Delta V7 (decisions section 226; history)

| Item | Decision | Fix |
|---|---|---|
| BA11V6-01 | AR7-08 | rule 3 names V2.4, V2.5, V2.6 and every later errata document |
| BA11V6-02 | AR7-08 | section 1 names V2.6's *Contract hash* row before the generic clause |
| BA11V6-03 | AR7-08 | informative line points to the V7 graph-check output |
| BA11V6-04 | AR7-08 | BASE-15: approved per class at 95690c28 (AR7-01) |
| BA11V6-05 | AR7-08 | BA-08 prerequisites use only the admitted gates |
| BA07V6-01 (4) | AR7-05 | BA-07 row prerequisites follow the BA-07 V7 header |
| relinked paths | section 227 | BA-02b/04/06/07/08/09/10 V7; V2.6 revision 2 in section 4 |

## 6b. Delta V6 (decisions sections 223 and 224; history)

| Item | Decision | Fix |
|---|---|---|
| bound SHA | AR6-01 | header and section 3.2 row `BOUND_PRODUCT_SHA` |
| fixture construction build | AR6-02 | section 3.2 row `FIXTURE_CONSTRUCTION_BUILD` |
| bound-SHA precondition | AR5-12 | section 3.2 row |
| contract V2.6 | AR6-05, AR6-06 | section 4 (six documents); BA-01 row prerequisites; section 1 precedence sentence generalized to later errata documents (the rule-5 agreement is made after V2.6, AR6-06) |
| relinked entries | section 223.4 | section 3 paths: BA-03 V5, BA-02b/04/06/07/08/09/10 V6 |

## 7. Delta V5 (decisions section 217; kept as history)

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
