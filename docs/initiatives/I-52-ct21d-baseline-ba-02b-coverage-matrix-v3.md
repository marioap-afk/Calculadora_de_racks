# I-52 — CT-21D Baseline Artifact BA-02b: Control to Group to Scenario Coverage Matrix (DRAFT V3)

> **BASELINE ARTIFACT BA-02b V3 — DRAFT, NOT SEALED. Design / documentation only.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-COVERAGE-MATRIX
> ARTIFACT_VERSION          = 3-DRAFT   (supersedes 2-DRAFT, blob 04ff81de53a77c4d0e471f79ef70ab1db45eb048, which stays as history)
> STATUS AUTHORITY          = the BA-11 registry entry of this artifact is the only authority for its status and hashes;
>                             this header is frozen, non-authoritative text of the candidate bytes
> DEPENDS ON                = BA-02a V3 (registry) and BA-04 V3 (catalog); BLOCKED by NB-2 for sealing
> PHASE-2 RULING APPLIED    = BA02-F01..F06 (AR2-08, AR2-09, AR2-10), BA02-F09 (AR2-21)
> BASE ITEM                 = BASE-20: NOT MET
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 1. Normative derivation rules (AR2-08..AR2-10)

1. **Control to group.** `GROUPS_SERVED(s)` = the union of `AFFECTED_GROUPS(c)` of BA-02a for every control `c` in `CONTROLS_EXERCISED(s)`, plus the evidence or `NOT_A_CONTROL` groups the scenario declares. BA-04 V3 derives the field with this rule, so sections 2 and 3 agree by construction.
2. **Fail-closed contribution.** PASS coverage counts only the controls of `CONTROLS_EXERCISED`. A `FAIL` or `UNKNOWN` of **any** control observed in a governing run counts toward the verdict of that control's group, whether or not the scenario lists it.
3. **Negative controls.** A scenario is a negative control of `c` only if `TARGET_NEGATIVE_CONTROL = c` **and** its expected result for `c` names `c`'s `FAIL`, `UNKNOWN`, difference or rejection. A scenario that detects `c` only as a secondary channel is listed in the column `SECONDARY CHANNEL`. Qualification scenarios (`Q-*`) target an instrument, never a control: they appear only in the no-violation reason.
4. **Countable scenarios.** Coverage counts only `GOVERNING` scenarios of mode `CHARACTERIZATION_RUN` or `VALIDATION` that are not exploratory and do not belong to an evidence group. `DRY_RUN`, `LEARNING`, non-governing and evidence-group scenarios are listed in their own column and are **not** clean coverage.
5. **Rule for `BASELINE_READY`** (phase-1 ruling B). Every mandatory control needs at least one concrete scenario whose expected result is `SEALED` or `SEALED_PENDING_PIN`, and a negative control or a sealed reason that it cannot be violated. **No scenario is sealed**, so the column `SEALED` is 0 everywhere.

## 2. Control coverage

