# I-55 — G11 ID18 pure multi-view queue / batch contract

Fecha: 2026-09-22

Gate: G11

`G11_START_SHA`: `c12aeff00268755764a4efecdb2c3778739090c6`

SHA de producto: `991d1694c39377fe7f03d67b024b5a9fcc8dd093`

`origin/main` observado: `7097057cf8685bf5ecc09083cba37379d4a4aae8`

I-49 observado: candidato `589e3db5536ae9cc9af9c051e694b4c1803f0196`, integrado en
`a61850a6095cf8ebc7d9d92eab6dbbcc2343a6d1`; sin rama ni worktree activo. Clasificacion: `NONE / STABLE`.

I-52 observado: `34649e5889912339515fece9ceb8d088250934ea`; su avance desde el tip observado en G10 solo modifica
documentacion de Proposal V23 y sus paquetes. No toca producto, sesiones ni requests de G11. Clasificacion:
`NON-MATERIAL / overlap NONE`.

Proposal V5: gobernante y congelada (blob `38bee23a3a886a5026664bfda6d260ba5c65fc26`)

Implementation Map V5: blob `17f6969ad2272dca5530d8533b6cbfd2afc05d23`

## 1. Contrato puro

`RackViewBatchPlan` orquesta una intencion ordenada de varias vistas de un solo rack sin Autodesk, UI visible ni
driver Plugin. Sus requests transportan `RackViewAddress` tipadas, por lo que cada item conserva su variante:
fondo, poste, Entrance/Exit o station segun el sistema. No usa indices de controles como identidad.

La entrada vacia, una mezcla de RackId o una cola multivista de Flow Bed se rechazan antes de invocar callbacks. Las
direcciones duplicadas no se deduplican ni reordenan. El contrato conserva exactamente el orden solicitado.

Estados observables:

```text
VARIANT_CANCELLED
SIBLING_GATE_FAILED
PREFLIGHT_FAILED
PREPARE_FAILED
REDRAW_ROLLED_BACK
REDRAW_APPLIED
REDRAW_NOT_REQUIRED
PLACEMENT_CANCELLED_PARTIAL_BATCH
PLACEMENT_FAILED_PARTIAL_BATCH
COMPLETED
```

El resultado estructurado contiene RackId, conteos solicitado y colocado, orden, direccion actual o fallida, estado
de redraw, razon terminal, diagnostico y warnings. No contiene strings de UI como autoridad. El resultado es
inmutable; no expone transiciones posteriores a un estado terminal.

## 2. Identidad y compatibilidad single-view

`RackEditorSession.RequestInsertViews` recibe la lista ordenada completa. Una intencion nueva aceptada obtiene un solo
RackId de la factory y lo comparte entre todas sus vistas; una sesion adoptada conserva el RackId existente sin llamar
a la factory. Una entrada invalida se rechaza antes de acuñar identidad.

`RackInsertionRequest.Views` expone la lista tipada ordenada. Las seis solicitudes historicas de insercion individual
siguen produciendo `Views.Count == 1` y conservan sus propiedades y semantica first/history. Los requests de update
siguen sin representar vistas nuevas. No se cambio el gesto visible `Insertar` de G9b.

## 3. Prepare, redraw y placement

El plan selecciona todas las variantes y prepara todas las vistas antes de cualquier escritura. Una cancelacion de
variante termina en `VARIANT_CANCELLED`; un rechazo, indisponibilidad o fallo de preparation termina en
`PREPARE_FAILED`. Ambos dejan cero redraw y cero placements.

Para un rack existente, el sibling gate ocurre una vez despues de Prepare All. Un fallo produce
`SIBLING_GATE_FAILED`. El redraw de G9a/G9b se representa mediante una sola invocacion: solo `REDRAW_APPLIED` habilita
la cola; `REDRAW_ROLLED_BACK` y `PREFLIGHT_FAILED` dejan cero placements. Para un rack nuevo no existe redraw de
hermanas y el plan entra por `REDRAW_NOT_REQUIRED` despues de Prepare All.

Cada placement es independiente y reutilizable por el futuro driver G12 sobre G8. Esc o Enter producen
`PLACEMENT_CANCELLED_PARTIAL_BATCH`, conservan el redraw ya confirmado y toda vista colocada anteriormente. Si se
detiene el primer jig, el conteo colocado es cero, pero el redraw existente permanece. Una excepcion real produce
`PLACEMENT_FAILED_PARTIAL_BATCH`, preserva lo ya confirmado y no ejecuta items posteriores. `COMPLETED` solo existe
cuando `placed == requested`.

