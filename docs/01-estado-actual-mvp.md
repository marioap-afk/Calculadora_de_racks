# Estado actual del MVP

## Que es hoy

Plugin de AutoCAD (.NET `net8.0-windows`, WPF) para **disenar y dibujar racks**. Ya no es
"solo un configurador de cabeceras": maneja **cuatro tipos de rack**, cada uno con su ventana
editora, su dibujo en AutoCAD y **round-trip de edicion en sitio**. La rama `release/claude-review`
esta con ~200 tests verdes.

Menu principal: comando `RACKCAD` (`RackMainMenuWindow`), desde donde se elige que disenar e
insertar. Cada tipo tiene ademas su comando directo.

## Los cuatro tipos de rack

### 1. Cabecera (marco) — `RackFrameConfiguratorWindow`

Un marco = 2 postes + placas base + celosia. Las **horizontales son la fuente de verdad**; los
paneles se derivan entre horizontales consecutivas. El **peralte de la placa base es editable por
placa** (`BasePlatePlacement.PeralteOverride`; `null` = derivado del poste con `StandardPeralte` =
base + slope*peralte_poste). En modo "Configuracion rapida", "Insertar" genera de un clic.

- Comandos: `RACKCABECERA` (configurador), `RACKCABECERALATERAL` (dibujo directo de la estandar),
  `QUICKCABECERA` (desde la linea de comandos: poste, fondo, alto).

### 2. Sistema dinamico (pallet flow) — `RackDynamicSystemWindow`

Cabeceras a lo largo del tramo + separadores por nivel.

- Comando: `RACKSISTEMADINAMICO` + opcion del menu.

### 3. Cama de rodamiento (flow bed) — `RackFlowBedWindow`

Riel + rodillos + frenos + tope; variantes dinamica o pushback (pushback omite frenos).

- Comando: `QUICKCAMA` + opcion del menu.

### 4. Selectivo (editor avanzado pallet-driven) — `RackSelectiveWindow`

Matriz **FRENTES × niveles** (el termino es "frente", no "bahia"). Cada celda = tarima (frente/alto)
+ tarimas por nivel + larguero + peralte de larguero (combo desde la lista `peraltes` del catalogo).
La geometria la resuelve `SelectiveGeometryResolver`: largueros por troquel, claro/separacion por
tarima+holgura, altura por frente medida desde el escalon (1/3 de la tarima superior), datum de piso
en Y=0, "larguero a piso" por frente (elevacion editable), etc. Overrides manuales opcionales
(vacio = auto): longitud de larguero y claro por celda, altura por frente, elevacion de larguero a
piso. Cada poste (N frentes -> N+1 postes) puede referenciar una **cabecera por poste**
(`RackFrameConfiguration` embebida), de donde sale su placa/peralte.

- **BOM** (postes, placas, largueros, mensulas) con `SelectiveBomBuilder` y `RackBomWindow`
  (grid + exportacion a CSV).
- Comando: `RACKSELECTIVO`.

## Identidad y round-trip (los cuatro tipos)

- Cada rack dibujado = **una definicion de bloque**; las copias son referencias a ella.
- En la definicion del bloque se embebe (diccionario de extension, Xrecord troceado ≤255) un sobre
  unificado `RackEmbedDocument { Kind, Id (GUID), Name, Design (JSON del diseno) }`. Kinds:
  `"selective"`, `"dynamic"`, `"cabecera"`, `"cama"`.
- Comando `RACKEDITAR`: seleccionas un rack -> lee el sobre -> **despacha por Kind** -> reabre el
  editor correcto precargado (`LoadExisting`) -> al confirmar **redefine la definicion en sitio**
  (`RedrawInPlace` + Regen) => todas las copias se actualizan a la vez, ninguna se mueve.
- El nombre "Rack A" (campo en cada editor) es el nombre del bloque; el GUID va en el sobre (evita
  colisiones).
- Escalable: agregar un tipo nuevo = su `Kind` + `Edit<Kind>` en `RackFrameCommands` + `LoadExisting`
  en su ventana + embed/RedrawInPlace en su draw service. Stores del diseno:
  `SelectivePalletDesignStore` (selectivo), `RackProjectStore` (dinamico/cabecera),
  `FlowBedConfigurationStore` (cama).

## Catalogos

`assets/catalogs/*.csv`, cargados por `JsonRackCatalogProvider` a `RackCatalog`:

- `post-profiles` (postes; los refuerzos son postes).
- `truss-profiles` (una sola lista de celosia = horizontales + diagonales).
- `beam-profiles` (largueros; columna `peraltes` = valores permitidos, FK a mensula).
- `mensulas`, `base-plates` (con `peralteBase`/`peraltePorPeraltePoste` -> `StandardPeralte`).
- `connection-points` + `connection-layout` (puntos de conexion parametricos: X = localX + slope*param).
- `blocks` (bloque por pieza y vista), `views`, `flow-bed-profiles`, `spacers-profiles`.

Persistencia de proyecto: `RackProjectStore` -> `.rackcad.json`.

## Vistas y BOM

- Vista **frontal** para el selectivo; vista **lateral** para cabecera / dinamico / cama.
- Dibujo block-based en AutoCAD para los cuatro tipos, con jig de colocacion.
- BOM con exportacion a CSV (selectivo).

## Que falta

- **Fase 5: vista lateral del selectivo** — cada poste desplegado como su cabecera completa,
  enlazado por el mismo GUID. Pendiente.
- Definicion de los bloques dinamicos en el DWG (deben existir previamente; los faltantes se
  reportan y se omiten al dibujar).
- Persistencia en base de datos (SQLite) y exportacion a Excel (hoy el BOM exporta CSV).
</content>
</invoke>
