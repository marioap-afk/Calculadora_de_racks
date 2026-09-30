# V4 coordination sheet (binding for every V4 implementer of I-52 CT-21D baseline artifacts)

Worktree (the ONLY place you write repository files): `C:\Users\alejandra-mendoza\.codex\worktrees\feature-rackmirror-espejo-semantico`
(branch `feature/rackmirror-espejo-semantico`, HEAD `aebb86c2`). Scratchpad for generators:
`C:\Users\ALEJAN~1\AppData\Local\Temp\claude\D--Documentos-Codex-Calculadora-de-racks\d096defd-71ce-4bf6-b50f-1d1c3a4d1c81\scratchpad`
(bundles in `v4b\`, V3 generators at the scratchpad root).

## 0. Authority and hard limits

- Governing decisions: `docs/automation/decisions/I-52.md` **section 214** (Architect delta ruling, decisions AR3-01..AR3-30,
  written in Spanish; they are closed decisions, apply them exactly) and section 215 (Coordinator: REVISION, round V4). Section 211
  (AR2-xx) still applies where section 214 does not change it. Contract: V2.1 + V2.2 + V2.3 + V2.4 (draft revision 3) + V2.5 (draft
  revision 1, new file `docs/initiatives/I-52-ct21d-authority-contract-v2.5.md`, adds the BUILD_BOUND tuple field `CATALOG_FOLDER`).
- Findings to fix: the bundle(s) named in your task (`v4b\<KEY>.md`: closure items not FIXED + new findings with `required_fix`
  + verification votes). Where a finding's `required_fix` and a section-214 decision differ, **section 214 prevails**. Every
  confirmed finding in your bundle for your artifact must end FIXED, or be reported as NOT_ADDRESSED with the exact reason.
- This is **design / documentation only**. Do NOT: run or touch AutoCAD or any host work; modify `src/`, tests, V1/V2/V3 files,
  the contract files, `docs/automation/decisions/I-52.md`, `docs/HANDOFF.md`, BA-02a V3, or any file not assigned to you; run
  `git add/commit/push/checkout/stash/reset` (read-only git such as `git show`, `git hash-object`, `git log` is fine); invent
  hashes, host evidence, measurements, Owner answers or values for `p`, `alpha`, `u`, `c`, `TOL_SCALE`, `PARAM-04`,
  `EVIDENCE_REPETITION`, `N_B` or any other UNSET parameter; turn any FAIL/UNKNOWN/INVALID into PASS; call a candidate hash normative.
- State that does not change: CT21D_AUTHORITY_BASELINE_READY = FALSE; CT21D_EXECUTION_READY = FALSE; CT21D_EXECUTION =
  NOT_AUTHORIZED; CURRENTLY_ADMISSIBLE_KIND_SCOPE = NONE; CIA = UNKNOWN; SafeOperationalState = FALSE_FOR_ADMISSION; G3 = STOPPED.

## 1. File and header conventions

- New version = new file with suffix `-v4` next to the V3 file (e.g. `docs/initiatives/I-52-ct21d-baseline-ba-08-selective-kind-baseline-v4.md`).
  V3 files stay byte-identical (history). Generators: copy the V3 generator to a new `*v4*.py` name in the scratchpad (or in
  `docs/automation/evidence/` if the V3 generator was published there) and edit the copy.
- Header block: `ARTIFACT_VERSION = 4-DRAFT (supersedes 3-DRAFT, blob <git blob of the V3 file>, which stays as history)`;
  `AUTHORITY_CONTRACT = CT-21D V2.1 + V2.2 + V2.3 + V2.4 (draft revision 3) + V2.5 (draft revision 1); V2.4 and V2.5 pending
  Coordinator textual verification`; a line `DELTA RULING APPLIED = decisions section 214 (AR3-..)` listing the decisions you applied.
  No `CANDIDATE_HASH`/`SEAL_HASH` fields in any artifact (the registry BA-11 is the only status authority, AR2-44).
- Cross-references to sibling artifacts use their V4 names ("BA-08 V4 section 12"). Keep section numbering stable where you can;
  if you renumber, say so in the delta section.
- Each V4 artifact ends with a section **"Delta V4 (decisions section 214)"**: a table `finding id | decision | fix (section/field)`
  covering every finding of your bundle for that artifact (FIXED / NOT_APPLICABLE with reason).
- Language: English, same register as V3 (short sentences, no marketing words). Files: UTF-8, LF line endings, final newline.

## 2. Shared identifiers and rules (use exactly these names)

| Id | Where defined | Content |
|---|---|---|
| `FX-ANN-BLANK` | BA-08 V4 section 12 (fixture spec) | variant of `FX-ANN`: logical `Name` blank, `DrawRackName` ON, every other annotation switch OFF, dimensions `None`. The source draws no name label and does not create its label layer; the mirror is named "<base> - espejo" (V17, never blank) and does create it (SL-Y "added" reachable; AR3-15) |
| `CL-CLEAN-FXANNBLANK-FP` | BA-04 V4 | governing clean scenario on `FX-ANN-BLANK` (same group/pattern as `CL-CLEAN-FXANN-FP`; `BLOCKED_BY` K-3), with its dry run `WU-DRY-CL-CLEAN-FXANNBLANK-FP` |
| `HDM-CHAR-FX1F`, `HDM-CHAR-FXANN`, `HDM-CHAR-FXDIM` | BA-04 V4 (scenarios), BA-08 V4 (rule of the `HOST_DEFAULT_MAP` pin) | `NON_GOVERNING_BY_DESIGN`, `GROUP` = `HDM`, `GROUPS_SERVED` = [], characterization build, mode of AR3-01; they vary the consumed context variables and record the effective attributes; ordered after the `LOCK_MODE` selection and before any governing run that uses S-1..S-3; product outcome `PROVISIONAL_NON_GOVERNING` (AR3-11) |
| `NC-CLONE-REPLACE` | BA-04 V4 (scenario), BA-02b V4 (coverage) | governing negative control on `FX-IMP-NESTED`, group `PW-CLONE`; a characterization-build switch forces `DuplicateRecordCloning.Replace`; post-import re-read detects the replaced homologue => `CLONE` FAIL; run stops at PREPARE-W; ABORT-VERIFY finds the difference => **O6** (V2.1 17.2 row 4); `DELIBERATE_VIOLATION_FLAG = YES`, target `CLONE`; `BLOCKED_BY` [K-8] (AR3-07) |
| characterization build | BA-04 V4 section 1 (schema), used by BA-06/BA-08/BA-09 | a build of the product with harness-only switches (like the I-14 writer builds of V2.1 4.3); its identity is a tuple field of the run (`CHARACTERIZATION_BUILD` = build receipt + DLL SHA-256). Scenario fields: `BUILD_UNDER_TEST` = `PRODUCT_BUILD` or `CHARACTERIZATION_BUILD`; `RECORDED_NOT_ENFORCED` = list of controls/phases that are recorded and do not govern the flow |
| AR3-01 mode | BA-04 V4 | `RECORDED_NOT_ENFORCED = ["E-03","E-04","E-12 phase A","E-12 phase C","S-1","S-2","S-3"]` on `CHARACTERIZATION_BUILD`; the run reaches PV, `Commit()`, V1 and PC; `EXPECTED_PRODUCT_OUTCOME = PROVISIONAL_NON_GOVERNING` (never O1..O7); no dependency on EVM, `HOST_DEFAULT_MAP` or K-3. Applies to `E4-LEARN-R1-*`, `LK-*`, `HDM-CHAR-*` and every `WU-DRY-*` |
| `WU-DRY-<scenario>` | BA-04 V4, BA-09 V4 | mandatory for **every GOVERNING scenario that enters the governed window (P0..CP)**, including those that serve evidence groups; not for non-governing runs, learning, qualification or no-product-run scenarios (declared). Mode of AR3-01; `PIN_SLOTS` = [`FIXTURE_INSTANCE@<FX>`] only; count per governing scenario = `WU_DRY_RUNS` = `N_B` (Owner, Q-O2; UNSET) (AR3-02, AR3-24) |
| `DESIGNED_STIMULUS` | BA-04 V4 schema | new field: `NONE` or a short description of a designed stimulus (source swap, clone variant, induced seam failure, stop after PREPARE-W, residual scan cell without an expected control FAIL). Designed stimuli carry `DELIBERATE_VIOLATION_FLAG = NO`; `YES` only for a deliberate violation of a control, with its target (AR3-05) |
| `CONTRACT_ASSIGNED_GROUPS` | BA-04 V4 schema, BA-02b V4 | new field: list of `{group, clause}` for groups the contract assigns to a `NOT_A_CONTROL` item or a qualification control (X7 cells -> SC-B, V2.1 10.8; residual cells -> SC-B, V2.1 10.6 and 21.4; closure completeness -> PR-REC, V2.2 PA-12). Evidence only, never PASS. `PR-CLOSURE-COMPLETENESS` moves to group `Q` with `PR-REC` in this field (AR3-06) |
| `PROVISIONAL_NON_GOVERNING` | BA-04 V4 | product-outcome token of AR3-01 runs |
| `NEGATIVE_CONTROL_NOT_EVALUATED` | BA-02b V4 | status of a control whose (conditional) negative-control scenario resolves `NOT_RUNNABLE`; the cell stays uncovered, never MET or PASS (AR3-03) |
| `CER-PERSIST` | BA-04 V4 CER table | registered conditional-expectation rule used by `NC-S5-O6` (AR3-04) |
| `NC-S5-O6` | BA-04 V4 | the object changed through `OpenCloseTransaction` is an entity of the **source definition** (member of the SL-S read-set and of `PRE_COMMAND_STATE_RECORD`), so E-08 FAIL at PV aborts unconditionally; E-12 phase A conditional on the witness; E-06 not declared; `BLOCKED_BY` E6-C4 (AR3-04) |
| `INVENTORY_REVIEW_RECORDS` | BA-06 V4 EVM schema | EVM-level field: hashes of the `REVIEWED` record and of every reopened class-review record; the `EVM_FROZEN` pin record cites it (AR3-22, landing of V24-N2) |
| `ANCHOR_RECORDED` | BA-07 V4 | model A anchor event (AR3-23) |
| `CATALOG_FOLDER` | V2.5 | BUILD_BOUND tuple field (resolved catalog dir + SHA-256 of every `*.csv`/`*.json` directly in it; sampled before library acquisition, re-checked after CP) |
| `PARAM-03` sample | BA-10 V4 | the governing `CL-CLEAN-<fixture>` scenarios (9 with `CL-CLEAN-FXANNBLANK-FP`) **plus** `EA2-DEFERRED-EVENTS`; K = their number in the sealed catalog (symbolic rule) (AR3-24) |
| `MAX_ATTEMPTS` | BA-10 V4 (and BA-04 V4 repetition rules) | per planned governing run (repetition slot); every attempt of the slot counts (contamination included); a slot that exhausts m leaves the scenario without a required run and its group cannot be PASS (AR3-24) |

## 3. DEPENDS_ON graph of BA-11 V4 (use it in your header's dependency line)

`DEPENDS_ON` (registry entries only) vs `PREREQUISITES` (external inputs, each with its gate: before candidate / before seal).

| Artifact | DEPENDS_ON |
|---|---|
| BA-01 | none |
| BA-02a | BA-01 |
| BA-03 | BA-01 |
| BA-05 | BA-01 |
| BA-07 | BA-01 |
| BA-10 | BA-01 |
| BA-06 | BA-01, BA-03, BA-05 |
| BA-08 | BA-01, BA-03, BA-05, BA-06 |
| BA-09 | BA-01, BA-06, BA-07, BA-08 |
| BA-04 | BA-01, BA-02a, BA-03, BA-05, BA-06, BA-08, BA-09, BA-10 |
| BA-02b | BA-02a, BA-04 |
| BA-11 | every other entry |

Seal order: `BA-01 -> BA-02a, BA-03, BA-05, BA-07, BA-10 -> BA-06 -> BA-08 -> BA-09 -> BA-04 -> BA-02b -> BA-11`.
BA-05 does not depend on BA-08 (AR3-17: vectors in slot units, census universe = every library block); BA-07 does not depend on
BA-05 (AR3-23: BA-07 defines its own JCS serialization); BA-10 does not depend on BA-04 (symbolic K; illustrative totals live in a
non-sealed cost memo).

## 4. Report back (your final answer)

Return JSON-like structured output: files written (paths), per finding id the status (FIXED / PARTIAL / NOT_ADDRESSED /
NOT_APPLICABLE) with the location of the fix, open questions for the Architect (only real contract ambiguities), and cross-artifact
notes (anything another V4 artifact must say to stay consistent with yours).
