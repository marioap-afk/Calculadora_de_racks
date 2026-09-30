# I-52 — CT-21D Cost Memo V4: Runs Implied by the Parameter Sheet (NOT SEALED; ILLUSTRATIVE)

> **NOT SEALED. NOT A BASELINE ARTIFACT. Arithmetic only: every figure is ILLUSTRATIVE; nothing here is a proposal, a choice or a decision. Every parameter of BA-10 V4 stays `UNSET`.**
>
> ```text
> DOCUMENT            = I-52 CT-21D cost memo V4 (support of BA-10 V4 sections 2.3 and 5)
> STATUS              = NOT SEALED; not a BA-11 registry entry; not part of the bytes of BA-10 V4 or BA-04 V4; no artifact depends on it
> OWNER               = NOT ASKED. This memo is not put to the Owner until BA-10 V4 and this memo are reviewed (decisions section 214, AR3-24)
> SOURCE              = docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v4.json (draft; every count changes with the catalog);
>                       for `HDM-CHAR-*`, the key list of docs/initiatives/I-52-ct21d-baseline-ba-08-selective-kind-baseline-v4.md
>                       section 11.2 (rows HDM-2 and HDM-4; draft); the script also checks that BA-10 V4 section 2.2 states the
>                       `CHAR_RUNS` rule of `HDM-CHAR-*`. The output cites the SHA-256 of the catalog and of BA-08 V4
> SCRIPT              = docs/automation/evidence/I-52-ct21d-cost-memo-v4.py (SHA-256 5776fd5c42117283cca7c12ab33a20c5720800c0eb8521107b9f8f651f50861a over its LF-normalized bytes,
>                       computed with Python hashlib); its complete output is section 4, verbatim
> DELTA RULING        = decisions section 214: AR3-24 (corrected totals, real sensitivities, memo not asked before the V4 review),
>                       AR3-25 (BA-10 has no edge to BA-04: the catalog numbers live here, not in BA-10); correction pass: AR3-11
>                       (`CHAR_RUNS` of `HDM-CHAR-*`: `N_A` runs at the pinned key values plus at least one informative run per varied
>                       key, outside the intent) and the corrected BA-04 V4 catalog (393 scenarios, 161 dry-run scenarios)
> ARCHITECT QUESTIONS = sizes only, none decided: AQ-V4-13 (block E: part (a) = Q-A1, part (b) the `CHAR_RUNS` intent); AQ-V4-01,
>                       AQ-V4-03 and K-6 (AQ-V4-07) with the other blockers of the draft scenarios (block F)
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 1. Purpose

BA-10 V4 is symbolic (AR3-24, AR3-25): it defines the repetition rules and the run expression of its section 2.3 and holds no count of the scenario catalog. This memo evaluates that expression on the **draft** BA-04 V4 catalog, for the illustrative choices that BA-10 V3 section 2.3 used, and gives the sensitivities the Owner will need. It is decision support to be reviewed, not an input of any value.

It also corrects the V3 figures. BA-10 V3 section 2.3 printed 9358, 4543, 2296 and 1333; its own formula gives **9377, 4562, 2315 and 1352** (the table charged the learning term as `19 * N_det` instead of `19 * (N_det + 1)`). Decisions section 214.1 records the corrected figures; section 4 block C recomputes them from the V3 catalog.

## 2. Illustrative choices (ILLUSTRATIVE; not decisions)

| Row | `p` | `alpha` | `u` | `c` | other assumptions (all rows) |
|---|---|---|---|---|---|
| row1 | 0.10 | 0.05 | 0.05 | 0.95 | one pair for both classes (`p_A = p_B = p`, `alpha_A = alpha_B = alpha`); `p_det = p`, `alpha_det = alpha`; `EVIDENCE_REPETITION = 1` (the Coordinator's pending proposal, not a value); `CHAR_RUNS` as BA-10 V4 section 2.2 states it (`N_A` for the pin- and selection-feeding runs; for `HDM-CHAR-*`, `N_A` runs at the pinned key values plus one informative run per varied key, the minimum, with the keys read from BA-08 V4 HDM-2; `EVIDENCE_REPETITION` for the exploratory run); `K` as read from the draft catalog |
| row2 | 0.20 | 0.05 | 0.10 | 0.95 | same |
| row3 | 0.30 | 0.10 | 0.10 | 0.90 | same |
| row4 | 0.50 | 0.10 | 0.20 | 0.90 | same |

These are the four rows of BA-10 V3 section 2.3, kept so that V3 and V4 can be compared. They are not a range the Owner is asked to choose from.

## 3. Reading the output (figures of section 4)

- **Totals.** Row1 gives 10097 runs: 5139 planned governing runs (the repetition slots `G`) and 4958 non-governing runs (`D`: 4669 dry runs and 289 non-governing characterization runs). Rows 2 to 4 give 4922, 2507 and 1472. The 12 `SYNTH` runs of each row are offline evaluations; every other run is a fresh AutoCAD process.
- **V3 to V4 (block C.1, row1: +720).** Dry runs +464: 16 more dry-run scenarios (block C.2: 17 added, and `WU-DRY-EA1-READONLY-OPENS` removed because `EA1-READONLY-OPENS` has no product run, AR3-09). The added dry runs belong to the governing scenarios that enter the governed window, the evidence scenarios included (AR3-02), to the new scenarios, and to the four qualification vehicles `Q-I14-*` (`WU-DRY-Q-I14-*`, DRAFT pending AQ-V4-01). `DEFAULT` + `CHAR` +115: `NC-CLONE-REPLACE` (AR3-07) adds 29; the three `HDM-CHAR-*` (AR3-11) add 114, that is 3 * (`N_A` + 9): `N_A` runs at the pinned key values plus one informative run for each of the 9 keys of BA-08 V4 HDM-2; the exploratory `E6-C4` now runs `EVIDENCE_REPETITION` times (-28). The evidence rules +85: `U-3`, `U-4` and `U-5` now carry the strictest-pair floor (+84, AR3-24), and `Q-I14-SILENT` became `Q-I14-SILENT-Y` and `Q-I14-SILENT-X` (+1). `CLEAN` +29: `CL-CLEAN-FXANNBLANK-FP` (AR3-15). Learning and validation +27: `EV-L-OC-TM-READ` (+30), and the validation scenarios run `max(N_strict, N_det)` with the frozen EVM as reference and no extra run (-3).
- **`N_B` (block D.1).** Below `N_A`, each unit of `N_B` moves 165 runs (161 dry-run scenarios, 3 class-B-only `DEFAULT` scenarios, 1 class-B-only `EVIDENCE_STRICT` scenario). Above `N_A`, each unit moves 293 runs, because the 128 mixed-class scenarios then follow `N_B`. The dry runs are the largest single term.
- **`EVIDENCE_REPETITION` (block D.2).** Each unit adds 36 runs (35 `EVIDENCE` scenarios and the exploratory run), while it stays below the floor of the `EVIDENCE_STRICT` scenarios.
- **`K` (block D.3).** At the four rows `N` dominates `ceil(n / K)`, so one more sample member adds its own `N_strict` runs and its `N_B` dry runs (+58 at row1). Where `n` dominates (row1 with `u = 0.01`) one more member adds 48 runs.
- **`u` and `c` (block D.4).** No effect at the four rows: halving `u` or `1 - c` leaves every total unchanged. They act only below `u*` (0.0103, 0.0212, 0.0324 and 0.0559 for the four rows), and then only the `CLEAN` term grows (row1 with `u = 0.01`: +10 runs).
- **`alpha` and `p` (block D.5).** `alpha` acts logarithmically: halving it adds 21% to 28%. `p` acts roughly inversely: halving it adds 103% to 117%.
- **`MAX_ATTEMPTS` (block D.6).** `m` is per repetition slot. In the worst case every slot uses `m` attempts: `5139 * m` governing attempts at row1, so each unit of `m` adds up to 5139 attempts. The non-governing runs have no slot.
- **`HDM-CHAR-*` informative runs (blocks A.6, B.2 and D.7).** BA-08 V4 HDM-2 lists 9 keys (the 7 context variables `CLAYER`, `CECOLOR`, `CELTYPE`, `CELTSCALE`, `CELWEIGHT`, `CETRANSPARENCY`, `CPLOTSTYLE`, and the current text style and dimension style). HDM-4 asks for one or more runs with each key changed; the memo counts the minimum, one run per key: 9 per scenario and 27 per row, outside the intent. No Owner answer moves them; one more informative run per key in every scenario adds 27.
- **Open question AQ-V4-13 of BA-10 V4 (block E).** Part (a) = Q-A1: 17 scenarios carry `CONTRACT_ASSIGNED_GROUPS`. If those groups entered the strictest pair, 14 scenarios would change class composition: +28 runs at row1 with `N_B = N_A` (`PR-CLOSURE-COMPLETENESS` would run `N_A` times instead of once), +41 with `N_B = N_A + 1`. The three `KS-*` scenarios carry `KS` and `SC-A` by contract (pending AQ-V4-03); they are mixed-class already, so this reading does not change them. Part (b)(ii): if `N_A` applied to every key configuration of HDM-4, each `HDM-CHAR-*` scenario would run `10 * N_A` times instead of `N_A + 9`: +756 non-governing runs at row1 (+351, +162 and +81 at rows 2 to 4). Part (b)(i) has no size without a value.
- **Draft scenarios by blocker (block F).** The totals count the draft scenarios as the catalog states them; the memo models no ruling. At row1: AQ-V4-01, 25 scenarios (17 `EV-L-*`, 4 `Q-I14-*` and their 4 dry runs), 630 runs; AQ-V4-03, the 3 `KS-*`, 87 runs; K-6 (AQ-V4-07), `CL-CLEAN-FXANNBLANK-FP`, 29 runs; K-3, 134 scenarios, 3690 runs; K-8, 32 scenarios, 845 runs; E6-C4 (the catalog does not read it as a seal blocker, pending AQ-V4-04), `NC-S5-O6`, 29 runs. A scenario with two blockers is counted under each.
- **Catalog labels (block A.5).** Every `REPETITION` label of the corrected catalog equals the BA-10 V4 derivation exactly, with `N_strict` resolved as `N_A`, `N_B` or `max(N_A, N_B)` and the `CHAR_RUNS` kind resolved. The 128 governing mixed-class scenarios all read `max(N_A, N_B)`.

## 4. Script output (verbatim)

```text
I-52 CT-21D COST MEMO V4 - ARITHMETIC OUTPUT (ILLUSTRATIVE; NOT A PROPOSAL, NOT A CHOICE, NOT A DECISION)
catalog file    : docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v4.json
catalog SHA-256 : 4f8e4ffdc1ed6c4cc4e237666277ce71733c9f7ceab426a27e6585120f2ced84 (LF-normalized bytes)
catalog version : 4-DRAFT ; scenarios: 393
key source file : docs/initiatives/I-52-ct21d-baseline-ba-08-selective-kind-baseline-v4.md (section 11.2, rows HDM-2 and HDM-4)
key source hash : 5b83a32c9bbf51add8ca85c0b44686d1907ef3226501b6e6442ab3af64bcc5d7 (SHA-256, LF-normalized bytes)

