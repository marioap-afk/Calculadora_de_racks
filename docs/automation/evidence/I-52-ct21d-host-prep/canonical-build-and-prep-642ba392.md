# I-52 CT-21D host preparation: canonical build and prerequisites (commit 642ba392)

> ```text
> DOCUMENT              = preparation record by the CAD manager's assistant. Documents/tooling only. Not a baseline artifact, not a ruling, not an authorization.
> BOUND_COMMIT          = 642ba39230e6e98b1565fde4d8a4d3a246cc9ef3  (branch feature/rackmirror-espejo-semantico; CI 36834378153 green)
> BOUND_MAIN_SHA        = 95690c28dc6268e61dff32a0cbc33cc9fde3d47f  (git diff --stat 95690c28 HEAD -- src tests is empty)
> FIRST_NON_GOVERNING_HOST_RUN = NOT_AUTHORIZED     HOST_RUN_STARTED = FALSE
> AUTOCAD               = NOT STARTED, NOT TOUCHED. acad.exe was not running at the start, during or at the end of this preparation
> MACHINE_CLASS         = UNSET (the label is produced by I-1 in S1-A)
> ```

Machine-sensitive records (hostname, account, hardware, full facts, designation/class/tuple records, module inventory) live OUTSIDE the repository under `D:/I52-CT21D-HOST/records/`; this file carries hashes and build facts only. No instrument, probe or AutoCAD process was run. Nothing was added, committed or pushed.

## 1. Canonical build

- Source: clean detached git worktree `D:/I52-CT21D-HOST/src-642ba392` at 642ba39230e6e98b1565fde4d8a4d3a246cc9ef3, `git status` clean, created with `-c core.autocrlf=false` so every file equals its committed blob (a checkout with `core.autocrlf=true` converts the R0 folder to CRLF and the R0 list hashes then differ; see finding F-1).
- SDK: user-level `%LOCALAPPDATA%\Microsoft\dotnet\dotnet.exe`, `--version` in the tree = **8.0.423** (global.json resolved). Runtime 8.0.29. Roslyn csc 4.11.0-3.25569.22 (3fb752d4). Configuration Release; `Deterministic=true` (Directory.Build.props).
- Compile references (never copied, `Private=false`): AcCoreMgd/AcDbMgd/AcMgd 25.0.171.0.0 of the installed AutoCAD 2025 (2025.1.3).
- Commands (each exit 0, 0 warnings, 0 errors; logs outside the repo):

```powershell
$d = "$env:LOCALAPPDATA\Microsoft\dotnet\dotnet.exe"; cd D:/I52-CT21D-HOST/src-642ba392
& $d build eng/research/I52Ct21dHostFacts/core/I52Ct21d.HostFacts.Core.csproj -c Release
& $d build eng/research/I52Ct21dHostFacts/r0/I52Ct21d.HostFacts.R0.csproj -c Release
& $d build eng/research/I52Ct21dHostFacts/tools/I52Ct21d.HostFacts.Tools.csproj -c Release
& $d build eng/research/I52Ct21dHostFactsRs/core-rs/I52Ct21d.HostFacts.Rs.Core.csproj -c Release
& $d build eng/research/I52Ct21dHostFactsRs/rs/I52Ct21d.HostFacts.Rs.csproj -c Release
& $d build eng/research/I52Ct21dHostFactsRs/tools-rs/I52Ct21d.HostFacts.Rs.Tools.csproj -c Release
```

- Reproducibility: after `git clean -fdx eng/research` a second Release build of the same six projects gave the same SHA-256 for all 31 dll/pdb/exe outputs (the diff of the two hash lists is empty). Caveat: the DLLs embed the absolute pdb path (`D:\I52-CT21D-HOST\src-642ba392\...\obj\Release\...`); a build at another path was not tried and may differ. The DLLs carry InformationalVersion `1.0.0+642ba39230e6e98b1565fde4d8a4d3a246cc9ef3`.
- Canonical copies (hash before = hash after copy for all four): `D:/I52-CT21D-HOST/canonical-642ba392/declared-set/` (4 DLLs + 4 `.pin`), `.../review-material/offline-tools-r0|rs/` (offline tools with their dependencies), `.../pdb-informational/`.

