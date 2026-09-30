# I-52 — CT-21D Baseline Artifact BA-10: `MUST_FIX` Parameter Sheet and Selection Methods (DRAFT V3)

> **BASELINE ARTIFACT BA-10 V3 — DRAFT, NOT SEALED. Design / documentation only. No value is chosen: every value is `UNSET`.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-PARAMETER-SHEET
> ARTIFACT_VERSION          = 3-DRAFT   (supersedes 2-DRAFT, blob 539d7c4273acc14d9bcc660ef073171683730969, which stays as history)
> STATUS AUTHORITY          = the BA-11 registry entry of this artifact is the only authority for its status and hashes;
>                             this header is frozen, non-authoritative text of the candidate bytes (BA-11 V3 section 2)
> AUTHORITY_CONTRACT        = CT-21D V2.1 section 21.7 and BASE-18 + V2.2 (PA-9) + V2.3 + V2.4
> PHASE-2 RULING APPLIED    = BA10-01..BA10-07; AR2-28 (PARAM-03 bound to the clean runs), AR2-43 (sequence: contract read as written; PARAM-04 method changed)
> BASE ITEM                 = BASE-18: NOT MET. Needs: the Owner decisions Q-O1..Q-O5; the Coordinator values PARAM-04 and EVIDENCE_REPETITION;
>                             a read-only machine-attribute observation for the PARAM-05 label (future host gate, not authorized); and the BA-08 V3
>                             tolerances with the TOL_SCALE decision, test and record (V2.2 PA-9; K-3)
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 1. Rules of this sheet

- Every value is **`UNSET`** until it is chosen **with its stated intent**, by the **authority** named in its row, and recorded with its justification. A number without an intent is **prohibited**.
- **Owner-reserved choices** (risk appetite: `p`, `alpha`, `u`, `c`, `p_det`, `alpha_det`; the machine-class policy) are **never** chosen by the Coordinator, the Architect or the implementer. They are asked in the decision memo of section 5.
- **Sequence (AR2-43).** The contract is read as written: **every `MUST_FIX_BEFORE_AUTHORITY_BASELINE` value is fixed before `BASELINE_READY`** (V2.1 21.7, 22 `BASE-18`); no pin mechanism applies to these values. A value is never derived from the outputs of governing runs; non-governing evidence may later inform a **new baseline version**, never a silent change.
- **Operational parameters** (section 3) never change a semantic verdict.

## 2. `MUST_FIX_BEFORE_AUTHORITY_BASELINE` parameters

### 2.1 Summary

| Id | Parameter | Method (section 2.2) | Who chooses | Inputs needed | Current value |
|---|---|---|---|---|---|
| `PARAM-01` | governing runs **per scenario** of the class-A and class-B groups | zero-failure detection `N >= ln(alpha) / ln(1 - p)`; the pair of the **strictest class** among the groups the scenario serves | **Owner** (`p`, `alpha` per class) ; the Coordinator records `N_A`, `N_B` | `p_A`, `alpha_A`, `p_B`, `alpha_B` | `UNSET` |
| `PARAM-02` | determinism-validation repetitions (V2.1 27.4), and the repetitions of the learning micro-scenarios | the same formula for "the multiset of a plan differs from the reference run"; `N_det` runs **after one reference run** | **Owner** (`p_det`, `alpha_det`); the Coordinator records `N_det` | `p_det`, `alpha_det` | `UNSET` |
| `PARAM-03` | clean-run sample for the false-rejection bound (V2.1 10.6) | `n >= ln(1 - c) / ln(1 - u)`; **drawn from the governing runs of the `CL-CLEAN-*` scenarios** (V2.1 26.3) | **Owner** (`u`, `c`); the Coordinator records `n` | `u`, `c` | `UNSET` |
| `PARAM-04` | `MAX_ATTEMPTS` per scenario | a **procedural cap** fixed by the Coordinator, independent of host evidence (section 2.2) | **Coordinator** | the justification of section 2.2 | `UNSET` |
| `PARAM-05` | machine-class label and equivalence policy | identity policy (section 2.2) | **Owner or CAD manager** confirms the policy; the label **value** is computed from a read-only observation of the class-defining attributes | the confirmation (Q-O5) and the observation (future host gate) | `UNSET` |
| `EVIDENCE_REPETITION` | governing runs per scenario of the evidence groups `Q`, `U-*`, `S1`, `PP` and `CL` (the learning group `E12-L` follows `PARAM-02`; the host-less `OU` scenarios run once) | Coordinator engineering choice: these groups carry **no** verdict about the host (V2.2 PA-3); proposal: **one** governing run per scenario | **Coordinator** | none | `UNSET` (proposal 1) |

### 2.2 Methods

