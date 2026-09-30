# I-52 — CT-21D AUTHORITY CONTRACT V1 (DRAFT)

> **CT-21D AUTHORITY CONTRACT V1 — DRAFT FOR COORDINATOR REVIEW. Design / documentation only. Nothing here is executed, built or authorized.**
>
> ```text
> OWNER_ACT1                        = SPONSORED  (OWNER_ACT1_DECISION = A; SPONSOR_ALT21D = YES)
> ALT21D_DIRECTION                  = OWNER_SPONSORED
> OWNER_DECISION_STATUS             = SPONSORED  (sponsorship of the pursuit ONLY; the final guarantee is NOT accepted)
> CT21D_AUTHORITY_CONTRACT          = DRAFT_V1
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
> ```
>
> The Owner authorized **only** the pursuit of the reduced-guarantee direction. The Owner did **not** accept the final guarantee, authorize CT-21D execution, AutoCAD, RACKMIRROR implementation, reopen G3, change CIA or
> `SafeOperationalState`, issue a Freeze, or modify ADR-0036. This contract executes nothing, builds no harness and changes no code.

## 0. Purpose, authority and reading rules

**Purpose.** To turn the parameters and pending requirements left open by Proposal ALT-21D V5 into a precise, finite **authority contract** that a future `CT-21D` characterization would obey. It defines *what must be measured, how,
with which instrument, under which identity, and how each result is classified*. It does not run anything.

**Authority of the guarantee.** Proposal ALT-21D **V5** (`bb8f230c02dab8abb34db326d9a7d7663f23c212`, blob `f73157d9ce2d5013f4f91fb2d4f4d127a4f879a1`), with V1-V4 as history. This contract **does not reinterpret** SUCCESS, `W-scan`, `W3`, the outcomes O1..O7, E-10/E-11/E-12, PREPARE-R/PREPARE-W,
ABORT-VERIFY, the Owner Act 1 disclosures or the blocker classifications. Where this contract needs a value that V5 leaves open, the parameter is named `TBD-nn` as in V5 section 11 (V3) and resolved here or explicitly left with its gate.

**Candidate host APIs.** Every host API, event or system variable named in this document is a **CANDIDATE instrument**: its existence, semantics and reliability in the exact interactive tuple are themselves characterization targets. Nothing is asserted as available.

**Draft parameters.** A value marked `DRAFT-PARAMETER` is a proposal that the Coordinator and the Architect must confirm at baseline. It is not a decision.

**Repository facts used** (verified on `main` `3375aadb`): product code has no environment observers (no active-transaction count, no system-variable reads, no long-transaction reader, no document-count reader, no load or database event subscriptions);
the AUTH-15 seam covers HeaderRun and Cantilever only; AUTH-12 queries open their own transactions and the external one reads the process-cached library database. The G3A probe exists only as a development tool on the I-52 branch.

## 1. CONTRACT IDENTITY

| Field | Value |
|---|---|
| Contract id | **`CT-21D-V1`** |
| Predicate | **`ALT21D_HOST_PASS(CT-21D-V1)`** |
| Contract status | `DRAFT_V1` |
| Authority of the guarantee | Proposal ALT-21D V5 (`bb8f230c...`) |
| Owner authority | Owner Act 1 = A: sponsorship of the pursuit only |
| Contract hash | `TO_BE_RECORDED_AT_BASELINE` (SHA-256 over the LF-normalized text of this document plus the V5 blob id) |
| Baseline | `CT21D_AUTHORITY_BASELINE_READY = FALSE` |

```text
ALT21D_HOST_PASS(CT-21D-V1)  !=  CTDA_HOST_PASS(V35-A3)
```

- **No automatic transfer of results.** No CT-DA row, no CT-DA PASS, no `CTDA_HOST_PASS` state and no `04NO-S` UNKNOWN is a CT-21D result or counts toward one. CT-DA research tooling (`eng/research/I52Ctda`) and the G3A probe are **reference material only**; using any of it is a decision of a future gate.
- **`CTDA_HOST_PASS` is not revived and stays `FALSE` (terminal).**
- CT-21D is a **characterization** of measurability and behaviour; it is **not** a product admission and never admits a product run.

### 1.1 States of the predicate

| State | Meaning |
|---|---|
| `NOT_EVALUATED` | initial |
| `TRUE` | every required group (section 21.4) has a governing verdict `PASS` and every reviewed amendment that applies has been applied |
| `AMENDMENT_PENDING` | at least one governing group verdict is `CHARACTERIZED_DIFFERENT` or `NOT_MEASURABLE` for a **class-B** control or an assumption (A-1, A-2, event-set determinism); non-terminal; resolved only by an Architect-reviewed amendment (V3 4.5) |
| `FALSE` | a governing group verdict `FAIL` or `UNKNOWN` for a **guarantee-bearing** (class-A) control, for the completion model, or for the abort model; terminal for this contract version; a new contract version is required to try again |

`DRAFT-PARAMETER`: the split between `AMENDMENT_PENDING` and `FALSE` follows V3 4.5 (class B is amendable, class A is not). The Coordinator and the Architect must confirm it.

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

### 3.2 Characterization mode (E-14 and E-15 are not circular)

`E-15` requires a CT-21D record and an Owner acceptance; `E-14` requires seam host evidence. A CT-21D run produces that evidence, so it cannot require it. CT-21D runs therefore use **`CHARACTERIZATION_ADMISSION`**:

