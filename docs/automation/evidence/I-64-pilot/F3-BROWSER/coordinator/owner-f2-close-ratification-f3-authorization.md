I-64 — F2 CLOSE RATIFICATION / F3 AUTHORIZATION
Current I-64 branch:
1b7d4cbc45872382e1062eab2e487aef1decef2c
Current origin/main:
812baf879612320653094a8aaf46aca0bac81383
Current branch already contains current main.
F2 remains:
CLOSED / PASS WITH LIMITATIONS
Do NOT reopen F2 merely because the host mechanism used by navigation had to
change during real AutoCAD Smoke-NAV.
Accepted evidence includes:

* F2-T1 CLOSED / PASS;
* F2-T2A CLOSED / PASS;
* F2-T2B CLOSED / PASS;
* F2-T2-HOST-C1 completed;
* final candidate 003be859;
* Core 16320 / 16320;
* UI 1792 passed + 17 skipped;
* exact candidate CI 42777768225 = 4 / 4;
* final custody CI 42781604339 = 4 / 4;
* real AutoCAD Smoke-NAV evidence.

The navigation host correction that replaced:
ExecuteInCommandContextAsync / command-context execution
with:
application context + explicit DocumentLock
for the explicit navigation actions is:
RATIFIED
as an I-64 host-mechanism substitution.
It does NOT reopen F2.
However, documentation must state the substitution explicitly.
Do NOT continue claiming that the implemented navigation actions execute in
command context.
The ratification applies ONLY to these explicit navigation operations:

* Seleccionar;
* Ver;
* Centrar;
* Localizar vista.

It does NOT automatically apply to:

* Actualizar;
* authored writes;
* editor mutations;
* F4 write paths;
* F5 editor integration;
* arbitrary Plugin operations.

Those retain their own Freeze/contract requirements.
The application-context implementation is accepted because the required
observable/safety invariants remain mandatory:

1. explicit user action only;
2. active document must match the originating SessionId;
3. document must be quiescent;
4. no RackCad modal may be active;
5. DocumentLock held for the AutoCAD operation;
6. immediate revalidation before effects;
7. target still exists;
8. target is still attributable under NavigationTargets authority;
9. failure means zero selection/view effect;
10. failure invalidates/refetches the rack navigation cache as specified;
11. no authored DWG write;
12. no event/context change may trigger navigation automatically;
13. content-based echo remains intact;
14. focus returns to the drawing after the explicit action;
15. no DBObject/ObjectId/Transaction survives the operation.

The mechanism changed.
The safety and observable contract did not.
Record an append-only implementation clarification before or with the next
useful documentation commit.
It must say, in substance:
For the four F2 navigation effects, real AutoCAD Smoke demonstrated that the
command-context implementation did not produce the required observable
effect. The accepted implementation therefore executes the explicit
navigation effect in application context under DocumentLock, preserving
session identity, quiescence, immediate target revalidation, zero authored
writes, echo semantics and focus policy.
Do NOT rewrite historical Proposal V5.
Do NOT silently edit the frozen historical text.
Record the implemented substitution and its evidence.
Accept:
Smoke-NAV = PASS WITH LIMITATIONS
Measured PASS:
SN-01 through SN-07
SN-10
SN-09:
measured and acceptable
first action <= 798 ms
subsequent actions 139-270 ms
Limitations:

1. SN-08 was not directly provoked because the final navigation execution is
synchronous.
2. Smoke used synthetic test drawings rather than representative production
racks.
3. SN-06c and the tab-driven portion of SN-10 depend on F3.

These limitations do NOT reopen F2.
They become later validation obligations.
Do NOT mark SN-08 as independently proven if it was not directly exercised.
Preserve it as:
LIMITED / covered structurally but not directly provoked
Before final I-64 Owner Validation, exercise the equivalent safety property
again where possible:
a request belonging to session/document A must never affect B.
F3/F4 multi-document tests may supply additional deterministic evidence.
Add to final Owner Validation:
on representative real RackCad racks, repeat at minimum:

* Seleccionar;
* Ver;
* Localizar vista;
* copy k/M where multiple references exist;
* one document/session switch safety case.

Also observe Undo/Redo behavior after view navigation.
Undo behavior is currently:
UNCHARACTERIZED
not:
FAILED
It is not an F2 close blocker.
Decision C-F3-01:
F3 begins with a NEW phase-local budget:
attempts = 1 / 3
AUTHORIZED.
The 3 / 3 state reached during F2 does NOT carry into F3.
Attempts are not a global I-64 lifetime counter.
Record this explicitly before the first F3 Worker delegation.
Do not rewrite F2 attempt history.
The prepared package:
docs/initiatives/I-64-f3-gate-contract-package.md
is accepted as the basis for issuing the F3 contract, subject to the
decisions below.
ACCEPTED WITH CLARIFICATION.
A newly created preview may begin from RackId alone.
Name, Kind, views and lightweight summary information are resolved lazily
when the active tab surface is materialized.
NavigationTargets remains the single authority for NAVIGATION TARGETS.
Do not redefine NavigationTargets as a general metadata authority merely to
supply Name/Kind.
Summary metadata may come from the already accepted lightweight I-64 capture
facts / summary adapter, while navigation references continue to come from
NavigationTargets.
No global F6 inventory is introduced in F3.
ACCEPTED.
When a tab is active, explicit navigation actions target:
the RackId of the active tab.
A SelectionContext Uno(R):

