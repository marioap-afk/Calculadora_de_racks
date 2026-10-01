# I-52 — CT-21D Host-Gate Execution Package V3 (non-governing host characterization / baseline-preparation evidence)

> ```text
> DOCUMENT            = host-gate execution package V3, drafted for the Coordinator's decision (Owner record of 2026-09-30, "NEXT WORK" steps 6 and 7;
>                       decisions section 242). Not a baseline artifact; not in the BA-11 registry; not a contract. It decides no value and
>                       authorizes nothing
> ROLE                = DRAFTER (documents only) for the COORDINATOR
> STATUS              = V3_PREPARED_FOR_COORDINATOR_DECISION. NO HOST RUN HAS STARTED. NO HOST RUN MAY START BEFORE THE COORDINATOR'S
>                       FIRST_NON_GOVERNING_HOST_RUN AUTHORIZATION (section 4), WHICH DOES NOT EXIST
> SUPERSEDES          = nothing. Package V1 (blob b23221a8b4cb8f2b30135b1e811456be2cbb976c) and package V2 (blob 99d5096845e2d4a2a1b203d9cfad9b8abc245fde)
>                       stay untouched as history. V3 replaces V2 for the next gate ruling only
> AUTHORITY_CONTRACT  = CT-21D-V2.6 AGREED (decisions section 230; composite 97100cff11868807f3c49f25bf25d0385c86e8873cba0cf2c1996338b9b98eb4),
>                       as cited by package V2 (not re-read for V3)
> BOUND_PRODUCT_SHA   = 95690c28dc6268e61dff32a0cbc33cc9fde3d47f (non-tuple bound item, as in V2). `git diff --stat HEAD -- src` is empty in this worktree
>                       (HEAD b1fd5027e057ed77e51cd0a229d7a2367e8922b3)
> SOURCES (git blob ids of the working tree, `git hash-object`, read when this package was written; sections 1 to 8 rest on these files)
>                       docs/initiatives/I-52-ct21d-host-gate-execution-package-v1.md                  b23221a8b4cb8f2b30135b1e811456be2cbb976c
>                       docs/initiatives/I-52-ct21d-host-gate-execution-package-v2.md                  99d5096845e2d4a2a1b203d9cfad9b8abc245fde
>                       docs/initiatives/I-52-ct21d-tol-scale-design-and-host-fact-matrix-v1.md        dc2897e5ee0c761f29d964215f7b23acb9627c1b
>                       docs/automation/evidence/I-52-ct21d-owner-decisions-q-o-1.md                   7f1db12d905c686b12d36e1d9819f9613cee5f5c  (untracked at HEAD)
>                       docs/automation/decisions/I-52.md (working tree, sections 233 to 242)         09d537eba30e175e55c5a03df2b5c573ffd66e9d  (modified, uncommitted)
>                       eng/research/I52Ct21dHostFacts/ (R0 class; tracked at HEAD b1fd5027)           identity for the gate = the declared set (section 3.2), never this text
>                       eng/research/I52Ct21dHostFactsRs/ (RS class; UNTRACKED at HEAD)                identity for the gate = HASHES-RS.txt as verified in section 3.3 and the declared set
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> HOST_RUN_STARTED = FALSE     G3 = STOPPED     CIA = UNKNOWN     FALLBACK = ALT-21E
> ```

## 0. Reading rules

- This package **prepares** the first non-governing host gate. It runs nothing, authorizes nothing, starts nothing. The Owner's authorization is the one of
  decisions sections 233, 237 and 242 and is not widened here. Where a source leaves a point open the row says `OI-n` (section 7); the drafter decides none.
- **UNKNOWN** means: not established in the repository. No hash, build, path, count or approval is invented. Fields that only the CAD manager or the host
  can supply appear as `<...>` and are empty. A DLL hash is **never** stated here: every DLL hash is `to be recorded from the canonical build` (3.4).
- `[HOST-TO-CONFIRM]` marks a behaviour of Windows or AutoCAD that no file of the repository establishes. The raw output on the designated machine governs; a mismatch
  makes the fact `UNKNOWN`, never a corrected guess.
- **Who operates.** The CAD manager (or the Owner), on the designated machine. **The agent does not operate the host and does not start any host run.**
- Spanish passages are the Owner's or the Coordinator's words, quoted verbatim; the English around them is the drafter's.
- **What the drafter did not verify.** (i) The Architect's review of the RS instruments and its required changes REQ-1..REQ-3 are stated to the drafter by the
  Coordinator; no review record is in the repository files read (decisions end at section 242). The README of the RS folder describes REQ-1 and REQ-2 (README,
  "Deviations" items 4 and 5); REQ-3 is not identifiable in the files read. (ii) The rulings (a) to (h) of section 1.4 are as communicated to the drafter; the
  repository does not yet hold the decisions entry that records them. (iii) No DLL was built or hashed for this package.

### 0.1 What V3 changes with respect to V2

| Change | Where |
|---|---|
| the Owner's decision Q-O-1 = `ALSO_SIDE_DB_WRITES_AND_NEW_FILES`, Q-O-0 = ACCEPT, RQ1_READING = ACCEPT (two-phase tuple) applied; the limits quoted verbatim | 1.1 to 1.3 |
| the RS class (I-3, I-5, I-6) is implemented; the activity list becomes four ordered sessions S1 to S4 | 2 |
| the Coordinator's rulings (a) to (h) on the Architect's review of the RS instruments (OP2 custody, no companion allowance, order U-RS-2 before U-RS-3, attestations, scratch hygiene, declared set, minors) | 1.4, 2.3, 3, 4, 5 |
| new Coordinator rules: `machineClassLabel = UNSET` is rejected for host runs; the designation is written only after phase 2 validates | 3.6, 3.7 |
| pre-run checklist P1..P20 in the Owner's order, hash verification table, templates for the machine-class record, tuple record phase 1 / phase 2, private copy, TRUSTEDPATHS, scratch root, cleanup ledger | 3 |
| a parametrized FIRST_NON_GOVERNING_HOST_RUN authorization template | 4 |
| stop conditions, classification, no-retry for the RS class | 5 |

## 1. Authority

### 1.1 The gate and the host execution discipline

The gate is `HOST_GATE_BA11_3_3 = AUTHORIZED`, NON_GOVERNING ONLY, extended by `G6 = EXTEND` (decisions sections 233 and 237; quoted in package V2 sections 1.1 and 1.2,
whose text stands). The six-step "HOST EXECUTION DISCIPLINE" and the line "No automatic retries." of package V2 section 1.1 stand verbatim.

### 1.2 Owner decision Q-O-1 (decisions section 242; verbatim record `docs/automation/evidence/I-52-ct21d-owner-decisions-q-o-1.md`)

Granted:

```text
Q_O_1 = ALSO_SIDE_DB_WRITES_AND_NEW_FILES

La autorización previa de host "read-only" debe interpretarse como:

READ_ONLY_PRODUCT_STATE

y NO como:

NO_WRITES_ANYWHERE_ON_THE_MACHINE.

Para la caracterización host NO gobernante autorizo de forma limitada:

1. creación y escritura en SIDE / SCRATCH DATABASES desechables usadas
   exclusivamente por instrumentación;

2. creación de NUEVOS archivos de evidencia/resultados dentro de una raíz
   allowlisted y fijada por la tuple/package;

3. logs, JSON, hashes, .pin, probe results y otros outputs expresamente
   declarados por los instrumentos aprobados;

4. las actividades RS necesarias para:
   - I-3
   - I-5
   - I-6
   - HF-C4
   - OP2
   - TOL_SCALE host characterization
   cuando sus diseños/instrumentos hayan pasado los gates requeridos.
```

The limits of Q-O-1 (reproduced verbatim, as the Coordinator asked):

```text
Esta autorización NO permite:

- modificar el governed AutoCAD document;
- guardar el governed drawing;
- modificar el original library DWG;
- escribir mediante product seams en el documento;
- escribir en src/;
- implementar RACKMIRROR;
- sobrescribir archivos arbitrarios;
- escribir fuera de los paths allowlisted;
- usar side DB para introducir cambios indirectos en el governed document;
- dejar estado persistente no declarado entre corridas;
- convertir una corrida no gobernante en governing.

SIDE DBs usadas por instrumentación deben ser:

- control-plane owned;
- disposable;
- no registradas como governed documents;
- identificadas en evidencia;
- limpiadas/verificadas según el contrato.

Los archivos nuevos deben:

- estar bajo una evidence/scratch root fija;
- usar paths resueltos y validados;
- no escapar de la raíz;
- quedar declarados en el output schema;
- entrar en hashes/custody cuando corresponda.
```

