# I-52 — CT-21D Baseline Artifact BA-11: Baseline Hash Registry and Sealing Workflow (DRAFT V2)

> **BASELINE ARTIFACT BA-11 V2 — DRAFT. It binds nothing yet: no artifact is sealed and no seal hash exists.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-HASH-REGISTRY
> ARTIFACT_VERSION          = 2-DRAFT   (supersedes 1-SKELETON, blob a549bd1b9dfb3c41261d3b45cf5b5bd17464993a, which stays as history)
> ARTIFACT_STATUS           = DRAFT (phase 2); the FINAL registry is the LAST baseline artifact to be sealed
> AUTHORITY_CONTRACT        = CT-21D V2.1 + V2.2 + V2.3 + V2.4 (V2.4 pending Coordinator textual verification)
> PHASE-1 RULING APPLIED    = CANDIDATE_HASH column; DRAFT -> CANDIDATE_FOR_SEALING -> RATIFIED -> SEALED; BASE-8 satisfied by the contract; BASE-16 criteria satisfied by the contract
> BASE ITEM                 = BASE-11 (baseline hashes recorded): NOT MET
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 1. Hash procedure (normative)

| Aspect | Rule |
|---|---|
| file hash | SHA-256 of the file bytes after CRLF is normalized to LF, lowercase hexadecimal |
| artifact hash, one file | the file hash |
| artifact hash, several files | SHA-256 of the concatenation, in ascending ordinal path order, of `<path>` LF `<file hash>` LF |
| contract composite hash | SHA-256 over the LF-normalized texts of the V2.1, V2.2, V2.3 and V2.4 blobs in that order, then LF, then the V5 blob id (N-1, V2.3 D-1, V2.4 D-1) |
| `CURRENT_HASH` column | the artifact hash of the bytes in this commit, **informational only**; it is not a candidate and not a seal |

## 2. Transitions (normative)

```text
DRAFT  ->  CANDIDATE_FOR_SEALING  ->  RATIFIED  ->  SEALED
```

| Transition | Required | Recorded |
|---|---|---|
| `DRAFT -> CANDIDATE_FOR_SEALING` | an **Architect ruling on the exact bytes** of the artifact | `CANDIDATE_HASH` = the artifact hash of those bytes, and the ruling reference |
| `CANDIDATE_FOR_SEALING -> RATIFIED` | a **Coordinator** ratification and an **Architect** ratification, **each citing the exact `CANDIDATE_HASH`**; for BA-03 also the seal conditions of V2.2 PA-14 (dated re-run, complete post-I-57 reading, no conflict, O-1 statement) | both ratification records (role, date, cited hash) |
| `RATIFIED -> SEALED` | the artifact hash recomputed on the committed bytes equals the `CANDIDATE_HASH` | `SEAL_HASH` (normative) = `CANDIDATE_HASH` |
| any byte change after a candidate ruling | the candidate and any ratification **expire**; the artifact returns to `DRAFT` as a new version | the expired hash stays in the history |
| change of a sealed artifact | a **new version** (new entry, new candidate, new ratifications, new seal); the old seal is retained and marked `SUPERSEDED` | both entries |
| the final registry | sealed **last**, after every other entry is `SEALED`; its hash enters the custody genesis record and the remote anchor (BA-07 V2 sections 8 and 9); any later change is a new registry version with a new anchor | the anchor record |

A candidate hash is **never normative**; only a `SEAL_HASH` is.

## 3. Registry entries

