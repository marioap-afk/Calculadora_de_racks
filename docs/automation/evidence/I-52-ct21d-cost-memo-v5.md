# I-52 — CT-21D Cost Memo V5: Runs Implied by the Parameter Sheet (NOT SEALED; ILLUSTRATIVE)

> **NOT SEALED. NOT A BASELINE ARTIFACT. Arithmetic only: every figure is ILLUSTRATIVE; nothing here is a proposal, a choice or a decision. Every parameter of BA-10 V5 stays `UNSET`.**
>
> ```text
> DOCUMENT            = I-52 CT-21D cost memo V5 (support of BA-10 V5 sections 2.3 and 5). It supersedes the V4 memo
>                       (docs/automation/evidence/I-52-ct21d-cost-memo-v4.md, blob dd85433f19868f601813a27b6889d6b47a841d04, and .py, blob
>                       5359a4a82937edf636f525fe2e527f0bfa399adc), which stays unchanged as history
> STATUS              = NOT SEALED; not a BA-11 registry entry; not part of the bytes of BA-10 V5 or BA-04 V5; no artifact depends on it
> OWNER               = NOT ASKED. Once BA-10 V5 and this memo are reviewed, the memo goes with Q-O1..Q-O5 in the Owner decision
>                       package (decisions section 217, AR4-21); the Coordinator's order of section 218 places that package after
>                       the candidate rulings, and it needs explicit authorization
> SOURCE              = docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v5.json (draft; every count changes with the
>                       catalog); the closed key list of docs/initiatives/I-52-ct21d-baseline-ba-08-selective-kind-baseline-v5.md
>                       section 11.2 (rows HDM-2 and HDM-4; draft), against which the keys each `HDM-CHAR-*` scenario declares are
>                       checked; docs/initiatives/I-52-ct21d-baseline-ba-10-parameter-sheet-v5.md, whose rules the script checks
>                       (block A.7); for block C, the V4 catalog and the V4 memo. The output cites the SHA-256 of each file
> SCRIPT              = docs/automation/evidence/I-52-ct21d-cost-memo-v5.py (SHA-256 f7b08ff5ee84adce006cbde161685a3fe74f3365bb58edd0c4ca6950bc268c39 over its LF-normalized bytes,
>                       computed with Python hashlib); its complete output is section 4, verbatim
> DELTA RULING        = decisions section 217: AR4-21 (the attribution of EVIDENCE_REPETITION = 1, finding R1:BA10-V4-03; the Owner
>                       package), AR4-14 ((a) N_strict over GROUPS_SERVED only; (b)(i) the derived CHAR_RUNS; (b)(ii) HDM-CHAR-*:
>                       N_A runs at the pinned key configuration plus exactly one informative run per varied key), AR4-06 (the
>                       qualification twins and the build under test in the counts), AR4-01 (EV-L-*, EV-L-COMPOSED, Q-I14-*),
>                       AR4-07 (the fourth HDM-CHAR-*); the LEARN rule of BA-10 V5 (R1/R2:BA10-V4-01); the correction pass of
>                       round V5 (section 6, rows "correction pass") and its re-check (row "correction pass (final)")
> ARCHITECT QUESTIONS = AQ-V4-13 is ruled by AR4-14, and AQ-V4-01, AQ-V4-03, AQ-V4-04 and AQ-V4-07, which moved the V4 counts,
>                       by AR4-01, AR4-03, AR4-04 and AR4-07 (section 217); block E checks the rulings and sizes no other reading.
>                       OPEN questions of round V5 that touch the counts of this memo (none is modeled): AQ-V5-06 (the
>                       composition-check count N_det + 1, BA-10 V5 section 6.1; blocks A.7 and B.2), AQ-V5-02 (twins of
>                       PR-CLOSURE-COMPLETENESS and WU-BOUNDARY; block F.4), AQ-V5-01 (the run prerequisites of the
>                       characterization-build runs that consume EVM_FROZEN; block F.5), AQ-V5-08 (the admission of the
>                       vehicle Q-I14-P and of WU-DRY-Q-I14-P, which carry it in BLOCKED_BY; block F.1). AQ-V5-03, AQ-V5-04,
>                       AQ-V5-05 and AQ-V5-07 move no count that this memo shows
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 1. Purpose

BA-10 V5 is symbolic (AR3-24, AR3-25): it defines the repetition rules and the run expression of its section 2.3 and holds no count of the scenario catalog. This memo evaluates that expression on the **draft** BA-04 V5 catalog (425 scenarios, after the catalog's correction pass), for the illustrative choices that BA-10 V3 section 2.3 used and the V4 memo kept, and gives the sensitivities the Owner will need. It is decision support to be reviewed, not an input of any value.

The V5 catalog adds the per-build qualification twins, the fourth `HOST_DEFAULT_MAP` characterization scenario and, in its correction pass, the qualification vehicle `Q-I14-P` with its dry run, and places each scenario on its build under test (AR4-06, AR4-07, AR4-01). This memo counts them, splits the runs by build, and reconciles its totals with the V4 memo (block C), whose figures it reproduces exactly from the V4 catalog. The V3 correction (9358 -> 9377) stays in the V4 memo and in decisions section 214.1.

## 2. Illustrative choices (ILLUSTRATIVE; not decisions)

| Row | `p` | `alpha` | `u` | `c` | other assumptions (all rows) |
|---|---|---|---|---|---|
| row1 | 0.10 | 0.05 | 0.05 | 0.95 | one pair for both classes (`p_A = p_B = p`, `alpha_A = alpha_B = alpha`); `p_det = p`, `alpha_det = alpha`; `EVIDENCE_REPETITION = 1`: the implementer's draft proposal in BA-10 (since V4) for the Coordinator value; not a value and not a Coordinator position (AR4-21); `CHAR_RUNS` as BA-10 V5 section 2.2 states it (`N_A` for the pin- and selection-feeding runs; for `HDM-CHAR-*`, `N_A` runs at the pinned key configuration plus exactly one informative run per varied key, with the varied keys read from each scenario and checked against BA-08 V5 HDM-2; `N_det + 1` for the composition check `EV-L-COMPOSED`; `EVIDENCE_REPETITION` for the exploratory runs); `K` as read from the draft catalog |
| row2 | 0.20 | 0.05 | 0.10 | 0.95 | same |
| row3 | 0.30 | 0.10 | 0.10 | 0.90 | same |
| row4 | 0.50 | 0.10 | 0.20 | 0.90 | same |

These are the four rows of BA-10 V3 section 2.3, kept so that the versions can be compared. They are not a range the Owner is asked to choose from.

## 3. Reading the output (figures of section 4)

- **Totals (block B).** Row1 gives 10390 runs: 5334 planned governing runs (the repetition slots `G`) and 5056 non-governing runs (`D`: 4698 dry runs and 358 non-governing characterization runs). Rows 2 to 4 give 5080, 2602 and 1540. The 12 `SYNTH` runs of each row are offline evaluations; every other run is a fresh AutoCAD process on the build under test of its scenario.
- **By build (blocks A.8 and B.3; AR4-06).** At row1 the `PRODUCT_BUILD` carries 1494 governing and 31 non-governing runs, and the `CHARACTERIZATION_BUILD` 3828 governing and 5025 non-governing runs: all 162 dry-run scenarios, the pin- and selection-feeding runs and the `HDM-CHAR-*` runs are on it. The 12 offline runs have no build.
- **Qualification twins (blocks A.9 and B.3; AR4-06 (3)).** Every `Q-*` and `E6-C*` scenario outside the I-14 family has a twin `<id>-CB` on the characterization build with the same `REPETITION` label: 29 twins (21 `EVIDENCE`, 7 `DEFAULT`, 1 exploratory), 225 runs at row1 (120, 71 and 50 at rows 2 to 4). The I-14 family, now six scenarios (the catalog's correction pass added the vehicle `Q-I14-P`), runs on the characterization build only.
- **V4 to V5 (block C; row1: +293).** The V4 block B rows are reproduced exactly from the V4 catalog. The difference is the 29 twins (+225), `HDM-CHAR-FXANNBLANK` (+38, that is `N_A + 9`, AR4-07), and the vehicle `Q-I14-P` with its dry run `WU-DRY-Q-I14-P` (+1 and +29; added by the catalog's correction pass for the PREPARE-phase coordinate of `PR-REC-REOBS-MISMATCH`, its admission part of the Architect's delta review of the catalog). Three labels change without changing a count at these rows: `EV-L-COMPOSED` becomes a non-governing composition check (AR4-01; its `N_det + 1` runs move from `G` to `D`), `NC-E12D` now serves only class-B groups (`DEFAULT: N_B`, equal to `max(N_A, N_B)` while `N_B = N_A`), and the three V4 `HDM-CHAR-*` labels read "exactly one" informative run per varied key (the V4 memo already counted one per key). Hence `G` +195 and `D` +98 at row1.
- **`N_B` (block D.1).** Below `N_A`, each unit of `N_B` moves 167 runs (162 dry-run scenarios, 4 class-B-only `DEFAULT` scenarios, 1 class-B-only `EVIDENCE_STRICT` scenario). Above `N_A`, each unit moves 294 runs, because the 127 governing mixed-class scenarios then follow `N_B`. The dry runs are the largest single term.
- **`EVIDENCE_REPETITION` (block D.2).** Each unit adds 59 runs (57 `EVIDENCE` scenarios, the 21 twins and the vehicle `Q-I14-P` included, and the 2 exploratory runs), while it stays below the floor of the `EVIDENCE_STRICT` scenarios.
- **`K` (block D.3).** At the four rows `N` dominates `ceil(n / K)`, so one more sample member adds its own `N_strict` runs and its `N_B` dry runs (+58 at row1). Where `n` dominates (row1 with `u = 0.01`) one more member adds 48 runs.
- **`u` and `c` (block D.4).** No effect at the four rows: halving `u` or `1 - c` leaves every total unchanged. They act only below `u*` (0.0103, 0.0212, 0.0324 and 0.0559 for the four rows), and then only the `CLEAN` term grows (row1 with `u = 0.01`: +10 runs).
- **`alpha` and `p` (block D.5).** `alpha` acts logarithmically: halving it adds 21% to 27%. `p` acts roughly inversely: halving it adds 102% to 115%.
- **`MAX_ATTEMPTS` (block D.6).** `m` is per repetition slot. In the worst case every slot uses `m` attempts: `5334 * m` governing attempts at row1, so each unit of `m` adds up to 5334 attempts. The non-governing runs have no slot.
- **`N_det` alone (block D.8, Q-O3).** Each unit adds 20 runs at the four rows: 16 learning micro-scenarios, the composition check (its `N_det + 1` count is BA-10 V5's reading, open as AQ-V5-06), and the 3 validation scenarios, because at these rows `N_det` equals `N_strict` and `max(N_strict, N_det)` then follows `N_det`.
- **`HDM-CHAR-*` informative runs (blocks A.6, D.7 and E.3).** The four `HDM-CHAR-*` scenarios each vary the 9 keys of BA-08 V5 HDM-2 (the 7 context variables `CLAYER`, `CECOLOR`, `CELTYPE`, `CELTSCALE`, `CELWEIGHT`, `CETRANSPARENCY`, `CPLOTSTYLE`, and the current text style and dimension style), each key exactly once: 36 informative runs in all, outside the intent. AR4-14 (b)(ii) makes this count determinate: no Owner answer moves it and no other authority adds runs.
- **Rulings (block E).** 17 scenarios carry `CONTRACT_ASSIGNED_GROUPS`, and each label resolves `N_strict` from `GROUPS_SERVED` only (AR4-14 (a)): `PR-CLOSURE-COMPLETENESS` stays under `EVIDENCE`, the X7 and residual cells read `N_A`, and the three `KS-*` are mixed-class through their own groups. The Q-O1 pair also sizes 290 non-governing runs at row1 (the 6 pin- and selection-feeding scenarios and the pinned-configuration runs of the 4 `HDM-CHAR-*`), with no pair of their own (AR4-14 (b)(i)). The 16 learning micro-scenarios are governing on the product build, and no learning micro-scenario serves a class-A or class-B group (the catalog's rule "Enforced, not exercised" keeps the controls they enforce out of `GROUPS_SERVED`), so the `LEARN` floor of BA-10 V5 adds nothing today (block A.2). The six-member I-14 family has 5 dry-run scenarios (block E.4).
- **Draft scenarios (block F).** The totals count the draft scenarios as the catalog states them; the memo models no ruling. At row1: K-3, 134 scenarios, 3690 runs; K-8, 34 scenarios (18 governing), 875 runs; the 160 `DRAFT` scenarios, 4361 runs. The 48 `PRODUCT_BUILD` scenarios that consume a pin derived on the characterization build need the `BUILD_EQUIVALENCE_RECORD` (1241 runs at row1), and the 17 `EV-L-*` scenarios the `REVIEWED_RECORD` (510 runs). AQ-V5-08, the open Architect item on the admission of the vehicle `Q-I14-P` that the catalog carries in `BLOCKED_BY` (after K-8) for `Q-I14-P` and `WU-DRY-Q-I14-P` only, covers 2 scenarios (1 governing) and 30 runs at row1; both are also under K-8, so the `DRAFT` count does not move. These counts move with K-3, K-8 and AQ-V5-08, the blockers of the V5 catalog.
- **Open questions of round V5 (blocks A.7, B.2, F.1, F.4 and F.5).** None is modeled; the memo counts what each touches. AQ-V5-02: an affirmative ruling would add the twins `PR-CLOSURE-COMPLETENESS-CB` and `WU-BOUNDARY-CB`, +30 runs at row1 (+1 and +29; +15, +8 and +5 at rows 2 to 4; neither source has a dry run). AQ-V5-01: 111 governing characterization-build scenarios consume `EVM_FROZEN` (3191 runs at row1); an affirmative ruling adds the `BUILD_EQUIVALENCE_RECORD` to their run prerequisites and moves no run, and the effect of any other answer is the Architect's to state. AQ-V5-06: the composition-check count `N_det + 1` (30 runs at row1) is BA-10 V5's reading, which A.7 checks is recorded as open; question Q-V5-BA04-3 of BA-04 V5 is closed by a pointer to BA-10 V5 section 2.2. AQ-V5-08: the vehicle `Q-I14-P` and its dry run carry it (block F.1: 2 scenarios, 30 runs at row1); a ruling that admits the vehicle moves no run, and the effect of any other answer is the Architect's to state. AQ-V5-03, AQ-V5-04, AQ-V5-05 and AQ-V5-07 move no count that the memo shows (a ruling that changed the set of learned operation classes would change the `EV-L-OC-*` set, as BA-04 V5 section 8.2 notes). AQ-V4-01, AQ-V4-03 and AQ-V4-07, which moved the V4 counts, are ruled.
- **Catalog labels (blocks A.1 and A.5).** Every `REPETITION` label of the catalog equals the BA-10 V5 derivation exactly (all 425), with `N_strict` resolved as `N_A`, `N_B` or `max(N_A, N_B)` and the `CHAR_RUNS` kind resolved. The 127 governing mixed-class scenarios all read `max(N_A, N_B)`. Block A.7 checks that BA-10 V5 states each rule the memo applies and records the composition-check count as the open question AQ-V5-06.

## 4. Script output (verbatim)

```text
I-52 CT-21D COST MEMO V5 - ARITHMETIC OUTPUT (ILLUSTRATIVE; NOT A PROPOSAL, NOT A CHOICE, NOT A DECISION)
catalog file    : docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v5.json
catalog SHA-256 : 68c0518c62ccdb43b793ddd7dbd29a056e20c29f0adde614f7bbaa84c0124af0 (LF-normalized bytes)
catalog version : 5-DRAFT ; scenarios: 425
key list file   : docs/initiatives/I-52-ct21d-baseline-ba-08-selective-kind-baseline-v5.md (section 11.2, rows HDM-2 and HDM-4)
key list hash   : 9cd61e640c07845e2a024ba2222dfe3b5c87c9dd5de5bea81fcd90dcce442964 (SHA-256, LF-normalized bytes)
rule sheet file : docs/initiatives/I-52-ct21d-baseline-ba-10-parameter-sheet-v5.md
rule sheet hash : 96b3aceb846427d9c00b0c06275e54e8df3feaa5aaa00d2d5136e29200b24275 (SHA-256, LF-normalized bytes)

