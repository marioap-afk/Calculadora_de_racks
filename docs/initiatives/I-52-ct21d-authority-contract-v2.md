# I-52 — CT-21D AUTHORITY CONTRACT V2 (DRAFT)

> **CT-21D AUTHORITY CONTRACT V2 — DRAFT FOR COORDINATOR REVIEW; THEN ARCHITECT DELTA REVIEW. Design / documentation only. Nothing here is executed, built or authorized.**
>
> ```text
> OWNER_ACT1                        = SPONSORED  (OWNER_ACT1_DECISION = A; SPONSOR_ALT21D = YES)
> ALT21D_DIRECTION                  = OWNER_SPONSORED
> OWNER_DECISION_STATUS             = SPONSORED  (sponsorship of the pursuit ONLY; the final guarantee is NOT accepted)
> CT21D_AUTHORITY_CONTRACT          = DRAFT_V2
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
> NB-1 NB-2 NB-3 NB-4 NB-6 = REMAINS_BLOCKER_FOR_AUTHORITY_BASELINE      NB-5 = REMAINS_BLOCKER_FOR_CT21D_EXECUTION
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

### 0.1 What V2 is, and its disposition of the Architect's required changes

V2 is a **complete restatement** of the authority contract, so that one document is authoritative. **V1 (`1613900118547313deb49bf60ad1126939741261`) is intact and historical.** V2 incorporates exactly the changes the Architect's review of V1
(`AGREED_WITH_REQUIRED_CHANGES`, `READY_AFTER_DOCUMENT_CHANGES`) required and adds nothing else. Sections that did not change are carried over verbatim.

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
| Contract id | **`CT-21D-V2`** |
| Authority text | this document, revision **`DRAFT_V2`**; it supersedes the draft `CT-21D-V1` (`1613900118547313deb49bf60ad1126939741261`), which was never baselined and remains intact as history |
| Predicate | **`ALT21D_HOST_PASS(CT-21D-V2)`** |
| Authority of the guarantee | Proposal ALT-21D V5 (`bb8f230c...`) |
| Owner authority | Owner Act 1 = A: sponsorship of the pursuit only |
| Contract hash | `TO_BE_RECORDED_AT_BASELINE` (SHA-256 over the LF-normalized text of this document plus the V5 blob id) |
| Baseline | `CT21D_AUTHORITY_BASELINE_READY = FALSE` |

```text
ALT21D_HOST_PASS(CT-21D-V2)  !=  CTDA_HOST_PASS(V35-A3)
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
| **D. `ALT21D_HOST_PASS` STATE** | derived **only** from governing group verdicts | the `ADMISSION_EVALUATION` | derived directly from any O1..O7 outcome |

**`ALT21D_HOST_PASS` is never derived directly from an O1..O7 outcome.** A deliberate negative-control scenario may produce O7 and still have a `PASS` group verdict, because O7 was exactly the declared expected result.

### 1.2 Control result and instrument state (Level B)

- **Control result:** `PASS`, `FAIL`, `UNKNOWN` (a reading that could not be established is `UNKNOWN`, never `PASS`).
- **Instrument state:** `RELIABLE` (qualified on the exact build, section 4.3, and its reliability controls passed in the run); `UNRELIABLE` (qualification or a reliability control failed; every control result read through it is `UNKNOWN`); `NOT_MEASURABLE` (a **qualified** instrument shows that the host offers no way to read the fact: a host limitation).
- **Instrument defect versus host limitation.** An implementation defect of the instrument makes it `UNRELIABLE`; it is **not** a host `NOT_MEASURABLE`.

### 1.3 Group verdict (Level C)

A **clean governing run** is a governing run of a scenario that contains no deliberate violation. A group verdict is one of:

| Verdict | Meaning |
|---|---|
| `PASS` | in every required governing run, the observed outcome and control results equal the scenario-declared expected result |
| `FAIL` | with reliable instruments, a governing run contradicts a **designed behaviour**: for example a mandatory control fails in clean governing runs, or a designed detection is missed |
| `CHARACTERIZED_DIFFERENT` | with reliable instruments, a **premise** of the design is shown different (for example A-1 or A-2 false; a companion event set that is not deterministic; an assumed event category absent) |
| `NOT_MEASURABLE` | a qualified instrument shows that a fact required by a mandatory control cannot be read in the host (host limitation) |
| `UNKNOWN` | a reading required for the verdict is missing or inconclusive; **not yet classified** (1.5) |
| `INVALID` | there is no governing run |

### 1.4 Mapping to the predicate state (Level D)

A group inherits the **class** of the mandatory control it evaluates (class A carries the guarantee, class B reduces risk; V5). A group that evaluates both takes the more severe row.

| Governing group verdict | Class A control | Class B control |
|---|---|---|
| `PASS` | satisfied | satisfied |
| `FAIL` | **`FALSE`** (terminal) | `AMENDMENT_PENDING` |
| `CHARACTERIZED_DIFFERENT` | **`FALSE`** | `AMENDMENT_PENDING` |
| `NOT_MEASURABLE` (host limitation) | **`FALSE`** | `AMENDMENT_PENDING` |
| `UNKNOWN` | `NOT_EVALUATED` until classified (1.5) | `NOT_EVALUATED` until classified (1.5) |
| `INVALID` | `NOT_EVALUATED` | `NOT_EVALUATED` |

### 1.5 Classification of a governing UNKNOWN

A governing `UNKNOWN` is **classified by a reviewed ruling** (`CLASSIFICATION_RULING`, Coordinator and Architect) with an exact cause:

| Cause | Consequence |
|---|---|
| `HOST_LIMITATION` | treated as `NOT_MEASURABLE` (row above) |
| `INSTRUMENT_DEFECT` | the measurement and the run are invalid **for that instrument**; the instrument must be corrected and qualified again; **a rerun needs a new authorization**; the state stays `NOT_EVALUATED`; it is not a host limitation |
| other exact cause | the ruling states the exact cause and which row of 1.4 applies |

Until a ruling exists the state stays `NOT_EVALUATED`; an unclassified `UNKNOWN` cannot become `TRUE`.

