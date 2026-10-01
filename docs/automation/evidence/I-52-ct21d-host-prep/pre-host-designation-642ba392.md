# I-52 CT-21D pre-host designation and ratification (build commit 642ba392)

> ```text
> DOCUMENT              = repo-safe summary by the CAD manager's assistant, written on the CAD manager's/Owner's explicit instruction (the ten items of the pre-host designation).
>                         Documents/tooling only. Not a baseline artifact, not a ruling, not an authorization. No hostname, account or hardware identifier appears here.
> SOURCE_HEAD_SHA       = 642ba39230e6e98b1565fde4d8a4d3a246cc9ef3   (canonical build commit)
> BRANCH_TIP            = 5eed29ae0e60e11cd64752e4fea13546de6bcd37   (feature/rackmirror-espejo-semantico; docs-only after the build commit: no change under eng/, src/, tests/, the
>                         package or the source-review evidence between 642ba392 and the tip)
> REBASE_BEFORE_HOST    = NO   (origin/main is at 819955d61a6da4c811a11fbd11b5dca13f634b7c, 21 commits after 95690c28; no change under eng/research; 6 files changed under src/tests.
>                         Recorded for POST-HOST reconciliation only; the canonical bytes are not rebuilt)
> FIRST_NON_GOVERNING_HOST_RUN = NOT_AUTHORIZED     HOST_RUN_STARTED = FALSE     AUTOCAD_STARTED = FALSE (acad.exe not running at the start nor the end of this pass)
> ```

Machine-sensitive records are OUTSIDE the repository under `D:/I52-CT21D-HOST/records/` (index with hashes: `records-sha256-index-v2.txt`, sha256 `3c2d0f8a59db6443ca5aceef547fd24d1ef42a73fcff81e6111a8b5a159a802f`).
Hard rules kept: AutoCAD not started or touched; no instrument, probe or I-1 script run; MachineGuid, SECURELOAD, TRUSTEDPATHS and the profile attributes of A1-R1..A1-R6 NOT read; no system or security setting, registry value
or ACL changed; nothing rebuilt or recompiled; nothing added, committed or pushed. Where work needs a change, the exact commands are written for the CAD manager and the status is `SPECIFIED_NOT_APPLIED`.

## 1. Machine (item 1)

- MACHINE = the machine of the existing machine-class record (this machine). Re-confirmed read-only: same computer name and model as recorded, `acad.exe` and the three managed assemblies re-hashed equal to the record
  (acad.exe sha256 `2a75996fd2a5c5ee0376fd5ba7cced227a098193ff495e8ceeda3a96fe326422`, R25.0.171.0.0), Windows 11 Pro 10.0.26200, asset id still EMPTY (CAD manager). MACHINE_ID: in the records, not reproduced here.
- MACHINE_CLASS label = **UNSET** (produced by S1-A with `tools label`). Machine-class record v2: `D:/I52-CT21D-HOST/records/machine-class-record-v2.txt` sha256 `bf03cba57d66f8a067f21f9dfbe1eb1b0df25a0ddbe0030904ba7e4f68f033d4`
  (v1 kept unchanged: sha256 `8abaafcf0ab78a2aa052896acbe95d3122d353fbdec26c1a038b6602afe090c8`). Both are unsigned and unpublished.
- Does the record hold the BA-10 class-defining attributes? **No.** The six strings (MachineGuid, OsVersionBuild, AutoCadProduct, AutoCadProfile, SECURELOAD, TRUSTEDPATHS) are the normative attributes (BA-10 V8 2.2) and are
  `NOT_READ` until S1-A reads them with I-1 (A1-R1..A1-R6). The record has only descriptive surrogates for three of them. No equivalence claim for any other machine.

## 2. TRUSTEDPATHS policy (item 2)

