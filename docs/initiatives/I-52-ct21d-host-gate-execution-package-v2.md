# I-52 — CT-21D Host-Gate Execution Package V2 (non-governing host characterization / baseline-preparation evidence)

> ```text
> DOCUMENT            = host-gate execution package V2, prepared for the Coordinator's review (decisions section 237, "NEXT WORK" item B:
>                       "corregir host package v2 / AR10-07"; decisions section 239 condition 3 and condition 4). Not a baseline artifact;
>                       not in the BA-11 registry; not a contract. It decides no value and authorizes nothing
> ROLE                = IMPLEMENTER (documents)
> STATUS              = V2_PREPARED_FOR_COORDINATOR_REVIEW. NO HOST RUN HAS STARTED. NO HOST RUN MAY START BEFORE THE COORDINATOR'S
>                       FIRST_NON_GOVERNING_HOST_RUN AUTHORIZATION (section 6), WHICH DOES NOT EXIST
> SUPERSEDES          = nothing: package V1 (docs/initiatives/I-52-ct21d-host-gate-execution-package-v1.md, blob b23221a8b4cb8f2b30135b1e811456be2cbb976c)
>                       stays untouched as history. V2 replaces V1 for the next gate ruling only
> AUTHORITY_CONTRACT  = CT-21D-V2.6 AGREED (decisions section 230; composite 97100cff11868807f3c49f25bf25d0385c86e8873cba0cf2c1996338b9b98eb4)
> BOUND_PRODUCT_SHA   = 95690c28dc6268e61dff32a0cbc33cc9fde3d47f (non-tuple bound item; `main`; `git diff --stat main HEAD -- src` is empty
>                       in this worktree, HEAD fc3cbe144815295f7d32bf25566debe3da2bf728)
> OWNER AUTHORITY     = decisions sections 233 and 237, verbatim records:
>                       docs/automation/evidence/I-52-ct21d-owner-decisions-q-o1-q-o5-and-host-gate.md        cdb1a13682c63571cf4a4b4bb03bbf0fb0dc19ec
>                       docs/automation/evidence/I-52-ct21d-owner-decisions-machine-g6-instruments.md          2feb0bec63fd712cb28cc5209b6dffb8429a0e3b
> SOURCES (git blob ids of the working tree, `git hash-object`; every fact below was read in these files)
>                       docs/automation/decisions/I-52.md (sections 234 to 239)                                f0c13c1aed0123cb71bfb263c940778c9f55f356
>                       docs/initiatives/I-52-ct21d-host-gate-execution-package-v1.md                          b23221a8b4cb8f2b30135b1e811456be2cbb976c
>                       docs/initiatives/I-52-ct21d-tol-scale-design-and-host-fact-matrix-v1.md                dc2897e5ee0c761f29d964215f7b23acb9627c1b
>                       docs/initiatives/I-52-ct21d-baseline-ba-05-fingerprint-specification-v5.md             b74af94ece4f0901a071ef5176f3c58bff01cfdc
>                       docs/initiatives/I-52-ct21d-baseline-ba-08-selective-kind-baseline-v7.md               57330c3a09d80e919a63a2b83a45d168da326140
>                       docs/initiatives/I-52-ct21d-baseline-ba-10-parameter-sheet-v8.md                       820d90c8a2172ad32697e66f0c9b0f3eb5fadbdb
>                       docs/initiatives/I-52-ct21d-baseline-ba-11-hash-registry-v10.md                        9984e9f8ff6675afa514656e49d5a2fa01756916
>                       docs/initiatives/I-52-ct21d-authority-contract-v2.1.md                                 6259a3beb7445a63620636acd696cdad689965b3
>                       docs/initiatives/I-52-ct21d-authority-contract-v2.6.md                                 fd9c16873c526e5f009ad92beb8dd2fe236b964f
>                       eng/validation/I52G3AAutoCadProbe/ProbeCommands.cs                                     51189e537606b26d13686ec7c7f246af2173fa89
>                       eng/research/I52Ctda/README.md                                                         e8cd6df4ac04d5f991ab07f655778a1ee2162661
>                       deploy/build-bundle.ps1                                                                9a4e6adecf935b90b52f97b1450aac30970d48f1
>                       src/RackCad.Application/Catalogs/BlockLibrary.cs                                       ce8042144e645a6408d7c3284b37e8808d2b4632
>                       commit 69daf03a35c630e453e1d9e98136f128bd0325a4, tree 3c34dc9be113e98088acf56653aa5181437d0893 (`git rev-parse 69daf03a^{tree}`)
> INSTRUMENT FOLDER   = eng/research/I52Ct21dHostFacts/ (R0 class; untracked in this worktree when this package was written, so no blob id is
>                       cited for any of its files; its identity for the gate is the declared set of section 4.3 as hashed in the tuple record, never this text)
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> HOST_RUN_STARTED = FALSE     INSTRUMENT_IMPLEMENTATION_GATE = OPEN (R0 CLASS ONLY: I-0, I-1, I-2, I-4, I-9)
> ```

## 0. Reading rules

- This package **prepares** the first non-governing host gate. It runs nothing, authorizes nothing, starts nothing. The Owner's authorization is
  the one of decisions sections 233 and 237 and is not widened here. Where a source leaves a point open the row says `RQ-n` and the question is
  listed in section 11; the implementer decides none of them.
- **UNKNOWN** means: not established in the repository. No hash, build, path, count or approval is invented. Fields that only the CAD manager
  or the host can supply appear as `<...>` in a template and are empty in this package.
- `[HOST-TO-CONFIRM]` marks a behaviour of Windows or AutoCAD that no file of the repository establishes (the design uses the same marker).
  The raw output on the designated machine governs; a mismatch makes the fact `UNKNOWN`, never a corrected guess.
- **Who operates.** The Owner or the CAD manager, on the designated machine. **The agent does not operate the host and does not start any host
  run**; nothing in sections 233 to 239 says otherwise. A later gate would have to say so in terms and bound it.
- **State of the instruments (stated once).** The R0 instruments exist in `eng/research/I52Ct21dHostFacts/` and are described in this package only
  as **implemented; independent review under decisions section 239 condition 1 NOT RECORDED in the repository (Coordinator confirmation open)**. This
  package therefore lists the independent read-only review and the Coordinator's confirmation of the set as evidence the Coordinator must verify
  before any run (section 5, P5; decisions section 239 condition 1). No instrument has been loaded into AutoCAD and none has been run on any host.
- Spanish passages are the Owner's or the Coordinator's words, quoted verbatim; the English around them is the implementer's.

### 0.1 What V2 changes with respect to V1

| Change | Where |
|---|---|
| the three minors of AR10-07 (decisions section 235): the last row of the classification table reads "INVALID o caida"; H4, H5 step 2 and H6 are marked `IN, condicional`; one sentence on `eng/validation/I52G3AAutoCadProbe` as a reference probe only | 3.3 (probe sentence), 8.2 (last row), 3.1 and 12 (marks) |
| activity list H1..H7 rewritten against the exact host-fact matrix of the Architect design (blob dc2897e5...); the H3 split (H3a / H3b) follows design 4.1; the H2 / H2-DYN split is the implementer's own | 3 |
| the Coordinator's rulings of sections 236 and 239 and the Owner's readings R8 / AR10-03(b) (section 237) applied | 2, 4 |
| `MACHINE_CLASS` and `SESSION_BUILD_TUPLE` recorded separately; the declared instrument set | 4, 7.3 |
| the PRE-RUN checklist in the Owner's order and the FIRST_NON_GOVERNING_HOST_RUN authorization template | 5, 6 |
| CAD-manager procedures: designation record, TRUSTEDPATHS decision, tuple record, private library copy, I-1 protocol | 7 |
| stop conditions, no automatic retry, `PARAM-04` and `EVIDENCE_REPETITION` readings | 8 |
| tooling gaps T-1..T-9 of V1 replaced by the instrument list I-0..I-9 and their state | 9 |
| what stays not authorized | 10 |

## 1. Authority (quoted)

### 1.1 The gate (decisions section 233; Owner message of 2026-09-30; verbatim record cdb1a136...)

V1 section 1 quotes the Owner's gate text (`HOST_GATE_BA11_3_3 = AUTHORIZED`, "NON_GOVERNING HOST CHARACTERIZATION / BASELINE-PREPARATION
EVIDENCE", the list of what it does not authorize) and the machine policy Q-O5; they stand as quoted in V1 section 1.2 to 1.4 and in the Owner record
above. The six-step "HOST EXECUTION DISCIPLINE" is reproduced here verbatim from the Owner record cdb1a136...:

```text
HOST EXECUTION DISCIPLINE

Before first host run:

1. CAD manager designates exact machine.
2. Record exact machine/session tuple.
3. Coordinator verifies requested scenario belongs to the authorized
   non-governing host gate.
4. Use exact build/package required by the current baseline-preparation
   authority.
5. Preserve raw evidence.
6. No result is governing merely because it ran in AutoCAD.

Any FAIL / UNKNOWN / INVALID remains governed by the existing contract.
```