### 1.6 The states of `ALT21D_HOST_PASS(CT-21D-V2)`

| State | Condition |
|---|---|
| `FALSE` | any class-A row of 1.4 that gives `FALSE`, or a determination under exit B below. Terminal for this contract version; a new contract version is required to try again |
| `AMENDMENT_PENDING` | no `FALSE` condition, and at least one class-B row of 1.4 that gives `AMENDMENT_PENDING` is unresolved |
| `NOT_EVALUATED` | no `FALSE`, no `AMENDMENT_PENDING`, and at least one required group is not `PASS` (unclassified `UNKNOWN`, `INVALID`, or not yet run) |
| `TRUE` | every mandatory **class-A** control satisfied; every mandatory **class-B** control satisfied **under the currently effective contract and amendments**; no `AMENDMENT_PENDING`; every required group has a governing `PASS` |

Precedence: `FALSE` > `AMENDMENT_PENDING` > `NOT_EVALUATED` > `TRUE`. **`AMENDMENT_PENDING` and `TRUE` are mutually exclusive.**

**`AMENDMENT_PENDING` exits only through:**
- **A.** an amendment reviewed by the Architect, approved and applied, **plus** the recharacterization it requires; or
- **B.** a **recorded determination that no acceptable amendment exists**, which makes the state `FALSE`.

There is no other exit, and a mandatory class-B failure never produces `TRUE`. `TRUE` means that the characterization is complete under the effective contract; it does **not** admit a product run and does not reopen G3.

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
| OS version and build (only where relevant to the enumerations) | MACHINE_PROFILE_BOUND | OS query |
| machine identity class | MACHINE_PROFILE_BOUND | recorded label agreed at baseline; the machine name is kept in local evidence only |
| AutoCAD profile identity; `SECURELOAD` and `TRUSTEDPATHS` values | MACHINE_PROFILE_BOUND | profile and system-variable reads |
| the machine's own module entries (drivers, security software) | MACHINE_PROFILE_BOUND | manifest machine layer |
| process id (PID) and process start time | SESSION_BOUND | process query |
| document identity; database identity | SESSION_BOUND | document/database readers |
| drawing identity and hash **before** the run (fixture copy) and after | SESSION_BOUND | file hash of the scratch copy |
| snapshots and checkpoints of the run | SESSION_BOUND | the run log |
| `LibraryInputRecord` (path, file SHA-256, cache-entry identity) | SESSION_BOUND | PREPARE-R |

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
| AI-4 library input | library file path, SHA-256, cache-entry identity | build / session |
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

- It compares **live objects only**: it compares the native identity (`UnmanagedObject`) of the caller's transaction with that of the current top transaction **at that instant**.
- It **rejects** a disposed or null transaction (UNKNOWN), and never compares with a stored value.
- The native pointer is **never persisted as a durable identity** and never used as a log key: a pointer value can be reused after a transaction ends. The log records the boolean result and a sequence number.
- **Scope:** the transaction manager of the **document `Database`**. Transactions of the library side database (AUTH-12 external queries) are **logged separately** and are **excluded** from the document count.

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
| I-12 fingerprint provider | conformance to the FINGERPRINT SPECIFICATION (section 28) | a 1-ulp change alters the digest; an unknown entity gives `UNKNOWN` | identical content gives identical digest across two separate sessions | all three | `INSTRUMENT_DEFECT` |
| I-13 library identity and cache probe | a fresh acquire yields a hash-equal copy | a modified source is detected; a stale cache is detected | repeated acquires equal | detection of change and staleness | `INSTRUMENT_DEFECT` |
| I-14 adversarial writers | each writer produces its intended modification and events (verified by an independent read) | with the writer disabled no change occurs | repeatable at the same position | intended effect and no effect when disabled | `INSTRUMENT_DEFECT`; usable only in characterization builds |
| I-15 result and log writer | complete log | a truncated log is detected by sequence and hash | repeated runs give logs of equal shape | detection of truncation | `INSTRUMENT_DEFECT` |
| I-16 control plane | launches a fresh verified process; the evaluator reproduces every row of section 17.2 on synthetic logs | a wrong binary, an extra `acad.exe` or a tampered log is detected | repeated synthetic evaluations equal | all rows reproduced and all negatives detected | `INSTRUMENT_DEFECT` |

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
| **V1** end of the last read of pass 2 | end of the W-scan interval | — | — | — | marker record |
| **PC = CP** continuity after V1 | continuity set (E-10 snapshot, E-11M, E-12 phase C, guarantee-bearing controls) | continuity unchanged | a control (then O7) | a control FAIL (then O7 or by precedence) | continuity record; the completion marker |

`CP` remains the only contractual completion point. No product write occurs after PC (V3 section 2). Nothing here depends on a host command-end signal.

## 6. PREPARE-R AUTHORITY

- **Content.** Pure computation; library input identity and cache freshness (section 19.1); AUTH-12 presence queries; all validations and rejection decisions; capture of the **single** `PRE_COMMAND_STATE_RECORD`.
- **All rejections known from these inputs occur here**, before PREPARE-W, as clean O1.
- **`PRE_COMMAND_STATE_RECORD` for Selective** contains the read-set of section 11.1 (identities and fingerprints), captured once. Its digest is logged.
- **Reads only.** AUTH-12 queries open their own transactions and commit them (callee-owned); the E-06 count must be 0 before and after each call (section 4.2). No product write may occur in PREPARE-R.
- **Log.** Every AUTH-12 call: sequence, count before and after, result, duration.

## 7. PREPARE-W AUTHORITY

- **Content.** Only the imports that PREPARE-R decided are necessary.
- **Before the first import:** re-observe the relevant elements of the `PRE_COMMAND_STATE_RECORD` and compare (V4 3.3); mismatch => no import, O1, empty manifest.
- **Before each import:** re-hash the library file and compare with the `LibraryInputRecord`; revalidate the cache identity (section 19.1).
- **After each import:** compute and record the fingerprint of the imported definition (section 28); only then may it enter the `PREPARE_RESIDUE_MANIFEST`.
- **Import failure or indeterminate:** the definition does not enter the manifest; the run stops; ABORT-VERIFY compares against the record plus the manifest of completed imports (V4 3.4).
- **Composition rule for a preexisting definition (W-5 / TBD-13)** — section 11.3.
- **Residue.** The manifest is the exact list of what PREPARE-W added or changed and may persist after an abort (Proposal V5 Owner summary, item 9).

