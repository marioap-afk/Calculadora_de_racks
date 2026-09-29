# I-52 — Proposal ALT-21D V2: reduced-guarantee contract, revised after the Architect ruling on V1

> **PROPOSAL ALT-21D V2 — DRAFT FOR ARCHITECT REVIEW. Architecture / documentation only.**
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
> SUBSTANTIVE IMPLEMENTATION = BLOCKED
> ```
>
> None of these statuses changes with this document. This is **not** an Owner selection of ALT-21D and requests **no** Owner
> decision. Nothing here authorizes AutoCAD, a host run, `CT-21D`, product code, a Freeze, an ADR amendment or a reopening of G3,
> and nothing here designs the executable `CT-21D` harness.

## 0. What V2 is

V2 is a new document. **V1 (`324c0cbdd98e32b4995a9bd4469717926a0d9ef0`) is not edited** and stays as the historical text that the
Architect reviewed. V2 incorporates every `REQUIRED_CHANGE` of the ruling `AGREED_WITH_REQUIRED_CHANGES` and adds nothing else.
Unchanged material of V1 is incorporated **by reference** where stated (identities, section 1.2 authorities table, section 11
stale conclusions); everything that decides the contract is restated here.

### 0.1 Disposition of the required changes

| Ruling item | Change | V2 section |
|---|---|---|
| S1 | completion point redefined; W-scan, W3a, W3b named; stability mechanism; no product writes after PC | 3 |
| S2 | guarantee-bearing controls separated from risk-reduction controls; E-11 split; E-12 redefined; new non-guarantees | 4, 5, 6 |
| S3 | PREPARE / MUTATION / VERIFY phase model; library input attested; PREPARE residue declared | 7 |
| S4 | orchestrator owns lock and transaction; AUTH-15 does neither; `LOCK_MODE` unfixed | 8 |
| S5 | kind scope: maximum `SELECTIVE_ONLY`, currently admissible `NONE` | 9 |
| S6 | outcomes O4 renamed, O5 kept, `ABORT_UNVERIFIED` added; UNDO precondition; provisional guidance marked | 10, 11 |
| S7 | product semantics matrix rebuilt in four columns | 13 |
| S8 | `04NO-S` design restrictions | 12 |
| S9 | Owner sequence corrected | 17 |
| S10 | manifest identity; unfixed parameters enumerated | 14, 15 |
| W1 / W2 / W3 | `FAILURE_COMMITTED_DIVERGENT`; "known module set attestation"; sequence aligned | 5, 10, 17 |

### 0.2 Blockers B1..B8

`RESOLVED_IN_V2_CONTRACT` means the *contract text* is now defined; it does **not** mean that code or evidence exists.

| Id | Blocker (Architect ruling) | Status | Note |
|---|---|---|---|
| B1 | RED-3 / scan interval / W3a-W3b | `RESOLVED_IN_V2_CONTRACT` | definition in section 3. The adequacy of the two-observation scan is **not** demonstrated and is carried as `TBD-09` (`DEFERRED_TO_CT21D_AUTHORITY_CONTRACT`) |
| B2 | policy controls mixed with guarantee-bearing controls | `RESOLVED_IN_V2_CONTRACT` | sections 4-6. Measurement authority for E-10, E-11M, E-12 stays open as `TBD-04..06` |
| B3 | PREPARE / MUTATION and library/cache | `RESOLVED_IN_V2_CONTRACT` | section 7. Cache-freshness rule is a product-gate parameter, `TBD-10` |
| B4 | lock / transaction ownership | `RESOLVED_IN_V2_CONTRACT` | section 8. `LOCK_MODE` is `TBD-03` |
| B5 | Selective caller-owned rollback authority does not exist | `REMAINS_EXTERNAL_BLOCKER` | needs code and host evidence outside this document (section 9) |
| B6 | UNDO authority does not exist | `REMAINS_EXTERNAL_BLOCKER` | needs physical evidence; contract requirement recorded in section 11 |
| B7 | PRESERVED marks contradicted by non-guarantees | `RESOLVED_IN_V2_CONTRACT` | section 13 |
| B8 | Owner / ADR sequencing | `RESOLVED_IN_V2_CONTRACT` | section 17 |

## 1. Governing baseline (bound, not changed)

Identities of V1 section 1 apply unchanged, plus:

| Authority | Identity / status |
|---|---|
| Proposal ALT-21D V1 | `324c0cbdd98e32b4995a9bd4469717926a0d9ef0`, blob `bcb44b5e85464425cd45754b233e5bc6d4682043`; historical, not edited |
| Architect ruling on V1 | `AGREED_WITH_REQUIRED_CHANGES`; conclusion B ("requires changes before it can be considered viable") |
| I-52 branch | parent of V2 is `324c0cbd`; 79 behind / 119 ahead of `origin/main` (`3375aadbbf929427a6d106b2fff275d64863b89c`); zero `src/` or `tests/` delta; **not reconciled** |
| Freeze V18 | `faaf709bf6401dcda91b07f41afe44f490fb916a`, `GOVERNING` |
| Decisions | `docs/automation/decisions/I-52.md` sections 117, 196-199 |
| Shared authorities on `main` | V1 section 1.2 table incorporated by reference. **Corrections stated in this V2:** AUTH-15 covers HeaderRun and Cantilever only and does not acquire the lock (sections 8, 9); none of I-57, I-58, I-59, I-49, AUTH-15 observes or excludes an environment channel |

## 2. Replacement authority and the relationship to CIA

```text
ContextIsolationAuthority is STILL REQUIRED for the ORIGINAL guarantee (G0.1, G0.2, G0.4-as-prevention) and stays UNKNOWN.
ALT-21D defines a DIFFERENT guarantee (the Reduced Guarantee, RG).
A narrower authority may replace CIA ONLY for RG, and ONLY after every condition in 2.3.
```

### 2.1 The original guarantee (unchanged from V1 section 2.1)

`G0.1` execution only when `ContextIsolationAuthority = TRUE`; `G0.2` every channel excluded, detected with complete semantics or
modelled; `G0.3` `OverallSuccess` (V22 section 4); `G0.4` no foreign write / commit-time mutation / secondary-transaction survival can
yield a reported success over a divergent result in `[S, completion]` (DA-P1..DA-P10); `G0.5` all-or-nothing, UNDO, SAVE/reopen; `G0.6`
fail-closed on unknown environments. `G0.2` and `G0.4` as prevention cannot be supported (CIA research `PARTIAL_ONLY`, `CT-49R = NOT READY`;
`CTDA_HOST_PASS = FALSE`).

### 2.2 The reduced guarantee RG, stated as claims

RG makes exactly these claims, and no others:

| Claim | Statement |
|---|---|
| RG-1 | RACKMIRROR starts a run only when `ALT21D_ADMISSION_PASS` holds (section 15). Every other state is unsupported |
| RG-2 | If the run reports `SUCCESS`, two agreeing fresh observations of the destination authority, the materialization and the source read-set, taken during the verification interval `W-scan`, equal the expected values, and the guarantee-bearing controls hold at the contractual completion point `CP` |
| RG-3 | Before `Commit()`, any failure or detected drift aborts and rolls back the product's own `MUTATION` writes, and the abort is verified by a state read |
| RG-4 | After `Commit()`, the run never reports success unless RG-2 holds; otherwise it reports a terminal non-success outcome (section 10) and performs no compensating write |

RG **does not** claim isolation, absence of other code, absence of interference, stability of the drawing during `W-scan`, persistence of
the result after `CP`, or a supported recovery path (section 11).

### 2.3 Conditions for RG to replace CIA (all required, none satisfied)

1. this Proposal agreed by Coordinator and Architect;
2. an explicit Owner sponsorship/redecision that supplements O-1 V18 (Act 1, section 17);
3. a `CT-21D` authority contract that fixes every parameter listed in section 15;
4. evidence produced under that contract in the interactive host;
5. an exact review of that evidence;
6. a replacement Freeze that states which V18 clauses it supersedes and for which contract;
7. explicit Owner final acceptance of RG (Act 2, section 17).

### 2.4 Naming

The authority that RG relies on is named `ALT21D_ENVELOPE_AUTHORITY`. It is **not** CIA, **not** `SafeOperationalState` (V1's
`SafeOperationalState_D` is withdrawn) and **not** an equivalent of `CTDA_HOST_PASS`. CT-DA is not evidence of CIA and is not evidence for
RG. The module-set part of the authority is named `KNOWN_MODULE_SET_ATTESTATION` (section 5).

## 3. Temporal model (RED-3 redone)

### 3.1 Phases and checkpoints

```text
PREPARE            under the document lock; pure computation, AUTH-12 reads, library imports (section 7)
MUTATION           ONE caller-owned transaction T_M; source read S, destination writes, staged reads, pre-commit
Commit()           the single commit of T_M
VERIFY/FINALIZE    non-DB post-commit actions, then W-scan, then continuity evaluation, then reporting
ABORT-VERIFY       alternative to Commit(): Abort() of T_M, then a state read (section 10)
```

| Cp | Instant |
|---|---|
| P0 | command entry, before the lock; no product write exists |
| PL | lock acquired |
| PP | end of PREPARE; PREPARE outputs recorded |
| PS | inside `T_M`, at the source authoritative read `S`, after re-observing the PREPARE outputs |
| PV | after all destination writes and the staged `R_pre`/`M_pre` reads, immediately before `Commit()` |
| V0 | start of the first fresh read of the post-commit verification pass |
| V1 | end of the last fresh read of that pass |
| PC | the continuity evaluation performed at V1, after the last state read |
| CP | the contractual completion point = PC |

### 3.2 W-scan

- **`W-scan` begins** at `V0`: the first read of the first fresh read-only transaction opened after `Commit()` returned (and after any non-DB
  post-commit action, 3.4).
- **`W-scan` ends** at `V1`: the end of the last read of the confirming pass.
- **Verification pass.** *Pass 1* reads, in a fixed order, every element of the destination authority (all siblings, per kind), the
  destination materialization, the NOD registry when the kind consumes it, and the source read-set, and evaluates them with the independent
  comparator. *Pass 2* re-reads **every element read in pass 1**, in the same order, and compares each with its pass-1 value **and** with
  the expected value. The continuity controls (section 4) are evaluated **after** the last read of pass 2, at V1.
- **What the mechanism detects:** an element whose value differs between the two observations, or from the expected value, or a continuity
  fact that changed. **What it does not detect:** an element changed and restored between its two reads (`W4`), and an element changed after
  its own pass-2 read (`W-scan tail`).
- **No host authority for stability.** There is no global database version and no mechanism that suspends same-context callbacks (V22 section 9,
  option R-B: not available; CIA research). Stability *during* `W-scan` is therefore **not demonstrated**; `TBD-09` records that `CT-21D` may
  characterize whether the two-observation scheme detects the interleavings it is meant to detect. Until then the residual is declared
  (`NG-12`) and belongs to what the Owner would be asked to accept.

### 3.3 The exact condition for `SUCCESS`

`SUCCESS` is reported iff **all** hold:

1. `ALT21D_ADMISSION_PASS` at P0 and the phase-model conditions of section 7 at PP and PS;
2. `ALT21D_CONTINUITY_PASS` at PV;
3. `Commit()` was accepted;
4. pass 1 equals the expected value for every element;
5. pass 2 equals the expected value **and** its pass-1 value for every element;
6. `ALT21D_CONTINUITY_PASS` at PC (guarantee-bearing **and** risk-reduction controls, section 4), with no UNKNOWN.

Any observed difference from the expected value is `FAILURE_COMMITTED_DIVERGENT`. Any inability to complete a read, any UNKNOWN, or a
risk-reduction control drifting while the state agrees is `COMMITTED_UNVERIFIED`. There is no partial success and no success-with-warning.

### 3.4 After PC

- **Product writes after PC are prohibited.** No database write, no `Xrecord`, no status object, no graphics-modified flag, no save.
- After `CP` the product only reports to the user and returns. Anything the product needs to do that is host-visible and not a database write
  (regen, redraw) is executed **before** `V0`, so that its callbacks fall inside `W-scan` and are observed.
- The lock is held from PL through V1 and released after `CP`.

### 3.5 Residual windows after V1

| Window | Definition | Status |
|---|---|---|
| `W-scan tail` | per element, from its pass-2 read to V1 | not covered |
| `W3a` | from `CP` until the host reports the command finished (host command-end processing), i.e. callbacks and deferred effects attributable to this `Commit()` or to the end of this command | outside the guarantee; `CT-21D` shall **characterize and report** what runs here (`TBD-11`), not close it |
| `W3b` | any independent modification after the command finished | outside the guarantee by definition; not attributable |
| `W4` | a change that reverts before the next read | not covered |

`W3a` is separated from `W3b` because V22 T8/DA-P6 treated callbacks between the last read and command completion as **in scope** of the original
guarantee. RG places both outside the guarantee; the separation keeps the Owner from reading `W3a` as "ordinary later editing".

## 4. Two classes of control

Both classes are fail-closed for admission and for `SUCCESS`: FAIL or UNKNOWN in either blocks. They differ in **what the guarantee claims**.

- **Class A — controls that carry the guarantee.** RG-2, RG-3 and RG-4 are statements about these controls. Their loss removes the claim.
- **Class B — risk-reduction controls.** They make a false success less likely and give diagnostics. They prove **no** isolation. A passing
  class-B control never strengthens a claim and never turns a failing class-A comparison into PASS. Their measurement imperfection is a
  declared non-guarantee, not a hidden premise.

| Class | Controls |
|---|---|
| **A1 state comparison** | S-1 `R_pre`/`M_pre` at PV; S-2 pass 1; S-3 pass 2; S-4 read-set re-observation at PV and PC (E-08); S-5 abort verification (section 10) |
| **A2 exclusion by measured fact** | E-01 HostExact; E-02 SingleDocument; E-03 LockHeld; E-04 SingleRackCadCommand; E-05 KnownModesInactive; E-06 TransactionOwnership; E-07 DatabaseComplete; E-09 OverrulesBounded; E-14 KindReadiness; E-15 ContractBinding |
| **B risk reduction** | E-10 KnownModuleSetAttestation; E-11M ManagedLoadContinuity; E-12 EventFenceConformance; E-13 ProfileEquality |
| **not a control** | E-11N native load continuity: no authority; declared residual `NG-10` |

## 5. The controls, revised

Column **Open** lists parameters that are not yet fixed (section 15). Status: **DOC** documented contract or behaviour in the CIA research or
G3A; **G3A** characterized in G3A on the exact tuple in **Core Console**; **NEW** no evidence in the repository. No status is sufficient
by itself; every control needs interactive-host confirmation under a future authority contract.

| Id | Cl. | Observable fact | Authority / how measured | PASS | UNKNOWN | Failure action | Cp | Open | Status |
|---|---|---|---|---|---|---|---|---|---|
| E-01 HostExact | A2 | `acad.exe` `R25.0.171.0.0` SHA-256 `2A75996FD2A5C5EE0376FD5BA7CCED227A098193FF495E8CEEDA3A96FE326422`; `AcCoreMgd`/`AcDbMgd`/`AcMgd` `25.0.171.0.0` (G3A hashes); interactive process | host identity properties, process image, file hashes | equals the manifest tuple | any read fails | REFUSE | P0 | TBD-12 | G3A values; NEW interactive |
| E-02 SingleDocument | A2 | exactly one open document; it is the invoking active document; its database is the working database; none opening or closing | document collection count and identity | count = 1, identities equal | unreadable | REFUSE; at PV/PC: ABORT / non-success | P0, PV, PC | TBD-12 (opening state) | DOC object model; NEW |
| E-03 LockHeld | A2 | the orchestrator holds the document lock, in `LOCK_MODE`, from PL through V1 | lock acquisition by the orchestrator; read-back of the held mode | acquired and read back as `LOCK_MODE` | error other than conflict, or unreadable | REFUSE (a conflict is FAIL) | PL, PV, PC | **TBD-03** | DOC (exclusion of other execution contexts only); NEW |
| E-04 SingleRackCadCommand | A2 | only the RackCad command is active; no nested, transparent, script-driven or application-context-queued invocation | `CMDNAMES`, `CMDACTIVE`, registered command flags | equals the admitted set | unreadable | REFUSE / ABORT | P0, PL, PV, PC | **TBD-01** | G3A variables; NEW admitted set |
| E-05 KnownModesInactive | A2 | `REFEDITNAME` empty, `BLOCKEDITOR`, `ARRAYEDITSTATE`, `BLOCKTESTWINDOW` inactive; no long transaction | G3A detectors; long-transaction predicate | all inactive | any read fails | REFUSE / ABORT | P0, PV, PC | TBD-12 | G3A (Core Console); NEW |
| E-06 TransactionOwnership | A2 | no active transaction at P0; while `T_M` is open exactly one, the orchestrator's own (identity equal); no `OpenCloseTransaction`; no product write to any side database | active-transaction count; AUTH-15 top-transaction identity check; static guard | count 0 at P0; exactly 1 own at PV | count unreadable | REFUSE / ABORT | P0, PS, PV | **TBD-02** | DOC (AUTH-15 scope); NEW count |
| E-07 DatabaseComplete | A2 | source and destination databases complete; xrefs complete or out of the read set | V17 PRE-19 checks | complete | incomplete | REFUSE / ABORT | P0, PS | none | preserved |
| E-08 ReadSetObservation | A1 | the mirror read-set (source authoritative read at `S` plus the I-49 `PlanReadSet` observations actually used) is re-observed at PV and at PC and equals the PS observation | I-49 (project variables only; no global snapshot); mirror read-set stays with I-52 | identical | a re-read fails | ABORT (PV) / DIVERGENT or UNVERIFIED (PC) | PS, PV, PC | none | PS preserved; PV/PC NEW |
| E-09 OverrulesBounded | A2 | no relevant overrule family with unknown effect on the subjects and classes consumed | `Overrule.HasOverrule(subject, RXClass)` | none relevant | relevant unknown | REFUSE / ABORT | P0, PV | none | G3A (Core Console) |
| **E-10 KnownModuleSetAttestation** | **B** | every member of the **enumerable** loaded-code sets equals its manifest entry by path and file SHA-256, in two snapshots | linker modules, ARX apps, process modules, managed assemblies; each file hashed | equal, both snapshots equal | an enumeration fails; snapshots differ; a member has no readable file (including in-memory assemblies) | REFUSE | P0 | **TBD-04** | DOC partial; NEW |
| **E-11M ManagedLoadContinuity** | **B** | the managed assembly set is unchanged from P0 to V1 and no managed load event occurred | snapshot equality at P0, PV, PC; managed assembly-load events | equal, zero events | subscription fails | ABORT (PV) / UNVERIFIED (PC) | PV, PC | **TBD-05** | NEW |
| **E-12 EventFenceConformance** | **B** | every observed event in the subscribed set is deterministically attributable to an entry of the expected set for the current phase (5.2) | subscription to the events listed in `TBD-06` | all events attributable; none unattributable | subscription fails; any event not deterministically attributable | ABORT (PV) / UNVERIFIED (PC); **never** infer authorship from absence or timing | P0..PC | **TBD-06** | NEW |
| E-13 ProfileEquality | B | `SECURELOAD` and `TRUSTEDPATHS` equal the manifest-declared values; the product never writes them | system variables | equal | unreadable | REFUSE | P0 | TBD-12 | DOC (reduces load origin only) |
| E-14 KindReadiness | A2 | for every selected kind: reader, writer, independent authored comparator, sibling authority, **caller-owned creation with rollback authority**, round-trip and save/reopen proof | registry plus per-kind evidence record | ready | unknown kind | REFUSE the whole invocation | P0 | **TBD-08** | see section 9; **never PASS today** |
| E-15 ContractBinding | A2 | the manifest names this contract version, the replacement Freeze, the `CT-21D` record for the exact tuple and the recorded Owner acceptance | manifest and records | all present and consistent | missing | REFUSE | P0 | none | NEW; does not exist |

### 5.1 E-10 is an attestation of *known* modules, not a proof about other code

`KNOWN_MODULE_SET_ATTESTATION` claims one thing: the **enumerable** loaded-code sets, at the snapshot instants, equal the manifest by file hash.
It does **not** claim that no other code exists or can run. Specifically:

- a manifest of known modules is not a proof of absence of every other code;
- the SHA-256 of a **file** on disk is not automatic proof of the identity of the **image already loaded** from it (`NG-16`);
- channels the enumerations do not list remain open: LISP, VBA, in-process COM, reflection-loaded code (`NG-17`);
- modules loaded as data files and native code that no enumeration reports remain open (`NG-18`).

The phrase "closed-world" is withdrawn.

### 5.2 E-12 redefined: expected sets, no inference of origin

An event does not identify its origin. E-12 therefore never says "this event was foreign".

- Before each phase the product derives, deterministically from its plan, an **expected set** `X_phase` of entries `(event kind, target class or handle, phase)`
  that its own operations are specified to produce, each consumable once.
- Every observed event of the subscribed set is either **matched** to an entry (attributable) or **unmatched**. An unmatched event, or an event
  that could match more than one entry, or whose phase cannot be determined, is **UNKNOWN / FAIL-CLOSED**.
- The control does not infer authorship from the absence of an event, from timing, or from ordering alone.
- **Blind spot, declared as `NG-19`:** a foreign same-context event that coincides with an expected entry is indistinguishable from the product's own and is
  not detected by E-12; only the state comparison can detect its effect.
- **Cost, declared:** the host may emit events the product did not predict (derived updates, dynamic-block or field effects). Under this rule they
  are UNKNOWN and block. Whether `X_phase` can be predicted deterministically for the exact tuple is `TBD-06`. If `CT-21D` shows it cannot, E-12 is
  not measurable and moves to non-guarantees **by an amendment of this Proposal reviewed by the Architect**, never silently.

The same rule applies to E-10 and E-11M: if the future authority contract shows a class-B control cannot be measured, the change is an explicit,
reviewed amendment. A control is never dropped by leaving it UNKNOWN forever.

### 5.3 E-11N — not a precondition

Native load and unload continuity has **no** measurement authority in a managed-only plugin: the linker notification is a native mechanism that
does not exist in the product, and introducing it changes the product architecture. V2 therefore does **not** keep a predicate that is designed to
be permanently UNKNOWN. E-11N is declared **non-guarantee `NG-10`** (native load/unload continuity, and transient loads of either kind between
snapshots). It can return only through a future, separately reviewed proposal of a native authority.

## 6. Explicit non-guarantees

| Id | Not guaranteed | Behaviour | Why |
|---|---|---|---|
| NG-01 | universal isolation; same-context behaviour of attested code | E-10 identifies enumerable code; E-03 excludes only other execution contexts | no contract that same-context callbacks are excluded; identified is not certified |
| NG-02 | external automation (COM, LISP, VBA, script) | other-context writers must take the lock; same-context ones are not enumerated | no inventory of automation channels |
| NG-03 | multiple documents | UNSUPPORTED at P0 | shared heap and statics across document contexts |
| NG-04 | modeless interference | not enumerable; only E-03/E-12 and the state comparison | no modeless registry |
| NG-05 | concurrent, nested, transparent commands started **during** the run | E-04 sees visible cases at checkpoints only | `CMDNAMES`/`CMDACTIVE` are boundary signals |
| NG-06 | background callbacks (idle, timers, synchronization, COM events) | not enumerable; state comparison only | no public enumeration |
| NG-07 | persistent modes unknown to the detector list | not modelled | no complete mode registry |
| NG-08 | application-context cases | UNSUPPORTED: only a registered document-context command | `16A-S` and `13A-SM` never executed |
| NG-09 | `OpenCloseTransaction`; side-database rollback; a foreign secondary transaction that commits a protected mutation | the product uses no OCT and writes no side database; a foreign committed mutation survives our abort | AUTH-15 Option B scope; DA-P3 UNKNOWN |
| NG-10 | native load/unload continuity (E-11N); transient loads of either kind between snapshots | not measured | no native authority; snapshots are fallible |
| NG-11 | post-commit rollback | none | a compensating write can fail or fire callbacks |
| NG-12 | the `W-scan tail`, `W3a`, `W3b`, `W4` | outside the guarantee (3.5) | DA-P6 not proven; no host mechanism |
| NG-13 | hostile or memory-patching code; tampering with manifest or binary | out of the threat model | V22 section 28 |
| NG-14 | absence of bugs in attested modules | out of scope | not measurable |
| NG-15 | anything CT-DA left structurally UNKNOWN | not relied on (section 12) | rows 5-14 not executed; `04NO-S` UNKNOWN |
| NG-16 | that a loaded image equals the file whose hash was checked | E-10 hashes files | image vs file identity has no authority |
| NG-17 | unenumerated channels: LISP, VBA, in-process COM, reflection-loaded code | not observed | not in any enumeration |
| NG-18 | data-file modules and native code no enumeration reports | not observed | documented omission of the enumeration API |
| NG-19 | a foreign event that coincides with an expected event entry | not detected by E-12 | events carry no origin (5.2) |
| NG-20 | PREPARE residue and shared library-cache state | declared (section 7); the cached library database is process state outside the document | AUTH-12 queries share `BlockLibraryDatabaseCache` |

## 7. Phase model: PREPARE, MUTATION, VERIFY/FINALIZE

### 7.1 "Single transaction" is a statement about MUTATION only

`ONE caller-owned transaction, ONE Commit` applies to **MUTATION** (`T_M`). It does not describe PREPARE, which performs library imports in
transactions that commit by design, nor VERIFY, which uses fresh read-only transactions.

### 7.2 PREPARE

Runs after PL, under the lock, before `T_M` is opened. It may perform the pure computation, the AUTH-12 presence queries and the library imports.

- **Callee transactions.** The AUTH-12 queries and imports open their own transactions on the document database or on the cached library database and
  commit them. They are callee-owned, not part of `T_M`. E-06 requires the active-transaction count to be 0 before each of them starts and after each ends,
  and 0 when `T_M` is opened.
- **Library input identity.** For every run the product records a `LibraryInputRecord`: library file path and file SHA-256 read in this run, and the identity of the
  cache entry that served the read (created-from hash or file identity). If the cache entry cannot be shown to correspond to the recorded file hash, freshness is
  not established: UNKNOWN, refuse. The freshness rule itself is `TBD-10`.
- **AUTH-12 semantics.** `Unknown` never equals `Ok`; an unreadable library is UNKNOWN and refuses.
- **PREPARE outputs** recorded at PP: imported definition names and a fingerprint of each imported definition, the `LibraryInputRecord`, the plan hash, and the AUTH-12 results.
- **Re-observation at PS.** Every PREPARE output that influences `MUTATION` is re-observed inside `T_M` at PS: imported definitions still present with the recorded
  fingerprint, library file hash unchanged, plan hash equal. Any difference aborts (O2).
- **Persistent effects of PREPARE (residue).** Imported library block definitions in the document database, their undo-stack steps, and the state of the process-wide
  library cache. These carry no `RackId`, payload or views (V21 section 17). They are **not** atomic with `T_M`, are **not** removed by an abort, and are stated in every
  outcome that follows a PREPARE write.
- **Reading is not writing.** Reading the cached library database is a read of an input. It is not a product write to a side database, and it does not contradict
  "one execution context". It is attested (identity above) and not left outside every comparison.

### 7.3 MUTATION

The orchestrator opens `T_M` (section 8), reads `S`, re-observes the PREPARE outputs, writes the destination authority and materialization, performs the staged
`R_pre`/`M_pre` reads and evaluates PV. Then `Commit()` or `Abort()`.

### 7.4 VERIFY/FINALIZE

Non-DB post-commit actions, `W-scan` (section 3), PC evaluation, user report. No product write after PC.

## 8. Lock and transaction ownership

```text
LOCK_MODE = TO_BE_FIXED_BY_CT21D_AUTHORITY_CONTRACT
```

The **orchestrator** (the command layer that calls the seams) is the only owner. It:

1. acquires `LockDocument` (PL) before opening any transaction it owns;
2. opens the `MUTATION` transaction `T_M`;
3. calls seams with a caller-owned `Database` and `Transaction`;
4. performs `Commit()` or `Abort()` and disposes `T_M`;
5. keeps the lock through V1 and releases it after `CP`.

**AUTH-15 (`RackDefinitionCreator.CreateInTransaction`)** acquires no lock, opens no transaction, performs no `Commit()`, no `Abort()`, does not dispose the caller's
transaction, and only verifies the identity of the top transaction (`TopTransaction.UnmanagedObject`) according to its contract. Any earlier text that read "lock via
AUTH-15" is withdrawn. The seam cannot assert that the lock is held; the orchestrator's E-03 read-back does.

`ExclusiveWrite` is **not** asserted. The product convention is the parameterless `LockDocument()` at about thirty sites, and the AUTH-15 evidence records "a held
`LockDocument`" without a stronger mode. No stronger mode has evidence; whether one is compatible with the command's implicit lock, with the seams and with the
library cache is `TBD-03`.

## 9. Product kind scope

```text
MAXIMUM_PROPOSED_KIND_SCOPE       = SELECTIVE_ONLY
CURRENTLY_ADMISSIBLE_KIND_SCOPE   = NONE
```

- AUTH-15 covers **HeaderRun and Cantilever** only (`RackDefinitionCreator`, the initiative contract). Selective is not covered, and no caller-owned creation seam for
  Selective with rollback authority equivalent to AUTH-15 Option B is evidenced in the repository. That absence is blocker B5 (`REMAINS_EXTERNAL_BLOCKER`).
- The AUTH-15 rollback authority (document database, held `LockDocument`, `StartTransaction`, Abort or Dispose without Commit) is **not** transferable to Selective or to any
  other path.
- The I-58 typed comparators are **contract/code evidence only**. They are not host-admission evidence and are not promoted to it.
- Making Selective admissible would require, all of them **new**: a caller-owned creation seam for Selective views; document-authority host evidence of its rollback;
  AutoCAD sibling capture (I-58 leaves scan completeness to the consumer); per-kind writer, `mu_k`, round-trip and save/reopen proofs.
- Every kind outside the future scope is `UNSUPPORTED` and fails closed at P0. Consequently `E-14` never passes today and `ALT21D_ADMISSION_PASS` is `NOT_YET_SATISFIABLE`.

## 10. Outcome model

`PREPARE residue` means the persistent effects listed in 7.2. Every outcome reached after a PREPARE write states it.

| Outcome | When | Persistent state | Class |
|---|---|---|---|
| O1 `REFUSED` | a control FAIL or UNKNOWN at P0 or PL, or a PREPARE read UNKNOWN, **before any product write** | none | non-success |
| O2 `ABORTED_VERIFIED` | product failure or drift after the first PREPARE write and before `Commit()`. If `T_M` was opened: `Abort()` done and ABORT-VERIFY confirmed the expected pre-command state for every `T_M` write. If the failure occurred in PREPARE before `T_M` opened: no `T_M` state exists and only the residue is stated | none from `T_M`; **PREPARE residue may persist**; a foreign committed secondary mutation is `NG-09` | non-success |
| O3 `SUCCESS` | section 3.3 | complete; PREPARE residue is part of the intended result | success |
| O4 `FAILURE_COMMITTED_DIVERGENT` | `Commit()` accepted and an observation differs from the expected value | present and divergent from the expected state | terminal non-success |
| O5 `COMMITTED_UNVERIFIED` | `Commit()` accepted and verification could not complete, produced UNKNOWN, or a class-B control drifted while the state agreed | present, unknown relative to expected | terminal non-success, distinct from success |
| O6 `ABORT_UNVERIFIED` | `Abort()` done (or `T_M` failed) and the state read cannot confirm the expected pre-command state (read failed, or residue found; the reason is recorded) | unknown or residue | terminal non-success, unverified family |

- The name `FAILURE_COMMITTED_DIVERGENT` replaces `FAILURE_COMMITTED_CORRUPT`: what is demonstrated is divergence from the expected state, which can also come from a
  defect of the comparator; "corrupt" is not claimed.
- O5 and O6 form the **unverified family**: never success, never "rolled back" by assumption.
- No outcome is derived from a callback. Rollback and abort are proven only by a state read.
- The command performs **no compensating write and no automatic save** in any outcome.
- Report text after O4, O5, O6: the state, the reason, the divergent or unreadable elements, and the operational guidance in 11.2.

## 11. UNDO

```text
UNDO = NEEDS NEW AUTHORITY
```

### 11.1 Requirement recorded

Physical evidence that one UNDO removes the payload and all views of a success (and states the effect on PREPARE residue) is a **precondition to presenting the final
guarantee to the Owner (Act 2)**. It is not a precondition of Act 1. It is not produced in this gate.

### 11.2 Provisional operational guidance

After O4, O5 or O6 the report may add: "revert to the last saved file, or try UNDO", marked **NOT A GUARANTEED RECOVERY PATH**. The product never states that UNDO restores the
previous state and never advertises it as the supported recovery.

## 12. Restrictions carried from `04NO-S`

`04NO-S` remains a governing structural UNKNOWN for `CTDA_HOST_PASS(V35-A3)`. It is not transferred to `ALT21D_HOST_PASS`, is not re-run, and is not evidence for RG.

| R | Restriction |
|---|---|
| R-1 | abort and rollback verification never depend on callbacks delivered after `Abort()` |
| R-2 | the absence of `modifyUndone`, `modified` or `closed` is not a signal of anything |
| R-3 | `cancelled` does not prove that the rollback finished |
| R-4 | the state after `Abort()` is checked by a fresh read (ABORT-VERIFY, S-5) |
| R-5 | inability to verify gives `ABORT_UNVERIFIED`, terminal non-success |
| R-6 | E-12 does not read a missing event as information (5.2) |

## 13. Product semantics matrix

Columns: **A** written correctly during MUTATION; **B** verified after Commit; **C** guaranteed to remain true through the contractual completion point `CP`; **D** after `CP`.
`PRESERVED` is used only where no non-guarantee weakens the cell before `CP`. "Authority available" is stated per row.

| Property | A during MUTATION | B verified after Commit | C remains true through CP | D after CP | Kind / phase / authority available |
|---|---|---|---|---|---|
| One `NewRackId` per logical rack | PRESERVED (RackCad-internal; T0/T1) | REDUCED: independent comparator, read-your-own-writes and consistent scan are NEW | REDUCED: `W-scan tail`, NG-01, NG-06 | OUTSIDE GUARANTEE | kind scope NONE; comparators are code evidence only |
| Authored payload (schema, `ExtensionData`, bindings, custom properties) | PRESERVED only where a caller-owned path exists (HeaderRun, Cantilever); Selective NEEDS NEW AUTHORITY | REDUCED: comparator independence and host reads NEW | REDUCED | OUTSIDE GUARANTEE | B5 |
| Project variables / read-set | PRESERVED (I-49, project variables only) | REDUCED: PV/PC re-observation NEW | REDUCED | OUTSIDE GUARANTEE | NOD writes NEED NEW AUTHORITY (managed fixture never instantiated) |
| Exact common mirror transform (**computed**) | PRESERVED (pure `mu_k`, T0) | not applicable | not applicable | not applicable | host-independent |
| Persisted placement / transform of views | PRESERVED within `T_M` where a caller-owned path exists | REDUCED: physical scan consistency NEW | REDUCED | OUTSIDE GUARANTEE | as above |
| No partial placement | PRESERVED before Commit within `T_M` | REDUCED | REDUCED; after Commit a partial result is reported as O4/O5, not prevented | OUTSIDE GUARANTEE | kind scope NONE |
| All-or-nothing (`T_M` atomic unit) | PRESERVED for the product's `T_M` writes in the document database under a held lock | not applicable | REDUCED after Commit (O4, O5, O6) | not applicable | PREPARE imports are **outside** the atomic unit; NG-09 |
| Rollback on product failure | PRESERVED for `T_M` writes on HeaderRun/Cantilever paths (Option B); Selective NEEDS NEW AUTHORITY | REDUCED: ABORT-VERIFY by state read is NEW | REDUCED: `ABORT_UNVERIFIED` possible | not applicable | side databases, OCT unsupported |
| PREPARE imports / residue | NOT ATOMIC by design: persists after abort | REDUCED: re-observed at PS (NEW) | not covered by rollback | OUTSIDE GUARANTEE | library cache state, NG-20 |
| Unsupported-family fail-closed | PRESERVED | PRESERVED | PRESERVED: decided at P0 before any write | not applicable | registry, host-independent; today refuses every kind |
| UNDO of a success | NEEDS NEW AUTHORITY | NEEDS NEW AUTHORITY | NEEDS NEW AUTHORITY | NEEDS NEW AUTHORITY | B6 |
| SAVE / reopen equality | NEEDS NEW AUTHORITY | NEEDS NEW AUTHORITY | NEEDS NEW AUTHORITY | NEEDS NEW AUTHORITY | physical proof required |
| Multi-document; non-interactive | UNSUPPORTED | UNSUPPORTED | UNSUPPORTED | UNSUPPORTED | E-02; Core Console and APS outside the envelope |

## 14. Manifest identity

The manifest is three-layered. None of the layers proves the absence of other channels (section 6).

| Layer | Content |
|---|---|
| **BUILD-BOUND** | `acad.exe` and API assembly identities; RackCad binaries; the enumerable module and assembly entries by path and file SHA-256; the library file identity; the contract version and the replacement Freeze identity |
| **MACHINE-PROFILE-BOUND** | the machine identity; the AutoCAD profile; the required `SECURELOAD`/`TRUSTEDPATHS` values; the machine's own module entries (drivers and security software vary per machine) |
| **SESSION-EVALUATED** | per run: process identity (PID) and process start time; the snapshots taken at P0, PV, V1; the event log; the `LibraryInputRecord`; the checkpoints of that execution |

A manifest captured in one session is not replayed as evidence for another: the session layer is recomputed for every run.

## 15. The predicate and its unfixed parameters

```text
ALT21D_ADMISSION_PASS iff
      E-01 AND E-02 AND E-03 AND E-04 AND E-05 AND E-06 AND E-07 AND E-08 AND E-09
  AND E-10 AND E-11M AND E-12 AND E-13 AND E-14(k) for every selected kind k AND E-15   are all PASS at P0
  (E-11M and E-12 PASS at P0 mean: armed, baseline snapshot taken, subscriptions established)

