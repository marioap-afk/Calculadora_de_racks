I-64 — F4-T1B v5
OWNER / COORDINATOR AUTHORIZATION
Current I-64 branch:
a332df7a0de454f5d472e7934b8c49d5b50a87e9
Current main:
812baf879612320653094a8aaf46aca0bac81383
F4-T4A1:
CLOSED / PASS WITH LIMITATIONS
F4-T4A2:
CLOSED / PASS WITH LIMITATIONS
Therefore:
F4-T4A / PREPARE = COMPLETE
Next required prerequisite before T4B:
F4-T1B — CLASSIC PUSH BACK CHARACTERIZATION
Authorize:
EMISSION + EXECUTION
attempts:
1 / 3
Use package:
v5
Do not emit T4B until T1B closes.
T1B is characterization.
It must freeze the current observable behavior of the classic Push Back
editor before T4B/T4C depend on its write-facing seams.
It must NOT redesign or clean up the editor.
The previously authorized interpretation remains binding.
RED may contain:

* the final characterization tests;
* ONLY the explicitly enumerated temporary product mutations from the v5
mutant table.

GREEN must:

* revert every intentional product mutation;
* retain the characterization tests.

Hard final condition:
for every production file intentionally mutated in RED:
GREEN bytes == BaseSha bytes
No residual production change is allowed.
The v5 mutant table is CLOSED.
Any mutation not explicitly present in that table:
STOP.
Do not invent a new mutant during Worker execution merely because a test
does not fail.
Freeze the measured legacy behavior as-is.
Do NOT fix during T1B:

* Enter behavior;
* Escape / close behavior;
* focus behavior;
* tab sequence;
* Insertar varias always enabled;
* confirmation/cancel oddities;
* header configurator close behavior;
* current subdialog ownership;
* any other explicitly characterized legacy quirk.

Record them as debt.
Use the v5 containment decision.
Do NOT instantiate the real header configurator in CI if doing so writes to
the real %LOCALAPPDATA% location.
Cover that behavior with:

* source guard;
* characterization of the surrounding contract;
* explicit declared limitation.

Do not modify production merely to make the configurator redirectable.
T1B must distinguish:
A. writes performed by the editor/window itself;
B. intent merely exposed by the window and consumed later;
C. actual DWG writes performed later by Plugin after the window closes.
Do not falsely characterize Plugin writes as window writes.
No real AutoCAD mutation is required in T1B.
Where a write occurs only after the classic window closes and inside Plugin:
characterize the seam with guards/tests.
Physical authored behavior is validated later in Smoke-2.
Characterize the actual current behavior.
Do not infer intended UX from WPF defaults.
Where the test can safely exercise real WPF behavior without uncontrolled
modal UI:
do so.
Where it cannot:
use source/structural guards and declare the limitation.
No characterization test may:

* hang waiting for a real dialog;
* leave a dispatcher/modal window alive;
* write to actual user profile data;
* require AutoCAD;
* depend on machine-specific paths.

Any such behavior:
STOP.
The contract must define per-mutant expected failures.
Do not use only a total failure count.
For each intentional RED mutation, prove the corresponding characterization
test(s) detect it.
A mutant with no discriminating test:
contract defect / STOP.
Required evidence:

* permanent tests added;
* every intentional product mutant reverted;
* final production bytes equal BaseSha;
* only authorized tests/docs remain changed.

This identity proof is central to T1B closure.
Require:

* planning fidelity;
* A1-A8;
* nc4 if applicable to the first delegation;
* Worker RED/GREEN;
* exact-SHA CI;
* direct review;
* independent adversarial review;
* Controller verification;
* nc1/nc2/nc3;
* custody.

Controller 14/14 alone is insufficient.
If T1B closes successfully:
the classic Push Back editor behavior becomes the characterized compatibility
baseline for T4B/T4C.
Do NOT treat characterization as endorsement of the odd behaviors.
After T1B closes:
return to F4-T4B package v3.
Re-run the relevant package validation/review against the post-T1B branch.
Then incorporate T1B findings into T4B assumptions.
Only then may T4B be emitted.
The prior Owner decision remains binding:
use the PILOT-SPECIFIC write path.
Application owns orchestration and classification.
Plugin exposes run-scoped primitive AutoCAD operations.
T4B owns mutation semantics.
T4C later owns command-context / DocumentLock integration.
Do not revert to the coarse I-55 seam merely because T1B characterizes the
legacy path.
F4-T4A:
COMPLETE
F4-T1B:
AUTHORIZED FOR EMISSION
attempts 1 / 3
F4-T4B:
WAITING FOR T1B CLOSE
F4-T4C:
WAITING
Smoke-2:
MANDATORY
F5-F7:
PENDING