### 7.1 Import rules (SC-8)

- **`MODIFY_PREEXISTING = PROHIBITED`.** PREPARE-W only **adds**.
- **Cloning mode.** The import must use a **verified non-replacing** cloning mode: today `DuplicateRecordCloning.Ignore` (verified in the importer on `main`), or a future equivalent **proven** non-replacing. The importer must **declare** the mode it used in its structured result, and PREPARE-W does not start unless the declared mode is on the allow-list. If the cloning policy differs or cannot be verified, the control is `FAIL` (differs) or `UNKNOWN` (cannot verify) according to this contract; **replacement is never silently permitted**.
- **Effect check.** After each import, the fingerprints of the preexisting elements recorded in the `PRE_COMMAND_STATE_RECORD` are re-read for the elements the import can reach; any difference is an unexpected mutation (ABORT-VERIFY then decides O2 or O6).
- **Closure.** The `PREPARE_RESIDUE_MANIFEST` lists the definition **and the closure of what the import newly added**: nested definitions, symbol-table records, and other cloned dependency records, each with its fingerprint. Only additions are listed.
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
| any writer at X7 | outside W-scan and outside the passes | outside phase C (ends at V1) | not detected: residual |
| any writer at X0 | detects a persistent change | boundary events are logged, not adjudicated (`NG-19b`) | O4 for a persistent change; an ABA restored inside the boundary is `NEITHER`: residual |
| any writer at X8 | outside the guarantee (W3) | outside | recorded for the disclosure only |
| silent writer (Wr-S) at any position in W-scan | detects a persistent change at X1..X4 only | no event | the residual that remains |

The residual therefore concentrates on: writes with **no delivered event**, positions X7 and X8, an ABA restored inside the commit boundary, and transient changes that escape both channels. **Verdict semantics (section 1):** a persistent change at X1..X4 that the semantic comparison misses is a `FAIL` of a guarantee-bearing mechanism (class A); a miss by E-12 where the event was delivered is a class-B `FAIL` (`AMENDMENT_PENDING`); the residual cells are **recorded, not failed**.

**Baseline runs without any writer** (a number of governing runs fixed at the baseline with an explicit statistical intent, section 21.7) measure the **false terminal failure rate** (O7 or another non-success on a correct state) and evaluate **A-1** and **A-2** (section 14.7). The rate is reported; **no threshold is set here**; it is an Owner Act 2 input.

### 10.7 Post-CP characterization (replaces TBD-11)

`POST_CP_CHARACTERIZATION_REQUIRED_BEFORE_ACT2`: for a bounded tail after CP (the length is operational, recorded and informational only: section 21.7), record every event on protected objects and re-read the sealed list once. The result is a **report**; it is not an input to SUCCESS.

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
| RackCad-owned definitions and library-derived definitions | full name set of the block table; per-definition fingerprint (section 28) for RackCad-owned and manifest definitions and for every preexisting definition the plan references | name enumeration trivial; **canonical definition dump / fingerprint reader: MISSING** |
| target space content | full handle set of the space's entities, and the RackCad reference set with class | handle enumeration trivial but not present as an instrument: **MISSING** |
| authored payload and envelopes | scan of every definition's RackCad payload and envelope; set of `RackId`; expected: no new `RackId` | `RackBlockData.Read` exists (code evidence); the aggregate scan comparator for this purpose: **MISSING** |
| bindings and addresses | per-view `RackViewAddress` and grouping | code evidence (Foundation); host reader: **MISSING** |
| library-derived residue | every imported definition of the manifest and its dependency closure, fingerprint | covered by the definition reader: **MISSING** |
| **symbol-table records** | name sets of layers, linetypes, text styles, dimension styles and registered applications, **and the fingerprint of each preexisting record the creators use or may touch (for example every layer the plan draws on: name, color, linetype, lineweight, on/off, frozen, locked, plot flag, transparency)** | **MISSING** |
| NOD and project-variable state | the registry entries relevant to the kind, fingerprint | I-49 store (code evidence); host aggregate reader: **MISSING** |
| identities | handles, `RackId`, sibling grouping identity | as above |

### 11.2 Verdict

Exact match with `EXPECTED_AFTER_ABORT` => O2. Extra residue, mismatch, insufficient read or UNKNOWN => O6.

### 11.3 Composition, reuse and closure for preexisting definitions (W-5, SC-8)

For **Selective V1** the kind rule is:

- **`MODIFY_PREEXISTING = PROHIBITED`.** PREPARE-W only **adds**.
- **Reuse.** If the expected block name **already exists and is a block definition of the expected type**, it is **`REUSE_AS_IS`**. It is **not** compared with the library content as an admission condition. Its fingerprint is part of the `PRE_COMMAND_STATE_RECORD` so that any later change is detected as unexpected.
- **Reject (O1)** when: the name exists but is a different kind of object (a layout, an anonymous block, an external-reference or overlay record, or any object that is not a block definition of the expected type), or another **structural incompatibility explicitly defined by this contract** applies. No other rejection ground is created here.
- **Closure.** The `PREPARE_RESIDUE_MANIFEST` lists the added definition **and the closure of the dependencies the import newly added** (nested definitions, symbol-table records, other cloned dependency records), each with its fingerprint (section 28).
- **Cloning.** Only under a verified non-replacing cloning mode (section 7.1). A differing or unverifiable policy is `FAIL` or `UNKNOWN`; replacement is never silently permitted.
- **Consequence.** `EXPECTED_AFTER_ABORT` = the record **plus** the additions listed in the manifest (definitions and their closure), and **nothing else**.

For any future kind that permits modification of a preexisting definition, its kind baseline must define: (1) what the preexisting state represents in the record; (2) which PREPARE-W change is permitted residue; (3) how `EXPECTED_AFTER_ABORT` is verified for that definition; (4) how permitted residue is distinguished from an unexpected modification.

