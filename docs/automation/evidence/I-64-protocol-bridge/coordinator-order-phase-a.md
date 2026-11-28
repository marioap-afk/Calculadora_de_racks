# I-64 — Orden del Coordinator: EMERGENCY PROTOCOL BRIDGE / PHASE A

Transcripción literal de la orden recibida por el chat de la sesión responsable el 2026-10-09 (ejecutada en §§33-34).

```text
I-64 — EMERGENCY PROTOCOL BRIDGE / PHASE A
TARGET_SHA:
39b45f36383e6017da38e5bf91e08e485298a4fc
Current origin/main:
bb0d5522e8411f66a51fdfb3f1f0d0514b737453
DO NOT start F1-T2-BRIDGE yet.
The Owner explicitly authorized EMERGENCY PARALLEL RESUMPTION through
alternative (B) of I-64 f1_resume_condition.
I-62 remains active and untouched.
This order authorizes preparation and review of an I-64-only authoritative
protocol bridge.
It does NOT authorize changing I-61.
Preserve:
ProductVerifiedSha:
39f7caa411a5ebb372614c233a32255de7398cca
Product CI:
37046833476 = 4/4 success
Verification:
R20261002T182925Z-5c9b
EXECUTION_VERIFIED / NONE
Historical controls:
nc1 = PASS
nc2 = FAIL
nc3 = PASS
nc4 = PASS
Do NOT rewrite nc2 as PASS.
Do NOT modify its evidence.
Create:
docs/initiatives/I-64-A-1.md
Use the standard amendment format.
Freeze:
commit:
9b43dafbe3b8874aa0ba5a85d62cbf953dbdd311
path:
docs/initiatives/I-64-proposal-v5.md
blob:
dc1924ff5a73a4e529fbc5f995621d29351a091b
Applies-to:
I-64
Authority:
Coordinator-only
Owner emergency authorization:
SATISFIED by the current Owner order.
Architect authority:
NOT REQUIRED under LIFECYCLE §6 because M-01..M-08 remain NO.
Independent conformance review:
REQUIRED BEFORE ACTIVATION by this emergency order.
Protocol-owner decision:
NOT separately required unless the review concludes this bridge changes
I-61 semantics rather than I-64 evidence requirements.
I-64 Proposal V5 remains subject to I-61 execution protocol and
gate-specific authorization.
The historical I-61 nc2 result remains FAIL.
Add an I-64-only evidence rule:
I64-SCOPE-BRIDGE-01.
For I-64 only, a delegated task's Scope negative-control obligation may be
satisfied by the mechanical bridge below instead of the model-executed nc2,
provided:

1. the original nc2 evidence is preserved;
2. normal Controller Scope verification remains required;
3. the real mechanical check passes;
4. the adversarial mechanical check fails closed;
5. all other I-61 controls remain unchanged.

This does not declare the historical nc2 PASS.
It provides new authoritative closure evidence for I-64 only.
The bridge remains applicable to future I-64 delegated tasks until an
integrated protocol authority supersedes it.
M-01 = NO
M-02 = NO
M-03 = NO
M-04 = NO
M-05 = NO
M-06 = NO
M-07 = NO
M-08 = NO
Reason:
evidence mechanism only; no frozen product result changes.
Keep the verifier unit-local.
Suggested location:
docs/automation/evidence/I-64-protocol-bridge/tools/verify_scope_bridge.py
Python stdlib only.
Inputs:
--repo
--delegation
--handoff
--expected-base
--expected-current
It MUST fail closed.
Checks:

* delegation schema/type is expected;
* delegation.BaseSha exactly equals expected-base;
* handoff.BaseSha exactly equals expected-base;
* handoff.CurrentSha exactly equals expected-current;
* DelegationRunId matches;
* BaseSha and CurrentSha are valid 40-hex commit objects;
* BaseSha is an ancestor of CurrentSha;
* obtain changed paths mechanically with Git from:
BaseSha..CurrentSha;
* read AllowedWriteScope directly from the accepted delegation;
* read ForbiddenWriteScope directly from the accepted delegation;
* validate scope syntax;
* every changed path must be covered by AllowedWriteScope;
* zero changed paths may intersect ForbiddenWriteScope;
* missing evidence, parse error, Git error, ambiguous path, invalid scope,
invalid SHA or unexpected condition => FAIL.

No Controller interpretation is part of PASS/FAIL.
Produce deterministic JSON containing at least:
Schema
Result
Reasons
BaseSha
CurrentSha
DelegationSha256
HandoffSha256
ChangedPaths
AllowedWriteScope
ForbiddenWriteScope
Exit:
0 only for PASS.
Nonzero for every FAIL/error.
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
Expected:
PASS
ADVERSARIAL:
use the already-custodied mutated delegation from final nc2:
docs/automation/evidence/I-64-pilot/F1-T1-MODEL-nc2/
R20261002T184050Z-8415/input-mutated-delegation.json
same handoff
same BaseSha
same CurrentSha
Expected:
FAIL
The result must identify:
src/RackCad.Application/Workspace/WorkspaceSessionRegistry.cs
as outside AllowedWriteScope.
Do not use the old Controller nc2 output as the substitute oracle.
Also demonstrate failure for at least:

* nonexistent BaseSha;
* nonexistent CurrentSha;
* malformed delegation JSON;
* missing AllowedWriteScope;
* BaseSha not ancestor of CurrentSha.

No mutation of product.
Before activation, run ONE independent read-only conformance review through
an eligible CLI transport.
Do not ask the reviewer to execute Scope by reasoning.
Review only:

* A-1 authority and materiality;
* whether the verifier exactly preserves the old Scope invariant;
* whether any route can fail open;
* whether historical nc2 remains FAIL;
* whether any I-61 or I-62 normative text is changed;
* whether future I-64 use is strictly scoped.

Required result:
AGREED / CONFORMING
0 REQUIRED
If REQUIRED exists:
STOP and return to Coordinator.
If no eligible independent CLI cell exists:
STOP and report that fact.
Do not invent eligibility.
Architect signature is not normatively required.
After:

* A-1 drafted;
* verifier implemented;
* self-tests PASS;
* independent review = AGREED / 0 REQUIRED;
* exact SHA and CI of that documentation/tooling commit are known;

STOP at:
COORDINATOR_A1_AGREEMENT_REQUIRED
Return:

* amendment commit/blob;
* verifier path/blob/SHA-256;
* self-test evidence;
* independent review RunId/result;
* CI;
* M-01..08;
* any deviations.

DO NOT execute the bridge against T1 before Coordinator AGREED.
Only after A-1 is AGREED:

1. execute real ScopeBridge => must PASS;
2. execute adversarial ScopeBridge => must FAIL;
3. preserve historical nc2 = FAIL;
4. retain nc1 PASS, nc3 PASS, nc4 PASS;
5. reconcile F1-T1-MODEL under A-1.

No T1 planning/Worker/Controller budget is reset.
No old nc2 retry is authorized.
origin/main has advanced:
819955d6 -> bb0d5522
I-63 is now integrated in main.
Before T2:

* fetch;
* coordinate the rebase under WORKFLOW;
* rebase I-64 onto current main;
* create the required rebase evidence/RebaseMap;
* preserve historical ProductVerifiedSha 39f7caa4;
* map the T1 product commit by patch-id / exact diff;
* preserve A-1 append-only;
* STOP on conflict or semantic drift.

After a clean rebase run:

* Workspace focal tests;
* Core full;
* exact-SHA CI 4/4.

If T1 product files conflict or their patch changes:
STOP to Coordinator; do not inherit the old verification blindly.
I-63:
already integrated in main at bb0d5522.
Consume it only through main.
No dependency on any unmerged I-63 branch.
I-52:
remains active.
Before T2:

* notify/coordinate using the existing boundary;
* no AutoCAD host run;
* no profile, TRUSTEDPATHS, SECURELOAD or ACL changes;
* no ROADMAP/HANDOFF/ADR-index hot-file write without the existing window;
* Smoke-1 remains subject to I-52 host release.

T2 code work may proceed later only if no hot-file/ownership conflict exists.
Do NOT:

* start F1-T2-BRIDGE;
* change T1 product;
* change I-61;
* change I-62;
* rewrite historical nc2;
* declare the protocol block resolved;
* declare F1 PASS.

Start with A-1 + mechanical verifier + independent review.
```
