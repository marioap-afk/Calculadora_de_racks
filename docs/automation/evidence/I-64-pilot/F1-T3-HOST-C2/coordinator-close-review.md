# I-64 — Cierre de F1-T3-HOST-C2 por el Coordinator (literal)

Recibido por el chat de la sesión responsable el 2026-11-04, tras el paquete de cierre delta (`596ad29c`, CI 41870715720 push 4/4) y
la parada en COORDINATOR_T3_C2_CLOSE_REQUIRED.

```text
I-64 — COORDINATOR F1-T3-HOST-C2 CLOSE
Disposition:
PASS
RED:
57fea37780abf134663f02ae827dda2a70552b50
CI:
41866515249
EXPECTED FAILURE
28 expected test failures
GREEN / PRODUCT:
0e0d536b10350cc85e4a932a0d576b22fae22fa9
CI:
41867521852
push
4/4 SUCCESS
Delta close:
596ad29c0b616ef120ed723540be1988a65ed54d
CI:
41870715720
push
4/4 SUCCESS
ACCEPTED.
CommandWillStart / EnteringQuiescentState now update presentation state
without changing the T2 notification/drain contract.
Required properties confirmed:

* no hint;
* no RequestReconciliation;
* no Idle attachment;
* no drain;
* no selection read;
* no DWG read;
* no DWG write;
* argument-free in-memory presentation notice only;
* controller relays it only as StateChanged.

The existing T2 oracle:
SourcesThatAreNotHintsQueueNothingAndNeverAttachIdle
remains unchanged and green.
SM-13 and SM-34 historical FAIL results remain unchanged.
ACCEPTED.
DocumentActivated with null Document now means:
NO ACTIVE DOCUMENT
not:
ignore event
and not:
destroy sessions.
Required behavior confirmed:

* no NativeId(null);
* no NullReferenceException;
* ActiveSession becomes none;
* all existing sessions remain alive;
* presentation notification occurs;
* visible panel can show "Sin documento";
* reactivating a real document restores that document's session/context.

SM-39 historical FAIL remains unchanged.
The four existing-guard amendments are ACCEPTED.
They only account for the exact new C2 surface:

* IsCommandActiveInActiveSession;
* OnNoActiveDocument;
* controller CommandStateChanged subscription.

No previous requirement was removed.
C2 product:
0e0d536b10350cc85e4a932a0d576b22fae22fa9
Debug RackCad.Plugin.dll SHA-256:
2C2433ABB82569D3E48271467186D8D5D1AA7C636E3937252328054897097B4E
Historical Candidate:
09f2f1c3c4e64491018ef1489ce4dd3030d1e4a8
Disposition:
HISTORICAL_SMOKE1_NOT_PASS_CANDIDATE
Do not reuse it.
Prepare a new Candidate after this C2 close.
Use the existing Candidate rules:

* exact Candidate commit;
* no further commits before the occurrence;
* clean tree;
* Core Full PASS;
* UI Full PASS;
* Debug UI PASS;
* Debug Plugin PASS except known MSB3277;
* exact-SHA CI 4/4;
* exact RackCad.Plugin.dll SHA-256;
* revalidate PR-12 / PR-13 fixtures against the new Application bytes;
* fresh coordinated host recheck/release.

Do not assume any prior DLL-load authorization transfers.
The next Smoke-1 must use:

* new Candidate;
* new RunId;
* new BASELINE;
* new POST;
* new operator record.

Critical regression checks:
SM-03 / SM-17:
C1 remains PASS.
SM-13:
panel must immediately show command-active state:

* Leer el dibujo = disabled;
* Escribir en el dibujo = disabled;
* "Hay un comando en curso."

and no instrumentation evidence of drain/read during the command.
SM-34:
same command-active presentation while RACKEDITAR is running;
editor itself remains unaffected.
SM-26 / Start state:
panel must show "Sin documento" without RackLog error.
SM-27 / Start tab:
same; no stale previous session.
SM-39:
no AutoCadEventBridge.DocumentActivated NullReferenceException and no
transaction-leak diagnostic.
Do NOT merge/rebase origin/main yet.
Current main already contains I-65.
Preserve the previously accepted sequencing:
new C2 Candidate
→ Smoke-1
→ Coordinator Smoke review
→ THEN synchronize I-64 with current main / I-65
→ resolve the two known command-census conflicts
→ full revalidation
→ only then consider F2.
After Candidate preparation and fresh host authorization, stop at:
COORDINATOR_SMOKE1_RETRY_AUTHORIZATION_REQUIRED
Do NOT launch AutoCAD before that authorization.
Do NOT start F2.
```
