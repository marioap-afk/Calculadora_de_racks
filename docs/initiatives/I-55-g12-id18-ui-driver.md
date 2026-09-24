# I-55 — G12 ID18 UI, presenter y driver Plugin

Fecha: 2026-09-23

Gate: G12

`PRE_REBASE_I55_SHA`: `08e45e0a2521a2e7feb4cf7c8d3fc7fc574aa74d`

`POST_REBASE_BASE`: `c75e7434a909d396c05e55c39a140ba53be9e98c`

`G12_START_SHA`: `335ac8c9701b4ef2731d96464de9fd0fabade2b3`

`PRODUCT_SHA`: `cdc4843d1abbf12a3158ae65fd60edde5aeadceb`

Proposal V5: gobernante e inmutable. ADR-0042: aceptada.

## 1. Reconciliacion I-58 y cierre de G12-CR-01

El tag anotado `integration/I-58` (objeto `8c87d923c981dc2db7f56375ea06a19673414874`) apunta a
`c75e7434a909d396c05e55c39a140ba53be9e98c`, ancestro de `origin/main`. Su mensaje acredita candidato
`85d746f0f760608b60e4c992e76c33287e197bdc`, cierre `7409ca957a2ab968b503b11a0adcf0d923055c4d`,
CI post-merge `35931780127 / SUCCESS` y coverage de candidato `35925861392 / SUCCESS`.

Antes del rebase se preservo el tip I-55 mediante el tag anotado
`archive/i-55-pre-i58-integration-08e45e0`. El rebase conserva los commits historicos y situa I-55 sobre el merge
I-58. La comparacion de rango no encontro perdida semantica; solo reconcilio el drift documental de ROADMAP.

G12-CR-01 queda `RESOLVED BY I-58 INTEGRATION`. I-55 consume, sin comparador ni canonicalizador local, los ports
AUTH-13 de Selective, Dynamic, Push Back, Cantilever y Cabecera. `Single` entrega una autoridad authored unica;
`Divergent` y `Unreadable` fallan cerrados antes de Resolve, redraw y placement. Cama permanece fuera.

## 2. UI y presenter

`RackViewBatchDialog` permite seleccionar y ordenar. `RackViewBatchDialogPresenter`, fuera de las ventanas de sistema
y sin Autodesk, filtra mediante `RackViewExposure(..., Batch)`, conserva el orden y devuelve `RackViewAddress`
tipadas. Fondo, PostIndex, Entrance/Exit, side/end y Station viajan en la direccion; ningun `SelectedIndex` es
identidad.

Selective, Dynamic, Push Back, Cantilever y Cabecera exponen el gesto batch. Las laterales Dynamic/Push Back se
enumeran por PostIndex fisico, Cantilever por Station y Selective por fondo/poste. Flow Bed no ofrece el gesto.
Cancelar el dialogo no produce request ni escritura.

## 3. Preparacion, authored gate y driver

`RackViewBatchProductSession` compone Product Prepare sobre los ports Foundation existentes. Para un rack nuevo usa un
solo `NewRackCreationContext`, por lo que toda la cola comparte el RackId aceptado. Para uno existente conserva el Id,
cachea el comparator oficial y el Resolve port: una cola ejecuta una sola comparacion authored y una sola resolucion.

Los cuatro kinds desbloqueados por I-58 reutilizan el snapshot fisico de `RackSiblingScan` tanto para AUTH-13 como
para la lista de hermanas; no recorren de nuevo el BlockTable. Selective mantiene el seam G9a/G9b. Todos preparan la
cola completa antes del redraw y solo habilitan placements tras el resultado aplicado/no requerido.

`RackViewBatchDriver` adapta AutoCAD al state machine G11. Verifica `TopTransaction == null` antes de cada jig y delega
cada placement a G8. `OK` coloca, `Cancel` cancela parcialmente, `None` detiene sin excepcion y cualquier estado real
de fallo, incluido `Error`, termina en `PLACEMENT_FAILED_PARTIAL_BATCH`. Esc/Enter conservan redraw y placements ya
confirmados. La regeneracion de nuevas vistas ocurre una vez al final de la cola.

Los reports de requirements de G8 se agregan como warnings estructurados del batch. Una pieza de biblioteca faltante
conserva `place + report`; no adopta la politica estricta de ID19.

