# I-52 — CT-21D Baseline Artifact BA-04: Scenario Catalog (DRAFT V5)

> **BASELINE ARTIFACT BA-04 V5 — DRAFT, NOT SEALED. Design / documentation only. Nothing here is executed or authorized.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-SCENARIO-CATALOG
> ARTIFACT_VERSION          = 5-DRAFT (supersedes 4-DRAFT, blobs a6587bfb7ca0db1ad485041213f7b813e9012bfc (json) and
>                             3b9774d74afc15d3b9dab4e6b58b9b447c11e725 (md), which stay as history)
> STATUS AUTHORITY          = the BA-11 registry entry of this artifact is the only authority for its status and hashes; this header and the
>                             per-scenario statuses are frozen, non-authoritative text of the candidate bytes
> AUTHORITY_CONTRACT        = CT-21D V2.1 + V2.2 + V2.3 + V2.4 (draft revision 3, textually verified) + V2.5 (draft revision 2); V2.5 pending
>                             Coordinator textual verification
> DELTA RULING APPLIED      = decisions section 217 (AR4-01, AR4-02, AR4-03, AR4-04, AR4-06, AR4-07, AR4-12 (the CQ-02 reading), AR4-13, AR4-14,
>                             AR4-15, AR4-16, AR4-17, AR4-20); section 214 (AR3-01..AR3-11, AR3-15, AR3-17, AR3-23, AR3-24, AR3-25, AR3-26, AR3-30)
>                             and section 211 (AR2-08..AR2-31) where section 217 does not change them; AR3-29 is applied by BA-02b, not by this
>                             artifact
> DEPENDS_ON                = BA-01, BA-02a, BA-03, BA-05, BA-06, BA-07, BA-08, BA-09, BA-10 (entries of the BA-11 registry)
> PREREQUISITES             = K-3 TOL_SCALE decision, test and record; K-8 FX-IMP construction; both before the candidate (the declarations enter the
>                             bytes; they are the seal blockers of section 7.1, AR3-26); the Architect's exact delta review of this V5 (before the
>                             candidate), with the open Architect questions of section 8.2 that can change scenario bytes before the candidate:
>                             AQ-V5-01 (the BUILD_EQUIVALENCE_RECORD for the 111 characterization-build runs that consume EVM_FROZEN), AQ-V5-02
>                             (per-build twins of PR-CLOSURE-COMPLETENESS and WU-BOUNDARY) and AQ-V5-08 (admission of the vehicle Q-I14-P (and
>                             WU-DRY-Q-I14-P) under the Q-I14-* pattern of AR4-01; added by correction pass CP-13; the BLOCKED_BY value of those two
>                             scenarios), and, inherited from BA-07 and BA-10, AQ-V5-04 and AQ-V5-06; no AQ-V4 question remains (all ruled or deferred
>                             by section 217); pin values, the BUILD_EQUIVALENCE_RECORD, the REVIEWED record and the qualification records of each
>                             build (before the first governing run that uses them, not baseline prerequisites: section 7.2)
> BLOCKER                   = NB-2 (REMAINS_BLOCKER_FOR_AUTHORITY_BASELINE)
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```
>
> The machine-readable catalog is [the JSON file](I-52-ct21d-baseline-ba-04-scenario-catalog-v5.json) and carries every field of every scenario; the two files form the artifact. The mechanical checks of this version are `docs/automation/evidence/I-52-ct21d-ba04-v5-checks.py` and its output `docs/automation/evidence/I-52-ct21d-ba04-v5-checks-output.json`. They are bound by hash in the BA-02b entry of the BA-11 registry (its paths), never in the paths of this artifact: the output holds the SHA-256 of BA-02b, which depends on this artifact (AR4-17). The candidate rulings of BA-04 and BA-02b cite both hashes.

## 1. Schema (normative)

Every scenario carries the fields of V2.1 26.1 (with V2.2 PA-3 `EXPECTED_PROCEDURAL_RESULT`) and these fields:

| Field | Content |
|---|---|
| `PURPOSE` | why the scenario exists (V2.1 26.1 `GROUP`, `PURPOSE`) |
| `GROUP` | the primary group (V2.1 26.1; V2.2 PA-1.3); `HDM` labels the four non-governing `HOST_DEFAULT_MAP` characterization scenarios, the name AR4-07 uses; `EV-L-COMPOSED` keeps `E12-L` as the label of its phase and serves no group |
| `RUN_GOVERNANCE` | `GOVERNING`, `NON_GOVERNING_BY_DESIGN` (dry runs; the characterization runs of AR2-11, AR3-11 and AR4-07; the composition check `EV-L-COMPOSED`, AR4-01) or `EXPLORATORY_NON_GOVERNING` |
| `BUILD_UNDER_TEST` | `PRODUCT_BUILD`, `CHARACTERIZATION_BUILD` (section 1.1) or `NONE` (a synthetic scenario with no product process, REACH `SYN`); each value has its own admissible BUILD_BOUND profile (AR4-06 (1)) |
| `RECORDED_NOT_ENFORCED` | the controls and phases that are recorded and do not govern the flow: empty; exactly the set of the AR3-01 mode (section 1.1); or, for `EV-L-*`, the set of AR4-01 (E-04, E-12 phase A, E-12 phase C), each with its contract basis in `EXPECTED_CONTROL_RESULTS` |
| `REACH` | the furthest checkpoint the run is designed to reach on any of its declared branches: `NONE` (no product run), `SYN` (synthetic log), `LEARN` (a run of the learning micro-scenario driver: no mirror command, no checkpoint), `P0`, `PL`, `PR`, `PW`, `PS` (T_M opened; the caller aborts at a seam failure before PV), `PV`, `CP` |
| `INJECTIONS` | the writers or faults (writer, variant, position), or none (V2.1 26.1); writer kinds in section 1.2; a load injection names the assembly or module by a declared identity (AR4-16) |
| `DESIGNED_STIMULUS` | `NONE`, or a short description of a designed stimulus: source swap, clone variant, induced seam failure, stop after PREPARE-W, residual scan cell with no expected control `FAIL`, induced host exception path, dry-run replay. A designed stimulus carries `DELIBERATE_VIOLATION_FLAG = NO` and no target (AR3-05) |
| `DELIBERATE_VIOLATION_FLAG`, `TARGET_NEGATIVE_CONTROL` | `YES` only when the scenario deliberately violates a control; the target is that control (a key of BA-02a), or `INSTRUMENT:I-nn` for the negative control of a qualification; `NO` with target `NONE` otherwise (V2.1 26.1; AR3-05). A cell whose detection is conditional on E-12 phase C has target `E12C` |
| `EXPECTED_EVENTS` | the event class expected per E-12 phase (V2.1 26.1) |
| `EXPECTED_DETECTION_CHANNEL` | the detection channel (V2.1 26.1): `SEMANTIC_COMPARISON`, `E-08_AT_PC`, `E-12`, `BOTH`, `NEITHER`, the target control, or `N/A` |
| `EXPECTED_LOG_RECORD_SET` | a log profile of section 4 (record types and counts), with the per-scenario deltas `LP-INJ`, `LP-WIT` and `LP-PAR` where they apply |
| `CONTROLS_EXERCISED` | the registry controls (keys of BA-02a) whose results the scenario expects, or whose recorded values it characterizes (E-04 in `E4-LEARN-R1-*`, E-03 in `LK-*`). Every other registry key of `EXPECTED_CONTROL_RESULTS` carries `NOT_DECLARED`, or, for a control of the scenario's own `RECORDED_NOT_ENFORCED` set, `RECORDED_NOT_ENFORCED`, or, in an `EV-L-*` scenario, `ENFORCED_NOT_EXERCISED` (the rule below) (checked mechanically). A-1 is exercised by `EA1-READONLY-OPENS` and by the members of the `PARAM-03` clean sample (the `CL-CLEAN-<fixture>` scenarios and `EA2-DEFERRED-EVENTS`), the baseline runs without writers of V2.1 10.6, and by no other scenario (section 3, "A-1 in a mirror run"; R1:BA04V4-09, second option). **Enforced, not exercised (EV-L-*; AR4-01).** In a learning micro-scenario `EV-L-OC-*` and in the composition check `EV-L-COMPOSED`, the controls the run enforces (E-03 under the `LOCK_MODE` pin; every other control, fail-closed, of the manifest-attested environment of V2.1 27.2 step 2) are entry conditions of the learning environment, not results the scenario characterizes: they are **not** in `CONTROLS_EXERCISED`, a registry key of one of them in `EXPECTED_CONTROL_RESULTS` carries a value that begins with `ENFORCED_NOT_EXERCISED`, their `PASS` is never coverage, and their `FAIL` refuses the run and counts toward the control's group (BA-02b V5 rule 2). Authority: AR4-01, which rules these runs to be governing runs **of the evidence group E12-L** (V2.2 PA-3: verdict `EVIDENCE_COMPLETE`), E-03 enforced and every other control fail-closed; exercising E-03 would add LK to `GROUPS_SERVED` against that assignment (checked mechanically) |
| `CONTROL_CLASSES_EXERCISED` | the registry classes of those controls (`A`, `B`), plus `NOT_A_CONTROL` when the scenario exercises no control, declares an evidence group or has a contract-assigned group (V2.1 26.1 and 31 check 3); derived |
| `EVIDENCE_GROUPS_DECLARED` | the groups of kind `EVIDENCE` (V2.2 PA-1.3) the scenario serves |
| `GROUPS_SERVED` | **Normative definition of this artifact** (AR4-17; authority AR2-08): GROUPS_SERVED(s) = the ordered union of AFFECTED_GROUPS(c) of the BA-02a registry for every control c of CONTROLS_EXERCISED(s), taken in the order of CONTROLS_EXERCISED, followed by the groups of EVIDENCE_GROUPS_DECLARED(s) not already present; nothing else. A control that the run only records (RECORDED_NOT_ENFORCED) or does not reach is not in CONTROLS_EXERCISED |
| `CONTRACT_ASSIGNED_GROUPS` | **Normative definition of this artifact** (AR4-17; authority AR3-06 as extended by AR4-03): a list of {group, clause}; each entry is a CONTROL group of V2.2 PA-1.3 that the contract assigns to the scenario through one of these sources, which the clause cites: V2.1 10.8 = an X7 scan cell: the tail log POST_V1_TO_CP_QUEUE_LOG (a NOT_A_CONTROL item) to SC-B (AR3-06, AR3-10); V2.1 10.6 and 21.4 = a residual scan cell to SC-B (AR3-06); V2.2 PA-12 = the qualification control CLOSURE_COMPLETENESS_CONTROL to PR-REC (AR3-06); AR4-03 = a scenario class that V2.1 26.3 and 31 list for a group whose controls the scenario cannot reach by design: the KS-* class to KS and SC-A (the AR4-03 extension of AR3-06). A contract-assigned group is never in GROUPS_SERVED; it is evidence only and never PASS; it is never counted as serving or countable coverage; it never enters N_strict (AR4-14 (a)); its expected group verdict says "evidence only, never PASS" |
| `PIN_SLOTS` | named slots filled by pin records outside the catalog (section 2.1) |
| `RUN_PREREQUISITES` | named records that must exist before the run and are neither pins nor seal blockers (the JSON `RUN_PREREQUISITE_VOCABULARY`): `BUILD_EQUIVALENCE_RECORD` = the recorded, verified equivalence of the CHARACTERIZATION_BUILD and the PRODUCT_BUILD (AR4-06 (4); section 1.1): a prerequisite of every PRODUCT_BUILD run that consumes a pin derived on the characterization build (E04_ADMITTED_SET@R-1, LOCK_MODE, HOST_DEFAULT_MAP); location, content schema (recordKind CT21D_BUILD_EQUIVALENCE) and custody: BA-07 V5 sections 2, 4 and 5.3; use precondition: BA-07 V5 section 7, U5; `REVIEWED_RECORD` = the REVIEWED inventory record of V2.4 (BA-06 V5 section 9), a review record in BA-07 custody: its REVIEW_RECORD_ADDED entry has an index at most the h of the run-start anchor of the run (IN_USE.runStartAnchor; BA-07 V5 sections 6.1 and 7, U1), so REVIEWED is anchored in custody before the run starts (V2.4: the first LEARNING run requires REVIEWED); named by every EV-L-* scenario, whose log profile carries REVIEWED_REF (section 4); `QUALIFICATION_RECORD@Q-I14-X` = the sealed Q-I14-X qualification record of the characterization build: the I-14 writers and the delivery witness at X0..X8; `QUALIFICATION_RECORD@Q-I14-Y` = the sealed Q-I14-Y qualification record of the characterization build: the I-14 writers, Wr-OCT and XDATA_WRITE, the delivery witness and the independent post-abort read at Y0..Y2 (AR4-01, AR4-04, AR4-16); `QUALIFICATION_RECORD@Q-I14-P` = the sealed Q-I14-P qualification record of the characterization build: the I-14 writer Wr-G and the delivery witness at the PREPARE-phase coordinate between PR and PW (the coordinate of PR-REC-REOBS-MISMATCH); `QUALIFICATION_RECORD@Q-I14-SILENT-X` = the sealed Q-I14-SILENT-X record: availability of the silent write at X1..X6 (CER-SIL); `QUALIFICATION_RECORD@Q-I14-SILENT-Y` = the sealed Q-I14-SILENT-Y record: availability of the silent write at Y0..Y2 (CER-SIL). **Citation (the JSON `RUN_PREREQUISITE_RULE`):** Every run of a scenario cites, in its first log record (RUN_START, or QUAL_START for a qualification run), the name and the SHA-256 of every record that the scenario's RUN_PREREQUISITES names (for a learning run, the REVIEWED_REF record of LP-LEARN is that citation for the REVIEWED_RECORD). A run that does not cite a named record, or whose cited record is not in the state the vocabulary requires, does not meet the prerequisite: for the BUILD_EQUIVALENCE_RECORD, the pins derived on the characterization build do not apply to it (AR4-06 (4)); for the REVIEWED_RECORD, the run does not meet V2.4 and does not enter the learning corpus (BA-06 V5 section 9); for a qualification record, the instrument is not qualified for the run (V2.1 4.3: no PASS). The BUILD_EQUIVALENCE_RECORD and the REVIEWED_RECORD are review records in BA-07 custody whose REVIEW_RECORD_ADDED entry has an index at most the h of the run-start anchor of the run: for the BUILD_EQUIVALENCE_RECORD, the use precondition of BA-07 V5 section 7, U5 (the record and its custody: BA-07 V5 section 5.3; the SHA-256 it is cited by is its record hash, BA-07 V5 section 4); for the REVIEWED_RECORD, BA-07 V5 sections 5.1, 6.1 and 7, U1; the qualification records are the sealed records of the named scenarios on the build of the run |
| `BLOCKED_BY` | open items that prevent declaring the expectation; the vocabulary (the JSON `BLOCKED_BY_VOCABULARY` holds the same entries): `K-3` = TOL_SCALE has no authority (BA-08 V5 K-3): no expectation that adjudicates S-1..S-3 can be declared; `K-8` = FX-IMP and its variants are SPECIFICATION_OPEN (BA-08 V5 K-8); `AQ-V5-08` = the open Architect item on the admission of the vehicle Q-I14-P (and WU-DRY-Q-I14-P) under the Q-I14-* pattern of AR4-01; added by correction pass CP-13 (section 8.2; resolved inside C-3, section 7.1): the expectation of Q-I14-P and WU-DRY-Q-I14-P, the only scenarios that carry it, cannot be declared until it is ruled |
| `CONDITIONAL_EXPECTATION_RULE` | the declared rules of section 3 that the scenario uses, or `NONE` |
| `SEALED_AT` | the SHA-256 of the canonical serialization of the scenario's expectation fields (section 1.3; AR3-25) |
| `NOTE` | informative text |

### 1.1 Builds under test, build profiles, per-build qualification and the RECORDED_NOT_ENFORCED sets

**Characterization build.** A build of the product at the exact source SHA with harness-only switches and injection points, disabled unless a scenario names them (like the I-14 writer builds of V2.1 4.3 and 10.6); its build receipt enumerates the switch and injection-point set of section 1.2. A scenario uses the `CHARACTERIZATION_BUILD` when it is in the AR3-01 mode, when one of its injections needs a switch or an injection point, or when a rule of section 3 resolves from an I-14 record (I-14 exists only there: V2.1 4.3 row I-14); otherwise the `PRODUCT_BUILD`.

**One admissible BUILD_BOUND profile per build (AR4-06 (1), (2); read as the Architect's reading, no errata).** The baseline records a profile for each `BUILD_UNDER_TEST` value, and every run is compared (V2.1 2.2) with the profile of its own build: `FULL_BASELINE_TUPLE` for the `PRODUCT_BUILD` and `CHARACTERIZATION_TUPLE` for the `CHARACTERIZATION_BUILD` (section 4, and the JSON `BUILD_PROFILES`). `CHARACTERIZATION_TUPLE` holds its own value for every build-dependent BUILD_BOUND field: the Plugin and harness DLL SHA-256, the manifest build layer (the exact E-10 set of V2.1 12.3 and the WARMUP_MEMBERSHIP set of 13.1 of that build), the warm-up definition where it differs, the qualification records of that build (V2.1 4.3) and `CATALOG_FOLDER` of the build under test (V2.5). **The one `PRODUCT_BUILD` profile** (correction pass): the learning micro-scenario driver is a declared harness DLL of the one PRODUCT_BUILD profile (V2.1 2.1 "any payload / observer / harness DLL"; AR4-01): present in every PRODUCT_BUILD run, part of the E-10 known-module set (V2.1 12.3) and of the WARMUP_MEMBERSHIP set (V2.1 13.1) of its manifest, and inert outside EV-L-*. There is no separate learning or validation profile: the `EV-L-*` and `EV-V-*` runs, like every other `PRODUCT_BUILD` run, are compared with `FULL_BASELINE_TUPLE`, whose text carries this clause.

**Per-build qualification (AR4-06 (3)).** Every `Q-*` and `E6-C*` scenario runs on each build under test that governing runs use: the `PRODUCT_BUILD` scenario stays and a twin `<id>-CB` runs on the `CHARACTERIZATION_BUILD` with the same expectation and a `QUALIFICATION_TUPLE` that names the build. There are **29** twins (`Q-I01-CB`, `Q-I02-CB`, `Q-I03-CB`, `Q-I04-CB`, `Q-I05-CB`, `Q-I06-CB`, `Q-I07-CB`, `Q-I08-CB`, `Q-I09-CB`, `Q-I10-CB`, `Q-I11-CB`, `Q-I12-CB`, `Q-I13-CB`, `Q-I15-CB`, `Q-I16-CB`, `Q-I09-LOSS-CB`, `Q-I10-LOSS-CB`, `Q-SIGNED-ZERO-CB`, `Q-FPSPEC-VECTORS-CB`, `Q-ROUTE-R1-CB`, `Q-I16-CLEANUP-NEG-CB`, `E6-C1-CB`, `E6-C2-CB`, `E6-C3-CB`, `E6-C4-CB`, `E6-C5-CB`, `E6-C6-CB`, `E6-C7-CB`, `E6-C8-CB`). The I-14 family (`Q-I14`, `Q-I14-Y`, `Q-I14-X`, `Q-I14-SILENT-Y`, `Q-I14-SILENT-X`, `Q-I14-P`) runs **only** on the `CHARACTERIZATION_BUILD` (V2.1 4.3 row I-14) and has no product twin. No transfer of a qualification between builds is ruled.

**Build equivalence (AR4-06 (4)).** BUILD_EQUIVALENCE_RECORD (AR4-06 (4)): the recorded and verified equivalence of the CHARACTERIZATION_BUILD and the PRODUCT_BUILD. Its location, content schema (recordKind CT21D_BUILD_EQUIVALENCE) and custody: BA-07 V5 sections 2, 4 and 5.3; use precondition: BA-07 V5 section 7, U5. BA-07 V5 is the single authority for the record (BA-07 is in DEPENDS_ON) and this catalog restates none of them. The rules of this catalog: the pins derived on the characterization build (E04_ADMITTED_SET@R-1 from E4-LEARN-R1-*, LOCK_MODE from LK-*, HOST_DEFAULT_MAP from HDM-CHAR-*) apply to a PRODUCT_BUILD run only under this record, and the E4-VAL-R1 membership post-check stays the safeguard (AR4-06 (4)); every PRODUCT_BUILD scenario that consumes such a pin names the record in RUN_PREREQUISITES; every run of such a scenario cites the record hash (BA-07 V5 section 4) in its RUN_START record (RUN_PREREQUISITE_RULE, section 4). It is not a pin, not a tuple field and not a seal blocker of this catalog. **48** `PRODUCT_BUILD` scenarios name it in `RUN_PREREQUISITES`. `EA2-DEFERRED-EVENTS` runs on the `CHARACTERIZATION_BUILD` (AR4-06 (6)); its contribution to `PARAM-03` is valid for the product build only under the same record. Scenarios whose injections need no switch stay on the `PRODUCT_BUILD` (AR4-06 (5)).

**The AR3-01 mode** (decisions section 214, AR3-01): `BUILD_UNDER_TEST = CHARACTERIZATION_BUILD` and `RECORDED_NOT_ENFORCED = ["E-03", "E-04", "E-12 phase A", "E-12 phase C", "S-1", "S-2", "S-3"]`. These controls and phases are recorded and do not govern the flow, so the run reaches PV, `Commit()`, V1 and PC wherever its path does. The product outcome is `PROVISIONAL_NON_GOVERNING` (never O1..O7). The run has no dependency on EVM, `HOST_DEFAULT_MAP` or K-3. Every other control stays fail-closed (V2.1 3.2), so a path that an enforced control aborts is declared as such (the `Q-I14-*` vehicles, section 5.1).

**Where the mode applies (by ruling; `Q-I14-P` by the pattern of AR4-01, open item AQ-V5-08).** `E4-LEARN-R1-*` and `LK-*` (AR3-01), the four `HDM-CHAR-*` (AR3-11; the fourth by AR4-07), every `WU-DRY-*` (AR3-02), and the governing qualification vehicles `Q-I14-Y`, `Q-I14-X`, `Q-I14-SILENT-Y`, `Q-I14-SILENT-X`, `Q-I14-P`, whose product result is not adjudicated: the verdict comes from the writer-effect and witness records against the independent read (AR4-01). **177** scenarios: 175 by ruling, none of them a proposal; `Q-I14-P` and `WU-DRY-Q-I14-P` apply AR4-01 through its `Q-I14-*` pattern; they were added by the correction pass, and their admission is the open Architect item AQ-V5-08 (section 8.2; both carry it in `BLOCKED_BY`). The vehicles keep their dry runs (`WU-DRY-Q-I14-Y`, `WU-DRY-Q-I14-X`, `WU-DRY-Q-I14-SILENT-Y`, `WU-DRY-Q-I14-SILENT-X`, `WU-DRY-Q-I14-P`).

**The learning set of `EV-L-*` (AR4-01).** The 16 learning micro-scenarios `EV-L-OC-*` are governing runs of the evidence group E12-L on the `PRODUCT_BUILD`, the build of the `EV-V-*` validation; the micro-scenario driver is a declared harness DLL of the one `PRODUCT_BUILD` profile (V2.1 2.1): present in every `PRODUCT_BUILD` run, part of its E-10 and WUM sets, inert outside `EV-L-*`, so BA-06 CQ-08 has no object (V2.1 27.5). They are **not** in the AR3-01 mode. `RECORDED_NOT_ENFORCED` = ["E-04", "E-12 phase A", "E-12 phase C"], with the basis per control: E04: RECORDED_NOT_ENFORCED by V2.1 3.3 (not as an extension of AR3-01): the admitted set is defined per checkpoint (P0, PL, PV, PC) for the designated route of the mirror command, and a micro-scenario is neither; the D-1..D-6 values are recorded (AR4-01); E12A: RECORDED_NOT_ENFORCED by V2.1 27.2 (learning records the raw E-12 events), 14.3 (the model level is frozen after learning) and V2.2 PA-2 (only a FROZEN model is used by governing validation): no EVM exists to adjudicate against (AR4-01); E12C: RECORDED_NOT_ENFORCED by V2.1 27.2, 14.3 and V2.2 PA-2, as E-12 phase A (AR4-01). E-03 is enforced under the `LOCK_MODE` pin and every other control is fail-closed, all `ENFORCED_NOT_EXERCISED` (section 1): not in `CONTROLS_EXERCISED`, never coverage; S-1..S-3 are `NOT_EVALUATED` (AR2-25); `EXPECTED_PRODUCT_OUTCOME = NONE`. Every `EV-L-*` names the `REVIEWED_RECORD` (V2.4; BA-06 V5 section 9) and cites it in its `REVIEWED_REF` record (`LP-LEARN`). `EV-L-COMPOSED` leaves the learning corpus and `LEARNING_CORPUS_HASH`: it is a `NON_GOVERNING_BY_DESIGN` composition check against the DRAFT or UNDER_REVIEW model that contributes no model entry, with the outcome `NONE (a non-governing composition check driven by the micro-scenario driver: no mirror product run; AR4-01)`; the events no isolated entry explains are recorded as `UNEXPLAINED_EVENTS` (V2.1 27.2 step 4; BA06V4-N04).

### 1.2 Injection writer kinds and the build under test

| Writer kind | Meaning |
|---|---|
| `Wr-A..Wr-G, Wr-S` | the I-14 adversarial writers of V2.1 10.6 at an X or Y position (injection point of the characterization build) (build switch) |
| `Wr-OCT` | a write through an OpenCloseTransaction at a Y position (NC-S5-O6, AR3-04); an I-14 writer qualified at Y1, with the delivery witness and the independent post-abort read, by Q-I14-Y (AR4-01) (build switch) |
| `XDATA_WRITE` | an XData write of a registered test application through T_M at a Y position (NC-E12A); an I-14 writer qualified at Y1, with the delivery witness, by Q-I14-Y (AR4-01, AR4-16) (build switch) |
| `HARNESS_FAULT` | a harness-only fault switch in the product or instrument path (a stop, a suppressed lock, a forced UNKNOWN read, a failing import, a failing seam, an ended transaction, a refused subscription, a foreign warm-up load); a foreign load names the assembly by a declared identity (AR4-16) (build switch) |
| `CANARY_LOAD` | a canary assembly or module load at a declared point of the run; the canary is named by a declared identity (for an assembly the E-11M identity of V2.1 13.2 and its file SHA-256, for a module the entry identity of V2.1 12.2), recorded in the build receipt of the characterization build (AR4-16) (build switch) |
| `SOURCE_SWAP, COPY_MODIFY, SOURCE_MODIFY_DURING_ACQUISITION` | a control-plane change of the library file or of the private copy at a pause point of the characterization build (build switch) |
| `CLONE_MODE_SWITCH` | a switch that forces the effective cloning mode of an import outside the allow-list while the importer's declaration stays on it (NC-CLONE-REPLACE, AR3-07) (build switch) |
| `CONFLICT_PROBE` | an adversarial probe from another execution context (LK-*-CONFLICT) (build switch) |
| `DRY_RUN_BRANCH_SWITCH` | a switch used only by a WU-DRY-* dry run whose source refuses or aborts because of a control of the RECORDED_NOT_ENFORCED set: it records the value of that control and then performs the refusal or abort that the source declares, at the same checkpoint (AR4-02; section 5.2) (build switch) |
| `CONTROL_PLANE_SETUP` | the control plane prepares the session before P0 (a second document, a persistent mode, an open transaction, an overrule registration, a non-designated route, a changed context variable of an informative HDM-CHAR-* run); no switch in the build (no build switch) |
| `FIXTURE_VARIANT` | the scratch template is a declared variant of BA-08 V5 section 12; no switch in the build (no build switch) |
| `LIBRARY_VARIANT` | the library under test is a declared variant library of BA-08 V5 section 12; no switch in the build (no build switch) |

### 1.3 `SEALED_AT` (AR3-25)

`SEALED_AT(s)` = SHA-256 of the canonical serialization of the expectation fields of scenario `s`. It is computed by the generator and written in the JSON **before** the candidate ruling of BA-04, so it lies inside the candidate bytes and meets V2.1 26.1 ("recorded before the first governing run") with no self-reference. The catalog is sealed at artifact level only; no scenario is sealed one by one.

| Item | Rule |
|---|---|
| Fields (in this set, nothing else) | `SCENARIO_ID`, `SCENARIO_VERSION`, `GROUP`, `MODE`, `RUN_GOVERNANCE`, `BUILD_UNDER_TEST`, `RECORDED_NOT_ENFORCED`, `TUPLE_REQUIREMENTS`, `PLAN_OR_INPUT`, `FIXTURE`, `REACH`, `INJECTIONS`, `INJECTION_COORDINATE`, `DESIGNED_STIMULUS`, `DELIBERATE_VIOLATION_FLAG`, `TARGET_NEGATIVE_CONTROL`, `CONTROLS_EXERCISED`, `EVIDENCE_GROUPS_DECLARED`, `CONTRACT_ASSIGNED_GROUPS`, `PIN_SLOTS`, `RUN_PREREQUISITES`, `EVM_VERSION`, `EXPECTED_PRODUCT_OUTCOME`, `EXPECTED_CONTROL_RESULTS`, `EXPECTED_INSTRUMENT_STATES`, `EXPECTED_GROUP_VERDICT`, `EXPECTED_EVENTS`, `EXPECTED_DETECTION_CHANNEL`, `EXPECTED_LOG_RECORD_SET`, `EXPECTED_EVIDENCE_SET`, `EXPECTED_CLEANUP`, `RETRY_APPLICABILITY`, `REPETITION`, `EXPECTED_PROCEDURAL_RESULT`, `CONDITIONAL_EXPECTATION_RULE`, `AUTHORITY_SOURCE_FOR_EXPECTED_RESULT` |
| Serialization | a JSON object holding exactly these fields of the scenario with their values as written in this file; keys sorted by Unicode code point (every key is ASCII); UTF-8; no insignificant whitespace (separators "," and ":"); no ASCII escaping of non-ASCII characters; arrays keep their order; every value is a string, an array or an object (no number, boolean or null), so the text equals its RFC 8785 (JCS) form; no trailing newline |
| Hash | SHA-256 of those UTF-8 bytes, 64 lowercase hexadecimal characters |
| Scope | SEALED_AT covers the expectation of one scenario, the fields that decide which group verdicts it contributes to (GROUP, EVIDENCE_GROUPS_DECLARED, CONTRACT_ASSIGNED_GROUPS; R1:BA04V4-10, R2:BA04V4-09) and its planned repetition (REPETITION, RETRY_APPLICABILITY). GROUPS_SERVED and CONTROL_CLASSES_EXERCISED are not listed because each is a function of hashed fields (CONTROLS_EXERCISED, EVIDENCE_GROUPS_DECLARED, CONTRACT_ASSIGNED_GROUPS) and of the BA-02a registry, a DEPENDS_ON entry whose new version cascades to this artifact; PURPOSE, INPUT_STATUS, STATUS, BLOCKED_BY and NOTE are not expectation fields: a change of any of them is covered by the artifact-level hash and needs a new catalog version. The catalog-level definitions a scenario names (DEFAULTS, REPETITION_RULES, LOG_PROFILES, CONDITIONAL_EXPECTATION_RULES, REGISTRY_MAP, GROUP_TABLE, BUILD_PROFILES, RUN_PREREQUISITE_VOCABULARY) are covered by the artifact-level hash of BA-04 only. SEALED_AT is computed and written before the BA-04 candidate ruling, inside its bytes (AR3-25); the catalog is sealed at artifact level only |

A change of any of these fields is a new `SCENARIO_VERSION` and a new `SEALED_AT` (V2.1 26.1). The fields that decide which group verdicts a scenario contributes to (`GROUP`, `EVIDENCE_GROUPS_DECLARED`, `CONTRACT_ASSIGNED_GROUPS`) and its planned repetition (`REPETITION`, `RETRY_APPLICABILITY`) are expectation fields (R1:BA04V4-10, R2:BA04V4-09). The published check recomputes every value.

## 2. Statuses and pins

| Status | Meaning |
|---|---|
| `DRAFT` | the scenario is not ready to seal; if `BLOCKED_BY` is not empty, its expectation **cannot be declared** until the blocker is resolved (V2.1 26.2) |
| `SEALED_PENDING_PIN_CANDIDATE` | the expected result has an authority except for pin slots; when the catalog is sealed (artifact level, AR3-25) it becomes `SEALED_PENDING_PIN` |
| `SEALED_CANDIDATE` | no pin and no blocker; ready for the artifact-level candidate ruling |
| `EXPLORATORY_NON_GOVERNING` | the expected result lacks an authority; never promoted after the fact |

`TOL_SCALE` is **not** a pin: it is a value of the Selective kind baseline (V2.2 PA-9) that has no authority today (K-3). Every governing scenario whose run adjudicates S-1, S-2 or S-3 carries `BLOCKED_BY = K-3`. A scenario in the AR3-01 mode records S-1..S-3 without adjudication and does not depend on `TOL_SCALE` (AR3-01), so it carries no K-3; neither does a learning run, which does not evaluate them (AR4-01).

### 2.1 Pin mechanism (AR2-16, AR2-17, AR3-23, AR4-13)

A pin is a **named slot** in the sealed scenario (`PIN_SLOTS`). Its value lives in a separate **pin record** with its own hash; the slot table of the BA-11 registry (section 3.1) names the kind. **Writing a pin never changes the bytes of this catalog**, and no BA-11 value is written. A pin is written after the value is frozen from its own evidence and before the first governing run that uses it; it is never filled from the outcome of the scenario that uses it. Custody follows BA-07 V5 sections 5.1, 7 and 8 (AR3-23), with the pin state machine and the carry-over ruled by AR4-13 (a normative dependency: BA-07 is in `DEPENDS_ON`):

- **the pin's own seal** is one ratification record of the COORDINATOR and one of the ARCHITECT (decision `RATIFIED`, `subjectKind PIN`, `subject` = the slot, `subjectHash` = the pin-record hash); only when both exist is the `PIN_RECORDED` custody entry appended, citing them; the pin record's `baselineVersion` is the version in force; **at most one live pin per slot** (AR4-13);
- **a pin's usability and its carry-over follow BA-07 V5 sections 5.1 (rules 1-4), 7 (U1-U5) and 8**, which this catalog points to and does not restate (correction pass; U5 since the correction pass (final)): a pin is usable by a governing run only as the use preconditions U1-U5 of BA-07 V5 section 7 state, which bind it to the run's run-start anchor (`IN_USE.runStartAnchor`, an `ANCHOR_RECORDED` anchor) and to the baseline version in force at the run's `IN_USE` entry (U5: the build-equivalence record, for a pin derived on the characterization build that a `PRODUCT_BUILD` run consumes); carrying it to a new baseline version is **fail-closed**, as BA-07 V5 section 5.1 rule 4 and section 8 state (AR4-13);
- **each governing run lists** the pin-record hashes it consumes (`IN_USE.pinRecordHashes`);
- **pins derived on the characterization build** (`E04_ADMITTED_SET@R-1`, `LOCK_MODE`, `HOST_DEFAULT_MAP`) apply to a `PRODUCT_BUILD` run only under the `BUILD_EQUIVALENCE_RECORD` (AR4-06 (4); BA-07 V5 section 7, U5); the `E4-VAL-R1` membership post-check stays the safeguard.

| Pin kind | Needed when | Source of the value |
|---|---|---|
| `FIXTURE_INSTANCE@<FX>` | the scenario has a fixture, or a variant library (`@L-VAR-DIFF`, `@L-VAR-PROXY`, BA04V3-15) | the instance created by host work under a future gate, conforming to the sealed BA-08 V5 specification; the variant libraries are instantiated after the library census (K-2), which fixes their content |
| `E04_ADMITTED_SET@R-1` | a governing run outside the AR3-01 mode that reaches the P0 control evaluation (V2.1 5 P0 row; a refusal at V2.1 13.1 step 3 or 4 does not: R2:BA04V4-04) | `E4-LEARN-R1-*` under the selected `LOCK_MODE` (AR2-11) |
| `LOCK_MODE` | a governing run outside the AR3-01 mode that passes PL; every learning run and `EV-L-COMPOSED` (E-03 enforced, AR4-01); `HDM-CHAR-*` | the selection review of V2.1 3.4 over `LK-*`; each `LK-*` and `E4-LEARN-R1-*` run takes its lock candidate as a scenario parameter (per-candidate design), never as a pin |
| `EVM_FROZEN` | a governing run outside the AR3-01 mode that reaches MUTATION evaluation (PV) | the EVM frozen after learning on the `PRODUCT_BUILD` (V2.1 27.2; V2.2 PA-2; AR4-01) |
| `HOST_DEFAULT_MAP` | a governing run that adjudicates S-1, S-2 or S-3 | `HDM-CHAR-FX1F`, `HDM-CHAR-FXANN`, `HDM-CHAR-FXDIM`, `HDM-CHAR-FXANNBLANK` (non-governing, AR3-01 mode; the rule of the pin is in BA-08 V5; AR3-11, AR4-07); the entries of the informative runs are marked informative and a governing lookup that resolves to one is UNKNOWN (O5; AR4-14 (b)(ii)) |

### 2.2 Order of the pin sources (AR2-11 as completed by AR3-01, AR3-11, AR4-01, AR4-06 and AR4-07)

0. the qualification of every instrument on each build under test (the `Q-*` and `E6-C*` scenarios, their `-CB` twins and the I-14 family; AR4-06 (3)), before any governing run on that build;
1. `E4-LEARN-R1-LM1`, `E4-LEARN-R1-LM2` under each `LOCK_MODE` candidate (AR3-01 mode);
2. `LK-LM1-*`, `LK-LM2-*` (AR3-01 mode);
3. the selection of `LOCK_MODE` by review (V2.1 3.4; `NO_CANDIDATE` gives LK `FAIL` by 3.4 itself), then the `LOCK_MODE` pin record;
4. the `E04_ADMITTED_SET@R-1` pin record, derived from the `E4-LEARN-R1-*` runs under the selected mode; an offline membership post-check in `E4-VAL-R1`;
5. `HDM-CHAR-FX1F`, `HDM-CHAR-FXANN`, `HDM-CHAR-FXDIM`, `HDM-CHAR-FXANNBLANK` (AR3-01 mode, under the pinned `LOCK_MODE`), then the `HOST_DEFAULT_MAP` pin record; this step precedes every governing run that uses S-1..S-3 and may run before or after learning;
6. the `BUILD_EQUIVALENCE_RECORD` (AR4-06 (4); section 1.1), in BA-07 custody before the first `PRODUCT_BUILD` run that consumes a pin of steps 3 to 5 (the learning runs included); its use precondition for each such run is BA-07 V5 section 7, U5;
7. EVM learning (`EV-L-OC-*` on the `PRODUCT_BUILD`, after `REVIEWED`, V2.4: the `REVIEWED_RECORD` in BA-07 custody before the run-start anchor of each learning run), then the composition check `EV-L-COMPOSED` against the DRAFT or UNDER_REVIEW model (its unexplained events block the freeze), then the `EVM_FROZEN` pin record;
8. validation and the governing runs.

The governing runs of negative controls never feed a pin.

## 3. Conditional-expectation rules (normative)

A conditional expectation is allowed **only** when it is **resolved offline, by a rule declared before the run**, from the **sealed evidence of the same run** or from a sealed qualification record it names. It is never resolved from hindsight, from a later run, or from an outcome the rule does not name. The offline evaluator (I-16) applies it; a rule that the sealed evidence cannot resolve makes the scenario result `UNKNOWN`. A scenario resolved `NOT_RUNNABLE`, or a conditional negative control on its non-detection branch, leaves its coverage cell uncovered and contributes no designed detection: BA-02b V5 shows `NEGATIVE_CONTROL_NOT_EVALUATED`, never MET or PASS (AR3-03; AR4-20 (a)..(d)). A rule that resolves from an I-14 record (`CER-WIT`, `CER-SIL`, `CER-A2`, `CER-PERSIST`) is used only on the `CHARACTERIZATION_BUILD`, and the scenario names the qualification record of that I-14 use in `RUN_PREREQUISITES`. Every governing scenario that injects an I-14 writer names the record of the vehicle that qualifies the writer at its coordinate, whether or not a rule resolves from it (X0..X8: `Q-I14-X`; Y0..Y2: `Q-I14-Y`; between PR and PW: `Q-I14-P`; a silent write: `Q-I14-SILENT-X` or `Q-I14-SILENT-Y`; correction pass). The declared rules (the JSON `CONDITIONAL_EXPECTATION_RULES` holds the same five):

| Rule | Text |
|---|---|
| `CER-ABV` | resolved offline from the sealed ABORT-VERIFY record of the same run: match => O2; mismatch or unreadable => O6, recorded as an S-5 / AB finding (never charged to the target control) |
| `CER-WIT` | resolved offline from the sealed I-14 DELIVERY WITNESS record of the same run at the injection position (an independent subscription of the I-14 instrument, qualified at the X positions by Q-I14-X and at the Y coordinates by Q-I14-Y, XDATA_WRITE and Wr-OCT at Y1 included; AR4-01), never from the E-12 log itself (AR2-12): a result conditional on delivery is the detection branch when the witness records the event delivered and the non-detection branch when it does not. One convention holds for every scenario whose designed detection needs the delivery (AR4-16, AR4-20 (d)): on the non-detection branch the target control's result is PASS (nothing was delivered to detect), the scenario is NOT_RUNNABLE as a negative control of that target, it contributes no designed detection to the target's groups, and the coverage cell stays uncovered (NEGATIVE_CONTROL_NOT_EVALUATED in BA-02b V5, AR3-03) |
| `CER-SIL` | RUNNABLE only if the sealed Q-I14-SILENT-Y record (Y coordinates) or Q-I14-SILENT-X record (X coordinates) shows AVAILABLE at the coordinate; otherwise the scenario is recorded NOT_RUNNABLE, the isolation of the target is NOT_ACHIEVED, it contributes no designed detection and its coverage cell stays uncovered (NEGATIVE_CONTROL_NOT_EVALUATED in BA-02b V5, AR3-03, AR4-20 (d)) |
| `CER-A2` | resolved offline from the sealed I-14 delivery witness record of the same run only (never from the E-12 log, AR2-12), positioned by the V0 and V1 markers: no deferred WRITE_CLASS event on a protected object after V0 => O3, A-2 PASS and E-12 phase C PASS; a deferred event delivered between V0 and V1 (phase C) => O7, A-2 FAIL and E-12 phase C FAIL; a deferred event delivered only after V1 (the tail) => O3, A-2 FAIL and E-12 phase C PASS; an A-2 FAIL gives the group verdict CHARACTERIZED_DIFFERENT (V2.2 PA-1.2). A-1 on the same witness record (V2.1 10.6: the baseline runs without writers evaluate A-1 and A-2): no WRITE_CLASS event on a protected object delivered in W-scan (V0..V1) => A-1 PASS; a delivered W-scan event => A-1 UNKNOWN, because it cannot be attributed to the verifier's read-only opens rather than to a deferred commit (V2.1 14: no origin inference), never A-1 FAIL; A-1 alone is measured by EA1-READONLY-OPENS (V2.1 14.7). The scenario runs on the CHARACTERIZATION_BUILD, where I-14 exists (AR4-06 (6); R1:BA04V4-09, second option) |
| `CER-PERSIST` | resolved offline from the sealed I-14 independent post-abort read of the changed entity (qualified for Wr-OCT at Y1 by Q-I14-Y, AR4-01; not from ABORT-VERIFY, not from the E-12 log): the change persisted after the abort => S-5 FAIL (ABORT-VERIFY mismatch, unexpected mutation, V2.1 11.3) and O6 (17.2 row 4); it did not persist => S-5 PASS, the product outcome follows CER-ABV (O2 on a match) and the scenario is NOT_RUNNABLE as a negative control of S-5: it contributes no designed detection and its S-5 cell stays uncovered (NEGATIVE_CONTROL_NOT_EVALUATED in BA-02b V5, AR3-03, AR3-04, AR4-20 (d)) |

**A-1 in a mirror run (the JSON `A1_EVALUATION`; R1:BA04V4-09, second option; correction pass).** A-1 is exercised by EA1-READONLY-OPENS (the measurement of the read-only opens alone, V2.1 14.7) and by the members of the PARAM-03 clean sample, the governing CL-CLEAN-<fixture> scenarios and EA2-DEFERRED-EVENTS, which are the baseline runs without any writer that V2.1 10.6 says evaluate A-1 and A-2; no other mirror run exercises A-1. In such a mirror run A-1 is evaluated on the W-scan record (V0..V1): no WRITE_CLASS event on a protected object in W-scan => A-1 PASS; an event in W-scan => A-1 UNKNOWN, never FAIL, because it cannot be attributed to the verifier's read-only opens rather than to a deferred commit (V2.1 14: no origin inference). In a CL-CLEAN-<fixture> run (PRODUCT_BUILD) the W-scan record is the run's own E-12 phase-C record, the one that evaluates E-12 phase C; the expected A-1 result is PASS, as every E-12 phase-C result of a clean run is (an event there makes E-12 phase C FAIL, O7, a false rejection of the sample, and A-1 UNKNOWN). In EA2-DEFERRED-EVENTS (CHARACTERIZATION_BUILD) A-1 is conditional by CER-A2, resolved from the I-14 delivery witness (AR2-12).

## 4. Defaults, repetition rules, log profiles, tuples and run prerequisites

| Field | Default |
|---|---|
| `EXPECTED_EVIDENCE_SET` | EVIDENCE_PACKAGE_STANDARD (V2.1 18.2) |
| `EXPECTED_CLEANUP` | CLEANUP_STANDARD (V2.1 20); the original library file is restored before the next run's tuple check where a scenario changed it (AR2-31) |
| `RETRY_APPLICABILITY` | NONE_AUTHORIZED (V2.1 21.3; INVALID only with a Coordinator authorization per occurrence; a governing FAIL or UNKNOWN is never retried) |
| `MAX_ATTEMPTS` | PARAM-04 m (Coordinator; BA-10 V5; UNSET) per planned governing run, that is per repetition slot: every attempt of the slot counts, contamination included; a slot that exhausts m leaves the scenario without a required run and its group cannot be PASS (AR3-24); a non-governing run has no repetition slot |
| `EXPECTED_INSTRUMENT_STATES` | every instrument RELIABLE (qualified on the exact build under test: V2.1 4.3; AR4-06 (3)) |
| `UNLISTED_CONTROLS` | every control the run evaluates and EXPECTED_CONTROL_RESULTS does not list is expected PASS; a control the run does not reach is NOT_EVALUATED (AR2-25); a control of the scenario's RECORDED_NOT_ENFORCED set is recorded, never PASS or FAIL (AR3-01; AR4-01 for EV-L-*); in an EV-L-* scenario a control the run enforces as an entry condition (E-03 under the LOCK_MODE pin; every other fail-closed control) is expected PASS as that condition and is ENFORCED_NOT_EXERCISED: it is not in CONTROLS_EXERCISED and its PASS is never coverage (section 1, CONTROLS_EXERCISED; AR4-01); the rule does not apply to a DRY_RUN scenario, whose only measured result is the WARMUP_COVERAGE_CRITERION |

| Tuple | Content |
|---|---|
| `FULL_BASELINE_TUPLE` | FULL_BASELINE_TUPLE (the one PRODUCT_BUILD profile): V2.1 2.1 as extended by V2.5 CATALOG_FOLDER; every BUILD_BOUND field equal to the admissible BUILD_BOUND profile of the PRODUCT_BUILD and every MACHINE_PROFILE_BOUND field equal to the baseline; SESSION_BOUND fields present and consistent; the block-library field is sampled once, before the acquisition (AR2-31); the learning micro-scenario driver is a declared harness DLL of the one PRODUCT_BUILD profile (V2.1 2.1 "any payload / observer / harness DLL"; AR4-01): present in every PRODUCT_BUILD run, part of the E-10 known-module set (V2.1 12.3) and of the WARMUP_MEMBERSHIP set (V2.1 13.1) of its manifest, and inert outside EV-L-* |
| `CHARACTERIZATION_TUPLE` | CHARACTERIZATION_TUPLE (the CHARACTERIZATION_BUILD profile; AR4-06 (1), (2)): the baseline records one admissible BUILD_BOUND profile per BUILD_UNDER_TEST value, and a run is compared (V2.1 2.2) with the profile of its own build. This profile holds its own value for every build-dependent BUILD_BOUND field: the Plugin DLL SHA-256 and the harness DLL SHA-256 of the characterization build (its build receipt enumerates the switch and injection-point set of section 1.2); the manifest build layer and its hash (the exact E-10 known-module set of V2.1 12.3 and the WARMUP_MEMBERSHIP set of V2.1 13.1 of that build); the warm-up definition where it differs; the instrument qualification records of that build (V2.1 4.3: the -CB twins and the I-14 family, section 1.1); and CATALOG_FOLDER of the build under test (V2.5). Every other BUILD_BOUND field (repo source SHA, AutoCAD version, build and acad.exe SHA-256, contract and catalog hashes) and every MACHINE_PROFILE_BOUND field equal the baseline; SESSION_BOUND fields present and consistent; the block-library field as in FULL_BASELINE_TUPLE |
| `CONTROL_PLANE_TUPLE` | CONTROL_PLANE_TUPLE (no host): control-plane source SHA, evaluator assembly SHA-256, evaluator version, synthetic log fixture hash, contract composite hash (the composite row of the BA-11 registry, section 1) and scenario-catalog hash; no host, machine-profile or session field applies (AR2-29) |
| `FPSPEC_CONFORMANCE_TUPLE` | FPSPEC_CONFORMANCE_TUPLE (BUILD_UNDER_TEST = <build>): the fingerprint-provider assembly SHA-256 of that build (its admissible BUILD_BOUND profile, AR4-06 (1)), FINGERPRINT_SPEC_VERSION and the BA-05 artifact hash (its BA-11 registry entry), SHA-256 of the vectors file, the SEAL_HASH of BA-08 as the BA-11 registry records it (the source of the slot values: the sealed tolerance values of its section 10, AR3-17), SHA-256 of the binary64 instance file generated by the harness, control-plane source SHA, contract composite hash (AR2-29) |
| `QUALIFICATION_TUPLE` | QUALIFICATION_TUPLE (BUILD_UNDER_TEST = <build>): the BUILD_BOUND fields of that build, as its admissible BUILD_BOUND profile records them (CHARACTERIZATION_TUPLE; V2.1 2.1: repo source SHA, Plugin and harness DLL SHA-256, AutoCAD version, build and acad.exe SHA-256, contract and catalog hashes), and the MACHINE_PROFILE_BOUND fields; SESSION_BOUND fields present and consistent; no fixture or library field unless the scenario names one (V2.1 4.3: qualification on the exact build; AR4-06 (3)) |
| `variant library` | FULL_BASELINE_TUPLE except the block-library field, which is the declared VARIANT library <L-VAR> of BA-08 V5 section 12 (its SHA-256 is a scenario-specific tuple field, pinned by the slot FIXTURE_INSTANCE@<L-VAR>) |

The `EV-L-*` and `EV-V-*` runs use `FULL_BASELINE_TUPLE`, the one `PRODUCT_BUILD` profile, whose text carries the learning micro-scenario driver (correction pass: no separate learning tuple).

| Run prerequisite | Content |
|---|---|
| `BUILD_EQUIVALENCE_RECORD` | the recorded, verified equivalence of the CHARACTERIZATION_BUILD and the PRODUCT_BUILD (AR4-06 (4); section 1.1): a prerequisite of every PRODUCT_BUILD run that consumes a pin derived on the characterization build (E04_ADMITTED_SET@R-1, LOCK_MODE, HOST_DEFAULT_MAP); location, content schema (recordKind CT21D_BUILD_EQUIVALENCE) and custody: BA-07 V5 sections 2, 4 and 5.3; use precondition: BA-07 V5 section 7, U5 |
| `REVIEWED_RECORD` | the REVIEWED inventory record of V2.4 (BA-06 V5 section 9), a review record in BA-07 custody: its REVIEW_RECORD_ADDED entry has an index at most the h of the run-start anchor of the run (IN_USE.runStartAnchor; BA-07 V5 sections 6.1 and 7, U1), so REVIEWED is anchored in custody before the run starts (V2.4: the first LEARNING run requires REVIEWED); named by every EV-L-* scenario, whose log profile carries REVIEWED_REF (section 4) |
| `QUALIFICATION_RECORD@Q-I14-X` | the sealed Q-I14-X qualification record of the characterization build: the I-14 writers and the delivery witness at X0..X8 |
| `QUALIFICATION_RECORD@Q-I14-Y` | the sealed Q-I14-Y qualification record of the characterization build: the I-14 writers, Wr-OCT and XDATA_WRITE, the delivery witness and the independent post-abort read at Y0..Y2 (AR4-01, AR4-04, AR4-16) |
| `QUALIFICATION_RECORD@Q-I14-P` | the sealed Q-I14-P qualification record of the characterization build: the I-14 writer Wr-G and the delivery witness at the PREPARE-phase coordinate between PR and PW (the coordinate of PR-REC-REOBS-MISMATCH) |
| `QUALIFICATION_RECORD@Q-I14-SILENT-X` | the sealed Q-I14-SILENT-X record: availability of the silent write at X1..X6 (CER-SIL) |
| `QUALIFICATION_RECORD@Q-I14-SILENT-Y` | the sealed Q-I14-SILENT-Y record: availability of the silent write at Y0..Y2 (CER-SIL) |

`BUILD_EQUIVALENCE_RECORD` (the JSON `BUILD_EQUIVALENCE`; section 1.1): BUILD_EQUIVALENCE_RECORD (AR4-06 (4)): the recorded and verified equivalence of the CHARACTERIZATION_BUILD and the PRODUCT_BUILD. Its location, content schema (recordKind CT21D_BUILD_EQUIVALENCE) and custody: BA-07 V5 sections 2, 4 and 5.3; use precondition: BA-07 V5 section 7, U5. BA-07 V5 is the single authority for the record (BA-07 is in DEPENDS_ON) and this catalog restates none of them. The rules of this catalog: the pins derived on the characterization build (E04_ADMITTED_SET@R-1 from E4-LEARN-R1-*, LOCK_MODE from LK-*, HOST_DEFAULT_MAP from HDM-CHAR-*) apply to a PRODUCT_BUILD run only under this record, and the E4-VAL-R1 membership post-check stays the safeguard (AR4-06 (4)); every PRODUCT_BUILD scenario that consumes such a pin names the record in RUN_PREREQUISITES; every run of such a scenario cites the record hash (BA-07 V5 section 4) in its RUN_START record (RUN_PREREQUISITE_RULE, section 4). It is not a pin, not a tuple field and not a seal blocker of this catalog.

**`RUN_PREREQUISITE_RULE`** (correction pass): Every run of a scenario cites, in its first log record (RUN_START, or QUAL_START for a qualification run), the name and the SHA-256 of every record that the scenario's RUN_PREREQUISITES names (for a learning run, the REVIEWED_REF record of LP-LEARN is that citation for the REVIEWED_RECORD). A run that does not cite a named record, or whose cited record is not in the state the vocabulary requires, does not meet the prerequisite: for the BUILD_EQUIVALENCE_RECORD, the pins derived on the characterization build do not apply to it (AR4-06 (4)); for the REVIEWED_RECORD, the run does not meet V2.4 and does not enter the learning corpus (BA-06 V5 section 9); for a qualification record, the instrument is not qualified for the run (V2.1 4.3: no PASS). The BUILD_EQUIVALENCE_RECORD and the REVIEWED_RECORD are review records in BA-07 custody whose REVIEW_RECORD_ADDED entry has an index at most the h of the run-start anchor of the run: for the BUILD_EQUIVALENCE_RECORD, the use precondition of BA-07 V5 section 7, U5 (the record and its custody: BA-07 V5 section 5.3; the SHA-256 it is cited by is its record hash, BA-07 V5 section 4); for the REVIEWED_RECORD, BA-07 V5 sections 5.1, 6.1 and 7, U1; the qualification records are the sealed records of the named scenarios on the build of the run.

| Repetition rule | Value |
|---|---|
| `DEFAULT` | N_strict (PARAM-01), exactly as BA-10 V5 section 2.2 defines it over the class-A and class-B CONTROL groups of GROUPS_SERVED: N_A if the scenario serves only class-A groups, N_B if it serves only class-B groups, max(N_A, N_B) if it serves both (the strictest pair); no ordering of N_A and N_B is assumed; KR (PA-4), the evidence groups (PA-3) and CONTRACT_ASSIGNED_GROUPS never enter N_strict (AR4-14 (a), section 217; AR3-06) (BA-10 V5; UNSET) |
| `CLEAN` | max(N_strict, ceil(n / K)) (PARAM-01 and PARAM-03): K = the number of scenarios of the PARAM-03 sample in the sealed catalog, that is the governing CL-CLEAN-<fixture> scenarios plus EA2-DEFERRED-EVENTS (AR3-24; symbolic, never a hard-coded count); EA2-DEFERRED-EVENTS runs on the CHARACTERIZATION_BUILD and its contribution to PARAM-03 is valid for the PRODUCT_BUILD only under the BUILD_EQUIVALENCE_RECORD (AR4-06 (6)) (BA-10 V5; UNSET) |
| `VALIDATE` | max(N_strict, the PARAM-02 validation count N_det) (AR3-24: the strictest pair applies to every governing scenario that serves a class-A or class-B group) (BA-10 V5; UNSET) |
| `EVIDENCE` | EVIDENCE_REPETITION (Coordinator; BA-10 V5; UNSET) |
| `EVIDENCE_STRICT` | max(N_strict, EVIDENCE_REPETITION): an evidence-group scenario that also serves a class-A or class-B CONTROL group through CONTROLS_EXERCISED (AR3-24) (BA-10 V5; UNSET) |
| `SYNTH` | ONE (deterministic offline evaluation; BA-10 V5) |
| `LEARN` | PARAM-02: N_det + 1 runs, the first the reference (BA-10 V5 section 2.2, rule LEARN), for the governing learning micro-scenarios EV-L-OC-*, which exercise no registry control (their enforced controls are ENFORCED_NOT_EXERCISED, section 1) and serve only E12-L (BA-10 V5; UNSET) |
| `DRY` | WU_DRY_RUNS = N_B (Owner, Q-O2; recorded by the Coordinator; BA-10 V5; UNSET) dry runs per dry-run scenario (AR3-02, AR3-24) |
| `CHAR` | CHAR_RUNS (BA-10 V5 section 2.2; UNSET): N_A for a non-governing run that feeds a pin or a selection (E4-LEARN-R1-*, LK-*; the derived N_c, AR4-14 (b)(i)); for HDM-CHAR-*, CHAR_RUNS(s) = N_A + V(s): N_A runs at the pinned key values (they carry the intent) plus exactly one informative run per varied key, V(s) = the number of keys the scenario varies (AR4-14 (b)(ii)); EVIDENCE_REPETITION for an exploratory scenario (E6-C4, E6-C4-CB); PARAM-02 for the non-governing composition check EV-L-COMPOSED, that is CHAR_RUNS(s) = N_det + 1, the PARAM-02 count of the learning micro-scenarios whose entries it composes (BA-10 V5 section 2.2, composition-check bullet of CHAR_RUNS; AR4-01); not a governing repetition slot |

The per-scenario `REPETITION` field names the rule (the first row of the rule table of BA-10 V5 section 2.2 that matches) and resolves `N_strict` as `N_A` (class-A groups only), `N_B` (class-B groups only) or `max(N_A, N_B)` (both classes), and `CHAR_RUNS` as BA-10 V5 defines it, with the intent ruled by AR4-14 (b). The PARAM-03 sample (AR3-24) is: `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, `CL-CLEAN-FX2F-ALL`, `CL-CLEAN-FX4F-ALL`, `CL-CLEAN-FXANN-FP`, `CL-CLEAN-FXANNBLANK-FP`, `CL-CLEAN-FXDIM-FP`, `CL-CLEAN-FXIMP-FP`, `EA2-DEFERRED-EVENTS` (K = 10 in this draft; the rule stays symbolic).

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
| `LP-LEARN` | RUN_START x1; REVIEWED_REF x1 (the SHA-256 of the REVIEWED inventory record of V2.4 in BA-07 custody, anchored by the run-start anchor: its REVIEW_RECORD_ADDED index is at most the h of IN_USE.runStartAnchor; BA-06 V5 section 9; BA-07 V5 section 7, U1); TUPLE x1 (FULL_BASELINE_TUPLE: the micro-scenario driver is a declared harness DLL of the one PRODUCT_BUILD profile, AR4-01); WARMUP x1; LOCK x1 (the pinned LOCK_MODE; E-03 enforced, ENFORCED_NOT_EXERCISED); MICRO_OPERATION x1; E12_EVENT_RAW x1 per delivered event; RECORDED_CONTROL x1 per control of the RECORDED_NOT_ENFORCED set (E-04, E-12 phase A, E-12 phase C); CLEANUP x1; SEAL x1 |
| `LP-COMPOSE` | LP-LEARN for a composed plan (REVIEWED_REF included), plus MODEL_REF x1 (the DRAFT or UNDER_REVIEW model version and hash it is checked against), COMPOSITION_CHECK x1 (the observed multiset against the composition of the model entries, V2.1 27.9) and UNEXPLAINED_EVENTS x1 (the events that no isolated entry explains, V2.1 27.2 step 4; EV-L-COMPOSED, AR4-01) |
| `LP-CHAR` | the profile of the reach, plus CHARACTERIZATION_BUILD x1 (build receipt and DLL SHA-256); the records of the RECORDED_NOT_ENFORCED controls carry RECORDED verdicts (AR3-01); plus the scenario records: D_VALUES x1 per checkpoint (E4-LEARN-R1-*), LOCK_PROPERTY x1 per property (LK-*), CONTEXT_VARIABLES x1 and EFFECTIVE_ATTRIBUTE x1 per recorded attribute of a created entity (HDM-CHAR-*; for HDM-CHAR-FXANNBLANK, the SYMBOL_RECORD_LAYER typed form of the created layer, AR4-07), WRITER_EFFECT x1 and WITNESS x1 per injection and POST_ABORT_READ x1 per Wr-OCT write (Q-I14-* vehicles, AR4-01) |
| `LP-DRY` | the profile of the source scenario's path (the same REACH), plus CHARACTERIZATION_BUILD x1 and LOAD_EVENT x1 per assembly-load event (expected zero inside the governed window, the stimulus loads excluded); STIMULUS_LOAD x1 per load event of an assembly or module named by a designed load injection of the source (CANARY_LOAD, foreign-load HARNESS_FAULT), identified by its declared identity and excluded from the measurement (AR4-16); BRANCH_SWITCH x1 where the source refuses or aborts because of a control of the RECORDED_NOT_ENFORCED set: the DRY_RUN_BRANCH_SWITCH records the value of that control and then performs the refusal or abort that the source declares, at the same checkpoint (AR4-02; section 5.2) |
| `LP-INJ` | INJECTION x1 per injection: writer, variant, position, enabled state and, for a load injection, the declared identity of the assembly or module (R1:BA04V4-03; AR4-16) |
| `LP-WIT` | WITNESS x1 per injection position: the I-14 delivery-witness record of the same run (AR2-12), from which CER-WIT and CER-A2 resolve; for EA2-DEFERRED-EVENTS, WITNESS x1 over the window from the Commit() return to the end of the post-CP tail, positioned by the V0 and V1 markers |
| `LP-PAR` | POST_ABORT_READ x1: the I-14 independent post-abort read of the changed entity, from which CER-PERSIST resolves (NC-S5-O6) |
| `LP-EVID` | the profile of the preceding run, plus UNDO_STEP x1 per UNDO step and STATE_READ x1 per compared element (U-*), SAVE_REOPEN x1 (S1), POST_CP_TAIL x1 and REREAD x1 per sealed element (PP), CLEANUP_CHECK x1 per check of V2.1 20 (CL), EXCEPTION_ORIGIN x1 (U-6b) |

