I-64 — COORDINATOR ORDER
A-11 — CORRECTION OF A-10 / I64-TESTFILTER-BRIDGE-01
Current branch:
50ed86fafd2bf8fc579341a02dbe2c0e9c8a0821
Exact-SHA CI:
42634064365
4 / 4 SUCCESS
A-10 review object:
3175bcb9cf382b78aa751c762dcfaf7be4a2787c
Architect review:
R20261109T141352Z-75ac
Verdict:
CHANGES REQUIRED
Prior findings:
I64-A9-TESTID-GUID-ALIAS = CLOSED
I64-A9-FAILURE-MESSAGE-AMBIGUITY = CLOSED
A-8 findings:
REMAIN CLOSED
New REQUIRED:

1. I64-A10-TESTRUN-ROOT-ANCHOR
2. I64-A10-SHADOW-RUN-PASS

Optional:
0
Both REQUIRED findings are ACCEPTED.
They are treated as ONE ROOT CAUSE:
the consumed TRX structure is not anchored to one unique actual TestRun root.
Neither finding requires:

* product changes;
* Freeze changes;
* global I-61 / AUTOMATION_PLAN changes;
* Owner scope decisions;
* processing real F2-T1 evidence.

A-10 remains:
HISTORICAL
CHANGES REQUIRED
NOT ACTIVE
Do not edit A-10 in place.
Create:
I-64-A-11.md
A-11 corrects / supersedes A-10 FOR ACTIVATION ONLY.
A-8, A-9 and A-10 remain immutable historical evidence.
Materiality remains:
M-01 = NO
M-02 = NO
M-03 = NO
M-04 = YES
M-05 = NO
M-06 = NO
M-07 = NO
M-08 = NO
Authority:
Owner + Architect + Coordinator
Owner:
PENDING
Architect:
REQUIRED
Coordinator:
PENDING FINAL AGREEMENT
Status until all three agree:
PROPOSED / NOT ACTIVE
The parser must establish a single structural root before reading any
definitions, results, counters or failure messages.
Required invariant:
THE DOCUMENT ELEMENT ITSELF must be the unique supported TestRun root.
Do not search for a usable TestRun descendant.
Do not choose the first TestRun.
Do not tolerate a nested TestRun.
Do not allow shadow copies of consumed containers outside the actual root
structure.
After parsing XML:

1. document.getroot() must exist;
2. its local name must be exactly:

TestRun

3. its namespace must be exactly the supported TRX namespace;
4. there must be NO descendant element, at any depth, whose local name is:

TestRun
other than the document root itself.
Any nested TestRun:
FAIL CLOSED
with a specific diagnostic such as:
TRX_NESTED_TESTRUN
If the document root is not the supported TestRun:
FAIL CLOSED
with:
TRX_ROOT_INVALID
or equivalent specific code.
Every consumed structural element must belong to the unique root's expected
parent chain.
At minimum enforce:
TestRun
-> TestDefinitions
-> UnitTest
-> TestMethod
TestRun
-> Results
-> UnitTestResult
TestRun
-> ResultSummary
-> Counters
For Failed result composition:
TestRun
-> Results
-> UnitTestResult
-> Output
-> ErrorInfo
-> Message
Only explicitly supported non-consumed structures such as the accepted
TextMessages location may exist outside the consumed failure path according
to the existing A-10 rule.
Do not validate consumed elements merely by checking their immediate parent
tag in isolation.
Their ancestry must terminate at the actual document root.
Before tallying anything, traverse the entire XML document.
For each local-name consumed by the bridge:
TestDefinitions
UnitTest
TestMethod
Results
UnitTestResult
ResultSummary
Counters
Output
ErrorInfo
Message
classify its structural location.
If a consumed-name element appears outside an explicitly authorized parent
chain:
FAIL CLOSED:
TRX_STRUCTURE_AMBIGUOUS
or a more specific structural code.
This includes:

* consumed structures inside a nested TestRun;
* duplicate/shadow Results containers elsewhere;
* shadow TestDefinitions;
* extra ResultSummary/Counters structures;
* misplaced UnitTestResult;
* failure-path nodes under an unrelated branch.

No ignored copy of consumed evidence may coexist with the evidence being
counted.
For the actual TestRun root require exactly:
1 TestDefinitions
1 Results
1 ResultSummary
and exactly:
1 Counters
under ResultSummary.
Zero or multiple:
FAIL CLOSED.
Do not rely on root.find()/findall() selecting only the intended first
container without proving cardinality.
Add the exact behavioral regression requested by the Architect.
Starting from the known PASS synthetic GREEN fixture:

* preserve outer handoff;
* preserve expected counts;
* preserve outer Counters;
* append a full nested TestRun;
* inside that nested TestRun place a selected Failed UnitTestResult whose
FQN would match the RequiredTests filter.

Expected corrected A-11 result:
FAIL CLOSED
with the structural root/nested-run diagnostic.
The hidden row must never simply disappear from tallying.
Add synthetic cases at minimum:

1. valid single root -> PASS;
2. nested TestRun under root -> FAIL;
3. nested TestRun containing selected Failed result -> FAIL;
4. nested TestRun containing apparently harmless Passed result -> FAIL;
5. second Results container under root -> FAIL;
6. second TestDefinitions under root -> FAIL;
7. second ResultSummary under root -> FAIL;
8. Counters outside ResultSummary -> FAIL;
9. UnitTestResult outside root/Results chain -> FAIL;
10. UnitTest outside root/TestDefinitions chain -> FAIL;
11. Output/ErrorInfo/Message copied into an unrelated branch -> FAIL;
12. foreign-namespace shadow TestRun or consumed structure:
fail according to the existing namespace/structure policy;
13. document root not TestRun -> FAIL;
14. one valid TestRun with all prior A-10 structures -> PASS.

This second REQUIRED is CLOSED by the SAME parser correction.
Do not implement a separate special-case fix just for T01.
A-11.md must state:
I64-A10-SHADOW-RUN-PASS is the observable PASS consequence of
I64-A10-TESTRUN-ROOT-ANCHOR.
Both are corrected by establishing and enforcing one root-anchored consumed
structure before counting.
Preserve unchanged:

* potential-match UTF-16 rule;
* ASCII-only case folding;
* Counters coherence;
* executionId GUID identity and uniqueness;
* UnitTest/testId GUID canonical identity;
* failure-message cardinality;
* ChainRedFiles;
* A-4 hardened Git;
* A-6 parsing/exact-comparison/term semantics;
* mutation exactness.

Do not reopen prior CLOSED findings unless a synthetic regression now proves
one is broken.
Preserve all A-10 cases.
Add root-anchor/shadow-run cases.
Final suite:
100% PASS
Run twice.
Outputs:
byte-identical.
Use exact historical A-10 verifier blob:
b134ba653b6f47871c6fc3ae7e15357487c6c88a
Demonstrate specifically that historical A-10:

* accepts the nested TestRun shadow fixture;
* ignores at least one consumed shadow structure;
* allows the selected hidden failure to escape counting.

Corrected A-11 must reject those exact fixtures with structural diagnostics.
Do not rewrite A-10 historical results.
During A-11 development and review:
DO NOT process:

* real GREEN TRX;
* real RED TRX;
* real Worker handoff;
* real nc3 mutated handoff.

Only synthetic fixtures.
Repository source inspection remains allowed.
Create:
docs/initiatives/I-64-A-11.md
It must state:
Corrects / supersedes for activation:
I-64-A-10
Historical:
A-8
A-9
A-10
remain immutable and inactive.
Cite exact Architect review:
R20261109T141352Z-75ac
Map both REQUIRED findings to the shared root-cause correction and its exact
regressions.
Record final blobs and SHA-256 values after versioning.
The existing Architect cell remains accredited:
gpt-6.1-sol
high
read-only
codex-cli 0.167.0-alpha.3.2
No new probe required if:

* binary unchanged;
* config.toml remains A/B/C/D;
* no usage/credit warning;
* relay/process checks clean.

If CLI binary changes:
STOP.
After:

* A-11 versioned;
* corrected verifier versioned;
* synthetic self-tests 100% PASS twice;
* regression against A-10 versioned;
* exact-SHA CI 4/4;

ONE formal Architect review is authorized.
No additional Coordinator round is needed before that review.
The review must classify:
I64-A10-TESTRUN-ROOT-ANCHOR
I64-A10-SHADOW-RUN-PASS
as:
CLOSED
or retained REQUIRED.
It must also confirm:

* A-9 findings remain closed;
* A-8 findings remain closed;
* M-04 unchanged;
* parser remains fail closed;
* no real occurrence evidence was processed;
* no global protocol authority;
* ChainRedFiles and mutation semantics unchanged.

If Architect = CHANGES REQUIRED:
STOP.
Do NOT create A-12 automatically.
Return exact findings to Coordinator.
If Architect = AGREED:
STOP at:
OWNER_A11_DECISION_REQUIRED
Do not execute the real bridge.
Owner approval remains deferred until Architect AGREED.
If Architect agrees, request exactly:
SATISFIED — authorize I-64-A-11 / I64-TESTFILTER-BRIDGE-01 as the corrected
I-64-only substitute evidence path for the exhausted nc3 Tests-filter
protocol defect, superseding A-8, A-9 and A-10 for activation, without
changing product semantics or global protocol.
Do not infer this decision.
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
A-8:
HISTORICAL / CHANGES REQUIRED / NOT ACTIVE
A-9:
HISTORICAL / CHANGES REQUIRED / NOT ACTIVE
A-10:
HISTORICAL / CHANGES REQUIRED / NOT ACTIVE
A-11:
AUTHORIZED TO PREPARE
NOT ACTIVE
Architect cell:
ACCREDITED
Owner:
PENDING AFTER ARCHITECT AGREED
attempts:
1 / 3
F2-T1:
BLOCKED_PROTOCOL_DEPENDENCY
F2-T2:
NOT AUTHORIZED