* activates the existing tab for R; or
* creates/replaces the preview for R.

Therefore selection context and active tab normally converge.
Changing tabs manually must NOT write AutoCAD selection.
ACCEPTED AS STAGED DELIVERY.
"Abrir en editor clásico" for systems not yet integrated may remain deferred
to F5.
F3 must still provide the lightweight summary/navigation surface.
Record the F5 obligation explicitly.
Do NOT create a temporary command-launch hack in F3 merely to satisfy the
label.
This deferral does not remove the final D-08/D-15 product obligation.
ACCEPTED.
Tabs belong to SessionId.
They exist only in memory.
Destroying the document session destroys its tab registry.
No tab state persists into another opening of the same DWG.
ACCEPTED.
Only SelectionContext Uno(R) creates/activates/replaces the preview through
automatic context synchronization.
The following do NOT automatically create or switch tabs:

* Selection (N);
* None;
* diagnostic/non-rack contexts.

User-explicit row/open actions may still open a rack according to their own
contract.
F3 implements:

* lightweight per-session tab registry;
* at most one preview per session;
* pinned tabs;
* close semantics without DWG writes;
* active tab;
* K = 1 live surface;
* lazy summary materialization;
* navigation bound to active tab;
* responsive tab strip / surface behavior;
* automated scenarios 4, 5 and 6.

F3 does NOT implement:

* editable DraftSession;
* real dirty semantics;
* authored updates;
* editor migration;
* F6 browser/inventory;
* batch editing.

Authorize first:
F3-T1-BROWSER-CORE
Application / pure.
Primary responsibilities:

* BrowserSession;
* per-SessionId registry;
* TabRecord;
* preview replacement;
* pin;
* activation;
* close-clean;
* session destruction;
* active RackId;
* K=1 surface policy state where appropriate.

No AutoCAD types.
No WPF types.
No authored writes.
No event subscriptions.
Required behavioral tests include:

* maximum one preview;
* selecting existing rack activates existing tab;
* new Uno(R) replaces only the previous preview;
* pinned tab never replaced by preview behavior;
* closing clean tab writes nothing;
* session isolation;
* document-session destruction clears tabs;
* Selection(N)/None/diagnostic context does not create/switch tabs;
* deterministic active-tab behavior.

After F3-T1 is verified/closed:
F3-T2-BROWSER-UI
Responsibilities:

* WorkspaceBrowserViewModel;
* responsive tab strip;
* active summary surface;
* K = 1 live surface;
* navigation actions bound to active RackId;
* lazy summary query;
* states/reasons when unavailable;
* narrow and wide Workspace layouts.

No fixed-width assumption.
No second permanent RackCad UX.
F3 must automate at minimum the accepted F3 scenarios:
Scenario 4:
rack/context change
Scenario 5:
tab change
Scenario 6:
10 lightweight tabs
For Scenario 6 establish explicitly:

* 10 tab records may exist;
* exactly 1 live surface;
* no per-tab AutoCAD subscriptions;
* no per-tab retained AutoCAD objects;
* tab switching does not write DWG;
* tab switching does not write selection/view;
* memory/live-surface count remains bounded by K.

F3 must characterize the cost of:

* creating/replacing preview;
* activating existing tab;
* switching among 10 tabs;
* materializing the K=1 summary surface.

Do not invent a hard performance threshold unless an existing authority
already provides one.
Record measured results for later Smoke-PERF/F7.
The current I-64 branch already contains current:
main = 812baf87...
Use a FRESH BaseSha/MainSha measurement when the F3 gate contract is issued.
If main moves before Worker invocation:
apply normal workflow reconciliation.
Do not reuse F2 historical control frames.
The directory:
/d/i64-snav
may be deleted if and only if a final safety check proves:

* it is the expected diagnostic directory;
* it contains no source/evidence needed for custody;
* the two DXF/build outputs are already represented by hashes/evidence;
* no process has them open.

Deletion is cleanup only.
Do not let cleanup block F3.
Proceed now with:

1. record this disposition;
2. set F3 attempts = 1 / 3;
3. issue the F3 gate contract using the accepted package and decisions;
4. run normal planning/acceptance;
5. execute F3-T1;
6. verify and close F3-T1;
7. execute F3-T2;
8. verify and close F3-T2;
9. run F3 scenario harnesses;
10. close F3 if evidence is sufficient.

Do not request another Owner decision merely to start F3.
Return only if:

* a real material scope change appears;
* product attempts exhaust;
* a frozen observable behavior must change;
* evidence contradicts correctness;
* or an external/manual-only blocker is reached.

F0:
CLOSED / PASS
F1:
CLOSED / PASS
F2:
CLOSED / PASS WITH LIMITATIONS
F3:
AUTHORIZED
attempts 1 / 3
F4-F7:
PENDING
Smoke-NAV:
PASS WITH LIMITATIONS
Smoke-2:
PENDING
Owner Validation:
PENDING