| KEY | CONTROL | CLASS | IN_PREDICATE | CLEAN (governing) | NEGATIVE CONTROLS (target = this control) | SECONDARY CHANNEL | NON-GOVERNING / EVIDENCE / LEARNING / DRY RUN | NO-VIOLATION REASON | SEALED | STATE |
|---|---|---|---|---|---|---|---|---|---|---|
| `S1` | S-1 staged reads | A | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (13) | `NC-S1-Y1`, `NC-S1-Y1-SILENT` | (none) | `E4-LEARN-R1-LM1`, `LK-LM1-PROPS`, `LK-LM1-CONFLICT`, ... (6) | - | 0 | COVERED_DRAFT |
| `S2` | S-2 pass 1 | A | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (13) | `WR-A-X0`, `WR-A-X1`, `WR-D-X0`, ... (16) | `WR-SA-X2`, `WR-SA-X3`, `WR-SA-X4` | `E4-LEARN-R1-LM1`, `LK-LM1-PROPS`, `LK-LM1-CONFLICT`, ... (6) | - | 0 | COVERED_DRAFT |
| `S3` | S-3 pass 2 | A | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (13) | `WR-A-X2`, `WR-A-X3`, `WR-A-X4`, ... (30) | `WR-A-X0`, `WR-D-X0`, `WR-E-X0`, ... (5) | `E4-LEARN-R1-LM1`, `LK-LM1-PROPS`, `LK-LM1-CONFLICT`, ... (6) | - | 0 | COVERED_DRAFT |
| `E08` | E-08 read-set re-observation (S-4) | A | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (13) | `NC-E08-Y0`, `NC-E08-Y0-SILENT`, `NC-E08-Y2`, ... (14) | `WR-F-X0`, `WR-F-X1`, `WR-F-X2`, ... (18) | `E4-LEARN-R1-LM1`, `LK-LM1-PROPS`, `LK-LM1-CONFLICT`, ... (6) | - | 0 | COVERED_DRAFT |
| `S5` | S-5 abort verification | A | YES | ONLY_ON_ABORT_PATHS: S-5 is evaluated only when a run aborts, which needs an induced failure of another control, so no run without a deliberate violation evaluates it; its non-violated evaluations are the scenarios that expect S-5 PASS (ABORT-VERIFY match): AB-AFTER-PREPARE-W, AB-AFTER-MUTATION | `NC-S5-O6` | (none) | (none) | - | 0 | COVERED_DRAFT |
| `S5REC` | S-5 registry component | A | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (13) | `PR-REC-UNREADABLE`, `PR-REC-REOBS-MISMATCH` | (none) | `E4-LEARN-R1-LM1`, `LK-LM1-PROPS`, `LK-LM1-CONFLICT`, ... (6) | - | 0 | COVERED_DRAFT |
| `S5MAN` | S-5 manifest component | A | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (13) | `PW-IMPORT-PARTIAL-FAIL` | (none) | `E4-LEARN-R1-LM1`, `LK-LM1-PROPS`, `LK-LM1-CONFLICT`, ... (6) | - | 0 | COVERED_DRAFT |
| `S5CLO` | S-5 closure component | A | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (13) | `PR-REC-CLOSURE-UNKNOWN` | `PW-CLONE-WRONG-TYPE` | `E4-LEARN-R1-LM1`, `LK-LM1-PROPS`, `LK-LM1-CONFLICT`, ... (6) | - | 0 | COVERED_DRAFT |
| `E01` | E-01 HostExact | A | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (13) | (none) | (none) | `E4-LEARN-R1-LM1`, `LK-LM1-PROPS`, `LK-LM1-CONFLICT`, ... (6) | CANNOT_BE_DELIBERATELY_VIOLATED_IN_A_GOVERNING_RUN: a mismatch of the verified tuple makes the run INVALID, never O1 (G-2); detection covered by qualification (Q-I02) | 0 | COVERED_DRAFT |
| `E02` | E-02 SingleDocument | A | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (13) | `NC-E02` | (none) | `E4-LEARN-R1-LM1`, `LK-LM1-PROPS`, `LK-LM1-CONFLICT`, ... (6) | - | 0 | COVERED_DRAFT |
| `E03` | E-03 LockHeld | A | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (13) | `NC-E03-NOT-HELD`, `NC-E03-REACQUIRE` | (none) | `E4-LEARN-R1-LM1`, `LK-LM1-PROPS`, `LK-LM1-CONFLICT`, ... (6) | - | 0 | COVERED_DRAFT |
| `E04` | E-04 SingleRackCadCommand | A | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (13) | `NC-E04-R2` | (none) | `E4-LEARN-R1-LM1`, `LK-LM1-PROPS`, `LK-LM1-CONFLICT`, ... (6) | - | 0 | COVERED_DRAFT |
| `E05` | E-05 KnownModesInactive | A | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (13) | `NC-E05` | (none) | `E4-LEARN-R1-LM1`, `LK-LM1-PROPS`, `LK-LM1-CONFLICT`, ... (6) | - | 0 | COVERED_DRAFT |
| `E06` | E-06 TransactionOwnership | A | YES | `E6-C1`, `E6-C2`, `E6-C3`, ... (18) | `E6-C6`, `E6-C8`, `NC-E06` | `KS-SL1-TX-MISMATCH` | `E6-C4`, `E4-LEARN-R1-LM1`, `LK-LM1-PROPS`, ... (7) | - | 0 | COVERED_DRAFT |
| `E07` | E-07 DatabaseComplete | A | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (13) | `NC-E07` | (none) | `E4-LEARN-R1-LM1`, `LK-LM1-PROPS`, `LK-LM1-CONFLICT`, ... (6) | - | 0 | COVERED_DRAFT |
| `E09` | E-09 OverrulesBounded | A | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (13) | `NC-E09` | (none) | `E4-LEARN-R1-LM1`, `LK-LM1-PROPS`, `LK-LM1-CONFLICT`, ... (6) | - | 0 | COVERED_DRAFT |
| `E10` | E-10 KnownModuleSetAttestation | B | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (13) | `NC-E10` | `NC-E11M-PV`, `NC-E11M-PC`, `AB-AFTER-MUTATION` | `E4-LEARN-R1-LM1`, `LK-LM1-PROPS`, `LK-LM1-CONFLICT`, ... (6) | - | 0 | COVERED_DRAFT |
| `E11M` | E-11M ManagedLoadContinuity | B | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (13) | `NC-E11M-PV`, `NC-E11M-PC` | `NC-E10`, `AB-AFTER-MUTATION` | `E4-LEARN-R1-LM1`, `LK-LM1-PROPS`, `LK-LM1-CONFLICT`, ... (151) | - | 0 | COVERED_DRAFT |
| `E12A` | E-12 phase A MUTATION | B | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (13) | `NC-E12A` | `NC-S1-Y1`, `NC-E08-Y0`, `NC-E08-Y2`, `NC-S5-O6` | `E4-LEARN-R1-LM1`, `LK-LM1-PROPS`, `LK-LM1-CONFLICT`, ... (6) | - | 0 | COVERED_DRAFT |
| `E12C` | E-12 phase C W-SCAN | B | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (14) | `NC-E12C` | `WR-A-X1`, `WR-A-X2`, `WR-A-X3`, ... (66) | `E4-LEARN-R1-LM1`, `LK-LM1-PROPS`, `LK-LM1-CONFLICT`, ... (6) | - | 0 | COVERED_DRAFT |
| `E12D` | E-12 phase D subscription | B | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (13) | `NC-E12D` | (none) | `E4-LEARN-R1-LM1`, `LK-LM1-PROPS`, `LK-LM1-CONFLICT`, ... (6) | - | 0 | COVERED_DRAFT |
| `E12DET` | E-12 determinism of the expected mutation set | B | YES | `EV-V-FX-2F`, `EV-V-FX-4F`, `EV-V-FX-BIG` | (none) | (none) | (none) | PREMISE_MEASURED: determinism is host behaviour measured by EV-V-* (a violation cannot be injected) | 0 | COVERED_DRAFT |
| `E13` | E-13 ProfileEquality | B | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (13) | (none) | (none) | `E4-LEARN-R1-LM1`, `LK-LM1-PROPS`, `LK-LM1-CONFLICT`, ... (6) | CANNOT_BE_DELIBERATELY_VIOLATED_IN_A_GOVERNING_RUN: profile values are MACHINE_PROFILE_BOUND tuple fields (G-2); detection covered by qualification (Q-I04) | 0 | COVERED_DRAFT |
| `E14` | E-14 KindReadiness | A | NO | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (8) | (none) | (none) | (none) | OUTSIDE_THE_PREDICATE: recorded UNDER_CHARACTERIZATION (V2.2 PA-4); no violation list | 0 | COVERED_DRAFT |
| `E15` | E-15 ContractBinding | A | NO | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (8) | (none) | (none) | (none) | OUTSIDE_THE_PREDICATE: recorded UNDER_CHARACTERIZATION (V2.2 PA-4); no violation list | 0 | COVERED_DRAFT |
| `A1` | A-1 read-only opens emit no write events | B | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (14) | (none) | (none) | `E4-LEARN-R1-LM1`, `LK-LM1-PROPS`, `LK-LM1-CONFLICT`, ... (6) | PREMISE_MEASURED: host behaviour; a violation cannot be injected (EA1 and the clean sample measure it) | 0 | COVERED_DRAFT |
| `A2` | A-2 no deferred write events after V0 | B | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (13) | (none) | (none) | `E4-LEARN-R1-LM1`, `LK-LM1-PROPS`, `LK-LM1-CONFLICT`, ... (6) | PREMISE_MEASURED: host behaviour (EA2 and the clean sample measure it) | 0 | COVERED_DRAFT |
| `WUM` | WARMUP_MEMBERSHIP | B | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (14) | `NC-WU-FOREIGN` | (none) | `E4-LEARN-R1-LM1`, `LK-LM1-PROPS`, `LK-LM1-CONFLICT`, ... (151) | - | 0 | COVERED_DRAFT |
| `LIBF` | LIBRARY_FRESHNESS | B | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (13) | `PR-SOURCE-SWAP-NEG-COPY`, `PR-TORN-READ` | (none) | `E4-LEARN-R1-LM1`, `LK-LM1-PROPS`, `LK-LM1-CONFLICT`, ... (6) | - | 0 | COVERED_DRAFT |
| `CLONE` | CLONE_POLICY_VERIFIED | A | YES | `CL-CLEAN-FXIMP-FP` | (none) | `PW-CLONE-CASE` | (none) | DESIGNED_STIMULUS: the PA-12 variants present homonyms under the non-replacing policy; acceptance is success or REJECT, so the control is exercised by its variants, not violated | 0 | COVERED_DRAFT |

