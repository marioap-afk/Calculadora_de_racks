# I-64 — Autorización del reintento de Smoke-1, RunId R20261105T050250Z-785a (literal)

Recibida el 2026-11-05 (~17:10Z) por el chat de la sesión responsable, que esperaba en COORDINATOR_SMOKE1_RETRY_AUTHORIZATION_REQUIRED.
El paquete era `F1-SMOKE-1-RETRY-PREP/host-recheck-after-a578/return-package.md`, revisión 2, con los 8 puntos. La autorización asigna
el RunId que la sesión propuso a las 05:02:50Z. La disposición de operador previa está literal en
`F1-SMOKE-1-RETRY-PREP/host-recheck-after-a578/coordinator-operator-disposition.md`.

```text
I-64 — COORDINATOR SMOKE-1 RETRY AUTHORIZATION
RunId:
R20261105T050250Z-785a
Disposition:
AUTHORIZED
Authorized Candidate:
cc7503e3e4e56bff3a2fc9de7ca30a5322b57ea6
Authorized RackCad.Plugin.dll SHA-256:
3C261B8F156E69192415E0084D0BC8774B3080ACE9D6CB4B9BB0C1760549B6A0
Current branch HEAD:
b5ea02f5d193d80e4b921b82bbeb7f87a0c8be0d
HEAD is NOT the Candidate.
b5ea02f5 is a documentary custody descendant of cc7503e3.
Reuse of cc7503e3 is explicitly AUTHORIZED without recompilation.
Before the occurrence prove:

* src/ delta cc7503e3..HEAD = none;
* tests/ delta cc7503e3..HEAD = none;
* project-file delta relevant to product = none;
* DLL embedded/source Candidate identity = cc7503e3;
* DLL SHA-256 = exact authorized SHA.

Do NOT require HEAD == Candidate.
Do NOT rebuild or recompile before the occurrence.
ONE non-persistent NETLOAD is AUTHORIZED.
Use:
Load Once
Authorized DLL only:
3C261B8F156E69192415E0084D0BC8774B3080ACE9D6CB4B9BB0C1760549B6A0
Any different DLL or recompilation invalidates this authorization.
HOST_RELEASE = GRANTED FOR I-64 / 785a
I-52 ACK/RELEASE is not required.
I-52 must only preserve its commitment not to touch the host from the I-64
BASELINE through POST and its post-run read-only recheck.
The revised process guard is ACCEPTED and BINDING.
Before BASELINE:
acad.exe count = 0
RAM Elements count = 0
After AutoCAD is opened by the authorized implementation-session operator:
acad.exe count = exactly 1
Record:
AUTHORIZED_AUTOCAD_PID
executable path
acad.exe SHA-256
process start time
At every critical checkpoint:
acad.exe count must remain exactly 1
and the sole PID must equal AUTHORIZED_AUTOCAD_PID.
If:

* another acad.exe appears;
* AUTHORIZED_AUTOCAD_PID disappears unexpectedly;
* PID attribution becomes ambiguous;

STOP immediately.
After NETLOAD inspect AUTHORIZED_AUTOCAD_PID only.
Required:

* exactly one RackCad.Plugin.dll;
* exact Candidate build path;
* exact authorized DLL SHA-256.

For 785a:
AutoCAD operator = implementation session
This is an explicit pre-run Coordinator exception.
It is not an unplanned T3D deviation.
The implementation session is also authorized to execute:

* preflight;
* HOST-EXTRAS capture;
* BASELINE;
* read-only process/module inspection;
* POST;
* diff/comparison tooling.

It may not:

* alter Smoke acceptance rules;
* skip mandatory steps;
* make exploratory host changes;
* modify security/profile/registry state;
* self-declare Smoke-1 PASS.

At the end of the occurrence, present the completed operator record to the
Owner.
The Owner will sign only after reviewing it.
The signature confirms the record, not PASS.
The Owner AutoCAD session from approximately 16:00Z to 16:57Z is classified:
OWNER_AUTOCAD_ACTIVITY_PRE_BASELINE
The Applications-key write at 16:57:18Z is:
WRITE_ACTIVITY = YES
SEMANTIC_DRIFT = NO
SECURITY_DRIFT = NO
LOAD_DRIFT = NO
PRE_BASELINE = YES
It is NON-BLOCKING.
Do not clean or modify the key.
The fresh BASELINE absorbs the current state.
The latest session + I-52 recheck supersedes the morning recheck.
Accepted:

* acad.exe = 0;
* RAM Elements = 0;
* profile unchanged;
* acad.exe identity unchanged;
* TRUSTEDPATHS unchanged;
* SECURELOAD reference unchanged;
* no RackCad registration drift;
* AssemblyMap RackCad.Plugin absent;
* CoreUser unchanged.

The correction to I-52's NETLOAD-history attribution is accepted:
the cited write already existed before the prior BASELINE and must not be
attributed to a578.
Prepare the 785a kit and execute the complete accepted Smoke-1 procedure.
Use:

* new BASELINE;
* new POST;
* new HOST-EXTRAS BEFORE/AFTER;
* new operator record;
* PID-bound process guard;
* exact loaded-DLL inspection.

Critical regression checks remain:
SM-03 / SM-17 — C1
SM-13 — command-active presentation
SM-34 — RACKEDITAR command-active presentation
SM-26 / SM-27 — Start / Sin documento
SM-39 — no DocumentActivated NullReferenceException or transaction leak
After the run return at:
COORDINATOR_SMOKE1_REVIEW_REQUIRED
with:

* signed operator record;
* all SM results;
* AUTHORIZED_AUTOCAD_PID evidence;
* loaded DLL path and SHA;
* BASELINE / POST hashes;
* T3C diff;
* HOST-EXTRAS before/after;
* final acad.exe = 0;
* I-52 post-run recheck;
* proposed PASS / NOT PASS / STOP.

Do NOT merge/rebase main.
Do NOT start F2.
```