The line that follows it in the same record, which also drives V2, is:

```text
No automatic retries.
```

### 1.2 The Owner's decisions of section 237 that V2 applies (verbatim, record 2feb0bec...)

```text
G6 = EXTEND

Amplío BA-11 §3.3 únicamente para caracterización host NO gobernante,
READ-ONLY, de los hechos adicionales necesarios para BA-05:

- tipos/rangos RB;
- variables de contexto;
- cualquier otro host-type fact expresamente requerido por la especificación
  actual de BA-05.

Esta ampliación:

- NO autoriza governing CT-21D;
- NO autoriza product admission;
- NO autoriza escrituras de producto;
- NO autoriza expandir el alcance más allá de los facts concretos que BA-05
  necesite.

Toda observación debe quedar ligada a build/machine/session tuple exacta.
```

```text
R8_READING = ACCEPT
Cargar mediante NETLOAD una build RackCad específicamente fijada para una corrida constituye identidad de BUILD/TUPLE.
No se trata por sí mismo como "plugin change" prohibido si:
- la build estaba declarada previamente para esa corrida;
- su DLL/package/hash está fijado;
- la sesión empieza bajo esa identidad;
- no hay sustitución silenciosa durante una corrida.
Una build diferente => nueva tuple.
No transferir evidencia automáticamente entre builds.

AR10_03B_READING = ACCEPT
Cambio de módulos, actualización o build de software: => nueva SESSION/BUILD TUPLE.
No implica automáticamente nueva MACHINE-CLASS LABEL.
La machine-class label cambia únicamente si cambia algún atributo que BA-10 define normativamente como parte de la clase de máquina.
Registrar ambas identidades por separado:

MACHINE_CLASS
SESSION_BUILD_TUPLE
```

The Owner's section 8 ("HOST GATE STATUS") of the same record, verbatim:

```text
La compuerta de host no gobernante permanece AUTHORIZED, ahora ampliada por
G6, pero todavía requiere antes del primer run:

1. machine designation por CAD manager;
2. tuple record completo;
3. exact build/package;
4. Architect TOL_SCALE design donde aplique;
5. required read-only instruments implementados y revisados;
6. Coordinator confirmation de que cada requested run pertenece al host gate
   autorizado.

NO iniciar host antes de cumplir esos prerrequisitos.
```

Prerequisite 5 says "implementados y revisados"; the repository does not record that review (section 0, P5).

The Owner accepted R8 with one precision that V2 must reflect (decisions section 237): **the build is declared before the run, its hash is pinned, and
there is no substitution during the run.** V2 applies it in 4.2 and in the authorization template (section 6, C-06 and C-07).

### 1.3 Scope reading kept from V1

The authorization covers only the activities the current workflow already identifies as needed to close baseline artifacts and that BA-11 3.3 (and the
G6 extension) name. Anything BA-11 3.3 does not name is not in the gate (V1 section 1.5, Coordinator ruling R-1 and R-2 of section 236).

## 2. Rulings applied (sections 236, 237, 239)

| Ruling | Text (short) | Effect in V2 |
|---|---|---|
| 236 R-1 | "otros K-*": empty | only K-2, K-3, K-8 and the machine-attribute observation (BA-11 3.3), plus the G6 extension |
| 236 R-2 | `HDM-CHAR-*`, `E4-LEARN-R1-*`, `LK-*` outside | not in any activity of V2 (section 10) |
| 236 R-3 | write probe of AR4-10 (G5) IN "sobre una base de datos de trabajo"; G6 OUT until explicit extension | G6 now extended by the Owner (section 237); the write probe is HF-C4, an RS instrument, blocked by Q-O-1 (3.2) |
| 236 R-4 | only `FX-ANN-BLANK` (build `69daf03a`) and `FX-IMP` advance; the other fixtures wait for the exact build | H5, H7 per 3.1; H6 `IN, condicional` |
| 236 R-5 | new folder under `eng/research/`, read-only, Architect review, final and loaded in the module baseline before H1 | the declared set (4.3); the folder is `eng/research/I52Ct21dHostFacts/` |
| 236 R-6 | evidence location and naming as V1 3.5 proposes | 7.6 |
| 236 R-7 | one execution per activity (`EVIDENCE_REPETITION` = 1); H4 per its design | 8.4 |
| 236 R-8 | a `NETLOAD` of a pinned build outside the installed package, in its own session, is a build-tuple fact; installing, updating or removing machine plugins is a plugin change | 4.2 (ratified by the Owner with the R8 precision) |
| 236 R-9 | manual operation, non-governing; interactive `acad.exe` when the activity loads RackCad; Core Console for the census if sufficient | 3.1; 239 R-I: interactive `acad.exe` for everything (the Core Console is not used by this package) |
| 236 R-11 | Release | 4.3, 9 |
| 236 R-12 | classification table of 4.0 with the AR10-07 correction | 8.2 |
| 236 R-13 | real library: the copy is private and hashed; the census record may be proposed to the repository without machine data | 7.4, 7.6 |
| 237 point 8 | six prerequisites before the first run | section 5 |
| 239 Q-O-0 (a), (b) | R0 side database from a private copy and new result files through the single `EvidenceWriter`: DENTRO de "solo lectura"; Coordinator's reading, **subject to the Owner's objection** | the set formerly gated by Q-O-0 is released (3.1); the objection status is a checkbox (C-17) |
| 239 Q-O-1 | goes to the Owner; I-3, I-5, I-6, the HF-C4 write probe and the OP2 save stay blocked | 3.2; it also decides the final declared set (4.3) |
| 239 Q-O-2 | F-RB accepted; HF-G2 not asked | HF-G2 not in any activity; BA-05 keeps only the ranges that M1 confirms |
| 239 R-B | `TOL_SCALE` test approved as H4 **conditional on Q-O-1 and PRE-1..PRE-4** | H4 blocked (3.2) |
| 239 R-D | HF-C5 NOT_OBSERVABLE read-only; no corroborating read | HF-C5 appears only under H3a (blocked) |
| 239 R-F | TRUSTEDPATHS option (a): one recursive trusted root decided before H1 | 7.2 |
| 239 R-G, R-N | the complete declared set final and recorded before H1; I-7 outside the H1 set | 4.3, 3.1 (H5, H7) |
| 239 R-H | A1-R1..A1-R6 accepted, A1-R2 proposal (i); the typed commands and the protocol are inside "no instrument" | 7.5 |
| 239 R-I | interactive `acad.exe`; a one-line command-line status is allowed | 3.1 |
| 239 R-M | `scaleX/Y/Z` to `EXACT_DOUBLE` (R1(b)) is NOT ruled today | not an input of any activity of V2 (H4 blocked) |
| 239 condition 1 | each instrument is reviewed independently as read-only (semantic review of forbidden calls) and the Coordinator confirms it before declaring it part of the set | P5 and C-09 |
| 239 condition 2 | no load and no execution in AutoCAD; only tests that need no AutoCAD | 0 (state of the instruments) |
| 239 condition 4 | other files written by I-1 / the CAD manager; the private library copy procedure; the scope of I-8 as a ruling request | 7.4, 7.5.2, RQ-4 |

## 3. Activities against the exact host-fact matrix

The matrix is section 3.4 of the Architect design (blob dc2897e5...); V2 cites its rows by id (`HF-...`) and restates none of them. The classes are the
design's: **R0** (STRICT READ-ONLY: no API write call, no save, an in-memory side database filled by `ReadDwgFile` from a private copy, new result files only
through the single `EvidenceWriter`) and **RS** (writes side databases and saves new files under the evidence folder).

### 3.1 The activity list

State codes. `RUNNABLE_AFTER_PRE_RUN` = all its content is inside the gate and its instruments exist; it still needs section 5 and a membership line
(7.7). `BLOCKED_Q-O-1` = needs an RS instrument that must not be implemented before the Owner answers Q-O-1. `NEEDS_PRODUCT_BUILD` = needs the exact
build that does not exist. `LATER_SESSION_I-7` = needs the conformance reader I-7, which is not implemented and is not in the H1 declared set (239 R-N(b)),
so it runs in a later session under its own declared tuple.

