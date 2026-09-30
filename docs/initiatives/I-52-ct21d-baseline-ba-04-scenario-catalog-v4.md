# I-52 — CT-21D Baseline Artifact BA-04: Scenario Catalog (DRAFT V4)

> **BASELINE ARTIFACT BA-04 V4 — DRAFT, NOT SEALED. Design / documentation only. Nothing here is executed or authorized.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-SCENARIO-CATALOG
> ARTIFACT_VERSION          = 4-DRAFT (supersedes 3-DRAFT, blobs 3e47ba6dd0778302dd6152c188b48ae19bb2976c (json) and e364be9554484118ad1180fa940877d951cbdf53 (md), which stay as history)
> STATUS AUTHORITY          = the BA-11 registry entry of this artifact is the only authority for its status and hashes;
>                             this header and the per-scenario statuses are frozen, non-authoritative text of the candidate bytes
> AUTHORITY_CONTRACT        = CT-21D V2.1 + V2.2 + V2.3 + V2.4 (draft revision 3) + V2.5 (draft revision 1); V2.4 and V2.5 pending
>                             Coordinator textual verification
> DELTA RULING APPLIED      = decisions section 214 (AR3-01, AR3-02, AR3-03, AR3-04, AR3-05, AR3-06, AR3-07, AR3-08, AR3-09, AR3-10, AR3-11,
>                             AR3-15, AR3-17, AR3-23, AR3-24, AR3-25, AR3-26, AR3-30); section 211 (AR2-08..AR2-31) where section 214 does not
>                             change it; correction pass of round V4 (pending rulings carried as AQ-V4-01, AQ-V4-03 and K-6 in BLOCKED_BY)
> DEPENDS_ON                = BA-01, BA-02a, BA-03, BA-05, BA-06, BA-08, BA-09, BA-10 (registry entries; BA-11 V4)
> PREREQUISITES             = K-3 TOL_SCALE decision, test and record; K-8 FX-IMP construction;
>                             K-6 Architect ruling Q-HDM-1 of BA-08 V4 (AQ-V4-07); the Architect's rulings AQ-V4-01..AQ-V4-06 and AQ-V4-14;
>                             all of them before the candidate (the declarations enter the bytes; they are the seal blockers of section 7.1,
>                             AR3-26; BA-11 V4 section 3); the Architect's delta ruling on this V4 (before the candidate); pin values (before the
>                             first governing run, not baseline prerequisites: section 7.2)
> BLOCKER                   = NB-2 (REMAINS_BLOCKER_FOR_AUTHORITY_BASELINE)
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```
>
> The machine-readable catalog is [the JSON file](I-52-ct21d-baseline-ba-04-scenario-catalog-v4.json) and carries every field of every scenario; the two files form the artifact. The mechanical checks of this version are `docs/automation/evidence/I-52-ct21d-ba04-v4-checks.py` and their output.

## 1. Schema (normative)

Every scenario carries the fields of V2.1 26.1 (with V2.2 PA-3 `EXPECTED_PROCEDURAL_RESULT`) and these fields:

| Field | Content |
|---|---|
| `PURPOSE` | why the scenario exists (V2.1 26.1 `GROUP`, `PURPOSE`) |
| `RUN_GOVERNANCE` | `GOVERNING`, `NON_GOVERNING_BY_DESIGN` (dry runs; the characterization runs of AR2-11 and AR3-11) or `EXPLORATORY_NON_GOVERNING` |
| `BUILD_UNDER_TEST` | `PRODUCT_BUILD`, `CHARACTERIZATION_BUILD` (section 1.1) or `NONE` (a synthetic scenario with no product process, REACH `SYN`) |
| `RECORDED_NOT_ENFORCED` | the controls and phases that are recorded and do not govern the flow; empty, or exactly the set of the AR3-01 mode (section 1.1) |
| `REACH` | the furthest checkpoint the run is designed to reach: `NONE` (no product run), `SYN` (synthetic log), `LEARN`, `P0`, `PL`, `PR`, `PW`, `PS` (T_M opened; the caller aborts at a seam failure before PV), `PV`, `CP` |
| `INJECTIONS` | the writers or faults (writer, variant, position), or none (V2.1 26.1); writer kinds in section 1.2 |
| `DESIGNED_STIMULUS` | `NONE`, or a short description of a designed stimulus: source swap, clone variant, induced seam failure, stop after PREPARE-W, residual scan cell with no expected control `FAIL`, induced host exception path, dry-run replay. A designed stimulus carries `DELIBERATE_VIOLATION_FLAG = NO` and no target (AR3-05) |
| `DELIBERATE_VIOLATION_FLAG`, `TARGET_NEGATIVE_CONTROL` | `YES` only when the scenario deliberately violates a control; the target is that control (a key of BA-02a), or `INSTRUMENT:I-nn` for the negative control of a qualification; `NO` with target `NONE` otherwise (V2.1 26.1; AR3-05). A cell whose detection is conditional on E-12 phase C has target `E12C` |
| `EXPECTED_EVENTS` | the event class expected per E-12 phase (V2.1 26.1) |
| `EXPECTED_DETECTION_CHANNEL` | the detection channel (V2.1 26.1): `SEMANTIC_COMPARISON`, `E-08_AT_PC`, `E-12`, `BOTH`, `NEITHER`, the target control, or `N/A` |
| `EXPECTED_LOG_RECORD_SET` | a log profile of section 4 (record types and counts) |
| `CONTROLS_EXERCISED` | the registry controls (keys of BA-02a) whose results the scenario expects, or whose recorded values it characterizes (E-04 in `E4-LEARN-R1-*`, E-03 in `LK-*`). Every other registry key of `EXPECTED_CONTROL_RESULTS` carries `NOT_DECLARED` or, for a control of the scenario's own `RECORDED_NOT_ENFORCED` set, `RECORDED_NOT_ENFORCED` (checked mechanically) |
| `CONTROL_CLASSES_EXERCISED` | the registry classes of those controls (`A`, `B`), plus `NOT_A_CONTROL` when the scenario exercises no control, declares an evidence group or has a contract-assigned group (V2.1 26.1 and 31 check 3); derived |
| `EVIDENCE_GROUPS_DECLARED` | the groups of kind `EVIDENCE` (V2.2 PA-1.3) the scenario serves |
| `GROUPS_SERVED` | derived by the normative rule of BA-02b V4 section 1: the union of `AFFECTED_GROUPS` of the controls in `CONTROLS_EXERCISED`, plus `EVIDENCE_GROUPS_DECLARED`; nothing else (AR2-08) |
| `CONTRACT_ASSIGNED_GROUPS` | a list of `{group, clause}`: a `CONTROL` group that the contract assigns to a `NOT_A_CONTROL` item or a qualification control served by the scenario (V2.1 10.8: X7 cells to SC-B; V2.1 10.6 and 21.4: residual cells to SC-B; V2.2 PA-12: closure completeness to PR-REC), or to the scenario class of a scenario whose `GROUPS_SERVED` does not reach it (V2.1 26.3 and 31: the `KS-*` class to SC-A and KS, recorded pending AQ-V4-03). **Evidence only, never PASS**; kept separate from `GROUPS_SERVED`, never counted as PASS-bearing and never used by `N_strict` (AR3-06) |
| `PIN_SLOTS` | named slots filled by pin records outside the catalog (section 2.1) |
| `BLOCKED_BY` | open items that prevent declaring the expectation; the vocabulary (the JSON `BLOCKED_BY_VOCABULARY` holds the same entries): `K-3` = TOL_SCALE has no authority (BA-08 V4 K-3): no expectation that adjudicates S-1..S-3 can be declared; `K-8` = FX-IMP and its variants are SPECIFICATION_OPEN (BA-08 V4 K-8); `K-6 (AQ-V4-07, Q-HDM-1)` = the Architect ruling Q-HDM-1 of BA-08 V4 (K-6; AQ-V4-07): the source of the unassigned fields of a created SL-Y layer; `AQ-V4-01` = the Architect ruling on the RECORDED_NOT_ENFORCED set and build of the governing learning runs EV-L-* and qualification vehicles Q-I14-* (AR3-01 rules the mode only for E4-LEARN-R1-*, LK-* and, by AR3-11 and AR3-02, HDM-CHAR-* and WU-DRY-*); `AQ-V4-03` = the Architect ruling on the contribution of the KS-* seam-failure scenarios to group KS; `E6-C4` = the exploratory question of NC-S5-O6 (AR3-04); not read as a catalog seal blocker (AQ-V4-04) |
| `CONDITIONAL_EXPECTATION_RULE` | the declared rules of section 3 that the scenario uses, or `NONE` |
| `SEALED_AT` | the SHA-256 of the canonical serialization of the scenario's expectation fields (section 1.3; AR3-25) |
| `NOTE` | informative text |

### 1.1 Characterization build and the AR3-01 mode

The **characterization build** is a build of the product at the exact source SHA with harness-only switches and injection points, disabled unless a scenario names them (like the I-14 writer builds of V2.1 4.3 and 10.6). It must differ from the product build only by those switches and their declared harness assemblies (an implementation requirement). Its identity is a tuple field of every run that uses it: `CHARACTERIZATION_BUILD` = build receipt and DLL SHA-256 (tuple `CHARACTERIZATION_TUPLE`, section 4).

**The AR3-01 mode** (decisions section 214, AR3-01): `BUILD_UNDER_TEST = CHARACTERIZATION_BUILD` and `RECORDED_NOT_ENFORCED = ["E-03", "E-04", "E-12 phase A", "E-12 phase C", "S-1", "S-2", "S-3"]`. These controls and phases are recorded and do not govern the flow, so the run reaches PV, `Commit()`, V1 and PC wherever its path does. The product outcome is `PROVISIONAL_NON_GOVERNING` (never O1..O7). The run has no dependency on EVM, `HOST_DEFAULT_MAP` or K-3. Every other control stays fail-closed (V2.1 3.2).

**Where the mode applies by ruling.** Only to `E4-LEARN-R1-*` and `LK-*` (AR3-01), `HDM-CHAR-*` (AR3-11) and every `WU-DRY-*` (AR3-02): runs that are non-governing by design (V2.1 21.1). **170** scenarios.

**Where it is a proposal (AQ-V4-01).** This catalog also writes the mode into the governing learning micro-scenarios `EV-L-*` and the governing qualification vehicles `Q-I14-*` (BA04V3-17). AR3-01 does not rule that extension. Their `RECORDED_NOT_ENFORCED`, `BUILD_UNDER_TEST` and outcome fields are therefore a labelled **PROPOSAL**: each of these **21** scenarios is `DRAFT` with `BLOCKED_BY` AQ-V4-01 (section 7.1, C-3a). The **4** dry runs of `Q-I14-*` are in the mode by ruling (AR3-02), but they exist only while their sources are governing, so they carry the same blocker (section 5.2). For `EV-L-*`, one part is contract-grounded and is stated as such: V2.1 27.2 (learning records the raw E-12 events; no EVM exists, so E-12 phases A and C are recorded, not enforced). The set this catalog states for `EV-L-*` is: **E-12 phase A and E-12 phase C** (contract-grounded, V2.1 27.2; the same two phases as BA-06 V4 section 9) and **E-03, E-04, S-1, S-2, S-3** (proposal, pending AQ-V4-01), all on the `CHARACTERIZATION_BUILD` (proposal; BA-06 V4 CQ-08). For `Q-I14-*`, the whole set, the build and the outcome are the proposal.

Scenarios with a non-empty `RECORDED_NOT_ENFORCED`: **191** (170 by ruling, 21 by proposal).

### 1.2 Injection writer kinds and the build under test

A scenario uses the `CHARACTERIZATION_BUILD` when it is in the AR3-01 mode or when one of its injections needs a switch or an injection point compiled into the build; otherwise it uses the `PRODUCT_BUILD`. A synthetic scenario has `BUILD_UNDER_TEST = NONE`.

| Writer kind | Meaning |
|---|---|
| `Wr-A..Wr-G, Wr-S` | the I-14 adversarial writers of V2.1 10.6 at an X or Y position (injection point of the characterization build) (build switch) |
| `Wr-OCT` | a write through an OpenCloseTransaction at a Y position (NC-S5-O6, AR3-04) (build switch) |
| `XDATA_WRITE` | an XData write of a registered test application through T_M at a Y position (NC-E12A) (build switch) |
| `HARNESS_FAULT` | a harness-only fault switch in the product or instrument path (a stop, a suppressed lock, a forced UNKNOWN read, a failing import, a failing seam, an ended transaction, a refused subscription, a foreign warm-up load) (build switch) |
| `CANARY_LOAD` | a canary assembly load at a declared point of the run (build switch) |
| `SOURCE_SWAP, COPY_MODIFY, SOURCE_MODIFY_DURING_ACQUISITION` | a control-plane change of the library file or of the private copy at a pause point of the characterization build (build switch) |
| `CLONE_MODE_SWITCH` | a switch that forces a cloning mode outside the allow-list (NC-CLONE-REPLACE, AR3-07) (build switch) |
| `CONFLICT_PROBE` | an adversarial probe from another execution context (LK-*-CONFLICT) (build switch) |
| `CONTROL_PLANE_SETUP` | the control plane prepares the session before P0 (a second document, a persistent mode, an open transaction, an overrule registration, a non-designated route); no switch in the build (no build switch) |
| `FIXTURE_VARIANT` | the scratch template is a declared variant of BA-08 V4 section 12; no switch in the build (no build switch) |
| `LIBRARY_VARIANT` | the library under test is a declared variant library of BA-08 V4 section 12; no switch in the build (no build switch) |

### 1.3 `SEALED_AT` (AR3-25)

`SEALED_AT(s)` = SHA-256 of the canonical serialization of the expectation fields of scenario `s`. It is computed by the generator and written in the JSON **before** the candidate ruling of BA-04, so it lies inside the candidate bytes and meets V2.1 26.1 ("recorded before the first governing run") with no self-reference. The catalog is sealed at artifact level only; no scenario is sealed one by one.

| Item | Rule |
|---|---|
| Fields (in this set, nothing else) | `SCENARIO_ID`, `SCENARIO_VERSION`, `MODE`, `RUN_GOVERNANCE`, `BUILD_UNDER_TEST`, `RECORDED_NOT_ENFORCED`, `TUPLE_REQUIREMENTS`, `PLAN_OR_INPUT`, `FIXTURE`, `REACH`, `INJECTIONS`, `INJECTION_COORDINATE`, `DESIGNED_STIMULUS`, `DELIBERATE_VIOLATION_FLAG`, `TARGET_NEGATIVE_CONTROL`, `CONTROLS_EXERCISED`, `PIN_SLOTS`, `EVM_VERSION`, `EXPECTED_PRODUCT_OUTCOME`, `EXPECTED_CONTROL_RESULTS`, `EXPECTED_INSTRUMENT_STATES`, `EXPECTED_GROUP_VERDICT`, `EXPECTED_EVENTS`, `EXPECTED_DETECTION_CHANNEL`, `EXPECTED_LOG_RECORD_SET`, `EXPECTED_EVIDENCE_SET`, `EXPECTED_CLEANUP`, `EXPECTED_PROCEDURAL_RESULT`, `CONDITIONAL_EXPECTATION_RULE`, `AUTHORITY_SOURCE_FOR_EXPECTED_RESULT` |
| Serialization | a JSON object holding exactly these fields of the scenario with their values as written in this file; keys sorted by Unicode code point (every key is ASCII); UTF-8; no insignificant whitespace (separators "," and ":"); no ASCII escaping of non-ASCII characters; arrays keep their order; every value is a string, an array or an object (no number, boolean or null), so the text equals its RFC 8785 (JCS) form; no trailing newline |
| Hash | SHA-256 of those UTF-8 bytes, 64 lowercase hexadecimal characters |
| Scope | SEALED_AT covers the expectation of one scenario; the catalog-level definitions it names (DEFAULTS, REPETITION_RULES, LOG_PROFILES, CONDITIONAL_EXPECTATION_RULES, REGISTRY_MAP, GROUP_TABLE) are covered by the artifact-level hash of BA-04 only; SEALED_AT is computed and written before the BA-04 candidate ruling, inside its bytes (AR3-25); the catalog is sealed at artifact level only |

A change of any of these fields is a new `SCENARIO_VERSION` and a new `SEALED_AT` (V2.1 26.1). The published check recomputes every value.

## 2. Statuses and pins

| Status | Meaning |
|---|---|
| `DRAFT` | the scenario is not ready to seal; if `BLOCKED_BY` is not empty, its expectation **cannot be declared** until the blocker is resolved (V2.1 26.2) |
| `SEALED_PENDING_PIN_CANDIDATE` | the expected result has an authority except for pin slots; when the catalog is sealed (artifact level, AR3-25) it becomes `SEALED_PENDING_PIN` |
| `SEALED_CANDIDATE` | no pin and no blocker; ready for the artifact-level candidate ruling |
| `EXPLORATORY_NON_GOVERNING` | the expected result lacks an authority; never promoted after the fact |

`TOL_SCALE` is **not** a pin: it is a value of the Selective kind baseline (V2.2 PA-9) that has no authority today (K-3). Every governing scenario whose run adjudicates S-1, S-2 or S-3 carries `BLOCKED_BY = K-3`. A scenario in the AR3-01 mode records S-1..S-3 without adjudication and does not depend on `TOL_SCALE` (AR3-01), so it carries no K-3.

### 2.1 Pin mechanism (AR2-16, AR2-17, AR3-23)

A pin is a **named slot** in the sealed scenario (`PIN_SLOTS`). Its value lives in a separate **pin record** with its own hash; the BA-11 V4 slot table names the kind. **Writing a pin never changes the bytes of this catalog**, and no BA-11 value is written. A pin is written after the value is frozen from its own evidence and before the first governing run that uses it; it is never filled from the outcome of the scenario that uses it. Custody (BA-07 V4 sections 5.1 and 7, AR3-23; the Architect's ruling on BA-07 V4's reading of the pin state machine is AQ-V4-12):

- **the pin's own seal** is the pair of ratification records of the Coordinator and of the Architect that cite the pin-record hash; the `PIN_RECORDED` custody entry cites both;
- **a pin is usable by a governing run** only when its `PIN_RECORDED` entry is covered by an `ANCHOR_RECORDED` anchor recorded before that run's `IN_USE` entry;
- **each governing run lists** the pin-record hashes it consumes (`IN_USE.pinRecordHashes`).

| Pin kind | Needed when | Source of the value |
|---|---|---|
| `FIXTURE_INSTANCE@<FX>` | the scenario has a fixture, or a variant library (`@L-VAR-DIFF`, `@L-VAR-PROXY`, BA04V3-15) | the instance created by host work under a future gate, conforming to the sealed BA-08 V4 specification; the variant libraries are instantiated after the library census (K-2), which fixes their content |
| `E04_ADMITTED_SET@R-1` | a governing run outside the AR3-01 mode passes P0 | `E4-LEARN-R1-*` under the selected `LOCK_MODE` (AR2-11) |
| `LOCK_MODE` | a governing run outside the AR3-01 mode passes PL; every learning run; `HDM-CHAR-*` | the selection review of V2.1 3.4 over `LK-*`; each `LK-*` and `E4-LEARN-R1-*` run takes its lock candidate as a scenario parameter (per-candidate design), never as a pin |
| `EVM_FROZEN` | a governing run outside the AR3-01 mode reaches MUTATION evaluation (PV) | the EVM frozen after learning (V2.1 27.2; V2.2 PA-2) |
| `HOST_DEFAULT_MAP` | a governing run adjudicates S-1, S-2 or S-3 | `HDM-CHAR-FX1F`, `HDM-CHAR-FXANN`, `HDM-CHAR-FXDIM` (non-governing, AR3-01 mode; the rule of the pin is in BA-08 V4; AR3-11) |

### 2.2 Order of the pin sources (AR2-11 as completed by AR3-01 and AR3-11)

1. `E4-LEARN-R1-LM1`, `E4-LEARN-R1-LM2` under each `LOCK_MODE` candidate (AR3-01 mode);
2. `LK-LM1-*`, `LK-LM2-*` (AR3-01 mode);
3. the selection of `LOCK_MODE` by review (V2.1 3.4; `NO_CANDIDATE` gives LK `FAIL` by 3.4 itself), then the `LOCK_MODE` pin record;
4. the `E04_ADMITTED_SET@R-1` pin record, derived from the `E4-LEARN-R1-*` runs under the selected mode; an offline membership post-check in `E4-VAL-R1`;
5. `HDM-CHAR-*` (AR3-01 mode, under the pinned `LOCK_MODE`), then the `HOST_DEFAULT_MAP` pin record; this step precedes every governing run that uses S-1..S-3 and may run before or after learning;
6. EVM learning (`EV-L-*`, after `REVIEWED`, V2.4), then the `EVM_FROZEN` pin record;
7. validation and the governing runs.

The governing runs of negative controls never feed a pin.

## 3. Conditional-expectation rules (normative)

A conditional expectation is allowed **only** when it is **resolved offline, by a rule declared before the run**, from the **sealed evidence of the same run** or from a sealed qualification record it names. It is never resolved from hindsight, from a later run, or from an outcome the rule does not name. The offline evaluator (I-16) applies it; a rule that the sealed evidence cannot resolve makes the scenario result `UNKNOWN`. A scenario resolved `NOT_RUNNABLE` leaves its coverage cell uncovered: BA-02b V4 shows `NEGATIVE_CONTROL_NOT_EVALUATED`, never MET or PASS (AR3-03). The declared rules (the JSON `CONDITIONAL_EXPECTATION_RULES` holds the same five):

| Rule | Text |
|---|---|
| `CER-ABV` | resolved offline from the sealed ABORT-VERIFY record of the same run: match => O2; mismatch or unreadable => O6, recorded as an S-5 / AB finding (never charged to the target control) |
| `CER-WIT` | resolved offline from the sealed I-14 DELIVERY WITNESS record of the same run at the injection position (an independent subscription of the I-14 instrument, qualified by Q-I14-X and Q-I14-Y), never from the E-12 log itself (AR2-12): a result conditional on delivery is the detection branch when the witness records the event delivered and the non-detection branch when it does not; a negative control whose detection needs the delivery is, on the non-detection branch, NOT_RUNNABLE for its target and its coverage cell stays uncovered (NEGATIVE_CONTROL_NOT_EVALUATED in BA-02b V4, AR3-03) |
| `CER-SIL` | RUNNABLE only if the sealed Q-I14-SILENT-Y record (Y coordinates) or Q-I14-SILENT-X record (X coordinates) shows AVAILABLE at the coordinate; otherwise the scenario is recorded NOT_RUNNABLE, the isolation of the target is NOT_ACHIEVED and its coverage cell stays uncovered (NEGATIVE_CONTROL_NOT_EVALUATED in BA-02b V4, AR3-03) |
| `CER-A2` | resolved offline from the sealed I-14 delivery witness record of the same run only (never from the E-12 log, AR2-12), positioned by the V0 and V1 markers: no deferred WRITE_CLASS event on a protected object after V0 => O3, A-2 PASS and E-12 phase C PASS; a deferred event delivered between V0 and V1 (phase C) => O7, A-2 FAIL and E-12 phase C FAIL; a deferred event delivered only after V1 (the tail) => O3, A-2 FAIL and E-12 phase C PASS; an A-2 FAIL gives the group verdict CHARACTERIZED_DIFFERENT (V2.2 PA-1.2) |
| `CER-PERSIST` | resolved offline from the sealed I-14 independent post-abort read of the changed entity (not from ABORT-VERIFY, not from the E-12 log): the change persisted after the abort => S-5 FAIL (ABORT-VERIFY mismatch, unexpected mutation, V2.1 11.3) and O6 (17.2 row 4); it did not persist => the product outcome follows CER-ABV (O2 on a match) and the scenario is recorded NOT_RUNNABLE for NC-S5-O6: its S-5 cell stays uncovered (NEGATIVE_CONTROL_NOT_EVALUATED in BA-02b V4, AR3-03, AR3-04) |

## 4. Defaults, repetition rules, log profiles and tuples

| Field | Default |
|---|---|
| `EXPECTED_EVIDENCE_SET` | EVIDENCE_PACKAGE_STANDARD (V2.1 18.2) |
| `EXPECTED_CLEANUP` | CLEANUP_STANDARD (V2.1 20); the original library file is restored before the next run's tuple check where a scenario changed it (AR2-31) |
| `RETRY_APPLICABILITY` | NONE_AUTHORIZED (V2.1 21.3; INVALID only with a Coordinator authorization per occurrence; a governing FAIL or UNKNOWN is never retried) |
| `MAX_ATTEMPTS` | PARAM-04 m (Coordinator; BA-10 V4; UNSET) per planned governing run, that is per repetition slot: every attempt of the slot counts, contamination included; a slot that exhausts m leaves the scenario without a required run and its group cannot be PASS (AR3-24); a non-governing run has no repetition slot |
| `EXPECTED_INSTRUMENT_STATES` | every instrument RELIABLE (qualified on the exact build) |
| `UNLISTED_CONTROLS` | every control the run evaluates and EXPECTED_CONTROL_RESULTS does not list is expected PASS; a control the run does not reach is NOT_EVALUATED (AR2-25); a control of the RECORDED_NOT_ENFORCED set is recorded, never PASS or FAIL (AR3-01); the rule does not apply to a DRY_RUN scenario, whose only measured result is the WARMUP_COVERAGE_CRITERION |

| Tuple | Content |
|---|---|
| `FULL_BASELINE_TUPLE` | FULL_BASELINE_TUPLE (V2.1 2.1 as extended by V2.5 CATALOG_FOLDER: every BUILD_BOUND and MACHINE_PROFILE_BOUND field equal to the baseline; SESSION_BOUND fields present and consistent; the block-library field is sampled once, before the acquisition, AR2-31) |
| `CHARACTERIZATION_TUPLE` | CHARACTERIZATION_TUPLE: FULL_BASELINE_TUPLE in which the build under test is the baseline CHARACTERIZATION_BUILD (its build receipt and DLL SHA-256 are BUILD_BOUND fields: V2.1 2.1 "any payload / observer / harness DLL"; AR3-01); every other field as in FULL_BASELINE_TUPLE |
| `CONTROL_PLANE_TUPLE` | CONTROL_PLANE_TUPLE (no host): control-plane source SHA, evaluator assembly SHA-256, evaluator version, synthetic log fixture hash, contract composite hash (BA-11 V4 section 4) and scenario-catalog hash; no host, machine-profile or session field applies (AR2-29) |
| `FPSPEC_CONFORMANCE_TUPLE` | FPSPEC_CONFORMANCE_TUPLE: fingerprint-provider assembly SHA-256, FINGERPRINT_SPEC_VERSION and the BA-05 V4 artifact hash, SHA-256 of the vectors file, the SEAL_HASH of BA-08 as the BA-11 registry records it (the source of the slot values: the sealed tolerance values of its section 10, AR3-17), SHA-256 of the binary64 instance file generated by the harness, control-plane source SHA, contract composite hash (AR2-29) |
| `QUALIFICATION_TUPLE` | QUALIFICATION_TUPLE: the BUILD_BOUND fields of the exact build and of the instruments under qualification (V2.1 2.1: repo source SHA, Plugin and harness DLL SHA-256, AutoCAD version, build and acad.exe SHA-256, contract and catalog hashes) and the MACHINE_PROFILE_BOUND fields; SESSION_BOUND fields present and consistent; no fixture or library field unless the scenario names one (V2.1 4.3: qualification on the exact build) |
| `variant library` | FULL_BASELINE_TUPLE except the block-library field, which is the declared VARIANT library <L-VAR> of BA-08 V4 section 12 (its SHA-256 is a scenario-specific tuple field, pinned by the slot FIXTURE_INSTANCE@<L-VAR>) |

| Repetition rule | Value |
|---|---|
| `DEFAULT` | N_strict (PARAM-01), exactly as BA-10 V4 section 2.2 defines it over the class-A and class-B CONTROL groups of GROUPS_SERVED: N_A if the scenario serves only class-A groups, N_B if it serves only class-B groups, max(N_A, N_B) if it serves both (the strictest pair); no ordering of N_A and N_B is assumed; KR (PA-4), the evidence groups (PA-3) and CONTRACT_ASSIGNED_GROUPS (AR3-06; BA-10 V4 Q-A1, pending AQ-V4-13) never enter N_strict (BA-10 V4; UNSET) |
| `CLEAN` | max(N_strict, ceil(n / K)) (PARAM-01 and PARAM-03): K = the number of scenarios of the PARAM-03 sample in the sealed catalog, that is the governing CL-CLEAN-<fixture> scenarios plus EA2-DEFERRED-EVENTS (AR3-24; symbolic, never a hard-coded count) (BA-10 V4; UNSET) |
| `VALIDATE` | max(N_strict, the PARAM-02 validation count N_det) (AR3-24: the strictest pair applies to every governing scenario that serves a class-A or class-B group) (BA-10 V4; UNSET) |
| `EVIDENCE` | EVIDENCE_REPETITION (Coordinator; BA-10 V4; UNSET) |
| `EVIDENCE_STRICT` | max(N_strict, EVIDENCE_REPETITION): an evidence-group scenario that also serves a class-A or class-B CONTROL group through CONTROLS_EXERCISED (AR3-24) (BA-10 V4; UNSET) |
| `SYNTH` | ONE (deterministic offline evaluation; BA-10 V4) |
| `LEARN` | PARAM-02 (learning micro-scenarios; BA-10 V4; UNSET) |
| `DRY` | WU_DRY_RUNS = N_B (Owner, Q-O2; recorded by the Coordinator; BA-10 V4; UNSET) dry runs per dry-run scenario (AR3-02, AR3-24) |
| `CHAR` | CHAR_RUNS (BA-10 V4 section 2.2; UNSET): N_A for a non-governing run that feeds a pin or a selection (E4-LEARN-R1-*, LK-*); for HDM-CHAR-*, CHAR_RUNS(s) = N_A + V(s): N_A runs at the pinned key values (they carry the intent) plus V(s) >= 1 informative run per varied key (outside the intent); EVIDENCE_REPETITION for an exploratory scenario (E6-C4); the intent is pending AQ-V4-13; not a governing repetition slot |

The per-scenario `REPETITION` field names the rule (the first row of the rule table of BA-10 V4 section 2.2 that matches) and resolves `N_strict` as `N_A` (class-A groups only), `N_B` (class-B groups only) or `max(N_A, N_B)` (both classes), and `CHAR_RUNS` as BA-10 V4 defines it. The PARAM-03 sample (AR3-24) is: `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, `CL-CLEAN-FX2F-ALL`, `CL-CLEAN-FX4F-ALL`, `CL-CLEAN-FXANN-FP`, `CL-CLEAN-FXANNBLANK-FP`, `CL-CLEAN-FXDIM-FP`, `CL-CLEAN-FXIMP-FP`, `EA2-DEFERRED-EVENTS` (K = 10 in this draft; the rule stays symbolic).

