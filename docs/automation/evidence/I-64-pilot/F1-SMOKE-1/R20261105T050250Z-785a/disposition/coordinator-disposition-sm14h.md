I-64 — COORDINATOR DISPOSITION FOR SM-14h OPERATOR INCIDENT
RunId:
R20261105T050250Z-785a
Disposition:
OPTION A AUTHORIZED
Continue the occurrence from SM-15.
Observed:
Ctrl+V was sent while the AutoCAD command line did not yet own the intended
text-input focus.
AutoCAD started PASTECLIP.
PASTECLIP was cancelled with Esc before an insertion point was provided.
No drawing was saved.
After cancellation:
DBMOD changed from 16 to 17.
Classification:
OPERATOR_INPUT_INCIDENT
This is not presently classified as a RackCad product defect.
Preserve the exact sequence and evidence.
Do not rewrite it as though DBMOD had remained unchanged.
SM-14h result:
NOT MET
Reason:
operator-input incident caused AutoCAD to enter PASTECLIP and DBMOD bit 1
became set.
Do NOT classify this as SM-14h product FAIL unless later evidence shows
RackCad caused the write.
Expected behavior remains unchanged.
SM-18 on Drawing A is now:
COMPROMISED_BY_PRIOR_OPERATOR_INCIDENT
It cannot prove whether RackCad changed DBMOD bit 1 because the bit was
already set before SM-18.
Execute SM-18 if the accepted procedure requires the observation, but record
its DBMOD conclusion as:
INCONCLUSIVE FOR DRAWING-WRITE ASSERTION
Do not convert the already-dirty baseline into a PASS.
This occurrence is no longer eligible for overall Smoke-1 PASS.
Even if all remaining steps pass, the maximum final disposition is:
NOT PASS
because SM-14h is NOT MET and SM-18's write assertion is compromised.
However, the occurrence remains valid and useful evidence.
Do NOT STOP solely because of this incident.
The remaining critical C1/C2 regression evidence should still be collected.
Continue from SM-15 through the remainder of the accepted T3D procedure.
Critical evidence still required:
SM-17:
C1 dock / floating behavior.
SM-26:
closing the last drawing reaches "Sin documento" without the previous
DocumentActivated error.
SM-27:
Start state has no stale session.
SM-34:
RACKEDITAR shows command-active presentation and remains usable.
SM-35:
Drawing B DBMOD assertion remains authoritative if B is still clean before
the step.
SM-39:
RackLog / transaction-leak / exception check.
All other mandatory steps remain mandatory.
For the remainder of this occurrence:
do NOT use Ctrl+V to enter AutoCAD commands or expressions.
Type required AutoCAD command-line text directly with the keyboard.
This restriction is specific to avoiding another operator-input ambiguity.
It does not change T3D expectations.
Before continuing confirm:

* acad.exe count = exactly 1;
* sole PID = AUTHORIZED_AUTOCAD_PID 78512;
* process guard remains PASS;
* no drawing has been saved;
* Candidate DLL identity remains unchanged.

If any fails:
STOP.
Do not attempt to repair or reset Drawing A's DBMOD.
Do not undo the PASTECLIP incident merely to recover a clean DBMOD.
Do not save A.
Preserve it as-is until the procedure closes it without saving.
This keeps the incident auditable.
At the end, return the complete occurrence normally with:

* all remaining SM results;
* SM-14h = NOT MET / OPERATOR_INPUT_INCIDENT;
* SM-18 = COMPROMISED / INCONCLUSIVE for the write assertion;
* authoritative SM-35 result on B if its prerequisites remain clean;
* final RackLog;
* BASELINE / POST;
* HOST-EXTRAS;
* process-guard evidence;
* I-52 recheck.

Proposed final result must not be PASS.
Expected maximum:
NOT PASS
unless another condition causes STOP.
Do not:

* save Drawing A;
* repair DBMOD;
* repeat SM-14h to overwrite history;
* skip later mandatory steps;
* change expected outcomes;
* merge/rebase main;
* start F2.
