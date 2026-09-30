# I-52 — CT-21D Baseline Artifact BA-10: `MUST_FIX` Parameter Sheet and Selection Methods (DRAFT V7)

> **BASELINE ARTIFACT BA-10 V7 — DRAFT, NOT SEALED. Design / documentation only. No value is chosen: every value is `UNSET`. The sheet is symbolic: it holds no count, no `K` and no total taken from the scenario catalog.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-PARAMETER-SHEET
> ARTIFACT_VERSION          = 7-DRAFT (supersedes 6-DRAFT, blob 3f3d2d4f76ca5fb70fbb95f2cf19bbb7cd66afd9, which stays as history)
> STATUS AUTHORITY          = the BA-10 entry of the BA-11 registry is the only authority for its status and hashes; this header
>                             is frozen, non-authoritative text of the candidate bytes (the sealing workflow of the BA-11 registry,
>                             section 2)
> AUTHORITY_CONTRACT        = CT-21D V2.1 + V2.2 + V2.3 + V2.4 (draft revision 3) + V2.5 (draft revision 2), V2.4 revision 3 and
>                             V2.5 revision 2 textually verified by the Coordinator (decisions sections 218 and 221); + V2.6 (draft,
>                             pending review; AR6-05). The effective contract is not yet agreed (AR6-06)
> BOUND_PRODUCT_SHA         = 95690c28 (AR6-01; recorded by the Coordinator in decisions section 224 as a non-tuple bound item).
>                             This sheet is symbolic and states no code fact (Gate 2 relink assessment, BA-10 row: "No code
>                             facts"); nothing in these bytes is bound to that value
> CONTRACT CLAUSES USED     = V2.1 1.3, 1.4, 1.6, 2.1, 2.2, 3.3, 3.4, 4.3, 5, 10.6, 13.1, 14.3, 20, 21.1, 21.3, 21.5, 21.7,
>                             22 (BASE-3, BASE-18), 26.3, 27.1-27.4, 27.8, 27.9, 29; V2.2 PA-1.3, PA-2, PA-3, PA-4, PA-9, PA-12
> DELTA RULING APPLIED      = decisions section 226: AR7-09, the minors BA10V6-01 (the consequence disclosure of P-L5..P-L7
>                             restored outside the AR5-08 quotation, 2.2 "Fail-closed" and section 5 Q-O1) and BA10V6-02 (the
>                             "labels" statement reworded, section 7); no rule, method, count or value changes;
>                             decisions section 220: AR5-08 (the `CHAR_RUNS = N_det + 1` count of `EV-L-COMPOSED` accepted, and the
>                             fail-closed statement in its exact words; section 2.2), AR5-04 (the reverse direction of the recorded
>                             build equivalence, recorded in 2.2 `PARAM-03` and section 4), AR5-05 (per-build qualification by
>                             content, 2.2 "Build under test"), AR5-06 (the pin custody readings, section 4 pointer), AR5-07 (the
>                             HDM-5 agreement rule, 2.2 "Fail-closed"), AR5-09 (`Q-I14-P` admitted, 2.2 "Build under test");
>                             decisions section 223: AR6-02 (`FX-ANN-BLANK` kept as a legacy-state fixture of the
>                             `FIXTURE_CONSTRUCTION_BUILD`: the rules and the membership of `HDM-CHAR-FXANNBLANK` and
>                             `CL-CLEAN-FXANNBLANK-FP` are unchanged), AR6-01 and AR6-05 as alignment (header), and the BA-10 row
>                             of 223.4 (labels re-pointed to the registered versions of the round). Sections 217 (AR4-14, AR4-21,
>                             AR4-01, AR4-06, AR4-07, AR4-13, AR4-16, AR4-17, AR4-20), 214 (AR3-24, AR3-25, AR3-06, AR3-01, AR3-02,
>                             AR3-11, AR3-14, AR3-26, AR3-30) and 211 (AR2-28, AR2-43) still apply where sections 220, 223 and
>                             226 do not change them
> ARCHITECT QUESTIONS       = none open on this sheet in these bytes. AQ-V4-13 and AQ-V4-01 are ruled by AR4-14 and AR4-01
>                             (section 217); AQ-V5-06 of this sheet is ruled by AR5-08, and AQ-V5-01, AQ-V5-02 and AQ-V5-05, raised
>                             on sibling artifacts, by AR5-04, AR5-05 and AR5-07 (section 220); section 6 records where each ruling
>                             is applied
> PHASE-2 RULING (V3, kept) = BA10-01..BA10-07 (AR2-28, AR2-43)
> DEPENDS_ON                = BA-01   (registry entries only; the BA-10 entry of the BA-11 registry). No edge to BA-04 (AR3-24,
>                             AR3-25; the edge is BA-04 -> BA-10): the rules are symbolic and BA-04 applies them; no count of the
>                             catalog is part of these bytes
> REFERENCES (NOT EDGES)    = every reference to BA-04 V6, BA-07 V6, BA-08 V6 and BA-09 V6 in this artifact is an informative
>                             pointer, a mention and not a dependency (AR4-17). The section numbers of those pointers were verified
>                             against the V6 bytes of BA-04, BA-07, BA-08 and BA-09 (correction pass of round V6; re-verified by the
>                             generator of round V7; section 7)
> PREREQUISITES             = gate "before the candidate" (the values are written into the candidate bytes): the answers to Q-O1..Q-O5
>                             (Q-O5 by the Owner or by the CAD manager the Owner designates); the Coordinator values PARAM-04 and
>                             EVIDENCE_REPETITION and the recording of N_A, N_B, N_det and n; the read-only machine-attribute observation
>                             for the PARAM-05 label (future non-governing host gate planned under BASE-18, AR3-25; not authorized).
>                             The V5 prerequisite "the Architect's rulings on AQ-V5-06, AQ-V5-01, AQ-V5-02 and AQ-V5-05" is met:
>                             AR5-08, AR5-04, AR5-05 and AR5-07, section 220
> COST MEMO (NOT SEALED)    = illustrative arithmetic over the draft catalog; not part of these bytes, not a registry entry, not an
>                             input of any value. The memo over the V7 catalog is docs/automation/evidence/I-52-ct21d-cost-memo-v7.md
>                             and .py (the re-run over these V7 bytes that BA10V6-02 requires, over the BA-04 V7 catalog), named
>                             here by path only and with no hash, so that no cycle arises (the memo binds the bytes of this sheet by
>                             their SHA-256). The V6 memo (I-52-ct21d-cost-memo-v6.md and .py, over the V6 catalog; it binds the V6
>                             bytes of this sheet) and the V5 memo (I-52-ct21d-cost-memo-v5.md and .py) stay as history. Once this
>                             V7 and the V7 memo are reviewed, the V7 memo goes with Q-O1..Q-O5 in the Owner decision package
>                             (AR4-21), which the Coordinator's order of section 218 places after the candidate rulings and which
>                             needs explicit authorization
> BASE ITEM                 = BASE-18: NOT MET. It needs the prerequisites above and, outside this sheet, the BA-08 V6 section 10
>                             tolerances with the TOL_SCALE decision, test and record (V2.2 PA-9; K-3)
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 1. Rules of this sheet

- Every value is **`UNSET`** until it is chosen **with its stated intent**, by the **authority** named in its row, and recorded with its justification. A number without an intent is **prohibited**.
- **Owner-reserved choices** (risk appetite: `p_A`, `alpha_A`, `p_B`, `alpha_B`, `p_det`, `alpha_det`, `u`, `c`) are answered by the **Owner** only. The **machine-class policy** (Q-O5) is answered by the **Owner, or by the CAD manager if the Owner designates one** (AR2-43, AR3-24). Nobody else answers these questions: the Coordinator, the Architect and the implementer never choose them. The Coordinator **records** the counts that the formulas derive from the answers (`N_A`, `N_B`, `N_det`, `n`).
- **Sequence (AR2-43).** The contract is read as written: **every `MUST_FIX_BEFORE_AUTHORITY_BASELINE` value is fixed before `BASELINE_READY`** (V2.1 21.7, 22 `BASE-18`); no pin mechanism applies to these values. A value is never derived from the outputs of governing runs; non-governing evidence may later inform a **new baseline version**, never a silent change.
- **Symbolic sheet (AR3-24, AR3-25).** This sheet defines parameters, methods and repetition rules in terms of scenario properties. It takes no value from the scenario catalog: the catalog (BA-04) applies the rules, and the number of scenarios under each rule, `K` and every total are read from the **sealed** catalog when a rule is applied, the scenarios of both builds under test included (section 2.2, "Build under test"; AR4-06). The number of keys that each `HOST_DEFAULT_MAP` characterization scenario varies, `V(s)`, is read in the same way from the sealed catalog: each `HDM-CHAR-*` scenario declares the keys it varies, from the closed key list of the kind baseline (BA-08 V6 section 11.2, HDM-2 and HDM-4; informative pointers). Illustrative numbers live only in the non-sealed cost memo (header).
- **Operational parameters** (section 3) never change a semantic verdict.

## 2. `MUST_FIX_BEFORE_AUTHORITY_BASELINE` parameters

### 2.1 Summary

| Id | Parameter | Method (section 2.2) | Who chooses | Inputs needed | Current value |
|---|---|---|---|---|---|
| `PARAM-01` | **governing** runs per scenario that serves a class-A or class-B control group: `N_A`, `N_B` | zero-failure detection `N >= ln(alpha) / ln(1 - p)`, one pair per class; the strictest pair `N_strict(s)`, taken over the groups of `GROUPS_SERVED` only (AR4-14 (a)), is a floor for **every** governing scenario that serves a class-A or class-B control group, whatever its rule (AR3-24) | **Owner** (Q-O1: `p_A`, `alpha_A`; Q-O2: `p_B`, `alpha_B`); the Coordinator records `N_A`, `N_B` | `p_A`, `alpha_A`, `p_B`, `alpha_B` | `UNSET` |
| `WU_DRY_RUNS` | non-governing dry runs per dry-run scenario `WU-DRY-<s>` (V2.1 13.1: "the number of dry runs is a classified parameter, 21.7") | `WU_DRY_RUNS = N_B` (the class of `WU`, V2.2 PA-1.3) | **Owner** (Q-O2); the Coordinator records it. It is **not** a Coordinator value (AR3-24) | as `N_B` | `UNSET` |
| `PARAM-02` | determinism repetitions: the learning micro-scenarios, the non-governing composition check `EV-L-COMPOSED` and the event-model validation (V2.1 27.2, 27.4) | `N_det >= ln(alpha_det) / ln(1 - p_det)`; the reference by use (AR3-24): learning = the first run of the micro-scenario (`N_det + 1` runs, raised to `N_strict(s)` when the micro-scenario is governing and serves a class-A or class-B control group); composition check = `N_det + 1` runs (`CHAR_RUNS`, section 2.2; ruled by AR5-08, section 220); validation = `f(plan)` of the frozen EVM (`N_det` runs, no extra run) | **Owner** (Q-O3); the Coordinator records `N_det` | `p_det`, `alpha_det` | `UNSET` |
| `PARAM-03` | clean governing runs for the false-rejection bound (V2.1 10.6, 21.7) | `n >= ln(1 - c) / ln(1 - u)`; sample = the governing `CL-CLEAN-<fixture>` scenarios **plus** `EA2-DEFERRED-EVENTS`; `K` = their number in the sealed catalog; each member runs `max(N_strict(s), ceil(n / K))` times; `EA2-DEFERRED-EVENTS` runs on the `CHARACTERIZATION_BUILD` and counts for the `PRODUCT_BUILD` only under the recorded build equivalence (AR4-06 (6)) | **Owner** (Q-O4); the Coordinator records `n` | `u`, `c` | `UNSET` |
| `PARAM-04` | `MAX_ATTEMPTS` `m` **per planned governing run** (repetition slot) | a procedural cap fixed without host evidence (section 2.2) | **Coordinator** | the justification of section 2.2 | `UNSET` |
| `PARAM-05` | machine-class label and equivalence policy | a closed list of class-defining attributes and a label rule (section 2.2) | the **Owner, or the CAD manager the Owner designates**, confirms the policy (Q-O5); the label **value** is computed from a read-only host observation | the Q-O5 answer and the observation (future non-governing host gate) | `UNSET` |
| `EVIDENCE_REPETITION` | governing runs per scenario that serves no class-A or class-B control group (evidence groups, V2.2 PA-1.3 kind `EVIDENCE`); the base value of the rule `EVIDENCE_STRICT`; the runs of an exploratory scenario | Coordinator engineering choice: these runs carry **no** verdict about the host (V2.2 PA-3). **Draft proposal of the implementer**, written in this sheet since V4: **one** run per scenario; it is not a value and not a Coordinator position (AR4-21) | **Coordinator** | none | `UNSET` (implementer's draft proposal: 1) |
| `CHAR_RUNS` | runs per **non-governing** characterization scenario (no repetition slot; not `PARAM-01` runs) | derived (section 2.2; ruled by AR4-14 (b)): the `N_c` of the class `c` of the control that consumes the pin or the selection the scenario feeds (today `N_A`); for the four `HOST_DEFAULT_MAP` scenarios `HDM-CHAR-*`, `N_A + V(s)`; `N_det + 1` for the composition check `EV-L-COMPOSED` (ruled by AR5-08); `EVIDENCE_REPETITION` for an exploratory scenario | derived: no new choice and no Owner pair of its own (AR4-14 (b)(i)); Q-O1 discloses that its pair also sizes these runs, and the scope of the fail-closed statement that AR5-08 fixes (section 5; section 2.2 "Fail-closed") | as `N_A`, `N_det` and `EVIDENCE_REPETITION` | `UNSET` |

### 2.2 Methods

**Terms.**

- A **class-A or class-B control group** is a group of kind `CONTROL` with class A or B in the group table of V2.2 PA-1.3. `KR` is not one (class A, outside the predicate: it never contributes to `TRUE`, `FALSE` or `AMENDMENT_PENDING`, PA-4), and neither is a group of kind `EVIDENCE` (PA-3).
- The groups a scenario **serves** are its `GROUPS_SERVED`: the AR2-08 derivation (the groups of the controls it exercises, plus the declared evidence groups). The field is defined normatively in BA-04 section 1 (AR4-17; authority AR2-08, AR3-06, AR4-03), which applies this sheet; the citation is an informative pointer, not a dependency. "Serves groups A or B" in AR3-24 means `GROUPS_SERVED` (AR4-14 (a)). The groups that the contract assigns to a scenario as evidence (`CONTRACT_ASSIGNED_GROUPS`, AR3-06 and its extension by AR4-03) never carry a `PASS`, and `CONTRACT_ASSIGNED_GROUPS` never enter `N_strict` (ruled by AR4-14 (a), section 217).
- **`N_strict(s)`** = the largest `N_c` over the classes `c` of the class-A and class-B control groups of `GROUPS_SERVED(s)`: `N_A` when the scenario serves only class-A groups, `N_B` when it serves only class-B groups, and `max(N_A, N_B)` when it serves both (the **strictest pair**; BA10-02, AR2-43, AR3-24). No ordering of `N_A` and `N_B` is assumed.
- **Build under test (AR3-01, AR4-06).** Every scenario runs on one build under test, its `BUILD_UNDER_TEST`: the `PRODUCT_BUILD`, the `CHARACTERIZATION_BUILD`, or none for an offline evaluation (rule `SYNTH`). The rules apply to each scenario whatever its build. Every qualification control of V2.1 4.3.1 (V2.2 PA-12) runs on each build under test that governing runs use (AR4-06 (3); AR5-05: by content, not by id prefix): every `Q-*` and `E6-C*` scenario and also `PR-CLOSURE-COMPLETENESS`, `WU-BOUNDARY` and the `PW-CLONE-*` variants that the catalog twins (at least BASE and NESTED, AR5-05). The product-build scenario and its twin `<id>-CB` on the characterization build are two scenarios with the same rule, each with its own `R(s)` and, when governing, its own repetition slots (AR5-05 fixes `WU-BOUNDARY-CB` as governing, group `WU`, rule `DEFAULT` with `N_B`, and without a dry run; the twins are listed by the catalog, informative pointer). The I-14 family (`Q-I14` and every qualification vehicle `Q-I14-*`, AR4-01, among them `Q-I14-P` with `WU-DRY-Q-I14-P`, admitted by AR5-09; the vehicles are listed by the catalog, informative pointer) runs on the characterization build only (V2.1 4.3 row I-14). The counts `c(X)` of section 2.3 include the scenarios of both builds.
- A **repetition slot** is one of the `R(s)` planned governing runs of a governing scenario. Scheduled repeats are repeats, not retries (V2.1 21.3), so each slot is separate.

**PARAM-01 (zero-failure detection; BA10-02, AR3-24).** A governing scenario that serves a class-A or class-B control group runs at least `N_strict(s)` governing times (each a fresh process, V2.1 section 20). If a failure mode of that scenario occurs in a run with probability at least `p`, the probability that none of `N` runs shows it is at most `(1 - p)^N`; requiring it to be at most `alpha` gives

```text
N >= ln(alpha) / ln(1 - p)        (N rounded up)
```

- `p` = the smallest per-run occurrence probability of a mode that the characterization must be able to catch; `alpha` = the miss risk the Owner accepts. One pair per class: `N_A` from (`p_A`, `alpha_A`), `N_B` from (`p_B`, `alpha_B`).
- **Floor (AR3-24).** `N_strict(s)` is a floor for every governing scenario that serves a class-A or class-B control group, **whatever its rule**: the rules `DEFAULT`, `CLEAN`, `VALIDATE` and `EVIDENCE_STRICT` below all carry it, and so does `LEARN` for a governing learning micro-scenario that serves such a group.
- A governing FAIL or UNKNOWN is never re-run to obtain a better result (V2.1 21.3); `R(s)` counts planned governing runs, not attempts.
- **Declared modelling assumption (BA10-04):** the runs are treated as independent trials with a constant per-run probability; correlated failures (for example a machine state that persists across runs) make the bound optimistic. The assumption is disclosed in Owner Act 2.
- `PARAM-01` counts **governing** runs only. The non-governing runs follow `WU_DRY_RUNS`, `CHAR_RUNS` and, for a non-governing learning micro-scenario, `PARAM-02`.

**Repetition rules (the vocabulary BA-04 applies).** The rule of a scenario follows from its properties, taking the **first** row that matches:

| Rule | Scenarios (by their properties) | Planned runs `R(s)` | Repetition slots |
|---|---|---|---|
| `DRY` | a dry-run scenario `WU-DRY-<s>` (non-governing by design; AR3-02) | `WU_DRY_RUNS` = `N_B` | none |
| `LEARN` | **every** learning micro-scenario of the event model (V2.1 26.3 `EV-L-<operation>`, 27.2 step 2), governing or not, whatever groups it serves | `N_det + 1`; `max(N_strict(s), N_det + 1)` when it is governing and serves a class-A or class-B control group; in both cases the first run is the reference (`PARAM-02`) | `R(s)` when governing; none otherwise |
| `CHAR` | any other non-governing scenario: non-governing by design (the characterization that feeds a pin or a selection, the `HDM-CHAR-*` scenarios, the composition check `EV-L-COMPOSED`) or exploratory (V2.1 21.1; AR2-11, AR3-01, AR3-11, AR4-01, AR4-07) | `CHAR_RUNS(s)` | none |
| `CLEAN` | a member of the `PARAM-03` sample | `max(N_strict(s), ceil(n / K))` | `R(s)` |
| `SYNTH` | a governing scenario with no host run (offline evaluation of a synthetic input) that serves no class-A or class-B control group | `1` (the offline evaluation is deterministic; a repetition adds no detection power) | 1 |
| `VALIDATE` | a governing event-model validation scenario (V2.1 26.3 `EV-V-<plan>`, 27.2 step 6; group `E12-V`) | `max(N_strict(s), N_det)` | `R(s)` |
| `EVIDENCE` | a governing scenario that serves no class-A or class-B control group (its `CONTRACT_ASSIGNED_GROUPS` do not count, AR4-14 (a)) | `EVIDENCE_REPETITION` | `R(s)` |
| `EVIDENCE_STRICT` | a governing scenario of an evidence group that also serves a class-A or class-B control group (through the controls it exercises) | `max(N_strict(s), EVIDENCE_REPETITION)` | `R(s)` |
| `DEFAULT` | every other governing scenario (it serves a class-A or class-B control group) | `N_strict(s)` | `R(s)` |

`LEARN` comes before every rule of governing scenarios, so a learning micro-scenario never falls through to `EVIDENCE_STRICT` or `DEFAULT` and never loses its reference run: if the controls enforced in a governing learning run ever make it serve a class-A or class-B control group, only the floor `N_strict(s)` is added (R1/R2:BA10-V4-01). BA-04 V6 section 1 ("Enforced, not exercised"; AR4-01) keeps the controls that a learning micro-scenario enforces (E-03 under the `LOCK_MODE` pin, and every other fail-closed control) out of its `CONTROLS_EXERCISED`, so they add no group to its `GROUPS_SERVED`; their `PASS` is never coverage and a `FAIL` refuses the run (informative pointer). The rule `LEARN` holds with or without that reading: it matches the micro-scenario first, and `GROUPS_SERVED` only decides whether the floor `N_strict(s)` applies. `EV-L-COMPOSED` is not a learning micro-scenario: AR4-01 takes it out of the learning corpus and `LEARNING_CORPUS_HASH` (V2.1 14.3, 27.2, 27.3), so `LEARN` does not match it and it falls to `CHAR`.

**`WU_DRY_RUNS` (AR3-02, AR3-24; BA10-V3-04).** `WU_DRY_RUNS = N_B`. The dry runs measure the `WARMUP_COVERAGE_CRITERION` of the class-B group `WU` (V2.1 13.1; V2.2 PA-1.3) for each governing scenario that enters the governed window (one dry-run scenario each, AR3-02; the qualification vehicles `Q-I14-*` keep theirs, AR4-01; the learning runs have none, AR3-02). Intent: an assembly load inside the governed window that the dry-run measurement counts and that occurs in a dry run with probability at least `p_B` is observed with probability at least `1 - alpha_B`. The measurement rule is not changed here: a dry run excludes the loads of an assembly or module named by a designed injection of its source and records them apart as the stimulus (AR4-16; BA-09 V6 section 4, informative pointer). The Owner fixes `WU_DRY_RUNS` through Q-O2 and the Coordinator records it; it is **not** a Coordinator value. It counts valid dry runs: an `INVALID` dry run does not count (V2.1 21.3). BA-09 V6 (section 4 "Count", W-2) and BA-04 V6 (rule `DRY`) use the same wording (informative pointers).

**`CHAR_RUNS` (non-governing characterization; BA10-V3-08; ruled by AR4-14 (b), section 217).**

- **Pin- or selection-feeding runs.** A non-governing characterization scenario that feeds a **pin or a selection** (the `E04_ADMITTED_SET@R-1` pin from the `E4-LEARN-R1-*` runs under the chosen lock mode, which run under each `LOCK_MODE` candidate; the `LOCK_MODE` selection from `LK-*`; the `HOST_DEFAULT_MAP` pin from `HDM-CHAR-*`; AR2-11, AR3-01, AR3-11) runs `CHAR_RUNS(s) = N_c` times, where `c` is the class of the group whose control consumes the pin or the selection. The current consumers are E-04 (group `E4`), E-03 (group `LK`) and S-1..S-3 (groups `KS` and `SC-A`), all class A (V2.2 PA-1.3), so `CHAR_RUNS = N_A`. This derived `N_c` is accepted by AR4-14 (b)(i): these runs have no Owner pair of their own, and Q-O1 discloses that its pair also sizes them (section 5). These runs are on the `CHARACTERIZATION_BUILD` (AR3-01); a pin or a selection derived there applies to `PRODUCT_BUILD` runs only under the recorded build equivalence (AR4-06 (4)).
- **`HOST_DEFAULT_MAP` scenarios (`HDM-CHAR-*`; AR3-11, AR4-07; ruled by AR4-14 (b)(ii)).** There are four: `HDM-CHAR-FX1F`, `HDM-CHAR-FXANN`, `HDM-CHAR-FXDIM` and `HDM-CHAR-FXANNBLANK` (kept by AR6-02 on the legacy-state fixture `FX-ANN-BLANK`, whose instances the `FIXTURE_CONSTRUCTION_BUILD` constructs; its rule and runs are unchanged, decisions section 223.2). Each runs `CHAR_RUNS(s) = N_A + V(s)` times: `N_A` runs at the pinned key configuration plus **exactly one** informative run per varied key, where `V(s)` is the number of keys the scenario varies, declared by the scenario in the sealed catalog from the closed key list of the kind baseline (BA-08 V6 section 11.2, HDM-2 and HDM-4; informative pointers). The plan is determinate: no other authority adds runs. Only the `N_A` runs at the pinned key configuration carry the intent below; the varied-key runs show which attributes depend on which key, carry no intent and no verdict, and their entries are **marked informative** in the `HOST_DEFAULT_MAP` pin record. A governing lookup that resolves to an informative entry is `UNKNOWN` (O5 path), so an entry backed by a single run never carries governing weight (AR4-14 (b)(ii); BA-08 V6 HDM-5, informative pointer). This sheet holds no count of the keys.
- **Composition check (AR4-01; V2.1 27.2 step 4, 27.9).** `CHAR_RUNS(s) = N_det + 1` for the composition check `EV-L-COMPOSED`, the non-governing check of a composed plan against the DRAFT or UNDER_REVIEW model: it contributes no model entry and no verdict, and the events that no isolated entry explains are recorded as `UNEXPLAINED_EVENTS`, which block the freeze (V2.1 27.2 step 4). `N_det + 1` is the `PARAM-02` count of the learning micro-scenarios whose entries it composes: the first run is the reference of the composed multiset, the further `N_det` runs must reproduce it, and every run is also compared with the composition of the model entries (V2.1 27.9). The reference run keeps a difference between runs of the composed plan apart from a defect of the draft model, which is not yet reviewed. BA-04 V6 closes its question Q-V5-BA04-3 (the repetition of `EV-L-COMPOSED`) by a pointer to this bullet (informative pointer). The count is **ruled by AR5-08** (section 220): `CHAR_RUNS = N_det + 1` for `EV-L-COMPOSED` is accepted, derived from the pair of Q-O3, and the governing validation `EV-V` catches what escapes.
- **Intent:** a behaviour of the characterized property that occurs in a run with probability at least `p_A` (an admitted-set member, a lock property or conflict outcome, an effective attribute value at the pinned key configuration) is observed at least once in the runs that derive the pin or the selection, with probability at least `1 - alpha_A` (`(1 - p_A)^(N_A) <= alpha_A`). The intent is a detection bound on these characterization runs only; for the lock it covers every property of V2.1 3.4 that the `LK-*` runs measure, but a behaviour they miss is safe only where the next bullet says so. For `HDM-CHAR-*` the intent covers the `N_A` runs at the pinned key configuration only. For the composition check, an event of the composed plan that no isolated entry explains, or a difference from its first run, that occurs in a run with probability at least `p_det` is observed with probability at least `1 - alpha_det` (the `PARAM-02` intent). The same independence assumption applies.
- **Fail-closed (AR4-14 (b)(i)), with the scope that AR5-08 fixes.** The statement that AR4-14 (b)(i) requires of this sheet says exactly (AR5-08, section 220): "fail-closed for E-04 membership, P-L1..P-L4 and the S-1..S-3 lookups; P-L5..P-L7 rest on the V2.1 3.4 selection review". The derived count bounds the chance that a behaviour is missed. Within that scope a value or state that the characterization runs did not show is absent from the pin or the selection, and when a governing run observes it, the consuming control does not give `PASS`: E-04 finds a tuple that is readable and not in the admitted set, which is `FAIL` (`UNKNOWN` when a dimension is unreadable; V2.1 3.3); E-03 re-reads the lock at PL and in the continuity set, and a lock not acquired, a wrong mode or an unreadable state is not `PASS` (acquisition and held mode at PL, P-L1 and P-L2, and continuity, P-L3; V2.1 3.4; V2.1 5, row PL); P-L4 (compatibility with the seams, AUTH-12 callee transactions and library imports, measured by scenario runs of PREPARE-R, PREPARE-W and MUTATION; V2.1 3.4) is within the fail-closed scope by AR5-08; S-1..S-3 find no entry, an informative entry or entries that disagree for their key tuple, which is `UNKNOWN` (O5 path), and an effective value different from the entry is a comparison mismatch (BA-08 V6 HDM-5, informative pointer; the agreement rule is ruled by AR5-07: an entry in disagreement stays present and `UNKNOWN`, citing the runs, and BA-08 V6 HDM-7 declares that the rule detects dependencies outside the key set only in part). For these behaviours a miss costs a rejection (`FAIL` or `UNKNOWN`) of a legitimate governing run, never a `PASS` that the characterization did not support; in a clean governing run of the `PARAM-03` sample such a rejection is counted as a false rejection. **P-L5..P-L7 rest on the selection review.** A conflict outcome (P-L5: another execution context cannot write while the lock is held; P-L6: what the conflicting context observes) is measured only by the adversarial probe of the `LK-*` conflict runs, and P-L7 (the mode does not alter the host behaviour that other controls depend on: E-06 counts, E-12 events) only by the comparison of logs across candidate modes (V2.1 3.4, "Measured by"). E-03 in a governing run re-reads the held lock and its mode, not these properties, so a P-L5, P-L6 or P-L7 behaviour that the `LK-*` runs miss is not re-observed by a governing run and can stand behind a governing `PASS`; for these properties the safeguard is the `LOCK_MODE` selection review (V2.1 3.4: the selection rule is applied by review, and the result is `UNKNOWN` when a property is unmeasurable or results differ across runs), not the consuming control. Q-O1 discloses the scope (section 5).
- An **exploratory** scenario (V2.1 21.1: runs of an exploratory scenario are non-governing) runs `EVIDENCE_REPETITION` times: it answers a design question, feeds no pin and carries no verdict (today `E6-C4` and its twin `E6-C4-CB`, AR4-06 (3)).
- These runs are **not** governing runs, carry no group verdict (AR2-11, AR3-01) and have **no repetition slot** (outside `PARAM-04`). `CHAR_RUNS` counts valid runs: an `INVALID` run does not count and is re-run only with the Coordinator's authorization per occurrence (V2.1 21.3).

**PARAM-02 (determinism; reference by use; BA10-04, AR3-24).** With `N_det >= ln(alpha_det) / ln(1 - p_det)` (rounded up), a plan whose event multiset differs from the reference in a run with probability at least `p_det` shows the difference with probability at least `1 - alpha_det`. The reference depends on the use:

- **Learning micro-scenario** (rule `LEARN`): the reference is the event multiset of the **first run of that micro-scenario**; the further runs must reproduce it, so `N_det + 1` runs in all, or `max(N_strict(s), N_det + 1)` when the micro-scenario is governing and serves a class-A or class-B control group (the first run stays the reference). Under AR4-01 the learning micro-scenarios `EV-L-OC-*` are governing runs of the evidence group `E12-L` on the `PRODUCT_BUILD`.
- **Composition check** (rule `CHAR`, `EV-L-COMPOSED`): `N_det + 1` runs, with the two comparisons stated under `CHAR_RUNS` (ruled by AR5-08, section 220).
- **Event-model validation** (rule `VALIDATE`): the reference is **`f(plan)` of the frozen EVM** (V2.1 27.1; V2.2 PA-2); every governing validation run is compared with it, with **no extra run**, so `N_det` runs, raised to `N_strict(s)` by the floor. A validation plan has no learning run (V2.1 27.1, 27.3, 27.8). When every run equals `f(plan)`, the multiset is identical in all governing validation runs (the determinism criterion of V2.1 27.4); the criterion is evaluated over all the governing validation runs of the plan.
- The independence assumption of `PARAM-01` applies.

**PARAM-03 (false-rejection bound; BA10-01, AR2-28, AR3-24).** With `k` false rejections observed in `n` clean governing runs, the reported quantity is the one-sided Clopper-Pearson upper bound at confidence `c`. For `k = 0` it is `1 - (1 - c)^(1/n)`; the sample that makes this bound at most `u` is

```text
n >= ln(1 - c) / ln(1 - u)        (n rounded up)
```

- **Sample membership (AR3-24).** The governing `CL-CLEAN-<fixture>` scenarios of the sealed catalog (`CL-CLEAN-FXANNBLANK-FP` included, AR3-15, read as AR5-11 states and kept by AR6-02 on the legacy-state fixture `FX-ANN-BLANK`; its membership is unchanged) **plus** `EA2-DEFERRED-EVENTS` (the baseline runs without any writer of V2.1 10.6). `CL-CLEANUP-CHECK` (the cleanup evidence scenario of group `CL`) is not a `CL-CLEAN-<fixture>` scenario and is not a member; `EA1-READONLY-OPENS` has no product run (AR3-09) and is not a member.
- **Build (AR4-06 (6)).** `EA2-DEFERRED-EVENTS` runs on the `CHARACTERIZATION_BUILD` (its result depends on the I-14 witness); its clean governing runs count toward the bound of the `PRODUCT_BUILD` only under the recorded build equivalence (AR4-06 (4)). Without that record they do not count for the product build, and the shortfall is stated with the bound (the rule of the next bullets). Its runs also consume the `EVM_FROZEN` pin, which is learned and validated on the `PRODUCT_BUILD` (AR4-01); AR5-04 rules that the recorded equivalence is also the basis of that reverse direction: without the record cited in the run's `RUN_START`, `EVM_FROZEN` does not apply (the run does not start, or E12A and E12C cannot be `PASS`; AR5-04 (4)), and `EA2-DEFERRED-EVENTS` is itself the catalog's safeguard (AR5-04 (5)). The record and its use are BA-07 V6's (section 5.3 and U5; informative pointer). No rule and no count of this sheet changes.
- **`K`** = the number of scenarios of that sample in the **sealed** catalog. The rule is symbolic: this sheet holds no value of `K`.
- **Combination rule:** each member runs `max(N_strict(s), ceil(n / K))` governing times. The sample then holds at least `K * ceil(n / K) >= n` runs, and every member keeps its `PARAM-01` floor.
- Only the clean governing runs of the members count: a clean governing run contains no deliberate violation (V2.1 1.3), and the members run without any writer (V2.1 10.6). The bound is reported on the clean governing runs that exist; if a member lacks a required governing run (`PARAM-04` rule 5), the shortfall is stated with the bound.
- **A-1 and A-2** are evaluated on this clean sample, as V2.1 10.6 states ("baseline runs without any writer ... evaluate A-1 and A-2"); the scenario expectations are BA-04 V6's (section 3, "A-1 in a mirror run" and `CER-A2`; informative pointer).
- The contract sets **no threshold**: the observed rate and its bound are **reported** to the Owner (Act 2). Choosing `u` and `c` fixes only the sample size.

**PARAM-04 (`MAX_ATTEMPTS`; BA10-03, BA10-05, AR2-43, AR3-24).** The **unit is the repetition slot** (one planned governing run). The Coordinator fixes a **procedural cap** `m` **without host evidence**, with this justification:

1. `m >= 1` is the maximum number of attempts of **one slot**; `m = 1` means that no retry is possible.
2. A slot is filled by its first governing attempt, whatever its result. After an `INVALID` attempt, another attempt of the same slot needs the Coordinator's authorization per occurrence (V2.1 21.3); `m` does not replace that review, it bounds the review effort and the duration of the characterization. A crash first needs its ruling (V2.1 21.1); an attempt ruled `INVALID` counts like any other.
3. **Every attempt of the slot counts** toward `m`, including attempts made `INVALID` by contamination (AR2-43; V2.1 21.5).
4. `MAX_ATTEMPTS` never permits the retry of a governing FAIL or UNKNOWN (V2.1 21.3).
5. A slot that reaches `m` attempts without a governing attempt leaves the scenario **without a required governing run**: the groups the scenario serves **cannot be `PASS`** (V2.1 1.3: `PASS` needs every required governing run; V2.2 PA-3 item 1 for evidence groups). Without another non-`PASS` verdict, the group state is `NOT_EVALUATED` (V2.1 1.4 row `INVALID` and 1.6; V2.2 PA-3: "an incomplete group yields `NOT_EVALUATED`"). A governing FAIL or UNKNOWN observed in any run still counts for the group of that control (AR2-08; V2.1 21.3: every governing attempt counts).
6. Non-governing runs (rules `DRY` and `CHAR`, and a non-governing `LEARN`) have **no slot** and are outside `m`; an `INVALID` non-governing run is re-run only with the per-occurrence authorization of V2.1 21.3, and every attempt stays in the attempt log.
7. A later revision of `m` informed by the `INVALID` rates of non-governing dry runs is a **new baseline version**. The V2 method (a bound `beta` on consecutive invalid attempts at the observed rate) stays withdrawn: it needed host evidence before the baseline.

**PARAM-05 (machine class; BA10-06, AR2-43, AR3-24).** Proposed **conservative** policy, for confirmation by the **Owner, or by the CAD manager if the Owner designates one** (a restriction, never an extension of the guarantee):

| Aspect | Proposal |
|---|---|
| equivalence rule | **no cross-machine equivalence**: one machine class = one physical machine installation |
| class-defining attributes (closed list) | (1) the Windows `MachineGuid`; (2) the OS version and build; (3) the AutoCAD product identity; (4) the AutoCAD profile identity; (5) the value of `SECURELOAD`; (6) the value of `TRUSTEDPATHS` |
| not class-defining | the **machine identity class** field itself (V2.1 2.1: it is the output, the label); the machine's own **module entries** (drivers, security software) of the manifest machine layer, which stay a `MACHINE_PROFILE_BOUND` tuple field of their own, compared through the manifest hash (V2.1 2.1, 2.2, 29; the manifest is captured at EXEC-11, after the baseline); every `BUILD_BOUND` field (AutoCAD version and build, `acad.exe` SHA-256, API assemblies, and the build-dependent fields of each build under test, AR4-06 (1), (2)), which stays in the build tuple |
| label | `MC-` + the first 12 lowercase hexadecimal digits of the SHA-256 of the canonical serialization below; the clear `MachineGuid` stays in local evidence (V2.1 2.1) |
| canonical serialization (defined here) | the RFC 8785 (JCS) serialization, in UTF-8, of a JSON object with exactly the six keys `AutoCadProduct`, `AutoCadProfile`, `MachineGuid`, `OsVersionBuild`, `SECURELOAD`, `TRUSTEDPATHS`; every value is a JSON string holding the exact text that the observation read (`SECURELOAD` as its decimal digits); JCS sorts the keys and adds no whitespace |
| change of a class-defining attribute | a new machine class (a new tuple) |
| change of a module entry | no change of the label; a mismatch of its own `MACHINE_PROFILE_BOUND` field (V2.1 2.2) |
| label value | computed from a **read-only host read** of the six attributes on the characterization machine (for example registry and OS queries and the AutoCAD profile and system-variable values). It needs **no implemented instrument and no manifest**, and changes nothing. It belongs to the future non-governing host gate whose plan the BA-11 registry records under `BASE-18` (AR3-25); that gate is **not authorized** now. The observation record keeps the six strings, and the label is recomputed from it. The observation is a prerequisite of this sheet **before the candidate** (header), because the label value is written into these bytes |

**EVIDENCE_REPETITION (Coordinator).** It applies to the rule `EVIDENCE`, is the base value of `EVIDENCE_STRICT` (where the floor `N_strict(s)` governs when larger) and is the count of an exploratory scenario. The proposal of one run per scenario (section 2.1) is the implementer's draft proposal for the Coordinator value, not a value and not a Coordinator position (AR4-21). A scenario that serves a class-A or class-B group only through `CONTRACT_ASSIGNED_GROUPS` (for example `PR-CLOSURE-COMPLETENESS`, group `Q` with `PR-REC` assigned by V2.2 PA-12, AR3-06) carries evidence and never a `PASS`, so it stays under `EVIDENCE`; the X7 and residual cells keep the `N_strict` of their `GROUPS_SERVED` (ruled by AR4-14 (a), section 217).

### 2.3 Total runs implied by the choices (symbolic)

`c(X)` is the number of scenarios of rule `X` in the **sealed** catalog, the scenarios of both builds under test included (a qualification twin `<id>-CB` is a scenario of its own; AR4-06 (3)), and `c_A(X)`, `c_AB(X)`, `c_B(X)` split it by the classes of the control groups of `GROUPS_SERVED` (class-A only, both classes, class-B only). This sheet holds **no** value of `c(X)`, `K`, `V(s)` or `TOTAL`.

```text
TOTAL = G + D

G (planned governing runs = repetition slots)
  =   c_A(DEFAULT) * N_A + c_AB(DEFAULT) * max(N_A, N_B) + c_B(DEFAULT) * N_B
    + sum over CLEAN            of max(N_strict(s), ceil(n / K))
    + sum over VALIDATE         of max(N_strict(s), N_det)
    + sum over EVIDENCE_STRICT  of max(N_strict(s), EVIDENCE_REPETITION)
    + c(LEARN, governing, serving no class-A or class-B group) * (N_det + 1)
    + sum over LEARN, governing, serving a class-A or class-B group, of max(N_strict(s), N_det + 1)
    + c(EVIDENCE) * EVIDENCE_REPETITION + c(SYNTH) * 1

D (non-governing runs; no slot)
  =   c(DRY) * N_B + c(LEARN, non-governing) * (N_det + 1)
    + sum over CHAR of CHAR_RUNS(s), that is
        c(CHAR, pin or selection, other than HOST_DEFAULT_MAP) * N_A
      + sum over CHAR of HOST_DEFAULT_MAP of (N_A + V(s))          (V(s) = the number of keys the scenario varies)
      + c(CHAR, composition check) * (N_det + 1)
      + c(CHAR, exploratory) * EVIDENCE_REPETITION

worst-case governing attempts = m * G        (PARAM-04, per slot)
```

The `SYNTH` runs are offline evaluations (no build under test); every other run is a fresh AutoCAD process (V2.1 section 20) on the build under test of its scenario, and `G` and `D` split by build in the same way (the catalog field `BUILD_UNDER_TEST`). The cost memo (header) evaluates this expression on the current draft catalog for illustrative choices, gives the split by build and the sensitivities; its figures are not part of this sheet.

The governance of the learning micro-scenarios and of the qualification vehicles is ruled by AR4-01 (section 217): the learning micro-scenarios `EV-L-OC-*` are governing runs of the evidence group `E12-L` on the `PRODUCT_BUILD`; `EV-L-COMPOSED` is the non-governing composition check (`CHAR`); the `Q-I14-*` vehicles are governing evidence of group `Q` on the `CHARACTERIZATION_BUILD` and keep their dry runs. The rules do not depend on the controls enforced in a learning run: `LEARN` matches every learning micro-scenario, and the groups it serves only add the floor `N_strict(s)` when it is governing; its place in `G` or `D` follows its governance. A dry run exists only for a governing scenario that enters the governed window (AR3-02). Every count `c(X)` is read from the sealed catalog.

## 3. Operational-only parameters (do not affect a semantic verdict)

| Id | Parameter | Classification | Rule |
|---|---|---|---|
| `OP-01` | control-plane time budgets or watchdogs | `SHOULD_BE_REMOVED` from the semantic verdict | a slow measurement never becomes `UNKNOWN`; an expired watchdog makes the **run or instrument `INVALID`**, is logged and counts in the attempt log |
| `OP-02` | duration of the post-CP observation queue | `MAY_BE_OPERATIONAL_AT_EXECUTION` | informational only; recorded |

## 4. Values that are **not** parameters of this sheet (each with its own gate; BA10-07)

Only the first row is part of `BASE-18`. The other rows have their own gates and are **not** `BASE-18` items.

| Item | Where it lives | Gate |
|---|---|---|
| tolerances of `SEMANTIC_COMPARISON`, including the `TOL_SCALE` decision, test and record | BA-08 V6 section 10 | **part of `BASE-18`** (V2.2 PA-9); K-3; a prerequisite of BA-08, not of this sheet |
| evidence-derived pins: `E04_ADMITTED_SET@R-1`, `LOCK_MODE`, `EVM_FROZEN`, `HOST_DEFAULT_MAP`, `FIXTURE_INSTANCE@<FX>` | pin records (AR2-16), with their own custody in BA-07 V6 section 5.1 (`RECORDED -> RATIFIED -> ANCHORED`, AR3-23; one COORDINATOR and one ARCHITECT ratification, at most one live pin per slot, and a fail-closed carry-over to a new baseline version: AR4-13; no replacement of a consumed pin within a baseline version, with "consumed" including a pin used to derive another: AR5-06; the `FIXTURE_INSTANCE@FX-ANN-BLANK` pin cites the fixture conformance record that carries the `FIXTURE_CONSTRUCTION_BUILD` identity: AR6-02); the slots are named by the scenarios of the catalog | **not `BASE-18`**: each pin value lives in its own pin record, never in these bytes, and is fixed **before the first governing run that uses it** (AR2-11, AR2-16, AR2-17, AR3-26); a pin derived on the `CHARACTERIZATION_BUILD` applies to `PRODUCT_BUILD` runs only under the recorded build equivalence (AR4-06 (4)); in the reverse direction, the `EVM_FROZEN` pin derived on the `PRODUCT_BUILD` applies to governing `CHARACTERIZATION_BUILD` runs under the same recorded equivalence (AR5-04; BA-07 V6 section 5.3, informative pointer) |
| E-04 designated route | BA-08 V6 section 8 | **not `BASE-18`**: `BASE-3`, with the Coordinator's ratification (K-5) and the conditions of AR3-14 |

## 5. Owner decision questions (prepared; **NOT asked**)

These questions are **not put to the Owner until this V7 sheet and the cost memo over the V7 catalog (`docs/automation/evidence/I-52-ct21d-cost-memo-v7.md` and `.py`, the re-run over these V7 bytes that BA10V6-02 requires) are reviewed**. Then they go to the Owner together with that memo in the Owner decision package (AR4-21, section 217), which the Coordinator's order of section 218 places after the candidate rulings and which needs explicit authorization. Q-O1..Q-O4 are answered by the **Owner** only; Q-O5 is answered by the **Owner, or by the CAD manager if the Owner designates one** (AR2-43, AR3-24). Nobody else answers them. Until they are answered, `BASE-18` is not met and `CT21D_AUTHORITY_BASELINE_READY` cannot become `TRUE`.

| Question | Who answers | What it decides | Effect |
|---|---|---|---|
| **Q-O1** | Owner | `p_A` and `alpha_A` for the scenarios of **class-A** groups (the guarantee-bearing controls) | fixes `N_A` of `PARAM-01` and, with the **same pair** (no separate pair is asked, AR4-14 (b)(i)), `CHAR_RUNS` of the pin- and selection-feeding characterization runs (`E4-LEARN-R1-*`, `LK-*`) and the `N_A` runs at the pinned key configuration of the four `HDM-CHAR-*`: their consumers are class A. A behaviour these runs miss is "fail-closed for E-04 membership, P-L1..P-L4 and the S-1..S-3 lookups; P-L5..P-L7 rest on the V2.1 3.4 selection review" (AR5-08; section 2.2, "Fail-closed"). A P-L5..P-L7 behaviour that the `LK-*` runs miss is not re-observed by a governing run and can stand behind a governing `PASS`; its safeguard is the `LOCK_MODE` selection review (V2.1 3.4) |
| **Q-O2** | Owner | `p_B` and `alpha_B` for the scenarios of **class-B** groups (risk-reduction controls), or "same as class A" | fixes `N_B` of `PARAM-01` and `WU_DRY_RUNS = N_B` |
| **Q-O3** | Owner | `p_det` and `alpha_det` for the determinism of the expected event set | fixes `N_det` of `PARAM-02` (`N_det + 1` learning runs, raised to `N_strict` for a governing micro-scenario that serves a class-A or class-B group; `N_det + 1` composition-check runs, ruled by AR5-08; `max(N_strict, N_det)` validation runs) |
| **Q-O4** | Owner | `u` and `c` for the false-rejection bound to be reported | fixes `n` of `PARAM-03` |
| **Q-O5** | Owner, or the CAD manager the Owner designates | confirmation (or change) of the machine-class policy of section 2.2, and the machine on which the characterization will run | fixes the `PARAM-05` policy; the label value needs the attribute observation |

**Consequences to weigh** (the figures are in the cost memo):

- `TOTAL` scales roughly with `1 / p` (for small `p`, `N` is close to `ln(1 / alpha) / p`) and only **logarithmically** with `1 / alpha`.
- `u` and `c` enter only through `ceil(n / K)`. They change `TOTAL` only when `ceil(n / K)` exceeds `N_strict` of a sample member, and then only the `CLEAN` term grows.
- `N_B` drives the dry-run term `c(DRY) * N_B` (one dry-run scenario per governing scenario that enters the governed window) and the class-B-only terms; when `N_B > N_A`, it also drives every mixed-class term.
- `N_det` drives the learning and composition-check runs, and the validation runs only above `N_strict`.
- `EVIDENCE_REPETITION` drives the `EVIDENCE` term and the exploratory runs, and `EVIDENCE_STRICT` only while it exceeds `N_strict`.
- The qualification scenarios run on each build under test (AR4-06 (3), AR5-05): every qualification control outside the I-14 family (every `Q-*` and `E6-C*`, `PR-CLOSURE-COMPLETENESS`, `WU-BOUNDARY` and the twinned `PW-CLONE-*` variants) is counted twice, once per build, with its own repetitions.
- One more `PARAM-03` sample member adds its own runs and its dry runs, and lowers `ceil(n / K)` for the others.
- `m` bounds the governing attempts at `m * G` in the worst case.
- The informative runs of `HDM-CHAR-*` (exactly one per varied key, AR4-14 (b)(ii)) do not depend on any Owner answer.

```text
PARAM-01 .. PARAM-05 = UNSET     WU_DRY_RUNS = N_B (UNSET)     CHAR_RUNS = derived (UNSET)
EVIDENCE_REPETITION = UNSET (implementer's draft proposal: 1; not a Coordinator position)
OP-01, OP-02 = OPERATIONAL
BASE-18 = NOT MET     OWNER DECISIONS Q-O1 .. Q-O5 = NOT ASKED (Owner package after the review of this V7 and the memo over the
                      V7 catalog, docs/automation/evidence/I-52-ct21d-cost-memo-v7.md and .py, AR4-21; explicit authorization
                      required, section 218)
OPEN ARCHITECT QUESTIONS OF THIS SHEET = NONE IN THESE BYTES (AQ-V5-06, AQ-V5-01, AQ-V5-02, AQ-V5-05 ruled by AR5-08, AR5-04,
                      AR5-05, AR5-07; section 6.1)
CT21D_AUTHORITY_BASELINE_READY = FALSE
```

## 6. Architect rulings on the questions of rounds V4 (section 217) and V5 (section 220)

The V4 open question AQ-V4-13 of this sheet and the question AQ-V4-01 that its section 2.3 referenced are ruled (section 217), and so are the round V5 questions that could change these bytes (section 220; section 6.1). Nothing in this section is decided here; the tables record where each ruling is applied.

| Id | Ruling | Applied in |
|---|---|---|
| AQ-V4-13 (a) = Q-A1 | ruled by AR4-14 (a), section 217: `CONTRACT_ASSIGNED_GROUPS` do not enter `N_strict`; "serves groups A or B" (AR3-24) = `GROUPS_SERVED`. `PR-CLOSURE-COMPLETENESS` stays under `EVIDENCE`; the X7 and residual cells keep the `N_strict` of their `GROUPS_SERVED` | 2.1 `PARAM-01`; 2.2 terms, rule `EVIDENCE`, "EVIDENCE_REPETITION" |
| AQ-V4-13 (b)(i) | ruled by AR4-14 (b)(i): the derived `N_c` (today `N_A`); no new Owner question, on two conditions: Q-O1 discloses that the same pair sizes these runs, and this sheet states that a missed behaviour fails closed | 2.1 `CHAR_RUNS`; 2.2 `CHAR_RUNS` (first bullet and "Fail-closed", with the scope that AR5-08 fixes, section 6.1); section 5 Q-O1 |
| AQ-V4-13 (b)(ii) | ruled by AR4-14 (b)(ii): for `HDM-CHAR-*`, `N_A` runs at the pinned key configuration plus exactly one informative run per varied key (determinate; no unnamed authority adds runs); the varied-key entries are marked informative in the pin record, and a governing lookup that resolves to one is `UNKNOWN` (O5) | 2.1 `CHAR_RUNS`; 2.2 `CHAR_RUNS`; 2.3 term `D`; section 5 |
| AQ-V4-01 | ruled by AR4-01, section 217: `EV-L-*` governing, group `E12-L`, on the `PRODUCT_BUILD`; `EV-L-COMPOSED` a non-governing composition check outside the learning corpus; `Q-I14-*` governing evidence of group `Q` on the `CHARACTERIZATION_BUILD`, with their dry runs | 2.2 rule `LEARN` and the paragraph after the table, `CHAR_RUNS` (composition check), `PARAM-02`; 2.3 closing paragraph |
| Q-V5-BA04-3 of BA-04 (the repetition of `EV-L-COMPOSED`; informative pointer) | BA-04 closes it by a pointer to the rule of this sheet, `CHAR_RUNS(s) = N_det + 1`, which AR5-08 accepts (section 6.1) | 2.2 `CHAR_RUNS` (composition check) |

### 6.1 Architect rulings on the questions of round V5 (section 220)

The V5 sheet recorded four questions whose rulings could change its sentences (AQ-V5-06 of this sheet; AQ-V5-01, AQ-V5-02 and AQ-V5-05 raised on sibling artifacts) and three for completeness. All are ruled by section 220. No question of round V6 is raised on this sheet.

| Id | Ruling (section 220) | Applied in |
|---|---|---|
| **AQ-V5-06** (this sheet) | **AR5-08**: `CHAR_RUNS = N_det + 1` for `EV-L-COMPOSED` accepted (derived from the pair of Q-O3; the governing validation `EV-V` catches what escapes); the fail-closed statement says exactly: "fail-closed for E-04 membership, P-L1..P-L4 and the S-1..S-3 lookups; P-L5..P-L7 rest on the V2.1 3.4 selection review" | 2.1 rows `PARAM-02` and `CHAR_RUNS`; 2.2 `CHAR_RUNS` (composition check; "Fail-closed", the words verbatim and P-L4 inside the scope) and `PARAM-02` (composition check); section 5 Q-O1 (the words verbatim) and Q-O3 |
| **AQ-V5-01** (raised on BA-04 V5) | **AR5-04**: yes, as a reading: the recorded equivalence of AR4-06 (4) is also the basis of the reverse direction (`EVM_FROZEN` consumed by governing `CHARACTERIZATION_BUILD` runs), with its conditions; the record and `U5` are BA-07 V6's (informative pointer) | 2.2 `PARAM-03` ("Build"); section 4 (pin row). No rule and no count changes |
| **AQ-V5-02** (raised on BA-04 V5 and BA-09 V5) | **AR5-05**: yes, by content and not by id prefix: AR4-06 (3) reaches every qualification control of V2.1 4.3.1 (V2.2 PA-12); twins `PR-CLOSURE-COMPLETENESS-CB`, `WU-BOUNDARY-CB` (governing, group `WU`, `DEFAULT: N_B`, no dry run) and `-CB` twins of the `PW-CLONE-*` variants (at least BASE and NESTED) | 2.2 "Build under test"; section 5 ("Consequences to weigh"). No rule changes: a twin takes the rule of its source, and `WU-BOUNDARY-CB` takes the rule the ruling names |
| **AQ-V5-05** (raised on BA-08 V5) | **AR5-07**: the HDM-5 agreement rule is accepted (an entry in disagreement stays present and `UNKNOWN`, citing the runs); HDM-7 declares that the rule detects dependencies outside the key set only in part | 2.2 `CHAR_RUNS` ("Fail-closed"), the S-1..S-3 clause |
| AQ-V5-08 (raised on BA-04 V5) | **AR5-09**: `Q-I14-P` and `WU-DRY-Q-I14-P` admitted under AR4-01 | 2.2 "Build under test" (the I-14 family); the V5 row CP-04 left the vehicle list to the catalog, so no other sentence changes |
| AQ-V5-03, AQ-V5-04, AQ-V5-07 (raised on BA-06 V5 and BA-07 V5) | **AR5-03**, **AR5-06** and **AR5-02**: the BA-06 inventory corrections, the BA-07 custody readings and the scope of the BA-06 frozen parts | recorded for completeness; only section 4 (pin row) points to the BA-07 V6 custody that AR5-06 rules; this sheet states nothing about the BA-06 inventory |

This sheet does not state that no further question can be raised on it; a new question would be recorded here with its own id.

## 7. Delta V7 (decisions 226)

Sections 1 to 6 keep their V6 numbers. The Delta V6 table (decisions sections 220 and 223) is not repeated: it stays in the V6 file (blob `3f3d2d4f76ca5fb70fbb95f2cf19bbb7cd66afd9`), which is kept as history, and the Delta V5 table in the V5 file. **Code facts:** none; V7 changes no rule, method, count, value or pointer target, and classifies or re-derives no fact. Every value stays `UNSET`. V7 applies decisions section 226 and nothing more; the V6 review evidence is `docs/automation/evidence/I-52-ct21d-relink-v6-architect-review.json`.

| Item | Decision | Fix (section / field) |
|---|---|---|
| BA10V6-01 (minor; AR7-09) | section 226 | **APPLIED.** The AR5-08 statement stays verbatim. Outside the quotation, the consequence disclosure that V6 had dropped is restored: in 2.2 `CHAR_RUNS` ("Fail-closed") the V5 words "and can stand behind a governing `PASS`" return to the sentence on a P-L5, P-L6 or P-L7 behaviour that the `LK-*` runs miss; in section 5 Q-O1 one sentence follows the quotation: a P-L5..P-L7 behaviour that the `LK-*` runs miss is not re-observed by a governing run and can stand behind a governing `PASS`, and its safeguard is the `LOCK_MODE` selection review (V2.1 3.4). This is the disclosure to the Owner on which AR4-14 (b)(i) conditions the derived count. No rule or count changes |
| BA10V6-02 (minor; AR7-09) | section 226 | **APPLIED.** The V6 row "labels" said that a certain label did not occur in the sheet, which was wrong. The statement now reads: no pointer to BA-06 content or section numbers occurs in this sheet; BA-06 is named only in the history of rulings (section 6.1), in the `NOT_APPLICABLE` row and in this row. The pointers of the V6 row "labels" (BA-04 V6, BA-07 V6, BA-08 V6 and BA-09 V6 and their section numbers) are unchanged and are re-verified against the V6 bytes by the generator of round V7 (header `REFERENCES (NOT EDGES)`) |
| cost memo | AR4-21; BA10V6-02 | **PRODUCED OUTSIDE this artifact.** The memo over the V7 catalog, `docs/automation/evidence/I-52-ct21d-cost-memo-v7.md` and `.py`, is the re-run over these V7 bytes that BA10V6-02 requires (its A.7 lines are unaffected); it binds these bytes by their SHA-256 and supersedes the V6 memo (`docs/automation/evidence/I-52-ct21d-cost-memo-v6.md` and `.py`, which binds the V6 bytes and stays as history). The header, section 5 and its status block point to the V7 memo by path only, with no hash (no cycle) |
| editorial | — | title, banner and header in the V7 form (`ARTIFACT_VERSION` = 7-DRAFT superseding the V6 blob; `DELTA RULING APPLIED` names section 226); section 5 and its status block name this V7 |
| AR7-01 to AR7-08; the other minors of AR7-09 | section 226 | **NOT_APPLICABLE** to these bytes: they rule BA-06, the contract V2.6, BA-07, BA-08, BA-09, BA-04, BA-02b, BA-11 and BA-03; no parameter, method or rule of this sheet depends on them |
| correction pass (V7 critic) | AR4-21 | the header, section 5, the status block and the "cost memo" row point by path only to `docs/automation/evidence/I-52-ct21d-cost-memo-v7.md` and `.py`, the memo over the V7 catalog; the memo was re-run over these bytes |
