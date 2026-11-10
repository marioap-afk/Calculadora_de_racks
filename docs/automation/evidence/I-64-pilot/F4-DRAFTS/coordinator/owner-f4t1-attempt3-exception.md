I-64 — F4-T1-DRAFT-CORE
ATTEMPT 3 — N-1
LIMITED 16.8 RED/GREEN EXCEPTION
Decision:
OPTION 1 AUTHORIZED
Scope:
F4-T1
Attempt 3 / 3
N-1 guard regression only
AUTOMATION_PLAN §16.8 normally requires:

* a new RED when the correction touches ChainRedFiles;
* GREEN must not touch RT ∪ ChainRedFiles.

For N-1, both the regression tests and the guard implementation live in:
WorkspaceDraftBoundaryTests.cs
which is already part of ChainRedFiles.
Therefore the ordinary rule cannot simultaneously produce:

1. a meaningful failing RED; and
2. a GREEN that fixes the guard;

without touching the same file in GREEN.
This is a structural collision between test placement and the generic
RED/GREEN rule.
It is NOT authority to relax §16.8 generally.
RED may modify ONLY the test/assertion portion required to expose N-1.
The RED must add regression coverage proving the current weakened BOM rule
fails to detect the required cases.
At minimum:
BOM
Bom
bom
plus the contract-required cases for:

* prefix;
* suffix;
* member-name position.

RED must:

* compile;
* fail by assertion/guard behavior;
* fail only for the intended N-1 regression surface;
* be committed and pushed;
* have exact-SHA CI evidence.

GREEN may modify the SAME ChainRedFile only under this exception.
Permitted GREEN delta in that file is LIMITED TO:
the BOM guard/rule implementation required to satisfy N-1.
GREEN MUST NOT:

* add tests;
* remove tests;
* rename tests;
* change assertions;
* weaken assertions;
* alter test data;
* change any unrelated guard family;
* modify DraftSession/core behavior;
* modify T2/T3/T4 product code.

The RED test portion must be byte-identical between:
RedSha
and
CurrentSha
except for the authorized guard implementation region.
Because the normal file-level immutability check cannot be used here, the
delivery must provide a finer-grained proof.
Record deterministic hashes for:

1. the RED regression test block;
2. the same test block in GREEN.

Required:
identical hashes.
Also prove that:
git diff RedSha..CurrentSha -- WorkspaceDraftBoundaryTests.cs
contains only the authorized BOM guard implementation delta.
Any test/assertion change in GREEN:
FAIL / REWORK.
The Controller verification receives this disposition as UNIT authority.
For Checks.Tests:
do NOT require:
git diff --name-only RedSha CurrentSha
to exclude WorkspaceDraftBoundaryTests.cs as a whole.
Instead, for this Attempt 3 only, substitute the narrow oracle:

* RED regression tests present and failing in RedSha;
* same regression tests byte-identical in GREEN;
* GREEN diff in the ChainRedFile contains only the authorized BOM-rule
implementation;
* GREEN exact-SHA tests pass.

All other §16.8 and §16.9 rules remain unchanged.
RED:
Core CI must fail because of the new N-1 regression tests.
GREEN:
exact-SHA CI must pass.
RedPart:
PASS only if the intended regression tests are among the RED failures.
No compile-failure RED.
No unrelated failing tests accepted.
The repaired rule must be at least as strong as the guard behavior required
by:
INV-F4T1-22
and must detect the required BOM family without weakening previously
protected cases.
The correction must not merely special-case one literal test string if that
would leave the underlying case-sensitivity defect intact.
F4-T1 remains:
Attempt 3 / 3
This exception does NOT create a fourth T1 attempt.
After successful verification/controls:
F4-T1 = CLOSED / PASS
If Attempt 3 fails for product reasons:
STOP.
Do not extend T1 beyond 3 / 3 without a new Owner decision.
N-2 remains OUTSIDE this correction.
It is a mandatory F4-T2 obligation exactly as previously ordered.
Do not fold N-2 into Attempt 3.
Proceed now:

1. record this exception as UNIT authority;
2. issue Attempt-3 contract;
3. create RED with regression tests only;
4. create GREEN with BOM-rule correction only;
5. prove test-block immutability;
6. exact-SHA CI;
7. direct review;
8. independent review;
9. Controller verification using the narrow Tests exception;
10. nc1-nc3;
11. close F4-T1 if PASS;
12. start F4-T2 at attempts = 1 / 3.

No further Owner decision is required merely because this file belongs to
ChainRedFiles.
