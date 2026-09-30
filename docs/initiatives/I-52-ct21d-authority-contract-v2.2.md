# I-52 — CT-21D AUTHORITY CONTRACT V2.2 (DRAFT — PRE-APPROVED ERRATA OVER V2.1)

> **CT-21D AUTHORITY CONTRACT V2.2 — MINIMAL ERRATA OVER V2.1, FOR COORDINATOR TEXTUAL VERIFICATION. Design / documentation only. Nothing here is executed, built or authorized.**
>
> ```text
> OWNER_ACT1                        = SPONSORED  (OWNER_ACT1_DECISION = A; SPONSOR_ALT21D = YES)
> ALT21D_DIRECTION                  = OWNER_SPONSORED
> OWNER_DECISION_STATUS             = SPONSORED  (sponsorship of the pursuit ONLY; the final guarantee is NOT accepted)
> CT21D_AUTHORITY_CONTRACT          = DRAFT_V2_2  (V2.1 with this errata applied)
> CT21D_AUTHORITY_BASELINE_READY    = FALSE
> CT21D_EXECUTION_READY             = FALSE
> CT21D_EXECUTION                   = NOT_AUTHORIZED
> ALT21D_ADMISSION_PASS             = NOT_YET_SATISFIABLE
> CURRENTLY_ADMISSIBLE_KIND_SCOPE   = NONE
> CTDA_HOST_PASS(V35-A3)            = FALSE (terminal, monotone)
> ContextIsolationAuthority         = UNKNOWN          SafeOperationalState = FALSE_FOR_ADMISSION
> G3 = STOPPED   G3B = NOT OPEN   CT-50 = NOT EXECUTED   ALT-21C = NOT ADMISSIBLE   ALT-21E = FALLBACK_IN_EFFECT
> Freeze V18 = GOVERNING   Freeze V35 = GOVERNING ITS OWN PREDICATE   Replacement Freeze = DOES NOT EXIST   ADR-0036 = PROPOSED / AMENDED FOR V18
> UNDO_EVIDENCE_STATUS              = NOT_EXECUTED
> B5 = ADVANCES_TO_IMPLEMENTATION_PREREQUISITE  (+ REMAINS_BLOCKER_FOR_CT21D_EXECUTION + REMAINS_BLOCKER_FOR_REPLACEMENT_FREEZE)
> B6 = REMAINS_BLOCKER_FOR_ACT2
> NB-1 NB-3 NB-4 NB-6 = ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION (each remains open until its artifact exists)
> NB-2 = REMAINS_BLOCKER_FOR_AUTHORITY_BASELINE      NB-5 = REMAINS_BLOCKER_FOR_CT21D_EXECUTION
> ```
>
> The Owner authorized **only** the pursuit of the reduced-guarantee direction. This errata executes nothing, builds no harness, changes no code, creates no baseline artifact and is **not** an authority baseline.

## 0. What V2.2 is and how to read it

### 0.1 Nature, identity and precedence

The Architect's item-by-item check of V2.1 (`c1cfe9bc7ca97d8a0fed2078cc7f5e40b7e35d56`, blob `6259a3beb7445a63620636acd696cdad689965b3`) ruled `ARCHITECT_V2_1_CHECK = CHANGES_REQUIRED` with a **closed, pre-approved** list: **PA-1..PA-14** and **PW-1..PW-5**.
V2.2 applies exactly that list and nothing else. It does not redesign anything, and it is **not** a restatement: **V1, V2 and V2.1 are intact and historical.**

| Item | Rule |
|---|---|
| Effective contract | **`CT-21D-V2.2`** = the text of V2.1 **with the clauses of this document applied** |
| Precedence | for every clause named in this document, **V2.2 governs over V2.1**; every other clause of V2.1 stands unchanged |
| Predicate | `ALT21D_HOST_PASS(CT-21D-V2.2, BASELINE_TUPLE)` (the binding rule of V2.1 section 1.7 is unchanged) |
| Contract hash | `TO_BE_RECORDED_AT_BASELINE`: SHA-256 over the LF-normalized text of the V2.1 blob followed by the V2.2 blob, plus the V5 blob id. A single-file consolidation may be produced during baseline preparation **only as a mechanical merge verified by diff, with no semantic change** |
| Scope of this document | it reproduces the corrected tables and clauses in full where a partial patch would be ambiguous (the registry, the group table, the coverage rows); elsewhere it gives the replacement or added text |

### 0.2 Map of the corrections onto V2.1

