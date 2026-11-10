# I-64 — Cierre del gate F3 (Browser Session)

**Resultado: CLOSED / PASS WITH LIMITATIONS** (F3-E13). No es Owner Validation y no integra en `main`.

## Tareas

| Tarea | Estado | Producto | CI |
|---|---|---|---|
| F3-T1-BROWSER-CORE | CLOSED / PASS | `a92039bb` | 42833048949 4/4 |
| F3-T2-BROWSER-UI | CLOSED / PASS | `206e0f7a` | 42860103890 4/4 |

Presupuesto: `attempts` 2 de 3, propio de F3 (F3-E01). Una corrección delegada (F3-E09).

## Obligaciones del gate (Freeze V5 §11)

| Obligación | Cobertura |
|---|---|
| INV-02 con pestañas | Sin suscripciones por pestaña: escenario 6 y guardas de fuente |
| Regla de preview de INV-04 | F3-T1: como máximo una preview; una fijada nunca se reemplaza. El texto pendiente es de F4 |
| INV-09 | Cerrar solo cambia memoria; cero llamadas de escritura |
| INV-15 | K = 1 sobre el árbol visual: un elemento de superficie reutilizado |
| INV-38 (pestaña) | La navegación usa el RackId de la pestaña y `NavigationTargets`; el resumen no usa `NavigationTargets` |

## Escenarios automatizados (CI exacta sobre `206e0f7a`)

| Escenario | Prueba | Resultado |
|---|---|---|
| 4, cambio de rack o contexto | `Scenario4AChangeOfRackOrOfContext…`, `Scenario4ThePinnedRackSurvives…` | Passed |
| 5, cambio de pestaña | `Scenario5AChangeOfTabMaterializesTheSurfaceOnce…` | Passed |
| 6, 10 pestañas ligeras | `Scenario6TenLightTabsAreTenRecordsWithExactlyOneLiveSurface…` | Passed |

El escenario 6 establece: 10 registros, 1 superficie viva, cero suscripciones y cero objetos de AutoCAD por pestaña, y cero llamadas a la navegación o escrituras al cambiar de pestaña.

## Coste caracterizado, sin umbrales

| Medida | Resultado |
|---|---|
| Crear preview | 0,013 ms |
| Reemplazar preview | 0,021 ms |
| Activar pestaña existente | 0,002 ms |
| Cambiar entre 10 pestañas | 0,023 ms en total |
| Materializar la superficie K = 1 | 0,006 ms, 1 lectura |

Son medidas de una corrida en un agente de CI con un puerto simulado, así que **no incluyen el barrido real del dibujo**. Sirven de línea base para Smoke-PERF y F7. Detalle en `scenario-harness.json`.

## Limitaciones

- R-1 a R-4 de F3-E12.
- Sin verificación en AutoCAD real: el bloqueo de lecturas en avisos, la omisión de racks borrados y la tira de pestañas solo están cubiertos sin host.
- SN-06c y la parte de pestañas de SN-10 quedan pendientes (IC-02). También sigue pendiente SN-08 como `LIMITED`.
- «Abrir en editor clásico» queda para F5 (IC-03).

## Siguiente

F4 necesita su propio contrato y un presupuesto expreso. Smoke-2, F4 a F7 y la Owner Validation siguen pendientes.
