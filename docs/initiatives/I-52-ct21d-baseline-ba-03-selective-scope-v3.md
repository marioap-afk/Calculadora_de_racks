# I-52 — CT-21D Baseline Artifact BA-03: Sealed Selective Scope (DRAFT V3, CANDIDATE PREPARATION)

> **BASELINE ARTIFACT BA-03 V3 — DRAFT prepared for `CANDIDATE_FOR_SEALING`, NOT SEALED, NOT RATIFIED. Design / documentation only.**
>
> ```text
> ARTIFACT_ID               = SELECTIVE_VIEW_FAMILY_SCOPE_V1   (baseline artifact CT21D-BASE-SCOPE-SELECTIVE)
> ARTIFACT_VERSION          = 3-DRAFT   (supersedes 2-DRAFT, blob 91172f279cd486074eee911cc928f2094c50d267, which stays as history)
> STATUS AUTHORITY          = the BA-11 registry entry of this artifact is the only authority for its status and hashes;
>                             this header is frozen, non-authoritative text of the candidate bytes (BA-11 V3 section 2)
> AUTHORITY_CONTRACT        = CT-21D V2.1 + V2.2 (PA-14) + V2.3 + V2.4
> SCOPE AGREEMENT           = scope agreed in ARCHITECT_BASELINE_PHASE1_RULING = AGREED_WITH_REQUIRED_CHANGES over
>                             1711112021394a6b14112b1787abff4d3062e064; this is NOT a ratification (BA-11: RATIFIED needs the Coordinator's
>                             and the Architect's records citing the exact CANDIDATE_HASH)
> PHASE-2 RULING APPLIED    = BA03-F1..F8 and XC-F9 (decisions section 211, AR2-45)
> CANDIDATE EVIDENCE RUN    = 2026-09-30T06:58:25Z (runner output: docs/automation/evidence/I-52-ct21d-ba03-v3-checks-output.json)
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

**Identity rule (normative, AR2-45).** The identity set of this artifact is the **(path, blob) pairs** above (and, for `DEC117`, the pair (path, section) with the SHA-256 of the LF-normalized section text). Branch and `origin/main` commit SHAs are **informative only**: this run resolved them at `1c3b7a9c626b0c3dc421404866288f8d7450537d` (branch) and `1304101d997b66748e21f4896ac203d6fdc6a1f1` (`origin/main`). The code identities are also equal at the bound historical SHA `3375aadb` (the `3375aadb..1304101d` delta touches only `RackDefinitionCreator.cs` and `RackDefinitionCreationResult.cs`, AR2-46).

## 3. Row-by-row comparison

`RESULT` uses the closed domain of V2.1 25.3: `IDENTICAL`, `NARROWER` (the CT-21D scope is a strict subset of what the authority permits), `CONFLICT` (a contradiction; **a conflict prevents sealing**) or `NO_STATEMENT` (the authority makes no statement about the scope: not a conflict, not counted as agreement). Qualifiers and rationale are in the `NOTE` column. Every row cites checks of the appendix (section 7).

| Scope row | Authority and section | Statement | RESULT | NOTE | Checks |
|---|---|---|---|---|---|
| SEL-FRONTAL | V17 1.3, 2.1 | Selectivo inside as maximum envelope: frontal by fondo, planta; mu_k = RUN | IDENTICAL | - | `V17_INSIDE` `V17_21` |
| SEL-FRONTAL | V17 3.7, 3.8 | `A_k`: frontal of each fondo `k`, `0 <= k < SelectiveDepthLayout.Count(sistema)`; the frontal row exposes `mu_k` (RUN) | IDENTICAL | - | `V17_AK` `V17_FRONTAL_RUN` |
| SEL-FRONTAL | Freeze V18 3 and 7 | V17 governs what V18 does not replace; exposure and supported combinations preserved | IDENTICAL | by reference to V17 | `V18_S3` `V18_S7` |
| SEL-FRONTAL | Freeze V17 post-I-57, 2 and 4 | no reconciliation changes product policy implicitly; the Foundation substitution is limited to the ownership or location of AUTH-01..13; outside that scope V17 keeps its full meaning; exposure policy and admitted view/kind combinations are an I-52 authority | IDENTICAL | by reference to V17 | `P57_S2` `P57_S2_ITEM2` `P57_S2_CLOSE` `P57_S4` |
| SEL-FRONTAL | Foundation reconciliation 4 | I-52 keeps the exposure of views and supported combinations | IDENTICAL | keeps V17 | `FND_S4` |
| SEL-FRONTAL | ADR-0036 decision 4 (both blobs) | first cut: frontal and planta for Selectivo | IDENTICAL | - | `ADR_NEW_D4` `ADR_OLD_D4` |
| SEL-FRONTAL | main: SelectiveViewAvailabilityFacts, SelectiveDepthLayout, RackSelectivoCommands | fondo address available when `Index < FondoCount`; `fondoCount = SelectiveDepthLayout.Count(system)`; `Count = Math.Max(1, DepthCount)` | IDENTICAL | checks scoped to the Selective members | `RVA_SEL_FONDO` `SDL_COUNT` `CMD_FONDOCOUNT` |
| SEL-PLANTA | V17 2.1, 3.7, 3.8 | planta inside; canonical section -1; RUN; `A_k` includes planta | IDENTICAL | - | `V17_21` `V17_AK` `V17_PLANTA_ROW` |
| SEL-PLANTA | Freeze V18 7; post-I-57 4; Foundation 4; ADR-0036 | same statements as above | IDENTICAL | by reference | `V18_S7` `P57_S4` `FND_S4` `ADR_NEW_D4` |
| SEL-PLANTA | main: SelectiveViewAvailabilityFacts, DecodeSelective | whole-rack variant available when the view kind is planta; the Selective decoder decodes planta to `Whole` | IDENTICAL | scoped | `RVA_SEL_WHOLE_PLANTA` `COD_SEL_PLANTA` |
| SEL-LATERAL | V17 2.2, 1.3 (L-01), 3.7 | laterals of Selectivo out (E5); the lateral row does not expose `mu_k` (MC) | IDENTICAL | - | `V17_22` `V17_L01` `V17_LAT_MC` |
| SEL-LATERAL | ADR-0036 accepted negative consequences (both blobs) | the first cut reflects no laterals | IDENTICAL | - | `ADR_NEW_LAT` `ADR_OLD_LAT` |
| SEL-LATERAL | main: SelectiveViewAvailabilityFacts, DecodeSelective | a post variant is available when the post index exists; the decoder decodes a lateral to `Post` | NARROWER | the scope excludes what availability allows; V17 3.8: `A_k` = availability intersected with exposure | `RVA_SEL_POST` `COD_SEL_LATERAL_POST` |
| SEL-LEGACY-M1 | V17 3.7 | legacy frontal `-1` exposed and canonicalized to fondo 0 | NARROWER | CT-21D does not exercise it | `V17_M1` |
| SEL-LEGACY-M1 | ADR-0036 decision 4 | the Selective frontal with `-1` becomes fondo 0 | NARROWER | - | `ADR_M1` |
| SEL-LEGACY-M1 | main: DecodeSelective, RackSelectivoCommands | negative section coerced to `Fondo(0)`; a block without a fondo address is fondo 0 | NARROWER | scoped | `COD_SEL_NEG_FONDO0` `CMD_LEGACY` |
| rack-level conditions | V17 1.3, L-11, L-12, L-13, L-14 | corner fondos, linked or dependent half-frentes, topes, explicit side protector, dependent deflector, overflowing grate or pallets: fail closed | IDENTICAL | inherited by reference; CT-21D fixtures avoid them (BA-08 V3 section 12) | `V17_L11` `V17_L12` `V17_L13` `V17_L14` |
| rack-level conditions | ADR-0036 accepted negative consequences | Selectivos with topes, half-frentes whose width depends on a linked property, overflowing grate or pallet rows: fail closed until a new decision | IDENTICAL | - | `ADR_NEW_TOPES` `ADR_NEG_MEDIO` `ADR_NEG_PARRILLA` |
| authority of V17 3.7 / 3.8 | Foundation reconciliation 3; post-I-57 2 item 2 | V17 3.3, 3.7, 3.8, 5.1, ... are `HISTORICAL ONLY` for their neutral plans (`SUPERSEDE_LOCAL_NEUTRAL_PLAN`); post-I-57 limits the substitution to the ownership or location of AUTH-01..13 and keeps V17 in full outside it | IDENTICAL | the historical-only classification applies to the neutral plans; the view-exposure authority stays with I-52 (section 3.1) | `FND_S3_HIST` `FND_S3_ROWS` `FND_S4` `P57_S2_ITEM2` `P57_S2_CLOSE` `V18_S7` `P57_S4` |
| ownership of the exposure | Freeze V17 post-I-57, full text (sections 1 to 8) | no section changes the Selective view exposure; section 7 makes a change of the supported kind/view matrix an invalidation trigger | NO_STATEMENT | no superseding statement: zero occurrences of the PW-3 keywords (section 4) | `P57_S7` `P57_ZERO_FRONTAL` `P57_ZERO_PLANTA` `P57_ZERO_LATERAL` `P57_ZERO_SECTION` `P57_ZERO_EXPOSICION` `P57_ZERO_AK` `P57_ZERO_SELECTIV` |
| O-1 (historical disposition) | Freeze V18 10 | `O1_DISPOSITION = REQUIRES_REDECISION` | NO_STATEMENT | superseded by the O-1 V18 redecision of the next row | `V18_S10` |
| O-1 V18 (registered disposition) | decisions section 117; ADR-0036 current blob | `O-1 V18 = ACCEPTED / REGISTERED`; product disposition `DEFERRED`; a separate investigation of `ContextIsolationAuthority` authorized | NO_STATEMENT | defers the product; does not change the Selective exposure or the envelope: no conflict (section 5) | `DEC117_O1` `DEC117_DEFERRED` `ADR_O1V18` |
| ADR-0036 amendment for V18 | ADR-0036 current blob | the amendment records the CT-49 / session-context delta, the O-1 V18 registration and the G3 reopening sequence; exposure statements identical in both blobs | NO_STATEMENT | the amendment does not touch view exposure | `ADR_V18` `ADR_O1V18` `ADR_NEW_D4` `ADR_OLD_D4` |

**Result: 0 rows with `CONFLICT`.**

### 3.1 The Foundation `HISTORICAL ONLY` classification

The Foundation reconciliation (section 3, zero-duplication audit) classifies V17 sections 3.3, 3.7, 3.8, 5.1, 5.4, 6.1, 8.1..8.5, 14.2, 15.4 and 20.1 as `HISTORICAL ONLY`, because they were marked `PROVISIONAL UNTIL CROSS-INITIATIVE RECONCILIATION`, and it supersedes prospectively their **neutral plans** (`SUPERSEDE_LOCAL_NEUTRAL_PLAN`).
The most direct textual limit is the post-I-57 consensus, section 2 item 2: the Foundation reconciliation "sustituye prospectivamente solo la propiedad o ubicacion provisional de AUTH-01..13 en V17", and "fuera de ese alcance, Proposal V17 conserva su significado completo" (checks `P57_S2_ITEM2`, `P57_S2_CLOSE`). The same reconciliation keeps with I-52 the "exposure of views and supported combinations" (section 4); the post-I-57 consensus freezes the exposure policy as an I-52 authority (section 4); Freeze V18 section 7 inherits "exposure, supported combinations" from V17. The Selective rows of V17 3.7 and 3.8 are therefore used as the **exposure statement**, not as a neutral plan. Result: **no conflict**.

**Observation (not a conflict).** The neutral `RackViewCodec` is more permissive than the mirror scope (for a Selective rack it coerces a non-lateral non-planta `View` to a frontal fondo address). The mirror's fail-closed policy (V17 3.7) applies above the codec; the scope of this artifact is the narrower V17 policy.

## 4. Complete reading of the post-I-57 consensus (this run)

The full text (identity `P57` of section 2) was read section by section:

| Section | Finding | Checks |
|---|---|---|
| 1 Autoridades exactas | identities of the frozen authorities only | none |
| 2 Precedencia | item 2: the Foundation reconciliation replaces prospectively only the provisional ownership or location of AUTH-01..13 in V17; closing: no reconciliation changes product policy implicitly, and outside the replaced scope V17 keeps its full meaning | `P57_S2`, `P57_S2_ITEM2`, `P57_S2_CLOSE` |
| 3 Shared View Foundation consumida por referencia | AUTH-01..13 consumed from main; no exposure statement | none |
| 4 Autoridades propias de I-52 | the mirror read-set, the exposure policy and the admitted view/kind combinations are I-52 authorities; the rest is frozen by reference to V17 | `P57_S4` |
| 5 RS-3 / PlanReadSet | no exposure statement | none |
| 6 Gates y resultados pendientes | no exposure statement | none |
| 7 Invalidation y enmiendas | a change of the supported kind/view matrix invalidates the Freeze | `P57_S7` |
| 8 Recheck exacto de publicacion | procedural; no exposure statement | none |

Keyword counts of the V2.2 PW-3 list (frontal, planta, lateral, `Section`, exposición, `A_k`) and "Selectiv": **zero** each (checks `P57_ZERO_*`). No section changes the Selective view exposure.

## 5. Relation to O-1, to the envelope and to ADR-0036

- **(a) The registered O-1 disposition.** Freeze V18 section 10 recorded `O1_DISPOSITION = REQUIRES_REDECISION`. The Owner has since **redecided**: decisions section 117 records **`O-1 V18 = ACCEPTED / REGISTERED`** (2026-09-21), with the product disposition **`DEFERRED`** and a separate investigation of `ContextIsolationAuthority` authorized; ADR-0036 records the same. O-1 V18 defers the product and does **not** change the Selective exposure or the envelope: **no conflict** with this scope.
- **(b) Any future O-1 redecision.** Sealing this scope **replaces neither O-1 V18 nor any later redecision**. If a future redecision removes Selective frontal or planta from the envelope, or otherwise changes the envelope, this artifact **reopens**.
- The scope is a **characterization subset** of the V17 / Freeze V18 envelope; it **does not exceed** it.
- ADR-0036 is `PROPOSED / AMENDED FOR V18`; the amendment records the CT-49 / session-context delta, the O-1 V18 registration and the G3 reopening sequence; it does not touch view exposure (checks `ADR_V18`, `ADR_O1V18`, `ADR_NEW_D4`, `ADR_OLD_D4`).

## 6. Seal conditions (V2.2 PA-14) and where each is recorded

| # | Condition | Where it is recorded | State |
|---|---|---|---|
| 1 | Coordinator ratification citing the exact `CANDIDATE_HASH` | the ratification record (BA-07 V3 section 2) and the BA-11 entry | PENDING |
| 2 | Architect ratification citing the exact `CANDIDATE_HASH` | the ratification record and the BA-11 entry | PENDING |
| 3 | dated re-execution of the comparison against the then-current identities | **outside this artifact**: the runner output of the seal-time run, cited by the ratification records and the BA-11 entry; if any (path, blob) pair of section 2 changed, the candidate is re-prepared | this candidate run: 2026-09-30T06:58:25Z |
| 4 | complete reading of the post-I-57 consensus | this run: section 4; the seal-time repetition is recorded outside this artifact, with condition 3 | done for this run |
| 5 | no `CONFLICT` | section 3 | holds (0 rows) |
| 6 | sealed artifact with its hash | the BA-11 entry (`SEAL_HASH` = `CANDIDATE_HASH`); **this file never records its own seal** | PENDING |
| 7 | explicit statement that sealing does not replace the O-1 disposition | section 5 | included |

The authoritative state of this artifact is its **BA-11 entry**; the status text of this file is frozen at candidate time (AR2-44).

## 7. Appendix: reproducible checks

The exact executed patterns, the controls and the source identities are published, without any display transformation, in `docs/automation/evidence/I-52-ct21d-ba03-v3-checks.json`; the committed runner `docs/automation/evidence/I-52-ct21d-ba03-v3-checks.py` evaluates each check and its **positive control** (must match) and **negative control** (must not match), so a vacuous pattern fails. The output of this run is `docs/automation/evidence/I-52-ct21d-ba03-v3-checks-output.json`. Reproduce with `python docs/automation/evidence/I-52-ct21d-ba03-v3-checks.py <output.json>` from the repository root.

| Check | Source | Kind | Result | Positive control | Negative control | Note |
|---|---|---|---|---|---|---|
| `V17_INSIDE` | `V17` | substring | TRUE | TRUE | FALSE | - |
| `V17_AK` | `V17` | substring | TRUE | TRUE | FALSE | includes "; planta" (BA03-F6) |
| `V17_21` | `V17` | substring | TRUE | TRUE | FALSE | V17 2.1 (BA03-F6) |
| `V17_22` | `V17` | substring | TRUE | TRUE | FALSE | V17 2.2 (BA03-F6) |
| `V17_FRONTAL_RUN` | `V17` | regex | TRUE | TRUE | FALSE | V17 3.7: the frontal row exposes mu_k (RUN) (BA03-F4) |
| `V17_PLANTA_ROW` | `V17` | regex | TRUE | TRUE | FALSE | V17 3.7: planta, canonical section -1, RUN (BA03-F6) |
| `V17_M1` | `V17` | substring | TRUE | TRUE | FALSE | - |
| `V17_L01` | `V17` | regex | TRUE | TRUE | FALSE | - |
| `V17_LAT_MC` | `V17` | regex | TRUE | TRUE | FALSE | - |
| `V17_L11` | `V17` | regex | TRUE | TRUE | FALSE | - |
| `V17_L12` | `V17` | regex | TRUE | TRUE | FALSE | restored (BA03-F4) |
| `V17_L13` | `V17` | regex | TRUE | TRUE | FALSE | - |
| `V17_L14` | `V17` | regex | TRUE | TRUE | FALSE | restored (BA03-F4) |
| `V18_S3` | `V18` | substring | TRUE | TRUE | FALSE | - |
| `V18_S7` | `V18` | substring | TRUE | TRUE | FALSE | - |
| `V18_S10` | `V18` | substring | TRUE | TRUE | FALSE | - |
| `DEC117_O1` | `DEC117` | substring | TRUE | TRUE | FALSE | O-1 V18 registered (BA03-F1) |
| `DEC117_DEFERRED` | `DEC117` | substring | TRUE | TRUE | FALSE | BA03-F1 |
| `ADR_O1V18` | `ADR` | substring | TRUE | TRUE | FALSE | BA03-F1 |
| `P57_S2` | `P57` | substring | TRUE | TRUE | FALSE | - |
| `P57_S2_ITEM2` | `P57` | regex | TRUE | TRUE | FALSE | BA03-F8 |
| `P57_S2_CLOSE` | `P57` | regex | TRUE | TRUE | FALSE | BA03-F8 |
| `P57_S4` | `P57` | substring | TRUE | TRUE | FALSE | - |
| `P57_S7` | `P57` | substring | TRUE | TRUE | FALSE | - |
| `P57_ZERO_FRONTAL` | `P57` | regex_count flags=I | TRUE | TRUE | FALSE | PW-3 keyword list; absence claim (BA03-F6) |
| `P57_ZERO_PLANTA` | `P57` | regex_count flags=I | TRUE | TRUE | FALSE | - |
| `P57_ZERO_LATERAL` | `P57` | regex_count flags=I | TRUE | TRUE | FALSE | - |
| `P57_ZERO_SECTION` | `P57` | regex_count flags=I | TRUE | TRUE | FALSE | - |
| `P57_ZERO_EXPOSICION` | `P57` | regex_count flags=I | TRUE | TRUE | FALSE | - |
| `P57_ZERO_AK` | `P57` | regex_count | TRUE | TRUE | FALSE | - |
| `P57_ZERO_SELECTIV` | `P57` | regex_count flags=I | TRUE | TRUE | FALSE | - |
| `FND_S3_HIST` | `FND` | substring | TRUE | TRUE | FALSE | - |
| `FND_S3_ROWS` | `FND` | substring | TRUE | TRUE | FALSE | - |
| `FND_S4` | `FND` | substring | TRUE | TRUE | FALSE | - |
| `ADR_NEW_D4` | `ADR` | regex | TRUE | TRUE | FALSE | extended to "para Selectivo" (BA03-F6) |
| `ADR_OLD_D4` | `ADR0` | regex | TRUE | TRUE | FALSE | - |
| `ADR_NEW_LAT` | `ADR` | substring | TRUE | TRUE | FALSE | - |
| `ADR_OLD_LAT` | `ADR0` | substring | TRUE | TRUE | FALSE | - |
| `ADR_NEW_TOPES` | `ADR` | substring | TRUE | TRUE | FALSE | - |
| `ADR_NEG_MEDIO` | `ADR` | substring | TRUE | TRUE | FALSE | restored (BA03-F4) |
| `ADR_NEG_PARRILLA` | `ADR` | substring | TRUE | TRUE | FALSE | restored (BA03-F4) |
| `ADR_M1` | `ADR` | substring | TRUE | TRUE | FALSE | - |
| `ADR_V18` | `ADR` | substring | TRUE | TRUE | FALSE | - |
| `RVA_SEL_WHOLE_PLANTA` | `RVA` | substring (scoped) | TRUE | TRUE | FALSE | scoped to SelectiveViewAvailabilityFacts (BA03-F6) |
| `RVA_SEL_FONDO` | `RVA` | substring (scoped) | TRUE | TRUE | FALSE | scoped |
| `RVA_SEL_POST` | `RVA` | substring (scoped) | TRUE | TRUE | FALSE | scoped |
| `SDL_COUNT` | `SDL` | substring | TRUE | TRUE | FALSE | - |
| `CMD_FONDOCOUNT` | `CMD` | substring | TRUE | TRUE | FALSE | - |
| `CMD_LEGACY` | `CMD` | substring | TRUE | TRUE | FALSE | - |
| `COD_SEL_NEG_FONDO0` | `COD` | substring (scoped) | TRUE | TRUE | FALSE | scoped to DecodeSelective |
| `COD_SEL_PLANTA` | `COD` | substring (scoped) | TRUE | TRUE | FALSE | scoped |
| `COD_SEL_LATERAL_POST` | `COD` | substring (scoped) | TRUE | TRUE | FALSE | scoped |

```text
SELECTIVE_VIEW_FAMILY_SCOPE_V1 = DRAFT V3 (candidate preparation; NOT SEALED; NOT RATIFIED)
NB-1 = ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION ; OPEN until conditions 1, 2, 3, 4 (seal-time repetition) and 6 are done
CT21D_AUTHORITY_BASELINE_READY = FALSE
```