The distinction the Owner asks to use from now on (`Q_O_0_READING = ACCEPT`):

```text
READ_ONLY_PRODUCT_STATE
vs
CONTROL_PLANE_AUXILIARY_WRITES.
```

Every RS record carries `classification = CONTROL_PLANE_AUXILIARY_WRITES` and `governing = false` (schema `ct21d.rs-run.v1`).

### 1.3 Owner decisions RQ-1 and instrument discipline (same record)

`RQ1_READING = ACCEPT`: the tuple has two phases. The Owner's list, condensed without changing its content:

| Phase | When | Contents |
|---|---|---|
| **PHASE 1, PRE-SESSION** | before AutoCAD is started | machine identity / machine class; Windows build; expected AutoCAD build / profile; exact RackCad package / DLL hashes; exact instrument build / hashes; allowed-api control hashes; `declaredSet`; authorized paths; configuration inputs |
| **PHASE 2, SESSION BINDING** | immediately after the session starts | PID; process start timestamp; the actual loaded-module set; session / profile facts; `.pin`; the required runtime identities |

The Owner's rule, kept in its terms: PHASE 2 must be **complete and validated BEFORE the first probe / characterization activity that will produce evidence used by this gate**. AutoCAD's
unavoidable internal startup activity is not forbidden by "before any other activity".

`MACHINE_DESIGNATION = DELEGATED_TO_CAD_MANAGER`. No host run until the CAD manager (1) designates the machine, (2) publishes the machine-class record, (3) completes the pre-session
phase of the tuple, (4) leaves ready the mechanism to complete session binding, (5) publishes / records `declaredSet` and `.pin` as the contract says.

The Owner's discipline per instrument unit (his words): `IMPLEMENT -> tests without AutoCAD where possible -> prohibited-API analysis -> independent source review -> hashes -> Architect review -> Coordinator ruling`; "No ejecutar host automáticamente al terminar implementación." The Owner also keeps the conservative model learned from R0: the automatic analysis **is not sufficient authority by itself**; before the host there must be independent human / source review, the exact hash of the allowed-apis list, of the formats / schemas, of the instrument DLLs, of the package, and the CAD manager verifies the hashes. "Cualquier cambio de bytes después de review invalida la aprobación."

The Owner's step 7: the Coordinator decides `FIRST_NON_GOVERNING_HOST_RUN` only when machine, tuple, hashes, instruments, source review, allowed APIs, paths, cleanup and exact scenario are all closed. Afterwards "podrán ejecutarse SOLO las probes no gobernantes explícitamente autorizadas".

### 1.4 The Coordinator's rulings on the RS instruments that V3 reflects (as communicated to the drafter)

`ARCHITECT_RS_REVIEW = APPROVED_WITH_REQUIRED_CHANGES; REQ-1..3 applied; delta review pending/recorded elsewhere.` This package does **not** claim that the delta review passed.

| Id | Ruling | Where applied |
|---|---|---|
| (a) | OP2 files are written to the declared SCRATCH root. After `scratch-verify` returns 0 the CAD manager copies each declared scratch DWG into the evidence root (create-new), compares its SHA-256 with `rs-run.scratch.files`, then seals with `tools seal`. Scratch originals are deleted only after the seal, and the deletion is entered in the cleanup ledger. The instrument does not copy. **This is an erratum to design 2.4 step S6** ("save the side database to a new file in the evidence folder") that the Coordinator records | 3.8, 3.9, 5 |
| (b) | No allowance for companion files after `SaveAs`: INVALID if one appears. U-RS-2 (`CT21DHG_DIMWRITEBACK`) runs BEFORE the one-shot U-RS-3 (`CT21DHG_TOLSCALE`) so any companion surfaces first | 2.1, 2.3, 5 |
| (c) | The `contentSha256` of `tolscale-raw` lives in `rs-run-tolscale.json` (the design schema of `tolscale-raw` is closed and has no such field) | 3.4 (records table), 5 |
| (d) | Readings of the TOL_SCALE analysis accepted: precedence INVALID > FAIL > UNKNOWN > PASS; a host-rejected construction = FAIL; offsets `2^-k` (PF-1) and `2^-20` (PF-2); witness bound 8 ulp; rotation or normal not as expected => row UNKNOWN | 2.3 (S4), 5 |
| (e) | `otherAcadProcess` and `loadRouteScripted` are CAD-manager attestations: the CAD manager keeps process-list snapshots before and after, and the NETLOAD transcript or script hash with the load outcome | 3.5, 4 |
| (f) | Scratch-root hygiene: one evidence root and one fresh, empty, dedicated scratch root per command (per run id); local fixed-drive path, not under OneDrive / synced / indexed folders; ACL limited to the AutoCAD user; `scratch-verify` after the run before any deletion | 3.8 |
| (g) | Declared set of V3: `I52Ct21d.HostFacts.Rs.dll`, `I52Ct21d.HostFacts.Core.dll`, `I52Ct21d.HostFacts.Rs.Core.dll`, the R0 DLL, each with its `.pin`; the Core DLL is **one set of bytes identical for R0 and RS**; review material: the five RS lists, six RS schemas, four R0 lists, the offline tools (tools-rs and R0 tools DLLs), and each `run-designation-rs.json` with its hash (it changes per run). Build once from the committed tree with SDK 8.0.423 and re-run `scan-rs` on those exact DLLs | 3.2, 3.4, 3.6 |
| (h) | MIN-A accepted with condition (local drive-letter roots only). MIN-B accepted with condition (the CAD manager runs `scratch-verify`; the Coordinator rules a crash). MIN-C (check-then-use race on `SaveAs` create-new) ACCEPTED by the Coordinator as a documented residual inside a dedicated, ACL-restricted, empty root. MIN-D accepted: the Coordinator's authorization names both roots and the designation file hash per run, and the CAD manager verifies the file hash immediately before each command. MIN-H deferred (fail-closed `NOT_RECORDED` with the slot consumed) | 3.5, 4, 5 |

Additional rules that **this package** adds for the Coordinator's decision (they are the Coordinator's to adopt, change or strike):

- **R3-1.** A record with `machineClassLabel = UNSET` is rejected for host runs (3.6).
- **R3-2.** The designation file is placed for the instrument only after phase 2 validates (3.7).
- **R3-3.** Each RS command runs in its own session (a fresh `acad.exe` process), once; a different build or any module change is a new tuple with no automatic evidence transfer (2.2).

## 2. Activities and order of sessions

The matrix is section 3.4 of the Architect design (blob dc2897e5...); V3 cites its rows by id (`HF-...`) and restates none. Instrument classes are the design's:
**R0** (strict read-only) and **RS** (side databases and new files only, `CONTROL_PLANE_AUXILIARY_WRITES`).

### 2.1 Order

```text
S1   R0 session group (strictly in this order, each sub-session a separate fresh acad.exe process, package V2 3.4 and decisions section 241 RQ-11)
       S1-A  H1   machine / session fact capture (I-1 protocol, I-0 label, I-9 seal). NO NETLOAD, nothing of RackCad, no instrument loaded
       S1-B  H2   R0 census of the private library copy: CT21DHG_CENSUS_R, then CT21DHG_INVENTORY
       S1-C  H3b  G6 host-type facts: CT21DHG_CENSUS_R on its OWN fresh private copy (HF-G1, section 241 RQ-11), then CT21DHG_CTXVARS (HF-G3)
S2   U-RS-1  CT21DHG_CENSUS_DYN     (H2-DYN, HF-C2)
S3   U-RS-2  CT21DHG_DIMWRITEBACK   (H3a, HF-C4 / HF-C5, OP2 save of the side database)       runs BEFORE S4 (ruling (b))
S4   U-RS-3  CT21DHG_TOLSCALE       (H4, HF-T1, 147-row probe, OP1 and OP2, one execution)    NOT ELIGIBLE until the gates of 2.3 are closed
```

The sequence is the Coordinator's to reorder or drop through the membership lines (section 4). An activity with no recorded IN does not run.

### 2.2 Session rules

