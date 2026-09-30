# I-52 — CT-21D Baseline Artifact BA-10: `MUST_FIX` Parameter Sheet and Selection Methods (DRAFT V4)

> **BASELINE ARTIFACT BA-10 V4 — DRAFT, NOT SEALED. Design / documentation only. No value is chosen: every value is `UNSET`. The sheet is symbolic: it holds no count, no `K` and no total taken from the scenario catalog.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-PARAMETER-SHEET
> ARTIFACT_VERSION          = 4-DRAFT   (supersedes 3-DRAFT, blob d4e0ac99ff85912338c1ca56f9e655690fd44bd4, which stays as history)
> STATUS AUTHORITY          = the BA-11 registry entry of this artifact is the only authority for its status and hashes;
>                             this header is frozen, non-authoritative text of the candidate bytes (BA-11 V4 section 2)
> AUTHORITY_CONTRACT        = CT-21D V2.1 + V2.2 + V2.3 + V2.4 (draft revision 3) + V2.5 (draft revision 1); V2.4 and V2.5 pending
>                             Coordinator textual verification
> CONTRACT CLAUSES USED     = V2.1 1.3, 1.4, 1.6, 2.1, 2.2, 10.6, 13.1, 20, 21.1, 21.3, 21.5, 21.7, 22 (BASE-3, BASE-18), 26.3, 27.1-27.4,
>                             27.8, 29; V2.2 PA-1.3, PA-2, PA-3, PA-4, PA-9
> DELTA RULING APPLIED      = decisions section 214: AR3-24 (WU_DRY_RUNS = N_B; MAX_ATTEMPTS per repetition slot; PARAM-03 sample and
>                             symbolic K; PARAM-05 attributes, observation and Q-O5 authority; PARAM-02 reference by use; strictest pair on
>                             every governing scenario of groups A or B; totals and sensitivities in a memo not asked before review),
>                             AR3-25 (DEPENDS_ON separate from PREREQUISITES; no edge to BA-04; BASE-18 host-gate plan), AR3-06
>                             (contract-assigned groups: evidence, never PASS), AR3-01, AR3-02, AR3-11 (non-governing characterization and
>                             dry runs; correction pass: the `CHAR_RUNS` of the `HOST_DEFAULT_MAP` runs `HDM-CHAR-*`), AR3-14 (E-04 route),
>                             AR3-26 (pins fixed before the first governing run), AR3-30 (minors); section 211 (AR2-28, AR2-43) where
>                             section 214 does not change it
> ARCHITECT QUESTIONS       = referenced, none decided here: AQ-V4-13 (section 6: part (a) = Q-A1, contract-assigned groups and
>                             `N_strict`; part (b), the intent of `CHAR_RUNS`); AQ-V4-01 (section 2.3: the governance of `EV-L-*` and
>                             `Q-I14-*` in BA-04 V4)
> PHASE-2 RULING (V3, kept) = BA10-01..BA10-07 (AR2-28, AR2-43)
> DEPENDS_ON                = BA-01   (registry entries only; BA-11 V4 sections 2.1 and 3). No edge to BA-04 (AR3-24, AR3-25): the rules
>                             are symbolic and BA-04 applies them; no count of the catalog is part of these bytes
> PREREQUISITES             = gate "before the candidate" (the values are written into the candidate bytes): the answers to Q-O1..Q-O5
>                             (Q-O5 by the Owner or by the CAD manager the Owner designates); the Coordinator values PARAM-04 and
>                             EVIDENCE_REPETITION and the recording of N_A, N_B, N_det and n; the read-only machine-attribute observation
>                             for the PARAM-05 label (future non-governing host gate planned under BASE-18, AR3-25; not authorized); the
>                             Architect's answer to AQ-V4-13 (section 6) or its recorded deferral
> COST MEMO (NOT SEALED)    = docs/automation/evidence/I-52-ct21d-cost-memo-v4.md and .py: illustrative arithmetic over the draft catalog
>                             (and the key list of the draft BA-08 V4 for `HDM-CHAR-*`); not part of these bytes, not a registry entry,
>                             not an input of any value; not put to the Owner until this V4 is reviewed (AR3-24)
> BASE ITEM                 = BASE-18: NOT MET. It needs the prerequisites above and, outside this sheet, the BA-08 V4 section 10
>                             tolerances with the TOL_SCALE decision, test and record (V2.2 PA-9; K-3)
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 1. Rules of this sheet

- Every value is **`UNSET`** until it is chosen **with its stated intent**, by the **authority** named in its row, and recorded with its justification. A number without an intent is **prohibited**.
- **Owner-reserved choices** (risk appetite: `p_A`, `alpha_A`, `p_B`, `alpha_B`, `p_det`, `alpha_det`, `u`, `c`) are answered by the **Owner** only. The **machine-class policy** (Q-O5) is answered by the **Owner, or by the CAD manager if the Owner designates one** (AR2-43, AR3-24). Nobody else answers these questions: the Coordinator, the Architect and the implementer never choose them. The Coordinator **records** the counts that the formulas derive from the answers (`N_A`, `N_B`, `N_det`, `n`).
- **Sequence (AR2-43).** The contract is read as written: **every `MUST_FIX_BEFORE_AUTHORITY_BASELINE` value is fixed before `BASELINE_READY`** (V2.1 21.7, 22 `BASE-18`); no pin mechanism applies to these values. A value is never derived from the outputs of governing runs; non-governing evidence may later inform a **new baseline version**, never a silent change.
- **Symbolic sheet (AR3-24, AR3-25).** This sheet defines parameters, methods and repetition rules in terms of scenario properties. It takes no value from the scenario catalog: the catalog (BA-04) applies the rules, and the number of scenarios under each rule, `K` and every total are read from the **sealed** catalog when a rule is applied. The number of key variables that the `HOST_DEFAULT_MAP` runs vary is read in the same way from the sealed kind baseline (section 2.2, `CHAR_RUNS`). Illustrative numbers live only in the non-sealed cost memo (header).
- **Operational parameters** (section 3) never change a semantic verdict.