| Errata | V2.1 sections affected | Treatment |
|---|---|---|
| PA-1 | 1.4, 21.4, 30, 31, 32.3 | 30 and 21.4 **replaced**; 1.4, 31 and 32.3 **amended** |
| PA-2 | 21.4, 27.2 | **added** |
| PA-3 | 21.4, 26.1 | **added** |
| PA-4 | 3.2, 30 | **added** |
| PA-5 | 13.1 | **added** (13.1.1) |
| PA-6 | 19.1 | step 2 and the authority rule **replaced** |
| PA-7 | 7.1, 11.3, 24 | **added** |
| PA-8 | 28.2 | **amended** |
| PA-9 | 28.2, 22 | **added** |
| PA-10 | 14.8 | **replaced** |
| PA-11 | 27.7 | **replaced** |
| PA-12 | 4.3.1 | `CLONE_NON_REPLACE_CONTROL` **replaced**; two controls **added** |
| PA-13 | 29.2 to 29.5 | **amended** |
| PA-14 | 25.8 | **replaced** |
| PW-1 | 1.7 | **added** |
| PW-2 | 19.1 | **added** |
| PW-3 | 25.1, 25.8 | **amended** |
| PW-4 | 4.2 | **amended** |
| PW-5 | 30, 32.3 | ENTRY_KIND column; question table **replaced** |

## PA-1 — Control class registry, ENTRY_KIND, group classes

### PA-1.1 Rulings recorded

- `LIBRARY_FRESHNESS`: **CLASS = B**, `BASIS = ARCHITECT_RULING` (Q-CCR-1). Reason: the guarantee does not cover the geometry of reused library content; the consequences of an import that differs from what was analysed are seen by the state comparison; no claim of RG-2..RG-4 depends on freshness.
- `CLONE_POLICY_VERIFIED`: **CLASS = A**, `BASIS = ARCHITECT_RULING` (Q-CCR-2). Reason: `EXPECTED_AFTER_ABORT = record + ADD set` presupposes that PREPARE-W does not replace pre-existing records; an unverified cloning mode could yield a false `ABORT_VERIFIED`, which RG-4 forbids.
- **Ratified as B** (`BASIS = ARCHITECT_RULING`, ratified derivations): A-1, A-2, determinism of the expected event set, E-12 phase D, `WARMUP_MEMBERSHIP`.
- **Ratified as A** (`BASIS = ARCHITECT_RULING`, ratified derivations from the explicit S-5): the S-5 **registry** component (`PRE_COMMAND_STATE_RECORD`), the S-5 **manifest** component (`PREPARE_RESIDUE_MANIFEST`) and the S-5 **closure** component (static dependency closure).
- E-12 **phase 0** and **phase B** are `LOG_ONLY`: they are **not A/B**.
- No entry of the registry is `UNRESOLVED` after this errata. The `UNRESOLVED` value and the rule of V2.1 1.4 and 1.6 remain valid for any **future** entry: a mandatory control of class `UNRESOLVED` still makes `TRUE` impossible.

### PA-1.2 `CONTROL_CLASS_REGISTRY` (replaces V2.1 section 30)

`ENTRY_KIND` values (normative): `CONTROL` (a control or premise that yields a control result and carries a class), `LOG_ONLY` (recorded, never adjudicated), `NOT_A_CONTROL` (a run-validity item, an evidence item or a rule). `CLASS` is `A`, `B` or `UNRESOLVED` for a `CONTROL`, and `N/A` for the other kinds.
`BASIS` values: `EXPLICIT` (the source states the class) or `ARCHITECT_RULING` (the class is assigned or ratified by an Architect ruling because the Proposal text does not state it directly; the ruling is named in `SOURCE_AUTHORITY`).
Sources: `P-V2-4` = Proposal ALT-21D V2 section 4, the class table (carried unchanged through V3, V4 and V5); `P-V3-4` = V3 section 4 (E-12 by phase, 4.5 degradation, 4.6 class B); `P-V4-2` = V4 section 2 (A-1, A-2); `AR-V2.1` = the Architect's item-by-item check of V2.1 (Q-CCR-1..6).

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

**Reading rules (unchanged in substance from V2.1 section 30).**
- A control of class `UNRESOLVED` maps to `NOT_EVALUATED` whatever its result, and its evidence is retained.
- A control with `IN_PREDICATE = NO` never moves the predicate state; its result is reported.
- A new entry may enter only through a reviewed amendment that adds it to this registry with its source, kind, class and basis.

