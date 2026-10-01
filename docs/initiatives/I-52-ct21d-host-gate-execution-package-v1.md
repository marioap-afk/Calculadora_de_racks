# I-52 — CT-21D Host-Gate Execution Package V1 (non-governing host characterization / baseline-preparation evidence)

> ```text
> DOCUMENT            = host-gate execution package, prepared for the Coordinator's review (decisions section 233, "NEXT WORK" item 5;
>                       this report is decisions section 234). Not a baseline artifact; not in the BA-11 registry; not a contract
> STATUS              = PREPARED_FOR_COORDINATOR_REVIEW. NO HOST RUN HAS STARTED. NO HOST RUN MAY START BEFORE THE COORDINATOR'S GATE
>                       RULING ON THIS PACKAGE (section 7)
> AUTHORITY_CONTRACT  = CT-21D-V2.6 AGREED (section 230; composite 97100cff11868807f3c49f25bf25d0385c86e8873cba0cf2c1996338b9b98eb4)
> BOUND_PRODUCT_SHA   = 95690c28dc6268e61dff32a0cbc33cc9fde3d47f (non-tuple bound item)
> OWNER AUTHORITY     = docs/automation/evidence/I-52-ct21d-owner-decisions-q-o1-q-o5-and-host-gate.md (verbatim; section 233):
>                       HOST_GATE_BA11_3_3 = AUTHORIZED, NON_GOVERNING ONLY
> SOURCES (git blob ids of the working tree, `git hash-object`; every fact below was read in these files)
>                       docs/automation/evidence/I-52-ct21d-owner-decisions-q-o1-q-o5-and-host-gate.md  cdb1a13682c63571cf4a4b4bb03bbf0fb0dc19ec
>                       docs/initiatives/I-52-ct21d-baseline-ba-11-hash-registry-v9.md                  6d6212a4bbddc8cb7c2ef3cbde7a56a16822f515
>                       docs/initiatives/I-52-ct21d-baseline-ba-05-fingerprint-specification-v5.md      b74af94ece4f0901a071ef5176f3c58bff01cfdc
>                       docs/initiatives/I-52-ct21d-baseline-ba-08-selective-kind-baseline-v7.md        57330c3a09d80e919a63a2b83a45d168da326140
>                       docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v7.md               07e18f5f5ac2d5ee4c4c613e5762f2f57d47aef4
>                       docs/initiatives/I-52-ct21d-baseline-ba-04-scenario-catalog-v7.json             03c55e11570ce622e91bf5d48bba075891446e96
>                       docs/initiatives/I-52-ct21d-baseline-ba-06-evm-preparation-v7.md                74fa270ccba8dff89ccbf3ac71a125bb8c142b44
>                       docs/initiatives/I-52-ct21d-baseline-ba-07-manifest-custody-v7.md               89a325820ad7975ef57c2e19d576d17d7cfdda5e
>                       docs/initiatives/I-52-ct21d-baseline-ba-09-warmup-v7.md                         6ae47569a19510c83cda7e87c2c9cec1e85741d5
>                       docs/initiatives/I-52-ct21d-baseline-ba-10-parameter-sheet-v7.md                92f2b3eef3fb1d892b3d7e5107f5df0a7406522d
>                       docs/initiatives/I-52-ct21d-owner-decision-package-v1.md                        7d5bb3cf5576f6a1fd015c58c3156115566839b9
>                       docs/initiatives/I-52-ct21d-authority-contract-v2.1.md                          6259a3beb7445a63620636acd696cdad689965b3
>                       docs/initiatives/I-52-ct21d-authority-contract-v2.6.md                          fd9c16873c526e5f009ad92beb8dd2fe236b964f
>                       src/RackCad.Application/Catalogs/BlockLibrary.cs                                ce8042144e645a6408d7c3284b37e8808d2b4632
>                       deploy/build-bundle.ps1                                                         9a4e6adecf935b90b52f97b1450aac30970d48f1
>                       eng/research/I52Ctda/README.md                                                  e8cd6df4ac04d5f991ab07f655778a1ee2162661
>                       docs/guias/validacion-manual-autocad.md                                         6ae44d08c53df2a3a0f71fab109a791285d229ff
> NOT RELIED UPON     = BA-07 V8 and BA-10 V8 (in preparation in the same round, not committed when this package was written); the
>                       V7 files above are cited. None of the activities of this package uses a value that BA-10 V8 will carry
>                       (section 4.0, "Repetition")
> CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
> ```

## 0. Reading rules

- This package **prepares** the gate; it does not run, authorize or start anything. The Owner's authorization is quoted in section 1 and is
  not widened here. Where a source is ambiguous the row says `NEEDS COORDINATOR RULING` and the question is listed in section 7; the
  implementer decides none of them.
- **UNKNOWN** in this package means: not established in the repository. No hash, build, path, count or approval is invented.
- Every item is classified by the exact text of BA-11 V9 section 3.3 and of the Owner's message. A neighbouring activity that is
  "similar" is not in scope (section 2).
- **Who operates.** The host is operated by the Owner / the CAD manager on the designated machine. **The agent does not operate the
  host and does not start any host run**, and nothing in the Owner's message or in section 233 changes that (section 233, "Lo que NO
  cambia"). A later gate would have to say otherwise in terms.
- Spanish passages are the Owner's words and are quoted verbatim; the English around them is the implementer's.

## 1. Authorization scope (quoted)

### 1.1 What BA-11 V9 section 3.3 says (verbatim)

> `BASELINE_READY` cannot be reached without host work that is **not authorized** today. The items below are recorded as the **plan** of one future **non-governing** host gate; this registry authorizes none of them, and each needs an explicit authorization.

| Item | Needed by | What it produces |
|---|---|---|
| machine-attribute observation (read-only, no instrument, no manifest) | the BA-10 entry (`PARAM-05` label); Owner Q-O5 | the attribute values of the machine class |
| `TOL_SCALE` test and record (K-3) | the BA-08 entry (section 10); the BA-04 entry (declaration of the S comparisons) | the evidence for the Architect's `TOL_SCALE` decision |
| library entity-type census (K-2), with the AR4-10 confirmations (dimension-override storage, order and equal-to-style behaviour) | the BA-05 entry (section 5); the BA-08 entry | the census record over every block of the library file |
| fixture instances and the `FX-IMP` construction (K-8) | the BA-08 entry (section 12); the BA-04 entry (`BLOCKED_BY` K-8) | the instances and their conformance records (instance hashes become the `FIXTURE_INSTANCE@<FX>` pins) |

So BA-11 3.3 names **four rows**. It names no other K-item than K-3 (row 2), K-2 (row 3) and K-8 (row 4).

### 1.2 What the Owner authorized (verbatim, from the Owner's message of 2026-09-30)

```text
HOST GATE — BA-11 §3.3
==================================================

HOST_GATE_BA11_3_3 = AUTHORIZED

Pero SOLO para:

NON_GOVERNING HOST CHARACTERIZATION / BASELINE-PREPARATION EVIDENCE

en la máquina designada por el CAD manager y en sesiones que satisfagan la
tuple exacta.

Esta autorización incluye únicamente las actividades host que el workflow
actual ya identifica como necesarias para cerrar artefactos de baseline,
por ejemplo:

- library-DWG entity census;
- TOL_SCALE non-governing characterization;
- fixture instantiation/feasibility checks;
- machine/session fact capture;
- otros K-* explícitamente autorizados por BA-11 §3.3.

NO autoriza por sí sola:

- governing CT-21D campaign;
- ADMISSION_EVALUATION;
- product admission;
- G3 reopening;
- CT-50;
- Owner Act 2;
- replacement Freeze;
- acceptance of W-scan residual;
- acceptance of final reduced guarantee.
```

### 1.3 The Owner's execution discipline (verbatim)

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
No automatic retries.
```

### 1.4 The Owner's machine policy (Q-O5, verbatim)

```text
Confirmo la política de machine class de BA-10 §2.2 con esta restricción:

Para la primera caracterización CT-21D no se intenta demostrar portabilidad
a una clase amplia de máquinas.

La autoridad se liga a UNA máquina física concreta y estable.

Delego al CAD manager la selección de esa máquina.

El CAD manager debe registrar antes de cualquier host run:

- machine identity;
- Windows version/build;
- AutoCAD version/build;
- AutoCAD profile identity;
- relevant hardware identity/class;
- user/profile identity según el contrato;
- loaded-module/plugin baseline;
- configuration inputs requeridos por la tuple.