## 2. `MUST_FIX_BEFORE_AUTHORITY_BASELINE` parameters

### 2.1 Summary

| Id | Parameter | Method (section 2.2) | Who chooses | Inputs needed | Current value |
|---|---|---|---|---|---|
| `PARAM-01` | **governing** runs per scenario that serves a class-A or class-B control group: `N_A`, `N_B` | zero-failure detection `N >= ln(alpha) / ln(1 - p)`, one pair per class; the strictest pair `N_strict(s)` is a floor for **every** governing scenario that serves a class-A or class-B control group, whatever its rule (AR3-24) | **Owner** (Q-O1: `p_A`, `alpha_A`; Q-O2: `p_B`, `alpha_B`); the Coordinator records `N_A`, `N_B` | `p_A`, `alpha_A`, `p_B`, `alpha_B` | `UNSET` |
| `WU_DRY_RUNS` | non-governing dry runs per dry-run scenario `WU-DRY-<s>` (V2.1 13.1: "the number of dry runs is a classified parameter, 21.7") | `WU_DRY_RUNS = N_B` (the class of `WU`, V2.2 PA-1.3) | **Owner** (Q-O2); the Coordinator records it. It is **not** a Coordinator value (AR3-24) | as `N_B` | `UNSET` |
| `PARAM-02` | determinism repetitions: the learning micro-scenarios and the event-model validation (V2.1 27.4) | `N_det >= ln(alpha_det) / ln(1 - p_det)`; the reference by use: learning = the first run of the micro-scenario (`N_det + 1` runs); validation = `f(plan)` of the frozen EVM (`N_det` runs, no extra run) | **Owner** (Q-O3); the Coordinator records `N_det` | `p_det`, `alpha_det` | `UNSET` |
| `PARAM-03` | clean governing runs for the false-rejection bound (V2.1 10.6, 21.7) | `n >= ln(1 - c) / ln(1 - u)`; sample = the governing `CL-CLEAN-<fixture>` scenarios **plus** `EA2-DEFERRED-EVENTS`; `K` = their number in the sealed catalog; each member runs `max(N_strict(s), ceil(n / K))` times | **Owner** (Q-O4); the Coordinator records `n` | `u`, `c` | `UNSET` |
| `PARAM-04` | `MAX_ATTEMPTS` `m` **per planned governing run** (repetition slot) | a procedural cap fixed without host evidence (section 2.2) | **Coordinator** | the justification of section 2.2 | `UNSET` |
| `PARAM-05` | machine-class label and equivalence policy | a closed list of class-defining attributes and a label rule (section 2.2) | the **Owner, or the CAD manager the Owner designates**, confirms the policy (Q-O5); the label **value** is computed from a read-only host observation | the Q-O5 answer and the observation (future non-governing host gate) | `UNSET` |
| `EVIDENCE_REPETITION` | governing runs per scenario that serves no class-A or class-B control group (evidence groups, V2.2 PA-1.3 kind `EVIDENCE`); the base value of the rule `EVIDENCE_STRICT`; the runs of an exploratory scenario | Coordinator engineering choice: these runs carry **no** verdict about the host (V2.2 PA-3); proposal: **one** run per scenario | **Coordinator** | none | `UNSET` (proposal 1) |
| `CHAR_RUNS` | runs per **non-governing** characterization scenario (no repetition slot; not `PARAM-01` runs) | derived (section 2.2): `N_c` of the class `c` of the control that consumes the pin or the selection the scenario feeds; for the `HOST_DEFAULT_MAP` scenarios `HDM-CHAR-*`, `N_A` runs at the pinned key values plus at least one informative run per varied key (outside the intent); `EVIDENCE_REPETITION` for an exploratory scenario; the intent is pending the Architect (AQ-V4-13 (b), section 6) | derived: no new choice | as `N_A` and `EVIDENCE_REPETITION` | `UNSET` |

### 2.2 Methods

**Terms.**

- A **class-A or class-B control group** is a group of kind `CONTROL` with class A or B in the group table of V2.2 PA-1.3. `KR` is not one (class A, outside the predicate: it never contributes to `TRUE`, `FALSE` or `AMENDMENT_PENDING`, PA-4), and neither is a group of kind `EVIDENCE` (PA-3).
- The groups a scenario **serves** are its `GROUPS_SERVED` (the AR2-08 derivation: the groups of the controls it exercises, plus the declared evidence groups). The groups that the contract assigns to a scenario as evidence (`CONTRACT_ASSIGNED_GROUPS`, AR3-06) never carry a `PASS` and do **not** enter `N_strict` (open question Q-A1 = AQ-V4-13 (a), section 6).
- **`N_strict(s)`** = the largest `N_c` over the classes `c` of the class-A and class-B control groups the scenario serves: `N_A` when it serves only class-A groups, `N_B` when it serves only class-B groups, and `max(N_A, N_B)` when it serves both (the **strictest pair**; BA10-02, AR2-43, AR3-24). No ordering of `N_A` and `N_B` is assumed.
- A **repetition slot** is one of the `R(s)` planned governing runs of a governing scenario. Scheduled repeats are repeats, not retries (V2.1 21.3), so each slot is separate.

