# I-52 — CT-21D Cost Memo V8: Run Plan Implied by the Owner and Coordinator Values (NOT SEALED; arithmetic only)

> **NOT SEALED. NOT A BASELINE ARTIFACT. NOT A BA-11 REGISTRY ENTRY. Arithmetic only: the values are the Owner's (Q-O1..Q-O4) and the Coordinator's (`PARAM-04`, `EVIDENCE_REPETITION`), as BA-10 V8 section 2.4 records them; the memo chooses nothing and proposes nothing. Every count below is read from the draft catalog and changes with it.**
>
> ```text
> DOCUMENT            = I-52 CT-21D cost memo V8 (support of BA-10 V8 sections 2.3 and 2.4; the run plan implied by the recorded
>                       values, over the BA-10 V8 bytes and the BA-04 V7 catalog, unchanged). It supersedes the V7 memo
>                       (docs/automation/evidence/I-52-ct21d-cost-memo-v7.md, blob 80ee88d9a1f03cc3ef2f1b575b340cd724755fa2, and .py, blob
>                       57e5330f148eb1153ad334ec46cc340cdb82f297), which stays unchanged as history, as do the earlier memos
> STATUS              = NOT SEALED; not a BA-11 registry entry; not part of the bytes of BA-10 V8 or BA-04 V7; no artifact depends on it
> AUTHORITY_CONTRACT  = CT-21D-V2.6 AGREED (decisions section 230; composite SHA-256
>                       97100cff11868807f3c49f25bf25d0385c86e8873cba0cf2c1996338b9b98eb4). The memo cites no clause of the contract
> BOUND_PRODUCT_SHA   = 95690c28 (a non-tuple bound item). The memo states no code fact; it counts the scenarios of BA-04 V7
> VALUES              = Owner decisions Q-O1..Q-O5, recorded verbatim in docs/automation/evidence/I-52-ct21d-owner-decisions-q-o1-q-o5-and-host-gate.md
>                       and in decisions section 233; Coordinator values PARAM-04 (m) and EVIDENCE_REPETITION recorded in section 233
>                       for the Architect's review. This report is decisions section 234
> SOURCE              = docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v7.json (draft; 437 scenarios; every count changes
>                       with the catalog); the closed key list of docs/initiatives/I-52-ct21d-baseline-ba-08-selective-kind-baseline-v7.md
>                       section 11.2 (rows HDM-2 and HDM-4; draft); docs/initiatives/I-52-ct21d-baseline-ba-10-parameter-sheet-v8.md
>                       (git blob 820d90c8a2172ad32697e66f0c9b0f3eb5fadbdb; SHA-256 over its LF-normalized bytes 867fc4adb332f0a22ade4a24cb74309e5c76f39a90f5b24ab50f605144889dd5),
>                       from which the script reads the Owner pairs, the stated counts, `m` and `EVIDENCE_REPETITION`, and whose rules
>                       it checks (block A.7); for block C, the V7 memo, the V6 memo, the V6 catalog and BA-08 V6. The output cites
>                       the SHA-256 of each file
> SCRIPT              = docs/automation/evidence/I-52-ct21d-cost-memo-v8.py (SHA-256 f3dded5380de5f170c3b93363630218f5a13b8e91e5ee31062ae281bdfba083f over its LF-normalized bytes,
>                       computed with Python hashlib); its complete output is section 4, verbatim. It exits 1, printing no figures, if
>                       BA-10 V8 does not state N_A = 44, N_B = 29, N_det = 90, n = 59, WU_DRY_RUNS = 29, m = 3 and
>                       EVIDENCE_REPETITION = 1, or if a count derived from the pairs differs from the one stated
> OPEN                = nothing in this memo is open on its own account. The questions on BA-10 V8 (AQ-V8-01, AQ-V8-02) and the review of the
>                       Coordinator values move no count shown here, except that a different `m` or `EVIDENCE_REPETITION` would move the
>                       lines D.2 and D.6
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 1. Purpose

BA-10 V8 is symbolic: it defines the repetition rules and the run expression of its section 2.3 and holds no count of the scenario catalog. Its section 2.4 records the values: the Owner's pairs `(p_A, alpha_A) = (0.10, 0.01)`, `(p_B, alpha_B) = (0.10, 0.05)`, `(p_det, alpha_det) = (0.05, 0.01)` and `(u, c) = (0.05, 0.95)`, the counts the formulas derive from them (`N_A = 44`, `N_B = 29`, `N_det = 90`, `n = 59`), `WU_DRY_RUNS = N_B = 29`, and the Coordinator values `m = 3` and `EVIDENCE_REPETITION = 1`. This memo evaluates the expression of section 2.3 on the draft BA-04 V7 catalog (437 scenarios) for exactly those values and gives the sensitivities of each value. It is decision support to be reviewed, not an input of any value.

The V7 memo evaluated four illustrative rows with one pair for both classes. V8 has one choice and separate pairs: the class-B pair differs from the class-A pair (`N_B < N_A`), and the determinism pair gives `N_det > N_A`. The rules do not assume any ordering of the counts (BA-10 V8 section 2.2, `N_strict(s)`). The memo applies `N_A = 44` only where those rules say so (through `N_strict(s)`, the floor, and the derived `CHAR_RUNS`); block B.4 shows where each count enters, and the Owner's instruction not to generalize 44 is kept.

## 2. Values (read from BA-10 V8 section 2.4; not decisions of this memo)

| Value | Pair or choice | Count | Authority |
|---|---|---|---|
| `N_A` | `p_A = 0.10`, `alpha_A = 0.01` | 44 | Owner, Q-O1 |
| `N_B` | `p_B = 0.10`, `alpha_B = 0.05` | 29 | Owner, Q-O2 |
| `WU_DRY_RUNS` | `= N_B` | 29 | Owner, Q-O2 (recorded by the Coordinator) |
| `N_det` | `p_det = 0.05`, `alpha_det = 0.01` | 90 | Owner, Q-O3 |
| `n` | `u = 0.05`, `c = 0.95` | 59 | Owner, Q-O4 |
| `m` (`PARAM-04`) | `MAX_ATTEMPTS` per repetition slot | 3 | Coordinator, section 233 (for the Architect's review) |
| `EVIDENCE_REPETITION` | runs of a scenario that serves no class-A or class-B control group | 1 | Coordinator, section 233 (for the Architect's review) |
| `K` | members of the `PARAM-03` sample | 10 | read from the draft catalog (not a value) |

The script derives the four counts from the pairs in exact rational arithmetic (the smallest integer `N` with `(1 - p)^N <= alpha`) and prints the unrounded ratios and the `k = 0` one-sided Clopper-Pearson bound at `n - 1` and `n` (block P), so that the method and the calculation are reproducible here as in BA-10 V8 section 2.2.

## 3. Reading the output (figures of section 4)

- **Totals (block B).** The plan is 14510 runs: 9098 planned governing runs (the repetition slots `G`) and 5412 non-governing runs (`D`: 4843 dry runs, 569 characterization runs and no non-governing learning run). The 12 `SYNTH` runs are offline evaluations; every other run is a fresh AutoCAD process on the build under test of its scenario.
- **Worst-case governing attempts (blocks B and D.6; `PARAM-04`).** With `m = 3` per repetition slot, every slot using its cap gives `m * G = 3 * 9098 = 27294` governing attempts (32706 runs and attempts together with the 5412 non-governing runs, which have no slot). Each unit of `m` adds up to 9098 attempts. The worst case assumes every slot is first ruled `INVALID` twice, each retry authorized by the Coordinator per occurrence (BA-10 V8 `PARAM-04`, point 2); it is a bound, not an expectation.
- **By build (blocks A.8 and B.3; AR4-06).** The `PRODUCT_BUILD` carries 3088 governing and 92 non-governing runs, and the `CHARACTERIZATION_BUILD` 5998 governing and 5320 non-governing runs: all 167 dry-run scenarios, the pin- and selection-feeding runs and the `HDM-CHAR-*` runs are on it. The 12 offline runs have no build.
- **Where each count enters (block B.4).** Class-A-only and mixed-class governing scenarios run `N_A = 44` times (mixed classes follow `max(N_A, N_B)`, which is `N_A` because `N_B < N_A`); class-B-only scenarios and the dry runs run `N_B = 29` times; the event-model validation runs `max(N_strict, N_det) = 90` times; a learning micro-scenario and the composition check run `N_det + 1 = 91` times; an `EVIDENCE` scenario runs 1 time. The 132 governing mixed-class scenarios all read `max(N_A, N_B)` in the catalog (block A.5), so the Owner's separate class-B pair acts only on the class-B-only scenarios and on `WU_DRY_RUNS`.
- **`PARAM-03` sample (block B.5).** `K = 10`; `ceil(n / K) = 6` is below `N_strict = 44` of every member, so each member runs `44` times, the sample holds 440 runs (396 on the nine `PRODUCT_BUILD` members and 44 on `EA2-DEFERRED-EVENTS`, which counts for the `PRODUCT_BUILD` only under the recorded build equivalence, AR4-06 (6)), and the bound `n = 59` is far exceeded. `u` and `c` do not move any total (block D.4) unless `u` falls below `u*` (0.0068).
- **Qualification twins (blocks A.9, B.3 and F.4; AR4-06 (3), AR5-05).** 36 twins `<id>-CB` on the characterization build, 580 runs. The seven twins by content of AR5-05 add, with the dry runs of the five `PW-CLONE-*-CB`, 395 runs. The I-14 family (six scenarios) runs on the characterization build only.
- **Against the V7 memo (block C).** The V7 block B rows are reproduced exactly by this engine, and the catalog is unchanged (no scenario, label, governance, build or blocker differs from V6). The nearest V7 row (all three counts 29) was 10710 runs; the plan is 14510, +3800 runs: +2250 in `DEFAULT`, +976 in `LEARN`, +211 in `CHAR`, +183 in `VALIDATE`, +150 in `CLEAN`, +30 in `EVIDENCE_STRICT`; the dry runs do not move because `N_B` stays 29. The V6 catalog at the plan values gives the same total (+0).
- **`N_A` and `N_B` (block D.1).** One unit of `N_A` moves 172 runs (every class-A-only and mixed-class governing scenario and the pin-feeding and `HDM-CHAR-*` runs). One unit of `N_B` moves 173 runs (the 167 dry-run scenarios and the 6 class-B-only governing scenarios); while `N_B` stays below `N_A` it does not touch the mixed-class scenarios. If `N_B` were above `N_A`, the mixed-class scenarios would follow it (+2897 runs at `N_B = 45`).
- **`EVIDENCE_REPETITION` (block D.2).** Each unit adds 60 runs (the 58 `EVIDENCE` scenarios and the 2 exploratory runs), while it stays below the floor of the `EVIDENCE_STRICT` scenarios.
- **`K` (block D.3).** One more sample member adds its own `N_strict` runs and its `N_B` dry runs (+73 runs).
- **`alpha` and `p` per pair (block D.5).** Halving `alpha_A` gives `N_A = 51` (+8.3% of the total); halving `p_A`, `N_A = 90` (+54.5%). Halving `alpha_B` gives `N_B = 36` (+8.3%); halving `p_B`, `N_B = 59` (+49.1%). Halving `alpha_det` gives `N_det = 104` (+1.9%); halving `p_det`, `N_det = 182` (+12.7%).
- **`N_det` alone (block D.8, Q-O3).** Each unit adds 20 runs: 16 learning micro-scenarios, the composition check (`N_det + 1`, ruled by AR5-08) and the 3 validation scenarios, because `N_det = 90` exceeds `N_strict = 44` and `max(N_strict, N_det)` then follows `N_det`.
- **`HDM-CHAR-*` informative runs (blocks A.6, D.7 and E.3).** The four `HDM-CHAR-*` scenarios each vary the 9 keys of BA-08 V7 HDM-2, each key exactly once: 36 informative runs in all, outside the intent (AR4-14 (b)(ii)); their pinned-configuration runs are `4 * N_A = 176`.
- **Rulings (block E).** 18 scenarios carry `CONTRACT_ASSIGNED_GROUPS`, and each label resolves `N_strict` from `GROUPS_SERVED` only (AR4-14 (a)). The Q-O1 pair also sizes 440 non-governing runs (the 6 pin- and selection-feeding scenarios and the pinned-configuration runs of the 4 `HDM-CHAR-*`), with no pair of their own (AR4-14 (b)(i)); the composition check is 91 runs. No learning micro-scenario serves a class-A or class-B group, so the `LEARN` floor adds nothing today.
- **Draft scenarios and run prerequisites (block F).** The totals count the draft scenarios as the catalog states them; the memo models no ruling. K-3: 138 scenarios, 5894 runs; K-8: 45 scenarios, 1497 runs; the 171 `DRAFT` scenarios, 6906 runs. Run prerequisites (F.3), which move no run: the `BOUND_SHA_PRECONDITION_RECORD` on the 425 scenarios on a build under test (14498 runs, every run except the 12 offline ones); the `BUILD_EQUIVALENCE_RECORD` on 163 scenarios (7793 runs); the `REVIEWED_RECORD` on 17 scenarios (1547 runs). The 180 non-governing scenarios on a build under test hold 5412 runs (F.6).
- **Catalog labels (blocks A.1 and A.5).** Every `REPETITION` label of the catalog equals the BA-10 V8 derivation exactly (all 437), with `N_strict` resolved as `N_A`, `N_B` or `max(N_A, N_B)` and the `CHAR_RUNS` kind resolved. Block A.7 checks that BA-10 V8 states each rule the memo applies.

## 4. Script output (verbatim)

```text
I-52 CT-21D COST MEMO V8 - ARITHMETIC OUTPUT (NOT SEALED; NOT A REGISTRY ENTRY; ARITHMETIC ONLY; THE MEMO CHOOSES NOTHING)
catalog file    : docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v7.json
catalog SHA-256 : 76abc5b2f248e77502cd0004c1a41d9d9ce78f493fc2b5a7c9b377db50b52c2b (LF-normalized bytes)
catalog version : 7-DRAFT ; scenarios: 437
key list file   : docs/initiatives/I-52-ct21d-baseline-ba-08-selective-kind-baseline-v7.md (section 11.2, rows HDM-2 and HDM-4)
key list hash   : 552e337ea6dfbbeea0ae730f609b7b2207976c7428d10bc267644955f723e72d (SHA-256, LF-normalized bytes)
rule sheet file : docs/initiatives/I-52-ct21d-baseline-ba-10-parameter-sheet-v8.md
rule sheet blob : 820d90c8a2172ad32697e66f0c9b0f3eb5fadbdb (git blob id of the file bytes)
rule sheet hash : 867fc4adb332f0a22ade4a24cb74309e5c76f39a90f5b24ab50f605144889dd5 (SHA-256, LF-normalized bytes)

