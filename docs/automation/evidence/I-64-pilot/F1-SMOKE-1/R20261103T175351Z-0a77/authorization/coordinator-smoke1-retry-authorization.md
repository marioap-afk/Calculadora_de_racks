# I-64 — Autorización del reintento de Smoke-1, RunId R20261103T175351Z-0a77 (literal)

Recibida el 2026-11-03 (~17:58Z) por el chat de la sesión responsable, que esperaba en COORDINATOR_SMOKE1_RETRY_AUTHORIZATION_REQUIRED
(paquete `return-package.md`). El RunId es el que la sesión propuso a las 17:53:51Z; queda asignado por esta autorización.

```text
I-64 — COORDINATOR SMOKE-1 RETRY AUTHORIZATION
Candidate:
09f2f1c3c4e64491018ef1489ce4dd3030d1e4a8
Exact DLL SHA-256:
2923E7F2DF4F62594810DBBC33003D36E6E4382EDECFB7E43C4A6C7C01F5AE26
New RunId:
R20261103T175351Z-0a77
Disposition:
AUTHORIZED FOR ONE SMOKE-1 RETRY OCCURRENCE
Current:
AssemblyMap\RackCad.Plugin = ABSENT
Historical provenance of its removal:
UNKNOWN
Disposition:
ACCEPTED AS CURRENT BASELINE STATE
Do not claim a governed cleanup occurred.
Before BASELINE:
capture AssemblyMap read-only.
After AutoCAD is fully closed:
capture AssemblyMap read-only again.
Any creation or change attributable to the occurrence:
STOP / Coordinator review.
Do not modify AssemblyMap from I-64.
Existing RackCad entries:
ACCEPTED AS HISTORICAL HOST STATE
because they pre-exist this occurrence and are currently unchanged.
Capture the relevant entries read-only before and after the occurrence.
Additionally, immediately after the exact Candidate NETLOAD, inspect the
AutoCAD process read-only and prove the loaded RackCad.Plugin.dll:

* exact path;
* exact SHA-256.

Required SHA:
2923E7F2DF4F62594810DBBC33003D36E6E4382EDECFB7E43C4A6C7C01F5AE26
A different loaded RackCad.Plugin.dll or ambiguous simultaneous RackCad
Plugin load:
STOP immediately.
Do not continue Smoke-1.
A normal update to NETLOAD dialog-history UI state is not by itself a
Candidate-integrity failure.
The AutoCAD activity observed on 2–3 November is classified:
UNKNOWN_HOST_ACTIVITY_PRE_BASELINE
Do not attribute it to Owner, I-62, I-66 or another initiative without
evidence.
It does not block this occurrence because the fresh host recheck occurred
after it and the new BASELINE will capture the actual current state.
Accepted fresh references:
acad.exe / RAM Elements:
0 / 0
Active profile:
<<Unnamed Profile>>
acad.exe SHA-256:
2A75996FD2A5C5EE0376FD5BA7CCED227A098193FF495E8CEEDA3A96FE326422
TRUSTEDPATHS:
19 entries
TRUSTEDPATHS chain SHA-256:
a9ba0fd1c2f15dfc7ee4477e0ea62e710ac842d89d2b0e18102b2362553a2498
ApplicationPlugins:
no RackCad
ACL:
accepted / unchanged
I-52 H10:
HELD from I-64 BASELINE through I-64 POST and I-52 post-run recheck
Before BASELINE:

* require acad.exe = 0;
* Owner/CAD manager reads current SECURELOAD read-only;
* record as SECURELOAD_BASELINE_CURRENT;
* T3C BASELINE must independently read the same value.

Mismatch:
STOP before AutoCAD.
Do not change SECURELOAD.
PaletteKeyAcceptance:
ACCEPTED
One-time non-persistent NETLOAD:
AUTHORIZED
for the exact Candidate DLL SHA above only.
Forbidden:

* TRUSTEDPATHS modification;
* SECURELOAD modification;
* profile modification;
* registry/autoload registration;
* bundle installation;
* ACL change;
* persistent environment changes.

Run a NEW:
BASELINE
AutoCAD occurrence
POST
DIFF
operator record
Do not reuse any previous BASELINE or POST.
Critical early checks:

1. Candidate DLL loaded path/hash is exact.
2. RACKPANEL at SM-03 is visible and usable after C1.
3. Do not manually rescue/reposition an unusable palette before the expected
smoke step.
4. If C1 fails again: STOP.

Then continue the accepted T3D sequence normally.
After AutoCAD closes:

* require acad.exe = 0;
* capture AssemblyMap again;
* capture AppDataLow/CoreUser again;
* take T3C POST;
* run DIFF with PaletteKeyAcceptance=ACCEPTED;
* return host to I-52;
* require its post-run recheck before H10 can proceed.

Return at:
COORDINATOR_SMOKE1_REVIEW_REQUIRED
with:

* operator record;
* S1/SM results;
* loaded DLL path + SHA;
* C1 SM-03/SM-17 observations;
* PR-12/PR-13 host results;
* BASELINE/POST hashes;
* T3C DIFF result;
* AssemblyMap before/after;
* AppDataLow/CoreUser before/after;
* acad.exe final count;
* I-52 recheck;
* proposed PASS / NOT PASS / STOP.

Do not start F2.
```
