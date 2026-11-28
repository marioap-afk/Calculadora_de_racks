I-64 — COORDINATOR DISPOSITION FOR SM-07 DEVIATION
RunId:
R20261105T050250Z-785a
Disposition:
OPTION A AUTHORIZED
The occurrence may continue.
At the first execution of SM-06, the operator selected rack:
RackId 54ef4061
That rack does not have the duplicate required for the intended SM-07
same-RackId selection sequence.
Classification:
OPERATOR_SELECTION_INCIDENT
This is NOT a product failure and does NOT invalidate the occurrence.
Preserve the original SM-06 observation exactly as executed.
Do not rewrite it as though the intended rack had been selected initially.
Perform the following recovery only:

1. Press Esc / clear the current selection using the normal accepted
selection-clearing action.
2. Repeat SM-06 using the upper-left rack, which is one member of the known
same-RackId pair.
3. Execute SM-07 by adding/selecting its copy at the upper-right.
4. Execute SM-08 using the lower rack as the distinct-RackId case.

Record:

* original SM-06 = executed / PASS as observed;
* repeated SM-06 = RECOVERY REPEAT AUTHORIZED BY COORDINATOR;
* reason = wrong fixture member selected for the SM-07 sequence;
* SM-07 and SM-08 = evaluated normally after the recovery.

Do not erase or replace the first SM-06 record.
Before continuing, confirm:

* no drawing was saved;
* DBMOD / write state did not change because of the incident beyond ordinary
view state already permitted by the procedure;
* AUTHORIZED_AUTOCAD_PID remains 78512;
* acad.exe count remains exactly 1;
* no second AutoCAD instance appeared;
* Candidate DLL identity remains unchanged.

If any of those conditions fail:
STOP.
This authorization is narrow.
It authorizes only the SM-06 selection recovery needed to make SM-07
executable with the intended same-RackId pair.
It does NOT authorize:

* other fixture substitutions;
* skipping SM steps;
* changing expected results;
* exploratory clicks;
* saving any drawing;
* changing the Smoke procedure.

After the recovery, resume T3D at SM-07 / SM-08 and continue normally.
Keep:
deviation-request-sm07.md
and add this Coordinator disposition to the run evidence.
The occurrence remains eligible for Smoke-1 PASS.
No STOP is required for this incident.