**PARAM-01 (zero-failure detection; BA10-02, AR3-24).** A governing scenario that serves a class-A or class-B control group runs at least `N_strict(s)` governing times (each a fresh process, V2.1 section 20). If a failure mode of that scenario occurs in a run with probability at least `p`, the probability that none of `N` runs shows it is at most `(1 - p)^N`; requiring it to be at most `alpha` gives

```text
N >= ln(alpha) / ln(1 - p)        (N rounded up)
```

- `p` = the smallest per-run occurrence probability of a mode that the characterization must be able to catch; `alpha` = the miss risk the Owner accepts. One pair per class: `N_A` from (`p_A`, `alpha_A`), `N_B` from (`p_B`, `alpha_B`).
- **Floor (AR3-24).** `N_strict(s)` is a floor for every governing scenario that serves a class-A or class-B control group, **whatever its rule**: the rules `DEFAULT`, `CLEAN`, `VALIDATE` and `EVIDENCE_STRICT` below all carry it.
- A governing FAIL or UNKNOWN is never re-run to obtain a better result (V2.1 21.3); `R(s)` counts planned governing runs, not attempts.
- **Declared modelling assumption (BA10-04):** the runs are treated as independent trials with a constant per-run probability; correlated failures (for example a machine state that persists across runs) make the bound optimistic. The assumption is disclosed in Owner Act 2.
- `PARAM-01` counts **governing** runs only. The non-governing runs follow `WU_DRY_RUNS`, `CHAR_RUNS` and, for a learning micro-scenario, `PARAM-02`.

**Repetition rules (the vocabulary BA-04 applies).** The rule of a scenario follows from its properties, taking the **first** row that matches:

| Rule | Scenarios (by their properties) | Planned runs `R(s)` | Repetition slots |
|---|---|---|---|
| `DRY` | a dry-run scenario `WU-DRY-<s>` (non-governing by design; AR3-02) | `WU_DRY_RUNS` = `N_B` | none |
| `LEARN` | a learning micro-scenario of the event model (V2.1 26.3 `EV-L-<operation>`, 27.2 step 2) that serves no class-A or class-B control group, governing or not | `N_det + 1` | `R(s)` when governing; none otherwise |
| `CHAR` | any other non-governing scenario: non-governing by design or exploratory (V2.1 21.1; AR2-11, AR3-01, AR3-11) | `CHAR_RUNS(s)` | none |
| `CLEAN` | a member of the `PARAM-03` sample | `max(N_strict(s), ceil(n / K))` | `R(s)` |
| `SYNTH` | a governing scenario with no host run (offline evaluation of a synthetic input) that serves no class-A or class-B control group | `1` (the offline evaluation is deterministic; a repetition adds no detection power) | 1 |
| `VALIDATE` | a governing event-model validation scenario (V2.1 26.3 `EV-V-<plan>`, 27.2 step 6; group `E12-V`) | `max(N_strict(s), N_det)` | `R(s)` |
| `EVIDENCE` | a governing scenario that serves no class-A or class-B control group | `EVIDENCE_REPETITION` | `R(s)` |
| `EVIDENCE_STRICT` | a governing scenario of an evidence group that also serves a class-A or class-B control group (through the controls it exercises) | `max(N_strict(s), EVIDENCE_REPETITION)` | `R(s)` |
| `DEFAULT` | every other governing scenario (it serves a class-A or class-B control group) | `N_strict(s)` | `R(s)` |

**`WU_DRY_RUNS` (AR3-02, AR3-24; BA10-V3-04).** `WU_DRY_RUNS = N_B`. The dry runs measure the `WARMUP_COVERAGE_CRITERION` of the class-B group `WU` (V2.1 13.1; V2.2 PA-1.3) for each governing scenario that enters the governed window (one dry-run scenario each, AR3-02). Intent: an assembly load inside the governed window that occurs in a dry run with probability at least `p_B` is observed with probability at least `1 - alpha_B`. The Owner fixes it through Q-O2 and the Coordinator records it; it is **not** a Coordinator value. It counts valid dry runs: an `INVALID` dry run does not count (V2.1 21.3).

**`CHAR_RUNS` (non-governing characterization; BA10-V3-08).**

