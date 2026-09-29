# I-52 — Proposal ALT-21D V1: reduced-guarantee, attested-envelope contract for RACKMIRROR

> **PROPOSAL ALT-21D V1 — DRAFT FOR ARCHITECT REVIEW. Architecture / documentation only.**
>
> ```text
> This is NOT an Owner selection of ALT-21D.
> ALT-21E                 = DEFAULT FALLBACK IN EFFECT (V18: NO SUPPORTED EXECUTION ENVIRONMENT)
> CTDA_HOST_PASS(V35-A3)  = FALSE (terminal, monotone)
> ContextIsolationAuthority = UNKNOWN
> SafeOperationalState    = FALSE_FOR_ADMISSION
> G3 = STOPPED   G3B = NOT OPEN   CT-50 = NOT EXECUTED
> ADR-0036 = PROPOSED / AMENDED FOR V18      Freeze V18 = GOVERNING      Freeze V35 = UNCHANGED
> SUBSTANTIVE IMPLEMENTATION = BLOCKED       Owner decision = NOT REQUESTED YET
> ```
>
> None of the statuses above changes with this document. Nothing here authorizes AutoCAD, a host run, a new predicate
> execution, product code, a Freeze, an ADR amendment or a reopening of G3.

## 0. Result in one page

**Question.** Can RACKMIRROR support a narrower execution contract that does not claim universal
`ContextIsolationAuthority` (CIA)?

**Answer of this Proposal.** A narrower contract can be *defined* concretely and mechanically. It is defined here as the
**Attested Envelope contract** (section 3) with a **detect-and-refuse-success** guarantee (section 2.2). It admits an
environment only when a finite list of measurable facts holds at five checkpoints; any UNKNOWN or FAIL means no
execution.

**What it costs.** ALT-21D is a *material* reduction, not a rewording:

1. it gives up *prevention* of same-context interference and of interference between the final read and the return of the
   command; it keeps *detection at named checkpoints*;
2. it gives up post-commit rollback: a detected divergence after commit is reported as a failure, never repaired
   (`FAILURE_COMMITTED_CORRUPT`, section 6);
3. it replaces "every channel is closed" with "every channel is classified as *excluded by a measured fact*, *detected by a
   state comparison*, or *explicitly non-guaranteed*" (sections 3, 4);
4. it supports one document, one lock owner, one command, and a load set equal to a pinned manifest, and nothing else.

**What it does not do.** It does **not** set CIA = TRUE, does not revive `CTDA_HOST_PASS`, does not prove that allowlisted
code is inert, and does not close the window between the final authoritative read and the return of the command
(residual `W3`, section 4). It is **not** a scoped CIA (section 9: choice **B**).

**Present admissibility.** On today's evidence the envelope admits **no environment**: every precondition that decides
between "safe" and "unknown" (load set, load continuity, event fence, exclusive lock inside the command, command-context
set) is either only *partly* backed by documentation or unmeasured in the interactive `acad.exe` (G3A ran in Core Console).
The rule "if a required fact cannot be measured authoritatively the environment stays unsupported" therefore gives
`ALT21D_SUPPORTED_ENVIRONMENT = EMPTY` until a new, separately versioned host characterization (`CT-21D`, section 8)
exists. This Proposal does not authorize it.

```text
PROPOSAL_RECOMMENDATION = NEEDS ARCHITECT RULING
  - contract:        DEFINED, finite, reviewable
  - admits today:    NOTHING (measurement authority not established)
  - viable only if:  Architect rules Q1 (a narrower replacement authority may replace the CIA precondition for reopening G3)
                     AND Q2 (detect-and-refuse-success with residual W3 is a presentable reduced guarantee)
                     AND a later CT-21D shows the section 3 facts are measurable and the section 6 outcomes occur as designed
  - NOT VIABLE if:   Q1 or Q2 is ruled negative, or CT-21D cannot measure E-10..E-12 => ALT-21E stays the final disposition
```

## 1. Governing baseline (bound, not changed)

| Authority | Identity / status | Use in this Proposal |
|---|---|---|
| I-52 branch | `feature/rackmirror-espejo-semantico` at `df454275d29fd01a2443794dadbf15282f42fb7b` (79 behind / 118 ahead of `origin/main`; zero `src/` or `tests/` delta) | documentation authority |
| `origin/main` | `3375aadbbf929427a6d106b2fff275d64863b89c` (tags `integration/I-57`, `I-58`, `I-59`, `I-52-AUTH15`) | shared authorities, section 1.2 |
| Freeze V18 | `faaf709bf6401dcda91b07f41afe44f490fb916a`; file blob `c12fa65f5b6203061e8d717d1ceed595c4fed1f4`; `GOVERNING` | `SafeOperationalState`, CIA obligation, reopening sequence (sections 5, 6, 11, 12, 14) |
| Proposal V18 | blob `827559b504ec6bc8b5f780267a40766c3fa6db8c` | ALT-A..E; ALT-D "product scope reduction: insufficient alone" |
| Proposal V21 / V22 | blobs `ff037f02c5e26136a47610ca2de848102c6d4ca9` / `66f56f624322ec426ef00a0fd7088e40ad4c511c` | ALT-21C/D/E; ALT-R1..R3 (V21 section 22); DA-P1..DA-P10 and T1..T15 (V22 sections 11-16); `OverallSuccess` (V22 section 4) |
| Decisions | `docs/automation/decisions/I-52.md` sections 117 (O-1 V18), 196, 197, 198 | O-1 wording; `CTDA_HOST_PASS = FALSE` terminal |
| ADR-0036 | blob `5628947673f2afddf141c0505e92283962459443`; `PROPOSED / AMENDED FOR V18` | core decision (semantic mirror by copy) survives; mode-catalog clauses already superseded |
| CIA research | blob `d0753ce32bb64705a0fb97e0da3b23bd8e69bc13`; result `PARTIAL_ONLY`; `CT-49R = NOT READY` | candidate mechanisms CIA-01..07 |
| CT-DA | `CTDA_HOST_PASS(V35-A3) = FALSE`; 04NO-S governing structural UNKNOWN; rows 5-14 not executed | evidence limits, section 8 |

### 1.1 What the V21/V22 requirements for ALT-21D actually say

- V21 section 22 (ALT-R1..R3): *own writes correct* is insufficient for durable authority; *detached correct although
  persisted corrupt* is not a correct RACKMIRROR; *success only if the final read matches* is "useful, but with no
  post-commit rollback and a residual race". V21 selects none of them and states that accepting a command that reports
  success on an already-corrupt result "would be a material reduction and would require a Proposal and an explicit Owner
  decision; acceptance is not inferred".
- V21 section 23: ALT-21D = "reduce the guarantee; requires Owner and does not close the product".
- V22 sections 16, 23, 25: an adverse CT-DA result returns to Proposal/fallback and the Owner is **not** asked to accept a
  non-viable guarantee; `FAILURE_COMMITTED_CORRUPT` is an observation state, "not product success, rollback or compensation";
  V22 designs no productive compensation.
- V18 section 5: ALT-D (scope reduction to an exact tuple) is "insufficient alone" because host identity is not closure of
  modes and does not prove absence of third-party extensions; "A and D may form part of a future proposal if sufficient
  external evidence appears". V18 section 9 requires any future proposal to define how it proves the absence or mediation
  of extensions.
- O-1 V18 (decisions section 117): the Owner accepted the deferral and stated that I-52 "does not reopen G3 until CT-49R
  demonstrates that authority and an explicit reopening decision exists". ALT-21D changes that precondition, so it needs
  its own Owner decision (section 10).

This Proposal answers V18 section 9 with an *attested closed world* (E-10, E-11) instead of an absence proof, and answers
V21 section 22 with ALT-R3 made precise (section 2.2) instead of leaving it as an unchosen option.

