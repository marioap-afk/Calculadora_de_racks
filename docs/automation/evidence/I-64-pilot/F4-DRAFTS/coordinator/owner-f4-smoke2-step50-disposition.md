I-64 — SMOKE-2
STEP 50 OWNER DISPOSITION

Decision:

(a)

Reclassify Step 50 as:

OBSERVED / OPERATOR ARTIFACT

not:

PRODUCT DEFECT

======================================================================
BASIS

The original FAIL was caused by screen-operation coordinates.

When the Selection section grows after a rack/object is selected, the
navigation button row moves downward.

The failed clicks used the old button position and landed outside “Ver”.

This has now been reproduced intentionally:

* click at old position -> no action;
* click at actual current “Ver” position -> navigation works.

Therefore the original observation does not demonstrate a RackCad navigation
failure.

======================================================================
TARGETED RE-SMOKE

Accept the 7-case mini-smoke as the required targeted re-smoke.

Results:

1. no preselection -> Ver:
    PASS
2. unrelated object preselected -> Ver:
    PASS
3. target rack preselected -> Ver:
    PASS
4. multiple objects preselected -> Ver:
    PASS
5. Seleccionar:
    PASS
6. Centrar:
    PASS
7. Localizar:
    PASS

Coverage also included:

* rack selected by expression;
* rack selected by mouse;
* panel floating;
* panel docked;
* two racks preselected;
* large unrelated preselection.

This is sufficient to CLOSE Step 50.

No product correction is authorized or required.

======================================================================
HISTORICAL RECORD

Do NOT rewrite the original Smoke-2 run log.

Preserve the original Step-50 FAIL exactly as observed at the time.

Add an addendum that records:

* diagnosis;
* coordinate/UI-layout cause;
* intentional reproduction;
* targeted mini-smoke;
* Owner reclassification.

Final classification:

STEP 50 =
CLOSED / FALSE FAIL / OPERATOR ARTIFACT

======================================================================
SMOKE-2 FINAL STATUS

With Step 50 closed:

SMOKE-2 = PASS WITH LIMITATIONS

Accepted limitations remain:

* Save Changes ordering caused by the test drawing / AutoCAD DBMOD condition;
* unavailable physical drawing classes;
* internal conditions not physically observable without instrumentation;
* exact close-during-mid-transaction race not physically provoked;
* any previously declared automated-only branches.

These are limitations of physical coverage, not open product defects.

======================================================================
F4 STATUS

After the Step-50 addendum and final custody:

F4 may be declared:

CLOSED / PASS WITH LIMITATIONS

No full Smoke-2 rerun is required.

======================================================================
OPTIONAL UI IMPROVEMENT

The fact that the Selection section changes height and moves the navigation
buttons is a legitimate usability observation.

Record as:

FUTURE UX IMPROVEMENT

Possible improvement:

keep the navigation/action row vertically stable while Selection content
changes.

Do NOT reopen F4 for this.

It is not a correctness defect.

======================================================================
DIAGNOSTIC FOLDERS

After:

1. Step-50 diagnosis/addendum is custodied;
2. final Smoke-2 disposition is published;
3. all required hashes/evidence are preserved;

the diagnostic folders may be cleaned.

Do not delete anything still referenced by uncustodied evidence.

======================================================================
FINAL OWNER SIGNATURE

Step 50:
CLOSED / OPERATOR ARTIFACT

Targeted navigation re-smoke:
PASS

Smoke-2:
PASS WITH LIMITATIONS

F4:
AUTHORIZED TO CLOSE AFTER FINAL CUSTODY