A. COUNTS (read from the draft catalog; they change with the catalog)
A.1 rule derived from the scenario properties (BA-10 V5 section 2.2) vs the REPETITION field of the catalog: all 425 agree
A.2 scenarios per rule and class composition (classes of the class-A/B CONTROL groups of GROUPS_SERVED; KR, evidence
    groups and CONTRACT_ASSIGNED_GROUPS excluded, AR4-14 (a))
    rule              total  A_ONLY  MIXED  B_ONLY   NONE
    DEFAULT             149      32    113       4      0
    CLEAN                10       0     10       0      0
    VALIDATE              3       0      3       0      0
    LEARN                16       0      0       0     16
    EVIDENCE             57       0      0       0     57
    EVIDENCE_STRICT       3       1      1       1      0
    SYNTH                12       0      0       0     12
    DRY                 162       0      0     162      0
    CHAR                 13       8      0       0      5
    ALL                 425
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
A.5 exact REPETITION label (N_strict resolved to N_A, N_B or max(N_A, N_B); CHAR_RUNS kind resolved) vs the catalog: all 425 agree
    governing mixed-class scenarios: 127; of them with a label without max(N_A, N_B): 0
A.6 HOST_DEFAULT_MAP keys (BA-08 V5 section 11.2 HDM-2, closed list): CLAYER, CECOLOR, CELTYPE, CELTSCALE, CELWEIGHT, CETRANSPARENCY, CPLOTSTYLE, current text style, current dimension style = 9 keys
    BA-08 V5 HDM-4: N_A runs at the pinned key configuration plus exactly one informative run per varied key (AR4-14 (b)(ii))
    V(s) read from the declared key-change injections of each scenario (each key once, from the closed list, V(s) stated):
    HDM-CHAR-FX1F          V(s) = 9 ; keys of the list it does not vary: 0
    HDM-CHAR-FXANN         V(s) = 9 ; keys of the list it does not vary: 0
    HDM-CHAR-FXDIM         V(s) = 9 ; keys of the list it does not vary: 0
    HDM-CHAR-FXANNBLANK    V(s) = 9 ; keys of the list it does not vary: 0
    informative runs in all: 36 (outside the intent; no Owner answer moves them)