**Cases distinguished**

| Case | Recognition |
|---|---|
| legal PREPARE residue | a definition or dependency record **added**, present in the manifest with a recorded fingerprint, equal to it now |
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

The comparison is **EXACT set equality** in V1. The availability cost (a demand-loaded module changing the set) is measured and reported, not softened. Hashes are recomputed at each point in CT-21D; a metadata-only shortcut needs a reviewed amendment.

### 12.4 UNKNOWN conditions

An enumeration throws or is unavailable; a file cannot be read for hashing (locked, denied); an entry has no file (in-memory assembly); path normalization is ambiguous; two enumerations of the same set disagree beyond a load the events explain.

**Elapsed time never creates a semantic `UNKNOWN` inside E-10.** A **control-plane watchdog** (section 21.1) that expires may make the run or the instrument status `INVALID`; that is a run status, not an E-10 verdict.

### 12.5 Known incompleteness (declared)

Loaded image versus file (`NG-16`); LISP, VBA, in-process COM, reflection-loaded code (`NG-17`); data-file modules and native code no enumeration reports (`NG-18`); transient loads and unloads between snapshots and native load events (`NG-10`). Snapshot equality is **not** isolation and **not** proof of absence of other code.

### 12.6 Instrument warm-up

Hashing, cryptography and file I/O can load modules. The instrument therefore runs its enumeration and hashing **once during the warm-up** (section 13.1), before the subscription and the baseline, so that its own dependencies are already loaded and belong to the manifest.

## 13. E-11M AUTHORITY (TBD-05) AND WARM-UP ORDER (SC-11)

Managed load continuity. **Native load events are out of authority** (V5, `NG-10`).

### 13.1 Warm-up order

The order is fixed:

| Step | Action |
|---|---|
| 1 | **warm-up** of the instrument and of the product code paths that will run inside the governed window; it performs **no** document write and no product mutation |
| 2 | **record all loads** of the warm-up (snapshot before and after the warm-up; the difference is the warm-up load record) |
| 3 | **check that every warm-up load belongs to the manifest**; a warm-up load outside the manifest refuses the run (O1) |
| 4 | **subscribe** (E-11M and E-12) |
| 5 | **capture the baseline** (E-10, two snapshots) |
| 6 | enter the **governed window** |

The warm-up covers: the product's assemblies, **resource and satellite assemblies** for the message cultures the run may use, the **instrumentation dependencies**, and the **cryptography and I/O dependencies** as applicable.

**`PRELOAD_SET` (the warm-up set) is an operational restriction, not isolation evidence.** It shows that the product's own first-time loads happened before the window; it says nothing about the absence of other code. A product code path that still triggers a first-time load inside the window is a **finding**, not a tolerated event.

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

Both are `UNVERIFIED_PRE_ACT1_ASSUMPTION` until characterized. Neither is authority nor isolation. **A false result is `CHARACTERIZED_DIFFERENT` for a class-B control**, hence `AMENDMENT_PENDING` (section 1.4): it cannot be degraded silently, it needs a reviewed amendment and recharacterization, or a recorded determination that no acceptable amendment exists (then `FALSE`).

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
| caller-owned `Transaction` | the seam receives the orchestrator's `Transaction` and, like AUTH-15, **verifies its identity against the top transaction** (`UnmanagedObject`); a mismatch is a PRE-WRITE failure |
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
SELECTIVE_AUTHORITY_DESIGN_STATUS = COMPLETE_FOR_BASELINE   (DRAFT_V2; pending delta agreement)
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
| U-6b | (optional) attempt `Commit()` against an **already-ended transaction** in the real host | the product's exception-path handling with a **genuine host exception**. It is **real host exception-path handling evidence only**: it is **not** evidence of an indeterminate `Commit()` and **not** a simulation of a partial `Commit()` |
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
| identity | contract id, contract hash, scenario id, run id, group, mode (`LEARNING`, `VALIDATION`, `CHARACTERIZATION_RUN`), event-model version and hash, fingerprint-specification version, sealed-scope version, manifest hash and custody record |
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

### 19.1 Library and cache freshness (TBD-10, SC-10)

**Facts (F-5).** The current cache is keyed on path, last-write time and length, has **no content hash**, and replaces its database when the key changes. The importer returns 0 silently on failure. Freshness therefore **cannot be established from the cache alone** today.

**Why the earlier `FRESH_ACQUIRE` is insufficient.** "Hash, invalidate, acquire, hash again" does not exclude a file that changes and is restored between the two hashes: equal hashes would not prove that the acquired content equals the hashed content.

**One allowed authority path (SC-10): B — single-read local copy.**

| Step | Rule |
|---|---|
| 1 | open the source file **once** for reading (with write access denied to others when the host allows it); stream it into a **private, run-scoped, read-only local copy**, computing SHA-256 over **the same bytes** as they are copied |
| 2 | record in the `LibraryInputRecord`: normalized source path, size, last-write time, the SHA-256 of those bytes, and the copy path |
| 3 | open the library `Database` **from the copy**, never from the source path |
| 4 | key the cache entry by the **copy's hash** and a **generation id** incremented at every fill; invalidate explicitly in PREPARE-R before the acquire |

The recorded hash therefore describes the exact bytes the `Database` was read from. **Atomicity is not claimed beyond this path.**

**Alternatives** (only through a reviewed amendment): **A** — the source held against writes for the entire acquisition interval; **C** — an explicit **residual** when no authority can guarantee same content (hash bracketing only), disclosed in Owner Act 2.

**Pre-import revalidation (kept).** In PREPARE-W, immediately before each import: re-hash the **copy** and compare with the `LibraryInputRecord`; re-hash the **source** file and compare with the record; compare the generation id unchanged. A mismatch stops the import (V3 5.2; O2 with the residue produced so far).

| Element | Rule |
|---|---|
| UNKNOWN | the file cannot be hashed or copied (locked, denied); the copy hash differs from the record; the acquire returns nothing or throws; the importer returns without a structured success; an import result not confirmed by a re-query and a fingerprint |
| status | designed here; **implemented before execution** (`BEFORE_CT21D_EXECUTION`, EXEC-6) |

