# I52Ct21dPhase2 — phase 2 session-binding attestation scripts (S1-A), review material

```text
STATUS      = REVIEW MATERIAL FOR THE CAD MANAGER AND THE COORDINATOR. DESIGNED AND IMPLEMENTED; NEVER RUN ON A HOST; NOT AUTHORIZED TO RUN.
              FIXER ROUND 1 APPLIED (two reviews: static scan not closed; sessionId identity; coverage).
              FIXER ROUND 2 APPLIED (two more reviews: `#requires` code-load channel, load-command words, schema pin, designation timing, S1-A exception,
              pinned writer calls, local-path guards).
              FIXER ROUND 3 APPLIED (two more reviews: member writes through ++/-- and multi-assignment, `$PSScriptRoot` reassignment, the public write wrapper,
              library-guard redefinition, FileStream overloads, the AFTER_NETLOAD pin coverage line). Needs a new review of the changed bytes.
GOVERNING   = FALSE      PHASE = 2      SCOPE = session S1-A first (the same scripts take S1-B .. S4 by the -Session parameter)
AUTHORITY   = I-52 decisions sections 237-248; section 248 asks for the phase 2 scripts "para su revision, sin ejecutarlos";
              package v3 3.5.3 (tuple record PHASE 2), 3.7 (R3-2, step 3), 5.1 stop conditions 14-16, P4 / C-05, ruling (e) attestations;
              Coordinator ruling for the S1-A pre-flight remediation: design and implement the scripts YES, execute them as phase 2 NO.
HOST        = AUTOCAD_STARTED = FALSE     HOST_RUN_STARTED = FALSE     FIRST_NON_GOVERNING_HOST_RUN = NOT_AUTHORIZED
```

This folder is **new**; nothing under `src/`, `tests/` or an existing `eng/` folder was changed. The scripts are PowerShell 7 (`#Requires -Version 7.0`),
Windows only, read-only on the system. Their bytes are listed in `HASHES-PHASE2.txt`.

## 1. Purpose