### 1.2 Shared authorities now on `main` (none of them solves CIA)

| Authority (tag on `main`) | What it provides | What it does **not** prove | Effect here |
|---|---|---|---|
| **I-57** Shared View Foundation, AUTH-01..13 (`integration/I-57`) | Pure facts and contracts in Application (address, codec, availability, frame, scan snapshot, selection, transform facts, resolve/plan ports, base name, requirements/query port, authored comparator port). AutoCAD probes, `BlockTable`, import, materialization and transactions stay in Plugin. Codec and availability "report facts, never permission to continue" | Nothing about document-lock ownership, caller/callee transaction ownership, command boundary, session or mode state, or multi-document behaviour (no such code or text found). R3 leaves "read-set/identity", "mirror exposure/build", "mirror members" and AUTH-15 with I-52. `FOUNDATIONS.md:69`: no general concurrency control between preflight and commit. `FOUNDATIONS.md:102`: strict authored equality across views is implemented only for Selective | consumed as code contracts; **no environment fact** |
| **I-58** AUTH-13 authored comparators (`integration/I-58`) | Typed comparators for Dynamic, Push Back, Cantilever and Cabecera that check **all** raw siblings; Selective delegates to `SelectiveAuthoredAuthority.Resolve`; outcome precedence `Unreadable` > `Divergent` > `Single`; zero siblings is never `Single`; unknown fields or `ExtensionData` give `Unreadable` | AutoCAD sibling capture is out of scope and the consumer attests membership and scan completeness (`RackAuthoredInput.cs:10`); Cama and generic factories fail closed; custom properties keep a separate authority | partly lifts the V21 section 15 "comparator missing" statement; does **not** lift capture, per-kind `mu_k`, round-trip or save/reopen (E-14) |
| **I-59** AUTH-08 + AUTH-12 (`integration/I-59`) | AUTH-08: capture adapter that copies AutoCAD values into a neutral input; transform facts (non-uniform scale, reflection, half-turn, negative Z are facts, not failures). AUTH-12: per-instance requirements `(PieceId, RackViewAddress, Role, LibraryKey)` with a typed address; `Unknown` never equals `Ok`; presence in the active drawing is separate from presence in the external library | consumer policy, persistence and AUTH-15 are out. The queries open **their own** transactions (`AutoCadLibraryBlockQuery`, `AutoCadExternalLibraryBlockQuery` in `src/RackCad.Plugin/Drawing/`), and the external one reads a process-cached side database (`BlockLibraryDatabaseCache`); nested behaviour under a caller transaction is not documented | shapes E-06 and the PREPARE phase; adds shared process state (NG-09); no isolation |
| **I-52-AUTH15** (`integration/I-52-AUTH15`, merge `3375aadb`) | `RackDefinitionCreator.CreateInTransaction`: the caller owns lock, transaction, Commit and Abort; `Precheck` compares `TopTransaction.UnmanagedObject` (wrapper identity never matches); typed PRE-WRITE and POST-WRITE failures; rollback affirmed **only** for a document database, a held `LockDocument`, `StartTransaction`, and Abort/Dispose without Commit (Option B); 15/15 host cases PASS under document-authority | side databases (RUN-3 leaked on Abort: 42 control and 128 characterization leaks; raw RUN-3 FAIL preserved); `OpenCloseTransaction` unsupported; existing drawing content, UNDO-stack effects and concurrency not covered; no production caller yet; identity exception on the host binaries | basis of E-06 and of "own-write rollback"; NG-09 |
| **I-49** (ADR-0043, superseding ADR-0038/0040/0041) | `PlanReadSet` = symbol-result observations (before/after) and repair-decision observations, recorded only if the preflight used them; an observed value that differs from the expected one aborts **before write**; commit path uses `LockDocument` + `StartTransaction` | no global snapshot hash, no full-registry comparison, no extra lock or preflight re-run at commit; a change outside the observations is not detected; does not cover rack authored data | E-08 uses it for project variables only; the PV/PC re-observation is new |

**Runtime observables in product code on `main` today** (`src/`, verified by search): the active document
(`MdiActiveDocument`, read and null-checked at command start), `LockDocument` acquisition (about 30 sites; acquisition
only, not lock-state detection), the top-transaction identity check (private to the AUTH-15 `Precheck`), and the advisory
`INSUNITS` read. **Absent:** `DocumentManager.Count`, `DocumentCollection` events, `IsQuiescent`, `CMDNAMES`/`CMDACTIVE`/
`GetSystemVariable`, `LongTransactionManager`, REFEDIT/BEDIT/ARRAYEDIT/BTESTBLOCK state, command hooks, lock-mode checks,
the active-transaction count and the UNDO controller. The G3A probe that reads several of them exists only as a
development tool on the I-52 branch (`eng/validation/I52G3AAutoCadProbe`), not in the product.

**Conclusion.** None of these authorities observes or excludes an environment channel. CIA stays UNKNOWN, and every
precondition in section 3 except the AUTH-15 identity check is **new product code and new characterization**.

## 2. The guarantee being reduced

### 2.1 Original guarantee, decomposed

| Id | Original guarantee | Source | Supportable today? |
|---|---|---|---|
| G0.1 | RACKMIRROR executes only in `SafeOperationalState`, whose last conjunct is `ContextIsolationAuthority = TRUE` | Freeze V18 section 5 | **No.** CIA = UNKNOWN (research `PARTIAL_ONLY`, `CT-49R = NOT READY`) |
| G0.2 | Every channel able to alter the source DB, the observed representation, symbol resolution or the consumed API semantics is excluded, detected with complete semantics, or modelled in the read-set (A/B/C) | Freeze V18 section 6 | **No.** No coverage argument for same-context callbacks, loadable code, deferred work (research gaps 1-8) |
| G0.3 | `OverallSuccess = SemanticMirrorCorrect AND PersistedSemanticCorrect AND MaterializationCorrect AND MaterializationCommitSucceeded` | V22 section 4 | Yes as a *definition*; its *prevention* premises are G0.2/G0.4 |
| G0.4 | No foreign semantic write, commit-time mutation or secondary-transaction survival can produce a reported success over a corrupt result, for the whole window `[S, command-completion]` (DA-P1..DA-P10) | V22 sections 11-16 | **Not admitted.** `CTDA_HOST_PASS = FALSE`; ALT-21C not admissible |
| G0.5 | All-or-nothing batch; one T; UNDO removes payload and views together; SAVE/reopen equality `D' = D1 = D2 = D3` | V21 sections 17-19 | Own-transaction part yes (AUTH-15); UNDO and SAVE/reopen need physical proof |
| G0.6 | Unknown or unsupported environment fails closed | Freeze V18 section 7 | Yes; ALT-21D keeps it |

The two items that cannot be supported are **G0.2** (closure of channels) and **G0.4 as prevention** (closure of the
window). ALT-21D leaves G0.3, G0.5 (within its stated limits) and G0.6 in place.

### 2.2 ALT-21D in positive terms

ALT-21D replaces G0.1/G0.2/G0.4 with the following four reductions. Everything else (section 2.3) stays.

