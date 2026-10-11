# I-64 — Decisión del Coordinator: descomposición de F1-T3-HOST en F1-T3A-HOST-CORE y F1-T3B-HOST-UI

Transcripción literal recibida por el chat de la sesión responsable el 2026-10-11, en respuesta al STOP
COORDINATOR_T3_GATE_CONTRACT_REQUIRED (paquete docs/initiatives/I-64-f1-t3-gate-contract-package.md en b687cb86, CI 38110790934 4/4).

```text
I-64 — COORDINATOR DECISION
F1-T3-HOST DECOMPOSITION
TARGET SHA:
b687cb86b5209a039ec8630eff19470cb42758b0
Expected origin/main at issuance:
bb0d5522e8411f66a51fdfb3f1f0d0514b737453
Do NOT issue monolithic F1-T3-HOST.
Replace its execution decomposition with:
F1-T3A-HOST-CORE
F1-T3B-HOST-UI
This is a decomposition of the already frozen F1 result.
It does not defer or remove scope.
T3A must close before T3B is planned.
Both are classified:
Implementacion transversal a capas (corta)
Semantic effort: Deep
Worker:
claude-sonnet-5-5
subagent
high
write-commit-push
Controller:
gpt-6-luna
Codex CLI 0.162.0-alpha.17.2
high
read-only
RoutingEnforcement:
required
For EACH of T3A and T3B:
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
README §10
plus A-4 / A-6 substitute mechanisms when applicable.
Attempt:
3
AttemptsRemaining:
0
CorrectionsAuthorized:
false
A valid REWORK requiring a new Worker:
STOP S-11.
No reset and no increase of max_attempts.
F1-T3A-HOST-CORE owns the non-visual/testable host foundation:

* IWorkspaceHostPort;
* one lazy Workspace controller per process;
* RACKPANEL command;
* starting T2 bridge lazily;
* active-session binding;
* SessionId authorization;
* fine selection reader used only from drain;
* SelectionContext.Classify / ApplyContext path;
* panel-visible policy;
* pure host policies;
* instrumentation toggle / measurement points;
* source guards;
* command censuses.

No PaletteSet/WPF implementation in T3A.
No AutoCAD execution.
Modify existing I-64 T2 guard:
WorkspaceBridgeSourceGuardTests.cs
only enough to permit named, narrowly scoped adapters.
Planning must derive the final filenames, but the contract must identify
them exactly before delegation.
Required semantic exceptions:

1. exactly one RACKPANEL command adapter under Plugin/Workspace may contain:

[CommandMethod("RACKPANEL")]

2. exactly one selection-reader adapter under Plugin/Workspace may use the
read-only APIs required by D-06.

The guard must continue to prohibit:

* SetImpliedSelection;
* view mutation;
* selection mutation;
* Autodesk.AutoCAD.Internal;
* new governed event subscriptions outside the existing central bridge;
* ordinary event handlers doing DB reads;
* persistence;
* PluginInitializer integration;
* command/editor changes outside RACKPANEL.

Do not weaken the guard by directory-wide exclusion.
Use exact-file exceptions.
The two existing command census tests may be modified only to:

* add RACKPANEL;
* change the exact expected total from 37 to 38.

No other command census changes.
Do not modify RackLog.cs in T3A.
Implement only:

* in-memory instrumentation toggle;
* timing collection;
* process-memory collection;
* structured measurement points;
* use of the existing logging surface only if its semantics are honest.

If the current RackLog API cannot record normal timing/memory information
without abusing error/warning semantics:
STOP C-T3-INSTRUMENTATION.
Do not invent a global logging API.
Return that issue to Coordinator.
Do not emit T3B yet.
After T3A closes, T3B will contain:

* single PaletteSet per process;
* AddVisual;
* RackCad.UI Workspace root UserControl;
* view-model;
* active-document / empty state;
* focus policy;
* panel exception containment;
* Error state;
* local WPF resource merge;
* UI instrumentation presentation.

No navigation.
No authored writes.
No AutoCAD.Internal.
Create no profile-diff documentation yet.
Reserve:
F1-T3C-PROFILE-DOC
as session-owned documentary work after T3A and T3B close and before F1
close / Smoke-1.
It does not use a product Worker.
T3A and T3B do NOT require I-52 host RELEASE because no AutoCAD process is
run.
Do not:

* launch AutoCAD;
* load DLL;
* touch profile;
* touch registry;
* touch ACL;
* touch TRUSTEDPATHS;
* touch SECURELOAD.

Smoke-1 still requires the applicable I-52 RELEASE.
No current product overlap.
Before every delegation freshly verify the I-65 remote tip.
Do not edit ROADMAP.
Respect any future repo-wide structural-analysis guard that is already
integrated in MainSha.
Do not consume unmerged I-65 code or rules.
First version this Coordinator decision and the T3 decomposition.
That commit X becomes the proposed:
AuthorityRevision = X
Wait for its exact CI 4/4.
Then fetch again.
MainSha = current origin/main at contract issuance.
If main != bb0d5522:
apply S-13 / 16.7 before issuing.
After decision commit + CI:
derive and emit ONLY the gate-contract for:
F1-T3A-HOST-CORE
Validate rackcad-gate-contract/v1.
Then invoke exactly one valid Controller planning output:
gpt-6-luna / high / read-only
Evaluate A1-A8 and nc4 without short-circuit.
Do NOT invoke Worker.
Stop at:
COORDINATOR_T3A_ACCEPTANCE_REQUIRED
Return:

* decision commit + CI;
* current MainSha;
* T3A contract SHA-256;
* Planning RunId;
* delegation SHA-256;
* A1-A8;
* nc4;
* exact AllowedWriteScope;
* exact ForbiddenWriteScope;
* exact guard/census files allowed to change;
* invariants;
* RequiredTests;
* RED plan;
* Worker routing;
* Attempt / AttemptsRemaining;
* any deviations.

F1-T2-BRIDGE = COMPLETE
F1-T3-HOST monolithic = NOT USED
F1-T3A-HOST-CORE = AUTHORIZED FOR CONTRACT + PLANNING ONLY
F1-T3B-HOST-UI = NOT YET AUTHORIZED
F1-T3C-PROFILE-DOC = RESERVED / NOT YET AUTHORIZED
F1 = NOT PASS
Smoke-1 = NOT AUTHORIZED
```
