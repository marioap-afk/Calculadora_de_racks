# Generación de cabecera lateral (block-based)

Esta es la lógica para generar una **cabecera en vista lateral** a partir de **bloques
independientes anclados a puntos de conexión** (no una composición visual libre). El **poste es la base
geométrica**; horizontales y diagonales de celosía cuelgan de la línea de troqueles del poste. La cabecera
es **uno de los cuatro tipos de rack** que dibuja el plugin (cabecera, sistema dinámico, cama de rodamiento
y selectivo); el **mismo patrón por bloques + round-trip** descrito aquí sirve a los cuatro (ver
[Patrón unificado](#patrón-unificado-block-based--round-trip-para-los-4-tipos)).

> **Estado:** la **lógica pura** y el **dibujo en AutoCAD** de la cabecera lateral ya están implementados y
> compilan en Windows (232 tests verdes). Los comandos `RACKCABECERA` / `RACKCABECERALATERAL` /
> `QUICKCABECERA` y el botón **Insertar en AutoCAD** del configurador dibujan la cabecera como **un bloque**;
> `RACKEDITAR` la reabre y la **redefine en sitio** para que todas las copias se actualicen a la vez. La
> cabecera tiene además una vista **PLANTA** ligada por GUID a la lateral (solo se inserta desde `RACKEDITAR`
> de una vista existente, para que nunca quede huérfana); al confirmar una edición se **redibujan todas las
> vistas** del sistema. Los bloques se resuelven desde la biblioteca vía `blocks.csv`
> (`FindBlock(pieceId, view)` es la ruta activa). Ver la
> sección [Paso 2](#paso-2-estado-del-cableado-en-autocad).

## Arquitectura (separación pura ↔ AutoCAD)

```
RackCad.Application/Headers/            (PURO, testeable en cualquier SO)
  LateralHeaderParameters     parámetros editables (sin números mágicos)
  HeaderConnectionGeometry    coords locales de los puntos + nombres de bloque
  HeaderBlockInstance         una inserción del plan (qué, dónde, params dinámicos)
  LateralHeaderLayout         el plan completo + totales
  LateralHeaderLayoutBuilder  la lógica (7 pasos)
  HeaderGeometryResolver      catálogo (connection-layout + blocks) → geometría

RackCad.Plugin/Headers/                (AutoCAD, solo Windows)
  LateralHeaderDrawer         adapter: convierte el plan en un bloque (InsertBlock + params dinámicos)
  LateralHeaderDrawService    orquesta: carga catálogo, arma el plan, crea el bloque y lo coloca (jig)
  BlockLibraryImporter        importa del DWG biblioteca los bloques que falten en el dibujo activo
```

Regla de oro: **toda la geometría y los cálculos son puros**; el drawer solo traduce el plan a la API de
AutoCAD. Así la lógica se prueba sin AutoCAD y el drawer queda mínimo.

## Parámetros (`LateralHeaderParameters`)

| Parámetro | Default | Significado | ¿Dónde se edita? |
|---|---|---|---|
| `Height` | 132 | Altura; mueve el parámetro dinámico `LONGITUD` del poste | Editor **clásico** |
| `Depth` | 42 | Fondo: separación entre los dos postes | Editor **clásico** |
| `PasoTroquel` | 2 | Paso entre troqueles del poste | (fijo) |
| `InicioCelosiaTroquel` | 3 | Troquel (1-based) donde va la primera horizontal → `(3-1)*2 = 4"` | Editor **avanzado** |
| `ClaroPanel` | 44 | Claro vertical entre horizontales | Editor **avanzado** (segmentos) |
| `OffsetDiagonalInicioTroqueles` | 2 | La diagonal arranca N troqueles arriba de la horizontal inferior | Editor **avanzado** |
| `OffsetDiagonalFinTroqueles` | 2 | La diagonal termina N troqueles abajo de la horizontal superior | Editor **avanzado** |
| `ValorClaroTravesano` | auto | Claro sobrante arriba; si `>0` se agrega una horizontal de cierre | auto / opcional |

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
4. **Y de horizontales**: vienen **explícitas** de la `RackFrameConfiguration`; la factory las calcula
   paramétricamente: primer travesaño en el troquel de inicio
   (`yFirst = Y_troquel0 + (InicioCelosiaTroquel−1)·PasoTroquel`), paneles de `ClaroPanel` (44") y cierres
   de 0/1/2. Las elevaciones de las plantillas JSON ya **no** se usan (solo aportan perfiles/placa/poste/
   puntos) y **no** escalan proporcionalmente.
5. **Horizontales**: el punto `CELOSIA` cae sobre la línea de troquel a su Y; largo = `LongitudHorizontal`.
6. **Diagonales** (una por panel): arranca `OffsetInicio` troqueles arriba de la horizontal inferior y
   termina `OffsetFin` troqueles abajo de la superior; **largo y ángulo se calculan de los puntos reales**.
7. **Horizontal de cierre**: la horizontal superior cae **exacto** en `Y = Height − PostTopRemate`
   (remate = 4"), de modo que el **alto construido = el alto pedido** (240 → 240).

Ejemplo (Height 132, Depth 42, paso 2, inicio 3, claro 44, troquel local X=2):
horizontales en Y = 4 / 48 / 92 (largo 38), cierre en Y = 128 (= 132 − remate de 4"), 2 diagonales.

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
   `Height`, `Depth`, `InicioCelosiaTroquel = CelosiaStartTroquel`,
   `OffsetDiagonalInicioTroqueles = DiagonalStartOffsetTroqueles`,
   `OffsetDiagonalFinTroqueles = DiagonalEndOffsetTroqueles`, `ClaroPanel` (derivado del primer claro entre
   horizontales) y los ids reales (`PostId`/`BasePlateId`/`TrussProfileId`).
2. ✅ **Servicio de dibujo**: `RackCad.Plugin/Headers/LateralHeaderDrawService.cs` carga el catálogo
   (`JsonRackCatalogProvider.FromBaseDirectory().Load()`), bloquea el documento, arma **un bloque** con todas
   las piezas y lo coloca con el mouse (jig). Implementa la interfaz `RackCad.UI.IHeaderDrawService` para que la
   WPF dispare el dibujo **sin** referenciar las DLLs de AutoCAD (regla de oro: la UI no conoce AutoCAD).
3. ✅ **Importación de bloques**: `BlockLibraryImporter.EnsureForLayout` clona en el dibujo activo los bloques
   que falten desde un DWG biblioteca (`blocks-library.dwg`, junto a los catálogos; ruta configurable). Se lee
   en una base lateral sin abrirlo en AutoCAD; los bloques que tampoco existan en la biblioteca se omiten.
4. ✅ **Comandos y botón**: `RACKCABECERALATERAL` dibuja la cabecera estándar (smoke test); `QUICKCABECERA`
   la pide por línea de comandos (poste/fondo/alto); `RACKCABECERA` abre el configurador. El **botón
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
  | `cabecera` | `RACKCABECERA` / `RACKCABECERALATERAL` / `QUICKCABECERA` | `RackFrameConfiguratorWindow` | `LateralHeaderDrawService` | `RackProjectStore` |
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
2. Elevaciones **explícitas** de la configuración (paneles de `ClaroPanel` + cierres de 0/1/2); la horizontal
   superior cae en `Height − PostTopRemate`, así el alto construido = el pedido.
3. Una diagonal por panel (zigzag estándar); si se requiere alternancia/X, se parametriza.

## Dónde está en el código

- Lógica pura: `src/RackCad.Application/Headers/` (builder, resolver, parámetros y
  `LateralHeaderParametersFactory` config→parámetros).
- Adapter AutoCAD: `src/RackCad.Plugin/Headers/LateralHeaderDrawer.cs` (+ `LateralHeaderDrawOutcome.cs`,
  `LateralHeaderBlockResult.cs`, `BlockLibraryImporter.cs`).
- Servicio de dibujo (puente UI↔AutoCAD): `src/RackCad.Plugin/Headers/LateralHeaderDrawService.cs`,
  que implementa `src/RackCad.UI/IHeaderDrawService.cs`.
- Round-trip: sobre `src/RackCad.Application/Persistence/RackEmbedDocument.cs` (`RackEmbedStore`), embebido con
  `src/RackCad.Plugin/Systems/RackBlockData.cs`; comandos y despacho en `src/RackCad.Plugin/RackFrameCommands.cs`
  (`RACKEDITAR`, `EditCabecera`/`EditDynamic`/`EditCama`/`EditSelective`).
- Comandos de cabecera: `RACKCABECERA`, `RACKCABECERALATERAL`, `QUICKCABECERA` en
  `src/RackCad.Plugin/RackFrameCommands.cs`; botón "Insertar en AutoCAD" en
  `src/RackCad.UI/RackFrameConfiguratorWindow.xaml(.cs)`.
- Tests: `tests/RackCad.Tests/LateralHeaderLayoutBuilderTests.cs` (builder/resolver) y
  `tests/RackCad.Tests/LateralHeaderParametersFactoryTests.cs` (mapeo config→parámetros).
- Parámetros en el editor: `RackFrameConfiguration` + `RackFrameConfiguratorViewModel` + el panel
  "Header" del editor avanzado en `RackFrameConfiguratorWindow.xaml`.
