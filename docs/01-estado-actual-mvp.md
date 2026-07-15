# Estado actual del MVP

## Que es hoy

Plugin de AutoCAD (.NET `net8.0-windows`, WPF) para **disenar y dibujar racks**. Ya no es
"solo un configurador de cabeceras": maneja **cuatro tipos de rack**, cada uno con su ventana
editora, su dibujo en AutoCAD y **round-trip de edicion en sitio**. La rama `release/claude-review`
tiene **503/503 tests verdes** y build Debug completo con 0 errores (estado vivo en `docs/HANDOFF.md`).

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

- Comandos: `RACKCABECERA` (configurador), `QUICKCABECERA` (desde la linea de comandos: poste, fondo, alto).

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
  redefinicion (no se persisten). "Dibujar tarima" coloca la referencia visual en frontal y lateral; planta sigue
  pendiente. La tarima no entra al BOM.
- **Cotas automaticas por vista** (2026-07-11): combobox **Cotas** (Ninguna/Minimo/Estandar/Detallado,
  persistido) + combobox **Estilo de cota** (de la `DimStyleTable` del dibujo abierto; "(Automatico)" usa el
  estilo vigente escalado por la escala de anotacion). `SelectiveDimensions` (puro, `HeaderBlockRole.Dimension`)
  emite las cotas por vista y `LateralHeaderDrawer.AppendDimension` las materializa como `RotatedDimension` en
  la capa **`RACKCAD_COTAS`** (roja). Frontal: alto/ancho totales, largo de CORTE del larguero por frente
  (desde el inicio del perfil), separaciones entre niveles, elevaciones (Detallado). Lateral (por corte): alto,
  fondo por cabecera, separaciones. Planta: largo total, ancho por frente, fondo total, fondo por fondo. Se
  regeneran en cada redibujo (el `*D` anonimo de cada cota se purga).
- **BOM por componentes**: cabeceras + largueros como **componentes expandibles a piezas**
  (`BomComponent`; `SelectiveBomBuilder` + `RackBomWindow` con arbol; CSV a dos niveles).
  `RACKBOMTOTAL` genera el **BOM consolidado de TODO el dibujo** (desglose por rack x copias +
  gran total por componente, `RackConsolidatedBomWindow`, export CSV). Ademas hay un **editor de
  larguero** (`RackLargueroWindow`, menu "Disenar larguero"; solo visual/BOM, sin bloque de
  AutoCAD aun).
- **Doble profundidad (espalda con espalda), Fase 1:** `DepthCount` (1..4 fondos; 1 = sencillo
  clasico) a lo largo del fondo, con separadores **por hueco** (`SeparatorLengths`; el bloque
  separador ya se dibuja en las vistas lateral y planta y entra al BOM como su propio componente
  "Separador"; en la frontal solo se deja el hueco, a proposito). **Cada fondo tiene sus propios niveles/alturas**
  (`ExtraFondoBays`; vacio = hereda las `Bays` del fondo 0) **y su propio numero de frentes**
  (`BayCountBox` habilitado en cualquier fondo; layout en esquina): el **fondo mas largo define la
  rejilla horizontal compartida** y los mas cortos son un **prefijo** de ella, asi los postes que se
  traslapan alinean; un frente sin niveles = **columna vacia** (si el frente maestro es una columna
  vacia, su ancho lo da la bahia real mas ancha de los otros fondos en ese indice). La
  **frontal se puede insertar por fondo** (cada cara con su elevacion; el fondo va en `Section` del
  sobre); **lateral y planta dibujan todos los fondos** (`FondoBays`), y el **BOM suma el contenido
  real de cada fondo x2** (frente/atras). Ademas **cada fondo tiene su propio fondo de tarima**
  (`ExtraFondoDepths`/`FondoDepths`) y el marco dibujado usa **fondo de cabecera = fondo de tarima −
  6"** (`SelectiveRackDefaults.CabeceraFondoAllowance`, via `SelectiveDepthLayout.CabeceraDepthOfFondo`),
  con override opcional por linea "Fondo de cabecera" (`CabeceraFondoOverrides`/`FondoCabeceraOverrides`).
  Defaults nuevos: frente de tarima 42, separacion entre fondos 12. En el editor: selector "Editando
  fondo" + separadores por hueco + un toggle **Frontal/Lateral** sobre la vista previa (la lateral es
  esquematica: cada fondo como su cabecera con celosia en zigzag). El **"medio frente" ya esta hecho,
  generalizado a N tramos**: un frente se parte en N tramos con N-1 postes intermedios y el ultimo
  tramo calculado (`SelectiveMedioFrente.Resolve`; si los tramos no caben se dibuja el frente
  completo), boton "Medio frente..." por frente (`SelectiveSegmentsWindow`) y round-trip via
  `SelectiveSegmentDocument` (+ fallback del `MedioFrenteLength` legacy).