- A non-governing characterization scenario that feeds a **pin or a selection** (the `E04_ADMITTED_SET@R-1` pin from the `E4-LEARN-R1-*` runs under the chosen lock mode, which run under each `LOCK_MODE` candidate; the `LOCK_MODE` selection from `LK-*`; the `HOST_DEFAULT_MAP` pin from `HDM-CHAR-*`; AR2-11, AR3-01, AR3-11) runs `CHAR_RUNS(s) = N_c` times, where `c` is the class of the group whose control consumes the pin or the selection. The current consumers are E-04 (group `E4`), E-03 (group `LK`) and S-1..S-3 (groups `KS` and `SC-A`), all class A (V2.2 PA-1.3), so `CHAR_RUNS = N_A`. For the `HOST_DEFAULT_MAP` scenarios the next item states how the `N_A` runs combine with the varied settings.
- **`HOST_DEFAULT_MAP` scenarios (`HDM-CHAR-*`; AR3-11; correction pass).** The pin rule requires each of them to run with the key values as pinned in the fixture instances and, once or more, with each key variable changed; a governing comparison looks up only the key tuple of its own `PRE_COMMAND_STATE_RECORD`, which on the pinned fixture instances is the pinned key tuple (BA-08 V4 section 11.2, HDM-2, HDM-4 and HDM-5; an informative pointer, not a dependency). So `CHAR_RUNS(s)` = **`N_A` runs at the pinned key values plus at least one informative run per varied key (outside the intent)**: `CHAR_RUNS(s) = N_A + V(s)`, where `V(s)` is at least the number of key variables the pin rule varies. Only the `N_A` runs at the pinned key values carry the intent below. The varied-key runs show which attributes depend on which key; they are informative, carry no intent and no verdict, and this sheet fixes only their minimum (one run per varied key). This sheet holds no count of the keys: the key count is read from the sealed kind baseline (BA-08) when the rule is applied, and the cost memo uses the key list of BA-08 V4 HDM-2. The other reading, `N_A` runs for every key configuration, is part (b) of the open question AQ-V4-13 (section 6).
- **Intent:** a behaviour of the characterized property that occurs in a run with probability at least `p_A` (an admitted-set member, a lock property or conflict outcome, an effective attribute value at the pinned key values) is observed at least once in the runs that derive the pin or the selection, with probability at least `1 - alpha_A` (`(1 - p_A)^(N_A) <= alpha_A`). For `HDM-CHAR-*` the intent covers the `N_A` runs at the pinned key values only. The same independence assumption applies. Whether this derived intent is enough, or the characterization runs need a pair of their own, is part (b) of AQ-V4-13 (section 6); this draft keeps the derived intent.
- An **exploratory** scenario (V2.1 21.1: runs of an exploratory scenario are non-governing) runs `EVIDENCE_REPETITION` times: it answers a design question, feeds no pin and carries no verdict.
- These runs are **not** governing runs, carry no group verdict (AR2-11, AR3-01) and have **no repetition slot** (outside `PARAM-04`). `CHAR_RUNS` counts valid runs: an `INVALID` run does not count and is re-run only with the Coordinator's authorization per occurrence (V2.1 21.3).

**PARAM-02 (determinism; reference by use; BA10-04, AR3-24).** With `N_det >= ln(alpha_det) / ln(1 - p_det)` (rounded up), a plan whose event multiset differs from the reference in a run with probability at least `p_det` shows the difference with probability at least `1 - alpha_det`. The reference depends on the use:

- **Learning micro-scenario** (rule `LEARN`): the reference is the event multiset of the **first run of that micro-scenario**; `N_det` further runs must reproduce it, so `N_det + 1` runs in all.
- **Event-model validation** (rule `VALIDATE`): the reference is **`f(plan)` of the frozen EVM** (V2.1 27.1; V2.2 PA-2); every governing validation run is compared with it, with **no extra run**, so `N_det` runs, raised to `N_strict(s)` by the floor. A validation plan has no learning run (V2.1 27.1, 27.3, 27.8). When every run equals `f(plan)`, the multiset is identical in all governing validation runs (the determinism criterion of V2.1 27.4); the criterion is evaluated over all the governing validation runs of the plan.
- The independence assumption of `PARAM-01` applies.

**PARAM-03 (false-rejection bound; BA10-01, AR2-28, AR3-24).** With `k` false rejections observed in `n` clean governing runs, the reported quantity is the one-sided Clopper-Pearson upper bound at confidence `c`. For `k = 0` it is `1 - (1 - c)^(1/n)`; the sample that makes this bound at most `u` is

```text
n >= ln(1 - c) / ln(1 - u)        (n rounded up)
```

- **Sample membership (AR3-24).** The governing `CL-CLEAN-<fixture>` scenarios of the sealed catalog (`CL-CLEAN-FXANNBLANK-FP` included, AR3-15) **plus** `EA2-DEFERRED-EVENTS` (the baseline runs without any writer of V2.1 10.6). `CL-CLEANUP-CHECK` (the cleanup evidence scenario of group `CL`) is not a `CL-CLEAN-<fixture>` scenario and is not a member; `EA1-READONLY-OPENS` has no product run (AR3-09) and is not a member.
- **`K`** = the number of scenarios of that sample in the **sealed** catalog. The rule is symbolic: this sheet holds no value of `K`.
- **Combination rule:** each member runs `max(N_strict(s), ceil(n / K))` governing times. The sample then holds at least `K * ceil(n / K) >= n` runs, and every member keeps its `PARAM-01` floor.
- Only the clean governing runs of the members count: a clean governing run contains no deliberate violation (V2.1 1.3), and the members run without any writer (V2.1 10.6). The bound is reported on the clean governing runs that exist; if a member lacks a required governing run (`PARAM-04` rule 5), the shortfall is stated with the bound.
- **A-1 and A-2** are evaluated on this clean sample, as V2.1 10.6 states ("baseline runs without any writer ... evaluate A-1 and A-2").
- The contract sets **no threshold**: the observed rate and its bound are **reported** to the Owner (Act 2). Choosing `u` and `c` fixes only the sample size.

**PARAM-04 (`MAX_ATTEMPTS`; BA10-03, BA10-05, AR2-43, AR3-24).** The **unit is the repetition slot** (one planned governing run). The Coordinator fixes a **procedural cap** `m` **without host evidence**, with this justification:

1. `m >= 1` is the maximum number of attempts of **one slot**; `m = 1` means that no retry is possible.
2. A slot is filled by its first governing attempt, whatever its result. After an `INVALID` attempt, another attempt of the same slot needs the Coordinator's authorization per occurrence (V2.1 21.3); `m` does not replace that review, it bounds the review effort and the duration of the characterization. A crash first needs its ruling (V2.1 21.1); an attempt ruled `INVALID` counts like any other.
3. **Every attempt of the slot counts** toward `m`, including attempts made `INVALID` by contamination (AR2-43; V2.1 21.5).
4. `MAX_ATTEMPTS` never permits the retry of a governing FAIL or UNKNOWN (V2.1 21.3).
5. A slot that reaches `m` attempts without a governing attempt leaves the scenario **without a required governing run**: the groups the scenario serves **cannot be `PASS`** (V2.1 1.3: `PASS` needs every required governing run; V2.2 PA-3 item 1 for evidence groups). Without another non-`PASS` verdict, the group state is `NOT_EVALUATED` (V2.1 1.4 row `INVALID` and 1.6; V2.2 PA-3: "an incomplete group yields `NOT_EVALUATED`"). A governing FAIL or UNKNOWN observed in any run still counts for the group of that control (AR2-08; V2.1 21.3: every governing attempt counts).
6. Non-governing runs (rules `DRY` and `CHAR`) have **no slot** and are outside `m`; an `INVALID` non-governing run is re-run only with the per-occurrence authorization of V2.1 21.3, and every attempt stays in the attempt log.
7. A later revision of `m` informed by the `INVALID` rates of non-governing dry runs is a **new baseline version**. The V2 method (a bound `beta` on consecutive invalid attempts at the observed rate) stays withdrawn: it needed host evidence before the baseline.

**PARAM-05 (machine class; BA10-06, AR2-43, AR3-24).** Proposed **conservative** policy, for confirmation by the **Owner, or by the CAD manager if the Owner designates one** (a restriction, never an extension of the guarantee):

| Aspect | Proposal |
|---|---|
| equivalence rule | **no cross-machine equivalence**: one machine class = one physical machine installation |
| class-defining attributes (closed list) | (1) the Windows `MachineGuid`; (2) the OS version and build; (3) the AutoCAD product identity; (4) the AutoCAD profile identity; (5) the value of `SECURELOAD`; (6) the value of `TRUSTEDPATHS` |
| not class-defining | the **machine identity class** field itself (V2.1 2.1: it is the output, the label); the machine's own **module entries** (drivers, security software) of the manifest machine layer, which stay a `MACHINE_PROFILE_BOUND` tuple field of their own, compared through the manifest hash (V2.1 2.1, 2.2, 29; the manifest is captured at EXEC-11, after the baseline); every `BUILD_BOUND` field (AutoCAD version and build, `acad.exe` SHA-256, API assemblies), which stays in the build tuple |
| label | `MC-` + the first 12 lowercase hexadecimal digits of the SHA-256 of the canonical serialization below; the clear `MachineGuid` stays in local evidence (V2.1 2.1) |
| canonical serialization (defined here) | the RFC 8785 (JCS) serialization, in UTF-8, of a JSON object with exactly the six keys `AutoCadProduct`, `AutoCadProfile`, `MachineGuid`, `OsVersionBuild`, `SECURELOAD`, `TRUSTEDPATHS`; every value is a JSON string holding the exact text that the observation read (`SECURELOAD` as its decimal digits); JCS sorts the keys and adds no whitespace |
| change of a class-defining attribute | a new machine class (a new tuple) |
| change of a module entry | no change of the label; a mismatch of its own `MACHINE_PROFILE_BOUND` field (V2.1 2.2) |
| label value | computed from a **read-only host read** of the six attributes on the characterization machine (for example registry and OS queries and the AutoCAD profile and system-variable values). It needs **no implemented instrument and no manifest**, and changes nothing. It belongs to the future non-governing host gate whose plan BA-11 V4 records under `BASE-18` (AR3-25); that gate is **not authorized** now. The observation record keeps the six strings, and the label is recomputed from it |

**EVIDENCE_REPETITION (Coordinator).** It applies to the rule `EVIDENCE`, is the base value of `EVIDENCE_STRICT` (where the floor `N_strict(s)` governs when larger) and is the count of an exploratory scenario. A scenario that serves a class-A or class-B group only through `CONTRACT_ASSIGNED_GROUPS` (for example `PR-CLOSURE-COMPLETENESS`, group `Q` with `PR-REC` assigned by V2.2 PA-12, AR3-06) carries evidence and never a `PASS`, so it stays under `EVIDENCE` (open question Q-A1 = AQ-V4-13 (a), section 6).

### 2.3 Total runs implied by the choices (symbolic)

`c(X)` is the number of scenarios of rule `X` in the **sealed** catalog, and `c_A(X)`, `c_AB(X)`, `c_B(X)` split it by the classes of the control groups served (class-A only, both classes, class-B only). This sheet holds **no** value of `c(X)`, `K`, `V(s)` or `TOTAL`.

```text
TOTAL = G + D

G (planned governing runs = repetition slots)
  =   c_A(DEFAULT) * N_A + c_AB(DEFAULT) * max(N_A, N_B) + c_B(DEFAULT) * N_B
    + sum over CLEAN            of max(N_strict(s), ceil(n / K))
    + sum over VALIDATE         of max(N_strict(s), N_det)
    + sum over EVIDENCE_STRICT  of max(N_strict(s), EVIDENCE_REPETITION)
    + c(LEARN, governing) * (N_det + 1) + c(EVIDENCE) * EVIDENCE_REPETITION + c(SYNTH) * 1

D (non-governing runs; no slot)
  =   c(DRY) * N_B + c(LEARN, non-governing) * (N_det + 1)
    + sum over CHAR of CHAR_RUNS(s), that is
        c(CHAR, pin or selection, other than HOST_DEFAULT_MAP) * N_A
      + sum over CHAR of HOST_DEFAULT_MAP of (N_A + V(s))          (V(s) >= the number of varied keys)
      + c(CHAR, exploratory) * EVIDENCE_REPETITION

worst-case governing attempts = m * G        (PARAM-04, per slot)
```

The `SYNTH` runs are offline evaluations; every other run is a fresh AutoCAD process (V2.1 section 20). The cost memo (header) evaluates this expression on the current draft catalog for illustrative choices and gives the sensitivities; its figures are not part of this sheet.