- every control E-01..E-13 is evaluated exactly as in product mode and logged with its verdict;
- **E-14** is evaluated as `KIND_UNDER_CHARACTERIZATION` for Selective only, and only if the kind authority baseline (section 15) is present and the seam implementation is integrated;
- **E-15** records the manifest binding to this contract version and marks the CT-21D record and the Owner acceptance as `NOT_YET_EXISTING (EXPECTED)`;
- a `CHARACTERIZATION_ADMISSION` result never admits a product run and never changes `CURRENTLY_ADMISSIBLE_KIND_SCOPE`.

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
| I-12 | fingerprint provider | DOES NOT EXIST | section 19.2 |
| I-13 | library identity and cache probe | DOES NOT EXIST | section 19.1 |
| I-14 | adversarial writer probes (characterization only) | DOES NOT EXIST | sections 3.4, 10.6 |
| I-15 | result and log writer | DOES NOT EXIST | section 18 |
| I-16 | control plane: launcher, process supervisor, offline outcome evaluator, evidence sealer | DOES NOT EXIST for CT-21D | evaluation happens **outside** the AutoCAD process from the raw log |

Every "DOES NOT EXIST" is an **implementation / instrumentation requirement** listed in section 23; none is claimed as existing.

### 4.2 E-06 transaction-counting authority (TBD-02)

**Candidate instrument.** The number of active transactions of the document database's transaction manager (candidate: the managed `TransactionManager` count of active transactions) together with the identity of the top transaction (`TopTransaction.UnmanagedObject`, since the managed wrapper is new on every read). Transaction start and end events of the transaction manager, if they exist, are an **additional log**, never proof. **If no API with sufficient authority exists in the exact tuple, this is an implementation / instrumentation requirement and E-06 stays unsatisfiable until it is met.**

**What is counted:** transactions that are **active** (started and not ended) on the document database's transaction manager; split into the **top-level** one (identity by `UnmanagedObject`) and **nested** ones (depth).

**Expected counts (sampling points)**

| Point | Expected count | Identity |
|---|---|---|
| P0, PL | 0 | none |
| before each AUTH-12 call or import | 0 | none |
| after each AUTH-12 call or import | 0 | none |
| immediately after `T_M` opens | 1 | equals the orchestrator's `T_M` |
| PS | 1 | same |
| PV | 1 | same, and no nested transaction |
| immediately after `Commit()` returns or after `Abort()` and disposal | 0 | none |
| before and after each VERIFY read transaction | 0 → 1 → 0 | the verifier's own read transaction |

**UNKNOWN conditions.** The instrument throws or is unavailable; a negative or impossible value; two sources disagree (counter, own bookkeeping, events); the count changes between two samples with no observed start or end event when events are subscribed; a sample is missing at a required point.

**Top versus internal.** The top transaction is identified by `UnmanagedObject` equality with the orchestrator's `T_M`; any additional active transaction at a sampling point is an internal or foreign one and fails the point.

**Blind spots (declared).** Transactions started and ended between two sampling points are seen only if events are subscribed and delivered; `OpenCloseTransaction` may not be counted by the instrument (characterized in E6-C4); this is the reason `NG-09` exists. The product already uses an `OpenCloseTransaction` in `LateralHeaderDrawService.ReadBlockName` (section 15.1, F-6): the mirror path must not reach it (P-5).

**Reliability controls for the counter itself** (all in the group `E6`)

| Control | What it shows |
|---|---|
| E6-C1 | at rest inside a command the count is 0 |
| E6-C2 | after `StartTransaction` the count is 1; after a nested start it is 2; after commit, abort, and dispose-without-commit it returns as expected |
| E6-C3 | the count sees the callee transactions of AUTH-12 queries and imports (0 → 1 → 0) |
| E6-C4 | whether `OpenCloseTransaction` and a side database transaction are visible to the count |
| E6-C5 | two independent sources agree on a scripted sequence of starts and ends |
| E6-C6 | deliberate violation (a second transaction started by an adversarial probe) is detected at a sampling point |

## 5. CHECKPOINT MODEL

Checkpoints are those inherited from ALT-21D (V3 1.5, V4 1.1); the completion point `CP` is unchanged. Each checkpoint writes a **checkpoint record** (sequence, phase, list of control verdicts, tuple digest).

| Cp | What is measured | What must be stable | May be UNKNOWN | FAIL | Evidence written |
|---|---|---|---|---|---|
| **P0** entry, no lock, no product write | E-01, E-02, E-04, E-05, E-06 (0), E-07, E-09, E-10 baseline (two snapshots), E-11M armed, E-12 armed, E-13, E-14, E-15; tuple | tuple fields; the two E-10 snapshots equal each other and the manifest | any reading (then O1) | any control FAIL (then O1) | tuple; admission record; baseline snapshots; subscription confirmations |
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
- **After each import:** compute and record the fingerprint of the imported definition (section 19.2); only then may it enter the `PREPARE_RESIDUE_MANIFEST`.
- **Import failure or indeterminate:** the definition does not enter the manifest; the run stops; ABORT-VERIFY compares against the record plus the manifest of completed imports (V4 3.4).
- **Composition rule for a preexisting definition (W-5 / TBD-13)** — section 11.3.
- **Residue.** The manifest is the exact list of what PREPARE-W added or changed and may persist after an abort (Proposal V5 Owner summary, item 9).

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

### 10.6 Scan characterization experiment (TBD-09) — design only, not executed

**Goal.** To measure what the two-observation scheme detects and misses, and how often it produces a false terminal failure. **`CHARACTERIZATION != PROOF_OF_STABILITY`**: no result of this experiment is a claim of stability.

**Adversarial writers** (I-14), each a synchronous same-context call injected at a deterministic injection point of a **characterization build** of the verifier:

| Writer | Effect |
|---|---|
| Wr-A | modify the payload of element `k` to a different value and leave it |
| Wr-B | modify element `k` and restore it before the next observation of `k` (ABA) |
| Wr-C | modify element `k` after its own pass-2 read (tail) |
| Wr-D | append an unexpected sibling |
| Wr-E | erase element `k` |
| Wr-F | change a NOD entry the kind consumes |

