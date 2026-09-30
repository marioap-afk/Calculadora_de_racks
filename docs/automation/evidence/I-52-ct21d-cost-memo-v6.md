# I-52 — CT-21D Cost Memo V6: Runs Implied by the Parameter Sheet (NOT SEALED; ILLUSTRATIVE)

> **NOT SEALED. NOT A BASELINE ARTIFACT. Arithmetic only: every figure is ILLUSTRATIVE; nothing here is a proposal, a choice or a decision. Every parameter of BA-10 V6 stays `UNSET`.**
>
> ```text
> DOCUMENT            = I-52 CT-21D cost memo V6 (support of BA-10 V6 sections 2.3 and 5; the memo that decisions section 224
>                       lists with the relink round). It supersedes the V5 memo (docs/automation/evidence/I-52-ct21d-cost-memo-v5.md,
>                       blob 78832321320c1afeed084ef94e9e4a4fd9f7caae, and .py, blob 79f07a48d30565875a74c34a263b3cbfe11a1339), which stays unchanged as
>                       history
> STATUS              = NOT SEALED; not a BA-11 registry entry; not part of the bytes of BA-10 V6 or BA-04 V6; no artifact depends on it
> AUTHORITY_CONTRACT  = CT-21D V2.1..V2.5 (V2.4 rev. 3 and V2.5 rev. 2 textually verified) + V2.6 (draft, pending review); the memo
>                       cites no clause of V2.6
> BOUND_PRODUCT_SHA   = 95690c28 (AR6-01; decisions section 224): a non-tuple bound item. The memo states no code fact; it counts the
>                       scenarios of BA-04 V6
> OWNER               = NOT ASKED. Once BA-10 V6 and this memo are reviewed, the memo goes with Q-O1..Q-O5 in the Owner decision
>                       package (decisions section 217, AR4-21); the Coordinator's order of section 218 places that package after
>                       the candidate rulings, and it needs explicit authorization
> SOURCE              = docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v6.json (draft; every count changes with the
>                       catalog); the closed key list of docs/initiatives/I-52-ct21d-baseline-ba-08-selective-kind-baseline-v6.md
>                       section 11.2 (rows HDM-2 and HDM-4; draft), against which the keys each `HDM-CHAR-*` scenario declares are
>                       checked; docs/initiatives/I-52-ct21d-baseline-ba-10-parameter-sheet-v6.md, whose rules and the record of the
>                       AR5-08 ruling the script checks (block A.7); for block C, the V5 catalog, BA-08 V5 and the V5 memo. The output
>                       cites the SHA-256 of each file
> SCRIPT              = docs/automation/evidence/I-52-ct21d-cost-memo-v6.py (SHA-256 a7a044a6dd4b75651763118af06b1afa18ff37f47eb5ba6da5d94e8c8b5ebece over its LF-normalized bytes,
>                       computed with Python hashlib); its complete output is section 4, verbatim
> DELTA RULING APPLIED = decisions section 220: AR5-05 (the twins by content and their dry runs in the counts, block F.4), AR5-04 (the
>                       reverse direction of the build equivalence: a run prerequisite, block F.5), AR5-08 (the composition-check
>                       count N_det + 1, ruled; block A.7), AR5-09 (Q-I14-P admitted: AQ-V5-08 leaves block F.1), AR5-12 (the
>                       bound-SHA precondition record: a run prerequisite, blocks F.3 and F.6); decisions section 223: AR6-02 through
>                       the catalog (the FX-ANN-BLANK scenarios keep their runs); section 217 (AR4-21, AR4-14, AR4-06, AR4-01, AR4-07)
>                       where section 220 does not change it
> ARCHITECT QUESTIONS = every V5 question that moved or could move the counts of this memo is ruled (AQ-V5-01 by AR5-04, AQ-V5-02 by
>                       AR5-05, AQ-V5-06 by AR5-08, AQ-V5-08 by AR5-09; section 220). OPEN question of round V6 that touches what
>                       the memo shows (not modeled): AQ-V6-BA04-1 of BA-04 V6 (the scope of BOUND_SHA_PRECONDITION_RECORD; block
>                       F.6): it adds a run prerequisite only and moves no run count. AQ-V6-BA07-1 and AQ-V6-BA06-01..04 move no
>                       count that this memo shows
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 1. Purpose

BA-10 V6 is symbolic (AR3-24, AR3-25): it defines the repetition rules and the run expression of its section 2.3 and holds no count of the scenario catalog. This memo evaluates that expression on the **draft** BA-04 V6 catalog (437 scenarios), for the illustrative choices that BA-10 V3 section 2.3 used and the V4 and V5 memos kept, and gives the sensitivities the Owner will need. It is decision support to be reviewed, not an input of any value.

The V6 catalog adds the per-build twins by content of AR5-05 (`PR-CLOSURE-COMPLETENESS-CB`, `WU-BOUNDARY-CB` and the five `PW-CLONE-*-CB`) and the dry runs of the five `PW-CLONE-*-CB`; it adds two run prerequisites that move no run (the reverse direction of the `BUILD_EQUIVALENCE_RECORD`, AR5-04; the `BOUND_SHA_PRECONDITION_RECORD`, AR5-12); it removes `AQ-V5-08` from `BLOCKED_BY` (AR5-09); and it gives the three `FX-ANN-BLANK` scenarios a provenance note that changes no run (AR6-02). This memo counts them and reconciles its totals with the V5 memo (block C), whose figures it reproduces exactly from the V5 catalog.

## 2. Illustrative choices (ILLUSTRATIVE; not decisions)

| Row | `p` | `alpha` | `u` | `c` | other assumptions (all rows) |
|---|---|---|---|---|---|
| row1 | 0.10 | 0.05 | 0.05 | 0.95 | one pair for both classes (`p_A = p_B = p`, `alpha_A = alpha_B = alpha`); `p_det = p`, `alpha_det = alpha`; `EVIDENCE_REPETITION = 1`: the implementer's draft proposal in BA-10 (since V4) for the Coordinator value; not a value and not a Coordinator position (AR4-21); `CHAR_RUNS` as BA-10 V6 section 2.2 states it (`N_A` for the pin- and selection-feeding runs; for `HDM-CHAR-*`, `N_A` runs at the pinned key configuration plus exactly one informative run per varied key, with the varied keys read from each scenario and checked against BA-08 V6 HDM-2; `N_det + 1` for the composition check `EV-L-COMPOSED`, ruled by AR5-08; `EVIDENCE_REPETITION` for the exploratory runs); `K` as read from the draft catalog |
| row2 | 0.20 | 0.05 | 0.10 | 0.95 | same |
| row3 | 0.30 | 0.10 | 0.10 | 0.90 | same |
| row4 | 0.50 | 0.10 | 0.20 | 0.90 | same |

These are the four rows of BA-10 V3 section 2.3, kept so that the versions can be compared. They are not a range the Owner is asked to choose from.

## 3. Reading the output (figures of section 4)

- **Totals (block B).** Row1 gives 10710 runs: 5509 planned governing runs (the repetition slots `G`) and 5201 non-governing runs (`D`: 4843 dry runs and 358 non-governing characterization runs). Rows 2 to 4 give 5235, 2680 and 1585. The 12 `SYNTH` runs of each row are offline evaluations; every other run is a fresh AutoCAD process on the build under test of its scenario.
- **By build (blocks A.8 and B.3; AR4-06).** At row1 the `PRODUCT_BUILD` carries 1494 governing and 31 non-governing runs (unchanged from V5), and the `CHARACTERIZATION_BUILD` 4003 governing and 5170 non-governing runs: all 167 dry-run scenarios, the pin- and selection-feeding runs and the `HDM-CHAR-*` runs are on it. The 12 offline runs have no build.
- **Qualification twins (blocks A.9, B.3 and F.4; AR4-06 (3), AR5-05).** Every qualification scenario outside the I-14 family has a twin `<id>-CB` on the characterization build with the same `REPETITION` label: 36 twins (22 `EVIDENCE`, 13 `DEFAULT`, 1 exploratory), 400 runs at row1 (205, 114 and 75 at rows 2 to 4). Seven of them are the twins by content of AR5-05: `PR-CLOSURE-COMPLETENESS-CB` (`EVIDENCE`, +1 at row1, no dry run), `WU-BOUNDARY-CB` (`DEFAULT: N_B`, +29, no dry run) and the five `PW-CLONE-*-CB` (`DEFAULT`, each +29 plus its 29 dry runs). With their dry runs they add 320 runs at row1 (155, 78 and 45 at rows 2 to 4). The I-14 family (six scenarios) runs on the characterization build only.
- **V5 to V6 (block C; row1: +320).** The V5 block B rows are reproduced exactly from the V5 catalog. The whole difference is the seven twins of AR5-05 (+175 at row1, all governing) and the dry runs of the five `PW-CLONE-*-CB` (+145, non-governing). No label of an existing scenario changed, no scenario changed its governance or its build, and no scenario was removed. Only two `BLOCKED_BY` values changed: `Q-I14-P` and `WU-DRY-Q-I14-P` lose `AQ-V5-08` (AR5-09) and keep K-8.
- **`N_B` (block D.1).** Below `N_A`, each unit of `N_B` moves 173 runs (167 dry-run scenarios, 5 class-B-only `DEFAULT` scenarios with `WU-BOUNDARY-CB`, 1 class-B-only `EVIDENCE_STRICT` scenario). Above `N_A`, each unit moves 305 runs, because the 132 governing mixed-class scenarios then follow `N_B`. The dry runs are the largest single term.
- **`EVIDENCE_REPETITION` (block D.2).** Each unit adds 60 runs (58 `EVIDENCE` scenarios, the 22 `EVIDENCE` twins included, `PR-CLOSURE-COMPLETENESS-CB` among them, and the 2 exploratory runs), while it stays below the floor of the `EVIDENCE_STRICT` scenarios.
- **`K` (block D.3).** At the four rows `N` dominates `ceil(n / K)`, so one more sample member adds its own `N_strict` runs and its `N_B` dry runs (+58 at row1). Where `n` dominates (row1 with `u = 0.01`) one more member adds 48 runs.
- **`u` and `c` (block D.4).** No effect at the four rows: halving `u` or `1 - c` leaves every total unchanged. They act only below `u*` (0.0103, 0.0212, 0.0324 and 0.0559 for the four rows), and then only the `CLEAN` term grows (row1 with `u = 0.01`: +10 runs).
- **`alpha` and `p` (block D.5).** `alpha` acts logarithmically: halving it adds 21% to 27%. `p` acts roughly inversely: halving it adds 102% to 115%.
- **`MAX_ATTEMPTS` (block D.6).** `m` is per repetition slot. In the worst case every slot uses `m` attempts: `5509 * m` governing attempts at row1, so each unit of `m` adds up to 5509 attempts. The non-governing runs have no slot.
- **`N_det` alone (block D.8, Q-O3).** Each unit adds 20 runs at the four rows: 16 learning micro-scenarios, the composition check (its `N_det + 1` count, ruled by AR5-08), and the 3 validation scenarios, because at these rows `N_det` equals `N_strict` and `max(N_strict, N_det)` then follows `N_det`.
- **`HDM-CHAR-*` informative runs (blocks A.6, D.7 and E.3).** The four `HDM-CHAR-*` scenarios each vary the 9 keys of BA-08 V6 HDM-2 (the 7 context variables `CLAYER`, `CECOLOR`, `CELTYPE`, `CELTSCALE`, `CELWEIGHT`, `CETRANSPARENCY`, `CPLOTSTYLE`, and the current text style and dimension style), each key exactly once: 36 informative runs in all, outside the intent. AR4-14 (b)(ii) makes this count determinate. `HDM-CHAR-FXANNBLANK` keeps its runs: its provenance note (AR6-02) changes no count.
- **Rulings (block E).** 18 scenarios carry `CONTRACT_ASSIGNED_GROUPS` (`PR-CLOSURE-COMPLETENESS-CB` is new), and each label resolves `N_strict` from `GROUPS_SERVED` only (AR4-14 (a)). The Q-O1 pair also sizes 290 non-governing runs at row1 (the 6 pin- and selection-feeding scenarios and the pinned-configuration runs of the 4 `HDM-CHAR-*`), with no pair of their own (AR4-14 (b)(i)). No learning micro-scenario serves a class-A or class-B group, so the `LEARN` floor of BA-10 V6 adds nothing today (block A.2). The six-member I-14 family has 5 dry-run scenarios (block E.4).
- **Draft scenarios and run prerequisites (block F).** The totals count the draft scenarios as the catalog states them; the memo models no ruling. At row1: K-3, 138 scenarios, 3806 runs; K-8, 45 scenarios (24 governing), 1166 runs; the 171 `DRAFT` scenarios, 4652 runs. The blockers of the V6 catalog are K-3 and K-8 only (AR5-09). Run prerequisites (F.3), which move no run: the `BOUND_SHA_PRECONDITION_RECORD` on the 245 governing scenarios on a build under test (5497 runs at row1; AR5-12, F.6); the `BUILD_EQUIVALENCE_RECORD` on 163 scenarios (4548 runs), that is the 48 `PRODUCT_BUILD` scenarios of direction (a) and the 115 governing `CHARACTERIZATION_BUILD` scenarios that consume `EVM_FROZEN` (3307 runs; AR5-04, F.5); the `REVIEWED_RECORD` on the 17 `EV-L-*` scenarios (510 runs).
- **Open question of round V6 (block F.6).** AQ-V6-BA04-1 (BA-04 V6 section 8.3) asks whether the non-governing runs on a build under test also name the `BOUND_SHA_PRECONDITION_RECORD`: 180 scenarios, 5201 runs at row1. A ruling either way adds or withholds a run prerequisite and moves no run count; it is not modeled.
- **Catalog labels (blocks A.1 and A.5).** Every `REPETITION` label of the catalog equals the BA-10 V6 derivation exactly (all 437), with `N_strict` resolved as `N_A`, `N_B` or `max(N_A, N_B)` and the `CHAR_RUNS` kind resolved. The 132 governing mixed-class scenarios all read `max(N_A, N_B)`. Block A.7 checks that BA-10 V6 states each rule the memo applies and records the AR5-08 ruling of the composition-check count.

## 4. Script output (verbatim)

```text
I-52 CT-21D COST MEMO V6 - ARITHMETIC OUTPUT (ILLUSTRATIVE; NOT A PROPOSAL, NOT A CHOICE, NOT A DECISION)
catalog file    : docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v6.json
catalog SHA-256 : ee8d0f1607ade422f30e42c22f6ad237dd09fa0b79fb882a981d7dec19d6e06c (LF-normalized bytes)
catalog version : 6-DRAFT ; scenarios: 437
key list file   : docs/initiatives/I-52-ct21d-baseline-ba-08-selective-kind-baseline-v6.md (section 11.2, rows HDM-2 and HDM-4)
key list hash   : 129f831d194f0c8667d228bb609cb2bc63e276740a8cff1de5dd7e636688156b (SHA-256, LF-normalized bytes)
rule sheet file : docs/initiatives/I-52-ct21d-baseline-ba-10-parameter-sheet-v6.md
rule sheet hash : f5f329b818708633b070c1daf7387d420c142a7722154f46e11ad20e570533f8 (SHA-256, LF-normalized bytes)

