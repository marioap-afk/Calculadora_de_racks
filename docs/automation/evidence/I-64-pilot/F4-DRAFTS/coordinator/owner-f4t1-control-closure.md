I-64 — F4-T1-DRAFT-CORE
CONTROL CLOSURE DISPOSITION
Current branch:
61d83788e7fa8decd861626b7e3ca89c8f23b9b4
Verified product:
ce73ceb7
Reverification:
1f4f
EXECUTION_VERIFIED / NONE
14 / 14 PASS
The product is VERIFIED.
No further:

* Worker;
* RED;
* GREEN;
* product attempt;
* product correction

is authorized or required for F4-T1.
The remaining issues are control-construction issues only.
Classification:
CONTROL NOT PASSED
Cause:
the nc1 mutation correctly changed CurrentSha, but the control preparation
failed to recompute the additional derived:
ReissuedSha256
inside reissue-record.json.
That caused:
Handoff = fail
before the intended:
Identity = fail
This is a control-preparation defect.
It is NOT a product defect.
Authorize ONE corrected nc1 execution.
Correct the control recipe so that when the nc1 handoff mutation changes the
handoff bytes it recomputes ALL derived hashes, including:

* relay/output derived SHA;
* ReissuedSha256 in reissue-record.json;
* any other field whose only semantic role is to authenticate the mutated
handoff bytes.

Do NOT alter:

* mutation semantics;
* product SHA;
* custody SHA;
* post-custody Identity rule;
* Controller prompt semantics;
* expected oracle.

Expected:
Handoff = pass
Identity = fail
FailureClass = Identity
EXECUTION_BLOCKED / STOP
This does not consume a product attempt.
Authorize nc3 using the SAME corrected derived-hash recipe.
Because nc3 deliberately changes free text in the handoff, every hash derived
from those handoff bytes must be recomputed consistently.
Expected:
all earlier checks = pass
FreeText = fail
FailureClass = FreeText
EXECUTION_REWORK_REQUIRED / REWORK
Do not launch nc3 with the known stale ReissuedSha256 preparation defect.
50da remains:
CONTROL NOT PASSED
Do NOT classify it as CONTROL PASS.
Observed:
Contract = fail
Scope = fail
The intended nc2 oracle requires:
Contract = pass
Scope = fail
Therefore the ordinary oracle was not met.
The F4-T1 Attempt-3 delegation has exactly ONE allowed write path.
nc2 is defined by removing from AllowedWriteScope a path actually touched by
the delivery.
With only one path:
removing that path makes AllowedWriteScope empty.
But the contract itself requires the exact single path.
Therefore the nc2 mutation necessarily creates BOTH:
Contract failure
and
Scope failure.
There is no alternative ordinary nc2 mutation that simultaneously:

* excludes the real changed path;
* preserves the exact contract scope;
* and leaves Contract = pass.

This is a structural property of this one-path delegation, not Controller
randomness.
Authorize one I-64/F4-T1-only substitute:
F4T1-NC2-SINGLE-PATH-SCOPE-01
Purpose:
prove the intended nc2 property without pretending the impossible ordinary
relative oracle can be satisfied.
This is NOT A-4.
Do not broaden A-4.
The substitute must prove deterministically:

1. the real accepted delegation has exactly one AllowedWriteScope path;
2. that path is:

WorkspaceDraftBoundaryTests.cs
at its exact contracted repository path;

3. BaseSha..CurrentSha touches that path;
4. the nc2 mutated delegation removes that exact path;
5. after removal, the real changed path is outside mutated
AllowedWriteScope;
6. therefore the Scope predicate evaluates:

FAIL

7. ForbiddenWriteScope behavior remains unchanged;
8. the only delegation semantic mutation is the nc2 AllowedWriteScope
removal plus mechanically derived hashes;
9. the Contract failure in 50da is an unavoidable consequence of the
one-path exact-scope contract, not a second independent mutation;
10. 50da itself demonstrates that the Controller also computed:

Scope = fail.
Before accepting the substitute, test at minimum:
A.
one-path real scope, changed path removed
-> SUBSTITUTE PASS
B.
mutated scope still covers changed path
-> FAIL
C.
different changed path than the removed path
-> FAIL
D.
second delegation field changed
-> FAIL
E.
ForbiddenWriteScope changed
-> FAIL
F.
real delegation has more than one allowed path
-> NOT APPLICABLE / FAIL
G.
BaseSha..CurrentSha does not touch the removed path
-> FAIL
H.
50da did not report Scope = fail
-> FAIL
No product data or tests need to change.
Record:
50da = CONTROL NOT PASSED
and separately:
nc2 = SATISFIED_UNDER_F4T1_SINGLE_PATH_SCOPE_SUBSTITUTE
Do NOT rewrite 50da.
Do NOT call it a normal CONTROL PASS.
A-4 covers the known Controller defect where Scope incorrectly remains PASS.
Here Scope correctly failed.
The problem is that Contract also necessarily failed first because the exact
single-path contract became empty.
Therefore:
A-4 is not the right authority.
Proceed in this order:

1. record this disposition without moving HEAD before a control if that
would invalidate the current post-custody frame;
2. run corrected nc1 once;
3. if nc1 closes, execute the nc2 single-path substitute;
4. if substitute passes, record nc2 satisfied under that substitute;
5. run nc3 using corrected derived-hash handling;
6. if nc3 closes, custody all evidence;
7. close:

F4-T1 = CLOSED / PASS

8. then begin F4-T2 with:

attempts = 1 / 3
and mandatory N-2 anti-aliasing requirement.
STOP if:

* corrected nc1 still fails Handoff;
* nc1 does not isolate Identity;
* single-path substitute fails any case;
* nc3 fails before FreeText;
* config changes again;
* product/custody branch moves unexpectedly.

Do not create another product attempt.
F4-T1 product:
VERIFIED
Product SHA:
ce73ceb7
Reverification:
1f4f = 14 / 14 PASS
nc1:
c4f7 CONTROL NOT PASSED
one corrected execution authorized
nc2:
50da CONTROL NOT PASSED
single-path substitute authorized
nc3:
not run
authorized after nc1/nc2 closure
F4-T2:
WAITING
attempts 1 / 3