A.7 BA-10 V5 states the rules this memo applies:
    HDM-CHAR-* rule (AR4-14 (b)(ii))                 yes
    EV-L-COMPOSED rule (AR4-01)                      yes
    LEARN rule (R1/R2:BA10-V4-01)                    yes
    N_strict over GROUPS_SERVED only (AR4-14 (a))    yes
    the V4 wording "at least one informative run" is absent: yes
    the composition-check count is recorded as the open question AQ-V5-06 (BA-10 V5 section 6.1): yes
    rules of its section 2.2 table: DRY, LEARN, CHAR, CLEAN, SYNTH, VALIDATE, EVIDENCE, EVIDENCE_STRICT, DEFAULT
A.8 scenarios per build under test (AR4-06) and governance
    BUILD_UNDER_TEST             total governing  non-gov.   explor.
    PRODUCT_BUILD                   80        78         1         1
    CHARACTERIZATION_BUILD         333       160       172         1
    NONE                            12        12         0         0
A.9 per-build qualification (AR4-06 (3)): 29 twins <id>-CB on the CHARACTERIZATION_BUILD; each with its PRODUCT_BUILD scenario
    and the same REPETITION label: yes; Q-*/E6-C* scenarios outside the I-14 family without a twin: none
    twins per rule: CHAR 1, DEFAULT 7, EVIDENCE 21
    the I-14 family (CHARACTERIZATION_BUILD only, no twin): Q-I14, Q-I14-Y, Q-I14-X, Q-I14-SILENT-Y, Q-I14-SILENT-X, Q-I14-P; builds: CHARACTERIZATION_BUILD