The governance of the learning micro-scenarios `EV-L-*` and of the qualification vehicles `Q-I14-*` is pending the Architect (AQ-V4-01; BA-04 V4). The rules above do not change with it: the rule `LEARN` applies whether a learning micro-scenario is governing or not (only its place in `G` or `D` depends on it), and a dry run exists only for a governing scenario that enters the governed window (AR3-02). Every count `c(X)` is read from the sealed catalog, after those rulings.

## 3. Operational-only parameters (do not affect a semantic verdict)

| Id | Parameter | Classification | Rule |
|---|---|---|---|
| `OP-01` | control-plane time budgets or watchdogs | `SHOULD_BE_REMOVED` from the semantic verdict | a slow measurement never becomes `UNKNOWN`; an expired watchdog makes the **run or instrument `INVALID`**, is logged and counts in the attempt log |
| `OP-02` | duration of the post-CP observation queue | `MAY_BE_OPERATIONAL_AT_EXECUTION` | informational only; recorded |

## 4. Values that are **not** parameters of this sheet (each with its own gate; BA10-07)

Only the first row is part of `BASE-18`. The other rows have their own gates and are **not** `BASE-18` items.

| Item | Where it lives | Gate |
|---|---|---|
| tolerances of `SEMANTIC_COMPARISON`, including the `TOL_SCALE` decision, test and record | BA-08 V4 section 10 | **part of `BASE-18`** (V2.2 PA-9); K-3; a prerequisite of BA-08, not of this sheet |
| evidence-derived pins: `E04_ADMITTED_SET@R-1`, `LOCK_MODE`, `EVM_FROZEN`, `HOST_DEFAULT_MAP`, `FIXTURE_INSTANCE@<FX>` | pin records (AR2-16), with their own seal and custody in BA-07 V4 section 5.1 (`RECORDED -> RATIFIED -> ANCHORED`, AR3-23); the slots are named by the scenarios of the catalog | **not `BASE-18`**: each pin value lives in its own pin record, never in these bytes, and is fixed **before the first governing run that uses it** (AR2-11, AR2-16, AR2-17, AR3-26) |
| E-04 designated route | BA-08 V4 section 8 | **not `BASE-18`**: `BASE-3`, with the Coordinator's ratification (K-5) and the conditions of AR3-14 |

## 5. Owner decision questions (prepared; **NOT asked**)

These questions are **not put to the Owner until this V4 sheet and the cost memo are reviewed** (AR3-24). Q-O1..Q-O4 are answered by the **Owner** only; Q-O5 is answered by the **Owner, or by the CAD manager if the Owner designates one** (AR2-43, AR3-24). Nobody else answers them. Until they are answered, `BASE-18` is not met and `CT21D_AUTHORITY_BASELINE_READY` cannot become `TRUE`.

| Question | Who answers | What it decides | Effect |
|---|---|---|---|
| **Q-O1** | Owner | `p_A` and `alpha_A` for the scenarios of **class-A** groups (the guarantee-bearing controls) | fixes `N_A` of `PARAM-01`, and `CHAR_RUNS` of the pin- and selection-feeding characterization runs (their current consumers are class A; for `HDM-CHAR-*`, the runs at the pinned key values) |
| **Q-O2** | Owner | `p_B` and `alpha_B` for the scenarios of **class-B** groups (risk-reduction controls), or "same as class A" | fixes `N_B` of `PARAM-01` and `WU_DRY_RUNS = N_B` |
| **Q-O3** | Owner | `p_det` and `alpha_det` for the determinism of the expected event set | fixes `N_det` of `PARAM-02` (`N_det + 1` learning runs; `max(N_strict, N_det)` validation runs) |
| **Q-O4** | Owner | `u` and `c` for the false-rejection bound to be reported | fixes `n` of `PARAM-03` |
| **Q-O5** | Owner, or the CAD manager the Owner designates | confirmation (or change) of the machine-class policy of section 2.2, and the machine on which the characterization will run | fixes the `PARAM-05` policy; the label value needs the attribute observation |

**Consequences to weigh** (the figures are in the cost memo):

- `TOTAL` scales roughly with `1 / p` (for small `p`, `N` is close to `ln(1 / alpha) / p`) and only **logarithmically** with `1 / alpha`.
- `u` and `c` enter only through `ceil(n / K)`. They change `TOTAL` only when `ceil(n / K)` exceeds `N_strict` of a sample member, and then only the `CLEAN` term grows.
- `N_B` drives the dry-run term `c(DRY) * N_B` (one dry-run scenario per governing scenario that enters the governed window) and the class-B-only terms; when `N_B > N_A`, it also drives every mixed-class term.
- `EVIDENCE_REPETITION` drives the `EVIDENCE` term and the exploratory runs, and `EVIDENCE_STRICT` only while it exceeds `N_strict`.
- One more `PARAM-03` sample member adds its own runs and its dry runs, and lowers `ceil(n / K)` for the others.
- `m` bounds the governing attempts at `m * G` in the worst case.
- The informative runs of `HDM-CHAR-*` (at least one per varied key, outside the intent) do not depend on any Owner answer.

```text
PARAM-01 .. PARAM-05 = UNSET     WU_DRY_RUNS = N_B (UNSET)     CHAR_RUNS = derived (UNSET)     EVIDENCE_REPETITION = UNSET (proposal 1)
OP-01, OP-02 = OPERATIONAL
BASE-18 = NOT MET     OWNER DECISIONS Q-O1 .. Q-O5 = NOT ASKED (not before the review of this V4, AR3-24)
CT21D_AUTHORITY_BASELINE_READY = FALSE
```

