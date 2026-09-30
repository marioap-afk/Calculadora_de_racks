# I-52 — CT-21D Baseline Artifact BA-02: Control Class Registry and Coverage Matrix (DRAFT V1)

> **BASELINE ARTIFACT BA-02 — DRAFT, NOT SEALED, NOT AGREED. Design / documentation only. Nothing here is executed or authorized.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-CONTROL-REGISTRY
> ARTIFACT_VERSION          = 1-DRAFT
> ARTIFACT_STATUS           = DRAFT (phase 1 of baseline preparation)
> ARTIFACT_HASH             = TO_BE_RECORDED_AT_SEAL   (see BA-11)
> AUTHORITY_CONTRACT        = CT-21D V2.1 + V2.2 + V2.3 (AGREED_FOR_BASELINE_ARTIFACT_PREPARATION)
> BASE ITEMS                = BASE-19 (registry agreed), BASE-20 (coverage matrix agreed): neither is agreed yet
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 1. Purpose and derivation

This artifact extracts the **agreed** control class registry of V2.2 (PA-1.2) and group table (PA-1.3) **without change of class**, and adds the control-to-group-to-scenario coverage matrix that ties each control to the scenarios of BA-04. The classes are the ones ruled by the Architect (V2.1 check, Q-CCR-1..6); none is invented here.
Registry statistics: **30** entries of kind `CONTROL`; **0** entries with class `UNRESOLVED` (a mandatory `UNRESOLVED` control may not survive an authority baseline).

## 2. Control class registry (extracted from V2.2 PA-1.2)

`ENTRY_KIND`: `CONTROL`, `LOG_ONLY` or `NOT_A_CONTROL`. `CLASS`: `A`, `B` or `UNRESOLVED` for a `CONTROL`; `N/A` otherwise. `BASIS`: `EXPLICIT` or `ARCHITECT_RULING`. `IN_PREDICATE`: whether the control decides `ALT21D_HOST_PASS`. Sources: `P-V2-4` = Proposal ALT-21D V2 section 4 class table (carried through V3, V4, V5); `P-V3-4` = V3 section 4; `P-V4-2` = V4 section 2; `AR-V2.1` = Architect item-by-item check of V2.1.

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

Reading rules: a control of class `UNRESOLVED` maps to `NOT_EVALUATED` whatever its result; a control with `IN_PREDICATE = NO` never moves the predicate state; a new entry may enter only through a reviewed amendment with its source, kind, class and basis.

## 3. Group table (extracted from V2.2 PA-1.3)

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

## 4. Control to group to scenario coverage matrix

Each row ties a registry control to its groups and to the scenarios of BA-04. **Concrete** scenarios exist in the draft catalog and exclude `EXPLORATORY_NON_GOVERNING` entries (which never count as coverage). **Required class entries** are placeholders: their results are **not invented**; BA-04 section 5 states what authority or input is missing for each.

`COVERAGE STATE`: `CONCRETE` (concrete scenarios and no class entries pending), `CONCRETE_PARTIAL` (concrete scenarios exist, further class entries still to be authored), `CLASS_ONLY` (only required class entries), `GAP` (nothing; not allowed).

