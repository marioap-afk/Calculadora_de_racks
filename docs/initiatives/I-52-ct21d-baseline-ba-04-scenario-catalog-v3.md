# I-52 — CT-21D Baseline Artifact BA-04: Scenario Catalog (DRAFT V3)

> **BASELINE ARTIFACT BA-04 V3 — DRAFT, NOT SEALED. Design / documentation only. Nothing here is executed or authorized.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-SCENARIO-CATALOG
> ARTIFACT_VERSION          = 3-DRAFT   (supersedes 2-DRAFT: blobs 35bf3ddef3ece05a9edef6620220912f46a7d20c (json) and caeff2c5bd266c449e6702c20e01beafd8917394 (md), which stay as history)
> STATUS AUTHORITY          = the BA-11 registry entry of this artifact is the only authority for its status and hashes;
>                             this header and the per-scenario statuses are frozen, non-authoritative text of the candidate bytes
> AUTHORITY_CONTRACT        = CT-21D V2.1 + V2.2 + V2.3 + V2.4
> PHASE-2 RULING APPLIED    = decisions section 211: AR2-08..AR2-31 and the BA-04 findings F01..F25
> BLOCKER                   = NB-2 (REMAINS_BLOCKER_FOR_AUTHORITY_BASELINE)
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```
>
> The machine-readable catalog is [the JSON file](I-52-ct21d-baseline-ba-04-scenario-catalog-v3.json) and carries every field of every scenario; the two files form the artifact.

## 1. Schema (normative)

Every scenario carries the fields of V2.1 26.1 (with V2.2 PA-3 `EXPECTED_PROCEDURAL_RESULT`) and these V3 fields:

| Field | Content |
|---|---|
| `PURPOSE` | why the scenario exists (V2.1 26.1 `GROUP`, `PURPOSE`) |
| `RUN_GOVERNANCE` | `GOVERNING`, `NON_GOVERNING_BY_DESIGN` (dry runs; the E-03/E-04 characterization of AR2-11) or `EXPLORATORY_NON_GOVERNING` |
| `REACH` | the furthest checkpoint the run is designed to reach: `NONE` (no product run), `SYN` (synthetic log), `LEARN`, `P0`, `PL`, `PR`, `PW`, `PV`, `CP` |
| `INJECTIONS` | the writers or faults (writer, variant, position), or none (V2.1 26.1) |
| `EXPECTED_EVENTS` | the event class expected per E-12 phase (V2.1 26.1) |
| `EXPECTED_DETECTION_CHANNEL` | the detection channel (V2.1 26.1): `SEMANTIC_COMPARISON`, `E-08_AT_PC`, `E-12`, `BOTH`, `NEITHER`, the target control, or `N/A` |
| `EXPECTED_LOG_RECORD_SET` | a log profile of section 4 (record types and counts) |
| `CONTROLS_EXERCISED` | the registry controls (keys of BA-02a) whose results the scenario expects |
| `CONTROL_CLASSES_EXERCISED` | the registry classes of those controls (`A`, `B`), plus `NOT_A_CONTROL` for evidence groups (V2.1 26.1 and 31 check 3); derived |
| `GROUPS_SERVED` | derived by the normative rule of BA-02b V3 section 1 (AR2-08) |
| `PIN_SLOTS` | named slots filled by pin records outside the catalog (section 2.1) |
| `BLOCKED_BY` | open items that prevent declaring or sealing the expectation (`K-3` = `TOL_SCALE` without authority; `K-8` = `FX-IMP` specification open) |
| `CONDITIONAL_EXPECTATION_RULE` | a declared rule of section 3, or `NONE` |
| `NOTE` | informative text |

## 2. Statuses and pins

| Status | Meaning |
|---|---|
| `DRAFT` | the scenario is not ready to seal; if `BLOCKED_BY` is not empty, its expectation **cannot be declared** until the blocker is resolved (V2.1 26.2) |
| `SEALED_PENDING_PIN_CANDIDATE` | the expected result has an authority except for pin slots; when sealed it becomes `SEALED_PENDING_PIN` |
| `SEALED_CANDIDATE` | no pin and no blocker; ready for the candidate ruling |
| `EXPLORATORY_NON_GOVERNING` | the expected result lacks an authority; never promoted after the fact |

### 2.1 Pin mechanism (AR2-16, AR2-17)

A pin is a **named slot** in the sealed scenario (`PIN_SLOTS`). Its value lives in a separate **pin record** with its own hash, seal, BA-11 entry and BA-07 custody entry (`PIN_RECORDED`, then `ANCHORED`). **Writing a pin never changes the bytes of this catalog.** A pin is written after the value is frozen from its own evidence and before the first governing run that uses it; it is never filled from the outcome of the scenario that uses it.

| Pin kind | Needed when | Source of the value |
|---|---|---|
| `FIXTURE_INSTANCE@<FX>` | the scenario has a fixture | the instance created by host work under a future gate, conforming to the sealed BA-08 specification |
| `E04_ADMITTED_SET@R-1` | a governing run passes P0 | `E4-LEARN-R1-*` under the selected `LOCK_MODE` (AR2-11) |
| `LOCK_MODE` | a governing run passes PL, and every learning run | the selection review of V2.1 3.4 over `LK-*` |
| `EVM_FROZEN` | a governing run reaches MUTATION | the EVM frozen after learning (V2.1 27.2; V2.2 PA-2) |
| `HOST_DEFAULT_MAP` | S-1, S-2 or S-3 decide results | host-default attributes derived from non-governing evidence (BA-08 V3, PH2-OR-1) |

`TOL_SCALE` is **not** a pin: it is a value of the Selective kind baseline (V2.2 PA-9) that has no authority today (K-3). Every governing scenario that evaluates S-1, S-2 or S-3 carries `BLOCKED_BY = K-3`.

## 3. Conditional-expectation rules (normative)

A conditional expectation is allowed **only** when it is **resolved offline, by a rule declared before the run**, from the **sealed evidence of the same run** or from a sealed qualification record it names. It is never resolved from hindsight, from a later run, or from an outcome the rule does not name. The offline evaluator (I-16) applies it; a rule that the sealed evidence cannot resolve makes the scenario result `UNKNOWN`. The declared rules:

| Rule | Text |
|---|---|
| `CER-ABV` | CER-ABV: resolved offline from the sealed ABORT-VERIFY record of the same run: match => O2; mismatch or unreadable => O6, recorded as an S-5 / AB finding (never charged to the target control) |
| `CER-WIT` | CER-WIT: resolved offline from the sealed I-14 DELIVERY WITNESS record of the same run at the injection position (an independent subscription of the I-14 instrument, qualified by Q-I14-X / Q-I14-Y), never from the E-12 log itself |
| `CER-SIL` | CER-SIL: RUNNABLE only if the sealed Q-I14-SILENT record shows AVAILABLE at the coordinate; otherwise the scenario is recorded NOT_RUNNABLE and the isolation of the target is NOT_ACHIEVED |
| `CER-A2` | resolved offline from the sealed phase-C log and the I-14 witness of the same run: no deferred event => O3 and A-2 PASS; a deferred event in phase C => O7 and A-2 CHARACTERIZED_DIFFERENT |

## 4. Defaults, repetition rules and log profiles

| Field | Default |
|---|---|
| `EXPECTED_EVIDENCE_SET` | EVIDENCE_PACKAGE_STANDARD (V2.1 18.2) |
| `EXPECTED_CLEANUP` | CLEANUP_STANDARD (V2.1 20); the original library file is restored before the next run's tuple check where a scenario changed it (AR2-31) |
| `RETRY_APPLICABILITY` | NONE_AUTHORIZED (V2.1 21.3; INVALID only with a Coordinator authorization per occurrence; a governing FAIL or UNKNOWN is never retried) |
| `EXPECTED_INSTRUMENT_STATES` | every instrument RELIABLE (qualified on the exact build) |
| `UNLISTED_CONTROLS` | every control the run evaluates and EXPECTED_CONTROL_RESULTS does not list is expected PASS; a control the run does not reach is NOT_EVALUATED (AR2-25) |
| `TUPLE_REQUIREMENTS` | `FULL_BASELINE_TUPLE (V2.1 2.1: every BUILD_BOUND and MACHINE_PROFILE_BOUND field equal to the baseline; SESSION_BOUND fields present and consistent; the block-library field is sampled once, before the acquisition, AR2-31)`; synthetic scenarios: `CONTROL_PLANE_TUPLE (no host): control-plane source SHA, evaluator assembly SHA-256, evaluator version, synthetic log fixture hash, contract composite hash (BA-11 section 4) and scenario-catalog hash; no host, machine-profile or session field applies (AR2-29)`; `Q-FPSPEC-VECTORS`: `FPSPEC_CONFORMANCE_TUPLE: fingerprint-provider assembly SHA-256, FINGERPRINT_SPEC_VERSION and the BA-05 artifact hash, SHA-256 of the vectors file, control-plane source SHA, contract composite hash (AR2-29)` |

| Repetition rule | Value |
|---|---|
| `DEFAULT` | PARAM-01 (BA-10 V3: per scenario, the pair of the strictest class among the groups served; UNSET) |
| `CLEAN` | max(PARAM-01 N of the strictest class served, ceil(PARAM-03 n / number of CL-CLEAN scenarios)) (BA-10 V3, AR2-28; UNSET) |
| `EVIDENCE` | EVIDENCE_REPETITION (BA-10 V3: one governing run per scenario, Coordinator choice pending; UNSET) |
| `SYNTH` | ONE (deterministic offline evaluation; BA-10 V3) |
| `LEARN` | PARAM-02 (BA-10 V3; UNSET) |
| `DRY` | WU_DRY_RUNS (BA-10 V3: the dry-run count of V2.1 21.7; UNSET) |
| `CHAR` | PARAM-01 (characterization; BA-10 V3; UNSET) |

| Log profile | Record types and counts |
|---|---|
| `LP-QUAL` | QUAL_START x1; POSITIVE_CONTROL x1; NEGATIVE_CONTROL x1 per negative variant; CONSISTENCY x1; QUAL_VERDICT x1; SEAL x1 |
| `LP-SYNTH` | INPUT_LOG_HASH x1; EVALUATION x1 per evaluated row or state; OUTCOME x1; SEAL x1 |
| `LP-P0` | RUN_START x1; TUPLE x1; WARMUP x1; WARMUP_LOADS x1; MEMBERSHIP x1; SUBSCRIPTION x2 (E-11M, E-12); BASELINE_SNAPSHOT x2; CHECKPOINT x1 (P0); OUTCOME x1; CLEANUP x1; SEAL x1 |
| `LP-PL` | LP-P0 plus LOCK x1 and CHECKPOINT x1 (PL) |
| `LP-PR` | LP-PL plus LIBRARY_INPUT_RECORD x1; AUTH12_CALL x (one per call); PRE_COMMAND_STATE_RECORD x1; PREPARE_R_DECISION x1; CHECKPOINT x1 (PR) |
| `LP-PW` | LP-PR plus REOBSERVATION x1; COPY_REHASH x1 per import; IMPORT x1 per import; IMPORT_FINGERPRINT x1 per completed import; MANIFEST x1; CHECKPOINT x1 (PP) |
| `LP-PV` | LP-PW plus TM_OPEN x1; SEAM_CALL x1 per seam call of f(plan); E12_EVENT x1 per delivered event (observed); CHECKPOINT x2 (PS, PV); ABORT x1; ABORT_VERIFY x1 |
| `LP-CP` | LP-PW plus TM_OPEN x1; SEAM_CALL x1 per seam call of f(plan); E12_EVENT x1 per delivered event; CHECKPOINT x2 (PS, PV); SEALED_LIST x1; COMMIT x1; PHASE_B_LOG x1; MARKER x2 (V0, V1); PASS1_READ and PASS2_READ x1 per sealed element each; PHASE_C_LOG_SEALED x1; POST_V1_TO_CP_QUEUE_LOG x1; CHECKPOINT x1 (PC = CP) |
| `LP-LEARN` | RUN_START x1; TUPLE x1; WARMUP x1; MICRO_OPERATION x1; E12_EVENT_RAW x1 per delivered event; CLEANUP x1; SEAL x1 |
| `LP-DRY` | the profile of the dry-run scenario's reach plus LOAD_EVENT x1 per assembly-load event (expected zero inside the governed window) |
| `LP-EVID` | the profile of the preceding run plus the evidence-specific records (UNDO_STEP x1 per step, STATE_READ x1 per compared element, SAVE_REOPEN, POST_CP_TAIL) |

## 5. Totals and index

Totals: **370** scenarios. Status: `DRAFT` = 155, `EXPLORATORY_NON_GOVERNING` = 1, `SEALED_CANDIDATE` = 44, `SEALED_PENDING_PIN_CANDIDATE` = 170. Governance: `EXPLORATORY_NON_GOVERNING` = 1, `GOVERNING` = 218, `NON_GOVERNING_BY_DESIGN` = 151. Blockers: `K-3` = 137, `K-8` = 26. Pin slots used: `E04_ADMITTED_SET` = 156, `EVM_FROZEN` = 137, `FIXTURE_INSTANCE` = 325, `HOST_DEFAULT_MAP` = 137, `LOCK_MODE` = 163.

Every expected outcome is an exact `O1..O7`, `NONE`, a declared conditional of section 3, or a non-governing result.

| Group | Count |
|---|---|
| `AB` | 3 |
| `CL` | 1 |
| `E10` | 1 |
| `E11M` | 2 |
| `E12-L` | 16 |
| `E12-V` | 6 |
| `E4` | 4 |
| `E6` | 9 |
| `E8` | 4 |
| `E9` | 1 |
| `EA1` | 1 |
| `EA2` | 1 |
| `ID-A` | 3 |
| `KS` | 13 |
| `LK` | 6 |
| `OU` | 11 |
| `PP` | 1 |
| `PR-FRESH` | 4 |
| `PR-REC` | 4 |
| `PW` | 1 |
| `PW-CLONE` | 5 |
| `Q` | 25 |
| `S1` | 1 |
| `SC-A` | 59 |
| `SC-B` | 32 |
| `U-1..U-8` | 8 |
| `U-6b` | 1 |
| `WU` | 147 |

### 5.1 Scenario summary (every scenario; `WU-DRY-*` are listed in section 5.2)

| ID | GROUP | REACH | COORD | TARGET | EXPECTED_PRODUCT_OUTCOME | STATUS | BLOCKED_BY |
|---|---|---|---|---|---|---|---|
| `Q-I01` | `Q` | NONE | NONE | INSTRUMENT:I-01 | NONE (no product run) | SEALED_CANDIDATE | - |
| `Q-I02` | `Q` | NONE | NONE | INSTRUMENT:I-02 | NONE (no product run) | SEALED_CANDIDATE | - |
| `Q-I03` | `Q` | NONE | NONE | INSTRUMENT:I-03 | NONE (no product run) | SEALED_CANDIDATE | - |
| `Q-I04` | `Q` | NONE | NONE | INSTRUMENT:I-04 | NONE (no product run) | SEALED_CANDIDATE | - |
| `Q-I05` | `Q` | NONE | NONE | INSTRUMENT:I-05 | NONE (no product run) | SEALED_CANDIDATE | - |
| `Q-I06` | `Q` | NONE | NONE | INSTRUMENT:I-06 | NONE (no product run) | SEALED_CANDIDATE | - |
| `Q-I07` | `Q` | NONE | NONE | INSTRUMENT:I-07 | NONE (no product run) | SEALED_CANDIDATE | - |
| `Q-I08` | `Q` | NONE | NONE | INSTRUMENT:I-08 | NONE (no product run) | SEALED_CANDIDATE | - |
| `Q-I09` | `Q` | NONE | NONE | INSTRUMENT:I-09 | NONE (no product run) | SEALED_CANDIDATE | - |
| `Q-I10` | `Q` | NONE | NONE | INSTRUMENT:I-10 | NONE (no product run) | SEALED_CANDIDATE | - |
| `Q-I11` | `Q` | NONE | NONE | INSTRUMENT:I-11 | NONE (no product run) | SEALED_CANDIDATE | - |
| `Q-I12` | `Q` | NONE | NONE | INSTRUMENT:I-12 | NONE (no product run) | SEALED_CANDIDATE | - |
| `Q-I13` | `Q` | NONE | NONE | INSTRUMENT:I-13 | NONE (no product run) | SEALED_CANDIDATE | - |
| `Q-I14` | `Q` | NONE | NONE | INSTRUMENT:I-14 | NONE (no product run) | SEALED_CANDIDATE | - |
| `Q-I15` | `Q` | NONE | NONE | INSTRUMENT:I-15 | NONE (no product run) | SEALED_CANDIDATE | - |
| `Q-I16` | `Q` | NONE | NONE | INSTRUMENT:I-16 | NONE (no product run) | SEALED_CANDIDATE | - |
| `Q-I09-LOSS` | `Q` | NONE | NONE | INSTRUMENT:I-09 | NONE (no product run) | SEALED_CANDIDATE | - |
| `Q-I10-LOSS` | `Q` | NONE | NONE | INSTRUMENT:I-10 | NONE (no product run) | SEALED_CANDIDATE | - |
| `Q-I14-Y` | `Q` | NONE | NONE | INSTRUMENT:I-14 | NONE (no product run) | SEALED_CANDIDATE | - |
| `Q-I14-X` | `Q` | NONE | NONE | INSTRUMENT:I-14 | NONE (no product run) | SEALED_CANDIDATE | - |
| `Q-I14-SILENT` | `Q` | NONE | NONE | NONE | NONE (no product run) | SEALED_CANDIDATE | - |
| `Q-SIGNED-ZERO` | `Q` | NONE | NONE | INSTRUMENT:I-12 | NONE (no product run) | SEALED_CANDIDATE | - |
| `Q-FPSPEC-VECTORS` | `Q` | NONE | NONE | NONE | NONE (no product run) | SEALED_CANDIDATE | - |
| `Q-ROUTE-R1` | `Q` | NONE | NONE | NONE | NONE (no product run) | SEALED_CANDIDATE | - |
| `Q-I16-CLEANUP-NEG` | `Q` | NONE | NONE | INSTRUMENT:I-16 | NONE (no product run) | SEALED_CANDIDATE | - |
| `E6-C1` | `E6` | NONE | NONE | NONE | NONE (no product run) | SEALED_CANDIDATE | - |
| `E6-C2` | `E6` | NONE | NONE | NONE | NONE (no product run) | SEALED_CANDIDATE | - |
| `E6-C3` | `E6` | NONE | NONE | NONE | NONE (no product run) | SEALED_CANDIDATE | - |
| `E6-C4` | `E6` | NONE | NONE | NONE | NONE (no product run) | EXPLORATORY_NON_GOVERNING | - |
| `E6-C5` | `E6` | NONE | NONE | NONE | NONE (no product run) | SEALED_CANDIDATE | - |
| `E6-C6` | `E6` | NONE | NONE | E06 | NONE (no product run) | SEALED_CANDIDATE | - |
| `E6-C7` | `E6` | NONE | NONE | NONE | NONE (no product run) | SEALED_CANDIDATE | - |
| `E6-C8` | `E6` | NONE | NONE | E06 | NONE (no product run) | SEALED_CANDIDATE | - |
| `CL-CLEAN-FX1F-F0` | `KS` | CP | NONE | NONE | O3 SUCCESS | DRAFT | K-3 |
| `CL-CLEAN-FX1F-P` | `KS` | CP | NONE | NONE | O3 SUCCESS | DRAFT | K-3 |
| `CL-CLEAN-FX1F-FP` | `KS` | CP | NONE | NONE | O3 SUCCESS | DRAFT | K-3 |
| `CL-CLEAN-FX2F-ALL` | `KS` | CP | NONE | NONE | O3 SUCCESS | DRAFT | K-3 |
| `CL-CLEAN-FX4F-ALL` | `KS` | CP | NONE | NONE | O3 SUCCESS | DRAFT | K-3 |
| `CL-CLEAN-FXANN-FP` | `KS` | CP | NONE | NONE | O3 SUCCESS | DRAFT | K-3 |
| `CL-CLEAN-FXDIM-FP` | `KS` | CP | NONE | NONE | O3 SUCCESS | DRAFT | K-3 |
| `CL-CLEAN-FXIMP-FP` | `KS` | CP | NONE | NONE | O3 SUCCESS | DRAFT | K-3, K-8 |
| `NC-E02` | `ID-A` | P0 | P0 | E02 | O1 | SEALED_PENDING_PIN_CANDIDATE | - |
| `NC-E05` | `ID-A` | P0 | P0 | E05 | O1 | SEALED_PENDING_PIN_CANDIDATE | - |
| `NC-E06` | `E6` | P0 | P0 | E06 | O1 | SEALED_PENDING_PIN_CANDIDATE | - |
| `NC-E07` | `ID-A` | P0 | P0 | E07 | O1 | SEALED_PENDING_PIN_CANDIDATE | - |
| `NC-E09` | `E9` | P0 | P0 | E09 | O1 | SEALED_PENDING_PIN_CANDIDATE | - |
| `NC-E10` | `E10` | P0 | 13.1 step 5 (between the two baseline snapshots) | E10 | O1 | SEALED_PENDING_PIN_CANDIDATE | - |
| `NC-E11M-PV` | `E11M` | PV | between PS and PV | E11M | O2 (CER-ABV) | DRAFT | K-3 |
| `NC-E11M-PC` | `E11M` | CP | X7 | E11M | O7 | DRAFT | K-3 |
| `NC-E12A` | `E12-V` | PV | Y1 | E12A | O2 (CER-ABV) | DRAFT | K-3 |
| `NC-E12D` | `E12-V` | P0 | P0 | E12D | O1 | SEALED_PENDING_PIN_CANDIDATE | - |
| `NC-E12C` | `E12-V` | CP | X6 | E12C | O7 (CER-WIT: delivered); NOT_RUNNABLE for NC-E12C purposes when not... | DRAFT | K-3 |
| `NC-E04-R2` | `E4` | P0 | P0 | E04 | O1 | SEALED_PENDING_PIN_CANDIDATE | - |
| `NC-WU-FOREIGN` | `WU` | P0 | 13.1 step 3 | WUM | O1 | SEALED_PENDING_PIN_CANDIDATE | - |
| `NC-E03-NOT-HELD` | `LK` | PL | PL | E03 | O1 | SEALED_PENDING_PIN_CANDIDATE | - |
| `NC-E03-REACQUIRE` | `LK` | PV | between PL and PS | E03 | O2 (CER-ABV) | DRAFT | K-3 |
| `NC-S1-Y1` | `KS` | PV | Y1 | S1 | O2 (CER-ABV; abort before Commit is unconditional) | DRAFT | K-3 |
| `NC-S1-Y1-SILENT` | `KS` | PV | Y1 | S1 | O2 (CER-ABV; abort before Commit is unconditional) | DRAFT | K-3 |
| `NC-E08-Y0` | `E8` | PV | Y0 | E08 | O2 (CER-ABV; abort before Commit is unconditional) | DRAFT | K-3 |
| `NC-E08-Y0-SILENT` | `E8` | PV | Y0 | E08 | O2 (CER-ABV; abort before Commit is unconditional) | DRAFT | K-3 |
| `NC-E08-Y2` | `E8` | PV | Y2 | E08 | O2 (CER-ABV; abort before Commit is unconditional) | DRAFT | K-3 |
| `NC-E08-Y2-SILENT` | `E8` | PV | Y2 | E08 | O2 (CER-ABV; abort before Commit is unconditional) | DRAFT | K-3 |
| `EA1-READONLY-OPENS` | `EA1` | CP | NONE | NONE | NONE (no commit in the measured interval) | DRAFT | K-3 |
| `EA2-DEFERRED-EVENTS` | `EA2` | CP | NONE | NONE | CONDITIONAL (CER-A2): O3 when no deferred event is delivered in pha... | DRAFT | K-3 |
| `OU-O1` | `OU` | SYN | NONE | NONE | O1 | SEALED_CANDIDATE | - |
| `OU-O2` | `OU` | SYN | NONE | NONE | O2 | SEALED_CANDIDATE | - |
| `OU-O6` | `OU` | SYN | NONE | NONE | O6 | SEALED_CANDIDATE | - |
| `OU-O4` | `OU` | SYN | NONE | NONE | O4 | SEALED_CANDIDATE | - |
| `OU-O5` | `OU` | SYN | NONE | NONE | O5 | SEALED_CANDIDATE | - |
| `OU-O7` | `OU` | SYN | NONE | NONE | O7 | SEALED_CANDIDATE | - |
| `OU-O3` | `OU` | SYN | NONE | NONE | O3 | SEALED_CANDIDATE | - |
| `OU-ROW1-INVALID` | `OU` | SYN | NONE | NONE | no governing outcome; PROVISIONAL_NON_GOVERNING_OUTCOME retained | SEALED_CANDIDATE | - |
| `OU-PREDICATE` | `OU` | SYN | NONE | NONE | NONE | SEALED_CANDIDATE | - |
| `AB-COMMIT-EXCEPTION-T1-A` | `OU` | SYN | NONE | NONE | O2 | SEALED_CANDIDATE | - |
| `AB-COMMIT-EXCEPTION-T1-B` | `OU` | SYN | NONE | NONE | O6 | SEALED_CANDIDATE | - |
| `PR-SOURCE-SWAP-MODIFY` | `PR-FRESH` | CP | after PR (acquisition) | NONE | O3 SUCCESS | DRAFT | K-3, K-8 |
| `PR-SOURCE-SWAP-DELETE` | `PR-FRESH` | CP | after PR (acquisition) | NONE | O3 SUCCESS | DRAFT | K-3, K-8 |
| `PR-SOURCE-SWAP-NEG-COPY` | `PR-FRESH` | PW | between the first and the second import | LIBF | O2 (CER-ABV: the first import is in the manifest; ABORT-VERIFY comp... | DRAFT | K-8 |
| `PR-TORN-READ` | `PR-FRESH` | PR | during the acquisition | LIBF | O1 (acquisition UNKNOWN => refusal before any product write) | SEALED_PENDING_PIN_CANDIDATE | - |
| `PW-CLONE-BASE` | `PW-CLONE` | CP | NONE | NONE | O3 SUCCESS | DRAFT | K-3, K-8 |
| `PW-CLONE-CASE` | `PW-CLONE` | CP | NONE | NONE | O3 SUCCESS | DRAFT | K-3, K-8 |
| `PW-CLONE-WRONG-TYPE` | `PW-CLONE` | PR | NONE | NONE | O1 | SEALED_PENDING_PIN_CANDIDATE | - |
| `PW-CLONE-NESTED` | `PW-CLONE` | CP | NONE | NONE | O3 SUCCESS | DRAFT | K-3, K-8 |
| `PW-CLONE-SYMTAB` | `PW-CLONE` | CP | NONE | NONE | O3 SUCCESS | DRAFT | K-3, K-8 |
| `PR-CLOSURE-COMPLETENESS` | `PR-REC` | NONE | NONE | NONE | NONE (qualification run on a test document; no mirror) | DRAFT | K-8 |
| `PR-REC-CLOSURE-UNKNOWN` | `PR-REC` | PR | NONE | S5CLO | O1 | SEALED_PENDING_PIN_CANDIDATE | - |
| `PR-REC-UNREADABLE` | `PR-REC` | PR | PR | S5REC | O1 | SEALED_PENDING_PIN_CANDIDATE | - |
| `PR-REC-REOBS-MISMATCH` | `PR-REC` | PW | between PR and PW | S5REC | O1 (empty manifest) | DRAFT | K-8 |
| `PW-IMPORT-PARTIAL-FAIL` | `PW` | PW | inside PW | S5MAN | CONDITIONAL (CER-ABV): O2 on a match of the record plus the complet... | DRAFT | K-8 |
| `AB-AFTER-PREPARE-W` | `AB` | PW | between PP and PS | NONE | O2 (T_M NOT_OPENED after a PREPARE-W write; ABORT-VERIFY match with... | DRAFT | K-8 |
| `AB-AFTER-MUTATION` | `AB` | PV | between PS and PV | NONE | O2 | DRAFT | K-3 |
| `NC-S5-O6` | `AB` | PV | Y1 | S5 | CONDITIONAL (CER-PERSIST): O6 when the I-14 independent post-abort ... | DRAFT | K-3 |
| `KS-SL1-POSTWRITE` | `KS` | PV | inside SL-1 | NONE | O2 (CER-ABV) | DRAFT | K-3 |
| `KS-SL1-TX-MISMATCH` | `KS` | PV | SL-1 entry | NONE | O2 (CER-ABV) | DRAFT | K-3 |
| `KS-SL4-NEG-DET` | `KS` | PV | SL-4 entry | NONE | O2 (CER-ABV) | DRAFT | K-3 |
| `E4-LEARN-R1-LM1` | `E4` | CP | NONE | NONE | O3 SUCCESS | SEALED_PENDING_PIN_CANDIDATE | - |
| `LK-LM1-PROPS` | `LK` | CP | NONE | NONE | O3 SUCCESS | SEALED_PENDING_PIN_CANDIDATE | - |
| `LK-LM1-CONFLICT` | `LK` | CP | between PL and V1 | NONE | O3 SUCCESS (the probe targets a non-protected object; its effect is... | SEALED_PENDING_PIN_CANDIDATE | - |
| `E4-LEARN-R1-LM2` | `E4` | CP | NONE | NONE | O3 SUCCESS | SEALED_PENDING_PIN_CANDIDATE | - |
| `LK-LM2-PROPS` | `LK` | CP | NONE | NONE | O3 SUCCESS | SEALED_PENDING_PIN_CANDIDATE | - |
| `LK-LM2-CONFLICT` | `LK` | CP | between PL and V1 | NONE | O3 SUCCESS (the probe targets a non-protected object; its effect is... | SEALED_PENDING_PIN_CANDIDATE | - |
| `E4-VAL-R1` | `E4` | CP | NONE | NONE | O3 SUCCESS | DRAFT | K-3 |
| `EV-L-OC-TM` | `E12-L` | LEARN | NONE | NONE | NONE (learning run) | SEALED_PENDING_PIN_CANDIDATE | - |
| `EV-L-OC-BT-OPEN` | `E12-L` | LEARN | NONE | NONE | NONE (learning run) | SEALED_PENDING_PIN_CANDIDATE | - |
| `EV-L-OC-BTR-NESTED` | `E12-L` | LEARN | NONE | NONE | NONE (learning run) | SEALED_PENDING_PIN_CANDIDATE | - |
| `EV-L-OC-BTR-VIEW` | `E12-L` | LEARN | NONE | NONE | NONE (learning run) | SEALED_PENDING_PIN_CANDIDATE | - |
| `EV-L-OC-REF-PIECE` | `E12-L` | LEARN | NONE | NONE | NONE (learning run) | SEALED_PENDING_PIN_CANDIDATE | - |
| `EV-L-OC-DYN` | `E12-L` | LEARN | NONE | NONE | NONE (learning run) | SEALED_PENDING_PIN_CANDIDATE | - |
| `EV-L-OC-REF-ARRAY` | `E12-L` | LEARN | NONE | NONE | NONE (learning run) | SEALED_PENDING_PIN_CANDIDATE | - |
| `EV-L-OC-LAYER` | `E12-L` | LEARN | NONE | NONE | NONE (learning run) | SEALED_PENDING_PIN_CANDIDATE | - |
| `EV-L-OC-LAYER-PRESENT` | `E12-L` | LEARN | NONE | NONE | NONE (learning run) | SEALED_PENDING_PIN_CANDIDATE | - |
| `EV-L-OC-TEXT` | `E12-L` | LEARN | NONE | NONE | NONE (learning run) | SEALED_PENDING_PIN_CANDIDATE | - |
| `EV-L-OC-DIM` | `E12-L` | LEARN | NONE | NONE | NONE (learning run) | SEALED_PENDING_PIN_CANDIDATE | - |
| `EV-L-OC-ENVELOPE` | `E12-L` | LEARN | NONE | NONE | NONE (learning run) | SEALED_PENDING_PIN_CANDIDATE | - |
| `EV-L-OC-IMPORT` | `E12-L` | LEARN | NONE | NONE | NONE (learning run) | DRAFT | K-8 |
| `EV-L-OC-REF-TOP` | `E12-L` | LEARN | NONE | NONE | NONE (learning run) | SEALED_PENDING_PIN_CANDIDATE | - |
| `EV-L-OC-VERIFY-READ` | `E12-L` | LEARN | NONE | NONE | NONE (learning run) | SEALED_PENDING_PIN_CANDIDATE | - |
| `EV-L-COMPOSED` | `E12-L` | LEARN | NONE | NONE | NONE (learning run) | SEALED_PENDING_PIN_CANDIDATE | - |
| `EV-V-FX-2F` | `E12-V` | CP | NONE | NONE | O3 SUCCESS | DRAFT | K-3 |
| `EV-V-FX-4F` | `E12-V` | CP | NONE | NONE | O3 SUCCESS | DRAFT | K-3 |
| `EV-V-FX-BIG` | `E12-V` | CP | NONE | NONE | O3 SUCCESS | DRAFT | K-3 |
| `WR-A-X0` | `SC-A` | CP | X0 | S2 | O4 (semantic comparison detects a persistent change) | DRAFT | K-3 |
| `WR-A-X1` | `SC-A` | CP | X1 | S2 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-A-X2` | `SC-A` | CP | X2 | S3 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-A-X3` | `SC-A` | CP | X3 | S3 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-A-X4` | `SC-A` | CP | X4 | S3 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-A-X5` | `SC-B` | CP | X5 | NONE | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 |
| `WR-A-X6` | `SC-B` | CP | X6 | NONE | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 |
| `WR-A-X7` | `SC-B` | CP | X7 | NONE | O3 with RESIDUAL_MISSED_CHANGE (X7 is outside W-scan and phase C); ... | DRAFT | K-3 |
| `WR-A-X8` | `SC-B` | CP | X8 | NONE | O3; outside the guarantee (W3); recorded for the disclosure only | DRAFT | K-3 |
| `WR-D-X0` | `SC-A` | CP | X0 | S2 | O4 (semantic comparison detects a persistent change) | DRAFT | K-3 |
| `WR-D-X1` | `SC-A` | CP | X1 | S2 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-D-X2` | `SC-A` | CP | X2 | S3 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-D-X3` | `SC-A` | CP | X3 | S3 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-D-X4` | `SC-A` | CP | X4 | S3 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-D-X5` | `SC-B` | CP | X5 | NONE | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 |
| `WR-D-X6` | `SC-B` | CP | X6 | NONE | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 |
| `WR-D-X7` | `SC-B` | CP | X7 | NONE | O3 with RESIDUAL_MISSED_CHANGE (X7 is outside W-scan and phase C); ... | DRAFT | K-3 |
| `WR-D-X8` | `SC-B` | CP | X8 | NONE | O3; outside the guarantee (W3); recorded for the disclosure only | DRAFT | K-3 |
| `WR-E-X0` | `SC-A` | CP | X0 | S2 | O4 (semantic comparison detects a persistent change) | DRAFT | K-3 |
| `WR-E-X1` | `SC-A` | CP | X1 | S2 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-E-X2` | `SC-A` | CP | X2 | S3 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-E-X3` | `SC-A` | CP | X3 | S3 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-E-X4` | `SC-A` | CP | X4 | S3 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-E-X5` | `SC-B` | CP | X5 | NONE | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 |
| `WR-E-X6` | `SC-B` | CP | X6 | NONE | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 |
| `WR-E-X7` | `SC-B` | CP | X7 | NONE | O3 with RESIDUAL_MISSED_CHANGE (X7 is outside W-scan and phase C); ... | DRAFT | K-3 |
| `WR-E-X8` | `SC-B` | CP | X8 | NONE | O3; outside the guarantee (W3); recorded for the disclosure only | DRAFT | K-3 |
| `WR-F-X0` | `SC-A` | CP | X0 | S2 | O4 (semantic comparison detects a persistent change) | DRAFT | K-3 |
| `WR-F-X1` | `SC-A` | CP | X1 | S2 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-F-X2` | `SC-A` | CP | X2 | S3 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-F-X3` | `SC-A` | CP | X3 | S3 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-F-X4` | `SC-A` | CP | X4 | S3 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-F-X5` | `SC-A` | CP | X5 | E08 | O7 (E-08 FAIL at PC: the read-set change persists at PC; 17.2 row 7... | DRAFT | K-3 |
| `WR-F-X6` | `SC-A` | CP | X6 | E08 | O7 (E-08 FAIL at PC: the read-set change persists at PC; 17.2 row 7... | DRAFT | K-3 |
| `WR-F-X7` | `SC-A` | CP | X7 | E08 | O7 (E-08 FAIL at PC; the change persists at PC) (AR2-14); POST_V1_T... | DRAFT | K-3 |
| `WR-F-X8` | `SC-B` | CP | X8 | NONE | O3; outside the guarantee (W3); recorded for the disclosure only | DRAFT | K-3 |
| `WR-G-X0` | `SC-A` | CP | X0 | S2 | O4 (semantic comparison detects a persistent change) | DRAFT | K-3 |
| `WR-G-X1` | `SC-A` | CP | X1 | S2 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-G-X2` | `SC-A` | CP | X2 | S3 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-G-X3` | `SC-A` | CP | X3 | S3 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-G-X4` | `SC-A` | CP | X4 | S3 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-G-X5` | `SC-A` | CP | X5 | E08 | O7 (E-08 FAIL at PC: the read-set change persists at PC; 17.2 row 7... | DRAFT | K-3 |
| `WR-G-X6` | `SC-A` | CP | X6 | E08 | O7 (E-08 FAIL at PC: the read-set change persists at PC; 17.2 row 7... | DRAFT | K-3 |
| `WR-G-X7` | `SC-A` | CP | X7 | E08 | O7 (E-08 FAIL at PC; the change persists at PC) (AR2-14); POST_V1_T... | DRAFT | K-3 |
| `WR-G-X8` | `SC-B` | CP | X8 | NONE | O3; outside the guarantee (W3); recorded for the disclosure only | DRAFT | K-3 |
| `WR-B-X0` | `SC-B` | CP | X0 | NONE | O3 with RESIDUAL_MISSED_CHANGE (ABA restored inside the commit boun... | DRAFT | K-3 |
| `WR-B-X3` | `SC-B` | CP | X3 | NONE | CONDITIONAL (CER-WIT): O7 when the witness records a delivered phas... | DRAFT | K-3 |
| `WR-B-X7` | `SC-B` | CP | X7 | NONE | O3 with RESIDUAL_MISSED_CHANGE (ABA between V1 and PC; nothing pers... | DRAFT | K-3 |
| `WR-C-X5` | `SC-B` | CP | X5 | NONE | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 |
| `WR-C-X6` | `SC-B` | CP | X6 | NONE | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 |
| `WR-C-X7` | `SC-B` | CP | X7 | NONE | O3 with RESIDUAL_MISSED_CHANGE (X7 is outside W-scan and phase C); ... | DRAFT | K-3 |
| `WR-SA-X1` | `SC-A` | CP | X1 | S2 | O4 (semantic comparison detects; no event delivered) | DRAFT | K-3 |
| `WR-SA-X2` | `SC-A` | CP | X2 | S3 | O4 (semantic comparison detects; no event delivered) | DRAFT | K-3 |
| `WR-SA-X3` | `SC-A` | CP | X3 | S3 | O4 (semantic comparison detects; no event delivered) | DRAFT | K-3 |
| `WR-SA-X4` | `SC-A` | CP | X4 | S3 | O4 (semantic comparison detects; no event delivered) | DRAFT | K-3 |
| `WR-SB-X3` | `SC-B` | CP | X3 | NONE | O3 with RESIDUAL_MISSED_CHANGE (silent writer: neither channel) | DRAFT | K-3 |
| `WR-SC-X5` | `SC-B` | CP | X5 | NONE | O3 with RESIDUAL_MISSED_CHANGE (silent writer: neither channel) | DRAFT | K-3 |
| `WR-SC-X6` | `SC-B` | CP | X6 | NONE | O3 with RESIDUAL_MISSED_CHANGE (silent writer: neither channel) | DRAFT | K-3 |
| `WR-A-X1-EV` | `SC-A` | CP | X1 | S2 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-A-X2-EV` | `SC-A` | CP | X2 | S3 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-A-X3-EV` | `SC-A` | CP | X3 | S3 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-A-X4-EV` | `SC-A` | CP | X4 | S3 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-A-X5-EV` | `SC-B` | CP | X5 | NONE | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 |
| `WR-A-X6-EV` | `SC-B` | CP | X6 | NONE | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 |
| `WR-D-X1-EV` | `SC-A` | CP | X1 | S2 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-D-X2-EV` | `SC-A` | CP | X2 | S3 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-D-X3-EV` | `SC-A` | CP | X3 | S3 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-D-X4-EV` | `SC-A` | CP | X4 | S3 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-D-X5-EV` | `SC-B` | CP | X5 | NONE | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 |
| `WR-D-X6-EV` | `SC-B` | CP | X6 | NONE | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 |
| `WR-E-X1-EV` | `SC-A` | CP | X1 | S2 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-E-X2-EV` | `SC-A` | CP | X2 | S3 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-E-X3-EV` | `SC-A` | CP | X3 | S3 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-E-X4-EV` | `SC-A` | CP | X4 | S3 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-E-X5-EV` | `SC-B` | CP | X5 | NONE | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 |
| `WR-E-X6-EV` | `SC-B` | CP | X6 | NONE | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 |
| `WR-F-X1-EV` | `SC-A` | CP | X1 | S2 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-F-X2-EV` | `SC-A` | CP | X2 | S3 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-F-X3-EV` | `SC-A` | CP | X3 | S3 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-F-X4-EV` | `SC-A` | CP | X4 | S3 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-F-X5-EV` | `SC-A` | CP | X5 | E08 | O7 (E-08 FAIL at PC: the read-set change persists at PC; 17.2 row 7... | DRAFT | K-3 |
| `WR-F-X6-EV` | `SC-A` | CP | X6 | E08 | O7 (E-08 FAIL at PC: the read-set change persists at PC; 17.2 row 7... | DRAFT | K-3 |
| `WR-G-X1-EV` | `SC-A` | CP | X1 | S2 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-G-X2-EV` | `SC-A` | CP | X2 | S3 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-G-X3-EV` | `SC-A` | CP | X3 | S3 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-G-X4-EV` | `SC-A` | CP | X4 | S3 | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 |
| `WR-G-X5-EV` | `SC-A` | CP | X5 | E08 | O7 (E-08 FAIL at PC: the read-set change persists at PC; 17.2 row 7... | DRAFT | K-3 |
| `WR-G-X6-EV` | `SC-A` | CP | X6 | E08 | O7 (E-08 FAIL at PC: the read-set change persists at PC; 17.2 row 7... | DRAFT | K-3 |
| `WR-B-X3-EV` | `SC-B` | CP | X3 | NONE | CONDITIONAL (CER-WIT): O7 when the witness records a delivered phas... | DRAFT | K-3 |
| `WR-C-X5-EV` | `SC-B` | CP | X5 | NONE | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 |
| `WR-C-X6-EV` | `SC-B` | CP | X6 | NONE | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 |
| `WU-BOUNDARY` | `WU` | NONE | NONE | NONE | NONE (no product mutation) | SEALED_PENDING_PIN_CANDIDATE | - |
| `U-1` | `U-1..U-8` | PW | NONE | NONE | preceding run: O2 (CER-ABV); then the UNDO sequence | DRAFT | K-8 |
| `U-2` | `U-1..U-8` | CP | NONE | NONE | preceding run: O3 SUCCESS; then the UNDO sequence | DRAFT | K-3 |
| `U-3` | `U-1..U-8` | CP | NONE | S3 | preceding run: O4; then the UNDO sequence | DRAFT | K-3 |
| `U-4` | `U-1..U-8` | CP | NONE | S3 | preceding run: O5; then the UNDO sequence | DRAFT | K-3 |
| `U-5` | `U-1..U-8` | CP | NONE | E11M | preceding run: O7; then the UNDO sequence | DRAFT | K-3 |
| `U-7` | `U-1..U-8` | CP | NONE | NONE | preceding run: O3 SUCCESS; then the UNDO sequence | DRAFT | K-3, K-8 |
| `U-8` | `U-1..U-8` | CP | NONE | NONE | preceding run: O3 SUCCESS; then the UNDO sequence | DRAFT | K-3 |
| `U-6` | `U-1..U-8` | SYN | NONE | NONE | O6 (T1 synthetic) | SEALED_CANDIDATE | - |
| `U-6b` | `U-6b` | PV | NONE | NONE | the host exception (origin recorded; no product outcome is declared) | DRAFT | K-3 |
| `S1-SAVE-REOPEN` | `S1` | CP | NONE | NONE | preceding run: O3 SUCCESS | DRAFT | K-3 |
| `PP-POSTCP` | `PP` | CP | NONE | NONE | preceding run: O3 SUCCESS | DRAFT | K-3 |
| `CL-CLEANUP-CHECK` | `CL` | CP | NONE | NONE | preceding run: O3 SUCCESS | DRAFT | K-3 |

### 5.2 Per-scenario warm-up dry runs (AR2-42)

V2.1 13.1 defines `WARMUP_COVERAGE_CRITERION` **per scenario**. Every governing host scenario `X` (reach `P0` or later) has one dry-run scenario `WU-DRY-X` with the same plan, fixture specification and injections, `MODE = DRY_RUN`, `RUN_GOVERNANCE = NON_GOVERNING_BY_DESIGN`, the expectation "no assembly-load event inside the governed window" and the repetition rule `DRY`. There are **145** of them (the JSON lists each one); no per-family reduction is used.

## 6. Rulings applied

### 6.1 G-1: the Y injection coordinates (harness level; no contract amendment)

| Coordinate | Point |
|---|---|
| `Y0` | after the PS observation and before the first seam write |
| `Y1` | after the last seam write and before the staged reads |
| `Y2` | after the sealing of `VERIFY_ELEMENT_LIST` and before the PV evaluation |

The injected write uses `T_M`. Its phase-A event is **conditional** on delivery, resolved by the **I-14 delivery witness** (AR2-12, AR2-18), never by the E-12 log itself. The target-control result and the abort are unconditional; O2 or O6 follows `CER-ABV`. Isolation of the target needs the silent variant `Wr-S`, whose availability is **measured** at every coordinate it uses (`Q-I14-SILENT` at Y0, Y1, Y2 and X1..X6).

### 6.2 G-2: tuple identity versus product refusal

| Situation | Result |
|---|---|
| **governed tuple identity mismatch** (any field of V2.1 2.1 differs from the baseline, whatever its origin) | the run is **INVALID**; never a product outcome |
| **session / runtime control failure** (E-02..E-07, E-09, E-10, E-11M, and a subscribe failure at P0 of E-11M or E-12, AR2-30) in a valid run | the product **refuses** (O1 at entry; abort at PV) |
| a subscription **lost** after it was established | the run is **INVALID** (V2.1 21.1); its detection is qualified by `Q-I09-LOSS` and `Q-I10-LOSS` |
| **product plan outside the envelope** | plan-time refusal (O1); outside the CT-21D baseline unless a scenario governs it |
| E-01 or E-13 mismatch **after** the control plane verified the tuple | an inconsistent tuple: **INVALID** (`NC-E01` and `NC-E13` stay withdrawn; `Q-I02` and `Q-I04` qualify the detection) |
| the library swap of `SOURCE_SWAP_CONTROL` | **not** a tuple mismatch: the tuple's library field is sampled once, before the acquisition; the swap is the control's designed stimulus; the original is restored in the cleanup (AR2-31) |

### 6.3 G-3 and G-4

`E6-C4` remains `EXPLORATORY_NON_GOVERNING` (G-3). Every scenario with a plan names its fixture; the fixture **instance** is a pin slot (AR2-17); `FX-IMP` is `SPECIFICATION_OPEN` (K-8) and the scenarios that need it carry `BLOCKED_BY = K-8` (G-4).

### 6.4 Phase-2 corrections (decisions section 211)

- **X0 (AR2-13):** E-12 is not adjudicated at X0 (`NG-19b`); `E12C` is not exercised; O4 comes from the semantic comparison; "precedence over O7" is removed.
- **Read-set writers (AR2-14):** Wr-G and Wr-F at X5, X6 and X7 give **E-08 FAIL at PC and O7**, unconditionally; at X1..X4 O4 takes precedence and E-08 at PC is a collateral FAIL.
- **Wr-F (AR2-15):** the creation of the consumed `RACKCAD_PROJECT` entry (the fixtures have none, B4).
- **Witness (AR2-12):** every "delivered" condition is resolved by the I-14 delivery witness; `NC-E12C` exists (X6, CER-WIT); `WR-B-X7` and `WR-C-X7` exist.
- **E-03 and E-04 learning (AR2-11):** `E4-LEARN-R1-LM1/LM2` and `LK-*` are `NON_GOVERNING_BY_DESIGN`, E-03 and E-04 `RECORDED_NOT_ENFORCED`; `NC-E03-NOT-HELD` (O1) and `NC-E03-REACQUIRE` (abort, CER-ABV) exist; the E-04 pin comes from learning under the selected mode with an offline membership post-check in `E4-VAL-R1`.
- **UNDO (AR2-20):** exact preceding outcomes; U-4 and U-5 induced failures are concrete.
- **KR (AR2-21):** `KS-KR-RECORD` is withdrawn; the clean runs record E-14 and E-15 and serve `KR` with `NO_VERDICT`.
- **Cleanup (AR2-22):** `CL-CLEANUP-INCOMPLETE` is withdrawn; `Q-I16-CLEANUP-NEG` qualifies the detection.
- **Copy swap (AR2-23) and imports (AR2-24):** the negative copy swap sits between two imports (O2 by row 3); source swap and clone scenarios use `FX-IMP`.
- **Collaterals and default (AR2-25):** unlisted evaluated controls PASS; collaterals declared (E-10 with canary loads; E-11M in `NC-E10`; E-06 in `KS-SL1-TX-MISMATCH`); `NC-E12A` is the XData write at Y1; `NC-E10` is pinned to 13.1 step 5.
- **E12-L (AR2-27):** one micro-scenario per `OC-*` class of BA-06 V3, with `OC-LAYER-PRESENT` and `OC-VERIFY-READ`.
- **PARAM-03 (AR2-28):** the clean runs use the repetition rule `CLEAN`.
- **Tuples (AR2-29):** the synthetic tuple binds the contract composite and the catalog hash; `Q-FPSPEC-VECTORS` has its own tuple.
- **Subscription at P0 (AR2-30):** `NC-E12D` is a runtime refusal (O1).

## 7. What prevents sealing the catalog

| Id | Blocking item | Gate |
|---|---|---|
| C-1 | fixture **instances** (pin slots `FIXTURE_INSTANCE@*`) and the template variants marked `FIXTURE_SPEC_PENDING` | host (not authorized) and BA-08 V3 |
| C-2 | `FX-IMP` construction (K-8) | fixture-instance gate (AR2-41) |
| C-3 | the repetition values `PARAM-01`, `PARAM-02`, `PARAM-03` (Owner decisions Q-O1..Q-O4) and the Coordinator values `EVIDENCE_REPETITION`, `WU_DRY_RUNS` | Owner and Coordinator decisions (BA-10 V3) |
| C-4 | the designated route R-1 and its delivery | Coordinator ratification; `Q-ROUTE-R1` measures it |
| C-5 | the synthetic log fixtures of the `OU-*` scenarios | implementation of the evaluator (implementation gate) |
| C-6 | `TOL_SCALE` (K-3), without which no scenario that evaluates S-1..S-3 can be declared | separate decision, test and record (phase-1 ruling; AR2-35) |
| C-7 | `HOST_DEFAULT_MAP` (pin) and the Architect rulings of BA-08 V3 section 16 | BA-08 V3 |
| C-8 | review and agreement of every expected result, then the seal (`SEALED_AT`) scenario by scenario | baseline review |

```text
CATALOG = DRAFT V3 (370 scenarios; 0 sealed)     NB-2 = REMAINS_BLOCKER_FOR_AUTHORITY_BASELINE
CT21D_AUTHORITY_BASELINE_READY = FALSE
```