Policy file (outside the repo): `D:/I52-CT21D-HOST/records/trustedpaths-policy.md` sha256 `04e8d354a8b35ea3013065420f10247c2c139efc6946170410bdb24775c0a05e`. **Status SPECIFIED_NOT_APPLIED** (nothing read from or written to TRUSTEDPATHS/SECURELOAD).

- Minimal set: exactly ONE added entry, the folder from which the declared set is NETLOADed (all four `path` values of `declared-set.json` share it): `D:\I52-CT21D-HOST\canonical-642ba392\declared-set`
  (no trailing backslash). Folder content: the 4 DLLs and 4 `.pin` (verified: 8 files) plus, after phase 2, the session's designation file.
- Recursive `\...` NOT allowed. No RackCad path for S1..S3 (none declared); a later session that loads a RackCad build needs its own policy revision and a new tuple. No other exceptions. The evidence, scratch and private-copy roots are data,
  not code, and are NOT trusted paths. The offline tools and the review material are not trusted.
- Mechanism: the CAD manager records the pre-existing machine entries verbatim, then adds exactly that one entry manually BEFORE S1-A (steps and a registry script in the policy file; alternative through AutoCAD's own interface). S1-A's raw read A1-R6
  captures the actual value; an extra, missing, duplicate or recursive entry created by the campaign, or a spelling difference = **STOP**. Pre-existing entries enter the class-defining string and are not removed or extended. Any change after S1-A = new tuple,
  no evidence transfer.
- SECURELOAD: intended value = what the machine has, not changed, captured by S1-A (A1-R5). Risk recorded: a NETLOAD under SECURELOAD that prompts makes the run INVALID; the entry cannot be proved before S1-B's NETLOAD.
- This policy is narrower than the "one recursive trusted root" of package v3 3.5.4 / decisions 239 R-F (by the Owner's instruction); the Coordinator should acknowledge the narrowing.

## 3. Evidence and scratch roots, ACL (item 3)

ROOT_ACL_RECORD: `D:/I52-CT21D-HOST/records/root-acl-record.md` sha256 `465645fba1dbdc9a76b9a4a5e58236136254bb2370a4fcc769574a3a9ae60d71` (raw read-only ACL capture `root-acl-current-raw.txt` sha256 `63cb8cca8333961901dfddfbe92fb367699fe366d2392ede5065ac98f527eda2`).
**ACL_STATUS = SPECIFIED_NOT_APPLIED.**

```text
EVIDENCE_ROOT (parent) = D:\I52-CT21D-HOST\evidence
SCRATCH_ROOT  (parent) = D:\I52-CT21D-HOST\scratch
PRIVATE_COPY  (parent) = D:\I52-CT21D-HOST\private-copy        per-run children <parent>\<runId>, created EMPTY by the CAD manager before each session (NOT created: run ids do not exist)
```

Verified by reading: separate sibling folders (no nesting); D: is a local fixed NTFS drive (no provider, no substituted drive); drive-letter paths; no reparse point in the roots or their ancestors; outside every OneDrive root; `NotContentIndexed` set on the host root and the three
parents; all three parents empty; no library exists, so no library or private-copy path lies inside a root. Current effective ACL: inherited from `D:\`, **Authenticated Users have modify and Users read** on all of them, so C-13 stays open.

ACL specification (commands for the CAD manager in the ROOT_ACL_RECORD, parts 1 to 3; NOT executed): host tree stops inheriting and drops Authenticated Users/Users; the TRUSTEDPATHS folder is limited to Administrators, SYSTEM and the CAD manager (read-only attribute on the 8 files);
per-run evidence child = the AutoCAD account may read and CREATE new files/directories (write data, append, write attributes) with an explicit DENY of delete and delete-child; scratch child = create + read (CAD manager: modify; a distinct AutoCAD account may delete only what it created);
private-copy child = read only; writes elsewhere denied by omission. Limit stated: the instruments run as the same account as AutoCAD, so the ACL protects against other accounts and accidents, and Q-O-1's limits rest on the instrument's own root checks.
Cleanup policy: evidence is never deleted by the campaign; scratch is cleaned only after `scratch-verify` returned 0, the CAD manager's copy of each declared scratch DWG (create-new, sha256 equal to `rs-run.scratch.files`) and `tools seal`; every deletion goes into the cleanup ledger.

## 4. Canonical build identity (item 4) and 5. Hash table ratification

Hash ratification record: `D:/I52-CT21D-HOST/records/hash-ratification-record.md` sha256 `7c72aaf6018a393d00a349372aadbf51347597a92bc13f3854d6038a304006f8` (script `prehost_verify.py` sha256 `b564554e156e87053db04a814d2e994d020faf8429f228442f1e75eb36636468`). 55 rows computed fresh, **0 mismatches**; no difference with decisions 244/246.

- SOURCE_HEAD_SHA = 642ba39230e6e98b1565fde4d8a4d3a246cc9ef3 (clean detached tree, `git status` clean after the offline tools ran); SDK 8.0.423; configuration Release; DLLs carry `+642ba392...`.
- Canonical binaries (re-hashed, equal to the record and present in both build hash logs): R0 `fcaf938e0eb79f70035dd9dd25184b8d6c38860581f35912e4ccb1ccbd051d3c`; Core (one byte set in the declared-set, tools-r0 and tools-rs copies) `6bf5957bcbce3110990c482a33cdfd4729aaf8f5989b363ac9c8443703469fb6`;
  Rs `46dfed73a2edbbe469e9285fcc0a85248ef6f3c4c7d0468fa067e7d7e5eecd10`; Rs.Core `03998dca344c650f0edd6c21071f57662d9b913f14dc1ea95b57bddedc36e78e`; tools R0 `d8cf6facc324b72fffbdace4d5e03f873fdafd51b5f9670bc57cbfd28bbf523e`; tools-rs `d2109afe2ca6796fe5824b874668b345de2985c16f7142402aeb195a742334f9`; the four pdb (informational) match.
- Lists: 5 RS + 4 R0 allowed-API/privileged/command lists MATCH the package v3 3.3 and decisions 244 values; 6 RS + 6 R0 schemas MATCH; protocol files, `HASHES-RS.txt` `7f190d2f5f16f36cb3860c6af484baae5e240a89d588abc2183bc875f64b16a1`, both READMEs MATCH; each row also equals the git blob at 642ba392 and at the tip.
- Package: package v3 blob `0c9e331ce6f69537df363668714edb3b91baba12` equal at build commit and tip; `package-manifest.txt` `229830cec6dcf233530cea7c6e1f1cd693eec8c04f2396a51f1540e22c85bc21` and all its 34 lines re-hashed (0 differences); freeze-v35 package hash `D9FD41B4...47554` as in the preparation log (guard not re-run in this pass).
- Prohibited-API analysis RE-RUN on the canonical DLL bytes with the canonical offline tools (AutoCAD not touched): `scan` R0 exit 0 CLEAN; `imports-allowlist-check` R0 exit 0 CLEAN; `scan-rs` exit 0 CLEAN (Rs.Core 468 rows/0 waived, Rs.Tools 443, Rs.dll 358 rows/57 waived/53 privileged call sites), lists CLEAN, list hashes printed equal;
  `ledger-verify` exit 0 "no difference"; privileged lists observed 18/15/4 (R0) and 30/60/4 (RS), identical to the previous output. Logs (outside the repo): `D:/I52-CT21D-HOST/logs/prehost/` (scan-r0 `2015a3e2...`, imports-check `bdaebf32...`, scan-rs `e419936c...`, ledger `eb745a38...`).
  Tests (509 R0, 1332 RS) and the freeze guard were not re-run (they need a build).
- Source-review records (committed at 642ba392 and at the tip, unchanged, equal to the files on disk), sha256:

| File (`docs/automation/evidence/...`) | sha256 |
|---|---|
| `I-52-ct21d-r0-instruments/round1-implementation-and-reviews.json` | `1870ef3366874d9d7623f9a93ff5d774b7a0a1a62b4f643fbbc119b7c84bbee2` |
| `I-52-ct21d-r0-instruments/round2-fix-and-rereviews.json` | `2a6991490dfc6ef2218d828143c15abaef49efb58ed18d95be320998ad696af2` |
| `I-52-ct21d-r0-instruments/workflow-fix-r0.js` | `03a187aa56a8a92626fcadb264c5958f0cf9b35ac59572b2faf8ab6445ad8bf2` |
| `I-52-ct21d-r0-instruments/workflow-implement-r0.js` | `f353f01f275fa899639a8a88bb377a1ea32dbc054887b9628fd5a50cac86b4a4` |
| `I-52-ct21d-rs-instruments/architect-rs-review.md` | `f610df5604d68c1cbab486eff2b97da1fd6b40145890d67bdf16b0039869a36e` |
| `I-52-ct21d-rs-instruments/rs-implementation-and-review-rounds.json` | `208e7c02630f362ec6f7bac6a79e19dee09d81ddd4979cebbf3b59e3237712be` |
| `I-52-ct21d-rs-instruments/workflow-rs-instruments.js` | `08f977e6603c1c076577f6fe070bb7638b400abc1e915287a402f4713603114d` |

```text
CAD_MANAGER_HASH_RATIFICATION = APPROVED   (all 55 rows MATCH; recorded by the assistant acting on the CAD manager's explicit instruction; the CAD manager remains accountable; no partial approval)
```

## 6. Tuple phase 1 (item 6)

`D:/I52-CT21D-HOST/records/tuple-phase1-S1-v2.json` sha256 `dd15b39e3e3782a3d1e98d17ed42c21faaf3fc39889691038605066ca30c93a3` (v1 `tuple-phase1-S1.json` kept, sha256 `64cf654be47163fc29cbfe13fd52790e38f95aacb1c716d3cd70b738537fc54d`). Adds: class record v2 hash, TRUSTEDPATHS policy hash,
roots and ROOT_ACL_RECORD hash, re-verification results, source-review hashes, source/tip/drift identity. No runtime/session facts. **OPEN, each with its reason:** library path/hash and template (not provided); sessionId and runId (composed by the Coordinator's authorization);
designationSha256 (not composable); buildTupleDigest (after phase 2; the v1 partial candidate is retired); machineClassLabel (S1-A); TRUSTEDPATHS entry and ACL application, pre-existing entries (CAD manager); packageManifestSha256 adoption; freeze acknowledgement; attestations; signatures.

## 7. Library and template (item 7)

`LIBRARY_DWG = NOT_PROVIDED`, `S1_TEMPLATE_DWG = NOT_PROVIDED`. Read-only bounded search (record `library-template-search.md`, sha256 `a702f5245227dca618ad567ec545f0a538c82b528d7455542a546d0c118275f7`): no `blocks-library*` in `D:\Documentos`, the user Documents, the repository and worktree `assets`/`deploy`
folders (only `.gitkeep` in `assets\blocks` and `assets\templates`), or the Autodesk install folder; the only DWG found (17 structural-section drawings of another application) are not candidates. Stock AutoCAD 2025 templates exist but none is designated; nothing was copied, opened or modified.
Consequence: S1-B, S1-C, S2, S3, S4 NOT_ELIGIBLE until the CAD manager designates both.

## 8. declaredSet and pins (item 8)

`declared-set.json` version `declaredSet-642ba392-v1` is **unchanged** (its paths already name the TRUSTEDPATHS folder): sha256 `19a64d1de93fbc0afbad473c8852ee7ae7945cf56646d2d2aa5c9d001b1acf13`, recomputed from the four DLL files and equal. The four `.pin` files each contain the lowercase sha256 of their DLL + LF (65 bytes): Core `60e939bd...10b8a`, R0 `26858a77...7ca83`,
Rs.Core `7e5920b8...d5fc`, Rs `52a8e1b0...abd5c` (file hashes; full values in the preparation report). Core DLL identical for R0 and RS.

## 9. Session eligibility (item 9; full table `session-eligibility-v2.md`, sha256 `92e21c25dd8673a8a557d5d38ff892ebc545930b7cdac96dffa6a91244736650`)

Met by recorded evidence: C-06, C-07, C-08/C-09/C-10 (where applicable), C-23; C-11 met by the delegated ratification (MET-D; the CAD manager's own signature is still absent). Still open: C-01 (signature), C-02 (policy applied, class record published), C-03 (tuple closure), C-04 (label), C-05, C-12, C-13 (ACL applied, per-run roots), C-14 (ledger), C-15 (library), C-16, C-17, C-18, C-19, C-20, ORD, TPL, P4+ (Coordinator authorization).

```text
S1-A = NOT_ELIGIBLE (13 open)     S1-B = NOT_ELIGIBLE (16)     S1-C = NOT_ELIGIBLE (17)     S2 = NOT_ELIGIBLE (16)     S3 = NOT_ELIGIBLE (16)
S4   = NOT_ELIGIBLE / BLOCKED (18; PRE-1..PRE-4, sealed pre-registration, R-M ruling on R1(b), S3 result)     TOL_SCALE = UNSET (not anticipated)
```

New points for the Coordinator: N-1 narrower TRUSTEDPATHS policy than package v3 3.5.4; N-2 `runId` pattern `^HGP-H[0-9]+-` cannot hold the package name `H3b` of S1-C; N-3 the RS designation requires `scratchRoot` while the template says none for S2; N-4 field names differ between the R0 and RS designations;
N-5 NETLOAD-under-SECURELOAD prompt risk; N-6 the ACL limit (same account). Details in the eligibility record.

## 10. FIRST_NON_GOVERNING_HOST_RUN

`FIRST_NON_GOVERNING_HOST_RUN = NOT_AUTHORIZED`. `CT21D_AUTHORITY_BASELINE_READY = FALSE`, `CT21D_EXECUTION_READY = FALSE`, `CT21D_EXECUTION = NOT_AUTHORIZED`, `HOST_RUN_STARTED = FALSE`, `G3 = STOPPED`, `CIA = UNKNOWN`, `FALLBACK = ALT-21E`.

## Manual actions left for the CAD manager (in order; commands are in the records named)

1. Sign (name, role, asset id) the designation record and the machine-class record v2; publish them. Acknowledge the freeze statement. Decide whether to adopt `packageManifestSha256`.
2. Apply the ACL hardening: `root-acl-record.md` section 4.2 part 1 (set the two account variables first); read back the ACLs; optionally the smoke test of 4.3.
3. Record the pre-existing TRUSTEDPATHS string and add the one entry: `trustedpaths-policy.md` section 3 (method B script, or method A); complete and sign the decision record (section 4). Do not touch SECURELOAD.
4. Designate the real `blocks-library.dwg` (path, sha256) and the S1-C template; supply them to the tuple.
5. Sign the hash table (or re-run `prehost_verify.py`), the stop-condition sheet and the no-retry statement; prepare the phase 2 procedure and the process-list/NETLOAD attestation scripts.
6. After the Coordinator's authorization names a run id: run part 2 and part 3 of the ACL commands for that run id, open the cleanup ledger, make the private library copy, compose and hash the designation file.

## Not done (could not or must not)

Reading MachineGuid/SECURELOAD/TRUSTEDPATHS/profile attributes, applying any ACL or setting, creating per-run roots, composing designation files, computing buildTupleDigest, closing the tuple, signing anything, re-running tests and the freeze guard (they need a build), any rebuild, any rebase, any git add/commit/push.
