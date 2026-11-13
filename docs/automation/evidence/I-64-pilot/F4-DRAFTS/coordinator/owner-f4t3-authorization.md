Orden del Owner / Coordinator recibida en el chat el 2026-11-12, tras el cierre de F4-T2C (custodia 94b4d468). Texto literal:

I-64 — F4-T2 CLOSE RATIFICATION / F4-T3 AUTHORIZATION
Current I-64 branch:
94b4d468e6a6df8551029ffd2d3963497597052c
Current main:
812baf879612320653094a8aaf46aca0bac81383
F4-T2 is:
COMPLETE AT SUBTASK LEVEL
Closed components:
F4-T2A:
CLOSED / PASS
F4-T2B1:
CLOSED / PASS WITH LIMITATIONS
F4-T2B2A:
CLOSED / PASS WITH LIMITATIONS
F4-T2B2B:
CLOSED / PASS WITH LIMITATIONS
F4-T2C:
CLOSED / PASS WITH LIMITATIONS
No further T2 subtask is required merely to start T3.
R-T2C-01:
A summary already displayed does not automatically refresh after MOVE or an
external property change; it refreshes on:

* tab change;
* document change;
* explicit "Volver a leer".

Decision:
DO NOT put this into F4-T3.
Reason:
F4-T3 owns editing UI / pending-text semantics.
Automatic summary invalidation/refresh is synchronization/browser behavior
and would expand T3 into another read-scheduling/browser task.
Record:
R-T2C-01 = DEFERRED
Preferred destination:
F6 synchronization/inventory behavior
or, if F6 architecture shows it belongs elsewhere:
F7 hardening / pre-Owner-Validation correction.
It must NOT disappear from the initiative.
Before final Owner Validation, prove one of:
A. summary automatically refreshes after a relevant external rack change;
or
B. product intentionally shows stale/possibly-stale state with an explicit
refresh affordance and the Owner accepts that UX.
Do not silently present stale information as current.
Authorize:
F4-T3 — EDITING UI / PENDING TEXT
attempts:
1 / 3
Start now.
No additional Owner authorization is required for ordinary package
preparation, review, contract issuance, planning and Worker execution.
Connect the bounded Push Back pilot editing surface to the DraftSession
architecture already proven by T1/T2.
T3 owns:

* editing UI;
* field text;
* pending text;
* validation presentation;
* dirty indication;
* blockable/non-blockable UI transitions;
* tab rematerialization;
* explicit discard UX;
* modal RackCad UI state where applicable.

T3 does NOT own authored mutation.
No Actualizar write path yet.
Use the already contracted bounded pilot:
AnnotationScale
Do not broaden the Push Back pilot field set in T3 without a new material
reason.
The objective is to prove editing semantics, not migrate the whole Push Back
editor.
T3 must distinguish:
CurrentDraftState
from:
pending text that has not yet been committed into CurrentDraftState.
Pending text itself counts as dirty/protected work.
Required:
pending text must survive every non-blockable boundary specified by the
Freeze.
For user-initiated blockable transitions such as:

* switching tab;
* switching rack through a panel action;
* future Actualizar entry point;

if pending text is invalid:
abort the transition
and leave the user's text intact.
Do NOT:

* auto-repair;
* silently discard;
* commit an invalid value.

For boundaries such as:

* document switch;
* panel hide;
* K=1 surface dematerialization;
* document/session lifecycle transitions;

invalid pending text must be captured and retained exactly.
Do NOT validate it into a different value.
Do NOT silently commit it.
When the surface returns:
restore the pending text to the field.
The first editing gesture must fix the preview BEFORE it can become a dirty
editing session.
This includes the already frozen gestures where applicable:

* first field interaction;
* Edit;
* double-click;
* Pin/Fijar semantics.

A dirty/pending-text tab must never remain replaceable preview state.
The UI must visibly represent dirty/pending state.
The visible dirty marker must account for:

* CurrentDraftState != InitialDraftState;
* pending text.

Do not calculate dirty by comparing raw authored JSON.
Expose the already accepted explicit discard semantics.
RACKPANELDESCARTAR may be surfaced where the contract requires it.
Explicit discard:

* discards only the intended in-memory draft/pending state;
* performs zero authored write;
* must be session/rack specific;
* must not silently select an ambiguous draft.

