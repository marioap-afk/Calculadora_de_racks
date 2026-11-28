# I-64 F1-T3B-HOST-UI — Aceptación formal del Coordinator (planificación R20261011T081209Z-deae)

Transcripción literal recibida por el chat de la sesión responsable el 2026-10-11, tras la parada en
COORDINATOR_T3B_ACCEPTANCE_REQUIRED.

```text
I-64 — COORDINATOR ACCEPTANCE
F1-T3B-HOST-UI
AuthorityRevision:
c7f389cbb2eff4be42f861d8114adc2af753a1c9
BaseSha:
f3a9f40e0d0057e14a6eb7a6681590b78cecf37f
MainSha:
bb0d5522e8411f66a51fdfb3f1f0d0514b737453
Contract SHA-256:
7807A9E5136DC6D7C84526FB3B175F935C20F85BD48DF1DEC04E4275372D2873
Planning RunId:
R20261011T081209Z-deae
Delegation SHA-256:
683CBE6F53DDF34EA4D2B6A1097D3949C6DE6B00CF5872EDBD6CC6555B068009
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
Invoke exactly ONE valid Worker execution.
Worker:
claude-sonnet-5-5
subagent
high
Task:
F1-T3B-HOST-UI
Class:
Implementacion transversal a capas (corta)
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
Any valid Worker result requiring correction:
STOP S-11.
Do not invoke a corrective Worker.
Immediately before Worker invocation require:
HEAD =
origin/architecture/workspace-persistente-rackcad =
f3a9f40e0d0057e14a6eb7a6681590b78cecf37f
origin/main =
bb0d5522e8411f66a51fdfb3f1f0d0514b737453
config.toml:
accepted A/B/C hash
with exact ordered 105-name inventory.
Freshly inspect:
I-52
I-65
I-62
Current I-65 observed by Coordinator:
e3ae36d0d6cb1f13de685ed0f9825d2b5a71ecf5
If any tip changes:
re-check overlap.
If main moves:
STOP S-13.
If a product/hot-file overlap appears:
STOP ownership conflict.
The T3B contract interpretation is:
AutoCadEventBridge.ActiveSessionChanged
MUST be:
internal
not public.
Reason:

* it is an internal bridge-to-controller notification;
* WorkspaceController is in the same Plugin assembly;
* the existing public-facade guard remains intact;
* no external consumer needs this event.

Do NOT modify:
WorkspaceBridgeFacadeSourceGuardTests.cs
for this event.
Do not enlarge AutoCadEventBridge's public surface.
Allowed exact delta:

* add internal argument-free ActiveSessionChanged event;
* raise it from the already-existing contained lifecycle handlers after the
core has updated state:
   * DocumentActivated;
   * DocumentToBeDestroyed only when active-session state changed.

No new AutoCAD subscription.
No new += / -= against AutoCAD.
No change to EventBridgeCore.
No change to Start reconciliation semantics.
No DWG read/write.
No timer.
Allowed exact behavior:
subscribe internally to:
_bridge.Drained
_bridge.ActiveSessionChanged
Expose one host-neutral presentation notification:
StateChanged
No AutoCAD types in the event payload.
On ActiveSessionChanged:

* if PanelVisible:
call _host.RefreshPanel();
* then raise StateChanged.

On successful Drained processing:

* preserve _host.OnDrained(outcome);
* raise StateChanged afterward.

OpenPanel / ShowPanel / HidePanel may raise StateChanged after the state
transition.
Do not subscribe directly to AutoCAD.
No timer/background worker.
Create exactly one process-lifetime PaletteSet lazily.
Own:

* PaletteSet("RackCad");
* AddVisual of exactly one RackCad.UI Workspace root UserControl;
* show/hide;
* visibility synchronization;
* controller.StateChanged subscription;
* snapshot/report pulls through controller.Port;
* UI-host callback containment;
* instrumentation-toggle delegate.

No toolID.
No Load/Save handlers.
No AutoCAD.Internal.
No DWG read/write.
No timer/polling.
Close means hide.
WorkspacePanelCommand.cs may be changed only so RACKPANEL:

1. gets WorkspaceController.GetOrCreate();
2. calls OpenPanel();
3. gets/creates WorkspacePaletteHost;
4. shows the palette.

Palette implementation stays out of the command.
No alias.
No command census changes.
New files only under:
src/RackCad.UI/Workspace/
UI may reference only Application host-neutral contracts/value types.
No:
RackCad.Plugin
Autodesk.AutoCAD.*
WorkspaceController
WorkspacePaletteHost
Use:
IWorkspaceHostPort
plus injected host-neutral callbacks/delegates.
Expected presentation includes:

* no-document empty state;
* active session/document state;
* NotStarted / Ready / Degraded / Error;
* action availability and reason;
* PICKFIRST state;
* five selection contexts;
* in-memory instrumentation presentation;
* instrumentation toggle.

Do not present selection context as project totals.
No App.xaml assumption.
Merge needed resources locally using existing RackCad.UI component URI
conventions.
Do not create application-global resources.
Public APIs only.
PaletteSet KeepFocus or equivalent public behavior is permitted.
Do NOT use:
Autodesk.AutoCAD.Internal
SetFocusToDwgView
reflection into internal APIs.
Static/source checks now.
Actual keyboard/focus behavior:
Smoke-1.
No panel-originated callback may escape into AutoCAD's message loop.
Contain:

* UI callbacks;
* StateChanged refresh;
* snapshot/report pull;
* instrumentation toggle;
* palette show/hide callbacks.

On failure:
UI enters Error state.
Classic RackCad commands remain usable.
WorkspaceHostSourceGuardTests.cs may change ONLY for:
G1:
exact-file allowance for WorkspacePaletteHost.cs to use:

* PaletteSet;
* AddVisual;
* Autodesk.AutoCAD.Windows;
* required public WPF host types;
* RackCad.UI root type.

Still forbidden there:

* Autodesk.AutoCAD.Internal;
* Dispatcher polling;
* timers;
* background/async work;
* DWG read/write.

G2:
whole-identifier/type-aware matching so IWorkspaceHostPort in RackCad.UI is
not falsely detected as WorkspaceHostPort access to Plugin internals.
Do not allow RackCad.UI access to:
WorkspaceController
WorkspaceHostCore
WorkspaceSelectionReader
RackCad.Plugin.
G3:
WorkspaceController may contain exactly the two internal bridge
subscriptions:
_bridge.Drained += ...
_bridge.ActiveSessionChanged += ...
No direct AutoCAD event subscription.
No -= requirement is added for process-lifetime controller in T3B.
T3B is a new TaskId.
ChainRedSha:
null
Real RED mandatory.
UI leg:
FullyQualifiedName~RackCad.UI.Tests.Workspace
MinSelected:
1
ExpectRed:
true
Core leg:
FullyQualifiedName~RackCad.Tests.Workspace
MinSelected:
487
ExpectRed:
true
Both must:

* compile;
* fail by assertion;
* exercise real behavior;
* contain no artificial failure.

Expected RED areas:
UI:

* empty/document/session states;
* five selection contexts;
* Error + retry;
* instrumentation presentation/toggle;
* local resources without App.xaml;
* no host query absent StateChanged/explicit UI interaction.

Core/source:

* PaletteSet single-owner structure;
* no polling/timer;
* internal ActiveSessionChanged;
* exact lifecycle relay;
* controller's two internal subscriptions;
* no public bridge-surface expansion;
* no AutoCAD.Internal;
* UI boundary.

GREEN must preserve RED tests.
No change to IWorkspaceHostPort.
UI reads existing:
GetMeasurementReport()
Toggle is an injected host-neutral callback.
No RackLog changes.
No normal metrics as Warning/Exception.
No durable logging.
Use the exact nc3 rule already written in the accepted T3B contract.
Do not reinterpret it by analogy.
If the Controller detects the synthetic FreeText violation correctly but
converts the expected REWORK into STOP solely from real-task attempt rules:
INVALID_OUTPUT
with normal retry rules.
If exhausted:
Controller nc3 =
NOT PASSED / P-04
A-6 only if all existing preconditions are satisfied.
Do not implement:

* help/catalog entry for RACKPANEL;
* modal flag wiring into classic commands;
* Plugin Terminate disposal;
* navigation;
* tabs;
* drafts;
* authored writes;
* inventory;
* profile diff;
* Smoke-1.

Do not run AutoCAD.
Worker must produce:

1. RED commit;
2. push;
3. GREEN commit;
4. push;
5. valid Worker handoff;
6. terminate.

Wait for exact GREEN CI.
Then invoke exactly ONE valid Controller verification:
gpt-6-luna
high
read-only
If verification returns:
EXECUTION_VERIFIED / NONE
continue to controls.
If a valid verification requires correction:
STOP S-11.
No corrective Worker.
Run applicable:
nc1
nc2
nc3
nc4 is already PASS.
Use A-4 / A-6 only under their existing authorized preconditions.
Do not rewrite historical Controller failures.
If Worker, verification and controls close successfully:
custody the full T3B chain,
create documentary close commit,
wait for exact CI,
then stop at:
COORDINATOR_T3B_CLOSE_REVIEW_REQUIRED
Return:

* Worker RunId;
* RED SHA;
* GREEN SHA;
* changed files;
* UI RED/GREEN counts;
* Core RED/GREEN counts;
* full Core suite;
* UI full suite;
* Plugin Release build/warnings;
* Worker handoff;
* Controller verification;
* nc1/nc2/nc3;
* bridge results if used;
* close commit;
* exact CI;
* T3B resulting status;
* current HEAD/main.

Do NOT start T3C.
Do NOT declare F1 PASS.
Proceed with the single T3B Worker.
```
