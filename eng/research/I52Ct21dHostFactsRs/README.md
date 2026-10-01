# I-52 CT-21D host-gate instruments, RS class (`I52Ct21dHostFactsRs`)

Research-only implementation of the **RS (WRITES-SIDE) instruments** of the CT-21D host-fact matrix: the dynamic-property census (I-3), the dimension
write-back probe (I-5, HF-C4 / HF-C5 with the OP2 save of its side database) and the TOL_SCALE probe (I-6, HF-T1). It is a SIBLING of
`eng/research/I52Ct21dHostFacts` (the R0 class) and reuses that folder **unchanged** (the R0 core library, the R0 scan engine, the evidence writer, the
schemas): no file of the R0 folder, of `src/`, of `tests/`, of any other `eng/` folder, of a solution, of CI or of `deploy/` was edited. Nothing in this
directory is referenced by a product project, by `RackCad.sln`, by `deploy/` or by CI.

```text
NOT LOADED. NOT EXECUTED ON ANY HOST. No AutoCAD process was started to write, build or test any of this. Building compiles against the installed
managed assemblies only (AcDbMgd, AcCoreMgd, AcMgd, Private="false": nothing is copied, no AutoCAD file is locked).
HOST_RUN_STARTED = FALSE     CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
HF-G2 = NOT_OPENED (decisions section 242 (c))     I-7 / I-8 = NOT IMPLEMENTED
```

## The gate

| Source | Content |
|---|---|
| Owner, `docs/automation/evidence/I-52-ct21d-owner-decisions-q-o-1.md` (verbatim) | `Q_O_1 = ALSO_SIDE_DB_WRITES_AND_NEW_FILES`: `READ_ONLY_PRODUCT_STATE` is preserved; allowed = disposable control-plane side/scratch databases used only by the instrument, NEW evidence/result files under one fixed allowlisted root; NOT allowed = modifying or saving the governed document, modifying the original library DWG, writing through product seams, writing in `src/`, implementing RACKMIRROR, overwriting arbitrary files, writing outside the allowlisted paths, indirect changes to the governed document through a side database, undeclared persistent state between runs, turning a non-governing run into a governing one |
| Coordinator, decisions section 242 | opens U-RS-1 (I-3, HF-C2), U-RS-2 (I-5, HF-C4/HF-C5 with the OP2 save), U-RS-3 (I-6, the 147-row probe). Reading (a): writes only to a database that the instrument creates itself with `new Database(...)`, never `WorkingDatabase`, never the active document, never a database opened by the product; saves that side database only as a NEW file under the declared scratch root, without overwrite. (b): the library DWG is only read, from a PRIVATE COPY with hashes. (c): HF-G2 is not opened. (d) PRE-1..PRE-4 and R-M condition the H4 run and the decision of the value, not the instrument |
| Architect, `docs/initiatives/I-52-ct21d-tol-scale-design-and-host-fact-matrix-v1.md` | section 2 (the TOL_SCALE test, probe table, `tolscale-raw` schema, state machine 2.5), section 3.4 (HF-C2, HF-C4, HF-C5, HF-T1), section 5 (instrument specifications) |

Pipeline of each unit (Owner): IMPLEMENT -> tests without AutoCAD -> prohibited-API analysis -> **independent source review** -> hashes -> Architect
review -> Coordinator ruling. This folder is the first step only (plus the analysis). The automatic analysis is **not** authority: the final authority is
the independent human source review plus the recorded hashes (`HASHES-RS.txt`, the list hashes below). Any byte change after the review invalidates the approval.

## What is implemented