| Binary | SHA-256 | size |
|---|---|---|
| `I52Ct21d.HostFacts.R0.dll` | `fcaf938e0eb79f70035dd9dd25184b8d6c38860581f35912e4ccb1ccbd051d3c` | 13824 |
| `I52Ct21d.HostFacts.Core.dll` (one byte set; identical in the core, r0, tools, core-rs, rs and tools-rs outputs) | `6bf5957bcbce3110990c482a33cdfd4729aaf8f5989b363ac9c8443703469fb6` | 164864 |
| `I52Ct21d.HostFacts.Rs.dll` | `46dfed73a2edbbe469e9285fcc0a85248ef6f3c4c7d0468fa067e7d7e5eecd10` | 24064 |
| `I52Ct21d.HostFacts.Rs.Core.dll` | `03998dca344c650f0edd6c21071f57662d9b913f14dc1ea95b57bddedc36e78e` | 124416 |
| `I52Ct21d.HostFacts.Tools.dll` (tools R0) | `d8cf6facc324b72fffbdace4d5e03f873fdafd51b5f9670bc57cbfd28bbf523e` | 110592 |
| `I52Ct21d.HostFacts.Rs.Tools.dll` (tools-rs) | `d2109afe2ca6796fe5824b874668b345de2985c16f7142402aeb195a742334f9` | 48128 |
| pdb (informational) | Core `ae5304d9ef4ac11f14e31afa5578a67e398a74f851dc907836a6f0ec9360663d`, R0 `da7a4816431d0d1f26d536d71d3a478dbad3072f3d739e93772f083a0ccbec30`, Rs.Core `b0022641f916b9b6bc06964a699f4d8da36090e735c304c382a849a5881394fe`, Rs `c7d009b96f96b8a81b568f63762880506f1fd701e3ce659b8cf3876a6d2d15fa`, Tools `f6aec2417eabf96bfa231d19b9eb77f902fc3f9d1602fd06d4980be20e59d5b4`, Rs.Tools `a8cc8728e8bfe89951bcaf36d5dd4ba5769ebd8e263c2756ceeba31f4551054f` | |

## 2. declaredSet, .pin, package identity

- `declared-set.json` (compact, keys sorted, paths sorted ordinally; `[{path, sha256}]` over the four DLLs in `declared-set/`): SHA-256 = `declaredSetSha256` = `19a64d1de93fbc0afbad473c8852ee7ae7945cf56646d2d2aa5c9d001b1acf13`. Artifact version: `declaredSet-642ba392-v1` (a preparation name; the repository defines no declaredSet schema, I-8 is not implemented). The `path` values are the canonical-store paths; if the TRUSTEDPATHS decision places the binaries in another folder, the list and its hash must be recomposed (bytes unchanged).
- `.pin` sidecars (lowercase hex SHA-256 of the DLL + LF, 65 bytes; the format `SelfPin.Verify` reads: `ReadAllText().Trim().ToLowerInvariant()` compared with the file hash; RS also self-pins `Core` and `Rs.Core`): Core `60e939bd169c631c242f9812ae9752144ff49da7b665213a335af7fc10310b8a`, R0 `26858a77026e8460b0f943eff5e63d323373760e330607a1748edd63e067ca83`, Rs.Core `7e5920b88ca115d2ebfef50f6a528c2b8c6803c43f81b3ceb094582cd480d5fc`, Rs `52a8e1b0ea847b7e367b9b09e9b1baefa1aea4f7da584e12121d7c50d7babd5c` (hashes of the .pin files; each content equals its DLL hash).
- `packageManifestSha256` (opaque by decisions 241 RQ-4/RQ-10; nothing checks it): `229830cec6dcf233530cea7c6e1f1cd693eec8c04f2396a51f1540e22c85bc21` = SHA-256 of `package-manifest.txt` (34 sorted `sha256  relpath` lines: declared set + pins, both offline-tool DLLs, the 5 RS and 4 R0 lists, 6 RS and 6 R0 schemas, `HASHES-RS.txt`, the two protocol files). Prepared for the CAD manager to adopt or replace.
- `buildTupleDigest`: only a PARTIAL candidate exists (`4baec7140de32bfec20233b48ca803c4c2d820f470f037cb802248b60fd0ea12`; library, template and catalog hashes, SECURELOAD/TRUSTEDPATHS and the S1-A module list are missing) and must not be placed in any designation.
- RackCad package for S1: **none declared for S1** (and none for S2..S4), per package v3 section 3.2 and 2.3.

