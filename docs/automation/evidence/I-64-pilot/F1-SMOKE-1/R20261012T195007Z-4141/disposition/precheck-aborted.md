# I-64 Smoke-1 — RunId R20261012T195007Z-4141: PRECHECK_ABORTED / NO_BASELINE / NO_AUTOCAD

## Instrucción del Owner/Coordinator (literal, recibida por el chat el 2026-10-13)

```text
- cierre R20261012T195007Z-4141 como PRECHECK_ABORTED / NO_BASELINE / NO_AUTOCAD;
- conserve la evidencia del intento;
- genere un RunId nuevo;
- regenere el owner-preflight-baseline.ps1 para ese nuevo RunId;
- mantenga el mismo Candidate 4d79e910 y los mismos fixtures;
- no cuente esto como una repetición de Smoke-1, porque la ocurrencia host nunca empezó.
```

## Estado final de este RunId

| Hecho | Valor |
|---|---|
| Disposición | **PRECHECK_ABORTED / NO_BASELINE / NO_AUTOCAD** |
| Repetición de Smoke-1 | **no**: la ocurrencia de host nunca empezó (instrucción anterior) |
| Preflight del Owner | `owner-preflight-baseline.ps1` SHA-256 `11eb788f2d52ebc81f6529f385d7fc6727999ecb8d82d5dc38e405de4e237545` (copia en `preflight/`), iniciado 2026-10-12T20:09:09.5634731Z |
| Pasos superados | 1 acad.exe = 0; 2 Candidato `4d79e910`, árbol limpio, producto = `e0205b5a`, DLL `E769219D…7512`; 3 herramientas T3C con su SHA-256 aprobado; 4 autoprueba 26/26 (salida `2981350C…5B3D`); 5 dibujo D `03e5606c…f165` |
| Parada | paso 5: «STOP: no existe el dibujo A» (la ruta pasada como `-DrawingA` no existía) |
| No ocurrió | lectura de SECURELOAD (no hay `SECURELOAD_BASELINE_CURRENT.txt`); BASELINE (`profile/` y `console/` vacíos); lanzamiento de AutoCAD; carga del DLL; ningún paso SM |
| acad.exe | 0 al iniciar el preflight y 0 al cerrar este RunId (2026-10-13T01:16Z) |
| Sustituido por | RunId `R20261013T011654Z-184b`, mismo Candidato `4d79e910`, mismo DLL y mismos dibujos |

La evidencia del intento se conserva tal cual en este directorio.