Durante la campaña:

- no updates;
- no profile changes;
- no plugin changes;
- no machine-class changes.

Cualquier cambio material:
NEW MACHINE/SESSION TUPLE
y no transferencia silenciosa de evidencia.

No interpretar esta decisión como evidencia de que otras máquinas son
equivalentes.
```

### 1.5 Reading of the scope (implementer's, for the Coordinator to confirm or correct)

1. **Authorized**: only the activities that the current workflow already identifies as needed to close baseline artifacts, and only
   where BA-11 3.3 names them (the Owner's list is introduced by "por ejemplo", but the closing line restricts the open category to
   K-items "explícitamente autorizados por BA-11 §3.3"). BA-11 3.3 names K-2, K-3 and K-8 and the machine-attribute observation.
2. **Not authorized by this gate**: everything in the Owner's `NO autoriza` list, and (reading the text without stretching it) every host
   activity that BA-11 3.3 does not name, including the characterization scenarios `HDM-CHAR-*`, `E4-LEARN-R1-*` and `LK-*` and the
   warm-up dry runs (section 2, rows G11 to G14).
3. **The Owner authorized the gate; he did not name the machine.** The Owner decision package V1 (section 3) had asked for it ("This needs an explicit Owner authorization, naming the machine and AutoCAD session"); the
   Owner delegated that to the CAD manager (Q-O5). Until the CAD manager's designation record exists (section 3.1) the gate has no
   machine and nothing may run.

## 2. Gate membership

Legend. **IN** = named in BA-11 3.3 or in the Owner's list. **OUT** = excluded by the Owner's `NO autoriza` list, by the nature of the
activity, or because BA-11 3.3 does not name it and it is not a baseline-closing item. **RULING** = `NEEDS COORDINATOR RULING`.
`Hn` = the activity id used by section 4.

| Id | Candidate activity | Source of the candidate | Membership | Why |
|---|---|---|---|---|
| G1 | machine-attribute observation: the six attributes of `PARAM-05` (`MachineGuid`, OS version and build, AutoCAD product identity, AutoCAD profile identity, `SECURELOAD`, `TRUSTEDPATHS`) | BA-11 3.3 row 1; BA-10 V7 2.2 `PARAM-05` "label value" | **IN** (H1) | named in BA-11 3.3 row 1; Owner: "machine/session fact capture" |
| G2 | machine / session fact capture beyond the six: Windows version/build, AutoCAD version/build, hardware identity/class, loaded-module and plugin baseline, configuration inputs of the tuple, session facts (PID, start time, document/database identity, scratch hash) | Owner Q-O5 list; Owner "machine/session fact capture"; V2.1 2.1 | **IN** (H1, and the session part of every Hn) | the Owner lists these as the CAD manager's pre-run record and names "machine/session fact capture" as an included activity. They are a record of facts, not a scenario. Which of them are class-defining is a separate question (section 7, A-2) |
| G3 | library entity-type census (K-2), steps 1 to 4 of BA-05 V5 section 5: every block table record of the library file, classes, dxf names, counts, nested names, symbol records, proxy/custom flag, anonymous-reference flag, dimension `DSTYLE` pairs in host order, dynamic properties through a scratch-database reference | BA-11 3.3 row 3; BA-05 V5 section 5; BA-08 V7 K-2, section 14 | **IN** (H2) | named |
| G4 | AR4-10 confirmations, read part: storage location of the stored dimension overrides (the `DSTYLE` section of the `ACAD` extended data) and host order of the pairs, read from the library's dimensions | BA-11 3.3 row 3 (parenthesis); BA-05 V5 5 step 5 (a), 2.3.2 | **IN** (H2) | named ("dimension-override storage, order") |
| G5 | AR4-10 confirmation, **write part**: write each override the product writes, once with a value different from the style and once equal to it, in a scratch database, and read the stored section back ("equal-to-style behaviour"); group-code designation of the dimension variables (BA-05 V5 3.1.2) | BA-11 3.3 row 3 (parenthesis: "equal-to-style behaviour"); BA-05 V5 5 step 5 (a) attributes this write to "the I-12 qualification on the exact build" | **RULING** (H3) | the behaviour is named in BA-11 3.3, but BA-05 V5 assigns the write-back to "the qualification". Qualification of instruments is OUT (G15). The Coordinator must say whether the write-back probe is a characterization probe inside this gate (reading: IN, it is the AR4-10 confirmation BA-11 3.3 names) or a qualification step outside it (reading: OUT). It writes to a scratch database only and never to the library copy |
| G6 | BA-05 V5 5 step 5 (b) host value types of the `RB` ranges and (c) host type of each context variable read by name | BA-05 V5 section 5 step 5 and L-4; section 6 L-4 | **RULING** (H3) | BA-05 L-4 requires (a), (b), (c) "before the candidate", but BA-11 3.3 names only the AR4-10 items (a). (b) and (c) are not named in 3.3 or in the Owner's list. Without a ruling they are not authorized, and BA-05's candidate cannot be reached without them |
| G7 | `TOL_SCALE` non-governing characterization test (K-3) | BA-11 3.3 row 2; BA-08 V7 10, K-3; Owner list | **IN** (H4), **blocked by a missing test design** | named. But **no document defines the test** (section 5, T-4); BA-08 V7 10 says only "separate decision, test and record". The decision of the value is the Architect's and is not a host activity |
| G8 | construction of the fixture construction build of `69daf03a` and of the `FX-ANN-BLANK` instance (step 1 on that build, step 2 on the exact build) | BA-11 3.3 row 4; BA-08 V7 12.1, 12.6 item 6; AR6-02 | **IN** (H5) | named ("fixture instances"); the build role is `FIXTURE_CONSTRUCTION_BUILD` (BA-08 V7 12.1 conditions 1 to 7). Building the DLLs is not a host run, but the instance is |
| G9 | instances of the other fixtures: `FX-1F`, `FX-2F`, `FX-4F`, `FX-ANN`, `FX-DIM`, `FX-BIG`, `FX-SCRATCH`, and the variants `FX-1F-XREF-UNRESOLVED`, `L-VAR-DIFF`, `L-VAR-PROXY` | BA-11 3.3 row 4 ("fixture instances"); Owner "fixture instantiation/feasibility checks"; BA-08 V7 12.3, 12.5, 16.2 K-1 | **IN** (H6) as to scope, **RULING** as to build identity and timing | the words "fixture instances" and "fixture instantiation/feasibility checks" cover them. But BA-08 V7 16.2 K-1 fixes the instance pins "before the first governing run", not before the baseline, and the instance must be drawn by "the exact build" (the `PRODUCT_BUILD`), whose identity does not exist yet (section 3.4). The Coordinator decides whether they are built now as feasibility evidence (candidates, not pins) or later (section 7, A-3) |
| G10 | `FX-IMP` construction and variants `FX-IMP-CASE`, `FX-IMP-NESTED`, `FX-IMP-SYMTAB`, `FX-IMP-XREF-HOMONYM` (K-8) | BA-11 3.3 row 4; BA-08 V7 12.5, K-8; BA-04 V7 `BLOCKED_BY` K-8 (45 scenarios) | **IN** (H7) | named; `SPECIFICATION_OPEN` today (BA-08 V7 12.5: "decided with the fixture-instance gate (host)"); the construction specification has to be decided as part of the activity (T-6) |
| G11 | `HDM-CHAR-FX1F`, `HDM-CHAR-FXANN`, `HDM-CHAR-FXDIM`, `HDM-CHAR-FXANNBLANK` (feed the `HOST_DEFAULT_MAP` pin) | BA-04 V7 section 4 order item 5; BA-10 V7 2.2 `CHAR_RUNS`; BA-08 V7 16.2 K-4 | **RULING** (recommendation: not part of this gate; own gate) | BA-11 3.3 does not name them. They are `NON_GOVERNING_BY_DESIGN` host runs, but they are scenarios of the sealed-catalog flow (status `SEALED_PENDING_PIN_CANDIDATE`), they run on the `CHARACTERIZATION_BUILD` (K-10, which does not exist), each costs `N_A + V(s)` runs (so at least 4 x 44 = 176 runs plus the varied-key runs), and their pin is "fixed before the first governing run (not baseline blockers)" (BA-08 V7 16.2 K-4). They are not needed to close a baseline artifact. The Owner's closing category is limited to K-items "explícitamente autorizados por BA-11 §3.3" and this one is not. Reading them into the gate would stretch the authorization, so this package does not |
| G12 | `E4-LEARN-R1-LM1`, `E4-LEARN-R1-LM2`, `LK-LM1-PROPS`, `LK-LM1-CONFLICT`, `LK-LM2-PROPS`, `LK-LM2-CONFLICT` | BA-04 V7 section 4 order items 1 to 3; BA-10 V7 `CHAR_RUNS` = `N_A` | **RULING** (recommendation: own gate) | same reasons as G11, plus the `E04_ADMITTED_SET@R-1` pin needs the K-5 ratification of route R-1 and the selected `LOCK_MODE`; 6 scenarios x `N_A` = 264 runs. Not named in BA-11 3.3 |
| G13 | `WU-DRY-*` warm-up dry runs (167 scenarios in BA-04 V7; `WU_DRY_RUNS` = `N_B` = 29 each, i.e. 167 x 29 = 4843 runs by arithmetic on the cited counts) | BA-09 V7 4 (rule), W-2; BA-04 V7 section 5; BA-10 V7 `WU_DRY_RUNS` | **OUT** | the dry runs measure the warm-up of the governed window of governing scenarios (the Owner's list excludes the governed window; the task excludes "warm-up governing window"). They are not named in BA-11 3.3, need the characterization build and the warm-up harness (BA-09 V7 W-7, "No warm-up and no dry run has been run"), and close no baseline artifact (W-2 is a parameter applied by BA-04, not a host item) |
| G14 | BA-06 V7 section 7 dynamic-trace dry runs of the implemented seams | BA-06 V7 header (`INVENTORY_COMPLETENESS = NOT_ESTABLISHED`), section 7; BA-09 W-7 | **OUT** | not in BA-11 3.3; they need seams that are not implemented (`grep RACKMIRROR src` at the working tree, whose `src/` is byte-identical to 95690c28, finds nothing) and the characterization build |
| G15 | qualification of instruments: `Q-*`, `Q-I14-*`, `E6-C*`, `WU-BOUNDARY(-CB)`, `Q-FPSPEC-VECTORS`; the I-12 qualification as an instrument qualification; per-build qualification records | V2.1 4.3, 4.3.1; BA-09 W-4; BA-05 V5 L-4 | **OUT** | excluded in terms by the task, and qualification is per exact build under test (AR4-06 (3)), after the baseline. Note the interplay with G5 above |
| G16 | learning and validation scenarios `EV-L-*`, `EV-V-*`, `EV-L-COMPOSED`, exploratory `E6-C4(-CB)` | BA-04 V7 | **OUT** | governing (EV-L, EV-V) or dependent on an unreviewed event model; not baseline preparation |
| G17 | governing CT-21D runs, `ADMISSION_EVALUATION`, product admission, G3 reopening, CT-50, Owner Act 2, replacement Freeze, acceptance of the W-scan residual, acceptance of the final reduced guarantee | Owner's `NO autoriza` list | **OUT** | excluded in terms |
| G18 | manifest capture (EXEC-11), build-equivalence record, bound-SHA precondition record, custody log and anchor creation | BA-07 V7 sections 3, 5.3, 5.4, 6, 9 | **OUT** | execution prerequisites after the baseline (BA-07 V7 3: "The Coordinator authorizes the capture"; no such authorization exists); no machine-profile manifest is captured by this gate. The precondition verifier `docs/automation/evidence/I-52-ct21d-precondition-verifier.py` is an offline tool, not a host activity |
| G19 | UNDO / rollback evidence (V2.1 section 16); scan characterization experiment (V2.1 10.6) and post-CP characterization (10.7); residual W-scan | V2.1 10.6, 10.7, 16 | **OUT** | not named in BA-11 3.3; 10.6 is "design only, not executed"; the W-scan residual is in the Owner's `NO autoriza` list |
| G20 | "otros K-*" of the Owner's list | Owner list, last bullet | **RULING** (reading: none beyond K-2, K-3, K-8, and K-1 through the fixture instances) | BA-11 3.3 names no other K-item. K-5 (Coordinator ratification of route R-1) is not a host activity. K-4 is G11. K-6 is closed. K-7, K-9, K-10, K-11, K-12 are execution prerequisites (BA-08 V7 16.3), not baseline-closing host items. The Coordinator confirms that no other K-item is meant |
| G21 | writing or building new instruments, the characterization build, the RACKMIRROR implementation | section 5 | **OUT** of this gate (not a host run) | listed as tooling gaps; writing them is not authorized by anything so far |

## 3. Pre-run checklist (the Owner's order)

No item below is done. Items 3.1 to 3.6 correspond to the Owner's steps 1 to 6; 3.0 holds the preconditions that the steps assume.

### 3.0 Preconditions

| Id | Precondition | State |
|---|---|---|
| P-0.1 | the Coordinator's gate ruling on this package (section 7) | NOT GIVEN |
| P-0.2 | the rulings R-1 to R-13 of section 7 that the Coordinator wants before the first run | NOT GIVEN |
| P-0.3 | every instrument the first activity needs exists, is reviewed and is **in the loaded-module baseline recorded in 3.2** (an instrument loaded into the host after the baseline is a plugin change; section 6) | H1 needs none (section 5, T-1); H2 onward: instrument does not exist (T-2..T-6) |
| P-0.4 | the repository state of the package: commit that contains this package and the Owner record, recorded as the `packageCommit` of every activity record | UNKNOWN until the orchestrator commits |

### 3.1 Step 1 — CAD manager designation record (template)

The CAD manager (Owner-delegated, Q-O5) fills and signs one record. The record is evidence of a designation, not an approval of the
gate. Nothing is invented by the implementer; every field is the CAD manager's.

```text
CT21D_HOST_GATE_DESIGNATION_RECORD
  recordId                      = <CAD manager assigns>
  designationDateUtc            = <ISO 8601>
  designatedBy                  = <name and role of the CAD manager>
  ownerDelegationReference      = Owner message of 2026-09-30, Q-O5 ("Delego al CAD manager la selección de esa máquina")
  gateReference                 = HOST_GATE_BA11_3_3 = AUTHORIZED, NON_GOVERNING ONLY (decisions section 233)
  packageReference              = docs/initiatives/I-52-ct21d-host-gate-execution-package-v1.md @ <git blob id of the file as ruled>
  coordinatorGateRuling         = <decisions section number; EMPTY until given>

  -- the Owner's Q-O5 list, in the Owner's order --
  machineIdentity               = <hostname, asset id, physical machine statement "ONE concrete stable physical machine">
  windowsVersionBuild           = <exact text read, with the command used>
  autocadVersionBuild           = <exact text read: product, version, build; acad.exe SHA-256>
  autocadProfileIdentity        = <exact text read, with the read used>
  relevantHardwareIdentityClass = <CPU model, RAM, GPU model/driver, storage class; as the CAD manager judges relevant, with the reason>
  userProfileIdentity           = <Windows account used to run AutoCAD and the AutoCAD profile; see A-2 on what "according to the contract" means>
  loadedModulePluginBaseline    = <list of every plugin/module/bundle loaded by AutoCAD in the designated profile, with SHA-256 of
                                   each file, taken in a fresh session BEFORE any gate instrument is added; the gate instruments
                                   listed separately and included in the baseline from this record on>
  configurationInputsOfTheTuple = <the BUILD_BOUND/MACHINE_PROFILE_BOUND inputs of V2.1 2.1 that are configuration: SECURELOAD,
                                   TRUSTEDPATHS, CATALOG_FOLDER (V2.5) and its CSV/JSON hashes, library file path and SHA-256, ...>

  -- the six PARAM-05 attributes (BA-10 V7 2.2), as read by H1, copied here only after H1 -- 
  MachineGuid | OsVersionBuild | AutoCadProduct | AutoCadProfile | SECURELOAD | TRUSTEDPATHS      = <exact strings>

  -- freeze statement (Q-O5 "Durante la campaña") --
  noUpdates / noProfileChanges / noPluginChanges / noMachineClassChanges                          = ACKNOWLEDGED BY <CAD manager>
  windowsUpdateAutoInstall      = <state of the policy that would make the machine change by itself, and who disabled it>
  autocadUpdateNotifications    = <state>
  materialChangeRule            = any material change => NEW MACHINE/SESSION TUPLE, no silent transfer of evidence
  noEquivalenceClaim            = THIS DESIGNATION IS NO EVIDENCE THAT OTHER MACHINES ARE EQUIVALENT
