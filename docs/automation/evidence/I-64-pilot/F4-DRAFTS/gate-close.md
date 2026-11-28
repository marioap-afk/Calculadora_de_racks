# I-64 — Cierre del gate F4 (Borradores y Actualizar, con piloto Push Back)

**Resultado: CLOSED / PASS WITH LIMITATIONS** (F4-E81). No es Owner Validation y no integra en `main`.

**Autoridad:** la firma final del Owner, «F4: AUTHORIZED TO CLOSE AFTER FINAL CUSTODY» (`F4-DRAFTS/coordinator/owner-f4-smoke2-step50-disposition.md`), una vez publicada la custodia final de Smoke-2 (`75f684cb`, F4-E80).
- Este cierre no tiene una revisión del Coordinator aparte: lo autoriza esa firma.
- Las dispensas del Owner ya registradas (F4-E09 y F4-E29, presupuesto por tarea) siguen en vigor.

## Secuencia de cierre (Proposal V5 §11)

| Paso | Estado |
|---|---|
| Implementación | 19 tareas cerradas (tabla de abajo) |
| Candidato F4 | `9abc1320`, el último cambio de `src/` y `tests/` (F4-T4-HOST-C2). CI exacta `43972738540` 4/4. Los commits posteriores de la rama son solo documentales |
| Evidencia automatizada (INV-04..08, 10..12, 16..20, 25..31) | en las tareas y su CI exacta (paquetes de cierre de cada tarea); este cierre no la vuelve a ejecutar |
| Smoke-2 | **PASS WITH LIMITATIONS** (F4-E79 y F4-E80), con el DLL compilado desde el Candidato |
| Revisión y firma | la firma final del Owner (arriba) |

## Tareas

| Tarea | Estado | Intentos | Producto | CI |
|---|---|---|---|---|
| F4-T1-DRAFT-CORE | CLOSED / PASS | 3 de 3 | `ce73ceb7` | 43029959059 |
| F4-T1B-PILOT-CHARACTERIZATION | CLOSED / PASS WITH LIMITATIONS | 1 de 3 | `1d36d006` | 43650040527 |
| F4-T2A-PILOT-STATE | CLOSED / PASS | 2 de 3 | `cf9f9441` | 43167736802 |
| F4-T2B1-INTENT-READ | CLOSED / PASS WITH LIMITATIONS | 2 de 3 | `fe7f136b` | 43190415649 |
| F4-T2B2A-INTENT-WIRING | CLOSED / PASS WITH LIMITATIONS | 2 de 3 | `56b0d497` | 43259514731 |
| F4-T2B2B-LIFECYCLE | CLOSED / PASS WITH LIMITATIONS | 1 de 3 | `8f249e3a` | 43294223849 |
| F4-T2C-IC04-READ-SCHEDULING | CLOSED / PASS WITH LIMITATIONS | 1 de 3 | `33e43f9f` | 43319785427 |
| F4-T3A-EDIT-PORT | CLOSED / PASS WITH LIMITATIONS | 1 de 3 | `18471988` | 43355611879 |
| F4-T3B1-EDIT-SURFACE | CLOSED / PASS WITH LIMITATIONS | 2 de 3 | `1708e764` | 43380342743 |
| F4-T3B2-EDIT-CLOSE-SYNC | CLOSED / PASS WITH LIMITATIONS | 1 de 3 | `23f67f56` | 43440641847 |
| F4-T3C1-HOST-HARDENING | CLOSED / PASS WITH LIMITATIONS | 1 de 3 | `7bd275e4` | 43477341374 |
| F4-T3C2-COMMAND-ADVISOR | CLOSED / PASS WITH LIMITATIONS | 1 de 3 | `4e34bb56` | 43510803096 |
| F4-T4A1-PREPARE-CORE | CLOSED / PASS WITH LIMITATIONS | 2 de 3 | `bbd9a35e` | 43554278196 |
| F4-T4A2-PREPARE-UI | CLOSED / PASS WITH LIMITATIONS | 2 de 3 | `ae0a7b40` | 43617317131 |
| F4-T4B-MUTATE-POST | CLOSED / PASS WITH LIMITATIONS | 1 de 3 | `4314c180` | 43668817337 |
| F4-T4C1-HOST | CLOSED / PASS WITH LIMITATIONS | 2 de 3 | `b2631084` | 43741634851 |
| F4-T4C2-PANEL | CLOSED / PASS WITH LIMITATIONS | 2 de 3 | `b88dabf2` | 43758023582 |
| F4-T4-HOST-C1 | CLOSED / PASS WITH LIMITATIONS | 1 de 3 | `b86c3cfe` | 43801607010 |
| F4-T4-HOST-C2 | CLOSED / PASS WITH LIMITATIONS | 1 de 3 | `9abc1320` | 43972738540 |

