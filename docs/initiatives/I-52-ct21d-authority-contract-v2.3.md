# I-52 — CT-21D AUTHORITY CONTRACT V2.3 (DRAFT — N-2 PRE-APPROVED MICRO-ERRATA OVER V2.2)

> **CT-21D AUTHORITY CONTRACT V2.3 — MICRO-ERRATA OVER V2.2 THAT CORRECTS ONLY N-2, FOR COORDINATOR TEXTUAL VERIFICATION. Design / documentation only. Nothing here is executed, built or authorized.**
>
> ```text
> OWNER_ACT1                        = SPONSORED  (OWNER_ACT1_DECISION = A; SPONSOR_ALT21D = YES)
> ALT21D_DIRECTION                  = OWNER_SPONSORED
> OWNER_DECISION_STATUS             = SPONSORED  (sponsorship of the pursuit ONLY; the final guarantee is NOT accepted)
> CT21D_AUTHORITY_CONTRACT          = DRAFT_V2_3  (V2.1 with the V2.2 errata and this micro-errata applied)
> CT21D_AUTHORITY_BASELINE_READY    = FALSE
> CT21D_EXECUTION_READY             = FALSE
> CT21D_EXECUTION                   = NOT_AUTHORIZED
> ALT21D_ADMISSION_PASS             = NOT_YET_SATISFIABLE
> CURRENTLY_ADMISSIBLE_KIND_SCOPE   = NONE
> CTDA_HOST_PASS(V35-A3)            = FALSE (terminal, monotone)
> ContextIsolationAuthority         = UNKNOWN          SafeOperationalState = FALSE_FOR_ADMISSION
> G3 = STOPPED   G3B = NOT OPEN   CT-50 = NOT EXECUTED   ALT-21C = NOT ADMISSIBLE   ALT-21E = FALLBACK_IN_EFFECT
> Freeze V18 = GOVERNING   Freeze V35 = GOVERNING ITS OWN PREDICATE   Replacement Freeze = DOES NOT EXIST   ADR-0036 = PROPOSED / AMENDED FOR V18
> UNDO_EVIDENCE_STATUS              = NOT_EXECUTED
> B5 = ADVANCES_TO_IMPLEMENTATION_PREREQUISITE  (+ REMAINS_BLOCKER_FOR_CT21D_EXECUTION + REMAINS_BLOCKER_FOR_REPLACEMENT_FREEZE)
> B6 = REMAINS_BLOCKER_FOR_ACT2
> NB-1 NB-3 NB-4 NB-6 = ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION (each remains open until its artifact exists)
> NB-2 = REMAINS_BLOCKER_FOR_AUTHORITY_BASELINE      NB-5 = REMAINS_BLOCKER_FOR_CT21D_EXECUTION
> ```
>
> This micro-errata executes nothing, builds no harness, changes no code, creates no baseline artifact and is **not** an authority baseline.

## 0. What V2.3 is and how to read it

The Architect's micro-ruling on note N-2 of V2.2 (`N2_RULING = ACCEPT_WITH_CHANGES`) approved a **closed** text for the supersession of a class-B instance. V2.3 applies that text and **nothing else**.
**V1, V2, V2.1 and V2.2 are intact and historical.** V2.2 (`b047a49e9ee21c7b95aa697cd0caeb545ed40219`, blob `fd7260c57ec05e11f6bbb9f67a728e7dfdf1f609`) applied PA-1..PA-14 and PW-1..PW-5 correctly and those are not touched.

| Item | Rule |
|---|---|
| Effective contract | **`CT-21D-V2.3`** = the text of V2.1, with the clauses of V2.2 applied, with the clauses of this document applied |
| Precedence | for the clauses named below, **V2.3 governs over V2.2**; every other clause of V2.2 and V2.1 stands unchanged |
| Contract hash | the composite identity approach of note N-1 (accepted by the Coordinator) is kept: SHA-256 over the LF-normalized texts of the V2.1 blob, the V2.2 blob and the V2.3 blob in that order, plus the V5 blob id (deviation D-1) |
| Notes N-1, N-3, N-4 of V2.2 | **not modified** (accepted by the Coordinator) |
| Note N-2 of V2.2 | **resolved by this document**: the V2.2 paragraph "Same rule, class B (extension, see note N-2)" of PW-1 and the N-2 row of the notes table are **replaced** by section 1 |