A. COUNTS (read from the draft catalog; they change with the catalog)
A.1 rule derived from the scenario properties (BA-10 V4 section 2.2) vs the REPETITION field of the catalog: all 393 agree
A.2 scenarios per rule and class composition (classes of the class-A/B CONTROL groups of GROUPS_SERVED; KR, evidence
    groups and CONTRACT_ASSIGNED_GROUPS excluded)
    rule              total  A_ONLY  MIXED  B_ONLY   NONE
    DEFAULT             142      25    114       3      0
    CLEAN                10       0     10       0      0
    VALIDATE              3       0      3       0      0
    LEARN                17       0      0       0     17
    EVIDENCE             35       0      0       0     35
    EVIDENCE_STRICT       3       1      1       1      0
    SYNTH                12       0      0       0     12
    DRY                 161       0      0     161      0
    CHAR                 10       7      0       0      3
    ALL                 393
A.3 CHAR split: pin- or selection-feeding 6 (E4-LEARN-R1-*, LK-*); HOST_DEFAULT_MAP 3 (HDM-CHAR-*, group HDM); exploratory 1 (E6-C4)
A.4 PARAM-03 sample (catalog field PARAM_03_SAMPLE): K = 10; every member GOVERNING with rule CLEAN: yes
    members: CL-CLEAN-FX1F-F0, CL-CLEAN-FX1F-P, CL-CLEAN-FX1F-FP, CL-CLEAN-FX2F-ALL, CL-CLEAN-FX4F-ALL, CL-CLEAN-FXANN-FP, CL-CLEAN-FXANNBLANK-FP, CL-CLEAN-FXDIM-FP, CL-CLEAN-FXIMP-FP, EA2-DEFERRED-EVENTS
    CL-CLEAN-* ids outside the sample: none