P. VALUES (read from BA-10 V8 section 2.4; counts derived from the pairs by the formulas of section 2.2, exact rational arithmetic)
   count                  p / u    alpha / c  derived   stated expected
   N_A (Q-O1)             0.10     0.01           44       44       44
   N_B (Q-O2)             0.10     0.05           29       29       29
   N_det (Q-O3)           0.05     0.01           90       90       90
   n (Q-O4)               0.05     0.95           59       59       59
   WU_DRY_RUNS (= N_B)                            29       29       29
   m (PARAM-04, Coordinator)                          -        3        3
   EVIDENCE_REPETITION                             -        1        1
   every derived count equals the stated one and the expected one: yes (the script exits 1 otherwise)
P.1 the ratios behind the counts (60-digit decimal arithmetic, BA-10 V8 section 2.4):
    N_A    ln(0.01) / ln(1 - 0.10) = 43.708690653565665 -> rounded up 44
    N_B    ln(0.05) / ln(1 - 0.10) = 28.433158805743416 -> rounded up 29
    N_det  ln(0.01) / ln(1 - 0.05) = 89.781134960709769 -> rounded up 90
    n      ln(1 - 0.95) / ln(1 - 0.05) = 58.403974814319771 -> rounded up 59
P.2 PARAM-03 method (k = 0): one-sided Clopper-Pearson upper bound at confidence c = 1 - (1 - c)^(1/n); the smallest n with the bound <= u
    n = 58 : 1 - (1 - 0.95)^(1/58) = 0.050339338330420121983511957501   > u = 0.05
    n = 59 : 1 - (1 - 0.95)^(1/59) = 0.049507609888226947385979516997   <= u = 0.05
    both figures are printed in BA-10 V8 section 2.2 (PARAM-03): yes