| Unit / id | Fact | Command | Records written (new files under the evidence root) |
|---|---|---|---|
| U-RS-1 / I-3 | HF-C2: per dynamic definition, the properties a reference exposes (name, read-only, host value type, units type, value set, duplicate names) | `CT21DHG_CENSUS_DYN` | `dyncensus-raw.json` (`ct21d.dyncensus.v1`), `hostfact-HF-C2.json`, `rs-run-dyncensus.json` |
| U-RS-2 / I-5 | HF-C4 (storage, order and equal-to-style behaviour of the 8 product overrides) and HF-C5 (group code -> variable, read only from the stored pairs of the probe); OP2 = save of the side database as a NEW scratch file, reopen, re-read | `CT21DHG_DIMWRITEBACK` | `writeback-raw.json` (`ct21d.writeback.v1`), `hostfact-HF-C4.json`, `hostfact-HF-C5.json`, `rs-run-writeback.json` + one scratch DWG |
| U-RS-3 / I-6 | HF-T1: 147 rows (132 PF-1 + 15 PF-2; 135 in scope), OP1 (re-read after the commit) and OP2 (save, reopen, re-read), witness, statistics, state machine PASS / FAIL / UNKNOWN / INVALID, single execution | `CT21DHG_TOLSCALE` | `tolscale-raw.json` (`ct21d.tolscale.v1`, the design schema verbatim), `tolscale-probe-table.json`, `rs-run-tolscale.json` + one scratch DWG |

The command names are the ones of the design (5.2). The three commands are plain `[CommandMethod]` (no `Session` flag, no document lock, no document
access other than the command-line status line), and each runs **once per session**: a second typing is refused before any side effect (no automatic
retry; PARAM-04 = 3 is a ceiling per slot, not a permission). A refusal (nothing was touched) is not an execution.

### Layout

```text
core-rs/     pure library (net8.0, no AutoCAD reference): designation, path arithmetic, scratch-root guard, ledgers, verifier, TOL_SCALE state machine / record /
             offline verifier, census and write-back logic, runners over thin host interfaces
rs/          the AutoCAD-facing DLL (net8.0-windows, x64): SideDbWriter, SideDbReader, ScratchSaver, SideDbHandle, host adapters, the commands
tools-rs/    offline tools (net8.0 console, writes nothing): scan-rs, imports-list-rs, privileged-list-rs, scratch-verify, tolscale-verify, ledger-verify, hash
tests/       xUnit, no AutoCAD
schemas/     the 6 schemas of the RS records (the design schema of tolscale-raw verbatim)
allowed-apis-rs.txt  rs-waivers.txt  privileged-surface-rs.txt  privileged-callers-rs.txt  expected-commands-rs.txt   the five checked-in lists of the RS scan
HASHES-RS.txt        SHA-256 of every source file of this folder (`.gitattributes` here turns the end-of-line conversion off, so the bytes hashed are the bytes checked out) (and, as comments, the DLLs as built); verified with `tools-rs ledger-verify`
```

The RS DLL depends on `I52Ct21d.HostFacts.Core.dll` (R0 core) and `I52Ct21d.HostFacts.Rs.Core.dll`: the declared set must include both, with their `.pin`
sidecars; the self-pin of all three is checked at command start (a mismatch makes the run refuse). `run-designation-rs.json` (`ct21d.designation.rs.v1`) sits next to the DLL.

## The RS write rules, as built

* **ONE type writes: `SideDbWriter`.** Every `new Database(...)`, every `ReadDwgFile`, every object creation, `AppendEntity`, property setter, `Commit` and
  `OpenMode.ForWrite` of the RS DLL is inside it, and only on a `Database` it created itself with `new Database(true, true)` (scratch) or
  `new Database(false, true)` (filled by `ReadDwgFile` from a source the guard authorized). `noDocument` is always `true`: the database is never attached to
  a document. It never touches `HostApplicationServices.WorkingDatabase`, a document's database, a document manager call or a system-variable write.
* **ONE type saves: `ScratchSaver`.** The single `Database.SaveAs`, of a `SideDbHandle` (so only a database the instrument created) to a `ScratchTarget`:
  a flat file name under the declared scratch root that `ScratchRootGuard.AuthorizeNew` authorized (absolute, resolved, `CT21D_<id>.dwg`, absent in any
  letter case, at most 8 per run, no reparse point in the root or its ancestors), re-validated by `AssertCreatable` right before the save; the SHA-256 is
  recorded in the guard's ledger right after. Create-new semantics come from the guard (an existing target is refused). It deletes nothing.
