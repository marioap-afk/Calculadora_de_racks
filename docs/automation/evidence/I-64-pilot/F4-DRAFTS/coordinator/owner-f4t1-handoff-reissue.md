I-64 — F4-T1-DRAFT-CORE
F4-E11 DISPOSITION
Current branch:
adc022e6b63ed3ddd70d3cdbe366932404d5971a
Product GREEN:
ce73ceb7
RED:
5a27ac8b
Current product-attempt budget:
3 / 3
Do NOT create F4-T1 Attempt 4.
Do NOT grant a FreeText exception.
Do NOT redefine AUTOMATION_PLAN §16.10.
Instead authorize exactly ONE:
HANDOFF TRANSPORT / EVIDENCE REISSUE
for the existing Attempt 3.
The product attempt remains:
3 / 3
The clean Controller verification ce94 establishes:
13 / 14 checks PASS
The only failure is:
FreeText
The prohibited term appears only because WorkCompleted references an
auxiliary filename containing:
candidate
It is not:

* a product defect;
* a test defect;
* a scope defect;
* a RED/GREEN defect;
* a routing defect;
* an authority defect;
* a CI defect.

N-1 itself is corrected and independently reviewed.
Therefore requiring a fourth product implementation attempt would
misclassify a transport-text defect as product rework.
Reissue ONLY the Worker handoff / delivery record for Attempt 3.
Keep identical:

* TaskId;
* Attempt = 3;
* DelegationRunId;
* Worker RunId identity where the schema permits the corrected handoff to
refer to the same Worker execution;
* BaseSha;
* RedSha = 5a27ac8b;
* CurrentSha = ce73ceb7;
* branch;
* worktree;
* timestamps/evidence derived from the same completed Worker execution;
* scope;
* CI references;
* test results;
* WorkCompleted meaning;
* KnownLimitations;
* OpenQuestions;
* all product facts.

No Git product commit.
No Worker invocation.
No RED.
No GREEN.
No test execution required solely to rewrite the handoff.
The corrected handoff may remove or rewrite ONLY the textual reference to
the auxiliary filename whose basename contains the prohibited §16.10 term.
For example:
instead of naming the auxiliary file,
describe it generically as:
"the temporary comparison copy used during the Worker run"
or omit that implementation-detail reference entirely.
Do not alter the underlying fact being reported.
The reissue must NOT:

* remove a real limitation;
* change any SHA;
* change test counts;
* change CI status;
* change scope;
* change Worker result;
* change product description materially;
* hide a denial/error;
* change evidence to make another Controller check pass.

This authorization exists only for the literal FreeText filename defect.
Preserve:
ce94
exactly as historical evidence:
EXECUTION_REWORK_REQUIRED / REWORK
FailureClass = FreeText
Do not rewrite or replace its output.
Create a separate record stating:
F4T1-HANDOFF-REISSUE-01
Reason:
same completed Worker execution and same product commits;
delivery free text contained a prohibited §16.10 token solely inside an
auxiliary filename.
Record old and new handoff SHA-256 values.
Record a structured diff of the handoff proving the allowed text field is
the only semantic delta.
Before the new Controller verification:
prove:

1. HEAD / remote product SHA unchanged;
2. RED unchanged;
3. GREEN unchanged;
4. exact-SHA CI still references ce73ceb7 and remains green;
5. test counts unchanged;
6. narrow §16.8 hash proof unchanged;
7. independent review result unchanged;
8. corrected handoff passes schema;
9. no §16.10 terms remain in the relevant free-text fields;
10. handoff structured diff contains only the authorized textual sanitation.

If any other field changes:
STOP.
Run ONE Controller verification using:

* same Attempt-3 delegation;
* same product commits;
* same authorities;
* corrected handoff;
* same narrow Tests oracle already authorized.

Expected:
14 / 14 PASS
EXECUTION_VERIFIED / NONE
This is a reverification of the SAME Attempt 3.
It is NOT Attempt 4.
Only if the reverification returns:
EXECUTION_VERIFIED
run nc1, nc2 and nc3 normally.
If all controls close:
F4-T1 = CLOSED / PASS
If the corrected handoff still fails FreeText:
STOP.
Do not sanitize repeatedly.
Do not create another reissue automatically.
If another check fails for an unrelated reason:
STOP and return to Coordinator.
F4-T1 history remains:
Attempt 1 = REWORK
Attempt 2 = REWORK
Attempt 3 = product correction complete
attempts = 3 / 3
No fourth product attempt exists.
The handoff reissue is transport/evidence correction only.
Update future Worker prompts so that §16.10 prohibited terms are treated as
prohibited even when they occur inside:

* filenames;
* paths;
* artifact names;
* temporary-file names.

Do not rename existing historical artifacts merely to clean history.
Proceed:

1. record this disposition;
2. produce one corrected handoff;
3. prove its constrained diff;
4. run one Controller reverification;
5. if VERIFIED, run nc1-nc3;
6. close F4-T1 if controls close;
7. start F4-T2 at attempts = 1 / 3 with N-2 mandatory.

No Owner decision is required merely to perform this one reissue.
