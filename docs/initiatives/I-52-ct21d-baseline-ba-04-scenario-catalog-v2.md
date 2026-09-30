# I-52 — CT-21D Baseline Artifact BA-04: Scenario Catalog (DRAFT V2)

> **BASELINE ARTIFACT BA-04 V2 — DRAFT, NOT SEALED. Design / documentation only. Nothing here is executed or authorized.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-SCENARIO-CATALOG
> ARTIFACT_VERSION          = 2-DRAFT   (supersedes 1-DRAFT, blobs f436f889... (json) and 9c3fe219... (md), which stay as history)
> ARTIFACT_STATUS           = DRAFT (phase 2)       CANDIDATE_HASH = UNSET     SEAL_HASH = UNSET
> AUTHORITY_CONTRACT        = CT-21D V2.1 + V2.2 + V2.3 + V2.4
> PHASE-1 RULING APPLIED    = G-1 (Y coordinates), G-2 (NC-E01/NC-E13 removed; INVALID vs REFUSE), G-3 (E6-C4 exploratory), G-4 (fixture dependency),
>                             OU tuple, exact outcomes, SEALED_PENDING_PIN, conditional-expectation rule, pending classes authored
> BLOCKER                   = NB-2 (REMAINS_BLOCKER_FOR_AUTHORITY_BASELINE)
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```
>
> The machine-readable catalog is [the JSON file](I-52-ct21d-baseline-ba-04-scenario-catalog-v2.json) and carries every field of every scenario; the two files form the artifact.

## 1. Schema (normative)

Every scenario carries: `SCENARIO_ID`, `SCENARIO_VERSION`, `GROUP`, `GROUPS_SERVED`, `MODE` (`CHARACTERIZATION_RUN`, `LEARNING`, `VALIDATION`, `DRY_RUN`), `TUPLE_REQUIREMENTS`, `FIXTURE`, `PLAN_OR_INPUT`, `INPUT_STATUS`, `INJECTION_COORDINATE`, `DELIBERATE_VIOLATION_FLAG`, `TARGET_NEGATIVE_CONTROL`, `EXPECTED_PRODUCT_OUTCOME`, `EXPECTED_CONTROL_RESULTS`, `EXPECTED_INSTRUMENT_STATES`, `EXPECTED_GROUP_VERDICT`, `EXPECTED_LOG_RECORD_SET`, `EXPECTED_EVIDENCE_SET`, `EXPECTED_CLEANUP`, `RETRY_APPLICABILITY`, `REPETITION`, `CONTROL_CLASSES_EXERCISED`, `EVM_VERSION`, `AUTHORITY_SOURCE_FOR_EXPECTED_RESULT`, `EXPECTED_PROCEDURAL_RESULT`, `CONDITIONAL_EXPECTATION_RULE`, `PIN`, `STATUS`, `SEALED_AT`.
V2 adds `GROUPS_SERVED` (a clean run serves several groups), `FIXTURE`, `INJECTION_COORDINATE` (X or Y coordinate, or a named point), `REPETITION`, `CONDITIONAL_EXPECTATION_RULE` and `PIN`.

## 2. Statuses

| Status | Meaning |
|---|---|
| `DRAFT` | the expected result has an authority; the scenario is not sealed |
| `SEALED_PENDING_PIN_CANDIDATE` | the expected result has an authority **except** for an evidence-derived value (`PIN`: E-04 admitted set, `LOCK_MODE` selection, frozen EVM version); when sealed it becomes `SEALED_PENDING_PIN` |
| `SEALED_PENDING_PIN` | sealed; the pinned value is written **after** it is frozen from its own learning or characterization evidence and **before** the first governing run of the scenario; never from this scenario's own outcome |
| `SEALED` | sealed with no pending pin |
| `EXPLORATORY_NON_GOVERNING` | the expected result lacks an authority; never promoted after the fact; promotion needs a new sealed scenario version with a reviewed authority |

## 3. Conditional-expectation rule (normative)

A conditional expectation (`CONDITIONAL_EXPECTATION_RULE`) is allowed **only** when it is **resolved offline, by a rule declared before the run**, from the **sealed log or evidence of the same run** (or from a sealed qualification record it names, such as `Q-I14-SILENT`). It is **never** resolved from hindsight, from a later run, or from an outcome the rule does not name. The offline evaluator (I-16) applies it; a rule that the sealed evidence cannot resolve makes the scenario result `UNKNOWN`.

## 4. Defaults

| Field | Default |
|---|---|
| `EXPECTED_LOG_RECORD_SET` | RUN_LOG_STANDARD (V2.1 18.1: no sequence gap; the record types of the phases the scenario reaches) |
| `EXPECTED_EVIDENCE_SET` | EVIDENCE_PACKAGE_STANDARD (V2.1 18.2) |
| `EXPECTED_CLEANUP` | CLEANUP_STANDARD (V2.1 20) |
| `RETRY_APPLICABILITY` | NONE_AUTHORIZED (V2.1 21.3; INVALID only with a Coordinator authorization per occurrence; a governing FAIL or UNKNOWN is never retried) |
| `EXPECTED_INSTRUMENT_STATES` | every instrument RELIABLE (qualified on the exact build) |
| `REPETITION` | PARAM-01 (BA-10 V2; UNSET) |
| `TUPLE_REQUIREMENTS` | `FULL_BASELINE_TUPLE (V2.1 2.1: every BUILD_BOUND and MACHINE_PROFILE_BOUND field equal to the baseline; SESSION_BOUND fields present and consistent)` |
| synthetic scenarios (`OU-*`, `U-6`, `AB-COMMIT-EXCEPTION-T1`, `Q-FPSPEC-VECTORS`) | `CONTROL_PLANE_TUPLE (no host): control-plane source SHA, evaluator assembly SHA-256, evaluator version, synthetic log fixture hash; no host, machine-profile or session field applies` |

## 5. Totals and index

Totals: **217** scenarios; statuses `DRAFT` = 107, `EXPLORATORY_NON_GOVERNING` = 1, `SEALED_PENDING_PIN_CANDIDATE` = 109; input status `FIXTURE_SPEC_DEFINED` = 176, `NO_PLAN_INPUT_REQUIRED` = 30, `SYNTHETIC_LOG_FIXTURE` = 11. Every scenario ID is concrete; every expected outcome is an exact `O1..O7`, `NONE`, an explicit conditional (section 3) or a non-governing result; no expected outcome says "per fixture plan".

| Group | Count |
|---|---|
| `AB` | 2 |
| `CL` | 2 |
| `E10` | 1 |
| `E11M` | 2 |
| `E12-L` | 12 |
| `E12-V` | 5 |
| `E4` | 3 |
| `E6` | 9 |
| `E8` | 4 |
| `E9` | 1 |
| `EA1` | 1 |
| `EA2` | 1 |
| `ID-A` | 3 |
| `KR` | 1 |
| `KS` | 13 |
| `LK` | 4 |
| `OU` | 10 |
| `PP` | 1 |
| `PR-FRESH` | 4 |
| `PR-REC` | 4 |
| `PW` | 1 |
| `PW-CLONE` | 5 |
| `Q` | 21 |
| `S1` | 1 |
| `SC-A` | 49 |
| `SC-B` | 40 |
| `U-1..U-8` | 8 |
| `U-6b` | 1 |
| `WU` | 8 |

### 5.1 Scenario summary

| ID | GROUP | COORD | EXPECTED_PRODUCT_OUTCOME | STATUS |
|---|---|---|---|---|
| `Q-I01` | `Q` | NONE | NONE (no product run) | DRAFT |
| `Q-I02` | `Q` | NONE | NONE (no product run) | DRAFT |
| `Q-I03` | `Q` | NONE | NONE (no product run) | DRAFT |
| `Q-I04` | `Q` | NONE | NONE (no product run) | DRAFT |
| `Q-I05` | `Q` | NONE | NONE (no product run) | DRAFT |
| `Q-I06` | `Q` | NONE | NONE (no product run) | DRAFT |
| `Q-I07` | `Q` | NONE | NONE (no product run) | DRAFT |
| `Q-I08` | `Q` | NONE | NONE (no product run) | DRAFT |
| `Q-I09` | `Q` | NONE | NONE (no product run) | DRAFT |
| `Q-I10` | `Q` | NONE | NONE (no product run) | DRAFT |
| `Q-I11` | `Q` | NONE | NONE (no product run) | DRAFT |
| `Q-I12` | `Q` | NONE | NONE (no product run) | DRAFT |
| `Q-I13` | `Q` | NONE | NONE (no product run) | DRAFT |
| `Q-I14` | `Q` | NONE | NONE (no product run) | DRAFT |
| `Q-I15` | `Q` | NONE | NONE (no product run) | DRAFT |
| `Q-I16` | `Q` | NONE | NONE (no product run) | DRAFT |
| `Q-I14-Y` | `Q` | NONE | NONE (no product run) | DRAFT |
| `Q-I14-SILENT` | `Q` | NONE | NONE (no product run) | DRAFT |
| `Q-SIGNED-ZERO` | `Q` | NONE | NONE (no product run) | DRAFT |
| `Q-FPSPEC-VECTORS` | `Q` | NONE | NONE (no product run) | DRAFT |
| `Q-ROUTE-R1` | `Q` | NONE | NONE (no product run) | DRAFT |
| `E6-C1` | `E6` | NONE | NONE (no product run) | DRAFT |
| `E6-C2` | `E6` | NONE | NONE (no product run) | DRAFT |
| `E6-C3` | `E6` | NONE | NONE (no product run) | DRAFT |
| `E6-C4` | `E6` | NONE | NONE (no product run) | EXPLORATORY_NON_GOVERNING |
| `E6-C5` | `E6` | NONE | NONE (no product run) | DRAFT |
| `E6-C6` | `E6` | NONE | NONE (no product run) | DRAFT |
| `E6-C7` | `E6` | NONE | NONE (no product run) | DRAFT |
| `E6-C8` | `E6` | NONE | NONE (no product run) | DRAFT |
| `CL-CLEAN-FX1F-F0` | `KS` | NONE | O3 SUCCESS | SEALED_PENDING_PIN_CANDIDATE |
| `CL-CLEAN-FX1F-P` | `KS` | NONE | O3 SUCCESS | SEALED_PENDING_PIN_CANDIDATE |
| `CL-CLEAN-FX1F-FP` | `KS` | NONE | O3 SUCCESS | SEALED_PENDING_PIN_CANDIDATE |
| `CL-CLEAN-FX2F-ALL` | `KS` | NONE | O3 SUCCESS | SEALED_PENDING_PIN_CANDIDATE |
| `CL-CLEAN-FX4F-ALL` | `KS` | NONE | O3 SUCCESS | SEALED_PENDING_PIN_CANDIDATE |
| `CL-CLEAN-FXANN-FP` | `KS` | NONE | O3 SUCCESS | SEALED_PENDING_PIN_CANDIDATE |
| `CL-CLEAN-FXANN-LAY-FP` | `KS` | NONE | O3 SUCCESS | SEALED_PENDING_PIN_CANDIDATE |
| `CL-CLEAN-FXDIM-FP` | `KS` | NONE | O3 SUCCESS | SEALED_PENDING_PIN_CANDIDATE |
| `NC-E02` | `ID-A` | P0 | O1 | DRAFT |
| `NC-E05` | `ID-A` | P0 | O1 | DRAFT |
| `NC-E06` | `E6` | P0 | O1 | DRAFT |
| `NC-E07` | `ID-A` | P0 | O1 | DRAFT |
| `NC-E09` | `E9` | P0 | O1 | DRAFT |
| `NC-E10` | `E10` | P0 | O1 | DRAFT |
| `NC-E11M-PV` | `E11M` | between PS and PV | O2 (abort verified; ABORT-VERIFY match) | DRAFT |
| `NC-E11M-PC` | `E11M` | X7 | O7 | DRAFT |
| `NC-E12A` | `E12-V` | Y1 | O2 (abort before Commit; ABORT-VERIFY match) | SEALED_PENDING_PIN_CANDIDATE |
| `NC-E12D` | `E12-V` | P0 | O1 | DRAFT |
| `NC-E04-R2` | `E4` | P0 | O1 | SEALED_PENDING_PIN_CANDIDATE |
| `NC-WU-FOREIGN` | `WU` | P0 | O1 | DRAFT |
| `NC-S1-Y1` | `KS` | Y1 | O2 (abort before Commit; ABORT-VERIFY match: the injected write was inside T_... | SEALED_PENDING_PIN_CANDIDATE |
| `NC-S1-Y1-SILENT` | `KS` | Y1 | O2 (abort before Commit; ABORT-VERIFY match: the injected write was inside T_... | SEALED_PENDING_PIN_CANDIDATE |
| `NC-E08-Y0` | `E8` | Y0 | O2 (abort before Commit; ABORT-VERIFY match: the injected write was inside T_... | SEALED_PENDING_PIN_CANDIDATE |
| `NC-E08-Y0-SILENT` | `E8` | Y0 | O2 (abort before Commit; ABORT-VERIFY match: the injected write was inside T_... | SEALED_PENDING_PIN_CANDIDATE |
| `NC-E08-Y2` | `E8` | Y2 | O2 (abort before Commit; ABORT-VERIFY match: the injected write was inside T_... | SEALED_PENDING_PIN_CANDIDATE |
| `NC-E08-Y2-SILENT` | `E8` | Y2 | O2 (abort before Commit; ABORT-VERIFY match: the injected write was inside T_... | SEALED_PENDING_PIN_CANDIDATE |
| `EA1-READONLY-OPENS` | `EA1` | NONE | NONE (no commit in the measured interval) | DRAFT |
| `EA2-DEFERRED-EVENTS` | `EA2` | NONE | O3 SUCCESS | DRAFT |
| `OU-O1` | `OU` | NONE | O1 | DRAFT |
| `OU-O2` | `OU` | NONE | O2 | DRAFT |
| `OU-O6` | `OU` | NONE | O6 | DRAFT |
| `OU-O4` | `OU` | NONE | O4 | DRAFT |
| `OU-O5` | `OU` | NONE | O5 | DRAFT |
| `OU-O7` | `OU` | NONE | O7 | DRAFT |
| `OU-O3` | `OU` | NONE | O3 | DRAFT |
| `OU-ROW1-INVALID` | `OU` | NONE | no governing outcome; PROVISIONAL_NON_GOVERNING_OUTCOME retained | DRAFT |
| `OU-PREDICATE` | `OU` | NONE | NONE | DRAFT |
| `AB-COMMIT-EXCEPTION-T1` | `OU` | NONE | O2 on a matching synthetic ABORT-VERIFY; O6 on a mismatching one (two sub-cases) | DRAFT |
| `PR-SOURCE-SWAP-MODIFY` | `PR-FRESH` | after PR (acquisition) | O3 SUCCESS | DRAFT |
| `PR-SOURCE-SWAP-DELETE` | `PR-FRESH` | after PR (acquisition) | O3 SUCCESS | DRAFT |
| `PR-SOURCE-SWAP-NEG-COPY` | `PR-FRESH` | between PR and PW | O2 (the import does not start; the run stops in PREPARE-W; ABORT-VERIFY match... | DRAFT |
| `PR-TORN-READ` | `PR-FRESH` | during PR acquisition | O1 (acquisition UNKNOWN => refusal before any product write) | DRAFT |
| `PW-CLONE-BASE` | `PW-CLONE` | NONE | O3 SUCCESS | DRAFT |
| `PW-CLONE-CASE` | `PW-CLONE` | NONE | O3 SUCCESS | DRAFT |
| `PW-CLONE-WRONG-TYPE` | `PW-CLONE` | NONE | O1 | DRAFT |
| `PW-CLONE-NESTED` | `PW-CLONE` | NONE | O3 SUCCESS | DRAFT |
| `PW-CLONE-SYMTAB` | `PW-CLONE` | NONE | O3 SUCCESS | DRAFT |
| `PR-CLOSURE-COMPLETENESS` | `PR-REC` | NONE | NONE (qualification run on a test document; no mirror) | DRAFT |
| `PR-REC-CLOSURE-UNKNOWN` | `PR-REC` | NONE | O1 | DRAFT |
| `PR-REC-UNREADABLE` | `PR-REC` | PR | O1 | DRAFT |
| `PR-REC-REOBS-MISMATCH` | `PR-REC` | between PR and PW | O1 (empty manifest) | DRAFT |
| `PW-IMPORT-PARTIAL-FAIL` | `PW` | inside PW | O2 if ABORT-VERIFY matches the record plus the manifest of completed imports;... | DRAFT |
| `AB-AFTER-PREPARE-W` | `AB` | between PP and PS | O2 (T_M NOT_OPENED after a PREPARE-W write; ABORT-VERIFY match with the manif... | DRAFT |
| `AB-AFTER-MUTATION` | `AB` | between PS and PV | O2 | DRAFT |
| `KS-SL1-POSTWRITE` | `KS` | inside SL-1 | O2 | DRAFT |
| `KS-SL1-TX-MISMATCH` | `KS` | SL-1 entry | O2 (nothing written by the seam; abort; ABORT-VERIFY match) | DRAFT |
| `KS-SL4-NEG-DET` | `KS` | SL-4 entry | O2 | DRAFT |
| `KS-KR-RECORD` | `KR` | NONE | as the clean run it accompanies | DRAFT |
| `LK-LM1-PROPS` | `LK` | NONE | O3 SUCCESS | DRAFT |
| `LK-LM1-CONFLICT` | `LK` | between PL and V1 | O3 SUCCESS (the probe targets a non-protected object; its effect is not adjud... | DRAFT |
| `LK-LM2-PROPS` | `LK` | NONE | O3 SUCCESS | DRAFT |
| `LK-LM2-CONFLICT` | `LK` | between PL and V1 | O3 SUCCESS (the probe targets a non-protected object; its effect is not adjud... | DRAFT |
| `E4-LEARN-R1` | `E4` | NONE | O3 SUCCESS | DRAFT |
| `E4-VAL-R1` | `E4` | NONE | O3 SUCCESS | SEALED_PENDING_PIN_CANDIDATE |
| `EV-L-OP01` | `E12-L` | NONE | NONE (learning run) | DRAFT |
| `EV-L-OP02` | `E12-L` | NONE | NONE (learning run) | DRAFT |
| `EV-L-OP03-04` | `E12-L` | NONE | NONE (learning run) | DRAFT |
| `EV-L-OP05-06` | `E12-L` | NONE | NONE (learning run) | DRAFT |
| `EV-L-OP07-08` | `E12-L` | NONE | NONE (learning run) | DRAFT |
| `EV-L-OP09-10` | `E12-L` | NONE | NONE (learning run) | DRAFT |
| `EV-L-OP11` | `E12-L` | NONE | NONE (learning run) | DRAFT |
| `EV-L-OP12-14` | `E12-L` | NONE | NONE (learning run) | DRAFT |
| `EV-L-OP15` | `E12-L` | NONE | NONE (learning run) | DRAFT |
| `EV-L-OP17` | `E12-L` | NONE | NONE (learning run) | DRAFT |
| `EV-L-SEAM-REF-PROPS` | `E12-L` | NONE | NONE (learning run) | DRAFT |
| `EV-L-COMPOSED` | `E12-L` | NONE | NONE (learning run) | DRAFT |
| `EV-V-FX-2F` | `E12-V` | NONE | O3 SUCCESS | SEALED_PENDING_PIN_CANDIDATE |
| `EV-V-FX-4F` | `E12-V` | NONE | O3 SUCCESS | SEALED_PENDING_PIN_CANDIDATE |
| `EV-V-FX-BIG` | `E12-V` | NONE | O3 SUCCESS | SEALED_PENDING_PIN_CANDIDATE |
| `WR-A-X0` | `SC-A` | X0 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-A-X1` | `SC-A` | X1 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-A-X2` | `SC-A` | X2 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-A-X3` | `SC-A` | X3 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-A-X4` | `SC-A` | X4 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-A-X5` | `SC-B` | X5 | CONDITIONAL (see rule) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-A-X6` | `SC-B` | X6 | CONDITIONAL (see rule) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-A-X7` | `SC-B` | X7 | O3 with RESIDUAL_MISSED_CHANGE (X7 outside W-scan and phase C); POST_V1_TO_CP... | SEALED_PENDING_PIN_CANDIDATE |
| `WR-A-X8` | `SC-B` | X8 | O3; outside the guarantee (W3); recorded for the disclosure only | SEALED_PENDING_PIN_CANDIDATE |
| `WR-D-X0` | `SC-A` | X0 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-D-X1` | `SC-A` | X1 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-D-X2` | `SC-A` | X2 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-D-X3` | `SC-A` | X3 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-D-X4` | `SC-A` | X4 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-D-X5` | `SC-B` | X5 | CONDITIONAL (see rule) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-D-X6` | `SC-B` | X6 | CONDITIONAL (see rule) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-D-X7` | `SC-B` | X7 | O3 with RESIDUAL_MISSED_CHANGE (X7 outside W-scan and phase C); POST_V1_TO_CP... | SEALED_PENDING_PIN_CANDIDATE |
| `WR-D-X8` | `SC-B` | X8 | O3; outside the guarantee (W3); recorded for the disclosure only | SEALED_PENDING_PIN_CANDIDATE |
| `WR-E-X0` | `SC-A` | X0 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-E-X1` | `SC-A` | X1 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-E-X2` | `SC-A` | X2 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-E-X3` | `SC-A` | X3 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-E-X4` | `SC-A` | X4 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-E-X5` | `SC-B` | X5 | CONDITIONAL (see rule) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-E-X6` | `SC-B` | X6 | CONDITIONAL (see rule) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-E-X7` | `SC-B` | X7 | O3 with RESIDUAL_MISSED_CHANGE (X7 outside W-scan and phase C); POST_V1_TO_CP... | SEALED_PENDING_PIN_CANDIDATE |
| `WR-E-X8` | `SC-B` | X8 | O3; outside the guarantee (W3); recorded for the disclosure only | SEALED_PENDING_PIN_CANDIDATE |
| `WR-F-X0` | `SC-A` | X0 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-F-X1` | `SC-A` | X1 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-F-X2` | `SC-A` | X2 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-F-X3` | `SC-A` | X3 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-F-X4` | `SC-A` | X4 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-F-X5` | `SC-B` | X5 | CONDITIONAL (see rule) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-F-X6` | `SC-B` | X6 | CONDITIONAL (see rule) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-F-X7` | `SC-B` | X7 | O3 with RESIDUAL_MISSED_CHANGE (X7 outside W-scan and phase C); POST_V1_TO_CP... | SEALED_PENDING_PIN_CANDIDATE |
| `WR-F-X8` | `SC-B` | X8 | O3; outside the guarantee (W3); recorded for the disclosure only | SEALED_PENDING_PIN_CANDIDATE |
| `WR-G-X0` | `SC-A` | X0 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-G-X1` | `SC-A` | X1 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-G-X2` | `SC-A` | X2 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-G-X3` | `SC-A` | X3 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-G-X4` | `SC-A` | X4 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-G-X5` | `SC-B` | X5 | CONDITIONAL (see rule) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-G-X6` | `SC-B` | X6 | CONDITIONAL (see rule) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-G-X7` | `SC-B` | X7 | O3 with RESIDUAL_MISSED_CHANGE (X7 outside W-scan and phase C); POST_V1_TO_CP... | SEALED_PENDING_PIN_CANDIDATE |
| `WR-G-X8` | `SC-B` | X8 | O3; outside the guarantee (W3); recorded for the disclosure only | SEALED_PENDING_PIN_CANDIDATE |
| `WR-B-X0` | `SC-B` | X0 | O3 with RESIDUAL_MISSED_CHANGE (ABA restored inside the commit boundary) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-B-X3` | `SC-B` | X3 | CONDITIONAL (see rule) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-C-X5` | `SC-B` | X5 | CONDITIONAL (see rule) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-C-X6` | `SC-B` | X6 | CONDITIONAL (see rule) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-SA-X1` | `SC-A` | X1 | O4 (semantic comparison detects; no event delivered) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-SA-X2` | `SC-A` | X2 | O4 (semantic comparison detects; no event delivered) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-SA-X3` | `SC-A` | X3 | O4 (semantic comparison detects; no event delivered) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-SA-X4` | `SC-A` | X4 | O4 (semantic comparison detects; no event delivered) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-SB-X3` | `SC-B` | X3 | O3 with RESIDUAL_MISSED_CHANGE (silent writer: neither channel) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-SC-X5` | `SC-B` | X5 | O3 with RESIDUAL_MISSED_CHANGE (silent writer: neither channel) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-SC-X6` | `SC-B` | X6 | O3 with RESIDUAL_MISSED_CHANGE (silent writer: neither channel) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-A-X1-EV` | `SC-A` | X1 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-A-X2-EV` | `SC-A` | X2 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-A-X3-EV` | `SC-A` | X3 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-A-X4-EV` | `SC-A` | X4 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-A-X5-EV` | `SC-B` | X5 | CONDITIONAL (see rule) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-A-X6-EV` | `SC-B` | X6 | CONDITIONAL (see rule) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-D-X1-EV` | `SC-A` | X1 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-D-X2-EV` | `SC-A` | X2 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-D-X3-EV` | `SC-A` | X3 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-D-X4-EV` | `SC-A` | X4 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-D-X5-EV` | `SC-B` | X5 | CONDITIONAL (see rule) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-D-X6-EV` | `SC-B` | X6 | CONDITIONAL (see rule) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-E-X1-EV` | `SC-A` | X1 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-E-X2-EV` | `SC-A` | X2 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-E-X3-EV` | `SC-A` | X3 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-E-X4-EV` | `SC-A` | X4 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-E-X5-EV` | `SC-B` | X5 | CONDITIONAL (see rule) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-E-X6-EV` | `SC-B` | X6 | CONDITIONAL (see rule) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-F-X1-EV` | `SC-A` | X1 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-F-X2-EV` | `SC-A` | X2 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-F-X3-EV` | `SC-A` | X3 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-F-X4-EV` | `SC-A` | X4 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-F-X5-EV` | `SC-B` | X5 | CONDITIONAL (see rule) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-F-X6-EV` | `SC-B` | X6 | CONDITIONAL (see rule) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-G-X1-EV` | `SC-A` | X1 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-G-X2-EV` | `SC-A` | X2 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-G-X3-EV` | `SC-A` | X3 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-G-X4-EV` | `SC-A` | X4 | O4 (semantic comparison detects; precedence over O7) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-G-X5-EV` | `SC-B` | X5 | CONDITIONAL (see rule) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-G-X6-EV` | `SC-B` | X6 | CONDITIONAL (see rule) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-B-X3-EV` | `SC-B` | X3 | CONDITIONAL (see rule) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-C-X5-EV` | `SC-B` | X5 | CONDITIONAL (see rule) | SEALED_PENDING_PIN_CANDIDATE |
| `WR-C-X6-EV` | `SC-B` | X6 | CONDITIONAL (see rule) | SEALED_PENDING_PIN_CANDIDATE |
| `WU-DRY-FX1F-FP` | `WU` | NONE | as the family (not adjudicated: dry run) | DRAFT |
| `WU-DRY-FX2F-ALL` | `WU` | NONE | as the family (not adjudicated: dry run) | DRAFT |
| `WU-DRY-FX4F-ALL` | `WU` | NONE | as the family (not adjudicated: dry run) | DRAFT |
| `WU-DRY-FXANN-FP` | `WU` | NONE | as the family (not adjudicated: dry run) | DRAFT |
| `WU-DRY-FXDIM-FP` | `WU` | NONE | as the family (not adjudicated: dry run) | DRAFT |
| `WU-DRY-FXIMP` | `WU` | NONE | as the family (not adjudicated: dry run) | DRAFT |
| `WU-BOUNDARY` | `WU` | NONE | NONE (no product mutation) | DRAFT |
| `U-1` | `U-1..U-8` | NONE | per the preceding run (declared in the input) | DRAFT |
| `U-2` | `U-1..U-8` | NONE | per the preceding run (declared in the input) | DRAFT |
| `U-3` | `U-1..U-8` | NONE | per the preceding run (declared in the input) | DRAFT |
| `U-4` | `U-1..U-8` | NONE | per the preceding run (declared in the input) | DRAFT |
| `U-5` | `U-1..U-8` | NONE | per the preceding run (declared in the input) | DRAFT |
| `U-7` | `U-1..U-8` | NONE | per the preceding run (declared in the input) | DRAFT |
| `U-8` | `U-1..U-8` | NONE | per the preceding run (declared in the input) | DRAFT |
| `U-6` | `U-1..U-8` | NONE | O6 (T1 synthetic) | DRAFT |
| `U-6b` | `U-6b` | NONE | per the host exception | DRAFT |
| `S1-SAVE-REOPEN` | `S1` | NONE | O3 SUCCESS (preceding run) | DRAFT |
| `PP-POSTCP` | `PP` | NONE | O3 SUCCESS (preceding run) | DRAFT |
| `CL-CLEANUP-CHECK` | `CL` | NONE | O3 SUCCESS (preceding run) | DRAFT |
| `CL-CLEANUP-INCOMPLETE` | `CL` | NONE | no governing outcome (run INVALID) | DRAFT |

