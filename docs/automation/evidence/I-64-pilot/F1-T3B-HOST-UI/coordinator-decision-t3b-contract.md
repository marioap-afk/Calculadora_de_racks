# I-64 — Decisión del Coordinator sobre el contrato de F1-T3B-HOST-UI

Transcripción literal recibida por el chat de la sesión responsable el 2026-10-11, en respuesta al STOP
COORDINATOR_T3B_GATE_CONTRACT_REQUIRED (paquete docs/initiatives/I-64-f1-t3b-gate-contract-package.md en 32231d90, CI 38122448571 4/4).

```text
I-64 — COORDINATOR DECISION
F1-T3B-HOST-UI
TARGET SHA:
32231d9090e46ca0942cc9c50b7166fb28270414
Current main:
bb0d5522e8411f66a51fdfb3f1f0d0514b737453
AUTHORIZED.
Reopen exactly:
src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs
for the minimum delta needed so RACKPANEL:

1. obtains the existing WorkspaceController;
2. calls OpenPanel();
3. creates/obtains the single WorkspacePaletteHost;
4. shows the palette.

Do not move PaletteSet logic into the command.
Do not add aliases.
Do not alter other commands.
REJECT the proposed 500 ms polling solution.
No DispatcherTimer.
No System.Threading.Timer.
No periodic polling.
No repeated GetSnapshot just because time elapsed.
Reason:
visible + idle must remain zero-work in the absence of a host/state event.
Use an EVENT-DRIVEN in-memory relay based on the existing AutoCAD
DocumentActivated subscription already owned by AutoCadEventBridge.
Reopen exactly:
src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs
src/RackCad.Plugin/Workspace/WorkspaceController.cs
for the deltas below.
AutoCadEventBridge.cs
Add one host-neutral in-memory notification:
ActiveSessionChanged
or an equivalently named Action/event with no AutoCAD type in its payload.
It may be raised only from existing document lifecycle handlers after the
existing core state has been updated:

* DocumentActivated;
* DocumentToBeDestroyed when active-session state changes.

Do NOT add any new AutoCAD += / -= subscription.
Do NOT modify EventBridgeCore for this purpose.
Do NOT enqueue reconciliation automatically inside EventBridgeCore
OnDocumentActivated.
AutoCadEventBridge remains the single owner of AutoCAD document events.
WorkspaceController.cs
May subscribe to exactly these bridge-internal notifications:

* Drained;
* ActiveSessionChanged.

No direct subscription to AutoCAD.
On ActiveSessionChanged:

* if PanelVisible:
call the existing _host.RefreshPanel();
* notify presentation state changed so the visual host can pull a fresh
snapshot through IWorkspaceHostPort.

On Drained:

* preserve existing _host.OnDrained(outcome);
* after successful containment, notify presentation state changed.

OpenPanel / ShowPanel / HidePanel may also notify presentation state changed
after their existing state transition.
Expose one in-memory presentation notification, e.g.:
StateChanged
No AutoCAD type in its event contract.
No persistence.
No background thread.
No timer.
The visual host may subscribe to StateChanged and then pull:
controller.Port.GetSnapshot()
and, when needed:
controller.Port.GetMeasurementReport().
When the panel is hidden:

* an active-document notification must NOT request reconciliation;
* no DWG read occurs;
* no timer/polling exists.

When the panel is visible:

* document activation is a real event and may request one reconciliation;
* after that reconciliation completes, idle returns to zero work until a
new hint/state event occurs.

This satisfies D-03 without violating P-02 / INV-33.
AUTHORIZED.
WorkspaceHostSourceGuardTests.cs may be modified only for exact,
semantically narrow refinements:
G1:
allow PaletteSet / AddVisual / required public WPF host types only in:
src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs
Continue to forbid there:

* Autodesk.AutoCAD.Internal;
* Dispatcher-based polling;
* Timer;
* async/background work;
* direct DWG read/write.

G2:
fix NothingOutsideThePluginWorkspaceReachesTheController so matching
WorkspaceHostPort is whole-token/type aware rather than an accidental
substring match.
This must permit RackCad.UI to reference IWorkspaceHostPort without opening
access to WorkspaceController or Plugin types.
G3:
update the controller subscription guard narrowly:
allowed internal subscriptions in WorkspaceController.cs:
_bridge.Drained += ...
_bridge.ActiveSessionChanged += ...
No direct AutoCAD event subscriptions.
No directory-wide exclusions.
No change to IWorkspaceHostPort.
UI presentation may consume:
IWorkspaceHostPort.GetMeasurementReport()
through the injected port.
Instrumentation toggle must be injected by the Plugin visual host as a
host-neutral delegate/callback.
RackCad.UI must not reference WorkspaceController or RackCad.Plugin.
No durable logging.
No RackLog change.
No periodic measurement polling.
Measurements are refreshed only on state notification or explicit UI
interaction.
Use only public host/WPF mechanisms.
PaletteSet KeepFocus or an equivalent public API is allowed where available.
Do NOT use:
Autodesk.AutoCAD.Internal
SetFocusToDwgView
reflection into AutoCAD internals.
Static/source verification is sufficient during T3B.
Actual keyboard/focus behavior remains a Smoke-1 obligation.
Use:
Palette title:
RackCad
Visual/root workspace label:
Workspace
This is not a new branding decision and does not freeze future copy.
No additional Owner decision is required for T3B implementation.
Put the rule explicitly in the T3B contract.
For synthetic nc3 only:
Expected oracle:
FreeText = fail
FailureClass = FreeText
Classification = EXECUTION_REWORK_REQUIRED
Disposition = REWORK
If the Controller:

* correctly detects the injected FreeText violation;
* reports FailureClass FreeText;
* preserves the other expected verification relationships;
* but converts synthetic REWORK into STOP solely because it applies the
real task AttemptsRemaining / CorrectionsAuthorized / S-11 rules;

then classify that control output as:
INVALID_OUTPUT
not as a valid nc3 PASS.
Permit the normal maximum two INVALID_OUTPUT/transport retries.
If all permitted nc3 outputs are exhausted in the same failure mode:
Controller nc3 =
NOT PASSED / P-04
and A-6 may be used only if all of its existing preconditions are
satisfied.
Do not infer or extend this rule again by analogy in later tasks; include it
in each applicable gate contract or return to Coordinator.
Accept the proposed minimums.
UI:
project:
tests/RackCad.UI.Tests/RackCad.UI.Tests.csproj
filter:
FullyQualifiedName~RackCad.UI.Tests.Workspace
current selected:
0
MinSelected:
1
ExpectRed:
true
Core:
project:
tests/RackCad.Tests/RackCad.Tests.csproj
filter:
FullyQualifiedName~RackCad.Tests.Workspace
current selected:
486
MinSelected:
487
ExpectRed:
true
Both RED legs must:

* compile;
* fail by assertion;
* exercise real T3B behavior;
* contain no artificial failure.

GREEN must preserve the RED test chain.
Keep OUT of T3B:

* adding RACKPANEL to help/catalog documentation;
* wiring the modal-RackCad flag into classic commands;
* disposing the palette/controller from Plugin Terminate();
* navigation;
* tabs;
* drafts;
* authored writes;
* inventory;
* profile diff;
* Smoke-1.

These are not required for T3B product closure.
The emitted contract may allow:

1. 

src/RackCad.UI/Workspace/
new files only

2. 

tests/RackCad.UI.Tests/Workspace/
new files only

3. 

src/RackCad.Plugin/Workspace/WorkspacePaletteHost.cs
new exact file

4. 

src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs
existing exact file, minimum RACKPANEL -> palette-show delta only

5. 

src/RackCad.Plugin/Workspace/WorkspaceController.cs
existing exact file, StateChanged + ActiveSessionChanged relay only

6. 

src/RackCad.Plugin/Workspace/AutoCadEventBridge.cs
existing exact file, ActiveSessionChanged notification only

7. 

tests/RackCad.Tests/Workspace/WorkspacePaletteHostSourceGuardTests.cs
new exact file

8. 

tests/RackCad.Tests/Workspace/WorkspaceHostSourceGuardTests.cs
existing exact file, G1/G2/G3 only
Everything else remains forbidden unless the planning proves another exact
file is indispensable, in which case:
STOP Coordinator.
Do NOT reopen:
EventBridgeCore.cs
WorkspaceHostCore.cs
IWorkspaceHostPort.cs
WorkspaceHostPort.cs
or other RackCad.Application Workspace product.
The current T3A Application surface is sufficient.
RackCad.UI may consume only host-neutral Application contracts/value types.
No reference to:
RackCad.Plugin
Autodesk.AutoCAD.*
WorkspaceController
WorkspacePaletteHost
The Plugin creates the UI root and injects:

* IWorkspaceHostPort;
* host-neutral delegates needed for:
   * instrumentation toggle;
   * other purely visual lifecycle callbacks if required.

The UI root/view-model must not acquire host services statically.
WorkspacePaletteHost.cs owns:

* the one lazy PaletteSet per process;
* AddVisual of exactly one RackCad.UI root UserControl;
* show/hide/visibility synchronization;
* StateChanged subscription to WorkspaceController;
* pulling snapshots/reports from the existing port;
* containment of visual-host exceptions.

No toolID.
No Load handler.
No Save handler.
Close = hide.
No DWG transaction/read/write in this file.
No timer.
Every panel-originated UI callback crossing into Plugin-owned callbacks must
be contained.
No exception from:

* UI event;
* view-model callback;
* StateChanged callback;
* host refresh;
* instrumentation toggle callback

may escape into AutoCAD's message loop.
On contained failure:
UI state becomes Error.
Classic commands remain usable.
T3B remains:
Implementacion transversal a capas (corta)
Semantic effort:
Deep
RoutingEnforcement:
required
Candidate Worker:
claude-sonnet-5-5
subagent
high
Controller:
gpt-6-luna
high
read-only
Codex CLI 0.162.0-alpha.17.2
Freshly verify eligibility and installed runtime before delegation.
If the final derived scope becomes Long-horizon:
STOP Coordinator.
Per T3B TaskId:
Planning:
1 valid output

* max 2 transport / INVALID_OUTPUT retries

Work:
1 valid output

* max 2 transport / INVALID_OUTPUT retries

Verification:
1 valid output

* max 2 transport / INVALID_OUTPUT retries

Controls:
nc1/nc2/nc3
nc4
A-4/A-6 only when their existing preconditions apply.
Attempt:
3
AttemptsRemaining:
0
CorrectionsAuthorized:
false
Any valid Worker result requiring correction:
STOP S-11.
No attempt reset.
No max_attempt increase.
Version this Coordinator decision first.
That commit becomes the proposed T3B AuthorityRevision.
Wait for exact CI 4/4.
Fetch again.
MainSha is the current origin/main at contract issuance.
If main advances from bb0d5522:
STOP S-13 / apply 16.7.
Freshly inspect:
I-52
I-65
I-62
before issuing.
Current observed I-65 at this Coordinator review:
e0c1120bcda13f90b7c4c8ad861ddd702c1ac891
Do not assume it remains current.
After the decision commit and exact CI:

1. revise the NOT-EMITTED T3B package to incorporate these decisions;
2. emit and schema-validate the exact T3B gate contract;
3. invoke exactly ONE valid Controller Planning output;
4. evaluate A1-A8 and nc4 without short-circuit;
5. do NOT invoke Worker.

Stop at:
COORDINATOR_T3B_ACCEPTANCE_REQUIRED
Return:

* decision commit + exact CI;
* current HEAD/MainSha;
* fresh I-52/I-65/I-62 tips;
* T3B contract SHA-256;
* Planning RunId;
* delegation SHA-256;
* A1-A8;
* nc4;
* exact AllowedWriteScope;
* exact ForbiddenWriteScope;
* G1/G2/G3 guard deltas;
* exact event-driven refresh design;
* RequiredTests / RED plan;
* routing;
* budget;
* Attempt / AttemptsRemaining;
* deviations.

Do NOT invoke T3B Worker.
```