| Entry | ARTIFACT_ID | Paths | STATUS | CURRENT_HASH (informational) | CANDIDATE_HASH | COORDINATOR | ARCHITECT | SEAL_HASH | DEPENDS_ON | BASE items | Blocker |
|---|---|---|---|---|---|---|---|---|---|---|---|
| BA-01 | authority contract `CT-21D-V2.4` | the four contract documents (section 4) | V2.1..V2.3 `AGREED_FOR_BASELINE_ARTIFACT_PREPARATION`; V2.4 pending textual verification | composite in section 4 | UNSET | UNSET | UNSET | UNSET | none | BASE-1 | none |
| BA-02a | `CT21D-BASE-CONTROL-REGISTRY` | `docs/initiatives/I-52-ct21d-baseline-ba-02a-control-registry-v2.md` | DRAFT | `8a19d0adca3989b1694e24b2a10012ab77264f2d30dfde431f6d8d612bd95293` | UNSET | UNSET | UNSET | UNSET | BA-01 | BASE-19 | none |
| BA-02b | `CT21D-BASE-COVERAGE-MATRIX` | `docs/initiatives/I-52-ct21d-baseline-ba-02b-coverage-matrix-v2.md` | DRAFT | `87c2082c3e0c4eb98d2634048e3c89c5869b1f99ae5d9b37fe60946b3961d11d` | UNSET | UNSET | UNSET | UNSET | BA-02a, BA-04 | BASE-20 | NB-2 |
| BA-03 | `SELECTIVE_VIEW_FAMILY_SCOPE_V1` | `docs/initiatives/I-52-ct21d-baseline-ba-03-selective-scope-v2.md` | DRAFT | `af3f8810a984a470319b96ca29cfd02b6423c08efa5a1b46e83979c30e9017e1` | UNSET | UNSET | UNSET | UNSET | BA-01 | BASE-12 | NB-1 |
| BA-04 | `CT21D-BASE-SCENARIO-CATALOG` | `docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v2.md`<br>`docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v2.json` | DRAFT | `7386e2534734809931f194a8ce73081554d296a2e5bdcde9218440a0bd6bb9e1` | UNSET | UNSET | UNSET | UNSET | BA-02a, BA-03, BA-08, BA-10 | BASE-13, BASE-16 (scenarios) | NB-2 |
| BA-05 | `CT21D-BASE-FINGERPRINT-SPEC (FPSPEC-V1)` | `docs/initiatives/I-52-ct21d-baseline-ba-05-fingerprint-specification-v2.md`<br>`docs/automation/evidence/I-52-ct21d-fpspec-v1-vectors.gen.py`<br>`docs/automation/evidence/I-52-ct21d-fpspec-v1-vectors.json` | DRAFT | `97d77113c4ca1d1b8fc74db59a0771933938c3fa7830d97b91881feed3feaf89` | UNSET | UNSET | UNSET | UNSET | BA-01, BA-08 (tolerances), library census (host) | BASE-14 | NB-3 |
| BA-06 | `CT21D-BASE-EVM-PREPARATION` | `docs/initiatives/I-52-ct21d-baseline-ba-06-evm-preparation-v2.md` | DRAFT | `f016cd705db1defa2c1a63a0ea9756cbdee96777a03ec9880265c18e512b09b2` | UNSET | UNSET | UNSET | UNSET | BA-01, BA-08, BA-10 | BASE-15, BASE-5 | NB-4 |
| BA-07 | `CT21D-BASE-MANIFEST-CUSTODY` | `docs/initiatives/I-52-ct21d-baseline-ba-07-manifest-custody-v2.md` | DRAFT | `2e15cbe55ccaa86dba26b857d2719010655ef3621088b9ae630ed31cc452c37c` | UNSET | UNSET | UNSET | UNSET | BA-01 | BASE-17 | NB-6 |
| BA-08 | `SELECTIVE_KIND_AUTHORITY_BASELINE` | `docs/initiatives/I-52-ct21d-baseline-ba-08-selective-kind-baseline-v2.md` | DRAFT | `82a1bb70a76266bd64a1cbbe808c1257da92748cdc25c609c744b7410b8e2fc4` | UNSET | UNSET | UNSET | UNSET | BA-01, BA-03, BA-05 | BASE-2, BASE-3, BASE-4, BASE-6, BASE-7, BASE-10 | none of its own |
| BA-09 | `CT21D-BASE-WARMUP` | `docs/initiatives/I-52-ct21d-baseline-ba-09-warmup-v2.md` | DRAFT | `ea00f8912e76ba8f08e83bedcab90d0dcbc48cfa5d39fd3bcb4fe5d8f4648506` | UNSET | UNSET | UNSET | UNSET | BA-01, BA-06, BA-08, BA-10 | BASE-5, BASE-10 | none of its own |
| BA-10 | `CT21D-BASE-PARAMETER-SHEET` | `docs/initiatives/I-52-ct21d-baseline-ba-10-parameter-sheet-v2.md` | DRAFT | `fde772fc48442e9f5d0bfc322bcddb84b18cad044444833349ff2a9593aeed28` | UNSET | UNSET | UNSET | UNSET | BA-01, Owner decisions Q-O1..Q-O5 | BASE-9, BASE-18 | none of its own |
| BA-11 | `CT21D-BASE-HASH-REGISTRY` (this file) | this file | DRAFT | not stored inside | UNSET | UNSET | UNSET | UNSET | every other entry | BASE-11 | none of its own |

### 3.1 Bound items that are not phase-2 artifacts

