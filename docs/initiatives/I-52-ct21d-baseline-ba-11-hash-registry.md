# I-52 — CT-21D Baseline Artifact BA-11: Baseline Hash Registry (SKELETON V1)

> **BASELINE ARTIFACT BA-11 — SKELETON, NOT SEALED, NOT AGREED. Design / documentation only. It binds nothing yet: every seal hash of a phase-1 artifact is `UNSET`.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-HASH-REGISTRY
> ARTIFACT_VERSION          = 1-SKELETON
> ARTIFACT_STATUS           = SKELETON (phase 1 of baseline preparation)
> REGISTRY_HASH             = not stored inside this file; recorded in the anchor commit at the seal (section 5)
> AUTHORITY_CONTRACT        = CT-21D V2.1 + V2.2 + V2.3 (AGREED_FOR_BASELINE_ARTIFACT_PREPARATION)
> BASE ITEM                 = BASE-11 (baseline hashes recorded): NOT MET
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 1. Purpose

This registry is the structure that will eventually **bind** the authority contract and every baseline artifact by hash (BASE-1 .. BASE-20). **No hash is recorded for an artifact that is not finalized.** The only hashes filled in are those of documents that already exist and are agreed (the contract documents); everything else is `UNSET`.

## 2. Hash procedure (normative for this registry)

| Aspect | Rule |
|---|---|
| file hash | SHA-256 of the file content as UTF-8 with **LF** line endings (CRLF normalized to LF), lowercase hexadecimal |
| artifact hash (one file) | the file hash |
| artifact hash (several files) | SHA-256 of the concatenation, in ascending path order, of the lines `<path>` + LF + `<file hash>` + LF |
| contract composite hash | SHA-256 over the LF-normalized text of the V2.1 blob, then the V2.2 blob, then the V2.3 blob (the composite identity approach of note N-1, extended by V2.3 as V2.3 deviation D-1), then LF, then the V5 blob id |
| when a hash is recorded | at the **seal** of the artifact, after ratification; a draft is never sealed |
| binding | the registry entry of an artifact is written **only** together with its Coordinator and Architect ratification records |
| change | any change of a sealed artifact is a **new version** with a new entry; a superseded entry is retained |
| the registry itself | its own hash is not stored inside it; it is recorded in the anchor commit of section 5 |

## 3. Registry entries

`STATUS` values: `AGREED_FOR_BASELINE_ARTIFACT_PREPARATION`, `DRAFT_PHASE1`, `SEALED`. `SEAL_HASH = UNSET` until sealed. `COORDINATOR` and `ARCHITECT` hold the ratification record (name, date, artifact hash) or `UNSET`.