A.5 exact REPETITION label (N_strict resolved to N_A, N_B or max(N_A, N_B); CHAR_RUNS kind resolved) vs the catalog: all 393 agree
    governing mixed-class scenarios: 128; of them with a label without max(N_A, N_B): 0
A.6 HOST_DEFAULT_MAP keys (BA-08 V4 section 11.2 HDM-2): CLAYER, CECOLOR, CELTYPE, CELTSCALE, CELWEIGHT, CETRANSPARENCY, CPLOTSTYLE, current text style, current dimension style = 9 keys
    HDM-4 requires, for each HDM-CHAR-* scenario, runs at the pinned key values and one or more runs with each key variable
    changed; this memo counts the minimum: V = 9 informative runs per HDM-CHAR-* scenario (outside the intent)
A.7 BA-10 V4 states the HDM-CHAR-* CHAR_RUNS rule (N_A runs at the pinned key values plus at least one informative run per
    varied key): yes; rules of its section 2.2 table: DRY, LEARN, CHAR, CLEAN, SYNTH, VALIDATE, EVIDENCE, EVIDENCE_STRICT, DEFAULT

B. ILLUSTRATIVE TOTALS FOR THE CHOICES USED IN BA-10 V3 SECTION 2.3
   assumptions (ILLUSTRATIVE): one pair for both classes (p_A = p_B = p, alpha_A = alpha_B = alpha), p_det = p, alpha_det = alpha,
   EVIDENCE_REPETITION = 1, K = 10; CHAR_RUNS = N_A (pin- or selection-feeding), N_A + 9 (HDM-CHAR-*: N_A runs at the pinned
   key values plus one informative run per varied key, the minimum) and EVIDENCE_REPETITION (exploratory)
   p     alpha  u     c        N    n  N_det ceil(n/K)       G       D   TOTAL
   0.10  0.05   0.05  0.95    29   59     29         6    5139    4958   10097
   0.20  0.05   0.10  0.95    14   29     14         3    2514    2408    4922
   0.30  0.10   0.10  0.90     7   22      7         3    1289    1218    2507
   0.50  0.10   0.20  0.90     4   11      4         2     764     708    1472
   G = governing planned runs (repetition slots); D = non-governing runs (dry runs, CHAR, any non-governing learning run); TOTAL = G + D.
   12 SYNTH runs per row are offline evaluations (no AutoCAD process).

