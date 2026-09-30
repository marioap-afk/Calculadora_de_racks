# I-52 — Proposal ALT-21D V3: delta contract revision after the Architect review of V2

> **PROPOSAL ALT-21D V3 — DRAFT FOR COORDINATOR REVIEW AND ARCHITECT DELTA REVIEW. Architecture / documentation only.**
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
> None of these statuses changes with this document, **even though V3 attempts to close blockers B1, B2, B3 and B7 at contract level**.
> Owner and CT-21D statuses stay as above until the Coordinator has reviewed V3 and the Architect has completed a delta review of it.
> This document requests no Owner decision and authorizes no AutoCAD, host run, `CT-21D`, product code, Freeze, ADR amendment or G3 change.

## 0. What V3 is, and how it relates to V1 and V2

V3 is a **delta**. It restates in full only what the Architect's ruling on V2 required to change and keeps every other V2 section in force by reference. It does
not redo the design. **V1 and V2 are not edited** (their blobs `bcb44b5e...` and `aac56c41...` are unchanged).

```text
V1 (324c0cbd) -> Architect ruling: AGREED_WITH_REQUIRED_CHANGES (ALT-21D V1 requires changes)
   -> V2 (bb7b86a2) -> Architect delta ruling: AGREED_WITH_REQUIRED_CHANGES, conclusion C (V2 requires further contract changes)
   -> V3 (this document) -> Coordinator review of V3 -> Architect delta review of V3
```

### 0.1 Supersession map

| V2 section | Status in V3 |
|---|---|
| 0 Disposition, 0.2 blockers | superseded by 14 |
| 1 Baseline | in force; add V2 (`bb7b86a203965bbcbcee7032a9854df726323c46`, blob `aac56c4106e12f3648529ad07b6937db2f958e3d`) as historical |
| 2 Replacement authority and CIA | in force, with the RG-2 wording replaced by 1.1 |
| 3 Temporal model | **superseded by 1, 2** |
| 4 Two classes of control | in force, with E-10 and E-12 amended by 3 and 4 |
| 5 Controls table (E-01..E-15) | in force except E-10, E-11M, E-12 rows, **replaced by 3 and 4**; 5.1 clarified by 3; 5.2 superseded by 4; 5.3 clarified by 3.3 |
| 6 Non-guarantees | in force with the amendments of 3.3, 2.5 and 4.6 |
| 7 Phase model | **superseded by 5** |
| 8 Lock and transaction ownership | in force unchanged (B4 closed) |
| 9 Kind scope | in force; sequencing in 9 of V3 |
| 10 Outcome model | **superseded by 6** |
| 11 UNDO | in force, with the sequence of 10 |
| 12 `04NO-S` restrictions | in force unchanged |
| 13 Semantics matrix | **superseded by 8** |
| 14 Manifest identity | in force unchanged |
| 15 Predicate and TBD table | predicate in force with the continuity set of 3.2; TBD table **superseded by 11** |
| 16 Evidence reuse | in force unchanged |
| 17 Owner sequence | in force with the naming of 12; Owner Act 1 summary added in 12 |
| 18 to 21 | in force; OQ-1..OQ-8 are answered by the Architect and their outcome is recorded in 13 |

### 0.2 Where each required change is handled

| Ruling item | V3 section |
|---|---|
| S-1 bounded SUCCESS claim, read-only VERIFY, sealed element list | 1 |
| S-2 new outcome and precedence | 6 |
| S-3 E-12 by phase, protected objects | 4 |
| S-4 E-10 snapshots | 3 |
| S-5 W3 wholly residual | 2 |
| S-6 PREPARE-R / PREPARE-W | 5 |
| S-7 EXPECTED_AFTER_ABORT | 7 |
| S-8 O5 and O6 disjoint | 6 |
| S-9 semantics matrix | 8 |
| S-10 UNDO sequence | 10 |
| S-11 Selective seam sequence | 9 |
| S-12 TBD reassignment | 11 |
| S-13 Act 1 exclusions and summary | 12 |
| W-1 "authority baseline" naming | 11, 12 |
| W-2 no causal wording in DIVERGENT | 6.3 |
| W-3 blocker vocabulary | 14 |
| W-4 native events versus snapshots | 3 |

## 1. The SUCCESS contract and `W-scan` (RED-3, amended)

### 1.1 What SUCCESS asserts, and only that

RG-2 of V2 is replaced by:

> **RG-2.** If the run reports `SUCCESS`, then (a) two separate observations of the sealed element list (1.3), taken one after the other between `V0` and `V1`,
> both equal the expected state, and (b) the mandatory controls applicable at the completion point `CP` were evaluated as satisfied. `SUCCESS` asserts nothing else.

`SUCCESS` **does not** assert continuous stability, continuous persistence, absence of interference, or that the state stayed constant during `W-scan`.
The two-pass scheme is a contract of **discrete observations**. It is not evidence of stability.

