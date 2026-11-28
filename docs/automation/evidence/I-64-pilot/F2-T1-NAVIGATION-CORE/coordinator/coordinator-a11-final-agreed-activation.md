I-64 — A-11 / I64-TESTFILTER-BRIDGE-01
COORDINATOR FINAL AGREED / ACTIVATION
Current branch:
c2ef7ee564f269fc277a24a25f3eed516da19f66
Exact-SHA CI:
42652882800
4 / 4 SUCCESS
Authority:
Owner = SATISFIED
Architect = AGREED
R20261109T155930Z-5dd4
0 REQUIRED
0 OPTIONAL
Coordinator = AGREED
Therefore:
I-64-A-11 = ACTIVE
for the single authorized F2-T1 occurrence.
A-8, A-9 and A-10 remain:
HISTORICAL
CHANGES REQUIRED
NOT ACTIVE
Exactly ONE execution of:
I64-TESTFILTER-BRIDGE-01
is AUTHORIZED against the real F2-T1 evidence.
Occurrence:
F2-T1-NAVIGATION-CORE
nc3 Tests-filter substitute evidence
No Controller nc3 invocation is authorized.
No Worker invocation is authorized.
No product modification is authorized.
Before executing the bridge, verify the exact SHA-256 bindings declared by
A-11 for all four real inputs:

1. GREEN TRX
2. RED TRX
3. real Worker handoff
4. nc3-mutated Worker handoff

Also verify:

* term-set authority/binding;
* verifier blob identity;
* required Git SHAs;
* branch/repository identity;
* config.toml remains accepted Snapshot A/B/C/D;
* no unauthorized participant/process condition.

If ANY bound input hash differs:
STOP.
Do not execute the bridge.
Do not substitute a regenerated equivalent file.
Execute the verifier using the exact versioned blob:
f9780f2f30c067243c93b6ab7175cc4b3179ca39
Extract the bytes from Git.
Do not execute an uncommitted working-tree copy.
Do not modify the verifier before execution.
Real A-11 bridge executions allowed:
1 / 1
No retry.
If execution cannot establish its required inputs:
STOP.
If execution returns FAIL:
STOP.
Do not repair inputs and rerun.
Do not fall back to another Controller nc3 invocation.
A valid PASS must independently establish the A-11 requirements, including:
RequiredTests filter:
FullyQualifiedName~Workspace
with the accepted deterministic selection semantics.
Expected GREEN:
Selected = 724
Passed = 724
Failed = 0
Expected RED:
Selected = 724
Passed = 673
Failed = 51
RED composition:
50 xUnit assertion failures
1 NullReferenceException
MinSelected:
724 >= 645
ChainRedFiles:
must equal the six already accredited test files.
RED -> GREEN:
must not modify those test files.
nc3 mutation:
must be schema-valid and exact;
only the authorized WorkCompleted mutation;
exactly one prohibited 16.10 term introduced;
no compensating mutation.
TRX parsing:
must satisfy all fail-closed structural, GUID, counter, execution identity,
message-cardinality and root-anchoring requirements established by A-11.
If the single real execution returns PASS:
record:
nc3 = SATISFIED_UNDER_I64_TESTFILTER_BRIDGE
and:
A-11 REAL APPLICATION = PASS
Preserve historical classifications:
46c1 = INVALID_OUTPUT
041d = ORACLE NOT MET
53af = INVALID_OUTPUT
Do not rewrite P-04 history.
Then:

1. custody the real A-11 execution;
2. custody this Coordinator activation;
3. update state;
4. update I-64 evidence;
5. commit/push documentation/evidence only;
6. exact-SHA CI;
7. return at:

COORDINATOR_F2_T1_CLOSE_REQUIRED
Do NOT start F2-T2 yet.
If the single real bridge execution returns FAIL:
STOP.
Return:
A-11 REAL APPLICATION = FAIL
with exact reasons/evidence.
No retry.
No new nc3.
No automatic A-12.
No F2-T2.
F2-T1 product:
COMPLETE
Real verification:
EXECUTION_VERIFIED
nc1:
PASS
nc2:
SATISFIED_UNDER_I64_SCOPE_BRIDGE
nc3 ordinary path:
EXHAUSTED
A-11 authority:
Owner SATISFIED
Architect AGREED
Coordinator AGREED
A-11:
ACTIVE FOR ONE REAL F2-T1 APPLICATION
attempts:
1 / 3
F2-T1:
PENDING REAL A-11 APPLICATION
F2-T2:
NOT AUTHORIZED