| Id | Reduction | Positive statement |
|---|---|---|
| RED-1 scope | from "not provably unsafe -> safe universally" to a **finite envelope** | RACKMIRROR runs only when `ALT21D_ADMISSION_PASS` (section 7) holds. Every state outside the envelope is *unsupported*, not "probably fine" |
| RED-2 isolation | from "all channels closed" to **three-way classification** | each channel is (i) `EXCLUDED` by a measured fact, (ii) `DETECTED` by a state comparison that does not assume isolation, or (iii) `NON-GUARANTEED` and named (section 4). No channel is left unclassified |
| RED-3 window | from "no interference in `[S, completion]`" to **detect-and-refuse-success at named checkpoints** | success is reported only if the fresh authoritative postcommit read (`R_post`/`M_post`, V22 section 8) equals `D'`/expected materialization **and** the envelope facts are unchanged at pre-commit (PV) and post-commit (PC). The command-completion point is **defined** as the instant `R_post` equality is established; a change after it is a *post-completion edit*, outside the guarantee (residual `W3`) |
| RED-4 failure | from "roll back or never happened" to a **defined non-success outcome after commit** | a divergence found after commit is reported as `FAILURE_COMMITTED_CORRUPT`; the command never reports success, never compensates, and directs the user to UNDO (whose atomicity needs proof, section 8) |

ALT-21D is therefore ALT-R3 of V21 with (a) a bounded envelope in front, (b) an explicit continuity requirement, and
(c) a defined completion point. It is not ALT-R1 (own writes correct) and not ALT-R2 (detached correct).

### 2.3 What is not reduced

Pure semantic transform `mu_k` and the exact common mirror transform; independence of writer, reader, comparator and
`mu_k` (V21 section 14); one `NewRackId` per logical rack and identical sibling authority; `PlanReadSet` per variable
(I-49); include-by-default `ExtensionData`; one caller-owned transaction and one commit for the whole invocation
(V21 section 17); rollback of RackCad's *own* writes before commit (AUTH-15 Option B); per-kind readiness and
unsupported-family fail-closed; no negative-scale placement (ADR-0036); UX/naming/copy-only semantics preserved by
reference from V17/V18.

### 2.4 The reduction, dimension by dimension

`EXCLUDED-BY-FACT` = a measured precondition (section 3). `DETECTED` = compared state at PV/PC (section 6).
`NON-GUARANTEED` = named in section 4. `UNSUPPORTED` = refused at admission.

| Dimension | ALT-21D disposition | Fact / non-guarantee |
|---|---|---|
| Supported AutoCAD document state | `EXCLUDED-BY-FACT` | E-05, E-07, E-08 |
| Number/type of open documents | `UNSUPPORTED` unless exactly one | E-02; NG-03 |
| Command context | `EXCLUDED-BY-FACT`: one non-transparent, non-nested RackCad command | E-04; NG-05 |
| Persistent-command / mode state | known modes `EXCLUDED-BY-FACT`; unknown modes `NON-GUARANTEED` | E-05; NG-07 |
| External events / callbacks | `DETECTED` where they change compared state; otherwise `NON-GUARANTEED` | E-12; NG-01, NG-06, NG-15 |
| Document lock ownership | `EXCLUDED-BY-FACT` for other execution contexts only | E-03; NG-01 (same-context) |
| Transaction ownership | `EXCLUDED-BY-FACT` for the product's own T; foreign secondary T `NON-GUARANTEED` | E-06; NG-09 |
| Concurrent RackCad commands | `EXCLUDED-BY-FACT` for visible cases; a transparent command started during the run is seen only at checkpoints | E-03, E-04, E-12; NG-05 |
| Non-RackCad activity | `DETECTED` when it changes compared state or the load set; otherwise `NON-GUARANTEED` | E-10..E-12; NG-06 |
| Modeless / UI activity | `NON-GUARANTEED` except through E-03/E-12 | NG-04 |
| Async / external automation | other-context writes `EXCLUDED-BY-FACT` only if they honour the lock; COM/LISP/VBA same-context `NON-GUARANTEED` | E-03; NG-02 |
| SAVE / reopen | `NEEDS NEW AUTHORITY` (physical proof D3) | section 5, section 8 |
| UNDO | `NEEDS NEW AUTHORITY` (physical proof) | section 5, section 8 |
| Multi-document execution | `UNSUPPORTED` | E-02; NG-03 |

## 3. Supported execution envelope

```text
ALT21D_SUPPORTED_ENVIRONMENT =
    "Attested single-document interactive session":
      exactly the AutoCAD 2025 tuple named by E-01, running interactively (acad.exe),
      one open document, one lock owner, one RackCad command, a load set equal to a pinned manifest,
      known persistent modes inactive, no active foreign transaction,
      every fact re-checked at PV and PC.

PRESENTLY ADMITS: NOTHING  (see the measurement-authority column; CT-21D not authorized)
NOT ADMITTED BY DEFINITION: Core Console, APS Automation, headless/batch, multi-document sessions, verticals not named in E-01
```

### 3.1 Checkpoints

| Cp | Instant | Purpose |
|---|---|---|
| P0 | command entry, before the lock and before any transaction | admission; no persistent result can exist |
| PL | immediately after the document lock is acquired | facts that depend on the lock |
| PS | inside the single transaction, at the source authoritative read `S` (V21 section 5) | snapshot point; last read before the first destination write |
| PV | after all destination writes and the staged `R_pre`/`M_pre` reads, immediately before `Commit()` | last chance to abort with rollback |
| PC | after `Commit()`, in a fresh read transaction, after `R_post`/`M_post` | decides success; defines the completion point |

### 3.2 Preconditions

Authority-status column: **DOC** = a documented contract or documented behaviour cited in the CIA research or G3A;
**G3A** = characterized in G3A, exact tuple, *Core Console*; **NEW** = no evidence in this repository. *No status here is
sufficient by itself*: every fact needs positive and adversarial confirmation in the **interactive** host under `CT-21D`
before it may count, because G3A ran in Core Console and its evidence is exact-tuple, exact-SHA (Freeze V18 section 8).