## 6. Open question for the Architect (AQ-V4-13)

The round V4 id of this question is **AQ-V4-13**. It has two parts: (a) is the V4 question Q-A1 (the id BA-04 V4 cites), and (b) is the intent of the `CHAR` repetitions. Neither is decided here; this draft applies the reading in the third column until the Architect rules.

| Id | Question | Reading used in this draft | Effect of the other reading |
|---|---|---|---|
| AQ-V4-13 (a) = Q-A1 | AR3-06 lets a scenario "serve" a group that the contract assigns to a `NOT_A_CONTROL` item or a qualification control, as evidence and never `PASS` (`CONTRACT_ASSIGNED_GROUPS`); AR3-24 applies the strictest pair to "every governing scenario that serves groups A or B". Do contract-assigned groups enter `N_strict`? | **No**: "serves" is `GROUPS_SERVED` (AR2-08); a contract-assigned group never carries a `PASS`, so it needs no detection sizing | `PR-CLOSURE-COMPLETENESS` would run `max(N_A, EVIDENCE_REPETITION)` times, and the X7 and residual cells that serve only class-A groups and carry `SC-B` by contract would run `max(N_A, N_B)` times (a change only when `N_B > N_A`); the cost memo, block E.1, gives the size |
| AQ-V4-13 (b) | The intent of `CHAR_RUNS` (non-governing characterization; BA10-V3-08). (i) Are the repetitions of these runs a "repetitions per group" parameter of V2.1 21.7 that needs its own Owner pair, or is the derived `N_c` of the class that consumes the pin or the selection enough? (ii) For `HDM-CHAR-*`, does `N_A` apply to the pinned key configuration only, or to every key configuration of the pin rule (BA-08 V4 section 11.2, HDM-4)? | (i) the derived `N_c` (today `N_A`); no new Owner question. (ii) `N_A` runs at the pinned key values plus at least one informative run per varied key (outside the intent), because a governing comparison looks up only its own key tuple, which is the pinned one (HDM-5) | (i) a pair of its own, chosen by the Owner like the other risk-appetite pairs (section 1); its size cannot be given without a value. (ii) `(1 + V(s)) * N_A` runs per `HDM-CHAR-*` scenario instead of `N_A + V(s)`; the cost memo, block E.2, gives the size |

## 7. Delta V4 (decisions section 214)

Sections 1 to 5 keep their V3 numbers; section 6 (open question AQ-V4-13) and section 7 (this table) are new. Section 2.3 no longer holds counts or illustrative totals: they moved to the cost memo (AR3-24). The rows marked "correction pass" record the targeted correction of this V4 draft after the critic review.

