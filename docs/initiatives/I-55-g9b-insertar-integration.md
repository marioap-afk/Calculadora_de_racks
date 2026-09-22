# I-55 — G9b Insertar integration

Fecha: 2026-09-22

Gate: G9b

`G9B_START_SHA`: `705ae301f696b7f16c8ec006e2b1ab526be46489`

SHA de producto: `fa8f6affa769f87afe795fd310fbc74c43494123`

`origin/main` observado: `7097057cf8685bf5ecc09083cba37379d4a4aae8`

I-52 observado: `faaf709bf6401dcda91b07f41afe44f490fb916a`

Proposal V5: gobernante y congelada (blob `38bee23a3a886a5026664bfda6d260ba5c65fc26`)

Implementation Map V5: blob `17f6969ad2272dca5530d8533b6cbfd2afc05d23`

## 1. Composicion de Insertar

`RackSiblingInsertRun` fija el orden unico:

```text
scan/classify once -> variant -> Custom Properties -> authored -> Resolve/Prepare
-> prepare every redraw unit -> atomic Mutate -> Post -> TopTransaction guard -> G8 Place(1)
```

El mismo `RackSiblingMembershipSnapshot` alimenta properties, authored y redraw. El barrido Plugin recorre una vez el
`BlockTable`, conserva definiciones, sobres, properties, conteos de referencias y layers, e incluye dependencias xref
como hechos read-only. No existe scan por gate, por hermana ni antes de placement.

Selective consume el comparator AUTH-13 vigente, una llamada al puerto Resolve y una llamada a Product Prepare. La
preparacion de unidades de G9a delega a los builders y facades caller-owned existentes por address; no crea geometria
neutral paralela. Dynamic, Push Back, Cantilever y Header consumen sus puertos AUTH-13 de Foundation y terminan
`Unreadable` porque esos comparadores siguen explicitamente no demostrados. No eligen first/majority/fallback y no
mutan el DWG. Flow Bed permanece excluido.

## 2. Identidad, gates y variantes

- Selective calcula el `RackId` de Insertar con `IsNullOrWhiteSpace` y lo toma una sola vez de la ventana. La fuente
  elegida participa aunque su Id original este vacio o tenga espacios; hermanas con el mismo Id de espacios son
  atribuibles y reciben el mismo Id curado.
- Header calcula un unico Guid solo en su rama Insertar. Su fuente y hermanas de Id de espacios entran al snapshot,
  pero AUTH-13 no demostrado detiene el flujo antes de persistir la cura.
- Dynamic conserva la cura historica de la ventana; AUTH-13 detiene antes de escribir. Push Back y Cantilever conservan
  su conducta de identidad y tambien fallan cerrados en AUTH-13.
- Actualizar conserva su camino historico. En Selective y Header su calculo de Id queda en una rama separada con
  `IsNullOrEmpty`; no pasa por `RackSiblingInsertRun`.
- La variante se decide en la ventana existente. Selective resuelve planta, frontal por fondo y lateral por post antes
  de los gates. Cancelar el prompt produce `VariantCancelled` y cero gate, Resolve, Prepare, Mutate o placement.

`RackSiblingCustomPropertiesGate` usa exactamente `CustomPropertiesGateMembers`: single/equivalent continua;
ilegible, read-only inesperado o divergente falla con remedio hacia `RACKPROPIEDADES`. Un ilegible no atribuible no
entra al conjunto. El gate authored usa exactamente `AuthoredGateMembers` y precede a Resolve.

## 3. CQ-01, transacciones y reporte

G9a prepara todas las unidades `Redraw` y `Erase` antes de escribir, valida layers de referencias directas y entidades
existentes y devuelve `PREPARE_FAILED` con vista/definicion/layer. Una MUTATE posee una transaccion y un commit; fallo
tipado, excepcion o fault Debug descarta todo y evita placement. Post corre solo despues del commit. Antes de G8 se
verifica `Database.TransactionManager.TopTransaction == null`; una violacion produce `PlacementBlocked`.

Los outcomes tipados distinguen preflight, rollback, redraw aplicado/no requerido y placement aplicado/cancelado/fallido.
El diagnostico agrega sisters read-only, definiciones borradas, warnings de Post y el conteo del reporte de requirements
de G8. Los missing blocks de ID17/ID18 siguen siendo `place + report`; no se convierten en gate estricto de ID19.