**Injection positions**

| Pos | Position |
|---|---|
| X0 | after `Commit()` returns, before V0 (commit boundary) |
| X1 | after V0, before the pass-1 read of `k` |
| X2 | immediately after the pass-1 read of `k` |
| X3 | between pass 1 and pass 2 |
| X4 | immediately before the pass-2 read of `k` |
| X5 | immediately after the pass-2 read of `k` (tail) |
| X6 | after the last read, before V1 |
| X7 | between V1 and the PC evaluation |
| X8 | after CP (post-CP characterization, section 10.7) |

**What is measured per (writer, position, element class):** the resulting outcome (SUCCESS, O4, O5, O7); whether the change was detected; whether a change was **missed** (SUCCESS reported); the E-12 events raised. **Designed expectations:** Wr-A, Wr-D, Wr-E, Wr-F at X1-X4 are detected (O4); Wr-B and Wr-C (X5, X6, X7) and X8 are **not** detected by design and are recorded as the residual. A design-detected position that is missed is a `FAIL` of the scan mechanism (guarantee-bearing).

**Baseline runs without any writer** (N runs, `DRAFT-PARAMETER`: at least 10 governing runs over at least 3 sessions) measure the **false terminal failure rate** (O7 or other non-success on a correct state) and evaluate **A-1** and **A-2** (section 14.7). The rate is reported; **no threshold is set here**; it is an Owner Act 2 input.

### 10.7 Post-CP characterization (replaces TBD-11)

`POST_CP_CHARACTERIZATION_REQUIRED_BEFORE_ACT2`: for a bounded tail after CP (`DRAFT-PARAMETER`: until the harness ends the run, at least one host idle cycle), record every event on protected objects and re-read the sealed list once. The result is a **report**; it is not an input to SUCCESS.

## 11. ABORT-VERIFY AUTHORITY

```text
EXPECTED_AFTER_ABORT  =  PRE_COMMAND_STATE_RECORD  +  EXACT PREPARE_RESIDUE_MANIFEST
```

Applies after `Abort()`, after `T_M` state `FAILED` (including a `Commit()` exception), and after a PREPARE-W failure. A fresh read-only transaction under the still-held lock. No callback is ever proof; `cancelled` does not prove rollback; the absence of `modifyUndone`, `modified` or `closed` is not a signal (`04NO-S` restrictions R-1..R-6).

### 11.1 Abort read-set for Selective (TBD-13)

| Component | Compared | Reader / comparator status |
|---|---|---|
| RackCad-owned definitions and library-derived definitions | full name set of the block table; per-definition fingerprint for RackCad-owned and manifest definitions | name enumeration trivial; **canonical definition dump / fingerprint reader: MISSING** |
| target space content | full handle set of the space's entities, and RackCad reference set with class | handle enumeration trivial but not present as an instrument: **MISSING** |
| authored payload and envelopes | scan of every definition's RackCad payload and envelope; set of `RackId`; expected: no new `RackId` | `RackBlockData.Read` exists (code evidence); the aggregate scan comparator for this purpose: **MISSING** |
| bindings and addresses | per-view `RackViewAddress` and grouping | code evidence (Foundation); host reader: **MISSING** |
| library-derived residue | every imported definition of the manifest, fingerprint | covered by the definition reader: **MISSING** |
| symbol tables | name sets of layers, linetypes, text styles, dimension styles and registered applications | **MISSING** |
| NOD and project-variable state | the registry entries relevant to the kind, fingerprint | I-49 store (code evidence); host aggregate reader: **MISSING** |
| identities | handles, `RackId`, sibling grouping identity | as above |

Any component without a reader or comparator is a **blocker for CT-21D execution** (section 23), not silently omitted.

### 11.2 Verdict

Exact match with `EXPECTED_AFTER_ABORT` => O2. Extra residue, mismatch, insufficient read or UNKNOWN => O6.

### 11.3 Composition of the record and the manifest for a preexisting definition (W-5)

For **Selective V1** the kind rule is:

- **PREPARE-W performs ADD only.** A required library definition that is **absent** is added. One that is **present** is reused as is; it is never modified by PREPARE-W. **`MODIFY_PREEXISTING = PROHIBITED`.**
- If PREPARE-R finds a preexisting definition that cannot be reused unmodified (for example, present under the same name with a fingerprint the plan cannot accept), the run is refused (O1); there is no overwrite.
- Consequently, for Selective: `EXPECTED_AFTER_ABORT` = the record **plus** the additions listed in the manifest, and **nothing else**.

For any future kind that permits modification of a preexisting definition, its kind baseline must define: (1) what the preexisting state represents in the record; (2) which PREPARE-W change is permitted residue; (3) how `EXPECTED_AFTER_ABORT` is verified for that definition; (4) how permitted residue is distinguished from an unexpected modification.

**Cases distinguished**

| Case | Recognition |
|---|---|
| legal PREPARE residue | a definition **added**, present in the manifest with a recorded fingerprint, equal to it now |
| partial failed import | a definition or entity content not in the manifest (import threw, or fingerprint not recorded) => unexpected => O6; **never legitimized by the manifest** |
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

Normalized full path; file size; file SHA-256 (computed at each evaluation point); file version information; for managed assemblies also the **module version id read from the file** compared with the **loaded assembly's module version id** (`MVID_CONSISTENCY`). `MVID_CONSISTENCY` is an additional consistency signal recorded and compared; **it does not prove image identity** and a match is not a claim of image integrity (`NG-16`).

### 12.3 Comparison

