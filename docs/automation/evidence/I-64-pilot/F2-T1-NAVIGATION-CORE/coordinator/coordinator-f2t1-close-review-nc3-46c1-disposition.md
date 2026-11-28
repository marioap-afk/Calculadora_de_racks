I-64 — F2-T1 CLOSE REVIEW / nc3 DISPOSITION
Task:
F2-T1-NAVIGATION-CORE
Custody commit:
82bbac8fb5837e53e057f95ba8715df31b5ec01b
Exact-SHA CI:
42564737431
Result:
4 / 4 SUCCESS
Custody:
ACCEPTED AS INTERMEDIATE EVIDENCE
F2-T1:
NOT CLOSED YET
Worker RED:
f2761979b89e784ed16220815e250d21bc476aa1
Worker GREEN:
c726410f3ac764f4a80a08a2b810b5fdda276ecc
Real verification:
R20261108T221347Z-ebfc
Status:
EXECUTION_VERIFIED / NONE
nc4:
PASS
nc1:
R20261109T032437Z-46b5
CONTROL PASS
nc2:
R20261109T043701Z-0706
CONTROL FAIL / HISTORICAL CONTROLLER SCOPE DEFECT
A-4:
I64-SCOPE-BRIDGE-01
R20261109T044628Z-34cd
PASS
Therefore:
nc2 = SATISFIED_UNDER_I64_SCOPE_BRIDGE
All of the above is accepted.
RunId:
R20261109T044704Z-46c1
Mechanical oracle:
MET
Observed:
FreeText = fail
FailureClass = FreeText
Classification = EXECUTION_REWORK_REQUIRED
Disposition = REWORK
Checks before FreeText:
match the accepted real verification.
Therefore the Controller successfully detected the injected handoff term.
However:
the Controller verification output itself contains the literal restricted
phrase:
GATE PASS
inside:

* Findings
* FreeText Evidence

AUTOMATION_PLAN 16.10 explicitly assigns the Coordinator a manual scan of
verification free text.
That manual scan therefore finds a restricted term in the verification
artifact itself.
Disposition:
46c1 = INVALID_OUTPUT FOR CONTROL ACCEPTANCE
The product/handoff mutation behaved correctly.
The problem is the Controller's own verification free text.
Do NOT rewrite 46c1.
Preserve it exactly.
A-6 / I64-FREETEXT-BRIDGE-01 is NOT applicable.
The historical A-6 condition was:
the Controller failed to produce the expected FreeText oracle.
That did NOT happen here.
46c1 correctly produced:
FreeText = fail
REWORK
So the known nc3 protocol defect did not reproduce.
The issue is different:
the Controller echoed the prohibited token in its own verification prose.
Do not use A-6 by analogy.
ONE nc3 retry is AUTHORIZED.
This is:
CONTROL INVALID_OUTPUT recovery
not:
REWORK of product
not:
Worker correction
not:
new attempt
Use:

* same verified delivery;
* same nc3 handoff mutation;
* same launcher;
* same normative verification criteria;
* new RunId.

Do NOT change:

* RED;
* GREEN;
* Worker handoff except for the existing nc3 mutation;
* delegation;
* contract;
* product.

The retry must detect the restricted term without repeating it literally in
verification free text.
Acceptable wording:
"the injected restricted 16.10 term"
or:
"a prohibited gate term"
Do NOT quote or reproduce the actual token in:

* Evidence;
* Findings;
* UnexpectedFindings;
* KnownLimitations;
* Deviations;
* OpenQuestions;
* RecommendedNextAction.

The mutated Worker handoff still contains the real injected token.
The Controller must detect it there.
The verification output must only describe the detection neutrally.
Required:
Termination = pass
Handoff = pass
Authority = pass
Contract = pass
Identity = pass
Remote = pass
Scope = pass
CleanTree = pass
Ci = pass
Tests = pass
Trailer = pass
Routing = pass
FreeText = fail
FailureClass = FreeText
Classification = EXECUTION_REWORK_REQUIRED
Disposition = REWORK
Denials = pass
Coordinator manual 16.10 scan of the verification output:
NO MATCHES
If all of the above is true:
nc3 = CONTROL PASS
The statement that all 51 RED failures are assertion failures is imprecise.
Measured split remains:
50 xUnit assertion failures
1 NullReferenceException inside the new RED test surface
This imprecision is:
NON-BLOCKING FOR nc3
because:

* the accepted real verification already preserves the correct 50 + 1 facts;
* it does not affect the FreeText oracle;
* it does not change the GREEN result.

Preserve the accurate 50 + 1 split in custody.
Do not rewrite historical Controller output.
attempts:
1 / 3
unchanged.
nc1:
closed PASS
nc2:
satisfied under A-4
nc3:
46c1 = INVALID_OUTPUT
Authorized:
one nc3 retry
No Worker invocation.
No product correction.
No attempt increment.
If the retry again fails the manual 16.10 scan or misses the relative oracle:
STOP to Coordinator.
Do not automatically spend another retry.
Commit:
82bbac8fb5837e53e057f95ba8715df31b5ec01b
remains valid custody of all evidence through 46c1.
Do not rewrite or amend that historical evidence.
After the nc3 retry:
add only the new control evidence, Coordinator disposition and updated state
in the next documentation/evidence custody commit.
The recorded debt:
"AllowedWriteScope (2 entradas)"
is CONFIRMED for I-62 handoff.
Keep it as:
PROTOCOL / PROMPT DEBT
It does not block F2-T1 closure once nc3 passes.
No normative prompt change during this chain.
F2-T1 product:
COMPLETE
Real verification:
EXECUTION_VERIFIED
nc4:
PASS
nc1:
PASS
nc2:
SATISFIED_UNDER_I64_SCOPE_BRIDGE
nc3:
PENDING ONE VALID RETRY
F2-T1:
NOT CLOSED
F2-T2:
NOT AUTHORIZED
Run exactly one nc3 retry.
If:
relative oracle = MET
and:
Coordinator manual 16.10 scan of the verification output = EMPTY
then:
nc3 = CONTROL PASS
After that:
custody/update state
exact-SHA CI
return at:
COORDINATOR_F2_T1_CLOSE_REQUIRED
If not:
STOP and return.
