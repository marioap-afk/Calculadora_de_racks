# V5 coordination sheet (binding for every V5 implementer of I-52 CT-21D baseline artifacts)

Worktree (the ONLY place you write repository files): `C:\Users\alejandra-mendoza\.codex\worktrees\feature-rackmirror-espejo-semantico`
(branch `feature/rackmirror-espejo-semantico`, HEAD `d7c89fcb`). Scratchpad:
`C:\Users\ALEJAN~1\AppData\Local\Temp\claude\D--Documentos-Codex-Calculadora-de-racks\d096defd-71ce-4bf6-b50f-1d1c3a4d1c81\scratchpad`
(V5 bundles in `v5b\`; V4 generators at the scratchpad root: gen_catalog_v4.py, gen_ba05v4.py, gen_ba06v4.py, gen_ba10v4.py,
gen_costmemo_v4.py, gen_ba03v4.py; the V4 round sheet is `v4b\COORD.md`).

## 0. Authority and hard limits

- Governing decisions: `docs/automation/decisions/I-52.md` **section 217** (Architect, decisions AR4-01..AR4-22, Spanish; closed
  decisions, apply them exactly) and section 218 (Coordinator: REVISION, round V5 narrow). Sections 214 (AR3) and 211 (AR2) still apply
  where 217 does not change them. Contract: V2.1 + V2.2 + V2.3 + V2.4 (draft revision 3, TEXTUALLY VERIFIED) + V2.5 (revision 2 being
  prepared by the lead; its section 1 is unchanged).
- Findings to fix: your bundle(s) in `v5b\<KEY>.md` contain BOTH review passes of section 217 (ids prefixed `R1:` / `R2:`; the same id
  can mean different findings in the two passes). Every confirmed finding for your artifact must end FIXED, or be reported
  NOT_ADDRESSED with the exact reason. Where a required_fix and an AR4 decision differ, **AR4 prevails**.
- The 14 open questions AQ-V4-01..14 are now **ruled** (AR4-01..AR4-15) or explicitly **DEFERRED** by a named AR4 decision. Replace
  every 'pending AQ-V4-xx' in your artifact with the ruling ('ruled by AR4-yy, section 217') or the recorded deferral. Do not re-open them.
- Design / documentation only. Do NOT: run or touch AutoCAD or any host work; modify `src/`, tests, V1..V4 files, the contract files,
  decisions, HANDOFF, BA-02a V3, BA-03 V4, BA-11 (the lead writes BA-11 V5), or any file not assigned to you; run git write commands;
  invent hashes, host evidence, measurements, Owner answers or UNSET values; turn FAIL/UNKNOWN/INVALID into PASS.

## 1. File conventions

- New version = new file with suffix `-v5` (V4 files stay byte-identical as history). Copy the V4 generator to a `*v5*` name and edit the
  copy. Header: `ARTIFACT_VERSION = 5-DRAFT (supersedes 4-DRAFT, blob <git blob of the V4 file>)`; `AUTHORITY_CONTRACT = CT-21D V2.1 +
  V2.2 + V2.3 + V2.4 (draft revision 3, textually verified) + V2.5 (draft revision 2); V2.5 pending Coordinator textual verification`;
  `DELTA RULING APPLIED = decisions section 217 (AR4-..)`. Cross-references to siblings use V5 names (BA-02a stays V3, BA-03 stays V4).
- Where BA-11 is cited, cite the **registry entry** ("the BA-11 registry", "the BA-04 entry of BA-11"), not a BA-11 version (AR4-17).
- Each V5 artifact ends with a section **"Delta V5 (decisions section 217)"**: `finding id | decision | fix`, covering every finding of your
  bundle for that artifact, and the AR4 decisions applied.
- Scenario version token `5-DRAFT`. English, same register as V4. UTF-8, LF, final newline.

## 2. Shared decisions (use exactly these names and rules)

| Item | Rule (source) |
|---|---|
| Per-build qualification (AR4-06 (3)) | every `Q-*` and `E6-C*` scenario runs on **each** build under test used by governing runs: keep the PRODUCT_BUILD scenario and add a twin `<id>-CB` on `CHARACTERIZATION_BUILD` (same expectation, `QUALIFICATION_TUPLE` naming the build); **exception**: the I-14 family (`Q-I14`, `Q-I14-Y`, `Q-I14-X`, `Q-I14-SILENT-Y`, `Q-I14-SILENT-X`) runs **only** on `CHARACTERIZATION_BUILD` (V2.1 4.3 row I-14), no product twin. No transfer is ruled |
| `CHARACTERIZATION_TUPLE` (AR4-06 (2)) | its own values for every build-dependent BUILD_BOUND field: Plugin and harness DLL SHA-256, manifest build layer (E-10 exact set, WUM set of 13.1), warm-up definition if it differs, qualification records of that build (4.3), `CATALOG_FOLDER` of the build under test (V2.5); one admissible BUILD_BOUND profile per `BUILD_UNDER_TEST` value; each run compared against its own build's profile |
| Build equivalence (AR4-06 (4)) | recorded, verified equivalence (product assemblies byte-identical, switches in separate harness assemblies, or a reviewed diff); pins derived on the characterization build apply to PRODUCT_BUILD runs only under it; `E4-VAL-R1` membership post-check stays the safeguard |
| `EV-L-*` (AR4-01) | GOVERNING, group E12-L, **PRODUCT_BUILD** (micro-scenario driver = declared harness DLL, present and inert in `EV-V-*` validation); **not** in the AR3-01 mode; `RECORDED_NOT_ENFORCED` = `["E-04", "E-12 phase A", "E-12 phase C"]` with the basis per control (E-04: V2.1 3.3, admitted set defined per checkpoint for the mirror command's designated route; E-12 A/C: V2.1 27.2, 14.3, V2.2 PA-2); E-03 enforced under the `LOCK_MODE` pin; S-1..S-3 `NOT_EVALUATED` (AR2-25); every other control fail-closed; `EXPECTED_PRODUCT_OUTCOME = NONE`; no longer BLOCKED_BY AQ-V4-01 |
| `EV-L-COMPOSED` (AR4-01, BA06V4-N04) | leaves the learning corpus and `LEARNING_CORPUS_HASH`; becomes `NON_GOVERNING_BY_DESIGN`, a composition check against the DRAFT / UNDER_REVIEW model that contributes no model entry; events no isolated entry explains are recorded as `UNEXPLAINED_EVENTS` (V2.1 27.2 step 4) |
| `Q-I14-*` (AR4-01) | governing Q evidence on `CHARACTERIZATION_BUILD`; the AR3-01 set applies to the **vehicle** (ruled by AR4-01, no longer a proposal; the vehicle's product result is not adjudicated); keep their `WU-DRY`; the qualified writer set adds `Wr-OCT`, `XDATA_WRITE` and the post-abort read |
| AR3-01 mode, ruled scope | E4-LEARN-R1-*, LK-*, HDM-CHAR-* (four, see below), WU-DRY-* (AR3-01/AR3-02/AR3-11/AR4-07) and the Q-I14-* vehicles (AR4-01) |
| Dry-run branches (AR4-02) | a WU-DRY of a source whose refusal/abort comes from a RECORDED_NOT_ENFORCED control takes the source's declared branch at the same checkpoint through a characterization-build switch that records the control value and then performs the declared refusal/abort; the switch is named in `PLAN_OR_INPUT` and `INJECTIONS`; REACH and profiles as the source |
| Dry-run load measurement (AR4-16) | a dry run **excludes** from its measurement the load events of any assembly/module named by a designed injection of its source (`CANARY_LOAD`, foreign-load `HARNESS_FAULT`), identified by declared identity, and records them separately as the stimulus; every other load in the window counts; **a warm-up revision never preloads an assembly named by an injection** |
| `KS-*` (AR4-03) | no PASS to KS or SC-A; `CONTRACT_ASSIGNED_GROUPS` KS and SC-A (V2.1 26.3, 31) by the AR4-03 extension of AR3-06 ("scenario classes the contract lists for a group whose controls the scenario cannot reach by design"); GROUP AB; KR NO_VERDICT; typed seam failure = `EXPECTED_PROCEDURAL_RESULT`; a mismatch is a seam-contract finding that prevents KS PASS; AQ-V4-03 blocker lifted |
| `NC-S5-O6` (AR4-04) | E-06 = PASS at every defined sampling point of V2.1 4.2 (the Wr-OCT write opens and closes between PS and PV); `BLOCKED_BY E6-C4` removed; rest of AR3-04 unchanged; needs K-3 and the Wr-OCT qualification (Q-I14-Y) |
| `NC-E12A` (AR4-16) | conditional by **CER-WIT**: delivered => E12A FAIL, abort before Commit(), O2/O6 by CER-ABV; not delivered => NOT_RUNNABLE, E12A PASS, commits (O3), cell NEGATIVE_CONTROL_NOT_EVALUATED; needs the witness qualified for XDATA_WRITE at Y1 |
| `EA2-DEFERRED-EVENTS` (AR4-06 (6)) | on `CHARACTERIZATION_BUILD`; its PARAM-03 contribution is valid for the product build only under the recorded equivalence |
| `HDM-CHAR-FXANNBLANK` (AR4-07) | fourth HDM characterization scenario on FX-ANN-BLANK (non-governing, group HDM, AR3-01 mode, slots FIXTURE_INSTANCE@FX-ANN-BLANK and LOCK_MODE, after the LOCK_MODE selection, before any governing S run; HDM-3..HDM-5); records the SYMBOL_RECORD_LAYER typed form of the created layer (all fields except name and colour) with the run's key values; `CL-CLEAN-FXANNBLANK-FP` drops K-6 |
| HDM-CHAR repetition (AR4-14) | `CHAR_RUNS(s) = N_A + V(s)`: N_A runs at the pinned key configuration plus **exactly one** informative run per varied key; varied-key entries are marked informative in the pin record, and a governing lookup that resolves to one is UNKNOWN (O5) |
| `N_strict` (AR4-14 (a)) | CONTRACT_ASSIGNED_GROUPS do not enter N_strict |
| E-08 domain (AR4-08) | ABORT-VERIFY compares every PRE_COMMAND_STATE_RECORD member exactly, CONTEXT included (contract); E-08 compares the REFERENCE **and** CONTEXT_VARIABLE members at PS/PV |
| Plot style (AR4-09) | declared gap; fixtures bind `PSTYLEMODE = 1` (verified in the conformance record) or capture it as CONTEXT_VARIABLE; a governing run on a named-plot-style drawing makes SL-R UNKNOWN (O5); disclosed among the Owner Act 2 coverage limits |
| Dimension overrides (AR4-10) | the stored override set; census and I-12 qualification confirm storage, order and equal-to-style behaviour before the BA-05 candidate; handle-valued overrides UNKNOWN, disclosed |
| `GROUPS_SERVED` / `CONTRACT_ASSIGNED_GROUPS` (AR4-17) | defined **normatively in BA-04 section 1** (AR2-08, AR3-06, AR4-03); BA-02b rule 1 applies "the definition of BA-04 section 1" |
| Pins (AR4-13) | one COORDINATOR + one ARCHITECT ratification (decision RATIFIED, subjectKind PIN, subject = slot, subjectHash = pin-record hash); baselineVersion = the version in force; at most one live pin per slot; carry-over fail-closed (new pin version per baseline version; manifest usable under v<k+1> only if the approving ruling records BA-07 and every tuple field unchanged, else REVOKED SUPERSEDED_BY_RULING) |
| AQ-V4-14 (AR4-15) | no expectation change; BA-06 states the residual with reasons (a)..(e) |
| BA-06 (AR4-12) | STATIC_REVIEWED approved on BA-06 V4; V5 must NOT change the content of the design-time inventory or of the dynamic-trace plan, except adding declared limit (4) of BA06V4-N03 (required by the approval); the Architect's exact delta check verifies this |

## 3. DEPENDS_ON graph of BA-11 V5

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
| BA-04 | BA-01, BA-02a, BA-03, BA-05, BA-06, BA-07, BA-08, BA-09, BA-10 |
| BA-02b | BA-01, BA-02a, BA-04 |

Seal order: `BA-01 -> BA-02a, BA-03, BA-05, BA-07, BA-10 -> BA-06 -> BA-08 -> BA-09 -> BA-04 -> BA-02b -> BA-11`.

## 4. Report back

Structured output: files written, per finding id (with R1:/R2: prefix) the status and location of the fix, AR4 decisions applied, any
remaining real contract ambiguity (new ones only), cross-artifact notes, counts.
