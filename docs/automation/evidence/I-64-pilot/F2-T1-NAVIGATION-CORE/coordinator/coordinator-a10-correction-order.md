I-64 — COORDINATOR ORDER
A-10 — CORRECTION OF A-9 / I64-TESTFILTER-BRIDGE-01
Current branch:
a78b6d5529f2b045ec24330e8252b49b0bf9bc5d
Exact-SHA CI:
42610063430
4 / 4 SUCCESS
A-9 review object:
1a2fd4e92c38a2b5bcc17b7f09b86342fa7ef2e2
Architect review:
R20261109T112612Z-04e8
Verdict:
CHANGES REQUIRED
Prior A-8 findings:
I64-A8-ORDINAL-FILTER-MISMATCH = CLOSED
I64-A8-COUNTER-COHERENCE = CLOSED
I64-A8-DUPLICATE-RESULT-FAIL-OPEN = CLOSED
New REQUIRED:

1. I64-A9-TESTID-GUID-ALIAS
2. I64-A9-FAILURE-MESSAGE-AMBIGUITY

Optional:
0
Both new REQUIRED findings are ACCEPTED.
Neither requires:

* product changes;
* Freeze changes;
* global I-61 / AUTOMATION_PLAN changes;
* Owner scope decisions;
* real F2-T1 bridge execution.

A-9 remains:
HISTORICAL
CHANGES REQUIRED
NOT ACTIVE
Do not edit A-9 in place.
Create:
I-64-A-10.md
A-10 corrects / supersedes A-9 FOR ACTIVATION ONLY.
A-8 and A-9 remain immutable historical evidence.
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
Status until all three authorities agree:
PROPOSED / NOT ACTIVE
A-10 must treat UnitTest definition identity as GUID identity, not raw string
identity.
For every:
UnitTest@id
UnitTestResult@testId
requirements:

1. value is required;
2. value must parse as a GUID;
3. GUID identity is canonicalized before:
   * duplicate detection;
   * dictionary lookup;
   * reference resolution.

Canonical identity must be independent of:

* hexadecimal case;
* accepted textual GUID casing.

Do NOT rely on raw string equality.
Two UnitTest definitions whose ids differ only by hexadecimal case but parse
to the same GUID:
FAIL CLOSED
with a specific diagnostic such as:
TRX_TEST_ID_DUPLICATE
A UnitTestResult@testId must resolve through canonical GUID identity.
Unknown canonical testId:
FAIL CLOSED.
Malformed UnitTest@id:
FAIL CLOSED.
Malformed UnitTestResult@testId:
FAIL CLOSED.
Do not silently normalize conflicting duplicate definitions into one.
Add synthetic cases at minimum:

1. duplicate UnitTest ids differing only by hex capitalization -> FAIL;
2. conflicting TestMethod definitions under GUID-equivalent ids -> FAIL;
3. UnitTestResult@testId with different textual casing from its definition ->
PASS and resolves to that definition;
4. malformed UnitTest@id -> FAIL;
5. malformed UnitTestResult@testId -> FAIL;
6. unknown but well-formed testId -> FAIL;
7. distinct GUIDs remain distinct;
8. legitimate multiple executions sharing the same testId continue to PASS
when executionIds are distinct.

The correction must preserve the already accepted executionId rules from A-9.
A-10 must make failure-message extraction fail closed.
For each Failed UnitTestResult that is used for RED failure composition:
validate cardinality before reading the message.
The consumed path must have exactly:
1 Output
1 ErrorInfo
1 Message
unless the TRX schema/fixture explicitly allows absence for a case the bridge
does not consume.
For the RED failure-composition path used by this bridge:

* duplicate Output -> FAIL;
* duplicate ErrorInfo -> FAIL;
* duplicate Message -> FAIL;
* contradictory parallel failure structures -> FAIL;
* missing required Message -> FAIL;
* ambiguous nested structure -> FAIL.

Do NOT use Element.find() as authority when multiple matching nodes can exist.
Enumerate the relevant nodes and validate cardinality explicitly.
Only after cardinality is proven may the verifier classify the failure as:

* xUnit assertion;
* NullReferenceException;
* or another supported/expected category.

