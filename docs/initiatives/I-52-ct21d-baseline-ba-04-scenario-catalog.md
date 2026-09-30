# I-52 — CT-21D Baseline Artifact BA-04: Scenario Catalog (DRAFT V1)

> **BASELINE ARTIFACT BA-04 — DRAFT, NOT SEALED, NOT AGREED. Design / documentation only. Nothing here is executed or authorized.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-SCENARIO-CATALOG
> ARTIFACT_VERSION          = 1-DRAFT
> ARTIFACT_STATUS           = DRAFT (phase 1 of baseline preparation)
> ARTIFACT_HASH             = TO_BE_RECORDED_AT_SEAL   (see BA-11)
> AUTHORITY_CONTRACT        = CT-21D V2.1 + V2.2 + V2.3 (AGREED_FOR_BASELINE_ARTIFACT_PREPARATION)
> BLOCKER                   = NB-2  (REMAINS_BLOCKER_FOR_AUTHORITY_BASELINE until the catalog is reviewed and agreed)
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```
>
> The machine-readable catalog is [the JSON file](I-52-ct21d-baseline-ba-04-scenario-catalog.json), which carries **every field of every scenario**. This document carries the schema, the defaults, the rules, the scenario index and the list of scenario classes that are **not yet authored**. Both files form the artifact.

## 1. Schema (normative, from V2.1 section 26.1 and V2.2 PA-3)

Every scenario carries: `SCENARIO_ID`, `SCENARIO_VERSION`, `GROUP`, `MODE`, `TUPLE_REQUIREMENTS`, `PLAN_OR_INPUT`, `DELIBERATE_VIOLATION_FLAG`, `TARGET_NEGATIVE_CONTROL`, `EXPECTED_PRODUCT_OUTCOME`, `EXPECTED_CONTROL_RESULTS`, `EXPECTED_INSTRUMENT_STATES`, `EXPECTED_GROUP_VERDICT`, `EXPECTED_LOG_RECORD_SET`, `EXPECTED_EVIDENCE_SET`,
`EXPECTED_CLEANUP`, `RETRY_APPLICABILITY`, `CONTROL_CLASSES_EXERCISED`, `EVM_VERSION`, `AUTHORITY_SOURCE_FOR_EXPECTED_RESULT`, `EXPECTED_PROCEDURAL_RESULT`, `STATUS`, `SEALED_AT`. The JSON adds `INPUT_STATUS` (whether the plan or fixture input is defined) and, where needed, `NOTE`.

## 2. Rules applied in this draft

- **An expected result cites its authority** (`AUTHORITY_SOURCE_FOR_EXPECTED_RESULT`): a clause of the contract, a row of the evaluator table, or `EVIDENCE_DERIVED` with a pinned artifact.
- **`EXPLORATORY_NON_GOVERNING`** is the status of a scenario whose expected result lacks an authority. It is never promoted after the fact; promotion needs a **new sealed scenario version** with a reviewed authority (V2.1 26.2).
- **No expected result is inferred from a future observation.** Where the contract determines an expected result only conditionally on the run's own log (the `COND` rows of the scan matrix), the condition is written into the expectation.
- `SEALED_AT = UNSET` for every scenario: nothing is sealed. `STATUS = DRAFT` means the authority for the expected result exists in the contract, while the input (plan or fixture) may still be `TO_BE_DEFINED`; such a scenario cannot be sealed until the input is defined.
- A group whose class is `EVIDENCE` uses `EVIDENCE_COMPLETE` (V2.2 PA-3); its `EXPECTED_PROCEDURAL_RESULT` is the reference.

## 3. Defaults (apply to every scenario unless the scenario overrides the field)

| Field | Default |
|---|---|
| `TUPLE_REQUIREMENTS` | FULL_BASELINE_TUPLE (V2.1 section 2.1; every BUILD_BOUND and MACHINE_PROFILE_BOUND field equal to the baseline; SESSION_BOUND fields present and consistent) |
| `EXPECTED_LOG_RECORD_SET` | RUN_LOG_STANDARD (V2.1 18.1: no sequence gap; record types required by the phases the scenario reaches) |
| `EXPECTED_EVIDENCE_SET` | EVIDENCE_PACKAGE_STANDARD (V2.1 18.2) |
| `EXPECTED_CLEANUP` | CLEANUP_STANDARD (V2.1 20: fresh process per run, fixture copy, private library copy retained then archived, profile unchanged, leftovers listed, evidence sealed) |
| `RETRY_APPLICABILITY` | NONE_AUTHORIZED (V2.1 21.3: INVALID => Coordinator authorization per occurrence; governing FAIL or UNKNOWN never retried) |
| `EVM_VERSION` | NONE |
| `EXPECTED_INSTRUMENT_STATES` | all instruments RELIABLE (qualified on the exact build) |

## 4. Index of scenarios in this draft

Totals: **115** scenarios; **3** are `EXPLORATORY_NON_GOVERNING`; **81** need a plan or fixture input that is still `TO_BE_DEFINED` (the rest need no plan).

| Group | Scenarios | Count |
|---|---|---|
| `E10` | `NC-E10` | 1 |
| `E11M` | `NC-E11M-PV`, `NC-E11M-PC` | 2 |
| `E12-V` | `NC-E12A` | 1 |
| `E4` | `NC-E04` | 1 |
| `E6` | `E6-C1`, `E6-C2`, `E6-C3`, `E6-C4`, `E6-C5`, `E6-C6`, `E6-C7`, `E6-C8`, `NC-E06` | 9 |
| `E9` | `NC-E09` | 1 |
| `EA1` | `EA1-READONLY-OPENS` | 1 |
| `EA2` | `EA2-DEFERRED-EVENTS` | 1 |
| `ID-A` | `NC-E02`, `NC-E05`, `NC-E07`, `NC-E01` | 4 |
| `ID-B` | `NC-E13` | 1 |
| `LK` | `NC-E03` | 1 |
| `OU` | `OU-O1`, `OU-O2`, `OU-O6`, `OU-O4`, `OU-O5`, `OU-O7`, `OU-O3`, `OU-ROW1-INVALID` | 8 |
| `PR-FRESH` | `PR-SOURCE-SWAP-MODIFY`, `PR-SOURCE-SWAP-DELETE`, `PR-SOURCE-SWAP-NEG-COPY`, `PR-TORN-READ` | 4 |
| `PR-REC` | `PR-CLOSURE-COMPLETENESS` | 1 |
| `PW-CLONE` | `PW-CLONE-BASE`, `PW-CLONE-CASE`, `PW-CLONE-WRONG-TYPE`, `PW-CLONE-NESTED`, `PW-CLONE-SYMTAB` | 5 |
| `Q` | `Q-I01`, `Q-I02`, `Q-I03`, `Q-I04`, `Q-I05`, `Q-I06`, ... , `Q-I16`, `Q-SIGNED-ZERO` | 17 |
| `SC-A` | `WR-A-X0`, `WR-A-X1`, `WR-A-X2`, `WR-A-X3`, `WR-A-X4`, `WR-D-X0`, ... , `WR-SA-X3`, `WR-SA-X4` | 29 |
| `SC-B` | `WR-A-X5`, `WR-A-X6`, `WR-A-X7`, `WR-A-X8`, `WR-D-X5`, `WR-D-X6`, ... , `WR-SC-X5`, `WR-SC-X6` | 27 |
| `WU` | `WU-BOUNDARY` | 1 |

### 4.1 Scenario summary (expected outcome, verdict, status)

| ID | GROUP | EXPECTED_PRODUCT_OUTCOME | EXPECTED_GROUP_VERDICT | STATUS | AUTHORITY (short) |
|---|---|---|---|---|---|
| `Q-I01` | `Q` | NONE (no product run) | PASS (EVIDENCE_COMPLETE) | DRAFT | V2.1 section 4.3, row I-01 (acceptance: no gap, duplicate or regres... |
| `Q-I02` | `Q` | NONE (no product run) | PASS (EVIDENCE_COMPLETE) | DRAFT | V2.1 section 4.3, row I-02 (acceptance: equality with the known tup... |
| `Q-I03` | `Q` | NONE (no product run) | PASS (EVIDENCE_COMPLETE) | DRAFT | V2.1 section 4.3, row I-03 (acceptance: correct counts and identities) |
| `Q-I04` | `Q` | NONE (no product run) | PASS (EVIDENCE_COMPLETE) | DRAFT | V2.1 section 4.3, row I-04 (acceptance: equality and correct `UNKNO... |
| `Q-I05` | `Q` | NONE (no product run) | PASS (EVIDENCE_COMPLETE) | DRAFT | V2.1 section 4.3, row I-05 (acceptance: detection in the induced case) |
| `Q-I06` | `Q` | NONE (no product run) | PASS (EVIDENCE_COMPLETE) | DRAFT | V2.1 section 4.3, row I-06 (acceptance: correct state in both cases) |
| `Q-I07` | `Q` | NONE (no product run) | PASS (EVIDENCE_COMPLETE) | DRAFT | V2.1 section 4.3, row I-07 (acceptance: all E6 controls pass) |
| `Q-I08` | `Q` | NONE (no product run) | PASS (EVIDENCE_COMPLETE) | DRAFT | V2.1 section 4.3, row I-08 (acceptance: detection of the canary and... |
| `Q-I09` | `Q` | NONE (no product run) | PASS (EVIDENCE_COMPLETE) | DRAFT | V2.1 section 4.3, row I-09 (acceptance: delivery and loss detection) |
| `Q-I10` | `Q` | NONE (no product run) | PASS (EVIDENCE_COMPLETE) | DRAFT | V2.1 section 4.3, row I-10 (acceptance: coverage of each declared c... |
| `Q-I11` | `Q` | NONE (no product run) | PASS (EVIDENCE_COMPLETE) | DRAFT | V2.1 section 4.3, row I-11 (acceptance: per-field detection and no ... |
| `Q-I12` | `Q` | NONE (no product run) | PASS (EVIDENCE_COMPLETE) | DRAFT | V2.1 section 4.3, row I-12 (acceptance: all three) |
| `Q-I13` | `Q` | NONE (no product run) | PASS (EVIDENCE_COMPLETE) | DRAFT | V2.1 section 4.3, row I-13 (acceptance: detection of change and sta... |
| `Q-I14` | `Q` | NONE (no product run) | PASS (EVIDENCE_COMPLETE) | DRAFT | V2.1 section 4.3, row I-14 (acceptance: intended effect and no effe... |
| `Q-I15` | `Q` | NONE (no product run) | PASS (EVIDENCE_COMPLETE) | DRAFT | V2.1 section 4.3, row I-15 (acceptance: detection of truncation) |
| `Q-I16` | `Q` | NONE (no product run) | PASS (EVIDENCE_COMPLETE) | DRAFT | V2.1 section 4.3, row I-16 (acceptance: all rows reproduced and all... |
| `E6-C1` | `E6` | NONE (no product run) | PASS (as declared) | DRAFT | V2.1 section 4.2, reliability control E6-C1 |
| `E6-C2` | `E6` | NONE (no product run) | PASS (as declared) | DRAFT | V2.1 section 4.2, reliability control E6-C2 |
| `E6-C3` | `E6` | NONE (no product run) | PASS (as declared) | DRAFT | V2.1 section 4.2, reliability control E6-C3 |
| `E6-C4` | `E6` | NONE (no product run) | NOT_DECLARED (characterization result) | EXPLORATORY_NON_GOVERNING | NONE: V2.1 4.2 declares the question, not an expected value |
| `E6-C5` | `E6` | NONE (no product run) | PASS (as declared) | DRAFT | V2.1 section 4.2, reliability control E6-C5 |
| `E6-C6` | `E6` | NONE (no product run) | PASS (as declared) | DRAFT | V2.1 section 4.2, reliability control E6-C6 |
| `E6-C7` | `E6` | NONE (no product run) | PASS (as declared) | DRAFT | V2.1 section 4.2, reliability control E6-C7 |
| `E6-C8` | `E6` | NONE (no product run) | PASS (as declared) | DRAFT | V2.1 section 4.2, reliability control E6-C8 |
| `PR-SOURCE-SWAP-MODIFY` | `PR-FRESH` | per fixture plan (import completes; outcome not the subject) | PASS | DRAFT | V2.1 section 4.3.1 SOURCE_SWAP_CONTROL; V2.2 PA-6 |
| `PR-SOURCE-SWAP-DELETE` | `PR-FRESH` | per fixture plan | PASS | DRAFT | V2.1 section 4.3.1; V2.2 PA-6 |
| `PR-SOURCE-SWAP-NEG-COPY` | `PR-FRESH` | O2 with the residue produced so far (import does not start) | PASS | DRAFT | V2.1 section 4.3.1 (negative variant) and V2.1 19.1 (a mismatch sto... |
| `PR-TORN-READ` | `PR-FRESH` | O1 (acquisition UNKNOWN => refusal) | PASS | DRAFT | V2.2 PA-6 (mismatch: UNKNOWN; refusal O1 under V2.1 19.1) |
| `Q-SIGNED-ZERO` | `Q` | NONE (no product run) | PASS (EVIDENCE_COMPLETE) | DRAFT | V2.1 section 4.3.1 SIGNED_ZERO_CONTROL; BA-05 (fingerprint specific... |
| `PW-CLONE-BASE` | `PW-CLONE` | per fixture plan (import completes; outcome not the subject) | PASS | DRAFT | V2.2 PA-12 (CLONE_NON_REPLACE_CONTROL variants); V2.1 7.1, 11.3 |
| `PW-CLONE-CASE` | `PW-CLONE` | per fixture plan (import completes; outcome not the subject) | PASS | DRAFT | V2.2 PA-12 (CLONE_NON_REPLACE_CONTROL variants); V2.1 7.1, 11.3 |
| `PW-CLONE-WRONG-TYPE` | `PW-CLONE` | O1 | PASS | DRAFT | V2.2 PA-12 (CLONE_NON_REPLACE_CONTROL variants); V2.1 7.1, 11.3 |
| `PW-CLONE-NESTED` | `PW-CLONE` | per fixture plan (import completes; outcome not the subject) | PASS | DRAFT | V2.2 PA-12 (CLONE_NON_REPLACE_CONTROL variants); V2.1 7.1, 11.3 |
| `PW-CLONE-SYMTAB` | `PW-CLONE` | per fixture plan (import completes; outcome not the subject) | PASS | DRAFT | V2.2 PA-12 (CLONE_NON_REPLACE_CONTROL variants); V2.1 7.1, 11.3 |
| `PR-CLOSURE-COMPLETENESS` | `PR-REC` | per fixture plan | PASS | DRAFT | V2.2 PA-12 (CLOSURE_COMPLETENESS_CONTROL) |
| `WU-BOUNDARY` | `WU` | NONE (no product mutation) | PASS | DRAFT | V2.2 PA-12 (WARMUP_BOUNDARY_CONTROL), PA-5 |
| `NC-E02` | `ID-A` | O1 | PASS (designed detection) | DRAFT | V2.1 17.2 row 2 (entry control FAIL => O1); Proposal V2 section 5 E... |
| `NC-E03` | `LK` | conflict observable; characterization of the candidate mode | PASS (designed detection) | DRAFT | V2.1 section 3.4 properties P-L5 and P-L6 (the mode selection is a ... |
| `NC-E04` | `E4` | O1 | PASS (designed detection) | DRAFT | V2.1 section 3.3 rules (a value observed only under another route i... |
| `NC-E05` | `ID-A` | O1 | PASS (designed detection) | DRAFT | V2.1 17.2 row 2; Proposal V2 section 5 E-05 (REFUSE) |
| `NC-E06` | `E6` | O1 | PASS (designed detection) | DRAFT | V2.1 17.2 row 2; V2.1 section 4.2 expected counts (P0 = 0) |
| `NC-E07` | `ID-A` | O1 | PASS (designed detection) | DRAFT | V2.1 17.2 row 2; Proposal V2 section 5 E-07 (REFUSE) |
| `NC-E09` | `E9` | O1 | PASS (designed detection) | DRAFT | V2.1 17.2 row 2; Proposal V2 section 5 E-09 (REFUSE) |
| `NC-E10` | `E10` | O1 | PASS (designed detection) | DRAFT | V2.1 12.3 (baseline: refuse O1); 17.2 row 2 |
| `NC-E11M-PV` | `E11M` | O2 (abort verified) | PASS (designed detection) | DRAFT | V2.1 5 (PV: any control FAIL => abort); 17.2 row 3; Proposal V2 sec... |
| `NC-E11M-PC` | `E11M` | O7 | PASS (designed detection) | DRAFT | V2.1 5 (PC: class-B failure => O7); 17.2 row 7 |
| `NC-E12A` | `E12-V` | O2 (abort before Commit) | PASS (designed detection) | DRAFT | V2.1 14 (phase A: fail-closed abort before Commit); 17.2 row 3 |
| `NC-E01` | `ID-A` | UNDETERMINED | NOT_DECLARED | EXPLORATORY_NON_GOVERNING | NONE: V2.1 2.2 makes a tuple mismatch INVALID while Proposal V2 E-0... |
| `NC-E13` | `ID-B` | UNDETERMINED | NOT_DECLARED | EXPLORATORY_NON_GOVERNING | NONE: same ambiguity as NC-E01 (V2.1 2.1 makes the profile values M... |
| `EA1-READONLY-OPENS` | `EA1` | NONE (no product commit in the measured interval) | PASS if zero events; CHARACTERIZED_DIFFERENT otherwise | DRAFT | V2.1 14.7 (A-1); V4 2.1 |
| `EA2-DEFERRED-EVENTS` | `EA2` | O3 | PASS if zero events; CHARACTERIZED_DIFFERENT otherwise | DRAFT | V2.1 14.7 (A-2); V4 2.1 |
| `OU-O1` | `OU` | O1 | PASS (EVIDENCE_COMPLETE) | DRAFT | V2.1 section 17.2, row 2 |
| `OU-O2` | `OU` | O2 | PASS (EVIDENCE_COMPLETE) | DRAFT | V2.1 section 17.2, row 3 |
| `OU-O6` | `OU` | O6 | PASS (EVIDENCE_COMPLETE) | DRAFT | V2.1 section 17.2, row 4 |
| `OU-O4` | `OU` | O4 | PASS (EVIDENCE_COMPLETE) | DRAFT | V2.1 section 17.2, row 5 |
| `OU-O5` | `OU` | O5 | PASS (EVIDENCE_COMPLETE) | DRAFT | V2.1 section 17.2, row 6 |
| `OU-O7` | `OU` | O7 | PASS (EVIDENCE_COMPLETE) | DRAFT | V2.1 section 17.2, row 7 |
| `OU-O3` | `OU` | O3 | PASS (EVIDENCE_COMPLETE) | DRAFT | V2.1 section 17.2, row 8 |
| `OU-ROW1-INVALID` | `OU` | no governing outcome; PROVISIONAL_NON_GOVERNING_OUTCOME retained | PASS (EVIDENCE_COMPLETE) | DRAFT | V2.1 section 17.2, row 1 and 21.1.1 |
| `WR-A-X0` | `SC-A` | O4 (semantic comparison detects; precedence over O7) | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X0; writer A:... |
| `WR-A-X1` | `SC-A` | O4 (semantic comparison detects; precedence over O7) | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X1; writer A:... |
| `WR-A-X2` | `SC-A` | O4 (semantic comparison detects; precedence over O7) | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X2; writer A:... |
| `WR-A-X3` | `SC-A` | O4 (semantic comparison detects; precedence over O7) | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X3; writer A:... |
| `WR-A-X4` | `SC-A` | O4 (semantic comparison detects; precedence over O7) | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X4; writer A:... |
| `WR-A-X5` | `SC-B` | CONDITIONAL on the run log: if the phase-C write event on the prote... | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X5; writer A:... |
| `WR-A-X6` | `SC-B` | CONDITIONAL on the run log: if the phase-C write event on the prote... | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X6; writer A:... |
| `WR-A-X7` | `SC-B` | O3 with RESIDUAL_MISSED_CHANGE (position X7 is outside W-scan and o... | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X7; writer A:... |
| `WR-A-X8` | `SC-B` | O3; outside the guarantee (W3); recorded for the disclosure only | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X8; writer A:... |
| `WR-D-X0` | `SC-A` | O4 (semantic comparison detects; precedence over O7) | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X0; writer D:... |
| `WR-D-X1` | `SC-A` | O4 (semantic comparison detects; precedence over O7) | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X1; writer D:... |
| `WR-D-X2` | `SC-A` | O4 (semantic comparison detects; precedence over O7) | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X2; writer D:... |
| `WR-D-X3` | `SC-A` | O4 (semantic comparison detects; precedence over O7) | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X3; writer D:... |
| `WR-D-X4` | `SC-A` | O4 (semantic comparison detects; precedence over O7) | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X4; writer D:... |
| `WR-D-X5` | `SC-B` | CONDITIONAL on the run log: if the phase-C write event on the prote... | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X5; writer D:... |
| `WR-D-X6` | `SC-B` | CONDITIONAL on the run log: if the phase-C write event on the prote... | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X6; writer D:... |
| `WR-D-X7` | `SC-B` | O3 with RESIDUAL_MISSED_CHANGE (position X7 is outside W-scan and o... | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X7; writer D:... |
| `WR-D-X8` | `SC-B` | O3; outside the guarantee (W3); recorded for the disclosure only | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X8; writer D:... |
| `WR-E-X0` | `SC-A` | O4 (semantic comparison detects; precedence over O7) | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X0; writer E:... |
| `WR-E-X1` | `SC-A` | O4 (semantic comparison detects; precedence over O7) | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X1; writer E:... |
| `WR-E-X2` | `SC-A` | O4 (semantic comparison detects; precedence over O7) | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X2; writer E:... |
| `WR-E-X3` | `SC-A` | O4 (semantic comparison detects; precedence over O7) | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X3; writer E:... |
| `WR-E-X4` | `SC-A` | O4 (semantic comparison detects; precedence over O7) | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X4; writer E:... |
| `WR-E-X5` | `SC-B` | CONDITIONAL on the run log: if the phase-C write event on the prote... | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X5; writer E:... |
| `WR-E-X6` | `SC-B` | CONDITIONAL on the run log: if the phase-C write event on the prote... | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X6; writer E:... |
| `WR-E-X7` | `SC-B` | O3 with RESIDUAL_MISSED_CHANGE (position X7 is outside W-scan and o... | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X7; writer E:... |
| `WR-E-X8` | `SC-B` | O3; outside the guarantee (W3); recorded for the disclosure only | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X8; writer E:... |
| `WR-F-X0` | `SC-A` | O4 (semantic comparison detects; precedence over O7) | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X0; writer F:... |
| `WR-F-X1` | `SC-A` | O4 (semantic comparison detects; precedence over O7) | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X1; writer F:... |
| `WR-F-X2` | `SC-A` | O4 (semantic comparison detects; precedence over O7) | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X2; writer F:... |
| `WR-F-X3` | `SC-A` | O4 (semantic comparison detects; precedence over O7) | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X3; writer F:... |
| `WR-F-X4` | `SC-A` | O4 (semantic comparison detects; precedence over O7) | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X4; writer F:... |
| `WR-F-X5` | `SC-B` | CONDITIONAL on the run log: if the phase-C write event on the prote... | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X5; writer F:... |
| `WR-F-X6` | `SC-B` | CONDITIONAL on the run log: if the phase-C write event on the prote... | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X6; writer F:... |
| `WR-F-X7` | `SC-B` | O3 with RESIDUAL_MISSED_CHANGE (position X7 is outside W-scan and o... | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X7; writer F:... |
| `WR-F-X8` | `SC-B` | O3; outside the guarantee (W3); recorded for the disclosure only | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X8; writer F:... |
| `WR-G-X0` | `SC-A` | O4 (semantic comparison detects; precedence over O7) | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X0; writer G:... |
| `WR-G-X1` | `SC-A` | O4 (semantic comparison detects; precedence over O7) | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X1; writer G:... |
| `WR-G-X2` | `SC-A` | O4 (semantic comparison detects; precedence over O7) | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X2; writer G:... |
| `WR-G-X3` | `SC-A` | O4 (semantic comparison detects; precedence over O7) | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X3; writer G:... |
| `WR-G-X4` | `SC-A` | O4 (semantic comparison detects; precedence over O7) | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X4; writer G:... |
| `WR-G-X5` | `SC-B` | CONDITIONAL on the run log: if the phase-C write event on the prote... | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X5; writer G:... |
| `WR-G-X6` | `SC-B` | CONDITIONAL on the run log: if the phase-C write event on the prote... | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X6; writer G:... |
| `WR-G-X7` | `SC-B` | O3 with RESIDUAL_MISSED_CHANGE (position X7 is outside W-scan and o... | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X7; writer G:... |
| `WR-G-X8` | `SC-B` | O3; outside the guarantee (W3); recorded for the disclosure only | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X8; writer G:... |
| `WR-B-X0` | `SC-B` | O3 with RESIDUAL_MISSED_CHANGE (ABA restored inside the commit boun... | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, ABA row; writer B: modify elem... |
| `WR-B-X3` | `SC-B` | CONDITIONAL on the run log: if the phase-C write event on the prote... | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, ABA row; writer B: modify elem... |
| `WR-C-X5` | `SC-B` | CONDITIONAL on the run log: if the phase-C write event on the prote... | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X5; writer C:... |
| `WR-C-X6` | `SC-B` | CONDITIONAL on the run log: if the phase-C write event on the prote... | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, row for position X6; writer C:... |
| `WR-SA-X1` | `SC-A` | O4 (semantic comparison detects; no event is delivered by a silent ... | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, silent-writer row; writer A: m... |
| `WR-SA-X2` | `SC-A` | O4 (semantic comparison detects; no event is delivered by a silent ... | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, silent-writer row; writer A: m... |
| `WR-SA-X3` | `SC-A` | O4 (semantic comparison detects; no event is delivered by a silent ... | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, silent-writer row; writer A: m... |
| `WR-SA-X4` | `SC-A` | O4 (semantic comparison detects; no event is delivered by a silent ... | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, silent-writer row; writer A: m... |
| `WR-SB-X3` | `SC-B` | O3 with RESIDUAL_MISSED_CHANGE (silent writer: no delivered event a... | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, silent-writer row; writer B: m... |
| `WR-SC-X5` | `SC-B` | O3 with RESIDUAL_MISSED_CHANGE (silent writer: no delivered event a... | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, silent-writer row; writer C: m... |
| `WR-SC-X6` | `SC-B` | O3 with RESIDUAL_MISSED_CHANGE (silent writer: no delivered event a... | PASS if the observed outcome and channel equal the declared expectation | DRAFT | V2.1 section 10.6 expectation table, silent-writer row; writer C: m... |

## 5. Scenario classes required by the coverage matrix and NOT yet authored

Each class below is **required** (V2.1 26.3, BA-02 coverage matrix). No entry is invented: each row states what authority or input is missing.

| Class | Group | Why it is not authored yet (missing authority or input) |
|---|---|---|
| `CL-CLEAN-<family>` | ID-A, ID-B, E6, E9, E10, E11M, SC-A, KS | needs the fixture rack designs of the Selective kind baseline (BA-08) inside the sealed scope (BA-03); the expected outcome is O3 by V2.1 17.2 row 8 |
| `E4-LEARN-<route>`, `E4-VAL-<route>` | E4 | the designated invocation routes and the evidence-derived admitted set do not exist (V2.1 3.3) |
| `LK-<mode>` | LK | the `LOCK_MODE` candidate set is a BA-08 input (V2.1 3.4) |
| `EV-L-<operation>` | E12-L | needs the operation inventory completeness review (BA-06) |
| `EV-V-<plan>` | E12-V | needs a `FROZEN` EVM (V2.2 PA-2) and validation plans structurally outside the learning corpus (BA-06) |
| `WU-DRY-<scenario>` | WU | needs the final list of governed scenarios and the classified number of dry runs (BA-10) |
| `PR-*`, `PW-*` (import, residue, partial failure) | PR-REC, PW | needs fixtures and the seams (implementation is out of this gate) |
| `AB-*` | AB | needs fixtures; the simulated `Commit()` exception path is T1, not host evidence |
| `KS-*` | KS, KR | needs fixtures and the seam contract of BA-08 |
| `NC-S1`, `NC-E08` | KS, E8 | **gap G-1**: V2.1 10.6 defines injection positions X0..X8 only after `Commit()`; a negative control of the staged reads before PV needs a pre-PV position that the contract does not define. A reviewed amendment or ruling is needed |
| event-triggered variants of the writers | SC-A, SC-B | the same entries with the injection driven from a database event handler (V2.1 10.6); to be enumerated after the fixture plans exist |
| `U-1..U-8`, `U-6b`, `S1`, `PP`, `CL-*` | U, S1, PP, CL | need fixtures; their expected result is the procedural result of `EVIDENCE_COMPLETE` |

## 6. Gaps and findings of this draft

| Id | Finding |
|---|---|
| G-1 | no pre-PV injection position exists for the negative controls of S-1 and E-08 (section 5) |
| G-2 | `NC-E01` and `NC-E13` are `EXPLORATORY_NON_GOVERNING`: V2.1 2.2 makes an injected tuple or profile mismatch `INVALID`, while Proposal V2 says the controls `REFUSE` (O1); the contract does not say which applies |
| G-3 | `E6-C4` is `EXPLORATORY_NON_GOVERNING`: the visibility of `OpenCloseTransaction` and side-database transactions is a characterization target with no declared expected value |
| G-4 | the scenarios with `PLAN_OR_INPUT = TO_BE_DEFINED` cannot be sealed until BA-08 defines the fixture designs |

## 7. Status

```text
NB-2 = REMAINS_BLOCKER_FOR_AUTHORITY_BASELINE
CATALOG = DRAFT_NOT_SEALED     SEALED_SCENARIOS = 0     EXPLORATORY_NON_GOVERNING = 3
CT21D_AUTHORITY_BASELINE_READY = FALSE
```
