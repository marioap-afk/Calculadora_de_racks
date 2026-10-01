# I-52 CT-21D host-gate instruments, R0 class (`I52Ct21dHostFacts`)

Research-only implementation of the **R0 (STRICT READ-ONLY) instruments** of the CT-21D host-fact matrix. Nothing in this directory is
referenced by a product project, by `RackCad.sln`, by `deploy/` or by CI.

```text
NOT LOADED. NOT EXECUTED ON ANY HOST. No AutoCAD process was started to write, build or test any of this.
HOST_RUN_STARTED = FALSE     CT21D_AUTHORITY_BASELINE_READY = FALSE     CT21D_EXECUTION_READY = FALSE     CT21D_EXECUTION = NOT_AUTHORIZED
INSTRUMENT_IMPLEMENTATION_GATE = OPEN FOR THE R0 CLASS ONLY
```

## The gate

| Section | Content |
|---|---|
| 237 (Owner) | G6 = EXTEND (non-governing, read-only host-type facts that the current BA-05 text expressly requires); the instrument implementation gate opens after the Architect's TOL_SCALE design |
| 238 (Architect) | `docs/initiatives/I-52-ct21d-tol-scale-design-and-host-fact-matrix-v1.md`: the host-fact matrix (section 3), the instrument specifications (section 5), STRICT / R0 (sections 0 and 5.1) |
| 239 (Coordinator) | PASS WITH CONDITIONS. The gate opens **only for the R0 class: I-0, I-1, I-2, I-4, I-9**, in a new governed folder under `eng/research/`. Q-O-0(a) and (b) are ruled inside "solo lectura" (subject to the Owner's objection). Conditions: (1) every instrument is reviewed independently as read-only by a semantic review of the forbidden calls, and the Coordinator confirms it before declaring it part of the set; (2) no load and no execution in AutoCAD, only tests that need no AutoCAD; (3) package v2 (AR10-07) is written after the instruments |

## What is implemented

| Id | Instrument | Where | Facts |
|---|---|---|---|
| I-0 | `core`: label computation (RFC 8785), canonical JSON, hashing, **the single `EvidenceWriter`**, schema validation (closed JSON Schema subset), RB / context-variable expectation tables, census / inventory / context-variable record builders over an abstract reader, the runners, the PF-1/PF-2 probe-table generator and the decision statistics (`D_A`, `K_pres`, `I_max`), the self-pin check, the evidence sealer | `core/` | HF-M7 and the logic of every fact below |
| I-1 | observation protocol (text), the offline **label helper** (`tools label`), the OS-level script. **Protocol text only**: nothing here assembles the `ct21d.hostfact.v1` records of HF-M1..HF-M11 (only HF-M7, the label, has a record) | `protocol/`, `core/RawAttributeText.cs`, `tools/` | HF-M1..HF-M11 (observation procedure) |
| I-2 | R0 plugin DLL, commands `CT21DHG_CENSUS_R` and (separate command, own record) `CT21DHG_INVENTORY` | `r0/` | HF-C1, HF-C3, HF-G1 (M1), HF-T2 |
| I-4 | R0 plugin DLL, non-Session command `CT21DHG_CTXVARS` (system-variable reads only, no database) | `r0/` | HF-G3 |
| I-9 | evidence seal (`tools seal`): `HASHES.sha256`, its digest, read-only files, re-run reports no change | `core/EvidenceSealer.cs`, `tools/` | the seal of every record |
| tools | `scan` (the forbidden-API semantic scan: deny-by-default against `allowed-apis.txt`, plus the pinned privileged surface `privileged-surface.txt` / `privileged-callers.txt` / `expected-commands.txt`), `imports-allowlist-check`, `imports-list`, `privileged-list`, `label`, `seal` | `tools/`, `allowed-apis.txt`, `privileged-*.txt`, `expected-commands.txt` | design 5.3 |

## What is NOT implemented, and why

| Not implemented | Why |
|---|---|
| **RS class: I-3** (dynamic-property census, HF-C2), **I-5** (write-back probe, HF-C4 / HF-C5 designation), **I-6** (TOL_SCALE probe, HF-T1), the **HF-C4 write probe**, the **OP2 DWG save** | Blocked by Owner question Q-O-1 (decisions section 239). They write to side databases and save files, which STRICT R0 forbids. No code of them exists here, and the forbidden-API scan would fail if any were added to these assemblies |
| `tolscale-raw.json` record builder and schema, the classification state machine of design 2.5 | They belong to the record of the I-6 probe (RS). Only the pure pieces that I-0 lists for the tables and statistics are present |
| **I-7** fixture conformance reader | Follows package v2 (assembly identity of the product Application layer is unsettled; not in the H1 declared set, R-N(b)) |
| **I-8** build and pin procedure, **including the package-manifest / pin generator** | Section 239: "No se implementan I-7 ni I-8". An earlier revision of this folder carried a manifest generator (`PackageManifest`, the `manifest` command, `ct21d.package-manifest.v1`); it was **removed** after the spec-fidelity review. The only pin-related code left is the `SelfPin` comparison of design 5.1 (an instrument checks its own file against a sidecar `<dll>.pin` that the CAD manager supplies) |
| Package v2 (AR10-07) | Written after the instruments (decisions section 239 condition 3) |

## STRICT R0, as built

* No API write call of any kind, no save, no write or write-open of a product or open-document database, no `Commit`, no event handler, no
  overrule, no `SetSystemVariable`, no command, no document open or lock, no registry write, no network, no process start, no P/Invoke.
* The only database is `new Database(false, true)` filled by `ReadDwgFile` from a **private copy** (a path declared in
  `run-designation.json`), read through `OpenCloseTransaction` objects that are only disposed (never committed), and never saved or
  attached. This lives in ONE type, `R0/AcadSideDbReader`; the scan allows `new Database`, `ReadDwgFile` and the transaction manager only there.
* Every file written by an instrument or a tool goes through **one class**, `Core/EvidenceWriter`: create-new (`FileMode.CreateNew`),
  flat names inside an existing folder, write-once, no append / overwrite / delete API, SHA-256 of every written file recorded in its
  ledger and re-verified by reading the file back. The scan allows file writes only inside that exact type, with constant arguments (`FileStream(path, CreateNew, Write, None)`, a `SetAttributes` that only sets ReadOnly), and only the callers of `privileged-callers.txt` may call it (layer 3).
* The R0 claim is "no API write call and no save", **not** "nothing is written in memory" (design 5.3 item 4): the host may evaluate or cache
  inside the side database. The controls are the host-run checks that each runner performs and records: SHA-256 of the private copy and
  of the library equal before and after, `DBMOD` equal before and after, the self-pin of the DLL, and the evidence folder holding only declared
  outputs (the seal). Any failed check makes **every** fact of the run `INVALID` (records retained, never repaired).

## Build and test WITHOUT AutoCAD

Use the user-level SDK (the `dotnet` on `PATH` does not resolve `global.json`):

```powershell
$dotnet = "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe"
& $dotnet build eng/research/I52Ct21dHostFacts/tests/I52Ct21d.HostFacts.Tests.csproj -c Release
& $dotnet test  eng/research/I52Ct21dHostFacts/tests/I52Ct21d.HostFacts.Tests.csproj -c Release --no-build
```

* `core/`, `tools/` and `tests/` reference no AutoCAD file. The tests never start, load or touch AutoCAD.
* `r0/` only **compiles** against `AcCoreMgd.dll`, `AcDbMgd.dll`, `AcMgd.dll` of an installation (`AutoCADInstallDir`, default
  `C:\Program Files\Autodesk\AutoCAD 2025`, the same property and default as `eng/validation/I52G3AAutoCadProbe`) with `Private="false"`: the
  AutoCAD assemblies are not copied and building does not lock them. When they are installed the test project builds `r0/` and the scan tests
  that read the real R0 DLL are compiled in (`HAVE_R0`); without them those tests are compiled out and the reviewer scans the DLL with `tools scan`.
* `bin/` and `obj/` are ignored by the repository `.gitignore`.
* `tests/data/*.json` and `schemas/*.json` are generated by `tests/data/gen_vectors.py` (expected vectors, a second implementation in Python:
  `hashlib`, `json`, `struct`) and `schemas/gen_schemas.py` (the authoring form of the schemas). The generated files are committed.

What the tests cover (all without AutoCAD): the `EvidenceWriter` (refuses overwrite, append, outside-folder and non-flat names; records hashes;
no mutating API on its surface); the RFC 8785 and label vectors computed independently with Python; the raw-file rules; schema validation of
every record (and mutations that must fail); the census, inventory and context-variable logic over a fake reader; the RB and context-variable
tables compared mechanically with BA-05 V5; the probe table (147 rows, 135 in scope) and statistics; the runners end to end with every host-run
check failing in turn; the overlapping-path refusals; the seal; and the **forbidden-API semantic scan with negative controls and the imports allowlist** (see below).

## The forbidden-API semantic scan (design 5.3; decisions section 239 condition 1)

`tools/Scan` reads the **metadata and IL** of an assembly (`System.Reflection.Metadata`; it is not a Roslyn analysis). It works in **three layers** (plus the raw-memory rules of layer 2b).

**Layer 1, the imports allowlist (design 5.3 item 2), is DENY-BY-DEFAULT.** `allowed-apis.txt` lists, one per line with a mandatory justification
(`KIND|ITEM|SCOPE|JUSTIFICATION`), every assembly (`A`), type (`T`) and member (`M`, `Type::Member`, or `Type::*` for a pure type) that the
`core`, `tools` and `r0` assemblies may reference. The scan fails on **every** row of the metadata tables (assembly references, type
references, member references: so calls, `newobj`, delegates, `ldftn`, tokens, custom attributes and reflection by token) that is not listed, and at each
instruction it checks the **scope** of the approval. A scope is `Assembly::Full.Type.Name`: the exact top-level type **and the assembly that defines
it** (so a type of the same name in another assembly, a spoof, gets nothing; the scoped types are `EvidenceWriter`, `EvidenceSealer`, `Sha256Hex`,
`JsonSchemaLite`, `AcadSideDbReader`, `HostEnvironment`, `HostGateCommands`, `Tools.Program`, `IlReader`, `ForbiddenApiScan`... and every one of the
scoped lines of the real list is tested against its spoof). Examples: `FileStream::.ctor`, `Stream::Write` and `File::SetAttributes` only inside
`Core::Core.EvidenceWriter` (their ARGUMENTS are checked by layer 3); `File::ReadAllBytes` / `ReadAllText` / `Exists`, the `Path` members, `EvidenceWriter::.ctor` and `IDisposable::Dispose` only inside the listed types that use them (one scoped line per type; the command attributes cannot be scoped by instruction and are pinned by `expected-commands.txt`); `Database::.ctor`, `ReadDwgFile`, `StartOpenCloseTransaction` and `Transaction::GetObject` only inside
`R0::R0.AcadSideDbReader`. The same pair is what grants the two structural privileges in layer 2 (`EvidenceWriter` must be defined in the core
assembly, `AcadSideDbReader` in the R0 assembly; a redefinition elsewhere is `R-WRITER-REDEFINED` / `R-SIDEDB-REDEFINED` and has no privilege). `calli`, `jmp` and a store to an external field are always refused. A wildcard is refused for every Autodesk type and
for the namespaces that can touch disk, process, registry, network, native code or reflection. The list was **drafted from the current code**
(`tools imports-list`, which approves nothing) and then **reviewed line by line by the implementer for read-only safety**: it holds no save, no
write, no load of native or managed code, no process start, no global-state setter, no database other than the in-memory side database. It is
minimal: `tools imports-allowlist-check` reports an approval that no assembly uses (only the three `[debug-only]` entries, compiler attributes of
Debug builds, are exempt). The independent reviewer of section 239 condition 1 must still read the list: it is the surface the scan trusts.

**Layer 2, the deny rules (kept).** For a call that takes an `OpenMode` the scan resolves the **constant operand** in the IL (`OpenMode.ForWrite`,
`(OpenMode)1` and a named constant all compile to the same `ldc.i4`); an `OpenMode` that is not a compile-time constant is rejected. Rules (each has
an id): `Save/SaveAs/Wblock/Insert/DxfOut/DxfIn/AttachXref/BindXrefs/ResolveXrefs/SwapIdWith`, write open, `Commit`, a transaction outside the
side-database reader, `new Database` other than `(false, true)` in the reader, `ReadDwgFile` outside the reader, `SetSystemVariable`, a handle on an
open document's database, `Overrule`, commands / document manager / lock, event subscriptions, property writes and a conservative mutator-name
heuristic over **every** Autodesk namespace (Runtime, Customization, PlottingServices, Publishing included; prefixes now include Write, Load, Swap,
Bind, Detach, Dxf, Regen, Highlight...), `DynamicLinker`, creation of database objects, file writes outside the `EvidenceWriter` (and
`Path.GetTempFileName`, `System.Xml`, `System.IO.Compression`, `MemoryMappedFiles`, `FileSystemWatcher`, pipes, `System.Data` as sink types),
every `Process` member except `GetCurrentProcess` and getters, registry writes, network types, assembly loading, reflection invoke and
`CreateDelegate`, dynamic code (`Expression` trees, the C# `dynamic` binder, `CallSite`, `Reflection.Emit` builders), P/Invoke, `calli`,
process-wide state (`CultureInfo` defaults, `Console.SetOut`, `Environment.CurrentDirectory`, `AppDomain.SetData`, `Thread`, `AppContext`...), and any
AutoCAD reference in `core` / `tools`. The heuristic stays deliberately conservative; a false positive is resolved by the reviewer.
Added after the independent audit: every `Marshal` member (`Copy`, `StructureToPtr`, `Write*`, `GetFunctionPointerFor*`, `AllocHGlobal`...), `GCHandle`,
`NativeMemory`, `MemoryMarshal`, `Buffer.MemoryCopy`, **every** `System.Runtime.CompilerServices.Unsafe` member (`R-UNSAFE-API` / `R-PINVOKE`);
`DocumentCollection` **and its extension class** `DocumentCollectionExtension` (`DocumentManager.Open(string)`) for the `Open` / `Close` / `Add` /
`Recover` / `Create` / `Remove` / `Load` prefixes (`R-COMMAND`); the constructors and starters of `Thread`, `ThreadPool`, `Timer`, `Task`, `TaskFactory`,
`Parallel` (`R-THREAD`).

**Layer 3, the privileged surface (checked-in lists; added after the second independent audit).** Layers 1 and 2 cannot see (a) a call between two
methods of the SAME assembly (it is a call to a method definition, so a new Core type could call `new EvidenceWriter(anyExistingFolder).WriteNew(...)`,
`MarkReadOnly(...)` or the sealer), (b) the arguments behind an approval scoped to a whole type (`FileStream::.ctor` inside the `EvidenceWriter` said nothing
about `FileMode`), or (c) a method added to the writer. Layer 3 is read from the IL and metadata too and needs no allowlist line:

| Rule | What it refuses |
|---|---|
| `R-FILESTREAM-ARGS` | inside `EvidenceWriter`, any `FileStream` constructor other than `(string, FileMode, FileAccess, FileShare)` with the three trailing arguments loaded by `ldc.i4` constants equal to `CreateNew` (1), `Write` (2), `None` (0) and no branch target among them. `Create`, `Append`, `OpenOrCreate`..., a non-constant or an unresolvable argument are refused (the same constant walk as `OpenMode`) |
| `R-SETATTRIBUTES-ARGS` | inside `EvidenceWriter`, `File.SetAttributes(string, FileAttributes)` whose second argument is not `File.GetAttributes(p) OR ReadOnly` (or the constant `ReadOnly`). `Normal`, any clear of the read-only bit and unresolved values are refused |
| `R-PRIVILEGED-SURFACE` | the set of methods (name, parameter types, return type, accessibility, static, virtual; nested closures included) of `EvidenceWriter` and `EvidenceSealer` differs from `privileged-surface.txt` in either direction. The message says that editing the surface needs an explicit independent review. A new method such as `Clobber(path, bytes)` fails here even if its body passes the rules above |
| `R-PRIVILEGED-CALLER` | any call, `newobj`, `ldftn` or `ldtoken` of a member of `EvidenceWriter` / `EvidenceSealer` from a method not listed in `privileged-callers.txt` (`Assembly::Type::Method`, a bar, `Type::Method`, a bar, why), in ANY of the three assemblies, **Core included** (method definitions are resolved, not skipped). A privileged type calling itself is not listed (its surface is pinned). When all three assemblies are scanned, a listed caller that nobody makes is reported as stale (`R-PRIVILEGED-CALLERS-UNUSED`) |
| `R-COMMAND-UNEXPECTED` | any `[CommandMethod]` / `[CommandClass]` whose owner and decoded fixed arguments are not in `expected-commands.txt`, an undecodable blob, or a type deriving from those attributes (stale entries: `R-COMMAND-UNUSED`) |

The three files carry the lists the reviewer reads: `privileged-surface.txt` (18 methods, a pin, no justification), `privileged-callers.txt` (15 callers,
each with a justification, derived with `tools privileged-list` and then reviewed caller by caller by reading its source line) and `expected-commands.txt`
(the 3 commands of design 5.2 and the one `CommandClass`). `scan` requires `--privileged-surface`, `--privileged-callers` and `--expected-commands`
(there is no way to run the CLI scan without them) and prints the SHA-256 of every list used. The surface file is a pin of the type, not an approval: the
Debug and Release builds produce the identical surface, and a new lambda in the sealer is a surface change on purpose.

**Layer 2b, raw memory and unsafe code (independent of the allowlist: these rules run with it and without it).** A write through a pointer in unsafe
code (`*p = 5`, `stackalloc` plus a pointer write, a block copy) uses only IL opcodes and no external member, so neither layer above can see it.
These rules read the opcodes and the signatures instead:

| Rule | What it refuses |
|---|---|
| `R-RAW-MEMORY` | the opcodes `localloc`, `cpblk`, `initblk`, everywhere; they can never be approved by a list line |
| `R-RAW-STORE` | the opcodes `stind.i`, `.i1`, `.i2`, `.i4`, `.i8`, `.r4`, `.r8`, `.ref`, `stobj`, `cpobj` (a store through a pointer or a by-ref) unless the list carries `O\|opcode\|Assembly::Type\|why` for the containing top-level type. The compiler emits them for managed `out` / `ref` parameters (record `Deconstruct`, `Try...(out x)`), so the real code needs 70 such (type, opcode) approvals (core: 30 types, tools: 7 types); the R0 DLL needs none. An approval says "these are managed by-ref stores"; the next rule makes that claim checkable |
| `R-RAW-STORE-NATIVE-ADDRESS` | an **approved** store inside a method that can forge a native address: a `conv.i` / `conv.u` (and `.ovf`) opcode, or `IntPtr` / `UIntPtr` in its signature or locals. The approval does not hold for that method |
| `R-POINTER-TYPE` | a pointer (`T*`, `void*`), a function pointer (`delegate*`) or a pinned local in ANY signature: method return / parameters, locals of the method body, fields, properties, events, member references (also reported at the call site), method and type specifications, standalone signatures |
| `R-UNSAFE-CODE` | `System.Security.UnverifiableCodeAttribute` on the module or the assembly (what csc emits for `AllowUnsafeBlocks`), referenced, or defined by the assembly itself |
| `R-NOT-ILONLY`, `R-NATIVE-METHOD` | an image that is not IL-only; a method implemented as native / unmanaged / internal call |

Not covered, by design: `ldind.*` (a read), `initobj`, array stores (`stelem.*`, bounds-checked), `Span<T>` indexers; reads through a pointer need the
pointer, which the signature rule refuses. A store through an **unmanaged** address without a pointer type or `conv.i/.u` (an `IntPtr` coming from an
external call, then `stind`) is caught by the `stind` refusal unless the type is approved, and the member that produced the `IntPtr` is refused by layer 1.
The residual risk is therefore an approved type that obtains a native `IntPtr` from an approved external call inside a method without `conv.i` /
`IntPtr` in its own signature: layer 1 shows every such call (none of the 70 approved types calls anything of `Marshal`, `Unsafe`, `GCHandle`,
`SafeHandle`...).

```powershell
$t = "eng/research/I52Ct21dHostFacts"
& $dotnet $t/tools/bin/Release/net8.0/I52Ct21d.HostFacts.Tools.dll scan --allowed $t/allowed-apis.txt `
    --privileged-surface $t/privileged-surface.txt --privileged-callers $t/privileged-callers.txt --expected-commands $t/expected-commands.txt `
    core:$t/core/bin/Release/net8.0/I52Ct21d.HostFacts.Core.dll `
    tools:$t/tools/bin/Release/net8.0/I52Ct21d.HostFacts.Tools.dll `
    r0:$t/r0/bin/Release/net8.0-windows/I52Ct21d.HostFacts.R0.dll       # exit 0 clean, 2 violations, 1 usage / malformed list
& $dotnet $t/tools/bin/Release/net8.0/I52Ct21d.HostFacts.Tools.dll imports-allowlist-check $t/allowed-apis.txt core:... tools:... r0:...   # layer 1 only + stale entries
```

`--allowed` and the three privileged lists are mandatory (there is no way to run the CLI scan without layers 1 and 3) and the report prints the SHA-256 of every list used. `tools privileged-list core:... tools:... r0:...` prints the observed surface, callers and command registrations in the list syntax (a review aid; nothing is approved by printing).

It is a **review aid with negative controls**, not the independent review. The tests build fixture assemblies (emitted with
`System.Reflection.Metadata`, no AutoCAD file) for every forbidden call and require the scan to fail **twice**: layer 2 alone must report the deny
rule while the import is approved, and layer 1 alone (the real list) must refuse the import. The fixtures include the seven calls an earlier
review injected into a copy of the R0 DLL and the scan wrongly passed (`Database.DxfOut`, `DynamicLinker.LoadModule`, `Path.GetTempFileName`,
`XDocument.Save`, the `CultureInfo.DefaultThreadCurrentCulture` setter, `Editor.Regen`, `Process.CloseMainWindow`) plus `Delegate.CreateDelegate`,
`Expression.Compile`, `calli`, the `Environment.CurrentDirectory` setter, `AppDomain.SetData`, `Console.SetOut`, `ZipFile` and `MemoryMappedFile`. The real
assemblies must be clean and the scan reports how many rows and call sites it examined; the real R0 DLL is mutated in memory (a `ForRead` constant
patched to `ForWrite`; the side-database constructor arguments patched; one approval removed from the list) to prove that its CLEAN verdict is not
vacuous. The same seven calls were also compiled into a scratch copy of the R0 sources (outside the repository, compile only, never loaded): the
scan reported all of them.

**Raw-memory negative controls (round 3).** `fixtures/UnsafeFixture` is a separate project compiled **with `AllowUnsafeBlocks`** (the only place that
switch exists; the csproj of `core`, `r0` and `tools` do not set it, and the scan refuses the `UnverifiableCodeAttribute` that it makes csc emit). Its DLL
is only read as bytes by `tests/RawMemoryScanTests.cs`, never loaded: `*p = 5` through `int*` and through `&local`, `stackalloc` plus a pointer write,
`Span<int> = stackalloc`, `Unsafe.Write(void*, T)`, `Unsafe.WriteUnaligned` over a `ref`, `Marshal.Copy` to an `IntPtr`, `Buffer.MemoryCopy`,
`GCHandle.Alloc`, `fixed`. Every class must fail (a) with the deny rules alone, (b) with the real list and (c) with every import approved. A note found
while writing them: Release builds elide the `int*` local of `int* p = &x; *p = 5;` (`ldloca; stind.i4`), so in that case **only** the `stind` refusal sees
the write. `FixtureBuilder` emits the shapes a compiler will not produce on demand (each `stind.*`, `stobj`, `cpobj`, `localloc`, `cpblk`, `initblk`, a pointer
or `delegate*` as parameter, return, local, field, property, event, member reference, a pinned local, the module / assembly attribute, a non-IL-only image,
a native method) and the document-manager extension (`DocumentCollectionExtension.Open`), the `Marshal` family, `Unsafe`, `GCHandle`, `NativeMemory`,
`MemoryMarshal`, `Buffer.MemoryCopy` and the `Thread` / `Timer` / `Task` / `ThreadPool` / `Parallel` starters. **Spoofing**: for every scoped line of the
real list (members and store opcodes) a fixture defines the same top-level type name in another assembly and must be refused (`R-IMPORT-SCOPE` /
`R-RAW-STORE`), while the genuine (assembly, type) pair passes. **Layer 3 negative controls** (`tests/PrivilegedSurfaceTests.cs`; each must make the scan FAIL while the real tree stays CLEAN): injection 16 (a new Core type calling `EvidenceWriter.WriteNew` and
`MarkReadOnly`, an IL clone of Core, plus the same call from another assembly), P1a / P1b (`FileMode.CreateNew` patched to `Create` / `Append` in the REAL Core DLL, in memory),
P2 (a `Clobber(anyPath)` method using `File.SetAttributes(Normal)` + `new FileStream(anyPath, FileMode.Create)`), a new `[CommandMethod]` not in the list (fixture, and the real R0 against a list
without one of its commands), an unresolvable `FileMode`, a dropped real caller line (`Runners::RunCensus`, an intra-Core call), a surface line added to / removed from the file. Tests: 509 in
Release and 509 in Debug (`dotnet test`, no AutoCAD process).

## Pinning (design 5.1 "Pinning and determinism")

* This folder has **no** manifest or pin generator (I-8 is not authorized; see "What is NOT implemented"). The CAD manager supplies, for the declared
  set, a plain list of `{path, sha256}` (field `declaredSet` of `run-designation.json`) and a `<dll>.pin` sidecar next to each instrument DLL
  (the lowercase SHA-256 of the DLL and an LF). How package v2 produces them is its own matter.
* At command start the instrument compares its own file with the sidecar `<dll>.pin` (`SelfPin`). This protects against an accidental mismatch only
  (a DLL substituted together with its sidecar passes; the check is on the file, not on the loaded image). The independent pins are the pre-H1
  tuple record and the module-list hash checked from outside the process. A missing or different pin makes the run `INVALID`.
* The repository `Directory.Build.targets` stamps the git HEAD into `InformationalVersion`, so the DLL bytes (and hashes) change after any commit:
  the canonical pinned build belongs to package v2. `allowed-apis.txt` is independent of that (it names APIs, not hashes).

## Running on a host (future, not authorized)

The CAD manager would place `I52Ct21d.HostFacts.R0.dll`, **`I52Ct21d.HostFacts.Core.dll`** (the R0 DLL depends on it, so the declared set must
include Core; that NETLOAD resolves it from the DLL's own folder is HOST-TO-CONFIRM), their `.pin` files and a `run-designation.json` (schema
`ct21d.designation.v1`: run id, paths, the declared hashes, the `declaredSet` list, the tuple bindings) in a trusted folder, and `NETLOAD` the R0 DLL. A
host gate needs the Coordinator's authorization and the declared set final (R-G / R-N). Nothing of that has happened.

The designation is **refused before anything is touched** (parser and every runner; nothing is written, the private copy is not even opened)
when the private-copy path equals the library path, or when the evidence folder is the directory of the library or of the private copy (or either file).
The comparison is on normalized full paths, ordinal and case-insensitive; it is a path check, not a file-identity check (a hard link or junction would pass).

## Deviations and notes for package v2 / the Architect

* **I-9 is written in C#**, through the single `EvidenceWriter` (`tools seal`), not as a PowerShell script. The Coordinator must accept this deviation.
* **`run-designation.json` (`ct21d.designation.v1`) and the `.pin` sidecar format are the implementer's own design**, not specified by the design
  document. Flagged for package v2 and the Architect. The designation carries `declaredSet` as a plain list supplied by the CAD manager; it still carries
  `tupleBinding.packageManifestSha256`, because every record schema of design 3.1 carries that tuple member: here it is an **opaque 64-hex value supplied by
  the CAD manager** (what package v2 defines it to be); nothing in this folder generates or checks a manifest.
* **The scan is IL / metadata based (`System.Reflection.Metadata`), not Roslyn**, so it sees what the compiler emitted, not the source. It does not
  see reflection by string (`GetType("...")`, `GetMethod("...")`) other than by refusing the types and members that perform the call; those are not in
  `allowed-apis.txt`.
* **The OS-level script `protocol/i1-os-observation.ps1` writes its files with a .NET `CreateNew` open** (disclosed Q-O-0(b), ruled inside "solo lectura"
  by section 239, subject to the Owner's objection). It was parse-checked, never run.
* **I-1 is protocol text.** Nothing here assembles the `ct21d.hostfact.v1` records for HF-M1..HF-M11; only HF-M7 (the label) has a record builder.
* **The R0 DLL depends on `I52Ct21d.HostFacts.Core.dll`**: the declared set must include Core (see above).
* **`Directory.Build.targets` stamps the git HEAD**, so DLL hashes change after a commit; the canonical pinned build is package v2's.
* The `allowed-apis.txt` approvals are scoped to `Assembly::TopLevelType` (the assembly that defines the type is part of the scope) where there is one type in the current code; a legitimate new use in
  another type needs a reviewed line, on purpose. The 70 `O|stind...` lines are by-ref stores of the compiler, per (assembly, type, opcode).
* The independent review required by section 239 condition 1 has **not** been replaced by any of this; the scan and the lists are aids for it.

## Residual limits of the scan (honest list)

* It is an IL / metadata review aid, not a proof. The privileged lists are only as good as the review of the edit that changes them: the scan makes the
  edit VISIBLE and mandatory, it does not judge it.
* The surface pin covers methods (and nested closures), **not** fields, nor the bodies of the existing methods. A body change is caught only by what the
  other rules see (new imports, constant arguments of `FileStream` / `SetAttributes`, new callers); e.g. `WriteNew` could be rewritten to skip its own name
  validation without any scan finding, which is why the tests of the writer and the independent reviewer remain.
* The argument rules resolve constants loaded by one `ldc.i4` right before the call. An argument computed any other way is **refused** (unresolvable), so the
  residual risk is a false positive, not a pass. The first argument (the path) of `FileStream` / `SetAttributes` is not analysed. It is bounded by the pinned surface and the listed callers ONLY IF the bodies of the privileged types are unchanged: an edit of a privileged body that adds no import, no non-constant argument and no new caller is invisible to the scan (independent audit attacks A1, A2, A3, A5 passed), and what such an edit can still do under the enforced `FileMode.CreateNew` and ReadOnly-only is create NEW files at any path, add an alternate data stream to an existing file (`CreateNew` on `existing.txt:stream`), and set ReadOnly on any file. `CreateNew` therefore does not mean that existing files are untouched. `WriteNew` / `MarkReadOnly` validate flat names; `MarkReadOnly` can set ReadOnly on any flat-named file of the root, not only files this writer wrote. The four checked-in lists (`allowed-apis.txt`, `privileged-surface.txt`, `privileged-callers.txt`, `expected-commands.txt`) are not integrity-protected by the scan: their SHA-256 values are printed by the scan and MUST be recorded in the review record and compared by the CAD manager. The caller list is keyed on `Assembly::Type::Method` and callee, with no signature or call count: overloads, generics and extra calls from a listed caller pass, and a listed constructor caller accepts any folder expression. The evidence folder of `run-designation.json` is any existing absolute directory (the CAD manager supplies it); it is checked only against the library and private-copy paths and, on its final component, for reparse points. Fields are not pinned; a duplicate `CommandMethod` on a listed method name passes.
* Caller identity is `Assembly::Type::Method` (nested closures are their own caller names). The callers list does not name the call site, so a listed caller
  can make additional calls of the same callee. A privileged type calling itself is not listed. Calls through reflection are refused by layer 2, and an
  `ldtoken` of a privileged type alone (no member) is not a call.
* The command check covers `CommandMethod` and `CommandClass` only; other registration attributes (`LispFunction`, `ExtensionApplication`, `Initialize`) are
  blocked by the imports allowlist, not by the expected-commands list. Command attributes cannot be scoped to a type in `allowed-apis.txt` (the metadata
  pass sees them without an instruction), so `expected-commands.txt` is their control.
* `File::ReadAllBytes` / `ReadAllText` / `Exists` and the `Path` members are now scoped to the types that use them; a new legitimate use in another type
  needs a reviewed line. Write-capable members outside `EvidenceWriter` remain refused by layers 1 and 2.
* The injections that ADD code (16, P2, the command) are IL fixtures that mimic the Core shape; they prove the rules fire on those shapes, not that every
  conceivable source edit is caught.

## HOST-TO-CONFIRM (not established without the host; marked in the code)

* `Database.ReadDwgFile(string, FileShare, bool, string)` on a private copy, and `CloseInput(true)` releasing the input file.
* How a proxy or a custom (third-party) object presents itself to the managed API (`IsProxyOrCustom` is `ProxyEntity` or an RX class not named `AcDb*`).
* That a reference to the anonymous representation of a dynamic block reports `IsDynamicBlock = true` (the `ReferencesAnonymousNonDynamic` flag).
* The `TypeCode` and runtime type of each typed value of the `ACAD` extended data and of stored result buffers.
* The runtime types returned by `GetSystemVariable` on the exact build; `Assembly.Location` being a real path under `NETLOAD`; that the R0 DLL resolves
  `I52Ct21d.HostFacts.Core.dll` from its own folder.
* The AutoLISP items of the I-1 protocol (`open` encoding argument, `write-line` terminator, `strlen` counting, `vlax-product-key`).
* That the host does not write inside the side database in a way that changes the private copy (checked, not assumed: the hashes before and after).

## Layout

```text
core/        I-0 library (net8.0, no AutoCAD reference)
r0/          R0 plugin DLL (net8.0-windows, x64; compiles against the AutoCAD managed assemblies)
tools/       scan, imports-allowlist-check, imports-list, label, seal (net8.0 console)
tests/       xUnit tests, FixtureBuilder (IL fixtures for the scan), data/ (vectors from Python)
fixtures/   UnsafeFixture: negative-control assembly compiled WITH AllowUnsafeBlocks (read as bytes by the scan tests, never loaded)
schemas/     JSON schemas of the records (and gen_schemas.py)
allowed-apis.txt   the approved imports of the three assemblies (deny-by-default scan, layer 1)
privileged-surface.txt   the pinned method set of EvidenceWriter / EvidenceSealer (layer 3)
privileged-callers.txt   the only callers of those two types, Core included (layer 3)
expected-commands.txt    the expected [CommandMethod] / [CommandClass] registrations (layer 3)
protocol/    I-1 observation protocol text and the OS-level script (parse-checked only, never run)
```