## 5. Totals and index

Totals: **425** scenarios (263 own scenarios and 162 `WU-DRY-*`). Status: `DRAFT` = 160, `EXPLORATORY_NON_GOVERNING` = 2, `SEALED_CANDIDATE` = 69, `SEALED_PENDING_PIN_CANDIDATE` = 194. Governance: `EXPLORATORY_NON_GOVERNING` = 2, `GOVERNING` = 250, `NON_GOVERNING_BY_DESIGN` = 173. Build under test: `CHARACTERIZATION_BUILD` = 333, `NONE` = 12, `PRODUCT_BUILD` = 80. Blockers: `AQ-V5-08` = 2, `K-3` = 134, `K-8` = 34. Pin slots used: `E04_ADMITTED_SET` = 155, `EVM_FROZEN` = 134, `FIXTURE_INSTANCE` = 358, `HOST_DEFAULT_MAP` = 134, `LOCK_MODE` = 169. Run prerequisites: `BUILD_EQUIVALENCE_RECORD` = 48, `QUALIFICATION_RECORD@Q-I14-P` = 1, `QUALIFICATION_RECORD@Q-I14-SILENT-X` = 7, `QUALIFICATION_RECORD@Q-I14-SILENT-Y` = 3, `QUALIFICATION_RECORD@Q-I14-X` = 87, `QUALIFICATION_RECORD@Q-I14-Y` = 5, `REVIEWED_RECORD` = 17.