- Each command runs **once per session**. The RS commands refuse a second typing before any side effect (a refusal is not an execution). A session runs the commands named for it and nothing else.
- Each session has its own run id `HGP-H<n>-<yyyymmddThhmmssZ>-<nn>`, its own evidence root, its own private copy of the library, and (RS) its own scratch root. `nn` counts attempts and is a log number, not a budget.
- A different build, or any module change (added, removed, updated), is a **new SESSION_BUILD_TUPLE**. No evidence is transferred automatically between builds, in either direction.
- It changes the MACHINE_CLASS label only if it changes one of the six class-defining attributes (BA-10 V8 section 2.2; Owner AR10_03B_READING). Both identities are recorded separately.
- Process hygiene for every session: before launch verify no `acad.exe` runs; one fresh process per attempt; process list at start and at end; profile variables read before and after.
- Building any DLL needs AutoCAD closed (the compile references the managed assemblies of the installed AutoCAD). Builds happen **before** the tuple record is signed and never between two sessions of one campaign.
- The RS DLL declares a **fixed** run-id shape for TOL_SCALE (`HGP-H4-...`; any other run id for `CT21DHG_TOLSCALE` is refused by the instrument). For U-RS-1 and U-RS-2 the designation schema accepts any `HGP-H<digits>-...`; which H number they take is `OI-3`.

### 2.3 The sessions

**S1-A, H1.** HF-M1..HF-M6, HF-M8..HF-M11 observed; HF-M7 (the `MC-<12 hex>` label) computed offline by `tools label`; HF-M4b only if the CAD manager declares it as a configuration input of the tuple (decisions section 241 RQ-5). Protocol: package V2 section 7.5 (script `protocol/i1-os-observation.ps1`, protocol `protocol/I-1-observation-protocol.md`, hashes in 3.3). No instrument, no designation file, no `NETLOAD`. Its phase 2 (PID, process start, `SESSION_START` module list and hashes) is complete and validated before the six reads. `machineClassLabel` is `UNSET` in the tuple record at this session because H1 produces it (R3-1 applies to the sessions that load an instrument, 3.6).

**S1-B, H2.** `CT21DHG_CENSUS_R` then `CT21DHG_INVENTORY` on a fresh private copy of the library (HF-C1, HF-C3, HF-T2). Two commands, two records. Needs the label from S1-A.

**S1-C, H3b.** `CT21DHG_CENSUS_R` (HF-G1) on its **own** fresh private copy, then `CT21DHG_CTXVARS` (HF-G3, system-variable reads only; the active document is declared in the tuple as an unsaved drawing from a copy of the designated template, never a product file). Needs the label.

**S2, U-RS-1, `CT21DHG_CENSUS_DYN`.** HF-C2 (dynamic properties exposed by a reference, read in a scratch side database). Writes `dyncensus-raw.json`, `hostfact-HF-C2.json`, `rs-run-dyncensus.json`. No scratch DWG is saved by this command (README, `core-rs/DynCensus.cs` header). Side databases are created by the instrument (`new Database(...)`), never `WorkingDatabase`, never the active document.

**S3, U-RS-2, `CT21DHG_DIMWRITEBACK`.** HF-C4 (storage, order, equal-to-style behaviour of the 8 product overrides) and HF-C5 (group code to variable, read only from the stored pairs); OP2 = save of the side database as a NEW scratch file, reopen, re-read. Writes `writeback-raw.json`, `hostfact-HF-C4.json`, `hostfact-HF-C5.json`, `rs-run-writeback.json` and **one scratch DWG**. Ruling (b): any companion file next to the saved DWG (backup, temporary, lock) makes the run INVALID; no allowance is declared. This session runs **before** S4 so that a companion file surfaces first.

**S4, U-RS-3, `CT21DHG_TOLSCALE`.** HF-T1: 147 rows (132 PF-1 plus 15 PF-2; 135 in scope), OP1 and OP2, witness, statistics, state machine, **single execution**. Writes `tolscale-raw.json`, `tolscale-probe-table.json`, `rs-run-tolscale.json` (which carries the `contentSha256` of `tolscale-raw`, ruling (c)) and one scratch DWG. Readings accepted by ruling (d).

> **S4 IS NOT ELIGIBLE.** It is gated, in addition to every condition of section 4, by: **PRE-1..PRE-4** of the design (oracle and writer scale rules sealed; the SL-4 writer route fixed; fixture plan parameters sealed; the pre-registration entry written and **sealed**), and by the Coordinator's **R-M ruling on R1(b)** (decisions section 239 R-M; `scaleX/Y/Z` to `EXACT_DOUBLE` is not ruled). Decisions section 242 (d) keeps these as conditions of the H4 run and of the value decision, not of the instrument. S4 also needs S3 to have run, with the Coordinator having recorded its outcome and the absence of any companion file. Until all of these are closed, S4 is `BLOCKED` and no authorization may name it. `TOL_SCALE` stays `UNSET`.

**Later gate, not in this package.** Fixtures H5 / H6 / H7 and the I-7 conformance reader remain in a later gate (the PRODUCT_BUILD does not exist; `RACKMIRROR` is not in `src/`). `HF-G2` is NOT opened (decisions section 242 (c)). I-7 and I-8 are not implemented.

## 3. Pre-run checklist, procedures and templates

State of every row today is NOT DONE / NOT PROVIDED unless the row says otherwise. Rows P1 to P6 follow the Owner's six prerequisites of decisions section 237; P7 to P20 are the later requirements of sections 241 and 242.

### 3.1 Checklist (Owner's order)

| Id | Requirement | Evidence the Coordinator needs | State |
|---|---|---|---|
| **P1** | The CAD manager designates the exact machine (one physical, stable machine) | designation record (3.5.1), its SHA-256, record id, the CAD manager's name and role, the Owner's delegation (Q-O5) | NOT DONE |
| **P2** | The CAD manager publishes the **machine-class record** | machine-class record (3.5.2) and its SHA-256 | NOT DONE |
| P2b | TRUSTEDPATHS decision applied before the six reads (option (a), one recursive trusted root, decisions 239 R-F) | decision record (3.5.4), SHA-256 | NOT DONE |
| **P3** | Tuple record, PHASE 1, complete | tuple record phase 1 (3.5.3), SHA-256, module baseline hashed file by file | NOT DONE |
| **P4** | Session-binding mechanism ready (how PID, process start, module list, `.pin` check and runtime identities will be captured and validated before the first probe) | tuple record phase 2 template filled in its procedure fields; I-1 script and AutoLISP transfer ready (package V2 7.5) | NOT DONE |
| **P5** | `declaredSet` and `.pin` published / recorded | the declared set with every DLL hash and `.pin`; `declaredSetSha256` | NOT DONE |
| **P6** | Canonical build and scan results | one build from the committed tree, SDK 8.0.423, Release; DLL hashes into 3.4; `scan-rs` and the R0 `scan` re-run on those exact DLLs, **CLEAN**, printing the SHA-256 of every list | NOT DONE |
| P7 | Independent source review of the RS instruments recorded | the review record, **named**, with the exact byte set it covers (HASHES-RS.txt hash). Not found in the repository files read | NOT RECORDED |
| P8 | Independent review of the R0 class (decisions section 241: PASS, conditions a-c) | section 241; the four R0 list hashes of 3.3 unchanged | RECORDED in section 241 for the R0 class |
| P9 | Architect review of `allowed-apis-rs.txt` and of the formats (schemas) | the Architect's entry. `ARCHITECT_RS_REVIEW = APPROVED_WITH_REQUIRED_CHANGES; REQ-1..3 applied; delta review pending/recorded elsewhere` | DELTA REVIEW NOT RECORDED HERE |
| P10 | Hashes verified by the CAD manager against 3.3 and 3.4 | the signed verification table (3.3 template) | NOT DONE |
| P11 | Evidence root and fresh scratch root per command created and hygienic (3.8); roots on local fixed drive-letter paths (MIN-A) | folder listing, ACL listing, the CAD manager's statement on OneDrive / sync / indexing | NOT DONE |
| P12 | Private library copy per run (package V2 7.4) | the A0, B0, A1 hash outputs; library path and SHA-256 | NOT DONE (per run) |
| P13 | Cleanup plan and cleanup ledger opened (3.9) | the ledger file, empty, with its SHA-256 | NOT DONE |
| P14 | Attestation records ready: process-list snapshots before and after; NETLOAD transcript or script hash with the load outcome | the attempt log first lines; the script hash | NOT DONE |
| P15 | `run-designation(.rs).json` composed for the session, its hash recorded (the file is placed only after phase 2 validates, 3.7) | file content hash `designationSha256` | NOT DONE (per session) |
| P16 | Exact scenario named: session, command(s), run id, roots, designation hash | the authorization (section 4) | NOT GIVEN |
| P17 | Coordinator's membership line for each requested session (package V2 7.7 form) | the decisions entry | NOT GIVEN |
| P18 | Owner objection window on the Coordinator's readings subject to objection (decisions 239, 242 (a)(b)) | the decisions entry | NOT RECORDED |
| P19 | S4 only: PRE-1..PRE-4, sealed pre-registration entry, R-M ruling on R1(b), S3 outcome recorded | entries | NOT MET |
| P20 | This package ruled by blob id | the decisions entry naming the blob id | NOT GIVEN |