### 1.2 Canonical user-facing SUCCESS statement

The English text governs. A localized rendering must keep the four elements **E1** two separate observations, **E2** a fixed list, **E3** mandatory controls
evaluated as satisfied at the completion point, **E4** the explicit non-claims.

```text
EN: RACKMIRROR finished. Two separate read-only observations of the fixed list of {N} elements, taken one after the other between the start and the end
    of the post-commit verification (V0 to V1), both matched the expected state, and the mandatory controls were evaluated as satisfied at the completion
    point. This report does not state that the drawing stayed unchanged while it was being verified, does not state that nothing else acted on the drawing,
    and says nothing about changes after the completion point.

ES: RACKMIRROR terminó. Dos observaciones separadas, de solo lectura, de la lista fija de {N} elementos, hechas una tras otra entre el inicio y el fin de la
    verificación posterior al commit (V0 a V1), coincidieron con el estado esperado, y los controles obligatorios se evaluaron como satisfechos en el punto de
    finalización. Este informe no afirma que el dibujo permaneciera sin cambios mientras se verificaba, no afirma que nada más actuara sobre el dibujo y no dice
    nada sobre cambios posteriores al punto de finalización.
```

The success report must not contain the words *stable, unchanged (as a claim), intact, safe, isolated, guaranteed* or an equivalent in any language. Non-normative
diagnostics may follow the statement but cannot change its meaning; a run with a normative warning is not `SUCCESS`.

### 1.3 Sealed element list (`VERIFY_ELEMENT_LIST`)

- The exact list of elements to verify is computed **before** `Commit()`, at PV, from the plan and the expected values, and **sealed** (a hash of the list and of each element's
  expected digest is recorded). It contains the destination authority elements (per sibling), the destination materialization elements, the NOD registry elements when the kind
  consumes them, and the source read-set elements.
- The list **cannot grow, shrink or change** during pass 1 or pass 2.
- Enumerating a container during VERIFY is a check **against** the sealed list, not a source of new list members. A member found that is not in the list, or a listed member not
  found, is an observation that differs from the expected state (outcome `FAILURE_COMMITTED_DIVERGENT`). Discovering a new element during VERIFY can never produce `SUCCESS`.
- Both passes read the same sealed list in the same order.

### 1.4 VERIFY is read-only

- Every VERIFY open is read-only; no upgrade to write, no write-capable API is reachable by the verifier. A verifier that writes, upgrades, or modifies any object is a **defect** and fails closed:
  the outcome is `COMMITTED_UNVERIFIED` with reason `VERIFIER_DEFECT` (or `FAILURE_COMMITTED_DIVERGENT` if a difference from the expected state was also observed, by precedence in 6.2).
- Enforcement is a contract requirement to be met at implementation: the verifier accesses the drawing only through a read-only abstraction; a static guard (T0) and behavioural tests (T1) forbid write paths.

### 1.5 Timeline (amends V2 sections 3.1-3.2)

```text
P0   entry (no lock, no product write)
PL   lock acquired
PR   end of PREPARE-R: decision record complete (5.1)
PP   end of PREPARE: after PREPARE-W (manifest emitted) if any import happened, otherwise equal to PR
PS   inside T_M: source read S, re-observation of PREPARE outputs
PV   pre-commit: staged reads done; VERIFY_ELEMENT_LIST sealed; continuity evaluated; last point at which abort is possible
     -- Commit() --
V0   start of the first read of pass 1
V1   end of the last read of pass 2
PC   continuity evaluation performed after V1
CP   the contractual completion point = PC
```

Pass 1 and pass 2 are unchanged from V2: pass 1 reads every sealed element and compares it with the expected value through the independent comparator; pass 2 re-reads every sealed element
in the same order and compares each with its pass-1 value **and** with the expected value. Continuity is evaluated after the last read of pass 2.

### 1.6 The temporal residual, and where it must appear

The `W-scan` residual is real and is **not** closed by documenting it: an element can change after its own pass-2 read (`W-scan tail`); an element can change and be restored between its two
observations (`W4`); there is no global database version and no host mechanism that suspends callbacks. The residual must appear, in the same words, in **(a)** this Proposal, **(b)** the
Owner Act 1 summary (section 12) and **(c)** the Owner Act 2 disclosure. `CT-21D` may characterize the detection capability of the scheme with adversarial writers at different temporal
positions; `CHARACTERIZATION != PROOF_OF_STABILITY`.

## 2. `W3`: the completion point is the only contractual point

- `CP` is the **only** contractual completion point. There is no dependency, anywhere in the contract, on the host command end or on any equivalent signal.
- **`W3` is everything after `CP`.** It is outside the guarantee.
- `W3a` (effects attributable to this `Commit()` or to the end of this command) and `W3b` (independent later modifications) are kept **only** as a descriptive taxonomy of risk for the Owner. They
  have **no effect** on `SUCCESS`, admission or completion, and nothing is measured to distinguish them.
