# I-55 — G10 ID17 first-view freedom

Fecha: 2026-09-22

Gate: G10

`G10_START_SHA`: `67779989233ef190810d2c5a61e1f1e499e31983`

SHA de producto: `c96ca4ecc382c1ccc4e8603de8fbaebecb666d32`

`origin/main` observado: `7097057cf8685bf5ecc09083cba37379d4a4aae8`

I-49 observado: candidato `589e3db5536ae9cc9af9c051e694b4c1803f0196`, integrado en
`a61850a6095cf8ebc7d9d92eab6dbbcc2343a6d1`; sin rama ni worktree activo y sin edicion concurrente de los archivos
calientes de G10. Clasificacion: `NONE / STABLE`.

I-52 observado: `88a0ad70f47a681c9d94dac33726519d9aabf8f8`; el avance desde G9b fue documental y no modifico seams compartidos.
Clasificacion: `NON-MATERIAL`.

Proposal V5: gobernante y congelada (blob `38bee23a3a886a5026664bfda6d260ba5c65fc26`)

Implementation Map V5: blob `17f6969ad2272dca5530d8533b6cbfd2afc05d23`

## 1. Matriz de primera vista

| Sistema | Primeras vistas expuestas por producto | Resultado G10 |
|---|---|---|
| Selectivo | Frontal por fondo, Lateral por post, Planta | Las tres quedan disponibles para un rack nuevo. |
| Dinamico | Frontal entrada, Frontal salida, Lateral por post, Planta | Las cuatro quedan disponibles y conservan direccion tipada. |
| Cabecera | Lateral, Planta | La ventana y los entry paths transportan la seleccion explicita. |
| Push Back | Las vistas ya expuestas por G6 | Sin cambio productivo; se conserva la libertad medida y `Post(index)`. |
| Cantilever | Las vistas ya expuestas por G6 | Sin cambio productivo; se conserva `Station(index)` y PlantaVisibility. |
| Cama | Solo su vista `CreateFirst` vigente | No adquiere sisters ni capacidades multivista. |

Las ventanas de Selectivo, Dinamico y Cabecera consultan `RackViewExposure.IsExposed(..., CreateFirst)`. No existe
una segunda matriz en UI. Los sentinels usados para habilitar los botones son direcciones tipadas; la variante concreta
elegida se transporta en el request. Una creacion sigue colocando exactamente una vista: no se agregaron checkboxes,
queue, batch presenter, proyeccion de grupos ni `CommonTransform2D`.

## 2. Identidad, requests y entry paths

Las sesiones nuevas de Selectivo y Dinamico ya poseian una `RackEditorIdentity`; cada boton permitido emite una sola
solicitud con ese mismo `RackId`. La cabecera ahora posee la misma clase de identidad y guarda `InsertAddress`.
`HeaderInsertionRequest` exige explicitamente `RackId` y `RackViewAddress InitialAddress`; se retiro el constructor que
implicaba lateral. `EditorModules` solo enruta esos valores y no decide `ViewKind`.

`RackMenuCommands` entrega identidad y address al Plugin. `RackCabeceraCommands.DrawAndPlace` valida `CreateFirst`,
codifica la direccion con el codec compartido y despacha al servicio de Planta o Lateral existente. `RACKCABECERA`
transporta la eleccion de la ventana; `QUICKCABECERA` conserva expresamente su comportamiento lateral historico. No se
crearon comandos paralelos.

Una intencion aceptada acuña exactamente un Guid antes del jig y ese valor viaja en el payload. La vista inicial no
crea otra identidad. Las inserciones posteriores siguen entrando por `RACKEDITAR -> Insertar` y por la composicion de
G9b, que comparte el RackId con la nueva sister. Authored, CustomProperties y el diseño aceptado no se reconstruyen
desde geometria.

## 3. Cancelacion, colocacion y conteo logico

G10 no modifica el host de colocacion de G8. Esc o Enter antes de colocar termina sin referencia ni rack usable y sin
acuñar una segunda identidad; se conserva el cleanup best effort y el residual R-25 documentado mientras AUTH-15 siga
fuera. Tampoco cambia la ruta posterior G9b: una sister usa el mismo RackId, los gates de authored y CustomProperties,
y el redibujo atomico vigente.

El BOM y `RACKLISTA` siguen agrupando por identidad logica. Elegir Planta, Lateral o una variante frontal como primera
vista no convierte la vista en otro rack. Las suites completas conservan las regresiones de conteo, cancelacion,
placement, G9b, Push Back y Cantilever.

## 4. RED y GREEN

El primer RED de `FirstViewFreedomTests` no compilo: tres llamadas exigian el contrato nuevo de Cabecera y
`HeaderInsertionRequest` todavia no transportaba identidad ni direccion tipada. Despues del cambio minimo, una prueba
fallo por construccion directa de una ventana Selective fuera de su fixture; la suite Full detecto ademas la guarda que
prohibe esa construccion. El test se corrigio para usar `SelectiveWindowTestSupport.Open`; no se relajo la guarda.