### 19.2 Fingerprint method

`FINGERPRINT_METHOD = TO_BE_FIXED_BY_CT21D_KIND_AUTHORITY_BASELINE`, gate `BEFORE_CT21D_BASELINE` (V5 W-4). It is designed as the normative artifact **FINGERPRINT SPECIFICATION** in section 28; the concrete instrument is an implementation requirement. `TBD-10` (`CACHE_FRESHNESS`, 19.1) is a separate item and is not merged with it.

### 19.3 Identity rules (all)

Handles are the database-stable identity; `ObjectId` values are session-bound and never persisted in evidence as identity. The `TopTransaction` identity is compared by `UnmanagedObject`. Files are identified by normalized full path plus SHA-256 computed in the run.

## 20. CLEANUP

| Item | Requirement |
|---|---|
| process | a **fresh AutoCAD process per run**; the launcher verifies that no `acad.exe` exists before launch; after the run the process is terminated by the control plane (or exits) and its absence is verified |
| drawing | a fresh byte copy of the scratch template per run, hash recorded; it is never saved except by a scenario that designates a save copy; after the run the scratch file hash is recorded (unchanged unless the scenario saves) |
| library and cache | the in-process cache dies with the process; the library file is a pinned, hash-verified read-only copy; its hash is re-verified after the run |
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
| **INVALID** | tuple mismatch; missing or unreadable log or sequence gap; lost subscription; wrong binary; actual Owner manual input; another process interfering with the governed AutoCAD run; crash; incomplete cleanup; unauthorized retry; a run in Core Console; evidence not sealed; a control-plane watchdog expiry |

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

### 21.4 Required groups

`Q` (instrument qualification), `WU` (warm-up), `ID` (identity and host, section 2.4), `E4` (admitted set), `E6` (counter), `LK` (lock), `E10`, `E11M`, `E12-L`, `E12-V`, `EA1`, `EA2`, `SC` (scan, section 10.6), `PP` (post-CP), `PR` (PREPARE-R, freshness), `PW` (PREPARE-W, residue), `AB` (ABORT-VERIFY), `KS` (Selective seams SL-1, SL-2, SL-4, SL-5, SL-6, section 15), `U-1..U-8` and `U-6b` (section 16), `S1` (SAVE/reopen), `OU` (outcome evaluator scenarios O1..O7), `CL` (cleanup).

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
| mapping of group verdicts to predicate states | **fixed** (section 1); no longer a draft | |
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
| BASE-1 | this authority contract is complete and agreed (Coordinator and Architect) | not met (`DRAFT_V2`) |
| BASE-2 | the Selective kind authority design is complete and agreed (section 15) | designed; agreement pending |
| BASE-3 | E-04 criteria fixed (section 3.3) | fixed here; agreement pending |
| BASE-4 | `LOCK_MODE` candidate contract fixed (section 3.4) | fixed here; agreement pending |
| BASE-5 | event instrumentation designed (sections 13, 14) | designed here; agreement pending |
| BASE-6 | abort read-set fixed (section 11.1) | fixed here; components without reader listed; agreement pending |
| BASE-7 | composition and reuse rules fixed (section 11.3) | fixed here; agreement pending |
| BASE-8 | UNDO evidence plan fixed (section 16) | fixed here; agreement pending |
| BASE-9 | governing-result, retry and attempt-log policies fixed (section 21) | fixed here; the `MUST_FIX` parameters pending |
| BASE-10 | every `BEFORE_CT21D_BASELINE` item resolved: TBD-01 criteria, TBD-03 candidate set, TBD-08 authority design, TBD-13 concrete reads, `FINGERPRINT_METHOD`, composition rule | designed here; agreement pending |
| BASE-11 | baseline hashes recorded (contract, scenario catalog, fingerprint specification, event-model procedure, sealed scope, manifest procedure) | pending |
| BASE-12 | **OD-1 closed**: the sealed Selective view-family scope ratified (section 25) | `NB-1` open |
| BASE-13 | **scenario catalog** complete with the expected result of every scenario and its authority (section 26) | `NB-2` open |
| BASE-14 | **FINGERPRINT SPECIFICATION** agreed (section 28) | `NB-3` open |
| BASE-15 | **E-12 event-model procedure and learning-corpus definition** agreed (section 27) | `NB-4` open |
| BASE-16 | **instrument qualification acceptance criteria** agreed (section 4.3) | designed here; agreement pending |
| BASE-17 | **manifest capture and custody procedure** agreed (section 29) | `NB-6` open |
| BASE-18 | the **classified parameters** fixed (section 21.7): every `MUST_FIX_BEFORE_AUTHORITY_BASELINE` value, with its justification | pending |

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
11. **disclosures:** `T1` tests exercise the evaluator and decision path, and **T1 is not host evidence**; `HOST_EVIDENCE_FOR_INDETERMINATE_COMMIT = NOT_AVAILABLE` unless a later authority adds it; `U-6b`, if used, covers **only real host exception-path handling**.

No decision is prepared here.

## 25. SEALED SELECTIVE VIEW-FAMILY SCOPE (NB-1, OD-1, SC-4)

**Artifact `SELECTIVE_VIEW_FAMILY_SCOPE_V1` — DEFINED from current authority; NOT YET RATIFIED.** `NB-1` stays `REMAINS_BLOCKER_FOR_AUTHORITY_BASELINE` until it is. The family list is **not guessed**: it is taken from the agreed Proposal V17, whose exposure and supported-view rules are preserved by reference in Freeze V18 (section 7) and were consumed in the Foundation reconciliation.

### 25.1 Source authority

Proposal V17: section 1.3 (maximum envelope) and L-01; section 2.1 (inside) and 2.2 (outside); section 3.8 (`A_k`, admissible views per kind). Caveat: V17 section 3.8 is labelled *provisional until cross-initiative reconciliation*; the Foundation reconciliation consumed AUTH-01..13 with no material conflict; the **effective exposure as of the current `main` must be confirmed** (25.5).

### 25.2 The scope