| Point | Rule | On failure |
|---|---|---|
| baseline (P0) | two snapshots equal each other and equal the manifest set entry by entry (path and hash); no other member | refuse (O1) |
| PV | one snapshot equal to the manifest and to the baseline | abort (O2) |
| PC | one snapshot equal to the manifest and to the baseline | mandatory control failure (O7 by precedence) |

The comparison is **EXACT set equality** in V1. The availability cost (a demand-loaded module changing the set) is measured and reported, not softened. Hashes are recomputed at each point in CT-21D; a metadata-only shortcut needs a reviewed amendment.

### 12.4 UNKNOWN conditions

An enumeration throws or is unavailable; a file cannot be read for hashing (locked, denied); an entry has no file (in-memory assembly); path normalization is ambiguous; two enumerations of the same set disagree beyond a load the events explain; a hash cannot be computed within the run's time budget (`DRAFT-PARAMETER`, fixed at baseline).

### 12.5 Known incompleteness (declared)

Loaded image versus file (`NG-16`); LISP, VBA, in-process COM, reflection-loaded code (`NG-17`); data-file modules and native code no enumeration reports (`NG-18`); transient loads and unloads between snapshots and native load events (`NG-10`). Snapshot equality is **not** isolation and **not** proof of absence of other code.

## 13. E-11M AUTHORITY (TBD-05)

Managed load continuity. **Native load events are out of authority** (V5, `NG-10`).

| Aspect | Contract |
|---|---|
| source | the managed assembly-load event of the application domain (candidate), plus the managed snapshot equality already carried by E-10 |
| subscription lifecycle | subscribe at P0 **before** the baseline snapshot; unsubscribe after PC; the subscription object is retained by the orchestrator |
| window | events from subscription to PC are recorded; events between subscription and completion of the baseline snapshot make the baseline invalid: refuse (O1) |
| record per event | sequence, tick, thread id, assembly full name, location, module version id, phase tag |
| loss of subscription | subscribe failure at P0 => refuse (O1); handler exception, disposal, or any condition that could have dropped events => the control is UNKNOWN |
| event gaps | an assembly present in the PV or PC snapshot but absent from the baseline **without** a load event => gap => UNKNOWN; a load event without a snapshot difference => recorded and FAIL |
| unexpected load | any load event in the window is FAIL (zero load events is PASS). To make this achievable the product must **pre-load every assembly it will use before the baseline snapshot** (`PRELOAD_SET`); lazy loads of the product's own assemblies during the run are a FAIL and are a design finding |
| identity | full name, location, module version id, file hash |
| UNKNOWN | subscription failure or loss; missing sequence numbers; inconsistent snapshot and event evidence |

If `CT-21D` shows that lazy loads cannot be avoided, E-11M is not satisfiable as designed: a **reviewed amendment** is required (no silent relaxation).

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

### 14.3 Expected-set representation and the two-stage event model

`EXPECTED_MUTATION_EVENT_SET` = a list of entries `(event kind, target selector, step id, multiplicity 1)` where a selector is a database handle when known in advance, otherwise `(class, container, logical role)` bound to a handle when the first matching event consumes the entry; later events on a bound handle are matched to entries that name it. Entries may carry a **partial order**; without one, matching is order-insensitive.

The host emits **companion events** the plan alone does not predict (for example modifications of the container when a reference is appended). The plan-derived set therefore comes from a **kind event model** `EVM-Selective-vX` that is obtained from evidence:

1. **LEARNING runs** (E12-L): the model is inferred from recorded events of known-correct MUTATIONs; E-12 phase A adjudicates nothing in these runs.
2. **VALIDATION runs** (E12-V): on fresh sessions and fresh fixtures, the frozen model is applied; determinism is judged here.
3. **Determinism criterion:** for the same plan, the multiset of `(kind, selector, step)` is identical in all validation runs (with the declared partial order). Otherwise MUTATION is **not deterministic** and E-12 phase A degrades to telemetry **only** by a reviewed amendment (V3 4.5).

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

Both are `UNVERIFIED_PRE_ACT1_ASSUMPTION` until characterized. If either is false, a reviewed amendment is required (V4 2.4). Results are not authority and not isolation.

## 15. SELECTIVE KIND AUTHORITY DESIGN (B5)

**Nothing is implemented.** This section is the **authority design** of the caller-owned creation seams that a future RACKMIRROR would need for the Selective kind, modelled on the AUTH-15 seam (`RackDefinitionCreator.CreateInTransaction`).
Names below are **working labels** of roles; final API names are an implementation matter. The design makes no claim that any seam exists.

### 15.1 Facts about the current Selective path (verified on `main` `3375aadb`)

| Id | Fact |
|---|---|
| F-1 | Selective **frontal** and **planta** definitions are created by `SystemBlockWriter.CreateBlock`, which itself takes `LockDocument`, calls the library importer, opens its own transaction, writes the payload with `RackBlockData.Write` and calls `Commit`. The **lateral** path (`LateralHeaderDrawService`) does the same. Reference placement (`BlockPlacement.PlaceBlockWithJig`) is a separate interactive step with its own lock and transaction. **Each view is a separate call and a separate transaction**; sibling views are grouped by the envelope id, not by an atomic unit |
| F-2 | The Selective **lateral** view appears to use the **HeaderRun** plan family, which the AUTH-15 HeaderRun overload dispatches. Whether that overload applies unchanged to the Selective lateral plan **must be confirmed by the design review**; it is a dependency, not a claim. Frontal and planta have **no** caller-owned seam. AUTH-15 never places references |
| F-3 | The rack id is minted in the UI (`RackEditorSession.Complete` -> `EnsureId`, a new GUID string); the plugin receives it. `NewRackId` exists only in duplication code |
| F-4 | The edit side already has caller-owned forms (`SystemBlockWriter.RedefineInTransaction`, `ViewBlockDraw.PrepareRedraw`, used by `ProjectVariableMutationExecutor` under one lock and one transaction): a precedent pattern, not a seam for creation |
| F-5 | The importer runs inside the document lock and outside the drawing transaction, opens its own transaction, and **returns 0 silently on a missing file or an exception**. The library cache is keyed on path, last-write time and length, with **no content hash** |
| F-6 | The product uses an `OpenCloseTransaction` in `LateralHeaderDrawService.ReadBlockName`. The mirror path must not reach it |
| F-7 | Selective creation writes **no NOD** |
| F-8 | Read side: `RackBlockData.Read`, `RackBlockFinder.ScanEnvelopes`, `SelectiveAuthoredAuthority.Resolve`. `Resolve` compares the **siblings among themselves**; **no expected-versus-persisted comparator exists** |