Package v3 3.7 step 3 requires the phase 2 of the tuple record ("pid / processStartUtc, actualLoadedModules, sessionProfileFacts, pinCheck,
processList") to be **captured and validated** after `acad.exe` has started and **before** the designation file is placed or any characterization
command is typed (stop condition 16). Until now no mechanism existed (`PHASE2_ATTESTATION_PREP = NOT_READY`, decisions section 248).
`Capture-Phase2.ps1` observes the already started AutoCAD process from the outside and writes one machine-checkable record; `Validate-Phase2.ps1`
re-checks that record offline. The CAD manager's human fields of the template (`validatedBy`, `validatedAtUtc`, the signature) stay human: the tools
produce a **verdict about the external observations**, not a signature, and **not the template validation** (see "What the verdict covers", 5).

## 2. What it does NOT do

The properties below are the design intent. How far they are *guaranteed* is stated in section 8: the proof is the line-by-line human review of the
three production scripts; the static scan and the tests are guards that make a deviation visible, not a proof of absence.

- It does not start, script or automate AutoCAD; it never runs `NETLOAD` (it only hashes a transcript text file the CAD manager provides and
  extracts its load-command lines).
- It does not write the registry, change an ACL or a file attribute, touch `TRUSTEDPATHS` / `SECURELOAD`, load a DLL, start another process,
  stop a process, or write any file outside the evidence root **by its own code**. (It **reads** the profile variables from the registry, see 5.)
  One exception is stated plainly: the signer column calls `Get-AuthenticodeSignature`, which is **not** side-effect free. It may do a certificate-revocation lookup
  (**network read**) and the CryptoAPI layer may write its URL / certificate cache under the invoking user's profile (**a file side effect outside the
  evidence root, made by the operating system, not by this tool's code**). The column is informational, volatile and in no check; dropping it is open question 6.
- The **only** function that creates a file is `Write-Phase2NewFile` (lib): create-new (`FileMode.CreateNew`), flat name, inside a validated local
  evidence root, refusing any existing entry. Nothing is deleted, moved, overwritten or appended.
- It does not decide `TRUSTEDPATHS` (its prior value is a separate evidence matter), does not apply the ACL, does not compose the designation, does
  not produce the `machineClassLabel`, does not read `CPROFILE` / `SECURELOAD` / `TRUSTEDPATHS` from inside the process (I-1 protocol A1-R4..R6 do).
- It does not guess: an observation that is not available is recorded as such and the corresponding check fails closed. What an external capture
  can never observe (in-process `CPROFILE` / `SECURELOAD` / `TRUSTEDPATHS`, `runtimeIdentities`, the human fields) is **not covered by the verdict** and is
  listed in `hostToConfirm` and in the `coverage` section of every record. No check can ever fail on those items, because they are not observed.

## 3. Files

| File | Role |
|---|---|
| `Capture-Phase2.ps1` | the capture (needs the PID of the started AutoCAD); exit 0 / 2 / 3 / 1 |
| `Validate-Phase2.ps1` | offline validator of a record; exit 0 valid / 2 invalid / 3 valid-only-as-test-hook / 1 usage |
| `lib/Phase2Common.ps1` | shared library: canonical JSON serializer and strict parser, closed JSON-Schema-subset validator, path / reparse-point guards, the single writer, transcript reader, declared-set / pin / folder checks, module baseline comparison, **the pure check evaluator** shared by the capture and the validator, read-only providers |
| `schemas/ct21d.phase2.v1.json` | record schema (draft 2020-12, `additionalProperties: false` everywhere, enums, patterns) |
| `schemas/ct21d.phase2.expected-inputs.v1.json` | schema of the independent expected-inputs file the CAD manager gives the validator |
| `allowed-commands.txt` | the allowed command list (deny by default), see 8 |
| `allowed-dotnet.txt` | the closed allowlist of the .NET surface (types, static members, instance member names, assignable members), see 8 |
| `tests/` | `Run-AllTests.ps1`, `Phase2.Tests.ps1`, `Phase2.Remediation.Tests.ps1`, `Phase2.Round2.Tests.ps1`, `StaticScan.Tests.ps1`, `StaticScan.Reproductions.Tests.ps1`, `StaticScan.Reproductions2.Tests.ps1`, `StaticScan.ps1`, `TestSupport.ps1` (no AutoCAD needed) |
| `HASHES-PHASE2.txt` | SHA-256 of every other file of this folder (`sha256  relative-path`, LF, sorted ordinal) |
| `.gitattributes` | `* -text`: the bytes are LF and are read as exact bytes |

## 4. How the CAD manager will run it (NOT run now)

Preconditions (all CAD-manager actions; the scripts check what they can and refuse otherwise): the hashes of this folder verified against
`HASHES-PHASE2.txt` (section 12); a fresh, **empty** evidence child root `...\evidence\<runId>` created by the CAD manager (its last component **is** the run
id; the record and its sidecar belong in the **sealed evidence set** of that run, package v3 3.8/3.9, and the seal hashes everything in the root; **the capture refuses a root that
already holds anything, hidden files included, so a transcript, a baseline or any other input file must live OUTSIDE the evidence root until after the capture**);
exactly one `acad.exe` started in the designated profile by the same Windows account, **non-elevated**, with no `/p` switch unless it names the
expected profile and no `/b` or `/ld` switch (they are recorded, not judged); **no designation file in the declared-set folder** (see "Between sessions" below);
no designation file placed yet (package v3 3.7 steps 1-3); PowerShell 7 started by the same account, non-elevated.

```powershell
# 1. Capture (typed once; the PID is the one of the started acad.exe, e.g. from Task Manager or (Get-Process acad).Id)
pwsh -NoProfile -File .\Capture-Phase2.ps1 `
     -ProcessId <pid> -Session S1-A -SessionId <the sessionId the CAD manager assigned in phase 1> `
     -RunId HGP-H1-<yyyymmddThhmmssZ>-<nn> `
     -EvidenceRoot 'D:\I52-CT21D-HOST\evidence\HGP-H1-<yyyymmddThhmmssZ>-<nn>' `
     -AllowedEvidenceParent 'D:\I52-CT21D-HOST\evidence' `
     -DeclaredSetJson 'D:\I52-CT21D-HOST\canonical-642ba392\declared-set.json' `
     -ExpectedDeclaredSetSha256 <declaredSetSha256 of the signed tuple record, 64 lowercase hex> `
     -ExpectedAcadSha256 <acad.exe SHA-256 of the signed tuple record> `
     -ExpectedProfile '<<Unnamed Profile>>' `
     [-TranscriptPath <LOGFILEON text or typed-command transcript>] [-RequireTranscript] `
     [-ModuleBaselineJson <baseline.json> -ExpectedModuleBaselineSha256 <its hash in the signed phase 1 record>] [-RequireModuleBaseline]

# 2. Offline validation. -ExpectedBuildTupleInputs, -HashesLedger, -DeclaredSetJson and the sidecar next to the record are MANDATORY:
#    the checks are re-derived from the record's own data, so a forged but self-consistent record passes them; only the independent
#    material (expected inputs from the SIGNED phase 1 record, the reviewed tool hashes, the sidecar the capture wrote, the declared-set folder
#    listed AGAIN now) can catch it. Run it BEFORE the designation is placed (package v3 3.7 step 4): see "The validation instant" below.
pwsh -NoProfile -File .\Validate-Phase2.ps1 'D:\I52-CT21D-HOST\evidence\<runId>\phase2-record-<runId>.json' `
     -Schema .\schemas\ct21d.phase2.v1.json -ExpectedBuildTupleInputs <expected-inputs.json> -HashesLedger .\HASHES-PHASE2.txt `
     -DeclaredSetJson 'D:\I52-CT21D-HOST\canonical-642ba392\declared-set.json' [-ModuleBaselineJson <baseline.json>] [-DesignationJson <composed run-designation.json, staged OUTSIDE the declared-set folder>]
```

`Validate-Phase2.ps1` refuses to run (exit 1) without `-ExpectedBuildTupleInputs`, `-HashesLedger` and `-DeclaredSetJson`, and its result is INVALID without the
`.sha256` sidecar next to the record. The paths of the record, `-DeclaredSetJson`, `-ModuleBaselineJson` and `-DesignationJson` must be absolute local fixed-drive paths without
reparse points (the same guard as the capture; otherwise exit 1). `-ModuleBaselineJson` (recomputation of the comparison) and `-DesignationJson` (the composed designation file
carries the same `sessionId` and `runId` as the record) are optional and each adds a check the record alone cannot give; use both. The `-Schema` file and the expected-inputs schema are read as
bytes, hashed, and **pinned**: the schema hash must equal its line in the ledger **and** the `schemaSha256` the capture recorded (checks `SCHEMA_FILE_EQUALS_LEDGER_AND_RECORD`,
`EXPECTED_INPUTS_SCHEMA_EQUALS_LEDGER`), so a validator run against an empty or edited schema is INVALID. The validator prints `LEDGER_SHA256 <hash>`: the ledger cannot list itself, so
compare that value with the one reported in the review hand-off (an out-of-band value).

**The validation instant (R3-2).** `OFFLINE_DECLARED_FOLDER_EXACT_AND_UNCHANGED_NOW` lists the declared-set folder again at validation time and requires exactly the recorded
entries (no unexpected entry, none missing, the listing equal to the recorded one). A `run-designation.json` written into the folder between the capture and the validation therefore makes
the validation INVALID; re-validating after the designation was legitimately placed (step 4) fails by design. Keep the composed designation outside the folder until phase 2 validates.

`expected-inputs.json` (schema `ct21d.phase2.expected-inputs.v1`) is written by the CAD manager from the signed phase 1 record, never from the capture:
`{"schema":"ct21d.phase2.expected-inputs.v1","session":"S1-A","sessionId":"...","runId":"...","declaredSetSha256":"...","acadExeSha256":"...","profileExpected":"...","requireTranscript":false,"requireModuleBaseline":false,"moduleBaselineSha256":null}`.

After exit 0 from both, the CAD manager records the SHA-256 of `phase2-record-<runId>.json` (also in `phase2-record-<runId>.json.sha256`) in the human
phase 2 record (`validatedBy`, `validatedAtUtc`) and **fills the fields this verdict does not cover** (section 5, "What the verdict covers") before
`validationResult = VALID`; only then continues with package v3 3.7 step 4. **Any exit other than 0 is a stop condition (package v3 section 5, items
14-16): nothing is repaired on the fly, no retry without the Coordinator's written authorization. Exit 3 (test hook) is never an attestation.**

**Between sessions.** `DECLARED_FOLDER_CONTENT_EXACT` requires the declared-set folder to hold exactly the DLLs and their `.pin` files. After a session in which
the CAD manager placed a designation file next to the DLL (package v3 3.7 step 4), that file must be removed (or the folder restored to its hashed state) by
the CAD manager **before the next session's phase 2**, otherwise that phase 2 is INVALID by design. The tool never removes anything.

**The two identifiers.** `tupleInputs.sessionId` is the identifier the CAD manager assigns in phase 1 (package v3 3.5.3, OI-6); it is a mandatory input and
is the one the designation file and the instruments carry. `captureBinding.captureBindingId` is a hash derived from the observed process (pid, start time, run id);
it exists only after the process starts, it is **not** the assigned sessionId, and nothing downstream should compare the two. The validator compares the assigned
one with the independent expected inputs and (optionally) with the composed designation file.

### Inputs

| Parameter | Meaning |
|---|---|
| `-ProcessId` | PID of the **already started** AutoCAD (positive integer; a missing, non-numeric or non-running PID is refused) |
| `-Session` | `S1-A`, `S1-B`, `S1-C`, `S2`, `S3`, `S4` (matched **ordinally**: `s1-a` is refused at the parameter check); the run id activity number must match (decisions 247: H1, H2, H3, H8, H9, H4) |
| `-SessionId` | **mandatory**: the sessionId assigned by the CAD manager in phase 1 (`^[A-Za-z0-9][A-Za-z0-9._:-]{0,127}$`; the template placeholder `<assigned at authorization>` is refused) |
| `-RunId` | `HGP-H<n>-<yyyymmddThhmmssZ>-<nn>` |
| `-EvidenceRoot` | existing, **empty** local fixed-drive folder whose last component equals the run id; no UNC / device path, no relative path, no `..`, no stream, no reparse point in the path or its ancestors |
| `-AllowedEvidenceParent` | **mandatory (round 4)**: the folder the CAD manager names as the parent of every evidence root (`D:\I52-CT21D-HOST\evidence`). It must be an existing local fixed-drive folder without reparse points, and `-EvidenceRoot` must be its **direct child**; a root anywhere else (another folder, a grandchild, another drive) is refused before anything is read. This binds the write target to a place given independently of the run id the operator types |
| `-DeclaredSetJson` | `declared-set.json` (array of `{path, sha256}`; all DLLs in one folder) |
| `-ExpectedDeclaredSetSha256`, `-ExpectedAcadSha256`, `-ExpectedProfile` | values of the signed phase 1 tuple record; mandatory (no default, no prompt) |
| `-TranscriptPath` | optional text file; hashed read-only, load-command lines extracted verbatim |
| `-RequireTranscript` | makes a missing transcript a failed check (`REQUIRED_NOW`) |
| `-ModuleBaselineJson`, `-ExpectedModuleBaselineSha256` | optional pair: the phase 1 module baseline (strict JSON array of `{path, sha256}`, absolute drive-letter paths, hashed file by file) and its hash from the signed phase 1 record; both or neither |
| `-RequireModuleBaseline` | makes a missing baseline a failed check **in S1-A too**. For every other session the baseline is mandatory whatever this flag says (round 2: the S1-A exception is encoded, see 5); the flag matters only for S1-A |
| `-ProfilesSubKey` | HKCU subkey whose default value is the current profile; default `Software\Autodesk\AutoCAD\R25.0\ACAD-8101:409\Profiles` |

### Outputs (create-new, flat, UTF-8 without BOM, LF)

`phase2-record-<runId>.json` (canonical JSON, sorted keys, 2-space indent, one trailing LF) and `phase2-record-<runId>.json.sha256`
(`<sha256>  <name>` + LF). Standard output echoes the record path, its hash, the assigned sessionId, the capture binding id, every check, the coverage
line and the verdict. Exit codes of the capture: **0** valid, **2** invalid (record kept as evidence), **3** test hook record (never a valid attestation), **4** partial write (the record was written, its sidecar was not; see 6), **1** usage or refusal (raised before the record write: nothing written).
Both files are written by `Write-Phase2RecordAndSidecar`, the only caller of the writer.

## 5. The record and its checks

Top level: `recordKind`, `schema = ct21d.phase2.v1`, `governing = false`, `phase = 2`, `session`, `runId`, `captureBinding`, `tool`, `testHook`,
`tupleInputs`, `process`, `acadBuild`, `profile`, `modules`, `moduleBaseline`, `profileAtRest`, `processList`, `pinCheck`, `transcript`, `coverage`,
`hostToConfirm`, `validation`, `volatile`.

- **tupleInputs.sessionId** = the CAD-manager-assigned identifier (input). **captureBinding.captureBindingId** = first 16 lowercase hex of SHA-256 over
  `"CT21D-PHASE2-CAPTURE-BINDING-V1" LF pid LF processStartUtc LF runId LF` (UTF-8). The process start time is `Process.StartTime` converted to UTC,
  `yyyy-MM-ddTHH:mm:ss.fffZ` (the same form the instruments record).
- **tool** = SHA-256 of `Capture-Phase2.ps1`, of `lib/Phase2Common.ps1` and of the schema, plus the PowerShell version. They are hashed at the start and again just
  before writing; a change in between makes the capture refuse (exit 1).
- **volatile** = `capturedAtUtc`, `evidenceRoot`, `moduleSigners` (the Authenticode status and subject of each module, which can depend on revocation lookups) and
  `secondsSinceProcessStart` (whole seconds between the process start and the capture; **information for the CAD manager, in no check**: "immediately after the session starts"
  is not enforced, a long delay is made visible): the only fields declared volatile. Two captures of the same state are byte-identical apart from them.
- **pinCheck.entries[].fileStatus** is `OK`, `MISSING`, `UNREADABLE`, `REPARSE` or `NOT_LOCAL`, and **pinCheck.folder.status** may be `NOT_LOCAL_OR_REPARSE`: the declared-set paths get the
  same local fixed-drive and reparse guards as the evidence root and the input files (a mapped network drive letter, a removable drive or a cloud placeholder is not read or hydrated).
- **modules** = every module of the process, sorted by lowercase path (ordinal), each with path, SHA-256 and file version; plus the subset under the declared-set
  folder (must be empty at phase 2).
- **moduleBaseline** = status (`PROVIDED`, `NOT_PROVIDED`, `UNREADABLE`, `MALFORMED`), the baseline file hash and entry count, and `outsideBaseline`: every loaded module
  whose lowercase path is not in the baseline (`NOT_IN_BASELINE`) or whose hash differs (`HASH_DIFFERS`). A module outside the baseline is the stop condition of 3.5.3.
- **process.commandLineSwitches** = the `/x` switches of the command line after the executable token (lower case, unique, sorted). `/b` (startup script) and `/ld`
  can run code at start-up; they are recorded and listed in `hostToConfirm` (`STARTUP_SWITCHES_RECORDED`), judged by no check.
- **profileAtRest** (informational, in no check) = the values of `SECURELOAD` and `TRUSTEDPATHS` stored in the profile registry key
  `<ProfilesSubKey>\<effective profile>\Variables`, read with a read-only `OpenSubKey(..., $false)` + `GetValue` + `GetValueKind`. They are the AT-REST values, not the
  in-process ones (those stay HOST-TO-CONFIRM). Not attempted when there is no effective profile or its name contains a path separator.
- **processList** = every process (pid, name, start time or `UNREADABLE`) sorted by pid, with `related` flagging the target name, `acad*`, and a list of
  AutoCAD / Autodesk helper names.
- **coverage** = what the verdict does and does not cover (below).

### What the verdict covers

The verdict `PHASE2_EXTERNAL_OBSERVATIONS_VALID` means: every external observation this tool makes is consistent with the independent expected inputs. It is **not**
the template 3.5.3 validation. `coverage` states, in every record: `verdictCovers = EXTERNAL_OBSERVATIONS_OF_THIS_TOOL_ONLY`, `templateComplete = false`,
`moduleBaselineComparison = COMPARED | NOT_COMPARED`, `sessionProfileFacts = HOST-TO-CONFIRM` (`CPROFILE`, `SECURELOAD`, `TRUSTEDPATHS` as seen inside the process),
`runtimeIdentities = HOST-TO-CONFIRM`, `pinCheckAfterLoad = HOST-TO-CONFIRM` (round 3: the record's `pinCheck` is an at-rest, pre-load check; the template's after-load pin check (AFTER_NETLOAD in the template) is not covered and `hostToConfirm` names it), `humanFields = CAD_MANAGER`. **The S1-A exception is encoded (round 2).** When the baseline was not provided, `NOT_COMPARED` is recorded explicitly and `MODULE_BASELINE_COMPARISON` passes **only for S1-A**
(which produces the SESSION_START list that becomes the baseline). For `S1-B`, `S1-C`, `S2`, `S3` and `S4` a record without a provided and compared baseline FAILS that check and the verdict is
`PHASE2_INVALID`, whatever the `-RequireModuleBaseline` / `requireModuleBaseline` switches say, so the plain valid verdict can never carry `NOT_COMPARED` outside S1-A. If the Coordinator
wants the comparison optional for later sessions, that is a code change for review (open question 7), not a flag.

The 24 named checks, in this fixed order, each `PASS` or `FAIL`; the verdict is `PHASE2_EXTERNAL_OBSERVATIONS_VALID` only if every one passes:

`PROCESS_NAME_IS_EXPECTED` (acad) · `SINGLE_TARGET_PROCESS` (exactly one process of that name, and it is the PID) · `PROCESS_OWNER_IS_INVOKING_USER` ·
`ACAD_BUILD_OBSERVED` · `ACAD_SHA256_EQUALS_TUPLE` · `PROFILE_EFFECTIVE_OBSERVED` · `PROFILE_SOURCES_CONSISTENT` · `PROFILE_EQUALS_EXPECTED` ·
`MODULES_ENUMERATED` · `MODULE_HASHES_READABLE` · `MODULE_INVENTORY_SORTED_UNIQUE` · `MAIN_MODULE_IN_INVENTORY` · `NO_DECLARED_SET_MODULE_LOADED` ·
`PROCESS_LIST_ENUMERATED` · `DECLARED_SET_JSON_WELL_FORMED` · `DECLARED_SET_SHA256_EQUALS_TUPLE` · `DECLARED_SET_SINGLE_FOLDER` ·
`DECLARED_FOLDER_CONTENT_EXACT` (the folder holds exactly the DLLs and their `.pin`, no designation yet, no subfolder, no reparse point) ·
`DECLARED_DLL_HASHES_EQUAL_ENTRY` · `PIN_FILES_EQUAL_DLL_AND_ENTRY` (each `.pin` is 64 lowercase hex + LF and equals the DLL hash and the entry) ·
`TRANSCRIPT_REQUIREMENT_MET` · `TRANSCRIPT_CONSISTENT_WITH_SESSION` (a load command or AutoLISP load form in the transcript fails in **every** session: phase 2 precedes any load, R3-2) ·
`SESSION_ID_ASSIGNED_WELL_FORMED` · `MODULE_BASELINE_COMPARISON`.

Load-command detection (`Get-Phase2LoadLines`, round 2): a line is a load line when, case-insensitively and bounded by "not a letter or digit" (**not** the regex word-boundary escape, because `_` is a word character
and AutoCAD echoes ribbon / menu commands as `_NETLOAD`), it holds one of `NETLOAD`, `APPLOAD`, `ARXLOAD`, `ARXUNLOAD`, `DBXLOAD`, `VLLOAD`, `FASLOAD`, `VLRUN`, `VBALOAD`, `VBARUN`, `STARTAPP`
(so `Command: _NETLOAD`, `_netload`, `._netload`, `-NETLOAD`, `_.netload`, `^C^C_netload`, `(command "_netload" ...)`, `_APPLOAD`, `_ARXLOAD`, `_-VBARUN` are all detected); one of `SCRIPT`, `RUNSCRIPT`, `RSCRIPT`
as a command echo (after `_`, `.`, `"`, `Command:` or at the start of the line, so prose such as "Enter script file name" is not a hit); or one of the forms `(load`, `(arxload`, `(autoload`, `(vl-load-all`, `(vl-vbaload`,
`(vl-vbarun`, `(startapp`. This is a **closed list of known commands**: a detection aid for the CAD manager's own review of the transcript, **not proof** that no code was loaded (a concatenated or
indirect form is not recognized). The static-scan rule `NETLOAD_OUTSIDE_TRANSCRIPT_READER` uses the same words (with the script words only after `_` or `.`), and a parity test runs a shared corpus through both. The
transcript semantics: `NOT_PROVIDED` is recorded explicitly with `requirement = NOT_APPLICABLE_S1A_NO_LOAD_STEP` for S1-A (absence is expected), `REQUIRED_LATER`
for the other sessions at phase 2 time, `REQUIRED_NOW` with `-RequireTranscript`.

