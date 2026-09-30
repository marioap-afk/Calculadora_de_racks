# I-52 — CT-21D AUTHORITY CONTRACT V2.4 (DRAFT — RC-C1 PRE-APPROVED MICRO-ERRATA OVER V2.3)

> **CT-21D AUTHORITY CONTRACT V2.4 — MICRO-ERRATA OVER V2.3 THAT RESOLVES ONLY RC-C1, FOR COORDINATOR TEXTUAL VERIFICATION. Design / documentation only. Nothing here is executed, built or authorized.**
>
> ```text
> CT21D_AUTHORITY_CONTRACT          = DRAFT_V2_4  (V2.1 with the V2.2, V2.3 and V2.4 errata applied)
> CT21D_AUTHORITY_CONTRACT_STATUS   = AGREED_FOR_BASELINE_ARTIFACT_PREPARATION (for V2.1 + V2.2 + V2.3; V2.4 pending Coordinator textual verification)
> DRAFT REVISION                    = 2 (corrections outside section 1 required by ARCHITECT_BASELINE_PHASE2_RULING, decisions section 211, AR2-01..AR2-07;
>                                     the reviewed draft is blob 1919a32a (commit d190ec0929ef5495b801a3164ce6e7b219cdd857) and stays in history;
>                                     section 1 is byte-identical to that draft)
> CT21D_AUTHORITY_BASELINE_READY    = FALSE
> CT21D_EXECUTION_READY             = FALSE
> CT21D_EXECUTION                   = NOT_AUTHORIZED
> CURRENTLY_ADMISSIBLE_KIND_SCOPE   = NONE
> CIA = UNKNOWN   SafeOperationalState = FALSE_FOR_ADMISSION   G3 = STOPPED   G3B = NOT OPEN   CT-50 = NOT EXECUTED   ALT-21E = FALLBACK_IN_EFFECT
> Freeze V18 = GOVERNING   Freeze V35 = GOVERNING ITS OWN PREDICATE   Replacement Freeze = DOES NOT EXIST   ADR-0036 = PROPOSED / AMENDED FOR V18
> ```

## 0. What V2.4 is

The Architect's review of the authority baseline artifacts, phase 1 (`ARCHITECT_BASELINE_PHASE1_RULING = AGREED_WITH_REQUIRED_CHANGES`, over commit `1711112021394a6b14112b1787abff4d3062e064`; full text recorded in `docs/automation/evidence/I-52-ct21d-architect-phase1-ruling.md`), found **one contradiction in the contract** (RC-C1):

- V2.1 section 27.10 and V2.2 PA-11 make the **independent completeness review** of the seam-operation inventory a **baseline** requirement, and PA-11 conditions 3 and 4 require a **dynamic trace over every seam entry point and every plan-shape class**;
- the seams do **not exist** (they are implementation prerequisites, V2.1 section 15.8 and EXEC-3), and host work is not authorized, so conditions 3 and 4 cannot be met before the baseline.

The Architect pre-approved a closed replacement text (phase-1 ruling, section 7, BA-06 / NB-4). **V2.4 applies that text** (section 1), plus the clause map, the mechanical consequences and the notes that the Architect's phase-2 ruling pre-approved (decisions section 211, AR2-02..AR2-07). V1, V2, V2.1, V2.2 and V2.3 are intact and historical.

| Item | Rule |
|---|---|
| Effective contract | **`CT-21D-V2.4`** = the text of V2.1, with the clauses of V2.2, V2.3 and V2.4 applied in that order |
| Precedence | for the clauses named in section 2, **V2.4 governs over V2.1 and V2.2**; the *Contract hash* row of this section governs over V2.2 section 0.1 / note N-1 of V2.2 and over V2.3 section 0 / D-1 of V2.3; every other clause stands unchanged |
| Contract hash | the composite identity approach of note N-1 of V2.2 (accepted by the Coordinator; extended by D-1 of V2.3) is extended by one more document: SHA-256 over the LF-normalized texts of the V2.1, V2.2, V2.3 and V2.4 blobs in that order, plus the V5 blob id. The byte-level procedure is the one of the hash registry (baseline artifact BA-11, section 1); it is cited, not restated here (deviation D-1) |
| Review of V2.4 | the scope of the review of this text is set by the phase-1 ruling (section 13: the micro-errata requires verification by the Coordinator); the Architect's phase-2 review covered the identity and text of V2.4 (decisions section 211); the Coordinator verifies identity and text |

## 1. Replacement text (the Architect's pre-approved text, verbatim)

