# I-55 — G8 Single-view placement

Fecha: 2026-09-21

Gate: G8

`G8_START_SHA`: `15ad90aae749ce8bce78b5f59bca28e27097e86a`

SHA de producto: `11004cf00d8f0f98765aba6cf01f1af2b8dbabeb`

`origin/main` observado: `7097057cf8685bf5ecc09083cba37379d4a4aae8`

I-52 observado: `faaf709bf6401dcda91b07f41afe44f490fb916a`

Proposal: V5 gobernante y congelada (blob `38bee23a3a886a5026664bfda6d260ba5c65fc26`)

Implementation Map V5: blob `17f6969ad2272dca5530d8533b6cbfd2afc05d23`

## 1. G8-CR-01

La primera orden de G8 contradecia Proposal V5 §3.5 al pedir que un requirement faltante bloqueara la colocacion.
La ejecucion se detuvo antes de modificar produccion. La resolucion vinculante del Coordinator reafirmo la matriz
congelada: G8 es el seam reutilizable de una vista para ID17/ID18 y aplica `place + report`; la politica fail-before-
write de ID19 permanece reservada a G14/G15.

| Hecho de requirement | ID17/ID18 en G8 | ID19, fuera de G8 |
|---|---|---|
| Required key null/blank | colocar y reportar `InvalidKey`; no fabricar clave | fallar antes de puntos |
| Valid key `Missing` tras import | colocar y reportar | fallar antes de escribir rack |
| Library file missing | colocar y reportar `LibraryUnavailable` | fallar antes de escribir |
| OptionalVisual missing | colocar y reportar warning | warning |
| Cero requirements | cero import/query; colocar | conforme al plan estricto futuro |

`G8-CR-01 = RESOLVED`. Proposal V5, ADR-0042 e ID19 no cambiaron; Proposal V6 no fue necesaria.

## 2. Frontera entregada

`RackSingleViewPlacement` recibe directamente `RackPreparedProductView<TPayload>` de G7. Ejecuta preflight,
evaluacion de requirements, creacion de una definicion, jig de una referencia, post y resultado estructurado. No llama
Resolve, Prepare, codec, builders de geometria ni generacion de identidad. Conserva el `RackId`, address canonica,
envelope, `BaseName`, requirements y payload ya preparados.

El resultado discrimina:

```text
Placed
PlacedWithReport
Cancelled
DefinitionFailed
PlacementFailed
CleanupFailed
BlockedForNonRequirementReason
```

El reporte tipado conserva `Key`, `Piece`, `Role`, `Issue` y `Cause`. `Role` distingue `Required` de
`OptionalVisual`; `Issue` distingue `Missing` de `InvalidKey`; `Cause` hace explicita
`LibraryUnavailable`. Ningun faltante por si solo produce `BlockedForNonRequirementReason`.

## 3. Availability y materializacion

El adaptador Plugin usa el orden de Foundation `Ensure/import -> final query`; la consulta sigue pura y separada del
importer. Cero requirements retorna antes de ambos puertos. Los facts finales se convierten a reporte mediante
`RackSingleViewRequirementReporting`, politica de I-55 en Application. Los blanks se observan en el plan tipado y no
se convierten en `LibraryBlockRequirement` ni pasan por sanitizacion.

Las cinco familias basadas en `HeaderRunPlan` reutilizan `LateralHeaderDrawer` mediante
`SystemBlockWriter.CreatePreparedBlock`; Cantilever reutiliza
`CantileverViewMaterializer.CreateBlockDefinitionNamed`. Los drawers historicos omiten piezas no disponibles y no se
inventa geometria de reemplazo. `BaseName` solo alimenta la politica de colision Plugin; nunca sustituye una key de
biblioteca. El nombre unico resultante queda separado de ambos.

Sentinels:

| Sistema | Payload/materializador |
|---|---|
| Selective | `HeaderRunPlan` / writer vigente |
| Dynamic | `HeaderRunPlan` / writer vigente |
| Push Back | `HeaderRunPlan` / writer vigente |
| Cantilever | `CantileverViewPlan` / materializador vigente |
| Header | `HeaderRunPlan` / writer vigente |
| Flow Bed | `HeaderRunPlan` / writer vigente; admite cero requirements |

## 4. Persistencia, transacciones y cleanup

La definicion se confirma antes del jig, residual historico mientras AUTH-15 no exista. La referencia se coloca en la
transaccion propia del jig. El payload se serializa desde `product.Envelope` y se escribe en la definicion; por ello
conserva el `RackId` y la direccion canonica compuestos en G7, sin reinterpretar View/Section.