The validator adds (all `CHECK` lines): `BYTES_STRICT_JSON`, `SCHEMA`, `CANONICAL_FORM`, `SIDECAR_EQUALS_RECORD_SHA256`, `TEST_HOOK_NOT_IN_PRODUCTION_RECORD`,
`EXPECTED_PROCESS_NAME_RULE`, `CAPTURE_BINDING_DERIVATION`, `COVERAGE_CONSISTENT`, `RUN_ID_MATCHES_SESSION`, `EVIDENCE_ROOT_LEAF_IS_RUN_ID`,
`CHECK_LIST_COMPLETE_AND_ORDERED`, `CHECKS_RECOMPUTED_EQUAL`, `VERDICT_CONSISTENT`, `RECORD_<each of the 24>`, `EXPECTED_SESSION`, `EXPECTED_SESSION_ID`, `EXPECTED_RUN_ID`,
`EXPECTED_DECLARED_SET_SHA256`, `EXPECTED_ACAD_SHA256`, `EXPECTED_PROFILE`, `EXPECTED_REQUIRE_TRANSCRIPT`, `EXPECTED_REQUIRE_MODULE_BASELINE`,
`EXPECTED_MODULE_BASELINE_SHA256`, `EXPECTED_MODULE_BASELINE_PROVIDED` (when required), `TOOL_IDENTITY_IN_HASHES_LEDGER`, `VALIDATOR_IDENTITY_IN_HASHES_LEDGER`
(this `Validate-Phase2.ps1` and the library it loaded equal the ledger), and when the optional inputs are given `OFFLINE_DECLARED_SET_UNCHANGED_SINCE_CAPTURE`,
`OFFLINE_DECLARED_FOLDER_EXACT_AND_UNCHANGED_NOW`, `OFFLINE_MODULE_BASELINE_RECOMPUTED`, `DESIGNATION_SESSION_ID_EQUALS_RECORD`, `DESIGNATION_RUN_ID_EQUALS_RECORD`; with the ledger, also
`SCHEMA_FILE_EQUALS_LEDGER_AND_RECORD` and `EXPECTED_INPUTS_SCHEMA_EQUALS_LEDGER` (a missing schema hash fails closed).