- **TBD-11 is removed** as a contract parameter. The item `POST_CP_CHARACTERIZATION_REQUIRED_BEFORE_ACT2` remains as an evidence disclosure: before Owner Act 2, `CT-21D` reports what it observes after
  `CP`; the report is not an input to the success contract.
- Product writes after `PC` remain prohibited (V2 section 3.4). Non-DB host-visible actions (regen, redraw) run before `V0`.

## 3. E-10 snapshots, E-11 and the native boundary

### 3.1 E-10 `KNOWN_MODULE_SET_ATTESTATION`, amended

E-10 evaluates the **enumerable known module set** (linker modules, ARX applications, process modules, managed assemblies), each by path and file SHA-256, against the manifest:

| Evaluation point | Content | On failure |
|---|---|---|
| baseline (P0) | two snapshots, equal to each other and to the manifest | refuse (O1) |
| PV | one snapshot equal to the manifest and to the baseline | abort (O2) |
| PC | one snapshot equal to the manifest and to the baseline | mandatory control failure (6.2) |

- Snapshot equality is evaluated on the enumerable set as the contract defines it (TBD-04 fixes sources and completeness limits).
- **A file hash is not proof of the identity of the loaded image** (NG-16). **Snapshot equality does not prove that no other code exists** (NG-17, NG-18).
- E-10 makes no isolation claim.

### 3.2 Continuity set (amends V2 section 15)

`ALT21D_CONTINUITY_PASS(cp)` for `cp` in `{PV, PC}` is: E-01, E-02, E-03, E-04, E-05, E-06, E-08, E-09, **E-10 (snapshot equality per 3.1)**, E-11M, E-12 (per phase, 4) unchanged and PASS.

### 3.3 Native observability (closes TBD-07)

| Aspect | Status |
|---|---|
| native **enumerable snapshots** (linker modules, ARX applications, process modules) | **part of E-10** |
| native **load and unload events** | **no measurement authority**: outside the contract, declared residual `NG-10` |
| managed assembly **load events** | **E-11M** (managed events; the snapshot equality of managed assemblies is carried by E-10) |
| transient loads and unloads between two snapshots, of either kind | not detected: `NG-10` |

`NG-10` is amended to read: "native load and unload event continuity, and transient loads of either kind between snapshots, are not measured; native enumerable snapshots are measured by E-10".
Taking E-11N out of the predicate does not turn the manifest into an isolation proof: the manifest attests the enumerable known set at the evaluation points and nothing else.

## 4. E-12 by phase

`E-12 EventFenceConformance` (class B) is replaced by the following phase contract. It never infers the origin of an event.

### 4.1 Protected objects

`PROTECTED_OBJECTS` is derived deterministically from the sealed `VERIFY_ELEMENT_LIST` (1.3) and comprises: every element of that list; every source object of the read set (including NOD registry objects
the kind consumes); every container or dictionary that is enumerated to check the sealed list (sibling set, definition table entries, extension dictionaries); and any other object whose stability the
comparison needs, as fixed by the kind's authority baseline. The set is hashed together with the sealed list.

### 4.2 Contract by phase

| Phase | Expected event set | Rule |
|---|---|---|
| **0. PREPARE** (PREPARE-R and PREPARE-W, before `T_M` opens) | none adjudicated | events are **recorded** (imports emit host events). E-12 does **not** adjudicate them; the imported definitions are checked by their fingerprints and re-observed at PS. Declared in `NG-19b` |
| **A. MUTATION** (from lock-held `T_M` open to the end of PV evaluation) | `EXPECTED_MUTATION_EVENT_SET`, derived deterministically from the plan; entries `(event kind, target, step)`, each consumable once | an event not deterministically attributable to an entry (unmatched, matchable to more than one entry, or of undeterminable step) is **fail-closed**: **Abort before `Commit()`** (O2 with abort verification). Authorship is never inferred from timing |
| **B. COMMIT BOUNDARY** (from the end of PV evaluation to `V0`) | none adjudicated | events are **recorded**; E-12 does **not** adjudicate them as PASS or FAIL. The host generates its own events here; the correctness of the result is decided by the state comparison. Recorded in the report as diagnostics |
| **C. W-SCAN** (`V0` to `V1`) | `EXPECTED_WRITE_EVENT_SET` over `PROTECTED_OBJECTS` = **EMPTY** (VERIFY is read-only) | any write-class event on a protected object is a **mandatory control failure** (resolved by 6.2) |
| **D. SUBSCRIPTION** | not applicable | failure to subscribe at P0 => **reject before mutation** (O1). Loss of the subscription later => the control is UNKNOWN |
| **E. INFERENCE** | not applicable | absence of events is not a signal; an observed event does not prove origin without an explicit authority; events outside `PROTECTED_OBJECTS` are recorded and not adjudicated |