Every expected outcome is an exact `O1..O7`, `NONE`, a declared conditional of section 3, `PROVISIONAL_NON_GOVERNING` (AR3-01 mode) or, for an evidence scenario, the exact or conditional outcome of its preceding run (checked mechanically): NONE = 81, PROVISIONAL_NON_GOVERNING = 177, conditional (section 3) = 24, exact O1..O7 = 133, preceding run: exact or conditional = 10.

Repetition rules used: `CHAR: CHAR_RUNS = EVIDENCE_REPETITION (exploratory)` = 2, `CHAR: CHAR_RUNS = N_A` = 6, `CHAR: CHAR_RUNS = N_A at the pinned key values, plus exactly one informative run per varied key (outside the intent)` = 4, `CHAR: CHAR_RUNS = PARAM-02 (non-governing composition check)` = 1, `CLEAN: max(max(N_A, N_B), ceil(n / K))` = 10, `DEFAULT: N_A` = 32, `DEFAULT: N_B` = 4, `DEFAULT: max(N_A, N_B)` = 113, `DRY: WU_DRY_RUNS = N_B` = 162, `EVIDENCE: EVIDENCE_REPETITION` = 57, `EVIDENCE_STRICT: max(N_A, EVIDENCE_REPETITION)` = 1, `EVIDENCE_STRICT: max(N_B, EVIDENCE_REPETITION)` = 1, `EVIDENCE_STRICT: max(max(N_A, N_B), EVIDENCE_REPETITION)` = 1, `LEARN: PARAM-02` = 16, `SYNTH: ONE` = 12, `VALIDATE: max(max(N_A, N_B), PARAM-02 validation count)` = 3.

