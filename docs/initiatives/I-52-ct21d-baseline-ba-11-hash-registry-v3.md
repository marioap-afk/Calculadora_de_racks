# I-52 — CT-21D Baseline Artifact BA-11: Baseline Hash Registry and Sealing Workflow (DRAFT V3)

> **BASELINE ARTIFACT BA-11 V3 — DRAFT. It binds nothing yet: no artifact is a candidate, none is sealed, and no seal hash exists.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-HASH-REGISTRY
> ARTIFACT_VERSION          = 3-DRAFT   (supersedes 2-DRAFT, blob dd83c2bb69369a6cda8bc7b38983d61dfc3cd419, which stays as history)
> AUTHORITY_CONTRACT        = CT-21D V2.1 + V2.2 + V2.3 + V2.4 (V2.4 draft revision 2, pending Coordinator textual verification)
> PHASE-2 RULING APPLIED    = BA11-F1 (own hashes external), BA11-F2 (registry is the only status authority), BA11-F3 (normative DEPENDS_ON, seal order,
>                             cascade), BA11-F4 (BASE-18 dependencies), BA11-F11 (BA-01), XC-F8; AR2-16/AR2-17 (pin records), AR2-44
> BASE ITEM                 = BASE-11 (baseline hashes recorded): NOT MET
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 1. Hash procedure (normative)

| Aspect | Rule |
|---|---|
| file hash | SHA-256 of the file bytes after CRLF is normalized to LF, lowercase hexadecimal |
| artifact hash, one file | the file hash |
| artifact hash, several files | SHA-256 of the concatenation, in ascending ordinal path order, of `<path>` LF `<file hash>` LF |
| contract composite hash | SHA-256 over the LF-normalized texts of the V2.1, V2.2, V2.3 and V2.4 blobs in that order, then **one LF**, then the V5 blob id. This is the byte-level procedure that note N-1 of V2.2, D-1 of V2.3 and D-1 of V2.4 cite |
| `CURRENT_HASH` column | the artifact hash of the bytes at the commit of this version, **informational only**; it is not a candidate and not a seal |

## 2. Transitions and authority (normative)

```text
DRAFT  ->  CANDIDATE_FOR_SEALING  ->  RATIFIED  ->  SEALED
```

| Transition | Required | Recorded |
|---|---|---|
| `DRAFT -> CANDIDATE_FOR_SEALING` | an **Architect ruling on the exact bytes** of the artifact, and every entry of its `DEPENDS_ON` at least `CANDIDATE_FOR_SEALING` | `CANDIDATE_HASH` = the artifact hash of those bytes, and the ruling reference, **in this registry** (never in the artifact) |
| `CANDIDATE_FOR_SEALING -> RATIFIED` | a **Coordinator** ratification and an **Architect** ratification, **each citing the exact `CANDIDATE_HASH`**; for BA-03 also the seal conditions of V2.2 PA-14 (the dated re-run and the post-I-57 reading are recorded in the ratification records, **never in BA-03**) | both ratification records (BA-07 V3 section 2), cited here |
| `RATIFIED -> SEALED` | the artifact hash recomputed on the committed bytes equals the `CANDIDATE_HASH`, and every entry of its `DEPENDS_ON` is `SEALED` (their `SEAL_HASH` cited) | `SEAL_HASH` (normative) = `CANDIDATE_HASH` |
| any byte change after a candidate ruling | the candidate and any ratification **expire**; the artifact returns to `DRAFT` as a new version | the expired hash stays in the history |
| **cascade** | a new version of an entry that is `CANDIDATE_FOR_SEALING` or `SEALED` **expires the candidates and seals of every entry that depends on it** (directly or transitively) | the expiry is recorded here |
| change of a sealed artifact | a **new version** (new entry, new candidate, new ratifications, new seal); the old seal is retained and marked `SUPERSEDED` | both entries |

**Authority rules (AR2-44).**