### 4.3 Declared blind spots

- `NG-19`: a foreign same-context event that coincides with an entry of the expected set is indistinguishable from the product's own and is not detected by E-12; only the state comparison sees its effect.
- `NG-19b`: events in the PREPARE phase (phase 0) and in the commit boundary (phase B) are not adjudicated by E-12; the state comparison and the fingerprints decide.

### 4.4 Assumption to be verified

`A-1`: read-only opens do not emit write-class events on the protected objects. This assumption is what makes "expected write set = empty" a valid rule in phase C. It is an instrument assumption for
`CT-21D` (TBD-06). If it is false for the exact tuple, the phase-C rule needs a reviewed amendment.

### 4.5 Degradation rule

If a future `CT-21D` shows that `EXPECTED_MUTATION_EVENT_SET` cannot be made deterministic, E-12 may degrade to risk telemetry **only** by an explicit amendment reviewed by the Architect, never
silently and never by leaving the control UNKNOWN.

### 4.6 Consequence for class B

E-12 remains class B. Its phase-C failure blocks `SUCCESS` as policy (through O7 or by precedence) but never turns a failing state comparison into PASS.

## 5. Phase model: PREPARE-R, PREPARE-W, MUTATION, VERIFY

```text
PREPARE-R  ->  PREPARE-W  ->  MUTATION  ->  VERIFY
```

Every operation that touches the document runs under the document lock held by the orchestrator (V2 section 8: acquired at PL, kept through `V1`). Modifying a document requires holding its lock, so
imports cannot precede the lock.

### 5.1 PREPARE-R (read and decide)

Allowed: pure computation; library input identity and cache freshness (`LibraryInputRecord`: path, file SHA-256, cache-entry identity); AUTH-12 presence queries; all validations; every rejection
decision. **Every rejection that can be known from PREPARE-R inputs must happen here, before PREPARE-W**, so that it is a clean `O1 REFUSED` with no residue. PREPARE-R ends at `PR` with a decision record and
the `PRE_COMMAND_STATE_RECORD`: the identity set (names or handles plus fingerprints) of every container and element class that PREPARE-W or MUTATION may write. Its per-kind scope is fixed by the kind's
authority baseline (TBD-13).

### 5.2 PREPARE-W (import)

Allowed: only the library imports that PREPARE-R decided are necessary. Rules:

- **immediately before each import**, re-hash the library file and compare with the `LibraryInputRecord`; a mismatch stops the import (outcome per 6.1, O2 with the residue produced so far);
- record the definition set before the first import (part of `PRE_COMMAND_STATE_RECORD`) and a **fingerprint of each imported or relevant definition**;
- at the end emit the `PREPARE_RESIDUE_MANIFEST`: the exact list of what PREPARE-W added or changed, with fingerprints. This manifest defines what may persist even when MUTATION aborts.
- PREPARE-W failures that could not be known from PREPARE-R inputs (an import error, a re-hash mismatch) are allowed after the first import and yield O2 with the residue produced so far.

### 5.3 AUTH-12 transactions

AUTH-12 queries and imports open their own transactions and commit them. Under the caller's document lock this is compatible **if** the expected count of top-level or active transactions is back to the
permitted condition before and after each call. V3 does not name a counting authority: it is TBD-02.

### 5.4 MUTATION and VERIFY

MUTATION is the only phase with the "one caller-owned transaction, one Commit" claim (V2 sections 7.1, 7.3). At PS the PREPARE outputs that influence MUTATION are re-observed (V2 section 7.2). VERIFY is
section 1.

## 6. Outcomes

### 6.1 Outcome table (amends V2 section 10)

| Outcome | Condition | Expected persistent state |
|---|---|---|
| O1 `REFUSED` | a control FAIL/UNKNOWN or a rejection known from PREPARE-R inputs, **before any product write** | pre-command state, no residue |
| O2 `ABORTED_VERIFIED` | failure or drift after `T_M` opened or after the first PREPARE-W write, and before `Commit()`; `Abort()` done if `T_M` opened; and the state read equals `EXPECTED_AFTER_ABORT` (7) | pre-command state + `PREPARE_RESIDUE_MANIFEST` exactly |
| O3 `SUCCESS` | 6.2 conditions | expected persisted state, per 1.1 |
| O4 `FAILURE_COMMITTED_DIVERGENT` | `Commit()` accepted and at least one required observation differs from the expected persisted state | committed; differs from expected |
| O5 `COMMITTED_UNVERIFIED` | `Commit()` accepted; a required **semantic observation** is incomplete or UNKNOWN, or the verifier is defective; no observation shows a difference | committed; relation to expected unknown |
| O6 `ABORT_UNVERIFIED` | abort done or `T_M` failed; the state read cannot confirm `EXPECTED_AFTER_ABORT` (7) | should be pre-command + residue; not confirmed |
| O7 `COMMITTED_MATCH_CONTINUITY_FAILED` | `Commit()` accepted; all required semantic observations complete and equal to expected in both passes; one or more mandatory continuity or risk controls FAILED or were UNKNOWN | committed; equals expected at both observations; continuity not established |

