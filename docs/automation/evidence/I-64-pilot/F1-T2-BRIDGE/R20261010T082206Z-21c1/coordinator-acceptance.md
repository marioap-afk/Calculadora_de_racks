# I-64 F1-T2-BRIDGE — Aceptación formal del Coordinator (planificación R20261010T082206Z-21c1)

Transcripción literal recibida por el chat de la sesión responsable el 2026-10-10, tras la parada en
COORDINATOR_T2_ACCEPTANCE_REQUIRED.

```text
I-64 — COORDINATOR ACCEPTANCE
F1-T2-BRIDGE
Planning RunId:
R20261010T082206Z-21c1
Delegation SHA-256:
c5f3d482597c54c28065ea01ad8a228cf2ea15fb301f8a31ed2c292c2fae5737
Formal disposition:
ACCEPTED
A1 = PASS
A2 = PASS
A3 = PASS
A4 = PASS
A5 = PASS
A6 = PASS
A7 = PASS
A8 = PASS
nc4 = PASS
Invoke exactly ONE valid Worker execution.
Worker:
claude-sonnet-5-5
subagent
high
Task:
F1-T2-BRIDGE
Attempt:
3
AttemptsRemaining:
0
CorrectionsAuthorized:
false
This is the only valid Worker output authorized for this task.
Transport/INVALID_OUTPUT retries remain governed separately.
A valid Worker result requiring correction is:
STOP S-11
Do not launch a correction Worker.
No prompt drift.
No scope expansion.
No modification of accepted delegation fields.
No additional files beyond AllowedWriteScope.
No change to any pre-existing T1 Application Workspace source file.
src/RackCad.Application/Workspace/
src/RackCad.Plugin/Workspace/
tests/RackCad.Tests/Workspace/
IMPORTANT:
Within:
src/RackCad.Application/Workspace/
ONLY NEW FILES may be added.
The following existing T1 files are strictly forbidden:
Hints.cs
SelectionContext.cs
SessionId.cs
WorkspaceSession.cs
WorkspaceSessionRegistry.cs
If any of them needs modification:
STOP.
Do not modify:
docs/
.github/
assets/
eng/
deploy/
global.json
Directory.Build.props
Directory.Build.targets
RackCad.sln
src/RackCad.UI/
src/RackCad.Domain/
tests/RackCad.UI.Tests/
src/RackCad.Plugin/Systems/
src/RackCad.Plugin/Views/
src/RackCad.Plugin/Drawing/
src/RackCad.Plugin/PluginInitializer.cs
any existing Plugin command file
any csproj
I-52 or I-62 branches
Implement the F1 central event bridge only.
Required properties:

* one central AutoCAD event subscriber;
* exactly one subscription per governed source;
* lifecycle follows live Workspace document sessions;
* no per-tab or per-editor subscriptions;
* ordinary handlers only enqueue hints;
* handlers do not read/write DB;
* handlers do not select, navigate or change view;
* deferred/coalesced drain;
* Idle subscription exists only while hints are pending;
* BeginDocumentClose remains subscribed for the entire session lifetime,
including degraded mode;
* degradation is session-local;
* no persistence;
* no authored write;
* no NOD/XData/profile/registry write;
* no host, PaletteSet, WPF or RACKPANEL;
* no I-63 metrics/providers/ProjectSummary;
* no RACKEDITAR/classic-command change;
* no Autodesk.AutoCAD.Internal.

Keep AutoCAD-dependent adapters in Plugin.
Any pure/simulable policy/state needed for Core tests may be NEW files under
Application/Workspace.
Account for the known repository guards before committing.
In Application.Workspace avoid names/tokens prohibited by the existing T1
boundary guards, including the measured problematic families:
Subscription
Tab
Provider
Transaction
Database
Do not evade the guards semantically.
Choose terminology that faithfully expresses the frozen model without
creating false ownership or persistence semantics.
Production namespaces must match folder structure.
RackCad.Tests files keep the assembly test namespace conventions.
Respect the existing nullable policy.
Do not introduce new Plugin build warnings.
A real behavioral RED is mandatory.
ChainBaseSha:
d5bdcb7aff31ba54901a3a0acdbfd81be92ab516
ChainRedSha:
null
ChainRedFiles:
[]
RED must:

* compile;
* select >= 1 test;
* fail because a frozen T2 behavior is absent;
* not be an artificial failure;
* not be a compile error.

Good targets include one of:

* subscription/lifecycle cardinality;
* event path only enqueueing;
* source guard preventing forbidden event behavior;
* pure bridge policy required by INV-02/INV-03/INV-14.

Do not modify the RED test files after the RED commit in a way that invalidates
the RED chain requirements.
Implement only what is necessary to make the accepted T2 contract green.
Run at minimum:
dotnet test tests/RackCad.Tests/RackCad.Tests.csproj 
--filter FullyQualifiedName~RackCad.Tests.Workspace
and Core full according to the accepted delegation.
Run required Release builds.
Worker must produce:

1. RED commit;
2. push;
3. GREEN commit;
4. push;
5. valid worker-handoff;
6. terminate.

No session Git writes between Worker completion and final verification /
controls except protocol-authorized recovery.
Before Worker starts, verify:
origin/main =
bb0d5522e8411f66a51fdfb3f1f0d0514b737453
HEAD = origin/I-64 =
d5bdcb7aff31ba54901a3a0acdbfd81be92ab516
If main moved before Worker writes:
STOP S-13.
Do not let the Worker start on stale authority.
Wait for exact-SHA CI.
Require the four required jobs in success before final verification can
become VERIFIED.
Then invoke exactly ONE valid:
CONTROLLER_VERIFICATION
gpt-6-luna
high
read-only
No second valid verification is authorized.
If verification returns:
EXECUTION_VERIFIED / NONE
continue immediately, with NO session Git writes, to the required controls.
If verification returns any valid:
REWORK
STOP S-11.
No correction Worker.
If STOP/BLOCKED:
apply only the governing recovery if it does not require a correction.
Otherwise stop.
After EXECUTION_VERIFIED and before any session Git write:
execute applicable nc1, nc2, nc3.
nc4 is already PASS.
Historical T1 controls are irrelevant to the T2 chain.
For T2 nc2:
run Controller nc2 normally and preserve its result.
Under active I-64-A-4, also execute I64-SCOPE-BRIDGE-01 according to its
exact authorized verifier and oracle.
Do not modify the verifier.
No old T1 input may be reused as T2 evidence.
Only then:

* custody planning, nc4, Worker, verification and controls;
* update state/evidence;
* create documentary close commit;
* wait for exact CI.

Do NOT declare F1 PASS yet.
T2 completion only enables the next F1 task.
Stop at:
COORDINATOR_T2_CLOSE_REVIEW_REQUIRED
and report the exact resulting SHA and evidence.
The previously published REBASE record containing unredacted
[projects.<path>] key names is NOT part of this Worker task.
Do not rewrite or purge history.
Record no additional local paths in new evidence.
Use the existing redaction convention:
[projects.<redacted>]
Any destructive history rewrite remains Owner-reserved.

1. trabajo completado;
2. participantes invocados;
3. RED / GREEN exactos;
4. pruebas y CI;
5. verificación;
6. nc1/nc2/nc3/nc4 + ScopeBridge;
7. deviations / STOP;
8. commits y SHA;
9. siguiente acción.

Start with the Worker.
```
