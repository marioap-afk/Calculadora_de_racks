# I-52 — CAD manager attestation (human) and readback verification of the manual pre-host actions

> Recorded 2026-10-01 after the CAD manager's message "MANUAL PRE-HOST ACTIONS COMPLETED". Decisions section 248. Two things are kept apart on purpose: (A) the human attestation, quoted as the Owner/CAD manager wrote it; (B) the assistant's read-only readback, which is technical verification and NOT a signature.

## A. Human attestation (verbatim extract; signed by the Owner/CAD manager)

```text
CAD_MANAGER_ATTESTATION = SIGNED_BY_OWNER_CAD_MANAGER
I ratify: designated machine; machine-class record v2; 55/55 canonical hash table; canonical source SHA
642ba39230e6e98b1565fde4d8a4d3a246cc9ef3; SDK 8.0.423; declaredSet / pins; evidence and scratch roots; TRUSTEDPATHS policy.
Do not represent previous assistant verification as the human signature. Preserve it separately as technical verification.
```

The earlier assistant verification (hash table 55/55, scans, `CAD_MANAGER_HASH_RATIFICATION = APPROVED` recorded by the assistant on the CAD manager's instruction; section 247) is preserved as TECHNICAL verification, not as the human signature.

## B. Read-only readback (assistant, 2026-10-01; AutoCAD not running: 0 `acad` processes; nothing was changed or repaired)

### B.1 Technical hash verification (fresh)
The eight files of `canonical-642ba392/declared-set` hash to the recorded values: `Core.dll` 6bf5957b…, `R0.dll` fcaf938e…, `Rs.Core.dll` 03998dca…, `Rs.dll` 46dfed73…; `.pin` files 60e939bd…, 26858a77…, 7e5920b8…, 52a8e1b0…; `declared-set.json` 19a64d1d…acf13; `package-manifest.txt` 229830ce…bc21. All MATCH; no drift. (The `declared-set` directory timestamp reads 08:40, after the preparation records; the file timestamps and hashes are unchanged.)

### B.2 ACL part 1 — NOT APPLIED
`ACL_COMMAND_EXPECTED` = part 1 of `root-acl-record.md` section 4.2: host root `/inheritance:d`, remove `*S-1-5-11` and `*S-1-5-32-545`, grant the CAD manager `(OI)(CI)M`; `declared-set` `/inheritance:r` with Administrators/SYSTEM/CAD manager only and `attrib +R` on the dll and pin files; read back with no `Authenticated Users` / `Users` on the host tree.

`ACL_READBACK_RAW` (same shape on `D:\I52-CT21D-HOST`, `evidence`, `scratch`, `private-copy` and `canonical-642ba392\declared-set`; localized account names as printed):
```text
BUILTIN\Administradores:(I)(F)  BUILTIN\Administradores:(I)(OI)(CI)(IO)(F)
NT AUTHORITY\SYSTEM:(I)(F)      NT AUTHORITY\SYSTEM:(I)(OI)(CI)(IO)(F)
NT AUTHORITY\Usuarios autentificados:(I)(M)   NT AUTHORITY\Usuarios autentificados:(I)(OI)(CI)(IO)(M)
BUILTIN\Usuarios:(I)(RX)        BUILTIN\Usuarios:(I)(OI)(CI)(IO)(GR,GE)
```
Attributes read: the host root and the three roots carry `NotContentIndexed`; the `declared-set` folder does not; the eight files are `Archive` (NOT read-only).

`ACL_EXPECTED_VS_ACTUAL` = DIFFERENT: every ACE is still inherited (`(I)`), `Authenticated Users` still has modify and `Users` read-and-execute on the host tree, there is no explicit CAD-manager ACE, `declared-set` still inherits, and the dll/pin files are not read-only. The state is identical to the one captured before the manual change (`root-acl-current-raw.txt`). **ACL_PART1_STATUS = NOT_APPLIED (or applied to a different object than the one read). STOP; no automatic repair.**

### B.3 TRUSTEDPATHS
`TRUSTEDPATHS_BEFORE_RAW` = **NOT PRESERVED**: there is no `trustedpaths-apply-*.txt` log and no decision record in `D:\I52-CT21D-HOST\records\`, so the pre-change string was not captured where the policy requires it.

`TRUSTEDPATHS_AFTER_RAW` (registry `HKCU\...\ACAD-8101:409\Profiles\<<Unnamed Profile>>\Variables`, value `TRUSTEDPATHS`, REG_SZ, read-only `reg query`):
```text
D:\I52-CTDA-R2H\cf302acd\transfer;D:\I52-CTDA-R2H;D:\I52-CTDA-R2H\40a591e7\transfer;D:\I52-CTDA-R2H\9323e55a;D:\I52-CTDA-R2H\9323e55a\run;D:\I52-CTDA-R2H\6e445fa8\run;D:\I52-CTDA-R2H\c4037098\run;D:\I52-CTDA-R2H\2887d6c5\run;D:\I52-CTDA-R2H\229c9e65\run;D:\I52-CTDA-R2H\e864a093\run;D:\I52-CTDA-R2H\afa65bc0\run;D:\I52-CTDA-R2H\bef11ce2\run;D:\I52-CTDA-R2H\bef6091b\run;D:\I52-AUTH15-HV\9674dcc9\run;D:\I52-AUTH15-HV\20158fa5\run;D:\I52-AUTH15-HV\fcca6e6c\run;D:\I52-AUTH15C1-HV\579de1af\run;D:\I52-AUTH15C1-HV\b7ee683e\run;D:\I52-CT21D-HOST\canonical-642ba392\declared-set
```
Structural check against the policy: the authorized entry is present exactly once, last, without a trailing backslash and non-recursive (no `\...`); no RackCad path was added; the other 18 entries are older research folders (I-52 CT-DA and AUTH-15 campaigns), which the policy records as pre-existing and which become part of the class-defining string.

`TRUSTEDPATHS_DELTA` = **UNVERIFIABLE** (cannot be classified `EXACT_EXPECTED` without the BEFORE string). It is consistent with "BEFORE + one appended entry" but that is an inference, not evidence. Needed: the CAD manager's verbatim pre-change string (or a restore point) to compare character by character.

### B.4 Roots
`D:\I52-CT21D-HOST\evidence` and `...\scratch` and `...\private-copy`: exist, are directories, hold 0 entries, are `NotContentIndexed`, no reparse points. The ACL does not correspond to an approved readback (B.2).

### B.5 Phase-2 preparation
No scripts for phase 2 exist: nothing in `records/` or in the committed tree captures, at S1-A startup, the PID, process start timestamp, effective AutoCAD build and profile, the loaded-module list (SESSION_START), the relevant process list, the `.pin` check outcome or the NETLOAD transcript. The package v3 defines the procedure (3.7 and P4/C-05) and the instruments record PID and process start in `volatile`, but the attestation scripts and the module-list capture are not written. **PHASE2_ATTESTATION_PREP = NOT_READY.**
