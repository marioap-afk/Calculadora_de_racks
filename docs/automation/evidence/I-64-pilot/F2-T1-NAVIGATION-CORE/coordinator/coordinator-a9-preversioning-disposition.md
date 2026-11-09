I-64 — A-9 PRE-VERSIONING STOP DISPOSITION
Current branch:
f1984abaf327e2349bc6005d0883a649b3bc5ea6
A-8:
HISTORICAL
CHANGES REQUIRED
NOT ACTIVE
A-9:
DRAFT / NOT VERSIONED
Coordinator disposition:
SESSION PROPOSAL ACCEPTED WITH A MORE PRECISE FAIL-CLOSED RULE
The literal rule previously ordered:
"if className or method name contains any non-ASCII code point, fail closed"
is TOO BROAD for this bridge.
Repository fact accepted:
RackCad.Tests.ProjectVariableMutationPreflightTests
.Create_AcuñaUnVariableIdNUEVO
contains U+00F1.
It is unrelated to:
FullyQualifiedName~Workspace
and cannot become a Workspace match merely because its non-ASCII character
has unknown OrdinalIgnoreCase behavior.
Therefore the literal all-non-ASCII rejection would make the bridge
inapplicable to the real Core TRX for a reason unrelated to the selected
test set.
Do NOT version A-9 with that literal rule.
A-9 must use:
POTENTIAL-MATCH FAIL-CLOSED SEMANTICS
not:
GLOBAL NON-ASCII REJECTION
The filter substring remains ASCII-only under the existing supported grammar.
For each FullyQualifiedName:
className + "." + methodName
use the following procedure.
CASE 1 — FQN IS ASCII ONLY
Perform the normal supported match:
case-insensitive ASCII substring
with only:
A-Z <-> a-z
No Unicode:
upper()
lower()
casefold()
may decide the match.
CASE 2 — FQN CONTAINS NON-ASCII
Do not immediately fail.
First determine whether the FQN could possibly match the ASCII filter if the
case behavior of every non-ASCII UTF-16 code unit is treated as UNKNOWN.
Perform this analysis over UTF-16 CODE UNITS.
Reason:
.NET string indexing / ordinal comparison operates on UTF-16 code units.
Convert the FQN to its UTF-16 code-unit sequence without normalization.
The ASCII filter is also represented as UTF-16 code units.
For every candidate window having the same number of UTF-16 code units as
the filter:
for each position:
A. If the FQN code unit is ASCII:
compare using ASCII-only case folding.
If it differs from the corresponding filter unit:
this window is impossible.
B. If the FQN code unit is non-ASCII:
treat that position as UNKNOWN / potentially compatible.
Do not apply Unicode case mapping.
If at least one complete window remains potentially compatible:
FAIL CLOSED:
UNSUPPORTED_NON_ASCII_FQN
because an exact .NET OrdinalIgnoreCase decision would matter to whether the
test is selected.
If NO candidate window is potentially compatible:
the FQN is deterministically NOT SELECTED.
Do not fail the whole TRX.
Continue processing.
This is a conservative three-state decision:
ASCII MATCH
ASCII / STRUCTURAL NON-MATCH
UNKNOWN POTENTIAL MATCH
Only:
UNKNOWN POTENTIAL MATCH
fails closed.
A non-ASCII code point outside any potentially matching window does not
invalidate the TRX.
The corrected synthetic suite must prove at least:

1. ASCII ordinary match:

RackCad.Tests.WorkspaceNavigationTests.SomeTest
filter Workspace
=> SELECTED

2. ASCII mixed-case match:

RackCad.Tests.wOrKsPaCeNavigationTests.SomeTest
=> SELECTED

3. long-s potential match:

Work + U+017F + pace
with filter Workspace
=> UNSUPPORTED_NON_ASCII_FQN
because the non-ASCII UTF-16 unit occupies a position that could determine
the match.

4. dotless-i potential match:

F + U+0131 + lter
with filter Filter
=> UNSUPPORTED_NON_ASCII_FQN

5. unrelated non-ASCII:

Create_AcuñaUnVariableIdNUEVO
with filter Workspace
=> NOT SELECTED
and:
NO failure
because no UTF-16 window is even potentially compatible with "Workspace".

6. non-ASCII outside a possible ASCII match:

a name containing unrelated non-ASCII text plus an actual ASCII
"Workspace" substring
=> SELECTED
The unrelated non-ASCII portion must not prevent the known ASCII match.

