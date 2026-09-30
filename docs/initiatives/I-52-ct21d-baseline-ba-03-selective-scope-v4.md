# I-52 — CT-21D Baseline Artifact BA-03: Sealed Selective Scope (DRAFT V4, CANDIDATE PREPARATION)

> **BASELINE ARTIFACT BA-03 V4 — DRAFT prepared for `CANDIDATE_FOR_SEALING`, NOT SEALED, NOT RATIFIED. Design / documentation only.**
>
> ```text
> ARTIFACT_ID               = SELECTIVE_VIEW_FAMILY_SCOPE_V1   (baseline artifact CT21D-BASE-SCOPE-SELECTIVE)
> ARTIFACT_VERSION          = 4-DRAFT (supersedes 3-DRAFT, blob 6157f626c4b1d1811ba2ae4c661e85a7ae22a8be, which stays as history)
> ARTIFACT_PATHS            = docs/automation/evidence/I-52-ct21d-ba03-v4-checks-output.json
>                             docs/automation/evidence/I-52-ct21d-ba03-v4-checks.json
>                             docs/automation/evidence/I-52-ct21d-ba03-v4-checks.py
>                             docs/initiatives/I-52-ct21d-baseline-ba-03-selective-scope-v4.md
>                             (ascending ordinal order; the runner output of the candidate run is an artifact path, AR3-28)
> STATUS AUTHORITY          = the BA-11 registry entry of this artifact is the only authority for its status and hashes;
>                             this header is frozen, non-authoritative text of the candidate bytes (BA-11 V4 section 2)
> AUTHORITY_CONTRACT        = CT-21D V2.1 + V2.2 + V2.3 + V2.4 (draft revision 3) + V2.5 (draft revision 1); V2.4 and V2.5 pending
>                             Coordinator textual verification
> DEPENDS_ON                = BA-01 (BA-11 V4 section 2.1)
> PREREQUISITES             = before candidate: none beyond the authority identities of section 2 (bound by the specification;
>                             a drift re-prepares the candidate); before seal: V2.2 PA-14 conditions 3 and 4 (dated re-execution
>                             and post-I-57 reading), recorded outside this artifact (section 6)
> SCOPE AGREEMENT           = scope agreed in ARCHITECT_BASELINE_PHASE1_RULING = AGREED_WITH_REQUIRED_CHANGES over
>                             1711112021394a6b14112b1787abff4d3062e064; this is NOT a ratification (BA-11: RATIFIED needs the Coordinator's
>                             and the Architect's records citing the exact CANDIDATE_HASH)
> PHASE-2 RULING APPLIED    = BA03-F1..F8 and XC-F9 (decisions section 211, AR2-45)
> DELTA RULING APPLIED      = decisions section 214 (AR3-28: BA03V3-N1..N6; AR3-25: DEPENDS_ON and PREREQUISITES of this header)
> CANDIDATE EVIDENCE RUN    = 2026-09-30T09:01:40Z (runner output: docs/automation/evidence/I-52-ct21d-ba03-v4-checks-output.json)
> BLOCKER                   = NB-1  (ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION; OPEN until sealed)
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 1. The scope

| Row | Domain | Status for CT-21D |
|---|---|---|
| `SEL-FRONTAL` | `View = frontal`, `Section = k` for `0 <= k < SelectiveDepthLayout.Count(system)` (one per fondo); scenarios cover one fondo and several fondos | **IN** |
| `SEL-PLANTA` | `View = planta` (canonical section `-1`) | **IN** |
| `SEL-LATERAL` | lateral, per post | **OUT** (fail-closed, E5) |
| `SEL-LEGACY-M1` | frontal with the legacy `Section = -1` (or a legacy empty `View`) | **OUT_OF_SCOPE** for CT-21D scenarios |

`Section = -1` is `OUT_OF_SCOPE` (V2.2 PA-14, Q-SEC-1): production canonicalizes it to fondo 0 before planning, so the seams receive and create canonical output; **catalog fixtures use the canonical `Section = k`** (V2.1 25.4). The decision narrows CT-21D only; it neither widens nor narrows the product envelope.

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
| `RVA` | RackViewAvailability.cs | `origin/main` | `src/RackCad.Application/Systems/Shared/RackViewAvailability.cs` | blob `74e1a1aba691e42e03d3744bf6631e444aa1d696` |
| `SDL` | SelectiveDepthLayout.cs | `origin/main` | `src/RackCad.Application/Systems/Selective/SelectiveDepthLayout.cs` | blob `9e45f2f2c90bcd2ac881d4fc2dc3db2debfcd762` |
| `CMD` | RackSelectivoCommands.cs | `origin/main` | `src/RackCad.Plugin/RackSelectivoCommands.cs` | blob `a22e6a6a4dba2ee2b62f2f47f358d9ac975fc486` |
| `COD` | RackViewCodec.cs | `origin/main` | `src/RackCad.Application/Systems/Shared/RackViewCodec.cs` | blob `b63661678a2488449baab37cc74023fff0550165` |

**Identity rule (normative, AR2-45).** The identity set of this artifact is the **(path, blob) pairs** above (and, for `DEC117`, the pair (path, section) with the SHA-256 of the LF-normalized section text). The specification publishes each pair as the **expected identity** of its source (`expected_blob`, or `expected_section_sha256`), and the runner fails on any drift (section 7); this run resolved every identity equal to the expected one (`identityDrift` = []). Branch and `origin/main` commit SHAs are **informative only**: this run resolved them at `aebb86c23d341b6c1ade138c752849c099287c54` (branch) and `1304101d997b66748e21f4896ac203d6fdc6a1f1` (`origin/main`). The four code identities are also equal at the bound historical SHA `3375aadb`: in `src/`, the `3375aadb..1304101d` delta touches only `RackDefinitionCreator.cs` and `RackDefinitionCreationResult.cs`; none of the four compared code paths changes (AR2-46: the reused Selective path is untouched). These facts were verified when this version was prepared, with `git diff --name-only 3375aadb 1304101d -- src/` and `git rev-parse <sha>:<path>` for the four code paths.

## 3. Row-by-row comparison

`RESULT` uses the closed domain of V2.1 25.3: `IDENTICAL`, `NARROWER` (the CT-21D scope is a strict subset of what the authority permits), `CONFLICT` (a contradiction; **a conflict prevents sealing**) or `NO_STATEMENT` (the authority makes no statement about the scope: not a conflict, not counted as agreement). Qualifiers and rationale are in the `NOTE` column. Every row cites checks of the appendix (section 7).

**Rule for authority-status rows (normative, AR3-28).** A row whose first column is not a scope row of section 1 or the rack-level conditions (here: the authority of V17 3.7 / 3.8, the ownership of the exposure, the two O-1 dispositions and the ADR-0036 amendment) only establishes the authority status of another text and makes no statement about a scope row. Its `RESULT` is `NO_STATEMENT`, and its resolution is written in the `NOTE`. It is neither a conflict nor counted as agreement; any agreement it relies on is counted in the scope rows that cite the same authorities.

| Scope row | Authority and section | Statement | RESULT | NOTE | Checks |
|---|---|---|---|---|---|
| SEL-FRONTAL | V17 1.3, 2.1 | Selectivo inside as maximum envelope: frontal by fondo, planta; mu_k = RUN | IDENTICAL | - | `V17_INSIDE` `V17_21` |
| SEL-FRONTAL | V17 3.7, 3.8 | `A_k`: frontal of each fondo `k`, `0 <= k < SelectiveDepthLayout.Count(sistema)`; the frontal row exposes `mu_k` (RUN) | IDENTICAL | - | `V17_AK` `V17_FRONTAL_RUN` |
| SEL-FRONTAL | Freeze V18 3 and 7 | V17 governs what V18 does not replace; exposure and supported combinations preserved | IDENTICAL | by reference to V17 | `V18_S3` `V18_S7` |
| SEL-FRONTAL | Freeze V17 post-I-57, 2 and 4 | no reconciliation changes product policy implicitly; the Foundation substitution is limited to the ownership or location of AUTH-01..13; outside that scope V17 keeps its full meaning; exposure policy and admitted view/kind combinations are an I-52 authority | IDENTICAL | by reference to V17 | `P57_S2` `P57_S2_ITEM2` `P57_S2_CLOSE` `P57_S4` |
| SEL-FRONTAL | Foundation reconciliation 4 | I-52 keeps the exposure of views and supported combinations | IDENTICAL | keeps V17 | `FND_S4` |
| SEL-FRONTAL | ADR-0036 decision 4 (both blobs) | first cut: frontal and planta for Selectivo | IDENTICAL | - | `ADR_NEW_D4` `ADR_OLD_D4` |
| SEL-FRONTAL | main: SelectiveViewAvailabilityFacts, SelectiveDepthLayout, RackSelectivoCommands | fondo address available when `Index < FondoCount`; `fondoCount = SelectiveDepthLayout.Count(system)`; `Count = Math.Max(1, DepthCount)` | IDENTICAL | checks scoped to the Selective members | `RVA_SEL_FONDO` `SDL_COUNT` `CMD_FONDOCOUNT` |
| SEL-PLANTA | V17 1.3, 2.1, 3.7, 3.8 | planta inside (maximum envelope, 1.3; first cut, 2.1); canonical section -1; RUN; `A_k` includes planta | IDENTICAL | - | `V17_INSIDE` `V17_21` `V17_AK` `V17_PLANTA_ROW` |
| SEL-PLANTA | Freeze V18 7; post-I-57 4; Foundation 4; ADR-0036 | same statements as above | IDENTICAL | by reference | `V18_S7` `P57_S4` `FND_S4` `ADR_NEW_D4` |
| SEL-PLANTA | main: SelectiveViewAvailabilityFacts, DecodeSelective | whole-rack variant available when the view kind is planta (the `Whole` case of the switch); the Selective decoder decodes planta to `Whole` | IDENTICAL | scoped; the Whole case is bound by the regex | `RVA_SEL_WHOLE_PLANTA` `COD_SEL_PLANTA` |
| SEL-LATERAL | V17 2.2, 1.3 (L-01), 3.7 | laterals of Selectivo out (E5); the lateral row does not expose `mu_k` (MC) | IDENTICAL | - | `V17_22` `V17_L01` `V17_LAT_MC` |
| SEL-LATERAL | ADR-0036 accepted negative consequences (both blobs) | the first cut reflects no laterals | IDENTICAL | - | `ADR_NEW_LAT` `ADR_OLD_LAT` |
| SEL-LATERAL | main: SelectiveViewAvailabilityFacts, DecodeSelective | a post variant is available when the post index exists; the decoder decodes a lateral to `Post` | NARROWER | the scope excludes what availability allows; V17 3.8: `A_k` = availability intersected with exposure | `RVA_SEL_POST` `COD_SEL_LATERAL_POST` |
| SEL-LEGACY-M1 | V17 3.7 | legacy frontal `-1` exposed and canonicalized to fondo 0 | NARROWER | CT-21D does not exercise it | `V17_M1` |
| SEL-LEGACY-M1 | ADR-0036 decision 4 | the Selective frontal with `-1` becomes fondo 0 | NARROWER | - | `ADR_M1` |
| SEL-LEGACY-M1 | main: DecodeSelective, RackSelectivoCommands | a frontal with a negative section is coerced to `Fondo(0)` (`SelectiveNegativeFondoToZero`); a block without a fondo address is fondo 0 | NARROWER | scoped; the `Section < 0` -> `Fondo(0)` branch is bound by the regex | `COD_SEL_NEG_FONDO0` `CMD_LEGACY` |
| rack-level conditions | V17 1.3, L-11, L-12, L-13, L-14 | corner fondos, linked or dependent half-frentes, topes, explicit side protector, dependent deflector, overflowing grate or pallets: fail closed | IDENTICAL | inherited by reference; CT-21D fixtures avoid them (BA-08 V4 section 12) | `V17_L11` `V17_L12` `V17_L13` `V17_L14` |
| rack-level conditions | ADR-0036 accepted negative consequences | Selectivos with topes, half-frentes whose width depends on a linked property, overflowing grate or pallet rows: fail closed until a new decision | IDENTICAL | - | `ADR_NEW_TOPES` `ADR_NEG_MEDIO` `ADR_NEG_PARRILLA` |
| authority of V17 3.7 / 3.8 | Foundation reconciliation 3; post-I-57 2 item 2 | V17 3.3, 3.7, 3.8, 5.1, ... are `HISTORICAL ONLY` for their neutral plans (`SUPERSEDE_LOCAL_NEUTRAL_PLAN`); post-I-57 limits the substitution to the ownership or location of AUTH-01..13 and keeps V17 in full outside it | NO_STATEMENT | authority-status row (rule of this section, AR3-28): it states the status of V17 3.7 / 3.8, not the scope. Resolution: the historical-only classification applies to the neutral plans; the view-exposure authority stays with I-52 (Foundation 4; post-I-57 2 and 4; Freeze V18 7; section 3.1); that agreement is counted in the SEL-FRONTAL and SEL-PLANTA rows | `FND_S3_HIST` `FND_S3_ROWS` `FND_S4` `P57_S2_ITEM2` `P57_S2_CLOSE` `V18_S7` `P57_S4` |
| ownership of the exposure | Freeze V17 post-I-57, full text (sections 1 to 8) | no section changes the Selective view exposure; section 4 assigns the exposure policy and the admitted view/kind combinations to I-52; section 7 makes a change of the supported kind/view matrix an invalidation trigger | NO_STATEMENT | authority-status row (rule of this section, AR3-28). Basis: the section-by-section reading (section 4 of this artifact, table); V2.2 PW-3: a keyword search is not a complete review. The consensus writes its exposure vocabulary in English, only in its own sections 4 ("exposure policy", "view/kind", line 71) and 7 ("kind/view", line 133), which the reading classifies; neither changes the Selective view exposure. The zero counts of the PW-3 keywords are literally true and only supplementary | `P57_S4` `P57_S7` `P57_EXPOSURE_ONE` `P57_KINDVIEW_TWO` `P57_ZERO_FRONTAL` `P57_ZERO_PLANTA` `P57_ZERO_LATERAL` `P57_ZERO_SECTION` `P57_ZERO_EXPOSICION` `P57_ZERO_AK` `P57_ZERO_SELECTIV` |
| O-1 (historical disposition) | Freeze V18 10 | `O1_DISPOSITION = REQUIRES_REDECISION` | NO_STATEMENT | authority-status row (rule of this section, AR3-28); superseded by the O-1 V18 redecision of the next row | `V18_S10` |
| O-1 V18 (registered disposition) | decisions section 117; ADR-0036 current blob | `O-1 V18 = ACCEPTED / REGISTERED`; product disposition `DEFERRED`; a separate investigation of `ContextIsolationAuthority` authorized | NO_STATEMENT | authority-status row (rule of this section, AR3-28); defers the product; does not change the Selective exposure or the envelope: no conflict (section 5) | `DEC117_O1` `DEC117_DEFERRED` `ADR_O1V18` |
| ADR-0036 amendment for V18 | ADR-0036 current blob | the amendment records the CT-49 / session-context delta, the O-1 V18 registration and the G3 reopening sequence; exposure statements identical in both blobs | NO_STATEMENT | authority-status row (rule of this section, AR3-28); the amendment does not touch view exposure | `ADR_V18` `ADR_O1V18` `ADR_NEW_D4` `ADR_OLD_D4` |

**Result: 0 rows with `CONFLICT`** (23 rows: 14 `IDENTICAL`, 4 `NARROWER`, 5 `NO_STATEMENT`).

### 3.1 The Foundation `HISTORICAL ONLY` classification

The Foundation reconciliation (section 3, zero-duplication audit) classifies V17 sections 3.3, 3.7, 3.8, 5.1, 5.4, 6.1, 8.1..8.5, 14.2, 15.4 and 20.1 as `HISTORICAL ONLY`, because they were marked `PROVISIONAL UNTIL CROSS-INITIATIVE RECONCILIATION`, and it supersedes prospectively their **neutral plans** (`SUPERSEDE_LOCAL_NEUTRAL_PLAN`).
The most direct textual limit is the post-I-57 consensus, section 2 item 2: the Foundation reconciliation "sustituye prospectivamente solo la propiedad o ubicacion provisional de AUTH-01..13 en V17", and "fuera de ese alcance, Proposal V17 conserva su significado completo" (checks `P57_S2_ITEM2`, `P57_S2_CLOSE`). The same reconciliation keeps with I-52 the "exposure of views and supported combinations" (section 4); the post-I-57 consensus freezes the exposure policy as an I-52 authority (section 4); Freeze V18 section 7 inherits "exposure, supported combinations" from V17. The Selective rows of V17 3.7 and 3.8 are therefore used as the **exposure statement**, not as a neutral plan. Result: **no conflict**. The row "authority of V17 3.7 / 3.8" of section 3 is an authority-status row and takes `NO_STATEMENT` under the rule of section 3; this resolution is its `NOTE`.

**Observation (not a conflict).** The neutral `RackViewCodec` is more permissive than the mirror scope (for a Selective rack it coerces a non-lateral non-planta `View` to a frontal fondo address). The mirror's fail-closed policy (V17 3.7) applies above the codec; the scope of this artifact is the narrower V17 policy.

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
| 1 | Coordinator ratification citing the exact `CANDIDATE_HASH` | the ratification record (BA-07 V4 section 2) and the BA-11 entry | PENDING |
| 2 | Architect ratification citing the exact `CANDIDATE_HASH` | the ratification record and the BA-11 entry | PENDING |
| 3 | dated re-execution of the comparison against the then-current identities | **outside this artifact**: the output of the seal-time run (written to a path given as the runner's mandatory argument, outside the artifact paths), cited by the ratification records and the BA-11 entry. The runner compares every resolved identity with the expected identity of the specification (= section 2) and exits with status 1 on any identity drift or any failed check or control. A non-zero exit, a non-empty `identityDrift` or a non-empty `failed` means the candidate is re-prepared as a new version. The comparison with section 2 is made by the runner, not by hand; the ratification records cite the output's `identityDrift` and `failed` fields | this candidate run: 2026-09-30T09:01:40Z; exit status 0; `identityDrift` = []; `failed` = [] |
| 4 | complete reading of the post-I-57 consensus | this run: section 4; the seal-time repetition is recorded outside this artifact, with condition 3 | done for this run |
| 5 | no `CONFLICT` | section 3 | holds (0 rows) |
| 6 | sealed artifact with its hash | the BA-11 entry (`SEAL_HASH` = `CANDIDATE_HASH`); **this file never records its own seal** | PENDING |
| 7 | explicit statement that sealing does not replace the O-1 disposition | section 5 | included |

The authoritative state of this artifact is its **BA-11 entry**; the status text of this file is frozen at candidate time (AR2-44).

## 7. Appendix: reproducible checks

The exact executed patterns, the controls, the source locators and the **expected identities** are published, without any display transformation, in `docs/automation/evidence/I-52-ct21d-ba03-v4-checks.json`. The committed runner `docs/automation/evidence/I-52-ct21d-ba03-v4-checks.py`:

- resolves every source identity and compares it with the expected identity; a difference, or an identity that cannot be resolved, is an **identity drift** (`identityDrift`) and fails the run;
- evaluates each check and its **positive control** (must match), its **negative control** (must not match) and, where defined, its **near-miss controls** (must not match), so a vacuous or non-discriminating pattern fails. `RVA_SEL_WHOLE_PLANTA` and `COD_SEL_NEG_FONDO0` are scoped regexes that bind the discriminating element: the `Whole` case of the Selective availability switch, and the `Section < 0` -> `Fondo(0)` branch of `DecodeSelective`; their near-miss controls change that element (`Whole` -> `Fondo`; `Fondo(0)` -> `Fondo(1)`) inside the scope;
- restricts a scoped check to its scope; a scope whose start or end marker is absent is **empty** (fail closed);
- records the SHA-256 (LF-normalized bytes) of the specification and of the runner in its output;
- takes the output path as a **mandatory argument** (exit status 2 without it) and writes no other file.

Reproduce with `python docs/automation/evidence/I-52-ct21d-ba03-v4-checks.py <output.json>` from the repository root. The output of this candidate run is `docs/automation/evidence/I-52-ct21d-ba03-v4-checks-output.json` (an artifact path). This run executed the specification with SHA-256 `8da090de56a6587f2214b084316761ce3b23bccb6b6c6cdb1b32dcc024608e6b` and the runner with SHA-256 `443e332ea1bb19277f7f6765fd2a873d606c3fc0587d067211ee0d5f9e93f709` (recorded in the output), before their commit; the committed files are the executed ones when their file hashes (BA-11 V4 section 1) equal these values.

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

## 8. Delta V4 (decisions section 214)

| Finding id | Decision | Fix (section / field) |
|---|---|---|
| BA03V3-N1 | AR3-28 (identity rewritten, only `src/`) | section 2, identity rule: "in `src/`, the `3375aadb..1304101d` delta touches only `RackDefinitionCreator.cs` and `RackDefinitionCreationResult.cs`; none of the four compared code paths changes (AR2-46: the reused Selective path is untouched)"; the facts are re-verified by the generator with read-only git |
| BA03V3-N2 | AR3-28 (two checks bounded by a discriminating regex) | specification: `RVA_SEL_WHOLE_PLANTA` and `COD_SEL_NEG_FONDO0` are now scoped regexes (flags S) that bind the `Whole` case and the `Section < 0` -> `Fondo(0)` branch; each has a near-miss control (`Whole` -> `Fondo`; `Fondo(0)` -> `Fondo(1)`) in addition to its scope control; runner: field `near_miss_controls`; section 3 rows SEL-PLANTA (main) and SEL-LEGACY-M1 (main); section 7 |
| BA03V3-N3 | AR3-28 (runner: expected identities and drift failure, spec and runner hashes, mandatory output argument, fail-closed scope; candidate output in the BA-03 paths) | (a) specification: `expected_blob` / `expected_section_sha256` for every source; runner: `identityDrift`, exit 1 on drift; section 6 condition 3. (b) runner: mandatory output argument (exit 2 without it), no default target, docstring corrected. (c) header `ARTIFACT_PATHS`: the runner output is the fourth path of BA-03 (BA-11 V4 section 3). (d) output fields `specification.sha256` and `runner.sha256`; section 7. (e) runner `scope()` returns an empty scope when a marker is absent; specification `rule` says so |
| BA03V3-N4 | AR3-28 (authority-status rows = `NO_STATEMENT`, rule) | section 3: normative rule for authority-status rows; row "authority of V17 3.7 / 3.8" is `NO_STATEMENT` with its resolution in the `NOTE`; rows "ownership of the exposure", "O-1 (historical disposition)", "O-1 V18 (registered disposition)" and "ADR-0036 amendment for V18" carry the same label; the generator asserts the rule |
| BA03V3-N5 | AR3-28 (`NO_STATEMENT` of the exposure row supported by the per-section reading) | section 3 row "ownership of the exposure" `NOTE`; section 4 rows 4 and 7 and closing paragraph; new counted checks `P57_EXPOSURE_ONE` ("exposure", =1) and `P57_KINDVIEW_TWO` ("kind/view\|view/kind", =2) |
| BA03V3-N6 | AR3-28 (`V17_INSIDE` in SEL-PLANTA) | section 3 row SEL-PLANTA / V17: authority "V17 1.3, 2.1, 3.7, 3.8", check `V17_INSIDE` added |
| BA03-F1..F8, XC-F9 | closure FIXED (section 214.1) | NOT_APPLICABLE: closed in V3; no V4 change |
| BA03V3-N5, table syntax of its row here (correction pass, minor) | AR3-28 (editorial; no ruling changed) | FIXED: the text of row BA03V3-N5 of this table writes the counted pattern with an escaped pipe ("kind/view\|view/kind"), so the row keeps its three cells; the regex of `P57_KINDVIEW_TWO`, the specification, the runner and the candidate output are unchanged (the md is regenerated from the published output, without a new run) |

Other V4 changes, required by the V4 file and header conventions and not by a finding: the header block (`ARTIFACT_VERSION`, `ARTIFACT_PATHS`, `AUTHORITY_CONTRACT`, `DEPENDS_ON` and `PREREQUISITES` per AR3-25, `DELTA RULING APPLIED`), and sibling references renamed to V4 (BA-07 V4, BA-08 V4, BA-11 V4). Sections 1 and 5 are unchanged; the section numbering is unchanged, and this section 8 is new.

```text
SELECTIVE_VIEW_FAMILY_SCOPE_V1 = DRAFT V4 (candidate preparation; NOT SEALED; NOT RATIFIED)
NB-1 = ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION ; OPEN until conditions 1, 2, 3, 4 (seal-time repetition) and 6 are done
CT21D_AUTHORITY_BASELINE_READY = FALSE
```
