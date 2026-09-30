# I-52 — CT-21D Baseline Artifact BA-10: `MUST_FIX` Parameter Sheet and Selection Methods (DRAFT V2)

> **BASELINE ARTIFACT BA-10 V2 — DRAFT, NOT SEALED. Design / documentation only. No value is chosen: every value is `UNSET`.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-PARAMETER-SHEET
> ARTIFACT_VERSION          = 2-DRAFT   (supersedes 1-DRAFT, blob 099c9a6a0b238db501780e5d11bdb45429652705, which stays as history)
> ARTIFACT_STATUS           = DRAFT (phase 2 of baseline preparation)
> CANDIDATE_HASH            = UNSET     SEAL_HASH = UNSET
> AUTHORITY_CONTRACT        = CT-21D V2.1 section 21.7 and BASE-18 + V2.2 + V2.3 + V2.4
> PHASE-1 RULING APPLIED    = selection method with formula per parameter; arbitrary numbers prohibited; Owner risk choices reserved to the Owner
> BASE ITEM                 = BASE-18: NOT MET (values UNSET; PARAM-01..03 need Owner risk choices; PARAM-04 needs non-governing evidence; PARAM-05 needs Owner/CAD-manager confirmation)
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 1. Rules of this sheet

- Every value is **`UNSET`** until it is chosen **with its stated intent**, by the **authority** named in its row, and recorded with its justification. A number without an intent is **prohibited**.
- **Owner-reserved choices** (risk appetite: `p`, `alpha`, `u`, `c`) are **never** chosen by the Coordinator, the Architect or the implementer. They are asked in the decision memo of section 5.
- A value, once chosen, is **sealed before the first governing run** that uses it and is **never derived from the outputs of governing runs**. Non-governing evidence (dry runs, qualification) may inform a Coordinator engineering choice (PARAM-04), never an Owner risk choice.
- **Operational parameters** (section 3) never change a semantic verdict.

## 2. `MUST_FIX_BEFORE_AUTHORITY_BASELINE` parameters

### 2.1 Summary

| Id | Parameter | Method (section 2.2) | Who chooses | Inputs needed | Current value |
|---|---|---|---|---|---|
| `PARAM-01` | governing runs per required group | zero-failure detection: `N >= ln(alpha) / ln(1 - p)` | **Owner** chooses `p` and `alpha` (possibly one pair for class-A groups and one for class-B groups); the Coordinator records `N` | `p`, `alpha` | `UNSET` |
| `PARAM-02` | determinism-validation repetitions (V2.1 27.4) | same formula, where the target mode is "the event multiset of a plan differs across runs" | **Owner** chooses `p_det` and `alpha_det`; the Coordinator records `N_det` | `p_det`, `alpha_det` | `UNSET` |
| `PARAM-03` | clean-run sample for false-rejection measurement (V2.1 10.6) | one-sided upper bound: with 0 false rejections in `n` clean runs, the bound at confidence `c` is `1 - (1 - c)^(1/n)`; the sample needed so that the bound is `<= u` is `n >= ln(1 - c) / ln(1 - u)` | **Owner** chooses `u` and `c`; the Coordinator records `n` | `u`, `c` | `UNSET` |
| `PARAM-04` | `MAX_ATTEMPTS` per scenario | Coordinator policy from the **observed INVALID rate by cause** in non-governing dry runs (section 2.2) | **Coordinator** (engineering policy, not an Owner risk choice) | dry-run evidence (host; not authorized yet) | `UNSET` |
| `PARAM-05` | machine-class label and equivalence policy | identity policy: which machine attributes define a class, and the equivalence rule (section 2.2) | **Owner or CAD manager** confirms the policy; the Coordinator records it; the label value is assigned at capture | confirmation of the proposed conservative policy | `UNSET` |

### 2.2 Methods

**PARAM-01 and PARAM-02 (zero-failure detection).** A governing group verdict rests on `N` independent governing runs (each a fresh process, V2.1 section 20). If a failure mode of the group occurs in a run with probability at least `p`, the probability that **none** of `N` runs shows it is at most `(1 - p)^N`. Requiring that miss probability to be at most `alpha` gives

```text
N >= ln(alpha) / ln(1 - p)        (N rounded up to the next integer)
```

- `p` = the smallest per-run occurrence probability of the targeted mode that the Owner wants the characterization to be able to catch; `alpha` = the miss risk the Owner accepts.
- The same `N` is used for every group of the same class unless the Owner chooses class-specific pairs.
- PARAM-02 uses the same formula; the mode is a non-identical `(event kind, selector, step)` multiset for the same plan.
- A governing FAIL or UNKNOWN is never re-run to obtain a better result (V2.1 21.3); `N` counts governing runs that were planned, not attempts.
- The number of non-governing dry runs of the group `WU` (BA-09 V2) and the repetition count of the groups `EA1` and `EA2` follow `PARAM-01`.