## 3. Prohibited-API analysis on the exact canonical bytes

| Check | Result |
|---|---|
| `scan-rs` (core: Rs.Core, tools: Rs.Tools, rs: Rs.dll) | exit 0, `lists: CLEAN`; Rs.Core CLEAN (468 import rows, 0 waived), Rs.Tools CLEAN (443), Rs.dll CLEAN (358 rows, 57 waived findings, 53 privileged call sites) |
| `scan` R0 (core, tools, r0) | exit 0, all CLEAN, `privileged lists: CLEAN` |
| `imports-allowlist-check` R0 | exit 0, CLEAN, allowed-apis.txt CLEAN |
| `privileged-list` / `privileged-list-rs` observed counts | R0 18 SURFACE / 15 CALLER / 4 COMMAND; RS 30 / 60 / 4: equal to the list sizes printed by the scans (layer 3 is part of the scans) |
| `ledger-verify HASHES-RS.txt` | exit 0, `ledger: no difference` (52 entries; the repository holds 53 files in the folder = 52 + the ledger) |
| R0 tests (Release) | 509 passed, 0 failed |
| RS tests (Release) | 1332 passed, 0 failed |
| Freeze guard `run --no-build --project eng/research/I52Ctda/harness -c Release -- freeze-v35 .` (harness built Release in the CLEAN tree) | exit 0, `Holds: true`, 24 blobs, no mismatch, package hash D9FD41B4CEBE5F698C2A597A96E4AAE9221A9DC32060ED791B73232636A47554 |

Printed list hashes (identical to section 4): RS `allowed-apis-rs.txt` f14fe6fa...d266 (814 entries), `rs-waivers.txt` 1bd95c25...baa5 (10), `privileged-surface-rs.txt` 9cf61870...480c (30), `privileged-callers-rs.txt` fa1daf86...a09d (60), `expected-commands-rs.txt` 53399...f77a7 (4); R0 `allowed-apis.txt` b2cb0d26...d880 (926), `privileged-surface.txt` 7a56a8e7...949c (18), `privileged-callers.txt` 278e8ccf...3a07 (15), `expected-commands.txt` f3465afd...7a9d (4). The scans are review aids; the final authority is the independent review plus the hashes (README of both folders).

## 4. Hash verification table (against decisions section 244 and package v3 3.3; 0 mismatches)

