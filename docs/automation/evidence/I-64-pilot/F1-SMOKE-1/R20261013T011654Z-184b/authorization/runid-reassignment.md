# I-64 Smoke-1 — Reasignación de RunId

La autorización de Smoke-1 del Coordinator (`coordinator-smoke1-authorization.md`, copia literal) se ejerce con este RunId, `R20261013T011654Z-184b`. El RunId anterior, `R20261012T195007Z-4141`, se cerró como PRECHECK_ABORTED / NO_BASELINE / NO_AUTOCAD: la ocurrencia de host nunca empezó y **no** cuenta como repetición de Smoke-1. Mismo Candidato `4d79e91036acdaa3e62b7b54677f394593f581a0`, mismo DLL `E769219D2F5EDF6E0DE0867633C4A89B94EBBD7F2BD88CF20864CBCF75EF7512` y mismos dibujos de prueba.

Instrucción del Owner/Coordinator (literal, recibida por el chat el 2026-10-13):

```text
- cierre R20261012T195007Z-4141 como PRECHECK_ABORTED / NO_BASELINE / NO_AUTOCAD;
- conserve la evidencia del intento;
- genere un RunId nuevo;
- regenere el owner-preflight-baseline.ps1 para ese nuevo RunId;
- mantenga el mismo Candidate 4d79e910 y los mismos fixtures;
- no cuente esto como una repetición de Smoke-1, porque la ocurrencia host nunca empezó.
```
