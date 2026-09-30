# I-52 — CT-21D Baseline Artifact BA-11: Baseline Hash Registry and Sealing Workflow (DRAFT V4)

> **BASELINE ARTIFACT BA-11 V4 — DRAFT. It binds nothing yet: no artifact is a candidate, none is sealed, and no seal hash exists.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-HASH-REGISTRY
> ARTIFACT_VERSION          = 4-DRAFT   (supersedes 3-DRAFT, blob 24769d088c71cb172da4d6d8473624bd0575f13a, which stays as history)
> AUTHORITY_CONTRACT        = CT-21D V2.1 + V2.2 + V2.3 + V2.4 (draft revision 3) + V2.5 (draft revision 1); V2.4 and V2.5 pending
>                             Coordinator textual verification
> DELTA RULING APPLIED      = decisions section 214: AR3-25 (workflow, graph, pins, BA-01 identity, SEALED_AT), AR3-13 (CATALOG_FOLDER),
>                             AR3-17 (BA-05 without BA-08), AR3-22 (EVM_FROZEN cites INVENTORY_REVIEW_RECORDS), AR3-23 (pin custody, model A),
>                             AR3-24 (WU_DRY_RUNS = N_B), AR3-26 (catalog seal blockers), AR3-27 (V24R2-05, V24R2-06)
> BASE ITEM                 = BASE-11 (baseline hashes recorded): NOT MET
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 1. Hash procedure (normative)

| Aspect | Rule |
|---|---|
| file hash | SHA-256 of the file bytes after CRLF is normalized to LF, lowercase hexadecimal |
| artifact hash, one file | the file hash |
| artifact hash, several files | SHA-256 of the concatenation, in ascending ordinal path order, of `<path>` LF `<file hash>` LF |
| contract composite hash | SHA-256 over the LF-normalized texts of the blobs of the effective contract documents (V2.1, then each errata document in version order), then **one LF**, then the V5 blob id written as 40 lowercase ASCII hexadecimal characters with **no** trailing LF. It is the byte-level reading of the "plus the V5 blob id" approach of V2.2 0.1 / note N-1 of V2.2, extended by V2.3 section 0 / D-1 of V2.3; V2.4 section 0 and D-1, and V2.5 section 0 and D-1, cite this row for the procedure (the V2.4 and V2.5 *Contract hash* rows govern over the earlier ones, AR2-03) |
| identity of the composite row (AR3-25, V24R2-06) | the row above is **fixed by the blob of the BA-11 version that the BA-01 candidate ruling cites**, as the procedure that D-1 of V2.4 cites. A change to it is a change of the contract identity procedure and requires a **new contract version**, which cascades to BA-01 and to every dependent entry. This is the **single declared exception** to the acyclicity of section 2.1 |
| `SEALED_AT` of a scenario (V2.1 26.1; AR3-25, BA11V3-05) | SHA-256 of the canonical serialization of the scenario's expectation fields, as defined in BA-04 V4 section 1. It is computed and written into the catalog **before** the BA-04 candidate ruling, so it is inside the candidate bytes. The catalog is sealed **only at artifact level** (the BA-04 entry of section 3); there is no per-scenario seal |
| `CURRENT_HASH` column | the artifact hash of the bytes at the commit of this version, **informational only**; it is not a candidate and not a seal |

## 2. Transitions and authority (normative)

```text
DRAFT  ->  CANDIDATE_FOR_SEALING  ->  RATIFIED  ->  SEALED
```

