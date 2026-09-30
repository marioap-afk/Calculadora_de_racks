# I-52 — Proposal ALT-21D V5: pre-approved final errata over V4

> **PROPOSAL ALT-21D V5 — DRAFT FOR COORDINATOR TEXTUAL VERIFICATION. Architecture / documentation only.**
>
> ```text
> ALT21D_DIRECTION                = PLAUSIBLE_NOT_YET_VIABLE
> ALT21D_ADMISSION_PASS           = NOT_YET_SATISFIABLE
> CURRENTLY_ADMISSIBLE_KIND_SCOPE = NONE
> OWNER_DECISION_STATUS           = NOT_YET
> CT21D_STATUS                    = NOT_AUTHORIZED
> CTDA_HOST_PASS(V35-A3)          = FALSE (terminal, monotone)
> ContextIsolationAuthority       = UNKNOWN
> SafeOperationalState            = FALSE_FOR_ADMISSION
> G3 = STOPPED   G3B = NOT OPEN   CT-50 = NOT EXECUTED
> ALT-21C = NOT ADMISSIBLE        ALT-21E = FALLBACK_IN_EFFECT
> ADR-0036 = PROPOSED / AMENDED FOR V18      Freeze V18 = GOVERNING      Freeze V35 = UNCHANGED
> Replacement Freeze = DOES NOT EXIST        SUBSTANTIVE IMPLEMENTATION = BLOCKED
> ```
>
> **V5 does not declare `OWNER_DECISION_STATUS = READY_FOR_SPONSORSHIP_DECISION`.** That transition is a Coordinator ruling, made only after the Coordinator has verified S-1 and W-1..W-5 textually and semantically.
> V5 requests no Owner decision and authorizes no AutoCAD, host run, `CT-21D`, product code, Freeze, ADR amendment or G3 change.

## 0. What V5 is

V5 is a **minimal errata document over V4**. It applies exactly the six items that the Architect's item-by-item check of V4 (`CHANGES_REQUIRED`, minimal) pre-approved: **S-1, W-1, W-2, W-3, W-4, W-5**. It does not reinterpret them.
It does not redesign ALT-21D, change outcomes beyond what is stated, change the phase model, the admission semantics or the blockers. **V1, V2, V3 and V4 are not edited.**

```text
V1 -> V2 -> V3 -> V4 -> Architect item-by-item check of V4 (CHANGES_REQUIRED, minimal; C-1 SATISFIED, C-2 ACCEPTABLE_FOR_ACT1, C-3 SATISFIED,
      W-A SATISFIED, C-5 RECORD_CORRECT, B3 CLOSED_BY_V4_CONTRACT) -> V5 (this document) -> Coordinator textual verification of V5
```

### 0.1 What V5 amends

| Where | Item | V5 section |
|---|---|---|
| V4 section 4 (Owner Act 1 summary), item 8 | S-1 | 1 |
| V4 section 1.1 (`T_M` lifecycle) | W-1 | 2 |
| V3 sections 6.1 and 6.3 (rows and wording for O4, O5, O7; meaning of "committed") | W-2 | 3 |
| V4 section 6 item 3 and item 4 (E4 identification) | W-3 | 4 |
| V4 section 8, row V4-RA-4 | W-4 | 5 |
| V4 section 3 (PRE_COMMAND_STATE_RECORD and manifest) | W-5 | 6 |

Everything else in V1-V4 stays in force as recorded there.

## 1. S-1 — Owner Act 1 disclosure, item 8

Item 8 of the Owner Act 1 summary (V4 section 4) is amended **only** by appending, at its end, the sentence pre-approved by the Architect, verbatim. The complete item 8 now reads:

> 8. **No post-commit rollback guarantee.** After `Commit()`, ALT-21D supports **no** rollback. Outcomes `FAILURE_COMMITTED_DIVERGENT` (O4), `COMMITTED_UNVERIFIED` (O5) and `COMMITTED_MATCH_CONTINUITY_FAILED` (O7) **may leave persistent changes in the drawing**.
> `ABORT_UNVERIFIED` (O6) can also leave MUTATION content in the drawing, notably when `Commit()` raised an exception or its result could not be established: an abort is confirmed only by a state read, and that confirmation can fail.

The disclosure of O4, O5 and O7 in the first sentences is kept unchanged. No cause is added: the text does not use *partial commit*, *corruption*, *interference* or *product defect* as a factual explanation.

For convenience, the **consolidated Owner Act 1 summary** below is the V4 summary with item 8 replaced by the text above; items 1-7 and 9-13 and the exclusions are **identical to V4 section 4**.