| Family | Domain | Status | Basis |
|---|---|---|---|
| `SEL-FRONTAL` | `View = frontal`, `Section = k` for `0 <= k < SelectiveDepthLayout.Count(system)` (one per depth); the legacy `Section = -1` documented case | **IN** (maximum envelope) | V17 2.1, 3.8 |
| `SEL-PLANTA` | `View = planta` | **IN** (maximum envelope) | V17 2.1, 3.8 |
| `SEL-LATERAL` | lateral, per post | **OUT** (fail-closed, E5) | V17 2.2, L-01 (S-20) |

The created sibling set of a logical rack is the **image of the selected source references** (each a frontal `k` or a planta); `A_k` (all admissible views) is a **pure plan-time commutation check** and creates nothing (V17 3.8).

### 25.3 Rack-level fail-closed conditions inherited by reference

V17 limitations L-11, L-12, L-13, L-14, L-20 to L-26 and L-29 to L-33 apply **per rack** at plan time; this contract does not enumerate them as scenarios. CT-21D scenarios use racks **inside** the maximum envelope; the concrete designs are catalog content (section 26). The V17 note on the effective library (L-28) applies.

### 25.4 What the scope governs

| Artifact | Effect of the scope |
|---|---|
| sealed list (10.2) | one definition and one reference per selected view; envelopes; addresses; grouping |
| protected objects (10.3) | the elements of those views, their containers and the read-set |
| abort read-set (11.1) | definitions, references, envelopes, symbol records of those views |
| scenario catalog (26) | scenarios cover `SEL-FRONTAL` (single and several depths) and `SEL-PLANTA`, and no lateral |
| required seams (15.2) | SL-1, SL-2, SL-4, SL-5, SL-6; **SL-3 not required** |
| comparators and readers (15.7) | for frontal and planta payloads and references |

### 25.5 Exact closure action for NB-1

1. Coordinator and Architect **ratify** that V17 sections 2.1, 2.2 and 3.8 (as reconciled) are the effective scope authority for the Selective view families;
2. **confirm** that no later authority changed the Selective view exposure (evidence: a dated check of the reconciliation table and of the current `RackViewAvailability` facts against V17 3.8);
3. **record** `SELECTIVE_VIEW_FAMILY_SCOPE_V1 = SEALED` with its hash; any later change reopens NB-1 and the dependent artifacts (25.4).


## 26. SCENARIO CATALOG SCHEMA (NB-2)

The **scenario catalog** is the normative list of runs. Its **content is not fixed here**: it needs the ratified sealed scope (section 25), the seam operation inventory and the kind authority baseline. **No scenario outcome is invented in this section.** The schema and the required content are fixed.

### 26.1 Fields of a scenario

| Field | Content |
|---|---|
| `scenarioId`, `group`, `purpose` | identity and the group it serves |
| `mode` | `LEARNING`, `VALIDATION` or `CHARACTERIZATION_RUN` |
| `planInput` | the rack design(s) and the selected views, by reference to fixtures inside the sealed scope |
| `fixture` | the scratch template, library copy and manifest by hash |
| `injections` | writers (10.6) and positions, or none; the deliberate violations of a negative control |
| `expectedProductOutcome` | O1..O7, or none |
| `expectedControlResults` | per control: PASS, FAIL or UNKNOWN |
| `expectedEvents` | the event class expected per phase (E-12) |
| `expectedDetectionChannel` | for scan scenarios: semantic comparison, E-12, both, neither |
| `repetition` | a reference to the classified parameter (section 21.7) |
| `expectedResultAuthority` | **mandatory**: the clause that determines the expected result |
| `sealedAt` | the hash of the expectation, recorded **before the first governing run** of the scenario |

### 26.2 Rules

- **An expected result must cite its authority**: a contract clause or evaluator row that determines it (for example "the injected violation at PV triggers row 3 of 17.2"), or `EVIDENCE_DERIVED` with the pinned artifact (the E-04 admitted set, the frozen event model). A result that no authority determines cannot be declared.
- **No post-hoc expectation.** An expected result is sealed before the first governing run of the scenario and is never filled from a run's output.
- An expected result that depends on an evidence-derived value is pinned to that value's version.

### 26.3 Required scenario classes (content to be enumerated at baseline)

| Class | Purpose |
|---|---|
| `CL-CLEAN` | clean runs per sealed family and plan class (control evaluation, false-rejection measurement) |
| `NC-<control>` | a deliberate violation of one control, to show its detection and outcome |
| `Q-<instrument>` | instrument qualification (section 4.3) |
| `WU` | warm-up and load membership |
| `E4`, `LK-<mode>`, `E6-C*`, `E10`, `E11M` | the measurability groups |
| `EV-L-<operation>` | learning micro-scenarios (section 27) |
| `EV-V-<plan>` | validation runs |
| `EA1`, `EA2` | assumption characterization |
| `WR-<writer>-<position>` | the scan matrix (10.6) |
| `PR-*`, `PW-*` | freshness, import, residue and failure scenarios |
| `AB-*` | abort verification |
| `KS-*` | Selective seam rollback and failure typing |
| `U-*`, `S1` | UNDO and SAVE/reopen |
| `OU-O1..O7` | evaluator scenarios producing each outcome |
| `CL-*` | cleanup |

`NB-2` remains open until the catalog content, with expected results and authorities, is complete and agreed.


## 27. E-12 EVENT-MODEL PROCEDURE AND LEARNING CORPUS (NB-4, SC-5)

### 27.1 Purpose and levels

The model `EVM-Selective-vX` is how `EXPECTED_MUTATION_EVENT_SET = f(plan)` is obtained. Levels (14.3): **contract** fixed before governing runs; **model** frozen after learning and review; **validation** governing. **The same run never teaches and validates.**

### 27.2 Procedure