| ARTIFACT | EXPECTED_HASH | ACTUAL_HASH | MATCH |
|---|---|---|---|
| RS list `allowed-apis-rs.txt` (sec.244 + pkg v3 3.3) | `f14fe6fa02c1846843f665b2ec394b963f4fc83e1befa08730dc2ae88b23d266` | `f14fe6fa02c1846843f665b2ec394b963f4fc83e1befa08730dc2ae88b23d266` | MATCH |
| RS list `rs-waivers.txt` (sec.244 + pkg v3 3.3) | `1bd95c25ef002b9830e9d7a9570a9ecabcc4c7b857750312eb24c00f3066baa5` | `1bd95c25ef002b9830e9d7a9570a9ecabcc4c7b857750312eb24c00f3066baa5` | MATCH |
| RS list `privileged-surface-rs.txt` (sec.244 + pkg v3 3.3) | `9cf61870cb2c7b910f84045b2042db63a5abb6b92ca486e1bf5bdb962639480c` | `9cf61870cb2c7b910f84045b2042db63a5abb6b92ca486e1bf5bdb962639480c` | MATCH |
| RS list `privileged-callers-rs.txt` (sec.244 + pkg v3 3.3) | `fa1daf86fd8662788f574f8295f1438cc2c464b6dbe28c5719369cde5b12a09d` | `fa1daf86fd8662788f574f8295f1438cc2c464b6dbe28c5719369cde5b12a09d` | MATCH |
| RS list `expected-commands-rs.txt` (sec.244 + pkg v3 3.3) | `533997c8016f2fd0ea2336e1c72761ca44f14ecece502cb890f68dd7a7ef77a7` | `533997c8016f2fd0ea2336e1c72761ca44f14ecece502cb890f68dd7a7ef77a7` | MATCH |
| RS schema `ct21d.designation.rs.v1.json` (sec.244 + pkg v3 3.3) | `fe31f053a6b664985e5526191d1a9dfaff229983637cd1991e558a0867085a0f` | `fe31f053a6b664985e5526191d1a9dfaff229983637cd1991e558a0867085a0f` | MATCH |
| RS schema `ct21d.dyncensus.v1.json` (sec.244 + pkg v3 3.3) | `bdc8c90de5cba1d6d3d02414c101b5eb0b4c4703bf1b908c1b4d6e571e8cab76` | `bdc8c90de5cba1d6d3d02414c101b5eb0b4c4703bf1b908c1b4d6e571e8cab76` | MATCH |
| RS schema `ct21d.rs-run.v1.json` (sec.244 + pkg v3 3.3) | `8f0861910470cef70614144595917856b4dc7c503303040c65d1513c5bc1757a` | `8f0861910470cef70614144595917856b4dc7c503303040c65d1513c5bc1757a` | MATCH |
| RS schema `ct21d.tolscale-probe-table.v1.json` (sec.244 + pkg v3 3.3) | `ed0dc0b66604b439fce2d52ea19b6c546bb19b020a8d0b9ab6b658a02a26c0fa` | `ed0dc0b66604b439fce2d52ea19b6c546bb19b020a8d0b9ab6b658a02a26c0fa` | MATCH |
| RS schema `ct21d.tolscale.v1.json` (sec.244 + pkg v3 3.3) | `f377cc0e4c53ad6261292acafbaf6707b431b309bdde1813e8147aedb8bc1ad9` | `f377cc0e4c53ad6261292acafbaf6707b431b309bdde1813e8147aedb8bc1ad9` | MATCH |
| RS schema `ct21d.writeback.v1.json` (sec.244 + pkg v3 3.3) | `04ded4aec9ad2829a9cf49f7ae103c14ed06b848281a6fc578a04b1800445ca6` | `04ded4aec9ad2829a9cf49f7ae103c14ed06b848281a6fc578a04b1800445ca6` | MATCH |
| R0 list `allowed-apis.txt` (sec.244 + pkg v3 3.3) | `b2cb0d26ac8dd5ce53a70845d62b2c3237e678aa586e4cc2f361687884a9d880` | `b2cb0d26ac8dd5ce53a70845d62b2c3237e678aa586e4cc2f361687884a9d880` | MATCH |
| R0 list `privileged-surface.txt` (sec.244 + pkg v3 3.3) | `7a56a8e77c841e0f481dba07dc0297714124ea716e978d3f2bbe497a1887949c` | `7a56a8e77c841e0f481dba07dc0297714124ea716e978d3f2bbe497a1887949c` | MATCH |
| R0 list `privileged-callers.txt` (sec.244 + pkg v3 3.3) | `278e8ccfa5da25a53614d4c54d958f0bf4c212f6e07eea62303da046899a3c07` | `278e8ccfa5da25a53614d4c54d958f0bf4c212f6e07eea62303da046899a3c07` | MATCH |
| R0 list `expected-commands.txt` (sec.244 + pkg v3 3.3) | `f3465afd52bc856a1852a9881a2130c2f801c54d0ebbc9428f95c85fb0267a9f` | `f3465afd52bc856a1852a9881a2130c2f801c54d0ebbc9428f95c85fb0267a9f` | MATCH |
| R0 schema `ct21d.census.v1.json` (pkg v3 3.3) | `03bedbe6fb7b48fa669e16f166d2e5fbacfbc3c409af744295ec299ac85dc12d` | `03bedbe6fb7b48fa669e16f166d2e5fbacfbc3c409af744295ec299ac85dc12d` | MATCH |
| R0 schema `ct21d.ctxvars.v1.json` (pkg v3 3.3) | `74823e1add1d2343b5abf64d475880edc7e6fb6df4fdb0811491ee09ab4df754` | `74823e1add1d2343b5abf64d475880edc7e6fb6df4fdb0811491ee09ab4df754` | MATCH |
| R0 schema `ct21d.designation.v1.json` (pkg v3 3.3) | `6ba2b81fc77bce9a9365ab0b4d384f23f27d907d8758646f369841d7538b73cf` | `6ba2b81fc77bce9a9365ab0b4d384f23f27d907d8758646f369841d7538b73cf` | MATCH |
| R0 schema `ct21d.hostfact.v1.json` (pkg v3 3.3) | `8108ba71583f2cca9ca2d214e3400d91ba93057e09bac97e108037001cf0ec76` | `8108ba71583f2cca9ca2d214e3400d91ba93057e09bac97e108037001cf0ec76` | MATCH |
| R0 schema `ct21d.inventory.v1.json` (pkg v3 3.3) | `bca6bd9de86529632f0748376ad17c1cf9fcee605602b7b40bb2d88d8000a87f` | `bca6bd9de86529632f0748376ad17c1cf9fcee605602b7b40bb2d88d8000a87f` | MATCH |
| R0 schema `ct21d.machine-label.v1.json` (pkg v3 3.3) | `388e2dab519dca5f100931cc3b015e0ff91aaba2fc45e03af825d30fd1c05717` | `388e2dab519dca5f100931cc3b015e0ff91aaba2fc45e03af825d30fd1c05717` | MATCH |
| R0 protocol `I-1-observation-protocol.md` (pkg v3 3.3) | `a608833e72d9a0401d8aa3c33496a6f8f3d098426ac8e72b3a563187df4037c8` | `a608833e72d9a0401d8aa3c33496a6f8f3d098426ac8e72b3a563187df4037c8` | MATCH |
| R0 protocol `i1-os-observation.ps1` (pkg v3 3.3) | `662dd9ab686427077e8d178466ee2a625d494dad9592fe7f582202148d23ceb3` | `662dd9ab686427077e8d178466ee2a625d494dad9592fe7f582202148d23ceb3` | MATCH |
| RS ledger `HASHES-RS.txt` (sec.244) | `7f190d2f5f16f36cb3860c6af484baae5e240a89d588abc2183bc875f64b16a1` | `7f190d2f5f16f36cb3860c6af484baae5e240a89d588abc2183bc875f64b16a1` | MATCH |
| RS README (informational) `README.md (RS)` (pkg v3 3.3 (informational)) | `1ff7b05fa9685733087edd47ae75a203a6e347cbab7634469f1cab8e2f3f4874` | `1ff7b05fa9685733087edd47ae75a203a6e347cbab7634469f1cab8e2f3f4874` | MATCH |
| R0 README (informational) `README.md (R0)` (pkg v3 3.3 (informational)) | `5a47b53fcbadc9f27814dd80cea3e478c296ca57f3df70b910bf348d3569128b` | `5a47b53fcbadc9f27814dd80cea3e478c296ca57f3df70b910bf348d3569128b` | MATCH |
| DLL `I52Ct21d.HostFacts.R0.dll` (declared set, R0) | none exists yet (recorded now) | `fcaf938e0eb79f70035dd9dd25184b8d6c38860581f35912e4ccb1ccbd051d3c` | n/a (recorded) |
| DLL `I52Ct21d.HostFacts.Core.dll` (declared set, shared core (one byte set for R0 and RS)) | none exists yet (recorded now) | `6bf5957bcbce3110990c482a33cdfd4729aaf8f5989b363ac9c8443703469fb6` | n/a (recorded) |
| DLL `I52Ct21d.HostFacts.Rs.dll` (declared set, RS) | none exists yet (recorded now) | `46dfed73a2edbbe469e9285fcc0a85248ef6f3c4c7d0468fa067e7d7e5eecd10` | n/a (recorded) |
| DLL `I52Ct21d.HostFacts.Rs.Core.dll` (declared set, RS core) | none exists yet (recorded now) | `03998dca344c650f0edd6c21071f57662d9b913f14dc1ea95b57bddedc36e78e` | n/a (recorded) |
| tools R0 `I52Ct21d.HostFacts.Tools.dll` | none exists yet | `d8cf6facc324b72fffbdace4d5e03f873fdafd51b5f9670bc57cbfd28bbf523e` | n/a (recorded) |
| tools-rs `I52Ct21d.HostFacts.Rs.Tools.dll` | none exists yet | `d2109afe2ca6796fe5824b874668b345de2985c16f7142402aeb195a742334f9` | n/a (recorded) |
| sidecar `I52Ct21d.HostFacts.Core.dll.pin` (65 bytes = lowercase hex + LF; content equals the DLL hash) | equals the DLL row | `60e939bd169c631c242f9812ae9752144ff49da7b665213a335af7fc10310b8a` (hash of the .pin file) | content verified equal |
| sidecar `I52Ct21d.HostFacts.R0.dll.pin` (65 bytes = lowercase hex + LF; content equals the DLL hash) | equals the DLL row | `26858a77026e8460b0f943eff5e63d323373760e330607a1748edd63e067ca83` (hash of the .pin file) | content verified equal |
| sidecar `I52Ct21d.HostFacts.Rs.Core.dll.pin` (65 bytes = lowercase hex + LF; content equals the DLL hash) | equals the DLL row | `7e5920b88ca115d2ebfef50f6a528c2b8c6803c43f81b3ceb094582cd480d5fc` (hash of the .pin file) | content verified equal |
| sidecar `I52Ct21d.HostFacts.Rs.dll.pin` (65 bytes = lowercase hex + LF; content equals the DLL hash) | equals the DLL row | `52a8e1b0ea847b7e367b9b09e9b1baefa1aea4f7da584e12121d7c50d7babd5c` (hash of the .pin file) | content verified equal |

