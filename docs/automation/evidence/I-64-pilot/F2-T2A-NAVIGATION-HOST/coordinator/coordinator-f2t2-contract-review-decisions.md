I-64 — F2-T2 CONTRACT REVIEW
COORDINATOR DECISIONS C-F2T2-01..13
Current branch:
09a1499a46d16c8251ac4575d370b551603b6372
Exact-SHA CI:
42659834369
4 / 4 SUCCESS
F2-T1:
CLOSED / PASS
F2:
IN PROGRESS
DECISION:
ACCEPT SPLIT
F2-T2 remains one lifecycle gate, but implementation is decomposed into:
F2-T2A-NAVIGATION-HOST
then
F2-T2B-NAVIGATION-UI
They are sequential.
T2B cannot start until T2A is:
EXECUTION_VERIFIED
and
Coordinator-closed for its delegated task.
Routing for both:
Deep
Worker:
anthropic/subagent/claude-sonnet-5-5
high
write / commit / push
Controller:
openai/process/gpt-6-luna
high
read-only
RoutingEnforcement:
required
No Frontier cell is required after this decomposition unless planning shows
either task is still Long-horizon.
If either split task evaluates Long-horizon:
STOP F2T2-C07.
DECISION:
USE PUBLIC:
DocumentCollection.ExecuteInCommandContextAsync
in exactly:
src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs
IMPORTANT CHANGE FROM PACKAGE RECOMMENDATION:
Do NOT impose "without async/await".
A controlled async exception is AUTHORIZED in this exact file.
Reason:
ExecuteInCommandContextAsync is intrinsically asynchronous.
The implementation must NOT compensate for a blanket async prohibition by:

* .Wait();
* .Result;
* GetAwaiter().GetResult();
* blocking the AutoCAD UI thread;
* ignoring an unobserved returned Task.

Authorized async surface:
WorkspaceNavigationHost.cs only.
Allowed there:
async
await
ExecuteInCommandContextAsync
Still forbidden:
Task.Run
Task.Factory.StartNew
Thread
Timer
background worker
fire-and-forget unobserved Task
ConfigureAwait(false) on the command-context path
The AutoCAD command-context callback must perform:

1. active-document validation;
2. SessionId validation;
3. target existence validation;
4. target attribution validation;
5. NavigationActionPolicy.Decide;
6. effects;
7. result capture;
8. focus return.

All AutoCAD reads/effects for the action occur inside the command context.
No hidden CommandMethod.
No SendStringToExecute.
No new AutoCAD command.
The exact-file source guard must encode this exception.
DECISION:
PUBLIC API ONLY FOR T2A
Implement focus return behind one isolated method in:
WorkspaceNavigationHost.cs
Use only supported/public AutoCAD/window API.
No:
Autodesk.AutoCAD.Internal
DllImport
Win32 focus hacks
HwndSource hacks
in T2A.
The implementation must degrade safely:
focus failure must not cause a DWG mutation or second navigation action.
Correctness is ultimately demonstrated by:
Smoke-NAV SN-07.
If public APIs compile but SN-07 shows that focus does not actually return to
the drawing:
STOP after Smoke-NAV evidence.
Do not silently introduce Autodesk.AutoCAD.Internal.
A separate Coordinator decision would then be required.
DECISION:
ACCEPT NEW ADAPTER
Create:
src/RackCad.Plugin/Workspace/WorkspaceNavigationFacts.cs
It implements:
INavigationFactsPort
It may:

* consume RackBlockFinder.ScanEnvelopes without changing RackBlockFinder;
* inspect direct top-level Model Space block references;
* obtain the handles required by NavigationFacts;
* classify only according to the existing F2-T1 authority contract.

It must NOT:

* create another NavigationTargets implementation;
* perform global inventory;
* change RackBlockFinder;
* change RackInventarioCommands;
* persist handles;
* retain AutoCAD DBObjects in fields.

