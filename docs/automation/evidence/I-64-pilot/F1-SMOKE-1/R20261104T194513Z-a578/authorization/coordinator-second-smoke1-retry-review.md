# I-64 — COORDINATOR SMOKE-1 REVIEW de la ocurrencia R20261104T194513Z-a578 (transcripción literal)

Recibida el 2026-11-05 (~05:00Z) por el chat de la sesión responsable, que esperaba en COORDINATOR_SMOKE1_REVIEW_REQUIRED (paquete
`disposition/return-package.md`, registro del operador firmado a las 04:47:56Z).

```text
I-64 — COORDINATOR SMOKE-1 REVIEW
Disposition:
STOP CONFIRMED
Primary reason:
HOST_ISOLATION_VIOLATION / MULTIPLE_AUTOCAD_INSTANCES
A Smoke-1 occurrence cannot be authoritative while two acad.exe processes
exist and host-level evidence cannot be attributed unambiguously.
T3D §1 requires:
Owner = AutoCAD operator
Observed:
implementation session operated AutoCAD.
Disposition:
PROCEDURAL_DEVIATION — PRESERVED
Do not rewrite history.
This deviation is NOT approved as precedent for another occurrence.
Next occurrence:
Owner operates AutoCAD.
Implementation session guides, captures and analyzes only.
If the implementation session operates AutoCAD:
STOP.
Preserve all observed results exactly as recorded.
Do not relabel passing SM steps as failures.
However, because host isolation was violated:
visual observations clearly tied to the operated window may be retained as:
OBSERVED / NON-AUTHORITATIVE FOR SMOKE PASS
Host-global evidence is not sufficient for Smoke closure, including:

* process-state evidence;
* global RackLog attribution;
* registry attribution;
* ApplicationPlugins / AssemblyMap attribution;
* POST host-state attribution;
* any loaded-module evidence not explicitly bound to the intended PID.

The next Smoke-1 occurrence must therefore execute the full accepted
procedure again.
No prior PASS observation exempts a step from repetition.
Observed:
AutoCAD rewrote the Applications key but values were unchanged.
Classification:
SEMANTIC_DRIFT = NO
WRITE_ACTIVITY = YES
ATTRIBUTION = UNKNOWN
Disposition:
REPORT / NON-BLOCKING FINDING
Do not clean or modify the key merely because its last-write timestamp
changed.
Because two AutoCAD instances existed, this occurrence's POST has:
LIMITED EVIDENTIARY VALUE
and cannot establish authoritative host cleanliness for Smoke-1 PASS.
A new RunId and new authorization are required.
Before BASELINE:
acad.exe count MUST equal 0.
After the Owner opens AutoCAD:
acad.exe count MUST equal exactly 1.
Record the authorized host instance:

* PID;
* executable path;
* acad.exe SHA-256;
* process start time.

Call it:
AUTHORIZED_AUTOCAD_PID
Every subsequent process-sensitive check must bind to that PID.
After NETLOAD verify, in AUTHORIZED_AUTOCAD_PID:

* exactly one RackCad.Plugin.dll loaded;
* exact Candidate build path;
* exact Candidate DLL SHA-256.

During the occurrence, at critical checkpoints:
if acad.exe count != 1
OR AUTHORIZED_AUTOCAD_PID no longer exists
OR another acad.exe PID appears
=> STOP immediately.
Before POST:
require the same authorized PID to be the sole AutoCAD process.
Then close it normally.
Before POST capture:
acad.exe count MUST equal 0.
Do not automatically reuse the current Candidate authorization.
The product may remain unchanged, but the next occurrence requires:

* Coordinator authorization;
* new RunId;
* fresh host recheck;
* new BASELINE;
* new POST;
* new operator record;
* one-time NETLOAD authorization.

If no product or Candidate commit changes, the same Candidate SHA may be
proposed again, but it must be explicitly re-authorized.
Obtain another read-only host recheck after the current AutoCAD activity is
fully closed.
The missing I-52/I-65 folders remain a separate issue and do not become an
I-64 product defect.
Prepare the close/custody package for this STOP occurrence and then prepare a
new-run proposal.
Return at:
COORDINATOR_SMOKE1_RETRY_AUTHORIZATION_REQUIRED
with:

* historical RunId and STOP custody;
* exact Candidate proposed for reuse or replacement;
* exact DLL SHA;
* fresh host recheck;
* acad.exe = 0;
* proposed new RunId;
* revised process-guard procedure;
* confirmation Owner will operate AutoCAD.

Do NOT merge/rebase main.
Do NOT start F2.
```
