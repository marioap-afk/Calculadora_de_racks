# I-64 F1-T3A-HOST-CORE — Aceptación formal del Coordinator (planificación R20261011T043044Z-ed7b)

Transcripción literal recibida por el chat de la sesión responsable el 2026-10-11, tras la parada en
COORDINATOR_T3A_ACCEPTANCE_REQUIRED.

```text
I-64 — COORDINATOR ACCEPTANCE
F1-T3A-HOST-CORE
AuthorityRevision:
42d45776c695ed32987c08213532a9f75edb4a9a
MainSha:
bb0d5522e8411f66a51fdfb3f1f0d0514b737453
Planning RunId:
R20261011T043044Z-ed7b
Delegation SHA-256:
07683DA70B1E10DE7C48F0EB11BD6312D7D93CE01038D75D930F086A5D4F8FFF
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
F1-T3A-HOST-CORE
Task class:
Implementacion transversal a capas (corta)
Semantic effort:
Deep
Attempt:
3
AttemptsRemaining:
0
CorrectionsAuthorized:
false
Any valid result requiring a corrective Worker:
STOP S-11.
Do not launch a correction Worker.
Transport / INVALID_OUTPUT retries remain governed separately.
Immediately before Worker invocation require:
HEAD =
origin/architecture/workspace-persistente-rackcad =
42d45776c695ed32987c08213532a9f75edb4a9a
origin/main =
bb0d5522e8411f66a51fdfb3f1f0d0514b737453
config.toml hash ∈ accepted A/B/C set
AND ordered key inventory = known 105-name inventory.
Freshly inspect I-65 remote tip.
If main moves:
STOP S-13.
If I-65 acquires a product/hot-file overlap:
STOP ownership conflict.
Implement the accepted delegation exactly.
T3A owns:

* pure IWorkspaceHostPort contract/policies;
* one lazy Workspace controller per process;
* RACKPANEL command adapter;
* lazy startup of the existing T2 bridge;
* active-session binding;
* SessionId authorization;
* fine-selection reader invoked only through drain;
* SelectionContext.Classify / ApplyContext path;
* visible/hidden and empty-state host policies;
* in-memory instrumentation;
* exact source-guard refinements;
* exact command-census changes.

No PaletteSet.
No WPF.
No T3B product.
No AutoCAD execution.
The accepted exact adapter files are:
src/RackCad.Plugin/Workspace/WorkspacePanelCommand.cs
src/RackCad.Plugin/Workspace/WorkspaceSelectionReader.cs
Do not rename these during implementation.
The T2 source guard may be refined ONLY for these exact files.
Only:
WorkspacePanelCommand.cs
may introduce:
[CommandMethod("RACKPANEL")]
No alias.
No existing command file changes.
The two accepted command-census files may change only to:

* add RACKPANEL;
* change the exact total 37 -> 38.

Nothing else in those census expectations changes.
Only:
WorkspaceSelectionReader.cs
may use the D-06 read path required for fine selection.
It may use only the read-side APIs actually necessary for:

* SelectImplied;
* read-only object retrieval;
* RackBlockData.Read;
* RackEmbedStore.Deserialize;
* short transaction / document-lock scope required by the Freeze.

It must NOT:

* SetImpliedSelection;
* modify selection;
* modify view;
* write DB objects;
* Commit a transaction;
* persist NOD/XData;
* enumerate the drawing globally;
* use Autodesk.AutoCAD.Internal;
* introduce an independent envelope codec;
* execute outside the drain path.

The existing T2 event handlers remain enqueue-only.
C-T3-INSTRUMENTATION is RESOLVED.
T3A instrumentation is MEMORY-ONLY.
Allowed:

* in-memory enable/disable toggle;
* Stopwatch/timing measurements;
* process-memory measurements;
* pure metric snapshots/state;
* measurement points;
* exposure through the T3A host abstraction for later presentation.

Forbidden:

* changing RackLog.cs;
* representing normal timing/memory data as Warning;
* representing normal timing/memory data as Exception;
* writing new log files;
* registry/profile/config persistence;
* introducing a new global logging authority.

If implementation discovers that T3A itself requires durable/logged timing
output:
STOP C-T3-INSTRUMENTATION.
Do not improvise a logging API.
F7 may later consolidate emission/persistence under its own authority.
This is a new TaskId.
ChainBaseSha:
42d45776c695ed32987c08213532a9f75edb4a9a
ChainRedSha:
null
ChainRedFiles:
[]
A real behavioral RED is mandatory.
The RED must:

* compile;
* select >= 1 Workspace Core test;
* fail by assertion;
* exercise frozen T3A behavior;
* not use an artificial failure;
* not require AutoCAD runtime.

Acceptable RED areas include:

* controller is lazy and unique;
* repeated RACKPANEL does not duplicate;
* wrong SessionId is rejected with no effect;
* port exposes no writes;
* hidden panel performs no reads;
* showing schedules one reconciliation;
* idle/no-hint state performs no work;
* no-document state;
* fine-selection classification with simulated reader.

GREEN must not invalidate the RED chain by modifying the RED tests in a way
forbidden by 16.8.
WorkspaceBridgeSourceGuardTests.cs may be modified only to implement:

1. exact-file allowance for:
WorkspacePanelCommand.cs
containing [CommandMethod("RACKPANEL")];
2. exact-file allowance for:
WorkspaceSelectionReader.cs
containing the explicitly authorized read-side selection/envelope APIs.

All existing prohibitions remain effective elsewhere.
Do not create directory-wide exclusions.
Do not relax:

* event-subscription ownership;
* enqueue-only event handlers;
* selection/view write prohibition;
* Autodesk.AutoCAD.Internal prohibition;
* persistence prohibition;
* PluginInitializer prohibition.

All pre-existing T1/T2 product files remain forbidden, including:
BridgeOutcomes.cs
BridgeSource.cs
EventBridgeCore.cs
Hints.cs
IHostEventSource.cs
SelectionContext.cs
SessionId.cs
WorkspaceSession.cs
WorkspaceSessionRegistry.cs
AutoCadEventBridge.cs
Do not modify them.
No UI files.
No UI tests.
No project files.
No existing Plugin commands.
Run the exact RequiredTests from the accepted delegation.
At minimum:
Workspace Core filter
FullyQualifiedName~RackCad.Tests.Workspace
and the full Core suite required by the delegation.
Run required Release builds.
Plugin build must introduce no new applicable warnings.
The source guards and both command census tests must pass in the full suite.
Worker must produce:

1. RED commit;
2. push;
3. GREEN commit;
4. push;
5. valid worker-handoff;
6. terminate.

Wait for exact-SHA CI.
Then invoke exactly ONE valid Controller verification:
gpt-6-luna
high
read-only
If:
EXECUTION_VERIFIED / NONE
then continue without Git writes to applicable controls:
nc1
nc2
I64-SCOPE-BRIDGE-01 if required under A-4
nc3
I64-FREETEXT-BRIDGE-01 if required under A-6
nc4 is already PASS.
If a valid verification returns REWORK:
STOP S-11.
No correction Worker.
Do not plan or implement F1-T3B-HOST-UI.
Do not create PaletteSet.
Do not add WPF.
Do not invoke AutoCAD.
If T3A verification and protocol controls close successfully:
custody and create the documentary T3A close commit,
wait for exact CI,
then stop at:
COORDINATOR_T3A_CLOSE_REVIEW_REQUIRED
If anything requires a corrective Worker:
STOP S-11.
If instrumentation requires durable logging:
STOP C-T3-INSTRUMENTATION.
If scope expands:
STOP Coordinator.
Proceed with the single T3A Worker.
```
