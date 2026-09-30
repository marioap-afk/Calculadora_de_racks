# I-52 — CT-21D AUTHORITY CONTRACT V2.1 (DRAFT)

> **CT-21D AUTHORITY CONTRACT V2.1 — DRAFT FOR COORDINATOR REVIEW; THEN ARCHITECT ITEM-BY-ITEM CHECK OF RC-1..RC-14. Design / documentation only. Nothing here is executed, built or authorized.**
>
> ```text
> OWNER_ACT1                        = SPONSORED  (OWNER_ACT1_DECISION = A; SPONSOR_ALT21D = YES)
> ALT21D_DIRECTION                  = OWNER_SPONSORED
> OWNER_DECISION_STATUS             = SPONSORED  (sponsorship of the pursuit ONLY; the final guarantee is NOT accepted)
> CT21D_AUTHORITY_CONTRACT          = DRAFT_V2_1
> CT21D_AUTHORITY_BASELINE_READY    = FALSE
> CT21D_EXECUTION_READY             = FALSE
> CT21D_EXECUTION                   = NOT_AUTHORIZED
> CT21D_STATUS                      = READY_FOR_AUTHORITY_CONTRACT_DESIGN_AFTER_OWNER_ACT1
> ALT21D_ADMISSION_PASS             = NOT_YET_SATISFIABLE
> CURRENTLY_ADMISSIBLE_KIND_SCOPE   = NONE
> CTDA_HOST_PASS(V35-A3)            = FALSE (terminal, monotone)
> ContextIsolationAuthority         = UNKNOWN          SafeOperationalState = FALSE_FOR_ADMISSION
> G3 = STOPPED   G3B = NOT OPEN   CT-50 = NOT EXECUTED   ALT-21C = NOT ADMISSIBLE   ALT-21E = FALLBACK_IN_EFFECT
> Freeze V18 = GOVERNING   Freeze V35 = GOVERNING ITS OWN PREDICATE   Replacement Freeze = DOES NOT EXIST   ADR-0036 = PROPOSED / AMENDED FOR V18
> UNDO_EVIDENCE_STATUS              = NOT_EXECUTED
> B5 = ADVANCES_TO_IMPLEMENTATION_PREREQUISITE  (+ REMAINS_BLOCKER_FOR_CT21D_EXECUTION + REMAINS_BLOCKER_FOR_REPLACEMENT_FREEZE)
> B6 = REMAINS_BLOCKER_FOR_ACT2
> NB-1 NB-2 NB-4 = REMAINS_BLOCKER_FOR_AUTHORITY_BASELINE      NB-1 NB-3 NB-6 = ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION (still blockers until artifact, hash and agreement)
> NB-5 = REMAINS_BLOCKER_FOR_CT21D_EXECUTION
> ```
>
> The Owner authorized **only** the pursuit of the reduced-guarantee direction. The Owner did **not** accept the final guarantee, authorize CT-21D execution, AutoCAD, RACKMIRROR implementation, reopen G3, change CIA or
> `SafeOperationalState`, issue a Freeze, or modify ADR-0036. This contract executes nothing, builds no harness, changes no code and is **not** an authority baseline.

## 0. Purpose, authority and reading rules

**Purpose.** To turn the parameters and pending requirements left open by Proposal ALT-21D V5 into a precise, finite **authority contract** that a future `CT-21D` characterization would obey. It defines *what must be measured, how,
with which instrument, under which identity, and how each result is classified*. It does not run anything.

**Authority of the guarantee.** Proposal ALT-21D **V5** (`bb8f230c02dab8abb34db326d9a7d7663f23c212`, blob `f73157d9ce2d5013f4f91fb2d4f4d127a4f879a1`), with V1-V4 as history. This contract **does not reinterpret** SUCCESS, `W-scan`, `W3`, the outcomes O1..O7, E-10/E-11/E-12, PREPARE-R/PREPARE-W,
ABORT-VERIFY, the Owner Act 1 disclosures or the blocker classifications. Where this contract needs a value that V5 leaves open, the parameter is named `TBD-nn` as in V5 section 11 (V3) and resolved here or explicitly left with its gate.

**Candidate host APIs.** Every host API, event or system variable named in this document is a **CANDIDATE instrument**: its existence, semantics and reliability in the exact interactive tuple are themselves characterization targets. Nothing is asserted as available.

**Parameters.** Every open numeric or policy parameter is classified in section 21.7 (`MUST_FIX_BEFORE_AUTHORITY_BASELINE`, `MAY_BE_OPERATIONAL_AT_EXECUTION` or `SHOULD_BE_REMOVED`). No value is chosen here.

**Repository facts used** (verified on `main` `3375aadb`): product code has no environment observers (no active-transaction count, no system-variable reads, no long-transaction reader, no document-count reader, no load or database event subscriptions);
the AUTH-15 seam covers HeaderRun and Cantilever only; AUTH-12 queries open their own transactions and the external one reads the process-cached library database. The G3A probe exists only as a development tool on the I-52 branch.

### 0.1 What V2.1 is, and its disposition of the Architect's required changes RC-1..RC-14

V2.1 is a **minimal revision of V2** (`d3a2e2a8f99c51e7524301ed6f8ff1fa51fba621`, blob `9254b1a2f8b8164184386253e9923636b19862e8`). The Architect's delta review of V2 (`ARCHITECT_V2_DELTA_RULING = AGREED_WITH_REQUIRED_CHANGES`,
`CONTRACT_ASSESSMENT = READY_AFTER_DOCUMENT_CHANGES`) required exactly **RC-1..RC-14**. V2.1 incorporates those and nothing else; it does not reopen the accepted architecture. **V1 (`1613900118547313deb49bf60ad1126939741261`) and V2 are intact and
historical.** Sections that RC-1..RC-14 did not touch are carried over verbatim from V2.

| Required change | Handled in |
|---|---|
| RC-1 control class registry and coverage matrix | 30, 31; 21.4; BASE-19, BASE-20 (22) |
| RC-2 predicate binding and terminal taxonomy | 1.3 to 1.7 |
| RC-3 warm-up vehicle and coverage criterion | 13.1 |
| RC-4 static dependency closure, reuse and name collision | 6, 7.1, 11.1, 11.3 |
| RC-5 private-copy authority for the import | 19.1, 20 |
| RC-6 X7 and the V1 to CP tail log | 5, 10.6, 10.8 |
| RC-7 two-layer fingerprint | 28 |
| RC-8 E-06 typed authority, `E6-C8` | 4.2, 15.3, 19.3 |
| RC-9 qualification and conformance controls | 4.3.1 |
| RC-10 scenario catalog schema | 26 |
| RC-11 event-model artifact, corpus and adjudication domain | 14.8, 27 |
| RC-12 manifest custody anchor and roles | 29 |
| RC-13 NB-1 authority comparison | 25 |
| RC-14 minor corrections | 12.3, 14.7, 16, 19.1/20, 21.7, 22, 23 |

### 0.2 Carry-over from V2: location of the SC, ID and WD changes

| Change | Handled in |
|---|---|
| SC-1 four-level result model | 1 |
| SC-2 `CHARACTERIZATION_RUN` versus `ADMISSION_EVALUATION` | 3.2 |
| SC-3 SL-3 has its own Selective contract | 15.2, 15.9 |
| SC-4 OD-1 closure (sealed Selective view-family scope) | 25 |
| SC-5 E-12 three-level authority | 14.3, 27 |
| SC-6 INVALID evidence retention | 17, 21.1 |
| SC-7 attempt and retry log | 21.3 |
| SC-8 `MODIFY_PREEXISTING`, reuse, closure, cloning mode | 7.1, 11.3 |
| SC-9 abort read-set refinement | 11.1 |
| SC-10 `FRESH_ACQUIRE` same-content authority | 19.1 |
| SC-11 warm-up order | 13.1, 5 |
| SC-12 expanded baseline checklist | 22 |
| ID-1 typed transaction authority | 4.2 |
| ID-2 instrument qualification | 4.3 |
| ID-3 E-10 (`MVID_CONSISTENCY`, warm-up, no time-based UNKNOWN) | 12 |
| ID-4 `FINGERPRINT SPECIFICATION` | 28 |
| ID-5 scan characterization | 10.6 |
| ID-6 `U-6b` and the indeterminate-`Commit()` disclosure | 16, 24 |
| WD-1 evaluator row 2 | 17.2 |
| WD-2 Act 2 disclosure | 24 |
| WD-3 draft parameters | 21.7 |
| WD-4 baseline-ready meaning | 22 |
| WD-5 execution checklist | 23 |
| WD-6 rename of the mode | 3.2 and throughout |
| NB-2 scenario catalog schema | 26 |
| NB-4 event-model procedure and learning corpus | 27 |
| NB-6 manifest capture and custody | 29 |

## 1. CONTRACT IDENTITY

| Field | Value |
|---|---|
| Contract id | **`CT-21D-V2.1`** |
| Authority text | this document, revision **`DRAFT_V2_1`**; it supersedes the drafts `CT-21D-V1` (`1613900118547313deb49bf60ad1126939741261`) and `CT-21D-V2` (`d3a2e2a8f99c51e7524301ed6f8ff1fa51fba621`), which were never baselined and remain intact as history |
| Predicate | **`ALT21D_HOST_PASS(CT-21D-V2.1, BASELINE_TUPLE)`**: bound to the contract version **and** the baseline tuple (1.7) |
| Authority of the guarantee | Proposal ALT-21D V5 (`bb8f230c...`) |
| Owner authority | Owner Act 1 = A: sponsorship of the pursuit only |
| Contract hash | `TO_BE_RECORDED_AT_BASELINE` (SHA-256 over the LF-normalized text of this document plus the V5 blob id) |
| Baseline | `CT21D_AUTHORITY_BASELINE_READY = FALSE` |

```text
ALT21D_HOST_PASS(CT-21D-V2.1, BASELINE_TUPLE)  !=  CTDA_HOST_PASS(V35-A3)
```

- **No automatic transfer of results.** No CT-DA row, no CT-DA PASS, no `CTDA_HOST_PASS` state and no `04NO-S` UNKNOWN is a CT-21D result or counts toward one. CT-DA research tooling (`eng/research/I52Ctda`) and the G3A probe are **reference material only**; using any of it is a decision of a future gate.
- **`CTDA_HOST_PASS` is not revived and stays `FALSE` (terminal).**
- CT-21D is a **characterization** of measurability and behaviour; it is **not** a product admission and never admits a product run.

### 1.1 The four levels (SC-1)

| Level | What it is | Produced by | It is never |
|---|---|---|---|
| **A. PRODUCT OUTCOME** | O1..O7 of one run (V3 6, V4 1.3, V5 3), computed by the outcome evaluator (section 17) | the offline evaluator, from the raw log | a verdict about the contract |
| **B. CONTROL RESULT** | per control and per run: `PASS`, `FAIL` or `UNKNOWN`; plus, per instrument and per run, the **instrument state** | the instruments (section 4) | a verdict about the contract |
| **C. GROUP VERDICT** | per group, over its governing runs: compares the observed product outcome, the control results and the **scenario-declared expected result** (section 26) | the `ADMISSION_EVALUATION` (section 3.2) | derived from a single outcome without the scenario's expectation |
| **D. `ALT21D_HOST_PASS` STATE** | derived **only** from governing group verdicts, through the **class** of each control (`CONTROL_CLASS_REGISTRY`, section 30) and the **cause** of each non-PASS verdict (1.3, 1.4) | the `ADMISSION_EVALUATION` | derived directly from any O1..O7 outcome |

**`ALT21D_HOST_PASS` is never derived directly from an O1..O7 outcome.** A deliberate negative-control scenario may produce O7 and still have a `PASS` group verdict, because O7 was exactly the declared expected result.

### 1.2 Control result and instrument state (Level B)

