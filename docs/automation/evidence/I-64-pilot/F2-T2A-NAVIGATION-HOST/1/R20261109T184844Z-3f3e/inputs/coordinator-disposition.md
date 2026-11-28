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