Cada fila sale del paquete de cierre de la tarea (`I-64-pilot/F4-<tarea>/task-close-package.md`); el detalle, las revisiones, los controles y las limitaciones están allí y en `F4-DRAFTS/f4-dispositions.md`.

## Smoke-2 (Proposal V5 §12.4)

| ID | Pasos de la guía | Resultado |
|---|---|---|
| S2-01 | 1, 2, 8 y 20 | PASS |
| S2-02 | 4 y 46 | PASS |
| S2-03 | 14 a 19 | PASS, con el DLL de C2 (`U` y `REDO` refrescan solos) |
| S2-04 | 31 a 33 | PASS |
| S2-05 | 28 | PASS |
| S2-06 | 69 a 74 | veto PASS en CLOSE, la X de la pestaña, CLOSEALL y QUIT; la pregunta de guardar antes del veto es OBSERVED / TEST-DRAWING CONDITION (F4-E79) |
| S2-07 | 58 | PASS |
| S2-08 | 57 | PASS |
| S2-09 | 79 | PASS |
| S2-10 | 82 | PASS; la relectura de POST queda solo automatizada (decisión del Owner, pasos 85 y 86) |
| S2-11 | 87 | PASS |
| S2-12 | 66 | NOT AVAILABLE FOR PHYSICAL SMOKE (el divergente y el A/B sin referencias en los dibujos); «fuera del piloto (F5)» observado en S2-15 |
| S2-13 | 43 | PASS |
| S2-14 | 23 | PASS |
| S2-15 | 68 | PASS en lo observable (19 Push Back admitidos; F5 en cabecera, selectivo y dinámico); las clases sin dibujo, NOT AVAILABLE |
| Paso 50 (SN-10, pestañas) | 50 | CLOSED / FALSE FAIL / OPERATOR ARTIFACT; re-smoke de navegación 7/7 PASS (F4-E80) |

Registros: `F4-SMOKE-2/run/run-log-1.md`, `run-log-2.md`, `owner-summary-run2.md` y `addendum-step50.md`.

## Limitaciones

Son de cobertura física; **ningún defecto de producto queda abierto**:
- el orden de la pregunta de guardar de AutoCAD frente al veto, cuando el dibujo tiene DBMOD ≠ 0 propio;
- las clases de dibujo no disponibles (divergente, A/B, Push Back antiguo, sin Id, ilegible, referencia externa y las demás de S2-15);
- las condiciones internas no observables sin instrumentación (NO VERIFICABLE FÍSICAMENTE / evidencia automatizada);
- la carrera de cierre a mitad de transacción, no provocada (75 a 77 en la rama a);
- las ramas declaradas solo automatizadas;
- las limitaciones de cada tarea (R-1 de F4-T4-HOST-C2 para la OV final; Cantilever sigue con F4-S2-03, R-4).

**Mejora de uso futura**, sin reabrir F4: que la fila de navegación del panel no se mueva cuando cambia «Selección» (`docs/ideas-futuras.md`).

## Siguiente

F5-A (editores con shell: Selectivo, Dinámico y Cantilever) entra con F4 PASS. Necesita su propio contrato y la autorización expresa del Owner. Siguen pendientes F5 a F7, Smoke-PERF y la Owner Validation final.