## 6. Rulings applied

### 6.1 G-1: the Y injection coordinates (harness level; no contract amendment)

The positions X0..X8 belong to the W-scan experiment (V2.1 10.6). Negative controls of S-1 and E-08 need coordinates **before PV**; they are defined at harness level (qualified as instrument I-14, scenario `Q-I14-Y`):

| Coordinate | Point |
|---|---|
| `Y0` | after the PS observation and before the first seam write |
| `Y1` | after the last seam write and before the staged reads |
| `Y2` | after the sealing of `VERIFY_ELEMENT_LIST` and before the PV evaluation |

The injected write uses the caller transaction `T_M` (it does not violate E-06 unless the scenario targets E-06). It normally emits a phase-A event, so E-12 phase A may detect it too; both channels are recorded. Isolation of the target control needs the silent variant `Wr-S`, whose **availability is measured** (`Q-I14-SILENT`), never assumed. Expected behaviour: S-1 at Y1 => S-1 FAIL => abort before `Commit()` => O2 when ABORT-VERIFY matches; E-08 at Y0/Y2 => E-08 FAIL at PV => abort => O2.

### 6.2 G-2: tuple identity versus product refusal

| Situation | Result |
|---|---|
| **governed tuple identity mismatch** (any field of V2.1 2.1 differs from the baseline, whatever its origin) | the run is **INVALID**; never a product outcome |
| **session / runtime control failure** (E-02..E-07, E-09, E-10, E-11M) in a valid run | the product **refuses** (O1 at entry; abort at PV) |
| **product plan outside the envelope** (for example topes, corner fondos) | plan-time refusal (O1); **outside the CT-21D baseline** unless a scenario explicitly governs it |
| E-01 or E-13 mismatch **after** the control plane verified the tuple | an inconsistent tuple: **INVALID** |

