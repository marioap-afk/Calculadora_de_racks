# I-52 — Phase-2 attestation scripts: independent review of the round-4 final bytes (record)

> Decisions section 250. Review by a NEW independent adversarial reviewer over `eng/research/I52Ct21dPhase2` at HEAD `88138f01` (folder committed). Ledger `HASHES-PHASE2.txt` sha256 `0207af83ce512e5a12af050d4655163a21923fed357c5a63ba5bbc3caa0bc7f0` (20/20 files verified; git blob ids of every file at HEAD equal the working tree; the reviewer modified nothing; all attacks ran on scratch copies).

**PHASE2_ROUND4_REVIEW = APPROVE_EXACT.** Test suite re-run by the reviewer: 1322 passed, 0 failed. Evidence roots (`evidence`, `scratch`, `private-copy`) empty before and after.

About 160 NEW attack cases (not the earlier reproductions) over: scoped variable forms (`global:`, `script:`, `local:`, `private:`, `using:`, braces, `Set-Variable`/`Set-Item variable:`/`$ExecutionContext.SessionState.PSVariable`, `PSDefaultParameterValues` methods, environment variables); writes through `[ref]` and other by-reference channels; common parameters and stream redirections; indirect calls to the single authorized writer (call operator, `Get-Command`, function redefinition, dot-source, delegates, pipelines, from other functions or from Validate); writes outside the declared root (relative, `..`, `\?\`, UNC, ADS, trailing dot/space, device names, junction and hardlink, case twins, `-AllowedEvidenceParent` games); registry/config/ACL mutation; process start (jobs, `powershell -c`, CIM `Win32_Process.Create`, `wmic`, `schtasks`, `mshta`, `rundll32`, `[Process]::Start` variants, `Add-Type`, `Assembly::LoadFrom`); hidden NETLOAD / AutoCAD automation (COM, `SendKeys`, `.lsp`/`.scr` generation); and evidence without a real session. Result: no attack adds a capability (no process start, registry or ACL write, NETLOAD or AutoCAD automation, new file write outside the writer, or new writer call) without being caught. Majors: none.

Passed attacks are data-flow or semantic edits and session forgery, which the README already says a static scan cannot prove (sections 7 and 8); the production scripts were judged by reading every line.

## Minors and residual limits (accepted by the Coordinator as recorded; optional hardening, no byte change required)
- m1 writer confinement is pinned by call text, not by data flow (a reassignment of `$RootFullPath`/`$root` before the pinned calls, or a second `FileStream CreateNew` inside the writer function, passes the scan). The exact bytes are confined; the proof is the line-by-line review.
- m2 preference variables (`$ErrorActionPreference` etc.) are assignable; `throw` and .NET exceptions still terminate, non-terminating cmdlet errors would not.
- m3 anchors use `$` instead of `\z` (a trailing LF passes at library level; every traced path is refused later); a `Name` with a trailing dot is accepted by the writer (unreachable from production names).
- m4 `-AllowedEvidenceParent` is typed, not pinned.
- R1 session evidence without a real session: whoever holds the folder can import the library with fake providers and the ledger's hashes and pass `Validate-Phase2.ps1` (no authenticity binding; `captureBindingId` is a public hash); a renamed non-AutoCAD process is blocked only by the signed phase-1 expected hash; a copy of the real `acad.exe` launched from another folder passes (`acadBuild.path` is recorded, not pinned). The test-stand-in hook is sound (a hook record can only reach exit 3 / `PHASE2_TEST_STANDIN_OBSERVATIONS_OK`).
- R2 member names allowlisted by name, not per type; reads from pipes/UNC/`HKLM:` pass the scan; obfuscated NETLOAD strings pass as data with no sink.
- R3 TOCTOU between root check and create; symbolic-link branch untested (no privilege).

The reviewer also noted that an unrelated temp file `%TEMP%\r437.txt` was deleted by its cleanup glob; it was not created by the review.
