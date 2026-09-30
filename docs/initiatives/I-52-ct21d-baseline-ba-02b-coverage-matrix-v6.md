# I-52 — CT-21D Baseline Artifact BA-02b: Control to Group to Scenario Coverage Matrix (DRAFT V6)

> **BASELINE ARTIFACT BA-02b V6 — DRAFT, NOT SEALED. Design / documentation only.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-COVERAGE-MATRIX
> ARTIFACT_VERSION          = 6-DRAFT (supersedes 5-DRAFT, blob 139bdafcd2c71a210b9c2503cd6f387e01dc931e, which stays as history)
> STATUS AUTHORITY          = the BA-11 registry entry of this artifact is the only authority for its status and hashes; this header is frozen,
>                             non-authoritative text of the candidate bytes
> AUTHORITY_CONTRACT        = CT-21D V2.1..V2.5 (V2.4 rev. 3 and V2.5 rev. 2 textually verified) + V2.6 (draft, pending review:
>                             docs/initiatives/I-52-ct21d-authority-contract-v2.6.md, the micro-errata of AR6-05); the effective contract is not
>                             agreed until V2.6 is verified (AR6-06)
> BOUND_PRODUCT_SHA         = 95690c28 (AR6-01; decisions section 224): a non-tuple bound item. This matrix states no code fact (Gate 2 relink
>                             assessment, BA-02b row: "No code facts"); it inherits the binding through BA-04 V6
> DELTA RULING APPLIED      = decisions section 223 (AR6-07, the BA-02b row of 223.4: cascade from BA-04 V6; AR6-02 through BA-04 V6) and section 220
>                             (AR5-05 and AR5-09 through BA-04 V6); section 217 (AR4-03, AR4-04, AR4-05, AR4-06, AR4-14 (a), AR4-16, AR4-17, AR4-20
>                             with the R-3 rulings (a)..(d)); section 214 (AR3-02, AR3-03, AR3-05, AR3-06, AR3-07, AR3-08, AR3-09, AR3-29, AR3-30) and
>                             section 211 (AR2-08..AR2-13 for BA02-F01..F06, AR2-21 for BA02-F09) where sections 220 and 223 do not change them
> DEPENDS_ON                = BA-01, BA-02a, BA-04 (entries of the BA-11 registry)
> PREREQUISITES             = its DEPENDS_ON entries: the readings this content applies are ruled (AR4-05 for PA-1.4, AR4-04 for NC-S5-O6, AR4-06 for
>                             the characterization build, AR4-20 for rule 5; AR5-05 for the per-build twins by content and AR5-09 for Q-I14-P, section
>                             220); the open Architect question AQ-V6-BA04-1 of BA-04 V6 section 8.3 changes RUN_PREREQUISITES only and no table here
>                             (section 6); BLOCKED by NB-2 for sealing
> BASE ITEM                 = BASE-20: NOT MET
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 0. Inputs

- **Registry:** BA-02a V3 (blob `57ef0ca4749bfae0967fd60d8f447db7939eaf7d`), unchanged (AR3-29, AR4-19). Its references to versions of other drafts are frozen, non-authoritative text (AR2-44); a later version of those drafts does not require a new BA-02a. The dated re-execution of its byte-for-byte comparison with V2.2 PA-1.2 and PA-1.3 is recorded outside its bytes, in the ratification records (AR3-29).
- **Catalog:** BA-04 V6 (`I-52-ct21d-baseline-ba-04-scenario-catalog-v6.json`, 437 scenarios). Every table below is derived from it by the rules of section 1; the definitions of `GROUPS_SERVED` and `CONTRACT_ASSIGNED_GROUPS` are those of BA-04 V6 section 1 (AR4-17).
- **Mechanical check:** `docs/automation/evidence/I-52-ct21d-ba04-v6-checks.py` (SHA-256 of its LF text `eae605d21463c7b771ec6e7dad0a41cd18128a4fa8db90364a1a1e35326ea9aa`) recomputes `GROUPS_SERVED` by the definition of BA-04 section 1, the states of section 2, the verbatim rows, the discharge lists and the enumerated sets of section 3, and the group coverage of section 4. The check and its output `docs/automation/evidence/I-52-ct21d-ba04-v6-checks-output.json` are bound by hash in the BA-02b entry of the BA-11 registry (its paths), the entry that consumes both inputs, and never in the paths of BA-04 (AR4-17; R1:BA11V4-04). The output holds the SHA-256 of this file, so it cannot be hashed here; the candidate rulings of BA-04 and BA-02b cite the hashes of both files.

## 1. Normative derivation rules