A. COUNTS (read from the draft catalog; they change with the catalog)
A.1 rule derived from the scenario properties (BA-10 V6 section 2.2) vs the REPETITION field of the catalog: all 437 agree
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
A.6 HOST_DEFAULT_MAP keys (BA-08 V6 section 11.2 HDM-2, closed list): CLAYER, CECOLOR, CELTYPE, CELTSCALE, CELWEIGHT, CETRANSPARENCY, CPLOTSTYLE, current text style, current dimension style = 9 keys
    BA-08 V6 HDM-4: N_A runs at the pinned key configuration plus exactly one informative run per varied key (AR4-14 (b)(ii))
    V(s) read from the declared key-change injections of each scenario (each key once, from the closed list, V(s) stated):
    HDM-CHAR-FX1F          V(s) = 9 ; keys of the list it does not vary: 0
    HDM-CHAR-FXANN         V(s) = 9 ; keys of the list it does not vary: 0
    HDM-CHAR-FXDIM         V(s) = 9 ; keys of the list it does not vary: 0
    HDM-CHAR-FXANNBLANK    V(s) = 9 ; keys of the list it does not vary: 0
    informative runs in all: 36 (outside the intent; no Owner answer moves them)
A.7 BA-10 V6 states the rules this memo applies:
    HDM-CHAR-* rule (AR4-14 (b)(ii))                 yes
    EV-L-COMPOSED rule (AR4-01)                      yes
    LEARN rule (R1/R2:BA10-V4-01)                    yes
    N_strict over GROUPS_SERVED only (AR4-14 (a))    yes
    the V4 wording "at least one informative run" is absent: yes
    the composition-check count is recorded as ruled by AR5-08 (AQ-V5-06; BA-10 V6 section 6.1): yes
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

