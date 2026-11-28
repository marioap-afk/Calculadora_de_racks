# I-64 — Autorización de Smoke-1 del Coordinator (literal)

Recibida por el chat de la sesión responsable el 2026-10-12. RunId asignado por la sesión al recibirla: `R20261012T195007Z-4141`.

```text
I-64 — COORDINATOR SMOKE-1 AUTHORIZATION
Authorize exactly ONE I-64 Smoke-1 occurrence.
Assign one fresh RunId:
R<yyyyMMddTHHmmssZ>-<4hex>
This authorization permits AutoCAD launch ONLY for this Smoke-1 occurrence
and only under the coordinated host RELEASE supplied by the Owner/CAD
manager.
No repeat is authorized.
A repeat after FAIL, STOP or INVALID requires a new Coordinator decision and
a new RunId.
Candidate SHA:
4d79e91036acdaa3e62b7b54677f394593f581a0
Candidate DLL SHA-256:
E769219D2F5EDF6E0DE0867633C4A89B94EBBD7F2BD88CF20864CBCF75EF7512
Exact Candidate CI:
38177504770
push
4/4 SUCCESS
Local Candidate evidence already accepted:
Core Full:
13122 / 13122
UI Full:
1701 passed
17 existing skips
0 failed
Debug UI build:
PASS
Debug Plugin build:
PASS with only known MSB3277 warnings
Do NOT rebuild from another commit.
Do NOT create another I-64 commit before this Smoke-1 occurrence.
HOST_RELEASE =
RELEASED
Ordering is binding:
I-64 Smoke-1
→ I-64 POST
→ full AutoCAD close / acad.exe count 0
→ I-52 host recheck
→ I-52 P2 H10
I52_P2_H10_DURING_I64 =
HELD
No I-52 AutoCAD/host run may overlap the I-64 occurrence.
Active profile expected:
<<Unnamed Profile>>
acad.exe SHA-256 expected:
2A75996FD2A5C5EE0376FD5BA7CCED227A098193FF495E8CEEDA3A96FE326422
TRUSTEDPATHS expected entry count:
19
TRUSTEDPATHS exact normalized chain SHA-256:
a9ba0fd1c2f15dfc7ee4477e0ea62e710ac842d89d2b0e18102b2362553a2498
Source:
I-52 governed record
CT21D-TRUSTEDPATHS-OBSERVED-BASELINE-01
Do not infer or abbreviate this value in the BASELINE evidence.
Historical SECURELOAD observations are NOT authority for this run.
Immediately before T3C BASELINE:

1. require acad.exe count = 0;
2. Owner/CAD manager reads current SECURELOAD read-only;
3. record exact current value and source in the run evidence;
4. call that value:

SECURELOAD_BASELINE_CURRENT

5. invoke T3C BASELINE with:

-ExpectSecureLoad SECURELOAD_BASELINE_CURRENT
The capture must independently read the same value.
Mismatch:
STOP before AutoCAD launch.
Do NOT change SECURELOAD.
PaletteKeyAcceptance = ACCEPTED
Therefore a new palette-state key classified as REPORT by T3C may yield:
REPORTED_ACCEPTED
provided:

* it is exactly palette/modeless-window state;
* no FORBIDDEN rule also applies;
* no TRUSTEDPATHS, SECURELOAD, profile, application-load or other forbidden
drift occurs.

Do not broaden ACCEPTED to arbitrary RackCad registry persistence.
AUTHORIZED for this occurrence only.
Exact DLL SHA required:
E769219D2F5EDF6E0DE0867633C4A89B94EBBD7F2BD88CF20864CBCF75EF7512
The load must be:
one-time
non-persistent
Forbidden:

* adding the DLL folder to TRUSTEDPATHS;
* "trust this location permanently";
* changing SECURELOAD;
* registering autoload;
* installing a bundle;
* changing profile;
* changing ACL;
* adding persistent environment variables.

If AutoCAD requires any persistent change to proceed:
STOP-06.
Do not continue.
Use the already-approved T3C tool bytes.
Before running them:

* verify their exact approved SHA-256;
* run Test-ProfileDiff.ps1;
* require 26/26 PASS;
* require acad.exe count = 0.

No tool modification is authorized.
BASELINE MUST occur before AutoCAD is launched.
Required order:

1. clean Candidate tree;
2. exact Candidate identity;
3. acad.exe = 0;
4. T3C tool hashes verified;
5. T3C self-test 26/26;
6. read current SECURELOAD as described above;
7. run T3C BASELINE with:

ActiveProfile:
<<Unnamed Profile>>
AcadSha256:
2A75996FD2A5C5EE0376FD5BA7CCED227A098193FF495E8CEEDA3A96FE326422
TrustedPathsSha256:
a9ba0fd1c2f15dfc7ee4477e0ea62e710ac842d89d2b0e18102b2362553a2498
SecureLoad:
SECURELOAD_BASELINE_CURRENT
CandidateDll:
exact DLL above

8. require BASELINE success;
9. verify no forbidden pre-smoke mismatch.

Any mismatch:
STOP.
Do NOT launch AutoCAD.
Prepared drawing:
I64-SMOKE1-FIXTURES-D.dxf
Status before host execution:
PREPARED / HOST-UNCONFIRMED
Contains:
PR-13 SIN IDENTIDAD
PR-12 DIAGNOSTICO
Offline preparation evidence is accepted:

* envelopes produced from Candidate RackEmbedStore;
* PR-13 classifier result = NoIdentity;
* PR-12 classifier result = Diagnostic;
* ezdxf audit = 0 errors;
* independent read-back preserved envelope bytes.

The fixture's exact SHA-256 must be recorded before launch.
After successful BASELINE:

1. launch AutoCAD under this authorized occurrence;
2. perform the authorized one-time load of the exact Candidate DLL;
3. confirm loaded DLL identity where the procedure requires;
4. FIRST drawing-level check:
open I64-SMOKE1-FIXTURES-D.dxf.

If AutoCAD cannot open it cleanly:
STOP.
Do not rewrite or repair the DXF during the run.
Close without saving if possible.
Proceed to POST / DIFF according to the STOP procedure.
Smoke-1 cannot PASS.
PR-13:
must render:
"Sin identidad"
and:
"El objeto seleccionado no tiene identidad de rack."
PR-12:
must render:
"Diagnóstico"
and:
"No se pudo interpretar la definición del objeto seleccionado."
A generic non-RackCad object remains the separate "otros objetos" case.
Both PR-12 and PR-13 must be observed in host for S1-04 to PASS.
Execute the accepted T3D procedure exactly.
Use the accepted A-7 interpretation:
S1-06a..e =
NOT_APPLICABLE_BY_DESIGN
never PASS
SM-14a..e =
SUPPLEMENTARY only
S1-06f/g/h =
mandatory
Execute all other applicable S1 steps and SM steps exactly as documented.
Do not improvise additional AutoLISP, commands or recovery actions beyond
the written procedure.
On any STOP condition:

* stop the functional sequence;
* preserve evidence;
* close AutoCAD without saving if safely possible;
* obtain POST as required by T3C once acad.exe is fully closed;
* do not repair;
* do not repeat;
* return to Coordinator.

A FAIL is not permission to continue seeking a different result.
After the smoke sequence or an applicable STOP:

1. close all drawings without saving;
2. exit AutoCAD normally;
3. require acad.exe count = 0;
4. immediately take T3C POST;
5. use the same:
Candidate DLL
Active Profile expectation
TRUSTEDPATHS SHA
SECURELOAD_BASELINE_CURRENT
6. run T3C DIFF with:

PaletteKeyAcceptance = ACCEPTED
Allowed successful profile outcomes:
CLEAN
or
REPORTED_ACCEPTED
Any:
REPORT_REQUIRED_STOP
FORBIDDEN_DRIFT
INVALID
means Smoke-1 cannot PASS.
After I-64 POST and with acad.exe count = 0:
do NOT start another I-64 AutoCAD session.
Hand host back to I-52.
I-52 must recheck:

* exact acad.exe hash;
* active profile;
* exact 19-entry TRUSTEDPATHS chain/full hash;
* current SECURELOAD;
* registry no-drift;
* ApplicationPlugins no-drift;
* I-52 ACL no-drift;
* approved P2 bytes no-drift.

Only after the I-52 recheck may its Coordinator consider releasing:
P2 H10
Any drift:
I52_P2_H10 remains HELD.
The responsible session does NOT self-declare final Smoke-1 PASS.
It proposes:
PASS
NOT PASS
or
STOP
based on T3D.
Owner/operator signs the record.
Then return to Coordinator at:
COORDINATOR_SMOKE1_REVIEW_REQUIRED
Return:

* RunId;
* exact Candidate SHA;
* exact Candidate DLL SHA;
* Candidate local-suite evidence;
* Candidate CI;
* exact fixture SHA;
* PR-12 host result;
* PR-13 host result;
* operator record;
* S1-01..S1-12 results;
* SM-00..SM-41 results;
* instrumentation observations;
* DBMOD observations;
* focus observations;
* RackLog fault evidence;
* BASELINE CanonicalSha256;
* POST CanonicalSha256;
* profile DIFF disposition;
* any palette keys reported;
* acad.exe count after close;
* I-52 post-run recheck result;
* proposed Smoke-1 disposition;
* custody manifest.

Do NOT start F2 before Coordinator reviews Smoke-1.
```