| Log profile | Record types and counts |
|---|---|
| `LP-QUAL` | QUAL_START x1; POSITIVE_CONTROL x1; NEGATIVE_CONTROL x1 per negative variant; CONSISTENCY x1; QUAL_VERDICT x1; SEAL x1 |
| `LP-SYNTH` | INPUT_LOG_HASH x1; EVALUATION x1 per evaluated row or state; OUTCOME x1; SEAL x1 |
| `LP-MEASURE` | RUN_START x1; TUPLE x1; SUBSCRIPTION x1 (the qualified I-10 subscriber); READ_ONLY_OPEN x1 per protected class; EVENT x1 per delivered event; MEASUREMENT x1; CLEANUP x1; SEAL x1 (no product run: EA1, AR3-09) |
| `LP-WU-BOUNDARY` | RUN_START x1; TUPLE x1; WARMUP x1 (WARMUP_SIDE_DATABASE); WARMUP_SIDE_DB x1 per side-database transaction; BOUNDARY_CHECK x4 (V2.2 PA-12); CLEANUP x1; SEAL x1 |
| `LP-P0-STEP3` | RUN_START x1; TUPLE x1; WARMUP x1; WARMUP_LOADS x1; MEMBERSHIP x1 (the foreign load); OUTCOME x1; CLEANUP x1; SEAL x1 (refusal at V2.1 13.1 step 3: no subscription, no baseline snapshot) |
| `LP-P0-STEP4` | RUN_START x1; TUPLE x1; WARMUP x1; WARMUP_LOADS x1; MEMBERSHIP x1; SUBSCRIPTION x1 per subscription attempted (the failing one records its failure); OUTCOME x1; CLEANUP x1; SEAL x1 (refusal at V2.1 13.1 step 4: no baseline snapshot) |
| `LP-P0` | RUN_START x1; TUPLE x1; WARMUP x1; WARMUP_LOADS x1; MEMBERSHIP x1; SUBSCRIPTION x2 (E-11M, E-12); BASELINE_SNAPSHOT x2; CHECKPOINT x1 (P0); OUTCOME x1; CLEANUP x1; SEAL x1 |
| `LP-PL` | LP-P0 plus LOCK x1 and CHECKPOINT x1 (PL) |
| `LP-PR` | LP-PL plus LIBRARY_INPUT_RECORD x1; AUTH12_CALL x1 per call; PRE_COMMAND_STATE_RECORD x1; PREPARE_R_DECISION x1; CHECKPOINT x1 (PR) |
| `LP-PW-REFUSE` | LP-PR plus REOBSERVATION x1 (the mismatch) and MANIFEST x1 (empty); no import record and no PP checkpoint (refusal before the first import) |
| `LP-PW` | LP-PR plus REOBSERVATION x1; COPY_REHASH x1 per import; IMPORT x1 per import; IMPORT_FINGERPRINT x1 per completed import; MANIFEST x1; CHECKPOINT x1 (PP) |
| `LP-PW-STOP` | LP-PW plus STOP x1 (T_M NOT_OPENED after a PREPARE-W write, V2.1 17.2 rows 3/4) and ABORT_VERIFY x1 (a run that stops after a PREPARE-W write) |
| `LP-PS` | LP-PW plus TM_OPEN x1; CHECKPOINT x1 (PS); SEAM_CALL x1 per seam call made, up to and including the failing call; SEAM_FAILURE x1 (typed, V2.1 15.4); E12_EVENT x1 per delivered event (recorded; phase A is not evaluated: PV is not reached); ABORT x1; ABORT_VERIFY x1 |
| `LP-PV` | LP-PW plus TM_OPEN x1; SEAM_CALL x1 per seam call of f(plan); E12_EVENT x1 per delivered event (observed); CHECKPOINT x2 (PS, PV); ABORT x1; ABORT_VERIFY x1 |
| `LP-CP` | LP-PW plus TM_OPEN x1; SEAM_CALL x1 per seam call of f(plan); E12_EVENT x1 per delivered event; CHECKPOINT x2 (PS, PV); SEALED_LIST x1; COMMIT x1; PHASE_B_LOG x1; MARKER x2 (V0, V1); PASS1_READ and PASS2_READ x1 per sealed element each; PHASE_C_LOG_SEALED x1; POST_V1_TO_CP_QUEUE_LOG x1; CHECKPOINT x1 (PC = CP) |
| `LP-LEARN` | RUN_START x1; TUPLE x1; WARMUP x1; LOCK x1 (the pinned LOCK_MODE); MICRO_OPERATION x1; E12_EVENT_RAW x1 per delivered event; CLEANUP x1; SEAL x1 |
| `LP-CHAR` | the profile of the reach, plus CHARACTERIZATION_BUILD x1 (build receipt and DLL SHA-256); the records of the RECORDED_NOT_ENFORCED controls carry RECORDED verdicts (AR3-01); plus the scenario records: D_VALUES x1 per checkpoint (E4-LEARN-R1-*), LOCK_PROPERTY x1 per property (LK-*), CONTEXT_VARIABLES x1 and EFFECTIVE_ATTRIBUTE x1 per recorded attribute of a created entity (HDM-CHAR-*), WRITER_EFFECT x1 and WITNESS x1 per injection (Q-I14-*); over LP-LEARN, the micro-operation records of the learning run (EV-L-*) |
| `LP-DRY` | the profile of the source scenario's path, plus CHARACTERIZATION_BUILD x1 and LOAD_EVENT x1 per assembly-load event (expected zero inside the governed window); where a control of the RECORDED_NOT_ENFORCED set fails, its record is kept and the dry run does not take the refusal or abort branch (section 5.2) |
| `LP-EVID` | the profile of the preceding run, plus UNDO_STEP x1 per UNDO step and STATE_READ x1 per compared element (U-*), SAVE_REOPEN x1 (S1), POST_CP_TAIL x1 and REREAD x1 per sealed element (PP), CLEANUP_CHECK x1 per check of V2.1 20 (CL), EXCEPTION_ORIGIN x1 (U-6b) |