B. ILLUSTRATIVE TOTALS FOR THE CHOICES USED SINCE BA-10 V3 SECTION 2.3
   assumptions (ILLUSTRATIVE): one pair for both classes (p_A = p_B = p, alpha_A = alpha_B = alpha), p_det = p, alpha_det = alpha,
   EVIDENCE_REPETITION = 1 (the implementer's draft proposal in BA-10 for the Coordinator value; not a value and not a Coordinator
   position), K = 10; CHAR_RUNS = N_A (pin- or selection-feeding), N_A + V(s) (HDM-CHAR-*: N_A runs at the pinned key
   configuration plus exactly one informative run per varied key), N_det + 1 (composition check), EVIDENCE_REPETITION (exploratory)
   p     alpha  u     c        N    n  N_det ceil(n/K)       G       D   TOTAL
   0.10  0.05   0.05  0.95    29   59     29         6    5509    5201   10710
   0.20  0.05   0.10  0.95    14   29     14         3    2704    2531    5235
   0.30  0.10   0.10  0.90     7   22      7         3    1395    1285    2680
   0.50  0.10   0.20  0.90     4   11      4         2     834     751    1585
   G = governing planned runs (repetition slots); D = non-governing runs (dry runs, CHAR, any non-governing learning run); TOTAL = G + D.
   12 SYNTH runs per row are offline evaluations (BUILD_UNDER_TEST = NONE; no AutoCAD process).

