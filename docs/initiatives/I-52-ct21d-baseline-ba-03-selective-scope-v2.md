# I-52 — CT-21D Baseline Artifact BA-03: Sealed Selective Scope (DRAFT V2, CANDIDATE PREPARATION)

> **BASELINE ARTIFACT BA-03 V2 — DRAFT prepared for `CANDIDATE_FOR_SEALING`, NOT SEALED, NOT RATIFIED. Design / documentation only.**
>
> ```text
> ARTIFACT_ID               = SELECTIVE_VIEW_FAMILY_SCOPE_V1   (baseline artifact CT21D-BASE-SCOPE-SELECTIVE)
> ARTIFACT_VERSION          = 2-DRAFT   (supersedes 1-DRAFT, blob 76789e68b69caaba6ccb0469f5f7d6c372cb7bb7, which stays as history)
> ARTIFACT_STATUS           = DRAFT (candidate preparation, phase 2)
> CANDIDATE_HASH            = UNSET     SEAL_HASH = UNSET   (BA-11 V2 section 3)
> AUTHORITY_CONTRACT        = CT-21D V2.1 + V2.2 (PA-14) + V2.3 + V2.4
> PHASE-1 RULING APPLIED    = scope ratified by the Architect; two changes: (1) Foundation HISTORICAL ONLY row and explanation restored; (2) reproducible appendix of the executed checks published
> COMPARISON RUN            = 2026-09-30T04:19Z  (this run is evidence for the candidate; a DATED RE-RUN at sealing time is required)
> BLOCKER                   = NB-1  (ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION; OPEN until sealed)
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 1. The scope (ratified by the Architect in the phase-1 review; unchanged)

| Row | Domain | Status for CT-21D |
|---|---|---|
| `SEL-FRONTAL` | `View = frontal`, `Section = k` for `0 <= k < SelectiveDepthLayout.Count(system)` (one per fondo); scenarios cover one fondo and several fondos | **IN** |
| `SEL-PLANTA` | `View = planta` (canonical section `-1`) | **IN** |
| `SEL-LATERAL` | lateral, per post | **OUT** (fail-closed, E5) |
| `SEL-LEGACY-M1` | frontal with the legacy `Section = -1` (or a legacy empty `View`) | **OUT_OF_SCOPE** for CT-21D scenarios |

`Section = -1` is `OUT_OF_SCOPE` (V2.2 PA-14, Q-SEC-1): production canonicalizes it to fondo 0 before planning, so the seams receive and create canonical output. The decision narrows CT-21D only; it neither widens nor narrows the product envelope.

## 2. Authority identities of this run

| Key | Authority | Where | Path | Git blob |
|---|---|---|---|---|
| `V17` | Proposal V17 | branch `HEAD` | `docs/initiatives/I-52-proposal-v17.md` | `dd944d8e9c62b083eebc2b1292789977b880c368` |
| `V18` | Freeze V18 | branch `HEAD` | `docs/initiatives/I-52-consensus-freeze-v18.md` | `c12fa65f5b6203061e8d717d1ceed595c4fed1f4` |
| `P57` | Freeze V17 post-I-57 | branch `HEAD` | `docs/initiatives/I-52-consensus-freeze-v17-post-i57.md` | `5f4933a3aa90d7052f571317107f1831ec02ac6e` |
| `FND` | Foundation reconciliation | branch `HEAD` | `docs/initiatives/I-52-foundation-consumption-reconciliation.md` | `89a21b6f9dc07e72472f3e34598e98049a3e1fb7` |
| `ADR` | ADR-0036 (current) | branch `HEAD` | `docs/adr/0036-rackmirror-espejo-semantico-por-copia.md` | `5628947673f2afddf141c0505e92283962459443` |
| `ADR0` | ADR-0036 (blob frozen by V17) | git object | `(blob)` | `ce5f6fae935c7acdc347e12a7d465bb438923549` |
| `RVA` | RackViewAvailability.cs | `origin/main` 1304101d997b | `src/RackCad.Application/Systems/Shared/RackViewAvailability.cs` | `74e1a1aba691e42e03d3744bf6631e444aa1d696` |
| `SDL` | SelectiveDepthLayout.cs | `origin/main` 1304101d997b | `src/RackCad.Application/Systems/Selective/SelectiveDepthLayout.cs` | `9e45f2f2c90bcd2ac881d4fc2dc3db2debfcd762` |
| `CMD` | RackSelectivoCommands.cs | `origin/main` 1304101d997b | `src/RackCad.Plugin/RackSelectivoCommands.cs` | `a22e6a6a4dba2ee2b62f2f47f358d9ac975fc486` |
| `COD` | RackViewCodec.cs | `origin/main` 1304101d997b | `src/RackCad.Application/Systems/Shared/RackViewCodec.cs` | `b63661678a2488449baab37cc74023fff0550165` |

Repository `HEAD` of the run: `1711112021394a6b14112b1787abff4d3062e064`; `origin/main`: `1304101d997b66748e21f4896ac203d6fdc6a1f1`.

## 3. Row-by-row comparison

`RESULT`: `IDENTICAL`, `NARROWER` (the CT-21D scope is a strict subset of what the authority permits) or `CONFLICT` (a contradiction; **a conflict prevents sealing**). Every row cites the checks of the appendix (section 7) that were executed on the identities of section 2.

| Scope row | Authority and section | Statement | RESULT | Checks |
|---|---|---|---|---|
| SEL-FRONTAL | V17 1.3, 2.1 | Selectivo inside as maximum envelope: frontal by fondo | IDENTICAL | `V17_INSIDE` |
| SEL-FRONTAL | V17 3.7, 3.8 | `A_k`: frontal of each fondo `k`, `0 <= k < SelectiveDepthLayout.Count(sistema)` | IDENTICAL | `V17_AK` |
| SEL-FRONTAL | Freeze V18 3 and 7 | V17 governs what V18 does not replace; exposure and supported combinations preserved by reference | IDENTICAL (by reference) | `V18_S3` `V18_S7` |
| SEL-FRONTAL | Freeze V17 post-I-57, 2 and 4 | no reconciliation changes product policy implicitly; exposure policy and admitted view/kind combinations frozen as I-52 authority by reference to V17 | IDENTICAL (by reference) | `P57_S2` `P57_S4` |
| SEL-FRONTAL | Foundation reconciliation 4 | I-52 keeps the exposure of views and supported combinations | IDENTICAL (keeps V17) | `FND_S4` |
| SEL-FRONTAL | ADR-0036 decision 4 (both blobs) | first cut: frontal and planta for Selectivo | IDENTICAL | `ADR_NEW_D4` `ADR_OLD_D4` |
| SEL-FRONTAL | main: RackViewAvailability, SelectiveDepthLayout, RackSelectivoCommands | fondo address available when `Index < FondoCount`; `fondoCount = SelectiveDepthLayout.Count(system)`; `Count = Math.Max(1, DepthCount)` | IDENTICAL | `RVA_FONDO` `SDL_COUNT` `CMD_FONDOCOUNT` |
| SEL-PLANTA | V17 1.3, 2.1, 3.7, 3.8 | planta inside; canonical section -1; `A_k` includes planta | IDENTICAL | `V17_INSIDE` `V17_AK` |
| SEL-PLANTA | Freeze V18 7; post-I-57 4; Foundation 4; ADR-0036 | same statements as above | IDENTICAL (by reference) | `V18_S7` `P57_S4` `FND_S4` `ADR_NEW_D4` |
| SEL-PLANTA | main: RackViewAvailability, RackViewCodec | whole-rack variant available when the view kind is planta; codec decodes planta to `Whole` | IDENTICAL | `RVA_WHOLE_PLANTA` `COD_PLANTA` |
| SEL-LATERAL | V17 1.3 (L-01), 2.2, 3.7 | laterals of Selectivo out (E5); the lateral row does not expose `mu_k` (MC) | IDENTICAL | `V17_L01` `V17_LAT_MC` |
| SEL-LATERAL | ADR-0036 accepted negative consequences (both blobs) | the first cut reflects no laterals | IDENTICAL | `ADR_NEW_LAT` `ADR_OLD_LAT` |
| SEL-LATERAL | main: RackViewAvailability, RackViewCodec | a post variant is available when the post index exists; codec decodes a lateral to `Post` | NARROWER (scope excludes what availability allows; V17 3.8: `A_k` = availability intersected with exposure) | `RVA_POST` `COD_LATERAL_POST` |
| SEL-LEGACY-M1 | V17 3.7 | legacy frontal `-1` exposed and canonicalized to fondo 0 | NARROWER (CT-21D does not exercise it) | `V17_M1` |
| SEL-LEGACY-M1 | ADR-0036 decision 4 | the Selective frontal with `-1` becomes fondo 0 | NARROWER | `ADR_M1` |
| SEL-LEGACY-M1 | main: RackViewCodec, RackSelectivoCommands | negative section coerced to `Fondo(0)` (`SelectiveNegativeFondoToZero`); a block without a fondo address is fondo 0 | NARROWER | `COD_NEG_FONDO0` `CMD_LEGACY` |
| rack-level conditions | V17 1.3, L-11 and L-13 | corner fondos and racks with topes fail closed | IDENTICAL (inherited; CT-21D fixtures avoid them) | `V17_L11` `V17_L13` |
| rack-level conditions | ADR-0036 accepted negative consequences | Selectivos with topes fail closed until a new decision | IDENTICAL | `ADR_NEW_TOPES` |
| authority of V17 3.7 / 3.8 | Foundation reconciliation 3 (zero-duplication audit) | V17 3.3, 3.7, 3.8, 5.1, ... are `HISTORICAL ONLY`: they were provisional, and their **neutral plans** are superseded prospectively by consumption from `main` (`SUPERSEDE_LOCAL_NEUTRAL_PLAN`) | no CONFLICT: the historical-only classification applies to the **neutral plans** (ownership, location and extraction of taxonomy, codec, frame, selection, placement, Resolve/Plan, requirements, comparator); the **view-exposure authority** is kept by I-52 (Foundation 4) and inherited by Freeze V18 7 and post-I-57 4 | `FND_S3_HIST` `FND_S3_ROWS` `FND_S4` `V18_S7` `P57_S4` |
| ownership of the exposure | Freeze V17 post-I-57, full text (sections 1 to 8) | no section changes the Selective view exposure; section 7 makes a change of the supported kind/view matrix an invalidation trigger | no CONFLICT (no superseding statement) | `P57_S7` |
| O-1 disposition | Freeze V18 10 | `O1_DISPOSITION = REQUIRES_REDECISION`; historical O-1 is historical only | not a scope statement; recorded in section 5 | `V18_S10` |
| ADR-0036 amendment for V18 | ADR-0036 current blob | the amendment concerns the G3 reopening sequence, not the view exposure; exposure statements identical in both blobs | no CONFLICT | `ADR_V18` `ADR_NEW_D4` `ADR_OLD_D4` |

**Result: 0 rows with `CONFLICT`.**

### 3.1 The Foundation `HISTORICAL ONLY` classification (restored explanation)

The Foundation reconciliation (section 3, zero-duplication audit) classifies V17 sections 3.3, 3.7, 3.8, 5.1, 5.4, 6.1, 8.1..8.5, 14.2, 15.4 and 20.1 as `HISTORICAL ONLY`, because they were marked `PROVISIONAL UNTIL CROSS-INITIATIVE RECONCILIATION`, and it supersedes prospectively their **neutral plans** (`SUPERSEDE_LOCAL_NEUTRAL_PLAN`): the ownership, location and extraction of the neutral taxonomy and codec, view frame, selection, placement, Resolve/Plan ports, block requirements and authored comparator, which are consumed from `main` (AUTH-01..13).
That classification does **not** remove the **view-exposure authority** of I-52: the same reconciliation keeps with I-52 the "exposure of views and supported combinations" (section 4), the post-I-57 consensus freezes the "exposure policy and admitted view/kind combinations" as an I-52 authority by reference to V17 (section 4), and Freeze V18 section 7 inherits "exposure, supported combinations" from V17. The Selective rows of V17 3.7 and 3.8 are therefore used here **as the exposure statement inherited through those authorities**, not as a neutral plan. Result: **no conflict**.

**Observation (not a conflict).** The neutral `RackViewCodec` is more permissive than the mirror scope (it coerces, for a Selective rack, a non-lateral non-planta `View` to a frontal fondo address). The mirror's fail-closed policy for unrecognized or impossible `View`/`Section` combinations (V17 3.7) applies above the codec; the scope of this artifact is the narrower V17 policy.

## 4. Complete reading of the post-I-57 consensus

The full text of the consensus (blob `5f4933a3aa90d7052f571317107f1831ec02ac6e`) was read section by section; no section changes the Selective view exposure; section 2 states that no reconciliation changes the product policy of RACKMIRROR implicitly; section 4 preserves the exposure by reference to V17; section 7 makes a change of the supported kind/view matrix an invalidation trigger. The reading is **to be repeated at sealing** (seal condition 4).

## 5. Relation to O-1, to the envelope and to ADR-0036

- Freeze V18 section 10: `O1_DISPOSITION = REQUIRES_REDECISION`; the historical O-1 is historical only. **Sealing this scope does NOT supersede, replace or decide the Owner O-1 redecision.**
- The scope is a **characterization subset** of the V17 / Freeze V18 envelope; it **does not exceed** it. If the Owner redecides O-1 removing Selective frontal or planta from the envelope, this artifact reopens.
- ADR-0036 is `PROPOSED / AMENDED FOR V18`; the amendment concerns the G3 reopening sequence, not the view exposure (checks `ADR_V18`, `ADR_NEW_D4`, `ADR_OLD_D4`).

## 6. Seal conditions (V2.2 PA-14) and state

| # | Condition | State |
|---|---|---|
| 1 | Coordinator ratification (citing the exact `CANDIDATE_HASH`) | PENDING |
| 2 | Architect ratification (citing the exact `CANDIDATE_HASH`) | PENDING |
| 3 | dated re-execution of the comparison against the then-current identities | this run (2026-09-30T04:19Z) supports the candidate; **a re-run at sealing is required**; if any identity of section 2 changed, the candidate is re-prepared |
| 4 | complete reading of the post-I-57 consensus | done for this run (section 4); repeated at sealing |
| 5 | no `CONFLICT` | holds (0 rows) |
| 6 | sealed artifact with its hash | PENDING (`SEAL_HASH = UNSET`) |
| 7 | explicit statement that sealing does not replace the Owner O-1 redecision | included (section 5) |

## 7. Appendix: reproducible checks

Each check is either `substring` (the exact pattern must occur in the text of the blob, after CRLF is normalized to LF) or `regex` (a Python `re.search` over the same text must match). Backquotes inside patterns are shown as single quotes and the vertical bar is escaped for the table; the executed pattern uses the literal characters. The run is reproducible with `git cat-file -p <blob>` on the listed blobs.

| Check | Source | Blob (prefix) | Kind | Pattern | Expected and observed |
|---|---|---|---|---|---|
| `V17_INSIDE` | `V17` | `dd944d8e9c62` | substring | `Selectivo (frontal por fondo y planta)` | TRUE |
| `V17_AK` | `V17` | `dd944d8e9c62` | substring | `frontal de cada fondo 'k' con '0 ≤ k < SelectiveDepthLayout.Count(sistema)'` | TRUE |
| `V17_M1` | `V17` | `dd944d8e9c62` | substring | `'−1' (legado documentado)` | TRUE |
| `V17_L01` | `V17` | `dd944d8e9c62` | regex | `\\| L-01 \\| Laterales de Selectivo` | TRUE |
| `V17_LAT_MC` | `V17` | `dd944d8e9c62` | regex | `\\| selective \\| lateral \\| poste 'p' .*\(\*\*MC\*\*\)` | TRUE |
| `V17_L11` | `V17` | `dd944d8e9c62` | regex | `\\| L-11 \\| Selectivo con frentes por fondo en esquina` | TRUE |
| `V17_L13` | `V17` | `dd944d8e9c62` | regex | `\\| L-13 \\| Selectivo con topes` | TRUE |
| `V18_S3` | `V18` | `c12fa65f5b62` | substring | `Proposal V17 gobierna todo lo no sustituido expresamente por V18.` | TRUE |
| `V18_S7` | `V18` | `c12fa65f5b62` | substring | `exposición, combinaciones soportadas, restricciones de fuente y completitud/relectura` | TRUE |
| `V18_S10` | `V18` | `c12fa65f5b62` | substring | `O1_DISPOSITION = REQUIRES_REDECISION` | TRUE |
| `P57_S2` | `P57` | `5f4933a3aa90` | substring | `Ninguna reconciliacion cambia de forma implicita la politica de producto de RACKMIRROR.` | TRUE |
| `P57_S4` | `P57` | `5f4933a3aa90` | substring | `mirror read-set, exposure policy y combinaciones view/kind admitidas` | TRUE |
| `P57_S7` | `P57` | `5f4933a3aa90` | substring | `la matriz kind/view soportada` | TRUE |
| `FND_S3_HIST` | `FND` | `89a21b6f9dc0` | substring | `'HISTORICAL ONLY'; estaban marcadas 'PROVISIONAL UNTIL CROSS-INITIATIVE RECONCILIATION'. Sus planes neutrales quedan 'SUPERSEDE_LOCAL_NEUTRAL_PLAN' prospectivamente.` | TRUE |
| `FND_S3_ROWS` | `FND` | `89a21b6f9dc0` | substring | `Proposal V17 §§3.3, 3.7, 3.8, 5.1` | TRUE |
| `FND_S4` | `FND` | `89a21b6f9dc0` | substring | `exposicion de vistas y combinaciones soportadas` | TRUE |
| `ADR_NEW_D4` | `ADR` | `5628947673f2` | substring | `Primer corte: frontal y planta` | TRUE |
| `ADR_NEW_LAT` | `ADR` | `5628947673f2` | substring | `no se reflejan laterales de Selectivo` | TRUE |
| `ADR_NEW_TOPES` | `ADR` | `5628947673f2` | substring | `los Selectivos con topes` | TRUE |
| `ADR_OLD_D4` | `ADR0` | `ce5f6fae935c` | substring | `Primer corte: frontal y planta` | TRUE |
| `ADR_OLD_LAT` | `ADR0` | `ce5f6fae935c` | substring | `no se reflejan laterales de Selectivo` | TRUE |
| `ADR_M1` | `ADR` | `5628947673f2` | substring | `la frontal Selectiva con '−1' pasa a fondo '0'` | TRUE |
| `ADR_V18` | `ADR` | `5628947673f2` | substring | `ADR-0036 = PROPOSED / AMENDED FOR V18` | TRUE |
| `RVA_WHOLE_PLANTA` | `RVA` | `74e1a1aba691` | regex | `RackViewVariantKind\.Whole:\s*return address\.Kind == DimensionViewKind\.Planta` | TRUE |
| `RVA_FONDO` | `RVA` | `74e1a1aba691` | substring | `address.Variant.Index < FondoCount` | TRUE |
| `RVA_POST` | `RVA` | `74e1a1aba691` | substring | `posts.Contains(address.Variant.Index)` | TRUE |
| `SDL_COUNT` | `SDL` | `9e45f2f2c90b` | substring | `Math.Max(1, system?.DepthCount ?? 1)` | TRUE |
| `CMD_FONDOCOUNT` | `CMD` | `a22e6a6a4dba` | substring | `var fondoCount = SelectiveDepthLayout.Count(system);` | TRUE |
| `CMD_LEGACY` | `CMD` | `a22e6a6a4dba` | substring | `a legacy block with -1 = fondo 0` | TRUE |
| `COD_NEG_FONDO0` | `COD` | `b63661678a24` | substring | `SelectiveNegativeFondoToZero` | TRUE |
| `COD_PLANTA` | `COD` | `b63661678a24` | substring | `DimensionViewKind.Planta, RackViewVariant.Whole()` | TRUE |
| `COD_LATERAL_POST` | `COD` | `b63661678a24` | substring | `DimensionViewKind.Lateral, RackViewVariant.Post(syntax.Section)` | TRUE |

```text
SELECTIVE_VIEW_FAMILY_SCOPE_V1 = DRAFT V2 (candidate preparation; NOT SEALED)
NB-1 = ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION ; OPEN until conditions 1, 2, 3 (re-run), 4 (repeat) and 6 are done
CT21D_AUTHORITY_BASELINE_READY = FALSE
```