### 15.2 Seam set (roles)

| Seam | Role | Caller-owned |
|---|---|---|
| SL-1 | create the **frontal** view definition in the caller's transaction | yes |
| SL-2 | create the **planta** view definition in the caller's transaction | yes |
| SL-3 | create the **lateral** view definition: the AUTH-15 HeaderRun overload if the review confirms it applies, otherwise a Selective-specific overload | yes |
| SL-4 | **place the block reference** in the caller's transaction from a computed transform (no jig) | yes |
| SL-5 | PREPARE-W **import** with a structured result and a fresh acquire of the library (outside `T_M`, under the lock) | callee transactions allowed (AUTH-12 pattern) |
| SL-6 | the **read-only verifier** and the expected-versus-persisted comparator | read-only |

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
| explicit library and input dependencies | the plan declares its required library blocks; SL-1..SL-3 **do not import**; a missing required block is a typed failure; imports belong to SL-5 in PREPARE-W |
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
| P-2 | confirm or adapt the lateral path to the AUTH-15 HeaderRun overload |
| P-3 | in-transaction, non-jig reference placement (SL-4) |
| P-4 | importer with a structured result and a fresh-acquire library path (SL-5); today it swallows failures |
| P-5 | static guards: no `Commit`, `Abort`, `Dispose`, `LockDocument`, `OpenCloseTransaction` or side-database write in the seams, and removal of `ReadBlockName` from the mirror path |
| P-6 | the read-only verifier and the expected-versus-persisted comparator (SL-6) |
| P-7 | the fingerprint provider (section 19.2) |
| P-8 | **host evidence** of the seams' rollback, in the AUTH-15 style (document, held lock, `StartTransaction`, abort without commit; side databases as characterization only): gate `BEFORE_REPLACEMENT_FREEZE` |

### 15.9 Open design decisions (do not block the design status)

| Id | Decision |
|---|---|
| OD-1 | which Selective view families (frontal, lateral, planta) are in the RACKMIRROR exposure scope; it decides which of SL-1..SL-3 are needed. The design is parametric in the family |
| OD-2 | whether SL-3 is the AUTH-15 HeaderRun overload unchanged |
| OD-3 | the scope of the host rollback evidence P-8 (which families and which failure injections) |

### 15.10 Status

```text
SELECTIVE_AUTHORITY_DESIGN_STATUS = COMPLETE_FOR_BASELINE   (DRAFT; pending Coordinator and Architect agreement)
```

Every clause required by the gate is specified. Nothing is BLOCKED at design level. The seams P-1..P-8 do not exist and are implementation prerequisites (section 23, EXEC-3).

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
| U-6 | applicable O6 cases (state that cannot be confirmed after abort) | whether UNDO recovers the expected state; `Commit()` exceptions can be exercised only at T1 with a fake host adapter, because a real host `Commit()` exception cannot be induced at will (declared limit) |
| U-7 | interplay with PREPARE residue | whether UNDO removes the imported definitions together with or separately from the MUTATION |
| U-8 | independence from callbacks | every comparison uses state reads only |

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

`ADM` admission verdicts at P0 and PL (per control PASS, FAIL, UNKNOWN); `PREP` PREPARE-R and PREPARE-W status and manifest; `TM` state of `T_M` (`ABORTED`, `FAILED`, `COMMIT_ACCEPTED`, or `NOT_OPENED`); `CONT_PV` and `CONT_PC` continuity verdicts (class A, class B); `OBS1`, `OBS2` per-element observations (equal, differs, incomplete or UNKNOWN); `VDEF` verifier-defect flag; `ABV` ABORT-VERIFY result (match, mismatch, unreadable); `EVT_C` phase-C write events on protected objects; log completeness flags.

### 17.2 Deterministic table (first matching row applies)

| # | Condition | Primary outcome | Secondary findings recorded |
|---|---|---|---|
| 1 | log incomplete or run INVALID | no outcome (NON_GOVERNING / INVALID, section 21) | the reason |
| 2 | any admission control FAIL or UNKNOWN, or a rejection known from PREPARE-R inputs, before any product write | O1 | the failing controls |
| 3 | `TM` = `NOT_OPENED` after a PREPARE-W write, or `TM` = `ABORTED` or `FAILED` (including a `Commit()` exception), and `ABV` = match | O2 | `COMMIT_EXCEPTION_OR_INDETERMINATE` when applicable; the residue manifest |
| 4 | same as 3 and `ABV` = mismatch or unreadable | O6 | what was observed |
| 5 | `TM` = `COMMIT_ACCEPTED` and any `OBS1` or `OBS2` element **differs** from expected (including sealed-list violations) | O4 | class-A and class-B control failures; `VDEF` |
| 6 | `TM` = `COMMIT_ACCEPTED` and (any `OBS` incomplete or UNKNOWN, or `VDEF`) | O5 | control failures |
| 7 | `TM` = `COMMIT_ACCEPTED`, all `OBS1` and `OBS2` equal, and any mandatory control in `CONT_PC` FAIL or UNKNOWN, or any `EVT_C` on protected objects | O7 | the control ids |
| 8 | `TM` = `COMMIT_ACCEPTED`, all `OBS` equal, all mandatory controls PASS, `EVT_C` empty, no `VDEF` | O3 SUCCESS | non-normative diagnostics only |