B.1 terms per rule
   rule                 row1     row2     row3     row4
   DEFAULT              4495     2170     1085      620
   CLEAN                 290      140       70       40
   VALIDATE               87       42       21       12
   LEARN                 480      240      128       80
   EVIDENCE               58       58       58       58
   EVIDENCE_STRICT        87       42       21       12
   SYNTH                  12       12       12       12
   DRY                  4843     2338     1169      668
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
   CHARACTERIZATION_BUILD     4003 / 5170   1948 / 2515    989 / 1276     578 / 745
   NONE                            12 / 0        12 / 0        12 / 0        12 / 0
   of which the 36 qualification twins (all on the CHARACTERIZATION_BUILD): row1 400 ; row2 205 ; row3 114 ; row4 75
   dry-run scenarios per build: CHARACTERIZATION_BUILD 167

C. RECONCILIATION WITH THE V5 MEMO (the V5 catalog evaluated through its own REPETITION labels)
   V5 catalog SHA-256 (LF-normalized): 68c0518c62ccdb43b793ddd7dbd29a056e20c29f0adde614f7bbaa84c0124af0 ; scenarios: 425 ; K = 10 ; HDM-CHAR informative runs: 36 in all
   V5 memo file: docs/automation/evidence/I-52-ct21d-cost-memo-v5.md ; SHA-256 (LF-normalized): 1e08acd0ed451564f664bf55a3ab9d3b9752656e158fd9f4ed696d6efccb6647
   V5 block B rows reproduced exactly from the V5 catalog: yes