The UI may also expose an equivalent explicit user action.
Known clean tab:
close without authored write.
Dirty or pending-text tab:
offer exactly the contractually accepted user choice:
Discard
or
Cancel
Closing never performs Actualizar.
Carry forward the T2 obligation:
subdialogs opened from the panel must mark RackCad modal state BEFORE the
dialog becomes active.
While modal RackCad is active:

* normal read/drain rules remain deferred;
* no unexpected AutoCAD interaction may occur.

Clear modal state reliably on every exit path.
Carry explicitly into the T3 package:
M-1..M-5
D-1
from the T2B2B close.
Especially:
the guard that currently requires exact:
_bridge.SetLifecycleAdvisor(_host);
must be deliberately changed if T3 needs to wrap/decorate the advisor.
Do not discover this later as an out-of-scope guard failure.
Any such change must be declared in the T3 contract.
Carry the 4 T2C minor findings where they belong to UI/guard behavior.
In particular:
add/adjust source guards so a future:
OnIdleTick
-> intermediate method
-> actual drain/read
cannot evade the IC-04 prohibition merely by indirection.
Do not reintroduce DWG reads into callback stacks.
T3 may address, when directly touched by its UI:

* keyboard focus loss on tab activation/fixing;
* transient presentation text issues.

But do not turn T3 into broad F7 polish.
At minimum cover:

1. valid text -> internal draft update;
2. invalid pending text remains pending;
3. invalid pending text marks work protected/dirty;
4. blockable tab switch with invalid text -> aborted;
5. non-blockable dematerialization with invalid text -> succeeds while
retaining text;
6. rematerialization restores exact pending text;
7. document switch preserves pending state in original session;
8. switching back restores it;
9. first edit gesture fixes preview;
10. dirty tab cannot be replaced as preview;
11. explicit discard removes pending/draft state without DWG write;
12. dirty close -> Discard/Cancel;
13. Cancel keeps state;
14. Discard removes state;
15. switching tabs does not write selection/view/DWG;
16. K=1 still holds while editing;
17. session isolation;
18. modal RackCad state set/cleared correctly.

A critical T3 oracle:
editing in T3 must produce ZERO authored DWG writes.
Actualizar remains T4.
If any T3 path writes authored state:
STOP / product defect.
Before emitting T3:

1. map current UI and pending-text mechanisms;
2. map the exact legacy field validation behavior relevant to AnnotationScale;
3. map the K=1 materialization lifecycle;
4. map carried guards/minors;
5. write the T3 gate-contract package;
6. independent adversarial review;
7. correct blocking/important findings;
8. emit contract only when package is acceptable.

Use the established F4 planning-copy rule:
first fidelity failure:
one reinforced replan allowed
without consuming product attempts.
If the reinforced replan also fails:
STOP to Coordinator.
After Worker delivery require:

* RED / GREEN;
* exact-SHA CI;
* direct code review;
* independent adversarial review;
* Controller verification;
* nc1/nc2/nc3;
* custody.

Do not close solely on Controller 14/14.
T3 itself may use UI tests and harnesses for most semantics.
However, if implementation changes focus/modal behavior in AutoCAD-specific
host code, add targeted host evidence where practical.
The complete end-to-end editing workflow still belongs to Smoke-2.
If F4-T3 closes:
prepare F4-T4.
T4 owns:

* stale/conflict workflow;
* explicit Actualizar;
* authored mutation;
* PREPARE / MUTATE / POST;
* write authority;
* own-mutation scope;
* reread;
* one logical Undo step.

Do not start T4 before T3 closes.
The Handoff/Identity nc1 ambiguity has now repeated.
For T3, if the exact same signature reproduces:

* false CurrentSha correctly detected;
* Identity = fail;
* Handoff also fails first only because the handoff itself contains the false
SHA;
* outcome BLOCKED / STOP;
* all other intended checks match;

you may reuse the established task-local deterministic Identity-signal
substitute pattern WITHOUT returning to the Owner again.
Record each use and preserve the ordinary control output.
This is an I-64/F4 execution rule only.
Do not change AUTOMATION_PLAN globally.
F4-T2:
COMPLETE
F4-T3:
AUTHORIZED
attempts 1 / 3
F4-T4:
WAITING
Smoke-2:
PENDING
F5-F7:
PENDING