| Finding id | Decision | Fix (section/field) |
|---|---|---|
| BA10-V3-01 | AR3-24 | FIXED: sample = the governing `CL-CLEAN-<fixture>` scenarios plus `EA2-DEFERRED-EVENTS`; `CL-CLEANUP-CHECK` and `EA1-READONLY-OPENS` excluded; `K` symbolic (the sealed catalog's number); the combination rule guarantees `n` (2.1 `PARAM-03` row; 2.2 `PARAM-03`; section 1 "Symbolic sheet"; header `DEPENDS_ON`) |
| BA10-V3-02 | AR3-24 | FIXED: `m` per repetition slot; every attempt of the slot counts, contamination included; an exhausted slot leaves the scenario without a required run, its groups cannot be `PASS` (`NOT_EVALUATED`); non-governing runs have no slot (2.1 `PARAM-04` row; 2.2 `PARAM-04` rules 1-7) |
| BA10-V3-03 | AR3-24 | FIXED: closed list of six attributes (`MachineGuid`, OS version and build, AutoCAD product and profile identity, `SECURELOAD`, `TRUSTEDPATHS`); the class field and the manifest module entries excluded; the observation is a read-only host read with no instrument and no manifest; the policy stays a proposal (2.2 `PARAM-05` table; 2.1 `PARAM-05` row) |
| BA10-V3-04 | AR3-24 | FIXED: `WU_DRY_RUNS = N_B` (Owner, Q-O2; recorded by the Coordinator; not a Coordinator value) (2.1 `WU_DRY_RUNS` row; 2.2 `WU_DRY_RUNS`; section 5 Q-O2). BA-09 V4 (section 4 "Count", W-2) and BA-04 V4 (rule `DRY`, C-9) use the same wording |
| BA10-V3-05 | AR3-24 | FIXED: the expression splits class-A-only, mixed and class-B-only terms and assumes no ordering of `N_A` and `N_B` (2.2 terms; 2.3); the numbers left this sheet: the corrected V3 totals and the V4 illustrative totals are in the cost memo (sections 1 and 4, blocks B and C); decisions section 214.1 already records the corrected PH2D-1 figures |
| BA10-V3-06 | AR3-24 | FIXED: actual sensitivities (`1 / p`, logarithmic in `1 / alpha`, `u` and `c` only through `ceil(n / K)` above `N_strict`, `N_B`, `EVIDENCE_REPETITION`, `K`, `m`) (section 5 "Consequences to weigh"); the figures in the cost memo (section 3, block D) |
| BA10-V3-07 | AR3-24 | FIXED: reference by use: learning = the first run of the micro-scenario (`N_det + 1`); validation = `f(plan)` of the frozen EVM, no extra run (`N_det`, raised by the floor) (2.1 `PARAM-02` row; 2.2 `PARAM-02`; rules `LEARN` and `VALIDATE`; 2.3) |
| BA10-V3-08 | AR3-24, AR3-06 | FIXED: the floor `N_strict(s)` applies to every governing scenario of groups A or B (rules `DEFAULT`, `CLEAN`, `VALIDATE`, `EVIDENCE_STRICT`); `PR-CLOSURE-COMPLETENESS` is a group-`Q` scenario with `PR-REC` contract-assigned (AR3-06), so it stays under `EVIDENCE` (open question Q-A1 = AQ-V4-13 (a)); the non-governing characterization runs have their own derived count `CHAR_RUNS` with a stated intent (its intent is AQ-V4-13 (b)) and are no longer under a parameter defined as governing runs (2.1 `PARAM-01` and `CHAR_RUNS` rows; 2.2 terms, table of rules, `CHAR_RUNS`; section 6) |
| BA10-V3-09 | AR3-30 | FIXED: only the tolerance row is part of `BASE-18`; the pins are fixed before the first governing run that uses them; the route is under `BASE-3` with the Coordinator's ratification (K-5) (section 4 heading and gate column) |
| BA10-V3-10 | AR3-24 | FIXED: Q-O5 is answered by the Owner, or by the CAD manager the Owner designates; Q-O1..Q-O4 by the Owner only (section 1 second bullet; 2.1 `PARAM-05` row; 2.2 `PARAM-05`; section 5 intro and "Who answers" column) |
| CLOSURE BA10-01 | AR3-24 | FIXED: the residual is BA10-V3-01 (above) |
| CLOSURE BA10-02 | AR3-24, AR3-06 | FIXED: residual (1) strictest pair and non-governing `CHAR` (BA10-V3-08); residual (2) totals (BA10-V3-05, cost memo); residual (3) owner of `WU_DRY_RUNS` (BA10-V3-04) |
| CLOSURE BA10-03 | AR3-24 | FIXED: the residual is BA10-V3-03 (above); the label no longer depends on the EXEC-11 manifest |
| CLOSURE BA10-04 | AR3-24 | FIXED: the residual is BA10-V3-07 (above) |
| CLOSURE BA10-06 | section 214.1 | NOT_APPLICABLE: refuted by the Architect (its remains belong to BA10-V3-03, FIXED) |
| D-19 | AR3-24 | FIXED on this side: `WU_DRY_RUNS = N_B` (Owner, Q-O2; recorded by the Coordinator) (2.1; 2.2). The BA-09 V4 wording (section 4 "Count", W-2) already reads the same |
| BA11V3-12 | AR3-24 | FIXED: this sheet holds no total (2.3 symbolic); the cost memo recomputes the V3 figures from the V3 catalog and shows the error of the V3 table (learning term charged without its reference run) (memo section 1, block C); decisions section 214.1 already records the corrected PH2D-1 figures |
| BA11V3-13 | AR3-24 | FIXED: one authority, `WU_DRY_RUNS = N_B` (Owner, Q-O2; recorded by the Coordinator) (2.1; 2.2; section 5); BA-09 V4 and BA-04 V4 aligned |
| AR3-25 (header) | AR3-25 | `DEPENDS_ON = BA-01` only, separate from `PREREQUISITES` with their gate; no edge to BA-04; the machine-attribute observation belongs to the future non-governing host gate planned under `BASE-18` (header) |
| AR3-11 / BA10-V3-08 (critic: `CHAR_RUNS` of `HDM-CHAR-*` cannot hold both the `N_A` runs at the pinned configuration and the varied-key runs of BA-08 V4 HDM-4) (correction pass) | AR3-11, AR3-24 | FIXED: for `HDM-CHAR-*`, `CHAR_RUNS(s)` = `N_A` runs at the pinned key values plus at least one informative run per varied key (outside the intent), `N_A + V(s)`; the intent covers the pinned-key runs only; the sheet holds no key count (section 1 "Symbolic sheet"; 2.1 `CHAR_RUNS` row; 2.2 `CHAR_RUNS`; 2.3 term `D`; section 5 Q-O1 and "Consequences to weigh"; header). The cost memo reads the key list of BA-08 V4 HDM-2 and counts one informative run per key, the minimum (memo blocks A.6, B, B.2, D.7) |
| AR3-24 / BA10-V3-05 (critic: BA-04 V4 read `N_strict` as `N_A` for mixed-class scenarios) (correction pass) | AR3-24 | NO CHANGE NEEDED on this sheet: section 2.2 already defines `N_strict` as `max(N_A, N_B)` for a scenario that serves both classes, with no ordering assumed. BA-04 V4 now applies it (every mixed-class label reads `max(N_A, N_B)`); the cost memo compares every catalog label with this derivation exactly (memo block A.5) |
| AR3-02 / D-05 (critic: `Q-I14-*` without dry runs) (correction pass) | AR3-02, AR3-24 | NO CHANGE NEEDED on this sheet: the rule `DRY` covers every governing scenario that enters the governed window and has no exclusion by mode. BA-04 V4 added the four `WU-DRY-Q-I14-*` (DRAFT, AQ-V4-01); the cost memo counts them (memo blocks A.2, C.2 and F) |
| Architect open questions (round V4 ids) (correction pass) | - | APPLIED: section 6 is AQ-V4-13, part (a) = Q-A1 (id kept, BA-04 V4 cites it) and part (b) = the intent of `CHAR_RUNS` (the lower-priority question of the first V4 report, now written into the sheet); AQ-V4-01 referenced in 2.3 (the governance of `EV-L-*` and `Q-I14-*`); header line `ARCHITECT QUESTIONS`; header `PREREQUISITES` gains the Architect's answer to AQ-V4-13 or its recorded deferral (before the candidate, as the BA-11 V4 row of BA-10 states); none decided here |