C.1 V6 minus V5 per row
   row         V5       V6    delta        G        D
   row1     10390    10710     +320     +175     +145
   row2      5080     5235     +155      +85      +70
   row3      2602     2680      +78      +43      +35
   row4      1540     1585      +45      +25      +20
   by cause (each scenario charged once; the columns add up to the delta):
   row        twins AR5-05    their dry runs       other added     changed label           removed
   row1               +175              +145                +0                +0                +0
   row2                +85               +70                +0                +0                +0
   row3                +43               +35                +0                +0                +0
   row4                +25               +20                +0                +0                +0
C.2 scenario sets, V5 -> V6
   scenarios 425 -> 437; added 12: PR-CLOSURE-COMPLETENESS-CB, PW-CLONE-BASE-CB, PW-CLONE-CASE-CB, PW-CLONE-NESTED-CB, PW-CLONE-SYMTAB-CB, PW-CLONE-WRONG-TYPE-CB, WU-BOUNDARY-CB, WU-DRY-PW-CLONE-BASE-CB, WU-DRY-PW-CLONE-CASE-CB, WU-DRY-PW-CLONE-NESTED-CB, WU-DRY-PW-CLONE-SYMTAB-CB, WU-DRY-PW-CLONE-WRONG-TYPE-CB
   removed: none
   changed governance: none
   changed build under test: 0
   changed BLOCKED_BY: Q-I14-P (K-8+AQ-V5-08 -> K-8), WU-DRY-Q-I14-P (K-8+AQ-V5-08 -> K-8)