1. **This registry is the only authority** for the status and the hashes of every artifact. The status and hash text inside an artifact is **frozen, non-authoritative text** of its candidate bytes (the V3 artifacts no longer carry `CANDIDATE_HASH` or `SEAL_HASH` fields). An artifact never records its own candidate, ratifications or seal.
2. **BA-11's own hashes live outside BA-11** (BA11-F1): its `CANDIDATE_HASH`, its two ratification records and its `SEAL_HASH` are recorded in the ratification records (BA-07 V3 section 2) and in the custody entries `BASELINE_RECORDED` and `BASELINE_APPROVED` and the anchor record (BA-07 V3 sections 8 and 9). The BA-11 row of section 3 says so and stays constant.
3. **BA-01 is the contract** (BA11-F11): its `CANDIDATE_HASH` and `SEAL_HASH` are the **contract composite hash** (section 4); it passes through the same transitions (the Coordinator's textual verification of V2.4 and the agreement of the effective contract precede its candidate ruling). `BASE-8` and `BASE-16` (criteria) are **`SATISFIED_BY_CONTRACT` at the BA-01 `SEAL_HASH`** and reopen with any new contract version.
4. **The final registry** is sealed **last**, after every other entry is `SEALED`; its seal hash enters the custody genesis entry and the remote anchor; any later change is a new registry version with a new anchor.

A candidate hash is **never normative**; only a `SEAL_HASH` is.

### 2.1 `DEPENDS_ON` and the seal order (normative; BA11-F3)

`DEPENDS_ON` lists **content dependencies**: artifact `X` depends on `Y` when a normative value of `X` is defined in `Y`. A mention of a name defined elsewhere (a fixture name, a scenario id) is not a dependency. The graph is acyclic; the V2 cycle BA-05 <-> BA-08 is broken because the tolerance **values** live only in BA-08 (V2.2 PA-9; AR2-35) and BA-05 names the slots; BA-08 depends on BA-05 and BA-06 (shape classes), not the reverse; the run counts of BA-10 section 2.3 are informative, so BA-10 does not depend on BA-04. **Seal order:**

```text
BA-01  ->  BA-02a, BA-03, BA-05, BA-06, BA-07, BA-10  ->  BA-08  ->  BA-09, BA-04  ->  BA-02b  ->  BA-11 (last)
```

## 3. Registry entries

| Entry | ARTIFACT_ID | Paths | STATUS | CURRENT_HASH (informational) | CANDIDATE_HASH | COORDINATOR | ARCHITECT | SEAL_HASH | DEPENDS_ON | BASE items | Blocker |
|---|---|---|---|---|---|---|---|---|---|---|---|
| BA-01 | authority contract `CT-21D-V2.4` | the four contract documents (section 4) | DRAFT (V2.1..V2.3 agreed for preparation; V2.4 pending textual verification) | composite in section 4 | UNSET | UNSET | UNSET | UNSET | none | BASE-1, BASE-8, BASE-16 (criteria) | none |
| BA-02a | `CT21D-BASE-CONTROL-REGISTRY` | `docs/initiatives/I-52-ct21d-baseline-ba-02a-control-registry-v3.md` | DRAFT | `acb5e01de3c1b722e9757bfb26aafb6a707599b37dd2902e4c64676c1bd2e3c3` | UNSET | UNSET | UNSET | UNSET | BA-01 | BASE-19 | none |
| BA-03 | `SELECTIVE_VIEW_FAMILY_SCOPE_V1` | `docs/initiatives/I-52-ct21d-baseline-ba-03-selective-scope-v3.md`<br>`docs/automation/evidence/I-52-ct21d-ba03-v3-checks.json`<br>`docs/automation/evidence/I-52-ct21d-ba03-v3-checks.py` | DRAFT | `2fe86f8c802ed053980a39c3c2de9d9523a6429990d56cdc73bed3eb4c1126ee` | UNSET | UNSET | UNSET | UNSET | BA-01 | BASE-12 | NB-1 |
| BA-05 | `CT21D-BASE-FINGERPRINT-SPEC (FPSPEC-V1)` | `docs/initiatives/I-52-ct21d-baseline-ba-05-fingerprint-specification-v3.md`<br>`docs/automation/evidence/I-52-ct21d-fpspec-v1-vectors-v3.gen.py`<br>`docs/automation/evidence/I-52-ct21d-fpspec-v1-vectors-v3.json` | DRAFT | `689a937fff4b6edd62a7d30ace650efada456bff0c21f94dc10d4c5d61af4773` | UNSET | UNSET | UNSET | UNSET | BA-01; library census (host) as a draft update before the candidate (BA-05 V3 section 2.6) | BASE-14 | NB-3 |
| BA-06 | `CT21D-BASE-EVM-PREPARATION` | `docs/initiatives/I-52-ct21d-baseline-ba-06-evm-preparation-v3.md` | DRAFT | `88339a2f037e95214d7b2a3881768d45ecc373759d5f96a370dbd010eccb7792` | UNSET | UNSET | UNSET | UNSET | BA-01; the evidence files bound inside it (section 14) | BASE-15, BASE-5 | NB-4 |
| BA-07 | `CT21D-BASE-MANIFEST-CUSTODY` | `docs/initiatives/I-52-ct21d-baseline-ba-07-manifest-custody-v3.md` | DRAFT | `759edd948c74837e4a4f7d1434046d9565a8a315a3d91f94cc660b9716de45c0` | UNSET | UNSET | UNSET | UNSET | BA-01 | BASE-17 | NB-6 |
| BA-10 | `CT21D-BASE-PARAMETER-SHEET` | `docs/initiatives/I-52-ct21d-baseline-ba-10-parameter-sheet-v3.md` | DRAFT | `edc20a02dc8deec3e40135ef91a2734e23aadccc40fbf3ea9a058cfbe9ba55b3` | UNSET | UNSET | UNSET | UNSET | BA-01; Owner decisions Q-O1..Q-O5; Coordinator values PARAM-04 and EVIDENCE_REPETITION; the machine-attribute observation for PARAM-05 (future host gate) | BASE-9, BASE-18 | none of its own |
| BA-08 | `SELECTIVE_KIND_AUTHORITY_BASELINE` | `docs/initiatives/I-52-ct21d-baseline-ba-08-selective-kind-baseline-v3.md` | DRAFT | `7e0811b2d87028e543a6e0bc81fcc4f7ace89a7abb54fc9cb44e7293f4ada8a1` | UNSET | UNSET | UNSET | UNSET | BA-01, BA-03, BA-05, BA-06; library census (host, K-2); TOL_SCALE decision, test and record (K-3); FX-IMP construction (K-8) | BASE-2, BASE-3, BASE-4, BASE-6, BASE-7, BASE-10, BASE-18 (tolerances) | none of its own |
| BA-09 | `CT21D-BASE-WARMUP` | `docs/initiatives/I-52-ct21d-baseline-ba-09-warmup-v3.md` | DRAFT | `7305c727e82f3dd0554710dbfcd96e21b5ab1b0903f1b9c2719f7f3a35fbea9e` | UNSET | UNSET | UNSET | UNSET | BA-01, BA-06, BA-07, BA-08; the Architect ruling WU-Q1 | BASE-5, BASE-10 | none of its own |
| BA-04 | `CT21D-BASE-SCENARIO-CATALOG` | `docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v3.md`<br>`docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v3.json` | DRAFT | `e47b3ae2dd809209a1f6067ade5ea5adc1dd59cee61b4d4130f064b4b177b242` | UNSET | UNSET | UNSET | UNSET | BA-02a, BA-03, BA-05, BA-06, BA-08, BA-10 | BASE-13, BASE-16 (scenarios) | NB-2 |
| BA-02b | `CT21D-BASE-COVERAGE-MATRIX` | `docs/initiatives/I-52-ct21d-baseline-ba-02b-coverage-matrix-v3.md` | DRAFT | `8adf2cabb21278051ed2642a216b0be744e75480738e3e3a117911ee601b6314` | UNSET | UNSET | UNSET | UNSET | BA-02a, BA-04 | BASE-20 | NB-2 |
| BA-11 | `CT21D-BASE-HASH-REGISTRY` (this file) | this file | DRAFT | recorded outside (BA-07 V3 sections 8 and 9) | recorded outside | recorded outside | recorded outside | recorded outside | every other entry | BASE-11 | none of its own |

### 3.1 Pin records (AR2-16, AR2-17)

A pin is a named slot in the sealed catalog (BA-04 V3 section 2.1). Its value lives in a **pin record** with its own entry below, its own seal and its custody entries (`PIN_RECORDED`, `ANCHORED`, BA-07 V3). Writing a pin never changes the bytes of the catalog; a pin is written after the value is frozen from its own evidence and before the first governing run that uses it.

| Pin slot | Value | Source | STATUS | PIN_RECORD_HASH |
|---|---|---|---|---|
| `E04_ADMITTED_SET@R-1` | the E-04 admitted set of the designated route | E4-LEARN-R1-* under the selected LOCK_MODE (AR2-11) | UNSET | UNSET |
| `LOCK_MODE` | the selected lock mode | the selection review of V2.1 3.4 over LK-* | UNSET | UNSET |
| `EVM_FROZEN` | the frozen event model | learning after REVIEWED (V2.4; V2.1 27.2) | UNSET | UNSET |
| `HOST_DEFAULT_MAP` | the host-default attribute map (BA-08 V3 section 11 rule 5) | non-governing characterization | UNSET | UNSET |
| `FIXTURE_INSTANCE@<FX>` | the hash of each fixture instance (template drawing and library copy) | host work under a future gate, with conformance to BA-08 V3 section 12 recorded | UNSET | UNSET |

`TOL_SCALE` is **not** a pin: it is a value of the Selective kind baseline (V2.2 PA-9, BA-08 V3 section 10) without authority today (K-3).

### 3.2 Bound items that are not artifacts of this registry

| Item | Kind | Status |
|---|---|---|
| block-library file (path, SHA-256) | tuple field (BUILD_BOUND) | UNSET: fixed for the exact build at execution preparation; the file is not versioned in the repository |
| catalog folder files (SHA-256 each) | tuple field (BUILD_BOUND; BA-08 V3 PH2-TP-1) | UNSET: fixed for the exact build |
| library entity-type census | input of BA-05 and BA-08 | NOT PERFORMED (host; not authorized) |
| machine-attribute observation | input of the PARAM-05 label (BA-10 V3) | NOT PERFORMED (host; not authorized) |
| manifest and custody log | execution prerequisite EXEC-11 | NOT CAPTURED |
| baseline anchor | BA-07 V3 section 9 | NOT CREATED (created only after every entry is sealed) |

## 4. Contract identity

| Document | Git blob |
|---|---|
| `docs/initiatives/I-52-ct21d-authority-contract-v2.1.md` | `6259a3beb7445a63620636acd696cdad689965b3` |
| `docs/initiatives/I-52-ct21d-authority-contract-v2.2.md` | `fd7260c57ec05e11f6bbb9f67a728e7dfdf1f609` |
| `docs/initiatives/I-52-ct21d-authority-contract-v2.3.md` | `bc3d67a957b784a874b5bab97ce22116af1eed70` |
| `docs/initiatives/I-52-ct21d-authority-contract-v2.4.md` | `fc137c52df8571b946e98697d59ef25447a742fb` |
| Proposal ALT-21D V5 (guarantee authority) | `f73157d9ce2d5013f4f91fb2d4f4d127a4f879a1` |

```text
CANDIDATE_COMPOSITE_HASH (V2.1 + V2.2 + V2.3 + V2.4 draft revision 2 + V5 blob id; informational until the BA-01 candidate ruling)
  = b308d8ce7b8e03c654038405df96f453a4ef70857ba6980e7fd44cc4140b88f4
```

## 5. `BASE-1 .. BASE-20` after the phase-2 delta

| Base | Requirement | State |
|---|---|---|
| BASE-1 | the authority contract is complete and agreed | V2.1..V2.3 agreed for preparation; V2.4 (draft revision 2) pending textual verification; BA-01 not a candidate |
| BASE-2 | the Selective kind authority design is complete and agreed | BA-08 V3 draft; K-2, K-3, K-5, K-6, K-8 open |
| BASE-3 | E-04 criteria fixed | criteria in the contract; route R-1 pending ratification (K-5); values are the pin `E04_ADMITTED_SET@R-1` |
| BASE-4 | `LOCK_MODE` candidate contract fixed | candidate set LM-1/LM-2 sufficient; selection is the pin `LOCK_MODE` |
| BASE-5 | event instrumentation designed | BA-06 V3 (`STATIC_REVIEWED` proposed again) and BA-09 V3 (WU-Q1 open) |
| BASE-6 | abort read-set fixed | specification in BA-08 V3 section 6; the MISSING readers are execution blockers (K-7) |
| BASE-7 | composition and reuse rules fixed | rules in BA-08 V3; library census (K-2) pending |
| BASE-8 | UNDO evidence plan fixed | **SATISFIED_BY_CONTRACT at the BA-01 `SEAL_HASH`** (V2.1 16); pending that seal; reopens with a new contract version |
| BASE-9 | governing-result, retry and attempt-log policies fixed | policies in the contract; `MAX_ATTEMPTS` in BASE-18 |
| BASE-10 | every `BEFORE_CT21D_BASELINE` item resolved | BA-08 V3, BA-09 V3 drafts |
| BASE-11 | baseline hashes recorded | this registry; nothing is a candidate |
| BASE-12 | OD-1 closed (sealed Selective scope) | BA-03 V3 prepared for candidate |
| BASE-13 | scenario catalog complete with expected results and authority | BA-04 V3 draft; K-3 and K-8 block the declaration of some expectations |
| BASE-14 | fingerprint specification agreed | BA-05 V3 draft with normative vectors; census pending |
| BASE-15 | E-12 procedure and corpus definition agreed | BA-06 V3 draft; `STATIC_REVIEWED` (V2.4 D-3) pending the Architect |
| BASE-16 | instrument qualification acceptance criteria agreed | criteria **SATISFIED_BY_CONTRACT at the BA-01 `SEAL_HASH`** (V2.1 4.3, 4.3.1, V2.2 PA-12); the `Q-*` scenarios are sealed with the catalog (BASE-13) |
| BASE-17 | manifest capture and custody procedure agreed | BA-07 V3 prepared for candidate (the independent copy to be designated by the Coordinator) |
| BASE-18 | classified parameters fixed | BA-10 V3 all `UNSET`. Needs: **Owner** Q-O1..Q-O5; **Coordinator** PARAM-04 cap and `EVIDENCE_REPETITION`; a **machine-attribute observation** (host gate, not authorized) for the PARAM-05 label; the **BA-08 V3 tolerances with the `TOL_SCALE` decision, test and record** (K-3; the test needs non-governing host evidence) (BA11-F4) |
| BASE-19 | control-class registry agreed | BA-02a V3 prepared for candidate |
| BASE-20 | coverage matrix agreed | BA-02b V3 draft (0 gaps in draft); depends on the catalog |

```text
BASE-11 = NOT MET     SEALED ENTRIES = 0     CANDIDATE ENTRIES = 0
CT21D_AUTHORITY_BASELINE_READY = FALSE
```
