# I-52-AUTH15 host-validation harness (TEMPORARY, non-product)

Research instrument for the AutoCAD 2025 host validation of AUTH-15 (`RackDefinitionCreator.CreateInTransaction`).
It exists on the branch `feature/i52-auth15-definition-creator` only until the validation evidence is published, and
is **removed by a revert commit before Candidate**. Nothing here is product API, a normal command, UI or a ribbon
item; nothing in `src/`, `tests/`, `RackCad.sln` or `deploy/` references it.

## The binding rule

The harness commit leaves `src/` and `tests/` **byte-identical** to the implementation SHA
`a80a3801cc39eaa9656be05c080853be87ab2581`. `build-hostval.ps1` refuses to build otherwise
(`git diff --quiet <implementation> HEAD -- src tests`, plus equal `git rev-parse <sha>:src` / `:tests` trees, plus:
the harness commit may only touch `eng/research/I52Auth15Host/` and `docs/`; plus: no IGNORED source/config file
(`.cs .csproj .props .targets .ps1 .scr .json .config .sln`) may exist under `src`, `tests`, this folder or the root
build files outside `bin/obj`, because `git status` cannot see them and the build would still read them). The evidence
records `IMPLEMENTATION_SHA`, both trees of the implementation and of the harness commit, and `treesEqual`. That is what
binds a PASS to the implementation SHA and not to the harness.

## What is here