## 5. Totals and index

Totals: **393** scenarios (232 own scenarios and 161 `WU-DRY-*`). Status: `DRAFT` = 185, `EXPLORATORY_NON_GOVERNING` = 1, `SEALED_CANDIDATE` = 41, `SEALED_PENDING_PIN_CANDIDATE` = 166. Governance: `EXPLORATORY_NON_GOVERNING` = 1, `GOVERNING` = 222, `NON_GOVERNING_BY_DESIGN` = 170. Build under test: `CHARACTERIZATION_BUILD` = 316, `NONE` = 12, `PRODUCT_BUILD` = 65. Blockers: `AQ-V4-01` = 25, `AQ-V4-03` = 3, `E6-C4` = 1, `K-3` = 134, `K-6 (AQ-V4-07, Q-HDM-1)` = 1, `K-8` = 32. Pin slots used: `E04_ADMITTED_SET` = 157, `EVM_FROZEN` = 134, `FIXTURE_INSTANCE` = 355, `HOST_DEFAULT_MAP` = 134, `LOCK_MODE` = 168.

Every expected outcome is an exact `O1..O7`, `NONE`, a declared conditional of section 3, `PROVISIONAL_NON_GOVERNING` (AR3-01 mode) or, for an evidence scenario, the exact or conditional outcome of its preceding run (checked mechanically): NONE = 35, PROVISIONAL_NON_GOVERNING = 191, conditional (section 3) = 23, exact O1..O7 = 134, preceding run: exact or conditional = 10.

Repetition rules used: `CHAR: CHAR_RUNS = EVIDENCE_REPETITION (exploratory)` = 1, `CHAR: CHAR_RUNS = N_A` = 6, `CHAR: CHAR_RUNS = N_A at the pinned key values, plus at least one informative run per varied key (outside the intent)` = 3, `CLEAN: max(max(N_A, N_B), ceil(n / K))` = 10, `DEFAULT: N_A` = 25, `DEFAULT: N_B` = 3, `DEFAULT: max(N_A, N_B)` = 114, `DRY: WU_DRY_RUNS = N_B` = 161, `EVIDENCE: EVIDENCE_REPETITION` = 35, `EVIDENCE_STRICT: max(N_A, EVIDENCE_REPETITION)` = 1, `EVIDENCE_STRICT: max(N_B, EVIDENCE_REPETITION)` = 1, `EVIDENCE_STRICT: max(max(N_A, N_B), EVIDENCE_REPETITION)` = 1, `LEARN: PARAM-02` = 17, `SYNTH: ONE` = 12, `VALIDATE: max(max(N_A, N_B), PARAM-02 validation count)` = 3.

| Group | Count |
|---|---|
| `AB` | 6 |
| `CL` | 1 |
| `E10` | 1 |
| `E11M` | 2 |
| `E12-L` | 17 |
| `E12-V` | 6 |
| `E4` | 4 |
| `E6` | 9 |
| `E8` | 14 |
| `E9` | 1 |
| `EA1` | 1 |
| `EA2` | 1 |
| `HDM` | 3 |
| `ID-A` | 3 |
| `KS` | 11 |
| `LK` | 6 |
| `OU` | 11 |
| `PP` | 1 |
| `PR-FRESH` | 4 |
| `PR-REC` | 3 |
| `PW` | 1 |
| `PW-CLONE` | 6 |
| `Q` | 27 |
| `S1` | 1 |
| `SC-A` | 49 |
| `SC-B` | 32 |
| `U-1..U-8` | 8 |
| `U-6b` | 1 |
| `WU` | 163 |

`HDM` is not a group of V2.2 PA-1.3: it labels the three non-governing `HOST_DEFAULT_MAP` characterization scenarios, which serve no group (`GROUPS_SERVED` = []).

### 5.1 Scenario summary (every scenario; `WU-DRY-*` are listed in section 5.2)