Conditional negative controls (a designed detection only on the detection branch, AR4-20 (d)): `NC-E12A`, `NC-E12C`, `NC-S1-Y1-SILENT`, `NC-E08-Y0-SILENT`, `NC-E08-Y2-SILENT`, `NC-S5-O6`, `WR-A-X5`, `WR-A-X6`, `WR-D-X5`, `WR-D-X6`, `WR-E-X5`, `WR-E-X6`, `WR-B-X3`, `WR-C-X5`, `WR-C-X6`, `WR-SA-X1`, `WR-SA-X2`, `WR-SA-X3`, `WR-SA-X4`, `WR-A-X5-EV`, `WR-A-X6-EV`, `WR-D-X5-EV`, `WR-D-X6-EV`, `WR-E-X5-EV`, `WR-E-X6-EV`, `WR-B-X3-EV`, `WR-C-X5-EV`, `WR-C-X6-EV`.

| Group | Count |
|---|---|
| `AB` | 6 |
| `CL` | 1 |
| `E10` | 1 |
| `E11M` | 2 |
| `E12-L` | 17 |
| `E12-V` | 6 |
| `E4` | 4 |
| `E6` | 17 |
| `E8` | 14 |
| `E9` | 1 |
| `EA1` | 1 |
| `EA2` | 1 |
| `HDM` | 4 |
| `ID-A` | 3 |
| `KS` | 11 |
| `LK` | 6 |
| `OU` | 11 |
| `PP` | 1 |
| `PR-FRESH` | 4 |
| `PR-REC` | 3 |
| `PW` | 1 |
| `PW-CLONE` | 6 |
| `Q` | 49 |
| `S1` | 1 |
| `SC-A` | 49 |
| `SC-B` | 32 |
| `U-1..U-8` | 8 |
| `U-6b` | 1 |
| `WU` | 164 |

`HDM` is not a group of V2.2 PA-1.3: it labels the four non-governing `HOST_DEFAULT_MAP` characterization scenarios (the label AR4-07 uses), which serve no group (`GROUPS_SERVED` = []). `EV-L-COMPOSED` keeps the label `E12-L` and serves no group (AR4-01).

### 5.1 Scenario summary (every scenario; `WU-DRY-*` are listed in section 5.2)