* **`SideDbReader`** is the read-only view: constant `OpenMode.ForRead`, `OpenCloseTransaction` objects that are only disposed.
* **Result files** go through the existing single `EvidenceWriter` of the R0 core (create-new, flat names, SHA-256 verified by reading back).
* **Cleanup contract.** Side databases are disposed (the run record lists every one with constructor, fill, source and `disposed`); every scratch file is
  declared in the run record with its SHA-256; the scratch root must be empty (or hold only files declared by earlier `rs-run-*.json` records of the same
  evidence root) at the start, and is listed again at the end by `ScratchVerifier`: any undeclared entry (any depth, hidden files included), missing file, changed hash or reparse
  point makes the run **INVALID**. `tools-rs scratch-verify <scratchRoot> <evidenceRoot>` repeats the check offline (exit 2 when not clean). Scratch files are
  not deleted by any instrument; they stay declared until the CAD manager removes them after the custody step. Use a fresh (or emptied) scratch root for each evidence root:
  files declared by the records of ANOTHER evidence root are undeclared here and refuse the run.
* **Refusals before any side effect:** self-pin mismatch, overlapping or non-absolute paths (evidence root, scratch root, library, private copy, governed
  document, declared product paths, any root with a `src` segment, `\\?\` / `..` / alternate-stream markers), a private copy or library whose hash differs from the
  designation, a scratch root that is not clean at the start, an existing output file, `otherAcadProcess` or a manual load route declared by the CAD manager,
  a writer on another folder, a non-H4 run id for TOL_SCALE.
* **Run-time INVALID reasons:** `DBMOD`, `SECURELOAD`, `TRUSTEDPATHS`, `CPROFILE` or the document identity changed during the run; the private copy or the library changed;
  a side database not disposed; the scratch verification failed; an exception inside the instrument; for TOL_SCALE also the offline recomputation mismatch.

## The prohibited-API analysis for RS (`tools-rs scan-rs`)

It reuses the R0 scan **unchanged** (`ForbiddenApiScan`, referenced as a library) and adds, in `tools-rs/`, only configuration and one extra layer:

1. **Layer 1, deny-by-default imports: `allowed-apis-rs.txt`** (814 entries; same engine and format as the R0 `allowed-apis.txt`). The write members of the host
   are approved only with a scope: `Database::.ctor`, `ReadDwgFile`, `CloseInput`, `WblockCloneObjects`, object creation, `Add`, `AppendEntity`,
   `AddNewlyCreatedDBObject`, the property setters, `Commit`, `RecomputeDimensionBlock` -> **SideDbWriter only**; `Database::SaveAs` -> **ScratchSaver only**; `SideDbReader` has
   getters, enumerators, `GetObject`, the transaction manager. The only document-related members are the command-line status line of `RsGateCommands`. No `System.IO` write member,
   process, registry, network, assembly-load or reflection-invoke member is approved anywhere (tests assert it).
2. **Layer 2, the R0 deny rules**, run unchanged with the side-database type set to `SideDbWriter` (`R-PRODUCT-DATABASE` for `WorkingDatabase` and a document's database,
   `R-COMMAND` for document-manager calls, `R-SYSVAR-SET`, `R-OVERRULE`, `R-FILE-WRITE`, `R-PROCESS`, `R-ASSEMBLY-LOAD`, `R-OPENMODE-UNRESOLVABLE`, raw memory, ...).
   The few findings the RS DLL must trip (database creation, object creation, setters, `Commit`, `ForWrite`, `WblockCloneObjects`, `SaveAs`, the reader's transaction) are
   **waived by `rs-waivers.txt`**, one line per (rule, type, member). The maximum a waiver file can do is fixed **in the source of the scan** (`RsWaivers.Waivable`): a line outside it
   is a format error, so editing the file cannot widen the privilege; `ForNotify` and `UpgradeOpen` can never be waived; an unused waiver is reported.
3. **Layer 3, the pinned surface** (same mechanisms as R0): `privileged-surface-rs.txt` pins the exact method set of `SideDbWriter`, `SideDbReader`, `ScratchSaver`, `SideDbHandle`;
   `privileged-callers-rs.txt` lists the only callers of those types and of the evidence writer (named methods, no lambdas); `expected-commands-rs.txt` pins the 3 commands and the command class.
4. **The RS layer (`RsLayer`), on the IL of the RS assembly:** `R-RS-DB-CTOR` (a database is built only in `SideDbWriter`, constructor `(bool, bool)`, constant arguments, `noDocument = true`),
   `R-RS-SAVE-SCOPE` (`SaveAs` only in `ScratchSaver`, only `(string, DwgVersion)`, constant version, the path argument is the result of `ScratchTarget.get_FullPath`, preceded in straight
   line by `ScratchRootGuard.AssertCreatable`), `R-RS-READDWG-SCOPE` (`ReadDwgFile` only in `SideDbWriter`, the path is the result of `ReadableSource.get_FullPath`, preceded in straight line
   by `AssertReadable`), `R-RS-SURFACE-AUTODESK` (no Autodesk type in a non-private signature of the privileged types, `SideDbHandle` excepted), `R-RS-PRIVILEGED-SURFACE` and `R-RS-PRIVILEGED-CALLER`.

```powershell
$dotnet = "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe"
$t = "eng/research/I52Ct21dHostFactsRs"
& $dotnet $t/tools-rs/bin/Release/net8.0/I52Ct21d.HostFacts.Rs.Tools.dll scan-rs --allowed $t/allowed-apis-rs.txt --waivers $t/rs-waivers.txt `
    --privileged-surface $t/privileged-surface-rs.txt --privileged-callers $t/privileged-callers-rs.txt --expected-commands $t/expected-commands-rs.txt `
    core:$t/core-rs/bin/Release/net8.0/I52Ct21d.HostFacts.Rs.Core.dll tools:$t/tools-rs/bin/Release/net8.0/I52Ct21d.HostFacts.Rs.Tools.dll `
    rs:$t/rs/bin/Release/net8.0-windows/I52Ct21d.HostFacts.Rs.dll        # exit 0 clean, 2 violations, 1 usage / malformed list
```

The report prints the SHA-256 of the five lists; the CAD manager compares them with `HASHES-RS.txt`. The R0 scan (`tools scan` with the R0 lists) must stay CLEAN too: it is the
control of the evidence writer the RS runners call.

**Negative controls** (`tests/RsScanTests.cs`, 164 fixtures x 3 checks): IL fixtures named like the RS assembly and types, each with one violation, must make the scan FAIL with the expected rule,
once with every import approved (deny rules + RS layer alone) and once with the real list. They include: `WorkingDatabase` get/set and `MdiActiveDocument.Database` in a write; `SaveAs` in `SideDbWriter`,
in the adapter, in the reader, without the guard call, with a wrong guard method, to a path that is not `ScratchTarget.FullPath` (including a `ReadableSource` path), with a non-constant version, with the
`SecurityParameters` overloads, after a branch or a `ret`; `Save`, `DxfOut`, `DwgOut`; `Database` constructors outside the factory and in the factory with `(false,false)`, `(true,false)`, non-constant,
parameterless and `(IntPtr,bool)` arguments; `DocumentManager.Open/Add/CloseAll`, `set_MdiActiveDocument`, `DocumentCollectionExtension.Open`, `CloseAndDiscard`, `LockDocument`, `Editor.Command`,
`SendStringToExecute`; `Wblock`, `Insert`, `DeepCloneObjects`, `WblockCloneObjects` outside `SideDbWriter` and `Wblock/Insert/DeepClone` inside it; `AttachXref`, `BindXrefs`, `SwapIdWith`; `ReadDwgFile`
outside `SideDbWriter`, without the guard, from another path, of the `IntPtr` overload; `ForNotify`, `UpgradeOpen`, `ForWrite` in the reader / saver / adapter, non-constant modes; `Commit`,
`AppendEntity`, setters and object creation outside `SideDbWriter`; `SetSystemVariable`, `Overrule`, `DynamicLinker`, events; 15 file members (`File.Copy` over the library, `Move`, `Delete`,
`WriteAll*`, `AppendAllText`, `Replace`, `Create`, `OpenWrite`, `SetAttributes`, `Encrypt`, `Directory.Delete/CreateDirectory/Move`, `Path.GetTempFileName`) in each of 4 types, `FileStream`, `StreamWriter`,
`BinaryWriter`; `Process.Start/Kill/GetProcessesByName`, `ProcessStartInfo`, `Assembly.Load/LoadFrom`, `Activator`, `MethodInfo.Invoke`, `Registry.SetValue`, `Environment.SetEnvironmentVariable`, `HttpClient`,
`Marshal.Copy`, `calli`, `localloc`, `stind`. Positive controls (a fixture that follows every rule is CLEAN with the real list and with a permissive one), spoof controls (the same type names in another
assembly or namespace get no privilege), list-mutation controls and **mutation controls on the real RS DLL** (a `ForRead` constant patched to `ForWrite`; the `noDocument` argument patched to
`false`; one write approval removed; the `SaveAs` waiver removed; a caller, a surface line or a command removed or added) complete them.

## Build and test WITHOUT AutoCAD

```powershell
$dotnet = "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe"      # the `dotnet` on PATH does not resolve global.json
& $dotnet test eng/research/I52Ct21dHostFactsRs/tests/I52Ct21d.HostFacts.Rs.Tests.csproj -c Release
& $dotnet test eng/research/I52Ct21dHostFactsRs/tests/I52Ct21d.HostFacts.Rs.Tests.csproj -c Debug
& $dotnet test eng/research/I52Ct21dHostFacts/tests/I52Ct21d.HostFacts.Tests.csproj -c Release     # the R0 suite must still pass
```

`rs/` only **compiles** against `AcCoreMgd.dll`, `AcDbMgd.dll`, `AcMgd.dll` of an installation (`AutoCADInstallDir`, default `C:\Program Files\Autodesk\AutoCAD 2025`, `Private="false"`). When
they are installed the test project builds `rs/` and the tests that read the real DLL are compiled in (`HAVE_RS`); without them those tests are compiled out and the reviewer scans the DLL with `tools-rs`.
`bin/` and `obj/` are ignored by the repository `.gitignore`. The lists were drafted with `imports-list-rs` and `privileged-list-rs` (review aids that approve nothing) and then reviewed line by line by the
implementer; the independent review has not been replaced by them.

What the tests cover (all without AutoCAD): path arithmetic and the designation rules (every overlap class); the scratch-root guard (names, create-new, limits, foreign targets, hash re-checks, junctions);
the verifier (undeclared, hidden, nested, missing, changed, case); the 147-row table and the row arithmetic (Sterbenz, witness bound, FAIL criterion); the state machine and its precedence; the record
against the design schema (verbatim; byte-equal to the schema block of the design document); the offline verifier (an independent second implementation of `D_A`, `K_pres`, the signed-zero count)
against 20 random hosts; the runners end to end with fake hosts for every classification and every INVALID reason; the write-back plan, the `DSTYLE` pair parser and the HF-C4 / HF-C5 findings; the
census builder; the five lists (write scope assertions, formats, hashes of the four R0 lists); the waiver table; the CLI exit codes; and the scan with its controls.

## Residual limits (honest list)

* **The scan is an IL / metadata review aid, not a proof.** It sees what the compiler emitted. It does not judge the edit that changes a list: it makes the edit visible and mandatory. **The final
  authority is the independent human source review plus the recorded hashes.** The five lists are not integrity-protected by the scan; their SHA-256 values are printed and MUST be compared by the CAD manager.
* **Bodies are not pinned.** The surface pin covers methods, not bodies or fields. An edit of a body of `SideDbWriter` that adds no import, no method and no new caller is invisible to the scan: e.g. a different
  `Database` passed to the `SideDbHandle` constructor, or `WblockCloneObjects` between the two handles in another direction (the scan does not check which handle is the source and which the destination).
  The same holds for the bodies of `ScratchRootGuard` and `PathRegions` (the rules that decide what a valid scratch path is live in `core-rs`, which the scan covers for imports only): they are protected by the
  tests, the source review and the hashes.
* **The `SaveAs` / `ReadDwgFile` argument checks are shape checks** (the path comes from `ScratchTarget.FullPath` / `ReadableSource.FullPath`, the guard call precedes in straight line, no branch between). They do
  not prove that the guard call dominates every path or that the guard's validation is right.
* **Check-then-use race.** Between `AssertCreatable` and the `SaveAs`, or `AssertReadable` and the `ReadDwgFile`, another process could create, replace or swap a file. Path checks are textual (normalized, case-insensitive);
  hard links, 8.3 short names and junction aliases are not detected, alternate data streams cannot be listed, and the reparse-point check covers the roots and their ancestors only at the moment it is made.
* **Aliases of the private copy and of the roots (review round 1).** The private copy is checked for the reparse attribute (symbolic link) by the preflight and by the guard, and a drive root is refused as a root.
  A hard link of the library passes every textual check (Windows exposes no cheap identity test without P/Invoke, which the scan forbids): the private copy is only READ (`ReadDwgFile`, never saved over), so the library cannot be
  changed through it, but the "private copy" isolation rests on the CAD manager creating a real copy (the SHA-256 before and after is recorded). UNC / admin-share spellings (`\localhost\C$\...`), 8.3 names and `subst` drives are
  not detected; the scratch root must be empty at start and every name is flat and create-new, which bounds the effect.
* **Scan holes found by the review (not closed, to be accepted or ruled):** (1) a delegate over a privileged method (`ldftn`) made inside a LISTED caller launders the call (the privileged methods are still bounded by their own bodies and
  the scoped import list); (2) the scan does not count the `new Database(true, true)`, `SaveAs` or `ReadDwgFile` sites, each of which satisfies its shape rule; (3) `RsGateCommands` may read any path with `File.ReadAllText`.
  These are caught by the source review and the hashes, not by the scan.
* **The designation file is not pinned.** `run-designation-rs.json` (the roots) is declared by the CAD manager next to the DLL; only the DLLs are self-pinned. The roots are bounded by the separation rules, by "scratch root empty at start" and by the
  create-new writers, not by a hash. Open: the CAD manager should record the designation hash in the seal / package v3, and decide whether `governedDocumentPath` must equal `DWGPREFIX + DWGNAME` (the instrument does not compare them).
* **Single execution is per session in memory plus the existence of the outputs.** No start marker is persisted before the first side effect: a crash before the records are written, and before the scratch DWG exists, leaves nothing that stops a
  new session from running the same command with the same designation. Open: a create-new "started" marker (a new file under the evidence root) would close it; not added because it is a new output file for the Coordinator to rule.
* **Create-new is the guard's, not the host's.** `Database.SaveAs` overwrites silently; "refuse if the target exists" is a check the guard makes just before.
* **The host may write more than the API shows**: caches or anonymous blocks inside the side database, and (HOST-TO-CONFIRM) companion files next to a saved DWG (backup, temporary, lock). A companion file in the
  scratch root is reported by the verifier as an undeclared entry and makes the run INVALID (conservative; see the open question below). AutoCAD's own journal, recovery and log files are outside every check.
* **Declarations, not observations.** `otherAcadProcess` and `loadRouteScripted` are declared by the CAD manager in the designation (the scan forbids every `Process` member but `GetCurrentProcess`); the
  instrument relays them. The module list, the machine class and the session binding are checked from outside the process (tuple record, I-1 / I-8 outside this folder).
* **V7 of the design (evidence sealed) is not decided by the instrument**: `PASS` means V1 to V6 and V8. The CAD manager runs the seal (`tools seal`) on the evidence root; the scratch root is not sealed by it
  (it is declared in the run record and verified by `scratch-verify`).
* **`DBMOD` equality is a control, not proof**: it shows the active document was not marked modified; it cannot show what AutoCAD did elsewhere.
* The instruments are written against the AutoCAD 2025 managed API without a running host; every behaviour that only the host can establish is marked `HOST-TO-CONFIRM` below.

## HOST-TO-CONFIRM (not established without the host; marked in the code)

* `new Database(true, true)` in a plugin (no document) builds a usable default drawing; `Database.SaveAs(string, DwgVersion)` on it; `ReadDwgFile(string, FileShare, bool, string)` and `CloseInput(true)` of a saved file.
* `WblockCloneObjects` of a dynamic block definition (with its extension dictionary) into such a database; the dynamic-property collection of a reference created through the API in a database with no document, and
  `GetAllowedValues()` on it; the exact `UnitsType` names.
* That the property setters of `Dimension` store their value in the `ACAD` extended data (`DSTYLE` section) as pairs of an `Int16` group code and a value (the parser refuses any other shape), and what `RecomputeDimensionBlock(true)` leaves.
* That `BlockReference.BlockTransform` gives column lengths equal to the absolute scale (the witness), and what the host stores for the 147 rows.
* That `Assembly.Location` is a real path for a `NETLOAD`ed DLL and that the Core and RS-core DLLs resolve from the DLL's folder; the runtime types of `GetSystemVariable` (`DBMOD`, `SECURELOAD`, `TRUSTEDPATHS`, `CPROFILE`).
* Whether `SaveAs` leaves a companion file in the scratch folder.

## Deviations and decisions to be ruled (flagged for the Architect and the Coordinator)

1. **Command names** follow the design (`CT21DHG_CENSUS_DYN`, `CT21DHG_DIMWRITEBACK`, `CT21DHG_TOLSCALE`), not the `*_RS` suffix proposed in the task.
2. **Where the OP2 file goes.** The design (2.4 S6) said "the evidence folder"; decisions section 242 (a) says the declared scratch root. The scratch root must differ from, and not nest with, the evidence root. Scratch files
   are therefore declared and hashed in the run record and verified offline, but not sealed by I-9. Open: should the CAD manager also copy and seal them?
3. **`tolscale-raw.json` has no `contentSha256`.** The design schema is closed and has no such field; the file validates against the schema **verbatim**, and the content hash (RFC 8785, without `volatile`) is carried by `rs-run-tolscale.json`
   (`contentHashes`). The other records carry their own `contentSha256`.
4. **Readings of the implementer (design 2.5 / 2.4), to be confirmed:** precedence INVALID > FAIL > UNKNOWN > PASS, a construction rejected by the host is a FAIL; the "offset of its own level" is `2^-k` for PF-1 rows and `2^-20`
   (the coarsest level) for PF-2 rows; the witness bound is 8 ulp of `max(1, |s|)`, applied to PF-1 rows with `k <= 36`; a row whose rotation is not 0 or whose normal is not +Z at either point is UNKNOWN.
   **Review change REQ-1:** the FAIL rule applies its MAGNITUDE test (`|read - intended| > offset of the row's own level`) to the in-scope rows only (`ProbeRow.InScope`, 135 rows); the 12 characterization rows
   (PF-2 bases 0.5, 2, 25.4, 1/25.4) enter no branch of the rule on magnitude, so a deviation there never FAILs and is recorded as `D_A,char`. The SIGN test stays on all 147 rows (a sign flip on a characterization row FAILs).
5. **Write-back scenarios (design 3.4 HF-C4 / HF-C5):** 18 dimensions in one side database (none; for each of the 8 product variables once different from the style and once equal to it; all 8 in the product order with the product's values
   for a text height of 3.0), read at OP1 and OP2. HF-C4 is `OBSERVED_DIFFERS` when a variable assigned a different value stores nothing, or when the stored pairs at OP2 differ from OP1; storing or dropping an override equal to the
   style value is a recorded finding, not a difference. HF-C5 is read only from the single stored pair of the "different" scenarios (source `HF-C4`), and is `UNKNOWN` on any ambiguity.
   **Review change REQ-2:** HF-C4 is also `OBSERVED_DIFFERS` when a "different" scenario stores a value other than the one written (`valueStoredAsWritten == false`), when the host order of the product-order scenario differs
   from the assignment order (`hostOrderEqualsAssignmentOrder == false`), or when that scenario stores a number of pairs other than 8; it is `UNKNOWN` when `valueStoredAsWritten` or `hostOrderEqualsAssignmentOrder` is undecided
   (null) (differences take precedence over undecided flags). Consequence: a host that drops an override equal to the style also drops product-order pairs whose product value equals the style, which now reads as `OBSERVED_DIFFERS`
   (the per-variable "equal to the style" scenarios remain a recorded finding only); two variables sharing a group code leave the host order undecided, so HF-C4 is `UNKNOWN` as well as HF-C5.
6. **Companion files make a run INVALID** (see the residual limits): open question whether a declared allowance for named companions is wanted once the host behaviour is known.
7. **A refusal is not an execution:** the once-per-session guard releases the command after a refusal that happened before any side effect, including a missing or invalid designation file (round 1).
8. **The R0 files are untouched.** `tools-rs` references the R0 tools project as a library; the IL fixture emitter of the R0 tests is linked as source (not copied) into the RS tests; the R0 scan, its 509 tests and its lists are unchanged.
9. **Debug builds** need three `[debug-only]` entries (compiler attributes) in the allowlist; the surface and the callers are identical in Debug and Release.
10. **Not implemented here, by gate:** HF-G2, I-7, I-8 (manifest / pin generator), package v3, any host run.
11. **Review round 1 (changes).** (a) The offline V6 equivalence `D_A = 0 <=> K_pres = 52` is evaluated over the 132 PF-1 in-scope rows only (design 2.3 says "for these rows"); with the PF-2 base-1 rows included the sentence is
    false (a host that loses only `nextDown(1.0)` is a valid observation). The Architect should confirm this reading. The R0 `ProbeStatisticsResult.EquivalenceDaZeroIffKpres52` (all rows, R0 file untouched) is no longer consulted by RS code.
    (b) A failure after the side effects (schema check, a result-file write) now writes a minimal `rs-run-*.json` (result INVALID, `POST_PROCESSING_FAILED:<type>`, the scratch ledger) through the same create-new writer, so the scratch DWG is declared;
    best effort, the original exception still propagates. (c) Drive roots, a symbolic-link private copy and a reparse-point read-back source are refused. (d) A missing or invalid designation releases the once-per-session flag.
    Not changed (rulings needed): HF-C4 does not read back the effective dimension getters (MIN-4); record field names of HF-C4/HF-C5 are the implementer's (MIN-5); `Normal`/`Rotation` rely on constructor defaults and are verified on read (MIN-6);
    `MANUAL_INPUT_DECLARED` is a refusal and not an INVALID record (MIN-7); the scan holes above.

## Conditions accepted by review

* **MIN-C (check-then-use race on `SaveAs` create-new).** The scratch guard checks that the target does not exist and the save happens afterwards; between the two a foreign process could create the file. Not closed in code: it needs the
  **explicit acceptance of the Coordinator** (the scratch root is a private folder of the CAD manager's session).
* **MIN-D (the fixed root is not enforced in code or pinned).** Nothing in the code or in a pin fixes the scratch/evidence root. The Coordinator authorization names, per run, **both roots and the hash of the designation file**.
