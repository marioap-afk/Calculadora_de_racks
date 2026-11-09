TaskId: F2-T2A-NAVIGATION-HOST
FailureClass: Ci
Disposition: REWORK
VerifiedSha:
null
ControllerVerificationRunId:
R20261109T184844Z-3f3e
WorkRunId:
R20261109T175557Z-a639
DelegationRunId:
R20261109T174847Z-5d33
Attempt closed:
1 (closed as REWORK by the Coordinator disposition ATTEMPT 2 REWORK AUTHORIZATION)

ATTEMPT 1 RESULT
The single Worker invocation ended with WorkerStatus BLOCKED and Disposition BLOCKED.
The RED commit 4fcea652d413414552d52f70998ce16f94f4c31b was pushed: 13 paths, all inside AllowedWriteScope.
No GREEN commit exists: RedSha = CurrentSha.
Controller verification: EXECUTION_REWORK_REQUIRED / REWORK, VerifiedSha null, no stop condition.
Checks in pass: Termination, Handoff, Authority, Contract, Identity, Remote, Scope, CleanTree, Trailer, Routing, FreeText, Denials.
Checks in fail: Ci and Tests, both with RedPart = pass.
The RED is accredited and accepted by the Coordinator as valid evidence.
No negative controls ran: the verification was not EXECUTION_VERIFIED.

FAILURE CLASS: Ci
CurrentSha is the RED commit, so its exact-SHA CI is red by construction.
Run 42673031307, attempt 2: Tests (Domain + Application) failure; Build UI success; UI Tests success; Build Plugin without AutoCAD skipped.
Core filter FullyQualifiedName~RackCad.Tests.Workspace: 755 selected, 709 passed, 46 failed.
UI filter FullyQualifiedName~RackCad.UI.Tests.Workspace: 64 selected, 64 passed.
Attempt 1 of that run ended with 0 jobs by a GitHub internal server error (Correlation ID 2c9d27d3-c894-417f-b3f2-c37a70fe3f2a).
The Coordinator classified it as CI transport / platform failure, not a RED, product or Worker failure.

ROOT CAUSE: impossible RED assertion
WorkspaceNavigationEchoTests.NothingChangedInTheSelectionReadOrTheSessionToCarryTheHandles asserts that the code of
src/RackCad.Application/Workspace/WorkspaceSession.cs does not match the regular expression Echo|Navigation.
That file, unchanged and out of scope, contains DrainOperation.InvalidateNavigationCaches at line 54.
The assertion tests a lexical coincidence rather than the invariant, and can never pass.
GREEN must not modify a RED test (16.8), so the Worker stopped as instructed.
The Worker left the implementation uncommitted; the Coordinator authorized restoring the tree to HEAD before verification.

EXACT CORRECTION
Attempt 2 must create a new RED, because the correction changes a ChainRedFile.
Correct only that assertion, in tests/RackCad.Tests/Workspace/WorkspaceNavigationEchoTests.cs, so that it:
* no longer rejects the unrelated existing symbol InvalidateNavigationCaches;
* proves the intended invariant directly:
  - WorkspaceSelectionRead.cs remains unchanged and carries no echo or handle transport;
  - WorkspaceSession.cs is not used to carry navigation echo handles;
  - no echo-specific state, property, field or transport is added to WorkspaceSession.cs;
  - the approved echo mechanism remains NavigationEcho plus the exact authorized reader and host-core integration.
The invariant must not be deleted. The test must still detect an echo transport added to WorkspaceSession.cs.
The corrected RED must fail for intended missing behavior and be capable of passing under the authorized GREEN.

SAVED PATCH PROVENANCE
artifacts/orchestration/I-64/F2-T2A-NAVIGATION-HOST/1/R20261109T175557Z-a639/green-uncommitted-worktree.patch
SHA-256 32C19A574CCF85E513309EF62116A29876EBFCDF8796579043A0B1209E44C046, 40138 bytes.
Produced by the Attempt 1 Worker against 4fcea652; byte-identical to git diff HEAD before the restoration.
It is historical Worker-produced implementation material. It is NOT accepted GREEN, NOT verified product and NOT session-owned.
It is excluded from versioning and identified by SHA-256 in the custody manifest of the Worker run.
The session does not apply it. The Attempt 2 Worker may inspect it, reuse valid portions, discard or reimplement others,
correct it, and remains responsible for the final implementation and commits.

F2T2A-KL-01 / ProbedId
Accepted known limitation, diagnostic only: the current integrated scan does not expose enough raw envelope identity to populate
NavigationDefinitionFact.ProbedId without crossing the accepted read-authority boundary.
Acceptable only while unreadable or probe-only members remain non-navigable, target attribution does not depend on ProbedId,
and INV-40 safety does not depend on ProbedId.
Read authority must not be expanded to populate it.
Tests must establish that ProbedId = null does not make an unreadable member navigable. No existing test covers that case:
the unreadable facts in WorkspaceNavigationTargetsTests.cs all carry a non-null ProbedId.
If Attempt 2 finds ProbedId is required for navigation correctness or revalidation: STOP.

CHAIN
ChainBaseSha: d9d386e91aa6df5db64fbb6946b7edb3542baace
ChainRedSha: 4fcea652d413414552d52f70998ce16f94f4c31b
ChainRedFiles:
* tests/RackCad.Tests/Workspace/WorkspaceBridgeSourceGuardTests.cs
* tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs
* tests/RackCad.Tests/Workspace/WorkspaceNavigationAuthorityTests.cs
* tests/RackCad.Tests/Workspace/WorkspaceNavigationEchoTests.cs
* tests/RackCad.Tests/Workspace/WorkspaceNavigationHostTests.cs
* tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs
* tests/RackCad.Tests/Workspace/WorkspaceSourceGuardExceptionTests.cs
The Attempt 2 delivery touches ChainRedFiles, so it requires a new RED.

SCOPE
Same gate contract, same AllowedWriteScope, ForbiddenWriteScope, Invariants, RequiredTests, ExpectedEvidence and StopConditions.
WorkspaceSelectionRead.cs, WorkspaceHostC2SourceGuardTests.cs and WorkspaceBridgeFacadeSourceGuardTests.cs remain out of scope: any need is STOP P-03.
No nc4 for Attempt 2: README section 10 limits it to the first delegation of the chain, satisfied by R20261109T175144Z-6a04.
No Attempt 3 is consumed automatically.