| Step | Action |
|---|---|
| 1 | **seam operation inventory**: every database-mutating primitive that SL-1, SL-2, SL-4 and SL-5 use (append of an entity, creation of a block definition, entity modification, erasure, extension-dictionary creation, `Xrecord` data set, block-reference append, symbol-table record addition, dynamic-property set, `RecordGraphicsModified`, and any other explicit category the inventory shows). No primitive may be used that has no micro-scenario |
| 2 | **learning by micro-scenarios**: each operation category is executed **in isolation** in the controlled environment (manifest-attested, no adversarial probes) to map it to its host events; several isolated repetitions across independent sessions |
| 3 | **draft model**: entries `(operation category, target class, event kinds emitted with multiplicity as a function of plan quantities, companion events with the relation to the primary target, partial order, phase applicability)` |
| 4 | **explanation review**: every entry carries an explanation of one of three kinds: `PRODUCT_OPERATION`; `HOST_DOCUMENTED` (with a citation); `HOST_OBSERVED_STABLE` (only from isolated micro-scenarios, stable over independent sessions, and accepted by Architect review). **Unexplained events are recorded as `UNEXPLAINED_EVENTS` and block the freeze**; they are never admitted automatically |
| 5 | **freeze**: the model gets a version and a hash; status `DRAFT`, `UNDER_REVIEW` or `FROZEN(hash)`; only `FROZEN` models are used by governing validation |
| 6 | **validation**: governing runs on fresh sessions and fresh fixtures, with plans **outside** the learning corpus, apply the frozen model |

### 27.3 Learning corpus definition

The corpus is the set of micro-scenarios, their fixtures and the sessions that ran them, **sealed by hash**. The **validation corpus** is disjoint (different sessions, fresh fixtures) and contains at least one plan structurally outside the learning corpus (different multiplicities), so that `f(plan)` is tested as a function and not as a lookup.

### 27.4 Determinism criterion

For the same plan, the multiset of `(event kind, selector, step)` is identical in all governing validation runs, with the declared partial order. The number of runs is a classified parameter (section 21.7). If it is not identical, MUTATION is **not deterministic**: the verdict is class B (`AMENDMENT_PENDING`), and E-12 phase A may degrade to telemetry **only** by a reviewed amendment (V3 4.5).

### 27.5 Change control

Any change to the model, the write-event categories, the subscription set, the tuple, or seam code that affects events requires a **new model version, review and revalidation**. A run made under an older model version is not evidence for the newer one.

`NB-4` remains open until this procedure and the concrete learning corpus are agreed as baselineable.


## 28. FINGERPRINT SPECIFICATION (NB-3, ID-4)

**`FINGERPRINT_SPEC_VERSION` is a tuple field.** Digest algorithm: SHA-256 over a canonical byte stream that begins with the domain separator `FPSPEC-V1|<record type>|`. A change of this specification is a change of the baseline.

### 28.1 Canonical schema by record type

| Record type | Canonical content |
|---|---|
| `DEFINITION_DUMP` (block table record) | block name; origin; explodability; scaling; units; annotative flag; then, for each entity **in the record's iteration order**, its canonical entity record; nested definitions are referenced **by name** and have their own digest; dependency records by name |
| `ENTITY` | dxf class name; layer name; linetype name; color (indexed or true color, with the kind of source); lineweight; transparency; linetype scale; visibility; plus the geometry and properties of its type: line (endpoints), arc, circle, polyline (vertices, bulges, widths, closed), text and multi-text (string, height, rotation, style name, alignment), block reference (block name, position, scale factors, rotation, normal, dynamic property values), attribute definition, and any type the specification lists |
| `SYMBOL_RECORD` | table kind; name; the properties of the record (for a layer: color, linetype name, lineweight, on/off, frozen, locked, plot flag, transparency) |
| `PAYLOAD` (semantic) | the envelope parsed by the **authoritative reader** into its typed form, then serialized by a canonical serializer: properties in sorted order, exact numeric representation, `ExtensionData` and unknown members included in sorted order; an unrecognized schema version is `UNKNOWN` |
| `PAYLOAD_BINARY` | the raw stored bytes, digested for diagnostics only |
| `REFERENCE` | block name; position; scale factors; rotation; normal; layer; dynamic property values; the transform is **compared by the comparator with the kind's declared tolerance**, not by the digest |
| `NOD_ENTRY` | key; canonical value |
| `ENUMERATION` | the ordered list of names or handles of a container, digested; **handle lists are valid only within one session** and are excluded from cross-session determinism |

### 28.2 Exclusions

`ObjectId`; database-local handles (except in `ENUMERATION` within one session); timestamps; volatile local identifiers; reactors and other host-local pointers.

### 28.3 Ordering, numbers and strings

- **Ordering:** entities in the iteration order of the record (verified by the determinism control); dictionary and property keys in sorted ordinal order; sets sorted by canonical key.
- **Numbers:** IEEE-754 binary64 **bit-exact** (16 hexadecimal digits); `-0.0` and `+0.0` are distinct (conservative; the determinism control reveals any false difference).
- **Strings:** exact stored text encoded as UTF-8, **no normalization**; names are compared with the exact stored case.

### 28.4 Unknown entities

A proxy, a custom entity, or any type not listed in the specification makes the fingerprint **`UNKNOWN`**, never a partial digest.

### 28.5 Determinism and comparison rules

- **Determinism control (qualification of I-12):** identical semantic content yields an identical digest in two **separate sessions**; a 1-ulp change of one coordinate changes the digest; an unlisted type yields `UNKNOWN`.
- Equal digests => equal; unequal => different; not computable => `UNKNOWN`. A semantic-equal / binary-different pair is a **secondary finding**, not a difference.
- Preexisting definitions: their fingerprint is part of the `PRE_COMMAND_STATE_RECORD`; under `MODIFY_PREEXISTING = PROHIBITED` any change is an unexpected mutation.

`NB-3` closes only when this specification is agreed as baselineable.


## 29. MANIFEST CAPTURE AND CUSTODY (NB-6)

The manifest has three layers (Proposal V2 14): build-bound, machine-profile-bound, session-evaluated. It attests the **enumerable known set** only; it proves no isolation (`NG-13..NG-18`). This section designs the procedure that gives it audit integrity.

