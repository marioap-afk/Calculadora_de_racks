# I-64 — Autorización del segundo reintento de Smoke-1, RunId R20261104T194513Z-a578 (literal)

Recibida el 2026-11-04 (~22:00Z) por el chat de la sesión responsable, que esperaba en COORDINATOR_SMOKE1_RETRY_AUTHORIZATION_REQUIRED
(paquete `F1-SMOKE-1-RETRY-PREP/host-recheck-c2/return-package.md`). El RunId es el que la sesión propuso a las 19:45:13Z; queda
asignado por esta autorización. Ninguna autorización anterior de carga del DLL se transfiere.

```text
I-64 — COORDINATOR SECOND SMOKE-1 RETRY AUTHORIZATION
Disposition:
AUTHORIZED WITH PRECONDITION
Candidate:
cc7503e3e4e56bff3a2fc9de7ca30a5322b57ea6
Product:
0e0d536b10350cc85e4a932a0d576b22fae22fa9
Exact RackCad.Plugin.dll SHA-256:
3C261B8F156E69192415E0084D0BC8774B3080ACE9D6CB4B9BB0C1760549B6A0
RunId:
R20261104T194513Z-a578
PaletteKeyAcceptance:
ACCEPTED
One-time non-persistent NETLOAD:
AUTHORIZED for the exact DLL SHA above only.
The Owner currently has AutoCAD open.
Before BASELINE:

* Owner closes AutoCAD normally;
* require acad.exe = 0;
* require RAM Elements = 0.

If either process remains:
DO NOT TAKE BASELINE.
This authorization becomes executable only after that precondition passes.
For I-64 only:
HOST_RELEASE = GRANTED BY OWNER / COORDINATOR
I-52 is not required to issue ACK/RELEASE.
I-52's role during this occurrence is:

* do not touch the host from I-64 BASELINE through I-64 POST;
* perform its read-only post-run recheck afterward.

This disposition does NOT release or authorize I-52 P2 H10.
I-52 H10 remains governed independently by I-52.
Missing:
<carpeta-de-I-52>
<carpeta-local-de-I-65>
Classification:
UNKNOWN_EXTERNAL_HOST_CHANGE
Attribution:
NONE
Do not infer who deleted them.
Disposition for I-64:
NON-BLOCKING
Reason:
the I-64 Candidate, Smoke kit, T3C tools and T3D/T3C procedures have no
dependency on either path.
Do not recreate, delete or modify either path from I-64.
Their effect on I-52 H10 is outside this Smoke-1 occurrence and remains for
I-52's Coordinator / Owner to resolve.
For this occurrence:
Owner = AutoCAD operator
The procedural deviation accepted for 0a77 does NOT automatically carry
forward.
The implementation session may:

* prepare scripts;
* inspect read-only host/process state;
* inspect the loaded DLL path/hash;
* capture evidence;
* guide the Owner;
* evaluate each step.

It must NOT operate AutoCAD instead of the Owner.
If the session operates AutoCAD:
STOP and return for Coordinator disposition.
Immediately before BASELINE, with acad.exe = 0:

* Owner/CAD manager reads SECURELOAD read-only;
* record it as SECURELOAD_BASELINE_CURRENT;
* expected current reference is 1, but historical/reference value is not
authority;
* T3C BASELINE independently rereads SECURELOAD;
* both values must match.

Mismatch:
STOP before AutoCAD.
No modification to SECURELOAD, TRUSTEDPATHS, profile, registry, ACL or
ApplicationPlugins is authorized.
Before BASELINE capture read-only:

* AssemblyMap;
* relevant CoreUser/AppDataLow RackCad state;
* NETLOAD history as informational state.

After AutoCAD is fully closed capture them again.
Rules from 0a77 remain:

* NETLOAD MRU/history change alone = informative;
* new/changed RackCad AssemblyMap / Applications / Loaded registration =
STOP / Coordinator review;
* unexplained RackCad load-state change = STOP.

Immediately after NETLOAD and before continuing the Smoke:
inspect AutoCAD read-only.
Required:

* exactly one RackCad.Plugin.dll loaded;
* loaded from the exact Candidate build location;
* SHA-256 exactly:

3C261B8F156E69192415E0084D0BC8774B3080ACE9D6CB4B9BB0C1760549B6A0
Any mismatch, ambiguity or second RackCad.Plugin.dll:
STOP.
Run the accepted Smoke-1 procedure.
Pay particular attention to:
SM-03:
C1 initial visibility/usability remains PASS.
SM-17:
dock left/right/floating behavior remains PASS.
SM-13:
with LINE active:

* Leer el dibujo disabled;
* Escribir en el dibujo disabled;
* "Hay un comando en curso.";
* no drain/read instrumentation during the active command.

SM-34:
while RACKEDITAR is active:

* same command-active presentation;
* editor operates normally;
* no panel interference.

SM-26:
after closing the last drawing:

* panel reaches "Sin documento";
* no DocumentActivated NullReferenceException.

SM-27:
while Start is active:

* panel remains "Sin documento";
* no stale previous session;
* reopening a drawing restores its correct session.

SM-39:

* no AutoCadEventBridge.DocumentActivated NullReferenceException;
* no transaction-leak diagnostic;
* no unexpected ERROR attributable to C2.

Historical results remain historical and must not be relabeled.
After the Smoke:

* close every drawing without saving unless T3D says otherwise;
* close AutoCAD normally;
* require acad.exe = 0;
* take HOST-EXTRAS after;
* take T3C POST;
* run DIFF with PaletteKeyAcceptance=ACCEPTED;
* obtain I-52 read-only post-run recheck.

Return at:
COORDINATOR_SMOKE1_REVIEW_REQUIRED
with:

* signed operator record;
* complete SM results;
* exact loaded DLL path + SHA;
* SM-03 / SM-17 observations;
* SM-13 result;
* SM-34 result;
* SM-26 / SM-27 Start-state result;
* SM-39 RackLog result;
* BASELINE / POST hashes;
* T3C DIFF;
* HOST-EXTRAS before/after;
* final acad.exe count;
* I-52 post-run recheck;
* proposed PASS / NOT PASS / STOP.

Do NOT merge/rebase main.
Do NOT start F2.
```