| ID | GROUP | REACH | COORD | TARGET | DS | BUILD | EXPECTED_PRODUCT_OUTCOME | STATUS | BLOCKED_BY | SEALED_AT (first 12) |
|---|---|---|---|---|---|---|---|---|---|---|
| `Q-I01` | `Q` | NONE | NONE | INSTRUMENT:I-01 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `8f40b15e81ca` |
| `Q-I02` | `Q` | NONE | NONE | INSTRUMENT:I-02 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `23121172889c` |
| `Q-I03` | `Q` | NONE | NONE | INSTRUMENT:I-03 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `c68a64582cbd` |
| `Q-I04` | `Q` | NONE | NONE | INSTRUMENT:I-04 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `9696fe1a886d` |
| `Q-I05` | `Q` | NONE | NONE | INSTRUMENT:I-05 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `e324d550462e` |
| `Q-I06` | `Q` | NONE | NONE | INSTRUMENT:I-06 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `86f8765a5fd8` |
| `Q-I07` | `Q` | NONE | NONE | INSTRUMENT:I-07 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `bc420d149c1e` |
| `Q-I08` | `Q` | NONE | NONE | INSTRUMENT:I-08 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `dd14801257e4` |
| `Q-I09` | `Q` | NONE | NONE | INSTRUMENT:I-09 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `c69824714128` |
| `Q-I10` | `Q` | NONE | NONE | INSTRUMENT:I-10 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `9e6ed971539a` |
| `Q-I11` | `Q` | NONE | NONE | INSTRUMENT:I-11 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `bae4c23e2fea` |
| `Q-I12` | `Q` | NONE | NONE | INSTRUMENT:I-12 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `d60db78f279e` |
| `Q-I13` | `Q` | NONE | NONE | INSTRUMENT:I-13 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `b3f75cde9162` |
| `Q-I14` | `Q` | NONE | NONE | INSTRUMENT:I-14 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `e33aedecba03` |
| `Q-I15` | `Q` | NONE | NONE | INSTRUMENT:I-15 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `5420b8047864` |
| `Q-I16` | `Q` | NONE | NONE | INSTRUMENT:I-16 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `4e081c1384a8` |
| `Q-I09-LOSS` | `Q` | NONE | NONE | INSTRUMENT:I-09 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `a1d80f555455` |
| `Q-I10-LOSS` | `Q` | NONE | NONE | INSTRUMENT:I-10 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `44208b6dcf51` |
| `Q-I14-Y` | `Q` | CP | Y0, Y1 and Y2 | INSTRUMENT:I-14 | - | C | PROVISIONAL_NON_GOVERNING | DRAFT | AQ-V4-01 | `2bab29ae7d6e` |
| `Q-I14-X` | `Q` | CP | X0..X8 | INSTRUMENT:I-14 | - | C | PROVISIONAL_NON_GOVERNING | DRAFT | AQ-V4-01 | `8bc98b15db3c` |
| `Q-I14-SILENT-Y` | `Q` | CP | Y0, Y1 and Y2 | NONE | - | C | PROVISIONAL_NON_GOVERNING | DRAFT | AQ-V4-01 | `a55ec7d31f02` |
| `Q-I14-SILENT-X` | `Q` | CP | X1..X6 | NONE | - | C | PROVISIONAL_NON_GOVERNING | DRAFT | AQ-V4-01 | `c5bc69cb2a3b` |
| `Q-SIGNED-ZERO` | `Q` | NONE | NONE | INSTRUMENT:I-12 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `39dfff24a694` |
| `Q-FPSPEC-VECTORS` | `Q` | NONE | NONE | NONE | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `71ed7b14ed62` |
| `Q-ROUTE-R1` | `Q` | NONE | NONE | NONE | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `8b3d18094727` |
| `Q-I16-CLEANUP-NEG` | `Q` | NONE | NONE | INSTRUMENT:I-16 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `7df0e0e027ef` |
| `E6-C1` | `E6` | NONE | NONE | NONE | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `89c113a5687d` |
| `E6-C2` | `E6` | NONE | NONE | NONE | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `af4e18deb4c8` |
| `E6-C3` | `E6` | NONE | NONE | NONE | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `f3c0027c873d` |
| `E6-C4` | `E6` | NONE | NONE | NONE | - | P | NONE (no product run) | EXPLORATORY_NON_GOVERNING | - | `4ddd3761ec38` |
| `E6-C5` | `E6` | NONE | NONE | NONE | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `a07cc581f25a` |
| `E6-C6` | `E6` | NONE | NONE | E06 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `44f31700a9c5` |
| `E6-C7` | `E6` | NONE | NONE | NONE | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `40302230e1fd` |
| `E6-C8` | `E6` | NONE | NONE | E06 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `670f6f4792ee` |
| `CL-CLEAN-FX1F-F0` | `KS` | CP | NONE | NONE | - | P | O3 SUCCESS | DRAFT | K-3 | `2d57f2ebb0a5` |
| `CL-CLEAN-FX1F-P` | `KS` | CP | NONE | NONE | - | P | O3 SUCCESS | DRAFT | K-3 | `f7b97ff57331` |
| `CL-CLEAN-FX1F-FP` | `KS` | CP | NONE | NONE | - | P | O3 SUCCESS | DRAFT | K-3 | `df99887093e8` |
| `CL-CLEAN-FX2F-ALL` | `KS` | CP | NONE | NONE | - | P | O3 SUCCESS | DRAFT | K-3 | `b867f7deb21f` |
| `CL-CLEAN-FX4F-ALL` | `KS` | CP | NONE | NONE | - | P | O3 SUCCESS | DRAFT | K-3 | `70e9505f25c4` |
| `CL-CLEAN-FXANN-FP` | `KS` | CP | NONE | NONE | - | P | O3 SUCCESS | DRAFT | K-3 | `8b1b6278bcff` |
| `CL-CLEAN-FXANNBLANK-FP` | `KS` | CP | NONE | NONE | - | P | O3 SUCCESS | DRAFT | K-3, K-6 (AQ-V4-07, Q-HDM-1) | `135eec1c7fbc` |
| `CL-CLEAN-FXDIM-FP` | `KS` | CP | NONE | NONE | - | P | O3 SUCCESS | DRAFT | K-3 | `676e2575e4b4` |
| `CL-CLEAN-FXIMP-FP` | `KS` | CP | NONE | NONE | - | P | O3 SUCCESS | DRAFT | K-3, K-8 | `92448fa3de25` |
| `NC-E02` | `ID-A` | P0 | P0 | E02 | - | P | O1 | SEALED_PENDING_PIN_CANDIDATE | - | `15e46474240a` |
| `NC-E05` | `ID-A` | P0 | P0 | E05 | - | P | O1 | SEALED_PENDING_PIN_CANDIDATE | - | `dcb552eec676` |
| `NC-E06` | `E6` | P0 | P0 | E06 | - | P | O1 | SEALED_PENDING_PIN_CANDIDATE | - | `c3f484adb051` |
| `NC-E07` | `ID-A` | P0 | P0 | E07 | - | P | O1 | SEALED_PENDING_PIN_CANDIDATE | - | `8293acf38448` |
| `NC-E09` | `E9` | P0 | P0 | E09 | - | P | O1 | SEALED_PENDING_PIN_CANDIDATE | - | `c9c6011e1e8f` |
| `NC-E10` | `E10` | P0 | 13.1 step 5 (between the two baseline snapshots) | E10 | - | C | O1 | SEALED_PENDING_PIN_CANDIDATE | - | `6722f4518a19` |
| `NC-E11M-PV` | `E11M` | PV | between PS and PV | E11M | - | C | O2 (CER-ABV) | DRAFT | K-3 | `2e7c1c2eefb9` |
| `NC-E11M-PC` | `E11M` | CP | X7 | E11M | - | C | O7 | DRAFT | K-3 | `27b5a5bc5333` |
| `NC-E12A` | `E12-V` | PV | Y1 | E12A | - | C | O2 (CER-ABV) | DRAFT | K-3 | `5a63f2c193c6` |
| `NC-E12D` | `E12-V` | P0 | 13.1 step 4 | E12D | - | C | O1 | SEALED_PENDING_PIN_CANDIDATE | - | `531cc181be2b` |
| `NC-E12C` | `E12-V` | CP | X6 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the write delive... | DRAFT | K-3 | `c77137b11e50` |
| `NC-E04-R2` | `E4` | P0 | P0 | E04 | - | P | O1 | SEALED_PENDING_PIN_CANDIDATE | - | `78d704252e37` |
| `NC-WU-FOREIGN` | `WU` | P0 | 13.1 step 3 | WUM | - | C | O1 | SEALED_PENDING_PIN_CANDIDATE | - | `176eaa0d9a27` |
| `NC-E03-NOT-HELD` | `LK` | PL | PL | E03 | - | C | O1 | SEALED_PENDING_PIN_CANDIDATE | - | `87b3f550c9b0` |
| `NC-E03-REACQUIRE` | `LK` | PV | between PL and PS | E03 | - | C | O2 (CER-ABV) | DRAFT | K-3 | `42d421d58d57` |
| `NC-S1-Y1` | `KS` | PV | Y1 | S1 | - | C | O2 (CER-ABV; the abort before Commit() is unconditional) | DRAFT | K-3 | `dabcbf3dfc79` |
| `NC-S1-Y1-SILENT` | `KS` | PV | Y1 | S1 | - | C | O2 (CER-ABV; the abort before Commit() is unconditional) | DRAFT | K-3 | `a5dc606f3f02` |
| `NC-E08-Y0` | `E8` | PV | Y0 | E08 | - | C | O2 (CER-ABV; the abort before Commit() is unconditional) | DRAFT | K-3 | `a9496aaadde2` |
| `NC-E08-Y0-SILENT` | `E8` | PV | Y0 | E08 | - | C | O2 (CER-ABV; the abort before Commit() is unconditional) | DRAFT | K-3 | `fc6e268e7e10` |
| `NC-E08-Y2` | `E8` | PV | Y2 | E08 | - | C | O2 (CER-ABV; the abort before Commit() is unconditional) | DRAFT | K-3 | `6363209fdf06` |
| `NC-E08-Y2-SILENT` | `E8` | PV | Y2 | E08 | - | C | O2 (CER-ABV; the abort before Commit() is unconditional) | DRAFT | K-3 | `19c0a39ae0b2` |
| `EA1-READONLY-OPENS` | `EA1` | NONE | NONE | NONE | - | P | NONE (no product run: the A-1 result comes from the measurement, AR... | SEALED_PENDING_PIN_CANDIDATE | - | `560d68794e65` |
| `EA2-DEFERRED-EVENTS` | `EA2` | CP | NONE | NONE | - | P | CONDITIONAL (CER-A2): O3 when no deferred event is delivered in pha... | DRAFT | K-3 | `9478cd46aa03` |
| `OU-O1` | `OU` | SYN | NONE | NONE | - | - | O1 | SEALED_CANDIDATE | - | `98310e90ce65` |
| `OU-O2` | `OU` | SYN | NONE | NONE | - | - | O2 | SEALED_CANDIDATE | - | `2123ac32be25` |
| `OU-O6` | `OU` | SYN | NONE | NONE | - | - | O6 | SEALED_CANDIDATE | - | `74d5f3d4948f` |
| `OU-O4` | `OU` | SYN | NONE | NONE | - | - | O4 | SEALED_CANDIDATE | - | `9c28356b5df1` |
| `OU-O5` | `OU` | SYN | NONE | NONE | - | - | O5 | SEALED_CANDIDATE | - | `d44c2e9a1ea5` |
| `OU-O7` | `OU` | SYN | NONE | NONE | - | - | O7 | SEALED_CANDIDATE | - | `209e96bfac35` |
| `OU-O3` | `OU` | SYN | NONE | NONE | - | - | O3 | SEALED_CANDIDATE | - | `ecc5506d7285` |
| `OU-ROW1-INVALID` | `OU` | SYN | NONE | NONE | - | - | NONE (row 1: no governing outcome; the evaluator retains a PROVISIO... | SEALED_CANDIDATE | - | `29d311c115d9` |
| `OU-PREDICATE` | `OU` | SYN | NONE | NONE | - | - | NONE | SEALED_CANDIDATE | - | `b9a463835a64` |
| `AB-COMMIT-EXCEPTION-T1-A` | `OU` | SYN | NONE | NONE | - | - | O2 | SEALED_CANDIDATE | - | `48b17efdd5e8` |
| `AB-COMMIT-EXCEPTION-T1-B` | `OU` | SYN | NONE | NONE | - | - | O6 | SEALED_CANDIDATE | - | `63b6925b0895` |
| `PR-SOURCE-SWAP-MODIFY` | `PR-FRESH` | CP | after PR (acquisition) | NONE | Y | C | O3 SUCCESS | DRAFT | K-3, K-8 | `2d48ae97ed6c` |
| `PR-SOURCE-SWAP-DELETE` | `PR-FRESH` | CP | after PR (acquisition) | NONE | Y | C | O3 SUCCESS | DRAFT | K-3, K-8 | `c0625c3b0391` |
| `PR-SOURCE-SWAP-NEG-COPY` | `PR-FRESH` | PW | between the first and the second import | LIBF | - | C | O2 (CER-ABV: the first import is in the manifest; ABORT-VERIFY comp... | DRAFT | K-8 | `d0c335093da9` |
| `PR-TORN-READ` | `PR-FRESH` | PR | during the acquisition | LIBF | - | C | O1 (acquisition UNKNOWN => refusal before any product write) | SEALED_PENDING_PIN_CANDIDATE | - | `36aabdf0aa8c` |
| `PW-CLONE-BASE` | `PW-CLONE` | CP | NONE | NONE | Y | P | O3 SUCCESS | DRAFT | K-3, K-8 | `2b4743a591fa` |
| `PW-CLONE-CASE` | `PW-CLONE` | CP | NONE | NONE | Y | P | O3 SUCCESS | DRAFT | K-3, K-8 | `45a71e2ab693` |
| `PW-CLONE-WRONG-TYPE` | `PW-CLONE` | PR | NONE | NONE | Y | P | O1 | DRAFT | K-8 | `4712f28d65fe` |
| `PW-CLONE-NESTED` | `PW-CLONE` | CP | NONE | NONE | Y | P | O3 SUCCESS | DRAFT | K-3, K-8 | `187b74d6ce43` |
| `PW-CLONE-SYMTAB` | `PW-CLONE` | CP | NONE | NONE | Y | P | O3 SUCCESS | DRAFT | K-3, K-8 | `6b11fe8daa99` |
| `PR-CLOSURE-COMPLETENESS` | `Q` | NONE | NONE | NONE | - | P | NONE (qualification run on a test document; no mirror) | DRAFT | K-8 | `359f1081d1a9` |
| `PR-REC-CLOSURE-UNKNOWN` | `PR-REC` | PR | NONE | S5CLO | - | P | O1 | SEALED_PENDING_PIN_CANDIDATE | - | `23ba2187c712` |
| `PR-REC-UNREADABLE` | `PR-REC` | PR | PR | S5REC | - | C | O1 | SEALED_PENDING_PIN_CANDIDATE | - | `15d8b0c28427` |
| `PR-REC-REOBS-MISMATCH` | `PR-REC` | PW | between PR and PW | S5REC | - | C | O1 (empty manifest) | DRAFT | K-8 | `7eaa55b43c06` |
| `PW-IMPORT-PARTIAL-FAIL` | `PW` | PW | inside PW | S5MAN | - | C | CONDITIONAL (CER-ABV): O2 on a match of the record plus the complet... | DRAFT | K-8 | `83a8265a7a5d` |
| `AB-AFTER-PREPARE-W` | `AB` | PW | between PP and PS | NONE | Y | C | O2 (T_M NOT_OPENED after a PREPARE-W write; ABORT-VERIFY match with... | DRAFT | K-8 | `beb29e6d6c77` |
| `AB-AFTER-MUTATION` | `AB` | PV | between PS and PV | E11M | - | C | O2 | DRAFT | K-3 | `48e603a32b03` |
| `NC-S5-O6` | `AB` | PV | Y1 | S5 | - | C | CONDITIONAL (CER-PERSIST): O6 when the independent post-abort read ... | DRAFT | K-3, E6-C4 | `6bce2d6aa29a` |
| `NC-CLONE-REPLACE` | `PW-CLONE` | PW | inside PW (the SL-5 import) | CLONE | - | C | O6 (T_M NOT_OPENED after a PREPARE-W write; ABORT-VERIFY mismatch: ... | DRAFT | K-8 | `79b4ddd3daf8` |
| `KS-SL1-POSTWRITE` | `AB` | PS | inside SL-1 | NONE | Y | C | O2 (CER-ABV) | DRAFT | AQ-V4-03 | `8bd157039bf3` |
| `KS-SL1-TX-MISMATCH` | `AB` | PS | SL-1 entry | NONE | Y | C | O2 (CER-ABV) | DRAFT | AQ-V4-03 | `15b8f9308326` |
| `KS-SL4-NEG-DET` | `AB` | PS | SL-4 entry | NONE | Y | C | O2 (CER-ABV) | DRAFT | AQ-V4-03 | `67004e0386a6` |
| `E4-LEARN-R1-LM1` | `E4` | CP | NONE | NONE | - | C | PROVISIONAL_NON_GOVERNING | SEALED_PENDING_PIN_CANDIDATE | - | `b15a3c83e492` |
| `LK-LM1-PROPS` | `LK` | CP | NONE | NONE | - | C | PROVISIONAL_NON_GOVERNING | SEALED_PENDING_PIN_CANDIDATE | - | `82c237f0293a` |
| `LK-LM1-CONFLICT` | `LK` | CP | between PL and V1 | NONE | Y | C | PROVISIONAL_NON_GOVERNING | SEALED_PENDING_PIN_CANDIDATE | - | `7c29f38bbd1f` |
| `E4-LEARN-R1-LM2` | `E4` | CP | NONE | NONE | - | C | PROVISIONAL_NON_GOVERNING | SEALED_PENDING_PIN_CANDIDATE | - | `4e8afff1aa2c` |
| `LK-LM2-PROPS` | `LK` | CP | NONE | NONE | - | C | PROVISIONAL_NON_GOVERNING | SEALED_PENDING_PIN_CANDIDATE | - | `ce6d9ee4e041` |
| `LK-LM2-CONFLICT` | `LK` | CP | between PL and V1 | NONE | Y | C | PROVISIONAL_NON_GOVERNING | SEALED_PENDING_PIN_CANDIDATE | - | `6483ab149c43` |
| `HDM-CHAR-FX1F` | `HDM` | CP | NONE | NONE | - | C | PROVISIONAL_NON_GOVERNING | SEALED_PENDING_PIN_CANDIDATE | - | `b00385350811` |
| `HDM-CHAR-FXANN` | `HDM` | CP | NONE | NONE | - | C | PROVISIONAL_NON_GOVERNING | SEALED_PENDING_PIN_CANDIDATE | - | `abbff8d19e2a` |
| `HDM-CHAR-FXDIM` | `HDM` | CP | NONE | NONE | - | C | PROVISIONAL_NON_GOVERNING | SEALED_PENDING_PIN_CANDIDATE | - | `fa2ef680d85d` |
| `E4-VAL-R1` | `E4` | CP | NONE | NONE | - | P | O3 SUCCESS | DRAFT | K-3 | `e365e094aff1` |
| `EV-L-OC-TM` | `E12-L` | LEARN | NONE | NONE | - | C | PROVISIONAL_NON_GOVERNING | DRAFT | AQ-V4-01 | `c4bcdbb7893b` |
| `EV-L-OC-BT-OPEN` | `E12-L` | LEARN | NONE | NONE | - | C | PROVISIONAL_NON_GOVERNING | DRAFT | AQ-V4-01 | `24faa6fcd7b8` |
| `EV-L-OC-BTR-NESTED` | `E12-L` | LEARN | NONE | NONE | - | C | PROVISIONAL_NON_GOVERNING | DRAFT | AQ-V4-01 | `8d801444919e` |
| `EV-L-OC-BTR-VIEW` | `E12-L` | LEARN | NONE | NONE | - | C | PROVISIONAL_NON_GOVERNING | DRAFT | AQ-V4-01 | `2c556662b4c2` |
| `EV-L-OC-REF-PIECE` | `E12-L` | LEARN | NONE | NONE | - | C | PROVISIONAL_NON_GOVERNING | DRAFT | AQ-V4-01 | `8f7921ce6910` |
| `EV-L-OC-DYN` | `E12-L` | LEARN | NONE | NONE | - | C | PROVISIONAL_NON_GOVERNING | DRAFT | AQ-V4-01 | `e6fba40679e2` |
| `EV-L-OC-REF-ARRAY` | `E12-L` | LEARN | NONE | NONE | - | C | PROVISIONAL_NON_GOVERNING | DRAFT | AQ-V4-01 | `394ee185f369` |
| `EV-L-OC-TM-READ` | `E12-L` | LEARN | NONE | NONE | - | C | PROVISIONAL_NON_GOVERNING | DRAFT | AQ-V4-01 | `a010af535198` |
| `EV-L-OC-LAYER` | `E12-L` | LEARN | NONE | NONE | - | C | PROVISIONAL_NON_GOVERNING | DRAFT | AQ-V4-01 | `49dada761c51` |
| `EV-L-OC-LAYER-PRESENT` | `E12-L` | LEARN | NONE | NONE | - | C | PROVISIONAL_NON_GOVERNING | DRAFT | AQ-V4-01 | `cb0ce9041d9d` |
| `EV-L-OC-TEXT` | `E12-L` | LEARN | NONE | NONE | - | C | PROVISIONAL_NON_GOVERNING | DRAFT | AQ-V4-01 | `0da8afe3bb22` |
| `EV-L-OC-DIM` | `E12-L` | LEARN | NONE | NONE | - | C | PROVISIONAL_NON_GOVERNING | DRAFT | AQ-V4-01 | `1a38f198fd9b` |
| `EV-L-OC-ENVELOPE` | `E12-L` | LEARN | NONE | NONE | - | C | PROVISIONAL_NON_GOVERNING | DRAFT | AQ-V4-01 | `8f57df2dfaa4` |
| `EV-L-OC-IMPORT` | `E12-L` | LEARN | NONE | NONE | - | C | PROVISIONAL_NON_GOVERNING | DRAFT | AQ-V4-01, K-8 | `2d6fe2dff5a3` |
| `EV-L-OC-REF-TOP` | `E12-L` | LEARN | NONE | NONE | - | C | PROVISIONAL_NON_GOVERNING | DRAFT | AQ-V4-01 | `8ae8bd056e80` |
| `EV-L-OC-VERIFY-READ` | `E12-L` | LEARN | NONE | NONE | - | C | PROVISIONAL_NON_GOVERNING | DRAFT | AQ-V4-01 | `bf013f390f2c` |
| `EV-L-COMPOSED` | `E12-L` | LEARN | NONE | NONE | - | C | PROVISIONAL_NON_GOVERNING | DRAFT | AQ-V4-01 | `d7ae7839f811` |
| `EV-V-FX-2F` | `E12-V` | CP | NONE | NONE | - | P | O3 SUCCESS | DRAFT | K-3 | `f95a79b83c9e` |
| `EV-V-FX-4F` | `E12-V` | CP | NONE | NONE | - | P | O3 SUCCESS | DRAFT | K-3 | `50b1178bdd0f` |
| `EV-V-FX-BIG` | `E12-V` | CP | NONE | NONE | - | P | O3 SUCCESS | DRAFT | K-3 | `a27d14e9f5ab` |
| `WR-A-X0` | `SC-A` | CP | X0 | S2 | - | C | O4 (semantic comparison detects a persistent change) | DRAFT | K-3 | `c437b84197ad` |
| `WR-A-X1` | `SC-A` | CP | X1 | S2 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `bfc0e3e2660b` |
| `WR-A-X2` | `SC-A` | CP | X2 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `049543405f2a` |
| `WR-A-X3` | `SC-A` | CP | X3 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `40ad99e645b1` |
| `WR-A-X4` | `SC-A` | CP | X4 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `f5e2f599042f` |
| `WR-A-X5` | `SC-B` | CP | X5 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 | `eba971b65fa9` |
| `WR-A-X6` | `SC-B` | CP | X6 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 | `4db840e1d3bb` |
| `WR-A-X7` | `SC-B` | CP | X7 | NONE | Y | C | O3 with RESIDUAL_MISSED_CHANGE (X7 is outside W-scan and phase C); ... | DRAFT | K-3 | `267bd30ea3fb` |
| `WR-A-X8` | `SC-B` | CP | X8 | NONE | Y | C | O3; outside the guarantee (W3); recorded for the disclosure only | DRAFT | K-3 | `6d506aa09a60` |
| `WR-D-X0` | `SC-A` | CP | X0 | S2 | - | C | O4 (semantic comparison detects a persistent change) | DRAFT | K-3 | `c44a0a8f0d68` |
| `WR-D-X1` | `SC-A` | CP | X1 | S2 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `11bc94ab4ed6` |
| `WR-D-X2` | `SC-A` | CP | X2 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `dec417d90cf9` |
| `WR-D-X3` | `SC-A` | CP | X3 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `90f617c2b858` |
| `WR-D-X4` | `SC-A` | CP | X4 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `9ce7490baf47` |
| `WR-D-X5` | `SC-B` | CP | X5 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 | `b85686209ffc` |
| `WR-D-X6` | `SC-B` | CP | X6 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 | `0e90784e3089` |
| `WR-D-X7` | `SC-B` | CP | X7 | NONE | Y | C | O3 with RESIDUAL_MISSED_CHANGE (X7 is outside W-scan and phase C); ... | DRAFT | K-3 | `0f63b30130aa` |
| `WR-D-X8` | `SC-B` | CP | X8 | NONE | Y | C | O3; outside the guarantee (W3); recorded for the disclosure only | DRAFT | K-3 | `b0e56a3d77c5` |
| `WR-E-X0` | `SC-A` | CP | X0 | S2 | - | C | O4 (semantic comparison detects a persistent change) | DRAFT | K-3 | `47c6c9af9d8b` |
| `WR-E-X1` | `SC-A` | CP | X1 | S2 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `cf366f520746` |
| `WR-E-X2` | `SC-A` | CP | X2 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `dc1e59e220ac` |
| `WR-E-X3` | `SC-A` | CP | X3 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `0b4b6f823519` |
| `WR-E-X4` | `SC-A` | CP | X4 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `93305bc5e096` |
| `WR-E-X5` | `SC-B` | CP | X5 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 | `d87fa2e8986c` |
| `WR-E-X6` | `SC-B` | CP | X6 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 | `8d6bde9dc37c` |
| `WR-E-X7` | `SC-B` | CP | X7 | NONE | Y | C | O3 with RESIDUAL_MISSED_CHANGE (X7 is outside W-scan and phase C); ... | DRAFT | K-3 | `2e59892dbf20` |
| `WR-E-X8` | `SC-B` | CP | X8 | NONE | Y | C | O3; outside the guarantee (W3); recorded for the disclosure only | DRAFT | K-3 | `d701c29ac5c8` |
| `WR-F-X0` | `SC-A` | CP | X0 | S2 | - | C | O4 (semantic comparison detects a persistent change) | DRAFT | K-3 | `99150985c524` |
| `WR-F-X1` | `SC-A` | CP | X1 | S2 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `f6d87cb5afff` |
| `WR-F-X2` | `SC-A` | CP | X2 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `62a620d74e85` |
| `WR-F-X3` | `SC-A` | CP | X3 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `4a166665b347` |
| `WR-F-X4` | `SC-A` | CP | X4 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `76b7a1d46220` |
| `WR-F-X5` | `E8` | CP | X5 | E08 | - | C | O7 (E-08 FAIL at PC: the read-set change persists at PC; 17.2 row 7... | DRAFT | K-3 | `404101e15165` |
| `WR-F-X6` | `E8` | CP | X6 | E08 | - | C | O7 (E-08 FAIL at PC: the read-set change persists at PC; 17.2 row 7... | DRAFT | K-3 | `b19720a2088b` |
| `WR-F-X7` | `E8` | CP | X7 | E08 | - | C | O7 (E-08 FAIL at PC; the change persists at PC) (AR2-14); POST_V1_T... | DRAFT | K-3 | `2e4cd3b0ba09` |
| `WR-F-X8` | `SC-B` | CP | X8 | NONE | Y | C | O3; outside the guarantee (W3); recorded for the disclosure only | DRAFT | K-3 | `38b046f8e4ca` |
| `WR-G-X0` | `SC-A` | CP | X0 | S2 | - | C | O4 (semantic comparison detects a persistent change) | DRAFT | K-3 | `3515f0e52289` |
| `WR-G-X1` | `SC-A` | CP | X1 | S2 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `377597cb475b` |
| `WR-G-X2` | `SC-A` | CP | X2 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `9f86050a6dd6` |
| `WR-G-X3` | `SC-A` | CP | X3 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `b3b5a01657dd` |
| `WR-G-X4` | `SC-A` | CP | X4 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `6686d3085add` |
| `WR-G-X5` | `E8` | CP | X5 | E08 | - | C | O7 (E-08 FAIL at PC: the read-set change persists at PC; 17.2 row 7... | DRAFT | K-3 | `4df3d9353c7c` |
| `WR-G-X6` | `E8` | CP | X6 | E08 | - | C | O7 (E-08 FAIL at PC: the read-set change persists at PC; 17.2 row 7... | DRAFT | K-3 | `d3b193782bb8` |
| `WR-G-X7` | `E8` | CP | X7 | E08 | - | C | O7 (E-08 FAIL at PC; the change persists at PC) (AR2-14); POST_V1_T... | DRAFT | K-3 | `75254af11dea` |
| `WR-G-X8` | `SC-B` | CP | X8 | NONE | Y | C | O3; outside the guarantee (W3); recorded for the disclosure only | DRAFT | K-3 | `9afe4de42747` |
| `WR-B-X0` | `SC-B` | CP | X0 | NONE | Y | C | O3 with RESIDUAL_MISSED_CHANGE (ABA restored inside the commit boun... | DRAFT | K-3 | `e577900da07f` |
| `WR-B-X3` | `SC-B` | CP | X3 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records a delivered phas... | DRAFT | K-3 | `849965868719` |
| `WR-B-X7` | `SC-B` | CP | X7 | NONE | Y | C | O3 with RESIDUAL_MISSED_CHANGE (ABA between V1 and PC; nothing pers... | DRAFT | K-3 | `6a3b8eb83eef` |
| `WR-C-X5` | `SC-B` | CP | X5 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 | `5bcb275ec66b` |
| `WR-C-X6` | `SC-B` | CP | X6 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 | `a098940aee50` |
| `WR-C-X7` | `SC-B` | CP | X7 | NONE | Y | C | O3 with RESIDUAL_MISSED_CHANGE (X7 is outside W-scan and phase C); ... | DRAFT | K-3 | `1424e7b3c386` |
| `WR-SA-X1` | `SC-A` | CP | X1 | S2 | - | C | O4 (semantic comparison detects; no event delivered) | DRAFT | K-3 | `89f0a275c76d` |
| `WR-SA-X2` | `SC-A` | CP | X2 | S3 | - | C | O4 (semantic comparison detects; no event delivered) | DRAFT | K-3 | `29dca4c8a85b` |
| `WR-SA-X3` | `SC-A` | CP | X3 | S3 | - | C | O4 (semantic comparison detects; no event delivered) | DRAFT | K-3 | `35e03f8f3f38` |
| `WR-SA-X4` | `SC-A` | CP | X4 | S3 | - | C | O4 (semantic comparison detects; no event delivered) | DRAFT | K-3 | `9e28e9c8f379` |
| `WR-SB-X3` | `SC-B` | CP | X3 | NONE | Y | C | O3 with RESIDUAL_MISSED_CHANGE (silent writer: neither channel) | DRAFT | K-3 | `f772024049b9` |
| `WR-SC-X5` | `SC-B` | CP | X5 | NONE | Y | C | O3 with RESIDUAL_MISSED_CHANGE (silent writer: neither channel) | DRAFT | K-3 | `85482f43a621` |
| `WR-SC-X6` | `SC-B` | CP | X6 | NONE | Y | C | O3 with RESIDUAL_MISSED_CHANGE (silent writer: neither channel) | DRAFT | K-3 | `935350924986` |
| `WR-A-X1-EV` | `SC-A` | CP | X1 | S2 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `77e4f189565b` |
| `WR-A-X2-EV` | `SC-A` | CP | X2 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `67bd08194f94` |
| `WR-A-X3-EV` | `SC-A` | CP | X3 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `121293da7ebc` |
| `WR-A-X4-EV` | `SC-A` | CP | X4 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `1d09c2c85e5a` |
| `WR-A-X5-EV` | `SC-B` | CP | X5 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 | `8544676f0530` |
| `WR-A-X6-EV` | `SC-B` | CP | X6 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 | `6d487df6dc2c` |
| `WR-D-X1-EV` | `SC-A` | CP | X1 | S2 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `958ed286e72c` |
| `WR-D-X2-EV` | `SC-A` | CP | X2 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `e0a6b2210507` |
| `WR-D-X3-EV` | `SC-A` | CP | X3 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `8a2c962522be` |
| `WR-D-X4-EV` | `SC-A` | CP | X4 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `45487c12253a` |
| `WR-D-X5-EV` | `SC-B` | CP | X5 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 | `c0acf7b878ba` |
| `WR-D-X6-EV` | `SC-B` | CP | X6 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 | `69f24fe096c2` |
| `WR-E-X1-EV` | `SC-A` | CP | X1 | S2 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `d577d9d8f7ac` |
| `WR-E-X2-EV` | `SC-A` | CP | X2 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `9a012d97e4c4` |
| `WR-E-X3-EV` | `SC-A` | CP | X3 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `e375f2bac04b` |
| `WR-E-X4-EV` | `SC-A` | CP | X4 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `4d67da9e4009` |
| `WR-E-X5-EV` | `SC-B` | CP | X5 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 | `4dbc89222deb` |
| `WR-E-X6-EV` | `SC-B` | CP | X6 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 | `fd8a69a915b3` |
| `WR-F-X1-EV` | `SC-A` | CP | X1 | S2 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `08f6e579c137` |
| `WR-F-X2-EV` | `SC-A` | CP | X2 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `db87a71021e3` |
| `WR-F-X3-EV` | `SC-A` | CP | X3 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `365a51e1b9db` |
| `WR-F-X4-EV` | `SC-A` | CP | X4 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `d09c0f7b74de` |
| `WR-F-X5-EV` | `E8` | CP | X5 | E08 | - | C | O7 (E-08 FAIL at PC: the read-set change persists at PC; 17.2 row 7... | DRAFT | K-3 | `5d8c823c49ea` |
| `WR-F-X6-EV` | `E8` | CP | X6 | E08 | - | C | O7 (E-08 FAIL at PC: the read-set change persists at PC; 17.2 row 7... | DRAFT | K-3 | `fda74988a5ac` |
| `WR-G-X1-EV` | `SC-A` | CP | X1 | S2 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `4af19a56f260` |
| `WR-G-X2-EV` | `SC-A` | CP | X2 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `718c0e0af410` |
| `WR-G-X3-EV` | `SC-A` | CP | X3 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `99448952502f` |
| `WR-G-X4-EV` | `SC-A` | CP | X4 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `6e232bffebf7` |
| `WR-G-X5-EV` | `E8` | CP | X5 | E08 | - | C | O7 (E-08 FAIL at PC: the read-set change persists at PC; 17.2 row 7... | DRAFT | K-3 | `39a27b9e9dc0` |
| `WR-G-X6-EV` | `E8` | CP | X6 | E08 | - | C | O7 (E-08 FAIL at PC: the read-set change persists at PC; 17.2 row 7... | DRAFT | K-3 | `f020caf52831` |
| `WR-B-X3-EV` | `SC-B` | CP | X3 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records a delivered phas... | DRAFT | K-3 | `65a62c085f7e` |
| `WR-C-X5-EV` | `SC-B` | CP | X5 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 | `2c8f2ca07d90` |
| `WR-C-X6-EV` | `SC-B` | CP | X6 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 | `247ff5e3542e` |
| `U-1` | `U-1..U-8` | PW | between PP and PS | NONE | Y | C | preceding run: O2 (CER-ABV); then the UNDO sequence | DRAFT | K-8 | `79b6352d1654` |
| `U-2` | `U-1..U-8` | CP | NONE | NONE | - | P | preceding run: O3 SUCCESS; then the UNDO sequence | DRAFT | K-3 | `b07baf8c5185` |
| `U-3` | `U-1..U-8` | CP | X3 | S3 | - | C | preceding run: O4 (S-3 detects; precedence over O7, 17.2 row 5); th... | DRAFT | K-3 | `4d00eb7c41eb` |
| `U-4` | `U-1..U-8` | CP | X4 | S3 | - | C | preceding run: O5 (an OBS2 element UNKNOWN, 17.2 row 6); then the U... | DRAFT | K-3 | `a024acbdcc8b` |
| `U-5` | `U-1..U-8` | CP | X7 | E11M | - | C | preceding run: O7 (17.2 row 7); then the UNDO sequence | DRAFT | K-3 | `6a4499a69f35` |
| `U-6` | `U-1..U-8` | SYN | NONE | NONE | - | - | O6 (T1 synthetic) | SEALED_CANDIDATE | - | `00780f3f1c19` |
| `U-7` | `U-1..U-8` | CP | NONE | NONE | - | P | preceding run: O3 SUCCESS; then the UNDO sequence | DRAFT | K-3, K-8 | `995d4b9f379e` |
| `U-8` | `U-1..U-8` | CP | NONE | NONE | - | P | preceding run: O3 SUCCESS; then the UNDO sequence | DRAFT | K-3 | `d357bbb26e3c` |
| `U-6b` | `U-6b` | PV | between PV and Commit() | NONE | Y | C | CONDITIONAL (CER-ABV): O2 on an ABORT-VERIFY match; O6 on a mismatc... | DRAFT | K-3 | `2455ce9e0e66` |
| `S1-SAVE-REOPEN` | `S1` | CP | NONE | NONE | - | P | preceding run: O3 SUCCESS | DRAFT | K-3 | `8c1007d840f5` |
| `PP-POSTCP` | `PP` | CP | NONE | NONE | - | P | preceding run: O3 SUCCESS | DRAFT | K-3 | `f9fe7a6e1d6e` |
| `CL-CLEANUP-CHECK` | `CL` | CP | NONE | NONE | - | P | preceding run: O3 SUCCESS | DRAFT | K-3 | `42dec2423d7d` |
| `WU-BOUNDARY` | `WU` | NONE | NONE | NONE | - | P | NONE (no product mutation) | SEALED_PENDING_PIN_CANDIDATE | - | `dccd47a061d2` |

`DS` = `Y` when `DESIGNED_STIMULUS` is not `NONE`. `BUILD`: `P` = `PRODUCT_BUILD`, `C` = `CHARACTERIZATION_BUILD`, `-` = `NONE`.

### 5.2 Per-scenario warm-up dry runs (AR3-02, AR3-24)

V2.1 13.1 defines `WARMUP_COVERAGE_CRITERION` **per scenario**; the rule, the mode and the expectation of the dry runs are those of BA-09 V4 section 4. **Rule (the one rule of BA-09 V4 section 4; AR3-02):** a dry-run scenario `WU-DRY-X` is mandatory for every scenario `X` with `RUN_GOVERNANCE = GOVERNING`, `MODE` = `CHARACTERIZATION_RUN` or `VALIDATION`, and `REACH` in `P0`..`CP` (`PS` included), the evidence-group scenarios included; no other scenario has one. Excluded: the non-governing runs, the learning runs (`MODE = LEARNING`) and the scenarios with `REACH = NONE` (and the synthetic ones, `REACH = SYN`). Each `WU-DRY-X` is in the AR3-01 mode (it only measures loads), has the same plan, fixture specification and injections as `X`, `MODE = DRY_RUN`, `RUN_GOVERNANCE = NON_GOVERNING_BY_DESIGN`, the expectation "no assembly-load event inside the governed window" and the repetition rule `DRY` (`WU_DRY_RUNS = N_B` dry runs). `PIN_SLOTS`: only `FIXTURE_INSTANCE` slots: the fixture of `X` and, where `X` uses one, its variant library (`FIXTURE_INSTANCE@L-VAR-*`). There are **161** of them (the JSON lists each one); no per-family reduction is used. The dry runs of the four `Q-I14-*` vehicles (`WU-DRY-Q-I14-Y`, `WU-DRY-Q-I14-X`, `WU-DRY-Q-I14-SILENT-Y`, `WU-DRY-Q-I14-SILENT-X`) are `DRAFT` and `BLOCKED_BY` AQ-V4-01 like their sources; if the ruling makes those sources non-governing, these dry runs are withdrawn.

**Branches the dry runs do not take.** Where the path of `X` refuses or aborts because of a control of the `RECORDED_NOT_ENFORCED` set, the dry run records that failure and does not take the branch (it keeps running where `X` would stop); the loads of that branch are part of the declared residual of BA-09 V4 section 4. This concerns `NC-E12A`, `NC-E04-R2`, `NC-E03-NOT-HELD`, `NC-E03-REACQUIRE`, `NC-S1-Y1`, `NC-S1-Y1-SILENT` (AQ-V4-02, section 8). Every other branch follows from the injections and from the controls that stay enforced.

Scenarios without a dry run, with the declared reason (AR3-02; BA-09 V4 section 4):

| Reason | Scenarios |
|---|---|
| no product run (REACH NONE) | `Q-I01`, `Q-I02`, `Q-I03`, `Q-I04`, `Q-I05`, `Q-I06`, `Q-I07`, `Q-I08`, `Q-I09`, `Q-I10`, `Q-I11`, `Q-I12`, `Q-I13`, `Q-I14`, `Q-I15`, `Q-I16`, `Q-I09-LOSS`, `Q-I10-LOSS`, `Q-SIGNED-ZERO`, `Q-FPSPEC-VECTORS`, `Q-ROUTE-R1`, `Q-I16-CLEANUP-NEG`, `E6-C1`, `E6-C2`, `E6-C3`, `E6-C5`, `E6-C6`, `E6-C7`, `E6-C8`, `EA1-READONLY-OPENS`, `PR-CLOSURE-COMPLETENESS`, `WU-BOUNDARY` (32) |
| non-governing (EXPLORATORY_NON_GOVERNING) | `E6-C4` (1) |
| no product run (REACH SYN) | `OU-O1`, `OU-O2`, `OU-O6`, `OU-O4`, `OU-O5`, `OU-O7`, `OU-O3`, `OU-ROW1-INVALID`, `OU-PREDICATE`, `AB-COMMIT-EXCEPTION-T1-A`, `AB-COMMIT-EXCEPTION-T1-B`, `U-6` (12) |
| non-governing (NON_GOVERNING_BY_DESIGN) | `E4-LEARN-R1-LM1`, `LK-LM1-PROPS`, `LK-LM1-CONFLICT`, `E4-LEARN-R1-LM2`, `LK-LM2-PROPS`, `LK-LM2-CONFLICT`, `HDM-CHAR-FX1F`, `HDM-CHAR-FXANN`, `HDM-CHAR-FXDIM` (9) |
| learning (MODE LEARNING, REACH LEARN: a micro-scenario, not a mirror run with a governed window) | `EV-L-OC-TM`, `EV-L-OC-BT-OPEN`, `EV-L-OC-BTR-NESTED`, `EV-L-OC-BTR-VIEW`, `EV-L-OC-REF-PIECE`, `EV-L-OC-DYN`, `EV-L-OC-REF-ARRAY`, `EV-L-OC-TM-READ`, `EV-L-OC-LAYER`, `EV-L-OC-LAYER-PRESENT`, `EV-L-OC-TEXT`, `EV-L-OC-DIM`, `EV-L-OC-ENVELOPE`, `EV-L-OC-IMPORT`, `EV-L-OC-REF-TOP`, `EV-L-OC-VERIFY-READ`, `EV-L-COMPOSED` (17) |

## 6. Rulings applied

### 6.1 G-1: the Y injection coordinates (harness level; no contract amendment)

| Coordinate | Point |
|---|---|
| `Y0` | after the PS observation and before the first seam write |
| `Y1` | after the last seam write and before the staged reads |
| `Y2` | after the sealing of `VERIFY_ELEMENT_LIST` and before the PV evaluation |

The injected write uses `T_M` (except `NC-S5-O6`, whose write goes through an `OpenCloseTransaction` by design, AR3-04). Its phase-A event is **conditional** on delivery, resolved by the **I-14 delivery witness** (AR2-12, AR2-18), never by the E-12 log itself. The target-control result and the abort are unconditional (for `NC-S5-O6` the abort is unconditional through E-08 and the S-5 result follows `CER-PERSIST`); O2 or O6 follows `CER-ABV`. Isolation of the target needs the silent variant `Wr-S`, whose availability is **measured** at every coordinate it uses (`Q-I14-SILENT-Y` at Y0, Y1, Y2 and `Q-I14-SILENT-X` at X1..X6).

### 6.2 G-2: tuple identity versus product refusal

| Situation | Result |
|---|---|
| **governed tuple identity mismatch** (any field of V2.1 2.1, as extended by V2.5, differs from the baseline, whatever its origin) | the run is **INVALID**; never a product outcome |
| **session / runtime control failure** (E-02..E-07, E-09, E-10, E-11M, and a subscribe failure at P0 of E-11M or E-12, AR2-30) in a valid run | the product **refuses** (O1 at entry; abort at PV) |
| a subscription **lost** after it was established | the run is **INVALID** (V2.1 21.1); its detection is qualified by `Q-I09-LOSS` and `Q-I10-LOSS` |
| **product plan outside the envelope** | plan-time refusal (O1); outside the CT-21D baseline unless a scenario governs it |
| E-01 or E-13 mismatch **after** the control plane verified the tuple | an inconsistent tuple: **INVALID** (`NC-E01` and `NC-E13` stay withdrawn; `Q-I02` and `Q-I04` qualify the detection) |
| the library swap of `SOURCE_SWAP_CONTROL` | **not** a tuple mismatch: the tuple's library field is sampled once, before the acquisition; the swap is the control's designed stimulus; the original is restored in the cleanup (AR2-31) |

### 6.3 G-3 and G-4

`E6-C4` remains `EXPLORATORY_NON_GOVERNING` (G-3); `NC-S5-O6` is `BLOCKED_BY` its question (AR3-04; whether that blocks the catalog seal is AQ-V4-04). Every scenario with a plan names its fixture; the fixture **instance** is a pin slot (AR2-17); `FX-IMP` is `SPECIFICATION_OPEN` (K-8) and the scenarios that need it, or one of its variants (`FX-IMP-CASE`, `FX-IMP-NESTED`, `FX-IMP-SYMTAB`, `FX-IMP-XREF-HOMONYM`), carry `BLOCKED_BY = K-8` (G-4).

### 6.4 Phase-2 corrections kept from V3 (decisions section 211)

- **X0 (AR2-13):** E-12 is not adjudicated at X0 (`NG-19b`); `E12C` is not exercised; O4 comes from the semantic comparison.
- **Read-set writers (AR2-14):** Wr-G and Wr-F at X5, X6 and X7 give **E-08 FAIL at PC and O7**, unconditionally; at X1..X4 O4 takes precedence and E-08 at PC is a collateral FAIL.
- **Wr-F (AR2-15):** the creation of the consumed `RACKCAD_PROJECT` entry (the fixtures have none, B4).
- **Witness (AR2-12):** every "delivered" condition is resolved by the I-14 delivery witness; `NC-E12C` exists (X6, `CER-WIT`); `WR-B-X7` and `WR-C-X7` exist.
- **KR (AR2-21):** the clean runs and the `KS-*` scenarios record E-14 and E-15 and give `KR` `NO_VERDICT`.
- **Cleanup (AR2-22):** `Q-I16-CLEANUP-NEG` qualifies the detection of an incomplete cleanup.
- **Copy swap (AR2-23) and imports (AR2-24):** the negative copy swap sits between two imports; source swap and clone scenarios use `FX-IMP`.
- **Collaterals and default (AR2-25):** unlisted evaluated controls PASS; `NC-E12A` is the XData write at Y1; `NC-E10` is pinned to 13.1 step 5.
- **E12-L (AR2-27):** one micro-scenario per `OC-*` class of BA-06 V4, with `OC-LAYER-PRESENT` and `OC-VERIFY-READ`.
- **Tuples (AR2-29) and subscription at P0 (AR2-30):** unchanged.

### 6.5 V4 corrections (decisions section 214)

- **AR3-01:** the six pre-EVM runs, `HDM-CHAR-*` and every `WU-DRY-*` use the characterization build in the `RECORDED_NOT_ENFORCED` mode; product outcome `PROVISIONAL_NON_GOVERNING`. The learning micro-scenarios (`EV-L-*`, with the new `EV-L-OC-TM-READ` of BA-06 V4) and the `Q-I14-*` vehicles carry the same fields as a proposal only: they are `DRAFT` and `BLOCKED_BY` AQ-V4-01 (section 1.1).
- **AR3-02, AR3-24:** a dry run for every governing scenario that enters the governed window, by the one rule of BA-09 V4 section 4 (section 5.2); `WU_DRY_RUNS = N_B`; `MAX_ATTEMPTS` per repetition slot; the strictest pair `N_strict` of BA-10 V4 section 2.2 (N_A, N_B or max(N_A, N_B)) applies to every governing scenario that serves a class-A or class-B group (`DEFAULT`, `CLEAN`, `VALIDATE`, `EVIDENCE_STRICT`).
- **AR3-03:** a scenario resolved `NOT_RUNNABLE` leaves its cell uncovered (section 3; BA-02b V4).
- **AR3-04:** `NC-S5-O6` as ruled; `CER-PERSIST` registered.
- **AR3-05:** `DESIGNED_STIMULUS`; designed stimuli have flag `NO`; conditional E-12 phase C cells target `E12C`; `AB-AFTER-MUTATION` targets E-11M (its canary load is a deliberate violation).
- **AR3-06:** `CONTRACT_ASSIGNED_GROUPS`; `PW-CLONE-WRONG-TYPE` exercises CLONE; `PR-CLOSURE-COMPLETENESS` is in group Q with PR-REC assigned by the contract; the derivation is checked mechanically.
- **AR3-07:** `NC-CLONE-REPLACE`.
- **AR3-08:** X1 cells: S-2 and S-3 `difference`; exact silent cells.
- **AR3-09:** U-1, U-3, U-4, U-5, U-6b and EA1 as ruled.
- **AR3-10:** Wr-F and Wr-G at X5..X7 in group E8; SC-B contract-assigned on every X7 cell.
- **AR3-11:** `HDM-CHAR-*` as the source of the `HOST_DEFAULT_MAP` pin.
- **AR3-15:** `CL-CLEAN-FXANNBLANK-FP` and its dry run.
- **AR3-17:** `Q-FPSPEC-VECTORS` evaluates vectors in slot units on binary64 instances generated from BA-08 V4 section 10.
- **AR3-25:** `SEALED_AT` computed for every scenario (section 1.3).
- **AR3-26:** section 7 reclassified.
- **AR3-30:** the minors BA04V3-07..BA04V3-17.

## 7. What prevents sealing the catalog (AR3-26)

### 7.1 Seal blockers (only these)

| Id | Blocking item | Gate |
|---|---|---|
| C-1 | K-3: `TOL_SCALE` has no authority, so no expectation that adjudicates S-1..S-3 can be declared (134 scenarios) | separate decision, test and record (phase-1 ruling; AR2-35); the test needs a future non-governing host gate |
| C-2 | K-8: the construction of `FX-IMP` and its variants (32 scenarios) | fixture-instance gate (AR2-41) |
| C-3 | the Architect rulings still required on this catalog: the delta review of this V4 and the open questions of section 8 (AQ-V4-01..AQ-V4-06, AQ-V4-14); those that block a declared expectation are the rows C-3a..C-3c | Architect |
| C-3a | AQ-V4-01: the `RECORDED_NOT_ENFORCED` set and build of the governing learning runs `EV-L-*` and qualification vehicles `Q-I14-*` (25 scenarios, the 4 dry runs of `Q-I14-*` included) | Architect |
| C-3b | K-6 (AQ-V4-07, Q-HDM-1): the source of the unassigned fields of a created SL-Y layer (`CL-CLEAN-FXANNBLANK-FP`; BA-08 V4 sections 11.2 HDM-8 and 16.1) | Architect (BA-08 V4) |
| C-3c | AQ-V4-03: the contribution of the `KS-*` seam-failure scenarios to group KS (3 scenarios) | Architect |

`E6-C4` blocks only the declaration of `NC-S5-O6` (AR3-04); it is not in the list of AR3-26 and is not read as a seal blocker (AQ-V4-04).

### 7.2 Fixed before the first governing run (pins; not seal blockers)

| Id | Item | Source |
|---|---|---|
| C-4 | fixture **instances** (`FIXTURE_INSTANCE@*`, the variant libraries included) | host work under a future gate, conforming to BA-08 V4 section 12; the variant libraries after the census (K-2) |
| C-5 | `HOST_DEFAULT_MAP` | `HDM-CHAR-*` (AR3-11) |
| C-6 | `E04_ADMITTED_SET@R-1`, `LOCK_MODE`, `EVM_FROZEN` | section 2.2 |

### 7.3 Execution items (not seal blockers)

| Id | Item | Gate |
|---|---|---|
| C-7 | the synthetic log fixtures of the `OU-*` scenarios and of `U-6` | evaluator implementation (EXEC-10); their hashes are `CONTROL_PLANE_TUPLE` fields |
| C-8 | the `CHARACTERIZATION_BUILD` (switches, injection points, the I-14 writers) | implementation; its identity is a tuple field |

### 7.4 Prerequisites of `BASELINE_READY` held by other artifacts (not catalog seal blockers)

| Id | Item | Owner |
|---|---|---|
| C-9 | the repetition values: `PARAM-01`, `PARAM-02`, `PARAM-03` and `WU_DRY_RUNS = N_B` (Owner decisions Q-O1..Q-O4, recorded by the Coordinator); `EVIDENCE_REPETITION` and `PARAM-04` (`MAX_ATTEMPTS`) (Coordinator) | BA-10 V4 |
| C-10 | the designated route R-1 and its delivery (K-5); `Q-ROUTE-R1` measures it | Coordinator ratification (BA-08 V4) |
| C-11 | the artifact-level review and agreement of every expected result, then the candidate ruling and the seal of this artifact (`SEALED_AT` is already inside the bytes, AR3-25) | baseline review (BA-11 V4) |

```text
CATALOG = DRAFT V4 (393 scenarios; 0 sealed)     NB-2 = REMAINS_BLOCKER_FOR_AUTHORITY_BASELINE
CT21D_AUTHORITY_BASELINE_READY = FALSE
```

## 8. Open questions for the Architect

The ids are fixed across the V4 artifacts. This catalog decides none of them; where a ruling is pending, the scenarios carry it in `BLOCKED_BY` (section 7.1) or the text names the id.

- **AQ-V4-01 — the AR3-01 mode for the governing learning runs and qualification vehicles.** AR3-01 rules the mode only for the pre-EVM characterization runs `E4-LEARN-R1-*` and `LK-*` (and, by AR3-11 and AR3-02, `HDM-CHAR-*` and `WU-DRY-*`), all non-governing by design. Its extension to governing runs is not ruled. This catalog proposes the same `RECORDED_NOT_ENFORCED` set, build and outcome for the 17 governing learning runs `EV-L-*` and the 4 governing qualification vehicles `Q-I14-*` (section 1.1 states the set). For `EV-L-*`, E-12 phases A and C recorded, not enforced, is contract-grounded (V2.1 27.2; BA-06 V4 section 9); E-03, E-04 and S-1..S-3 and the build are the proposal (BA-06 V4 CQ-08 is the related question on the build). Until the ruling these scenarios, and the dry runs of `Q-I14-*`, are `DRAFT` with `BLOCKED_BY` AQ-V4-01. If the ruling makes the `Q-I14-*` vehicles non-governing, their dry runs are withdrawn (AR3-02).
- **AQ-V4-02 — branches the dry runs do not take.** AR3-02 puts the dry runs in the AR3-01 mode. For a source scenario whose declared refusal or abort comes from a control of the `RECORDED_NOT_ENFORCED` set (`NC-E12A`, `NC-E04-R2`, `NC-E03-NOT-HELD`, `NC-E03-REACQUIRE`, `NC-S1-Y1`, `NC-S1-Y1-SILENT`), the dry run therefore does not take that branch, and the per-scenario criterion of V2.1 13.1 is measured on a longer path. V4 follows BA-09 V4 section 4, which declares those branches a residual. The alternative, a harness switch of the characterization build that takes the declared branch at the same checkpoint, is not applied without a ruling.
- **AQ-V4-03 — `KS-*` and group KS.** The `KS-*` class (V2.1 26.3: seam rollback and failure typing) aborts at the seam, before PV (`REACH PS`, BA04V3-09), so it reaches neither S-1 nor S-2, the only registry controls whose `AFFECTED_GROUPS` include KS. Under AR2-08 the scenarios then serve AB (through S-5) and KR (NO_VERDICT), not KS, and V4 files them under AB. V2.1 31 lists the `KS-*` class for the S-1..S-3 row, groups SC-A and KS: that membership is recorded in `CONTRACT_ASSIGNED_GROUPS` (evidence only, never PASS), and the three `KS-*` scenarios are `DRAFT` with `BLOCKED_BY` AQ-V4-03. The question is how the seam failure typing contributes to KS.
- **AQ-V4-04 — `E6-C4` and the catalog seal.** AR3-04 makes `NC-S5-O6` `BLOCKED_BY` E6-C4, an exploratory question that only a host characterization answers; AR3-26 lists K-3, K-8 and the Architect rulings as the only seal blockers. V4 reads the two together: E6-C4 does not block the seal of the catalog; `NC-S5-O6` stays `DRAFT` (its expectation not declared) and the S-5 negative-control cell stays uncovered (`NEGATIVE_CONTROL_NOT_EVALUATED` at acceptance, BA-02b V4 rule 5) until a new version of the scenario declares it. The question is whether this reading holds.
- **AQ-V4-05 — cumulative reading of the V2.2 PA-1.4 rows.** PA-1.4 gives "replaced or added rows" without saying which row replaces which. BA-02b V4 section 3 reads every PA-1.4 row together with the V2.1 31 row of the same control, so no V2.1 clause is removed (for E-12 it keeps "validation plans structurally outside the learning corpus"). The question is whether this reading holds.
- **AQ-V4-06 — governing runs on the characterization build.** V2.1 10.6 places the I-14 writers at injection points of a characterization build, and AR3-01 makes the identity of that build a tuple field. V4 therefore gives `BUILD_UNDER_TEST = CHARACTERIZATION_BUILD` to every governing scenario whose injection needs a build switch (125 governing scenarios outside the AR3-01 mode: the scan cells, the Y-coordinate and harness-fault negative controls, the `KS-*` seam failures, the canary loads, the swap pause points), and binds that build through `CHARACTERIZATION_TUPLE` (section 4) as a BUILD_BOUND field under V2.1 2.1 "any payload / observer / harness DLL". The baseline tuple then carries two build identities (product and characterization). The question is whether this reading of V2.1 2.1 and 10.6 holds.
- **AQ-V4-14 — failure-path event classes in governing abort scenarios.** BA-06 V4 declares that its trace plan covers success paths only: the event classes of failure paths (T_M abort after partial writes, POST-WRITE failures, failed or partial imports, ABORT-VERIFY reads) are a residual. In a governing scenario that takes a failure path, E-12 may then meet events that the frozen EVM does not explain (`EVM_INCOMPLETE`, V2.2 PA-10). The governing scenarios outside the AR3-01 mode whose log profile holds an abort verification (`LP-PW-STOP`, `LP-PS`, `LP-PV`) are `NC-E11M-PV`, `NC-E12A`, `NC-E03-REACQUIRE`, `NC-S1-Y1`, `NC-S1-Y1-SILENT`, `NC-E08-Y0`, `NC-E08-Y0-SILENT`, `NC-E08-Y2`, `NC-E08-Y2-SILENT`, `PR-SOURCE-SWAP-NEG-COPY`, `PW-IMPORT-PARTIAL-FAIL`, `AB-AFTER-PREPARE-W`, `AB-AFTER-MUTATION`, `NC-S5-O6`, `NC-CLONE-REPLACE`, `KS-SL1-POSTWRITE`, `KS-SL1-TX-MISMATCH`, `KS-SL4-NEG-DET`, `U-1`, `U-6b`. The question is how their expected E-12 results, or a conditional rule, should reflect that residual. It is recorded as a question only: no expectation of this catalog is changed.

Rulings of other V4 artifacts that this catalog cites: AQ-V4-07 (K-6, Q-HDM-1 of BA-08 V4; `BLOCKED_BY` of `CL-CLEAN-FXANNBLANK-FP`, C-3b); AQ-V4-11 (BA-06 V4 CQ-01..CQ-07: for example CQ-03 on the outcomes of copy re-hash and generation-id failures may change the expectations of scenarios that exercise those cases; none is changed here); AQ-V4-12 (the pin state machine of BA-07 V4, section 2.1); AQ-V4-13 (BA-10 V4 Q-A1 and the `CHAR` repetition intent, section 4).

## 9. Delta V4 (decisions section 214)

| Finding id | Decision | Fix (section / field) |
|---|---|---|
| CLOSURE BA04-F01 (PARTIALLY_FIXED) | AR3-01, AR3-02 | the six pre-EVM runs are in the mode of AR3-01 (no EVM, HOST_DEFAULT_MAP or K-3; `FIXTURE_INSTANCE` only; `PROVISIONAL_NON_GOVERNING`); every `WU-DRY-*` is in the same mode with the `FIXTURE_INSTANCE` slot only (sections 1.1, 2.1, 5.2; JSON) |
| CLOSURE BA04-F08 (PARTIALLY_FIXED) | AR3-01 | `E4-LEARN-R1-*` and `LK-*` reach PC with E-12 phases A and C and S-1..S-3 recorded, not enforced; no phantom EVM; `E4-LEARN-R1-*` MODE = `CHARACTERIZATION_RUN` (section 2.2; JSON) |
| CLOSURE BA04-F09 (PARTIALLY_FIXED) | AR3-09 | U-1 declares its stop as a designed stimulus; U-3, U-4, U-5 declare their targets and collaterals; U-6b is O2/O6 by `CER-ABV`; EA1 has no product run; section 5 claim verified mechanically (JSON; section 5) |
| CLOSURE BA04-F13 (PARTIALLY_FIXED) | AR3-05 | the swap and clone scenarios carry `DESIGNED_STIMULUS` with flag `NO` and no target (section 1; JSON) |
| CLOSURE BA04-F14 (PARTIALLY_FIXED) | AR3-04, AR3-08, AR3-09, AR3-30 | X1 cells declare S-3; U-3..U-5 declare targets and the E-10 collateral; `NC-S5-O6` redesigned; the E-06 collateral of `KS-SL1-TX-MISMATCH` names defined sampling points (JSON) |
| CLOSURE BA04-F20 (PARTIALLY_FIXED) | AR3-30 | `PW-CLONE-WRONG-TYPE` uses the constructible variant `FX-IMP-XREF-HOMONYM`, `FIXTURE_SPEC_PENDING`, `BLOCKED_BY` K-8, `DRAFT` (JSON; cross-note to BA-08 V4 section 12) |
| CLOSURE BA04-F24 (PARTIALLY_FIXED) | AR3-06, AR3-30 | `GROUPS_SERVED` is the derivation and is checked mechanically; `PR-CLOSURE-COMPLETENESS` is in group Q with PR-REC in `CONTRACT_ASSIGNED_GROUPS`; log profiles match the reach; `CER-A2` uses the witness only; U-6 and `Q-FPSPEC-VECTORS` carry notes (sections 1, 3, 4; JSON) |
| BA02-F04 (FIXED in V3) | - | NOT_APPLICABLE: closed in V3; X0 cells unchanged |
| BA04V3-01 [BLOCKING] | AR3-01 | as CLOSURE BA04-F01 and F08: the characterization build, the `RECORDED_NOT_ENFORCED` set and `PROVISIONAL_NON_GOVERNING`; K-3 applies only where S-1..S-3 are adjudicated (sections 1.1, 2, 2.1, 2.2; JSON) |
| BA04V3-02 | AR3-02, AR3-24 | a `WU-DRY-*` for every governing scenario that enters the governed window, the 11 evidence scenarios included (and, since the correction pass, the four `Q-I14-*` vehicles); none for non-governing, learning and no-product-run scenarios (declared in 5.2); `WU_DRY_RUNS = N_B` (sections 4, 5.2; JSON) |
| BA04V3-03 | AR3-08 | the ten X1 cells declare S-2 and S-3 `difference`; `WR-SA-X1` S-2 and S-3 `difference`; `WR-SA-X2..X4` S-3 `difference`, S-2 `equal` (JSON) |
| BA04V3-04 | AR3-09 | U-1, U-3, U-4, U-5, U-6b and `EA1-READONLY-OPENS` as ruled; `INJECTION_COORDINATE` set; section 5 claim checked mechanically (JSON; section 5) |
| BA04V3-05 | AR3-10 | Wr-F and Wr-G at X5..X7 and their EV variants: `GROUP` = E8; every X7 cell of A..G declares SC-B in `CONTRACT_ASSIGNED_GROUPS` (JSON; section 5 counts) |
| BA04V3-06 | AR3-04 | `NC-S5-O6` changes an entity of the source definition: E-08 `FAIL` at PV aborts unconditionally; E-12 phase A conditional on the witness; E-06 not declared; `BLOCKED_BY` E6-C4; `CER-PERSIST` registered (section 3; JSON) |
| BA04V3-07 | AR3-30 | the five rules are registered in both files; `CER-A2` resolves from the I-14 witness only; EA2 declares E-12 phase C conditional and A-2 as a control result (section 3; JSON) |
| BA04V3-08 | AR3-06 | `EVIDENCE_GROUPS_DECLARED` filled for every evidence contribution; `CONTRACT_ASSIGNED_GROUPS` added; `PW-CLONE-WRONG-TYPE` exercises CLONE; mechanical check published (`docs/automation/evidence/I-52-ct21d-ba04-v4-checks.py`) |
| BA04V3-09 | AR3-30 | profiles `LP-PW-STOP`, `LP-PW-REFUSE`, `LP-P0-STEP3`, `LP-P0-STEP4`, `LP-PS`, `LP-MEASURE`, `LP-WU-BOUNDARY` and `LP-CHAR` added; LOCK in `LP-LEARN`; counts in `LP-EVID`; `QUALIFICATION_TUPLE` defined; `KS-*` reach `PS` (section 4; JSON) |
| BA04V3-10 | AR3-30 | `PR-SOURCE-SWAP-NEG-COPY` declares LIBF `UNKNOWN` (V2.1 19.1 UNKNOWN row) (JSON) |
| BA04V3-11 | AR3-30 | the wrong-type variant is `FX-IMP-XREF-HOMONYM` (an FX-IMP base whose absent required name is taken by a resolved xref): K-8, `FIXTURE_SPEC_PENDING`, `DRAFT` (JSON) |
| BA04V3-12 | AR3-24 | EA2 is named in the PARAM-03 sample; `CLEAN` rule symbolic with K = the sample size of the sealed catalog (10 in this draft) (section 4; JSON `PARAM_03_SAMPLE`); the section 213 counts are corrected by the counts of section 5 (for the next decisions entry) |
| BA04V3-13 | AR3-30 | `KS-SL1-TX-MISMATCH`: E-06 declared at the defined sampling points of V2.1 4.2 (the harness transaction lies between two of them) (JSON) |
| BA04V3-14 | AR3-05 | `DESIGNED_STIMULUS` field; designed stimuli carry flag `NO`; `YES` always has a target; conditional E-12 phase C cells target E12C (section 1; JSON) |
| BA04V3-15 | AR3-30 | slots `FIXTURE_INSTANCE@L-VAR-PROXY` and `@L-VAR-DIFF`; the K-2 census dependency declared as the pin source (sections 2.1, 7.2; JSON) |
| BA04V3-16 | AR3-11 | `HDM-CHAR-FX1F`, `HDM-CHAR-FXANN`, `HDM-CHAR-FXDIM` are the source of the `HOST_DEFAULT_MAP` pin, ordered after the LOCK_MODE selection and before any governing run that uses S-1..S-3 (sections 2.1, 2.2; JSON) |
| BA04V3-17 | AR3-30 | `Q-I14-Y`, `Q-I14-X`, `Q-I14-SILENT-Y`, `Q-I14-SILENT-X` run on the fixtures of the scenarios that use them, through a mirror run that reaches the coordinates; the use of the AR3-01 mode for this governing vehicle is a proposal pending AQ-V4-01 (JSON; section 8) |
| D-01 (scenario part) | AR3-15 | `CL-CLEAN-FXANNBLANK-FP` on `FX-ANN-BLANK` with `WU-DRY-CL-CLEAN-FXANNBLANK-FP`; `EV-L-OC-LAYER` text corrected (JSON) |
| D-05 / CLOSURE BA09-02 | AR3-02 | as BA04V3-02 (section 5.2) |
| D-06 | AR3-11 | as BA04V3-16 (sections 2.1, 2.2; JSON) |
| D-19 / BA11V3-13 | AR3-24 | `WU_DRY_RUNS = N_B` (Owner, Q-O2; recorded by the Coordinator) in the `DRY` rule and in section 7.4 |
| BA11V3-02 (BA-04 edge) | AR3-25 | the header `DEPENDS_ON` includes BA-09 |
| BA11V3-04 | AR3-26 | section 7 reclassified: seal blockers = K-3, K-8 and the Architect rulings; instances and `HOST_DEFAULT_MAP` fixed before the first governing run; `OU-*` fixtures are execution items (EXEC-10) |
| BA11V3-05 | AR3-25 | `SEALED_AT` defined (fields, canonical serialization) and computed for every scenario; sealing at artifact level (sections 1.3, 2, 7) |
| BA11V3-11, BA11V3-12 | - | NOT_APPLICABLE to these bytes (decisions record and BA-10); the corrected catalog counts are in section 5 |
| correction pass: AR3-01 extension to governing runs (critic MAJOR; AR3-01 / BA04V3-17 / BA04V3-01) | AR3-01; AQ-V4-01 (not decided) | the 17 `EV-L-*` and the 4 `Q-I14-*` are `DRAFT` with `BLOCKED_BY` AQ-V4-01; their `RECORDED_NOT_ENFORCED`, `BUILD_UNDER_TEST` and outcome fields stay as a labelled PROPOSAL (`NOTE`, `AUTHORITY_SOURCE_FOR_EXPECTED_RESULT`, `EXPECTED_CONTROL_RESULTS`); for `EV-L-*` the contract-grounded part (V2.1 27.2: E-12 phases A and C recorded, not enforced; the same two phases as BA-06 V4 section 9) is stated as such and this catalog states the whole set (no deferral); section 1.1 limits the ruled use of the mode to `E4-LEARN-R1-*`, `LK-*`, `HDM-CHAR-*` and `WU-DRY-*` (contradictory sentence removed); AQ-V4-01 in the `BLOCKED_BY` vocabulary (section 1) and in section 7.1 (C-3a) |
| correction pass: WU-DRY for `Q-I14-*` (critic MAJOR; AR3-02 / D-05) | AR3-02; AQ-V4-01 | `WU-DRY-Q-I14-Y`, `WU-DRY-Q-I14-X`, `WU-DRY-Q-I14-SILENT-Y`, `WU-DRY-Q-I14-SILENT-X` added (AR3-01 mode, `FIXTURE_INSTANCE` slot only, rule `DRY`, `DRAFT`, `BLOCKED_BY` AQ-V4-01 like their sources; withdrawn if the ruling makes the sources non-governing); section 5.2 states the one rule of BA-09 V4 section 4 (the qualifier "outside the AR3-01 mode" removed) and the `PIN_SLOTS` wording of the dry runs (sections 5.2, 7.1; JSON) |
| correction pass: N_strict (critic MAJOR; AR3-24 / BA10-V3-05 / BA10-V3-08) | AR3-24; AQ-V4-13 (not decided) | the `DEFAULT` rule states `N_strict` as BA-10 V4 section 2.2 (N_A if only class A, N_B if only class B, max(N_A, N_B) if both; no ordering assumed); every mixed-class `REPETITION` label reads `max(N_A, N_B)` (`DEFAULT`, `CLEAN`, `VALIDATE`, `EVIDENCE_STRICT`); the `CHAR` rule names `CHAR_RUNS` of BA-10 V4 and each `CHAR` label resolves it; the check derives the whole label by the rule table of BA-10 V4 (section 4; JSON `REPETITION_RULES` and `REPETITION`) |
| correction pass: `CL-CLEAN-FXANNBLANK-FP` and K-6 (critic MAJOR; AR3-15 / AR3-11 / AR3-26) | AR3-26; K-6 (AQ-V4-07, Q-HDM-1; not decided) | `BLOCKED_BY` = K-3, K-6 (AQ-V4-07, Q-HDM-1); O3 kept only as the expectation that holds once the ruling supplies the field source (`NOTE`); K-6 in the `BLOCKED_BY` vocabulary (section 1), in section 7.1 (C-3b) and in the header `PREREQUISITES` |
| correction pass: U-3 S-2 (critic minor; AR3-09 / BA04V3-04) | AR3-09, AR3-30 | `S2` added to `CONTROLS_EXERCISED` of U-3; `GROUPS_SERVED` re-derived (adds KS), `CONTROL_CLASSES_EXERCISED` and `SEALED_AT` recomputed; the check verifies that every registry key of `EXPECTED_CONTROL_RESULTS` is exercised, `NOT_DECLARED`, or `RECORDED_NOT_ENFORCED` of the scenario's own mode (section 1; JSON; check) |
| correction pass: `Q-FPSPEC-VECTORS` and `Q-SIGNED-ZERO` (critic minor; AR3-17 / BA05V3-N08) | AR3-17, AR3-30 | every REJECTED input (rejectedInputs RJ-01..RJ-06) reproduced as REJECTED, and a slot-unit instance that fails the exact-arithmetic check of BA-05 V4 3.3.1 INVALID and reported, never PASS; the input sections exactVectors, rejectedInputs and semanticVectors of the vectors file named; `FPSPEC_CONFORMANCE_TUPLE` binds the BA-08 SEAL_HASH (source of the slot values); `Q-SIGNED-ZERO` cites EV-01, EV-02, SV-05, EV-06 and EV-07 (JSON; section 4; checked against the vectors file) |
| correction pass: `KS-*` and group KS (critic minor; BA04V3-09 consequence / D-07) | AR3-06, AR3-30; AQ-V4-03 (not decided) | the membership that V2.1 26.3 and 31 give the `KS-*` class (groups SC-A and KS) is recorded in `CONTRACT_ASSIGNED_GROUPS` (evidence only, never PASS); the three `KS-*` are `DRAFT` with `BLOCKED_BY` AQ-V4-03 (sections 1, 7.1 C-3c, 8; JSON; BA-02b V4) |
| correction pass: pins (BA-07 V4 cross note; AR3-23) | AR3-23; AQ-V4-12 (not decided) | section 2.1: a pin's seal is the Coordinator + Architect ratification pair citing the pin-record hash, cited by `PIN_RECORDED`; a pin is usable by a governing run only when its `PIN_RECORDED` entry is covered by an `ANCHOR_RECORDED` anchor recorded before the run's `IN_USE` entry; each governing run lists the pin-record hashes it consumes; no BA-11 value is written |
| correction pass: open questions (critic; AR3-30) | - | section 8 lists AQ-V4-01..AQ-V4-06 and AQ-V4-14 with the fixed ids (none decided; AQ-V4-14 is recorded without changing any expectation); sections 1.1, 5.2, 6.3, 7.1 and the JSON cite the ids where a ruling is pending |
| correction pass: `HDM-CHAR-*` text (BA-08 V4 and BA-10 V4 cross notes; AR3-11) | AR3-11, AR3-24 | `HDM-CHAR-*` `PLAN_OR_INPUT`, `EXPECTED_CONTROL_RESULTS` and `NOTE` follow BA-08 V4 11.2 HDM-3 and HDM-4 and the `CHAR_RUNS` rule of BA-10 V4 (N_A runs at the pinned key values plus at least one informative run per varied key, outside the intent); the run on `FX-ANN-BLANK` that HDM-8 proposes is not applied (AQ-V4-07) |

Section numbering: sections 1 to 7 keep their V3 numbers (7 is split into 7.1..7.4); section 8 (open questions) and section 9 (this table) are new. The correction pass adds the rows C-3a..C-3c inside 7.1 and keeps every other row id; section 8 replaces its numbered questions by the fixed ids AQ-V4-xx.
