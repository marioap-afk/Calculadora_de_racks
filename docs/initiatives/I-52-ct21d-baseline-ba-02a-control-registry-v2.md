# I-52 — CT-21D Baseline Artifact BA-02a: Control Class Registry (DRAFT V2, CANDIDATE PREPARATION)

> **BASELINE ARTIFACT BA-02a V2 — DRAFT prepared for `CANDIDATE_FOR_SEALING`, NOT SEALED. Design / documentation only.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-CONTROL-REGISTRY
> ARTIFACT_VERSION          = 2-DRAFT   (split from BA-02 1-DRAFT, blob 05eb89e2c402babb4e52c146b042821b734cc26c, which stays as history;
>                                        the coverage matrix is now BA-02b)
> ARTIFACT_STATUS           = DRAFT (candidate preparation, phase 2)
> CANDIDATE_HASH            = UNSET     SEAL_HASH = UNSET
> AUTHORITY_CONTRACT        = CT-21D V2.1 + V2.2 (PA-1) + V2.3 + V2.4
> SOURCE OF THE TABLES      = V2.2 PA-1.2 and PA-1.3, extracted verbatim from blob fd7260c57ec05e11f6bbb9f67a728e7dfdf1f609
> BASE ITEM                 = BASE-19 (registry agreed): NOT MET until sealed
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 1. Purpose and derivation

This artifact is the **normative control class registry** of the baseline. It extracts, **without any change of class, kind, basis or predicate membership**, the registry of V2.2 PA-1.2 and the group table of V2.2 PA-1.3 (the classes ruled by the Architect in the item-by-item check of V2.1, Q-CCR-1..6). The coverage matrix that was part of BA-02 V1 is now the separate artifact **BA-02b**, because it depends on the scenario catalog (NB-2) while this registry does not.

| Statistic | Value |
|---|---|
| entries of kind `CONTROL` | 30 (class A: 19; class B: 11) |
| `CONTROL` entries in the predicate (`IN_PREDICATE = YES`) | 28 |
| entries of kind `LOG_ONLY` | 2 |
| entries of kind `NOT_A_CONTROL` | 7 |
| entries with class `UNRESOLVED` | **0** (a mandatory `UNRESOLVED` control may not survive an authority baseline) |

**E-14 and E-15** are class **A** and **`NOT_PART_OF_CT21D_CHARACTERIZATION_PREDICATE`** (V2.2 PA-4): in a CT-21D run they are recorded `UNDER_CHARACTERIZATION` only (group `KR`); a future product admission consumes the seam evidence (E-14) and `ALT21D_HOST_PASS = TRUE` of the exact instance (E-15).

## 2. Control class registry

`ENTRY_KIND`: `CONTROL`, `LOG_ONLY` or `NOT_A_CONTROL`. `CLASS`: `A`, `B` or `UNRESOLVED` for a `CONTROL`; `N/A` otherwise. `BASIS`: `EXPLICIT` or `ARCHITECT_RULING`. Sources: `P-V2-4` = Proposal ALT-21D V2 section 4 class table (carried through V3, V4, V5); `P-V3-4` = V3 section 4; `P-V4-2` = V4 section 2; `AR-V2.1` = Architect item-by-item check of V2.1.

