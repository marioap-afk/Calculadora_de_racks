# I-64 F1-T3A-HOST-CORE — Corrección de alcance del Coordinator (STOP previo al Worker aceptado)

Transcripción literal recibida por el chat de la sesión responsable el 2026-10-11, tras la parada previa al Worker sobre la
planificación R20261011T043044Z-ed7b (artifacts/.../R20261011T043044Z-ed7b/pre-worker-stop.json).

```text
I-64 — COORDINATOR SCOPE CORRECTION
F1-T3A-HOST-CORE
Current accepted base:
42d45776c695ed32987c08213532a9f75edb4a9a
MainSha remains:
bb0d5522e8411f66a51fdfb3f1f0d0514b737453
The pre-Worker STOP is ACCEPTED.
The Worker was never invoked.
Therefore:
Work budget consumed = 0
This is NOT Worker REWORK.
This is a CONTRACT / PLANNING SCOPE DEFECT.
Planning R20261011T043044Z-ed7b remains valid historical evidence but is:
SUPERSEDED_BY_SCOPE_CORRECTION
Do not relabel it INVALID_OUTPUT.
Do not erase its A1-A8 or nc4 results.
The accepted contract incorrectly assumed that the existing T1/T2 public
surface was sufficient for T3A.
It is not.
EventBridgeCore owns:

* ActiveSession;
* TryGetSession;
* WorkspaceSessionRegistry.Authorize;

but AutoCadEventBridge encapsulates the core and exposes none of those
session/request capabilities.
Also, panel startup/show requires an explicit reconciliation request; the
existing Start / document-activation behavior does not enqueue one.
Creating a second core or registry is forbidden.
Add exactly these two PRE-EXISTING product files to AllowedWriteScope:
src/RackCad.Application/Workspace/EventBridgeCore.cs
src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs
They are removed from ForbiddenWriteScope only for the exact deltas below.
No other existing T1/T2 product file is released.
Permitted additions only:

1. a public Authorize facade with semantics equivalent to:

RequestDecision Authorize(
WorkspaceRequest request,
long executingNativeInstanceId)
delegating directly to:
WorkspaceSessionRegistry.Authorize(...)

2. a public RequestReconciliation() operation.

RequestReconciliation must:

* operate only on the current active session;
* no-op if there is no active session;
* no-op if that session is degraded or retired;
* enqueue HintKind.ImpliedSelectionChanged on that active session;
* synchronize Idle using the existing bridge policy;
* never drain synchronously;
* never read the DWG;
* never write the DWG;
* never ApplyContext directly;
* never create another session;
* never add an event subscription;
* never persist anything.

ActiveSession and TryGetSession already exist.
Do not redesign them.
Do not expose the WorkspaceSessionRegistry itself.
Do not expose mutable internal collections/state.
Permitted additions only:
thin facade members for:

* ActiveSession;
* TryGetSession(...);
* Authorize(...);
* RequestReconciliation().

They must delegate to the existing single EventBridgeCore.
Do NOT:

* create a second EventBridgeCore;
* create a second WorkspaceSessionRegistry;
* add subscriptions;
* add event handlers;
* alter Hook(...) ownership;
* alter ordinary event-handler semantics;
* read the DWG in this bridge;
* write selection/view;
* change OnIdle semantics;
* change BeginDocumentClose behavior;
* add persistence;
* use Autodesk.AutoCAD.Internal.

Do NOT change Start() or OnDocumentActivated() to automatically enqueue
initial reconciliation as part of T2 semantics.
T3A owns the decision to request reconciliation.
The T3A controller must call RequestReconciliation when panel lifecycle
requires an initial/refresh reconciliation.
This preserves:
T2 = event/scheduling mechanism
T3 = panel lifecycle policy
The replacement T3A contract must add explicit RED/GREEN coverage for:

1. EventBridgeCore.RequestReconciliation with active synchronized session:
   * queues exactly the intended reconciliation hint;
   * causes Idle ownership to become pending as required;
   * does not drain synchronously.
2. no active session:
   * no-op;
   * no exception;
   * no Idle attachment.
3. degraded/retired session:
   * no-op;
   * no resurrection;
   * no new synchronization subscription.
4. Authorize facade:
   * identical result to WorkspaceSessionRegistry semantics;
   * correct session accepted;
   * wrong executing document rejected;
   * destroyed origin rejected.
5. AutoCadEventBridge facade source guard:
   * no second core;
   * no second registry;
   * no new += / -= ownership;
   * no DWG read/write introduced into the bridge.

All pre-existing T2 tests must remain green.
Authorize exactly ONE replacement valid Planning output.
Reason:
the first valid planning output faithfully followed a contract later proven
non-implementable before Worker invocation.
This authorization:

* does NOT reset attempts;
* does NOT reset Work budget;
* does NOT reset Verification budget;
* does NOT grant a correction Worker;
* does NOT modify max_attempts;
* applies only to T3A planning after this exact scope correction.

Planning budget becomes:
original valid planning:
1 consumed / historical / SUPERSEDED_BY_SCOPE_CORRECTION
replacement valid planning:
1 authorized
plus up to two transport / INVALID_OUTPUT retries for the replacement
planning only.
After one valid replacement planning output:
Planning is exhausted again.
Version this Coordinator scope-correction decision first.
That new commit Y becomes the new:
AuthorityRevision = Y
The prior 42d45776 remains historical authority for the decomposition and
initial contract.
Do not rewrite it.
Wait for exact CI 4/4 on Y.
Then fetch again.
If origin/main changed:
STOP S-13 / apply 16.7.
Freshly inspect I-65 before replacement planning.
Re-emit F1-T3A-HOST-CORE gate contract with:

* the two released existing files;
* the exact delta restrictions above;
* revised RequiredTests;
* revised RED plan;
* unchanged task objective otherwise;
* unchanged Worker routing;
* unchanged Attempt 3;
* unchanged AttemptsRemaining 0;
* unchanged CorrectionsAuthorized false;
* unchanged T3B/T3C exclusions.

Validate rackcad-gate-contract/v1.
Invoke exactly ONE valid replacement Controller planning:
gpt-6-luna
high
read-only
Codex CLI 0.162.0-alpha.17.2
Run A1-A8 and nc4 again against the replacement contract.
Do not reuse the previous A1-A8 as authority for the changed scope.
Do not invoke Worker.
Stop at:
COORDINATOR_T3A_REPLACEMENT_ACCEPTANCE_REQUIRED
Return:

* scope-correction decision commit + exact CI;
* current MainSha;
* replacement contract SHA-256;
* replacement Planning RunId;
* replacement delegation SHA-256;
* A1-A8;
* nc4;
* revised AllowedWriteScope;
* revised ForbiddenWriteScope;
* exact delta restrictions on EventBridgeCore.cs;
* exact delta restrictions on AutoCadEventBridge.cs;
* revised RED plan;
* Worker routing;
* Attempt / AttemptsRemaining;
* confirmation Work budget remains 0/1;
* confirmation no Worker was invoked.

Do NOT invoke the Worker before the replacement acceptance.
```