### PA-1.3 Group table (replaces V2.1 section 21.4)

Every group has **one** class taken from the registry. The kind of a group is `CONTROL` when it evaluates controls of the registry, `EVIDENCE` when it evaluates only `NOT_A_CONTROL` items (its rule is PA-3).

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

### PA-1.4 Amendments to V2.1 section 1.4 and section 31

- **Section 1.4, last row.** Replace the row "control class `UNRESOLVED` (registry)" by: "control class `UNRESOLVED` (registry): `NOT_EVALUATED`; **no entry currently has this class**; the row stays for any future entry". The other rows are unchanged.
- **Section 1.4, addition.** A group of kind `EVIDENCE` has **no row** in this table: it never yields `FALSE` or `AMENDMENT_PENDING` (PA-3). The group `KR` has no row either (PA-4).
- **Section 21.7, first row.** The sentence "the mapping is not fully closed until the registry has no `UNRESOLVED` mandatory control" is satisfied at the contract level by PA-1.1; agreement of the registry artifact is `BASE-19`.
- **Section 31 (coverage matrix), replaced or added rows.**

| CONTROL | Class | GROUP | SCENARIO CLASSES | MINIMUM CATALOG REQUIREMENT |
|---|---|---|---|---|
| S-5 registry and closure components | A | PR-REC | `PR-*`, `Q-I13`, `CLOSURE_COMPLETENESS_CONTROL` | the record covers every closure member; a closure the traversal cannot classify refuses the run |
| S-5 manifest component | A | PW | `PW-*` | additions listed with exact fingerprints; a partial failed import is never legitimized |
| `LIBRARY_FRESHNESS` | B | PR-FRESH | `PR-*`, `Q-I13`, `SOURCE_SWAP_CONTROL` | a modified source detected; a stale cache detected; torn acquisition detected; `ACQUISITION_AUTHORITY` recorded |
| `CLONE_POLICY_VERIFIED` | A | PW-CLONE | `PW-*`, `CLONE_NON_REPLACE_CONTROL` | all variants of PA-12 |
| E-14, E-15 | A (outside the predicate) | KR | `KS-*` | recorded `UNDER_CHARACTERIZATION` |
| E-12 phase A, determinism, phase C, phase D | B | E12-V | `EV-V-<plan>`, `NC-E12A`, `NC-E12C` | validation only with a `FROZEN` EVM (PA-2) |
| `WARMUP_MEMBERSHIP` | B | WU | `WU`, `WARMUP_BOUNDARY_CONTROL` | coverage criterion measured; boundary control passes |

### PA-1.5 Groups outside the predicate and design-level coverage

At the level of this design every registry `CONTROL` has a group, every group has a scenario class, and no entry is `UNRESOLVED`. `TRUE` remains impossible while the coverage matrix is incomplete (V2.1 1.6). Agreement of the registry and of the matrix is `BASE-19` and `BASE-20`.

## PA-2 — `E12-V` requires a frozen event model

`E12-V` may be evaluated **only if `EVM_STATUS = FROZEN`** (V2.1 27.2 step 5). If no frozen EVM exists, `E12-V = NOT_EVALUATED`. The absence of a frozen EVM is **never** turned into a `PASS` or a `FAIL` of the host: it is a missing precondition, and the predicate state stays `NOT_EVALUATED` for that group. V2.1 27.2 step 6 reads accordingly: validation runs are governing only against a `FROZEN(hash)` model.

## PA-3 — Evidence-only groups: `EVIDENCE_COMPLETE`

Groups of kind `EVIDENCE` (PA-1.3) are evaluated by **`EVIDENCE_COMPLETE`**. Their group verdict is `PASS` when **all** of these hold:

1. every required **governing** run of the group exists;
2. the **evidence set** of each run is complete (`EXPECTED_EVIDENCE_SET`);
3. the **log-record set** of each run matches the one declared (`EXPECTED_LOG_RECORD_SET`);
4. the **observed** result equals the scenario's declared **`EXPECTED_PROCEDURAL_RESULT`**.

`EXPECTED_PROCEDURAL_RESULT` is a scenario field (added to V2.1 section 26.1 for scenarios of `EVIDENCE` groups): it declares that the procedure was executed as planned, the states were read, and the records were written. **The technical content of the evidence is not judged** in these groups.