| Entry | ARTIFACT_ID | Paths | STATUS | SEAL_HASH | COORDINATOR | ARCHITECT | DEPENDS_ON | BASE items | Blocker |
|---|---|---|---|---|---|---|---|---|---|
| BA-01 | authority contract `CT-21D-V2.3` (V2.1 + V2.2 + V2.3) | the three contract documents (section 4) | AGREED_FOR_BASELINE_ARTIFACT_PREPARATION | composite computed, **not yet recorded at baseline** (section 4) | ruling `AGREED_FOR_BASELINE_ARTIFACT_PREPARATION` (recorded in the phase-1 gate order) | V2.1 check and N-2 micro-ruling done; no further review required for literal application | none | BASE-1 | none |
| BA-02 | `CT21D-BASE-CONTROL-REGISTRY` | `docs/initiatives/I-52-ct21d-baseline-ba-02-control-registry-and-coverage.md` | DRAFT_PHASE1 | UNSET | UNSET | UNSET | BA-01, BA-04 | BASE-19, BASE-20 | none of its own |
| BA-03 | `SELECTIVE_VIEW_FAMILY_SCOPE_V1` | `docs/initiatives/I-52-ct21d-baseline-ba-03-selective-scope.md` | DRAFT_PHASE1 | UNSET | UNSET | UNSET | BA-01 | BASE-12 | NB-1 |
| BA-04 | `CT21D-BASE-SCENARIO-CATALOG` | `docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog.md` and `docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog.json` | DRAFT_PHASE1 | UNSET | UNSET | UNSET | BA-02, BA-03, BA-08, BA-10 | BASE-13 | NB-2 |
| BA-05 | `CT21D-BASE-FINGERPRINT-SPEC` (`FPSPEC-V1`) | `docs/initiatives/I-52-ct21d-baseline-ba-05-fingerprint-specification.md` | DRAFT_PHASE1 | UNSET | UNSET | UNSET | BA-01, BA-08 (tolerance values) | BASE-14 | NB-3 |
| BA-06 | `CT21D-BASE-EVM-PREPARATION` | `docs/initiatives/I-52-ct21d-baseline-ba-06-evm-preparation.md` | DRAFT_PHASE1 | UNSET | UNSET | UNSET | BA-01, BA-08, BA-10 | BASE-15, BASE-5 | NB-4 |
| BA-07 | `CT21D-BASE-MANIFEST-CUSTODY` | `docs/initiatives/I-52-ct21d-baseline-ba-07-manifest-custody.md` | DRAFT_PHASE1 | UNSET | UNSET | UNSET | BA-01 | BASE-17 | NB-6 |
| BA-08 | `SELECTIVE_KIND_AUTHORITY_BASELINE` | `docs/initiatives/I-52-ct21d-baseline-ba-08-selective-kind-baseline.md` | DRAFT_PHASE1 | UNSET | UNSET | UNSET | BA-01, BA-03, BA-05 | BASE-2, BASE-3, BASE-4, BASE-6, BASE-7, BASE-10 | none of its own |
| BA-09 | `CT21D-BASE-WARMUP` | `docs/initiatives/I-52-ct21d-baseline-ba-09-warmup.md` | DRAFT_PHASE1 | UNSET | UNSET | UNSET | BA-01, BA-08, BA-10 | BASE-5, BASE-10 | none of its own |
| BA-10 | `CT21D-BASE-PARAMETER-SHEET` | `docs/initiatives/I-52-ct21d-baseline-ba-10-parameter-sheet.md` | DRAFT_PHASE1 | UNSET | UNSET | UNSET | BA-01 | BASE-9, BASE-18 | none of its own |
| BA-11 | `CT21D-BASE-HASH-REGISTRY` (this file) | `docs/initiatives/I-52-ct21d-baseline-ba-11-hash-registry.md` | SKELETON | not stored inside | UNSET | UNSET | BA-01..BA-10 | BASE-11 | none of its own |

### 3.1 Bound items that are **not** phase-1 artifacts (recorded here so that nothing is forgotten)

| Item | Kind | Status | Why not now |
|---|---|---|---|
| block-library file path and SHA-256 | tuple field (BUILD_BOUND) | UNSET | fixed for the exact build at execution preparation; the library file is not versioned in the repository (V17 L-28) |
| manifest and its custody log | execution prerequisite EXEC-11 | NOT CAPTURED | the capture happens on the exact build and machine at execution preparation |
| manifest custody anchor record | BA-07 placeholder | NOT CREATED | created at baseline preparation together with the remote publication |
| learned event model (`EVM-Selective-vX`) | product of authorized characterization | DOES NOT EXIST | produced by execution; the baseline needs only the procedure, the corpora definitions, the schema and the completeness review (BA-06) |
| evidence-derived values (E-04 admitted set, `LOCK_MODE` choice) | evidence-derived | UNSET | fixed before Owner Act 2 |
| scenario expected results sealed (`SEALED_AT`) | BA-04 | UNSET in every scenario | sealed per scenario, before its first governing run |

## 4. Contract document identities (the only real hashes in this skeleton)

| Document | Path | Git blob id |
|---|---|---|
| V2.1 | `docs/initiatives/I-52-ct21d-authority-contract-v2.1.md` | `6259a3beb7445a63620636acd696cdad689965b3` |
| V2.2 | `docs/initiatives/I-52-ct21d-authority-contract-v2.2.md` | `fd7260c57ec05e11f6bbb9f67a728e7dfdf1f609` |
| V2.3 | `docs/initiatives/I-52-ct21d-authority-contract-v2.3.md` | `bc3d67a957b784a874b5bab97ce22116af1eed70` |
| Proposal ALT-21D V5 (guarantee authority) | `docs/initiatives/I-52-alt21d-reduced-guarantee-proposal-v5.md` | `f73157d9ce2d5013f4f91fb2d4f4d127a4f879a1` |

```text
CANDIDATE_COMPOSITE_HASH (computed on the tree of 01194bb9590b7fc3c03836b5ea9a4ea9be701157, informational until the baseline record)
  = f2ea8f1237e6b8afea654c0898437814f725d85dfe92f009b597c08ff472c9d5
```

The composite is deterministic from the four blobs above (procedure of section 2). It is **not** the recorded contract hash: `Contract hash = TO_BE_RECORDED_AT_BASELINE`.

## 5. Anchor and seal order