| CONTROL_ID | ENTRY_KIND | SOURCE_AUTHORITY | CLASS | BASIS | IN_PREDICATE | RATIONALE | AFFECTED_GROUPS | EXPECTED_FAILURE_MAPPING |
|---|---|---|---|---|---|---|---|---|
| S-1 staged reads `R_pre`, `M_pre` at PV | CONTROL | P-V2-4 (A1) | A | EXPLICIT | YES | state comparison carries RG-2/RG-3 | KS, SC-A | ABORT at PV (O2); class-A row of 1.4 by cause |
| S-2 pass 1 | CONTROL | P-V2-4 (A1) | A | EXPLICIT | YES | state comparison | SC-A, KS | difference => O4; class-A row |
| S-3 pass 2 | CONTROL | P-V2-4 (A1) | A | EXPLICIT | YES | state comparison | SC-A | difference => O4; class-A row |
| E-08 read-set re-observation (S-4) | CONTROL | P-V2-4 (A1) | A | EXPLICIT | YES | state comparison | E8 | ABORT (PV) / divergent or unverified (PC); class-A row |
| S-5 abort verification | CONTROL | P-V2-4 (A1), V4 3.4 | A | EXPLICIT | YES | abort claim RG-4 | AB | O2 or O6; class-A row |
| S-5 registry component (`PRE_COMMAND_STATE_RECORD`) | CONTROL | AR-V2.1 Q-CCR-3 (ratified derivation from S-5) | A | ARCHITECT_RULING | YES | defines the pre-command side of `EXPECTED_AFTER_ABORT` | PR-REC | incomplete or `UNKNOWN` => O1; class-A row |
| S-5 manifest component (`PREPARE_RESIDUE_MANIFEST`) | CONTROL | AR-V2.1 Q-CCR-3 | A | ARCHITECT_RULING | YES | defines the additions side of `EXPECTED_AFTER_ABORT` | PW | mismatch => O6; class-A row |
| S-5 closure component (static dependency closure) | CONTROL | AR-V2.1 Q-CCR-3 | A | ARCHITECT_RULING | YES | makes the record cover every pre-existing homologue | PR-REC | closure `UNKNOWN` => O1; class-A row |
| E-01 HostExact | CONTROL | P-V2-4 (A2) | A | EXPLICIT | YES | exclusion by measured fact | ID-A | REFUSE (O1); class-A row |
| E-02 SingleDocument | CONTROL | P-V2-4 (A2) | A | EXPLICIT | YES | | ID-A | REFUSE / ABORT; class-A row |
| E-03 LockHeld | CONTROL | P-V2-4 (A2) | A | EXPLICIT | YES | | LK | REFUSE; class-A row |
| E-04 SingleRackCadCommand | CONTROL | P-V2-4 (A2) | A | EXPLICIT | YES | | E4 | REFUSE / ABORT; class-A row |
| E-05 KnownModesInactive | CONTROL | P-V2-4 (A2) | A | EXPLICIT | YES | | ID-A | REFUSE / ABORT; class-A row |
| E-06 TransactionOwnership | CONTROL | P-V2-4 (A2) | A | EXPLICIT | YES | | E6 | REFUSE / ABORT; class-A row |
| E-07 DatabaseComplete | CONTROL | P-V2-4 (A2) | A | EXPLICIT | YES | | ID-A | REFUSE / ABORT; class-A row |
| E-09 OverrulesBounded | CONTROL | P-V2-4 (A2) | A | EXPLICIT | YES | | E9 | REFUSE / ABORT; class-A row |
| E-10 KnownModuleSetAttestation | CONTROL | P-V2-4 (B) | B | EXPLICIT | YES | risk reduction; proves no isolation | E10, WU | REFUSE / ABORT; class-B row |
| E-11M ManagedLoadContinuity | CONTROL | P-V2-4 (B) | B | EXPLICIT | YES | | E11M, WU | ABORT (PV) / unverified (PC); class-B row |
| E-12 phase 0 PREPARE | LOG_ONLY | P-V3-4 | N/A | EXPLICIT | NO | not adjudicated (`NG-19b`); not A/B | E12-V | none |
| E-12 phase A MUTATION | CONTROL | P-V3-4, P-V3-4.6 | B | EXPLICIT | YES | fail-closed abort before `Commit()` | E12-V | O2; class-B row |
| E-12 phase B COMMIT BOUNDARY | LOG_ONLY | P-V3-4 | N/A | EXPLICIT | NO | not adjudicated (`NG-19b`); not A/B | E12-V | none |
| E-12 phase C W-SCAN | CONTROL | P-V3-4.6 | B | EXPLICIT | YES | expected write set empty | E12-V, SC-B | O7 or by precedence; class-B row |
| E-12 phase D subscription | CONTROL | AR-V2.1 Q-CCR-3 (ratified derivation from P-V3-4.6) | B | ARCHITECT_RULING | YES | part of E-12, class B | E12-V | REFUSE (O1) / UNKNOWN; class-B row |
| E-12 phase E inference rule | NOT_A_CONTROL | P-V3-4 | N/A | EXPLICIT | NO | a rule: no origin inference | E12-V | none |
| E-12 determinism of the expected mutation set | CONTROL | AR-V2.1 Q-CCR-3 (ratified derivation from P-V3-4.5) | B | ARCHITECT_RULING | YES | the amendment route of V3 4.5 is the class-B route | E12-V | `EMPIRICAL_BEHAVIOR_DIFFERENCE`; class-B row |
| E-13 ProfileEquality | CONTROL | P-V2-4 (B) | B | EXPLICIT | YES | | ID-B | REFUSE; class-B row |
| E-14 KindReadiness | CONTROL | P-V2-4 (A2) | A | EXPLICIT | NO (`NOT_PART_OF_CT21D_CHARACTERIZATION_PREDICATE`) | consumed at future product admission (PA-4) | KR | `UNDER_CHARACTERIZATION` only |
| E-15 ContractBinding | CONTROL | P-V2-4 (A2) | A | EXPLICIT | NO (`NOT_PART_OF_CT21D_CHARACTERIZATION_PREDICATE`) | consumed at future product admission (PA-4) | KR | `UNDER_CHARACTERIZATION` only |
| E-11N native load continuity | NOT_A_CONTROL | P-V2-4 ("not a control"), `NG-10` | N/A | EXPLICIT | NO | no authority; declared residual | none | none |
| A-1 read-only opens emit no write events | CONTROL | P-V4-2; AR-V2.1 Q-CCR-3 | B | ARCHITECT_RULING | YES | premise of E-12 phase C; failure yields only false non-success | EA1 | `CHARACTERIZED_DIFFERENT`; class-B row |
| A-2 no deferred write events after V0 | CONTROL | P-V4-2; AR-V2.1 Q-CCR-3 | B | ARCHITECT_RULING | YES | same as A-1 | EA2 | `CHARACTERIZED_DIFFERENT`; class-B row |
| `WARMUP_MEMBERSHIP` | CONTROL | AR-V2.1 Q-CCR-3 (ratified derivation from E-10 and E-11M) | B | ARCHITECT_RULING | YES | baseline condition of the class-B controls | WU | REFUSE (O1); class-B row |
| `LIBRARY_FRESHNESS` | CONTROL | AR-V2.1 Q-CCR-1 | B | ARCHITECT_RULING | YES | see PA-1.1 | PR-FRESH | REFUSE (O1) / O2; class-B row |
| `CLONE_POLICY_VERIFIED` | CONTROL | AR-V2.1 Q-CCR-2 | A | ARCHITECT_RULING | YES | see PA-1.1 | PW-CLONE | `FAIL` / `UNKNOWN`; class-A row |
| `LIBRARY_EQUIVALENCE_OBSERVATION` (PA-7) | NOT_A_CONTROL | AR-V2.1 Q-DEP-1 | N/A | ARCHITECT_RULING | NO | informational only | PW | none: no verdict, no gate |
| INSTRUMENT_QUALIFICATION (I-01..I-16) | NOT_A_CONTROL | V2.1 4.3 | N/A | EXPLICIT | NO | run-validity precondition; failure => instrument `UNRELIABLE` | Q | `INSTRUMENT_DEFECT` |
| EVALUATOR_CONFORMANCE | NOT_A_CONTROL | V2.1 EXEC-10 | N/A | EXPLICIT | NO | evaluator defect => I-16 `UNRELIABLE` | OU | `INSTRUMENT_DEFECT` |
| CLEANUP | NOT_A_CONTROL | V2.1 20 | N/A | EXPLICIT | NO | incomplete cleanup => INVALID | CL | INVALID |
| UNDO_EVIDENCE, SAVE_REOPEN, POST_CP_REPORT, POST_V1_TO_CP_QUEUE_LOG, E-12 learning | NOT_A_CONTROL | V2.1 16, 10.7, 10.8, 27 | N/A | EXPLICIT | NO | evidence for Owner Act 2; not a pass mark | U, S1, PP, SC-B, E12-L | none (`EVIDENCE_COMPLETE`, PA-3) |