## 4. Matriz de sistemas

| Kind | Variantes batch | AUTH-13 | Resultado |
|---|---|---|---|
| Selective | Fondo, Post, Planta | Foundation Selective | habilitado |
| Dynamic | Entrance/Exit, Post, Planta | Foundation Dynamic I-58 | habilitado |
| Push Back | side/end, PostIndex fisico, Planta | Foundation PushBack I-58 | habilitado |
| Cantilever | Frontal, Station, Planta con `PlantaVisibility` | Foundation Cantilever I-58 | habilitado |
| Cabecera | Lateral, Planta | Foundation Cabecera I-58 | habilitado |
| Cama | ninguna cola multivista | fail-closed | excluido |

## 5. Cancelacion, error y compatibilidad

- Cancelar antes de aceptar el dialogo produce cero writes.
- Un fallo de Prepare All produce cero redraw y cero placements.
- Cancelar el primer jig conserva el redraw y coloca cero vistas nuevas.
- Cancelar o detener un jig posterior conserva las vistas anteriores y no inicia el resto.
- `PromptStatus.Error` queda separado de Cancel/None y produce fallo parcial.
- `COMPLETED` exige que `placed == requested`.
- Una lista de una vista conserva la semantica single-view de G8/G10.
- No se implementan ID19, `RACKPROYECTAR` ni AUTH-15.

## 6. RED, GREEN y evidencia

El RED inicial fallo por ausencia del dialogo, presenter y driver. Tras I-58 se activaron primero las guardias de
consumo para los cuatro comparators: cada port oficial aparece en el consumer y los resultados distintos de `Single`
no seleccionan first, majority o fallback. Las focales finales seleccionaron mas de cero pruebas.

| Evidencia | Resultado |
|---|---|
| Batch/authored/impacto Core focal | 47/47 PASS |
| Dialogo/census UI focal | 11/11 PASS |
| Driver/state machine/single scan focal | 20/20 PASS |
| Core Full local previo al PRODUCT_SHA | 11401/11401 PASS |
| UI Full local de iteracion | 1616 PASS / 17 historical skipped |
| Build UI Debug | PASS / 0 warnings / 0 errors |
| Build Plugin Debug | PASS / 0 errores; solo MSB3277 conocidos |
| CI push exacta del PRODUCT_SHA | `35939888447` / 4/4 SUCCESS |

SDK resuelto: `8.0.423`. No se agregaron skips.

## 7. Archivos y limites

El cambio productivo se limita a Application batch reporting, UI/dialogo/session forwarding y adapters Plugin de
preparacion, authored gate, redraw y placement. No modifica Domain, DTO de persistencia, assets, schema, Foundation
AUTH-01..13, I-58, Proposal V5, ADR-0042 ni AUTH-15.

Foundation diff propio de I-55: `NONE`. Schema diff: `NONE`.

I-52 observado en `c27b6354476ed3afce3591ea9c477d4493ee8a9e`; no hubo overlap productivo con G12. AUTH-15 no esta integrado en
main y permanece independiente, por lo que G15 sigue bloqueado.

## 8. Owner Validation y salida

Owner Validation: `DEFERRED TO CANDIDATE`. G12 hace ID18 visible, pero Workflow V1 no exige validacion manual local a
este gate. OV-ID18 conserva Selective F+L+P, Esc segundo, Enter final, edit/update desde lateral, save/reopen/edit
desde planta, Undo, Dynamic existente con cancelacion inicial, Push Back compuesto, Cantilever con stations/planta,
cancelacion de variante, caso huerfano y redraw/cancel.

```text
G12-CR-01 = RESOLVED BY I-58
PR-1 = RESOLVED / UNCHANGED
PR-2 = RESOLVED / UNCHANGED
ID17 = COMPLETE
ID18 = COMPLETE / USER-VISIBLE
ID19 = NOT IMPLEMENTED
AUTH-15 = NOT IMPLEMENTED
Foundation diff = NONE
Schema diff = NONE
Material contradictions = NONE
Open Material = NONE
Open Minor = NONE
G12 = COMPLETE
G14 = OPEN
G15 = BLOCKED BY AUTH-15 / I-52
```