O4, O5, O6 and O7 are **terminal non-success**. None is `SUCCESS`, and none may be reported as a rolled-back state.

### 6.2 Precedence and resolution when several conditions occur

The reported primary outcome is the first that applies in this order:

1. `FAILURE_COMMITTED_DIVERGENT` — any required observation differs from expected;
2. `COMMITTED_UNVERIFIED` — else, any required semantic observation is incomplete or UNKNOWN, or the verifier is defective;
3. `COMMITTED_MATCH_CONTINUITY_FAILED` — else, any mandatory continuity or risk control failed or is UNKNOWN;
4. `SUCCESS` — else.

Every other condition that also occurred is recorded as a **secondary finding** in the report; nothing is dropped and nothing is downgraded. Examples: divergence and a control failure => O4 listing the control failure;
an incomplete read and a control failure => O5 listing the control failure; equal state and a control failure or UNKNOWN control => O7. A control that is UNKNOWN is a control failure for this purpose.
Before any product write, a control failure or UNKNOWN is a refusal (O1). After a product write and before `Commit()`, it aborts (O2, or O6 if the abort cannot be verified). The committed outcomes above apply only after `Commit()` was accepted.

`SUCCESS` (O3) requires, in addition to admission and pre-commit continuity: the sealed list (1.3); pass 1 and pass 2 equal to expected for every element and pass 2 equal to pass 1; read-only verification (1.4);
mandatory continuity controls at `PC` all satisfied, with none UNKNOWN. There is no partial success and no success with a normative warning.

### 6.3 O5 and O6 are disjoint; report and guidance

There is no "unverified family". O5 concerns a **committed MUTATION** whose relation to the expected state could not be established. O6 concerns an **aborted MUTATION** that must not exist, where only the declared
residue may. The guidance below is provisional and is **NOT A GUARANTEED RECOVERY PATH**.

| Outcome | State expectation | What the report says (canonical) | Provisional operator guidance |
|---|---|---|---|
| O4 | MUTATION committed; differs from expected | "Committed. The observed persisted state differs from the expected persisted state for: {elements}." No cause is stated | review the listed elements; revert to the last saved file or try UNDO (not guaranteed) |
| O5 | MUTATION committed; relation unknown | "Committed. Verification could not reach a conclusion: {reasons}. The persisted state is unknown relative to the expected state." | do not assume the result is correct; revert to the last saved file or try UNDO (not guaranteed) |
| O7 | MUTATION committed; equal at both observations; continuity not established | "Committed. The observed persisted state matched the expected state in both observations, but the mandatory control(s) {ids} failed or could not be evaluated. Continuity of the environment is not established." | review before saving; revert to the last saved file or try UNDO (not guaranteed) |
| O6 | MUTATION must be absent; only the residue is allowed | "Aborted. The state afterwards could not be confirmed as the pre-command state plus the declared preparation residue: {reasons}." | verify that no mirrored racks exist; the declared preparation residue may remain; revert to the last saved file if unsure (not guaranteed) |
| O2 | MUTATION absent; residue present | "Aborted. State confirmed as the pre-command state plus the declared preparation residue: {manifest}." | none required beyond the residue notice |

`FAILURE_COMMITTED_DIVERGENT` reports only that the observed persisted state differs from the expected persisted state. It attributes **no** cause: not interference, not corruption, not a comparator defect,
not a product defect, unless separate evidence establishes it. The product performs no compensating write and no automatic save in any outcome.

## 7. Abort verification (S-5)

```text
EXPECTED_AFTER_ABORT = PRE_COMMAND_STATE_RECORD  +  EXACT PREPARE_RESIDUE_MANIFEST
```

After `Abort()` (or after `T_M` fails) the product performs a **fresh read** and compares it with `EXPECTED_AFTER_ABORT`. Any of the following gives **O6 `ABORT_UNVERIFIED`**: residue beyond the manifest; a state
mismatch; an insufficient read; an UNKNOWN. Callbacks are never proof: `cancelled` does not prove the rollback finished; the absence of `modifyUndone`, `modified` or `closed` is not a signal (V2 section 12).
The concrete per-kind reads are fixed in the kind's authority baseline (TBD-13); the rule above is fixed here.

## 8. Product semantics matrix (rebuilt, `TARGET_KIND = SELECTIVE`)

**Marks.** `PRESERVED` = the contractual property has **not been reduced by ALT-21D**. It does **not** mean demonstrated, implemented, host-validated or currently admissible. `REDUCED` = weakened as stated.
`OUTSIDE GUARANTEE` = no claim. `UNSUPPORTED` = refused. `NOT APPLICABLE`.