**Reading rules.** A control of class `UNRESOLVED` maps to `NOT_EVALUATED` whatever its result; a control with `IN_PREDICATE = NO` never moves the predicate state; a new entry may enter only through a reviewed amendment that adds it with its source, kind, class and basis.

## 3. Group table (one class per group)

| Group | Class | Kind | Content | V2.1 section |
|---|---|---|---|---|
| `Q` | N/A | EVIDENCE | instrument qualification (an instrument defect makes the instrument `UNRELIABLE`) | 4.3, 4.3.1 |
| `WU` | B | CONTROL | warm-up load record, membership in the manifest, `WARMUP_COVERAGE_CRITERION` | 13.1 |
| `ID-A` | A | CONTROL | E-01, E-02, E-05, E-07 | 2.4 |
| `ID-B` | B | CONTROL | E-13 | 2.4 |
| `E4` | A | CONTROL | E-04 admitted set | 3.3 |
| `LK` | A | CONTROL | E-03 lock | 3.4 |
| `E6` | A | CONTROL | E-06, `IsTopTransaction`, controls E6-C1..E6-C8 | 4.2 |
| `E8` | A | CONTROL | E-08 (S-4) | 5, 11.1 |
| `E9` | A | CONTROL | E-09 | 2.4 |
| `E10` | B | CONTROL | E-10 | 12 |
| `E11M` | B | CONTROL | E-11M | 13.2 |
| `E12-V` | B | CONTROL | E-12 validation: phase A, determinism, phase C, phase D. **Evaluable only with `EVM_STATUS = FROZEN` (PA-2)** | 14, 27 |
| `E12-L` | N/A | EVIDENCE | E-12 learning micro-scenarios | 27 |
| `EA1`, `EA2` | B | CONTROL | assumptions A-1 and A-2 | 14.7 |
| `SC-A` | A | CONTROL | scan: semantic comparison S-2, S-3 and designed detection at X1..X4 | 10.6 |
| `SC-B` | B | CONTROL | scan: E-12 phase C detection, channels and residual cells; the X7 tail log is reported here as evidence | 10.6, 10.8 |
| `PP` | N/A | EVIDENCE | post-CP characterization (report only) | 10.7 |
| `PR-REC` | A | CONTROL | S-5 registry and closure components | 6, 11.3 |
| `PR-FRESH` | **B** | CONTROL | `LIBRARY_FRESHNESS` (private copy authority, torn-read check) | 19.1 |
| `PW` | A | CONTROL | S-5 manifest component with exact fingerprints | 7, 11.3 |
| `PW-CLONE` | **A** | CONTROL | `CLONE_POLICY_VERIFIED` (non-replacing cloning mode) | 7.1, 4.3.1 |
| `AB` | A | CONTROL | ABORT-VERIFY (S-5) | 11 |
| `KS` | A | CONTROL | Selective seams SL-1, SL-2, SL-4, SL-5, SL-6 (comparators S-1..S-3) | 15 |
| `KR` | A | CONTROL, **outside the predicate** | E-14 and E-15 recorded `UNDER_CHARACTERIZATION` (PA-4) | 3.2 |
| `U-1..U-8`, `U-6b` | N/A | EVIDENCE | UNDO evidence | 16 |
| `S1` | N/A | EVIDENCE | SAVE/reopen equality evidence | 16 |
| `OU` | N/A | EVIDENCE | outcome evaluator scenarios O1..O7 (an evaluator defect makes I-16 `UNRELIABLE`) | 17 |
| `CL` | N/A | EVIDENCE | cleanup (an incomplete cleanup makes the run INVALID) | 20 |

A group of kind `EVIDENCE` is evaluated by `EVIDENCE_COMPLETE` (V2.2 PA-3) and never yields `FALSE` or `AMENDMENT_PENDING`. `E12-V` is evaluable only with `EVM_STATUS = FROZEN` (V2.2 PA-2). `KR` is outside the predicate (V2.2 PA-4).

## 4. Candidate readiness

This artifact has no dependency on host evidence, on fixtures or on Owner decisions: its content is fixed by the agreed contract. It is **prepared for an Architect ruling `CANDIDATE_FOR_SEALING` on its exact bytes**; the `CANDIDATE_HASH` is recorded in BA-11 V2 at that ruling, and the seal follows the transitions of BA-11 V2 section 3.

```text
BA-02a = DRAFT V2 (candidate preparation)     BASE-19 = NOT MET until sealed
CT21D_AUTHORITY_BASELINE_READY = FALSE
```