**Missing evidence:** a missing semantic observation is incomplete (row 6); a missing control record is that control UNKNOWN (row 7); a missing ABORT-VERIFY read is unreadable (row 4); a missing admission record is UNKNOWN (row 2). **Pre-commit continuity** failures at PV abort (rows 3 or 4). **No cause is attributed** in any report. An evaluator defect is detected by evaluating the table against the scenario's declared expected outcome.

## 18. RESULT / LOG SCHEMA (conceptual)

### 18.1 Run log

An append-only event log (one JSON record per line): `runId`, `sequence`, `tick`, `utc`, `thread`, `phase`, `checkpoint`, `recordType` (`ENTRY`, `TUPLE`, `CONTROL`, `SNAPSHOT`, `EVENT`, `OBSERVATION`, `LOCK`, `TXCOUNT`, `LIBRARY`, `PREPARE`, `IMPORT`, `MUTATION`, `COMMIT`, `ABORT`, `VERIFY`, `ABORT_VERIFY`, `OUTCOME`, `CLEANUP`, `FINISH`) and a typed payload. A record with a missing sequence number marks a gap.

### 18.2 Evidence package (future)

| Part | Content |
|---|---|
| identity | contract id, contract hash, scenario id, run id, group, mode (`LEARNING`, `VALIDATION`, `CHARACTERIZATION`) |
| tuple | section 2 fields with hashes |
| checkpoints | records of section 5 |
| observations | pass 1 and pass 2 values per element; `PRE_COMMAND_STATE_RECORD`; `PREPARE_RESIDUE_MANIFEST`; ABORT-VERIFY reads |
| events | E-11M and E-12 logs |
| outcomes | raw runner classification, canonical result, primary and secondary findings |
| hashes | of every artifact and of the log |
| timestamps | start, checkpoints, finish |
| cleanup | section 22 record |
| owner-touch | statement and process list snapshots |
| failures | crashes, exceptions, instrument UNKNOWNs |
| raw versus canonical | the tooling's label is preserved unchanged; the canonical result is the ruling; a tooling label is never authority |

**No evidence is generated by this document.** The schema is conceptual; a concrete schema is fixed at the baseline.

## 19. FRESHNESS / IDENTITY RULES

### 19.1 Library and cache freshness (TBD-10)

**Facts (F-5).** The current cache is keyed on path, last-write time and length, has **no content hash**, and replaces its database when the key changes. The importer returns 0 silently on failure. Freshness therefore **cannot be established from the cache alone** today.

**Contract**

| Element | Rule |
|---|---|
| source file identity | normalized full path, size, last-write time and **SHA-256 of the file computed in this run** (recorded in the `LibraryInputRecord`) |
| cache identity | the cache entry's path, the hash bracketing its fill, a **generation id** incremented at every fill, and the process id |
| freshness rule | **`FRESH_ACQUIRE`**: (1) hash the file; (2) invalidate the entry; (3) acquire (fill) the entry; (4) hash the file again. The entry is FRESH only if the two hashes are equal and equal to the `LibraryInputRecord`. Key equality of path, time and length is **never** accepted as freshness |
| invalidation | explicit, in PREPARE-R, before the acquire; the cache dies with the process |
| pre-import revalidation | in PREPARE-W, immediately before each import: re-hash the file and compare with the `LibraryInputRecord`, and compare the generation id unchanged; a mismatch stops the import (V3 5.2) |
| UNKNOWN | the file cannot be hashed (locked, denied); the two hashes differ; the acquire returns nothing or throws; the importer returns without a structured success; an import result that is not confirmed by a re-query and a fingerprint |
| status | designed here; **implemented before execution** (`BEFORE_CT21D_EXECUTION`, EXEC-6) |

### 19.2 Fingerprint method

`FINGERPRINT_METHOD = TO_BE_FIXED_BY_CT21D_KIND_AUTHORITY_BASELINE`, gate `BEFORE_CT21D_BASELINE` (V5 W-4). This section designs it; the concrete instrument is an implementation requirement.

