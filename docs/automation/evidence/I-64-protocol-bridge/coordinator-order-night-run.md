# I-64 — Orden del Coordinator: NIGHT RUN (EMERGENCY PARALLEL RESUMPTION / AUTHORITATIVE PROTOCOL BRIDGE)

Transcripción literal de la orden recibida por el chat de la sesión responsable el 2026-10-10. Incluye el mandato del Owner (custodiado
también aparte en `owner-mandate-emergency.md`) y la disposición condicional del Coordinator sobre A-2 o su A-n correctora.

```text
I-64 — NIGHT RUN
EMERGENCY PARALLEL RESUMPTION / AUTHORITATIVE PROTOCOL BRIDGE
PRIORITY: HIGH
Operate autonomously for the full run. Do not stop merely to report intermediate progress when the next action is already authorized below.
The objective tonight is:

1. resolve ARCHITECT_CELL_REQUIRED;
2. obtain the formal Architect review of A-2;
3. if conforming, activate A-2 through the Coordinator authority embedded below;
4. execute and verify the mechanical ScopeBridge against T1;
5. reconcile F1-T1-MODEL without rewriting historical nc2;
6. rebase I-64 onto current main if still required;
7. revalidate the preserved T1 product after the rebase;
8. determine whether F1-T2-BRIDGE is executable;
9. if executable, prepare its exact gate-contract package and stop only at the first authority boundary that is not already covered by this prompt.

Do NOT wait for I-62.
Expected I-64 branch:
architecture/workspace-persistente-rackcad
Expected HEAD / origin branch:
5533b9fe324fbffc8dbcb4765d13f50c295bb117
Expected origin/main at issuance:
bb0d5522e8411f66a51fdfb3f1f0d0514b737453
If origin/main has advanced:
do not treat that alone as fatal.
Record it and follow the rebase rules below at the authorized rebase point.
Freeze:
commit:
9b43dafbe3b8874aa0ba5a85d62cbf953dbdd311
path:
docs/initiatives/I-64-proposal-v5.md
blob:
dc1924ff5a73a4e529fbc5f995621d29351a091b
A-1:
docs/initiatives/I-64-A-1.md
blob:
fb6a49c2e8e96689f82a2ad01fda7994fc2397b0
A-2:
docs/initiatives/I-64-A-2.md
blob at 5533b9fe:
de6f6088356b0dec0e7a33431c7d00d0ba0d396b
Mechanical verifier:
docs/automation/evidence/I-64-protocol-bridge/tools/verify_scope_bridge.py
blob:
093a570f71a359e8e004b8983ea9625e74ebbc87
SHA-256:
327982a9f97335e23afecce00a08f1b1a3e90aef5da7fa087fcd1863878b8bca
Self-test:
23/23 PASS
ProductVerifiedSha:
39f7caa411a5ebb372614c233a32255de7398cca
Product CI:
37046833476
4/4 SUCCESS
Product verification:
R20261002T182925Z-5c9b
EXECUTION_VERIFIED / NONE
Historical controls:
nc1 = PASS
R20261002T183529Z-b10b
nc2 = FAIL
R20261002T184050Z-8415
nc3 = PASS
R20261002T185053Z-8b6f
nc4 = PASS
R20261002T004303Z-8d3c
Historical nc2 remains FAIL permanently.
Never relabel it PASS.
Never edit its evidence.
Never rerun it.
The following is the Owner's emergency mandate that was previously missing
from the local review package. Preserve it verbatim in the durable evidence
for this bridge:
--- OWNER MANDATE BEGIN ---
I-64 — EMERGENCY PARALLEL RESUMPTION
AUTHORITATIVE PROTOCOL BRIDGE
OWNER PRIORITY: HIGH.
Objective: unblock I-64 independently of I-62 completion, using the explicit alternative (B) already recorded in I-64's f1_resume_condition.
I-62 remains active and unchanged. Do not wait for its integration.

1. Verify the exact current state of I-64, its Freeze, F1 evidence, existing protocol failure nc2, and the governing WORKFLOW, AGENTS, LIFECYCLE and AUTOMATION_PLAN.
2. Preserve F1-T1-MODEL's ProductVerifiedSha 39f7caa4 as historical evidence. Do not rewrite or retroactively declare its failed protocol close PASS.
3. Prepare the smallest authoritative amendment applicable ONLY to I-64 that permits a conforming, independently verifiable substitute for the failed Scope negative control.
4. The substitute must mechanically check:
   * exact BaseSha and CurrentSha;
   * changed paths from git diff --name-only;
   * AllowedWriteScope from the accepted delegation;
   * every changed path belongs to AllowedWriteScope;
   * no ambiguous or missing evidence;
   * fail-closed on mismatch.
5. Determine explicitly whether the replacement requires Owner authority, independent Architect agreement, Coordinator agreement, or a protocol-owner decision. Obtain all required authorities before use.
6. Do not change I-61's normative text, I-62's branch, the original failed nc2 evidence, or the frozen product scope of I-64.
7. After the amendment is agreed, reconcile I-64's previous verification and determine which controls must be re-executed. Do not assume exhausted budgets have reset or that repeating the old nc2 is authorized.
8. Rebase I-64 if necessary, preserving its historical evidence and observing worktree/ROADMAP ownership.
9. If every prerequisite passes, authorize resumption of F1-T2-BRIDGE and continue I-64 independently.
10. Coordinate hot files with I-52 and I-63 using their existing boundaries; do not create a dependency on unmerged I-62 code.
11. Keep the Owner's intervention minimal. Prepare independent reviews through eligible CLI transports where authorized. Request Owner-reserved decisions in one consolidated packet.
12. Report:

* exact blocker and replacement control;
* authoritative amendment and required approvals;
* gates that must be repeated;
* whether F1-T2-BRIDGE is now executable;
* exact commit, CI and next task.

Do not declare the block resolved until the substitute is agreed and verified.
Priority: resume product development in I-64, not redesign the entire execution protocol.
--- OWNER MANDATE END ---
This satisfies the Owner authority identified in A-2 for the unit-scoped
protocol exception.
It does NOT authorize a global I-61 amendment.
Accepted Owner baseline:
config.toml SHA-256:
73890CA3...
Key-name count:
105
Observed structural delta from the old baseline:
new [projects.<redacted>] section with trust_level.
Do not read or persist sensitive values.
Any later hash change:
STOP P-01.
The current runtime was re-measured successfully:
probe:
R20261010T014704Z-97f8
CLI:
codex-cli 0.162.0-alpha.2
Model:
gpt-6-luna
Effort:
high
Transport:
CLI / read-only
Probe result:
PASS
No P-01.
No P-02.
No P-06.
This cell may continue to be used only for capabilities its measurement
actually supports.
A formal Architect review of A-2 requires:
Profile:
ARCHITECTURE_REVIEW
Semantic effort:
Deep
Transport:
independent / read-only
Do NOT route down silently.
Candidate preferred cell:
gpt-6.1-sol
Codex CLI
read-only
effort high
Reason:
gpt-6.1-sol is level Equilibrado and can satisfy Deep if the exact cell is
measured and eligible.
The Owner hereby authorizes exactly ONE read-only capability/eligibility
probe of:
gpt-6.1-sol
Codex CLI current runtime
effort high
read-only
Purpose:
measure eligibility for this Architect review only.
Conditions:

* no Git writes;
* no config.toml changes;
* no product access beyond harmless known-oracle reads;
* no API key;
* no paid API setup;
* use existing authenticated subscription transport only;
* no P-06 usage/credit warning;
* config.toml must remain at accepted baseline;
* process/relevo rules still apply.

The probe must establish:

1. requested model = effective model = gpt-6.1-sol;
2. requested effort = effective effort = high;
3. read-only enforced;
4. structured output works;
5. tool-use/read works;
6. no P-06;
7. existing authentication succeeds without new credentials;
8. config.toml unchanged.

If PASS:
record the cell as measured for this review and continue immediately.
If FAIL because the model/runtime itself is unavailable or ineligible:
do not retry using another unmeasured cell.
Continue to the STOP rules at the end.
If the invocation would require API credits, a key, purchase or new auth:
STOP OWNER_RESERVED.
If the gpt-6.1-sol/high probe passes, invoke ONE formal Architect review.
Mode:
SEPARATE SESSION / independent read-only CLI
Object:

* I-64 Freeze V5 exact commit/path/blob;
* I-64 A-1 exact commit/blob;
* I-64 A-2 exact commit/blob;
* mechanical verifier exact blob/SHA-256;
* self-test evidence 23/23;
* historical T1 verification;
* historical nc1/nc2/nc3/nc4 evidence;
* independent A-1 review R20261010T012851Z-de0d;
* both REQUIRED from that review;
* Owner emergency mandate above;
* LIFECYCLE §5-§6;
* AUTOMATION_PLAN §16;
* README §10;
* WORKFLOW and AGENTS applicable authority.

Architect must explicitly answer:

1. Is M-04 = YES correct?
2. Is the Owner authority above sufficient for this I-64-only protocol exception?
3. Does A-2 preserve the exact Scope invariant?
4. Is the mechanical verifier at least as fail-closed for Scope as the old nc2 oracle?
5. Can historical nc2 remain FAIL while substitute evidence supplies closure without contradiction?
6. Is the exception genuinely limited to I-64?
7. Is I-62 authority or implementation unnecessary?
8. Are M-01,02,03,05,06,07,08 correctly NO?
9. Are REAL PASS + ADVERSARIAL FAIL sufficient?
10. Is there any fail-open path?
11. Does A-2 improperly modify global I-61 semantics?
12. Are there any hidden authority/predecessor conflicts after main = bb0d5522?

Required successful result:
AGREED
REQUIRED = 0
Optional findings:
record and disposition if they do not change the amendment materially.
If REQUIRED > 0:
do not activate A-2.
If correction is strictly within the Owner mandate and does not expand scope,
you may prepare A-3 as an append-only correction, but DO NOT self-approve it:
obtain a new Architect review of the corrected exact object if the remaining
night budget safely allows it.
Maximum Architect correction loop tonight:
2 correction rounds after A-2.
No infinite revision loop.
If a REQUIRED implies:

* global I-61 change;
* expanded product scope;
* I-62 dependency;
* new Owner-reserved decision;

STOP and consolidate.
This prompt carries the Coordinator disposition:
IF AND ONLY IF the exact A-2 or its authorized append-only correcting A-n
receives:
Architect = AGREED
REQUIRED = 0
and all of the following remain true:

* Owner emergency mandate unchanged;
* Applies-to = I-64 only;
* historical nc2 remains FAIL;
* verifier identity is exact;
* M-04 = YES;
* no other M is YES;
* no I-61 text modified;
* no I-62 branch modified;
* CI of the amendment/evidence commit = 4/4 SUCCESS;

THEN the Coordinator decision is:
COORDINATOR = AGREED
for that exact amendment object.
Version this decision before executing the bridge.
If any predicate is false:
Coordinator agreement is NOT granted.
Only after Phase 3 succeeds.
Do NOT rerun Controller nc2.
Use the exact authorized verifier.
REAL:
delegation:
docs/automation/evidence/I-64-pilot/F1-T1-MODEL/
R20261002T171954Z-754a/delegation.json
handoff:
docs/automation/evidence/I-64-pilot/F1-T1-MODEL/
R20261002T181905Z-faf1/worker-handoff.json
BaseSha:
0b7db52f12ddbf7ed36c8c2d020645818335c0fa
CurrentSha:
39f7caa411a5ebb372614c233a32255de7398cca
Required:
PASS
exit 0
ADVERSARIAL:
delegation:
docs/automation/evidence/I-64-pilot/F1-T1-MODEL-nc2/
R20261002T184050Z-8415/inputs/delegation.json
same handoff
same BaseSha
same CurrentSha
Required:
FAIL
nonzero exit
The ONLY substantive failure reason must be:
OUTSIDE_ALLOWED_WRITE_SCOPE
and OutsideAllowed must contain:
src/RackCad.Application/Workspace/WorkspaceSessionRegistry.cs
Identity/Git/evidence checks must otherwise pass.
If REAL fails:
STOP.
If ADVERSARIAL passes:
STOP.
If ADVERSARIAL fails for another reason:
STOP.
Do not modify inputs to make the oracle pass.
If:

* amendment AGREED;
* REAL = PASS;
* ADVERSARIAL = expected FAIL;

then record:
Historical nc1 = PASS
Historical nc2 = FAIL
Historical nc3 = PASS
Historical nc4 = PASS
ScopeBridge REAL = PASS
ScopeBridge ADVERSARIAL = PASS AS A NEGATIVE TEST
(meaning verifier correctly returned FAIL for out-of-scope mutation)
Do not relabel nc2.
Reconcile:
ProductStatus:
EXECUTION_VERIFIED
Historical ProductVerifiedSha:
39f7caa411a5ebb372614c233a32255de7398cca
Protocol closure under applicable I-64 A-n:
SATISFIED
F1-T1-MODEL:
COMPLETE_UNDER_I64_SCOPE_BRIDGE
or the closest existing state vocabulary that accurately records:
historical product verification + subsequent unit-local protocol closure.
Do not invent a state enum if the state schema forbids it.
Use fields/text if necessary.
Budgets remain historically exhausted:
do not reset them.
After T1 reconciliation, fetch again.
main was:
bb0d5522e8411f66a51fdfb3f1f0d0514b737453
at issuance.
If unchanged or advanced:
rebase onto current origin/main as required by WORKFLOW.
Preserve:

* Freeze identity;
* A-1 historical;
* A-2 / applicable correction A-n append-only;
* all old evidence;
* historical ProductVerifiedSha;
* historical nc2 FAIL.

Create proper RebaseMap/evidence.
For the T1 product commit:
verify patch-id / semantic diff mapping.
Critical file:
src/RackCad.Application/Workspace/WorkspaceSessionRegistry.cs
The T1 product delta must remain only the authorized documentation comment.
If rebase conflict touches:

* T1 product;
* Workspace model semantics;
* applicable Freeze/A-n;
* I-63-integrated code in a way that changes T1 assumptions;

STOP to Coordinator.
Do not resolve a semantic conflict speculatively.
Document-only merge conflicts may be resolved only when meaning is provably preserved.
On the rebased exact SHA:
run at minimum:

1. Workspace focal:
FullyQualifiedName~RackCad.Tests.Workspace
2. Core full suite.
3. required Release builds according to current AGENTS.
4. exact-SHA push CI:
all required jobs success.

Also mechanically verify:

* no new dependency from Workspace model into I-63 metrics/providers;
* MASTER-I63-I64-02 separation remains intact after I-63 integration;
* no AutoCAD refs entered Application Workspace;
* ADR 0047 remains PROPOSED;
* RACKEDITAR unchanged.

This is REVALIDATION after rebase.
Do not pretend it is the old Controller verification.
If any product test or invariant fails:
STOP; T1 needs a new product decision.
If only documentation/evidence needs updating:
fix within authority and continue.
I-52 remains active at issuance around:
feature/rackmirror-espejo-semantico
fb6b5648...
Before authorizing T2 product work:
perform read-only ownership/hot-file reconciliation.
Do not:

* start AutoCAD;
* interfere with I-52 host campaign;
* touch ACL;
* touch TRUSTEDPATHS;
* touch SECURELOAD;
* modify profile/registry;
* modify I-52 branch.

Confirm no active ownership conflict with the files proposed for T2.
Smoke-1 remains blocked until the applicable I-52 host RELEASE.
T2 implementation itself does not require Smoke-1 to be executed first
unless the Freeze/gate sequence explicitly says otherwise.
Only if all previous phases PASS.
At that point F1-T2-BRIDGE becomes eligible for preparation.
DO NOT invent its scope from memory.
Derive its exact contract from:

* Freeze V5;
* current I-64 evidence/state;
* current post-rebase code;
* F1 task decomposition already recorded;
* applicable A-n;
* current main authorities.

Expected conceptual purpose:
central Workspace bridge / host-facing integration required after the pure
T1 model, without yet expanding into unrelated later gates.
But exact files, invariants, RED obligations and boundaries must be derived
from the authoritative documents.
Prepare:

* task objective;
* AllowedWriteScope;
* ForbiddenWriteScope;
* invariants;
* expected RED;
* RequiredTests;
* STOP conditions;
* authorities;
* routing;
* ChainBase/RED implications;
* I-52 hot-file exclusions;
* I-63 integrated-main implications.

Run a read-only Controller planning step only if:

* an eligible cell exists;
* current applicable protocol/A-n permits it;
* invocation budget for the NEW T2 task is defined independently from exhausted T1 budget.

T1 exhausted budget MUST NOT be reused or reset.
T2 is a new TaskId and requires its own explicit budget derived from the
F1 gate plan.
If the existing F1 contract already fixes that budget, use it.
If not, stop at:
COORDINATOR_T2_GATE_CONTRACT_REQUIRED
Do not invent a budget.
You may autonomously correct:

* documentary inconsistencies;
* malformed evidence records;
* deterministic verifier/test harness defects found before activation;
* review-package fidelity defects;
* append-only A-n corrections responding to Architect REQUIRED findings,
only when they remain fully inside the Owner emergency mandate;
* rebase documentary conflicts with provable semantic equality.

Each correction must preserve history.
Do not edit previous A-n or historical evidence.
Return to Owner only for:

* new credentials/authentication;
* paid/API/credit requirement;
* config.toml hash changes from accepted baseline;
* scope expansion beyond I-64;
* global I-61 change;
* need to depend on unmerged I-62 implementation;
* product scope change;
* removal or weakening of OV;
* AutoCAD manual validation;
* security/profile/registry/ACL/TRUSTEDPATHS/SECURELOAD changes;
* destructive/irreversible action;
* Architect finding that the unit-local exception is insufficient.

Consolidate all Owner-reserved items into ONE packet.
Do not ask the Owner questions one at a time.
STOP to Coordinator on:

* Architect REQUIRED that cannot be corrected within this mandate;
* ScopeBridge oracle mismatch;
* rebase semantic conflict;
* product test regression;
* inability to establish exact authority;
* ambiguous ownership/hot-file collision;
* invalid or non-green exact-SHA CI;
* no valid T2 budget;
* protocol contradiction.

Do not merge I-64 into main.
Do not auto-merge.
Do not modify ROADMAP/HANDOFF as integrated.
Do not declare FINAL_CANDIDATE_SHA.
At the end, report only the highest useful achieved state, plus blockers.
Use:

1. Trabajo completado
2. Participantes invocados directamente
3. Loops autónomos y correcciones
4. Autoridades / A-n
5. ScopeBridge REAL / ADVERSARIAL
6. Rebase y revalidación
7. I-52 / I-63 coordinación
8. Estado de F1-T1-MODEL
9. Estado de F1-T2-BRIDGE
10. STOP / decisiones pendientes
11. Commits, blobs y CI exactos
12. Siguiente acción

Explicitly answer:

* Did gpt-6.1-sol/high become eligible?
* Architect verdict and REQUIRED count?
* Which A-n is the active amendment?
* Did Coordinator AGREED become valid?
* REAL bridge result?
* ADVERSARIAL bridge result?
* Is historical nc2 still FAIL?
* Is T1 now protocol-closed?
* Was I-64 rebased?
* Are focal/Core/CI green after rebase?
* Is T2 executable?
* Was T2 planning started?
* What exact authority boundary stopped the run?

Do not stop merely because a phase completed.
Continue until a real boundary above is reached.
```