UNKNOWN => NO EXECUTION.   FAIL => NO EXECUTION.   No partial PASS.   No "known list + allow otherwise".

ALT21D_CONTINUITY_PASS(cp), cp in {PV, PC}:
      E-01 E-02 E-03 E-04 E-05 E-06 E-08 E-09 E-11M E-12 unchanged and PASS
```

**Structurally finite** (a fixed list of three-valued controls). **Not yet finite in its parameters.** The following are open. Each is
`TO_BE_FIXED_BY_CT21D_AUTHORITY_CONTRACT` unless marked external. Until they are fixed the predicate must not be read as resolved.

| Id | Open element | Affects |
|---|---|---|
| TBD-01 | the admitted value set of `CMDNAMES`/`CMDACTIVE` | E-04 |
| TBD-02 | the API/authority that counts active transactions and its semantics | E-06 |
| TBD-03 | `LOCK_MODE` | E-03, section 8 |
| TBD-04 | the enumeration sources, completeness limits and measurement authority of the module inventory | E-10 |
| TBD-05 | managed load-event subscription semantics | E-11M |
| TBD-06 | the subscribed event set, the expected set `X_phase`, the ambiguity rules | E-12 |
| TBD-07 | the managed/native observability boundary (native stays `NG-10` unless a native authority is proposed) | E-11N |
| TBD-08 | **external prerequisite:** a caller-owned creation seam for Selective with rollback authority; it is not a `CT-21D` output | E-14, B5 |
| TBD-09 | adequacy of the two-observation scan; whether it detects the interleavings it targets | S-2, S-3, section 3.2 |
| TBD-10 | library cache freshness rule and `LibraryInputRecord` details (product gate parameter) | section 7.2 |
| TBD-11 | what runs in `W3a`; whether the host command-end event is observable | section 3.5 |
| TBD-12 | interactive-host confirmation of E-01, E-02 (opening state), E-05, E-13 | those controls |
| TBD-13 | the read set that ABORT-VERIFY must cover | S-5, O2, O6 |

## 16. Evidence reuse

| Evidence | Class and limit |
|---|---|
| G3A | characterization only, for its tuple and SHA; taken in Core Console, which is not interactive `acad.exe`; the G-M24 census is not transferable to another SHA |
| AUTH-15 | contract/code plus host characterization **within its exact scope** (HeaderRun and Cantilever; document, held `LockDocument`, `StartTransaction`; Option B); identity exception on the host binaries; **not** environment-admission evidence |
| Foundation I-57, I-49, I-58 | contract/code only |
| I-59 | contract/code plus Owner Validation of its own contract; not environment-admission evidence |
| CT-DA, 3 PASS on `bef6091b` (`09N-D`, `10N-M`, `10N-SM`) | characterization only, for their class, row and build |
| CT-DA, 6 PASS on earlier builds | need an Architect **equivalence ruling** before they are cited; V2 cites none as evidence |
| `CTDA_HOST_PASS` | **no transfer** |
| coverage inferred from CT-DA; census counts; Core Console read as interactive | **no transfer** |
| `04NO-S` UNKNOWN | no transfer as a result; it only constrains the design (section 12) |

## 17. Owner sequence, Freeze and ADR

```text
Proposal V2 -> Architect review
-> Coordinator + Architect technical consensus (V3 if changes are required)
-> [Act 1] OWNER SPONSORSHIP / REDECISION
      supplements O-1 V18 explicitly; authorizes ONLY the pursuit of the design and of the CT-21D authority contract;
      does NOT select ALT-21D definitively; does NOT accept the final reduced guarantee