| What needs a fingerprint | Kind of identity |
|---|---|
| imported or relevant block definitions | **exact canonical dump** of the definition's entities and properties (type, geometry compared bitwise, layer, attributes, dynamic-block data), digested with SHA-256 |
| destination authored payload and envelope | **semantic**: the payload is parsed by the authoritative reader into its typed form and canonically serialized; digested. The raw bytes are digested as a **binary** fingerprint for diagnostics |
| materialization elements (references) | semantic: block name, transform (compared by the comparator with the kind's declared tolerance, not by the digest), layer, dynamic-property values |
| NOD registry entries | semantic canonical form |
| containers and enumerations | ordered lists of handles and names, digested |

**Comparison.** Equal digests => equal. Unequal digests => different. A digest that cannot be computed => `UNKNOWN`. A semantic-equal / binary-different pair is recorded as a secondary finding (possible writer nondeterminism), not treated as different. Digest collisions are treated as negligible for SHA-256 and produce no special path; there is no "collision" verdict. **Preexisting definitions:** their fingerprint is part of the `PRE_COMMAND_STATE_RECORD`; under `MODIFY_PREEXISTING = PROHIBITED` (section 11.3) any change is unexpected mutation.

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

Status is evaluated per **run** (one scenario in one process).

| Status | Conditions |
|---|---|
| **GOVERNING** | tuple equal to the baseline; log complete with no sequence gap and sealed; all required subscriptions intact for the whole run; correct binaries; no Owner touch; no process contamination; normal termination by the control plane; cleanup verified; run id unique and not an unauthorized retry; scenario equal to the catalog entry |
| **NON_GOVERNING** | a valid execution that by design does not count toward acceptance: dry runs, rehearsal runs, LEARNING runs when used as validation, runs of a scenario marked exploratory. Their data stays evidence of behaviour |
| **INVALID** | tuple mismatch; missing or unreadable log or sequence gap; lost subscription; wrong binary; actual Owner manual input; another process interfering with the governed AutoCAD run; crash; incomplete cleanup; unauthorized retry; a run in Core Console; evidence not sealed. Results are discarded as evidence of the behaviour and retained as evidence of the invalidity |

CT-DA rules are **not copied**: these are designed for CT-21D. (A crash caused by the product logic under test is recorded as a **finding** that needs a Coordinator ruling; it is not silently an INVALID.)

### 21.2 Group verdicts

`PASS` (designed behaviour observed in the required governing runs), `FAIL` (behaviour contradicts the design), `CHARACTERIZED_DIFFERENT` (the host behaves differently from an assumption, for example A-1 or A-2 false), `NOT_MEASURABLE` (the instrument cannot measure), `UNKNOWN` (a reading required for the verdict is missing), `INVALID` (no governing run). Mapping to the predicate: section 1.1.

### 21.3 Retry policy (a policy to be frozen at baseline; **no retry is authorized now**)

| Case | Policy |
|---|---|
| PASS | no retry; scheduled repeats required by the repetition policy are repeats, not retries |
| FAIL (governing) | no retry; the result stands; leads to `AMENDMENT_PENDING` or `FALSE` per section 1.1 |
| UNKNOWN (governing) | no retry to seek a better verdict; goes to Architect review; instrument defects are fixed and the run repeated only under a new authorization |
| INVALID | the run is discarded; a re-run needs an explicit Coordinator authorization per occurrence, a new run id, and a recorded cause; the cause is classified tooling, environment or contamination; the maximum attempts per scenario are set at baseline (`DRAFT-PARAMETER`) |
| process crash | INVALID for the measurement; recorded as a finding; no automatic retry; a Coordinator ruling decides |
| any case | a governing FAIL or UNKNOWN is never retried to obtain a better result |

**Repetition policy** (`DRAFT-PARAMETER`, to be confirmed): each measurability group needs at least 3 governing runs over at least 2 independent sessions; determinism validation (section 14.3) at least 5; the baseline scan runs of section 10.6 at least 10.

### 21.4 Required groups

`ID` (identity and host, section 2.4), `E4` (admitted set), `E6` (counter), `LK` (lock), `E10`, `E11M`, `E12-L`, `E12-V`, `EA1`, `EA2`, `SC` (scan, section 10.6), `PP` (post-CP), `PR` (PREPARE-R, freshness), `PW` (PREPARE-W, residue), `AB` (ABORT-VERIFY), `KS` (Selective seam, section 15), `U-1..U-8` (section 16), `S1` (SAVE/reopen), `OU` (outcome evaluator scenarios O1..O7), `CL` (cleanup).

### 21.5 Owner no-touch and automation

- **No-touch is required** during every governing run: no Owner mouse or keyboard input after launch.
- **Allowed automation:** the governed control plane (launcher, script-driven in-process harness commands, supervisor). It is part of the contract and recorded.
- **Contamination:** any manual Owner input; any other process interacting with the governed AutoCAD process (automation, message injection, another RackCad-loaded session); another `acad.exe` running; a modal dialog answered by hand.
- **Not contamination** (Coordinator ruling carried as a principle): unrelated automated windows that do not touch AutoCAD.
- **Other RackCad processes:** none may run during a governing run; the process list is snapshotted at start and end.

### 21.6 Fail-closed rules (summary)

Any UNKNOWN required reading => the control is UNKNOWN => the run outcome follows the table of section 17. Any missing record => UNKNOWN. Any instrument without a reliability control that passed => its group verdict cannot be `PASS`. Nothing is inferred from the absence of an event or from timing. No cause is attributed to an outcome without separate evidence.

## 22. BASELINE REQUIREMENTS

`CT21D_AUTHORITY_BASELINE_READY = TRUE` only when **all** of the following hold. Nothing here authorizes execution.

| Id | Requirement | Status |
|---|---|---|
| BASE-1 | this authority contract is complete and agreed (Coordinator and Architect) | not met (`DRAFT_V1`) |
| BASE-2 | the Selective kind authority design is complete and agreed (section 15) | design drafted; agreement pending |
| BASE-3 | E-04 criteria fixed (section 3.3) | fixed here; agreement pending |
| BASE-4 | `LOCK_MODE` candidate contract fixed (section 3.4) | fixed here; agreement pending |
| BASE-5 | event instrumentation designed (sections 13, 14) | designed here; agreement pending |
| BASE-6 | abort read-set fixed (section 11.1) | fixed here; components without reader listed; agreement pending |
| BASE-7 | fingerprint and composition rules fixed (sections 11.3, 19.2) | fixed here; agreement pending |
| BASE-8 | UNDO evidence plan fixed (section 16) | fixed here; agreement pending |
| BASE-9 | governing-result and retry policies fixed (section 21) with the `DRAFT-PARAMETER` values confirmed | pending |
| BASE-10 | every `BEFORE_CT21D_BASELINE` item resolved: TBD-01 criteria, TBD-03 candidate set, TBD-08 authority design, TBD-13 concrete reads, `FINGERPRINT_METHOD`, composition rule | designed here; agreement pending |
| BASE-11 | baseline hashes recorded (contract hash, scenario catalog hash) | pending |

```text
CT21D_AUTHORITY_BASELINE_READY = FALSE
```

## 23. EXECUTION PREREQUISITES

`CT21D_EXECUTION_READY = TRUE` requires **all** of the following, and is **separate** from baseline readiness.

| Id | Prerequisite | Status |
|---|---|---|
| EXEC-1 | `CT21D_AUTHORITY_BASELINE_READY = TRUE` | not met |
| EXEC-2 | implementation of the required instrumentation I-01..I-16 (all "DOES NOT EXIST" items in section 4.1), with their reliability controls passing | not met |
| EXEC-3 | Selective caller-owned seam **implemented and integrated** in `src/` under a separate, separately governed workflow (section 15.9) | not met |
| EXEC-4 | the abort read-set components without readers or comparators (section 11.1) implemented | not met |
| EXEC-5 | exact build and exact package: source SHA, binaries, control plane, scenario catalog, manifest; hashes recorded | not met |
| EXEC-6 | resolved `BEFORE_CT21D_EXECUTION` items: TBD-02 (E-06 counter), TBD-04 (instrument), TBD-05, TBD-06 (instrumentation), TBD-10 (freshness), plus assumption characterization designs A-1, A-2 | designed here; instruments not built |
| EXEC-7 | Coordinator authorization of execution, including the trusted-path and Owner no-touch preconditions | not met |
| EXEC-8 | any Architect review required by the workflow | not met |

```text
CT21D_EXECUTION_READY = FALSE        CT21D_EXECUTION = NOT_AUTHORIZED
```

## 24. OWNER ACT 2 EVIDENCE REQUIREMENTS

Before the Owner is asked for final acceptance, the following must exist and have been reviewed (no later than the replacement Freeze):

1. admission results for the characterized tuple (which controls are measurable, which are not, and any reviewed amendments);
2. the residual scan characterization (section 10.6), including missed transient changes, the false terminal failure rate, A-1 and A-2;
3. false-rejection behaviour (availability cost) from baseline runs;
4. UNDO evidence (section 16) and SAVE/reopen equality (group `S1`);
5. Selective rollback host evidence (document-authority evidence of the AUTH-15 kind for the Selective seam);
6. interactive-host confirmation (section 2.4);
7. post-CP characterization (section 10.7);
8. limits and non-guarantees restated (V5 `NG-01..NG-20`, W-scan residual, W3);
9. every reviewed amendment made after Owner Act 1;
10. the replacement Freeze draft.

No decision is prepared here.

## 25. OPEN BLOCKERS AND STATUS

### 25.1 Blockers

| Id | Status |
|---|---|
| B1, B2, B7 | `CLOSED_BY_V3_CONTRACT` |
| B3 | `CLOSED_BY_V4_CONTRACT` |
| B4, B8 | `CLOSED_BY_V2_CONTRACT` |
| **B5** | `ADVANCES_TO_IMPLEMENTATION_PREREQUISITE`: the **authority design** is drafted (`COMPLETE_FOR_BASELINE`, section 15, pending Coordinator and Architect agreement). The blocker **persists** as `REMAINS_BLOCKER_FOR_CT21D_EXECUTION` (implementation and integration of P-1..P-7, EXEC-3) and as `REMAINS_BLOCKER_FOR_REPLACEMENT_FREEZE` (host evidence P-8). If the reviewers do not agree the design, B5 reverts to `REMAINS_BLOCKER_FOR_CT21D_DESIGN`. |
| **B6** | `REMAINS_BLOCKER_FOR_ACT2` (plan designed in section 16; `UNDO_EVIDENCE_STATUS = NOT_EXECUTED`) |

### 25.2 Open questions for the Coordinator and the Architect

| Id | Question |
|---|---|
| CQ-1 | the mapping of group verdicts to predicate states (section 1.1), in particular `FALSE` for class-A failures |
| CQ-2 | the `DRAFT-PARAMETER` values (repetition policy, time budgets, baseline run counts, maximum retry attempts) |
| CQ-3 | the two-stage event model (learning and validation) of section 14.3 as the way to satisfy `EXPECTED_MUTATION_EVENT_SET` determinism |
| CQ-4 | `PRELOAD_SET` as the requirement to make E-11M satisfiable (section 13) |
| CQ-5 | `MODIFY_PREEXISTING = PROHIBITED` for Selective V1 (section 11.3) |
| CQ-6 | `MVID_CONSISTENCY` as an added E-10 field (section 12.2) |
| CQ-7 | the requirement to cover the `Commit()` exception path only at T1 (section 16, U-6) |

### 25.3 Final status

```text
OWNER_ACT1 = SPONSORED     OWNER_DECISION_STATUS = SPONSORED (pursuit only)
CT21D_AUTHORITY_CONTRACT = DRAFT_V1
CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
CURRENTLY_ADMISSIBLE_KIND_SCOPE = NONE     ALT21D_ADMISSION_PASS = NOT_YET_SATISFIABLE
SELECTIVE_AUTHORITY_DESIGN_STATUS = COMPLETE_FOR_BASELINE (DRAFT; pending agreement)
CTDA_HOST_PASS = FALSE (terminal)     CIA = UNKNOWN     SafeOperationalState = FALSE_FOR_ADMISSION     G3 = STOPPED     G3B = NOT OPEN     CT-50 = NOT EXECUTED
ALT-21E = FALLBACK_IN_EFFECT     Freeze V18 = GOVERNING     Freeze V35 = GOVERNING ITS OWN PREDICATE     Replacement Freeze = DOES NOT EXIST     ADR-0036 = PROPOSED / AMENDED FOR V18
HOST VALIDATION = NOT RUN     BRANCH = NOT RECONCILED
NEXT GATE = COORDINATOR REVIEW OF CT-21D AUTHORITY CONTRACT V1
```