| Id | Activity | HF rows | Instrument and command | Class | Membership | State | What is missing |
|---|---|---|---|---|---|---|---|
| **H1** | machine/session fact capture and the six-attribute observation; AutoCAD session in the designated profile; **no `NETLOAD`**, no RackCad, no loaded instrument | HF-M1..HF-M6, HF-M8..HF-M11 observed; HF-M7 (the `MC-` label) computed offline; HF-M4b open (RQ-5) | I-1 protocol and OS script (nothing loaded), I-0 label helper `tools label` (offline), I-9 seal `tools seal` (offline) | strict | **IN** (BA-11 3.3 row 1; Owner "machine/session fact capture") | `RUNNABLE_AFTER_PRE_RUN` | section 5 complete, above all the final declared set (it depends on Q-O-1, 4.3), the designation, the TRUSTEDPATHS decision, the Coordinator's authorization (section 6) |
| **H2** | R0 library entity census of the private copy of the library: block table records, entity classes, `DSTYLE` storage and order of the library's own dimensions; and, as a **separate command with its own record**, the scale components of the library's references | HF-C1, HF-C3 (read half of the AR4-10 confirmations); HF-T2 | I-2 R0 DLL: `CT21DHG_CENSUS_R`; `CT21DHG_INVENTORY` | R0 | **IN** (BA-11 3.3 row 3 and row 2 for HF-T2); the Q-O-0 gate on this set is released by 239 | `RUNNABLE_AFTER_PRE_RUN` after H1 | section 5; the label from H1 in the tuple; the private copy (7.4); the declared set |
| **H3b** | G6 read-only host-type facts: (i) the host type of each `RB` range as read from stored data of the private copy (M1); which run records HF-G1 is not decided (RQ-11): if it is the `CT21DHG_CENSUS_R` run of H2 no extra execution exists, if H3b(i) needs its own `CT21DHG_CENSUS_R` run on a private copy it is a second execution (7.4, C-16); (ii) the host type of the context-variable reads | (i) HF-G1; (ii) HF-G3 | (i) I-2 `CT21DHG_CENSUS_R`; (ii) I-4 R0 DLL: `CT21DHG_CTXVARS`, a non-Session command, no database | R0 | **IN** (G6 extension, Owner section 237 point 2; boundary test of design 3.5; HF-G3 was never gated by Q-O-0) | `RUNNABLE_AFTER_PRE_RUN` after H1 | as H2 (for H3b(i) a run of its own would need its own fresh private copy, RQ-11); for (ii) the active document is declared in the tuple (an unsaved drawing from a copy of the designated template, never a product file) |
| **H2-DYN** | dynamic-property census, read through a reference in a scratch side database | HF-C2 | I-3 RS (`CT21DHG_CENSUS_DYN`) | RS | **IN** as a fact (BA-05 V5:1180 requires it) | `BLOCKED_Q-O-1`; **I-3 is not implemented** | the Owner's answer to Q-O-1; implementation after it |
| **H3a** | write-back probe: equal-to-style behaviour and storage/order of the overrides the product writes; the group-code designation of the dimension variables | HF-C4, HF-C5 (HF-C5 is `NOT_OBSERVABLE` read-only; only this probe observes it) | I-5 RS (`CT21DHG_DIMWRITEBACK`) | RS | **IN** as a host activity (236 R-3); as an instrument it is not covered by the Owner's instrument gate | `BLOCKED_Q-O-1`; **I-5 is not implemented** | Q-O-1. If Q-O-1 = STRICT the designation stays unconfirmed (design 3.4, HF-C5) and BA-05 decides how to carry it |
| **H4** | `TOL_SCALE` non-governing characterization: 147 probe rows (135 in scope), OP1 and OP2, one execution; the inventory of the library references (HF-T2) is not part of it (it is H2) | HF-T1 | I-6 RS (`CT21DHG_TOLSCALE`), OP2 saves a DWG | RS | **IN, condicional** (239 R-B) | `BLOCKED_Q-O-1`; **I-6 is not implemented**; also gated by PRE-1..PRE-4 of the design (oracle and writer rules, SL-4 write route, fixture plan parameters, sealed pre-registration entry) none of which is met, and by the ruling on R1(b) (239 R-M) before the run | Q-O-1, PRE-1..PRE-4, the pre-registration entry. The design of the test exists (section 238, PASS WITH CONDITIONS in 239); V1's gap T-4 is closed on paper and nothing else |
| **H5** | `FX-ANN-BLANK`: step 1 on a build of `69daf03a` (R1: `RACKSELECTIVO`, new rack, `Insertar frontal`); step 2 on the exact build (R3: `RACKEDITAR`, `Insertar planta`); conformance record BA-08 V7 12.6 | HF-F1 | product commands by hand; I-7 reader; I-8 procedure | product | step 1: **IN** (236 R-4). **Step 2: IN, condicional** | step 1: `LATER_SESSION_I-7` (I-7 not implemented; the instance without its conformance record is not useful, so step 1 is not started before I-7 exists: RQ-3). **Step 2: `NEEDS_PRODUCT_BUILD`** (the PRODUCT_BUILD does not exist; `RACKMIRROR` is not in `src/`) | I-7 and its own declared tuple; I-8 (RQ-4); the construction build declared, hashed and pinned before its session (4.2); for step 2 the exact build |
| **H6** | instances of `FX-1F`, `FX-2F`, `FX-4F`, `FX-ANN`, `FX-DIM`, `FX-BIG`, `FX-SCRATCH` and the variants | HF-F3 | as H5 | product | **IN, condicional** (the words "fixture instances", but 236 R-4 makes them wait for the exact build); design HF-F3: OUT for now until the exact build | `NEEDS_PRODUCT_BUILD`. Not requested in this package | the exact build; a new membership line when it exists |
| **H7** | `FX-IMP` and its four variants (K-8): reachable with product operations or declared artificial | HF-F2 | product commands by hand; I-7 reader | product | **IN** (236 R-4) | `LATER_SESSION_I-7`; also needs the written construction specification (V1 gap T-6: a document by the Architect or the Coordinator; it does not exist) and a library copy that lacks at least two blocks (a new input with its own hash). Which build draws the source is not decided by any source (RQ-3) | I-7; T-6; the declared build |

Every row above is `NON_GOVERNING` and labelled as 7.6 says. **Nothing is requested for H2-DYN, H3a, H4, H5, H6 or H7 by this package**; their rows
record state so the Coordinator can see what each missing item blocks. **The only activities this package prepares to run** are H1, H2 and H3b, in
that order, each as a separate fresh AutoCAD process.

### 3.2 What Q-O-1 decides (one sentence for the Owner, restated from section 239)

If the Owner answers STRICT, the `TOL_SCALE` host test cannot run (`TOL_SCALE` stays `UNSET`), the dynamic-property census (H2-DYN) and the write-back probe
(H3a) cannot run, and BA-05 L-1 (dynamic part) and L-4 (write-back part) cannot close unless BA-05 is re-prepared without them. If the Owner answers
that side databases and new files are admitted, I-3, I-5 and I-6 are implemented after the answer, reviewed, and enter the declared set before H1.
**The answer therefore changes the composition of the declared set that must be final before H1 (4.3):** with STRICT the set is the R0 package (the R0 DLL and `I52Ct21d.HostFacts.Core.dll`, which the R0 DLL depends on), without any RS DLL; with the wider answer it contains the RS DLL too and H1 waits for it. V2 does not assume either.

### 3.3 The reference probe `eng/validation/I52G3AAutoCadProbe` (AR10-07, third minor)

`eng/validation/I52G3AAutoCadProbe` is a **reference probe only** (authority contract V2.1 line 116: CT-DA tooling and the G3A probe are "reference material
only"); it is **not a census, not part of any activity of this package and not read-only**: it calls `Overrule.AddOverrule` (`ProbeCommands.cs` lines 122
and 155) and writes a JSON file with `File.WriteAllText` (line 98). It is not loaded in the gate. The same holds for `eng/research/I52Ctda` (the CT-DA research harness, README blob e8cd6df4...).

### 3.4 Order and sessions

1. H1 first: the machine class is frozen by it. It needs the final declared set because `TRUSTEDPATHS` is class-defining (7.2) and no tuple record is closed before the
   declared set is final (239 R-G).
2. Then H2 and H3b, each in its own fresh `acad.exe` process, each with its own run id (`HGP-H2-...`, `HGP-H3B-...`), each under the SESSION_BUILD_TUPLE of the declared set.
   The Coordinator may reorder or drop an activity by its membership line.
3. Process hygiene for every attempt (V1 section 6, tension 4, by analogy with V2.1 sections 20 and 21.5): verify before launch that no `acad.exe` runs; one fresh
   process per attempt; record the process list at start and end; profile values read before and after; verify the absence of another `acad.exe` at the end.
4. Building any DLL needs AutoCAD closed and the AutoCAD 2025 managed assemblies installed for the compile references (`deploy/build-bundle.ps1` header, project note). Builds
   happen **before** the tuple is recorded and never between two activities of one campaign (a rebuilt DLL is a new tuple, 4.2).

## 4. Machine class, session/build tuple, declared set, build declaration

### 4.1 Two identities, recorded separately (Owner AR10_03B_READING; design 3.3)