## 5. Source/review identity (what binds the reviewed sources to these bytes)

- The commit 642ba39230e6e98b1565fde4d8a4d3a246cc9ef3 is the single source: the DLLs carry `+642ba39230e6e98b1565fde4d8a4d3a246cc9ef3`, the worktree is clean and detached at it.
- The RS folder: 53 tracked files, tree id `0d89829bcb22708d0866c18676094b49da138dc0`; the 53 blob ids of the clean tree equal `git hash-object` of the same paths in the reviewed feature worktree (53 checked, 0 differ; list outside the repo). `HASHES-RS.txt` (7f190d2f...b16a1, equal to the section 244 value) lists the SHA-256 of 52 source files; `ledger-verify` exit 0 on the clean tree. The folder is `* -text`, so bytes are checkout-independent.
- The R0 folder: tree id `7e68bd22d401416010b0e20902ee64f259ad3df8`; `git diff --stat b1fd5027 642ba392 -- eng/research/I52Ct21dHostFacts` is empty (the section 241 reviewed state); the four R0 list hashes equal the reference when files are checked out LF.
- `src/` and `tests/` do not differ from main 95690c28 (empty diff).
- Package v3 blob `0c9e331ce6f69537df363668714edb3b91baba12` = the blob named in decisions 245 = `git rev-parse 642ba39230e6e98b1565fde4d8a4d3a246cc9ef3:docs/initiatives/I-52-ct21d-host-gate-execution-package-v3.md`.
- Limit: no file hash links the reviewed SOURCE to the compiled DLL except the commit stamp and the deterministic rebuild. The two builds were made by the same toolchain on one machine; an independent rebuild by the CAD manager from the same commit at the same path would be the cross-check.

