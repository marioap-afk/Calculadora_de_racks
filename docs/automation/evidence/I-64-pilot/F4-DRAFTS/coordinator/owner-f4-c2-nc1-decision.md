# Disposición del Owner / Coordinator sobre nc1 de F4-T4-HOST-C2 (2026-11-16, chat de la sesión principal, literal)

```
I-64 — C2 nc1
OWNER / COORDINATOR DISPOSITION

Decision:

A — SOLO C2

Authorize a task-local C2 substitute for nc1 with ONE narrow exception:

Trailer

Do NOT modify the previously ratified general substitute.

======================================================================
REQUIRED CONDITIONS

The C2-specific substitute may ignore the Controller’s Trailer result ONLY
if it proves deterministically that:

1. the real verification is 14 / 14;
2. the nc1 mutation changes only the intended false CurrentSha plus
    mechanically derived fields;
3. the Controller correctly detects/rejects the false CurrentSha;
4. the Trailer field in the mutated delivery is byte/value-equivalent to
    the Trailer field in the real verified delivery;
5. Trailer is located under Worker, as required by the actual handoff
    schema;
6. the real verification gave Trailer PASS;
7. the nc1 mutation does not alter Trailer;
8. no other unrelated verification check differs unexpectedly;
9. the outcome remains non-VERIFIED / BLOCKED as required for nc1;
10. the identity signal itself still fails for the false SHA.

======================================================================
RESULT

If those conditions pass:

ordinary nc1 remains:

CONTROL NOT PASSED

and separately record:

nc1 =
SATISFIED_UNDER_F4C2_NC1_TRAILER_EXCEPTION

Record new protocol debt:

PD-F4-08

Controller may look for Trailer at the handoff root instead of under Worker
during a negative-control run.

This exception is:

* local to C2;
* not global;
* not reusable automatically by later tasks;
* not a modification of the ratified general substitute.

======================================================================
C2 CLOSE

If the C2-specific substitute passes and no new issue appears:

C2 may close under the existing D2 rule.

Do not repeat nc1.

Do not spend another product attempt.

After closure:

* custody;
* build the new DLL with AutoCAD closed;
* prepare the Smoke-2 annex;
* continue Smoke-2 with the C2 DLL.

======================================================================
SUSPEND INCIDENT

Record the suspend/resume incident as execution history only.

Because the same Worker resumed with preserved context and the final
delivery/CI/reviews are valid, it is not by itself a product defect or a
reason to repeat the Worker.

======================================================================
STATUS

Decision:

A, SOLO C2
```

Responde a `F4-DRAFTS/c2/owner-decision-request-c2-nc1.md` (la petición de la sesión).