| KEY | CONTROL | CLASS | GROUPS | CONCRETE SCENARIOS | DELIBERATE-VIOLATION SCENARIOS | REQUIRED CLASS ENTRIES NOT YET AUTHORED | COVERAGE STATE |
|---|---|---|---|---|---|---|---|
| `S1` | S-1 staged reads | A | KS, SC-A | (none yet) | (none yet) | KS-* (S-1 staged-read negative control: needs a pre-PV injection position, see gap G-1); CL-CLEAN-<family> | CLASS_ONLY |
| `S2` | S-2 pass 1 | A | SC-A, KS | `WR-A-X0`, `WR-A-X1`, `WR-A-X2`, ... (56 scenarios; see the catalog) | `WR-A-X0`, `WR-A-X1`, `WR-A-X2`, ... (56 scenarios; see the catalog) | CL-CLEAN-<family> | CONCRETE_PARTIAL (class entries still to be authored) |
| `S3` | S-3 pass 2 | A | SC-A | `WR-A-X0`, `WR-A-X1`, `WR-A-X2`, ... (56 scenarios; see the catalog) | `WR-A-X0`, `WR-A-X1`, `WR-A-X2`, ... (56 scenarios; see the catalog) | CL-CLEAN-<family> | CONCRETE_PARTIAL (class entries still to be authored) |
| `E08` | E-08 read-set re-observation | A | E8 | `WR-G-X0`, `WR-G-X1`, `WR-G-X2`, ... (9 scenarios; see the catalog) | `WR-G-X0`, `WR-G-X1`, `WR-G-X2`, ... (9 scenarios; see the catalog) | NC-E08 (source read-set changed between PS and PV; needs a pre-PV injection position, gap G-1); CL-CLEAN-<family> | CONCRETE_PARTIAL (class entries still to be authored) |
| `S5` | S-5 abort verification | A | AB | (none yet) | (none yet) | AB-* (abort after PREPARE-W; after MUTATION; after a simulated Commit() exception at T1, not host evidence); PW-* | CLASS_ONLY |
| `S5REC` | S-5 registry component | A | PR-REC | `PR-CLOSURE-COMPLETENESS` | (none yet) | PR-* (record completeness; closure UNKNOWN refuses the run) | CONCRETE_PARTIAL (class entries still to be authored) |
| `S5MAN` | S-5 manifest component | A | PW | `PW-CLONE-BASE`, `PW-CLONE-CASE`, `PW-CLONE-WRONG-TYPE`, `PW-CLONE-NESTED`, `PW-CLONE-SYMTAB` | `PW-CLONE-BASE`, `PW-CLONE-CASE`, `PW-CLONE-WRONG-TYPE`, `PW-CLONE-NESTED`, `PW-CLONE-SYMTAB` | PW-* (partial failed import is never legitimized) | CONCRETE_PARTIAL (class entries still to be authored) |
| `S5CLO` | S-5 closure component | A | PR-REC | `PR-CLOSURE-COMPLETENESS` | (none yet) | PR-* (unclassifiable closure member refuses the run) | CONCRETE_PARTIAL (class entries still to be authored) |
| `E01` | E-01 HostExact | A | ID-A | (none yet) | (none yet) | CL-CLEAN-<family> | CLASS_ONLY |
| `E02` | E-02 SingleDocument | A | ID-A | `NC-E02` | `NC-E02` | CL-CLEAN-<family> | CONCRETE_PARTIAL (class entries still to be authored) |
| `E03` | E-03 LockHeld | A | LK | `NC-E03` | `NC-E03` | LK-<mode> (one per candidate LOCK_MODE) | CONCRETE_PARTIAL (class entries still to be authored) |
| `E04` | E-04 SingleRackCadCommand | A | E4 | `NC-E04` | `NC-E04` | E4-LEARN-<route>, E4-VAL-<route> (designated routes; admitted set evidence-derived) | CONCRETE_PARTIAL (class entries still to be authored) |
| `E05` | E-05 KnownModesInactive | A | ID-A | `NC-E05` | `NC-E05` | CL-CLEAN-<family> | CONCRETE_PARTIAL (class entries still to be authored) |
| `E06` | E-06 TransactionOwnership | A | E6 | `E6-C1`, `E6-C2`, `E6-C3`, ... (8 scenarios; see the catalog) | `E6-C6`, `NC-E06` | CL-CLEAN-<family> | CONCRETE_PARTIAL (class entries still to be authored) |
| `E07` | E-07 DatabaseComplete | A | ID-A | `NC-E07` | `NC-E07` | CL-CLEAN-<family> | CONCRETE_PARTIAL (class entries still to be authored) |
| `E09` | E-09 OverrulesBounded | A | E9 | `NC-E09` | `NC-E09` | CL-CLEAN-<family> | CONCRETE_PARTIAL (class entries still to be authored) |
| `E10` | E-10 KnownModuleSetAttestation | B | E10, WU | `NC-E10` | `NC-E10` | CL-CLEAN-<family> | CONCRETE_PARTIAL (class entries still to be authored) |
| `E11M` | E-11M ManagedLoadContinuity | B | E11M, WU | `NC-E11M-PV`, `NC-E11M-PC` | `NC-E11M-PV`, `NC-E11M-PC` | CL-CLEAN-<family>; WU-DRY-<scenario> (dry runs, coverage criterion) | CONCRETE_PARTIAL (class entries still to be authored) |
| `E12A` | E-12 phase A MUTATION | B | E12-V | `NC-E12A` | `NC-E12A` | EV-L-<operation> (learning), EV-V-<plan> (validation) | CONCRETE_PARTIAL (class entries still to be authored) |
| `E12C` | E-12 phase C W-SCAN | B | E12-V, SC-B | `WR-A-X0`, `WR-A-X1`, `WR-A-X2`, ... (52 scenarios; see the catalog) | `WR-A-X0`, `WR-A-X1`, `WR-A-X2`, ... (52 scenarios; see the catalog) | EV-V-<plan> | CONCRETE_PARTIAL (class entries still to be authored) |
| `E12D` | E-12 phase D subscription | B | E12-V | (none yet) | (none yet) | EV-V-<plan> (subscription failure at P0 refuses) | CLASS_ONLY |
| `E12DET` | E-12 determinism of the expected mutation set | B | E12-V | (none yet) | (none yet) | EV-V-<plan> (determinism repetitions) | CLASS_ONLY |
| `E13` | E-13 ProfileEquality | B | ID-B | (none yet) | (none yet) | CL-CLEAN-<family> | CLASS_ONLY |
| `A1` | A-1 read-only opens | B | EA1 | `EA1-READONLY-OPENS` | (none yet) | (none) | CONCRETE |
| `A2` | A-2 no deferred write events | B | EA2 | `EA2-DEFERRED-EVENTS` | (none yet) | (none) | CONCRETE |
| `WUM` | WARMUP_MEMBERSHIP | B | WU | `WU-BOUNDARY` | (none yet) | WU-DRY-<scenario> | CONCRETE_PARTIAL (class entries still to be authored) |
| `LIBF` | LIBRARY_FRESHNESS | B | PR-FRESH | `PR-SOURCE-SWAP-MODIFY`, `PR-SOURCE-SWAP-DELETE`, `PR-SOURCE-SWAP-NEG-COPY`, `PR-TORN-READ` | `PR-SOURCE-SWAP-MODIFY`, `PR-SOURCE-SWAP-DELETE`, `PR-SOURCE-SWAP-NEG-COPY`, `PR-TORN-READ` | (none) | CONCRETE |
| `CLONE` | CLONE_POLICY_VERIFIED | A | PW-CLONE | `PW-CLONE-BASE`, `PW-CLONE-CASE`, `PW-CLONE-WRONG-TYPE`, `PW-CLONE-NESTED`, `PW-CLONE-SYMTAB` | `PW-CLONE-BASE`, `PW-CLONE-CASE`, `PW-CLONE-WRONG-TYPE`, `PW-CLONE-NESTED`, `PW-CLONE-SYMTAB` | (none) | CONCRETE |
| `E14` | E-14 KindReadiness | A (outside the predicate) | KR | (none yet) | (none yet) | KS-* (recorded UNDER_CHARACTERIZATION) | CLASS_ONLY |
| `E15` | E-15 ContractBinding | A (outside the predicate) | KR | (none yet) | (none yet) | KS-* (recorded UNDER_CHARACTERIZATION) | CLASS_ONLY |

