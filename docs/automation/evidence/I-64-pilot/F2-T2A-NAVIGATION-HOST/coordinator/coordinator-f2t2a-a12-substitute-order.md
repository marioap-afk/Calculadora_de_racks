I-64 — F2-T2A-NAVIGATION-HOST
nc1 NORMAL PATH EXHAUSTED
SUBSTITUTE PROTOCOL ORDER
Verified product:
44a692a4ec386e559cc6d330b3d78b87dbad6581
Real verification:
R20261109T201422Z-89a6
EXECUTION_VERIFIED / NONE
14 / 14 PASS
nc1 runs:
3d86 = INVALID_OUTPUT
16b6 = INVALID_OUTPUT
bfc8 = ORACLE NOT MET / NORMAL PATH EXHAUSTED
P-04:
ACTIVE FOR ORDINARY nc1 RETRIES
nc2:
NOT RUN
nc3:
NOT RUN
R20261109T212644Z-bfc8 is classified as:
SCHEMA_VALID_BUT_ORACLE_INVALID
CONTROL NOT PASSED
The Controller correctly established all of the following:

* the mutated CurrentSha is false;
* Git HEAD remains 44a692a4;
* remote branch remains 44a692a4;
* Identity fails;
* execution is BLOCKED / STOP;
* all checks other than Handoff and Identity match the real verification.

The defect is:
Handoff also fails because the Controller evaluates repository consistency
inside Handoff.
That violates the nc1 relative oracle, which requires the mutation to be
isolated to Identity.
Preserve bfc8 exactly as emitted.
Ordinary nc1 path is:
EXHAUSTED
No additional Controller nc1 invocation is authorized.
Do not:

* rerun nc1;
* rewrite the prompt;
* spend another control retry;
* start nc2;
* start nc3.

Open a new I-64-only protocol amendment:
I-64-A-12
Proposed bridge name:
I64-IDENTITY-BRIDGE-01
Purpose:
provide deterministic substitute evidence for nc1 when the Controller
systematically conflates Handoff record validation with repository Identity.
This bridge applies only to:
F2-T2A-NAVIGATION-HOST
nc1
and only to the exact defect signature observed in:
3d86
16b6
bfc8
A-12 must independently establish two separate questions.
A. HANDOFF INTEGRITY
The handoff artifact itself is internally valid:

* schema valid;
* TaskId correct;
* Attempt correct;
* DelegationRunId correct;
* WorkRunId correct;
* branch/worktree identifiers correct;
* output time valid;
* relay OutputSha256 matches the handoff bytes;
* no unrelated handoff field was changed by the nc1 mutation.

The deliberately mutated CurrentSha is allowed to differ from repository
state for this control.
That repository mismatch must NOT make the Handoff-integrity check fail.
B. IDENTITY FAILURE
The mutated CurrentSha must then be compared with authoritative repository
state and must fail Identity because:
mutated CurrentSha != HEAD
and/or
mutated CurrentSha != remote branch
while all non-mutated identity facts remain coherent.
The bridge must compare:
real worker handoff
vs
nc1-mutated handoff
and prove:

* both are schema-valid;
* only CurrentSha changed;
* the real CurrentSha is exactly:
44a692a4ec386e559cc6d330b3d78b87dbad6581
* the mutated CurrentSha is the nc1 synthetic nonexistent SHA;
* no other field changed;
* relay OutputSha256 matches the mutated handoff bytes where applicable.

If any second mutation exists:
FAIL CLOSED.
The real repository state must independently prove:
HEAD =
44a692a4ec386e559cc6d330b3d78b87dbad6581
remote branch =
44a692a4ec386e559cc6d330b3d78b87dbad6581
BaseSha ancestry:
PASS
RedSha ancestry:
PASS
branch identity:
PASS
mutated CurrentSha equality:
FAIL
Expected bridge conclusion:
HandoffIntegrity = PASS
Identity = FAIL
ControlResult = PASS
meaning:
the nc1 negative control successfully demonstrates that the Identity check
rejects the false CurrentSha without any actual handoff-copy corruption.
A-12 does NOT:

* change the product;
* change the real verification;
* reinterpret nc2 or nc3;
* alter AUTOMATION_PLAN globally;
* redefine normal Handoff semantics outside this substitute;
* authorize a fourth nc1 Controller run;
* reuse A-11;
* modify P-04 history.

3d86, 16b6 and bfc8 remain historical failures of the ordinary Controller
oracle.
Materiality:
M-01 = NO
M-02 = NO
M-03 = NO
M-04 = YES
M-05 = NO
M-06 = NO
M-07 = NO
M-08 = NO
Reason:
A-12 changes only the substitute evidence path used after a systematic
Controller protocol defect.
No observable RackCad behavior changes.
Authority required:
Owner + Architect + Coordinator
Status on creation:
PROPOSED / NOT ACTIVE
Do not execute the bridge against real T2A evidence until:
Owner = SATISFIED
Architect = AGREED
Coordinator = AGREED
Before activation, implement synthetic fail-closed tests.
Minimum cases:
T01:
real handoff + real repo state -> HandoffIntegrity PASS / Identity PASS
T02:
only CurrentSha mutated -> HandoffIntegrity PASS / Identity FAIL
T03:
TaskId also changed -> HandoffIntegrity FAIL
T04:
Attempt changed -> HandoffIntegrity FAIL
T05:
DelegationRunId changed -> HandoffIntegrity FAIL
T06:
WorkRunId changed -> HandoffIntegrity FAIL
T07:
branch changed -> HandoffIntegrity FAIL
T08:
relay output hash does not match handoff -> HandoffIntegrity FAIL
T09:
mutated CurrentSha accidentally equals HEAD -> control FAIL
T10:
HEAD and remote disagree -> FAIL CLOSED
T11:
BaseSha not ancestor -> FAIL CLOSED
T12:
RedSha not on expected chain -> FAIL CLOSED
T13:
malformed handoff JSON -> FAIL CLOSED
T14:
second mutation hidden in nested field -> FAIL CLOSED
T15:
nonexistent CurrentSha only -> exact nc1 substitute PASS
The suite must be deterministic and run twice byte-identically.
Until activation:
do NOT run A-12 against:

* real Worker handoff;
* real nc1 mutated handoff;
* real relay files;
* real repository occurrence inputs.

Synthetic fixtures only.
Real occurrence may be bound by SHA-256 in the amendment.
After:

* A-12 document versioned;
* verifier versioned;
* synthetic self-tests 100% PASS twice;
* regression demonstration versioned;
* exact-SHA CI complete;

run one formal read-only Architect review using the already accredited:
gpt-6.1-sol / high
cell, provided the CLI binary/config remain unchanged.
Architect must review:

* Handoff/Identity separation;
* mutation exactness;
* fail-closed behavior;
* no global protocol authority;
* no real occurrence data before activation;
* M-04;
* no semantic expansion into nc2/nc3.

Verdict:
AGREED
or
CHANGES REQUIRED
If Architect = AGREED, request exactly:
SATISFIED — authorize I-64-A-12 / I64-IDENTITY-BRIDGE-01 as an I-64-only
substitute evidence path for the exhausted F2-T2A nc1 Handoff/Identity
Controller defect, without changing product semantics or global protocol.
Do not infer Owner approval.
Only after:
Owner SATISFIED
Architect AGREED
Coordinator AGREED
may A-12 execute ONCE against the real T2A nc1 evidence.
If PASS:
record:
nc1 = SATISFIED_UNDER_I64_IDENTITY_BRIDGE
Then proceed normally to:
nc2
and then:
nc3
If A-12 FAILS:
STOP.
No further nc1 Controller run.
F2-T1:
CLOSED / PASS
F2-T2A Attempt 1:
REWORK
F2-T2A Attempt 2:
EXECUTION_VERIFIED
nc1 ordinary path:
EXHAUSTED
P-04:
ACTIVE
A-12:
AUTHORIZED TO PREPARE
NOT ACTIVE
nc2:
WAITING
nc3:
WAITING
attempts:
2 / 3
F2-T2B:
WAITING
Smoke-NAV:
PENDING