B. ILLUSTRATIVE TOTALS FOR THE CHOICES USED SINCE BA-10 V3 SECTION 2.3
   assumptions (ILLUSTRATIVE): one pair for both classes (p_A = p_B = p, alpha_A = alpha_B = alpha), p_det = p, alpha_det = alpha,
   EVIDENCE_REPETITION = 1 (the implementer's draft proposal in BA-10 for the Coordinator value; not a value and not a Coordinator
   position), K = 10; CHAR_RUNS = N_A (pin- or selection-feeding), N_A + V(s) (HDM-CHAR-*: N_A runs at the pinned key
   configuration plus exactly one informative run per varied key), N_det + 1 (composition check), EVIDENCE_REPETITION (exploratory)
   p     alpha  u     c        N    n  N_det ceil(n/K)       G       D   TOTAL
   0.10  0.05   0.05  0.95    29   59     29         6    5334    5056   10390
   0.20  0.05   0.10  0.95    14   29     14         3    2619    2461    5080
   0.30  0.10   0.10  0.90     7   22      7         3    1352    1250    2602
   0.50  0.10   0.20  0.90     4   11      4         2     809     731    1540
   G = governing planned runs (repetition slots); D = non-governing runs (dry runs, CHAR, any non-governing learning run); TOTAL = G + D.
   12 SYNTH runs per row are offline evaluations (BUILD_UNDER_TEST = NONE; no AutoCAD process).

B.1 terms per rule
   rule                 row1     row2     row3     row4
   DEFAULT              4321     2086     1043      596
   CLEAN                 290      140       70       40
   VALIDATE               87       42       21       12
   LEARN                 480      240      128       80
   EVIDENCE               57       57       57       57
   EVIDENCE_STRICT        87       42       21       12
   SYNTH                  12       12       12       12
   DRY                  4698     2268     1134      648
   CHAR                  358      193      116       83
B.2 CHAR term per kind
   PIN_FEEDING           174       84       42       24
   HDM                   152       92       64       52
   COMPOSITION            30       15        8        5
   EXPLORATORY             2        2        2        2
   (HDM: 4 scenarios * N_A plus 36 informative runs, exactly one per varied key; COMPOSITION: N_det + 1)
