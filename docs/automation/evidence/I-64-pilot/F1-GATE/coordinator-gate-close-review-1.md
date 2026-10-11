# I-64 — Revisión del Coordinator del cierre del gate F1 (1): CHANGES REQUIRED

Transcripción literal recibida por el chat de la sesión responsable el 2026-10-11, tras el commit b35548fd
(CI 38169928014 4/4) y la parada en COORDINATOR_F1_GATE_CLOSE_REQUIRED.

```text
I-64 — COORDINATOR F1 GATE CLOSE REVIEW
TARGET SHA:
b35548fd516af690a48a69afda876bc95a645c7a
MainSha:
bb0d5522e8411f66a51fdfb3f1f0d0514b737453
Exact CI:
38169928014
SUCCESS
4/4
F1 — Workspace Foundation
Disposition:
CHANGES REQUIRED
Do NOT declare F1 PASS yet.
Product implementation itself is accepted.
T1, T2, T3A, T3B, T3C and T3D remain closed/accepted as previously
recorded.
The only material blocker to F1 PASS is D-T3D-01 / INV-24.
BLOCKING.
Freeze INV-24 states:
"Con el foco en un campo, ninguna tecla ni atajo llega a AutoCAD..."
and S1-06a..e specifically require:

* typing text with spaces in a panel field;
* Ctrl+Z undoing text;
* Ctrl+C / Ctrl+V operating on text;
* Delete not deleting drawing objects while focus is in a field;
* Esc / Enter not cancelling or repeating AutoCAD commands from a field.

The F1 Workspace contains no editable text field.
Therefore:
SM-14a..e on the instrumentation checkbox are useful SUPPLEMENTARY
focus-isolation evidence.
They are NOT a literal execution of S1-06a..e.
Do not mark S1-06a..e PASS.
Do not silently redefine "field".
Prepare an I-64 amendment proposal:
I-64-A-7
Subject:
F1 INV-24 / S1-06a..e applicability before editable Workspace fields exist.
Recommended disposition:
For F1 / Smoke-1:
S1-06a..e =
NOT_APPLICABLE_BY_DESIGN
because F1 intentionally exposes no editable text control.
SM-14a..e =
SUPPLEMENTARY_EVIDENCE
for keyboard/focus isolation only.
Keep in Smoke-1:
S1-06f
S1-06g
S1-06h
Reassign the literal editable-field cases S1-06a..e to the first frozen
milestone whose Workspace contains editable text, expected F4 / Smoke-2.
The amendment must identify the exact future scenario IDs or define them
unambiguously.
It must NOT weaken the eventual requirement.
Because the Freeze itself says reassignment requires an A-n, do not treat
this Coordinator message alone as the completed amendment.
Apply the normal amendment authority/review process appropriate to a
material lifecycle verification reassignment.
Stop for Coordinator/Architect disposition if that process requires it.
ACCEPTED.
No new fault-injection hook is required for F1.
Reason:
INV-25 already has automated UI/source evidence that exceptions crossing
the hosted panel boundary are contained and produce Error state.
Smoke-1 supplements that by observing:

* no unhandled .NET/AutoCAD exception dialog;
* no unexplained Error/Degraded state;
* classic commands remain usable if a contained panel error appears.

Do not modify product merely to force a failure.
Smoke-1 PASS requires the S1-04 diagnostic case.
Prepare or identify BEFORE Smoke-1:
one fixture drawing containing a block whose RackCad definition is
intentionally unreadable or represents the supported "future/invalid"
diagnostic condition.
Record:

* fixture SHA-256;
* exact expected diagnostic;
* confirmation that the fixture is never saved during the smoke.

If no such fixture exists:
S1-04 is incomplete.
Do NOT convert it to PASS using only Core evidence.
Core evidence remains supporting evidence, not the host observation required
by S1-04.
No AutoCAD execution is authorized now merely to create the fixture.
ACCEPTED WITH WORDING CORRECTION.
Do not assert that Debug.Assert necessarily terminates AutoCAD.
During Smoke-1 record:

* no Debug.Assert UI/diagnostic was observed;
* no unhandled exception was observed;
* no unexpected AutoCAD termination occurred;
* RackLog contains no transaction-leak indication;
* all expected drain/selection operations completed.

Any visible Debug assertion or equivalent transaction-leak diagnostic:
FAIL / STOP for review.
CONFIRMED.
For F1:
the relevant normal measurement record is the in-memory instrumentation
table exposed by the Workspace UI.
S1-09 uses:

* the in-memory measurement table;
* command-line observation for unwanted Regen/messages.

RackLog is NOT the normal metrics sink.
RackLog remains fault evidence only.
Do not change the T3 instrumentation decision.
ACCEPTED for Smoke-1 S1-07.
RACKEDITAR keeps an AutoCAD command active while its editor is open.
Therefore the F1 drain policy is already inhibited by the command-active
condition.
Expected panel text during the editor:
"Hay un comando en curso."
Do NOT require:
"Hay un cuadro de diálogo de RackCad abierto."
for F1 Smoke-1.
Record accurately:

* S1-07 validates coexistence and synchronization around the real
RACKEDITAR workflow;
* the host wiring of SetModalRackCadActive from classic commands is not
exercised;
* the modal branch of INV-23 remains covered by Core simulated-source
evidence.

Do not reopen classic commands in F1.
T3D is accepted subject to these documentary corrections:

1. D-T3D-01 remains unresolved until A-7 is accepted.
2. D-T3D-04 wording must not claim Debug.Assert necessarily terminates the
process.
3. D-T3D-03 fixture becomes a prerequisite for a Smoke-1 PASS.
4. D-T3D-05 and D-T3D-06 use the dispositions above.

No product change is authorized.
The existing gate-close package is accepted in substance.
Preserve its residuals R-01..R-12.
Do not rewrite historical task evidence.
Before resubmitting F1 close:

* incorporate the A-7 disposition;
* update INV-24 mapping;
* record D-T3D-01..06 decisions;
* update stale f1_resume_condition wording;
* record T3D review;
* keep src/ and tests/ identical to e0205b5a.

No new product verification is required if product bytes stay unchanged.
After A-7 completes and the documentary corrections are versioned with
exact CI 4/4:
return at:
COORDINATOR_F1_GATE_CLOSE_REQUIRED
Include:

* A-7 exact authority/status;
* corrected T3D procedure;
* corrected gate-close package;
* current HEAD/main;
* exact CI;
* final INV-24 mapping;
* confirmation src/tests remain identical to e0205b5a;
* confirmation no AutoCAD/Smoke-1/F2 occurred.

Do NOT run Smoke-1.
Do NOT request I-52 RELEASE yet.
Do NOT start F2.
Do NOT declare F1 PASS before this final gate review.
```
