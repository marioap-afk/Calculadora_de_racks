# I-52 — CT-21D AUTHORITY CONTRACT V2.5 (DRAFT — PH2-TP-1 PRE-APPROVED MICRO-ERRATA OVER V2.4)

> **CT-21D AUTHORITY CONTRACT V2.5 — MICRO-ERRATA OVER V2.4 THAT ADDS ONLY THE TUPLE FIELD `CATALOG_FOLDER` (PH2-TP-1), FOR COORDINATOR TEXTUAL VERIFICATION. Design / documentation only. Nothing here is executed, built or authorized.**
>
> ```text
> CT21D_AUTHORITY_CONTRACT          = DRAFT_V2_5  (V2.1 with the V2.2, V2.3, V2.4 and V2.5 errata applied)
> CT21D_AUTHORITY_CONTRACT_STATUS   = AGREED_FOR_BASELINE_ARTIFACT_PREPARATION (for V2.1 + V2.2 + V2.3; V2.4 and V2.5 pending Coordinator textual verification)
> DRAFT REVISION                    = 2 (content changes limited to the words required by the Architect's delta ruling, decisions
>                                     section 217, AR4-18 / finding V25R1-01: the section 0 paraphrase of the catalog folder and the
>                                     sampling restriction after the section 2 table; apart from them only the revision identifiers changed:
>                                     this header field and the draft-revision word of the section 4 status line; revision 1 is blob
>                                     c4405ed502ce34ed9e112a1cb0f4a5f3de98c60a (commit b6fab832) and stays in history; section 1 is
>                                     byte-identical to revision 1)
> CT21D_AUTHORITY_BASELINE_READY    = FALSE
> CT21D_EXECUTION_READY             = FALSE
> CT21D_EXECUTION                   = NOT_AUTHORIZED
> CURRENTLY_ADMISSIBLE_KIND_SCOPE   = NONE
> CIA = UNKNOWN   SafeOperationalState = FALSE_FOR_ADMISSION   G3 = STOPPED   G3B = NOT OPEN   CT-50 = NOT EXECUTED   ALT-21E = FALLBACK_IN_EFFECT
> Freeze V18 = GOVERNING   Freeze V35 = GOVERNING ITS OWN PREDICATE   Replacement Freeze = DOES NOT EXIST   ADR-0036 = PROPOSED / AMENDED FOR V18
> ```

## 0. What V2.5 is

The Implementer's phase-2 report proposed PH2-TP-1: the folder of catalog files that the catalog provider reads (the `*.csv` and `*.json` files of the resolved catalog directory) is an input of the product path of the exact build, and the tuple of V2.1 section 2.1 has no field for it. The Architect's phase-2 delta ruling (decisions section 214, AR3-13) accepted the proposal. Because it adds a field to the tuple, the ruling requires a micro-errata and **pre-approved its text verbatim**. V2.5 applies that text (section 1) and the mechanical extension of the contract identity that the same decision orders. V1, V2, V2.1, V2.2, V2.3 and V2.4 are intact and historical.

| Item | Rule |
|---|---|
| Effective contract | **`CT-21D-V2.5`** = the text of V2.1, with the clauses of V2.2, V2.3, V2.4 and V2.5 applied in that order |
| Precedence | for the clause named in section 2, **V2.5 governs over V2.1**; the *Contract hash* row of this section governs over the *Contract hash* row of V2.4 section 0; every other clause stands unchanged |
| Contract hash | the composite identity of V2.4 section 0 is extended by one more document (AR3-13: "la identidad compuesta se extiende mecánicamente a V2.5"): SHA-256 over the LF-normalized texts of the V2.1, V2.2, V2.3, V2.4 and V2.5 blobs in that order, plus the V5 blob id. The byte-level procedure is the one of the hash registry (baseline artifact BA-11, section 1); it is cited, not restated here (deviation D-1) |
| Review of V2.5 | the Architect's next delta review checks the identity and text of V2.5 against AR3-13; the Coordinator verifies identity and text |

## 1. Added text (the Architect's pre-approved text, verbatim)

> V2.1 section 2.1 gains the BUILD_BOUND field `CATALOG_FOLDER`: the resolved catalog directory path of the build under test and the SHA-256 of the deployed bytes of every `*.csv` and `*.json` file directly in that directory (the scope of the catalog provider's signature). It is sampled before the library acquisition together with the other BUILD_BOUND fields and re-checked after CP; a difference is a tuple mismatch under V2.1 section 2.2.

## 2. Clause affected

| Clause | Effect of V2.5 |
|---|---|
| V2.1 section 2.1 (tuple fields) | gains one BUILD_BOUND row, `CATALOG_FOLDER`, with the content of section 1. Every other row, and the mismatch rules of V2.1 section 2.2, stand unchanged |

Nothing else changes: the tuple classes, the mismatch rules (a BUILD_BOUND mismatch makes the run `INVALID`, V2.1 section 2.2 and 21.1), the admission inputs and every blocker state stay as they are. V2.5 authorizes no host work.

## 3. Deviations and notes for the Coordinator's textual verification

| Id | Content |
|---|---|
| D-1 | the composite contract identity is extended to the V2.1, V2.2, V2.3, V2.4 and V2.5 blobs; mechanical consequence ordered by AR3-13 (not a semantic change). The byte-level reading of "plus the V5 blob id" is the procedure of BA-11 section 1, which is cited and not restated here |
| `V25-N1` | the field name `CATALOG_FOLDER`, its class `BUILD_BOUND`, its content and its two sampling points are exactly those of the pre-approved text; the clause map of section 2 is implementer text and adds nothing to it |

There is **no other deviation** from the pre-approved text. The note of V2.5 uses the identifier `V25-N1` so that it does not collide with the notes N-1..N-5 of V2.2 or with `V24-N1` and `V24-N2`.

## 4. Status

```text
CT21D_AUTHORITY_CONTRACT = DRAFT_V2_5 (V2.1 + V2.2 + V2.3 + V2.4 + V2.5), draft revision 2
CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
NEXT = ARCHITECT DELTA REVIEW (IDENTITY AND TEXT OF V2.5), THEN COORDINATOR TEXTUAL VERIFICATION OF V2.4 AND V2.5
```