B.1 terms per rule
   rule                 row1     row2     row3     row4
   DEFAULT              4118     1988      994      568
   CLEAN                 290      140       70       40
   VALIDATE               87       42       21       12
   LEARN                 510      255      136       85
   EVIDENCE               35       35       35       35
   EVIDENCE_STRICT        87       42       21       12
   SYNTH                  12       12       12       12
   DRY                  4669     2254     1127      644
   CHAR                  289      154       91       64
B.2 CHAR term per kind
   PIN_FEEDING           174       84       42       24
   HDM                   114       69       48       39
   EXPLORATORY             1        1        1        1
   (HDM: 3 scenarios * (N_A + 9); the 27 informative runs per row are outside the intent)

C. RECONCILIATION WITH BA-10 V3 SECTION 2.3 (V3 formula over the V3 catalog)
   V3 catalog SHA-256 (LF-normalized): f50c61c49641a3909a04b2da09e4b4da1e282e0c9364b904c1faefb0e0e13fbf ; scenarios: 370
   V3 counts: CLEAN 9, DEFAULT_A 131, DEFAULT_B 3, CHAR 14, LEARN 19, EVIDENCE 37, SYNTH 12, DRY 145
   p     alpha  u     c          V3 formula  V3 printed (19*N_det)  difference
   0.10  0.05   0.05  0.95             9377                   9358          19
   0.20  0.05   0.10  0.95             4562                   4543          19
   0.30  0.10   0.10  0.90             2315                   2296          19
   0.50  0.10   0.20  0.90             1352                   1333          19
   The V3 table printed the right-hand figures: it charged LEARN as 19 * N_det instead of 19 * (N_det + 1).
C.1 V4 minus V3 (V3 formula) per row, by comparable block
   row         V3       V4    delta  DEFAULT+CHAR     CLEAN  LEARN+VALIDATE  EVIDENCE*     DRY
   row1      9377    10097     +720          +115       +29             +27        +85    +464
   row2      4562     4922     +360           +70       +14             +12        +40    +224
   row3      2315     2507     +192           +49        +7              +5        +19    +112
   row4      1352     1472     +120           +40        +4              +2        +10     +64
   EVIDENCE* = EVIDENCE + EVIDENCE_STRICT + SYNTH (V4) against EVIDENCE + SYNTH (V3)
