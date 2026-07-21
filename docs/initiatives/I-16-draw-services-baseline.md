# I-16 — Línea base de equivalencia de Draw Services

> Fase F1 de [I-16](I-16-refactor-draw-services.md). Inventario mecánico del comportamiento **actual**
> (previo al refactor) para poder comparar mecánicamente el antes y el después. Los nombres de bloque,
> mensajes, firmas y el patrón de `Regen` viven en `src/RackCad.Plugin` (que referencia AutoCAD) y se
> conservan por **inventario y revisión de código**, no por golden automatizado. Las estructuras de plan
> reachables sin AutoCAD se fijan además con las pruebas de `tests/RackCad.Tests`
> (`DrawServicePlanBaselineTests`). Las referencias `archivo:línea` son ubicaciones de código, no hashes.

## 1. Matriz de servicios y especializaciones

Los **candidatos de consolidación** son los siete servicios «cascarón»; **permanece especializado**
`LateralHeaderDrawService`. Todos producen un `DynamicSystemPlan` y lo entregan a `SystemBlockWriter`
(salvo la ruta propia de la cabecera lateral).

| Servicio (`src/RackCad.Plugin`) | Vista | Payload | Fábrica de `DynamicSystemPlan` (exacta) | Forma del plan | Especialización |
|---|---|---|---|---|---|
| `Systems/SelectiveFrontalDrawService` | frontal | `SelectiveRackSystem` | `new SelectiveFrontalBuilder().BuildPlan(system, catalog)` | builder | — |
| `Systems/SelectivePlantaDrawService` | planta | `SelectiveRackSystem` | `new SelectivePlantaBuilder().BuildPlan(system, catalog)` | agrupado (ARRAY) | sufijo `- planta` |
| `Systems/DynamicSystemDrawService` | lateral | `DynamicRackSystem` | `builder.Build(system, catalog)` / `builder.Build(system, catalog, postIndex)` | agrupado | **`postIndex`** |
| `Systems/DynamicFrontalDrawService` | frontal | `DynamicRackSystem` | `new DynamicSystemFrontalBuilder().BuildPlan(system, catalog, end)` | builder | **`DynamicRackEnd end`** + sufijo entrada/salida |
| `Systems/DynamicPlantaDrawService` | planta | `DynamicRackSystem` | `new DynamicSystemPlantaBuilder().BuildPlan(system, catalog)` | builder | sufijo `- planta` |
| `Systems/FlowBedDrawService` | lateral (cama) | `FlowBedConfiguration` | `new DynamicSystemPlan(new List<HeaderGroup>(), builder.Build(config, catalog))` | **all-loose** | `RedrawInPlace` **sin** parámetro `regen` |
| `Headers/PlantaHeaderDrawService` | planta (cabecera) | `RackFrameConfiguration` | `new DynamicSystemPlan(new List<HeaderGroup>(), builder.Build(config, catalog))` | **all-loose** | sufijo `- planta` |
| `Headers/LateralHeaderDrawService` | lateral (cabecera) | `RackFrameConfiguration` | ruta propia (`LateralHeaderLayoutBuilder` + `HeaderInstanceGrouper`) | agrupado | **ESPECIAL**: `extraInstances`/`Merge`, `DrawAt`, reconstrucción de conteos; hospeda `LoadCatalog`/`PlaceAndReport`/`DescribeMissing` |

## 2. Invariantes observables (deben preservarse)

- **Forma del plan por vista**: `all-loose` (cama, cabecera planta) vs `agrupado` (dinámico lateral,
  selectivo planta, cabecera lateral). No unificar ni reagrupar: el generic debe tratar la fábrica de
  plan como opaca.
- **`postIndex`** (dinámico lateral) y **`DynamicRackEnd end`** (dinámico frontal) cambian el plan y/o el
  nombre; no pueden eliminarse de la firma.
- **Nombres de bloque y sufijos por vista** (§3), **mensajes** (§4) y **firmas públicas** (§5) verbatim.
- **Un único `Regen` final** por operación multivista (§6).
- **Limpieza tras cancelación del jig** y reconstrucción de conteos de la cabecera lateral (§7).
- **Payload/GUID (`RackBlockData`), geometría (`LateralHeaderDrawer`), BOM y persistencia**: intactos.

## 3. Nombres de bloque (por defecto y override de `rackName`)

`rackName` en blanco ⇒ nombre por defecto; `rackName` presente ⇒ override (con o sin sufijo).

