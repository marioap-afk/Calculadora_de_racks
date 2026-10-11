# I-64 — Orden del Coordinator: recuperación autoritativa del control FreeText (A-5)

Transcripción literal recibida por el chat de la sesión responsable el 2026-10-11, tras el STOP P-04 del nc3 de F1-T2-BRIDGE.
La decisión del Owner que exige está en `owner-decision-a5-freetext.md`.

```text
I-64 — F1-T2-BRIDGE
AUTHORITATIVE FREETEXT CONTROL RECOVERY
CURRENT PRODUCT SHA:
e2f642b86197441eea0838c8055c1db666606f11
PRODUCT VERIFICATION:
R20261010T225551Z-41fd
EXECUTION_VERIFIED / NONE
Preserve permanently:
R20261011T012027Z-9c23
= INVALID_OUTPUT
R20261011T014150Z-3200
= INVALID_OUTPUT
R20261011T014542Z-0c58
= INVALID_OUTPUT / P-04
Do NOT rerun nc3.
Do NOT relabel any of these PASS.
Do NOT reinterpret EXECUTION_BLOCKED/STOP as the README §10 oracle.
Controller nc3 remains:
NOT PASSED
This recovery expands the existing I-64 emergency protocol exception beyond
Scope/nc2.
Therefore do NOT claim the prior Scope-specific Owner mandate alone covers
A-5.
Record the following Owner decision only if supplied through the Owner
channel:
OWNER DECISION:
For I-64 only, I authorize a unit-scoped mechanical substitute for the
FreeText negative-control obligation after:

* the real delivery was independently EXECUTION_VERIFIED;
* all permitted nc3 executions were exhausted;
* the model consistently detected FreeText correctly but transformed the
synthetic REWORK result using the real task's STOP/correction rules.

This authority:

* applies only to I-64;
* does not modify global I-61 text;
* does not make historical nc3 executions PASS;
* does not waive FreeText enforcement;
* does not apply to another negative control;
* expires when an integrated protocol authority supersedes it.

Without this explicit Owner decision:
STOP OWNER_DECISION_REQUIRED.
After Owner authority is explicit, create:
docs/initiatives/I-64-A-5.md
Append-only.
Do NOT edit A-1..A-4.
Title:
I-64 Amendment A-5 — Mechanical FreeText negative-control substitute
(I64-FREETEXT-BRIDGE-01)
Freeze:
commit =
9b43dafbe3b8874aa0ba5a85d62cbf953dbdd311
path =
docs/initiatives/I-64-proposal-v5.md
blob =
dc1924ff5a73a4e529fbc5f995621d29351a091b
Applies-to:
I-64
Authority:
Owner + Architect + Coordinator
Materiality:
M-01 = NO
M-02 = NO
M-03 = NO
M-04 = YES
M-05 = NO
M-06 = NO
M-07 = NO
M-08 = NO
M-04 rationale:
A-5 changes how I-64 may satisfy the mandatory nc3 FreeText negative-control
closure obligation after the model-based control mechanism is exhausted.
It does not change product behavior.
README §10 remains authoritative globally.
Historical/model nc3 oracle remains:
FreeText = fail
FailureClass = FreeText
EXECUTION_REWORK_REQUIRED / REWORK
A-5 does not rewrite that rule.
It permits I-64 to satisfy the negative-control obligation with independent
mechanical evidence after the model-based nc3 route is exhausted.
Implement a deterministic, fail-closed verifier.
Suggested path:
docs/automation/evidence/I-64-protocol-bridge/tools/
verify_freetext_bridge.py
Python standard library only.
The verifier receives:
--real-handoff
--mutated-handoff
--term-set
The authoritative term set must be derived mechanically from the exact
AUTOMATION_PLAN §16.10 authority used by the verified task.
Do NOT hard-code a reduced ad-hoc subset unless A-5 explicitly freezes the
full authoritative set.
FAIL on any ambiguity, malformed evidence or unexpected difference.
Require:

1. both inputs are strict UTF-8 JSON;
2. duplicate JSON keys are rejected;
3. NaN / Infinity / invalid Unicode are rejected;
4. both schemas are rackcad-worker-handoff/v1;
5. TaskId identical;
6. RunId identical;
7. Initiative identical;
8. Gate identical;
9. Attempt identical;
10. BaseSha identical;
11. RedSha identical;
12. CurrentSha identical;
13. Branch identical;
14. Worktree identical;
15. FilesChanged identical;
16. all other fields byte/structure-equivalent except the authorized
WorkCompleted mutation;
17. the REAL handoff contains no forbidden gate term in any field governed
by AUTOMATION_PLAN §16.10;
18. the MUTATED handoff differs only in WorkCompleted;
19. the MUTATED WorkCompleted contains at least one exact forbidden term;
20. identify the introduced term(s);
21. all governed FreeText fields other than WorkCompleted remain identical;
22. no forbidden term disappears from another field to compensate;
23. no additional mutation exists.

For this T2 historical application specifically:
the mutation must be exactly the already-custodied nc3 mutation:
append:
" GATE PASS."
to WorkCompleted.
The verifier must prove that exact relation mechanically.
Deterministic JSON, ASCII/LF.
At minimum:
Schema
Result
Reasons
RealHandoffSha256
MutatedHandoffSha256
TaskId
RunId
BaseSha
CurrentSha
ChangedFields
IntroducedForbiddenTerms
RealForbiddenHits
MutatedForbiddenHits
Checks
Exit:
0 only for PASS.
Any parse/evidence/authority/identity/difference ambiguity:
nonzero FAIL.
Create deterministic synthetic self-tests.
Required PASS case:
REAL clean
+
MUTATED differs only by appending forbidden FreeText term.
Required FAIL cases include at minimum:

* real already contains forbidden term;
* mutation changes a second field;
* mutation changes identity;
* mutation changes FilesChanged;
* mutation removes text;
* forbidden term appears in another governed field instead;
* malformed JSON;
* duplicate key;
* NaN/Infinity;
* lone surrogate;
* missing WorkCompleted;
* term-set missing/ambiguous;
* lowercase/uppercase variants according to §16.10 matching semantics;
* substring that is NOT a whole-word/whole-phrase match.

Self-test results must be reproducible byte-for-byte.
A-5 is M-04 YES.
Obtain ONE independent formal Architect review of the exact A-5 object and
exact verifier blob.
Profile:
ARCHITECTURE_REVIEW
Effort:
Deep
Use the already measured eligible independent read-only cell if still fresh
and available.
Architect must answer:

1. Is M-04 YES correct?
2. Is the Owner authority sufficient for this I-64-only exception?
3. Does A-5 preserve global README §10 unchanged?
4. Does the mechanical verifier test exactly the FreeText invariant?
5. Is it fail-closed?
6. Does it prove that only WorkCompleted changed?
7. Does it use the complete authoritative §16.10 term semantics?
8. Does it avoid silently accepting the Controller STOP classification?
9. Can historical nc3 remain NOT PASSED while substitute evidence supplies
closure without contradiction?
10. Is the exception genuinely I-64-only?
11. Are there hidden M-01..08 implications?
12. Is any I-62 authority required?

Successful result:
AGREED
0 REQUIRED
If REQUIRED:
STOP Coordinator.
Do not enter autonomous amendment loops unless separately authorized.
After Architect AGREED / 0 REQUIRED:
STOP at:
COORDINATOR_A5_AGREEMENT_REQUIRED
Return:

* Owner decision evidence;
* A-5 commit/blob;
* verifier blob/SHA-256;
* self-test result;
* Architect RunId/model/effort;
* verdict;
* REQUIRED count;
* M-01..08;
* CI exact.

Do NOT execute the FreeText bridge yet.
Only after explicit Coordinator AGREED:
execute I64-FREETEXT-BRIDGE-01 against the already existing T2:
REAL handoff:
the verified Worker handoff from
R20261010T202008Z-3df4
MUTATED handoff:
the exact nc3 mutated handoff used by the exhausted nc3 chain.
Required:
REAL/MUTATION RELATION = PASS
FreeText negative test = PASS
Do not invoke Controller again.
Do not rerun nc3.
If the bridge passes, preserve:
nc1 =
PASS
Controller nc2 =
CONTROL NOT PASSED
I64-SCOPE-BRIDGE-01 =
PASS
Controller nc3 =
NOT PASSED / P-04
(all attempts preserved)
I64-FREETEXT-BRIDGE-01 =
PASS
nc4 =
PASS
Product verification =
EXECUTION_VERIFIED / NONE
This is protocol closure by substitute evidence.
It does NOT rewrite nc2 or nc3 history.
A-5 recovery is protocol/documentary only.
Do NOT modify:
src/
tests/
Do NOT rerun Worker.
Do NOT modify RED/GREEN commits.
Do NOT start F1-T3-HOST.
Continue using the closed accepted set:
SNAPSHOT A =
73890CA3319206B85B68A42E23E8D323C45361CFC29E06B3E116EFFF5DAD0FBF
SNAPSHOT B =
64BEEBC7401225A910A80EC166BFD852A8AC9E9E06EC1CCEFB6663DC72AF397A
Any third hash or key inventory change:
STOP P-01.
Current required next boundary:
OWNER_DECISION_REQUIRED
then:
A-5
-> self-test
-> Architect AGREED / 0 REQUIRED
-> COORDINATOR_A5_AGREEMENT_REQUIRED
```