**Additions to package v3 3.5.3, for the Coordinator to adopt or strike.** Two things in this tool are not in the template: the check `PROCESS_OWNER_IS_INVOKING_USER` (the `acad.exe` owner equals the
account running the capture) and the informational `profileAtRest` section (at-rest `SECURELOAD` / `TRUSTEDPATHS` from the profile registry key; layout from `cad-manager-attestation-and-readback.md` B.3; in no
check). Both are honestly labelled in the record; neither is required by the package. Also recorded and judged by nothing: `process.commandLineSwitches` and `volatile.secondsSinceProcessStart`.

## 6. Fail-closed semantics

| Situation | Result |
|---|---|
| usage error, missing / malformed parameter (including the `-SessionId` and the baseline pair; `-Session` is matched ordinally), unknown parameter, run id / session mismatch, root outside the rules, **root not empty**, output already exists (record **or** sidecar), unreadable process start time, PID not running, process changed or ended during the capture, a tool file changed during the capture, internal schema violation | **refusal**, message on stderr, exit **1**, **nothing written** |
| the record was written but its `.sha256` sidecar could not be (ACL, disk error, a name taken in between) | `PARTIAL WRITE` on stderr, exit **4**. The record stays (nothing is ever deleted); there is no sidecar, so the validator flags it. The run is **failed**: the root now holds an entry, so it cannot be reused (the next attempt is refused as non-empty); keep it as evidence and start a new run id with the Coordinator's written authorization. Exit 1 therefore means "nothing written" only for refusals raised before the record write. |
| any observation that is missing or different from the tuple (wrong name, two acad, hash mismatch, pin mismatch, extra file, profile unobservable, module outside the baseline, ...) | the record is **written** with the failing checks and `PHASE2_INVALID` (the evidence is kept), exit **2** |
| every check passes | record written, `PHASE2_EXTERNAL_OBSERVATIONS_VALID`, exit **0** |
| every check passes but the test hook was active | record written, `PHASE2_TEST_STANDIN_OBSERVATIONS_OK`, exit **3** (tests only) |

`Validate-Phase2.ps1` re-derives the whole check list **from the record's own data** with the same pure function, so a record whose results were edited,
reordered, re-formatted, re-hashed or given a different verdict is invalid. It also checks strict bytes (no BOM, no CR, no duplicate keys, integers only), the sidecar,
the closed schema, canonical form, the capture binding derivation, the run id / session rule, the independent expected inputs, and the tool identity (capture, validator and
library) against `HASHES-PHASE2.txt`. **A fully forged but self-consistent record passes the re-derived checks**; the independent expected inputs, the ledger and the sidecar are
what catches it, which is why they are mandatory (section 4).