1. The artifacts are sealed in dependency order: BA-10, BA-07, BA-03, BA-05 (slots), BA-08 (with the tolerance values), BA-02, BA-04, BA-06, BA-09, then BA-11.
2. BA-11 is completed last with every seal hash and every ratification record.
3. The hash of BA-11 is written into the custody anchor record and published at the remote (BA-07 section 8); that publication is the **immutable reference** of the baseline.

## 6. `BASE-1 .. BASE-20`: state after phase 1

| Base | Requirement (V2.1 section 22 and V2.2) | Artifact | State after phase 1 |
|---|---|---|---|
| BASE-1 | the authority contract is complete and agreed | BA-01 | agreed for baseline artifact preparation; hash not recorded |
| BASE-2 | the Selective kind authority design is complete and agreed | BA-08 | design at contract level; concrete details drafted; agreement pending |
| BASE-3 | E-04 criteria fixed | BA-08 section 8 | criteria and candidates drafted; values evidence-derived and UNSET |
| BASE-4 | `LOCK_MODE` candidate contract fixed | BA-08 section 9 | candidates drafted; selection UNSET |
| BASE-5 | event instrumentation designed | BA-06, BA-09 | drafted; agreement pending |
| BASE-6 | abort read-set fixed | BA-08 section 6 | drafted; readers MISSING (execution blocker EXEC-4) |
| BASE-7 | composition and reuse rules fixed | BA-08 section 7 | drafted; library census missing |
| BASE-8 | UNDO evidence plan fixed | contract section 16 | in the contract; no separate artifact |
| BASE-9 | governing-result, retry and attempt-log policies fixed | BA-10 | policies in the contract; parameter values UNSET |
| BASE-10 | every `BEFORE_CT21D_BASELINE` item resolved | BA-08, BA-09 | drafted; K-1..K-5 of BA-08 open |
| BASE-11 | baseline hashes recorded | BA-11 | skeleton only |
| BASE-12 | OD-1 closed (sealed Selective scope) | BA-03 | drafted; not ratified; not sealed |
| BASE-13 | scenario catalog complete with expected results and authority | BA-04 | draft (115 scenarios, 0 sealed); class entries missing |
| BASE-14 | FINGERPRINT SPECIFICATION agreed | BA-05 | drafted; open items L-1..L-4 |
| BASE-15 | E-12 procedure and corpus definition agreed | BA-06 | drafted; completeness review not performed |
| BASE-16 | instrument qualification acceptance criteria agreed | contract 4.3, 4.3.1; BA-04 `Q-*` | criteria in the contract; scenarios drafted |
| BASE-17 | manifest capture and custody procedure agreed | BA-07 | drafted; agreement, evidence location and anchor remote pending |
| BASE-18 | classified parameters fixed | BA-10 | all UNSET |
| BASE-19 | `CONTROL_CLASS_REGISTRY` agreed | BA-02 | drafted; 0 UNRESOLVED; agreement pending |
| BASE-20 | coverage matrix agreed | BA-02 section 4 | drafted; class entries pending |

## 7. Exact items that still block `CT21D_AUTHORITY_BASELINE_READY = TRUE`

1. **NB-1**: Coordinator and Architect ratification of BA-03, the re-execution of the authority comparison at sealing, and the seal hash.
2. **NB-2**: the scenario catalog completed (fixtures, the class entries of BA-04 section 5, resolution of gaps G-1 to G-4) and reviewed and agreed; expected results sealed.
3. **NB-3**: the library entity-type census, the property lists of BA-05, the tolerance values (through BA-08), the hash and the agreement of BA-05.
4. **NB-4**: the independent completeness review of the seam-operation inventory by a non-compiler, the fixtures, the plan quantity vector and shape classes, the hash and the agreement of BA-06.
5. **NB-6**: agreement of BA-07, the evidence location, the anchor remote and the `CUSTODY_ROLE` holder.
6. **BASE-18**: the values and justifications of `PARAM-01` .. `PARAM-05`.
7. **BASE-8, BASE-16**: agreement of the plans and criteria that stay inside the contract.
8. **BASE-19, BASE-20**: agreement of the registry and the matrix.
9. **BASE-2, BASE-10**: the fixtures, the library census and the agreement of BA-08.
10. **BASE-11 and BASE-1**: every seal hash and the contract hash recorded in this registry, with the anchor published at the remote.

## 8. Status

```text
BASE-11 = NOT MET (skeleton only)
SEALED ARTIFACTS = 0 of 10 phase-1 artifacts
CT21D_AUTHORITY_BASELINE_READY = FALSE
CT21D_EXECUTION_READY = FALSE
CT21D_EXECUTION = NOT_AUTHORIZED
```