B.3 runs per build under test (AR4-06): G / D
   BUILD_UNDER_TEST                  row1          row2          row3          row4
   PRODUCT_BUILD                1494 / 31      744 / 16       394 / 9       244 / 6
   CHARACTERIZATION_BUILD     3828 / 5025   1863 / 2445    946 / 1241     553 / 725
   NONE                            12 / 0        12 / 0        12 / 0        12 / 0
   of which the 29 qualification twins (all on the CHARACTERIZATION_BUILD): row1 225 ; row2 120 ; row3 71 ; row4 50
   dry-run scenarios per build: CHARACTERIZATION_BUILD 162

C. RECONCILIATION WITH THE V4 MEMO (the V4 catalog evaluated through its own REPETITION labels)
   V4 catalog SHA-256 (LF-normalized): 4f8e4ffdc1ed6c4cc4e237666277ce71733c9f7ceab426a27e6585120f2ced84 ; scenarios: 393 ; K = 10 ; HDM-CHAR informative runs counted by the V4 memo: 9 per scenario
   V4 memo file: docs/automation/evidence/I-52-ct21d-cost-memo-v4.md ; SHA-256 (LF-normalized): d9e602d2abead79ca44c3c99561ab67a75664fea6a7512c6f539aeaf49c48781
   V4 block B rows reproduced exactly from the V4 catalog: yes
C.1 V5 minus V4 per row
   row         V4       V5    delta        G        D
   row1     10097    10390     +293     +195      +98
   row2      4922     5080     +158     +105      +53
   row3      2507     2602      +95      +63      +32
   row4      1472     1540      +68      +45      +23
   by cause (each scenario charged once; the four columns add up to the delta):
   row      twins (AR4-06 (3))           other added         changed label               removed
   row1                   +225                   +68                    +0                    +0
   row2                   +120                   +38                    +0                    +0
   row3                    +71                   +24                    +0                    +0
   row4                    +50                   +18                    +0                    +0
C.2 scenario sets, V4 -> V5
   scenarios 393 -> 425; added 32: E6-C1-CB, E6-C2-CB, E6-C3-CB, E6-C4-CB, E6-C5-CB, E6-C6-CB, E6-C7-CB, E6-C8-CB, HDM-CHAR-FXANNBLANK, Q-FPSPEC-VECTORS-CB, Q-I01-CB, Q-I02-CB, Q-I03-CB, Q-I04-CB, Q-I05-CB, Q-I06-CB, Q-I07-CB, Q-I08-CB, Q-I09-CB, Q-I09-LOSS-CB, Q-I10-CB, Q-I10-LOSS-CB, Q-I11-CB, Q-I12-CB, Q-I13-CB, Q-I14-P, Q-I15-CB, Q-I16-CB, Q-I16-CLEANUP-NEG-CB, Q-ROUTE-R1-CB, Q-SIGNED-ZERO-CB, WU-DRY-Q-I14-P
   removed: none
   changed label: EV-L-COMPOSED: LEARN: PARAM-02 -> CHAR: CHAR_RUNS = PARAM-02 (non-governing composition check)
   changed label: HDM-CHAR-FX1F: CHAR: CHAR_RUNS = N_A at the pinned key values, plus at least one informative run per varied key (outside the intent) -> CHAR: CHAR_RUNS = N_A at the pinned key values, plus exactly one informative run per varied key (outside the intent)
   changed label: HDM-CHAR-FXANN: CHAR: CHAR_RUNS = N_A at the pinned key values, plus at least one informative run per varied key (outside the intent) -> CHAR: CHAR_RUNS = N_A at the pinned key values, plus exactly one informative run per varied key (outside the intent)
   changed label: HDM-CHAR-FXDIM: CHAR: CHAR_RUNS = N_A at the pinned key values, plus at least one informative run per varied key (outside the intent) -> CHAR: CHAR_RUNS = N_A at the pinned key values, plus exactly one informative run per varied key (outside the intent)
   changed label: NC-E12D: DEFAULT: max(N_A, N_B) -> DEFAULT: N_B
   changed governance: EV-L-COMPOSED: GOVERNING -> NON_GOVERNING_BY_DESIGN
   changed build under test: 19: EA2-DEFERRED-EVENTS (PRODUCT_BUILD -> CHARACTERIZATION_BUILD), EV-L-COMPOSED (CHARACTERIZATION_BUILD -> PRODUCT_BUILD), EV-L-OC-BT-OPEN (CHARACTERIZATION_BUILD -> PRODUCT_BUILD), EV-L-OC-BTR-NESTED (CHARACTERIZATION_BUILD -> PRODUCT_BUILD), EV-L-OC-BTR-VIEW (CHARACTERIZATION_BUILD -> PRODUCT_BUILD), EV-L-OC-DIM (CHARACTERIZATION_BUILD -> PRODUCT_BUILD), EV-L-OC-DYN (CHARACTERIZATION_BUILD -> PRODUCT_BUILD), EV-L-OC-ENVELOPE (CHARACTERIZATION_BUILD -> PRODUCT_BUILD), EV-L-OC-IMPORT (CHARACTERIZATION_BUILD -> PRODUCT_BUILD), EV-L-OC-LAYER (CHARACTERIZATION_BUILD -> PRODUCT_BUILD), EV-L-OC-LAYER-PRESENT (CHARACTERIZATION_BUILD -> PRODUCT_BUILD), EV-L-OC-REF-ARRAY (CHARACTERIZATION_BUILD -> PRODUCT_BUILD), EV-L-OC-REF-PIECE (CHARACTERIZATION_BUILD -> PRODUCT_BUILD), EV-L-OC-REF-TOP (CHARACTERIZATION_BUILD -> PRODUCT_BUILD), EV-L-OC-TEXT (CHARACTERIZATION_BUILD -> PRODUCT_BUILD), EV-L-OC-TM (CHARACTERIZATION_BUILD -> PRODUCT_BUILD), EV-L-OC-TM-READ (CHARACTERIZATION_BUILD -> PRODUCT_BUILD), EV-L-OC-VERIFY-READ (CHARACTERIZATION_BUILD -> PRODUCT_BUILD), Q-I14 (PRODUCT_BUILD -> CHARACTERIZATION_BUILD)