**PARAM-03 (false-rejection bound).** With `k` false rejections observed in `n` clean governing runs, the reported quantity is the one-sided Clopper-Pearson upper bound at confidence `c`. For `k = 0` it is `1 - (1 - c)^(1/n)`; the sample that makes this bound at most `u` is

```text
n >= ln(1 - c) / ln(1 - u)        (n rounded up)
```

- `u` = the largest false-rejection rate the Owner wants to be able to bound; `c` = the confidence the Owner wants.
- The contract sets **no threshold**: the observed rate and its bound are **reported** to the Owner (Act 2). Choosing `u` and `c` fixes only the sample size; it does not accept any observed rate.

**Illustrative evaluations of the formulas (arithmetic only; NOT a proposal and NOT a choice):**

| `p` (or `u`) | `alpha` (or `1 - c`) | `N` (or `n`) |
|---|---|---|
| 0.10 | 0.05 | 29 |
| 0.05 | 0.05 | 59 |
| 0.01 | 0.05 | 299 |
| 0.10 | 0.01 | 44 |
| 0.05 | 0.01 | 90 |

**PARAM-04 (`MAX_ATTEMPTS`).** The Coordinator fixes, per scenario, the smallest `m` such that the probability that `m` consecutive attempts are all `INVALID` (for causes classified tooling or environment, V2.1 21.3), at the INVALID rate `r` observed in non-governing dry runs, is at most a Coordinator-chosen engineering bound `beta`; contamination causes are excluded from the rate because they are removed by procedure, not by retry. Every attempt is logged; no cherry-picking; `MAX_ATTEMPTS` never permits the retry of a governing FAIL or UNKNOWN. The value stays `UNSET` until the dry-run evidence exists (host work, not authorized).

**PARAM-05 (machine class).** Proposed **conservative** policy (a restriction, never an extension of the guarantee), for Owner or CAD-manager confirmation:

| Aspect | Proposal |
|---|---|
| equivalence rule | **no cross-machine equivalence**: one machine class = one physical machine installation |
| identity attributes recorded | Windows `MachineGuid` (hashed in the repository; clear value only in local evidence), OS version and build, AutoCAD product and build (`acad.exe` SHA-256), AutoCAD profile name, `SECURELOAD` and `TRUSTEDPATHS` values |
| label | `MC-<first 12 hex digits of SHA-256(MachineGuid)>`, assigned at the manifest capture |
| change of any attribute | a new machine class (a new tuple) |

## 3. Operational-only parameters (do not affect a semantic verdict)

| Id | Parameter | Classification | Rule |
|---|---|---|---|
| `OP-01` | control-plane time budgets or watchdogs | `SHOULD_BE_REMOVED` from the semantic verdict | a slow measurement never becomes `UNKNOWN`; an expired watchdog makes the **run or instrument `INVALID`**, is logged and counts in the attempt log |
| `OP-02` | duration of the post-CP observation queue | `MAY_BE_OPERATIONAL_AT_EXECUTION` | informational only; recorded |

## 4. Values that are **not** parameters of this sheet

| Item | Where it lives |
|---|---|
| tolerances of `SEMANTIC_COMPARISON` | the Selective kind baseline (BA-08 V2 section 10); `TOL_SCALE` has no authority and needs a separate decision, test and record |
| E-04 admitted set, `LOCK_MODE` selection | evidence-derived (BA-08 V2) |
| E-04 designated route | BA-08 V2 section 8 (Coordinator ratification) |

## 5. Owner decision memo (prepared; the Owner has **not** answered)

The following questions are **reserved to the Owner**. Nobody answers them on the Owner's behalf. Until they are answered, `BASE-18` is not met and `CT21D_AUTHORITY_BASELINE_READY` cannot become `TRUE`.

| Question | What it decides | Effect |
|---|---|---|
| **Q-O1** | `p` and `alpha` for **class-A** groups (the guarantee-bearing controls) | fixes `PARAM-01` for class-A groups |
| **Q-O2** | `p` and `alpha` for **class-B** groups (risk-reduction controls), or "same as class A" | fixes `PARAM-01` for class-B groups, the `WU` dry runs and `EA1`/`EA2` |
| **Q-O3** | `p_det` and `alpha_det` for the determinism of the expected event set | fixes `PARAM-02` |
| **Q-O4** | `u` and `c` for the false-rejection bound to be reported | fixes `PARAM-03` |
| **Q-O5** | confirmation (or change) of the conservative machine-class policy of section 2.2 | fixes the `PARAM-05` policy; the label is assigned at capture |

A consequence the Owner should weigh: a smaller `p`, `u`, `alpha` or `1 - c` increases the number of governing AutoCAD runs (see the illustrative table), and therefore the duration and cost of the characterization.

```text
PARAM-01 .. PARAM-05 = UNSET     OP-01, OP-02 = OPERATIONAL
BASE-18 = NOT MET     OWNER DECISIONS Q-O1 .. Q-O5 = NOT ASKED YET (memo prepared)
CT21D_AUTHORITY_BASELINE_READY = FALSE
```