D. SENSITIVITIES (finite differences at each illustrative row; ILLUSTRATIVE)
D.1 N_B moved by -1 and by +1 with N_A, N_det, n, E, K fixed (N_B > N_A makes every MIXED term follow N_B)
    row1: N_B 29 -> 28: -173 runs ; N_B 29 -> 30: +305 runs
    row2: N_B 14 -> 13: -173 runs ; N_B 14 -> 15: +305 runs
    row3: N_B 7 -> 6: -173 runs ; N_B 7 -> 8: +305 runs
    row4: N_B 4 -> 3: -173 runs ; N_B 4 -> 5: +305 runs
D.2 EVIDENCE_REPETITION 1 -> 2 (everything else fixed)
    row1: +60 runs
    row2: +60 runs
    row3: +60 runs
    row4: +60 runs
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
    row1: alpha 0.050 -> 0.0250: N 29 -> 36, TOTAL 10710 -> 13265 (+24%) ; p 0.10 -> 0.050: N 29 -> 59, TOTAL 10710 -> 21660 (+102%)
    row2: alpha 0.050 -> 0.0250: N 14 -> 17, TOTAL 5235 -> 6330 (+21%) ; p 0.20 -> 0.100: N 14 -> 29, TOTAL 5235 -> 10710 (+105%)
    row3: alpha 0.100 -> 0.0500: N 7 -> 9, TOTAL 2680 -> 3410 (+27%) ; p 0.30 -> 0.150: N 7 -> 15, TOTAL 2680 -> 5600 (+109%)
    row4: alpha 0.100 -> 0.0500: N 4 -> 5, TOTAL 1585 -> 1950 (+23%) ; p 0.50 -> 0.250: N 4 -> 9, TOTAL 1585 -> 3410 (+115%)
D.6 MAX_ATTEMPTS (PARAM-04 m, per repetition slot): worst case = every slot uses m attempts
    row1: G = 5509 slots: worst-case governing attempts = 5509 * m (each unit of m adds up to 5509 attempts); the 5201 non-governing runs have no slot
    row2: G = 2704 slots: worst-case governing attempts = 2704 * m (each unit of m adds up to 2704 attempts); the 2531 non-governing runs have no slot
    row3: G = 1395 slots: worst-case governing attempts = 1395 * m (each unit of m adds up to 1395 attempts); the 1285 non-governing runs have no slot
    row4: G = 834 slots: worst-case governing attempts = 834 * m (each unit of m adds up to 834 attempts); the 751 non-governing runs have no slot
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

E. RULINGS OF DECISIONS SECTIONS 217 AND 220 APPLIED BY BA-10 V6 AND BA-04 V6 (checks; no other reading is sized)
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
    HDM-CHAR-* scenarios: row1 290 runs, row2 140, row3 70, row4 40
E.3 AR4-14 (b)(ii) and AR4-07: 4 HDM-CHAR-* scenarios, CHAR_RUNS(s) = N_A + V(s) with V(s) = 9, 9, 9, 9; informative runs 36 in all
E.4 AR4-01: 16 governing learning micro-scenarios (rule LEARN, N_det + 1 each, on the PRODUCT_BUILD); EV-L-COMPOSED non-governing
    (CHAR, N_det + 1); the Q-I14 family on the CHARACTERIZATION_BUILD with 5 dry-run scenarios

F. RUNS OF THE DRAFT SCENARIOS (counted in the totals of block B as the draft catalog states them; a scenario with two
   blockers is counted under each; the memo does not model any ruling)
