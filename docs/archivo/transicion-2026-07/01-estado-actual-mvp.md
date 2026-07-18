# Estado actual del MVP

> **Archivo de transición; no es una fuente vigente.** El estado vivo está en
> [HANDOFF](../../HANDOFF.md) y la arquitectura actual en [ARCHITECTURE](../../ARCHITECTURE.md).

## Que es hoy

Plugin de AutoCAD (.NET `net8.0-windows`, WPF) para **disenar y dibujar racks**. Ya no es
"solo un configurador de cabeceras": maneja **cuatro tipos de rack**, cada uno con su ventana
editora, su dibujo en AutoCAD y **round-trip de edicion en sitio**. El trunk `main`
tiene la **suite completa de tests verde** y build Debug con 0 errores (conteo y última corrida real: `docs/HANDOFF.md` sección 12; no se copian números aquí para evitar que diverjan).

**Todas las ventanas editoras** comparten hoy: (a) un campo de **nombre** ("Rack A", como lo ve el
cliente), (b) el patron de botones **Actualizar / Insertar** (ver "Identidad y round-trip") y
(c) generacion desde el menu `RACKCAD` o desde su comando directo. RackCad puede **cargarse solo al
abrir AutoCAD** con el bundle del Autoloader (ver
[despliegue](../../guias/despliegue.md#4-carga-automatica-autoloader-bundle--recomendado));
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

El campo global **Peralte de poste** se persiste en `DynamicRackDesign.PostPeralte` y se aplica a todas las cabeceras,
incluidas las personalizadas. Un documento anterior sin el campo hereda el ancho del perfil de poste del catalogo.

Cuando dos separadores consecutivos derivan un poste intermedio reforzado, el limite comun coincide con
`FIN_POSTE`: el poste principal se desplaza a la izquierda ese offset y el refuerzo comienza en el limite. Asi el
conjunto queda centrado; si se desactiva el refuerzo, el poste sencillo conserva su origen directamente en el limite.

La base interna ya sigue el patron del selectivo: `DynamicRackDesign` conserva las entradas editables y
`DynamicRackSystemResolver` produce un `DynamicRackSystem` resuelto. Coordenadas y miembros fisicos se
regeneran; los campos nuevos tienen fallback legacy y las cabeceras personalizadas no se reemplazan al recalcular.
Cada nivel resuelto incluye un larguero completo `LARGUERO_IN_OUT_C6` en entrada y otro en salida. Ambos hacen
mate por el origen del bloque: salida baja/no espejeada en `X=0` y entrada alta/espejeada en `X=TotalLength`.
El peralte C6 se resuelve desde `secciones.csv`; no se ensambla ni cuenta una mensula separada.
La cama completa del nivel reutiliza `FlowBedLateralBuilder`: su `TROQUEL_IN` se atornilla al `TROQUEL_CAMA`
del larguero izquierdo y el conjunto gira con la pendiente resuelta entre largueros. La longitud comercial no se
deduce de esa diagonal: es siempre el tramo longitudinal propio del frente menos `4"`. Una definicion anidada se
comparte entre niveles. El BOM del sistema cuenta una unidad `Cama` por posicion
transversal y por nivel configurado en cada frente, sin despiece interno, y muestra en ella la longitud y el BFR.
Tambien repite las cabeceras por cada linea transversal de postes, cuenta ambos perfiles/placas de un apoyo reforzado,
los separadores, los IN/OUT por frente/nivel y los largueros intermedios por longitud/peralte. Botas, protectores laterales y desviadores
reutilizan las familias catalogadas y el editor de seguridad del selectivo. Se proyectan tambien en frontal y planta
con la misma seleccion; el protector lateral sustituye las botas de su vista y los desviadores respetan los niveles
reales de los frentes adyacentes. El BOM toma BOTA/LATERAL de planta y DESVIADOR de salida + entrada como inventario
fisico; un lateral sustituye ambas botas de su linea de poste y ninguna proyeccion multiplica una
pieza por aparecer tambien en lateral o planta. Esas piezas sobreviven
guardar/abrir/RACKEDITAR.

La familia dinamica `DEFENSA` agrega `DEFENSA_MONTACARGAS`: un grid por poste permite activar o apagar de forma
independiente la salida y la entrada y asignar una `LONGITUD` distinta a cada extremo. Los defaults son 12" por lado
en postes de orilla y 36" por lado en intermedios. Lateral/planta usan el offset `ORIGEN_POSTE` catalogado (-4.75" en
X); frontal coincide con el origen del poste. El BOM cuenta las piezas fisicas desde planta, agrupadas por longitud.

La familia `GUIA` agrega `GUIA_ENTRADA` solo en el extremo de entrada. Su grid frente x nivel nace activo en todas
las celdas, coloca una pareja espejeada 8" sobre el IN/OUT y fija `LONGITUD` al ultimo tramo longitudinal ocupado por
ese frente. Lateral, frontal de entrada y planta consumen el mismo plan; el BOM toma la frontal de entrada para contar
las dos piezas fisicas sin duplicarlas por las otras proyecciones.

El editor recibe la cantidad total de frentes como un entero; al aumentarla, conserva los existentes y copia el frente
seleccionado para crear los nuevos. Usa una matriz frente x nivel y una celda seleccionada real. La ficha separa dos
contratos: posiciones, niveles, fondos, inicio longitudinal e inicio del primer larguero pertenecen solo al frente
seleccionado; claro libre, frente/alto/peso de tarima, largo manual y tipo/peralte de IN/OUT e intermedio pertenecen
a la celda. Celda/Nivel/Frente/Todas copia exclusivamente estos ultimos valores; `Frente` nunca modifica otra columna.
Ctrl + clic mantiene una seleccion multiple: `Seleccionadas` aplica los datos de celda solo a esas coordenadas. Los
datos estructurales ofrecen `Este frente`, `Frentes seleccionados` (frentes con alguna celda marcada) y
`Todos los frentes`, por lo que fondos, posiciones, niveles o primer larguero se pueden uniformar explicitamente.
El largo fisico entre postes es el mayor solicitado por los niveles de ese frente. El frente con
menos fondos gobierna el intervalo base: sus extremos reciben los dos `+6"` y su patron cabecera/separador se prolonga
hacia los fondos extra. Todos los frentes contienen ese intervalo compartido. Un extremo que cae en separador conserva
ese tipo y recibe un poste limite independiente, sin convertirse en cabecera. Cada frente resuelve sus propios X de
salida/entrada, pendiente, cama y soportes. La lateral se genera como un corte por poste: cada corte usa la union de
profundidad y el maximo de altura, niveles y peraltes intermedios de los uno o dos frentes que toca. En los cortes
frontales cada poste aplica la misma regla de altura adyacente.
Para cada posicion `BFR = frenteTarima + 2"` y el largo IN/OUT automatico es
`BFR * posiciones + 6"` (40" de frente -> BFR 42" -> larguero 48" para una posicion), con override manual opcional.
`DynamicFrontGeometry` almacena el BFR resuelto y alimenta la frontal y la planta. La UI incluye selector de poste en la preliminar lateral,
frontal salida y frontal entrada, ademas de numeracion de frentes/niveles, nombre del rack y cotas configurables.

Hay dos cortes frontales: salida usa las elevaciones bajas y entrada las elevaciones altas; ambos dibujan solo
postes/placas y `LARGUERO_IN_OUT_C6`. Un poste compartido toma la altura comercial del frente adyacente mas alto, sin
propagar el maximo a postes ajenos. La planta repite la estructura lateral en cada limite de frente, dibuja IN/OUT e
intermedios colapsados por nivel y **no coloca camas**. En planta, el poste derivado principal termina en el limite
definido por `FIN_POSTE` y el refuerzo comienza ahi: ambos estan en la misma linea transversal, consecutivos sobre X
y conservan la misma orientacion. Cada limite
interno entre modulos representa una posicion de poste y recibe un solo
`LARGUERO_ESCALON_INFINITO` por nivel; los limites `X=0`/`X=TotalLength` quedan reservados para IN/OUT. El segundo
poste de una cabecera usa el bloque espejeado con `INICIO_DERECHO`; el primero usa el normal con
`INICIO_IZQUIERDO`. El poste derivado reforzado central cuenta como una sola posicion y conserva un unico bloque
normal sobre su perfil principal. A diferencia del armado principal por troqueles, esos mates superiores tocan la
linea del **origen del riel**. La ranura vertical no se ajusta a un troquel discreto en altura y el preview consume
las colocaciones puras de Application.

El tipo y peralte de ambos largueros se eligen en la celda frente x nivel; cada frente conserva sus configuraciones
y los botones de alcance replican la celda sin duplicar aritmetica. Las opciones compatibles salen de `secciones.csv`
(`3;3.5;4;4.5;5;5.5;6`); Application valida y coloca
`PERALTE` en cada bloque lateral. Cada corte lateral usa el mayor valor entre sus frentes adyacentes activos en cada
nivel; la planta usa la envolvente del frente que esta dibujando. El DTO guarda una lista de celdas nullable por
frente y acepta como fallback tanto las listas anteriores como los valores globales de documentos mas antiguos.

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
  `"lateral"`, `"planta"`. `Section` = indice de fondo/corte segun la vista; en lateral selectivo y dinamico es el
  indice de poste (`-1` = vista no seccionada o payload legacy).
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
    vistas ya presentes. En racks multi-vista (selectivo, dinamico, cabecera) aparece "Actualizar" + un
    "Insertar" por vista; la cama conserva una sola vista.
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
- **Dinamico**: laterales por poste + frontal salida/entrada + planta; **cama**: lateral unica.
- Las vistas lateral/planta **solo se insertan desde `RACKEDITAR`** de una vista frontal/lateral
  existente (los botones se deshabilitan con tooltip si no aplica), para que nunca queden huerfanas.
- Dibujo block-based en AutoCAD para los cuatro tipos, con jig de colocacion.
- BOM con exportacion a CSV y a Excel (.xlsx) (selectivo; CRLF RFC-4180).

## Que falta / queda fuera del alcance actual

- Definicion de los bloques dinamicos en el DWG (deben existir previamente; los faltantes se
  reportan y se omiten al dibujar).
- No hay base de datos y no se necesita para el alcance local actual: disenos/configuracion viven en JSON y DWG. Una
  migracion futura a SQLite solo tendria sentido si el volumen, consultas o gobierno de catalogos lo justifican.
