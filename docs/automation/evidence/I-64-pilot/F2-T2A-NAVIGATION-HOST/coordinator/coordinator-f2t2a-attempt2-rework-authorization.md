I-64 — F2-T2A-NAVIGATION-HOST
ATTEMPT 2 REWORK AUTHORIZATION
Current branch:
d324e905bdd41aa5d72385428e9c223be3701791
origin/main:
a140eb3a9f055518f0ffa5a3f927ff1c0e1c1e60
Attempt 1 verification:
R20261109T184844Z-3f3e
Classification:
EXECUTION_REWORK_REQUIRED
Disposition:
REWORK
FailureClass:
Ci
RED:
4fcea652d413414552d52f70998ce16f94f4c31b
RED PART:
ACCEPTED
attempts:
2 / 3
Attempt 1 is CLOSED AS REWORK.
The RED commit is valid evidence.
The Worker did not produce a valid GREEN.
The impossible RED assertion is accepted as the correction trigger.
No nc1/nc2/nc3 are run for Attempt 1 because the real verification was not
EXECUTION_VERIFIED.
Attempt 2 is AUTHORIZED.
This is a correction under AUTOMATION_PLAN 16.8.
Before the new delegation:

1. create analysis.md describing:
   * Attempt 1 result;
   * FailureClass Ci;
   * impossible RED assertion;
   * exact correction;
   * saved patch provenance;
   * F2T2A-KL-01 / ProbedId decision;
2. update state:
attempts = 2 / 3;
3. custody the Coordinator dispositions and Attempt-1 classification;
4. commit/push that documentation/evidence/state correction package.

That commit becomes the NEW BaseSha.
Do not modify product in that commit.
The current repository contains the accredited Attempt-1 RED.
Therefore a docs-only correction-state commit may still have the same Core
RED failures.
Do NOT require 4/4 green from that docs-only commit.
Instead verify that:

* the commit is docs/evidence/state only;
* the Core failure set remains the same Attempt-1 RED set;
* no new UI failure appears;
* no product/test file changes are introduced by the session;
* HEAD = remote and tree is clean.

That is sufficient to establish the new planning BaseSha.
Attempt 2 MUST create a new RED.
Reason:
the correction changes a ChainRedFile / RED assertion.
The new RED must minimally correct:
WorkspaceNavigationEchoTests
.NothingChangedInTheSelectionReadOrTheSessionToCarryTheHandles
The previous broad regex:
Echo|Navigation
must not reject the unrelated existing symbol:
InvalidateNavigationCaches
in WorkspaceSession.cs.
The replacement assertion must prove the intended invariant directly.
Required intent:

* WorkspaceSelectionRead.cs remains unchanged;
* WorkspaceSession.cs is not used to carry navigation echo handles;
* no echo-specific state/property/field/transport is added there;
* the approved echo mechanism remains NavigationEcho plus the exact
authorized reader/host-core integration.

Do not remove the invariant entirely.
The saved patch:
SHA-256
32C19A574CCF85E513309EF62116A29876EBFCDF8796579043A0B1209E44C046
MAY be supplied to the Attempt-2 Worker as:
REFERENCE / SEED MATERIAL
The session must NOT apply it.
The Worker may:

* inspect it;
* reuse valid portions;
* discard invalid portions;
* reimplement portions;
* correct them.

The Worker remains responsible for the final implementation and commits.
The patch is not accepted GREEN evidence.
F2T2A-KL-01 remains ACCEPTED.
ProbedId may remain null where the permitted integrated scan cannot expose
the raw probe identity.
This is diagnostic-only.
It is acceptable only if:

* unreadable/probe-only members remain non-navigable;
* target attribution does not depend on ProbedId;
* INV-40 safety does not depend on ProbedId.

Do NOT expand read authority merely to populate it.
If Attempt 2 discovers that ProbedId is required for navigation correctness
or revalidation:
STOP.
After the correction-state commit:
run ONE new Controller planning invocation.
Task:
F2-T2A-NAVIGATION-HOST
Attempt:
2
CorrectionOf:
R20261109T184844Z-3f3e
FailureClass:
Ci
Use the same issued gate contract and Coordinator clarifications.
The new BaseSha is the correction-state commit.
origin/main must remain:
a140eb3a...
DO NOT REPEAT nc4.
Reason:
README §10 defines nc4 as:
"solo en la primera delegación de la cadena"
T2A remains the same delegated chain.
nc4:
R20261109T175144Z-6a04
already satisfied the relative oracle for this chain.
The exception requiring nc4 to repeat applies only if the real delegation
failed A6 because main advanced.
That is not the current condition.
Therefore Attempt 2 requires:
A1-A8
but:
NO NEW nc4
Evaluate the new Attempt-2 delegation normally.
All eight must PASS.
The corrected delegation must preserve the same contract authorities,
forbidden scope, invariants, tests and STOP conditions except for legitimate
Attempt/CorrectionOf metadata and the corrected RED acceptance criteria.
To avoid another BaseSha movement, the Attempt-2 Worker is PRE-AUTHORIZED if
all of the following hold:

1. planning schema valid;
2. planning normal termination;
3. gpt-6-luna / high effective;
4. routing remains Deep and not Long-horizon;
5. A1-A8 = 8 PASS;
6. BaseSha = current HEAD = remote;
7. origin/main = a140eb3a;
8. AllowedWriteScope does not expand beyond the accepted T2A contract;
9. no previously forbidden existing file is added;
10. WorkspaceSelectionRead.cs remains outside scope;
11. WorkspaceHostC2SourceGuardTests.cs remains outside scope;
12. WorkspaceBridgeFacadeSourceGuardTests.cs remains outside scope;
13. RequiredTests unchanged;
14. ForbiddenWriteScope unchanged;
15. invariants unchanged except explicit correction wording;
16. correction is limited to the impossible RED assertion and resulting
implementation;
17. config.toml remains accepted;
18. tree clean;
19. no participant conflict;
20. no usage/credit warning.

If all 20 hold:
Worker Attempt 2 is AUTHORIZED immediately.
Do not commit planning/A1-A8 before the Worker.
If any differs:
STOP at:
COORDINATOR_F2_T2A_ACCEPTANCE_REQUIRED
Worker:
claude-sonnet-5-5 / high
One invocation.
The Worker may use the saved Attempt-1 patch as seed material.
Required sequence:
BaseSha
→ corrected RED commit
→ GREEN commit
→ push
No session-authored commit between planning and Worker.
STOP after the Worker.
Then run normal Controller verification.
If VERIFIED:
continue with the normal negative-control chain.
If REWORK:
return to Coordinator.
Do not automatically consume Attempt 3.
F2-T1:
CLOSED / PASS
F2-T2A:
ATTEMPT 1 = REWORK
ATTEMPT 2 = AUTHORIZED
attempts:
2 / 3
nc4:
ALREADY SATISFIED FOR T2A
DO NOT REPEAT
saved patch:
AUTHORIZED AS WORKER SEED ONLY
ProbedId null:
KNOWN LIMITATION ACCEPTED
F2-T2B:
WAITING
Smoke-NAV:
PENDING
