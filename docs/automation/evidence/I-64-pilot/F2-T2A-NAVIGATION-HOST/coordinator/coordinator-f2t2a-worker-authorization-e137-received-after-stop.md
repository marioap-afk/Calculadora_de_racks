I-64 — F2-T2A-NAVIGATION-HOST
COORDINATOR ACCEPTANCE / WORKER AUTHORIZATION
Planning:
R20261109T171357Z-e137
Delegation:
ACCEPTED
A1-A8:
8 / 8 PASS
nc4:
R20261109T171828Z-fd2a
RELATIVE ORACLE MET
REJECTED_BEFORE_INVOCATION
P-03 confined to nc4
Worker:
claude-sonnet-5-5 / high
Authorization:
ONE WORKER INVOCATION
Attempt:
1 / 3
Use exactly the AllowedWriteScope emitted by e137.
Do NOT add:

* WorkspaceHostC2SourceGuardTests.cs
* WorkspaceBridgeFacadeSourceGuardTests.cs
* WorkspaceSelectionRead.cs
* any other existing file not in the accepted delegation.

If another existing file is required:
STOP P-03.
WorkspaceNavigationEffects.cs MAY open the navigation target with GetObject
or equivalent scoped AutoCAD DB read for the minimum information required
to execute:
VIEW
CENTER
Authorized reads include:

* target existence;
* target/reference type;
* geometric extents;
* geometry strictly required for the approved view operation.

The target must already have been selected by:
NavigationTargets / NavigationActionPolicy.
This exception does NOT authorize:

* Model Space enumeration;
* sibling discovery;
* target discovery;
* rack inventory;
* authored rack-data discovery;
* alternative-reference search.

WorkspaceNavigationFacts.cs remains the navigation-facts discovery adapter.
Effects may read only the target they were instructed to act upon.
All transactions remain scoped in using blocks.
Use:
DocumentCollection.ExecuteInCommandContextAsync
through:
WorkspaceNavigationHost.cs
The approved async exception remains limited to that file and that command-
context path.
Still prohibited:
.Wait()
.Result
GetAwaiter().GetResult()
Task.Run
Task.Factory.StartNew
Thread
Timer
SendStringToExecute
hidden CommandMethod
unobserved fire-and-forget Task
Revalidation and effects happen inside the command-context callback.
RED must compile.
RED must fail through intended behavioral/source-guard assertions.
Do not use compilation failure as RED.
Guard exceptions and their meta-tests are part of RED.
Implement only F2-T2A.
No T2B UI controls.
No F3 tabs.
No F4 drafts/editing.
No responsive redesign.
No new commands.
No legacy editor migration.
Core:
FullyQualifiedName~RackCad.Tests.Workspace
minimum 656
ExpectRed = true
UI:
FullyQualifiedName~RackCad.UI.Tests.Workspace
minimum 64
ExpectRed = false
Do not substitute the broad ~Workspace filter.
Worker completion must provide:

* BaseSha
* RedSha
* CurrentSha / GREEN SHA
* exact changed paths
* RED counts
* GREEN counts
* CI runs
* guard delta summary
* routing/model evidence
* known limitations

No gate terms in Worker free text.
STOP after this Worker invocation.
Do not invoke a second Worker automatically.
Then proceed through the normal Controller verification chain.
F2-T2B remains WAITING until T2A is verified and Coordinator-closed.
Current status:
F2-T1 = CLOSED / PASS
F2-T2A = WORKER AUTHORIZED
F2-T2B = WAITING
Smoke-NAV = PENDING
attempts = 1 / 3