**Authority status of the target kind.** `TARGET_KIND = SELECTIVE`; `CURRENTLY_ADMISSIBLE_KIND_SCOPE = NONE`. Selective has **no** caller-owned creation seam with rollback authority (B5); every row that depends on it
is `NEEDS NEW AUTHORITY`. HeaderRun and Cantilever appear **only** as the evidence that the AUTH-15 mechanism exists for those two families; they are **not** candidate kinds of ALT-21D and no cell below applies to them.

Columns: **A** written correctly during MUTATION; **B** verified after Commit; **C** remains true through `CP`; **D** after `CP`.

| Property | A | B | C | D | Authority status (Selective) |
|---|---|---|---|---|---|
| One `NewRackId` per logical rack | PRESERVED | REDUCED (two discrete observations) | REDUCED (`W-scan tail`, `W4`, NG-01, NG-06) | OUTSIDE GUARANTEE | NEEDS NEW AUTHORITY (seam; independent host read) |
| Authored payload (schema, `ExtensionData`, bindings, custom properties) | PRESERVED | REDUCED | REDUCED | OUTSIDE GUARANTEE | NEEDS NEW AUTHORITY (seam; sibling capture; comparator is code evidence only) |
| Project variables / read-set | PRESERVED | REDUCED (PV and PC re-observation) | REDUCED | OUTSIDE GUARANTEE | project-variable part: I-49 code evidence; NOD writes NEED NEW AUTHORITY |
| Computed mirror transform (pure) | PRESERVED | NOT APPLICABLE | NOT APPLICABLE | NOT APPLICABLE | NOT IMPLEMENTED; host-independent |
| Persisted placement / transform of views | PRESERVED | REDUCED | REDUCED | OUTSIDE GUARANTEE | NEEDS NEW AUTHORITY |
| No partial placement | PRESERVED within `T_M` | REDUCED | REDUCED (a partial result after Commit is reported, not prevented: O4/O5/O7) | OUTSIDE GUARANTEE | NEEDS NEW AUTHORITY |
| All-or-nothing over `T_M` | PRESERVED for the product's `T_M` writes under a held lock | NOT APPLICABLE | REDUCED after Commit (O4, O5, O7) | NOT APPLICABLE | NEEDS NEW AUTHORITY (seam). PREPARE imports are **outside** the atomic unit; NG-09 |
| Rollback of `T_M` on product failure | PRESERVED for `T_M` writes in the document database under a held lock | REDUCED (abort verification by a fresh read; O6 possible) | REDUCED | NOT APPLICABLE | NEEDS NEW AUTHORITY (seam and host evidence). Side databases and `OpenCloseTransaction` unsupported |
| PREPARE imports and residue | NOT ATOMIC by design: persists after abort | REDUCED (fingerprints; re-observed at PS) | not covered by rollback; exactly the `PREPARE_RESIDUE_MANIFEST` | OUTSIDE GUARANTEE | NEEDS NEW AUTHORITY (residue and UNDO steps) |
| Unsupported-family fail-closed | PRESERVED | PRESERVED | PRESERVED (decided at P0 before any write) | NOT APPLICABLE | NOT IMPLEMENTED; host-independent. Today every kind is refused |
| UNDO of a success | NEEDS NEW AUTHORITY | NEEDS NEW AUTHORITY | NEEDS NEW AUTHORITY | NEEDS NEW AUTHORITY | NEEDS NEW AUTHORITY (B6) |
| SAVE / reopen equality | NEEDS NEW AUTHORITY | NEEDS NEW AUTHORITY | NEEDS NEW AUTHORITY | NEEDS NEW AUTHORITY | physical proof required |
| Multi-document; non-interactive | UNSUPPORTED | UNSUPPORTED | UNSUPPORTED | UNSUPPORTED | outside the envelope |

Consistency: no cell marks `PRESERVED` a property that a non-guarantee weakens before `CP` (all persisted-state properties are `REDUCED` in column C).

## 9. Selective seam sequence

| Gate | What is required |
|---|---|
| before Owner Act 1 | **no implementation.** The seam is shown in the Act 1 summary (12) as an external prerequisite with its cost and risk |
| after Act 1, before the CT-21D AUTHORITY BASELINE | the **authority design** of the caller-owned Selective creation seam (its contract, its rollback statement and scope, its failure typing, its stance on side databases) |
| before CT-21D execution | **implementation and integration** in `src/`, under a separate, separately governed workflow |
| before the replacement Freeze | **host evidence** of the seam's rollback (document-authority evidence of the AUTH-15 kind) |

Nothing is implemented in this gate. The seam is not a `CT-21D` output.

## 10. UNDO sequence

`UNDO = NEEDS NEW AUTHORITY`.

| Gate | What is required |
|---|---|
| before Owner Act 1 | nothing |
| before the CT-21D AUTHORITY BASELINE | the **plan** of what must be demonstrated, in the authority contract |
| during CT-21D | the physical evidence is part of the CT-21D evidence set |
| before the replacement Freeze | that evidence is **reviewed**; the Freeze's recovery wording depends on it |
| therefore before Owner Act 2 | — |

