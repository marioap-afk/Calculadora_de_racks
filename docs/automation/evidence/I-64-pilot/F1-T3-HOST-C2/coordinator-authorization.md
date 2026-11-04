# I-64 — Disposición del análisis de fallos de Smoke-1 y autorización de F1-T3-HOST-C2 (literal)

Recibida por el chat de la sesión responsable el 2026-11-04, tras la custodia del reintento de Smoke-1 (`88475713`, CI 41855702427
push 4/4) y la parada en COORDINATOR_SMOKE1_FAILURE_ANALYSIS_REQUIRED.

```text
I-64 — COORDINATOR SMOKE-1 FAILURE ANALYSIS DISPOSITION
Reviewed occurrence:
R20261103T175351Z-0a77
Custody commit:
88475713473cd56694975fa537b0c9a4a786aa66
CI:
41855702427
push
4/4 SUCCESS
Disposition:
FAILURE ANALYSIS ACCEPTED
SM-13:
PRODUCT_CORRECTION
SM-34:
PRODUCT_CORRECTION
SM-39:
PRODUCT_CORRECTION
SM-13 and SM-34 share one correction.
SM-39 is independent.
No procedure change is authorized.
No Architect review is required.
Reason:
the required behavior is already frozen by D-03 / D-04 / S1-06f / S1-07.
The correction can preserve the existing T2 rule that CommandWillStart and
EnteringQuiescentState queue no hint and attach no Idle source.
Authorize:
F1-T3-HOST-C2
with two narrowly scoped corrections.
C2-A — command-state presentation notification
Problem:
EventBridgeCore correctly changes its in-memory Quiescent state on
CommandWillStart / EnteringQuiescentState, but the visible Workspace is not
notified to pull a fresh snapshot.
Required behavior:
CommandWillStart:

* set Quiescent false as today;
* emit an in-memory presentation/state-change notice;
* visible Workspace refreshes its snapshot;
* CanReadDrawing false;
* CanWriteDrawing false;
* display "Hay un comando en curso."

EnteringQuiescentState:

* set Quiescent true as today;
* emit the corresponding in-memory presentation/state-change notice;
* panel may refresh to the current available state.

The notification MUST NOT:

* enqueue a hint;
* call RequestReconciliation;
* attach Idle;
* perform a drain;
* read the selection;
* read the DWG;
* write the DWG;
* change selection or view.

Preserve the existing T2 oracle:
SourcesThatAreNotHintsQueueNothingAndNeverAttachIdle
This test must remain green.
The presentation notification should carry no AutoCAD types and should be
internal to the Workspace host plumbing.
Expected outcome:
SM-13 and SM-34 both receive the correct command-active UI state.
C2-B — null DocumentActivated means no active document
Problem:
AutoCAD may raise DocumentActivated with e.Document == null, including when
the Start page becomes active.
Current code calls NativeId(e.Document), causing a contained
NullReferenceException and leaving the previous active Workspace session
visible.
Required behavior:
DocumentActivated with null Document:

* no exception;
* no RackLog ERROR;
* active Workspace session becomes none;
* existing document sessions remain alive;
* no session is destroyed merely because Start is active;
* ActiveSessionChanged / equivalent presentation notice is emitted;
* visible panel shows the existing "Sin documento" state.

When a real document is activated again:

* its existing/new session becomes active normally;
* no stale context from the previous active document remains.

Do NOT implement this as a silent:
if (e.Document == null) return;
because that preserves the stale-panel defect.
The pure core should have an explicit no-active-document transition or an
equivalent testable mechanism.
Prefer only:
src/RackCad.Application/Workspace/EventBridgeCore.cs
src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs
src/RackCad.Plugin/Workspace/WorkspaceController.cs
and narrowly related Workspace tests/guards.
Touch fewer files if possible.
Do not modify:

* WorkspacePaletteHost placement/C1 behavior except if a source guard needs
awareness of a new internal event;
* selection reader behavior;
* DWG persistence;
* classic RackCad commands;
* RACKEDITAR;
* profile/security code;
* T3C/T3D semantics.

Create failing tests first.
For C2-A prove:

1. CommandWillStart causes a presentation-state notification.
2. EnteringQuiescentState causes the complementary notification.
3. neither event queues a hint.
4. neither event attaches Idle.
5. neither event causes a drain.
6. no DWG/selection read is introduced.
7. existing
SourcesThatAreNotHintsQueueNothingAndNeverAttachIdle
remains green.
8. controller forwards the in-memory notification to StateChanged.

For C2-B prove:

1. DocumentActivated(null) produces no exception.
2. active session becomes null.
3. existing sessions are retained.
4. presentation/ActiveSessionChanged notification occurs.
5. activating an existing document afterward restores that session.
6. no new subscription is introduced.
7. no registry/profile/file write is introduced.

After GREEN require:

* focused Workspace Core tests PASS;
* focused Workspace/UI tests PASS;
* all T2/T3A/T3B/C1 guards PASS;
* Core Full PASS;
* UI Full PASS;
* Debug UI build PASS;
* Debug Plugin build PASS except known MSB3277;
* exact-SHA CI 4/4.

Historical results remain unchanged:
R20261103T175351Z-0a77 = NOT PASS
SM-13 = FAIL
SM-34 = FAIL
SM-39 = FAIL
Do not relabel them after the fix.
Candidate 09f2f1c3 becomes:
HISTORICAL_SMOKE1_NOT_PASS_CANDIDATE
Do not reuse it for another Smoke.
After C2 PASS, establish a new Candidate with:

* exact Candidate SHA;
* exact DLL SHA-256;
* local Full evidence;
* exact-SHA CI;
* fresh host authorization.

Prepare a delta close package showing:

* RED SHA;
* GREEN SHA;
* exact files changed;
* tests introduced;
* confirmation T2 non-hint/no-Idle rule remains intact;
* confirmation no DWG reads/writes were introduced;
* confirmation null activation preserves sessions but clears active session;
* local verification;
* exact CI.

Stop at:
COORDINATOR_T3_C2_CLOSE_REQUIRED
Do NOT run AutoCAD.
Do NOT run another Smoke-1 yet.
Do NOT merge/rebase main yet.
Do NOT start F2.
```