-> CT-21D authority contract (fixes TBD-01..TBD-13 that belong to it), reviewed and frozen
-> external prerequisites, outside CT-21D: TBD-08 (Selective creation seam), UNDO evidence plan (B6)
-> separate explicit authorization of CT-21D execution
-> CT-21D execution -> exact review of the evidence
-> replacement Freeze (states which V18 clauses it supersedes, for which contract)
-> [Act 2] OWNER FINAL ACCEPTANCE of the reduced guarantee  (precondition: UNDO evidence, the residual list of sections 3.5 and 6)
-> ADR-0036 amendment that records the accepted guarantee  -> ADR acceptance (separate Owner act)
-> explicit G3 reopening decision -> G3B -> CT-50 -> implementation gates
Any control that CT-21D shows unmeasurable is handled by an explicit, Architect-reviewed amendment (5.2); if the guarantee-bearing
controls or the completion model cannot be established, ALT-21E remains the final disposition.
```

The final Owner acceptance precedes the ADR amendment, as in V22 sections 21 and 23, so that the amendment records an accepted guarantee.

## 18. Validation plan (unchanged in structure, updated in content; nothing is authorized)

| Tier | Content |
|---|---|
| T0 pure | admission evaluator truth table (any UNKNOWN or FAIL => NO EXECUTION); outcome state machine O1-O6 (no path to SUCCESS without every clause of 3.3; no path from an unverified state to a success or a "rolled back" claim); expected-set matcher of 5.2; manifest three-layer comparison; comparator per-field mutations; `mu_k` oracle; static guards: no `OpenCloseTransaction`, no side-database write, no compensating write, **no product write after PC** |
| T1 integration | orchestrator against fake host adapters injecting drift at each checkpoint and each phase; PREPARE residue assertions; ABORT-VERIFY paths; AUTH-15 seam under abort; G-M24/T-M75 census regenerated on the candidate SHA |
| T2 host controls | a **new** predicate `ALT21D_HOST_PASS(CT-21D-V1)`; not `CTDA_HOST_PASS`; not designed here; the groups of V1 section 8.1 stand, with the parameters of section 15 as their subject and W-scan, ABORT-VERIFY and PREPARE freshness added |
| T3 Owner validation | as V1 section 8, with O4, O5, O6 walk-throughs and the provisional guidance labelled **NOT A GUARANTEED RECOVERY PATH** |

## 19. Stale conclusions

V1 section 11 (S1-S7) is incorporated by reference. Two corrections: S1 must be read with the AUTH-15 scope of section 9 (HeaderRun and Cantilever, not Selective); S2 must be read with section 9 (the I-58 comparators lift only the comparator part).

## 20. Open questions remaining for the Architect (closed list)

| Id | Question |
|---|---|
| OQ-1 | Is the two-observation scan of 3.2 an acceptable contract for `W-scan`, given that stability is not demonstrated (`TBD-09`), or is a stronger mechanism required before consensus? |
| OQ-2 | A class-B control drifting while the state agrees yields `COMMITTED_UNVERIFIED` (3.3, O5). Is that the right classification, or should it have a distinct terminal label? |
| OQ-3 | Is the expected-set contract of E-12 (5.2), with its refusal cost and `NG-19` blind spot, acceptable as a precondition, or should E-12 be logging-only from the start? |
| OQ-4 | Is `W3a` defined operationally (`CP` to the host command end) precisely enough, given that the host command-end event may not be observable (`TBD-11`)? |
| OQ-5 | Is executing PREPARE, with committed callee transactions and library imports, **under** the document lock acceptable, or must imports precede the lock? |
| OQ-6 | Is UNDO evidence as a precondition of Act 2 and not of Act 1 (11.1) the correct placement? |
| OQ-7 | `TBD-08` (Selective creation seam) is an external prerequisite: must it exist before Act 1, before `CT-21D` execution, or only before Act 2? |
| OQ-8 | Does the Architect require the equivalence ruling on the six earlier-build CT-DA passes now, although V2 cites none? |

## 21. Review checklist

- [ ] All statuses in the header are unchanged; the direction and admissibility lines say `PLAUSIBLE_NOT_YET_VIABLE`, `NOT_YET_SATISFIABLE`, `NONE`.
- [ ] CIA remains required for the original guarantee; RG is a different guarantee; the authority is not called CIA, `SafeOperationalState` or a `CTDA_HOST_PASS` equivalent.
- [ ] `W-scan`, `W3a`, `W3b` are defined; `V0`, `V1`, `CP` are precise; the condition for `SUCCESS` is exact; no product write after PC.
- [ ] Class A and class B are separated; E-10 makes no absence claim; E-11 is split and E-11N is a declared residual; E-12 infers no origin.
- [ ] PREPARE, MUTATION, VERIFY are separate; "single transaction" is scoped to MUTATION; the library input is attested; PREPARE residue is stated in the outcomes.
- [ ] The orchestrator owns lock and transaction; AUTH-15 does neither; `LOCK_MODE` is unfixed and `ExclusiveWrite` is not asserted.
- [ ] `MAXIMUM_PROPOSED_KIND_SCOPE = SELECTIVE_ONLY`, `CURRENTLY_ADMISSIBLE_KIND_SCOPE = NONE`; AUTH-15 is not presented as covering Selective.
- [ ] `FAILURE_COMMITTED_DIVERGENT`, `COMMITTED_UNVERIFIED`, `ABORT_UNVERIFIED` exist and none is success.
- [ ] UNDO is `NEEDS NEW AUTHORITY`; guidance is marked `NOT A GUARANTEED RECOVERY PATH`.
- [ ] The `04NO-S` restrictions are recorded and nothing is transferred.
- [ ] The matrix has four columns and hides no blocker behind `PRESERVED`.
- [ ] Every unfixed parameter is enumerated (section 15); B1-B8 carry an honest status.
- [ ] The Owner sequence puts final acceptance before the ADR amendment.

```text
PROPOSAL ALT-21D V2 = PUBLISHED / ARCHITECT REVIEW REQUIRED
ALT21D_DIRECTION = PLAUSIBLE_NOT_YET_VIABLE      ALT21D_ADMISSION_PASS = NOT_YET_SATISFIABLE
CURRENTLY_ADMISSIBLE_KIND_SCOPE = NONE           OWNER_DECISION_STATUS = NOT_YET      CT21D_STATUS = NOT_AUTHORIZED
CTDA_HOST_PASS = FALSE                           CIA = UNKNOWN     SafeOperationalState = FALSE_FOR_ADMISSION
G3 = STOPPED   G3B = NOT OPEN   CT-50 = NOT EXECUTED   ALT-21E = FALLBACK_IN_EFFECT   ADR-0036 = PROPOSED / AMENDED FOR V18
SUBSTANTIVE IMPLEMENTATION = BLOCKED             BRANCH = NOT RECONCILED (79 behind / 119 ahead before this commit)
NEXT GATE = ARCHITECT REVIEW OF ALT-21D PROPOSAL V2
```
