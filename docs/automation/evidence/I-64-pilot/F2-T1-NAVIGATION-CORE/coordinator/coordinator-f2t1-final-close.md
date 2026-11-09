I-64 — F2-T1-NAVIGATION-CORE
COORDINATOR FINAL CLOSE
Current branch:
8b649a788183e80618d650732c8174f1a431747a
Exact-SHA CI:
42655487294
4 / 4 SUCCESS
F2-T1-NAVIGATION-CORE:
CLOSED / PASS
Product implementation:
COMPLETE
Real verification:
R20261108T221347Z-ebfc
EXECUTION_VERIFIED / NONE
ACCEPTED
Negative-control chain:
nc4:
PASS
nc1:
PASS
nc2:
SATISFIED_UNDER_I64_SCOPE_BRIDGE
nc3:
SATISFIED_UNDER_I64_TESTFILTER_BRIDGE
A-11 real application:
R20261109T162731Z-7588
PASS
61 / 61 checks
A-11 execution budget:
1 / 1 CONSUMED
Product attempts:
1 / 3
Preserve exactly:
46c1 = INVALID_OUTPUT
041d = ORACLE NOT MET
53af = INVALID_OUTPUT
0706 = CONTROL FAIL /
HISTORICAL CONTROLLER SCOPE DEFECT
P-04 history remains unchanged.
Do not rewrite any historical control output.
Verified product SHA remains:
c726410f3ac764f4a80a08a2b810b5fdda276ecc
Later commits through:
8b649a788183e80618d650732c8174f1a431747a
are documentation / evidence / protocol custody and do not replace the
verified product identity.
All F2-T1 close requirements are considered satisfied.
No additional:

* Controller run;
* Worker run;
* bridge;
* AutoCAD run;
* protocol amendment

is required for F2-T1.
Record:
f2_t1_status = CLOSED_PASS
and:
next_action = F2_T2_CONTRACT_REQUIRED
F2-T2-NAVIGATION-HOST-UI is now:
AUTHORIZED TO PREPARE CONTRACT
This does NOT yet authorize a Worker invocation.
Prepare a NEW delegation / gate contract for F2-T2.
Do not reuse the F2-T1 contract.
F2-T2 connects the already verified Navigation Core to:

* RackCad Workspace UI;
* AutoCAD host actions;
* focus behavior;
* document/session safety;
* navigation echo suppression.

Required user-visible actions remain:
SELECT
VIEW
CENTER
LOCATE VIEW
The implementation must use the F2-T1 NavigationTargets authority rather
than recreate navigation discovery in Plugin/UI.
Preserve D-05 / D-07:

1. NavigationTargets remains the single navigation authority.
2. Before executing an AutoCAD action:
   * validate active document;
   * validate session;
   * validate target existence;
   * validate target attribution.
3. Validation failure:
   * zero navigation action;
   * invalidate/refetch target state.
4. Navigation must not author DWG data.
5. Navigation must not:
   * change layouts automatically;
   * change TILEMODE automatically;
   * perform unrelated view mutations.
6. Select:
selects the applicable Model Space references.
7. View:
frames the target with the accepted margin.
8. Center:
preserves current scale.
9. Locate:
navigates to the selected deterministic view/reference.
10. Focus returns to the drawing after the action.
11. Echo suppression remains content-based.
12. No next-event token shortcut.

For F2-T2 and subsequent I-64 UI work, preserve this product direction:
RackCad Workspace is modeless and must coexist with normal AutoCAD use.
Do not design the navigation UI around the assumption that the PaletteSet is
always a narrow docked sidebar.
The Workspace may be:

* docked;
* floating;
* substantially resized;
* placed on another monitor.

F2-T2 does not need to complete the final responsive UX, but it must not
introduce host/UI assumptions that prevent that later layout.
This is a design constraint, not authorization to expand F2-T2 into F3/F4.
Do not implement:

* F3 tabs/context migration beyond what F2 navigation strictly requires;
* F4 editing/drafts;
* migration of legacy editors;
* responsive redesign of all system interfaces;
* new rack systems;
* global inventory;
* removal of RACKEDITAR;
* final dual-monitor UX.

Those remain later I-64 or post-infrastructure work.
Prepare:
F2-T2-NAVIGATION-HOST-UI
contract + delegation + acceptance package.
Before any Worker invocation:

* verify current main;
* verify current I-64 branch;
* verify config.toml against accepted A/B/C/D;
* define exact write scope;
* define required tests and RED expectation;
* run A1-A8 acceptance;
* run nc4 if required by the chain rules.

Return at:
COORDINATOR_F2_T2_CONTRACT_REVIEW_REQUIRED
Do not invoke the Worker until that contract is explicitly accepted.
F0:
CLOSED
F1:
PASS
Smoke-1:
PASS
Post-I65 integration/sanity:
PASS
F2-T1:
CLOSED / PASS
F2-T2:
AUTHORIZED TO CONTRACT
NOT YET AUTHORIZED TO IMPLEMENT
Smoke-NAV:
PENDING AFTER F2
F3-F7:
PENDING
F2 overall:
IN PROGRESS