P.3 N_B < N_A (29 < 44): no ordering is assumed by the rules; N_strict(s) = N_A for class-A-only and for mixed-class scenarios,
    N_B for class-B-only scenarios, and WU_DRY_RUNS = N_B. N_A is applied only where the rules of BA-10 V8 section 2.2 say so (block B.4).

A. COUNTS (read from the draft catalog; they change with the catalog)
A.1 rule derived from the scenario properties (BA-10 V8 section 2.2) vs the REPETITION field of the catalog: all 437 agree
A.2 scenarios per rule and class composition (classes of the class-A/B CONTROL groups of GROUPS_SERVED; KR, evidence
    groups and CONTRACT_ASSIGNED_GROUPS excluded, AR4-14 (a))
    rule              total  A_ONLY  MIXED  B_ONLY   NONE
    DEFAULT             155      32    118       5      0
    CLEAN                10       0     10       0      0
    VALIDATE              3       0      3       0      0
    LEARN                16       0      0       0     16
    EVIDENCE             58       0      0       0     58
    EVIDENCE_STRICT       3       1      1       1      0
    SYNTH                12       0      0       0     12
    DRY                 167       0      0     167      0
    CHAR                 13       8      0       0      5
    ALL                 437
    LEARN: 16 learning micro-scenarios, 16 governing; governing ones that serve a class-A or class-B group (R(s) = max(N_strict,
    N_det + 1)): 0