C.2 scenario sets, V3 -> V4
   own scenarios 225 -> 232; added: CL-CLEAN-FXANNBLANK-FP, EV-L-OC-TM-READ, HDM-CHAR-FX1F, HDM-CHAR-FXANN, HDM-CHAR-FXDIM, NC-CLONE-REPLACE, Q-I14-SILENT-X, Q-I14-SILENT-Y
   own scenarios removed: Q-I14-SILENT
   dry-run scenarios 145 -> 161; added: WU-DRY-CL-CLEAN-FXANNBLANK-FP, WU-DRY-CL-CLEANUP-CHECK, WU-DRY-NC-CLONE-REPLACE, WU-DRY-PP-POSTCP, WU-DRY-Q-I14-SILENT-X, WU-DRY-Q-I14-SILENT-Y, WU-DRY-Q-I14-X, WU-DRY-Q-I14-Y, WU-DRY-S1-SAVE-REOPEN, WU-DRY-U-1, WU-DRY-U-2, WU-DRY-U-3, WU-DRY-U-4, WU-DRY-U-5, WU-DRY-U-6b, WU-DRY-U-7, WU-DRY-U-8
   dry-run scenarios removed: WU-DRY-EA1-READONLY-OPENS

D. SENSITIVITIES (finite differences at each illustrative row; ILLUSTRATIVE)
D.1 N_B moved by -1 and by +1 with N_A, N_det, n, E, K fixed (N_B > N_A makes every MIXED term follow N_B)
    row1: N_B 29 -> 28: -165 runs ; N_B 29 -> 30: +293 runs
    row2: N_B 14 -> 13: -165 runs ; N_B 14 -> 15: +293 runs
    row3: N_B 7 -> 6: -165 runs ; N_B 7 -> 8: +293 runs
    row4: N_B 4 -> 3: -165 runs ; N_B 4 -> 5: +293 runs
D.2 EVIDENCE_REPETITION 1 -> 2 (everything else fixed)
    row1: +36 runs
    row2: +36 runs
    row3: +36 runs
    row4: +36 runs
D.3 K + 1: one more PARAM-03 sample member (a new governing CL-CLEAN-<fixture> scenario, mixed class) with its dry-run scenario
    row1: +58 runs (the member itself 29, its dry runs 29)
    row2: +28 runs (the member itself 14, its dry runs 14)
    row3: +14 runs (the member itself 7, its dry runs 7)
    row4: +8 runs (the member itself 4, its dry runs 4)
    example where n dominates (row1 with u = 0.01, n = 299): K 10 -> 11: +48 runs
D.4 u and c: they enter only through ceil(n / K); TOTAL changes only when ceil(n / K) > N_strict of the sample members.
    u* = 1 - (1 - c)^(1 / (K * N_strict)) is the value below which that happens (then only the CLEAN term grows)
    row1: u* = 0.0103 ; u halved (0.025): n 59 -> 119, +0 runs ; 1 - c halved (c = 0.975): n 59 -> 72, +0 runs
    row2: u* = 0.0212 ; u halved (0.050): n 29 -> 59, +0 runs ; 1 - c halved (c = 0.975): n 29 -> 36, +0 runs
    row3: u* = 0.0324 ; u halved (0.050): n 22 -> 45, +0 runs ; 1 - c halved (c = 0.950): n 22 -> 29, +0 runs
    row4: u* = 0.0559 ; u halved (0.100): n 11 -> 22, +0 runs ; 1 - c halved (c = 0.950): n 11 -> 14, +0 runs
    example below u* (row1 with u = 0.01): n = 299, ceil(n / K) = 30 > N = 29: +10 runs, all in the CLEAN term