The plan and the evidence must cover: the UNDO steps produced by the **PREPARE residue**; the recoverability of terminal non-success outcomes (O4, O5, O6, O7) where applicable. Nothing is executed now.

## 11. TBD reassignment (supersedes V2 section 15 table)

**Gate names.** `BEFORE_ACT1`; `BEFORE_CT21D_BASELINE` (before the CT-21D AUTHORITY BASELINE); `BEFORE_CT21D_EXECUTION`; `BEFORE_REPLACEMENT_FREEZE`; `BEFORE_ACT2`. They occur in this order. Items whose resolution is CT-21D
evidence are reviewed no later than the replacement Freeze, because Owner Act 2 follows it. The term "frozen CT-21D contract" is not used; the term is **CT-21D AUTHORITY BASELINE**.

| TBD | Subject | Resolution gate |
|---|---|---|
| TBD-01 | E-04 admitted set | criteria in the CT-21D authority contract (`BEFORE_CT21D_BASELINE`); value and evidence `BEFORE_ACT2` |
| TBD-02 | E-06 counting authority | `BEFORE_CT21D_EXECUTION` |
| TBD-03 | `LOCK_MODE` | candidate set at the authority baseline; chosen mode with evidence `BEFORE_ACT2` |
| TBD-04 | E-10 sources | instrument authority `BEFORE_CT21D_EXECUTION`; completeness limits as evidence `BEFORE_ACT2` |
| TBD-05 | E-11M semantics | `BEFORE_CT21D_EXECUTION` |
| TBD-06 | E-12 policy | **phase policy fixed by V3 (4)**; subscription and expected-set instrumentation, and assumption `A-1`, `BEFORE_CT21D_EXECUTION` |
| TBD-07 | managed/native boundary | **`BEFORE_ACT1`: resolved in V3 (3.3)** |
| TBD-08 | Selective seam | design `BEFORE_CT21D_BASELINE`; implementation `BEFORE_CT21D_EXECUTION`; host evidence `BEFORE_REPLACEMENT_FREEZE` (9) |
| TBD-09 | scan adequacy | evidence result `BEFORE_ACT2`; the residual is declared already in Act 1 (1.6, 12) |
| TBD-10 | cache freshness | `BEFORE_CT21D_EXECUTION` |
| TBD-11 | W3a content | **removed as a contract TBD** (2); `POST_CP_CHARACTERIZATION_REQUIRED_BEFORE_ACT2` |
| TBD-12 | interactive-host confirmation | evidence `BEFORE_ACT2` |
| TBD-13 | abort verification read set | **rule `EXPECTED_AFTER_ABORT` fixed by V3 (7)**; concrete per-kind reads at the authority baseline (`BEFORE_CT21D_BASELINE`) |

## 12. Owner sequence naming and the Owner Act 1 summary

Sequence (unchanged in order, renamed):

```text
V3 -> Coordinator review -> Architect delta review -> Coordinator + Architect agreement
-> [Act 1] OWNER SPONSORSHIP / REDECISION (supplements O-1 V18 for that future path)
-> Selective seam authority design -> CT-21D AUTHORITY BASELINE (fixes the parameters of 11)
-> Selective seam implementation and integration (separate workflow) -> separate authorization of CT-21D execution
-> CT-21D execution -> exact evidence review (incl. UNDO, seam host evidence, post-CP characterization)
-> replacement Freeze -> [Act 2] OWNER FINAL ACCEPTANCE -> ADR-0036 amendment -> ADR acceptance (separate Owner act)
-> explicit G3 reopening decision -> G3B -> CT-50 -> implementation gates
```

### 12.1 OWNER ACT 1 SUMMARY (template; NOT PRESENTED; NO OWNER DECISION IS REQUESTED BY THIS DOCUMENT)

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

**Act 1 DOES NOT:** authorize CT-21D execution; reopen G3; change `SafeOperationalState`; make CIA known; issue a replacement Freeze; amend ADR-0036; accept the final ALT-21D guarantee; make Selective admissible;
authorize product implementation except through future, separately governed gates. **Act 1 only** sponsors or redecides the **pursuit** of the reduced-guarantee direction, supplementing the O-1 V18 restriction for that
future path.

## 13. Architect open questions of V2: outcome recorded

| OQ | Ruling recorded in V3 |
|---|---|
| OQ-1 | option B: two-pass model acceptable with the changes of sections 1 and 2; not "stability" |
| OQ-2 | new outcome O7 (6) |
| OQ-3 | E-12 by phase (4) |
| OQ-4 | `CP` sole contractual point; W3 wholly residual (2) |
| OQ-5 | PREPARE-R and PREPARE-W under lock (5) |
| OQ-6 | UNDO sequence (10) |
| OQ-7 | Selective seam sequence (9) |
| OQ-8 | NOT_NEEDED_NOW; the six earlier-build CT-DA passes are never CT-21D evidence and may be cited only as background |