F.1 by blocker
   BLOCKED_BY                 scenarios governing     row1     row2     row3     row4
   K-3                              138       138     3806     1841      924      531
   K-8                               45        24     1166      566      286      166
   scenarios per rule under each blocker:
    K-3: CLEAN 10, DEFAULT 115, EVIDENCE 7, EVIDENCE_STRICT 3, VALIDATE 3
    K-8: CLEAN 1, DEFAULT 17, DRY 21, EVIDENCE 5, LEARN 1
F.2 by status
   STATUS                         scenarios     row1     row2     row3     row4
   DRAFT                                171     4652     2252     1132      652
   EXPLORATORY_NON_GOVERNING              2        2        2        2        2
   SEALED_CANDIDATE                      69      461      251      153      111
   SEALED_PENDING_PIN_CANDIDATE         195     5595     2730     1393      820
F.3 by run prerequisite (a scenario with two prerequisites is counted under each)
   RUN_PREREQUISITES                      scenarios     row1     row2     row3     row4
   BOUND_SHA_PRECONDITION_RECORD                245     5497     2692     1383      822
   BUILD_EQUIVALENCE_RECORD                     163     4548     2208     1116      648
   QUALIFICATION_RECORD@Q-I14-P                   1       29       14        7        4
   QUALIFICATION_RECORD@Q-I14-SILENT-X            7      203       98       49       28
   QUALIFICATION_RECORD@Q-I14-SILENT-Y            3       87       42       21       12
   QUALIFICATION_RECORD@Q-I14-X                  87     2523     1218      609      348
   QUALIFICATION_RECORD@Q-I14-Y                   5      145       70       35       20
   REVIEWED_RECORD                               17      510      255      136       85
F.4 AR5-05 (the ruling of AQ-V5-02): the twins by content on the CHARACTERIZATION_BUILD, each with the rule of its source
    PR-CLOSURE-COMPLETENESS-CB rule EVIDENCE  (source EVIDENCE), REACH NONE; dry-run scenario: no; the twin with its dry runs: row1 +1 ; row2 +1 ; row3 +1 ; row4 +1
    WU-BOUNDARY-CB             rule DEFAULT   (source DEFAULT), REACH NONE; dry-run scenario: no; the twin with its dry runs: row1 +29 ; row2 +14 ; row3 +7 ; row4 +4
    PW-CLONE-BASE-CB           rule DEFAULT   (source DEFAULT), REACH CP; dry-run scenario: yes; the twin with its dry runs: row1 +58 ; row2 +28 ; row3 +14 ; row4 +8
    PW-CLONE-CASE-CB           rule DEFAULT   (source DEFAULT), REACH CP; dry-run scenario: yes; the twin with its dry runs: row1 +58 ; row2 +28 ; row3 +14 ; row4 +8
    PW-CLONE-WRONG-TYPE-CB     rule DEFAULT   (source DEFAULT), REACH PR; dry-run scenario: yes; the twin with its dry runs: row1 +58 ; row2 +28 ; row3 +14 ; row4 +8
    PW-CLONE-NESTED-CB         rule DEFAULT   (source DEFAULT), REACH CP; dry-run scenario: yes; the twin with its dry runs: row1 +58 ; row2 +28 ; row3 +14 ; row4 +8
    PW-CLONE-SYMTAB-CB         rule DEFAULT   (source DEFAULT), REACH CP; dry-run scenario: yes; the twin with its dry runs: row1 +58 ; row2 +28 ; row3 +14 ; row4 +8
    all seven with their dry runs: row1 +320 ; row2 +155 ; row3 +78 ; row4 +45 runs
F.5 AR5-04 (the ruling of AQ-V5-01): governing CHARACTERIZATION_BUILD scenarios whose PIN_SLOTS name EVM_FROZEN (learned and
    validated on the PRODUCT_BUILD, AR4-01): 115; per rule: CLEAN 1, DEFAULT 110, EVIDENCE 1, EVIDENCE_STRICT 3
    of them naming BUILD_EQUIVALENCE_RECORD in RUN_PREREQUISITES: 115; their runs: row1 3307 ; row2 1597 ; row3 799 ; row4 457
    the ruling adds a run prerequisite and moves no run count