Rules: groups of kind `EVIDENCE` **do not produce `FALSE`** and **do not produce `AMENDMENT_PENDING`**. An incomplete group yields `NOT_EVALUATED`. Findings inside their evidence (for example an UNDO that does not recover) go to the Freeze wording and Owner Act 2, not to the predicate. A defect of an instrument that produces such evidence is handled by instrument state (`UNRELIABLE`), not by these groups.

## PA-4 — E-14 and E-15

```text
E-14 = CLASS_A        E-15 = CLASS_A        both: NOT_PART_OF_CT21D_CHARACTERIZATION_PREDICATE
```

- In a CT-21D `CHARACTERIZATION_RUN` E-14 and E-15 are **`UNDER_CHARACTERIZATION` only** (V2.1 3.2). Group `KR` records this and never contributes to `TRUE`, `FALSE` or `AMENDMENT_PENDING`.
- **E-14** (KindReadiness): a future product admission consumes the **seam evidence**.
- **E-15** (ContractBinding): a future product admission consumes **`ALT21D_HOST_PASS = TRUE`** for the exact instance (contract version and baseline tuple), together with the replacement Freeze and the recorded Owner acceptance.
- **CT-21D is not circular:** it produces the evidence that E-14 and E-15 later consume, and does not require it.
- **P-8 (seam host evidence of rollback)** remains required **before the replacement Freeze** (V2.1 15.8; V2.1 24, item 5). Registering E-14 as outside the predicate does not make that evidence optional.

## PA-5 — Warm-up boundary

`WARMUP_SIDE_DATABASE` is registered as **`CONTROL_PLANE_ACTIVITY`** and **`NOT_PRODUCT_MUTATION`**. This resolves Q-WU-1 as acceptable control-plane activity. The following **11 conditions are mandatory** (added to V2.1 as section 13.1.1):

1. the side database is **created and owned by the control plane**;
2. it is **disposable**;
3. it is **never saved**;
4. it is **never registered as a document** (it does not enter the document collection, so E-02 is unaffected);
5. it is **discarded before step 4** of the governed sequence;
6. the warm-up **does not mutate the `LibraryInputRecord`**;
7. the warm-up **does not change the library generation id**;
8. **from step 4 through CP there are no side-database writes**, neither by the product nor by the control plane (E-06 prohibition, in full, for the governed window);
9. **the seams only receive the side database as an argument**; they never open or create one;
10. the **P-5 static guards are unchanged**;
11. **no observable static state may be left that alters the governed window** (the code loaded is the intended effect; any other retained state is prohibited).

**Residual, to remain disclosed: `WARMUP_STATIC_STATE_RESIDUAL`** — retained static state that the control plane cannot enumerate. It is listed in the Owner Act 2 disclosures (V2.1 24, item 15).

**Scope limit.** This clause **does not authorize a productive warm-up**: a warm-up in product mode is out of scope and must be reviewed with the replacement Freeze.

## PA-6 — Torn-read check and original-source reads

The check of a **torn acquisition is mandatory**. Step 2 of V2.1 19.1 is replaced by the following; the acquisition authority is **one of two**, recorded in the `LibraryInputRecord` as `ACQUISITION_AUTHORITY`:

| Authority | Rule |
|---|---|
| **A** | the source handle is **held with writes denied** for the **entire streaming acquisition** |
| **B** | after the streaming, the original source is **re-hashed** and the hash **must equal the hash of the private-copy acquisition** |

If neither A nor B can be established, or B does not match, the acquisition is `UNKNOWN` and the run is refused (O1) under the existing authority of V2.1 19.1 (step 2 and the UNKNOWN row).

**Two kinds of read of the original source are distinguished:**

| Kind | Rule |
|---|---|
| `ORIGINAL_SOURCE_IMPORT_READ` | **FORBIDDEN** once the authoritative private copy exists. Any such read is `AUTHORITY_INVALID` (V2.1 19.1) |
| `ORIGINAL_SOURCE_VERIFICATION_READ` | **ALLOWED** for the torn-read verification (authority B) and for the observation of drift after the acquisition |

**Verification reads never feed the library `Database`.** Drift of the original source after the authoritative acquisition remains **informational only** (`SOURCE_DRIFT_AFTER_ACQUISITION`, V2.1 19.1). The authority rule of V2.1 19.1 ("any reread of the original source for import") is read as forbidding `ORIGINAL_SOURCE_IMPORT_READ` only. Q-ACQ-1 is closed by this clause.

## PA-7 — `REUSED_BOUND` sufficiency and `LIBRARY_EQUIVALENCE_OBSERVATION`

For an **existing reusable library dependency** the contract requires **only**:

- the **correct symbol table or category**;
- the **correct expected object type**;
- its **exact pre-command fingerprint remains unchanged**.

**No structural or semantic equivalence to the library is required.** `MODIFY_PREEXISTING = PROHIBITED` is unchanged and is not reintroduced.

For each reused dependency (`REUSE_AS_IS` or `REUSED_BOUND`) the run records **`LIBRARY_EQUIVALENCE_OBSERVATION`** with one of:

| Value | Meaning |
|---|---|
| `EQUAL` | the exact fingerprint of the pre-existing record equals the exact fingerprint of the corresponding record in the private copy |
| `DIFFERENT` | the two exact fingerprints differ |
| `NOT_COMPARABLE` | either fingerprint is `UNKNOWN`, or the two records are not of the same kind |

The field is **`INFORMATIONAL_ONLY`, `NO_VERDICT`, `NO_GATE`**: it never changes a control result, an outcome or the predicate state. It is reported in the evidence package and **feeds the Owner Act 2 disclosure** `REUSED_EXTERNAL_GEOMETRY_NOT_VERIFIED` (V2.1 24, item 12). Q-DEP-1 is closed by this clause.

## PA-8 — `ExtensionData` and unknown members in the semantic comparison

V2.1 section 28.2 is amended: the typed form compared by `SEMANTIC_COMPARISON` for the authored payload and envelope **includes**:

- **`ExtensionData`** as the authoritative reader exposes it;
- **authored and forward-compatible members**;
- **unknown members preserved by contract** (the I-11 rule: unknown fields are preserved and the schema version is not downgraded).

Rules:

1. Unknown and extension members are compared **exactly** (canonical serialization: sorted keys, exact numbers) between the expected form and the persisted form. The caller composes the envelope and the seam writes it verbatim, so the expected form contains those members.
2. **A writer that drops or alters an unknown member must not pass the semantic verification**: the comparison is `DIFFERENT` (product outcome O4 through S-2 or S-3).
3. If a member cannot be compared (a member kind the reader does not expose, or the declared I-11 limitation that `JsonExtensionData` is not recursive applies), the comparison state is **`UNKNOWN_MEMBER_UNSUPPORTED`**, **surfaced explicitly** in the result, giving `UNKNOWN` (O5 path). It is **never ignored and never treated as absent**.
4. An unrecognized schema version remains `UNKNOWN`.

## PA-9 — Semantic tolerance gate

Numeric tolerances and normalizations of `SEMANTIC_COMPARISON` belong to the **`SELECTIVE_KIND_AUTHORITY_BASELINE`** and must:

- be **agreed before `BASELINE_READY`**;
- be **sealed before the first governing scenario that uses them**;
- **not be derived from the outputs of governing runs**;
- if changed later, produce a **new baseline**.

Normative ordering of the `EXACT_STATE_FINGERPRINT` (unchanged, restated): **ordered entity containers** (the entity sequence of a block table record) use the **host iteration / drawing order**; **unordered containers** use **stored-name order**. Q-FP-1 is closed by this clause. The tolerances are part of the Selective kind authority baseline artifact (BA-8) and of the content of `BASE-18` (V2.1 21.7, 22).

## PA-10 — E-12 phase-A adjudication domain (replaces V2.1 section 14.8)

Phase A occurs **before** the sealed set exists, so its domain is **not** defined as "the `PROTECTED_OBJECTS` known at PV". The domain of adjudication of phase A is:

- the **protected objects determinable from the plan**: the source objects, the relevant containers, the NOD entries the kind consumes;
- **objects created inside `T_M`** (listed by handle in the seam results).

| Phase | Adjudication domain | Outside the domain |
|---|---|---|
| A. MUTATION | the domain above | recorded, **not adjudicated** by itself (V3 4.2 row E) |
| C. W-SCAN | events on `PROTECTED_OBJECTS` (expected write set = empty) | recorded, not adjudicated |

**At PV** the sealed `PROTECTED_OBJECTS` is checked for **consistency with the earlier domain**: a member of the sealed set that the domain did not include is an inconsistency and aborts before `Commit()`.

- An event whose **target cannot be resolved**, or that matches more than one open entry, remains **ambiguous** (V2.1 14.5) and fails closed.
- **During validation, an event category that the frozen EVM does not explain is `EVM_INCOMPLETE`**, which requires a **new model version, review and revalidation** (V2.1 27.5). It is not a host verdict.
- This clause clarifies V3 4.2; it does not change any phase policy. Q-EVT-1 is closed by this clause.