## 14. Blockers B1..B8 (supersede V2 section 0.2)

`CLOSED_BY_V3_CONTRACT` means the **contractual defect is corrected**. It does not mean code, authority or evidence exists, and it does not mean the physical residual is removed.

| Id | Blocker | Status | Note |
|---|---|---|---|
| B1 | RED-3 / W-scan | `CLOSED_BY_V3_CONTRACT` | bounded SUCCESS statement, sealed list, read-only VERIFY, W3 wholly residual. **The physical residual stays** and must be accepted or refused at Act 2; adequacy of the scan is evidence (TBD-09) |
| B2 | guarantee-bearing versus risk controls | `CLOSED_BY_V3_CONTRACT` | O7, E-12 by phase, E-10 snapshots, TBD-07 resolved. Depends on `A-1` and on the degradation rule, both explicit |
| B3 | PREPARE / MUTATION / library | `CLOSED_BY_V3_CONTRACT` | PREPARE-R / PREPARE-W, residue manifest, `EXPECTED_AFTER_ABORT`. Freshness rule is TBD-10 (`BEFORE_CT21D_EXECUTION`) |
| B4 | lock / transaction ownership | `CLOSED_BY_V2_CONTRACT` | unchanged |
| B5 | Selective caller-owned rollback authority | `REMAINS_BLOCKER_FOR_CT21D_DESIGN` | sequence in 9; not closed |
| B6 | UNDO authority | `REMAINS_BLOCKER_FOR_ACT2` | sequence in 10; not closed |
| B7 | PRESERVED versus non-guarantees | `CLOSED_BY_V3_CONTRACT` | marks defined; matrix rebuilt for Selective; HeaderRun and Cantilever removed as candidates |
| B8 | Owner / ADR sequence | `CLOSED_BY_V2_CONTRACT` | unchanged; naming aligned |

## 15. Remaining contract ambiguities (closed list)

None of these prevents the four `CLOSED_BY_V3_CONTRACT` classifications; each is stated so the Architect can rule on it.

| Id | Ambiguity | Why it is left, and where it is resolved |
|---|---|---|
| RA-1 | assumption `A-1` (read-only opens emit no write-class events) is unverified; the phase-C rule of E-12 depends on it | instrument assumption; `TBD-06`, `BEFORE_CT21D_EXECUTION`; a false result needs a reviewed amendment |
| RA-2 | the write-class event list and the subscribed event set are not enumerated | instrumentation; `TBD-06` |
| RA-3 | the content of `VERIFY_ELEMENT_LIST`, of `PROTECTED_OBJECTS` and of `PRE_COMMAND_STATE_RECORD` is fixed by rule but not enumerated for Selective | depends on the seam authority design (TBD-08) and the authority baseline (TBD-13) |
| RA-4 | whether phase A (MUTATION) can be made deterministic for the exact tuple | evidence; the degradation rule (4.5) covers a negative result |
| RA-5 | the precise instant at which the commit boundary starts (end of PV evaluation versus the call to `Commit()`) is stated as the end of PV evaluation; the host may emit events earlier | to be confirmed by `CT-21D`; events before it are adjudicated by phase A |
| RA-6 | localization of the canonical SUCCESS statement: English governs, a rendering must keep E1-E4 | conformance is checked by review, not by tooling |

```text
PROPOSAL ALT-21D V3 = PUBLISHED / COORDINATOR REVIEW + ARCHITECT DELTA REVIEW REQUIRED
V1, V2 = HISTORICAL / NOT EDITED
B1 B2 B3 B7 = CLOSED_BY_V3_CONTRACT   B4 B8 = CLOSED_BY_V2_CONTRACT   B5 = REMAINS_BLOCKER_FOR_CT21D_DESIGN   B6 = REMAINS_BLOCKER_FOR_ACT2
ALT21D_DIRECTION = PLAUSIBLE_NOT_YET_VIABLE      ALT21D_ADMISSION_PASS = NOT_YET_SATISFIABLE      CURRENTLY_ADMISSIBLE_KIND_SCOPE = NONE
OWNER_DECISION_STATUS = NOT_YET                  CT21D_STATUS = NOT_AUTHORIZED
CTDA_HOST_PASS = FALSE   CIA = UNKNOWN   SafeOperationalState = FALSE_FOR_ADMISSION   G3 = STOPPED   ALT-21E = FALLBACK_IN_EFFECT
SUBSTANTIVE IMPLEMENTATION = BLOCKED   BRANCH = NOT RECONCILED (79 behind / 120 ahead before this commit)   HOST VALIDATION = NOT RUN
NEXT GATE = ARCHITECT DELTA REVIEW OF ALT-21D PROPOSAL V3
```