Exact-file exceptions for required read APIs are authorized.
All transactions remain scoped with using.
DECISION:
CREATE NEW PORT
Create:
src/RackCad.Application/Workspace/IWorkspaceNavigationPort.cs
Do NOT modify:
IWorkspaceHostPort
WorkspaceHostPort
The new port contains no AutoCAD types.
Because execution is asynchronous by design, the port MAY expose a .NET-only
asynchronous contract, e.g.:
Task<WorkspaceNavigationResult>
or an equivalent immutable result contract.
The exact API is for planning to propose.
Requirements:

* request contains SessionId;
* request contains RackId/context required by the policy;
* action kind is explicit;
* view/copy input is explicit when applicable;
* result is immutable;
* result distinguishes executed / unavailable / rejected / stale / failed;
* no exception is required as normal UI control flow.

Do not add an event bus merely to return the result.
DECISION:
ACCEPT PURE PER-SESSION CONTENT ECHO
Implement:
src/RackCad.Application/Workspace/NavigationEcho.cs
The echo memory is keyed by SessionId.
For SELECT:
store the set of handles written by RackCad.
During the normal drain:
compare the current implied-selection handle set as a SET.
If equal:
the selection event is recognized as navigation echo.
It must not:

* advance context;
* change rack;
* change tab;
* trigger another navigation action.

The first different selection resumes ordinary synchronization.
No:
next-event token
boolean "ignoreNextSelection"
time window
timer
is allowed.
Echo state must be cleared when the session is no longer live.
DECISION:
ACCEPT LAZY PURGE
Do not expand the bridge's public notice surface.
At:

* each navigation-related drain;
* immediately before navigation execution;

reconcile navigation-owned session state against the bridge/session
authority.
If TryGetSession(SessionId) fails:
purge:

* NavigationTargets cache for that session;
* NavigationEcho state;
* pending navigation state owned by the host.

A purged SessionId must never be revived by stale UI state.
No new bridge event is authorized.
DECISION:
ACCEPT EXACT-FILE EXCEPTIONS ONLY
No directory-wide exception.
Every new privileged Plugin/Workspace file must be explicitly named in:
WorkspaceSourceGuardExceptionTests.cs
The RED commit must introduce:

* the required new failing behavior tests;
* the exact guard deltas needed to make the intended architecture possible.

Guard changes are part of RED, not an untracked GREEN relaxation.
Update exact meta-counts deliberately.
Do not weaken a guard using:
contains path prefix
wildcard folder
"all navigation files"
or equivalent broad wording.
The async exception from C-F2T2-02 is ONLY for:
WorkspaceNavigationHost.cs
and ONLY for the approved command-context pattern.
DECISION:
ACCEPT
T2A:
Core:
FullyQualifiedName~RackCad.Tests.Workspace
baseline:
656
UI:
FullyQualifiedName~RackCad.UI.Tests.Workspace
baseline:
64
T2B:
same namespace-prefix strategy.
These filters are preferred specifically because the namespace prefix lies
in the class identity and therefore class-only and FQN selection produce the
same selected universe for the control protocol.
Do not revert to:
FullyQualifiedName~Workspace
for these delegated tasks.
DECISION:
ACCEPT WITH PRODUCT CLARIFICATION
T2B must not assume RackCad is a permanently narrow sidebar.
Navigation controls must work when the Workspace is:

* docked;
* floating;
* narrow;
* wide;
* placed on a secondary monitor.

For F2-T2 this does NOT authorize a general redesign.
Specifically:

* no fixed maximum width for navigation UI;
* no layout whose correctness depends on 320 px;
* controls must stretch with available width;
* no screen-coordinate assumptions tying the UI to AutoCAD's main monitor.

