# I-64 — Disposición del Coordinator: P-01 CLEARED y autorización de aa47 (literal)

Recibida el 2026-11-08 por el chat de la sesión responsable, que esperaba en COORDINATOR_HOST_SANITY_AUTHORIZATION_REQUIRED con
`disposition/operator-exception-request.md` (SHA-256 97E61D7C…0FEC) y `P01-CONFIG/C20261108T065112Z/p01-cleared-report.md`
(SHA-256 27679ABD…8E24). La pegó el Owner.

```text
I-64 — COORDINATOR DISPOSITION
RunId:
R20261108T065253Z-aa47
Disposition:
P-01 CLEARED
+
POST-I65 HOST SANITY CHECK AUTHORIZED
Owner acceptance reported:
"Acepto 377D0C3C (vía B)."
Accepted governed configuration:
config.toml SHA-256:
377D0C3C2DA32B0F763501628CF4A4F054FF33C64C40C2D50399801539EAD1E8
Ordered inventory:
104 names
The complete ordered inventory captured for this exact SHA is accepted as
the governed inventory for the new configuration.
Disposition:
P-01 = CLEARED BY OWNER ACCEPTANCE / VIA B
This new governed state is distinct from historical snapshots A, B and C.
Assign the label:
SNAPSHOT D
for I-64 protocol records.
Snapshot D identity:
SHA-256:
377D0C3C2DA32B0F763501628CF4A4F054FF33C64C40C2D50399801539EAD1E8
Ordered-name count:
104
Do not infer acceptance of any future config.toml state.
Before any future model invocation:

* recompute config.toml SHA-256;
* require exact match to an accepted governed snapshot;
* if it differs from A/B/C/D:
P-01 becomes active again.

Do not modify config.toml from I-64.
The protocol-level block caused by P-01 is now cleared.
This does NOT itself authorize unnecessary model invocations.
Any future invocation remains subject to the existing task/protocol budget and
authorization rules.
Run:
R20261108T065253Z-aa47
is AUTHORIZED.
Purpose:
validate real-host coexistence of the synchronized I-64 + I-65 Plugin.
This is:
NOT a Smoke-1 repetition
NOT a new F1 gate
NOT an I-65 functional validation
Evaluated product SHA:
3f5c7014351d1dc59de4eee86a7d1319fbeccc07
Current documentary HEAD:
53683249c229329e3f576fc8f9b9919b1755c63f
Authorized Debug RackCad.Plugin.dll SHA-256:
0C0F2FDE...6C0B
Before execution, record the FULL SHA-256 and require exact match.
Do not rebuild or recompile before aa47.
Authorized AutoCAD operator:
implementation session
Scope:
aa47 only
This is an explicit pre-run exception.
The implementation session may:

* execute preflight;
* take BASELINE / host-state captures required by the sanity procedure;
* open AutoCAD;
* perform Load Once;
* execute the approved non-destructive command checks;
* inspect process/module state;
* collect RackLog evidence;
* close AutoCAD normally;
* take POST / host-state comparison.

It may not:

* extend the procedure;
* execute exploratory commands;
* publish a RAM package;
* alter host security/profile/registry state;
* self-declare PASS.

HOST_RELEASE = GRANTED FOR I-64 / aa47
Before starting:
acad.exe = 0
RAM Elements = 0
After AutoCAD opens:
exactly one acad.exe instance
Record:
AUTHORIZED_AUTOCAD_PID
acad.exe path
acad.exe SHA-256
process start UTC
Any second AutoCAD instance:
STOP.
A fresh I-52 recheck is REQUIRED before the host window if I-52 is reachable.
Required scope:

* active profile;
* acad.exe identity;
* TRUSTEDPATHS;
* SECURELOAD reference;
* RackCad registration state;
* AssemblyMap;
* ApplicationPlugins;
* relevant ACL/no-drift state.

I-52 ACK/RELEASE is NOT required.
Its role is read-only host observation and non-interference during aa47.
If I-52 is temporarily unreachable but its latest fresh read-only recheck is
still temporally valid and no host activity occurred afterward, return to the
Coordinator before opening AutoCAD rather than assuming equivalence.
The proposed path is ACCEPTED.
Use Drawing B with the Selective rack context.
Expected path:
RACKRAMPAQUETE
→ selected rack is type selective
→ Unsupported
→ return before station/load/folder/package publication
Required observation:

* command is registered;
* command is callable;
* returns the documented Unsupported result for selective;
* no package folder is created;
* no RAM package is published;
* no RAM Elements process is launched;
* no drawing write occurs.

This validates command coexistence only.
Do NOT test a supported Cantilever export path in aa47.
Required:

* RACKPANEL is registered;
* command opens the Workspace;
* palette is visible and usable;
* normal active-document state is reached;
* no unexpected RackLog ERROR attributable to coexistence with I-65.

No full C1/C2 Smoke sequence is required.
ONE non-persistent NETLOAD:
AUTHORIZED
Use:
Load Once
Immediately afterward inspect AUTHORIZED_AUTOCAD_PID.
Require:

* exactly one RackCad.Plugin.dll;
* exact synchronized build path;
* full SHA-256 matches the authorized synchronized DLL.

Any mismatch:
STOP.
Do not use Ctrl+V for AutoCAD command-line input.
Use direct keyboard entry.
Review required captures immediately after taking them.
No unrelated/private window may obscure the evidence region.
HS-00 through HS-13 as prepared may be used provided they remain within the
scope above.
No additional AutoCAD behavior is authorized without Coordinator disposition.
After checks:

* close drawings without saving;
* close AutoCAD normally;
* require acad.exe = 0;
* require RAM Elements = 0;
* inspect RackLog;
* compare relevant host state;
* verify no unexpected RackCad registration/persistence/security drift.

Return at:
COORDINATOR_POST_I65_HOST_SANITY_REVIEW_REQUIRED
with:

* exact synchronized DLL full SHA-256;
* AUTHORIZED_AUTOCAD_PID evidence;
* Load Once evidence;
* RACKPANEL result;
* RACKRAMPAQUETE Unsupported result;
* proof no RAM package/folder was published;
* RackLog result;
* final host state;
* host diff/recheck;
* proposed PASS / NOT PASS / STOP.

Do not start F2 until Coordinator review.
Do not merge/rebase main again during aa47.
```