### 4.1 Group coverage

| GROUP | CONCRETE SCENARIOS IN BA-04 | STATE |
|---|---|---|
| `Q` | `Q-I01`, `Q-I02`, `Q-I03`, ... (17 scenarios; see the catalog) | CONCRETE |
| `WU` | `WU-BOUNDARY` | CONCRETE |
| `ID-A` | `NC-E02`, `NC-E05`, `NC-E07` | CONCRETE |
| `ID-B` | (none yet) | CLASS_ONLY (BA-04 section 5) |
| `E4` | `NC-E04` | CONCRETE |
| `LK` | `NC-E03` | CONCRETE |
| `E6` | `E6-C1`, `E6-C2`, `E6-C3`, ... (8 scenarios; see the catalog) | CONCRETE |
| `E8` | (none yet) | CLASS_ONLY (BA-04 section 5) |
| `E9` | `NC-E09` | CONCRETE |
| `E10` | `NC-E10` | CONCRETE |
| `E11M` | `NC-E11M-PV`, `NC-E11M-PC` | CONCRETE |
| `E12-V` | `NC-E12A` | CONCRETE |
| `E12-L` | (none yet) | CLASS_ONLY (BA-04 section 5) |
| `EA1` | `EA1-READONLY-OPENS` | CONCRETE |
| `EA2` | `EA2-DEFERRED-EVENTS` | CONCRETE |
| `SC-A` | `WR-A-X0`, `WR-A-X1`, `WR-A-X2`, ... (29 scenarios; see the catalog) | CONCRETE |
| `SC-B` | `WR-A-X5`, `WR-A-X6`, `WR-A-X7`, ... (27 scenarios; see the catalog) | CONCRETE |
| `PP` | (none yet) | CLASS_ONLY (BA-04 section 5) |
| `PR-REC` | `PR-CLOSURE-COMPLETENESS` | CONCRETE |
| `PR-FRESH` | `PR-SOURCE-SWAP-MODIFY`, `PR-SOURCE-SWAP-DELETE`, `PR-SOURCE-SWAP-NEG-COPY`, `PR-TORN-READ` | CONCRETE |
| `PW` | (none yet) | CLASS_ONLY (BA-04 section 5) |
| `PW-CLONE` | `PW-CLONE-BASE`, `PW-CLONE-CASE`, `PW-CLONE-WRONG-TYPE`, `PW-CLONE-NESTED`, `PW-CLONE-SYMTAB` | CONCRETE |
| `AB` | (none yet) | CLASS_ONLY (BA-04 section 5) |
| `KS` | (none yet) | CLASS_ONLY (BA-04 section 5) |
| `KR` | (none yet) | CLASS_ONLY (BA-04 section 5) |
| `U-1..U-8` | (none yet) | CLASS_ONLY (`U-*`, BA-04 section 5) |
| `U-6b` | (none yet) | CLASS_ONLY (`U-*`, BA-04 section 5) |
| `S1` | (none yet) | CLASS_ONLY (BA-04 section 5) |
| `OU` | `OU-O1`, `OU-O2`, `OU-O6`, ... (8 scenarios; see the catalog) | CONCRETE |
| `CL` | (none yet) | CLASS_ONLY (BA-04 section 5) |

