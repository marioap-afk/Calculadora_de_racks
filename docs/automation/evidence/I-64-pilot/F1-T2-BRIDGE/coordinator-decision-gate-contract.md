# I-64 F1-T2-BRIDGE — Decisión del Coordinator: contrato de gate (presupuesto, attempts, alcance e INV-21)

Transcripción literal de la orden del Coordinator recibida por el chat de la sesión responsable el 2026-10-10, en respuesta al STOP
COORDINATOR_T2_GATE_CONTRACT_REQUIRED (evidencia §39; paquete `docs/initiatives/I-64-f1-t2-gate-contract-package.md`). La propia orden
exige versionarla, con la actualización de estado resultante, antes de emitir el `gate-contract.json` transitorio; el commit que la
versiona es la `AuthorityRevision` y el `BaseSha` inicial de F1-T2-BRIDGE.

```text
I-64 — COORDINATOR DECISION
F1-T2-BRIDGE GATE CONTRACT
TARGET_SHA:
9ff0a287861e1740691e45253360538a7fc8c05a
Expected origin/main:
bb0d5522e8411f66a51fdfb3f1f0d0514b737453
F1-T1-MODEL is accepted as:
COMPLETE_UNDER_I64_SCOPE_BRIDGE
Historical nc2 remains FAIL.
Active protocol amendment:
I-64-A-4
T1 product after rebase/revalidation is accepted as preserved evidence.
Do not reopen T1.
RESOLVED.
F1-T2-BRIDGE has the following gate-specific invocation budget:
PLANNING:

* 1 valid output maximum;
* up to 2 additional invocations only for transport failure /
INVALID_OUTPUT.

WORK:

* 1 valid output maximum;
* up to 2 additional invocations only for transport failure /
INVALID_OUTPUT.

VERIFICATION:

* 1 valid output maximum;
* up to 2 additional invocations only for transport failure /
INVALID_OUTPUT.

CONTROL:

* nc1..nc4 and I64-SCOPE-BRIDGE-01 according to README §10 and A-4;
* transport retries only as their governing rules allow.

A second valid Planning/Work/Verification output is NOT authorized merely
to improve or correct the first one.
P-07 applies against these limits.
RESOLVED.
Canonical initiative state remains:
attempts = 3
max_attempts = 3
Therefore the first T2 delegation uses:
Attempt = 3
AttemptsRemaining = 0
MaxReworkLoops = 3
This DOES NOT prohibit the first delegation of the new TaskId.
However:
ANY valid T2 result that requires a correction consuming attempts
=> STOP S-11.
No correction Worker is authorized.
No reset.
No increase of max_attempts.
No reuse of T1 budget.
CorrectionsAuthorized in the gate contract:
FALSE
because there is no remaining correction budget.
Transport retries are not corrections and follow their separate limits.
ACCEPTED.
AllowedWriteScope:
src/RackCad.Application/Workspace/
src/RackCad.Plugin/Workspace/
tests/RackCad.Tests/Workspace/
The Application allowance exists only so T2 can implement pure,
AutoCAD-free bridge policy/state required for deterministic Core testing.
STRICT CONDITION:
existing T1 product files in src/RackCad.Application/Workspace/ are
FORBIDDEN.
Planning must enumerate those exact existing files in ForbiddenWriteScope.
Only NEW Application Workspace files may be created by T2.
No modification of:
SessionId.cs
SelectionContext.cs
Hints.cs
WorkspaceSession.cs
WorkspaceSessionRegistry.cs
or any other pre-existing T1 file.
If implementation requires changing one:
STOP C-04 / scope expansion.
This decision is NON-MATERIAL relative to Freeze D-02:
Application already owns pure state machines and policies; AutoCAD access
remains exclusively in Plugin.
ACCEPTED.
For T2:
INV-21 obligation is limited to:

* bridge performs no authored write;
* no NOD write;
* no XData write;
* no profile/registry persistence;
* no persistent Workspace state;
* no write path is exposed by the bridge.

The full IWorkspaceHostPort write-surface rule is verified in T3, where the
host/UI port is introduced.
T2 MUST NOT create a premature host/UI port merely to satisfy INV-21.
If T2 discovers that a port is technically necessary:
STOP as T3 scope.
Before emitting the transient gate-contract:

1. version this Coordinator decision and the resulting state update;
2. push it;
3. wait for its exact CI 4/4 success.

That new commit X becomes:
AuthorityRevision = X
and initial:
BaseSha = X
provided:
HEAD = origin/architecture/workspace-persistente-rackcad = X
and origin/main remains:
bb0d5522e8411f66a51fdfb3f1f0d0514b737453
MainSha =
bb0d5522e8411f66a51fdfb3f1f0d0514b737453
If origin/main changed before contract emission:
DO NOT emit against stale MainSha.
Apply the required 16.7 recovery/rebase and return if any semantic conflict
appears.
TaskId:
F1-T2-BRIDGE
Gate:
F1
Objective:
Implement the central F1 AutoCAD event bridge with a simulable pure event
source/policy boundary, without host, panel or product writes.
Required behavior includes:

* one central subscriber;
* exactly one subscription per governed source;
* subscription lifecycle follows live Workspace document sessions;
* handlers enqueue hints only;
* deferred/coalesced drain;
* Idle only while hints are pending;
* BeginDocumentClose remains alive for the whole session including degraded
mode;
* degradation is session-local;
* no event writes selection or view;
* no persistence;
* no authored mutation;
* no existing commands/editors changed.

Use INV-F1-T2-01..14 from:
docs/initiatives/I-64-f1-t2-gate-contract-package.md
with the Coordinator dispositions above applied.
In particular:
INV-F1-T2-02:
AutoCAD event `+=` subscriptions only in
src/RackCad.Plugin/Workspace/.
INV-F1-T2-03:
ordinary event handlers only enqueue.
INV-F1-T2-05:
BeginDocumentClose lifecycle survives degraded mode.
INV-F1-T2-07:
transaction/document-lock lifetime safety.
INV-F1-T2-08:
T2 bridge has zero persistence/write authority.
INV-F1-T2-12:
no I-63 metrics/providers/ProjectSummary/aggregation.
INV-F1-T2-13:
RACKEDITAR and classic commands unchanged.
INV-F1-T2-14:
no Autodesk.AutoCAD.Internal.
Project:
tests/RackCad.Tests/RackCad.Tests.csproj
Filter:
FullyQualifiedName~RackCad.Tests.Workspace
MinSelected:
1
ExpectRed:
true
T2 is a new chain.
Therefore:
ChainBaseSha = first T2 BaseSha
ChainRedSha = null
ChainRedFiles = []
A real RED is mandatory.
RED must be behavioral and compilable.
No artificial compile failure.
The RED must demonstrate at least one frozen T2 behavior not yet
implemented.
GREEN must not modify the RED test files after the RED commit contrary to
16.8.
Controller:
gpt-6-luna
Codex CLI
high
read-only
Worker:
claude-sonnet-5-5
subagent
high
Task class:
Implementacion transversal a capas (corta)
Semantic effort:
Deep
RoutingEnforcement:
required
CorrectionsAuthorized:
false
Normal Controller Scope verification remains mandatory.
nc2 is still executed and preserved.
If Controller nc2 again fails to discriminate Scope, use the active
I64-SCOPE-BRIDGE-01 under A-4 exactly as authorized.
Do not modify the A-4 verifier.
Do not treat a ScopeBridge result as rewriting historical Controller nc2.
T2 may proceed because its current product/file scope does not collide with
I-52.
But T2 MUST NOT:

* start AutoCAD;
* touch host campaign state;
* touch ACL;
* touch profile/registry;
* touch TRUSTEDPATHS;
* touch SECURELOAD;
* perform Smoke-1.

Smoke-1 remains after F1 PASS and requires the applicable I-52 RELEASE.
Use S-01..S-14 and P-01..P-08 plus C-01..C-13 from the T2 package.
Additionally STOP if:

* any existing T1 Workspace source file must change;
* any valid Worker/verifier result requires correction;
* any valid verification returns REWORK;
* scope must expand;
* UI or host work is needed;
* PluginInitializer or existing commands must change;
* csproj/sln change is needed;
* AutoCAD execution is needed;
* main moves in a way requiring unresolved semantic reconciliation.

After versioning this decision and obtaining exact CI 4/4:

1. emit gate-contract.json using the exact post-decision AuthorityRevision;
2. validate it against rackcad-gate-contract/v1;
3. invoke ONE valid CONTROLLER_PLANNING run;
4. evaluate A1-A8 without short-circuit.

If A1-A8 all PASS:
STOP at:
COORDINATOR_T2_ACCEPTANCE_REQUIRED
Do NOT invoke Worker yet.
Return:

* decision commit and CI;
* AuthorityRevision;
* MainSha;
* contract SHA-256;
* planning RunId;
* delegation SHA-256;
* A1-A8 table;
* routing facts;
* Attempt / AttemptsRemaining;
* ChainBaseSha / ChainRedSha / ChainRedFiles;
* exact AllowedWriteScope and ForbiddenWriteScope;
* any deviations.

If any A1-A8 fails:
do not invoke Worker.
Return the failure to Coordinator.
F1-T2-BRIDGE:
AUTHORIZED FOR PLANNING
NOT YET AUTHORIZED FOR WORKER
F1:
NOT PASS
Smoke-1:
NOT AUTHORIZED
No merge.
No Candidate.
No Owner Validation yet.
Proceed.
```
