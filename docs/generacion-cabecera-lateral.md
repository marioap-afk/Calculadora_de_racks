# Generación de cabecera lateral (block-based)

Esta es la lógica para generar una **cabecera en vista lateral** a partir de **bloques
independientes anclados a puntos de conexión** (no una composición visual libre). El **poste es la base
geométrica**; horizontales y diagonales de celosía cuelgan de la línea de troqueles del poste. La cabecera
es **uno de los cuatro tipos de rack** que dibuja el plugin (cabecera, sistema dinámico, cama de rodamiento
y selectivo); el **mismo patrón por bloques + round-trip** descrito aquí sirve a los cuatro (ver
[Patrón unificado](#patrón-unificado-block-based--round-trip-para-los-4-tipos)).

> **Estado:** la **lógica pura** y el **dibujo en AutoCAD** de la cabecera lateral ya están implementados y
> compilan en Windows. Los comandos `RACKCABECERA` / `QUICKCABECERA` y el botón **Insertar en AutoCAD**
> del configurador dibujan la cabecera como **un bloque**;
> `RACKEDITAR` la reabre y la **redefine en sitio** para que todas las copias se actualicen a la vez. La
> cabecera tiene además una vista **PLANTA** ligada por GUID a la lateral (solo se inserta desde `RACKEDITAR`
> de una vista existente, para que nunca quede huérfana); al confirmar una edición se **redibujan todas las
> vistas** del sistema. Los bloques se resuelven desde la biblioteca vía `blocks.csv`
> (`FindBlock(pieceId, view)` es la ruta activa). Ver la
> sección [Paso 2](#paso-2-estado-del-cableado-en-autocad).

## Arquitectura (separación pura ↔ AutoCAD)

```
RackCad.Application/Headers/            (PURO, testeable en cualquier SO)
  LateralHeaderParameters         parámetros editables (sin números mágicos)
  LateralHeaderParametersFactory  config → parámetros (mapea RackFrameConfiguration → LateralHeaderParameters)
  HeaderBlockInstance             una inserción del plan (qué, dónde, params dinámicos)
  LateralHeaderLayout             el plan completo + totales
  LateralHeaderLayoutBuilder      la lógica (7 pasos)
  PlantaHeaderLayoutBuilder       el plan de la vista PLANTA (huellas de poste + celosía colapsada)

  La geometría de los puntos NO tiene una clase "resolver": se resuelve con
  CatalogLookup (RackCad.Application/Catalogs), que lee connection-layout.csv
  (coords locales por vista) y blocks.csv (nombres de bloque por vista).

RackCad.Plugin/Headers/                (AutoCAD, solo Windows)
  LateralHeaderDrawer         adapter: convierte el plan en un bloque (InsertBlock + params dinámicos)
  LateralHeaderDrawService    orquesta: carga catálogo, arma el plan, crea el bloque y lo coloca (jig)
  PlantaHeaderDrawService     dibuja la vista PLANTA como bloque, ligada por GUID a la lateral (jig)
  LateralHeaderDrawOutcome    resultado del dibujo: piezas insertadas + bloques referidos pero no definidos
  LateralHeaderBlockResult    el bloque creado (definición + nombre final) que devuelve el drawer
  BlockLibraryImporter        importa del DWG biblioteca los bloques que falten en el dibujo activo
  RackBlockRenamer            sincroniza el nombre de la definición del bloque con el nombre del rack
```

Regla de oro: **toda la geometría y los cálculos son puros**; el drawer solo traduce el plan a la API de
AutoCAD. Así la lógica se prueba sin AutoCAD y el drawer queda mínimo.

## Parámetros (`LateralHeaderParameters`)

| Parámetro | Default | Significado | ¿Dónde se edita? |
|---|---|---|---|
| `Height` | 132 | Altura; mueve el parámetro dinámico `LONGITUD` del poste | Editor **clásico** |
| `Depth` | 42 | Fondo: separación entre los dos postes | Editor **clásico** |
| `PasoTroquel` | 2 | Paso entre troqueles del poste | Configurable (`config.PasoTroquel`) |
| `OffsetDiagonalInicioTroqueles` | 2 | La diagonal arranca N troqueles arriba de la horizontal inferior | Editor **avanzado** |
| `OffsetDiagonalFinTroqueles` | 2 | La diagonal termina N troqueles abajo de la horizontal superior | Editor **avanzado** |
| `DiagonalDoubleSpacingTroqueles` | 1 | Separación en troqueles entre las dos diagonales paralelas de un panel de doble diagonal | Editor **avanzado** |
| `HorizontalDoubleOffsetTroqueles` | 1 | Cada travesaño extra de una horizontal doble (`Quantity > 1`) sube N troqueles sobre el anterior | Editor **avanzado** |
| `PeralteParameter` | `PERALTE` | Nombre del parámetro dinámico de peralte que recibe el override manual de la placa base | (nombre de parámetro) |
| `ValorClaroTravesano` | auto | Claro sobrante arriba (legado: existe pero el builder ya **no** lo usa; el cierre es `closingGap` calculado, ver Paso 7) | auto / opcional |

En el editor, estos viven en `RackFrameConfiguration` (`Height`, `Depth`, `CelosiaStartTroquel`,
`DiagonalStartOffsetTroqueles`, `DiagonalEndOffsetTroqueles`) y persisten en el proyecto. La **placa base**
tiene además un **peralte editable por placa** (`BasePlatePlacement.PeralteOverride`; `null` = derivado con
`StandardPeralte = base + slope·peralte_poste`). En modo **Configuración rápida**, "Insertar" genera la
cabecera en un clic (igual que el resto de tipos).

## Puntos de conexión (la lógica se ancla en estos)

| Punto | Vive en | Rol |
|---|---|---|
| `MONTAJE_POSTE` | placa base | El `(0,0)` del poste coincide aquí |
| `TROQUEL_CELOSIA` | poste | Primer troquel = línea de referencia de la celosía |
| `FIN_POSTE` | poste | Punto donde encaja el origen de un refuerzo; su X da el desplazamiento del troquel interno del refuerzo |
| `CELOSIA` | travesaño (celosía) | El punto del travesaño que cae sobre la línea de troqueles |

Sus posiciones 2D por vista están en `connection-layout.csv` y los nombres de bloque por vista en
`blocks.csv` (ver [modelo-de-datos.md](modelo-de-datos.md)).

## Algoritmo (`LateralHeaderLayoutBuilder.Build`)

Ejes en vista lateral: **X = fondo (entre postes), Y = altura**.

1. **Poste izquierdo + placa** en X=0: la placa se inserta primero, el poste con su `(0,0)` sobre
   `MONTAJE_POSTE`; se fija `LONGITUD = Height`.
2. **Poste derecho espejeado** con origen en `X = Depth`.
3. **Línea de troqueles** desde `TROQUEL_CELOSIA` (derecho espejeado):
   `LongitudHorizontal = X_troquel_dcho − X_troquel_izq`.
4. **Y de horizontales**: vienen **explícitas** de la `RackFrameConfiguration`;
   **`RackFrameConfigurationFactory`** es quien calcula esas elevaciones paramétricamente: primer travesaño en
   el troquel de inicio (`yFirst = Y_troquel0 + (CelosiaStartTroquel−1)·PasoTroquel`), paneles de `PanelClear`
   (44") y cierres de 0/1/2. **`LateralHeaderParametersFactory` solo mapea** la `RackFrameConfiguration` a
   `LateralHeaderParameters` (ids, offsets, paso…); **no** calcula elevaciones. Las elevaciones de las
   plantillas JSON ya **no** se usan (solo aportan perfiles/placa/poste/puntos) y **no** escalan
   proporcionalmente.
5. **Horizontales**: el punto `CELOSIA` cae sobre la línea de troquel a su Y; largo = `LongitudHorizontal`.
6. **Diagonales** (una por panel): arranca `OffsetInicio` troqueles arriba de la horizontal inferior y
   termina `OffsetFin` troqueles abajo de la superior; **largo y ángulo se calculan de los puntos reales**.
7. **Cierre**: el builder ya **no** agrega una "Horizontal de cierre" (rol `ClosingHorizontal`); solo calcula
   `closingGap = max(0, Height − topHorizontalY)`. Las elevaciones —incluida la que hace que la horizontal
   superior caiga **exacto** en `Y = Height − PostTopRemate` (remate = 4")— las precalcula
   `RackFrameConfigurationFactory` (`const PostTopRemate = 4.0`), y el builder **solo lee** `config.Horizontals`.
   Así el **alto construido = el alto pedido** (240 → 240).

Ejemplo (Height 132, Depth 42, paso 2, inicio 3, claro 44, troquel local X=2):
horizontales en Y = 4 / 48 / 92 (largo 38), cierre en Y = 128 (= 132 − remate de 4"), 2 diagonales.

### Capacidades del builder (además del caso base)

El builder no se limita al poste-poste con una diagonal por panel; sobre el mismo plan por bloques soporta:

- **Postes y placas INDEPENDIENTES por lado**: cada lado resuelve su propio poste/placa (izquierdo desde
  `config.LeftPost`/`LeftBasePlate`, derecho desde `config.RightPost`/`RightBasePlate`); el lado derecho **no**
  hereda en silencio los ids del izquierdo, y cada placa acepta su `PeralteOverride`.
- **Refuerzos por lado**: opcionalmente un lado lleva un **segundo poste** anclado en `FIN_POSTE` (`Y=0`) con su
  propio `LONGITUD`, que cubre la zona inferior. La celosía que cae dentro de la zona reforzada se ancla al
  **troquel interno del refuerzo** (una anchura de poste más adentro) en vez del troquel del poste base.
- **Horizontales dobles**: una horizontal con `Quantity > 1` apila travesaños extra, cada uno
  `HorizontalDoubleOffsetTroqueles` troqueles arriba del anterior.
- **Cuatro patrones de arriostramiento** por panel (`BracingPanel.Arrangement`): `SingleDiagonal` (una diagonal
  en la dirección del panel), `DoubleDiagonal` (dos diagonales paralelas separadas por
  `DiagonalDoubleSpacingTroqueles`), `XBracing` (una `UpRight` + una `UpLeft` cruzadas) y `AutoAlternating`
  (alterna la dirección de la diagonal panel a panel). Cada extremo toma el troquel más interno disponible
  (refuerzo donde exista, si no el poste), así una diagonal puede arrancar en el refuerzo y terminar en el
  poste liso.

## El plan: `HeaderBlockInstance`

Cada inserción trae: `Role` (BasePlate/Post/Horizontal/Diagonal/ClosingHorizontal), `BlockName`, `View`,
`Insertion` (origen del bloque), `ConnectionAnchor` (dónde cae su punto de referencia), `RotationRadians`,
`MirroredX`, y `DynamicParameters` (p. ej. `LONGITUD`, `Distancia1`). El drawer (`LateralHeaderDrawer`)
recorre el plan, agrega cada pieza como una `BlockReference` (posición + rotación + espejo `X=-1`) y fija sus
parámetros dinámicos por nombre; las piezas cuyo bloque no exista en el dibujo se **omiten y se reportan**.

## Paso 2: estado del cableado en AutoCAD

El dibujo (`RackCad.Plugin/Headers/`) **ya está cableado**:

1. ✅ **Mapeo config → parámetros**: `LateralHeaderParametersFactory.FromConfiguration` (capa pura,
   `RackCad.Application/Headers/`, con tests) arma `LateralHeaderParameters` desde la `RackFrameConfiguration`:
   `Height`, `Depth`,
   `OffsetDiagonalInicioTroqueles = DiagonalStartOffsetTroqueles`,
   `OffsetDiagonalFinTroqueles = DiagonalEndOffsetTroqueles` y los ids reales
   (`PostId`/`BasePlateId`/`TrussProfileId`).
2. ✅ **Servicio de dibujo**: `RackCad.Plugin/Headers/LateralHeaderDrawService.cs` carga el catálogo
   (`JsonRackCatalogProvider.FromBaseDirectory().Load()`), bloquea el documento, arma **un bloque** con todas
   las piezas (`LateralHeaderDrawer.CreateSystemBlock` agrupa los cortes idénticos en UNA definición anidada
   referenciada N veces — patrón ARRAY, igual que frontal/planta) y lo coloca con el mouse (jig). Implementa la
   interfaz `RackCad.UI.IHeaderDrawService` para que la
   WPF dispare el dibujo **sin** referenciar las DLLs de AutoCAD (regla de oro: la UI no conoce AutoCAD).
3. ✅ **Importación de bloques**: `BlockLibraryImporter.EnsureForPlan` clona en el dibujo activo los bloques
   que falten desde un DWG biblioteca (`blocks-library.dwg`, junto a los catálogos; ruta configurable). Se lee
   en una base lateral sin abrirlo en AutoCAD; los bloques que tampoco existan en la biblioteca se omiten.
4. ✅ **Comandos y botón**: `QUICKCABECERA` pide la cabecera por línea de comandos (poste/fondo/alto);
   `RACKCABECERA` abre el configurador. El **botón
   "Insertar en AutoCAD"** dibuja la cabecera **que el usuario configuró**.
5. ✅ **Round-trip**: la cabecera se **embebe** en la definición del bloque y `RACKEDITAR` la reabre y la
   **redefine en sitio** (`LateralHeaderDrawService.RedrawInPlace` → `RedefineSystemBlock` + `Regen`), de modo
   que todas las copias se actualizan sin moverse (ver [Patrón unificado](#patrón-unificado-block-based--round-trip-para-los-4-tipos)).
6. ✅ **Reporte de bloques faltantes**: `BuildAndDraw`/`RedrawInPlace` devuelven un `LateralHeaderDrawOutcome`
   con las piezas insertadas y los **bloques referidos pero no definidos** (se omiten en vez de lanzar). El
   comando y el botón informan al usuario qué bloques faltan modelar.

### Bloques y biblioteca (DWG)

- Los bloques referidos en `blocks.csv` (vista `LATERAL`) viven en `blocks-library.dwg` y **sí se consumen**
  al dibujar: `FindBlock(pieceId, view)` es la ruta activa de los 4 tipos. El `blockName` (columna 3) debe
  coincidir **exacto** con el nombre del bloque en la biblioteca DWG:
  - el bloque del **poste** con el parámetro dinámico **`LONGITUD`**;
  - los bloques de **travesaño** con **`Distancia1`** (largo);
  - cada bloque con sus **puntos de conexión** y su posición en `connection-layout.csv` para la vista `LATERAL`
    (placa → `MONTAJE_POSTE`, poste → `TROQUEL_CELOSIA`, travesaño → `CELOSIA`).
- Los catálogos son "Excel-first" (el `.csv` gana sobre el `.json`, acepta UTF-8 y ANSI de Excel) con caché
  invalidada por firma de archivos: editar el CSV y relanzar el comando recarga.

## Patrón unificado (block-based + round-trip) para los 4 tipos

El dibujo por **bloques** y el **round-trip de edición** no son exclusivos de la cabecera: son un mecanismo
único reutilizado por los **cuatro tipos de rack**.

- **Cada rack dibujado = UNA definición de bloque**; las copias son referencias a ella. Redefinir la
  definición actualiza todas las copias a la vez, sin recolocarlas.
- **Sobre embebido unificado** `RackEmbedDocument { SchemaVersion, Kind, View, Section, Id (GUID), Name,
  Design (JSON) }` guardado en la **definición** del bloque (diccionario de extensión, `Xrecord` troceado
  ≤255 vía `RackBlockData`). El `Name` es el nombre del bloque ("Rack A"); el `Id` (GUID) evita colisiones.
  `Kind` ∈ `{ selective, dynamic, cabecera, cama }`; `View` ∈ `{ frontal, lateral, planta }`; `Section` es el
  índice de corte lateral del selectivo (−1 = vista no seccionada).
- **`RACKEDITAR`**: seleccionas un rack → lee el sobre de su definición → **despacha por `Kind`** → reabre el
  editor correcto precargado (`LoadExisting`) → al confirmar **redefine la definición en sitio**
  (`RedefineSystemBlock` + `Regen`).
- **Servicios de dibujo por tipo** (cada uno con `DrawAndPlace` + `RedrawInPlace`) y **store** del diseño:

  | Kind | Comando | Editor (WPF) | Servicio de dibujo | Store del diseño |
  |---|---|---|---|---|
  | `cabecera` | `RACKCABECERA` / `QUICKCABECERA` | `RackFrameConfiguratorWindow` | `LateralHeaderDrawService` | `RackProjectStore` |
  | `dynamic` | `RACKSISTEMADINAMICO` | `RackDynamicSystemWindow` | `DynamicSystemDrawService` | `RackProjectStore` |
  | `cama` | `QUICKCAMA` | `RackFlowBedWindow` | `FlowBedDrawService` | `FlowBedConfigurationStore` |
  | `selective` | `RACKSELECTIVO` | `RackSelectiveWindow` | `SelectiveFrontalDrawService` | `SelectivePalletDesignStore` |

  El menú principal es `RACKCAD`. Vistas por tipo (`views.csv`): el selectivo dibuja **FRONTAL + LATERAL +
  PLANTA** (la lateral es un bloque por poste, con corte elegido por número de poste al insertar); la cabecera
  **LATERAL + PLANTA**; dinámico y cama solo **LATERAL**. Las vistas lateral/planta se insertan **únicamente**
  desde `RACKEDITAR` de una vista existente (los botones se deshabilitan con tooltip si no aplica), y
  `RACKEDITAR` sobre **cualquier** vista reabre el editor del sistema completo y al confirmar redibuja
  **todas** las vistas (encontradas por GUID escaneando las definiciones de bloque).
- **Escalable**: agregar un tipo nuevo = su `Kind` + `Edit<Kind>` en `RackFrameCommands` + `LoadExisting` en su
  ventana + embed/`RedrawInPlace` en su servicio de dibujo.

## Supuestos de geometría (ya verificados)

1. Eje de espejo del poste derecho: vertical en el fondo ⇒ `LongitudHorizontal = Depth − 2·inset_troquel`.
2. Elevaciones **explícitas** de la configuración (paneles de `PanelClear` + cierres de 0/1/2); la horizontal
   superior cae en `Height − PostTopRemate`, así el alto construido = el pedido.
3. Una diagonal por panel es el caso base (zigzag estándar), pero la **alternancia** (`AutoAlternating`) y la
   **X** (`XBracing`) **ya están implementadas** por panel (no diferidas), igual que la doble diagonal.

## Dónde está en el código

- Lógica pura: `src/RackCad.Application/Headers/` (builder, parámetros, `LateralHeaderParametersFactory`
  config→parámetros y `PlantaHeaderLayoutBuilder` para la vista PLANTA); la geometría de los puntos la resuelve
  `CatalogLookup` en `src/RackCad.Application/Catalogs/` (connection-layout.csv + blocks.csv).
- Adapter AutoCAD: `src/RackCad.Plugin/Headers/LateralHeaderDrawer.cs` (+ `LateralHeaderDrawOutcome.cs`,
  `LateralHeaderBlockResult.cs`, `BlockLibraryImporter.cs`).
- Servicio de dibujo (puente UI↔AutoCAD): `src/RackCad.Plugin/Headers/LateralHeaderDrawService.cs`,
  que implementa `src/RackCad.UI/IHeaderDrawService.cs`.
- Round-trip: sobre `src/RackCad.Application/Persistence/RackEmbedDocument.cs` (`RackEmbedStore`), embebido con
  `src/RackCad.Plugin/Systems/RackBlockData.cs`; comandos y despacho en `src/RackCad.Plugin/RackFrameCommands.cs`
  (`RACKEDITAR`, `EditCabecera`/`EditDynamic`/`EditCama`/`EditSelective`).
- Comandos de cabecera: `RACKCABECERA`, `QUICKCABECERA` en
  `src/RackCad.Plugin/RackFrameCommands.cs`; botón "Insertar en AutoCAD" en
  `src/RackCad.UI/RackFrameConfiguratorWindow.xaml(.cs)`.
- Tests: `tests/RackCad.Tests/LateralHeaderLayoutBuilderTests.cs` (builder) y
  `tests/RackCad.Tests/LateralHeaderParametersFactoryTests.cs` (mapeo config→parámetros).
- Parámetros en el editor: `RackFrameConfiguration` + `RackFrameConfiguratorViewModel` + el panel
  "Header" del editor avanzado en `RackFrameConfiguratorWindow.xaml`.