| V2.2 clause | Treatment in V2.3 |
|---|---|
| PW-1, first part (class-A `SUPERSEDED_FALSE_INSTANCE`) | **unchanged** |
| PW-1, paragraph "Same rule, class B (extension, see note N-2)" | **replaced** by section 1.1 |
| Notes table, row N-2 | **replaced** by "N-2: resolved by V2.3 section 1" |
| V2.1 section 24 (Owner Act 2 evidence) | **added**: item 16 (section 1.7) |

## 1. N-2 — Supersession of a class-B instance

### 1.1 Replacement text (the Architect's closed text)

> **Class B (PW-1, accepted with conditions).** An instance in `AMENDMENT_PENDING` may take the designation `SUPERSEDED_AMENDMENT_PENDING_INSTANCE` **only if all** of these hold: (1) every open `AMENDMENT_PENDING` obligation of the instance has the cause `PRODUCT_OR_SEAM_DEFECT`, fixed by a `CLASSIFICATION_RULING`; (2) a corrected build creates a new build-bound tuple, that is, a new evaluation instance under V2.1 section 1.7 and not a retry; (3) the control contract is unchanged: the `CONTROL_CLASS_REGISTRY`, the control classes, the control criteria, the scenario expectations, the fingerprint specification and the manifest procedure have equal hashes before and after the correction, and if any of them changes, this rule does not apply and the reviewed-amendment exits of V2.1 section 1.6 apply; (4) a hash-linked `SUPERSESSION_RECORD` is written with the old instance, the ruling, the defect and correction reference, the corrected tuple and the statement of item 3.
>
> The designation is an attribute: the state stays `AMENDMENT_PENDING`, satisfies neither exit A nor exit B of V2.1 section 1.6, is retained as immutable historical evidence, is disclosed in Owner Act 2, and the old instance never becomes `TRUE`. The new instance starts `NOT_EVALUATED`, evaluates all groups and receives no transferred evidence.
>
> Obligations with cause `HOST_LIMITATION`, `NOT_MEASURABLE` or `EMPIRICAL_BEHAVIOR_DIFFERENCE` are not supersedable and leave `AMENDMENT_PENDING` only through exit A or B. If any such obligation is open, the instance takes no designation. If a class-A `FALSE` is also present, `FALSE` prevails and PW-1 applies.

### 1.2 State versus designation

`SUPERSEDED_AMENDMENT_PENDING_INSTANCE` is a **designation (an attribute)**. It is **not a new predicate state**.

| Field of the old instance | Value |
|---|---|
| `STATE` | `AMENDMENT_PENDING` (unchanged) |
| `DESIGNATION` | `SUPERSEDED_AMENDMENT_PENDING_INSTANCE` |

The designation:

- does **not** satisfy exit A (a reviewed amendment plus the required recharacterization);
- does **not** satisfy exit B (a recorded determination that no acceptable amendment exists, which gives `FALSE`);
- does **not** make the old instance `TRUE`, ever;
- keeps the old instance as **immutable historical evidence**;
- must be **disclosed in Owner Act 2** (section 1.7);
- **transfers no evidence** to the new tuple.

**"Superseded" does not mean** resolved, passed, closed, forgiven or removed. The historical `AMENDMENT_PENDING` remains historical evidence and is never deleted or edited.

### 1.3 The four conditions (all are required)

| # | Condition |
|---|---|
| 1 | every open `AMENDMENT_PENDING` obligation of the instance has the cause `PRODUCT_OR_SEAM_DEFECT`, **established by a `CLASSIFICATION_RULING`** |
| 2 | a corrected build creates a **new build-bound tuple** and a **new evaluation instance**; it is **not a retry** |
| 3 | the **control contract is unchanged**: identical hashes before and after the correction of the `CONTROL_CLASS_REGISTRY`, the control classes, the control criteria, the scenario expectations, the fingerprint specification and the manifest procedure. If any of them changes, **this supersession rule does not apply** and the normal reviewed-amendment path (V2.1 section 1.6) applies |
| 4 | a **hash-linked `SUPERSESSION_RECORD`** is written (section 1.4) |