| Record | Contents | Changes when | Digest |
|---|---|---|---|
| **MACHINE_CLASS** | the six exact strings `MachineGuid`, `OsVersionBuild`, `AutoCadProduct`, `AutoCadProfile`, `SECURELOAD`, `TRUSTEDPATHS` (HF-M1..HF-M6), the command and raw output of each, the label `MC-<12 hex>` (HF-M7). Reads fixed by the design as A1-R1..A1-R6, accepted by 239 R-H | one of the six changes (BA-10 V8 section 2.2) | the label |
| **SESSION_BUILD_TUPLE**, build part | AutoCAD version/build, `acad.exe` SHA-256, API assembly versions and SHA-256; the loaded-module/plugin baseline; every instrument DLL and the package manifest value supplied by the CAD manager (RQ-10); the RackCad build declared for the run (none in H1, H2, H3b); catalog folder and library file hashes | a module or plugin is added, removed or updated; a software update; a different build | `buildTupleDigest` (TB-B of the design) |
| **SESSION_BUILD_TUPLE**, session part | `pid`, process start, document and database identity, private-copy hashes before and after, run id and attempt, profile and variable reads before and after | every process | not a digest (TB-S) |
| **Facts of the CAD manager** (AR10-03(a)) | hardware identity/class, Windows account, configuration inputs | any change is a stop and a new tuple record | part of the tuple record, not class-defining |

A change in the second and third rows is a **new SESSION_BUILD_TUPLE** and changes the label **only if** it changes one of the six attributes. No evidence is
transferred silently between tuples; the Owner's record states that the designation is no evidence that other machines are equivalent.

### 4.2 The R8 condition applied (Owner section 237, point 3 and its precision)

- **Declared before the run.** The build (the instrument set, and, for a session that loads one, the RackCad build) is written into the tuple record and the
  authorization **before** the session starts.
- **Pinned.** The SHA-256 of each DLL, the package manifest SHA-256 and the `.pin` sidecars are fixed in the record; the session begins under that identity. The `.pin` sidecars are supplied by the CAD manager (the instrument only compares its own file with its sidecar; it never writes one), and the manifest is produced by a procedure that does not exist yet (I-8 / RQ-4 / RQ-10).
- **No substitution during the run.** If a file of the declared set differs from its pinned hash at any check (start, after each `NETLOAD`, end), the run is `INVALID` (8.1,
  stop condition 11). The CAD manager does not replace a DLL "to fix" an attempt: a different build is a new tuple, a new run id and, after an `INVALID`, the Coordinator's per-occurrence authorization.
- **A different build is a new tuple.** No automatic evidence transfer between builds, in either direction (for example the evidence of H1 is not transferred to a later
  session that declares I-7, and the evidence of that session is not transferred back).
- **Instruments.** Applying the R8 condition to the instruments is the extension of R8 that 239 R-N(a) states and declares as such; it is not the Owner's text. A `NETLOAD` of a declared module is an
  expected event of the declared set, not a plugin change. Installing, updating or removing a machine plugin, a profile change or any update is a stop (8.1).

### 4.3 The declared set (what must be final and recorded before H1)

| Element | Content | State |
|---|---|---|
| R0 package | `I52Ct21d.HostFacts.R0.dll` (I-2 and I-4 commands), `I52Ct21d.HostFacts.Core.dll` (part of the R0 package: the R0 DLL depends on it), their `.pin` sidecars (supplied by the CAD manager), and a package manifest, built **in Release** (236 R-11) from a committed SHA. The manifest and the `.pin` files are produced by a procedure that does not exist yet (I-8 / RQ-4 / RQ-10), not by the instrument folder; `packageManifestSha256` is an opaque 64-hex value supplied by the CAD manager, and what it means is RQ-10 | instruments implemented; **no package exists**: nothing was built for the gate, so every hash is UNKNOWN |
| RS package | I-3, I-5, I-6 | **does not exist**; its presence in the set depends on Q-O-1 (3.2) |
| offline tools | `tools label`, `tools seal`, `tools scan`, `tools imports-allowlist-check`, `tools imports-list`; they are never loaded into AutoCAD, but their identity (SHA-256) is recorded because the evidence is produced with them. The imports allowlist `allowed-apis.txt` is part of what the CAD manager must include in the declared set only if the Coordinator rules so (RQ-12); the scan prints its SHA-256 (P5, C-09) | UNKNOWN until built |
| RackCad build | **none** in H1, H2 and H3b | declared as "none" in the record |
| I-7, I-8 | I-7 is **not** in the set (239 R-N(b)); I-8 is a procedure | see RQ-3, RQ-4 |

`buildTupleDigest` is computed and recorded before H1 from the files above and the host build facts; **R0 compile references must equal the designated
machine's AutoCAD build** (design 5.1), so the package is built against that installation, after the CAD manager records the AutoCAD build and before the
record is signed. `Directory.Build.targets` stamps the git HEAD into the DLL, so its hash changes after any commit; the canonical pinned build belongs to package v2 (instrument folder README, "Pinning"). Nothing in the instrument folder pins `SourceRevisionId`.

## 5. PRE-RUN checklist (the Owner's order; nothing below is done)

State today for every row is **NOT DONE / NOT PROVIDED** unless the row says otherwise. Evidence is what the Coordinator will check (section 6 repeats each row as a checkbox).

| Id | Owner step | Requirement | Evidence the Coordinator needs | State |
|---|---|---|---|---|
| **P1** | 1. designation of the machine by the CAD manager | one physical, stable machine designated by the CAD manager; the record of 7.1, complete and signed | the designation record file, its SHA-256, its record id; the CAD manager's name and role; the Owner's delegation reference (Q-O5, "Delego al CAD manager la selección de esa máquina") | NOT DONE |
| P1b | (before H1, decision of 239 R-F) | the TRUSTEDPATHS decision of 7.2 made and applied **before** the six strings are read | the TRUSTEDPATHS decision record (7.2), its SHA-256 | NOT DONE |
| **P2** | 2. exact machine/session tuple record | MACHINE_CLASS section and SESSION_BUILD_TUPLE build part of 7.3 filled with the Owner's Q-O5 fields: machine identity, Windows version/build, AutoCAD version/build, AutoCAD profile identity, relevant hardware identity/class, user/profile identity, loaded-module/plugin baseline, configuration inputs; the freeze statement (no updates, no profile changes, no plugin changes, no machine-class changes). **The six strings and the label are the product of H1 itself** and are entered after it (RQ-1) | the tuple record file and its SHA-256; the module baseline hashed file by file | NOT DONE |
| **P3** | 3. exact build/package | the declared set of 4.3 built, hashed and pinned; the manifest SHA-256 recorded (the manifest and the `.pin` files are produced by a procedure that does not exist yet: I-8 / RQ-4 / RQ-10); "RackCad build = none" stated for H1, H2, H3b; rebuild comparison (build twice, same hashes: design 5.4) | the package manifest, its SHA-256, every DLL hash, the `.pin` files, the committed SHA, the build log hash, the rebuild comparison output | NOT DONE |
| **P4** | 4. `TOL_SCALE` design by the Architect where it applies | the design is delivered (section 238; PASS WITH CONDITIONS, section 239). It applies to H4 only; **H4 is not requested** by this package. PRE-1..PRE-4, the pre-registration entry, Q-O-1 and the ruling on R1(b) are the conditions of a later H4 line | decisions sections 238 and 239; for a later H4 line, the entries for PRE-1..PRE-4 | DESIGN DELIVERED; H4 NOT REQUESTED |
| **P5** | 5. read-only instruments implemented and reviewed | every instrument of the declared set is implemented, **independently reviewed as read-only by a semantic review of the forbidden calls** (239 condition 1), and the Coordinator confirmed each one before declaring it in the set; the tests that need no AutoCAD pass; the forbidden-API scan of the built DLLs reports clean, with the number of call sites examined; the imports allowlist `allowed-apis.txt` is approved by the independent reviewer and the Architect (RQ-6) | the independent review report, the Coordinator's confirmation entry, the scan output and its hash, **the SHA-256 of `allowed-apis.txt` that the scan prints (the value printed by the scan; none is stated here)**, the test run log hash and commit SHA | R0 CLASS: implemented; independent review under decisions 239 condition 1 NOT RECORDED in the repository (Coordinator confirmation open); the allowlist (layer 1) is implemented, drafted from the code and checked by the implementer only. RS: not implemented |
| **P6** | 6. Coordinator confirms each requested run belongs to the gate | one membership line per requested activity (7.7), against section 3 | the decisions entry with the lines | NOT GIVEN |
| P7 | declared set final (239 R-G, R-N) | Q-O-1 answered; the composition of the set fixed (3.2); recorded in the tuple record before H1 | the Owner's answer; the tuple record | NOT DONE |
| P8 | evidence location and tooling | evidence root and run folder created; seal tool (`tools seal`) present; attempt log started; the operator has the stop conditions (8.1) | the folder listing, the attempt-log file | NOT DONE |
| P9 | the Owner's objection window (239 "Decisiones del Owner que quedan" (c)) | status recorded of any objection to Q-O-0(a)/(b), R-H(a), R-N(a) | the decisions entry | NOT RECORDED |
| P10 | this package | the Coordinator's ruling names this package by its blob id | the decisions entry | NOT GIVEN |

## 6. FIRST_NON_GOVERNING_HOST_RUN authorization (template the Coordinator fills)

The Owner reserves this authorization to the Coordinator (decisions section 237, point 9 D). **This template is empty. Nothing in it is a ruling.** The
authorization covers **H1 only** (the first run). Every later activity needs its own line (7.7). Any condition unchecked, any evidence missing or any
change of a pinned item voids the authorization.