- **Rendimiento (validado con ~30 frentes):** la matriz del editor NO se reconstruye por clic (cache de
  celdas + restyle de 2 bordes; `Recompute` coalescido por gesto; brushes congelados; lookups de catalogo
  memoizados — equivalencia fijada por `SelectiveTwentyBaysEquivalenceTests`). La vista **planta** usa el
  patron ARRAY: los marcos identicos se agrupan en UNA definicion anidada (`SelectivePlantaBuilder.BuildPlan`
  -> `HeaderGroup` + `HeaderPlacement.InsertionY`) referenciada N veces, en vez de insertar cada pieza con
  sus parametros dinamicos por referencia. El purge de definiciones huerfanas solo verifica las defs que el
  contenido nuevo ya no referencia (sin coste fijo por redibujo).
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
- Comando `RACKLISTA`: tabla de todos los racks del dibujo (nombre, tipo, vistas presentes, numero
  de copias). `RackListBuilder` (puro, testeado) agrupa los sobres por GUID; "Ir al rack" hace zoom
  a la primera referencia en el modelo (vista frontal si existe).
- Escalable: agregar un tipo nuevo = su `Kind` + `Edit<Kind>` en `RackFrameCommands` + `LoadExisting`
  en su ventana + embed/RedrawInPlace en su draw service. Stores del diseno:
  `SelectivePalletDesignStore` (selectivo), `RackProjectStore` (dinamico/cabecera),
  `FlowBedConfigurationStore` (cama).

## Catalogos

`assets/catalogs/*.csv`, cargados por `JsonRackCatalogProvider` a `RackCatalog`. "Excel-first": el
`.csv` gana sobre el `.json`; acepta UTF-8 y ANSI/Windows-1252 de Excel; cache con invalidacion por
firma de archivos (editar el CSV y relanzar el comando recarga):

- `secciones` (TODOS los perfiles estructurales en una hoja con columna `rol` = POSTE | CELOSIA | LARGUERO;
  el provider los separa en `PostProfiles`/`TrussProfiles`/`BeamProfiles`, API intacta; los tres CSV legacy
  siguen leyendose como fallback). Los refuerzos son postes; los largueros llevan `peraltes` + `mensula` (FK).
- `mensulas`, `base-plates` (con `peralteBase`/`peraltePorPeraltePoste` -> `StandardPeralte`).
- `connection-points` + `connection-layout` (puntos de conexion parametricos en X **y en Y**:
  X = localX + localXPorParam*valor(paramX), Y = localY + localYPorParam*valor(paramY); columnas
  `pieceId,connectionPointId,view,localX,localXPorParam,paramX,localY,localYPorParam,paramY`).
- `blocks` (bloque por pieza y vista; `FindBlock(pieceId, view)` es la ruta activa de los cuatro
  tipos y `blockName` debe coincidir exacto con el nombre del bloque en la libreria DWG), `views`,
  `flow-bed-profiles`.

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
- BOM con exportacion a CSV y a Excel (.xlsx) (selectivo; CRLF RFC-4180).

## Que falta / queda fuera del alcance actual

- Definicion de los bloques dinamicos en el DWG (deben existir previamente; los faltantes se
  reportan y se omiten al dibujar).
- No hay base de datos y no se necesita para el alcance local actual: disenos/configuracion viven en JSON y DWG. Una
  migracion futura a SQLite solo tendria sentido si el volumen, consultas o gobierno de catalogos lo justifican.
