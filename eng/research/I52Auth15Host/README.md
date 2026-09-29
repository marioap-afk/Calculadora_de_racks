# I-52-AUTH15 host-validation harness (TEMPORARY, non-product)

Research instrument for the AutoCAD 2025 host validation of AUTH-15 (`RackDefinitionCreator.CreateInTransaction`).
It exists on the branch `feature/i52-auth15-definition-creator` only until the validation evidence is published, and
is **removed by a revert commit before Candidate**. Nothing here is product API, a normal command, UI or a ribbon
item; nothing in `src/`, `tests/`, `RackCad.sln` or `deploy/` references it.

## The binding rule

The harness commit leaves `src/` and `tests/` **byte-identical** to the implementation SHA
`2d10de705fffee3dc03473c2d4c76bff489fc13e`. `build-hostval.ps1` refuses to build otherwise
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

## Host model

Each case works on a **harness-owned side database** (`new Database(true, true)`), with
`HostApplicationServices.WorkingDatabase` pointed at it for the case and restored afterwards. A scratch `.dwg` (a
copy of a blank drawing the Owner supplies) is opened only so that `WorkingDatabase` has a document to return to; it is
never written and the launcher proves its hash is unchanged. The caller of AUTH-15 in every case is the harness, which
opens, aborts or commits the transaction itself and takes SNAP before and after.

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
`launcher-record.json` is **always** written.

| Exit | `RUN_RESULT` | Meaning |
|---|---|---|
| 0 | `PASS` | valid launch AND evidence verdict PASS |
| 2 | `INVALID` | refusal, or any launch/binding/package/evidence/process condition failed (including a non-zero AutoCAD exit **even if the evidence says PASS**) |
| 3 | `FAIL` / `UNKNOWN` | the launch was valid but the harness verdict is FAIL or UNKNOWN |

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
- A **NEW DEVIATION** (a HV-08 characterization that does not come out as `WriteFailed`) stops the run and needs an Architect
  ruling. HV-00 not PASS, or FILEDIA not restored, stops the run before any AUTH-15 call.
- Human interaction = INVALID RUN; a rerun needs authorization. A non-zero AutoCAD exit with complete PASS evidence is INVALID
  pending an Architect ruling; the launcher never reruns.

## Offline tests (`offline/`) - NOT host evidence

`offline/run-offline-tests.ps1` characterizes the launcher and the FILEDIA logic **without AutoCAD**. It builds `offlineacad.exe`
(a stand-in that runs the REAL `run.scr` through a mini script/LISP interpreter and plays the harness with the real
`FilediaRecord` and `EvidenceDoc`), builds synthetic packages, points the launcher at a private test registry key
(`-AutoCadRegistryRoot`; only keys created by the test are written or removed), and asserts exit codes, records, refusals,
timeout and FILEDIA behaviour. What it cannot show: that real AutoCAD parses `run.scr` the way the mini interpreter does, or
anything about AUTH-15. It is never copied into a package, and nothing it produces may be cited as host validation.

## Removal

`git revert` of the harness commits (or `git rm -r eng/research/I52Auth15Host`). `src/` and `tests/` are unaffected by
construction, so the evidence stays bound to the implementation SHA.