## PA-11 — Independent completeness review (replaces V2.1 section 27.7)

The seam operation inventory is **the starting input**, not the completeness authority. It **must not be called complete** before an **`INDEPENDENT_COMPLETENESS_REVIEW`**. The review may use **static analysis plus dynamic trace**, under **all** of these conditions:

1. the static enumeration is **bound to the exact source SHA**;
2. the **limits of reflection and dynamic dispatch** are declared;
3. the dynamic trace covers **every seam entry point**;
4. the dynamic trace covers **all plan-shape classes** (V2.1 27.8);
5. the reviewer **did not compile or create the inventory**;
6. **differences between the static and the dynamic source are resolved explicitly** (each primitive found by either source appears in the inventory with a micro-scenario, or is justified as unreachable);
7. **facts seen by neither source remain residual** and surface as **unexplained events in learning**.

The review record is a baseline artifact. Nothing in the inventory is called complete until it exists.

## PA-12 — Qualification controls (amends V2.1 section 4.3.1)

**Added: `CLOSURE_COMPLETENESS_CONTROL`** (instrument I-13 with the fingerprint provider I-12, group `PR-REC`).

| Control | Procedure (concept) | Acceptance |
|---|---|---|
| `CLOSURE_COMPLETENESS_CONTROL` | compute the static dependency closure from the private library copy (V2.1 11.3); run the import into a **test document**; compare the records the import actually added with the ADD set of the closure | the **closure predicted from the private copy equals the ADD set actually added**; a difference in either direction is a finding of the closure computation (instrument defect) |

**Replaced: `CLONE_NON_REPLACE_CONTROL`** (SL-5 importer with I-13 and I-12, group `PW-CLONE`). The control keeps the V2.1 procedure (a pre-existing homonymous dependency with **different** content, under `DuplicateRecordCloning.Ignore` or a proven non-replacing equivalent; unchanged exact fingerprints; `REUSED_BOUND` in the manifest; declared cloning mode on the allow-list) and adds these **variants**, each a scenario:

| Variant | Expected |
|---|---|
| homonym that differs **only by case** | the record is treated as pre-existing (host symbol-table semantics), unchanged, with its **stored case** recorded; a case difference is a secondary finding |
| homonym of the **wrong object or type** | **`REJECT`** (V2.1 11.3, closed list); nothing written (O1) |
| **nested dependencies** | pre-existing nested blocks are `REUSED_BOUND`, unchanged |
| **symbol-table records** (layers, linetypes, text styles, dimension styles) | pre-existing records unchanged; only absent records are added |

**Added: `WARMUP_BOUNDARY_CONTROL`** (control plane, group `WU`). It verifies, for a warm-up that uses the `WARMUP_SIDE_DATABASE` (PA-5):

| Check | Acceptance |
|---|---|
| document transaction count | **no change** of the document database's count |
| document collection | **no document created or registered** |
| governed subscriptions | **no event generated** in the E-11M or E-12 governed logs by the warm-up |
| side database | **discarded before the governed window** (step 4) |

## PA-13 — Custody roles and remote anchor

Policy: **`YES_WITH_ROLE_SEPARATION_IN_LOG`**. One human **may** hold several logical roles (`CAPTURE_ROLE`, `REVIEW_ROLE`, `APPROVAL_ROLE`, `CUSTODY_ROLE`). Requirements (amending V2.1 29.2 to 29.5):

- **each role transition is a separate log entry**;
- the **actor is recorded** in each entry;
- **capture is always generated by the control plane**;
- the **role multiplicity is disclosed in Owner Act 2**.

**External anchor.** The anchor of `MANIFEST_CUSTODY_HEADER_HASH` must be a **PUBLISHED REMOTE reference**, not merely a local commit: the anchoring commit is pushed to the remote, and its presence at the remote is verified and recorded in the custody log (remote, reference, commit SHA).

Unchanged: **tamper evidence = yes; non-repudiation and signer identity = no**; no cryptographic signature; a hostile local administrator is out of the threat model. Self-review is a declared limit. Q-CUS-1 is closed by this clause.

## PA-14 — NB-1 seal conditions (replaces V2.1 section 25.8)

The scope result is kept:

```text
IN  : frontal by depth (0 <= k < SelectiveDepthLayout.Count(system)), planta
OUT : lateral, Section = -1 scenarios (OUT_OF_SCOPE for CT-21D)
```