| Id | Observable fact | Authority / source | How it is measured | PASS | UNKNOWN | Failure action | Cp | Status |
|---|---|---|---|---|---|---|---|---|
| E-01 HostExact | product/program/market, `acad.exe` `R25.0.171.0.0` SHA-256 `2A75996FD2A5C5EE0376FD5BA7CCED227A098193FF495E8CEEDA3A96FE326422`; `AcCoreMgd`/`AcDbMgd`/`AcMgd` `25.0.171.0.0` with the G3A SHA-256 values; interactive process | `HostApplicationServices`, process image, file hashes (V18 `HostExact`; G3A section 1) | read identity properties; hash the loaded files | equals the manifest tuple exactly | any read fails | REFUSE (no lock, no T) | P0 | G3A (values), NEW (interactive) |
| E-02 SingleDocument | exactly one open document; it is the invoking, active document; its `Database` is the working DB; no document opening/closing | `DocumentCollection`, `MdiActiveDocument`, document/database correspondence (V18 `CurrentDocumentOwned`) | count = 1; identity equality; no document-created/destroyed event since P0 | count = 1 and identities equal | count or identity unreadable | REFUSE / at PV,PC: ABORT or `FAILURE_COMMITTED_CORRUPT` | P0, PV, PC | DOC (object model), NEW (opening-state) |
| E-03 LockExclusive | the product holds the document lock in a mode that excludes other execution contexts | `Document.LockDocument(...)`; `DocumentLockMode.ExclusiveWrite` excludes all locks of other contexts (CIA-01, OFFICIAL CONTRACT) | acquire via the AUTH-15 seam; read back the lock mode; conflict/veto is an observable error | acquired in the manifest-declared mode and read back | error other than a lock conflict, or lock mode unreadable | REFUSE (lock conflict is FAIL, not retry-forever) | PL, PV | DOC (exclusion between contexts); NEW (mode compatibility with the command's implicit lock and AUTH-15) |
| E-04 CommandContext | exactly the RackCad command is active; no nested, transparent, script-driven or application-context-queued invocation | `CMDNAMES`, `CMDACTIVE` (documented system variables; G3A: not proof *between* commands, valid *inside* the command boundary); registered command flags | read both at P0/PL/PV/PC; compare with the manifest's admitted value set | equals the admitted set (the numeric set is fixed by CT-21D, not asserted here) | unreadable | REFUSE / ABORT | P0, PL, PV, PC | G3A (variables), NEW (admitted set, interactive) |
| E-05 KnownModesInactive | `REFEDITNAME = ""`, `BLOCKEDITOR = 0`, `ARRAYEDITSTATE = 0`, `BLOCKTESTWINDOW = 0`; no long transaction for the document | G3A detectors (V18 `KnownDetectorsInactive`, `NoLongTransaction`); `LongTransactionManager.CurrentLongTransactionFor(Document)` | read the four variables and the long-transaction predicate | all inactive | any read fails | REFUSE / ABORT | P0, PV, PC | G3A (Core Console), NEW (interactive) |
| E-06 TransactionOwnership | no active transaction on the document's transaction manager at P0; from the start of the single transaction to PV exactly one active transaction, the product's own; no `OpenCloseTransaction`; no **write** to any side database. PREPARE-phase infrastructure (AUTH-12 presence queries and library imports) finishes and closes its own transactions before the single transaction starts, and may read the process-cached library side database read-only | AUTH-15 Option B rollback authority: document DB + `LockDocument` + `StartTransaction`; `OpenCloseTransaction` unsupported (`integration/I-52-AUTH15`); AUTH-12 queries are callee-owned (section 1.2) | active-transaction count at P0, at transaction start and at PV; AUTH-15 top-transaction identity check (`UnmanagedObject`); static guard that the product uses no OCT and no side-DB write | count = 0 at P0; exactly 1 (own, identity equal) at PV; guard green | count unreadable | REFUSE / ABORT | P0, PV | DOC (AUTH-15 scope), NEW (count, interactive) |
| E-07 DatabaseComplete | source and destination databases fully opened; xrefs complete or explicitly out of the read set | V17 PRE-19 / Freeze V18 `DatabaseComplete`; XREF rules (loaded and complete = current in-memory content; demand-loaded incomplete or unresolved = UNKNOWN) | preserved V17 checks | complete | incomplete/unresolved | REFUSE / ABORT | P0, PS | preserved from V17/V18 |
| E-08 ReadSetStable | the mirror read-set (the source authoritative read at `S`, plus the I-49 `PlanReadSet` observations of the project variables actually used) is **re-observed** at PV and PC and equals what was observed at PS | I-49 `PlanReadSet` (symbol results and repair reasons only; no global snapshot, no concurrency control: `FOUNDATIONS.md:69`); mirror read-set stays with I-52 (R3) | re-run the same reads and compare typed values | identical | any re-read fails | ABORT (PV) / `FAILURE_COMMITTED_CORRUPT` (PC) | PS, PV, PC | PS: preserved from V17/V18; PV/PC re-observation: NEW |
| E-09 OverrulesBounded | no relevant overrule family with UNKNOWN effect on the subjects/classes consumed | `Overrule.HasOverrule(subject, RXClass)` (G3A `OVERRULE_GRANULARITY = SUBJECT + CLASS / CHARACTERIZED`) | per-subject/class query | none relevant, or provably irrelevant | relevant family with unknown effect | REFUSE / ABORT | P0, PV | G3A (Core Console) |
| E-10 LoadSetAttested | the set of loaded native modules, process modules and managed assemblies equals the pinned manifest, entry by entry (path + SHA-256); no other member | `DynamicLinker.GetLoadedModules`, `acedArxLoaded`/`acrxLoadedApps`, `EnumProcessModulesEx`/`Process.Modules`, `AppDomain.GetAssemblies` (CIA-03/04: PARTIAL_ONLY; each is a fallible snapshot) | enumerate all four; hash each file; **two snapshots** and require equality; unreadable file or in-memory-only module is UNKNOWN | equal to the manifest, both snapshots equal | any enumeration fails, snapshots differ, a module has no readable file | REFUSE | P0 | DOC (partial), NEW (interactive completeness) |
| E-11 LoadContinuity | no module or assembly load/unload between P0 and PC | snapshot equality at P0, PV, PC; managed `AppDomain.AssemblyLoad` events; native linker load/unload notification if CT-21D shows it is observable | compare snapshots; count events | equal snapshots, zero load events | event subscription fails or linker events not observable | ABORT (PV) / `FAILURE_COMMITTED_CORRUPT` (PC); a *transient* load-and-unload between snapshots is `NG-10`, not detected | PV, PC | NEW |
| E-12 EventFenceClean | no unattributed modification of any object in the protected set during `[P0, PC]`, and no document-collection, other-command or transaction event outside the product's own operation | `Database`, `Document`, `DocumentCollection`, `Editor` and transaction events (CIA-07: PARTIAL_ONLY, detects only channels that emit events) | subscribe at P0; attribute every event to a product operation id; log | zero unattributed events | subscription fails | ABORT (PV) / `FAILURE_COMMITTED_CORRUPT` (PC). *Advisory strengthening only*: it never turns an otherwise failing state comparison into PASS | P0..PC | NEW; CT-DA rows that passed characterize some event classes on the campaign build only |
| E-13 ProfileEquality | `SECURELOAD` and `TRUSTEDPATHS` equal the manifest-declared values | system variables read by the product; values set only by the Owner/CAD manager (the product never writes them) | read at P0 | equal | unreadable | REFUSE | P0 | DOC (reduces load origin only; PARTIAL_ONLY as isolation) |
| E-14 KindReadiness | every selected rack kind `k` has reader, writer, independent authored comparator, sibling authority, round-trip and save/reopen proof | V21 section 15; AUTH-13 comparator ports (section 1.2) | compile-time registry + per-kind evidence record | ready | unknown kind | REFUSE that kind (all-or-nothing: REFUSE the whole invocation) | P0 | I-58 typed comparators exist for Selective (delegate), Dynamic, Push Back, Cantilever, Cabecera; Cama fails closed. NEW: AutoCAD sibling capture, per-kind writer and `mu_k`, round-trip, save/reopen. Strict authored equality is proven only for Selective (`FOUNDATIONS.md:102`) |
| E-15 ContractBinding | the manifest names this contract version, the Freeze V19 identity, the `CT-21D` characterization record for the exact tuple (PASS), and the recorded Owner acceptance | new artifacts (section 7); Owner/CAD-manager custody | read and verify the manifest and records | all present and consistent | missing or unreadable | REFUSE | P0 | NEW (does not exist) |

The manifest (E-10, E-13, E-15) is produced by a *baseline capture* of the characterized profile, reviewed and signed off
by the Owner/CAD manager, and stored outside the product binary. It pins each entry by path and SHA-256. Classes such as
"Autodesk-signed" may be used to *build* a manifest, never to *admit* an entry that the manifest does not list. In V1,
third-party modules are not admissible at all: there is no "asserted inert" route. The expected consequence is refusal on
machines with injected third-party DLLs; that is the safe direction.

### 3.3 Facts deliberately not preconditions

| Fact | Why it is not a precondition |
|---|---|
| "no pending input", "no queued command", "no idle/synchronization callback registered", "no COM client attached" | no public authority enumerates them (research gaps 4, 5); a precondition that cannot be measured would make the environment unsupported forever. They are `NON-GUARANTEED` (NG-02, NG-04, NG-06) and covered only by detection |
| "allowlisted code is inert" | not measurable; asserted by no one in V1. Attested modules are *identified*, not *certified* (NG-01, NG-14) |
| "no code injected without a module" | outside the threat model (V22 section 28: memory patching, OS injection, hostile in-process code) |

## 4. Explicit non-guarantees

Nothing below is hidden behind a generic limitation. For each item: what ALT-21D does, why it cannot be in the guarantee,
what the user would observe.

| Id | Not guaranteed | ALT-21D behaviour | Why it cannot be guaranteed | Consequence |
|---|---|---|---|---|
| NG-01 | Universal context isolation; same-context behaviour of attested code | E-10 identifies the code; E-03 excludes only *other* contexts (research gap 1; the lock does not suspend same-context callbacks) | no official contract that same-context callbacks are excluded; attested is not certified | an attested module that writes RackCad payload from a same-context callback is caught only if a compared state or event changes at a checkpoint |
| NG-02 | Arbitrary external automation (COM, LISP, VBA, script) | other-context writers must take the document lock and are excluded by E-03; same-context LISP/VBA callbacks are not enumerated | no inventory of all automation channels (research gaps 2, 5) | a same-context automation writer is detected only by comparison |
| NG-03 | Multiple active documents; multi-document execution | `UNSUPPORTED` at P0 (E-02) | shared heap/static data across document contexts (research section 3) | refusal with a message |
| NG-04 | Modeless interference (palettes, dialogs) | not enumerable; only E-03/E-12 | no modeless registry (research section 11) | refusal only when a checkpoint sees drift |
| NG-05 | Concurrent, nested or transparent commands | E-04 refuses the visible cases; a transparent command started *during* the run is seen only through E-04/E-12 at PV/PC | `CMDNAMES`/`CMDACTIVE` are boundary signals, not a session authority (V18 section 3, item 5) | drift detected at the next checkpoint or not at all in between |
| NG-06 | Background callbacks (idle, timers, synchronization, COM events) | not enumerable; detected only through state or event drift | no public enumeration (research gap 4) | possible undetected change between checkpoints if it reverts before the next read |
| NG-07 | Persistent modes unknown to the detector list | not modelled; the known four are E-05 | no complete mode registry (V18 section 2) | unknown mode may alter semantics without changing any compared value |
| NG-08 | APPCTX cases: `ExecuteInApplicationContext`-driven or application-context-queued invocation | `UNSUPPORTED`: the product runs only as a registered document-context command | rows 16A-S and 13A-SM were never executed; nested-lock and application-context host facts are UNKNOWN | refusal when E-04 shows another context |
| NG-09 | `OpenCloseTransaction`; side-database rollback semantics; the process-cached library side database | the product uses no OCT and writes no side database (E-06); a *foreign* secondary transaction that commits a protected mutation is not rolled back by our abort; the content of `BlockLibraryDatabaseCache` (shared process state read by AUTH-12 queries) is in no comparison | Option B scope of AUTH-15: rollback authority is document DB + LockDocument + StartTransaction only; DA-P3 UNKNOWN | on abort, a foreign committed mutation survives; on commit it is detected at PC only if it changes a compared value |
| NG-10 | Transient module load-and-unload between snapshots; loads that do not change any compared value | snapshots at P0/PV/PC; load events if observable | `EnumProcessModulesEx` is a fallible snapshot; no load fence (research gap 3) | undetected transient code execution |
| NG-11 | Post-commit rollback | none; `FAILURE_COMMITTED_CORRUPT` is reported, not repaired (section 6) | a compensating write can fail or trigger more callbacks (V21 section 11; V22 section 25) | the drawing may hold a divergent result until the user runs UNDO |
| NG-12 | Post-completion edits (`W3`) | anything after `R_post` equality, including `commandEnded` handlers and handlers between the last read and the host's command-end events, is outside the guarantee | DA-P6 (no synchronous mutation between the final accepted snapshot and command-completion) is not proven; no executed CT-DA row establishes it | a divergence appearing after PC is invisible to the command |
| NG-13 | Hostile or memory-patching code, admin-level tampering with the manifest or the binary | out of the threat model | V22 section 28 | none claimed |
| NG-14 | That an attested Autodesk or RackCad module is free of bugs that write protected state | out of scope | not measurable | covered only by tests and by comparison |
| NG-15 | Anything CT-DA showed as structurally UNKNOWN | not relied on | see 4.1 | see 4.1 |

### 4.1 What CT-DA showed, and what ALT-21D therefore may not rely on

- **`04NO-S` (governing structural UNKNOWN).** After `Abort` of the primary transaction the host delivered `cancelled`
  (`N-OBJ-CANCEL`) but none of `modifyUndone`, `modified`, `closed`. Consequence: ALT-21D must not use abort-time observer
  notifications as a *detection* mechanism, and must not assume an observer sees the effect of its own abort. Detection is
  by state comparison after the abort/commit, never by an expected notification.
- **Rows 5-14 were never executed.** Their conditional host facts stay UNKNOWN for ALT-21D: whether `04NO-UNDO-S` behaves
  like `04NO-S`; whether a write-capable lock is observable before the request is granted (`10NDOC-WILL-SM`), already held at `commandWillStart` (`02NEDW-ALL`) or observable in `commandCancelled` (`02NEDC-ALL`);
  whether a nested `kWrite` request returns eOk in the APPCTX chain (`13A-SM`); whether a nested append is accepted inside
  `objectAppended` (`02NAPP-SM`); whether the transaction manager accepts a start from the callback (`02NTAS-ALL`) and whether the aborted primary
  transaction is no longer active there (`02NTA-ALL`); whether `cancel()` sends `modifyUndone` (`COBJUNDO16*`). ALT-21D takes **no** dependency on any of them; any
  future dependency must be created and characterized under `CT-21D`, not inferred.
- **The nine rows that passed** (`09N-B`, `02NDBMOD-S`, `02NO-S`, `16N-S`, `10N-S`, `16C-S`, `09N-D`, `10N-M`, `10N-SM`)
  are characterization evidence for their classes on their builds only (section 8.2).

## 5. Product semantics under ALT-21D

`PRESERVED` = same guarantee as before. `REDUCED` = kept with the stated weaker statement. `UNSUPPORTED` = refused.
`NEEDS NEW AUTHORITY` = cannot be claimed without evidence that does not exist.

| Product property | Mark | Statement under ALT-21D | Basis |
|---|---|---|---|
| All-or-nothing batch | **PRESERVED before commit; REDUCED after** | one transaction, one commit for the whole invocation; any product failure or detected drift before commit aborts everything (PREPARE-phase library imports stay outside the atomic unit, unchanged from V21 section 17). After commit the guarantee is: no success is reported unless PC equality holds; a divergence yields `FAILURE_COMMITTED_CORRUPT` for the *whole* invocation with no partial-success report (V22 section 26) | V21 section 17; RED-3, RED-4 |
| One `NewRackId` per logical mirrored rack | **PRESERVED** | RackCad-internal identity, verified by the independent comparator at `R_pre` and `R_post` | V21 sections 8, 9 |
| Semantic persistence | **REDUCED** | equal to `D'` at `R_post`; not protected against a change after PC (`NG-12`) or against interference that reverts before a read | RED-3 |
| Authored payload preservation | **PRESERVED** (per ready kind) | include-by-default `ExtensionData`, schema, custom properties, bindings, `DimensionViews`; independent comparator | V21 section 8; AUTH-13 |
| PV / reference preservation | **PRESERVED** for the read-set; **NEEDS NEW AUTHORITY** if the kind writes NOD | `PlanReadSet` per variable, re-observed at PS/PV/PC (E-08); NOD writes only in the same DB and T; managed fixture `HF-V31-1` with `F-NOD`/`F-SRC` was never instantiated | V21 section 16; V22 DA-P8; decisions 197 |
| Exact common mirror transform | **PRESERVED** | pure `mu_k`, T0-testable, independent of the host | V17/V18 by reference |
| No partial placement | **PRESERVED before commit; REDUCED after** | see all-or-nothing | as above |
| Rollback on product failure | **PRESERVED** for own writes; **`NG-09`** for foreign secondary transactions | Option B rollback authority | `integration/I-52-AUTH15` |
| Unsupported-family fail-closed | **PRESERVED** | E-14 refuses the whole invocation if any selected kind is not ready | V21 section 15 |
| UNDO of a success removes payload and all views together | **NEEDS NEW AUTHORITY** | requires physical proof; a compensating delete does not substitute | V21 section 18 |
| SAVE / reopen `D' = D1 = D2 = D3` | **NEEDS NEW AUTHORITY** | requires physical proof with the authoritative reader | V21 section 19 |
| Multi-document | **UNSUPPORTED** | E-02 | NG-03 |
| Non-interactive execution | **UNSUPPORTED** | Core Console and APS are outside the envelope | CIA-06 `NOT_APPLICABLE`; axis acquisition is interactive |

**Is the reduced guarantee still strong enough?** For the product's *authored-state* promises (identity, payload,
read-set, transform, unsupported-family refusal) the answer is yes, because those depend on RackCad code and on comparisons
this contract keeps. For the *environment* promises (nothing else touched the drawing while the command ran) the answer is
"only as far as the checkpoints and events see"; that is precisely the reduction the Owner would be asked to accept.

