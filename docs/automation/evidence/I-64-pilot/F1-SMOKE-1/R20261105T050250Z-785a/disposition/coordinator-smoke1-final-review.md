I-64 — COORDINATOR SMOKE-1 FINAL REVIEW
RunId:
R20261105T050250Z-785a
Disposition:
NOT PASS
The occurrence is ACCEPTED AS VALID EVIDENCE.
It is not PASS because:
SM-14h = NOT MET / OPERATOR_INPUT_INCIDENT
and:
SM-18 = COMPROMISED_BY_PRIOR_OPERATOR_INCIDENT / INCONCLUSIVE FOR DRAWING-WRITE ASSERTION
These historical results must remain exactly as recorded.
Classification:
PROCEDURAL / TOOLING ARTIFACT
NON-BLOCKING
The internal paste did not execute in AutoCAD.
It is not a RackCad product failure and does not alter any Smoke result.
Disposition:
INFORMATIVE
Confirmed:

* no new RackCad AssemblyMap registration;
* no changed RackCad CoreUser state;
* no RackCad entry in Applications;
* no RackCad entry in Loaded;
* no RackCad entry in HKLM.

The only value-level difference is the AutoCAD recent-file list:
the same Drawing D path moved from position 12 to position 4 after Drawing D
was opened during the occurrence.
Classification:
EXPECTED MRU / UI-STATE REORDERING
SEMANTIC RACKCAD DRIFT = NO
This reorder is not a semantic change to RackCad host state.
The path is captured only because its containing worktree path includes the
RackCad name.
Therefore:
HOST-EXTRAS = INFORMATIVE / NON-BLOCKING
No cleanup is required.
Observed:
last-write timestamp changed when AutoCAD closed.
Values:
UNCHANGED
T3C:
no value diff
I-52:
same keys / no drift
Classification:
WRITE_ACTIVITY = YES
SEMANTIC_DRIFT = NO
SECURITY_DRIFT = NO
LOAD_DRIFT = NO
INFORMATIVE
NON-BLOCKING.
Do not clean or rewrite the key.
Classification:
EXPECTED AUTOCAD TEMPORARY HOST ARTIFACT
NON-BLOCKING
Accepted because all three conditions are satisfied:

* no DWG was saved;
* all five source drawing files remain byte-identical;
* no Smoke result relies on the .sv$ files.

Additionally, the .sv$ files disappeared when AutoCAD closed and are not
part of custody evidence.
They are not a RackCad product write.
The PID-bound isolation evidence is accepted.
AUTHORIZED_AUTOCAD_PID:
78512
The occurrence maintained the authorized single-instance process guard except
for no reported isolation failure in this run.
Candidate DLL identity evidence is accepted as recorded.
Final host state:
acad.exe = 0
I-52 post-run recheck:
NO DRIFT within I-52's declared scope.
I-52's lack of ACK/RELEASE does not alter this occurrence's disposition,
because HOST_RELEASE for I-64 was granted by the Owner / Coordinator before
the run.
Preserve the individual SM results exactly as recorded.
In particular, the valid observations obtained in 785a remain evidence for
the implemented C1/C2 behavior.
Do not rewrite the occurrence as PASS.
Do not rewrite SM-14h or SM-18 after closure.
Owner signature:
"Firmo el registro."
received 2026-11-05T20:41:06Z.
Accepted.
The signature confirms the operator record and does not imply PASS.
CUSTODY IS AUTHORIZED.
Create a documentation/evidence-only custody commit for 785a.
Requirements:

* preserve the complete operator record;
* preserve Coordinator dispositions and incident records;
* preserve Owner signature;
* preserve SM results;
* preserve evidence hashes;
* preserve BASELINE / POST identities;
* preserve T3C DIFF result;
* preserve HOST-EXTRAS classifications;
* preserve I-52 response;
* preserve PID / DLL identity evidence.

Local-machine paths may be excluded or redacted using the same custody rules
already established for earlier I-64 Smoke occurrences.
Record original SHA-256 values for excluded/redacted evidence where the
existing custody convention requires them.
The custody commit must not change:

* src/;
* tests/;
* project files;
* product behavior.

Run exact-SHA CI after the documentation-only custody commit.
After custody:
R20261105T050250Z-785a = NOT PASS / CLOSED
Reason preventing PASS:
OPERATOR_INPUT_INCIDENT at SM-14h
+
SM-18 write assertion compromised.
This occurrence does NOT establish a new RackCad product defect from those
two results.
Do not implement a product correction for SM-14h / SM-18 from this evidence.
After the custody commit and successful CI, return with:
COORDINATOR_POST_SMOKE1_NOT_PASS_DISPOSITION_REQUIRED
including:

* custody commit;
* exact CI;
* concise matrix of which Smoke requirements now have authoritative PASS
evidence across the accepted occurrences;
* which requirements still require a clean occurrence for final Smoke-1 PASS;
* recommendation on whether another full Smoke-1 is necessary or whether a
narrowly scoped completion occurrence is defensible under the frozen
procedure.

Do NOT merge/rebase main yet.
Do NOT start F2 yet.
Do NOT change product.
