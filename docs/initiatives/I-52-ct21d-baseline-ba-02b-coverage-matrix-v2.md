# I-52 — CT-21D Baseline Artifact BA-02b: Control to Group to Scenario Coverage Matrix (DRAFT V2)

> **BASELINE ARTIFACT BA-02b V2 — DRAFT, NOT SEALED. Design / documentation only.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-COVERAGE-MATRIX
> ARTIFACT_VERSION          = 2-DRAFT   (split from BA-02 1-DRAFT, blob 05eb89e2c402babb4e52c146b042821b734cc26c, which stays as history)
> ARTIFACT_STATUS           = DRAFT (phase 2)     CANDIDATE_HASH = UNSET     SEAL_HASH = UNSET
> DEPENDS ON                = BA-02a V2 (registry) and BA-04 V2 (catalog); BLOCKED by NB-2 for sealing
> BASE ITEM                 = BASE-20: NOT MET
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 1. Rule for `BASELINE_READY` (Architect phase-1 ruling)

During preparation, coverage by scenario class was acceptable. For `BASELINE_READY`, **every mandatory control (`IN_PREDICATE = YES`) needs at least one concrete scenario whose expected result is `SEALED` or `SEALED_PENDING_PIN`, and either a deliberate-violation scenario or a sealed reason that it cannot be violated**. E-14 and E-15 need a concrete scenario that records them `UNDER_CHARACTERIZATION` (`KS-KR-RECORD`). In V2 every scenario ID is concrete; **none is sealed**, so the column `SEALED SCENARIOS` is 0 for every row.

## 2. Control coverage

