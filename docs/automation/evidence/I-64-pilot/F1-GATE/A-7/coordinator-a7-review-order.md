# I-64 — Orden del Coordinator de revisión de I-64-A-7

Transcripción literal recibida por el chat de la sesión responsable el 2026-10-11, tras el commit 6a574fc0
(CI 38171122754 4/4) y la parada en COORDINATOR_A7_DISPOSITION_REQUIRED.

```text
I-64 — COORDINATOR A-7 REVIEW ORDER
Amendment:
I-64-A-7
Exact object:
docs/initiatives/I-64-A-7.md
Revision / branch tip to review:
6a574fc09a64a82151aae0a9504391a67a224dab
Freeze authority:
commit:
9b43dafbe3b8874aa0ba5a85d62cbf953dbdd311
path:
docs/initiatives/I-64-proposal-v5.md
blob:
dc1924ff5a73a4e529fbc5f995621d29351a091b
Coordinator decision:
M-04 = YES
Reason:
A-7 changes the closing/failure semantics of Smoke-1:

* S1-06a..e are presently literal Smoke-1 checks;
* under A-7 they become NOT_APPLICABLE_BY_DESIGN in Smoke-1;
* their literal editable-field verification moves to Smoke-2;
* therefore what prevents or permits Smoke-1/F1 closure changes.

This is a material lifecycle verification change.
Authority required:
Architect + Coordinator
Owner:
NOT REQUIRED for amendment approval,
but INFORMED.
Reason:
OV-19 is not retired and its final verification obligation is not removed.
It retains assignments through the new S2-16..S2-22 scenarios.
Invoke exactly ONE formal Architect review.
Model:
gpt-6.1-sol
Effort:
high
Mode:
read-only
Use the currently measured/accepted CLI cell if still eligible.
If CLI/config/model eligibility changed:
STOP Coordinator before invocation.
No Git writes during the review.
Architect must read at minimum:

1. 

docs/initiatives/I-64-A-7.md
at 6a574fc0

2. 

docs/initiatives/I-64-proposal-v5.md
Freeze exact blob dc1924ff...

3. 

docs/INITIATIVE_LIFECYCLE.md
materiality and amendment/lifecycle rules

4. 

docs/initiatives/I-64-f1-t3d-smoke-1-procedure.md
at 6a574fc0

5. 

docs/automation/evidence/I-64-pilot/F1-GATE/gate-close-package.md
at 6a574fc0

6. 

the Coordinator F1 close review that required A-7.
No product execution is needed.
Q1 — MATERIALITY
Does A-7 correctly classify M-04 = YES because it changes what evidence is
required for Smoke-1/F1 closure?
Return:
AGREE
or
REQUIRED finding.
Q2 — REQUIREMENT PRESERVATION
Does A-7 preserve the full semantic obligation of INV-24 / PV2-12, rather
than weakening it?
Specifically verify that:

* "field" is not redefined;
* S1-06a..e are never marked PASS in F1;
* SM-14a..e remain supplementary only;
* S1-06f/g/h remain in Smoke-1;
* literal editable-field behavior is still mandatory later.

Q3 — REASSIGNMENT
Is reassignment to:
S2-16
S2-17
S2-18
S2-19
S2-20
S2-21
S2-22
a valid and unambiguous future verification location?
Check that those IDs do not collide with existing Smoke-2 IDs and that the
new cases collectively cover:

* text entry with spaces;
* Ctrl+Z on text;
* Ctrl+C/V on text;
* Delete isolation;
* Esc/Enter isolation;
* DBMOD;
* focus not returning to drawing merely because an edit ends.

Q4 — OWNER AUTHORITY
Is Owner approval correctly NOT REQUIRED because:

* OV-19 is not removed;
* no last assignment is removed;
* the obligation remains mandatory;
* only its literal editable-field execution is reassigned?

If not, return REQUIRED and cite the lifecycle clause.
Q5 — F1 CLOSURE
Assuming A-7 is accepted, is it architecturally valid for INV-24 at F1 to
close with:

* S1-06f/g/h remaining Smoke-1 obligations;
* SM-14a..e as supplementary evidence;
* S1-06a..e classified NOT_APPLICABLE_BY_DESIGN;
* S2-16..S2-22 carrying the deferred literal editable-field obligation?

Q6 — FUTURE-GATE BINDING
Does A-7 bind the deferred scenarios strongly enough that F4 cannot later
omit them without another amendment?
If not, identify the exact required wording.
Return:
Disposition:
AGREED
or
CHANGES REQUIRED
Then:

* M-04 disposition;
* Owner-required disposition;
* REQUIRED findings, each with stable ID;
* OPTIONAL findings;
* explicit statement whether INV-24 is weakened;
* explicit statement whether S2-16..S2-22 are sufficient and binding;
* explicit statement whether A-7 is suitable for Coordinator activation.

No implementation recommendations outside A-7 scope.
One valid Architect review.
Up to two additional invocations only for:

* transport failure;
* INVALID_OUTPUT.

A valid CHANGES REQUIRED result consumes the valid review and returns to
Coordinator.
Do not self-correct A-7 after a valid CHANGES REQUIRED without new
Coordinator authority.
Separately from A-7 authority, Coordinator confirms T3D's corrected S1-04
fixture requirements.
Smoke-1 PASS requires host observation of BOTH:
A. SIN IDENTIDAD
A RackCad-recognizable/readable block whose envelope has a blank RackId.
Expected UI:
"Sin identidad"
and its exact current explanatory text.
B. DIAGNOSTICO
A RackCad block whose definition/envelope is intentionally unreadable,
invalid or future-version according to the current classifier.
Expected UI:
"Diagnóstico"
and its exact current explanatory text.
For each fixture record before Smoke-1:

* source file;
* SHA-256;
* exact expected panel text;
* provenance/purpose;
* confirmation it will never be saved during Smoke-1.

A normal non-RackCad block is NOT the Sin identidad fixture.
It belongs to the "otros objetos" selection case.
If either required fixture is unavailable:
S1-04 = INCOMPLETE
Smoke-1 cannot PASS.
Do not run AutoCAD now to create these fixtures.
After the Architect review:
do not activate A-7 yourself.
Stop at:
COORDINATOR_A7_ARCHITECT_REVIEW_REQUIRED
Return:

* Architect RunId;
* exact model/effort;
* disposition;
* REQUIRED findings;
* OPTIONAL findings;
* M-04 result;
* Owner-required result;
* assessment of S2-16..S2-22;
* whether Architect says A-7 is ready for Coordinator activation.

Do NOT run Smoke-1.
Do NOT declare F1 PASS.
Do NOT start F2.
```
