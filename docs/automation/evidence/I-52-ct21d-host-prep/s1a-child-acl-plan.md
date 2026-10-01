# I-52 CT-21D — S1-A per-run child ACL plan (PREPARED; NOT EXECUTED; the run id does not exist yet)

```text
RECORD_ID     = CT21D-S1A-CHILD-ACL-PLAN-01
SESSION       = S1-A (activity H1: I-1 machine/session fact capture, I-0 label, I-9 seal; NO NETLOAD, no RackCad, no instrument loaded)
RUN_ID        = <named by the Coordinator's S1-A authorization>; shape HGP-H1-<yyyymmddThhmmssZ>-<nn> (activity number H1, section 247 ruling 2)
STATUS        = PLAN ONLY. The CAD manager runs it AFTER the authorization names the run id and BEFORE phase 2; the assistant does not run it
CHILDREN      = evidence child ONLY (no scratch child, no private-copy child: S1-A uses no side database and no library copy)
```

Use part 2 of `root-acl-record.md` section 4.2 with these two switches set exactly as shown and the run id from the authorization; nothing else in the script changes:

```text
$NeedScratch = $false     # S1-A: no scratch root
$NeedCopy    = $false     # S1-A: no private library copy
$RunId       = '<RUN ID FROM THE AUTHORIZATION>'   # must match ^HGP-H\d+-\d{8}T\d{6}Z-\d{2}$ ; H1 for S1-A
```

Expected readback of the evidence child (what part 3 must show; the assistant compares it exactly after the CAD manager reports execution):
```text
D:\I52-CT21D-HOST\evidence\<RunId>   (inheritance removed; no inherited ACE)
  BUILTIN\Administradores:(OI)(CI)(F)
  NT AUTHORITY\SYSTEM:(OI)(CI)(F)
  <CAD_MANAGER_ACCOUNT>:(OI)(CI)(RD,REA,RA,X,RC,S,WD,AD,WA,WEA)
  <CAD_MANAGER_ACCOUNT>:(OI)(CI)(N)(DE,DC)        # DENY delete and delete-child
attributes: Directory, NotContentIndexed; 0 entries; not under a reparse point; fixed drive; not under OneDrive
```
(The exact printed form of the deny ACE and of the rights list is `HOST-TO-CONFIRM` until the CAD manager's readback exists; the comparison will be done on the ACE meaning, not on a guess of the print.)

Smoke test (optional, recommended, section 4.3): create a text file in the evidence child (must succeed), try to delete it (must be denied), set read-only (must succeed). No instrument, no AutoCAD.

Checks before S1-A that stay read-only for the assistant: no `acad.exe` process; the evidence child exists, is empty and has the readback above; the host root, `declared-set` and `TRUSTEDPATHS` still match their recorded baselines; the phase-2 folder bytes still match `HASHES-PHASE2.txt`.
