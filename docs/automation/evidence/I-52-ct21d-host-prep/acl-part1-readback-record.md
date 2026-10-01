# I-52 CT-21D — ACL part 1: readback record

```text
RECORD_ID                = CT21D-ACL-PART1-READBACK-01
BASIS                    = CAD manager report "ACL PART 1 COMPLETED" (manual execution of the exact section 247 commands, root-acl-record.md 4.2 part 1) and the assistant's READ-ONLY readback (decisions section 251)
READBACK_AT              = 2026-10-01 (after the manual change); acad.exe running: 0; nothing was modified by the assistant
ACL_COMMAND_EXPECTED     = root-acl-record.md section 4.2 part 1: host root /inheritance:d, /remove:g *S-1-5-11 and *S-1-5-32-545, /grant Mgr (OI)(CI)M; declared-set /inheritance:r, /grant:r Administrators and SYSTEM (OI)(CI)F and Mgr (OI)(CI)M, attrib +R on *.dll and *.pin; Acad = Mgr, so no extra grant
ACL_EXPECTED_VS_ACTUAL   = EXACT_MATCH
```

## Expected vs actual per object (ACEs compared as sets; localized account names as printed by icacls; in this repo copy the CAD-manager account is shown as <CAD_MANAGER_ACCOUNT>, the unredacted record is under D:/I52-CT21D-HOST/records/)
- `D:\I52-CT21D-HOST`: EXACT_MATCH
- `D:\I52-CT21D-HOST\evidence`: EXACT_MATCH
- `D:\I52-CT21D-HOST\scratch`: EXACT_MATCH
- `D:\I52-CT21D-HOST\private-copy`: EXACT_MATCH
- `D:\I52-CT21D-HOST\canonical-642ba392\declared-set`: EXACT_MATCH

## Raw readback (icacls)
```text
=== D:\I52-CT21D-HOST
BUILTIN\Administradores:(F)
<CAD_MANAGER_ACCOUNT>:(OI)(CI)(M)
BUILTIN\Administradores:(OI)(CI)(IO)(F)
NT AUTHORITY\SYSTEM:(F)
NT AUTHORITY\SYSTEM:(OI)(CI)(IO)(F)

=== D:\I52-CT21D-HOST\evidence
<CAD_MANAGER_ACCOUNT>:(I)(OI)(CI)(M)
BUILTIN\Administradores:(I)(F)
BUILTIN\Administradores:(I)(OI)(CI)(IO)(F)
NT AUTHORITY\SYSTEM:(I)(F)
NT AUTHORITY\SYSTEM:(I)(OI)(CI)(IO)(F)

=== D:\I52-CT21D-HOST\scratch
<CAD_MANAGER_ACCOUNT>:(I)(OI)(CI)(M)
BUILTIN\Administradores:(I)(F)
BUILTIN\Administradores:(I)(OI)(CI)(IO)(F)
NT AUTHORITY\SYSTEM:(I)(F)
NT AUTHORITY\SYSTEM:(I)(OI)(CI)(IO)(F)

=== D:\I52-CT21D-HOST\private-copy
<CAD_MANAGER_ACCOUNT>:(I)(OI)(CI)(M)
BUILTIN\Administradores:(I)(F)
BUILTIN\Administradores:(I)(OI)(CI)(IO)(F)
NT AUTHORITY\SYSTEM:(I)(F)
NT AUTHORITY\SYSTEM:(I)(OI)(CI)(IO)(F)

=== D:\I52-CT21D-HOST\canonical-642ba392\declared-set
<CAD_MANAGER_ACCOUNT>:(OI)(CI)(M)
NT AUTHORITY\SYSTEM:(OI)(CI)(F)
BUILTIN\Administradores:(OI)(CI)(F)
```

## Attributes read
host root and evidence/scratch/private-copy: Directory, NotContentIndexed; declared-set: Directory; the eight files (4 dll + 4 pin): ReadOnly, Archive (IsReadOnly = True)

## Notes
- Expected sets are derived mechanically from the pre-change capture (section 248: every ACE inherited, Authenticated Users M, Users RX) and from part 1: the inherited ACEs become explicit on the host root, the two accounts are removed, the CAD manager account gains (OI)(CI)M; the three roots inherit; `declared-set` carries exactly three explicit ACEs.
- `Authenticated Users` and `Users` are absent on the host tree. Roots remain empty (0 entries) and `NotContentIndexed`.
- The ACL protects against other accounts and accidents only (the instruments run as the same account as AutoCAD); see root-acl-record.md section 4.1 rule 3.
- Not changed by the assistant: any ACL, attribute, registry value or setting.