A.3 CHAR split: pin- or selection-feeding 6 (E4-LEARN-R1-*, LK-*); HOST_DEFAULT_MAP 4 (HDM-CHAR-*); composition check 1 (EV-L-COMPOSED);
    exploratory 2 (E6-C4, E6-C4-CB); the composition check has the log profile LP-COMPOSE: yes
A.4 PARAM-03 sample (catalog field PARAM_03_SAMPLE): K = 10; every member GOVERNING with rule CLEAN: yes
    members: CL-CLEAN-FX1F-F0, CL-CLEAN-FX1F-P, CL-CLEAN-FX1F-FP, CL-CLEAN-FX2F-ALL, CL-CLEAN-FX4F-ALL, CL-CLEAN-FXANN-FP, CL-CLEAN-FXANNBLANK-FP, CL-CLEAN-FXDIM-FP, CL-CLEAN-FXIMP-FP, EA2-DEFERRED-EVENTS
    members per build: CHARACTERIZATION_BUILD 1, PRODUCT_BUILD 9
    on the CHARACTERIZATION_BUILD: EA2-DEFERRED-EVENTS (its PARAM-03 contribution holds for the PRODUCT_BUILD only under the recorded build
    equivalence, AR4-06 (6))
    CL-CLEAN-* ids outside the sample: none
A.5 exact REPETITION label (N_strict resolved to N_A, N_B or max(N_A, N_B); CHAR_RUNS kind resolved) vs the catalog: all 437 agree
    governing mixed-class scenarios: 132; of them with a label without max(N_A, N_B): 0
A.6 HOST_DEFAULT_MAP keys (BA-08 V7 section 11.2 HDM-2, closed list): CLAYER, CECOLOR, CELTYPE, CELTSCALE, CELWEIGHT, CETRANSPARENCY, CPLOTSTYLE, current text style, current dimension style = 9 keys
    BA-08 V7 HDM-4: N_A runs at the pinned key configuration plus exactly one informative run per varied key (AR4-14 (b)(ii))
    V(s) read from the declared key-change injections of each scenario (each key once, from the closed list, V(s) stated):
    HDM-CHAR-FX1F          V(s) = 9 ; keys of the list it does not vary: 0
    HDM-CHAR-FXANN         V(s) = 9 ; keys of the list it does not vary: 0
    HDM-CHAR-FXDIM         V(s) = 9 ; keys of the list it does not vary: 0
    HDM-CHAR-FXANNBLANK    V(s) = 9 ; keys of the list it does not vary: 0
    informative runs in all: 36 (outside the intent; no Owner answer moves them)
A.7 BA-10 V8 states the rules this memo applies:
    HDM-CHAR-* rule (AR4-14 (b)(ii))                 yes
    EV-L-COMPOSED rule (AR4-01)                      yes
    LEARN rule (R1/R2:BA10-V4-01)                    yes
    N_strict over GROUPS_SERVED only (AR4-14 (a))    yes
    WU_DRY_RUNS = N_B (class of WU)                  yes
    worst case m * G per repetition slot (PARAM-04)  yes
    the V4 wording "at least one informative run" is absent: yes
    the composition-check count is recorded as ruled by AR5-08 (AQ-V5-06; BA-10 V8 section 6.1): yes
    rules of its section 2.2 table: DRY, LEARN, CHAR, CLEAN, SYNTH, VALIDATE, EVIDENCE, EVIDENCE_STRICT, DEFAULT
A.8 scenarios per build under test (AR4-06) and governance
    BUILD_UNDER_TEST             total governing  non-gov.   explor.
    PRODUCT_BUILD                   80        78         1         1
    CHARACTERIZATION_BUILD         345       167       177         1
    NONE                            12        12         0         0