**PARAM-01 (zero-failure detection; BA10-02).** The **unit is the scenario**: a scenario of the catalog that serves a class-A or class-B group runs `N` governing times (each a fresh process, V2.1 section 20). If a failure mode of that scenario occurs in a run with probability at least `p`, the probability that none of `N` runs shows it is at most `(1 - p)^N`; requiring it to be at most `alpha` gives

```text
N >= ln(alpha) / ln(1 - p)        (N rounded up)
```

- `p` = the smallest per-run occurrence probability of a mode that the characterization must be able to catch; `alpha` = the miss risk the Owner accepts.
- **Strictest pair.** A scenario serving groups of different classes uses the larger of `N_A` and `N_B`.
- **Scope of `PARAM-01`:** the scenarios with repetition rule `DEFAULT` and `CHAR` (BA-04 V3), and the dry runs of the warm-up (`WU_DRY_RUNS` = `N_B`, the class of `WU`).
- **Not governed by `PARAM-01`:** the learning micro-scenarios and the validation of the event model (`PARAM-02`); the clean runs (the rule of `PARAM-03` below); the evidence groups (`EVIDENCE_REPETITION`); the synthetic evaluator scenarios (**one** run: the offline evaluation is deterministic, so a repetition adds no detection power).
- A governing FAIL or UNKNOWN is never re-run to obtain a better result (V2.1 21.3); `N` counts planned governing runs, not attempts.
- **Declared modelling assumption (BA10-04):** the runs are treated as independent trials with a constant per-run probability; correlated failures (for example a machine state that persists across runs) make the bound optimistic. The assumption is disclosed in Owner Act 2.

**PARAM-02 (determinism; BA10-04).** The **reference** is the event multiset of the **first** learning run of a plan; `N_det` further runs must reproduce it (so `N_det + 1` runs in all), which removes the off-by-one of a between-runs failure mode. The same formula and the same independence assumption apply.

**PARAM-03 (false-rejection bound; BA10-01, AR2-28).** With `k` false rejections observed in `n` clean governing runs, the reported quantity is the one-sided Clopper-Pearson upper bound at confidence `c`. For `k = 0` it is `1 - (1 - c)^(1/n)`; the sample that makes this bound at most `u` is

```text
n >= ln(1 - c) / ln(1 - u)        (n rounded up)
```

- The sample is drawn from the **`CL-CLEAN-*`** scenarios (9 scenarios in BA-04 V3). **Combination rule:** each `CL-CLEAN` scenario runs `max(N_strict, ceil(n / 9))` governing times, so that the clean runs satisfy both `PARAM-01` and `PARAM-03`.
- **A-1 and A-2** are evaluated on this clean sample, as V2.1 10.6 states ("baseline runs without any writer ... evaluate A-1 and A-2"); `EA1` and `EA2` add their dedicated scenarios.
- The contract sets **no threshold**: the observed rate and its bound are **reported** to the Owner (Act 2). Choosing `u` and `c` fixes only the sample size.

**PARAM-04 (`MAX_ATTEMPTS`; BA10-03, BA10-05, AR2-43).** The Coordinator fixes, per scenario, a **procedural cap** `m >= 1` on the attempts of a scenario, **without host evidence**. Justification required by the method: (1) every retry of an `INVALID` attempt already needs a Coordinator authorization per occurrence (V2.1 21.3), so `m` does not replace that review; it bounds the review effort and the duration of the characterization; (2) **every attempt counts** toward `m`, including attempts made `INVALID` by contamination; (3) `MAX_ATTEMPTS` never permits the retry of a governing FAIL or UNKNOWN; (4) when a scenario reaches `m` without a governing run, its group verdict is `INVALID` (V2.1 1.3: there is no governing run). A later revision of `m` informed by the `INVALID` rates of non-governing dry runs is a **new baseline version**. The V2 method (a bound `beta` on consecutive invalid attempts at the observed rate) is withdrawn: it needed host evidence before the baseline.

**PARAM-05 (machine class; BA10-06).** Proposed **conservative** policy, for Owner or CAD-manager confirmation (a restriction, never an extension of the guarantee):

| Aspect | Proposal |
|---|---|
| equivalence rule | **no cross-machine equivalence**: one machine class = one physical machine installation |
| class-defining attributes | Windows `MachineGuid` and the `MACHINE_PROFILE_BOUND` fields of the tuple (V2.1 2.1: OS version and build, AutoCAD profile name, `SECURELOAD`, `TRUSTEDPATHS`, and the other machine-profile fields); the `BUILD_BOUND` fields (for example the SHA-256 of `acad.exe`) stay in the build tuple and are **not** part of the label |
| label | `MC-` + the first 12 hexadecimal digits of the SHA-256 of the canonical serialization (BA-07 V3 section 4) of all the class-defining attributes; the clear `MachineGuid` stays in local evidence |
| change of any class-defining attribute | a new machine class (a new tuple) |
| label value | computed from a **read-only observation** of the attributes on the characterization machine; that observation needs a host gate that is **not authorized** now |

