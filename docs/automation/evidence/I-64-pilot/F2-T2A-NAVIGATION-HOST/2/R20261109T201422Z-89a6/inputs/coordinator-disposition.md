I-64 — F2-T2A-NAVIGATION-HOST
COORDINATOR ACCEPTANCE
Planning RunId:
R20261109T171357Z-e137
Current branch:
d9d386e91aa6df5db64fbb6946b7edb3542baace
Exact-SHA CI:
42663763540
4 / 4 SUCCESS
A1-A8:
PASS / PASS / PASS / PASS / PASS / PASS / PASS / PASS
nc4:
R20261109T171828Z-fd2a
Relative oracle:
MET
A3 = FAIL
all others = same as real delegation
Disposition:
REJECTED_BEFORE_INVOCATION
confined to nc4
The delegation produced by:
R20261109T171357Z-e137
is:
ACCEPTED
Worker invocation:
AUTHORIZED ONCE
Worker:
anthropic/subagent/claude-sonnet-5-5
Transport:
subagent
Effort:
high
Routing:
required
Attempt:
1
AttemptsRemaining:
2
MaxReworkLoops:
3
AllowedWriteScope is the exact delegation scope produced by e137.
Do not expand it.
In particular:
the two candidate existing files omitted by planning remain OUT OF SCOPE:
WorkspaceHostC2SourceGuardTests.cs
WorkspaceBridgeFacadeSourceGuardTests.cs
If either becomes necessary:
STOP P-03
Return to Coordinator.
Do not add it opportunistically.
The acceptance criteria contain an ambiguity around read APIs.
This disposition resolves it.
WorkspaceNavigationEffects.cs is explicitly authorized to OPEN the already
selected navigation target reference inside a scoped transaction for the
minimum read-only information required by:
VIEW
and
CENTER
including:

* existence;
* entity/reference type;
* geometric extents;
* information required to calculate the accepted view/center operation.

Therefore:
GetObject / equivalent scoped DB read
is AUTHORIZED in:
src/RackCad.Plugin/Workspace/WorkspaceNavigationEffects.cs
for this narrow purpose.
This does NOT make WorkspaceNavigationEffects a discovery authority.
It must NOT:

* enumerate Model Space;
* discover rack siblings;
* derive NavigationTargets;
* search for alternative references;
* build inventory;
* read authored rack data for target discovery.

Target identity comes from:
NavigationTargets / NavigationActionPlan
and is revalidated before the effect.
WorkspaceNavigationFacts.cs remains the exact-file authority for collecting
NavigationFacts / reference handles.
WorkspaceNavigationEffects.cs may only read the target it has already been
instructed to act upon.
Update the RED guard delta so that:
WorkspaceNavigationEffects.cs
is an exact-file exception for the minimum read APIs necessary for VIEW /
CENTER.
The exception must distinguish:
READ
from:
DISCOVERY
and from:
WRITE.
Writes remain limited to the approved navigation effects themselves:

* implied selection;
* view state required by VIEW/CENTER/LOCATE.

No authored data mutation.
No system-variable mutation.
No layout mutation.
No TILEMODE mutation.
Do not weaken the guard for the whole Plugin/Workspace directory.
The accepted command-context mechanism remains:
DocumentCollection.ExecuteInCommandContextAsync
only through:
WorkspaceNavigationHost.cs
The exact-file async exception remains accepted.
Do not use:
.Wait()
.Result
GetAwaiter().GetResult()
Task.Run
Task.Factory.StartNew
Thread
Timer
SendStringToExecute
hidden CommandMethod
The decisive revalidation and effects occur inside the command-context
callback.
The planned navigation port direction is ACCEPTED:
Task<WorkspaceNavigationResult> ExecuteAsync(...)
or the exact API emitted by the accepted delegation.
Requirements remain:

* no AutoCAD types in Application;
* immutable result;
* SessionId carried;
* rack/context carried;
* explicit action;
* explicit copy/view inputs as needed;
* normal rejected/stale/unavailable outcomes represented as values.

No event bus for action completion.
The planned shared NavigationEcho architecture is ACCEPTED.
WorkspaceSelectionReader may feed the complete implied-selection handle set
into the shared echo mechanism using only the exact accepted files.
Do NOT modify:
WorkspaceSelectionRead.cs
unless the accepted delegation explicitly includes it.
It currently does not.
Therefore the Worker must solve the echo using the accepted scope.
If that proves impossible without changing WorkspaceSelectionRead.cs:
STOP.
Do not extend scope.
RED must:

* compile;
* preserve the existing baseline except for intended failing new behavior
tests / exact guard deltas;
* establish the navigation-host behavior before GREEN.

Do not use a compile failure as RED.
The exact guard exceptions are introduced deliberately in RED.
GREEN must implement only what is required to satisfy the accepted T2A
contract.
No UI controls for T2B.
No F3 tabs.
No F4 editing.
No responsive redesign.
No new AutoCAD commands.
No legacy editor migration.
Core:
FullyQualifiedName~RackCad.Tests.Workspace
minimum:
656
ExpectRed:
true
UI:
FullyQualifiedName~RackCad.UI.Tests.Workspace
minimum:
64
ExpectRed:
false
Do not substitute ~Workspace.
On completion the Worker must provide:
RED SHA
GREEN SHA
CurrentSha
BaseSha
exact changed paths
required test counts
CI runs
routing/model evidence
guard-delta summary
known limitations
no AutoCAD claim beyond what CI/source/tests establish
No gate terms in Worker free text.
After one Worker invocation:
STOP.
Do not automatically run another Worker.
Proceed to:
real Controller verification
only through the normal T2A verification chain.
Then:
nc1
nc2
nc3
as required by the accepted protocol.
No A-11 substitute exists for T2A.
F2-T1:
CLOSED / PASS
F2-T2:
IN PROGRESS
F2-T2A:
DELEGATION ACCEPTED
WORKER AUTHORIZED ONCE
F2-T2B:
WAITING FOR T2A CLOSE
Smoke-NAV:
PENDING
attempts:
1 / 3

=====

I-64 — F2-T2A-NAVIGATION-HOST
BASESHA RECOVERY DISPOSITION
Current branch:
d9d386e91aa6df5db64fbb6946b7edb3542baace
origin branch:
d9d386e91aa6df5db64fbb6946b7edb3542baace
origin/main:
a140eb3a9f055518f0ffa5a3f927ff1c0e1c1e60
Worker:
NOT INVOKED
Worker authorization from the previous acceptance:
NOT CONSUMED
attempts:
1 / 3
The pre-invocation STOP is ACCEPTED.
Root cause:
SESSION / PROTOCOL ORDER DEFECT
The documentation custody commit:
d9d386e9
was created after planning and before Worker invocation.
That invalidated the accepted delegation's:
BaseSha = 35c3cd05...
for a normal Worker invocation.
Do NOT invoke the Worker from the old e137 delegation.
Do NOT change verification semantics to compensate.
Do NOT rewrite or revert published history.
Use:
REPLAN_ON_CURRENT_BASE
New planning BaseSha:
d9d386e91aa6df5db64fbb6946b7edb3542baace
No additional session/state/custody commit is authorized before planning.
The already saved:

* acceptance literal;
* pre-invocation STOP record;
* recovery disposition;

remain in ignored artifacts until the next normal custody point.
They do not need to be committed before the Worker.
Reuse the SAME issued T2A gate contract.
AuthorityRevision:
09a1499a46d16c8251ac4575d370b551603b6372
MainSha:
a140eb3a9f055518f0ffa5a3f927ff1c0e1c1e60
Do NOT issue a new contract.
Do NOT change:

* Objective;
* ForbiddenWriteScope;
* Invariants;
* RequiredTests;
* ExpectedEvidence;
* StopConditions;
* routing requirements.

The recovery changes only the planning base and carries the already accepted
Coordinator clarifications.
The new planning input must include the already accepted T2A disposition.
In particular:

1. WorkspaceNavigationEffects.cs may perform READ-ONLY GetObject /
equivalent access on the ALREADY-SELECTED target for VIEW/CENTER.
2. That read is only for:
   * existence;
   * type;
   * geometric extents;
   * information strictly necessary for the approved view/center effect.
3. WorkspaceNavigationEffects.cs is NOT a discovery authority.
4. WorkspaceHostC2SourceGuardTests.cs remains out of scope.
5. WorkspaceBridgeFacadeSourceGuardTests.cs remains out of scope.
6. WorkspaceSelectionRead.cs remains out of scope.
7. Any need for one of those files:
STOP P-03.
8. ExecuteInCommandContextAsync async/await exception remains limited to
WorkspaceNavigationHost.cs.