D.5 alpha halved and p halved (for both classes and for the determinism pair at once)
    row1: alpha 0.050 -> 0.0250: N 29 -> 36, TOTAL 10097 -> 12512 (+24%) ; p 0.10 -> 0.050: N 29 -> 59, TOTAL 10097 -> 20447 (+103%)
    row2: alpha 0.050 -> 0.0250: N 14 -> 17, TOTAL 4922 -> 5957 (+21%) ; p 0.20 -> 0.100: N 14 -> 29, TOTAL 4922 -> 10097 (+105%)
    row3: alpha 0.100 -> 0.0500: N 7 -> 9, TOTAL 2507 -> 3197 (+28%) ; p 0.30 -> 0.150: N 7 -> 15, TOTAL 2507 -> 5267 (+110%)
    row4: alpha 0.100 -> 0.0500: N 4 -> 5, TOTAL 1472 -> 1817 (+23%) ; p 0.50 -> 0.250: N 4 -> 9, TOTAL 1472 -> 3197 (+117%)
D.6 MAX_ATTEMPTS (PARAM-04 m, per repetition slot): worst case = every slot uses m attempts
    row1: G = 5139 slots: worst-case governing attempts = 5139 * m (each unit of m adds up to 5139 attempts); the 4958 non-governing runs have no slot
    row2: G = 2514 slots: worst-case governing attempts = 2514 * m (each unit of m adds up to 2514 attempts); the 2408 non-governing runs have no slot
    row3: G = 1289 slots: worst-case governing attempts = 1289 * m (each unit of m adds up to 1289 attempts); the 1218 non-governing runs have no slot
    row4: G = 764 slots: worst-case governing attempts = 764 * m (each unit of m adds up to 764 attempts); the 708 non-governing runs have no slot
D.7 HDM-CHAR-* informative runs (outside the intent; no Owner answer moves them): the minimum is 9 per scenario, 27 in all;
    one more informative run per varied key in every HDM-CHAR-* scenario adds +27 runs at every row

E. OPEN ARCHITECT QUESTION AQ-V4-13 OF BA-10 V4 SECTION 6: size of the other readings (no reading is decided here)
E.1 AQ-V4-13 (a) = Q-A1 (contract-assigned groups and the strictest pair)
   scenarios with CONTRACT_ASSIGNED_GROUPS: 17; class composition changed by the other reading: 14
    PR-CLOSURE-COMPLETENESS: NONE -> A_ONLY
    WR-A-X7: A_ONLY -> MIXED
    WR-A-X8: A_ONLY -> MIXED
    WR-D-X7: A_ONLY -> MIXED
    WR-D-X8: A_ONLY -> MIXED
    WR-E-X7: A_ONLY -> MIXED
    WR-E-X8: A_ONLY -> MIXED
    WR-F-X7: A_ONLY -> MIXED
    WR-F-X8: A_ONLY -> MIXED
    WR-G-X7: A_ONLY -> MIXED
    WR-G-X8: A_ONLY -> MIXED
    WR-B-X0: A_ONLY -> MIXED
    WR-B-X7: A_ONLY -> MIXED
    WR-C-X7: A_ONLY -> MIXED
   unchanged (already of the same composition): KS-SL1-POSTWRITE, KS-SL1-TX-MISMATCH, KS-SL4-NEG-DET
    row1: +28 runs with N_B = N_A ; +41 runs with N_B = N_A + 1
    row2: +13 runs with N_B = N_A ; +26 runs with N_B = N_A + 1
    row3: +6 runs with N_B = N_A ; +19 runs with N_B = N_A + 1
    row4: +3 runs with N_B = N_A ; +16 runs with N_B = N_A + 1
E.2 AQ-V4-13 (b) (the CHAR_RUNS intent), part (ii): N_A per key configuration of HDM-4 instead of N_A at the pinned key values
    only: each HDM-CHAR-* scenario would run (1 + 9) * N_A times instead of N_A + 9
    row1: +756 runs (all non-governing, in D)
    row2: +351 runs (all non-governing, in D)
    row3: +162 runs (all non-governing, in D)
    row4: +81 runs (all non-governing, in D)
    part (i) (own Owner pair for the CHAR runs) has no size without a value; it is not computed