La seleccion final de first-view freedom cubre:

- Selective: botones Frontal/Lateral/Planta y request real para las tres;
- Dynamic: botones Entrance/Exit/Lateral/Planta y request real para las cuatro;
- Cabecera: Lateral/Planta, Guid unico y address de Planta tipada;
- ownership de exposure para Selective, Dynamic y Header;
- request/routing sin coleccion de vistas, sin default implicito y sin eleccion de kind en el registry;
- remocion de los rechazos historicos y de sus modals.

Resultados de iteracion: first-view focal 21/21 PASS; focal con la guarda de construccion Selective 23/23 PASS;
entry/historicos 59/59 PASS; impactos de editores 107/107 PASS; Core relevante G6-G9b, BOM/list, Push Back y
Cantilever 487/487 PASS. No se agregaron skips.

## 5. Archivos y limites

Producto:

```text
src/RackCad.Plugin/RackCabeceraCommands.cs
src/RackCad.Plugin/RackMenuCommands.cs
src/RackCad.UI/Editor/EditorModules.cs
src/RackCad.UI/Editor/RackInsertionRequest.cs
src/RackCad.UI/RackFrames/RackFrameConfiguratorWindow.xaml.cs
src/RackCad.UI/Systems/Dynamic/RackDynamicSystemWindow.xaml.cs
src/RackCad.UI/Systems/Selective/RackSelectiveWindow.xaml.cs
```

Pruebas:

```text
tests/RackCad.Tests/FirstViewFreedomGuardTests.cs
tests/RackCad.UI.Tests/FirstViewFreedomTests.cs
tests/RackCad.UI.Tests/DynamicHeaderBatchSeamGuardTests.cs
tests/RackCad.UI.Tests/DynamicShellMigrationTests.cs
tests/RackCad.UI.Tests/FirstViewGateCharacterizationTests.cs
tests/RackCad.UI.Tests/RackInsertionRequestTests.cs
tests/RackCad.UI.Tests/SelectiveHeaderBatchSeamTests.cs
```

Foundation diff = `NONE`. Schema diff = `NONE`. No se modificaron Domain, DTO, stores, assets, eng, deploy o
workflows. ID18, ID19 y AUTH-15 permanecen fuera. G9b solo recibe las firmas explicitas necesarias en Cabecera; su
driver, scan, gates, transaccion y placement no cambian.

## 6. Evidencia exacta del producto

| Evidencia sobre `c96ca4ecc382c1ccc4e8603de8fbaebecb666d32` | Resultado |
|---|---|
| Core Full local | 8396/8396 PASS |
| UI Full local | 1608 PASS / 17 historical skipped |
| Build UI Debug | PASS / 0 warnings / 0 errors |
| Build Plugin Debug | PASS / 0 errors; solo los dos MSB3277 conocidos |
| SDK resuelto | 8.0.423 |
| CI push exacta | `35795365919` / 4/4 SUCCESS / `head_sha=c96ca4ecc382c1ccc4e8603de8fbaebecb666d32` |

La CI fue `event=push`, `ref=refs/heads/feature/creacion-de-vistas`; terminaron en success Tests Domain/Application,
UI Tests, Build UI y Build Plugin without AutoCAD.

## 7. Owner Validation diferida

Estado: `DEFERRED TO CANDIDATE`; no se declara PASS en G10.

Plan `OV-ID17`:

1. Selective nuevo -> Planta first -> Insertar frontal fondo 2.
2. Selective nuevo -> Lateral first -> Insertar frontal + Planta.
3. Dynamic nuevo -> Exit first -> luego Lateral.
4. Dynamic nuevo -> Entrance first -> luego Lateral.
5. Dynamic nuevo -> Planta first -> luego Lateral.
6. Cabecera -> Planta first -> luego Lateral.
7. Push Back y Cantilever: cada primera vista soportada.
8. Cancelar jig alternativo -> no queda rack usable ni referencia.
9. Guardar/reabrir -> BOM y `RACKLISTA` cuentan un rack logico.
10. Id blank legacy, cuando aplique, sigue la cura de G9b.

## 8. Cierre

```text
PR-1 = RESOLVED / UNCHANGED
PR-2 = RESOLVED / UNCHANGED
ID17 = COMPLETE
ID18 = NOT IMPLEMENTED
ID19 = NOT IMPLEMENTED
AUTH-15 = NOT IMPLEMENTED
Foundation diff = NONE
Schema diff = NONE
Owner Validation = DEFERRED TO CANDIDATE
Material contradictions = NONE
Open Material = NONE
Open Minor = NONE
G10 = COMPLETE
G11 = OPEN
```