1. **Control to group (the definition of BA-04 section 1; AR4-17).** `GROUPS_SERVED(s)` and `CONTRACT_ASSIGNED_GROUPS(s)` are the fields of BA-04 as the definition of BA-04 section 1 fixes them (authority AR2-08, AR3-06 and AR4-03); this matrix applies that definition and does not restate it. A contract-assigned membership (sources: V2.1 10.8, V2.1 10.6 and 21.4, V2.2 PA-12, and the AR4-03 extension of AR3-06 for the `KS-*` class) is **evidence only and never PASS**: it is kept apart from `GROUPS_SERVED`, never counted as serving or countable, and never enters `N_strict` (AR4-14 (a)). The published check recomputes both fields by that definition and accepts a contract-assigned entry only when its clause cites an admitted source, so sections 2 and 4 agree with the catalog by construction.
2. **Fail-closed contribution.** PASS coverage counts only the controls of `CONTROLS_EXERCISED`. A `FAIL` or `UNKNOWN` of **any** control observed in a governing run counts toward the verdict of that control's group, whether or not the scenario lists it.
3. **Negative controls.** A scenario is a negative control of `c` only if `DELIBERATE_VIOLATION_FLAG = YES`, `TARGET_NEGATIVE_CONTROL = c` **and** its expected result for `c` names `c`'s `FAIL`, `UNKNOWN`, difference or rejection. A scenario with a `DESIGNED_STIMULUS` carries flag `NO` and is not a negative control (AR3-05); with no deliberate violation it is a clean governing run (V2.1 1.3). A scenario that detects `c` only as a secondary channel is listed in the column `SECONDARY CHANNEL`. In section 2, qualification scenarios (`Q-*`) target an instrument, never a control, and appear only in the no-violation reason; section 3 may cite a `Q-*` scenario where a clause of the minimum catalog requirement is the qualification of an instrument (rule 6).
4. **Countable scenarios.** Control coverage counts only `GOVERNING` scenarios of mode `CHARACTERIZATION_RUN` or `VALIDATION` that are not exploratory, are not in the AR3-01 mode and do not belong to an evidence group. Evidence-group scenarios are listed in their own column; `DRY_RUN`, `LEARNING`, non-governing and AR3-01-mode scenarios in another; neither is clean coverage. Governing scenarios on the `CHARACTERIZATION_BUILD` count like those on the `PRODUCT_BUILD`: AR4-06 accepts that reading (one admissible BUILD_BOUND profile per build, the qualification of every instrument on each build, the recorded build equivalence for the product-build use of pins derived on the characterization build); the column "NEGATIVE CONTROLS ON THE CHARACTERIZATION BUILD" of section 2 shows that dependency (informational, like the K-3/K-8 column). For evidence groups the verdict rule is `EVIDENCE_COMPLETE` over their governing runs (V2.2 PA-3), so their countable column shows N/A (PA-3); KR shows N/A (PA-4). The column "GOVERNING SCENARIOS SERVING THE GROUP" of section 4 counts, for every group, the `GOVERNING` scenarios whose `GROUPS_SERVED` holds the group; contract-assigned memberships are excluded (R1:BA02V4-N06); for an evidence group it is the basis of `EVIDENCE_COMPLETE`.
5. **Conditional negative controls (AR3-03; AR4-20 R-3 (a)..(d)).** A negative control is **conditional** when its result for the target depends on `CER-WIT`, `CER-SIL` or `CER-PERSIST` (BA-04 V6 section 3). A control whose negative controls are all conditional is `COVERED_DRAFT_CONDITIONAL` in the draft. (a) At acceptance its state is `NEGATIVE_CONTROL_NOT_EVALUATED` (never MET or PASS) unless at least one of its conditional negative controls resolved to its detection branch, read per control; a scenario resolved `NOT_RUNNABLE`, or on its non-detection branch, leaves its cell uncovered. (b) At acceptance, `NEGATIVE_CONTROL_NOT_EVALUATED` makes V2.1 31 check 2 **NOT MET** for that control. (c) V2.1 1.6 "the coverage matrix (section 31) is complete" reads as checks 1 to 4 all met, so a control in `NEGATIVE_CONTROL_NOT_EVALUATED` keeps `ALT21D_HOST_PASS` at `NOT_EVALUATED` and never `TRUE`. (d) The group verdict of a conditional negative control is a designed detection only on its detection branch (BA-04 V6 section 3, the `CER-WIT` convention).
6. **Section 3 derivation.** The rows in effect are the rows of V2.1 31 together with the rows of V2.2 PA-1.4 (the reading of AR4-05, section 3). Each minimum catalog requirement is split into clauses; each clause is a verbatim fragment of the requirement, and a conjunctive clause is split (R02, R14). A clause is discharged by the listed scenarios; each one is a member of a scenario class of its row (an ID prefix of V2.1 26.3, a group name of 26.3 read as "serves that group", or the literal class `OU-O1..O7` = `OU-O1`..`OU-O7`), or a declared exception (a clause that the contract leaves to an instrument qualification, a withdrawal by phase-1 ruling G-2, a class named by the row's CONTROL cell, or a required member outside the literal class). A clause is `MET_IN_DRAFT` when at least one listed scenario exists in BA-04 V6 and is not `EXPLORATORY_NON_GOVERNING`; **a clause quantified over an enumerated set** ("each", "all", "per", "one ... per") is `MET_IN_DRAFT` only when the listed scenarios cover **every member** of the set named in the column "ENUMERATED SET" of section 3.2, a member kept exploratory by phase-1 ruling G-3 (E6-C4) counting as present and never toward PASS (R1:BA02V4-N04). A row is `MET_IN_DRAFT` when all its clauses are; a control is `MET_IN_DRAFT` when every row that names it is.
7. **Rule for `BASELINE_READY`** (phase-1 ruling B). Every mandatory control needs at least one concrete scenario whose expected result is `SEALED` or `SEALED_PENDING_PIN`, and a negative control or a sealed reason that it cannot be violated. **No scenario is sealed**, so the column `SEALED` is 0 everywhere.

## 2. Control coverage

| KEY | CONTROL | CLASS | IN_PREDICATE | CLEAN (governing) | CLEAN FREE OF K-3 AND K-8 | NEGATIVE CONTROLS | CONDITIONAL NEGATIVE CONTROLS | NEGATIVE CONTROLS ON THE CHARACTERIZATION BUILD | SECONDARY CHANNEL | EVIDENCE-GROUP SCENARIOS | NON-GOVERNING / LEARNING / DRY RUN | NO-VIOLATION REASON | SEALED | STATE |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| `S1` | S-1 staged reads | A | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (24) | 0 | `NC-S1-Y1` | `NC-S1-Y1-SILENT` | 2 of 2 | (none) | (none) | (none) | - | 0 | COVERED_DRAFT |
| `S2` | S-2 pass 1 | A | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (38) | 0 | `WR-A-X0`, `WR-A-X1`, `WR-D-X0`, ... (15) | `WR-SA-X1` | 16 of 16 | (none) | `U-3` | (none) | - | 0 | COVERED_DRAFT |
| `S3` | S-3 pass 2 | A | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (38) | 0 | `WR-A-X2`, `WR-A-X3`, `WR-A-X4`, ... (30) | `WR-SA-X2`, `WR-SA-X3`, `WR-SA-X4` | 33 of 33 | `WR-A-X0`, `WR-A-X1`, `WR-D-X0`, ... (16) | `U-3`, `U-4` | (none) | - | 0 | COVERED_DRAFT |
| `E08` | E-08 read-set re-observation (S-4) | A | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (24) | 0 | `NC-E08-Y0`, `NC-E08-Y2`, `WR-F-X5`, ... (12) | `NC-E08-Y0-SILENT`, `NC-E08-Y2-SILENT` | 14 of 14 | `NC-S5-O6`, `WR-F-X0`, `WR-F-X1`, ... (19) | (none) | (none) | - | 0 | COVERED_DRAFT |
| `S5` | S-5 abort verification | A | YES | `AB-AFTER-PREPARE-W`, `KS-SL1-POSTWRITE`, `KS-SL1-TX-MISMATCH`, `KS-SL4-NEG-DET` | 3 | (none) | `NC-S5-O6` | 1 of 1 | `NC-CLONE-REPLACE` | (none) | (none) | - | 0 | COVERED_DRAFT_CONDITIONAL |
| `S5REC` | S-5 registry component | A | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (30) | 3 | `PR-REC-UNREADABLE`, `PR-REC-REOBS-MISMATCH` | (none) | 2 of 2 | (none) | (none) | (none) | - | 0 | COVERED_DRAFT |
| `S5MAN` | S-5 manifest component | A | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (28) | 3 | `PW-IMPORT-PARTIAL-FAIL` | (none) | 1 of 1 | (none) | (none) | (none) | - | 0 | COVERED_DRAFT |
| `S5CLO` | S-5 closure component | A | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (30) | 3 | `PR-REC-CLOSURE-UNKNOWN` | (none) | 0 of 1 | (none) | (none) | (none) | - | 0 | COVERED_DRAFT |
| `E01` | E-01 HostExact | A | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (30) | 3 | (none) | (none) | 0 of 0 | (none) | (none) | (none) | CANNOT_BE_DELIBERATELY_VIOLATED_IN_A_GOVERNING_RUN: a mismatch of the verified tuple makes the run INVALID, never O1 (G-2); detection covered by qualification (Q-I02, Q-I02-CB) | 0 | COVERED_DRAFT |
| `E02` | E-02 SingleDocument | A | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (30) | 3 | `NC-E02` | (none) | 0 of 1 | (none) | (none) | (none) | - | 0 | COVERED_DRAFT |
| `E03` | E-03 LockHeld | A | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (30) | 3 | `NC-E03-NOT-HELD`, `NC-E03-REACQUIRE` | (none) | 2 of 2 | (none) | (none) | `LK-LM1-PROPS`, `LK-LM1-CONFLICT`, `LK-LM2-PROPS`, `LK-LM2-CONFLICT` | - | 0 | COVERED_DRAFT |
| `E04` | E-04 SingleRackCadCommand | A | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (30) | 3 | `NC-E04-R2` | (none) | 0 of 1 | (none) | (none) | `E4-LEARN-R1-LM1`, `E4-LEARN-R1-LM2` | - | 0 | COVERED_DRAFT |
| `E05` | E-05 KnownModesInactive | A | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (30) | 3 | `NC-E05` | (none) | 0 of 1 | (none) | (none) | (none) | - | 0 | COVERED_DRAFT |
| `E06` | E-06 TransactionOwnership | A | YES | `E6-C1`, `E6-C2`, `E6-C3`, ... (40) | 13 | `E6-C6`, `E6-C8`, `E6-C6-CB`, ... (5) | (none) | 2 of 5 | (none) | (none) | `E6-C4`, `E6-C4-CB` | - | 0 | COVERED_DRAFT |
| `E07` | E-07 DatabaseComplete | A | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (30) | 3 | `NC-E07` | (none) | 0 of 1 | (none) | (none) | (none) | - | 0 | COVERED_DRAFT |
| `E09` | E-09 OverrulesBounded | A | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (30) | 3 | `NC-E09` | (none) | 0 of 1 | (none) | (none) | (none) | - | 0 | COVERED_DRAFT |
| `E10` | E-10 KnownModuleSetAttestation | B | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (30) | 3 | `NC-E10` | (none) | 1 of 1 | `NC-E11M-PV`, `NC-E11M-PC`, `AB-AFTER-MUTATION` | `U-5` | (none) | - | 0 | COVERED_DRAFT |
| `E11M` | E-11M ManagedLoadContinuity | B | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (30) | 3 | `NC-E11M-PV`, `NC-E11M-PC`, `AB-AFTER-MUTATION` | (none) | 3 of 3 | `NC-E10` | `U-5` | `WU-DRY-Q-I14-Y`, `WU-DRY-Q-I14-X`, `WU-DRY-Q-I14-SILENT-Y`, ... (167) | - | 0 | COVERED_DRAFT |
| `E12A` | E-12 phase A MUTATION | B | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (24) | 0 | (none) | `NC-E12A` | 1 of 1 | `NC-S1-Y1`, `NC-E08-Y0`, `NC-E08-Y2`, `NC-S5-O6` | (none) | (none) | - | 0 | COVERED_DRAFT_CONDITIONAL |
| `E12C` | E-12 phase C W-SCAN | B | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (27) | 0 | (none) | `NC-E12C`, `WR-A-X5`, `WR-A-X6`, ... (19) | 19 of 19 | `WR-A-X1`, `WR-A-X2`, `WR-A-X3`, ... (48) | `U-3` | (none) | - | 0 | COVERED_DRAFT_CONDITIONAL |
| `E12D` | E-12 phase D subscription | B | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (30) | 3 | `NC-E12D` | (none) | 1 of 1 | (none) | (none) | (none) | - | 0 | COVERED_DRAFT |
| `E12DET` | E-12 determinism of the expected mutation set | B | YES | `EV-V-FX-2F`, `EV-V-FX-4F`, `EV-V-FX-BIG` | 0 | (none) | (none) | 0 of 0 | (none) | (none) | (none) | PREMISE_MEASURED: determinism is host behaviour measured by EV-V-* (a violation cannot be injected) | 0 | COVERED_DRAFT |
| `E13` | E-13 ProfileEquality | B | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (30) | 3 | (none) | (none) | 0 of 0 | (none) | (none) | (none) | CANNOT_BE_DELIBERATELY_VIOLATED_IN_A_GOVERNING_RUN: profile values are MACHINE_PROFILE_BOUND tuple fields (G-2); detection covered by qualification (Q-I04, Q-I04-CB) | 0 | COVERED_DRAFT |
| `E14` | E-14 KindReadiness | A | NO | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (12) | 3 | (none) | (none) | 0 of 0 | (none) | (none) | (none) | OUTSIDE_THE_PREDICATE: recorded UNDER_CHARACTERIZATION (V2.2 PA-4); no violation list | 0 | COVERED_DRAFT |
| `E15` | E-15 ContractBinding | A | NO | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (12) | 3 | (none) | (none) | 0 of 0 | (none) | (none) | (none) | OUTSIDE_THE_PREDICATE: recorded UNDER_CHARACTERIZATION (V2.2 PA-4); no violation list | 0 | COVERED_DRAFT |
| `A1` | A-1 read-only opens emit no write events | B | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (11) | 1 | (none) | (none) | 0 of 0 | (none) | (none) | (none) | PREMISE_MEASURED: host behaviour; a violation cannot be injected; EA1 measures it (V2.1 14.7), and the baseline runs without writers (the PARAM-03 clean sample: the CL-CLEAN-<fixture> scenarios and EA2) evaluate it (V2.1 10.6): PASS with no W-scan event on a protected object, UNKNOWN with one (unattributable, V2.1 14), never FAIL (BA-04 V6 section 3, "A-1 in a mirror run"; R1:BA04V4-09, second option) | 0 | COVERED_DRAFT |
| `A2` | A-2 no deferred write events after V0 | B | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (24) | 0 | (none) | (none) | 0 of 0 | (none) | (none) | (none) | PREMISE_MEASURED: host behaviour (EA2 and the clean sample measure it) | 0 | COVERED_DRAFT |
| `WUM` | WARMUP_MEMBERSHIP | B | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (32) | 5 | `NC-WU-FOREIGN` | (none) | 1 of 1 | (none) | (none) | `WU-DRY-Q-I14-Y`, `WU-DRY-Q-I14-X`, `WU-DRY-Q-I14-SILENT-Y`, ... (167) | - | 0 | COVERED_DRAFT |
| `LIBF` | LIBRARY_FRESHNESS | B | YES | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, ... (30) | 3 | `PR-SOURCE-SWAP-NEG-COPY`, `PR-TORN-READ` | (none) | 2 of 2 | (none) | (none) | (none) | - | 0 | COVERED_DRAFT |
| `CLONE` | CLONE_POLICY_VERIFIED | A | YES | `CL-CLEAN-FXIMP-FP`, `PR-SOURCE-SWAP-MODIFY`, `PR-SOURCE-SWAP-DELETE`, ... (13) | 0 | `NC-CLONE-REPLACE` | (none) | 1 of 1 | (none) | (none) | (none) | - | 0 | COVERED_DRAFT |

Rows whose state is neither `COVERED_DRAFT` nor `COVERED_DRAFT_CONDITIONAL`: **0**. Conditional rows (rule 5): `S5` (`NC-S5-O6`), `E12A` (`NC-E12A`), `E12C` (`NC-E12C`, `WR-A-X5`, `WR-A-X6`, `WR-D-X5`, `WR-D-X6`, `WR-E-X5`, `WR-E-X6`, `WR-B-X3`, `WR-C-X5`, `WR-C-X6`, `WR-A-X5-EV`, `WR-A-X6-EV`, `WR-D-X5-EV`, `WR-D-X6-EV`, `WR-E-X5-EV`, `WR-E-X6-EV`, `WR-B-X3-EV`, `WR-C-X5-EV`, `WR-C-X6-EV`). The column "CLEAN FREE OF K-3 AND K-8" counts the governing clean scenarios whose `BLOCKED_BY` holds neither K-3 nor K-8 (they can reach `SEALED` or `SEALED_PENDING_PIN` without those decisions; informational, reviewer item R-7); no scenario counted there carries another blocker. The column "NEGATIVE CONTROLS ON THE CHARACTERIZATION BUILD" counts the negative controls (unconditional and conditional) that run on the `CHARACTERIZATION_BUILD`; the controls whose every counted negative control runs there are `S1`, `S2`, `S3`, `E08`, `S5`, `S5REC`, `S5MAN`, `E03`, `E10`, `E11M`, `E12A`, `E12C`, `E12D`, `WUM`, `LIBF`, `CLONE`: their negative-control coverage rests on the reading AR4-06 accepts (rule 4; R1:BA02V4-N03, R-5), and under a restrictive reading they would be `NEGATIVE_CONTROL_MISSING`.

## 3. Required scenario classes and minimum catalog requirement (V2.1 31 as amended by V2.2 PA-1.4)

**Reading of PA-1.4 (ruled by AR4-05, section 217: the Architect's reading, recorded like AR3-19).** PA-1.4 gives "replaced or added rows" without saying which row replaces which. This matrix reads every PA-1.4 row **together with** the V2.1 row of the same control (the cumulative reading), with the three precisions of AR4-05: (1) where a PA-1.4 cell contradicts a V2.1 31 cell of the same control, PA-1.x governs; the only such cells are the class cells of `LIBRARY_FRESHNESS` and `CLONE_POLICY_VERIFIED` (UNRESOLVED in V2.1, B and A by PA-1.1 and PA-1.2), and the GROUP column of section 31 is never the group authority (the groups come from PA-1.2 and PA-1.3); (2) for SCENARIO CLASSES and MINIMUM CATALOG REQUIREMENT both rows apply, and the PA-1.4 E-12 row (P6) applies in addition to R12 and R13 to E12A, E12DET, E12C and E12D ("subscription" in R13 is phase D); (3) the PA-1.4 rows of the S-5 components (P1, P2) are added rows, and R03 also covers S5REC, S5MAN and S5CLO (section 3.3). No state changes under the reading. For E-12 it keeps "validation plans structurally outside the learning corpus" (V2.1, phase A and determinism) and the writer variants (V2.1, phase C and subscription), and adds "validation only with a `FROZEN` EVM (PA-2)" and the class `NC-E12C` for the four E-12 controls.

### 3.1 Rows in effect (the last five cells are verbatim copies of the contract cells)

| ROW | SOURCE | CONTROL | Class | GROUP | SCENARIO CLASSES | MINIMUM CATALOG REQUIREMENT |
|---|---|---|---|---|---|---|
| R01 | V2.1 31 | S-1, S-2, S-3 | A | SC-A, KS | `CL-CLEAN`, `WR-<writer>-<position>` (X0..X6), `KS-*` | clean runs per family; each persistent-change writer at X1..X4 detected by the semantic comparison; a single-field mutation per compared field |
| R02 | V2.1 31 | E-08 (S-4) | A | E8 | `E8`, `NC-E08` | a clean re-observation and a deliberate read-set change detected at PV |
| R03 | V2.1 31 | S-5 | A | AB, PR-REC, PW | `AB-*`, `PR-*`, `PW-*` | abort after PREPARE-W, after MUTATION, after a `Commit()` exception path (simulated at T1, not host evidence), with exact-fingerprint comparison and the closure |
| R04 | V2.1 31 | E-01, E-02, E-05, E-07 | A | ID-A | `CL-CLEAN`, `NC-E01`, `NC-E02`, `NC-E05`, `NC-E07` | one deliberate violation per control refused at P0 |
| R05 | V2.1 31 | E-03 | A | LK | `LK-<mode>`, `NC-E03` | each candidate mode; a conflicting writer probe |
| R06 | V2.1 31 | E-04 | A | E4 | `E4`, `NC-E04` | each designated invocation route; a non-designated route excluded |
| R07 | V2.1 31 | E-06 | A | E6 | `E6-C1..E6-C8`, `NC-E06` | all counter controls, including `STALE_WRAPPER_CONTROL`; a second transaction detected at a sampling point |
| R08 | V2.1 31 | E-09 | A | E9 | `E9`, `NC-E09` | subjects and classes consumed; an overrule with unknown effect refused |
| R09 | V2.1 31 | E-10 | B | E10 | `E10`, `NC-E10` | a canary module changes the snapshot; quiescent equality |
| R10 | V2.1 31 | E-11M | B | E11M | `E11M`, `NC-E11M` | a canary load delivers an event; loss of subscription detected |
| R11 | V2.1 31 | WARMUP_MEMBERSHIP | B | WU | `WU` (dry runs) | the coverage criterion measured; a warm-up load outside the manifest refuses the run |
| R12 | V2.1 31 | E-12 phase A, determinism | B | E12-V | `EV-V-<plan>`, `NC-E12A` | validation plans structurally outside the learning corpus; an unattributable event aborts before `Commit()` |
| R13 | V2.1 31 | E-12 phase C, subscription | B | E12-V, SC-B | `WR-<writer>-<position>`, `NC-E12C` | event-triggered and silent writer variants; the detection channel recorded |
| R14 | V2.1 31 | E-13 | B | ID-B | `CL-CLEAN`, `NC-E13` | profile equality and a deliberate difference |
| R15 | V2.1 31 | A-1, A-2 | B | EA1, EA2 | `EA1`, `EA2` | read-only opens with zero write events; known-correct commits with zero deferred events |
| R16 | V2.1 31 | LIBRARY_FRESHNESS | UNRESOLVED | PR-FRESH | `PR-*`, `Q-I13` | `SOURCE_SWAP_CONTROL`; a modified source detected; stale cache detected |
| R17 | V2.1 31 | CLONE_POLICY_VERIFIED | UNRESOLVED | PW-CLONE | `PW-*` | `CLONE_NON_REPLACE_CONTROL` |
| R18 | V2.1 31 | E-14, E-15 | A (not in predicate) | KR | `KS-*` | recorded `UNDER_CHARACTERIZATION` |
| R19 | V2.1 31 | INSTRUMENT_QUALIFICATION | NOT_A_CONTROL | Q | `Q-<instrument>` | one qualification scenario per instrument I-01..I-16 |
| R20 | V2.1 31 | EVALUATOR_CONFORMANCE | NOT_A_CONTROL | OU | `OU-O1..O7` | one scenario per row of 17.2 |
| R21 | V2.1 31 | CLEANUP | NOT_A_CONTROL | CL | `CL-*` | cleanup checks pass; an induced incomplete cleanup makes the run INVALID |
| R22 | V2.1 31 | UNDO, SAVE_REOPEN, POST_CP, tail log, E-12 learning | NOT_A_CONTROL | U, S1, PP, SC-B, E12-L | `U-*`, `S1`, `WR-*-X7`, `EV-L-<operation>` | evidence complete per scenario |
| P1 | V2.2 PA-1.4 | S-5 registry and closure components | A | PR-REC | `PR-*`, `Q-I13`, `CLOSURE_COMPLETENESS_CONTROL` | the record covers every closure member; a closure the traversal cannot classify refuses the run |
| P2 | V2.2 PA-1.4 | S-5 manifest component | A | PW | `PW-*` | additions listed with exact fingerprints; a partial failed import is never legitimized |
| P3 | V2.2 PA-1.4 | `LIBRARY_FRESHNESS` | B | PR-FRESH | `PR-*`, `Q-I13`, `SOURCE_SWAP_CONTROL` | a modified source detected; a stale cache detected; torn acquisition detected; `ACQUISITION_AUTHORITY` recorded |
| P4 | V2.2 PA-1.4 | `CLONE_POLICY_VERIFIED` | A | PW-CLONE | `PW-*`, `CLONE_NON_REPLACE_CONTROL` | all variants of PA-12 |
| P5 | V2.2 PA-1.4 | E-14, E-15 | A (outside the predicate) | KR | `KS-*` | recorded `UNDER_CHARACTERIZATION` |
| P6 | V2.2 PA-1.4 | E-12 phase A, determinism, phase C, phase D | B | E12-V | `EV-V-<plan>`, `NC-E12A`, `NC-E12C` | validation only with a `FROZEN` EVM (PA-2) |
| P7 | V2.2 PA-1.4 | `WARMUP_MEMBERSHIP` | B | WU | `WU`, `WARMUP_BOUNDARY_CONTROL` | coverage criterion measured; boundary control passes |

### 3.2 Discharge by clause (rule 6)

| ROW | CLAUSE (verbatim fragment) | SCENARIOS THAT DISCHARGE IT | ENUMERATED SET (rule 6) | STATE |
|---|---|---|---|---|
| R01 | clean runs per family | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, `CL-CLEAN-FX2F-ALL`, `CL-CLEAN-FX4F-ALL`, `CL-CLEAN-FXANN-FP`, `CL-CLEAN-FXANNBLANK-FP`, `CL-CLEAN-FXDIM-FP`, `CL-CLEAN-FXIMP-FP` | - | MET_IN_DRAFT |
| R01 | each persistent-change writer at X1..X4 detected by the semantic comparison | `WR-A-X1`, `WR-A-X2`, `WR-A-X3`, `WR-A-X4`, `WR-D-X1`, `WR-D-X2`, `WR-D-X3`, `WR-D-X4`, `WR-E-X1`, `WR-E-X2`, `WR-E-X3`, `WR-E-X4`, `WR-F-X1`, `WR-F-X2`, `WR-F-X3`, `WR-F-X4`, `WR-G-X1`, `WR-G-X2`, `WR-G-X3`, `WR-G-X4` | the persistent-change writers of V2.1 10.6 (Wr-A, Wr-D, Wr-E, Wr-F, Wr-G) x the positions X1..X4: 20 of 20 members covered | MET_IN_DRAFT |
| R01 | a single-field mutation per compared field | `Q-I11` | - | MET_IN_DRAFT |
| R02 | a clean re-observation | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, `CL-CLEAN-FX2F-ALL`, `CL-CLEAN-FX4F-ALL`, `CL-CLEAN-FXANN-FP`, `CL-CLEAN-FXANNBLANK-FP`, `CL-CLEAN-FXDIM-FP`, `CL-CLEAN-FXIMP-FP` | - | MET_IN_DRAFT |
| R02 | a deliberate read-set change detected at PV | `NC-E08-Y0`, `NC-E08-Y2` | - | MET_IN_DRAFT |
| R03 | abort after PREPARE-W | `AB-AFTER-PREPARE-W` | - | MET_IN_DRAFT |
| R03 | after MUTATION | `AB-AFTER-MUTATION` | - | MET_IN_DRAFT |
| R03 | after a `Commit()` exception path (simulated at T1, not host evidence) | `AB-COMMIT-EXCEPTION-T1-A`, `AB-COMMIT-EXCEPTION-T1-B` | - | MET_IN_DRAFT |
| R03 | with exact-fingerprint comparison and the closure | `AB-AFTER-PREPARE-W`, `PW-IMPORT-PARTIAL-FAIL` | - | MET_IN_DRAFT |
| R04 | one deliberate violation per control refused at P0 | `Q-I02`, `NC-E02`, `NC-E05`, `NC-E07` | the controls of the row (E-01, E-02, E-05, E-07): 4 of 4 members covered | MET_IN_DRAFT |
| R05 | each candidate mode | `LK-LM1-PROPS`, `LK-LM2-PROPS` | the LOCK_MODE candidates of the catalog (LM1, LM2): 2 of 2 members covered | MET_IN_DRAFT |
| R05 | a conflicting writer probe | `LK-LM1-CONFLICT`, `LK-LM2-CONFLICT` | - | MET_IN_DRAFT |
| R06 | each designated invocation route | `E4-LEARN-R1-LM1`, `E4-LEARN-R1-LM2`, `E4-VAL-R1` | the designated routes (R-1; BA-08 V6 section 8): 1 of 1 members covered | MET_IN_DRAFT |
| R06 | a non-designated route excluded | `NC-E04-R2` | - | MET_IN_DRAFT |
| R07 | all counter controls, including `STALE_WRAPPER_CONTROL` | `E6-C1`, `E6-C2`, `E6-C3`, `E6-C4`, `E6-C5`, `E6-C6`, `E6-C7`, `E6-C8`, `E6-C1-CB`, `E6-C2-CB`, `E6-C3-CB`, `E6-C4-CB`, `E6-C5-CB`, `E6-C6-CB`, `E6-C7-CB`, `E6-C8-CB` | the counter controls E6-C1..E6-C8 of V2.1 4.2 x the builds under test used by governing runs (PRODUCT_BUILD, CHARACTERIZATION_BUILD; AR4-06 (3)): 16 of 16 members covered | MET_IN_DRAFT (E6-C4 and E6-C4-CB are EXPLORATORY_NON_GOVERNING, G-3: present, never counted toward PASS) |
| R07 | a second transaction detected at a sampling point | `E6-C6`, `E6-C6-CB`, `NC-E06` | - | MET_IN_DRAFT |
| R08 | subjects and classes consumed | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, `CL-CLEAN-FX2F-ALL`, `CL-CLEAN-FX4F-ALL`, `CL-CLEAN-FXANN-FP`, `CL-CLEAN-FXANNBLANK-FP`, `CL-CLEAN-FXDIM-FP`, `CL-CLEAN-FXIMP-FP` | - | MET_IN_DRAFT |
| R08 | an overrule with unknown effect refused | `NC-E09` | - | MET_IN_DRAFT |
| R09 | a canary module changes the snapshot | `NC-E10` | - | MET_IN_DRAFT |
| R09 | quiescent equality | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, `CL-CLEAN-FX2F-ALL`, `CL-CLEAN-FX4F-ALL`, `CL-CLEAN-FXANN-FP`, `CL-CLEAN-FXANNBLANK-FP`, `CL-CLEAN-FXDIM-FP`, `CL-CLEAN-FXIMP-FP` | - | MET_IN_DRAFT |
| R10 | a canary load delivers an event | `NC-E11M-PV`, `NC-E11M-PC` | - | MET_IN_DRAFT |
| R10 | loss of subscription detected | `Q-I09-LOSS` | - | MET_IN_DRAFT |
| R11 | the coverage criterion measured | `WU-DRY-Q-I14-Y`, `WU-DRY-Q-I14-X`, `WU-DRY-Q-I14-SILENT-Y`, `WU-DRY-Q-I14-SILENT-X`, `WU-DRY-Q-I14-P`, `WU-DRY-CL-CLEAN-FX1F-F0`, `WU-DRY-CL-CLEAN-FX1F-P`, `WU-DRY-CL-CLEAN-FX1F-FP`, `WU-DRY-CL-CLEAN-FX2F-ALL`, `WU-DRY-CL-CLEAN-FX4F-ALL`, `WU-DRY-CL-CLEAN-FXANN-FP`, `WU-DRY-CL-CLEAN-FXANNBLANK-FP`, `WU-DRY-CL-CLEAN-FXDIM-FP`, `WU-DRY-CL-CLEAN-FXIMP-FP`, `WU-DRY-NC-E02`, `WU-DRY-NC-E05`, `WU-DRY-NC-E06`, `WU-DRY-NC-E07`, `WU-DRY-NC-E09`, `WU-DRY-NC-E10`, `WU-DRY-NC-E11M-PV`, `WU-DRY-NC-E11M-PC`, `WU-DRY-NC-E12A`, `WU-DRY-NC-E12D`, `WU-DRY-NC-E12C`, `WU-DRY-NC-E04-R2`, `WU-DRY-NC-WU-FOREIGN`, `WU-DRY-NC-E03-NOT-HELD`, `WU-DRY-NC-E03-REACQUIRE`, `WU-DRY-NC-S1-Y1`, `WU-DRY-NC-S1-Y1-SILENT`, `WU-DRY-NC-E08-Y0`, `WU-DRY-NC-E08-Y0-SILENT`, `WU-DRY-NC-E08-Y2`, `WU-DRY-NC-E08-Y2-SILENT`, `WU-DRY-EA2-DEFERRED-EVENTS`, `WU-DRY-PR-SOURCE-SWAP-MODIFY`, `WU-DRY-PR-SOURCE-SWAP-DELETE`, `WU-DRY-PR-SOURCE-SWAP-NEG-COPY`, `WU-DRY-PR-TORN-READ`, `WU-DRY-PW-CLONE-BASE`, `WU-DRY-PW-CLONE-CASE`, `WU-DRY-PW-CLONE-WRONG-TYPE`, `WU-DRY-PW-CLONE-NESTED`, `WU-DRY-PW-CLONE-SYMTAB`, `WU-DRY-PR-REC-CLOSURE-UNKNOWN`, `WU-DRY-PR-REC-UNREADABLE`, `WU-DRY-PR-REC-REOBS-MISMATCH`, `WU-DRY-PW-IMPORT-PARTIAL-FAIL`, `WU-DRY-AB-AFTER-PREPARE-W`, `WU-DRY-AB-AFTER-MUTATION`, `WU-DRY-NC-S5-O6`, `WU-DRY-NC-CLONE-REPLACE`, `WU-DRY-KS-SL1-POSTWRITE`, `WU-DRY-KS-SL1-TX-MISMATCH`, `WU-DRY-KS-SL4-NEG-DET`, `WU-DRY-E4-VAL-R1`, `WU-DRY-EV-V-FX-2F`, `WU-DRY-EV-V-FX-4F`, `WU-DRY-EV-V-FX-BIG`, `WU-DRY-WR-A-X0`, `WU-DRY-WR-A-X1`, `WU-DRY-WR-A-X2`, `WU-DRY-WR-A-X3`, `WU-DRY-WR-A-X4`, `WU-DRY-WR-A-X5`, `WU-DRY-WR-A-X6`, `WU-DRY-WR-A-X7`, `WU-DRY-WR-A-X8`, `WU-DRY-WR-D-X0`, `WU-DRY-WR-D-X1`, `WU-DRY-WR-D-X2`, `WU-DRY-WR-D-X3`, `WU-DRY-WR-D-X4`, `WU-DRY-WR-D-X5`, `WU-DRY-WR-D-X6`, `WU-DRY-WR-D-X7`, `WU-DRY-WR-D-X8`, `WU-DRY-WR-E-X0`, `WU-DRY-WR-E-X1`, `WU-DRY-WR-E-X2`, `WU-DRY-WR-E-X3`, `WU-DRY-WR-E-X4`, `WU-DRY-WR-E-X5`, `WU-DRY-WR-E-X6`, `WU-DRY-WR-E-X7`, `WU-DRY-WR-E-X8`, `WU-DRY-WR-F-X0`, `WU-DRY-WR-F-X1`, `WU-DRY-WR-F-X2`, `WU-DRY-WR-F-X3`, `WU-DRY-WR-F-X4`, `WU-DRY-WR-F-X5`, `WU-DRY-WR-F-X6`, `WU-DRY-WR-F-X7`, `WU-DRY-WR-F-X8`, `WU-DRY-WR-G-X0`, `WU-DRY-WR-G-X1`, `WU-DRY-WR-G-X2`, `WU-DRY-WR-G-X3`, `WU-DRY-WR-G-X4`, `WU-DRY-WR-G-X5`, `WU-DRY-WR-G-X6`, `WU-DRY-WR-G-X7`, `WU-DRY-WR-G-X8`, `WU-DRY-WR-B-X0`, `WU-DRY-WR-B-X3`, `WU-DRY-WR-B-X7`, `WU-DRY-WR-C-X5`, `WU-DRY-WR-C-X6`, `WU-DRY-WR-C-X7`, `WU-DRY-WR-SA-X1`, `WU-DRY-WR-SA-X2`, `WU-DRY-WR-SA-X3`, `WU-DRY-WR-SA-X4`, `WU-DRY-WR-SB-X3`, `WU-DRY-WR-SC-X5`, `WU-DRY-WR-SC-X6`, `WU-DRY-WR-A-X1-EV`, `WU-DRY-WR-A-X2-EV`, `WU-DRY-WR-A-X3-EV`, `WU-DRY-WR-A-X4-EV`, `WU-DRY-WR-A-X5-EV`, `WU-DRY-WR-A-X6-EV`, `WU-DRY-WR-D-X1-EV`, `WU-DRY-WR-D-X2-EV`, `WU-DRY-WR-D-X3-EV`, `WU-DRY-WR-D-X4-EV`, `WU-DRY-WR-D-X5-EV`, `WU-DRY-WR-D-X6-EV`, `WU-DRY-WR-E-X1-EV`, `WU-DRY-WR-E-X2-EV`, `WU-DRY-WR-E-X3-EV`, `WU-DRY-WR-E-X4-EV`, `WU-DRY-WR-E-X5-EV`, `WU-DRY-WR-E-X6-EV`, `WU-DRY-WR-F-X1-EV`, `WU-DRY-WR-F-X2-EV`, `WU-DRY-WR-F-X3-EV`, `WU-DRY-WR-F-X4-EV`, `WU-DRY-WR-F-X5-EV`, `WU-DRY-WR-F-X6-EV`, `WU-DRY-WR-G-X1-EV`, `WU-DRY-WR-G-X2-EV`, `WU-DRY-WR-G-X3-EV`, `WU-DRY-WR-G-X4-EV`, `WU-DRY-WR-G-X5-EV`, `WU-DRY-WR-G-X6-EV`, `WU-DRY-WR-B-X3-EV`, `WU-DRY-WR-C-X5-EV`, `WU-DRY-WR-C-X6-EV`, `WU-DRY-U-1`, `WU-DRY-U-2`, `WU-DRY-U-3`, `WU-DRY-U-4`, `WU-DRY-U-5`, `WU-DRY-U-7`, `WU-DRY-U-8`, `WU-DRY-U-6b`, `WU-DRY-S1-SAVE-REOPEN`, `WU-DRY-PP-POSTCP`, `WU-DRY-CL-CLEANUP-CHECK`, `WU-DRY-PW-CLONE-BASE-CB`, `WU-DRY-PW-CLONE-CASE-CB`, `WU-DRY-PW-CLONE-WRONG-TYPE-CB`, `WU-DRY-PW-CLONE-NESTED-CB`, `WU-DRY-PW-CLONE-SYMTAB-CB` | the governing scenarios that enter the governed window (the one rule of BA-09 V6 section 4): 167 of 167 members covered | MET_IN_DRAFT |
| R11 | a warm-up load outside the manifest refuses the run | `NC-WU-FOREIGN` | - | MET_IN_DRAFT |
| R12 | validation plans structurally outside the learning corpus | `EV-V-FX-2F`, `EV-V-FX-4F`, `EV-V-FX-BIG` | - | MET_IN_DRAFT |
| R12 | an unattributable event aborts before `Commit()` | `NC-E12A` | - | MET_IN_DRAFT |
| R13 | event-triggered and silent writer variants | `WR-SA-X1`, `WR-SA-X2`, `WR-SA-X3`, `WR-SA-X4`, `WR-SB-X3`, `WR-SC-X5`, `WR-SC-X6`, `WR-A-X1-EV`, `WR-A-X2-EV`, `WR-A-X3-EV`, `WR-A-X4-EV`, `WR-A-X5-EV`, `WR-A-X6-EV`, `WR-D-X1-EV`, `WR-D-X2-EV`, `WR-D-X3-EV`, `WR-D-X4-EV`, `WR-D-X5-EV`, `WR-D-X6-EV`, `WR-E-X1-EV`, `WR-E-X2-EV`, `WR-E-X3-EV`, `WR-E-X4-EV`, `WR-E-X5-EV`, `WR-E-X6-EV`, `WR-F-X1-EV`, `WR-F-X2-EV`, `WR-F-X3-EV`, `WR-F-X4-EV`, `WR-F-X5-EV`, `WR-F-X6-EV`, `WR-G-X1-EV`, `WR-G-X2-EV`, `WR-G-X3-EV`, `WR-G-X4-EV`, `WR-G-X5-EV`, `WR-G-X6-EV`, `WR-B-X3-EV`, `WR-C-X5-EV`, `WR-C-X6-EV` | the two writer variants of V2.1 10.6 (event-triggered, silent): 2 of 2 members covered | MET_IN_DRAFT |
| R13 | the detection channel recorded | `WR-A-X0`, `WR-A-X1`, `WR-A-X2`, `WR-A-X3`, `WR-A-X4`, `WR-A-X5`, `WR-A-X6`, `WR-A-X7`, `WR-A-X8`, `WR-D-X0`, `WR-D-X1`, `WR-D-X2`, `WR-D-X3`, `WR-D-X4`, `WR-D-X5`, `WR-D-X6`, `WR-D-X7`, `WR-D-X8`, `WR-E-X0`, `WR-E-X1`, `WR-E-X2`, `WR-E-X3`, `WR-E-X4`, `WR-E-X5`, `WR-E-X6`, `WR-E-X7`, `WR-E-X8`, `WR-F-X0`, `WR-F-X1`, `WR-F-X2`, `WR-F-X3`, `WR-F-X4`, `WR-F-X5`, `WR-F-X6`, `WR-F-X7`, `WR-F-X8`, `WR-G-X0`, `WR-G-X1`, `WR-G-X2`, `WR-G-X3`, `WR-G-X4`, `WR-G-X5`, `WR-G-X6`, `WR-G-X7`, `WR-G-X8`, `WR-B-X0`, `WR-B-X3`, `WR-B-X7`, `WR-C-X5`, `WR-C-X6`, `WR-C-X7`, `WR-SA-X1`, `WR-SA-X2`, `WR-SA-X3`, `WR-SA-X4`, `WR-SB-X3`, `WR-SC-X5`, `WR-SC-X6`, `WR-A-X1-EV`, `WR-A-X2-EV`, `WR-A-X3-EV`, `WR-A-X4-EV`, `WR-A-X5-EV`, `WR-A-X6-EV`, `WR-D-X1-EV`, `WR-D-X2-EV`, `WR-D-X3-EV`, `WR-D-X4-EV`, `WR-D-X5-EV`, `WR-D-X6-EV`, `WR-E-X1-EV`, `WR-E-X2-EV`, `WR-E-X3-EV`, `WR-E-X4-EV`, `WR-E-X5-EV`, `WR-E-X6-EV`, `WR-F-X1-EV`, `WR-F-X2-EV`, `WR-F-X3-EV`, `WR-F-X4-EV`, `WR-F-X5-EV`, `WR-F-X6-EV`, `WR-G-X1-EV`, `WR-G-X2-EV`, `WR-G-X3-EV`, `WR-G-X4-EV`, `WR-G-X5-EV`, `WR-G-X6-EV`, `WR-B-X3-EV`, `WR-C-X5-EV`, `WR-C-X6-EV`, `NC-E12C` | - | MET_IN_DRAFT |
| R14 | profile equality | `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, `CL-CLEAN-FX2F-ALL`, `CL-CLEAN-FX4F-ALL`, `CL-CLEAN-FXANN-FP`, `CL-CLEAN-FXANNBLANK-FP`, `CL-CLEAN-FXDIM-FP`, `CL-CLEAN-FXIMP-FP` | - | MET_IN_DRAFT |
| R14 | a deliberate difference | `Q-I04` | - | MET_IN_DRAFT |
| R15 | read-only opens with zero write events | `EA1-READONLY-OPENS` | - | MET_IN_DRAFT |
| R15 | known-correct commits with zero deferred events | `EA2-DEFERRED-EVENTS` | - | MET_IN_DRAFT |
| R16 | `SOURCE_SWAP_CONTROL` | `PR-SOURCE-SWAP-MODIFY`, `PR-SOURCE-SWAP-DELETE`, `PR-SOURCE-SWAP-NEG-COPY` | - | MET_IN_DRAFT |
| R16 | a modified source detected | `Q-I13`, `PR-TORN-READ` | - | MET_IN_DRAFT |
| R16 | stale cache detected | `Q-I13` | - | MET_IN_DRAFT |
| R17 | `CLONE_NON_REPLACE_CONTROL` | `PW-CLONE-BASE`, `PW-CLONE-BASE-CB` | - | MET_IN_DRAFT |
| R18 | recorded `UNDER_CHARACTERIZATION` | `KS-SL1-POSTWRITE`, `KS-SL1-TX-MISMATCH`, `KS-SL4-NEG-DET` | - | MET_IN_DRAFT |
| R19 | one qualification scenario per instrument I-01..I-16 | `Q-I01`, `Q-I02`, `Q-I03`, `Q-I04`, `Q-I05`, `Q-I06`, `Q-I07`, `Q-I08`, `Q-I09`, `Q-I10`, `Q-I11`, `Q-I12`, `Q-I13`, `Q-I14`, `Q-I15`, `Q-I16`, `Q-I01-CB`, `Q-I02-CB`, `Q-I03-CB`, `Q-I04-CB`, `Q-I05-CB`, `Q-I06-CB`, `Q-I07-CB`, `Q-I08-CB`, `Q-I09-CB`, `Q-I10-CB`, `Q-I11-CB`, `Q-I12-CB`, `Q-I13-CB`, `Q-I15-CB`, `Q-I16-CB` | the instruments I-01..I-16 of V2.1 4.3 x the builds under test used by governing runs (I-14 only on the CHARACTERIZATION_BUILD; AR4-06 (3)): 31 of 31 members covered | MET_IN_DRAFT |
| R20 | one scenario per row of 17.2 | `OU-ROW1-INVALID`, `OU-O1`, `OU-O2`, `OU-O6`, `OU-O4`, `OU-O5`, `OU-O7`, `OU-O3` | the rows 1..8 of V2.1 17.2: 8 of 8 members covered | MET_IN_DRAFT |
| R21 | cleanup checks pass | `CL-CLEANUP-CHECK` | - | MET_IN_DRAFT |
| R21 | an induced incomplete cleanup makes the run INVALID | `Q-I16-CLEANUP-NEG` | - | MET_IN_DRAFT |
| R22 | evidence complete per scenario | `U-1`, `U-2`, `U-3`, `U-4`, `U-5`, `U-6`, `U-6b`, `U-7`, `U-8`, `S1-SAVE-REOPEN`, `PP-POSTCP`, `WR-A-X7`, `WR-D-X7`, `WR-E-X7`, `WR-F-X7`, `WR-G-X7`, `WR-B-X7`, `WR-C-X7`, `EV-L-OC-TM`, `EV-L-OC-BT-OPEN`, `EV-L-OC-BTR-NESTED`, `EV-L-OC-BTR-VIEW`, `EV-L-OC-REF-PIECE`, `EV-L-OC-DYN`, `EV-L-OC-REF-ARRAY`, `EV-L-OC-TM-READ`, `EV-L-OC-LAYER`, `EV-L-OC-LAYER-PRESENT`, `EV-L-OC-TEXT`, `EV-L-OC-DIM`, `EV-L-OC-ENVELOPE`, `EV-L-OC-IMPORT`, `EV-L-OC-REF-TOP`, `EV-L-OC-VERIFY-READ` | - | MET_IN_DRAFT |
| P1 | the record covers every closure member | `PR-CLOSURE-COMPLETENESS`, `PR-CLOSURE-COMPLETENESS-CB` | - | MET_IN_DRAFT |
| P1 | a closure the traversal cannot classify refuses the run | `PR-REC-CLOSURE-UNKNOWN` | - | MET_IN_DRAFT |
| P2 | additions listed with exact fingerprints | `PW-CLONE-BASE`, `PW-CLONE-NESTED`, `PW-CLONE-SYMTAB`, `PW-CLONE-BASE-CB`, `PW-CLONE-NESTED-CB`, `PW-CLONE-SYMTAB-CB` | - | MET_IN_DRAFT |
| P2 | a partial failed import is never legitimized | `PW-IMPORT-PARTIAL-FAIL` | - | MET_IN_DRAFT |
| P3 | a modified source detected | `Q-I13`, `PR-TORN-READ` | - | MET_IN_DRAFT |
| P3 | a stale cache detected | `Q-I13` | - | MET_IN_DRAFT |
| P3 | torn acquisition detected | `PR-TORN-READ` | - | MET_IN_DRAFT |
| P3 | `ACQUISITION_AUTHORITY` recorded | `PR-TORN-READ` | - | MET_IN_DRAFT |
| P4 | all variants of PA-12 | `PW-CLONE-BASE`, `PW-CLONE-CASE`, `PW-CLONE-WRONG-TYPE`, `PW-CLONE-NESTED`, `PW-CLONE-SYMTAB`, `PW-CLONE-BASE-CB`, `PW-CLONE-CASE-CB`, `PW-CLONE-WRONG-TYPE-CB`, `PW-CLONE-NESTED-CB`, `PW-CLONE-SYMTAB-CB` | the V2.1 procedure (BASE) and the four variants of V2.2 PA-12 (CASE, WRONG-TYPE, NESTED, SYMTAB) x the builds under test used by governing runs (PRODUCT_BUILD, CHARACTERIZATION_BUILD; AR4-06 (3) by content, AR5-05): 10 of 10 members covered | MET_IN_DRAFT |
| P5 | recorded `UNDER_CHARACTERIZATION` | `KS-SL1-POSTWRITE`, `KS-SL1-TX-MISMATCH`, `KS-SL4-NEG-DET` | - | MET_IN_DRAFT |
| P6 | validation only with a `FROZEN` EVM (PA-2) | `EV-V-FX-2F`, `EV-V-FX-4F`, `EV-V-FX-BIG`, `NC-E12A`, `NC-E12C` | - | MET_IN_DRAFT |
| P7 | coverage criterion measured | `WU-DRY-Q-I14-Y`, `WU-DRY-Q-I14-X`, `WU-DRY-Q-I14-SILENT-Y`, `WU-DRY-Q-I14-SILENT-X`, `WU-DRY-Q-I14-P`, `WU-DRY-CL-CLEAN-FX1F-F0`, `WU-DRY-CL-CLEAN-FX1F-P`, `WU-DRY-CL-CLEAN-FX1F-FP`, `WU-DRY-CL-CLEAN-FX2F-ALL`, `WU-DRY-CL-CLEAN-FX4F-ALL`, `WU-DRY-CL-CLEAN-FXANN-FP`, `WU-DRY-CL-CLEAN-FXANNBLANK-FP`, `WU-DRY-CL-CLEAN-FXDIM-FP`, `WU-DRY-CL-CLEAN-FXIMP-FP`, `WU-DRY-NC-E02`, `WU-DRY-NC-E05`, `WU-DRY-NC-E06`, `WU-DRY-NC-E07`, `WU-DRY-NC-E09`, `WU-DRY-NC-E10`, `WU-DRY-NC-E11M-PV`, `WU-DRY-NC-E11M-PC`, `WU-DRY-NC-E12A`, `WU-DRY-NC-E12D`, `WU-DRY-NC-E12C`, `WU-DRY-NC-E04-R2`, `WU-DRY-NC-WU-FOREIGN`, `WU-DRY-NC-E03-NOT-HELD`, `WU-DRY-NC-E03-REACQUIRE`, `WU-DRY-NC-S1-Y1`, `WU-DRY-NC-S1-Y1-SILENT`, `WU-DRY-NC-E08-Y0`, `WU-DRY-NC-E08-Y0-SILENT`, `WU-DRY-NC-E08-Y2`, `WU-DRY-NC-E08-Y2-SILENT`, `WU-DRY-EA2-DEFERRED-EVENTS`, `WU-DRY-PR-SOURCE-SWAP-MODIFY`, `WU-DRY-PR-SOURCE-SWAP-DELETE`, `WU-DRY-PR-SOURCE-SWAP-NEG-COPY`, `WU-DRY-PR-TORN-READ`, `WU-DRY-PW-CLONE-BASE`, `WU-DRY-PW-CLONE-CASE`, `WU-DRY-PW-CLONE-WRONG-TYPE`, `WU-DRY-PW-CLONE-NESTED`, `WU-DRY-PW-CLONE-SYMTAB`, `WU-DRY-PR-REC-CLOSURE-UNKNOWN`, `WU-DRY-PR-REC-UNREADABLE`, `WU-DRY-PR-REC-REOBS-MISMATCH`, `WU-DRY-PW-IMPORT-PARTIAL-FAIL`, `WU-DRY-AB-AFTER-PREPARE-W`, `WU-DRY-AB-AFTER-MUTATION`, `WU-DRY-NC-S5-O6`, `WU-DRY-NC-CLONE-REPLACE`, `WU-DRY-KS-SL1-POSTWRITE`, `WU-DRY-KS-SL1-TX-MISMATCH`, `WU-DRY-KS-SL4-NEG-DET`, `WU-DRY-E4-VAL-R1`, `WU-DRY-EV-V-FX-2F`, `WU-DRY-EV-V-FX-4F`, `WU-DRY-EV-V-FX-BIG`, `WU-DRY-WR-A-X0`, `WU-DRY-WR-A-X1`, `WU-DRY-WR-A-X2`, `WU-DRY-WR-A-X3`, `WU-DRY-WR-A-X4`, `WU-DRY-WR-A-X5`, `WU-DRY-WR-A-X6`, `WU-DRY-WR-A-X7`, `WU-DRY-WR-A-X8`, `WU-DRY-WR-D-X0`, `WU-DRY-WR-D-X1`, `WU-DRY-WR-D-X2`, `WU-DRY-WR-D-X3`, `WU-DRY-WR-D-X4`, `WU-DRY-WR-D-X5`, `WU-DRY-WR-D-X6`, `WU-DRY-WR-D-X7`, `WU-DRY-WR-D-X8`, `WU-DRY-WR-E-X0`, `WU-DRY-WR-E-X1`, `WU-DRY-WR-E-X2`, `WU-DRY-WR-E-X3`, `WU-DRY-WR-E-X4`, `WU-DRY-WR-E-X5`, `WU-DRY-WR-E-X6`, `WU-DRY-WR-E-X7`, `WU-DRY-WR-E-X8`, `WU-DRY-WR-F-X0`, `WU-DRY-WR-F-X1`, `WU-DRY-WR-F-X2`, `WU-DRY-WR-F-X3`, `WU-DRY-WR-F-X4`, `WU-DRY-WR-F-X5`, `WU-DRY-WR-F-X6`, `WU-DRY-WR-F-X7`, `WU-DRY-WR-F-X8`, `WU-DRY-WR-G-X0`, `WU-DRY-WR-G-X1`, `WU-DRY-WR-G-X2`, `WU-DRY-WR-G-X3`, `WU-DRY-WR-G-X4`, `WU-DRY-WR-G-X5`, `WU-DRY-WR-G-X6`, `WU-DRY-WR-G-X7`, `WU-DRY-WR-G-X8`, `WU-DRY-WR-B-X0`, `WU-DRY-WR-B-X3`, `WU-DRY-WR-B-X7`, `WU-DRY-WR-C-X5`, `WU-DRY-WR-C-X6`, `WU-DRY-WR-C-X7`, `WU-DRY-WR-SA-X1`, `WU-DRY-WR-SA-X2`, `WU-DRY-WR-SA-X3`, `WU-DRY-WR-SA-X4`, `WU-DRY-WR-SB-X3`, `WU-DRY-WR-SC-X5`, `WU-DRY-WR-SC-X6`, `WU-DRY-WR-A-X1-EV`, `WU-DRY-WR-A-X2-EV`, `WU-DRY-WR-A-X3-EV`, `WU-DRY-WR-A-X4-EV`, `WU-DRY-WR-A-X5-EV`, `WU-DRY-WR-A-X6-EV`, `WU-DRY-WR-D-X1-EV`, `WU-DRY-WR-D-X2-EV`, `WU-DRY-WR-D-X3-EV`, `WU-DRY-WR-D-X4-EV`, `WU-DRY-WR-D-X5-EV`, `WU-DRY-WR-D-X6-EV`, `WU-DRY-WR-E-X1-EV`, `WU-DRY-WR-E-X2-EV`, `WU-DRY-WR-E-X3-EV`, `WU-DRY-WR-E-X4-EV`, `WU-DRY-WR-E-X5-EV`, `WU-DRY-WR-E-X6-EV`, `WU-DRY-WR-F-X1-EV`, `WU-DRY-WR-F-X2-EV`, `WU-DRY-WR-F-X3-EV`, `WU-DRY-WR-F-X4-EV`, `WU-DRY-WR-F-X5-EV`, `WU-DRY-WR-F-X6-EV`, `WU-DRY-WR-G-X1-EV`, `WU-DRY-WR-G-X2-EV`, `WU-DRY-WR-G-X3-EV`, `WU-DRY-WR-G-X4-EV`, `WU-DRY-WR-G-X5-EV`, `WU-DRY-WR-G-X6-EV`, `WU-DRY-WR-B-X3-EV`, `WU-DRY-WR-C-X5-EV`, `WU-DRY-WR-C-X6-EV`, `WU-DRY-U-1`, `WU-DRY-U-2`, `WU-DRY-U-3`, `WU-DRY-U-4`, `WU-DRY-U-5`, `WU-DRY-U-7`, `WU-DRY-U-8`, `WU-DRY-U-6b`, `WU-DRY-S1-SAVE-REOPEN`, `WU-DRY-PP-POSTCP`, `WU-DRY-CL-CLEANUP-CHECK`, `WU-DRY-PW-CLONE-BASE-CB`, `WU-DRY-PW-CLONE-CASE-CB`, `WU-DRY-PW-CLONE-WRONG-TYPE-CB`, `WU-DRY-PW-CLONE-NESTED-CB`, `WU-DRY-PW-CLONE-SYMTAB-CB` | the governing scenarios that enter the governed window (the one rule of BA-09 V6 section 4): 167 of 167 members covered | MET_IN_DRAFT |
| P7 | boundary control passes | `WU-BOUNDARY`, `WU-BOUNDARY-CB` | - | MET_IN_DRAFT |

Declared exceptions to the class membership (rule 6):

| ROW | SCENARIO | REASON |
|---|---|---|
| R01 | `Q-I11` | V2.1 4.3 row I-11: "a single-field mutation, per field, is detected" is the negative control of the qualification of I-11 |
| R04 | `Q-I02` | phase-1 ruling G-2: E-01 cannot be violated in a governing run; Q-I02 qualifies the detection |
| R10 | `Q-I09-LOSS` | V2.1 21.1: a product run with a lost subscription is INVALID; the loss detection is qualified by Q-I09-LOSS |
| R14 | `Q-I04` | phase-1 ruling G-2: E-13 cannot be violated in a governing run; Q-I04 qualifies the detection |
| R20 | `OU-ROW1-INVALID` | the clause requires one scenario per row of 17.2, and row 1 (an INVALID run after a product mutation) produces no O-outcome, so its scenario lies outside the literal class `OU-O1..O7` (evaluator scenarios producing each outcome) |
| R21 | `Q-I16-CLEANUP-NEG` | AR2-22: the incomplete-cleanup negative is the qualification of I-16 |
| R22 | `PP-POSTCP` | the row's CONTROL cell names POST_CP; V2.1 26.3 has no class prefix for it |

### 3.3 State per control

| KEY | ROWS IN EFFECT | STATE |
|---|---|---|
| `S1` | R01 | MET_IN_DRAFT |
| `S2` | R01 | MET_IN_DRAFT |
| `S3` | R01 | MET_IN_DRAFT |
| `E08` | R02 | MET_IN_DRAFT |
| `S5` | R03 | MET_IN_DRAFT |
| `S5REC` | R03, P1 | MET_IN_DRAFT |
| `S5MAN` | R03, P2 | MET_IN_DRAFT |
| `S5CLO` | R03, P1 | MET_IN_DRAFT |
| `E01` | R04 | MET_IN_DRAFT |
| `E02` | R04 | MET_IN_DRAFT |
| `E03` | R05 | MET_IN_DRAFT |
| `E04` | R06 | MET_IN_DRAFT |
| `E05` | R04 | MET_IN_DRAFT |
| `E06` | R07 | MET_IN_DRAFT |
| `E07` | R04 | MET_IN_DRAFT |
| `E09` | R08 | MET_IN_DRAFT |
| `E10` | R09 | MET_IN_DRAFT |
| `E11M` | R10 | MET_IN_DRAFT |
| `E12A` | R12, P6 | MET_IN_DRAFT |
| `E12C` | R13, P6 | MET_IN_DRAFT |
| `E12D` | R13, P6 | MET_IN_DRAFT |
| `E12DET` | R12, P6 | MET_IN_DRAFT |
| `E13` | R14 | MET_IN_DRAFT |
| `E14` | R18, P5 | MET_IN_DRAFT |
| `E15` | R18, P5 | MET_IN_DRAFT |
| `A1` | R15 | MET_IN_DRAFT |
| `A2` | R15 | MET_IN_DRAFT |
| `WUM` | R11, P7 | MET_IN_DRAFT |
| `LIBF` | R16, P3 | MET_IN_DRAFT |
| `CLONE` | R17, P4 | MET_IN_DRAFT |
| `INSTRUMENT_QUALIFICATION` | R19 | MET_IN_DRAFT |
| `EVALUATOR_CONFORMANCE` | R20 | MET_IN_DRAFT |
| `CLEANUP` | R21 | MET_IN_DRAFT |
| `UNDO, SAVE_REOPEN, POST_CP, tail log, E-12 learning` | R22 | MET_IN_DRAFT |

## 4. Group coverage

| GROUP | KIND | CLASS | PRIMARY (GROUP) | SERVING (GROUPS_SERVED) | CONTRACT-ASSIGNED (evidence only) | COUNTABLE FOR CONTROL COVERAGE (rule 4) | GOVERNING SCENARIOS SERVING THE GROUP (rule 4) | SEALED |
|---|---|---|---|---|---|---|---|---|
| `Q` | EVIDENCE | N/A | 50 | 50 | 0 | N/A (PA-3) | 50 | 0 |
| `WU` | CONTROL | B | 170 | 230 | 0 | 62 | 63 | 0 |
| `ID-A` | CONTROL | A | 3 | 58 | 0 | 58 | 58 | 0 |
| `ID-B` | CONTROL | B | 0 | 58 | 0 | 58 | 58 | 0 |
| `E4` | CONTROL | A | 4 | 60 | 0 | 58 | 58 | 0 |
| `LK` | CONTROL | A | 6 | 55 | 0 | 51 | 51 | 0 |
| `E6` | CONTROL | A | 17 | 74 | 0 | 72 | 72 | 0 |
| `E8` | CONTROL | A | 14 | 65 | 0 | 65 | 65 | 0 |
| `E9` | CONTROL | A | 1 | 58 | 0 | 58 | 58 | 0 |
| `E10` | CONTROL | B | 1 | 59 | 0 | 58 | 59 | 0 |
| `E11M` | CONTROL | B | 2 | 226 | 0 | 58 | 59 | 0 |
| `E12-V` | CONTROL | B | 6 | 133 | 0 | 132 | 133 | 0 |
| `E12-L` | EVIDENCE | N/A | 17 | 16 | 0 | N/A (PA-3) | 16 | 0 |
| `EA1` | CONTROL | B | 1 | 11 | 0 | 11 | 11 | 0 |
| `EA2` | CONTROL | B | 1 | 27 | 0 | 27 | 27 | 0 |
| `SC-A` | CONTROL | A | 49 | 130 | 3 | 128 | 130 | 0 |
| `SC-B` | CONTROL | B | 32 | 101 | 13 | 100 | 101 | 0 |
| `PP` | EVIDENCE | N/A | 1 | 1 | 0 | N/A (PA-3) | 1 | 0 |
| `PR-REC` | CONTROL | A | 3 | 50 | 2 | 50 | 50 | 0 |
| `PR-FRESH` | CONTROL | B | 4 | 50 | 0 | 50 | 50 | 0 |
| `PW` | CONTROL | A | 1 | 45 | 0 | 45 | 45 | 0 |
| `PW-CLONE` | CONTROL | A | 11 | 16 | 0 | 16 | 16 | 0 |
| `AB` | CONTROL | A | 6 | 9 | 0 | 9 | 9 | 0 |
| `KS` | CONTROL | A | 11 | 129 | 3 | 128 | 129 | 0 |
| `KR` | CONTROL_OUTSIDE_PREDICATE | A | 0 | 12 | 0 | N/A (PA-4) | 12 | 0 |
| `U-1..U-8` | EVIDENCE | N/A | 8 | 8 | 0 | N/A (PA-3) | 8 | 0 |
| `U-6b` | EVIDENCE | N/A | 1 | 1 | 0 | N/A (PA-3) | 1 | 0 |
| `S1` | EVIDENCE | N/A | 1 | 1 | 0 | N/A (PA-3) | 1 | 0 |
| `OU` | EVIDENCE | N/A | 11 | 11 | 0 | N/A (PA-3) | 11 | 0 |
| `CL` | EVIDENCE | N/A | 1 | 1 | 0 | N/A (PA-3) | 1 | 0 |

Groups of V2.2 PA-1.3 with no serving or contract-assigned scenario: **none**. `HDM` (BA-04 V6) is not a group of PA-1.3: its four non-governing characterization scenarios serve no group, and neither does the non-governing composition check `EV-L-COMPOSED`.

## 5. Design-level checks (V2.1 31, repeated on the catalog)

| Check | Result |
|---|---|
| 1. every class-A or class-B control has at least one group; every group has at least one serving scenario | PASS |
| 2. every mandatory control has a governing clean scenario and a negative control, or a recorded reason | PASS (in draft; the controls whose negative controls run on the CHARACTERIZATION_BUILD count under AR4-06, rule 4; S5, E12A, E12C conditional: at acceptance each is NEGATIVE_CONTROL_NOT_EVALUATED unless a conditional negative control resolves to its detection branch (AR3-03), and then this check is NOT MET for that control (AR4-20 R-3 (b)) |
| 3. every scenario's `CONTROL_CLASSES_EXERCISED` is a subset of the registry classes | PASS |
| 4. no control without a group and no group without a scenario | PASS |

The matrix is complete for V2.1 1.6 only when checks 1 to 4 are all met at acceptance (AR4-20 R-3 (c)); a control in `NEGATIVE_CONTROL_NOT_EVALUATED` makes check 2 NOT MET and keeps `ALT21D_HOST_PASS` at `NOT_EVALUATED`, never `TRUE`.

## 6. Status

```text
BA-02b = DRAFT V6     0 sealed scenarios     BASE-20 = NOT MET (depends on NB-2)
CT21D_AUTHORITY_BASELINE_READY = FALSE
```

**Architect questions that bear on this matrix.** The V5 questions that could change these tables are ruled (decisions section 220): **AQ-V5-02** by AR5-05 (the twins `PR-CLOSURE-COMPLETENESS-CB`, `WU-BOUNDARY-CB`, `PW-CLONE-BASE-CB`, `PW-CLONE-CASE-CB`, `PW-CLONE-WRONG-TYPE-CB`, `PW-CLONE-NESTED-CB`, `PW-CLONE-SYMTAB-CB` are in sections 2 to 4), **AQ-V5-08** by AR5-09 (`Q-I14-P` and `WU-DRY-Q-I14-P` admitted, as the tables already showed them) and **AQ-V5-01** by AR5-04 (a run prerequisite only, which this matrix does not show). The open question of round V6 recorded in BA-04 V6 section 8.3, **AQ-V6-BA04-1** (the scope of the `BOUND_SHA_PRECONDITION_RECORD`), changes `RUN_PREREQUISITES` only: no state or count of this matrix depends on it. No AQ-V4 or AQ-V5 reading remains open here.

## 7. Delta V6 (decisions 220 and 223)

| Ruling or finding | Decision | Fix (section / field) |
|---|---|---|
| AR6-07 (the BA-02b row of 223.4: cascade from BA-04 V6) | applied | every table is re-derived from BA-04 V6 (437 scenarios); the version citations name BA-04 V6, BA-08 V6 and BA-09 V6 (BA-02a V3 unchanged); the published check `docs/automation/evidence/I-52-ct21d-ba04-v6-checks.py` is extended (never weakened) to the V6 content and re-run, and its output `docs/automation/evidence/I-52-ct21d-ba04-v6-checks-output.json` is the new path of the BA-02b entry of the BA-11 registry; section 0 names the check's SHA-256 |
| AR5-05 (per-build twins by content, through BA-04 V6) | applied | the twins `PR-CLOSURE-COMPLETENESS-CB`, `WU-BOUNDARY-CB`, `PW-CLONE-BASE-CB`, `PW-CLONE-CASE-CB`, `PW-CLONE-WRONG-TYPE-CB`, `PW-CLONE-NESTED-CB`, `PW-CLONE-SYMTAB-CB` enter the tables: section 2 (the WUM, CLONE and S5CLO rows through their exercised controls), section 3.2 (clauses P1, P2, P4, P7 and R17 list them; the enumerated set of P4 is now the five variants x the two builds, as R07 and R19 are per build) and section 4 (the Q, WU, PW-CLONE and PR-REC group rows); the five `PW-CLONE-*-CB` dry runs enter the R11 and P7 criterion sets |
| AR5-09 (Q-I14-P admitted, through BA-04 V6) | applied | `Q-I14-P` and `WU-DRY-Q-I14-P` stay in the tables as before; `AQ-V5-08` is no longer a `BLOCKED_BY` value (no state or count of this matrix depended on it); the header and section 6 no longer name it as open |
| AR5-04 and AR5-12 (run prerequisites, through BA-04 V6) | recorded | the `BUILD_EQUIVALENCE_RECORD` of the reverse direction and the `BOUND_SHA_PRECONDITION_RECORD` change `RUN_PREREQUISITES` only, which this matrix does not show; no state or count changes |
| AR5-11, AR6-02 and FXB-01 (BA-02b part) | applied | the rows that list the `FX-ANN-BLANK` scenarios (R01, R02, R08, R09, R14 through `CL-CLEAN-FXANNBLANK-FP`; R11 and P7 through its dry run) are unchanged: the ids survive under AR5-11 (a legacy-state fixture; the run build of each scenario is unchanged), as the Gate 2 assessment states for this artifact |
| G2-CR-05, AR6-05 and AR6-06 (the contract line) | applied | header `AUTHORITY_CONTRACT`: V2.1..V2.5 textually verified where the Coordinator verified them, plus V2.6 (draft, pending review) |

Section numbering: sections 0 to 7 keep their V5 numbers; the enumerated set of clause P4 (section 3.2) is now per build; section 7 replaces the Delta V5 table, which stays in the V5 file as history.
