I-64 — F2-T2A-NAVIGATION-HOST
WORKER BLOCKED RECOVERY DISPOSITION
Current branch:
4fcea652d413414552d52f70998ce16f94f4c31b
origin/main:
a140eb3a9f055518f0ffa5a3f927ff1c0e1c1e60
Worker run:
R20261109T175557Z-a639
Worker status:
BLOCKED
RED SHA:
4fcea652d413414552d52f70998ce16f94f4c31b
GREEN SHA:
NONE
attempts:
1 / 3
The later message authorizing a Worker from e137 was OUT OF ORDER.
It is:
VOID FOR EXECUTION
It does NOT:

* revive e137;
* authorize another Worker;
* reset attempts;
* supersede the BASESHA recovery disposition;
* supersede the a639 BLOCKED state.

e137 remains:
VALID HISTORICAL PLANNING
SUPERSEDED_FOR_EXECUTION_BY_BASE_MOVEMENT
The executed Worker chain was:
5d33 -> a639
DECISION:
OPTION A
AUTHORIZED.
Sequence:

1. Preserve the existing saved patch exactly as evidence.
2. Restore the 8 uncommitted tracked paths to:
HEAD = 4fcea652
working-tree restoration only.
3. Do NOT create a commit.
4. Confirm:
   * clean tree;
   * HEAD = remote = 4fcea652;
   * config.toml accepted;
   * no participants.
5. Re-run the existing GitHub Actions run for RED:
42673031307
on the SAME SHA:
4fcea652
6. Do not modify ci.yml or push a replacement commit merely to obtain CI.
7. After the rerun reaches a terminal state, run the normal real Controller
verification against the BLOCKED Worker delivery / RED CurrentSha.

The purpose of this verification is to classify the actual a639 outcome,
not to pretend RED is a completed delivery.
Expected material facts:

* no GREEN SHA;
* WorkerStatus BLOCKED;
* RED behavior exists;
* clean tree after restoration;
* exact RED commit is remote;
* the Worker could not complete because one RED assertion is impossible
under the accepted scope.

Do not run nc1/nc2/nc3 unless the real verification itself is VERIFIED.
The original GitHub run:
42673031307
failed with zero jobs because of a GitHub internal server error.
That is:
CI TRANSPORT / PLATFORM FAILURE
not:
RED test failure
product failure
Worker implementation failure
A rerun of that same SHA is authorized.
It does not consume a product attempt.
If the rerun again fails before jobs execute because of platform failure:
STOP and return.
Do not create a new commit to force another CI.
DECISION:
THE PATCH MAY SEED ATTEMPT 2
but only under a NEW correction delegation.
The saved patch is:
historical Worker-produced implementation material.
It is NOT:

* accepted GREEN;
* verified product;
* a session-owned implementation;
* authorization to apply it before the next Worker.

After the current verification classifies Attempt 1 and a correction is
authorized:

1. attempts becomes:
2 / 3
2. correction state / analysis is committed BEFORE the new delegation as
required by 16.8;
3. new planning occurs on the new clean BaseSha;
4. A1-A8 / nc4 occur normally;
5. new Worker is invoked only after acceptance;
6. that Worker MAY read and reuse/apply the saved patch as a seed.

The new Worker must independently:

* inspect it;
* decide what is still valid;
* apply only in-scope portions;
* correct it as needed;
* run tests;
* create the new RED;
* create GREEN;
* commit and push its own chain.

Do NOT have the session apply the patch before the Worker.
Do NOT preserve dirty worktree state across the delegation boundary.
The impossible RED assertion is accepted as a defect in the Attempt-1 RED.
The new RED may narrow:
WorkspaceNavigationEchoTests
.NothingChangedInTheSelectionReadOrTheSessionToCarryTheHandles
so that it tests the intended architectural guarantee rather than rejecting
an unrelated pre-existing symbol:
InvalidateNavigationCaches
in:
WorkspaceSession.cs
The correction must be minimal.
It must still prove:

* WorkspaceSelectionRead.cs is unchanged;
* WorkspaceSession.cs is not modified to transport navigation-echo handles;
* no second echo transport is introduced there;
* the accepted echo mechanism stays within the authorized files.

Do not simply delete the architectural assertion.
The corrected RED must fail for intended missing behavior and be capable of
passing under the authorized GREEN implementation.
Do not classify Attempt 1 finally before the real Controller verification.
Expected direction:
REWORK / correction required
because:

* no GREEN exists;
* Worker stopped on an impossible RED assertion;
* product completion was not established.

The exact FailureClass / Disposition comes from the Controller.
DECISION:
ACCEPT AS A KNOWN LIMITATION FOR F2-T2A
WorkspaceNavigationFacts may leave:
ProbedId = null
when the existing permitted integrated authorities do not expose the raw
identifier required to populate it.
Do NOT:

* change RackBlockFinder;
* read RackBlockData/RackEmbedStore outside the authorized reader;
* expand into global inventory;
* add another authored-data reader solely to populate ProbedId.

Reason:
ProbedId is diagnostic information for an unreadable/probe-only member.
It must NOT determine a navigable target.
The navigation safety rule remains:
only members with an interpretable envelope and an attributable nonblank Id
may become NavigationTargets.
An unreadable member whose probe-only Id cannot be surfaced remains:
NON-NAVIGABLE
and must never be promoted into a target because ProbedId is absent.
Record the limitation explicitly:
F2T2A-KL-01:
The current integrated scan does not expose enough raw envelope identity to
populate NavigationDefinitionFact.ProbedId without crossing the accepted
read-authority boundary.
Consequences:

* unreadable/probe-only members may have less specific diagnostics;
* navigation target correctness is unaffected;
* no unreadable member becomes navigable;
* no second authored-data authority is introduced.

Tests must establish:
ProbedId = null
does NOT make an unreadable member navigable.
If implementation discovers that ProbedId is actually required for target
attribution or INV-40 safety:
STOP.
Return to Coordinator.
The limitation is accepted only while it is diagnostic-only.
Authorized now:
restore the 8 dirty paths to HEAD.
This is not a product correction.
It is cleanup required to hand the repository to the Controller.
The saved patch SHA remains evidence of the discarded uncommitted Worker
state.
Do NOT commit the restoration.
Do NOT delete the saved patch.
Execute now:

1. restore 8 dirty paths to HEAD;
2. prove clean tree;
3. rerun CI 42673031307;
4. wait for terminal CI result;
5. run normal Controller verification of a639 / 4fcea652;
6. custody/classify Attempt 1;
7. STOP at:

COORDINATOR_F2_T2A_REWORK_DECISION_REQUIRED
Do NOT:

* run negative controls before VERIFIED;
* invoke another Worker;
* apply the saved patch;
* start T2B.

F2-T1:
CLOSED / PASS
F2-T2A:
ATTEMPT 1 BLOCKED
FORMAL VERIFICATION PENDING
a639:
WORKER BLOCKED
attempts:
1 / 3
Attempt 2:
NOT YET AUTHORIZED
saved patch:
MAY BE USED AS FUTURE WORKER SEED
ProbedId null:
ACCEPTED KNOWN LIMITATION
DIAGNOSTIC ONLY
F2-T2B:
WAITING
Smoke-NAV:
PENDING
