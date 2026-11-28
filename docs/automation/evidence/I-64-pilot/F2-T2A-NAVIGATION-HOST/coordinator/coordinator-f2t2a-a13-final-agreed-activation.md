I-64 — F2-T2A / A-13
COORDINATOR FINAL AGREED / ACTIVATION
Product branch:
architecture/workspace-persistente-rackcad
Verified product SHA:
44a692a4ec386e559cc6d330b3d78b87dbad6581
Temporary A-13 branch:
architecture/workspace-persistente-rackcad-a12
A-13 SHA:
31a1c837a3c63f2e8886ef38c63b68bd52354078
Current main:
04a7041492d760170ce3f3ba5fceeb29146107cc
Owner:
SATISFIED
Architect:
AGREED
Review:
R20261110T022847Z-bc2b
REQUIRED:
0
OPTIONAL:
0
Coordinator:
AGREED
Therefore:
I-64-A-13 / I64-IDENTITY-BRIDGE-01
is:
ACTIVE
for exactly ONE real application against the bound F2-T2A nc1 occurrence.
A-12 remains:
HISTORICAL
CHANGES REQUIRED
NOT ACTIVE
Neutral Git object-query probe:
PASS
Accepted facts:

* exit 0;
* exact missing-object stdout;
* empty stderr;
* repository unchanged before/after;
* neutral id distinct from bound occurrence ids;
* no bridge execution;
* no Identity evaluation;
* no HandoffIntegrity evaluation.

This probe does not consume the one real A-13 execution.
origin/main movement to:
04a7041492d760170ce3f3ba5fceeb29146107cc
is accepted for this activation because all relevant authority blobs were
remeasured and remain identical to the A-13 authority set.
Before the real bridge execution:
remeasure the relevant authority blobs once more.
If any differ:
STOP.
Exactly ONE real execution is authorized.
No retry.
Immediately before execution, verify:

1. product branch HEAD =
44a692a4ec386e559cc6d330b3d78b87dbad6581
2. product remote branch =
44a692a4ec386e559cc6d330b3d78b87dbad6581
3. product tree clean
4. temporary A-13 branch =
31a1c837a3c63f2e8886ef38c63b68bd52354078
5. A-13 verifier blob =
1c2510f71160804c6d42ae4da580641523c72a07
6. config.toml matches accepted A/B/C/D
7. 0 conflicting participants/processes
8. relevant authority blobs unchanged
9. every bound real-input SHA-256 in A-13 Sec.8 matches exactly

If any condition fails:
STOP
Do not execute the bridge.
Extract the verifier bytes from the exact Git blob:
1c2510f71160804c6d42ae4da580641523c72a07
Do not execute an uncommitted worktree copy.
The real repository occurrence for Identity is the PRODUCT branch:
44a692a4...
The A-13 side branch is only the source of the versioned protocol/verifier.
Do not checkout the side branch over the product worktree.
The bridge must independently establish:
HandoffIntegrity = PASS
Identity = FAIL
ControlResult = PASS
Required meaning:

* real and nc1-mutated handoffs are schema-valid;
* relay is schema-valid;
* relay/handoff binding is valid;
* the only authorized mutation is CurrentSha;
* no second mutation exists;
* real CurrentSha = verified product SHA;
* mutated CurrentSha = bound nonexistent synthetic SHA;
* product HEAD/remote remain the verified SHA;
* BaseSha / RedSha / branch identity are coherent;
* the mutated object is proven missing through the exact successful Git query;
* no ambiguous/error Git state is accepted.

If the single real execution returns PASS:
record:
A-13 REAL APPLICATION = PASS
and:
nc1 = SATISFIED_UNDER_I64_IDENTITY_BRIDGE
Preserve ordinary-control history:
3d86 = INVALID_OUTPUT
16b6 = INVALID_OUTPUT
bfc8 = CONTROL NOT PASSED / P-04 exhausted
Do not rewrite P-04 history.
Then proceed immediately to ordinary:
nc2
while the product branch remains frozen at:
44a692a4...
If nc2 closes:
proceed to:
nc3
Again:
product branch must remain 44a692a4 during both controls.
If A-13 real application returns FAIL:
STOP.
Do not retry A-13.
Do not run nc2.
Do not run nc3.
Do not create another bridge automatically.
Return exact evidence to Coordinator.
A-13 does not alter normal nc2/nc3 semantics.
nc2 and nc3 run with:

* normal Controller prompt;
* normal Identity;
* no post-custody mode;
* product branch still exactly CurrentSha.

A-4 may be used only if nc2 reproduces its exact accepted historical Scope
defect.
A-6 may be used only if nc3 reproduces its exact accepted historical
FreeText defect.
A-11 is consumed and unavailable.
Do NOT commit A-13 or control custody to the product branch before:
A-13 real application
nc2
nc3
are complete.
Keep all new evidence in ignored artifacts or on the temporary side branch.
This preserves ordinary Identity for the remaining controls.
After all three controls are closed:

1. append final A-13 / nc1 / nc2 / nc3 custody and state documentation to
the temporary side branch;
2. verify product branch still equals 44a692a4;
3. verify temporary branch ancestry begins directly from that product SHA;
4. fast-forward the product branch to the completed temporary-branch tip;
5. no merge commit;
6. no rebase;
7. no cherry-pick;
8. push;
9. run exact-SHA CI;
10. only after successful fast-forward/CI, delete the temporary side branch.

Then return at:
COORDINATOR_F2_T2A_CLOSE_REQUIRED
Do NOT start T2B before that close.
F2-T1:
CLOSED / PASS
F2-T2A product:
EXECUTION_VERIFIED
nc1 ordinary path:
EXHAUSTED
A-13:
ACTIVE
ONE REAL APPLICATION AUTHORIZED
nc2:
WAITING
nc3:
WAITING
attempts:
2 / 3
product branch:
FROZEN AT 44a692a4
F2-T2B:
WAITING
Smoke-NAV:
PENDING
