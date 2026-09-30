# I-52 — CT-21D Baseline Artifact BA-10: `MUST_FIX` Parameter Sheet (DRAFT V1)

> **BASELINE ARTIFACT BA-10 — DRAFT, NOT HASHED, NOT AGREED. Design / documentation only. No value is chosen: every value is `UNSET`.**
>
> ```text
> ARTIFACT_ID               = CT21D-BASE-PARAMETER-SHEET
> ARTIFACT_VERSION          = 1-DRAFT
> ARTIFACT_STATUS           = DRAFT (phase 1 of baseline preparation)
> ARTIFACT_HASH             = TO_BE_RECORDED_AT_SEAL   (see BA-11)
> AUTHORITY_CONTRACT        = CT-21D V2.1 section 21.7 and BASE-18 + V2.2 + V2.3 (AGREED_FOR_BASELINE_ARTIFACT_PREPARATION)
> BASE ITEM                 = BASE-18  (every MUST_FIX_BEFORE_AUTHORITY_BASELINE value, with its justification): NOT MET
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 1. Rules of this sheet

- A parameter classified `MUST_FIX_BEFORE_AUTHORITY_BASELINE` needs an **explicit statistical or engineering intent and a justification**; the count is **never invented**.
- Each parameter states: purpose, intent, the authority needed to choose the value, the constraints the contract already imposes, and the current value. **Current value = `UNSET`** for all of them.
- **Operational-only parameters** (section 3) are kept apart: they never change a semantic verdict.
- A value fixed later is **sealed before the first governing run** that uses it (as in the catalog, BA-04) and is **not derived from the outputs of governing runs**.

## 2. `MUST_FIX_BEFORE_AUTHORITY_BASELINE` parameters

| Id | Parameter | Purpose | Intent that must be stated to choose the value | Authority needed to choose | Constraints already fixed by the contract | Current value |
|---|---|---|---|---|---|---|
| `PARAM-01` | **repetitions per group** (governing runs per required group) | to make a group verdict rest on more than one observation and to detect non-determinism of the host | the confidence the Owner needs in a group verdict, and which groups are deterministic by construction (a control-plane-only group may need one run) | Coordinator and Architect, with Owner input on the confidence needed | applies to every required group of BA-02; also fixes the number of **non-governing dry runs** of the group `WU` (BA-09) and the repetition count `N` of the groups `EA1` and `EA2` (BA-04); a governing FAIL or UNKNOWN is never re-run to obtain a better result (V2.1 21.3) | `UNSET` |
| `PARAM-02` | **determinism-validation repetitions** | to test that the multiset of `(event kind, selector, step)` of a plan is identical across governing validation runs (V2.1 27.4) | the level of evidence that `EXPECTED_MUTATION_EVENT_SET` is a deterministic function of the plan, and the number of independent sessions needed | Coordinator and Architect | the runs are on fresh sessions and fresh fixtures; a non-identical multiset gives `EMPIRICAL_BEHAVIOR_DIFFERENCE` (class B) | `UNSET` |
| `PARAM-03` | **clean-run sample for false-rejection measurement** | to measure the rate at which a correct state is reported as non-success (O7 or another), the availability cost reported to the Owner (V2.1 10.6) | the **precision the Owner needs** for that rate (for example the width of the interval on the reported proportion) and the number of sessions over which it is measured | Owner (intent) with Coordinator and Architect (design) | no threshold is set by the contract: the rate is **reported**, not judged; it is an Owner Act 2 input | `UNSET` |
| `PARAM-04` | **`MAX_ATTEMPTS`** | to bound the attempts per scenario so that a governing result cannot be obtained by retrying | how many authorized attempts per scenario are tolerable given the INVALID causes (tooling, environment, contamination) | Coordinator | attempt log is mandatory and lists every attempt; a retry needs a Coordinator authorization per occurrence with a new run id; no retry is authorized now (V2.1 21.3) | `UNSET` |
| `PARAM-05` | **machine-class label** | to bind the machine and profile part of the tuple to a stable label | which machines are equivalent for the purpose of the characterization | Owner or CAD manager, recorded by the Coordinator | part of the `MACHINE_PROFILE_BOUND` tuple; the machine name stays in local evidence only (V2.1 2.1) | `UNSET` |

## 3. Operational-only parameters (do not affect a semantic verdict)

| Id | Parameter | Classification | Rule |
|---|---|---|---|
| `OP-01` | time budgets or watchdogs of the control plane (for example hashing, process supervision) | `SHOULD_BE_REMOVED` from the semantic verdict | a slow measurement never becomes `UNKNOWN`; a **control-plane watchdog** that expires makes the **run or instrument `INVALID`** (a run status), is logged and counts in the attempt log (V2.1 12.4, 21.1) |
| `OP-02` | duration of the post-CP observation queue | `MAY_BE_OPERATIONAL_AT_EXECUTION` | informational only; recorded; the `POST_V1_TO_CP_QUEUE_LOG` and the post-CP characterization are reports, not inputs to SUCCESS (V2.1 10.7, 10.8) |

## 4. Values that are **not** parameters of this sheet

| Item | Where it lives |
|---|---|
| numeric tolerances and normalizations of `SEMANTIC_COMPARISON` | the Selective kind authority baseline (BA-08); V2.2 PA-9 gate |
| the E-04 admitted set and the `LOCK_MODE` choice | evidence-derived values (BA-08 states the criteria and candidate sets, not the values) |
| the designated invocation routes | BA-08 (candidate set) |

## 5. Status

```text
PARAM-01 .. PARAM-05 = UNSET     OP-01, OP-02 = OPERATIONAL (not semantic)
BASE-18 = NOT MET (values and justifications missing)
CT21D_AUTHORITY_BASELINE_READY = FALSE
```