### Test hook (documented; refused in production)

The environment variable `CT21D_PHASE2_TEST_STANDIN` (a process name, `^[A-Za-z0-9_.-]{1,64}$`) lets the **tests** use a stand-in process. Without it the
process name must be `acad`. With it the record carries `testHook.active = true`, can never reach `PHASE2_EXTERNAL_OBSERVATIONS_VALID` (the best verdict is
`PHASE2_TEST_STANDIN_OBSERVATIONS_OK`), the capture exits **3** (not 0) and `Validate-Phase2.ps1` exits **3** (not 0) and rejects the record unless the same variable is set to
the same name. A wrapper that only tests `exit == 0` therefore never accepts a stand-in record. The CAD manager must not set it.

## 7. Residual limits (what an external capture cannot observe, and what the checks do not prove)

- The profile name **seen from inside** the running AutoCAD (`CPROFILE`), `SECURELOAD` and `TRUSTEDPATHS` of the process: typed reads of the I-1 protocol
  (A1-R4..R6). The effective profile here is the command-line `/p` value (read after the executable token), else the registry default value at capture time. A profile
  switch after start-up, a `/p` pointing at an `.arg` file, or a key that does not exist fail closed. The at-rest registry values are recorded as information only.
- Hashes are of the **files on disk** named by the loader, not of the mapped images (as the instruments' self-pin and the R0 README already state).
- The Authenticode status can involve certificate-revocation lookups (a **network read**) and the CryptoAPI layer may write its cache **under the invoking user's profile** (a file side effect outside the evidence root, made
  by the operating system); it is informational, in no check, and recorded in `volatile`. Accept it or drop the column; it costs most of the run time (open question 6).
- **Residual side effects found by the round 3 review (round 4, documented, not removable by the scripts).** (1) `Get-AuthenticodeSignature` may perform **network revocation lookups** (CRL / OCSP) and the CryptoAPI layer
  **writes its URL cache under the invoking user's profile** (`%USERPROFILE%\AppData\LocalLow\Microsoft\CryptnetUrlCache`), outside the evidence root; that is the operating system acting on the signature check, not a script write. (2) `pwsh` itself
  and the CIM / WMI providers keep their own caches and logs (the PowerShell module-analysis cache and telemetry files under the user profile, the WMI repository and event logs, `Microsoft-Windows-PowerShell/Operational` entries); none of it
  is written by script code and none of it can be turned off from inside the script. (3) The evidence root only has to be an existing empty directory named like the run id; to stop an operator typo from placing it anywhere, `-AllowedEvidenceParent` is now a **required** parameter of the capture (see Inputs): the root must be a direct child of
  the parent folder the CAD manager gives, and a parent that is missing, relative, UNC, a device path or a reparse point, or a root that is not its direct child, is refused with nothing written. The parent folder is still created and
  protected by the CAD manager (the script creates no folder); the check is a guard against a wrong path, not against a hostile operator who types both parameters.
- A module enumeration needs a non-elevated 64-bit PowerShell by the same account (otherwise `MODULES_ENUMERATED` fails closed).
- **Command resolution depends on `PSModulePath` (round 3, review 1 m4).** `Get-CimInstance`, `Invoke-CimMethod` and `Get-AuthenticodeSignature` are auto-loaded from whichever module path wins; `-NoProfile` does not stop a
  user-scope module from shadowing them. The scripts do not pin the module or check `(Get-Command).Source`. Residual, accepted for this review: the CAD manager runs the scripts from a clean profile (`pwsh -NoProfile`, no
  user-scope module of those names), and the out-of-band comparison of the ledger covers the scripts, not the modules of the machine. Recording the module of each command in the record is a possible later change.
- **`processList.related` is informational only (round 3, review 2).** Processes named `acadlt`, `accoreconsole` and Autodesk helper names are listed in `related` but no check reads them; `SINGLE_TARGET_PROCESS` counts only
  the name exactly `acad`, which is the scope of the `otherAcadProcess` attestation.
- **S1-A needs a declared set (round 3, review 2).** The `DECLARED_SET_*` and `PIN_FILES_*` checks are mandatory in every session, S1-A included (`instrumentBuild` is phase 1 for every session in 3.5.3), although S1-A loads no
  instrument. S1-A cannot reach `PHASE2_EXTERNAL_OBSERVATIONS_VALID` until a declared set has been built and pinned. In S1-A without a baseline, a copy of a declared-set DLL loaded from *another* folder is not detected
  (`NO_DECLARED_SET_MODULE_LOADED` looks under the declared-set folder only); from S1-B on the mandatory baseline covers it.
- **Timing is measured, not enforced (round 3, review 2).** `volatile.secondsSinceProcessStart` is information; the spec gives no bound for "immediately after the session starts".
- **The `sessionId` pattern is stricter than the designation schemas (round 3, review 2; open question 11).** The capture accepts `^[A-Za-z0-9][A-Za-z0-9._:-]{0,127}$`; the designation schema accepts any non-empty string. It is not relaxed here
  because the real format is the CAD manager's, not invented by this tool.
- **Detection, not prevention (tool identity).** The tool identity in the record (`scriptSha256`, `librarySha256`, `schemaSha256`) is self-reported by the script that ran and is compared with the ledger only later, by the
  validator. The capture does not refuse to run when its own hash is not in the ledger; a modified tool would write a record that the validator (itself pinned to the ledger) then rejects.
- **Write race on the evidence root.** `Write-Phase2NewFile` checks the reparse chain of the root before the create-new, and again after the write; between the two checks a directory swapped for a junction by a local hostile
  actor could redirect one write. The post-write check detects that and refuses; it cannot undo it. This needs a hostile local actor on the machine; it adds to the `subst` residual below.
- **Declared-set and baseline inputs** are read after the local-drive and reparse guards (round 2); the folder listing is repeated at validation time. A cloud placeholder or network letter is not read.
- Between the observations a hostile local actor could change files; the single capture is not atomic. The process identity is re-checked at the end
  (same pid, name and start time), and the declared-set files are hashed once. The tool files are hashed at the start and again before writing; this detects a change
  during the run, it cannot prove that the text loaded at the start equals the bytes hashed at the start.
- `Assert-Phase2LocalPath` accepts any drive whose `DriveType` is `Fixed`; a `subst`-ed virtual drive can report `Fixed` and the reparse-point chain check does not cover it.
  The CAD manager creates the evidence root and must not use a `subst` drive.
- The `.pin` and the declared set are checked from outside; a DLL replaced together with its pin and declared-set entry is excluded only by the CAD
  manager's hash table (package v3 3.3) and the tuple's `declaredSetSha256`, which is exactly why that value comes from the signed record.
- Run time grows with the number of modules (Authenticode status per module); about 40 s for 415 modules on the development machine, so expect one to a few minutes for AutoCAD.
- Documents/databases identity (`runtimeIdentities`) exists only inside the process and no instrument is loaded at phase 2.
- The symbolic-link branch of the reparse guard is not exercised by the tests (no privilege on the development machine); the junction branches (root and ancestor) are.

## 8. Allowed commands, the allowed .NET surface and the static scan

**What is guaranteed.** `tests/StaticScan.ps1` has five layers (commands, .NET surface, comment channel and call sites, regular expressions, write targets and function definitions) over the three production scripts:

1. **Commands** (`allowed-commands.txt`, deny by default): every command, alias or dynamic command not listed is a finding. For the commands that matter the **parameters are read
   from the AST** (never from the command text, so a comment cannot fake them): `Get-CimInstance` only `-ClassName Win32_Process` (+ `-Filter`), `Invoke-CimMethod` only `-InputObject`
   and `-MethodName GetOwner`, `Get-ChildItem` only `-LiteralPath` (+ `-Force`), `Get-AuthenticodeSignature` only `-LiteralPath`, `New-Object` only a literal type on the .NET list
   (and a `FileStream` only as `Open`+`Read`, or `CreateNew`+`Write` inside the writer), `ForEach-Object` / `Where-Object` only with a script block literal (no `-MemberName`), no splatting.
2. **.NET surface** (`allowed-dotnet.txt`, deny by default): every type literal, static member, instance member name (method or property), and assigned member name must be
   listed; a dynamic member name, a static call on something that is not a type literal (`$t::X`), a string-typed `-as` / `-is`, `using` / `class`, a write to a provider-drive variable
   (`${env:X} = 1`), a write to a static member, and `$ExecutionContext` / `$Host` / `$PSCmdlet` / `$MyInvocation` are findings. The list is checked **in both directions** by the
   tests (nothing unlisted is used; nothing listed is unused) and was reviewed line by line: it contains no way to start a process, write the registry or a file other than through the single
   writer, change an ACL, load code or reach the network. `Write` and `Flush` are scoped to `Write-Phase2NewFile`.
3. **Comment channel and call sites (round 2).** (a) Every `#requires` marker anywhere in the raw text of a production script (line comments, text after code on the same line, block comments and strings, conservatively)
   must be exactly `#Requires -Version 7.0`, and `ScriptRequirements` must name no module, assembly, edition, application id, elevation or other version (`REQUIRES_NOT_EXACT`, `REQUIRES_NOT_ALLOWED`): a
   `#Requires -Modules <path>` is a comment to the scan but makes pwsh import and run that module before the script body starts. (b) **The calls of the single writer are pinned**: only
   `Write-Phase2RecordAndSidecar` may call `Write-Phase2NewFile`, with exactly two call texts, and the production set must contain exactly two calls (`WRITER_CALL_NOT_PINNED`, `WRITER_CALL_COUNT_NOT_PINNED`); the writer
   itself also refuses any root whose leaf is not a run id, and the pair function requires the leaf to equal the run id. (c) `switch -File`, `data` and `configuration` blocks and `New-Object` of `Process` /
   `ProcessStartInfo` are findings.
4. **Regular expressions** over the comment-blanked source (strings are not blanked, so a forbidden token inside a string is a finding): a drift guard and a second net (process start,
   registry writes in both `.X(` and `::X(` form, file-mutating members, `FileMode` values, `CreateNew` / `FileAccess.Write` outside the writer, `ScriptBlock` / `InvokeScript` /
   `ExecutionContext` / reflection words, network types, environment writes, ACL tools, load-command words outside `Get-Phase2LoadLines`, ...).
5. **Write targets, automatic variables, the write wrapper, function definitions, FileStream arity (round 3).** (a) *Every write target of a statement is read from the AST*: assignment
   (`=`, `+=`, ...), multi-assignment (`$a, $f[0].Attributes = 0, 1`, also parenthesized and nested), index targets (`$o.Items[0] = 1`), casts and parentheses around the target, and `++` / `--` (prefix and
   postfix). A member target of an `=`-style assignment must be a non-static member on the `[assignable]` list; **any** `++` / `--` on a member (or on an index of a member) is a finding
   (`INCREMENT_OF_MEMBER_FORBIDDEN`, `MEMBER_ASSIGNMENT_NOT_ALLOWED`, `STATIC_ASSIGNMENT_FORBIDDEN`). (b) *A write to an automatic variable that redirects code loading or the environment* is a finding
   (`FORBIDDEN_VARIABLE_ASSIGNMENT`): `PSScriptRoot`, `PSCommandPath`, `pwd`, `args`, `input`, `home`, `pid`, `profile`, `PSVersionTable`, `PSBoundParameters`, `PSEdition`, by assignment, multi-assignment, `++`,
   a `foreach` variable or a parameter of that name, with or without a `script:` / `global:` / `local:` / `private:` prefix. The dot-source of the library is still pinned by its exact text; with
   `$PSScriptRoot` unassignable in all three scanned scripts (the library runs in the same scope) that text can only load `lib/Phase2Common.ps1` next to the script that contains it. (c) *The public write wrapper*
   `Write-Phase2RecordAndSidecar` may be called only at the top level of a file named `Capture-Phase2.ps1`, with the single call text of the capture, and the production set must contain exactly one call
   (`WRITER_WRAPPER_CALL_NOT_PINNED`, `WRITER_WRAPPER_CALL_COUNT_NOT_ONE`); the two inner `Write-Phase2NewFile` calls stay pinned as in round 2. (d) *Function definitions*: outside `lib/Phase2Common.ps1` the only
   function a production script may define is its own exit helper (`Exit-Phase2Usage` in the capture, `Exit-Phase2ValidateUsage` in the validator); a function whose name is defined in the library is
   `FUNCTION_REDEFINES_LIBRARY_FUNCTION`, any other is `FUNCTION_DEFINITION_NOT_ALLOWED`; inside the library a function with the name of a listed command (`FUNCTION_SHADOWS_LISTED_COMMAND`) or one defined
   twice (`FUNCTION_DEFINED_TWICE`) is a finding. (e) *`FileStream`* takes exactly 3 arguments, or 4 with a `FileShare` literal (or a `-bor` of literals) as the fourth: no buffer-size or `FileOptions` overload
   (`FILESTREAM_ARGS_NOT_ALLOWED`). The unit tests scan their snippets with a test-only switch (`-AllowFixtureFunctions`) that skips rule (d) for fixtures that must define pinned library functions; the command line mode
   and the production-set scan never pass it, and the round 3 fixtures are scanned without it.

6. **Scoped automatic variables, `[ref]`, common parameters, advanced functions (round 4, review of the round 3 scan).** (a) The forbidden automatic variables (`$ExecutionContext`, `$Host`, `$PSCmdlet`, `$MyInvocation`,
   `$PSDefaultParameterValues`, `$PSHOME`, `$ShellId`) are matched **after stripping the scope prefix** (`script:`, `global:`, `local:`, `private:`, `using:`), so `$global:Host` or `$script:PSCmdlet` is a finding (`FORBIDDEN_AUTOMATIC_VARIABLE`);
   *any* mention of `$PSDefaultParameterValues` is a finding (assigning the variable, an index or a member of it, replacing the table, or passing it on), and it is also in the forbidden-assigned list. (b) `[ref]` (and its long name
   `System.Management.Automation.PSReference`) is allowed **only** as the cast of the plain local variable `$l`, which is what the two `TryGetInt64` calls of the library use; `[ref]` of any other variable (an automatic one with or without a scope prefix,
   a member, an index, a parenthesized expression) and a `[ref]` parameter type are `REF_NOT_ALLOWED`. (c) A common parameter that writes a **named variable or buffer** (`-ErrorVariable`, `-WarningVariable`, `-InformationVariable`,
   `-OutVariable`, `-PipelineVariable`, `-OutBuffer`, the aliases `-ev -wv -iv -ov -pv -ob`, and any prefix of two or more letters such as `-OutVar`) is `COMMON_VARIABLE_PARAMETER_FORBIDDEN` on **every** command call, library functions
   included; a one-letter prefix (`-p`) is a finding on every command except calls of the library's own functions (where `-P` is a declared parameter). (d) An attribute (`[CmdletBinding()]`, `[Parameter()]`, ...) is allowed only in the
   script-level `param` block of the script itself; inside any function or nested script block it is `ADVANCED_ATTRIBUTE_OUTSIDE_SCRIPT_PARAM`, so no library function is an advanced function and the common parameters cannot become live
   there. The production scripts' own script-level `[CmdletBinding()]` / `[Parameter(Position = 0)]` stay (the CAD manager is the caller). `Validate-Phase2.ps1` now also passes `-Schema`, `-ExpectedBuildTupleInputs` and
   `-HashesLedger` through the same input-file guard as the record (local, fixed drive, no UNC / device path, no reparse point).