Consequently **`NC-E01` and `NC-E13` are removed** as governing product scenarios. Their detection is covered by the qualification scenarios `Q-I02` (host identity) and `Q-I04` (system-variable reads), and BA-02b records for E-01 and E-13: `CANNOT_BE_DELIBERATELY_VIOLATED_IN_A_GOVERNING_RUN`.

### 6.3 G-3: `E6-C4`

`E6-C4` remains `EXPLORATORY_NON_GOVERNING`. Its observed value characterizes `NG-09` and is reported in Owner Act 2. It is never promoted without a reviewed contract change.

### 6.4 G-4: fixture dependency (the gap exists; it is not a documentation error)

G-4 is the dependency of every scenario with a plan input on the fixtures of BA-08. In V2 the fixtures are **specified** (BA-08 V2 section 12: `FX-1F`, `FX-2F`, `FX-4F`, `FX-ANN`, `FX-DIM`, `FX-IMP`, `FX-BIG`, templates and the variant library `L-VAR`), so every such scenario names its fixture; the **fixture instances** (template drawings with the source rack drawn by the exact build, and the library copies) need host work that is not authorized. `INPUT_STATUS` records this per scenario.

### 6.5 Other corrections

- Synthetic scenarios use `CONTROL_PLANE_TUPLE` (no host field).
- Every former "per fixture plan" outcome is now exact (`O3`, `O1` or `NONE`) with its authority.
- `SEALED_PENDING_PIN` is used for scenarios whose expectation depends on an evidence-derived value.
- Pending classes are authored as concrete scenarios: clean runs, E-04 learning and validation, `LOCK_MODE` characterization, E-12 learning micro-scenarios and validation, warm-up dry runs, `PR-*`, `PW-*`, `AB-*`, `KS-*`, `U-*`, `S1`, `PP`, cleanup, and the event-triggered writer variants.

## 7. What prevents sealing the catalog

| Id | Blocking item | Gate |
|---|---|---|
| C-1 | fixture **instances** (template drawings with the source rack drawn by the exact build; library copies; the variant library `L-VAR`) | host (not authorized) |
| C-2 | the construction procedure of `FX-IMP` (a template in which a block required by the mirrored plan is absent): its natural occurrence is UNKNOWN (BA-08 V2 K-8) | Architect ruling |
| C-3 | the repetition values (`PARAM-01`, `PARAM-02`) | Owner decisions Q-O1..Q-O3 |
| C-4 | the designated route R-1 and its delivery (keystrokes by the control plane) | Coordinator ratification; `Q-ROUTE-R1` measures it |
| C-5 | the synthetic log fixtures of the `OU-*` scenarios | implementation of the evaluator (implementation gate) |
| C-6 | review and agreement of every expected result, then the seal (`SEALED_AT`) scenario by scenario | baseline review |

```text
CATALOG = DRAFT V2 (217 scenarios; 0 sealed)     NB-2 = REMAINS_BLOCKER_FOR_AUTHORITY_BASELINE
CT21D_AUTHORITY_BASELINE_READY = FALSE
```
