# I-64 — F2-T2A-NAVIGATION-HOST: paquete de cierre para la revisión del Coordinator

Preparado por la sesión responsable el 2026-11-10, tras la cadena completa autorizada por las disposiciones del Coordinator
(literales en `coordinator/`). Parada: **COORDINATOR_F2_T2A_CLOSE_REQUIRED**.

Este paquete:
- **no** declara F2 PASS, GATE PASS, Candidato ni FINAL_CANDIDATE_SHA;
- no reetiqueta ninguna salida del Controller (3d86, 16b6, bfc8 y b454 se conservan tal como se emitieron);
- no inicia F2-T2B y no usa AutoCAD;
- deja al Coordinator la revisión del cierre.

## 1. Estado resultante

| Hecho | Valor |
|---|---|
| Producto | **EXECUTION_VERIFIED / NONE**: verificación `R20261109T201422Z-89a6`, 14/14 en pass |
| VerifiedSha | `44a692a4ec386e559cc6d330b3d78b87dbad6581`, CI 42687835409 push 4/4 |
| Cadena | BaseSha `53302b85`; RED `2da0e60d` (CI 42687231210); GREEN `44a692a4` |
| Intentos | 2 de 3; el intento 1 se cerró como REWORK / Ci (`3f3e`) |
| nc1 ordinario | **AGOTADO** (P-04): `3d86` y `16b6` = INVALID_OUTPUT; `bfc8` = CONTROL NOT PASSED |
| nc1 | **SATISFIED_UNDER_I64_IDENTITY_BRIDGE**: A-13 REAL APPLICATION = PASS |
| nc2 | `b454` = CONTROL NOT PASSED (pre-empción ambiental, no probatorio de Scope); **`03d2` = CONTROL PASS** en el marco histórico |
| nc3 | **`42a7` = CONTROL PASS** en el marco histórico |
| A-4 / A-6 | no aplicados: no hizo falta |

## 2. Verificación y controles

| Corrida | Resultado | Notas |
|---|---|---|
| `R20261109T201422Z-89a6` | EXECUTION_VERIFIED / NONE, 14/14 | filtro Core 756/756; Core 15148; UI 64/64; UI completa 1701 + 17 omitidas |
| `R20261109T202056Z-3d86`, `R20261109T203840Z-16b6` | INVALID_OUTPUT | Handoff e Identity en fail, FailureClass Handoff |
| `R20261109T212644Z-bfc8` | CONTROL NOT PASSED | mismo modo; camino normal agotado |
| `R20261110T030457Z-b454` | CONTROL NOT PASSED | Remote en fail por S-13: el Controller consultó `origin/main` vivo; Scope en pass; A-4 no aplicado |
| `R20261110T040218Z-03d2` | **CONTROL PASS** | Scope en fail, FailureClass Scope, EXECUTION_BLOCKED / STOP; anteriores iguales a la real |
| `R20261110T041007Z-42a7` | **CONTROL PASS** | FreeText en fail, EXECUTION_REWORK_REQUIRED / REWORK; las demás iguales a la real |

## 3. A-12 y A-13 (I64-IDENTITY-BRIDGE-01)

- **A-12** (`4206b2da`): HISTÓRICA, CHANGES REQUIRED (`R20261109T233319Z-e044`), inactiva, inmutable.
- **A-13** (`31a1c837`): corrige A-12 y la sustituye para la activación. Architect AGREED (`R20261110T022847Z-bc2b`), Owner SATISFIED, Coordinator AGREED.
- **Pre-chequeo de esquema del relevo real:** `SCHEMA_VALID`.
- **Sonda neutra de Git** en el repositorio real: `A13_GIT_OBJECT_QUERY_ENVIRONMENT = PASS`.
- **Aplicación real única:** exit 0, `HandoffIntegrity = PASS`, `Identity = FAIL`, `ControlResult = PASS`, 52 comprobaciones, objeto `MISSING`. Salida SHA-256 `f8624114b06d754f20e7fe31bffb20ae76d417171c571b11cbf6039560486111`.

## 4. Marco histórico de los controles

- `origin/main` avanzó de `a140eb3a` a `04a70414` tras la verificación (integración de I-67). Los blobs de autoridad son idénticos y el Coordinator aceptó el avance.
- nc2 y nc3 se ejecutaron con el directorio de trabajo en el worktree real, `GIT_WORK_TREE` real y `GIT_DIR` en un directorio Git histórico aislado, construido solo con objetos locales y con `origin/main = a140eb3a`. Las entradas siguen idénticas por bytes y el prompt es el normal.
- **LiveRemoteFacts:** `origin/main = 04a70414`; rama de producto `44a692a4`. **HistoricalControlRemoteFacts:** `origin/main = a140eb3a`; rama de producto `44a692a4`. El entorno histórico existe solo para evaluar un control negativo relativo a la verificación 89a6; no afirma que `a140eb3a` sea el main actual.
- El repositorio real no se modificó y el directorio histórico se borró tras registrar la evidencia. Durante los controles cambió una referencia ajena a I-64 en el repositorio común (`feature/bom-excel-analisis-nativo`, otra sesión).

## 5. Entorno de Codex

- Tras `b454`, una actualización de la aplicación reemplazó el binario del CLI y cambió `config.toml`.
- **Snapshot E** (`2EEDDAAB…`): ACEPTADO por el Owner y el Coordinator. El delta por clave desde el Snapshot D es **DESCONOCIDO** (limitación histórica de evidencia, renuncia acotada a la transición D → E), no una afirmación de igualdad.
- **Binario** `5449c1f0f26f1f98/codex.exe`, SHA-256 `4F91AE86…EFC17`: ACREDITADO solo para la celda del Controller `gpt-6-luna / high / read-only` (sonda `R20261110T034758Z-6a68`). `gpt-6.1-sol` no queda acreditado para este binario.
- **Diferencia observada:** las órdenes se lanzan con `-NoProfile`. Aceptada como no material; su causa no está demostrada.

## 6. Pendiente

Revisión del cierre por el Coordinator. F2-T2B sigue en espera y Smoke-NAV pendiente.