Existing host initial/minimum sizes may remain unchanged in F2.
Full responsive system-editor redesign is later work.
DECISION:
ACCEPT
T2A and T2B are separate delegated control chains.
For each:
Attempt = 1
AttemptsRemaining = 2
MaxReworkLoops = 3
CorrectionsAuthorized = true
nc1..nc4 per README.
nc4 required for first delegation of each chain.
A-4 and A-6 remain usable only under their exact accepted conditions.
A-11:
NOT AVAILABLE
Its single F2-T1 application is consumed.
If a new Tests-control protocol defect exhausts nc3:
STOP P-04.
No bridge is inferred.
DECISION:
ACCEPT SEPARATE DOCUMENTATION TASK
After:
T2A CLOSED
and
T2B CLOSED
but before actual Smoke-NAV:
prepare one documentation-only task equivalent in role to F1-T3D.
It defines exact operator steps and evidence for SN-01..SN-10.
No AutoCAD execution is authorized merely by completing that document.
Smoke-NAV requires its own milestone authorization.
DECISION:
AUTHORIZED THROUGH CONTRACT ACCEPTANCE ONLY
Now prepare and issue:
F2-T2A-NAVIGATION-HOST
gate contract.
Then:

1. relay 16.4 preconditions;
2. Controller planning;
3. validate planning output;
4. A1-A8;
5. nc4;
6. STOP at:

COORDINATOR_F2_T2A_ACCEPTANCE_REQUIRED
Do NOT invoke the Worker automatically.
The Coordinator will explicitly accept or reject the final T2A delegation
after reviewing:

* planning;
* A1-A8;
* nc4;
* exact AllowedWriteScope;
* exact guard exceptions;
* async/command-context contract.

The proposed T2A scope is accepted as the basis for planning:
NEW:
src/RackCad.Application/Workspace/IWorkspaceNavigationPort.cs
src/RackCad.Application/Workspace/NavigationEcho.cs
src/RackCad.Plugin/Workspace/WorkspaceNavigationHost.cs
src/RackCad.Plugin/Workspace/WorkspaceNavigationFacts.cs
src/RackCad.Plugin/Workspace/WorkspaceNavigationEffects.cs
tests/RackCad.Tests/Workspace/WorkspaceNavigationHostTests.cs
tests/RackCad.Tests/Workspace/WorkspaceNavigationEchoTests.cs
EXACT DELTAS MAY BE PROPOSED IN:
src/RackCad.Plugin/Workspace/WorkspaceController.cs
src/RackCad.Plugin/Workspace/WorkspaceSelectionReader.cs
src/RackCad.Application/Workspace/WorkspaceHostCore.cs
and exact existing Workspace guard-test files needed by the accepted
exceptions.
Planning must enumerate the final existing-file list explicitly.
No generic tests/RackCad.Tests/Workspace/ write prefix is sufficient for
existing files.
New test files may use the declared test prefix.
Remain forbidden:
NavigationFacts.cs
NavigationTargetSet.cs
NavigationTargets.cs
NavigationActions.cs
RackBlockFinder.cs
RackInventarioCommands.cs
EventBridgeCore.cs
AutoCadEventBridge.cs
WorkspaceHostPort.cs
IWorkspaceHostPort.cs
WorkspacePanelCommand.cs
Domain
StructuralAnalysis
Plugin outside Workspace
Application outside Workspace except exact allowed files
docs
.github
eng
tools
assets
deploy
solution/build files
No F3/F4/F6.
No legacy-editor migration.
No new rack system.
No global inventory.
Add to the contract:
The only authorized asynchronous execution is the public AutoCAD
ExecuteInCommandContextAsync path in WorkspaceNavigationHost.cs.
Async is an execution-context mechanism, not background work.
The command-context callback must contain the decisive validation and
effects.
No navigation action may execute after:

* SessionId becomes stale;
* active Document changes;
* target disappears;
* target attribution changes.

No blocking wait on Task is permitted.
No detached/unobserved Task is permitted.
F0:
CLOSED
F1:
PASS
Smoke-1:
PASS
Post-I65:
PASS
F2-T1:
CLOSED / PASS
F2-T2:
SPLIT ACCEPTED
F2-T2A:
AUTHORIZED TO ISSUE CONTRACT AND PLAN
NOT AUTHORIZED TO IMPLEMENT
F2-T2B:
WAITING FOR T2A CLOSE
Smoke-NAV:
PENDING
F3-F7:
PENDING
attempts:
1 / 3