D. SENSITIVITIES (finite differences at each illustrative row; ILLUSTRATIVE)
D.1 N_B moved by -1 and by +1 with N_A, N_det, n, E, K fixed (N_B > N_A makes every MIXED term follow N_B)
    row1: N_B 29 -> 28: -167 runs ; N_B 29 -> 30: +294 runs
    row2: N_B 14 -> 13: -167 runs ; N_B 14 -> 15: +294 runs
    row3: N_B 7 -> 6: -167 runs ; N_B 7 -> 8: +294 runs
    row4: N_B 4 -> 3: -167 runs ; N_B 4 -> 5: +294 runs
D.2 EVIDENCE_REPETITION 1 -> 2 (everything else fixed)
    row1: +59 runs
    row2: +59 runs
    row3: +59 runs
    row4: +59 runs
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
    row1: alpha 0.050 -> 0.0250: N 29 -> 36, TOTAL 10390 -> 12868 (+24%) ; p 0.10 -> 0.050: N 29 -> 59, TOTAL 10390 -> 21010 (+102%)
    row2: alpha 0.050 -> 0.0250: N 14 -> 17, TOTAL 5080 -> 6142 (+21%) ; p 0.20 -> 0.100: N 14 -> 29, TOTAL 5080 -> 10390 (+105%)
    row3: alpha 0.100 -> 0.0500: N 7 -> 9, TOTAL 2602 -> 3310 (+27%) ; p 0.30 -> 0.150: N 7 -> 15, TOTAL 2602 -> 5434 (+109%)
    row4: alpha 0.100 -> 0.0500: N 4 -> 5, TOTAL 1540 -> 1894 (+23%) ; p 0.50 -> 0.250: N 4 -> 9, TOTAL 1540 -> 3310 (+115%)
D.6 MAX_ATTEMPTS (PARAM-04 m, per repetition slot): worst case = every slot uses m attempts
    row1: G = 5334 slots: worst-case governing attempts = 5334 * m (each unit of m adds up to 5334 attempts); the 5056 non-governing runs have no slot
    row2: G = 2619 slots: worst-case governing attempts = 2619 * m (each unit of m adds up to 2619 attempts); the 2461 non-governing runs have no slot
    row3: G = 1352 slots: worst-case governing attempts = 1352 * m (each unit of m adds up to 1352 attempts); the 1250 non-governing runs have no slot
    row4: G = 809 slots: worst-case governing attempts = 809 * m (each unit of m adds up to 809 attempts); the 731 non-governing runs have no slot
D.7 HDM-CHAR-* informative runs: exactly one per varied key (AR4-14 (b)(ii)): 36 in all, the same at every row; no Owner
    answer moves them and no other authority adds runs
D.8 N_det moved by +1 with N_A, N_B, n, E, K fixed (Q-O3 alone)
    row1: N_det 29 -> 30: +20 runs (LEARN +16, composition check +1, VALIDATE +3: max(N_strict, N_det) follows N_det once
           it exceeds N_strict = 29)
    row2: N_det 14 -> 15: +20 runs (LEARN +16, composition check +1, VALIDATE +3: max(N_strict, N_det) follows N_det once
           it exceeds N_strict = 14)
    row3: N_det 7 -> 8: +20 runs (LEARN +16, composition check +1, VALIDATE +3: max(N_strict, N_det) follows N_det once
           it exceeds N_strict = 7)
    row4: N_det 4 -> 5: +20 runs (LEARN +16, composition check +1, VALIDATE +3: max(N_strict, N_det) follows N_det once
           it exceeds N_strict = 4)

E. RULINGS OF DECISIONS SECTION 217 APPLIED BY BA-10 V5 (checks; no other reading is sized)
E.1 AR4-14 (a): CONTRACT_ASSIGNED_GROUPS never enter N_strict. Scenarios that carry them: 17; each label resolves N_strict from
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
E.2 AR4-14 (b)(i): CHAR_RUNS = the derived N_c of the class that consumes the pin or the selection (today N_A); no Owner pair of
    its own; the Q-O1 pair (p_A, alpha_A) sizes 6 pin- or selection-feeding scenarios and the pinned-configuration runs of the 4
    HDM-CHAR-* scenarios: row1 290 runs, row2 140, row3 70, row4 40
E.3 AR4-14 (b)(ii) and AR4-07: 4 HDM-CHAR-* scenarios, CHAR_RUNS(s) = N_A + V(s) with V(s) = 9, 9, 9, 9; informative runs 36 in all
E.4 AR4-01: 16 governing learning micro-scenarios (rule LEARN, N_det + 1 each, on the PRODUCT_BUILD); EV-L-COMPOSED non-governing
    (CHAR, N_det + 1); the Q-I14 family on the CHARACTERIZATION_BUILD with 5 dry-run scenarios

F. RUNS OF THE DRAFT SCENARIOS (counted in the totals of block B as the draft catalog states them; a scenario with two
   blockers is counted under each; the memo does not model any ruling)