En exito hay una definicion, una referencia y un unico regen POST. En cancelacion o fallo despues de crear la
definicion, el flujo intenta borrar la definicion no referenciada y sus nested defs privados. La limpieza tiene
resultado observable: si falla, el outcome es `CleanupFailed`. Las rutas historicas `PlaceAndReport` y
`PlaceDefinition` tambien intentan cleanup ante excepcion. No se promete atomicidad caller-owned ni rollback de una
definicion ya confirmada.

| Resultado jig/materializacion | Referencia | Cleanup |
|---|---:|---|
| exito | una | no aplica |
| cancelacion | cero | intentado; fallo reportado |
| excepcion/fallo tras definicion | cero | intentado; fallo reportado |
| fallo al crear definicion | cero | no existe definicion que limpiar |

## 5. RED -> GREEN

La primera seleccion ejecuto 17 casos y fallo 17. Trece fallos probaron el seam deliberadamente incompleto; cuatro
detectaron fixtures con variantes no expuestas y se corrigieron sin cambiar produccion. Tras implementar, ampliar el
oraculo al reporte real y agregar guardas Plugin:

```text
RED focal: 17/17 FAIL
GREEN focal final: 22/22 PASS
```

El regression directo de G8-CR-01 demuestra `required Missing -> PlacedWithReport`, una referencia y un item con la
key faltante. Los tests adicionales cubren FileMissing, blank sin clave fabricada, OptionalVisual, cero I/O con cero
requirements, cancelacion, excepcion, cleanup fallido, identidad/sobre/nombre y seis sistemas. Cada filtro selecciono
al menos una prueba.

## 6. Archivos

Producto y adaptadores:

```text
src/RackCad.Application/Views/Placement/RackSingleViewPlacement.cs
src/RackCad.Plugin/Views/RackViewPlacement.cs
src/RackCad.Plugin/Drawing/BlockPlacement.cs
src/RackCad.Plugin/Systems/Shared/SystemBlockWriter.cs
```

Pruebas:

```text
tests/RackCad.Tests/RackSingleViewPlacementTests.cs
tests/RackCad.Tests/RackViewPlacementGuardTests.cs
```

Foundation diff = `NONE`: no cambio ningun archivo bajo `Systems/Shared`, ni AUTH-01..13. Schema diff = `NONE`:
ningun DTO, store o formato persistido cambio. No hay cambios en Domain, UI, assets, eng, deploy o workflows.

## 7. Evidencia sobre el SHA de producto

| Evidencia | Resultado |
|---|---|
| RED focal | 17 seleccionadas / 17 FAIL |
| GREEN focal + guardas | 22/22 PASS |
| Core Full local | 8337/8337 PASS |
| UI Full local | 1587 PASS / 17 historical skipped |
| Build UI Debug | PASS / 0 warnings / 0 errors |
| Build Plugin Debug | PASS / 0 errors; solo los dos MSB3277 conocidos |
| SDK resuelto | 8.0.423 |
| CI push exacta | `35683553699` / 4/4 SUCCESS / `head_sha=11004cf00d8f0f98765aba6cf01f1af2b8dbabeb` |

No se agregaron skips. El primer intento concurrente de Core/UI produjo un lock de compilacion `CS2012`; no cuenta
como evidencia. Ambas suites se ejecutaron despues en serie sobre el mismo SHA limpio y quedaron verdes.

## 8. Limites y cierre

G8 entrega el seam y los adaptadores reutilizables. No conecta todavia la UX completa de ID17, no crea siblings,
queue, batch, transformacion grupal ni transaccion de ID19. G9a/G9b conservan sus gates separados. La validacion
manual se difiere al Candidato final porque este gate no expone un comando nuevo completo y las verificaciones
visuales acumuladas de I-55 se ejecutan juntas sobre el SHA final exacto.

```text
PR-1 = RESOLVED / UNCHANGED
PR-2 = RESOLVED / UNCHANGED
G8-CR-01 = RESOLVED
AUTH-15 = NOT IMPLEMENTED / residual documentado
Foundation diff = NONE
Schema diff = NONE
Owner Validation = DEFERRED TO CANDIDATE
Material contradictions = NONE
Open Material = NONE
Open Minor = NONE
G8 = COMPLETE
G9a = OPEN
G9b = NOT OPEN
```