Proposal V5 distingue este fallo puro del caller Plugin actual: ese caller colapsa `PromptStatus.Error` a cancelacion.
G11 prueba el estado de fallo mediante excepcion; G12 sera responsable del mapeo Autodesk sin cambiar este contrato.

## 4. Casos de producto

- Un warning tipado por library requirement faltante se agrega al reporte y no aborta: se conserva `place + report`
  de G8-CR-01 para ID18.
- Una vista invalida, no expuesta o unavailable si detiene Prepare All.
- Flow Bed no adquiere sisters y rechaza `Views.Count > 1`.
- Un rack parcial existente puede pedir sisters soportadas sin exigir la primera vista historica.
- Requests repetidas conservan orden y multiplicidad; G11 no introduce una unicidad incidental.
- El plan solo modela una invocacion del seam G9b; no replica scan, membership, gates ni transaccion de redraw.
- No hay regen real, dialogo, presenter, boton, comando ni driver Plugin.

## 5. RED y GREEN

El RED focal inicial no compilo porque no existian el namespace `RackCad.Application.Views.Batch`,
`RackViewBatchPlan` ni sus tipos. Despues de implementar el contrato, la primera Core Full detecto que una guarda G10
todavia prohibia cualquier `IReadOnlyList<RackViewAddress>`; se actualizo esa guarda porque G11 exige la lista ordenada,
manteniendo la obligacion de que Cabecera conserve identidad y address explicitas.

Resultados de iteracion:

| Seleccion | Resultado |
|---|---|
| Batch Core focal | 15/15 PASS |
| Batch/session/request UI focal | 13/13 PASS |
| Impacto Core G6-G9b + G11 | 91/91 PASS |
| Impacto UI G10/session/batch | 61/61 PASS |

Cada seleccion ejecuto mas de cero pruebas. No se agregaron skips.

## 6. Archivos y limites

Producto:

```text
src/RackCad.Application/Views/Batch/RackViewBatchPlan.cs
src/RackCad.UI/Editor/CantileverComponentInsertionRequest.cs
src/RackCad.UI/Editor/RackEditorSession.cs
src/RackCad.UI/Editor/RackInsertionRequest.cs
```

Pruebas:

```text
tests/RackCad.Tests/FirstViewFreedomGuardTests.cs
tests/RackCad.Tests/RackViewBatchPlanTests.cs
tests/RackCad.UI.Tests/EditorViewBatchRequestTests.cs
tests/RackCad.UI.Tests/RackEditorSessionTests.cs
tests/RackCad.UI.Tests/RackInsertionRequestTests.cs
```

Plugin product diff = `NONE`. Foundation diff = `NONE`. Schema diff = `NONE`. No se modificaron Domain, DTO, stores,
assets, eng, deploy, workflows o comandos visibles. ID19 y AUTH-15 permanecen fuera.

## 7. Evidencia exacta del producto

| Evidencia sobre `991d1694c39377fe7f03d67b024b5a9fcc8dd093` | Resultado |
|---|---|
| Core Full local | 8411/8411 PASS |
| UI Full local | 1613 PASS / 17 historical skipped |
| Build UI Debug | PASS / 0 warnings / 0 errors |
| Build Plugin Debug | PASS / 0 errors; solo los dos MSB3277 conocidos |
| SDK resuelto | 8.0.423 |
| CI push exacta | `35800250527` / 4/4 SUCCESS / `head_sha=991d1694c39377fe7f03d67b024b5a9fcc8dd093` |

La CI fue `event=push`, `ref=refs/heads/feature/creacion-de-vistas`; terminaron en success Tests Domain/Application,
UI Tests, Build UI y Build Plugin without AutoCAD.

## 8. Owner Validation y cierre

Owner Validation: `NOT REQUIRED FOR THIS GATE`. G11 no agrega consumidor visible. La ejecucion `OV-ID18` corresponde
despues de G12 y en el Candidato.

```text
PR-1 = RESOLVED / UNCHANGED
PR-2 = RESOLVED / UNCHANGED
ID17 = COMPLETE
ID18 = CONTRACT COMPLETE / NOT YET USER-VISIBLE
ID19 = NOT IMPLEMENTED
AUTH-15 = NOT IMPLEMENTED
Foundation diff = NONE
Schema diff = NONE
Material contradictions = NONE
Open Material = NONE
Open Minor = NONE
G11 = COMPLETE
G12 = OPEN
```