## 6. Session eligibility (package v3 sections 2 and 4; nothing upgraded by interpretation)

| Id | Prerequisite (package v3 section 4 / 3.1) | S1-A | S1-B | S1-C | S2 | S3 | S4 | Note |
|---|---|---|---|---|---|---|---|---|
| C-01 | machine designated by the CAD manager (P1) | NOT MET | NOT MET | NOT MET | NOT MET | NOT MET | NOT MET | Designation record prepared (designation made on the Owner-delegated request, revisable); the signer name/role/asset id is empty and the record is unsigned |
| C-02 | machine-class record published (P2) and TRUSTEDPATHS decision applied (P2b) | NOT MET | NOT MET | NOT MET | NOT MET | NOT MET | NOT MET | Class record prepared with label UNSET, unpublished; TRUSTEDPATHS decision (3.5.4) not made or applied; AutoCAD settings were not touched |
| C-03 | tuple record PHASE 1 complete (P3) | NOT MET | NOT MET | NOT MET | NOT MET | NOT MET | NOT MET | Prepared and hashed but INCOMPLETE: SECURELOAD/TRUSTEDPATHS intended values, library hash, template hash, freeze acknowledgement, sessionId/runId open; the module baseline is a partial static inventory |
| C-04 | machineClassLabel is MC-<12 hex> (R3-1), or S1-A | N/A | NOT MET | NOT MET | NOT MET | NOT MET | NOT MET | No label exists until S1-A runs and `tools label` is applied |
| C-05 | session-binding mechanism ready (P4) | NOT MET | NOT MET | NOT MET | NOT MET | NOT MET | NOT MET | Phase 2 form referenced, not filled in its procedure fields; I-1 script/protocol hashes verified equal but not run; no process-list or NETLOAD attestation script prepared |
| C-06 | declaredSet and .pin published; Core DLL is one byte set for R0 and RS | MET | MET | MET | MET | MET | MET | Produced and byte-verified (4 DLLs, 4 .pin); S1-A loads nothing but the set must be final before the first session (package 3.2) |
| C-07 | canonical build once from the committed tree, SDK 8.0.423, Release; scan-rs and R0 scan re-run on those exact DLLs, CLEAN, printed list hashes equal | MET | MET | MET | MET | MET | MET | Done in this preparation (build commit 642ba392; both scans exit 0; list hashes printed equal) |
| C-08 | independent source review recorded (P7): named record, byte set = HASHES-RS.txt hash, no byte changed | N/A | N/A | N/A | MET | MET | MET | Named by decisions 245 (e): the three rounds of section 243 and the delta of section 244; ledger-verify exit 0 on the clean tree (7f190d2f...) |
| C-09 | Architect review recorded (P9) incl. delta review | N/A | N/A | N/A | MET | MET | MET | Section 244: APPROVED_WITH_REQUIRED_CHANGES, REQ-1..3 delta APPROVE_EXACT |
| C-10 | R0 class review holds; four R0 list hashes unchanged | N/A | MET | MET | N/A | N/A | N/A | Section 241 PASS (a-c); the four R0 list hashes equal the reference; R0 folder has no diff from b1fd5027 to 642ba392 |
| C-11 | hashes verified by the CAD manager (P10), table signed | NOT MET | NOT MET | NOT MET | NOT MET | NOT MET | NOT MET | The table was machine-verified in this preparation (0 mismatches); the CAD manager has not signed |
| C-12 | exact scenario, command(s), run id, order named in the authorization | NOT MET | NOT MET | NOT MET | NOT MET | NOT MET | NOT MET | No authorization exists |
| C-13 | paths: evidence root, fresh empty scratch root (RS), local fixed drive, not synced/indexed, ACL limited, no reparse points, private copy outside both | NOT MET | NOT MET | NOT MET | NOT MET | NOT MET | NOT MET | Parent roots exist and are empty on a local fixed NTFS drive outside OneDrive with NotContentIndexed; per-run roots not created; ACL NOT restricted to the AutoCAD user |
| C-14 | cleanup plan and cleanup ledger opened (P13) | NOT MET | NOT MET | NOT MET | NOT MET | NOT MET | NOT MET | Ledger is per run id; not created |
| C-15 | private library copy made for this run id; A0=B0=A1 (P12) | N/A | NOT MET | NOT MET | NOT MET | NOT MET | NOT MET | blocks-library.dwg was not found on this machine; no library hash exists |
| C-16 | Coordinator confirms the run belongs to the authorized gate (membership line) | NOT MET | NOT MET | NOT MET | NOT MET | NOT MET | NOT MET | No decisions entry |
| C-17 | no other acad.exe; no pending Windows/AutoCAD update; process-list snapshot before | NOT MET | NOT MET | NOT MET | NOT MET | NOT MET | NOT MET | acad.exe observed not running during preparation; pending-update state unknown; the per-session snapshot does not exist |
| C-18 | stop conditions read and signed by the operator | NOT MET | NOT MET | NOT MET | NOT MET | NOT MET | NOT MET | Not signed |
| C-19 | no-automatic-retry rule accepted (signed statement) | NOT MET | NOT MET | NOT MET | NOT MET | NOT MET | NOT MET | Not signed |
| C-20 | status of the Owner objection window recorded (P18) | NOT MET | NOT MET | NOT MET | NOT MET | NOT MET | NOT MET | Not recorded in decisions 245 |
| C-21 | S3/S4 ordering: S3 completed, outcome recorded, no companion file | N/A | N/A | N/A | N/A | N/A | NOT MET | S3 has not run |
| C-22 | S4 only: PRE-1..PRE-4 closed, sealed pre-registration entry, R-M ruling on R1(b) | N/A | N/A | N/A | N/A | N/A | NOT MET | PRE-1..PRE-4, sealed pre-registration and R-M ruling are NOT closed (package 2.3 S4, P19) |
| C-23 | package ruled by blob id (P20); state lines unchanged | MET | MET | MET | MET | MET | MET | Decisions 245 names blob 0c9e331c...; equals git rev-parse of the file at 642ba392 |
| ORD | S1-A completed first and its label produced (S1-B, S1-C, S2, S3, S4 all need MC-<12 hex>) | N/A | NOT MET | NOT MET | NOT MET | NOT MET | NOT MET | S1-A has not run |
| TPL | S1-C only: unsaved drawing from a designated template copy, hash recorded | N/A | N/A | NOT MET | N/A | N/A | N/A | No template designated |
| P4+ | FIRST_NON_GOVERNING_HOST_RUN = AUTHORIZED for the session | NOT MET | NOT MET | NOT MET | NOT MET | NOT MET | NOT MET | FIRST_NON_GOVERNING_HOST_RUN = NOT_AUTHORIZED |

