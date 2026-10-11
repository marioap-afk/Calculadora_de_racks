# Bloque A2-P2, sonda 1 NATIVE_CONTROLLER: resultado (R20261011T050156Z-a2p2-v4-controller)

> Supervisión, plano a. Autoridad: decisiones §69 (bloque A2-P2 para `d13914ce…/26.1007.2314.0/F909F480…`; OD-2 = A y `BLOQUE-CODEX-CONSUMO` = A
> del Owner). Kit congelado en `7bcc48f5` (texto `f88d3313…`, igual al lanzado). **Una sola invocación, sin reintento.**

## Hechos

- `codex exec` read-only con la receta 16.4 literal, sin shell declarada; celda observada `gpt-6-luna` / `high`;
  `-C D:\r62-fixture\A2`. Salida 0 en ~105 s de 600. Identidad, incluidos los ejecutables acompañantes, igual antes y después;
  `HEAD` y árbol sin cambios; escenario retirado sin diferencias.
- **Ruta de la shell:** shell por defecto = el `pwsh` del runtime ({'pwsh-runtime': {'total': 4, 'ok': 3}}). **Sin la firma de F6-OBS-03** y sin recurrir a
  otra shell: la regresión de la versión anterior no aparece. `CmdA41Opens` = false.
- 4 comandos (3 correctos). **Veredicto de `compare`: NOT_DEMONSTRATED** (94 claves fallidas). Los esquemas canónicos v2 se
  aceptan y la salida los valida.

## Resultado por paso

| Paso | `ok` de la celda | Claves fallidas | Error que declara la celda |
|---|---|---|---|
| 1 | sí | 0 | — |
| 2 | sí | 0 | — |
| 3 | sí | 0 | — |
| 4 | sí | 0 | — |
| 5 | sí | 2 | — |
| 6 | sí | 2 | — |
| 7 | sí | 2 | — |
| 8 | sí | 0 | — |
| 9 | no | 2 | fatal: Could not read from remote repository.  Please make sure you have the correct access rights a |
| 10 | sí | 0 | — |
| 11 | sí | 3 | — |
| 12 | sí | 0 | — |
| 13 | sí | 4 | — |
| 14 | sí | 9 | — |
| 15 | sí | 11 | — |
| 16 | sí | 2 | — |
| 17 | sí | 0 | — |
| 18 | sí | 0 | — |
| 19 | no | 6 | — |
| 20 | no | 3 | — |
| 21 | no | 7 | — |
| 22 | no | 21 | — |
| 23 | no | 5 | — |
| 24 | sí | 0 | — |
| 25 | no | 3 | — |
| 26 | no | 3 | — |
| 27 | no | 3 | — |
| 28 | no | 3 | — |
| 29 | sí | 0 | — |
| 30 | no | 1 | — |
| 31 | no | 1 | — |
| 32 | no | 1 | — |

## Causas (NOT_DEMONSTRATED)

- **Omisión:** la celda paró a los ~105 s con 4 comandos. No ejecutó la cadena de AP 16.13 (pasos 19-23) ni los controles negativos (25-32), y no leyó
  el TRX ni la entrega.
- **Errores:** diffs con otra base (pasos 5 y 18); ascendencia de AR mal resuelta (6); blobs en AR equivocados (7); conteo de `ForbiddenWriteScope`
  (13).
- **Fidelidad del no ASCII:** también con el `pwsh` del runtime, la salida deforma el UTF-8 (`Objective` llega como «A?adir … operaci?n»).
- **Sandbox:** `git ls-remote` del origen vuelve a fallar (paso 9).