| Transition | Required | Recorded |
|---|---|---|
| `DRAFT -> CANDIDATE_FOR_SEALING` | an **Architect ruling on the exact bytes** of the artifact; every entry of its `DEPENDS_ON` at least `CANDIDATE_FOR_SEALING`; every `PREREQUISITES` item whose gate is *before candidate* met and its record cited | `CANDIDATE_HASH` = the artifact hash of those bytes, and the ruling reference, **in this registry** (never in the artifact) |
| `CANDIDATE_FOR_SEALING -> RATIFIED` | a **Coordinator** ratification and an **Architect** ratification, **each citing the exact `CANDIDATE_HASH`**; for BA-03 also the seal conditions of V2.2 PA-14: its seal-time re-run writes its output **outside** the artifact paths (the runner's mandatory output argument) and the ratification records cite that output's exit status, `identityDrift` and `failed` (a non-zero exit, any drift or any failure means BA-03 is re-prepared); for BA-02a the dated re-run of its byte-level comparison (AR3-29); all recorded in the ratification records, **never in the artifact** | both ratification records (location and fields: BA-07 V4 section 2 and 5.2), cited here |
| `RATIFIED -> SEALED` | the artifact hash recomputed on the committed bytes equals the `CANDIDATE_HASH`; every entry of its `DEPENDS_ON` is `SEALED` (their `SEAL_HASH` cited); every `PREREQUISITES` item whose gate is *before seal* met and its record cited | `SEAL_HASH` (normative) = `CANDIDATE_HASH` |
| any byte change after a candidate ruling | the candidate and any ratification **expire**; the artifact returns to `DRAFT` as a new version | the expired hash stays in the history |
| **cascade** (BA11V3-06) | a new version of an entry that is **at least `CANDIDATE_FOR_SEALING`** (`CANDIDATE_FOR_SEALING`, `RATIFIED` or `SEALED`) **expires the candidates, the ratifications and the seals of every entry that depends on it** (directly or transitively) | the expiry is recorded here |
| change of a sealed artifact | a **new version** (new entry, new candidate, new ratifications, new seal); the old seal is retained and marked `SUPERSEDED` | both entries |

**Authority rules (AR2-44, AR3-25).**

1. **This registry is the only authority** for the status and the hashes of every artifact entry of section 3. The status and hash text inside an artifact is **frozen, non-authoritative text** of its candidate bytes. An artifact never records its own candidate, ratifications or seal. The per-scenario `SEALED_AT` values of BA-04 are expectation hashes written before the candidate (section 1), not status.
2. **BA-11's own status and hashes live outside BA-11** (BA11-F1, BA11V3-08): its status, its `CANDIDATE_HASH`, its two ratification records and its `SEAL_HASH` are established by its ratification records and by the custody entries `BASELINE_RECORDED`, `BASELINE_APPROVED` and `ANCHOR_RECORDED` of BA-07 V4 (sections 8 and 9; anchoring is derived from `ANCHOR_RECORDED`, there is no separate `ANCHORED` event). The BA-11 row of section 3 says so and stays constant.
3. **BA-01 is the contract** (BA11-F11): its `CANDIDATE_HASH` and `SEAL_HASH` are the **contract composite hash** (section 4); it passes through the same transitions (the Coordinator's textual verification of V2.4 and V2.5 and the agreement of the effective contract precede its candidate ruling). `BASE-8` and `BASE-16` (criteria) are **`SATISFIED_BY_CONTRACT` at the BA-01 `SEAL_HASH`** and reopen with any new contract version.
4. **The final registry** is sealed **last**, after every other **artifact entry of section 3** is `SEALED` ("every other entry" means those entries only; the pin slots of section 3.1 are not entries). Its seal hash enters the custody genesis entry and the remote anchor; any later change is a new registry version with a new anchor.
5. **Sealing workflow agreement** (AR3-25): before the **first** candidate ruling, the Architect agrees sections 1, 2 and 2.1 of this registry as the governing sealing workflow, on the exact bytes of the BA-11 version that the agreement cites. A later change to those sections **reopens every candidate issued under them**.

A candidate hash is **never normative**; only a `SEAL_HASH` is.

### 2.1 `DEPENDS_ON`, `PREREQUISITES` and the seal order (normative; BA11-F3, BA11V3-01, BA11V3-02, BA11V3-07)

`DEPENDS_ON` lists **content dependencies between entries of this registry only**: entry `X` depends on `Y` when a normative value of `X` is defined in `Y`. A mention of a name defined elsewhere (a fixture name, a scenario id) is not a dependency. `DEPENDS_ON` is governed by the gating and cascade rules of section 2. `PREREQUISITES` lists **external inputs** (host observations, Owner and Coordinator decisions, Architect rulings, construction work); each names its **gate** (*before candidate* or *before seal*) and the record that proves it; a prerequisite has no registry state.

Complete edge list (`X -> Y` means `X` depends on `Y`):

```text
BA-02a -> BA-01
BA-02b -> BA-02a
BA-02b -> BA-04
BA-03 -> BA-01
BA-04 -> BA-01
BA-04 -> BA-02a
BA-04 -> BA-03
BA-04 -> BA-05
BA-04 -> BA-06
BA-04 -> BA-08
BA-04 -> BA-09
BA-04 -> BA-10
BA-05 -> BA-01
BA-06 -> BA-01
BA-06 -> BA-03
BA-06 -> BA-05
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

The graph is **acyclic** (checked mechanically by `docs/automation/evidence/I-52-ct21d-ba11-v4-graph-check.py`, output `docs/automation/evidence/I-52-ct21d-ba11-v4-graph-check.json`, which also checks that the seal order is topological and that the `DEPENDS_ON` cells of section 3 equal this list), with the single declared exception of section 1 (the composite row, fixed by blob). The V2 and V3 cycle BA-05 <-> BA-08 is broken **in content**: BA-05 V4 writes its semantic vectors in slot units and takes the census universe from every block of the library file (AR3-17), so only BA-08 -> BA-05 remains. BA-07 V4 defines its own canonical serialization (AR3-23), so BA-07 does not depend on BA-05. BA-10 V4 is symbolic (K = the number of sampled scenarios in the sealed catalog) and its illustrative totals live in a non-sealed cost memo, so BA-10 does not depend on BA-04 (AR3-24). **Seal order** (a topological order of the graph):

```text
BA-01  ->  BA-02a, BA-03, BA-05, BA-07, BA-10  ->  BA-06  ->  BA-08  ->  BA-09  ->  BA-04  ->  BA-02b  ->  BA-11 (last)
```

## 3. Registry entries

| Entry | ARTIFACT_ID | Paths | STATUS | CURRENT_HASH (informational) | CANDIDATE_HASH | COORDINATOR | ARCHITECT | SEAL_HASH | DEPENDS_ON | PREREQUISITES (gate) | BASE items | Blocker |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| BA-01 | authority contract `CT-21D-V2.5` | the five contract documents (section 4) | DRAFT (V2.1..V2.3 agreed for preparation; V2.4 and V2.5 pending textual verification) | composite in section 4 | UNSET | UNSET | UNSET | UNSET | none | Coordinator textual verification of V2.4 and V2.5 and agreement of the effective contract (before candidate) | BASE-1, BASE-8, BASE-16 (criteria) | none |
| BA-02a | `CT21D-BASE-CONTROL-REGISTRY` | `docs/initiatives/I-52-ct21d-baseline-ba-02a-control-registry-v3.md` | DRAFT | `acb5e01de3c1b722e9757bfb26aafb6a707599b37dd2902e4c64676c1bd2e3c3` | UNSET | UNSET | UNSET | UNSET | BA-01 | the dated re-run of its byte-level comparison with the contract (AR3-29; recorded in the ratification records, before seal) | BASE-19 | none |
| BA-03 | `SELECTIVE_VIEW_FAMILY_SCOPE_V1` | `docs/initiatives/I-52-ct21d-baseline-ba-03-selective-scope-v4.md`<br>`docs/automation/evidence/I-52-ct21d-ba03-v4-checks.json`<br>`docs/automation/evidence/I-52-ct21d-ba03-v4-checks.py`<br>`docs/automation/evidence/I-52-ct21d-ba03-v4-checks-output.json` | DRAFT | `fbbb5f4c688abecedaea6747c00234b4bb786849e66b9c6cd0e6cf7234399c4f` | UNSET | UNSET | UNSET | UNSET | BA-01 | V2.2 PA-14 conditions 3 and 4: the seal-time re-run and the post-I-57 reading (ratification records, before seal) | BASE-12 | NB-1 |
| BA-05 | `CT21D-BASE-FINGERPRINT-SPEC (FPSPEC-V1)` | `docs/initiatives/I-52-ct21d-baseline-ba-05-fingerprint-specification-v4.md`<br>`docs/automation/evidence/I-52-ct21d-fpspec-v1-vectors-v4.gen.py`<br>`docs/automation/evidence/I-52-ct21d-fpspec-v1-vectors-v4.json` | DRAFT | `eb67c1ccb6479a5ef8692b060e68b8cdfc3d02a0c642b371e65520d2a06a385d` | UNSET | UNSET | UNSET | UNSET | BA-01 | the library entity-type census over every block of the library file (host, K-2; section 3.3; before candidate, as a draft update of BA-05); the Architect's answers to AQ-V4-08, AQ-V4-09 and AQ-V4-10 or their recorded deferral (before candidate) | BASE-14 | NB-3 |
| BA-07 | `CT21D-BASE-MANIFEST-CUSTODY` | `docs/initiatives/I-52-ct21d-baseline-ba-07-manifest-custody-v4.md`<br>`docs/automation/evidence/I-52-ct21d-ba07-v4-jcs-vector.json`<br>`docs/automation/evidence/I-52-ct21d-ba07-v4-jcs-vector.py` | DRAFT | `3a9df15379ffa651b809b65dd461e3a6fb41f7346c713f696397cda8dcd3f014` | UNSET | UNSET | UNSET | UNSET | BA-01 | the Architect's answer to AQ-V4-12 or its recorded deferral (before candidate); the Coordinator's designation of the independent copy (location and holder) in the BA-07 candidate or ratification record (before seal) | BASE-17 | NB-6 |
| BA-10 | `CT21D-BASE-PARAMETER-SHEET` | `docs/initiatives/I-52-ct21d-baseline-ba-10-parameter-sheet-v4.md` | DRAFT | `3336fe5f1d1b5d917debaa99d24af977be35ea0d39b21a8d5c0b2361c77bbc20` | UNSET | UNSET | UNSET | UNSET | BA-01 | Owner Q-O1..Q-O5 (Q-O5 by the Owner or the CAD manager the Owner designates); Coordinator PARAM-04 cap, EVIDENCE_REPETITION and the recording of N_A, N_B, N_det, n and WU_DRY_RUNS = N_B; the machine-attribute observation (section 3.3); the Architect's answer to AQ-V4-13 or its recorded deferral (all before candidate: the values enter the bytes) | BASE-9, BASE-18 | none of its own |
| BA-06 | `CT21D-BASE-EVM-PREPARATION` | `docs/initiatives/I-52-ct21d-baseline-ba-06-evm-preparation-v4.md` | DRAFT | `4faae1658dc031808868102761af430b8fcce64b049cac0f6e6c6ac534746f09` | UNSET | UNSET | UNSET | UNSET | BA-01, BA-03, BA-05 | the Architect's exact delta check and approval of STATIC_REVIEWED together with the dynamic-trace plan on the V4 bytes, the PA-11 condition 5 judgment included (AR3-22); the Architect's answers to AQ-V4-11 (CQ-01..CQ-07) and AQ-V4-01 (CQ-08) or their recorded deferral (all before candidate); the evidence files bound by hash in its section 14 exist at those hashes (before candidate). AQ-V4-14 is routed by BA-06 to the catalog and is not a BA-06 prerequisite | BASE-15, BASE-5 | NB-4 |
| BA-08 | `SELECTIVE_KIND_AUTHORITY_BASELINE` | `docs/initiatives/I-52-ct21d-baseline-ba-08-selective-kind-baseline-v4.md` | DRAFT | `5b83a32c9bbf51add8ca85c0b44686d1907ef3226501b6e6442ab3af64bcc5d7` | UNSET | UNSET | UNSET | UNSET | BA-01, BA-03, BA-05, BA-06 | K-3 (TOL_SCALE decision, test and record; section 3.3), K-8 (FX-IMP and its variants), K-6 (the Architect's rulings: AQ-V4-07 = Q-HDM-1, and AQ-V4-08, AQ-V4-09, AQ-V4-10 where a ruling differs from the reading recorded in BA-08) (before candidate); K-2 (library census) and K-5 (Coordinator ratification of route R-1 with the AR3-14 conditions) (before seal) | BASE-2, BASE-3, BASE-4, BASE-6, BASE-7, BASE-10, BASE-18 (tolerances) | none of its own |
| BA-09 | `CT21D-BASE-WARMUP` | `docs/initiatives/I-52-ct21d-baseline-ba-09-warmup-v4.md` | DRAFT | `05189ad8e491dd8e6972cf139c4ad81bbeb955d6196132568c948ba481220bcf` | UNSET | UNSET | UNSET | UNSET | BA-01, BA-06, BA-07, BA-08 | W-1: the recorded synthetic warm-up plan data (its file becomes a path of this entry when produced); W-9: the Architect's answer to AQ-V4-02 or its recorded deferral (both before candidate) | BASE-5, BASE-10 | none of its own |
| BA-04 | `CT21D-BASE-SCENARIO-CATALOG` | `docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v4.json`<br>`docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v4.md` | DRAFT | `fb5b78e62d051709ec8f0f7a9cd07042d8313d475b016f40822e64a8d88a4dbc` | UNSET | UNSET | UNSET | UNSET | BA-01, BA-02a, BA-03, BA-05, BA-06, BA-08, BA-09, BA-10 | only K-3, K-8 and the Architect's rulings block its seal (AR3-26): K-3 (declaration of the S comparisons), K-8, K-6 (AQ-V4-07), AQ-V4-01..AQ-V4-06 and AQ-V4-14 (before candidate: the declarations enter the bytes); fixture instances and HOST_DEFAULT_MAP are pins fixed before the first governing run, not prerequisites | BASE-13, BASE-16 (scenarios) | NB-2 |
| BA-02b | `CT21D-BASE-COVERAGE-MATRIX` | `docs/initiatives/I-52-ct21d-baseline-ba-02b-coverage-matrix-v4.md` | DRAFT | `315b4e3268a6947764940e8ae8aefbc063d0cb98ead686d2b2ed317bb2360f87` | UNSET | UNSET | UNSET | UNSET | BA-02a, BA-04 | none beyond its DEPENDS_ON | BASE-20 | NB-2 |
| BA-11 | `CT21D-BASE-HASH-REGISTRY` (this file) | this file | recorded outside (rule 2; BA-07 V4) | recorded outside | recorded outside | recorded outside | recorded outside | recorded outside | every other artifact entry of this section | the Architect's agreement of sections 1, 2 and 2.1 (rule 5; before the first candidate ruling of any entry) | BASE-11 | none of its own |

### 3.1 Pin slots (sealed with BA-11; AR2-16, AR2-17, AR3-23, BA11V3-03)

This table is a **slot table sealed with BA-11**: it fixes the slot names and the rule for where their values are recorded. A pin is a named slot in the sealed catalog (BA-04 V4 section 2.1). **After the baseline, a pin's value, state and record hash are authoritative only in its pin record and in the BA-07 custody log; they are never written into BA-11.** The pin's own seal is the pair of **Coordinator and Architect ratification records that cite the hash of the pin record** (location: BA-07 V4 section 2), cited by its `PIN_RECORDED` entry. Pin state machine: `RECORDED -> RATIFIED -> ANCHORED` (BA-07 V4 section 5.1); `ANCHORED` is derived from a later `ANCHOR_RECORDED` entry that covers the `PIN_RECORDED` entry. A pin is usable by a governing run only when its `PIN_RECORDED` entry is covered by an anchor recorded before that run's `IN_USE` entry, and the run lists the pin-record hashes it consumes. Writing a pin never changes the bytes of the catalog or of this registry; a pin is written after the value is frozen from its own evidence and before the first governing run that uses it.

| Pin slot | Value | Source | Where the value and state live |
|---|---|---|---|
| `E04_ADMITTED_SET@R-1` | the E-04 admitted set of the designated route | E4-LEARN-R1-* under the selected `LOCK_MODE` (AR2-11; AR3-01 characterization build) | its pin record and the BA-07 V4 custody log (`PIN_RECORDED`; ratification records; state `RECORDED -> RATIFIED -> ANCHORED`) |
| `LOCK_MODE` | the selected lock mode | the selection review of V2.1 3.4 over LK-* (AR3-01 characterization build) | its pin record and the BA-07 V4 custody log (`PIN_RECORDED`; ratification records; state `RECORDED -> RATIFIED -> ANCHORED`) |
| `EVM_FROZEN` | the frozen event model | learning after `REVIEWED` (V2.4; V2.1 27.2); the pin record cites the EVM field `INVENTORY_REVIEW_RECORDS` (BA-06 V4; AR3-22) | its pin record and the BA-07 V4 custody log (`PIN_RECORDED`; ratification records; state `RECORDED -> RATIFIED -> ANCHORED`) |
| `HOST_DEFAULT_MAP` | the host-default attribute map (BA-08 V4, rule of the pin) | only the non-governing characterization scenarios `HDM-CHAR-FX1F`, `HDM-CHAR-FXANN`, `HDM-CHAR-FXDIM` of BA-04 V4 (AR3-11) | its pin record and the BA-07 V4 custody log (`PIN_RECORDED`; ratification records; state `RECORDED -> RATIFIED -> ANCHORED`) |
| `FIXTURE_INSTANCE@<FX>` | the hash of each fixture instance (template drawing and library copy); `<FX>` ranges over the fixtures and variants of BA-08 V4 section 12 and the variant libraries `L-VAR-*` that BA-04 V4 uses (among them `FX-ANN-BLANK`, `FX-IMP-XREF-HOMONYM`, `L-VAR-DIFF`, `L-VAR-PROXY`) | host work under a future gate, with conformance to BA-08 V4 section 12 recorded | its pin record and the BA-07 V4 custody log (`PIN_RECORDED`; ratification records; state `RECORDED -> RATIFIED -> ANCHORED`) |

`TOL_SCALE` is **not** a pin: it is a value of the Selective kind baseline (V2.2 PA-9, BA-08 V4 section 10) without authority today (K-3).

### 3.2 Bound items that are not artifacts of this registry

| Item | Kind | Status |
|---|---|---|
| block-library file (path, SHA-256) | tuple field (BUILD_BOUND, V2.1 2.1) | UNSET: fixed for the exact build at execution preparation; the file is not versioned in the repository |
| catalog folder (`CATALOG_FOLDER`: resolved path and SHA-256 of every `*.csv` and `*.json` directly in it) | tuple field (BUILD_BOUND) added by **V2.5** (AR3-13; pending the Coordinator's textual verification of V2.5) | UNSET: fixed for the exact build; sampled before the library acquisition and re-checked after CP |
| library entity-type census (universe: every block of the library file, BA-05 V4) | prerequisite of BA-05 (before candidate) and of BA-08 (before candidate) | NOT PERFORMED (host; not authorized) |
| machine-attribute observation (BA-10 V4 `PARAM-05`) | prerequisite of BA-10 (before seal) | NOT PERFORMED (host; not authorized) |
| custody log | created at the baseline by its genesis entry (BA-07 V4 section 8) | NOT CREATED |
| manifest | execution prerequisite EXEC-11 | NOT CAPTURED |
| baseline anchor (`ANCHOR_RECORDED`, BA-07 V4) | custody | NOT CREATED (created only after every artifact entry is sealed) |

### 3.3 Plan of a future non-governing host gate (BASE-18; AR3-25)

`BASELINE_READY` cannot be reached without host work that is **not authorized** today. The items below are recorded as the **plan** of one future **non-governing** host gate; this registry authorizes none of them, and each needs an explicit authorization.

| Item | Needed by | What it produces |
|---|---|---|
| machine-attribute observation (read-only, no instrument, no manifest) | BA-10 V4 `PARAM-05` label; Owner Q-O5 | the attribute values of the machine class |
| `TOL_SCALE` test and record (K-3) | BA-08 V4 section 10; BA-04 V4 (declaration of the S comparisons) | the evidence for the Architect's `TOL_SCALE` decision |
| library entity-type census (K-2) | BA-05 V4 section 5; BA-08 V4 | the census record over every block of the library file |
| fixture instances and the `FX-IMP` construction (K-8) | BA-08 V4 section 12; BA-04 V4 (`BLOCKED_BY` K-8) | the instances and their conformance records (instance hashes become the `FIXTURE_INSTANCE@<FX>` pins) |

## 4. Contract identity

| Document | Git blob |
|---|---|
| `docs/initiatives/I-52-ct21d-authority-contract-v2.1.md` | `6259a3beb7445a63620636acd696cdad689965b3` |
| `docs/initiatives/I-52-ct21d-authority-contract-v2.2.md` | `fd7260c57ec05e11f6bbb9f67a728e7dfdf1f609` |
| `docs/initiatives/I-52-ct21d-authority-contract-v2.3.md` | `bc3d67a957b784a874b5bab97ce22116af1eed70` |
| `docs/initiatives/I-52-ct21d-authority-contract-v2.4.md` | `b99f89d209469362ef585508b2dfb2370f0db7d8` |
| `docs/initiatives/I-52-ct21d-authority-contract-v2.5.md` | `c4405ed502ce34ed9e112a1cb0f4a5f3de98c60a` |
| Proposal ALT-21D V5 (guarantee authority) | `f73157d9ce2d5013f4f91fb2d4f4d127a4f879a1` |

```text
CANDIDATE_COMPOSITE_HASH (V2.1 + V2.2 + V2.3 + V2.4 draft revision 3 + V2.5 draft revision 1 + one LF + V5 blob id;
                          informational until the BA-01 candidate ruling)
  = abb3495e1a9ec8899d54f56ff993108820639c6e134c27ccc05e993b3f6c5b82
```

## 5. `BASE-1 .. BASE-20` after the phase-2 delta 2

| Base | Requirement | State |
|---|---|---|
| BASE-1 | the authority contract is complete and agreed | V2.1..V2.3 agreed for preparation; V2.4 (draft revision 3) and V2.5 (draft revision 1) pending textual verification; BA-01 not a candidate |
| BASE-2 | the Selective kind authority design is complete and agreed | BA-08 V4 draft; K-2, K-3, K-5, K-6, K-8 open (see BA-08 V4 section 16) |
| BASE-3 | E-04 criteria fixed | criteria in the contract; route R-1 pending ratification (K-5) with the conditions of AR3-14; values are the pin `E04_ADMITTED_SET@R-1` |
| BASE-4 | `LOCK_MODE` candidate contract fixed | candidate set LM-1/LM-2 sufficient; selection is the pin `LOCK_MODE` |
| BASE-5 | event instrumentation designed | BA-06 V4 (`STATIC_REVIEWED` and the trace plan proposed together for an exact delta check, AR3-22) and BA-09 V4 (WU-Q1 ruled by AR3-16) |
| BASE-6 | abort read-set fixed | specification in BA-08 V4 section 6; the MISSING readers are execution blockers (K-7) |
| BASE-7 | composition and reuse rules fixed | rules in BA-08 V4; library census (K-2) pending |
| BASE-8 | UNDO evidence plan fixed | **SATISFIED_BY_CONTRACT at the BA-01 `SEAL_HASH`** (V2.1 16); pending that seal; reopens with a new contract version |
| BASE-9 | governing-result, retry and attempt-log policies fixed | policies in the contract; `MAX_ATTEMPTS` per repetition slot (BA-10 V4, AR3-24) in BASE-18 |
| BASE-10 | every `BEFORE_CT21D_BASELINE` item resolved | BA-08 V4, BA-09 V4 drafts |
| BASE-11 | baseline hashes recorded | this registry; nothing is a candidate |
| BASE-12 | OD-1 closed (sealed Selective scope) | BA-03 V4 prepared for candidate |
| BASE-13 | scenario catalog complete with expected results and authority | BA-04 V4 draft; only K-3, K-8 and the Architect's rulings block its seal (AR3-26); fixture instances and `HOST_DEFAULT_MAP` are pinned before the first governing run |
| BASE-14 | fingerprint specification agreed | BA-05 V4 draft with normative vectors in slot units; census pending (prerequisite before candidate). `TOL_SCALE` is not a gate of BA-05 (AR3-17) |
| BASE-15 | E-12 procedure and corpus definition agreed | BA-06 V4 draft; `STATIC_REVIEWED` (V2.4 D-3) pending the Architect's exact delta check |
| BASE-16 | instrument qualification acceptance criteria agreed | criteria **SATISFIED_BY_CONTRACT at the BA-01 `SEAL_HASH`** (V2.1 4.3, 4.3.1, V2.2 PA-12); the `Q-*` scenarios are sealed with the catalog (BASE-13) |
| BASE-17 | manifest capture and custody procedure agreed | BA-07 V4 (model A anchors); the independent-copy designation is recorded in its candidate or ratification record (AR3-23) |
| BASE-18 | classified parameters fixed | BA-10 V4 all `UNSET`. Needs: **Owner** Q-O1..Q-O5 (Q-O2 also fixes `WU_DRY_RUNS` = `N_B`, recorded by the Coordinator); **Coordinator** `PARAM-04` cap and `EVIDENCE_REPETITION`; the **machine-attribute observation** (section 3.3) for the `PARAM-05` label; the **BA-08 V4 tolerances with the `TOL_SCALE` decision, test and record** (K-3, section 3.3) |
| BASE-19 | control-class registry agreed | BA-02a V3 (unchanged) prepared for candidate |
| BASE-20 | coverage matrix agreed | BA-02b V4 draft; depends on the catalog |

```text
BASE-11 = NOT MET     SEALED ENTRIES = 0     CANDIDATE ENTRIES = 0
CT21D_AUTHORITY_BASELINE_READY = FALSE
```

## 6. Delta V4 (decisions section 214)

| Finding | Decision | Fix |
|---|---|---|
| BA11V3-01 | AR3-17, AR3-25 | section 2.1: BA-05 has no edge to BA-08 (vectors in slot units, census universe = library file); BASE-14 row |
| BA11V3-02 | AR3-25 | section 2.1: complete edge list (BA-04 -> BA-09, BA-06 -> BA-03, BA-06 -> BA-05), mechanical acyclicity check, seal order of AR3-25 |
| BA11V3-03 | AR3-23, AR3-25 | section 3.1: slot table sealed with BA-11; values, state and hash only in pin records and custody; rule 4 "every other entry" = artifact entries |
| BA11V3-04 | AR3-26 | BA-04 V4 section 7 and BASE-13 row agree: only K-3, K-8 and the Architect's rulings block the catalog seal |
| BA11V3-05 | AR3-25 | section 1 row `SEALED_AT`; authority rule 1 |
| BA11V3-06 | AR3-25 | section 2 cascade row covers `CANDIDATE_FOR_SEALING`, `RATIFIED` and `SEALED` and expires candidates, ratifications and seals |
| BA11V3-07 | AR3-25 | section 3 splits `DEPENDS_ON` and `PREREQUISITES` (with gate); section 2 transitions gate both |
| BA11V3-08 | AR3-25 | rule 2 and the BA-11 row: status recorded outside |
| BA11V3-09 | AR3-25 | section 1: composite row fixed by blob, single declared exception, V5 id encoding, reworded citation |
| BA11V3-10 | AR3-13 | section 3.2: `CATALOG_FOLDER` is the V2.5 tuple field |
| BA11V3-11 | section 214.1 | the correct catalog composition was recorded in decisions section 214.1 (225 + 145 dry runs; 151 non-governing) |
| BA11V3-12 | AR3-24 | the illustrative totals moved to the non-sealed cost memo; 9377 recorded in section 214.1 |
| BA11V3-13 | AR3-24 | `WU_DRY_RUNS` = `N_B` (Owner, Q-O2; recorded by the Coordinator) in BA-04 V4, BA-09 V4, BA-10 V4 and BASE-18 |
| CLOSURE BA11-F2 | AR3-25 | `SEALED_AT` settled (section 1) |
| CLOSURE BA11-F3 | AR3-17, AR3-25 | graph complete and acyclic in content |
| V24R2-05 | AR3-27 | section 1 composite row reworded (who cites what) |
| V24R2-06 | AR3-25 | section 1 identity row (fixed by blob; new contract version on change) |
| BASE-18 plan | AR3-25 | section 3.3 |