A.9 per-build qualification (AR4-06 (3); AR5-05 by content): 36 twins <id>-CB on the CHARACTERIZATION_BUILD (7 of them by content:
    PR-CLOSURE-COMPLETENESS-CB, WU-BOUNDARY-CB, PW-CLONE-BASE-CB, PW-CLONE-CASE-CB, PW-CLONE-WRONG-TYPE-CB, PW-CLONE-NESTED-CB, PW-CLONE-SYMTAB-CB); each with its PRODUCT_BUILD scenario
    and the same REPETITION label: yes; qualification scenarios outside the I-14 family without a twin: none
    twins per rule: CHAR 1, DEFAULT 13, EVIDENCE 22
    the I-14 family (CHARACTERIZATION_BUILD only, no twin): Q-I14, Q-I14-Y, Q-I14-X, Q-I14-SILENT-Y, Q-I14-SILENT-X, Q-I14-P; builds: CHARACTERIZATION_BUILD

B. RUN PLAN IMPLIED BY THE VALUES OF BLOCK P (single choice; BA-10 V8 section 2.3)
   N_A = 44, N_B = 29, N_det = 90, n = 59, WU_DRY_RUNS = 29, EVIDENCE_REPETITION = 1, m = 3, K = 10 (read from the draft catalog);
   ceil(n / K) = 6. CHAR_RUNS = N_A (pin- or selection-feeding), N_A + V(s) (HDM-CHAR-*: N_A runs at the pinned key configuration
   plus exactly one informative run per varied key), N_det + 1 (composition check), EVIDENCE_REPETITION (exploratory)
   G = 9098   (governing planned runs = repetition slots)
   D = 5412   (non-governing runs: dry runs, CHAR, any non-governing learning run; no slot)
   TOTAL = G + D = 14510
   12 SYNTH runs are offline evaluations (BUILD_UNDER_TEST = NONE; no AutoCAD process).
   worst-case governing attempts = m * G = 3 * 9098 = 27294   (PARAM-04, per repetition slot; every slot uses m attempts)
   TOTAL if every governing slot used m attempts = m * G + D = 32706
B.1 terms per rule
   rule             scenarios   gov.      runs
   DEFAULT                155      G      6745
   CLEAN                   10      G       440
   VALIDATE                 3      G       270
   LEARN                   16  mixed      1456
   EVIDENCE                58      G        58
   EVIDENCE_STRICT          3      G       117
   SYNTH                   12      G        12
   DRY                    167      D      4843
   CHAR                    13      D       569
   (LEARN: governing micro-scenarios count in G, non-governing ones in D; DRY and CHAR are in D)
B.2 CHAR term per kind
   PIN_FEEDING              6 scenarios       264 runs
   HDM                      4 scenarios       212 runs
   COMPOSITION              1 scenarios        91 runs
   EXPLORATORY              2 scenarios         2 runs
   (HDM: 4 scenarios * N_A plus 36 informative runs, exactly one per varied key; COMPOSITION: N_det + 1)
B.3 runs per build under test (AR4-06): G / D
   PRODUCT_BUILD              3088 / 92       worst-case governing attempts m * G = 9264
   CHARACTERIZATION_BUILD     5998 / 5320     worst-case governing attempts m * G = 17994
   NONE                         12 / 0        worst-case governing attempts m * G = 36
   of which the 36 qualification twins (all on the CHARACTERIZATION_BUILD): 580 runs
   dry-run scenarios per build: CHARACTERIZATION_BUILD 167
B.4 where each count enters (planned runs per scenario by rule and class composition; this is where the rules of BA-10 V8
    section 2.2 apply N_A = 44, N_B = 29 and N_det = 90; "-" = no scenario)
    rule              A_ONLY   MIXED  B_ONLY    NONE
    DEFAULT               44      44      29       -
    CLEAN                  -      44       -       -
    VALIDATE               -      90       -       -
    LEARN                  -       -       -      91
    EVIDENCE               -       -       -       1
    EVIDENCE_STRICT       44      44      29       -
    SYNTH                  -       -       -       1
    DRY                    -       -      29       -
    (the CHAR rule is in B.2: N_A for pin- or selection-feeding, N_A + V(s) for HDM-CHAR-*, N_det + 1 for the composition check,
    EVIDENCE_REPETITION for the exploratory runs; the learning micro-scenarios and the evidence scenarios serve no class-A or class-B
    control group, so they sit in the NONE column)
B.5 PARAM-03 sample: K = 10 members, n = 59, ceil(n / K) = 6; each member runs max(N_strict(s), ceil(n / K))
    member                     BUILD_UNDER_TEST         class    N_strict    runs
    CL-CLEAN-FX1F-F0           PRODUCT_BUILD            MIXED    44            44
    CL-CLEAN-FX1F-P            PRODUCT_BUILD            MIXED    44            44
    CL-CLEAN-FX1F-FP           PRODUCT_BUILD            MIXED    44            44
    CL-CLEAN-FX2F-ALL          PRODUCT_BUILD            MIXED    44            44
    CL-CLEAN-FX4F-ALL          PRODUCT_BUILD            MIXED    44            44
    CL-CLEAN-FXANN-FP          PRODUCT_BUILD            MIXED    44            44
    CL-CLEAN-FXANNBLANK-FP     PRODUCT_BUILD            MIXED    44            44
    CL-CLEAN-FXDIM-FP          PRODUCT_BUILD            MIXED    44            44
    CL-CLEAN-FXIMP-FP          PRODUCT_BUILD            MIXED    44            44
    EA2-DEFERRED-EVENTS        CHARACTERIZATION_BUILD   MIXED    44            44
    the sample holds 440 runs >= n = 59: yes; on the PRODUCT_BUILD members 396 runs, on the CHARACTERIZATION_BUILD member 44 runs
    every member is at its PARAM-01 floor (ceil(n / K) is below N_strict of each): yes