7. supplementary-plane character / surrogate-pair case:

must be evaluated as TWO UTF-16 code units.
Add at least one synthetic regression proving the verifier does not silently
collapse a surrogate pair into one wildcard position.
The corrected rule must preserve:
when exact Unicode/.NET case behavior could change inclusion in the selected
set, the bridge refuses to decide.
It may only return NOT SELECTED when the surrounding ASCII/code-unit
structure proves that no match is possible regardless of how the non-ASCII
units compare.
This is stricter than guessing Unicode behavior and narrower than rejecting
the entire TRX.
I64-A8-ORDINAL-FILTER-MISMATCH remains ACCEPTED.
A-9 closes it by:

* eliminating Python Unicode case mapping as an authority;
* performing exact ASCII folding where sufficient;
* failing closed whenever unknown non-ASCII ordinal behavior could affect
selection;
* permitting deterministic non-match when Unicode behavior cannot affect
selection.

A-9.md must state this explicitly.
Do not claim to implement full .NET OrdinalIgnoreCase.
The prepared corrections for the other two REQUIRED findings are ACCEPTED
without change:
COUNTERS:

* closed allowlist;
* non-negative integer values;
* total = top-level rows;
* passed / failed / notExecuted derived from rows;
* executed = passed + failed;
* unsupported/non-neutral categories must be zero;
* unknown Counter attribute => fail closed.

EXECUTION IDENTITY:

* executionId mandatory;
* GUID format;
* unique case-insensitively;
* duplicates fail before tallying;
* same testId with distinct executionIds allowed;
* legitimate Theory-like executions remain separate.

Preserve T01-T35.
Preserve the already prepared new counter and executionId tests.
Replace/extend the ordinal tests so they verify the new potential-match
semantics.
The final A-9 suite must include:

* long-s potential match -> FAIL CLOSED;
* dotless-i potential match -> FAIL CLOSED;
* unrelated U+00F1 FQN vs Workspace -> NOT SELECTED / PASS;
* actual ASCII Workspace substring in otherwise non-ASCII FQN -> SELECTED;
* surrogate-pair / two-UTF-16-unit regression;
* ASCII mixed-case control.

All tests:
100% PASS
Run twice:
byte-identical output.
Keep the regression against historical verifier blob:
1691003d2286a3c8f597a182597e6622da074bd3
The regression must continue demonstrating all three Architect findings.
For Q03, demonstrate that historical A-8 incorrectly makes at least one
selection decision using Python Unicode casing where corrected A-9 instead
returns:
UNSUPPORTED_NON_ASCII_FQN
for a potential match.
Do not require historical A-8 to reject the unrelated Acuña-style fixture;
that fixture exists to prove A-9 is not over-broad.
Still do NOT run the bridge against real F2-T1 TRX or handoffs.
Repository source inspection is not bridge execution and does not violate
the pre-activation boundary.
The Acuña repository fact may be recorded as:
APPLICABILITY DISCOVERY
but not as a bridge result.
Now authorized:

1. update the draft verifier with this rule;
2. update the synthetic self-test;
3. update the regression demo;
4. obtain 100% synthetic PASS twice;
5. write I-64-A-9.md;
6. version A-9 + corrected tools + synthetic outputs;
7. commit/push docs/evidence/tools only;
8. exact-SHA CI 4/4.

A-8 remains unchanged.
After the A-9 commit and exact-SHA CI pass:
the previously authorized formal re-review remains authorized.
No new Architect accreditation probe is required if:

* codex-cli binary is unchanged;
* gpt-6.1-sol/high remains available;
* config.toml is A/B/C/D;
* relay/process preconditions are clean.

Architect must specifically determine whether A-9 closes:
I64-A8-ORDINAL-FILTER-MISMATCH
I64-A8-COUNTER-COHERENCE
I64-A8-DUPLICATE-RESULT-FAIL-OPEN
For Q03, the review question is:
"Does the verifier fail closed exactly when non-ASCII UTF-16 units could
affect the ASCII filter result, while allowing a provable non-match without
attempting Unicode case mapping?"
Owner decision remains deferred.
Do not request approval until:
A-9 is versioned
and
Architect = AGREED
The eventual Owner decision applies to A-9, not historical A-8.
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
AUTHORIZED TO COMPLETE AND VERSION
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