F.6 AR5-12 (BOUND_SHA_PRECONDITION_RECORD, BA-04 V6): scenarios that name it: 245; the governing scenarios on a build under test: 245;
    the same set: yes; their runs: row1 5497 ; row2 2692 ; row3 1383 ; row4 822
    the open question AQ-V6-BA04-1 of BA-04 V6 (a wider scope) touches the 180 non-governing scenarios on a build (per governance: EXPLORATORY_NON_GOVERNING 2, NON_GOVERNING_BY_DESIGN 178);
    their runs: row1 5201 ; row2 2531 ; row3 1285 ; row4 751; a ruling adds a run prerequisite only and moves no run count (not modeled)

END OF OUTPUT
```

## 5. Reproduction

From the repository root: `python docs/automation/evidence/I-52-ct21d-cost-memo-v6.py` (Python 3, standard library only; it reads the V6 catalog, BA-08 V6 for the key list, BA-10 V6 for the check A.7 (the rules and the record of the AR5-08 ruling), and the V5 catalog, BA-08 V5 and the V5 memo for block C). It stops, printing no figures, if the HDM-2 and HDM-4 rows of BA-08 V6 or V5 do not read as expected, or if an `HDM-CHAR-*` scenario declares a key change that is not a key of the list, varies a key twice or does not state its `V(s)`. The output is deterministic: the generator of this memo ran the script twice and compared the two outputs byte for byte before embedding them, and it checks that every figure quoted in section 3 is in the output and that no check printed NO. It must be re-run whenever the V6 catalog, BA-08 V6 section 11.2 or BA-10 V6 changes (the output cites the SHA-256 of each).

## 6. Delta V6 (decisions 220 and 223)

| Ruling or finding | Decision | Fix (section/field) |
|---|---|---|
| decisions section 224 (the memo of the relink round) | applied | a V6 memo over the V6 catalog; the V5 memo files stay unchanged as history; block C reconciles with them and reproduces the V5 block B rows exactly |
| AR5-05 (per-build twins by content) | applied | the seven twins and the five dry runs are in the counts (blocks A.9, B.3, C.1, F.4); `WU-BOUNDARY-CB` reads `DEFAULT: N_B` (block D.1 moves by 173 below `N_A`); the V5 block F.4, which sized the open question, becomes the count of the ruling |
| AR5-04 (reverse direction of the build equivalence) | applied | block F.5 counts the 115 governing `CHARACTERIZATION_BUILD` scenarios that consume `EVM_FROZEN`, all now naming the record; no run moves; the V5 block F.5, which sized the open question, becomes the count of the ruling |
| AR5-08 (the composition-check count) | applied | block A.7 checks that BA-10 V6 section 6.1 records the ruling (it checked in V5 that the question was recorded as open); the `N_det + 1` count is unchanged |
| AR5-09 (`Q-I14-P` admitted) | applied | block F.1 has no `AQ-V5-08` row; block C.2 lists the two `BLOCKED_BY` changes; no run moves |
| AR5-12 and G2-CR-03 (the bound-SHA precondition record) | applied | blocks F.3 and F.6 count the 245 scenarios that name the record and check that they are exactly the governing scenarios on a build under test; F.6 counts what the open question AQ-V6-BA04-1 touches |
| AR6-02 (`FX-ANN-BLANK` provenance, through BA-04 V6) | recorded | no count changes: the three scenarios keep their rules and builds (block C: no changed label, governance or build) |
| G2-CR-05, AR6-05, AR6-06 (the contract line) | applied | header `AUTHORITY_CONTRACT`: V2.1..V2.5 with the textual verifications, plus V2.6 (draft, pending review) |
| AR6-01 (the bound SHA) | applied | header `BOUND_PRODUCT_SHA`; the memo states no code fact |
| correction pass (V6) | applied | re-run over the corrected BA-08 V6 and BA-10 V6 bytes (BA-08 V6: where the `FIXTURE_CONSTRUCTION_BUILD` identity is recorded; BA-10 V6: the header and section 7 name this memo by path only, with no hash, and state that the cited section numbers were verified): only the `key list hash` and `rule sheet hash` lines of block A change; no count, rule or figure changes |