| Servicio | Por defecto (InvariantCulture) | Override con `rackName` |
|---|---|---|
| SelectiveFrontal | `Selectivo frontal - {Bays.Count} frentes - H{Height:0.##}` | `rackName.Trim()` (sin sufijo) |
| SelectivePlanta | `Selectivo planta - {Bays.Count} frentes` | `rackName.Trim() + " - planta"` |
| DynamicSystem (lateral) | `Sistema dinamico - {PalletsDeep} fondos - L{TotalLength:0.##}` | `rackName.Trim()` (sin sufijo) |
| DynamicFrontal | `Dinamico {frontal entrada\|frontal salida} - {Fronts.Count} frentes` | `rackName.Trim() + " - " + {frontal entrada\|frontal salida}` |
| DynamicPlanta | `Dinamico planta - {Fronts.Count} frentes` | `rackName.Trim() + " - planta"` |
| FlowBed | `Cama {pushback\|dinamica} - fondo {LaneDepth:0.##}` | `rackName.Trim()` (sin sufijo) |
| PlantaHeader | `Cabecera planta` | `rackName.Trim() + " - planta"` |
| LateralHeader | `Cabecera {descPoste} - F{Depth:0.##} A{Height:0.##}` | `rackName.Trim()` (sin sufijo) |

Regla de sufijo: `- planta` en SelectivePlanta/DynamicPlanta/PlantaHeader; `- frontal entrada`/
`- frontal salida` en DynamicFrontal; sin sufijo en los otros cuatro. El sufijo es *load-bearing* para
que las vistas de un mismo rack no colisionen de nombre ni fuercen `UniqueBlockName` a añadir `_1`.

## 4. Mensajes observables (verbatim)

- Documento nulo (los ocho): `"No hay un dibujo activo en AutoCAD."`
- Payload nulo — **sustantivo dibujar / actualizar** (asimétrico dentro de un servicio):

| Servicio | Dibujar | Actualizar |
|---|---|---|
| SelectiveFrontal / SelectivePlanta | `No hay sistema selectivo para dibujar.` | `No hay rack para actualizar.` |
| DynamicSystem (lateral) | `No hay sistema para dibujar.` | `No hay sistema para actualizar.` |
| DynamicFrontal / DynamicPlanta | `No hay sistema dinámico para dibujar.` | `No hay sistema dinámico para actualizar.` |
| FlowBed | `No hay cama para dibujar.` | `No hay cama para actualizar.` |
| PlantaHeader / LateralHeader | `No hay configuracion para dibujar.` | `No hay cabecera para actualizar.` |

Nota: solo DynamicFrontal/DynamicPlanta escriben «dinámico» acentuado; DynamicSystem escribe «sistema»
llano. `configuracion` va sin acento. Preservar tal cual.

## 5. Firmas públicas actuales (por conservar)

`DrawAndPlace` devuelve `HeaderPlacementResult`; `RedrawInPlace` idem. Firmas relevantes:

- SelectiveFrontal / SelectivePlanta: `DrawAndPlace(Document, SelectiveRackSystem, string payloadJson=null, string rackName=null)`; `RedrawInPlace(Document, ObjectId blockId, SelectiveRackSystem, string payloadJson, bool regen=true)`.
- DynamicSystem: `DrawAndPlace(Document, DynamicRackSystem, string payloadJson=null, string rackName=null, int postIndex=-1)`; `RedrawInPlace(Document, ObjectId, DynamicRackSystem, string payloadJson, bool regen=true, int postIndex=-1)`.
- DynamicFrontal: `DrawAndPlace(Document, DynamicRackSystem, DynamicRackEnd end, string payloadJson=null, string rackName=null)`; `RedrawInPlace(Document, ObjectId, DynamicRackSystem, DynamicRackEnd end, string payloadJson, bool regen=true)`.
- DynamicPlanta: `DrawAndPlace(Document, DynamicRackSystem, string=null, string=null)`; `RedrawInPlace(Document, ObjectId, DynamicRackSystem, string payloadJson, bool regen=true)`.
- FlowBed: `DrawAndPlace(Document, FlowBedConfiguration, string=null, string=null)`; `RedrawInPlace(Document, ObjectId, FlowBedConfiguration, string payloadJson)` — **sin `regen`**.
- PlantaHeader: `DrawAndPlace(Document, RackFrameConfiguration, string=null, string=null)`; `RedrawInPlace(Document, ObjectId, RackFrameConfiguration, string payloadJson, bool regen=true)`.
- LateralHeader (especial): `DrawAndPlace(Document, RackFrameConfiguration, string=null, string=null, IReadOnlyList<HeaderBlockInstance> extraInstances=null)`; `RedrawInPlace(Document, ObjectId, RackFrameConfiguration, string payloadJson, IReadOnlyList<HeaderBlockInstance> extraInstances=null, bool regen=true)`; `DrawAt(Document, RackFrameConfiguration, Point3d, string=null, string=null)`; `PlaceLayout(...)`; `PlaceAndReport(Document, RackCatalog, LateralHeaderBlockResult)`; `static RackCatalog LoadCatalog()`; `internal static string[] DescribeMissing(...)`.