C. RECONCILIATION WITH THE V7 AND V6 MEMOS (structure kept: the same catalog, labels, engine and checks)
   V7 memo file: docs/automation/evidence/I-52-ct21d-cost-memo-v7.md ; SHA-256 (LF-normalized): f094f232b67d4c03e9e65908b7ad30493d3f6d4f9704237648b948b4969f4e96
   V6 memo file: docs/automation/evidence/I-52-ct21d-cost-memo-v6.md ; SHA-256 (LF-normalized): a796810c6e742cdea3c338f0d40fd60e193ea3c6d9fde1dc6dca86df9dfd8da8
   V6 catalog SHA-256 (LF-normalized): ee8d0f1607ade422f30e42c22f6ad237dd09fa0b79fb882a981d7dec19d6e06c ; scenarios: 437 ; K = 10 ; HDM-CHAR informative runs: 36 in all
C.1 engine regression: the four block B rows of the V7 memo (one pair for both classes) re-evaluated by this engine over the V7 catalog
    are reproduced exactly (G, D and TOTAL): yes; row1 of the V7 memo (N = N_det = 29, n = 59): TOTAL 10710
    the same rows over the V6 catalog through its own REPETITION labels agree with this engine: yes
C.2 plan minus the V7 memo row1 (the nearest V7 row: N_A = N_B = N_det = 29, n = 59, E = 1), per rule
    rule               V7 row1      plan     delta
    DEFAULT               4495      6745     +2250
    CLEAN                  290       440      +150
    VALIDATE                87       270      +183
    LEARN                  480      1456      +976
    EVIDENCE                58        58        +0
    EVIDENCE_STRICT         87       117       +30
    SYNTH                   12        12        +0
    DRY                   4843      4843        +0
    CHAR                   358       569      +211
    TOTAL                10710     14510     +3800   (G +3589, D +211)
C.3 the V6 catalog at the plan values through its own labels vs the V7 catalog
                    V6        V7     delta         G         D
    plan         14510     14510        +0        +0        +0
    by cause (each scenario charged once): added +0, changed label +0, removed +0
C.4 scenario sets, V6 -> V7
    scenarios 437 -> 437; added 0: none
    removed: none
    changed governance: none
    changed build under test: 0
    changed BLOCKED_BY: none
    changed RUN_PREREQUISITES: 180 (AR7-04); in each, BOUND_SHA_PRECONDITION_RECORD appended and nothing else: yes

D. SENSITIVITIES (finite differences at the plan values; each parameter alone, everything else fixed)
D.1 N_A and N_B moved by -1 and by +1 (N_B < N_A: every MIXED term follows N_A; MIXED would follow N_B only if N_B exceeded N_A)
    N_A 44 -> 43: -172 runs ; N_A 44 -> 45: +172 runs
    N_B 29 -> 28: -173 runs ; N_B 29 -> 30: +173 runs
    N_B 29 -> 45 (one above N_A; MIXED then follows N_B): +2897 runs
D.2 EVIDENCE_REPETITION 1 -> 2: +60 runs
D.3 K + 1: one more PARAM-03 sample member (a new governing CL-CLEAN-<fixture> scenario, mixed class) with its dry-run scenario
    +73 runs (the member itself 44, its dry runs 29)
D.4 u and c: they enter only through ceil(n / K); TOTAL changes only when ceil(n / K) > N_strict of a sample member.
    u* = 1 - (1 - c)^(1 / (K * N_strict)) = 0.0068 (N_strict = 44, the smallest of the members) is the value below which that happens
    u halved (0.025): n 59 -> 119, +0 runs ; 1 - c halved (c = 0.975): n 59 -> 72, +0 runs
    example below u* (u = 0.005 with c = 0.95): n = 598, ceil(n / K) = 60 > N_strict = 44: +160 runs, all in the CLEAN term
D.5 each Owner pair alone: alpha halved and p halved (the count follows the formula; the other pairs fixed)
    N_A   alpha 0.01 -> 0.005: N 44 -> 51, TOTAL 14510 -> 15714 (+8.3%) ; p 0.10 -> 0.05: N 44 -> 90, TOTAL 14510 -> 22422 (+54.5%)
    N_B   alpha 0.05 -> 0.025: N 29 -> 36, TOTAL 14510 -> 15721 (+8.3%) ; p 0.10 -> 0.05: N 29 -> 59, TOTAL 14510 -> 21635 (+49.1%)
    N_det alpha 0.01 -> 0.005: N 90 -> 104, TOTAL 14510 -> 14790 (+1.9%) ; p 0.05 -> 0.025: N 90 -> 182, TOTAL 14510 -> 16350 (+12.7%)
D.6 MAX_ATTEMPTS (PARAM-04 m, per repetition slot): worst case = every slot uses m attempts
    m = 3: worst-case governing attempts = 3 * 9098 = 27294 ; each unit of m adds up to 9098 attempts
    the 5412 non-governing runs have no slot; m = 1 would mean no retry (9098 governing attempts)
