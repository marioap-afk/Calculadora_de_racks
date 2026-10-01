# I-52 CT-21D — TRUSTEDPATHS observed-baseline record

```text
RECORD_ID                         = CT21D-TRUSTEDPATHS-OBSERVED-BASELINE-01
BASIS                             = Coordinator micro-ruling on the TRUSTEDPATHS sub-gate (decisions section 250)
TRUSTEDPATHS_PREVIOUS_STATE       = NOT_PROVABLE   (no authoritative pre-change capture exists; see section 249)
TRUSTEDPATHS_CURRENT_SNAPSHOT     = ACCEPTED_AS_OBSERVED_MACHINE_CONFIGURATION_BASELINE
SOURCE                            = read-only registry read, HKCU\Software\Autodesk\AutoCAD\R25.0\ACAD-8101:409\Profiles\<<Unnamed Profile>>\Variables, value TRUSTEDPATHS (REG_SZ)
OBSERVED_AT                       = 2026-10-01, after the section 249 commit (88138f01); read by the assistant; AutoCAD process 37312 (the user's own drawing session, started 10:31 local) was running at the time and is not part of the campaign
NORMALIZATION                     = the value exactly as read, UTF-8 without BOM, no trailing newline; entries split on ';'
VALUE_LENGTH_CHARS                = 573
ENTRY_COUNT                       = 19
LEGACY_ENTRY_COUNT                = 18
AUTHORIZED_ENTRY                  = D:\I52-CT21D-HOST\canonical-642ba392\declared-set   (occurs exactly once, position 19, last)
RECURSIVE_FORMS                   = 0
RACKCAD_TRUSTED_PATH              = NO
EMPTY_SEGMENTS                    = 0
CHAIN_SHA256                      = a9ba0fd1c2f15dfc7ee4477e0ea62e710ac842d89d2b0e18102b2362553a2498
```

## Chain as observed
1. `D:\I52-CTDA-R2H\cf302acd\transfer`
2. `D:\I52-CTDA-R2H`
3. `D:\I52-CTDA-R2H\40a591e7\transfer`
4. `D:\I52-CTDA-R2H\9323e55a`
5. `D:\I52-CTDA-R2H\9323e55a\run`
6. `D:\I52-CTDA-R2H\6e445fa8\run`
7. `D:\I52-CTDA-R2H\c4037098\run`
8. `D:\I52-CTDA-R2H\2887d6c5\run`
9. `D:\I52-CTDA-R2H\229c9e65\run`
10. `D:\I52-CTDA-R2H\e864a093\run`
11. `D:\I52-CTDA-R2H\afa65bc0\run`
12. `D:\I52-CTDA-R2H\bef11ce2\run`
13. `D:\I52-CTDA-R2H\bef6091b\run`
14. `D:\I52-AUTH15-HV\9674dcc9\run`
15. `D:\I52-AUTH15-HV\20158fa5\run`
16. `D:\I52-AUTH15-HV\fcca6e6c\run`
17. `D:\I52-AUTH15C1-HV\579de1af\run`
18. `D:\I52-AUTH15C1-HV\b7ee683e\run`
19. `D:\I52-CT21D-HOST\canonical-642ba392\declared-set`  <- I-52 authorized entry

## Disclosure (mandatory)
- The historical delta is **unavailable**: this record does NOT assert that the manual change of 2026-10-01 was exactly "old value + I-52 entry", and the delta is never called `EXACT_EXPECTED`.
- The 18 entries other than the I-52 entry are **inherited**: their historical introduction was not proven here (the older campaign tuples show that the first 16 existed on 2026-09-29; the two `D:\I52-AUTH15C1-HV` entries were seen as trusted by 2026-09-29 21:43; none of that proves the state immediately before the I-52 append).
- The chain above is the **observed baseline** of this machine/session configuration for this campaign.
- S1-A will record the actual chain again as session evidence (A1-R6).
- Future drift: from this baseline on, any byte or textual change of the normalized chain is CONFIGURATION DRIFT, a new tuple/configuration instance and no silent evidence transfer. The 18 legacy entries are NOT removed by this gate; removing them is itself a configuration change requiring new tuple handling.
- Not changed by the assistant: TRUSTEDPATHS, SECURELOAD, the registry, ACLs.