```text
CT21D_FIRST_NON_GOVERNING_HOST_RUN_AUTHORIZATION
  decisionsSection              = <number, EMPTY until the Coordinator writes it>
  date                          = <ISO 8601>
  coordinator                   = <name>
  packageReference              = docs/initiatives/I-52-ct21d-host-gate-execution-package-v2.md @ <git blob id of the file as ruled>
  designationRecord             = <record id> @ <SHA-256>
  tupleRecord                   = <record id> @ <SHA-256>        machineClassLabel = UNSET (H1 produces it)
  buildTupleDigest              = <sha256>          packageManifestSha256 = <sha256>
  authorizedActivity            = H1
  operator                      = <CAD manager>      (the agent does not operate the host)
  runId                         = HGP-H1-<yyyymmddThhmmssZ>-01
  GOVERNING                     = FALSE   RUN_STATUS_CLASS = NON_GOVERNING   GATE = HOST_GATE_BA11_3_3

  CONDITIONS (check each one only with the evidence named)
  [ ] C-01 designation record complete and signed (P1)                       evidence: file, SHA-256, signer, role
  [ ] C-02 TRUSTEDPATHS decision applied before any of the six reads (P1b)  evidence: decision record, SHA-256, the value set
  [ ] C-03 MACHINE_CLASS section filled except what H1 itself produces (P2) evidence: tuple record, SHA-256
  [ ] C-04 SESSION_BUILD_TUPLE build part complete: module baseline file by file, configuration inputs, hardware, Windows account (P2)
                                                                              evidence: tuple record; hashes listed
  [ ] C-05 freeze statement signed: no updates, no profile changes, no plugin changes, no machine-class changes; automatic updates state recorded
                                                                              evidence: signed statement
  [ ] C-06 build/package declared before the run, pinned, no substitution (R8 precision): manifest SHA-256, every DLL hash, committed SHA, Release, rebuild comparison (P3)
                                                                              evidence: manifest SHA-256 (value supplied by the CAD manager, RQ-10), .pin files, DLL hashes, build log hash
  [ ] C-07 "RackCad build = none" stated for this run
                                                                              evidence: the tuple record line `rackcadBuild = NONE` (7.3 section B) and its SHA-256
  [ ] C-08 TOL_SCALE: design delivered (section 238/239); H4 not part of this authorization (P4)
                                                                              evidence: decisions sections 238 and 239; this authorization naming H1 only
  [ ] C-09 every instrument of the declared set implemented, reviewed independently as read-only, confirmed by the Coordinator (239 condition 1); scan clean; tests green; allowlist approved (P5, RQ-6)
                                                                              evidence: independent review report, confirmation entry, scan and test hashes, the SHA-256 of allowed-apis.txt printed by the scan
  [ ] C-10 Q-O-1 answered and the declared set final and recorded (P7)        evidence: Owner answer, tuple record
  [ ] C-11 membership line for H1 written against package section 3 and the host-fact matrix rows HF-M1..HF-M11 (P6)  evidence: this entry
  [ ] C-12 evidence folder, seal tool and attempt log ready; labeling block (7.6) prepared (P8)
                                                                              evidence: the folder listing, the SHA-256 of the seal tool, the empty attempt-log file
  [ ] C-13 operator has read the stop conditions (8.1) and the no-retry rule (8.3); no other acad.exe; no pending Windows or AutoCAD update
                                                                              evidence: the attempt log (first line: operator, date, process list at start)
  [ ] C-14 the six reads and their transfer follow the I-1 protocol of 7.5; the label will be computed offline by `tools label` and entered after H1 (RQ-1)
                                                                              evidence: the SHA-256 of the protocol and script files as found in the declared set; the operator's signed statement
  [ ] C-15 package ruled by blob id (P10); state lines unchanged (below)
                                                                              evidence: the decisions entry naming the blob id; `git hash-object` of the package file as ruled
  [ ] C-16 [H2 line, and the H3b(i) line if RQ-11 gives it its own CENSUS_R run] private library copy procedure of 7.4 performed (a fresh copy per run); library path and SHA-256 recorded; copy hash equal
                                                                              evidence: the A0, B0, A1 hash commands and outputs pasted into the record
  [ ] C-17 status of the Owner's possible objection to Q-O-0(a)/(b), R-H(a), R-N(a) recorded (P9)
                                                                              evidence: the decisions entry recording the Owner's answer or the absence of an objection

  RESULT                        = FIRST_NON_GOVERNING_HOST_RUN: AUTHORIZED | NOT_AUTHORIZED     (Coordinator)
  SCOPE OF THE AUTHORIZATION    = H1 only, on the designated machine, under the recorded tuple. It authorizes no governing run, no admission, no G3 reopening,
                                  no CT-50, no Owner Act 2, no replacement Freeze, no acceptance of the W-scan residual or of the final reduced guarantee
  VOID WHEN                     = any item of C-01..C-17 changes; any pinned hash differs; any stop condition of 8.1 occurs
  AFTER                         = the result of H1 is `NON_GOVERNING` evidence; it does not change CT21D_AUTHORITY_BASELINE_READY, CT21D_EXECUTION_READY or CT21D_EXECUTION
```

## 7. CAD-manager procedures

The CAD manager is the Owner's delegate for the machine (Q-O5). Every field in the templates is the CAD manager's; the implementer invents none.

### 7.1 Designation record (template)

```text
CT21D_HOST_GATE_DESIGNATION_RECORD
  recordId                      = <CAD manager assigns>
  designationDateUtc            = <ISO 8601>
  designatedBy                  = <name and role of the CAD manager>
  ownerDelegationReference      = Owner message of 2026-09-30, Q-O5 ("Delego al CAD manager la selección de esa máquina"); decisions section 237 point 1
  gateReference                 = HOST_GATE_BA11_3_3 = AUTHORIZED, NON_GOVERNING ONLY (decisions section 233); G6 = EXTEND (section 237)
  packageReference              = docs/initiatives/I-52-ct21d-host-gate-execution-package-v2.md @ <git blob id of the file as ruled>
  coordinatorGateRuling         = <decisions section number; EMPTY until given>

  -- the Owner's Q-O5 list, in the Owner's order --
  machineIdentity               = <hostname, asset id, statement "ONE concrete stable physical machine">
  windowsVersionBuild           = <exact text read, with the command used>
  autocadVersionBuild           = <product, version, build; acad.exe SHA-256; the command or file read>
  autocadProfileIdentity        = <exact text read, with the read used>
  relevantHardwareIdentityClass = <CPU model, RAM, GPU model/driver, storage class, as the CAD manager judges relevant, with the reason>
  userProfileIdentity           = <Windows account that runs AutoCAD; AutoCAD profile; recorded as a MACHINE_PROFILE_BOUND fact, not class-defining (AR10-03(a))>
  loadedModulePluginBaseline    = <static inventory of every plugin, bundle and autoload entry of the designated profile with SHA-256 per file, read from
                                   the file system and the autoload registry keys WITHOUT starting AutoCAD (RQ-1); the H1 SESSION_START module list confirms it>
  configurationInputsOfTheTuple = <SECURELOAD intended value, TRUSTEDPATHS value per 7.2, catalog folder path and the SHA-256 of its CSV/JSON files,
                                   library file path and SHA-256, designated template copy path and SHA-256>

  -- freeze statement (Q-O5 "Durante la campaña") --
  noUpdates / noProfileChanges / noPluginChanges / noMachineClassChanges       = ACKNOWLEDGED BY <CAD manager>
  windowsUpdateAutoInstall      = <state of the policy and who disabled it>
  autocadUpdateNotifications    = <state>
  materialChangeRule            = any material change => NEW MACHINE/SESSION TUPLE, no silent transfer of evidence
  noEquivalenceClaim            = THIS DESIGNATION IS NO EVIDENCE THAT OTHER MACHINES ARE EQUIVALENT
```

### 7.2 The TRUSTEDPATHS decision (before H1; option (a) of design R-F, ruled by 239)

`TRUSTEDPATHS` and `SECURELOAD` are two of the six class-defining attributes (BA-10 V8 section 2.2). Any change after H1 reads them gives a new MACHINE_CLASS label.
The CAD manager therefore decides, applies and records the following **before** the six strings are read, and again for every future RackCad build that will be placed under the root:

```text
CT21D_TRUSTEDPATHS_DECISION
  recordId / date / decidedBy                = <...>
  option                                     = (a) one recursive trusted root (239 R-F); alternatives (b) per-package entries and (c) another mechanism are not used
  trustedRootPath                            = <absolute path chosen by the CAD manager>
  contentsOfTheRoot                          = <instrument package folder(s) and, later, each pinned RackCad build package folder; nothing else; list with hashes>
  accessControl                              = <who can write there; the CAD manager recommended to restrict writes to himself or herself: a recommendation of the implementer, not a ruling>
  trustedPathsValueApplied                   = <the exact string set in the designated profile, with the AutoCAD syntax for a recursive entry [HOST-TO-CONFIRM]>
  secureLoadValue                            = <value kept; lowering SECURELOAD is NOT proposed>
  hashPinningCompensation                    = hashes recorded in the tuple record and checked at load (4.2); the recursive root is the weaker posture
  appliedAtUtc / appliedBy                   = <... before any of the six reads>
  consequence                                = adding or changing a trusted entry after the six reads => NEW MACHINE_CLASS label
```