### 1.4 `SUPERSESSION_RECORD`

The record contains:

- the **old instance identity** (contract version and baseline tuple);
- the **`CLASSIFICATION_RULING`**;
- the **defect reference**;
- the **correction reference**;
- the **corrected (new) tuple**;
- the **explicit assertion that the control-contract hashes of condition 3 are unchanged** (with those hashes).

"Hash-linked" is read operationally (deviation D-2): the record carries the hash of each artifact it references (the sealed evidence package of the old instance, the ruling, the defect and correction references, the control-contract artifacts), and its own hash is recorded in the Owner Act 2 evidence package.

### 1.5 Causes and instance state (reproduced from the micro-ruling)

| CAUSE | OLD_INSTANCE_STATE | NEW_BUILD_REQUIREMENT | CONTRACT_VERSION_REQUIREMENT |
|---|---|---|---|
| `PRODUCT_OR_SEAM_DEFECT` (class B), every open obligation of this cause | `AMENDMENT_PENDING` with the designation `SUPERSEDED_AMENDMENT_PENDING_INSTANCE`; immutable historical evidence | corrected build = new tuple = new instance; full re-evaluation; no evidence transfer; new EVM version if the corrected code affects events (section 1.6) | the same contract version, **only if** the control contract is unchanged; otherwise a reviewed amendment |
| `HOST_LIMITATION` (class B) | `AMENDMENT_PENDING`; **not supersedable** | a build does not cure it; a new tuple may be evaluated but does not discharge the obligation | reviewed amendment plus recharacterization (exit A), or a recorded determination that no acceptable amendment exists (exit B, `FALSE`) |
| `NOT_MEASURABLE` (host limitation) | as `HOST_LIMITATION` | as `HOST_LIMITATION` | as `HOST_LIMITATION` |
| `EMPIRICAL_BEHAVIOR_DIFFERENCE` | `AMENDMENT_PENDING`; **not supersedable** | a build does not cure it | as `HOST_LIMITATION` |
| mixed: at least one obligation of host or empirical cause is open | plain `AMENDMENT_PENDING`, **no designation** | as the row of its cause | as the row of its cause |
| other non-pass (`INSTRUMENT_DEFECT`, unclassified `UNKNOWN`, `INVALID`) | `NOT_EVALUATED`; **no `AMENDMENT_PENDING` is created** | repair and requalification, or a `CLASSIFICATION_RULING` | not applicable |

### 1.6 Consequences and boundaries

- **Non-supersedable causes.** `HOST_LIMITATION`, `NOT_MEASURABLE` caused by a host limitation, and `EMPIRICAL_BEHAVIOR_DIFFERENCE` are **not** supersedable merely by a new build. They remain `AMENDMENT_PENDING` until **exit A** (reviewed amendment plus required recharacterization) or **exit B** (recorded determination that no acceptable amendment exists, giving `FALSE`). A new build or tuple does **not** discharge them.
- **Mixed instance.** If **any** open `AMENDMENT_PENDING` obligation is `HOST_LIMITATION`, `NOT_MEASURABLE` (host limitation) or `EMPIRICAL_BEHAVIOR_DIFFERENCE`, there is **no** `SUPERSEDED_AMENDMENT_PENDING_INSTANCE` designation: the old instance remains plain `AMENDMENT_PENDING`.
- **Class-A precedence.** If the same instance also contains a class-A `FALSE`, **`FALSE` dominates** and the already approved PW-1 semantics apply (`SUPERSEDED_FALSE_INSTANCE` where applicable). No competing supersession state is created.
- **Other causes.** `INSTRUMENT_DEFECT` gives `NOT_EVALUATED` (repair and qualification). An unclassified `UNKNOWN` gives `NOT_EVALUATED` until a `CLASSIFICATION_RULING`. `INVALID` does not create `AMENDMENT_PENDING`. **No extra class-B cause category is created.**
- **EVM consequence.** If the corrected seam or product code affects host events, a **new EVM version is required** according to the existing EVM contract (V2.1 section 27.5). This is a requirement of the **new evaluation instance**. It is **not by itself a control-contract amendment**, unless the normative control contract also changes.
- **Instance-bound predicate.** `TRUE` remains per instance (V2.1 section 1.7): the new instance may reach `TRUE` on its own evidence, and the old instance never does.