- **S1-A: NOT_ELIGIBLE** (14 open): C-01, C-02, C-03, C-05, C-11, C-12, C-13, C-14, C-16, C-17, C-18, C-19, C-20, P4+
- **S1-B: NOT_ELIGIBLE** (17 open): C-01, C-02, C-03, C-04, C-05, C-11, C-12, C-13, C-14, C-15, C-16, C-17, C-18, C-19, C-20, ORD, P4+
- **S1-C: NOT_ELIGIBLE** (18 open): C-01, C-02, C-03, C-04, C-05, C-11, C-12, C-13, C-14, C-15, C-16, C-17, C-18, C-19, C-20, ORD, TPL, P4+
- **S2: NOT_ELIGIBLE** (17 open): C-01, C-02, C-03, C-04, C-05, C-11, C-12, C-13, C-14, C-15, C-16, C-17, C-18, C-19, C-20, ORD, P4+
- **S3: NOT_ELIGIBLE** (17 open): C-01, C-02, C-03, C-04, C-05, C-11, C-12, C-13, C-14, C-15, C-16, C-17, C-18, C-19, C-20, ORD, P4+
- **S4: NOT_ELIGIBLE / BLOCKED** (19 open): C-01, C-02, C-03, C-04, C-05, C-11, C-12, C-13, C-14, C-15, C-16, C-17, C-18, C-19, C-20, C-21, C-22, ORD, P4+

