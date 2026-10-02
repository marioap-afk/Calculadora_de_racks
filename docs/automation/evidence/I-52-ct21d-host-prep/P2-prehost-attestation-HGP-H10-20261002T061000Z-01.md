# I-52 CT-21D — P2 PRE-HOST ATTESTATION (H10) for `HGP-H10-20261002T061000Z-01` — PREPARED; NOT SIGNED; NOT called Phase-2 binding

```text
RECORD_ID                         = CT21D-P2-PREHOST-ATTESTATION-HGP-H10-20261002T061000Z-01
BASIS                             = P2 text section 3.1 (PHASE2_CAPTURE_FOR_P2 = WAIVED; Coordinator "P2 H10 RUN PREPARATION")
RUN_ID                            = HGP-H10-20261002T061000Z-01     ACTIVITY = H10
P2_EXECUTION                      = NOT_AUTHORIZED
ATTESTATION_STATUS                = PREPARED_BY_ASSISTANT_READ_ONLY; THE CAD MANAGER MUST ATTEST AND SIGN; the lines marked PENDING need the CAD manager's action
```

## A. Facts read by the assistant (read-only, 2026-10-02; AutoCAD NOT started; nothing created, changed or repaired)

| Line | Value | Source | Status |
|---|---|---|---|
| DESIGNATED_MACHINE | <DESIGNATED_MACHINE> (name kept outside the repository) | machine-class-record v2 (CT21D-MCLASS-PREP-642ba392-02) vs `$env:COMPUTERNAME` now | MATCH = YES |
| EXPECTED_AUTOCAD_BUILD | AutoCAD 2025 - English; acad.exe FileVersion R25.0.171.0.0; sha256 2a75996fd2a5c5ee0376fd5ba7cced227a098193ff495e8ceeda3a96fe326422 | machine-class record vs `acad.exe` file info and hash now | MATCH = YES (file identity; the running build is not observed by P2) |
| EXPECTED_PROFILE | `<<Unnamed Profile>>` (registry-configured current profile name) | machine-class record vs HKCU Profiles default value now | MATCH = YES (the CAD manager must observe the profile actually in use at start) |
| ACAD_PROCESS_COUNT_PRESTART | 0 | `Get-Process acad` now | MATCH = YES |
| TRUSTEDPATHS_OBSERVED_BASELINE_SHA256 | a9ba0fd1c2f15dfc7ee4477e0ea62e710ac842d89d2b0e18102b2362553a2498 (19 entries) | read-only registry read of the value now, UTF-8 sha256 | MATCH = YES (`trustedpaths-baseline-record.md`; CAD manager attests the in-effect value) |
| P2_PROTOCOL_SHA256 | e9e53a9e8ed375053ed568cefc3a41059d2cd1ee44718cf4f478e402d1554c59 (blob 2ff43675181e98f5752e38613e85e87ad436b88a) | `sha256sum` + `git rev-parse HEAD:` at 22a4ca4d | VERIFIED = YES |
| P2_ALLOWLIST_SHA256 | eeb7ffc0f3119457951535003ac67662c955c2139bb43b81c502eb063f0d4b36 (blob d1633a85f899d2185e5dd954cce374ff3a4aaefe) | same | VERIFIED = YES |
| P2_SCHEMA_SHA256 | fb8996d04489bc8f04f4e4a019b430c0a7dba3bc4cbe1a0a1be24cab3b681da3 (blob 83c8fb36cd64291751fb187edca2722365229dd8) | same | VERIFIED = YES |
| P2_ANALYZER_SHA256 | 74ce331fde145e25d0c270355c829d9fdc4e837522480137b5948122d1832f39 (blob a3268ad409700a81f1044630c6ffed47880611fa) | same | VERIFIED = YES |
| P2_LEDGER_SHA256 | bd49d0f4d7915d6126eadb63c8f80f620c920830d179c11979c184c6b564bd5a (blob 7feeafc6d603c14990c5b77ff0782a8f65f83255) | `sha256sum`; `sha256sum -c HASHES-I1-V2.txt` = 7/7 OK | VERIFIED = YES |
| ERRATUM_SHA256 | 5b6cc264e203c8c4de6a384b4e35cfd4b04ab5a18e38617ca68916b70fd780ab (blob 60e5f993cc5f57a7034fc26dc6f1b8761876b6ea) | same | VERIFIED = YES |
| EVIDENCE_CHILD_PATH | D:\I52-CT21D-HOST\evidence\HGP-H10-20261002T061000Z-01 | not created | EXISTS = NO (as required before creation) |
| EVIDENCE ROOT | `D:\I52-CT21D-HOST\evidence`: no reparse point on the chain; fixed NTFS disk; NotContentIndexed; not under OneDrive | read-only | OK |
| RACKCAD_ALLOWED / NETLOAD_ALLOWED | NO / NO | protocol 3.1 item 7 | n/a |

## B. Items that need the CAD manager (PENDING; the assistant does not create the child or touch any ACL)

```text
EVIDENCE_CHILD_CREATED_BY_CAD_MANAGER = PENDING
EVIDENCE_CHILD_ACL (part 2, NeedScratch = false, NeedCopy = false) APPLIED = PENDING
EVIDENCE_CHILD_READBACK (part 3) = PENDING     EMPTY = PENDING     NotContentIndexed = PENDING     NO_REPARSE = PENDING     LOCAL_FIXED_DISK = PENDING
TRUSTEDPATHS IN EFFECT AT START = CAD MANAGER ATTESTS     PROFILE IN USE AT START = CAD MANAGER OBSERVES
ATTESTED_BY (CAD manager) = ____________________     DATE_UTC = ____________________     ANY "NO" = P2 DOES NOT START
```

Exact commands (only the two switches and the run id differ from the governed plan; files outside the repository; **NOT executed by the assistant**):

1. `D:\I52-CT21D-HOST\records\h10-child-acl-part2.ps1` (sha256 cd8d1c4732e081a18fabb6d65257d083b96d93590b563b0da9b72800de7358a1): refuses if `acad.exe` runs or if the child exists; creates the evidence child only, `attrib +I`, inheritance removed, Administrators and SYSTEM full, the CAD-manager account create-only, DELETE and DELETE-CHILD denied.
2. `D:\I52-CT21D-HOST\records\h10-child-acl-part3-readback.ps1` (sha256 95281f84635e428d1977466e8806989517419b6fa6374b5be50249edf3b697be): read-only; throws if not empty / indexed / reparse / not fixed; prints the ACL for the assistant to compare.
Run from an **elevated** PowerShell 7 prompt of the CAD manager (the ACL commands need it), with AutoCAD closed:

```powershell
pwsh -NoProfile -File D:\I52-CT21D-HOST\records\h10-child-acl-part2.ps1
pwsh -NoProfile -File D:\I52-CT21D-HOST\records\h10-child-acl-part3-readback.ps1
```
Expected evidence-child readback (comparison on the meaning of the ACEs; no auto-repair on mismatch): inheritance removed; `BUILTIN\Administradores:(OI)(CI)(F)`, `NT AUTHORITY\SYSTEM:(OI)(CI)(F)`, `<CAD_MANAGER_ACCOUNT>:(OI)(CI)(RD,REA,RA,X,RC,S,WD,AD,WA,WEA)` and the DENY `(OI)(CI)(DE,DC)`; attributes `Directory, NotContentIndexed`; 0 entries.

```text
P2_EXECUTION = NOT_AUTHORIZED
```