F. RUNS OF THE DRAFT SCENARIOS, BY BLOCKER (counted in the totals of block B as the draft catalog states them; a scenario with
   two blockers is counted under each; the memo does not model any ruling)
   BLOCKED_BY                 scenarios governing     row1     row2     row3     row4
   K-3                              134       134     3690     1785      896      515
   K-8                               32        17      845      410      207      120
   K-6 (AQ-V4-07, Q-HDM-1)            1         1       29       14        7        4
   AQ-V4-01                          25        21      630      315      168      105
   AQ-V4-03                           3         3       87       42       21       12
   E6-C4                              1         1       29       14        7        4
   scenarios per rule under each blocker:
    K-3: CLEAN 10, DEFAULT 111, EVIDENCE 7, EVIDENCE_STRICT 3, VALIDATE 3
    K-8: CLEAN 1, DEFAULT 12, DRY 15, EVIDENCE 3, LEARN 1
    K-6 (AQ-V4-07, Q-HDM-1): CLEAN 1
    AQ-V4-01: DRY 4, EVIDENCE 4, LEARN 17
    AQ-V4-03: DEFAULT 3
    E6-C4: DEFAULT 1
   BA-10 V4 section 2.2: the rule LEARN does not depend on the governance of a learning micro-scenario (only its place in G or D
   does); a dry run exists only for a governing scenario that enters the governed window (AR3-02)

END OF OUTPUT
```

## 5. Reproduction

From the repository root: `python docs/automation/evidence/I-52-ct21d-cost-memo-v4.py` (Python 3, standard library only; it reads the V4 catalog, the V3 catalog for block C, BA-08 V4 for the `HDM-CHAR-*` keys and BA-10 V4 for the check A.7). It stops, printing no figures, if the HDM-2 and HDM-4 rows of BA-08 V4 do not read as expected. The output is deterministic: the generator of this memo ran the script twice and compared the two outputs byte for byte before embedding them. It must be re-run whenever the catalog, BA-08 V4 section 11.2 or BA-10 V4 section 2.2 changes.

## 6. Delta V4 (decisions section 214)

| Finding id | Decision | Fix (section/field) |
|---|---|---|
| BA10-V3-05 | AR3-24 | FIXED: the corrected V3 totals 9377, 4562, 2315, 1352 are recomputed (section 1; section 4 block C); the V4 evaluation splits class-A-only, mixed and class-B-only terms (block A.2; `n_strict`) |
| BA10-V3-06 | AR3-24 | FIXED: real sensitivities for `p`, `alpha`, `u`, `c`, `N_B`, `EVIDENCE_REPETITION`, `K` and `MAX_ATTEMPTS` (section 3; section 4 block D) |
| BA11V3-12 | AR3-24 | FIXED: the 19-run error of the V3 table is shown and recomputed (section 1; block C); the decisions record already carries the corrected figures (section 214.1) |
| CLOSURE BA10-02 (residual 2, totals) | AR3-24 | FIXED: as BA10-V3-05 |
| AR3-24 (memo not asked) | AR3-24 | the header and section 1 state that the memo is not put to the Owner before the V4 review |
| AR3-11 / BA10-V3-08 (critic: the memo counted `HDM-CHAR-*` as `3 * N_A`, which cannot hold the varied-key runs of BA-08 V4 HDM-4) (correction pass) | AR3-11, AR3-24 | FIXED: `HDM-CHAR-*` run `N_A + V` each, `V` = the 9 keys read from BA-08 V4 HDM-2 (one informative run per key, the minimum; outside the intent); the script stops if the rows do not read as expected (script `hdm_keys`; blocks A.6, B, B.1, B.2, D.7; section 3). The other reading is sized in block E.2 |
| AR3-24 / BA10-V3-05 (critic: 128 mixed-class labels read `N_A`) (correction pass) | AR3-24 | FIXED in the catalog by BA-04 V4. Block A.5 no longer counts `N_A` labels: it compares every `REPETITION` label with the BA-10 V4 derivation, exactly (all 393 agree; 0 governing mixed-class labels without `max(N_A, N_B)`) |
| AR3-02 / D-05 and the other catalog corrections (critic; re-run on the corrected BA-04 V4 catalog) (correction pass) | AR3-02, AR3-24 | FIXED: the memo was re-run on the corrected catalog (393 scenarios: 232 own and 161 dry-run scenarios, 4 of them `WU-DRY-Q-I14-*`); the totals (row1 10097), the V3-to-V4 decomposition (new block C.2 lists the scenario sets) and the sensitivities (`N_B`: -165 and +293) are updated (sections 3 and 4) |
| Architect open questions (round V4 ids) (correction pass) | - | APPLIED: block E = AQ-V4-13 (part (a) = Q-A1; part (b)(ii) sized, part (b)(i) not sizable); block F = the draft scenarios by blocker (AQ-V4-01, AQ-V4-03, K-6 = AQ-V4-07, K-3, K-8, E6-C4); header line `ARCHITECT QUESTIONS`; none decided |
