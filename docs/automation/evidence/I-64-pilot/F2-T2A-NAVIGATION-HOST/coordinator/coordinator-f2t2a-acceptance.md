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