### 1.7 Owner Act 2 disclosure (addition to V2.1 section 24)

Item 16 is added to the evidence-package requirements of Owner Act 2:

> 16. **superseded instances**: for each, its state, its designation, its `SUPERSESSION_RECORD`, its cause, the defect and correction references, the old and the new tuples, and the evidence of contract-hash equality (section 1.3, condition 3).

## 2. Deviations and clarifications for the Coordinator's textual verification

| Id | Content |
|---|---|
| D-1 | the composite contract identity of note N-1 is extended by one document: the hash spans the V2.1, V2.2 and V2.3 blobs. It is the mechanical consequence of N-1; N-1 itself is not modified |
| D-2 | "hash-linked" in the `SUPERSESSION_RECORD` is stated operationally (section 1.4): each referenced artifact by hash, and the record's own hash in the Owner Act 2 evidence package |

There is **no other deviation** from the pre-approved text. The verbatim text of section 1.1 is the semantic authority.

## 3. Blockers (unchanged)

| Id | Status |
|---|---|
| B1, B2, B7 | `CLOSED_BY_V3_CONTRACT` |
| B3 | `CLOSED_BY_V4_CONTRACT` |
| B4, B8 | `CLOSED_BY_V2_CONTRACT` |
| **B5** | `ADVANCES_TO_IMPLEMENTATION_PREREQUISITE` + `REMAINS_BLOCKER_FOR_CT21D_EXECUTION` + `REMAINS_BLOCKER_FOR_REPLACEMENT_FREEZE` |
| **B6** | `REMAINS_BLOCKER_FOR_ACT2` |
| **NB-1** | `ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION`; open until sealed |
| **NB-2** | `REMAINS_BLOCKER_FOR_AUTHORITY_BASELINE` |
| **NB-3** | `ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION`; open until the independent artifact, its hash and its agreement exist |
| **NB-4** | `ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION`; open until the inventory, the completeness review and the corpus artifacts exist |
| **NB-5** | `REMAINS_BLOCKER_FOR_CT21D_EXECUTION` |
| **NB-6** | `ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION`; open until the procedure, the remote anchor and the agreement exist |

## 4. Final status

```text
OWNER_ACT1 = SPONSORED     OWNER_DECISION_STATUS = SPONSORED (pursuit only)
CT21D_AUTHORITY_CONTRACT = DRAFT_V2_3 (V2.1 + V2.2 + V2.3)
CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
CURRENTLY_ADMISSIBLE_KIND_SCOPE = NONE     ALT21D_ADMISSION_PASS = NOT_YET_SATISFIABLE
SELECTIVE_AUTHORITY_DESIGN_STATUS = COMPLETE_FOR_BASELINE (DRAFT_V2_3; pending agreement)
UNDO_EVIDENCE_STATUS = NOT_EXECUTED
CTDA_HOST_PASS = FALSE (terminal)     CIA = UNKNOWN     SafeOperationalState = FALSE_FOR_ADMISSION     G3 = STOPPED     G3B = NOT OPEN     CT-50 = NOT EXECUTED
ALT-21E = FALLBACK_IN_EFFECT     Freeze V18 = GOVERNING     Freeze V35 = GOVERNING ITS OWN PREDICATE     Replacement Freeze = DOES NOT EXIST     ADR-0036 = PROPOSED / AMENDED FOR V18
HOST VALIDATION = NOT RUN     BRANCH = NOT RECONCILED
NEXT GATE = COORDINATOR TEXTUAL VERIFICATION OF CT-21D AUTHORITY CONTRACT V2.3
IF PASS: CT21D_AUTHORITY_CONTRACT_STATUS = AGREED_FOR_BASELINE_ARTIFACT_PREPARATION, THEN PREPARATION OF AUTHORITY BASELINE ARTIFACTS
```
