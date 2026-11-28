I-64 — COORDINATOR POST-SMOKE-1 NOT PASS DISPOSITION
Reviewed custody:
1917bc19e11e0fa09f4484956794825ba960098a
Exact-SHA CI:
42324525500
push
4/4 SUCCESS
Run:
R20261105T050250Z-785a
Status:
NOT PASS / CLOSED
Custody:
ACCEPTED
The custody commit is accepted.
Confirmed:

* documentation/evidence only;
* no src/ changes;
* no tests/ changes;
* no project-file changes;
* 785a preserved as NOT PASS / CLOSED;
* four obscured original screenshots excluded from Git;
* their SHA-256 identities and exclusion reasons preserved;
* derived/cropped evidence explicitly labeled as supporting evidence;
* exact-SHA CI is green.

The change from the originally estimated 83 occurrence files to 85 is
accepted because the two additional files are the later accepted addendum
and its Coordinator disposition.
No custody correction is required.
The matrix must continue to distinguish:
RESULT STATUS
from:
EVIDENCE AUTHORITY
Accepted authoritative evidence includes the requirements already listed in
the custody package, including the clean B write check and the completed
post-run host controls.
Supporting / derived only:
SM-17
SM-26
SM-27b
SM-34
Their recorded observations remain valid, but their visual artifacts are not
authoritative T3D captures.
Still requiring clean proof because of the operator incident:
S1-06h
S1-08 / OV-20
(the assertion that normal Workspace use on a newly opened drawing does not
set the drawing-modified bit)
Still unobserved:
S1-02 middle-button drag behavior.
Disposition:
FULL NEW SMOKE
A narrow completion occurrence is NOT authorized.
Reason:

1. The frozen Smoke-1 procedure defines one complete occurrence and does not
currently define a partial-completion mode.
2. The unresolved evidence is distributed across multiple parts of the run:
   * early interaction / S1-02;
   * command/panel behavior;
   * SM-17 visual state;
   * clean DBMOD path through SM-14h / SM-18;
   * closing all drawings / SM-26;
   * Start state / SM-27b;
   * RACKEDITAR / SM-34.
3. A partial run would require a new procedural exception or amendment merely
to combine evidence from multiple RunIds.
4. The expected time saving is small compared with the additional evidentiary
complexity.
5. The product has no currently identified correction pending. A full clean
occurrence is now the shortest path to an unambiguous Smoke-1 PASS.

No product change is authorized.
The current product remains:
C2 PASS
The failures preventing 785a PASS were procedural/evidentiary:

* SM-14h operator-input incident;
* SM-18 compromised by that prior incident;
* four captures with reduced evidence authority;
* S1-02 not observed.

Do not create a product correction from these findings.
The same product/Candidate may be proposed again if all of the following
remain true:

* src/ unchanged;
* tests/ unchanged;
* project files unchanged;
* Candidate DLL not rebuilt;
* exact Candidate identity remains verifiable;
* exact DLL SHA remains unchanged.

However, no previous run authorization transfers.
The next occurrence requires:

* explicit Candidate reauthorization;
* new RunId;
* fresh host recheck;
* new HOST_RELEASE;
* new one-time NETLOAD authorization;
* new BASELINE;
* new POST;
* new operator record.

Before starting another Smoke:
fix the capture arrangement.
The session/chat window must not overlap the RackCad palette or any area
required by the Smoke evidence.
Before SM-00a, perform a capture precheck that proves:

* the complete palette is visible;
* its title/status region is visible;
* the relevant command line is visible when required;
* no unrelated/private chat/session content is inside the intended capture;
* the capture mechanism records the same visible desktop state used for the
Smoke evidence.

If the capture layout becomes obstructed during the run:
pause before the affected SM step and correct the window arrangement without
altering the drawing state.
Do not discover capture obstruction only during custody.
The next full occurrence must actually observe the middle-button drag
requirement.
If the current automation/input tooling cannot perform that exact interaction,
do not silently mark it PASS or skip it.
Before the run, establish an operator method capable of performing and
observing S1-02.
If that cannot be done under the accepted procedure, return to the
Coordinator before BASELINE.
The next occurrence must preserve a clean Drawing A write baseline.
Do not use Ctrl+V for AutoCAD command-line entry.
Use direct keyboard entry.
Before the relevant DBMOD assertion, verify that no unrelated operator action
has set the drawing-write bit.
If the write bit becomes dirty for an unrelated operator/tooling reason before
SM-18:
STOP / return for disposition.
Do not continue another knowingly compromised write assertion.
The currently open AutoCAD process reported after custody is pre-run host
activity and does not affect the closed 785a occurrence.
Before any new occurrence:

* close AutoCAD normally;
* require acad.exe = 0;
* require RAM Elements = 0;
* obtain a fresh host recheck after closure.

The existing PID guard model remains required:
0 acad.exe before BASELINE
→ exactly 1 authorized PID during Smoke
→ 0 acad.exe before POST completion.
Next sequence:
fresh host state
→ Candidate/run authorization
→ FULL Smoke-1
→ Coordinator review
Only after:
Smoke-1 = PASS
may I-64 proceed to:
synchronize with current main / I-65
→ resolve known command-census conflicts
→ full revalidation
→ consider F2.
Do NOT merge/rebase main yet.
Do NOT start F2.
