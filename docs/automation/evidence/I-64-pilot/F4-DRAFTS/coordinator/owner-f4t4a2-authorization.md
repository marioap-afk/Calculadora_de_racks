Ratificación del cierre de F4-T4A1 y autorización de F4-T4A2 del Owner (2026-11-14, tras la custodia `b9e1ed5a`). Texto pegado por el Owner en la sesión, literal:

I-64 — F4-T4A1 CLOSE RATIFICATION / F4-T4A2 AUTHORIZATION

Current I-64 branch:

b9e1ed5a6ef08e666d0b9ab75743da00157143d9

Current main:

812baf879612320653094a8aaf46aca0bac81383

======================================================================
F4-T4A1 DISPOSITION

F4-T4A1 is:

CLOSED / PASS WITH LIMITATIONS

Verified product:

bbd9a35e

Custody:

b9e1ed5a

Attempt history:

Attempt 1 = REWORK
Attempt 2 = CLOSED / PASS

No Attempt 3 used.

Accepted evidence:

* exact-SHA GREEN CI = 4 / 4;
* Core = 2843 / 2843;
* UI = 458 / 458;
* valid RED;
* Controller verification = 14 / 14;
* nc3 ordinary CONTROL PASS;
* nc1 satisfied under the exact PD-F4-04 substitute;
* nc2 satisfied under the exact PD-F4-01 substitute;
* direct review;
* independent review with 0 blockers / 0 important findings.

======================================================================
T4A1 MINORS

The 14 minor findings remain OPEN as carried quality obligations.

Do NOT blindly put all 14 into T4A2.

Assign each finding to:

* T4A2 if T4A2 touches that guard/surface;
* T4B if mutation/write code owns it;
* later hardening only if neither task legitimately owns it.

Priority carry-ins include:

* drag/test case that also passes against old code;
* anti-alias guard holes such as pilot?.Read / aliases;
* R-3 guard missing forms such as:
    this.ErrorReason =
    and
    ??=

Where T4A2 owns the surface, fix them as DECLARED contract changes.

======================================================================
F4-T4A2

Authorize:

F4-T4A2 — CHECK-NOW / RECONCILIATION

attempts:

1 / 3

Proceed now.

No further Owner approval is required for ordinary package preparation,
review, contract issuance, planning or Worker execution.

======================================================================
PRIMARY OBJECTIVE

T4A2 owns the user-visible read-only reconciliation workflow around:

“Comprobar ahora”

and the explicit resolution of stale/conflict information in the panel.

It remains part of PREPARE.

Therefore:

ZERO authored DWG writes.

======================================================================
CORE FLOW

The intended flow is:

Draft exists
-> user requests “Comprobar ahora”
-> authoritative reread
-> compare with StaleBase
-> classify result
-> present result to user
-> if reconciliation choice is required:
explicit user choice
-> update in-memory draft/base according to frozen policy
-> no authored write

Actualizar remains T4B.

======================================================================
COMPROBAR AHORA

“Comprobar ahora” must:

1. be explicit user action;
2. validate SessionId / active document;
3. execute only through the authorized read path;
4. require quiescent/no-modal conditions;
5. use DocumentLock where the read path requires it;
6. reread authored authority;
7. compare against the existing StaleBase;
8. return a deterministic reconciliation result;
9. perform zero authored mutation;
10. leave all DBObjects/Transactions disposed.

======================================================================
RESULT CLASSES

The T4A2 contract must explicitly distinguish at least:

UNCHANGED

EXTERNAL_CHANGE

CONFLICT

NOT_FOUND / NO_LONGER_EDITABLE

READ_ERROR / UNKNOWN

Use the exact names already established by the current architecture where
available.

Do not collapse:

UNKNOWN
or
ERROR

into:

UNCHANGED.

======================================================================
NO FALSE CLEAN

A failed reread or comparison must NEVER result in:

“sin cambios”

or equivalent clean state.

Unknown/error remains conservative.

======================================================================
RECONCILIATION CHOICES

Where the frozen design requires an explicit user decision after an external
change, T4A2 owns the panel choice and the in-memory result.

At minimum contract the exact available choices already authorized by the
Freeze / current package, such as the applicable equivalents of:

* reload external/base;
* keep my draft;
* cancel / do nothing.

Do not invent a fourth policy casually.

======================================================================
CONFIRMATION

Any destructive reconciliation choice must require explicit confirmation.

The Sí/No confirmation guard carried from T3C2/T4A1 must be hardened so it
can detect an inverted implementation.

Required test:

if Yes and No behaviors are swapped,
the guard/test MUST fail.