D.7 HDM-CHAR-* informative runs: exactly one per varied key (AR4-14 (b)(ii)): 36 in all; no Owner answer moves them and no other
    authority adds runs
D.8 N_det moved by +1 (Q-O3 alone): N_det 90 -> 91: +20 runs (LEARN +16, composition check +1, VALIDATE +3: max(N_strict, N_det)
    follows N_det once it exceeds N_strict = 44; at N_det = 90 it does)

E. RULINGS OF DECISIONS SECTIONS 217, 220 AND 226 APPLIED BY BA-10 V8 AND BA-04 V7 (checks; no other reading is sized)
E.1 AR4-14 (a): CONTRACT_ASSIGNED_GROUPS never enter N_strict. Scenarios that carry them: 18; each label resolves N_strict from
    GROUPS_SERVED only (A.5): yes
    PR-CLOSURE-COMPLETENESS    rule EVIDENCE         class composition of GROUPS_SERVED NONE    contract-assigned PR-REC
    KS-SL1-POSTWRITE           rule DEFAULT          class composition of GROUPS_SERVED MIXED   contract-assigned KS, SC-A
    KS-SL1-TX-MISMATCH         rule DEFAULT          class composition of GROUPS_SERVED MIXED   contract-assigned KS, SC-A
    KS-SL4-NEG-DET             rule DEFAULT          class composition of GROUPS_SERVED MIXED   contract-assigned KS, SC-A
    WR-A-X7                    rule DEFAULT          class composition of GROUPS_SERVED A_ONLY  contract-assigned SC-B
    WR-A-X8                    rule DEFAULT          class composition of GROUPS_SERVED A_ONLY  contract-assigned SC-B
    WR-D-X7                    rule DEFAULT          class composition of GROUPS_SERVED A_ONLY  contract-assigned SC-B
    WR-D-X8                    rule DEFAULT          class composition of GROUPS_SERVED A_ONLY  contract-assigned SC-B
    WR-E-X7                    rule DEFAULT          class composition of GROUPS_SERVED A_ONLY  contract-assigned SC-B
    WR-E-X8                    rule DEFAULT          class composition of GROUPS_SERVED A_ONLY  contract-assigned SC-B
    WR-F-X7                    rule DEFAULT          class composition of GROUPS_SERVED A_ONLY  contract-assigned SC-B
    WR-F-X8                    rule DEFAULT          class composition of GROUPS_SERVED A_ONLY  contract-assigned SC-B
    WR-G-X7                    rule DEFAULT          class composition of GROUPS_SERVED A_ONLY  contract-assigned SC-B
    WR-G-X8                    rule DEFAULT          class composition of GROUPS_SERVED A_ONLY  contract-assigned SC-B
    WR-B-X0                    rule DEFAULT          class composition of GROUPS_SERVED A_ONLY  contract-assigned SC-B
    WR-B-X7                    rule DEFAULT          class composition of GROUPS_SERVED A_ONLY  contract-assigned SC-B
    WR-C-X7                    rule DEFAULT          class composition of GROUPS_SERVED A_ONLY  contract-assigned SC-B
    PR-CLOSURE-COMPLETENESS-CB rule EVIDENCE         class composition of GROUPS_SERVED NONE    contract-assigned PR-REC
E.2 AR4-14 (b)(i): CHAR_RUNS = the derived N_c of the class that consumes the pin or the selection (today N_A); no Owner pair of
    its own; the Q-O1 pair (p_A, alpha_A) sizes 6 pin- or selection-feeding scenarios and the pinned-configuration runs of the 4
    HDM-CHAR-* scenarios: 440 runs
E.3 AR4-14 (b)(ii) and AR4-07: 4 HDM-CHAR-* scenarios, CHAR_RUNS(s) = N_A + V(s) with V(s) = 9, 9, 9, 9; informative runs 36 in all
E.4 AR4-01: 16 governing learning micro-scenarios (rule LEARN, N_det + 1 each, on the PRODUCT_BUILD); EV-L-COMPOSED non-governing
    (CHAR, N_det + 1); the Q-I14 family on the CHARACTERIZATION_BUILD with 5 dry-run scenarios

F. RUNS OF THE DRAFT SCENARIOS (counted in the totals of block B as the draft catalog states them; a scenario with two
   blockers is counted under each; the memo does not model any ruling)
F.1 by blocker
   BLOCKED_BY                 scenarios governing      runs
   K-3                              138       138      5894
   K-8                               45        24      1497
   scenarios per rule under each blocker:
    K-3: CLEAN 10, DEFAULT 115, EVIDENCE 7, EVIDENCE_STRICT 3, VALIDATE 3
    K-8: CLEAN 1, DEFAULT 17, DRY 21, EVIDENCE 5, LEARN 1
F.2 by status
   STATUS                         scenarios      runs
   DRAFT                                171      6906
   EXPLORATORY_NON_GOVERNING              2         2
   SEALED_CANDIDATE                      69       671
   SEALED_PENDING_PIN_CANDIDATE         195      6931
F.3 by run prerequisite (a scenario with two prerequisites is counted under each)
   RUN_PREREQUISITES                      scenarios      runs
   BOUND_SHA_PRECONDITION_RECORD                425     14498
   BUILD_EQUIVALENCE_RECORD                     163      7793
   QUALIFICATION_RECORD@Q-I14-P                   1        44
   QUALIFICATION_RECORD@Q-I14-SILENT-X            7       308
   QUALIFICATION_RECORD@Q-I14-SILENT-Y            3       132
   QUALIFICATION_RECORD@Q-I14-X                  87      3828
   QUALIFICATION_RECORD@Q-I14-Y                   5       220
   REVIEWED_RECORD                               17      1547