### 3.2 The declared set (final and recorded before the first session)

| Element | Content | State |
|---|---|---|
| R0 package | `I52Ct21d.HostFacts.R0.dll` (commands `CT21DHG_CENSUS_R`, `CT21DHG_INVENTORY`, `CT21DHG_CTXVARS`) plus its `.pin` | DLL hash: to be recorded from the canonical build |
| shared core | `I52Ct21d.HostFacts.Core.dll` plus its `.pin`. **One set of bytes**, identical in the R0 and RS packages (same SHA-256 wherever it is placed) | to be recorded from the canonical build |
| RS package | `I52Ct21d.HostFacts.Rs.dll` (commands `CT21DHG_CENSUS_DYN`, `CT21DHG_DIMWRITEBACK`, `CT21DHG_TOLSCALE`), `I52Ct21d.HostFacts.Rs.Core.dll`, each with its `.pin`. The RS DLL self-pins all three at command start | to be recorded from the canonical build |
| RackCad build | **none** in any session of this package | declared as `NONE` in the tuple record |
| review material (not loaded in AutoCAD) | the five RS lists, the six RS schemas, the four R0 lists (3.3); the offline tools: `I52Ct21d.HostFacts.Rs.Tools.dll` (tools-rs) and `I52Ct21d.HostFacts.Tools.dll` (R0 tools); `run-designation-rs.json` of each run with its hash (the file changes per run) | list and schema hashes: 3.3. Tool DLL hashes: to be recorded from the canonical build |
| `packageManifestSha256` | an opaque 64-hex value supplied by the CAD manager; nothing in the instrument folders generates or checks a manifest (I-8 not implemented; decisions section 241 RQ-4 / RQ-10) | UNKNOWN |

Build rule (ruling (g)): **build once** from the committed tree with SDK 8.0.423 (`%LOCALAPPDATA%\Microsoft\dotnet\dotnet.exe`; the `dotnet` on PATH does not resolve `global.json`), **Release**, against the designated machine's AutoCAD build, with AutoCAD closed; then re-run the scans on those exact DLLs. The RS folder is **untracked** at HEAD b1fd5027: it must be committed before "the committed tree" means anything for it (`OI-1`). The repository `Directory.Build.targets` stamps the git HEAD into the DLLs, so their bytes change after any commit: the DLL hashes of any earlier build, including those printed as comments in `HASHES-RS.txt`, are informational only.

### 3.3 Hash verification table for the CAD manager

Computed from the working-tree files when this package was written (`sha256sum` over the file bytes; the bytes are LF, no CR, in both folders; the RS folder carries a `.gitattributes` `* -text`, the R0 folder is stored LF). The CAD manager recomputes each value on the machine and signs the third column. Any difference stops the process (stop condition 11).

**RS lists** (printed by `scan-rs`; HASHES-RS.txt records the same five values):

| File (`eng/research/I52Ct21dHostFactsRs/`) | SHA-256 | CAD manager: equal? |
|---|---|---|
| `allowed-apis-rs.txt` | `f14fe6fa02c1846843f665b2ec394b963f4fc83e1befa08730dc2ae88b23d266` | [ ] |
| `rs-waivers.txt` | `1bd95c25ef002b9830e9d7a9570a9ecabcc4c7b857750312eb24c00f3066baa5` | [ ] |
| `privileged-surface-rs.txt` | `9cf61870cb2c7b910f84045b2042db63a5abb6b92ca486e1bf5bdb962639480c` | [ ] |
| `privileged-callers-rs.txt` | `fa1daf86fd8662788f574f8295f1438cc2c464b6dbe28c5719369cde5b12a09d` | [ ] |
| `expected-commands-rs.txt` | `533997c8016f2fd0ea2336e1c72761ca44f14ecece502cb890f68dd7a7ef77a7` | [ ] |

**RS schemas:**

| File (`.../schemas/`) | SHA-256 | Equal? |
|---|---|---|
| `ct21d.designation.rs.v1.json` | `fe31f053a6b664985e5526191d1a9dfaff229983637cd1991e558a0867085a0f` | [ ] |
| `ct21d.dyncensus.v1.json` | `bdc8c90de5cba1d6d3d02414c101b5eb0b4c4703bf1b908c1b4d6e571e8cab76` | [ ] |
| `ct21d.rs-run.v1.json` | `8f0861910470cef70614144595917856b4dc7c503303040c65d1513c5bc1757a` | [ ] |
| `ct21d.tolscale-probe-table.v1.json` | `ed0dc0b66604b439fce2d52ea19b6c546bb19b020a8d0b9ab6b658a02a26c0fa` | [ ] |
| `ct21d.tolscale.v1.json` | `f377cc0e4c53ad6261292acafbaf6707b431b309bdde1813e8147aedb8bc1ad9` | [ ] |
| `ct21d.writeback.v1.json` | `04ded4aec9ad2829a9cf49f7ae103c14ed06b848281a6fc578a04b1800445ca6` | [ ] |

**R0 lists** (`eng/research/I52Ct21dHostFacts/`; the four values are the reference of decisions section 241 condition (a); each appears in decisions section 240):

| File | SHA-256 | Equal? |
|---|---|---|
| `allowed-apis.txt` | `b2cb0d26ac8dd5ce53a70845d62b2c3237e678aa586e4cc2f361687884a9d880` | [ ] |
| `privileged-surface.txt` | `7a56a8e77c841e0f481dba07dc0297714124ea716e978d3f2bbe497a1887949c` | [ ] |
| `privileged-callers.txt` | `278e8ccfa5da25a53614d4c54d958f0bf4c212f6e07eea62303da046899a3c07` | [ ] |
| `expected-commands.txt` | `f3465afd52bc856a1852a9881a2130c2f801c54d0ebbc9428f95c85fb0267a9f` | [ ] |

**R0 schemas, protocol and the RS review ledger** (computed for this package; the R0 schemas are the formats the R0 commands write):