## 6. Failure and abort policy

### 6.1 Outcomes

| Outcome | When | Persistent semantic result | Reported as |
|---|---|---|---|
| O1 `REFUSED` | any precondition FAIL or UNKNOWN at P0 or PL | none (no transaction was opened) | refusal naming the failing facts |
| O2 `ABORTED` | product failure, or any continuity FAIL/UNKNOWN at PS or PV | none from RackCad (own writes rolled back); a foreign committed secondary mutation is `NG-09` | failure with cause; no success |
| O3 `SUCCESS` | ADMISSION_PASS, PV continuity PASS, commit accepted, PC continuity PASS, `R_post`/`M_post` equal | complete | success |
| O4 `FAILURE_COMMITTED_CORRUPT` | commit accepted and (`R_post`/`M_post` differ, or PC continuity FAIL) | present and divergent | failure; lists the divergent objects and facts; no compensation; instructs UNDO |
| O5 `COMMITTED_UNVERIFIED` | commit accepted and the PC read is UNKNOWN (read error, timeout, unreadable fact) | present, unknown | failure of verification; **never** success; same handling as O4 |

`SUCCESS` requires every clause; there is no partial success and no warning-with-success.

### 6.2 Policy answers

- **Detect before transaction start only?** No. The facts are re-measured at PL, PS, PV and PC.
- **Continuously revalidate?** No polling; the event fence (E-12) records from P0 to PC and any unattributed event forces
  ABORT at PV or O4/O5 after commit. Between checkpoints only events observe; where a channel emits no event, drift that
  reverts before the next checkpoint is undetected (`NG-06`, `NG-10`).