```

### 3.2 Step 2 — exact machine/session tuple record

Two layers, because the contract has two (V2.1 2.1: MACHINE_PROFILE_BOUND are machine-level; SESSION_BOUND are per session).

**Machine layer (once, before the first run; repeated only on a new tuple).**

| Field | Class (V2.1 2.1) | Read how (read-only) | In the PARAM-05 closed list of six |
|---|---|---|---|
| OS version and build | MACHINE_PROFILE_BOUND | OS query | yes (2) |
| machine identity class | MACHINE_PROFILE_BOUND | the `MC-` label computed from the six (BA-10 V7 2.2) after H1; the machine name stays in local evidence only | the output, not an input |
| AutoCAD profile identity; `SECURELOAD`; `TRUSTEDPATHS` | MACHINE_PROFILE_BOUND | profile and system-variable reads | yes (4, 5, 6) |
| `MachineGuid` | (PARAM-05 attribute) | registry/OS read | yes (1) |
| AutoCAD product identity | (PARAM-05 attribute) | the exact text H1 records | yes (3) |
| the machine's own module entries (drivers, security software) | MACHINE_PROFILE_BOUND | manifest machine layer in the contract; for this gate, the module/plugin baseline of 3.1 | no |
| hardware identity/class; Windows account; configuration inputs | Owner's Q-O5 list | CAD manager | not in the closed list of six (A-2) |

**Session layer (one record per AutoCAD process of the gate).**

| Field | Class | Note |
|---|---|---|
| process id and process start time | SESSION_BOUND | process query, at start and end |
| document identity; database identity | SESSION_BOUND | document/database reader of the instrument |
| drawing identity and SHA-256 before the activity (private copy) and after | SESSION_BOUND | file hash of the private copy; the original library or template is hashed too and must not change |
| `LibraryInputRecord`-like identity | SESSION_BOUND | for this gate: library source path, stat, SHA-256 of the copied bytes, private copy path (the V2.1 19.1 pattern applied by analogy; the Coordinator confirms, R-9) |
| AutoCAD version and build; `acad.exe` SHA-256; API assembly versions | BUILD_BOUND (V2.1 2.1) | recorded per session even though no build is "under test" in this gate |
| the RackCad build identity used in the session (source SHA; Plugin and UI DLL SHA-256; configuration Release or Debug; absence of `RACKCAD_DEBUG_FAIL_SIBLING_REDRAW_UNIT` when Debug, K-12) | build identity of the activity | only for H5, H6, H7 (and H4 if its design loads RackCad); see 3.4 |
| `SECURELOAD`, `TRUSTEDPATHS`, profile name | MACHINE_PROFILE_BOUND | read before and after the session; a difference stops the gate (section 6) |
| process list at start and end; other `acad.exe` | cleanup (V2.1 20, 21.5 by analogy) | recorded |
| run id | attempt log | section 4.0 |

### 3.3 Step 3 — the Coordinator verifies that each requested activity belongs to the gate

One line per activity, written by the Coordinator in the gate ruling (or in a per-activity authorization record). The Coordinator checks
the row of section 2 and writes IN or OUT. **Only activities with a recorded IN may run; an activity with no line does not run.**

```text
ACTIVITY | section 2 row | membership (Coordinator) | ruling number | conditions
H1       | G1, G2        | <IN>                    | <section>      |
H2       | G3, G4        | <IN>                    |                |
H3       | G5, G6        | <IN / OUT per R-3>      |                |
H4       | G7            | <IN, after test design (T-4)> |          |
H5       | G8            | <IN>                    |                |
H6       | G9            | <IN / later per R-4>    |                |
H7       | G10           | <IN>                    |                |
(G11, G12: HDM-CHAR-*, E4-LEARN-R1-*, LK-*: <OUT of this gate / own gate per R-2>)
```

### 3.4 Step 4 — exact build / package per activity

"The exact build/package required by the current baseline-preparation authority" is, per activity:

| Activity | Build or package | Identity established in the repository | State |
|---|---|---|---|
| H1 | none: the installed AutoCAD of the designated machine, read-only; RackCad is not required | AutoCAD build: UNKNOWN until the CAD manager records it | **UNKNOWN** (machine) |
| H2, H3 | a read-only instrument that does not exist (T-2, T-3) and the **library file** `blocks-library.dwg` (name: `BlockLibraryLocator.FileName`, `src/RackCad.Application/Catalogs/BlockLibrary.cs`, blob `ce8042144e645a6408d7c3284b37e8808d2b4632`; the path is the user override or `<catalog folder>/blocks-library.dwg`, so it is outside the repository) | library path and SHA-256: **UNKNOWN**. BA-05 V5 5: "the exact library file of the baseline (a tuple field)"; the repository holds no DWG | **UNKNOWN** (instrument, library) |
| H4 | UNKNOWN: it depends on the test design that does not exist (T-4) | none | **UNKNOWN** |
| H5 step 1 | `FIXTURE_CONSTRUCTION_BUILD`: a build compiled from a **clean checkout of `69daf03a35c630e453e1d9e98136f128bd0325a4`** (verified in this worktree: commit "Merge I-55: View Placement & Projection (ID17 + ID18 + ID19)", tree `3c34dc9be113e98088acf56653aa5181437d0893`, an ancestor of 95690c28 whose tree is `967c4ccb6a977782fddf06d295ffda5c14a5aba5`; `git diff --stat 69daf03a 95690c28 -- src` lists exactly nine files, the I-60 naming, as BA-08 V7 12.1 says). No other build substitutes (FXB-04) | source SHA: established. **DLL SHA-256 of the Plugin and the UI: UNKNOWN** (no built artifact is in the repository; they are captured when the build is made). Configuration Release or Debug: UNKNOWN (R-11). How it is built and where: not decided (T-7). The canonical script is `deploy/build-bundle.ps1` (blob `9a4e6adecf935b90b52f97b1450aac30970d48f1`; it needs AutoCAD 2025 installed for the compile references and AutoCAD closed, and can write an inventory of SHA-256 per file with `-InventoryOutPath`) | source established; DLLs **UNKNOWN** |
| H5 step 2, H6, H7 | "the exact build" = the `PRODUCT_BUILD` (BA-08 V7 12.1: "A fixture instance is drawn by the exact build") | **UNKNOWN: the PRODUCT_BUILD under test does not exist.** It would contain the RACKMIRROR implementation, which is not in `src/` (no `RACKMIRROR` in `src`). BA-08 V7 3.2 says the bound SHA 95690c28 is where the static facts come from and is never a tuple value, and that the build's own source SHA and DLLs are values of its own BUILD_BOUND profile. The R1/R3 routes the fixtures need exist at 95690c28 | **UNKNOWN**; Coordinator ruling R-4 |

If R-4 rules that a build of 95690c28 (without RACKMIRROR) is the feasibility build for H5 step 2, H6 and H7, its instances are
**candidates for feasibility**, never the `FIXTURE_INSTANCE@<FX>` pins, which stay "fixed before the first governing run" on the exact
build (BA-08 V7 12.6, 16.2 K-1).

### 3.5 Step 5 — preservation of raw evidence

| Rule | Content |
|---|---|
| raw = what the instrument or the operator produced, unedited | exported files, console/log output, screenshots of dialogs (when a screenshot is the only record), the instrument's stdout/stderr, the private copies' hashes. A derived table (a summary, a spreadsheet, the census JSON re-sorted) is a **separate** file that cites the raw file's hash; the raw file is never altered |
| location (proposal; the Coordinator designates it, R-6) | local to the designated machine: `%LOCALAPPDATA%\RackCad\ct21d-evidence\hostgate-prep\<runId>\` (the same machine-local root BA-07 V7 section 2 plans for the machine-profile layer, `%LOCALAPPDATA%\RackCad\ct21d-evidence\`). Machine-profile content stays local; **only hashes and non-machine facts** enter the repository, as BA-07 V7 section 2 says for machine data. Non-machine derived records (for example the census record) may be proposed for `docs/automation/evidence/` by the Coordinator's designation. BA-07 V7 designates no location for gate-preparation evidence (the `ct21d/` folders it plans are for the baseline custody and are not created); this one is new |
| naming (proposal) | `<runId>` = `HGP-<H-id>-<yyyymmddThhmmssZ>-<nn>` (e.g. `HGP-H2-20261001T140500Z-01`), `nn` = the attempt number of the activity; files inside keep the instrument's own names; prefix `NG-` on every derived file |
| hashing | SHA-256 of the **raw bytes** of every file (binary files and DWG: exact bytes; text produced by the gate: as written), recorded in `HASHES.sha256` in the format `<sha256>  <relative/path>` (the inventory format of `deploy/build-bundle.ps1 -InventoryOutPath`), plus the SHA-256 of `HASHES.sha256` itself, written at the end of the activity. Repository text files follow BA-11 V9 section 1 (LF-normalized) when they enter a document |
| write-once | after `HASHES.sha256` is written the folder is set read-only; a later hash mismatch is an `INVALID` fact (V2.1 21.1: "evidence not sealed") |
| nothing edited | an error in a record is corrected by a new file that cites the old one; the old file stays |
| originals | the library file and every template are used through **private copies** (BA-05 V5 5; V2.1 19.1/20 by analogy); the source file is hashed before and after and must not change |
| attempt log | one line per attempt (V2.1 21.3): attempt number, tuple, contract version, state (NON_GOVERNING / INVALID), outcome, reason for stopping; every attempt listed, invalid ones disclosed |

### 3.6 Step 6 — labeling: no result is governing because it ran in AutoCAD

Every activity record, every raw-evidence folder and every derived file carries this block, written by the operator before the first
action and copied into the file header of every derived record:

```text
GOVERNING = FALSE
RUN_STATUS_CLASS = NON_GOVERNING (V2.1 21.1: "a valid execution that by design does not count toward acceptance")
GATE = HOST_GATE_BA11_3_3 (non-governing host characterization / baseline-preparation evidence; decisions section 233)
NOT_EVIDENCE_FOR = any control result, any group verdict, ALT21D_HOST_PASS, admission, G3, CT-50, Owner Act 2
MACHINE_TUPLE = <record id of 3.2>      PACKAGE = <blob id of the ruled package>
```

A result taken in AutoCAD becomes an input of a baseline artifact only through that artifact's own candidate and ratification steps
(BA-11 V9 section 2), never by being cited as a CT-21D result. Core Console, CT-DA and G3A observations remain hypotheses (V2.1 2.3).

## 4. Run plan

### 4.0 Rules common to every activity

**Order.** H1 first (the machine class is frozen by it). Then H2, H3 (if ruled in), H4, H5, H7, H6. H2 and H4 first among the rest because they
gate the candidates of BA-05 and BA-04 (K-2, K-3); H5 to H7 gate BA-08 section 12 and BA-04 (`BLOCKED_BY` K-8). The Coordinator may reorder.

**Classification of a result (existing contract; no new category).**

| Case | Treatment (V2.1 21.1, 21.1.1, 21.3; Owner: "Any FAIL / UNKNOWN / INVALID remains governed by the existing contract. No automatic retries.") |
|---|---|
| a valid execution that reached its end and produced its raw outputs | `NON_GOVERNING` (valid). Record. The data is evidence of behaviour (V2.1 21.1). If the observation differs from what a baseline artifact expected (for example AR4-10 finds another storage location), the artifact changes under its own version rule (BA-05 V5 2.6); it is not a FAIL of anything |
| the activity could not read a fact (the instrument returned nothing, a class has no readable property) | recorded as `UNKNOWN` for that fact, with the raw output; no substitute value |
| tuple mismatch, missing or unreadable log, instrument error, crash, another `acad.exe`/process interfering, unsealed evidence, incomplete cleanup, manual input where the activity is scripted | `INVALID` (V2.1 21.1 list, applied by analogy to the non-governing activity; the Coordinator confirms the mapping, R-12). `INVALID` cannot support a conclusion; all observed facts and logs stay retained (21.1.1) |
| a crash | recorded with an explicit status; it needs a Coordinator ruling (`CRASH_FINDING` or `CRASH_ENVIRONMENT`); never silently invalid; no automatic retry |
| any of the above | **no automatic retry.** A rerun needs the **Coordinator's per-occurrence authorization**, a new run id and a recorded cause (tooling, environment or contamination); the rerun is the same activity, same package, same tuple requirements; changing the activity or its instrument is a new version, not a retry. A FAIL/UNKNOWN is never repeated to obtain a better result |

**Repetition.** No source defines a repetition count for these activities: they are not catalog scenarios. The counts of the Owner's answers (`N_A` = 44,
`N_B` = 29, `N_det` = 90, `n` = 59, `WU_DRY_RUNS` = 29) apply to scenario runs (`CHAR_RUNS`, `N_strict`, dry runs) which are all outside this gate (G11 to
G13) and are **not applied here** (the Owner: "No generalices automáticamente 44 a todos los grupos"). The reading proposed to the Coordinator is
one execution per activity, with `EVIDENCE_REPETITION = 1` as the only recorded Coordinator value that resembles it (section 233); H4's
design may need a stated number of observations of its own (T-4). The count per activity is a ruling (R-7).

**Stop conditions (all activities).** The operator stops, writes the attempt-log line and informs the Coordinator when any of these happens: (1) any
tuple drift (a machine-layer field differs from the record of 3.2); (2) `SECURELOAD`, `TRUSTEDPATHS` or the profile differ between the
before-read and the after-read; (3) the source library or template hash differs from its before-hash; (4) AutoCAD crashes, hangs, shows an
unscripted dialog, or another `acad.exe` appears; (5) an update prompt or an installer of AutoCAD/Windows/the plugins starts; (6) the
instrument reports an error or an unexpected class/property; (7) the operator deviates from the written steps; (8) the evidence folder cannot be
written or hashed. No step is "fixed on the fly".

### H1 — Machine/session fact capture and the six-attribute observation (G1, G2)

| Aspect | Content |
|---|---|
| purpose | give BA-10 the `PARAM-05` label: `MC-` + the first 12 lowercase hex digits of the SHA-256 of the RFC 8785 (JCS) serialization of the JSON object with exactly the six keys `AutoCadProduct`, `AutoCadProfile`, `MachineGuid`, `OsVersionBuild`, `SECURELOAD`, `TRUSTEDPATHS`, every value a JSON string holding the exact text read (`SECURELOAD` as its decimal digits) (BA-10 V7 2.2 `PARAM-05`). Also fill the machine layer of 3.2 |
| inputs | the designation record (3.1); the designated machine; AutoCAD as installed in the designated profile; no RackCad, no instrument |
| tool | **none required by BA-10** ("It needs no implemented instrument and no manifest, and changes nothing"): OS and registry queries and AutoCAD profile/system-variable reads. The repository has no script for this (section 5, T-1). The operator uses commands the CAD manager chooses and **writes each command and its raw output into the record**. The exact string that each attribute represents (e.g. which text is "AutoCAD product identity") is not fixed by BA-10 V7 (A-1), so the record keeps the exact text and the command |
| ordering note | the observation must be taken **after** the profile is in its final state for the campaign. `SECURELOAD` and `TRUSTEDPATHS` are class-defining (attributes 5 and 6): adding the folder of a later instrument or of a RackCad build to `TRUSTEDPATHS` changes the class. So the trusted paths needed by H2 to H7 are decided and set **before** H1 (section 6) |
| expected raw outputs | the six strings with the command used for each and the time; the machine-layer facts of 3.2; the process list; the before/after reads of the profile values |
| non-governing result | `NON_GOVERNING`, record; a missing attribute is `UNKNOWN` and the label is not computed; no guess |
| stop conditions | section 4.0; additionally any change of the six between two reads in the same session |
| consumer | **BA-10** (the `PARAM-05` label, before its candidate: "the label value is written into these bytes"); the machine layer of every later tuple record; the observation record keeps the six strings and the label is recomputed from it |

### H2 — Library entity-type census with the AR4-10 read confirmations (G3, G4)

| Aspect | Content |
|---|---|
| purpose | BA-05 V5 section 5 steps 1 to 4 (and the read half of step 5 (a)): enumerate **every** block table record of the library file (named, anonymous and layout), compute the class universe and `classesWithoutTable`, and read the stored dimension overrides of the library's dimensions in host order |
| inputs | a private copy of the library file; its path, SHA-256 before and after; the designated machine and profile; the schema of the census record (BA-05 V5 section 5 "Schema of the census record") |
| tool | **GAP (T-2).** No census instrument exists in the repository. `eng/validation/I52G3AApiCensus` is a Roslyn static census of the plugin's API use (blob `a41235ab740eaffed87e7b01c427e34408a47e2c`), not a DWG census; `eng/research/I52Ctda` is the CT-DA research harness (README blob `e8cd6df4ac04d5f991ab07f655778a1ee2162661`) and contains nothing about the library, `DSTYLE` or RXClass census (`grep` for census, `DSTYLE`, `TOL_SCALE`, `FX-IMP` finds no instrument in `eng/` or `src/`). Procedure requirements that the instrument must satisfy: read-only on the private copy; dynamic properties read "through a reference created in a separate scratch database of the census (never in the library copy)"; `instrumentBuild` and `dynamicPropertyMethod` recorded |
| expected raw outputs | the census record in the schema of BA-05 V5 section 5 (`libraryFileSha256`, `libraryPath`, `censusUtc`, `instrumentBuild`, `dynamicPropertyMethod`, `blocks[]`, `classUniverse[]`, `classesWithoutTable[]`); the instrument's own log; hashes before and after of the library and its private copy |
| non-governing result | `NON_GOVERNING`; a block, entity or property the instrument could not read is `UNKNOWN` in the record with the reason; an incomplete census is `INVALID` for use as "every block" (BA-05 V5 3 "Universe") |
| stop conditions | section 4.0; additionally any write to the private library copy (hash differs) and any entity class that the instrument cannot name |
| consumer | **BA-05** (step 4: new class-map rows, tables and vectors for the classes without table; the census is "required before the candidate" and changes the bytes: a new `ARTIFACT_VERSION`, BA-05 V5 2.6); the dynamic-property value sets and the units types go to **BA-08** (section 11 rule 2, section 14); the `L-VAR-DIFF` and `L-VAR-PROXY` variants take their content from it (BA-08 V7 12.5); the AR4-10 part to **BA-05** 2.3.2 (`overrides`) and 3.1.2 |

### H3 — Host-type and write-back confirmations (G5, G6) — runs only if the Coordinator rules them in (R-3)

| Aspect | Content |
|---|---|
| purpose | (G5) BA-05 V5 5 step 5 (a), write half: write each override the product writes (`DIMSCALE` 40, `DIMASZ` 41, `DIMEXO` 42, `DIMEXE` 44, `DIMTAD` 77, `DIMTXT` 140, `DIMGAP` 147, `DIMDEC` 271, "to be confirmed by the same step", BA-05 V5 3.1.2) once with a value different from the style and once equal to it, in a scratch database, and read the stored section back; (G6) host value types of the `RB` ranges (2.1.2) and of each context variable read by name (2.1.3) |
| inputs | a scratch database created by the instrument; the product's override list (BA-05 V5 3.1.2); the context variable list of BA-05 V5 2.1.3 |
| tool | **GAP (T-3).** Nothing exists |
| expected raw outputs | per variable: written value, stored `(code, host type)` pairs in host order, whether the equal-to-style value is stored or dropped; per range and per variable: the host type read |
| non-governing result | as H2. A finding that differs from BA-05 V5's draft is not a failure: it changes BA-05 before its seal (BA-05 V5 5 step 5, 2.6) |
| stop conditions | section 4.0; any write outside the scratch database |
| consumer | **BA-05** (L-4, before its candidate); the oracle of **BA-08** predicts "the stored set under the confirmed behaviour" (BA-05 V5 2.3.2) |

### H4 — `TOL_SCALE` non-governing characterization (G7) — blocked until a test design exists (T-4)

| Aspect | Content |
|---|---|
| purpose | produce "the evidence for the Architect's `TOL_SCALE` decision" (BA-11 3.3 row 2). The slot is `UNSET — NO AUTHORITY FOR CT-21D` (BA-08 V7 10): the product value is absolute, "fixed at G4 with test and record"; the phase-1 ruling says it "needs a decision with test and record" and "may rely on measurements of non-governing dry runs, never governing outputs" (`docs/automation/evidence/I-52-ct21d-architect-phase1-ruling.md`, item 5). BA-05 V5 uses the slot for the scale factors of a reference (`scaleX/Y/Z`; 3.1.2). 138 scenarios in BA-04 V7 carry `BLOCKED_BY = K-3` |
| inputs | **a test design that no document contains.** BA-08 V7 10 and 16.1 say "separate decision; the test needs non-governing host evidence (not decided by this session)". The implementer does not invent it. What the design must state before the Coordinator can authorize H4: the quantity measured; the operations that produce it (product routes or host operations); the number of observations and why; the rule that turns observations into a bound (absolute, not relative); the build on which they run; what makes an observation `INVALID`; and that no governing output is used |
| tool | UNKNOWN until the design exists (T-4) |
| expected raw outputs | per the design |
| non-governing result | `NON_GOVERNING`; the decision of the value is the Architect's, not a result of the run |
| stop conditions | section 4.0 plus the design's own |
| consumer | **BA-08** section 10, row `TOL_SCALE` (the decision); **BA-04** (declaration of the S-1..S-3 comparisons; 138 scenarios with `BLOCKED_BY = K-3`); **BA-05** names the slot only |

### H5 — `FX-ANN-BLANK` instance (G8)

| Aspect | Content |
|---|---|
| purpose | build the legacy-state fixture exactly as BA-08 V7 12.1 pins it and record its conformance (12.6) |
| inputs | the `FIXTURE_CONSTRUCTION_BUILD` (3.4); the exact build for step 2 (R-4); an empty template (the fixture template of BA-08 V7 12.3 `FX-SCRATCH` row is the learning one; the template for the instances is "a template drawing", BA-08 V7 12.6: its identity is UNKNOWN); the library file and the catalog folder (their SHA-256, 12.6 item 6); the editor inputs of BA-08 V7 12.2 and 12.3 (`FX-ANN-BLANK`: `D` = 1, 2 x 2, `DrawBasePlate` and `DrawRackName` on, blank logical `Name`) |
| steps (pinned by BA-08 V7 12.1) | step 1: on the construction build, route R1 (`RACKSELECTIVO`, a **new rack**, then the single-view button **`Insertar frontal`**); step 2: on the exact build, route R3 (`RACKEDITAR` on the frontal, no input changed, then **`Insertar planta`**) |
| tool | the product's own commands, operated by hand by the Owner/CAD manager (the contract's no-touch rule, V2.1 21.5, binds governing runs only; this activity is non-governing and manual operation is intended, but it is declared, R-9). Conformance reader: **GAP (T-5)**: the authored-document bytes, the envelope `Name`, the inner design `Name`, the view block names, the layer `RACKCAD_ANOTACIONES`, `PSTYLEMODE` and the keys of 11.2 must be read "with the exact build's product reader"; no script exists |
| expected raw outputs | the instance file and its SHA-256; the conformance record of BA-08 V7 12.6 items 1 to 6 (authored bytes of every source view; keys of 11.2; `PSTYLEMODE` = 1; the construction record: build identity per step, route, action, ordered addresses, answers to prompts; source references with rotation 0, scale (1,1,1), normal +Z; for `FX-ANN-BLANK`: construction build identity, catalog and library hashes, blank `Name` in both copies, view block names `Selectivo` and `Selectivo planta - 2 frentes`, `DrawRackName` true, no `RACKCAD_ANOTACIONES` layer); the absence of `RACKCAD_DEBUG_FAIL_SIBLING_REDRAW_UNIT` when a step runs on a Debug build |
| non-governing result | `NON_GOVERNING`. A non-conforming instance (a field authored differently from 12.2, a layer present, a name that differs) is **recorded as non-conforming**, with the raw evidence; it is not repaired by editing |
| stop conditions | section 4.0; any prompt that the pinned sequence does not predict; a block name different from the expected names (recorded, not corrected) |
| consumer | **BA-08** sections 12.1, 12.6 and 4.1; **BA-04** (the pin slot `FIXTURE_INSTANCE@FX-ANN-BLANK`, whose hash is fixed before the first governing run; the conformance record is cited in `sourceEvidence`, BA-07 V7 5.1, 5.5); the identity of the construction build enters custody through the conformance record (BA-07 V7 5.5) |

### H6 — Instances of the other fixtures (G9) — scope and build per R-4

| Aspect | Content |
|---|---|
| purpose | feasibility and, later, the `FIXTURE_INSTANCE@<FX>` pins of `FX-1F`, `FX-2F`, `FX-4F`, `FX-ANN`, `FX-DIM`, `FX-BIG`, and the learning template `FX-SCRATCH`, plus the variants `FX-1F-XREF-UNRESOLVED`, `L-VAR-DIFF`, `L-VAR-PROXY` |
| inputs / steps | BA-08 V7 12.1 (step 1: R1 on the exact build; step 2, where the fixture has more views: R3 with `Insertar planta` for `D` = 1 or `Insertar varias…` for `D` > 1), 12.2, 12.3, 12.5; `L-VAR-DIFF` and `L-VAR-PROXY` need the census (H2) first ("content depends on the census") |
| tool | as H5 (T-5) |
| expected raw outputs / classification / stop | as H5 |
| consumer | **BA-08** section 12; **BA-04** pin slots (372 `FIXTURE_INSTANCE` uses in BA-04 V7) |

### H7 — `FX-IMP` and its variants (G10, K-8)

| Aspect | Content |
|---|---|
| purpose | decide and demonstrate the construction of `FX-IMP`: "reachable with product operations or declared artificial with its reason" (BA-08 V7 12.5); and of `FX-IMP-CASE`, `FX-IMP-NESTED`, `FX-IMP-SYMTAB`, `FX-IMP-XREF-HOMONYM` |
| inputs | the candidate reachable construction in BA-08 V7 12.5 ("the source is drawn while the configured library lacks at least two blocks its plan requires; the drawer omits those pieces and the product reports them on insert; the library later supplies them"), **not decided**; whether a source with omitted pieces is an admissible mirror source is "part of K-8"; the census (H2) to know which blocks the library holds; a library copy that lacks at least two blocks (a modified copy of the library: its identity is a new input) |
| tool | the product's commands by hand; a conformance reader (T-5); the construction needs a specification that does not exist yet (T-6) |
| expected raw outputs | for each fixture: the specification used (written before the run), the instance and its SHA-256, the product's report of omitted pieces, the block table before and after, the conformance facts of BA-08 V7 12.6 |
| non-governing result | `NON_GOVERNING`; "not constructible with product operations" is a valid result, recorded with the raw evidence, and leads to "declared artificial with its reason" (the Architect/Coordinator decide) |
| stop conditions | section 4.0; any step that would modify the user's real library or catalog (all work is on copies, and the catalog folder used is a copy declared in the tuple) |
| consumer | **BA-08** sections 12.3, 12.5 (the K-8 state) and 16.1; **BA-04** (`BLOCKED_BY` K-8: 45 scenarios) |

## 5. Tooling gaps

Nothing in this section is authorized: the instruction chain so far authorizes **no new tooling** under `eng/` or `src/`. Each gap is a Coordinator decision
(section 7, R-5). The minimal path proposed for every instrument is the same, for the Architect's review before any of it is written:
a **read-only**, console-less instrument placed under a new folder `eng/research/` (separate from `src/`, referenced by no product project, no solution file and no `deploy/` script,
as the CT-DA harness is, README of `eng/research/I52Ctda`), reviewed by the Architect for read-only behaviour, built from a committed SHA, its DLL hash recorded,
and its folder trusted **before H1** so that it is in the loaded-module baseline (section 6). A scripted alternative (AutoLISP or a script file) is possible for part
of the census; the choice is the Architect's (a script cannot name the host class of every entity as precisely as a managed read, which the
census schema requires: `rxClassName`, host value types).

| Id | Missing instrument or artifact | Needed by | Exists today? | Evidence of the search |
|---|---|---|---|---|
| T-1 | script for the six-attribute observation and the machine layer | H1 | **No script.** BA-10 V7 2.2 says no instrument is needed; commands chosen by the CAD manager and recorded are sufficient. A helper that only formats the six strings and computes the label could be written offline (no host) | no match in `eng/` |
| T-2 | library entity-type census instrument (BA-05 V5 section 5 schema) | H2 | **No** | the only "census" in `eng/` is `eng/validation/I52G3AApiCensus` (static API census); `eng/research/I52Ctda` is the CT-DA research harness |
| T-3 | scratch-database write-and-read-back probe for dimension overrides; host-type readers for `RB` ranges and context variables | H3 | **No** | as above |
| T-4 | the `TOL_SCALE` test **design** (a document, then an instrument) | H4 | **No design anywhere** (BA-08 V7 10, 16.1) | no document defines the test |
| T-5 | fixture conformance reader (authored document bytes, envelope and inner `Name`, block names, layers, `PSTYLEMODE`, keys of 11.2, source-reference transforms) | H5, H6, H7 | **No.** BA-08 V7 K-7 lists "the readers and comparators marked MISSING in section 6" (a different, execution-prerequisite item) | no match in `eng/` or `src/` |
| T-6 | written construction specification of `FX-IMP` and its four variants | H7 | **No** (`SPECIFICATION_OPEN`, BA-08 V7 12.5) | BA-08 V7 12.5 |
| T-7 | build and packaging of the `FIXTURE_CONSTRUCTION_BUILD` from a clean checkout of `69daf03a`, and of the exact build for H5 step 2 / H6 / H7: who builds, where, the configuration, and how the DLL hashes are captured | H5, H6, H7 | the build script exists (`deploy/build-bundle.ps1`, with `-InventoryOutPath`); the decision does not. Note from the project memory and from `deploy/build-bundle.ps1`: building needs AutoCAD closed (the plugin DLL is locked while loaded) and AutoCAD 2025 installed for compile references, and the .NET SDK of `global.json` (8.0.423) | `global.json` at 95690c28 |
| T-8 | the precondition-verifier output of K-11 for the exact build | not an H-activity; execution preparation | the verifier exists (`docs/automation/evidence/I-52-ct21d-precondition-verifier.py`, "has not been run against any build", BA-08 V7 16.3 K-11) | BA-08 V7 16.3 |
| T-9 | the evidence folder tool: hash the folder, write `HASHES.sha256`, set it read-only, record the process list | all | **No script** (a few shell commands suffice; the CAD manager may do it by hand and record it) | none |

## 6. Session discipline

**Machine rules (Owner, Q-O5, section 1.4).** One concrete stable physical machine chosen by the CAD manager. During the campaign: no updates (Windows, AutoCAD,
drivers, plugins, security software), no profile changes, no plugin changes, no machine-class changes. Any material change is a **NEW MACHINE/SESSION TUPLE**
and no evidence is carried over silently; the Owner's record states that this is no evidence that other machines are equivalent.

**What counts as a material change (proposal, for the Coordinator to confirm, R-8).** Any change of the six class-defining attributes (BA-10 V7 2.2:
"change of a class-defining attribute: a new machine class (a new tuple)"): the Windows `MachineGuid` (a different machine or a reinstall), the OS version or build (a Windows
update), the AutoCAD product identity, the AutoCAD profile identity (a different or reset profile), `SECURELOAD`, `TRUSTEDPATHS` (adding a trusted folder).
Further, by the Owner's list: a change of the AutoCAD version or build, of the hardware class, of the Windows account, of the loaded-module/plugin baseline (an added,
removed or updated plugin, an added instrument, a different RackCad build installed as a plugin), or of any configuration input of the tuple (catalog folder content, library file,
settings). A change of the module entries only is, by BA-10, a mismatch of its own field and not a new class; but the Owner's "no plugin changes" is stricter, so this package treats it as a stop.

**Tensions the Coordinator has to rule on before the first run.**

1. *The instrument and the trusted path.* An instrument loaded into AutoCAD adds a module and may need `TRUSTEDPATHS`/`SECURELOAD` handling. Both are part of the baseline. Therefore every instrument for
   H2 to H7 must be final and its folder trusted **before** H1, or a new tuple follows. Practical consequence: H1 cannot run until the instrument set is known (R-5).
2. *Two RackCad builds in one campaign.* H5 needs a build of `69daf03a` (step 1) and the exact build (step 2). The Owner says "no plugin changes". BA-10 keeps build-dependent fields in the build tuple and
   not in the machine class, and BA-08 V7 12.1 makes the `FIXTURE_CONSTRUCTION_BUILD` a separate build role. Whether loading a different RackCad build in a separate AutoCAD session
   is a "plugin change" of the Owner's rule or a build-tuple fact is **not decided by any source**. The conservative reading is that it needs the Owner's (or the Coordinator's) explicit statement. Options:
   (a) the RackCad build is a session fact recorded per session, the module baseline is the non-RackCad modules plus the loader mechanism; (b) each RackCad build change is a new tuple; (c) use `NETLOAD`
   of a DLL path outside the installed bundle per session so that no installed plugin changes. The package does not choose (R-8).
3. *AutoCAD open or closed.* Building and replacing the DLL needs AutoCAD closed (project memory; `deploy/build-bundle.ps1` header). H1 needs a session or at least profile reads; H2 to H7 need an
   interactive AutoCAD (or, for the census, possibly the Core Console, whose observations are "a hypothesis, never CT-21D evidence" for governing purposes, V2.1 2.3, but this gate is non-governing: R-9).
4. *Process hygiene (by analogy with V2.1 20, 21.5).* One fresh AutoCAD process per activity attempt; verify before launch that no `acad.exe` exists; verify its absence at the end; record the process list at start and end;
   no other RackCad-loaded session; profile values read before and after.

**Who operates.** The Owner or the CAD manager, on the designated machine. The agent does not operate the host and does not start any run, and no instruction so far lets it. If a later gate asks the agent
to drive the host, that gate must say so and bound it.

## 7. What the Coordinator must verify before the first run

**No host run starts until the Coordinator's gate ruling on this package.** The ruling is a decisions-log entry that names this package by its blob id, lists the activities with recorded IN
(section 3.3), and answers the questions below. Until then `HOST_RUN_STARTED = FALSE`.

### 7.1 Verifications (checklist)

- [ ] V-1. The Owner's record `cdb1a13682c63571cf4a4b4bb03bbf0fb0dc19ec` is the verbatim message and section 233 quotes it (sections 1.2 to 1.4 of this package quote the same lines).
- [ ] V-2. The gate membership of section 2 is correct row by row, especially G5, G6, G9, G11, G12, G20 (the rows marked RULING).
- [ ] V-3. The CAD manager's designation record (3.1) exists, is complete and is signed; the six attributes are read (H1) and the label is recomputed from the record.
- [ ] V-4. The machine/session tuple record (3.2) is complete; the loaded-module baseline includes every gate instrument; `SECURELOAD`/`TRUSTEDPATHS` are in their final state.
- [ ] V-5. For every activity to run, the instrument or build exists, is reviewed and its SHA-256 is recorded (3.4); an item that is UNKNOWN in 3.4 is not run.
- [ ] V-6. The evidence location and naming (3.5) are designated.
- [ ] V-7. The attempt log and the per-occurrence retry authorization procedure are in place; the stop conditions (4.0) are known to the operator.
- [ ] V-8. The labeling block (3.6) is on every record.
- [ ] V-9. The `FIXTURE_CONSTRUCTION_BUILD` can be traced to `69daf03a35c630e453e1d9e98136f128bd0325a4` (its tree `3c34dc9be113e98088acf56653aa5181437d0893`), and no other build substitutes (FXB-04).
- [ ] V-10. The `PARAM-04` value `m` = 3 and `EVIDENCE_REPETITION` = 1 of section 233 are the Coordinator's values subject to the Architect's round V8; this package uses neither (no activity has a repetition slot), and the retry rule here is the contract's (per-occurrence authorization), not an `m`-bounded allowance.

### 7.2 Rulings requested (each is a genuine open question; the implementer proposes, does not decide)

| Id | Question | Proposal |
|---|---|---|
| R-1 | Is "otros K-*" empty, i.e. only K-2, K-3, K-8 (and K-1 through the fixture instances) are in the gate? | yes (BA-11 3.3 names no others) |
| R-2 | Are `HDM-CHAR-*`, `E4-LEARN-R1-*`, `LK-*` part of this gate or do they need their own? | own gate (not named in BA-11 3.3; need the characterization build, the sealed catalog, K-5; produce pins fixed before the first governing run) |
| R-3 | Are the write-back probe of AR4-10 (G5) and the `RB`/context-variable type confirmations (G6) inside this gate? | G5: yes, as the "equal-to-style behaviour" BA-11 3.3 names, on a scratch database; G6: yes only by an explicit ruling, because BA-05 V5 L-4 needs it before its candidate and 3.3 does not name it. If not, BA-05 V5 cannot reach its candidate |
| R-4 | What is "the exact build" for H5 step 2, H6 and H7 while the PRODUCT_BUILD does not exist? May a build of 95690c28 without RACKMIRROR be used for feasibility, with its instances as candidates only? | yes, candidates only; the pins are fixed on the exact build before the first governing run |
| R-5 | Who authorizes writing the read-only instruments of section 5 and under which path; must they be final before H1? | a new folder under `eng/research/`, Architect review for read-only behaviour, final before H1 (section 6, tension 1) |
| R-6 | Designation of the evidence location, naming and the repository part (3.5) | as proposed in 3.5 |
| R-7 | Number of executions per activity | one each (`EVIDENCE_REPETITION` = 1), H4 per its design |
| R-8 | Which changes are material (section 6), and is a different RackCad build in a separate session a "plugin change" (Owner's rule) or a build-tuple fact? | needs the Owner's or the Coordinator's statement; implementer's reading: option (a) with `NETLOAD` from a path outside the installed bundle |
| R-9 | May the activities use manual operation (non-governing), interactive `acad.exe` vs Core Console for the census, and the V2.1 19.1/20 pattern applied by analogy (private copies, fresh process per attempt)? | manual operation yes; interactive `acad.exe` for every activity that loads RackCad; the census instrument may use the Core Console only if the Coordinator rules it so and records it as a non-governing observation |
| R-10 | Who writes the `TOL_SCALE` test design (T-4) and when? H4 does not run before it | Architect writes the design; the implementer prepares nothing for H4 until then |
| R-11 | Release or Debug for the construction build and the exact build for the fixtures | Release (it removes the K-12 debug fault-injection variable from the question), unless the Coordinator wants a Debug build for traceability |
| R-12 | Mapping of the contract's FAIL/UNKNOWN/INVALID to non-governing preparation activities (section 4.0) | as in the 4.0 table |
| R-13 | Custody of the real `blocks-library.dwg` (a user asset outside the repository): which file, the copy rules, and whether the census record of a real library may enter the repository | the copy is private and hashed; the census record is not machine data and may be proposed for the repository by the Coordinator |

### 7.3 Open questions for the Architect (in addition to the rulings above)

- A-1. BA-10 V7 2.2 fixes the six attributes and the serialization but not the **exact text** each one represents (for example whether "AutoCAD product identity" is the product name, the product code or the `acad.exe` file version). The label hashes the exact text, so a different text gives a different label. BA-10 V8 should fix the read for each attribute, or H1 records the command and text as the authority.
- A-2. The Owner's Q-O5 list is wider than the closed list of six (hardware identity/class, Windows account, loaded-module baseline, configuration inputs), and "user/profile identity según el contrato" has no Windows-account field in V2.1 2.1 (the contract names the AutoCAD profile identity, `SECURELOAD`, `TRUSTEDPATHS` and the module entries). Are the extra fields recorded as `MACHINE_PROFILE_BOUND`/session facts without changing the class label (this package's reading), or does BA-10 V8 add them to the class definition?
- A-3. May the instance construction of the fixtures that are not `FX-IMP` wait for the exact build (BA-08 V7 16.2 K-1 says so), the gate then covering only H5 and H7 now? This package keeps H6 IN by the words "fixture instances", pending R-4.

## 8. Status

```text
HOST_GATE_PACKAGE                = PREPARED_FOR_COORDINATOR_REVIEW
HOST_GATE_BA11_3_3               = AUTHORIZED BY THE OWNER (NON_GOVERNING ONLY); NO MACHINE DESIGNATED; NO TUPLE RECORDED; NO COORDINATOR GATE RULING
HOST_RUN_STARTED                 = FALSE
ACTIVITIES IN (section 2)        = H1, H2, H4 (blocked by T-4), H5, H7; H6 IN as to scope with R-4; H3 RULING
ACTIVITIES OUT                   = governing, admission, G3, CT-50, Act 2, freeze, W-scan residual, final guarantee, WU-DRY-*, BA-06 dynamic trace,
                                   qualification of instruments, EV-L/EV-V, manifest/custody/anchor, UNDO evidence
ACTIVITIES NEEDING A RULING      = G5, G6, G9 (build), G11, G12, G20
TOOLING GAPS                     = T-1..T-9 (section 5); none written; none authorized
CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
```