| File | Role |
|---|---|
| `I52Auth15.HostHarness.csproj` | net8.0-windows x64. AutoCAD references `Private=false`. References the exact `RackCad.Domain/Application/Plugin` built from the implementation content. Not in `RackCad.sln`. |
| `HostValidationCommand.cs` | The ONE command `I52AUTH15_HOSTVAL` (`CommandFlags.Session`), the bootstrap that loads the RackCad assemblies from the run folder, the FILEDIA gate, and cases HV-00..HV-14. |
| `Auth15Binding.cs` | Reflection seam onto the **internal** AUTH-15 surface. No `InternalsVisibleTo` exists anywhere. Fail-closed and exact: see "Binding". |
| `Fixtures.cs` | Harness-owned plans and envelopes, their fingerprints, and the small setup helpers. |
| `Snapshot.cs` | SNAP (see below). The handseed is not part of it. |
| `SysVarCatalog.cs` | The audited list of AutoCAD system variables the harness may read (`ACADVER`, `CPROFILE`, `FILEDIA`) and the known-invalid ones (`PROFILENAME`). Pure; the offline rig audits the source against it. |
| `Evidence.cs` | Case/assertion records, the strict `filedia.txt` parser and the evidence document (schema `I52-AUTH15-HV/1`), written atomically. Pure (no AutoCAD), so the offline rig reuses it. |
| `build-hostval.ps1` | Verifies the binding rule, builds, and writes `D:\I52-AUTH15-HV\<HARNESS_SHA8>\{run,out,launcher,logs}`, `SHA256SUMS`, `TRANSFER-METADATA.json` and a transfer zip. Never overwrites a package. Never starts AutoCAD. |
| `run-hostval.ps1` | Launches ONE AutoCAD process and verifies everything afterwards. See "Launcher". |
| `run.scr` | Template of the driver script (`{RUN}` = the versioned run folder, `{OUT_FWD}` = `out\` with forward slashes). |
| `offline/` | **Characterization rig only** (see "Offline tests"). Never part of a package, never host evidence. |

## How the product Plugin is (not) loaded

Only `I52Auth15.HostHarness.dll` is `NETLOAD`ed. `RackCad.Plugin.dll` is loaded **as a dependency**, from the same
versioned `run\` folder, by the bootstrap. No RackCad command, ribbon or `IExtensionApplication` is started. HV-00
proves that exactly one `RackCad.Plugin` / `RackCad.Application` / `RackCad.Domain` is loaded, from `run\`, and that
each one's SHA-256 equals BOTH `SHA256SUMS` and `TRANSFER-METADATA.json` (the harness DLL too).

### Binding

`Auth15Binding` asserts, and HV-00 records one assertion for each: the overload count is exactly 2; each is found by its
exact parameter types (compared as sequences, not by assignability) and they are two different static methods; both return
exactly the same type, the value type `RackDefinitionCreationResult` of the Plugin assembly; the parameter types the Plugin
uses are the harness's own Application types; the failure enum has exactly the eight normative values (no
`BlockNameUnavailable`); and every result member has its expected type. Anything else and the binding is not `Ready`, HV-00
fails, and no AUTH-15 call is made.

## System variables (HC-5 / HC-6)

RUN-1 was lost to `GetSystemVariable("PROFILENAME")`, which raises `eInvalidInput` in AutoCAD 2025; the current profile is
`CPROFILE`. Since then:

- **One reader.** `SysVar.Read` / `SysVar.ReadInt` are the only place `GetSystemVariable` is called. They read only names in
  `SysVarCatalog.Audited`, never throw, and return a value or an error text. A missing value is never replaced by a default.
- **Failure is data, and it is not a pass.** An unreadable `ACADVER` or `CPROFILE` is recorded with its error
  (`host.acadVersionReadError`, `host.profileReadError`) and makes HV-00 UNKNOWN; an unreadable live `FILEDIA` fails the FILEDIA gate.
- **A static audit** (offline test T23-T26) lists every literal passed to the harness's system-variable reads and requires it to equal
  the reviewed list, requires exactly one `GetSystemVariable` call site, and rejects known-invalid names. A new name cannot enter
  without a reviewer changing the catalog and the test. The audit was run against the RUN-1 source and rejects it.

## Early identity (HC-7)

Before HV-00, and before any fallible system-variable read, the harness persists what is immutable about this run: implementation
and harness SHAs, `treesEqual`, PID and process start time, run folder, harness DLL path and hash, the acad.exe path/version/hash,
the scratch document's SHA-256 (as the harness sees it and as the launcher declared it), the Owner's no-touch flag as declared by the
launcher, the FILEDIA record, and the **expected** package hashes (`package.expected{Plugin,Application,Domain,Harness}Sha256`).
Each item is recorded independently, so one failing does not lose the rest; a failure is listed in `host.earlyIdentityErrors` and
blocks a PASS.

**Loaded** assembly facts (`binding.loaded{Plugin,Application,Domain,Harness}{Path,Sha256}`) are HV-00 observations, written only when
observed, never fabricated earlier. The launcher requires expected == `SHA256SUMS` == `TRANSFER-METADATA` == loaded for each.

## Host model: DOCUMENT AUTHORITY and SIDE-DB CHARACTERIZATION

RUN-2 (the first run in which AUTH-15 really executed) found that everything created inside a caller's transaction survived the
caller's `Abort` on a harness-owned side database, and could not say why. A rollback claim is only meaningful on the condition the
product uses: a **document database under `LockDocument`**. So, from RUN-3 on:

- Every **rollback-sensitive** case (HV-01, 02, 03, 05, 08, 10, 12, 13, 14) runs on a **DOCUMENT** database and that run is the
  AUTHORITATIVE one (`dbKind = DOCUMENT-AUTHORITY`). A second document is opened for each case from a copy of `out\blank-template.dwg`
  (a copy of the Owner's blank drawing that the launcher never opens), locked, and closed with DISCARD afterwards. The anchor scratch
  drawing is never written and the launcher proves both files unchanged.
- The same body then runs again on a harness-owned side database (`dbKind = SIDE-DB-CHARACTERIZATION`). Those runs are kept in
  `sideCharacterizations`, never in `cases`, never in `leaks`, and never change the verdict. Nothing is silently replaced.
- The other cases (HV-00, 04, 06, 07, 09, 11) do not depend on the database kind and stay on side databases (`dbKind = SIDE-DB`);
  HV-04 needs `SaveAs`/reopen.
- If a document cannot be opened, locked or closed, the authority cases are UNKNOWN (never silently side-database), the failure is
  recorded in `documentAuthority`. Only a working database that was not restored to the anchor stops the run
  (`stopKind = exception`); a changed open-document count or a document that could not be closed is recorded as a Problem
  (it blocks a PASS) and the independent cases that follow still run.

Document authority is UNPROVEN on the host: RUN-3 will be the first time this code runs.

## Rollback controls (RB-xx)

Run right after HV-00 and before HV-01. **No AUTH-15 call** except where the name says so; each control snapshots, writes inside ONE caller
transaction, snapshots inside (sanity: something was written), ends the transaction (recorded, see below), snapshots again and
enumerates EVERY difference. A control's result is a **raw outcome**: it is never reinterpreted, and
its leaks are listed apart (`controlLeaks`). `dbKind` says which database it ran on.

**The controls GOVERN the verdict.** The expected set is exactly 14 records (id + `dbKind`): RB-01/SIDE-DB, RB-01V/SIDE-DB, RB-01V/DOCUMENT-AUTHORITY,
RB-01D/DOCUMENT-AUTHORITY and RB-02a, RB-02b, RB-02c, RB-03, RB-05 each on SIDE-DB and DOCUMENT-AUTHORITY. A **PASS** needs every one present exactly once and
PASS, no control leak, and `documentAuthority.available = true`; a missing, duplicated, wrong-`dbKind`, unexpected or UNKNOWN control (or unavailable document
authority) can never PASS (UNKNOWN); **any control FAIL makes the campaign FAIL**. A side record never stands in for a missing document one (the pairing is the
key), and the rollback-sensitive HV cases (HV-01, 02, 03, 05, 08, 10, 12, 13, 14) must be the DOCUMENT-AUTHORITY run: a SIDE-DB label, or the
`sideCharacterizations`, never satisfies them. `run-hostval.ps1` recomputes all of this on its own (`evidenceControlSet`, `evidenceRollbackCasesDocument`) and
refuses (INVALID) a PASS the controls do not back; a FAIL must be backed by a FAIL case, a FAIL control or a deviation.

| Id | Writes | Databases |
|---|---|---|
| RB-01 | one block definition + one layer + one entity + extension dictionary/Xrecord, then Abort | side |
| RB-01V | the same, but the transaction is only disposed (what a forgotten Commit does) | side, document |
| RB-01D | the RB-01 writes on a document database under `LockDocument` (the production condition) | document |
| RB-02a | `LateralHeaderDrawer.CreateSystemBlock` directly (no dimension/annotation), no AUTH-15 | side, document |
| RB-02b | the same with a dimension | side, document |
| RB-02c | `CantileverViewMaterializer.CreateBlockDefinitionNamed` directly | side, document |
| RB-03 | a hand-made definition + `RackBlockData.Write` | side, document |
| RB-05 | a dimension + `RecomputeDimensionBlock` only (native `Defpoints`/`*D` residue in isolation) | side, document |

How the outcomes are read (not encoded anywhere in the harness or in AUTH-15): RB-01 leaks but RB-01D is clean -> the side-database
model is the problem; both leak -> the caller-rollback premise is false in AutoCAD (Coordinator/Owner contract decision); RB-01 clean
but an RB-02 leaks -> that family creator path; all controls clean but AUTH-15 leaks -> investigate AUTH-15; only RB-05 leaks -> native
dimension residue isolated (still a FAIL for the HV cases until formally reclassified).

## Ending a transaction is never silent

`End` (abort), `EndDisposeOnly` and `EndAfterCommit` record, per transaction end: whether Abort and Dispose were attempted and
succeeded (with the exception type and message if not), `IsDisposed` before and after, the active-transaction count before and
after, and the identity of the top transaction after. A failed Abort or Dispose, or a transaction that is not disposed afterwards,
FAILS the current case. RUN-2's helper swallowed every exception, which is one of the causes that could not be excluded.

- **Disposed before the Abort.** On a path that expects the caller's Abort, a transaction that is already disposed BEFORE the Abort is a FAIL: there was
  nothing left to roll back, and it is never reinterpreted as "already rolled back". (A dispose-only control, which does not Abort, is not judged this way.)
- **Active-transaction deltas** (rollback-sensitive cases and controls only; the flag is cleared afterwards, so HV-06's deliberate `OpenCloseTransaction` is not
  judged): relative to the count read just before the end, the count must go down by exactly one, the top transaction afterwards must agree with the count (none
  when nothing is active, some when something is) and must not be the ended transaction. No absolute zero is required. A metric that cannot be read (count,
  top, or the ended transaction's identity) is a FAIL, never assumed good.

## Stopping, deviations and classification (`stopKind`)

A **NEW DEVIATION** (for example an HV-08 result that is not `WriteFailed` + clean rollback) is recorded in `deviations`, the case is
FAIL, and the run **continues**: the later cases are independent and stopping lost them in RUN-2. The run stops only when continuing
would make later evidence untrustworthy, and says why in `stopKind`:

| `stopKind` | Meaning | Launcher |
|---|---|---|
| `none` | ran to the end | normal |
| `deviation` | a case explicitly asked to stop (nothing does today) | VALID FAIL if the verdict is FAIL and a case beyond HV-00 ran |
| `hv00` | HV-00 was not PASS: no AUTH-15 call | INVALID |
| `filedia` | FILEDIA was not restored | INVALID |
| `exception` | the runner or the environment became untrustworthy | INVALID |

## SNAP

Sorted names **and handles** of: the block table (and the entity handles + extension-dictionary flag of every
definition), layers, dimension styles, text styles, **linetypes, registered applications, UCS, views and viewports**,
the named-objects dictionary keys, and the entity handles of model space and of every layout.

## The leak rule

After a caller's abort, **any** difference from the baseline is a leak and a **FAIL**: anonymous `*D` dimension blocks,
nested definitions, layers, dictionaries, table entries, anything. Nothing is normalized, excused or reclassified by the
harness. Every leaked key and handle is enumerated in the failing assertion, in the case's `leaks`, and in the
evidence's top-level `leaks`. A ruling on a leak belongs to the Architect, after the run, from that list.

## FILEDIA

`FILEDIA` is the Owner's preference and is normally 1. It is never assumed and never left changed:

1. `run.scr` reads the original into a LISP variable and writes `before=<n>` to `out\filedia.txt` **before** changing anything.
2. It sets `FILEDIA` to 0 and appends `during=<n>` (the value read back).
3. It runs `NETLOAD`.
4. It **immediately** restores the captured original value (`(setvar "FILEDIA" <variable>)`, not a constant) and appends `after=<n>`.
5. Only if `after == before` does it run `I52AUTH15_HOSTVAL`.

Before HV-00, the harness parses `filedia.txt` (strictly), requires `during == 0`, `after == before`, and the LIVE
`GetSystemVariable("FILEDIA") == before`. Otherwise `StoppedBy = "FILEDIA not restored: INVALID RUN"`, a `Problems` entry
is added and **no** HV case runs. At the end it re-reads the live value and requires it still equals `before`. The evidence
host section records `filediaBefore`, `filediaDuringNetload`, `filediaAfter`, `filediaLiveAtHarnessStart` and
`filediaLiveAtHarnessEnd`. The launcher parses `filedia.txt` itself, computes `filediaRestored` and cross-checks the
evidence.

**Recovery.** If a run is cut short (timeout, crash, a dialog), AutoCAD's `FILEDIA` may still be 0. The launcher prints the
captured original prominently ("FILEDIA RECOVERY") and it is also the `before=` line of `out\filedia.txt`. In a new AutoCAD
session, type `FILEDIA <original>`. **The launcher never writes `FILEDIA`, `SECURELOAD`, `TRUSTEDPATHS` or any other setting.**

## Launcher (`run-hostval.ps1`)

Refuses (exit 2, `RUN_RESULT = INVALID`, nothing launched) unless, in order: `-OwnerConfirmsNoTouch` is given; the package
matches `SHA256SUMS` exactly and `run\` holds nothing else; the metadata declares `treesEqual`; **no `acad.exe` is running**
(a running instance could swallow the launch); no RackCad autoload bundle exists; the scratch file is a `.dwg` with a DWG
signature (`AC10xx`); **the exact run folder is trusted** (read-only registry preflight of the profile's `TRUSTEDPATHS`,
honouring `SECURELOAD=0` and `...` recursive entries) - otherwise `ENVIRONMENT_PREREQUISITE = TRUSTEDPATH_REQUIRED`, or
`TRUSTEDPATH_UNDETERMINED` when the registry cannot be read (it does not guess); and `out\` holds no evidence or leftovers
from an earlier run.

It then launches one process, tracks its PID, enforces an external timeout (`taskkill /T /F`), and afterwards checks - each check
defensively, so an error inside a check is a failed check and never a crash - the clean exit (code 0, process gone, no
second `acad.exe`), the scratch hash, package drift, FILEDIA, and the evidence: it exists, **parses**, has the schema, comes
from this PID, is bound to `SHA256SUMS`, and its Plugin / Application / Domain / harness hashes, `harnessSha`, `implementationSha`,
the four tree hashes and the FILEDIA fields agree with `SHA256SUMS` and `TRANSFER-METADATA.json`, and it is `final` and `completed`.
`launcher-record.json` is **always** written. It also ties the evidence to THIS launch even when HV-00 never completed: the evidence must
carry this PID, a process start time within 2 s of the OS's, the launcher-declared scratch hash equal to the file's hash, and the
Owner no-touch declaration. Finally it does not take the harness's word for PASS: a PASS verdict must be backed by 15 PASS cases, no
problems, no leaks and no deviation in the evidence itself.

| Exit | `RUN_RESULT` | Meaning |
|---|---|---|
| 0 | `PASS` | valid launch AND evidence verdict PASS |
| 2 | `INVALID` | refusal, or any launch/binding/package/evidence/process condition failed (including a non-zero AutoCAD exit **even if the evidence says PASS**) |
| 3 | `FAIL` / `UNKNOWN` | the launch was valid but the harness verdict is FAIL or UNKNOWN. A run that did not finish is still a valid FAIL when the evidence is `final`, the verdict is FAIL, `stopKind = deviation`, and a case beyond HV-00 ran (RUN-2's shape). |

`RUN_RESULT = ...` is always the last line printed. A run is never repeated automatically.

## Evidence

`out\hostval-evidence.json` (schema `I52-AUTH15-HV/1`), `out\hostval.log`, `out\filedia.txt`, `out\run.scr` and
`out\launcher-record.json`. The evidence is replaced **atomically after every completed case** (temp file + replace,
`state = in-progress`), so a crash cannot erase the cases already finished; the final write sets `state = final`. The
publishable copy goes to `docs/automation/evidence/` and survives the harness, which is reverted before Candidate.

## Prerequisites (each failure is a STOP, not a workaround)

1. **No `acad.exe` running at launch.** (Building while AutoCAD runs is allowed: the build never touches it, and `TRANSFER-METADATA.json` records `acadRunningDuringBuild`.)
2. The Owner has added the package's `run\` folder to `TRUSTEDPATHS`. Neither script touches `SECURELOAD` or `TRUSTEDPATHS`.
3. No RackCad bundle autoloads (`%APPDATA%` / `%ProgramData%` `Autodesk\ApplicationPlugins\*RackCad*`).
4. A fresh blank `.dwg` for `-ScratchDrawing`.
5. The Owner does not touch the computer from launch until the process exits (`-OwnerConfirmsNoTouch`).
6. One run. No retry, no patching, no hand-editing of `run\`.

```powershell
pwsh eng\research\I52Auth15Host\build-hostval.ps1 -HarnessSha <40-hex harness commit>
pwsh D:\I52-AUTH15-HV\<sha8>\launcher\run-hostval.ps1 -Package D:\I52-AUTH15-HV\<sha8> -ScratchDrawing <fresh.dwg> -OwnerConfirmsNoTouch
```

The scratch document is never modified, so `QUIT` does not prompt (if it were, `_Y` would save into the scratch COPY in `out\`,
and the launcher would report the changed hash).

## Cases (HV-00 .. HV-14)

HV-00 bind, hashes and the B-1 fact; HV-01 HeaderRun success; HV-02 Cantilever success; HV-03 rollback; HV-04 commit, `SaveAs`, reopen;
HV-05 name collision; HV-06 `TransactionMismatch` (a null transaction, b foreign database, c outer while nested is top,
d disposed transaction, e disposed database); HV-07 `InvalidPlan`; HV-08 `InvalidBlockName` and the HeaderRun `"<>"`
characterization; HV-09 `InvalidEnvelope`; HV-10 `MissingLibraryBlocks`; HV-11 immutability; HV-12 no batch memory;
HV-13 no placement; HV-14 no internal commit.

Recorded but **not** counted in the verdict: `null database` and `OpenCloseTransaction` (both expected
`TransactionMismatch`); each carries a machine-readable `matchesExpectation`. `EnvelopeWriteFailed` and an `InvalidEnvelope`
from a serialization failure are declared `NOT_EXERCISABLE_WITHOUT_FAULT_INJECTION` and never given a PASS; `WriteFailed` is
exercised by the HV-08 `"<>"` characterization.

## PASS / FAIL

- **PASS** = HV-00..HV-14 all PASS, 0 FAIL, 0 UNKNOWN, `completed`, no `problems`, `treesEqual = true`, binding, hash and
  FILEDIA checks passed, the process exited by script (exit 0, no timeout, no second `acad.exe`), and the Owner confirms
  no interaction. A UNKNOWN (binding, structure, reflection) is not PASS. Any leak is a FAIL.
- A **NEW DEVIATION** (a HV-08 characterization that does not come out as `WriteFailed`) is recorded, makes the case FAIL and needs an Architect
  ruling. HV-00 not PASS, or FILEDIA not restored, stops the run before any AUTH-15 call.
- Human interaction = INVALID RUN; a rerun needs authorization. A non-zero AutoCAD exit with complete PASS evidence is INVALID
  pending an Architect ruling; the launcher never reruns.

## Offline tests (`offline/`) - NOT host evidence

`offline/run-offline-tests.ps1` characterizes the launcher and the FILEDIA logic **without AutoCAD**. It builds `offlineacad.exe`
(a stand-in that runs the REAL `run.scr` through a mini script/LISP interpreter and plays the harness with the real
`FilediaRecord` and `EvidenceDoc`), builds synthetic packages, points the launcher at a private test registry key
(`-AutoCadRegistryRoot`; only keys created by the test are written or removed), and asserts exit codes, records, refusals,
timeout and FILEDIA behaviour, plus the system-variable audit and the RUN-1 reproduction (the mock rejects `PROFILENAME` with
`eInvalidInput`, and the audit rejects the RUN-1 source). What it cannot show: that real AutoCAD parses `run.scr` the way the mini interpreter does, or
anything about AUTH-15. It is never copied into a package, and nothing it produces may be cited as host validation.

## Removal

`git revert` of the harness commits (or `git rm -r eng/research/I52Auth15Host`). `src/` and `tests/` are unaffected by
construction, so the evidence stays bound to the implementation SHA.
