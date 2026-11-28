I-64 — COORDINATOR ORDER
A-9 — CORRECTION OF A-8 / I64-TESTFILTER-BRIDGE-01
Current branch:
f1984abaf327e2349bc6005d0883a649b3bc5ea6
Exact-SHA CI:
42574267416
4 / 4 SUCCESS
Architect accreditation probe:
R20261109T062522Z-e8be
PASS
Architect review:
R20261109T062937Z-e1d0
CHANGES REQUIRED
Required findings:

1. I64-A8-ORDINAL-FILTER-MISMATCH
2. I64-A8-COUNTER-COHERENCE
3. I64-A8-DUPLICATE-RESULT-FAIL-OPEN

Optional findings:
0
All three REQUIRED findings are ACCEPTED.
None requires:

* a product change;
* a Freeze change;
* a global I-61 / AUTOMATION_PLAN change;
* a new product architecture decision;
* use of real F2-T1 evidence.

A-8 remains:
HISTORICAL
PROPOSED
NOT ACTIVE
CHANGES REQUIRED
Do not edit A-8 in place.
Create:
I-64-A-9.md
A-9 corrects / supersedes A-8 FOR ACTIVATION ONLY.
A-8 remains immutable historical evidence.
Applies-to:
I-64 only
Materiality:
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
The real bridge remains forbidden until activation.
Do NOT attempt to emulate full Unicode .NET OrdinalIgnoreCase in Python.
Use the conservative fail-closed correction.
For every TRX test definition used to construct a FullyQualifiedName:
className
and
method name
must contain ASCII characters only.
If either contains any code point > U+007F:
FAIL CLOSED
with a specific diagnostic such as:
UNSUPPORTED_NON_ASCII_FQN
No test with a non-ASCII class or method name may participate in selection.
For supported ASCII FQNs:
construct:
className + "." + methodName
and perform case-insensitive substring matching using ASCII-only case
folding:
A-Z <-> a-z only.
Do not use:
str.upper()
str.lower()
casefold()
as the authority for Unicode comparison.
The filter expression remains limited to the A-8 supported
FullyQualifiedName~<substring> form.
Add synthetic regressions proving:

* U+017F LATIN SMALL LETTER LONG S cannot create a match;
* U+0131 LATIN SMALL LETTER DOTLESS I cannot create a match;
* ordinary ASCII mixed-case matching still passes;
* the actual Workspace-style ASCII fixture still selects the intended rows.

The long-s and dotless-i cases must FAIL CLOSED because the FQN is
unsupported, not because Python happened to compare them a particular way.
The TRX parser must validate Counters against the actual top-level
UnitTestResult rows before any selection result can PASS.
For the outcomes supported by this bridge:
Passed
Failed
NotExecuted
derive row counts directly.
Required relationships:
total =
number of top-level UnitTestResult rows
passed =
number of Passed rows
failed =
number of Failed rows
notExecuted =
number of NotExecuted rows
executed =
passed + failed
because NotExecuted rows are not executed.
The parser must define an explicit allowlist of TRX Counter attributes it
supports.
For every supported category not represented by the bridge's accepted
outcomes:
its value must be zero.
Examples include categories such as error/timeout/aborted/inconclusive and
other non-Passed/Failed/NotExecuted states supported by the TRX Counters
format.
If a supported non-neutral category is nonzero:
FAIL CLOSED.
If a Counter attribute exists that the verifier does not understand:
FAIL CLOSED
rather than silently ignore it.
All counter values must:

* parse as non-negative integers;
* be internally coherent;
* be coherent with the result rows.

Add synthetic regressions at minimum:

* executed = 0 with Passed rows -> FAIL;
* total mismatch -> FAIL;
* passed mismatch -> FAIL;
* failed mismatch -> FAIL;
* notExecuted mismatch -> FAIL;
* an unsupported/non-neutral counter category nonzero -> FAIL;
* coherent Passed/Failed/NotExecuted counters -> PASS.

Do not weaken the existing malformed-TRX fail-closed rules.
Each top-level UnitTestResult must have an unambiguous execution identity.
Use:
executionId
as the execution-record identity.
Requirements:

* executionId must exist;
* it must parse in the format expected by TRX;
* it must be unique across all top-level UnitTestResult rows.

Duplicate executionId:
FAIL CLOSED
before tallying selection or MinSelected.
Do NOT deduplicate duplicates silently.
testId is NOT an execution identity.
Multiple legitimate executions / Theory cases may share the same testId if
they have distinct executionId values.
Therefore add synthetic regressions proving:

* duplicate executionId -> FAIL;
* duplicate executionId cannot inflate MinSelected;
* same testId + distinct executionId values -> allowed;
* multiple Theory-like executions with distinct executionId values remain
separate selected rows;
* missing executionId -> FAIL;
* malformed executionId -> FAIL.