> The baseline requires the seam-operation inventory to be `STATIC_REVIEWED`: conditions 1, 2, 5, 6 and 7 of V2.2 PA-11 satisfied for the design-time inventory (the reused product path at a bound SHA plus the operations required by the seam contract) and the dynamic-trace plan of conditions 3 and 4 approved. The inventory becomes `REVIEWED` (complete) only after the dynamic trace covers every seam entry point and every plan-shape class on the implemented seams, and that is required before the first `LEARNING` run. `INVENTORY_COMPLETENESS = NOT_ESTABLISHED` until `REVIEWED`. An operation added by the implemented seams after the design-time review reopens the review of its operation class.

## 2. Clauses affected

| Clause | Effect of V2.4 |
|---|---|
| V2.1 section 27.10 ("the baseline requires ... the independent completeness review") | the completeness review required **for the baseline** is the design-time review that yields `STATIC_REVIEWED` (section 1); the rest of 27.10 is unchanged (the baseline still requires the procedure, the corpus definitions and the EVM schema, and still does **not** require learned EVM content) |
| V2.2 PA-11 (independent completeness review, seven conditions) | the seven conditions are unchanged. For the **baseline**, conditions 1, 2, 5, 6 and 7 apply to the design-time inventory and conditions 3 and 4 apply as an **approved plan**. The full review with the dynamic trace (conditions 3 and 4 executed on the implemented seams) yields `REVIEWED` and is required **before the first `LEARNING` run**. The sentence "Nothing in the inventory is called complete until it exists" reads: `INVENTORY_COMPLETENESS = NOT_ESTABLISHED` until `REVIEWED`. The status of the review records is stated in note `V24-N2` |
| V2.1 section 22, `BASE-15` ("E-12 event-model procedure and learning-corpus definition agreed") | for the inventory part requires `STATIC_REVIEWED`, with its review record agreed; `REVIEWED` is **not** a baseline requirement (mechanical consequence D-3) |
| `NB-4` in the blocker tables of V2.1 section 32, V2.2 and V2.3 | "the completeness review" reads as the design-time review that yields `STATIC_REVIEWED` (27.10 as amended); the state of `NB-4` is unchanged (mechanical consequence D-2) |
| V2.1 section 27.2 step 1 (inventory as starting input) | unchanged; an operation that the implemented seams add after the design-time review **reopens the review of its operation class** (section 1, last sentence) |

Nothing else changes: the phase policy of E-12, the class registry, the cause taxonomy, `E12-V` requiring a `FROZEN` model (V2.2 PA-2) and every blocker state stay as they are.

## 3. Deviations and notes for the Coordinator's textual verification

| Id | Content |
|---|---|
| D-1 | the composite contract identity is extended to the V2.1, V2.2, V2.3 and V2.4 blobs; mechanical consequence of note N-1 of V2.2 (not a semantic change). The byte-level reading of "plus the V5 blob id" is the procedure of BA-11 section 1, which is cited and not restated here |
| D-2 | the `NB-4` row of section 2 (pre-approved by the Architect, AR2-05): a mechanical consequence of section 1 for the blocker tables; no state changes |
| D-3 | the `BASE-15` row of section 2 (pre-approved by the Architect, AR2-05): a mechanical consequence of section 1 for the baseline requirement; `REVIEWED` is not a baseline requirement |
| `V24-N1` | the states `STATIC_REVIEWED`, `REVIEWED` and `INVENTORY_COMPLETENESS = NOT_ESTABLISHED` are exactly those of the pre-approved text; no other state is introduced |
| `V24-N2` | (pre-approved by the Architect, AR2-06) PA-11 "the review record is a baseline artifact" applies to the `STATIC_REVIEWED` record. The `REVIEWED` record (dynamic trace on the implemented seams) and any reopened operation-class review are execution-phase records: they are kept under the custody of BA-07 and cited by the `LEARNING` entry of the EVM; they do not modify the sealed baseline unless they change the design-time inventory, which requires a new BA-06 version and a new baseline. For the design-time review, condition 6 is read as in item (10) of the phase-1 ruling: the differences between the static reviewers and the critic are resolved explicitly |

There is **no other deviation** from the pre-approved text. The notes of V2.4 use the identifiers `V24-N1` and `V24-N2` so that they do not collide with the notes N-1..N-4 of V2.2.

## 4. Status

```text
CT21D_AUTHORITY_CONTRACT = DRAFT_V2_4 (V2.1 + V2.2 + V2.3 + V2.4), draft revision 2
CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
INVENTORY_COMPLETENESS = NOT_ESTABLISHED (until REVIEWED)
NEXT = ARCHITECT DELTA REVIEW (IDENTITY AND TEXT OF V2.4), THEN COORDINATOR TEXTUAL VERIFICATION OF V2.4
```
