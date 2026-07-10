# Estado actual del MVP

## Que es hoy

Plugin de AutoCAD (.NET `net8.0-windows`, WPF) para **disenar y dibujar racks**. Ya no es
"solo un configurador de cabeceras": maneja **cuatro tipos de rack**, cada uno con su ventana
editora, su dibujo en AutoCAD y **round-trip de edicion en sitio**. La rama `release/claude-review`
esta con 249 tests verdes.

**Todas las ventanas editoras** comparten hoy: (a) un campo de **nombre** ("Rack A", como lo ve el
cliente), (b) el patron de botones **Actualizar / Insertar** (ver "Identidad y round-trip") y
(c) generacion desde el menu `RACKCAD` o desde su comando directo. RackCad puede **cargarse solo al
abrir AutoCAD** con el bundle del Autoloader (ver [despliegue.md](despliegue.md#4-carga-automatica-autoloader-bundle--recomendado));
si no, se hace `NETLOAD` por sesion.

Menu principal: comando `RACKCAD` (`RackMainMenuWindow`), desde donde se elige que disenar e
insertar. Cada tipo tiene ademas su comando directo.

## Los cuatro tipos de rack

### 1. Cabecera (marco) — `RackFrameConfiguratorWindow`

Un marco = 2 postes + placas base + celosia. Las **horizontales son la fuente de verdad**; los
paneles se derivan entre horizontales consecutivas. El **peralte de la placa base es editable por
placa** (`BasePlatePlacement.PeralteOverride`; `null` = derivado del poste con `StandardPeralte` =
base + slope*peralte_poste). En modo "Configuracion rapida" ademas del alto/fondo se puede fijar el
**peralte del poste** (`PostPeralte`, afecta la planta y la frontal del selectivo cuando esta cabecera
se referencia) **sin abrir el editor avanzado**, y hay un **campo de nombre**; "Insertar" genera de un
clic.

- Comandos: `RACKCABECERA` (configurador), `RACKCABECERALATERAL` (dibujo directo de la estandar),
  `QUICKCABECERA` (desde la linea de comandos: poste, fondo, alto).

### 2. Sistema dinamico (pallet flow) — `RackDynamicSystemWindow`

Cabeceras a lo largo del tramo + separadores por nivel. La altura de la cabecera se deriva de los
niveles, pero tambien se puede **editar a mano** (`ManualHeaderHeightOverride`). El recomponer es
**no destructivo**: cambiar niveles/altura o alternar "reforzar poste derivado" preserva los fondos
de las cabeceras personalizadas (`UpdateHeaderHeightInPlace`); solo cambiar la especificacion de
tarima (o `PalletsDeep`) fuerza un rebuild completo. Las personalizaciones avanzadas sobreviven a
guardar y reabrir.

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
(`RackFrameConfiguration` embebida), de donde sale su placa/peralte. El **peralte del poste es por
poste** (no uno solo para todo el frente): `SelectivePostGeometry.Compute` devuelve un `TroquelXs`
por poste y las bahias entre postes de distinto peralte se espacian bien.

- **Toggles de dibujo** (seccion propia en el editor): "Numerar frentes", "Numerar niveles",
  "Colocar nombre de rack" y "Dibujar placa base" (real). Las tres anotaciones de texto se dibujan en
  una **capa dedicada `RACKCAD_ANOTACIONES`** (amarilla) via `SelectiveAnnotations` +
  `HeaderBlockRole.Annotation`, presentes en frontal, planta y lateral, y se regeneran en cada
  redefinicion (no se persisten). "Dibujar tarima" queda diferido (ver ideas-futuras.md).
- **BOM** (postes, placas, largueros, mensulas) con `SelectiveBomBuilder` y `RackBomWindow`
  (grid + exportacion a CSV).
- Comando: `RACKSELECTIVO`.

## Identidad y round-trip (los cuatro tipos)

- Cada rack dibujado = **una definicion de bloque**; las copias son referencias a ella.
- En la definicion del bloque se embebe (diccionario de extension, Xrecord troceado ≤255) un sobre
  unificado `RackEmbedDocument { SchemaVersion, Kind, View, Section, Id (GUID), Name, Design (JSON
  del diseno) }`. Kinds: `"selective"`, `"dynamic"`, `"cabecera"`, `"cama"`. Views: `"frontal"`,
  `"lateral"`, `"planta"`. `Section` = indice del corte lateral del selectivo (`-1` = vista no
  seccionada).
- Comando `RACKEDITAR`: seleccionas un rack (cualquier vista) -> lee el sobre -> **despacha por
  Kind** -> reabre el editor del sistema completo precargado (`LoadExisting`) -> al confirmar
  **redefine la definicion en sitio** (`RedrawInPlace` + Regen) y **redibuja TODAS las vistas del
  mismo sistema** (encontradas por GUID escaneando las definiciones de bloque) => todas las copias
  se actualizan a la vez, ninguna se mueve.
- El nombre "Rack A" (campo en cada editor) es el nombre del bloque; el GUID va en el sobre (evita
  colisiones).
- **Convencion de botones (permanente en las cuatro ventanas):**
  - **Actualizar** = redibuja en sitio las vistas existentes del sistema (mismo GUID); es solo edicion.
  - **Insertar {vista}** = agrega una vista NUEVA **enlazada** (mismo GUID) y ademas **refresca** las
    vistas ya presentes. En racks multi-vista (selectivo, cabecera) aparece "Actualizar" + un
    "Insertar" por vista; en los de una sola vista (dinamico, cama) el unico boton alterna su etiqueta
    Insertar <-> Actualizar segun si esa vista ya existe.
  - **`RACKDUPLICAR`** = copia **independiente** (GUID nuevo, nombre "- copia"); editar la copia no
    toca al original. Distinto del `COPY` de AutoCAD, que comparte la definicion y por ende el GUID
    (esas copias se editan juntas, que es lo correcto para "replicas").
- Escalable: agregar un tipo nuevo = su `Kind` + `Edit<Kind>` en `RackFrameCommands` + `LoadExisting`
  en su ventana + embed/RedrawInPlace en su draw service. Stores del diseno:
  `SelectivePalletDesignStore` (selectivo), `RackProjectStore` (dinamico/cabecera),
  `FlowBedConfigurationStore` (cama).

## Catalogos

`assets/catalogs/*.csv`, cargados por `JsonRackCatalogProvider` a `RackCatalog`. "Excel-first": el
`.csv` gana sobre el `.json`; acepta UTF-8 y ANSI/Windows-1252 de Excel; cache con invalidacion por
firma de archivos (editar el CSV y relanzar el comando recarga):

- `post-profiles` (postes; los refuerzos son postes).
- `truss-profiles` (una sola lista de celosia = horizontales + diagonales).
- `beam-profiles` (largueros; columna `peraltes` = valores permitidos, FK a mensula).
- `mensulas`, `base-plates` (con `peralteBase`/`peraltePorPeraltePoste` -> `StandardPeralte`).
- `connection-points` + `connection-layout` (puntos de conexion parametricos en X **y en Y**:
  X = localX + localXPorParam*valor(paramX), Y = localY + localYPorParam*valor(paramY); columnas
  `pieceId,connectionPointId,view,localX,localXPorParam,paramX,localY,localYPorParam,paramY`).
- `blocks` (bloque por pieza y vista; `FindBlock(pieceId, view)` es la ruta activa de los cuatro
  tipos y `blockName` debe coincidir exacto con el nombre del bloque en la libreria DWG), `views`,
  `flow-bed-profiles`, `spacers-profiles`.

Persistencia de proyecto: `RackProjectStore` -> `.rackcad.json`.

## Vistas y BOM

- **Selectivo**: tres vistas ligadas por el mismo GUID. **Frontal** (un bloque: postes + placas +
  largueros por nivel). **Lateral** (cortes: un bloque por poste — cada corte es la cabecera del
  poste en perfil + las secciones de largueros frente/atras por nivel; al insertar se pregunta que
  corte por numero de poste y se coloca con jig). **Planta** (un bloque: una cabecera-planta por
  frente apilada en Y + largueros frente/atras por bahia a lo largo de Y; X = fondo, Y = frente).
- **Cabecera**: dos vistas ligadas por GUID, **lateral** y **planta** (planta = 2 huellas de poste,
  frente en 0 / atras en fondo, + placas + celosia colapsada a un miembro con longitud = A-corte del
  travesano = fondo − 2×(inset troquel − mensula); peralte de celosia = peralte del poste − 1").
- Vista **lateral** para dinamico / cama.
- Las vistas lateral/planta **solo se insertan desde `RACKEDITAR`** de una vista frontal/lateral
  existente (los botones se deshabilitan con tooltip si no aplica), para que nunca queden huerfanas.
- Dibujo block-based en AutoCAD para los cuatro tipos, con jig de colocacion.
- BOM con exportacion a CSV (selectivo; CRLF RFC-4180).

## Que falta

- Definicion de los bloques dinamicos en el DWG (deben existir previamente; los faltantes se
  reportan y se omiten al dibujar).
- Persistencia en base de datos (SQLite) y exportacion a Excel (hoy el BOM exporta CSV).
</content>
</invoke>