**Honest limit of the write-target rule.** It covers the syntactic write forms of the language (assignment, multi-assignment, `++` / `--`, `foreach` variable, parameter). A write performed by a *method* of an
object (for example a method that sets an attribute) is covered only by the member-name allowlist (a method name that is not listed is a finding), not by this rule; and the property **reads** and method names that
are listed remain allowlisted by name, not per type. The reviewers' scan harness could not find another syntactic write form after this round, but that is evidence, not proof.

**What is NOT guaranteed (stated plainly).** The scan is a *guard that makes a deviation visible*; it is not a proof of the absence of side effects. The proof is the **line-by-line human
review of the three production scripts** (the first reviewer found no violation in them). The residual of layer 2 is that member names are allowlisted **by name, not per type**: a name on
the list is allowed on every object (for example `Dispose`, `Read`, `Add`); the list was reviewed so that none of its names is a dangerous method on any object the scripts can obtain, but a
future edit that adds an object with a same-named dangerous member would not be caught by the scan. Property **reads** are covered by the same name list. Any new .NET member used by a
production script is a finding until a reviewer adds it, so the list cannot grow silently (it is in `HASHES-PHASE2.txt`).

**What the second reviewers tried** (all kept as fixtures; the ones in the first list passed the round 1 scan and are now caught): `#requires -Modules`, the `_NETLOAD` string, a writer call with another root,
`switch -File`, `data` / `configuration` blocks, `New-Object System.Diagnostics.Process`; and, caught before and kept as regression controls: the aliases `ni`, `sc`, `iex`, `saps`, `%`, `?`, `select`; native `cmd` / `ping` / path calls;
`& $c`; `ForEach-Object { & $_ }` and `ForEach-Object Kill`; `[IO.File]::WriteAllText`; `[Diagnostics.Process]::Start`; splatting; here-strings; base64 `[Convert]`; `Microsoft.PowerShell.Management\Set-Content`; backtick-split and homoglyph
names; `$env:` reads and `+=`; redirections; `$k.setvalue`; `Invoke-CimMethod ... Terminate`; `Get-CimInstance -Namespace`; `[Console]::Error.Write`; abbreviated parameters. **Still not proof**: `ScriptBlock.Invoke()` and `InvokeReturnAsIs` on arbitrary
objects pass because the library itself uses them on its provider script blocks, and member names remain allowlisted by name, not per type, so the word "closed" holds only for named members and for the syntactic write forms of layer 5 (see the honest limit after it); it is a statement about the scan, not about the absence of side effects, which rests on the line-by-line review.

The scan has **94 negative-control fixtures** (one snippet per forbidden token or form), **61 reproduction fixtures** (every evasion form of the review: `Registry::SetValue`, CIM calls with the
allowed argument hidden in a comment, `StreamWriter` / `FileStream` with a string or integer mode / `OpenHandle` / `CreateSymbolicLink` / `GetTempFileName` / `FileInfo` and `DirectoryInfo` `Create*`,
dynamic or indirect members, `CloseMainWindow`, `PriorityClass`, `ScriptBlock::Create`, `InvokeScript`, `${env:X} =`, `Environment::CurrentDirectory =`, `TcpClient`, ...; each must fail with a named rule and
none may pass with zero findings), **35 round 2 reproduction fixtures** (the `#requires` forms, the `_NETLOAD` family in the static rule, the writer-call pins, `switch -File`, `data`, `configuration`, `New-Object Process`), **28 regression fixtures** of the forms the
reviewers confirmed as caught, **57 round 3 reproduction fixtures** (the seven member-write forms of review 1 M1 and their relatives, the `$PSScriptRoot` / `$PSCommandPath` / `$pwd` / `$args` writes of M2, the write-wrapper pin, the
library-guard redefinitions, the `FileStream` overloads), **7 production-copy fixtures** (the real scripts with the reviewer's appended lines, each flagged, and each unmodified copy clean), **15 round 3 positive controls** (scanned strictly), **64 round 4 reproduction fixtures**, **13 round 4 production-copy append fixtures** and **8 round 4 positive controls** (layer 6), a **parity run** of a shared corpus of transcript lines through both the reader and the scan rule, and positive controls (comments, the writer's own primitive, the load-command reader, read-only opens, constant member names must pass).

## 9. Tests (run without AutoCAD)

```powershell
pwsh -NoProfile -File .\tests\Run-AllTests.ps1            # all;  -Only static | functional
```

They use fake providers for every logic branch and a **benign stand-in process** (a renamed copy of `ping.exe`, started and stopped by the test harness, not by
the production scripts) with the documented environment hook for the end-to-end subprocess runs of both scripts. They create only a temp folder under the user
temp directory and remove it; the end-to-end section also performs the same read-only registry reads as the capture. Covered: happy path; every fail-closed branch (missing / non-numeric /
non-running PID, wrong process name, two acad, owner, build hash, profile cases, module hash / enumeration / duplicate, declared-set module loaded, module outside the baseline, DLL vs pin vs
`declared-set.json` hash mismatches, pin format, missing pin / DLL, extra file / designation / subfolder, malformed declared set, missing required transcript, load command in any session, output exists,
path outside root, UNC, device path, ADS, `..`, junction as root and as ancestor, invalid run id, session mismatch, placeholder sessionId); determinism (byte-identical apart from `volatile`, input order
independence, signer lookups); schema positive / negative (with a cross-check against the built-in `Test-Json` draft 2020-12); offline validator tampering, sidecar, ledger identity of the validator and
the library, designation and baseline recomputation; exit codes (0 / 2 / 3 / 1); the static scan. Round 2 adds (file `Phase2.Round2.Tests.ps1` and the second reproduction file): the load-command corpus and the reviewer's
`_NETLOAD` / `_appload` transcript; the pinned writer (another root refused, the pair writer, the partial write); the non-empty evidence root (function level and end to end); `NOT_LOCAL` and junction-reached declared sets;
the schema pin (an empty `{}` schema, a tampered ledger, missing hashes, a mismatched expected-inputs schema); a designation placed after the capture, a missing `.pin` and the restored state; the baseline rule for S1-B, S1-C, S2, S3, S4
versus S1-A; the seconds since the process start; the lower-case session at the command line. Round 3 adds (file `StaticScan.Reproductions3.Tests.ps1`): the member-write, automatic-variable, write-wrapper, function-redefinition and FileStream-arity fixtures of section 8 (layer 5), the production scripts with the reviewers' appended lines, and the `pinCheckAfterLoad` coverage line. Round 4 adds (file `StaticScan.Reproductions4.Tests.ps1`, plus a block at the end of `Phase2.Round2.Tests.ps1`): the scoped-automatic-variable, `$PSDefaultParameterValues`, `[ref]`,
common-parameter and advanced-function fixtures of section 8 (layer 6), the production scripts with the reviewer's appended lines, the round 4 positive controls, the `-AllowedEvidenceParent` cases and the input-file guard of the validator.
Result recorded in section 13.

## 10. Authority and what the review should NOT read into this