F.4 AR5-05 (the ruling of AQ-V5-02): the twins by content on the CHARACTERIZATION_BUILD, each with the rule of its source
    PR-CLOSURE-COMPLETENESS-CB rule EVIDENCE  (source EVIDENCE), REACH NONE; dry-run scenario: no; the twin with its dry runs: +1
    WU-BOUNDARY-CB             rule DEFAULT   (source DEFAULT), REACH NONE; dry-run scenario: no; the twin with its dry runs: +29
    PW-CLONE-BASE-CB           rule DEFAULT   (source DEFAULT), REACH CP; dry-run scenario: yes; the twin with its dry runs: +73
    PW-CLONE-CASE-CB           rule DEFAULT   (source DEFAULT), REACH CP; dry-run scenario: yes; the twin with its dry runs: +73
    PW-CLONE-WRONG-TYPE-CB     rule DEFAULT   (source DEFAULT), REACH PR; dry-run scenario: yes; the twin with its dry runs: +73
    PW-CLONE-NESTED-CB         rule DEFAULT   (source DEFAULT), REACH CP; dry-run scenario: yes; the twin with its dry runs: +73
    PW-CLONE-SYMTAB-CB         rule DEFAULT   (source DEFAULT), REACH CP; dry-run scenario: yes; the twin with its dry runs: +73
    all seven with their dry runs: +395 runs
F.5 AR5-04 (the ruling of AQ-V5-01): governing CHARACTERIZATION_BUILD scenarios whose PIN_SLOTS name EVM_FROZEN (learned and
    validated on the PRODUCT_BUILD, AR4-01): 115; per rule: CLEAN 1, DEFAULT 110, EVIDENCE 1, EVIDENCE_STRICT 3
    of them naming BUILD_EQUIVALENCE_RECORD in RUN_PREREQUISITES: 115; their runs: 5002
    the ruling adds a run prerequisite and moves no run count
F.6 AR5-12 as ruled by AR7-04 (BOUND_SHA_PRECONDITION_RECORD, BA-04 V7): scenarios that name it: 425; the scenarios on a build under test,
    governing or not: 425; the same set: yes; their runs: 14498
    per governance: EXPLORATORY_NON_GOVERNING 2, GOVERNING 245, NON_GOVERNING_BY_DESIGN 178
    the non-governing ones (added by AR7-04): 180; their runs: 5412
    mandatory E4-LEARN-R1-*     2 scenarios; runs: 88
    mandatory LK-*              4 scenarios; runs: 176
    mandatory HDM-CHAR-*        4 scenarios; runs: 212
    mandatory WU-DRY-*        167 scenarios; runs: 4843
    under the single rule: E6-C4, E6-C4-CB, EV-L-COMPOSED; runs: 93
    synthetic scenarios (BUILD_UNDER_TEST NONE) naming it: 0
    the ruling adds a run prerequisite only and moves no run count (block C)

END OF OUTPUT
```

## 5. Reproduction

From the repository root: `python docs/automation/evidence/I-52-ct21d-cost-memo-v8.py` (Python 3, standard library only; it reads the V7 catalog, BA-08 V7 for the key list, BA-10 V8 for the pairs, the stated values and the check A.7, and the V6 catalog, BA-08 V6, the V6 memo and the V7 memo for block C). It exits 1, printing no figures, if the value block or the `m` and `EVIDENCE_REPETITION` rows of BA-10 V8 section 2.4 do not state the values above, if a count derived from the pairs differs from the stated one, if the HDM-2 and HDM-4 rows of BA-08 V7 or V6 do not read as expected, or if an `HDM-CHAR-*` scenario declares a key change that is not a key of the list, varies a key twice or does not state its `V(s)`. The output is deterministic: the generator of this memo ran the script twice and compared the two outputs byte for byte before embedding them. Every figure of section 3 is taken from the script's own variables, not typed. It must be re-run whenever the V7 catalog, BA-08 V7 section 11.2 or BA-10 V8 changes (the output cites the SHA-256 of each).

## 6. Delta V8 (decisions 233)

| Ruling or finding | Decision | Fix (section/field) |
|---|---|---|
| Owner values Q-O1..Q-O4 (decisions section 233) | applied | the script reads the pairs from BA-10 V8 section 2.4, derives `N_A`, `N_B`, `N_det`, `n` and `WU_DRY_RUNS = N_B`, and fails closed on any difference; separate pairs replace the single pair of V7 |
| Coordinator values `m = 3`, `EVIDENCE_REPETITION = 1` (section 233, for the Architect's review) | applied | `m` appears only as the worst case `m * G` (blocks B, B.3 and D.6); `EVIDENCE_REPETITION` enters the rules `EVIDENCE`, `EVIDENCE_STRICT` and the exploratory runs |
| Owner instruction on 44 | kept | `N_A` enters only through `N_strict(s)` and the derived `CHAR_RUNS`; block B.4 shows each rule; nothing generalizes it |
| single choice instead of four illustrative rows | applied | block B is one plan; the V7 rows survive only as the engine regression of block C.1 |
| V7 structure | kept | blocks A (counts and checks against BA-10 V8), C (reconciliation), E (rulings) and F (blocker, status, prerequisite) keep their content; the sensitivities of block D are re-based on the actual pairs |
| Q-O5 (machine policy) and the host gate | no count | they move no run count shown here |
