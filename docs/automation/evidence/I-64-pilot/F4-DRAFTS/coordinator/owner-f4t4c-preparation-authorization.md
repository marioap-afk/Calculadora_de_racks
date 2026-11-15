# I-64 F4-T4B: ratificación del cierre y autorización de preparación de F4-T4C (2026-11-15)

Orden del Owner/Coordinator recibida en el chat después del cierre de F4-T4B (custodia `b15072ff`). Se transcribe literalmente.

```text
I-64 — F4-T4B CLOSE RATIFICATION / F4-T4C PREPARATION AUTHORIZATION

Current I-64 branch:

b15072ff4d30e72ed820df8a40bada84729fc331

Current main:

812baf879612320653094a8aaf46aca0bac81383

======================================================================
F4-T4B DISPOSITION

F4-T4B-MUTATE-POST is:

CLOSED / PASS WITH LIMITATIONS

Verified product:

4314c180

Custody:

b15072ff

Attempt:

1 / 3

Accepted evidence:

* valid RED f4476bc4;
* GREEN exact-SHA CI = 4 / 4;
* Core = 18684 / 18684;
* Controller verification = 14 / 14;
* direct review;
* independent review;
* nc1 CONTROL PASS;
* nc3 CONTROL PASS;
* nc2 ordinary output preserved as CONTROL NOT PASSED;
* nc2 satisfied under F4T4B exact-whitelist substitute;
* PD-F4-07 recorded.

Do not reopen T4B merely because T4C integrates it.

======================================================================
F4-T4C

Authorize:

PACKAGE PREPARATION + INDEPENDENT REVIEW ONLY

Do NOT emit the T4C contract yet.

Return to Owner/Coordinator after the package has been independently
reviewed and corrected.

======================================================================
PRIMARY OBJECTIVE

F4-T4C integrates the already-tested T4B mutation implementation into the
real AutoCAD host.

T4C owns:

* Actualizar host invocation;
* ExecuteInCommandContextAsync;
* DocumentLock;
* wiring of the run-scoped primitive writer;
* catalog/read dependencies required by the run;
* one host execution per explicit Actualizar;
* command-context/host failure containment;
* diagnostic/presentation integration assigned from T4A2;
* T4B carried items I-01, M-01, M-03 and M-06.

T4C must NOT implement a second mutation algorithm.

======================================================================
AUTHORITATIVE MUTATION LOGIC

T4B remains the sole authored mutation implementation.

T4C may:

* create the run-scoped AutoCAD adapter;
* enter command context;
* lock the correct Document;
* invoke T4B;
* translate host-level failures;
* return/present T4B outcome.

T4C may NOT independently decide:

* stale;
* conflict;
* which authored values win;
* mutation sequencing;
* POST classification.

Those remain T4B/Application responsibilities.

======================================================================
EXECUTE IN COMMAND CONTEXT

For authored mutation:

ExecuteInCommandContextAsync

is REQUIRED.

This is intentionally different from F2 navigation.

Keep this authority narrow:

only the exact Actualizar host/mutation integration path.

Do not migrate unrelated Workspace reads/navigation into command context.

======================================================================
DOCUMENT LOCK

Before primitive mutation:

* revalidate SessionId;
* identify exact target Document;
* reject document mismatch;
* acquire DocumentLock in the proper host context;
* do not lock one document and mutate another.

Wrong/closed document:

zero write.

======================================================================
FINAL REVALIDATION

T4C must not trust an old T4A2 result as write authorization.

Immediately before invoking T4B MUTATE:

revalidate all host-level conditions that can change between PREPARE and
MUTATE.

T4B’s own stale/conflict validation remains binding.

No TOCTOU shortcut.

======================================================================
RUN-SCOPED WRITER

The primitive writer introduced by T4B must exist only for one Actualizar
run.

Required:

* created after host admission;
* receives only the exact current document/context;
* not retained by Workspace controller;
* not cached between updates;
* disposed/released on every path;
* cannot be reused after run completion.

No AutoCAD DBObject, Transaction or Document may escape through the
Application-facing interface.

======================================================================
ONE ACTUALIZAR

One explicit user Actualizar must create:

one T4B run.

No duplicate invocation from:

* UI events;
* Idle;
* event bridge;
* POST reread;
* own-mutation notifications.

The existing Applying lock must prevent concurrency.

======================================================================
T4B I-01

Carry I-01 into T4C.

Move the three new host texts to the accepted run-owned/constants surface
if that is the surface touched by T4C.

Do not broaden this into localization cleanup.

======================================================================
T4B M-01 / M-03 / M-06

Carry these explicitly into the package.

For each:

* identify exact owning file/surface;
* state whether T4C closes it;
* add discriminating test/guard;
* do not silently mark it resolved.

======================================================================
T4A2 DIAGNOSTIC

T4C owns the deferred user-visible diagnostic from T4A2 if the current
architecture requires host/UI wiring here.

Keep diagnostic presentation separate from mutation classification.

A diagnostic failure must not alter authored state.

======================================================================
COMMIT FAILURE

Preserve the T4B outcome model.

Host integration must distinguish:

* failure before mutation;
* mutation/unit failure before commit;
* commit failure / unknown persistence;
* successful commit;
* successful commit with POST warning.

Do not translate UNKNOWN into ordinary Failure or Success.

======================================================================
POST

T4C must allow T4B POST reread to complete under the authorized host path.

Do not clear dirty merely because:

Commit() returned.

T4B outcome remains authoritative.

======================================================================
OWN-MUTATION

T4C is responsible for correct host scoping of the own-mutation window.

It must be:

* session-bound;
* rack/member-bound;
* active only around the actual authored operation;
* released on success;
* released on exception;
* released on cancellation;
* unable to mask another rack.

This is a major Smoke-2 target.

======================================================================
UNDO

Design T4C so that one Actualizar can form one logical AutoCAD Undo step.

Package must identify:

* transaction/command boundary;
* any explicit undo mark/API currently used;
* what unit tests can prove;
* what only real AutoCAD can prove.

Do NOT claim final Undo compliance before Smoke-2.

======================================================================
T4C AUTOMATED TESTS

At minimum package automated coverage for:

1. correct document admitted;
2. wrong document rejected before writer creation;
3. closed session rejected;
4. ExecuteInCommandContextAsync invoked once;
5. DocumentLock acquired once for target document;
6. writer created once per run;
7. writer disposed on success;
8. writer disposed on failure;
9. T4B invoked exactly once;
10. command-context failure -> zero T4B invocation if it happens before
    admission;
11. lock failure -> zero mutation;
12. T4B Conflict -> host performs no extra write;
13. T4B Failure preserved;
14. T4B Unknown/commit uncertainty preserved;
15. Applied preserved;
16. AppliedWithWarnings preserved;
17. own-mutation scope opens/closes exactly once;
18. concurrent Actualizar rejected;
19. no writer retained after run;
20. no DWG write outside the T4B path.

======================================================================
NO SECOND WRITE PATH

Add a guard proving there is no second Workspace authored mutation
implementation in T4C/Plugin.

The only authored pilot mutation must flow through the T4B writer seam.

======================================================================
T1B COMPATIBILITY

Use the closed T1B characterization as a compatibility authority where T4C
touches classic Push Back seams.

If T4C changes a T1B-characterized legacy behavior:

STOP and return to Owner.

Do not silently update the characterization tests.

======================================================================
PACKAGE REVIEW

Before emission:

1. map exact T4B call seam;
2. map command-context entry point;
3. map DocumentLock ownership;
4. map current transaction/Undo boundary;
5. map own-mutation signals;
6. map T1B-characterized legacy seams touched;
7. map I-01/M-01/M-03/M-06;
8. write package v1;
9. independent adversarial review;
10. incorporate all blocking and important findings;
11. re-review if the first review finds architectural blockers.

======================================================================
RETURN BEFORE EMISSION

Return to Owner/Coordinator if review reveals:

* T4B cannot be invoked without duplicating mutation policy;
* more than one authored mutation transaction is required;
* T1B behavior must change;
* DocumentLock/command context cannot wrap the T4B run cleanly;
* Undo requires a material architecture change;
* the pilot-specific writer needs a new persistent host authority;
* or any other material product decision.

Otherwise bring back the reviewed package and proposed contract boundaries
before emission.

======================================================================
SMOKE-2

Smoke-2 begins only after T4C product closes.

Smoke-2 remains mandatory and is the first physical validation of the new
pilot-specific authored write path in AutoCAD.

It must include:

* real Push Back rack;
* actual Actualizar;
* correct authored result;
* one logical Undo;
* Undo restore;
* Redo where applicable;
* conflict;
* cancel;
* document mismatch;
* own-mutation containment;
* another rack not masked;
* lifecycle CLOSE/CLOSEALL/QUIT;
* IC-04 timing;
* navigation representative rack cases.

======================================================================
STATUS

F4-T4A:
COMPLETE

F4-T1B:
CLOSED / compatibility authority

F4-T4B:
CLOSED / PASS WITH LIMITATIONS

F4-T4C:
PACKAGE PREPARATION AUTHORIZED
EMISSION NOT YET AUTHORIZED

Smoke-2:
WAITING FOR T4C

F5-F7:
PENDING
```
