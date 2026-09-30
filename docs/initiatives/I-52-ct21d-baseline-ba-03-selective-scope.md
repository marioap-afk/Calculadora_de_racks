# I-52 — CT-21D Baseline Artifact BA-03: Sealed Selective Scope (DRAFT V1, NOT SEALED)

> **BASELINE ARTIFACT BA-03 — DRAFT, NOT SEALED, NOT RATIFIED. Design / documentation only. Nothing here is executed or authorized.**
>
> ```text
> ARTIFACT_ID               = SELECTIVE_VIEW_FAMILY_SCOPE_V1   (baseline artifact CT21D-BASE-SCOPE-SELECTIVE)
> ARTIFACT_VERSION          = 1-DRAFT
> ARTIFACT_STATUS           = DRAFT   (SEALED only when all seven conditions of section 6 are met)
> ARTIFACT_HASH             = TO_BE_RECORDED_AT_SEAL   (see BA-11)
> AUTHORITY_CONTRACT        = CT-21D V2.1 + V2.2 + V2.3 (AGREED_FOR_BASELINE_ARTIFACT_PREPARATION)
> COMPARISON_DATE           = 2026-09-29
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

`Section = -1` is decided `OUT_OF_SCOPE` (V2.2 PA-14, Q-SEC-1). Reason: production canonicalizes it to fondo 0 **before** planning, so the seams receive and create canonical output; the plan-time reading of a legacy source is not part of the mutation whose rollback CT-21D characterizes. Catalog fixtures use canonical `Section = k`.
The decision narrows CT-21D only; it neither widens nor narrows the product envelope.

## 2. Authority identities compared (dated 2026-09-29)

| Authority | Identity |
|---|---|
| repository `HEAD` (branch `feature/rackmirror-espejo-semantico`) | `01194bb9590b7fc3c03836b5ea9a4ea9be701157` |
| `origin/main` (files of `src/`) | `3375aadbbf929427a6d106b2fff275d64863b89c` |
| Proposal V17 | `docs/initiatives/I-52-proposal-v17.md`, blob `dd944d8e9c62b083eebc2b1292789977b880c368` |
| Freeze V18 | `docs/initiatives/I-52-consensus-freeze-v18.md`, blob `c12fa65f5b6203061e8d717d1ceed595c4fed1f4` |
| Freeze V17 post-I-57 | `docs/initiatives/I-52-consensus-freeze-v17-post-i57.md`, blob `5f4933a3aa90d7052f571317107f1831ec02ac6e` |
| Foundation reconciliation | `docs/initiatives/I-52-foundation-consumption-reconciliation.md`, blob `89a21b6f9dc07e72472f3e34598e98049a3e1fb7` |
| ADR-0036, current | `docs/adr/0036-rackmirror-espejo-semantico-por-copia.md`, blob `5628947673f2afddf141c0505e92283962459443` |
| ADR-0036, blob frozen by V17 | `ce5f6fae935c7acdc347e12a7d465bb438923549` |
| `RackViewAvailability.cs` on main | blob `74e1a1aba691e42e03d3744bf6631e444aa1d696` |
| `SelectiveDepthLayout.cs` on main | blob `9e45f2f2c90bcd2ac881d4fc2dc3db2debfcd762` |
| `RackSelectivoCommands.cs` on main | blob `a22e6a6a4dba2ee2b62f2f47f358d9ac975fc486` |
| `RackViewCodec.cs` on main | blob `b63661678a2488449baab37cc74023fff0550165` |

## 3. Row-by-row comparison

`RESULT`: `IDENTICAL` (same statement), `NARROWER` (the CT-21D scope is a strict subset of what the authority permits), or `CONFLICT` (a contradiction; **a conflict prevents sealing**). Every row cites the **scripted check** that was executed on the identities of section 2 (a check is a string or pattern test on the exact text or source; all checks passed when this artifact was generated).

| Scope row | Authority and section | Statement | RESULT | Executed check |
|---|---|---|---|---|
| SEL-FRONTAL | V17 section 1.3 and 2.1 | Selectivo inside as maximum envelope: frontal by fondo | IDENTICAL | `V17_INSIDE` |
| SEL-FRONTAL | V17 section 3.7 and 3.8 | `A_k`: frontal of each fondo `k`, `0 <= k < SelectiveDepthLayout.Count(sistema)` | IDENTICAL | `V17_AK` |
| SEL-FRONTAL | Freeze V18 section 7 | exposure and supported combinations preserved from V17 by reference | IDENTICAL (by reference) | `V18_S7` |
| SEL-FRONTAL | Freeze V17 post-I-57 section 4 | I-52 owns the exposure policy and the admitted view/kind combinations; frozen by reference to V17 | IDENTICAL (by reference) | `P57_S4` |
| SEL-FRONTAL | Foundation reconciliation section 4 | I-52 keeps the exposure of views and supported combinations | IDENTICAL (keeps V17) | `FND_S4` |
| SEL-FRONTAL | ADR-0036 decision 4 (both blobs) | first cut: frontal and planta for Selectivo | IDENTICAL | `ADR_NEW_D4` `ADR_OLD_D4` |
| SEL-FRONTAL | main: `RackViewAvailability`, `SelectiveDepthLayout`, `RackSelectivoCommands` | availability of a fondo address is `Index < FondoCount`; `fondoCount = SelectiveDepthLayout.Count(system)`; `Count = Math.Max(1, DepthCount)` | IDENTICAL | `RVA_FONDO` `SDL_COUNT` `CMD_FONDOCOUNT` |
| SEL-PLANTA | V17 section 1.3, 2.1, 3.7 and 3.8 | planta inside; canonical section -1 | IDENTICAL | `V17_INSIDE` `V17_AK` |
| SEL-PLANTA | Freeze V18 section 7; post-I-57 section 4; Foundation section 4; ADR-0036 | same statements as above | IDENTICAL (by reference) | `V18_S7` `P57_S4` `FND_S4` `ADR_NEW_D4` |
| SEL-PLANTA | main: `RackViewAvailability`, `RackViewCodec` | whole-rack variant available when the view kind is planta; codec decodes planta to `Whole` | IDENTICAL | `RVA_WHOLE_PLANTA` `COD_PLANTA` |
| SEL-LATERAL | V17 section 1.3 (L-01) and 2.2; V17 3.7 | laterals of Selectivo out (E5); the lateral row does not expose `mu_k` (MC) | IDENTICAL | `V17_L01` `V17_LAT_MC` |
| SEL-LATERAL | ADR-0036 accepted negative consequences (both blobs) | the first cut reflects no laterals | IDENTICAL | `ADR_NEW_D4` `ADR_OLD_D4` |
| SEL-LATERAL | main: `RackViewAvailability`, `RackViewCodec` | a post variant is available when the post index exists; the codec decodes a lateral to `Post` | NARROWER (the scope excludes what availability allows; consistent with V17 3.8, `A_k` = availability intersected with exposure) | `RVA_POST` `COD_LATERAL_POST` |
| SEL-LEGACY-M1 | V17 section 3.7 | legacy frontal `-1` exposed and canonicalized to fondo 0 | NARROWER (CT-21D does not exercise it) | `V17_M1` |
| SEL-LEGACY-M1 | ADR-0036 decision 4 | the Selective frontal with `-1` becomes fondo 0 | NARROWER | `ADR_M1` |
| SEL-LEGACY-M1 | main: `RackViewCodec`, `RackSelectivoCommands` | the codec coerces a negative section to `Fondo(0)` (`SelectiveNegativeFondoToZero`); the command treats a block without a fondo address as fondo 0 | NARROWER | `COD_NEG_FONDO0` `CMD_LEGACY` |
| rack-level conditions | V17 section 1.3, rows L-11 and L-13 | corner fondos and racks with topes fail closed | IDENTICAL (inherited; CT-21D fixtures avoid them) | `V17_L11` `V17_L13` |
| rack-level conditions | ADR-0036 accepted negative consequences | Selectivos with topes fail closed until a new decision | IDENTICAL | `ADR_NEW_D4` |
| ownership of the exposure | Freeze V17 post-I-57, full text (sections 1 to 8) | no section changes the Selective view exposure; section 7 makes a change of the supported kind/view matrix an invalidation trigger | no CONFLICT (no superseding statement) | `P57_S7` |
| O-1 disposition | Freeze V18 section 10 | `O1_DISPOSITION = REQUIRES_REDECISION`; historical O-1 is historical only | not a scope statement; recorded in section 5 | `V18_S10` |
| ADR-0036 amendment for V18 | ADR-0036 current blob | the amendment concerns the G3 reopening sequence, not the view exposure; the exposure statements are the same in both blobs | no CONFLICT | `ADR_V18_G3` `ADR_NEW_D4` `ADR_OLD_D4` |

**Result: 0 rows with `CONFLICT`.** The comparison is a textual and behavioural comparison of the listed authorities at the listed identities; it is **not** an audit of the whole product.

**Observation (not a conflict).** The neutral `RackViewCodec` is more permissive than the mirror scope: it also coerces, for a Selective rack, a non-lateral non-planta `View` to a frontal fondo address (dispositions `SelectiveDefaultFrontalAndFondoZero`, `SelectiveNonLateralNonPlantaToFrontal`). The mirror's fail-closed policy for unrecognized or impossible `View`/`Section` combinations (V17 section 3.7) is applied **above** the codec by the mirror plan, and the scope of this artifact is the narrower V17 policy. The codec is a Foundation fact consumed by reference (AUTH-03).

## 4. Complete reading of the post-I-57 consensus

The full text of `I-52-consensus-freeze-v17-post-i57.md` (170 lines, blob `5f4933a3aa90d7052f571317107f1831ec02ac6e`) was read on 2026-09-29, section by section. It is **not** a keyword search. Findings by section:

| Section of the consensus | Bearing on the Selective view exposure |
|---|---|
| 1 Exact authorities | lists Proposal V17 as the base technical contract, ADR-0036 `PROPOSED`, Foundation reconciliation, R3, the I-57 receipt, RS-3 and the I-49 final freeze; no exposure statement |
| 2 Precedence | V17 is the base contract; Foundation reconciliation replaces prospectively only the provisional ownership and location of AUTH-01..13; "no reconciliation changes the product policy of RACKMIRROR implicitly" |
| 3 Shared View Foundation | consumption of AUTH-01..13 by reference (including `RackViewAvailability`); does not restate the exposure |
| 4 Own authorities of I-52 | freezes as I-52 responsibility the "mirror read-set, exposure policy and combinations view/kind admitted"; the rest of observable behaviour stays frozen by reference to Proposal V17 |
| 5 RS-3 / PlanReadSet | not about view exposure |
| 6 Pending gates | O-1, G3, ADR acceptance, implementation are not declared complete; not about exposure |
| 7 Invalidation and amendments | a material change of "the supported kind/view matrix" invalidates the Freeze; a docs-only diagnostic update does not create a new authority |
| 8 Publication recheck | procedure only |

**Result:** no section changes the Selective view exposure; section 4 preserves it by reference to V17. As an additional (non-substitute) indication, a keyword count over the file gives:

| Keyword | Occurrences in the consensus |
|---|---|
| `frontal` | 0 |
| `planta` | 0 |
| `lateral` | 0 |
| `Section` | 0 |
| `exposici` | 0 |
| `A_k` | 0 |
| `Selectiv` | 0 |

## 5. Relation to O-1, to the envelope and to ADR-0036

- Freeze V18 section 10: `O1_DISPOSITION = REQUIRES_REDECISION`; the historical O-1 is historical only. **Sealing this scope does NOT supersede, replace or decide the Owner O-1 redecision.**
- The scope is a **characterization** subset of the V17 / Freeze V18 envelope; it **does not exceed** it.
- ADR-0036 is `PROPOSED / AMENDED FOR V18`. The amendment concerns the G3 reopening sequence, not the view exposure; the exposure statements are the same in the blob frozen by V17 and in the current blob (scripted checks `ADR_NEW_D4` and `ADR_OLD_D4`).
- A future Owner redecision of O-1 that changes the Selective envelope reopens this artifact.

## 6. Seal conditions (V2.2 PA-14) and current state

| # | Condition | State on 2026-09-29 |
|---|---|---|
| 1 | Coordinator ratification | **PENDING** placeholder: `COORDINATOR_RATIFICATION = UNSET` (name, date, artifact hash) |
| 2 | Architect ratification | **PENDING** placeholder: `ARCHITECT_RATIFICATION = UNSET` (name, date, artifact hash) |
| 3 | dated re-execution of the authority comparison against the then-current identities | **executed on 2026-09-29** for this draft; it must be **re-executed at the moment of sealing** if any identity of section 2 has changed |
| 4 | complete reading of the post-I-57 consensus, not keyword search only | **done** (section 4); to be confirmed by the ratifiers |
| 5 | no `CONFLICT` | **holds** (0 rows) |
| 6 | sealed artifact with its hash | **NOT DONE**: `ARTIFACT_HASH = TO_BE_RECORDED_AT_SEAL` |
| 7 | explicit statement that sealing does NOT replace the Owner O-1 redecision | **included** (section 5) |

```text
SELECTIVE_VIEW_FAMILY_SCOPE_V1 = DRAFT (NOT SEALED)
NB-1 = ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION ; remains OPEN until conditions 1, 2 and 6 are done and condition 3 is re-executed at sealing
CT21D_AUTHORITY_BASELINE_READY = FALSE
```