======================================================================
AUTHORIZE

Likewise harden the Authorize guard carried from T3C2.

A test/guard must fail if:

* authorization result is inverted;
* wrong session/document is authorized;
* unauthorized state is accepted.

Do not rely only on the presence of the word/method Authorize.

======================================================================
KEEP MY DRAFT

If the user chooses the accepted equivalent of:

“Conservar mi borrador”

the resulting draft must NOT carry stale authored fields that the pilot does
not own.

Preserve the rule already fixed in T2A:

non-pilot authored data comes from the current authoritative base.

Only the pilot-owned editable value(s) come from the user’s draft.

No silent rollback of external changes.

======================================================================
RELOAD

If the user reloads external state:

StaleBase
and
InitialDraftState

must be replaced together from the SAME logical authoritative read.

CurrentDraftState then follows the frozen reload policy.

Do not update only one side of the baseline.

======================================================================
PENDING TEXT

If reconciliation occurs while invalid pending text exists:

follow the already frozen T3 blockable/non-blockable rules.

Do not silently discard or normalize pending text.

If the user action is blockable and the pending text prevents the
transition:

abort and preserve text.

======================================================================
NO DWG WRITE

This is a HARD T4A2 invariant:

Comprobar ahora
Reload
Keep draft
Cancel

produce:

ZERO authored DWG writes.

Any authored mutation is:

STOP / product defect.

======================================================================
T4A2 TESTS

Required automated scenarios include at minimum:

1. unchanged authored state -> unchanged result;
2. external pilot-owned field changed;
3. external non-pilot field changed;
4. both pilot and non-pilot changed;
5. deleted/not-found rack;
6. no-longer-editable rack;
7. unreadable/future envelope;
8. comparison/read exception -> UNKNOWN/ERROR;
9. reload replaces base and initial state atomically;
10. keep-draft preserves only pilot-owned user edits;
11. keep-draft adopts current authoritative non-pilot data;
12. cancel leaves draft/base untouched;
13. invalid pending text blocks blockable reconciliation;
14. wrong SessionId -> zero effect;
15. wrong active document -> zero effect;
16. no authored writes in every path;
17. confirmation Yes/No inversion test;
18. Authorize inversion/session mismatch test.

======================================================================
TOCTOU

T4A2 must not pretend its PREPARE result is a permanent write authorization.

Any plan/result produced here is advisory/read-only until T4B.

T4B MUST reread/revalidate again immediately before MUTATE.

Therefore:

T4A2 passing does NOT eliminate T4B stale checks.

======================================================================
IMMUTABLE PLAN

If T4A2 produces a mutation/reconciliation plan object for T4B:

make it immutable and composed only of plain values.

No:

DBObject
ObjectId
Transaction
Document
mutable host object

may survive inside it.

======================================================================
PACKAGE PROCESS

Before contract issuance:

1. map current “Comprobar ahora” code;
2. map current reconciliation UI;
3. map current stale/conflict state machine;
4. map confirmation and Authorize guards;
5. map the 14 T4A1 minors to actual owners;
6. write T4A2 package;
7. independent adversarial review;
8. incorporate all blocking/important findings;
9. emit contract only when acceptable.

======================================================================
QUALITY CHAIN

After Worker:

* RED / GREEN;
* exact-SHA CI;
* direct review;
* independent review;
* Controller verification;
* nc1 / nc2 / nc3;
* custody.

Controller 14/14 is necessary but not sufficient.

======================================================================
PLANNING COPY RULE

Use the established F4 rule:

one reinforced replan is permitted if the first planning output fails only
literal collection fidelity.

No product attempt consumed.

Second fidelity failure:

STOP.

======================================================================
CONTROL DEBT

Existing task-local substitutes may be reused ONLY when the exact signature
matches their accepted applicability conditions.

Do not infer a substitute merely because the same check name fails.

======================================================================
AFTER T4A2

If T4A2 closes:

T4A / PREPARE is complete.

Then prepare:

F4-T4B — MUTATE / POST / UNDO

T4B remains the first and only task in F4 authorized to write authored DWG
state.

======================================================================
SMOKE-2

Do not perform final Smoke-2 before T4B closes.

But T4A2 should prepare the expected reconciliation scenarios so Smoke-2 can
exercise them later with real racks.

======================================================================
STATUS

F4-T4A1:
CLOSED / PASS WITH LIMITATIONS

F4-T4A2:
AUTHORIZED
attempts 1 / 3

F4-T4B:
WAITING

Smoke-2:
PENDING

F5-F7:
PENDING