- **Validate before commit?** Yes: PV runs the staged `R_pre`/`M_pre` reads (V22 sections 8-10) and the full continuity set.
- **Abort on any drift?** Before commit, yes: any FAIL or UNKNOWN in the continuity set aborts and rolls back the product's
  own writes. After commit there is nothing to abort; the outcome is O4/O5.
- **Refuse placement / produce no persistent semantic result?** O1 and O2 produce none from RackCad. O4/O5 do leave a
  committed state; ALT-21D says so instead of hiding it.
- **Do not assume CIA to detect what CIA was supposed to prove.** No control's correctness depends on the absence of
  interference. Every control is a *comparison of an observed value with an expected value* whose failure or unreadability
  yields FAIL/UNKNOWN. State comparison at PV/PC is decisive; events (E-12) and inventories (E-10, E-11) only add
  detections and can never turn a failing comparison into PASS.
- **Compensation.** None. A compensating write can itself fail or fire callbacks; ALT-21D adds no compensating writes.

### 6.3 Residual windows (named, not closed)

| Window | Description | Status |
|---|---|---|
| W1 | between PS and PV (destination writes and the staged reads) | shrunk by reading `S` in the same T; interference here is caught at PV by `R_pre`/`M_pre` and E-08 |
| W2 | between PV and the end of `Commit()` (commit-time callbacks) | detected at PC only if visible to the read |
| W3 | between `R_post` equality and the host's command-end events | **not covered**; post-completion edit by definition (NG-12) |
| W4 | between any two checkpoints for a change that reverts before the next read | not covered (NG-06, NG-10) |

## 7. Admission contract

```text
ALT21D_ADMISSION_PASS(D, H, S, C, K) iff
      E-01 = PASS AND E-02 = PASS AND E-03 = PASS AND E-04 = PASS AND E-05 = PASS
  AND E-06 = PASS AND E-07 = PASS AND E-08 = PASS AND E-09 = PASS AND E-10 = PASS
  AND E-11 = PASS AND E-12 = PASS AND E-13 = PASS AND E-14(k) = PASS for every kind k in K
  AND E-15 = PASS

ALT21D_ADMISSION_UNKNOWN  =>  NO EXECUTION        (identical to FAIL: refuse, open no transaction)
ALT21D_ADMISSION_FAIL     =>  NO EXECUTION
No partial PASS. No "known list + allow otherwise". No retry loop inside the command.
At P0, E-11 and E-12 are PASS iff they are *armed* (baseline snapshot taken, subscriptions established); they are
evaluated for drift only at PV and PC.

ALT21D_CONTINUITY_PASS(cp) iff, at cp in {PV, PC},
      E-01 E-02 E-03 E-04 E-05 E-06 E-08 E-09 E-11 E-12 unchanged and PASS

ALT21D_SUCCESS iff
      ALT21D_ADMISSION_PASS
  AND ALT21D_CONTINUITY_PASS(PV)
  AND Commit() accepted
  AND ALT21D_CONTINUITY_PASS(PC)
  AND OverallSuccess (V22 section 4) evaluated on R_post / M_post
```

`ALT21D_ADMISSION_PASS` is **not** `SafeOperationalState = TRUE` and does not touch it. If ALT-21D is ever adopted, a
Freeze would define a *separate* `SafeOperationalState_D` whose last conjunct is `ClosedWorldEnvironmentAttestation`
(section 9) instead of CIA; V18's predicate remains historical and `FALSE_FOR_ADMISSION`.

## 8. Validation plan (only if the Proposal is accepted; nothing here is authorized)

The host validation is a **new, separately versioned authority and predicate**, `ALT21D_HOST_PASS(CT-21D-V1)`. It is not
`CTDA_HOST_PASS(V35-A3)`, does not reuse its rows as passes, and does not revive it.

| Tier | Scope | Contents |
|---|---|---|
| **T0 pure / unit** | no AutoCAD | admission evaluator truth table (any UNKNOWN or FAIL => NO EXECUTION; property tests over all 3^n fact combinations for a reduced model); manifest parse/diff; two-snapshot comparison logic; outcome state machine O1-O5 (no path reaches SUCCESS without every clause); independent comparator with per-field mutations (`ExtensionData`, schema, bindings, siblings); `mu_k` oracle; static guard: no `OpenCloseTransaction`, no side-database write, no compensating write after commit |
| **T1 integration** | CI, no AutoCAD | command orchestration against fake host adapters that inject drift at each checkpoint (PL, PS, PV, PC) and each fact; assertions: refusal/abort/O4/O5 exactly as section 6; AUTH-15 seam under abort; G-M24/T-M75 census regenerated on the candidate SHA (`UNCLASSIFIED => RED`); per-kind readiness registry |
| **T2 host controls** | interactive `acad.exe`, exact tuple; requires a later explicit authorization | see 8.1 |
| **T3 Owner validation** | Owner, exact `FINAL_CANDIDATE_SHA`, same AutoCAD tuple and manifest | successful mirror on a real drawing; deliberate non-admitted states (second drawing open, REFEDIT active, unlisted plugin loaded) must be refused with a clear message; UNDO of a success; SAVE/close/reopen; one forced `FAILURE_COMMITTED_CORRUPT` walk-through with the documented message |