Preserve all existing A-8 synthetic cases.
Add new cases for all three REQUIRED findings.
The new self-test package must include at least:

* long-s non-ASCII FQN;
* dotless-i non-ASCII FQN;
* ASCII mixed-case control;
* executed mismatch;
* other counter mismatch / nonzero unsupported category;
* duplicate executionId;
* missing executionId;
* malformed executionId;
* same testId with distinct executionIds PASS.

The complete suite must:
PASS 100%
on the corrected A-9 verifier.
Run it twice.
The two outputs must be byte-identical.
Add a regression demonstration against the exact historical A-8 verifier
blob:
1691003d2286a3c8f597a182597e6622da074bd3
The regression package must demonstrate that the A-8 verifier fails to
protect against the three Architect findings.
At minimum demonstrate:

* one non-ASCII ordinal mismatch case;
* one incoherent Counters case;
* one duplicate executionId case.

Expected:
historical A-8 verifier = inadequate on those signature cases
corrected A-9 verifier = PASS on the self-test oracle by rejecting the bad
fixtures as required.
Do not rewrite historical A-8 results.
During A-9 development and self-test:
DO NOT process:

* real GREEN TRX;
* real RED TRX;
* real Worker handoff;
* real nc3 mutated handoff.

Continue using synthetic fixtures only.
Real occurrence identifiers may remain documented by hash/binding as in A-8,
but their bytes must not be fed to the verifier before activation.
A-9 may continue to reuse:

* A-4 hardened Git execution semantics;
* A-6 handoff/schema/exact-comparison/16.10 term semantics

exactly as A-8 did.
Do not execute A-4 or A-6 against the real F2-T1 occurrence as part of A-9
development.
A-9 remains a distinct amendment.
I-64-A-9.md must identify:
Corrects / supersedes for activation:
I-64-A-8
Historical A-8 remains immutable.
It must cite the exact Architect review:
R20261109T062937Z-e1d0
and map each REQUIRED finding to the exact correction and synthetic
regression.
The document must contain the new tool blobs and SHA-256 values after
versioning.
Do NOT request the Owner to approve historical A-8 now.
Owner approval should apply to the corrected activation object.
After A-9 is complete and Architect AGREED, request this exact decision:
SATISFIED — authorize I-64-A-9 / I64-TESTFILTER-BRIDGE-01 as the corrected
I-64-only substitute evidence path for the exhausted nc3 Tests-filter
protocol defect, superseding A-8 for activation, without changing product
semantics or global protocol.
No Owner decision is inferred before that.
The Architect cell is now accredited:
gpt-6.1-sol
high
read-only
codex-cli 0.167.0-alpha.3.2
No new accreditation probe is required for the A-9 re-review IF:

* the exact CLI binary remains unchanged;
* config.toml still matches A/B/C/D;
* no usage/credit warning invalidates the cell;
* the relay preconditions remain clean.

If the CLI binary changes:
STOP and return before review.
After:

* A-9 document is versioned;
* corrected verifier is versioned;
* updated synthetic self-tests are 100% PASS;
* regression demonstration is versioned;
* exact-SHA CI of that documentation/tool commit is 4/4;

one formal Architect re-review is AUTHORIZED.
No additional Coordinator round is required before that review.
Use the same accredited:
gpt-6.1-sol / high / read-only
cell.
Review object:
the exact A-9 commit.
The Architect must specifically close or retain:
I64-A8-ORDINAL-FILTER-MISMATCH
I64-A8-COUNTER-COHERENCE
I64-A8-DUPLICATE-RESULT-FAIL-OPEN
and re-check:

* M-04;
* fail-closed behavior;
* no real F2-T1 evidence before activation;
* no global protocol authority;
* exact mutation semantics;
* ChainRedFiles derivation.

Expected verdict:
AGREED
or
CHANGES REQUIRED
No implicit activation.
If Architect = CHANGES REQUIRED:
STOP.
Do not produce A-10 automatically.
Return findings to Coordinator.
If Architect = AGREED:
STOP at:
OWNER_A9_DECISION_REQUIRED
unless the Owner has already explicitly provided the A-9 decision text.
If Owner = SATISFIED:
return to Coordinator for final A-9 activation.
Coordinator AGREED is NOT granted in advance.
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
HISTORICAL
CHANGES REQUIRED
NOT ACTIVE
A-9:
AUTHORIZED TO PREPARE
NOT ACTIVE
Architect cell:
ACCREDITED
Owner A-9:
PENDING
attempts:
1 / 3
F2-T1:
BLOCKED_PROTOCOL_DEPENDENCY
F2-T2:
NOT AUTHORIZED
