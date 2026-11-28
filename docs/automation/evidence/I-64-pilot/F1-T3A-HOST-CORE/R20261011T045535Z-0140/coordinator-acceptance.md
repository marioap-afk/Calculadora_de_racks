# I-64 F1-T3A-HOST-CORE — Aceptación formal del Coordinator del contrato de reemplazo (planificación R20261011T045535Z-0140)

Transcripción literal recibida por el chat de la sesión responsable el 2026-10-11, tras la parada en
COORDINATOR_T3A_REPLACEMENT_ACCEPTANCE_REQUIRED.

```text
I-64 — COORDINATOR ACCEPTANCE
F1-T3A-HOST-CORE — REPLACEMENT CONTRACT
AuthorityRevision:
ee6766684f560c5128f0cc6af9c5d091de6e8353
MainSha:
bb0d5522e8411f66a51fdfb3f1f0d0514b737453
Replacement contract SHA-256:
2616AC79E4DE1A4872BFCC9796DEB375DC17ABB60AFB3C5CC9F84CAB77DF3D74
Replacement Planning RunId:
R20261011T045535Z-0140
Replacement delegation SHA-256:
1D61B5FFCFD6EF3508C23E51D84100F6BAADCCF5636A63790C55903BC70A45D1
A1 = PASS
A2 = PASS
A3 = PASS
A4 = PASS
A5 = PASS
A6 = PASS
A7 = PASS
A8 = PASS
nc4 = PASS
Disposition:
ACCEPTED
Preserve:
R20261011T043044Z-ed7b
as:
VALID / HISTORICAL /
SUPERSEDED_BY_SCOPE_CORRECTION
Do not relabel it INVALID_OUTPUT.
The replacement planning R20261011T045535Z-0140 is now the operative T3A
planning authority.
Planning budget is exhausted.
Invoke exactly ONE valid Worker execution.
Worker:
claude-sonnet-5-5
subagent
high
Task:
F1-T3A-HOST-CORE
Semantic effort:
Deep
RoutingEnforcement:
required
Attempt:
3
AttemptsRemaining:
0
CorrectionsAuthorized:
false
Work budget before invocation:
0 / 1
After one valid Worker result:
1 / 1
Any valid result requiring correction:
STOP S-11.
Do not launch a correction Worker.
Immediately before Worker invocation require:
HEAD =
origin/architecture/workspace-persistente-rackcad =
ee6766684f560c5128f0cc6af9c5d091de6e8353
origin/main =
bb0d5522e8411f66a51fdfb3f1f0d0514b737453
config.toml =
accepted A/B/C hash
with exact known ordered 105-name inventory.
Freshly inspect I-65.
Current observed I-65 tip at Coordinator review:
ff0c57c7125ae268bcb42b2d56c0d5255e9a69ed
If it changes again, re-check overlap.
If main moves:
STOP S-13.
If product/hot-file overlap appears:
STOP ownership conflict.
The following two pre-existing T2 files are released ONLY for the exact
authorized deltas:
src/RackCad.Application/Workspace/EventBridgeCore.cs
src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs
No other pre-existing T1/T2 product file is released.
Only add the minimum public surface required by the accepted contract:
Authorize(
WorkspaceRequest request,
long executingNativeInstanceId)
which delegates directly to the existing registry authorization semantics.
And:
RequestReconciliation()
with exactly these semantics:

* current active session only;
* no-op with no active session;
* no-op when degraded/retired;
* enqueue only HintKind.ImpliedSelectionChanged;
* synchronize existing Idle policy;
* no synchronous drain;
* no DWG read;
* no DWG write;
* no ApplyContext;
* no session creation;
* no subscription creation;
* no persistence.

Do not expose WorkspaceSessionRegistry.
Do not expose mutable internal state.
Do not change Start() or OnDocumentActivated() reconciliation behavior.
Only add thin facades for:

* ActiveSession;
* TryGetSession(...);
* Authorize(...);
* RequestReconciliation().

All delegate to the existing single EventBridgeCore.
Do not:

* create a second core;
* create a second registry;
* add += or -=;
* alter Hook(...);
* alter OnIdle;
* alter Start;
* alter Dispose;
* alter BeginDocumentClose;
* add DWG reads;
* add DWG writes;
* add persistence;
* use Autodesk.AutoCAD.Internal.

Because the file does not enable nullable annotations:
do not introduce avoidable CS8632 warnings.
Use a signature/style compatible with the file's nullable context while
preserving the semantic contract.
A real behavioral RED remains mandatory.
Required Core filter:
FullyQualifiedName~RackCad.Tests.Workspace
MinSelected:
160
ExpectRed:
true
RED must compile and fail by assertion.
It must include coverage for both the original T3A behavior and the new
scope-correction behavior.
At minimum, new RED coverage must prove:

1. RequestReconciliation with active synchronized session:
   * queues reconciliation hint;
   * makes Idle pending as required;
   * does not synchronously drain.
2. No active session:
   * no-op;
   * no exception;
   * no Idle attachment.
3. Degraded/retired:
   * no-op;
   * no resurrection;
   * no new synchronization subscription.
4. Authorize:
   * correct session accepted;
   * wrong executing document rejected;
   * destroyed origin rejected;
   * semantics match WorkspaceSessionRegistry.
5. AutoCadEventBridge source guard:
   * one existing core only;
   * no second registry;
   * no new event ownership;
   * no DWG read/write introduced.

All pre-existing T2 tests remain green after GREEN.
Do not weaken RED tests during GREEN.
The prior exact-file exceptions remain accepted:
WorkspacePanelCommand.cs
WorkspaceSelectionReader.cs
WorkspaceBridgeSourceGuardTests.cs may be modified only to encode the exact
authorized exceptions.
Command censuses may change only:
37 -> 38
and add:
RACKPANEL
No alias.
Prior disposition remains unchanged:
MEMORY-ONLY.
Do not modify RackLog.cs.
Do not log normal metrics as Warning or Exception.
If durable logging becomes necessary:
STOP C-T3-INSTRUMENTATION.
Worker must produce:

1. RED commit;
2. push;
3. GREEN commit;
4. push;
5. valid worker-handoff;
6. terminate.

The RED and GREEN relationship must satisfy the accepted delegation and
16.8.
Do not touch T3B/T3C.
Do not run AutoCAD.
Wait for exact GREEN SHA CI.
Then invoke exactly ONE valid Controller verification:
gpt-6-luna
high
read-only
If:
EXECUTION_VERIFIED / NONE
continue to the applicable controls without Git writes.
If valid verification requires REWORK:
STOP S-11.
No correction Worker.
Use:
nc1
nc2
nc3
and the authorized I-64 substitute mechanisms A-4 / A-6 when their
preconditions apply.
nc4 replacement control is already PASS.
Preserve all historical outputs.
Do not rewrite Controller nc2/nc3 results.
If Worker + verification + controls close successfully:
custody the complete T3A chain,
create documentary close commit,
wait for exact CI,
and STOP at:
COORDINATOR_T3A_CLOSE_REVIEW_REQUIRED
Return:

* Worker RunId;
* RED SHA;
* GREEN SHA;
* exact changed files;
* RED selected/pass/fail;
* GREEN selected/pass/fail;
* full Core suite;
* Plugin Release result/warnings;
* Worker handoff;
* Controller verification RunId/result;
* nc1/nc2/nc3;
* applicable bridge results;
* documentary close commit;
* exact CI;
* resulting T3A status;
* current I-64 HEAD/main.

Do NOT start T3B.
Proceed with the single T3A Worker.
```