F.1 by blocker
   BLOCKED_BY                 scenarios governing     row1     row2     row3     row4
   K-3                              134       134     3690     1785      896      515
   K-8                               34        18      875      425      215      125
   AQ-V5-08                           2         1       30       15        8        5
   scenarios per rule under each blocker:
    K-3: CLEAN 10, DEFAULT 111, EVIDENCE 7, EVIDENCE_STRICT 3, VALIDATE 3
    K-8: CLEAN 1, DEFAULT 12, DRY 16, EVIDENCE 4, LEARN 1
    AQ-V5-08: DRY 1, EVIDENCE 1
F.2 by status
   STATUS                         scenarios     row1     row2     row3     row4
   DRAFT                                160     4361     2111     1061      611
   EXPLORATORY_NON_GOVERNING              2        2        2        2        2
   SEALED_CANDIDATE                      69      461      251      153      111
   SEALED_PENDING_PIN_CANDIDATE         194     5566     2716     1386      816
F.3 by run prerequisite (a scenario with two prerequisites is counted under each)
   RUN_PREREQUISITES                      scenarios     row1     row2     row3     row4
   BUILD_EQUIVALENCE_RECORD                      48     1241      611      317      191
   QUALIFICATION_RECORD@Q-I14-P                   1       29       14        7        4
   QUALIFICATION_RECORD@Q-I14-SILENT-X            7      203       98       49       28
   QUALIFICATION_RECORD@Q-I14-SILENT-Y            3       87       42       21       12
   QUALIFICATION_RECORD@Q-I14-X                  87     2523     1218      609      348
   QUALIFICATION_RECORD@Q-I14-Y                   5      145       70       35       20
   REVIEWED_RECORD                               17      510      255      136       85
F.4 open question AQ-V5-02 (BA-04 V5 section 8.2 and BA-09 V5; recorded in BA-04 V5 as Q-V5-BA04-2): per-build qualification
    of the qualification scenarios whose id is neither Q-* nor E6-C*. An affirmative ruling would add a twin <id>-CB on the
    CHARACTERIZATION_BUILD for each, with the rule of its source (as for the twins of A.9); the memo does not model the ruling
    PR-CLOSURE-COMPLETENESS  build PRODUCT_BUILD, rule EVIDENCE, REACH NONE; its source has a dry-run scenario: no; the twin itself: row1 +1 ; row2 +1 ; row3 +1 ; row4 +1
    WU-BOUNDARY              build PRODUCT_BUILD, rule DEFAULT, REACH NONE; its source has a dry-run scenario: no; the twin itself: row1 +29 ; row2 +14 ; row3 +7 ; row4 +4
    missing from the catalog: none
    both twins: row1 +30 ; row2 +15 ; row3 +8 ; row4 +5 runs
F.5 open question AQ-V5-01 (BA-04 V5 section 8.2; recorded in BA-04 V5 as Q-V5-BA04-1): governing CHARACTERIZATION_BUILD
    scenarios whose PIN_SLOTS name EVM_FROZEN (learned and validated on the PRODUCT_BUILD, AR4-01): 111; per rule: CLEAN 1, DEFAULT 106, EVIDENCE 1, EVIDENCE_STRICT 3
    of them naming BUILD_EQUIVALENCE_RECORD in RUN_PREREQUISITES: 0; their runs: row1 3191 ; row2 1541 ; row3 771 ; row4 441
    an affirmative ruling adds the BUILD_EQUIVALENCE_RECORD to their RUN_PREREQUISITES and moves no run count; the effect of any
    other answer is the Architect's to state and is not modeled

