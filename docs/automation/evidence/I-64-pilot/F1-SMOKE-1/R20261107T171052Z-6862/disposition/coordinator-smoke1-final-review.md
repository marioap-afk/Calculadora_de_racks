I-64 — COORDINATOR SMOKE-1 FINAL REVIEW
RunId:
R20261107T171052Z-6862
Disposition:
PASS
The occurrence is accepted as the authoritative complete Smoke-1 occurrence
for Candidate:
cc7503e3e4e56bff3a2fc9de7ca30a5322b57ea6
RackCad.Plugin.dll SHA-256:
3C261B8F156E69192415E0084D0BC8774B3080ACE9D6CB4B9BB0C1760549B6A0
Owner signature:
"Firmo el registro."
received:
2026-11-07T19:16:29Z
The signature confirms the operator record; this Coordinator review assigns
PASS.
Disposition:
OPTION A ACCEPTED
The clean repetition of the middle-button drag is accepted as the
authoritative S1-02 observation.
The earlier unsuccessful/ambiguous attempt must remain preserved in the
operator record and must not be rewritten as though it had succeeded.
The repeated attempt is accepted because:

* it was performed during the same governed occurrence;
* the Owner performed only the explicitly authorized middle-button gesture;
* the implementation session remained the primary operator;
* VIEWCTR was captured before and after;
* the repeated gesture demonstrated actual view movement;
* no product state, DWG content or acceptance criterion was modified to
obtain the result.

Classification:
FIRST ATTEMPT = OPERATOR / INPUT INCIDENT, HISTORICAL
CLEAN REPEAT = AUTHORIZED RECOVERY / AUTHORITATIVE
S1-02 = PASS
The recovery does not invalidate the occurrence.
The Recent File List changes are accepted as:
EXPECTED MRU / UI-STATE ACTIVITY
SEMANTIC RACKCAD DRIFT = NO
INFORMATIVE / NON-BLOCKING
Movement of drawings in AutoCAD's recent-file ordering is a normal
consequence of opening those drawings during the Smoke and is not RackCad
state.
No cleanup is required.
The Applications-key activity is accepted as:
WRITE_ACTIVITY = YES
VALUE_CHANGE = NO
SEMANTIC_DRIFT = NO
SECURITY_DRIFT = NO
LOAD_DRIFT = NO
INFORMATIVE / NON-BLOCKING
A last-write timestamp change without a value-level change is not sufficient
to classify host drift.
Do not clean or rewrite the key.
AutoCAD .sv$ files are classified:
EXPECTED AUTOCAD TEMPORARY HOST ARTIFACT
NON-BLOCKING
They do not affect Smoke-1 PASS provided the final record confirms:

* no source DWG was saved by the Smoke;
* source drawing hashes remain unchanged where required;
* no Smoke assertion depends on .sv$ content;
* temporary autosave files are not treated as product evidence.

Accepted.
Observed value:
NOT_RECORDED
Disposition:
ACCEPTED FOR THIS OCCURRENCE
This does not invalidate Smoke-1.
Reason:
PaletteKeyAcceptance existed to govern interpretation of palette-related
profile/host persistence during T3C comparison.
For 6862:

* the T3C result and host evidence did not identify a palette persistence
change requiring that exception;
* no forbidden RackCad registration/persistence drift is reported;
* no result depends on accepting an otherwise REPORT_REQUIRED palette key.

Therefore the missing explicit recording is:
PROCEDURAL RECORDING OMISSION
NON-BLOCKING
Do not retroactively invent a value in the operator record.
Record in the final disposition instead:
PaletteKeyAcceptance = NOT_RECORDED / NOT_NEEDED_FOR_DIFF_DISPOSITION
If the underlying T3C/HOST-EXTRAS package contains a palette-key difference
that actually required acceptance, this disposition is void and the run
returns for Coordinator review.
With the SM-29 recovery accepted and the host classifications above:
Smoke-1 = PASS
This PASS applies to the complete 6862 occurrence only.
Historical occurrences remain unchanged:
184b = STOP
0a77 = NOT PASS
a578 = STOP
785a = NOT PASS / CLOSED
Do not use 6862 to relabel any historical result.
C1:
PASS / host validated
C2:
PASS / host validated
The clean complete occurrence supplies the required host evidence for the
current Candidate.
No additional product correction is authorized from Smoke-1.
Custody of 6862 is AUTHORIZED.
Create a documentation/evidence-only custody commit.
Preserve:

* complete signed operator record;
* complete return package;
* SM-29 incident/recovery and this disposition;
* process-guard evidence;
* exact DLL identity;
* BASELINE / POST identities;
* T3C DIFF;
* HOST-EXTRAS;
* DBMOD guard evidence;
* capture-precheck evidence;
* clean authoritative critical captures;
* I-52 recheck;
* Owner signature;
* evidence manifest and SHA-256 inventory.

Apply the established privacy/local-path custody rules.
Do not modify:

* src/;
* tests/;
* project files;
* product behavior.

Run exact-SHA CI on the custody commit.
After custody and successful CI:
F1 / Smoke-1 closure is complete.
The next work is NOT another Smoke-1.
Proceed to the previously approved integration checkpoint:
current I-64
+
current origin/main containing I-65
Before starting F2:

1. synchronize I-64 with current main;
2. resolve the known command-census overlaps;
3. preserve both I-64 RACKPANEL and I-65 RACKRAMPAQUETE;
4. expected combined command census = 39, subject to verification against
the actual current main;
5. run complete Core/UI tests;
6. run Debug builds;
7. run exact-SHA CI;
8. revalidate I-64 Workspace guards;
9. revalidate I-65 structural-analysis/bundle guards;
10. return for Coordinator integration review.

Do not begin F2 until that post-I65 integration checkpoint is reviewed.
F1 = PASS
C1 = PASS
C2 = PASS
Smoke-1 = PASS
Candidate cc7503e3:
SMOKE1_VALIDATED_CANDIDATE
Next:
CUSTODY 6862
→ CI
→ SYNCHRONIZE WITH MAIN / I-65
→ FULL REVALIDATION
→ COORDINATOR INTEGRATION REVIEW
→ then consider F2