### 8.1 T2 host controls (`CT-21D-V1`, design only)

| Group | Question |
|---|---|
| M-* measurability | for each of E-01..E-13: positive control (PASS in the characterized profile) and adversarial control (FAIL/UNKNOWN when the fact is violated: second document, transparent command, REFEDIT, unlisted managed assembly, unlisted native module, changed `TRUSTEDPATHS`, unreadable file) |
| L-* lock | is `ExclusiveWrite` (or the manifest-declared mode) compatible with the command's implicit lock and with the AUTH-15 seam; what does a conflicting context observe |
| F-* fence | which of E-12's events fire for a same-context writer in each window W1-W4; classify each window as detected / undetected (undetected is a *recorded residual*, not a failure of the control) |
| C-* completion | what runs between `R_post` equality and the host's command-end events; is `W3` empty in this tuple |
| U-* rollback | own-T abort leaves zero product writes; a foreign secondary-T commit survival is measured, not assumed |
| Z-* durability | UNDO of a success; O4 followed by UNDO; SAVE/close/reopen with the authoritative reader (`D3`) |
| K-* kinds | per-kind comparator and sibling authority with the host in the loop |

### 8.2 Evidence reuse

| Evidence | Reusable? | Limit |
|---|---|---|
| G3A host tuple, `HostExact` values, detectors, `Overrule.HasOverrule` granularity | **Yes, as a candidate** | exact tuple; G3A ran in **Core Console**; needs interactive confirmation (M-*) |
| G-M24 census (1,359 sites) | **No for a new SHA** | describes SHA `560ec72...` only; regenerate on the candidate SHA (Freeze V18 section 8) |
| CIA research candidate matrix CIA-01..07 | **Yes, as source of mechanisms** | it concluded `PARTIAL_ONLY`; nothing in it is a coverage argument |
| CT-DA passing rows | **Only as characterization of their class, build and tuple** | 3 on the baseline `bef6091b` (`09N-D`, `10N-M`, `10N-SM`); 6 on superseded builds (3 A2 results carried forward conditionally, decisions 197.4, and 3 A3 results on `afa65bc0`) need an Architect equivalence ruling; none counts as a `CT-21D` pass |
| `CTDA_HOST_PASS = FALSE`, `04NO-S` UNKNOWN | **Cannot be reversed or re-run** | it constrains the design (section 4.1) |
| AUTH-15 (`integration/I-52-AUTH15`, RUN-3 PASS under document-authority) | **Yes for its scope** | rollback authority = document DB + LockDocument + StartTransaction; side-DB characterization only |
| Foundation AUTH-01..13, I-58, I-59, I-49 | **Yes as code contracts** | see 1.2; none proves environment isolation |
| Owner Validation | **No** | none exists for RACKMIRROR |

New evidence required: everything in 8.1, the manifest baseline capture, per-kind comparator/sibling proofs, UNDO and
SAVE/reopen physical proof, and the Owner acceptance record.

## 9. Relationship to CIA

```text
ALT-21D does NOT set ContextIsolationAuthority = TRUE.
CIA stays UNKNOWN. SafeOperationalState stays FALSE_FOR_ADMISSION.
```

**Choice: B — ALT-21D introduces a narrower replacement authority**, named `ClosedWorldEnvironmentAttestation` (CWEA):
"the loaded code and the observable session facts equal a pinned, Owner-reviewed manifest, and remain equal at PV and PC".

Why B and not the others:

- **Not A (bypass by shrinking the envelope).** Shrinking the envelope does not remove the need for an authority; V18
  section 5 already rejected "scope reduction alone" as insufficient. ALT-21D still relies on an authority (CWEA) plus a
  detection contract.
- **Not C (a subset of CIA).** CIA quantifies over *every* channel that can alter observations and demands a coverage
  argument (A/B/C per channel, V18 section 6). A scoped CIA would still owe that argument for same-context callbacks of
  attested code, for deferred work and for transient loads, which the research found unavailable (gaps 1, 3, 4). Calling
  ALT-21D a subset of CIA would import an obligation it cannot meet.
- **Not D (impossible without CIA).** It is possible *as a reduced guarantee*: what is promised changes (section 2.2), so
  the missing coverage argument is replaced by declared non-guarantees plus checkpointed detection.

The honest statement to the Owner is therefore: RACKMIRROR would not be *isolated*; it would be *attested and verified after
the fact*, with the residuals of sections 4 and 6.3 accepted explicitly. Whether B satisfies the reopening rule of V18
section 14 ("`ContextIsolationAuthority` demonstrated") is exactly the Architect ruling Q1; this Proposal does not
pre-decide it.

## 10. ADR and Freeze impact, and sequencing (nothing is performed here)

If the Owner eventually selects ALT-21D:

| Item | Required? | Note |
|---|---|---|
| Architect review of this Proposal | Yes | next gate |
| Coordinator consensus on the exact SHA | Yes | Proposal V2 if changes are required |
| Owner decision | **Yes, more than once** | (1) direction/sponsorship of ALT-21D before any `CT-21D` authorization, because O-1 V18 said G3 does not reopen until CT-49R proves CIA; (2) final acceptance of the reduced guarantee after `CT-21D` and the replacement Freeze (Q4) |
| Replacement Freeze (V19 or next free number) | **Yes** | must state that it supersedes Freeze V18 sections 5, 6, 11, 14 for this contract, define `SafeOperationalState_D`, CWEA, the outcomes O1-O5, the residual list, and keep V18 historical and immutable |
| ADR-0036 amendment | **Yes**, after the Freeze | the current amendment is for V18/ALT-E; a new one must record the reduced guarantee and the envelope; ADR acceptance is a separate later Owner act |
| G3 reopening | **Yes**, explicit, only after `CT-21D` passes and the amendment exists | not implied by Owner selection |
| G3B / CT-50 | **Yes, later** | representability matrix, only after G3 reopens |
| CT-49R | **Superseded by `CT-21D` only if the Architect rules Q1 in favour** | otherwise still required and still `NOT READY` |
| Product implementation, guards, Candidate, Owner Validation | later, per the gates above | blocked |

```text
G3_REOPENING_SEQUENCE =
  Architect review of ALT-21D V1
  -> Coordinator + Architect technical consensus (Proposal V2 if required)
  -> Owner decision: sponsor ALT-21D direction and authorize CT-21D design/authority (O-1 redecision)
  -> CT-21D authority contract (new versioned predicate) reviewed and frozen
  -> CT-21D host execution (separate explicit authorization)
  -> exact review of CT-21D result
  -> replacement Freeze (records ALT-21D, CWEA, residuals, supersession of the V18 reopening rule for this contract)
  -> ADR-0036 amendment
  -> Owner final acceptance of the reduced guarantee (and of the ADR)
  -> explicit G3 reopening decision
  -> G3B / CT-50 later
  -> implementation gates
Any FAIL or UNKNOWN in CT-21D that touches E-10..E-12 or the completion point => ALT-21E remains final.
```

## 11. Stale conclusions because `main` now contains I-58, I-59 and AUTH-15

The branch is not reconciled. These statements in the branch documents are stale or must be re-verified before any
reuse; none of them is edited here.