| ID | GROUP | REACH | COORD | TARGET | DS | BUILD | EXPECTED_PRODUCT_OUTCOME | STATUS | BLOCKED_BY | SEALED_AT (first 12) |
|---|---|---|---|---|---|---|---|---|---|---|
| `Q-I01` | `Q` | NONE | NONE | INSTRUMENT:I-01 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `bcef9df424dd` |
| `Q-I02` | `Q` | NONE | NONE | INSTRUMENT:I-02 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `caf0ae585889` |
| `Q-I03` | `Q` | NONE | NONE | INSTRUMENT:I-03 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `a2703a133e6d` |
| `Q-I04` | `Q` | NONE | NONE | INSTRUMENT:I-04 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `0632c1d918f1` |
| `Q-I05` | `Q` | NONE | NONE | INSTRUMENT:I-05 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `9094b37bf0db` |
| `Q-I06` | `Q` | NONE | NONE | INSTRUMENT:I-06 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `941650eb0dbb` |
| `Q-I07` | `Q` | NONE | NONE | INSTRUMENT:I-07 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `ebb9ae3e378e` |
| `Q-I08` | `Q` | NONE | NONE | INSTRUMENT:I-08 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `6051e2955dcd` |
| `Q-I09` | `Q` | NONE | NONE | INSTRUMENT:I-09 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `45fb025c8e2b` |
| `Q-I10` | `Q` | NONE | NONE | INSTRUMENT:I-10 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `614d9a8c7e12` |
| `Q-I11` | `Q` | NONE | NONE | INSTRUMENT:I-11 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `853f2067120c` |
| `Q-I12` | `Q` | NONE | NONE | INSTRUMENT:I-12 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `becb86e8caa9` |
| `Q-I13` | `Q` | NONE | NONE | INSTRUMENT:I-13 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `98f4e6e9252f` |
| `Q-I14` | `Q` | NONE | NONE | INSTRUMENT:I-14 | - | C | NONE (no product run) | SEALED_CANDIDATE | - | `386e8757b890` |
| `Q-I15` | `Q` | NONE | NONE | INSTRUMENT:I-15 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `0d8a8c4aff5e` |
| `Q-I16` | `Q` | NONE | NONE | INSTRUMENT:I-16 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `603916422ea5` |
| `Q-I09-LOSS` | `Q` | NONE | NONE | INSTRUMENT:I-09 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `a64bd7836429` |
| `Q-I10-LOSS` | `Q` | NONE | NONE | INSTRUMENT:I-10 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `1970a29c8a29` |
| `Q-I14-Y` | `Q` | CP | Y0, Y1 and Y2 | INSTRUMENT:I-14 | - | C | PROVISIONAL_NON_GOVERNING | SEALED_PENDING_PIN_CANDIDATE | - | `06a4e4c6a9b9` |
| `Q-I14-X` | `Q` | CP | X0..X8 | INSTRUMENT:I-14 | - | C | PROVISIONAL_NON_GOVERNING | SEALED_PENDING_PIN_CANDIDATE | - | `0e73a473cfd4` |
| `Q-I14-SILENT-Y` | `Q` | CP | Y0, Y1 and Y2 | NONE | - | C | PROVISIONAL_NON_GOVERNING | SEALED_PENDING_PIN_CANDIDATE | - | `fbcd8948068e` |
| `Q-I14-SILENT-X` | `Q` | CP | X1..X6 | NONE | - | C | PROVISIONAL_NON_GOVERNING | SEALED_PENDING_PIN_CANDIDATE | - | `f401544f9e1d` |
| `Q-I14-P` | `Q` | CP | between PR and PW | INSTRUMENT:I-14 | - | C | PROVISIONAL_NON_GOVERNING | DRAFT | K-8, AQ-V5-08 | `cb0be12364ee` |
| `Q-SIGNED-ZERO` | `Q` | NONE | NONE | INSTRUMENT:I-12 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `6741784ef7e2` |
| `Q-FPSPEC-VECTORS` | `Q` | NONE | NONE | NONE | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `2f39ea490231` |
| `Q-ROUTE-R1` | `Q` | NONE | NONE | NONE | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `40cb3bf4bbc9` |
| `Q-I16-CLEANUP-NEG` | `Q` | NONE | NONE | INSTRUMENT:I-16 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `0d4254b4947e` |
| `E6-C1` | `E6` | NONE | NONE | NONE | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `b7721097ecea` |
| `E6-C2` | `E6` | NONE | NONE | NONE | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `d49cf9698dc5` |
| `E6-C3` | `E6` | NONE | NONE | NONE | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `905b1282b9b9` |
| `E6-C4` | `E6` | NONE | NONE | NONE | - | P | NONE (no product run) | EXPLORATORY_NON_GOVERNING | - | `7ee123e2e08d` |
| `E6-C5` | `E6` | NONE | NONE | NONE | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `49f7bf3a54d4` |
| `E6-C6` | `E6` | NONE | NONE | E06 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `b6b07726be70` |
| `E6-C7` | `E6` | NONE | NONE | NONE | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `aa8611c3fa81` |
| `E6-C8` | `E6` | NONE | NONE | E06 | - | P | NONE (no product run) | SEALED_CANDIDATE | - | `5937a178f746` |
| `Q-I01-CB` | `Q` | NONE | NONE | INSTRUMENT:I-01 | - | C | NONE (no product run) | SEALED_CANDIDATE | - | `caa696115436` |
| `Q-I02-CB` | `Q` | NONE | NONE | INSTRUMENT:I-02 | - | C | NONE (no product run) | SEALED_CANDIDATE | - | `81e1d65e4850` |
| `Q-I03-CB` | `Q` | NONE | NONE | INSTRUMENT:I-03 | - | C | NONE (no product run) | SEALED_CANDIDATE | - | `e421ae93f680` |
| `Q-I04-CB` | `Q` | NONE | NONE | INSTRUMENT:I-04 | - | C | NONE (no product run) | SEALED_CANDIDATE | - | `b6cc3beac190` |
| `Q-I05-CB` | `Q` | NONE | NONE | INSTRUMENT:I-05 | - | C | NONE (no product run) | SEALED_CANDIDATE | - | `537edf577a23` |
| `Q-I06-CB` | `Q` | NONE | NONE | INSTRUMENT:I-06 | - | C | NONE (no product run) | SEALED_CANDIDATE | - | `aa0342a7f3c9` |
| `Q-I07-CB` | `Q` | NONE | NONE | INSTRUMENT:I-07 | - | C | NONE (no product run) | SEALED_CANDIDATE | - | `dbcd2744f9aa` |
| `Q-I08-CB` | `Q` | NONE | NONE | INSTRUMENT:I-08 | - | C | NONE (no product run) | SEALED_CANDIDATE | - | `44ded49fb575` |
| `Q-I09-CB` | `Q` | NONE | NONE | INSTRUMENT:I-09 | - | C | NONE (no product run) | SEALED_CANDIDATE | - | `fa287e559c4e` |
| `Q-I10-CB` | `Q` | NONE | NONE | INSTRUMENT:I-10 | - | C | NONE (no product run) | SEALED_CANDIDATE | - | `992fb00b6d7c` |
| `Q-I11-CB` | `Q` | NONE | NONE | INSTRUMENT:I-11 | - | C | NONE (no product run) | SEALED_CANDIDATE | - | `7aa277ec47f7` |
| `Q-I12-CB` | `Q` | NONE | NONE | INSTRUMENT:I-12 | - | C | NONE (no product run) | SEALED_CANDIDATE | - | `9b6268aa0852` |
| `Q-I13-CB` | `Q` | NONE | NONE | INSTRUMENT:I-13 | - | C | NONE (no product run) | SEALED_CANDIDATE | - | `3ac6c76ee5b6` |
| `Q-I15-CB` | `Q` | NONE | NONE | INSTRUMENT:I-15 | - | C | NONE (no product run) | SEALED_CANDIDATE | - | `618a57b6f244` |
| `Q-I16-CB` | `Q` | NONE | NONE | INSTRUMENT:I-16 | - | C | NONE (no product run) | SEALED_CANDIDATE | - | `98b1199691e6` |
| `Q-I09-LOSS-CB` | `Q` | NONE | NONE | INSTRUMENT:I-09 | - | C | NONE (no product run) | SEALED_CANDIDATE | - | `5fee7b68c1ca` |
| `Q-I10-LOSS-CB` | `Q` | NONE | NONE | INSTRUMENT:I-10 | - | C | NONE (no product run) | SEALED_CANDIDATE | - | `2af25750e9af` |
| `Q-SIGNED-ZERO-CB` | `Q` | NONE | NONE | INSTRUMENT:I-12 | - | C | NONE (no product run) | SEALED_CANDIDATE | - | `5bce5c8088ba` |
| `Q-FPSPEC-VECTORS-CB` | `Q` | NONE | NONE | NONE | - | C | NONE (no product run) | SEALED_CANDIDATE | - | `a37c274f54da` |
| `Q-ROUTE-R1-CB` | `Q` | NONE | NONE | NONE | - | C | NONE (no product run) | SEALED_CANDIDATE | - | `166c9c334d06` |
| `Q-I16-CLEANUP-NEG-CB` | `Q` | NONE | NONE | INSTRUMENT:I-16 | - | C | NONE (no product run) | SEALED_CANDIDATE | - | `8369c98d2018` |
| `E6-C1-CB` | `E6` | NONE | NONE | NONE | - | C | NONE (no product run) | SEALED_CANDIDATE | - | `1ca158dead67` |
| `E6-C2-CB` | `E6` | NONE | NONE | NONE | - | C | NONE (no product run) | SEALED_CANDIDATE | - | `72b6ef956e39` |
| `E6-C3-CB` | `E6` | NONE | NONE | NONE | - | C | NONE (no product run) | SEALED_CANDIDATE | - | `61248b0834e3` |
| `E6-C4-CB` | `E6` | NONE | NONE | NONE | - | C | NONE (no product run) | EXPLORATORY_NON_GOVERNING | - | `f64dbc4ae140` |
| `E6-C5-CB` | `E6` | NONE | NONE | NONE | - | C | NONE (no product run) | SEALED_CANDIDATE | - | `aecd0f816538` |
| `E6-C6-CB` | `E6` | NONE | NONE | E06 | - | C | NONE (no product run) | SEALED_CANDIDATE | - | `7774098be4a3` |
| `E6-C7-CB` | `E6` | NONE | NONE | NONE | - | C | NONE (no product run) | SEALED_CANDIDATE | - | `9e41bad74a81` |
| `E6-C8-CB` | `E6` | NONE | NONE | E06 | - | C | NONE (no product run) | SEALED_CANDIDATE | - | `5e9f1548c0ec` |
| `CL-CLEAN-FX1F-F0` | `KS` | CP | NONE | NONE | - | P | O3 SUCCESS | DRAFT | K-3 | `429efb5d1d98` |
| `CL-CLEAN-FX1F-P` | `KS` | CP | NONE | NONE | - | P | O3 SUCCESS | DRAFT | K-3 | `e81d14e749ea` |
| `CL-CLEAN-FX1F-FP` | `KS` | CP | NONE | NONE | - | P | O3 SUCCESS | DRAFT | K-3 | `d07e301c2cfe` |
| `CL-CLEAN-FX2F-ALL` | `KS` | CP | NONE | NONE | - | P | O3 SUCCESS | DRAFT | K-3 | `93d16bc0dae6` |
| `CL-CLEAN-FX4F-ALL` | `KS` | CP | NONE | NONE | - | P | O3 SUCCESS | DRAFT | K-3 | `8e74a81602bc` |
| `CL-CLEAN-FXANN-FP` | `KS` | CP | NONE | NONE | - | P | O3 SUCCESS | DRAFT | K-3 | `1b45d0d933e6` |
| `CL-CLEAN-FXANNBLANK-FP` | `KS` | CP | NONE | NONE | - | P | O3 SUCCESS | DRAFT | K-3 | `0613ab24e9e5` |
| `CL-CLEAN-FXDIM-FP` | `KS` | CP | NONE | NONE | - | P | O3 SUCCESS | DRAFT | K-3 | `51f9008b49d5` |
| `CL-CLEAN-FXIMP-FP` | `KS` | CP | NONE | NONE | - | P | O3 SUCCESS | DRAFT | K-3, K-8 | `5eaf2d4010ba` |
| `NC-E02` | `ID-A` | P0 | P0 | E02 | - | P | O1 | SEALED_PENDING_PIN_CANDIDATE | - | `2c5a42f22004` |
| `NC-E05` | `ID-A` | P0 | P0 | E05 | - | P | O1 | SEALED_PENDING_PIN_CANDIDATE | - | `1848f480abda` |
| `NC-E06` | `E6` | P0 | P0 | E06 | - | P | O1 | SEALED_PENDING_PIN_CANDIDATE | - | `a07e09e51af4` |
| `NC-E07` | `ID-A` | P0 | P0 | E07 | - | P | O1 | SEALED_PENDING_PIN_CANDIDATE | - | `c24f8bacc884` |
| `NC-E09` | `E9` | P0 | P0 | E09 | - | P | O1 | SEALED_PENDING_PIN_CANDIDATE | - | `5290e42b0def` |
| `NC-E10` | `E10` | P0 | 13.1 step 5 (between the two baseline snapshots) | E10 | - | C | O1 | SEALED_PENDING_PIN_CANDIDATE | - | `79edff88889a` |
| `NC-E11M-PV` | `E11M` | PV | between PS and PV | E11M | - | C | O2 (CER-ABV) | DRAFT | K-3 | `e51375814448` |
| `NC-E11M-PC` | `E11M` | CP | X7 | E11M | - | C | O7 | DRAFT | K-3 | `a70369ae3c02` |
| `NC-E12A` | `E12-V` | CP | Y1 | E12A | - | C | CONDITIONAL (CER-WIT): when the witness records the XData event del... | DRAFT | K-3 | `beaffb9a7689` |
| `NC-E12D` | `E12-V` | P0 | 13.1 step 4 | E12D | - | C | O1 | SEALED_PENDING_PIN_CANDIDATE | - | `af2c7bf3ac1e` |
| `NC-E12C` | `E12-V` | CP | X6 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the write delive... | DRAFT | K-3 | `0e7a40a20ecf` |
| `NC-E04-R2` | `E4` | P0 | P0 | E04 | - | P | O1 | SEALED_PENDING_PIN_CANDIDATE | - | `d912f1573524` |
| `NC-WU-FOREIGN` | `WU` | P0 | 13.1 step 3 | WUM | - | C | O1 | SEALED_PENDING_PIN_CANDIDATE | - | `27e522c02f3e` |
| `NC-E03-NOT-HELD` | `LK` | PL | PL | E03 | - | C | O1 | SEALED_PENDING_PIN_CANDIDATE | - | `2c40ad1b0525` |
| `NC-E03-REACQUIRE` | `LK` | PV | between PL and PS | E03 | - | C | O2 (CER-ABV) | DRAFT | K-3 | `55ba84716bd9` |
| `NC-S1-Y1` | `KS` | PV | Y1 | S1 | - | C | O2 (CER-ABV; the abort before Commit() is unconditional) | DRAFT | K-3 | `42ac828567f4` |
| `NC-S1-Y1-SILENT` | `KS` | PV | Y1 | S1 | - | C | O2 (CER-ABV; the abort before Commit() is unconditional) | DRAFT | K-3 | `19c3481d27ea` |
| `NC-E08-Y0` | `E8` | PV | Y0 | E08 | - | C | O2 (CER-ABV; the abort before Commit() is unconditional) | DRAFT | K-3 | `319e1fcccf38` |
| `NC-E08-Y0-SILENT` | `E8` | PV | Y0 | E08 | - | C | O2 (CER-ABV; the abort before Commit() is unconditional) | DRAFT | K-3 | `17a1ed24a8c0` |
| `NC-E08-Y2` | `E8` | PV | Y2 | E08 | - | C | O2 (CER-ABV; the abort before Commit() is unconditional) | DRAFT | K-3 | `6e2d7a7b52c3` |
| `NC-E08-Y2-SILENT` | `E8` | PV | Y2 | E08 | - | C | O2 (CER-ABV; the abort before Commit() is unconditional) | DRAFT | K-3 | `46ba36e9a3fb` |
| `EA1-READONLY-OPENS` | `EA1` | NONE | NONE | NONE | - | P | NONE (no product run: the A-1 result comes from the measurement, AR... | SEALED_PENDING_PIN_CANDIDATE | - | `b633fbf7930e` |
| `EA2-DEFERRED-EVENTS` | `EA2` | CP | NONE | NONE | - | C | CONDITIONAL (CER-A2): O3 when no deferred event is delivered in pha... | DRAFT | K-3 | `f8ba747a3cb7` |
| `OU-O1` | `OU` | SYN | NONE | NONE | - | - | O1 | SEALED_CANDIDATE | - | `e50c8a53a6f3` |
| `OU-O2` | `OU` | SYN | NONE | NONE | - | - | O2 | SEALED_CANDIDATE | - | `0de75946e448` |
| `OU-O6` | `OU` | SYN | NONE | NONE | - | - | O6 | SEALED_CANDIDATE | - | `58079ff5073b` |
| `OU-O4` | `OU` | SYN | NONE | NONE | - | - | O4 | SEALED_CANDIDATE | - | `b81e6b19b70d` |
| `OU-O5` | `OU` | SYN | NONE | NONE | - | - | O5 | SEALED_CANDIDATE | - | `4df17bb5aee6` |
| `OU-O7` | `OU` | SYN | NONE | NONE | - | - | O7 | SEALED_CANDIDATE | - | `ed921fab77c8` |
| `OU-O3` | `OU` | SYN | NONE | NONE | - | - | O3 | SEALED_CANDIDATE | - | `0f2fa2105773` |
| `OU-ROW1-INVALID` | `OU` | SYN | NONE | NONE | - | - | NONE (row 1: no governing outcome; the evaluator retains a PROVISIO... | SEALED_CANDIDATE | - | `b9885e2fd8e8` |
| `OU-PREDICATE` | `OU` | SYN | NONE | NONE | - | - | NONE | SEALED_CANDIDATE | - | `30176dceef3e` |
| `AB-COMMIT-EXCEPTION-T1-A` | `OU` | SYN | NONE | NONE | - | - | O2 | SEALED_CANDIDATE | - | `4e48c92bc5fb` |
| `AB-COMMIT-EXCEPTION-T1-B` | `OU` | SYN | NONE | NONE | - | - | O6 | SEALED_CANDIDATE | - | `0083705fa230` |
| `PR-SOURCE-SWAP-MODIFY` | `PR-FRESH` | CP | after PR (acquisition) | NONE | Y | C | O3 SUCCESS | DRAFT | K-3, K-8 | `07b2904d0e39` |
| `PR-SOURCE-SWAP-DELETE` | `PR-FRESH` | CP | after PR (acquisition) | NONE | Y | C | O3 SUCCESS | DRAFT | K-3, K-8 | `e867bae59d58` |
| `PR-SOURCE-SWAP-NEG-COPY` | `PR-FRESH` | PW | between the first and the second import | LIBF | - | C | O2 (CER-ABV: the first import is in the manifest; ABORT-VERIFY comp... | DRAFT | K-8 | `027c4b250916` |
| `PR-TORN-READ` | `PR-FRESH` | PR | during the acquisition | LIBF | - | C | O1 (acquisition UNKNOWN => refusal before any product write) | SEALED_PENDING_PIN_CANDIDATE | - | `fc176e66e515` |
| `PW-CLONE-BASE` | `PW-CLONE` | CP | NONE | NONE | Y | P | O3 SUCCESS | DRAFT | K-3, K-8 | `ac5ce03ff706` |
| `PW-CLONE-CASE` | `PW-CLONE` | CP | NONE | NONE | Y | P | O3 SUCCESS | DRAFT | K-3, K-8 | `14a47b5c0902` |
| `PW-CLONE-WRONG-TYPE` | `PW-CLONE` | PR | NONE | NONE | Y | P | O1 | DRAFT | K-8 | `3c58e3f94756` |
| `PW-CLONE-NESTED` | `PW-CLONE` | CP | NONE | NONE | Y | P | O3 SUCCESS | DRAFT | K-3, K-8 | `5c3fc2063637` |
| `PW-CLONE-SYMTAB` | `PW-CLONE` | CP | NONE | NONE | Y | P | O3 SUCCESS | DRAFT | K-3, K-8 | `4a3efa5d4a55` |
| `PR-CLOSURE-COMPLETENESS` | `Q` | NONE | NONE | NONE | - | P | NONE (qualification run on a test document; no mirror) | DRAFT | K-8 | `10d6a4b42285` |
| `PR-REC-CLOSURE-UNKNOWN` | `PR-REC` | PR | NONE | S5CLO | - | P | O1 | SEALED_PENDING_PIN_CANDIDATE | - | `b461d3c39605` |
| `PR-REC-UNREADABLE` | `PR-REC` | PR | PR | S5REC | - | C | O1 | SEALED_PENDING_PIN_CANDIDATE | - | `657abc30bc08` |
| `PR-REC-REOBS-MISMATCH` | `PR-REC` | PW | between PR and PW | S5REC | - | C | O1 (empty manifest) | DRAFT | K-8 | `649433fe2551` |
| `PW-IMPORT-PARTIAL-FAIL` | `PW` | PW | inside PW | S5MAN | - | C | CONDITIONAL (CER-ABV): O2 on a match of the record plus the complet... | DRAFT | K-8 | `14822cd38095` |
| `AB-AFTER-PREPARE-W` | `AB` | PW | between PP and PS | NONE | Y | C | O2 (T_M NOT_OPENED after a PREPARE-W write; ABORT-VERIFY match with... | DRAFT | K-8 | `5ff57d5d7a63` |
| `AB-AFTER-MUTATION` | `AB` | PV | between PS and PV | E11M | - | C | O2 | DRAFT | K-3 | `b70308439597` |
| `NC-S5-O6` | `AB` | PV | Y1 | S5 | - | C | CONDITIONAL (CER-PERSIST): O6 when the independent post-abort read ... | DRAFT | K-3 | `42b56c7999f4` |
| `NC-CLONE-REPLACE` | `PW-CLONE` | PW | inside PW (the SL-5 import) | CLONE | - | C | O6 (T_M NOT_OPENED after a PREPARE-W write; ABORT-VERIFY mismatch: ... | DRAFT | K-8 | `97fd751df3f0` |
| `KS-SL1-POSTWRITE` | `AB` | PS | inside SL-1 | NONE | Y | C | O2 (CER-ABV) | SEALED_PENDING_PIN_CANDIDATE | - | `f2520e4662b8` |
| `KS-SL1-TX-MISMATCH` | `AB` | PS | SL-1 entry | NONE | Y | C | O2 (CER-ABV) | SEALED_PENDING_PIN_CANDIDATE | - | `c1406bf8d139` |
| `KS-SL4-NEG-DET` | `AB` | PS | SL-4 entry | NONE | Y | C | O2 (CER-ABV) | SEALED_PENDING_PIN_CANDIDATE | - | `274b3060feba` |
| `E4-LEARN-R1-LM1` | `E4` | CP | NONE | NONE | - | C | PROVISIONAL_NON_GOVERNING | SEALED_PENDING_PIN_CANDIDATE | - | `b51960af833e` |
| `LK-LM1-PROPS` | `LK` | CP | NONE | NONE | - | C | PROVISIONAL_NON_GOVERNING | SEALED_PENDING_PIN_CANDIDATE | - | `c58923fa9daa` |
| `LK-LM1-CONFLICT` | `LK` | CP | between PL and V1 | NONE | Y | C | PROVISIONAL_NON_GOVERNING | SEALED_PENDING_PIN_CANDIDATE | - | `89167170459a` |
| `E4-LEARN-R1-LM2` | `E4` | CP | NONE | NONE | - | C | PROVISIONAL_NON_GOVERNING | SEALED_PENDING_PIN_CANDIDATE | - | `88ecc1b1944d` |
| `LK-LM2-PROPS` | `LK` | CP | NONE | NONE | - | C | PROVISIONAL_NON_GOVERNING | SEALED_PENDING_PIN_CANDIDATE | - | `ea024e585e27` |
| `LK-LM2-CONFLICT` | `LK` | CP | between PL and V1 | NONE | Y | C | PROVISIONAL_NON_GOVERNING | SEALED_PENDING_PIN_CANDIDATE | - | `0dec9300d05b` |
| `HDM-CHAR-FX1F` | `HDM` | CP | before P0 (the informative runs only) | NONE | - | C | PROVISIONAL_NON_GOVERNING | SEALED_PENDING_PIN_CANDIDATE | - | `e95677c774f6` |
| `HDM-CHAR-FXANN` | `HDM` | CP | before P0 (the informative runs only) | NONE | - | C | PROVISIONAL_NON_GOVERNING | SEALED_PENDING_PIN_CANDIDATE | - | `b9644767cd0b` |
| `HDM-CHAR-FXDIM` | `HDM` | CP | before P0 (the informative runs only) | NONE | - | C | PROVISIONAL_NON_GOVERNING | SEALED_PENDING_PIN_CANDIDATE | - | `2b7420384b4b` |
| `HDM-CHAR-FXANNBLANK` | `HDM` | CP | before P0 (the informative runs only) | NONE | - | C | PROVISIONAL_NON_GOVERNING | SEALED_PENDING_PIN_CANDIDATE | - | `ae0eb2d037a7` |
| `E4-VAL-R1` | `E4` | CP | NONE | NONE | - | P | O3 SUCCESS | DRAFT | K-3 | `db5c159911cc` |
| `EV-L-OC-TM` | `E12-L` | LEARN | NONE | NONE | - | P | NONE (a learning micro-scenario: no mirror product run; AR4-01) | SEALED_PENDING_PIN_CANDIDATE | - | `62d9b7c57d85` |
| `EV-L-OC-BT-OPEN` | `E12-L` | LEARN | NONE | NONE | - | P | NONE (a learning micro-scenario: no mirror product run; AR4-01) | SEALED_PENDING_PIN_CANDIDATE | - | `72a1aa3bda6c` |
| `EV-L-OC-BTR-NESTED` | `E12-L` | LEARN | NONE | NONE | - | P | NONE (a learning micro-scenario: no mirror product run; AR4-01) | SEALED_PENDING_PIN_CANDIDATE | - | `0d8402822305` |
| `EV-L-OC-BTR-VIEW` | `E12-L` | LEARN | NONE | NONE | - | P | NONE (a learning micro-scenario: no mirror product run; AR4-01) | SEALED_PENDING_PIN_CANDIDATE | - | `23b62880ce1e` |
| `EV-L-OC-REF-PIECE` | `E12-L` | LEARN | NONE | NONE | - | P | NONE (a learning micro-scenario: no mirror product run; AR4-01) | SEALED_PENDING_PIN_CANDIDATE | - | `c65597279f2d` |
| `EV-L-OC-DYN` | `E12-L` | LEARN | NONE | NONE | - | P | NONE (a learning micro-scenario: no mirror product run; AR4-01) | SEALED_PENDING_PIN_CANDIDATE | - | `870d3cbba591` |
| `EV-L-OC-REF-ARRAY` | `E12-L` | LEARN | NONE | NONE | - | P | NONE (a learning micro-scenario: no mirror product run; AR4-01) | SEALED_PENDING_PIN_CANDIDATE | - | `aa241a8c6b3f` |
| `EV-L-OC-TM-READ` | `E12-L` | LEARN | NONE | NONE | - | P | NONE (a learning micro-scenario: no mirror product run; AR4-01) | SEALED_PENDING_PIN_CANDIDATE | - | `d82c6677e05c` |
| `EV-L-OC-LAYER` | `E12-L` | LEARN | NONE | NONE | - | P | NONE (a learning micro-scenario: no mirror product run; AR4-01) | SEALED_PENDING_PIN_CANDIDATE | - | `797572b5b452` |
| `EV-L-OC-LAYER-PRESENT` | `E12-L` | LEARN | NONE | NONE | - | P | NONE (a learning micro-scenario: no mirror product run; AR4-01) | SEALED_PENDING_PIN_CANDIDATE | - | `059586fe250e` |
| `EV-L-OC-TEXT` | `E12-L` | LEARN | NONE | NONE | - | P | NONE (a learning micro-scenario: no mirror product run; AR4-01) | SEALED_PENDING_PIN_CANDIDATE | - | `93d5924427e0` |
| `EV-L-OC-DIM` | `E12-L` | LEARN | NONE | NONE | - | P | NONE (a learning micro-scenario: no mirror product run; AR4-01) | SEALED_PENDING_PIN_CANDIDATE | - | `f49eeebb886d` |
| `EV-L-OC-ENVELOPE` | `E12-L` | LEARN | NONE | NONE | - | P | NONE (a learning micro-scenario: no mirror product run; AR4-01) | SEALED_PENDING_PIN_CANDIDATE | - | `e9adc62e11ec` |
| `EV-L-OC-IMPORT` | `E12-L` | LEARN | NONE | NONE | - | P | NONE (a learning micro-scenario: no mirror product run; AR4-01) | DRAFT | K-8 | `adadebbcf4e7` |
| `EV-L-OC-REF-TOP` | `E12-L` | LEARN | NONE | NONE | - | P | NONE (a learning micro-scenario: no mirror product run; AR4-01) | SEALED_PENDING_PIN_CANDIDATE | - | `e6ade6bb205b` |
| `EV-L-OC-VERIFY-READ` | `E12-L` | LEARN | NONE | NONE | - | P | NONE (a learning micro-scenario: no mirror product run; AR4-01) | SEALED_PENDING_PIN_CANDIDATE | - | `f44f80b44399` |
| `EV-L-COMPOSED` | `E12-L` | LEARN | NONE | NONE | - | P | NONE (a non-governing composition check driven by the micro-scenari... | SEALED_PENDING_PIN_CANDIDATE | - | `45e4c63ff929` |
| `EV-V-FX-2F` | `E12-V` | CP | NONE | NONE | - | P | O3 SUCCESS | DRAFT | K-3 | `94c4b0497a8e` |
| `EV-V-FX-4F` | `E12-V` | CP | NONE | NONE | - | P | O3 SUCCESS | DRAFT | K-3 | `39c4b8bb11d1` |
| `EV-V-FX-BIG` | `E12-V` | CP | NONE | NONE | - | P | O3 SUCCESS | DRAFT | K-3 | `e907c8b5b926` |
| `WR-A-X0` | `SC-A` | CP | X0 | S2 | - | C | O4 (semantic comparison detects a persistent change) | DRAFT | K-3 | `3a0858f236d0` |
| `WR-A-X1` | `SC-A` | CP | X1 | S2 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `6fefd905dc37` |
| `WR-A-X2` | `SC-A` | CP | X2 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `23b4b6f5f84b` |
| `WR-A-X3` | `SC-A` | CP | X3 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `9c204694ad24` |
| `WR-A-X4` | `SC-A` | CP | X4 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `750cd2e9f441` |
| `WR-A-X5` | `SC-B` | CP | X5 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 | `7d70cbbe40f8` |
| `WR-A-X6` | `SC-B` | CP | X6 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 | `a1a1a5cabd7f` |
| `WR-A-X7` | `SC-B` | CP | X7 | NONE | Y | C | O3 with RESIDUAL_MISSED_CHANGE (X7 is outside W-scan and phase C); ... | DRAFT | K-3 | `f5971b01301a` |
| `WR-A-X8` | `SC-B` | CP | X8 | NONE | Y | C | O3; outside the guarantee (W3); recorded for the disclosure only | DRAFT | K-3 | `64f91342c06d` |
| `WR-D-X0` | `SC-A` | CP | X0 | S2 | - | C | O4 (semantic comparison detects a persistent change) | DRAFT | K-3 | `2071c194faf9` |
| `WR-D-X1` | `SC-A` | CP | X1 | S2 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `ef11163eb6b3` |
| `WR-D-X2` | `SC-A` | CP | X2 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `fbd164b1dccc` |
| `WR-D-X3` | `SC-A` | CP | X3 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `396c9a1caf74` |
| `WR-D-X4` | `SC-A` | CP | X4 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `c04c29660b04` |
| `WR-D-X5` | `SC-B` | CP | X5 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 | `ed0283edf8a4` |
| `WR-D-X6` | `SC-B` | CP | X6 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 | `a65cfe11e0d9` |
| `WR-D-X7` | `SC-B` | CP | X7 | NONE | Y | C | O3 with RESIDUAL_MISSED_CHANGE (X7 is outside W-scan and phase C); ... | DRAFT | K-3 | `cb18d0af5b30` |
| `WR-D-X8` | `SC-B` | CP | X8 | NONE | Y | C | O3; outside the guarantee (W3); recorded for the disclosure only | DRAFT | K-3 | `32e8e0478fd4` |
| `WR-E-X0` | `SC-A` | CP | X0 | S2 | - | C | O4 (semantic comparison detects a persistent change) | DRAFT | K-3 | `70cb2f78d155` |
| `WR-E-X1` | `SC-A` | CP | X1 | S2 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `3981e8257a80` |
| `WR-E-X2` | `SC-A` | CP | X2 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `4fa8a7b0a767` |
| `WR-E-X3` | `SC-A` | CP | X3 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `df9ad2ad9191` |
| `WR-E-X4` | `SC-A` | CP | X4 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `916fc09176a4` |
| `WR-E-X5` | `SC-B` | CP | X5 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 | `d7c8746f78d3` |
| `WR-E-X6` | `SC-B` | CP | X6 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 | `d5d46e379762` |
| `WR-E-X7` | `SC-B` | CP | X7 | NONE | Y | C | O3 with RESIDUAL_MISSED_CHANGE (X7 is outside W-scan and phase C); ... | DRAFT | K-3 | `bd9ff9f67c90` |
| `WR-E-X8` | `SC-B` | CP | X8 | NONE | Y | C | O3; outside the guarantee (W3); recorded for the disclosure only | DRAFT | K-3 | `b4d3574d72eb` |
| `WR-F-X0` | `SC-A` | CP | X0 | S2 | - | C | O4 (semantic comparison detects a persistent change) | DRAFT | K-3 | `fe0681f103e6` |
| `WR-F-X1` | `SC-A` | CP | X1 | S2 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `617a299f5dd3` |
| `WR-F-X2` | `SC-A` | CP | X2 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `bd85cb51f8c7` |
| `WR-F-X3` | `SC-A` | CP | X3 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `0f5a82993fc7` |
| `WR-F-X4` | `SC-A` | CP | X4 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `4d3edb90f455` |
| `WR-F-X5` | `E8` | CP | X5 | E08 | - | C | O7 (E-08 FAIL at PC: the read-set change persists at PC; 17.2 row 7... | DRAFT | K-3 | `76006f4beba2` |
| `WR-F-X6` | `E8` | CP | X6 | E08 | - | C | O7 (E-08 FAIL at PC: the read-set change persists at PC; 17.2 row 7... | DRAFT | K-3 | `e34a02ad7eea` |
| `WR-F-X7` | `E8` | CP | X7 | E08 | - | C | O7 (E-08 FAIL at PC; the change persists at PC) (AR2-14); POST_V1_T... | DRAFT | K-3 | `ab9470dae516` |
| `WR-F-X8` | `SC-B` | CP | X8 | NONE | Y | C | O3; outside the guarantee (W3); recorded for the disclosure only | DRAFT | K-3 | `c98696cfdec1` |
| `WR-G-X0` | `SC-A` | CP | X0 | S2 | - | C | O4 (semantic comparison detects a persistent change) | DRAFT | K-3 | `5d72989d38fa` |
| `WR-G-X1` | `SC-A` | CP | X1 | S2 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `0682706a1e12` |
| `WR-G-X2` | `SC-A` | CP | X2 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `e2f5b9a93800` |
| `WR-G-X3` | `SC-A` | CP | X3 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `bdfb1fe74414` |
| `WR-G-X4` | `SC-A` | CP | X4 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `13f51b3d69bc` |
| `WR-G-X5` | `E8` | CP | X5 | E08 | - | C | O7 (E-08 FAIL at PC: the read-set change persists at PC; 17.2 row 7... | DRAFT | K-3 | `733c24f6ec11` |
| `WR-G-X6` | `E8` | CP | X6 | E08 | - | C | O7 (E-08 FAIL at PC: the read-set change persists at PC; 17.2 row 7... | DRAFT | K-3 | `a22b184a1ca8` |
| `WR-G-X7` | `E8` | CP | X7 | E08 | - | C | O7 (E-08 FAIL at PC; the change persists at PC) (AR2-14); POST_V1_T... | DRAFT | K-3 | `b77d495bd3a3` |
| `WR-G-X8` | `SC-B` | CP | X8 | NONE | Y | C | O3; outside the guarantee (W3); recorded for the disclosure only | DRAFT | K-3 | `f30ffbaed7c8` |
| `WR-B-X0` | `SC-B` | CP | X0 | NONE | Y | C | O3 with RESIDUAL_MISSED_CHANGE (ABA restored inside the commit boun... | DRAFT | K-3 | `fe2dff5be980` |
| `WR-B-X3` | `SC-B` | CP | X3 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records a delivered phas... | DRAFT | K-3 | `691031c145ee` |
| `WR-B-X7` | `SC-B` | CP | X7 | NONE | Y | C | O3 with RESIDUAL_MISSED_CHANGE (ABA between V1 and PC; nothing pers... | DRAFT | K-3 | `0c99292140db` |
| `WR-C-X5` | `SC-B` | CP | X5 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 | `f13e92e60f86` |
| `WR-C-X6` | `SC-B` | CP | X6 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 | `95b1ebc1fbaf` |
| `WR-C-X7` | `SC-B` | CP | X7 | NONE | Y | C | O3 with RESIDUAL_MISSED_CHANGE (X7 is outside W-scan and phase C); ... | DRAFT | K-3 | `3601edc59176` |
| `WR-SA-X1` | `SC-A` | CP | X1 | S2 | - | C | O4 (semantic comparison detects; no event delivered) | DRAFT | K-3 | `11a0f64a1c5c` |
| `WR-SA-X2` | `SC-A` | CP | X2 | S3 | - | C | O4 (semantic comparison detects; no event delivered) | DRAFT | K-3 | `43716f4151d3` |
| `WR-SA-X3` | `SC-A` | CP | X3 | S3 | - | C | O4 (semantic comparison detects; no event delivered) | DRAFT | K-3 | `c62bbdfa7d24` |
| `WR-SA-X4` | `SC-A` | CP | X4 | S3 | - | C | O4 (semantic comparison detects; no event delivered) | DRAFT | K-3 | `1a35d85d0d16` |
| `WR-SB-X3` | `SC-B` | CP | X3 | NONE | Y | C | O3 with RESIDUAL_MISSED_CHANGE (silent writer: neither channel) | DRAFT | K-3 | `1f9da52e75c5` |
| `WR-SC-X5` | `SC-B` | CP | X5 | NONE | Y | C | O3 with RESIDUAL_MISSED_CHANGE (silent writer: neither channel) | DRAFT | K-3 | `176aa99e8874` |
| `WR-SC-X6` | `SC-B` | CP | X6 | NONE | Y | C | O3 with RESIDUAL_MISSED_CHANGE (silent writer: neither channel) | DRAFT | K-3 | `384787be8c3e` |
| `WR-A-X1-EV` | `SC-A` | CP | X1 | S2 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `ee721010ac63` |
| `WR-A-X2-EV` | `SC-A` | CP | X2 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `a96c85f3827d` |
| `WR-A-X3-EV` | `SC-A` | CP | X3 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `b623cab4689e` |
| `WR-A-X4-EV` | `SC-A` | CP | X4 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `df4fd2085bd7` |
| `WR-A-X5-EV` | `SC-B` | CP | X5 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 | `942d1ec33a80` |
| `WR-A-X6-EV` | `SC-B` | CP | X6 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 | `e88f38aa9868` |
| `WR-D-X1-EV` | `SC-A` | CP | X1 | S2 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `3ebc22fc2c3a` |
| `WR-D-X2-EV` | `SC-A` | CP | X2 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `c250a4099dc5` |
| `WR-D-X3-EV` | `SC-A` | CP | X3 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `da277c2c5cfc` |
| `WR-D-X4-EV` | `SC-A` | CP | X4 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `cd26e17ae849` |
| `WR-D-X5-EV` | `SC-B` | CP | X5 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 | `ee052d86060a` |
| `WR-D-X6-EV` | `SC-B` | CP | X6 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 | `081cbb29b74e` |
| `WR-E-X1-EV` | `SC-A` | CP | X1 | S2 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `273542d34dcb` |
| `WR-E-X2-EV` | `SC-A` | CP | X2 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `d21e2591300c` |
| `WR-E-X3-EV` | `SC-A` | CP | X3 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `07868df67ff7` |
| `WR-E-X4-EV` | `SC-A` | CP | X4 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `737b89239c41` |
| `WR-E-X5-EV` | `SC-B` | CP | X5 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 | `6e0a35f5a76e` |
| `WR-E-X6-EV` | `SC-B` | CP | X6 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 | `7a6d7773b8b5` |
| `WR-F-X1-EV` | `SC-A` | CP | X1 | S2 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `7b7b4bf23a18` |
| `WR-F-X2-EV` | `SC-A` | CP | X2 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `93be2b45a685` |
| `WR-F-X3-EV` | `SC-A` | CP | X3 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `be8b77bdf840` |
| `WR-F-X4-EV` | `SC-A` | CP | X4 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `b2d08b937d02` |
| `WR-F-X5-EV` | `E8` | CP | X5 | E08 | - | C | O7 (E-08 FAIL at PC: the read-set change persists at PC; 17.2 row 7... | DRAFT | K-3 | `ab9d1b2413ad` |
| `WR-F-X6-EV` | `E8` | CP | X6 | E08 | - | C | O7 (E-08 FAIL at PC: the read-set change persists at PC; 17.2 row 7... | DRAFT | K-3 | `feb8826e4fd2` |
| `WR-G-X1-EV` | `SC-A` | CP | X1 | S2 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `83fb0601a398` |
| `WR-G-X2-EV` | `SC-A` | CP | X2 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `8a9b14d7cc79` |
| `WR-G-X3-EV` | `SC-A` | CP | X3 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `eaefb911e152` |
| `WR-G-X4-EV` | `SC-A` | CP | X4 | S3 | - | C | O4 (semantic comparison detects; precedence over O7, 17.2 row 5) | DRAFT | K-3 | `5a5a8a2b3d3b` |
| `WR-G-X5-EV` | `E8` | CP | X5 | E08 | - | C | O7 (E-08 FAIL at PC: the read-set change persists at PC; 17.2 row 7... | DRAFT | K-3 | `ea69a4127d98` |
| `WR-G-X6-EV` | `E8` | CP | X6 | E08 | - | C | O7 (E-08 FAIL at PC: the read-set change persists at PC; 17.2 row 7... | DRAFT | K-3 | `3bd68f99f89b` |
| `WR-B-X3-EV` | `SC-B` | CP | X3 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records a delivered phas... | DRAFT | K-3 | `7569e2948f0d` |
| `WR-C-X5-EV` | `SC-B` | CP | X5 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 | `0b4b35ccc7d9` |
| `WR-C-X6-EV` | `SC-B` | CP | X6 | E12C | - | C | CONDITIONAL (CER-WIT): O7 when the witness records the delivered ph... | DRAFT | K-3 | `8573e1105cf5` |
| `U-1` | `U-1..U-8` | PW | between PP and PS | NONE | Y | C | preceding run: O2 (CER-ABV); then the UNDO sequence | DRAFT | K-8 | `0e0de20aad2e` |
| `U-2` | `U-1..U-8` | CP | NONE | NONE | - | P | preceding run: O3 SUCCESS; then the UNDO sequence | DRAFT | K-3 | `aff8a59d50e4` |
| `U-3` | `U-1..U-8` | CP | X3 | S3 | - | C | preceding run: O4 (S-3 detects; precedence over O7, 17.2 row 5); th... | DRAFT | K-3 | `4ce3c9166d36` |
| `U-4` | `U-1..U-8` | CP | X4 | S3 | - | C | preceding run: O5 (an OBS2 element UNKNOWN, 17.2 row 6); then the U... | DRAFT | K-3 | `8734b2bcca33` |
| `U-5` | `U-1..U-8` | CP | X7 | E11M | - | C | preceding run: O7 (17.2 row 7); then the UNDO sequence | DRAFT | K-3 | `998c55ae0761` |
| `U-6` | `U-1..U-8` | SYN | NONE | NONE | - | - | O6 (T1 synthetic) | SEALED_CANDIDATE | - | `e7628936d17e` |
| `U-7` | `U-1..U-8` | CP | NONE | NONE | - | P | preceding run: O3 SUCCESS; then the UNDO sequence | DRAFT | K-3, K-8 | `67bdc5c0a10a` |
| `U-8` | `U-1..U-8` | CP | NONE | NONE | - | P | preceding run: O3 SUCCESS; then the UNDO sequence | DRAFT | K-3 | `63587a9c6013` |
| `U-6b` | `U-6b` | PV | between PV and Commit() | NONE | Y | C | CONDITIONAL (CER-ABV): O2 on an ABORT-VERIFY match; O6 on a mismatc... | DRAFT | K-3 | `3be1d3d98c89` |
| `S1-SAVE-REOPEN` | `S1` | CP | NONE | NONE | - | P | preceding run: O3 SUCCESS | DRAFT | K-3 | `0cd8bcf26e5f` |
| `PP-POSTCP` | `PP` | CP | NONE | NONE | - | P | preceding run: O3 SUCCESS | DRAFT | K-3 | `0bdf6090e7cb` |
| `CL-CLEANUP-CHECK` | `CL` | CP | NONE | NONE | - | P | preceding run: O3 SUCCESS | DRAFT | K-3 | `b32435f8b211` |
| `WU-BOUNDARY` | `WU` | NONE | NONE | NONE | - | P | NONE (no product mutation) | SEALED_PENDING_PIN_CANDIDATE | - | `ea3a62c7f7d2` |

`DS` = `Y` when `DESIGNED_STIMULUS` is not `NONE`. `BUILD`: `P` = `PRODUCT_BUILD`, `C` = `CHARACTERIZATION_BUILD`, `-` = `NONE`. `REACH` is the furthest checkpoint over the declared branches: `NC-E12A` reaches `CP` on its non-delivery branch; in the vehicles `Q-I14-Y` and `Q-I14-SILENT-Y` the writes that E-08 (enforced) aborts end at PV, and in `Q-I14-P` the enabled write ends at the refusal of the re-observation before the first import (the S-5 registry component, enforced), as their `PLAN_OR_INPUT` and log profile state.

### 5.2 Per-scenario warm-up dry runs (AR3-02, AR3-24, AR4-02, AR4-16)

V2.1 13.1 defines `WARMUP_COVERAGE_CRITERION` **per scenario**; the rule, the mode and the expectation of the dry runs are those of BA-09 V5 section 4. **Rule (the one rule of BA-09 V5 section 4; AR3-02):** a dry-run scenario `WU-DRY-X` is mandatory for every scenario `X` with `RUN_GOVERNANCE = GOVERNING`, `MODE` = `CHARACTERIZATION_RUN` or `VALIDATION`, and `REACH` in `P0`..`CP` (`PS` included), the evidence-group scenarios included; no other scenario has one. Excluded: the non-governing runs, the learning runs (`MODE = LEARNING`) and the scenarios with `REACH = NONE` (and the synthetic ones, `REACH = SYN`). Each `WU-DRY-X` is in the AR3-01 mode (it only measures loads), has the same plan, fixture specification and injections as `X`, `MODE = DRY_RUN`, `RUN_GOVERNANCE = NON_GOVERNING_BY_DESIGN`, the expectation "no assembly-load event inside the governed window" and the repetition rule `DRY` (`WU_DRY_RUNS = N_B` dry runs). `PIN_SLOTS`: only `FIXTURE_INSTANCE` slots: the fixture of `X` and, where `X` uses one, its variant library (`FIXTURE_INSTANCE@L-VAR-*`). There are **162** of them (the JSON lists each one); no per-family reduction is used. The dry runs of the 5 `Q-I14-*` vehicles (`WU-DRY-Q-I14-Y`, `WU-DRY-Q-I14-X`, `WU-DRY-Q-I14-SILENT-Y`, `WU-DRY-Q-I14-SILENT-X`, `WU-DRY-Q-I14-P`) follow their sources (AR4-01; `WU-DRY-Q-I14-P` comes with the vehicle `Q-I14-P` of the correction pass and carries its open item AQ-V5-08, section 8.2).

**Branches taken by switch (AR4-02).** Where the path of `X` refuses or aborts because of a control of the `RECORDED_NOT_ENFORCED` set, the dry run takes the declared branch of `X` at the same checkpoint through the `DRY_RUN_BRANCH_SWITCH` of the characterization build, which records the value of that control and then performs the refusal or abort that `X` declares; the switch is named in the dry run's `PLAN_OR_INPUT` and `INJECTIONS`, and the dry run keeps the `REACH` and log profile of `X`. This concerns `NC-E12A`, `NC-E04-R2`, `NC-E03-NOT-HELD`, `NC-E03-REACQUIRE`, `NC-S1-Y1`, `NC-S1-Y1-SILENT`. For `NC-E12A`, whose abort is conditional on delivery (`CER-WIT`), the switch performs the abort at PV when it records the delivered event and otherwise lets the run commit, as the source does. No branch of a dry run is left to the residual of BA-09 V5 section 4 on this account.

**Designed load injections (AR4-16).** A dry run excludes from its measurement the load events of any assembly or module named by a designed injection of its source (`CANARY_LOAD`, the foreign-load `HARNESS_FAULT`), identified by its declared identity, and records them separately as the stimulus (`STIMULUS_LOAD`); every other load in the window counts. **A warm-up revision never preloads an assembly named by an injection.** This concerns `NC-E10`, `NC-E11M-PV`, `NC-E11M-PC`, `NC-WU-FOREIGN`, `AB-AFTER-MUTATION`, `U-5`.

Scenarios without a dry run, with the declared reason (AR3-02; BA-09 V5 section 4):

| Reason | Scenarios |
|---|---|
| no product run (REACH NONE) | `Q-I01`, `Q-I02`, `Q-I03`, `Q-I04`, `Q-I05`, `Q-I06`, `Q-I07`, `Q-I08`, `Q-I09`, `Q-I10`, `Q-I11`, `Q-I12`, `Q-I13`, `Q-I14`, `Q-I15`, `Q-I16`, `Q-I09-LOSS`, `Q-I10-LOSS`, `Q-SIGNED-ZERO`, `Q-FPSPEC-VECTORS`, `Q-ROUTE-R1`, `Q-I16-CLEANUP-NEG`, `E6-C1`, `E6-C2`, `E6-C3`, `E6-C5`, `E6-C6`, `E6-C7`, `E6-C8`, `Q-I01-CB`, `Q-I02-CB`, `Q-I03-CB`, `Q-I04-CB`, `Q-I05-CB`, `Q-I06-CB`, `Q-I07-CB`, `Q-I08-CB`, `Q-I09-CB`, `Q-I10-CB`, `Q-I11-CB`, `Q-I12-CB`, `Q-I13-CB`, `Q-I15-CB`, `Q-I16-CB`, `Q-I09-LOSS-CB`, `Q-I10-LOSS-CB`, `Q-SIGNED-ZERO-CB`, `Q-FPSPEC-VECTORS-CB`, `Q-ROUTE-R1-CB`, `Q-I16-CLEANUP-NEG-CB`, `E6-C1-CB`, `E6-C2-CB`, `E6-C3-CB`, `E6-C5-CB`, `E6-C6-CB`, `E6-C7-CB`, `E6-C8-CB`, `EA1-READONLY-OPENS`, `PR-CLOSURE-COMPLETENESS`, `WU-BOUNDARY` (60) |
| non-governing (EXPLORATORY_NON_GOVERNING) | `E6-C4`, `E6-C4-CB` (2) |
| no product run (REACH SYN) | `OU-O1`, `OU-O2`, `OU-O6`, `OU-O4`, `OU-O5`, `OU-O7`, `OU-O3`, `OU-ROW1-INVALID`, `OU-PREDICATE`, `AB-COMMIT-EXCEPTION-T1-A`, `AB-COMMIT-EXCEPTION-T1-B`, `U-6` (12) |
| non-governing (NON_GOVERNING_BY_DESIGN) | `E4-LEARN-R1-LM1`, `LK-LM1-PROPS`, `LK-LM1-CONFLICT`, `E4-LEARN-R1-LM2`, `LK-LM2-PROPS`, `LK-LM2-CONFLICT`, `HDM-CHAR-FX1F`, `HDM-CHAR-FXANN`, `HDM-CHAR-FXDIM`, `HDM-CHAR-FXANNBLANK`, `EV-L-COMPOSED` (11) |
| learning (MODE LEARNING, REACH LEARN: a micro-scenario, not a mirror run with a governed window) | `EV-L-OC-TM`, `EV-L-OC-BT-OPEN`, `EV-L-OC-BTR-NESTED`, `EV-L-OC-BTR-VIEW`, `EV-L-OC-REF-PIECE`, `EV-L-OC-DYN`, `EV-L-OC-REF-ARRAY`, `EV-L-OC-TM-READ`, `EV-L-OC-LAYER`, `EV-L-OC-LAYER-PRESENT`, `EV-L-OC-TEXT`, `EV-L-OC-DIM`, `EV-L-OC-ENVELOPE`, `EV-L-OC-IMPORT`, `EV-L-OC-REF-TOP`, `EV-L-OC-VERIFY-READ` (16) |

## 6. Rulings applied

### 6.1 G-1: the Y injection coordinates (harness level; no contract amendment)

| Coordinate | Point |
|---|---|
| `Y0` | after the PS observation and before the first seam write |
| `Y1` | after the last seam write and before the staged reads |
| `Y2` | after the sealing of `VERIFY_ELEMENT_LIST` and before the PV evaluation |

The injected write uses `T_M` (except `NC-S5-O6`, whose write goes through an `OpenCloseTransaction` by design, AR3-04). Its phase-A event is **conditional** on delivery, resolved by the **I-14 delivery witness** (AR2-12, AR2-18), never by the E-12 log itself. The target-control result and the abort are unconditional where a fingerprinted or read-set change is the target (for `NC-S5-O6` the abort is unconditional through E-08, E-06 is PASS at every defined sampling point by AR4-04, and the S-5 result follows `CER-PERSIST`); O2 or O6 follows `CER-ABV`. `NC-E12A`, whose only detector is E-12 phase A, is conditional on delivery as a whole (AR4-16). Isolation of the target needs the silent variant `Wr-S`, whose availability is **measured** at every coordinate it uses (`Q-I14-SILENT-Y` at Y0, Y1, Y2 and `Q-I14-SILENT-X` at X1..X6). The writers, Wr-OCT and XDATA_WRITE included, and the witness are qualified at Y by `Q-I14-Y` (AR4-01).

### 6.2 G-2: tuple identity versus product refusal

| Situation | Result |
|---|---|
| **governed tuple identity mismatch** (any field of V2.1 2.1, as extended by V2.5, differs from the admissible profile of the run's own build, AR4-06 (1), whatever its origin) | the run is **INVALID**; never a product outcome |
| **session / runtime control failure** (E-02..E-07, E-09, E-10, E-11M, and a subscribe failure at P0 of E-11M or E-12, AR2-30) in a valid run | the product **refuses** (O1 at entry; abort at PV) |
| a subscription **lost** after it was established | the run is **INVALID** (V2.1 21.1); its detection is qualified by `Q-I09-LOSS` and `Q-I10-LOSS` (and their `-CB` twins) |
| **product plan outside the envelope** | plan-time refusal (O1); outside the CT-21D baseline unless a scenario governs it |
| E-01 or E-13 mismatch **after** the control plane verified the tuple | an inconsistent tuple: **INVALID** (`NC-E01` and `NC-E13` stay withdrawn; `Q-I02` and `Q-I04` qualify the detection) |
| the library swap of `SOURCE_SWAP_CONTROL` | **not** a tuple mismatch: the tuple's library field is sampled once, before the acquisition; the swap is the control's designed stimulus; the original is restored in the cleanup (AR2-31) |

### 6.3 G-3 and G-4

`E6-C4` (and its twin `E6-C4-CB`) remains `EXPLORATORY_NON_GOVERNING` (G-3). It no longer blocks `NC-S5-O6` (AR4-04). Every scenario with a plan names its fixture; the fixture **instance** is a pin slot (AR2-17); `FX-IMP` is `SPECIFICATION_OPEN` (K-8) and the scenarios that need it, or one of its variants (`FX-IMP-CASE`, `FX-IMP-NESTED`, `FX-IMP-SYMTAB`, `FX-IMP-XREF-HOMONYM`), carry `BLOCKED_BY = K-8` (G-4).

### 6.4 Phase-2 corrections kept (decisions section 211)

- **X0 (AR2-13):** E-12 is not adjudicated at X0 (`NG-19b`); `E12C` is not exercised; O4 comes from the semantic comparison.
- **Read-set writers (AR2-14):** Wr-G and Wr-F at X5, X6 and X7 give **E-08 FAIL at PC and O7**, unconditionally; at X1..X4 O4 takes precedence and E-08 at PC is a collateral FAIL.
- **Wr-F (AR2-15):** the creation of the consumed `RACKCAD_PROJECT` entry (the fixtures have none, B4).
- **Witness (AR2-12):** every "delivered" condition is resolved by the I-14 delivery witness; `NC-E12C` exists (X6, `CER-WIT`); `WR-B-X7` and `WR-C-X7` exist.
- **KR (AR2-21):** the clean runs and the `KS-*` scenarios record E-14 and E-15 and give `KR` `NO_VERDICT`.
- **Cleanup (AR2-22):** `Q-I16-CLEANUP-NEG` qualifies the detection of an incomplete cleanup.
- **Copy swap (AR2-23) and imports (AR2-24):** the negative copy swap sits between two imports; source swap and clone scenarios use `FX-IMP`.
- **Collaterals and default (AR2-25):** unlisted evaluated controls PASS; `NC-E12A` is the XData write at Y1; `NC-E10` is pinned to 13.1 step 5.
- **E12-L (AR2-27):** one micro-scenario per `OC-*` class of BA-06 V5, with `OC-LAYER-PRESENT` and `OC-VERIFY-READ`.
- **Tuples (AR2-29) and subscription at P0 (AR2-30):** unchanged.

### 6.5 V4 corrections kept (decisions section 214)

- **AR3-01:** the six pre-EVM runs, `HDM-CHAR-*` and every `WU-DRY-*` use the characterization build in the `RECORDED_NOT_ENFORCED` mode; product outcome `PROVISIONAL_NON_GOVERNING`. Its use for `Q-I14-*` is ruled by AR4-01; `EV-L-*` are not in it (AR4-01, section 1.1).
- **AR3-02, AR3-24:** a dry run for every governing scenario that enters the governed window (section 5.2); `WU_DRY_RUNS = N_B`; `MAX_ATTEMPTS` per repetition slot; the strictest pair `N_strict` (N_A, N_B or max(N_A, N_B)) applies to every governing scenario that serves a class-A or class-B group.
- **AR3-03:** a scenario resolved `NOT_RUNNABLE` leaves its cell uncovered (section 3; BA-02b V5).
- **AR3-04:** `NC-S5-O6` as ruled, with its E-06 clause revised by AR4-04; `CER-PERSIST` registered.
- **AR3-05:** `DESIGNED_STIMULUS`; designed stimuli have flag `NO`; conditional E-12 phase C cells target `E12C`; `AB-AFTER-MUTATION` targets E-11M (its canary load is a deliberate violation).
- **AR3-06:** `CONTRACT_ASSIGNED_GROUPS` (defined in section 1, extended by AR4-03); `PW-CLONE-WRONG-TYPE` exercises CLONE; `PR-CLOSURE-COMPLETENESS` is in group Q with PR-REC assigned by the contract.
- **AR3-07:** `NC-CLONE-REPLACE`. **AR3-08:** X1 cells: S-2 and S-3 `difference`; exact silent cells. **AR3-09:** U-1, U-3, U-4, U-5, U-6b and EA1 as ruled. **AR3-10:** Wr-F and Wr-G at X5..X7 in group E8; SC-B contract-assigned on every X7 cell.
- **AR3-11:** `HDM-CHAR-*` as the source of the `HOST_DEFAULT_MAP` pin. **AR3-15:** `CL-CLEAN-FXANNBLANK-FP` and its dry run. **AR3-17:** `Q-FPSPEC-VECTORS` in slot units. **AR3-23:** pin custody (section 2.1). **AR3-25:** `SEALED_AT` (section 1.3). **AR3-26:** section 7.

### 6.6 V5 corrections (decisions section 217)

- **AR4-01:** `EV-L-*` on the `PRODUCT_BUILD` with their own `RECORDED_NOT_ENFORCED` set and basis; `EV-L-COMPOSED` a non-governing composition check; `Q-I14-*` vehicles ruled (`Q-I14-P`, by the pattern: AQ-V5-08, section 8.2), the writer set extended with Wr-OCT, XDATA_WRITE and the post-abort read (sections 1.1, 1.2).
- **AR4-02:** the `DRY_RUN_BRANCH_SWITCH` of the six dry runs of section 5.2.
- **AR4-03:** `KS-*`: evidence-only KS and SC-A membership; the typed failure is the procedural result; no blocker.
- **AR4-04:** `NC-S5-O6`: E-06 PASS at every defined sampling point; no E6-C4 blocker.
- **AR4-06:** one BUILD_BOUND profile per build; `CHARACTERIZATION_TUPLE` with its own build-dependent values; 29 per-build twins; `Q-I14` on the `CHARACTERIZATION_BUILD`; `BUILD_EQUIVALENCE_RECORD`; `EA2-DEFERRED-EVENTS` on the `CHARACTERIZATION_BUILD`.
- **AR4-07:** `HDM-CHAR-FXANNBLANK`; `CL-CLEAN-FXANNBLANK-FP` without K-6.
- **AR4-12 (CQ-02), AR4-13, AR4-14, AR4-15:** `NC-CLONE-REPLACE` declaration; pin custody; `N_strict` and `CHAR_RUNS`; no change of the abort expectations.
- **AR4-16:** `NC-E12A` by `CER-WIT`; the load exclusion of the dry runs.
- **AR4-17:** the normative group-field definitions of section 1; BA-07 in `DEPENDS_ON`; the check bound in the BA-02b entry of the registry.
- **AR4-20:** every confirmed minor (section 9).

## 7. What prevents sealing the catalog (AR3-26)

### 7.1 Seal blockers (only these)

| Id | Blocking item | Gate |
|---|---|---|
| C-1 | K-3: `TOL_SCALE` has no authority, so no expectation that adjudicates S-1..S-3 can be declared (134 scenarios) | separate decision, test and record (phase-1 ruling; AR2-35); the test needs a future non-governing host gate |
| C-2 | K-8: the construction of `FX-IMP` and its variants (34 scenarios) | fixture-instance gate (AR2-41) |
| C-3 | the Architect's exact delta review of these V5 bytes (decisions section 218 order), which takes the open Architect questions of section 8.2 before the candidate: **AQ-V5-01** (whether the `BUILD_EQUIVALENCE_RECORD` covers the 111 governing `CHARACTERIZATION_BUILD` runs that consume `EVM_FROZEN`; a yes adds it to their `RUN_PREREQUISITES` and changes their `SEALED_AT`), **AQ-V5-02** (per-build qualification of `PR-CLOSURE-COMPLETENESS` and `WU-BOUNDARY`; a yes adds `-CB` twins) and **AQ-V5-08** (admission of the vehicle `Q-I14-P` (and `WU-DRY-Q-I14-P`) under the `Q-I14-*` pattern of AR4-01; added by correction pass CP-13; `Q-I14-P` and `WU-DRY-Q-I14-P` carry it in `BLOCKED_BY`, the one value there that is not K-3 or K-8), and the inherited AQ-V5-04 (BA-07) and AQ-V5-06 (BA-10), which can change the bytes named in section 8.2; no AQ-V4 question remains: AQ-V4-01..AQ-V4-14 are ruled by AR4-01..AR4-15 or deferred by a named AR4 decision (section 8.1) | Architect |

The V4 rows C-3a, C-3b and C-3c (AQ-V4-01, K-6 = AQ-V4-07, AQ-V4-03) are closed by AR4-01, AR4-07 and AR4-03: no scenario carries them. `E6-C4` is no blocker of any kind (AR4-04). **Remaining seal blockers: K-3 and K-8, and the Architect's review of this version (C-3). No AQ-V4 question remains; the new questions AQ-V5-01, AQ-V5-02 and AQ-V5-08 (section 8.2) go with the Architect's delta review (C-3) and may change scenario bytes before the candidate, as the inherited AQ-V5-04 and AQ-V5-06 may; AQ-V5-08 is also the `BLOCKED_BY` value of `Q-I14-P` and `WU-DRY-Q-I14-P`, resolved inside C-3.**

### 7.2 Fixed before the first governing run (pins and run prerequisites; not seal blockers)

| Id | Item | Source |
|---|---|---|
| C-4 | fixture **instances** (`FIXTURE_INSTANCE@*`, the variant libraries included) | host work under a future gate, conforming to BA-08 V5 section 12; the variant libraries after the census (K-2) |
| C-5 | `HOST_DEFAULT_MAP` | `HDM-CHAR-FX1F`, `HDM-CHAR-FXANN`, `HDM-CHAR-FXDIM`, `HDM-CHAR-FXANNBLANK` (AR3-11, AR4-07) |
| C-6 | `E04_ADMITTED_SET@R-1`, `LOCK_MODE`, `EVM_FROZEN` | section 2.2 |
| C-6b | the qualification records of each build under test (the `Q-*` and `E6-C*` scenarios, their `-CB` twins and the I-14 family), the `BUILD_EQUIVALENCE_RECORD` and the `REVIEWED_RECORD` | sections 1.1, 2.2 and 4 (AR4-06 (3), (4); V2.4); `RUN_PREREQUISITES` names them where a run depends on them and the run cites them (`RUN_PREREQUISITE_RULE`) |

### 7.3 Execution items (not seal blockers)

| Id | Item | Gate |
|---|---|---|
| C-7 | the synthetic log fixtures of the `OU-*` scenarios and of `U-6` | evaluator implementation (EXEC-10); their hashes are `CONTROL_PLANE_TUPLE` fields |
| C-8 | the `CHARACTERIZATION_BUILD` (switches, injection points, the I-14 writers, the `DRY_RUN_BRANCH_SWITCH`) and its admissible BUILD_BOUND profile | implementation; its identity and profile are `CHARACTERIZATION_TUPLE` fields |

### 7.4 Prerequisites of `BASELINE_READY` held by other artifacts (not direct `BLOCKED_BY` blockers)

| Id | Item | Owner |
|---|---|---|
| C-9 | the repetition values: `PARAM-01`, `PARAM-02`, `PARAM-03` and `WU_DRY_RUNS = N_B` (Owner decisions Q-O1..Q-O4, recorded by the Coordinator); `EVIDENCE_REPETITION` and `PARAM-04` (`MAX_ATTEMPTS`) (Coordinator) | BA-10 V5 |
| C-10 | the designated route R-1 and its delivery (K-5); `Q-ROUTE-R1` and `Q-ROUTE-R1-CB` measure it. K-5, and the library census K-2, are before-seal prerequisites of BA-08 V5, so they precede the catalog seal through the BA-08 seal (the sealing workflow of the BA-11 registry: every `DEPENDS_ON` entry is sealed first; the AR3-25 seal order); K-2 also gates the catalog candidate, through BA-05 (BA-08 V5 section 16.1). Neither is a `BLOCKED_BY` value of a scenario | Coordinator ratification (K-5) and the library census (K-2) (BA-08 V5) |
| C-11 | the artifact-level review and agreement of every expected result, then the candidate ruling and the seal of this artifact (`SEALED_AT` is already inside the bytes, AR3-25) | baseline review (the BA-11 registry) |

```text
CATALOG = DRAFT V5 (425 scenarios; 0 sealed)     NB-2 = REMAINS_BLOCKER_FOR_AUTHORITY_BASELINE
CT21D_AUTHORITY_BASELINE_READY = FALSE
```

## 8. Rulings of decisions section 217 on the V4 open questions, and the open Architect questions of round V5

### 8.1 The V4 open questions (all ruled or deferred by a named decision)

| Id | Ruling | Effect on this catalog |
|---|---|---|
| AQ-V4-01 | ruled by AR4-01, section 217 | `EV-L-*` governing on the `PRODUCT_BUILD`, `RECORDED_NOT_ENFORCED` = E-04, E-12 phases A and C with their basis, E-03 enforced, S-1..S-3 `NOT_EVALUATED`, outcome `NONE`; `EV-L-COMPOSED` non-governing composition check; `Q-I14-*` ruled with the AR3-01 set on the vehicle; no scenario is blocked by it (the vehicle `Q-I14-P` of the correction pass waits for AQ-V5-08, section 8.2) |
| AQ-V4-02 | ruled by AR4-02, section 217 | the `DRY_RUN_BRANCH_SWITCH` of the six dry runs (section 5.2) |
| AQ-V4-03 | ruled by AR4-03, section 217 | `KS-*`: no PASS to KS or SC-A; contract-assigned membership by the AR4-03 extension of AR3-06; the typed failure is the procedural result |
| AQ-V4-04 | ruled by AR4-04, section 217 | E6-C4 blocks nothing; `NC-S5-O6` declares E-06 PASS at every defined sampling point |
| AQ-V4-05 | ruled by AR4-05, section 217 | no effect on this catalog; BA-02b V5 section 3 applies it |
| AQ-V4-06 | ruled by AR4-06, section 217 (the Architect's reading; no errata) | section 1.1: one profile per build, per-build qualification, the equivalence record, `EA2-DEFERRED-EVENTS` on the `CHARACTERIZATION_BUILD` |
| AQ-V4-07 | ruled by AR4-07, section 217 | `HDM-CHAR-FXANNBLANK`; K-6 leaves `CL-CLEAN-FXANNBLANK-FP` |
| AQ-V4-08 | ruled by AR4-08, section 217 | no expectation of this catalog changes (ABORT-VERIFY and E-08 domains are BA-08 content) |
| AQ-V4-09 | ruled by AR4-09, section 217 (a declared gap) | no expectation changes; a governing run on a named-plot-style drawing makes SL-R UNKNOWN (O5), which the fixtures avoid (BA-08 V5) |
| AQ-V4-10 | ruled by AR4-10, section 217 | no expectation changes (BA-05 V5 and BA-08 V5 content) |
| AQ-V4-11 | ruled by AR4-12, section 217; CQ-03 (iii), CQ-04, part of CQ-05 and the rest of CQ-07 (to EXEC-3) deferred by AR4-12 | the CQ-02 reading in `NC-CLONE-REPLACE`; no deferred item changes an expectation of this catalog |
| AQ-V4-12 | ruled by AR4-13, section 217 | section 2.1 (pin custody and fail-closed carry-over) |
| AQ-V4-13 | ruled by AR4-14, section 217 | `REPETITION_RULES.DEFAULT` (a) and `CHAR` (b); `HDM-CHAR-*` labels |
| AQ-V4-14 | ruled by AR4-15, section 217 | no expectation change in the twenty abort scenarios (reasons (a)..(e) are stated in BA-06 V5); the governing abort scenarios outside the AR3-01 mode are `NC-E11M-PV`, `NC-E12A`, `NC-E03-REACQUIRE`, `NC-S1-Y1`, `NC-S1-Y1-SILENT`, `NC-E08-Y0`, `NC-E08-Y0-SILENT`, `NC-E08-Y2`, `NC-E08-Y2-SILENT`, `PR-SOURCE-SWAP-NEG-COPY`, `PW-IMPORT-PARTIAL-FAIL`, `AB-AFTER-PREPARE-W`, `AB-AFTER-MUTATION`, `NC-S5-O6`, `NC-CLONE-REPLACE`, `KS-SL1-POSTWRITE`, `KS-SL1-TX-MISMATCH`, `KS-SL4-NEG-DET`, `U-1`, `U-6b` |

### 8.2 Open Architect questions of round V5 (recorded, not decided here)

The ids are the fixed ids of the round (correction pass). An open question listed here can change the bytes of this catalog before the candidate where the third column says so; it is then a before-candidate item of the Architect's delta review (section 7.1, C-3; header `PREREQUISITES`). This version decides none of them and changes no expectation because of them.

| Id | Question | Bytes of this catalog it can change | State |
|---|---|---|---|
| AQ-V5-01 (recorded in V5 as Q-V5-BA04-1) | `EVM_FROZEN` is learned and validated on the `PRODUCT_BUILD` (AR4-01) but consumed by **111** governing `CHARACTERIZATION_BUILD` runs (the scan cells, the Y-coordinate and harness-fault negative controls, `NC-E12A`, `NC-E12C`, `EA2-DEFERRED-EVENTS`, `NC-S5-O6`, U-3..U-5 and others): does the recorded build equivalence of AR4-06 (4) also cover this reverse direction (V2.1 27.5 ties a model to its tuple)? AR4-06 (4) rules only the characterization-to-product direction; this version does not add the `BUILD_EQUIVALENCE_RECORD` to those runs (the ruled rule names `PRODUCT_BUILD` runs only) | yes: a yes adds `BUILD_EQUIVALENCE_RECORD` to the `RUN_PREREQUISITES` of the 111 scenarios and changes their `SEALED_AT`; any other answer is the Architect's to state, with its effect on those scenarios | OPEN, before the candidate (C-3) |
| AQ-V5-02 (recorded in V5 as Q-V5-BA04-2, with the `WU-BOUNDARY` question of BA-09 V5) | per-build qualification of the qualification scenarios whose id is not `Q-*` or `E6-C*`: `PR-CLOSURE-COMPLETENESS` (`CLOSURE_COMPLETENESS_CONTROL`, V2.2 PA-12, I-13 with I-12) and `WU-BOUNDARY` (`WARMUP_BOUNDARY_CONTROL`, V2.2 PA-12; BA-09 V5 W-4); both run on the `PRODUCT_BUILD` only, and AR4-06 (3) names `Q-*` and `E6-C*`: does the per-build rule reach them? `Q-I12`, `Q-I13` and their twins qualify the instruments of `PR-CLOSURE-COMPLETENESS` on each build; `WU-BOUNDARY` is the qualification of `WARMUP_BOUNDARY_CONTROL` (BA-09 V5 W-4) | yes: a yes adds the twins `PR-CLOSURE-COMPLETENESS-CB` and `WU-BOUNDARY-CB` (new scenarios with their `SEALED_AT`, and the counts of section 5 and of BA-02b V5) | OPEN, before the candidate (C-3) |
| AQ-V5-08 (correction pass (final); raised by CP-13) | Admission of the vehicle `Q-I14-P` (and `WU-DRY-Q-I14-P`) under the `Q-I14-*` pattern of AR4-01; added by correction pass CP-13. AR4-01 ruled the `Q-I14-*` vehicles of the V4 proposal (`Q-I14-Y`, `Q-I14-X`, `Q-I14-SILENT-Y`, `Q-I14-SILENT-X`); `Q-I14-P` qualifies Wr-G and the delivery witness at the PREPARE-phase coordinate between PR and PW (the coordinate of `PR-REC-REOBS-MISMATCH`), which no V4 vehicle qualified. Is its admission under the `Q-I14-*` pattern of AR4-01 accepted, or does the vehicle need a ruling of its own? This version applies the pattern and does not decide the admission | yes: `Q-I14-P` and `WU-DRY-Q-I14-P` carry `AQ-V5-08` in `BLOCKED_BY` (status `DRAFT`, with K-8; `BLOCKED_BY` is not an expectation field, so no `SEALED_AT` depends on it); a ruling that does not admit the vehicle changes or removes the two scenarios, the `QUALIFICATION_RECORD@Q-I14-P` prerequisite of `PR-REC-REOBS-MISMATCH` (and its `SEALED_AT`), the vocabulary entries, and the counts of section 5 and of BA-02b V5 | OPEN, before the candidate (C-3) |
| Q-V5-BA04-3 | the repetition of `EV-L-COMPOSED` | none now: the label `CHAR: CHAR_RUNS = PARAM-02 (non-governing composition check)` names the rule of BA-10 V5 section 2.2 (composition-check bullet of `CHAR_RUNS`): `CHAR_RUNS(s) = N_det + 1`, the `PARAM-02` count (`REPETITION_RULES.CHAR`) | **CLOSED** by BA-10 V5 section 2.2 (pointer; the BA-10 reading itself is AQ-V5-06) |
| AQ-V5-04 (inherited from BA-07 V5) | the BA-07 V5 readings: the custody-log reference as a per-manifest constant with `IN_USE.runStartAnchor`; no supersession of a consumed `LOCK_MODE`, `HOST_DEFAULT_MAP` or `FIXTURE_INSTANCE` pin within one baseline version; the carry-over block before the first anchor of v<k+1>, with the mandatory `REVOKED` of the pins not carried over | yes: the custody condition of the `REVIEWED_RECORD` (section 4, `LP-LEARN`) cites `IN_USE.runStartAnchor`; the `BUILD_EQUIVALENCE_RECORD` (sections 1.1 and 4) and section 2.1 point to BA-07 V5 (section 5.3 and U5 for the record, which use the run-start anchor; sections 5.1, 7 and 8 for the pins) without restating them | OPEN (BA-07), before the candidate (C-3) |
| AQ-V5-06 (inherited from BA-10 V5) | `CHAR_RUNS = N_det + 1` for `EV-L-COMPOSED` and the scope of the AR4-14 (b)(i) fail-closed statement | yes: the `REPETITION` of `EV-L-COMPOSED` (and its `SEALED_AT`) and `REPETITION_RULES.CHAR` follow the BA-10 rule; the fail-closed scope is BA-10 content | OPEN (BA-10), before the candidate (C-3) |
| AQ-V5-03, AQ-V5-05, AQ-V5-07 (BA-06, BA-08) | the BA-06 inventory corrections withheld by AR4-12; the BA-08 V5 HDM-5 agreement rule; the scope of the BA-06 frozen parts for the AR4-12 exact delta check | none that these bytes show: the `EV-L-OC-*` plans name the operation class of BA-06 V5 section 3.3, not its members, and the `HDM-CHAR-*` plans cite BA-08 V5 HDM-3..HDM-5 by pointer; a ruling that changed the set of learned operation classes would change the `EV-L-OC-*` set | OPEN (BA-06, BA-08); recorded for completeness |

## 9. Delta V5 (decisions section 217)

| Finding id | Decision | Fix (section / field) |
|---|---|---|
| R1:BA04V4-01 [MAJOR] | AR4-06 (1)-(5) | FIXED. `CHARACTERIZATION_TUPLE` holds its own value for every build-dependent BUILD_BOUND field (Plugin and harness DLL SHA-256, manifest build layer with the E-10 set and the WUM set of that build, warm-up definition where it differs, qualification records of that build, `CATALOG_FOLDER` of the build under test); the JSON `BUILD_PROFILES` records one admissible profile per `BUILD_UNDER_TEST` value (sections 1.1, 4); per-build qualification: 29 twins `<id>-CB` of every `Q-*` and `E6-C*` outside the I-14 family, and `Q-I14` moved to the `CHARACTERIZATION_BUILD` (section 1.1; JSON `PER_BUILD_QUALIFICATION`); the equivalence is recorded and verified by the `BUILD_EQUIVALENCE_RECORD`, a `RUN_PREREQUISITES` entry of the 48 `PRODUCT_BUILD` runs that consume a pin derived on the characterization build (sections 1.1, 2.1, 4). Item (5) of the fix (BA-07 per-build manifest, BA-08 K-10, BA-09 W-7, BA-11) is outside these bytes: cross-artifact note |
| R1:BA04V4-04 [MAJOR] | AR4-16 | FIXED. `NC-E12A` is conditional by `CER-WIT`: delivered => E12A FAIL, abort before `Commit()`, O2/O6 by `CER-ABV`; not delivered => NOT_RUNNABLE, E12A PASS, the run commits (O3) and the cell is `NEGATIVE_CONTROL_NOT_EVALUATED`; REACH `CP` (the non-delivery branch commits), log `LP-PV` or `LP-CP` by branch; `RUN_PREREQUISITES` names `QUALIFICATION_RECORD@Q-I14-Y` (XDATA_WRITE at Y1 with the witness); `SEALED_AT` recomputed; the BA-02b E12A row is now `COVERED_DRAFT_CONDITIONAL` (JSON; BA-02b V5) |
| R1:BA04V4-02 | AR4-02 | FIXED. The six dry runs of `NC-E12A`, `NC-E04-R2`, `NC-E03-NOT-HELD`, `NC-E03-REACQUIRE`, `NC-S1-Y1`, `NC-S1-Y1-SILENT` take the declared branch of their source at the same checkpoint through the `DRY_RUN_BRANCH_SWITCH`, named in `PLAN_OR_INPUT` and `INJECTIONS`; REACH and profile stay those of the source; they are not blocked (the question is ruled) (sections 1.2, 4 `LP-DRY`, 5.2; JSON `DRY_RUN_RULES`) |
| R1:BA04V4-03 | AR4-20 (AR3-30) | FIXED. Log profiles `LP-INJ` (INJECTION x1 per injection), `LP-WIT` (WITNESS x1 per injection position) and `LP-PAR` (POST_ABORT_READ x1) added and appended to the `EXPECTED_LOG_RECORD_SET` of every scenario with injections, with `CER-WIT`, `CER-A2` or `CER-PERSIST`, or with a post-abort read; `EXPECTED_EVIDENCE_SET` names the I-14 witness log, the post-abort-read record and the `Q-I14-SILENT-*` record where a rule resolves from them (section 4; JSON) |
| R1:BA04V4-05 | AR4-01 | FIXED. `Q-I14` and `Q-I14-Y` qualify Wr-OCT and XDATA_WRITE at Y1 with the delivery witness, and the independent post-abort read of a Wr-OCT write (positive and disabled controls); `NC-S5-O6` and `NC-E12A` name `QUALIFICATION_RECORD@Q-I14-Y` in `RUN_PREREQUISITES`; `CER-WIT` and `CER-PERSIST` cite the qualification (sections 1.2, 3; JSON) |
| R1:BA04V4-06 | AR3-07; AR4-12 (CQ-02 reading) | FIXED. `NC-CLONE-REPLACE`: the switch forces the effective mode below the importer's declaration; the pre-import declaration and the structured result keep the allow-listed mode, so PREPARE-W starts and the post-import effect check is the detector; a result that declared Replace would be a CLONE FAIL on the same path (`PLAN_OR_INPUT`, `INJECTIONS`, `EXPECTED_CONTROL_RESULTS`, `EXPECTED_DETECTION_CHANNEL`) |
| R1:BA04V4-07 | AR4-20 | FIXED. `PW-CLONE-WRONG-TYPE`: `EXPECTED_PROCEDURAL_RESULT` = closure classified, REJECT recorded, refusal before PREPARE-W, nothing imported; S5CLO = `FAIL` (the rejection decision of V2.1 5 PR row) => O1; `SEALED_AT` recomputed |
| R1:BA04V4-08 | AR4-14 (b)(ii); AR4-20 | FIXED. The informative runs of the 4 `HDM-CHAR-*` are declared: one `CONTROL_PLANE_SETUP` injection per key before P0 (the key, one declared value different from its pinned value, the position); exactly one informative run per varied key; their entries are marked informative in the pin record and a governing lookup that resolves to one is UNKNOWN (O5) (JSON; sections 2.1, 4) |
| R1:BA04V4-09 | AR4-20 | FIXED by the **second option** of the required fix (correction pass: the first option, taken by the V5 draft, dropped the evaluation of A-1 that V2.1 10.6 requires of the baseline runs without writers). A1 is exercised by `EA1-READONLY-OPENS` and by the 10 members of the `PARAM-03` sample only; `CER-A2` declares A-1 conditional in EA2 (no W-scan event => PASS; a delivered W-scan event => UNKNOWN, unattributable, V2.1 14: no origin inference; never FAIL), and the clean runs expect A-1 PASS by the rule "A-1 in a mirror run" (section 3; JSON `A1_EVALUATION`); `GROUPS_SERVED` and `SEALED_AT` recomputed (JSON; BA-02b V5 A1 row) |
| R1:BA04V4-10 | AR4-20 | FIXED. `SEALED_AT_RULE.fields` adds `GROUP`, `EVIDENCE_GROUPS_DECLARED`, `CONTRACT_ASSIGNED_GROUPS`, `REPETITION`, `RETRY_APPLICABILITY` and the new `RUN_PREREQUISITES`; section 1.3 states why `GROUPS_SERVED`, `CONTROL_CLASSES_EXERCISED`, `PURPOSE`, `INPUT_STATUS`, `STATUS`, `BLOCKED_BY` and `NOTE` are not listed; every value recomputed |
| R1:BA04V4-11 | AR4-13, AR4-14, AR4-20 | FIXED. AQ-V4-12 and AQ-V4-13 are ruled (AR4-13, AR4-14) and applied (section 2.1; `REPETITION_RULES`); no AQ-V4 question remains pending in the header or in section 7.1, and the open V5 questions are listed there as before-candidate items (correction pass); `DELTA_RULING_APPLIED` is the same text in the md header and the JSON (AR3-23 applied; AR3-29 is BA-02b's, stated) |
| R2:CLOSURE BA04V3-17 (PARTIALLY_FIXED) | AR4-01 | FIXED. The four vehicles of the V4 proposal are ruled (AR4-01: governing Q evidence on the `CHARACTERIZATION_BUILD`, the AR3-01 set on the vehicle, its product result not adjudicated), no longer DRAFT or blocked (the fifth, `Q-I14-P`, added by the correction pass, carries K-8 and the open item AQ-V5-08: rows CP-13 and `correction pass (final)` F-2); the writer set adds Wr-OCT, XDATA_WRITE and the post-abort read; `Q-I14-Y` and `Q-I14-SILENT-Y` declare the per-write path (E-08 enforced aborts at PV for Wr-F, Wr-G, Wr-OCT and for the Y0/Y2 silent attempts) |
| R2:BA04V4-01 [MAJOR] | AR4-06 (1)-(3) | FIXED as R1:BA04V4-01, `CATALOG_FOLDER` of the build under test included; `QUALIFICATION_TUPLE` names the build; the counts of section 5 and BA-02b V5 include the twins |
| R2:BA04V4-02 [MAJOR] | AR4-06 (6) | FIXED. `EA2-DEFERRED-EVENTS` runs on the `CHARACTERIZATION_BUILD` (`CHARACTERIZATION_TUPLE`, `QUALIFICATION_RECORD@Q-I14-X` for the witness); its `PARAM-03` contribution is valid for the product build only under the `BUILD_EQUIVALENCE_RECORD` (`REPETITION_RULES.CLEAN`, `NOTE`); `SEALED_AT` recomputed |
| R2:BA04V4-03 | AR4-02 | FIXED as R1:BA04V4-02; the published check verifies the switch in `PLAN_OR_INPUT` and `INJECTIONS` and the equality of REACH and profile with the source (not by construction only) |
| R2:BA04V4-04 | AR4-20 | FIXED. `NC-E12D` exercises only `WUM` and `E12D` (E11M `NOT_DECLARED`, with the reason); `GROUPS_SERVED` and `CONTROL_CLASSES_EXERCISED` re-derived; `E04_ADMITTED_SET@R-1` dropped from `NC-E12D` and `NC-WU-FOREIGN` (a refusal at V2.1 13.1 step 3 or 4 never reaches the P0 control evaluation); the section 2.1 row and the check reworded |
| R2:BA04V4-05 | AR4-16 | FIXED as R1:BA04V4-04 |
| R2:BA04V4-06 | AR4-01 | FIXED as R1:BA04V4-05 and R2:CLOSURE BA04V3-17 (per-writer reach and profile of the vehicle) |
| R2:BA04V4-07 | AR3-07; AR4-12 | FIXED as R1:BA04V4-06 |
| R2:BA04V4-08 | AR4-17 | FIXED. `DEPENDS_ON` adds BA-07; section 2.1 takes the custody rules from BA-07 V5 sections 5.1, 7 and 8 by pointer (a normative dependency, now declared; correction pass) |
| R2:BA04V4-09 | AR4-20 | FIXED as R1:BA04V4-10, with `REPETITION` and `RETRY_APPLICABILITY` |
| R2:BA04V4-10 | AR4-14 (a); AR4-07; AR4-20 | FIXED. (1) the `N_strict` exclusion cites AR4-14 (a); (2) one `DELTA_RULING_APPLIED` text in both files; (3) AR4-07 names the group `HDM` for the fourth scenario: the catalog reads it as the Architect's acceptance of the declared non-group label for the four `HDM-CHAR-*` (`GROUPS_SERVED` = []) |
| R1:BA11V4-01 (BA-04 part: edge BA-04 -> BA-07) | AR4-17 | FIXED in the header `DEPENDS_ON`; the edge list of the BA-11 registry is the lead's (cross-artifact note) |
| R2:BA11V4-01 [MAJOR] | AR4-17 | FIXED. `GROUPS_SERVED` and `CONTRACT_ASSIGNED_GROUPS` are defined normatively in section 1 of this artifact (authority AR2-08, AR3-06, AR4-03; JSON `GROUP_FIELD_DEFINITIONS`); no BA-02b rule is cited as their source; BA-02b V5 rule 1 applies this definition, so only BA-02b -> BA-04 remains |
| R2:BA11V4-03 | AR4-17 | FIXED as R2:BA04V4-08 |
| R1:BA11V4-04 (BA-04 part) | AR4-17 | FIXED on this side: the header states that the published check and its output are bound by hash in the BA-02b entry of the BA-11 registry and never in this artifact's paths (the output holds the SHA-256 of BA-02b); the registry paths are the lead's (cross-artifact note) |
| AR4-01 | applied | `EV-L-*`: governing, E12-L, `PRODUCT_BUILD`, the driver a declared harness DLL of the one `PRODUCT_BUILD` profile; `RECORDED_NOT_ENFORCED` = E-04, E-12 phase A, E-12 phase C with the basis per control; E-03 enforced (`ENFORCED_NOT_EXERCISED`); S-1..S-3 `NOT_EVALUATED`; outcome `NONE`; `EV-L-COMPOSED` non-governing composition check outside the corpus; `Q-I14-*` ruled (`Q-I14-P`, added by the correction pass, applies it through the `Q-I14-*` pattern; its admission is AQ-V5-08) (sections 1.1, 2.2; JSON `EV_L_SET`, `AR3_01_MODE`) |
| AR4-02 | applied | as R1:BA04V4-02 |
| AR4-03 | applied | `KS-*`: no blocker; KS and SC-A in `CONTRACT_ASSIGNED_GROUPS` by the AR4-03 extension of AR3-06 (evidence only); GROUP AB; KR NO_VERDICT; the typed failure is the `EXPECTED_PROCEDURAL_RESULT`; a mismatch is a seam-contract finding that prevents KS PASS; the non-control `SEAM` key removed from `EXPECTED_CONTROL_RESULTS` |
| AR4-04 | applied | `NC-S5-O6`: E-06 PASS at every defined sampling point; `BLOCKED_BY` E6-C4 removed (E6-C4 leaves the vocabulary); K-3 and the Wr-OCT qualification remain |
| AR4-06 | applied | as R1:BA04V4-01 and R2:BA04V4-02 |
| AR4-07 | applied | `HDM-CHAR-FXANNBLANK` added (the fourth characterization scenario); `CL-CLEAN-FXANNBLANK-FP` drops K-6 (K-6 leaves the vocabulary) and keeps K-3 and the slot `HOST_DEFAULT_MAP` |
| AR4-12 (CQ-02) | applied | as R1:BA04V4-06 |
| AR4-13 | applied | section 2.1: one COORDINATOR and one ARCHITECT ratification per pin (decision RATIFIED, subjectKind PIN, subject = slot, subjectHash = pin-record hash), the baselineVersion in force, at most one live pin per slot; usability and fail-closed carry-over by pointer to BA-07 V5 sections 5.1 (rules 1-4), 7 (U1-U5) and 8 |
| AR4-14 | applied | (a) in `REPETITION_RULES.DEFAULT`; (b)(i) and (b)(ii) in `REPETITION_RULES.CHAR` and the `HDM-CHAR-*` labels (exactly one informative run per varied key) |
| AR4-15 | applied | no expectation of the twenty abort scenarios changes; section 8 records the ruling |
| AR4-16 | applied | `NC-E12A` as R1:BA04V4-04; the dry runs of `NC-E10`, `NC-E11M-PV`, `NC-E11M-PC`, `NC-WU-FOREIGN`, `AB-AFTER-MUTATION`, `U-5` exclude the stimulus loads of the named injection, record them as `STIMULUS_LOAD`, and a warm-up revision never preloads an assembly named by an injection (sections 1.2, 4 `LP-DRY`, 5.2); the load injections carry a declared identity |
| AR4-17 | applied | as R2:BA11V4-01, R2:BA04V4-08 and R1:BA11V4-04; references to BA-11 cite the registry entry, not a version |
| AR4-20 | applied | every confirmed minor of both passes (rows above); the conditional negative controls follow one convention (section 3, `CER-WIT`) and give a designed-detection verdict only on their detection branch (BA-02b V5 R-3 (d)) |
| cross-references of round V5 (coordination sheet section 1) | - | siblings are cited by their V5 names (BA-02a stays V3); `Q-SIGNED-ZERO`, `Q-FPSPEC-VECTORS` and their twins cite the vectors file of BA-05 V5 (`docs/automation/evidence/I-52-ct21d-fpspec-v1-vectors-v5.json`: rejectedInputs RJ-01..RJ-25, semanticVectors SV-01..SV-18, slot-unit vectors SV-06, SV-07, SV-10, SV-11, SV-14 and SV-15), and the published check reads the same file; BA-11 is cited by its registry entry (AR4-17) |
| correction pass CP-01 (coverage critic, MAJOR: open Architect questions) | AR3-26; AR4-20 | FIXED. Q-V5-BA04-3 is **CLOSED** by pointer to BA-10 V5 section 2.2 (the composition-check bullet of `CHAR_RUNS`: `N_det + 1`, the `PARAM-02` count; `REPETITION_RULES.CHAR`); Q-V5-BA04-1 and Q-V5-BA04-2 are recorded under the fixed ids **AQ-V5-01** and **AQ-V5-02** (the latter with the `WU-BOUNDARY` question of BA-09 V5) as before-candidate Architect items in the header `PREREQUISITES`, in 7.1 row C-3 and the sentence after it, and in the table of 8.2 with their effect on these bytes; the inherited AQ-V5-04 (BA-07) and AQ-V5-06 (BA-10) are listed where they can change these bytes, and AQ-V5-03, AQ-V5-05 and AQ-V5-07 are recorded for completeness; the V5 draft's statements that no Architect question or item was open (header and 7.1) are withdrawn |
| correction pass CP-02 (coverage critic, MAJOR: A-1 in the baseline runs without writers) | AR4-20 (R1:BA04V4-09, second option); V2.1 10.6, 14 | FIXED as the R1:BA04V4-09 row states: A1 in `CONTROLS_EXERCISED` of `CL-CLEAN-FX1F-F0`, `CL-CLEAN-FX1F-P`, `CL-CLEAN-FX1F-FP`, `CL-CLEAN-FX2F-ALL`, `CL-CLEAN-FX4F-ALL`, `CL-CLEAN-FXANN-FP`, `CL-CLEAN-FXANNBLANK-FP`, `CL-CLEAN-FXDIM-FP`, `CL-CLEAN-FXIMP-FP`, `EA2-DEFERRED-EVENTS` and `EA1-READONLY-OPENS`, and of no other scenario; `CER-A2` and the section 3 paragraph "A-1 in a mirror run" (JSON `A1_EVALUATION`); `GROUPS_SERVED` gains EA1 on the ten sample members (their `REPETITION` is unchanged: they already serve class-A and class-B groups); `SEALED_AT` recomputed; BA-10 V5 section 2.2 (PARAM-03: "A-1 and A-2 are evaluated on this clean sample") and this catalog agree; the BA-02b V5 A1 row follows |
| correction pass CP-03 (consistency critic, MAJOR: HDM-CHAR-FXDIM class text) | AR4-20 (R1:D4-03, R2:BA0809V4-01 of BA-08 V5) | FIXED. The FX-DIM class text of `HDM-CHAR-FXDIM` reads "RotatedDimension (Color, Linetype, LinetypeScale, LineWeight, Transparency; its dimension style is writer-assigned and excluded: BA-08 V5 section 11.2 HDM-1 and section 11, paragraph "Rule 1, dimension style")"; the dimension style is no longer an in-scope attribute, so the circularity with the key `DIMSTYLE` is gone; `SEALED_AT` and the 5.1 row recomputed |
| correction pass CP-04 (consistency critic, MAJOR: the driver DLL in the one PRODUCT_BUILD profile) | AR4-01; AR4-06 (1) | FIXED. `BUILD_PROFILES.PRODUCT_BUILD` and `FULL_BASELINE_TUPLE` state that the learning micro-scenario driver is a declared harness DLL of the one PRODUCT_BUILD profile (V2.1 2.1 "any payload / observer / harness DLL"; AR4-01): present in every PRODUCT_BUILD run, part of the E-10 known-module set (V2.1 12.3) and of the WARMUP_MEMBERSHIP set (V2.1 13.1) of its manifest, and inert outside EV-L-*; the `EV-L-*` and `EV-V-*` tuples are that same text (the separate learning-tuple row of section 4 is removed; section 1.1, `LP-LEARN`, the `EV-L-*` and `EV-V-*` plans and the JSON `EV_L_SET` say the same); the `TUPLE_REQUIREMENTS` and `SEALED_AT` of the 48 `FULL_BASELINE_TUPLE` scenarios recomputed; one `PRODUCT_BUILD` profile and one E-10 exact set remain (AR4-06 (1)); the BA-06 V5 (sections 9, 10.1) and BA-08 V5 (K-10) wording is theirs (cross-artifact note) |
| correction pass CP-05 (both critics, minor: LP-LEARN and the REVIEWED record) | V2.4; AR4-20 | FIXED. `LP-LEARN` gains `REVIEWED_REF x1` (the SHA-256 of the REVIEWED record in BA-07 custody, anchored by the run-start anchor; BA-06 V5 section 9), which `LP-COMPOSE` inherits; `RUN_PREREQUISITE_VOCABULARY` gains `REVIEWED_RECORD`, named by the 16 `EV-L-OC-*` and by `EV-L-COMPOSED` (its log carries `REVIEWED_REF`); `SEALED_AT` recomputed |
| correction pass CP-06 (coverage critic, minor: Q-FPSPEC-VECTORS instance rule) | AR4-20 (R2:BA05V4-N01) | FIXED. The I-12 expectation and `EXPECTED_PROCEDURAL_RESULT` of `Q-FPSPEC-VECTORS` and `Q-FPSPEC-VECTORS-CB` carry the `semanticSlotInstanceRule` of BA-05 V5 3.3.1: the per-predicate realized quantity (exact rational difference; the wrapped difference under `NORM_ANGLE` by a certified enclosure with exact pi; the normal angle by a certified enclosure) and an instance whose check fails or whose enclosure straddles the slot is INVALID, never PASS; "exact-arithmetic check" removed; `SEALED_AT` recomputed |
| correction pass CP-07 (coverage critic, minor: CONTROLS_EXERCISED of EV-L-OC-*) | AR4-01 | FIXED by option (b) of the gap: section 1 (`CONTROLS_EXERCISED` row) states the normative rule "Enforced, not exercised" with its authority (AR4-01: governing runs of the evidence group E12-L), `DEFAULTS.UNLISTED_CONTROLS` states it, and the 17 `EV-L-*` carry the E-03 expectation under the registry key `E03` with the value `ENFORCED_NOT_EXERCISED` (checked mechanically); `GROUPS_SERVED` stays [`E12-L`] and `REPETITION` stays `LEARN: PARAM-02`, as BA-10 V5 rule `LEARN` and its note on the V5 counts state; `SEALED_AT` recomputed |
| correction pass CP-08 (coverage critic, minor: EV-L-COMPOSED outcome) | AR4-01 | FIXED. `EXPECTED_PRODUCT_OUTCOME` of `EV-L-COMPOSED` = `NONE (a non-governing composition check driven by the micro-scenario driver: no mirror product run; AR4-01)`; `SEALED_AT` recomputed; the BA-06 V5 section 9 wording ("a full mirror of FX-1F") is BA-06's (cross-artifact note) |
| correction pass CP-09 (coverage critic, minor: the build equivalence record and BA-07) | AR4-06 (4) | FIXED, then superseded in part by the row `correction pass (final)` F-1: the location, content and custody sentences that this pass wrote into section 1.1 and the JSON `BUILD_EQUIVALENCE` duplicated the record of BA-07 V5 section 5.3 with a different path, field set and use rule, and are replaced there by a pointer to BA-07 V5 (sections 2, 4 and 5.3; section 7, U5). Kept from this pass: the new `RUN_PREREQUISITE_RULE` (section 4; JSON), which requires every run to cite, in its first log record, the SHA-256 of every record its `RUN_PREREQUISITES` names |
| correction pass CP-10 (consistency critic, minor: section 2.1 pin mechanism) | AR4-13; AR4-17 | FIXED. The second bullet of 2.1 points to BA-07 V5 sections 5.1 (rules 1-4), 7 and 8 (U1-U4 in this pass; U1-U5 since the row `correction pass (final)` F-1) in place of the weaker restatement ("an anchor recorded before that run's `IN_USE` entry", "before the first governing use"); md only |
| correction pass CP-11 (consistency critic, minor: section 8.1 row AQ-V4-11) | AR4-12 | FIXED. The row names the rest of CQ-07 (to EXEC-3) among the AR4-12 deferrals |
| correction pass CP-12 (consistency critic, minor: section 7.4 and C-10) | AR3-25, AR3-26 | FIXED. The 7.4 heading reads "not direct `BLOCKED_BY` blockers"; C-10 states that K-5 and K-2 precede the catalog seal through the BA-08 seal (the BA-11 registry workflow, the AR3-25 order) and that K-2 also gates the catalog candidate through BA-05 (BA-08 V5 section 16.1) |
| correction pass CP-13 (consistency critic, minor: I-14 writer coordinates) | AR4-01; AR4-20 | FIXED. The vehicle `Q-I14-P` (`FX-IMP`, `CHARACTERIZATION_BUILD`, AR3-01 mode, with its dry run `WU-DRY-Q-I14-P`; K-8 with its fixture) qualifies Wr-G and the witness at the PREPARE-phase coordinate between PR and PW of `PR-REC-REOBS-MISMATCH`; it applies AR4-01 through its `Q-I14-*` pattern, and its admission (with `WU-DRY-Q-I14-P`) is the open Architect item **AQ-V5-08** (section 8.2; C-3; row `correction pass (final)` F-2). The `Q-I14` sentence names each vehicle and coordinate; every governing scenario that injects an I-14 writer names its vehicle's record in `RUN_PREREQUISITES` (section 3): `QUALIFICATION_RECORD@Q-I14-X` added to the 18 `WR-*-X0/X7/X8` cells, `QUALIFICATION_RECORD@Q-I14-P` to `PR-REC-REOBS-MISMATCH` (new vocabulary entry); `SEALED_AT` recomputed; that the I-14 family now has six members is a cross-artifact note for BA-08, BA-09 and BA-10 |
| correction pass (final) F-1 (re-check, MAJOR: the build-equivalence record defined twice, here and in BA-07 V5) | AR4-06 (4); AR4-17 | FIXED. BA-07 V5 is the single authority for the record (the edge BA-04 -> BA-07): section 1.1, the section 4 paragraph on the record and the JSON `BUILD_EQUIVALENCE` replace the location, content and custody sentences of CP-09 by the pointer "location, content schema (recordKind CT21D_BUILD_EQUIVALENCE) and custody: BA-07 V5 sections 2, 4 and 5.3; use precondition: BA-07 V5 section 7, U5" and keep only the rules of this catalog (which runs name the record; the citation of its record hash in `RUN_START`); `RUN_PREREQUISITE_VOCABULARY.BUILD_EQUIVALENCE_RECORD`, the `RUN_PREREQUISITE_RULE` and step 6 of 2.2 cite U5 for the record (U1 stays for the `REVIEWED_RECORD`); the second bullet of 2.1 points to U1-U5 of BA-07 V5 section 7; the 8.2 row AQ-V5-04 follows. The published check verifies the pointer and the absence of the superseded path and field names (in place of the CP-09 field names) and reads BA-07 V5 to confirm the sections pointed to. `BUILD_EQUIVALENCE` and the vocabulary are catalog-level: no scenario field and no `SEALED_AT` changes |
| correction pass (final) F-2 (re-check, minor: the status of the vehicle `Q-I14-P`) | AR4-01 (by pattern; not decided); AR3-26 | FIXED (recorded, not decided). Section 1.1 no longer says "all by ruling ... none is a proposal" of `Q-I14-P` and `WU-DRY-Q-I14-P`: they apply AR4-01 through its `Q-I14-*` pattern, were added by the correction pass (CP-13), and their admission is the open Architect item **AQ-V5-08** (section 8.2; 7.1 C-3 and the sentence after it; header `PREREQUISITES`). Both carry `AQ-V5-08` in `BLOCKED_BY` after K-8 (status `DRAFT` unchanged); the `BLOCKED_BY` vocabulary (section 1; JSON `BLOCKED_BY_VOCABULARY`), the totals of section 5, the rows CP-13, R2:CLOSURE BA04V3-17 and AR4-01, section 8.1 row AQ-V4-01, the `Q-I14-P` `NOTE` and the JSON `AR3_01_MODE.ruled_for` follow, and the published check verifies them. No `SEALED_AT` changes (`BLOCKED_BY` and `NOTE` are not expectation fields) |

Section numbering: sections 1 to 8 keep their V4 numbers (1.1 is retitled; 2.2 gains steps 0 and 6; 6.6 and 7.2 row C-6b are new; section 8 is split into 8.1 and 8.2); section 9 replaces the Delta V4 table, which stays in the V4 file as history. Findings of other artifacts that this version answers only in part are listed with the part that lands here. The rows `correction pass CP-01`..`CP-13` record the correction pass of round V5 (the gaps of the coverage and consistency critics on BA-04 and BA-02b); it adds the paragraph "A-1 in a mirror run" to section 3, the `RUN_PREREQUISITE_RULE` paragraph to section 4 and the table of 8.2, and removes the learning-tuple row of section 4. The rows `correction pass (final)` F-1 and F-2 record the final consistency corrections of the re-check of that pass (the build-equivalence record by pointer to BA-07 V5; the open item AQ-V5-08 and its row in 8.2).