En `Survivors = empty && Erase != empty`, AUTH-15 no esta integrada y se usa exclusivamente modo 2. G8 crea la
definicion antes del jig; tras `OK`, un callback generico aplica todas las unidades `Redraw + Erase` dentro de la misma
transaccion del jig y antes de añadir la referencia. Cancelar no ejecuta el callback y G8 intenta limpiar la definicion;
R-25 permanece como residual si esa limpieza falla. No se implemento creacion caller-owned ni deteccion por reflection.

## 4. Archivos y limites

Producto nuevo principal:

```text
src/RackCad.Application/Views/Insertion/
src/RackCad.Plugin/RackSelectivoInsertIntegration.cs
src/RackCad.Plugin/Views/RackSiblingScan.cs
src/RackCad.Plugin/Views/RackUnsupportedSiblingInsert.cs
src/RackCad.Plugin/Systems/Shared/AutoCadSiblingRedrawPort.cs
```

Se ampliaron los facades caller-owned de Dynamic, Push Back y planta de Header, el seam transaccional G9a, el callback
generico de G8 y los cinco entry points reales de `RACKEDITAR`. No se creo comando paralelo. No hay cambios en Domain,
UI, assets, eng, deploy, workflows, DTO, stores ni formatos persistidos.

Foundation diff = `NONE`: no se modificaron AUTH-01..13 ni los archivos neutralizados por I-57. Schema diff = `NONE`.
ID17 arbitrario, ID18 queue/batch, ID19 y AUTH-15 permanecen fuera.

## 5. Pruebas y oraculos

Las pruebas nuevas cubren un scan, reutilizacion por identidad del snapshot, cancelacion de variante, rechazo de
properties/authored antes de Resolve, una preparacion, layer bloqueada, rollback, Post antes de placement,
`TopTransaction`, modo 2, estados terminales de placement, sonda de Id y properties equivalentes/divergentes. Las
guardas requeridas comprueban wiring real, predicados blank por rama, transaccion del primer jig, facades caller-owned,
fault Debug y exclusion de Flow Bed.

La seleccion focal final fue 103/103 PASS; el censo de sobres y sus mutaciones fue 172/172 PASS. Un primer intento de
pruebas y build en paralelo encontro el lock local `CS2012`; se descarto como evidencia y todas las corridas validas se
hicieron en serie. No se agregaron skips.

Como control RED reproducible, se invirtio temporalmente la condicion de aceptacion de variante en el driver y se
ejecutaron sus 11 casos: 11/11 fallaron, incluyendo orden, cancelacion, gates, layer, rollback, transaccion abierta,
placement y huerfanas. Tras restaurar byte-for-byte el archivo desde el SHA de producto, la misma seleccion dio
11/11 PASS. La mutacion no se commiteo.

## 6. Evidencia exacta del producto

| Evidencia sobre `fa8f6affa769f87afe795fd310fbc74c43494123` | Resultado |
|---|---|
| RED control del driver | 11 seleccionadas / 11 FAIL |
| GREEN del mismo driver | 11/11 PASS |
| Focal G9b + impactos G9a/G8/G7/Foundation | 103/103 PASS |
| Guards de sobre afectados | 172/172 PASS |
| Core Full local | 8391/8391 PASS |
| UI Full local | 1587 PASS / 17 historical skipped |
| Build UI Debug | PASS / 0 warnings / 0 errors |
| Build Plugin Debug | PASS / 0 errors; solo los dos MSB3277 conocidos |
| Build Plugin Release | PASS; sentinel Debug ausente |
| SDK resuelto | 8.0.423 |
| CI push exacta | `35789416985` / 4/4 SUCCESS / `head_sha=fa8f6affa769f87afe795fd310fbc74c43494123` |

La Owner Validation visible queda `DEFERRED TO CANDIDATE`. Escenarios acumulados: OV-RED-01 redibujo normal,
OV-RED-02 properties divergentes, OV-RED-03 layer bloqueada, OV-RED-04 solo-huerfanas/cancelacion y residual modo 2,
OV-RED-06 fallo Debug de segunda unidad y OV-RED-07 cura blank. OV-RED-05 pertenece al batch futuro.

## 7. Cierre

```text
PR-1 = RESOLVED / UNCHANGED
PR-2 = RESOLVED / UNCHANGED
AUTH-15 = NOT IMPLEMENTED
Foundation diff = NONE
Schema diff = NONE
Owner Validation = DEFERRED TO CANDIDATE
Material contradictions = NONE
Open Material = NONE
Open Minor = NONE
G9b = COMPLETE
G10 = OPEN
```