END OF OUTPUT
```

## 5. Reproduction

From the repository root: `python docs/automation/evidence/I-52-ct21d-cost-memo-v5.py` (Python 3, standard library only; it reads the V5 catalog, BA-08 V5 for the key list, BA-10 V5 for the check A.7 (the rules and the record of AQ-V5-06), and the V4 catalog, BA-08 V4 and the V4 memo for block C). It stops, printing no figures, if the HDM-2 and HDM-4 rows of BA-08 V5 or V4 do not read as expected, or if an `HDM-CHAR-*` scenario declares a key change that is not a key of the list, varies a key twice or does not state its `V(s)`. The output is deterministic: the generator of this memo ran the script twice and compared the two outputs byte for byte before embedding them, and it checks that every figure quoted in section 3 is in the output and that no check printed NO. It must be re-run whenever the V5 catalog, BA-08 V5 section 11.2 or BA-10 V5 changes (the output cites the SHA-256 of each).

## 6. Delta V5 (decisions section 217)

| Finding id | Decision | Fix (section/field) |
|---|---|---|
| R1:BA10-V4-03 | AR4-21 | FIXED: `EVIDENCE_REPETITION = 1` is "the implementer's draft proposal in BA-10 (since V4) for the Coordinator value; not a value and not a Coordinator position" (section 2, row1; block B assumptions). BA-10 V5 states the same attribution (its 2.1 row `EVIDENCE_REPETITION`) |
| R1:BA10-V4-01 and R2:BA10-V4-01 | AR4-20; AR4-01 | APPLIED on this side: the script derives the BA-10 V5 rule `LEARN` for every learning micro-scenario, with `max(N_strict, N_det + 1)` for a governing one that serves a class-A or class-B group, and reports how many do (block A.2: 0, so no count changes from this rule); block A.7 checks that BA-10 V5 states the rule. The memo was re-run, as the required fix asks when a count changes: the V5 counts change for the reasons of section 3, not for this rule |
| R1:BA10-V4-02 and R2:BA10-V4-02 | AR4-20 | NOT_APPLICABLE to the memo (a BA-11 row; see BA-10 V5 section 7) |
| AR4-21 | applied | header `OWNER`: the memo goes with Q-O1..Q-O5 in the Owner package once BA-10 V5 and this memo are reviewed; the package needs explicit authorization (section 218) |
| AR4-14 (a) | applied | block A.2 excludes `CONTRACT_ASSIGNED_GROUPS` from the class composition; block E.1 checks every scenario that carries them; the V4 sizing of the other reading (V4 block E.1) is removed, since the question is ruled |
| AR4-14 (b)(i) | applied | block E.2: the runs the Q-O1 pair sizes without a pair of their own |
| AR4-14 (b)(ii) | applied | `V(s)` read per scenario from its declared key changes and checked against BA-08 V5 HDM-2 and HDM-4 (each key exactly once; block A.6); the V4 "one more informative run per key" sensitivity (V4 block D.7) is replaced by the fixed count (block D.7); the V4 sizing of the other reading (V4 block E.2) is removed |
| AR4-06 | applied | the twins and the characterization build are in the counts: blocks A.8 (scenarios per build), A.9 (the 29 twins, checked against their product-build scenarios; the I-14 family), B.3 (runs per build), C.1 (the twins as a cause of the V4-to-V5 difference); `EA2-DEFERRED-EVENTS` on the characterization build in the `PARAM-03` sample (block A.4); the runs that need the `BUILD_EQUIVALENCE_RECORD` (block F.3) |
| AR4-01 | applied | the 16 governing `EV-L-OC-*` on the product build (rule `LEARN`); `EV-L-COMPOSED` non-governing (`CHAR`, composition check, `N_det + 1`; blocks A.3, B.2, E.4); the `Q-I14-*` family with its dry-run scenarios (block E.4; 5 since the catalog's correction pass added `Q-I14-P`) |
| AR4-07 | applied | the four `HDM-CHAR-*` (blocks A.6, B.2, E.3) |
| Review recommendation (pass 2, section 217): "tell the Owner that the counts in block F move with AQ-V4-01, AQ-V4-03, AQ-V4-07, K-3 and K-8" | AR4-01, AR4-03, AR4-07 | APPLIED as it now stands: those three questions are ruled, so section 3 states that block F moves with K-3 and K-8 and, since the catalog's correction pass (final), with AQ-V5-08 (the blockers of the V5 catalog), and counts what the open questions of round V5 touch (section 3, "Open questions of round V5"); block F adds the counts by status and by run prerequisite |
| V4 block C (the V3 reconciliation) | - | replaced by the V4 reconciliation (block C); the V3 correction stays in the V4 memo and in decisions section 214.1 |
| correction pass CP-01 (the corrected V5 catalog: coverage and consistency critics) | AR4-20; AR4-21 | APPLIED: the script was re-run on the corrected catalog (section 4 cites its SHA-256): 425 scenarios, 162 dry-run scenarios. A.1 and A.5 agree with all 425 `REPETITION` labels, and no label of an existing scenario changed (A-1 back in the `PARAM-03` sample and `EA2-DEFERRED-EVENTS` changes their `GROUPS_SERVED`, not their rule; the `HDM-CHAR-FXDIM` and `PRODUCT_BUILD` profile texts move no count). New in the counts: the vehicle `Q-I14-P` (`EVIDENCE`, +1) and its dry run `WU-DRY-Q-I14-P` (+29 at row1), so row1 moves from 10360 to 10390; the `REVIEWED_RECORD` and `QUALIFICATION_RECORD@Q-I14-P` prerequisites (F.3). Every figure of section 3 was updated and is checked against the output by the generator |
| correction pass CP-02 (open Architect questions of round V5) | AR3-26; AR4-20 | FIXED: the V5 draft's header statement that no question was open for this memo is withdrawn; the header and section 3 name the open questions that touch the counts (AQ-V5-06: A.7, B.2; AQ-V5-02: F.4; AQ-V5-01: F.5) and state that AQ-V5-03, AQ-V5-04, AQ-V5-05 and AQ-V5-07 move no count the memo shows. F.4 now sizes both twins of AQ-V5-02 (`PR-CLOSURE-COMPLETENESS-CB` and `WU-BOUNDARY-CB`; the V5 draft sized the first only); F.5 (new) counts the 111 scenarios of AQ-V5-01 and their runs. No ruling is modeled |
| correction pass CP-03 (BA-10 V5 correction pass; Q-V5-BA04-3) | AR4-14 (b)(i); AR4-01 | APPLIED: Q-V5-BA04-3 is closed by BA-04 V5 with a pointer to BA-10 V5 section 2.2; the count `N_det + 1` that the memo uses for `EV-L-COMPOSED` is BA-10 V5's reading, open as AQ-V5-06, and a new line of A.7 checks that BA-10 V5 section 6.1 records it (eleven yes/no checks now); the rule-sheet hash of the output follows the corrected BA-10 V5 |
| correction pass CP-04 (the I-14 family) | AR4-01 | APPLIED: A.9 and E.4 read the six-member I-14 family (`Q-I14-P` added by the catalog's correction pass; its admission is the open Architect item AQ-V5-08 of the catalog) with 5 dry-run scenarios |
| correction pass (final) (the re-check of the correction pass: the catalog and BA-08 V5 changed) | AR4-01; AR3-26 | APPLIED: the script was re-run on the catalog and on BA-08 V5 as corrected by the correction pass (final) (section 4 cites their new SHA-256; the script is unchanged): A.1 and A.5 still agree with all 425 `REPETITION` labels, and no count of blocks A to E and no count of F.2 to F.5 changes; F.1 gains the row AQ-V5-08 (2 scenarios, 1 governing, 30 runs at row1), the `BLOCKED_BY` value that the catalog now carries for `Q-I14-P` and `WU-DRY-Q-I14-P` (both also under K-8). The header, section 3 and the rows above name AQ-V5-08; the new F.1 figures are checked against the output by the generator |