These are accepted Coordinator interpretations of the existing contract,
not a scope expansion.
Authorize:
ONE new Controller planning invocation.
Controller:
gpt-6-luna
Effort:
high
read-only
Use the same planning recipe as e137.
New planning must have:
BaseSha =
d9d386e91aa6df5db64fbb6946b7edb3542baace
The old e137 delegation becomes:
SUPERSEDED_FOR_EXECUTION_BY_BASE_MOVEMENT
It remains historical and immutable.
Do not label it invalid.
After the new planning output:
run A1-A8.
Then run nc4.
Do NOT commit either result before Worker invocation.
All artifacts remain under ignored artifacts/.
To avoid repeating the same protocol-order defect, the Worker is
PRE-AUTHORIZED without another Coordinator round IF AND ONLY IF every
condition below is true.

1. Planning output is schema-valid.
2. Planning terminates normally.
3. Effective Controller model/effort are:
gpt-6-luna / high.
4. Routing remains:
Deep
and:
NOT Long-horizon.
5. A1-A8:
all eight PASS.
6. nc4:
relative oracle MET;
A3 = fail;
every other A-result equals the real delegation.
7. New AllowedWriteScope is EXACTLY the same ordered scope as the accepted
e137 delegation:
same 7 new files
plus
same 8 exact existing files.
8. It does NOT add:
WorkspaceHostC2SourceGuardTests.cs
9. It does NOT add:
WorkspaceBridgeFacadeSourceGuardTests.cs
10. It does NOT add:
WorkspaceSelectionRead.cs
11. It does NOT remove any path from the accepted e137 scope.
12. ForbiddenWriteScope is identical to the contract.
13. Invariants are identical to the contract.
14. RequiredTests are identical to the contract.
15. ExpectedEvidence is identical to the contract.
16. StopConditions are identical to the contract.
17. Objective is identical to the contract.
18. BaseSha is exactly current HEAD/remoto:
d9d386e9...
19. origin/main remains:
a140eb3a...
20. config.toml remains accepted A/B/C/D immediately before Worker.
21. repository tree is clean.
22. HEAD = remote branch.
23. no participant/process conflict exists.
24. no usage/credit warning occurs during planning or nc4.

If ALL 24 conditions are met:
the new delegation is:
ACCEPTED_BY_PREAUTHORIZED_MECHANICAL_EQUIVALENCE
and:
ONE Worker invocation is AUTHORIZED immediately.
Do not stop for another Coordinator acceptance.
If ANY of the 24 conditions fails:
STOP.
Do NOT invoke Worker.
Return at:
COORDINATOR_F2_T2A_ACCEPTANCE_REQUIRED
with the exact difference.
No interpretation by the session.
From the start of the new planning through Worker completion:
NO session-authored Git commit.
Sequence:

1. current clean d9d386e9;
2. planning;
3. A1-A8;
4. nc4;
5. mechanical equivalence check;
6. Worker invocation;
7. Worker RED commit;
8. Worker GREEN commit/push.

The first product commits after BaseSha must therefore be Worker commits.
Do not custody planning/nc4 into Git before the Worker.
Their durable custody happens AFTER the Worker phase when the protocol again
permits session documentation commits.
With BaseSha = d9d386e9:
git log BaseSha..CurrentSha
during verification must contain only the Worker product chain relevant to
this delegation.
Therefore the previous session documentation commits are outside the
verification interval.
This restores normal Trailer semantics.
With BaseSha = d9d386e9:
git diff --name-only BaseSha..CurrentSha
contains only Worker changes.
Therefore the previous docs/ custody paths are outside the T2A Scope
interval.
No special Scope exception is required.
Preserve:
R20261109T171357Z-e137
as:
VALID PLANNING
VALID DELEGATION AT ITS ORIGINAL BASE
SUPERSEDED_FOR_EXECUTION_BY_BASE_MOVEMENT
Its:
A1-A8 PASS
and:
nc4 PASS
remain historical evidence.
They do NOT authorize execution after BaseSha moved.
The replan is recovery from a pre-invocation protocol-order defect.
Worker attempts:
still 1 / 3 available.
No Worker attempt has been consumed.
The new planning invocation is authorized as recovery.
Do not reset any historical counters.
If the mechanically equivalent delegation is produced and Worker completes:
STOP after the single Worker invocation.
Then proceed to the normal T2A verification chain.
Do NOT automatically invoke a second Worker.
Do NOT start T2B.
F2-T1:
CLOSED / PASS
F2-T2A:
BASE RECOVERY AUTHORIZED
old e137:
SUPERSEDED FOR EXECUTION
new planning:
AUTHORIZED ONCE
Worker:
CONDITIONALLY PRE-AUTHORIZED
NOT YET INVOKED
F2-T2B:
WAITING
Smoke-NAV:
PENDING
attempts:
1 / 3

