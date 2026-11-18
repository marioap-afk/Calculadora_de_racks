# I-64 F4B — Decisiones D1 y D2 del Owner / Coordinator (F4B-E11)

**Fecha:** 2026-11-18. **Canal:** el chat de la sesión principal. Es un mensaje del Owner, transcrito **literal** a continuación.

**Contexto:**
- **D1** responde a la vuelta C14 de PB-TC2 (la parada prevista PBTC2-C14 del Worker `R20261118T055535Z-38d3`).
- **D2** responde a la vuelta L-1 del paquete de PB-T1 v2.1.

---

```text
I-64 F4B — OWNER / COORDINATOR DECISIONS

Current I-64 branch:

eedb069ce8ae79cda684f2620694949fd402051d

Current main:

812baf879612320653094a8aaf46aca0bac81383

======================================================================
D1 — PB-TC2 BOM PRECISION

Decision:

CONTINUE WITH THE FIX.

Ratify exact-value preservation.

For a manual header height such as:

150.00044

the authored/BOM state may legitimately distinguish quantities that round
differently for presentation, e.g.:

32 @ 150
4 @ 150.0004

instead of forcing:

36 @ 150

if the underlying exact values are actually different.

Do NOT reintroduce rounding merely to preserve the previous BOM grouping.

Reason:

the purpose of PB-TC/PB-TC2 is to stop losing exact user-entered values.

If exact authored values differ, collapsing them into one BOM row would
reintroduce semantic loss downstream.

======================================================================
BOM PRESENTATION

This decision does NOT require displaying excessive precision everywhere.

The BOM may use the existing display/formatting rules.

But grouping/equality must not pretend two distinct exact values are equal
unless the BOM contract explicitly defines a tolerance.

Do not invent such a tolerance inside PB-TC2.

======================================================================
WORKER RESUME

The Worker stopped before modifying code under a contractually required
measurement/decision gate.

Therefore:

resume/relaunch the same correction work WITHOUT consuming a product attempt.

Preserve the stop and Owner decision in evidence.

No new planning is required unless the contract itself changes.

======================================================================
D2 — PB-T1 L-1 / NO ALIASING

Decision:

ACCEPT DECLARED EXCEPTION.

The PB-T1 copied state may retain shared references ONLY for:

1. resolved rack;
2. module report.

Everything else governed by the extracted editable state must satisfy the
normal no-aliasing requirement.

======================================================================
WHY THE EXCEPTION IS ACCEPTED

These two objects already behave as shared/reference objects in the current
product.

Their known post-editor use is limited to the existing drawing path, where
the resolved rack may receive the established name adjustment after the
window has closed.

PB-T1 is a state-extraction task, not a redesign of those authorities.

Therefore cloning them merely to satisfy a blanket alias rule would add a
new semantic boundary without evidence that it is needed.

======================================================================
L-1 SAFETY CONDITIONS

The exception is valid only if PB-T1 proves all of the following:

1. the exact two shared-reference types are enumerated;
2. no other mutable component of editable state aliases the source;
3. DraftSession editing does not mutate either shared object;
4. validation does not mutate either shared object;
5. pending-text/edit operations do not mutate either shared object;
6. discard/reload does not mutate either shared object;
7. comparing InitialDraftState vs CurrentDraftState does not depend on
    mutable changes inside those shared objects;
8. the current permitted post-window name adjustment remains outside the
    draft editing lifetime or is otherwise explicitly controlled;
9. a test proves that mutating all ordinary editable state in the copy does
    not change the source;
10. source guards/tests identify any future added mutation of resolved rack
    or module report inside the draft/editing path.

======================================================================
IMPORTANT LIMIT

This is NOT authorization for general shallow copying.

It is exactly:

two declared shared references.

If later PB-T1/PB-T2/PB-T3 needs to mutate either:

* resolved rack;
* module report;

inside the Workspace draft lifetime:

STOP and reopen L-1.

At that point the object must either:

* be cloned;
* be made immutable;
* or be replaced by a value snapshot appropriate to the draft model.

======================================================================
PB-T1 PLAN

With L-1 resolved, the PB-T1 v2.1 decomposition is accepted in principle:

1. safety-net tests;
2. extracted Application state;
3. classic window consuming that state.

Re-measure it against the final PB-TC2 product before emission.

If that remeasurement changes no material premise:

issue the first PB-T1 task normally.

======================================================================
STATUS

PB-TC:
CLOSED

PB-TC2:
RESUME AUTHORIZED
NO ATTEMPT CONSUMED BY THE MEASUREMENT STOP

PB-T1:
L-1 RESOLVED
v2.1 MAY PROCEED AFTER PB-TC2 CLOSE + REMEASUREMENT

Decision summary:

D1 = CONTINUE EXACT-VALUE FIX
D2 = ACCEPT TWO-REFERENCE ALIAS EXCEPTION
```