| File | SHA-256 | Equal? |
|---|---|---|
| R0 `schemas/ct21d.census.v1.json` | `03bedbe6fb7b48fa669e16f166d2e5fbacfbc3c409af744295ec299ac85dc12d` | [ ] |
| R0 `schemas/ct21d.ctxvars.v1.json` | `74823e1add1d2343b5abf64d475880edc7e6fb6df4fdb0811491ee09ab4df754` | [ ] |
| R0 `schemas/ct21d.designation.v1.json` | `6ba2b81fc77bce9a9365ab0b4d384f23f27d907d8758646f369841d7538b73cf` | [ ] |
| R0 `schemas/ct21d.hostfact.v1.json` | `8108ba71583f2cca9ca2d214e3400d91ba93057e09bac97e108037001cf0ec76` | [ ] |
| R0 `schemas/ct21d.inventory.v1.json` | `bca6bd9de86529632f0748376ad17c1cf9fcee605602b7b40bb2d88d8000a87f` | [ ] |
| R0 `schemas/ct21d.machine-label.v1.json` | `388e2dab519dca5f100931cc3b015e0ff91aaba2fc45e03af825d30fd1c05717` | [ ] |
| R0 `protocol/I-1-observation-protocol.md` | `a608833e72d9a0401d8aa3c33496a6f8f3d098426ac8e72b3a563187df4037c8` | [ ] |
| R0 `protocol/i1-os-observation.ps1` | `662dd9ab686427077e8d178466ee2a625d494dad9592fe7f582202148d23ceb3` | [ ] |
| RS `HASHES-RS.txt` (review ledger of the RS sources; all 52 listed source files were re-hashed and **matched** when this package was written; the file set listed equals the folder's) | `7f190d2f5f16f36cb3860c6af484baae5e240a89d588abc2183bc875f64b16a1` | [ ] |
| RS `README.md` (informational) | `1ff7b05fa9685733087edd47ae75a203a6e347cbab7634469f1cab8e2f3f4874` | [ ] |
| R0 `README.md` (informational) | `5a47b53fcbadc9f27814dd80cea3e478c296ca57f3df70b910bf348d3569128b` | [ ] |

Note: the ledger was verified against the **current** bytes, but the Architect's delta review of the REQ-1..3 changes is not recorded here (P9). Per the Owner, any byte changed after the independent review invalidates the approval: P7 must name the review record and the ledger hash it covers.

**DLLs and tools (to be filled from the canonical build, P6):**

| Artifact | SHA-256 | `.pin` present and equal | Equal to the declared set / manifest |
|---|---|---|---|
| `I52Ct21d.HostFacts.R0.dll` | to be recorded from the canonical build | [ ] | [ ] |
| `I52Ct21d.HostFacts.Core.dll` (as placed for R0) | to be recorded from the canonical build | [ ] | [ ] |
| `I52Ct21d.HostFacts.Core.dll` (as placed for RS) | must equal the line above | [ ] | [ ] |
| `I52Ct21d.HostFacts.Rs.dll` | to be recorded from the canonical build | [ ] | [ ] |
| `I52Ct21d.HostFacts.Rs.Core.dll` | to be recorded from the canonical build | [ ] | [ ] |
| `I52Ct21d.HostFacts.Rs.Tools.dll` (tools-rs) | to be recorded from the canonical build | n/a | [ ] |
| `I52Ct21d.HostFacts.Tools.dll` (R0 tools) | to be recorded from the canonical build | n/a | [ ] |
| `scan-rs` report (exit 0, five list hashes printed equal to the table above) | hash of the saved report | n/a | [ ] |
| R0 `scan` report (exit 0, four list hashes printed equal) | hash of the saved report | n/a | [ ] |
| `run-designation-rs.json` of the session | `designationSha256` per run (verified immediately before each command) | n/a | [ ] |

The identity of the instrument for the gate is the declared set hashed in the tuple record, not the informational DLL hashes in `HASHES-RS.txt`.

### 3.4 What the RS commands write (for the cleanup and custody steps)

| Session | Result files (new, flat, create-new, through the single `EvidenceWriter`, under the evidence root) | Scratch files (under the scratch root) |
|---|---|---|
| S2 | `dyncensus-raw.json`, `hostfact-HF-C2.json`, `rs-run-dyncensus.json` | none |
| S3 | `writeback-raw.json`, `hostfact-HF-C4.json`, `hostfact-HF-C5.json`, `rs-run-writeback.json` | one DWG `CT21D_<id>.dwg` (OP2) |
| S4 | `tolscale-raw.json`, `tolscale-probe-table.json`, `rs-run-tolscale.json` (carries the `contentSha256` of `tolscale-raw`) | one DWG `CT21D_<id>.dwg` (OP2) |

`rs-run-*.json` declares each scratch file with its SHA-256. The scratch DWG is **not** byte-deterministic (GUIDs and timestamps), as design 2.4 step S6 says.

### 3.5 CAD-manager records (templates)

Every field is the CAD manager's; the drafter invents none. Package V2 sections 7.1 to 7.5 stand for the designation record, the TRUSTEDPATHS decision, the private copy, and the I-1 protocol; V3 adds or changes the following.

#### 3.5.1 Designation record

As package V2 section 7.1, with `packageReference = docs/initiatives/I-52-ct21d-host-gate-execution-package-v3.md @ <git blob id of the file as ruled>`.

#### 3.5.2 Machine-class record (published by the CAD manager before S1-A)

```text
CT21D_MACHINE_CLASS_RECORD
  recordId / designationRecordId / date / publishedBy
  machineIdentity               = <hostname, asset id, "ONE concrete stable physical machine">
  windowsVersionBuild           = <text and command>
  autocadExpected               = <product, version, build; acad.exe SHA-256 expected>
  autocadProfileExpected        = <profile name / identity expected>
  SECURELOADIntendedValue       = <decimal digits; not lowered>
  TRUSTEDPATHSIntendedValue     = <exact string per the decision record 3.5.4>
  machineClassLabel             = UNSET until S1-A and `tools label` produce it (MC-<12 hex>)
  noEquivalenceClaim            = THIS DESIGNATION IS NO EVIDENCE THAT OTHER MACHINES ARE EQUIVALENT
  changeRule                    = the label changes only if one of the six attributes of BA-10 V8 2.2 changes
```

#### 3.5.3 Tuple record

One record with two phases, plus the machine-class section, as package V2 7.3 sections A and B, restated against the Owner's RQ1 list. **MACHINE_CLASS and SESSION_BUILD_TUPLE are separate identities.**

```text
CT21D_MACHINE_SESSION_TUPLE_RECORD_PHASE1       (complete before acad.exe is started; one per session)
  recordId / sessionId (assigned here by the CAD manager as an identifier) / runId / planned session (S1-A | S1-B | S1-C | S2 | S3 | S4)
  machineIdentity / machineClassRecordId / machineClassLabel (= MC-<12 hex> for every session except S1-A)
  windowsBuild                  = <...>
  autocadExpected               = <build, profile>
  rackcadBuild                  = NONE
  instrumentBuild               = <the declared set of 3.2 with every SHA-256 and the .pin values>
  allowedApiControlHashes       = <the list hashes of 3.3, as printed by the scans>
  declaredSet / declaredSetSha256 / packageManifestSha256 (opaque, from the CAD manager) / buildTupleDigest
  authorizedPaths               = evidenceRoot, scratchRoot (RS), libraryPath, privateCopyPath, designation file path (local drive-letter paths only)
  configurationInputs           = catalog folder hashes, library SHA-256, template copy SHA-256, SECURELOAD / TRUSTEDPATHS intended values, HF-M4b only if declared
  attestations ready            = process-list snapshots (before / after), NETLOAD transcript or script hash (ruling (e))
  designationSha256             = <hash of the composed run-designation(-rs).json>
```

```text
CT21D_MACHINE_SESSION_TUPLE_RECORD_PHASE2      (filled immediately after the session starts; complete and VALIDATED before the first probe)
  pid / processStartUtc
  actualLoadedModules           = SESSION_START module list (file hashes), compared with the phase 1 baseline; modules outside the declared set = stop condition
  sessionProfileFacts           = AutoCAD version/build read, acad.exe SHA-256, profile, SECURELOAD, TRUSTEDPATHS, CPROFILE
  pinCheck                      = .pin files present and equal to the SHA-256 of each declared DLL (AFTER_NETLOAD hashes)
  runtimeIdentities             = documentIdentity / databaseIdentity of the active document where the command reads it
  processList                   = other acad.exe = none (attestation otherAcadProcess)
  validatedBy / validatedAtUtc  = <CAD manager> / <ISO 8601>     validationResult = VALID | INVALID
```

Phase 2 is signed **before** the designation is placed (3.7) and before the first characterization command.

#### 3.5.4 TRUSTEDPATHS decision

As package V2 section 7.2 (option (a), one recursive trusted root, decided and applied before the six strings are read; `SECURELOAD` not lowered). The trusted root holds the declared set, `.pin` files and the designation file, nothing else.

#### 3.5.5 Private library copy

As package V2 section 7.4: a fresh copy per run id and attempt, by the CAD manager, plain copy, A0 / B0 / A1 hashes equal; **outside** both the evidence root and the scratch root. Decisions section 241 RQ-2 puts it inside Q-O-0(b); RQ-11 gives S1-C its own copy.

### 3.6 The designation file and the UNSET rule (rule R3-1)

- Each instrument run reads `run-designation.json` (R0, schema `ct21d.designation.v1`) or `run-designation-rs.json` (RS, schema `ct21d.designation.rs.v1`) **next to the DLL**. The RS schema requires `runId`, `attempt`, `sessionId`, `evidenceRoot`, `scratchRoot`, `privateCopyPath`, `privateCopySha256`, `libraryPath`, `libraryFileSha256`, `governedDocumentPath`, `productPaths`, `hostChecks{otherAcadProcess, loadRouteScripted}`, `declaredSet[{path, sha256}]` and `tupleBinding{machineClassLabel, buildTupleDigest, packageManifestSha256, declaredSetSha256, designBlob, ba05Blob}`.
- The schemas accept `machineClassLabel = UNSET`; the instrument does not forbid it. **Rule R3-1: a designation or tuple record with `machineClassLabel = UNSET` is rejected for host runs of S1-B, S1-C, S2, S3 and S4.** The CAD manager checks it in P3 and the Coordinator in C-04. S1-A has no designation file and no loaded instrument; its tuple carries `UNSET` only for the field that S1-A itself produces (`OI-5`).
- `hostChecks.otherAcadProcess` and `hostChecks.loadRouteScripted` are **attestations** by the CAD manager (ruling (e)); the instrument relays them and refuses to run when `otherAcadProcess` is true or the load route is not scripted.
- The designation is not pinned by the instrument. Hence MIN-D: the authorization names both roots and `designationSha256`; the CAD manager verifies that hash immediately before each command.

### 3.7 Order of events inside one instrument session (rule R3-2)

```text
 0  PHASE 1 complete and signed; authorization (section 4) issued naming the scenario, both roots, designationSha256
 1  CAD manager verifies hashes of 3.3 / 3.4; verifies no acad.exe; creates the fresh evidence root and fresh scratch root (3.8); private copy (3.5.5); process-list snapshot BEFORE
 2  start acad.exe in the designated profile (AutoCAD's internal startup activity is not forbidden)
 3  PHASE 2 captured (pid, process start, module list SESSION_START, profile facts) and VALIDATED
 4  ONLY NOW the designation file is placed next to the DLL; its SHA-256 is verified equal to the authorized designationSha256
 5  NETLOAD of the declared DLL (script or transcript recorded; load outcome loaded / prompt / refused); AFTER_NETLOAD hashes; .pin check
 6  re-verify the designation hash immediately before the command (MIN-D)
 7  the command, typed once; command-line status noted
 8  process-list snapshot AFTER; profile reads again; end the AutoCAD process
 9  RS only: scratch-verify (3.8); on exit 0 copy scratch DWG(s) into the evidence root, compare hashes, seal (3.9)
10  R0: tools seal
```

If any step fails, the operator stops (section 5); nothing is repaired on the fly. The CAD manager does not edit or replace the designation or a DLL after step 4.

### 3.8 Scratch root hygiene and `scratch-verify` (ruling (f), (h))

- **One evidence root and one fresh, empty, dedicated scratch root per command (per run id).** Not reused for another command or attempt. The scratch root must differ from, and not nest with, the evidence root (RS README).
- Local fixed-drive path with a drive letter (MIN-A); not UNC; not under OneDrive, any synced folder or an indexed folder (the CAD manager records how sync and indexing were excluded); no reparse points in the root or its ancestors; the ACL limits access to the Windows account that runs AutoCAD. `[HOST-TO-CONFIRM]` how Windows applies the ACL to the AutoCAD process on the machine.
- The root is empty before the session (the instrument refuses a root holding anything not declared by an earlier `rs-run-*.json` of the same evidence root; with a fresh evidence root, any content is undeclared).
- **After the run and before any deletion**, the CAD manager runs `tools-rs scratch-verify <scratchRoot> <evidenceRoot>`. Exit 0 = clean (every entry declared, hashes equal, nothing extra at any depth, hidden files included, no reparse point). Exit 2 = not clean. A crash of the instrument or of AutoCAD is recorded with an explicit status and **the Coordinator rules it** (MIN-B).
- A companion file next to the saved DWG (backup, temporary, lock) is an undeclared entry: the run is **INVALID**. The file is left in place and listed; no allowance exists (ruling (b)).
- MIN-C is accepted by the Coordinator as a documented residual: between the guard's check that the target is absent and `SaveAs`, a foreign process could create the file; the exposure is bounded by the dedicated ACL-restricted empty root.

### 3.9 Custody and the cleanup ledger (ruling (a))

After `scratch-verify` returns 0 (RS sessions with a scratch DWG: S3, S4):

1. The CAD manager copies **each declared scratch DWG** into the evidence root, **create-new** (no overwrite; a name collision is a stop). The instrument does not copy.
2. The CAD manager computes the SHA-256 of each copy and compares it with the corresponding entry of `rs-run.scratch.files` in `rs-run-writeback.json` / `rs-run-tolscale.json`. A difference is a stop and makes the run INVALID.
3. `tools seal <evidenceRoot>` (writes `HASHES.sha256` and its digest; the evidence folder is flat and the seal reports a subdirectory as a difference). The seal covers the copied DWG.
4. Only after the seal, the CAD manager deletes the scratch originals. Each deletion is entered in the **cleanup ledger**; then the scratch root is verified empty and removed or retained empty, per the plan of P13.

```text
CT21D_CLEANUP_LEDGER   (one per run id; created in P13, empty)
  runId / sessionId / scratchRoot / evidenceRoot
  scratchVerifyExit             = <0 | other>    scratchVerifyOutputSha256 = <...>
  filesDeclared                 = <name, sha256 per rs-run.scratch.files>
  copiesInEvidenceRoot          = <name, sha256, equal to declared: yes/no>
  sealDigest                    = <HASHES.sha256 digest>   sealedAtUtc = <...>
  deletions                     = <name, deletedAtUtc, deletedBy>   (only after the seal)
  scratchRootStateAfter         = <empty | removed>    privateCopyDisposition = <retained | removed, by whom, when>
  companionFilesObserved        = <none | list>        (any entry => INVALID, Coordinator ruling)
  signedBy                      = <CAD manager>
```

R0 sessions write only through the `EvidenceWriter` and have no scratch root: their cleanup ledger records the private copy only.

## 4. FIRST_NON_GOVERNING_HOST_RUN authorization template (one parametrized template; the Coordinator fills it)

The authorization is reserved to the Coordinator. **This template is empty. Nothing in it is a ruling.** One authorization per session; the `session` field selects S1-A, S1-B, S1-C, S2, S3 or S4. Any unchecked condition, any missing evidence or any change of a pinned item voids it. The first authorization covers **the session named in it only**; every later session needs its own.

```text
CT21D_FIRST_NON_GOVERNING_HOST_RUN_AUTHORIZATION
  decisionsSection              = <number, EMPTY until the Coordinator writes it>
  date / coordinator            = <ISO 8601> / <name>
  packageReference              = docs/initiatives/I-52-ct21d-host-gate-execution-package-v3.md @ <git blob id of the file as ruled>
  session                       = <S1-A | S1-B | S1-C | S2 | S3 | S4>
  command(s), in order          = <S1-A: none (I-1 protocol); S1-B: CT21DHG_CENSUS_R, CT21DHG_INVENTORY; S1-C: CT21DHG_CENSUS_R, CT21DHG_CTXVARS;
                                   S2: CT21DHG_CENSUS_DYN; S3: CT21DHG_DIMWRITEBACK; S4: CT21DHG_TOLSCALE>
  runId                         = <HGP-H<n>-<yyyymmddThhmmssZ>-<nn>>     (S4: HGP-H4-...)
  evidenceRoot / scratchRoot    = <local drive-letter path> / <local drive-letter path or "none" (R0, S2)>      (MIN-A, MIN-D)
  designationFile / designationSha256 = <path> / <sha256>        (placed only after phase 2 validates; verified immediately before each command)
  tupleRecordPhase1             = <record id> @ <SHA-256>        machineClassLabel = <MC-... | UNSET only for S1-A>
  buildTupleDigest / packageManifestSha256 / declaredSetSha256 = <sha256> / <sha256> / <sha256>
  operator                      = <CAD manager>      (the agent does not operate the host)
  GOVERNING = FALSE     RUN_STATUS_CLASS = NON_GOVERNING     GATE = HOST_GATE_BA11_3_3     CLASSIFICATION (RS) = CONTROL_PLANE_AUXILIARY_WRITES

  CONDITIONS (check each one only with the evidence named)
  [ ] C-01 machine designated by the CAD manager (P1)                              evidence: designation record, SHA-256, signer, role
  [ ] C-02 machine-class record published (P2); TRUSTEDPATHS decision applied (P2b) evidence: records, SHA-256, the value set
  [ ] C-03 tuple record PHASE 1 complete (P3) incl. freeze statement, module baseline hashed file by file, authorized paths, configuration inputs
                                                                                  evidence: tuple record, SHA-256
  [ ] C-04 machineClassLabel is MC-<12 hex> (rule R3-1), or this is S1-A          evidence: the tuple record line; the label file from S1-A
  [ ] C-05 session-binding mechanism ready (P4): phase 2 procedure, I-1 script / transfer, process-list and NETLOAD attestations (ruling (e))
                                                                                  evidence: the prepared phase 2 form; script hash
  [ ] C-06 declaredSet and .pin published (P5); the Core DLL is one set of bytes for R0 and RS   evidence: 3.3 DLL table, equal Core hashes
  [ ] C-07 canonical build once from the committed tree, SDK 8.0.423, Release; scan-rs and R0 scan re-run on those exact DLLs, CLEAN, with the printed list hashes equal to 3.3
                                                                                  evidence: build log hash, committed SHA, both scan reports and their hashes
  [ ] C-08 independent source review recorded (P7): record NAMED = <id/path>, byte set covered = HASHES-RS.txt <sha256>, no byte changed since
                                                                                  evidence: the review record; ledger verification (tools-rs ledger-verify exit 0)
  [ ] C-09 Architect review recorded (P9): ARCHITECT_RS_REVIEW and the delta review of REQ-1..3 recorded at <where>
                                                                                  evidence: the Architect entry; the delta review entry
  [ ] C-10 R0 class review (section 241) holds; the four R0 list hashes unchanged (S1-B, S1-C)  evidence: section 241; hashes of 3.3
  [ ] C-11 hashes verified by the CAD manager (P10): every row of 3.3 and 3.4 signed   evidence: the signed table
  [ ] C-12 exact scenario and command(s) named above; run id, session, order of commands correct against 2.1
                                                                                  evidence: this authorization
  [ ] C-13 paths: evidence root and (RS) a fresh, empty, dedicated scratch root per 3.8 (local fixed drive, not synced / indexed, ACL limited, no reparse points); private copy outside both
                                                                                  evidence: listing, ACL output, CAD manager statement
  [ ] C-14 cleanup plan and cleanup ledger opened (P13); scratch-verify, custody copy, seal and deletion steps of 3.9 understood
                                                                                  evidence: the empty ledger; the plan
  [ ] C-15 private library copy made for this run id (P12); A0 = B0 = A1                  evidence: the hash outputs
  [ ] C-16 Coordinator confirms the run belongs to the authorized non-governing gate (Owner discipline step 3; membership line, package V2 7.7 form)
                                                                                  evidence: the decisions entry
  [ ] C-17 no AutoCAD open on the machine other than the session (otherAcadProcess = false attested); no pending Windows / AutoCAD update; process-list snapshot before
                                                                                  evidence: attempt log first line; snapshot file hash
  [ ] C-18 stop conditions (section 5) read and signed by the operator
  [ ] C-19 no-automatic-retry rule accepted: no further attempt without the Coordinator's per-occurrence authorization; PARAM-04 = 3 is a per-slot capacity cap, not a retry permission; EVIDENCE_REPETITION = 1 does not reduce N_A, N_B, N_det or N_clean
                                                                                  evidence: operator's signed statement
  [ ] C-20 status of the Owner's possible objection to the Coordinator's readings recorded (P18)   evidence: the decisions entry
  [ ] C-21 S3 / S4 ordering: for S4, S3 completed, its outcome recorded, no companion file observed (ruling (b))   evidence: decisions entry on S3
  [ ] C-22 S4 only: PRE-1..PRE-4 closed, pre-registration entry sealed, R-M ruling on R1(b) given   evidence: the entries (P19)
  [ ] C-23 package ruled by blob id (P20); state lines unchanged (below)                evidence: decisions entry; `git hash-object` of the file as ruled

  RESULT                        = FIRST_NON_GOVERNING_HOST_RUN: AUTHORIZED | NOT_AUTHORIZED     (Coordinator)
  SCOPE                         = the named session only, on the designated machine, under the recorded tuple. No governing run, no ADMISSION_EVALUATION, no admission,
                                  no G3 reopening, no CT-50, no Owner Act 2, no replacement Freeze, no W-scan residual acceptance, no final reduced guarantee acceptance
  VOID WHEN                     = any C-xx changes; any pinned hash differs; any stop condition of section 5 occurs; the designation hash differs at the pre-command check
  AFTER                         = the result is NON_GOVERNING evidence; it does not change CT21D_AUTHORITY_BASELINE_READY, CT21D_EXECUTION_READY or CT21D_EXECUTION
```

State lines to be repeated unchanged in any authorization: `CT21D_AUTHORITY_BASELINE_READY = FALSE`, `CT21D_EXECUTION_READY = FALSE`, `CT21D_EXECUTION = NOT_AUTHORIZED`, `HOST_RUN_STARTED = FALSE` until a run starts, `G3 = STOPPED`, `CIA = UNKNOWN`, `FALLBACK = ALT-21E`.

## 5. Stop conditions and handling of FAIL / UNKNOWN / INVALID

### 5.1 Stop conditions (all sessions)

The operator stops, writes the attempt-log line and informs the Coordinator. No step is "fixed on the fly". Package V2 8.1 items 1 to 13 stand. V3 adds:

14. A hash of 3.3 / 3.4, a `.pin`, or the designation hash differs from the authorized value at any check (start, after phase 2, immediately before the command, after the run). The Core DLL differs between its R0 and RS placements.
15. The `otherAcadProcess` or `loadRouteScripted` attestation cannot be made, or the process-list snapshots before and after differ in `acad.exe` processes.
16. Phase 2 is incomplete or not validated when the first probe or characterization command is about to be typed.
17. `scratch-verify` returns anything but 0; a companion file, undeclared, hidden, nested, missing or changed entry, or a reparse point is found in the scratch root.
18. A custody copy (3.9 step 1) would overwrite, or its SHA-256 differs from `rs-run.scratch.files`.
19. A second typing of the command is refused (a refusal is not an execution; the operator logs it and does not retry).
20. The instrument writes anything outside the evidence root and the scratch root (other than the private copy and CAD-manager inputs); any write to the governed document, the original library DWG, a product file, `src/`.
21. A crash of AutoCAD or of the instrument: recorded with an explicit status; the Coordinator rules it (`CRASH_FINDING` or `CRASH_ENVIRONMENT`, package V2 8.2).

### 5.2 Classification (existing contract; no new category)

Package V2 section 8.2 stands (a valid run is `NON_GOVERNING`; an unreadable fact is `UNKNOWN`; an unobservable datum `NOT_OBSERVABLE`; absent data `NOT_OBSERVED`; a failed host-run check or any item of the INVALID list is `INVALID` for every fact of the run, all evidence retained). RS additions:

| Case | Treatment |
|---|---|
| a companion file after `SaveAs` (S3, S4) | `INVALID`; no allowance; the file is retained and listed; Coordinator ruling before any further RS session |
| `scratch-verify` exit 2, undeclared entry, hash mismatch, side database not disposed, `DBMOD` / `SECURELOAD` / `TRUSTEDPATHS` / `CPROFILE` / document identity changed during the run, private copy or library changed, the offline recomputation of S4 mismatches | `INVALID` (the instrument's own invalid reasons, README) |
| S4 row outcomes | state machine as in ruling (d): precedence INVALID > FAIL > UNKNOWN > PASS; a construction rejected by the host is `FAIL`; a row with rotation or normal not as expected is `UNKNOWN`. A FAIL / UNKNOWN / INVALID of S4 remains governed by the existing contract; it is not repeated to obtain a better result |
| the result record of a run whose post-processing failed | the instrument writes a minimal `rs-run-*.json` (result INVALID, `POST_PROCESSING_FAILED:<type>`) so the scratch DWG is declared; best effort (README, review round 1 (b)) |
| a crash | explicit status, Coordinator ruling; never silently invalid; no automatic retry. MIN-H is deferred: the slot is consumed and the field reads fail-closed `NOT_RECORDED` |
| `INVALID` or a crash | no automatic retry. A rerun needs the Coordinator's per-occurrence written authorization, a new run id, a recorded cause (tooling, environment, contamination); it is the same session, same package and same tuple requirements. A change of activity, package or instrument is a new version, not a retry |

### 5.3 No automatic retry; what the numbers mean

- No attempt after an `INVALID` or a crash begins without the Coordinator's written authorization for that occurrence. `nn` counts attempts; it is a log number, not a budget.
- `PARAM-04 = 3` is `MAX_ATTEMPTS_PER_SLOT`, a capacity cap per slot; these sessions are not governing slots; the value **is not a permission to retry**.
- `EVIDENCE_REPETITION = 1` means one evidence package per execution and one execution per session. It does **not** replace or reduce `N_A = 44`, `N_B = 29`, `N_det = 90` or `N_clean = 59`; those counts are not applied to these sessions and nothing here reduces them.
- Every attempt is listed in the attempt log (attempt number, tuple, contract version, state `NON_GOVERNING` or `INVALID`, outcome, reason for stopping). Invalid attempts are disclosed.

## 6. What stays NOT authorized

- A governing CT-21D campaign; `ADMISSION_EVALUATION`; product admission; G3 reopening; CT-50; Owner Act 2; a replacement Freeze; acceptance of the W-scan residual; acceptance of the final reduced guarantee. Currently admissible kind scope = NONE; `SafeOperationalState = FALSE_FOR_ADMISSION`.
- Any host run before the Coordinator's FIRST_NON_GOVERNING_HOST_RUN authorization for that session; any session without a recorded IN; any command outside those named in 2.1.
- S4 (`CT21DHG_TOLSCALE`) until PRE-1..PRE-4, the sealed pre-registration entry and the R-M ruling on R1(b) are closed; choosing any `TOL_SCALE` value (it stays `UNSET`).
- `HF-G2`; H5, H6, H7 (fixtures); I-7 conformance; I-8; the PRODUCT_BUILD; `RACKMIRROR`; product seams.
- Writes to the governed AutoCAD document; saving the governed drawing; the original library DWG (read only from a private copy); `src/`; arbitrary or non-allowlisted paths; side-database use to change the governed document indirectly; undeclared persistent state between runs; turning a non-governing run into a governing one.
- `HDM-CHAR-*`, `E4-LEARN-R1-*`, `LK-*`, `WU-DRY-*`, BA-06 dynamic-trace dry runs, qualification of instruments, `EV-L-*`, `EV-V-*`, manifest / custody / anchor capture of the governing campaign, UNDO evidence.
- Loading any instrument before the authorization; using `eng/validation/I52G3AAutoCadProbe` or `eng/research/I52Ctda` in the gate (reference material only).
- Any change under `src/`, `tests/`, existing `eng/` folders, a solution or CI file by this package. Treating any result as governing because it ran in AutoCAD; treating the designation as evidence that other machines are equivalent; transferring evidence between tuples.

## 7. Open items

### 7.1 For the Coordinator

| Id | Item |
|---|---|
| OI-1 | The RS folder `eng/research/I52Ct21dHostFactsRs/` is untracked at HEAD b1fd5027 and `docs/automation/decisions/I-52.md` is modified. Ruling (g) says "build once from the committed tree": the RS tree, HASHES-RS.txt and the REQ-1..3 changes must be committed (under the Coordinator's gate) before the canonical build, and the commit SHA recorded in P6 |
| OI-2 | The independent source review of the RS instruments and the Architect's **delta review** after REQ-1..3 are not recorded in the repository files read. This package does not claim either. Name the records (C-08, C-09) |
| OI-3 | Run-id numbering: the RS schema accepts `HGP-H<digits>-...`; only TOL_SCALE is bound to `H4` by the instrument. Which H numbers do S2 (U-RS-1) and S3 (U-RS-2) take? (Package V2 used H2-DYN and H3a as activity names, which the run-id pattern does not allow as written) |
| OI-4 | S1 shape: this package keeps three separate fresh processes (S1-A, S1-B, S1-C) as package V2 3.4 and section 241 RQ-11 require. The Coordinator's wording "Session S1" may intend one group; confirm |
| OI-5 | R3-1 (UNSET rejected) is applied from S1-B onward; S1-A necessarily carries `UNSET` for the label it produces. Confirm, and confirm that the rule is a CAD-manager / Coordinator check since the schemas accept `UNSET` |
| OI-6 | R3-2 reconciliation: MIN-D needs the designation hash in the authorization **before** the run, while R3-2 places the file only **after** phase 2 validates. V3 resolves it by composing the content in phase 1 (the `sessionId` is an identifier the CAD manager assigns; nothing in the `tupleBinding` is born with the session), hashing it, and placing it later. Confirm the reading |
| OI-7 | Custody (3.9) puts the copied scratch DWG into the evidence root before `tools seal`. The seal tool accepts any flat files (it reports only subdirectories); confirm that a DWG there is acceptable, and that the sealed evidence-root hash is the custody anchor for it |
| OI-8 | `packageManifestSha256` is an opaque CAD-manager value (decisions section 241 RQ-4 / RQ-10; I-8 not opened). Confirm it stays opaque for V3 |
| OI-9 | Whether `governedDocumentPath` must equal `DWGPREFIX + DWGNAME` (the instrument does not compare them; RS README) is open; the V3 draft leaves it to the CAD manager's tuple record |
| OI-10 | The scan "holes" the review found (ldftn laundering, uncounted `new Database` / `SaveAs` / `ReadDwgFile` sites, `File.ReadAllText` in `RsGateCommands`) are caught by the source review and hashes, not by the scan (RS README); the independent reviewer's record should state that it covered them |
| OI-11 | A "started" marker (single-execution across crashes before any file exists) was not added by the implementer (README); the Coordinator decides whether the attempt log is the substitute |
| OI-12 | Hardware, account, HF-M4b and catalog hashes remain CAD-manager inputs; no machine is designated |

### 7.2 For the Owner

| Id | Item |
|---|---|
| OQ-1 | Objection window: the Coordinator's readings of decisions section 242 (a) and (b) (side database via `new Database(...)` only; saved only as a NEW scratch file; the library only from a private copy) and of Q-O-0 are subject to the Owner's objection |
| OQ-2 | Whether the Owner wants to attend or sign the first RS session (S2); not required by any source |
| OQ-3 | Q-O-2 only if the M1 coverage of `RB` ranges is too small (HF-G2 not opened) |

## 8. Status

```text
HOST_GATE_PACKAGE                = V3_PREPARED_FOR_COORDINATOR_DECISION
HOST_GATE_BA11_3_3               = AUTHORIZED BY THE OWNER (NON_GOVERNING ONLY; G6 EXTENDED; Q-O-1 = ALSO_SIDE_DB_WRITES_AND_NEW_FILES); NO MACHINE DESIGNATED BY THE CAD MANAGER IN THE REPOSITORY; NO TUPLE RECORD; NO COORDINATOR AUTHORIZATION OF A FIRST RUN
FIRST_NON_GOVERNING_HOST_RUN     = NOT_AUTHORIZED (section 4 template empty)
HOST_RUN_STARTED                 = FALSE
INSTRUMENT_IMPLEMENTATION_GATE   = OPEN_FOR_AUTHORIZED_RESEARCH_INSTRUMENTS (Owner, decisions section 242); R0 CLASS REVIEWED (section 241); RS CLASS IMPLEMENTED, NOT LOADED, NOT RUN
ARCHITECT_RS_REVIEW              = APPROVED_WITH_REQUIRED_CHANGES; REQ-1..3 APPLIED; DELTA REVIEW PENDING / RECORDED ELSEWHERE (as communicated; not verified in the repository)
SESSIONS PREPARED                = S1-A, S1-B, S1-C, S2, S3 (each after section 3 and its authorization)
SESSION BLOCKED                  = S4 (PRE-1..PRE-4, sealed pre-registration entry, R-M ruling on R1(b), S3 outcome)
LATER GATE                       = H5, H6, H7, I-7 (PRODUCT_BUILD absent); HF-G2 NOT OPENED
TOL_SCALE VALUE                  = UNSET
DLL HASHES                       = TO BE RECORDED FROM THE CANONICAL BUILD
CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
CURRENTLY_ADMISSIBLE_KIND_SCOPE = NONE     CIA = UNKNOWN     SafeOperationalState = FALSE_FOR_ADMISSION     G3 = STOPPED     FALLBACK = ALT-21E     OWNER_ACT2 = RESERVED
HOST_GATE_PACKAGE = V3_PREPARED_FOR_COORDINATOR_DECISION
HOST_RUN_STARTED = FALSE
```

## 9. Coordinator rulings on the open items of section 7 (decisions section 245)

| Open item | Ruling |
|---|---|
| Run ids of S2 and S3 (the RS run-id pattern is `HGP-H<digits>-...`) | S2 `CT21DHG_CENSUS_DYN` uses activity number **H8**; S3 `CT21DHG_DIMWRITEBACK` uses **H9**; S4 `CT21DHG_TOLSCALE` keeps **H4**. The names `H2-DYN` and `H3a` of package v2 are history. The authorization of each session names its run id |
| Designation timing | Confirmed: the designation file is composed and hashed in phase 1, the authorization names that hash, and the file is placed in the instrument folder only after phase 2 validates; the CAD manager verifies the file hash immediately before each command |
| `machineClassLabel = UNSET` | Confirmed: a host record carrying `UNSET` is rejected from S1-B onward (S1-A produces the label); the check is the CAD manager's and the Coordinator's, because the schemas accept `UNSET` |
| Scratch DWG copied into the evidence root before `tools seal` | Confirmed as intended (Architect ruling (a), section 244): create-new copy, SHA-256 compared with `rs-run.scratch.files`, then `tools seal`; scratch originals are deleted only after the seal and the deletion goes in the cleanup ledger |
| Commit state | The RS tree and this package are committed with decisions section 245; the canonical build is made from that commit. Any byte change after the independent reviews invalidates them |