=====

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

=====

I-64 — F2-T2A-NAVIGATION-HOST
ATTEMPT 2 DELEGATION ACCEPTANCE
Planning:
R20261109T193332Z-bafe
BaseSha:
53302b85518a8d0eaaa07cf1af5affc97581aa95
Current branch:
53302b85518a8d0eaaa07cf1af5affc97581aa95
origin/main:
a140eb3a9f055518f0ffa5a3f927ff1c0e1c1e60
A1-A8:
8 / 8 PASS
nc4:
NOT REPEATED
CORRECT FOR THIS CHAIN
attempts:
2 / 3
Delegation:
R20261109T193332Z-bafe
is:
ACCEPTED
Worker Attempt 2:
AUTHORIZED ONCE
No new planning is required.
No commit is required before Worker invocation.
Accepted difference:
Dimensions.Horizon
previous:
Low
new:
Medium
This does NOT change effective routing.
The task remains:
Deep
and:
NOT Long-horizon
because only:
Horizon = High
forces Long-horizon under the current routing authority.
Therefore:
F2T2-C07 = NOT TRIGGERED
The Horizon reevaluation is accepted as a planning judgment difference.
It does not require a new contract or delegation.
Condition 16 is overridden narrowly for this occurrence.
The condition was intended to prevent a material expansion of the correction.
The only observed difference outside the prepared expected-difference list is:
Dimensions.Horizon: Low -> Medium
That field does not change:

* Objective;
* AllowedWriteScope;
* ForbiddenWriteScope;
* Invariants;
* RequiredTests;
* ExpectedEvidence;
* StopConditions;
* Worker model;
* effort;
* routing level;
* implementation scope;
* correction content.

Therefore:
Condition 16 = SATISFIED BY COORDINATOR DISPOSITION
for bafe only.
Do not generalize this override.
Criterion 22 is ACCEPTED.
The added RED test proving:
ProbedId = null
does NOT make an unreadable member navigable
is required evidence for:
F2T2A-KL-01
and follows directly from the prior Coordinator disposition.
It is NOT scope expansion.
Required behavior:

* unreadable/probe-only member remains non-navigable;
* null ProbedId does not promote it into NavigationTargets;
* target attribution/safety do not depend on ProbedId;
* no new read authority is introduced merely to populate it.

The test belongs within the accepted T2A Core test scope.
Worker must create a NEW RED.
The RED must include the corrected version of:
NothingChangedInTheSelectionReadOrTheSessionToCarryTheHandles
so it tests the actual architectural invariant without rejecting the
pre-existing:
InvalidateNavigationCaches
symbol.
The RED must also include Criterion 22 coverage.
RED must compile.
Expected failures must be assertions/behavioral guards, not compilation
failure.
The Attempt-1 saved patch MAY be provided to the Worker as seed/reference.
The Worker owns all decisions about reuse.
The session must not apply it.
The Worker must independently:

* inspect;
* reuse/discard;
* correct;
* test;
* commit RED;
* commit GREEN;
* push.

Immediately before invoking the Worker, re-check:

1. HEAD = remote = 53302b85...
2. origin/main = a140eb3a...
3. tree clean;
4. config.toml = accepted A/B/C/D;
5. 0 conflicting participants;
6. no new session commit since planning;
7. AllowedWriteScope remains bafe exact scope.

If any fails:
STOP.
Do not invoke Worker.
Worker:
claude-sonnet-5-5
Effort:
high
Attempt:
2
One invocation.
BaseSha:
53302b85518a8d0eaaa07cf1af5affc97581aa95
Required sequence:
BaseSha
-> corrected RED commit
-> GREEN commit
-> push
No session-authored commit between planning and Worker.
STOP after the single Attempt-2 Worker invocation.
Do not invoke Attempt 3 automatically.
Then run the normal Controller verification chain.
If real verification = EXECUTION_VERIFIED:
proceed with normal controls.
If REWORK/BLOCKED/STOP:
return to Coordinator.
F2-T1:
CLOSED / PASS
F2-T2A:
ATTEMPT 1 = REWORK
ATTEMPT 2 = DELEGATION ACCEPTED
WORKER AUTHORIZED ONCE
attempts:
2 / 3
nc4:
already satisfied for T2A
F2-T2B:
WAITING
Smoke-NAV:
PENDING
