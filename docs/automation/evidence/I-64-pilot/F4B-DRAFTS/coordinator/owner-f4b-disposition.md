# I-64 F4B: disposición del Owner y del Coordinator sobre el paquete v1 de «Push Back completo» (2026-11-17)

El Owner la pegó en el chat como respuesta al paquete v1 (`I-64-push-back-completo-package-v1.md`). Se transcribe literal, sin cambios, entre las dos rayas.

---

I-64 — F4B PUSH BACK COMPLETO
OWNER / COORDINATOR DISPOSITION
Current I-64 branch:
5ccaa770577a700de6c0bdb58d64af106c771bb9
Current main:
812baf879612320653094a8aaf46aca0bac81383
Authorize introduction of:
F4B — PUSH BACK COMPLETO
between:
F4
and
F5-A
F4 remains CLOSED / PASS WITH LIMITATIONS.
F4B extends the proven Workspace architecture; it does not reopen F4.
F5-A waits until F4B closes.
ACCEPT RECOMMENDATION.
The Workspace exposes:
"Editar todo"
which opens the existing Push Back editor as a modal/full-size editing
surface loaded from the Workspace draft.
In Workspace mode:

* the editor reads from the draft;
* its internal Apply/Actualizar action modifies the draft only;
* it performs ZERO authored DWG writes;
* the Workspace remains the authority for the final Actualizar;
* T4 PREPARE / MUTATE / POST still performs the authored write;
* the drawing cannot be edited while the modal editor is open.

Do not build a second full Push Back editor.
Do not try to fit the current ~1120 px editor inside the ~320 px panel.
RACKEDITAR behavior must remain observably compatible.
If the same window is used in two modes:
CLASSIC MODE:
existing RACKEDITAR semantics
WORKSPACE MODE:
draft-in / draft-out semantics
The mode must be explicit.
Do not let a Workspace invocation accidentally execute the classic
drawing-write path.
If changing the button label in Workspace mode improves clarity, that is
allowed only if the package declares it and RACKEDITAR mode remains
unchanged.
ACCEPT RECOMMENDATION.
A/B Push Back racks are IN SCOPE for F4B.
The current pilot rejection of A/B racks must be removed only after the full
draft state can represent:

* side A;
* side B;
* modules/fronts;
* parked queue state;
* all authored state required for correct redraw.

Do not admit A/B racks before that state is lossless.
Ratify the measured behavior:
checking "Rack de dos sentidos (lado B)" means side B is permitted to exist.
It does NOT by itself create persisted side-B content.
If all side-B fronts remain inactive/empty:
side B is not persisted
and reopening may show the checkbox unchecked.
This is:
DESIGN / CHARACTERIZED BEHAVIOR
not a persistence defect.
However:
accept D-PB-08 conceptually:
the UI should make this consequence understandable.
Do not silently imply that checking the box alone creates a persisted side B.
ACCEPT RECOMMENDATION WITH THE PROPOSED BOUNDARY.
F4B may delete obsolete sibling views when the new full design no longer
requires them.
Creating NEW views is NOT yet authorized by this decision.
PB-T0 must characterize:

* when the classic path creates a new view;
* view naming;
* placement;
* orientation;
* layer;
* scale;
* annotations;
* relationship to existing sibling views;
* Undo behavior.

After PB-T0:
if creation semantics are sufficiently characterized and fit the T4
architecture, include creation in the later write task.
If not:
return to Owner before adding view creation.
MODIFY THE RECOMMENDATION SLIGHTLY.
Do NOT define it as:
"the entire stale serialized draft wins."
Instead:
"the user's FULL EDITABLE PUSH BACK DESIGN wins for every field owned by the
full Push Back editor, after explicit conflict presentation."
At reconciliation time:

1. authoritative current rack is reread;
2. external differences are shown/classified;
3. user explicitly chooses "Conservar mi borrador";
4. all fields OWNED by the full Push Back editor come from the user's draft;
5. envelope/infrastructure fields NOT owned by that editor come from the
current authoritative rack;
6. unknown/future ExtensionData must not be silently destroyed;
7. identity/schema/session metadata must not be rolled back from a stale
draft.

Therefore:
FULL EDITABLE DESIGN wins
but:
STALE ENVELOPE / NON-EDITOR METADATA does NOT.
This preserves the safety rule established in F4 while allowing a true full
Push Back editor.
"Conservar mi borrador" must be an informed destructive choice.
Before accepting it, show enough information to distinguish at least:

* fields changed externally;
* fields changed by the user;
* overlapping changes;
* structural changes such as modules/fronts/side B;
* view-related implications where applicable.

Do not require a generic JSON diff as the user-facing UX.
ACCEPT THE PRODUCT CORRECTIONS, BUT NOT AS AN INCIDENTAL REFACTOR.
The two known losses:

* saved dimension style;
* rounding behavior/state;

may be corrected in F4B because the shared full editor state must otherwise
reproduce data loss in both Workspace and RACKEDITAR.
However they are explicit changes to classic behavior.
Therefore:

1. characterize the CURRENT lossy behavior first;
2. add RED tests proving each loss;
3. implement the corrected shared state/load behavior;
4. prove both:
   * Workspace;
   * RACKEDITAR
now preserve the intended value;
5. update the compatibility characterization explicitly;
6. record this as an intentional classic-editor behavior correction, not as
"no observable change".

Do not hide these corrections inside PB-T1 state extraction.
If either correction turns out to alter drawing geometry or another
downstream contract materially:
return to Owner.
ACCEPT.
PB-T0 is characterization / measurement only.
It must establish the missing facts before structural migration.
At minimum characterize:

* editor state ownership;
* module state;
* parked queue;
* A/B behavior;
* add/remove fronts;
* add/remove modules;
* rename;
* view deletion;
* view creation;
* classic update/close distinction;
* subdialogs;
* catalog dependencies;
* dimension-style loss;
* rounding loss;
* all drawing writes initiated after the classic editor returns.

No product cleanup in PB-T0.
ACCEPT IN PRINCIPLE.
PB-T1 extracts a lossless Push Back editable state outside the WPF window.
Hard requirements:

* module state preserved;
* parked queue preserved;
* A/B preserved;
* no aliasing;
* no WPF control becomes source of truth;
* state round-trip lossless for supported classic racks;
* classic RACKEDITAR still works through the extracted state.

Do not rewrite the entire editor merely to obtain an MVVM architecture.
The goal is state separation, not aesthetic refactoring.
ACCEPT IN PRINCIPLE.
Separate:
"apply edits to current editing state"
from:
"close editor"
and from:
"write authored DWG state".
Workspace mode must return edited state to DraftSession.
Classic RACKEDITAR may continue its compatible existing outer workflow.
Avoid two independent implementations of field validation.
ACCEPT IN PRINCIPLE.
The Workspace authored writer may expand to full Push Back design.
It must eventually support, as characterized/authorized:

* field changes;
* modules/fronts;
* side B;
* rename;
* deleting obsolete views;
* blocking authored write on diagnostics/errors;
* view creation only if PB-T0 subsequently authorizes it.

Still require:
PREPARE
-> MUTATE
-> POST
and one logical Update.
ACCEPT.
Add "Editar todo" to the Workspace only after PB-T1/T2 establish the
draft-based editor seam.
Do not make PB-T4 invent state translation logic.
It should consume the already proven full Push Back draft/state API.
ACCEPT WITH ONE DISTINCTION.
Smoke-PB may use intentionally fabricated A/B test racks because no
representative real A/B rack currently exists.
But label them explicitly:
CONSTRUCTED REPRESENTATIVE TEST CASES
not:
real production examples.
Smoke-PB should still use real existing Push Back racks for the behaviors
where real examples exist.
Each separately contracted PB task starts at:
1 / 3
No shared 18-attempt pool.
Attempt 3 always returns to Owner unless separately authorized later.
Do not edit T1B characterization tests merely because F4B changes a
behavior.
For behavior intentionally preserved:
T1B must remain green.
For D-PB-06 or another explicitly authorized behavior change:
create a declared superseding characterization/change record.
Do not silently weaken the old guard.
RACKEDITAR remains a compatibility surface throughout F4B.
Observable behavior must remain unchanged except for explicitly authorized
corrections such as D-PB-06.
No forced migration to Workspace.
No removal.
Create package v2 with these dispositions.
Then:

1. include the exact text of ALL ten D-PB decisions;
2. run independent adversarial review;
3. resolve every blocker and important finding;
4. return to Owner only if that review introduces a new MATERIAL product
decision.

Otherwise:
PB-T0 may be emitted and executed.
Conditional authorization:
If package v2's independent review ends with:
0 blockers
and
0 important unresolved findings
and no new material Owner decision is required:
you may emit PB-T0 without returning to Owner.
PB-T0 begins:
attempts = 1 / 3
F5-A remains authorized but WAITING.
Order:
F4B
-> F5-A
Reason:
the complete Push Back editor will establish the reusable full-editor
migration pattern before Selectivo/Dinámico/Cantilever are connected.
F4:
CLOSED / PASS WITH LIMITATIONS
F4B:
AUTHORIZED
PB-T0:
CONDITIONALLY AUTHORIZED AFTER v2 REVIEW
PB-T1:
WAITING
PB-T2:
WAITING
PB-T3:
WAITING
PB-T4:
WAITING
PB-T5:
WAITING
F5-A:
AUTHORIZED / WAITING FOR F4B
F6:
PENDING
F7:
PENDING

---