Rows whose state is not `COVERED_DRAFT`: **0** (none).

## 3. Required scenario classes and minimum catalog requirement (V2.1 31 as amended by V2.2 PA-1.4)

| KEY | REQUIRED SCENARIO CLASSES (V2.1 31 / V2.2 PA-1.4) | MINIMUM CATALOG REQUIREMENT | SCENARIOS THAT DISCHARGE IT | NOTE | STATE |
|---|---|---|---|---|---|
| `S1` | CL-CLEAN, WR-* (X0..X6), KS-* | clean runs per family; each persistent-change writer at X1..X4 detected by the semantic comparison; a single-field mutation per compared field | `NC-S1-Y1` | the single-field mutation per compared field is qualified by Q-I11 (V2.1 4.3 row I-11 negative control); the product-run negative is NC-S1-Y1 | MET_IN_DRAFT |
| `S2` | CL-CLEAN, WR-* (X0..X6), KS-* | as S-1 | `WR-A-X0`, `WR-A-X1`, `WR-D-X0`, ... (16) | - | MET_IN_DRAFT |
| `S3` | CL-CLEAN, WR-* (X0..X6), KS-* | as S-1 | `WR-A-X2`, `WR-A-X3`, `WR-A-X4`, ... (33) | - | MET_IN_DRAFT |
| `E08` | E8, NC-E08 | a clean re-observation and a deliberate read-set change detected at PV | `NC-E08-Y0`, `NC-E08-Y2` | - | MET_IN_DRAFT |
| `S5` | AB-*, PR-*, PW-* | abort after PREPARE-W, after MUTATION, after a Commit() exception path (T1) | `AB-AFTER-PREPARE-W`, `AB-AFTER-MUTATION`, `AB-COMMIT-EXCEPTION-T1-A` | - | MET_IN_DRAFT |
| `S5REC` | PR-*, Q-I13, CLOSURE_COMPLETENESS_CONTROL | the record covers every closure member; a closure the traversal cannot classify refuses the run (PA-1.4) | `PR-REC-UNREADABLE`, `PR-REC-REOBS-MISMATCH` | - | MET_IN_DRAFT |
| `S5MAN` | PW-* | additions listed with exact fingerprints; a partial failed import is never legitimized (PA-1.4) | `PW-IMPORT-PARTIAL-FAIL` | - | MET_IN_DRAFT |
| `S5CLO` | PR-*, Q-I13, CLOSURE_COMPLETENESS_CONTROL | as S5REC (PA-1.4) | `PR-REC-CLOSURE-UNKNOWN`, `PR-CLOSURE-COMPLETENESS` | - | MET_IN_DRAFT |
| `E01` | CL-CLEAN, NC-E01 | one deliberate violation refused at P0 (replaced by G-2: qualification Q-I02) | `Q-I02` | NC-E01 withdrawn by G-2 | MET_IN_DRAFT |
| `E02` | CL-CLEAN, NC-E02 | one deliberate violation refused at P0 | `NC-E02` | - | MET_IN_DRAFT |
| `E03` | LK-<mode>, NC-E03 | each candidate mode; a conflicting writer probe | `LK-LM1-PROPS`, `LK-LM2-PROPS`, `LK-LM1-CONFLICT`, ... (6) | - | MET_IN_DRAFT |
| `E04` | E4, NC-E04 | each designated invocation route; a non-designated route excluded | `E4-VAL-R1`, `NC-E04-R2` | - | MET_IN_DRAFT |
| `E05` | CL-CLEAN, NC-E05 | one deliberate violation refused at P0 | `NC-E05` | - | MET_IN_DRAFT |
| `E06` | E6-C1..E6-C8, NC-E06 | all counter controls, including STALE_WRAPPER_CONTROL; a second transaction detected at a sampling point | `E6-C6`, `E6-C8`, `NC-E06`, `KS-SL1-TX-MISMATCH` | E6-C4 is EXPLORATORY (G-3) and does not count | MET_IN_DRAFT |
| `E07` | CL-CLEAN, NC-E07 | one deliberate violation refused at P0 | `NC-E07` | - | MET_IN_DRAFT |
| `E09` | E9, NC-E09 | subjects and classes consumed; an overrule with unknown effect refused | `NC-E09` | - | MET_IN_DRAFT |
| `E10` | E10, NC-E10 | a canary module changes the snapshot; quiescent equality | `NC-E10` | - | MET_IN_DRAFT |
| `E11M` | E11M, NC-E11M | a canary load delivers an event; loss of subscription detected | `NC-E11M-PV`, `NC-E11M-PC`, `Q-I09-LOSS` | loss detection is qualified by Q-I09-LOSS (a product run with a lost subscription is INVALID, 21.1) | MET_IN_DRAFT |
| `E12A` | EV-V-<plan>, NC-E12A | validation only with a FROZEN EVM (PA-2); an unattributable event aborts before Commit() | `NC-E12A`, `EV-V-FX-2F` | - | MET_IN_DRAFT |
| `E12C` | WR-*, NC-E12C | event-triggered and silent writer variants; the detection channel recorded | `NC-E12C`, `WR-A-X1-EV`, `WR-SA-X1` | - | MET_IN_DRAFT |
| `E12D` | EV-V-<plan>, NC-E12C | validation only with a FROZEN EVM (PA-2) | `NC-E12D`, `Q-I10-LOSS` | - | MET_IN_DRAFT |
| `E12DET` | EV-V-<plan> | validation only with a FROZEN EVM (PA-2) | `EV-V-FX-2F`, `EV-V-FX-4F`, `EV-V-FX-BIG` | - | MET_IN_DRAFT |
| `E13` | CL-CLEAN, NC-E13 | profile equality and a deliberate difference (replaced by G-2: qualification Q-I04) | `Q-I04` | NC-E13 withdrawn by G-2 | MET_IN_DRAFT |
| `E14` | KS-* | recorded UNDER_CHARACTERIZATION | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (9) | - | MET_IN_DRAFT |
| `E15` | KS-* | recorded UNDER_CHARACTERIZATION | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (9) | - | MET_IN_DRAFT |
| `A1` | EA1 | read-only opens with zero write events | `EA1-READONLY-OPENS` | - | MET_IN_DRAFT |
| `A2` | EA2 | known-correct commits with zero deferred events | `EA2-DEFERRED-EVENTS` | - | MET_IN_DRAFT |
| `WUM` | WU, WARMUP_BOUNDARY_CONTROL | coverage criterion measured; boundary control passes (PA-1.4) | `WU-BOUNDARY`, `NC-WU-FOREIGN`, `WU-DRY-CL-CLEAN-FX1F-F0` | dry runs per scenario (AR2-42) | MET_IN_DRAFT |
| `LIBF` | PR-*, Q-I13, SOURCE_SWAP_CONTROL | a modified source detected; a stale cache detected; torn acquisition detected; ACQUISITION_AUTHORITY recorded (PA-1.4) | `PR-SOURCE-SWAP-MODIFY`, `PR-SOURCE-SWAP-DELETE`, `PR-SOURCE-SWAP-NEG-COPY`, ... (5) | the stale cache is qualified by Q-I13 | MET_IN_DRAFT |
| `CLONE` | PW-*, CLONE_NON_REPLACE_CONTROL | all variants of PA-12 | `PW-CLONE-BASE`, `PW-CLONE-CASE`, `PW-CLONE-WRONG-TYPE`, ... (5) | - | MET_IN_DRAFT |

