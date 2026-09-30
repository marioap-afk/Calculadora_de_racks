# SA-1 task input: contract clause fragments (verbatim)

Extracted mechanically from the contract files at branch commit `1c3b7a9c` (blobs unchanged since `d190ec09`). This is the **only** document the independent compiler received besides its prompt.

---

<!-- FRAGMENT: V2.1 4.2 -->

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

---

<!-- FRAGMENT: V2.1 7 and 7.1 -->

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

---

<!-- FRAGMENT: V2.1 15.2-15.7 -->

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

---

<!-- FRAGMENT: V2.1 19.1 -->

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

---

<!-- FRAGMENT: V2.1 27.2 -->

### 27.2 Procedure

| Step | Action |
|---|---|
| 1 | **seam operation inventory**: every database-mutating primitive that SL-1, SL-2, SL-4 and SL-5 use (append of an entity, creation of a block definition, entity modification, erasure, extension-dictionary creation, `Xrecord` data set, block-reference append, symbol-table record addition, dynamic-property set, `RecordGraphicsModified`, and any other explicit category the inventory shows). The inventory is a **starting input, not the completeness authority**; it must pass the independent completeness review (27.7). No primitive may be used that has no micro-scenario |
| 2 | **learning by micro-scenarios**: each operation category is executed **in isolation** in the controlled environment (manifest-attested, no adversarial probes) to map it to its host events; several isolated repetitions across independent sessions |
| 3 | **draft model**: entries as in the EVM schema (27.6) |
| 4 | **explanation review**: every entry carries an explanation of one of three types: `PRODUCT_OPERATION`; `HOST_DOCUMENTED` (with a citation); or `HOST_OBSERVED_STABLE`. **`HOST_OBSERVED_STABLE` may appear only as accompanying host behaviour associated with a product operation** (a companion event of that operation), must carry raw evidence, a citation to the isolated micro-scenarios that show it stable across independent sessions, an Architect review and a **risk label**; it is **never a free-standing permission**. **Unexplained events are recorded as `UNEXPLAINED_EVENTS` and block the freeze**; they are never admitted automatically |
| 5 | **freeze**: the model gets a version and a hash; status `DRAFT`, `UNDER_REVIEW` or `FROZEN(hash)`; only `FROZEN` models are used by governing validation |
| 6 | **validation**: governing runs on fresh sessions and fresh fixtures, with plans **structurally outside** the learning corpus (27.8), apply the frozen model |

---

<!-- FRAGMENT: V2.2 PA-6 -->

## PA-6 — Torn-read check and original-source reads

The check of a **torn acquisition is mandatory**. Step 2 of V2.1 19.1 is replaced by the following; the acquisition authority is **one of two**, recorded in the `LibraryInputRecord` as `ACQUISITION_AUTHORITY`:

| Authority | Rule |
|---|---|
| **A** | the source handle is **held with writes denied** for the **entire streaming acquisition** |
| **B** | after the streaming, the original source is **re-hashed** and the hash **must equal the hash of the private-copy acquisition** |

If neither A nor B can be established, or B does not match, the acquisition is `UNKNOWN` and the run is refused (O1) under the existing authority of V2.1 19.1 (step 2 and the UNKNOWN row).

**Two kinds of read of the original source are distinguished:**

| Kind | Rule |
|---|---|
| `ORIGINAL_SOURCE_IMPORT_READ` | **FORBIDDEN** once the authoritative private copy exists. Any such read is `AUTHORITY_INVALID` (V2.1 19.1) |
| `ORIGINAL_SOURCE_VERIFICATION_READ` | **ALLOWED** for the torn-read verification (authority B) and for the observation of drift after the acquisition |

**Verification reads never feed the library `Database`.** Drift of the original source after the authoritative acquisition remains **informational only** (`SOURCE_DRIFT_AFTER_ACQUISITION`, V2.1 19.1). The authority rule of V2.1 19.1 ("any reread of the original source for import") is read as forbidding `ORIGINAL_SOURCE_IMPORT_READ` only. Q-ACQ-1 is closed by this clause.

---

<!-- FRAGMENT: V2.2 PA-10 -->

## PA-10 — E-12 phase-A adjudication domain (replaces V2.1 section 14.8)

Phase A occurs **before** the sealed set exists, so its domain is **not** defined as "the `PROTECTED_OBJECTS` known at PV". The domain of adjudication of phase A is:

- the **protected objects determinable from the plan**: the source objects, the relevant containers, the NOD entries the kind consumes;
- **objects created inside `T_M`** (listed by handle in the seam results).

| Phase | Adjudication domain | Outside the domain |
|---|---|---|
| A. MUTATION | the domain above | recorded, **not adjudicated** by itself (V3 4.2 row E) |
| C. W-SCAN | events on `PROTECTED_OBJECTS` (expected write set = empty) | recorded, not adjudicated |

**At PV** the sealed `PROTECTED_OBJECTS` is checked for **consistency with the earlier domain**: a member of the sealed set that the domain did not include is an inconsistency and aborts before `Commit()`.

- An event whose **target cannot be resolved**, or that matches more than one open entry, remains **ambiguous** (V2.1 14.5) and fails closed.
- **During validation, an event category that the frozen EVM does not explain is `EVM_INCOMPLETE`**, which requires a **new model version, review and revalidation** (V2.1 27.5). It is not a host verdict.
- This clause clarifies V3 4.2; it does not change any phase policy. Q-EVT-1 is closed by this clause.

---

<!-- FRAGMENT: V2.4 section 1 -->

## 1. Replacement text (the Architect's pre-approved text, verbatim)

> The baseline requires the seam-operation inventory to be `STATIC_REVIEWED`: conditions 1, 2, 5, 6 and 7 of V2.2 PA-11 satisfied for the design-time inventory (the reused product path at a bound SHA plus the operations required by the seam contract) and the dynamic-trace plan of conditions 3 and 4 approved. The inventory becomes `REVIEWED` (complete) only after the dynamic trace covers every seam entry point and every plan-shape class on the implemented seams, and that is required before the first `LEARNING` run. `INVENTORY_COMPLETENESS = NOT_ESTABLISHED` until `REVIEWED`. An operation added by the implemented seams after the design-time review reopens the review of its operation class.