- **Control result:** `PASS`, `FAIL`, `UNKNOWN` (a reading that could not be established is `UNKNOWN`, never `PASS`).
- **Instrument state**, per instrument, per fact and per tuple: `RELIABLE` (qualified on the exact build, section 4.3, and its reliability controls passed in the run; the qualification record hash equals the tuple's); `UNRELIABLE` (qualification or a reliability control failed; every control result read through it is `UNKNOWN`);
  `NOT_MEASURABLE` (a **qualified** instrument shows that the host offers no way to read the fact).
- **Instrument defect versus host limitation.** An implementation defect of the instrument makes it `UNRELIABLE`; it is **not** a host `NOT_MEASURABLE`.

### 1.3 Cause taxonomy and group verdict (Level C) — RC-2

Every non-`PASS` governing result has exactly one **cause**. The four causes are the only ones this contract recognizes.

| Cause | What it is | Group verdict it produces |
|---|---|---|
| `HOST_LIMITATION` | a **qualified** instrument shows that, in the exact tuple, the host offers no way to read or produce a fact a mandatory control requires (an API, event, variable or category does not exist or is not readable) | `NOT_MEASURABLE` |
| `INSTRUMENT_DEFECT` | the instrument, its qualification, the control plane or the evaluator is defective; **nothing is concluded about the host or the product** | none: the instrument state is `UNRELIABLE`; every result read through it is `UNKNOWN` |
| `EMPIRICAL_BEHAVIOR_DIFFERENCE` | with reliable instruments, the host behaves differently from a **premise** of the design (for example A-1 or A-2 false, a companion event set that is not deterministic, an assumed event category absent) | `CHARACTERIZED_DIFFERENT` |
| `PRODUCT_OR_SEAM_DEFECT` | with reliable instruments, the **build under test** (seam, orchestrator, verifier, importer, product code) contradicts its own **designed behaviour**, for example a mandatory control fails in clean governing runs because of the build, or a designed detection is missed | `FAIL` |

A **clean governing run** is a governing run of a scenario that contains no deliberate violation. The complete list of group verdicts is:

| Verdict | Meaning |
|---|---|
| `PASS` | in every required governing run, the observed outcome and control results equal the scenario-declared expected result |
| `FAIL` | cause `PRODUCT_OR_SEAM_DEFECT` |
| `CHARACTERIZED_DIFFERENT` | cause `EMPIRICAL_BEHAVIOR_DIFFERENCE` |
| `NOT_MEASURABLE` | cause `HOST_LIMITATION` |
| `UNKNOWN` | a reading required for the verdict is missing or inconclusive; **not yet classified** (1.5) |
| `INVALID` | there is no governing run |

Every non-`PASS` governing verdict is recorded with its cause. A cause may be corrected only by a `CLASSIFICATION_RULING` that shows the instrument or the scenario was wrong; a ruling **cannot** turn a `FALSE` into a milder state on the same evidence unless it re-adjudicates the underlying run as `INVALID` or the instrument as `UNRELIABLE`.

### 1.4 Mapping to the predicate state (Level D)

The **class** of a control comes only from the `CONTROL_CLASS_REGISTRY` (section 30); a group inherits the class of the controls it evaluates, and mixed groups are split so that each group has one class. **No class is inferred elsewhere.**

| Cause / situation | Class A control | Class B control |
|---|---|---|
| `PASS` | satisfied | satisfied |
| `HOST_LIMITATION` (`NOT_MEASURABLE`) | **`FALSE`** for the contract version and the baseline tuple. **Reopening requires a new reviewed contract version.** | `AMENDMENT_PENDING` |
| `EMPIRICAL_BEHAVIOR_DIFFERENCE` (`CHARACTERIZED_DIFFERENT`) | **`FALSE`**. **A new reviewed contract version is required to change the premise.** | `AMENDMENT_PENDING` |
| `PRODUCT_OR_SEAM_DEFECT` (`FAIL`) | **`FALSE` for the affected build tuple.** A **corrected build is a NEW tuple** (1.7) and may be evaluated under authorized future work; no evidence transfers | `AMENDMENT_PENDING` (for the affected build tuple) |
| `INSTRUMENT_DEFECT` (`UNRELIABLE`) | **`NOT_EVALUATED`**: repair and requalification; **no host verdict** | `NOT_EVALUATED` |
| `UNKNOWN` (not yet classified) | `NOT_EVALUATED` until a `CLASSIFICATION_RULING` (1.5) | `NOT_EVALUATED` until a `CLASSIFICATION_RULING` (1.5) |
| `INVALID` | `NOT_EVALUATED` | `NOT_EVALUATED` |
| control class `UNRESOLVED` (registry) | `NOT_EVALUATED`: no mapping applies until the class is resolved; the evidence is retained | same |

### 1.5 Classification of a governing UNKNOWN

A governing `UNKNOWN` that does not come from an `UNRELIABLE` instrument is **classified by a `CLASSIFICATION_RULING`** (Coordinator and Architect). The ruling **chooses exactly one of the four causes** of 1.3 and states the evidence; the corresponding row of 1.4 then applies.

- If no cause fits, the state stays `NOT_EVALUATED` and the item is escalated as a **contract amendment** (a new reviewed version); there is no "other" cause.
- For `INSTRUMENT_DEFECT`, the measurement and the run are invalid **for that instrument**; the instrument must be corrected and qualified again; **a rerun needs a new authorization**; it is not a host limitation.
- An unclassified `UNKNOWN` cannot become `TRUE`.

### 1.6 The states of `ALT21D_HOST_PASS`

| State | Condition |
|---|---|
| `FALSE` | any class-A row of 1.4 that gives `FALSE`, or a determination under exit B below. **Terminal and monotone for the bound instance** (1.7) |
| `AMENDMENT_PENDING` | no `FALSE` condition, and at least one class-B row of 1.4 that gives `AMENDMENT_PENDING` is unresolved |
| `NOT_EVALUATED` | no `FALSE`, no `AMENDMENT_PENDING`, and at least one mandatory control or required group is not `PASS` (unclassified `UNKNOWN`, `INVALID`, instrument defect, class `UNRESOLVED`, or not yet run) |
| `TRUE` | **all** of: no `FALSE`; no `AMENDMENT_PENDING`; **no mandatory control `NOT_EVALUATED`**; **no unresolved mandatory classification** (no `UNKNOWN` without ruling, no control of class `UNRESOLVED`); every mandatory class-A control satisfied; every mandatory class-B control satisfied **under the currently effective contract and amendments**; every required group has a governing `PASS`; and the control-to-group-to-scenario coverage matrix (section 31) is complete |

Precedence: `FALSE` > `AMENDMENT_PENDING` > `NOT_EVALUATED` > `TRUE`. **`AMENDMENT_PENDING` and `TRUE` are mutually exclusive.**

**`AMENDMENT_PENDING` exits only through:**
- **A.** an amendment reviewed by the Architect, approved and applied, **plus** the recharacterization it requires; or
- **B.** a **recorded determination that no acceptable amendment exists**, which makes the state `FALSE`.

There is no other exit, and a mandatory class-B failure never produces `TRUE`. `TRUE` means that the characterization is complete under the effective contract for the bound instance; it does **not** admit a product run and does not reopen G3.

### 1.7 Predicate binding

`ALT21D_HOST_PASS` is **not an abstract, eternal predicate**. An **instance** is the pair `(CONTRACT_VERSION, BASELINE_TUPLE)`:

- the state is evaluated **per instance**, only from the governing evidence recorded under that pair;
- `FALSE` is terminal and monotone **for that instance**: no later evidence moves it;
- a change of the **build-bound** part of the tuple (a corrected build) creates a **new instance** that starts as `NOT_EVALUATED`; **no evidence transfers** from the previous instance;
- a change of the **contract version** creates a new instance, and is the only way to reopen a `FALSE` caused by `HOST_LIMITATION` or `EMPIRICAL_BEHAVIOR_DIFFERENCE`;
- a `FALSE` caused by `PRODUCT_OR_SEAM_DEFECT` does not by itself require a new contract version; it requires a new build tuple and authorized future work.

## 2. BUILD / HOST / SESSION TUPLE

A run is **governing** only if its tuple equals the baseline tuple in every BUILD_BOUND and MACHINE_PROFILE_BOUND field and every SESSION_BOUND field is present and consistent. No concrete hash is fixed here.

### 2.1 Fields

| Field | Class | Recorded how |
|---|---|---|
| repo source SHA of the build | BUILD_BOUND | build receipt |
| Plugin DLL SHA-256 | BUILD_BOUND | hash of the file loaded |
| any payload / observer / harness DLL SHA-256 | BUILD_BOUND | hash of each file |
| AutoCAD version and build; `acad.exe` SHA-256; API assembly versions and SHA-256 | BUILD_BOUND | host identity instrument (I-02) |
| authority-contract version and hash | BUILD_BOUND | this document |
| manifest version and hash (build layer) | BUILD_BOUND | manifest file |
| Selective kind-authority version and hash (when it exists) | BUILD_BOUND | kind authority baseline |
| block-library file path and SHA-256 | BUILD_BOUND | hash of the file |
| scenario catalog version and hash | BUILD_BOUND | catalog file |
| fingerprint-specification version and hash (section 28) | BUILD_BOUND | specification file |
| event-model version and hash (`EVM-Selective-vX`, section 27) | BUILD_BOUND | model file |
| sealed Selective view-family scope version and hash (section 25) | BUILD_BOUND | scope file |
| manifest hash and custody-log reference (section 29) | BUILD_BOUND | custody log |
| instrument qualification records for the exact build (section 4.3) | BUILD_BOUND | qualification evidence |
| `CONTROL_CLASS_REGISTRY` and coverage-matrix version and hash (sections 30, 31) | BUILD_BOUND | registry and matrix files |
| warm-up definition and `WARMUP_COVERAGE_CRITERION` version and hash (13.1) | BUILD_BOUND | warm-up definition file |
| OS version and build (only where relevant to the enumerations) | MACHINE_PROFILE_BOUND | OS query |
| machine identity class | MACHINE_PROFILE_BOUND | recorded label agreed at baseline; the machine name is kept in local evidence only |
| AutoCAD profile identity; `SECURELOAD` and `TRUSTEDPATHS` values | MACHINE_PROFILE_BOUND | profile and system-variable reads |
| the machine's own module entries (drivers, security software) | MACHINE_PROFILE_BOUND | manifest machine layer |
| process id (PID) and process start time | SESSION_BOUND | process query |
| document identity; database identity | SESSION_BOUND | document/database readers |
| drawing identity and hash **before** the run (fixture copy) and after | SESSION_BOUND | file hash of the scratch copy |
| snapshots and checkpoints of the run | SESSION_BOUND | the run log |
| `LibraryInputRecord` (source path and stat, the SHA-256 of the copied bytes, the per-run authoritative private library copy path, generation id) | SESSION_BOUND | PREPARE-R |

### 2.2 Mismatch rules

- Any BUILD_BOUND or MACHINE_PROFILE_BOUND mismatch against the baseline: the run is **INVALID** (section 21.1).
- A SESSION_BOUND field missing or inconsistent (PID changed mid-run, drawing identity changed, snapshot not attributable to the run): **INVALID**.
- A manifest captured in one session is never replayed as evidence for another (Proposal V2 section 14).

### 2.3 Interactive host is required

A governing run is executed in **interactive `acad.exe`**. Anything taken in Core Console (including G3A) is a hypothesis, never CT-21D evidence.

### 2.4 Interactive-host confirmation (TBD-12)

The future evidence must show, in interactive `acad.exe` and for the exact tuple:

| Item | To be shown |
|---|---|
| E-01 | host identity properties and file hashes read inside the interactive process, equal to the manifest tuple |
| E-02 | document count, active document and database correspondence; behaviour while a document is opening or closing |
| E-04 | `CMDNAMES` and `CMDACTIVE` (and registration flags) at P0, PL, PV, PC for the designated invocation route |
| E-05 | the four persistent-mode indicators and the long-transaction reader, in their inactive and active states (positive and negative controls) |
| E-09 | `HasOverrule` behaviour for the subjects and classes consumed |
| E-13 | `SECURELOAD` and `TRUSTEDPATHS` readable and equal to the manifest |
| all | none of the above may be inferred from a Core Console or CT-DA observation |

## 3. ADMISSION INPUTS

### 3.1 The inputs

| Input | Content | Bound |
|---|---|---|
| AI-1 tuple | section 2 | build / machine / session |
| AI-2 manifest | three layers (Proposal V2 section 14): known module set (E-10), managed set (E-11M), profile values (E-13) | build / machine / session |
| AI-3 kind registry and per-kind evidence record | E-14 | build |
| AI-4 library input | source path and stat, per-run authoritative private library copy path and SHA-256, generation id | build / session |
| AI-5 contract binding | E-15 | build |
| AI-6 scenario | the scenario id and plan input (selection, axis, options) from the scenario catalog | build |
| AI-7 fixture drawing | a fresh byte copy of a designated scratch template, hash recorded | session |

### 3.2 `CHARACTERIZATION_RUN` and `ADMISSION_EVALUATION` (SC-2, WD-6)

`E-15` requires a CT-21D record and an Owner acceptance, and `E-14` requires seam host evidence. A CT-21D run produces that evidence, so it cannot require it. The two operations are therefore **formally separate**. The single combined label used in the earlier draft is **withdrawn** and replaced by these two.

| | `CHARACTERIZATION_RUN` | `ADMISSION_EVALUATION` |
|---|---|---|
| what it is | a governed execution of **one catalog scenario in one AutoCAD process** | a separate operation over the **completed governing evidence package** |
| controls | E-01..E-13 are evaluated and logged as **control results** and stay **fail-closed** (a failure aborts or refuses as in product mode), **except** an explicitly declared deliberate violation in a negative-control scenario | applies the four-level result model (section 1) |
| E-14 and E-15 | recorded as **`UNDER_CHARACTERIZATION`**; E-14 only for Selective and only if the kind authority baseline exists and the seam implementation is integrated; E-15 records the manifest binding to this contract and marks the CT-21D record and the Owner acceptance as `NOT_YET_EXISTING (EXPECTED)` | evaluates the group verdicts |
| emits | run log, product outcome, control results, evidence package | the group verdicts and the `ALT21D_HOST_PASS` state |
| **never** | emits `ADMISSION_PASS`; admits a product run; changes `CURRENTLY_ADMISSIBLE_KIND_SCOPE`; reopens G3 | reopens G3; issues a Freeze; admits a product run; accepts the guarantee |

- The token **`ADMISSION_PASS`** exists only in product mode, after `E-15` is satisfied by a future replacement Freeze and Owner acceptance. **No single run may bypass the separation**: a run has no way to produce a predicate state, and the evaluation has no way to run a product.
- The evidence of a `CHARACTERIZATION_RUN` is used to determine what authority, amendments and values a future product admission would need.

### 3.3 E-04 admitted set (TBD-01): the contract of the set

Criteria are fixed here. **The accepted value is evidence-derived** and fixed only before Owner Act 2.

**Dimensions of the admitted-set tuple**

| Dim | Content |
|---|---|
| D-1 | `CMDNAMES` value at each checkpoint (string, exact) |
| D-2 | `CMDACTIVE` value at each checkpoint (bit pattern, exact) |
| D-3 | registration flags of the RackCad command (transparent, session, modal, others the host reports) |
| D-4 | invocation route (typed at the command line; menu or ribbon; script; LISP; `SendStringToExecute`; API call), recorded, not inferred |
| D-5 | nesting depth of commands as the host reports it |
| D-6 | invoking document is the active document |

**Rules**

- A value enters the admitted set **only if** it was observed at the checkpoint in **every** governing run of the **designated invocation route(s)** and in no governing run that violates the envelope.
- A value observed only under another invocation route, or under a transparent, nested, scripted or application-context-queued invocation, is **excluded** and is a FAIL when seen in a product run.
- The set is defined **per checkpoint** (P0, PL, PV, PC may legitimately differ).
- Designated routes are fixed at the baseline (candidate: the interactive typed command); routes not designated are `UNSUPPORTED`.
- Verdicts: **PASS** = the observed tuple equals a member of the admitted set for that checkpoint; **FAIL** = readable and not in the set; **UNKNOWN** = any dimension unreadable, inconsistent between two reads, or not recordable.
- Evidence-derived part: the concrete values of D-1, D-2, D-3, D-5; criteria part: everything above.

### 3.4 LOCK_MODE (TBD-03): the contract of the choice

`LOCK_MODE = TO_BE_FIXED_BY_CT21D_EVIDENCE` and **no mode is selected here.**

| Candidate | Description |
|---|---|
| LM-1 | the product convention: parameterless `LockDocument()` (about thirty product sites; the AUTH-15 evidence records "a held `LockDocument`" without a stronger mode) |
| LM-2 | explicit exclusive write mode (`DocumentLockMode.ExclusiveWrite`) |
| LM-3 | other documented modes, only if a candidate is added by a reviewed baseline amendment |

Properties the chosen mode must **demonstrate**, each with its measurement:

| Prop | Property | Measured by |
|---|---|---|
| P-L1 | acquisition succeeds inside the RackCad command in the interactive host | acquire at PL; record success |
| P-L2 | the held mode reads back as the chosen mode | lock-state reader (I-06) at PL, PV, PC |
| P-L3 | the lock is held continuously from PL to V1 (never released and re-acquired) | reads at every checkpoint plus an acquisition counter |
| P-L4 | compatible with the seams, with AUTH-12 callee transactions and with library imports (no lock violation) | scenario runs of PREPARE-R, PREPARE-W and MUTATION |
| P-L5 | another execution context cannot write while the lock is held; the conflict is observable | adversarial writer probe from another context (I-14) |
| P-L6 | what a conflicting context observes (error type) | recorded |
| P-L7 | the mode does not alter host behaviour that other controls depend on (E-06 counts, E-12 events) | comparison of E-06 and E-12 logs across candidate modes |

**Selection rule (applied by review, never by the implementer):** among candidates that demonstrate P-L1..P-L4 and P-L7, choose the one whose documented contract excludes the broader set of other contexts and whose P-L5 result is confirmed. **UNKNOWN** when any property is unmeasurable or results differ across runs. If no candidate demonstrates P-L1..P-L4, E-03 is not satisfiable: the group verdict is `FAIL` for a class-A control (predicate `FALSE`).

## 4. REQUIRED INSTRUMENTS

### 4.1 Instrument list

| Id | Instrument | Existence today | Notes |
|---|---|---|---|
| I-01 | monotonic clock and per-run sequence counter | DOES NOT EXIST as a product service | ordering of every log record is by sequence, not by wall clock |
| I-02 | host identity reader | dev tool (G3A probe) | Core Console evidence only |
| I-03 | document collection, active document, database correspondence reader | active document only (product) | count and opening-state readers missing |
| I-04 | system-variable reader (`CMDNAMES`, `CMDACTIVE`, `REFEDITNAME`, `BLOCKEDITOR`, `ARRAYEDITSTATE`, `BLOCKTESTWINDOW`, `SECURELOAD`, `TRUSTEDPATHS`) | dev tool | DOES NOT EXIST in product |
| I-05 | long-transaction reader | dev tool | DOES NOT EXIST in product |
| I-06 | lock-state reader (held mode) | DOES NOT EXIST | candidate host reads to be confirmed |
| I-07 | active-transaction counter (E-06) | DOES NOT EXIST | section 4.2 |
| I-08 | module enumerators and file hasher (E-10) | DOES NOT EXIST in product | section 12 |
| I-09 | managed load-event subscriber (E-11M) | DOES NOT EXIST | section 13 |
| I-10 | database, document and transaction event subscriber (E-12) | DOES NOT EXIST | section 14 |
| I-11 | read-only verifier abstraction, independent comparator, expected-state oracle | comparators exist as code evidence (I-58); the read-only abstraction and the host-side reader for Selective do not | sections 10, 15 |
| I-12 | fingerprint provider | DOES NOT EXIST | section 28 |
| I-13 | library identity and cache probe | DOES NOT EXIST | section 19.1 |
| I-14 | adversarial writer probes (characterization only) | DOES NOT EXIST | sections 3.4, 10.6 |
| I-15 | result and log writer | DOES NOT EXIST | section 18 |
| I-16 | control plane: launcher, process supervisor, offline outcome evaluator, evidence sealer | DOES NOT EXIST for CT-21D | evaluation happens **outside** the AutoCAD process from the raw log |

Each instrument must pass its qualification (section 4.3) on the exact build before it can support a governing verdict. Every "DOES NOT EXIST" is an **implementation / instrumentation requirement** listed in section 23; none is claimed as existing.

### 4.2 E-06 transaction-counting authority (TBD-02, ID-1)

**Candidate instrument.** The number of active transactions of the **document database's** transaction manager (candidate: the managed `TransactionManager` count of active transactions), and a **typed identity check** of the top transaction. Transaction start and end events of the transaction manager, if they exist, are an **additional log**, never proof. **If no API with sufficient authority exists in the exact tuple, this is an implementation / instrumentation requirement and E-06 stays unsatisfiable until it is met.**

**Typed identity authority.** The identity of the top transaction is exposed only through a typed authority equivalent to

```text
IsTopTransaction(myTransaction)  ->  YES / NO / UNKNOWN
```

- It compares **live objects only**: as an internal implementation detail (never an identity the contract stores), it compares the native identity (`UnmanagedObject`) of the caller's transaction with that of the current top transaction **at that instant**.
- It **rejects** a disposed, null or **stale** transaction wrapper (a wrapper whose transaction has ended): the result is `UNKNOWN` (or `NO`), **never `YES`** for a different live transaction; it never compares with a stored value.
- The native pointer is **never persisted as a durable identity** and never used as a log key: a pointer value can be reused after a transaction ends. The log records the boolean result and a sequence number.
- **Scope:** the transaction manager of the **document `Database`**. Transactions of the library side database (AUTH-12 external queries) and of the warm-up side database (13.1) are **logged separately** and are **excluded** from the document count.

**What is counted:** transactions that are active on the document database's manager: the top-level one and nested ones (depth).

**Expected counts (sampling points)**

| Point | Expected |
|---|---|
| P0 | 0 |
| PL | 0 |
| AUTH-12 call or import, **before** | 0 |
| AUTH-12 call or import, **after** (the callee's own execution is not sampled) | 0 |
| PS | 1, the orchestrator's `T_M` (`IsTopTransaction = YES`) |
| PV | 1, same, no nested transaction |
| after `Commit()` returns, or after `Abort()` and disposal | 0 |
| VERIFY, per read scope | 0 → 1 → 0 (the verifier's own read transaction), or the exact equivalent the Architect reviews |

**UNKNOWN conditions.** The instrument throws or is unavailable; a negative or impossible value; two sources disagree (counter, own bookkeeping, events); the count changes between two samples with no observed start or end event when events are subscribed; a sample is missing at a required point; `IsTopTransaction` cannot decide.

**Blind spots (declared).** Transactions started and ended between two sampling points are seen only if events are subscribed and delivered; `OpenCloseTransaction` may not be counted (characterized in E6-C4), which is why `NG-09` exists. The product already uses an `OpenCloseTransaction` in `LateralHeaderDrawService.ReadBlockName` (section 15.1, F-6): the mirror path must not reach it (P-5).

**Reliability controls for the counter itself** (group `E6`, part of the qualification of I-07, section 4.3)

| Control | What it shows |
|---|---|
| E6-C1 | at rest inside a command the count is 0 |
| E6-C2 | after `StartTransaction` the count is 1; after a nested start it is 2; after commit, abort, and dispose-without-commit it returns as expected |
| E6-C3 | the count sees the callee transactions of AUTH-12 queries and imports (0 → 1 → 0) |
| E6-C4 | whether `OpenCloseTransaction` and a side database transaction are visible to the count |
| E6-C5 | two independent sources agree on a scripted sequence of starts and ends |
| E6-C6 | a deliberate violation (a second transaction started by an adversarial probe) is detected at a sampling point |
| E6-C7 | `IsTopTransaction` returns YES for the live top, NO for another live transaction, UNKNOWN for a disposed one |
| E6-C8 | `STALE_WRAPPER_CONTROL`: transaction A is finished; transaction B is created; the old wrapper of A is queried with `IsTopTransaction`; the result is **never `YES`** for B (it is `NO` or `UNKNOWN`), even if the native pointer of B equals the one A had |

### 4.3 Instrument qualification (ID-2)

Every instrument I-01..I-16 must be **qualified on the exact execution build before any governing run** (EXEC-2, EXEC-9). Qualification results are recorded in the evidence package. A failed qualification is an **instrument defect**, not a host `NOT_MEASURABLE`, and a governing run with an unqualified instrument cannot yield a `PASS`.

| Id | Positive control | Negative control | Consistency / repeatability | Acceptance | Failure semantics |
|---|---|---|---|---|---|
| I-01 clock, sequence | strictly increasing sequence over the run | an injected duplicate or gap is detected by the log validator | monotonic ticks across threads | no gap, duplicate or regression in the qualification run | `INSTRUMENT_DEFECT` |
| I-02 host identity | reads equal a known tuple | pointing at a different file gives a mismatch | two reads equal | equality with the known tuple and detection of the mismatch | `INSTRUMENT_DEFECT` |
| I-03 document collection | count 1 with one document | opening a second document gives count 2 | repeated reads equal | correct counts and identities | `INSTRUMENT_DEFECT` |
| I-04 system-variable reader | values equal an independent read of the same variables | an unreadable variable gives `UNKNOWN`, never a value | repeated reads equal | equality and correct `UNKNOWN` | `INSTRUMENT_DEFECT` |
| I-05 long-transaction reader | inactive baseline | an induced long transaction is detected | repeated reads equal | detection in the induced case | `INSTRUMENT_DEFECT` |
| I-06 lock-state reader | held mode reads back | after release it reads "not held" | repeated reads equal | correct state in both cases | `INSTRUMENT_DEFECT` |
| I-07 transaction counter | controls E6-C1..E6-C3 | E6-C6 | E6-C5 | all E6 controls pass | `INSTRUMENT_DEFECT` (or host `NOT_MEASURABLE` if a qualified control shows the host cannot expose the fact) |
| I-08 module enumerators, hasher | a known module appears with its known hash | a canary module loaded in qualification changes the snapshot | two quiescent snapshots equal; a locked file gives `UNKNOWN` | detection of the canary and equality when quiescent | `INSTRUMENT_DEFECT` |
| I-09 managed load subscriber | loading a canary assembly delivers an event | after unsubscribing, no event is delivered and the loss is detected | repeated loads deliver repeated events | delivery and loss detection | `INSTRUMENT_DEFECT` |
| I-10 event subscriber | known writes produce the expected categories | a category not subscribed is reported missing; overflow gives `UNKNOWN` | the same operation gives the same event multiset | coverage of each declared category | `INSTRUMENT_DEFECT`; a category the host does not emit is `CHARACTERIZED_DIFFERENT` |
| I-11 verifier, comparator, oracle | known-equal content compares equal | a single-field mutation, per field, is detected | independence from the writer and from `mu_k` by construction tests; no write path reachable by the verifier | per-field detection and no write surface | `INSTRUMENT_DEFECT` |
| I-12 fingerprint provider | conformance to the FINGERPRINT SPECIFICATION (section 28), both layers; `SIGNED_ZERO_CONTROL` (4.3.1) | a 1-ulp change alters the digest; an unknown entity gives `UNKNOWN` | identical content gives identical digest across two separate sessions | all three | `INSTRUMENT_DEFECT` |
| I-13 library identity and cache probe | a fresh acquire yields a hash-equal copy | a modified source is detected; a stale cache is detected; `SOURCE_SWAP_CONTROL` (4.3.1) | repeated acquires equal | detection of change and staleness | `INSTRUMENT_DEFECT` |
| I-14 adversarial writers | each writer produces its intended modification and events (verified by an independent read) | with the writer disabled no change occurs | repeatable at the same position | intended effect and no effect when disabled | `INSTRUMENT_DEFECT`; usable only in characterization builds |
| I-15 result and log writer | complete log | a truncated log is detected by sequence and hash | repeated runs give logs of equal shape | detection of truncation | `INSTRUMENT_DEFECT` |
| I-16 control plane | launches a fresh verified process; the evaluator reproduces every row of section 17.2 on synthetic logs | a wrong binary, an extra `acad.exe` or a tampered log is detected | repeated synthetic evaluations equal | all rows reproduced and all negatives detected | `INSTRUMENT_DEFECT` |

### 4.3.1 Named qualification and conformance controls (RC-9)

These controls are added to the qualification of section 4.3. Each is a scenario of the catalog (section 26) with a declared expected result.

| Control | Owner instrument / group | Procedure (concept) | Acceptance |
|---|---|---|---|
| `SOURCE_SWAP_CONTROL` | I-13 (library identity and cache probe), group `PR-FRESH` | after the authoritative acquisition (19.1 step 4), the control plane **modifies** the original source (bytes appended, or replaced by a different valid library) and, in a second variant, **deletes** it; PREPARE-W then runs the import | the import **succeeds** and the imported definitions equal the content of the **private copy**, not the swapped one; the `LibraryInputRecord` hash equals the copy hash; the drift is recorded as `SOURCE_DRIFT_AFTER_ACQUISITION` (informational). Negative variant: the **copy** is modified between acquisition and import; the pre-import copy re-hash detects it and the import does not start |
| `SIGNED_ZERO_CONTROL` | I-12 (fingerprint provider), group `Q` | two records identical except one coordinate `-0.0` versus `+0.0` | the `EXACT_STATE_FINGERPRINT` digests **differ**; the `SEMANTIC_COMPARISON` of the coordinate type declares them **equal**; a non-finite value yields `UNKNOWN` in the exact layer |
| `CLONE_NON_REPLACE_CONTROL` | SL-5 importer with I-13, I-12, group `PW` | the document already holds a homonymous library dependency with **different content** (a block, a nested block, and a layer, a linetype and a text style with different properties); the import runs under `DuplicateRecordCloning.Ignore` | every existing record keeps its pre-import **exact fingerprint** (unchanged); nested dependencies and symbol records obey the non-replacing policy; the manifest marks each bound existing record `REUSED_BOUND` with its pre-command fingerprint; no new record appears that is not in the ADD set of the closure (11.3); the declared cloning mode is on the allow-list |

## 5. CHECKPOINT MODEL

Checkpoints are those inherited from ALT-21D (V3 1.5, V4 1.1); the completion point `CP` is unchanged. Each checkpoint writes a **checkpoint record** (sequence, phase, list of control verdicts, tuple digest).

| Cp | What is measured | What must be stable | May be UNKNOWN | FAIL | Evidence written |
|---|---|---|---|---|---|
| **P0** entry, no lock, no product write | **warm-up sequence first** (section 13.1: warm-up, record loads, check membership in the manifest), then subscription (E-11M, E-12), then the baseline: E-01, E-02, E-04, E-05, E-06 (0), E-07, E-09, E-10 baseline (two snapshots), E-11M and E-12 armed, E-13, E-14, E-15; tuple | tuple fields; the two E-10 snapshots equal each other and the manifest | any reading (then O1) | any control FAIL (then O1) | tuple; control evaluation record; warm-up record; baseline snapshots; subscription confirmations |
| **PL** lock acquired | E-03 (mode read-back), E-04 again, E-06 (0) | lock held | lock read | lock not acquired or wrong mode | lock record |
| **PR** end of PREPARE-R | decision record, `LibraryInputRecord`, `PRE_COMMAND_STATE_RECORD` (authoritative capture) | record complete | a component unreadable (then O1) | any rejection decision | the record and its digest |
| **PP** end of PREPARE (PREPARE-W if any, else = PR) | `PREPARE_RESIDUE_MANIFEST` (exact); E-06 (0) | manifest matches what was completed | fingerprint unreadable | import failure | the manifest with fingerprints |
| **PS** inside `T_M` | source read `S`; re-observation of PREPARE outputs; E-06 (1, own); E-08 baseline observation | PREPARE outputs equal PP | a re-read | any difference (then abort) | `S` observation; re-observation results |
| **PV** before `Commit()` | staged reads (`R_pre`, `M_pre`); E-08 re-observation; `VERIFY_ELEMENT_LIST` sealed; continuity set (E-01..E-06, E-08, E-09, E-10, E-11M, E-12 phase A) | continuity unchanged | a control | any control FAIL or UNKNOWN (then abort) | sealed list and hash; staged results; continuity record |
| **V0** first read of pass 1 | start of the W-scan interval | — | — | — | marker record |
| **V1** end of the last read of pass 2 | end of the W-scan interval; **E-12 phase C ends and its log is sealed** | — | — | — | marker record; sealed phase-C log |
| **PC = CP** continuity after V1 | continuity set (E-10 snapshot, E-11M, guarantee-bearing controls); E-12 phase C **ended at V1 and its log was sealed there**: PC reads the sealed phase-C log and reports the `POST_V1_TO_CP_QUEUE_LOG` (10.8) | continuity unchanged | a control (then O7) | a control FAIL (then O7 or by precedence) | continuity record; the completion marker; the `POST_V1_TO_CP_QUEUE_LOG` |

`CP` remains the only contractual completion point. No product write occurs after PC (V3 section 2). Nothing here depends on a host command-end signal.

## 6. PREPARE-R AUTHORITY

- **Content.** Pure computation; library input identity and cache freshness (section 19.1); AUTH-12 presence queries; all validations and rejection decisions; capture of the **single** `PRE_COMMAND_STATE_RECORD`.
- **All rejections known from these inputs occur here**, before PREPARE-W, as clean O1.
- **`PRE_COMMAND_STATE_RECORD` for Selective** contains the read-set of section 11.1 (identities and exact fingerprints), captured once, **including, for every member of the static dependency closure of the required library blocks (11.3), the exact fingerprint of its pre-existing homologue or `ABSENT`**. Its digest is logged.
- **Static dependency closure.** Before PREPARE-W, PREPARE-R traverses the private library copy (read-only, no import) and computes the closure of the required library blocks (11.3). A closure member the traversal cannot classify (a proxy or unsupported entity) makes the closure `UNKNOWN` and the run is refused (O1).
- **Reads only.** AUTH-12 queries open their own transactions and commit them (callee-owned); the E-06 count must be 0 before and after each call (section 4.2). No product write may occur in PREPARE-R.
- **Log.** Every AUTH-12 call: sequence, count before and after, result, duration.

## 7. PREPARE-W AUTHORITY

- **Content.** Only the imports that PREPARE-R decided are necessary.
- **Before the first import:** re-observe the relevant elements of the `PRE_COMMAND_STATE_RECORD` and compare (V4 3.3); mismatch => no import, O1, empty manifest.
- **Before each import:** re-hash the **private library copy** and compare with the `LibraryInputRecord`; revalidate the generation id (section 19.1).
- **After each import:** compute and record the fingerprint of the imported definition (section 28); only then may it enter the `PREPARE_RESIDUE_MANIFEST`.
- **Import failure or indeterminate:** the definition does not enter the manifest; the run stops; ABORT-VERIFY compares against the record plus the manifest of completed imports (V4 3.4).
- **Composition rule for a preexisting definition (W-5 / TBD-13)** — section 11.3.
- **Residue.** The manifest is the exact list of what PREPARE-W added or changed and may persist after an abort (Proposal V5 Owner summary, item 9).

### 7.1 Import rules (SC-8, RC-4)

- **`MODIFY_PREEXISTING = PROHIBITED`.** PREPARE-W only **adds**.
- **Cloning mode.** The import must use a **verified non-replacing** cloning mode: today `DuplicateRecordCloning.Ignore` (verified in the importer on `main`), or a future equivalent **proven** non-replacing. The importer must **declare** the mode it used in its structured result, and PREPARE-W does not start unless the declared mode is on the allow-list. If the cloning policy differs or cannot be verified, the control is `FAIL` (differs) or `UNKNOWN` (cannot verify); **replacement is never silently permitted**. The behaviour is shown by `CLONE_NON_REPLACE_CONTROL` (4.3.1), not by reading the code alone.
- **Import source.** The import reads **only** the opened `Database` of the per-run authoritative private library copy (19.1). The importer receives that `Database`, never a path to the original source.
- **Closure.** Before any import, PREPARE-R computed the **static dependency closure** from the private copy (11.3). PREPARE-W adds exactly the **ADD set** of that closure.
- **Effect check.** After each import, the exact fingerprints of the pre-existing homologues recorded in the `PRE_COMMAND_STATE_RECORD` are re-read; any difference is an unexpected mutation (ABORT-VERIFY then decides O2 or O6). The set of newly present records must equal the ADD set; an extra record is unexpected.
- **Manifest.** The `PREPARE_RESIDUE_MANIFEST` lists the definitions and dependency records **added** by the import, each with its exact fingerprint, and marks every pre-existing record the imported content binds to as `REUSED_BOUND` (pre-command fingerprint; not residue).
- **Failure.** A partially failed import never becomes expected residue (section 11.3).

## 8. MUTATION AUTHORITY

- **Ownership.** The orchestrator acquires `LockDocument` (PL), opens `T_M`, calls the seam with the caller-owned `Database` and `Transaction`, performs `Commit()` or `Abort()`, disposes `T_M`, and keeps the lock through V1 (Proposal V2 section 8; V4 1.1; V5 section 2). The seams acquire no lock and do not commit, abort or dispose.
- **Sequence inside `T_M`:** open (count 1) → source read `S` and re-observation of PREPARE outputs (PS) → seam writes → staged reads → seal of `VERIFY_ELEMENT_LIST` → PV → `Commit()` or `Abort()`.
- **Events.** The `EXPECTED_MUTATION_EVENT_SET` of phase A is derived from the plan (section 14.3); an event not deterministically attributable aborts before `Commit()`.
- **State of `T_M`.** Fixed when `Commit()` returns normally or throws (V5 W-1): `COMMIT_ACCEPTED`, `ABORTED` or `FAILED`.

## 9. COMMIT BOUNDARY

- From the end of PV evaluation to V0.
- Events are **logged, not adjudicated** (`NG-19b`); non-DB host-visible actions (regen, redraw) run in this interval, before V0.
- `Commit()` returning normally sets `COMMIT_ACCEPTED`; throwing or indeterminate sets `FAILED` and takes the C-1 path (V4 1.2).
- The characterization records every event in this interval (input to A-2 and to the event model of section 14.3).

## 10. VERIFY / W-SCAN AUTHORITY

### 10.1 Interval and passes

`W-scan` = V0 to V1 (V3 1.5). Pass 1 reads every element of the sealed list, in a fixed order, and compares with the expected value through the independent comparator. Pass 2 re-reads every element in the same order and compares with pass 1 and with the expected value. Continuity is evaluated after V1 (PC).

### 10.2 The sealed list for Selective (`VERIFY_ELEMENT_LIST`)

Elements (each with an expected digest computed from the plan and the oracle before `Commit()`): every destination sibling view (definition and reference) for the new `RackId`; each sibling's authored envelope (payload); the address (`RackViewAddress`) and grouping of every sibling; the source read-set elements; the NOD registry entries the kind consumes; the containers enumerated to check the list (section 10.3). The exact enumeration per kind is fixed by the kind authority baseline (section 15).

### 10.3 `PROTECTED_OBJECTS` for Selective

Derived deterministically from the sealed list (V3 4.1): every list element; every source object of the read set; the enumerated containers (block table record of the target space, extension dictionaries of the destination definitions, the destination sibling set, the NOD entries consumed); and any other object whose stability the comparison needs, fixed by the kind authority baseline. Identity is by **database handle**, with class and logical role. The set is hashed with the sealed list.

### 10.4 Read-only abstraction

The verifier reaches the drawing only through a read-only abstraction with no write-capable surface. Opens are read-only; no upgrade to write. A verifier that writes or upgrades is a defect: `COMMITTED_UNVERIFIED` with reason `VERIFIER_DEFECT` (V3 1.4). Enforcement is a T0 static guard and T1 tests at implementation; CT-21D verifies by the absence of write-class events on the protected objects during W-scan (assumption A-1, section 14.7).

### 10.5 Elements discovered during VERIFY

A container enumeration is a check **against** the sealed list. A member not in the list, or a listed member not found, is an observation that differs from expected. Nothing found during VERIFY can enter the list.

### 10.6 Scan characterization experiment (TBD-09, ID-5) — design only, not executed

**Goal.** To measure what the two-observation scheme and the phase-C event fence detect and miss, and how often a correct state produces a false terminal failure. **`CHARACTERIZATION != STABILITY PROOF`**: no result is a claim of stability.

**Adversarial writers** (I-14), each a synchronous same-context call injected at a deterministic injection point of a **characterization build** of the verifier, and each also available as an **event-triggered variant** (the same writer invoked from a database event handler that a benign, declared operation triggers):

| Writer | Effect |
|---|---|
| Wr-A | modify the payload of element `k` to a different value and leave it |
| Wr-B | modify element `k` and restore it before the next observation of `k` (ABA) |
| Wr-C | modify element `k` after its own pass-2 read (tail) |
| Wr-D | append an unexpected sibling |
| Wr-E | erase element `k` |
| Wr-F | change a NOD entry the kind consumes |
| **Wr-G** | modify a **source** object or state of the read-set |
| Wr-S | a **silent** variant of Wr-A/B/C that emits no delivered event (only where the host allows such a write; used to measure the event-less residual) |

**Injection positions:** X0 after `Commit()` returns and before V0; X1 after V0 and before the pass-1 read of `k`; X2 immediately after that read; X3 between pass 1 and pass 2; X4 immediately before the pass-2 read of `k`; X5 immediately after it (tail); X6 after the last read and before V1; X7 between V1 and the PC evaluation; X8 after CP.

**Measured for every finding: the detection channel.** Each observation records which channel detected the change: **`SEMANTIC_COMPARISON`** (pass 1 or pass 2 against expected), **`E-12`** (a phase-C write event on a protected object), **`BOTH`**, or **`NEITHER`**. Also measured: the outcome (SUCCESS, O4, O5, O7); a **missed change** (SUCCESS reported); the false terminal failures; A-1 and A-2; the **duration of W-scan** (V0 to V1) and the **per-element read timing** (time from each element's pass-2 read to V1), for representative sizes.

**Expectation table (by channel; to be declared per scenario before the first governing run)**

| Case | Semantic comparison | E-12 (phase C) | Expected combined result |
|---|---|---|---|
| persistent change (Wr-A, Wr-D, Wr-E, Wr-F, Wr-G) at X1..X4 | detects | detects if the event is delivered | O4 (by precedence over O7). **Designed detection: guarantee-bearing** |
| persistent change at X5, X6 (tail) | does not detect | detects if the event is delivered | O7. A miss of both channels is recorded as the residual |
| ABA (Wr-B) between the two reads of `k` | does not detect | detects if the event is delivered | O7. A miss of both channels is recorded as the residual |
| any writer at X7 | outside W-scan and outside the passes | outside phase C (ends at V1); **recorded, not adjudicated,** in the `POST_V1_TO_CP_QUEUE_LOG` (10.8) if the event is delivered | not detected by the guarantee: **disclosed residual**. The X7 scenario measures what the tail log sees |
| any writer at X0 | detects a persistent change | boundary events are logged, not adjudicated (`NG-19b`) | O4 for a persistent change; an ABA restored inside the boundary is `NEITHER`: residual |
| any writer at X8 | outside the guarantee (W3) | outside | recorded for the disclosure only |
| silent writer (Wr-S) at any position in W-scan | detects a persistent change at X1..X4 only | no event | the residual that remains |

The residual therefore concentrates on: writes with **no delivered event**, positions X7 and X8, an ABA restored inside the commit boundary, and transient changes that escape both channels. **Position X7 stays a disclosed residual**: E-12 phase C ends at V1 (Proposal V5), and this contract does not move that boundary. The `POST_V1_TO_CP_QUEUE_LOG` (10.8) measures X7 without adjudicating it. **Verdict semantics (section 1):** a persistent change at X1..X4 that the semantic comparison misses is a `FAIL` of a guarantee-bearing mechanism (class A); a miss by E-12 where the event was delivered is a class-B `FAIL` (`AMENDMENT_PENDING`); the residual cells are **recorded, not failed**.

**Baseline runs without any writer** (a number of governing runs fixed at the baseline with an explicit statistical intent, section 21.7) measure the **false terminal failure rate** (O7 or another non-success on a correct state) and evaluate **A-1** and **A-2** (section 14.7). The rate is reported; **no threshold is set here**; it is an Owner Act 2 input.

### 10.7 Post-CP characterization (replaces TBD-11)

`POST_CP_CHARACTERIZATION_REQUIRED_BEFORE_ACT2`: for a bounded tail after CP (the length is operational, recorded and informational only: section 21.7), record every event on protected objects and re-read the sealed list once. The result is a **report**; it is not an input to SUCCESS.

### 10.8 `POST_V1_TO_CP_QUEUE_LOG` (RC-6)

E-12 phase C **ends at V1**. This contract does **not** extend the phase-C fence to PC or CP.

| Property | Rule |
|---|---|
| content | every event delivered on the subscribed categories between V1 and the CP completion marker (database, document and transaction events; the same record shape as 14.2, with the phase tag `POST_V1_TO_CP`) |
| adjudication | **none**. The log is **non-adjudicated**: it never produces PASS, FAIL or UNKNOWN by itself and is not an input to `EVT_C` |
| sealing | the log is sealed at the CP completion marker and **read and reported at PC** |
| effect on SUCCESS | **none**, unless an existing approved rule explicitly says so (none does) |
| relation to phase C | it does **not** extend phase C; phase C's own log was sealed at V1 |
| X7 scenario | group `SC-B` includes an X7 scenario per writer (10.6) that reports what the tail log delivered for the injected change |
| extension of the fence | any future extension of the E-12 fence through PC requires a **reviewed amendment** and an **Owner disclosure**; this section is not that amendment |

## 11. ABORT-VERIFY AUTHORITY

```text
EXPECTED_AFTER_ABORT  =  PRE_COMMAND_STATE_RECORD  +  EXACT PREPARE_RESIDUE_MANIFEST
```

Applies after `Abort()`, after `T_M` state `FAILED` (including a `Commit()` exception), and after a PREPARE-W failure. A fresh read-only transaction under the still-held lock. No callback is ever proof; `cancelled` does not prove rollback; the absence of `modifyUndone`, `modified` or `closed` is not a signal (`04NO-S` restrictions R-1..R-6).

### 11.1 Abort read-set for Selective (TBD-13, SC-9)

**Scope statement.** "Unexpected mutation" is defined **only within this normative read-set**. **Non-RackCad entities outside this read-set (the content and properties of preexisting drawing entities that are not listed) are NOT covered by this guarantee.**
Any normative component without an authoritative reader or comparator is an **execution blocker**, never silently omitted.

| Component | Compared | Reader / comparator status |
|---|---|---|
| RackCad-owned definitions and library-derived definitions | full name set of the block table; per-definition **exact fingerprint** (section 28) for RackCad-owned and manifest definitions and for **every pre-existing homologue in the static dependency closure (11.3)** | name enumeration trivial; **canonical definition dump / fingerprint reader: MISSING** |
| target space content | full handle set of the space's entities, and the RackCad reference set with class | handle enumeration trivial but not present as an instrument: **MISSING** |
| authored payload and envelopes | scan of every definition's RackCad payload and envelope; set of `RackId`; expected: no new `RackId` | `RackBlockData.Read` exists (code evidence); the aggregate scan comparator for this purpose: **MISSING** |
| bindings and addresses | per-view `RackViewAddress` and grouping | code evidence (Foundation); host reader: **MISSING** |
| library-derived residue | every imported definition of the manifest and its dependency closure, fingerprint | covered by the definition reader: **MISSING** |
| **symbol-table records** | name sets of layers, linetypes, text styles, dimension styles and registered applications, **and the exact fingerprint (or `ABSENT`) of each pre-existing record of the static dependency closure (11.3) and of each record the creators use or may touch (for example every layer the plan draws on: name, color, linetype, lineweight, on/off, frozen, locked, plot flag, transparency)** | **MISSING** |
| NOD and project-variable state | the registry entries relevant to the kind, fingerprint | I-49 store (code evidence); host aggregate reader: **MISSING** |
| identities | handles, `RackId`, sibling grouping identity | as above |

### 11.2 Verdict

Exact match with `EXPECTED_AFTER_ABORT` => O2. Extra residue, mismatch, insufficient read or UNKNOWN => O6.

### 11.3 Static dependency closure, reuse and cases (W-5, SC-8, RC-4)

For **Selective V1** the kind rule is:

- **`MODIFY_PREEXISTING = PROHIBITED`.** PREPARE-W only **adds**.
- **RackCad view definitions are always NEW.** The frontal and planta definitions of a mirrored rack get a unique effective name under the family naming policy (15.5), computed in PREPARE-R under the lock. They are never `REUSE_AS_IS`. A name collision is resolved by the naming policy before any write; a collision the policy cannot resolve is a refusal (O1).
- **Library-derived definitions and dependencies may be `REUSE_AS_IS`.** If a required library block, or a dependency of the closure, **already exists** in the document as a record of the expected kind, it is reused and is **never modified** by PREPARE-W. It is not compared with the library content as an admission condition.

**Static dependency closure.** Before PREPARE-W, PREPARE-R computes, from the **private library copy** (19.1), by read-only traversal and without importing, the closure of the required library blocks. The closure includes, as applicable:

- nested block definitions;
- symbol-table records: layers, linetypes, text styles, dimension styles, registered applications, and other symbol tables the traversal meets;
- other cloned dependencies the import would create.

For **every** closure member PREPARE-R queries the document with **host symbol-table semantics** and records the result in the `PRE_COMMAND_STATE_RECORD`: the **exact fingerprint of the pre-existing homologue, or `ABSENT`**, the record kind, and the **stored case** of the pre-existing name. Each member is then classified:

| Class | Condition | PREPARE-W / manifest effect |
|---|---|---|
| `ADD` | `ABSENT` | the import will add it; after the import it enters the manifest with its exact fingerprint |
| `REUSE_AS_IS` | a pre-existing record of the expected kind for a **required top-level** library block | not modified; fingerprint stays in the record |
| `REUSED_BOUND` | a pre-existing record of the expected kind for a **nested or dependency** member that the imported content will bind to | not modified; the manifest marks it `REUSED_BOUND` with its **pre-command fingerprint**; it is not residue |
| `REJECT` | the name exists but as an object that is not a definition of the expected kind (see below) | refusal, O1, nothing written |

**Name collision uses host symbol-table semantics**, including case insensitivity where the host applies it. A homonym that differs only in case counts as pre-existing (`REUSE_AS_IS` or `REUSED_BOUND`); the record keeps the **actual stored case**, and a case difference from the expected name is a secondary finding.

**Rejection grounds (closed list, no catch-all).** `REJECT` applies only when, in the **block table**, the pre-existing name is a layout, an anonymous block, or an external-reference or overlay record (that is, not a block definition of the expected kind). For the other symbol tables the only refusal ground is an unreadable or `UNKNOWN` record. A further ground requires a reviewed amendment that adds it to this list.

**Reused external geometry is not verified.** The contract does **not** guarantee the semantic correctness of a reused pre-existing library-derived definition or dependency (it may differ from the library content). It only guarantees that PREPARE-W does not modify it and that any later change is detected as unexpected. This limitation is disclosed for Owner Act 2 (section 24, item 12) as `REUSED_EXTERNAL_GEOMETRY_NOT_VERIFIED`. Verification of a reused definition against the library needs a separate, reviewed rule.

**Cloning.** Only under a verified non-replacing cloning mode (`DuplicateRecordCloning.Ignore` or a proven non-replacing equivalent), section 7.1.

**Consequence.** `EXPECTED_AFTER_ABORT` = the `PRE_COMMAND_STATE_RECORD` **plus** the ADD set actually added (listed in the manifest with exact fingerprints), and **nothing else**.

For any future kind that permits modification of a preexisting definition, its kind baseline must define: (1) what the preexisting state represents in the record; (2) which PREPARE-W change is permitted residue; (3) how `EXPECTED_AFTER_ABORT` is verified for that definition; (4) how permitted residue is distinguished from an unexpected modification.

**Cases distinguished**

| Case | Recognition |
|---|---|
| legal PREPARE residue | a definition or dependency record of the ADD set **added**, present in the manifest with a recorded exact fingerprint, equal to it now |
| bound pre-existing record | a `REUSED_BOUND` or `REUSE_AS_IS` record, unchanged from its pre-command fingerprint |
| partial failed import | a definition, entity content or dependency record not in the manifest (the import threw, or its fingerprint was not recorded) => unexpected => O6; **never legitimized by the manifest** |
| unexpected mutation | any change to an element of the record, or any new element not in the manifest => O6 |
| expected state after abort | the record plus the manifest additions |

## 12. E-10 AUTHORITY (TBD-04)

`KNOWN_MODULE_SET_ATTESTATION`. It attests the **enumerable known set** at evaluation points. It is **not isolation** and proves nothing about other code.

### 12.1 Sources (all candidates)

| Set | Candidate source | Notes |
|---|---|---|
| managed assemblies | assemblies loaded in the current application domain and load contexts | each entry: assembly full name, location, module version id, file hash |
| managed loader modules known to the host | the host's loaded-modules enumeration | ObjectARX-side modules as the managed API reports them |
| native ARX applications | the host's ARX-loaded enumeration; needs a native or P/Invoke call: existence to be characterized | if unobtainable from the managed product, this is a **declared incompleteness**, not a silent omission |
| native process modules | the process module list | documented as a fallible snapshot: races during load or unload; omits modules loaded as data files |

### 12.2 Identity fields per entry

Normalized full path; file size; file SHA-256 (computed at each evaluation point); file version information; for managed assemblies also the **module version id read from the file** compared with the **loaded assembly's module version id** (`MVID_CONSISTENCY`).

- **`MVID_CONSISTENCY = ADDITIONAL_CONSISTENCY_SIGNAL`.** A **mismatch is an E-10 FAIL**. A **match makes no claim** of loaded-image identity and does not prove image integrity (`NG-16`).

### 12.3 Comparison

| Point | Rule | On failure |
|---|---|---|
| baseline (P0) | two snapshots equal each other and equal the manifest set entry by entry (path and hash); no other member | refuse (O1) |
| PV | one snapshot equal to the manifest and to the baseline | abort (O2) |
| PC | one snapshot equal to the manifest and to the baseline | mandatory control failure (O7 by precedence) |

The comparison is **EXACT set equality** in this contract version. The availability cost (a demand-loaded module changing the set) is measured and reported, not softened. Hashes are recomputed at each point in CT-21D; a metadata-only shortcut needs a reviewed amendment.

### 12.4 UNKNOWN conditions

An enumeration throws or is unavailable; a file cannot be read for hashing (locked, denied); an entry has no file (in-memory assembly); path normalization is ambiguous; two enumerations of the same set disagree beyond a load the events explain.

**Elapsed time never creates a semantic `UNKNOWN` inside E-10.** A **control-plane watchdog** (section 21.1) that expires may make the run or the instrument status `INVALID`; that is a run status, not an E-10 verdict.

### 12.5 Known incompleteness (declared)

Loaded image versus file (`NG-16`); LISP, VBA, in-process COM, reflection-loaded code (`NG-17`); data-file modules and native code no enumeration reports (`NG-18`); transient loads and unloads between snapshots and native load events (`NG-10`). Snapshot equality is **not** isolation and **not** proof of absence of other code.

### 12.6 Instrument warm-up

Hashing, cryptography and file I/O can load modules. The instrument therefore runs its enumeration and hashing **once during the warm-up** (section 13.1), before the subscription and the baseline, so that its own dependencies are already loaded and belong to the manifest.

## 13. E-11M AUTHORITY (TBD-05) AND WARM-UP ORDER (SC-11)

Managed load continuity. **Native load events are out of authority** (V5, `NG-10`).

### 13.1 Warm-up order and warm-up vehicle (SC-11, RC-3)

The order is fixed:

| Step | Action |
|---|---|
| 1 | **warm-up** of the instrument and of the product code paths that will run inside the governed window, executed on the **warm-up vehicle** below; it performs **no** document write and no product mutation on the governed document |
| 2 | **record all loads** of the warm-up (snapshot before and after the warm-up; the difference is the warm-up load record) |
| 3 | **check that every warm-up load belongs to the manifest**; a warm-up load outside the manifest refuses the run (O1) |
| 4 | **subscribe** (E-11M and E-12) |
| 5 | **capture the baseline** (E-10, two snapshots) |
| 6 | enter the **governed window** |

**Warm-up vehicle.** The warm-up runs against an explicit `WARMUP_SIDE_DATABASE`:

- a **scratch side `Database`**, created and owned by the control plane, **never the governed document's `Database`**; it is disposed before step 4;
- its transactions are **outside the normative document transaction count** (E-06 scope is the document database's transaction manager, 4.2) and are **logged separately** as `WARMUP_SIDE_DB` records;
- it is **never** a governed product mutation, takes **no** document lock, and **no resulting product state transfers** to the governed document (its content is discarded);
- it exercises the required paths with a synthetic minimal plan per family: the seams SL-1, SL-2 and SL-4 (they receive the side `Database` and a side transaction as their caller-owned arguments), the SL-5 import from the private library copy into the side database, the fingerprint provider, the comparator, the log writer, the hashers and the cryptography and I/O dependencies; the E-12 and E-11M handler code is exercised by attaching the handlers **to the side database only** and detaching them before step 4.

**The warm-up must load** (as far as deterministic): the product assemblies; the resource and satellite assemblies for the message cultures the run may use; the instrumentation dependencies; the cryptography and I/O dependencies; and any other deterministic lazy-load dependency the governed window reaches.

**`WARMUP_COVERAGE_CRITERION`.** The warm-up set is sufficient for a scenario when, in the **non-governing dry runs** of that scenario on a scratch document (the `WU` group; the number of dry runs is a classified parameter, 21.7), **no assembly-load event occurs inside the governed window** after steps 1 to 5. The criterion is measured, not asserted: a load inside the window is an E-11M `FAIL` (class B) and a design finding.

**Declared residual.** Code paths that can lazy-load **only during a real document mutation** and cannot be exercised by the warm-up remain: error and abort paths, paths that depend on document-specific object types, and rare plan multiplicities. A load on such a path inside the window is a finding under E-11M; it is not tolerated and not hidden.

**`PRELOAD_SET` (the warm-up set) is an operational restriction, not isolation evidence.** It shows that the product's own first-time loads happened before the window; it says nothing about the absence of other code. **The warm-up vehicle claims no isolation.**

### 13.2 E-11M

| Aspect | Contract |
|---|---|
| source | the managed assembly-load event of the application domain (candidate), plus the managed snapshot equality already carried by E-10 |
| subscription lifecycle | subscribe at step 4, **after** the warm-up and **before** the baseline snapshot; unsubscribe after PC; the subscription object is retained by the orchestrator |
| window | events from subscription to PC are recorded; events between subscription and completion of the baseline snapshot make the baseline invalid: refuse (O1) |
| record per event | sequence, tick, thread id, assembly full name, location, module version id, phase tag |
| loss of subscription | subscribe failure => refuse (O1); handler exception, disposal, or any condition that could have dropped events => the control is UNKNOWN |
| event gaps | an assembly present in the PV or PC snapshot but absent from the baseline **without** a load event => gap => UNKNOWN; a load event without a snapshot difference => recorded and FAIL |
| unexpected load | any load event in the window is FAIL (zero load events is PASS) |
| identity | full name, location, module version id, file hash |
| UNKNOWN | subscription failure or loss; missing sequence numbers; inconsistent snapshot and event evidence |

If `CT-21D` shows that first-time loads cannot be avoided inside the window, E-11M is not satisfiable as designed: the verdict is class B (`AMENDMENT_PENDING`, section 1.4) and a **reviewed amendment** is required (no silent relaxation).

## 14. E-12 AUTHORITY (TBD-06)

Phase policy is **fixed by V3 section 4 and V5** and is not reinterpreted:

| Phase | Policy |
|---|---|
| PREPARE (0) | log only |
| MUTATION (A) | deterministic `EXPECTED_MUTATION_EVENT_SET`; unattributable event => fail closed => Abort before `Commit()` |
| COMMIT BOUNDARY (B) | log, no adjudication (`NG-19b`) |
| W-SCAN (C) | expected write events on `PROTECTED_OBJECTS` = EMPTY |
| subscription failure at P0 | refuse; later loss => UNKNOWN |
| inference | absence of events is not a signal; no origin inference |

### 14.1 Event sources and categories (candidates; to be confirmed by characterization)

| Category | Content |
|---|---|
| WRITE_CLASS | database events for object appended, modified, erased, opened for modify, reappended, unappended |
| STRUCTURE_CLASS | document collection events (created, destroyed, became current), document command events (will start, ended, cancelled, failed), editor events |
| TX_CLASS | transaction manager start and end events |
| LOAD_CLASS | E-11M events |

Which of these exist and fire in the exact tuple is a **characterization result**. If a needed category is missing, that is declared, not assumed.

### 14.2 Subscription lifecycle and log record

Subscribe at P0 before the baseline; unsubscribe after PC. Handlers do not throw, write nothing to the database, and append a record: run sequence number, monotonic tick, UTC time, thread id, category, event kind, database handle, class, current phase tag, phase-step id. Ordering is by sequence. Event-loss rule: handler exception, buffer overflow, sequence gap or unexpected unsubscribe makes the control UNKNOWN for the affected phase (A => abort; C => O7; P0 => refuse).

### 14.3 Expected-set representation and the three-level authority (SC-5)

`EXPECTED_MUTATION_EVENT_SET` = a list of entries `(event kind, target selector, step id, multiplicity 1)`; a selector is a database handle when known in advance, otherwise `(class, container, logical role)` bound to a handle when the first matching event consumes the entry; later events on a bound handle are matched to entries that name it. Entries may carry a **partial order**; without one, matching is order-insensitive.

E-12 has **three levels of authority**. The full procedure is in section 27.

| Level | Content | Fixed |
|---|---|---|
| **1. contract** | event categories; the derivation schema `f(plan)`; the determinism criterion; the subscriptions; the ambiguity rules | before governing runs |
| **2. event model** | the versioned artifact `EVM-Selective-vX`, with hash, source learning corpus, Architect review status and the operation-to-event mapping | frozen after learning and review, before validation |
| **3. governing validation runs** | separate sessions, disjoint from the learning corpus, including plans outside it | governing |

- **Learning uses only micro-scenarios that isolate single operations.** Every model entry must be attributable to a **product operation** or to **documented host behaviour**. There is **no rule "everything observed becomes allowed"**.
- **The same run never teaches and validates.**
- Any change to the model, the write-event categories, the subscription set, the tuple, or seam code that affects events requires a **new event-model version, review and revalidation**.
- `NB-4` remains open until the procedure and the learning corpus are baselineable (section 27).

### 14.4 Protected-object identity

By database handle, class and logical role; the set is that of section 10.3.

### 14.5 Ambiguous-event rule

An event that matches more than one open entry, matches none, has no phase tag, or falls in a phase transition window is **ambiguous** => fail closed (phase A) or mandatory control failure (phase C). Tie-breaking by timing or order is not allowed unless a partial order is declared.

### 14.6 Blind spots (declared)

`NG-19` (a foreign event that coincides with an expected entry is indistinguishable) and `NG-19b` (PREPARE and commit-boundary events are not adjudicated).

### 14.7 A-1 and A-2 characterization

| Assumption | Experiment |
|---|---|
| **A-1** read-only opens emit no write-class events on protected objects | group `EA1`: each protected class is opened read-only through the verifier abstraction, enumerated, and its payload read, N times; expect zero WRITE_CLASS events |
| **A-2** the host emits no deferred write events on protected objects after V0 as a consequence of `Commit()` | group `EA2`: after known-correct commits, record WRITE_CLASS events on protected objects from `Commit()` return through V1 and the post-CP tail; expect zero after V0 |

Both are `UNVERIFIED_PRE_ACT1_ASSUMPTION` until characterized. Neither is authority nor isolation. **A false result is `CHARACTERIZED_DIFFERENT`** (cause `EMPIRICAL_BEHAVIOR_DIFFERENCE`); the class of A-1 and A-2 is the one recorded in the `CONTROL_CLASS_REGISTRY` (section 30, entries `A-1` and `A-2`, class B by derivation), hence `AMENDMENT_PENDING` (section 1.4): it cannot be degraded silently, it needs a reviewed amendment and recharacterization, or a recorded determination that no acceptable amendment exists (then `FALSE`).

### 14.8 Adjudication domain of E-12 (RC-11)

Proposal V3 4.2 fixes that **events outside `PROTECTED_OBJECTS` are recorded and not adjudicated** (row E). To make phase A operable, this contract fixes the domain in which events are adjudicated:

| Phase | Adjudication domain | Outside the domain |
|---|---|---|
| A. MUTATION | events whose target is a member of `PROTECTED_OBJECTS` known at PV, **or** an object **created inside `T_M`** (listed by handle in the seam results) | recorded, **not adjudicated**; becomes evidence for the event model (27) |
| C. W-SCAN | events on `PROTECTED_OBJECTS` (expected write set = empty) | recorded, not adjudicated |

- An event whose **target cannot be resolved** to a handle, or that matches more than one open entry, is **ambiguous** (14.5) and fails closed.
- An event on an object outside the domain that the model cannot explain is recorded as `UNEXPLAINED_EVENT` (27.2 step 4); it never becomes an implicit permission and it never blocks phase A by itself, but it blocks the freeze of the event model if it recurs in learning.
- This section **clarifies** V3 4.2 row E for phase A; it does not change any phase policy. The interpretation is listed as an open question (32.3).

## 15. SELECTIVE KIND AUTHORITY DESIGN (B5)

**Nothing is implemented.** This section is the **authority design** of the caller-owned creation seams that a future RACKMIRROR would need for the Selective kind, modelled on the AUTH-15 seam (`RackDefinitionCreator.CreateInTransaction`).
Names below are **working labels** of roles; final API names are an implementation matter. The design makes no claim that any seam exists.

### 15.1 Facts about the current Selective path (verified on `main` `3375aadb`)

| Id | Fact |
|---|---|
| F-1 | Selective **frontal** and **planta** definitions are created by `SystemBlockWriter.CreateBlock`, which itself takes `LockDocument`, calls the library importer, opens its own transaction, writes the payload with `RackBlockData.Write` and calls `Commit`. The **lateral** path (`LateralHeaderDrawService`) does the same. Reference placement (`BlockPlacement.PlaceBlockWithJig`) is a separate interactive step with its own lock and transaction. **Each view is a separate call and a separate transaction**; sibling views are grouped by the envelope id, not by an atomic unit |
| F-2 | The Selective **lateral** view appears to use the HeaderRun plan family, which the AUTH-15 HeaderRun overload dispatches, and the lateral is **outside the sealed RACKMIRROR scope** (section 25; V17 L-01). Frontal and planta have **no** caller-owned seam. AUTH-15 never places references |
| F-3 | The rack id is minted in the UI (`RackEditorSession.Complete` -> `EnsureId`, a new GUID string); the plugin receives it. `NewRackId` exists only in duplication code |
| F-4 | The edit side already has caller-owned forms (`SystemBlockWriter.RedefineInTransaction`, `ViewBlockDraw.PrepareRedraw`, used by `ProjectVariableMutationExecutor` under one lock and one transaction): a precedent pattern, not a seam for creation |
| F-5 | The importer runs inside the document lock and outside the drawing transaction, opens its own transaction, and **returns 0 silently on a missing file or an exception**. The library cache is keyed on path, last-write time and length, with **no content hash** |
| F-6 | The product uses an `OpenCloseTransaction` in `LateralHeaderDrawService.ReadBlockName`. The mirror path must not reach it |
| F-7 | Selective creation writes **no NOD** |
| F-8 | Read side: `RackBlockData.Read`, `RackBlockFinder.ScanEnvelopes`, `SelectiveAuthoredAuthority.Resolve`. `Resolve` compares the **siblings among themselves**; **no expected-versus-persisted comparator exists** |

### 15.2 Seam set (roles) and SL-3

| Seam | Role | Caller-owned | Required by the sealed scope V1 (section 25) |
|---|---|---|---|
| SL-1 | create the **frontal** view definition (one per depth `k`) in the caller's transaction | yes | **required** |
| SL-2 | create the **planta** view definition in the caller's transaction | yes | **required** |
| SL-3 | the **lateral** view definition: its **own Selective contract** (below) | yes | **not required** (the Selective lateral is out of scope, V17 L-01); the contract is retained for the case in which the scope changes |
| SL-4 | **place the block reference** in the caller's transaction from a computed transform (no jig) | yes | **required** |
| SL-5 | PREPARE-W **import** with a structured result and a fresh acquire of the library (outside `T_M`, under the lock) | callee transactions allowed (AUTH-12 pattern) | **required** |
| SL-6 | the **read-only verifier** and the expected-versus-persisted comparator | read-only | **required** |

**SL-3 has its own Selective contract.** The clauses of section 15.3 apply to it. **AUTH-15 is not the authority of SL-3.** AUTH-15 may later **implement** SL-3 only if **equivalence is demonstrated** for all of:

| Id | Equivalence requirement |
|---|---|
| EQ-1 | the same plan type |
| EQ-2 | the same dispatcher and path |
| EQ-3 | the same caller-owned transaction semantics |
| EQ-4 | exact Option-B host evidence on that path |
| EQ-5 | the same relevant authored and persistence contract |

Equivalence is a **future prerequisite, not a current fact**. Until it is demonstrated, **no AUTH-15 evidence transfers** to SL-3.

### 15.3 Design clauses (each is a requirement on SL-1..SL-4)

| Clause | Design |
|---|---|
| caller-owned `Database` | the seam receives the document `Database`; it never opens a database of its own |
| caller-owned `Transaction` | the seam receives the orchestrator's `Transaction` and, like AUTH-15, **verifies that it is the live top transaction through the typed authority `IsTopTransaction(myTransaction)`** (4.2); a `NO` or `UNKNOWN` is a PRE-WRITE failure. No native pointer is persisted as an identity |
| no internal `Commit` | forbidden; static guard |
| no internal `Abort` | forbidden; static guard |
| no `Dispose` of the caller transaction | forbidden; static guard |
| no `LockDocument` | forbidden; the orchestrator holds the lock; static guard |
| no `OpenCloseTransaction`, no side database write | forbidden; static guard (this excludes today's `ReadBlockName` from the mirror path, F-6) |
| deterministic result | the same plan and the same block-table state give the same effective name, definition content and result |
| structured failure | typed failures split into PRE-WRITE and POST-WRITE (15.4) |
| exact authored payload | the envelope is composed **by the caller** and written **verbatim**; the seam reads it back and compares (AUTH-15 pattern); it never rewrites, restamps or reinterprets |
| exact `NewRackId` policy | 15.5 |
| placement and transform contract | 15.6 |
| participation in one `MUTATION` transaction | every definition and every reference of the logical rack is created inside `T_M`; no seam commits |
| rollback compatible with Option B | seams write only document-database objects under a held lock in the caller's transaction; a POST-WRITE failure obliges the caller to abort; the seam claims no cleanup |
| fresh reread and comparator support | 15.7 |
| explicit library and input dependencies | the plan declares its required library blocks; SL-1, SL-2 and SL-4 (and SL-3 if it is ever authorized) **do not import**; a missing required block is a typed failure; imports belong to SL-5 in PREPARE-W |
| failure atomicity | a failed seam call leaves the caller obliged to abort; a seam never reports success over a partially written definition |

### 15.4 Result and failure typing

Result (working shape): `IsSuccess`, `DefinitionId`, `EffectiveBlockName`, `ReferenceId` (SL-4), the **handles of all created objects** (for the sealed list and the protected objects), `MissingInstances`, `Diagnostic`.

| Failure | Class | Meaning |
|---|---|---|
| `TransactionMismatch`, `InvalidPlan`, `InvalidBlockName`, `InvalidEnvelope` | PRE-WRITE | nothing was written |
| `InvalidTransform` | PRE-WRITE | determinant not strictly positive, or scale outside the kind's declared tolerance (ADR-0036: no negative scale) |
| `TargetSpaceInvalid` | PRE-WRITE | the target space cannot be opened for write in this transaction |
| `MissingLibraryBlocks` | PRE-WRITE if detectable from the declared requirements, otherwise POST-WRITE | required block absent |
| `DefinitionWriteFailed`, `EnvelopeWriteFailed`, `ReferencePlacementFailed`, `EnvelopeReadbackMismatch` | POST-WRITE | something may have been written; the caller must abort |

### 15.5 `NewRackId` and naming policy

- **`NewRackId`** is minted **once per logical mirrored rack** by the orchestrator (a new GUID string, as the editor's `EnsureId` does), is never equal to the source id, and is **identical in the envelope of every sibling** of that logical rack; different logical racks get different ids. The seam receives the composed envelope and does not restamp it.
- **Names.** The family policy gives a unique effective name; PREPARE-R computes the expected effective names under the lock (deterministic for the current block-table state) and records them; the seam returns the effective name it used, and **the sealed list uses the effective names**. A name that the sanitizer reduces to empty is a failure (AUTH-15 finding F-2).

### 15.6 Placement and transform contract

The orchestrator supplies the computed common transform (RACKMIRROR reflection, `mu_k`). SL-4 accepts only a transform with **strictly positive determinant** and scale within the kind's declared tolerance (no negative scale, ADR-0036); sets layer and properties from the plan; applies dynamic properties and `RecordGraphicsModified` **inside** `T_M`; and creates the reference in the target space in the caller's transaction. No jig, no interaction, no post-commit graphics writes.

### 15.7 Fresh reread and comparator support

SL-6 enumerates siblings with the existing `RackBlockFinder.ScanEnvelopes` (which skips layouts, anonymous and xref records) and reads payloads with `RackBlockData.Read`. It compares **expected versus persisted** with a **new comparator independent of the writer and of `mu_k`** that uses the typed canonical form of the Selective design document. `SelectiveAuthoredAuthority.Resolve` may be used as a **secondary** sibling-consistency check only. Completeness of the scan is attested by comparing the enumeration with the sealed list, not by trusting the scan.

### 15.8 Implementation prerequisites (seams that would need future implementation)

| Id | Prerequisite |
|---|---|
| P-1 | in-transaction forms of the frontal and planta creators (a refactor of the `SystemBlockWriter` / `ViewBlockDraw` path; precedent: `RedefineInTransaction`, `PrepareRedraw`) |
| P-2 | if the scope ever includes the lateral: the SL-3 contract, implemented by AUTH-15 only after EQ-1..EQ-5 (not required by the sealed scope V1) |
| P-3 | in-transaction, non-jig reference placement (SL-4) |
| P-4 | importer with a structured result and a fresh-acquire library path (SL-5); today it swallows failures |
| P-5 | static guards: no `Commit`, `Abort`, `Dispose`, `LockDocument`, `OpenCloseTransaction` or side-database write in the seams, and removal of `ReadBlockName` from the mirror path |
| P-6 | the read-only verifier and the expected-versus-persisted comparator (SL-6) |
| P-7 | the fingerprint provider (section 28) |
| P-8 | **host evidence** of the seams' rollback, in the AUTH-15 style (document, held lock, `StartTransaction`, abort without commit; side databases as characterization only): gate `BEFORE_REPLACEMENT_FREEZE` |

### 15.9 Open design decisions

| Id | Decision |
|---|---|
| OD-1 | **closed at contract level by section 25** (sealed Selective view-family scope, derived from V17); pending Coordinator and Architect ratification (`NB-1`) |
| OD-2 | replaced by the equivalence requirements EQ-1..EQ-5 (15.2) |
| OD-3 | the scope of the host rollback evidence P-8 (which families and which failure injections): the families are those of the sealed scope V1 |

### 15.10 Status

```text
SELECTIVE_AUTHORITY_DESIGN_STATUS = COMPLETE_FOR_BASELINE   (DRAFT_V2_1; pending agreement)
B5 = ADVANCES_TO_IMPLEMENTATION_PREREQUISITE
     + REMAINS_BLOCKER_FOR_CT21D_EXECUTION   (implementation and integration of P-1, P-3..P-7)
     + REMAINS_BLOCKER_FOR_REPLACEMENT_FREEZE (host evidence P-8)
```

Every clause required by the gate is specified. Nothing is BLOCKED at design level. The seams do not exist and are implementation prerequisites (section 23, EXEC-3). If the reviewers do not agree the design, B5 reverts to `REMAINS_BLOCKER_FOR_CT21D_DESIGN`.

## 16. UNDO EVIDENCE PLAN (B6)

```text
UNDO_EVIDENCE_STATUS = NOT_EXECUTED        B6 = REMAINS_BLOCKER_FOR_ACT2
```

This section designs the **plan**; no evidence is produced. UNDO is driven by the harness through the host's UNDO command inside the same governed session. Verification is **always by state reads** (the ABORT-VERIFY read-set, section 11.1); **no result depends on the callbacks that `04NO-S` left UNKNOWN** (`modifyUndone`, `modified`, `closed`).

| Group | Scenario | To be shown |
|---|---|---|
| U-1 | UNDO of the PREPARE-W imports alone (a run that imports and then refuses) | the number of UNDO steps the imports create; whether one UNDO removes them; the resulting state versus the record |
| U-2 | UNDO after O3 SUCCESS | the number of UNDO steps needed to reach the pre-command state; whether the state equals the `PRE_COMMAND_STATE_RECORD` (with or without the PREPARE residue) |
| U-3 | UNDO after O4 (a divergence produced by an adversarial writer) | the same, and whether the divergent element is also reverted |
| U-4 | UNDO after O5 (an induced incomplete verification) | the same |
| U-5 | UNDO after O7 (an induced control failure) | the same |
| U-6 | applicable O6 cases (state that cannot be confirmed after abort) | whether UNDO recovers the expected state. The **indeterminate-`Commit()` path** (V4 1.2) is exercised **only by the simulated adapter at T1**: a real host `Commit()` exception cannot be induced at will |
| U-6b | (optional) attempt `Commit()` against an **already-ended transaction** in the real host | the product's exception-path handling with a **genuine host exception**. It is **real host exception-path handling evidence only**: it is **not** evidence of an indeterminate `Commit()` and **not** a simulation of a partial `Commit()`. The evidence **records the exact origin of the exception**: `NATIVE_HOST_API`, `MANAGED_WRAPPER`, or `OTHER` (stated exactly); an exception raised by the managed wrapper is labelled as such and is not presented as a host exception |
| U-7 | interplay with PREPARE residue | whether UNDO removes the imported definitions together with or separately from the MUTATION |
| U-8 | independence from callbacks | every comparison uses state reads only |

**Disclosure carried to Owner Act 2 (section 24).** `HOST_EVIDENCE_FOR_INDETERMINATE_COMMIT = NOT_AVAILABLE` unless a future authority changes that fact. T1 tests exercise the decision and evaluator path; **T1 is not host evidence**.

**Possible findings and the recovery wording they would permit** (the wording is fixed at the replacement Freeze, not here):

| Finding | Permitted statement |
|---|---|
| one UNDO returns the state to the record for all applicable outcomes | may be described as "UNDO restores the pre-command state" for those outcomes |
| N UNDO steps, or residue remains | the exact step count and residue are stated |
| any outcome not recoverable | guidance stays `NOT A GUARANTEED RECOVERY PATH` for that outcome |

Acceptance is not a pass mark: the results are what the Freeze wording and Owner Act 2 rely on. SAVE/reopen equality is a separate group `S1` (SAVE, close, reopen, compare with the authoritative reader).

## 17. OUTCOME EVALUATOR

The evaluator runs in the **control plane, offline, from the raw log** (I-16). The outcomes O1..O7 and their precedence are those of V3 6, V4 1.3 and V5 3; they are not changed.

### 17.1 Inputs

`CTL_ENTRY` control verdicts at P0 and PL (per control PASS, FAIL, UNKNOWN); `PREP` PREPARE-R and PREPARE-W status and manifest; `TM` state of `T_M` (`ABORTED`, `FAILED`, `COMMIT_ACCEPTED`, or `NOT_OPENED`); `CONT_PV` and `CONT_PC` continuity verdicts (class A, class B); `OBS1`, `OBS2` per-element observations (equal, differs, incomplete or UNKNOWN); `VDEF` verifier-defect flag; `ABV` ABORT-VERIFY result (match, mismatch, unreadable); `EVT_C` phase-C write events on protected objects; log completeness flags; run status (section 21.1).

### 17.2 Deterministic table (first matching row applies)

| # | Condition | Primary outcome | Secondary findings recorded |
|---|---|---|---|
| 1 | the run is `INVALID`, or the log is incomplete | **no governing outcome.** A `PROVISIONAL_NON_GOVERNING_OUTCOME` is computed from the available evidence by rows 2..8 (or `UNDETERMINED` if none applies), **retained**, and **never promoted** to a governing result | the reason; the facts and states observed (section 21.1) |
| 2 | **before any product write**: any entry control FAIL or UNKNOWN; **a PREPARE-R FAIL or UNKNOWN**; a rejection known from PREPARE-R inputs; or **a mismatch in the re-observation immediately before the first import** | O1 | the failing controls or the mismatch |
| 3 | `TM` = `NOT_OPENED` after a PREPARE-W write, or `TM` = `ABORTED` or `FAILED` (including a `Commit()` exception), and `ABV` = match | O2 | `COMMIT_EXCEPTION_OR_INDETERMINATE` when applicable; the residue manifest |
| 4 | same as 3 and `ABV` = mismatch or unreadable | O6 | what was observed |
| 5 | `TM` = `COMMIT_ACCEPTED` and any `OBS1` or `OBS2` element **differs** from expected (including sealed-list violations) | O4 | class-A and class-B control failures; `VDEF` |
| 6 | `TM` = `COMMIT_ACCEPTED` and (any `OBS` incomplete or UNKNOWN, or `VDEF`) | O5 | control failures |
| 7 | `TM` = `COMMIT_ACCEPTED`, all `OBS1` and `OBS2` equal, and any mandatory control in `CONT_PC` FAIL or UNKNOWN, or any `EVT_C` on protected objects | O7 | the control ids |
| 8 | `TM` = `COMMIT_ACCEPTED`, all `OBS` equal, all mandatory controls PASS, `EVT_C` empty, no `VDEF` | O3 SUCCESS | non-normative diagnostics only |

**Missing evidence:** a missing semantic observation is incomplete (row 6); a missing control record is that control UNKNOWN (row 7); a missing ABORT-VERIFY read is unreadable (row 4); a missing entry record is UNKNOWN (row 2). **Pre-commit continuity** failures at PV abort (rows 3 or 4). **No cause is attributed** in any report. An evaluator defect is detected by evaluating the table against the scenario's declared expected outcome and by the conformance test on synthetic logs (EXEC-10).

## 18. RESULT / LOG SCHEMA (conceptual)

### 18.1 Run log

An append-only event log (one JSON record per line): `runId`, `sequence`, `tick`, `utc`, `thread`, `phase`, `checkpoint`, `recordType` (`ENTRY`, `TUPLE`, `CONTROL`, `SNAPSHOT`, `EVENT`, `OBSERVATION`, `LOCK`, `TXCOUNT`, `LIBRARY`, `PREPARE`, `IMPORT`, `MUTATION`, `COMMIT`, `ABORT`, `VERIFY`, `ABORT_VERIFY`, `OUTCOME`, `CLEANUP`, `FINISH`) and a typed payload. A record with a missing sequence number marks a gap.

### 18.2 Evidence package (future)

| Part | Content |
|---|---|
| identity | contract id, contract hash, scenario id, run id, group, mode (`LEARNING`, `VALIDATION`, `CHARACTERIZATION_RUN`), event-model version and hash, fingerprint-specification version, sealed-scope version, control-class registry and coverage-matrix versions, manifest hash and custody record |
| tuple | section 2 fields with hashes |
| qualification | the instrument qualification records (section 4.3) for the exact build |
| checkpoints | records of section 5 |
| observations | pass 1 and pass 2 values per element; `PRE_COMMAND_STATE_RECORD`; `PREPARE_RESIDUE_MANIFEST`; ABORT-VERIFY reads |
| events | E-11M and E-12 logs |
| outcomes | raw runner classification, canonical result, primary and secondary findings; for a non-governing run the `PROVISIONAL_NON_GOVERNING_OUTCOME` |
| attempt log | the per-scenario attempt log (section 21.3) |
| rulings | `CLASSIFICATION_RULING` records and reviewed amendments that apply |
| hashes | of every artifact and of the log |
| timestamps | start, checkpoints, finish |
| cleanup | section 20 record |
| owner-touch | statement and process list snapshots |
| failures | crashes, exceptions, instrument UNKNOWNs |
| raw versus canonical | the tooling's label is preserved unchanged; the canonical result is the ruling; a tooling label is never authority |

**No evidence is generated by this document.** The schema is conceptual; a concrete schema is fixed at the baseline.

## 19. FRESHNESS / IDENTITY RULES

### 19.1 Library acquisition: the per-run authoritative private library copy (TBD-10, SC-10, RC-5)

**Facts (F-5).** The current cache is keyed on path, last-write time and length, has **no content hash**, and replaces its database when the key changes. The importer returns 0 silently on failure. Freshness therefore **cannot be established from the cache alone** today.

**Why "hash, invalidate, acquire, hash again" is insufficient.** It does not exclude a file that changes and is restored between the two hashes.

**One authority: the per-run authoritative private library copy.** This is the **only** authoritative import source. It is one concept, used identically in section 20.

| Step | Rule |
|---|---|
| 1 | acquire the original source bytes **once** into a **private, run-scoped copy** (the source is opened for reading with write access denied to others when the host allows it), streaming the bytes and computing the SHA-256 **over the same bytes** as they are copied |
| 2 | capture the **source stat and hash necessary to detect a torn acquisition**: size and last-write time (and, when cheap, a source hash) before and after the streaming; a difference means the acquisition is `UNKNOWN` and the run is refused (O1) |
| 3 | record in the `LibraryInputRecord`: normalized source path, source stat, the SHA-256 of the **copied bytes**, the copy path and the generation id |
| 4 | open the library `Database` **from the private copy**, fully read into memory, never from the source path |
| 5 | **retain the private copy against writes** (a handle that denies write and delete) for the whole run, and key any cache entry by the **copy's hash** and a generation id incremented at every fill; invalidate explicitly in PREPARE-R before the acquire |
| 6 | **SL-5 receives the opened `Database`, not a path to the original source** |

The recorded hash therefore describes the exact bytes the `Database` was read from. **Atomicity is not claimed beyond this path.**

**Authority rule.** **Any reread of the original source for import makes the authority void: `AUTHORITY_INVALID`.** The run then cannot be governing (INVALID, with a mandatory ruling); it is recorded as a `PRODUCT_OR_SEAM_DEFECT` finding, and the `LibraryInputRecord` cannot support a `PASS`.

**Source drift policy (explicit choice).** Drift of the original source **after** the authoritative acquisition is **informational**: it is recorded as the finding `SOURCE_DRIFT_AFTER_ACQUISITION` and **does not stop the import**. Justification: after capture the source is no longer authoritative, and gating the run on a non-authoritative file would create false rejections without adding protection. The conservative safeguards that remain are: the torn-acquisition bracket (step 2); the re-hash of the **copy** immediately before each import (a mismatch stops the import: O2 with the residue produced so far); the retention of the copy against writes; the post-run re-hash of the copy; the `SOURCE_SWAP_CONTROL` (4.3.1); and the disclosure of any drift in the Owner Act 2 evidence.

**Alternatives** (only through a reviewed amendment): **A** — the source held against writes for the entire acquisition interval; **C** — an explicit **residual** when no authority can guarantee same content (hash bracketing only), disclosed in Owner Act 2.

| Element | Rule |
|---|---|
| UNKNOWN | the file cannot be hashed or copied (locked, denied); the acquisition bracket differs; the copy hash differs from the record; the acquire returns nothing or throws; the importer returns without a structured success; an import result not confirmed by a re-query and a fingerprint |
| qualification | `SOURCE_SWAP_CONTROL` (4.3.1) |
| status | designed here; **implemented before execution** (`BEFORE_CT21D_EXECUTION`, EXEC-6) |

### 19.2 Fingerprint method

`FINGERPRINT_METHOD = TO_BE_FIXED_BY_CT21D_KIND_AUTHORITY_BASELINE`, gate `BEFORE_CT21D_BASELINE` (V5 W-4). It is designed as the normative artifact **FINGERPRINT SPECIFICATION** in section 28; the concrete instrument is an implementation requirement. `TBD-10` (`CACHE_FRESHNESS`, 19.1) is a separate item and is not merged with it.

### 19.3 Identity rules (all)

Handles are the database-stable identity; `ObjectId` values are session-bound and never persisted in evidence as identity. The identity of the top transaction is decided only by the typed live authority `IsTopTransaction(myTransaction)` (4.2); no native pointer is persisted as an identity. Files are identified by normalized full path plus SHA-256 computed in the run.

## 20. CLEANUP

| Item | Requirement |
|---|---|
| process | a **fresh AutoCAD process per run**; the launcher verifies that no `acad.exe` exists before launch; after the run the process is terminated by the control plane (or exits) and its absence is verified |
| drawing | a fresh byte copy of the scratch template per run, hash recorded; it is never saved except by a scenario that designates a save copy; after the run the scratch file hash is recorded (unchanged unless the scenario saves) |
| library and cache | the in-process cache dies with the process; the **per-run authoritative private library copy** (19.1) is retained against writes until the end of the run, its hash is re-verified after the run, and it is then archived with the evidence or removed |
| profile | `SECURELOAD`, `TRUSTEDPATHS` and other profile values are read before and after; no change; the product and the harness never write them |
| leftovers | temp files, lock files and extra processes checked; the list recorded |
| evidence | every artifact hash-sealed; the seal recorded |
| verification | a `CLEANUP` record with each check; an incomplete cleanup makes the run INVALID (section 21.1) |

## 21. FAIL-CLOSED RULES, GOVERNING RESULTS, RETRY, NO-TOUCH

### 21.1 Run status

Status is evaluated per **run** (one scenario in one process). CT-DA rules are **not copied**; these are designed for CT-21D.

| Status | Conditions |
|---|---|
| **GOVERNING** | tuple equal to the baseline; instruments qualified on the exact build; log complete with no sequence gap and sealed; all required subscriptions intact for the whole run; correct binaries; no Owner touch; no process contamination; normal termination by the control plane; cleanup verified; run id unique and recorded in the attempt log, not an unauthorized retry; scenario equal to the catalog entry with its expected result fixed **before** the run |
| **NON_GOVERNING** | a valid execution that by design does not count toward acceptance: dry runs, rehearsal runs, LEARNING runs when used as validation, runs of an exploratory scenario. Their data stays evidence of behaviour |
| **INVALID** | tuple mismatch; missing or unreadable log or sequence gap; lost subscription; wrong binary; actual Owner manual input; another process interfering with the governed AutoCAD run; crash; incomplete cleanup; unauthorized retry; an `AUTHORITY_INVALID` library import (19.1); a run in Core Console; evidence not sealed; a control-plane watchdog expiry |

**Crash.** A crash is recorded with an explicit status: `CRASH_FINDING` when it occurs in code under test or its cause is unknown, `CRASH_ENVIRONMENT` when a ruling classifies it so. A crash is never silently INVALID: it needs a Coordinator ruling.

### 21.1.1 Retention of evidence (SC-6)

- **`INVALID` cannot support a `PASS`.** It may not erase product evidence.
- **All observed facts and logs remain retained**, even when the run is not governing.
- **If a product mutation occurred before the run became invalid**, the product state, the observed facts and the logs are preserved, and a **`PROVISIONAL_NON_GOVERNING_OUTCOME`** is computed from the available evidence (section 17.2 row 1). It is **never promoted** to a governing result.
- **A negative finding** (a provisional outcome different from the scenario's declared expected result, or a product defect observed) requires a **Coordinator ruling** on whether a governing confirmation is required. It cannot be ignored because the run was invalid.
- A crash after a product mutation preserves the state facts observed before it.

### 21.2 Group verdicts

Defined in section 1.3 and mapped to the predicate in sections 1.4 to 1.6.

### 21.3 Retry policy and attempt log (SC-7; a policy to be frozen at baseline; **no retry is authorized now**)

**Attempt log (mandatory).** For every scenario the control plane keeps a log with, for each attempt: attempt number; tuple; contract, event-model and fingerprint-specification versions; governing, non-governing or invalid state; outcome and verdict; and the reason for the retry or for stopping. **No cherry-picking:** every attempt is listed, every governing attempt counts, invalid attempts do not count but are disclosed.

| Case | Policy |
|---|---|
| PASS | no retry; scheduled repeats required by the repetition policy are repeats, not retries |
| governing FAIL | **no retry**; the result stands |
| governing UNKNOWN | **no retry** unless a future reviewed ruling changes its category (section 1.5) |
| INVALID | retry **only by Coordinator authorization** per occurrence, with a new run id and a recorded cause (tooling, environment or contamination) |
| process crash | a ruling is required (section 21.1); no automatic retry |
| any case | a governing FAIL or UNKNOWN is never retried to obtain a better result |

**Every authorized retry** runs the **same scenario, the same contract, the same event model and the same tuple requirements.** Changing the scenario, the contract or the model is a **new version, not a retry.**

`MAX_ATTEMPTS` = `MUST_FIX_BEFORE_AUTHORITY_BASELINE` (section 21.7). No retry is authorized now.

### 21.4 Required groups (RC-1)

Every group has **one class**, taken from the `CONTROL_CLASS_REGISTRY` (section 30). Mixed groups of V2 are split. The control-to-group-to-scenario coverage matrix is section 31. A group whose class is `NOT_A_CONTROL` is a run-validity or evidence group (see the registry): its `PASS` means that the required governing runs exist and their evidence is complete per the scenario, and it never carries the guarantee.

| Group | Class | Content | Section |
|---|---|---|---|
| `Q` | NOT_A_CONTROL | instrument qualification (an instrument defect makes the instrument `UNRELIABLE`) | 4.3, 4.3.1 |
| `WU` | B | warm-up load record, membership in the manifest, `WARMUP_COVERAGE_CRITERION` (E-10 and E-11M baseline) | 13.1 |
| `ID-A` | A | E-01, E-02, E-05, E-07 (identity and host, interactive confirmation) | 2.4 |
| `ID-B` | B | E-13 | 2.4 |
| `E4` | A | E-04 admitted set | 3.3 |
| `LK` | A | E-03 lock | 3.4 |
| `E6` | A | E-06 counter, `IsTopTransaction`, controls E6-C1..E6-C8 | 4.2 |
| `E8` | A | E-08 read-set re-observation (S-4) | 5, 11.1 |
| `E9` | A | E-09 overrules | 2.4 |
| `E10` | B | E-10 | 12 |
| `E11M` | B | E-11M | 13.2 |
| `E12-V` | B | E-12 validation: phase A determinism, phase C fence, subscription | 14, 27 |
| `E12-L` | NOT_A_CONTROL | E-12 learning micro-scenarios (produce the event model; carry no verdict about the host) | 27 |
| `EA1`, `EA2` | B | assumptions A-1 and A-2 | 14.7 |
| `SC-A` | A | scan: semantic comparison S-2 and S-3 and designed detection at X1..X4 | 10.6 |
| `SC-B` | B | scan: E-12 phase C detection, channels and residual cells, X7 tail log | 10.6, 10.8 |
| `PP` | NOT_A_CONTROL | post-CP characterization (report only) | 10.7 |
| `PR-REC` | A | `PRE_COMMAND_STATE_RECORD` and static dependency closure (component of S-5) | 6, 11.3 |
| `PR-FRESH` | UNRESOLVED | library acquisition and freshness (TBD-10) | 19.1 |
| `PW` | A | PREPARE-W residue manifest with exact fingerprints (component of S-5) | 7, 11.3 |
| `PW-CLONE` | UNRESOLVED | verification of the non-replacing cloning mode (registry entry `CLONE_POLICY_VERIFIED`) | 7.1, 4.3.1 |
| `AB` | A | ABORT-VERIFY (S-5) | 11 |
| `KS` | A | Selective seams SL-1, SL-2, SL-4, SL-5, SL-6 (E-14 evidence under characterization; S-1..S-3 comparators) | 15 |
| `KR` | A (not in the predicate) | E-14 and E-15 recorded `UNDER_CHARACTERIZATION` (3.2) | 3.2 |
| `U-1..U-8`, `U-6b` | NOT_A_CONTROL | UNDO evidence | 16 |
| `S1` | NOT_A_CONTROL | SAVE/reopen equality evidence | 16 |
| `OU` | NOT_A_CONTROL | outcome evaluator scenarios O1..O7 (an evaluator defect makes I-16 `UNRELIABLE`) | 17 |
| `CL` | NOT_A_CONTROL | cleanup (an incomplete cleanup makes the run INVALID) | 20 |

### 21.5 Owner no-touch and automation

- **No-touch is required** during every governing run: no Owner mouse or keyboard input after launch.
- **Allowed automation:** the governed control plane (launcher, script-driven in-process harness commands, supervisor). It is part of the contract and recorded.
- **Contamination:** any manual Owner input; any other process interacting with the governed AutoCAD process (automation, message injection, another RackCad-loaded session); another `acad.exe` running; a modal dialog answered by hand.
- **Not contamination** (Coordinator ruling carried as a principle): unrelated automated windows that do not touch AutoCAD.
- **Other RackCad processes:** none may run during a governing run; the process list is snapshotted at start and end.

### 21.6 Fail-closed rules (summary)

Any UNKNOWN required reading => the control is UNKNOWN => the run outcome follows the table of section 17. Any missing record => UNKNOWN. Any instrument without a passed qualification => its group verdict cannot be `PASS`. Nothing is inferred from the absence of an event or from timing. No cause is attributed to an outcome without separate evidence. An unclassified governing UNKNOWN cannot become `TRUE`.

### 21.7 Parameter classification (WD-3; no numbers are chosen here)

| Parameter | Classification | Note |
|---|---|---|
| mapping of group verdicts to predicate states | **structure fixed** (section 1); the mapping is **not fully closed until the `CONTROL_CLASS_REGISTRY` has no `UNRESOLVED` mandatory control** (section 30, open questions of 32.3) | depends on RC-1 |
| repetitions per group | `MUST_FIX_BEFORE_AUTHORITY_BASELINE` | requires an explicit **statistical intent and justification**; counts are not invented here |
| determinism-validation repetitions | `MUST_FIX_BEFORE_AUTHORITY_BASELINE` | same requirement |
| clean governing runs for false-rejection measurement | `MUST_FIX_BEFORE_AUTHORITY_BASELINE` | the sample must support the rate reported to the Owner (an explicit intent, for example the precision the Owner needs); not invented here |
| `MAX_ATTEMPTS` | `MUST_FIX_BEFORE_AUTHORITY_BASELINE` | |
| machine-class label | `MUST_FIX_BEFORE_AUTHORITY_BASELINE` | part of the machine/profile-bound tuple |
| time budgets that turn a slow measurement into `UNKNOWN` (for example hashing) | **`SHOULD_BE_REMOVED` from the semantic verdict** | a control-plane watchdog making the run or instrument `INVALID` is operational and allowed |
| post-CP observation queue duration | `MAY_BE_OPERATIONAL_AT_EXECUTION` | informational only; recorded |

## 22. BASELINE REQUIREMENTS (SC-12, WD-4)

`CT21D_AUTHORITY_BASELINE_READY = TRUE` would mean **only** that the authority contract and **all pre-baseline artifacts are fixed and agreed**. It does **not** mean: instrumentation implemented; execution ready; CT-21D authorized; product admissible. `BASELINE_READY != EXECUTION_AUTHORIZED`.

It requires **all** of the following.

| Id | Requirement | Status |
|---|---|---|
| BASE-1 | this authority contract is complete and agreed (Coordinator and Architect) | not met (`DRAFT_V2_1`) |
| BASE-2 | the Selective kind authority design is complete and agreed (section 15) | designed; agreement pending |
| BASE-3 | E-04 criteria fixed (section 3.3) | fixed here; agreement pending |
| BASE-4 | `LOCK_MODE` candidate contract fixed (section 3.4) | fixed here; agreement pending |
| BASE-5 | event instrumentation designed (sections 13, 14) | designed here; agreement pending |
| BASE-6 | abort read-set fixed (section 11.1) | fixed here; components without reader listed; agreement pending |
| BASE-7 | composition and reuse rules fixed (section 11.3) | fixed here; agreement pending |
| BASE-8 | UNDO evidence plan fixed (section 16) | fixed here; agreement pending |
| BASE-9 | governing-result, retry and attempt-log policies fixed (section 21) | fixed here; the `MUST_FIX` parameters pending |
| BASE-10 | every `BEFORE_CT21D_BASELINE` item resolved: TBD-01 criteria, TBD-03 candidate set, TBD-08 authority design, TBD-13 concrete reads, `FINGERPRINT_METHOD`, composition rule | designed here; agreement pending |
| BASE-11 | baseline hashes recorded (contract, control-class registry, coverage matrix, scenario catalog, fingerprint specification, event-model procedure and schema, sealed scope and authority comparison, manifest procedure, warm-up definition) | pending |
| BASE-12 | **OD-1 closed**: the sealed Selective view-family scope ratified (section 25) | `NB-1` open |
| BASE-13 | **scenario catalog** complete with the expected result of every scenario and its authority (section 26) | `NB-2` open |
| BASE-14 | **FINGERPRINT SPECIFICATION** agreed (section 28) | `NB-3` open |
| BASE-15 | **E-12 event-model procedure and learning-corpus definition** agreed (section 27) | `NB-4` open |
| BASE-16 | **instrument qualification acceptance criteria** agreed (section 4.3) | designed here; agreement pending |
| BASE-17 | **manifest capture and custody procedure** agreed (section 29) | `NB-6` open |
| BASE-18 | the **classified parameters** fixed (section 21.7): every `MUST_FIX_BEFORE_AUTHORITY_BASELINE` value, with its justification | pending |
| BASE-19 | **`CONTROL_CLASS_REGISTRY` agreed**, with no `UNRESOLVED` mandatory control (section 30) | registry drafted; `UNRESOLVED` entries and questions open |
| BASE-20 | **control-to-group-to-scenario coverage matrix agreed** (section 31) | matrix drafted; agreement pending |

```text
CT21D_AUTHORITY_BASELINE_READY = FALSE
```

## 23. EXECUTION PREREQUISITES (WD-5)

`CT21D_EXECUTION_READY = TRUE` requires **all** of the following, and is **separate** from baseline readiness.

| Id | Prerequisite | Status |
|---|---|---|
| EXEC-1 | `CT21D_AUTHORITY_BASELINE_READY = TRUE` | not met |
| EXEC-2 | implementation of the required instrumentation I-01..I-16 (all "DOES NOT EXIST" items in section 4.1) | not met (`NB-5`) |
| EXEC-3 | Selective caller-owned seams (SL-1, SL-2, SL-4, SL-5, SL-6; SL-3 only if the scope changes) **implemented and integrated** in `src/` under a separate, separately governed workflow (section 15.8) | not met |
| EXEC-4 | the abort read-set components without readers or comparators (section 11.1) implemented | not met |
| EXEC-5 | exact build and exact package: source SHA, binaries, control plane, scenario catalog, fingerprint specification, event model, manifest; hashes recorded | not met |
| EXEC-6 | resolved `BEFORE_CT21D_EXECUTION` items: TBD-02 (E-06 counter), TBD-04 (instrument), TBD-05, TBD-06 (instrumentation), TBD-10 (freshness, path B), plus the assumption characterization designs A-1, A-2 | designed here; instruments not built |
| EXEC-7 | Coordinator authorization of execution, including the trusted-path and Owner no-touch preconditions | not met |
| EXEC-8 | any Architect review required by the workflow | not met |
| **EXEC-9** | **instrument qualification PASS on the exact build** (section 4.3), before any governing run | not met |
| **EXEC-10** | **evaluator conformance** against synthetic T0/T1 scenarios that reproduce every row of section 17.2 | not met |
| **EXEC-11** | the **manifest captured** for the exact build and machine under the custody procedure (section 29) | not met |
| EXEC-12 | **static guards P-5 active and passing in CI**: no `Commit`, `Abort`, `Dispose`, `LockDocument`, `OpenCloseTransaction` or side-database write in the seams, and `ReadBlockName` removed from the mirror path (section 15.8) | not met |

```text
CT21D_EXECUTION_READY = FALSE        CT21D_EXECUTION = NOT_AUTHORIZED
```

## 24. OWNER ACT 2 EVIDENCE REQUIREMENTS

Before the Owner is asked for final acceptance, the following must exist and have been reviewed (no later than the replacement Freeze):

1. admission-evaluation results for the characterized tuple: the group verdicts and the `ALT21D_HOST_PASS` state (which controls are measurable, which are not, and any reviewed amendments);
2. the residual scan characterization (section 10.6), including missed transient changes, detection by channel, the false terminal failure rate, the duration of W-scan, A-1 and A-2;
3. false-rejection behaviour (availability cost) from baseline runs;
4. UNDO evidence (section 16) and SAVE/reopen equality (group `S1`);
5. Selective rollback host evidence (document-authority evidence of the AUTH-15 kind for the Selective seams);
6. interactive-host confirmation (section 2.4);
7. post-CP characterization (section 10.7);
8. limits and non-guarantees restated (V5 `NG-01..NG-20`, W-scan residual, W3);
9. every reviewed amendment made after Owner Act 1;
10. the replacement Freeze draft;
11. **disclosures:** `T1` tests exercise the evaluator and decision path, and **T1 is not host evidence**; `HOST_EVIDENCE_FOR_INDETERMINATE_COMMIT = NOT_AVAILABLE` unless a later authority adds it; `U-6b`, if used, covers **only real host exception-path handling** and states the origin of the exception;
12. **`REUSED_EXTERNAL_GEOMETRY_NOT_VERIFIED`**: the guarantee does not cover the semantic correctness of a reused pre-existing library-derived definition or dependency (11.3);
13. **X7**: the interval between V1 and the PC evaluation is outside E-12 phase C; the `POST_V1_TO_CP_QUEUE_LOG` reports what was delivered there, and no fence extension exists without a reviewed amendment and an Owner disclosure (10.8);
14. **`SOURCE_DRIFT_AFTER_ACQUISITION`** findings, if any (19.1);
15. the **warm-up residual**: code paths that can lazy-load only during a real document mutation (13.1).

No decision is prepared here.

## 25. SEALED SELECTIVE VIEW-FAMILY SCOPE AND AUTHORITY COMPARISON (NB-1, OD-1, SC-4, RC-13)

**Artifact `SELECTIVE_VIEW_FAMILY_SCOPE_V1` — DEFINED and COMPARED against the effective authorities; NOT YET SEALED.** The family list is **not inferred from family names**: this section compares each scope row against the effective authorities listed in 25.1, row by row. The comparison read the documents of this branch and the files of `main` (`3375aadb`) without modifying any of them.

### 25.1 Authorities compared

| Authority | Identity | Role in the comparison |
|---|---|---|
| Proposal V17 | blob `dd944d8e9c62b083eebc2b1292789977b880c368`; sections 1.3, 2.1, 2.2, 3.7, 3.8, limitation rows L-01, L-11..L-14 | historical agreed authority; governs whatever V18 does not expressly replace (Freeze V18 section 3) |
| Freeze V18 | blob `c12fa65f5b6203061e8d717d1ceed595c4fed1f4`; sections 3 (precedence), 7 (preserved V17 authorities), 10 (O-1 and ADR-0036) | effective precedence and preserved exposure |
| Freeze V17 post-I-57 | blob `5f4933a3aa90d7052f571317107f1831ec02ac6e` | `HISTORICAL / IMMUTABLE / SUPERSEDED ONLY FOR AFFECTED CT-49 CONTRACT` (Freeze V18 section 2); read for exposure statements only |
| ADR-0036 | current blob `5628947673f2afddf141c0505e92283962459443` (`PROPOSED / AMENDED FOR V18`); blob frozen by V17 `ce5f6fae935c7acdc347e12a7d465bb438923549`; decision 4 and the accepted negative consequences | both blobs carry the same exposure statements |
| Foundation reconciliation | blob `89a21b6f9dc07e72472f3e34598e98049a3e1fb7`; AUTH-04 and section 4 | consumption of `RackViewAvailability`; states that I-52 keeps the exposure of views |
| `RackViewAvailability` (facts) | `main` `3375aadb`, `src/RackCad.Application/Systems/Shared/RackViewAvailability.cs`, blob `74e1a1aba691e42e03d3744bf6631e444aa1d696`; class `SelectiveViewAvailabilityFacts` | availability facts of the current code (**availability is not exposure**) |
| `SelectiveDepthLayout` | `main` `3375aadb`, `src/RackCad.Application/Systems/Selective/SelectiveDepthLayout.cs`, blob `9e45f2f2c90bcd2ac881d4fc2dc3db2debfcd762`; `Count(system) = Math.Max(1, system.DepthCount)` | number of fondos |
| Selective commands | `main` `3375aadb`, `src/RackCad.Plugin/RackSelectivoCommands.cs`, blob `a22e6a6a4dba2ee2b62f2f47f358d9ac975fc486`; `fondoCount = SelectiveDepthLayout.Count(system)`; a frontal block without a fondo address is treated as fondo 0 | production reading of `View` and `Section` |

### 25.2 The scope

| Row | Domain | Status for CT-21D |
|---|---|---|
| `SEL-FRONTAL` | `View = frontal`, `Section = k` for `0 <= k < SelectiveDepthLayout.Count(system)` (one per fondo); catalog covers a single fondo and several fondos | **IN** |
| `SEL-PLANTA` | `View = planta` (canonical section `-1`) | **IN** |
| `SEL-LEGACY-M1` | frontal with the legacy `Section = -1` (or a legacy empty `View`) | **OUT_OF_SCOPE** for CT-21D scenarios (25.4) |
| `SEL-LATERAL` | lateral, per post | **OUT** (fail-closed, E5) |

The created sibling set of a logical rack is the **image of the selected source references** (each a frontal `k` or a planta). `A_k` (all admissible views) is a **pure plan-time commutation check** and creates nothing (V17 3.8).

### 25.3 Row-by-row comparison

`RESULT` is `IDENTICAL` (same statement), `NARROWER` (the CT-21D scope is a strict subset of what the authority permits), or `CONFLICT` (contradiction; **a conflict prevents sealing**). `NO_STATEMENT` marks an authority that does not address the row; it is not a conflict and is not counted as agreement.

| Scope row | Authority and section | Statement | RESULT |
|---|---|---|---|
| `SEL-FRONTAL` | V17 1.3, 2.1 | Selectivo inside as maximum envelope: frontal by fondo | IDENTICAL |
| `SEL-FRONTAL` | V17 3.7 | frontal, `Section = k`, `0 <= k < fondos`, exposes `mu_k` (RUN) | IDENTICAL |
| `SEL-FRONTAL` | V17 3.8 | `A_k` = frontal of each fondo `k` with `0 <= k < SelectiveDepthLayout.Count(system)` | IDENTICAL |
| `SEL-FRONTAL` | Freeze V18 7 | exposure and supported combinations inherited from V17 by reference; not replaced | IDENTICAL (by reference) |
| `SEL-FRONTAL` | ADR-0036 decision 4 | first cut: frontal and planta for Selectivo | IDENTICAL |
| `SEL-FRONTAL` | Foundation reconciliation section 4 | I-52 keeps the exposure of views and supported combinations; Foundation facts feed but do not replace them | IDENTICAL (keeps V17) |
| `SEL-FRONTAL` | `main`: `SelectiveViewAvailabilityFacts`, `SelectiveDepthLayout.Count`, `RackSelectivoCommands` | availability for a fondo address is `Index < FondoCount`, and `fondoCount = SelectiveDepthLayout.Count(system)` | IDENTICAL |
| `SEL-PLANTA` | V17 1.3, 2.1, 3.7, 3.8 | planta inside; canonical section `-1`; `A_k` includes planta | IDENTICAL |
| `SEL-PLANTA` | Freeze V18 7; ADR-0036 decision 4; Foundation section 4 | same as above | IDENTICAL |
| `SEL-PLANTA` | `main`: `SelectiveViewAvailabilityFacts` | the whole-rack variant is available when the view kind is planta | IDENTICAL |
| `SEL-LATERAL` | V17 1.3 (L-01), 2.2 | laterals of Selectivo out (E5) | IDENTICAL |
| `SEL-LATERAL` | ADR-0036 accepted negative consequences | the first cut reflects no laterals | IDENTICAL |
| `SEL-LATERAL` | `main`: `SelectiveViewAvailabilityFacts` | a post variant is **available** when the post index exists | NARROWER (the scope excludes what availability allows; consistent with V17 3.8, where `A_k` = availability intersected with exposure) |
| `SEL-LEGACY-M1` | V17 3.7 | legacy `-1` is exposed and canonicalized to fondo 0 | NARROWER (CT-21D does not exercise it) |
| `SEL-LEGACY-M1` | ADR-0036 decision 4 | the Selective frontal with `-1` becomes fondo 0 | NARROWER |
| `SEL-LEGACY-M1` | `main`: `RackSelectivoCommands` | a frontal block without a fondo address is handled as fondo 0 | NARROWER |
| rack-level conditions | V17 1.3 rows L-11 (corner fondos), L-12 (linked half-frentes), L-13 (topes), L-14 (side protector, deflector, grate or pallets overflowing) | those racks fail closed | IDENTICAL (inherited; CT-21D fixtures avoid them, 25.6) |
| rack-level conditions | ADR-0036 accepted negative consequences | Selectivos with topes fail closed until a new decision | IDENTICAL |
| `A_k` provisional flag | V17 3.7, 3.8 headers | "PROVISIONAL UNTIL CROSS-INITIATIVE RECONCILIATION" | resolved: Foundation reconciliation section 4 keeps the exposure with I-52 and Freeze V18 section 7 inherits it; **no material change** found |
| ownership of exposure | Freeze V17 post-I-57 | historical; superseded only for the affected CT-49 contract | NO_STATEMENT superseding the Selective view exposure |

**Result: no `CONFLICT` was found.** The comparison is a textual and behavioural comparison of the listed authorities at the listed identities; it is **not** an audit of the whole product.

### 25.4 Decision on `Section = -1`

```text
Section = -1 (legacy frontal, or legacy empty View)  =  OUT_OF_SCOPE for CT-21D scenarios
```

Reason: production canonicalizes it to fondo 0 **before** planning, so the seams receive and create canonical output (`Section = k`); the plan-time reading of a legacy source is not part of the mutation whose rollback CT-21D characterizes. Catalog fixtures therefore use canonical `Section = k`. This **narrows** CT-21D only; it neither widens nor narrows the product envelope. A decision to characterize the legacy reading needs a new scenario version and a reviewed amendment of this section.

### 25.5 Relation to O-1 and to the envelope

Freeze V18 section 10 records `O1_DISPOSITION = REQUIRES_REDECISION` and `HISTORICAL O-1 = HISTORICAL ONLY`. Sealing this scope **does not supersede O-1, does not decide it, and does not exceed the Freeze V18 / V17 envelope**: it selects a subset of that envelope for a **characterization**. A future Owner redecision of O-1 that changes the Selective envelope reopens this section.

### 25.6 Rack-level fail-closed conditions inherited by reference

V17 limitations L-11, L-12, L-13, L-14, L-20 to L-26 and L-29 to L-33 apply **per rack** at plan time. CT-21D fixtures are racks **inside** the maximum envelope: no topes, no corner fondos, no linked half-frentes, no side protector or asymmetric deflector, no overflowing grate or pallets, and every other listed condition absent. Concrete designs are catalog content (section 26). The V17 note on the effective library (L-28) applies.

### 25.7 What the scope governs

| Artifact | Effect of the scope |
|---|---|
| sealed list (10.2) | one definition and one reference per selected view; envelopes; addresses; grouping |
| protected objects (10.3) | the elements of those views, their containers and the read-set |
| abort read-set (11.1) | definitions, references, envelopes, symbol records of those views |
| scenario catalog (26) | scenarios cover `SEL-FRONTAL` (single and several fondos) and `SEL-PLANTA`, and no lateral, no legacy `-1` |
| required seams (15.2) | SL-1, SL-2, SL-4, SL-5, SL-6; **SL-3 not required** |
| comparators and readers (15.7) | for frontal and planta payloads and references |

### 25.8 Closure action for NB-1 and what is still missing

The comparison found no conflict, so NB-1 **advances to baseline artifact preparation**; it stays a blocker until the artifact is sealed. What is missing, exactly:

1. **Ratification** by the Coordinator and the Architect of this comparison (25.3) and of the decision of 25.4;
2. **confirmation** that no authority after the compared identities changed the Selective view exposure. Known open item: `ADR-0036 = PROPOSED / AMENDED FOR V18` and Freeze V18 section 10 (`NEEDS_AMENDMENT`) leave the ADR for a separate gate; the amendment concerns the finite mode catalog and the session authority, not the view exposure, but that must be **confirmed when the amendment exists**;
3. **record** `SELECTIVE_VIEW_FAMILY_SCOPE_V1 = SEALED` with its hash; any later change reopens NB-1 and the dependent artifacts (25.7).


## 26. SCENARIO CATALOG SCHEMA (NB-2, RC-10)

The **scenario catalog** is the normative list of runs. Its **content is not fixed here**: it needs the ratified sealed scope (section 25), the seam operation inventory, the coverage matrix (section 31) and the kind authority baseline. **No scenario outcome is invented in this section.** The schema and the required content are fixed.

### 26.1 Fields of a scenario

| Field | Content |
|---|---|
| `SCENARIO_ID`, `SCENARIO_VERSION` | identity and version; a change of any expectation is a **new version** |
| `GROUP`, `PURPOSE` | the group it serves (21.4) and why |
| `MODE` | `LEARNING`, `VALIDATION`, `CHARACTERIZATION_RUN` or `DRY_RUN` (non-governing) |
| `TUPLE_REQUIREMENTS` | which tuple fields (section 2) must equal the baseline for the run to be governing |
| `PLAN_OR_INPUT` | the rack design(s) and selected views, by reference to fixtures inside the sealed scope |
| `FIXTURE` | the scratch template, private library copy and manifest, by hash |
| `DELIBERATE_VIOLATION_FLAG` | `YES` when the scenario deliberately violates a control |
| `TARGET_NEGATIVE_CONTROL` | the control the violation must be detected by (empty if the flag is `NO`) |
| `INJECTIONS` | writers (10.6) and positions, or none |
| `EXPECTED_PRODUCT_OUTCOME` | O1..O7, or none |
| `EXPECTED_CONTROL_RESULTS` | per control: PASS, FAIL or UNKNOWN |
| `EXPECTED_INSTRUMENT_STATES` | per instrument: `RELIABLE`, `UNRELIABLE` or `NOT_MEASURABLE` |
| `EXPECTED_GROUP_VERDICT` | the group verdict the scenario contributes (1.3) |
| `EXPECTED_EVENTS` / `EXPECTED_DETECTION_CHANNEL` | the event class expected per phase (E-12); for scan scenarios: semantic comparison, E-12, both, neither |
| `EXPECTED_LOG_RECORD_SET` | the record types and counts the run log must contain (18.1) |
| `EXPECTED_EVIDENCE_SET` | the artifacts the evidence package must contain (18.2) |
| `EXPECTED_CLEANUP` | the cleanup checks that must pass (20) |
| `RETRY_APPLICABILITY` | whether a retry may be authorized for this scenario and under which cause (21.3) |
| `REPETITION` | a reference to the classified parameter (21.7) |
| `CONTROL_CLASSES_EXERCISED` | the registry classes (section 30) the scenario exercises |
| `EVM_VERSION` | the event-model version pinned (27), or none |
| `AUTHORITY_SOURCE_FOR_EXPECTED_RESULT` | **mandatory**: the clause that determines the expected result |
| `SEALED_AT` | the hash of the expectation, recorded **before the first governing run** of the scenario |

### 26.2 Rules

- **An expected result must cite its authority**: a contract clause or evaluator row that determines it (for example "the injected violation at PV triggers row 3 of 17.2"), or `EVIDENCE_DERIVED` with the pinned artifact (the E-04 admitted set, the frozen event model). A result that no authority determines cannot be declared.
- **`EXPLORATORY_NON_GOVERNING`.** A scenario whose expected result lacks an authority is `EXPLORATORY_NON_GOVERNING`. It **may not be promoted after the fact**. Promotion requires a **new sealed scenario version** with a reviewed authority. Its results stay evidence of behaviour (21.1) and never count toward a `PASS`.
- **No post-hoc expectation.** An expected result is sealed before the first governing run of the scenario and is never filled from a run's output.
- An expected result that depends on an evidence-derived value is pinned to that value's version.

### 26.3 Required scenario classes (content to be enumerated at baseline)

| Class | Purpose |
|---|---|
| `CL-CLEAN` | clean runs per sealed family and plan class (control evaluation, false-rejection measurement) |
| `NC-<control>` | a deliberate violation of one control, to show its detection and outcome |
| `Q-<instrument>` | instrument qualification (section 4.3, 4.3.1) |
| `WU` | warm-up, load membership, `WARMUP_COVERAGE_CRITERION` (dry runs) |
| `E4`, `LK-<mode>`, `E6-C*`, `E10`, `E11M`, `E8`, `E9` | the measurability groups |
| `EV-L-<operation>` | learning micro-scenarios (section 27) |
| `EV-V-<plan>` | validation runs |
| `EA1`, `EA2` | assumption characterization |
| `WR-<writer>-<position>` | the scan matrix (10.6), including X7 with the tail log |
| `PR-*`, `PW-*` | closure, freshness, import, residue, cloning and failure scenarios |
| `AB-*` | abort verification |
| `KS-*` | Selective seam rollback and failure typing |
| `U-*`, `S1` | UNDO and SAVE/reopen |
| `OU-O1..O7` | evaluator scenarios producing each outcome |
| `CL-*` | cleanup |

`NB-2` remains open until the catalog **content**, with expected results and authorities, exists and is agreed. This schema alone does not close it.


## 27. E-12 EVENT-MODEL PROCEDURE, EVM ARTIFACT AND LEARNING CORPUS (NB-4, SC-5, RC-11)

### 27.1 Purpose and levels

The model `EVM-Selective-vX` is how `EXPECTED_MUTATION_EVENT_SET = f(plan)` is obtained. Levels (14.3): **contract** fixed before governing runs; **model** frozen after learning and review; **validation** governing. **The same run never teaches and validates.**

### 27.2 Procedure

| Step | Action |
|---|---|
| 1 | **seam operation inventory**: every database-mutating primitive that SL-1, SL-2, SL-4 and SL-5 use (append of an entity, creation of a block definition, entity modification, erasure, extension-dictionary creation, `Xrecord` data set, block-reference append, symbol-table record addition, dynamic-property set, `RecordGraphicsModified`, and any other explicit category the inventory shows). The inventory is a **starting input, not the completeness authority**; it must pass the independent completeness review (27.7). No primitive may be used that has no micro-scenario |
| 2 | **learning by micro-scenarios**: each operation category is executed **in isolation** in the controlled environment (manifest-attested, no adversarial probes) to map it to its host events; several isolated repetitions across independent sessions |
| 3 | **draft model**: entries as in the EVM schema (27.6) |
| 4 | **explanation review**: every entry carries an explanation of one of three types: `PRODUCT_OPERATION`; `HOST_DOCUMENTED` (with a citation); or `HOST_OBSERVED_STABLE`. **`HOST_OBSERVED_STABLE` may appear only as accompanying host behaviour associated with a product operation** (a companion event of that operation), must carry raw evidence, a citation to the isolated micro-scenarios that show it stable across independent sessions, an Architect review and a **risk label**; it is **never a free-standing permission**. **Unexplained events are recorded as `UNEXPLAINED_EVENTS` and block the freeze**; they are never admitted automatically |
| 5 | **freeze**: the model gets a version and a hash; status `DRAFT`, `UNDER_REVIEW` or `FROZEN(hash)`; only `FROZEN` models are used by governing validation |
| 6 | **validation**: governing runs on fresh sessions and fresh fixtures, with plans **structurally outside** the learning corpus (27.8), apply the frozen model |

### 27.3 Learning corpus definition

The corpus is the set of micro-scenarios, their fixtures and the sessions that ran them, **sealed by hash**. The **validation corpus** is disjoint (different sessions, fresh fixtures) and contains plans structurally outside the learning corpus (27.8), so that `f(plan)` is tested as a function and not as a lookup.

### 27.4 Determinism criterion

For the same plan, the multiset of `(event kind, selector, step)` is identical in all governing validation runs, with the declared partial order. The number of runs is a classified parameter (section 21.7). If it is not identical, MUTATION is **not deterministic**: the cause is `EMPIRICAL_BEHAVIOR_DIFFERENCE`, the verdict is class B (`AMENDMENT_PENDING`), and E-12 phase A may degrade to telemetry **only** by a reviewed amendment (V3 4.5).

### 27.5 Change control

Any change to the model, the write-event categories, the subscription set, the tuple, or seam code that affects events requires a **new model version, review and revalidation**. A run made under an older model version is not evidence for the newer one.

### 27.6 EVM artifact schema

The event model is a versioned artifact with these fields.

| Field | Content |
|---|---|
| `EVM_ID`, `VERSION`, `HASH` | identity of the model and the hash of its canonical serialization |
| `LEARNING_CORPUS_HASH` | the hash of the sealed learning corpus (27.3) |
| `ENTRY_ID` | identity of one entry |
| `OPERATION_CLASS` | the seam operation category of the inventory the entry belongs to |
| `EXPECTED_EVENT_SEQUENCE/SET` | the multiset of `(event kind, target selector, step)` with the multiplicity as a function of the declared plan quantities (27.9), and the partial order if any |
| `EXPLANATION_TYPE` | `PRODUCT_OPERATION`, `HOST_DOCUMENTED` or `HOST_OBSERVED_STABLE` (27.2 step 4) |
| `AUTHORITY/CITATION` | the product operation, or the host documentation citation, or the micro-scenario identities that show a companion behaviour |
| `RAW_EVIDENCE_REF` | reference to the raw event logs that support the entry |
| `RISK_LABEL` | `NONE`, or the declared risk of the entry (mandatory for `HOST_OBSERVED_STABLE`) |
| `REVIEW_STATUS` | `DRAFT`, `UNDER_REVIEW`, `ACCEPTED` or `REJECTED`, with the reviewer role |

### 27.7 Independent completeness review of the operation inventory (CQ-G)

The seam operation inventory is **the starting input**. Before the baseline it must pass an **`INDEPENDENT_COMPLETENESS_REVIEW`** made by a reviewer who did not compile the inventory, using at least two independent sources: (a) a static enumeration of the mutating primitives reachable from the seam entry points (SL-1, SL-2, SL-4, SL-5); and (b) a dynamic trace of dry runs on a scratch document (non-governing) that lists the primitives actually invoked. Every primitive found by either source must appear in the inventory with a micro-scenario, or be justified as unreachable. The review record is a baseline artifact.

### 27.8 `STRUCTURALLY_OUTSIDE_LEARNING_CORPUS`

The kind authority baseline declares the **plan quantity vector** `Q(plan)`: the integer quantities the multiplicities depend on (for example the number of fondos, the number of views, the entities per definition, the dynamic properties set) and the **shape class** of the plan (which view families it selects: frontal set, planta, or both). A validation plan `P` is **structurally outside** the learning corpus `C` when at least one of these holds:

- for some dimension `q` of `Q`, the value `Q_q(P)` is **not** in `{Q_q(p) : p in C}` (an unseen multiplicity, including one above the maximum seen);
- the **shape class** of `P` is not represented in `C`.

The validation corpus must contain at least one plan of each kind, where the sealed scope permits it.

### 27.9 `f(plan)`

`f(plan)` is defined as the multiset union, over the ordered list `Ops(plan)` of seam operation instances derived from the plan (SL-1 per selected frontal fondo, SL-2 for the planta, SL-4 per view, the envelope writes, and the PREPARE-independent operations inside `T_M`), of `E(op, Q(plan))`, where `E` is the EVM entry of the operation class and its multiplicity is a function of the declared quantities. Each multiplicity function must be either affine in the quantities or a table validated over structurally distinct shapes (27.8). The domain of adjudication is 14.8.

### 27.10 What the authority baseline requires

The baseline requires the **procedure** (27.2), the **corpus definition** (27.3, 27.8), the **EVM schema** (27.6) and the **independent completeness review** (27.7). It does **not** require the learned EVM content: the model entries are produced by authorized characterization execution later.

`NB-4` remains open until these baseline artifacts exist and are agreed.


## 28. FINGERPRINT SPECIFICATION (NB-3, ID-4, RC-7)

**`FINGERPRINT_SPEC_VERSION` is a tuple field.** A change of this specification is a change of the baseline. The specification has **two layers** with different jobs. The digest of either layer **never decides semantic equivalence**.

### 28.1 Layer 1: `EXACT_STATE_FINGERPRINT`

Digest algorithm: SHA-256 over a canonical byte stream that begins with the domain separator `FPSPEC-V1|EXACT|<record type>|`.

**Use.** Preserved-state comparisons: the `PRE_COMMAND_STATE_RECORD` against the state after an abort; pre-existing definitions and dependency records; `REUSED_BOUND` records; exact residue detection (the manifest additions); cross-session determinism of the provider.

**Rules.**
- **Numbers:** IEEE-754 binary64 **bit-exact**, encoded as 16 hexadecimal digits. **`-0.0` and `+0.0` differ.**
- **Non-finite values** (NaN, infinities) are **unsupported** in version 1: a record that contains one is `UNKNOWN`, never a digest.
- **Strings:** exact stored text encoded as UTF-8, **no normalization**.
- **Names and lookup:** existence and collision of a name are decided with **host symbol-table semantics** (11.3); the fingerprint records the **stored case** of the name, and two records are equal only if the stored names are equal.
- **Normative iteration order:**
  - an **ordered container** (the entity sequence of a block table record, which is draw order) is digested in the **host iteration order** as reported, because the order is part of the state;
  - an **unordered container** (symbol-table name sets, dictionary entries, sibling sets) is digested **sorted** by ordinal comparison of the stored name or canonical key;
  - property keys inside a record are in sorted ordinal order.
- **Unknown entities:** a proxy, a custom entity, or any type not listed in the specification makes the fingerprint **`UNKNOWN`**, never a partial digest.
- **Exclusions:** `ObjectId`; database-local handles (except in `ENUMERATION` within one session); timestamps; volatile local identifiers; reactors and other host-local pointers.

**Canonical schema by record type**

| Record type | Canonical content |
|---|---|
| `DEFINITION_DUMP` (block table record) | block name (stored case); origin; explodability; scaling; units; annotative flag; then, for each entity **in the record's iteration order**, its canonical entity record; nested definitions are referenced **by name** and have their own digest; dependency records by name |
| `ENTITY` | dxf class name; layer name; linetype name; color (indexed or true color, with the kind of source); lineweight; transparency; linetype scale; visibility; plus the geometry and properties of its type: line (endpoints), arc, circle, polyline (vertices, bulges, widths, closed), text and multi-text (string, height, rotation, style name, alignment), block reference (block name, position, scale factors, rotation, normal, dynamic property values), attribute definition, and any type the specification lists |
| `SYMBOL_RECORD` | table kind; stored name; the properties of the record (for a layer: color, linetype name, lineweight, on/off, frozen, locked, plot flag, transparency) |
| `PAYLOAD_EXACT` | the raw stored bytes of the RackCad payload and envelope |
| `NOD_ENTRY` | key; canonical value |
| `ENUMERATION` | the ordered or sorted list of names or handles of a container as above; **handle lists are valid only within one session** and are excluded from cross-session determinism |

### 28.2 Layer 2: `SEMANTIC_COMPARISON`

**Use.** Expected-versus-persisted product semantics: the pass-1 and pass-2 comparisons S-2 and S-3, the staged reads S-1, transforms, payload and domain equality, and the independent comparator of SL-6.

**Rules.**
- **Equivalence is decided by a typed domain comparator**, not by a digest. The comparator parses the persisted record with the **authoritative reader** into its typed form and compares it with the expected typed form computed by the oracle.
- **Tolerance and normalization are declared by type** in the kind authority baseline (a baseline artifact): coordinates and transform components with a declared tolerance and with `-0.0` equal to `+0.0`; angles with a declared normalization; sibling sets unordered; sequences whose order the plan determines (for example draw order) order-sensitive. Version 1 of this contract fixes **no numeric tolerance**; the values are a baseline artifact.
- An unrecognized schema version, an unsupported type, or an unparsable record is `UNKNOWN`.
- A `SEMANTIC_DIGEST` may exist for evidence indexing only; it never decides equivalence.
- **A signed-zero or other bit-level difference between domain-equivalent values is not a difference of this layer and must not produce O4.** It may be recorded as a secondary finding `BIT_LEVEL_DIFFERENCE_DOMAIN_EQUIVALENT`.

| Use | Layer |
|---|---|
| expected versus persisted (S-1, S-2, S-3) | `SEMANTIC_COMPARISON` |
| read-set re-observation E-08 (S-4), abort verification (S-5), `PRE_COMMAND_STATE_RECORD`, `PREPARE_RESIDUE_MANIFEST`, `REUSED_BOUND` | `EXACT_STATE_FINGERPRINT` |

### 28.3 Determinism and qualification

- **Determinism control (qualification of I-12):** identical content yields an identical exact digest in two **separate sessions**; a 1-ulp change of one coordinate changes the exact digest; an unlisted type yields `UNKNOWN`; the `SIGNED_ZERO_CONTROL` (4.3.1) shows the two layers differ exactly where they are meant to.
- Equal exact digests => equal; unequal => different; not computable => `UNKNOWN`.
- Pre-existing definitions: their exact fingerprint is part of the `PRE_COMMAND_STATE_RECORD`; under `MODIFY_PREEXISTING = PROHIBITED` any change is an unexpected mutation.

`NB-3` advances to baseline artifact preparation: the specification must be extracted as a standalone hashed artifact and agreed before it closes.


## 29. MANIFEST CAPTURE, CUSTODY AND EXTERNAL ANCHOR (NB-6, RC-12)

The manifest has three layers (Proposal V2 14): build-bound, machine-profile-bound, session-evaluated. It attests the **enumerable known set** only; it proves no isolation (`NG-13..NG-18`). This section designs the procedure that gives it audit integrity.

### 29.1 Procedure

| Aspect | Procedure |
|---|---|
| **capture** | performed by the **control plane** in a controlled fresh session on the exact build, machine and profile, after the warm-up (13.1), by running the E-10 enumerations and the hasher; **never hand-edited** |
| **hash** | the manifest is serialized canonically (sorted keys, LF, exact numbers); its SHA-256 is the manifest identity |
| **immutable association with the tuple** | the manifest hash is a tuple field; the run records it; a run whose recorded hash differs from the baseline hash is `INVALID` |
| **storage** | content-addressed and read-only: `manifest-<hash>.json` in a designated evidence location outside the product binary; the machine-profile layer stays with the machine's evidence, the build layer with the build evidence; the repository may hold the build layer and the hashes, not the machine layer |
| **retrieval** | by hash; the hash is **verified on read**; a mismatch refuses use |
| **amendment and versioning** | a manifest is never edited: a change is a **new capture with a new hash** and a `supersedes: <hash>` link; adding a module entry follows the transitions of 29.3; superseded manifests are retained |
| **custody log** | an **append-only custody log** records each capture, review, approval, anchoring, use and supersession with time, role and manifest hash; **each entry contains the hash of the previous entry** (a hash chain) |

### 29.2 Roles (`CAPTURE_ROLE`, `REVIEW_ROLE`, `APPROVAL_ROLE`, `CUSTODY_ROLE`)

| Role | Holds | May not |
|---|---|---|
| `CAPTURE_ROLE` | runs the capture procedure through the control plane and submits the manifest | approve its own capture |
| `REVIEW_ROLE` | the Owner or CAD manager (or a delegate they record) who checks the machine-layer entries (drivers, security software) and the build layer against the build receipt | capture or approve |
| `APPROVAL_ROLE` | the Coordinator, who approves a manifest hash as the baseline manifest | capture |
| `CUSTODY_ROLE` | keeps the custody log and the external anchor (29.4) | alter an entry already anchored |

The roles are **hats**: one natural person may hold more than one hat, and the custody log states it. Separation between `CAPTURE_ROLE` and `APPROVAL_ROLE` is still required as **separate recorded statements**.

### 29.3 Allowed transitions

```text
CAPTURED  ->  REVIEWED  ->  APPROVED  ->  ANCHORED  ->  IN_USE  ->  SUPERSEDED
   any state  ->  REVOKED  (with a recorded cause)
```

- `CAPTURED -> REVIEWED`: by `REVIEW_ROLE`.
- `REVIEWED -> APPROVED`: by `APPROVAL_ROLE`, with the manifest hash in the log.
- `APPROVED -> ANCHORED`: by `CUSTODY_ROLE` (29.4). A manifest that is not `ANCHORED` cannot be `IN_USE`.
- `IN_USE`: recorded at each use (the run id and the tuple).
- `SUPERSEDED` and `REVOKED` never return to `IN_USE`.

### 29.4 External anchor

At baseline preparation, **`MANIFEST_CUSTODY_HEADER_HASH`** (the hash of the head entry of the custody log, including the approval entry) is anchored in a **Coordinator-controlled immutable reference**: a reviewed git commit that contains the header hash in a version-controlled authority record, or an equivalent version-controlled reference. The commit identity is recorded in the custody log. Every later custody event (use, supersession, revocation) is re-anchored in the next baseline or evidence commit. Verification recomputes the chain and compares its head with the anchored value.

### 29.5 What this provides and what it does not

- It provides **tamper evidence** within the threat model: accidental change, unrecorded change and rewriting of the chain are detectable.
- It does **not** provide **non-repudiation or signer identity**; there is **no cryptographic signature**. A hostile local administrator is out of the threat model (`NG-13`).

`NB-6` advances to baseline artifact preparation: the procedure, the anchor and the roles must be agreed before it closes. The captured manifest itself belongs to the execution prerequisites (EXEC-11).


## 30. CONTROL CLASS REGISTRY (RC-1)

`CONTROL_CLASS_REGISTRY` is the **normative** source of the class of every control. Section 1.4 reads the class only from here.

**Method.** Classes are **derived from Proposal ALT-21D and the agreed authorities; none is invented.** `BASIS = EXPLICIT` means the source states the class. `BASIS = DERIVED` means the class follows by a stated inference from a source and needs confirmation (32.3). `CLASS = UNRESOLVED` means no source determines it: the entry is an explicit question, and while any **mandatory** control is `UNRESOLVED`, `ALT21D_HOST_PASS` cannot be `TRUE` (1.6).

Sources: `P-V2-4` = Proposal ALT-21D V2 section 4, the class table (carried unchanged through V3, V4 and V5); `P-V3-4` = V3 section 4 (E-12 by phase); `P-V4-2` = V4 section 2 (A-1, A-2); `P-V5-10` = V5 and V4 owner-summary item 13 (class-B degradation only by a reviewed amendment).

`MANDATORY_IN_PREDICATE` says whether the control decides `ALT21D_HOST_PASS`. Classes: `A` carries the guarantee; `B` reduces risk; `NOT_A_CONTROL` is a run-validity, evidence or rule item; `LOG_ONLY` is recorded and never adjudicated.

| CONTROL_ID | SOURCE_AUTHORITY | CLASS | BASIS | MANDATORY_IN_PREDICATE | RATIONALE | AFFECTED_GROUPS | EXPECTED_FAILURE_MAPPING |
|---|---|---|---|---|---|---|---|
| S-1 staged reads `R_pre`, `M_pre` at PV | P-V2-4 (A1 state comparison) | A | EXPLICIT | YES | state comparison carries RG-2/RG-3 | KS, SC-A | ABORT at PV (O2); cause per 1.3 then class-A row of 1.4 |
| S-2 pass 1 | P-V2-4 (A1) | A | EXPLICIT | YES | state comparison | SC-A, KS | difference => O4; class-A row of 1.4 |
| S-3 pass 2 | P-V2-4 (A1) | A | EXPLICIT | YES | state comparison | SC-A, KS | difference => O4; class-A row |
| E-08 read-set re-observation (S-4) | P-V2-4 (A1) | A | EXPLICIT | YES | state comparison | E8 | ABORT (PV) / divergent or unverified (PC); class-A row |
| S-5 abort verification, with `PRE_COMMAND_STATE_RECORD`, `PREPARE_RESIDUE_MANIFEST` and the static dependency closure as the components of `EXPECTED_AFTER_ABORT` | P-V2-4 (A1) and V4 3.4 | A | EXPLICIT for S-5; DERIVED for the components | YES | the record, the manifest and the closure define what S-5 compares | AB, PR-REC, PW | O2 or O6; class-A row |
| E-01 HostExact | P-V2-4 (A2) | A | EXPLICIT | YES | exclusion by measured fact | ID-A | REFUSE (O1); class-A row |
| E-02 SingleDocument | P-V2-4 (A2) | A | EXPLICIT | YES | | ID-A | REFUSE / ABORT; class-A row |
| E-03 LockHeld | P-V2-4 (A2) | A | EXPLICIT | YES | | LK | REFUSE; class-A row |
| E-04 SingleRackCadCommand | P-V2-4 (A2) | A | EXPLICIT | YES | | E4 | REFUSE / ABORT; class-A row |
| E-05 KnownModesInactive | P-V2-4 (A2) | A | EXPLICIT | YES | | ID-A | REFUSE / ABORT; class-A row |
| E-06 TransactionOwnership | P-V2-4 (A2) | A | EXPLICIT | YES | | E6 | REFUSE / ABORT; class-A row |
| E-07 DatabaseComplete | P-V2-4 (A2) | A | EXPLICIT | YES | | ID-A | REFUSE / ABORT; class-A row |
| E-09 OverrulesBounded | P-V2-4 (A2) | A | EXPLICIT | YES | | E9 | REFUSE / ABORT; class-A row |
| E-10 KnownModuleSetAttestation | P-V2-4 (B) | B | EXPLICIT | YES | risk reduction; proves no isolation | E10, WU | REFUSE / ABORT; class-B row |
| E-11M ManagedLoadContinuity | P-V2-4 (B) | B | EXPLICIT | YES | | E11M, WU | ABORT (PV) / unverified (PC); class-B row |
| E-12 phase 0 PREPARE | P-V3-4 (log only) | LOG_ONLY | EXPLICIT | NO | not adjudicated (`NG-19b`) | E12-V | none |
| E-12 phase A MUTATION | P-V3-4, P-V3-4.6 (E-12 class B) | B | EXPLICIT | YES | fail-closed abort before `Commit()` | E12-V | O2; class-B row |
| E-12 phase B COMMIT BOUNDARY | P-V3-4 (log only) | LOG_ONLY | EXPLICIT | NO | not adjudicated (`NG-19b`) | E12-V | none |
| E-12 phase C W-SCAN | P-V3-4.6 (class B) | B | EXPLICIT | YES | expected write set empty | E12-V, SC-B | O7 or by precedence; class-B row |
| E-12 phase D subscription | P-V3-4 | B | DERIVED | YES | failure to subscribe refuses; loss is UNKNOWN; part of E-12, class B | E12-V | REFUSE (O1) / UNKNOWN; class-B row |
| E-12 phase E inference rule | P-V3-4 | NOT_A_CONTROL | EXPLICIT | NO | a rule: no origin inference | E12-V | none |
| E-12 determinism of the expected mutation set | P-V3-4.5 (degradation only by amendment) | B | DERIVED | YES | the amendment route of V3 4.5 is the class-B route | E12-V | `EMPIRICAL_BEHAVIOR_DIFFERENCE`; class-B row |
| E-13 ProfileEquality | P-V2-4 (B) | B | EXPLICIT | YES | | ID-B | REFUSE; class-B row |
| E-14 KindReadiness | P-V2-4 (A2) | A | EXPLICIT | NO (admission-time; `UNDER_CHARACTERIZATION` in CT-21D, 3.2) | evaluated at product admission | KR | recorded only |
| E-15 ContractBinding | P-V2-4 (A2) | A | EXPLICIT | NO (admission-time; 3.2) | evaluated at product admission | KR | recorded only |
| E-11N native load continuity | P-V2-4 ("not a control"), `NG-10` | NOT_A_CONTROL | EXPLICIT | NO | no authority; declared residual | none | none |
| A-1 read-only opens emit no write events | P-V4-2 | B | DERIVED | YES | premise of E-12 phase C (class B); failure yields only false non-success; amendment route V4 2.4 | EA1 | `CHARACTERIZED_DIFFERENT`; class-B row |
| A-2 no deferred write events after V0 | P-V4-2 | B | DERIVED | YES | same as A-1 | EA2 | `CHARACTERIZED_DIFFERENT`; class-B row |
| WARMUP_MEMBERSHIP (warm-up loads belong to the manifest; coverage criterion) | contract 13.1 over E-10 and E-11M | B | DERIVED | YES | baseline condition of the class-B controls E-10 and E-11M | WU | REFUSE (O1); class-B row |
| LIBRARY_FRESHNESS (private copy authority, TBD-10) | V3 5.2 and contract 19.1 | UNRESOLVED | UNRESOLVED | YES (mandatory) | no source assigns a class; it gates import content but is not in the V5 class table | PR-FRESH | REFUSE (O1) / O2; **no mapping until resolved** (Q-CCR-1) |
| CLONE_POLICY_VERIFIED (non-replacing cloning mode) | contract 7.1 (SC-8) | UNRESOLVED | UNRESOLVED | YES (mandatory) | protects `MODIFY_PREEXISTING = PROHIBITED`; not in the V5 class table | PW-CLONE | FAIL / UNKNOWN; **no mapping until resolved** (Q-CCR-2) |
| INSTRUMENT_QUALIFICATION (I-01..I-16) | contract 4.3 | NOT_A_CONTROL | EXPLICIT (this contract) | NO | run-validity precondition; failure => `UNRELIABLE` | Q | `INSTRUMENT_DEFECT` |
| EVALUATOR_CONFORMANCE | contract EXEC-10 | NOT_A_CONTROL | EXPLICIT (this contract) | NO | evaluator defect => I-16 `UNRELIABLE` | OU | `INSTRUMENT_DEFECT` |
| CLEANUP | contract 20 | NOT_A_CONTROL | EXPLICIT (this contract) | NO | incomplete cleanup => INVALID | CL | INVALID |
| UNDO_EVIDENCE, SAVE_REOPEN, POST_CP_REPORT, POST_V1_TO_CP_QUEUE_LOG, E-12 learning | contract 16, 10.7, 10.8, 27 | NOT_A_CONTROL | EXPLICIT (this contract) | NO (evidence groups) | evidence for Owner Act 2; not a pass mark | U, S1, PP, SC-B, E12-L | none (see the PASS semantics in 21.4) |

**Reading rules.**
- A control of class `UNRESOLVED` maps to `NOT_EVALUATED` (1.4) whatever its result, and its evidence is retained.
- A control with `MANDATORY_IN_PREDICATE = NO` never moves the predicate state; its result is reported.
- A new control may enter only through a reviewed amendment that adds it to this registry with its source and class.


## 31. CONTROL TO GROUP TO SCENARIO COVERAGE MATRIX (RC-1)

The matrix maps every registry control to its groups (21.4) and to the scenario classes (26.3) that must exist in the catalog, and states the **minimum catalog requirement**. It is a design-level requirement; the catalog content and its expected results belong to `NB-2`.

| CONTROL | Class | GROUP | SCENARIO CLASSES | MINIMUM CATALOG REQUIREMENT |
|---|---|---|---|---|
| S-1, S-2, S-3 | A | SC-A, KS | `CL-CLEAN`, `WR-<writer>-<position>` (X0..X6), `KS-*` | clean runs per family; each persistent-change writer at X1..X4 detected by the semantic comparison; a single-field mutation per compared field |
| E-08 (S-4) | A | E8 | `E8`, `NC-E08` | a clean re-observation and a deliberate read-set change detected at PV |
| S-5 | A | AB, PR-REC, PW | `AB-*`, `PR-*`, `PW-*` | abort after PREPARE-W, after MUTATION, after a `Commit()` exception path (simulated at T1, not host evidence), with exact-fingerprint comparison and the closure |
| E-01, E-02, E-05, E-07 | A | ID-A | `CL-CLEAN`, `NC-E01`, `NC-E02`, `NC-E05`, `NC-E07` | one deliberate violation per control refused at P0 |
| E-03 | A | LK | `LK-<mode>`, `NC-E03` | each candidate mode; a conflicting writer probe |
| E-04 | A | E4 | `E4`, `NC-E04` | each designated invocation route; a non-designated route excluded |
| E-06 | A | E6 | `E6-C1..E6-C8`, `NC-E06` | all counter controls, including `STALE_WRAPPER_CONTROL`; a second transaction detected at a sampling point |
| E-09 | A | E9 | `E9`, `NC-E09` | subjects and classes consumed; an overrule with unknown effect refused |
| E-10 | B | E10 | `E10`, `NC-E10` | a canary module changes the snapshot; quiescent equality |
| E-11M | B | E11M | `E11M`, `NC-E11M` | a canary load delivers an event; loss of subscription detected |
| WARMUP_MEMBERSHIP | B | WU | `WU` (dry runs) | the coverage criterion measured; a warm-up load outside the manifest refuses the run |
| E-12 phase A, determinism | B | E12-V | `EV-V-<plan>`, `NC-E12A` | validation plans structurally outside the learning corpus; an unattributable event aborts before `Commit()` |
| E-12 phase C, subscription | B | E12-V, SC-B | `WR-<writer>-<position>`, `NC-E12C` | event-triggered and silent writer variants; the detection channel recorded |
| E-13 | B | ID-B | `CL-CLEAN`, `NC-E13` | profile equality and a deliberate difference |
| A-1, A-2 | B | EA1, EA2 | `EA1`, `EA2` | read-only opens with zero write events; known-correct commits with zero deferred events |
| LIBRARY_FRESHNESS | UNRESOLVED | PR-FRESH | `PR-*`, `Q-I13` | `SOURCE_SWAP_CONTROL`; a modified source detected; stale cache detected |
| CLONE_POLICY_VERIFIED | UNRESOLVED | PW-CLONE | `PW-*` | `CLONE_NON_REPLACE_CONTROL` |
| E-14, E-15 | A (not in predicate) | KR | `KS-*` | recorded `UNDER_CHARACTERIZATION` |
| INSTRUMENT_QUALIFICATION | NOT_A_CONTROL | Q | `Q-<instrument>` | one qualification scenario per instrument I-01..I-16 |
| EVALUATOR_CONFORMANCE | NOT_A_CONTROL | OU | `OU-O1..O7` | one scenario per row of 17.2 |
| CLEANUP | NOT_A_CONTROL | CL | `CL-*` | cleanup checks pass; an induced incomplete cleanup makes the run INVALID |
| UNDO, SAVE_REOPEN, POST_CP, tail log, E-12 learning | NOT_A_CONTROL | U, S1, PP, SC-B, E12-L | `U-*`, `S1`, `WR-*-X7`, `EV-L-<operation>` | evidence complete per scenario |

**Design-level coverage checks (to be repeated when the catalog exists).**
1. Every registry control of class A or B has at least one group above; every group of 21.4 has at least one scenario class.
2. Every mandatory control has at least one clean scenario and one deliberate-violation scenario, unless the control cannot be violated by design (recorded with the reason).
3. Every scenario's `CONTROL_CLASSES_EXERCISED` (26.1) is a subset of the registry classes.
4. A control with no group, or a group with no scenario, makes the matrix incomplete; `ALT21D_HOST_PASS` cannot be `TRUE` while the matrix is incomplete.

At the level of this design every registry control has a group and every group has a scenario class. Agreement of the matrix is `BASE-20`.


## 32. OPEN BLOCKERS, QUESTIONS AND STATUS

### 32.1 Blockers

| Id | Status |
|---|---|
| B1, B2, B7 | `CLOSED_BY_V3_CONTRACT` |
| B3 | `CLOSED_BY_V4_CONTRACT` |
| B4, B8 | `CLOSED_BY_V2_CONTRACT` |
| **B5** | `ADVANCES_TO_IMPLEMENTATION_PREREQUISITE` (design authority drafted, `COMPLETE_FOR_BASELINE`, conditional on RC-4 and RC-8 as applied here; pending agreement); persists as `REMAINS_BLOCKER_FOR_CT21D_EXECUTION` (implementation and integration) and `REMAINS_BLOCKER_FOR_REPLACEMENT_FREEZE` (host evidence). If the design is not agreed it reverts to `REMAINS_BLOCKER_FOR_CT21D_DESIGN` |
| **B6** | `REMAINS_BLOCKER_FOR_ACT2` (plan designed in section 16; `UNDO_EVIDENCE_STATUS = NOT_EXECUTED`) |
| **NB-1** | `REMAINS_BLOCKER_FOR_AUTHORITY_BASELINE` and `ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION`: the comparison of section 25 found no conflict; the scope is not sealed until ratified (25.8) |
| **NB-2** | `REMAINS_BLOCKER_FOR_AUTHORITY_BASELINE`: schema fixed (section 26); the catalog content and its expected results do not exist |
| **NB-3** | `ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION`: two-layer specification designed (section 28); remains a blocker until it exists as a standalone hashed artifact and is agreed |
| **NB-4** | `REMAINS_BLOCKER_FOR_AUTHORITY_BASELINE`: procedure, EVM schema and corpus definition designed (section 27); the completeness review and the agreement are missing |
| **NB-5** | `REMAINS_BLOCKER_FOR_CT21D_EXECUTION`: implementation of instruments I-01..I-16 |
| **NB-6** | `ADVANCES_TO_BASELINE_ARTIFACT_PREPARATION`: procedure, roles and anchor designed (section 29); remains a blocker until agreed |

### 32.2 Architect rulings on CQ-A..CQ-H, as incorporated

| Id | Ruling | Incorporated in |
|---|---|---|
| CQ-A | AGREED with the cause taxonomy | 1.3 to 1.7 |
| CQ-B | AGREED; `NOT_MEASURABLE` is per instrument, fact and tuple; `RELIABLE` needs the qualification record hash of the tuple | 1.2 |
| CQ-C | CLOSED once the private copy is the only import source | 19.1 |
| CQ-D | option B: valid only after an explicit authority comparison | 25 |
| CQ-E | two fingerprint layers | 28 |
| CQ-F | AGREED with an external anchor | 29 |
| CQ-G | option B: valid starting source, independent completeness review required | 27.7 |
| CQ-H | option C: X7 stays a disclosed residual, tail log non-adjudicated, extension only by reviewed amendment | 10.8 |

### 32.3 Open questions introduced by V2.1

| Id | Question |
|---|---|
| Q-CCR-1 | class of `LIBRARY_FRESHNESS` (registry entry, `UNRESOLVED`): A, B or not a control? |
| Q-CCR-2 | class of `CLONE_POLICY_VERIFIED` (registry entry, `UNRESOLVED`): A, B or not a control? |
| Q-CCR-3 | confirm the `DERIVED` classes: A-1 and A-2 (B), E-12 phase D (B), E-12 determinism (B), `WARMUP_MEMBERSHIP` (B), and the components of S-5 (A) |
| Q-CCR-4 | confirm that E-14 and E-15 are admission-time and not mandatory in the CT-21D predicate (registry, 3.2) |
| Q-CCR-5 | confirm the meaning of `PASS` for `NOT_A_CONTROL` evidence groups: required governing runs exist and the evidence is complete per the scenario (21.4) |
| Q-CCR-6 | confirm the split of V2 groups into single-class groups (`ID-A`/`ID-B`, `SC-A`/`SC-B`, `PR-REC`/`PR-FRESH`, `PW`/`PW-CLONE`, `E8`, `E9`, `KR`) |
| Q-WU-1 | does a warm-up on a scratch side database (13.1) conflict with the reading of E-06 "no product write to any side database"? This contract treats the warm-up as outside the governed window and outside the product run |
| Q-DEP-1 | confirm the closed list of rejection grounds of 11.3 (block table only) and the `REUSED_BOUND` rule for nested pre-existing records |
| Q-ACQ-1 | confirm that source drift after the authoritative acquisition is informational (19.1) |
| Q-EVT-1 | confirm the clarification of the phase-A adjudication domain (14.8) as consistent with V3 4.2 row E |
| Q-SEC-1 | confirm `Section = -1` as `OUT_OF_SCOPE` for CT-21D scenarios (25.4) |
| Q-FP-1 | the numeric tolerances and normalizations of the semantic layer are left to the kind authority baseline (28.2): confirm |
| Q-CUS-1 | confirm that roles are hats and separation is by recorded statements when one person holds several (29.2) |

### 32.4 Final status

```text
OWNER_ACT1 = SPONSORED     OWNER_DECISION_STATUS = SPONSORED (pursuit only)
CT21D_AUTHORITY_CONTRACT = DRAFT_V2_1
CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
CURRENTLY_ADMISSIBLE_KIND_SCOPE = NONE     ALT21D_ADMISSION_PASS = NOT_YET_SATISFIABLE
SELECTIVE_AUTHORITY_DESIGN_STATUS = COMPLETE_FOR_BASELINE (DRAFT_V2_1; pending agreement)
UNDO_EVIDENCE_STATUS = NOT_EXECUTED
CTDA_HOST_PASS = FALSE (terminal)     CIA = UNKNOWN     SafeOperationalState = FALSE_FOR_ADMISSION     G3 = STOPPED     G3B = NOT OPEN     CT-50 = NOT EXECUTED
ALT-21E = FALLBACK_IN_EFFECT     Freeze V18 = GOVERNING     Freeze V35 = GOVERNING ITS OWN PREDICATE     Replacement Freeze = DOES NOT EXIST     ADR-0036 = PROPOSED / AMENDED FOR V18
HOST VALIDATION = NOT RUN     BRANCH = NOT RECONCILED
NEXT GATE = COORDINATOR REVIEW OF CT-21D AUTHORITY CONTRACT V2.1, THEN (IF PASS) ARCHITECT ITEM-BY-ITEM CHECK OF RC-1..RC-14
```