### 7.3 Tuple record (templates)

The tuple record is one file (or one signed set of files) with two sections, as 4.1 requires. It is **opened by the CAD manager before H1** and **closed only
when the declared set is final** (239 R-G); the fields marked H1 are filled from H1's own raw output.

```text
CT21D_MACHINE_SESSION_TUPLE_RECORD
  recordId / designationRecordId / date

  -- SECTION A: MACHINE_CLASS (separate record, same file) --
  MachineGuid          text=<exact> command=<...> rawFile=raw-MachineGuid.txt rawSha256=<...>              (H1)
  OsVersionBuild       text=<exact> command=<...> rawFile=raw-OsVersionBuild.txt rawSha256=<...>           (H1)   A1-R2 proposal (i), one read
  AutoCadProduct       text=<exact> command=<...> crossCheck=<...> rawFile=raw-AutoCadProduct.txt           (H1)
  AutoCadProfile       text=<exact> command=<...> rawFile=raw-AutoCadProfile.txt                            (H1)
  SECURELOAD           text=<decimal digits> rawFile=raw-SECURELOAD.txt                                    (H1)
  TRUSTEDPATHS         text=<exact, may be empty> rawFile=raw-TRUSTEDPATHS.txt                             (H1)
  label                = MC-<12 hex> | UNSET     computed offline by `tools label`; entered after H1 (RQ-1)

  -- SECTION B: SESSION_BUILD_TUPLE, build part (before H1) --
  autocad              = version/build <...>, acad.exe SHA-256 <...>, AcDbMgd/AcMgd/AcCoreMgd versions and SHA-256 <...>
  loadedModuleBaseline = <the static inventory of 7.1 and, after H1, the SESSION_START module list; both hashed>
  declaredSet          = R0 package (DLLs, .pin files supplied by the CAD manager, manifest SHA-256 value), RS package (if Q-O-1 admits it), offline tools, with every SHA-256 (4.3)
  rackcadBuild         = NONE (H1, H2, H3b)
  configuration        = catalog folder hashes, library path and SHA-256, designated template copy SHA-256
  hardware / windowsAccount = <as recorded in the designation>
  buildTupleDigest     = <sha256 of the canonical serialization of the above>

  -- SECTION C: SESSION_BUILD_TUPLE, session part (one per AutoCAD process) --
  runId / attempt / pid / processStartUtc / documentIdentity / databaseIdentity
  privateCopy          = path, SHA-256 before and after the activity; library SHA-256 before and after
  profileReads         = SECURELOAD, TRUSTEDPATHS, CPROFILE before and after
  processList          = at start and at end; other acad.exe = none
  nonClassFacts        = HF-M4b (profile digest) only if RQ-5 is ruled in
```

### 7.4 The private library copy (resolves the minor of the final review of the design)

**Why.** BA-05 V5 section 5 and design 5.1: the census reads a private copy and never the library file; `run-designation.json` carries `libraryPath`,
`libraryFileSha256`, `privateCopyPath` and `privateCopySha256`, and the runner checks them before and after the run.

| Aspect | Procedure |
|---|---|
| **who** | the CAD manager (or the Owner), by hand, on the designated machine. Not an instrument, not the agent |
| **which file** | the exact library file of the baseline: `blocks-library.dwg` (name `BlockLibraryLocator.FileName`, `src/RackCad.Application/Catalogs/BlockLibrary.cs`, blob ce804214...) at the path the product resolves: the user override or `<catalog folder>/blocks-library.dwg`. The path and SHA-256 are tuple fields. The repository holds no DWG |
| **where** | outside the evidence folder, because the seal requires that the evidence folder holds only declared outputs: `%LOCALAPPDATA%\RackCad\ct21d-evidence\hostgate-prep\private\<runId>\blocks-library.dwg`, a sibling of the run folder of 7.6 (a proposal in line with 236 R-6; RQ-2). The folder is created by the CAD manager |
| **when** | with no `acad.exe` running and no RackCad session; **a fresh copy per run id and attempt**, never reused for another attempt or another activity. If RQ-11 gives H3b(i) a `CT21DHG_CENSUS_R` run of its own, that run is a second execution with its own run id and its own fresh copy |
| **by which operation** | one plain file copy typed in 64-bit PowerShell: `Copy-Item -LiteralPath <library> -Destination <private copy>`; no AutoCAD, no script of the repository, no editing, no attribute change |
| **hashing** | `Get-FileHash -Algorithm SHA256` of the library **before** the copy (A0), of the copy **after** it (B0), of the library **after** the copy (A1); A0, B0, A1 must be equal; each command and its output are pasted into the record. The I-1 script (`-Mode Session -LibraryPath ... -PrivateCopyPath ...`) writes `hash-library-<phase>.txt` and `hash-private-copy-<phase>.txt` at `SESSION_START` and `END`; the library and copy must have the same hash at both phases |
| **declaration** | A0 is the tuple's `libraryFileSha256`; B0 is `privateCopySha256`; both go in `run-designation.json` before `NETLOAD` |
| **stop** | any hash difference (source changed, copy differs, copy changed during the run) is a stop condition (8.1, items 3 and 12); nothing is repaired; a library whose hash differs from the one in the tuple is a configuration-input change: a new tuple |
| **catalog folder** | not copied for H2 and H3b; its file hashes are recorded as configuration inputs (7.3 section B) |
| **ruling needed** | the copy creates a new file outside the evidence folder; it is an operator file operation that touches no product, catalog, library or open-document file. The Coordinator confirms that it lies inside the rationale of Q-O-0(b) (RQ-2) |

### 7.5 The I-1 observation protocol

The protocol text is `eng/research/I52Ct21dHostFacts/protocol/I-1-observation-protocol.md` and the OS-level script is
`protocol/i1-os-observation.ps1`; both are text for the CAD manager, not loaded and not run by anyone. V2 does not copy them; it fixes how they are used.

1. **The six strings** (A1-R1..A1-R6, accepted by 239 R-H): `MachineGuid` and `OsVersionBuild` by the script in mode `MachineStrings` (64-bit view of the registry; one read of
   `Win32_OperatingSystem.Version`, no join, no `UBR`); `AutoCadProduct`, `AutoCadProfile`, `SECURELOAD`, `TRUSTEDPATHS` by one typed AutoLISP expression each, which writes
   the file itself with an explicit encoding and a create-new guard. The raw file is one string in UTF-8 without BOM plus one line terminator; nothing is trimmed, case-folded
   or normalized. Copying from the command-line window or the clipboard is **not** a transfer mechanism. The `declared-length-<Key>.txt` file records `strlen` for the offline check. The
   typed AutoLISP items (`open` encoding argument, `write-line` terminator, `strlen`, `vlax-product-key`) are `[HOST-TO-CONFIRM]`; a mismatch makes the attribute `UNKNOWN`.
2. **Files written by I-1 and by the CAD manager** (239 condition 4; the Coordinator ruled the typed writes and the script inside "no instrument", R-H(b)). Every file is a **new** file; none replaces another. As the protocol and the script stand when this package was written:

   | Producer | Files |
   |---|---|
   | script, `MachineStrings` | `raw-MachineGuid.txt`, `raw-OsVersionBuild.txt`, `supplementary-os.txt` (UBR, DisplayVersion, ProductName) |
   | typed AutoLISP | `raw-AutoCadProduct.txt`, `raw-AutoCadProfile.txt`, `raw-SECURELOAD.txt`, `raw-TRUSTEDPATHS.txt` and their `declared-length-<Key>.txt` |
   | script, `Session`, per phase `SESSION_START` / `AFTER_NETLOAD` / `END` | `hash-acad-exe-<phase>.txt`, `hash-AcDbMgd-<phase>.txt`, `hash-AcMgd-<phase>.txt`, `hash-AcCoreMgd-<phase>.txt`, `version-acad-<phase>.txt`, `hash-library-<phase>.txt`, `hash-private-copy-<phase>.txt`, `modules-<phase>.txt`, `other-acad-<phase>.txt` |
   | `tools label --record` | `machine-label.json` (the clear strings are not copied into it) |
   | `tools seal` | `HASHES.sha256` and its digest |
   | CAD manager, by hand | the designation record, the TRUSTEDPATHS decision, the tuple record, the attempt log, the command transcripts pasted into the record, the hardware/account notes, the private copy of 7.4, `run-designation.json` and the `.pin` sidecars placed in the trusted folder (inputs, hashed; the instrument never writes them) |
   | instruments (H2, H3b) | only through the single `EvidenceWriter`: create-new, flat names, write-once; the census, inventory and context-variable records and the instrument log |

   The file names above are those of the protocol and script at the time of writing; the Coordinator re-checks them against the built, pinned package (P3, P5).