Q-SEC-1 is closed: the narrower scope is permitted, does not supersede O-1 and does not exceed Freeze V18. **NB-1 closes only when all of these are done:**

1. **Coordinator ratification**;
2. **Architect ratification**;
3. **dated re-execution of the authority comparison** (V2.1 25.3) against the **then-current identities**;
4. **complete reading of the post-I-57 consensus**, **not keyword search only**;
5. **no `CONFLICT`**;
6. a **sealed artifact with its hash** (`SELECTIVE_VIEW_FAMILY_SCOPE_V1 = SEALED`);
7. the explicit statement that **sealing does NOT replace the Owner O-1 redecision**.

**None of items 1 to 7 is performed by this errata.** Until they are, **NB-1 remains open**. Any later change of the Selective view exposure reopens NB-1 and the dependent artifacts.

## PW-1 — Superseded instance

When a class-A `PRODUCT_OR_SEAM_DEFECT` makes an instance `FALSE` for one build tuple (V2.1 1.4) and a **corrected build creates a new tuple**:

- the old instance takes the designation **`SUPERSEDED_FALSE_INSTANCE`**; its state stays `FALSE`;
- it **remains historical evidence**: it is immutable, not deleted and never reopened;
- the corrected build is a **new evaluation instance** (V2.1 1.7): **not a retry** of the old one; no evidence transfers; its attempt log starts empty, and it needs a baseline tuple whose build-bound fields identify the corrected build (baseline artifacts whose hash is unchanged may be reused only by an explicit Coordinator record of that equality);
- the rule does **not** apply to `FALSE` caused by `HOST_LIMITATION` or `EMPIRICAL_BEHAVIOR_DIFFERENCE`, which need a new contract version.

**Same rule, class B (extension, see note N-2).** A class-B `PRODUCT_OR_SEAM_DEFECT` instance in `AMENDMENT_PENDING` whose build tuple is replaced takes the designation **`SUPERSEDED_AMENDMENT_PENDING_INSTANCE`** by the identical rule, so that it does not remain pending indefinitely.

## PW-2 — Private copy alias

In V2.1 the terms **"private copy"** and **"authoritative private library copy"** (also written "per-run authoritative private library copy") **denote one and the same concept**: the copy defined in V2.1 19.1. There is **no second concept**. The short form "private copy" is an alias.

## PW-3 — Authority notes for V2.1 sections 25.1 and 25.8

The following verified facts amend the notes of V2.1 25.1 and 25.8:

- **ADR-0036.** The amendment "for V18" recorded in the ADR concerns the **G3 reopening sequence**, not the Selective view exposure. The exposure statements (decision 4 and the accepted negative consequences) were found in **both** the blob frozen by V17 (`ce5f6fae935c7acdc347e12a7d465bb438923549`) and the current blob (`5628947673f2afddf141c0505e92283962459443`).
- **Post-I-57 consensus** (blob `5f4933a3aa90d7052f571317107f1831ec02ac6e`). A **keyword search** for view and exposure terms (frontal, planta, lateral, Section, exposición, `A_k`) found **no occurrence**. This is **not a complete review** and is **not sufficient for sealing**. The statement that this consensus contains no statement that changes the Selective view exposure is **provisional** until the **complete reading** required by PA-14, item 4.

## PW-4 — `STALE_WRAPPER_CONTROL` result

`E6-C8` (V2.1 4.2) records **`POINTER_REUSE_OBSERVED = YES / NO`**.

- If reuse **did not** occur, the control may still demonstrate that the stale wrapper is rejected (`NO` or `UNKNOWN`, never `YES`), but it **must not claim that pointer reuse was exercised**.
- If reuse occurred, the result records it, and the acceptance (never `YES` for the new live transaction) applies to that case.

## PW-5 — `ENTRY_KIND` and closed questions

The `ENTRY_KIND` column is part of the registry of PA-1.2. The table below **replaces V2.1 section 32.3**: none of these questions remains open.

| Id | Status | Closed by |
|---|---|---|
| Q-CCR-1 | **CLOSED** | PA-1.1: `LIBRARY_FRESHNESS` = B |
| Q-CCR-2 | **CLOSED** | PA-1.1: `CLONE_POLICY_VERIFIED` = A |
| Q-CCR-3 | **CLOSED** | PA-1.1, PA-1.2: derived classes ratified |
| Q-CCR-4 | **CLOSED** | PA-4 |
| Q-CCR-5 | **CLOSED** | PA-3 |
| Q-CCR-6 | **CLOSED** | PA-1.3 (splits ratified; `PR-FRESH` B, `PW-CLONE` A), PA-2 |
| Q-WU-1 | **CLOSED** | PA-5 |
| Q-DEP-1 | **CLOSED** | PA-7 |
| Q-ACQ-1 | **CLOSED** | PA-6 |
| Q-EVT-1 | **CLOSED** | PA-10 |
| Q-SEC-1 | **CLOSED** | PA-14 |
| Q-FP-1 | **CLOSED** | PA-9 |
| Q-CUS-1 | **CLOSED** | PA-13 |