Sitios de llamada (comandos → servicios / `LoadCatalog`):

- `RackSelectivoCommands`: SelectiveFrontal (redraw :118, draw :311), SelectivePlanta (redraw :157, draw :261), LateralHeader para cortes (:131, :384); `LoadCatalog` (:130, :340).
- `RackCabeceraCommands`: LateralHeader (draw :167, redraw :242), PlantaHeader (redraw :253, draw :274/:282); `LoadCatalog` (:65).
- `RackDinamicoCommands`: DynamicPlanta (draw :107, redraw :190), DynamicFrontal (draw :111, redraw :203), DynamicSystem (draw :120/:322, redraw :226); `LoadCatalog` (:41, :182, :286).
- `RackCamaCommands`: FlowBed (draw :102/:167, redraw :209); `LoadCatalog` (:41).
- `RackInventarioCommands.BomTotal`: solo `LoadCatalog` (:74).
- `RackLayoutCommands` / `.Fill`: **no** instancian ningún DrawService (solo su `Regen` propio).

## 6. Mapa de `Regen`

Exactamente siete `document.Editor.Regen()` en el Plugin:

| # | Sitio | Guarda | Rol |
|---|---|---|---|
| A | `Headers/LateralHeaderDrawService.cs:98` | `if (regen)` | por bloque (cabecera / corte lateral) |
| B | `Systems/SystemBlockWriter.cs:72` | `if (regen)` | por bloque (compartido por los 6 servicios de sistema) |
| C | `RackSelectivoCommands.cs:183` | `if (contador de cambios > 0)` | **único final** — selectivo multivista |
| D | `RackCabeceraCommands.cs:264` | `if (updated > 0)` | **único final** — cabecera multivista |
| E | `RackDinamicoCommands.cs:253` | `if (contador de cambios > 0)` | **único final** — dinámico multivista |
| F | `RackLayoutCommands.cs:242` | incondicional | layout |
| G | `RackLayoutCommands.Fill.cs:475` | incondicional | fill |

- **Default `regen = true`** en las ocho firmas que lo tienen (SelectiveFrontal :56, SelectivePlanta :48,
  DynamicSystem :65, DynamicFrontal :46, DynamicPlanta :48, PlantaHeader :48, LateralHeader :58,
  SystemBlockWriter :47). **FlowBed.RedrawInPlace no tiene el parámetro** (:49) y siempre regenera vía el
  default de B.
- Los editores multivista pasan `regen: false` en cada redibujo intermedio (Selectivo :118/:142/:157;
  Cabecera :243/:254; Dinámico :191/:204/:226) y disparan **un** `Regen` final (C/D/E), gateado por el
  contador (incluye vistas borradas / `erasedPhantoms`).
- Cama (`EditCama` :209): sin `Regen` a nivel de comando; su único regen es el interno (B, single-view).
- Inserción (jig), `DrawAndPlace`, `AppendReference`, `CreateBlock`, `RACKDUPLICAR`: **sin** `Regen`.

## 7. Mapa de colocación y cancelación del jig

- **Inserción interactiva**: `PlaceBlockWithJig` + `HeaderInsertionJig` (`LateralHeaderDrawService.cs:341-368, 423-460`), envuelto por `PlaceAndReport` (:204-231), que los siete servicios consumen por `new LateralHeaderDrawService().PlaceAndReport(...)`.
- **Cancelación del jig** (`placedId.IsNull`, :215-219): `EraseUnreferencedDefinition` (:236-282) borra la definición fantasma (que ya lleva payload) **y** sus defs anidadas (purga post-commit), para que el escaneo por GUID de `RACKEDITAR` no redibuje una vista fantasma.
- **Colocación en punto fijo** (sin jig): `DrawAt` → `AppendReference` (:162-179) — cortes laterales del selectivo.
- **Redibujo**: `SystemBlockWriter.RedrawInPlace` (:45-83) = lock → `EnsureForPlan` → transacción → `RedefineSystemBlock` → `RackBlockData.Write` → commit → `PurgeUnreferenced` → `Regen` opcional. `LateralHeaderDrawService.RedrawInPlace` (:58-109) tiene su propia secuencia (Merge `extraInstances` → group → redefine → purge → regen).
- **Reconstrucción de conteos**: `LateralHeaderDrawService.CreateBlock` (:284-319) reconstruye el `LateralHeaderDrawOutcome` con el layout real y el conteo de-inflado (el drawer devuelve layout vacío y conteo inflado por prototipos de def anidada); lo consume `RackCabeceraCommands` (:313-315). `SystemBlockWriter.CreateBlock` **no** hace esa reconstrucción.
- **Colaboradores compartidos**: `SystemBlockWriter` (orquestación de transacción write/redraw + toggle de regen), `BlockLibraryImporter.EnsureForPlan`, `LateralHeaderDrawer` (geometría — fuera de alcance), `RackBlockData` (Xrecord — fuera de alcance), y los estáticos `LoadCatalog`/`DescribeMissing` de `LateralHeaderDrawService`.