| # | Statement in the branch documents | Why it is stale or must be re-verified | Effect on this Proposal |
|---|---|---|---|
| S1 | V17-V22 and Freeze V18 section 7 carry AUTH-15 / caller-owned creation as "preserved, not implemented" | AUTH-15 is integrated (`integration/I-52-AUTH15`) with **Option B** scope only; side-DB leaks and OCT are explicitly unsupported | E-06 and NG-09 use the integrated scope; earlier text must not be read as "unimplemented" nor as broader than Option B |
| S2 | V21 section 15 / V22 section 27: "only Selective has a demonstrated authored comparator; Dynamic, Header, Push Back, Cantilever inadmissible" | I-58 delivers typed comparators for Dynamic, Push Back, Cantilever and Cabecera (all raw siblings) | the *comparator* part is stale; **admissibility is not**: AutoCAD sibling capture, per-kind `mu_k`/writer, round-trip and save/reopen are still missing (E-14) |
| S3 | `I-52-foundation-consumption-reconciliation.md`: AUTH-13 "Selectivo delegates; kinds without demonstrated equivalence stay `Unreadable`"; AUTH-08 = `Transform2D`/`RackTransformFacts`; AUTH-12 = `LibraryBlockRequirement`, extractors, query, importer | I-58 evolved AUTH-13; I-59 evolved AUTH-08 (capture adapter, classifier) and AUTH-12 (per-instance requirements with typed `RackViewAddress`, active-drawing vs external-library presence) | reconciliation must be refreshed before any product gate; no conclusion of this Proposal depends on the old mapping |
| S4 | G-M24 census (1,359 sites), T-M75 fixtures | describe SHA `560ec72...`; Plugin changed on `main` (I-55, I-58, I-59, AUTH-15 and later) | Freeze V18 section 8: regenerate on the candidate SHA; T1 in section 8 |
| S5 | V17-V22 cite I-49 through ADR-0038/0040/0041 and "A3-R2 pending" | ADR-0043 now supersedes ADR-0038, ADR-0040 and ADR-0041 | E-08 cites ADR-0043 semantics (observation set, abort before write); re-verify any older reading |
| S6 | Decisions 196/197 "main at `3375aadb`"; HANDOFF I-52 "79 behind / 115 ahead" | the branch is now 79 behind / 118 ahead (§198 and earlier gates) | none |
| S7 | `main` `HANDOFF.md` lines 15-51 and 1895-1908 still say I-57/I-58 are "NOT integrated" | contradicted by tags `integration/I-57` and `integration/I-58` | not an I-52 document; noted only so nobody reuses that text as evidence |

**Not stale:** `ContextIsolationAuthority = UNKNOWN`; the research result `PARTIAL_ONLY` and `CT-49R = NOT READY`; the G3A host tuple
values (exact tuple unchanged: `acad.exe` `R25.0.171.0.0`); the V22 threat classes T1..T15 as a *classification*; `CTDA_HOST_PASS = FALSE`.
Main's new authorities are pure-Application contracts or seam code and do not observe any environment channel.

## 12. Blockers and open questions

**Blockers (each prevents admission today):**

| Id | Blocker |
|---|---|
| B1 | Measurement authority for E-10..E-12 (load set, load continuity, event fence) is `PARTIAL_ONLY`; there is no official load freeze or complete inventory (research gaps 2, 3, 5) |
| B2 | `W3` cannot be closed with current authority (DA-P6 not proven); ALT-21D can only *define* it as outside the guarantee |
| B3 | No interactive-host characterization exists; G3A ran in Core Console |
| B4 | No Owner decision for a reduced guarantee; O-1 V18 requires CT-49R and an explicit reopening |
| B5 | Per-kind readiness: only kinds with an independent comparator and sibling authority are admissible (comparators now exist for five kinds, but AutoCAD sibling capture, per-kind writer/`mu_k` and the physical proofs do not; V1 scope recommendation: Selective only) |
| B6 | Branch is 79 commits behind `main`; census, kind readiness and AUTH-15 statements must be regenerated |

**Open questions for the Architect (rulings requested):**

| Id | Question |
|---|---|
| Q1 | May a *narrower replacement authority* (CWEA, choice B) satisfy the reopening precondition that V18 section 14 writes as "`ContextIsolationAuthority` demonstrated", or does any reopening require CIA = TRUE for a declared scope? If the latter, ALT-21D is NOT VIABLE |
| Q2 | Is "detect-and-refuse-success, no post-commit rollback, residual `W3` and `NG-01`" a reduced guarantee that may be presented to the Owner, or is `W3`/`DA-P6` a disqualifier for any admission? |
| Q3 | Manifest policy: pin every module (this Proposal) or admit Autodesk-signed modules by class? |
| Q4 | Owner timing: sponsor direction before `CT-21D`, then final acceptance after; or a single decision after `CT-21D`? |
| Q5 | Is the lock mode `ExclusiveWrite` acceptable alongside the AUTH-15 seam, or must the design use the seam's existing mode? (T2 `L-*`) |
| Q6 | Which kinds are in scope of V1 (E-14)? Recommendation: Selective only, others fail closed |
| Q7 | Which of the six superseded-build CT-DA passes may be cited as characterization without re-execution (Architect equivalence ruling, decisions 197.4)? |
| Q8 | Interactive vs Core Console: is a Core Console G3A datum citable for the interactive tuple at all? |
| Q9 | Must the product treat the absence of abort-time observer notifications (`04NO-S`) as a permanent design constraint? (This Proposal does) |
| Q10 | Custody, format and review of the manifest (Owner / CAD manager), and the user-facing wording of O1-O5 |

## 13. Recommendation

```text
PROPOSAL_RECOMMENDATION = NEEDS ARCHITECT RULING
```

The narrower contract is definable, finite and reviewable, and it keeps the product's authored-state guarantees. It cannot
be called safe: it converts an unprovable isolation claim into a checkpointed detection claim with named residuals, and on
current evidence it admits no environment. It should proceed only if the Architect rules Q1 and Q2 in favour; otherwise the
disposition is ALT-21E as the final I-52 disposition, with no further host work.

## 14. Review checklist

- [ ] Every status in the header block is unchanged (ALT-21E, `CTDA_HOST_PASS = FALSE`, CIA UNKNOWN, `FALSE_FOR_ADMISSION`, G3 STOPPED).
- [ ] The reduced guarantee is stated in positive terms (RED-1..RED-4) and the non-reduced part is listed (2.3).
- [ ] Every precondition has fact, source, measurement, PASS, UNKNOWN, failure action and an honest status; no phrase like "safe enough" or "normal conditions" is used as a value.
- [ ] Facts that cannot be measured are not preconditions and are listed as non-guarantees (3.3, 4).
- [ ] The completion point is defined, and `W3` is named rather than hidden.
- [ ] Nothing depends on `04NO-S` or on the unexecuted rows 5-14; nothing revives `CTDA_HOST_PASS`.
- [ ] CIA is not set TRUE; the relationship choice (B) is justified against A, C, D.
- [ ] Validation is split T0-T3, only `CT-21D` is new, and nothing is authorized.
- [ ] Stale main-dependent conclusions are identified (11) and not silently fixed.

```text
PROPOSAL ALT-21D V1 = PUBLISHED / ARCHITECT REVIEW REQUIRED
OWNER DECISION = NOT REQUESTED     ALT-21E = DEFAULT FALLBACK IN EFFECT
CTDA_HOST_PASS = FALSE             CIA = UNKNOWN     SafeOperationalState = FALSE_FOR_ADMISSION
G3 = STOPPED   G3B = NOT OPEN   CT-50 = NOT EXECUTED   ADR-0036 = PROPOSED / AMENDED FOR V18
SUBSTANTIVE IMPLEMENTATION = BLOCKED     BRANCH = NOT RECONCILED (79 behind / 118 ahead)
NEXT GATE = ARCHITECT REVIEW OF ALT-21D PROPOSAL V1
```
