I-64 — COORDINATOR CAPTURE-DEFECT ADDENDUM DISPOSITION
RunId:
R20261105T050250Z-785a
Disposition:
ADDENDUM ACCEPTED
The previously established run disposition remains:
NOT PASS
No SM result is relabeled by this addendum.
Affected full-screen captures:

* SM-17
* SM-26
* SM-27b
* SM-34

Observed defect:
the Claude/session window obscures the upper portion of the RackCad palette.
As a result, those four full-screen captures do not visibly demonstrate all
of the UI state claimed in the operator record.
The earlier statement that all captures contained only the palette/command
line and no unrelated data was incorrect.
Preserve that correction explicitly.
Do not rewrite the signed operator record.
Use a separate addendum.
OPTION A IS APPROVED.
Do NOT commit the four original obscured screenshots to the public repo.
Reason:
they contain unrelated session/chat content and do not add useful product
evidence beyond the redacted/cropped material.
Custody treatment:

* exclude each original screenshot from Git;
* record its original SHA-256 in the custody manifest;
* record the exclusion reason:
PRIVACY / UNRELATED SESSION CONTENT / NON-ESSENTIAL EVIDENCE;
* preserve the original files only in the transient/local evidence set;
* custody the clean cropped palette evidence;
* custody this addendum;
* record any transformation/redaction explicitly.

Do NOT publish the originals merely for completeness.
The cropped/tool-derived palette images may be retained as:
SUPPORTING EVIDENCE
with an explicit limitation:
DERIVED / CROPPED FROM OPERATOR-VISIBLE CAPTURE
NOT ORIGINAL FULL-SCREEN T3D CAPTURE
They may support the operator record, but they are NOT equivalent to the
original capture form expected by the Smoke guide.
Therefore:
SM-17:
the existing observation remains recorded, but the visual artifact is
SUPPORTING / DERIVED evidence.
SM-26:
the existing observation remains recorded, but the visual artifact is
SUPPORTING / DERIVED evidence.
SM-27b:
the existing observation remains recorded, but the visual artifact is
SUPPORTING / DERIVED evidence.
SM-34:
the existing observation remains recorded, but the visual artifact is
SUPPORTING / DERIVED evidence.
Do not call the cropped images authoritative replacements for the original
T3D screenshots.
For the post-run evidence matrix:
Do NOT mark SM-17 / SM-26 / SM-27b / SM-34 as lacking all evidence.
Instead distinguish:
OBSERVATION RESULT:
preserved exactly as recorded
VISUAL EVIDENCE QUALITY:
SUPPORTING / DERIVED
not full-authority T3D capture
For any future determination of whether accumulated evidence can substitute
for another complete Smoke-1 occurrence, these four visual requirements
remain:
NEEDS CLEAN AUTHORITATIVE CAPTURE
unless the frozen procedure or a later accepted disposition explicitly says
otherwise.
This addendum therefore reduces the evidentiary strength of those four
capture points, but does not create a RackCad product failure.
The Owner signature remains valid for the operator record as it existed.
The record itself is not rewritten.
The new facts are appended through this addendum:

* four original screenshots were partially obscured;
* the earlier statement about their content was inaccurate;
* the results remain based on contemporaneous operator/tool observations;
* the derived crops match the recorded observations;
* the original screenshots are excluded from public custody.

This is an evidence/custody correction, not a result correction.
Custody remains AUTHORIZED.
Use the tested Option A custody layout.
Acceptable:

* 83 occurrence files custodied;
* originals with unrelated/private session content excluded by SHA;
* preparation evidence custodied according to the existing manifest rules;
* no local paths exposed;
* addendum included;
* cropped evidence included with its derived-evidence label.

Before commit verify:

* no original obscured screenshot enters Git;
* no session/chat content enters Git through another image;
* manifests preserve the SHA-256 of excluded originals;
* src/, tests/ and project files remain unchanged.

Then create a documentation/evidence-only custody commit and run exact-SHA CI.
After successful custody CI, return at:
COORDINATOR_POST_SMOKE1_NOT_PASS_DISPOSITION_REQUIRED
with:

* custody commit;
* exact CI result;
* evidence matrix that separates:
RESULT STATUS
from
EVIDENCE AUTHORITY;
* list of requirements with authoritative PASS evidence;
* list of requirements with only supporting/derived evidence;
* list of requirements still unproven because of SM-14h / SM-18;
* recommendation:
FULL NEW SMOKE
or
NARROW COMPLETION OCCURRENCE.

Do not merge/rebase main.
Do not start F2.
Do not change product.
