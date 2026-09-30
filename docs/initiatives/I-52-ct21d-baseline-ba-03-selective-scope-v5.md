# I-52 — CT-21D Baseline Artifact BA-03: Sealed Selective Scope (DRAFT V5, RE-LINK ROUND V6)

> **BASELINE ARTIFACT BA-03 V5 — DRAFT prepared for `CANDIDATE_FOR_SEALING`, NOT SEALED, NOT RATIFIED. Design / documentation only.**
>
> ```text
> ARTIFACT_ID               = SELECTIVE_VIEW_FAMILY_SCOPE_V1   (baseline artifact CT21D-BASE-SCOPE-SELECTIVE)
> ARTIFACT_VERSION          = 5-DRAFT (supersedes 4-DRAFT, blob efadcdf617c932eb6c74c60f4c8c2dd8ebede00b, which stays as history)
> ARTIFACT_PATHS            = docs/automation/evidence/I-52-ct21d-ba03-v5-checks-output.json
>                             docs/automation/evidence/I-52-ct21d-ba03-v5-checks.json
>                             docs/automation/evidence/I-52-ct21d-ba03-v5-checks.py
>                             docs/initiatives/I-52-ct21d-baseline-ba-03-selective-scope-v5.md
>                             (ascending ordinal order; the runner output of the candidate run is an artifact path, AR3-28)
> STATUS AUTHORITY          = the BA-11 registry entry of this artifact is the only authority for its status and hashes;
>                             this header is frozen, non-authoritative text of the candidate bytes (BA-11 section 2)
> AUTHORITY_CONTRACT        = CT-21D V2.1..V2.5 (V2.4 rev. 3 and V2.5 rev. 2 textually verified) + V2.6 (draft, pending review)
> BOUND_PRODUCT_SHA         = 95690c28 (95690c28dc6268e61dff32a0cbc33cc9fde3d47f; decisions section 223, AR6-01; never a tuple value, AR5-12)
> DEPENDS_ON                = BA-01 (BA-11 section 2.1)
> PREREQUISITES             = before candidate: none beyond the authority identities of section 2 and the bound product SHA
>                             (bound by the specification; a drift re-prepares the candidate), and the Architect's ruling on
>                             AQ-V6-01 (coverage of the AR5-12 precondition for the BA-06 and BA-08 pairs; section 8.1);
>                             before seal: V2.2 PA-14 conditions 3 and 4 (dated re-execution and post-I-57 reading), recorded
>                             outside this artifact (section 6)
> SCOPE AGREEMENT           = scope agreed in ARCHITECT_BASELINE_PHASE1_RULING = AGREED_WITH_REQUIRED_CHANGES over
>                             1711112021394a6b14112b1787abff4d3062e064; this is NOT a ratification (BA-11: RATIFIED needs the Coordinator's
>                             and the Architect's records citing the exact CANDIDATE_HASH)
> DELTA RULING APPLIED      = decisions 223 (AR6-01, AR6-02 with FXB-03 and FXB-06, AR6-05 with G2-CR-02, AR6-07 / 223.4 row BA-03;
>                             223.1 naming ruling) and 220 (AR5-12, AR5-13, AR5-15)
> CANDIDATE EVIDENCE RUN    = 2026-09-30T20:17:10Z (runner output: docs/automation/evidence/I-52-ct21d-ba03-v5-checks-output.json)
> BLOCKER                   = NB-1  (ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION; OPEN until sealed)
> I60_NAMING_FOR_RACKMIRROR = NOT_YET_ADOPTED (Owner; the mirror keeps the V17 name "<base> - espejo")
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 1. The scope

| Row | Domain | Status for CT-21D |
|---|---|---|
| `SEL-FRONTAL` | `View = frontal`, `Section = k` for `0 <= k < SelectiveDepthLayout.Count(system)` (one per fondo); scenarios cover one fondo and several fondos | **IN** |
| `SEL-PLANTA` | `View = planta` (canonical section `-1`) | **IN** |
| `SEL-LATERAL` | lateral, per post | **OUT** (fail-closed, E5) |
| `SEL-LEGACY-M1` | frontal with the legacy `Section = -1` (or a legacy empty `View`) | **OUT_OF_SCOPE** for CT-21D scenarios |
| `SEL-LEGACY-BLANK-NAME` | an existing Selective rack whose logical `Name` is blank (legacy product state) | **IN** as a legacy product state (AR5-11, AR6-02, FXB-06) |

`Section = -1` is `OUT_OF_SCOPE` (V2.2 PA-14, Q-SEC-1): production canonicalizes it to fondo 0 before planning, so the seams receive and create canonical output; **catalog fixtures use the canonical `Section = k`** (V2.1 25.4). The decision narrows CT-21D only; it neither widens nor narrows the product envelope.

**Legacy blank logical `Name` = IN SCOPE as a legacy product state (AR5-11; decisions section 223.2, FXB-06).** This line is **distinct from** the exclusion of V2.1 25.4 and is not obtained from it by analogy: 25.4 is literal to `Section = -1`, which production canonicalizes before planning, whereas a blank `Name` reaches planning as it is and drives the blank-base branch of the mirror name (Gate 2 finding FXB-06). At the bound SHA no product route creates a **new** Selective rack with a blank name (I-60 gives every new rack an automatic name), but the exact build **preserves** a legacy blank name (I-60 Freeze section 5, A1-5, N-10 validated by the Owner); the scope keeps it, so SL-Y "added" stays covered. Its fixture is `FX-ANN-BLANK`, a legacy-state fixture built by the declared fixture construction build `69daf03a35c630e453e1d9e98136f128bd0325a4` (fixture ruling A; construction route and conformance record: BA-08 V6). The mirror of such a rack keeps the V17 name rule (`<base> - espejo`, with the blank-base fallback), because `I60_NAMING_FOR_RACKMIRROR = NOT_YET_ADOPTED` (decisions section 223.1); only the predicted label text would change under a later adoption (FXB-07).

## 2. Authority identities of this run

| Key | Authority | Resolved at | Path | Identity |
|---|---|---|---|---|
| `V17` | Proposal V17 | branch HEAD | `docs/initiatives/I-52-proposal-v17.md` | blob `dd944d8e9c62b083eebc2b1292789977b880c368` |
| `V18` | Freeze V18 | branch HEAD | `docs/initiatives/I-52-consensus-freeze-v18.md` | blob `c12fa65f5b6203061e8d717d1ceed595c4fed1f4` |
| `P57` | Freeze V17 post-I-57 | branch HEAD | `docs/initiatives/I-52-consensus-freeze-v17-post-i57.md` | blob `5f4933a3aa90d7052f571317107f1831ec02ac6e` |
| `FND` | Foundation reconciliation | branch HEAD | `docs/initiatives/I-52-foundation-consumption-reconciliation.md` | blob `89a21b6f9dc07e72472f3e34598e98049a3e1fb7` |
| `ADR` | ADR-0036 (current) | branch HEAD | `docs/adr/0036-rackmirror-espejo-semantico-por-copia.md` | blob `5628947673f2afddf141c0505e92283962459443` |
| `ADR0` | ADR-0036 (blob frozen by V17) | git object | `(blob)` | blob `ce5f6fae935c7acdc347e12a7d465bb438923549` |
| `DEC117` | O-1 V18 (decisions section 117) | branch HEAD, one section | `docs/automation/decisions/I-52.md` | section `117. O-1 V18 — redecisión del Owner y registro del estado diferido`, SHA-256 `a5c0fd9383c5010af2c641fe5a8a67ff24a3e2084c9691b6bc42c464d16829f9` |
| `RVA` | RackViewAvailability.cs | bound SHA `95690c28` and `origin/main` | `src/RackCad.Application/Systems/Shared/RackViewAvailability.cs` | blob `74e1a1aba691e42e03d3744bf6631e444aa1d696` |
| `SDL` | SelectiveDepthLayout.cs | bound SHA `95690c28` and `origin/main` | `src/RackCad.Application/Systems/Selective/SelectiveDepthLayout.cs` | blob `9e45f2f2c90bcd2ac881d4fc2dc3db2debfcd762` |
| `CMD` | RackSelectivoCommands.cs (partial, part 1) | bound SHA `95690c28` and `origin/main` | `src/RackCad.Plugin/RackSelectivoCommands.cs` | blob `f8e514a085dd7a9889e7b8b3006926acb4350166` |
| `COD` | RackViewCodec.cs | bound SHA `95690c28` and `origin/main` | `src/RackCad.Application/Systems/Shared/RackViewCodec.cs` | blob `b63661678a2488449baab37cc74023fff0550165` |
| `CMDI` | RackSelectivoInsertIntegration.cs (partial, part 2; I-55) | bound SHA `95690c28` and `origin/main` | `src/RackCad.Plugin/RackSelectivoInsertIntegration.cs` | blob `308a42e540e329921926c7db64727a41f7aa3a46` |
| `EXP` | RackViewExposure.cs (I-55 product exposure) | bound SHA `95690c28` and `origin/main` | `src/RackCad.Application/Views/Policy/RackViewExposure.cs` | blob `81c714ca1e7ff889b6104b2234c68d82a935962d` |
| `DEC223` | Gate 2 ruling (decisions section 223) | branch HEAD, one section | `docs/automation/decisions/I-52.md` | section `` 223. ARQUITECTO — GATE 2: EVALUACIÓN DE RE-LIGADO EN `95690c28`, DICTAMEN DE NOMBRES Y DE `FX-ANN-BLANK` ``, SHA-256 `fb75326974838e3a347a0a18a3e4683cf201f5ea842444100efa63f912136f2a` |

**Identity rule (normative, AR2-45; bound SHA per AR6-01).** The identity set of this artifact is the **(path, blob) pairs** above (and, for `DEC117` and `DEC223`, the pair (path, section) with the SHA-256 of the LF-normalized section text). The specification publishes each pair as the **expected identity** of its source (`expected_blob`, or `expected_section_sha256`) and the bound product SHA (`bound_product_sha` = `95690c28dc6268e61dff32a0cbc33cc9fde3d47f`). The six code sources (`RVA`, `SDL`, `CMD`, `COD`, `CMDI`, `EXP`) are of kind `BOUND`: the runner resolves each at the bound SHA **and** at `origin/main`, and both must equal the expected blob; with the optional second argument it also resolves them at the source commit of a build under test (the BA-03 part of the AR5-12 precondition). This run resolved every identity equal to the expected one (`identityDrift` = []), with `origin/main` = the bound SHA. The branch commit is **informative only**: this run resolved it at `a63c052773c8f0eeb953e56020505701e393a86a`; its `src/`, `tests/` and `assets/` trees equal those of the bound SHA (Gate 1 reconciliation, decisions section 222).

The identity set now covers the whole Selective command class, which I-55 split into two partial files (`CMD`, `CMDI`), and the I-55 product exposure (`EXP`). Relative to the previous bound SHAs (facts re-derived with read-only git when this version was prepared; `git rev-parse <sha>:<path>`, `git diff --name-status 3375aadb 95690c28 -- src/`):

| Key | at `3375aadb` | at `1304101d` | at `69daf03a` (fixture construction build) | at `95690c28` (bound) | Gate 2 classification |
|---|---|---|---|---|---|
| `RVA` | `74e1a1ab` | `74e1a1ab` | `74e1a1ab` | `74e1a1ab` | UNCHANGED |
| `SDL` | `9e45f2f2` | `9e45f2f2` | `9e45f2f2` | `9e45f2f2` | UNCHANGED |
| `CMD` | `a22e6a6a` | `a22e6a6a` | `493b89bd` | `f8e514a0` | ARTIFACT_UPDATE_REQUIRED (identity), facts STILL_TRUE_WITH_NEW_IDENTITY |
| `COD` | `b6366167` | `b6366167` | `b6366167` | `b6366167` | UNCHANGED |
| `CMDI` | absent | absent | `308a42e5` | `308a42e5` | ARTIFACT_UPDATE_REQUIRED (added to the set) |
| `EXP` | absent | absent | `81c714ca` | `81c714ca` | CHANGED_BUT_CONTRACT_COMPATIBLE (added to the set) |

In `src/`, the `3375aadb..95690c28` delta has 89 files (34 modified, 55 added, 0 deleted), and the `1304101d..95690c28` delta 87 files: the statement of BA-03 V4 that the reused Selective path is untouched (AR2-46) **no longer holds** and is not carried. Of the code sources of this artifact, `RVA`, `SDL` and `COD` are byte-identical at the four SHAs; `CMD` changed; `CMDI` and `EXP` are new in I-55. The fixture construction build `69daf03a` is **not** a build under test and is never passed to the runner (FXB-03): its `CMD` blob differs from the bound one (I-60), and that difference is expected.

## 3. Row-by-row comparison

`RESULT` uses the closed domain of V2.1 25.3: `IDENTICAL`, `NARROWER` (the CT-21D scope is a strict subset of what the authority permits), `CONFLICT` (a contradiction; **a conflict prevents sealing**) or `NO_STATEMENT` (the authority makes no statement about the scope: not a conflict, not counted as agreement). Qualifiers and rationale are in the `NOTE` column. Every row cites checks of the appendix (section 7).

**Rule for authority-status rows (normative, AR3-28).** A row whose first column is not a scope row of section 1 or the rack-level conditions (here: the authority of V17 3.7 / 3.8, the ownership of the exposure, the product exposure of I-55, the two O-1 dispositions, the ADR-0036 amendment and the mirror naming) only establishes the authority status of another text and makes no statement about a scope row. Its `RESULT` is `NO_STATEMENT`, and its resolution is written in the `NOTE`. It is neither a conflict nor counted as agreement; any agreement it relies on is counted in the scope rows that cite the same authorities.

| Scope row | Authority and section | Statement | RESULT | NOTE | Checks |
|---|---|---|---|---|---|
| SEL-FRONTAL | V17 1.3, 2.1 | Selectivo inside as maximum envelope: frontal by fondo, planta; mu_k = RUN | IDENTICAL | - | `V17_INSIDE` `V17_21` |
| SEL-FRONTAL | V17 3.7, 3.8 | `A_k`: frontal of each fondo `k`, `0 <= k < SelectiveDepthLayout.Count(sistema)`; the frontal row exposes `mu_k` (RUN) | IDENTICAL | - | `V17_AK` `V17_FRONTAL_RUN` |
| SEL-FRONTAL | Freeze V18 3 and 7 | V17 governs what V18 does not replace; exposure and supported combinations preserved | IDENTICAL | by reference to V17 | `V18_S3` `V18_S7` |
| SEL-FRONTAL | Freeze V17 post-I-57, 2 and 4 | no reconciliation changes product policy implicitly; the Foundation substitution is limited to the ownership or location of AUTH-01..13; outside that scope V17 keeps its full meaning; exposure policy and admitted view/kind combinations are an I-52 authority | IDENTICAL | by reference to V17 | `P57_S2` `P57_S2_ITEM2` `P57_S2_CLOSE` `P57_S4` |
| SEL-FRONTAL | Foundation reconciliation 4 | I-52 keeps the exposure of views and supported combinations | IDENTICAL | keeps V17 | `FND_S4` |
| SEL-FRONTAL | ADR-0036 decision 4 (both blobs) | first cut: frontal and planta for Selectivo | IDENTICAL | - | `ADR_NEW_D4` `ADR_OLD_D4` |
| SEL-FRONTAL | bound SHA: SelectiveViewAvailabilityFacts, SelectiveDepthLayout, RackSelectivoCommands | fondo address available when `Index < FondoCount`; `fondoCount = SelectiveDepthLayout.Count(system)`; `Count = Math.Max(1, DepthCount)` | IDENTICAL | checks scoped to the Selective members; `CMD_FONDOCOUNT` is STILL_TRUE_WITH_NEW_IDENTITY (Gate 2) | `RVA_SEL_FONDO` `SDL_COUNT` `CMD_FONDOCOUNT` `CMD_PARTIAL` |
| SEL-FRONTAL | bound SHA: RackSelectivoInsertIntegration (`SelectiveInsertPort`, I-55) | the G9b availability uses `SelectiveDepthLayout.Count(system)`; a frontal fondo address is prepared only when `Index < FondoCount` | IDENTICAL | second part of the partial Selective command class (Gate 2 RL-01); checks scoped to the constructor | `CMDI_PARTIAL` `CMDI_AVAIL_COUNT` `CMDI_FRONTAL_FONDO_LT` |
| SEL-PLANTA | V17 1.3, 2.1, 3.7, 3.8 | planta inside (maximum envelope, 1.3; first cut, 2.1); canonical section -1; RUN; `A_k` includes planta | IDENTICAL | - | `V17_INSIDE` `V17_21` `V17_AK` `V17_PLANTA_ROW` |
| SEL-PLANTA | Freeze V18 7; post-I-57 4; Foundation 4; ADR-0036 | same statements as above | IDENTICAL | by reference | `V18_S7` `P57_S4` `FND_S4` `ADR_NEW_D4` |
| SEL-PLANTA | bound SHA: SelectiveViewAvailabilityFacts, DecodeSelective | whole-rack variant available when the view kind is planta (the `Whole` case of the switch); the Selective decoder decodes planta to `Whole` | IDENTICAL | scoped; the Whole case is bound by the regex | `RVA_SEL_WHOLE_PLANTA` `COD_SEL_PLANTA` |
| SEL-LATERAL | V17 2.2, 1.3 (L-01), 3.7 | laterals of Selectivo out (E5); the lateral row does not expose `mu_k` (MC) | IDENTICAL | - | `V17_22` `V17_L01` `V17_LAT_MC` |
| SEL-LATERAL | ADR-0036 accepted negative consequences (both blobs) | the first cut reflects no laterals | IDENTICAL | - | `ADR_NEW_LAT` `ADR_OLD_LAT` |
| SEL-LATERAL | bound SHA: SelectiveViewAvailabilityFacts, DecodeSelective | a post variant is available when the post index exists; the decoder decodes a lateral to `Post` | NARROWER | the scope excludes what availability allows; V17 3.8: `A_k` = availability intersected with exposure | `RVA_SEL_POST` `COD_SEL_LATERAL_POST` |
| SEL-LATERAL | bound SHA: RackViewExposure (I-55 product exposure) | for a `SelectiveRack` the product exposes frontal `Fondo`, lateral `Post` and planta `Whole` to its four product operations | NARROWER | the mirror scope excludes the lateral that the product exposes (Gate 2 RL-01: CHANGED_BUT_CONTRACT_COMPATIBLE); scoped to the `SelectiveRack` case | `EXP_SEL_ADDR` |
| SEL-LEGACY-M1 | V17 3.7 | legacy frontal `-1` exposed and canonicalized to fondo 0 | NARROWER | CT-21D does not exercise it | `V17_M1` |
| SEL-LEGACY-M1 | ADR-0036 decision 4 | the Selective frontal with `-1` becomes fondo 0 | NARROWER | - | `ADR_M1` |
| SEL-LEGACY-M1 | bound SHA: DecodeSelective, RackSelectivoCommands, RackSelectivoInsertIntegration | a frontal with a negative section is coerced to `Fondo(0)` (`SelectiveNegativeFondoToZero`); a block without a fondo address is fondo 0; the G9b redraw of a sibling without a fondo address uses fondo 0 | NARROWER | scoped; the `Section < 0` -> `Fondo(0)` branch and the `PrepareMember` fallback are bound by regexes (Gate 2: CHANGED_BUT_CONTRACT_COMPATIBLE, still NARROWER) | `COD_SEL_NEG_FONDO0` `CMD_LEGACY` `CMDI_PREPARE_FONDO0` |
| SEL-LEGACY-BLANK-NAME | decisions section 223.2 (AR6-02, FXB-06); section 220.2 (AR5-11) by reference | a legacy blank logical `Name` is inside the scope as a legacy product state, distinct from the V2.1 25.4 exclusion; `FX-ANN-BLANK` is a legacy-state fixture (fixture ruling A) | IDENTICAL | by ruling; the scope line of section 1 states exactly this | `DEC223_FXB06` `DEC223_FIXTURE_A` |
| rack-level conditions | V17 1.3, L-11, L-12, L-13, L-14 | corner fondos, linked or dependent half-frentes, topes, explicit side protector, dependent deflector, overflowing grate or pallets: fail closed | IDENTICAL | inherited by reference; CT-21D fixtures avoid them (BA-08 V6 section 12) | `V17_L11` `V17_L12` `V17_L13` `V17_L14` |
| rack-level conditions | ADR-0036 accepted negative consequences | Selectivos with topes, half-frentes whose width depends on a linked property, overflowing grate or pallet rows: fail closed until a new decision | IDENTICAL | - | `ADR_NEW_TOPES` `ADR_NEG_MEDIO` `ADR_NEG_PARRILLA` |
| authority of V17 3.7 / 3.8 | Foundation reconciliation 3; post-I-57 2 item 2 | V17 3.3, 3.7, 3.8, 5.1, ... are `HISTORICAL ONLY` for their neutral plans (`SUPERSEDE_LOCAL_NEUTRAL_PLAN`); post-I-57 limits the substitution to the ownership or location of AUTH-01..13 and keeps V17 in full outside it | NO_STATEMENT | authority-status row (rule of this section, AR3-28): it states the status of V17 3.7 / 3.8, not the scope. Resolution: the historical-only classification applies to the neutral plans; the view-exposure authority stays with I-52 (Foundation 4; post-I-57 2 and 4; Freeze V18 7; section 3.1); that agreement is counted in the SEL-FRONTAL and SEL-PLANTA rows | `FND_S3_HIST` `FND_S3_ROWS` `FND_S4` `P57_S2_ITEM2` `P57_S2_CLOSE` `V18_S7` `P57_S4` |
| ownership of the exposure | Freeze V17 post-I-57, full text (sections 1 to 8) | no section changes the Selective view exposure; section 4 assigns the exposure policy and the admitted view/kind combinations to I-52; section 7 makes a change of the supported kind/view matrix an invalidation trigger | NO_STATEMENT | authority-status row (rule of this section, AR3-28). Basis: the section-by-section reading (section 4 of this artifact, table); V2.2 PW-3: a keyword search is not a complete review. The consensus writes its exposure vocabulary in English, only in its own sections 4 ("exposure policy", "view/kind", line 71) and 7 ("kind/view", line 133), which the reading classifies; neither changes the Selective view exposure. The zero counts of the PW-3 keywords are literally true and only supplementary | `P57_S4` `P57_S7` `P57_EXPOSURE_ONE` `P57_KINDVIEW_TWO` `P57_ZERO_FRONTAL` `P57_ZERO_PLANTA` `P57_ZERO_LATERAL` `P57_ZERO_SECTION` `P57_ZERO_EXPOSICION` `P57_ZERO_AK` `P57_ZERO_SELECTIV` |
| product exposure of I-55 (`RackViewExposure`) | bound SHA: RackViewExposure.cs; decisions section 223.3 (AR6-05) | "Product exposure only"; `RackViewProductOperation` has exactly four members (`CreateFirst`, `InsertSibling`, `Batch`, `GroupProjection`) and no mirror member | NO_STATEMENT | authority-status row (rule of this section, AR3-28): it is a product-exposure authority, not the Selective (mirror) view exposure of V17 3.7 / 3.8, Freeze V18 7 and ADR-0036 decision 4. Resolution (AR6-05, G2-CR-02): it is **not** a change of the Selective view exposure in the sense of V2.2 PA-14, so NB-1 stays open and is **not** reopened; physical existence stays with `RackViewAvailability` | `EXP_PRODUCT_ONLY` `EXP_FOUR_OPS` `EXP_NO_MIRROR` |
| O-1 (historical disposition) | Freeze V18 10 | `O1_DISPOSITION = REQUIRES_REDECISION` | NO_STATEMENT | authority-status row (rule of this section, AR3-28); superseded by the O-1 V18 redecision of the next row | `V18_S10` |
| O-1 V18 (registered disposition) | decisions section 117; ADR-0036 current blob | `O-1 V18 = ACCEPTED / REGISTERED`; product disposition `DEFERRED`; a separate investigation of `ContextIsolationAuthority` authorized | NO_STATEMENT | authority-status row (rule of this section, AR3-28); defers the product; does not change the Selective exposure or the envelope: no conflict (section 5) | `DEC117_O1` `DEC117_DEFERRED` `ADR_O1V18` |
| ADR-0036 amendment for V18 | ADR-0036 current blob | the amendment records the CT-49 / session-context delta, the O-1 V18 registration and the G3 reopening sequence; exposure statements identical in both blobs | NO_STATEMENT | authority-status row (rule of this section, AR3-28); the amendment does not touch view exposure | `ADR_V18` `ADR_O1V18` `ADR_NEW_D4` `ADR_OLD_D4` |
| mirror naming (I-52 vs I-60) | decisions section 223.1 | `NAMING_RULING = NAMING_COMPATIBLE_NO_CHANGE`; the mirror keeps the V17 name `<base> - espejo`; `I60_NAMING_FOR_RACKMIRROR = NOT_YET_ADOPTED` | NO_STATEMENT | authority-status row (rule of this section, AR3-28); it fixes the mirror name, not a scope row; it does not change the scope of the legacy blank-name row | `DEC223_NAMING` |

**Result: 0 rows with `CONFLICT`** (28 rows: 16 `IDENTICAL`, 5 `NARROWER`, 7 `NO_STATEMENT`).

### 3.1 The Foundation `HISTORICAL ONLY` classification

The Foundation reconciliation (section 3, zero-duplication audit) classifies V17 sections 3.3, 3.7, 3.8, 5.1, 5.4, 6.1, 8.1..8.5, 14.2, 15.4 and 20.1 as `HISTORICAL ONLY`, because they were marked `PROVISIONAL UNTIL CROSS-INITIATIVE RECONCILIATION`, and it supersedes prospectively their **neutral plans** (`SUPERSEDE_LOCAL_NEUTRAL_PLAN`).
The most direct textual limit is the post-I-57 consensus, section 2 item 2: the Foundation reconciliation "sustituye prospectivamente solo la propiedad o ubicacion provisional de AUTH-01..13 en V17", and "fuera de ese alcance, Proposal V17 conserva su significado completo" (checks `P57_S2_ITEM2`, `P57_S2_CLOSE`). The same reconciliation keeps with I-52 the "exposure of views and supported combinations" (section 4); the post-I-57 consensus freezes the exposure policy as an I-52 authority (section 4); Freeze V18 section 7 inherits "exposure, supported combinations" from V17. The Selective rows of V17 3.7 and 3.8 are therefore used as the **exposure statement**, not as a neutral plan. Result: **no conflict**. The row "authority of V17 3.7 / 3.8" of section 3 is an authority-status row and takes `NO_STATEMENT` under the rule of section 3; this resolution is its `NOTE`.

**Observation (not a conflict).** The neutral `RackViewCodec` is more permissive than the mirror scope (for a Selective rack it coerces a non-lateral non-planta `View` to a frontal fondo address). The I-55 product exposure `RackViewExposure` is also wider than the mirror scope (it exposes the Selective lateral to the product operations). The mirror's fail-closed policy (V17 3.7) applies above both; the scope of this artifact is the narrower V17 policy.

## 4. Complete reading of the post-I-57 consensus (this run)

The full text (identity `P57` of section 2) was read section by section:

| Section | Finding | Checks |
|---|---|---|
| 1 Autoridades exactas | identities of the frozen authorities only | none |
| 2 Precedencia | item 2: the Foundation reconciliation replaces prospectively only the provisional ownership or location of AUTH-01..13 in V17; closing: no reconciliation changes product policy implicitly, and outside the replaced scope V17 keeps its full meaning | `P57_S2`, `P57_S2_ITEM2`, `P57_S2_CLOSE` |
| 3 Shared View Foundation consumida por referencia | AUTH-01..13 consumed from main; no exposure statement | none |
| 4 Autoridades propias de I-52 | the mirror read-set, the exposure policy and the admitted view/kind combinations are I-52 authorities, written in English ("mirror read-set, exposure policy y combinaciones view/kind admitidas", line 71); the rest is frozen by reference to V17 | `P57_S4`, `P57_EXPOSURE_ONE`, `P57_KINDVIEW_TWO` |
| 5 RS-3 / PlanReadSet | no exposure statement | none |
| 6 Gates y resultados pendientes | no exposure statement | none |
| 7 Invalidation y enmiendas | a change of the supported kind/view matrix ("la matriz kind/view soportada", line 133) invalidates the Freeze | `P57_S7`, `P57_KINDVIEW_TWO` |
| 8 Recheck exacto de publicacion | procedural; no exposure statement | none |

The conclusion rests on this **section-by-section reading**, not on a keyword search (V2.2 PW-3: a keyword search is not a complete review and is not sufficient for sealing). The post-I-57 consensus writes its exposure vocabulary in **English**: "exposure policy" (section 4, line 71; `P57_EXPOSURE_ONE`: exactly one case-insensitive occurrence of "exposure" in the whole text) and "view/kind" / "kind/view" (sections 4 and 7, lines 71 and 133; `P57_KINDVIEW_TWO`: exactly two occurrences). Both are in sections the reading classifies: section 4 assigns the exposure policy and the admitted view/kind combinations to I-52, and section 7 makes a change of the supported kind/view matrix an invalidation trigger; neither changes the Selective view exposure. The zero counts of the PW-3 keywords (frontal, planta, lateral, `Section`, exposición, `A_k`) and of "Selectiv" (checks `P57_ZERO_*`) are literally true and only supplementary: they cannot detect an exposure statement written in English. No section changes the Selective view exposure.

## 5. Relation to O-1, to the envelope and to ADR-0036

- **(a) The registered O-1 disposition.** Freeze V18 section 10 recorded `O1_DISPOSITION = REQUIRES_REDECISION`. The Owner has since **redecided**: decisions section 117 records **`O-1 V18 = ACCEPTED / REGISTERED`** (2026-09-21), with the product disposition **`DEFERRED`** and a separate investigation of `ContextIsolationAuthority` authorized; ADR-0036 records the same. O-1 V18 defers the product and does **not** change the Selective exposure or the envelope: **no conflict** with this scope.
- **(b) Any future O-1 redecision.** Sealing this scope **replaces neither O-1 V18 nor any later redecision**. If a future redecision removes Selective frontal or planta from the envelope, or otherwise changes the envelope, this artifact **reopens**.
- The scope is a **characterization subset** of the V17 / Freeze V18 envelope; it **does not exceed** it.
- ADR-0036 is `PROPOSED / AMENDED FOR V18`; the amendment records the CT-49 / session-context delta, the O-1 V18 registration and the G3 reopening sequence; it does not touch view exposure (checks `ADR_V18`, `ADR_O1V18`, `ADR_NEW_D4`, `ADR_OLD_D4`).

## 6. Seal conditions (V2.2 PA-14) and where each is recorded

| # | Condition | Where it is recorded | State |
|---|---|---|---|
| 1 | Coordinator ratification citing the exact `CANDIDATE_HASH` | the ratification record (BA-07 V6) and the BA-11 registry entry | PENDING |
| 2 | Architect ratification citing the exact `CANDIDATE_HASH` | the ratification record and the BA-11 registry entry | PENDING |
| 3 | dated re-execution of the comparison against the then-current identities | **outside this artifact**: the output of the seal-time run (written to a path given as the runner's mandatory argument, outside the artifact paths), cited by the ratification records and the BA-11 registry entry. The runner compares every resolved identity with the expected identity of the specification (= section 2), resolving the code sources at the bound SHA and at the then-current `origin/main`, and exits with status 1 on any identity drift, any failed check or control, or an unresolvable bound SHA. The re-execution includes the product-exposure row of I-55 (`EXP`; G2-CR-02). A non-zero exit, a non-empty `identityDrift` or a non-empty `failed` means the candidate is re-prepared as a new version. The comparison with section 2 is made by the runner, not by hand; the ratification records cite the output's `identityDrift` and `failed` fields | this candidate run: 2026-09-30T20:17:10Z; exit status 0; `identityDrift` = []; `failed` = []; bound SHA resolved |
| 4 | complete reading of the post-I-57 consensus | this run: section 4; the seal-time repetition is recorded outside this artifact, with condition 3 | done for this run |
| 5 | no `CONFLICT` | section 3 | holds (0 rows) |
| 6 | sealed artifact with its hash | the BA-11 registry entry (`SEAL_HASH` = `CANDIDATE_HASH`); **this file never records its own seal** | PENDING |
| 7 | explicit statement that sealing does not replace the O-1 disposition | section 5 | included |

The authoritative state of this artifact is its **BA-11 registry entry**; the status text of this file is frozen at candidate time (AR2-44). The "ready in its bytes" declaration of BA-03 V4 is withdrawn (decisions section 223.4); no `CANDIDATE_HASH` existed.

## 7. Appendix: reproducible checks

The exact executed patterns, the controls, the source locators, the **expected identities** and the **bound product SHA** are published, without any display transformation, in `docs/automation/evidence/I-52-ct21d-ba03-v5-checks.json`. The committed runner `docs/automation/evidence/I-52-ct21d-ba03-v5-checks.py`:

- resolves every source identity and compares it with the expected identity; a code source (kind `BOUND`) is resolved at the bound SHA and at `origin/main`, and, when a second argument is given, at that build source commit; a difference, or an identity that cannot be resolved, is an **identity drift** (`identityDrift`) and fails the run; an unresolvable bound SHA (or build source SHA) fails the run;
- evaluates each check and its **positive control** (must match), its **negative control** (must not match) and, where defined, its **near-miss controls** (must not match), so a vacuous or non-discriminating pattern fails. The scoped regexes bind their discriminating element: `RVA_SEL_WHOLE_PLANTA` (`Whole` -> `Fondo`), `COD_SEL_NEG_FONDO0` (`Fondo(0)` -> `Fondo(1)`), `CMDI_AVAIL_COUNT` (`SelectiveDepthLayout.Count` -> `DepthCount`), `CMDI_FRONTAL_FONDO_LT` (`<` -> `<=`), `CMDI_PREPARE_FONDO0` (`0` -> `1`), `EXP_FOUR_OPS` (a fifth member; three members) and `EXP_SEL_ADDR` (lateral `Post` -> `Whole`);
- restricts a scoped check to its scope; a scope whose start or end marker is absent is **empty** (fail closed);
- records the SHA-256 (LF-normalized bytes) of the specification and of the runner in its output;
- takes the output path as a **mandatory argument** (exit status 2 without it) and writes no other file.

The 54 checks of BA-03 V4 are carried **byte-identical** from the V4 specification (same ids, patterns, controls and notes); the 12 new checks follow them. Reproduce with `python docs/automation/evidence/I-52-ct21d-ba03-v5-checks.py <output.json> [<build-source-sha>]` from the repository root. The output of this candidate run is `docs/automation/evidence/I-52-ct21d-ba03-v5-checks-output.json` (an artifact path). This run executed the specification with SHA-256 `c958cb2021ab072b0b6fecb609bebb7819cf0aba00c6c09d5a23be6275cc93c3` and the runner with SHA-256 `fad48597e9361a0f2a885128cc428667cb6e28a5004532905f7a2013f1d47dd9` (recorded in the output), before their commit; the committed files are the executed ones when their file hashes (BA-11 section 1) equal these values.

| Check | Source | Kind | Result | Positive control | Negative control | Near-miss controls | Note |
|---|---|---|---|---|---|---|---|
| `V17_INSIDE` | `V17` | substring | TRUE | TRUE | FALSE | - | V17 1.3 maximum envelope; cited for SEL-FRONTAL and SEL-PLANTA (BA03V3-N6) |
| `V17_AK` | `V17` | substring | TRUE | TRUE | FALSE | - | includes "; planta" (BA03-F6) |
| `V17_21` | `V17` | substring | TRUE | TRUE | FALSE | - | V17 2.1 (BA03-F6) |
| `V17_22` | `V17` | substring | TRUE | TRUE | FALSE | - | V17 2.2 (BA03-F6) |
| `V17_FRONTAL_RUN` | `V17` | regex | TRUE | TRUE | FALSE | - | V17 3.7: the frontal row exposes mu_k (RUN) (BA03-F4) |
| `V17_PLANTA_ROW` | `V17` | regex | TRUE | TRUE | FALSE | - | V17 3.7: planta, canonical section -1, RUN (BA03-F6) |
| `V17_M1` | `V17` | substring | TRUE | TRUE | FALSE | - | - |
| `V17_L01` | `V17` | regex | TRUE | TRUE | FALSE | - | - |
| `V17_LAT_MC` | `V17` | regex | TRUE | TRUE | FALSE | - | - |
| `V17_L11` | `V17` | regex | TRUE | TRUE | FALSE | - | - |
| `V17_L12` | `V17` | regex | TRUE | TRUE | FALSE | - | restored (BA03-F4) |
| `V17_L13` | `V17` | regex | TRUE | TRUE | FALSE | - | - |
| `V17_L14` | `V17` | regex | TRUE | TRUE | FALSE | - | restored (BA03-F4) |
| `V18_S3` | `V18` | substring | TRUE | TRUE | FALSE | - | - |
| `V18_S7` | `V18` | substring | TRUE | TRUE | FALSE | - | - |
| `V18_S10` | `V18` | substring | TRUE | TRUE | FALSE | - | - |
| `DEC117_O1` | `DEC117` | substring | TRUE | TRUE | FALSE | - | O-1 V18 registered (BA03-F1) |
| `DEC117_DEFERRED` | `DEC117` | substring | TRUE | TRUE | FALSE | - | BA03-F1 |
| `ADR_O1V18` | `ADR` | substring | TRUE | TRUE | FALSE | - | BA03-F1 |
| `P57_S2` | `P57` | substring | TRUE | TRUE | FALSE | - | - |
| `P57_S2_ITEM2` | `P57` | regex | TRUE | TRUE | FALSE | - | BA03-F8 |
| `P57_S2_CLOSE` | `P57` | regex | TRUE | TRUE | FALSE | - | BA03-F8 |
| `P57_S4` | `P57` | substring | TRUE | TRUE | FALSE | - | - |
| `P57_S7` | `P57` | substring | TRUE | TRUE | FALSE | - | - |
| `P57_ZERO_FRONTAL` | `P57` | regex_count flags=I =0 | TRUE | TRUE | FALSE | - | PW-3 keyword list; supplementary, not the basis of row "ownership of the exposure" (BA03V3-N5) |
| `P57_ZERO_PLANTA` | `P57` | regex_count flags=I =0 | TRUE | TRUE | FALSE | - | - |
| `P57_ZERO_LATERAL` | `P57` | regex_count flags=I =0 | TRUE | TRUE | FALSE | - | - |
| `P57_ZERO_SECTION` | `P57` | regex_count flags=I =0 | TRUE | TRUE | FALSE | - | - |
| `P57_ZERO_EXPOSICION` | `P57` | regex_count flags=I =0 | TRUE | TRUE | FALSE | - | - |
| `P57_ZERO_AK` | `P57` | regex_count =0 | TRUE | TRUE | FALSE | - | - |
| `P57_ZERO_SELECTIV` | `P57` | regex_count flags=I =0 | TRUE | TRUE | FALSE | - | - |
| `P57_EXPOSURE_ONE` | `P57` | regex_count flags=I =1 | TRUE | TRUE | FALSE | - | English exposure vocabulary: exactly one occurrence, in section 4 (BA03V3-N5) |
| `P57_KINDVIEW_TWO` | `P57` | regex_count flags=I =2 | TRUE | TRUE | FALSE | - | English kind/view vocabulary: exactly two occurrences, in sections 4 and 7 (BA03V3-N5) |
| `FND_S3_HIST` | `FND` | substring | TRUE | TRUE | FALSE | - | - |
| `FND_S3_ROWS` | `FND` | substring | TRUE | TRUE | FALSE | - | - |
| `FND_S4` | `FND` | substring | TRUE | TRUE | FALSE | - | - |
| `ADR_NEW_D4` | `ADR` | regex | TRUE | TRUE | FALSE | - | extended to "para Selectivo" (BA03-F6) |
| `ADR_OLD_D4` | `ADR0` | regex | TRUE | TRUE | FALSE | - | - |
| `ADR_NEW_LAT` | `ADR` | substring | TRUE | TRUE | FALSE | - | - |
| `ADR_OLD_LAT` | `ADR0` | substring | TRUE | TRUE | FALSE | - | - |
| `ADR_NEW_TOPES` | `ADR` | substring | TRUE | TRUE | FALSE | - | - |
| `ADR_NEG_MEDIO` | `ADR` | substring | TRUE | TRUE | FALSE | - | restored (BA03-F4) |
| `ADR_NEG_PARRILLA` | `ADR` | substring | TRUE | TRUE | FALSE | - | restored (BA03-F4) |
| `ADR_M1` | `ADR` | substring | TRUE | TRUE | FALSE | - | - |
| `ADR_V18` | `ADR` | substring | TRUE | TRUE | FALSE | - | - |
| `RVA_SEL_WHOLE_PLANTA` | `RVA` | regex (scoped) flags=S | TRUE | TRUE | FALSE | FALSE | scoped to SelectiveViewAvailabilityFacts; binds the Whole case; near miss Whole -> Fondo (BA03V3-N2) |
| `RVA_SEL_FONDO` | `RVA` | substring (scoped) | TRUE | TRUE | FALSE | - | scoped |
| `RVA_SEL_POST` | `RVA` | substring (scoped) | TRUE | TRUE | FALSE | - | scoped |
| `SDL_COUNT` | `SDL` | substring | TRUE | TRUE | FALSE | - | - |
| `CMD_FONDOCOUNT` | `CMD` | substring | TRUE | TRUE | FALSE | - | - |
| `CMD_LEGACY` | `CMD` | substring | TRUE | TRUE | FALSE | - | - |
| `COD_SEL_NEG_FONDO0` | `COD` | regex (scoped) flags=S | TRUE | TRUE | FALSE | FALSE | scoped to DecodeSelective; binds Section < 0 -> Fondo(0); near miss Fondo(0) -> Fondo(1) (BA03V3-N2) |
| `COD_SEL_PLANTA` | `COD` | substring (scoped) | TRUE | TRUE | FALSE | - | scoped |
| `COD_SEL_LATERAL_POST` | `COD` | substring (scoped) | TRUE | TRUE | FALSE | - | scoped |
| `CMD_PARTIAL` | `CMD` | substring | TRUE | TRUE | FALSE | - | the Selective command class is partial at the bound SHA (Gate 2 RL-01) |
| `CMDI_PARTIAL` | `CMDI` | substring | TRUE | TRUE | FALSE | - | second part of the same class (I-55); in the identity set (AR6-07, 223.4) |
| `CMDI_AVAIL_COUNT` | `CMDI` | regex (scoped) flags=S | TRUE | TRUE | FALSE | FALSE | scoped to the SelectiveInsertPort constructor; the G9b availability uses SelectiveDepthLayout.Count; near miss Count -> DepthCount |
| `CMDI_FRONTAL_FONDO_LT` | `CMDI` | regex (scoped) flags=S | TRUE | TRUE | FALSE | FALSE | scoped; a frontal fondo address is prepared only when Index < FondoCount; near miss < -> <= |
| `CMDI_PREPARE_FONDO0` | `CMDI` | regex (scoped) flags=S | TRUE | TRUE | FALSE | FALSE | scoped to PrepareMember; a frontal without a fondo address is redrawn as fondo 0; near miss 0 -> 1 |
| `EXP_PRODUCT_ONLY` | `EXP` | substring | TRUE | TRUE | FALSE | - | G2-CR-02: product exposure, not the mirror exposure |
| `EXP_FOUR_OPS` | `EXP` | regex (scoped) flags=S | TRUE | TRUE | FALSE | FALSE, FALSE | G2-CR-02: the enum has exactly four members (near misses: a fifth member; three members) |
| `EXP_NO_MIRROR` | `EXP` | regex_count flags=I =0 | TRUE | TRUE | FALSE | - | G2-CR-02: no mirror member or mention in the product exposure |
| `EXP_SEL_ADDR` | `EXP` | regex (scoped) flags=S | TRUE | TRUE | FALSE | FALSE | scoped to the SelectiveRack case; frontal Fondo, lateral Post, planta Whole; near miss lateral Post -> Whole |
| `DEC223_FIXTURE_A` | `DEC223` | substring | TRUE | TRUE | FALSE | - | AR6-02 |
| `DEC223_FXB06` | `DEC223` | substring | TRUE | TRUE | FALSE | - | FXB-06 |
| `DEC223_NAMING` | `DEC223` | substring | TRUE | TRUE | FALSE | - | 223.1; I60_NAMING_FOR_RACKMIRROR = NOT_YET_ADOPTED |

## 8. Delta V6 (decisions 220 and 223)

This is the re-link round V6; the artifact version is 5. The delta of V4 (decisions section 214) stays in the V4 blob.

| Ruling / finding | Decision | Fix (section / field) |
|---|---|---|
| AR6-01 | bound product SHA `95690c28` for all static facts; never a tuple value | header `BOUND_PRODUCT_SHA`; specification field `bound_product_sha`; section 2 (code sources of kind `BOUND`); section 7 |
| AR6-07 / 223.4 row BA-03, Gate 2 RL-01 | V5 with identities at `95690c28`, the enlarged set and the legacy blank-name scope line; the 54 checks of V4 stay true | section 2: `CMD` = `f8e514a0` (was `a22e6a6a`); `CMDI` (`RackSelectivoInsertIntegration.cs`, `308a42e5`) and `EXP` (`RackViewExposure.cs`, `81c714ca`) added with scoped checks (`CMD_PARTIAL`, `CMDI_*`, `EXP_*`); the 54 V4 checks are carried byte-identical from the V4 specification and are TRUE; identity rule rewritten for `3375aadb..95690c28` |
| AR6-02 / FXB-06 | legacy blank logical `Name` = IN SCOPE as a legacy product state, distinct from V2.1 25.4 | section 1: row `SEL-LEGACY-BLANK-NAME` and its paragraph; section 3 scope row citing `DEC223_FXB06`, `DEC223_FIXTURE_A` (source `DEC223` added) |
| AR6-02 / FXB-03 | the fixture construction build `69daf03a` is outside the AR5-12 precondition | section 2 identity rule (the `CMD` blob differs at `69daf03a`; that build is never a build under test); runner docstring |
| AR6-05 / G2-CR-02 | `RackViewExposure` is not a PA-14 exposure change; scoped check that the enum has exactly four members | section 3 rows "product exposure of I-55" (`NO_STATEMENT`, `EXP_PRODUCT_ONLY`, `EXP_FOUR_OPS`, `EXP_NO_MIRROR`) and SEL-LATERAL (`NARROWER`, `EXP_SEL_ADDR`); section 6 condition 3 |
| 223.1 (naming ruling) | `NAMING_COMPATIBLE_NO_CHANGE`; `I60_NAMING_FOR_RACKMIRROR = NOT_YET_ADOPTED` recorded, not re-decided | section 1 (the mirror name stays `<base> - espejo`); section 3 authority-status row "mirror naming" (`DEC223_NAMING`) |
| AR5-12 | the BA-03 runner detects a drift **only of the bound (path, blob) pairs of BA-03 itself** (the six `BOUND` code sources of section 2: `RVA`, `SDL`, `CMD`, `COD`, `CMDI`, `EXP`) and fails closed; it does **not** cover the reused-route pairs of BA-06 V6 section 2.1 (the route rows) or of BA-08 V6 section 3.2 (the W and C rows), whose coverage is the open Architect question AQ-V6-01 (section 8.1) | runner: `BOUND` sources resolved at the bound SHA and at `origin/main`; optional second argument = build source SHA (third resolution); exit 1 on any drift or unresolvable SHA; section 7; section 8.1 (AQ-V6-01) |
| AR5-15, 223.4 (candidate status) | the V4 "ready in its bytes" declaration is withdrawn; no `CANDIDATE_HASH` existed | header `STATUS AUTHORITY` and closing block (V5 is a DRAFT; its status is the BA-11 registry entry) |
| AR5-13, AR6-05 | contract code facts re-dated only by V2.6 | header `AUTHORITY_CONTRACT` cites V2.6 (draft); the live code identities of V2.1 section 25 are those of section 2 of this artifact |
| correction pass (V6) | critic gap of round V6: the AR5-12 row claimed that the runner detects the precondition for every cited pair; it covers only the bound pairs of BA-03 itself | AR5-12 row of this table corrected; open Architect question AQ-V6-01 added (section 8.1) and to the header `PREREQUISITES` (before candidate). The specification and the runner are unchanged (same bytes and identities); the runner was not re-run, and the candidate run output is unchanged |

Other V5 changes, required by the file and header conventions of the round and not by a ruling: the header block (`ARTIFACT_VERSION`, `ARTIFACT_PATHS`, `AUTHORITY_CONTRACT`, `BOUND_PRODUCT_SHA`, `DELTA RULING APPLIED`, `I60_NAMING_FOR_RACKMIRROR`); the code identities are labelled "bound SHA" instead of "main"; sibling references renamed (BA-07 V6, BA-08 V6; BA-11 cited by its registry entry). Section 5 and section 4 are unchanged; the section numbering is unchanged.

### 8.1 Open items (Architect question of round V6; recorded, not decided here)

| Id | Question | Gate |
|---|---|---|
| AQ-V6-01 | AR5-12 (decisions section 220) makes the BA-03 runner the fail-closed detector of the preparation precondition for every (path, blob) pair of the reused product route that BA-03, BA-06 and BA-08 cite. The V5 specification and runner cover **BA-03's own bound pairs only** (the six `BOUND` code sources of section 2); no runner compares the reused-route pairs of **BA-06 V6 section 2.1** (the route rows; precedent rows excluded) or of **BA-08 V6 section 3.2** (the W and C rows) with a build source SHA. The Architect rules where that detection lives: (a) the BA-03 specification and runner gain those pairs as `BOUND` sources (a new BA-03 version, with new specification and runner identities), or (b) a dedicated precondition runner, with the reading of "el runner de BA-03" in AR5-12 and the `runnerOutput` field of BA-07 V6 section 5.4 re-pointed to it. Until the ruling, the output of this runner is evidence of the AR5-12 precondition for BA-03's pairs only | before candidate (header `PREREQUISITES`) |

```text
SELECTIVE_VIEW_FAMILY_SCOPE_V1 = DRAFT V5 (re-link round V6; NOT SEALED; NOT RATIFIED; BA-11 registry entry governs)
NB-1 = ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION ; OPEN until conditions 1, 2, 3, 4 (seal-time repetition) and 6 are done
CT21D_AUTHORITY_BASELINE_READY = FALSE
```
