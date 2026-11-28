Orden del Owner / Coordinator recibida en el chat el 2026-11-12, tras el cierre de F4-T2B2B (custodia 92ad237c). Texto literal:

I-64 — F4-T2C-IC04-READ-SCHEDULING
OWNER / COORDINATOR AUTHORIZATION
Current I-64 branch:
92ad237c99c1fb80a95334b58c98249d9d890a1f
Current main:
812baf879612320653094a8aaf46aca0bac81383
F4-T1:
CLOSED / PASS
F4-T2A:
CLOSED / PASS
F4-T2B1:
CLOSED / PASS WITH LIMITATIONS
F4-T2B2A:
CLOSED / PASS WITH LIMITATIONS
F4-T2B2B:
CLOSED / PASS WITH LIMITATIONS
Next:
F4-T2C-IC04-READ-SCHEDULING
F4-T2C starts with its own task-local:
attempts = 1 / 3
Do not carry attempts from T2B2B.
T2C exists to enforce IC-04:
AutoCAD event notification handlers and the Application.Idle callback must
NOT perform DWG reads merely because an event/idle callback fired.
They may only:

* enqueue/coalesce hints;
* update in-memory scheduling state;
* arrange a later eligible read.

The actual summary/read operation must occur through the normal authorized
application-context read path after the callback stack has unwound.
Forbidden:
event callback
-> direct DWG read
Forbidden:
Application.Idle callback
-> synchronous summary/DWG read in the same callback stack
Required:
event / idle callback
-> mark/schedule
-> return from callback
-> later eligible operation
-> validate session/document
-> quiescent
-> no RackCad modal
-> DocumentLock
-> bounded read
-> dispose all DBObjects/transactions
Keep T2C narrowly focused on:

* read scheduling;
* separation of notification stack from read stack;
* one-shot/coalesced scheduling;
* cancellation/invalid-session behavior;
* preventing duplicate reads;
* preserving panel-hidden behavior;
* preserving K=1 summary semantics.

Do NOT pull into T2C:

* editing UI;
* pending text;
* Actualizar;
* mutation host;
* DraftRegistry lifecycle redesign;
* T3 focus changes;
* T4 write semantics.

At minimum contract:

1. event handlers never read DWG;
2. Application.Idle callback never performs the actual summary read;
3. Idle may schedule/defer work and then detach itself as appropriate;
4. multiple hints coalesce into one later read;
5. hidden panel causes zero DWG summary reads;
6. destroyed/inactive session cannot execute a stale scheduled read;
7. document switch cannot cause the old session read to run against the new
document;
8. modal/command-active state delays the read;
9. failed read does not spin indefinitely on Idle;
10. once eligible, exactly one read is performed for the materialization /
refresh request;
11. no per-tab AutoCAD event subscription is introduced;
12. no authored write is possible on this path;
13. TopTransaction / DBObject lifetime returns clean after the operation.

The automated tests must distinguish call-stack timing, not merely count
reads.
Required adversarial proof:
A.
inside notification callback:
read count = 0
B.
inside Idle callback before return:
read count = 0
C.
after deferred/scheduled continuation:
read count = 1
D.
N notifications:
still one eventual read
E.
session destroyed before continuation:
zero reads
F.
panel hidden:
zero reads
G.
document changes:
no cross-document read
H.
command/modal remains active:
read stays deferred
I.
eligibility later becomes true:
one read
J.
read throws:
contained, no callback reentry loop
T2C should have at least a targeted AutoCAD host smoke if practical.
The key observation is:
trigger an event that invalidates the active summary and prove no DWG read
occurs during the event/Idle callback itself, while the summary later
refreshes correctly.
If direct instrumentation is required, keep it diagnostic-only and do not
ship permanent debug behavior.
If host instrumentation cannot prove stack timing reliably:
record that limitation for Smoke-2 rather than inventing evidence.
Before emitting the contract:

1. map the current host/read scheduling code;
2. identify the exact current path that violates or risks IC-04;
3. prepare the gate-contract package;
4. run independent adversarial review of the package;
5. incorporate blocking/important findings;
6. only then emit the contract.

Apply the now-established F4 rule:
if the first Controller planning output fails literal collection fidelity,
one reinforced replan is allowed without consuming attempts.
Record both outputs.
Do not generalize beyond one replan per task.
After Worker delivery require:

* RED / GREEN as contracted;
* exact-SHA CI;
* direct review;
* independent adversarial review;
* Controller verification;
* nc1 / nc2 / nc3;
* custody;
* close only with evidence.

Controller 14/14 remains necessary but not sufficient.
Keep existing protocol debts visible:

* exact-whitelist nc2 may produce misleading ordering;
* Controller FreeText detector has produced false PASS;
* Handoff/Identity ordering can vary.

Do not automatically reuse a substitute unless its applicability conditions
match the new task exactly.
If T2C closes:
F4-T2 is considered complete at the subtask level.
Next prepare:
F4-T3 — EDITING UI / PENDING TEXT
Carry into T3:

* T2B2B minors M-1..M-5 and D-1;
* focus/container limitations already known from F3;
* RACKPANELDESCARTAR UI exposure;
* exact pending-text behavior;
* blockable vs non-blockable transitions.

Do not start T4 before T3 closes.
F4-T2C:
AUTHORIZED
attempts 1 / 3
F4-T3:
WAITING
F4-T4:
WAITING
Smoke-2:
PENDING / REAL AUTOCAD REQUIRED
F5-F7:
PENDING