Add synthetic cases at minimum:

1. exactly one Output/ErrorInfo/Message -> PASS;
2. duplicate Message, second contradictory -> FAIL;
3. duplicate Message, same text -> FAIL;
4. duplicate ErrorInfo -> FAIL;
5. duplicate Output -> FAIL;
6. missing Message -> FAIL;
7. second contradictory exception type -> FAIL;
8. unchanged Counters/rows with ambiguous failure structure -> still FAIL;
9. expected 50 assertion + 1 NRE fixture remains PASS when unambiguous.

No ambiguity may be resolved by "first node wins".
Preserve unchanged:

* UTF-16 potential-match fail-closed semantics;
* ASCII-only folding;
* Counters coherence;
* executionId GUID requirement and uniqueness;
* ChainRedFiles derivation;
* A-4 hardened Git behavior;
* A-6 parsing/exact-comparison/16.10 primitives;
* mutation exactness;
* no real evidence before activation.

Do not reopen the three CLOSED A-8 findings unless a regression demonstrates
a real break.
Preserve all existing A-9 cases.
Add the new GUID-alias and failure-cardinality regressions.
Final suite:
100% PASS.
Run twice.
Outputs:
byte-identical.
Use the exact historical A-9 verifier blob:
39d30be33cda7c947ab4ec509ee30501ca89db49
Demonstrate at minimum:

* historical A-9 accepts GUID-alias duplicate definitions that A-10 rejects;
* historical A-9 accepts duplicate/ambiguous failure-message structures that
A-10 rejects.

Corrected A-10 must reject those fixtures with specific fail-closed codes.
Do not rewrite historical A-9 outputs.
During A-10 development:
DO NOT process:

* real GREEN TRX;
* real RED TRX;
* real Worker handoff;
* real nc3 mutated handoff.

Only synthetic fixtures.
Real occurrence references may remain documented by hashes.
I-64-A-10.md must state:
Corrects / supersedes for activation:
I-64-A-9
Historical A-8 and A-9 remain immutable.
It must cite exact Architect review:
R20261109T112612Z-04e8
It must map:
I64-A9-TESTID-GUID-ALIAS
I64-A9-FAILURE-MESSAGE-AMBIGUITY
to:

* exact code correction;
* exact synthetic regression;
* resulting fail-closed behavior.

Record new tool blobs and SHA-256 after versioning.
The existing Architect cell remains accredited:
gpt-6.1-sol
high
read-only
codex-cli 0.167.0-alpha.3.2
No new probe is required if:

* binary is unchanged;
* config.toml remains A/B/C/D;
* no usage/credit warning appears;
* relay/process preconditions remain clean.

If the CLI changes:
STOP.
After:

* A-10 document versioned;
* corrected verifier versioned;
* synthetic self-tests 100% PASS twice;
* regression demo versioned;
* exact-SHA CI 4/4;

one formal Architect review is AUTHORIZED.
No additional Coordinator round is required before that review.
The review object is the exact A-10 commit.
Architect must specifically classify:
I64-A9-TESTID-GUID-ALIAS
I64-A9-FAILURE-MESSAGE-AMBIGUITY
as:
CLOSED
or retain them with REQUIRED findings.
Also re-check:

* prior CLOSED A-8 findings remain closed;
* M-04;
* fail-closed parsing;
* no real evidence before activation;
* no global protocol authority;
* ChainRedFiles derivation;
* mutation exactness.

Expected verdict:
AGREED
or
CHANGES REQUIRED
If Architect = CHANGES REQUIRED:
STOP.
Do not create A-11 automatically.
Return findings to Coordinator.
If Architect = AGREED:
STOP at:
OWNER_A10_DECISION_REQUIRED
Do not execute the real bridge yet.
Owner approval remains deferred until Architect AGREED.
If Architect agrees, request exactly:
SATISFIED — authorize I-64-A-10 / I64-TESTFILTER-BRIDGE-01 as the corrected
I-64-only substitute evidence path for the exhausted nc3 Tests-filter
protocol defect, superseding A-8 and A-9 for activation, without changing
product semantics or global protocol.
Do not infer Owner SATISFIED from prior messages.
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
