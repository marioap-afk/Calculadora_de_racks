# I-52-AUTH15 host-validation harness (TEMPORARY, non-product)

Research instrument for the AutoCAD 2025 host validation of AUTH-15 (`RackDefinitionCreator.CreateInTransaction`).
It exists on the branch `feature/i52-auth15-definition-creator` only until the validation evidence is published, and
is **removed by a revert commit before Candidate**. Nothing here is product API, a normal command, UI or a ribbon
item; nothing in `src/`, `tests/`, `RackCad.sln` or `deploy/` references it.

## The binding rule

The harness commit leaves `src/` and `tests/` **byte-identical** to the implementation SHA
`2d10de705fffee3dc03473c2d4c76bff489fc13e`. `build-hostval.ps1` refuses to build otherwise
(`git diff --quiet <implementation> HEAD -- src tests`, plus equal `git rev-parse <sha>:src` / `:tests` trees, plus:
the harness commit may only touch `eng/research/I52Auth15Host/` and `docs/`). The evidence records
`IMPLEMENTATION_SHA`, both trees of the implementation and of the harness commit, and `treesEqual`. That is what binds a
PASS to the implementation SHA and not to the harness.

## What is here

| File | Role |
|---|---|
| `I52Auth15.HostHarness.csproj` | net8.0-windows x64. AutoCAD references `Private=false`. References the exact `RackCad.Domain/Application/Plugin` built from the implementation content. Not in `RackCad.sln`. |
| `HostValidationCommand.cs` | The ONE command `I52AUTH15_HOSTVAL` (`CommandFlags.Session`), the bootstrap that loads the RackCad assemblies from the run folder, and cases HV-00..HV-14. |
| `Auth15Binding.cs` | Reflection seam onto the **internal** AUTH-15 surface. No `InternalsVisibleTo` exists anywhere. |
| `Fixtures.cs` | Harness-owned plans and envelopes, their fingerprints, and the small setup helpers. |
| `Snapshot.cs` | SNAP: block table (+ contents), layers, dimension styles, text styles, NOD keys, model space and every layout's entity handles. The handseed is not part of it. |
| `Evidence.cs` | Case/assertion records and the evidence document (schema `I52-AUTH15-HV/1`). |
| `build-hostval.ps1` | Verifies the binding rule, builds, and writes `D:\I52-AUTH15-HV\<HARNESS_SHA8>\{run,out,launcher,logs}`, `SHA256SUMS`, `TRANSFER-METADATA.json` and a transfer zip. Never overwrites a package. Never starts AutoCAD. |
| `run-hostval.ps1` | Launches ONE AutoCAD process, enforces an external timeout, tracks the PID, and verifies the exit, the package and the evidence. |
| `run.scr` | Template of the driver script (`{RUN}` = the versioned run folder). |

## How the product Plugin is (not) loaded

Only `I52Auth15.HostHarness.dll` is `NETLOAD`ed. `RackCad.Plugin.dll` is loaded **as a dependency**, from the same
versioned `run\` folder, by the bootstrap. No RackCad command, ribbon or `IExtensionApplication` is started. HV-00
proves that exactly one `RackCad.Plugin` / `RackCad.Application` / `RackCad.Domain` is loaded, from `run\`, byte-identical
to `SHA256SUMS`.

## Host model

Each case works on a **harness-owned side database** (`new Database(true, true)`), with
`HostApplicationServices.WorkingDatabase` pointed at it for the case and restored afterwards. A scratch `.dwg` (a
copy of a blank drawing the Owner supplies) is opened only so that `WorkingDatabase` has a document to return to; it is
never written and the launcher proves its hash is unchanged. The caller of AUTH-15 in every case is the harness, which
opens, aborts or commits the transaction itself and takes SNAP before and after.

## Prerequisites (each failure is a STOP, not a workaround)

1. AutoCAD 2025 closed while building; **no `acad.exe`** running at launch.
2. The Owner has added the package's `run\` folder to `TRUSTEDPATHS`. Neither script touches `SECURELOAD` or
   `TRUSTEDPATHS`. Without it the harness never loads: `ENVIRONMENT_PREREQUISITE = TRUSTEDPATH_REQUIRED`.
3. No RackCad bundle autoloads (`%APPDATA%` / `%ProgramData%` `Autodesk\ApplicationPlugins\*RackCad*`): the launcher
   refuses with `ENVIRONMENT_PREREQUISITE = RACKCAD_AUTOLOAD_PRESENT`.
4. A fresh blank `.dwg` for `-ScratchDrawing`.
5. The Owner does not touch the computer from launch until the process exits (`-OwnerConfirmsNoTouch`).
6. One run. No retry, no patching, no hand-editing of `run\`.

```powershell
pwsh eng\research\I52Auth15Host\build-hostval.ps1 -HarnessSha <40-hex harness commit>
pwsh D:\I52-AUTH15-HV\<sha8>\launcher\run-hostval.ps1 -Package D:\I52-AUTH15-HV\<sha8> -ScratchDrawing <fresh.dwg> -OwnerConfirmsNoTouch
```

`run.scr` sets `FILEDIA` to 0 around the `NETLOAD` (as the I52Ctda driver does, so no file dialog can hang an unattended run)
and back to 1 before `QUIT`. The scratch document is never modified, so `QUIT` does not prompt.

## Cases (HV-00 .. HV-14)

HV-00 bind and B-1 fact; HV-01 HeaderRun success; HV-02 Cantilever success; HV-03 rollback; HV-04 commit, `SaveAs`, reopen;
HV-05 name collision; HV-06 `TransactionMismatch` (a null transaction, b foreign database, c outer while nested is top,
d disposed transaction, e disposed database); HV-07 `InvalidPlan`; HV-08 `InvalidBlockName` and the HeaderRun `"<>"`
characterization; HV-09 `InvalidEnvelope`; HV-10 `MissingLibraryBlocks`; HV-11 immutability; HV-12 no batch memory;
HV-13 no placement; HV-14 no internal commit.

Recorded but **not** counted in the verdict: `null database` and `OpenCloseTransaction` (both expected
`TransactionMismatch`). `EnvelopeWriteFailed` and an `InvalidEnvelope` from a serialization failure are declared
`NOT_EXERCISABLE_WITHOUT_FAULT_INJECTION` and never given a PASS; `WriteFailed` is exercised by the HV-08 `"<>"`
characterization.

## PASS / FAIL

- **PASS** = HV-00..HV-14 all PASS, 0 FAIL, 0 UNKNOWN, `completed`, no `problems`, `treesEqual = true`, binding and
  hash checks passed, the process exited by script (exit 0, no timeout, no second `acad.exe`), and the Owner confirms
  no interaction. A UNKNOWN (binding, structure, reflection) is not PASS.
- A **NEW DEVIATION** (including a HV-08 characterization that does not come out as `WriteFailed` + clean rollback) stops
  the run and needs an Architect ruling. HV-00 not PASS stops the run before any AUTH-15 call.
- Human interaction = INVALID RUN; a rerun needs authorization.

## Evidence

`out\hostval-evidence.json` (schema `I52-AUTH15-HV/1`), `out\hostval.log` (harness log) and `out\launcher-record.json`
(launcher record: PID, times, exit code, timeout, scratch hashes, package checks). The publishable copy goes to
`docs/automation/evidence/` and survives the harness, which is reverted before Candidate.

## Removal

`git revert` of the harness commit (or `git rm -r eng/research/I52Auth15Host`). `src/` and `tests/` are unaffected by
construction, so the evidence stays bound to the implementation SHA.