| Aspect | Procedure |
|---|---|
| **capture** | performed by the **control plane** in a controlled fresh session on the exact build, machine and profile, after the warm-up (13.1), by running the E-10 enumerations and the hasher; **never hand-edited**; the capture is authorized by the Coordinator; the Owner or CAD manager reviews and approves the result |
| **hash** | the manifest is serialized canonically (sorted keys, LF, exact numbers); its SHA-256 is the manifest identity |
| **immutable association with the tuple** | the manifest hash is a tuple field; the run records it; a run whose recorded hash differs from the baseline hash is `INVALID` |
| **storage** | content-addressed and read-only: `manifest-<hash>.json` in a designated evidence location outside the product binary; the machine-profile layer stays with the machine's evidence, the build layer with the build evidence; the repository may hold the build layer and the hashes, not the machine layer |
| **retrieval** | by hash; the hash is **verified on read**; a mismatch refuses use |
| **amendment and versioning** | a manifest is never edited: a change is a **new capture with a new hash** and a `supersedes: <hash>` link; adding a module entry needs the Owner or CAD manager approval and a Coordinator record; superseded manifests are retained |
| **custody log** | an **append-only custody log** records each capture, approval, use and supersession with time, actor role and manifest hash; **each entry contains the hash of the previous entry** (a hash chain), so that accidental or unrecorded change is detectable |
| **approval statement** | the approval is a recorded statement with the manifest hash in the custody log; cryptographic signing is an open question (CQ) |
| **scope of integrity** | detection of accidental change and of unrecorded change; a hostile local administrator is out of the threat model (`NG-13`) |

`NB-6` closes only when this procedure is agreed as baselineable. The captured manifest itself belongs to the execution prerequisites (EXEC-11).


## 30. OPEN BLOCKERS AND STATUS

### 30.1 Blockers

| Id | Status |
|---|---|
| B1, B2, B7 | `CLOSED_BY_V3_CONTRACT` |
| B3 | `CLOSED_BY_V4_CONTRACT` |
| B4, B8 | `CLOSED_BY_V2_CONTRACT` |
| **B5** | `ADVANCES_TO_IMPLEMENTATION_PREREQUISITE` (design authority drafted, `COMPLETE_FOR_BASELINE`, pending delta agreement); persists as `REMAINS_BLOCKER_FOR_CT21D_EXECUTION` (implementation and integration) and `REMAINS_BLOCKER_FOR_REPLACEMENT_FREEZE` (host evidence). If the design is not agreed it reverts to `REMAINS_BLOCKER_FOR_CT21D_DESIGN` |
| **B6** | `REMAINS_BLOCKER_FOR_ACT2` (plan designed in section 16; `UNDO_EVIDENCE_STATUS = NOT_EXECUTED`) |
| **NB-1** | `REMAINS_BLOCKER_FOR_AUTHORITY_BASELINE`: the sealed Selective view-family scope is **defined from V17** (section 25) and needs ratification and the confirmation of 25.5 |
| **NB-2** | `REMAINS_BLOCKER_FOR_AUTHORITY_BASELINE`: schema and required content fixed (section 26); the catalog content and its expected results are not |
| **NB-3** | `REMAINS_BLOCKER_FOR_AUTHORITY_BASELINE`: the FINGERPRINT SPECIFICATION is designed (section 28); agreement pending |
| **NB-4** | `REMAINS_BLOCKER_FOR_AUTHORITY_BASELINE`: the procedure is designed (section 27); the concrete corpus and agreement pending |
| **NB-5** | `REMAINS_BLOCKER_FOR_CT21D_EXECUTION`: implementation of instruments I-01..I-16 |
| **NB-6** | `REMAINS_BLOCKER_FOR_AUTHORITY_BASELINE`: the procedure is designed (section 29); agreement pending |

### 30.2 Open questions for the Coordinator and the Architect

| Id | Question |
|---|---|
| CQ-A | in 1.4 a class-A `CHARACTERIZED_DIFFERENT` or host `NOT_MEASURABLE` gives `FALSE` (as `FAIL` does): is that the intended reading of "mandatory class-A governing failure"? |
| CQ-B | the instrument-state vocabulary `RELIABLE` / `UNRELIABLE` / `NOT_MEASURABLE` |
| CQ-C | path B (single-read local copy) as the authority for `FRESH_ACQUIRE` (19.1), with A and C only by amendment |
| CQ-D | the sealed Selective scope derived from V17 (25) and the evidence that no later authority changed the exposure (25.5) |
| CQ-E | bit-exact numbers including the distinction of `-0.0` in the fingerprint specification (28.3) |
| CQ-F | a hash-chained custody log without cryptographic signing (29) |
| CQ-G | the seam operation inventory as the source of the learning micro-scenarios (27.2) |
| CQ-H | phase C of E-12 ends at V1, so position X7 (between V1 and the PC evaluation) is recorded as residual (10.6) |

### 30.3 Final status

```text
OWNER_ACT1 = SPONSORED     OWNER_DECISION_STATUS = SPONSORED (pursuit only)
CT21D_AUTHORITY_CONTRACT = DRAFT_V2
CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
CURRENTLY_ADMISSIBLE_KIND_SCOPE = NONE     ALT21D_ADMISSION_PASS = NOT_YET_SATISFIABLE
SELECTIVE_AUTHORITY_DESIGN_STATUS = COMPLETE_FOR_BASELINE (DRAFT_V2; pending delta agreement)
UNDO_EVIDENCE_STATUS = NOT_EXECUTED
CTDA_HOST_PASS = FALSE (terminal)     CIA = UNKNOWN     SafeOperationalState = FALSE_FOR_ADMISSION     G3 = STOPPED     G3B = NOT OPEN     CT-50 = NOT EXECUTED
ALT-21E = FALLBACK_IN_EFFECT     Freeze V18 = GOVERNING     Freeze V35 = GOVERNING ITS OWN PREDICATE     Replacement Freeze = DOES NOT EXIST     ADR-0036 = PROPOSED / AMENDED FOR V18
HOST VALIDATION = NOT RUN     BRANCH = NOT RECONCILED
NEXT GATE = COORDINATOR REVIEW OF CT-21D AUTHORITY CONTRACT V2, THEN ARCHITECT DELTA REVIEW
```