Closing a question records a ruling and its text. It **does not close a blocker**: the blockers keep the states of section "Blockers" below.

## Notes for the Coordinator's textual verification

| Note | Content |
|---|---|
| N-1 | The contract hash covers the V2.1 blob followed by the V2.2 blob (0.1). This is a mechanical choice to keep V2.1 intact; a consolidated single file is allowed only as a verified mechanical merge |
| N-2 | PW-1 states the class-A case as pre-approved and applies **the identical rule** to the class-B `AMENDMENT_PENDING` instance. This extension follows from V2.1 1.4 and is flagged for verification |
| N-3 | PA-3 uses `EXPECTED_PROCEDURAL_RESULT`, a scenario field added to V2.1 26.1 so that `EVIDENCE_COMPLETE` has a declared reference |
| N-4 | PA-6 records `ACQUISITION_AUTHORITY` (A or B) in the `LibraryInputRecord`, so that the chosen authority is auditable |
| N-5 | PA-1.2 adds the registry row `LIBRARY_EQUIVALENCE_OBSERVATION` as `NOT_A_CONTROL` only to carry the PA-7 field; no class is added |

## Blockers

| Id | Status |
|---|---|
| B1, B2, B7 | `CLOSED_BY_V3_CONTRACT` |
| B3 | `CLOSED_BY_V4_CONTRACT` |
| B4, B8 | `CLOSED_BY_V2_CONTRACT` |
| **B5** | `ADVANCES_TO_IMPLEMENTATION_PREREQUISITE` + `REMAINS_BLOCKER_FOR_CT21D_EXECUTION` + `REMAINS_BLOCKER_FOR_REPLACEMENT_FREEZE` |
| **B6** | `REMAINS_BLOCKER_FOR_ACT2` |
| **NB-1** | `ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION`; **remains open until sealed** (PA-14) |
| **NB-2** | `REMAINS_BLOCKER_FOR_AUTHORITY_BASELINE` (schema accepted; the catalog content does not exist) |
| **NB-3** | `ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION`; **open until the independent artifact, its hash and its agreement exist** |
| **NB-4** | `ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION`; **open until the inventory, the completeness review and the corpus artifacts exist** |
| **NB-5** | `REMAINS_BLOCKER_FOR_CT21D_EXECUTION` |
| **NB-6** | `ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION`; **open until the remote anchor and the procedure are agreed** |

No blocker is closed merely because this errata describes its future artifact.

## Final status

```text
OWNER_ACT1 = SPONSORED     OWNER_DECISION_STATUS = SPONSORED (pursuit only)
CT21D_AUTHORITY_CONTRACT = DRAFT_V2_2 (V2.1 with this errata applied)
CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
CURRENTLY_ADMISSIBLE_KIND_SCOPE = NONE     ALT21D_ADMISSION_PASS = NOT_YET_SATISFIABLE
SELECTIVE_AUTHORITY_DESIGN_STATUS = COMPLETE_FOR_BASELINE (DRAFT_V2_2; pending agreement)
UNDO_EVIDENCE_STATUS = NOT_EXECUTED
CTDA_HOST_PASS = FALSE (terminal)     CIA = UNKNOWN     SafeOperationalState = FALSE_FOR_ADMISSION     G3 = STOPPED     G3B = NOT OPEN     CT-50 = NOT EXECUTED
ALT-21E = FALLBACK_IN_EFFECT     Freeze V18 = GOVERNING     Freeze V35 = GOVERNING ITS OWN PREDICATE     Replacement Freeze = DOES NOT EXIST     ADR-0036 = PROPOSED / AMENDED FOR V18
HOST VALIDATION = NOT RUN     BRANCH = NOT RECONCILED
NEXT GATE = COORDINATOR TEXTUAL VERIFICATION OF CT-21D AUTHORITY CONTRACT V2.2, THEN (IF PASS) PREPARATION OF AUTHORITY BASELINE ARTIFACTS
```
