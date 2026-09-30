# I-52 — CT-21D Owner Decision Package V1 (BA-10 Q-O1..Q-O5 and the non-governing host gate)

> ```text
> DOCUMENT            = Owner decision package ordered by the Coordinator (decisions section 232; AR4-21, section 217; order of
>                       section 218: after the candidate rulings, which are those of section 231)
> STATUS              = PREPARED; QUESTIONS PUT TO THE OWNER; NO ANSWER RECORDED. Not a baseline artifact; not in the BA-11 registry
> AUTHORITY_CONTRACT  = CT-21D-V2.6 AGREED (section 230; composite 97100cff11868807f3c49f25bf25d0385c86e8873cba0cf2c1996338b9b98eb4)
> BOUND_PRODUCT_SHA   = 95690c28 (non-tuple bound item)
> SOURCES             = docs/initiatives/I-52-ct21d-baseline-ba-10-parameter-sheet-v7.md section 5 (the questions, verbatim in
>                       substance) and docs/automation/evidence/I-52-ct21d-cost-memo-v7.md / .py (ILLUSTRATIVE arithmetic)
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

Nothing in this package is a recommendation of a value. The agents do not choose `p`, `alpha`, `u`, `c`, residual-risk acceptance or the final reduced-guarantee acceptance: those are the Owner's.

## 1. State the Owner is deciding on

| Item | State |
|---|---|
| Effective contract | CT-21D-V2.6 agreed (section 230) |
| Candidates | BA-01 (`97100cff…`), BA-02a V3 (`acb5e01d…`), BA-03 V5 (`0afb5d9d…`), section 231; registered in BA-11 V9 |
| Not candidates | BA-05 (host census K-2), BA-07 (finding V7-01), BA-10 (this package), BA-06/08/09/04/02b (dependencies; host K-3/K-8) |
| Ratified / sealed | none |

## 2. Parameter questions (BA-10 V7 section 5)

| Question | Who answers | What it fixes |
|---|---|---|
| **Q-O1** | Owner | `p_A`, `alpha_A` for class-A groups (guarantee-bearing controls): `N_A` of `PARAM-01`, and with the same pair `CHAR_RUNS` of the pin- and selection-feeding characterization runs and of the `HDM-CHAR-*` pinned configuration |
| **Q-O2** | Owner | `p_B`, `alpha_B` for class-B groups (risk-reduction controls), or "same as class A": `N_B` of `PARAM-01` and `WU_DRY_RUNS = N_B` |
| **Q-O3** | Owner | `p_det`, `alpha_det` for the determinism of the expected event set: `N_det` of `PARAM-02` |
| **Q-O4** | Owner | `u`, `c` of the false-rejection bound to report: `n` of `PARAM-03` |
| **Q-O5** | Owner, or the CAD manager the Owner designates | confirmation or change of the machine-class policy of BA-10 section 2.2, and the machine on which the characterization will run: the `PARAM-05` policy |

What moves the cost (details in the memo): `TOTAL` grows roughly with `1/p` and only logarithmically with `1/alpha`; `u` and `c` act only through `ceil(n/K)`; `N_B` drives the dry runs; `N_det` drives the learning and composition-check runs. Illustrative totals of the memo over the BA-04 V7 catalog (437 scenarios), **not options offered to the Owner**: row1 (`p` 0.10, `alpha` 0.05, `u` 0.05, `c` 0.95) 10710 runs; row2 5235; row3 2680; row4 1585.

## 3. Host gate authorization (BA-11 section 3.3)

`BASELINE_READY` cannot be reached without one **non-governing** host gate that no document authorizes today. The plan, verbatim in scope from BA-11 section 3.3:

| Item | Produces |
|---|---|
| machine-attribute observation (read-only, no instrument, no manifest) | the `PARAM-05` label values (needs Q-O5 first) |
| `TOL_SCALE` test and record (K-3) | evidence for the Architect's `TOL_SCALE` decision (BA-08, BA-04) |
| library entity-type census (K-2) with the AR4-10 confirmations | the census record (BA-05 candidate, BA-08) |
| fixture instances and the `FX-IMP` construction (K-8) on the fixture construction build 69daf03a (AR6-02) | instances and conformance records (`FIXTURE_INSTANCE@<FX>` pins) |

This needs an explicit Owner authorization, naming the machine and AutoCAD session and confirming that the gate is non-governing and produces no guarantee evidence.

## 4. Items that stay with others after the answers

- **Coordinator:** the `PARAM-04` cap and `EVIDENCE_REPETITION`.
- **Architect:** the `TOL_SCALE` decision, after K-3.
- **Implementer:** BA-10 V8 with the answered values, then the memo re-run and the candidate chain in seal order.
- **Owner, reserved for later:** Owner Act 2, residual-risk acceptance, and the final reduced-guarantee acceptance. None of these is asked here.
