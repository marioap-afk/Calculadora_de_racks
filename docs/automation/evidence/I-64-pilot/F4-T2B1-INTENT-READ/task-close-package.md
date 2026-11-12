# I-64 — F4-T2B1-INTENT-READ: paquete de cierre

Lo prepara la sesión responsable el 2026-11-12. Las decisiones están en `I-64-pilot/F4-DRAFTS/f4-dispositions.md`, de F4-E26 a F4-E29. El cierre lo dispone la orden del Owner `F4-DRAFTS/coordinator/owner-f4t2b1-t2b2-disposition.md` (F4-E29).

Este paquete cierra una tarea, no un gate. No declara GATE PASS.

## 1. Estado resultante

| Hecho | Valor |
|---|---|
| Producto | `fe7f136b09900880e1ebe2cb20491e2f84c85988`, CI `43190415649` push 4/4 (Core en Linux) |
| Verificación | `R20261112T093234Z-6d39`: `EXECUTION_VERIFIED` / NONE, 14/14 |
| Cadena | ChainBaseSha `445c46ce`; intento 1: RED `04717913`, GREEN `23792b2d`; intento 2: BaseSha `b9707bc6`, RED `a044bb4b`, GREEN `fe7f136b` |
| Contratos | `gate-contract.json` (intento 1) y `gate-contract-attempt2.json` (corrección, F4-E27) |
| Intentos | 2 de 3; el intento 3 no se gasta en los menores (F4-E29) |
| Config | Snapshot G (`7C1516A9…`) en todas las invocaciones |
| Resultado | **CLOSED / PASS WITH LIMITATIONS** (F4-E29; limitaciones en el §5) |

## 2. Planificaciones y corridas

| Corrida | Resultado | Notas |
|---|---|---|
| `84ff` | aceptada (fidelidad PASS, A1-A8 8/8) | primera delegación de la cadena; nc4 `1552` con su oráculo |
| `4a75` | entrega del intento 1 | RED `04717913`, GREEN `23792b2d` |
| `16bc` | `EXECUTION_VERIFIED`, 14/14 | la revisión independiente dio NO ACEPTABLE por cobertura de pruebas (F4-E27) |
| `58d4` | REJECTED_BEFORE_INVOCATION (A4) | `ForbiddenWriteScope[47]` copiado con un espacio delante (F4-E28) |
| `614a` | aceptada (fidelidad PASS, A1-A8 8/8) | replanificación única de la corrección |
| `96a8` | entrega del intento 2 | el Worker lanzó suites en segundo plano y esperó su resultado; no quedó nada suyo vivo |
| `6d39` | `EXECUTION_VERIFIED`, 14/14 | sobre `fe7f136b` |

## 3. Controles negativos (sobre las entradas de `6d39`)

| Control | Corrida ordinaria | Resultado ordinario | Sustituto (F4-E29) | Registro |
|---|---|---|---|---|
| nc1 (CurrentSha falso) | `R20261112T094531Z-d11b` | CONTROL PASS: Identity, `EXECUTION_BLOCKED` / STOP | — | CONTROL PASS |
| nc2 (alcance estrechado, sin `PushBackPilotIntentReader.cs`) | `R20261112T094935Z-bd5b` | CONTROL NOT PASSED: FailureClass Contract, con Scope en fail sobre la ruta retirada | F4T2B1-NC2-EXACT-WHITELIST-01 (`R20261112T133004Z-59c6`): PASS | SATISFIED_UNDER_F4T2B1_EXACT_WHITELIST_SUBSTITUTE |
| nc3 (término de gate en `WorkCompleted`) | `R20261112T095410Z-4eb8` | CONTROL NOT PASSED / FALSE PASS BY CONTROLLER: FreeText `pass` y `EXECUTION_VERIFIED` | F4T2B1-NC3-FREETEXT-DETECTOR-01 (`R20261112T133005Z-fb15`): PASS | SATISFIED_UNDER_F4T2B1_FREETEXT_DETECTOR |

Las corridas ordinarias se conservan tal cual. Los sustitutos están en `F4-T2B1-INTENT-READ-nc2-substitute/2/` y `F4-T2B1-INTENT-READ-nc3-substitute/2/`, cada uno con su script y su resultado.

## 4. Producto entregado

La lectura pura de la intención de edición del piloto acotado de Push Back, en `RackCad.Application.Systems.PushBack.Pilot`:
- `PushBackPilotIntentScan`: los hechos sin clasificar del barrido y el texto crudo por clave, copiados y expuestos como vistas de solo lectura, con sus precondiciones estructurales.
- `PushBackPilotIntentReader.BuildStaleBase`: la StaleBase total, independiente de la admisión, que sirve también a «Comprobar ahora».
- `PushBackPilotIntentReader.Read`: la admisión en las 14 reglas ordenadas de D-T2B-03, sin lanzar ante ningún estado del dibujo, con los diagnósticos unificados («No editable aquí: …») y la indicación de RACKEDITAR.
- `PushBackPilotIntentResult`: la base siempre, la lectura de la intención, la clave de origen, el detalle y el estado del testigo de la preview (INV-27).

## 5. Limitaciones y abierto

- **Limitaciones aceptadas** (re-revisión independiente del intento 2, ACEPTABLE CON MENORES). Pasan a F4-T2B2A, dueña de la lectura de la intención (F4-E29):
  - **N-1:** una prueba de `WorkspaceDraftPilotIntentStaleBaseTests` se presenta en su comentario como discriminante y pasaría contra el esqueleto del RED. Es una desviación de la letra de la cláusula M-10 de INV-F4T2B1-15, no una falta de cobertura: la prueba sí discrimina implementaciones reales mal ordenadas.
  - **N-2:** el par de orden (9,10) sí se puede construir (un Kind `PushBack` con otras mayúsculas). Falta su prueba, y la nota de M-9 se apoyaba en que era imposible.
  - **N-3:** un sobre interpretable con un Kind vacío o en blanco da `Detail` `""` en la regla 9, en vez de null.
  - **N-4:** nombres y comentarios de pruebas que afirman más de lo que comprueban.
- **Controles:** nc2 y nc3 se satisfacen con sustitutos deterministas, no con su oráculo ordinario (§3).
- **Deudas de protocolo** (F4-E29; no se resuelven de forma global dentro de I-64 salvo que una tarea posterior vuelva a bloquearse):
  - PD-F4-01: con la lista blanca exacta, nc2 puede hacer fallar Contract antes que Scope;
  - PD-F4-02: el Controller puede no detectar una violación literal de FreeText de 16.10.
- **M-8:** la prueba «`Design` crudo distinto» no es alcanzable vía `Read` (F4-E27).
- **Sin AutoCAD:** la proyección del barrido, el adaptador, el origen y el host son de F4-T2B2A; el ciclo de vida, de F4-T2B2B.