S4 stays NOT_ELIGIBLE until PRE-1..PRE-4, the sealed pre-registration entry and the R-M ruling on R1(b) are closed and S3 has run with its outcome recorded.

## 7. Findings and things not done

- **F-1 line endings.** The repo config has `core.autocrlf=true`; a default checkout writes the R0 folder with CRLF, and `scan`/`imports-allowlist-check` hash the raw file bytes, so R0 list hashes would not equal the section 244 (LF) values. The canonical tree was therefore created with `core.autocrlf=false`. The package should say so.
- **F-2 path-bound DLL bytes.** The DLLs embed the build path; reproducibility was shown at one path only.
- **F-3 inputs absent on this machine.** `blocks-library.dwg` was not found (repository, D:/Documentos, user profile); no template designated; so S1-B/S1-C/S2/S3 cannot get private copies or designations. The `run-designation.json` for S1-B/S1-C was NOT composed (it would need the S1-A label, the library hash and the run timestamp); S1-A has no designation file by package 2.3. No `designationSha256` exists.
- **F-4 not done on purpose.** MachineGuid, SECURELOAD, TRUSTEDPATHS, the PARAM-05 strings and the I-1 script were not read/run; no TRUSTEDPATHS change; no ACL change (the roots inherit D:\ ACLs, Authenticated Users have modify); no per-run roots; no session binding; no host evidence.
- **F-5** The module baseline inventory is partial (registry Applications keys and ApplicationPlugins bundles only). Autodesk Access/Licensing background services were running; acad.exe was not.
- Checklist P-items still open for the CAD manager: P1/P2/P2b/P3/P4/P10/P11/P12/P13/P14/P15/P16-P18 as in the table above.

## 8. Removing the working tree later

The source worktree is kept for reproducibility. To remove it: `git -C <repo> worktree remove --force D:/I52-CT21D-HOST/src-642ba392` (then `git worktree prune`); the canonical binaries under `D:/I52-CT21D-HOST/canonical-642ba392/` are independent of it.