**OWNER ACT 1 SUMMARY — template; NOT PRESENTED; NO OWNER DECISION IS REQUESTED BY THIS DOCUMENT.**

1. **What is proposed.** To pursue a reduced-guarantee direction (ALT-21D) for RACKMIRROR, under which the command reports success only on **two discrete observations** of a fixed list of elements plus mandatory controls
   evaluated at a completion point. It is **not** an isolation guarantee; `ContextIsolationAuthority` remains UNKNOWN.
2. **Temporal residual (W-scan).** The drawing may change during verification (after an element's last read, or changing and being restored between observations); nothing suspends host callbacks and there is no global
   version. Anything after the completion point (`W3`) is outside the guarantee. Characterization may measure detection; it does not prove stability.
3. **Observability limits.** Native load events are not observable; the known-module attestation identifies enumerable code by file hash, not the loaded image, and proves nothing about other code (LISP, VBA, in-process COM,
   reflection, data-file modules). Events cannot identify their origin.
4. **Kind scope.** `CURRENTLY_ADMISSIBLE_KIND_SCOPE = NONE`. Proposed maximum: Selective only.
5. **Selective seam.** Pending: a caller-owned creation seam with rollback authority, designed after Act 1, implemented under a separate workflow before host execution, with host evidence before any Freeze. Cost and risk are external to I-52.
6. **UNDO.** Pending for later stages; no recovery path is guaranteed; any guidance is provisional.
7. **Fallback.** `ALT-21E` remains the fallback in effect if the direction is not pursued or fails.
8. **No post-commit rollback guarantee.** After `Commit()`, ALT-21D supports **no** rollback. Outcomes `FAILURE_COMMITTED_DIVERGENT` (O4), `COMMITTED_UNVERIFIED` (O5) and `COMMITTED_MATCH_CONTINUITY_FAILED` (O7) **may leave persistent changes in the drawing**.
   `ABORT_UNVERIFIED` (O6) can also leave MUTATION content in the drawing, notably when `Commit()` raised an exception or its result could not be established: an abort is confirmed only by a state read, and that confirmation can fail.
9. **Preparation residue.** The library imports of PREPARE-W are **not** part of the atomicity of MUTATION; they **may persist after an Abort**; they are recorded in the `PREPARE_RESIDUE_MANIFEST`.
10. **False terminal failure and availability cost.** A semantically correct result can be reported as `COMMITTED_MATCH_CONTINUITY_FAILED` when a mandatory class-B control fails or cannot be evaluated. That is a **terminal
    non-success even though the result may be correct**. The cost in false rejections is **not yet quantified**. Two unverified assumptions are possible sources: **A-1** (read-only opens emit no write events) and **A-2**
    (the host emits no deferred write events after the commit); both must be characterized.
11. **Current admission.** `ALT21D_ADMISSION_PASS` is `NOT_YET_SATISFIABLE` and `CURRENTLY_ADMISSIBLE_KIND_SCOPE` is `NONE`: nothing can be admitted today.
12. **A negative result is possible.** `CT-21D` may show that the instruments are not sufficient, that an assumption is false, that a predicate cannot be satisfied, or that ALT-21D has **no admissible environment**. Act 1 therefore
    does **not** presuppose that the work ends in adopting ALT-21D. `ALT-21E` remains the fallback.
13. **Class B degradation.** A class-B control can be degraded, withdrawn or changed from mandatory to telemetry **only** by an explicit, reviewed amendment; never automatically.

**Act 1 DOES NOT:** authorize CT-21D execution; reopen G3; change `SafeOperationalState`; make CIA known; issue a replacement Freeze; amend ADR-0036; accept the final ALT-21D guarantee; make Selective admissible;
authorize product implementation outside future, separately governed gates. **Act 1 only** sponsors or redecides the **pursuit** of the reduced-guarantee direction, supplementing the O-1 V18 restriction for that future path.

## 2. W-1 — the fixing point of `T_M`

The following is added to V4 section 1.1 and does not alter C-1:

> The state of `T_M` is **fixed at the instant the call to `Commit()` returns normally or throws.** After that instant, an exception during `Dispose`, a cleanup, a later release, or any other later operation **does not retroactively change the state of `T_M`.**
> Any such later event is recorded only as a **secondary finding** of the report.

The attempt at `Abort()` and disposal prescribed by V4 section 1.2 step 3 for the states `FAILED` is a consequence of that state, not a change of it.

## 3. W-2 — meaning of "committed"

### 3.1 The definition

For the outcomes O4, O5 and O7, and in their names and reports, **"committed" means exclusively: `Commit()` returned normally** (`T_M` state `COMMIT_ACCEPTED`).
It does **not** by itself mean: persistence verified; semantic correctness verified; durable state proven; state matched expected.

### 3.2 Corrections reproduced from V3 (V3 is not edited)

V3 section 6.1 column "Expected persistent state" and V3 section 6.3, for O4, O5 and O7, are read as follows. The conditions, the semantics and the precedence of V3 6.2 are **unchanged**.

| Outcome | Expected persistent state (V3 6.1, corrected wording) |
|---|---|
| O4 `FAILURE_COMMITTED_DIVERGENT` | `Commit()` returned normally; the observed persisted state differs from the expected one |
| O5 `COMMITTED_UNVERIFIED` | `Commit()` returned normally; relation to the expected state unknown |
| O7 `COMMITTED_MATCH_CONTINUITY_FAILED` | `Commit()` returned normally; equals expected at both observations; continuity not established |

V3 section 6.3, first paragraph: "O5 concerns a **committed MUTATION** whose relation to the expected state could not be established" reads "O5 concerns a **MUTATION for which `Commit()` returned normally** and whose relation to the expected state could not be established". O6 is unchanged.

| Outcome | State expectation (V3 6.3, corrected) | Canonical report (corrected) |
|---|---|---|
| O4 | `Commit()` returned normally; differs from expected | "`Commit()` returned normally. The observed persisted state differs from the expected persisted state for: {elements}." No cause is stated |
| O5 | `Commit()` returned normally; relation unknown | "`Commit()` returned normally. Verification could not reach a conclusion: {reasons}. The persisted state is unknown relative to the expected state." |
| O7 | `Commit()` returned normally; equal at both observations; continuity not established | "`Commit()` returned normally. The observed persisted state matched the expected state in both observations, but the mandatory control(s) {ids} failed or could not be evaluated. Continuity of the environment is not established." |

The provisional operator guidance of V3 6.3 for O4, O5 and O7 stays and is **NOT A GUARANTEED RECOVERY PATH**. The outcome **names** keep their tokens; "COMMITTED" in them is read as defined in 3.1.

The canonical SUCCESS statement of V3 1.2 is **not** changed by W-2; the words "post-commit verification" there name the phase that follows a normal return of `Commit()`.

## 4. W-3 — E4 identified verbatim by reference to V3 section 1.2

V4 section 6 identified the sanctioned E4 segment with an abbreviated quotation. That abbreviation is **superseded**. V5 does **not** reproduce E4, so that no second textual authority exists.

**The only textual authority for the canonical SUCCESS statement, and for E4 in English and in Spanish, is V3 section 1.2.**

- The canonical statement is the text of the two code-block paragraphs of V3 1.2 (EN and ES), unchanged.
- **E4** is, in each language, the **last sentence of that paragraph** (English: the sentence that begins "This report does not state..."; Spanish: the sentence that begins "Este informe no afirma..."), taken **verbatim from V3 1.2**. The phrases in this paragraph locate the sentence; they are not its text.
- E1-E3 are the preceding sentences of the same paragraph, including the placeholder `{N}`.

The future T0 rule (not implemented now) is therefore:

| Check | Rule |
|---|---|
| **A** | the canonical SUCCESS claim **excluding the exact sanctioned E4 segment** is checked against the prohibited vocabulary; it must contain none |
| **B** | the E4 segment is checked for **exact verbatim equality** against the phrase of V3 section 1.2 for the same language |

All other provisions of V4 section 6 (scope of the rule, exempt quotations and explanations, prohibited vocabulary lists) are unchanged.

## 5. W-4 — fingerprint method and TBD-10 are separate

V4 section 8 row V4-RA-4 tied the fingerprint method to TBD-10. That reference is corrected:

```text
FINGERPRINT_METHOD  =  TO_BE_FIXED_BY_CT21D_KIND_AUTHORITY_BASELINE
gate                =  BEFORE_CT21D_BASELINE
```

The concrete method used to decide that a definition was imported and to record its state (the fingerprint of imported or relevant definitions, V3 5.2 and V4 3.3-3.4) belongs to the **CT-21D KIND AUTHORITY BASELINE**, the same baseline that fixes the per-kind reads of TBD-13. No new TBD number is created.

`TBD-10` keeps only its own meaning: **`CACHE_FRESHNESS`**, with its own gate `BEFORE_CT21D_EXECUTION`. The two are **not merged**.

Ruling recorded: **V4-RA-4 = `DEFER_TO_CT21D_AUTHORITY_BASELINE`**.

## 6. W-5 — composition rule for a preexisting definition modified by PREPARE-W

When PREPARE-W **modifies a preexisting definition** (as opposed to adding a new one), the composition of `PRE_COMMAND_STATE_RECORD` and `PREPARE_RESIDUE_MANIFEST` into `EXPECTED_AFTER_ABORT` requires a **kind-specific rule**. That rule must define:

1. what the preexisting state represents in the `PRE_COMMAND_STATE_RECORD`;
2. which change produced by PREPARE-W is part of the permitted residue;
3. how `EXPECTED_AFTER_ABORT` is verified for that definition;
4. how the permitted residue is distinguished from an unexpected modification.

```text
COMPOSITION RULE  =  TO_BE_FIXED_BY_CT21D_KIND_AUTHORITY_BASELINE
gate              =  BEFORE_CT21D_BASELINE
linked to         =  TBD-13 (abort verification read set) and the kind authority baseline
```

**A partially failed import is never made expected.** If an import fails and leaves an effect that is not authorized, ABORT-VERIFY detects it; the manifest does not legitimize it (V4 section 3.4 stands: an import enters the manifest only when it completed and its fingerprint was recorded).

## 7. Architect rulings preserved (unchanged)

| Ruling | Recorded |
|---|---|
| V4-RA-1 | the current model is sufficient; does not block Act 1 |
| V4-RA-2 | CORRECT |
| V4-RA-3 | the current contract is sufficient; a partially failed import is not legitimized |
| V4-RA-4 | `DEFER_TO_CT21D_AUTHORITY_BASELINE` (section 5) |
| V4-RA-5 | `DOCUMENTATION_ONLY` |
| RA-1..RA-6 of V4 | unchanged |

## 8. Blockers (unchanged)

| Id | Status |
|---|---|
| B1 | `CLOSED_BY_V3_CONTRACT` |
| B2 | `CLOSED_BY_V3_CONTRACT` |
| B3 | `CLOSED_BY_V4_CONTRACT` |
| B4 | `CLOSED_BY_V2_CONTRACT` |
| B5 | `REMAINS_BLOCKER_FOR_CT21D_DESIGN` |
| B6 | `REMAINS_BLOCKER_FOR_ACT2` |
| B7 | `CLOSED_BY_V3_CONTRACT` |
| B8 | `CLOSED_BY_V2_CONTRACT` |

"Closed by contract" means only that the contractual defect is corrected; not that code, authority or evidence exists.

## 9. Items for the Coordinator's textual verification (closed list)

| Item | Where | Verify |
|---|---|---|
| S-1 | section 1 | the appended sentence equals the pre-approved text character by character; O4/O5/O7 disclosure kept; consolidated summary identical to V4 section 4 except item 8 |
| W-1 | section 2 | the fixing point is the return or throw of `Commit()`; later events are secondary findings only |
| W-2 | section 3 | "committed" defined as `Commit()` returned normally; the three reports start with that phrase; semantics and precedence unchanged |
| W-3 | section 4 | E4 defined by reference to V3 1.2; no reproduction; checks A and B |
| W-4 | section 5 | `FINGERPRINT_METHOD` gate; TBD-10 = `CACHE_FRESHNESS`; not merged |
| W-5 | section 6 | composition rule with its four elements, gate, link to TBD-13, no legitimization of a partial import |

```text
PROPOSAL ALT-21D V5 = PUBLISHED / COORDINATOR TEXTUAL VERIFICATION REQUIRED
V1, V2, V3, V4 = HISTORICAL / NOT EDITED
B1 B2 B7 = CLOSED_BY_V3_CONTRACT   B3 = CLOSED_BY_V4_CONTRACT   B4 B8 = CLOSED_BY_V2_CONTRACT   B5 = REMAINS_BLOCKER_FOR_CT21D_DESIGN   B6 = REMAINS_BLOCKER_FOR_ACT2
ALT21D_DIRECTION = PLAUSIBLE_NOT_YET_VIABLE      ALT21D_ADMISSION_PASS = NOT_YET_SATISFIABLE      CURRENTLY_ADMISSIBLE_KIND_SCOPE = NONE
OWNER_DECISION_STATUS = NOT_YET                  CT21D_STATUS = NOT_AUTHORIZED
CTDA_HOST_PASS = FALSE   CIA = UNKNOWN   SafeOperationalState = FALSE_FOR_ADMISSION   G3 = STOPPED   ALT-21E = FALLBACK_IN_EFFECT
SUBSTANTIVE IMPLEMENTATION = BLOCKED   BRANCH = NOT RECONCILED (79 behind / 122 ahead before this commit)   HOST VALIDATION = NOT RUN
NEXT GATE = COORDINATOR TEXTUAL VERIFICATION OF ALT-21D PROPOSAL V5
```