3. **Order inside H1.** (i) Verify no `acad.exe`, hash the files of 7.3 section B; (ii) start AutoCAD in the designated profile; (iii) `SESSION_START` module list and hashes (before any
   `NETLOAD`; H1 does no `NETLOAD`); (iv) the six reads; (v) the `SECURELOAD`, `TRUSTEDPATHS` and profile reads again before ending (stop condition 2); (vi) `END` phase; (vii) end the process, record
   the process list; (viii) `tools label` offline; (ix) `tools seal`. Take the six strings **after** the profile is in its final state and after 7.2.
4. **A missing attribute** is `UNKNOWN`, the label stays `UNSET`, no guess; no tuple record is closed.

### 7.6 Evidence preservation and labeling (V1 3.5 and 3.6, as ruled by 236 R-6)

- Raw means what the instrument or the operator produced, unedited. Derived tables are separate files that cite the raw file's hash, prefix `NG-`. A correction is a new file that cites the old one.
- Location (ruled as proposed): `%LOCALAPPDATA%\RackCad\ct21d-evidence\hostgate-prep\<runId>\`; machine-profile content stays local; only hashes and non-machine facts enter the repository (the library census record has no machine data: 236 R-13, the Coordinator may propose it).
- Name: `<runId>` = `HGP-<H-id>-<yyyymmddThhmmssZ>-<nn>`, `nn` = the attempt number. `HASHES.sha256` (format `<sha256>  <relative/path>`) and its own digest are written at the end by `tools seal`; the folder is then read-only; a later mismatch is an `INVALID` fact (V2.1 21.1: evidence not sealed).
- Labeling block, on every record, raw folder and derived file, written before the first action:

```text
GOVERNING = FALSE
RUN_STATUS_CLASS = NON_GOVERNING (V2.1 21.1: "a valid execution that by design does not count toward acceptance")
GATE = HOST_GATE_BA11_3_3 (non-governing host characterization / baseline-preparation evidence; decisions sections 233, 237)
NOT_EVIDENCE_FOR = any control result, any group verdict, ALT21D_HOST_PASS, admission, G3, CT-50, Owner Act 2
MACHINE_CLASS = <label or UNSET>     SESSION_BUILD_TUPLE = <buildTupleDigest>     PACKAGE = <blob id of the ruled package>
```

### 7.7 Membership line per activity (the Coordinator writes one per requested activity)

```text
ACTIVITY | package section 3 row | membership (Coordinator) | ruling number | conditions
H1       | H1                    | <IN>                      | <section>      | C-01..C-17 of section 6
H2       | H2                    | <IN / OUT>                |                | label in the tuple; private copy of 7.4 (C-16)
H3b      | H3b                   | <IN / OUT>                |                | active document declared in the tuple
(H2-DYN, H3a, H4: OUT until Q-O-1 and, for H4, PRE-1..PRE-4; H5, H6, H7: not requested)
```

Only an activity with a recorded IN runs. An activity with no line does not run.

## 8. Run discipline

### 8.1 Stop conditions (all activities)

The operator stops, writes the attempt-log line and informs the Coordinator when any of these happens. No step is "fixed on the fly".

1. Tuple drift: a MACHINE_CLASS or machine-layer field differs from the record.
2. `SECURELOAD`, `TRUSTEDPATHS` or the profile differ between the before-read and the after-read; or any of the six differs between two reads in one session.
3. The source library or any template differs from its before-hash.
4. AutoCAD crashes, hangs, shows an unscripted dialog, or another `acad.exe` appears.
5. An update prompt or installer of Windows, AutoCAD or a plugin starts; any update, profile change, plugin change or machine-class change during the campaign (Q-O5).
6. An instrument reports an error or an unexpected class/property.
7. The operator deviates from the written steps.
8. The evidence folder cannot be written or hashed.
9. A `NETLOAD` prompt, refusal or manual answer (the load outcome is recorded: loaded / prompt / refused). A prompt answered by hand is manual input and makes the activity that needed the load `INVALID`.
10. A module outside the declared set is loaded, except a module the host itself loads that lies under the AutoCAD installation directory and is signed by the vendor `[HOST-TO-CONFIRM]`, which is listed as host-loaded and is not `INVALID` (design HF-M9).
11. A file of the declared set differs from its pinned hash, or the self-pin reports `PIN_FILE_MISSING` or `PIN_MISMATCH` (8.2).
12. `DBMOD` of the active document differs before and after, or the private-file hashes differ before and after.
13. Anything is written outside the evidence folder (other than the private copy and inputs of 7.5.2).

### 8.2 Classification (existing contract; no new category)

| Case | Treatment (V2.1 21.1, 21.1.1, 21.3; Owner: "Any FAIL / UNKNOWN / INVALID remains governed by the existing contract. No automatic retries.") |
|---|---|
| a valid execution that reached its end and produced its raw outputs | `NON_GOVERNING` (valid). Fact status `OBSERVED`, or `OBSERVED_DIFFERS` when a read differs from what a baseline artifact expected (that changes the artifact under its own version rule, BA-05 V5 section 2.6; it is not a FAIL of anything) |
| a fact the activity could not read | `UNKNOWN` for that fact, with the raw output; no substitute value |
| a datum that no read-only source can show | `NOT_OBSERVABLE` (for example HF-C5 without the write probe); not `UNKNOWN`, which is a failed read |
| no occurrence in the data read (for example an `RB` range that the library never stores) | `NOT_OBSERVED`; never filled by a guess; the range is not confirmed (fallback F-RB) |
| tuple mismatch, missing or unreadable log, instrument error, crash, another `acad.exe` or interfering process, unsealed evidence, incomplete cleanup, manual input where the activity is scripted, a failed host-run check (hashes, `DBMOD`, self-pin, declared outputs only) | `INVALID` (V2.1 21.1 list, applied by analogy; 236 R-12 confirms the mapping). A failed host-run check makes **every** fact of that run `INVALID`; all facts and logs stay retained (21.1.1) |
| a crash | recorded with an explicit status; needs a Coordinator ruling (`CRASH_FINDING` or `CRASH_ENVIRONMENT`); never silently invalid; no automatic retry |
| **INVALID o caída** (AR10-07: the V1 row said "any of the above") | **no automatic retry.** A rerun needs the **Coordinator's per-occurrence authorization**, a new run id and a recorded cause (tooling, environment or contamination); the rerun is the same activity, same package, same tuple requirements. Changing the activity, the package or an instrument is a new version, not a retry |
| a valid `UNKNOWN` or `OBSERVED_DIFFERS` | recorded; **never repeated to obtain a better result** |

### 8.3 No automatic retry; what the numbers mean

- **No attempt after an `INVALID` or a crash begins without the Coordinator's written authorization for that occurrence.** The `nn` of the run id counts attempts of an activity; it is a log number, not a budget.
- **`PARAM-04` = 3 is `MAX_ATTEMPTS_PER_SLOT`**: a capacity ceiling per slot (the Owner's word: "ranura / slot"; "planned governing" is the implementer's gloss), accepted by the Owner with the words "NO significa “tres retries automáticos”". These activities are not governing slots and have none; the
  value is **not a permission to retry** an activity up to three times, in this gate or in any other. The existing retry rules govern.
- Every attempt is listed in the attempt log (V2.1 21.3): attempt number, tuple, contract version, state (`NON_GOVERNING` or `INVALID`), outcome, the reason for the retry or for stopping. Invalid attempts are disclosed.

### 8.4 Repetition

`EVIDENCE_REPETITION` = 1 (Owner, accepted with its condition): **one evidence package per execution**, and one execution per activity (236 R-7). It does **not** replace or reduce
`N_A` = 44, `N_B` = 29, `N_det` = 90 or `N_clean` = 59, nor any other governing statistical repetition; those counts are **not applied** to these activities and nothing here reduces them. If a later
authorized activity needs several executions, each is a distinct execution with its own evidence. (H4, which has a design with its own count, is blocked.)

## 9. Instruments and tooling: state of V1 gaps T-1..T-9

State is what the files show at the time of writing; no independent review of the instruments is recorded in the repository (section 0).

| V1 gap | Now | Instrument (design 5.2) | State |
|---|---|---|---|
| T-1 six-attribute observation | I-1 protocol, OS script, label helper | I-0, I-1 | implemented as text and offline tools; nothing loaded; no host use |
| T-2 library census | I-2 `CT21DHG_CENSUS_R`, `CT21DHG_INVENTORY`; I-3 for the dynamic part | I-2 (R0), I-3 (RS) | I-2 implemented; **I-3 not implemented (Q-O-1)** |
| T-3 type readers and write-back | I-4 `CT21DHG_CTXVARS` (R0); I-5 (RS) | I-4, I-5 | I-4 implemented; **I-5 not implemented (Q-O-1)** |
| T-4 `TOL_SCALE` test design | section 238 design; I-6 | I-6 (RS) | design delivered; **I-6 not implemented (Q-O-1)** |
| T-5 fixture conformance reader | I-7 | I-7 | **not implemented**; assembly identity unsettled; outside the H1 declared set |
| T-6 `FX-IMP` construction specification | a document by the Architect or the Coordinator | none | **does not exist** |
| T-7 build and packaging | I-8 procedure | I-8 | **not written**; no manifest or pin generator exists in the instrument folder (the README says the earlier generator was removed); scope is RQ-4 |
| T-8 precondition verifier | `docs/automation/evidence/I-52-ct21d-precondition-verifier.py` | none | exists; not a host activity |
| T-9 evidence folder tool | I-9 `tools seal` | I-9 | implemented |

The instrument folder holds `core/`, `r0/`, `tools/`, `tests/`, `fixtures/`, `schemas/`, `protocol/` and `allowed-apis.txt`; its `bin/` and `obj/` outputs are ignored by the repository `.gitignore` (lines `bin/` and `obj/`). The imports-allowlist backstop
(design 5.3 item 2, layer 1: deny-by-default `allowed-apis.txt`) **is implemented**; it was drafted from the code (`tools imports-list`) and checked by the implementer only. **V2 approves no list**: approval of the list by the independent reviewer and the Architect is RQ-6. The scan prints the SHA-256 of the list it used, and that hash must be recorded in P5 and C-09.

## 10. What stays NOT authorized

- A governing CT-21D campaign; `ADMISSION_EVALUATION`; product admission; G3 reopening; CT-50; Owner Act 2; a replacement Freeze; acceptance of the W-scan residual; acceptance of the final reduced guarantee (Owner's `NO autoriza` list).
- Any host run before the Coordinator's FIRST_NON_GOVERNING_HOST_RUN authorization; any activity without a recorded IN; any activity outside H1, H2, H3b in this package.
- The RS class: I-3, I-5, I-6, the HF-C4 write probe and the OP2 save, until the Owner answers Q-O-1. HF-G2 (the round trip of `RB` ranges) is not asked (Q-O-2).
- The `TOL_SCALE` host test H4 (also blocked by PRE-1..PRE-4 and the R1(b) ruling); choosing any `TOL_SCALE` value (it stays `UNSET`).
- H5, H6, H7 (fixtures) in this package; the PRODUCT_BUILD, RACKMIRROR implementation, product seams; I-7 and I-8.
- `HDM-CHAR-*`, `E4-LEARN-R1-*`, `LK-*`, `WU-DRY-*`, BA-06 dynamic-trace dry runs, qualification of instruments, `EV-L-*` and `EV-V-*`, manifest/custody/anchor capture, UNDO evidence (V1 section 2 rows G11 to G19 stand).
- Loading any instrument into AutoCAD before the authorization; using `eng/validation/I52G3AAutoCadProbe` or `eng/research/I52Ctda` in the gate.
- Any change under `src/`, `tests/`, any existing `eng/` folder, any solution or CI file by this package.
- Any product, catalog, library, template or open-document write; the agent operating the host.
- Treating any result as governing because it ran in AutoCAD; treating the designation as evidence that other machines are equivalent; transferring evidence between tuples.

## 11. Requests to the Coordinator (the implementer proposes; decides none)

| Id | Question | Proposal |
|---|---|---|
| RQ-1 | The Owner asks for a complete tuple record before the first run, but the six strings, the label and the observed module list at `SESSION_START` are produced by H1 itself. May the pre-run record be complete except those fields (opened before H1, closed after it), with the module baseline taken statically from the file system and confirmed by `SESSION_START`? | yes, as written in 7.1 and 7.3; the label is entered after H1 and before any later activity |
| RQ-2 | The private library copy (7.4): confirm its location outside the evidence folder, the operation (a plain copy by the CAD manager) and that it lies inside the rationale of Q-O-0(b) | confirm |
| RQ-3 | Where and with which build do H5 step 1, H7 and the conformance reads run? I-7 is outside the H1 set, so these run in a later session under its own declared tuple | no H5/H7 construction before I-7 exists; the construction build (a clean build of `69daf03a`) and I-7 declared and pinned in that later tuple; the build for H7 to be named by the Coordinator |
| RQ-4 | Scope of I-8 (the build and pin procedure): who builds, where, which builds (the instrument package; the `FIXTURE_CONSTRUCTION_BUILD`; the exact build, which does not exist), Release, `SourceRevisionId` pinned, deterministic rebuild check, building against the designated machine's AutoCAD build | a procedure written after the ruling; V2 provides none, and no manifest or pin generator exists in the instrument folder (the README says it was removed) |
| RQ-5 | HF-M4b (digest of the exported profile registry subtree before and after each session): is it inside the Owner's "configuration inputs" (design 3.4: NEEDS-COORDINATOR)? | rule it in or out; the protocol does not prescribe it |
| RQ-6 | The imports-allowlist backstop (design 5.3 item 2, layer 1) is implemented: `allowed-apis.txt` was drafted from the code and checked by the implementer only. Who approves it? | approval by the independent reviewer and the Architect; the SHA-256 printed by the scan recorded in P5 and C-09; V2 approves nothing |
| RQ-7 | Confirmation only: H1 is not authorized while the RS DLL is declared but not implemented; the set is final only after Q-O-1 (239 R-G already requires the set final before H1) | confirm, as 3.2 and the design say |
| RQ-8 | Does the Coordinator accept the `run-designation.json` format `ct21d.designation.v1` (including `declaredSet`, `declaredSetSha256` and `tupleBinding.packageManifestSha256`) and the `.pin` sidecar format? Both are the implementer's own design, not specified by the Architect design | accept, or name changes before the package is built |
| RQ-9 | I-9 (`tools seal`) is written in C# through the single `EvidenceWriter`, not as a PowerShell script. The Coordinator must accept this deviation | accept or rule otherwise |
| RQ-10 | Who builds, hashes and writes the package manifest and the `.pin` files, and what does `packageManifestSha256` mean (an opaque 64-hex value supplied by the CAD manager; nothing in the instrument folder generates or checks it)? May fold into RQ-4 | rule it together with RQ-4 |
| RQ-11 | Which run records HF-G1 (H3b(i))? The `CT21DHG_CENSUS_R` run of H2, or a run of its own on a fresh private copy (a second execution of the same command, C-16, 8.4)? | the H2 run records it, unless the Coordinator rules otherwise |
| RQ-12 | Is `allowed-apis.txt` (and the offline tools) part of the declared set that the CAD manager hashes into the tuple record? | the Coordinator rules; the package declares only the scan's printed hash as required evidence |

## 12. Status

```text
HOST_GATE_PACKAGE                = V2_PREPARED_FOR_COORDINATOR_REVIEW
HOST_GATE_BA11_3_3               = AUTHORIZED BY THE OWNER (NON_GOVERNING ONLY; G6 EXTENDED); NO MACHINE DESIGNATED; NO TUPLE RECORD; NO COORDINATOR AUTHORIZATION OF A FIRST RUN
FIRST_NON_GOVERNING_HOST_RUN     = NOT_AUTHORIZED (section 6 template empty)
HOST_RUN_STARTED                 = FALSE
INSTRUMENT_IMPLEMENTATION_GATE   = OPEN (R0 CLASS ONLY: I-0, I-1, I-2, I-4, I-9); RS CLASS (I-3, I-5, I-6, HF-C4 write probe, OP2 save) BLOCKED BY Q-O-1; I-7, I-8 NOT IMPLEMENTED
INSTRUMENTS                      = R0 IMPLEMENTED; INDEPENDENT REVIEW (239 CONDITION 1) NOT RECORDED IN THE REPOSITORY, COORDINATOR CONFIRMATION OPEN; NONE LOADED; NONE RUN ON ANY HOST
ACTIVITIES PREPARED TO RUN       = H1, H2, H3b (each after section 5 and its membership line)
ACTIVITIES BLOCKED_Q-O-1         = H2-DYN (HF-C2), H3a (HF-C4, HF-C5), H4 (HF-T1; also PRE-1..PRE-4)
ACTIVITIES IN, CONDICIONAL       = H4, H5 step 2, H6
ACTIVITIES LATER_SESSION_I-7     = H5 step 1, H7 (H7 also needs the construction specification)
ACTIVITIES NEEDING PRODUCT_BUILD = H5 step 2, H6
TOL_SCALE VALUE                  = UNSET
OWNER_QUESTIONS_PENDING          = Q-O-1 (scope of "solo lectura"); Q-O-2 only if the M1 coverage is too small; machine designation (CAD manager)
CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
CIA = UNKNOWN     SafeOperationalState = FALSE_FOR_ADMISSION     SCOPE = NONE     G3 = STOPPED     FALLBACK = ALT-21E     OWNER_ACT2 = RESERVED
HOST_GATE_PACKAGE = V2_PREPARED_FOR_COORDINATOR_REVIEW
```