## 4. Group coverage

| GROUP | PRIMARY (GROUP) | SERVING (GROUPS_SERVED) | SERVING, GOVERNING AND COUNTABLE | SEALED |
|---|---|---|---|---|
| `Q` | 25 | 26 | 1 | 0 |
| `WU` | 147 | 205 | 54 | 0 |
| `ID-A` | 3 | 58 | 52 | 0 |
| `ID-B` | 0 | 58 | 52 | 0 |
| `E4` | 4 | 58 | 52 | 0 |
| `LK` | 6 | 50 | 44 | 0 |
| `E6` | 9 | 66 | 59 | 0 |
| `E8` | 4 | 69 | 63 | 0 |
| `E9` | 1 | 58 | 52 | 0 |
| `E10` | 1 | 58 | 52 | 0 |
| `E11M` | 2 | 203 | 52 | 0 |
| `E12-V` | 6 | 132 | 126 | 0 |
| `E12-L` | 16 | 16 | 0 | 0 |
| `EA1` | 1 | 28 | 22 | 0 |
| `EA2` | 1 | 27 | 21 | 0 |
| `SC-A` | 59 | 132 | 126 | 0 |
| `SC-B` | 32 | 112 | 106 | 0 |
| `PP` | 1 | 1 | 0 | 0 |
| `PR-REC` | 4 | 50 | 44 | 0 |
| `PR-FRESH` | 4 | 49 | 43 | 0 |
| `PW` | 1 | 45 | 39 | 0 |
| `PW-CLONE` | 5 | 10 | 10 | 0 |
| `AB` | 3 | 8 | 8 | 0 |
| `KS` | 13 | 132 | 126 | 0 |
| `KR` | 0 | 11 | 11 | 0 |
| `U-1..U-8` | 8 | 8 | 0 | 0 |
| `U-6b` | 1 | 1 | 0 | 0 |
| `S1` | 1 | 1 | 0 | 0 |
| `OU` | 11 | 11 | 0 | 0 |
| `CL` | 1 | 1 | 0 | 0 |

Groups of V2.2 PA-1.3 with no serving scenario: **none**.

## 5. Design-level checks (V2.1 31, repeated on the catalog)

| Check | Result |
|---|---|
| 1. every class-A or class-B control has at least one group; every group has at least one serving scenario | PASS |
| 2. every mandatory control has a governing clean scenario and a negative control, or a recorded reason | PASS (in draft) |
| 3. every scenario's `CONTROL_CLASSES_EXERCISED` is a subset of the registry classes | PASS |
| 4. no control without a group and no group without a scenario (else the matrix is incomplete and `ALT21D_HOST_PASS` cannot be `TRUE`) | PASS |

## 6. Status

```text
BA-02b = DRAFT V3     0 sealed scenarios     BASE-20 = NOT MET (depends on NB-2)
CT21D_AUTHORITY_BASELINE_READY = FALSE
```