| Item | Kind | Status |
|---|---|---|
| block-library file (path, SHA-256) | tuple field (BUILD_BOUND) | UNSET: fixed for the exact build at execution preparation; the file is not versioned in the repository |
| library entity-type census | input of BA-05 and BA-08 | NOT PERFORMED (host; not authorized) |
| manifest and custody log | execution prerequisite EXEC-11 | NOT CAPTURED |
| baseline anchor | BA-07 V2 section 9 | NOT CREATED (created only after every entry is sealed) |
| learned event model | product of authorized characterization | DOES NOT EXIST |
| evidence-derived values (E-04 admitted set, `LOCK_MODE` choice) | pinned later (`SEALED_PENDING_PIN` scenarios) | UNSET |

## 4. Contract identity

| Document | Git blob (or pending) |
|---|---|
| `docs/initiatives/I-52-ct21d-authority-contract-v2.1.md` | `6259a3beb7445a63620636acd696cdad689965b3` |
| `docs/initiatives/I-52-ct21d-authority-contract-v2.2.md` | `fd7260c57ec05e11f6bbb9f67a728e7dfdf1f609` |
| `docs/initiatives/I-52-ct21d-authority-contract-v2.3.md` | `bc3d67a957b784a874b5bab97ce22116af1eed70` |
| `docs/initiatives/I-52-ct21d-authority-contract-v2.4.md` | `1919a32abcb1690238c4ec9b8588deaaaa032233` |
| Proposal ALT-21D V5 (guarantee authority) | `f73157d9ce2d5013f4f91fb2d4f4d127a4f879a1` |

```text
CANDIDATE_COMPOSITE_HASH (V2.1 + V2.2 + V2.3 + V2.4 + V5 blob id; informational until the baseline record)
  = 74725e4d4bbf285b1dbaf0c53cdb428d9e143784d6433cc8771f97f66cd26d12
```

## 5. `BASE-1 .. BASE-20` after phase 2

| Base | Requirement | State |
|---|---|---|
| BASE-1 | the authority contract is complete and agreed | V2.1..V2.3 agreed for preparation; V2.4 pending textual verification; composite hash not recorded |
| BASE-2 | the Selective kind authority design is complete and agreed | BA-08 V2 draft (fixtures, oracle, tolerance mapping, route); `TOL_SCALE` and census pending |
| BASE-3 | E-04 criteria fixed | criteria in the contract; designated route R-1 pending Coordinator ratification; values evidence-derived |
| BASE-4 | `LOCK_MODE` candidate contract fixed | candidate set LM-1/LM-2 sufficient (Architect); selection evidence-derived |
| BASE-5 | event instrumentation designed | BA-06 V2 and BA-09 V2 drafts; `STATIC_REVIEWED` pending (V2.4) |
| BASE-6 | abort read-set fixed | specification in BA-08 V2; readers MISSING are execution blockers |
| BASE-7 | composition and reuse rules fixed | rules in BA-08 V2; library census (host) pending |
| BASE-8 | UNDO evidence plan fixed | **SATISFIED_BY_CONTRACT** (V2.1 16, as agreed; no separate artifact) |
| BASE-9 | governing-result, retry and attempt-log policies fixed | policies in the contract; `MAX_ATTEMPTS` value in BASE-18 |
| BASE-10 | every `BEFORE_CT21D_BASELINE` item resolved | BA-08 V2, BA-09 V2 drafts |
| BASE-11 | baseline hashes recorded | this registry; nothing sealed |
| BASE-12 | OD-1 closed (sealed Selective scope) | BA-03 V2 prepared for candidate |
| BASE-13 | scenario catalog complete with expected results and authority | BA-04 V2 draft; not sealed |
| BASE-14 | fingerprint specification agreed | BA-05 V2 draft with normative vectors; census and `TOL_SCALE` pending |
| BASE-15 | E-12 procedure and corpus definition agreed | BA-06 V2 draft; `STATIC_REVIEWED` pending review |
| BASE-16 | instrument qualification acceptance criteria agreed | criteria **SATISFIED_BY_CONTRACT** (V2.1 4.3, 4.3.1, V2.2 PA-12); the `Q-*` scenarios are sealed with the catalog (BASE-13) |
| BASE-17 | manifest capture and custody procedure agreed | BA-07 V2 draft |
| BASE-18 | classified parameters fixed | BA-10 V2: all `UNSET`; Owner decisions Q-O1..Q-O5 needed |
| BASE-19 | control-class registry agreed | BA-02a V2 prepared for candidate |
| BASE-20 | coverage matrix agreed | BA-02b V2 draft; depends on the catalog |

```text
BASE-11 = NOT MET     SEALED ENTRIES = 0     CANDIDATE ENTRIES = 0
CT21D_AUTHORITY_BASELINE_READY = FALSE
```