### 4.2 Design-level coverage checks (V2.2 PA-1.5)

| Check | Result |
|---|---|
| every registry control of class A or B with `IN_PREDICATE = YES` has at least one group | **holds** (30 controls listed in section 4, none with state `GAP`) |
| every group of section 3 has at least one scenario class | **holds** (concrete scenarios or a required class entry) |
| every mandatory control has a clean scenario and a deliberate-violation scenario, unless it cannot be violated by design | **does not hold yet for the controls in section 4.3**; they are covered by required class entries, not by concrete scenarios |
| no scenario exercises a class outside the registry | **holds** (`CONTROL_CLASSES_EXERCISED` values are registry keys) |
| no mandatory control is `UNRESOLVED` | **holds** (0 entries) |
| the coverage matrix is complete | **not yet**: class entries are pending, and `TRUE` stays impossible while the matrix is incomplete (V2.1 1.6) |

### 4.3 Mandatory controls without a concrete deliberate-violation scenario

| CONTROL | State | Reason |
|---|---|---|
| `S1` S-1 staged reads | CLASS_ONLY | needs a pre-PV injection position (BA-04 gap G-1) |
| `S5` S-5 abort verification | CLASS_ONLY | needs fixtures and the seams; the simulated `Commit()` exception path is T1, not host evidence |
| `S5REC` S-5 registry component | CONCRETE_PARTIAL (class entries still to be authored) | closure/record negative controls need fixtures (`PR-*`); `PR-CLOSURE-COMPLETENESS` is a positive control |
| `S5CLO` S-5 closure component | CONCRETE_PARTIAL (class entries still to be authored) | closure negative controls need fixtures (`PR-*`) |
| `E01` E-01 HostExact | CLASS_ONLY | `NC-E01` is exploratory (BA-04 gap G-2) |
| `E12D` E-12 phase D subscription | CLASS_ONLY | subscription-failure scenarios need the instrumentation design and a frozen EVM |
| `E12DET` E-12 determinism of the expected mutation set | CLASS_ONLY | determinism repetitions need a frozen EVM and validation plans |
| `E13` E-13 ProfileEquality | CLASS_ONLY | `NC-E13` is exploratory (BA-04 gap G-2) |
| `A1` A-1 read-only opens | CONCRETE | a premise: the scenario measures the premise (`EA1-READONLY-OPENS`); no violation is injected |
| `A2` A-2 no deferred write events | CONCRETE | a premise: the scenario measures the premise (`EA2-DEFERRED-EVENTS`); no violation is injected |
| `WUM` WARMUP_MEMBERSHIP | CONCRETE_PARTIAL (class entries still to be authored) | `WU-BOUNDARY` is a positive boundary control; dry-run negative controls need the scenario list |
| `E14` E-14 KindReadiness | CLASS_ONLY | outside the predicate; recorded `UNDER_CHARACTERIZATION` |
| `E15` E-15 ContractBinding | CLASS_ONLY | outside the predicate; recorded `UNDER_CHARACTERIZATION` |

## 5. Status

```text
BASE-19 = NOT AGREED (registry drafted; 0 UNRESOLVED; agreement pending)
BASE-20 = NOT AGREED (matrix drafted; class entries pending)
CT21D_AUTHORITY_BASELINE_READY = FALSE
```