| KEY | CONTROL | CLASS | CLEAN SCENARIOS (concrete, draft) | DELIBERATE-VIOLATION SCENARIOS (concrete, draft) | NO-VIOLATION REASON | SEALED SCENARIOS | STATE |
|---|---|---|---|---|---|---|---|
| `S1` | S-1 staged reads | A | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, `CL-CLEAN-FX2F-ALL`, ... (8) | `NC-S1-Y1`, `NC-S1-Y1-SILENT` | - | 0 | COVERED_DRAFT (concrete scenario IDs; 0 sealed) |
| `S2` | S-2 pass 1 | A | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, `CL-CLEAN-FX2F-ALL`, ... (8) | `WR-A-X0`, `WR-A-X1`, `WR-A-X2`, `WR-A-X3`, ... (89) | - | 0 | COVERED_DRAFT (concrete scenario IDs; 0 sealed) |
| `S3` | S-3 pass 2 | A | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, `CL-CLEAN-FX2F-ALL`, ... (8) | `WR-A-X0`, `WR-A-X1`, `WR-A-X2`, `WR-A-X3`, ... (89) | - | 0 | COVERED_DRAFT (concrete scenario IDs; 0 sealed) |
| `E08` | E-08 read-set re-observation | A | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, `CL-CLEAN-FX2F-ALL`, ... (8) | `NC-E08-Y0`, `NC-E08-Y0-SILENT`, `NC-E08-Y2`, `NC-E08-Y2-SILENT`, ... (19) | - | 0 | COVERED_DRAFT (concrete scenario IDs; 0 sealed) |
| `S5` | S-5 abort verification | A | (none) | `PW-IMPORT-PARTIAL-FAIL`, `AB-AFTER-PREPARE-W`, `AB-AFTER-MUTATION`, `KS-SL1-POSTWRITE`, ... (6) | - | 0 | COVERED_DRAFT (concrete scenario IDs; 0 sealed) |
| `S5REC` | S-5 registry component | A | (none) | `PR-REC-UNREADABLE`, `PR-REC-REOBS-MISMATCH` | - | 0 | COVERED_DRAFT (concrete scenario IDs; 0 sealed) |
| `S5MAN` | S-5 manifest component | A | (none) | `PW-CLONE-BASE`, `PW-CLONE-CASE`, `PW-CLONE-WRONG-TYPE`, `PW-CLONE-NESTED`, ... (7) | - | 0 | COVERED_DRAFT (concrete scenario IDs; 0 sealed) |
| `S5CLO` | S-5 closure component | A | `PR-CLOSURE-COMPLETENESS` | `PR-REC-CLOSURE-UNKNOWN` | - | 0 | COVERED_DRAFT (concrete scenario IDs; 0 sealed) |
| `E01` | E-01 HostExact | A | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, `CL-CLEAN-FX2F-ALL`, ... (8) | `Q-I02` | CANNOT_BE_DELIBERATELY_VIOLATED_IN_A_GOVERNING_RUN (a mismatch of the verified tuple makes the run INVALID, never O1); detection covered by Q-I02 (Architect phase 1, G-2) | 0 | COVERED_DRAFT (concrete scenario IDs; 0 sealed) |
| `E02` | E-02 SingleDocument | A | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, `CL-CLEAN-FX2F-ALL`, ... (8) | `NC-E02` | - | 0 | COVERED_DRAFT (concrete scenario IDs; 0 sealed) |
| `E03` | E-03 LockHeld | A | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, `CL-CLEAN-FX2F-ALL`, ... (10) | `LK-LM1-CONFLICT`, `LK-LM2-CONFLICT` | - | 0 | COVERED_DRAFT (concrete scenario IDs; 0 sealed) |
| `E04` | E-04 SingleRackCadCommand | A | `Q-ROUTE-R1`, `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (11) | `NC-E04-R2` | - | 0 | COVERED_DRAFT (concrete scenario IDs; 0 sealed) |
| `E05` | E-05 KnownModesInactive | A | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, `CL-CLEAN-FX2F-ALL`, ... (8) | `NC-E05` | - | 0 | COVERED_DRAFT (concrete scenario IDs; 0 sealed) |
| `E06` | E-06 TransactionOwnership | A | `E6-C1`, `E6-C2`, `E6-C3`, `E6-C5`, ... (13) | `E6-C6`, `E6-C8`, `NC-E06`, `KS-SL1-TX-MISMATCH` | - | 0 | COVERED_DRAFT (concrete scenario IDs; 0 sealed) |
| `E07` | E-07 DatabaseComplete | A | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, `CL-CLEAN-FX2F-ALL`, ... (8) | `NC-E07` | - | 0 | COVERED_DRAFT (concrete scenario IDs; 0 sealed) |
| `E09` | E-09 OverrulesBounded | A | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, `CL-CLEAN-FX2F-ALL`, ... (8) | `NC-E09` | - | 0 | COVERED_DRAFT (concrete scenario IDs; 0 sealed) |
| `CLONE` | CLONE_POLICY_VERIFIED | A | (none) | `PW-CLONE-BASE`, `PW-CLONE-CASE`, `PW-CLONE-WRONG-TYPE`, `PW-CLONE-NESTED`, `PW-CLONE-SYMTAB` | - | 0 | COVERED_DRAFT (concrete scenario IDs; 0 sealed) |
| `E10` | E-10 KnownModuleSetAttestation | B | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, `CL-CLEAN-FX2F-ALL`, ... (8) | `NC-E10` | - | 0 | COVERED_DRAFT (concrete scenario IDs; 0 sealed) |
| `E11M` | E-11M ManagedLoadContinuity | B | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, `CL-CLEAN-FX2F-ALL`, ... (14) | `NC-E11M-PV`, `NC-E11M-PC`, `AB-AFTER-MUTATION` | - | 0 | COVERED_DRAFT (concrete scenario IDs; 0 sealed) |
| `E12A` | E-12 phase A | B | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, `CL-CLEAN-FX2F-ALL`, ... (11) | `NC-E12A`, `NC-S1-Y1`, `NC-E08-Y0`, `NC-E08-Y2` | - | 0 | COVERED_DRAFT (concrete scenario IDs; 0 sealed) |
| `E12C` | E-12 phase C | B | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, `CL-CLEAN-FX2F-ALL`, ... (11) | `WR-A-X0`, `WR-A-X1`, `WR-A-X2`, `WR-A-X3`, ... (82) | - | 0 | COVERED_DRAFT (concrete scenario IDs; 0 sealed) |
| `E12D` | E-12 phase D | B | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, `CL-CLEAN-FX2F-ALL`, ... (11) | `NC-E12D` | - | 0 | COVERED_DRAFT (concrete scenario IDs; 0 sealed) |
| `E12DET` | E-12 determinism | B | `EV-V-FX-2F`, `EV-V-FX-4F`, `EV-V-FX-BIG` | (none) | PREMISE_MEASURED (determinism is host behaviour measured by EV-V-*; a violation cannot be injected) | 0 | COVERED_DRAFT (concrete scenario IDs; 0 sealed) |
| `E13` | E-13 ProfileEquality | B | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, `CL-CLEAN-FX2F-ALL`, ... (8) | `Q-I04` | CANNOT_BE_DELIBERATELY_VIOLATED_IN_A_GOVERNING_RUN (profile values are MACHINE_PROFILE_BOUND tuple fields; a mismatch is INVALID); detection covered by Q-I04 (G-2) | 0 | COVERED_DRAFT (concrete scenario IDs; 0 sealed) |
| `A1` | A-1 | B | `EA1-READONLY-OPENS` | (none) | PREMISE_MEASURED (host behaviour; a violation cannot be injected: EA1 measures it) | 0 | COVERED_DRAFT (concrete scenario IDs; 0 sealed) |
| `A2` | A-2 | B | `EA2-DEFERRED-EVENTS` | (none) | PREMISE_MEASURED (host behaviour; EA2 measures it) | 0 | COVERED_DRAFT (concrete scenario IDs; 0 sealed) |
| `WUM` | WARMUP_MEMBERSHIP | B | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, `CL-CLEAN-FX2F-ALL`, ... (15) | `NC-WU-FOREIGN` | - | 0 | COVERED_DRAFT (concrete scenario IDs; 0 sealed) |
| `LIBF` | LIBRARY_FRESHNESS | B | (none) | `PR-SOURCE-SWAP-MODIFY`, `PR-SOURCE-SWAP-DELETE`, `PR-SOURCE-SWAP-NEG-COPY`, `PR-TORN-READ` | - | 0 | COVERED_DRAFT (concrete scenario IDs; 0 sealed) |
| `E14` | E-14 KindReadiness | A — NOT_PART_OF_CT21D_CHARACTERIZATION_PREDICATE | `KS-KR-RECORD` | `KS-SL1-POSTWRITE`, `KS-SL1-TX-MISMATCH`, `KS-SL4-NEG-DET` | OUTSIDE_THE_PREDICATE (recorded UNDER_CHARACTERIZATION; V2.2 PA-4) | 0 | COVERED_DRAFT (concrete scenario IDs; 0 sealed) |
| `E15` | E-15 ContractBinding | A — NOT_PART_OF_CT21D_CHARACTERIZATION_PREDICATE | `KS-KR-RECORD` | (none) | OUTSIDE_THE_PREDICATE (recorded UNDER_CHARACTERIZATION; V2.2 PA-4) | 0 | COVERED_DRAFT (concrete scenario IDs; 0 sealed) |

Rows with state `GAP` or `NEGATIVE_CONTROL_MISSING`: **0**. Groups of the registry with no serving scenario: **none**.

## 3. Group coverage

| GROUP | SCENARIOS (primary GROUP) | SCENARIOS SERVING IT (GROUPS_SERVED) | SEALED |
|---|---|---|---|
| `Q` | 21 | 21 | 0 |
| `WU` | 8 | 16 | 0 |
| `ID-A` | 3 | 11 | 0 |
| `ID-B` | 0 | 8 | 0 |
| `E4` | 3 | 11 | 0 |
| `LK` | 4 | 12 | 0 |
| `E6` | 9 | 17 | 0 |
| `E8` | 4 | 12 | 0 |
| `E9` | 1 | 9 | 0 |
| `E10` | 1 | 9 | 0 |
| `E11M` | 2 | 10 | 0 |
| `E12-V` | 5 | 13 | 0 |
| `E12-L` | 12 | 12 | 0 |
| `EA1` | 1 | 1 | 0 |
| `EA2` | 1 | 1 | 0 |
| `SC-A` | 49 | 57 | 0 |
| `SC-B` | 40 | 40 | 0 |
| `PP` | 1 | 1 | 0 |
| `PR-REC` | 4 | 12 | 0 |
| `PR-FRESH` | 4 | 12 | 0 |
| `PW` | 1 | 9 | 0 |
| `PW-CLONE` | 5 | 5 | 0 |
| `AB` | 2 | 2 | 0 |
| `KS` | 13 | 13 | 0 |
| `KR` | 1 | 1 | 0 |
| `U-1..U-8` | 8 | 8 | 0 |
| `U-6b` | 1 | 1 | 0 |
| `S1` | 1 | 1 | 0 |
| `OU` | 10 | 10 | 0 |
| `CL` | 2 | 2 | 0 |

## 4. Status

```text
BA-02b = DRAFT V2     every control has concrete draft scenarios; 0 sealed     BASE-20 = NOT MET (depends on NB-2)
CT21D_AUTHORITY_BASELINE_READY = FALSE
```