These scripts are **review material**. They do not authorize or perform a host run; `FIRST_NON_GOVERNING_HOST_RUN` stays `NOT_AUTHORIZED`; the record is
`governing = false`; `S1-A` remains `NOT_ELIGIBLE` until the Coordinator says otherwise. No value, hash or approval of the host was fabricated: the only
host values appearing in the tests are fakes in a temp folder.

**P4 / C-05 are not satisfied by these scripts alone.** The template fields `sessionProfileFacts` (`CPROFILE`, `SECURELOAD`, `TRUSTEDPATHS` as the process sees them) and `runtimeIdentities` have no mechanism here: they are
`HOST-TO-CONFIRM` in every record (`templateComplete = false`, verdict renamed `PHASE2_EXTERNAL_OBSERVATIONS_VALID`). The only in-process reads are the I-1 protocol commands A1-R4..R6, which run **after** phase 2, so the template's
"complete and validated before the first probe" is circular for S1-A as written (open question 12). The template's `pinCheck (AFTER_NETLOAD hashes)` is not covered either: phase 2 precedes `NETLOAD` (package v3 3.7 step 5), so the post-load pin
check remains with the CAD manager or the instrument.

## 11. Open questions for the Coordinator / CAD manager

1. **Profile key**: the key `...\ACAD-8101:409\Profiles` is in the preparation records (`machine-descriptive-facts.json`) and a read-only read by the second reviewer returned `<<Unnamed Profile>>`;
   the CAD manager still confirms on the designated machine that its default value is the current profile (`-ProfilesSubKey`). The `Variables` subkey layout read for `profileAtRest` is the one
   `cad-manager-attestation-and-readback.md` B.3 used (`...\Profiles\<profile>\Variables`, values `SECURELOAD` / `TRUSTEDPATHS`).
2. **Evidence root leaf = run id** is enforced (a refusal). Confirm that the CAD manager's per-run child is named exactly by the run id.
3. **Other `acad*` processes** (`acadlt`, helpers) and Autodesk helper processes are flagged in `processList.related` but only a second process named exactly `acad`
   fails. Confirm, or make any `acad*` fatal.
4. **`NO_DECLARED_SET_MODULE_LOADED` and `DECLARED_FOLDER_CONTENT_EXACT`** apply to every session at phase 2 time (the load and the designation come after, R3-2). Any extra file in the declared-set
   folder makes phase 2 `INVALID`; the procedure therefore needs the explicit **between-sessions cleanup** of section 4 (the CAD manager removes the placed designation). Confirm.
5. **Transcript format**: the AutoCAD `LOGFILEON` text encoding is not established here (UTF-8 / UTF-16 / ANSI are decoded and the encoding is recorded; HOST-TO-CONFIRM). A load command in the
   transcript fails phase 2 in every session; confirm that a phase 2 transcript is only ever the start-up part of the session.
6. **Revocation lookups and cache** of `Get-AuthenticodeSignature`: a network read and an operating-system cache write under the user profile (sections 2 and 7), and most of the run time. Accept (now in `volatile`), or drop the signer
   column, which would make "no network, no file outside the evidence root" literally true.
7. **Module baseline source**: the phase 1 baseline that exists today is a partial static inventory (`module-baseline-static-inventory.json`, "H1 SESSION_START confirms/replaces it"), not a file-by-file hashed
   `{path, sha256}` list. The comparison is implemented and tested, S1-A has no baseline to compare with (recorded `NOT_COMPARED`, the one session where it passes), and **since round 2 the baseline is mandatory from S1-B on in the code**
   (the plain valid verdict cannot carry `NOT_COMPARED` outside S1-A); the baseline for later sessions is the S1-A SESSION_START list. Confirm, or rule that it is optional (then it is a reviewed code change).
8. The human fields of template 3.5.3 (`validatedBy`, `validatedAtUtc`, `validationResult`) and `sessionProfileFacts` / `runtimeIdentities` as seen inside the process remain with the CAD
   manager (I-1 protocol); this tool contributes `pinCheck`, `processList`, `actualLoadedModules` (+ the baseline comparison), `pid`, `processStartUtc` and the build / profile observations.
9. Canonical JSON here is a pretty-printed sorted-key form, not RFC 8785 (the record is not hashed into a label); the file bytes are what is hashed.
10. `packageManifestSha256` and `buildTupleDigest` are not computed here (they remain the CAD manager's opaque value / a post-phase-2 computation).
11. The format of the assigned `sessionId` is not fixed by the package ("an identifier the CAD manager assigns"); the pattern `^[A-Za-z0-9][A-Za-z0-9._:-]{0,127}$` is this tool's choice and is **stricter than the instrument designation schemas**
   (`ct21d.designation.v1` / `.rs.v1` require only a non-empty string). An identifier outside the pattern is refused at the capture (exit 1, nothing written, so it is found before anything exists) but would be accepted downstream. Give the real format, or
   allow the relaxation of this pattern to the instrument rule.
12. **P4 / C-05 reading**: how are `sessionProfileFacts` and `runtimeIdentities` to be read, given that the only in-process reads (A1-R4..R6) follow phase 2 (section 10)? And who performs the `pinCheck (AFTER_NETLOAD)` of step 5?
13. **Evidence root must be empty**: the capture refuses any entry, hidden files included. Confirm that the transcript, the baseline and the expected-inputs file are kept outside the per-run child root until after the capture.
14. **Exit 4 / failed run**: after a partial write the run is failed and its root is not reusable. Confirm that a new run id (not a retry in the same root) is the intended procedure.
15. **Additions** `PROCESS_OWNER_IS_INVOKING_USER` and `profileAtRest` (section 5): adopt or strike.

## 12. Verifying the hash ledger

```powershell
# PowerShell 7, from this folder: lists any file whose hash differs from HASHES-PHASE2.txt (prints nothing when all equal)
Get-Content -LiteralPath .\HASHES-PHASE2.txt | ForEach-Object { $h, $n = $_ -split '  ', 2; if ((Get-FileHash -Algorithm SHA256 -LiteralPath $n).Hash.ToLowerInvariant() -ne $h) { "DIFFERS: $n" } }
```

`tests/StaticScan.Tests.ps1` also checks that the ledger lists exactly the files of the folder with the right hashes.

## 13. Result of the tests and the exact review request

At the end of fixer round 4: `pwsh -NoProfile -File .\tests\Run-AllTests.ps1` gave `TESTS passed=1322 failed=0 total=1322` (no AutoCAD started or needed). The ledger hashes are in `HASHES-PHASE2.txt`; the hash of
the ledger file itself (the value the validator prints as `LEDGER_SHA256`) is reported only in the hand-off message of the session that produced this folder, not here, because a file cannot contain its own hash. The review
request:

> Please review, **by reading only (do not run anything)**: (a) `lib/Phase2Common.ps1`, `Capture-Phase2.ps1` and `Validate-Phase2.ps1` against package v3 3.5.3 / 3.7 / 5.1
> and section 248 (read-only, create-new, fail-closed, single writer); (b) `schemas/ct21d.phase2.v1.json` and the 24 checks, the `coverage` honesty and the two identifiers; (c) `allowed-commands.txt`,
> `allowed-dotnet.txt` (line by line: is any listed name dangerous on any object the scripts can obtain?) and `tests/StaticScan.ps1` (is each exemption positional, and are layers 1 and 2 closed?);
> (d) the open questions of section 11. Rule on each of them, and on whether the files with the hashes of `HASHES-PHASE2.txt` are accepted as the phase 2 mechanism for S1-A. Any byte
> changed after the review invalidates it and the ledger must be regenerated.