### 2.3 Total runs implied by the choices (for the Owner; arithmetic only)

With the counts of BA-04 V3 (repetition rules: `CLEAN` 9 scenarios, `DEFAULT` class A 131, `DEFAULT` class B 3, `CHAR` 14, `LEARN` 19, `EVIDENCE` 37, `SYNTH` 12, `DRY` 145):

```text
TOTAL = 9 * max(N_strict, ceil(n / 9)) + 131 * N_strict + 3 * N_B + 14 * N_strict
      + 19 * (N_det + 1) + 37 * EVIDENCE_REPETITION + 12 * 1 + 145 * N_B          (N_strict = max(N_A, N_B))
```

Illustrative evaluations (**arithmetic only; NOT a proposal and NOT a choice**; one pair for both classes, `p_det = p`, `alpha_det = alpha`, `EVIDENCE_REPETITION = 1`):

| `p` | `alpha` | `u` | `c` | `N` | `n` | TOTAL governing and dry runs |
|---|---|---|---|---|---|---|
| 0.10 | 0.05 | 0.05 | 0.95 | 29 | 59 | 9358 |
| 0.20 | 0.05 | 0.10 | 0.95 | 14 | 29 | 4543 |
| 0.30 | 0.10 | 0.10 | 0.90 | 7 | 22 | 2296 |
| 0.50 | 0.10 | 0.20 | 0.90 | 4 | 11 | 1333 |

The totals are dominated by the scan matrix and the per-scenario dry runs; each run is a fresh AutoCAD process.

## 3. Operational-only parameters (do not affect a semantic verdict)

| Id | Parameter | Classification | Rule |
|---|---|---|---|
| `OP-01` | control-plane time budgets or watchdogs | `SHOULD_BE_REMOVED` from the semantic verdict | a slow measurement never becomes `UNKNOWN`; an expired watchdog makes the **run or instrument `INVALID`**, is logged and counts in the attempt log |
| `OP-02` | duration of the post-CP observation queue | `MAY_BE_OPERATIONAL_AT_EXECUTION` | informational only; recorded |

## 4. Values that are **not** parameters of this sheet (but are part of `BASE-18`, BA10-07)

| Item | Where it lives |
|---|---|
| tolerances of `SEMANTIC_COMPARISON`, including the `TOL_SCALE` decision, test and record | BA-08 V3 section 10 (V2.2 PA-9 makes them part of `BASE-18`) |
| E-04 admitted set, `LOCK_MODE` selection | evidence-derived pins (BA-04 V3 section 2.1; BA-08 V3) |
| E-04 designated route | BA-08 V3 section 8 (Coordinator ratification) |

## 5. Owner decision memo (prepared; the Owner has **not** answered)

The following questions are **reserved to the Owner**. Nobody answers them on the Owner's behalf. Until they are answered, `BASE-18` is not met and `CT21D_AUTHORITY_BASELINE_READY` cannot become `TRUE`.

| Question | What it decides | Effect |
|---|---|---|
| **Q-O1** | `p_A` and `alpha_A` for scenarios of **class-A** groups (the guarantee-bearing controls) | fixes `N_A` of `PARAM-01` |
| **Q-O2** | `p_B` and `alpha_B` for scenarios of **class-B** groups (risk-reduction controls), or "same as class A" | fixes `N_B` of `PARAM-01` and the warm-up dry runs |
| **Q-O3** | `p_det` and `alpha_det` for the determinism of the expected event set | fixes `PARAM-02` |
| **Q-O4** | `u` and `c` for the false-rejection bound to be reported | fixes `PARAM-03` |
| **Q-O5** | confirmation (or change) of the conservative machine-class policy of section 2.2, and of the machine on which the characterization will run | fixes the `PARAM-05` policy; the label needs the attribute observation |

A consequence the Owner should weigh: a smaller `p`, `u`, `alpha` or `1 - c` increases the number of AutoCAD runs roughly in proportion (section 2.3), and therefore the duration and cost of the characterization.

```text
PARAM-01 .. PARAM-05 = UNSET     EVIDENCE_REPETITION = UNSET (proposal 1)     OP-01, OP-02 = OPERATIONAL
BASE-18 = NOT MET     OWNER DECISIONS Q-O1 .. Q-O5 = NOT ASKED YET (memo prepared)
CT21D_AUTHORITY_BASELINE_READY = FALSE
```