## 8. Cobertura de pruebas agregada

`tests/RackCad.Tests/DrawServicePlanBaselineTests.cs` (solo Application, sin AutoCAD; reconstruye cada
plan **exactamente** como su DrawService):

- `DynamicLateral_IsGrouped_AndFlattenExpandsEveryPlacement` — dinámico lateral es agrupado; `Flatten` expande grupos×placements + loose.
- `FlowBed_DrawServicePlanShape_IsAllLoose` — cama: `Headers` vacío, `LooseInstances` no vacío.
- `PlantaHeader_DrawServicePlanShape_IsAllLoose` — cabecera planta: idéntico all-loose.
- `AllLoose_And_Grouped_ShapesCoexist_SoTheGenericMustNotUnifyThePlanFactory` — coexisten ambas formas.
- `DynamicLateral_PostIndexOverload_ProducesADistinctNonEmptyCut` — el overload `postIndex` produce un corte no vacío y acotado por el plan completo.
- `DynamicFrontal_EntranceAndExit_ProduceDistinctNonEmptyPlans` — `DynamicRackEnd` cambia el plan.
- `DynamicViews_Lateral_Frontal_Planta_AreThreeDistinctPlans` — las tres vistas ligadas son planes distintos.
- `Selective_FrontalAndPlanta_AreDistinctNonEmptyPlans` — frontal vs planta del selectivo difieren.

Estas pruebas fijan la **estructura de plan** (agrupado/all-loose, `postIndex`, `DynamicRackEnd`,
distinción por vista) que el refactor debe preservar. La suite completa preexistente de `RackCad.Tests`
sigue siendo la red de regresión de la geometría de los builders.

## 9. Aspectos que solo se validan en build del Plugin o AutoCAD

- **Nombres de bloque y sufijos** (§3), **mensajes** (§4) y **firmas/propagación de `regen`/`postIndex`/`end`** (§5): viven en `src/RackCad.Plugin` (referencia AutoCAD); no reachables desde `RackCad.Tests`. Se verifican por inventario mecánico y revisión de código, y el **build Debug del Plugin** en CI (`Build Plugin without AutoCAD`) los compila.
- **Colocación con jig, cancelación e limpieza de definiciones, purga, `Editor.Regen`, `RecordGraphicsModified`, extents, round-trip de Xrecord**: solo observables en **AutoCAD** (validación por muestreo del dueño al integrar; I-16 no está marcada con ✋).
- **Impresión de conteos de la cabecera lateral** («N piezas (X horizontales, Y diagonales)»): Plugin + AutoCAD.

## 10. Validación de esta fase

- Pruebas y builds locales: **no ejecutables en esta estación** — no hay .NET SDK instalado (solo runtimes; `global.json` fija SDK `8.0.423`). No se instala SDK ni se altera la estrategia vigente.
- Validación efectiva: **CI de la rama `refactor/draw-services`** (Tests + Build UI + Build Plugin without AutoCAD), que sí dispone del SDK. Las pruebas nuevas y la suite completa se ejecutan allí tras el push.
- Sin validación manual en AutoCAD en esta fase.

## 11. Comparación final F4 — uniformidad de `regen`

F4 cambia **cómo** se aplica el flag, no **cuándo** se regenera. Los dos guardados de redibujo
byte-idénticos (antes en `SystemBlockWriter.RedrawInPlace` y en `LateralHeaderDrawService.RedrawInPlace`)
se unificaron en un único helper `SystemBlockWriter.ApplyRegen(document, regen)`, invocado desde ambos
**en la misma posición**: tras `Commit` y `PurgeUnreferenced`, dentro del lock del documento.

- **Ubicaciones efectivas de regeneración: 7 (sin cambio)** — las dos de redibujo (ahora vía `ApplyRegen`),
  los tres finales-únicos multivista (Selectivo/Cabecera/Dinámico), layout y fill.
- **Llamadas literales `Editor.Regen()`: 7 → 6** — las dos de redibujo comparten ahora el cuerpo de
  `ApplyRegen`; las otras cinco quedan intactas.
- `ViewBlockDraw` no aplica regen: reenvía el flag a `SystemBlockWriter.RedrawInPlace`.
- `FlowBedDrawService.RedrawInPlace` conserva su firma sin `regen` y su `regen: true` interno.
- Sin cambios: comandos (`regen: false` intermedios + el único `Regen` final gateado), layout/fill,
  cancelación del jig, y el orden `Commit` → `Purge` → `Regen`.
