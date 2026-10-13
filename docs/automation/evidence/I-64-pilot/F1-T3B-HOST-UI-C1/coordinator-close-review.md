# I-64 — Cierre de F1-T3B-HOST-UI-C1 por el Coordinator (literal)

Recibido por el chat de la sesión responsable el 2026-10-13, tras el paquete de cierre delta (`ce2dffea`, CI 38342239991 push 4/4) y
la parada en COORDINATOR_T3B_C1_CLOSE_REQUIRED.

```text
I-64 — F1-T3B-HOST-UI-C1 CLOSE
Disposition:
`PASS`
Product:
`b874705e26fff203e8727f6ed2362eca497b5860`
Historical Candidate:
`4d79e91036acdaa3e62b7b54677f394593f581a0`
remains:
`HISTORICAL_SMOKE1_STOP_CANDIDATE`
Do not reuse it.
Next:
prepare a new Smoke-1 Candidate based on the C1 product.
Requirements:

* choose the exact SHA to serve as Candidate under the existing manual-validation rule;
* clean tree;
* Core Full PASS;
* UI Full PASS;
* Debug UI build PASS;
* Debug Plugin build PASS except known warnings;
* exact-SHA CI 4/4;
* exact new `RackCad.Plugin.dll` SHA-256;
* preserve the same PR-12/PR-13 fixtures unless their hashes/content changed;
* re-request coordinated host RELEASE for the new Candidate/DLL SHA;
* get fresh host references as required by T3C;
* do not assume the prior one-time NETLOAD authorization transfers to the new DLL;
* do not run AutoCAD;
* do not take BASELINE yet.

Stop at:
`COORDINATOR_SMOKE1_RETRY_AUTHORIZATION_REQUIRED`
Return:

* exact Candidate SHA;
* exact DLL SHA-256;
* local Full results;
* exact Candidate CI;
* fixture hashes;
* host RELEASE/ack for the new DLL;
* active profile;
* TRUSTEDPATHS full SHA;
* current SECURELOAD source/value rule;
* `PaletteKeyAcceptance`;
* one-time load disposition;
* confirmation `acad.exe = 0`.

Do not start F2.
```
